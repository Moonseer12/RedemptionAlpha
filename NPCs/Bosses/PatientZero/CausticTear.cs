using System;
using Microsoft.Xna.Framework;
using Redemption.Dusts;
using Redemption.Globals;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.NPCs.Bosses.PatientZero
{
    public class CausticTear : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Caustic Tear");
            Main.projFrames[Projectile.type] = 4;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.penetrate = 1;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 200;
            Projectile.GetGlobalProjectile<RedeProjectile>().Unparryable = true;
        }
        public override Color? GetAlpha(Color lightColor) => Color.White * Projectile.Opacity;
        public override void AI()
        {
            if (++Projectile.frameCounter >= 3)
            {
                Projectile.frameCounter = 0;
                if (++Projectile.frame >= 4)
                    Projectile.frame = 0;
            }
            if (++Projectile.localAI[0] > 8)
                Projectile.tileCollide = true;

            Lighting.AddLight(Projectile.Center, 0, Projectile.Opacity * 0.8f, 0);
            Projectile.rotation = Projectile.velocity.ToRotation() + 1.57f;
        }
        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.NPCDeath1, Projectile.position);
            for (int i = 0; i < 10; i++)
            {
                int dustIndex = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<SludgeDust>(), Scale: 2);
                Main.dust[dustIndex].velocity *= 2f;
            }
        }
    }
    public class InfectiousBeat : CausticTear
    {
        public override string Texture => "Redemption/NPCs/Bosses/PatientZero/CausticTear";
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Infectious Beat");
            Main.projFrames[Projectile.type] = 4;
        }
        public override void SetDefaults() => base.SetDefaults();

        public override void PostAI()
        {
            Projectile.velocity.Y += 0.2f;
        }
    }
}