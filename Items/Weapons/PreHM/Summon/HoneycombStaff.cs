using Redemption.Buffs.Minions;
using Redemption.Globals;
using Redemption.Items.Materials.PreHM;
using Redemption.Projectiles.Minions;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Items.Weapons.PreHM.Summon
{
    public class HoneycombStaff : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.staff[Type] = true;
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
            ElementID.ItemNature[Type] = true;
            ElementID.ItemPoison[Type] = true;
        }
        public override void SetDefaults()
        {
            Item.width = 46;
            Item.height = 50;
            Item.value = Item.sellPrice(0, 1, 0, 0);
            Item.rare = ItemRarityID.Blue;

            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.UseSound = SoundID.DD2_DefenseTowerSpawn;
            Item.autoReuse = true;

            Item.DamageType = DamageClass.Summon;
            Item.damage = 12;
            Item.knockBack = 1;
            Item.noMelee = true;
            Item.sentry = true;

            Item.buffType = BuffType<HoneycombStaffBuff>();
            Item.shootSpeed = 0;
            Item.shoot = ProjectileType<HoneycombStaff_Proj>();
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<LivingTwig>(), 4)
                .AddIngredient(ItemID.BeeWax, 14)
                .AddTile(TileID.Anvils)
                .Register();
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            position = Main.MouseWorld;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);

            var projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, Main.myPlayer);
            projectile.originalDamage = Item.damage;

            player.UpdateMaxTurrets();
            return false;
        }
    }
}
