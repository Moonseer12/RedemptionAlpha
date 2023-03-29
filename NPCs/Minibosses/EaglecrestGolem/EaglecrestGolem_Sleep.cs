using Redemption.Buffs.Debuffs;
using Redemption.Buffs.NPCBuffs;
using Redemption.Globals;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace Redemption.NPCs.Minibosses.EaglecrestGolem
{
    public class EaglecrestGolem_Sleep : ModNPC
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sleeping Stones");
            NPCID.Sets.MPAllowedEnemies[Type] = true;

            NPCID.Sets.DebuffImmunitySets.Add(Type, new NPCDebuffImmunityData
            {
                SpecificallyImmuneTo = new int[] {
                    BuffID.Poisoned,
                    ModContent.BuffType<InfestedDebuff>(),
                    ModContent.BuffType<NecroticGougeDebuff>(),
                    ModContent.BuffType<ViralityDebuff>(),
                    ModContent.BuffType<DirtyWoundDebuff>()
                }
            });

            NPCID.Sets.NPCBestiaryDrawModifiers value = new(0)
            {
                Hide = true
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
        }
        public override void SetDefaults()
        {
            NPC.width = 80;
            NPC.height = 80;
            NPC.friendly = false;
            NPC.damage = 0;
            NPC.defense = 50;
            NPC.lifeMax = 100;
            NPC.HitSound = SoundID.NPCHit41;
            NPC.SpawnWithHigherTime(30);
            NPC.knockBackResist = 0f;
            NPC.aiStyle = -1;
            NPC.chaseable = false;
        }
        public override void HitEffect(int hitDirection, double damage)
        {
            Dust.NewDust(NPC.position + NPC.velocity, NPC.width, NPC.height, DustID.Stone, NPC.velocity.X * 0.5f, NPC.velocity.Y * 0.5f, 20, default, 3f);
        }
        public override void ModifyHitByItem(Player player, Item item, ref int damage, ref float knockback, ref bool crit)
        {
            if (item.pick > 0)
                damage += item.pick / 2;
        }
        public override bool CheckDead()
        {
            NPC.SetDefaults(ModContent.NPCType<EaglecrestGolem>());
            return false;
        }
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC target = Main.npc[i];
                if (!target.active)
                    continue;

                if (target.boss)
                    return 0f;
            }

            return SpawnCondition.OverworldDay.Chance * (NPC.downedBoss2 && !RedeBossDowned.downedEaglecrestGolem &&
                !NPC.AnyNPCs(ModContent.NPCType<EaglecrestGolem>()) && !NPC.AnyNPCs(ModContent.NPCType<EaglecrestGolem_Sleep>()) ? 0.1f : 0f);
        }
    }
}