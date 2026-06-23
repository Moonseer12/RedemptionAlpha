using Microsoft.Xna.Framework.Graphics;
using Redemption.BaseExtension;
using Redemption.Buffs.NPCBuffs;
using Redemption.Dusts;
using Redemption.Globals;
using Redemption.Items.Tools.PreHM;
using Redemption.NPCs.Minibosses.Calavia;
using Redemption.Particles;
using Redemption.Projectiles.Magic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace Redemption.Items.Weapons.PreHM.Melee
{
    public class BladeOfTheMountain_Slash : TrueMeleeProjectile
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Blade of the Mountain");
            Main.projFrames[Projectile.type] = 10;
            ElementID.ProjIce[Type] = true;
        }
        public override bool ShouldUpdatePosition() => false;
        public override void SetSafeDefaults()
        {
            Projectile.width = 216;
            Projectile.height = 106;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Player player = Main.player[Projectile.owner];
            Projectile.scale *= player.GetAdjustedItemScale(player.HeldItem);
        }
        public override bool? CanCutTiles() => Projectile.frame is 5;
        public override bool? CanHitNPC(NPC target) => Projectile.frame is 5 ? null : false;
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

            Projectile.Redemption().swordHitbox = new((int)(Projectile.Center.X - 100 * Projectile.scale), (int)(Projectile.Center.Y - 70), (int)(200 * Projectile.scale), (int)(136 * Projectile.scale));
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
                    if (Projectile.frame is 5)
                    {
                        SoundEngine.PlaySound(SoundID.Item71, Projectile.position);
                        player.velocity.X += 2 * player.direction;
                    }
                    if (Projectile.frame >= 5 && Projectile.frame <= 6)
                    {
                        foreach (Projectile target in Main.ActiveProjectiles)
                        {
                            if (target.ai[0] is 0 && (target.type == ProjectileType<Icefall_Proj>() || target.type == ProjectileType<Calavia_Icefall>() || target.type == ProjectileType<IceSpikeShard>()) && Projectile.Redemption().swordHitbox.Intersects(target.Hitbox))
                            {
                                DustHelper.DrawCircle(target.Center, DustID.IceTorch, 1, 2, 2, dustSize: 2, nogravity: true);
                                if (!Main.dedServ)
                                    SoundEngine.PlaySound(CustomSounds.CrystalHit, Projectile.position);
                                target.velocity.Y = Main.rand.NextFloat(-2, 0);
                                target.velocity.X = player.direction * 18f;
                                target.damage *= 2;
                                target.friendly = true;
                                target.ai[0] = 1;
                                continue;
                            }
                            if (!ProjReflect.FriendlyReflectCheck(Projectile, target, 500))
                                continue;

                            if (target.velocity.Length() == 0 || !Projectile.Redemption().swordHitbox.Intersects(target.Hitbox) || (!target.HasElement(ElementID.Ice) && target.alpha > 0) || ProjReflect.ProjBlockBlacklist(target, true))
                                continue;

                            SoundEngine.PlaySound(SoundID.Tink, Projectile.position);
                            RedeDraw.SpawnExplosion(target.Center, new Color(214, 239, 243), shakeAmount: 0, scale: .5f, noDust: true, rot: RedeHelper.RandomRotation(), tex: "Redemption/Textures/SwordClash");
                            ProjReflect.FriendlyReflectEffect(target, true, .9f);
                        }
                    }
                    if (Projectile.frame > 9)
                        Projectile.Kill();
                }
            }
            bool parryActive = false;
            if (Projectile.frame is 4 or 5)
                parryActive = true;
            if (Projectile.frame is 5)
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
        public bool paused;
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];

            ProjHelper.Decapitation(target, ref damageDone, ref hit.Crit);
            SoundEngine.PlaySound(CustomSounds.Slice4, Projectile.position);

            if (!paused)
            {
                player.RedemptionScreen().ScreenShakeIntensity += 5;
                pauseTimer = (int)(6 / SetSpeedBonus(25, player.HeldItem.useTime));
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
            RedeParticleManager.CreateSlashParticle(drawPos, dir.RotatedBy(player.direction * -1.5f) * 80, 1f, Color.LightBlue, 12);

            if (target.DistanceSQ(player.Center) > 100 * 100 && target.knockBackResist > 0 && !target.RedemptionNPCBuff().iceFrozen)
            {
                SoundEngine.PlaySound(SoundID.Item30, target.position);
                DustHelper.DrawDustImage(target.Center, DustID.Frost, 0.5f, "Redemption/Effects/DustImages/Flake", 2, true, RedeHelper.RandomRotation());
                target.AddBuff(BuffType<IceFrozen>(), 1800 - ((int)MathHelper.Clamp(target.lifeMax, 60, 1780)));
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];

            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Rectangle rect = texture.Frame(1, 10, 0, Projectile.frame);
            Vector2 drawOrigin = rect.Size() * 0.5f;
            var effects = Projectile.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            int offset = Projectile.frame > 4 ? 16 : 0;

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition - new Vector2(40 * player.direction, 40 - offset) * Projectile.scale,
                new Rectangle?(rect), Projectile.GetAlpha(lightColor), Projectile.rotation, drawOrigin, Projectile.scale, effects, 0);

            Texture2D slash = Request<Texture2D>("Redemption/Items/Weapons/PreHM/Melee/BladeOfTheMountain_SlashProj").Value;
            int height2 = slash.Height / 6;
            int y2 = height2 * (Projectile.frame - 5);
            Rectangle rect2 = new(0, y2, slash.Width, height2);
            Vector2 drawOrigin2 = new(slash.Width / 2, slash.Height / 2);

            if (Projectile.frame >= 5 && Projectile.frame <= 9)
                Main.EntitySpriteDraw(slash, Projectile.Center - Main.screenPosition - new Vector2(0 * player.direction, -331 - offset) * Projectile.scale, new Rectangle?(rect2), Projectile.GetAlpha(Color.White), Projectile.rotation, drawOrigin2, Projectile.scale, effects, 0);
            return false;
        }
    }
}
