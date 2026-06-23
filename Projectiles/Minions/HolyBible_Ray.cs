using Microsoft.Xna.Framework.Graphics;
using Redemption.BaseExtension;
using Redemption.Globals;
using Redemption.NPCs.Bosses.Erhan;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Projectiles.Minions
{
    public class HolyBible_Ray : LaserProjectile
    {
        public override string Texture => "Redemption/Projectiles/Magic/SunshardRay";
        private new const float FirstSegmentDrawDist = 7;
        public override void SetSafeStaticDefaults()
        {
            ElementID.ProjHoly[Type] = true;
            ElementID.ProjArcane[Type] = true;
        }
        public override void SetSafeDefaults()
        {
            Projectile.DamageType = DamageClass.Summon;
            Projectile.Redemption().FromMinion = true;
            Projectile.hostile = false;
            Projectile.friendly = true;

            Projectile.tileCollide = false;
            Projectile.timeLeft = 180;
            Projectile.penetrate = -1;

            LaserSegmentLength = 16;
            LaserWidth = 20;
            LaserEndSegmentLength = 14;
            MaxLaserLength = 112;
            NewCollision = true;
            StopsOnTiles = false;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (target.type is NPCID.EaterofWorldsBody or NPCID.EaterofWorldsHead or NPCID.EaterofWorldsTail or NPCID.Creeper)
                modifiers.FinalDamage /= 2;
        }
        public override void AI()
        {
            Projectile proj = Main.projectile[(int)Projectile.ai[0]];
            Projectile.rotation = Projectile.velocity.ToRotation();

            if (proj.type == ProjectileType<Erhan_Bible>())
            {
                MaxLaserLength = 77;
                Projectile.hostile = true;
                Projectile.friendly = true;
                Projectile.Redemption().friendlyHostile = true;
            }
            #region Beginning And End Effects
            if (AITimer == 0)
                LaserScale = 0.1f;
            else
                Projectile.Center = proj.Center - Vector2.Normalize(Projectile.velocity) * 10f;

            Projectile.velocity = Projectile.velocity.RotatedBy(-0.08f * proj.spriteDirection);

            if (AITimer <= 10)
                LaserScale += 0.09f;
            else if (Projectile.timeLeft < 10 || !proj.active)
            {
                if (Projectile.timeLeft > 10)
                {
                    Projectile.timeLeft = 10;
                }
                LaserScale -= 0.1f;
            }
            #endregion

            endPoint = Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation) * (MaxLaserLength + 10);
            LaserLength = LengthSetting(Projectile, endPoint);

            ++AITimer;
        }
        #region Drawcode
        // The core function of drawing a Laser, you shouldn't need to touch this
        public void DrawLaser(Texture2D texture, Vector2 start, Vector2 unit, float rotation = 0f, float scale = 1f, float maxDist = 2000f, Color color = default, int transDist = 1)
        {
            float r = unit.ToRotation() + rotation;
            // Draws the Laser 'body'
            for (float i = transDist; i <= (maxDist * (1 / LaserScale)); i += LaserSegmentLength)
            {
                //Color c = Color.White;
                var origin = start + i * unit;
                Main.EntitySpriteDraw(texture, origin - Main.screenPosition + new Vector2(0, Projectile.gfxOffY),
                    new Rectangle((int)(LaserWidth * Frame), LaserEndSegmentLength, LaserWidth, LaserSegmentLength), color, r,
                    new Vector2(LaserWidth / 2, LaserSegmentLength / 2), scale, 0, 0);
            }
            // Draws the Laser 'base'
            Main.EntitySpriteDraw(texture, start + unit * (transDist - LaserEndSegmentLength) - Main.screenPosition + new Vector2(0, Projectile.gfxOffY),
                new Rectangle((int)(LaserWidth * Frame), 0, LaserWidth, LaserEndSegmentLength), color, r, new Vector2(LaserWidth / 2, LaserSegmentLength / 2), scale, 0, 0);
            // Draws the Laser 'end'
            Main.EntitySpriteDraw(texture, start + maxDist * (1 / scale) * unit - Main.screenPosition + new Vector2(0, Projectile.gfxOffY),
                new Rectangle((int)(LaserWidth * Frame), LaserSegmentLength + LaserEndSegmentLength, LaserWidth, LaserEndSegmentLength), color, r, new Vector2(LaserWidth / 2, LaserSegmentLength / 2), scale, 0, 0);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.BeginAdditive();

            DrawLaser(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center + (new Vector2(Projectile.width, 0).RotatedBy(Projectile.rotation) * LaserScale), new Vector2(1f, 0).RotatedBy(Projectile.rotation) * LaserScale, -1.57f, LaserScale, LaserLength, Color.White, (int)FirstSegmentDrawDist);

            Main.spriteBatch.End();
            Main.spriteBatch.BeginDefault();
            return false;
        }
        #endregion
    }
}
