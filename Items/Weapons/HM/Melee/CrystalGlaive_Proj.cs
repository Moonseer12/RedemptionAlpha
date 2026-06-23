using Microsoft.Xna.Framework.Graphics;
using Redemption.BaseExtension;
using Redemption.Dusts;
using Redemption.Effects.PrimitiveTrails;
using Redemption.Globals;
using Redemption.Helpers;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;

namespace Redemption.Items.Weapons.HM.Melee
{
    public class CrystalGlaive_Proj : TrueMeleeProjectile, ITrailProjectile
    {
        private Vector2 startVector;
        private Vector2 vector;
        public ref float Length => ref Projectile.localAI[0];
        public ref float Rot => ref Projectile.localAI[1];
        public ref float Timer => ref Projectile.localAI[2];
        public int pauseTimer;
        public float progress;

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Crystal Glaive");
            ElementID.ProjHoly[Type] = true;
        }
        public override void SetSafeDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.alpha = 255;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.usesOwnerMeleeHitCD = true;
            Projectile.ownerHitCheck = true;
            Projectile.ownerHitCheckDistance = 300f;
            Projectile.extraUpdates = 4;

            Projectile.Redemption().TechnicallyMelee = true;
            Rot = MathHelper.ToRadians(3);
            Length = 60;
        }
        public void DoTrailCreation(TrailManager tManager)
        {
            if (Projectile.ai[0] != 3)
                tManager.CreateTrail(Projectile, new RainbowTrail(saturation: 0.4f), new RoundCap(), new DefaultTrailPosition(), 100f, 250f, new ImageShader(Request<Texture2D>("Redemption/Textures/Trails/GlowTrail").Value, 0.01f, 1f, 1f));
        }
        public int maxTime;
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            player.itemTime = 2;
            player.itemAnimation = 2;

            Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter);
            maxTime = SetUseTime(player.HeldItem.useTime);
            progress = Timer / (maxTime * Projectile.MaxUpdates);
            Projectile.spriteDirection = player.direction;

            if (Main.myPlayer == Projectile.owner && --pauseTimer <= 0)
            {
                switch (Projectile.ai[0])
                {
                    case 0:
                        if (Timer++ == 0)
                        {
                            Projectile.scale *= Projectile.ai[2];
                            startVector = RedeHelper.PolarVector(1, Projectile.velocity.ToRotation() - (MathHelper.PiOver2 * Projectile.spriteDirection));
                        }
                        if (progress < 1f)
                        {
                            Length = 70 + 50 * MathF.Sin(MathHelper.Pi * progress);
                            Rot = MathHelper.ToRadians(80 - 75f * MathF.Cos(MathHelper.Pi * progress)) * Projectile.spriteDirection;
                            vector = startVector.RotatedBy(Rot) * Length * Projectile.scale;
                        }
                        else
                            Projectile.Kill();
                        break;
                    case 1:
                        if (Timer++ == 0)
                        {
                            Projectile.scale *= Projectile.ai[2];
                            startVector = RedeHelper.PolarVector(1, Projectile.velocity.ToRotation() + (MathHelper.PiOver2 * Projectile.spriteDirection));
                        }
                        if (progress < 1f)
                        {
                            Length = 60 + 60 * MathF.Sin(MathHelper.Pi * progress);
                            Rot = -MathHelper.ToRadians(80 - 75f * MathF.Cos(MathHelper.Pi * progress)) * Projectile.spriteDirection;
                            vector = startVector.RotatedBy(Rot) * Length * Projectile.scale;
                        }
                        else
                            Projectile.Kill();
                        break;
                    case 2:
                        Projectile.stopsDealingDamageAfterPenetrateHits = false;
                        Projectile.usesOwnerMeleeHitCD = false;
                        Projectile.localNPCHitCooldown = (int)(maxTime);
                        if (Timer++ == 0)
                        {
                            Projectile.scale *= Projectile.ai[2];
                            startVector = RedeHelper.PolarVector(1, Projectile.velocity.ToRotation());
                        }
                        if (progress < 1f)
                        {
                            Length = 150 * MathF.Exp(-MathHelper.Pi * 0.1f * progress) * MathF.Cos(MathHelper.Pi * (progress - 0.5f));
                            Length = MathHelper.Clamp(Length, 0f, 125f);
                            vector = startVector * Length * Projectile.scale;
                        }
                        else
                            Projectile.Kill();
                        break;
                    case 3:
                        Projectile.stopsDealingDamageAfterPenetrateHits = false;
                        Projectile.usesOwnerMeleeHitCD = false;
                        Projectile.localNPCHitCooldown = (int)(maxTime);

                        if (Timer++ == 0)
                        {
                            Projectile.scale *= Projectile.ai[2];
                            startVector = RedeHelper.PolarVector(1, Projectile.velocity.ToRotation());
                        }
                        if (Timer == (int)(maxTime / 4 * Projectile.MaxUpdates) && Projectile.owner == Main.myPlayer)
                        {
                            SoundEngine.PlaySound(SoundID.Item101, Projectile.position);
                            if (Projectile.ai[1] == 1)
                            {
                                for (int i = 0; i < Main.rand.Next(5, 8); i++)
                                {
                                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, RedeHelper.PolarVector(Main.rand.Next(7, 11), Projectile.velocity.ToRotation() + Main.rand.NextFloat(-0.1f, 0.1f)), ProjectileID.CrystalStorm, Projectile.damage / 3, Projectile.knockBack, Projectile.owner);
                                }
                            }
                            else
                            {
                                for (int i = 0; i < Main.rand.Next(1, 3); i++)
                                {
                                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, RedeHelper.PolarVector(Main.rand.Next(7, 11), Projectile.velocity.ToRotation() + Main.rand.NextFloat(-0.1f, 0.1f)), ProjectileID.CrystalStorm, Projectile.damage / 3, Projectile.knockBack, Projectile.owner);
                                }
                            }
                        }
                        if (progress < 1f)
                        {
                            Length = 150 * MathF.Exp(-MathHelper.Pi * 0.1f * progress) * MathF.Cos(MathHelper.Pi * (progress - 0.5f));
                            Length = MathHelper.Clamp(Length, 0f, 125f);
                            vector = startVector * Length * Projectile.scale;
                        }
                        else
                            Projectile.Kill();
                        break;
                }
            }
            Projectile.Center = playerCenter + vector;

            if (Projectile.spriteDirection == 1)
                Projectile.rotation = (Projectile.Center - playerCenter).ToRotation() + MathHelper.PiOver4;
            else
                Projectile.rotation = (Projectile.Center - playerCenter).ToRotation() - MathHelper.Pi - MathHelper.PiOver4;

            player.SetCompositeArmFront(true, Length >= 100 ? Player.CompositeArmStretchAmount.Full : Player.CompositeArmStretchAmount.Quarter, (playerCenter - Projectile.Center).ToRotation() + MathHelper.PiOver2);
           
            if (Timer > 2)
                Projectile.alpha = 0;

            if (Timer % 5 == 0)
            {
                Dust dust = Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.AncientLight)];
                dust.velocity *= 0;
                dust.noGravity = true;
                dust.shader = GameShaders.Armor.GetSecondaryShader(77, Main.player[Projectile.owner]);
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Player player = Main.player[Projectile.owner];
            if (Helper.CheckLinearCollision(player.Center, Projectile.Center, targetHitbox, out Vector2 _))
                return true;
            return null;
        }
        private bool strike;
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];
            SoundEngine.PlaySound(CustomSounds.NimbleThrust with { Volume = .7f, Pitch = .5f }, Projectile.position);
            if (Projectile.ai[0] == 0 || Projectile.ai[0] == 1)
            {
                if (!strike)
                {
                    strike = true;
                    pauseTimer = (int)(maxTime / 10 * Projectile.MaxUpdates);
                    player.RedemptionScreen().ScreenShakeIntensity += maxTime / 10f;
                }
            }
            else
            {
                if (!strike)
                {
                    strike = true;
                    pauseTimer = (int)(maxTime / 20 * Projectile.MaxUpdates);
                    player.RedemptionScreen().ScreenShakeIntensity += maxTime / 30f;
                }
            }
            Vector2 directionTo = target.DirectionFrom(player.Center);
            for (int i = 0; i < 4; i++)
                Dust.NewDustPerfect(target.Center + directionTo * 5 + new Vector2(0, 50) + player.velocity, DustType<DustSpark2>(), directionTo.RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * Main.rand.NextFloat(6f, 10f) + (player.velocity / 2), 0, Main.DiscoColor * .8f, 2f);

            if (Projectile.ai[0] == 2 && player.Redemption().crystalGlaiveShotCount <= 0)
            {
                if (player.Redemption().crystalGlaiveShotCount <= 0)
                {
                    SoundEngine.PlaySound(SoundID.DD2_DarkMageHealImpact, player.position);
                    DustHelper.DrawCircle(player.Center, DustID.CrystalPulse, 4, 1, 1, 1, 2, nogravity: true);
                }
                player.Redemption().crystalGlaiveLevel = 0;
                player.Redemption().crystalGlaiveShotCount = 5;
            }
            else if (Projectile.ai[0] < 2)
            {
                player.Redemption().crystalGlaiveLevel = (int)Projectile.ai[0] + 1;
            }

        }
        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Rectangle rect = new(0, 0, texture.Width, texture.Height);
            Vector2 drawOrigin = new(texture.Width / 2, texture.Height / 2);
            var effects = Projectile.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter, true);
            Vector2 v = Projectile.Center - RedeHelper.PolarVector(64, (Projectile.Center - playerCenter).ToRotation());
            if (Projectile.ai[1] == 1)
            {
                int shader = ContentSamples.CommonlyUsedContentSamples.ColorOnlyShaderIndex;
                Main.spriteBatch.End();
                Main.spriteBatch.BeginAdditive(true);
                GameShaders.Armor.ApplySecondary(shader, Main.LocalPlayer, null);

                Main.EntitySpriteDraw(texture, v - Main.screenPosition, new Rectangle?(rect), Projectile.GetAlpha(Color.Pink) * 0.5f, Projectile.rotation, drawOrigin, Projectile.scale + 0.2f, effects, 0);

                Main.spriteBatch.End();
                Main.spriteBatch.BeginDefault();
            }

            Main.EntitySpriteDraw(texture, v - Main.screenPosition, new Rectangle?(rect), Projectile.GetAlpha(lightColor), Projectile.rotation, drawOrigin, Projectile.scale, effects, 0);
            return false;
        }

    }
}
