using Microsoft.Build.Experimental.ProjectCache;
using Microsoft.CodeAnalysis;
using Redemption.BaseExtension;
using Redemption.Buffs.NPCBuffs;
using Redemption.NPCs.Critters;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Redemption.Globals
{
    public class ProjHelper : GlobalProjectile
    {
        public override void PostAI(Projectile projectile)
        {
            if (projectile.type is ProjectileID.VilePowder or ProjectileID.ViciousPowder)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    Terraria.NPC npc = Main.npc[i];
                    if (!npc.active || npc.ModNPC == null || npc.ModNPC is not Chicken || !projectile.Hitbox.Intersects(npc.Hitbox))
                        continue;

                    if (projectile.type is ProjectileID.VilePowder)
                        npc.Transform(NPCType<CorruptChicken>());
                    else
                        npc.Transform(NPCType<ViciousChicken>());
                }
            }
        }
        public override void AI(Projectile projectile)
        {
            if (ArenaSystem.ArenaActive && projectile.aiStyle == ProjAIStyleID.Hook && !projectile.Hitbox.Intersects(ArenaSystem.ArenaBoundsWorld))
                projectile.Kill();
        }
        public static bool Decapitation(Terraria.NPC target, ref int damage, ref bool crit, int chance = 200)
        {
            bool humanoid = NPCLists.SkeletonHumanoid.Contains(target.type) || NPCLists.Humanoid.Contains(target.type);
            if (target.life < target.lifeMax && target.life < damage * 100 && humanoid)
            {
                if (Main.rand.NextBool(chance))
                {
                    DecapitationEffect(target, ref crit);
                    return true;
                }
            }
            return false;
        }
        public static void DecapitationEffect(Terraria.NPC target, ref bool crit)
        {
            RedeDraw.SpawnExplosion(new Vector2(target.Center.X, target.position.Y + target.height / 4), Color.Orange, shakeAmount: 0, scale: .5f, noDust: true, rot: Main.rand.NextFloat(-0.1f, 0.1f), tex: "Redemption/Textures/SwordClash");

            if (!Main.dedServ)
                SoundEngine.PlaySound(CustomSounds.Slash2.WithPitchOffset(.3f), target.position);

            CombatText.NewText(target.getRect(), Color.Orange, Language.GetTextValue("Mods.Redemption.StatusMessage.Other.Decapitated"));
            target.Redemption().decapitated = true;
            crit = true;
            if (Main.netMode != NetmodeID.MultiplayerClient)
                target.StrikeInstantKill();

            RedeQuest.SetBonusDiscovered(RedeQuest.Bonuses.Slash, false);
            RedeQuest.SetBonusDiscovered(RedeQuest.Bonuses.Axe);
        }
        public static void DecapitationEffect(Terraria.NPC target, ref Terraria.NPC.HitModifiers modifiers)
        {
            RedeDraw.SpawnExplosion(new Vector2(target.Center.X, target.position.Y + target.height / 4), Color.Orange, shakeAmount: 0, scale: .5f, noDust: true, rot: Main.rand.NextFloat(-0.1f, 0.1f), tex: "Redemption/Textures/SwordClash");

            if (!Main.dedServ)
                SoundEngine.PlaySound(CustomSounds.Slash2.WithPitchOffset(.3f), target.position);

            CombatText.NewText(target.getRect(), Color.Orange, Language.GetTextValue("Mods.Redemption.StatusMessage.Other.Decapitated"));
            target.Redemption().decapitated = true;
            modifiers.SetInstantKill();
            modifiers.SetCrit();

            RedeQuest.SetBonusDiscovered(RedeQuest.Bonuses.Slash, false);
            RedeQuest.SetBonusDiscovered(RedeQuest.Bonuses.Axe);
        }

        public static void HoldOutProjBasics(Projectile proj, Terraria.Player player, Vector2 vector, Vector2 target = default)
        {
            if (target == default)
                target = Main.MouseWorld;
            if (Main.myPlayer == proj.owner)
            {
                float scaleFactor6 = 1f;
                if (player.inventory[player.selectedItem].shoot == proj.type)
                    scaleFactor6 = player.inventory[player.selectedItem].shootSpeed * proj.scale;
                Vector2 vector13 = target - vector;
                vector13.Normalize();
                if (vector13.HasNaNs())
                    vector13 = Vector2.UnitX * player.direction;
                vector13 *= scaleFactor6;
                if (vector13.X != proj.velocity.X || vector13.Y != proj.velocity.Y)
                    proj.netUpdate = true;

                proj.velocity = vector13;
                if (player.noItems || player.CCed || player.dead || !player.active)
                    proj.Kill();
                proj.netUpdate = true;
            }
        }
        public static void HoldOutProj_SlowTurn(Projectile proj, Terraria.Player player, Vector2 vector, float responsiveness)
        {
            if (Main.myPlayer == proj.owner)
            {
                float scaleFactor6 = 1f;
                if (player.inventory[player.selectedItem].shoot == proj.type)
                    scaleFactor6 = player.inventory[player.selectedItem].shootSpeed * proj.scale;

                Vector2 targetVector = Main.MouseWorld - vector;
                targetVector.Normalize();

                if (targetVector.HasNaNs())
                    targetVector = Vector2.UnitX * player.direction;

                if (targetVector.X != proj.velocity.X || targetVector.Y != proj.velocity.Y)
                    proj.netUpdate = true;

                proj.velocity = Vector2.Normalize(Vector2.Lerp(proj.velocity, targetVector, responsiveness));
                proj.velocity *= scaleFactor6;

                if (player.noItems || player.CCed || player.dead || !player.active)
                    proj.Kill();

                proj.netUpdate = true;
            }
        }
        public static bool Parry(Projectile projectile, Player player, bool meleeOnly = false)
        {
            Rectangle projHitbox = projectile.getRect();
            if (!player.Redemption().parried)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active || npc.dontTakeDamage || npc.friendly || npc.Redemption().invisible || npc.immortal)
                        continue;

                    bool canHit = NPCLoader.CanHitPlayer(npc, player, ref player.hurtCooldowns[0]);
                    if (npc.RedemptionHitbox().extendedHitbox != Rectangle.Empty)
                        canHit = true;
                    if (npc.RedemptionHitbox().IntersectsNPCExtended(projHitbox, npc) && canHit)
                    {
                        player.Redemption().parried = true;
                        return true;
                    }
                }

                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile other = Main.projectile[i];
                    if (!other.active || other.whoAmI == projectile.whoAmI || !other.hostile || other.Redemption().ParryBlacklist || !ProjectileLoader.CanHitPlayer(other, player))
                        continue;

                    if ((meleeOnly && !other.Redemption().TechnicallyMelee) || !projHitbox.Intersects(other.Hitbox))
                        continue;

                    Rectangle targetHitbox = other.getRect();
                    if (projHitbox.Intersects(targetHitbox))
                    {
                        player.Redemption().parried = true;
                        return true;
                    }
                }
            }
            return false;
        }
        public static bool Parry(Projectile projectile, Player player, ref NPC target, ref Projectile proj, bool meleeOnly = false)
        {
            Rectangle projHitbox = projectile.getRect();
            if (!player.Redemption().parried)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active || npc.dontTakeDamage || npc.friendly || npc.Redemption().invisible || npc.immortal)
                        continue;

                    bool canHit = NPCLoader.CanHitPlayer(npc, player, ref player.hurtCooldowns[0]);
                    if (npc.RedemptionHitbox().extendedHitbox != Rectangle.Empty)
                        canHit = true;
                    if (npc.RedemptionHitbox().IntersectsNPCExtended(projHitbox, npc) && canHit)
                    {
                        player.Redemption().parried = true;
                        target = npc;
                        proj = null;
                        return true;
                    }
                }

                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile other = Main.projectile[i];
                    if (!other.active || other.whoAmI == projectile.whoAmI || !other.hostile || other.Redemption().ParryBlacklist || !ProjectileLoader.CanHitPlayer(other, player))
                        continue;

                    if ((meleeOnly && !other.Redemption().TechnicallyMelee) || !projHitbox.Intersects(other.Hitbox))
                        continue;

                    Rectangle targetHitbox = other.getRect();
                    if (projHitbox.Intersects(targetHitbox))
                    {
                        player.Redemption().parried = true;
                        target = null;
                        proj = other;
                        return true;
                    }
                }
            }
            target = null;
            proj = null;
            return false;
        }
        public static bool SwordClashFriendly(Projectile projectile, Entity player, ref bool parried)
        {
            Rectangle projectileHitbox = projectile.Hitbox;
            if (projectile.Redemption().swordHitbox != default)
                projectileHitbox = projectile.Redemption().swordHitbox;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                Terraria.NPC npc = Main.npc[i];
                if (!npc.active)
                    continue;
                Rectangle target = npc.Redemption().parryHitbox;
                if (target == Rectangle.Empty)
                    continue;

                if (!parried && target.Intersects(projectileHitbox))
                {
                    if (player is Terraria.Player p)
                    {
                        p.immune = true;
                        p.immuneTime = 60;
                        p.AddBuff(BuffID.ParryDamageBuff, 120);

                        player.velocity.X += 4 * player.RightOfDir(npc);
                    }

                    RedeDraw.SpawnExplosion(RedeHelper.CenterPoint(projectile.Center, target.Center.ToVector2()), Color.White, shakeAmount: 0, scale: 1f, noDust: true, rot: Main.rand.NextFloat(MathHelper.PiOver4, 3 * MathHelper.PiOver4), tex: "Redemption/Textures/SwordClash");

                    if (!Main.dedServ)
                        SoundEngine.PlaySound(CustomSounds.SwordClash, projectile.position);
                    DustHelper.DrawCircle(RedeHelper.CenterPoint(projectile.Center, target.Center.ToVector2()), DustID.SilverCoin, 1, 4, 4, nogravity: true);
                    parried = true;

                    RedeQuest.SetBonusDiscovered(RedeQuest.Bonuses.Clash);
                    return true;
                }
            }
            return false;
        }
        public static bool SwordClashFriendly(Rectangle rect, Entity player, ref bool parried)
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                Terraria.NPC npc = Main.npc[i];
                if (!npc.active)
                    continue;
                Rectangle target = npc.Redemption().parryHitbox;
                if (target == Rectangle.Empty)
                    continue;

                if (!parried && target.Intersects(rect))
                {
                    if (player is Terraria.Player p)
                    {
                        p.immune = true;
                        p.immuneTime = 60;
                        p.AddBuff(BuffID.ParryDamageBuff, 120);
                    }
                    player.velocity.X += 4 * player.RightOfDir(npc);
                    RedeDraw.SpawnExplosion(RedeHelper.CenterPoint(rect.Center(), target.Center.ToVector2()), Color.White, shakeAmount: 0, scale: 1f, noDust: true, rot: Main.rand.NextFloat(MathHelper.PiOver4, 3 * MathHelper.PiOver4), tex: "Redemption/Textures/SwordClash");

                    if (!Main.dedServ)
                        SoundEngine.PlaySound(CustomSounds.SwordClash, rect.TopRight());
                    DustHelper.DrawCircle(RedeHelper.CenterPoint(rect.Center(), target.Center.ToVector2()), DustID.SilverCoin, 1, 4, 4, nogravity: true);
                    parried = true;

                    RedeQuest.SetBonusDiscovered(RedeQuest.Bonuses.Clash);
                    return true;
                }
            }
            return false;
        }
        public static bool SwordClashHostile(Projectile projectile, Terraria.NPC npc, ref bool parried)
        {
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Terraria.Player player = Main.player[i];
                if (!player.active || player.dead)
                    continue;
                Rectangle target = player.Redemption().parryHitbox;
                if (target == Rectangle.Empty)
                    continue;

                if (!parried && target.Intersects(projectile.Redemption().swordHitbox))
                {
                    npc.velocity.X += 4 * npc.RightOfDir(player);

                    RedeDraw.SpawnExplosion(RedeHelper.CenterPoint(projectile.Center, target.Center.ToVector2()), Color.White, shakeAmount: 0, scale: .5f, noDust: true, rot: Main.rand.NextFloat(MathHelper.PiOver4, 3 * MathHelper.PiOver4), tex: "Redemption/Textures/SwordClash");

                    if (!Main.dedServ)
                        SoundEngine.PlaySound(CustomSounds.SwordClash with { Volume = .5f }, projectile.position);
                    DustHelper.DrawCircle(RedeHelper.CenterPoint(projectile.Center, target.Center.ToVector2()), DustID.SilverCoin, 1, 4, 4, nogravity: true);

                    parried = true;
                    return true;
                }
            }
            return false;
        }
        public static bool SwordClashHostile(Rectangle rect, Terraria.NPC npc, ref bool parried)
        {
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Terraria.Player player = Main.player[i];
                if (!player.active || player.dead)
                    continue;
                Rectangle target = player.Redemption().parryHitbox;
                if (target == Rectangle.Empty)
                    continue;

                if (!parried && target.Intersects(rect))
                {
                    npc.velocity.X += 4 * npc.Center.RightOfDir(target.Center.ToVector2());

                    RedeDraw.SpawnExplosion(RedeHelper.CenterPoint(rect.Center(), target.Center.ToVector2()), Color.White, shakeAmount: 0, scale: .5f, noDust: true, rot: Main.rand.NextFloat(MathHelper.PiOver4, 3 * MathHelper.PiOver4), tex: "Redemption/Textures/SwordClash");

                    if (!Main.dedServ)
                        SoundEngine.PlaySound(CustomSounds.SwordClash with { Volume = .5f }, rect.TopRight());
                    DustHelper.DrawCircle(RedeHelper.CenterPoint(rect.Center(), target.Center.ToVector2()), DustID.SilverCoin, 1, 4, 4, nogravity: true);

                    parried = true;
                    return true;
                }
            }
            return false;
        }
        public static void OverlapCheck(Projectile projectile, float overlapVelocity = 0.04f)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile other = Main.projectile[i];

                if (i != projectile.whoAmI && other.active && other.owner == projectile.owner && Math.Abs(projectile.position.X - other.position.X) + Math.Abs(projectile.position.Y - other.position.Y) < projectile.width)
                {
                    if (projectile.position.X < other.position.X)
                        projectile.velocity.X -= overlapVelocity;
                    else
                        projectile.velocity.X += overlapVelocity;

                    if (projectile.position.Y < other.position.Y)
                        projectile.velocity.Y -= overlapVelocity;
                    else
                        projectile.velocity.Y += overlapVelocity;
                }
            }
        }
        public static Dictionary<int, (Entity entity, IEntitySource source)> projOwners = new();
        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Entity attacker = null;
            if (source is EntitySource_ItemUse item && projectile.friendly && !projectile.hostile)
                attacker = item.Entity;
            else if (source is EntitySource_Buff buff && projectile.friendly && !projectile.hostile)
                attacker = buff.Entity;
            else if (source is EntitySource_ItemUse_WithAmmo itemAmmo && projectile.friendly && !projectile.hostile)
                attacker = itemAmmo.Entity;
            else if (source is EntitySource_Mount mount && projectile.friendly && !projectile.hostile)
                attacker = mount.Entity;
            else if (source is EntitySource_Parent parent)
            {
                if (parent.Entity is Projectile proj)
                    attacker = Main.player[proj.owner];
                else
                    attacker = parent.Entity;
            }
            if (attacker != null)
            {
                projOwners.Remove(projectile.whoAmI);
                projOwners.Add(projectile.whoAmI, (attacker, source));
            }
        }
    }
}