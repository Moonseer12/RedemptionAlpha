using Microsoft.Xna.Framework.Graphics;
using Redemption.Base;
using Redemption.BaseExtension;
using Redemption.Buffs;
using Redemption.Globals;
using Redemption.Textures;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Projectiles.Minions
{
    public class RowanTreeSummon : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Rowan Tree");

            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 110;
            Projectile.height = 140;
            Projectile.tileCollide = false;
            Projectile.sentry = true;
            Projectile.timeLeft = Projectile.SentryLifeTime;

            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.Redemption().auraSentry = true;
        }

        public override bool? CanDamage() => false;
        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 0.5f, 1f, 0.5f);
            Player owner = Main.player[Projectile.owner];

            if (!CheckActive(owner))
                return;

            Projectile.velocity *= 0;
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (!player.active || player.dead || Projectile.DistanceSQ(player.Center) > 460 * 460)
                    continue;

                player.AddBuff(BuffType<RowanAuraBuff>(), 4);
            }
            if (Projectile.localAI[0]++ >= 40)
                Projectile.localAI[0] = 0;
        }
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = false;
            return true;
        }
        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                Projectile.Kill();
                return false;
            }
            return true;
        }
        private float drawTimer;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new(texture.Width / 2, Projectile.height / 2);
            DrawPulse();
            RedeDraw.DrawTreasureBagEffect(Main.spriteBatch, texture, ref drawTimer, Projectile.Center - Main.screenPosition, null, Color.Lime * Projectile.Opacity, Projectile.rotation, drawOrigin, Projectile.scale);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, lightColor * Projectile.Opacity, Projectile.rotation, drawOrigin, Projectile.scale, 0, 0);
            return false;
        }
        public void DrawPulse()
        {
            Texture2D texture = Request<Texture2D>("Redemption/Textures/DreamsongLight_Visual").Value;
            Vector2 drawOrigin = new(texture.Width / 2, texture.Height / 2);
            Color c = Color.Lime * BaseUtility.MultiLerp(Projectile.localAI[0] / 40f, 0, 1, 0);
            float scale = Projectile.scale * BaseUtility.MultiLerp(Projectile.localAI[0] / 40f, 1, 1.25f);

            Main.spriteBatch.End();
            Main.spriteBatch.BeginAdditive();
            Main.EntitySpriteDraw(texture, Projectile.Center - Vector2.UnitY * 0 - Main.screenPosition, null, c, Projectile.rotation, drawOrigin, scale, 0, 0);
            Main.spriteBatch.End();
            Main.spriteBatch.BeginDefault();
        }
    }
}