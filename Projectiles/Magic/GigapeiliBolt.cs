using Microsoft.Xna.Framework.Graphics;
using Redemption.Dusts;
using Redemption.Effects.PrimitiveTrails;
using Redemption.Globals;
using Terraria;
using Terraria.ModLoader;

namespace Redemption.Projectiles.Magic
{
    public class GigapeiliBolt : ModProjectile, ITrailProjectile
    {
        public override string Texture => Redemption.EMPTY_TEXTURE;
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Energy Bolt");
            ElementID.ProjThunder[Type] = true;
            ElementID.ProjFire[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Magic;
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 60;
            Projectile.extraUpdates = 1;
            Projectile.penetrate = 1;
        }
        public new void DoTrailCreation(TrailManager tManager)
        {
            tManager.CreateTrail(Projectile, new GradientTrail(new Color(255, 236, 100, 100), new Color(0, 0, 0, 0)), new RoundCap(), new DefaultTrailPosition(), 20f, 100f);
            tManager.CreateTrail(Projectile, new GradientTrail(new Color(255, 29, 29, 0), new Color(106, 16, 16, 0)), new RoundCap(), new DefaultTrailPosition(), 10f, 200f);
        }
        NPC target;
        bool targetted;
        public override bool PreAI()
        {
            float spread = .04f;
            if (Projectile.ai[0] is 1)
            {
                spread = .02f;
                Projectile.tileCollide = false;
            }
            Projectile.velocity = Projectile.velocity.RotatedBy(Main.rand.NextFloat(-spread, spread));
            flareScale += Main.rand.NextFloat(-.02f, .02f);
            flareScale = MathHelper.Clamp(flareScale, .2f, .3f);
            flareOpacity += Main.rand.NextFloat(-.2f, .2f);
            flareOpacity = MathHelper.Clamp(flareOpacity, 0.6f, 1.1f);
            return false;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 3; i++)
            {
                int dust = Dust.NewDust(Projectile.Center - (Projectile.timeLeft > 0 ? Vector2.Zero : Projectile.velocity) - Vector2.One, 1, 1, DustType<GlowDust>(), 0, 0, 0, default, .5f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0;
                Color dustColor = new(255, 176, 70) { A = 0 };
                Main.dust[dust].color = dustColor;
            }
        }
        public override void AI()
        {
            Projectile.velocity *= 1.02f;
            flareScale += Main.rand.NextFloat(-.02f, .02f);
            flareScale = MathHelper.Clamp(flareScale, .9f, 1.1f);
            flareOpacity += Main.rand.NextFloat(-.2f, .2f);
            flareOpacity = MathHelper.Clamp(flareOpacity, 0.6f, 1.1f);
        }
        public float flareScale;
        public float flareOpacity;
        public override void PostDraw(Color lightColor)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.BeginAdditive();

            Texture2D flare = Request<Texture2D>("Redemption/Textures/RedEyeFlare").Value;
            Rectangle rect = new(0, 0, flare.Width, flare.Height);
            Vector2 origin = new(flare.Width / 2, flare.Height / 2);
            Vector2 position = Projectile.Center - Main.screenPosition;

            Main.EntitySpriteDraw(flare, position, new Rectangle?(rect), Color.White * flareOpacity, Projectile.rotation, origin, 1f * flareScale, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(flare, position, new Rectangle?(rect), Color.White * flareOpacity * 0.4f, Projectile.rotation, origin, 1.4f * flareScale, SpriteEffects.None, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.BeginDefault();
        }
    }
}