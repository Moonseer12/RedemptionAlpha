using Redemption.Globals;
using Redemption.Items.Weapons.PostML.Melee;
using Redemption.Projectiles.Ranged;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Redemption.Items.Weapons.PostML.Ranged
{
    public class DarkSteelBow : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Daerel's Dark-Steel Bow");
            /* Tooltip.SetDefault("Shoots Dark-Steel arrows that create shadow tendrils upon hitting a target\n" +
                "20% chance not to consume ammo"); */
            ItemID.Sets.ShimmerTransformToItem[Type] = ItemType<MythrilsBane>();
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            // Common Properties
            Item.width = 30;
            Item.height = 50;
            Item.rare = ItemRarityID.Purple;
            Item.value = Item.buyPrice(platinum: 1);

            // Use Properties
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.UseSound = SoundID.Item5;
            Item.autoReuse = true;

            // Weapon Properties
            Item.damage = 130;
            Item.knockBack = 3;
            Item.DamageType = DamageClass.Ranged;
            Item.noMelee = true;

            // Projectile Properties
            Item.shootSpeed = 20f;
            Item.shoot = ProjectileType<DarkSteelArrow>();
            Item.useAmmo = AmmoID.Arrow;
        }
        public override bool ReforgePrice(ref int reforgePrice, ref bool canApplyDiscount)
        {
            reforgePrice = Item.value / 10;
            return true;
        }
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-2, 0);
        }
        public override bool CanConsumeAmmo(Item ammo, Player player)
        {
            return Main.rand.NextFloat() >= .2f;
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            type = Item.shoot;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position + RedeHelper.PolarVector(8, (player.Center - Main.MouseWorld).ToRotation() + MathHelper.PiOver2), velocity, type, damage, knockback, player.whoAmI);
            Projectile.NewProjectile(source, position + RedeHelper.PolarVector(8, (player.Center - Main.MouseWorld).ToRotation() - MathHelper.PiOver2), velocity, type, damage, knockback, player.whoAmI);
            return false;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (Main.keyState.PressingShift())
            {
                TooltipLine line = new(Mod, "HoldShift", Language.GetTextValue("Mods.Redemption.Items.DarkSteelBow.Lore"))
                {
                    OverrideColor = Color.LightGray
                };
                tooltips.Add(line);
            }
            else
            {
                TooltipLine line = new(Mod, "HoldShift", Language.GetTextValue("Mods.Redemption.SpecialTooltips.Viewer"))
                {
                    OverrideColor = Color.Gray,
                };
                tooltips.Add(line);
            }
        }
    }
}
