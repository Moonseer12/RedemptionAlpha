using Microsoft.Xna.Framework.Graphics;
using Redemption.Globals.Players;
using Redemption.Items.Materials.HM;
using Redemption.Projectiles.Ranged;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Items.Weapons.HM.Ranged
{
    public class CorruptedDAN : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Corrupted D.A.N");
            /* Tooltip.SetDefault("Fires two blasts of rockets per use\n" +
                "Continuing to hold left-click will spin the weapon while firing, creating a spiral of homing rockets\n" +
                "\n(15[i:" + ModContent.ItemType<EnergyPack>() + "]) Continuing to hold left-click while aiming downwards will charge a red beam that'll cause eruptions on impact\n" +
                "66% chance to not consume ammo, 90% chance during the homing rocket spiral"); */
            ItemID.Sets.SkipsInitialUseSound[Item.type] = true;
            ItemID.Sets.IsRangedSpecialistWeapon[Type] = true;
            RedeGlowmask.AddGlowMask(Type, Texture + "_Glow");
        }
        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) => GlowmaskPlayer.DrawItemGlowMaskWorld(spriteBatch, Item, Request<Texture2D>(Texture + "_Glow").Value, rotation, scale);

        public override void SetDefaults()
        {
            Item.damage = 40;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 132;
            Item.height = 52;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.knockBack = 2;
            Item.value = Item.sellPrice(0, 15, 0, 0);
            Item.rare = ItemRarityID.Yellow;
            Item.UseSound = CustomSounds.ShotgunBlast1;
            Item.autoReuse = true;
            Item.shoot = ProjectileType<DAN_Rocket>();
            Item.shootSpeed = 10;
            Item.useAmmo = AmmoID.Rocket;
        }
        public override bool ReforgePrice(ref int reforgePrice, ref bool canApplyDiscount)
        {
            reforgePrice = Item.value / 3;
            return true;
        }
        public override bool CanConsumeAmmo(Item ammo, Player player) => player.ItemUsesThisAnimation != 0;
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            type = ProjectileType<CorruptedDAN_Proj>();
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<DAN>())
                .AddIngredient(ItemType<OmegaPowerCell>())
                .AddIngredient(ItemType<CorruptedXenomite>(), 8)
                .AddIngredient(ItemType<CarbonMyofibre>(), 6)
                .AddIngredient(ItemType<Plating>(), 4)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
