using Microsoft.Xna.Framework.Graphics;
using Redemption.Globals;
using Redemption.Projectiles.Ranged;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;

namespace Redemption.Items.Weapons.PreHM.Ranged
{
    public class EaglecrestSling_Throw : TrueMeleeProjectile
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Eaglecrest Sling");
            Main.projFrames[Projectile.type] = 6;
        }

        public override bool ShouldUpdatePosition() => false;

        public override void SetSafeDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 46;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 4;
        }
        public override bool? CanHitNPC(NPC target) => false ? null : false;

        public ref float Timer => ref Projectile.localAI[0];
        public Vector2 startVector;
        public Vector2 positionVector;
        public Vector2 launchDirection;
        public int directionLock = 0;
        public float Rot;
        public float initialLength;
        public float initialRot;
        public float acc;
        public bool launch;
        public bool parallel;
        public bool rhythm;
        public bool success;
        public float strLength;
        public int swingDir;
        public int bonus;
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (player.noItems || player.CCed || player.dead || !player.active)
                Projectile.Kill();

            if (player.HeldItem.ModItem is EaglecrestSling sling)
            {
                acc = sling.shot * 0.1f + 1;
                bonus = sling.shot + 1;
            }
            Projectile.spriteDirection = player.direction;

            Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter);
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;

            if (Projectile.ai[0] == 0)
            {
                if (Timer++ == 0)
                {
                    directionLock = player.direction;
                    startVector = Vector2.UnitX * directionLock;
                }

                if (LaunchAngleCheck())
                    parallel = true;
                else
                    parallel = false;

                if (!success)
                {
                    float angle = (Timer * 2 * acc) % 360;
                    if (angle >= 180& angle <= 270)
                    {
                        if (!rhythm)
                        {
                            SoundEngine.PlaySound(SoundID.Item19 with { Volume = 2f, Pitch = -.4f }, player.position);
                            SoundEngine.PlaySound(SoundID.Item4 with { Pitch = 1.2f, Volume = 0.4f }, player.position);
                            RedeDraw.SpawnExplosion(playerCenter + positionVector, Color.Yellow, scale: 1, noDust: true, shakeAmount: 0, tex: "Redemption/Textures/WhiteFlare");
                            RedeDraw.SpawnExplosion(playerCenter + positionVector, Color.White, scale: .8f, noDust: true, shakeAmount: 0, tex: "Redemption/Textures/WhiteFlare");
                            DustHelper.DrawCircle(playerCenter + positionVector, DustID.SandSpray, 3, 1, 1, 1, 1, nogravity: true);
                            rhythm = true;
                        }
                    }
                    else
                        rhythm = false;
                }

                if (!player.channel)
                {
                    if (rhythm || success)
                    {
                        success = true;
                        if (parallel)
                        {
                            Projectile.ai[0] = 1;
                            SoundEngine.PlaySound(SoundID.Item1, Projectile.position);
                            if (Projectile.owner == Main.myPlayer)
                                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Projectile.Center.DirectionTo(Main.MouseWorld) * player.HeldItem.shootSpeed, ProjectileType<EaglecrestSling_Proj>(), Projectile.damage * bonus, Projectile.knockBack, Projectile.owner);
                        }
                    }
                    else
                    {
                        Projectile.ai[0] = 1;
                        SoundEngine.PlaySound(SoundID.Item1, Projectile.position);
                        if (Projectile.owner == Main.myPlayer)
                            Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Projectile.velocity * player.HeldItem.shootSpeed, ProjectileType<EaglecrestSling_Proj>(), Projectile.damage * bonus, Projectile.knockBack, Projectile.owner);
                    }
                }
            }
            else
            {
                Timer++;

                if (++Projectile.frameCounter >= ((6 - bonus / 2) * Projectile.MaxUpdates))
                {
                    Projectile.frameCounter = 0;
                    Projectile.frame++;
                    if (Projectile.frame > 5)
                        Projectile.Kill();
                }
            }
            player.direction = directionLock;
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, (playerCenter - Projectile.Center).ToRotation() + MathHelper.PiOver2);
            Projectile.rotation = (playerCenter - Projectile.Center).ToRotation() - MathHelper.PiOver2;
            Projectile.velocity = RedeHelper.PolarVector(player.direction, Projectile.rotation);
            Projectile.Center = playerCenter + positionVector;
            Rot = MathHelper.ToRadians(Timer * 2 * acc) * directionLock;
            positionVector = startVector.RotatedBy(Rot) * 40;
        }
        private bool LaunchAngleCheck()
        {
            if (Projectile.owner == Main.myPlayer)
            {
                Vector2 cursor = Main.MouseWorld - Projectile.Center;
                Vector2 launchDir = Projectile.velocity;
                float num = cursor.ToRotation() - launchDir.ToRotation();
                if (Math.Abs(num) <= MathHelper.ToRadians(30))
                    return true;
            }
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteEffects spriteEffects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Texture2D glow = Request<Texture2D>(Texture + "_Glow").Value;
            int height = texture.Height / 6;
            int y = height * Projectile.frame;
            Rectangle rect = new(0, y, texture.Width, height);
            Vector2 drawOrigin = new(texture.Width / 2, Projectile.height / 2);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition,
                new Rectangle?(rect), Projectile.GetAlpha(lightColor), Projectile.rotation, drawOrigin, Projectile.scale, spriteEffects, 0);
            Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition,
                new Rectangle?(rect), Projectile.GetAlpha(Color.White), Projectile.rotation, drawOrigin, Projectile.scale, spriteEffects, 0);
            return false;
        }
    }
}
