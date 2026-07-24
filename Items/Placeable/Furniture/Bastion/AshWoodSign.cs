using Redemption.Tiles.Furniture.Bastion;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Items.Placeable.Furniture.Bastion
{
    public class AshWoodSign : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(TileType<AshWoodSignTile>(), 0);
            Item.width = 32;
            Item.height = 38;
            Item.maxStack = Item.CommonMaxStack;
            Item.rare = ItemRarityID.White;
            Item.value = 0;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.AshWood, 6)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}