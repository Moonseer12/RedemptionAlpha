using Microsoft.Xna.Framework.Graphics;
using Redemption.Globals.Players;
using Redemption.Items.Weapons.HM.Ranged;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Items.Weapons.HM.Melee
{
    public class CyberChakram : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ShimmerTransformToItem[Type] = ItemType<HyperTechRevolvers>();
            RedeGlowmask.AddGlowMask(Type, Texture + "_Glow");
        }
        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) => GlowmaskPlayer.DrawItemGlowMaskWorld(spriteBatch, Item, Request<Texture2D>(Texture + "_Glow").Value, rotation, scale);
        public override void SetDefaults()
        {
            Item.damage = 25;
            Item.ArmorPenetration = 5;
            Item.useTime = 8;
            Item.useAnimation = 30;
            Item.width = 30;
            Item.height = 30;
            Item.DamageType = DamageClass.Melee;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.knockBack = 4;
            Item.value = Item.sellPrice(0, 6, 0, 0);
            Item.rare = ItemRarityID.LightPurple;
            Item.UseSound = SoundID.Item15;
            Item.autoReuse = true;
            Item.noUseGraphic = true;
            Item.shoot = ProjectileType<CyberChakram_Proj>();
            Item.shootSpeed = 12;
        }
        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[Item.shoot] < 3;
        }
    }
}
