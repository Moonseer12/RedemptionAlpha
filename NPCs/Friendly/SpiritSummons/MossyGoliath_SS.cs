using Microsoft.Xna.Framework.Graphics;
using Redemption.Base;
using Redemption.BaseExtension;
using Redemption.Buffs.Debuffs;
using Redemption.Globals;
using Redemption.Globals.NPC;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.NPCs.Friendly.SpiritSummons
{
    public class MossyGoliath_SS : SSBase
    {
        public override string Texture => "Redemption/NPCs/Friendly/SpiritSummons/MossyGoliath";

        private float[] oldrot = new float[3];
        public enum ActionState
        {
            Start,
            Idle,
            Wander,
            AlertRun,
            AlertStand,
            Attack,
            SoulMove = 10
        }
        public ActionState AIState
        {
            get => (ActionState)NPC.ai[0];
            set => NPC.ai[0] = (int)value;
        }
        public ref float AITimer => ref NPC.ai[1];
        public ref float TimerRand => ref NPC.ai[2];
        public ref float AniType => ref NPC.localAI[0];
        public override void SetSafeStaticDefaults()
        {
            // DisplayName.SetDefault("Mossy Goliath");
            Main.npcFrameCount[NPC.type] = 6;

            ElementID.NPCEarth[Type] = true;

            NPCID.Sets.TrailCacheLength[NPC.type] = 3;
            NPCID.Sets.TrailingMode[NPC.type] = 1;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Venom] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffType<DirtyWoundDebuff>()] = true;
        }
        public override void SetSafeDefaults()
        {
            NPC.lifeMax = 2000;
            NPC.damage = 40;
            NPC.defense = 14;
            NPC.knockBackResist = 0f;
            NPC.aiStyle = -1;
            NPC.width = 126;
            NPC.height = 58;
            NPC.HitSound = SoundID.NPCHit24;
            NPC.DeathSound = SoundID.NPCDeath27;
        }
        public override void ModifyTypeName(ref string typeName)
        {
            if (NPC.ai[3] != -1)
                typeName = Main.player[(int)NPC.ai[3]].name + "'s " + Lang.GetNPCNameValue(Type);
        }
        private int runCooldown;

        public int runFrame;
        public int roarFrame;
        public float frameCounters;
        public int roarCooldown;
        public override void OnSpawn(IEntitySource source)
        {
            TimerRand = Main.rand.Next(80, 120);
            NPC.netUpdate = true;
        }
        public override void AI()
        {
            for (int k = oldrot.Length - 1; k > 0; k--)
                oldrot[k] = oldrot[k - 1];
            oldrot[0] = NPC.rotation;
            Player player = Main.player[(int)NPC.ai[3]];

            RedeNPC globalNPC = NPC.Redemption();
            var attacker = globalNPC.attacker;
            NPC.TargetClosest();
            NPC.LookByVelocity();

            Point point2 = NPC.Bottom.ToTileCoordinates();

            switch (AIState)
            {
                case ActionState.Start:
                    AniType = 1;
                    if (AITimer++ == 25 && !Main.dedServ)
                        SoundEngine.PlaySound(CustomSounds.Roar1, NPC.position);

                    if (AITimer == 30)
                    {
                        Main.LocalPlayer.RedemptionScreen().ScreenShakeOrigin = NPC.Center;
                        Main.LocalPlayer.RedemptionScreen().ScreenShakeIntensity += 10;
                    }
                    if (AITimer >= 80)
                    {
                        AniType = 0;
                        NPC.ai[0] = 1;
                        AITimer = 0;
                        roarFrame = 0;
                        NPC.netUpdate = true;
                    }
                    break;
                case ActionState.Idle:
                    if (NPC.velocity.Y == 0)
                        NPC.velocity.X = 0;
                    AniType = 0;
                    NPC.noTileCollide = false;
                    NPC.noGravity = false;

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
                    AniType = 0;
                    NPC.noTileCollide = false;
                    NPC.noGravity = false;

                    AITimer++;
                    if (AITimer >= TimerRand || NPC.Center.X + 20 > moveTo.X * 16 && NPC.Center.X - 20 < moveTo.X * 16)
                    {
                        AITimer = 0;
                        TimerRand = Main.rand.Next(80, 120);
                        AIState = ActionState.Idle;
                        NPC.netUpdate = true;
                    }

                    NPC.PlatformFallCheck(ref NPC.Redemption().fallDownPlatform, 40, (moveTo.Y - 32) * 16);
                    NPCHelper.HorizontallyMove(NPC, moveTo * 16, 0.3f, 4f, 26, 26, NPC.Center.Y > moveTo.Y * 16, player);
                    break;
                case ActionState.AlertRun:
                    if (NPC.ThreatenedCheck(ref runCooldown, 380))
                    {
                        runCooldown = 0;
                        AIState = ActionState.Wander;
                        AITimer = 0;
                        TimerRand = 0;
                        break;
                    }
                    AniType = 0;
                    NPC.noTileCollide = false;
                    NPC.noGravity = false;

                    if (!NPC.Sight(attacker, 800, true, true))
                        runCooldown++;
                    else if (runCooldown > 0)
                        runCooldown--;

                    NPC.DamageAnyAttackers(0, 8);

                    NPC.PlatformFallCheck(ref NPC.Redemption().fallDownPlatform, 40, attacker.Center.Y);
                    if (roarCooldown > 0)
                        NPCHelper.HorizontallyMove(NPC, attacker.Center, 0.6f, 8f, 26, 26, NPC.Center.Y > attacker.Center.Y, attacker);
                    else
                        NPCHelper.HorizontallyMove(NPC, attacker.Center, 0.3f, 4f, 26, 26, NPC.Center.Y > attacker.Center.Y, attacker);

                    if (++AITimer > 120 && NPC.velocity.Y == 0)
                    {
                        NPC.velocity.X *= 0;
                        AIState = ActionState.Attack;
                        AITimer = 0;
                        TimerRand = Main.rand.Next(4);
                        NPC.netUpdate = true;
                    }
                    break;
                case ActionState.AlertStand:
                    if (NPC.ThreatenedCheck(ref runCooldown, 380))
                    {
                        runCooldown = 0;
                        AIState = ActionState.Wander;
                        AITimer = 0;
                        TimerRand = 0;
                        break;
                    }
                    AniType = 0;
                    NPC.noTileCollide = false;
                    NPC.noGravity = false;

                    if (NPC.velocity.Y == 0)
                        NPC.velocity.X = 0;
                    NPC.LookAtEntity(attacker);

                    if (!NPC.Sight(attacker, 800, true, true))
                        runCooldown++;
                    else if (runCooldown > 0)
                        runCooldown--;

                    if (++AITimer > 80 && NPC.velocity.Y == 0)
                    {
                        NPC.velocity.X *= 0;
                        AIState = ActionState.Attack;
                        AITimer = 0;
                        TimerRand = Main.rand.Next(4);
                        NPC.netUpdate = true;
                    }
                    break;
                case ActionState.Attack:
                    if (NPC.ThreatenedCheck(ref runCooldown, 380))
                    {
                        NPC.noTileCollide = false;
                        NPC.noGravity = false;
                        AniType = 0;
                        frameCounters = 0;
                        runFrame = 0;
                        roarFrame = 0;
                        runCooldown = 0;
                        AIState = ActionState.Wander;
                        AITimer = 0;
                        TimerRand = 0;
                        break;
                    }
                    NPC.LookAtEntity(attacker);
                    switch ((int)TimerRand)
                    {
                        // Rawr xd
                        #region Roar
                        case 0:
                            if (NPC.velocity.Y == 0)
                                NPC.velocity.X = 0;

                            AniType = 1;
                            if (AITimer++ == 25 && !Main.dedServ)
                                SoundEngine.PlaySound(CustomSounds.Roar1, NPC.position);

                            if (AITimer == 30)
                            {
                                Main.LocalPlayer.RedemptionScreen().ScreenShakeOrigin = NPC.Center;
                                Main.LocalPlayer.RedemptionScreen().ScreenShakeIntensity += 10;
                            }
                            if (AITimer > 25 && AITimer % 10 == 0)
                                NPC.Shoot(new Vector2(NPC.Center.X + (64 * NPC.spriteDirection), NPC.Center.Y - 8), ProjectileType<MossyGoliath_SS_Screech>(), 0, new Vector2(3 * NPC.spriteDirection, 0), ai2: NPC.whoAmI);
                            if (AITimer >= 80)
                            {
                                AniType = 0;
                                frameCounters = 0;
                                runFrame = 0;
                                if (roarCooldown > 0)
                                    roarCooldown--;
                                if (Main.rand.NextBool(3) && roarCooldown == 0)
                                    AIState = ActionState.AlertStand;
                                else
                                    AIState = ActionState.AlertRun;
                                AITimer = 0;
                                roarCooldown = 2;
                                roarFrame = 0;
                                NPC.netUpdate = true;
                            }
                            break;
                        #endregion

                        // Stomp
                        #region Stomp
                        case 1:
                            if (NPC.velocity.Y == 0)
                                NPC.velocity.X = 0;

                            if (AITimer++ == 0)
                                AniType = 2;
                            if (AITimer == 20)
                            {
                                SoundEngine.PlaySound(SoundID.Item14, NPC.position);
                                for (int i = 0; i < 20; i++)
                                {
                                    int dustIndex2 = Dust.NewDust(new Vector2(NPC.Center.X - 300, NPC.Center.Y), NPC.width + 600, NPC.height / 2, DustID.Smoke, 0f, 0f, 100, default, 2f);
                                    Main.dust[dustIndex2].velocity.Y *= 3.6f;
                                    Main.dust[dustIndex2].velocity.X *= 0f;
                                }
                                foreach (NPC target in Main.ActiveNPCs)
                                {
                                    if (target.friendly || target.lifeMax <= 5 || target.velocity.Y != 0 || NPC.DistanceSQ(target.Center) >= 600 * 600)
                                        continue;

                                    if (target.immune[NPC.whoAmI] <= 0)
                                    {
                                        for (int i = 0; i < 20; i++)
                                        {
                                            int dustIndex2 = Dust.NewDust(new Vector2(target.position.X, target.Bottom.Y), target.width, 2, DustID.Smoke, Scale: 2f);
                                            Main.dust[dustIndex2].velocity.Y -= 3.6f;
                                            Main.dust[dustIndex2].velocity.X *= 0f;
                                        }
                                        target.immune[NPC.whoAmI] = 10;
                                        int hitDirection = target.RightOfDir(NPC);
                                        BaseAI.DamageNPC(target, NPC.damage, 6, hitDirection, NPC);
                                        if (target.knockBackResist > 0)
                                            target.AddBuff(BuffType<StunnedDebuff>(), 60);
                                    }
                                }
                            }
                            if (AITimer >= 60)
                            {
                                AniType = 0;
                                frameCounters = 0;
                                runFrame = 0;
                                if (roarCooldown > 0)
                                    roarCooldown--;
                                if (Main.rand.NextBool(3) && roarCooldown == 0)
                                    AIState = ActionState.AlertStand;
                                else
                                    AIState = ActionState.AlertRun;
                                AITimer = 0;
                                roarFrame = 0;
                                NPC.netUpdate = true;
                            }
                            break;
                        #endregion

                        // Falling Stomp
                        #region Falling Stomp
                        case 2:
                            if (NPC.life < (int)(NPC.lifeMax * 0.7f))
                            {
                                Vector2 FallPos = new(attacker.Center.X, attacker.Center.Y - 250);
                                if (++AITimer < 180)
                                {
                                    if (NPC.DistanceSQ(FallPos) < 200 * 200 || AITimer == 179)
                                    {
                                        NPC.noTileCollide = false;
                                        NPC.noGravity = false;

                                        NPC.velocity.X *= .3f;
                                        AITimer = 180;
                                        NPC.netUpdate = true;
                                    }
                                    else
                                    {
                                        NPC.noTileCollide = true;
                                        NPC.noGravity = true;
                                        NPC.MoveToVector2(FallPos, 12);
                                    }
                                }
                                if (AITimer == 180)
                                    NPC.velocity.Y += 30;
                                if (AITimer > 180)
                                {
                                    if (NPC.velocity.Y == 0)
                                    {
                                        SoundEngine.PlaySound(SoundID.Item14, NPC.position);
                                        for (int i = 0; i < 20; i++)
                                        {
                                            int dustIndex2 = Dust.NewDust(new Vector2(NPC.Center.X - 300, NPC.Center.Y), NPC.width + 600, NPC.height / 2, DustID.Smoke, 0f, 0f, 100, default, 2f);
                                            Main.dust[dustIndex2].velocity.Y *= 3.6f;
                                            Main.dust[dustIndex2].velocity.X *= 0f;
                                        }
                                        foreach (NPC target in Main.ActiveNPCs)
                                        {
                                            if (target.friendly || target.lifeMax <= 5 || target.velocity.Y != 0 || NPC.DistanceSQ(target.Center) >= 600 * 600)
                                                continue;

                                            if (target.immune[NPC.whoAmI] <= 0)
                                            {
                                                for (int i = 0; i < 20; i++)
                                                {
                                                    int dustIndex2 = Dust.NewDust(new Vector2(target.position.X, target.Bottom.Y), target.width, 2, DustID.Smoke, Scale: 2f);
                                                    Main.dust[dustIndex2].velocity.Y -= 3.6f;
                                                    Main.dust[dustIndex2].velocity.X *= 0f;
                                                }

                                                target.immune[NPC.whoAmI] = 10;
                                                int hitDirection = target.RightOfDir(NPC);
                                                BaseAI.DamageNPC(target, NPC.damage, 6, hitDirection, NPC);
                                                if (target.knockBackResist > 0)
                                                    target.AddBuff(BuffType<StunnedDebuff>(), 60);
                                            }
                                        }
                                        AniType = 0;
                                        frameCounters = 0;
                                        runFrame = 0;
                                        if (roarCooldown > 0)
                                            roarCooldown--;
                                        if (Main.rand.NextBool(3) && roarCooldown == 0)
                                            AIState = ActionState.AlertStand;
                                        else
                                            AIState = ActionState.AlertRun;
                                        AITimer = 0;
                                        roarFrame = 0;
                                        NPC.netUpdate = true;
                                    }
                                }
                            }
                            else
                            {
                                TimerRand = Main.rand.Next(4);
                                AITimer = 0;
                                AniType = 0;
                                NPC.netUpdate = true;
                            }
                            break;
                        #endregion

                        // Toxic Breath
                        #region Toxic Breath
                        case 3:
                            AniType = 1;
                            AITimer++;
                            if (AITimer == 25 && !Main.dedServ)
                                SoundEngine.PlaySound(CustomSounds.Roar1 with { Pitch = -.5f }, NPC.position);

                            if (AITimer > 25 && AITimer % 4 == 0)
                                NPC.Shoot(new Vector2(NPC.Center.X + (64 * NPC.spriteDirection), NPC.Center.Y - 8), ProjectileType<MossyGoliath_SS_ToxicBreath>(), NPC.damage / 2, new Vector2(7 * NPC.spriteDirection, Main.rand.NextFloat(-1f, 1f)), ai2: NPC.whoAmI);

                            if (AITimer >= 80)
                            {
                                AniType = 0;
                                frameCounters = 0;
                                runFrame = 0;
                                if (roarCooldown > 0)
                                    roarCooldown--;
                                if (Main.rand.NextBool(3) && roarCooldown == 0)
                                    AIState = ActionState.AlertStand;
                                else
                                    AIState = ActionState.AlertRun;
                                AITimer = 0;
                                roarFrame = 0;
                                NPC.netUpdate = true;
                            }
                            break;
                            #endregion
                    }
                    break;
                case ActionState.SoulMove:
                    SoulMoveState(NPC, ref AITimer, player, ref TimerRand, ref runCooldown, ref moveTo, 4, 4, 34);
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
        }
        public override bool? CanFallThroughPlatforms() => NPC.Redemption().fallDownPlatform;
        public override void FindFrame(int frameHeight)
        {
            if (NPC.velocity.Y == 0)
            {
                if (AniType == 0)
                {
                    if (NPC.velocity.X == 0)
                    {
                        runFrame = 0;
                        frameCounters = 0;
                        if (++NPC.frameCounter >= 5)
                        {
                            NPC.frameCounter = 0;
                            NPC.frame.Y += 88;
                            if (NPC.frame.Y > 440)
                            {
                                NPC.frameCounter = 0;
                                NPC.frame.Y = 0;
                            }
                        }
                    }
                    else
                    {
                        frameCounters += NPC.velocity.X * 0.5f;
                        int speed = 6;
                        if (roarCooldown > 0)
                            speed = 5;
                        if (frameCounters >= speed || frameCounters <= -speed)
                        {
                            frameCounters = 0;
                            if (++runFrame >= 7)
                            {
                                runFrame = 0;
                                frameCounters = 0;
                            }
                        }
                    }
                }
                else if (AniType == 1)
                {
                    if (++frameCounters > 5)
                    {
                        frameCounters = 0;
                        if (++roarFrame >= 7)
                            roarFrame = 5;
                    }
                }
                else if (AniType == 2)
                {
                    if (++frameCounters > 5)
                    {
                        frameCounters = 0;
                        if (++roarFrame >= 5)
                        {
                            roarFrame = 4;
                            AniType = 0;
                        }
                    }
                }
            }
            else
                roarFrame = 2;
        }
        public void SightCheck()
        {
            Player player = Main.player[(int)NPC.ai[3]];
            RedeNPC globalNPC = NPC.Redemption();

            int gotNPC = GetNearestNPC(NPC);
            if (player.MinionAttackTargetNPC != -1)
                gotNPC = player.MinionAttackTargetNPC;
            if (gotNPC != -1 && (NPC.Sight(Main.npc[gotNPC], 800, true, true) || gotNPC == player.MinionAttackTargetNPC))
            {
                globalNPC.attacker = Main.npc[gotNPC];
                frameCounters = 0;
                runFrame = 0;
                if (roarCooldown > 0)
                    roarCooldown--;
                if (Main.rand.NextBool(3) && roarCooldown == 0)
                    AIState = ActionState.AlertStand;
                else
                    AIState = ActionState.AlertRun;

                moveTo = NPC.FindGround(20);
                AITimer = 0;
                NPC.netUpdate = true;
            }
        }
        public override bool CanHitNPC(NPC target) => !NPC.friendly && AIState == ActionState.AlertRun;
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[NPC.type].Value;
            Texture2D runAni = Request<Texture2D>(Texture + "_Run").Value;
            Texture2D roarAni = Request<Texture2D>(Texture + "_Roar").Value;
            var effects = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            bool noSpiritEffect = NoSpiritEffect(NPC);
            Color color = noSpiritEffect ? drawColor : Color.White;
            if (!noSpiritEffect)
            {
                int shader = GameShaders.Armor.GetShaderIdFromItemId(ItemID.WispDye);
                spriteBatch.End();
                spriteBatch.BeginAdditive(true);
                GameShaders.Armor.ApplySecondary(shader, Main.LocalPlayer, null);
            }

            if (AniType == 0 && NPC.velocity.Y == 0)
            {
                if (NPC.velocity.X == 0)
                    spriteBatch.Draw(texture, NPC.Center - new Vector2(0, 11) - screenPos, NPC.frame, NPC.ColorTintedAndOpacity(color), NPC.rotation, NPC.frame.Size() / 2, NPC.scale, effects, 0f);
                else
                {
                    Vector2 drawCenter = new(NPC.Center.X, NPC.Center.Y - 14);
                    int height = runAni.Height / 7;
                    int y = height * runFrame;
                    Vector2 origin = new(runAni.Width / 2f, height / 2f);
                    if (roarCooldown > 0 && AIState is ActionState.AlertRun or ActionState.Wander)
                    {
                        for (int i = 0; i < NPCID.Sets.TrailCacheLength[NPC.type]; i++)
                        {
                            Vector2 oldPos = NPC.oldPos[i];
                            spriteBatch.Draw(runAni, oldPos - new Vector2(17, 31) + origin - screenPos, new Rectangle?(new Rectangle(0, y, runAni.Width, height)), NPC.GetAlpha(color) * ((NPC.oldPos.Length - i) / (float)NPC.oldPos.Length) * .5f, oldrot[i], origin, NPC.scale, effects, 0f);
                        }
                    }
                    spriteBatch.Draw(runAni, drawCenter - screenPos, new Rectangle?(new Rectangle(0, y, runAni.Width, height)), NPC.ColorTintedAndOpacity(color), NPC.rotation, origin, NPC.scale, effects, 0f);
                }
            }
            else
            {
                Vector2 drawCenter = new(NPC.Center.X, NPC.Center.Y - 16);
                int height = roarAni.Height / 7;
                int y = height * roarFrame;
                spriteBatch.Draw(roarAni, drawCenter - screenPos, new Rectangle?(new Rectangle(0, y, roarAni.Width, height)), NPC.ColorTintedAndOpacity(color), NPC.rotation, new Vector2(roarAni.Width / 2f, height / 2f), NPC.scale, effects, 0f);
            }
            if (!noSpiritEffect)
            {
                spriteBatch.End();
                spriteBatch.BeginDefault();
            }
            return false;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 50; i++)
                {
                    int dust = Dust.NewDust(NPC.position + NPC.velocity, NPC.width, NPC.height, DustID.DungeonSpirit,
                        NPC.velocity.X * 0.5f, NPC.velocity.Y * 0.5f, Scale: 4);
                    Main.dust[dust].velocity *= 5f;
                    Main.dust[dust].noGravity = true;
                }
            }
            Dust.NewDust(NPC.position + NPC.velocity, NPC.width, NPC.height, DustID.DungeonSpirit, 0, 0, Scale: 2);

            if (AIState is ActionState.Idle or ActionState.Wander)
            {
                AITimer = 0;
                AIState = ActionState.AlertRun;
            }
        }
        public override void OnKill()
        {
            RedeHelper.SpawnNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, NPCType<LostSoulNPC>(), Main.rand.NextFloat(3f, 4f));
        }
    }
}