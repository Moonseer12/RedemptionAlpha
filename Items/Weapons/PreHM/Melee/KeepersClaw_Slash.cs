using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.Utilities;
using Redemption.BaseExtension;
using Redemption.Buffs.NPCBuffs;
using Redemption.Globals;
using Redemption.Particles;
using Redemption.Projectiles.Melee;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

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
            Projectile.scale = 1.25f;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Player player = Main.player[Projectile.owner];
            Projectile.scale *= player.GetAdjustedItemScale(player.HeldItem);
        }
        public override bool? CanCutTiles() => Projectile.frame is 2;
        public override bool? CanHitNPC(NPC target) => Projectile.frame is 2 ? null : false;
        public float speedBonus;
        int directionLock = 0;
        private float glow;
        Vector2 mousePoint;
        private bool parried;
        private int maxTime;
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;

            Projectile.Redemption().swordHitbox = new((int)(Projectile.spriteDirection == -1 ? Projectile.Center.X - 63 * Projectile.scale : Projectile.Center.X), (int)(Projectile.Center.Y - 55 * Projectile.scale), (int)(63 * Projectile.scale), (int)(104 * Projectile.scale));
          
            maxTime = SetUseTime(player.HeldItem.useTime);
            speedBonus = SetSpeedBonus(20, player.HeldItem.useTime);

            if (player.noItems || player.CCed || player.dead || !player.active)
                Projectile.Kill();

            if (Projectile.ai[0] == 0)
            {
                if (Projectile.owner == Main.myPlayer)
                    mousePoint = Main.MouseWorld;

                player.itemRotation = MathHelper.ToRadians(-90f * player.direction);
                player.bodyFrame.Y = 5 * player.bodyFrame.Height;
                player.ChangeDir(mousePoint.X > player.Center.X ? 1 : -1);

                glow += 0.02f * speedBonus;
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
                    if (Projectile.owner == Main.myPlayer)
                    {
                        if (Main.MouseWorld.X < player.Center.X)
                            directionLock = -1;
                        else
                            directionLock = 1;
                    }
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
                if (++Projectile.frameCounter >= maxTime / 6)
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
                            if (Projectile.owner == Main.myPlayer)
                            {
                                for (int i = 0; i < 4; i++)
                                {
                                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), player.Center, RedeHelper.PolarVector(Main.rand.NextFloat(5, 8), (mousePoint - player.Center).ToRotation() + Main.rand.NextFloat(-0.2f, 0.2f)), ProjectileType<KeepersClaw_BloodWave>(), Projectile.damage / 3, 2, player.whoAmI);
                                }
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
            bool parryActive = false;
            if (Projectile.frame is 2)
            {
                parryActive = true;
                ProjHelper.SwordClashFriendly(Projectile, player, ref parried);
            }
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
            if (NPCLists.Undead.Contains(target.type) || NPCLists.Skeleton.Contains(target.type))
                modifiers.FinalDamage *= 2f;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];

            ProjHelper.Decapitation(target, ref damageDone, ref hit.Crit);

            Projectile.localNPCImmunity[target.whoAmI] = 30;
            target.immune[Projectile.owner] = 0;

            for (int i = 0; i < 8; i++)
            {
                float randomRotation = Main.rand.NextFloat(-0.5f, 0.5f);
                float randomVel = Main.rand.NextFloat(2f, 4f) * 4;
                Vector2 direction = Vector2.UnitX.RotatedBy(-0.5f * player.direction) * player.direction;
                Vector2 position = target.Center - direction * 14;
                RedeParticleManager.CreateSharpParticle(position, direction.RotatedBy(randomRotation) * randomVel, 0.8f, Color.DarkRed, 14, 0.98f, gravity: 1);
            }

            target.AddBuff(BuffType<NecroticGougeDebuff>(), 600);
        }
        private float drawTimer;
        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];

            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Rectangle rect = texture.Frame(1, 6, 0, Projectile.frame);
            Vector2 drawOrigin = rect.Size() * 0.5f;
            var effects = Projectile.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            int offset = Projectile.frame > 1 ? 14 : 0;

            Vector2 pos = Projectile.Center - Main.screenPosition + new Vector2(player.direction, offset - 6) * Projectile.scale;
            Main.spriteBatch.End();
            Main.spriteBatch.BeginAdditive();
            RedeDraw.DrawTreasureBagEffect(Main.spriteBatch, texture, ref drawTimer, pos, new Rectangle?(rect), Color.Red * Projectile.Opacity * glow, Projectile.rotation, drawOrigin, Projectile.scale, effects);
            Main.spriteBatch.End();
            Main.spriteBatch.BeginDefault(true);

            Texture2D slash = Request<Texture2D>("Redemption/Items/Weapons/PreHM/Melee/KeepersClaw_SlashProj").Value;
            Rectangle rect2 = slash.Frame(1, 3, 0, Projectile.frame - 2);
            Vector2 drawOrigin2 = rect2.Size() * 0.5f;

            if (Projectile.frame >= 2)
                Main.EntitySpriteDraw(slash, Projectile.Center - Main.screenPosition + new Vector2(32 * player.direction, offset - 12) * Projectile.scale, rect2, Projectile.GetAlpha(Color.White), Projectile.rotation, drawOrigin2, Projectile.scale, effects, 0);

            Main.EntitySpriteDraw(texture, pos, new Rectangle?(rect), Projectile.GetAlpha(lightColor), Projectile.rotation, drawOrigin, Projectile.scale, effects, 0);
            return false;
        }
    }
}