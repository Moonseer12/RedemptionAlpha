﻿using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.NPCs.Bosses.Thorn
{
    public class SlashFlashPro : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Flash");
        }

        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 46;
            Projectile.aiStyle = -1;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 50;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 1f)
            {
                Projectile.alpha += 10;
                if (Projectile.alpha >= 255)
                {
                    Projectile.Kill();
                }
            }
            else
            {
                Projectile.alpha -= 10;
                if (Projectile.alpha <= 0)
                {
                    Projectile.localAI[0] = 1f;
                }
            }
        }
        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item71, Projectile.position);
            Projectile.NewProjectile(Projectile.InheritSource(Projectile), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<SlashPro1>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
        }
    }
}