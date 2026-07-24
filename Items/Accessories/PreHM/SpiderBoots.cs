using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Items.Accessories.PreHM
{
    [AutoloadEquip(EquipType.Shoes)]
    public class SpiderBoots : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 30;
            Item.value = Item.sellPrice(0, 0, 50);
            Item.rare = ItemRarityID.Green;
            Item.accessory = true;
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.buffImmune[BuffID.Webbed] = true;
            player.GetModPlayer<SpiderBoots_Player>().spiderBoots = true;
        }
    }
    public class SpiderBoots_System : ModSystem
    {
        public override void Load()
        {
            On_Player.StickyMovement += On_Player_StickyMovement;
            On_Collision.SwitchTiles_Entity_Vector2_int_int_Vector2_int += On_Collision_SwitchTiles_Entity_Vector2_int_int_Vector2_int;
        }
        public override void Unload()
        {
            On_Player.StickyMovement -= On_Player_StickyMovement;
            On_Collision.SwitchTiles_Entity_Vector2_int_int_Vector2_int -= On_Collision_SwitchTiles_Entity_Vector2_int_int_Vector2_int;
        }
        private bool On_Collision_SwitchTiles_Entity_Vector2_int_int_Vector2_int(On_Collision.orig_SwitchTiles_Entity_Vector2_int_int_Vector2_int orig, Entity entity, Vector2 Position, int Width, int Height, Vector2 oldPosition, int objType)
        {
            if (entity is Player player && player.GetModPlayer<SpiderBoots_Player>().spiderBoots)
            {
                if (Main.rand.NextBool())
                    return orig.Invoke(entity, Position, Width, Height, oldPosition, objType);

                return false;
            }
            return orig.Invoke(entity, Position, Width, Height, oldPosition, objType);
        }
        private void On_Player_StickyMovement(On_Player.orig_StickyMovement orig, Player self)
        {
            if (self.GetModPlayer<SpiderBoots_Player>().spiderBoots && TouchingCobweb(self))
                return;
            orig.Invoke(self);
        }
        private static bool TouchingCobweb(Player player)
        {
            Rectangle hitbox = player.Hitbox;

            int left = hitbox.Left / 16;
            int right = (hitbox.Right - 1) / 16;
            int top = hitbox.Top / 16;
            int bottom = (hitbox.Bottom - 1) / 16;

            for (int x = left; x <= right; x++)
            {
                for (int y = top; y <= bottom; y++)
                {
                    Tile tile = Framing.GetTileSafely(x, y);

                    if (tile.HasTile && tile.TileType == TileID.Cobweb)
                        return true;
                }
            }
            return false;
        }
    }
    public class SpiderBoots_Player : ModPlayer
    {
        public bool spiderBoots;
        public override void ResetEffects()
        {
            spiderBoots = false;
        }
        public override void PostUpdateRunSpeeds()
        {
            if (spiderBoots && TouchingCobweb(Player))
                Player.runAcceleration *= 2f;
        }
        private bool TouchingCobweb(Player player)
        {
            Rectangle hitbox = player.Hitbox;

            int left = hitbox.Left / 16;
            int right = hitbox.Right / 16;
            int top = hitbox.Top / 16;
            int bottom = hitbox.Bottom / 16;

            for (int x = left; x <= right; x++)
            {
                for (int y = top; y <= bottom; y++)
                {
                    Tile tile = Framing.GetTileSafely(x, y);

                    if (tile.TileType == TileID.Cobweb)
                        return true;
                }
            }
            return false;
        }
    }
}