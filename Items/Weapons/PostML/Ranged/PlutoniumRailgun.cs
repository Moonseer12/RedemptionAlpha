using Microsoft.Xna.Framework.Graphics;
using Redemption.BaseExtension;
using Redemption.Globals;
using Redemption.Globals.Players;
using Redemption.Items.Materials.HM;
using Redemption.Items.Materials.PostML;
using Redemption.Projectiles.Ranged;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Items.Weapons.PostML.Ranged
{
    public class PlutoniumRailgun : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.IsRangedSpecialistWeapon[Type] = true;
            RedeGlowmask.AddGlowMask(Type, Texture + "_Glow");
        }
        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) => GlowmaskPlayer.DrawItemGlowMaskWorld(spriteBatch, Item, Request<Texture2D>(Texture + "_Glow").Value, rotation, scale);
        public override void SetDefaults()
        {
            Item.damage = 410;
            Item.noMelee = true;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 92;
            Item.height = 40;
            Item.useTime = 15;
            Item.useAnimation = 45;
            Item.useLimitPerAnimation = 3;
            Item.reuseDelay = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.shoot = ProjectileType<PlutoniumBeam>();
            Item.knockBack = 6;
            Item.value = Item.buyPrice(0, 35, 0, 0);
            Item.rare = ItemRarityID.Cyan;
            Item.UseSound = SoundID.Item75;
            Item.autoReuse = true;
            Item.shootSpeed = 3;
        }
        public override bool ReforgePrice(ref int reforgePrice, ref bool canApplyDiscount)
        {
            reforgePrice = Item.value / 5;
            return true;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.GetModPlayer<EnergyPlayer>().statEnergy < 6)
                return false;

            if (!Main.dedServ)
                SoundEngine.PlaySound(CustomSounds.Zap2 with { Pitch = 0.2f, Volume = 0.6f }, player.position);
            player.RedemptionScreen().ScreenShakeIntensity += 2;
            player.GetModPlayer<EnergyPlayer>().statEnergy -= 3;
            player.velocity -= RedeHelper.PolarVector(2, (Main.MouseWorld - player.Center).ToRotation());
            return true;
        }
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-20, 0);
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Plutonium>(), 30)
                .AddIngredient(ItemType<Plating>(), 5)
                .AddIngredient(ItemType<Capacitor>())
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}
