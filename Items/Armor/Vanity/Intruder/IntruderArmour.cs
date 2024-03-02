﻿using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Items.Armor.Vanity.Intruder
{
    [AutoloadEquip(EquipType.Body)]
    class IntruderArmour : ModItem
    {
        public override void SetStaticDefaults()
        {
            //DisplayName.SetDefault("Intruder's Body Armor");
            //Tooltip.SetDefault("'From Intrusion, with love'");
            ArmorIDs.Body.Sets.HidesHands[EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body)] = false;
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 18;
            Item.rare = ItemRarityID.LightRed;
            Item.vanity = true;
        }
    }
}
