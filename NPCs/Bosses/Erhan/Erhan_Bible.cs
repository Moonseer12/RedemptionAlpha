using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Redemption.Base;
using Redemption.Globals;
using Redemption.Projectiles.Minions;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using System.IO;

namespace Redemption.NPCs.Bosses.Erhan
{
    public class Erhan_Bible : ModProjectile
    {
        public override string Texture => "Redemption/Items/Weapons/PreHM/Summon/HolyBible";
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 36;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
        }

        private bool openBook;
        private float godrayFade = 1;

        public int HostNPC
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        // 0 - pre attack (update Erhan)
        // 1 - actual attack
        public int AttackState
        {
            get => (int)Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        public ref float AITimer => ref Projectile.ai[2];

        public int AIAttack;
        public int TimerRand;
        public Vector2 PlayerOrigin;

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write((byte)AIAttack);
            writer.Write((byte)TimerRand);
            writer.WriteVector2(PlayerOrigin);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            AIAttack = reader.ReadByte();
            TimerRand = reader.ReadByte();
            PlayerOrigin = reader.ReadVector2();
        }

        public override void AI()
        {
            NPC host = Main.npc[HostNPC];
            Player player = Main.player[host.target];
            if (!host.active || (host.type == ModContent.NPCType<Erhan>() && host.ai[0] == 4) || (host.type != ModContent.NPCType<Erhan>() && host.type != ModContent.NPCType<ErhanSpirit>()))
                Projectile.Kill();
            drawTimer++;
            Projectile.timeLeft = 10;
            switch (AIAttack)
            {
                case 0:
                    Projectile.alpha -= 4;
                    if (Projectile.alpha <= 0)
                    {
                        AIAttack++;
                    }
                    break;
                case 1:
                    Projectile.velocity *= 0.98f;
                    godrayFade -= 0.015f;
                    if (godrayFade <= 0.3)
                    {
                        openBook = true;
                        SoundEngine.PlaySound(SoundID.Item68, Projectile.position);
                        RedeDraw.SpawnExplosion(Projectile.Center, Color.White, scale: 2, noDust: true, tex: "Redemption/Textures/HolyGlow2");
                        Projectile.velocity *= 0;
                        AIAttack++;
                    }
                    break;
                case 2:
                    if (AITimer++ >= 60)
                    {
                        AITimer = 0;

                        int choice = Main.rand.Next(3, 7);
                        if (host.ModNPC is Erhan erhan)
                            choice = erhan.BibleID;
                        if (host.ModNPC is ErhanSpirit erhan2)
                            choice = erhan2.BibleID;

                        AIAttack = choice;
                        Projectile.netUpdate = true;
                    }
                    break;
                case 3: // Seeds of Virtue
                    Projectile.rotation = Projectile.velocity.X * 0.05f;
                    switch (AttackState)
                    {
                        case 0:
                            host.ai[1] = 0;
                            host.ai[2] = 1;
                            host.netUpdate = true;
                            AttackState++;
                            break;
                        case 1:
                            if (AITimer++ <= 180)
                            {
                                Projectile.Move(new Vector2(player.Center.X - 500, player.Center.Y - 270), 15, 40, false);
                                if (Projectile.Center.X < player.Center.X - 500)
                                    AITimer = 180;
                            }
                            else
                            {
                                Projectile.Move(new Vector2(player.Center.X + 500, player.Center.Y - 270), 15, 40, false);
                                if (Projectile.Center.X > player.Center.X + 500 || AITimer >= 360)
                                    AITimer = 0;
                            }
                            if (TimerRand++ % 30 == 0)
                            {
                                RedeDraw.SpawnRing(Projectile.Center, new Color(255, 255, 120));
                                SoundEngine.PlaySound(SoundID.Item42, Projectile.position);
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                    Projectile.NewProjectile(Terraria.Entity.InheritSource(host), Projectile.Center, RedeHelper.SpreadUp(10), ModContent.ProjectileType<Bible_Seed>(), Projectile.damage, Projectile.knockBack, Main.myPlayer, player.whoAmI);
                            }
                            if (TimerRand >= 460)
                                Projectile.Kill();
                            break;
                    }
                    break;
                case 4: // Dual Phalanx
                    Projectile.rotation = Projectile.velocity.X * 0.05f;
                    switch (AttackState)
                    {
                        case 0:
                            host.ai[1] = 0;
                            host.ai[2] = 2;
                            host.netUpdate = true;
                            AttackState++;
                            break;
                        case 1:
                            if (AITimer++ == 0)
                            {
                                PlayerOrigin = player.Center;
                            }
                            if (AITimer < 120)
                                Projectile.Move(new Vector2(PlayerOrigin.X - 600, player.Center.Y - 270), 18, 20, false);
                            else if (AITimer >= 120 && AITimer < 220)
                                Projectile.velocity *= 0.5f;

                            if (AITimer >= 130 && AITimer % 7 == 0 && AITimer <= 170)
                            {
                                if (!Main.dedServ)
                                    SoundEngine.PlaySound(CustomSounds.Slice3, Projectile.position);

                                SoundEngine.PlaySound(SoundID.Item125, Projectile.Center);

                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<HolyPhalanx_Proj2>(), Projectile.damage, 3, Main.myPlayer, Projectile.whoAmI, TimerRand * 60, TimerRand * 7);
                                }
                                TimerRand++;

                            }
                            if (AITimer >= 220)
                            {
                                Projectile.Move(new Vector2(PlayerOrigin.X + 600, player.Center.Y - 270), 6, 40, false);
                            }
                            if (AITimer >= 580)
                                Projectile.Kill();
                            break;
                    }
                    break;
                case 5: // Crossrays
                    Projectile.LookByVelocity();
                    Projectile.rotation += Projectile.velocity.Length() / 50 * Projectile.spriteDirection;
                    switch (AttackState)
                    {
                        case 0:
                            host.ai[1] = 0;
                            host.ai[2] = 1;
                            host.netUpdate = true;
                            AttackState++;
                            break;
                        case 1:
                            if (AITimer++ < 50)
                                Projectile.Move(new Vector2(player.Center.X + 400, player.Center.Y - 400), 10, 40, false);
                            else
                                Projectile.Move(player.Center, 18, 80);
                            if (AITimer == 50)
                            {
                                SoundEngine.PlaySound(SoundID.Item122, Projectile.position);

                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    for (int i = 0; i < 4; i++)
                                    {
                                        int p = Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, RedeHelper.PolarVector(2, MathHelper.PiOver2 * i), ModContent.ProjectileType<HolyBible_Ray>(), Projectile.damage, Projectile.knockBack, Projectile.owner, Projectile.whoAmI);
                                        Main.projectile[p].timeLeft = 390;
                                    }
                                }
                            }
                            if (AITimer >= 460)
                                Projectile.Kill();
                            break;
                    }
                    break;
                case 7: // Graceful Cover
                    break;
                case 6: // To The Heavens!
                    Projectile.rotation = Projectile.velocity.X * 0.05f;
                    switch (AttackState)
                    {
                        case 0:
                            host.ai[1] = 0;
                            host.ai[2] = 1;
                            host.netUpdate = true;
                            AttackState++;
                            break;
                        case 1:
                            if (AITimer++ == 0)
                            {
                                int tilePosY = BaseWorldGen.GetFirstTileFloor((int)(player.Center.X / 16), (int)(player.Center.Y / 16));
                                PlayerOrigin = new Vector2(player.Center.X, (tilePosY * 16) + 38);
                                SoundEngine.PlaySound(SoundID.Item68, Projectile.position);
                                RedeDraw.SpawnExplosion(Projectile.Center, Color.White, scale: 2, noDust: true, tex: "Redemption/Textures/HolyGlow2");
                                Projectile.position = PlayerOrigin + new Vector2(-600, 120);
                                Projectile.netUpdate = true;
                                SoundEngine.PlaySound(SoundID.Item68, Projectile.position);
                                RedeDraw.SpawnExplosion(Projectile.Center, Color.White, scale: 2, noDust: true, tex: "Redemption/Textures/HolyGlow2");

                                SoundEngine.PlaySound(SoundID.Item162, Projectile.position);
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    Projectile.NewProjectile(Terraria.Entity.InheritSource(host), Projectile.Center, new Vector2(2, 0), ModContent.ProjectileType<Bible_Ray>(), Projectile.damage * 4, Projectile.knockBack, Main.myPlayer, Projectile.whoAmI);
                                }
                            }
                            if (AITimer >= 80)
                                Projectile.velocity.Y = -1.5f;
                            Projectile.position.X = player.position.X - 600;
                            if (Main.netMode != NetmodeID.MultiplayerClient && AITimer != 0 && AITimer % 60 == 0 && AITimer <= 400)
                            {
                                PlayerOrigin.Y -= 96;
                                PlayerOrigin.X += Main.rand.Next(-220, 220);
                                RedeHelper.SpawnNPC(Projectile.GetSource_FromAI(), (int)PlayerOrigin.X, (int)PlayerOrigin.Y, ModContent.NPCType<Bible_Platform>());
                                if (Main.rand.NextBool(4))
                                {
                                    RedeHelper.SpawnNPC(Projectile.GetSource_FromAI(), (int)PlayerOrigin.X + 280, (int)PlayerOrigin.Y, ModContent.NPCType<Bible_Platform>());
                                    if (Main.rand.NextBool())
                                        PlayerOrigin.X += 280;
                                }
                                if (Main.rand.NextBool(4))
                                {
                                    RedeHelper.SpawnNPC(Projectile.GetSource_FromAI(), (int)PlayerOrigin.X - 280, (int)PlayerOrigin.Y, ModContent.NPCType<Bible_Platform>());
                                    if (Main.rand.NextBool())
                                        PlayerOrigin.X -= 280;
                                }
                                Projectile.netUpdate = true;
                            }
                            if (Main.netMode != NetmodeID.MultiplayerClient && AITimer == 420)
                            {
                                PlayerOrigin.Y -= 96;
                                PlayerOrigin.X += Main.rand.Next(-220, 220);
                                RedeHelper.SpawnNPC(Projectile.GetSource_FromAI(), (int)PlayerOrigin.X, (int)PlayerOrigin.Y, ModContent.NPCType<Bible_Platform2>());

                                Projectile.netUpdate = true;
                            }
                            if (AITimer >= 60)
                            {
                                for (int i = 0; i < Main.maxPlayers; i++)
                                {
                                    int playerCount = 0;
                                    int playerCount2 = 0;
                                    Player player2 = Main.player[i];
                                    if (!player2.active || player2.dead)
                                        continue;
                                    playerCount++;
                                    if (player2.Center.Y > Projectile.Center.Y)
                                        playerCount2++;
                                    if (playerCount2 >= playerCount)
                                    {
                                        host.ai[1] = 460;
                                        host.netUpdate = true;
                                        Projectile.Kill();
                                    }
                                }
                            }
                            if (AITimer >= 540)
                                Projectile.Kill();
                            break;
                    }
                    break;
                case 8: // Tough Read
                    break;
            }
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item68, Projectile.position);
            RedeDraw.SpawnExplosion(Projectile.Center, Color.White, scale: 2, noDust: true, tex: "Redemption/Textures/HolyGlow2");
        }
        private float drawTimer;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            if (openBook)
                texture = ModContent.Request<Texture2D>("Redemption/Items/Weapons/PreHM/Summon/HolyBible_Proj").Value;
            Vector2 drawOrigin = new(texture.Width / 2, Projectile.height / 2);

            if (godrayFade > 0)
            {
                float fluctuate = (float)Math.Abs(Math.Sin(Main.GlobalTimeWrappedHourly * 4.5f)) * 0.1f;
                float modifiedScale = Projectile.scale * (1 + fluctuate);

                Color godrayColor = Color.Lerp(new Color(255, 255, 120), Color.White * Projectile.Opacity, 0.5f);
                godrayColor.A = 0;
                RedeDraw.DrawGodrays(Main.spriteBatch, Projectile.Center - Main.screenPosition, godrayColor * godrayFade, 100 * modifiedScale * Projectile.Opacity, 30 * modifiedScale * Projectile.Opacity, 16);
            }

            RedeDraw.DrawTreasureBagEffect(Main.spriteBatch, texture, ref drawTimer, Projectile.Center - Main.screenPosition, null, new Color(255, 255, 120) * Projectile.Opacity, Projectile.rotation, drawOrigin, Projectile.scale, 0);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(Color.White), Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}