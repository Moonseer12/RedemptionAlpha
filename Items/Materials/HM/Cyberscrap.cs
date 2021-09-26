using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Items.Materials.HM
{
    public class Cyberscrap : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Cyberscrap");
            Tooltip.SetDefault("'Versatile, and can be used to make anything.'");
        }

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 28;
            Item.maxStack = 999;
            Item.value = Item.buyPrice(0, 10, 0, 0);
            Item.rare = ItemRarityID.Cyan;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            //Item.createTile = ModContent.TileType<JunkMetalTile>(); 
        }
    }
}