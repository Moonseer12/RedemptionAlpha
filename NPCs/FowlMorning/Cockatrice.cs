using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Redemption.BaseExtension;
using Redemption.Biomes;
using Redemption.Dusts;
using Redemption.Globals;
using Redemption.Globals.NPC;
using Redemption.Globals.World;
using Redemption.Items.Accessories.PreHM;
using Redemption.Items.Usable.Potions;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.NPCs.FowlMorning
{
    [AutoloadBossHead]
    public class Cockatrice : ModNPC
    {
        public enum ActionState
        {
            Idle,
            Stare
        }
        public ActionState AIState
        {
            get => (ActionState)NPC.ai[0];
            set => NPC.ai[0] = (int)value;
        }
        public ref float AITimer => ref NPC.ai[1];
        public ref float TimerRand => ref NPC.ai[2];
        public ref float TimerRand2 => ref NPC.ai[3];
        public float[] oldrot = new float[5];
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 16;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.NPCBestiaryDrawModifiers value = new(0)
            {
                Velocity = 1,
                Position = new Vector2(0, 10),
                PortraitPositionYOverride = 0
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
        }

        public override void SetDefaults()
        {
            NPC.lifeMax = 440;
            NPC.damage = 30;
            NPC.defense = 4;
            NPC.knockBackResist = 0.1f;
            NPC.value = 1000;
            NPC.aiStyle = -1;
            NPC.width = 48;
            NPC.height = 60;
            NPC.SpawnWithHigherTime(30);
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.boss = true;
            tailChain = new CockatriceScarfPhys();
            if (!Main.dedServ)
                Music = MusicLoader.GetMusicSlot(Mod, "Sounds/Music/FowlMorning");
            SpawnModBiomes = new int[1] { ModContent.GetInstance<FowlMorningBiome>().Type };
            NPC.GetGlobalNPC<ElementalNPC>().OverrideMultiplier[ElementID.Psychic] *= .75f;
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot) => false;
        public override bool? CanHitNPC(NPC target) => false;

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new List<IBestiaryInfoElement> {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.DayTime,

                new FlavorTextBestiaryInfoElement("Squawk!")
            });
        }
        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * 0.6f * bossLifeScale);
            NPC.damage = (int)(NPC.damage * 0.6f);
        }
        public override void OnSpawn(IEntitySource source)
        {
            TimerRand = Main.rand.Next(180, 201);
            NPC.netUpdate = true;
        }
        private float FlareTimer;
        private bool Flare;
        private static IPhysChain tailChain;
        public override void AI()
        {
            NPC.GetGlobalNPC<NPCPhysChain>().npcPhysChain[0] = tailChain;
            NPC.GetGlobalNPC<NPCPhysChain>().npcPhysChainOffset[0] = new Vector2(-3f * NPC.spriteDirection, 4f);
            NPC.GetGlobalNPC<NPCPhysChain>().npcPhysChainDir[0] = -NPC.spriteDirection;

            Player player = Main.player[NPC.target];
            if (NPC.target < 0 || NPC.target == 255 || player.dead || !player.active)
                NPC.TargetClosest();

            DespawnHandler();

            if (AIState != ActionState.Stare || (TimerRand <= 0 && AITimer < 108))
                NPC.LookAtEntity(player);

            switch (AIState)
            {
                case ActionState.Idle:
                    if (AITimer++ >= TimerRand && NPC.velocity.Y == 0)
                    {
                        AITimer = 0;
                        TimerRand = 0;
                        AIState = ActionState.Stare;
                    }
                    if (NPC.DistanceSQ(player.Center) <= 100 * 100)
                        NPC.velocity.X *= .2f;
                    else
                    {
                        NPC.PlatformFallCheck(ref NPC.Redemption().fallDownPlatform, 28);
                        NPCHelper.HorizontallyMove(NPC, player.Center, .04f, 1, 12, 12, NPC.Center.Y > player.Center.Y, player);
                    }
                    break;
                case ActionState.Stare:
                    NPC.velocity.X *= .2f;
                    if (TimerRand == 0)
                    {
                        if (AITimer++ == 18)
                        {
                            FlareTimer = 0;
                            Flare = true;
                            NPC.Shoot(NPC.Center, ModContent.ProjectileType<Cockatrice_Ray_Tele>(), NPC.damage, NPC.DirectionTo(player.Center + player.velocity), false, SoundID.Item1, ai1: NPC.whoAmI);
                        }
                        if (AITimer == 108)
                            SoundEngine.PlaySound(SoundID.Item4 with { Volume = .5f, Pitch = -.6f}, NPC.position);
                        if (AITimer >= 138)
                        {
                            AITimer = 0;
                            TimerRand++;
                        }
                    }
                    break;
            }
        }
        public override bool? CanFallThroughPlatforms() => NPC.Redemption().fallDownPlatform;
        public override void FindFrame(int frameHeight)
        {
            if (Flare)
            {
                FlareTimer++;
                if (FlareTimer > 60)
                {
                    Flare = false;
                    FlareTimer = 0;
                }
            }

            if (AIState is ActionState.Stare)
            {
                NPC.rotation = 0;
                if (NPC.frame.Y < 10 * frameHeight)
                    NPC.frame.Y = 10 * frameHeight;

                if (++NPC.frameCounter >= 6)
                {
                    NPC.frameCounter = 0;
                    NPC.frame.Y += frameHeight;
                    if (NPC.frame.Y > 13 * frameHeight && TimerRand < 1)
                    {
                        NPC.frame.Y = 13 * frameHeight;
                        return;
                    }
                    if (NPC.frame.Y > 15 * frameHeight)
                    {
                        TimerRand = Main.rand.Next(180, 201);
                        AITimer = 0;
                        AIState = ActionState.Idle;
                        NPC.frame.Y = 0;
                        NPC.netUpdate = true;
                    }
                }
                return;
            }
            if (NPC.velocity.Y == 0)
            {
                NPC.rotation = 0;
                if (NPC.velocity.X == 0)
                    NPC.frame.Y = 0;
                else
                {
                    if (NPC.frame.Y < 2 * frameHeight)
                        NPC.frame.Y = 2 * frameHeight;

                    NPC.frameCounter += NPC.velocity.X * 0.5f;
                    if (NPC.frameCounter is >= 3 or <= -3)
                    {
                        NPC.frameCounter = 0;
                        NPC.frame.Y += frameHeight;
                        if (NPC.frame.Y > 9 * frameHeight)
                            NPC.frame.Y = 2 * frameHeight;
                    }
                }
            }
            else
            {
                NPC.rotation = NPC.velocity.X * 0.05f;
                NPC.frame.Y = frameHeight;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[NPC.type].Value;
            var effects = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            spriteBatch.Draw(texture, NPC.Center - screenPos, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, NPC.frame.Size() / 2, NPC.scale, effects, 0f);
            return false;
        }
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (Flare)
            {
                Vector2 position = NPC.Center - screenPos - new Vector2(-10 * NPC.spriteDirection, 24);
                RedeDraw.DrawEyeFlare(spriteBatch, ref FlareTimer, position, Color.Orange, NPC.rotation);
            }
        }
        private void DespawnHandler()
        {
            Player player = Main.player[NPC.target];
            if (!player.active || player.dead || !FowlMorningWorld.FowlMorningActive)
            {
                NPC.TargetClosest(false);
                player = Main.player[NPC.target];
                if (!player.active || player.dead || !FowlMorningWorld.FowlMorningActive)
                {
                    NPC.alpha += 2;
                    if (NPC.alpha >= 255)
                        NPC.active = false;
                    if (NPC.timeLeft > 10)
                        NPC.timeLeft = 10;
                    return;
                }
            }
        }
        public override bool PreKill()
        {
            if (FowlMorningWorld.FowlMorningActive)
            {
                FowlMorningWorld.ChickPoints += Main.expertMode ? 40 : 20;
                if (Main.netMode == NetmodeID.Server)
                    NetMessage.SendData(MessageID.WorldData);
            }
            return true;
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<EggShield>(), 10));
            npcLoot.Add(ItemDropRule.ByCondition(new OnFireCondition(), ModContent.ItemType<FriedChicken>(), 1, 2, 3));
        }
        public override void OnHitByItem(Player player, Item item, int damage, float knockback, bool crit)
        {
            if (NPC.life <= 0)
            {
                if (item.HasElement(ElementID.Fire))
                    Item.NewItem(NPC.GetSource_Loot(), NPC.getRect(), ModContent.ItemType<FriedChicken>(), Main.rand.Next(2, 4));
                else if (NPC.FindBuffIndex(BuffID.OnFire) != -1 || NPC.FindBuffIndex(BuffID.OnFire3) != -1)
                    Item.NewItem(NPC.GetSource_Loot(), NPC.getRect(), ModContent.ItemType<FriedChicken>(), Main.rand.Next(2, 4));
            }
        }
        public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit)
        {
            if (NPC.life <= 0)
            {
                if (projectile.HasElement(ElementID.Fire))
                    Item.NewItem(NPC.GetSource_Loot(), NPC.getRect(), ModContent.ItemType<FriedChicken>(), Main.rand.Next(2, 4));
                else if (NPC.FindBuffIndex(BuffID.OnFire) != -1 || NPC.FindBuffIndex(BuffID.OnFire3) != -1)
                    Item.NewItem(NPC.GetSource_Loot(), NPC.getRect(), ModContent.ItemType<FriedChicken>(), Main.rand.Next(2, 4));
            }
        }
        public override void HitEffect(int hitDirection, double damage)
        {
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 8; i++)
                    Dust.NewDust(NPC.position + NPC.velocity, NPC.width, NPC.height, DustID.Smoke,
                        NPC.velocity.X * 0.5f, NPC.velocity.Y * 0.5f);

                for (int i = 0; i < 50; i++)
                {
                    int dust = Dust.NewDust(NPC.position + NPC.velocity, NPC.width, NPC.height, ModContent.DustType<ChickenFeatherDust3>(),
                        NPC.velocity.X * 0.5f, NPC.velocity.Y * 0.5f, Scale: 2);
                    Main.dust[dust].velocity *= 10f;
                }
            }
            if (Main.rand.NextBool(2))
                Dust.NewDust(NPC.position + NPC.velocity, NPC.width, NPC.height, ModContent.DustType<ChickenFeatherDust3>(), NPC.velocity.X * 0.5f, NPC.velocity.Y * 0.5f);
        }
    }
    internal class CockatriceScarfPhys : IPhysChain
    {
        public Texture2D GetTexture(Mod mod)
        {
            return ModContent.Request<Texture2D>("Redemption/NPCs/FowlMorning/Cockatrice_Tail").Value;
        }

        public int NumberOfSegments => 5;

        public Color GetColor(PlayerDrawSet drawInfo, Color baseColour)
        {
            return baseColour;
        }

        public Vector2 AnchorOffset => new(-2, -9);

        public Vector2 OriginOffset(int index) //padding
        {
            switch (index)
            {
                case 0:
                    return new Vector2(0, 0);
                case 1:
                    return new Vector2(-2, 0);
                case 2:
                    return new Vector2(-4, 0);
                case 3:
                    return new Vector2(-6, 0);
                default:
                    return new Vector2(-8, 0);
            }
        }

        public int Length(int index)
        {
            return index switch
            {
                0 => 14,
                _ => 16,
            };
        }

        public Rectangle GetSourceRect(Texture2D texture, int index)
        {
            return texture.Frame(NumberOfSegments, 1, NumberOfSegments - 1 - index, 0);
        }
        public Vector2 Force(Player player, int index, int dir, float gravDir, float time, NPC npc = null)
        {
            Vector2 force = new(
                -dir * 0.5f,
                Player.defaultGravity * (0.5f + NumberOfSegments * NumberOfSegments * 0.5f / (1 + index))
                );

            if (!Main.gameMenu)
            {
                float windPower = 0.6f * dir * -10;

                // Wave in the wind
                force.X += 16f * -npc.spriteDirection;
                force.Y -= 8;
                force -= npc.velocity * 2;
                force.Y += (float)(Math.Sin(time * 1f * windPower - index * Math.Sign(force.X)) * 0.25f * windPower) * 6f * dir;
            }
            return force;
        }
    }
}