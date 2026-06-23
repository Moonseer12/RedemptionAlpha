using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.Utilities;
using Redemption.Base;
using Redemption.Buffs.Minions;
using Redemption.Buffs.NPCBuffs;
using Redemption.Globals;
using Redemption.Particles;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Projectiles.Minions
{
    public class LightningStoneMinion : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            ElementID.ProjThunder[Type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Summon;
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.penetrate = -1;
            Projectile.minion = true;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.minionSlots = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }
        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(BuffType<LightningStoneBuff>());
                return false;
            }
            if (owner.HasBuff(BuffType<LightningStoneBuff>()))
                Projectile.timeLeft = 2;
            return true;
        }
        public override bool MinionContactDamage() => true;

        public NPC target;
        public bool detect;
        public float launchCooldown;
        public Player Owner => Main.player[Projectile.owner];
        public override void AI()
        {
            if (!CheckActive(Owner))
                return;
            ProjHelper.OverlapCheck(Projectile);
            Lighting.AddLight(Projectile.Center, Projectile.Opacity * .5f, Projectile.Opacity * .5f, Projectile.Opacity * 0);
            Projectile.rotation += Projectile.velocity.X * 0.05f;
            Projectile.LookByVelocity();

            if (Main.myPlayer == Owner.whoAmI && Projectile.DistanceSQ(Owner.Center) > 2000 * 2000)
            {
                Projectile.position = Owner.Center;
                Projectile.velocity *= 0.1f;
                Projectile.netUpdate = true;
            }

            if (Projectile.ai[0] == 0)
            {
                Projectile.velocity.Y += 1f;
            }
            if (Projectile.ai[0] == 1)
            {
                Projectile.velocity.Y += 1f;
                if (Projectile.ai[1]++ > 5)
                    Projectile.ai[0] = 2;
            }
            if (Projectile.ai[0] == 2)
            {
                glowOpacity -= .05f;
                glowOpacity = MathHelper.Max(glowOpacity, 0);

                launchCooldown--;
                if (RedeHelper.ClosestNPC(ref target, 800, Projectile.Center, false, Owner.MinionAttackTargetNPC))
                {
                    if (launchCooldown <= 0)
                    {
                        Projectile.Move(target.Center, 20, 10);
                        if (RedeHelper.ClosestNPC(ref target, 150, Projectile.Center, true, Owner.MinionAttackTargetNPC))
                        {
                            if (launchCooldown <= 0)
                            {
                                Projectile.Move(target.Center, 35, 0);
                                launchCooldown = 10;
                            }
                        }
                    }
                }
                else
                {
                    if (Projectile.DistanceSQ(Owner.Center) >= 80 * 80)
                        Projectile.Move(Owner.Center, MathHelper.Lerp(10, 18, Projectile.Distance(Owner.Center) / 80));
                }
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.ai[0] = 1;
            Projectile.friendly = true;
            Projectile.tileCollide = false;

            Collision.HitTiles(Projectile.position, oldVelocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);

            if (Projectile.velocity.X != oldVelocity.X)
                Projectile.velocity.X = -oldVelocity.X;
            if (Projectile.velocity.Y != oldVelocity.Y)
                Projectile.velocity.Y = -oldVelocity.Y;
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 startPos = target.Center;
            Vector2 direction = target.DirectionFrom(Projectile.Center);

            float randomRotation = Main.rand.NextFloat(-0.5f, 0.5f);
            float randomVel = Main.rand.NextFloat(2f, 3f);

            Vector2 position = target.Center - direction * 10;
            RedeParticleManager.CreateSpeedParticle(position, direction.RotatedBy(randomRotation) * randomVel * 8, 1, Color.LightYellow.WithAlpha(0));

            for (int i  = 0; i < 3; ++i)
            {
                float randomRotation2 = Main.rand.NextFloat(-0.5f, 0.5f);
                float randomVel2 = Main.rand.NextFloat(2f, 3f);

                Dust dust8 = Dust.NewDustPerfect(startPos, DustID.YellowStarDust, direction.RotatedBy(randomRotation2) * 5 * randomVel2, 0);
                dust8.fadeIn = 0.4f + Main.rand.NextFloat() * 0.15f;
                dust8.noGravity = true;
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 5; i++)
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Sandnado);
        }
        private float drawTimer;
        public float glowOpacity;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Rectangle rect = new(0, 0, texture.Width, texture.Height);
            Vector2 drawOrigin = rect.Size() / 2;
            var effects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, new Rectangle?(rect), lightColor * Projectile.Opacity, Projectile.rotation, drawOrigin, Projectile.scale, effects, 0);
            if (glowOpacity > 0)
                RedeDraw.DrawTreasureBagEffect(Main.spriteBatch, texture, ref drawTimer, Projectile.Center - Main.screenPosition, new Rectangle?(rect), Color.LightGoldenrodYellow, Projectile.rotation, drawOrigin, Projectile.scale, opacity: Projectile.Opacity * glowOpacity);
            return false;
        }
    }
    public class LightningStoneMinionGlobalNPC : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public static int GetMinionCount() => Main.projectile.Count(p => p.active && p.type == ProjectileType<LightningStoneMinion>());
        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (ProjectileID.Sets.IsAWhip[projectile.type] && projectile.CountsAsClass(DamageClass.Summon))
            {
                int count = GetMinionCount();
                if (count > 0)
                {
                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        Projectile proj = Main.projectile[i];
                        if (proj.active && proj.type == ProjectileType<LightningStoneMinion>() && proj.ModProjectile is LightningStoneMinion stoneMinion && stoneMinion.glowOpacity <= 0)
                        {
                            stoneMinion.glowOpacity = 1;
                            if (!Main.dedServ)
                                SoundEngine.PlaySound(CustomSounds.Zap2 with { Volume = .5f }, proj.position);
                            DustHelper.DrawParticleElectricity(RedeHelper.RandAreaInEntity(proj), target.Center, .51f, 20, 0.05f, 1);
                            DustHelper.DrawParticleElectricity(RedeHelper.RandAreaInEntity(proj), target.Center, .5f, 20, 0.05f, 1);
                            int hitDirection = target.RightOfDir(proj);
                            target.AddBuff(BuffType<ElectrifiedDebuff>(), 10);
                            BaseAI.DamageNPC(target, proj.damage / 4, proj.knockBack * 2, hitDirection, proj);
                        }
                    }
                }
            }
        }
    }
}