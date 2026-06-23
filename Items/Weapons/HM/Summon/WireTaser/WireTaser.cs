using Microsoft.Xna.Framework.Graphics;
using Redemption.BaseExtension;
using Redemption.Buffs.Debuffs;
using Redemption.Buffs.NPCBuffs;
using Redemption.Globals;
using Redemption.Helpers;
using Redemption.Items.Materials.HM;
using Redemption.Items.Weapons.HM.Magic;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Items.Weapons.HM.Summon.WireTaser
{
    public class WireTaser : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ShimmerTransformToItem[Type] = ItemType<GlobalDischarge>();
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 34;
            Item.DefaultToWhip(ProjectileType<WireTaser_Proj>(), 60, 2, 12, 30);
            Item.shootSpeed = 12;
            Item.rare = ItemRarityID.LightPurple;
            Item.channel = true;
            Item.value = Item.sellPrice(0, 9, 55, 0);
        }
        public override bool MeleePrefix() => true;
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<CyberPlating>(), 4)
                .AddIngredient(ItemType<Capacitor>(), 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    public class WireTaser_Proj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Wire Taser");
            ProjectileID.Sets.IsAWhip[Type] = true;
            ElementID.ProjThunder[Type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.DefaultToWhip();

            Projectile.WhipSettings.Segments = 26;
            Projectile.WhipSettings.RangeMultiplier = .6f;
            Projectile.Redemption().TechnicallyMelee = true;
        }
        private int FrameX;
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.NextBool())
                target.AddBuff(BuffType<ElectrifiedDebuff>(), 180);

            target.AddBuff(BuffID.SwordWhipNPCDebuff, 180);
            Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
        }
        private int soundTimer;
        private bool loop;
        public override void PostAI()
        {
            if (soundTimer++ == 26 && !Main.dedServ)
                SoundEngine.PlaySound(CustomSounds.Spark1, Projectile.position);

            if (soundTimer > 20 && ++Projectile.frameCounter >= 2)
            {
                Projectile.frameCounter = 0;
                if (!loop)
                {
                    FrameX++;
                    if (FrameX >= 9)
                    {
                        loop = true;
                        FrameX = 0;
                    }
                }
            }
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (soundTimer >= 26)
            {
                Player player = Main.player[Projectile.owner];
                Rectangle hitbox1 = target.Hitbox;
                Rectangle hitbox2 = new((int)Projectile.WhipPointsForCollision[^1].X - 16, (int)Projectile.WhipPointsForCollision[^1].Y - 16, 32, 32);

                if (hitbox1.Intersects(hitbox2))
                {
                    if (!Main.dedServ)
                    {
                        SoundEngine.PlaySound(CustomSounds.ElectricNoise.WithPitchOffset(1.5f), Projectile.position);
                        SoundEngine.PlaySound(CustomSounds.StaticFlare.WithVolumeScale(2), Projectile.position);
                    }

                    if (Projectile.owner == Main.myPlayer)
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), player.Center, Vector2.Zero, ProjectileType<WireTaser_Proj2>(), Projectile.damage / 2, Projectile.knockBack, player.whoAmI, target.whoAmI);

                    Projectile.Kill();
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            List<Vector2> list = new();
            Projectile.FillWhipControlPoints(Projectile, list);

            //DrawLine(list);

            SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Main.instance.LoadProjectile(Type);
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Texture2D glow = Request<Texture2D>(Texture + "_Glow").Value;
            int width = texture.Width / 10;
            int x = width * FrameX;

            Vector2 pos = list[0];

            for (int i = 0; i < list.Count - 1; i++)
            {
                Rectangle frame = new(x, 0, width, 26);
                Vector2 origin = new(width / 2, 12);
                float scale = 1;

                if (i == list.Count - 2)
                {
                    frame.Y = 116;
                    frame.Height = 24;

                    #region Dusts
                    // For a more impactful look, this scales the tip of the whip up when fully extended, and down when curled up.
                    Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out int _, out float _);
                    float t = soundTimer / timeToFlyOut;
                    scale = MathHelper.Lerp(0.4f, 1.3f, Utils.GetLerpValue(0.1f, 0.7f, t, true) * Utils.GetLerpValue(0.9f, 0.7f, t, true));

                    float dustChance = Utils.GetLerpValue(0.1f, 0.7f, t, clamped: true) * Utils.GetLerpValue(0.9f, 0.7f, t, clamped: true);

                    // Spawn dust
                    if (dustChance > 0.5f && Main.rand.NextFloat() < dustChance * 0.7f)
                    {
                        Vector2 outwardsVector = list[^2].DirectionTo(list[^1]).SafeNormalize(Vector2.Zero);
                        Dust dust = Dust.NewDustDirect(list[^1] - texture.Size() / 2, texture.Width, texture.Height, DustID.Electric, 0f, 0f, 100, default, Main.rand.NextFloat(1f, 1.5f));

                        dust.noGravity = true;
                        dust.velocity *= Main.rand.NextFloat() * 0.8f;
                        dust.velocity += outwardsVector * 0.8f;
                    }
                    #endregion
                }
                else if (i % 3 == 0)
                {
                    frame.Y = 92;
                    frame.Height = 16;
                }
                else if ((i + 2) % 3 == 0)
                {
                    frame.Y = 64;
                    frame.Height = 16;
                }
                else if ((i + 1) % 3 == 0)
                {
                    frame.Y = 36;
                    frame.Height = 16;
                }
                if(i == 0)
                {
                    origin = new(width / 2, 8);
                    frame.Y = 0;
                    frame.Height = 24;
                }

                Vector2 element = list[i];
                Vector2 diff = list[i + 1] - element;

                float rotation = diff.ToRotation() - MathHelper.PiOver2;
                Color color = Lighting.GetColor(element.ToTileCoordinates());

                Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, flip, 0);
                Main.EntitySpriteDraw(glow, pos - Main.screenPosition, frame, Color.White, rotation, origin, scale, flip, 0);

                pos += diff;
            }
            return false;
        }
    }
    public class WireTaser_Proj2 : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Wire Taser");
        }
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 14;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.alpha = 255;
            Projectile.DamageType = DamageClass.SummonMeleeSpeed;
            Projectile.ownerHitCheck = true;
            Projectile.ignoreWater = true;
            Projectile.Redemption().TechnicallyMelee = true;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (player.ownedProjectileCounts[Type] > 1)
                Projectile.Kill();

            float num = MathHelper.ToRadians(0f);
            Vector2 armCenter = player.RotatedRelativePoint(player.MountedCenter);
            if (Projectile.spriteDirection == -1)
                num = MathHelper.ToRadians(180f);

            Projectile.Center = armCenter;
            Projectile.rotation = Projectile.velocity.ToRotation() + num;
            Projectile.spriteDirection = Projectile.direction;
            Projectile.timeLeft = 2;
            player.ChangeDir(Projectile.direction);
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;
            player.itemRotation = (float)Math.Atan2(Projectile.velocity.Y * Projectile.direction, Projectile.velocity.X * Projectile.direction);

            if (Projectile.localAI[0]++ == 0)
            {
                NPC target = Main.npc[(int)Projectile.ai[0]];
                Projectile.velocity = Projectile.Center.DirectionTo(target.Center);
                Projectile.alpha = 0;

                if (Projectile.owner == Main.myPlayer)
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero, ProjectileType<WireTaser_Proj2_End>(), Projectile.damage, Projectile.knockBack, player.whoAmI, Projectile.whoAmI, target.whoAmI);
            }
            else if (Projectile.localAI[0] >= 2 && player.ownedProjectileCounts[ProjectileType<WireTaser_Proj2_End>()] <= 0)
                Projectile.Kill();
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new(texture.Width / 2, Projectile.height / 2);
            SpriteEffects spriteEffects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Main.EntitySpriteDraw(texture, Projectile.Center + Projectile.velocity * 12 - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, drawOrigin, Projectile.scale, spriteEffects, 0);
            return false;
        }
    }
    public class WireTaser_Proj2_End : ModProjectile
    {
        private static Asset<Texture2D> chainTexture;
        private static Asset<Texture2D> chainGlow;
        public override void Load()
        {
            if (Main.dedServ)
                return;
            chainTexture = Request<Texture2D>("Redemption/Items/Weapons/HM/Summon/WireTaser/WireTaser_Proj2_Chain");
            chainGlow = Request<Texture2D>("Redemption/Items/Weapons/HM/Summon/WireTaser/WireTaser_Proj2_Chain_Glow");
        }
        public override void Unload()
        {
            chainTexture = null;
            chainGlow = null;
        }

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Wire Taser");
            ElementID.ProjThunder[Type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.SummonMeleeSpeed;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 5;
            Projectile.Redemption().TechnicallyMelee = true;
        }
        public override void AI()
        {
            if (++Projectile.frameCounter >= 2)
            {
                Projectile.frameCounter = 0;
                if (++FrameX >= 9)
                    FrameX = 0;
                if (++ChainFrameX >= 9)
                    ChainFrameX = 0;
                if (++ChainFrameY > 2)
                    ChainFrameY = 0;
            }
            Projectile handle = Main.projectile[(int)Projectile.ai[0]];
            NPC target = Main.npc[(int)Projectile.ai[1]];
            if (!handle.active)
                Projectile.Kill();

            Projectile.rotation = (handle.Center - Projectile.Center).ToRotation() + MathHelper.PiOver2;
            Projectile.timeLeft = 2;
            switch (Projectile.localAI[0])
            {
                case 0:
                    if (Projectile.localAI[1]++ >= 60 || !target.active || Projectile.DistanceSQ(handle.Center) >= 900 * 900)
                    {
                        Projectile.localAI[0] = 1;
                        break;
                    }
                    Projectile.Center = target.Center;
                    break;
                case 1:
                    Projectile.Move(handle.Center, 40, 1);
                    if (Projectile.DistanceSQ(handle.Center) < 20 * 20)
                        Projectile.Kill();
                    break;
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Player player = Main.player[Projectile.owner];
            if (Helper.CheckLinearCollision(player.Center, Projectile.Center, targetHitbox, out Vector2 colissionPoint))
                return true;

            return projHitbox.Intersects(targetHitbox);
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.Knockback *= 0;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (target.knockBackResist > 0)
                target.AddBuff(BuffType<StunnedDebuff>(), (int)(40 * target.knockBackResist));
            target.AddBuff(BuffType<ElectrifiedDebuff>(), 180);
        }
        private int FrameX;
        private int ChainFrameY;
        private int ChainFrameX;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Texture2D glow = Request<Texture2D>(Texture + "_Glow").Value;
            int width = texture.Width / 10;
            int x = width * FrameX;
            Rectangle rect = new(x, 0, width, Projectile.height);
            Vector2 drawOrigin = new(width / 2, Projectile.height / 2);
            var effects = Projectile.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, new Rectangle?(rect), Projectile.GetAlpha(lightColor), Projectile.rotation, drawOrigin, Projectile.scale, effects, 0);
            Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, new Rectangle?(rect), Projectile.GetAlpha(Color.White), Projectile.rotation, drawOrigin, Projectile.scale, effects, 0);
            return false;
        }
        public override bool PreDrawExtras()
        {
            int height = chainTexture.Value.Height / 3;
            int width = chainTexture.Value.Width / 10;
            int y = height * ChainFrameY;
            int x = width * ChainFrameX;

            Vector2 handleCenter = Main.projectile[(int)Projectile.ai[0]].Center;
            Vector2 center = Projectile.Center;
            Vector2 directionToHandle = handleCenter - Projectile.Center;
            float chainRotation = directionToHandle.ToRotation() - MathHelper.PiOver2;
            float distanceToHandle = directionToHandle.Length();

            while (distanceToHandle > 20f && !float.IsNaN(distanceToHandle))
            {
                directionToHandle /= distanceToHandle; //get unit vector
                directionToHandle *= height - 2; //multiply by chain link length

                center += directionToHandle; //update draw position
                directionToHandle = handleCenter - center; //update distance
                distanceToHandle = directionToHandle.Length();

                Color drawColor = Lighting.GetColor((int)center.X / 16, (int)(center.Y / 16));

                Rectangle frame = new(x, y, width, height);
                Vector2 origin = new(width / 2, height / 2);
                //Draw chain
                Main.EntitySpriteDraw(chainTexture.Value, center - Main.screenPosition, frame, drawColor, chainRotation, origin, 1f, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(chainGlow.Value, center - Main.screenPosition, frame, Color.White, chainRotation, origin, 1f, SpriteEffects.None, 0);
            }
            return false;
        }
    }
}