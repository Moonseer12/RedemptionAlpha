using Microsoft.Xna.Framework.Graphics;
using Redemption.BaseExtension;
using Redemption.Buffs.NPCBuffs;
using Redemption.Dusts;
using Redemption.Globals;
using Redemption.NPCs.Friendly.TownNPCs;
using Redemption.Particles;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;

namespace Redemption.Items.Weapons.PreHM.Melee
{
    public abstract class BaseSwordSlicer_Slash : TrueMeleeProjectile
    {
        public override string Texture => "Redemption/Items/Weapons/PreHM/Melee/SwordSlicer_Slash";
        protected abstract Entity Owner { get; }
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 9;
        }
        public override bool ShouldUpdatePosition() => false;
        public override void SetSafeDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            if (Owner is NPC)
                Projectile.npcProj = true;
        }

        public override bool? CanCutTiles() => Projectile.frame is 5;
        public override bool? CanHitNPC(NPC target)
        {
            if (Owner is Player)
                return !target.friendly && Projectile.frame is 5 ? null : false;
            return !target.friendly && target.type != NPCID.TargetDummy && Projectile.frame is 5 ? null : false;
        }

        private int maxTime;
        int directionLock = 0;
        private bool parried;
        private int pauseTimer;
        public override void AI()
        {
            Player player = Owner as Player;
            NPC npc = Owner as NPC;

            if (player != null)
                player.heldProj = Projectile.whoAmI;
            Projectile.Redemption().swordHitbox = new((int)(Projectile.spriteDirection == -1 ? Projectile.Center.X - 90 : Projectile.Center.X), (int)(Projectile.Center.Y - 67), 90, 126);
            maxTime = player != null ? SetUseTime(player.HeldItem.useTime) : 26;

            if (player != null && (player.noItems || player.CCed || player.dead || !player.active))
            {
                Projectile.Kill();
                return;
            }
            if (npc != null && (!npc.active || npc.type != NPCType<Zephos>() || npc.ai[1] <= 1))
            {
                Projectile.Kill();
                return;
            }
            if (Projectile.ai[0] == 0)
            {
                if (player != null)
                {
                    player.itemRotation = MathHelper.ToRadians(-90f * player.direction);
                    player.bodyFrame.Y = 5 * player.bodyFrame.Height;
                }
                Projectile.ai[0] = 1;
                directionLock = Owner.direction;
            }
            if (Projectile.ai[0] >= 1)
            {
                Owner.direction = directionLock;
                if (--pauseTimer <= 0)
                    Projectile.ai[0]++;
                if (player != null)
                {
                    if (Projectile.frame > 4)
                        player.itemRotation -= MathHelper.ToRadians(-25f * player.direction);
                    else
                        player.bodyFrame.Y = 5 * player.bodyFrame.Height;
                }
                if (pauseTimer <= 0 && ++Projectile.frameCounter >= maxTime / 9)
                {
                    Projectile.frameCounter = 0;
                    Projectile.frame++;
                    if (Projectile.frame is 5)
                    {
                        SoundEngine.PlaySound(SoundID.Item71, Projectile.position);
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
                            target.Kill();
                        }
                    }
                    if (Projectile.frame > 8)
                        Projectile.Kill();
                }
            }
            bool parryActive = false;
            if (Projectile.frame is 4 or 5)
                parryActive = true;
            if (Projectile.frame is 5)
                ProjHelper.SwordClashFriendly(Projectile, player ?? Main.LocalPlayer, ref parried);

            (player ?? Main.LocalPlayer).Redemption().CreateParryWindow(Projectile.Redemption().swordHitbox, ref parryActive);

            Projectile.spriteDirection = Owner.direction;

            Projectile.Center = player != null ? player.RotatedRelativePoint(player.MountedCenter, true) : Owner.Center;
            if (player != null)
            {
                player.itemTime = 2;
                player.itemAnimation = 2;
            }
        }
        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            hitbox = Projectile.Redemption().swordHitbox;
        }
        private bool paused;
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(CustomSounds.Slice3 with { Pitch = -.1f }, Projectile.position);
            if (!paused)
            {
                if (Owner is Player player)
                    player.RedemptionScreen().ScreenShakeIntensity += 3;
                pauseTimer = 5;
                paused = true;
            }
            Vector2 directionTo = target.DirectionTo(Owner.Center);
            for (int i = 0; i < 4; i++)
                Dust.NewDustPerfect(target.Center + directionTo * 10 + new Vector2(0, 40) + Owner.velocity, DustType<DustSpark2>(), directionTo.RotatedBy(Main.rand.NextFloat(-0.01f, 0.01f) + 3.14f + Owner.direction * MathHelper.PiOver4) * Main.rand.NextFloat(4f, 5f) + (Owner.velocity / 2), 0, new Color(214, 239, 243) * .8f, 2f);

            Vector2 dir = target.DirectionTo(Owner.Center);
            Vector2 drawPos = Vector2.Lerp(Projectile.Center, target.Center, 0.9f);
            RedeParticleManager.CreateSlashParticle(drawPos, dir.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f) + Owner.direction * MathHelper.PiOver4 * 1.2f) * 60, 1f, Color.LightSteelBlue, 14);

            ProjHelper.Decapitation(target, ref damageDone, ref hit.Crit);
            if (NPCLists.Armed.Contains(target.type))
                target.AddBuff(BuffType<DisarmedDebuff>(), 1800);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            int height = texture.Height / 9;
            int y = height * Projectile.frame;
            Rectangle rect = new(0, y, texture.Width, height);
            Vector2 drawOrigin = new(texture.Width / 2, Projectile.height / 2);
            var effects = Projectile.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            int offset = Projectile.frame > 4 ? -6 : 0;

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition - new Vector2(-1 * Owner.direction, 40 - offset),
                new Rectangle?(rect), Projectile.GetAlpha(lightColor), Projectile.rotation, drawOrigin, Projectile.scale, effects, 0);

            Texture2D slash = Request<Texture2D>("Redemption/Items/Weapons/PreHM/Melee/SwordSlicer_SlashProj").Value;
            int height2 = slash.Height / 2;
            int y2 = height2 * (Projectile.frame - 5);
            Rectangle rect2 = new(0, y2, slash.Width, height2);
            Vector2 drawOrigin2 = new(slash.Width / 2, slash.Height / 2);

            if (Projectile.frame >= 5)
                Main.EntitySpriteDraw(slash, Projectile.Center - Main.screenPosition - new Vector2(-40 * Owner.direction, -63 - offset), new Rectangle?(rect2), Projectile.GetAlpha(Color.White), Projectile.rotation, drawOrigin2, Projectile.scale, effects, 0);
            return false;
        }
    }
    public class SwordSlicer_Slash : BaseSwordSlicer_Slash
    {
        protected override Entity Owner => Main.player[Projectile.owner];
    }
}
