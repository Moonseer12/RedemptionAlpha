using Microsoft.Xna.Framework.Graphics;
using Redemption.Base;
using Redemption.BaseExtension;
using Redemption.Buffs.NPCBuffs;
using Redemption.Globals;
using Redemption.Particles;
using Redemption.Projectiles;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Items.Weapons.PreHM.Melee
{
    public class EaglecrestJavelin_Proj : ModProjectile
    {
        public float[] oldrot = new float[4];
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Eaglecrest Javelin");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
            ElementID.ProjEarth[Type] = true;
            ProjectileLists.ProjSpear[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
        }
        public override bool? CanHitNPC(NPC target) => !target.friendly && Projectile.ai[0] >= 1 ? null : false;
        public override bool? CanCutTiles() => Projectile.ai[0] >= 1 ? null : false;
        private float glow;
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (player.noItems || player.CCed || player.dead || !player.active)
                Projectile.Kill();

            float speedBonus = 60f / player.HeldItem.useTime * player.GetAttackSpeed(DamageClass.Melee);
            if (Projectile.ai[0] == 0)
            {
                Vector2 rrp = player.RotatedRelativePoint(player.MountedCenter) + Vector2.UnitY * -10;
                ProjHelper.HoldOutProjBasics(Projectile, player, rrp);
                Projectile.Center = rrp;
                Projectile.spriteDirection = Projectile.direction;
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                Projectile.timeLeft = 300;

                player.heldProj = Projectile.whoAmI;
                player.itemTime = 2;
                player.itemAnimation = 2;
                player.itemRotation = MathHelper.ToRadians(-90f * player.direction);
                player.bodyFrame.Y = 5 * player.bodyFrame.Height;
                player.ChangeDir(Projectile.direction);

                glow += 0.02f * speedBonus;
                glow = MathHelper.Clamp(glow, 0, 0.4f);
                if (glow >= 0.4 && Projectile.localAI[0] == 0)
                {
                    Vector2 pos = Projectile.Center + Projectile.velocity * 20;
                    for (int i = 0; i < 3; i++)
                    {
                        RedeParticleManager.CreateAdditiveGlowParticle(pos, Vector2.Zero, new(2, .2f), Color.LightYellow, 12);
                        RedeParticleManager.CreateAdditiveGlowParticle(pos, Vector2.Zero, new(.2f, 1f), Color.LightYellow, 12);
                    }
                    SoundEngine.PlaySound(SoundID.Item88, Projectile.position);
                    Projectile.localAI[0] = 1;
                }
                if (!player.channel)
                {
                    if (Projectile.localAI[0] == 1)
                    {
                        Projectile.ai[0] = 1;
                        SoundEngine.PlaySound(SoundID.Item19, Projectile.position);
                        if (Projectile.owner == Main.myPlayer)
                            Projectile.velocity = RedeHelper.PolarVector(22 * speedBonus, (Main.MouseWorld - Projectile.Center).ToRotation());

                        player.itemTime = (int)(30 / speedBonus);
                        player.itemAnimation = (int)(30 / speedBonus);
                    }
                    else
                    {
                        player.itemTime = 2;
                        player.itemAnimation = 2;
                        Projectile.Kill();
                    }
                }
            }
            if (Projectile.ai[0] >= 1)
            {
                Projectile.tileCollide = true;
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                Projectile.velocity.Y += 0.2f;
                player.itemRotation -= MathHelper.ToRadians(-15f * player.direction);
            }
            if (Projectile.ai[0] == 0)
            {
                player.itemRotation = MathHelper.ToRadians(-90f * player.direction);
                player.bodyFrame.Y = 5 * player.bodyFrame.Height;
            }

            for (int k = Projectile.oldPos.Length - 1; k > 0; k--)
                oldrot[k] = oldrot[k - 1];
            oldrot[0] = Projectile.rotation;
        }
        public override void OnKill(int timeLeft)
        {
            if (Projectile.ai[0] >= 1)
                StrikeLightning();
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Collision.HitTiles(Projectile.position, oldVelocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            return true;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.localNPCImmunity[target.whoAmI] = 30;
            target.immune[Projectile.owner] = 0;
        }
        private void StrikeLightning()
        {
            Player player = Main.player[Projectile.owner];
            if (Projectile.DistanceSQ(player.Center) < 800 * 800)
                player.RedemptionScreen().ScreenShakeIntensity += 5;

            for (int i = 0; i < 10; i++)
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Stone, -Projectile.velocity.X * 0.01f, -5f, Scale: 2);
            for (int i = 0; i < 3; i++)
                DustHelper.DrawParticleElectricity(Projectile.Center - new Vector2(0, 400), Projectile.Center, 2f, 30, 0.1f, 1);

            RedeParticleManager.CreateAdditiveGlowParticle(Projectile.Center - new Vector2(0, 400), Vector2.Zero, new(5, .5f), Color.LightYellow, 8);
            RedeParticleManager.CreateAdditiveGlowParticle(Projectile.Center - new Vector2(0, 400), Vector2.Zero, new(.5f, 2.5f), Color.LightYellow, 8);

            if (!Main.dedServ)
            {
                SoundEngine.PlaySound(CustomSounds.Thunderstrike, Projectile.position);
                SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact, Projectile.position);
            }
            if (Projectile.owner == Main.myPlayer)
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center - new Vector2(0, 400), new Vector2(0, 5), ProjectileType<EaglecrestJavelin_Thunder>(), (int)(Projectile.damage * .75f), 0, Projectile.owner);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteEffects spriteEffects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = new(texture.Width / 2f, texture.Height / 2f);
            int shader = ContentSamples.CommonlyUsedContentSamples.ColorOnlyShaderIndex;
            float scale = BaseUtility.MultiLerp(Main.LocalPlayer.miscCounter % 100 / 100f, 1.2f, 1.1f, 1.2f);

            Main.spriteBatch.End();
            Main.spriteBatch.BeginAdditive(true);
            GameShaders.Armor.ApplySecondary(shader, Main.LocalPlayer, null);

            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition;
                Color color = new Color(255, 180, 0) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos + new Vector2(12, 12), null, color * glow, oldrot[k], origin, Projectile.scale * scale, spriteEffects, 0);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.BeginDefault(true);

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);
            return false;
        }
    }
    public class EaglecrestJavelin_Thunder : ModRedeProjectile
    {
        public override string Texture => Redemption.EMPTY_TEXTURE;
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Lightning");
            ElementID.ProjThunder[Type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.extraUpdates = 100;
            Projectile.timeLeft = 80;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffType<ElectrifiedDebuff>(), 30);
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.Electrified, 30);
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
        }
    }
}
