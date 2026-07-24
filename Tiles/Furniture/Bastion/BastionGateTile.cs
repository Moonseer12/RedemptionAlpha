using Microsoft.Xna.Framework.Graphics;
using Redemption.BaseExtension;
using Redemption.Globals;
using Redemption.Rarities;
using Redemption.WorldGeneration;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Redemption.Tiles.Furniture.Bastion
{
    public class BastionGateTile : ModTile
    {
        public override string Texture => Redemption.EMPTY_TEXTURE;
        public Asset<Texture2D> GateTexture;
        public override void Load()
        {
            if (!Main.dedServ)
                GateTexture = Request<Texture2D>("Redemption/Tiles/Furniture/Bastion/BastionGateTile");
        }

        private bool _activated;
        public int _drawOffset;
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileNoAttach[Type] = true;
            TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
            TileID.Sets.PreventsTileHammeringIfOnTopOfIt[Type] = true;
            RedeTileHelper.CannotMineTileAbove[Type] = true;
            TileID.Sets.NotReallySolid[Type] = true;
            TileID.Sets.DrawsWalls[Type] = true;
            TileID.Sets.DisableSmartCursor[Type] = true;
            TileObjectData.newTile.Width = 3;
            TileObjectData.newTile.Height = 11;
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16 };
            TileObjectData.newTile.CoordinateWidth = 20;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.Origin = new Point16(1, 0);
            TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
            TileObjectData.addTile(Type);
            LocalizedText name = CreateMapEntryName();
            AddMapEntry(new Color(116, 121, 144), name);
            MinPick = 5000;
            MineResist = 30f;
            HitSound = SoundID.Tink;
            DustType = DustID.Lead;
        }
        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = 1;
        }
        public override bool CreateDust(int i, int j, ref int type) => !_activated;
        public override bool KillSound(int i, int j, bool fail) => !_activated;
        public override bool CanKillTile(int i, int j, ref bool blockDamaged) => false;
        public override bool CanExplode(int i, int j) => false;
        public virtual void Offset()
        {
            if (_activated)
            {
                if (_drawOffset > -160)
                    _drawOffset -= 5;
            }
            else
            {
                if (_drawOffset < 0)
                    _drawOffset += 20;
                else
                    _drawOffset = 0;
            }
        }
        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (Main.tile[i, j].TileFrameX != 0 || Main.tile[i, j].TileFrameY != 0)
                return false;

            Vector2 zero = new(Main.offScreenRange, Main.offScreenRange);
            if (Main.drawToScreen)
                zero = Vector2.Zero;

            Offset();

            Vector2 drawPos = new Vector2((i + 1.5f) * 16, ((j + 5.5f) * 16) + _drawOffset) - Main.screenPosition + zero;
            Vector2 origin = GateTexture.Size() / 2;
            int horizontalSlices = (int)MathF.Ceiling(GateTexture.Value.Width / 16f);
            int verticalSlices = (int)MathF.Ceiling(GateTexture.Value.Height / 16f);
            for (int x = 0; x < horizontalSlices; x++)
            {
                for (int y = 0; y < verticalSlices; y++)
                {
                    Rectangle slicedFrame = new(x * 16, y * 16, 16, 16);
                    if ((x + 1) * 16 > GateTexture.Value.Width)
                        slicedFrame.Width = GateTexture.Value.Width % 16;
                    if ((y + 1) * 16 > GateTexture.Value.Height)
                        slicedFrame.Height = GateTexture.Value.Height % 16;

                    Vector2 sliceOffset = slicedFrame.Location.ToVector2();
                    Point sliceTileCoords = (drawPos + Main.screenPosition - zero + sliceOffset - origin).ToTileCoordinates();
                    Color sliceColor = Lighting.GetColor(sliceTileCoords);

                    spriteBatch.Draw(GateTexture.Value, drawPos, slicedFrame, sliceColor, 0f, origin - sliceOffset, 1f, 0, 0f);
                }
            }
            return true;
        }
    }
    public class BastionGate : PlaceholderTile
    {
        public override string Texture => "Redemption/Tiles/Placeholder/BastionGate";
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.createTile = TileType<BastionGateTile>();
            Item.rare = RarityType<TurquoiseRarity>();
        }
    }
}