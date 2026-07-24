using Microsoft.Xna.Framework.Graphics;
using Redemption.BaseExtension;
using Redemption.Buffs.NPCBuffs;
using Redemption.Globals;
using Redemption.Textures;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Projectiles.Misc
{
    public class HellstoneBallista_Bolt : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
            ElementID.ProjFire[Type] = true;
            ElementID.ProjExplosive[Type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Default;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 3000 * 10;
            Projectile.extraUpdates = 10;
            Projectile.usesLocalNPCImmunity = true;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            var effects = Projectile.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Vector2 drawOrigin = new(texture.Width / 2, Projectile.height / 2);

            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Color.Orange * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, effects, 0);
            }

            Main.EntitySpriteDraw(texture, Projectile.position - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, drawOrigin, Projectile.scale, effects, 0);
            return false;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            width = height = 6;
            return true;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.ai[1] > 0)
                return;
            target.AddBuff(BuffID.OnFire3, 156);
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Pitch = -0.5f }, Projectile.position);
            RedeDraw.SpawnExplosion(Projectile.Center, Color.OrangeRed, DustID.Torch, 10, scale: 2, tex: "Redemption/Textures/BigFlare", rot: RedeHelper.RandomRotation());
            RedeHelper.NPCRadiusDamage(Projectile.Center, 300, Projectile, Projectile.damage, 8, Projectile.CritChance);
            Projectile.ai[1] = 10;
        }
        public override void AI()
        {
            Projectile.ai[1]--;
            Vector2 position = Projectile.Center + (Vector2.Normalize(Projectile.velocity) * 10f);
            Dust dust20 = Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 0, default, Main.rand.NextFloat(1, 2f))];
            dust20.position = position;
            dust20.velocity = (Projectile.velocity.RotatedBy(2) * 0.1f) + (Projectile.velocity / 2);
            dust20.position += Projectile.velocity.RotatedBy(MathHelper.PiOver2) / 2;
            dust20.fadeIn = 0.5f;
            dust20.noGravity = true;
            dust20 = Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 0, default, Main.rand.NextFloat(1f, 2f))];
            dust20.position = position;
            dust20.velocity = (Projectile.velocity.RotatedBy(-2) * 0.1f) + (Projectile.velocity / 2);
            dust20.position += Projectile.velocity.RotatedBy(-MathHelper.PiOver2) / 2;
            dust20.fadeIn = 0.5f;
            dust20.noGravity = true;

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (Projectile.timeLeft < 120 * 10)
                Projectile.velocity.Y += 0.06f / 10;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Collision.HitTiles(Projectile.position, oldVelocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Pitch = -0.5f, Volume = 2 }, Projectile.position);
            RedeDraw.SpawnExplosion(Projectile.Center, Color.OrangeRed, DustID.Torch, 10, scale: 2, tex: "Redemption/Textures/BigFlare", rot: RedeHelper.RandomRotation());
            RedeHelper.NPCRadiusDamage(Projectile.Center, 300, Projectile, Projectile.damage, 8, Projectile.CritChance);
            return true;
        }
    }
}