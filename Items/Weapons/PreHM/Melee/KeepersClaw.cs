using Redemption.BaseExtension;
using Redemption.Items.Materials.PreHM;
using Redemption.Items.Weapons.PreHM.Ranged;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Redemption.Items.Weapons.PreHM.Melee
{
    public class KeepersClaw : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Keeper's Claw");
            /* Tooltip.SetDefault("Hitting enemies with the slash inflicts Necrotic Gouge, causing them to burst into blood upon death\n" +
                "Physical slashes deal double damage to undead and skeletons\n" +
                "Hold left-click to charge a Blood Wave, taking away some of your life to fire life-stealing projectiles" +
                "\n'The hand of my beloved, cold and dead...'"); */
            ItemID.Sets.ShimmerTransformToItem[Type] = ItemType<FanOShivs>();
            ItemID.Sets.SkipsInitialUseSound[Item.type] = true;
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            // Common Properties
            Item.width = 54;
            Item.height = 48;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(gold: 1);

            // Use Properties
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;

            // Weapon Properties
            Item.damage = 26;
            Item.knockBack = 4;
            Item.noUseGraphic = true;
            Item.DamageType = DamageClass.Melee;
            Item.noMelee = true;
            Item.channel = true;

            // Projectile Properties
            Item.shootSpeed = 5f;
            Item.shoot = ProjectileType<KeepersClaw_Slash>();

            Item.Redemption().TechnicallySlash = true;
            Item.Redemption().CanSwordClash = true;
        }
        public override bool MeleePrefix() => true;
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<GrimShard>())
                .AddIngredient(ItemID.DemoniteBar, 12)
                .AddTile(TileID.Anvils)
                .Register();
            CreateRecipe()
                .AddIngredient(ItemType<GrimShard>())
                .AddIngredient(ItemID.CrimtaneBar, 12)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}