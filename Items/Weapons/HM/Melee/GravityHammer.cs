using Redemption.BaseExtension;
using Redemption.Items.Weapons.HM.Ranged;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Items.Weapons.HM.Melee
{
    public class GravityHammer : ModItem
    {
        public override void SetStaticDefaults()
        {
            // Tooltip.SetDefault("Slamming the ground creates a shockwave");
            ItemID.Sets.ShimmerTransformToItem[Type] = ItemType<PrototypeAtomRifle>();
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            // Common Properties
            Item.width = 72;
            Item.height = 66;
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(gold: 20);

            // Use Properties
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 52;
            Item.useTime = 52;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = false;

            // Weapon Properties
            Item.damage = 135;
            Item.knockBack = 14;
            Item.noUseGraphic = true;
            Item.DamageType = DamageClass.Melee;
            Item.noMelee = true;
            Item.channel = true;

            // Projectile Properties
            Item.shootSpeed = 3.7f;
            Item.shoot = ProjectileType<GravityHammer_Proj>();
            Item.Redemption().TechnicallyHammer = true;
        }
        public override bool MeleePrefix() => true;
    }
}
