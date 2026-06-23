using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework.Graphics;
using Redemption.BaseExtension;
using Redemption.DamageClasses;
using Redemption.Helpers;
using Redemption.Items.Weapons.PostML.Summon;
using Redemption.Items.Weapons.PreHM.Ritualist;
using Redemption.Particles;
using Redemption.Projectiles;
using Redemption.Projectiles.Ranged;
using ReLogic.Content;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Globals
{
    public class RedeProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public bool TechnicallyMelee;
        public bool IsHammer;
        public bool IsAxe;
        public bool IsSpear;
        public bool RitDagger;
        public bool RitDaggerNoSpirit;
        public float SpiritScale = 0;
        public bool EnergyBased;
        public bool ParryBlacklist;
        public bool friendlyHostile;
        public int DissolveTimer;
        public Rectangle swordHitbox;
        public bool FromMinion;
        public bool auraSentry;

        public override void SetDefaults(Projectile projectile)
        {
            if (ProjectileLists.IsTechnicallyMelee.Contains(projectile.type))
                TechnicallyMelee = true;

            if (friendlyHostile)
                projectile.noEnchantments = true;
        }
        public override bool? CanCutTiles(Projectile projectile)
        {
            return !friendlyHostile ? base.CanCutTiles(projectile) : false;
        }
        public override void ModifyHitNPC(Projectile projectile, NPC npc, ref NPC.HitModifiers modifiers)
        {
            if (friendlyHostile)
                modifiers.SourceDamage *= NPCHelper.HostileProjDamageMultiplier();

            #region Whip Effect
            if (projectile.Redemption().FromMinion)
                ApplyVanillaWhipEffect(projectile, npc, ref modifiers);

            if (projectile.minion || ProjectileID.Sets.MinionShot[projectile.type] || ProjectileID.Sets.SentryShot[projectile.type] || projectile.Redemption().FromMinion)
            {
                if (npc.RedemptionNPCBuff().cosmosChainsDebuff)
                {
                    if (Main.rand.NextBool(5))
                        modifiers.SetCrit();

                    RedeParticleManager.CreateRainbowParticle(projectile.RandAreaInEntity(), Vector2.UnitX.RotateRandom(6.28f), 0.2f, Main.DiscoColor);
                }
            }
            #endregion
        }
        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (projectile.owner == Main.myPlayer && Main.player[projectile.owner].RedemptionPlayerBuff().thornCirclet && projectile.CountsAsClass(DamageClass.Summon) && Main.rand.NextBool(6) && Main.projPet[projectile.type] && projectile.type != ProjectileType<BlightedClaw_Slash>())
            {
                float rotation = (projectile.Center - target.Center).ToRotation() + MathHelper.PiOver2;

                int upDown = Main.rand.NextBool() ? 1 : -1;
                if (!Main.dedServ)
                {
                    if (upDown is -1)
                        SoundEngine.PlaySound(CustomSounds.Slice5 with { Volume = .3f, Pitch = -.2f, PitchVariance = .2f, MaxInstances = 10 }, projectile.position);
                    else
                        SoundEngine.PlaySound(CustomSounds.Slice5 with { Volume = .3f, PitchVariance = .2f, MaxInstances = 10 }, projectile.position);
                }

                Vector2 spawnPosition = new(projectile.direction * -120, upDown * 20);
                Vector2 position = projectile.Center + spawnPosition.RotatedBy(rotation);

                Vector2 spawnVelocity = new(projectile.direction * 3, upDown * -1);
                Vector2 velocity = spawnVelocity.SafeNormalize(default).RotatedBy(rotation);

               int p = Projectile.NewProjectile(projectile.GetSource_OnHit(target), position, velocity * 5, ProjectileType<BlightedClaw_Slash>(), projectile.damage / 2, projectile.knockBack, projectile.owner, upDown);
                Main.projectile[p].DamageType = DamageClass.Summon;
            }
        }
        public override void OnKill(Projectile projectile, int timeLeft)
        {
            if (projectile.owner == Main.myPlayer && Main.player[projectile.owner].RedemptionPlayerBuff().thornCirclet && projectile.CountsAsClass(DamageClass.Ranged) && Main.rand.NextBool(6) && projectile.damage > 0 && !projectile.ownerHitCheck && projectile.friendly && projectile.type != ProjectileType<ThornCircletSplinter>() && projectile.velocity.Length() > 1)
            {
                int damage = projectile.damage / 2;
                if (damage <= 0)
                    damage = 1;
                for (int i = 0; i < Main.rand.Next(2, 4); i++)
                {
                    Projectile.NewProjectile(projectile.GetSource_FromAI(), projectile.Center, RedeHelper.PolarVector(Main.rand.Next(8, 10), RedeHelper.RandomRotation()), ProjectileType<ThornCircletSplinter>(), damage / 2, 1, Main.myPlayer);
                }
            }
        }
        public static void ApplyVanillaWhipEffect(Projectile projectile, NPC npc, ref NPC.HitModifiers modifiers)
        {
            bool flag19 = false;
            bool flag2 = false;
            bool flag3 = false;
            bool flag4 = false;
            bool flag5 = false;
            bool flag6 = false;
            bool flag7 = false;
            bool flag8 = false;
            for (int j = 0; j < 5; j++)
            {
                if (npc.buffTime[j] >= 1)
                {
                    switch (npc.buffType[j])
                    {
                        case 307:
                            flag19 = true;
                            break;
                        case 309:
                            flag2 = true;
                            break;
                        case 313:
                            flag3 = true;
                            break;
                        case 310:
                            flag4 = true;
                            break;
                        case 315:
                            flag5 = true;
                            break;
                        case 326:
                            flag6 = true;
                            break;
                        case 319:
                            flag7 = true;
                            break;
                        case 316:
                            flag8 = true;
                            break;
                    }
                }
            }
            if (flag19)
            {
                modifiers.FlatBonusDamage += 4;
            }
            if (flag5)
            {
                modifiers.FlatBonusDamage += 6;
            }
            if (flag6)
            {
                modifiers.FlatBonusDamage += 7;
            }
            if (flag2)
            {
                modifiers.FlatBonusDamage += 9;
            }
            if (flag7)
            {
                modifiers.FlatBonusDamage += 5;
                if (Main.rand.NextBool(20))
                {
                    modifiers.SetCrit();
                }
            }
            if (flag4)
            {
                int num7 = 10;
                int num8 = Projectile.NewProjectile(projectile.GetSource_FromAI(), npc.Center, Vector2.Zero, ProjectileID.ScytheWhipProj, num7, 0f, projectile.owner);
                Main.projectile[num8].localNPCImmunity[projectile.owner] = -1;
                Projectile.EmitBlackLightningParticles(npc);
            }
            if (flag8)
            {
                modifiers.FlatBonusDamage += 20;
                if (Main.rand.NextBool(10))
                {
                    modifiers.SetCrit();
                }
                ParticleOrchestraSettings particleOrchestraSettings = default;
                particleOrchestraSettings.PositionInWorld = projectile.Center;
                ParticleOrchestraSettings settings = particleOrchestraSettings;
                ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.RainbowRodHit, settings);
            }
            if (flag3)
            {
                npc.RequestBuffRemoval(313);
                float dmg = modifiers.GetDamage(projectile.damage, false);
                int num10 = (int)((float)dmg * 1.75f);
                int num12 = Projectile.NewProjectile(projectile.GetSource_FromAI(), npc.Center, Vector2.Zero, ProjectileID.FireWhipProj, num10, 0f, projectile.owner);
                Main.projectile[num12].localNPCImmunity[projectile.owner] = -1;
                modifiers.FlatBonusDamage += num10;
            }
        }
    }
    public abstract class TrueMeleeProjectile : ModProjectile
    {
        public float SetSwingSpeed(float speed)
        {
            Player player = Main.player[Projectile.owner];
            return speed / player.GetAttackSpeed(DamageClass.Melee);
        }
        public float SetSpeedBonus(int defaultUseTime, int useTime)
        {
            Player player = Main.player[Projectile.owner];
            return (float)defaultUseTime / useTime * player.GetAttackSpeed(DamageClass.Melee);
        }
        public int SetUseTime(int useTime)
        {
            Player player = Main.player[Projectile.owner];
            return (int)(useTime / player.GetAttackSpeed(DamageClass.Melee));
        }
        public virtual void SetSafeDefaults() { }
        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.ownerHitCheck = true;
            Projectile.Redemption().TechnicallyMelee = true;
            SetSafeDefaults();
        }
    }
    public abstract class LaserProjectile : ModRedeProjectile
    {
        public float AITimer
        {
            get => Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }
        public float Frame
        {
            get => Projectile.localAI[1];
            set => Projectile.localAI[1] = value;
        }
        public float LaserLength = 0;
        public float LaserScale = 0;
        public int LaserSegmentLength = 20;
        public int LaserWidth = 20;
        public int LaserEndSegmentLength = 14;
        public Vector2 endPoint;
        public bool NewCollision;

        public const float FirstSegmentDrawDist = 7;

        public int MaxLaserLength = 2000;
        public int maxLaserFrames = 1;
        public int LaserFrameDelay = 5;
        public bool StopsOnTiles = true;

        public virtual void SetSafeStaticDefaults() { }
        public override void SetStaticDefaults()
        {
            SetSafeStaticDefaults();
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 2400;
        }
        public virtual void SetSafeDefaults() { }
        public override void SetDefaults()
        {
            Projectile.width = LaserWidth;
            Projectile.height = LaserWidth;
            Projectile.Redemption().ParryBlacklist = true;
            SetSafeDefaults();
        }
        public virtual void EndpointTileCollision()
        {
            for (LaserLength = FirstSegmentDrawDist; LaserLength < MaxLaserLength; LaserLength += LaserSegmentLength)
            {
                Vector2 start = Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation) * LaserLength;
                if (!Collision.CanHitLine(Projectile.Center, 1, 1, start, 1, 1))
                {
                    LaserLength -= LaserSegmentLength;
                    break;
                }
            }
        }
        public void CheckHits() //code from slr
        {
            Terraria.Player player = Main.player[Projectile.owner];
            // done manually for clients that aren't the Projectile owner since onhit methods are clientside
            foreach (Terraria.NPC NPC in Main.npc.Where(n => n.active &&
                 !n.dontTakeDamage &&
                 !n.townNPC &&
                 n.immune[player.whoAmI] <= 0 &&
                 Colliding(new Rectangle(), n.Hitbox) == true))
            {
                OnHitNPC(NPC, new Terraria.NPC.HitInfo() { Damage = 0 }, 0);
            }
        }
        public float LengthSetting(Projectile projectile, Vector2 endpoint)
        {
            float hitscanBeamLength = (endpoint - projectile.Center).Length();
            float laserLength = MathHelper.Lerp(LaserLength, hitscanBeamLength, 1f);
            return laserLength;
        }
        public override void CutTiles()
        {
            // tilecut_0 is an unnamed decompiled variable which tells CutTiles how the tiles are being cut (in this case, via a Projectile).
            DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
            Utils.TileActionAttempt cut = new(DelegateMethods.CutTiles);
            Vector2 beamStartPos = Projectile.Center;
            Vector2 beamEndPos = beamStartPos + Projectile.velocity * LaserLength;

            // PlotTileLine is a function which performs the specified action to all tiles along a drawn line, with a specified width.
            // In this case, it is cutting all tiles which can be destroyed by Projectiles, for example grass or pots.
            Utils.PlotTileLine(beamStartPos, beamEndPos, Projectile.width * Projectile.scale, cut);
        }
        public virtual void CastLights(Vector3 color)
        {
            // Cast a light along the line of the Laser
            DelegateMethods.v3_1 = color;
            Utils.PlotTileLine(Projectile.Center, Projectile.Center + new Vector2(1f, 0).RotatedBy(Projectile.rotation) * LaserLength, 26, DelegateMethods.CastLight);
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) // code from slr
        {
            if (NewCollision)
            {
                if (Helper.CheckLinearCollision(Projectile.Center, endPoint, targetHitbox, out Vector2 collisionPoint))
                    return true;
            }
            else
            {
                Vector2 unit = new Vector2(1.5f, 0).RotatedBy(Projectile.rotation);
                float point = 0f;
                // Run an AABB versus Line check to look for collisions
                if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center,
                    Projectile.Center + unit * LaserLength, Projectile.width * LaserScale, ref point))
                    return true;
            }
            return false;
        }
    }
}
