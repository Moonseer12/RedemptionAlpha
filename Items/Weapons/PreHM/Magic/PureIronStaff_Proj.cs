using Microsoft.Xna.Framework.Graphics;
using Redemption.BaseExtension;
using Redemption.Globals;
using Redemption.Projectiles.Magic;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Items.Weapons.PreHM.Magic
{
    public class PureIronStaff_Proj : ModProjectile
    {
        public override string Texture => "Redemption/Items/Weapons/PreHM/Magic/PureIronStaff";
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Pure-Iron Staff");
        }
        public override void SetDefaults()
        {
            Projectile.width = 38;
            Projectile.height = 44;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.alpha = 255;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.ownerHitCheck = true;
            Projectile.ignoreWater = true;
            Projectile.Redemption().TechnicallyMelee = true;
        }
        private int maxTime;
        private float speedBonus;
        public override void OnSpawn(IEntitySource source)
        {
            Player player = Main.player[Projectile.owner];
            maxTime = (int)(player.HeldItem.useTime / player.GetAttackSpeed(DamageClass.Magic));
            speedBonus = 26f / maxTime;
        }
        public bool glow;
        public float glowTimer;
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter);
            float num = 0;
            if (Projectile.spriteDirection == -1)
                num = MathHelper.ToRadians(90f);

            if (!player.channel)
                DelayKill();

            ProjHelper.HoldOutProjBasics(Projectile, player, playerCenter);

            if (glow)
            {
                glowTimer += speedBonus;
                if (glowTimer > 60)
                {
                    glow = false;
                    glowTimer = 0;
                }
            }

            Projectile.Center = playerCenter + RedeHelper.PolarVector(30, Projectile.velocity.ToRotation());
            Projectile.rotation = Projectile.velocity.ToRotation() + num + MathHelper.PiOver4;
            Projectile.spriteDirection = Projectile.direction;
            Projectile.timeLeft = 2;
            player.ChangeDir(Projectile.direction);
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;
            player.itemRotation = (float)Math.Atan2(Projectile.velocity.Y * Projectile.direction, Projectile.velocity.X * Projectile.direction);

            if (Projectile.localAI[0]++ == 0)
            {
                Projectile.alpha = 0;
                SoundEngine.PlaySound(SoundID.Item30, player.position);
                glow = true;
                if (Projectile.owner == Main.myPlayer)
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), player.Center + Vector2.Normalize(Projectile.velocity) * 35f, Projectile.velocity, ProjectileType<IceBolt>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 0, Projectile.whoAmI);
            }
        }
        public void DelayKill()
        {
            Projectile.localAI[1]++;
            if (Projectile.localAI[1] > maxTime)
                Projectile.Kill();
        }
        private float Opacity { get => glowTimer; set => glowTimer = value; }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new(texture.Width / 2, Projectile.height / 2);
            SpriteEffects spriteEffects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, drawOrigin, Projectile.scale, spriteEffects, 0);
            return false;
        }
        public override void PostDraw(Color lightColor)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.BeginAdditive();

            Texture2D texture = Request<Texture2D>("Redemption/Textures/Star").Value;
            Vector2 origin = new(texture.Width / 2f, texture.Height / 2f);
            Vector2 position = Projectile.Center - Main.screenPosition + RedeHelper.PolarVector(15, Projectile.velocity.ToRotation());
            Color colour = Color.Lerp(Color.LightBlue, Color.LightCyan, 1f / Opacity * 10f) * (1f / Opacity * 10f);
            SpriteEffects spriteEffects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            if (glow)
            {
                Main.EntitySpriteDraw(texture, position, null, colour, Projectile.rotation, origin, 0.8f, spriteEffects, 0);
                Main.EntitySpriteDraw(texture, position, null, colour * 0.4f, Projectile.rotation, origin, 1, spriteEffects, 0);
            }
            Main.spriteBatch.End();
            Main.spriteBatch.BeginDefault(true);
        }
    }
}
