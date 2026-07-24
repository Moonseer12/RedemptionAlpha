using Microsoft.Xna.Framework.Graphics;
using Redemption.Buffs.Minions;
using Redemption.Globals;
using Redemption.Particles;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Projectiles.Minions
{
    public class AkkaRosePortal : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 2;

            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            ElementID.ProjArcane[Type] = true;
            ElementID.ProjNature[Type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.width = 44;
            Projectile.height = 44;
            Projectile.penetrate = -1;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.hide = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = Projectile.SentryLifeTime;
            Projectile.sentry = true;
            Projectile.DamageType = DamageClass.Summon;
        }
        public override bool ShouldUpdatePosition() => false;
        public override bool? CanDamage() => false;
        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(BuffType<AkkaRoseSentryBuff>());
                return false;
            }

            if (!owner.HasBuff(BuffType<AkkaRoseSentryBuff>()))
                Projectile.Kill();

            return true;
        }
        private float shaderTimer;
        private float shaderProgress;
        private float noiseOffset;
        private ref float Timer => ref Projectile.ai[0];
        public override void AI()
        {
            leafSway = (float)(Math.Sin(Main.GlobalTimeWrappedHourly) / 2);
            leafSway2 = (float)(Math.Sin(Main.GlobalTimeWrappedHourly + .5f) / 3);
            Player player = Main.player[Projectile.owner];
            if (!CheckActive(player))
                return;

            if (Timer == 0)
                noiseOffset = Main.rand.NextFloat(0.25f, 0.75f);

            if (Timer++ < 60)
            {
                FadeIn();
            }
            if (Timer > 30)
            {
                if (Timer % 7 == 0)
                {
                    NPC target = null;
                    if (RedeHelper.ClosestNPC(ref target, 2000, Projectile.Center))
                    {
                        pulse += 0.5f;

                        Vector2 pos = Projectile.Center + RedeHelper.Spread(20);
                        Vector2 vel = pos.DirectionTo(target.Center);
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), pos, vel * 12, ProjectileType<AkkaRosePortal_Proj>(), Projectile.damage, Projectile.knockBack, Projectile.owner);

                        Vector2 particleVel = Vector2.UnitX.RotateRandom(6.28f);
                        RedeParticleManager.CreateSlashParticle(pos, particleVel * 20, .25f, Color.Red);
                        RedeParticleManager.CreateSlashParticle(pos, particleVel.RotatedBy(1.57f) * 20, .25f, Color.Red);

                        for (int k = 0; k < 4; k++)
                        {
                            Dust dust = Dust.NewDustPerfect(pos, DustID.GrassBlades, particleVel.RotatedBy(k * 1.57f));
                            dust.noGravity = true;
                        }
                    }
                }
            }
            pulse -= 0.05f;
            pulse = MathHelper.Clamp(pulse, 1, 1.1f);
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindProjectiles.Add(index);
        }
        public float drawTimer;
        public float pulse = 1;
        float leafSway;
        float leafSway2;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Rectangle rect = texture.Frame(1, 2, 0, 0);
            Rectangle rectLeaf = new Rectangle(0, 44, 24, 20);
            Rectangle rectLeaf2 = new Rectangle(0, 68, 20, 20);
            Vector2 origin = rect.Size() / 2;
            Vector2 originLeaf = rectLeaf.Size() / 2 + new Vector2(6, 6);
            Vector2 originLeaf2 = rectLeaf2.Size() / 2 - new Vector2(6, 8);

            if (Timer > 30)
            {
                Color col = lightColor * Utils.GetLerpValue(30, 60, Timer, true);
                RedeDraw.DrawTreasureBagEffect(Main.spriteBatch, texture, ref drawTimer, Projectile.Center - Main.screenPosition, new Rectangle?(rect), Projectile.GetAlpha(col), Projectile.rotation, origin, Projectile.scale * 0.9f * pulse);
            }
            Main.spriteBatch.End();
            Main.spriteBatch.BeginDefault(true);

            Effect effect = Request<Effect>("Redemption/Effects/NoisyFade").Value;

            Texture2D noise = Request<Texture2D>("Redemption/Textures/Noise/swirlnoiseharsh").Value;

            Color outline = Color.OrangeRed;
            effect.Parameters["outlineColor"].SetValue(outline.ToVector4());
            effect.Parameters["uImageSize0"].SetValue(rect.Size());
            effect.Parameters["uSourceRect"].SetValue(new Vector4(rect.X, rect.Y, rect.Width, rect.Height));
            effect.Parameters["uImageSize1"].SetValue(new Vector2(.5f, .5f));
            effect.Parameters["progress"].SetValue(EaseFunction.EaseCubicOut.Ease(shaderProgress));
            effect.Parameters["noiseOffset"].SetValue(Vector2.One * noiseOffset);
            effect.Parameters["outlineWidth"].SetValue(0.05f);

            Main.graphics.GraphicsDevice.Textures[1] = noise;
            effect.CurrentTechnique.Passes[0].Apply();
            Main.EntitySpriteDraw(texture, Projectile.Center - new Vector2(14, 8) - Main.screenPosition, rectLeaf, Projectile.GetAlpha(lightColor), leafSway, originLeaf, Projectile.scale, 0, 0);
            Main.EntitySpriteDraw(texture, Projectile.Center + new Vector2(4, 14) - Main.screenPosition, rectLeaf2, Projectile.GetAlpha(lightColor), leafSway2, originLeaf2, Projectile.scale, 0, 0);

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, rect, Projectile.GetAlpha(lightColor), Projectile.rotation + (float)(Math.Sin(Main.GlobalTimeWrappedHourly / 2)), origin, Projectile.scale, 0, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.BeginDefault(true);
            return false;
        }
        private void FadeIn()
        {
            shaderProgress = shaderTimer / 60f;
            shaderTimer++;
        }
    }
    public class AkkaRosePortal_Proj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionShot[Projectile.type] = true;
            ElementID.ProjArcane[Type] = true;
            ElementID.ProjNature[Type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Summon;
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.frame = Main.rand.Next(4);

            Projectile.tileCollide = true;
            Projectile.friendly = true;
            Projectile.hostile = false;

            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.extraUpdates = 1;
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + 1.57f;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 usePos = Projectile.position;
            Vector2 rotVector = (Projectile.rotation - MathHelper.ToRadians(90f)).ToRotationVector2();
            usePos += rotVector * 8f;
            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustDirect(usePos, Projectile.width, Projectile.height, DustID.GrassBlades);
                dust.position = (dust.position + Projectile.Center) / 2f;
                dust.velocity += rotVector * 2f;
                dust.velocity *= 0.5f;
                dust.noGravity = true;
                usePos -= rotVector * 8f;
            }
            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustDirect(usePos, Projectile.width, Projectile.height, DustID.SomethingRed);
                dust.position = (dust.position + Projectile.Center) / 2f;
                dust.velocity += rotVector * 2f;
                dust.velocity *= 0.5f;
                dust.noGravity = true;
                usePos -= rotVector * 8f;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> texture = TextureAssets.Projectile[Type];
            Rectangle rect = texture.Frame(4, 1, Projectile.frame, 0);
            Vector2 origin = rect.Size() / 2;
            Main.EntitySpriteDraw(texture.Value, Projectile.Center - Main.screenPosition, rect, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, 0);
            return false;
        }
    }
}