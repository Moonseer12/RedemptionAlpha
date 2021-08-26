using Redemption.Dusts.Tiles;
using Redemption.NPCs.PreHM;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace Redemption.Projectiles.Hostile
{
    public class AncientGladestonePillar : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 34;
            Projectile.height = 110;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.alpha = 255;
            Projectile.tileCollide = false;
            Projectile.hide = true;
        }
        public override bool? CanHitNPC(NPC target)
        {
            return Projectile.velocity.Length() != 0 && target.type != ModContent.NPCType<AncientGladestoneGolem>();
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }
        public override void AI()
        {
            if (Main.rand.Next(2) == 0 && Projectile.localAI[0] < 30)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<SlateDust>(), Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, Scale: 2);
            }
            if (Projectile.velocity.Length() != 0)
            {
                Projectile.hostile = true;
                Projectile.friendly = true;
            }
            else
            {
                Projectile.hostile = false;
                Projectile.friendly = false;
            }
            Projectile.localAI[0]++;
            if (Projectile.localAI[0] < 30)
                Projectile.alpha -= 10;
            else if (Projectile.localAI[0] == 30)
            {
                //if (!Main.dedServ) { SoundEngine.PlaySound(Mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/EarthBoom").WithVolume(.3f), (int)projectile.position.X, (int)Projectile.position.Y); }
                Projectile.velocity.Y -= 10;
            }
            else if (Projectile.localAI[0] == 40)
                Projectile.velocity.Y = 0;
            else if (Projectile.localAI[0] > 45)
            {
                Projectile.alpha += 10;
                if (Projectile.alpha >= 255)
                {
                    Projectile.Kill();
                }
            }
            for (int p = 0; p < Main.maxPlayers; p++)
            {
                Player target = Main.player[p];
                if (target.noKnockback || Projectile.velocity.Length() == 0 || !Projectile.Hitbox.Intersects(target.Hitbox))
                    continue;

                target.velocity.Y = Projectile.velocity.Y * 1.5f;
            }
            foreach (NPC target in Main.npc.Take(Main.maxNPCs))
            {
                if (target.knockBackResist <= 0 || Projectile.velocity.Length() == 0 ||
                    !Projectile.Hitbox.Intersects(target.Hitbox))
                    continue;

                target.velocity.Y = Projectile.velocity.Y * 1.5f;
            }
        }
    }
}