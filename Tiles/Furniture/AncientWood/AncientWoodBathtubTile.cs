﻿using Microsoft.Xna.Framework;
using Redemption.Items.Placeable.Furniture.AncientWood;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Redemption.Tiles.Furniture.AncientWood
{
	public class AncientWoodBathtubTile : ModTile
    {
		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style4x2);
			TileObjectData.newTile.Origin = new Point16(1, 1);
			TileObjectData.newTile.DrawYOffset = 2;
			TileObjectData.addTile(Type);

			ModTranslation name = CreateMapEntryName();
			name.SetDefault("Ancient Wood Bathtub");
			AddMapEntry(new Color(109, 87, 78), name);

			DustType = DustID.t_BorealWood;
			AdjTiles = new int[] { TileID.Bathtubs };
		}

		public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
		public override void KillMultiTile(int i, int j, int frameX, int frameY) => Item.NewItem(new EntitySource_TileBreak(i, j), new Vector2(i, j) * 16f, ModContent.ItemType<AncientWoodBathtub>());
	}
}
