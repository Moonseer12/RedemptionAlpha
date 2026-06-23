using Microsoft.Xna.Framework.Graphics;
using Redemption.BaseExtension;
using Redemption.Buffs.Debuffs;
using Redemption.Dusts;
using Redemption.Effects.Trails;
using Redemption.Effects.Trails.Tips;
using Redemption.Globals;
using Redemption.Helpers;
using Redemption.Particles;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;

namespace Redemption.Items.Weapons.HM.Melee
{
    public class InfectiousGlaive_Proj : TrueMeleeProjectile
    {
        private float startRotation;
        private Vector2 vector;
        public ref float Timer => ref Projectile.localAI[0];
        public ref float Rot => ref Projectile.localAI[1];
        private Player Player => Main.player[Projectile.owner];
        public float progress;
        public override void SetStaticDefaults()
        {
            //DisplayName.SetDefault("Infectious Glaive");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 60;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
            ElementID.ProjPoison[Type] = true;
        }
        public override void SetSafeDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.alpha = 255;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.ownerHitCheck = true;
            Projectile.ownerHitCheckDistance = 300f;
            Projectile.usesOwnerMeleeHitCD = true;
            Projectile.extraUpdates = 4;

            InitializeTrail();
        }
        public int maxTime;
        public override void AI()
        {
            Player.itemTime = 2;
            Player.itemAnimation = 2;
            Player.heldProj = Projectile.whoAmI;

            maxTime = SetUseTime(Player.HeldItem.useTime);
            Projectile.spriteDirection = Player.direction;
            Vector2 armCenter = Player.RotatedRelativePoint(Player.MountedCenter) + new Vector2(Player.direction * -4, -4);
            switch (Projectile.ai[0])
            {
                case -1:
                    progress = Timer / (maxTime * Projectile.MaxUpdates);
                    if (Timer++ == 0)
                    {
                        Projectile.scale *= Projectile.ai[2];
                        startRotation = Projectile.velocity.ToRotation() + MathHelper.Pi;
                    }
                    if (progress < 1f)
                    {
                        float modifiedProgress = 1 / (1 + MathF.Pow(3, -15 * (progress - 0.5f)));
                        float x = 30 * MathF.Cos(modifiedProgress * MathHelper.TwoPi * Projectile.spriteDirection * 0.9f);
                        float y = 30 * MathF.Sin(modifiedProgress * MathHelper.TwoPi * Projectile.spriteDirection * 0.9f);
                        Vector2 ellipse = new(x, y);
                        vector = ellipse.RotatedBy(startRotation) * 4 * Projectile.scale;
                    }
                    else
                        Projectile.Kill();

                    if (Timer == 30)
                    {
                        if (!Main.dedServ)
                            SoundEngine.PlaySound(CustomSounds.Swing1, Player.position);
                    }
                    if (progress > 0.3f && progress < 0.8f)
                        Projectile.friendly = true;
                    break;
                case 1:
                    progress = Timer / (maxTime * Projectile.MaxUpdates);
                    if (Timer++ == 0)
                    {
                        Projectile.scale *= Projectile.ai[2];
                        startRotation = Projectile.velocity.ToRotation() + MathHelper.Pi;
                    }
                    if (progress < 1f)
                    {
                        float modifiedProgress = 1 / (1 + MathF.Pow(3, -15 * (progress - 0.5f)));
                        float x = 30 * MathF.Cos((1 - modifiedProgress) * MathHelper.TwoPi * Projectile.spriteDirection * 0.9f);
                        float y = 30 * MathF.Sin((1 - modifiedProgress) * MathHelper.TwoPi * Projectile.spriteDirection * 0.9f);
                        Vector2 ellipse = new(x, y);
                        vector = ellipse.RotatedBy(startRotation) * 4 * Projectile.scale;
                    }
                    else
                        Projectile.Kill();

                    if (Timer == 40)
                    {
                        if (!Main.dedServ)
                            SoundEngine.PlaySound(CustomSounds.Swoosh1, Player.position);
                    }
                    if (progress > 0.3f && progress < 0.8f)
                        Projectile.friendly = true;
                    break;
                case 0:
                    progress = Timer / (maxTime * 2 * Projectile.MaxUpdates);
                    Projectile.localNPCHitCooldown = maxTime / 3 * Projectile.MaxUpdates;
                    Projectile.usesOwnerMeleeHitCD = false;
                    if (Timer++ == 0)
                    {
                        Projectile.scale *= Projectile.ai[2];
                        startRotation = Projectile.velocity.ToRotation() + MathHelper.Pi;
                    }
                    if (progress < 0.7f)
                    {
                        float x = 30 * MathF.Cos(5 * progress * MathHelper.TwoPi * Projectile.spriteDirection);
                        float y = 30 * MathF.Sin(5 * progress * MathHelper.TwoPi * Projectile.spriteDirection);
                        Vector2 ellipse = new(x, y);
                        float offset = Projectile.spriteDirection == 1 ? -1 : MathHelper.Pi + 1;
                        vector = ellipse.RotatedBy(offset) * 4 * Projectile.scale;
                    }
                    else if (progress < 0.8f)
                    {
                        Rot = MathHelper.ToRadians(progress / 2) * Projectile.spriteDirection;
                        vector = vector.RotatedBy(Rot);
                    }
                    else if (progress >= 1f)
                        Projectile.Kill();

                    if (progress < 0.8f)
                        Projectile.friendly = true;
                    else
                        Projectile.friendly = false;
                    break;
            }
            Projectile.Center = armCenter + vector;

            if (Projectile.spriteDirection == 1)
                Projectile.rotation = (Projectile.Center - armCenter).ToRotation() + MathHelper.PiOver4;
            else
                Projectile.rotation = (Projectile.Center - armCenter).ToRotation() + 3 * MathHelper.PiOver4;

            Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, (armCenter - Projectile.Center).ToRotation() + MathHelper.PiOver2);

            if (Timer == 2)
            {
                Projectile.alpha = 0;
                for (int i = 0; i < oldDirVector.Length; i++)
                    oldDirVector.SetValue(vector, i);
            }

            for (int k = oldDirVector.Length - 1; k > 0; k--)
            {
                oldDirVector[k] = oldDirVector[k - 1];
            }
            oldDirVector[0] = vector;

            if (Projectile.ai[0] != 0)
            {
                if (progress < 0.2f)
                    opacity = 0;
                else if (progress < 0.78f)
                    opacity = 1;
                else
                    opacity = MathHelper.Lerp(1, 0, progress);
            }
            else
                opacity = 1;

            if (Main.netMode != NetmodeID.Server)
            {
                TrailHelper.ManageSwordTrailPosition(oldDirVector.Length, armCenter, oldDirVector, ref directionVectorCache, ref positionCache, 1f);
                ManageTrail();
            }
        }

        #region draw trail
        private Vector2[] oldDirVector = new Vector2[60];
        private List<Vector2> directionVectorCache = new();
        private List<Vector2> positionCache = new();
        private DanTrail trail;
        private Color baseColor = Color.DarkOliveGreen;
        private Color endColor = Color.DarkGreen;
        private readonly float thickness = 24f;
        private float opacity;

        public void InitializeTrail()
        {
            trail = new DanTrail(RedeGraphics.Instance.Primitives, new NoTip(),
            factor =>
            {
                float mult = factor;
                float delay = 0;
                if (mult < 0.98f)
                    delay = 1;
                return thickness * MathF.Pow(mult, 0.2f) * Projectile.scale * delay;
            },
            factor =>
            {
                float progress = EaseFunction.EaseCubicOut.Ease(1 - factor.X);
                return Color.Lerp(baseColor, endColor, EaseFunction.EaseCubicIn.Ease(progress)) * (1 - progress) * opacity;
            });
        }

        public void ManageTrail()
        {
            trail.SetPositions(positionCache.ToArray(), Projectile.Center);
        }

        public void DrawTrail()
        {
            Main.spriteBatch.End();
            Main.spriteBatch.BeginDefault();

            Effect effect = Request<Effect>("Redemption/Effects/GlowTrailShader").Value;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.X, -Main.screenPosition.Y, 0);
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            Texture2D texture = (Player.direction * Projectile.ai[0] == -1) || (Projectile.ai[0] == 0 && Player.direction == 1) ? Request<Texture2D>("Redemption/Textures/Trails/SlashTrail_5").Value : Request<Texture2D>("Redemption/Textures/Trails/SlashTrail_5_flipped2").Value;

            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(texture);
            effect.Parameters["time"].SetValue(1);
            effect.Parameters["repeats"].SetValue(-1);

            trail?.Render(effect);

            Main.spriteBatch.End();
            Main.spriteBatch.BeginDefault();
        }
        #endregion

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Helper.CheckLinearCollision(Player.Center, Projectile.Center, targetHitbox, out Vector2 _))
                return true;
            return null;
        }

        private bool strike;
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];
            SoundEngine.PlaySound(CustomSounds.Slice4 with { Volume = .7f, Pitch = .5f }, Projectile.position);

            if (Projectile.ai[0] == 0)
            {
                if (!strike)
                {
                    strike = true;
                    player.RedemptionScreen().ScreenShakeIntensity += 1.5f;
                }
            }
            else
            {
                if (!strike)
                {
                    strike = true;
                    player.RedemptionScreen().ScreenShakeIntensity += maxTime / 30 * Projectile.MaxUpdates;
                }
            }

            Vector2 directionTo = target.DirectionFrom(player.Center);
            for (int i = 0; i < 8; i++)
                Dust.NewDustPerfect(target.Center + directionTo * -5 + new Vector2(0, 50) + player.velocity, DustType<DustSpark2>(), directionTo.RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * Main.rand.NextFloat(4f, 5f) + (player.velocity / 2), 0, Color.LimeGreen * .8f, 1.6f);

            Vector2 dir = Projectile.Center.DirectionFrom(target.Center);
            Vector2 drawPos = Vector2.Lerp(Projectile.Center, target.Center, 0.9f);
            RedeParticleManager.CreateSlashParticle(drawPos, dir.RotateRandom(1f) * 90, 1, Color.DarkGreen, 8);

            if (Main.rand.NextBool(3))
                target.AddBuff(BuffType<GreenRashesDebuff>(), 300);
            else if (Main.rand.NextBool(6))
                target.AddBuff(BuffType<GlowingPustulesDebuff>(), 150);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            DrawTrail();
            Player player = Main.player[Projectile.owner];
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Texture2D glow = Request<Texture2D>(Texture + "_Glow").Value;
            Rectangle rect = new(0, 0, texture.Width, texture.Height);
            Vector2 drawOrigin = new(texture.Width / 2, texture.Height / 2);
            var effects = Projectile.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            var effects2 = Projectile.ai[0] == 1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            float offset = Projectile.ai[0] == 1 ? MathHelper.PiOver2 : 0;
            Vector2 armCenter = Player.RotatedRelativePoint(Player.MountedCenter) + new Vector2(Player.direction * -4, -4);
            Vector2 drawPos = armCenter + vector * 0.5f;
            Main.EntitySpriteDraw(texture, drawPos - Main.screenPosition, new Rectangle?(rect), Projectile.GetAlpha(lightColor), Projectile.rotation - offset * Projectile.spriteDirection, drawOrigin, Projectile.scale, effects | effects2, 0);
            Main.EntitySpriteDraw(glow, drawPos - Main.screenPosition, new Rectangle?(rect), Projectile.GetAlpha(Color.White), Projectile.rotation - offset * Projectile.spriteDirection, drawOrigin, Projectile.scale, effects | effects2, 0);
            return false;
        }
    }
}