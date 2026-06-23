using Redemption.BaseExtension;
using Terraria;
using Terraria.ModLoader;

namespace Redemption.Projectiles.Misc
{
    public class ProjDeath : ModProjectile
    {
        public override string Texture => Redemption.EMPTY_TEXTURE;

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("death");
        }
        public override void SetDefaults()
        {
            Projectile.width = 2000;
            Projectile.height = 2000;
            Projectile.penetrate = -1;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.alpha = 255;
            Projectile.timeLeft = 6;
        }
        public override void AI()
        {
            foreach (Projectile target in Main.ActiveProjectiles)
            {
                if (Projectile == target || target.damage <= 0 || target.hostile || target.minion || target.sentry)
                    continue;

                target.Kill();
            }
        }
    }
}