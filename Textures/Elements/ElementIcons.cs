using Terraria;
using Terraria.ModLoader;

namespace Redemption.Textures.Elements
{
    public class Arcane : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 22;
        }
    }
    public class Axe : Arcane { }
    public class Blood : Arcane { }
    public class Cosmic : Arcane { }
    public class Clash : Arcane { }
    public class Earth : Arcane { }
    public class Explosive : Arcane { }
    public class Fire : Arcane { }
    public class Hammer : Arcane { }
    public class Holy : Arcane { }
    public class Ice : Arcane { }
    public class Nature : Arcane { }
    public class Poison : Arcane { }
    public class Psychic : Arcane { }
    public class Shadow : Arcane { }
    public class Slash : Arcane { }
    public class Spear : Arcane { }
    public class Thunder : Arcane { }
    public class Water : Arcane { }
    public class Wind : Arcane { }
    public class ChaliceIcon : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 36;
        }
    }
}