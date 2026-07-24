using Redemption.Dusts.Tiles;
using Terraria;
using Terraria.ModLoader;

namespace Redemption.Walls
{
    public class ShinkiteBrickOrnateWallTile : ModWall
    {
        public override void SetStaticDefaults()
        {
            Main.wallHouse[Type] = true;
            DustType = DustType<ShinkiteDust>();
            AddMapEntry(new Color(38, 20, 22));
        }
        public override bool CanExplode(int i, int j)
        {
            return false;
        }
    }
    public class ShinkiteBrickOrnateWallTileUnsafe : ShinkiteBrickOrnateWallTile
    {
        public override string Texture => "Redemption/Walls/ShinkiteBrickOrnateWallTile";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Main.wallHouse[Type] = false;
        }
    }
}