using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption
{
	public class RedeNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;
		public override bool CloneNewInstances => true;
        public bool decapitated;
        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref int damage, ref float knockback, ref bool crit)
        {
            // Decapitation
            if (npc.life < npc.lifeMax && item.CountsAsClass(DamageClass.Melee) && item.damage >= 4 && item.useStyle == ItemUseStyleID.Swing && NPCTags.SkeletonHumanoid.Has(npc.type))
            {
                if (Main.rand.NextBool(300) && !ItemTags.BluntSwing.Has(item.type))
                {
                    decapitated = true;
                    damage = damage < npc.life ? npc.life : damage;
                    crit = true;
                }
                else if (Main.rand.NextBool(100) && item.axe > 0)
                {
                    decapitated = true;
                    damage = damage < npc.life ? npc.life : damage;
                    crit = true;
                }
            }
        }
        public override void ModifyGlobalLoot(GlobalLoot globalLoot)
        {
            var condition = new DecapitationCondition();
            var ruleToChain = new LeadingConditionRule(condition);
            globalLoot.Add(ruleToChain.OnSuccess(ItemDropRule.Common(ItemID.Skull)));
        }
    }
}