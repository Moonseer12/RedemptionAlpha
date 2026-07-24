using Microsoft.Xna.Framework.Graphics;
using Redemption.Biomes;
using Redemption.Globals;
using Redemption.Projectiles.Misc;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Tiles.Furniture.Bastion
{
    public class HellstoneBallista_Top : ModProjectile
    {
        public Tile Parent;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 5;
        }
        public override void SetDefaults()
        {
            Projectile.width = 92;
            Projectile.height = 44;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.ignoreWater = true;
        }
        public override void AI()
        {
            if (Projectile.frame > 0)
            {
                arrowOpacity = 0;
                if (Projectile.frameCounter++ >= 7)
                {
                    Projectile.frameCounter = 0;
                    if (++Projectile.frame > 4)
                        Projectile.frame = 0;
                }
            }

            if (!Parent.HasTile || Parent.TileType != TileType<HellstoneBallistaTile>())
                Projectile.Kill();
            else
                Projectile.timeLeft = 2;

            Projectile.velocity *= 0;

            NPC target = null;

            if (RedeHelper.ClosestNPC(ref target, 30000, Projectile.Center, false))
            {
                Projectile.LookAtEntity(target);
                Projectile.rotation.SlowRotation((target.Center - Projectile.Center).ToRotation() + (Projectile.spriteDirection == 1 ? 0 : MathHelper.Pi), MathHelper.Pi / 120);

                if (Projectile.ai[0] >= 110)
                    Projectile.ai[1] += .04f;
                if (Projectile.ai[0]++ >= 180)
                {
                    RedeDraw.SpawnExplosion(Projectile.Center, Color.OrangeRed, DustID.Torch, 10, scale: 1, tex: "Redemption/Textures/BigFlare", rot: RedeHelper.RandomRotation());

                    SoundEngine.PlaySound(SoundID.DD2_BallistaTowerShot with { Volume = 2, Pitch = -.7f }, Projectile.position);
                    Projectile.ai[0] = 0;
                    Projectile.ai[1] = 0;
                    Projectile.frame = 1;

                    if (Main.myPlayer == Projectile.owner)
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, RedeHelper.PolarVector(10 * Projectile.spriteDirection, Projectile.rotation), ProjectileType<HellstoneBallista_Bolt>(), 100, 0, Main.myPlayer);
                }
            }
            else
            {
                Projectile.ai[1] -= .01f;
                if (Projectile.ai[1] <= 0)
                    Projectile.ai[1] = 0;

                Projectile.rotation.SlowRotation(Projectile.spriteDirection == 1 ? 0 : 0, MathHelper.Pi / 120);
            }
            arrowOpacity += .05f;
            arrowOpacity = MathHelper.Min(arrowOpacity, 1);
        }

        Asset<Texture2D> arrowTex;
        float arrowOpacity;
        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> texture = TextureAssets.Projectile[Projectile.type];
            var effects = Projectile.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Rectangle rect = texture.Frame(1, Main.projFrames[Type], 0, Projectile.frame);

            Vector2 shaky = RedeHelper.Spread(Projectile.ai[1]);
            if (Projectile.frame == 0)
            {
                arrowTex ??= TextureAssets.Projectile[ProjectileType<HellstoneBallista_Bolt>()];
                Vector2 drawOrigin = arrowTex.Size() / 2;

                Main.EntitySpriteDraw(arrowTex.Value, Projectile.Center + shaky + RedeHelper.OffsetWithRotation(Projectile.rotation, 12 * Projectile.spriteDirection, -7) - Main.screenPosition, null, Projectile.GetAlpha(lightColor) * arrowOpacity, Projectile.rotation + (MathHelper.PiOver2 * Projectile.spriteDirection), drawOrigin, Projectile.scale, effects, 0);
            }

            Main.EntitySpriteDraw(texture.Value, Projectile.Center + shaky - Main.screenPosition, rect, Projectile.GetAlpha(lightColor), Projectile.rotation, rect.Size() / 2, Projectile.scale, effects, 0);
            return false;
        }
    }
}