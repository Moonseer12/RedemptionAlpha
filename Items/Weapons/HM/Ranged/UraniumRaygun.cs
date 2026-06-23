using Microsoft.Xna.Framework.Graphics;
using Redemption.Globals.Players;
using Redemption.Items.Materials.HM;
using Redemption.Projectiles.Ranged;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Items.Weapons.HM.Ranged
{
    public class UraniumRaygun : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.IsRangedSpecialistWeapon[Type] = true;
            RedeGlowmask.AddGlowMask(Type, Texture + "_Glow");
        }
        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) => GlowmaskPlayer.DrawItemGlowMaskWorld(spriteBatch, Item, Request<Texture2D>(Texture + "_Glow").Value, rotation, scale);
        public override void SetDefaults()
        {
            Item.damage = 86;
            Item.useTime = 5;
            Item.useAnimation = 20;
            Item.reuseDelay = 25;
            Item.autoReuse = true;
            Item.shoot = ProjectileType<UraniumRaygun_Proj>();
            Item.shootSpeed = 11f;
            Item.UseSound = CustomSounds.RaygunShot.WithVolumeScale(0.3f);
            Item.DamageType = DamageClass.Ranged;
            Item.width = 58;
            Item.height = 36;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 0;
            Item.value = Item.sellPrice(0, 6, 0, 0);
            Item.rare = ItemRarityID.Lime;
        }
        public override bool CanUseItem(Player player)
        {
            return player.GetModPlayer<EnergyPlayer>().statEnergy >= 2;
        }
        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
                player.GetModPlayer<EnergyPlayer>().statEnergy -= 2;
            return base.UseItem(player);
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Uranium>(), 18)
                .AddIngredient(ItemType<Plating>(), 3)
                .AddIngredient(ItemType<Capacitor>())
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-2, 0);
        }
    }
}