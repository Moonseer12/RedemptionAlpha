using Microsoft.Xna.Framework.Graphics;
using Redemption.BaseExtension;
using Redemption.Globals;
using Redemption.Items.Weapons.PreHM.Ranged;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Items.Weapons.PreHM.Melee
{
    public class Halbirdhouse_Proj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Halbirdhouse");
        }
        private Vector2 startVector;
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.Spear);
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.alpha = 255;
            Projectile.usesLocalNPCImmunity = true;

            Projectile.Redemption().TechnicallyMelee = true;
        }

        private Vector2 vector;
        public ref float Length => ref Projectile.localAI[0];
        public float Rot;
        public int HitCount;
        public ref float Timer => ref Projectile.localAI[1];
        public float SwingSpeed;
        public float progress;

        public override bool PreAI()
        {
            Player player = Main.player[Projectile.owner];
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;

            Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter, true);
            Projectile.Center = playerCenter + vector;

            SwingSpeed = 1 / player.GetAttackSpeed(DamageClass.Melee);
            progress = Timer / (9 * SwingSpeed);

            Projectile.localNPCHitCooldown = (int)(10 * SwingSpeed);

            Projectile.spriteDirection = player.direction;

            player.SetCompositeArmFront(true, Length >= 40 ? Player.CompositeArmStretchAmount.Full : Player.CompositeArmStretchAmount.Quarter, (playerCenter - Projectile.Center).ToRotation() + MathHelper.PiOver2);
            if (Timer++ == 0)
            {
                if (HitCount >= 3 && Projectile.owner == Main.myPlayer)
                {
                    int p = Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, player.DirectionTo(Main.MouseWorld) * 14, ProjectileType<ChickenEgg_Proj>(), Projectile.damage, Projectile.knockBack, player.whoAmI, 1);
                    Main.projectile[p].DamageType = DamageClass.Melee;
                    Main.projectile[p].netUpdate = true;
                    HitCount = 0;
                }
                SoundEngine.PlaySound(SoundID.Item1, Projectile.position);
                startVector = RedeHelper.PolarVector(1, (Main.MouseWorld - playerCenter).ToRotation());
                Rot = MathHelper.ToRadians(Main.rand.NextFloat(-10, 10));
                player.direction = Main.MouseWorld.X > player.Center.X ? 1 : -1;
                Length = 30;
            }

            if (progress < 0.78f)
                Length = 30 * MathF.Pow(5f, progress);
            else
                Length = 107 * MathF.Pow(0.05f, progress);
            Length = MathHelper.Clamp(Length, 36, 60);

            vector = startVector.RotatedBy(Rot) * Length;
            if (progress >= 1)
            {
                if (player.channel)
                    Timer = 0;
                else
                    Projectile.Kill();
            }

            if (Timer > 1)
                Projectile.alpha = 0;


            if (Projectile.spriteDirection == 1)
                Projectile.rotation = (Projectile.Center - playerCenter).ToRotation() + MathHelper.PiOver4;
            else
                Projectile.rotation = (Projectile.Center - playerCenter).ToRotation() - MathHelper.Pi - MathHelper.PiOver4;
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            HitCount++;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Rectangle rect = new(0, 0, texture.Width, texture.Height);
            Vector2 drawOrigin = new(texture.Width / 2, texture.Height / 2);
            var effects = Projectile.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter, true);
            Vector2 v = Projectile.Center - RedeHelper.PolarVector(28, (Projectile.Center - playerCenter).ToRotation());

            Main.EntitySpriteDraw(texture, v - Main.screenPosition, new Rectangle?(rect), Projectile.GetAlpha(lightColor), Projectile.rotation, drawOrigin, Projectile.scale, effects, 0);
            return false;
        }
    }
}
