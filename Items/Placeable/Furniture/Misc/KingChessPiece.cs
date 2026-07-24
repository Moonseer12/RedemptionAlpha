using Redemption.Tiles.Furniture.Misc;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Items.Placeable.Furniture.Misc
{
    public class KingChessPiece : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Marble King Chess Piece");
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(TileType<KingChessPieceTile>(), 0);
            Item.width = 22;
            Item.height = 52;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.sellPrice(0, 0, 8, 0);
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Marble, 16)
                .AddTile(TileID.HeavyWorkBench)
                .Register();
        }
    }
}