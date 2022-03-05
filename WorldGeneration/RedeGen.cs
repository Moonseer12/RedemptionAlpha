using Terraria.ModLoader;
using Terraria;
using Terraria.WorldBuilding;
using System.Collections.Generic;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Terraria.ID;
using Terraria.DataStructures;
using Redemption.Globals;
using Redemption.Items.Placeable.Tiles;
using Redemption.Tiles.Containers;
using Redemption.Tiles.Tiles;
using Redemption.Tiles.Plants;
using Microsoft.Xna.Framework;
using Redemption.Tiles.Ores;
using Redemption.Base;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Chat;
using Terraria.Localization;
using Redemption.Tiles.Furniture.Misc;
using Redemption.Tiles.Natural;
using Terraria.ModLoader.IO;
using System.IO;
using Redemption.NPCs.Friendly;
using Redemption.Tiles.Furniture.AncientWood;
using Redemption.Walls;
using Redemption.Tiles.Bars;
using Redemption.Items.Accessories.PreHM;
using Redemption.Items.Tools.PreHM;
using Redemption.Items.Weapons.PreHM.Melee;
using Redemption.Items.Usable.Summons;
using Redemption.Items.Materials.PreHM;
using Redemption.Items.Usable;
using Redemption.Items.Armor.Single;
using Redemption.Items.Usable.Potions;
using Redemption.Tiles.MusicBoxes;
using Redemption.Tiles.Furniture.Archcloth;
using Redemption.NPCs.Bosses.KSIII;

namespace Redemption.WorldGeneration
{
    public class RedeGen : ModSystem
    {
        public static bool dragonLeadSpawn;
        public static Vector2 newbCaveVector = new Vector2(-1, -1);
        public static Vector2 gathicPortalVector = new Vector2(-1, -1);
        public static Vector2 slayerShipVector = new Vector2(-1, -1);
        public static Vector2 HallOfHeroesVector = new Vector2(-1, -1);
        public static Vector2 LabVector = new Vector2(-1, -1);

        public override void OnWorldLoad()
        {
            if (NPC.downedBoss3)
                dragonLeadSpawn = true;
            else
                dragonLeadSpawn = false;

            newbCaveVector = new Vector2(-1, -1);
            gathicPortalVector = new Vector2(-1, -1);
            slayerShipVector = new Vector2(-1, -1);
            HallOfHeroesVector = new Vector2(-1, -1);
            LabVector = new Vector2(-1, -1);
        }

        public override void OnWorldUnload()
        {
            dragonLeadSpawn = false;
            newbCaveVector = new Vector2(-1, -1);
            gathicPortalVector = new Vector2(-1, -1);
            slayerShipVector = new Vector2(-1, -1);
            HallOfHeroesVector = new Vector2(-1, -1);
            LabVector = new Vector2(-1, -1);
        }

        public override void PostUpdateWorld()
        {
            if (NPC.downedBoss3 && !dragonLeadSpawn)
            {
                dragonLeadSpawn = true;
                if (RedeWorld.alignment >= 0)
                {
                    string status = "Crystals form in the icy caverns...";
                    if (Main.netMode == NetmodeID.Server)
                        ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(status), Color.LightBlue);
                    else if (Main.netMode == NetmodeID.SinglePlayer)
                        Main.NewText(Language.GetTextValue(status), Color.LightBlue);

                    for (int k = 0; k < (int)(Main.maxTilesX * Main.maxTilesY * 0.006f); k++)
                    {
                        int i2 = WorldGen.genRand.Next(200, Main.maxTilesX - 200);
                        int j2 = WorldGen.genRand.Next((int)WorldGen.rockLayerHigh, (int)(Main.maxTilesY * .7f));
                        int tileUp = Framing.GetTileSafely(i2, j2 - 1).TileType;
                        int tileDown = Framing.GetTileSafely(i2, j2 + 1).TileType;
                        int tileLeft = Framing.GetTileSafely(i2 - 1, j2).TileType;
                        int tileRight = Framing.GetTileSafely(i2 + 1, j2).TileType;
                        if (!Framing.GetTileSafely(i2, j2).HasTile &&
                            (tileUp == TileID.SnowBlock || tileDown == TileID.SnowBlock || tileLeft == TileID.SnowBlock || tileRight == TileID.SnowBlock || TileID.Sets.Conversion.Ice[tileUp] || TileID.Sets.Conversion.Ice[tileDown] || TileID.Sets.Conversion.Ice[tileLeft] || TileID.Sets.Conversion.Ice[tileRight]))
                        {
                            WorldGen.PlaceObject(i2, j2, ModContent.TileType<CryoCrystalTile>(), true);
                            NetMessage.SendObjectPlacment(-1, i2, j2, ModContent.TileType<CryoCrystalTile>(), 0, 0, -1, -1);
                        }
                    }

                    for (int i = 0; i < Main.maxTilesX; i++)
                    {
                        for (int j = 0; j < Main.maxTilesY; j++)
                        {
                            Tile tile = Main.tile[i, j];
                            if (tile.TileType == ModContent.TileType<DragonLeadOre2Tile>())
                                tile.TileType = TileID.Stone;
                        }
                    }
                }
                else
                {
                    string status = "The caverns are heated with dragon bone...";
                    if (Main.netMode == NetmodeID.Server)
                        ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(status), Color.Orange);
                    else if (Main.netMode == NetmodeID.SinglePlayer)
                        Main.NewText(Language.GetTextValue(status), Color.Orange);

                    for (int i = 0; i < Main.maxTilesX; i++)
                    {
                        for (int j = 0; j < Main.maxTilesY; j++)
                        {
                            Tile tile = Main.tile[i, j];
                            if (tile.TileType == ModContent.TileType<DragonLeadOre2Tile>())
                                tile.TileType = (ushort)ModContent.TileType<DragonLeadOreTile>();
                        }
                    }
                }
                if (Main.netMode == NetmodeID.Server)
                    NetMessage.SendData(MessageID.WorldData);
            }
        }

        /// <summary>
        /// Checks if the given area is more or less flattish.
        /// Returns false if the average tile height variation is greater than the threshold.
        /// Expects that the first tile is solid, and traverses from there.
        /// Use the weight parameters to change the importance of up/down checks. - Spirit Mod
        /// </summary>
        public static bool CheckFlat(int startX, int startY, int width, float threshold, int goingDownWeight = 0, int goingUpWeight = 0)
        {
            // Fail if the tile at the other end of the check plane isn't also solid
            if (!WorldGen.SolidTile(startX + width, startY)) return false;

            float totalVariance = 0;
            for (int i = 0; i < width; i++)
            {
                if (startX + i >= Main.maxTilesX) return false;

                // Fail if there is a tile very closely above the check area
                for (int k = startY - 1; k > startY - 100; k--)
                {
                    if (WorldGen.SolidTile(startX + i, k)) return false;
                }

                // If the tile is solid, go up until we find air
                // If the tile is not, go down until we find a floor
                int offset = 0;
                bool goingUp = WorldGen.SolidTile(startX + i, startY);
                offset += goingUp ? goingUpWeight : goingDownWeight;
                while ((goingUp && WorldGen.SolidTile(startX + i, startY - offset))
                    || (!goingUp && !WorldGen.SolidTile(startX + i, startY + offset)))
                {
                    offset++;
                }
                if (goingUp) offset--; // account for going up counting the first tile
                totalVariance += offset;
            }
            return totalVariance / width <= threshold;
        }

        private static void SpawnThornSummon()
        {
            bool placed1 = false;
            int attempts = 0;
            int placed2 = 0;
            int placeX2 = 0;
            while (!placed1 && attempts++ < 100000)
            {
                int placeX = Main.spawnTileX + WorldGen.genRand.Next(-600, 600);

                int placeY = (int)Main.worldSurface - 180;

                if (placeX > Main.spawnTileX - 100 && placeX < Main.spawnTileX + 100)
                    continue;
                // We go down until we hit a solid tile or go under the world's surface
                while (!WorldGen.SolidTile(placeX, placeY) && placeY <= Main.worldSurface)
                {
                    placeY++;
                }
                // If we went under the world's surface, try again
                if (placeY > Main.worldSurface)
                    continue;
                Tile tile = Main.tile[placeX, placeY];
                if (tile.TileType != TileID.Grass)
                    continue;
                if (!CheckFlat(placeX, placeY, 2, 0))
                    continue;

                WorldGen.PlaceObject(placeX, placeY - 1, ModContent.TileType<HeartOfThornsTile>(), true);
                NetMessage.SendObjectPlacment(-1, placeX, placeY - 1, (ushort)ModContent.TileType<HeartOfThornsTile>(), 0, 0, -1, -1);
                if (Main.tile[placeX, placeY - 1].TileType != ModContent.TileType<HeartOfThornsTile>())
                    continue;
                placeX2 = placeX;
                attempts = 0;
                placed1 = true;
            }
            while (placed1 && placed2 < 30 && attempts++ < 100000)
            {
                int placeX3 = placeX2 + WorldGen.genRand.Next(-20, 20);

                int placeY = (int)Main.worldSurface - 200;
                // We go down until we hit a solid tile or go under the world's surface
                while (!WorldGen.SolidTile(placeX3, placeY) && placeY <= Main.worldSurface)
                    placeY++;
                // If we went under the world's surface, try again
                if (placeY > Main.worldSurface)
                    continue;
                Tile tile = Main.tile[placeX3, placeY];
                if (tile.TileType != TileID.Grass)
                    continue;
                switch (WorldGen.genRand.Next(2))
                {
                    case 0:
                        WorldGen.PlaceObject(placeX3, placeY - 1, ModContent.TileType<ThornsTile>(), true, WorldGen.genRand.Next(2));
                        NetMessage.SendObjectPlacment(-1, placeX3, placeY - 1, (ushort)ModContent.TileType<ThornsTile>(), WorldGen.genRand.Next(2), 0, -1, -1);
                        break;
                    case 1:
                        WorldGen.PlaceObject(placeX3, placeY - 1, ModContent.TileType<ThornsTile2>(), true, WorldGen.genRand.Next(2));
                        NetMessage.SendObjectPlacment(-1, placeX3, placeY - 1, (ushort)ModContent.TileType<ThornsTile>(), WorldGen.genRand.Next(2), 0, -1, -1);
                        break;
                }
                placed2++;
            }
        }

        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight)
        {
            int ShiniesIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Shinies"));
            int ShiniesIndex2 = tasks.FindIndex(genpass => genpass.Name.Equals("Final Cleanup"));
            int GuideIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Sunflowers"));
            if (GuideIndex == -1)
                return;
            tasks.Insert(ShiniesIndex2, new PassLegacy("Heart of Thorns", delegate (GenerationProgress progress, GameConfiguration configuration)
            {
                progress.Message = "Cursing the forest";
                SpawnThornSummon();
            }));
            if (ShiniesIndex != -1)
            {
                tasks.Insert(ShiniesIndex + 2, new PassLegacy("Generating P L A N T", delegate (GenerationProgress progress, GameConfiguration configuration)
                {
                    progress.Message = "Generating P L A N T";
                    for (int k = 0; k < (int)(Main.maxTilesX * Main.maxTilesY * 6E-05); k++)
                    {
                        int x = Main.maxTilesX;
                        int y = Main.maxTilesY;
                        int tilesX = WorldGen.genRand.Next(0, x);
                        int tilesY = WorldGen.genRand.Next((int)(y * .05f), (int)(y * .3));
                        if (Main.tile[tilesX, tilesY].TileType == TileID.Dirt)
                        {
                            WorldGen.OreRunner(tilesX, tilesY, WorldGen.genRand.Next(3, 5), WorldGen.genRand.Next(4, 6), (ushort)ModContent.TileType<PlantMatterTile>());
                        }
                    }
                    for (int k = 0; k < (int)(Main.maxTilesX * Main.maxTilesY * 8E-05); k++)
                    {
                        int x = Main.maxTilesX;
                        int y = Main.maxTilesY;
                        int tilesX = WorldGen.genRand.Next(0, x);
                        int tilesY = WorldGen.genRand.Next((int)(y * .05f), (int)(y * .5));
                        if (Main.tile[tilesX, tilesY].TileType == TileID.Mud)
                        {
                            WorldGen.OreRunner(tilesX, tilesY, WorldGen.genRand.Next(3, 5), WorldGen.genRand.Next(4, 8), (ushort)ModContent.TileType<PlantMatterTile>());
                        }
                    }
                }));
                tasks.Insert(ShiniesIndex + 3, new PassLegacy("Generating Infested Stone", delegate (GenerationProgress progress, GameConfiguration configuration)
                {
                    progress.Message = "Hiding Spiders";
                    for (int k = 0; k < (int)(Main.maxTilesX * Main.maxTilesY * 5E-05); k++)
                    {
                        int i2 = WorldGen.genRand.Next(0, Main.maxTilesX);
                        int j2 = WorldGen.genRand.Next((int)(Main.maxTilesY * .4f), (int)(Main.maxTilesY * .8f));
                        WorldGen.OreRunner(i2, j2, WorldGen.genRand.Next(3, 8), WorldGen.genRand.Next(3, 8), (ushort)ModContent.TileType<InfestedStoneTile>());
                    }
                }));
                tasks.Insert(ShiniesIndex + 4, new PassLegacy("Generating Dragon Fossils", delegate (GenerationProgress progress, GameConfiguration configuration)
                {
                    #region Dragon-Lead
                    progress.Message = "Generating dragon fossils";
                    Mod mod = Redemption.Instance;
                    Dictionary<Color, int> colorToTile = new()
                    {
                        [new Color(255, 0, 0)] = ModContent.TileType<DragonLeadOre2Tile>(),
                        [new Color(150, 150, 150)] = -2, //turn into air
                        [Color.Black] = -1 //don't touch when genning
                    };
                    for (int k = 0; k < (int)(Main.maxTilesX * Main.maxTilesY * 2E-05); k++)
                    {
                        bool placed = false;
                        int attempts = 0;
                        while (!placed && attempts++ < 10000)
                        {
                            int tilesX = WorldGen.genRand.Next(12, Main.maxTilesX - 12);
                            int tilesY = WorldGen.genRand.Next((int)(Main.maxTilesY * .65f), (int)(Main.maxTilesY * .8));
                            if (!WorldGen.InWorld(tilesX, tilesY))
                                continue;

                            Texture2D tex = ModContent.Request<Texture2D>("Redemption/WorldGeneration/DL" + (WorldGen.genRand.Next(11) + 1),
                                AssetRequestMode.ImmediateLoad).Value;

                            bool whitelist = false;
                            int stoneScore = 0;
                            int emptyScore = 0;
                            for (int x = 0; x < tex.Width; x++)
                            {
                                for (int y = 0; y < tex.Height; y++)
                                {
                                    if (!WorldGen.InWorld(tilesX + x, tilesY + y) || TileLists.BlacklistTiles.Contains(Main.tile[tilesX + x, tilesY + y].TileType))
                                    {
                                        whitelist = true;
                                        break;
                                    }
                                    int type = Main.tile[tilesX + x, tilesY + y].TileType;
                                    if (type == TileID.Stone || type == TileID.Dirt)
                                        stoneScore++;
                                    else
                                        emptyScore++;
                                }
                            }
                            if (whitelist)
                                continue;
                            if (stoneScore < (int)(emptyScore * 1.5))
                                continue;

                            Point16 origin = new(tilesX, tilesY);
                            Main.QueueMainThreadAction(() =>
                            {
                                TexGen gen = BaseWorldGenTex.GetTexGenerator(tex, colorToTile);
                                gen.Generate(origin.X, origin.Y, true, true);
                            });
                            placed = true;
                        }
                    }
                    #endregion
                }));
                tasks.Insert(ShiniesIndex2 + 1, new PassLegacy("Portals", delegate (GenerationProgress progress, GameConfiguration configuration)
                {
                    #region Surface Portal
                    progress.Message = "Thinking with portals";
                    Mod mod = Redemption.Instance;
                    Dictionary<Color, int> colorToTile = new()
                    {
                        [new Color(255, 0, 0)] = TileID.Dirt,
                        [new Color(0, 255, 0)] = TileID.Grass,
                        [new Color(0, 0, 255)] = TileID.Emerald,
                        [new Color(150, 150, 150)] = -2,
                        [Color.Black] = -1
                    };

                    Dictionary<Color, int> colorToWall = new()
                    {
                        [new Color(0, 255, 0)] = WallID.DirtUnsafe3,
                        [new Color(0, 0, 255)] = WallID.DirtUnsafe1,
                        [new Color(0, 255, 255)] = WallID.GrassUnsafe,
                        [Color.Black] = -1
                    };

                    bool placed = false;
                    bool genned = false;

                    while (!genned)
                    {
                        if (placed)
                            continue;

                        int placeX = WorldGen.genRand.Next(0, Main.maxTilesX);

                        int placeY = (int)Main.worldSurface - 160;

                        if (!WorldGen.InWorld(placeX, placeY) || (placeX > Main.spawnTileX - 200 && placeX < Main.spawnTileX + 200))
                            continue;
                        // We go down until we hit a solid tile or go under the world's surface
                        while (!WorldGen.SolidTile(placeX, placeY) && placeY <= Main.worldSurface)
                        {
                            placeY++;
                        }
                        // If we went under the world's surface, try again
                        if (placeY > Main.worldSurface)
                            continue;
                        Tile tile = Main.tile[placeX, placeY];
                        if (tile.TileType != TileID.Grass)
                            continue;
                        if (!CheckFlat(placeX, placeY, 10, 2))
                            continue;

                        Texture2D tex = ModContent.Request<Texture2D>("Redemption/WorldGeneration/NewbCave", AssetRequestMode.ImmediateLoad).Value;
                        Texture2D texWall = ModContent.Request<Texture2D>("Redemption/WorldGeneration/NewbCaveWalls", AssetRequestMode.ImmediateLoad).Value;
                        Texture2D texClear = ModContent.Request<Texture2D>("Redemption/WorldGeneration/NewbCaveClear", AssetRequestMode.ImmediateLoad).Value;

                        Point origin = new(placeX - 34, placeY - 11);

                        bool whitelist = false;
                        for (int i = 0; i <= 60; i++)
                        {
                            for (int j = 0; j <= 82; j++)
                            {
                                int type = Main.tile[origin.X + i, origin.Y + j].TileType;
                                if (TileLists.BlacklistTiles.Contains(type) || type == TileID.SnowBlock || type == TileID.Sand ||
                                type == ModContent.TileType<HeartOfThornsTile>())
                                {
                                    whitelist = true;
                                    break;
                                }
                            }
                        }
                        if (whitelist)
                            continue;

                        Main.QueueMainThreadAction(() =>
                        {
                            TexGen genC = BaseWorldGenTex.GetTexGenerator(texClear, colorToTile);
                            genC.Generate(origin.X, origin.Y, true, true);

                            TexGen gen = BaseWorldGenTex.GetTexGenerator(tex, colorToTile, texWall, colorToWall);
                            gen.Generate(origin.X, origin.Y, true, true);

                            genned = true;
                        });

                        newbCaveVector = origin.ToVector2();
                        placed = true;
                    }

                    Point originPoint = newbCaveVector.ToPoint();
                    GenUtils.ObjectPlace(originPoint.X + 34, originPoint.Y + 10, (ushort)ModContent.TileType<AnglonPortalTile>());
                    GenUtils.ObjectPlace(originPoint.X + 25, originPoint.Y + 9, (ushort)ModContent.TileType<AncientWoodWorkbenchTile>());
                    GenUtils.ObjectPlace(originPoint.X + 26, originPoint.Y + 8, (ushort)ModContent.TileType<DemonScrollTile>());
                    GenUtils.ObjectPlace(originPoint.X + 34, originPoint.Y + 64, (ushort)ModContent.TileType<NewbMound>());

                    BaseWorldGen.SmoothTiles(originPoint.X, originPoint.Y, originPoint.X + 60, originPoint.Y + 82);

                    for (int i = originPoint.X; i < originPoint.X + 60; i++)
                    {
                        for (int j = originPoint.Y; j < originPoint.Y + 30; j++)
                        {
                            WorldGen.SpreadGrass(i, j);
                        }
                    }
                    for (int i = originPoint.X + 13; i < originPoint.X + 53; i++)
                    {
                        for (int j = originPoint.Y + 66; j < originPoint.Y + 74; j++)
                        {
                            if (!Framing.GetTileSafely(i, j).HasTile)
                                WorldGen.PlaceLiquid(i, j, LiquidID.Water, 255);
                        }
                    }

                    for (int i = originPoint.X; i < originPoint.X + 60; i++)
                    {
                        for (int j = originPoint.Y; j < originPoint.Y + 82; j++)
                        {
                            WorldGen.GrowTree(i, j - 1);
                            if (Main.tile[i, j].TileType == TileID.Dirt && !Framing.GetTileSafely(i, j - 1).HasTile &&
                            Framing.GetTileSafely(i, j).HasTile)
                            {
                                if (WorldGen.genRand.NextBool(3))
                                {
                                    WorldGen.PlaceObject(i, j - 1, TileID.LargePiles2, true, WorldGen.genRand.Next(47, 50));
                                    NetMessage.SendObjectPlacment(-1, i, j - 1, TileID.LargePiles2, WorldGen.genRand.Next(47, 50), 0, -1, -1);
                                }
                            }
                            if (WorldGen.genRand.NextBool(2))
                                WorldGen.PlacePot(i, j - 1);
                        }
                    }
                    #endregion
                }));
                tasks.Insert(ShiniesIndex2 + 2, new PassLegacy("Portals 2", delegate (GenerationProgress progress, GameConfiguration configuration)
                {
                    #region Underground Portal
                    progress.Message = "Thinking with portals";
                    Mod mod = Redemption.Instance;
                    Dictionary<Color, int> colorToTile = new()
                    {
                        [new Color(255, 0, 0)] = ModContent.TileType<GathicStoneBrickTile>(),
                        [new Color(200, 0, 0)] = ModContent.TileType<GathicGladestoneBrickTile>(),
                        [new Color(0, 255, 0)] = ModContent.TileType<GathicStoneTile>(),
                        [new Color(0, 200, 0)] = ModContent.TileType<GathicGladestoneTile>(),
                        [new Color(0, 0, 255)] = ModContent.TileType<AncientHallBrickTile>(),
                        [new Color(100, 90, 70)] = ModContent.TileType<AncientDirtTile>(),
                        [new Color(200, 200, 50)] = ModContent.TileType<AncientGoldCoinPileTile>(),
                        [new Color(180, 180, 150)] = TileID.AmberGemspark,
                        [new Color(150, 150, 150)] = -2,
                        [Color.Black] = -1
                    };

                    Dictionary<Color, int> colorToWall = new()
                    {
                        [new Color(0, 0, 255)] = ModContent.WallType<GathicStoneBrickWallTile>(),
                        [new Color(0, 0, 200)] = ModContent.WallType<GathicGladestoneBrickWallTile>(),
                        [new Color(255, 0, 0)] = ModContent.WallType<GathicStoneWallTile>(),
                        [new Color(200, 0, 0)] = ModContent.WallType<GathicGladestoneWallTile>(),
                        [new Color(0, 255, 0)] = ModContent.WallType<AncientHallPillarWallTile>(),
                        [new Color(150, 150, 150)] = -2,
                        [Color.Black] = -1
                    };

                    bool placed = false;
                    bool genned = false;

                    while (!genned)
                    {
                        if (placed)
                            continue;

                        int placeX = WorldGen.genRand.Next((int)(Main.maxTilesX * .35f), (int)(Main.maxTilesX * .65f));

                        int placeY = WorldGen.genRand.Next((int)(Main.maxTilesY * .4f), (int)(Main.maxTilesY * .7));

                        if (!WorldGen.InWorld(placeX, placeY))
                            continue;

                        Tile tile = Main.tile[placeX, placeY];
                        if (tile.TileType != TileID.Stone)
                            continue;

                        Texture2D tex = ModContent.Request<Texture2D>("Redemption/WorldGeneration/GathicPortal", AssetRequestMode.ImmediateLoad).Value;
                        Texture2D texWall = ModContent.Request<Texture2D>("Redemption/WorldGeneration/GathicPortalWalls", AssetRequestMode.ImmediateLoad).Value;
                        Texture2D texSlope = ModContent.Request<Texture2D>("Redemption/WorldGeneration/GathicPortalSlopes", AssetRequestMode.ImmediateLoad).Value;
                        Texture2D texClear = ModContent.Request<Texture2D>("Redemption/WorldGeneration/GathicPortalClear", AssetRequestMode.ImmediateLoad).Value;

                        Point origin = new(placeX - 46, placeY - 23);

                        bool whitelist = false;
                        for (int i = 0; i <= 88; i++)
                        {
                            for (int j = 0; j <= 47; j++)
                            {
                                int type = Main.tile[origin.X + i, origin.Y + j].TileType;
                                if (type == TileID.SnowBlock || type == TileID.Sandstone || TileLists.BlacklistTiles.Contains(type))
                                {
                                    whitelist = true;
                                    break;
                                }
                            }
                        }
                        if (whitelist)
                            continue;

                        WorldUtils.Gen(origin, new Shapes.Rectangle(88, 47), Actions.Chain(new GenAction[]
                        {
                            new Actions.SetLiquid(0, 0)
                        }));

                        Main.QueueMainThreadAction(() =>
                        {
                            TexGen genC = BaseWorldGenTex.GetTexGenerator(texClear, colorToTile);
                            genC.Generate(origin.X, origin.Y, true, true);

                            TexGen gen = BaseWorldGenTex.GetTexGenerator(tex, colorToTile, texWall, colorToWall, null, texSlope);
                            gen.Generate(origin.X, origin.Y, true, true);

                            genned = true;
                        });

                        gathicPortalVector = origin.ToVector2();
                        placed = true;
                    }

                    Point originPoint = gathicPortalVector.ToPoint();
                    GenUtils.ObjectPlace(originPoint.X + 45, originPoint.Y + 21, (ushort)ModContent.TileType<GathuramPortalTile>());
                    GenUtils.ObjectPlace(originPoint.X + 16, originPoint.Y + 22, (ushort)ModContent.TileType<AncientWoodTableTile>());
                    GenUtils.ObjectPlace(originPoint.X + 18, originPoint.Y + 22, (ushort)ModContent.TileType<AncientWoodChairTile>());
                    GenUtils.ObjectPlace(originPoint.X + 12, originPoint.Y + 22, (ushort)ModContent.TileType<AncientWoodDoorClosed>());
                    GenUtils.ObjectPlace(originPoint.X + 21, originPoint.Y + 22, (ushort)ModContent.TileType<AncientWoodDoorClosed>());
                    GenUtils.ObjectPlace(originPoint.X + 69, originPoint.Y + 22, (ushort)ModContent.TileType<AncientWoodDoorClosed>());
                    GenUtils.ObjectPlace(originPoint.X + 78, originPoint.Y + 22, (ushort)ModContent.TileType<AncientWoodDoorClosed>());
                    GenUtils.ObjectPlace(originPoint.X + 71, originPoint.Y + 22, (ushort)ModContent.TileType<AncientWoodClockTile>());

                    AncientWoodChest(originPoint.X + 73, originPoint.Y + 22, 2);
                    AncientWoodChest(originPoint.X + 62, originPoint.Y + 36);

                    for (int i = originPoint.X; i < originPoint.X + 88; i++)
                    {
                        for (int j = originPoint.Y; j < originPoint.Y + 47; j++)
                        {
                            switch (Main.tile[i, j].TileType)
                            {
                                case TileID.AmberGemspark:
                                    Main.tile[i, j].ClearTile();
                                    WorldGen.PlaceObject(i, j, (ushort)ModContent.TileType<GraveSteelAlloyTile>(), true);
                                    NetMessage.SendObjectPlacment(-1, i, j, (ushort)ModContent.TileType<GraveSteelAlloyTile>(), 0, 0, -1, -1);
                                    break;
                            }
                            if (WorldGen.genRand.NextBool(3))
                                WorldGen.PlacePot(i, j - 1);
                        }
                    }
                    #endregion
                }));
                tasks.Insert(ShiniesIndex2 + 3, new PassLegacy("Ancient Hut", delegate (GenerationProgress progress, GameConfiguration configuration)
                {
                    #region Ancient Hut
                    Mod mod = Redemption.Instance;
                    Dictionary<Color, int> colorToTile = new()
                    {
                        [new Color(255, 0, 0)] = ModContent.TileType<GathicStoneBrickTile>(),
                        [new Color(200, 0, 0)] = ModContent.TileType<GathicGladestoneBrickTile>(),
                        [new Color(0, 255, 0)] = ModContent.TileType<GathicStoneTile>(),
                        [new Color(0, 200, 0)] = ModContent.TileType<GathicGladestoneTile>(),
                        [new Color(100, 80, 80)] = ModContent.TileType<AncientWoodTile>(),
                        [new Color(100, 90, 70)] = ModContent.TileType<AncientDirtTile>(),
                        [new Color(0, 255, 255)] = TileID.AmethystGemspark,
                        [new Color(0, 0, 255)] = TileID.DiamondGemspark,
                        [new Color(180, 180, 150)] = TileID.AmberGemspark,
                        [new Color(150, 150, 150)] = -2,
                        [Color.Black] = -1
                    };

                    Dictionary<Color, int> colorToWall = new()
                    {
                        [new Color(0, 0, 255)] = ModContent.WallType<GathicStoneBrickWallTile>(),
                        [new Color(0, 0, 200)] = ModContent.WallType<GathicGladestoneBrickWallTile>(),
                        [new Color(255, 0, 0)] = ModContent.WallType<GathicStoneWallTile>(),
                        [new Color(200, 0, 0)] = ModContent.WallType<GathicGladestoneWallTile>(),
                        [new Color(0, 255, 0)] = ModContent.WallType<AncientWoodWallTile>(),
                        [new Color(150, 150, 150)] = -2,
                        [Color.Black] = -1
                    };

                    bool placed = false;
                    bool genned = false;
                    Point origin = Point.Zero;

                    while (!genned)
                    {
                        if (placed)
                            continue;

                        int placeX = WorldGen.genRand.Next(50, Main.maxTilesX - 50);

                        int placeY = WorldGen.genRand.Next((int)(Main.maxTilesY * .4f), (int)(Main.maxTilesY * .7));

                        if (!WorldGen.InWorld(placeX, placeY))
                            continue;

                        Tile tile = Main.tile[placeX, placeY];
                        if (tile.TileType != TileID.Stone)
                            continue;

                        Texture2D tex = ModContent.Request<Texture2D>("Redemption/WorldGeneration/AncientHutTiles", AssetRequestMode.ImmediateLoad).Value;
                        Texture2D texWall = ModContent.Request<Texture2D>("Redemption/WorldGeneration/AncientHutWalls", AssetRequestMode.ImmediateLoad).Value;
                        Texture2D texSlope = ModContent.Request<Texture2D>("Redemption/WorldGeneration/AncientHutSlopes", AssetRequestMode.ImmediateLoad).Value;
                        Texture2D texClear = ModContent.Request<Texture2D>("Redemption/WorldGeneration/AncientHutClear", AssetRequestMode.ImmediateLoad).Value;

                        origin = new(placeX - 15, placeY - 11);

                        bool whitelist = false;
                        for (int i = 0; i <= 29; i++)
                        {
                            for (int j = 0; j <= 21; j++)
                            {
                                int type = Main.tile[origin.X + i, origin.Y + j].TileType;
                                if (type == TileID.SnowBlock || type == TileID.Sandstone || TileLists.BlacklistTiles.Contains(type))
                                {
                                    whitelist = true;
                                    break;
                                }
                            }
                        }
                        if (whitelist)
                            continue;

                        WorldUtils.Gen(origin, new Shapes.Rectangle(29, 21), Actions.Chain(new GenAction[]
                        {
                            new Actions.SetLiquid(0, 0)
                        }));

                        Main.QueueMainThreadAction(() =>
                        {
                            TexGen genC = BaseWorldGenTex.GetTexGenerator(texClear, colorToTile);
                            genC.Generate(origin.X, origin.Y, true, true);

                            TexGen gen = BaseWorldGenTex.GetTexGenerator(tex, colorToTile, texWall, colorToWall, null, texSlope);
                            gen.Generate(origin.X, origin.Y, true, true);

                            genned = true;
                        });

                        placed = true;
                    }

                    GenUtils.ObjectPlace(origin.X + 17, origin.Y + 11, (ushort)ModContent.TileType<AncientWoodDoorClosed>());
                    GenUtils.ObjectPlace(origin.X + 26, origin.Y + 12, (ushort)ModContent.TileType<AncientWoodDoorClosed>());
                    GenUtils.ObjectPlace(origin.X + 10, origin.Y + 3, (ushort)ModContent.TileType<AncientWoodBedTile>());
                    GenUtils.ObjectPlace(origin.X + 6, origin.Y + 13, (ushort)ModContent.TileType<AncientWoodClockTile>());
                    GenUtils.ObjectPlace(origin.X + 22, origin.Y + 12, (ushort)ModContent.TileType<AncientWoodTableTile>());
                    GenUtils.ObjectPlace(origin.X + 20, origin.Y + 12, (ushort)ModContent.TileType<AncientWoodChairTile>(), 0, 1);
                    GenUtils.ObjectPlace(origin.X + 14, origin.Y + 17, (ushort)ModContent.TileType<DoppelsSwordTile>());

                    AncientWoodChest(origin.X + 4, origin.Y + 13, 1);

                    for (int i = origin.X; i < origin.X + 88; i++)
                    {
                        for (int j = origin.Y; j < origin.Y + 47; j++)
                        {
                            switch (Main.tile[i, j].TileType)
                            {
                                case TileID.AmberGemspark:
                                    Main.tile[i, j].ClearTile();
                                    WorldGen.PlaceObject(i, j, (ushort)ModContent.TileType<GraveSteelAlloyTile>(), true);
                                    NetMessage.SendObjectPlacment(-1, i, j, (ushort)ModContent.TileType<GraveSteelAlloyTile>(), 0, 0, -1, -1);
                                    break;
                                case TileID.DiamondGemspark:
                                    Main.tile[i, j].ClearTile();
                                    WorldGen.PlaceTile(i, j, ModContent.TileType<AncientWoodPlatformTile>(), true, false, -1, 0);
                                    WorldGen.SlopeTile(i, j, 2);
                                    break;
                                case TileID.AmethystGemspark:
                                    Main.tile[i, j].ClearTile();
                                    WorldGen.PlaceTile(i, j, ModContent.TileType<AncientWoodPlatformTile>(), true, false, -1, 0);
                                    break;
                            }
                            if (WorldGen.genRand.NextBool(3))
                                WorldGen.PlacePot(i, j - 1);
                        }
                    }
                    #endregion
                }));
                tasks.Insert(ShiniesIndex2 + 4, new PassLegacy("Hall of Heroes", delegate (GenerationProgress progress, GameConfiguration configuration)
                {
                    #region Hall of Heroes
                    progress.Message = "Unearthing Halls";
                    Mod mod = Redemption.Instance;
                    Dictionary<Color, int> colorToTile = new()
                    {
                        [new Color(255, 0, 0)] = ModContent.TileType<GathicStoneBrickTile>(),
                        [new Color(200, 0, 0)] = ModContent.TileType<GathicGladestoneBrickTile>(),
                        [new Color(0, 0, 255)] = ModContent.TileType<AncientHallBrickTile>(),
                        [new Color(100, 80, 80)] = ModContent.TileType<AncientWoodTile>(),
                        [new Color(200, 200, 50)] = ModContent.TileType<AncientGoldCoinPileTile>(),
                        [new Color(200, 200, 200)] = TileID.Cobweb,
                        [new Color(0, 255, 0)] = TileID.AmberGemspark,
                        [new Color(255, 255, 0)] = TileID.AmethystGemspark,
                        [new Color(0, 255, 255)] = TileID.DiamondGemspark,
                        [new Color(150, 150, 150)] = -2,
                        [Color.Black] = -1
                    };

                    Dictionary<Color, int> colorToWall = new()
                    {
                        [new Color(0, 255, 0)] = ModContent.WallType<AncientHallPillarWallTile>(),
                        [new Color(150, 150, 150)] = -2,
                        [Color.Black] = -1
                    };

                    bool placed = false;
                    bool genned = false;

                    while (!genned)
                    {
                        if (placed)
                            continue;

                        int placeX2 = WorldGen.genRand.Next((int)(Main.maxTilesX * .35f), (int)(Main.maxTilesX * .65f));

                        int placeY2 = WorldGen.genRand.Next((int)(Main.maxTilesY * .4f), (int)(Main.maxTilesY * .6));

                        if (!WorldGen.InWorld(placeX2, placeY2))
                            continue;

                        Tile tile = Main.tile[placeX2, placeY2];
                        if (tile.TileType != TileID.Stone)
                            continue;

                        Texture2D tex = ModContent.Request<Texture2D>("Redemption/WorldGeneration/HallOfHeroesTiles", AssetRequestMode.ImmediateLoad).Value;
                        Texture2D texWall = ModContent.Request<Texture2D>("Redemption/WorldGeneration/HallOfHeroesWalls", AssetRequestMode.ImmediateLoad).Value;
                        Texture2D texSlope = ModContent.Request<Texture2D>("Redemption/WorldGeneration/HallOfHeroesSlopes", AssetRequestMode.ImmediateLoad).Value;
                        Texture2D texClear = ModContent.Request<Texture2D>("Redemption/WorldGeneration/HallOfHeroesClear", AssetRequestMode.ImmediateLoad).Value;

                        Point origin2 = new(placeX2 - 40, placeY2 - 27);

                        bool whitelist = false;
                        for (int i = 0; i <= 88; i++)
                        {
                            for (int j = 0; j <= 47; j++)
                            {
                                int type = Main.tile[origin2.X + i, origin2.Y + j].TileType;
                                if (TileLists.BlacklistTiles.Contains(type))
                                {
                                    whitelist = true;
                                    break;
                                }
                            }
                        }
                        if (whitelist)
                            continue;

                        WorldUtils.Gen(origin2, new Shapes.Rectangle(84, 43), Actions.Chain(new GenAction[]
                        {
                            new Actions.SetLiquid(0, 0)
                        }));

                        Main.QueueMainThreadAction(() =>
                        {
                            TexGen genC = BaseWorldGenTex.GetTexGenerator(texClear, colorToTile);
                            genC.Generate(origin2.X, origin2.Y, true, true);

                            TexGen gen = BaseWorldGenTex.GetTexGenerator(tex, colorToTile, texWall, colorToWall, null, texSlope);
                            gen.Generate(origin2.X, origin2.Y, true, true);

                            genned = true;
                        });

                        HallOfHeroesVector = origin2.ToVector2();
                        placed = true;
                    }

                    Point HallPoint = HallOfHeroesVector.ToPoint();
                    GenUtils.ObjectPlace(HallPoint.X + 24, HallPoint.Y + 24, (ushort)ModContent.TileType<KSStatueTile>());
                    GenUtils.ObjectPlace(HallPoint.X + 54, HallPoint.Y + 24, (ushort)ModContent.TileType<NStatueTile>());
                    GenUtils.ObjectPlace(HallPoint.X + 43, HallPoint.Y + 20, (ushort)ModContent.TileType<JStatueTile>());
                    GenUtils.ObjectPlace(HallPoint.X + 35, HallPoint.Y + 20, (ushort)ModContent.TileType<HKStatueTile>());
                    GenUtils.ObjectPlace(HallPoint.X + 39, HallPoint.Y + 16, (ushort)ModContent.TileType<HallOfHeroesBoxTile>());
                    GenUtils.ObjectPlace(HallPoint.X + 39, HallPoint.Y + 27, (ushort)ModContent.TileType<AncientAltarTile>());
                    GenUtils.ObjectPlace(HallPoint.X + 59, HallPoint.Y + 13, (ushort)ModContent.TileType<ArchclothBannerTile>());
                    GenUtils.ObjectPlace(HallPoint.X + 20, HallPoint.Y + 13, (ushort)ModContent.TileType<ArchclothBannerTile>());
                    GenUtils.ObjectPlace(HallPoint.X + 49, HallPoint.Y + 13, (ushort)ModContent.TileType<ArchclothBannerTile>());
                    GenUtils.ObjectPlace(HallPoint.X + 30, HallPoint.Y + 13, (ushort)ModContent.TileType<ArchclothBannerTile>());

                    AncientWoodChest(HallPoint.X + 2, HallPoint.Y + 30);
                    AncientWoodChest(HallPoint.X + 75, HallPoint.Y + 30);

                    for (int i = HallPoint.X; i < HallPoint.X + 88; i++)
                    {
                        for (int j = HallPoint.Y; j < HallPoint.Y + 47; j++)
                        {
                            switch (Main.tile[i, j].TileType)
                            {
                                case TileID.AmberGemspark:
                                    Main.tile[i, j].ClearTile();
                                    WorldGen.PlaceTile(i, j, ModContent.TileType<AncientWoodPlatformTile>(), true, false, -1, 0);
                                    WorldGen.SlopeTile(i, j, 2);
                                    break;
                                case TileID.AmethystGemspark:
                                    Main.tile[i, j].ClearTile();
                                    WorldGen.PlaceTile(i, j, ModContent.TileType<AncientWoodPlatformTile>(), true, false, -1, 0);
                                    WorldGen.SlopeTile(i, j, 1);
                                    break;
                                case TileID.DiamondGemspark:
                                    Main.tile[i, j].ClearTile();
                                    WorldGen.PlaceTile(i, j, ModContent.TileType<AncientWoodPlatformTile>(), true, false, -1, 0);
                                    break;
                            }
                            if (WorldGen.genRand.NextBool(3))
                                WorldGen.PlacePot(i, j - 1);
                        }
                    }
                    #endregion
                }));
                tasks.Insert(ShiniesIndex2 + 5, new PassLegacy("Tied Lair", delegate (GenerationProgress progress, GameConfiguration configuration)
                {
                    #region Tied Lair
                    Mod mod = Redemption.Instance;
                    Dictionary<Color, int> colorToTile = new()
                    {
                        [new Color(255, 0, 0)] = ModContent.TileType<GathicStoneBrickTile>(),
                        [new Color(200, 0, 0)] = ModContent.TileType<GathicGladestoneBrickTile>(),
                        [new Color(0, 255, 0)] = ModContent.TileType<GathicStoneTile>(),
                        [new Color(0, 200, 0)] = ModContent.TileType<GathicGladestoneTile>(),
                        [new Color(0, 0, 255)] = TileID.BoneBlock,
                        [new Color(150, 150, 150)] = -2,
                        [Color.Black] = -1
                    };

                    Dictionary<Color, int> colorToWall = new()
                    {
                        [new Color(0, 0, 255)] = ModContent.WallType<GathicStoneBrickWallTile>(),
                        [new Color(0, 0, 200)] = ModContent.WallType<GathicGladestoneBrickWallTile>(),
                        [new Color(255, 0, 0)] = ModContent.WallType<GathicStoneWallTile>(),
                        [new Color(200, 0, 0)] = ModContent.WallType<GathicGladestoneWallTile>(),
                        [new Color(150, 150, 150)] = -2,
                        [Color.Black] = -1
                    };

                    bool placed = false;
                    bool genned = false;
                    Point origin = Point.Zero;

                    while (!genned)
                    {
                        if (placed)
                            continue;

                        int placeX = WorldGen.genRand.Next(50, Main.maxTilesX - 50);

                        int placeY = WorldGen.genRand.Next((int)(Main.maxTilesY * .5f), (int)(Main.maxTilesY * .7));

                        if (!WorldGen.InWorld(placeX, placeY))
                            continue;

                        Tile tile = Main.tile[placeX, placeY];
                        if (tile.TileType != TileID.Stone)
                            continue;

                        Texture2D tex = ModContent.Request<Texture2D>("Redemption/WorldGeneration/TiedLairTiles", AssetRequestMode.ImmediateLoad).Value;
                        Texture2D texWall = ModContent.Request<Texture2D>("Redemption/WorldGeneration/TiedLairWalls", AssetRequestMode.ImmediateLoad).Value;
                        Texture2D texClear = ModContent.Request<Texture2D>("Redemption/WorldGeneration/TiedLairClear", AssetRequestMode.ImmediateLoad).Value;

                        origin = new(placeX - 10, placeY - 11);

                        bool whitelist = false;
                        for (int i = 0; i <= 19; i++)
                        {
                            for (int j = 0; j <= 16; j++)
                            {
                                int type = Main.tile[origin.X + i, origin.Y + j].TileType;
                                if (type == TileID.SnowBlock || type == TileID.Sandstone || TileLists.BlacklistTiles.Contains(type))
                                {
                                    whitelist = true;
                                    break;
                                }
                            }
                        }
                        if (whitelist)
                            continue;

                        WorldUtils.Gen(origin, new Shapes.Rectangle(19, 16), Actions.Chain(new GenAction[]
                        {
                            new Actions.SetLiquid(0, 0)
                        }));

                        Main.QueueMainThreadAction(() =>
                        {
                            TexGen genC = BaseWorldGenTex.GetTexGenerator(texClear, colorToTile);
                            genC.Generate(origin.X, origin.Y, true, true);

                            TexGen gen = BaseWorldGenTex.GetTexGenerator(tex, colorToTile, texWall, colorToWall);
                            gen.Generate(origin.X, origin.Y, true, true);

                            genned = true;
                        });

                        placed = true;
                    }

                    GenUtils.ObjectPlace(origin.X + 9, origin.Y + 10, TileID.Campfire, 7);
                    GenUtils.ObjectPlace(origin.X + 9, origin.Y + 5, (ushort)ModContent.TileType<HangingTiedTile>());
                    #endregion
                }));
                tasks.Insert(ShiniesIndex2 + 1, new PassLegacy("Clearing Liquids for Ship", delegate (GenerationProgress progress, GameConfiguration configuration)
                {
                    Point origin = new((int)(Main.maxTilesX * 0.65f), (int)Main.worldSurface - 180);
                    if (Main.dungeonX < Main.maxTilesX / 2)
                        origin = new Point((int)(Main.maxTilesX * 0.35f), (int)Main.worldSurface - 180);

                    origin.Y = BaseWorldGen.GetFirstTileFloor(origin.X, origin.Y, true);
                    origin.X -= 60;

                    WorldUtils.Gen(origin, new Shapes.Rectangle(80, 50), Actions.Chain(new GenAction[]
                    {
                        new Actions.SetLiquid(0, 0)
                    }));
                    slayerShipVector = origin.ToVector2();

                    SlayerShipClear delete = new();
                    SlayerShip biome = new();
                    delete.Place(origin, WorldGen.structures);
                    biome.Place(origin, WorldGen.structures);
                }));
                tasks.Insert(ShiniesIndex2 + 7, new PassLegacy("Slayer's Crashed Spaceship", delegate (GenerationProgress progress, GameConfiguration configuration)
                {
                    Point origin = slayerShipVector.ToPoint();
                    SlayerShipDeco deco = new();
                    deco.Place(origin, WorldGen.structures);
                }));
                tasks.Insert(ShiniesIndex2, new PassLegacy("Abandoned Lab", delegate (GenerationProgress progress, GameConfiguration configuration)
                {
                    progress.Message = "Placing the Abandoned Lab in the island which is not\nactually canonically meant to be there but that'll change in 0.9";
                    Point origin = new((int)(Main.maxTilesX * 0.55f), (int)(Main.maxTilesY * 0.65f));
                    WorldUtils.Gen(origin, new Shapes.Rectangle(289, 217), Actions.Chain(new GenAction[]
                    {
                        new Actions.SetLiquid(0, 0)
                    }));
                    LabVector = origin.ToVector2();

                    AbandonedLab biome = new();
                    LabClear delete = new();
                    delete.Place(origin, WorldGen.structures);
                    biome.Place(origin, WorldGen.structures);
                }));
            }

        }
        public static void AncientWoodChest(int x, int y, int ID = 0)
        {
            int PlacementSuccess = WorldGen.PlaceChest(x, y, (ushort)ModContent.TileType<AncientWoodChestTile>(), false);

            int[] ChestLoot = new int[] {
                ModContent.ItemType<PouchBelt>(), ModContent.ItemType<RopeHook>(), ModContent.ItemType<BeardedHatchet>(), ModContent.ItemType<WeddingRing>() };
            int[] ChestLoot2 = new int[] {
                ModContent.ItemType<ZweihanderFragment1>(), ModContent.ItemType<ZweihanderFragment2>() };
            int[] ChestLoot3 = new int[] {
                ItemID.MiningPotion, ItemID.BattlePotion, ItemID.BuilderPotion, ItemID.InvisibilityPotion };
            int[] ChestLoot4 = new int[] {
                ItemID.SpelunkerPotion, ItemID.StrangeBrew, ItemID.RecallPotion, ModContent.ItemType<VendettaPotion>(), };

            if (PlacementSuccess >= 0)
            {
                int slot = 0;
                Chest chest = Main.chest[PlacementSuccess];

                if (ID == 1)
                    chest.item[slot].SetDefaults(ModContent.ItemType<ForgottenSword>());
                else if (ID == 2)
                    chest.item[slot].SetDefaults(ModContent.ItemType<DeadRinger>());
                else
                    chest.item[slot].SetDefaults(Utils.Next(WorldGen.genRand, ChestLoot));
                chest.item[slot++].stack = 1;

                if (RedeHelper.GenChance(.6f))
                {
                    chest.item[slot].SetDefaults(ModContent.ItemType<GraveSteelAlloy>());
                    chest.item[slot++].stack = WorldGen.genRand.Next(4, 10);
                }
                if (RedeHelper.GenChance(.6f))
                {
                    chest.item[slot].SetDefaults(ModContent.ItemType<AncientWood>());
                    chest.item[slot++].stack = WorldGen.genRand.Next(5, 15);
                }
                if (RedeHelper.GenChance(.6f))
                {
                    chest.item[slot].SetDefaults(ModContent.ItemType<AncientGoldCoin>());
                    chest.item[slot++].stack = WorldGen.genRand.Next(3, 16);
                }
                if (RedeHelper.GenChance(.2f))
                {
                    chest.item[slot].SetDefaults(ModContent.ItemType<Archcloth>());
                    chest.item[slot++].stack = WorldGen.genRand.Next(1, 2);
                }
                if (RedeHelper.GenChance(.1f))
                {
                    chest.item[slot].SetDefaults(Utils.Next(WorldGen.genRand, ChestLoot2));
                    chest.item[slot++].stack = 1;
                }
                if (RedeHelper.GenChance(.02f))
                {
                    chest.item[slot].SetDefaults(ModContent.ItemType<JollyHelm>());
                    chest.item[slot++].stack = 1;
                }
                if (RedeHelper.GenChance(.66f))
                {
                    chest.item[slot].SetDefaults(Utils.Next(WorldGen.genRand, ChestLoot3));
                    chest.item[slot++].stack = WorldGen.genRand.Next(1, 2);
                }
                if (RedeHelper.GenChance(.33f))
                {
                    chest.item[slot].SetDefaults(Utils.Next(WorldGen.genRand, ChestLoot4));
                    chest.item[slot++].stack = WorldGen.genRand.Next(1, 2);
                }
                chest.item[slot].SetDefaults(ItemID.SilverCoin);
                chest.item[slot++].stack = WorldGen.genRand.Next(5, 10);
            }
        }

        public override void PreUpdateWorld()
        {
            Vector2 anglonPortalPos = new(((newbCaveVector.X + 35) * 16) - 8, ((newbCaveVector.Y + 12) * 16) - 4);
            if (newbCaveVector.X != -1 && newbCaveVector.Y != -1 && Main.LocalPlayer.DistanceSQ(anglonPortalPos) < 2000 * 2000 &&
                !NPC.AnyNPCs(ModContent.NPCType<AnglonPortal>()))
            {
                NPC.NewNPC(new EntitySource_SpawnNPC(), (int)anglonPortalPos.X, (int)anglonPortalPos.Y, ModContent.NPCType<AnglonPortal>());
            }
            Vector2 gathicPortalPos = new(((gathicPortalVector.X + 46) * 16) - 8, ((gathicPortalVector.Y + 23) * 16) - 4);
            if (gathicPortalVector.X != -1 && gathicPortalVector.Y != -1 && Main.LocalPlayer.DistanceSQ(gathicPortalPos) < 2000 * 2000 &&
                !NPC.AnyNPCs(ModContent.NPCType<GathuramPortal>()))
            {
                NPC.NewNPC(new EntitySource_SpawnNPC(), (int)gathicPortalPos.X, (int)gathicPortalPos.Y, ModContent.NPCType<GathuramPortal>());
            }
            Vector2 slayerSittingPos = new((slayerShipVector.X + 92) * 16, (slayerShipVector.Y + 28) * 16);
            if (slayerShipVector.X != -1 && slayerShipVector.Y != -1 && RedeBossDowned.downedSlayer && !RedeBossDowned.downedVlitch3 &&
                Main.LocalPlayer.DistanceSQ(slayerSittingPos) < 2000 * 2000 && !NPC.AnyNPCs(ModContent.NPCType<KS3Sitting>()) && !NPC.AnyNPCs(ModContent.NPCType<KS3>()))
            {
                NPC.NewNPC(new EntitySource_SpawnNPC(), (int)slayerSittingPos.X, (int)slayerSittingPos.Y, ModContent.NPCType<KS3Sitting>());
            }
        }

        public override void SaveWorldData(TagCompound tag)
        {
            tag["newbCaveVectorX"] = newbCaveVector.X;
            tag["newbCaveVectorY"] = newbCaveVector.Y;
            tag["gathicPortalVectorX"] = gathicPortalVector.X;
            tag["gathicPortalVectorY"] = gathicPortalVector.Y;
            tag["slayerShipVectorX"] = slayerShipVector.X;
            tag["slayerShipVectorY"] = slayerShipVector.Y;
            tag["HallOfHeroesVectorX"] = HallOfHeroesVector.X;
            tag["HallOfHeroesVectorY"] = HallOfHeroesVector.Y;
            tag["LabVectorX"] = LabVector.X;
            tag["LabVectorY"] = LabVector.Y;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            newbCaveVector.X = tag.GetFloat("newbCaveVectorX");
            newbCaveVector.Y = tag.GetFloat("newbCaveVectorY");
            gathicPortalVector.X = tag.GetFloat("gathicPortalVectorX");
            gathicPortalVector.Y = tag.GetFloat("gathicPortalVectorY");
            slayerShipVector.X = tag.GetFloat("slayerShipVectorX");
            slayerShipVector.Y = tag.GetFloat("slayerShipVectorY");
            HallOfHeroesVector.X = tag.GetFloat("HallOfHeroesVectorX");
            HallOfHeroesVector.Y = tag.GetFloat("HallOfHeroesVectorY");
            LabVector.X = tag.GetFloat("LabVectorX");
            LabVector.Y = tag.GetFloat("LabVectorY");
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.WritePackedVector2(newbCaveVector);
            writer.WritePackedVector2(gathicPortalVector);
            writer.WritePackedVector2(slayerShipVector);
            writer.WritePackedVector2(HallOfHeroesVector);
            writer.WritePackedVector2(LabVector);
        }
        public override void NetReceive(BinaryReader reader)
        {
            newbCaveVector = reader.ReadPackedVector2();
            gathicPortalVector = reader.ReadPackedVector2();
            slayerShipVector = reader.ReadPackedVector2();
            HallOfHeroesVector = reader.ReadPackedVector2();
            LabVector = reader.ReadPackedVector2();
        }
    }
    public class GenUtils
    {
        public static void ObjectPlace(Point Origin, int x, int y, int TileType, int style = 0, int direction = -1)
        {
            WorldGen.PlaceObject(Origin.X + x, Origin.Y + y, TileType, true, style, 0, -1, direction);
            NetMessage.SendObjectPlacment(-1, Origin.X + x, Origin.Y + y, TileType, style, 0, -1, direction);
        }
        public static void ObjectPlace(int x, int y, int TileType, int style = 0, int direction = -1)
        {
            WorldGen.PlaceObject(x, y, TileType, true, style, 0, -1, direction);
            NetMessage.SendObjectPlacment(-1, x, y, TileType, style, 0, -1, direction);
        }
    }
}