using Redemption.BaseExtension;
using Redemption.Globals;
using Redemption.Particles;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Redemption.Projectiles.Ranged
{
    public class CorruptedDoubleRifle_Beam : ModProjectile
    {
        public override string Texture => Redemption.EMPTY_TEXTURE;
        public override void SetStaticDefaults()
        {
            ElementID.ProjThunder[Type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.Redemption().EnergyBased = true;
            Projectile.width = 4;
            Projectile.height = 4;

            Projectile.friendly = true;
            Projectile.hostile = false;

            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;

            Projectile.timeLeft = 700;
            timeLeftMax = Projectile.timeLeft;
            Projectile.penetrate = 1;
            Projectile.extraUpdates = 25;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }
        private int timeLeftMax;
        public override bool ShouldUpdatePosition() => true;
        private Vector2 origPos;
        private Vector2 targetPos;
        public override void OnSpawn(IEntitySource source)
        {
            origPos = Projectile.Center;
            if (Main.myPlayer == Projectile.owner)
                targetPos = Main.MouseWorld;
        }
        public override void AI()
        {
            float p = EaseFunction.EaseQuinticOut.Ease(Utils.GetLerpValue(700, 100, Projectile.timeLeft, true));
            Projectile.Center = Vector2.Lerp(origPos, targetPos, p);

            Vector2 v = Projectile.position;
            Color bright = Color.Multiply(new(255, 146, 135, 0), 1);
            Color mid = Color.Multiply(new(255, 62, 55, 0), 1);
            Color dark = Color.Multiply(new(150, 20, 54, 0), 1);

            Color emberColor = Color.Multiply(Color.Lerp(bright, dark, (float)(timeLeftMax - Projectile.timeLeft) / timeLeftMax), 1);
            Color glowColor = Color.Multiply(Color.Lerp(mid, dark, (float)(timeLeftMax - Projectile.timeLeft) / timeLeftMax), 1f);
            RedeParticleManager.CreateQuadParticle(v, Vector2.Zero, new Vector2(.35f), emberColor, glowColor, 10);
        }
        public override void OnKill(int timeLeft)
        {
            Player player = Main.player[Projectile.owner];
            RedeDraw.SpawnRing(Projectile.Center, Color.IndianRed, glowScale: 3);
            if (!Main.dedServ)
                SoundEngine.PlaySound(CustomSounds.PlasmaBlast with { Volume = 0.5f }, Projectile.position);
            if (Projectile.owner == Main.myPlayer)
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero, ProjectileType<PlasmaRound_Blast>(), Projectile.damage, Projectile.knockBack, Main.myPlayer);
            player.RedemptionScreen().ScreenShakeIntensity += 3;
        }
    }
}
