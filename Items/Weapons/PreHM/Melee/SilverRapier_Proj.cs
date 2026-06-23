using Microsoft.Xna.Framework.Graphics;
using Redemption.BaseExtension;
using Redemption.Globals;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Items.Weapons.PreHM.Melee
{
    public class SilverRapier_Proj : ModProjectile
    {
        public override string Texture => "Redemption/Items/Weapons/PreHM/Melee/SilverRapier";
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Silver Rapier");
        }
        private Vector2 startVector;
        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 20;
            Projectile.height = 20;
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
            Projectile.Redemption().TechnicallyMelee = true;
            Rot = MathHelper.ToRadians(Main.rand.NextFloat(-10, 10));
            Length = 10;
        }

        private Vector2 vector;
        public ref float Length => ref Projectile.localAI[0];
        public float Rot;
        public int HitCount;
        public ref float Timer => ref Projectile.localAI[1];
        public float progress;
        public float SwingSpeed;
        public override bool PreAI()
        {
            Player player = Main.player[Projectile.owner];
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;
            SwingSpeed = 1 / player.GetAttackSpeed(DamageClass.Melee);

            progress = Timer / (8 * SwingSpeed);

            Projectile.spriteDirection = player.direction;
            player.SetCompositeArmFront(true, Length >= 95 ? Player.CompositeArmStretchAmount.Full : Player.CompositeArmStretchAmount.Quarter, (player.Center - Projectile.Center).ToRotation() + MathHelper.PiOver2);
            if (Timer++ == 0)
            {
                SoundEngine.PlaySound(SoundID.Item1, Projectile.position);
                if (Projectile.owner == Main.myPlayer)
                {
                    startVector = RedeHelper.PolarVector(1, (Main.MouseWorld - player.Center).ToRotation());
                    player.direction = Main.MouseWorld.X > player.Center.X ? 1 : -1;
                }
            }

            if (progress < 1f)
            {
                if (progress < 0.5f)
                {
                    Length = 90 + 10 * (MathF.Pow(15, progress) - 1);
                    Length = MathHelper.Clamp(Length, 90f, 100f);
                    vector = startVector.RotatedBy(Rot) * Length;
                }
                else if (progress < 1f)
                {
                    Length = 90 + 10 * (MathF.Pow(15, 1 - progress) - 1);
                    Length = MathHelper.Clamp(Length, 90f, 100f);
                    vector = startVector.RotatedBy(Rot) * Length;
                }
            }

            if (progress >= 1f)
            {
                Projectile.Kill();
            }

            if (Timer > 1)
                Projectile.alpha = 0;

            Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter, true);
            Projectile.Center = playerCenter + vector;

            if (Projectile.spriteDirection == 1)
                Projectile.rotation = (Projectile.Center - playerCenter).ToRotation() + MathHelper.PiOver4;
            else
                Projectile.rotation = (Projectile.Center - playerCenter).ToRotation() - MathHelper.Pi - MathHelper.PiOver4;

            if (!Main.rand.NextBool(3))
                return false;

            int sparkle = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.SilverCoin, 0, 0, 20);
            Main.dust[sparkle].velocity *= 0;
            Main.dust[sparkle].noGravity = true;
            return false;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Player player = Main.player[Projectile.owner];
            modifiers.HitDirectionOverride = target.RightOfDir(player);
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 unit = new Vector2(1f, 0).RotatedBy(Projectile.rotation + MathHelper.PiOver2 + MathHelper.PiOver4);
            if (Projectile.spriteDirection != 1)
                unit = new Vector2(1f, 0).RotatedBy(Projectile.rotation + MathHelper.PiOver2 - MathHelper.PiOver4);
            float point = 0f;
            // Run an AABB versus Line check to look for collisions
            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center - (unit * 4),
                Projectile.Center + unit * 48, 12, ref point))
                return true;
            else
                return false;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Rectangle rect = new(0, 0, texture.Width, texture.Height);
            Vector2 drawOrigin = new(texture.Width / 2, texture.Height / 2);
            var effects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter, true);
            Vector2 v = Projectile.Center - RedeHelper.PolarVector(50, (Projectile.Center - playerCenter).ToRotation());

            Main.EntitySpriteDraw(texture, v - Main.screenPosition, new Rectangle?(rect), Projectile.GetAlpha(lightColor), Projectile.rotation, drawOrigin, Projectile.scale, effects, 0);
            return false;
        }
    }
}
