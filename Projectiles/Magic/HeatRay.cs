using Microsoft.Xna.Framework.Graphics;
using Redemption.BaseExtension;
using Redemption.Buffs.NPCBuffs;
using Redemption.Dusts;
using Redemption.Globals;
using Redemption.Helpers;
using Redemption.Particles;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Projectiles.Magic
{
    public class HeatRay : LaserProjectile
    {
        public override void SetSafeStaticDefaults()
        {
            // DisplayName.SetDefault("Heat Ray");
            ElementID.ProjFire[Type] = true;
            ElementID.ProjArcane[Type] = true;
        }

        public override void SetSafeDefaults()
        {
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.timeLeft = 180;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;

            NewCollision = true;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (NPCLists.Dragonlike.Contains(target.type))
                modifiers.FinalDamage *= 4;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];

            if (Main.rand.NextBool(3))
                target.AddBuff(BuffID.OnFire3, 60);

            if (player.RedemptionPlayerBuff().dragonLeadBonus)
                target.AddBuff(BuffType<DragonblazeDebuff>(), 300);
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Projectile host = Main.projectile[(int)Projectile.ai[0]];
            Projectile.rotation = host.rotation + (host.spriteDirection == -1 ? (float)Math.PI : 0);
            Projectile.velocity = RedeHelper.PolarVector(1f, Projectile.rotation);

            RedeParticleManager.CreateEmberParticle(Projectile.position + new Vector2(Main.rand.Next(0, Projectile.width), Main.rand.Next(0, Projectile.height)), RedeHelper.PolarVector(5, Projectile.rotation), 1, Main.rand.Next(90, 121));
            RedeParticleManager.CreateAdditiveGlowParticle(Projectile.Center + RedeHelper.PolarVector(24, Projectile.rotation), Vector2.Zero, Vector2.One , new Color(255, 151, 101) * 0.5f, 12);

            for (int i = 0; i < 2; i++)
            {
                int num5 = Dust.NewDust(Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation) * (LaserLength + 10) - new Vector2(4, 4), 8, 8, DustType<GlowDust>());
                Color dustColor = new(255, 151, 101) { A = 0 };
                if (Main.rand.NextBool())
                    dustColor = new(249, 71, 3) { A = 0 };
                Main.dust[num5].velocity = -Projectile.velocity * Main.rand.NextFloat(.1f, .3f);
                Main.dust[num5].color = dustColor * Projectile.Opacity;
                Main.dust[num5].noGravity = true;
            }

            #region Beginning And End Effects
            if (AITimer == 0)
                LaserScale = 0.1f;
            else
                Projectile.Center = host.Center + RedeHelper.PolarVector(6, host.rotation + MathHelper.PiOver2);

            if (AITimer <= 10)
            {
                LaserScale += 0.18f;
            }
            else if (!player.channel || Projectile.timeLeft < 10 || !player.active)
            {
                if (Projectile.timeLeft > 10)
                {
                    Projectile.timeLeft = 10;
                }
                LaserScale -= 0.1f;
            }
            #endregion

            #region length
            // code from slr
            for (int k = 0; k < MaxLaserLength; k++)
            {
                Vector2 posCheck = Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation) * k * 8;

                if (Helper.PointInTile(posCheck) || k == MaxLaserLength - 1)
                {
                    endPoint = posCheck;
                    break;
                }
            }

            LaserLength = LengthSetting(host, endPoint);
            #endregion

            ++AITimer;
            if (Main.myPlayer != player.whoAmI)
                CheckHits();
        }
        #region Drawcode
        // The core function of drawing a Laser, you shouldn't need to touch this
        public void DrawLaser(Texture2D texture, Vector2 start, Vector2 unit, float rotation = 0f, float scale = 1f, float maxDist = 2000f, Color color = default, int transDist = 1)
        {
            float r = unit.ToRotation() + rotation;
            // Draws the Laser 'body'
            for (float i = transDist; i <= (maxDist * (1 / LaserScale)); i += LaserSegmentLength)
            {
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

            DrawLaser(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center + (new Vector2(Projectile.width, 0).RotatedBy(Projectile.rotation) * LaserScale), new Vector2(1f, 0).RotatedBy(Projectile.rotation) * LaserScale, -1.57f, LaserScale, LaserLength - LaserSegmentLength * 2, Color.White, (int)FirstSegmentDrawDist);

            Main.spriteBatch.End();
            Main.spriteBatch.BeginDefault();
            return false;
        }
        #endregion
    }
}
