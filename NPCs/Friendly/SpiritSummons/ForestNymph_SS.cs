using Microsoft.Build.Execution;
using Microsoft.Xna.Framework.Graphics;
using Redemption.Base;
using Redemption.BaseExtension;
using Redemption.Globals;
using Redemption.Globals.NPC;
using Redemption.Projectiles.Hostile;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using static Redemption.NPCs.PreHM.ForestNymph;

namespace Redemption.NPCs.Friendly.SpiritSummons
{
    public class ForestNymph_SS : SSBase
    {
        public override string Texture => "Redemption/NPCs/PreHM/ForestNymph";
        public enum ActionState
        {
            Idle,
            Wander,
            Alert,
            Retreating,
            Attacking,
            Slash,
            RootAtk,
            SoulMove = 10
        }
        public ActionState AIState
        {
            get => (ActionState)NPC.ai[0];
            set => NPC.ai[0] = (int)value;
        }

        public int HairExtType;
        public bool HasHat;
        public int EyeType;
        public int HairType;
        public int FlowerType;
        public Vector2 EyeOffset;
        public readonly int VisionRange = 600;

        public ref float AITimer => ref NPC.ai[1];
        public ref float TimerRand => ref NPC.ai[2];
        public override void SetSafeStaticDefaults()
        {
            // DisplayName.SetDefault("Forest Nymph");
            Main.npcFrameCount[NPC.type] = 10;
            NPCID.Sets.AllowDoorInteraction[Type] = true;

            BuffNPC.NPCTypeImmunity(Type, BuffNPC.NPCDebuffImmuneType.Inorganic);
            ElementID.NPCNature[Type] = true;
        }
        public override void SetSafeDefaults()
        {
            NPC.width = 44;
            NPC.height = 48;
            NPC.damage = 28;
            NPC.defense = 5;
            NPC.lifeMax = 500;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.3f;
            NPC.GetGlobalNPC<ElementalNPC>().OverrideMultiplier[ElementID.Nature] *= .75f;
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 10; i++)
                {
                    int dust = Dust.NewDust(NPC.position + NPC.velocity, NPC.width, NPC.height, DustID.DungeonSpirit,
                        NPC.velocity.X * 0.5f, NPC.velocity.Y * 0.5f, Scale: 2);
                    Main.dust[dust].velocity *= 5f;
                    Main.dust[dust].noGravity = true;
                }
            }
            Dust.NewDust(NPC.position + NPC.velocity, NPC.width, NPC.height, DustID.DungeonSpirit, 0, 0, Scale: 2);

            if (AIState is ActionState.Idle or ActionState.Wander or ActionState.Alert)
            {
                AITimer = 0;
                TimerRand = 0;
                AIState = ActionState.Attacking;
            }
        }
        public Vector2 SetEyeOffset(ref int frameHeight)
        {
            return (NPC.frame.Y / frameHeight) switch
            {
                0 => new Vector2(0, -6),
                2 => new Vector2(2, 0),
                3 => new Vector2(2, -2),
                4 => new Vector2(0, -2),
                5 => new Vector2(-2, -2),
                6 => new Vector2(0, -2),
                7 => new Vector2(-10, 0),
                8 => new Vector2(-8, 0),
                9 => new Vector2(-6, 0),
                _ => new Vector2(0, 0),
            };
        }
        private int runCooldown;
        public float[] doorVars = new float[3];
        public override void ModifyTypeName(ref string typeName)
        {
            if (NPC.ai[3] != -1)
                typeName = Main.player[(int)NPC.ai[3]].name + "'s " + Lang.GetNPCNameValue(Type);
        }
        public override void OnSpawn(IEntitySource source)
        {
            ChoosePersonality();

            TimerRand = Main.rand.Next(80, 280);
            NPC.alpha = 0;
            NPC.netUpdate = true;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(HairExtType);
            writer.Write(HasHat);
            writer.Write(EyeType);
            writer.Write(HairType);
            writer.Write(FlowerType);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            HairExtType = reader.ReadInt32();
            HasHat = reader.ReadBoolean();
            EyeType = reader.ReadInt32();
            HairType = reader.ReadInt32();
            FlowerType = reader.ReadInt32();
        }
        public override void AI()
        {
            Player player = Main.player[(int)NPC.ai[3]];
            RedeNPC globalNPC = NPC.Redemption();
            var attacker = globalNPC.attacker;
            NPC.TargetClosest();
            if (AIState is not ActionState.Alert)
                NPC.LookByVelocity();

            RegenCheck();
            switch (AIState)
            {
                case ActionState.Idle:
                    if (NPC.velocity.Y == 0)
                        NPC.velocity.X = 0;
                    AITimer++;
                    if (AITimer >= TimerRand || NPC.DistanceSQ(player.Center) > 200 * 200)
                    {
                        moveTo = NPC.FindGround(20);
                        if (NPC.DistanceSQ(player.Center) > 200 * 200)
                            moveTo = (player.Center + new Vector2(20 * NPC.RightOfDir(player), 0)) / 16;
                        AITimer = 0;
                        TimerRand = Main.rand.Next(120, 260);
                        AIState = ActionState.Wander;
                        NPC.netUpdate = true;
                    }
                    SightCheck();
                    break;

                case ActionState.Wander:
                    SightCheck();
                    EyeState = 0;

                    AITimer++;
                    if (AITimer >= TimerRand || NPC.Center.X + 20 > moveTo.X * 16 && NPC.Center.X - 20 < moveTo.X * 16)
                    {
                        AITimer = 0;
                        TimerRand = Main.rand.Next(80, 280);
                        AIState = ActionState.Idle;
                        NPC.netUpdate = true;
                    }
                    BaseAI.AttemptOpenDoor(NPC, ref doorVars[0], ref doorVars[1], ref doorVars[2], 80, 1, 10, interactDoorStyle: 2);

                    NPC.PlatformFallCheck(ref NPC.Redemption().fallDownPlatform, 20, (moveTo.Y - 32) * 16);
                    NPCHelper.HorizontallyMove(NPC, moveTo * 16, 0.4f, 1.5f, 12, 8, NPC.Center.Y > moveTo.Y * 16, player);
                    break;

                case ActionState.Alert:
                    if (NPC.velocity.Y == 0)
                        NPC.velocity.X = 0;

                    EyeState = 2;
                    if (NPC.ThreatenedCheck(ref runCooldown, 300))
                    {
                        runCooldown = 0;
                        moveTo = NPC.FindGround(20);
                        AITimer = 0;
                        TimerRand = Main.rand.Next(120, 260);
                        AIState = ActionState.Wander;
                        NPC.netUpdate = true;
                        break;
                    }
                    NPC.LookAtEntity(attacker);

                    if (!NPC.Sight(attacker, 600, false, true, false, false, headOffset: 30))
                        runCooldown++;
                    else if (runCooldown > 0)
                        runCooldown--;

                    if (AITimer++ == 0)
                    {
                        if ((globalNPC.attacker is NPC attackerNPC2 && attackerNPC2.life >= NPC.life) || NPC.DistanceSQ(globalNPC.attacker.Center) <= 100 * 100)
                        {
                            AITimer = 0;
                            TimerRand = 0;
                            AIState = ActionState.Attacking;
                        }
                    }
                    else
                    {
                        if (NPC.Sight(attacker, 600, false, true, false, false, headOffset: 30))
                            TimerRand++;

                        if (TimerRand >= 60)
                        {
                            AITimer = 0;
                            TimerRand = 0;
                            AIState = ActionState.Attacking;
                        }
                    }
                    break;

                case ActionState.Attacking:
                    if (NPC.ThreatenedCheck(ref runCooldown, 300))
                    {
                        runCooldown = 0;
                        moveTo = NPC.FindGround(20);
                        AITimer = 0;
                        TimerRand = Main.rand.Next(120, 260);
                        AIState = ActionState.Wander;
                        NPC.netUpdate = true;
                        break;
                    }
                    EyeState = 0;
                    BaseAI.AttemptOpenDoor(NPC, ref doorVars[0], ref doorVars[1], ref doorVars[2], 80, 1, 10, interactDoorStyle: 2);

                    if (!NPC.Sight(attacker, 600, false, true, false, false, headOffset: 30))
                        runCooldown++;
                    else if (runCooldown > 0)
                        runCooldown--;

                    if (NPC.velocity.Y == 0 && NPC.DistanceSQ(attacker.Center) < 60 * 60)
                    {
                        NPC.LookAtEntity(attacker);
                        AITimer = 0;
                        NPC.frameCounter = 0;
                        NPC.velocity.X = 0;
                        AIState = ActionState.Slash;
                    }
                    if (Main.rand.NextBool(200) && NPC.velocity.Y == 0 && NPC.DistanceSQ(attacker.Center) > 100 * 100)
                    {
                        AITimer = 0;
                        AIState = ActionState.RootAtk;
                        NPC.netUpdate = true;
                    }

                    NPC.PlatformFallCheck(ref NPC.Redemption().fallDownPlatform, 20, attacker.Center.Y);
                    if (NPC.life < NPC.lifeMax / 10)
                    {
                        if (Main.rand.NextBool(200) && NPC.velocity.Y == 0 && NPC.DistanceSQ(attacker.Center) > 100 * 100)
                        {
                            AITimer = 0;
                            AIState = ActionState.RootAtk;
                            NPC.netUpdate = true;
                        }
                        NPCHelper.HorizontallyMove(NPC, NPCHelper.RunAwayVector(NPC, attacker), 0.2f, 2.5f, 12, 8, NPC.Center.Y > attacker.Center.Y, attacker);
                        break;
                    }
                    NPCHelper.HorizontallyMove(NPC, attacker.Center, 0.2f, 2.5f, 12, 8, NPC.Center.Y > attacker.Center.Y, attacker);
                    break;

                case ActionState.Slash:
                    if (NPC.ThreatenedCheck(ref runCooldown, 300))
                    {
                        runCooldown = 0;
                        AITimer = 0;
                        moveTo = NPC.FindGround(20);
                        TimerRand = Main.rand.Next(120, 260);
                        AIState = ActionState.Wander;
                        NPC.netUpdate = true;
                        break;
                    }
                    NPC.LookAtEntity(attacker);
                    Rectangle SlashHitbox = new((int)(NPC.spriteDirection == -1 ? NPC.Center.X - 58 : NPC.Center.X), (int)(NPC.Center.Y - 33), 58, 80);

                    if (NPC.velocity.Y < 0)
                        NPC.velocity.Y = 0;
                    if (NPC.velocity.Y == 0)
                        NPC.velocity.X *= 0.9f;

                    if (NPC.frame.Y == 6 * 94 && AITimer++ < 3)
                        NPC.frameCounter = 0;
                    if (NPC.frame.Y == 6 * 94 && AITimer == 2 && NPC.Hitbox.Intersects(attacker.Hitbox))
                        NPC.velocity.X -= 6 * NPC.spriteDirection;

                    if (NPC.frame.Y == 7 * 94)
                        NPC.RedemptionHitbox().DamageInHitbox(NPC, 0, SlashHitbox, NPC.damage, 5f, false, 10);
                    break;

                case ActionState.RootAtk:
                    if (NPC.ThreatenedCheck(ref runCooldown, 300))
                    {
                        runCooldown = 0;
                        AITimer = 0;
                        moveTo = NPC.FindGround(20);
                        TimerRand = Main.rand.Next(120, 260);
                        AIState = ActionState.Wander;
                        NPC.netUpdate = true;
                        break;
                    }
                    EyeState = 1;
                    for (int i = 0; i < 2; i++)
                    {
                        int dustIndex = Dust.NewDust(NPC.BottomLeft, NPC.width, 1, DustID.DryadsWard, 0f, 0f, 100, default, 1);
                        Main.dust[dustIndex].velocity.Y -= 4f;
                        Main.dust[dustIndex].velocity.X *= 0f;
                        Main.dust[dustIndex].noGravity = true;
                    }

                    NPC.velocity.X *= 0.5f;

                    AITimer++;
                    if (AITimer == 5)
                    {
                        int tilePosY = BaseWorldGen.GetFirstTileFloor((int)(attacker.Center.X + (attacker.velocity.X * 30)) / 16, (int)(attacker.Bottom.Y / 16) - 2);
                        NPC.Shoot(new Vector2(attacker.Center.X + (attacker.velocity.X * 30), (tilePosY * 16) + 30), ProjectileType<LivingBloomRoot_SS>(), NPC.damage, Vector2.Zero, ai2: NPC.whoAmI);
                        foreach (NPC target in Main.ActiveNPCs)
                        {
                            if (target.whoAmI == NPC.whoAmI || target.whoAmI == attacker.whoAmI || target.Redemption().invisible)
                                continue;

                            if (target.lifeMax < 5 || target.damage == 0 || NPC.DistanceSQ(target.Center) > 400 * 400 || target.type == NPC.type)
                                continue;

                            if (Main.rand.NextBool(3))
                                continue;

                            int tilePosY2 = BaseWorldGen.GetFirstTileFloor((int)(target.Center.X + (target.velocity.X * 30)) / 16, (int)(target.Bottom.Y / 16) - 2);
                            NPC.Shoot(new Vector2(target.Center.X + (target.velocity.X * 30), (tilePosY2 * 16) + 30), ProjectileType<LivingBloomRoot_SS>(), NPC.damage, Vector2.Zero, ai2: NPC.whoAmI);
                        }
                        foreach (Player target in Main.ActivePlayers)
                        {
                            if (globalNPC.attacker is NPC)
                                continue;

                            if (NPC.DistanceSQ(target.Center) > 400 * 400)
                                continue;

                            if (Main.rand.NextBool(3))
                                continue;

                            int tilePosY2 = BaseWorldGen.GetFirstTileFloor((int)(target.Center.X + (target.velocity.X * 30)) / 16, (int)(target.Bottom.Y / 16) - 2);
                            NPC.Shoot(new Vector2(target.Center.X + (target.velocity.X * 30), (tilePosY2 * 16) + 30), ProjectileType<LivingBloomRoot_SS>(), NPC.damage, Vector2.Zero, ai2: NPC.whoAmI);
                        }
                    }
                    else if (AITimer >= 40)
                    {
                        AITimer = 0;
                        AIState = ActionState.Attacking;
                    }
                    break;
                case ActionState.SoulMove:
                    SoulMoveState(NPC, ref AITimer, player, ref TimerRand, ref runCooldown, ref moveTo, yOffset: 4);
                    break;
            }
            if (AIState is not ActionState.SoulMove)
            {
                if (NoSpiritEffect(NPC))
                    NPC.alpha = 0;
                else
                {
                    NPC.alpha += Main.rand.Next(-10, 11);
                    NPC.alpha = (int)MathHelper.Clamp(NPC.alpha, 0, 30);
                }
            }
            CustomFrames(94);
        }
        private void CustomFrames(int frameHeight)
        {
            if (AIState is ActionState.Slash)
            {
                NPC.rotation = 0;

                if (NPC.frame.Y < 4 * frameHeight)
                    NPC.frame.Y = 4 * frameHeight;

                NPC.frameCounter++;
                if (NPC.frameCounter >= 5)
                {
                    NPC.frameCounter = 0;
                    NPC.frame.Y += frameHeight;
                    if (NPC.frame.Y == 7 * frameHeight)
                    {
                        SoundEngine.PlaySound(SoundID.Item71 with { Volume = .7f }, NPC.position);
                        NPC.velocity.X += 4 * NPC.spriteDirection;
                    }
                    if (NPC.frame.Y > 8 * frameHeight)
                    {
                        NPC.frame.Y = 0;
                        NPC.frameCounter = 0;
                        AIState = ActionState.Attacking;
                    }
                }
                EyeOffset = SetEyeOffset(ref frameHeight);
                return;
            }
        }
        public override bool? CanFallThroughPlatforms() => NPC.Redemption().fallDownPlatform;
        private int EyeFrame;
        private int EyeFrameCounter;
        private int EyeState;
        public override void FindFrame(int frameHeight)
        {
            switch (EyeState)
            {
                case 0:
                    if (EyeFrameCounter++ % 10 == 0)
                    {
                        if (EyeFrame >= 2)
                            EyeFrame = 0;
                        if (EyeFrame == 1)
                            EyeFrame++;
                        if (EyeFrame == 0 && Main.rand.NextBool(20))
                            EyeFrame = 1;
                    }
                    break;
                case 1:
                    if (EyeFrameCounter++ % 10 == 0)
                    {
                        if (EyeFrame >= 2)
                            EyeFrame = 2;
                        if (EyeFrame == 1)
                            EyeFrame++;
                        if (EyeFrame == 0)
                            EyeFrame = 1;
                    }
                    break;
                case 2:
                    EyeFrame = 1;
                    break;
            }
            if (AIState is ActionState.Slash)
                return;
            if (NPC.collideY || NPC.velocity.Y == 0)
            {
                NPC.rotation = 0;
                if (NPC.velocity.X == 0)
                    NPC.frame.Y = frameHeight;
                else
                {
                    if (NPC.frame.Y < frameHeight)
                        NPC.frame.Y = frameHeight;

                    NPC.frameCounter += NPC.velocity.X * 0.5f;
                    if (NPC.frameCounter is >= 3 or <= -3)
                    {
                        NPC.frameCounter = 0;
                        NPC.frame.Y += frameHeight;
                        if (NPC.frame.Y > 4 * frameHeight)
                            NPC.frame.Y = frameHeight;
                    }
                }
            }
            else
            {
                NPC.rotation = NPC.velocity.X * 0.05f;
                NPC.frame.Y = 0;
            }
            EyeOffset = SetEyeOffset(ref frameHeight);
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (NPCLists.Dark.Contains(target.type))
                modifiers.FinalDamage *= 1.5f;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit)
        {
            int damageDone = hit.Damage;
            RedeProjectile.Decapitation(target, ref damageDone, ref hit.Crit);

            if (Main.rand.NextBool(3))
                target.AddBuff(BuffID.DryadsWardDebuff, 300);
        }
        public void SightCheck()
        {
            Player player = Main.player[(int)NPC.ai[3]];
            RedeNPC globalNPC = NPC.Redemption();

            int gotNPC = GetNearestNPC(NPC, 1);
            if (player.MinionAttackTargetNPC != -1)
                gotNPC = player.MinionAttackTargetNPC;
            if (gotNPC != -1 && (NPC.Sight(Main.npc[gotNPC], 600, true, true, false, false, headOffset: 30) || gotNPC == player.MinionAttackTargetNPC))
            {
                globalNPC.attacker = Main.npc[gotNPC];
                moveTo = NPC.FindGround(20);
                AITimer = 0;
                TimerRand = 0;
                AIState = ActionState.Alert;
                NPC.netUpdate = true;
            }
        }
        public void ChoosePersonality()
        {
            WeightedRandom<int> hair = new(Main.rand);
            hair.Add(0);
            hair.Add(1, 0.5);
            hair.Add(2, 0.5);
            hair.Add(3, 0.1);
            HairType = hair;
            FlowerType = Main.rand.Next(6);
            HairExtType = Main.rand.Next(3);
            if (Main.rand.NextBool(10))
                HasHat = true;
            WeightedRandom<int> eyes = new(Main.rand);
            eyes.Add(0);
            eyes.Add(1);
            eyes.Add(2);
            eyes.Add(3);
            eyes.Add(4, 0.1);
            EyeType = eyes;
            NPC.netUpdate = true;
        }
        int regenTimer;
        public void RegenCheck()
        {
            int regenCooldown = NPC.wet && !NPC.lavaWet ? 30 : 40;
            if ((NPC.wet && !NPC.lavaWet) || NPC.HasBuff(BuffID.Wet) || (Main.raining && NPC.position.Y < Main.worldSurface * 16 && Framing.GetTileSafely(NPC.Center).WallType == WallID.None))
            {
                regenTimer++;
                if (regenTimer % regenCooldown == 0 && NPC.life < NPC.lifeMax)
                {
                    int heal = 10;
                    NPC.life += heal;
                    NPC.HealEffect(heal);
                    if (NPC.life > NPC.lifeMax)
                        NPC.life = NPC.lifeMax;
                }
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            var effects = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Vector2 pos = NPC.Center + new Vector2(NPC.spriteDirection == -1 ? -19 : 17, -17);

            bool noSpiritEffect = NoSpiritEffect(NPC);
            Color color = noSpiritEffect ? drawColor : Color.White;
            if (!noSpiritEffect)
            {
                int shader = GameShaders.Armor.GetShaderIdFromItemId(ItemID.WispDye);
                spriteBatch.End();
                spriteBatch.BeginAdditive(true);
                GameShaders.Armor.ApplySecondary(shader, Main.LocalPlayer, null);
            }

            spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, pos - screenPos, NPC.frame, NPC.ColorTintedAndOpacity(color), NPC.rotation, NPC.frame.Size() / 2, NPC.scale, effects, 0);

            int EyeHeight = eye.Value.Height / 3;
            int EyeWidth = eye.Value.Width / 6;
            int EyeY = EyeHeight * EyeFrame;
            int EyeX = EyeWidth * EyeType;
            Rectangle EyeRect = new(EyeX, EyeY, EyeWidth, EyeHeight);
            spriteBatch.Draw(eye.Value, pos - screenPos, new Rectangle?(EyeRect), NPC.ColorTintedAndOpacity(color), NPC.rotation, NPC.frame.Size() / 2 + new Vector2(NPC.spriteDirection == -1 ? -56 : -28, -32) - new Vector2(EyeOffset.X * -NPC.spriteDirection, EyeOffset.Y), NPC.scale, effects, 0);

            int Height = hair1.Value.Height / 10;
            int Width = hair1.Value.Width / 4;
            int Height2 = hair2.Value.Height / 10;
            int Width2 = hair2.Value.Width / 4;
            int Width3 = hair3.Value.Width / 5;
            int y = Height * (NPC.frame.Y / 94);
            int x = Width * HairType;
            int y2 = Height2 * (NPC.frame.Y / 94);
            int x2 = Width2 * HairType;
            int x3 = Width3 * FlowerType;
            Rectangle rect = new(x, y, Width, Height);
            Rectangle rect2 = new(x2, y2, Width2, Height2);
            Rectangle rect3 = new(x3, 0, Width3, hair3.Value.Height);
            Texture2D h1 = hair1.Value;
            switch (HairExtType)
            {
                case 1:
                    h1 = hair1b.Value;
                    break;
                case 2:
                    h1 = hair1c.Value;
                    Height = hair1c.Value.Height / 10;
                    Width = hair1c.Value.Width / 4;
                    y = Height * (NPC.frame.Y / 94);
                    x = Width * HairType;
                    rect = new(x, y, Width, Height);
                    break;
            }
            spriteBatch.Draw(h1, pos - screenPos, new Rectangle?(rect), NPC.ColorTintedAndOpacity(color), NPC.rotation, NPC.frame.Size() / 2 + new Vector2(NPC.spriteDirection == -1 ? -58 : 6, -12) - (HairExtType == 2 ? new Vector2(NPC.spriteDirection == -1 ? -2 : 8, -2) : Vector2.Zero), NPC.scale, effects, 0);

            spriteBatch.Draw(hair2.Value, pos - screenPos, new Rectangle?(rect2), NPC.ColorTintedAndOpacity(color), NPC.rotation, NPC.frame.Size() / 2 + new Vector2(NPC.spriteDirection == -1 ? -34 : -18, -16), NPC.scale, effects, 0);
            if (FlowerType <= 4)
                spriteBatch.Draw(hair3.Value, pos - screenPos, new Rectangle?(rect3), NPC.ColorTintedAndOpacity(color), NPC.rotation, NPC.frame.Size() / 2 + new Vector2(NPC.spriteDirection == -1 ? -34 : -18, -16) - new Vector2(EyeOffset.X * -NPC.spriteDirection, EyeOffset.Y), NPC.scale, effects, 0);

            if (HasHat)
                spriteBatch.Draw(tophat.Value, pos - screenPos, null, NPC.ColorTintedAndOpacity(color), NPC.rotation, NPC.frame.Size() / 2 + new Vector2(NPC.spriteDirection == -1 ? -44 : -24, -10) - new Vector2(EyeOffset.X * -NPC.spriteDirection, EyeOffset.Y), NPC.scale, effects, 0);

            if (!noSpiritEffect)
            {
                spriteBatch.End();
                spriteBatch.BeginDefault();
            }
            return false;
        }
        public override bool CanHitNPC(NPC target) => false;
        public override bool CanHitPlayer(Player target, ref int cooldownSlot) => false;
        public override void OnKill()
        {
            RedeHelper.SpawnNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, NPCType<LostSoulNPC>(), Main.rand.NextFloat(0.6f, 1.2f));
        }
    }
    public class LivingBloomRoot_SS : LivingBloomRoot
    {
        public override string Texture => "Redemption/Projectiles/Hostile/LivingBloomRoot";
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Living Root");
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.DamageType = DamageClass.Summon;
            Projectile.extraUpdates = 1;
            Projectile.hostile = false;
            Projectile.Redemption().friendlyHostile = false;
        }
        public override bool? CanHitNPC(NPC target)
        {
            return !target.friendly && Projectile.velocity.Length() != 0 ? null : false;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            bool noSpiritEffect = SSBase.NoSpiritEffect(Main.npc[(int)Projectile.ai[2]]);
            Color color = noSpiritEffect ? lightColor : Color.White;
            if (!noSpiritEffect)
            {
                int shader = GameShaders.Armor.GetShaderIdFromItemId(ItemID.WispDye);
                Main.spriteBatch.End();
                Main.spriteBatch.BeginAdditive(true);
                GameShaders.Armor.ApplySecondary(shader, Main.LocalPlayer, null);
            }

            Vector2 drawOrigin = new(Projectile.width / 2, Projectile.height / 2);
            Main.EntitySpriteDraw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(color), Projectile.rotation, drawOrigin, Projectile.scale, 0, 0);

            if (!noSpiritEffect)
            {
                Main.spriteBatch.End();
                Main.spriteBatch.BeginDefault();
            }
            return false;
        }
    }
}