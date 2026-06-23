using Microsoft.Xna.Framework.Graphics;
using Redemption.Base;
using Redemption.BaseExtension;
using Redemption.Dusts;
using Redemption.Globals;
using Redemption.Projectiles.Magic;
using Redemption.Projectiles.Melee;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Items.Weapons.PreHM.Melee
{
    public class ForestNymphsSickle_Proj : TrueMeleeProjectile
    {
        public override string Texture => "Redemption/Items/Weapons/PreHM/Melee/ForestNymphsSickle";

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Forest Nymph's Sickle");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 7;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
            ElementID.ProjNature[Type] = true;
        }
        public override bool ShouldUpdatePosition() => false;
        public override void SetSafeDefaults()
        {
            Projectile.width = 46;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;

            Length = 70;
            Rot = MathHelper.ToRadians(2);
            Projectile.alpha = 255;
        }
        public override bool? CanHitNPC(NPC target) => !target.friendly && progress < 0.25 && Projectile.ai[0] != 2 ? null : false;
        public override void OnSpawn(IEntitySource source)
        {
            if (Projectile.ai[0] == 2)
                Projectile.DamageType = DamageClass.Magic;

            Projectile.scale *= Owner.GetAdjustedItemScale(Owner.HeldItem);
            Length *= Projectile.scale;
        }
        private Player Owner => Main.player[Projectile.owner];
        public float[] oldrot = new float[7];
        public Vector2[] oldPos = new Vector2[7];
        private Vector2 startVector;
        private Vector2 positionVector;
        public ref float Length => ref Projectile.localAI[0];
        public ref float Rot => ref Projectile.localAI[1];
        public float Timer;
        private int maxTime;
        private Vector2 mouseOrig;
        private float glow;
        private bool lifeDrained;
        public int pauseTimer;
        public float progress;
        public override void AI()
        {
            if (Owner.noItems || Owner.CCed || Owner.dead || !Owner.active)
                Projectile.Kill();

            maxTime = SetUseTime(Owner.HeldItem.useTime);
            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;

            Projectile.spriteDirection = Owner.direction;
            Vector2 armCenter = Owner.RotatedRelativePoint(Owner.MountedCenter) + new Vector2(Owner.direction * -4, -4);
            progress = Timer / (maxTime);

            if (--pauseTimer <= 0)
            {
                switch (Projectile.ai[0])
                {
                    case 0:
                        if (Timer++ == 0)
                        {
                            if (Projectile.owner == Main.myPlayer)
                                mouseOrig = Main.MouseWorld;

                            startVector = RedeHelper.PolarVector(1, Projectile.velocity.ToRotation() - ((MathHelper.PiOver2 + 0.6f) * Projectile.spriteDirection));
                            positionVector = startVector * Length;
                            SoundEngine.PlaySound(SoundID.Item71, Owner.position);
                        }
                        if (Timer == (int)(maxTime / 6) && Projectile.owner == Main.myPlayer)
                            Projectile.NewProjectile(Projectile.GetSource_FromAI(), Owner.Center,
                            RedeHelper.PolarVector(14, (mouseOrig - armCenter).ToRotation()),
                            ProjectileType<ForestSickle_Proj>(), (int)(Projectile.damage * .75f), Projectile.knockBack / 2, Projectile.owner);

                        if (progress < 0.24f)
                        {
                            Rot = MathHelper.ToRadians(750 * progress) * Projectile.spriteDirection;
                            positionVector = startVector.RotatedBy(Rot) * Length;
                        }
                        else
                        {
                            Rot = MathHelper.ToRadians(750 * (0.333f - MathF.Pow(0.00005f, progress))) * Projectile.spriteDirection;
                            positionVector = startVector.RotatedBy(Rot) * Length;
                        }
                        if (progress >= 1)
                        {
                            if (!Owner.channel)
                            {
                                Projectile.Kill();
                                return;
                            }
                            if (Projectile.owner == Main.myPlayer)
                            {
                                if (Main.MouseWorld.X < Owner.Center.X)
                                    Owner.direction = -1;
                                else
                                    Owner.direction = 1;
                                Projectile.velocity = RedeHelper.PolarVector(5, (Main.MouseWorld - armCenter).ToRotation());
                                startVector = RedeHelper.PolarVector(1, (Main.MouseWorld - armCenter).ToRotation() + ((MathHelper.PiOver2 + 0.6f) * Owner.direction));
                                positionVector = startVector * Length;
                                mouseOrig = Main.MouseWorld;
                            }
                            Projectile.alpha = 255;
                            Rot = MathHelper.ToRadians(2);
                            lifeDrained = false;
                            Projectile.ai[0]++;
                            SoundEngine.PlaySound(SoundID.Item71, Projectile.position);
                            glow = 0;
                            Timer = 0;
                            Projectile.netUpdate = true;
                        }
                        break;
                    case 1:
                        if (Timer++ == (int)(maxTime / 6) && Projectile.owner == Main.myPlayer)
                            Projectile.NewProjectile(Projectile.GetSource_FromAI(), Owner.Center, RedeHelper.PolarVector(14, (mouseOrig - armCenter).ToRotation()), ProjectileType<ForestSickle_Proj>(), (int)(Projectile.damage * .75f), Projectile.knockBack / 2, Projectile.owner);

                        if (progress < 0.24f)
                        {
                            Rot = -MathHelper.ToRadians(750 * progress) * Projectile.spriteDirection;
                            positionVector = startVector.RotatedBy(Rot) * Length;
                        }
                        else
                        {
                            Rot = -MathHelper.ToRadians(750 * (0.333f - MathF.Pow(0.00005f, progress))) * Projectile.spriteDirection;
                            positionVector = startVector.RotatedBy(Rot) * Length;
                        }
                        if (progress >= 1)
                            Projectile.Kill();
                        break;
                    case 2:
                        if (Timer++ == 0)
                            positionVector = new Vector2(6 * Owner.direction, -20);

                        int dustIndex = Dust.NewDust(new Vector2(Owner.position.X, Owner.Bottom.Y - 2), Owner.width, 2, DustID.DryadsWard);
                        Main.dust[dustIndex].velocity.Y = -Main.rand.Next(3, 7);
                        Main.dust[dustIndex].velocity.X = 0;
                        Main.dust[dustIndex].noGravity = true;
                        if (glow < 1)
                            glow += .03f;
                        Projectile.rotation = ((float)Math.Sin(Timer / 20) / 6) + (Owner.direction == -1 ? .3f : -.3f);
                        if (!lifeDrained)
                        {
                            startVector.Y += 0.1f;
                            if (startVector.Y > 1.2f)
                                lifeDrained = true;
                        }
                        else if (lifeDrained)
                        {
                            startVector.Y -= 0.1f;
                            if (startVector.Y < -1.2f)
                                lifeDrained = false;
                        }
                        positionVector = new Vector2(6 * Owner.direction, -20) + startVector;
                        if (Timer >= 30)
                        {
                            if (Main.rand.NextBool(5) && Owner.ownedProjectileCounts[ProjectileType<NaturePixie_Magic>()] < 4)
                            {
                                if (BasePlayer.ReduceMana(Owner, 8))
                                {
                                    SoundEngine.PlaySound(SoundID.Item101, Projectile.position);
                                    if (Projectile.owner == Main.myPlayer)
                                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, new Vector2(Main.rand.NextFloat(-3, 3), -Main.rand.NextFloat(4, 8)), ProjectileType<NaturePixie_Magic>(), Projectile.damage, Projectile.knockBack, Owner.whoAmI);
                                }
                            }
                        }
                        if (!Owner.channel)
                            Projectile.Kill();
                        break;
                }
            }
            if (Timer > 1)
                Projectile.alpha = 0;

            Projectile.Center = armCenter + positionVector;
            if (Projectile.ai[0] < 2)
            {
                if (Projectile.spriteDirection == 1)
                    Projectile.rotation = (Projectile.Center - armCenter).ToRotation() + MathHelper.PiOver4;
                else
                    Projectile.rotation = (Projectile.Center - armCenter).ToRotation() - MathHelper.Pi - MathHelper.PiOver4;
                glow += 0.03f;
            }
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, (armCenter - Projectile.Center).ToRotation() + MathHelper.PiOver2);

            for (int k = Projectile.oldPos.Length - 1; k > 0; k--)
            {
                oldPos[k] = oldPos[k - 1];
                oldrot[k] = oldrot[k - 1];
            }
            oldrot[0] = Projectile.rotation;
            oldPos[0] = positionVector;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (NPCLists.Dark.Contains(target.type))
                modifiers.FinalDamage *= 1.5f;
        }
        private bool paused;
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(CustomSounds.Slice4 with { Volume = .7f, Pitch = .2f }, Projectile.position);

            if (!paused)
            {
                Owner.RedemptionScreen().ScreenShakeIntensity += 4;
                pauseTimer = 4;
                paused = true;
            }

            Vector2 armCenter = Owner.RotatedRelativePoint(Owner.MountedCenter) + new Vector2(Owner.direction * -4, -4);
            Vector2 directionTo = target.DirectionTo(armCenter);
            for (int i = 0; i < 8; i++)
                Dust.NewDustPerfect(target.Center + directionTo * 5 + new Vector2(0, 35) + Owner.velocity, DustType<DustSpark2>(), directionTo.RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f) + 3.14f) * Main.rand.NextFloat(4f, 5f) + (Owner.velocity / 2), 0, Color.LimeGreen * .8f, 1.6f);

            ProjHelper.Decapitation(target, ref damageDone, ref hit.Crit);
            if (!lifeDrained)
            {
                Owner.statLife += (int)Math.Floor((double)damageDone / 20);
                Owner.HealEffect((int)Math.Floor((double)damageDone / 20));
                lifeDrained = true;
            }
            Projectile.localNPCImmunity[target.whoAmI] = 10;
            target.immune[Projectile.owner] = 0;

            if (Main.rand.NextBool(3))
                target.AddBuff(BuffID.DryadsWardDebuff, 300);
        }
        public override void CutTiles()
        {
            DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
            Utils.TileActionAttempt cut = new(DelegateMethods.CutTiles);
            Vector2 lineStart = Owner.RotatedRelativePoint(Owner.MountedCenter);
            Vector2 lineEnd = Projectile.Center;
            float height = Projectile.height * Projectile.scale;
            Utils.PlotTileLine(lineStart, lineEnd, height, cut);
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float point = 0f;
            Vector2 lineStart = Owner.RotatedRelativePoint(Owner.MountedCenter);
            Vector2 lineEnd = Projectile.Center;
            float height = Projectile.height * Projectile.scale;
            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), lineStart, lineEnd, height, ref point))
                return true;
            return false;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteEffects spriteEffects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = Projectile.spriteDirection == 1 ? new(texture.Height / 2, texture.Height / 2) : new(texture.Width - texture.Height / 2, texture.Height / 2);
            if (Projectile.ai[0] == 2)
                drawOrigin = new(texture.Width / 2f - (30 * Owner.direction), texture.Height / 2f + 34);

            int shader = ContentSamples.CommonlyUsedContentSamples.ColorOnlyShaderIndex;

            Vector2 armCenter = Owner.RotatedRelativePoint(Owner.MountedCenter) + new Vector2(Owner.direction * -4, -4);
            Vector2 drawPos = armCenter + positionVector.SafeNormalize(default) * 60 * Projectile.scale;
            if (Projectile.ai[0] == 2)
                drawPos = armCenter + positionVector * 0.5f * Projectile.scale;

            Main.spriteBatch.End();
            Main.spriteBatch.BeginAdditive();
            GameShaders.Armor.ApplySecondary(shader, Main.LocalPlayer, null);

            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 drawPos2 = armCenter + oldPos[k].SafeNormalize(default) * 60 * Projectile.scale;
                if (Projectile.ai[0] == 2)
                    drawPos2 = armCenter + oldPos[k] * 0.5f * Projectile.scale;
                Color color = Color.LimeGreen * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos2 - Main.screenPosition, null, color * Projectile.Opacity * glow, oldrot[k], drawOrigin, Projectile.scale, spriteEffects, 0);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.BeginDefault(true);

            Main.EntitySpriteDraw(texture, drawPos - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, drawOrigin, Projectile.scale, spriteEffects, 0);
            return false;
        }
    }
}