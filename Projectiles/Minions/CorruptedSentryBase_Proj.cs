using Microsoft.Xna.Framework.Graphics;
using Redemption.Buffs.Minions;
using Redemption.Globals;
using Redemption.UI;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Projectiles.Minions
{
    public class CorruptedSentryBase_Proj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.width = 52;
            Projectile.height = 72;
            Projectile.hide = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = Projectile.SentryLifeTime;

            Projectile.DamageType = DamageClass.Summon;
            Projectile.friendly = true;
            Projectile.sentry = true;
            Projectile.penetrate = -1;
        }
        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => false;
        public override bool ShouldUpdatePosition() => false;
        private int projBuffed;
        private float sentryHeight = 20;
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            if (!CheckActive(owner))
                return;

            Lighting.AddLight(Projectile.Center, Projectile.Opacity * 0.2f, Projectile.Opacity * 0, Projectile.Opacity * 0);

            AI_GetMyGroupIndexAndFillBlackList(null, out int index, out int total);
            Projectile.Center = owner.RotatedRelativePoint(owner.MountedCenter) + RedeHelper.PolarVector(200, 6.28f / total * index - 1.57f * total);
            Projectile.localAI[1]++;

            if (Projectile.localAI[0] == 0)
            {
                foreach (Projectile proj in Main.ActiveProjectiles)
                {
                    if (!proj.active || !proj.sentry || proj.type == Type || proj.GetGlobalProjectile<CorruptedSentryBase_Proj_Global>().SentryBaseBuffed > 0)
                        continue;

                    projBuffed = proj.whoAmI;
                    Projectile.localAI[0] = 1;
                }
            }
            if (Projectile.localAI[0] == 1 && projBuffed >= 0)
            {
                Projectile p = Main.projectile[projBuffed];
                if (!p.active || p.type == ProjectileType<CorruptedSentryBase_Proj>())
                {
                    Projectile.localAI[0] = 0;
                    sentryHeight = 20;
                }
                else
                {
                    sentryHeight = p.height + 20;
                    p.velocity *= 0;
                    p.Bottom = Projectile.Center;
                    p.GetGlobalProjectile<CorruptedSentryBase_Proj_Global>().SentryBaseBuffed = 1;
                    for (int i = 0; i < 1; i++)
                    {
                        Rectangle rect = new((int)p.BottomLeft.X, (int)p.BottomLeft.Y + 2, p.width, 2);
                        Dust d = Dust.NewDustPerfect(Main.rand.NextVector2FromRectangle(rect), DustID.RedTorch, Vector2.UnitY * -2);
                        d.noGravity = true;
                    }
                }
            }
        }
        private void AI_GetMyGroupIndexAndFillBlackList(List<int> blackListedTargets, out int index, out int totalIndexesInGroup)
        {
            index = 0;
            totalIndexesInGroup = 0;
            for (int i = 0; i < 1000; i++)
            {
                Projectile projectile = Main.projectile[i];
                if (projectile.active && projectile.owner == Projectile.owner && projectile.type == Projectile.type && (projectile.type != ProjectileID.BabyBird || projectile.frame == Main.projFrames[projectile.type] - 1))
                {
                    if (Projectile.whoAmI > i)
                    {
                        index++;
                    }
                    totalIndexesInGroup++;
                }
            }
        }
        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(BuffType<CorruptedSentryBaseBuff>());
                return false;
            }

            if (!owner.HasBuff(BuffType<CorruptedSentryBaseBuff>()))
                Projectile.Kill();
            return true;
        }
        public override void OnKill(int timeLeft)
        {
            if (Projectile.localAI[0] == 1)
            {
                Projectile p = Main.projectile[projBuffed];
                p.Kill();
            }
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindProjectiles.Add(index);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> texture = TextureAssets.Projectile[Type];
            Rectangle leftRect = new(0, 0, 12, 14);
            Rectangle rightRect = new(14, 0, 12, 14);
            Rectangle midRect = new(12, 0, 2, 14);

            int width = 50;
            if (Projectile.localAI[0] == 1 && projBuffed >= 0)
            {
                Projectile p = Main.projectile[projBuffed];
                if (p.active && p.type != ProjectileType<CorruptedSentryBase_Proj>())
                {
                    width = p.width;
                }
            }

            Vector2 drawOrigin = new(midRect.Width / 2, 0);
            Main.EntitySpriteDraw(texture.Value, Projectile.Center - Main.screenPosition, midRect, Projectile.GetAlpha(Color.White), Projectile.rotation, drawOrigin, new Vector2(width / 2, Projectile.scale), 0, 0);
            drawOrigin = new(leftRect.Width / 2, 0);
            Main.EntitySpriteDraw(texture.Value, Projectile.Center - new Vector2(width / 2, 0) - Main.screenPosition, leftRect, Projectile.GetAlpha(Color.White), Projectile.rotation, drawOrigin, Projectile.scale, 0, 0);
            drawOrigin = new(rightRect.Width / 2, 0);
            Main.EntitySpriteDraw(texture.Value, Projectile.Center + new Vector2(width / 2, 0) - Main.screenPosition, rightRect, Projectile.GetAlpha(Color.White), Projectile.rotation, drawOrigin, Projectile.scale, 0, 0);
            return false;
        }
    }
    public class CorruptedSentryBase_Proj_Global : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public byte SentryBaseBuffed;
        public override bool PreAI(Projectile projectile)
        {
            if (SentryBaseBuffed <= 0)
                return base.PreAI(projectile);

            if (projectile.ContinuouslyUpdateDamageStats)
                projectile.damage = (int)(projectile.damage * 1.5f);
            return base.PreAI(projectile);
        }
    }
}