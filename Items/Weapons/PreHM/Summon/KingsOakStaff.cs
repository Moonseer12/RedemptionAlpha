using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Redemption.Buffs.Minions;
using Redemption.Projectiles.Minions;
using Redemption.Items.Materials.PreHM;
using Terraria.Localization;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Redemption.BaseExtension;
using Redemption.Items.Placeable.Tiles;

namespace Redemption.Items.Weapons.PreHM.Summon
{
    public class KingsOakStaff : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("King's Oak Staff");
			Tooltip.SetDefault("Summons a Nature Pixie to fight for you\n" +
                "Occassionally shouts at their target, dealing Psychic damage");
			SacrificeTotal = 1;

			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
			ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
		}

		public override void SetDefaults()
		{
			Item.damage = 10;
			Item.DamageType = DamageClass.Summon;
			Item.width = 54;
			Item.height = 58;
			Item.useTime = 18;
			Item.useAnimation = 18;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.noMelee = true;
			Item.knockBack = 2;
			Item.value = 1500;
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item44;
			Item.autoReuse = false;
			Item.buffType = ModContent.BuffType<NaturePixieBuff>();
			Item.shoot = ModContent.ProjectileType<NaturePixie>();
			Item.mana = 6;
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
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ElderWood>(), 24)
                .AddIngredient(ModContent.ItemType<GrimShard>(), 4)
                .AddIngredient(ModContent.ItemType<LostSoul>(), 8)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}