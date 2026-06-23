using Microsoft.Xna.Framework.Graphics;
using Redemption.BaseExtension;
using Redemption.Globals;
using Redemption.Globals.Players;
using Redemption.Items.Materials.HM;
using Redemption.Items.Materials.PostML;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Items.Weapons.PostML.Ranged
{
    public class BlastBattery : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.IsRangedSpecialistWeapon[Type] = true;
            RedeGlowmask.AddGlowMask(Type, Texture + "_Glow");
        }
        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) => GlowmaskPlayer.DrawItemGlowMaskWorld(spriteBatch, Item, Request<Texture2D>(Texture + "_Glow").Value, rotation, scale);

        public override void SetDefaults()
        {
            Item.damage = 160;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 30;
            Item.height = 38;
            Item.useTime = 5;
            Item.useAnimation = 30;
            Item.useLimitPerAnimation = 6;
            Item.reuseDelay = 60;
            Item.knockBack = 7;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.value = Item.sellPrice(0, 20, 0, 0);
            Item.rare = ItemRarityID.Red;
            Item.UseSound = CustomSounds.AlarmItem;
            Item.autoReuse = false;
            Item.useTurn = true;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.shoot = ProjectileType<BlastBattery_Missile>();
            Item.useAmmo = AmmoID.Rocket;
        }
        public override bool ReforgePrice(ref int reforgePrice, ref bool canApplyDiscount)
        {
            reforgePrice = Item.value / 3;
            return true;
        }
        public override bool CanConsumeAmmo(Item ammo, Player player)
        {
            NPC target = null;
            if (player.altFunctionUse != 2 && !RedeHelper.ClosestNPC(ref target, 300, Main.MouseWorld, true, player.MinionAttackTargetNPC))
                return false;
            return true;
        }
        public override void HoldItem(Player player)
        {
            player.RedemptionPlayerBuff().blastBattery = true;
        }
        public override bool AltFunctionUse(Player player) => true;
        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
                Item.useLimitPerAnimation = 6;
            else
                Item.useLimitPerAnimation = 1;
            return true;
        }
        public override float UseTimeMultiplier(Player player)
        {
            return player.altFunctionUse == 2 ? 1 : 6;
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            type = ProjectileType<BlastBattery_Crosshair>();
            position = Main.MouseWorld;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
                Projectile.NewProjectile(source, position + RedeHelper.Spread(80), Vector2.Zero, type, damage, knockback, Main.myPlayer, 1);
            else
                return true;

            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<RoboBrain>())
                .AddIngredient(ItemType<OmegaPowerCell>(), 2)
                .AddIngredient(ItemType<CorruptedXenomite>(), 12)
                .AddIngredient(ItemType<CarbonMyofibre>(), 4)
                .AddIngredient(ItemType<Plating>(), 2)
                .AddIngredient(ItemType<Capacitor>())
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
