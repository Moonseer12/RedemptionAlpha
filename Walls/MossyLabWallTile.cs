using Microsoft.Xna.Framework;
using Redemption.Items.Placeable.Tiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Walls
{
    public class MossyLabWallTile : ModWall
    {
        public override void SetStaticDefaults()
        {
            Main.wallHouse[Type] = false;
            AddMapEntry(new Color(8, 64, 39));
            RegisterItemDrop(ModContent.ItemType<LabPlating>());
            HitSound = SoundID.Grass;
        }
        public override bool CanExplode(int i, int j) => false;
        public override void KillWall(int i, int j, ref bool fail) => fail = true;
    }
    public class MossyLabWall : PlaceholderTile
    {
        public override string Texture => "Redemption/Tiles/Placeholder/MossyLabWall";
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.createWall = ModContent.WallType<MossyLabWallTile>();
        }
    }
}