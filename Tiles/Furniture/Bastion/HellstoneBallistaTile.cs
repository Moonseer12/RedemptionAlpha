using Redemption.Items.Placeable.Furniture.Bastion;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Redemption.Tiles.Furniture.Bastion
{
    public class HellstoneBallistaTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileLavaDeath[Type] = false;
            Main.tileNoAttach[Type] = true;
            TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
            TileID.Sets.PreventsTileHammeringIfOnTopOfIt[Type] = true;
            TileID.Sets.HasOutlines[Type] = true;
            TileObjectData.newTile.Width = 6;
            TileObjectData.newTile.Height = 4;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 18 };
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.Origin = new Point16(2, 3);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.addTile(Type);
            MinPick = 200;
            MineResist = 3f;
            DustType = DustID.Obsidian;
            LocalizedText name = CreateMapEntryName();
            AddMapEntry(new Color(68, 68, 76), name);
            RegisterItemDrop(ItemType<BrokenHellstoneBallista>());
        }

        private static bool HasRepairMaterials() => Main.LocalPlayer.CountItem(ItemID.HellstoneBar) >= 30;

        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => Main.tile[i, j].TileFrameX < 108 && HasRepairMaterials();
        public override void MouseOver(int i, int j)
        {
            Main.LocalPlayer.cursorItemIconEnabled = true;
            Main.LocalPlayer.cursorItemIconID = -1;
            if (Main.tile[i, j].TileFrameX >= 108)
                Main.LocalPlayer.cursorItemIconText = "";
            else
                Main.LocalPlayer.cursorItemIconText = Language.GetTextValue("Mods.Redemption.Tiles.HellstoneBallistaTile.Repair");
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (closer)
                return;

            Tile tile = Framing.GetTileSafely(i, j);
            if (!closer && tile.TileFrameX == 108 && tile.TileFrameY == 0)
            {
                if (!Main.projectile.Any(projectile => projectile.type == ProjectileType<HellstoneBallista_Top>() && (projectile.ModProjectile as HellstoneBallista_Top).Parent == tile && projectile.active))
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int ballista = Projectile.NewProjectile(Wiring.GetProjectileSource(i, j), new Vector2(i * 16 + 46, j * 16 + 34), Vector2.Zero, ProjectileType<HellstoneBallista_Top>(), 0, 0, Main.myPlayer);
                        (Main.projectile[ballista].ModProjectile as HellstoneBallista_Top).Parent = tile;
                    }
                }
            }
        }

        public override bool RightClick(int i, int j)
        {
            Player player = Main.LocalPlayer;
            int left = i - Main.tile[i, j].TileFrameX / 18 % 6;
            int top = j - Main.tile[i, j].TileFrameY / 18 % 4;
            if (Main.tile[left, top].TileFrameX == 0 && HasRepairMaterials())
            {
                for (int h = 0; h < 30; h++)
                    player.ConsumeItem(ItemID.HellstoneBar);

                SoundEngine.PlaySound(SoundID.Grab);
                SoundEngine.PlaySound(SoundID.Item37);
                for (int x = left; x < left + 6; x++)
                {
                    for (int y = top; y < top + 4; y++)
                    {
                        int d = Dust.NewDust(new Vector2(x * 16, y * 16), 16, 16, DustID.Torch, 0f, -1, 0, default, 2f);
                        Main.dust[d].velocity.Y -= 2f;
                        Main.dust[d].noGravity = true;

                        if (Main.tile[x, y].TileFrameX < 108)
                            Main.tile[x, y].TileFrameX += 108;
                    }
                }
                return true;
            }
            return false;
        }
        public override bool CanExplode(int i, int j) => false;
    }
}