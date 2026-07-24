using Redemption.Dusts.Tiles;
using Terraria;
using Terraria.ID;

namespace Redemption.Tiles.Tiles
{
    public class DarkShinkiteBrickTile : CompositeTile
    {
        public override int HorizontalSheetCount { get; } = 3;
        public override int VerticalSheetCount { get; } = 3;

        public override void SetStaticDefaults()
        {
            Main.tileMergeDirt[Type] = false;
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileBrick[Type] = true;
            TileID.Sets.DrawsWalls[Type] = true;
            DustType = DustType<ShinkiteDust>();

            MinPick = 200;
            MineResist = 4f;
            HitSound = CustomSounds.BrickHit;
            AddMapEntry(new Color(110, 66, 78));
        }
        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
        public override bool CanExplode(int i, int j) => false;
    }
}