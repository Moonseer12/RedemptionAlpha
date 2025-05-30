using Terraria;
using Terraria.ModLoader;
using Redemption.BaseExtension;
using Redemption.CrossMod;

namespace Redemption.Buffs.Debuffs
{
    public class BileDebuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = true;

            ThoriumHelper.AddPlayerDoTBuffID(Type);
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.RedemptionPlayerBuff().bileDebuff = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.RedemptionNPCBuff().bileDebuff = true;
        }
    }
}