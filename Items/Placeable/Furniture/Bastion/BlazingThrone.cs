using Redemption.Rarities;
using Redemption.Tiles.Furniture.Bastion;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Items.Placeable.Furniture.Bastion
{
    public class BlazingThrone : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.DrawUnsafeIndicator[Type] = true;
        }
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(TileType<BlazingThroneTile>(), 0);
            Item.width = 60;
            Item.height = 68;
            Item.maxStack = Item.CommonMaxStack;
            Item.rare = RarityType<TurquoiseRarity>();
            Item.value = 10000;
        }
    }
}