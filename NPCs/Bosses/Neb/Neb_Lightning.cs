using Microsoft.Xna.Framework.Graphics;
using Redemption.BaseExtension;
using Redemption.Buffs.NPCBuffs;
using Redemption.Globals;
using Redemption.Projectiles;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.NPCs.Bosses.Neb
{
    public class Neb_Lightning : ModProjectile
    {
        public override string Texture => Redemption.EMPTY_TEXTURE;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 2400;
            ElementID.ProjThunder[Type] = true;
            ElementID.ProjCelestial[Type] = true;
            ElementID.ProjArcane[Type] = true;
        }
        private int maxUpdates;
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = -1;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            maxUpdates = 25;
            Projectile.timeLeft = 90 + 150 * maxUpdates;
        }
        private ref float Progression => ref Projectile.ai[1];
        private Vector2 origVel;
        private Vector2[] nodesStraight;
        private Vector2[] nodesOriginal;
        private Vector2[] nodes;
        private void EvaluateNodes(int nodeLength)
        {
            Vector2 origPos = Projectile.Center;
            origVel = Vector2.UnitY * 3000;

            int nodeCount = (int)Vector2.Distance(origPos, origPos + origVel) / nodeLength;
            nodesOriginal = new Vector2[nodeCount + 1];
            nodesStraight = new Vector2[nodeCount + 1];
            nodes = new Vector2[nodeCount + 1];
            nodesOriginal[0] = origPos;
            nodesStraight[0] = origPos;
            nodesOriginal[nodeCount] = origPos + origVel;
            nodesStraight[nodeCount] = origPos + origVel;
            for (int k = 1; k < nodesOriginal.Length - 1; k++)
            {
                float rand = Main.rand.NextFloat(-nodeLength, nodeLength);
                nodesOriginal[k] = Vector2.Lerp(origPos, origPos + origVel, k / (float)nodeCount) + Vector2.Normalize(-origVel).RotatedBy(1.58f) * rand;
                nodesStraight[k] = Vector2.Lerp(origPos, origPos + origVel, k / (float)nodeCount) + Vector2.Normalize(-origVel).RotatedBy(0) * rand;
            }
        }
        public override void OnSpawn(IEntitySource source)
        {
            EvaluateNodes(60);
        }
        public Vector2 EvaluatePathByDistance(Vector2[] points, float t)
        {
            if (points == null || points.Length == 0)
                return Vector2.Zero;

            if (points.Length == 1)
                return points[0];

            t = Math.Clamp(t, 0f, 1f);

            int segmentCount = points.Length - 1;

            float scaledT = t * segmentCount;

            int segmentIndex = (int)MathF.Floor(scaledT);

            if (segmentIndex >= segmentCount)
                return points[^1];

            float localT = scaledT - segmentIndex;

            Vector2 a = points[segmentIndex];
            Vector2 b = points[segmentIndex + 1];

            return Vector2.Lerp(a, b, localT);
        }
        public override bool ShouldUpdatePosition() => Projectile.timeLeft > 150 * maxUpdates;
        private Color c;
        public override void AI()
        {
            c = new Color(100, 100, 255);
            if (Projectile.timeLeft > 150 * maxUpdates)
            {
                Projectile.velocity.X *= .98f;
                float progress = Utils.GetLerpValue(150 * maxUpdates, 90 + 150 * maxUpdates, Projectile.timeLeft, true);
                for (int k = 0; k < nodesOriginal.Length; k++)
                {
                    nodesOriginal[k] += Projectile.velocity;
                    nodesStraight[k] += Projectile.velocity;
                    nodes[k] = Vector2.Lerp(nodesOriginal[k], nodesStraight[k], EaseFunction.EaseQuadOut.Ease(progress));
                }
            }
            else
            {
                Projectile.MaxUpdates = maxUpdates;
                Projectile.velocity *= 0;
                if (Projectile.timeLeft == 150 * maxUpdates)
                {
                    Main.LocalPlayer.GetModPlayer<ScreenPlayer>().Rumble(10, 16);
                    if (!Main.dedServ)
                        SoundEngine.PlaySound(CustomSounds.Thunderstrike, Projectile.position);
                    Main.NewLightning();
                }

                Projectile.hostile = true;
                Progression += 150 / maxUpdates / (origVel.Length() + 1);
                Projectile.rotation = Projectile.Center.DirectionTo(EvaluatePathByDistance(nodes, Progression)).ToRotation();
                Projectile.Center = EvaluatePathByDistance(nodes, Progression);

                if (Main.rand.NextBool(30))
                    Dust.NewDust(Projectile.Center, Projectile.width, Projectile.height, DustID.Electric);
            }
            if (Progression >= 2)
                Projectile.Kill();

            if (Projectile.timeLeft == 90 + 150 * maxUpdates)
            {
                for (int k = oldPos.Length - 1; k >= 0; k--)
                {
                    oldPos[k] = Projectile.Center;
                    oldRot[k] = Projectile.rotation;
                }
            }
            for (int k = oldPos.Length - 1; k > 0; k--)
            {
                oldPos[k] = oldPos[k - 1];
                oldRot[k] = oldRot[k - 1];
            }
            oldPos[0] = Projectile.Center;
            oldRot[0] = Projectile.rotation;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffType<ElectrifiedDebuff>(), 180);
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.Electrified, 180);
        }
        private float[] oldRot = new float[400];
        private Vector2[] oldPos = new Vector2[400];
        public override bool PreDraw(ref Color lightColor)
        {
            //Texture2D glow = TextureAssets.Extra[ExtrasID.SharpTears].Value;
            Texture2D glow = Request<Texture2D>("Redemption/Textures/SoftGlow").Value;
            Rectangle rect = glow.Frame();
            Vector2 origin = rect.Size() * 0.5f;
            Color edge = Projectile.GetAlpha(c);

            Main.spriteBatch.End();
            Main.spriteBatch.BeginAdditive();

            if (Projectile.timeLeft > 150 * maxUpdates)
            {
                DrawTelegraph();
            }
            if (Progression > 0)
            {
                for (int k = 0; k < oldPos.Length; k++)
                {
                    Color color = edge * (1 - k / (float)oldPos.Length);
                    Vector2 scale = new Vector2(1, 3) * (1 - k / (float)oldPos.Length);
                    Main.EntitySpriteDraw(glow, oldPos[k] - Main.screenPosition, null, color, oldRot[k] + 1.57f, origin, scale, 0, 0);
                    Main.EntitySpriteDraw(glow, oldPos[k] - Main.screenPosition, null, Color.White, oldRot[k] + 1.57f, origin, scale * 0.5f, 0, 0);
                }
            }

            Main.spriteBatch.End();
            Main.spriteBatch.BeginDefault();

            return false;
        }
        private Vector2 telegraphPos;
        private void DrawTelegraph()
        {
            //Texture2D glow = TextureAssets.Extra[ExtrasID.SharpTears].Value;
            Texture2D glow = Request<Texture2D>("Redemption/Textures/SoftGlow").Value;
            Rectangle rect = glow.Frame();
            Vector2 origin = rect.Size() * 0.5f;
            Color edge = Projectile.GetAlpha(c);
            float opacity = Utils.GetLerpValue(90 + 150 * maxUpdates, 150 * maxUpdates, Projectile.timeLeft, true);
            for (float i = 0; i < 1f; i += 60 / maxUpdates / (origVel.Length() + 1))
            {
                telegraphPos = EvaluatePathByDistance(nodes, i);
                Color color = edge * (1) * opacity;
                Main.EntitySpriteDraw(glow, telegraphPos - Main.screenPosition, null, color, 0, origin, 0.25f, 0, 0);
            }
        }
    }
}