using Redemption.Buffs.Minions;
using Redemption.Items.Materials.HM;
using Redemption.Projectiles.Minions;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Items.Weapons.HM.Summon
{
    public class CorruptedSentryBase : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            // Common Properties
            Item.width = 30;
            Item.height = 40;
            Item.rare = ItemRarityID.Yellow;
            Item.value = Item.sellPrice(gold: 2);

            // Use Properties
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.DD2_DefenseTowerSpawn;
            Item.useAnimation = 12;
            Item.useTime = 12;
            Item.autoReuse = true;

            // Weapon Properties
            //Item.damage = 90;
            //Item.knockBack = 3;
            Item.DamageType = DamageClass.Summon;
            Item.noMelee = true;
            Item.sentry = true;

            // Projectile Properties
            Item.shootSpeed = 5f;
            Item.buffType = BuffType<CorruptedSentryBaseBuff>();
            Item.shoot = ProjectileType<CorruptedSentryBase_Proj>();
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);

            var projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, Main.myPlayer);
            projectile.originalDamage = Item.damage;

            player.UpdateMaxTurrets();
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<CorruptedXenomite>(), 4)
                .AddIngredient(ItemType<CarbonMyofibre>(), 8)
                .AddIngredient(ItemType<Plating>(), 2)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}