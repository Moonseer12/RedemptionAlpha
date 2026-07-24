using Microsoft.Xna.Framework.Graphics;
using Redemption.BaseExtension;
using Redemption.Globals;
using Redemption.Items.Materials.PreHM;
using Redemption.Projectiles.Ranged;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Redemption.Items.Weapons.HM.Ranged
{
    public class GhastlyRecurve : ModItem
    {
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(ElementID.ArcaneS);
        public override void SetStaticDefaults()
        {
            ElementID.ItemArcane[Type] = true;
        }
        public override void SetDefaults()
        {
            // Common Properties
            Item.width = 30;
            Item.height = 76;
            Item.rare = ItemRarityID.Yellow;
            Item.value = Item.sellPrice(gold: 5);

            // Use Properties
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.UseSound = null;
            Item.autoReuse = true;

            // Weapon Properties
            Item.damage = 57;
            Item.knockBack = 2;
            Item.DamageType = DamageClass.Ranged;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;

            // Projectile Properties
            Item.shootSpeed = 20f;
            Item.shoot = ProjectileType<GhastlyRecurve_Proj>();
            Item.useAmmo = AmmoID.Arrow;

            Item.Redemption().HideElementTooltip[ElementID.Arcane] = true;
        }
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-2, 0);
        }
        public override bool AltFunctionUse(Player player) => true;
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                Item.noUseGraphic = false;
                Item.channel = false;
                int p = Projectile.NewProjectile(source, position, Vector2.Zero, ProjectileType<GhastlyRecurve_Proj>(), damage, knockback, player.whoAmI);
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (!proj.active || proj.type != ProjectileType<GhastlyRecurve_Proj>() || i == p || proj.owner != player.whoAmI)
                        continue;
                    if (player.ownedProjectileCounts[ProjectileType<GhastlyRecurve_Proj>()] > 1)
                    {
                        proj.timeLeft = 2;
                        continue;
                    }
                    (proj.ModProjectile as GhastlyRecurve_Proj).other = Main.projectile[p];
                    (Main.projectile[p].ModProjectile as GhastlyRecurve_Proj).other = Main.projectile[i];
                }
                SoundEngine.PlaySound(SoundID.Zombie54, player.Center);
                return false;
            }
            else
            {
                Item.noUseGraphic = true;
                Item.channel = true;
                Projectile.NewProjectile(source, position, velocity, ProjectileType<GhastlyRecurve_Holdout>(), damage, knockback, player.whoAmI);
            }
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Marrow)
                .AddIngredient(ItemType<LostSoul>(), 12)
                .AddIngredient(ItemID.Ectoplasm, 6)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    public class GhastlyRecurve_Holdout : TrueMeleeProjectile
    {
        public override string Texture => "Redemption/Items/Weapons/HM/Ranged/GhastlyRecurve";
        public override void SetSafeDefaults()
        {
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.width = 30;
            Projectile.height = 76;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
        }
        public override bool ShouldUpdatePosition() => false;
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter);
            ProjHelper.HoldOutProjBasics(Projectile, player, playerCenter);
            int maxTime = (int)(player.HeldItem.useTime / player.GetTotalAttackSpeed(DamageClass.Ranged));

            Projectile.timeLeft = maxTime;
            player.ChangeDir(Projectile.direction);
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;
            player.itemRotation = (float)Math.Atan2(Projectile.velocity.Y * Projectile.direction, Projectile.velocity.X * Projectile.direction);

            Vector2 gunPos1 = playerCenter + Projectile.velocity.SafeNormalize(default) * 20 + Projectile.velocity.SafeNormalize(default).RotatedBy(1.57f) * 0 * Projectile.direction;
            if (Main.myPlayer == Projectile.owner)
            {
                if (Projectile.localAI[0]++ % maxTime == 0)
                {
                    if (!player.channel)
                    {
                        Projectile.Kill();
                        return;
                    }
                    if (player.PickAmmo(player.HeldItem, out int arrow, out float shootSpeed, out int weaponDamage, out float weaponKnockback, out int usedAmmoId))
                    {
                        SoundEngine.PlaySound(SoundID.Item5, Projectile.position);
                        Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), gunPos1, Projectile.velocity.SafeNormalize(default) * shootSpeed, arrow, Projectile.damage, Projectile.knockBack, player.whoAmI).rotation = Projectile.velocity.ToRotation() + 1.57f;
                    }
                }
                if (Main.mouseRight && Main.mouseRightRelease)
                {
                    if (Projectile.localAI[1] <= 0)
                    {
                        Projectile.localAI[1] = maxTime;
                        if (player.PickAmmo(player.HeldItem, out int arrow, out float shootSpeed, out int weaponDamage, out float weaponKnockback, out int usedAmmoId))
                        {
                            int type = ProjectileType<GhastlyRecurve_Proj>();
                            int p = Projectile.NewProjectile(Projectile.GetSource_FromAI(), gunPos1, Vector2.Zero, type, weaponDamage, weaponKnockback, player.whoAmI);
                            for (int i = 0; i < Main.maxProjectiles; i++)
                            {
                                Projectile proj = Main.projectile[i];
                                if (!proj.active || proj.type != type || i == p || proj.owner != player.whoAmI)
                                    continue;

                                if (player.ownedProjectileCounts[type] > 1)
                                {
                                    proj.timeLeft = 2;
                                    continue;
                                }
                                (proj.ModProjectile as GhastlyRecurve_Proj).other = Main.projectile[p];
                                (Main.projectile[p].ModProjectile as GhastlyRecurve_Proj).other = Main.projectile[i];
                            }
                        }
                    }
                }
            }
            Projectile.localAI[1]--;
            Projectile.Center = playerCenter;
            Projectile.spriteDirection = Projectile.direction;
            float num = Projectile.spriteDirection == 1 ? 0 : MathHelper.Pi;
            Projectile.rotation = Projectile.velocity.ToRotation() + num;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteEffects spriteEffects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new(texture.Width / 2, Projectile.height / 2);
            Vector2 pos = Projectile.Center + Projectile.velocity.SafeNormalize(default) * 10;
            Main.EntitySpriteDraw(texture, pos - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, drawOrigin, Projectile.scale, spriteEffects, 0);
            return false;
        }
    }
}