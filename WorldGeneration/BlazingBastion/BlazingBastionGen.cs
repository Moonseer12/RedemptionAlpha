using Redemption.Base;
using Redemption.Globals;
using Redemption.Tiles.Tiles;
using Redemption.Walls;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace Redemption.WorldGeneration
{
    public class BlazingBastion : MicroBiome
    {
        private readonly int WIDTH = 295;
        private readonly int HEIGHT = 155;
        public override bool Place(Point origin, StructureMap structures)
        {
            Mod mod = Redemption.Instance;
            bool placed = false;
            bool nearEnd = false;
            int attempts = 0;
            Point16 bridgeDims = StructureHelper.API.Generator.GetStructureDimensions("WorldGeneration/BlazingBastion/BastionBridgeMid", mod);
            Point16 pillarDims = StructureHelper.API.Generator.GetStructureDimensions("WorldGeneration/BlazingBastion/BastionBridgePillar", mod);

            int bridgesPlaced = 0;
            while (!placed && attempts++ < Main.maxTilesX / 2)
            {
                int nearEndNum = 1100;
                int endNum = 500;
                if (Main.maxTilesX <= 5000 && RedeGen.bastionLeftSide)
                {
                    nearEndNum -= 200;
                    endNum -= 200;
                }

                int tilesX = Main.maxTilesX / 2 + (RedeGen.bastionLeftSide ? -Main.maxTilesX / 4 : Main.maxTilesX / 4) + (RedeGen.bastionLeftSide ? -attempts : attempts);
                int tilesY = Main.UnderworldLayer + 65;
                if (RedeGen.bastionLeftSide ? tilesX < endNum : tilesX > Main.maxTilesX - endNum)
                    break;

                if (!WorldGen.InWorld(tilesX, tilesY))
                    continue;

                while (!(WorldGen.SolidTile(tilesX, tilesY) || Framing.GetTileSafely(tilesX, tilesY).LiquidType == LiquidID.Lava) && tilesY <= Main.maxTilesY - 30)
                    tilesY++;

                if (tilesY > Main.maxTilesY - 30)
                    continue;

                int segNum = WorldGen.genRand.Next(3, 7);
                if (nearEnd)
                    segNum = 6;

                bool blacklist = false;
                bool noBridge = false;
                for (int x = 0; x < bridgeDims.X * segNum; x++)
                {
                    for (int y = 0; y < bridgeDims.Y; y++)
                    {
                        int type = Framing.GetTileSafely(tilesX + x, tilesY + y).TileType;
                        if (type == TileID.ObsidianBrick || type == TileID.HellstoneBrick || TileLists.BlacklistTiles.Contains(type))
                        {
                            blacklist = true;
                            break;
                        }
                        if (!WorldGen.InWorld(tilesX + x, tilesY + y) || !GenVars.structures.CanPlace(new Rectangle(tilesX, tilesY, x, y)))
                        {
                            blacklist = true;
                            break;
                        }
                        if (!nearEnd && CrossMod.CrossMod.Calamity.Enabled)
                        {
                            if (CrossMod.CrossMod.Calamity.TryFind("BrimstoneSlab", out ModTile brimstoneSlab) && type == brimstoneSlab.Type)
                                noBridge = true;
                            //if (CrossMod.CrossMod.Calamity.TryFind("BrimstoneSlag", out ModTile brimstoneSlag) && type == brimstoneSlag.Type)
                                //noBridge = true;
                            //if (CrossMod.CrossMod.Calamity.TryFind("ScorchedRemains", out ModTile scorchedRemains) && type == scorchedRemains.Type)
                                //noBridge = true;
                        }
                    }
                }
                if (blacklist)
                    continue;

                Vector2 origin2 = new(tilesX, tilesY - WorldGen.genRand.Next(2, 6));
                PlaceBridge(mod, segNum, origin2, bridgeDims, pillarDims, bridgesPlaced, noBridge);
                bridgesPlaced++;

                attempts += (bridgeDims.X * segNum) + WorldGen.genRand.Next(20, 61);
                if (!nearEnd && RedeGen.bastionLeftSide ? origin2.X < nearEndNum : origin2.X > Main.maxTilesX - nearEndNum)
                    nearEnd = true;

                if (RedeGen.bastionLeftSide ? origin2.X < endNum : origin2.X > Main.maxTilesX - endNum)
                    placed = true;
            }

            PlaceBastionGateAndBazaar(mod);

            return true;
        }

        void PlaceBastionGateAndBazaar(Mod mod)
        {
            if (midBridgeSegments == null || midBridgeSegments.Count < 2)
                return;

            Point16 gateDims = StructureHelper.API.Generator.GetStructureDimensions("WorldGeneration/BlazingBastion/BastionGate", mod);

            Point16 lastBridgeOrigin = midBridgeSegments[^1];
            int x = 17;
            int y = 5 - gateDims.Y;

            if (RedeGen.bastionLeftSide)
            {
                x -= 1 + gateDims.X;
                Point16 gateOrigin = new(lastBridgeOrigin.X + x, lastBridgeOrigin.Y + y);
                StructureHelper.API.Generator.GenerateStructure("WorldGeneration/BlazingBastion/BastionGateL", gateOrigin, mod);
            }
            else
            {
                x += 1;
                Point16 gateOrigin = new(lastBridgeOrigin.X + x, lastBridgeOrigin.Y + y);
                StructureHelper.API.Generator.GenerateStructure("WorldGeneration/BlazingBastion/BastionGate", gateOrigin, mod);
            }

            Point16 bazaarDims = StructureHelper.API.Generator.GetStructureDimensions("WorldGeneration/BlazingBastion/BastionBazaar", mod);

            Point16 bazaarOrigin;
            x = 17;
            y = 5 - 11;

            if (RedeGen.bastionLeftSide)
            {
                x -= 17 + bazaarDims.X + 18;
                bazaarOrigin = new(lastBridgeOrigin.X + x, lastBridgeOrigin.Y + y);

                WorldUtils.Gen(bazaarOrigin.ToPoint() + new Point(bazaarDims.X / 2, 9), new Shapes.Mound((bazaarDims.X / 2) + 5, 25), Actions.Chain(
                [
                    new Actions.ClearTile()
                ]));
                WorldUtils.Gen(bazaarOrigin.ToPoint() + new Point(-20, -40), new Shapes.Rectangle(bazaarDims.X + 40, 52), Actions.Chain(
                [
                    new Actions.SetLiquid(0, 0)
                ]));

                GenUtils.ClearTrees(new Point16(bazaarDims.X + 40, 52), bazaarOrigin.ToPoint() + new Point(-20, -40));

                StructureHelper.API.Generator.GenerateStructure("WorldGeneration/BlazingBastion/BastionBazaarL", bazaarOrigin, mod);
            }
            else
            {
                x += 17 + 18;
                bazaarOrigin = new(lastBridgeOrigin.X + x, lastBridgeOrigin.Y + y);

                WorldUtils.Gen(bazaarOrigin.ToPoint() + new Point(bazaarDims.X / 2, 9), new Shapes.Mound((bazaarDims.X / 2) + 5, 25), Actions.Chain(
                [
                    new Actions.ClearTile()
                ]));
                WorldUtils.Gen(bazaarOrigin.ToPoint() + new Point(-20, -40), new Shapes.Rectangle(bazaarDims.X + 40, 52), Actions.Chain(
                [
                    new Actions.SetLiquid(0, 0)
                ]));

                GenUtils.ClearTrees(new Point16(bazaarDims.X + 40, 52), bazaarOrigin.ToPoint() + new Point(-20, -40));

                StructureHelper.API.Generator.GenerateStructure("WorldGeneration/BlazingBastion/BastionBazaar", bazaarOrigin, mod);
            }

            // Fill in pillars of Bazaar
            int pillarX = RedeGen.bastionLeftSide ? 105 : 6;
            for (int i = pillarX; i < pillarX + 6; i++)
            {
                for (int j = bazaarDims.Y; j < Main.maxTilesY - 20; j++)
                {
                    Point16 pos = new(bazaarOrigin.X + i, bazaarOrigin.Y + j);
                    WorldGen.KillTile(pos.X, pos.Y);
                    WorldGen.PlaceTile(pos.X, pos.Y, TileType<DarkShinkiteBrickTile>(), true, true);
                    //WorldGen.SlopeTile(pos.X, pos.Y, 0);
                }
            }
            pillarX = RedeGen.bastionLeftSide ? 83 : 26;
            for (int i = pillarX; i < pillarX + 8; i++)
            {
                for (int j = bazaarDims.Y; j < Main.maxTilesY - 20; j++)
                {
                    Point16 pos = new(bazaarOrigin.X + i, bazaarOrigin.Y + j);
                    WorldGen.KillTile(pos.X, pos.Y);
                    WorldGen.PlaceTile(pos.X, pos.Y, TileType<DarkShinkiteBrickTile>(), true, true);
                    //WorldGen.SlopeTile(pos.X, pos.Y, 0);
                }
            }
            pillarX = RedeGen.bastionLeftSide ? 49 : 60;
            for (int i = pillarX; i < pillarX + 8; i++)
            {
                for (int j = bazaarDims.Y; j < Main.maxTilesY - 20; j++)
                {
                    Point16 pos = new(bazaarOrigin.X + i, bazaarOrigin.Y + j);
                    WorldGen.KillTile(pos.X, pos.Y);
                    WorldGen.PlaceTile(pos.X, pos.Y, TileType<DarkShinkiteBrickTile>(), true, true);
                    //WorldGen.SlopeTile(pos.X, pos.Y, 0);
                }
            }

            // Bastion
            Point16 bastionDims = StructureHelper.API.Generator.GetStructureDimensions("WorldGeneration/BlazingBastion/BlazingBastion", mod);

            Point16 bastionOrigin;
            x = 0;
            y = Main.maxTilesY - 42 - bastionDims.Y;

            if (RedeGen.bastionLeftSide)
            {
                x -= bastionDims.X + 72;
                bastionOrigin = new(bazaarOrigin.X + bazaarDims.X + x, y);

                WorldUtils.Gen(bastionOrigin.ToPoint() + new Point(0, -40), new Shapes.Rectangle(bastionDims.X, bastionDims.Y - 40), Actions.Chain(
                [
                    new Actions.SetLiquid(0, 0)
                ]));

                GenUtils.ClearTrees(new Point16(bastionDims.X, bastionDims.Y - 40), bastionOrigin.ToPoint() + new Point(0, 0));

                StructureHelper.API.Generator.GenerateStructure("WorldGeneration/BlazingBastion/BlazingBastionL", bastionOrigin, mod);
            }
            else
            {
                x += 72;
                bastionOrigin = new(bazaarOrigin.X + x, y);

                WorldUtils.Gen(bastionOrigin.ToPoint() + new Point(0, -40), new Shapes.Rectangle(bastionDims.X, bastionDims.Y - 40), Actions.Chain(
                [
                    new Actions.SetLiquid(0, 0)
                ]));

                GenUtils.ClearTrees(new Point16(bastionDims.X, bastionDims.Y - 40), bastionOrigin.ToPoint() + new Point(0, 0));

                StructureHelper.API.Generator.GenerateStructure("WorldGeneration/BlazingBastion/BlazingBastion", bastionOrigin, mod);
            }

            for (int i = 0; i < bastionDims.X; i++)
            {
                for (int j = 0; j < bastionDims.Y; j++)
                {
                    int wallX = bastionOrigin.X + i;
                    int wallY = bastionOrigin.Y + j;
                    ushort wall = Framing.GetTileSafely(wallX, wallY).WallType;

                    if (wall == WallType<ShinkiteBrickWallTile>())
                        WorldGen.ReplaceWall(wallX, wallY, (ushort)WallType<ShinkiteBrickWallTileUnsafe>());
                    if (wall == WallType<ShinkiteBrickOrnateWallTile>())
                        WorldGen.ReplaceWall(wallX, wallY, (ushort)WallType<ShinkiteBrickOrnateWallTileUnsafe>());
                }
            }

            RedeGen.BastionVector = bastionOrigin.ToVector2();
        }

        private readonly List<Point16> midBridgeSegments = new();
        void PlaceBridge(Mod mod, int segNum, Vector2 origin, Point16 bridgeDims, Point16 pillarDims, int bridgeID, bool noBridge = false)
        {
            int endNum = 500;
            if (Main.maxTilesX <= 5000 && RedeGen.bastionLeftSide)
                endNum -= 200;

            if (noBridge && bridgeID != 0)
            {
                for (int i = 0; i < segNum - 2; i++)
                {
                    if (RedeGen.bastionLeftSide ? origin.X < endNum : origin.X > Main.maxTilesX - endNum)
                        break;

                    origin.X += bridgeDims.X;
                    midBridgeSegments.Add(origin.ToPoint16());
                }
                return;
            }
            for (int i = 0; i < segNum; i++)
            {
                GenUtils.ClearTrees(new Vector2(bridgeDims.X * segNum, 24).ToPoint16(), origin.ToPoint16() - new Point16(0, 20));

                WorldUtils.Gen(origin.ToPoint() - new Point(10 - (bridgeDims.X * i), 40), new Shapes.Rectangle(bridgeDims.X + 20, 57), Actions.Chain(
                [
                        new Actions.SetLiquid(0, 0)
                ]));

                int extra = 0;
                if (i == 0)
                    extra = bridgeDims.X / 2;
                else if (i == segNum - 1)
                    extra = -bridgeDims.X / 2;
                WorldUtils.Gen(origin.ToPoint() + new Point((bridgeDims.X / 2) + (bridgeDims.X * i) + extra, 4), new Shapes.Mound(bridgeDims.X, 20), Actions.Chain(
                [
                        new Actions.ClearTile()
                ]));
                WorldUtils.Gen(origin.ToPoint() + new Point((bridgeDims.X / 2) + (bridgeDims.X * i) + extra, 4), new Shapes.Circle(bridgeDims.X, WorldGen.genRand.Next(7, 26)), Actions.Chain(
                [
                        new Actions.ClearTile()
                ]));
            }
            StructureHelper.API.MultiStructureGenerator.GenerateMultistructureRandom("WorldGeneration/BlazingBastion/BastionBridgeLeft", origin.ToPoint16(), mod);
            PlaceBridgePillar(mod, origin, bridgeDims, pillarDims);
            bool specialPlaced = false;
            Point16 specialOrigin = origin.ToPoint16();

            for (int i = 0; i < segNum - 2; i++)
            {
                if (RedeGen.bastionLeftSide ? origin.X < endNum : origin.X > Main.maxTilesX - endNum)
                    break;

                origin.X += bridgeDims.X;
                StructureHelper.API.Generator.GenerateStructure("WorldGeneration/BlazingBastion/BastionBridgeMid", origin.ToPoint16(), mod);
                PlaceBridgePillar(mod, origin, bridgeDims, pillarDims);

                midBridgeSegments.Add(origin.ToPoint16());

                if (bridgeID == 0 && !specialPlaced)
                {
                    specialOrigin = origin.ToPoint16() + new Point16(-33, -38);
                    specialPlaced = true;
                }
            }
            origin.X += bridgeDims.X;
            StructureHelper.API.MultiStructureGenerator.GenerateMultistructureRandom("WorldGeneration/BlazingBastion/BastionBridgeRight", origin.ToPoint16(), mod);
            PlaceBridgePillar(mod, origin, bridgeDims, pillarDims);

            if (bridgeID == 0)
            {
                StructureHelper.API.Generator.GenerateStructure("WorldGeneration/BlazingBastion/BastionWatchtower", specialOrigin, mod);

                RedeGen.BastionWatchtowerPoint = specialOrigin;

                if (RedeGen.bastionLeftSide)
                {
                    Point16 ballista = specialOrigin + new Point16(62, 14);
                    WorldGen.KillTile(ballista.X, ballista.Y);
                }
                else
                {
                    Point16 ballista = specialOrigin + new Point16(40, 14);
                    WorldGen.KillTile(ballista.X, ballista.Y);
                }
            }

        }

        static void PlaceBridgePillar(Mod mod, Vector2 origin, Point16 bridgeDims, Point16 pillarDims)
        {
            int pillarNum = WorldGen.genRand.Next(3, 7);
            for (int i = 0; i < pillarNum; i++)
            {
                Point16 pillarOrigin = new((int)origin.X + 15, (int)origin.Y + bridgeDims.Y + (pillarDims.Y * i));
                StructureHelper.API.Generator.GenerateStructure("WorldGeneration/BlazingBastion/BastionBridgePillar", pillarOrigin, mod);
                if (i == pillarNum - 1)
                {
                    int randX = WorldGen.genRand.Next(pillarDims.X, pillarDims.X + 5);
                    int randY = WorldGen.genRand.Next(5, 11);
                    WorldUtils.Gen(pillarOrigin.ToPoint() + new Point(pillarDims.X, pillarDims.Y) - new Point(randX / 2, randY / 2), new Shapes.Circle(randX, randY), Actions.Chain(
                    [
                            new Actions.SetSlope((int)SlopeType.Solid),
                            new Actions.PlaceTile(TileID.Ash)
                    ]));
                }
            }
            for (int i = (int)origin.X; i < origin.X + bridgeDims.X; i++)
            {
                for (int j = (int)origin.Y - 20; j < origin.Y + bridgeDims.Y; j++)
                {
                    if (WorldGen.genRand.NextBool(10))
                        WorldGen.PlacePot(i, j - 1, 28, Main.rand.Next(13, 16));
                }
            }
        }
    }
}