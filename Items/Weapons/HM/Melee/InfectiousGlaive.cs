using Microsoft.Xna.Framework.Graphics;
using Redemption.Globals.Players;
using Redemption.Items.Materials.HM;
using Redemption.Items.Weapons.PreHM.Melee;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Items.Weapons.HM.Melee
{
    public class InfectiousGlaive : ModItem
    {
        public override void SetStaticDefaults()
        {
            //DisplayName.SetDefault("Infectious Glaive");
            //Tooltip.SetDefault("Fires a spread of xenomite shards every two swings");
            ItemID.Sets.SkipsInitialUseSound[Item.type] = true;
            ItemID.Sets.Spears[Item.type] = true;
            RedeGlowmask.AddGlowMask(Type, Texture + "_Glow");
        }
        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) => GlowmaskPlayer.DrawItemGlowMaskWorld(spriteBatch, Item, Request<Texture2D>(Texture + "_Glow").Value, rotation, scale);

        public override void SetDefaults()
        {
            // Common Properties
            Item.width = 76;
            Item.height = 82;
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 4, 50);

            // Use Properties
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;

            // Weapon Properties
            Item.damage = 120;
            Item.crit = 16;
            Item.knockBack = 10f;
            Item.noUseGraphic = true;
            Item.DamageType = DamageClass.Melee;
            Item.noMelee = true;

            // Projectile Properties
            Item.shootSpeed = 3.7f;
            Item.shoot = ProjectileType<InfectiousGlaive_Proj>();
        }
        private int swingType;
        public override bool MeleePrefix() => true;
        public override bool AltFunctionUse(Player player) => false;
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float adjustedItemScale2 = player.GetAdjustedItemScale(Item);
            switch (swingType)
            {
                default:
                    swingType = 0;
                    if (!Main.dedServ)
                        SoundEngine.PlaySound(CustomSounds.Swoosh1, player.position);
                    Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0, 0, adjustedItemScale2);
                    break;
                case 0:
                    swingType++;
                    Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, -1, 0, adjustedItemScale2);
                    break;
                case 1:
                    swingType++;
                    Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 1, 0, adjustedItemScale2);
                    break;
                case 2:
                    swingType++;
                    Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, -1, 0, adjustedItemScale2);
                    break;
            }
            return false;
        }
        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[Item.shoot] < 1;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<XenoXyston>())
                .AddIngredient(ItemType<Xenomite>(), 8)
                .AddIngredient(ItemType<ToxicBile>(), 5)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}