using Microsoft.Xna.Framework.Graphics;
using Redemption.Base;
using Redemption.Effects;
using Redemption.Globals;
using Redemption.Particles;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Projectiles.Ranged
{
    public class GhastlyRecurve_Proj : ModProjectile, IDrawAdditive
    {
        public override string Texture => "Terraria/Images/NPC_" + NPCID.DungeonSpirit;
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Ghastly Spirit");
            Main.projFrames[Projectile.type] = 3;
        }
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 3600;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Ranged;
        }
        public override bool ShouldUpdatePosition() => false;
        private Vector2 origPos;
        private Vector2 targetPos;
        public override void OnSpawn(IEntitySource source)
        {
            origPos = Projectile.Center;
            if (Main.myPlayer == Projectile.owner)
                targetPos = Main.MouseWorld;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (++Projectile.frameCounter >= 5)
            {
                Projectile.frameCounter = 0;
                if (++Projectile.frame >= 3)
                    Projectile.frame = 0;
            }
            int d2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.DungeonSpirit, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f);
            Main.dust[d2].noGravity = true;

            if (Projectile.timeLeft > 3600 - 60)
            {
                float p = EaseFunction.EaseQuinticOut.Ease(Utils.GetLerpValue(3600, 3600 - 60, Projectile.timeLeft, true));
                Projectile.Center = Vector2.Lerp(origPos, targetPos, p);
            }
            else
                Projectile.velocity *= 0;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (!proj.active || !proj.arrow || !proj.friendly || proj.type == ProjectileType<SpiritArrow_Proj>() || proj.type == ProjectileType<SpiritArrow_Shard>() || proj.type == Type || proj.type == ProjectileID.PhantasmArrow)
                    continue;

                float point = 0;
                if (other == null || !other.active || other.type != Type || !Collision.CheckAABBvLineCollision(proj.position, proj.Size, Projectile.Center, other.Center, 20 + proj.velocity.Length(), ref point))
                    continue;

                SoundEngine.PlaySound(SoundID.Zombie53 with { Volume = 0.6f }, proj.Center);
                for (int j = 0; j < 10; j++)
                {
                    int d = Dust.NewDust(proj.position, proj.width, proj.height, DustID.DungeonSpirit, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f);
                    Main.dust[d].velocity *= 3f;
                    Main.dust[d].noGravity = true;
                    Vector2 vel = proj.velocity.RotateRandom(1) * Main.rand.NextFloat(0.1f, 1);
                    RedeParticleManager.CreateSharpParticle(proj.position, vel, 0.5f, Color.LightCyan);
                }
               
                proj.active = false;
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), proj.Center, proj.velocity, ProjectileType<SpiritArrow_Proj>(), proj.damage, proj.knockBack, player.whoAmI);
            }
        }
        public Projectile other;
        public void AdditiveCall(SpriteBatch sB, Vector2 screenPos)
        {
            if (other != null && other.active && other.type == Type)
                DrawTether(other, screenPos, new Color(35, 200, 254) * .2f, new Color(196, 247, 255), 10, 1);
        }
        public void DrawTether(Projectile Target, Vector2 screenPos, Color color1, Color color2, float Size, float Strength)
        {
            Effect effect = Request<Effect>("Redemption/Effects/Beam").Value;
            effect.Parameters["uTexture"].SetValue(Request<Texture2D>("Redemption/Textures/Trails/Trail_1").Value);
            effect.Parameters["progress"].SetValue(Main.GlobalTimeWrappedHourly / 3);
            effect.Parameters["uColor"].SetValue(color1.ToVector4());
            effect.Parameters["uSecondaryColor"].SetValue(color2.ToVector4());
            Vector2 dist = Target.Center - Projectile.Center;
            TrianglePrimitive tri = new()
            {
                TipPosition = Projectile.Center - screenPos,
                Rotation = dist.ToRotation(),
                Height = Size + 20 + dist.Length() * 1.5f,
                Color = Color.White * Strength,
                Width = Size + ((Target.width + Target.height))
            };
            PrimitiveRenderer.DrawPrimitiveShape(tri, effect);
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 255, 255, 0) * Projectile.Opacity;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.DungeonSpirit, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, Scale: 2);
                Main.dust[d].noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.NPCDeath39 with { Volume = 0.4f }, Projectile.position);
        }
    }
}
