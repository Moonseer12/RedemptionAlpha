using Microsoft.Xna.Framework.Graphics;
using Redemption.Base;
using Redemption.BaseExtension;
using Redemption.Globals;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Projectiles.Magic
{
    public class DragonSkull_Proj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Dragon Skull");
            ElementID.ProjFire[Type] = true;
            ElementID.ProjArcane[Type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.width = 54;
            Projectile.height = 40;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
        }
        public override bool ShouldUpdatePosition() => false;
        private int maxTime;
        public Player Owner => Main.player[Projectile.owner];
        public override void OnSpawn(IEntitySource source)
        {
            maxTime = (int)(Owner.HeldItem.useTime / Owner.GetWeaponAttackSpeed(Owner.HeldItem));
        }
        private bool faceLeft;
        private float jawRot;
        private float rotation;
        public override void AI()
        {
            if (Owner.noItems || Owner.CCed || Owner.dead || !Owner.active)
                Projectile.Kill();

            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;

            if (Main.myPlayer == Projectile.owner)
            {
                if (Owner.channel && Projectile.ai[1] == 0)
                {
                    if (Projectile.rotation >= -1.57f && Projectile.rotation <= 1.57f)
                    {
                        if (faceLeft)
                        {
                            faceLeft = false;
                            Projectile.spriteDirection = 1;
                        }
                    }
                    else
                    {
                        if (!faceLeft)
                        {
                            faceLeft = true;
                            Projectile.spriteDirection = -1;
                        }
                    }
                    int mana = Owner.inventory[Owner.selectedItem].mana;
                    if (Main.myPlayer == Projectile.owner)
                    {
                        float responsiveness = 0;
                        if (Projectile.ai[0] > 40)
                            responsiveness = 80;
                        if (Projectile.ai[0] >= 180)
                            responsiveness = 160;
                        Projectile.rotation.SlowRotation(Projectile.Center.DirectionTo(Main.MouseWorld).ToRotation(), MathF.PI / responsiveness);
                    }
                    if (Projectile.ai[0]++ == 0)
                    {
                        for (int i = 0; i < 20; i++)
                            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, Scale: 2);
                    }
                    if (Projectile.ai[0] >= 20 && Projectile.ai[0] < 30)
                    {
                        jawRot += 0.03f;
                    }
                    if (Projectile.ai[0] == maxTime)
                    {
                        SoundEngine.PlaySound(CustomSounds.FlameRise2, Projectile.position);
                        SoundEngine.PlaySound(SoundID.DD2_BetsyFlameBreath, Projectile.position);
                    }
                    Vector2 pos = Projectile.Center + Projectile.rotation.ToRotationVector2() * 6;
                    if (Projectile.ai[0] >= maxTime && Projectile.ai[0] % 3 == 0 && Projectile.ai[0] <= 180 && Main.myPlayer == Projectile.owner)
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), pos, Projectile.rotation.ToRotationVector2() * 6, ProjectileType<DragonSkullFlames_Proj>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                    if (Projectile.ai[0] == maxTime * 6)
                    {
                        jawRot += 0.15f;
                        if (BasePlayer.ReduceMana(Owner, mana * 2))
                        {
                            Owner.RedemptionScreen().ScreenShakeIntensity += 6;
                            SoundEngine.PlaySound(SoundID.Item122, Projectile.position);
                            DustHelper.DrawCircle(Projectile.Center, DustID.Torch, 2, 4, 4, 1, 2, nogravity: true);
                            Projectile.NewProjectile(Projectile.GetSource_FromAI(), pos, Vector2.Zero, ProjectileType<HeatRay>(), Projectile.damage, Projectile.knockBack, Projectile.owner, Projectile.whoAmI);
                        }
                        else
                            Projectile.ai[1] = 1;
                    }
                    if (Projectile.ai[0] >= 380)
                        Projectile.ai[1] = 1;
                }
                else
                {
                    Projectile.ai[1] = 1;
                    Projectile.alpha += 20;
                    if (Projectile.alpha >= 255)
                        Projectile.Kill();
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Texture2D jawTex = Request<Texture2D>(Texture + "_Jaw").Value;
            Vector2 origin2 = new(4, texture.Height);
            Vector2 origin1 = new(4, 8);
            if (faceLeft)
            {
                origin1 = new(4, texture.Height - 12);
                origin2 = new(4, 0);
            }
            var effects = !faceLeft ? SpriteEffects.None : SpriteEffects.FlipVertically;

            Main.EntitySpriteDraw(jawTex, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(Color.White), Projectile.rotation + (jawRot * Projectile.spriteDirection), origin1, Projectile.scale, effects, 0);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(Color.White), Projectile.rotation - (jawRot * Projectile.spriteDirection), origin2, Projectile.scale, effects, 0);
            return false;
        }
    }
}