using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.Core;
using Redemption.Base;
using Redemption.BaseExtension;
using Redemption.Buffs.NPCBuffs;
using Redemption.Dusts;
using Redemption.Globals;
using Redemption.Items.Weapons.HM.Melee;
using Redemption.Particles;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Items.Weapons.HM.Magic
{
    public class LightningRod : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ShimmerTransformToItem[Type] = ItemType<GravityHammer>();
            Item.staff[Item.type] = true;
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults()
        {
            Item.height = 60;
            Item.width = 60;
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(0, 2, 0, 0);

            Item.useStyle = ItemUseStyleID.Shoot;
            Item.UseSound = CustomSounds.ChainCurrents.WithVolumeScale(0.3f) with { MaxInstances = 5, SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest };
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.autoReuse = true;

            Item.DamageType = DamageClass.Magic;
            Item.damage = 100;
            Item.mana = 10;
            Item.knockBack = 3;
            Item.noMelee = true;

            Item.shootSpeed = 10;
            Item.shoot = ProjectileType<LightningRod_Conductor>();
        }
        public override bool CanUseItem(Player player) => player.altFunctionUse == 2 || player.ownedProjectileCounts[ProjectileType<LightningRod_Conductor>()] > 0;
        public override bool AltFunctionUse(Player player) => true;
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                SoundEngine.PlaySound(SoundID.DD2_DefenseTowerSpawn, position);
                Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            }
            else
            {
                Projectile.NewProjectile(source, position, velocity, ProjectileType<LightningRod_Proj>(), damage, knockback, player.whoAmI);
            }
            return false;
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                if (player.ownedProjectileCounts[type] >= 1)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        int num = 9999999;
                        int preExist = -1;
                        foreach (Projectile proj in Main.ActiveProjectiles)
                        {
                            if (proj.type != type)
                                continue;

                            if (proj.timeLeft < num)
                            {
                                preExist = proj.whoAmI;
                                num = proj.timeLeft;
                            }
                        }
                        if (preExist > -1)
                            Main.projectile[preExist].Kill();
                    }
                }
            }
            else
            {
                foreach (Projectile proj in Main.ActiveProjectiles)
                {
                    if (!proj.active || proj.owner != player.whoAmI || proj.type != ProjectileType<LightningRod_Conductor>())
                        continue;

                    velocity = position.DirectionTo(proj.Top) * (proj.Top - position).Length();
                    break;
                }
            }
        }
    }
    public class LightningRod_Conductor : ModProjectile
    {
        public override string Texture => "Redemption/Items/Weapons/HM/Magic/LightningRod";
        public override void SetStaticDefaults()
        {
            ElementID.ProjThunder[Type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;

            Projectile.DamageType = DamageClass.Magic;
            Projectile.friendly = false;
            Projectile.hostile = false;

            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;

            Projectile.penetrate = -1;
            Projectile.timeLeft = Projectile.SentryLifeTime;
        }
        public ref float Charge => ref Projectile.ai[0];
        public ref float Timer => ref Projectile.localAI[0];
        public ref float DischargeTimer => ref Projectile.localAI[1];
        public Player Owner => Main.player[Projectile.owner];
        public override bool ShouldUpdatePosition() => false;
        private Vector2 origPos;
        public override void OnSpawn(IEntitySource source)
        {
            origPos = Projectile.Center;
        }
        public override void AI()
        {
            if (!Owner.active || Owner.dead)
                Projectile.Kill();

            Lighting.AddLight(Projectile.Center, 0.5f, 0.5f, 1);
            Projectile.rotation = MathF.PI * -0.25f;

            int dist = 100 + (int)(20 * Charge);
            for (int i = 0; i <= 1; i++)
            {
                float angle = Main.rand.NextFloat() * MathF.PI * 2;
                Vector2 vec = new(MathF.Sin(angle), MathF.Cos(angle));
                Dust d = Dust.NewDustPerfect(Projectile.Center + vec * dist, DustID.Electric);
                d.scale = 0.75f;
                d.noGravity = true;
                d.velocity = -vec * 1f;
            }
            if (Timer % 5 == 0)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Top, DustID.Electric);
                d.scale = 0.75f;
                DustHelper.DrawParticleElectricity(Projectile.Top, Projectile.Top + RedeHelper.PolarVector(30, RedeHelper.RandomRotation()), 0.5f, 10, 0.05f);
                DustHelper.DrawParticleElectricity(Projectile.Top, Projectile.Top + RedeHelper.PolarVector(30, RedeHelper.RandomRotation()), 0.5f, 10, 0.05f);
            }
            if (Timer++ % 30 == 0 && Charge < 9)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active || npc.friendly || npc.dontTakeDamage)
                        continue;

                    if (npc.DistanceSQ(Projectile.Center) > dist * dist)
                        continue;

                    if (!Main.dedServ)
                        SoundEngine.PlaySound(CustomSounds.Zap2 with { Volume = .5f }, Projectile.position);

                    Vector2 pos = Projectile.Center;
                    Vector2 vel = pos.DirectionTo(npc.Center);
                    ParticleSystem.NewParticle(pos, vel * (pos - npc.Center).Length(), new ElectricParticle(20, 60, 1), new Color(100, 255, 255), 1);

                    int hitDirection = npc.RightOfDir(Projectile);
                    BaseAI.DamageNPC(npc, Projectile.damage, Projectile.knockBack, hitDirection, Projectile, crit: Projectile.HeldItemCrit());
                }
            }
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (!proj.active || proj.type != ProjectileType<LightningRod_Proj>())
                    continue;

                if (Projectile.Hitbox.Intersects(proj.Hitbox))
                {
                    if (!Main.dedServ)
                        SoundEngine.PlaySound(CustomSounds.SparkPierce with { Volume = .5f }, Projectile.position);

                    if (Charge < 9)
                        Charge++;

                    for (float k = 0; k <= 6.28f; k += 0.2f)
                    {
                        Vector2 vec = new(MathF.Sin(k), MathF.Cos(k));
                        Dust d = Dust.NewDustPerfect(Projectile.Top, DustID.Electric);
                        d.scale = 0.75f;
                        d.noGravity = true;
                        d.velocity = vec * 2f;
                    }
                    proj.Kill();
                }
            }
            if (Charge >= 9)
            {
                if (DischargeTimer == 0)
                {
                    if (!Main.dedServ)
                        SoundEngine.PlaySound(CustomSounds.Zap1 with { Volume = .5f }, Projectile.position);
                }
                Dust.NewDustPerfect(Projectile.Top, DustID.Electric, -Vector2.UnitY.RotateRandom(2) * 3);

                Vector2 rand = new(Main.rand.NextFloat(-1, 1), Main.rand.NextFloat(-1, 1));
                origPos = Projectile.Center;
                Projectile.Center = origPos + rand;
                if (DischargeTimer >= 40)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero, ProjectileType<LightningRod_Discharge>(), Projectile.damage * 10, Projectile.knockBack * 5, Projectile.owner);
                    if (!Main.dedServ)
                        SoundEngine.PlaySound(CustomSounds.MissileExplosion, Projectile.position);
                    RedeDraw.SpawnExplosion(Projectile.Center, Color.White, DustID.Electric, 60, 40, 2, 5);
                    Charge = 0;
                    DischargeTimer = 0;
                    Projectile.timeLeft = 3600;
                }
                DischargeTimer++;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffType<ElectrifiedDebuff>(), 180);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteEffects spriteEffects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = new(texture.Width / 2f, texture.Height / 2f);
            Vector2 pos = Projectile.Bottom + Vector2.UnitY * 12;
            Main.EntitySpriteDraw(texture, pos - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);
            return false;
        }
        public override void PostDraw(Color lightColor)
        {
            float glow = Charge / 9f;
            SpriteEffects spriteEffects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = new(texture.Width / 2f, texture.Height / 2f);
            Vector2 pos = Projectile.Bottom + Vector2.UnitY * 12;
            int shader = ContentSamples.CommonlyUsedContentSamples.ColorOnlyShaderIndex;

            Main.EntitySpriteDraw(texture, pos - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.BeginAdditive(true);
            GameShaders.Armor.ApplySecondary(shader, Main.LocalPlayer, null);

            Main.EntitySpriteDraw(texture, pos - Main.screenPosition, null, Color.Orange * glow, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.BeginDefault();
        }
    }
    public class LightningRod_Proj : ModProjectile
    {
        public override string Texture => Redemption.EMPTY_TEXTURE;
        public override void SetStaticDefaults()
        {
            ElementID.ProjThunder[Type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Magic;
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 480;
            Projectile.extraUpdates = 10;
        }
        public Player Owner => Main.player[Projectile.owner];
        public ref float Timer => ref Projectile.ai[0];
        private Vector2 origVel;
        private Vector2[] nodes;
        public override void OnSpawn(IEntitySource source)
        {
            Vector2 origPos = Projectile.Center + Projectile.velocity.SafeNormalize(default) * 90f;
            origVel = Projectile.velocity - Projectile.velocity.SafeNormalize(default) * 90f;
            Projectile.velocity = Projectile.velocity.SafeNormalize(default);

            int nodeCount = (int)Vector2.Distance(origPos, origPos + origVel) / 40;
            nodes = new Vector2[nodeCount + 1];
            nodes[0] = origPos;
            nodes[nodeCount] = origPos + origVel;
            for (int k = 1; k < nodes.Length - 1; k++)
                nodes[k] = Vector2.Lerp(origPos, origPos + origVel, k / (float)nodeCount) + Vector2.Normalize(-origVel).RotatedBy(1.58f) * Main.rand.NextFloat(-40, 40);

            for (int i = 0; i < 4; i++)
                Dust.NewDustPerfect(origPos, DustID.Electric, Scale: 0.75f);
        }
        public Vector2 EvaluatePathByDistance(Vector2[] points, float t)
        {
            if (points == null || points.Length == 0)
                return Vector2.Zero;

            if (points.Length == 1)
                return points[0];

            t = Math.Clamp(t, 0f, 1f);

            int segmentCount = points.Length - 1;

            float scaledT = t * segmentCount;

            int segmentIndex = (int)MathF.Floor(scaledT);

            if (segmentIndex >= segmentCount)
                return points[^1];

            float localT = scaledT - segmentIndex;

            Vector2 a = points[segmentIndex];
            Vector2 b = points[segmentIndex + 1];

            return Vector2.Lerp(a, b, localT);
        }
        public override void AI()
        {
            Timer += 30 / Projectile.MaxUpdates / (origVel.Length() + 1);
            Projectile.Center = EvaluatePathByDistance(nodes, Timer);

            Owner.heldProj = Projectile.whoAmI;
            Owner.ChangeDir(Projectile.velocity.X > 0 ? 1 : -1);
            Owner.itemRotation = (float)Math.Atan2(Projectile.velocity.Y * Projectile.direction, Projectile.velocity.X * Projectile.direction);

            Lighting.AddLight(Projectile.Center, 0.2f, 0.2f, 1f);
            RedeParticleManager.CreateAdditiveGlowParticle(Projectile.Center, Vector2.Zero, Vector2.One * 0.2f, Color.LightCyan, 12, style: ParticleBehaviors.ParticleFlags.Fading);
            RedeParticleManager.CreateAdditiveGlowParticle(Projectile.Center, Vector2.Zero, Vector2.One * 0.3f, Color.Cyan, 12, style: ParticleBehaviors.ParticleFlags.Fading);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffType<ElectrifiedDebuff>(), 180);
        }
    }
    public class LightningRod_Discharge : ModProjectile
    {
        public float glowTimer;

        public float glowScale = 3.5f;

        public Vector2 origCenter;
        public override string Texture => Redemption.EMPTY_TEXTURE;
        public override void SetStaticDefaults()
        {
            ElementID.ProjShadow[Type] = true;
            ElementID.ProjExplosive[Type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Magic;
            Projectile.width = 600;
            Projectile.height = 600;
            Projectile.penetrate = -1;
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.timeLeft = 30;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Main.LocalPlayer.RedemptionScreen().ScreenShakeOrigin = Projectile.Center;
            Main.LocalPlayer.RedemptionScreen().ScreenShakeIntensity += 2;

            for (int i = 0; i < 15; i++)
            {
                int dust = Dust.NewDust(Projectile.Center, 1, 1, DustType<GlowDust>(), Scale: 1);
                Main.dust[dust].velocity *= 3;
                Main.dust[dust].noGravity = true;
                Main.dust[dust].color = Color.GhostWhite with { A = 0 };
            }
        }
        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 0.5f, 0.5f, 1);
            glowTimer += 2;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D shockwave = Request<Texture2D>("Redemption/Textures/Shockwave2").Value;
            Rectangle rect = shockwave.Frame();
            Vector2 origin = rect.Size() * 0.5f;

            Texture2D glow = Request<Texture2D>("Redemption/Textures/SoftGlow").Value;
            Rectangle rect2 = glow.Frame();
            Vector2 origin2 = rect2.Size() * 0.5f;

            float opacity = Projectile.timeLeft / 30f;
            Color edge = Color.GhostWhite * (4f / (glowTimer + 1)) * opacity;
            Color core = Color.White * (4f / (glowTimer + 1)) * opacity;

            Vector2 position = Projectile.Center - Main.screenPosition;
            for (int i = 0; i < 3; i++)
            {
                Main.spriteBatch.Draw(glow, position, new Rectangle?(rect2), edge with { A = 0 }, 0, origin2, glowScale * 2, 0, 0);
                Main.spriteBatch.Draw(glow, position, new Rectangle?(rect2), core with { A = 0 }, 0, origin2, glowScale, 0, 0);
                Main.spriteBatch.Draw(glow, position, new Rectangle?(rect2), core with { A = 0 }, 0, origin2, glowScale, 0, 0);
            }
            float shockwaveScale = EaseFunction.EaseQuadOut.Ease(1 - opacity) * glowScale * 0.15f;
            Main.spriteBatch.Draw(shockwave, position, new Rectangle?(rect), edge with { A = 0 }, 0, origin, shockwaveScale, 0, 0);
            return false;
        }
    }
}
