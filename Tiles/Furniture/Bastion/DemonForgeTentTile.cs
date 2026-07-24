using Redemption.Globals;
using Redemption.NPCs.Bastion.Bazaar;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Redemption.Tiles.Furniture.Bastion
{
    public class DemonForgeTentTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileSolid[Type] = false;
            Main.tileNoAttach[Type] = true;
            TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
            TileID.Sets.PreventsTileHammeringIfOnTopOfIt[Type] = true;
            TileID.Sets.DisableSmartCursor[Type] = true;

            TileObjectData.newTile.Width = 11;
            TileObjectData.newTile.Height = 8;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 16, 16, 16, 16, 16 };
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 0;
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.newTile.Origin = new Point16(5, 7);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.addTile(Type);
            LocalizedText name = CreateMapEntryName();
            AddMapEntry(new Color(112, 90, 84), name);
        }
        public override bool CanKillTile(int i, int j, ref bool blockDamaged) => false;
        public override bool CanExplode(int i, int j) => false;
        public override bool KillSound(int i, int j, bool fail) => false;
        public override bool CreateDust(int i, int j, ref int type) => false;

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (!closer)
                SpawnDemon(i, j, false);
        }
        internal static void SpawnDemon(int i, int j, bool manmade)
        {
            Tile tile = Main.tile[i, j];
            if (tile.TileFrameX != 0 || tile.TileFrameY != 0)
            {
                return;
            }

            int demonID = NPCType<HollowfireSmith>();
            bool canSpawn = true;

            foreach (var npc in Main.ActiveNPCs)
            {
                if (npc.type != demonID || npc.ModNPC == null)
                {
                    continue;
                }
                if (npc.ModNPC is HollowfireSmith demon && demon.Parent.X == i && demon.Parent.Y == j)
                {
                    canSpawn = false;
                    break;
                }
            }

            if (canSpawn)
            {
                SendSpawnDemon(i, j);
            }
        }

        public static void SendSpawnDemon(int i, int j)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                Vector2 spawnPos = new(i * 16 + 125, j * 16 + 131);
                int demonNPC = NPC.NewNPC(new EntitySource_TileUpdate(i, j), (int)spawnPos.X, (int)spawnPos.Y, NPCType<HollowfireSmith>());
                if (Main.npc[demonNPC].ModNPC is HollowfireSmith demon)
                {
                    demon.Parent = new Point16(i, j);
                }
            }
            else if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                var netMessage = Redemption.Instance.GetPacket(5);
                netMessage.Write((byte)RedeNet.ModMessageType.SpawnBazaarSmithNPC);
                netMessage.Write((short)i);
                netMessage.Write((short)j);
                netMessage.Send();
            }
        }

        public static void HandlerSpawnDemon(BinaryReader reader, int whoAmI)
        {
            if (Main.netMode != NetmodeID.Server)
                return;

            int i = reader.ReadInt16();
            int j = reader.ReadInt16();

            Vector2 spawnPos = new(i * 16 + 125, j * 16 + 131);
            int demonNPC = NPC.NewNPC(new EntitySource_TileUpdate(i, j), (int)spawnPos.X, (int)spawnPos.Y, NPCType<HollowfireSmith>(), Target: whoAmI);
            if (Main.npc[demonNPC].ModNPC is HollowfireSmith demon)
            {
                demon.Parent = new Point16(i, j);
                demon.NPC.netUpdate = true;
            }
        }
    }
    public class DemonForgeTent : PlaceholderTile
    {
        public override string Texture => "Redemption/Tiles/Placeholder/DemonForgeTent";
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.createTile = TileType<DemonForgeTentTile>();
        }
    }
}