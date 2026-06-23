using Microsoft.Xna.Framework.Graphics;
using Redemption.BaseExtension;
using Redemption.Dusts;
using Redemption.Globals;
using Redemption.Particles;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace Redemption.Items.Weapons.PreHM.Melee
{
    public class Zweihander_SlashProj : TrueMeleeProjectile
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Zweihander");
            Main.projFrames[Projectile.type] = 8;
        }
        public override bool ShouldUpdatePosition() => false;
        public override void SetSafeDefaults()
        {
            Projectile.width = 166;
            Projectile.height = 158;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Player player = Main.player[Projectile.owner];
            Projectile.scale *= player.GetAdjustedItemScale(player.HeldItem);
        }
        public override bool? CanCutTiles() => Projectile.frame is 4;
        public override bool? CanHitNPC(NPC target) => Projectile.frame is 4 ? null : false;
        public int maxTime;
        int directionLock = 0;
        private bool parried;
        public int pauseTimer;
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;

            Projectile.Redemption().swordHitbox = new((int)(Projectile.Center.X - 80 * Projectile.scale), (int)(Projectile.Center.Y - 70), (int)(160 * Projectile.scale), (int)(136 * Projectile.scale));

            maxTime = SetUseTime(player.HeldItem.useTime);

            if (player.noItems || player.CCed || player.dead || !player.active)
                Projectile.Kill();
            if (Projectile.ai[0] == 0)
            {
                player.itemRotation = MathHelper.ToRadians(-90f * player.direction);
                player.bodyFrame.Y = 5 * player.bodyFrame.Height;
                if (Projectile.owner == Main.myPlayer)
                    player.ChangeDir(Main.MouseWorld.X > player.Center.X ? 1 : -1);
                if (!player.channel)
                {
                    Projectile.ai[0] = 1;
                    directionLock = player.direction;
                }
            }
            if (Projectile.ai[0] >= 1)
            {
                player.direction = directionLock;
                if (--pauseTimer <= 0)
                    Projectile.ai[0]++;
                if (Projectile.frame > 3)
                    player.itemRotation -= MathHelper.ToRadians(-20f * player.direction);
                else
                    player.bodyFrame.Y = 5 * player.bodyFrame.Height;
                if (pauseTimer <= 0 && ++Projectile.frameCounter >= maxTime / 10)
                {
                    Projectile.frameCounter = 0;
                    Projectile.frame++;
                    if (Projectile.frame is 4)
                    {
                        SoundEngine.PlaySound(SoundID.Item71, Projectile.position);
                        player.velocity.X += 2 * player.direction;
                    }
                    if (Projectile.frame >= 4 && Projectile.frame <= 5)
                    {
                        foreach (Projectile target in Main.ActiveProjectiles)
                        {
                            if (!ProjReflect.FriendlyReflectCheck(Projectile, target, 500) || ProjReflect.ProjBlockBlacklist(target, true))
                                continue;

                            if (target.width + target.height > Projectile.width + Projectile.height)
                                continue;

                            if (target.velocity.Length() == 0 || !Projectile.Redemption().swordHitbox.Intersects(target.Hitbox) || target.alpha > 0)
                                continue;

                            SoundEngine.PlaySound(SoundID.Tink, Projectile.position);
                            RedeDraw.SpawnExplosion(target.Center, Color.White, shakeAmount: 0, scale: .5f, noDust: true, rot: RedeHelper.RandomRotation(), tex: "Redemption/Textures/SwordClash");
                            ProjReflect.FriendlyReflectEffect(target, false, .8f);
                        }
                    }
                    if (Projectile.frame > 9)
                    {
                        Projectile.Kill();
                    }
                }
            }
            bool parryActive = false;
            if (Projectile.frame is 3 or 4)
                parryActive = true;
            if (Projectile.frame is 4)
                ProjHelper.SwordClashFriendly(Projectile, player, ref parried);

            player.Redemption().CreateParryWindow(Projectile.Redemption().swordHitbox, ref parryActive);
            Projectile.spriteDirection = player.direction;
            Projectile.Center = player.RotatedRelativePoint(player.MountedCenter);
        }
        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            hitbox = Projectile.Redemption().swordHitbox;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Player player = Main.player[Projectile.owner];
            float tipBonus;
            tipBonus = player.Distance(target.Center) / 3;
            tipBonus = MathHelper.Clamp(tipBonus, 0, 20);

            modifiers.FlatBonusDamage += (int)tipBonus;
        }
        private bool paused;
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];

            ProjHelper.Decapitation(target, ref damageDone, ref hit.Crit);
            SoundEngine.PlaySound(CustomSounds.Slice4, Projectile.position);

            if (!paused)
            {
                player.RedemptionScreen().ScreenShakeIntensity += 5;
                pauseTimer = (int)(6 / SetSpeedBonus(30, player.HeldItem.useTime));
                paused = true;
            }
            Vector2 directionTo = player.Center.DirectionTo(target.Center);
            for (int i = 0; i < 4; i++)
            {
                Vector2 pos = target.Center + directionTo * 10 + new Vector2(0, 40) + player.velocity;
                Vector2 vel = directionTo.RotateRandom(1) * Main.rand.NextFloat(4f, 5f) + (player.velocity / 2);
                Dust.NewDustPerfect(pos, DustType<DustSpark2>(), vel, 0, new Color(214, 239, 243) * .8f, 2f);
            }
            Vector2 dir = player.Center.DirectionTo(target.Center).RotateRandom(0.01f);
            Vector2 drawPos = Vector2.Lerp(Projectile.Center, target.Center, 0.9f);
            RedeParticleManager.CreateSlashParticle(drawPos, dir.RotatedBy(player.direction * -1.5f) * 80, 1f, Color.White, 12);
        }

        Asset<Texture2D> slashTex;
        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];

            Asset<Texture2D> texture = TextureAssets.Projectile[Projectile.type];
            Rectangle rect = texture.Frame(1, 8, 0, Projectile.frame);
            Vector2 drawOrigin = rect.Size() / 2;
            var effects = Projectile.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            int offset = Projectile.frame > 3 ? 14 : 0;
            Vector2 pos = Projectile.Center + new Vector2(-16 * player.direction, offset - 5) * Projectile.scale;

            Main.EntitySpriteDraw(texture.Value, pos - Main.screenPosition, rect, Projectile.GetAlpha(lightColor), Projectile.rotation, drawOrigin, Projectile.scale, effects, 0);

            slashTex = Request<Texture2D>(Texture + "2");

            Main.EntitySpriteDraw(slashTex.Value, pos - Main.screenPosition, rect, Projectile.GetAlpha(Color.White), Projectile.rotation, drawOrigin, Projectile.scale, effects, 0);
            return false;
        }
    }
}
