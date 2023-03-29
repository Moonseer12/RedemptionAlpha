using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Redemption.Tiles.Banners;

namespace Redemption.Items.Placeable.Banners
{
    public class SneezyFlinxBanner : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sneezy Snow Flinx Banner");
            SacrificeTotal = 1;
        }
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<SneezyFlinxBannerTile>(), 0);
            Item.width = 12;
            Item.height = 28;
            Item.maxStack = 9999;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.buyPrice(0, 0, 10, 0);
        }
    }
}