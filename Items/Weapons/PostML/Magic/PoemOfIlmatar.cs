using Redemption.BaseExtension;
using Redemption.Globals;
using Redemption.Items.Weapons.PostML.Melee;
using Redemption.Projectiles.Magic;
using Redemption.Rarities;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Redemption.Items.Weapons.PostML.Magic
{
    public class PoemOfIlmatar : ModItem
    {
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(ElementID.EarthS);
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ShimmerTransformToItem[Type] = ItemType<Ukonvasara>();

            ElementID.ItemWind[Type] = true;
            ElementID.ItemEarth[Type] = true;
            ElementID.ItemArcane[Type] = true;
        }
        public override void SetDefaults()
        {
            Item.damage = 220;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 20;
            Item.width = 32;
            Item.height = 50;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.noMelee = true;
            Item.autoReuse = true;
            Item.channel = true;
            Item.knockBack = 12;
            Item.value = Item.sellPrice(0, 25);
            Item.rare = RarityType<TurquoiseRarity>();
            Item.shootSpeed = 0;
            Item.shoot = ProjectileType<PoemTornado_Proj>();
            Item.UseSound = CustomSounds.WindLong;

            Item.Redemption().HideElementTooltip[ElementID.Earth] = true;
        }
    }
}