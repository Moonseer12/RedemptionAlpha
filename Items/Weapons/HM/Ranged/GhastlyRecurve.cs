using Redemption.BaseExtension;
using Redemption.Globals;
using Redemption.Items.Materials.PreHM;
using Redemption.Projectiles.Ranged;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
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
            Item.width = 26;
            Item.height = 88;
            Item.rare = ItemRarityID.Yellow;
            Item.value = Item.sellPrice(gold: 5);

            // Use Properties
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.UseSound = SoundID.Item5;
            Item.autoReuse = true;

            // Weapon Properties
            Item.damage = 57;
            Item.knockBack = 2;
            Item.DamageType = DamageClass.Ranged;
            Item.noMelee = true;

            // Projectile Properties
            Item.shootSpeed = 20f;
            Item.shoot = ProjectileID.WoodenArrowFriendly;
            Item.useAmmo = AmmoID.Arrow;

            Item.Redemption().HideElementTooltip[ElementID.Arcane] = true;
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
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-2, 0);
        }
        public override bool AltFunctionUse(Player player) => true;
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                SoundEngine.PlaySound(SoundID.Zombie54, player.Center);
                type = ProjectileType<GhastlyRecurve_Proj>();
            }
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                Vector2 vector = Main.MouseWorld;
                int p = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, vector.X, vector.Y);
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
                return false;
            }
            return true;
        }
    }
}
