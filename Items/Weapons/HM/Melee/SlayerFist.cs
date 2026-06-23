using Redemption.Globals;
using Redemption.Items.Materials.HM;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Redemption.Items.Weapons.HM.Melee
{
    public class SlayerFist : ModItem
    {
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(ElementID.ExplosiveS);
        public override void Load()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.HandsOn}", EquipType.HandsOn, Item.ModItem, null, new EquipTexture());
            }
        }
        private void SetupDrawing()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                EquipLoader.GetEquipSlot(Mod, Name, EquipType.HandsOn);
            }
        }
        public override void SetStaticDefaults()
        {
            ItemID.Sets.SkipsInitialUseSound[Item.type] = true;
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
            Item.ResearchUnlockCount = 1;

            ElementID.ItemExplosive[Type] = true;
            SetupDrawing();
        }
        public override void SetDefaults()
        {
            Item.damage = 220;
            Item.DamageType = DamageClass.Melee;
            Item.width = 46;
            Item.height = 24;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.knockBack = 8;
            Item.value = Item.sellPrice(0, 15, 0, 0);
            Item.rare = ItemRarityID.LightPurple;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ProjectileType<SlayerFist_Proj>();
            Item.shootSpeed = 5f;
        }
        public override void HoldItem(Player player)
        {
            var p = player.GetModPlayer<SlayerFist_Player>();
            p.VanityOn = true;
        }
        public override bool MeleePrefix() => true;
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<CyberPlating>(), 4)
                .AddIngredient(ItemType<Capacitor>(), 2)
                .AddIngredient(ItemType<AIChip>())
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    public class SlayerFist_Player : ModPlayer
    {
        public bool VanityOn;

        public override void ResetEffects()
        {
            VanityOn = false;
        }
        public override void FrameEffects()
        {
            if (VanityOn)
            {
                var item = GetInstance<SlayerFist>();
                Player.handon = (sbyte)EquipLoader.GetEquipSlot(Mod, item.Name, EquipType.HandsOn);
            }
        }
    }

}
