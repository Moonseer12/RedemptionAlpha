using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.Utilities;
using Redemption.Base;
using Redemption.BaseExtension;
using Redemption.Buffs.NPCBuffs;
using Redemption.Globals;
using Redemption.Particles;
using Redemption.Projectiles.Melee;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace Redemption.Items.Weapons.PostML.Melee
{
    public class MythrilsBane_Proj : TrueMeleeProjectile
    {
        public Player Owner => Main.player[Projectile.owner];
        public ref float Length => ref Projectile.localAI[0];
        private ref float SwingType => ref Projectile.ai[0];
        private ref float Timer => ref Projectile.ai[1];

        public float[] oldrot = new float[6];

        public Vector2[] oldPos = new Vector2[6];

        private float swingProgress;

        private float startRotation;

        public Vector2 positionVector;

        public bool parried;

        public int pauseTimer;

        public float progress;

        public bool strike;

        public int maxTime;

        public override string Texture => "Redemption/Items/Weapons/PostML/Melee/MythrilsBane";
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Mythril's Bane");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }
        public override bool ShouldUpdatePosition() => false;
        public override void SetSafeDefaults()
        {
            Projectile.width = 90;
            Projectile.height = 90;
            Projectile.alpha = 255;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.MaxUpdates = 5;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            Utils.WriteVector2(writer, positionVector);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            positionVector = Utils.ReadVector2(reader);
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.spriteDirection = Owner.direction;
            maxTime = SetUseTime(Owner.HeldItem.useTime);
        }
        public override void AI()
        {
            if (Owner.noItems || Owner.CCed || Owner.dead || !Owner.active)
                Projectile.Kill();

            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;

            progress = Timer / (maxTime * Projectile.MaxUpdates);

            Vector2 armCenter = Owner.RotatedRelativePoint(Owner.MountedCenter) + new Vector2(Owner.direction * -4, -4);

            bool parryActive = false;
            if (Projectile.owner == Main.myPlayer)
            {
                if (--pauseTimer <= 0)
                {
                    Projectile.friendly = swingProgress < 1;
                    if (Timer == 0)
                    {
                        Projectile.scale *= Projectile.ai[2];
                        Length = 80 * Projectile.scale;

                        if (!Main.dedServ)
                            SoundEngine.PlaySound(CustomSounds.Swing1, Owner.position);

                        startRotation = Owner.direction == 1 ? Projectile.velocity.ToRotation() : -Projectile.velocity.ToRotation() + MathHelper.Pi;
                    }
                    if (progress <= 1)
                    {
                        swingProgress = EaseFunction.EaseQuadInOut.Ease(progress);
                        float angle = (0.8f * swingProgress + 0.6f) * MathHelper.TwoPi * SwingType;

                        Vector2 realPos = new(Length * MathF.Cos(angle), Length * MathF.Sin(angle));
                        realPos = realPos.RotatedBy(startRotation);
                        realPos.X *= Projectile.spriteDirection;
                        positionVector = realPos;
                    }
                    if (progress > 1)
                    {
                        if (Owner.channel && Owner.HeldItem.ModItem is MythrilsBane weapon)
                        {
                            if(SwingType == 1)
                                weapon.channeled = true;
                        }
                        Projectile.Kill();
                    }
                    if (progress >= .5f && progress <= .58f)
                    {
                        parryActive = true;
                        ProjHelper.SwordClashFriendly(Projectile, Owner, ref parried);
                    }
                    if (Timer == (int)(maxTime / 4 * Projectile.MaxUpdates))
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), Owner.Center, RedeHelper.PolarVector(Main.rand.Next(45, 66), Projectile.velocity.ToRotation() + Main.rand.NextFloat(-0.2f, 0.2f)), ProjectileType<MythrilsBaneSlash_Proj>(), Projectile.damage, Projectile.knockBack, Projectile.owner, SwingType == 1 ? 1 : 0);

                    Timer++;
                }
            }
            if (progress < .8f)
            {
                foreach (Projectile target in Main.ActiveProjectiles)
                {
                    if (!ProjReflect.FriendlyReflectCheck(Projectile, target, 500) || ProjReflect.ProjBlockBlacklist(target, true))
                        continue;

                    if (target.width + target.height > Projectile.width + Projectile.height)
                        continue;

                    if (target.velocity.Length() == 0 || !Projectile.Hitbox.Intersects(target.Hitbox) || target.alpha > 0)
                        continue;

                    SoundEngine.PlaySound(SoundID.Tink, Projectile.position);
                    RedeDraw.SpawnExplosion(target.Center, new Color(239, 255, 229), shakeAmount: 0, scale: .5f, noDust: true, rot: RedeHelper.RandomRotation(), tex: "Redemption/Textures/SwordClash");
                    target.Kill();
                }
            }
            Owner.Redemption().CreateParryWindow(Projectile.Hitbox, ref parryActive);

            Projectile.rotation = positionVector.ToRotation();
            Projectile.Center = armCenter + positionVector;
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation + MathHelper.ToRadians(-90));

            if (Timer > 1)
                Projectile.alpha = 0;

            for (int k = Projectile.oldPos.Length - 1; k > 0; k--)
            {
                oldPos[k] = oldPos[k - 1];
                oldrot[k] = oldrot[k - 1];
            }
            oldrot[0] = Projectile.rotation;
            oldPos[0] = positionVector;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            ProjHelper.Decapitation(target, ref damageDone, ref hit.Crit, 50);
            if (NPCLists.Armed.Contains(target.type))
                target.AddBuff(BuffType<DisarmedDebuff>(), 1800);
            target.AddBuff(BuffType<BrokenArmorDebuff>(), 600);

            if (!strike)
            {
                SoundEngine.PlaySound(CustomSounds.Slice4 with { Volume = .7f, Pitch = -.2f }, Projectile.position);
                Owner.RedemptionScreen().ScreenShakeIntensity += 4;
                strike = true;
                pauseTimer = maxTime;
            }
            Vector2 dir = Owner.Center.DirectionFrom(target.Center);
            Vector2 drawPos = Vector2.Lerp(Projectile.Center, target.Center, 0.9f);
            RedeParticleManager.CreateSlashParticle(drawPos, dir.RotatedBy(Owner.direction * SwingType) * 120, 1.5f, Color.LightSeaGreen, 8);

            for (int i = 0; i < 5; i++)
            {
                float randomRotation = Main.rand.NextFloat(-0.6f, 0.6f);
                float randomVel = Main.rand.NextFloat(2f, 3f);
                Vector2 direction = target.Center.DirectionFrom(Owner.Center);
                Vector2 position = target.Center - direction * 30;
                RedeParticleManager.CreateSpeedParticle(position, direction.RotatedBy(randomRotation) * randomVel * 16, .8f, Color.LightSeaGreen.WithAlpha(0));
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteEffects effect1 = Projectile.spriteDirection == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            SpriteEffects effect2 = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Rectangle rect = new(0, 0, texture.Width, texture.Height);
            Vector2 drawOrigin = new(texture.Width / 2f, texture.Height / 2f);
            Vector2 armCenter = Owner.RotatedRelativePoint(Owner.MountedCenter) + new Vector2(Owner.direction * -4, -4);
            Vector2 drawPos = armCenter + positionVector * 0.8f;
            float rotation = Projectile.spriteDirection == -1 ? Projectile.rotation + 3 * MathHelper.PiOver4 : Projectile.rotation + 1 * MathHelper.PiOver4;

            Texture2D Swing = Request<Texture2D>("Redemption/Textures/BladeSwing").Value;
            Rectangle rectangle = Swing.Frame(1, 4);
            Vector2 origin = rectangle.Size() / 2f;
            float opacity = BaseUtility.MultiLerp(EaseFunction.EaseQuadInOut.Ease(progress), 0, 1, 0);
            Vector2 vec = Projectile.velocity.SafeNormalize(default) + positionVector.SafeNormalize(default);
            float rotation2 = vec.ToRotation();

            Main.spriteBatch.End();
            Main.spriteBatch.BeginAdditive();

            if (Timer > 2)
            {
                for (int k = 0; k < Projectile.oldPos.Length; k++)
                {
                    Vector2 drawPos2 = armCenter + oldPos[k] * 0.8f;
                    Color color = Color.LightSeaGreen * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                    float rot = Projectile.spriteDirection == -1 ? oldrot[k] + 3 * MathHelper.PiOver4 : oldrot[k] + 1 * MathHelper.PiOver4;
                    Main.EntitySpriteDraw(texture, drawPos2 - Main.screenPosition, null, color, rot, drawOrigin, Projectile.scale, effect2, 0);
                }
            }

            Main.spriteBatch.Draw(Swing, armCenter - Main.screenPosition, Swing.Frame(1, 4, 0, 0), Color.LightSeaGreen * opacity * 0.3f, rotation2 + Owner.direction * 0.1f, origin, Projectile.scale * 1.5f, effect1, 0f);
            Main.spriteBatch.Draw(Swing, armCenter - Main.screenPosition, Swing.Frame(1, 4, 0, 1), Color.LightSeaGreen * opacity * 0.2f, rotation2 + Owner.direction * 0.01f, origin, Projectile.scale * 1.5f, effect1, 0f);
            Main.spriteBatch.Draw(Swing, armCenter - Main.screenPosition, Swing.Frame(1, 4, 0, 2), Color.LightSeaGreen * opacity * 0.5f, rotation2 + Owner.direction * 0.1f, origin, Projectile.scale * 1.52f, effect1, 0f);
            Main.spriteBatch.Draw(Swing, armCenter - Main.screenPosition, Swing.Frame(1, 4, 0, 3), new Color(204, 153, 255) * 0.6f * opacity, rotation2 + Owner.direction * 0.01f, origin, Projectile.scale * 1.5f, effect1, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.BeginDefault(true);

            Main.EntitySpriteDraw(texture, drawPos - Main.screenPosition, new Rectangle?(rect), Projectile.GetAlpha(lightColor), rotation, drawOrigin, Projectile.scale, effect2, 0);
            return false;
        }
    }
}