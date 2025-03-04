using Redemption.BaseExtension;
using Redemption.Buffs.Minions;
using Redemption.Globals;
using Redemption.Items.Weapons.PostML.Magic;
using Redemption.Projectiles.Minions;
using Redemption.Rarities;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Redemption.Items.Weapons.PostML.Summon
{
    public class UkonRuno : ModItem
    {
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(ElementID.ThunderS);
        public override void SetStaticDefaults()
		{
            ItemID.Sets.ShimmerTransformToItem[Type] = ItemType<PoemOfIlmatar>();
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;

            ElementID.ItemWater[Type] = true;
            ElementID.ItemThunder[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 240;
            Item.DamageType = DamageClass.Summon;
            Item.width = 38;
            Item.height = 30;
            Item.mana = 20;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.noMelee = true;
            Item.knockBack = 2;
            Item.value = Item.sellPrice(0, 20, 0, 0);
            Item.rare = RarityType<TurquoiseRarity>();
            Item.UseSound = SoundID.Item44;
            Item.autoReuse = false;
            Item.buffType = BuffType<UkkonenBuff>();
            Item.shoot = ProjectileType<Ukkonen>();

            Item.Redemption().HideElementTooltip[ElementID.Thunder] = true;
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

            return false;
        }
    }
}
