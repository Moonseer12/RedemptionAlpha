using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.Core;
using ParticleLibrary.Utilities;
using Redemption.Base;
using Redemption.Globals;
using Redemption.Particles;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Items.Weapons.HM.Melee
{
    public class CyberChakram_Proj : ModProjectile
    {
        public override string Texture => "Redemption/Items/Weapons/HM/Melee/CyberChakram";
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Cyber Chakram");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
            ElementID.ProjThunder[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.light = 0.5f;
            Projectile.extraUpdates = 1;
            Projectile.tileCollide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Player player = Main.player[Projectile.owner];
            Projectile.velocity = Projectile.velocity.SafeNormalize(default) * 12f * MathF.Max(1, MathF.Pow(player.GetAttackSpeed(DamageClass.Melee), 0.5f));
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.damage = (int)(Projectile.damage * 0.95f);

            float rotation = Main.rand.NextFloat(6.28f);
            Vector2 drawPos = Main.rand.NextVector2FromRectangle(target.Hitbox);
            Vector2 randVel = rotation.ToRotationVector2();
            RedeParticleManager.CreateSlashParticle(drawPos, randVel * 10, .75f, Color.LightCyan, 8);
            RedeParticleManager.CreateSlashParticle(drawPos, randVel.RotatedBy(MathF.PI / 3) * 10, .75f, Color.LightCyan, 8);
            RedeParticleManager.CreateSlashParticle(drawPos, randVel.RotatedBy(MathF.PI * 2 / 3) * 10, .75f, Color.LightCyan, 8);
        }
        int num = 4;
        public override void AI()
        {
            Player p = Main.player[Projectile.owner];
            BaseAI.AIBoomerang(Projectile, ref Projectile.ai, p.position, p.width, p.height, true, 30, 40, 2f, 2f, false);

            if (Projectile.localAI[0]++ % 6 == num && Projectile.ai[0] == 0 && Projectile.owner == Main.myPlayer)
            {
                num = Main.rand.Next(2, 6);
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center.X, Projectile.Center.Y, 0, 0, ProjectileType<CyberChakram_Proj2>(), Projectile.damage, 0, Main.myPlayer, Projectile.rotation, Projectile.direction);
            }
        }
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            width = height = 20;
            return true;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Projectile.localAI[0] >= 35)
                modifiers.Knockback *= 0;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Texture2D glow = Request<Texture2D>(Texture + "_Glow").Value;
            Texture2D trail = Request<Texture2D>(Texture + "_Proj2").Value;
            Vector2 drawOrigin = new(texture.Width / 2, texture.Height / 2);
            SpriteEffects effects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Main.spriteBatch.End();
            Main.spriteBatch.BeginAdditive();

            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Color.White * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                float scale = Projectile.scale;
                Main.EntitySpriteDraw(trail, drawPos, null, Projectile.GetAlpha(color), Projectile.rotation, drawOrigin, scale, effects, 0);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.BeginDefault();

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, drawOrigin, Projectile.scale, effects, 0);
            Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(Color.White), Projectile.rotation, drawOrigin, Projectile.scale, effects, 0);
            return false;
        }
        public override bool OnTileCollide(Vector2 velocityChange)
        {
            Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            BaseAI.TileCollideBoomerang(Projectile, ref velocityChange, true);
            return false;
        }

    }
}
