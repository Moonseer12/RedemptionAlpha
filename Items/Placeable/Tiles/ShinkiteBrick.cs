using Redemption.Rarities;
using Redemption.Tiles.Tiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Items.Placeable.Tiles
{
    public class ShinkiteBrick : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(TileType<ShinkiteBrickTile>(), 0);
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = Item.CommonMaxStack;
            Item.rare = RarityType<TurquoiseRarity>();
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<ShinkiteBrickWall>(), 4)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}