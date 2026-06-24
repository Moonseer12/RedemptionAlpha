using Microsoft.Xna.Framework.Graphics;
using Redemption.Base;
using Redemption.BaseExtension;
using Redemption.Buffs.Minions;
using Redemption.Buffs.NPCBuffs;
using Redemption.Globals;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Projectiles.Minions
{
    public class Ukkonen : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            //DisplayName.SetDefault("Ukkonen");
            Main.projFrames[Type] = 16;
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;

            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            ElementID.ProjThunder[Type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.width = 74;
            Projectile.height = 36;
            Projectile.penetrate = -1;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.minionSlots = 1;
            Projectile.alpha = 30;
        }

        NPC target;
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!CheckActive(player))
                return;
            ProjHelper.OverlapCheck(Projectile);

            glowOpacity -= .05f;
            glowOpacity = MathHelper.Max(glowOpacity, 0);
            if (++Projectile.frameCounter >= 4)
            {
                Projectile.frameCounter = 0;
                if (++Projectile.frame >= 16)
                    Projectile.frame = 0;
            }
            float sin = (float)(Math.Sin(Projectile.ai[1]++ / 20) * 20);
            Vector2 DefaultPos = new(player.Center.X + sin, player.Center.Y - 40 - (Projectile.minionPos * 30));
            if (RedeHelper.ClosestNPC(ref target, 900, Projectile.Center, true, player.MinionAttackTargetNPC))
            {
                Vector2 AttackPos = new(target.Center.X + sin, target.position.Y - 60 - (Projectile.minionPos * 30));
                Projectile.Move(AttackPos, 30, 2);
                if (Projectile.ai[1] % 20 == 0 && Projectile.owner == Main.myPlayer)
                {
                    int p = Projectile.NewProjectile(player.GetSource_FromThis(), RedeHelper.RandAreaInEntity(Projectile), new Vector2(0, 8), ProjectileID.RainFriendly, Projectile.damage, Projectile.knockBack, Projectile.owner);
                    Main.projectile[p].DamageType = DamageClass.Summon;
                    Main.projectile[p].netUpdate = true;
                    Main.projectile[p].penetrate = 6;
                    Main.projectile[p].usesIDStaticNPCImmunity = false;
                    Main.projectile[p].usesLocalNPCImmunity = true;
                    Main.projectile[p].localNPCHitCooldown = -1;
                    Main.projectile[p].Redemption().FromMinion = true;
                }
            }
            else
                Projectile.Move(DefaultPos, 30, 2);
            if (Main.myPlayer == player.whoAmI && Projectile.DistanceSQ(player.Center) > 2000 * 2000)
            {
                Projectile.position = player.Center;
                Projectile.velocity *= 0.1f;
                Projectile.netUpdate = true;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffType<ElectrifiedDebuff>(), 120);
        }
        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(BuffType<UkkonenBuff>());
                return false;
            }
            if (owner.HasBuff(BuffType<UkkonenBuff>()))
                Projectile.timeLeft = 2;
            return true;
        }

        public override bool MinionContactDamage() => false;
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 15; i++)
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Sandnado);
            for (int i = 0; i < 30; i++)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.RainCloud);
                Main.dust[d].noGravity = true;
            }
        }
        private float drawTimer;
        public float glowOpacity;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Texture2D glow = Request<Texture2D>(Texture + "_Glow").Value;
            int height = texture.Height / 16;
            int y = height * Projectile.frame;
            Rectangle rect = new(0, y, texture.Width, height);
            Vector2 drawOrigin = new(texture.Width / 2, Projectile.height / 2);
            var effects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos, new Rectangle?(rect), color, Projectile.rotation, drawOrigin, Projectile.scale, effects, 0);
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, new Rectangle?(rect), lightColor * Projectile.Opacity, Projectile.rotation, drawOrigin, Projectile.scale, effects, 0);
            Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, new Rectangle?(rect), Color.White * Projectile.Opacity, Projectile.rotation, drawOrigin, Projectile.scale, effects, 0);
            if (glowOpacity > 0)
                RedeDraw.DrawTreasureBagEffect(Main.spriteBatch, texture, ref drawTimer, Projectile.Center - Main.screenPosition, new Rectangle?(rect), Color.LightGoldenrodYellow, Projectile.rotation, drawOrigin, Projectile.scale, opacity: Projectile.Opacity * glowOpacity);
            return false;
        }
    }
    public class UkkonenGlobalNPC : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public static int GetUkkonenCount() => Main.projectile.Count(p => p.active && p.type == ProjectileType<Ukkonen>());
        public int ukkonenNum;
        public override bool PreAI(Projectile projectile)
        {
            ukkonenNum = Main.projectile.Count(p => p.active && p.type == ProjectileType<Ukkonen>());
            return base.PreAI(projectile);
        }
        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (ProjectileID.Sets.IsAWhip[projectile.type] && projectile.CountsAsClass(DamageClass.Summon))
            {
                if (ukkonenNum > 0)
                {
                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        Projectile proj = Main.projectile[i];
                        if (proj.active && proj.owner == projectile.owner && proj.type == ProjectileType<Ukkonen>() && proj.ModProjectile is Ukkonen ukkonen && ukkonen.glowOpacity <= 0)
                        {
                            ukkonen.glowOpacity = 1;
                            if (!Main.dedServ)
                                SoundEngine.PlaySound(CustomSounds.Zap2 with { Volume = .5f }, proj.position);
                            DustHelper.DrawParticleElectricity(RedeHelper.RandAreaInEntity(proj), target.Center, .51f, 20, 0.05f, 1);
                            DustHelper.DrawParticleElectricity(RedeHelper.RandAreaInEntity(proj), target.Center, .5f, 20, 0.05f, 1);
                            int hitDirection = target.RightOfDir(proj);
                            BaseAI.DamageNPC(target, proj.damage / 4, proj.knockBack * 2, hitDirection, proj);
                        }
                    }
                }
            }
        }
    }
}
