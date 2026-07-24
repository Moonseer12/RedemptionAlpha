using Redemption.Tiles.Containers;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Items.Placeable.Containers
{
    public class AshWoodBarrel : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(TileType<AshWoodBarrelTile>(), 0);
            Item.width = 26;
            Item.height = 28;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.sellPrice(0, 0, 1, 0);
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.AshWood, 9)
                .AddRecipeGroup(RecipeGroupID.IronBar)
                .AddTile(TileID.Sawmill)
                .Register();
        }
    }
}
