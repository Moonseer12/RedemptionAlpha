using Microsoft.Xna.Framework.Graphics;
using Redemption.Base;
using Redemption.BaseExtension;
using Redemption.Items.Accessories.PostML;
using Redemption.NPCs.Friendly.SpiritSummons;
using Redemption.NPCs.Minibosses.Calavia;
using Redemption.NPCs.PreHM;
using Redemption.UI.ChatUI;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Globals.NPC
{
    public class GuardNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public int GuardPoints;
        public bool IgnoreArmour;
        public bool GuardBroken;
        public bool GuardPierce;
        public bool GuardHammer;
        public double GuardDamage = 1;
        public void GuardHit(ref Terraria.NPC.HitInfo info, Terraria.NPC npc, SoundStyle sound, float dmgReduction = .25f, bool noNPCHitSound = false, int dustType = 0, SoundStyle breakSound = default, int dustAmount = 10, float dustScale = 1, int damage = 0)
        {
            if (breakSound == default)
                breakSound = CustomSounds.GuardBreak;
            if (IgnoreArmour || GuardPoints < 0 || GuardBroken)
            {
                IgnoreArmour = false;
                return;
            }

            int guardDamage = (int)(info.Damage * GuardDamage * dmgReduction);
            SoundEngine.PlaySound(sound, npc.position);
            CombatText.NewText(npc.getRect(), Colors.RarityPurple, guardDamage, true, true);
            GuardPoints -= guardDamage;

            if (GuardPierce)
            {
                info.Damage /= 4;
                info.Knockback /= 2;
                GuardPierce = false;

                GuardBreakCheck(npc, dustType, breakSound, dustAmount, dustScale, damage);
                return;
            }
            npc.HitEffect();
            info.HideCombatText = true;
            if (!noNPCHitSound && npc.HitSound.HasValue)
                SoundEngine.PlaySound(npc.HitSound.Value, npc.position);
            info.Damage = 0;
            if (!GuardHammer)
                info.Knockback = 0;
            else
                GuardHammer = false;
            npc.life++;

            GuardBreakCheck(npc, dustType, breakSound, dustAmount, dustScale, damage);
        }
        public void GuardBreakCheck(Terraria.NPC npc, int dustType, SoundStyle sound, int dustAmount = 10, float dustScale = 1, int damage = 0)
        {
            if (IgnoreArmour)
                IgnoreArmour = false;

            if (GuardPoints > 0 || GuardBroken)
                return;

            SoundEngine.PlaySound(sound, npc.position);

            CombatText.NewText(npc.getRect(), Colors.RarityPurple, "Guard Broken!", false, true);
            for (int i = 0; i < dustAmount; i++)
                Dust.NewDust(npc.position + npc.velocity, npc.width, npc.height, dustType, npc.velocity.X * 0.5f, npc.velocity.Y * 0.5f, Scale: dustScale);
            GuardBroken = true;
            if (damage > 0)
                BaseAI.DamageNPC(npc, damage, 0, Main.LocalPlayer, true, true);

            #region Unique NPC Effects
            if (npc.type == NPCType<SkeletonWarden>())
            {
                if (Main.netMode != NetmodeID.Server)
                    Gore.NewGore(npc.GetSource_FromThis(), npc.position, npc.velocity, Find<ModGore>("Redemption/SkeletonWardenGore2").Type, 1);
            }
            else if (npc.type == NPCType<SkeletonWarden_SS>())
            {
                for (int i = 0; i < 4; i++)
                {
                    int dust = Dust.NewDust(npc.position + npc.velocity, npc.width, npc.height, DustID.DungeonSpirit,
                        npc.velocity.X * 0.5f, npc.velocity.Y * 0.5f, Scale: 2);
                    Main.dust[dust].velocity *= 5f;
                    Main.dust[dust].noGravity = true;
                }
            }
            else if (npc.type == NPCType<Calavia>())
            {
                if (Main.netMode != NetmodeID.Server)
                {
                    Gore.NewGore(npc.GetSource_FromThis(), npc.position, npc.velocity, Find<ModGore>("Redemption/CalaviaShieldGore1").Type, 1);
                    Gore.NewGore(npc.GetSource_FromThis(), npc.position, npc.velocity, Find<ModGore>("Redemption/CalaviaShieldGore2").Type, 1);
                }
                EmoteBubble.NewBubble(1, new WorldUIAnchor(npc), 120);
                if (!Main.dedServ)
                {
                    Texture2D bubble = !Main.dedServ ? Request<Texture2D>("Redemption/UI/TextBubble_Epidotra").Value : null;
                    SoundStyle voice = CustomSounds.Voice1 with { Pitch = 0.6f };
                    Dialogue d1 = new(npc, "Oru'takh!", Color.White, Color.Gray, voice, 0.01f, 1f, .5f, true, bubble: bubble);
                    ChatUI.Visible = true;
                    ChatUI.Add(d1);
                }
            }
            #endregion
        }
        public override void ModifyHitByItem(Terraria.NPC npc, Terraria.Player player, Item item, ref Terraria.NPC.HitModifiers modifiers)
        {
            if (GuardPoints <= 0)
                return;
            GuardDamage = 1;
            if (npc.RedemptionNPCBuff().brokenArmor || npc.RedemptionNPCBuff().stunned)
                GuardPierce = true;
            else if (ItemID.Sets.Spears[item.type])
            {
                GuardPierce = true;
                RedeQuest.SetBonusDiscovered(RedeQuest.Bonuses.Spear);
            }
            if (item.HasElement(ElementID.Psychic))
            {
                IgnoreArmour = true;
                RedeQuest.SetBonusDiscovered(RedeQuest.Bonuses.Psychic);
            }
            if (player.RedemptionPlayerBuff().wardbreaker && (item.HasElement(ElementID.Arcane) || item.DamageType == DamageClass.Magic))
                GuardDamage += 1;
            if (item.HasElement(ElementID.Explosive))
            {
                GuardDamage += 1;
                RedeQuest.SetBonusDiscovered(RedeQuest.Bonuses.Explosive);
            }
            if (item.hammer > 0 || item.Redemption().TechnicallyHammer)
            {
                GuardHammer = true;
                GuardDamage += 3;
                RedeQuest.SetBonusDiscovered(RedeQuest.Bonuses.Hammer);
            }
        }
        public override void ModifyHitByProjectile(Terraria.NPC npc, Projectile projectile, ref Terraria.NPC.HitModifiers modifiers)
        {
            if (GuardPoints <= 0)
                return;
            GuardDamage = 1;
            if (projectile.HasElement(ElementID.Psychic))
            {
                IgnoreArmour = true;
                RedeQuest.SetBonusDiscovered(RedeQuest.Bonuses.Psychic);
            }
            if (projectile.Redemption().IsHammer || projectile.type == ProjectileID.PaladinsHammerFriendly)
            {
                GuardHammer = true;
                GuardDamage += 3;
                RedeQuest.SetBonusDiscovered(RedeQuest.Bonuses.Hammer);
            }

            bool isSpear = projectile.Redemption().IsSpear || ProjectileLists.ProjSpear[projectile.type];
            if (Main.player[projectile.owner].HeldItem.shoot == projectile.type)
                isSpear |= ItemID.Sets.Spears[Main.player[projectile.owner].HeldItem.type];
            if (npc.RedemptionNPCBuff().brokenArmor || npc.RedemptionNPCBuff().stunned || projectile.Redemption().EnergyBased)
                GuardPierce = true;
            else if (isSpear)
            {
                GuardPierce = true;
                RedeQuest.SetBonusDiscovered(RedeQuest.Bonuses.Spear);
            }

            if (Main.player[projectile.owner].RedemptionPlayerBuff().wardbreaker && (projectile.HasElement(ElementID.Arcane) || projectile.DamageType == DamageClass.Magic))
                GuardDamage += 1;

            if (projectile.HasElement(ElementID.Explosive))
            {
                GuardDamage += 1;
                RedeQuest.SetBonusDiscovered(RedeQuest.Bonuses.Explosive);
            }
        }
        public override void SetDefaults(Terraria.NPC npc)
        {
            base.SetDefaults(npc);
            if (RedeConfigServer.Instance.VanillaGuardPointsDisable)
                return;
            if (npc.type is NPCID.GreekSkeleton or NPCID.AngryBonesBig or NPCID.AngryBonesBigHelmet or NPCID.AngryBonesBigMuscle or NPCID.GoblinWarrior)
                GuardPoints = 25;
            if (npc.type is NPCID.ArmoredSkeleton or NPCID.ArmoredViking or NPCID.PossessedArmor)
                GuardPoints = 80;
            if (npc.type is NPCID.BlueArmoredBones or NPCID.BlueArmoredBonesMace or NPCID.BlueArmoredBonesNoPants or NPCID.BlueArmoredBonesSword or NPCID.RustyArmoredBonesAxe or NPCID.RustyArmoredBonesFlail or NPCID.RustyArmoredBonesSword or NPCID.HellArmoredBones or NPCID.HellArmoredBonesMace or NPCID.HellArmoredBonesSpikeShield or NPCID.HellArmoredBonesSword)
                GuardPoints = 160;
            if (npc.type is NPCID.Paladin)
                GuardPoints = 500;
        }
        public override void ModifyIncomingHit(Terraria.NPC npc, ref Terraria.NPC.HitModifiers modifiers)
        {
            if (RedeConfigServer.Instance.VanillaGuardPointsDisable)
                return;

            if (npc.type is NPCID.GreekSkeleton)
            {
                if (GuardPoints >= 0)
                {
                    modifiers.DisableCrit();
                    modifiers.ModifyHitInfo += (ref Terraria.NPC.HitInfo n) => GuardHit(ref n, npc, SoundID.NPCHit4, .25f, false, DustID.Gold, damage: npc.lifeMax / 4);
                }
            }
            if (npc.type is NPCID.AngryBonesBig or NPCID.AngryBonesBigHelmet or NPCID.AngryBonesBigMuscle or NPCID.ArmoredSkeleton or NPCID.ArmoredViking or NPCID.BlueArmoredBones or NPCID.BlueArmoredBonesMace or NPCID.BlueArmoredBonesNoPants or NPCID.BlueArmoredBonesSword or NPCID.RustyArmoredBonesAxe or NPCID.RustyArmoredBonesFlail or NPCID.RustyArmoredBonesSword)
            {
                if (GuardPoints >= 0)
                {
                    modifiers.DisableCrit();
                    modifiers.ModifyHitInfo += (ref Terraria.NPC.HitInfo n) => GuardHit(ref n, npc, SoundID.NPCHit4, .35f, false, DustID.Bone, damage: npc.lifeMax / 4);
                }
            }
            if (npc.type is NPCID.HellArmoredBones or NPCID.HellArmoredBonesMace or NPCID.HellArmoredBonesSpikeShield or NPCID.HellArmoredBonesSword)
            {
                if (GuardPoints >= 0)
                {
                    modifiers.DisableCrit();
                    modifiers.ModifyHitInfo += (ref Terraria.NPC.HitInfo n) => GuardHit(ref n, npc, SoundID.NPCHit4, .35f, false, DustID.Torch, damage: npc.lifeMax / 4);
                }
            }
            if (npc.type is NPCID.PossessedArmor)
            {
                if (GuardPoints >= 0)
                {
                    modifiers.DisableCrit();
                    modifiers.ModifyHitInfo += (ref Terraria.NPC.HitInfo n) => GuardHit(ref n, npc, SoundID.NPCHit4, .25f, false, DustID.Demonite, damage: npc.lifeMax / 2);
                }
            }
            if (npc.type is NPCID.GoblinWarrior)
            {
                if (GuardPoints >= 0)
                {
                    modifiers.DisableCrit();
                    modifiers.ModifyHitInfo += (ref Terraria.NPC.HitInfo n) => GuardHit(ref n, npc, SoundID.NPCHit4, .4f, false, DustID.Iron, damage: npc.lifeMax / 4);
                }
            }
            if (npc.type is NPCID.Paladin)
            {
                if (GuardPoints >= 0)
                {
                    modifiers.DisableCrit();
                    modifiers.ModifyHitInfo += (ref Terraria.NPC.HitInfo n) => GuardHit(ref n, npc, SoundID.NPCHit4, .2f, false, DustID.GoldCoin, damage: npc.lifeMax / 3);
                }
            }
        }
    }
}
