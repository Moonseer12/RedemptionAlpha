using Redemption.Items.Materials.PreHM;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Items.Armor.PreHM.LivingWood
{
    [AutoloadEquip(EquipType.Body)]
    public class LivingWoodBody : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 22;
            Item.value = Item.sellPrice(copper: 35);
            Item.rare = ItemRarityID.White;
            Item.defense = 3;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Summon).Flat += 1;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<LivingTwig>(36)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}