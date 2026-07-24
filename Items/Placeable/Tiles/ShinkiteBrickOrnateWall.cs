using Redemption.Rarities;
using Redemption.Walls;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Items.Placeable.Tiles
{
    public class ShinkiteBrickOrnateWall : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Ornate Shinkite Brick Wall");
            Item.ResearchUnlockCount = 400;
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableWall((ushort)WallType<ShinkiteBrickOrnateWallTile>());
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = Item.CommonMaxStack;
            Item.rare = RarityType<TurquoiseRarity>();
        }

        public override void AddRecipes()
        {
            CreateRecipe(4)
                .AddIngredient(ItemType<ShinkiteBrickOrnate>())
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}