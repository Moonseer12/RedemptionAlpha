using Microsoft.Xna.Framework.Graphics;
using Redemption.BaseExtension;
using Redemption.Globals;
using Redemption.Projectiles.Ranged;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Items.Weapons.HM.Ranged
{
    public class BileLauncher_Proj : TrueMeleeProjectile
    {
        public override string Texture => "Redemption/Items/Weapons/HM/Ranged/BileLauncher";
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Bile Launcher");
        }
        public override void SetSafeDefaults()
        {
            Projectile.width = 66;
            Projectile.height = 36;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
        }
        private float offset;
        private float shake;
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Vector2 vector = player.RotatedRelativePoint(player.MountedCenter);
            ProjHelper.HoldOutProjBasics(Projectile, player, vector);
            Projectile.Center = vector;
            Projectile.spriteDirection = Projectile.direction;
            Projectile.timeLeft = 2;
            player.ChangeDir(Projectile.direction);
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;
            player.itemRotation = (float)Math.Atan2(Projectile.velocity.Y * Projectile.direction, Projectile.velocity.X * Projectile.direction);

            float num = 0;
            if (Projectile.spriteDirection == -1)
                num = MathHelper.Pi;
            Projectile.rotation = Projectile.velocity.ToRotation() + num;

            offset -= 2;
            if (Main.myPlayer == Projectile.owner)
            {
                if (Projectile.localAI[1] >= player.HeldItem.useTime / player.GetAttackSpeed(DamageClass.Ranged))
                {
                    if (Projectile.localAI[0]++ % 3 == 0)
                    {
                        float shootSpeed = player.inventory[player.selectedItem].shootSpeed;
                        Vector2 gunPos = Projectile.Center + RedeHelper.PolarVector(31 * Projectile.spriteDirection, Projectile.rotation);
                        Vector2 gunSmokePos = Projectile.Center + RedeHelper.PolarVector(31 * Projectile.spriteDirection, Projectile.rotation) + RedeHelper.PolarVector(-2, Projectile.rotation + MathHelper.PiOver2);
                        for (int i = 0; i < 6; i++)
                        {
                            int num5 = Dust.NewDust(gunSmokePos, 2, 8, DustID.ToxicBubble, Projectile.velocity.X / 2f, Projectile.velocity.Y / 2f);
                            Main.dust[num5].velocity = RedeHelper.PolarVector(Main.rand.NextFloat(6, 8) * Projectile.spriteDirection, Projectile.rotation + Main.rand.NextFloat(-0.2f, 0.2f));
                            Main.dust[num5].velocity *= 3f;
                            Main.dust[num5].noGravity = true;
                        }
                        offset = 6;
                        player.RedemptionScreen().ScreenShakeIntensity += 1;
                        SoundEngine.PlaySound(SoundID.NPCHit20, Projectile.position);
                        for (int i = 0; i < 2; i++)
                            Projectile.NewProjectile(Projectile.GetSource_FromAI(), gunPos, RedeHelper.PolarVector(shootSpeed + (Projectile.localAI[0] / 2), (Main.MouseWorld - gunPos).ToRotation() + Main.rand.NextFloat(-0.13f, 0.13f)), ProjectileType<BileLauncher_Gloop>(), Projectile.damage / 3, Projectile.knockBack / 2, player.whoAmI);
                    }
                    if (Projectile.localAI[0] >= 40)
                        Projectile.Kill();
                }
                else
                {
                    Projectile.localAI[1]++;
                    shake += 0.05f;
                    if (Projectile.localAI[1] >= player.HeldItem.useTime - 1 && !Main.dedServ)
                        SoundEngine.PlaySound(CustomSounds.ShootChange, Projectile.position);
                    Projectile.position += new Vector2(Main.rand.NextFloat(-shake, shake), Main.rand.NextFloat(-shake, shake));
                }
            }
            shake = MathHelper.Min(shake, 1.6f);
            offset = MathHelper.Clamp(offset, 0, 20);
            if (Projectile.ai[1]++ > 1)
                Projectile.alpha = 0;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new(texture.Width / 2, Projectile.height / 2);
            Vector2 v = RedeHelper.PolarVector(-16 + offset, Projectile.velocity.ToRotation());
            SpriteEffects spriteEffects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Main.EntitySpriteDraw(texture, Projectile.Center - v - Main.screenPosition,
                null, Projectile.GetAlpha(lightColor), Projectile.rotation, drawOrigin, Projectile.scale, spriteEffects, 0);
            return false;
        }
    }
}