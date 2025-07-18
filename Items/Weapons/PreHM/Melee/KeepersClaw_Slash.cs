using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.Core;
using ParticleLibrary.Utilities;
using Redemption.BaseExtension;
using Redemption.Buffs.NPCBuffs;
using Redemption.Globals;
using Redemption.Particles;
using Redemption.Projectiles.Melee;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Items.Weapons.PreHM.Melee
{
    public class KeepersClaw_Slash : TrueMeleeProjectile
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Keeper's Claw");
            Main.projFrames[Projectile.type] = 6;
            ElementID.ProjBlood[Type] = true;
        }
        public override bool ShouldUpdatePosition() => false;
        public override void SetSafeDefaults()
        {
            Projectile.width = 90;
            Projectile.height = 110;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
        }

        public override bool? CanCutTiles() => Projectile.frame is 2;
        public override bool? CanHitNPC(NPC target) => Projectile.frame is 2 ? null : false;
        public float SwingSpeed;
        int directionLock = 0;
        private float glow;
        Vector2 mousePoint;
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            player.heldProj = Projectile.whoAmI;
            Projectile.Redemption().swordHitbox = new((int)(Projectile.spriteDirection == -1 ? Projectile.Center.X - 63 : Projectile.Center.X), (int)(Projectile.Center.Y - 55), 63, 104);
            SwingSpeed = SetSwingSpeed(20);

            if (player.noItems || player.CCed || player.dead || !player.active)
                Projectile.Kill();
            if (Main.myPlayer == Projectile.owner)
            {
                if (Projectile.ai[0] == 0)
                {
                    mousePoint = Main.MouseWorld;
                    player.itemRotation = MathHelper.ToRadians(-90f * player.direction);
                    player.bodyFrame.Y = 5 * player.bodyFrame.Height;
                    directionLock = player.direction;
                    glow += 0.02f * 20f / SwingSpeed;
                    glow = MathHelper.Clamp(glow, 0, 0.8f);
                    if (glow >= 0.8 && Projectile.localAI[0] == 0)
                    {
                        RedeDraw.SpawnRing(Projectile.Center, Color.DarkRed, 0.2f);
                        SoundEngine.PlaySound(SoundID.NPCDeath7 with { Pitch = -.2f, Volume = .5f }, Projectile.position);
                        Projectile.localAI[0] = 1;
                    }
                    if (!player.channel)
                    {
                        Projectile.ai[0] = 1;
                        if (Main.MouseWorld.X < player.Center.X)
                            directionLock = -1;
                        else
                            directionLock = 1;
                    }
                }
                if (Projectile.ai[0] >= 1)
                {
                    player.direction = directionLock;
                    Projectile.ai[0]++;
                    if (Projectile.frame > 1)
                        player.itemRotation -= MathHelper.ToRadians(-25f * player.direction);
                    else
                        player.bodyFrame.Y = 5 * player.bodyFrame.Height;
                    if (++Projectile.frameCounter >= SwingSpeed / 6)
                    {
                        Projectile.frameCounter = 0;
                        Projectile.frame++;
                        if (Projectile.frame is 2)
                        {
                            if (Projectile.localAI[0] == 1)
                            {
                                player.statLife -= 15;
                                if (player.statLife < 1)
                                    player.statLife = 1;
                                CombatText.NewText(player.getRect(), Colors.RarityRed, 15, true, true);
                                SoundEngine.PlaySound(SoundID.NPCDeath19, Projectile.position);
                                for (int i = 0; i < 4; i++)
                                {
                                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), player.Center, RedeHelper.PolarVector(Main.rand.NextFloat(5, 8), (mousePoint - player.Center).ToRotation() + Main.rand.NextFloat(-0.2f, 0.2f)), ModContent.ProjectileType<KeepersClaw_BloodWave>(), Projectile.damage / 3, 2, player.whoAmI);
                                }
                                for (int i = 0; i < 20; i++)
                                {
                                    int dustIndex = Dust.NewDust(player.position + player.velocity, player.width, player.height, DustID.Blood);
                                    Main.dust[dustIndex].velocity *= 5f;
                                }
                            }
                            SoundEngine.PlaySound(SoundID.Item71, Projectile.position);
                        }

                        if (Projectile.frame > 5)
                        {
                            Projectile.Kill();
                        }
                    }
                }
            }

            Projectile.spriteDirection = player.direction;

            Projectile.Center = player.RotatedRelativePoint(player.MountedCenter, true);
            player.itemTime = 2;
            player.itemAnimation = 2;
        }
        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            hitbox = Projectile.Redemption().swordHitbox;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (NPCLists.Undead.Contains(target.type) || NPCLists.Skeleton.Contains(target.type))
                modifiers.FinalDamage *= 2f;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];

            RedeProjectile.Decapitation(target, ref damageDone, ref hit.Crit);

            Projectile.localNPCImmunity[target.whoAmI] = 30;
            target.immune[Projectile.owner] = 0;

            Vector2 dir = target.DirectionTo(player.Center);
            Vector2 drawPos = Vector2.Lerp(Projectile.Center, target.Center, 0.9f);
            RedeParticleManager.CreateSlashParticle(drawPos, dir.RotatedBy(Main.rand.NextFloat(-0.4f, 0.4f) + player.direction * MathHelper.PiOver4) * 30, .5f, Color.PaleVioletRed, 16);
            for (int i = 0; i < 4; i++)
            {
                float randomRotation = Main.rand.NextFloat(-0.5f, 0.5f);
                float randomVel = Main.rand.NextFloat(2f, 4f);
                Vector2 direction = target.DirectionFrom(player.Center);
                Vector2 position = target.Center - direction * 10;
                RedeParticleManager.CreateSpeedParticle(position, direction.RotatedBy(randomRotation) * randomVel * 8, 0.8f, Color.DarkRed.WithAlpha(0));
            }

            target.AddBuff(ModContent.BuffType<NecroticGougeDebuff>(), 600);
        }
        private float drawTimer;
        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];

            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            int height = texture.Height / 6;
            int y = height * Projectile.frame;
            Rectangle rect = new(0, y, texture.Width, height);
            Vector2 drawOrigin = new(texture.Width / 2, Projectile.height / 2);
            var effects = Projectile.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            int offset = Projectile.frame > 1 ? 14 : 0;

            Vector2 pos = Projectile.Center - Main.screenPosition - new Vector2(-1 * player.direction, 6 - offset);
            Main.spriteBatch.End();
            Main.spriteBatch.BeginAdditive();
            RedeDraw.DrawTreasureBagEffect(Main.spriteBatch, texture, ref drawTimer, pos, new Rectangle?(rect), Color.Red * Projectile.Opacity * glow, Projectile.rotation, drawOrigin, Projectile.scale, effects);
            Main.spriteBatch.End();
            Main.spriteBatch.BeginDefault(true);

            Main.EntitySpriteDraw(texture, pos, new Rectangle?(rect), Projectile.GetAlpha(lightColor), Projectile.rotation, drawOrigin, Projectile.scale, effects, 0);

            Texture2D slash = ModContent.Request<Texture2D>("Redemption/Items/Weapons/PreHM/Melee/KeepersClaw_SlashProj").Value;
            int height2 = slash.Height / 3;
            int y2 = height2 * (Projectile.frame - 2);
            Rectangle rect2 = new(0, y2, slash.Width, height2);
            Vector2 drawOrigin2 = new(slash.Width / 2, slash.Height / 2);

            if (Projectile.frame >= 2)
                Main.EntitySpriteDraw(slash, Projectile.Center - Main.screenPosition - new Vector2(-31 * player.direction, -99 - offset), new Rectangle?(rect2), Projectile.GetAlpha(Color.White), Projectile.rotation, drawOrigin2, Projectile.scale, effects, 0);
            return false;
        }
    }
}
