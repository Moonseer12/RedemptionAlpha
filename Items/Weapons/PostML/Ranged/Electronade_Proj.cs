using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Redemption.Base;
using Redemption.Buffs.NPCBuffs;
using Redemption.Globals;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Items.Weapons.PostML.Ranged
{
    public class Electronade_Proj : ModProjectile
    {
        public override string Texture => "Redemption/Items/Weapons/PostML/Ranged/Electronade";
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Electonade");
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 1;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 190;
        }
        public override void AI()
        {
            Projectile.LookByVelocity();
            Projectile.rotation += Projectile.velocity.X / 20;
            Projectile.velocity.Y += 0.2f;
        }
        public override void Kill(int timeLeft)
        {
            if (!Main.dedServ)
                SoundEngine.PlaySound(SoundLoader.GetLegacySoundSlot(Mod, "Sounds/Custom/ElectricNoise"), Projectile.position);

            DustHelper.DrawCircle(Projectile.Center, DustID.Electric, 3, 3, 3, 1, 1, nogravity: true);
            for (int i = 0; i < 20; i++)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f);
                Main.dust[dust].velocity *= 5;
                Main.dust[dust].noGravity = true;
            }
            if (Projectile.owner == Main.myPlayer)
                Projectile.NewProjectile(Projectile.InheritSource(Projectile), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<Electronade_TeslaField>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.velocity.X != oldVelocity.X)
            {
                if (oldVelocity.X > 4 || oldVelocity.X < -4)
                    SoundEngine.PlaySound(SoundID.Tink, Projectile.position);

                Projectile.velocity.X = -oldVelocity.X;
            }
            if (Projectile.velocity.Y != oldVelocity.Y)
            {
                if (oldVelocity.Y > 4 || oldVelocity.Y < -4)
                    SoundEngine.PlaySound(SoundID.Tink, Projectile.position);

                Projectile.velocity.Y = -oldVelocity.Y;
            }
            Projectile.velocity.Y *= 0.3f;
            Projectile.velocity.X *= 0.7f;
            return false;
        }
    }
    public class Electronade_TeslaField : ModProjectile
    {
        public override string Texture => "Redemption/Textures/StaticBall";
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Tesla Field");
            Main.projFrames[Projectile.type] = 3;
        }
        public override void SetDefaults()
        {
            Projectile.width = 164;
            Projectile.height = 164;
            Projectile.penetrate = -1;
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 180;
            Projectile.GetGlobalProjectile<RedeProjectile>().Unparryable = true;
        }
        public override void AI()
        {
            if (++Projectile.frameCounter >= 5)
            {
                Projectile.frameCounter = 0;
                if (++Projectile.frame >= 3)
                    Projectile.frame = 0;
            }
            Projectile.rotation += 0.01f;

            if (Projectile.timeLeft > 30 && Main.rand.NextBool(10))
            {
                DustHelper.DrawElectricity(Projectile.Center, Projectile.Center + RedeHelper.PolarVector(90, Main.rand.NextFloat(0, MathHelper.TwoPi)), DustID.Electric, 1, 20, default, 0.2f);
                DustHelper.DrawElectricity(Projectile.Center, Projectile.Center + RedeHelper.PolarVector(90, Main.rand.NextFloat(0, MathHelper.TwoPi)), DustID.Electric, 1, 20, default, 0.2f);
            }

            if (Projectile.timeLeft <= 60)
                Projectile.alpha += 5;
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(ModContent.BuffType<ElectrifiedDebuff>(), 360);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            int height = texture.Height / 3;
            int y = height * Projectile.frame;
            Rectangle rect = new(0, y, texture.Width, height);
            Vector2 drawOrigin = new(texture.Width / 2, height / 2);
            SpriteEffects effects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            float scale = BaseUtility.MultiLerp(Main.LocalPlayer.miscCounter % 100 / 100f, 1f, 1.3f, 1f);
            float scale2 = BaseUtility.MultiLerp(Main.LocalPlayer.miscCounter % 100 / 100f, 1.3f, 1f, 1.3f);
            Color color = BaseUtility.MultiLerpColor(Main.LocalPlayer.miscCounter % 100 / 100f, Color.LightCyan, Color.Cyan, Color.LightCyan);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, new Rectangle?(rect), Projectile.GetAlpha(color), Projectile.rotation, drawOrigin, Projectile.scale * scale, effects, 0);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, new Rectangle?(rect), Projectile.GetAlpha(color), -Projectile.rotation, drawOrigin, Projectile.scale * scale2, effects, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
    }
}