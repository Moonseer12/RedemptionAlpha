using Redemption.BaseExtension;
using Redemption.Globals;
using Redemption.Items.Materials.HM;
using Redemption.Items.Weapons.PreHM.Melee;
using Redemption.Projectiles.Melee;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Redemption.Items.Weapons.HM.Melee
{
    public class ForgottenGreatsword : ModItem
    {
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(ElementID.WindS);
        public override void SetStaticDefaults()
        {
            ElementID.ItemFire[Type] = true;
            ElementID.ItemWind[Type] = true;
        }

        public override void SetDefaults()
        {
            // Common Properties
            Item.width = 70;
            Item.height = 70;
            Item.rare = ItemRarityID.Yellow;
            Item.value = Item.sellPrice(gold: 18);

            // Use Properties
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 28;
            Item.useTime = 28;
            Item.UseSound = SoundID.DD2_BetsyWindAttack;
            Item.autoReuse = true;
            Item.channel = true;

            // Weapon Properties
            Item.damage = 70;
            Item.knockBack = 5;
            Item.noUseGraphic = true;
            Item.DamageType = DamageClass.Melee;
            Item.noMelee = true;

            // Projectile Properties
            Item.shootSpeed = 5f;
            Item.shoot = ProjectileType<ForgottenGreatsword_Proj>();

            Item.Redemption().TechnicallySlash = true;
            Item.Redemption().HideElementTooltip[ElementID.Wind] = true;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<ForgottenSword>())
                .AddIngredient(ItemType<OphosNotes>())
                .AddCondition(RedeConditions.RepairedByFallen)
                .Register();
        }
        public override bool MeleePrefix() => true;
        private static readonly int[] unwantedPrefixes = new int[] { PrefixID.Terrible, PrefixID.Dull, PrefixID.Shameful, PrefixID.Annoying, PrefixID.Broken, PrefixID.Damaged, PrefixID.Shoddy, PrefixID.Weak, PrefixID.Lazy, PrefixID.Small, PrefixID.Slow, PrefixID.Tiny, PrefixID.Sluggish, PrefixID.Unhappy };
        public override bool AllowPrefix(int pre)
        {
            if (Array.IndexOf(unwantedPrefixes, pre) > -1)
                return false;
            return true;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (Main.keyState.PressingShift())
            {
                TooltipLine line = new(Mod, "Lore", Language.GetTextValue("Mods.Redemption.Items.ForgottenGreatsword.Lore"))
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
