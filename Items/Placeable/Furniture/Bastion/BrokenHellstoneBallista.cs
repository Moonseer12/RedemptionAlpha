using Redemption.Tiles.Furniture.Bastion;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Items.Placeable.Furniture.Bastion
{
    public class BrokenHellstoneBallista : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(TileType<HellstoneBallistaTile>(), 0);
            Item.width = 56;
            Item.height = 48;
            Item.maxStack = Item.CommonMaxStack;
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 10, 0, 0);
        }
    }
}