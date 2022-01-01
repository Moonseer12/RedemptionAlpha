using Microsoft.Xna.Framework;
using Redemption.Items.Materials.PreHM;
using Redemption.Projectiles.Melee;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Items.Weapons.PostML.Melee
{
    public class Hacksaw : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Automated Hacksaw");
            Tooltip.SetDefault("Right-click to change attack modes");

            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            // Common Properties
            Item.width = 64;
            Item.height = 60;
            Item.rare = ItemRarityID.Purple;
            Item.value = Item.sellPrice(0, 7, 50);

            // Use Properties
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 10;
            Item.useTime = 10;
            Item.UseSound = SoundID.Item23;
            Item.autoReuse = false;

            // Weapon Properties
            Item.damage = 200;
            Item.knockBack = 4;
            Item.noUseGraphic = true;
            Item.DamageType = DamageClass.Melee;
            Item.noMelee = true;
            Item.channel = true;

            // Projectile Properties
            Item.shootSpeed = 10f;
            Item.shoot = ModContent.ProjectileType<Hacksaw_Proj>();
        }

        public int AttackMode;
        public override bool AltFunctionUse(Player player) => true;
        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
                Item.UseSound = SoundLoader.GetLegacySoundSlot(Mod, "Sounds/Custom/ShootChange");
            else
                Item.UseSound = SoundID.Item23;
            return true;
        }
        public override bool Shoot(Player player, ProjectileSource_Item_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                player.itemAnimationMax = 5;
                player.itemTime = 5;
                player.itemAnimation = 5;

                AttackMode++;
                if (AttackMode >= 3)
                    AttackMode = 0;

                switch (AttackMode)
                {
                    case 0:
                        CombatText.NewText(player.getRect(), Color.LightCyan, "Attack Mode 1", true, false);
                        break;
                    case 1:
                        CombatText.NewText(player.getRect(), Color.LightCyan, "Attack Mode 2", true, false);
                        break;
                    case 2:
                        CombatText.NewText(player.getRect(), Color.LightCyan, "Attack Mode 3", true, false);
                        break;
                }
            }
            else
            {
                switch (AttackMode)
                {
                    case 0:
                        Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0);
                        break;
                    case 1:
                        break;
                    case 2:
                        break;

                }
            }
            return false;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            string shotType = "";
            switch (AttackMode)
            {
                case 0:
                    shotType = "Attack Mode 1: Swings the hacksaw in a circle around the user";
                    break;
                case 1:
                    shotType = "Attack Mode 2: Revves up the blade, causing it to overheat and firing a powerful heat blast";
                    break;
                case 2:
                    shotType = "Attack Mode 3: Holds like a normal chainsaw, doing increasing damage over the time spent damaging a target";
                    break;
            }
            TooltipLine line = new(Mod, "ShotName", shotType)
            {
                overrideColor = Color.LightCyan,
            };
            tooltips.Add(line);
        }
    }
}