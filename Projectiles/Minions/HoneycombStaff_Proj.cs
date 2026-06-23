using Microsoft.Xna.Framework.Graphics;
using Redemption.Base;
using Redemption.BaseExtension;
using Redemption.Buffs.Minions;
using Redemption.Globals;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Projectiles.Minions
{
    public class HoneycombStaff_Proj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 48;
            Projectile.penetrate = -1;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = Projectile.SentryLifeTime;
            Projectile.sentry = true;
            Projectile.DamageType = DamageClass.Summon;
        }
        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(BuffType<HoneycombStaffBuff>());
                return false;
            }

            if (!owner.HasBuff(BuffType<HoneycombStaffBuff>()))
                Projectile.Kill();

            return true;
        }
        public override bool? CanDamage() => false;
        private ref float Timer => ref Projectile.ai[0];
        private ref float HitCount => ref Projectile.ai[1];
        private ref float HitCountTimer => ref Projectile.ai[2];
        private Player Owner => Main.player[Projectile.owner];
        private float hitDirection;
        public override void AI()
        {
            if (!Projectile.tileCollide)
            {
                if (!CheckActive(Owner))
                    return;

                if (++Timer % 60 == 0)
                {
                    if (Main.myPlayer == Projectile.owner)
                    {
                        int p = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, RedeHelper.Spread(2), ProjectileID.Bee, Projectile.damage, Projectile.knockBack, Owner.whoAmI);
                        Main.projectile[p].usesIDStaticNPCImmunity = false;
                        Main.projectile[p].usesLocalNPCImmunity = true;
                        Main.projectile[p].localNPCHitCooldown = 10;
                        Main.projectile[p].Redemption().FromMinion = true;
                    }
                }

                float intensity = MathHelper.Clamp(HitCountTimer / 30f, HitCount - 1, HitCount);
                Projectile.rotation = BaseUtility.MultiLerp(HitCountTimer / 30f, 0, 1, 0, -1, 0) * MathF.Sqrt(intensity) * .5f * hitDirection;

                if (HitCount >= 3)
                {
                    if (++Projectile.frameCounter >= 6)
                    {
                        Projectile.frameCounter = 0;
                        if (++Projectile.frame > 3)
                            Projectile.frame = 2;

                        if (++frame2 > 4)
                            frame2 = 2;
                    }
                }
                else
                {
                    if (++Projectile.frameCounter >= 6)
                    {
                        Projectile.frameCounter = 0;
                        if (Projectile.frame > 0)
                            Projectile.frame--;

                        if (++frame2 > 2)
                            frame2 = 0;
                    }
                }
                if (HitCountTimer > 0)
                    HitCountTimer--;
                if (HitCount > 0)
                {
                    if (HitCountTimer <= 0)
                    {
                        HitCount--;
                        if (HitCount > 0)
                            HitCountTimer = 30;
                    }
                }
                if (HitCountTimer > 20)
                    return;

                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];

                    if (proj == null || !proj.active || !ProjectileID.Sets.IsAWhip[proj.type])
                        continue;

                    bool colliding = false;

                    for (int n = 0; n < proj.WhipPointsForCollision.Count; n++)
                    {
                        var point = proj.WhipPointsForCollision[n].ToPoint();
                        var myRect = new Rectangle(0, 0, proj.width, proj.height);
                        myRect.Location = new Point(point.X - myRect.Width / 2, point.Y - myRect.Height / 2);

                        if (myRect.Intersects(Projectile.Hitbox))
                        {
                            hitDirection = Owner.Center.X < Projectile.Center.X ? 1 : -1;
                            colliding = true;
                            break;
                        }
                    }
                    if (colliding)
                    {
                        HitCount++;
                        HitCountTimer = 30;

                        if (Main.myPlayer == Projectile.owner)
                        {
                            int p = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, RedeHelper.Spread(2), ProjectileID.GiantBee, Projectile.damage, Projectile.knockBack, Owner.whoAmI);
                            Main.projectile[p].usesIDStaticNPCImmunity = false;
                            Main.projectile[p].usesLocalNPCImmunity = true;
                            Main.projectile[p].localNPCHitCooldown = 10;
                            Main.projectile[p].Redemption().FromMinion = true;
                        }

                        if (HitCount >= 5)
                        {
                            Projectile.velocity.Y = -5;
                            Projectile.velocity.X = hitDirection * 7;
                            Projectile.tileCollide = true;
                        }
                    }
                }
            }
            else
            {
                Projectile.velocity.Y += 0.5f;
                Projectile.rotation += Projectile.velocity.Length() * 0.025f * hitDirection;
                Projectile.frame = 0;
                frame2 = 5;
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.velocity.Y != oldVelocity.Y)
                Projectile.velocity.Y = -oldVelocity.Y * 0.5f;

            if (Projectile.velocity.X != oldVelocity.X)
            {
                SoundEngine.PlaySound(SoundID.NPCDeath1, Projectile.position);
                for(int i = 0; i < 10; i++)
                    Dust.NewDust(Projectile.position, 16, 16, DustID.Hive);

                NPC.NewNPC(Projectile.GetSource_FromThis(), (int)Projectile.Center.X, (int)Projectile.Center.Y, NPCID.Bee);
                NPC.NewNPC(Projectile.GetSource_FromThis(), (int)Projectile.Center.X, (int)Projectile.Center.Y, NPCID.BeeSmall);
                NPC.NewNPC(Projectile.GetSource_FromThis(), (int)Projectile.Center.X, (int)Projectile.Center.Y, NPCID.BeeSmall);
                Projectile.Kill();
            }
            return false;
        }
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            width = 16;
            height = 16;
            return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
        }
        private int frame2;
        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> texture = TextureAssets.Projectile[Type];
            Rectangle rect = texture.Frame(1, 7, 0, frame2);
            Vector2 origin = new(rect.Width / 2, 0);
            Vector2 pos = Projectile.Center + Vector2.UnitY * -24;
            if (frame2 == 5)
            {
                origin = rect.Size() / 2;
                pos = Projectile.Center;
            }   
            Main.EntitySpriteDraw(texture.Value, pos - Main.screenPosition, new Rectangle?(rect), Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, 0, 0);
            return false;
        }
        public override void PostDraw(Color lightColor)
        {
            Asset<Texture2D> texture = Request<Texture2D>(Texture + "_Emote");
            Rectangle rect = texture.Frame(1, 4, 0, Projectile.frame);
            Vector2 origin = rect.Size() / 2;
            Vector2 pos = Projectile.Center + Vector2.UnitX * 24 + Vector2.UnitY * -36;
            Main.EntitySpriteDraw(texture.Value, pos - Main.screenPosition, new Rectangle?(rect), Projectile.GetAlpha(lightColor), 0, origin, 1, 0, 0);
        }
    }
}