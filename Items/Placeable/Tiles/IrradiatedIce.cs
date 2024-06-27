using Redemption.Tiles.Tiles;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Items.Placeable.Tiles
{
    public class IrradiatedIce : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<IrradiatedSnow>();
            ItemTrader.ChlorophyteExtractinator.AddOption_OneWay(Type, 1, ItemID.IceBlock, 1);
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<IrradiatedIceTile>(), 0);
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = Item.CommonMaxStack;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<IrradiatedIceWall>(), 4)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}