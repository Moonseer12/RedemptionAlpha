using Redemption.BaseExtension;
using Redemption.Buffs.Debuffs;
using Terraria;
using Terraria.ModLoader;

namespace Redemption.Buffs
{
    public class PanaceaBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Panacea");
            // Description.SetDefault("You feel great");
            Main.buffNoTimeDisplay[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.RedemptionRad().radiationLevel = 0;
            player.ClearBuff(ModContent.BuffType<HeadacheDebuff>());
            player.ClearBuff(ModContent.BuffType<NauseaDebuff>());
            player.ClearBuff(ModContent.BuffType<FatigueDebuff>());
            player.ClearBuff(ModContent.BuffType<FeverDebuff>());
            player.ClearBuff(ModContent.BuffType<HairLossDebuff>());
            player.ClearBuff(ModContent.BuffType<SkinBurnDebuff>());
            player.ClearBuff(ModContent.BuffType<RadiationDebuff>());
        }
    }
}