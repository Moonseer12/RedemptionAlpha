using Redemption.Tiles.Furniture.Bastion;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Items.Placeable.Furniture.Bastion
{
    public class HolyAllianceStatue : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(TileType<HolyAllianceStatueTile>(), 0);
            Item.width = 30;
            Item.height = 44;
            Item.maxStack = Item.CommonMaxStack;
            Item.rare = ItemRarityID.White;
            Item.value = 60;
        }
    }
}