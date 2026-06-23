using Microsoft.Xna.Framework.Graphics;
using Redemption.BaseExtension;
using Redemption.Dusts;
using Redemption.Globals;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;

namespace Redemption.Items.Weapons.HM.Melee
{
    public class OversizedScrewdriver_Proj : TrueMeleeProjectile
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Oversized Screwdriver");
        }
        public override bool ShouldUpdatePosition() => false;
        public override void SetSafeDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.alpha = 255;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Length = 40;
            Projectile.usesLocalNPCImmunity = true;
        }
        private Vector2 startVector;
        private Vector2 vector;
        public ref float Length => ref Projectile.localAI[0];
        public ref float Rot => ref Projectile.localAI[1];
        public float Timer;
        public bool rotRight;
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (player.noItems || !player.channel || player.CCed || player.dead || !player.active)
                Projectile.Kill();

            if (Rot++ > 2)
                Projectile.alpha = 0;

            if (Projectile.frameCounter++ >= (Projectile.ai[0] == 0 ? 6 : 2))
            {
                Projectile.frameCounter = 0;
                if (++frame > 3)
                    frame = 0;
            }

            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;
            Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter, true);
            Projectile.Center = playerCenter + vector;

            if (Projectile.owner == Main.myPlayer)
            {
                if (Main.MouseWorld.X < player.Center.X)
                    player.direction = -1;
                else
                    player.direction = 1;
            }
            Projectile.spriteDirection = player.direction;
            if (Projectile.spriteDirection == 1)
                Projectile.rotation = (Projectile.Center - playerCenter).ToRotation() + MathHelper.PiOver2;
            else
                Projectile.rotation = (Projectile.Center - playerCenter).ToRotation() + MathHelper.PiOver2;

            switch (Projectile.ai[0])
            {
                case 0:
                    damageIncrease = 1;
                    break;
                case 1:
                    if (Timer++ == 4 && Projectile.owner == Main.myPlayer)
                        player.velocity = -Main.MouseWorld.DirectionTo(player.Center) * 10;
                    player.Redemption().contactImmune = true;
                    if (Timer >= 20)
                        Projectile.ai[0] = 0;
                    break;
            }
            if (Projectile.owner == Main.myPlayer)
            {
                startVector = -Main.MouseWorld.DirectionTo(player.Center);
                vector = startVector * Length;
            }
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, (player.Center - Projectile.Center).ToRotation() + MathHelper.PiOver2);
            player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, (player.Center - Projectile.Center).ToRotation() + MathHelper.PiOver2);
        }
        private float damageIncrease = 1;
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.localNPCImmunity[target.whoAmI] = 4;
            target.immune[Projectile.owner] = 0;

            Player player = Main.player[Projectile.owner];
            if (Projectile.owner == Main.myPlayer)
                player.velocity = Main.MouseWorld.DirectionTo(player.Center) * 7;
            if (player.channel)
            {
                if (Projectile.ai[0] == 1 && damageIncrease <= 3f)
                    damageIncrease += .04f;
                SoundEngine.PlaySound(SoundID.Item23, Projectile.position);
                Timer = 0;
                Projectile.ai[0] = 1;
            }
            Vector2 directionTo = target.DirectionTo(player.Center);
            Vector2 directionFrom = target.DirectionFrom(player.Center);
            Vector2 drawPos = Vector2.Lerp(Projectile.Center, target.Center, 0.8f);
            for (int i = 0; i < 5; i++)
                Dust.NewDustPerfect(drawPos + directionFrom * 10 + new Vector2(0, 45), DustType<DustSpark>(), directionFrom.RotatedBy(Main.rand.NextFloat(-0.6f, 0.6f) + 3.14f) * -Main.rand.NextFloat(0.5f, 5f), 0, new Color(255, 230, 60) * 0.8f, 1.6f);
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.FinalDamage *= damageIncrease;
            if (NPCLists.Robotic.Contains(target.type))
                modifiers.FinalDamage *= 1.2f;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 unit = new Vector2(1.5f, 0).RotatedBy(Projectile.rotation + MathHelper.PiOver2);
            float point = 0f;
            // Run an AABB versus Line check to look for collisions
            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center - unit * 20,
                Projectile.Center + unit * 10, 16, ref point))
                return true;
            else
                return false;
        }
        private int frame;
        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            SpriteEffects spriteEffects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            int width = texture.Width / 4;
            int x = width * frame;
            Rectangle rect = new(x, 0, width, texture.Height);

            Vector2 origin = new(width / 2f, texture.Height / 2f);
            Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter, true);
            Vector2 v = RedeHelper.PolarVector(2, (Projectile.Center - playerCenter).ToRotation());

            Main.EntitySpriteDraw(texture, Projectile.Center - v - Main.screenPosition, new Rectangle?(rect), Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);
            return false;
        }
    }
}