using Redemption.BaseExtension;
using Redemption.Items.Weapons.PostML.Ranged;
using Redemption.Projectiles.Melee;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Redemption.Items.Weapons.PostML.Melee
{
    public class MythrilsBane : ModItem
    {
        public bool channeled;
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ShimmerTransformToItem[Type] = ItemType<DarkSteelBow>();
            ItemID.Sets.SkipsInitialUseSound[Item.type] = true;
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            // Common Properties
            Item.width = 84;
            Item.height = 84;
            Item.rare = ItemRarityID.Purple;
            Item.value = Item.buyPrice(platinum: 1);

            // Use Properties
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 20;
            Item.useTime = 20;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.channel = true;

            // Weapon Properties
            Item.damage = 300;
            Item.knockBack = 5;
            Item.noUseGraphic = true;
            Item.DamageType = DamageClass.Melee;
            Item.noMelee = true;
            Item.channel = true;

            // Projectile Properties
            Item.shootSpeed = 5f;
            Item.shoot = ProjectileType<MythrilsBaneSlash_Proj>();

            Item.Redemption().TechnicallySlash = true;
            Item.Redemption().CanSwordClash = true;
        }
        public override bool ReforgePrice(ref int reforgePrice, ref bool canApplyDiscount)
        {
            reforgePrice = Item.value / 10;
            return true;
        }
        public override bool MeleePrefix() => true;
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float adjustedItemScale2 = player.GetAdjustedItemScale(Item);
            if (!channeled)
                Projectile.NewProjectile(source, position, velocity, ProjectileType<MythrilsBane_Proj>(), damage, knockback, player.whoAmI, 1, 0, adjustedItemScale2);
            else
            {
                Projectile.NewProjectile(source, position, velocity, ProjectileType<MythrilsBane_Proj>(), damage, knockback, player.whoAmI, -1, 0, adjustedItemScale2);
                channeled = false;
            }
            return false;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (Main.keyState.PressingShift())
            {
                TooltipLine line = new(Mod, "Lore", Language.GetTextValue("Mods.Redemption.Items.MythrilsBane.Lore"))
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