using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Redemption.Tiles.Furniture.Bastion
{
    public class HolyAllianceStatue2Tile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileObsidianKill[Type] = true;
            Main.tileSpelunker[Type] = true;
            TileID.Sets.DisableSmartCursor[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style6x3);
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 48 };
            TileObjectData.newTile.DrawYOffset = -30;
            TileObjectData.addTile(Type);

            DustType = DustID.Stone;

            AddMapEntry(new Color(165, 165, 165), Language.GetText("MapObject.Statue"));
        }
    }
}