using Redemption.Rarities;
using Redemption.Tiles.Tiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Items.Placeable.Tiles
{
    public class DarkShinkiteBrick : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 50;
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(TileType<DarkShinkiteBrickTile>());
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = Item.CommonMaxStack;
            Item.rare = RarityType<TurquoiseRarity>();
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ShinkiteBrick>(2)
                .AddIngredient(ItemID.Obsidian)
                .AddTile(TileID.HeavyWorkBench)
                .Register();
        }
    }
}