using ParticleLibrary.Utilities;
using Redemption.BaseExtension;
using Redemption.Buffs.Debuffs;
using Redemption.Globals;
using Redemption.Globals.Players;
using Redemption.Particles;
using Redemption.Projectiles.Melee;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace Redemption.Items.Weapons.HM.Melee
{
    public class SlayerFist_Proj : TrueMeleeProjectile
    {
        public Player Owner => Main.player[Projectile.owner];

        public Vector2 dirVec;
        public Vector2 launchVec;
        public float timer;
        public float progress;
        public float damageBoost;
        public float OpacityTimer;
        public bool fullCharge;
        public bool launch;
        public bool rotRight;
        public bool onHit;
        public override bool ShouldUpdatePosition() => false;
        public override void SetStaticDefaults()
        {
            ElementID.ProjExplosive[Type] = true;
        }
        public override void SetSafeDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
        }
        public override void AI()
        {
            if (Owner.noItems || Owner.CCed || Owner.dead || !Owner.active)
                Projectile.Kill();

            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;
            Vector2 armCenter = Owner.RotatedRelativePoint(Owner.MountedCenter);

            if (Main.myPlayer == Projectile.owner)
            {
                if (!launch)
                {
                    dirVec = armCenter.DirectionTo(Main.MouseWorld);
                    Owner.direction = armCenter.X < Main.MouseWorld.X ? 1 : -1;
                    Projectile.spriteDirection = Owner.direction;
                    Projectile.rotation = dirVec.ToRotation() + MathHelper.PiOver2;
                    Projectile.Center = armCenter;

                    Vector2 rand = Projectile.position + new Vector2(Main.rand.Next(-1, 1), Main.rand.Next(-1, 1));
                    Projectile.position = rand;
                    Charge();
                }
                else
                {
                    Launch();
                    Projectile.Center = armCenter;
                }
            }
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation + MathHelper.Pi);
        }
        public int maxTime;
        public void Charge()
        {
            Owner.ChangeDir(Projectile.spriteDirection);
            maxTime = SetUseTime(Owner.HeldItem.useTime);
            progress = MathHelper.Clamp(timer / (maxTime * 3), 0, 1);
            if (Owner.channel)
            {
                timer++;
            }
            else
            {
                if(Projectile.owner == Main.myPlayer)
                    launchVec = Owner.Center.DirectionTo(Main.MouseWorld);
                dirVec = launchVec;
                damageBoost = progress + 1;
                launch = true;
                timer = 0;
            }
            if (!fullCharge && progress >= 1)
            {
                fullCharge = true;
                Projectile.damage *= 5;
                SoundEngine.PlaySound(CustomSounds.ShootChange, Projectile.position);
            }
        }
        public void Launch()
        {
            if (!fullCharge)
            {
                if (timer++ == 0)
                {
                    if (!Main.dedServ)
                        SoundEngine.PlaySound(CustomSounds.MissileFire1, Projectile.Center);
                    Vector2 armCenter = Owner.RotatedRelativePoint(Owner.MountedCenter);
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), armCenter, RedeHelper.PolarVector(15, (Main.MouseWorld - armCenter).ToRotation()), ProjectileType<KS3_FistF>(), (int)(Projectile.damage * damageBoost), Projectile.knockBack, Owner.whoAmI);
                }
                if (timer > 25)
                    Projectile.Kill();
            }
            else
            {
                if (timer++ == 0)
                {
                    SoundEngine.PlaySound(SoundID.Item74, Projectile.position);
                    Projectile.friendly = true;
                    Projectile.tileCollide = true;
                    Projectile.velocity = launchVec * 10;
                    Projectile.extraUpdates = 3;
                }
                if (onHit)
                {
                    Projectile.extraUpdates = 0;
                    if (timer >= 16 && timer <= 35)
                    {
                        Owner.velocity *= 0.9f;
                        Owner.fullRotation += rotRight ? 0.33f : -0.33f;
                        Owner.fullRotationOrigin = new Vector2(10, 20);
                    }
                    if (timer >= 35)
                        Projectile.Kill();
                }
                else
                {
                    if (timer <= 12 * Projectile.MaxUpdates)
                    {
                        MakeDust();
                        Owner.GetModPlayer<RedePlayer>().fallSpeedIncrease += 40;
                        Owner.velocity = Projectile.velocity * 4;
                        Owner.Redemption().contactImmune = true;
                        OpacityTimer += 0.25f;
                    }
                    else if (timer <= 12 * Projectile.MaxUpdates + 6)
                    {
                        Projectile.extraUpdates = 0;
                        Projectile.friendly = false;
                        Owner.velocity *= 0.1f;
                        OpacityTimer += 1;
                    }
                    else
                        Projectile.Kill();
                }
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false;
        }
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            width = 2;
            height = 2;
            return true;
        }
        public override void OnKill(int timeLeft)
        {
            Owner.fullRotation = 0f;
        }
        public override bool? CanHitNPC(NPC target) => launch? null : false;
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
            Main.LocalPlayer.RedemptionScreen().ScreenShakeOrigin = Projectile.Center;
            Main.LocalPlayer.RedemptionScreen().ScreenShakeIntensity += 3;

            if (target.knockBackResist > 0)
                target.AddBuff(BuffType<StunnedDebuff>(), (int)(20 * target.knockBackResist));

            Projectile.friendly = false;
            if (fullCharge)
            {
                if (target.Center.X < Owner.Center.X)
                    rotRight = true;

                onHit = true;
                Owner.velocity = -launchVec * 40;
                Owner.velocity.Y -= 16;
                timer = 16;
            }

            for (int i = 0; i < 15; i++)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, Scale: 3);
                Main.dust[dust].velocity *= 4;
                Main.dust[dust].noGravity = true;
            }
            for (int i = 0; i < 20; i++)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, Scale: 3);
                Main.dust[dust].velocity *= 8;
                Main.dust[dust].noGravity = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
        public override void PostDraw(Color lightColor)
        {
        }
        public void MakeDust()
        {
            Vector2 unitVel = Projectile.velocity.SafeNormalize(default);
            Vector2 position = Owner.Center + (unitVel * 10f);
            Dust dust = Main.dust[Dust.NewDust(Owner.position, Owner.width, Owner.height, DustID.Frost)];
            dust.position = position;
            dust.velocity = (unitVel.RotatedBy(1.57f) * 6f) + (unitVel * 10f);
            dust.position += unitVel.RotatedBy(1.57f);
            dust.fadeIn = 0.5f;
            dust.noGravity = true;
            dust = Main.dust[Dust.NewDust(Owner.position, Owner.width, Owner.height, DustID.Frost)];
            dust.position = position;
            dust.velocity = (unitVel.RotatedBy(-1.57f) * 6f) + (unitVel * 10f);
            dust.position += unitVel.RotatedBy(-1.57f);
            dust.fadeIn = 0.5f;
            dust.noGravity = true;

            if (timer % 4 == 3)
                RedeParticleManager.CreateSpeedParticle(Main.rand.NextVector2FromRectangle(Owner.Hitbox), -unitVel * 30, 1, Color.Cyan.WithAlpha(0), extension: 100);
            
            Vector2 drawPos = Projectile.Center + Projectile.velocity *0;
            RedeParticleManager.CreateAdditiveGlowParticle(drawPos, Projectile.velocity, new(2, 1), Color.LightCyan * 0.5f, 8);
        }
    }
}
