﻿using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Redemption.Globals;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Redemption.BaseExtension;
using Redemption.Items.Weapons.PreHM.Summon;
using Redemption.Items.Usable.Summons;
using Redemption.Items.Materials.PreHM;
using Redemption.Items.Quest.KingSlayer;
using Terraria.Audio;
using Redemption.Items.Armor.Vanity;
using Redemption.Base;

namespace Redemption.NPCs.Friendly
{
    public class SpiritWalkerMan : ModNPC
    {
        public ref float AITimer => ref NPC.ai[1];
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Spirit Stranger");
            Main.npcFrameCount[NPC.type] = 4;
            NPCID.Sets.ActsLikeTownNPC[Type] = true;

            NPCID.Sets.DebuffImmunitySets.Add(Type, new NPCDebuffImmunityData
            {
                ImmuneToAllBuffsThatAreNotWhips = true
            });
            NPCID.Sets.NPCBestiaryDrawModifiers value = new(0)
            {
                Hide = true
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
        }
        public override void SetDefaults()
        {
            NPC.friendly = true;
            NPC.dontTakeDamage = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.width = 24;
            NPC.height = 44;
            NPC.lifeMax = 250;
            NPC.aiStyle = -1;
            NPC.knockBackResist = 0f;
            NPC.npcSlots = 0;
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot) => false;
        public override bool? CanHitNPC(NPC target) => false;

        public bool floatTimer;
        public override void AI()
        {
            Player player = Main.player[RedeHelper.GetNearestAlivePlayer(NPC)];
            if (NPC.target < 0 || NPC.target == 255 || player.dead || !player.active)
                NPC.TargetClosest();

            NPC.LookAtEntity(player);

            if (AITimer < 60)
                NPC.velocity *= 0.94f;

            if (AITimer++ == 0)
            {
                for (int i = 0; i < 25; i++)
                {
                    int dustIndex = Dust.NewDust(NPC.Center, 2, 2, DustID.DungeonSpirit, 0f, 0f, 100, default, 2);
                    Main.dust[dustIndex].velocity *= 2f;
                    Main.dust[dustIndex].noGravity = true;
                }
                DustHelper.DrawDustImage(NPC.Center, DustID.DungeonSpirit, 0.5f, "Redemption/Effects/DustImages/DeadRingerDust", 2, true, 0);
            }
            NPC.alpha += Main.rand.Next(-10, 11);
            NPC.alpha = (int)MathHelper.Clamp(NPC.alpha, 40, 60);
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            if (NPC.frameCounter >= 5)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;
                if (NPC.frame.Y >= 4 * frameHeight)
                    NPC.frame.Y = 0;
            }
            if (!floatTimer)
            {
                NPC.velocity.Y += 0.03f;
                if (NPC.velocity.Y > .5f)
                {
                    floatTimer = true;
                    NPC.netUpdate = true;
                }
            }
            else if (floatTimer)
            {
                NPC.velocity.Y -= 0.03f;
                if (NPC.velocity.Y < -.5f)
                {
                    floatTimer = false;
                    NPC.netUpdate = true;
                }
            }
        }
        public static int ChatNumber = 0;
        public override void SetChatButtons(ref string button, ref string button2)
        {
            switch (ChatNumber)
            {
                case 0:
                    button = "Spirit Walking?";
                    break;
                case 1:
                    button = "Dead Ringer?";
                    break;
                case 2:
                    button = "Lost Souls & Vagrants?";
                    break;
                case 3:
                    button = "Other uses for Dead Ringer?";
                    break;
                case 4:
                    button = "About you?";
                    break;
                case 5:
                    if (Main.LocalPlayer.HasItem(ModContent.ItemType<OldTophat>()))
                        button = "Request Old Tophat's Crux";
                    else
                        button = "Request Crux";
                    break;
            }
            button2 = "Cycle Dialogue";
        }

        public override void OnChatButtonClicked(bool firstButton, ref bool shop)
        {
            if (firstButton)
            {
                Main.npcChatText = ChitChat();
                if (ChatNumber == 5)
                {
                    if (!Main.LocalPlayer.RedemptionAbility().SpiritwalkerActive)
                    {
                        Main.npcChatText = "Sorry! Can't give ya anything while you're completely in the physical realm. Peak in and I'll give it to ya!";
                        ChatNumber = 4;
                        return;
                    }
                    int card = Main.LocalPlayer.FindItem(ModContent.ItemType<EmptyCruxCard>());
                    if (Main.LocalPlayer.HasItem(ModContent.ItemType<OldTophat>()))
                    {
                        int oldTophat = Main.LocalPlayer.FindItem(ModContent.ItemType<OldTophat>());
                        if (card >= 0 && oldTophat >= 0)
                        {
                            Main.LocalPlayer.inventory[oldTophat].stack--;
                            if (Main.LocalPlayer.inventory[oldTophat].stack <= 0)
                                Main.LocalPlayer.inventory[oldTophat] = new Item();

                            Main.LocalPlayer.QuickSpawnItem(NPC.GetSource_Loot(), ModContent.ItemType<CruxCardTied>());
                            Main.npcChatText = "I see, I see. Within this tophat lies an odd spirit. I don't believe I've ever met it before, but they appear ta be quite powerful.";
                            Main.npcChatCornerItem = ModContent.ItemType<CruxCardTied>();
                            SoundEngine.PlaySound(SoundID.Chat);
                        }
                        else
                        {
                            Main.npcChatText = "There's somethin' in that hat I can use to imbue into something, however, ya don't have anythin' for me to imbue.";
                            Main.npcChatCornerItem = ModContent.ItemType<EmptyCruxCard>();
                        }
                        ChatNumber = 4;
                        return;
                    }
                    if (card >= 0)
                    {
                        Main.LocalPlayer.inventory[card].stack--;
                        if (Main.LocalPlayer.inventory[card].stack <= 0)
                            Main.LocalPlayer.inventory[card] = new Item();

                        Main.LocalPlayer.QuickSpawnItem(NPC.GetSource_Loot(), ModContent.ItemType<CruxCardSkeleton>());
                        Main.npcChatText = "Bravo! I 'ave infused ya card with the spirits of two skeletons, use some souls to summon 'em! They'll assist ya in battle, or make nice company if ya feelin' lonely. They do 'ave a mind of their own, so if they scurry off somewhere, use the card to tug their souls back!";
                        Main.npcChatCornerItem = ModContent.ItemType<CruxCardSkeleton>();
                        SoundEngine.PlaySound(SoundID.Chat);
                    }
                    else
                    {
                        Main.npcChatText = "Ya don't have anythin' I can imbue. If you want help findin' somethin', I'll say to look for those gathic tombs scattered aroun' the caverns. Ya might find somethin' in there.";
                        Main.npcChatCornerItem = ModContent.ItemType<EmptyCruxCard>();
                    }
                    ChatNumber = 4;
                }
            }
            else
            {
                ChatNumber++;
                int max = 4;
                if (Main.LocalPlayer.HasItem(ModContent.ItemType<OldTophat>()))
                {
                    if (Main.LocalPlayer.RedemptionAbility().SpiritwalkerActive && !Main.LocalPlayer.HasItem(ModContent.ItemType<CruxCardTied>()))
                        max = 5;
                }
                else
                {
                    if (Main.LocalPlayer.RedemptionAbility().SpiritwalkerActive && !Main.LocalPlayer.HasItem(ModContent.ItemType<CruxCardSkeleton>()))
                        max = 5;
                }
                if (ChatNumber > max)
                    ChatNumber = 0;
            }
        }
        public static string ChitChat()
        {
            return ChatNumber switch
            {
                0 => "The ability given to ya gives you a peak into the Spirit Realm - A realm parallel to our own. Normally, neither realm can see each other, nor interact. You'll be able to kill foes connected to the realm while peakin', or find corpses to use that bell thing-a-ma-jig on. Such as mine! Another thing, we spirits can gift to you our \"crux\" if ya have somethin' to imbue it in - as long as you're submerged enough in our realm.",
                1 => "A nifty artifact of Gathuram, that is. Usin' it on corpses will force their spirit back to their location while tuggin' them outta the Spirit Realm a tad, which is why you can see me now even if you stopped Spirit Walkin', despite me being a full-fledged spirit - not one of those Vagrants or Lost Souls.",
                2 => "When folk die, their soul leaves their body to find anotha'. During this pilgrimage, they're still connected to the physical world, meanin' some with the arcane eye may see 'em, or even do... worse things with 'em. If a lost soul wanders too long, they can seep outta the spirit realm and become Vagrant Spirits - poor things forever lost.",
                3 => !Main.rand.NextBool(8) ? "I couldn't tell ya where to find corpses to use it on, but I can recommend findin' spirit currents. They're only visible in my realm, and can take ya closer to a soulful corpse if ya travel through one. You'll only find 'em in caverns, as that'll be where our energy is most contained. If ya do find what you're lookin' for, be wary. Tombs of the mighty are seldom left unguarded." : "I once met anotha' spirit in a mind-bogglin' location! It felt outta this world, which says a lot considerin' I'm not in your realm. The walls were white, pipes with green fluids hung from ceilings, strange blue lights emitted from metal boxes an' tubes... And the spirit I saw was most intriguing! He just kept repeatin' about alarms, and wore a strange white an' blue suit. Very odd.",
                4 => "Like I said, I've been a spirit so long that I forgot my own name, yet I still remember most other things 'bout me. Strange that. I'm from Gathuram, the portal behind me leads to the Catacombs of Gathuram, and is how the undead and skeleton folk came here. Doesn't seem like you can enter it though.",
                _ => "...",
            };
        }
        public override bool CanChat() => true;
        public override string GetChat()
        {
            bool wearingHat = BasePlayer.HasHelmet(Main.LocalPlayer, ModContent.ItemType<OldTophat>());
            string s = "P";
            if (wearingHat)
                s = "Take it off an' p";
            if (Main.LocalPlayer.HasItem(ModContent.ItemType<OldTophat>()) || wearingHat)
                return "Ey, that old tophat ya got has something spiritual 'bout it. " + s + "eak into the Spirit Realm for a bit so I can get a closer look.";
            return "Great! You figured out how ta' call me! I hope ya take fancy to ma' gift. I'm err, let's just say a spiritual enthusiast. I don't remember my actual name. Ask me anythin'!";
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            var effects = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            int shader = GameShaders.Armor.GetShaderIdFromItemId(ItemID.MirageDye);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            GameShaders.Armor.ApplySecondary(shader, Main.player[Main.myPlayer], null);

            spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, NPC.Center - screenPos, NPC.frame, NPC.GetAlpha(Color.White), NPC.rotation, NPC.frame.Size() / 2, NPC.scale, effects, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
    }
}