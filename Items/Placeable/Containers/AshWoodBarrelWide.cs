using Redemption.Tiles.Containers;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Items.Placeable.Containers
{
    public class AshWoodBarrelWide : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(TileType<AshWoodBarrelWideTile>(), 0);
            Item.width = 30;
            Item.height = 34;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.sellPrice(0, 0, 1, 0);
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.AshWood, 11)
                .AddRecipeGroup(RecipeGroupID.IronBar)
                .AddTile(TileID.Sawmill)
                .Register();
        }
    }
}
