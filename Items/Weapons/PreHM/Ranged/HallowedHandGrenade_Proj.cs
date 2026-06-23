using Microsoft.Xna.Framework.Graphics;
using Redemption.Base;
using Redemption.BaseExtension;
using Redemption.Globals;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Items.Weapons.PreHM.Ranged
{
    public class HallowedHandGrenade_Proj : ModProjectile
    {
        public override string Texture => "Redemption/Items/Weapons/PreHM/Ranged/HallowedHandGrenade";
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Hallowed Hand Grenade of Anglon");
            ElementID.ProjArcane[Type] = true;
            ElementID.ProjHoly[Type] = true;
            ElementID.ProjExplosive[Type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 22;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 190;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Projectile.LookByVelocity();
            Projectile.rotation += Projectile.velocity.X / 20;
            if (Projectile.localAI[0] < 180)
                Projectile.velocity.Y += 0.2f;

            if (Projectile.localAI[0]++ == 180)
            {
                Projectile.velocity *= 0;
                Projectile.alpha = 255;
                Main.LocalPlayer.RedemptionScreen().ScreenShakeOrigin = Projectile.Center;
                Main.LocalPlayer.RedemptionScreen().ScreenShakeIntensity += 10;
                SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
                RedeDraw.SpawnExplosion(Projectile.Center, new Color(255, 216, 0), DustID.GoldFlame, 0, 30, 3);
                Rectangle boom = new((int)Projectile.Center.X - 150, (int)Projectile.Center.Y - 150, 300, 300);
                Rectangle boom2 = new((int)Projectile.Center.X - 80, (int)Projectile.Center.Y - 80, 160, 160);
                if (player.active && !player.dead && player.Hitbox.Intersects(boom2))
                {
                    int hitDirection = player.RightOfDir(Projectile);
                    BaseAI.DamagePlayer(player, Projectile.damage / 4, 4.5f, hitDirection, Projectile);
                }
                RedeHelper.NPCRadiusDamage(boom, Projectile, Projectile.damage, Projectile.knockBack, Projectile.CritChance);
            }
            if (Projectile.localAI[0] == 182)
                Projectile.friendly = false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.immune[Projectile.owner] = 20;

            if (Projectile.localAI[0] < 180)
                Projectile.localAI[0] = 180;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (target.type is NPCID.EaterofWorldsBody or NPCID.EaterofWorldsHead or NPCID.EaterofWorldsTail or NPCID.Creeper)
                modifiers.FinalDamage /= 2;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new(texture.Width / 2, texture.Height / 2 + 6);

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.velocity.X != oldVelocity.X)
                Projectile.velocity.X = -oldVelocity.X;
            if (Projectile.velocity.Y != oldVelocity.Y)
                Projectile.velocity.Y = -oldVelocity.Y;
            Projectile.velocity.Y *= 0.5f;
            Projectile.velocity.X *= 0.8f;
            return false;
        }
    }
}
