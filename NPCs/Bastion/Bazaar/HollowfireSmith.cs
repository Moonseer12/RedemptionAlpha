using BetterDialogue.UI;
using Microsoft.Xna.Framework.Graphics;
using Redemption.Base;
using Redemption.Biomes;
using Redemption.Globals;
using Redemption.Globals.Players;
using Redemption.Textures;
using Redemption.Tiles.Furniture.Bastion;
using Redemption.UI.ChatUI;
using Redemption.UI.Dialect;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Utilities;

namespace Redemption.NPCs.Bastion.Bazaar
{
    public class HollowfireSmith : ModRedeNPC
    {
        public ref float AITimer => ref NPC.localAI[0];

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 12;

            BetterDialogue.BetterDialogue.SupportedNPCs.Add(Type);

            NPCID.Sets.ActsLikeTownNPC[Type] = true;
            NPCID.Sets.NoTownNPCHappiness[Type] = true;
            NPCID.Sets.NPCBestiaryDrawModifiers value = new();
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
        }

        public override void SetDefaults()
        {
            NPC.friendly = true;
            NPC.dontTakeDamage = true;
            NPC.noGravity = true;
            NPC.width = 140;
            NPC.height = 84;
            NPC.lifeMax = 250;
            NPC.aiStyle = -1;
            NPC.knockBackResist = 0f;
            NPC.npcSlots = 0;
            NPC.behindTiles = true;
            SpawnModBiomes = new int[1] { GetInstance<BlazingBastionBiome>().Type };

            DialogueBoxStyle = DEMON;
        }
        public override bool HasTalkButton() => true;
        public override bool HasLeftHangingButton(Player player) => RedeGlobalButton.talkActive;
        public override HangingButtonParams LeftHangingButton(Player player) => new(1);

        private static Texture2D Bubble => !Main.dedServ ? CommonTextures.TextBubble_Demon.Value : null;
        public static readonly SoundStyle voice = CustomSounds.GhostlyVoice.WithPitchOffset(-1f);

        readonly DialogueChain chain = new();

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.UIInfoProvider = new TownNPCUICollectionInfoProvider(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[Type]);
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheUnderworld,

                new FlavorTextBestiaryInfoElement(Language.GetTextValue("Mods.Redemption.FlavorTextBestiary.HollowfireSmith"))
            });
        }
        public override bool UsesPartyHat() => false;
        public override bool CanTownNPCSpawn(int numTownNPCs) => false;
        public override bool CanChat() => NPC.ai[3] == 0;

        public Point16 Parent
        {
            get => new((int)NPC.ai[1], (int)NPC.ai[2]);
            set
            {
                NPC.ai[1] = value.X;
                NPC.ai[2] = value.Y;
            }
        }

        public override void AI()
        {
            Lighting.AddLight(NPC.Center, 1.5f * NPC.Opacity, .7f * NPC.Opacity, .6f * NPC.Opacity);

            if (NPC.target < 0 || NPC.target >= 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
                NPC.TargetClosest();

            Player player = Main.player[NPC.target];

            if (NPC.ai[0] > 0)
                NPC.ai[0]--;

            if (player.InModBiome<BlazingBastionBiome>())
                NPC.DiscourageDespawn(60);

            int nearestPlayer = NPC.GetNearestAlivePlayer();
            if (NPC.ai[3] == 0 && nearestPlayer >= 0 && Main.player[nearestPlayer].active && NPC.DistanceSQ(Main.player[nearestPlayer].Center) < 500 * 500)
            {
                if (RedeQuest.hollowfireSmithVar == 0)
                {
                    NPC.ai[3] = 1;
                    NPC.netUpdate = true;
                }
            }
            if (NPC.ai[3] == 1)
            {
                if (AITimer++ == 5 && chain.Dialogue.Count == 0)
                {
                    string s1 = Mod.GetLocalization("Cutscene.HollowfireSmith.0").Value;
                    string s2 = Mod.GetLocalization("Cutscene.HollowfireSmith.1").Value;
                    string s3 = Mod.GetLocalization("Cutscene.HollowfireSmith.2").Value;
                    string s4 = Mod.GetLocalization("Cutscene.HollowfireSmith.3").Value;

                    chain.Add(new(NPC, s1, Color.Orange, Color.DarkRed, voice, .03f, 3f, 0, false, bubble: Bubble, blipDelay: 3))
                     .Add(new(NPC, s2, Color.Orange, Color.DarkRed, voice, .03f, 2f, 0, false, bubble: Bubble, blipDelay: 3))
                     .Add(new(NPC, s3, Color.Orange, Color.DarkRed, voice, .03f, 2f, 0, false, bubble: Bubble, blipDelay: 3))
                     .Add(new(NPC, s4, Color.Orange, Color.DarkRed, voice, .03f, 2f, 0.5f, true, bubble: Bubble, endID: 1, blipDelay: 3));

                    chain.OnEndTrigger += Chain_OnEndTrigger;
                    ChatUI.Visible = true;
                    ChatUI.Add(chain);
                }
                if (AITimer >= 10000)
                {
                    if (RedeQuest.hollowfireSmithVar < 3 && Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        if (RedeQuest.hollowfireSmithVar != 0)
                            RedeQuest.hollowfireSmithVar = 3;
                        else
                            RedeQuest.hollowfireSmithVar++;
                        RedeQuest.SyncData();
                    }

                    NPC.ai[3] = 0;
                    AITimer = 0;
                    NPC.netUpdate = true;
                }
            }

            if (!Framing.GetTileSafely(Parent).HasTile || Framing.GetTileSafely(Parent).TileType != TileType<DemonForgeTentTile>())
            {
                NPC.active = false;
            }
        }
        private void Chain_OnEndTrigger(Dialogue dialogue, int ID)
        {
            AITimer = 10000;
            NPC.netUpdate = true;
        }

        public override void ModifyHoverBoundingBox(ref Rectangle boundingBox)
        {
            boundingBox = NPC.Hitbox;
        }

        int faceFrame;
        int faceFrameX;
        int overlayFrame;
        int overlayFrameCounter;
        public override void FindFrame(int frameHeight)
        {
            if (++overlayFrameCounter >= 5)
            {
                overlayFrameCounter = 0;
                overlayFrame++;
                if (overlayFrame > 5)
                    overlayFrame = 0;
            }

            if (NPC.ai[0] > 0)
            {
                if (NPC.frame.Y > 3)
                    NPC.frame.Y = 3;

                if (++NPC.frameCounter >= 5)
                {
                    NPC.frameCounter = 0;
                    NPC.frame.Y++;
                    if (NPC.frame.Y == 2)
                        SoundEngine.PlaySound(SoundID.Item37.WithVolumeScale(.5f).WithPitchOffset(-0.4f), NPC.position);

                    if (NPC.frame.Y > 3)
                        NPC.frame.Y = 0;
                }
            }
            else
            {
                if (++NPC.frameCounter >= 10)
                {
                    if (Main.LocalPlayer.talkNPC == NPC.whoAmI)
                    {
                        if (faceFrameX < 2)
                            faceFrameX++;
                    }
                    else
                    {
                        if (faceFrameX > 0)
                            faceFrameX--;
                    }

                    NPC.frameCounter = 0;
                    NPC.frame.Y++;
                    if (NPC.frame.Y > 11)
                        NPC.frame.Y = 0;

                    faceFrame++;
                    if (faceFrame > 5)
                        faceFrame = 0;
                }
                if (faceFrameX == 1)
                    faceFrame = 0;
            }
        }
        Asset<Texture2D> extraTex;
        Asset<Texture2D> overlayTex;
        Asset<Texture2D> forgingTex;
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Asset<Texture2D> texture = TextureAssets.Npc[Type];

            extraTex ??= Request<Texture2D>(Texture + "_Extra");
            overlayTex ??= Request<Texture2D>(Texture + "_Overlay");
            forgingTex ??= Request<Texture2D>(Texture + "_Forge");

            Vector2 offset = Vector2.Zero;
            if (NPC.IsABestiaryIconDummy)
                offset = new Vector2(11, 7);

            spriteBatch.Draw(extraTex.Value, NPC.Center + new Vector2(-109, -15) + offset - screenPos, null, NPC.GetAlpha(drawColor), 0, extraTex.Size() / 2, 1, 0, 0);

            if (NPC.ai[0] > 0)
            {
                Rectangle rect = forgingTex.Frame(1, 4, 0, NPC.frame.Y);
                Vector2 origin = rect.Size() / 2;
                spriteBatch.Draw(forgingTex.Value, NPC.Center - new Vector2(0, 6) - screenPos, rect, NPC.ColorTintedAndOpacity(drawColor), 0, origin, 1, 0, 0);
            }
            else
            {
                Rectangle rect = new(0, NPC.frame.Y * NPC.frame.Height, 142, NPC.frame.Height);
                Vector2 origin = rect.Size() / 2;
                spriteBatch.Draw(texture.Value, NPC.Center + offset - screenPos, rect, NPC.ColorTintedAndOpacity(drawColor), 0, origin, 1, 0, 0);
                // Face
                rect = new((faceFrameX * 42) + 142, faceFrame * 40, 42, 40);
                origin = rect.Size() / 2;
                spriteBatch.Draw(texture.Value, NPC.Center + new Vector2(-8, -17) + offset - screenPos, rect, NPC.ColorTintedAndOpacity(drawColor), 0, origin, 1, 0, 0);
            }

            Rectangle overlayRect = overlayTex.Frame(1, 6, 0, overlayFrame);
            Vector2 overlayOrigin = overlayRect.Size() / 2;
            spriteBatch.Draw(overlayTex.Value, NPC.Center + new Vector2(41, 8) + offset - screenPos, overlayRect, NPC.GetAlpha(Color.White), 0, overlayOrigin, 1, 0, 0);
            return false;
        }

        public static int TalkID;
        public override string GetChat()
        {
            Player player = Main.LocalPlayer;
            WeightedRandom<string> chat = new(Main.rand);
            string lad = player.Male ? Mod.GetLocalization("Dialogue.General.Lad").Value : Mod.GetLocalization("Dialogue.General.Lassy").Value;

            if (RedeQuest.hollowfireSmithVar >= 2)
            {
                chat.Add(Mod.GetLocalization("Dialogue.HollowfireSmith.Chat7").Value);
                chat.Add(Mod.GetLocalization("Dialogue.HollowfireSmith.Chat8").Value);
                chat.Add(Mod.GetLocalization("Dialogue.HollowfireSmith.Chat9").Value);
                return chat;
            }
            if (RedeQuest.hollowfireSmithVar < 2)
            {
                RedeQuest.hollowfireSmithVar = 2;
                RedeQuest.SyncData();
            }
            string s2 = Mod.GetLocalization("Dialogue.HollowfireSmith.Chat0").WithFormatArgs(lad).Value;
            return s2;
        }

        public sealed class Button0_HollowfireSmith : HangingButtonBase
        {
            public override int NPCType => NPCType<HollowfireSmith>();
            public override int YOffset => 0;
            public override bool ActiveRequirements => RedeGlobalButton.talkActive;

            public override string NewText(NPC npc, Player player) => Mod.GetLocalization("DialogueBox.HollowfireSmith.A" + TalkID).Value;
            public override Color? NewColor(NPC npc, Player player) => DialoguePlayer.GetTalkStateLocal(DialoguePlayer.TalkType.HollowfireSmith0) ? Color.Gray : null;

            public override void NewOnClick(NPC npc, Player player)
            {
                SoundEngine.PlaySound(SoundID.Chat);
                int maxLines = 3;
                Main.npcChatText = Mod.GetLocalization("Dialogue.HollowfireSmith.A" + TalkID).Value;
                TalkID++;
                if (TalkID >= maxLines)
                {
                    DialoguePlayer.SetTalkStateLocal(DialoguePlayer.TalkType.HollowfireSmith0);
                    TalkID = 0;
                }
            }
        }
    }
}