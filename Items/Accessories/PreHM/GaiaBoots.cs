using Redemption.BaseExtension;
using Redemption.Globals;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Redemption.Items.Accessories.PreHM
{
    [AutoloadEquip(EquipType.Shoes)]
    public class GaiaBoots : ModItem
    {
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(ElementID.NatureS);
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 36;
            Item.hasVanityEffects = true;
            Item.value = Item.sellPrice(0, 15, 0);
            Item.rare = ItemRarityID.Lime;
            Item.accessory = true;
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.rocketBoots = 2;
            player.vanityRocketBoots = 2;
            player.fairyBoots = true;
            player.flowerBoots = true;
            player.frogLegJumpBoost = true;
            player.buffImmune[BuffID.Webbed] = true;
            player.GetModPlayer<SpiderBoots_Player>().spiderBoots = true;
            player.RedemptionPlayerBuff().ElementalResistance[ElementID.Nature] += 0.1f;
        }
        public override void UpdateEquip(Player player)
        {
            player.accRunSpeed = 6;

            #region Flower Boots code
            if (player.whoAmI == Main.myPlayer && player.velocity.Y == 0f && player.grappling[0] == -1)
            {
                var x = (int)player.Center.X / 16;
                var y = (int)(player.position.Y + player.height - 1f) / 16;
                var tile = Main.tile[x, y];
                if (tile == null)
                {
                    tile = new Tile();
                }
                if (!tile.HasTile && tile.LiquidAmount == 0 && Main.tile[x, y + 1] != null && WorldGen.SolidTile(x, y + 1))
                {
                    tile.TileFrameY = 0;
                    tile.Get<TileWallWireStateData>().Slope = SlopeType.Solid;
                    tile.Get<TileWallWireStateData>().IsHalfBlock = false;

                    if (Main.tile[x, y + 1].TileType == TileID.Dirt)
                    {
                        if (Main.rand.NextBool(1000))
                        {
                            tile.Get<TileWallWireStateData>().HasTile = true;
                            tile.TileType = TileID.DyePlants;
                            tile.TileFrameX = (short)(34 * Main.rand.Next(0, 13));
                            while (tile.TileFrameX == 144)
                            {
                                tile.TileFrameX = (short)(34 * Main.rand.Next(0, 13));
                            }
                        }
                        if (Main.netMode == NetmodeID.MultiplayerClient)
                        {
                            NetMessage.SendTileSquare(-1, x, y, 1, TileChangeType.None);
                        }
                    }
                    if (Main.tile[x, y + 1].TileType == TileID.Grass)
                    {
                        if (Main.rand.NextBool())
                        {
                            tile.Get<TileWallWireStateData>().HasTile = true;
                            tile.TileType = TileID.Plants;
                            tile.TileFrameX = (short)(18 * Main.rand.Next(6, 11));
                            while (tile.TileFrameX == 144)
                            {
                                tile.TileFrameX = (short)(18 * Main.rand.Next(6, 11));
                            }
                        }
                        else
                        {
                            tile.Get<TileWallWireStateData>().HasTile = true;
                            tile.TileType = TileID.Plants2;
                            tile.TileFrameX = (short)(18 * Main.rand.Next(6, 21));
                            while (tile.TileFrameX == 144)
                            {
                                tile.TileFrameX = (short)(18 * Main.rand.Next(6, 21));
                            }
                        }
                        if (Main.netMode == NetmodeID.MultiplayerClient)
                        {
                            NetMessage.SendTileSquare(-1, x, y, 1, TileChangeType.None);
                        }
                    }
                    else if (Main.tile[x, y + 1].TileType == TileID.HallowedGrass)
                    {
                        if (Main.rand.NextBool())
                        {
                            tile.Get<TileWallWireStateData>().HasTile = true;
                            tile.TileType = TileID.HallowedPlants;
                            tile.TileFrameX = (short)(18 * Main.rand.Next(4, 7));
                            while (tile.TileFrameX == 90)
                            {
                                tile.TileFrameX = (short)(18 * Main.rand.Next(4, 7));
                            }
                        }
                        else
                        {
                            tile.Get<TileWallWireStateData>().HasTile = true;
                            tile.TileType = TileID.HallowedPlants2;
                            tile.TileFrameX = (short)(18 * Main.rand.Next(2, 8));
                            while (tile.TileFrameX == 90)
                            {
                                tile.TileFrameX = (short)(18 * Main.rand.Next(2, 8));
                            }
                        }
                        if (Main.netMode == NetmodeID.MultiplayerClient)
                        {
                            NetMessage.SendTileSquare(-1, x, y, 1, TileChangeType.None);
                        }
                    }
                    else if (Main.tile[x, y + 1].TileType == TileID.JungleGrass)
                    {
                        tile.Get<TileWallWireStateData>().HasTile = true;
                        tile.TileType = TileID.JunglePlants2;
                        tile.TileFrameX = (short)(18 * Main.rand.Next(9, 17));
                        if (Main.netMode == NetmodeID.MultiplayerClient)
                        {
                            NetMessage.SendTileSquare(-1, x, y, 1, TileChangeType.None);
                        }
                    }
                }
            }
            #endregion
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.FairyBoots)
                .AddIngredient(ItemID.AmphibianBoots)
                .AddIngredient<SpiderBoots>()
                .AddTile(TileID.TinkerersWorkbench)
                .Register();
        }
    }
}