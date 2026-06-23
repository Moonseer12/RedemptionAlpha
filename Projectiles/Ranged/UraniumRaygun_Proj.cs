using Microsoft.Xna.Framework.Graphics;
using Redemption.Base;
using Redemption.BaseExtension;
using Redemption.Effects;
using Redemption.Effects.Trails;
using Redemption.Effects.Trails.Tips;
using Redemption.Globals;
using Redemption.Globals.Projectiles;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Projectiles.Ranged
{
    public class UraniumRaygun_Proj : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 56;
            Projectile.height = 56;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.hostile = false;
            Projectile.penetrate = 4;
            Projectile.timeLeft = 80;
            Projectile.alpha = 50;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
            Projectile.Redemption().EnergyBased = true;

            InitializeTrail();
        }
        public bool offsetLeft = false;
        public Vector2 originalVelocity = Vector2.Zero;
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + 1.57f;
            if (originalVelocity == Vector2.Zero)
                originalVelocity = Projectile.velocity;
            if (offsetLeft)
            {
                Projectile.scale -= 0.04f;
                if (Projectile.scale <= 0.7f)
                {
                    Projectile.scale = 0.7f;
                    offsetLeft = false;
                }
            }
            else
            {
                Projectile.scale += 0.04f;
                if (Projectile.scale >= 1.3f)
                {
                    Projectile.scale = 1.3f;
                    offsetLeft = true;
                }
            }
            if (Projectile.timeLeft <= 30)
                Projectile.alpha = (int)MathHelper.Lerp(255f, 0f, Projectile.timeLeft / 30f);

            if (Main.netMode != NetmodeID.Server)
            {
                ManageCaches();
                ManageTrail();
            }
        }

        private readonly int numPoint = 3;
        private List<Vector2> cache;
        private List<Vector2> cache2;
        private DanTrail trail;
        private DanTrail trail2;
        public float SinProgress => (float)Math.Sin((80 - Projectile.timeLeft) / 32 * 3.14f);
        private void ManageCaches()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();

                for (int i = 0; i < numPoint; i++)
                {
                    cache.Add(Projectile.Center + Projectile.velocity.SafeNormalize(default).RotatedBy(MathHelper.PiOver4 * 0.4f * Projectile.scale) * (43 + 2 * Projectile.scale * Projectile.scale));
                }
            }

            cache.Add(Projectile.Center + Projectile.velocity.SafeNormalize(default).RotatedBy(MathHelper.PiOver4 * 0.4f * Projectile.scale) * (43 + 2 * Projectile.scale * Projectile.scale));

            while (cache.Count > numPoint)
            {
                cache.RemoveAt(0);
            }

            if (cache2 == null)
            {
                cache2 = new List<Vector2>();

                for (int i = 0; i < numPoint; i++)
                {
                    cache2.Add(Projectile.Center + Projectile.velocity.SafeNormalize(default).RotatedBy(-MathHelper.PiOver4 * 0.4f * Projectile.scale) * (43 + 2 * Projectile.scale * Projectile.scale));
                }
            }

            cache2.Add(Projectile.Center + Projectile.velocity.SafeNormalize(default).RotatedBy(-MathHelper.PiOver4 * 0.4f * Projectile.scale) * (43 + 2 * Projectile.scale * Projectile.scale));

            while (cache2.Count > numPoint)
            {
                cache2.RemoveAt(0);
            }

        }

        public void InitializeTrail()
        {
            trail = new DanTrail(RedeGraphics.Instance.Primitives, new NoTip(),
            factor =>
            {
                float width = 60 * Projectile.Opacity;
                if (width <= 5)
                    return 0;
                else
                    return width;
            },
            factor =>
            {
                if (factor.X >= 0.99f)
                    return Color.White * 0;

                return new Color(80, 200 + (int)(factor.X * 50), 97) * (factor.X) * Projectile.Opacity;
            });
            trail2 = new DanTrail(RedeGraphics.Instance.Primitives, new NoTip(),
            factor =>
            {
                float width = 60 * Projectile.Opacity;
                if (width <= 5)
                    return 0;
                else
                    return width;
            },
            factor =>
            {
                if (factor.X >= 0.99f)
                    return Color.White * 0;

                return new Color(80, 200 + (int)(factor.X * 50), 97) * (factor.X) * Projectile.Opacity;
            });
        }

        private void ManageTrail()
        {
            trail.SetPositions(cache.ToArray(), Projectile.Center);
            trail2.SetPositions(cache2.ToArray(), Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            var effects = Projectile.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Vector2 drawOrigin = new(texture.Width / 2, Projectile.height / 2);

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(Color.White), Projectile.rotation, drawOrigin, Projectile.scale, effects, 0);

            Effect effect = Terraria.Graphics.Effects.Filters.Scene["MoR:GlowTrailShader"]?.GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(Request<Texture2D>("Redemption/Textures/Trails/GlowTrail").Value);
            effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.05f);
            effect.Parameters["repeats"].SetValue(1f);

            trail?.Render(effect);
            trail2?.Render(effect);
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int m = 0; m < 16; m++)
            {
                int dustID = Dust.NewDust(new Vector2(Projectile.Center.X - 1, Projectile.Center.Y - 1), 2, 2, DustID.GreenFairy, 0f, 0f, 100, Color.White, 1.6f);
                Main.dust[dustID].velocity = BaseUtility.RotateVector(default, new Vector2(4f, 0f), m / (float)16 * 6.28f);
                Main.dust[dustID].noLight = false;
                Main.dust[dustID].noGravity = true;
            }
            target.immune[Projectile.owner] = 5;
        }
    }
}