using Microsoft.Xna.Framework.Graphics;
using Redemption.Buffs;
using Redemption.Buffs.Debuffs;
using Redemption.Globals;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Items.Weapons.HM.Ranged
{
    public class HyperTechRevolvers_Proj : TrueMeleeProjectile
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Hyper-Tech Revolver");
        }
        public override void SetSafeDefaults()
        {
            Projectile.width = 56;
            Projectile.height = 30;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.hide = true;
        }
        public override bool ShouldUpdatePosition() => false;
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            if (Projectile.ai[0] == 1)
                behindNPCs.Add(index);
        }
        private float offset;
        private float rotOffset;
        private int bullet = 1;
        private bool swap;
        private bool reset;
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (Projectile.ai[1]++ == 0 && Projectile.ai[0] == 1)
                swap = true;

            bool playerBuff = player.HasBuff<RevolverTossBuff>() || player.HasBuff<RevolverTossBuff2>() || player.HasBuff<RevolverTossBuff3>();
            if (player.HasBuff<RevolverTossDebuff>() && !playerBuff)
            {
                if (Projectile.ai[0] == 1)
                {
                    Projectile.Kill();
                    return;
                }
            }
            if (player.HasBuff<RevolverTossDebuff>() && player.ownedProjectileCounts[ProjectileType<HyperTechRevolvers_Proj>()] == 2)
            {
                Projectile.Kill();
                return;
            }
            if (!player.HasBuff<RevolverTossDebuff>() && player.ownedProjectileCounts[ProjectileType<HyperTechRevolvers_Proj>()] == 1 && player.ownedProjectileCounts[ProjectileType<HyperTechRevolvers_Proj2>()] == 0)
            {
                Projectile.Kill();
                return;
            }
            if (player.ownedProjectileCounts[ProjectileType<HyperTechRevolvers_Proj2>()] > 0)
            {
                if (playerBuff)
                    reset = true;
            }
            else if (reset)
            {
                Projectile.Kill();
                return;
            }

            Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter);
            ProjHelper.HoldOutProjBasics(Projectile, player, playerCenter);
            int firerate = (int)(player.HeldItem.useTime / player.GetTotalAttackSpeed(DamageClass.Ranged));

            Projectile.Center = playerCenter;
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

            Vector2 gunPos = playerCenter + Projectile.velocity.SafeNormalize(default).RotatedBy(1.57f) * -4 * Projectile.direction + Projectile.velocity.SafeNormalize(default) * 20;
            if (Projectile.ai[0] == 1)
                gunPos = playerCenter + Projectile.velocity.SafeNormalize(default).RotatedBy(1.57f) * -14 * Projectile.direction + Projectile.velocity.SafeNormalize(default) * 20;

            offset -= 5;
            rotOffset += 0.05f;
            if (player.HasBuff<RevolverTossBuff>())
                firerate = (int)(player.HeldItem.useTime * 0.8f);
            else if (player.HasBuff<RevolverTossBuff2>())
                firerate = (int)(player.HeldItem.useTime * 0.6f);
            else if (player.HasBuff<RevolverTossBuff3>())
                firerate = (int)(player.HeldItem.useTime * 0.4f);

            if (Projectile.localAI[1]++ % firerate == 0)
            {
                if (!player.channel)
                {
                    Projectile.Kill();
                    return;
                }
                if (!swap)
                {
                    if (player.PickAmmo(player.HeldItem, out bullet, out float shootSpeed, out int weaponDamage, out float weaponKnockback, out int usedAmmoId))
                    {
                        if (bullet == ProjectileID.Bullet)
                            bullet = ProjectileID.NanoBullet;

                        offset = 15;
                        rotOffset = -0.3f;
                        SoundEngine.PlaySound(SoundID.Item41, Projectile.position);

                        if (Projectile.owner == Main.myPlayer)
                            Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), gunPos, Projectile.velocity.SafeNormalize(default) * shootSpeed, bullet, Projectile.damage, Projectile.knockBack, player.whoAmI).rotation = Projectile.velocity.ToRotation() + 1.57f;
                    }
                }
                swap = !swap;
            }
            if (Main.mouseRight && Main.mouseRightRelease)
            {
                if (player.ownedProjectileCounts[ProjectileType<HyperTechRevolvers_Proj2>()] == 0 && Projectile.ai[0] == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item7, player.Center);
                    float dir = player.velocity.X > 0 ? 1 : -1;
                    float speed = MathF.Abs(player.velocity.X);
                    float velX = speed < 3 ? Main.rand.NextFloat(-3, 3) : Main.rand.NextFloat(3, 3 + speed * 0.2f) * dir;
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), playerCenter, new Vector2(velX, -10), ProjectileType<HyperTechRevolvers_Proj2>(), 0, 0, player.whoAmI, -player.direction);
                    Projectile.Kill();
                }
            }
            offset = MathHelper.Clamp(offset, 0, 20);
            rotOffset = MathHelper.Clamp(rotOffset, -1, 0);
            if (Projectile.ai[1] > 1)
                Projectile.alpha = 0;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteEffects spriteEffects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Texture2D glow = Request<Texture2D>(Texture + "_Glow").Value;
            Vector2 drawOrigin = Projectile.spriteDirection == 1 ? new(0, texture.Height / 2) : new(texture.Width, texture.Height / 2);
            Vector2 v = RedeHelper.PolarVector(offset, Projectile.velocity.ToRotation());
            Vector2 pos = Projectile.Center;
            if (Projectile.ai[0] == 1)
                pos = Projectile.Center - new Vector2(6 * Projectile.spriteDirection, 6);

            Main.EntitySpriteDraw(texture, pos - v - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation + (rotOffset * Projectile.spriteDirection), drawOrigin, Projectile.scale, spriteEffects, 0);
            Main.EntitySpriteDraw(glow, pos - v - Main.screenPosition, null, Projectile.GetAlpha(Color.White), Projectile.rotation + (rotOffset * Projectile.spriteDirection), drawOrigin, Projectile.scale, spriteEffects, 0);
            return false;
        }
    }
}