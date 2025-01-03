using Redemption.Globals;
using Redemption.Items.Materials.HM;
using Redemption.Items.Materials.PostML;
using Redemption.Projectiles.Magic;
using Redemption.Tiles.Furniture.Lab;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Redemption.Items.Weapons.PostML.Magic
{
    public class XeniumStaff : ModItem
    {
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(ElementID.PoisonS);
        public override void SetStaticDefaults()
        {
            /* Tooltip.SetDefault("Casts two harmless bubble mines\n" +
                "Right-click to fire a small " + ElementID.PoisonS + " beam that detonates any mine it hits"); */
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 375;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 14;
            Item.width = 58;
            Item.height = 58;
            Item.useTime = 17;
            Item.useAnimation = 17;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 5;
            Item.value = Item.sellPrice(0, 15, 0, 0);
            Item.rare = ItemRarityID.Purple;
            Item.UseSound = SoundID.Item117;
            Item.autoReuse = true;
            Item.shoot = ProjectileType<XeniumBubble_Proj>();
            Item.shootSpeed = 13f;
        }
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-8, 0);
        }
        public override bool AltFunctionUse(Player player) => true;
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            Vector2 Offset = Vector2.Normalize(velocity) * 70f;

            if (Collision.CanHit(position, 0, 0, position + Offset, 0, 0))
                position += Offset;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                type = ProjectileType<XeniumStaff_Proj>();
                Projectile.NewProjectile(source, position, velocity, type, damage / 4, knockback, player.whoAmI);
                return false;
            }
            int numberProjectiles = 2;
            for (int i = 0; i < numberProjectiles; i++)
            {
                velocity *= Main.rand.NextFloat(0.7f, 1.5f);
                Vector2 perturbedSpeed = velocity.RotatedByRandom(MathHelper.ToRadians(15));
                Projectile.NewProjectile(source, position, perturbedSpeed, type, damage, knockback, player.whoAmI);
            }

            if (player.ownedProjectileCounts[type] >= 30)
            {
                for (int i = 0; i < 2; i++)
                {
                    int num = 9999999;
                    int oldestBubble = -1;
                    foreach (Projectile proj in Main.ActiveProjectiles)
                    {
                        if (proj.type != type)
                            continue;

                        if (proj.timeLeft < num)
                        {
                            oldestBubble = proj.whoAmI;
                            num = proj.timeLeft;
                        }
                    }
                    if (oldestBubble > -1)
                        Main.projectile[oldestBubble].Kill();
                }
            }
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<XeniumAlloy>(), 12)
                .AddIngredient(ItemType<Capacitor>())
                .AddIngredient(ItemType<CarbonMyofibre>(), 5)
                .AddTile(TileType<XeniumRefineryTile>())
                .Register();
        }
    }
}