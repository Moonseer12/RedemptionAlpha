﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Redemption.BaseExtension;
using Redemption.Globals;
using Redemption.Items.Materials.PreHM;
using Redemption.Items.Usable;
using Redemption.NPCs.Friendly;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Redemption.Globals.RedeNet;

namespace Redemption.Tiles.Natural
{
    public class SkeletonRemainsTile1_Special : SkeletonRemainsTile1
    {
        public override string Texture => "Redemption/Tiles/Natural/SkeletonRemainsTile1";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RedeTileHelper.CannotMineTileBelow[Type] = true;
            TileID.Sets.HasOutlines[Type] = true;
            Main.tileLighted[Type] = true;
            DustType = DustID.DungeonSpirit;
            MinPick = 5000;
            MineResist = 50;
            LocalizedText name = CreateMapEntryName();
            // name.SetDefault("Skeletal Remains");
            AddMapEntry(new Color(229, 229, 195), name);
        }
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.3f;
            g = 0.3f;
            b = 0.6f;
        }
        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (Main.tile[i, j].TileFrameX == 0 && Main.tile[i, j].TileFrameY == 0)
            {
                if (Main.rand.NextBool(20))
                {
                    int d = Dust.NewDust(new Vector2(i * 16 + 12, j * 16 + 16), 54, 14, DustID.DungeonSpirit);
                    Main.dust[d].velocity.Y -= 3f;
                    Main.dust[d].noGravity = true;
                }
            }
        }
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            RedeTileHelper.SimpleGlowmask(i, j, Color.White, Texture);
        }
        public override bool CanExplode(int i, int j) => false;
        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
            player.cursorItemIconID = ModContent.ItemType<DeadRinger>();
        }
        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => RedeTileHelper.CanDeadRing(Main.LocalPlayer);
        public override bool RightClick(int i, int j)
        {
            Player player = Main.LocalPlayer;
            if (RedeTileHelper.CanDeadRing(player))
            {
                static int GetNPCIndex() => NPCType<SpiritwalkerSoul>();

                if (!player.RedemptionAbility().Spiritwalker)
                {
                    if (!NPC.AnyNPCs(GetNPCIndex()))
                    {
                        Vector2 pos = new Vector2(i, j + 1).ToWorldCoordinates();

                        if (!Main.dedServ)
                            SoundEngine.PlaySound(CustomSounds.Bell, pos);

                        if (Main.netMode == NetmodeID.SinglePlayer)
                            NPC.NewNPC(new EntitySource_TileUpdate(i, j), (int)pos.X, (int)pos.Y, GetNPCIndex());
                        else
                            SpawnNPCFromClient(GetNPCIndex(), pos);

                        SoundEngine.PlaySound(SoundID.Item74, pos);
                    }
                }
                else
                {
                    static int GetNPCIndex2() => NPCType<SpiritWalkerMan>();

                    if (!NPC.AnyNPCs(GetNPCIndex()) && !NPC.AnyNPCs(GetNPCIndex2()))
                    {
                        Vector2 pos = new Vector2(i, j + 1).ToWorldCoordinates();

                        if (!Main.dedServ)
                            SoundEngine.PlaySound(CustomSounds.Bell, pos);

                        if (Main.netMode == NetmodeID.SinglePlayer)
                            NPC.NewNPC(new EntitySource_TileUpdate(i, j), (int)pos.X, (int)pos.Y, GetNPCIndex2());
                        else
                            SpawnNPCFromClient(GetNPCIndex2(), pos);

                        SoundEngine.PlaySound(SoundID.Item74, pos);
                    }
                }
            }
            return true;
        }
        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            RedeTileHelper.DrawSpiritFlare(spriteBatch, i, j, 0, 2.45f, 1.7f);
            return true;
        }
        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            if (!WorldGen.gen && Main.netMode != NetmodeID.MultiplayerClient && !Main.LocalPlayer.RedemptionAbility().Spiritwalker)
                NPC.NewNPC(new EntitySource_TileBreak(i, j), i * 16 + 32, j * 16, ModContent.NPCType<SpiritwalkerSoul>());

            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 64, 32, ModContent.ItemType<GraveSteelShards>(), Main.rand.Next(5, 9));
        }
        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
    }
    public class SkeletonRemainsTile3_Special : SkeletonRemainsTile3
    {
        public override string Texture => "Redemption/Tiles/Natural/SkeletonRemainsTile3";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RedeTileHelper.CannotMineTileBelow[Type] = true;
            TileID.Sets.HasOutlines[Type] = true;
            Main.tileLighted[Type] = true;
            DustType = DustID.DungeonSpirit;
            MinPick = 5000;
            MineResist = 50;
            LocalizedText name = CreateMapEntryName();
            // name.SetDefault("Skeletal Remains");
            AddMapEntry(new Color(229, 229, 195), name);
        }
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.3f;
            g = 0.3f;
            b = 0.6f;
        }
        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (Main.tile[i, j].TileFrameX == 0 && Main.tile[i, j].TileFrameY == 0)
            {
                if (Main.rand.NextBool(20))
                {
                    int d = Dust.NewDust(new Vector2(i * 16, j * 16 + 2), 48, 32, DustID.DungeonSpirit);
                    Main.dust[d].velocity.Y -= 3f;
                    Main.dust[d].noGravity = true;
                }
            }
        }
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            if (!TileDrawing.IsVisible(tile))
                return;
            Vector2 zero = new(Main.offScreenRange, Main.offScreenRange);
            if (Main.drawToScreen)
                zero = Vector2.Zero;

            int height = tile.TileFrameY == 36 ? 18 : 16;
            Main.spriteBatch.Draw(ModContent.Request<Texture2D>(Texture + "_Glow").Value, new Vector2((i * 16) - (int)Main.screenPosition.X, (j * 16) - (int)Main.screenPosition.Y) + zero, new Rectangle(tile.TileFrameX, tile.TileFrameY + 8, 16, height), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }
        public override bool CanExplode(int i, int j) => false;
        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
            if (RedeTileHelper.CanDeadRing(player))
                player.cursorItemIconID = ModContent.ItemType<DeadRinger>();
            else
            {
                player.cursorItemIconEnabled = false;
                player.cursorItemIconID = 0;
            }
        }
        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => RedeTileHelper.CanDeadRing(Main.LocalPlayer);
        public override bool RightClick(int i, int j)
        {
            Player player = Main.LocalPlayer;
            if (RedeTileHelper.CanDeadRing(player))
            {
                static int GetNPCIndex() => NPCType<SpiritAssassin>();

                if (NPC.AnyNPCs(GetNPCIndex()))
                    return false;

                int offset = 0;
                if (Main.tile[i, j].TileFrameX == 0)
                    offset = 2;
                if (Main.tile[i, j].TileFrameX == 18)
                    offset = 1;
                Vector2 pos = new Vector2(i + offset, j + 1).ToWorldCoordinates();

                if (!Main.dedServ)
                    SoundEngine.PlaySound(CustomSounds.Bell, pos);

                if (Main.netMode == NetmodeID.SinglePlayer)
                    NPC.NewNPC(new EntitySource_TileUpdate(i, j), (int)pos.X, (int)pos.Y, GetNPCIndex());
                else
                    SpawnNPCFromClient(GetNPCIndex(), pos);

                SoundEngine.PlaySound(SoundID.Item74, pos);
            }
            return true;
        }
        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            RedeTileHelper.DrawSpiritFlare(spriteBatch, i, j, 0, 1.4f, 0.7f);
            return true;
        }
        public override void KillMultiTile(int i, int j, int frameX, int frameY) => Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 64, 32, ModContent.ItemType<GraveSteelShards>(), Main.rand.Next(5, 9));
        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
    }
    public class SkeletonRemainsTile4_Special : SkeletonRemainsTile4
    {
        public override string Texture => "Redemption/Tiles/Natural/SkeletonRemainsTile4";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RedeTileHelper.CannotMineTileBelow[Type] = true;
            TileID.Sets.HasOutlines[Type] = true;
            Main.tileLighted[Type] = true;
            DustType = DustID.DungeonSpirit;
            MinPick = 5000;
            MineResist = 50;
            LocalizedText name = CreateMapEntryName();
            // name.SetDefault("Skeletal Remains");
            AddMapEntry(new Color(229, 229, 195), name);
        }
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.3f;
            g = 0.3f;
            b = 0.6f;
        }
        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (Main.tile[i, j].TileFrameX == 54 && Main.tile[i, j].TileFrameY == 0)
            {
                if (Main.rand.NextBool(20))
                {
                    int d = Dust.NewDust(new Vector2(i * 16, j * 16), 48, 14, DustID.DungeonSpirit);
                    Main.dust[d].velocity.Y -= 3f;
                    Main.dust[d].noGravity = true;
                }
            }
        }
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            if (!TileDrawing.IsVisible(tile))
                return;
            Vector2 zero = new(Main.offScreenRange, Main.offScreenRange);
            if (Main.drawToScreen)
                zero = Vector2.Zero;

            int height = tile.TileFrameY == 36 ? 18 : 16;
            Main.spriteBatch.Draw(ModContent.Request<Texture2D>(Texture + "_Glow").Value, new Vector2((i * 16) - (int)Main.screenPosition.X, (j * 16) - (int)Main.screenPosition.Y) + zero, new Rectangle(tile.TileFrameX, tile.TileFrameY - 2, 16, height), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }
        public override bool CanExplode(int i, int j) => false;
        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
            if (RedeTileHelper.CanDeadRing(player))
                player.cursorItemIconID = ModContent.ItemType<DeadRinger>();
            else
            {
                player.cursorItemIconEnabled = false;
                player.cursorItemIconID = 0;
            }
        }
        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => RedeTileHelper.CanDeadRing(Main.LocalPlayer);
        public override bool RightClick(int i, int j)
        {
            Player player = Main.LocalPlayer;
            if (RedeTileHelper.CanDeadRing(player))
            {
                static int GetNPCIndex() => NPCType<SpiritCommonGuard>();

                if (NPC.AnyNPCs(GetNPCIndex()))
                    return false;

                int offset = 0;
                if (Main.tile[i, j].TileFrameX == 54)
                    offset = 2;
                if (Main.tile[i, j].TileFrameX == 54 + 18)
                    offset = 1;
                Vector2 pos = new Vector2(i + offset, j + 1).ToWorldCoordinates();

                if (!Main.dedServ)
                    SoundEngine.PlaySound(CustomSounds.Bell, pos);

                if (Main.netMode == NetmodeID.SinglePlayer)
                    NPC.NewNPC(new EntitySource_TileUpdate(i, j), (int)pos.X, (int)pos.Y, GetNPCIndex());
                else
                    SpawnNPCFromClient(GetNPCIndex(), pos);

                SoundEngine.PlaySound(SoundID.Item74, pos);
            }
            return true;
        }
        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            RedeTileHelper.DrawSpiritFlare(spriteBatch, i, j, 54, 1.4f, 0.7f);
            return true;
        }
        public override void KillMultiTile(int i, int j, int frameX, int frameY) => Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 64, 32, ModContent.ItemType<GraveSteelShards>(), Main.rand.Next(5, 9));
        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
    }
    public class SkeletonRemainsTile5_Special : SkeletonRemainsTile5
    {
        public override string Texture => "Redemption/Tiles/Natural/SkeletonRemainsTile5";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RedeTileHelper.CannotMineTileBelow[Type] = true;
            TileID.Sets.HasOutlines[Type] = true;
            Main.tileLighted[Type] = true;
            DustType = DustID.DungeonSpirit;
            MinPick = 5000;
            MineResist = 50;
            LocalizedText name = CreateMapEntryName();
            // name.SetDefault("Skeletal Remains");
            AddMapEntry(new Color(229, 229, 195), name);
        }
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.3f;
            g = 0.3f;
            b = 0.6f;
        }
        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (Main.tile[i, j].TileFrameX == 54 && Main.tile[i, j].TileFrameY == 0)
            {
                if (Main.rand.NextBool(20))
                {
                    int d = Dust.NewDust(new Vector2(i * 16, j * 16), 48, 32, DustID.DungeonSpirit);
                    Main.dust[d].velocity.Y -= 3f;
                    Main.dust[d].noGravity = true;
                }
            }
        }
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            if (!TileDrawing.IsVisible(tile))
                return;
            Vector2 zero = new(Main.offScreenRange, Main.offScreenRange);
            if (Main.drawToScreen)
                zero = Vector2.Zero;

            int height = tile.TileFrameY == 36 ? 18 : 16;
            Main.spriteBatch.Draw(ModContent.Request<Texture2D>(Texture + "_Glow").Value, new Vector2((i * 16) - (int)Main.screenPosition.X, (j * 16) - (int)Main.screenPosition.Y) + zero, new Rectangle(tile.TileFrameX, tile.TileFrameY + 2, 16, height), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }
        public override bool CanExplode(int i, int j) => false;
        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
            if (RedeTileHelper.CanDeadRing(player))
                player.cursorItemIconID = ModContent.ItemType<DeadRinger>();
            else
            {
                player.cursorItemIconEnabled = false;
                player.cursorItemIconID = 0;
            }
        }
        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => RedeTileHelper.CanDeadRing(Main.LocalPlayer);
        public override bool RightClick(int i, int j)
        {
            Player player = Main.LocalPlayer;
            if (RedeTileHelper.CanDeadRing(player))
            {
                static int GetNPCIndex() => NPCType<SpiritGathicMan>();

                if (NPC.AnyNPCs(GetNPCIndex()))
                    return false;

                int offset = -1;
                if (Main.tile[i, j].TileFrameX == 54)
                    offset = 1;
                if (Main.tile[i, j].TileFrameX == 54 + 18)
                    offset = 0;
                Vector2 pos = new Vector2(i + offset, j + 1).ToWorldCoordinates();

                if (!Main.dedServ)
                    SoundEngine.PlaySound(CustomSounds.Bell, pos);

                if (Main.netMode == NetmodeID.SinglePlayer)
                    NPC.NewNPC(new EntitySource_TileUpdate(i, j), (int)pos.X, (int)pos.Y, GetNPCIndex());
                else
                    SpawnNPCFromClient(GetNPCIndex(), pos);

                SoundEngine.PlaySound(SoundID.Item74, pos);
            }
            return true;
        }
        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            RedeTileHelper.DrawSpiritFlare(spriteBatch, i, j, 54, 1.3f, 0.9f);
            return true;
        }
        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
    }
    public class SkeletonRemainsTile7_Special : SkeletonRemainsTile7
    {
        public override string Texture => "Redemption/Tiles/Natural/SkeletonRemainsTile7";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RedeTileHelper.CannotMineTileBelow[Type] = true;
            TileID.Sets.HasOutlines[Type] = true;
            Main.tileLighted[Type] = true;
            DustType = DustID.DungeonSpirit;
            MinPick = 5000;
            MineResist = 50;
            LocalizedText name = CreateMapEntryName();
            // name.SetDefault("Skeletal Remains");
            AddMapEntry(new Color(229, 229, 195), name);
        }
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.3f;
            g = 0.3f;
            b = 0.6f;
        }
        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (Main.tile[i, j].TileFrameX == 54 && Main.tile[i, j].TileFrameY == 0)
            {
                if (Main.rand.NextBool(20))
                {
                    int d = Dust.NewDust(new Vector2(i * 16, j * 16), 48, 32, DustID.DungeonSpirit);
                    Main.dust[d].velocity.Y -= 3f;
                    Main.dust[d].noGravity = true;
                }
            }
        }
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            if (!TileDrawing.IsVisible(tile))
                return;
            Vector2 zero = new(Main.offScreenRange, Main.offScreenRange);
            if (Main.drawToScreen)
                zero = Vector2.Zero;

            int height = tile.TileFrameY == 36 ? 18 : 16;
            Main.spriteBatch.Draw(ModContent.Request<Texture2D>(Texture + "_Glow").Value, new Vector2((i * 16) - (int)Main.screenPosition.X, (j * 16) - (int)Main.screenPosition.Y) + zero, new Rectangle(tile.TileFrameX, tile.TileFrameY - 2, 16, height), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }
        public override bool CanExplode(int i, int j) => false;
        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
            if (RedeTileHelper.CanDeadRing(player))
                player.cursorItemIconID = ModContent.ItemType<DeadRinger>();
            else
            {
                player.cursorItemIconEnabled = false;
                player.cursorItemIconID = 0;
            }
        }
        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => RedeTileHelper.CanDeadRing(Main.LocalPlayer);
        public override bool RightClick(int i, int j)
        {
            Player player = Main.LocalPlayer;
            if (RedeTileHelper.CanDeadRing(player))
            {
                static int GetNPCIndex() => NPCType<SpiritDruid>();

                if (NPC.AnyNPCs(GetNPCIndex()))
                    return false;

                int offset = -1;
                if (Main.tile[i, j].TileFrameX == 54)
                    offset = 1;
                if (Main.tile[i, j].TileFrameX == 54 + 18)
                    offset = 0;
                Vector2 pos = new Vector2(i + offset, j + 1).ToWorldCoordinates();

                if (!Main.dedServ)
                    SoundEngine.PlaySound(CustomSounds.Bell, pos);

                if (Main.netMode == NetmodeID.SinglePlayer)
                    NPC.NewNPC(new EntitySource_TileUpdate(i, j), (int)pos.X, (int)pos.Y, GetNPCIndex());
                else
                    SpawnNPCFromClient(GetNPCIndex(), pos);

                SoundEngine.PlaySound(SoundID.Item74, pos);
            }
            return true;
        }
        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            RedeTileHelper.DrawSpiritFlare(spriteBatch, i, j, 54, 1.5f, 1.4f);
            return true;
        }
        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
    }
    public class SkeletonRemains1_Special : PlaceholderTile
    {
        public override string Texture => "Redemption/Tiles/Placeholder/SkeletonRemains";
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.createTile = ModContent.TileType<SkeletonRemainsTile1_Special>();
        }
    }
    public class SkeletonRemains3_Special : PlaceholderTile
    {
        public override string Texture => "Redemption/Tiles/Placeholder/SkeletonRemains";
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.createTile = ModContent.TileType<SkeletonRemainsTile3_Special>();
        }
    }
    public class SkeletonRemains4_Special : PlaceholderTile
    {
        public override string Texture => "Redemption/Tiles/Placeholder/SkeletonRemains";
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.createTile = ModContent.TileType<SkeletonRemainsTile4_Special>();
        }
    }
    public class SkeletonRemains5_Special : PlaceholderTile
    {
        public override string Texture => "Redemption/Tiles/Placeholder/SkeletonRemains";
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.createTile = ModContent.TileType<SkeletonRemainsTile5_Special>();
        }
    }
    public class SkeletonRemains7_Special : PlaceholderTile
    {
        public override string Texture => "Redemption/Tiles/Placeholder/SkeletonRemains";
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.createTile = ModContent.TileType<SkeletonRemainsTile7_Special>();
        }
    }
}