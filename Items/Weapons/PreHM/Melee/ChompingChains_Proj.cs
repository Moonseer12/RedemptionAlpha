using Microsoft.Xna.Framework.Graphics;
using Redemption.BaseExtension;
using Redemption.Globals;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Items.Weapons.PreHM.Melee
{
    public class ChompingChains_Proj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Chomping Chains");
        }
        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 18;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.alpha = 255;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.ownerHitCheck = true;
            Projectile.ignoreWater = true;
            Projectile.Redemption().TechnicallyMelee = true;
        }
        public float SetSwingSpeed(float speed)
        {
            Player player = Main.player[Projectile.owner];
            return speed / player.GetAttackSpeed(DamageClass.Melee);
        }
        private float SwingSpeed;
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            float num = MathHelper.ToRadians(0f);
            if (Projectile.spriteDirection == -1)
                num = MathHelper.ToRadians(180f);

            Projectile.position = player.RotatedRelativePoint(player.MountedCenter, true) - Projectile.Size / 2f;
            Projectile.rotation = Projectile.velocity.ToRotation() + num;
            Projectile.spriteDirection = Projectile.direction;
            Projectile.timeLeft = 2;
            player.ChangeDir(Projectile.direction);
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;
            player.itemRotation = (float)Math.Atan2(Projectile.velocity.Y * Projectile.direction, Projectile.velocity.X * Projectile.direction);

            SwingSpeed = SetSwingSpeed(1);

            if (Projectile.localAI[0]++ == 0)
            {
                Projectile.alpha = 0;

                if (Projectile.owner == Main.myPlayer)
                {
                    float numberProjectiles = 3;
                    float rotation = MathHelper.ToRadians(25);
                    for (int i = 0; i < numberProjectiles; i++)
                    {
                        Vector2 perturbedSpeed = Projectile.velocity.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (numberProjectiles - 1)));
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, perturbedSpeed * 1.2f / SwingSpeed, ProjectileType<ChompingChains_Proj_Skull>(), Projectile.damage, Projectile.knockBack, player.whoAmI, Projectile.whoAmI, i);
                    }
                }
            }
            else if (Projectile.localAI[0] >= 10 && player.ownedProjectileCounts[ProjectileType<ChompingChains_Proj_Skull>()] <= 0)
                Projectile.Kill();
        }
    }
    public class ChompingChains_Proj_Skull : ModProjectile
    {
        private static Asset<Texture2D> chainTexture;
        public override void Load()
        {
            if (Main.dedServ)
                return;
            chainTexture = Request<Texture2D>("Redemption/Items/Weapons/PreHM/Melee/ChompingChains_Proj_Chain");
        }
        public override void Unload()
        {
            chainTexture = null;
        }

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Chomping Chains");
            Main.projFrames[Projectile.type] = 3;
        }
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.Redemption().TechnicallyMelee = true;
        }
        NPC nommed;
        Vector2 locked;
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Projectile handle = Main.projectile[(int)Projectile.ai[0]];
            if (!handle.active)
                Projectile.Kill();

            Projectile.frame = (int)Projectile.ai[1];
            Projectile.timeLeft = 2;
            float num = MathHelper.ToRadians(0f);
            if (Projectile.spriteDirection == -1)
                num = MathHelper.ToRadians(180f);
            switch (Projectile.localAI[0])
            {
                case 0:
                    Projectile.rotation = Projectile.velocity.ToRotation() + num;
                    Projectile.spriteDirection = Projectile.direction;
                    if (Projectile.DistanceSQ(handle.Center) >= 240 * 240 || !player.channel)
                        Projectile.localAI[0] = 2;
                    break;
                case 1:
                    Projectile.tileCollide = false;
                    Projectile.localAI[1]++;
                    Projectile.Center = nommed.Center + locked;
                    if (Projectile.localAI[1] >= 300 || !nommed.active || Projectile.DistanceSQ(handle.Center) >= 600 * 600 || !player.channel)
                        Projectile.localAI[0] = 2;
                    break;
                case 2:
                    Projectile.tileCollide = false;
                    Projectile.Move(handle.Center, 15, 1);
                    if (Projectile.DistanceSQ(handle.Center) < 20 * 20)
                        Projectile.Kill();
                    break;
            }
        }
        public override bool? CanHitNPC(NPC target)
        {
            if (Projectile.localAI[0] == 1)
                return target == nommed ? null : false;
            return null;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Projectile.localAI[0] == 1)
                modifiers.Knockback *= 0;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.localNPCImmunity[target.whoAmI] = 20;
            target.immune[Projectile.owner] = 0;
            if (Projectile.localAI[0] == 0)
            {
                SoundEngine.PlaySound(SoundID.Item2, target.position);
                nommed = target;
                locked = Projectile.Center - target.Center - Projectile.velocity;
                Projectile.localAI[0] = 1;
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.localAI[0] == 0)
            {
                Collision.HitTiles(Projectile.position, oldVelocity, Projectile.width, Projectile.height);
                SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
                Projectile.localAI[0] = 2;
            }
            return false;
        }
        public override bool PreDrawExtras()
        {
            Vector2 handleCenter = Main.projectile[(int)Projectile.ai[0]].Center;
            Vector2 center = Projectile.Center;
            Vector2 directionToHandle = handleCenter - Projectile.Center;
            float chainRotation = directionToHandle.ToRotation() - MathHelper.PiOver2;
            float distanceToHandle = directionToHandle.Length();

            while (distanceToHandle > 20f && !float.IsNaN(distanceToHandle))
            {
                directionToHandle /= distanceToHandle; //get unit vector
                directionToHandle *= chainTexture.Height(); //multiply by chain link length

                center += directionToHandle; //update draw position
                directionToHandle = handleCenter - center; //update distance
                distanceToHandle = directionToHandle.Length();

                Color drawColor = Lighting.GetColor((int)center.X / 16, (int)(center.Y / 16));

                //Draw chain
                Main.EntitySpriteDraw(chainTexture.Value, center - Main.screenPosition,
                    chainTexture.Value.Bounds, drawColor, chainRotation,
                    chainTexture.Size() * 0.5f, 1f, SpriteEffects.None, 0);
            }
            return false;
        }
    }
}