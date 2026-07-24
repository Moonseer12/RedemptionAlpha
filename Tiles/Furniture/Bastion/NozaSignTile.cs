using Redemption.Items;
using Terraria;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Redemption.Tiles.Furniture.Bastion
{
    public class NozaSignTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSign[Type] = true;
            Main.tileFrameImportant[Type] = true;
            Main.tileLavaDeath[Type] = true;
            TileID.Sets.DisableSmartCursor[Type] = true;
            TileID.Sets.FramesOnKillWall[Type] = true;
            TileID.Sets.AvoidedByNPCs[Type] = true;
            TileID.Sets.TileInteractRead[Type] = true;
            TileID.Sets.InteractibleByNPCs[Type] = true;

            VanillaFallbackOnModDeletion = TileID.Signs;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 22 };
            TileObjectData.newTile.DrawYOffset = -6;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.StyleMultiplier = 5;
            TileObjectData.newTile.AnchorBottom = AnchorData.Empty;

            // To reduce code repetition, we'll use the same AnchorData value multiple times. This works because the tile is as tall as it is wide.
            AnchorData SolidOrSolidSideAnchor2TilesLong = new(AnchorType.SolidTile | AnchorType.SolidSide, 2, 0);

            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.Origin = Point16.Zero;
            TileObjectData.newAlternate.AnchorTop = SolidOrSolidSideAnchor2TilesLong;
            TileObjectData.addAlternate(1);

            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.Origin = Point16.Zero;
            TileObjectData.newAlternate.AnchorLeft = SolidOrSolidSideAnchor2TilesLong;
            TileObjectData.addAlternate(2);

            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.Origin = new Point16(1, 0);
            TileObjectData.newAlternate.AnchorRight = SolidOrSolidSideAnchor2TilesLong;
            TileObjectData.addAlternate(3);

            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.Origin = Point16.Zero;
            TileObjectData.newAlternate.AnchorWall = true;
            TileObjectData.addAlternate(4);

            // Finally, we restore the default AnchorBottom, the extra AnchorTypes here allow placing on tables, platforms, and other tiles.
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.Table | AnchorType.SolidSide, 2, 0);
            TileObjectData.addTile(Type);

            // Map entry and extra localization
            LocalizedText name = CreateMapEntryName();
            AddMapEntry(new Color(143, 117, 121), name);
            DustType = DustID.Shadewood;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Sign.KillSign(i, j);
        }

        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
        {
            return true;
        }
    }
}