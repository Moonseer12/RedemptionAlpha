using Microsoft.Build.Construction;
using Microsoft.Build.Execution;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.Core;
using Redemption.Base;
using Redemption.BaseExtension;
using Redemption.Buffs.NPCBuffs;
using Redemption.Dusts;
using Redemption.Globals;
using Redemption.Particles;
using Redemption.Projectiles.Ranged;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ModLoader;
using static Terraria.NPC.NPCNameFakeLanguageCategoryPassthrough;

namespace Redemption.Items.Weapons.PostML.Melee
{
    public class Ukonvasara_Hammer : TrueMeleeProjectile
    {
        public override string Texture => "Redemption/Items/Weapons/PostML/Melee/Ukonvasara";
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Ukonvasara");
            ElementID.ProjEarth[Type] = true;
            ElementID.ProjThunder[Type] = true;
        }
        public override bool ShouldUpdatePosition() => TeleportTrigger;
        public override void SetSafeDefaults()
        {
            Projectile.width = 90;
            Projectile.height = 90;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = false;
            Projectile.ownerHitCheck = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.alpha = 255;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 100;
            Projectile.Redemption().IsHammer = true;
        }
        private Player Owner => Main.player[Projectile.owner];
        public ref float Length => ref Projectile.localAI[0];
        public ref float Rot => ref Projectile.localAI[1];
        private Vector2 startVector;
        private Vector2 positionVector;
        public float Timer;
        public int pauseTimer;
        public float progress;
        public int maxTime;
        public override void AI()
        {
            if (Owner.noItems || Owner.CCed || Owner.dead || !Owner.active)
                Projectile.Kill();

            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;
            Projectile.timeLeft = 2;
            maxTime = SetUseTime(Owner.HeldItem.useTime);

            if (!TeleportTrigger && !LaunchTrigger)
            {
                Swing();
            }
            if (TeleportTrigger && !LaunchTrigger)
            {
                Teleport();
            }
            if (TeleportTrigger && LaunchTrigger)
            {
                Projectile.extraUpdates = 2;
                Launch();
            }
            if (Projectile.penetrate == 0)
                Projectile.Kill();
        }

        public void Swing()
        {
            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;

            if (Owner.noItems || Owner.CCed || Owner.dead || !Owner.active)
                Projectile.Kill();

            Vector2 armCenter = Owner.RotatedRelativePoint(Owner.MountedCenter) + new Vector2(Owner.direction * -4, -4);
            Projectile.spriteDirection = Owner.direction;
            progress = Timer / (maxTime * 6 * Projectile.MaxUpdates);
            if (Timer++ == 0)
            {
                Projectile.scale = 2 * Projectile.ai[2];
                Length = 24 * Projectile.scale;
                startVector = RedeHelper.PolarVector(1, Projectile.spriteDirection * MathHelper.PiOver2 + MathHelper.PiOver2) * Length;
            }
            if (Timer == (int)(maxTime * 2 * Projectile.MaxUpdates))
            {
                if (!Main.dedServ)
                    SoundEngine.PlaySound(CustomSounds.Swoosh1 with { Pitch = -.6f }, Owner.position);
            }
            if (progress < 0.3f)
            {
                Rot = -MathHelper.ToRadians(120 + 100f * MathF.Atan(5 * MathHelper.Pi * (progress - 0.5f))) * Projectile.spriteDirection;
                positionVector = startVector.RotatedBy(Rot);
            }
            else if (progress < 1f)
            {
                Rot = MathHelper.ToRadians(120 + 100f * MathF.Atan(5 * MathHelper.Pi * (progress - 0.5f))) * Projectile.spriteDirection;
                positionVector = startVector.RotatedBy(Rot);
            }
            else
                Projectile.Kill();

            Projectile.friendly = progress >= 0.45f;

            if (Timer == 2)
            {
                Projectile.alpha = 0;
            }
            Projectile.Center = armCenter + positionVector;

            if (Projectile.spriteDirection == 1)
                Projectile.rotation = positionVector.ToRotation() + MathHelper.PiOver4;
            else
                Projectile.rotation = positionVector.ToRotation() + 3 * MathHelper.PiOver4;

            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, positionVector.ToRotation() - MathHelper.PiOver2);
        }

        public bool TeleportTrigger;
        public bool LaunchTrigger;
        public float TeleportTimer;
        private List<Vector2> cache;
        NPC initialTarget;
        NPC target;
        List<int> targetCache = new();
        public void Teleport()
        {
            Projectile.velocity *= 0;

            target = initialTarget;
            if (targetCache == null)
                targetCache = [target.whoAmI];
            else
                targetCache.Add(target.whoAmI);
            if (RedeHelper.ClosestNPC(ref initialTarget, 500, target.Center, true, -1, (target) => !targetCache.Contains(target.whoAmI)))
            {
                int hitDirection = initialTarget.RightOfDir(Projectile);
                BaseAI.DamageNPC(target, Projectile.damage, Projectile.knockBack, hitDirection, Projectile, crit: Projectile.HeldItemCrit());

                int maxTime = Main.rand.Next(8, 12) * 4;
                ParticleSystem.NewParticle(target.Center, initialTarget.Center - target.Center, new ElectricParticle(maxTime, 120, 1), Color.LightYellow, 1);
            }
            else
            {
                int hitDirection = initialTarget.RightOfDir(Projectile);
                BaseAI.DamageNPC(target, Projectile.damage, Projectile.knockBack, hitDirection, Projectile, crit: Projectile.HeldItemCrit());

                Projectile.friendly = false;
                LaunchTrigger = true;
            }
        }
        public bool PossibleTarget(NPC nextTarget)
        {
            if (Projectile.localNPCImmunity[nextTarget.whoAmI] <= 0)
                return true;
            return false;
        }

        public float LaunchTimer;
        public float floating;
        NPC target2;
        public void Launch()
        {
            if (LaunchTimer++ == 0)
                Projectile.damage *= 5;

            Projectile.rotation += LaunchTimer * 0.0015f;

            if (Main.rand.NextBool(5))
            {
                for (int i = 0; i < 2; i++)
                    DustHelper.DrawParticleElectricity(Projectile.Center - new Vector2(20 * Projectile.direction, 0), Projectile.Center - new Vector2(20 * Projectile.direction, 0) + RedeHelper.PolarVector(Main.rand.Next(70, 121), RedeHelper.RandomRotation()), .8f, 10, 0.2f);
            }
            if (LaunchTimer < maxTime * 12 * Projectile.MaxUpdates)
            {
                Projectile.Move(Owner.Center + new Vector2(-Owner.direction * 200f, -100), 25, 20);
            }
            else
            {
                Projectile.friendly = true;
                Projectile.penetrate = 1;
                if (RedeHelper.ClosestNPC(ref target2, 1000, Projectile.Center))
                {
                    Projectile.Move(target2.Center, 25, 10);
                }
                else
                {
                    Projectile.Move(Owner.Center, 25, 2);
                    if (Vector2.Distance(Projectile.Center, Owner.Center) < 20f)
                        Projectile.Kill();
                }
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(CustomSounds.ChainCurrents, target.position);
            target.AddBuff(BuffType<ElectrifiedDebuff>(), 180);
            if (!TeleportTrigger)
            {
                TeleportTrigger = true;
                initialTarget = target;
            }
            Vector2 directionTo = target.DirectionTo(Projectile.Center);
            float num = LaunchTrigger ? 2f : 1;
            if (LaunchTrigger)
            {
                for (int i = 0; i < 8; i++)
                    Dust.NewDustPerfect(target.Center + directionTo * 5 + new Vector2(0, 70), DustType<DustSpark2>(), directionTo.RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f) + 3.14f) * Main.rand.NextFloat(4f * num, 5f * num), 0, Color.White * .8f, 3f);
            }
            if (Main.myPlayer == Projectile.owner)
            {
                int p = Projectile.NewProjectile(Projectile.GetSource_FromAI(), target.Center + new Vector2(Main.rand.NextFloat(-2, 2)), Vector2.Zero, ProjectileType<UkonArrowStrike>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 0, 1);
                Main.projectile[p].DamageType = DamageClass.Melee;
                Main.projectile[p].localAI[0] = 35;
                Main.projectile[p].alpha = 0;
                Main.projectile[p].position.Y -= 540;
                Main.projectile[p].frame = 12;
                Main.projectile[p].netUpdate = true;
            }
        }
        private float drawTimer;
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteEffects spriteEffects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = new(texture.Width / 2f, texture.Height / 2f);
            Vector2 armCenter = Owner.RotatedRelativePoint(Owner.MountedCenter) + new Vector2(Owner.direction * -4, -4);
            Vector2 drawPos = !LaunchTrigger ?  armCenter + positionVector.SafeNormalize(default) * 24 * Projectile.scale : Projectile.Center;
            float opacity = EaseFunction.EaseQuadIn.Ease(Utils.GetLerpValue(0, maxTime * 12 * Projectile.MaxUpdates, LaunchTimer, true));
            Color c = Color.Lerp(Color.Transparent, Color.LightYellow with { A = 0 }, opacity);
            if(LaunchTrigger)
                Main.EntitySpriteDraw(texture, drawPos - Main.screenPosition, null, Projectile.GetAlpha(c), Projectile.rotation, origin, Projectile.scale * 1.5f, spriteEffects, 0);
            Main.EntitySpriteDraw(texture, drawPos - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);
            return false;
        }
    }
}
