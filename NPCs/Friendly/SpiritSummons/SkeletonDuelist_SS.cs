using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Redemption.Base;
using Redemption.Buffs.Debuffs;
using Redemption.Globals;
using Redemption.Globals.NPC;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using Redemption.BaseExtension;
using Terraria.DataStructures;
using ParticleLibrary;
using Redemption.Dusts;
using Redemption.Particles;
using Terraria.Graphics.Shaders;
using Redemption.NPCs.PreHM;

namespace Redemption.NPCs.Friendly.SpiritSummons
{
    public class SkeletonDuelist_SS : SkeletonBase
    {
        public override string Texture => "Redemption/NPCs/PreHM/SkeletonDuelist";
        public enum ActionState
        {
            Idle,
            Wander,
            Alert,
            IdleAlert,
            Attack,
            SoulMove = 10
        }
        public ActionState AIState
        {
            get => (ActionState)NPC.ai[0];
            set => NPC.ai[0] = (int)value;
        }
        public override void SetSafeStaticDefaults()
        {
            // DisplayName.SetDefault("Skeleton Noble");
            Main.npcFrameCount[NPC.type] = 13;
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.TrailCacheLength[NPC.type] = 4;
            NPCID.Sets.TrailingMode[NPC.type] = 1;
            NPCID.Sets.NPCBestiaryDrawModifiers value = new(0) { Hide = true };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
        }
        public override void SetDefaults()
        {
            NPC.width = 30;
            NPC.height = 48;
            NPC.damage = 22;
            NPC.friendly = true;
            NPC.defense = 8;
            NPC.lifeMax = 124;
            NPC.HitSound = SoundID.DD2_SkeletonHurt;
            NPC.DeathSound = SoundID.DD2_SkeletonDeath;
            NPC.knockBackResist = 0.5f;
            NPC.aiStyle = -1;
            NPC.Redemption().spiritSummon = true;
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

            if (AIState is ActionState.Idle or ActionState.Wander or ActionState.IdleAlert)
            {
                if (!Main.dedServ)
                    SoundEngine.PlaySound(new("Redemption/Sounds/Custom/" + SoundString + "Notice"), NPC.position);
                AITimer = 0;
                AIState = ActionState.Alert;
            }
        }

        public override bool CheckActive() => false;
        private int runCooldown;
        public override void ModifyTypeName(ref string typeName)
        {
            if (NPC.ai[3] != -1)
                typeName = Main.player[(int)NPC.ai[3]].name + "'s " + Lang.GetNPCNameValue(Type);
        }
        private int dodgeCooldown;

        private int AniFrameY;
        public override void OnSpawn(IEntitySource source)
        {
            ChoosePersonality();
            SetStats();

            TimerRand = Main.rand.Next(80, 280);
            NPC.netUpdate = true;
        }
        public override void AI()
        {
            CustomFrames(58);

            Player player = Main.player[(int)NPC.ai[3]];
            RedeNPC globalNPC = NPC.Redemption();
            if (!player.active || player.dead || !SSBase.CheckActive(player))
                NPC.SimpleStrikeNPC(999, 1);
            NPC.TargetClosest();
            NPC.LookByVelocity();
            Rectangle SlashHitbox1 = new((int)(NPC.spriteDirection == -1 ? NPC.Center.X - 66 : NPC.Center.X + 4), (int)(NPC.Center.Y - 60), 62, 86);
            Rectangle SlashHitbox2 = new((int)(NPC.spriteDirection == -1 ? NPC.Center.X - 94 : NPC.Center.X), (int)(NPC.Center.Y - 40), 94, 84);
            dodgeCooldown--;
            dodgeCooldown = (int)MathHelper.Max(0, dodgeCooldown);

            if (Main.rand.NextBool(4500) && !Main.dedServ)
                SoundEngine.PlaySound(new("Redemption/Sounds/Custom/" + SoundString + "Ambient"), NPC.position);

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

                case ActionState.IdleAlert:
                    if (NPC.velocity.Y == 0)
                        NPC.velocity.X = 0;
                    AITimer++;
                    if (AITimer >= TimerRand)
                    {
                        AITimer = 0;
                        TimerRand = Main.rand.Next(80, 280);
                        AIState = ActionState.Idle;
                        NPC.netUpdate = true;
                    }

                    SightCheck();
                    break;

                case ActionState.Wander:
                    SightCheck();

                    AITimer++;
                    if (AITimer >= TimerRand || NPC.Center.X + 20 > moveTo.X * 16 && NPC.Center.X - 20 < moveTo.X * 16)
                    {
                        AITimer = 0;
                        TimerRand = Main.rand.Next(80, 280);
                        AIState = ActionState.Idle;
                        NPC.netUpdate = true;
                    }
                    BaseAI.AttemptOpenDoor(NPC, ref doorVars[0], ref doorVars[1], ref doorVars[2], 80, interactDoorStyle: HasEyes ? 2 : 0);

                    NPC.PlatformFallCheck(ref NPC.Redemption().fallDownPlatform, 20, (moveTo.Y - 32) * 16);
                    NPCHelper.HorizontallyMove(NPC, moveTo * 16, 0.4f, 1 * SpeedMultiplier, 12, 8, NPC.Center.Y > moveTo.Y * 16);
                    break;

                case ActionState.Alert:
                    if (NPC.ThreatenedCheck(ref runCooldown, 180, 2))
                    {
                        runCooldown = 0;
                        TimerRand = Main.rand.Next(120, 240);
                        AIState = ActionState.IdleAlert;
                        NPC.netUpdate = true;
                    }

                    if (!NPC.Sight(globalNPC.attacker, VisionRange, HasEyes, HasEyes, false))
                        runCooldown++;
                    else if (runCooldown > 0)
                        runCooldown--;

                    if (dodgeCooldown <= 0 && NPC.velocity.Y == 0)
                    {
                        for (int i = 0; i < Main.maxProjectiles; i++)
                        {
                            Projectile proj = Main.projectile[i];
                            if (!proj.active || !proj.hostile || proj.damage <= 0 || proj.velocity.Length() == 0)
                                continue;

                            if (!NPC.Sight(proj, 80 + (proj.velocity.Length() * 4), true, true))
                                continue;

                            for (int l = 0; l < 10; l++)
                            {
                                int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Smoke);
                                Main.dust[dust].velocity *= 0.2f;
                                Main.dust[dust].noGravity = true;
                            }
                            NPC.Dodge(proj, 6, 2, 10);
                            dodgeCooldown = 90;
                        }
                    }
                    BaseAI.AttemptOpenDoor(NPC, ref doorVars[0], ref doorVars[1], ref doorVars[2], 80, interactDoorStyle: HasEyes ? 2 : 0);

                    if (NPC.velocity.Y == 0 && NPC.DistanceSQ(globalNPC.attacker.Center) < 80 * 80)
                    {
                        NPC.LookAtEntity(globalNPC.attacker);
                        AITimer = 0;
                        NPC.frameCounter = 0;
                        NPC.velocity *= 0;
                        AIState = ActionState.Attack;
                    }

                    NPC.PlatformFallCheck(ref NPC.Redemption().fallDownPlatform, 20, globalNPC.attacker.Center.Y);
                    NPCHelper.HorizontallyMove(NPC, globalNPC.attacker.Center, 0.2f, 2.2f * SpeedMultiplier * (NPC.RedemptionNPCBuff().rallied ? 1.2f : 1),
                        12, 8, NPC.Center.Y > globalNPC.attacker.Center.Y, globalNPC.attacker);

                    break;

                case ActionState.Attack:
                    if (NPC.ThreatenedCheck(ref runCooldown, 180, 2))
                    {
                        runCooldown = 0;
                        AIState = ActionState.Wander;
                    }

                    NPC.LookAtEntity(globalNPC.attacker);

                    if (NPC.velocity.Y < 0)
                        NPC.velocity.Y = 0;
                    if (NPC.velocity.Y == 0)
                        NPC.velocity.X *= 0.9f;

                    if ((AniFrameY == 3 && globalNPC.attacker.Hitbox.Intersects(SlashHitbox1)) || (AniFrameY == 6 && globalNPC.attacker.Hitbox.Intersects(SlashHitbox2)))
                    {
                        if (globalNPC.attacker is NPC attackerNPC && attackerNPC.immune[NPC.whoAmI] <= 0)
                        {
                            attackerNPC.immune[NPC.whoAmI] = 10;
                            int hitDirection = attackerNPC.RightOfDir(NPC);
                            BaseAI.DamageNPC(attackerNPC, NPC.damage, 6, hitDirection, NPC);
                            if (Main.rand.NextBool(3))
                                attackerNPC.AddBuff(ModContent.BuffType<DirtyWoundDebuff>(), Main.rand.Next(400, 1200));
                        }
                    }
                    break;
                case ActionState.SoulMove:
                    NPC.alpha = 255;
                    NPC.noGravity = true;
                    NPC.noTileCollide = true;
                    AITimer = 0;

                    ParticleManager.NewParticle(NPC.Center + RedeHelper.Spread(10) + NPC.velocity, Vector2.Zero, new SpiritParticle(), Color.White, 0.6f * 1, 0, 1);
                    for (int i = 0; i < 2; i++)
                    {
                        int dust = Dust.NewDust(NPC.Center + NPC.velocity - Vector2.One, 1, 1, ModContent.DustType<GlowDust>(), 0, 0, 0, default, 1f);
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].velocity *= .1f;
                        Color dustColor = new(188, 244, 227) { A = 0 };
                        Main.dust[dust].color = dustColor;
                    }

                    if (NPC.Hitbox.Intersects(player.Hitbox) && Collision.CanHit(NPC.Center, 0, 0, player.Center, 0, 0) && !Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            int dust = Dust.NewDust(NPC.position + NPC.velocity, NPC.width, NPC.height, DustID.DungeonSpirit, 0, 0, Scale: 2);
                            Main.dust[dust].velocity *= 2f;
                            Main.dust[dust].noGravity = true;
                        }

                        NPC.alpha = 0;
                        NPC.noGravity = false;
                        NPC.noTileCollide = false;
                        NPC.velocity *= 0f;

                        moveTo = NPC.FindGround(20);
                        runCooldown = 0;
                        TimerRand = Main.rand.Next(120, 260);
                        AIState = ActionState.Idle;
                        NPC.netUpdate = true;
                    }
                    else
                        NPC.Move(player.Center - new Vector2(0, 8), 20, 20);
                    break;
            }
            if (Main.myPlayer == player.whoAmI && NPC.DistanceSQ(player.Center) > 2000 * 2000)
            {
                NPC.Center = player.Center;
                NPC.velocity *= 0.1f;
                NPC.netUpdate = true;
            }
            if (AIState is not ActionState.SoulMove)
            {
                NPC.alpha += Main.rand.Next(-10, 11);
                NPC.alpha = (int)MathHelper.Clamp(NPC.alpha, 0, 30);
            }

            if (!Main.rand.NextBool(40))
                return;
            ParticleManager.NewParticle(NPC.RandAreaInEntity(), RedeHelper.Spread(2), new SpiritParticle(), Color.White, 1);
        }
        private void CustomFrames(int frameHeight)
        {
            if (AIState is ActionState.Attack)
            {
                if (++NPC.frameCounter >= 5)
                {
                    NPC.frameCounter = 0;
                    AniFrameY++;
                    if (AniFrameY is 3 or 6)
                    {
                        SoundEngine.PlaySound(SoundID.Item19, NPC.position);
                        NPC.velocity.X = 2 * NPC.spriteDirection;
                    }
                    if (AniFrameY > 10)
                    {
                        AniFrameY = 0;
                        NPC.frame.Y = 0;
                        HeadOffset = 0;

                        RedeNPC globalNPC = NPC.Redemption();
                        if (NPC.velocity.Y == 0 && NPC.DistanceSQ(globalNPC.attacker.Center) < 100 * 100)
                            NPC.LookAtEntity(globalNPC.attacker);
                        else
                            AIState = ActionState.Alert;
                    }
                }
                if (AIState is ActionState.Attack)
                    HeadOffset = SetHeadOffsetY();
                else
                    HeadOffset = SetHeadOffset(ref frameHeight);
                HeadOffsetX = SetHeadOffsetX();
                return;
            }
        }
        public override bool? CanFallThroughPlatforms() => NPC.Redemption().fallDownPlatform;
        private int HeadOffsetX;
        public override void FindFrame(int frameHeight)
        {
            if (Main.netMode != NetmodeID.Server)
                NPC.frame.Width = TextureAssets.Npc[NPC.type].Width() / 6;
            NPC.frame.X = AIState is ActionState.Alert or ActionState.IdleAlert ? 0 : NPC.frame.Width;
            if (AIState is ActionState.Attack)
                return;
            AniFrameY = 0;

            if (NPC.collideY || NPC.velocity.Y == 0)
            {
                NPC.rotation = 0;
                if (NPC.velocity.X == 0)
                {
                    if (++NPC.frameCounter >= 10)
                    {
                        NPC.frameCounter = 0;
                        NPC.frame.Y += frameHeight;
                        if (NPC.frame.Y > 3 * frameHeight)
                            NPC.frame.Y = 0 * frameHeight;
                    }
                }
                else
                {
                    if (NPC.frame.Y < 5 * frameHeight)
                        NPC.frame.Y = 5 * frameHeight;

                    NPC.frameCounter += NPC.velocity.X * 0.5f;
                    if (NPC.frameCounter is >= 3 or <= -3)
                    {
                        NPC.frameCounter = 0;
                        NPC.frame.Y += frameHeight;
                        if (NPC.frame.Y > 12 * frameHeight)
                            NPC.frame.Y = 5 * frameHeight;
                    }
                }
            }
            else
            {
                NPC.rotation = NPC.velocity.X * 0.05f;
                NPC.frame.Y = 4 * frameHeight;
            }
            if (AIState is ActionState.Attack)
                HeadOffset = SetHeadOffsetY();
            else
                HeadOffset = SetHeadOffset(ref frameHeight);
            HeadOffsetX = SetHeadOffsetX();
        }
        public int SetHeadOffsetY()
        {
            return AniFrameY switch
            {
                1 => 2,
                3 => -2,
                4 => -2,
                10 => 2,
                _ => 0,
            };
        }
        public int SetHeadOffsetX()
        {
            return AniFrameY switch
            {
                1 => 2,
                6 => -6,
                7 => -14,
                8 => -14,
                9 => -6,
                _ => 0,
            };
        }
        public int GetNearestNPC()
        {
            float nearestNPCDist = -1;
            int nearestNPC = -1;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC target = Main.npc[i];
                if (!target.active || target.whoAmI == NPC.whoAmI || target.dontTakeDamage || target.type == NPCID.OldMan || target.type == NPCID.TargetDummy)
                    continue;

                if (target.friendly || target.lifeMax <= 5 || NPCID.Sets.TakesDamageFromHostilesWithoutBeingFriendly[target.type])
                    continue;

                if (nearestNPCDist != -1 && !(target.Distance(NPC.Center) < nearestNPCDist))
                    continue;

                nearestNPCDist = target.Distance(NPC.Center);
                nearestNPC = target.whoAmI;
            }

            return nearestNPC;
        }
        public void SightCheck()
        {
            Player player = Main.player[(int)NPC.ai[3]];
            RedeNPC globalNPC = NPC.Redemption();
            int gotNPC = GetNearestNPC();
            if (player.MinionAttackTargetNPC != -1)
                gotNPC = player.MinionAttackTargetNPC;
            if (gotNPC != -1 && (NPC.Sight(Main.npc[gotNPC], VisionRange, HasEyes, HasEyes, false) || gotNPC == player.MinionAttackTargetNPC))
            {
                if (!Main.dedServ)
                    SoundEngine.PlaySound(new("Redemption/Sounds/Custom/" + SoundString + "Notice"), NPC.position);
                globalNPC.attacker = Main.npc[gotNPC];
                moveTo = NPC.FindGround(20);
                AITimer = 0;
                AIState = ActionState.Alert;
                NPC.netUpdate = true;
            }
        }
        public void ChoosePersonality()
        {
            WeightedRandom<int> head = new(Main.rand);
            head.Add(0);
            head.Add(1, 0.6);
            head.Add(2, 0.6);
            head.Add(3, 0.4);
            head.Add(4, 0.4);
            head.Add(5, 0.1);
            head.Add(6, 0.1);
            head.Add(7, 0.1);
            head.Add(8, 0.06);
            head.Add(9, 0.06);
            head.Add(10, 0.06);
            head.Add(11, 0.06);
            HeadType = head;

            if (Main.rand.NextBool(3) || Personality == PersonalityState.Soulful)
                HasEyes = true;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D Glow = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
            var effects = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            int shader = GameShaders.Armor.GetShaderIdFromItemId(ItemID.WispDye);
            spriteBatch.End();
            spriteBatch.BeginAdditive(true);
            GameShaders.Armor.ApplySecondary(shader, Main.LocalPlayer, null);

            int HeightH = head2.Value.Height / 12;
            int WidthH = head2.Value.Width / 2;
            int yH = HeightH * HeadType;
            int xH = WidthH * HeadX;
            Rectangle rectH = new(xH, yH, WidthH, HeightH);

            if (AIState is ActionState.Attack)
            {
                int Height = SlashAni.Value.Height / 11;
                int Width = SlashAni.Value.Width / 3;
                int y = Height * AniFrameY;
                Rectangle rect = new(0, y, Width, Height);
                Vector2 origin = new(Width / 2f, Height / 2f);
                spriteBatch.Draw(SlashAni.Value, NPC.Center - screenPos - new Vector2(0, 11), new Rectangle?(rect), NPC.GetAlpha(Color.White), NPC.rotation, origin, NPC.scale, effects, 0);

                spriteBatch.Draw(head2.Value, NPC.Center - screenPos, new Rectangle?(rectH), NPC.GetAlpha(Color.White), NPC.rotation, NPC.frame.Size() / 2 + new Vector2((NPC.spriteDirection == 1 ? -48 : -38) + (HeadOffsetX * NPC.spriteDirection), -1 + HeadOffset), NPC.scale, effects, 0);

                if (HasEyes)
                    spriteBatch.Draw(SlashGlow.Value, NPC.Center - screenPos - new Vector2(0, 11), new Rectangle?(rect), NPC.GetAlpha(Color.White), NPC.rotation, origin, NPC.scale, effects, 0);
            }
            else
            {
                if (!NPC.IsABestiaryIconDummy)
                {
                    float fade = dodgeCooldown / 120f;
                    for (int i = 0; i < NPCID.Sets.TrailCacheLength[NPC.type]; i++)
                    {
                        Vector2 oldPos = NPC.oldPos[i];
                        Main.spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, oldPos + NPC.Size / 2f - screenPos + new Vector2(0, NPC.gfxOffY), NPC.frame, NPC.GetAlpha(Color.White) * MathHelper.Lerp(0, 1, fade) * ((NPC.oldPos.Length - i) / (float)NPC.oldPos.Length), NPC.rotation, NPC.frame.Size() / 2, NPC.scale, effects, 0);
                    }
                }
                spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, NPC.Center - screenPos, NPC.frame, NPC.GetAlpha(Color.White), NPC.rotation, NPC.frame.Size() / 2, NPC.scale, effects, 0);

                spriteBatch.Draw(head2.Value, NPC.Center - screenPos, new Rectangle?(rectH), NPC.GetAlpha(Color.White), NPC.rotation, NPC.frame.Size() / 2 + new Vector2((NPC.spriteDirection == 1 ? -48 : -38) + (HeadOffsetX * NPC.spriteDirection), -2 + HeadOffset), NPC.scale, effects, 0);

                if (HasEyes)
                    spriteBatch.Draw(Glow, NPC.Center - screenPos, NPC.frame, NPC.GetAlpha(Color.White), NPC.rotation, NPC.frame.Size() / 2, NPC.scale, effects, 0);
            }
            spriteBatch.End();
            spriteBatch.BeginDefault();
            return false;
        }
        public override bool CanHitNPC(NPC target) => false;
        public override void OnKill()
        {
            RedeHelper.SpawnNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<LostSoulNPC>(), Main.rand.NextFloat(0.2f, 0.6f));
        }
    }
}
