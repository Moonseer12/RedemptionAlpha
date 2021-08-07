using Microsoft.Xna.Framework;
using Redemption.Base;
using Redemption.Globals;
using Redemption.Items.Critters;
using Redemption.Items.Materials.PreHM;
using Redemption.NPCs.PreHM;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace Redemption.NPCs.Critters
{
    public class SandskinSpider : ModNPC
    {
        private enum ActionState
        {
            Begin,
            Idle,
            Wander,
            Hop,
            DigDown,
            DigUp,
            Buried
        }

        public ref float AIState => ref NPC.ai[0];

        public ref float AITimer => ref NPC.ai[1];

        public ref float TimerRand => ref NPC.ai[2];

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 4;
            NPCID.Sets.CountsAsCritter[Type] = true;
            NPCID.Sets.DontDoHardmodeScaling[Type] = true;
            NPCID.Sets.TakesDamageFromHostilesWithoutBeingFriendly[Type] = true;

            NPCID.Sets.NPCBestiaryDrawModifiers value = new(0)
            {
                Velocity = 1f
            };

            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
        }

        public override void SetDefaults()
        {
            NPC.width = 18;
            NPC.height = 10;
            NPC.defense = 0;
            NPC.lifeMax = 5;
            NPC.HitSound = SoundID.NPCHit13;
            NPC.DeathSound = SoundID.NPCDeath16;
            NPC.value = 0;
            NPC.knockBackResist = 0.5f;
            NPC.aiStyle = -1;
            NPC.behindTiles = true;
            NPC.catchItem = (short)ModContent.ItemType<SandskinSpiderItem>();
        }

        public NPC npcTarget;
        public Vector2 moveTo;
        public int hopCooldown;

        public override void AI()
        {
            NPC.TargetClosest();
            NPC.LookByVelocity();

            if (hopCooldown > 0)
                hopCooldown--;

            NPC.catchItem = (short)ModContent.ItemType<SandskinSpiderItem>();

            switch (AIState)
            {
                case (float)ActionState.Begin:
                    TimerRand = Main.rand.Next(80, 180);
                    AIState = (float)ActionState.Idle;
                    break;

                case (float)ActionState.Idle:
                    if (NPC.velocity.Y == 0)
                        NPC.velocity.X *= 0.5f;
                    AITimer++;
                    if (AITimer >= TimerRand)
                    {
                        moveTo = NPC.FindGround(10);
                        AITimer = 0;
                        TimerRand = Main.rand.Next(120, 260);
                        AIState = (float)ActionState.Wander;
                    }

                    HopCheck();

                    Point tileBelow = NPC.Bottom.ToTileCoordinates();
                    Tile tile = Main.tile[tileBelow.X, tileBelow.Y];
                    if (BuryCheck() && Main.rand.NextBool(60) && tile is { IsActiveUnactuated: true } && Main.tileSolid[tile.type] && TileID.Sets.Conversion.Sand[tile.type])
                    {
                        NPC.velocity.Y = 0;
                        AITimer = 0;
                        AIState = (float)ActionState.DigDown;
                    }

                    if (RedeHelper.ClosestNPC(ref npcTarget, 100, NPC.Center) && npcTarget.damage > 0)
                    {
                        moveTo = NPC.FindGround(10);
                        AITimer = 0;
                        TimerRand = Main.rand.Next(120, 260);
                        AIState = (float)ActionState.Wander;
                    }
                    break;

                case (float)ActionState.Wander:
                    HopCheck();

                    tileBelow = NPC.Bottom.ToTileCoordinates();
                    tile = Main.tile[tileBelow.X, tileBelow.Y];
                    if (BuryCheck() && Main.rand.NextBool(100) && tile is { IsActiveUnactuated: true } && Main.tileSolid[tile.type] && tile.Slope == SlopeType.Solid && TileID.Sets.Conversion.Sand[tile.type])
                    {
                        NPC.velocity.Y = 0;
                        AITimer = 0;
                        AIState = (float)ActionState.DigDown;
                    }

                    if (RedeHelper.ClosestNPC(ref npcTarget, 100, NPC.Center) && npcTarget.damage > 0)
                    {
                        RedeHelper.HorizontallyMove(NPC,
                            new Vector2(npcTarget.Center.X < NPC.Center.X ? NPC.Center.X + 50 : NPC.Center.X - 50,
                                NPC.Center.Y), 0.5f, 2, 4, 2, false);
                        return;
                    }

                    AITimer++;
                    if (AITimer >= TimerRand || NPC.Center.X + 20 > moveTo.X * 16 && NPC.Center.X - 20 < moveTo.X * 16)
                    {
                        AITimer = 0;
                        TimerRand = Main.rand.Next(120, 260);
                        AIState = (float)ActionState.Idle;
                    }

                    RedeHelper.HorizontallyMove(NPC, moveTo * 16, 0.2f, 1, 4, 2, false);
                    break;

                case (float)ActionState.Hop:
                    if (BaseAI.HitTileOnSide(NPC, 3))
                    {
                        moveTo = NPC.FindGround(10);
                        hopCooldown = 180;
                        TimerRand = Main.rand.Next(120, 260);
                        AIState = (float)ActionState.Wander;
                    }
                    break;

                case (float)ActionState.DigDown:
                    NPC.velocity.X = 0;
                    NPC.noGravity = true;
                    NPC.noTileCollide = true;
                    NPC.velocity.Y = 0.08f;

                    if (NPC.soundDelay == 0)
                    {
                        SoundEngine.PlaySound(SoundID.Roar, (int)NPC.position.X, (int)NPC.position.Y, 1, volumeScale: 0.3f);
                        NPC.soundDelay = 20;
                    }
                    for (int i = 0; i < 2; i++)
                    {
                        int dustIndex = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Sand);
                        Main.dust[dustIndex].velocity.Y = -2.6f;
                        Main.dust[dustIndex].velocity.X *= 0f;
                        Main.dust[dustIndex].noGravity = true;
                    }

                    AITimer++;
                    if (AITimer >= 60)
                    {
                        NPC.velocity *= 0;
                        AITimer = 0;
                        AIState = (float)ActionState.Buried;
                    }
                    break;

                case (float)ActionState.Buried:
                    NPC.dontTakeDamage = true;
                    NPC.catchItem = 0;
                    Point tileIn = NPC.Bottom.ToTileCoordinates();
                    Tile tile2 = Main.tile[tileIn.X, tileIn.Y];
                    if (tile2 is not { IsActiveUnactuated: true } || !Main.tileSolid[tile2.type] || !TileID.Sets.Conversion.Sand[tile2.type])
                    {
                        AITimer = 0;
                        NPC.dontTakeDamage = false;
                        NPC.noGravity = false;
                        NPC.noTileCollide = false;
                        moveTo = NPC.FindGround(10);
                        TimerRand = Main.rand.Next(120, 260);
                        AIState = (float)ActionState.Wander;
                    }
                    if ((!BuryCheck() && Main.rand.NextBool(200)))
                    {
                        NPC.dontTakeDamage = false;
                        AITimer = 0;
                        AIState = (float)ActionState.DigUp;
                    }
                    break;

                case (float)ActionState.DigUp:
                    NPC.dontTakeDamage = false;
                    NPC.velocity.X = 0;
                    NPC.velocity.Y = -0.08f;

                    if (NPC.soundDelay == 0)
                    {
                        SoundEngine.PlaySound(SoundID.Roar, (int)NPC.position.X, (int)NPC.position.Y, 1, volumeScale: 0.3f);
                        NPC.soundDelay = 20;
                    }
                    for (int i = 0; i < 2; i++)
                    {
                        int dustIndex = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Sand);
                        Main.dust[dustIndex].velocity.Y = -2.6f;
                        Main.dust[dustIndex].velocity.X *= 0f;
                        Main.dust[dustIndex].noGravity = true;
                    }

                    AITimer++;
                    if (AITimer >= 60)
                    {
                        AITimer = 0;
                        NPC.noGravity = false;
                        NPC.noTileCollide = false;
                        moveTo = NPC.FindGround(10);
                        TimerRand = Main.rand.Next(120, 260);
                        AIState = (float)ActionState.Wander;
                    }
                    break;
            }
        }
        public bool BuryCheck()
        {
            bool spooked = false;
            foreach (NPC target in Main.npc)
            {
                if (!target.active || target.whoAmI == NPC.whoAmI ||
                    target.friendly || target.lifeMax <= 5 || target.type == ModContent.NPCType<DevilsTongue>())
                    continue;

                if (NPC.Sight(target, 300, false, true) && BaseAI.HitTileOnSide(NPC, 3))
                {
                    spooked = true;
                }
            }
            foreach (Player target in Main.player)
            {
                if (!target.active || target.dead)
                    continue;

                if (NPC.Sight(target, 300, false, true) && BaseAI.HitTileOnSide(NPC, 3))
                {
                    spooked = true;
                }
            }
            return spooked;
        }
        public void HopCheck()
        {
            if (hopCooldown == 0 && BaseAI.HitTileOnSide(NPC, 3) &&
                RedeHelper.ClosestNPC(ref npcTarget, 50, NPC.Center) && npcTarget.damage > 0)
            {
                NPC.velocity.X *= npcTarget.Center.X < NPC.Center.X ? 1.4f : -1.4f;
                NPC.velocity.Y = Main.rand.NextFloat(-2f, -5f);
                AIState = (float)ActionState.Hop;
            }
        }

        public override void FindFrame(int frameHeight)
        {
            switch (AIState)
            {
                case (float)ActionState.Begin:
                    NPC.frameCounter += NPC.velocity.X * 0.5f;
                    if (NPC.frameCounter is >= 3 or <= -3)
                    {
                        NPC.frameCounter = 0;
                        NPC.frame.Y += frameHeight;
                        if (NPC.frame.Y > 3 * frameHeight)
                        {
                            NPC.frame.Y = 0;
                        }
                    }
                    break;
                case (float)ActionState.Idle:
                    NPC.frame.Y = 0;
                    break;
                case (float)ActionState.Wander:
                    NPC.frameCounter += NPC.velocity.X * 0.5f;
                    if (NPC.frameCounter is >= 3 or <= -3)
                    {
                        NPC.frameCounter = 0;
                        NPC.frame.Y += frameHeight;
                        if (NPC.frame.Y > 3 * frameHeight)
                        {
                            NPC.frame.Y = 0;
                        }
                    }
                    break;
                case (float)ActionState.Hop:
                    NPC.frame.Y = frameHeight;
                    break;
            }
            if (AIState is (float)ActionState.DigDown or (float)ActionState.DigUp)
            {
                NPC.frameCounter++;
                if (NPC.frameCounter >= 3)
                {
                    NPC.frameCounter = 0;
                    NPC.frame.Y += frameHeight;
                    if (NPC.frame.Y > 3 * frameHeight)
                    {
                        NPC.frame.Y = 0;
                    }
                }
            }
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot) => false;

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return SpawnCondition.OverworldDayDesert.Chance * 1.8f;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Desert,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.DayTime,

                new FlavorTextBestiaryInfoElement(
                    "A spider that commonly burrows in the desert sands. They don't normally come out, due to the intense heat of the sun.")
            });
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            if (AIState is (float)ActionState.Idle)
            {
                moveTo = NPC.FindGround(10);
                AITimer = 0;
                TimerRand = Main.rand.Next(120, 260);
                AIState = (float)ActionState.Wander;
            }

            if (NPC.life <= 0)
            {
                int goreType1 = ModContent.Find<ModGore>("Redemption/SandskinSpiderGore1").Type;
                int goreType2 = ModContent.Find<ModGore>("Redemption/SandskinSpiderGore2").Type;

                Gore.NewGore(NPC.position, NPC.velocity, goreType1);
                Gore.NewGore(NPC.position, NPC.velocity, goreType2);

                for (int i = 0; i < 4; i++)
                    Dust.NewDust(NPC.position + NPC.velocity, NPC.width, NPC.height, DustID.GreenBlood,
                        NPC.velocity.X * 0.5f, NPC.velocity.Y * 0.5f);
            }

            Dust.NewDust(NPC.position + NPC.velocity, NPC.width, NPC.height, DustID.GreenBlood, NPC.velocity.X * 0.5f,
                NPC.velocity.Y * 0.5f);
        }
    }
}