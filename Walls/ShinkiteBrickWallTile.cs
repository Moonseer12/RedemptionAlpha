using Redemption.Dusts.Tiles;
using Terraria;
using Terraria.ModLoader;

namespace Redemption.Walls
{
    public class ShinkiteBrickWallTile : ModWall
    {
        public override void SetStaticDefaults()
        {
            Main.wallHouse[Type] = true;
            DustType = DustType<ShinkiteDust>();
            AddMapEntry(new Color(38, 20, 22));
        }
    }
    public class ShinkiteBrickWallTileUnsafe : ShinkiteBrickWallTile
    {
        public override string Texture => "Redemption/Walls/ShinkiteBrickWallTile";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Main.wallHouse[Type] = false;
        }
    }
}