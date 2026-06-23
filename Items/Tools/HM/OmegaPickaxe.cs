using Microsoft.Xna.Framework.Graphics;
using Redemption.Globals.Players;
using Redemption.Items.Materials.HM;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Items.Tools.HM
{
    public class OmegaPickaxe : ModItem
    {
        public override void SetStaticDefaults()
        {
            RedeGlowmask.AddGlowMask(Type, Texture + "_Glow");
        }
        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) => GlowmaskPlayer.DrawItemGlowMaskWorld(spriteBatch, Item, Request<Texture2D>(Texture + "_Glow").Value, rotation, scale);

        public override void SetDefaults()
        {
            Item.attackSpeedOnlyAffectsWeaponAnimation = true;
            Item.damage = 46;
            Item.DamageType = DamageClass.Melee;
            Item.width = 56;
            Item.height = 50;
            Item.useTime = 5;
            Item.useAnimation = 16;
            Item.pick = 210;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5.5f;
            Item.value = Item.sellPrice(0, 4, 0, 0);
            Item.rare = ItemRarityID.Yellow;
            Item.UseSound = SoundID.Item15;
            Item.autoReuse = true;
            Item.useTurn = true;
            Item.tileBoost += 1;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<OmegaPowerCell>())
                .AddIngredient(ItemType<CorruptedXenomite>(), 6)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}