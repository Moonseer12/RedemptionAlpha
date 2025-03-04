using BetterDialogue.UI;
using Microsoft.Xna.Framework.Graphics;
using Redemption.Base;
using Redemption.BaseExtension;
using Redemption.Dusts;
using Redemption.Globals;
using Redemption.Items.Armor.Vanity;
using Redemption.Items.Placeable.Furniture.Misc;
using Redemption.Items.Usable;
using Redemption.Items.Usable.Summons;
using Redemption.Textures.Emotes;
using Redemption.Tiles.Furniture.ElderWood;
using Redemption.Tiles.Furniture.PetrifiedWood;
using Redemption.Tiles.Furniture.Shade;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Personalities;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Redemption.NPCs.Friendly.TownNPCs
{
    [AutoloadHead]
    public class Newb : ModRedeNPC
    {
        public static int HeadIndex2;
        public override void Load()
        {
            // Adds our Shimmer Head to the NPCHeadLoader.
            HeadIndex2 = Mod.AddNPCHeadTexture(Type, Texture + "_Serious_Head");
        }
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Fool");
            Main.npcFrameCount[Type] = 25;
            NPCID.Sets.ExtraFramesCount[Type] = 5;
            NPCID.Sets.AttackFrameCount[Type] = 5;
            NPCID.Sets.DangerDetectRange[Type] = 80;
            NPCID.Sets.AttackType[Type] = 3;
            NPCID.Sets.AttackTime[Type] = 20;
            NPCID.Sets.AttackAverageChance[Type] = 10;
            NPCID.Sets.HatOffsetY[Type] = 4;
            NPCID.Sets.FaceEmote[Type] = EmoteBubbleType<NewbTownNPCEmote>();

            NPC.Happiness.SetBiomeAffection<ForestBiome>(AffectionLevel.Like);
            NPC.Happiness.SetBiomeAffection<UndergroundBiome>(AffectionLevel.Love);
            NPC.Happiness.SetBiomeAffection<DesertBiome>(AffectionLevel.Dislike);
            NPC.Happiness.SetBiomeAffection<OceanBiome>(AffectionLevel.Hate);

            NPC.Happiness.SetNPCAffection<Zephos>(AffectionLevel.Like);
            NPC.Happiness.SetNPCAffection<Daerel>(AffectionLevel.Like);
            NPC.Happiness.SetNPCAffection(NPCID.Wizard, AffectionLevel.Love);
            NPC.Happiness.SetNPCAffection(NPCID.Clothier, AffectionLevel.Dislike);
            NPC.Happiness.SetNPCAffection(NPCID.TaxCollector, AffectionLevel.Hate);

            NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new()
            {
                Velocity = 1f
            };

            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
        }

        public override void SetDefaults()
        {
            NPC.townNPC = true;
            NPC.friendly = true;
            NPC.width = 18;
            NPC.height = 46;
            NPC.aiStyle = 7;
            NPC.damage = 10;
            NPC.defense = 9999;
            NPC.lifeMax = 250;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.5f;

            DialogueBoxStyle = EPIDOTRA;

            AnimationType = NPCID.Guide;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Underground,

                new FlavorTextBestiaryInfoElement("..."),
            });
        }
        public override void AI()
        {
            if (RedeWorld.newbGone)
            {
                for (int i = 0; i < 30; i++)
                {
                    int d = Dust.NewDust(NPC.position + NPC.velocity, NPC.width, NPC.height, DustType<SightDust>(), NPC.velocity.X * 0.5f, NPC.velocity.Y * 0.5f, Scale: 4);
                    Main.dust[d].noGravity = true;
                }
                NPC.active = false;
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 15; i++)
                    Dust.NewDust(NPC.position + NPC.velocity, NPC.width, NPC.height, DustType<SightDust>(), NPC.velocity.X * 0.5f, NPC.velocity.Y * 0.5f, Scale: 4);
            }
            Dust.NewDust(NPC.position + NPC.velocity, NPC.width, NPC.height, DustID.Blood, NPC.velocity.X * 0.5f, NPC.velocity.Y * 0.5f);

        }
        public override bool CanTownNPCSpawn(int numTownNPCs)
        {
            return RedeBossDowned.foundNewb && !RedeWorld.newbGone;
        }

        private static bool HasPiano;
        public override bool CheckConditions(int left, int right, int top, int bottom)
        {
            bool gem = false;
            bool RUBIES = false;
            for (int x = left; x <= right; x++)
            {
                for (int y = top; y <= bottom; y++)
                {
                    int type = Framing.GetTileSafely(x, y).TileType;
                    if (type is TileID.Amethyst or TileID.Topaz or TileID.Sapphire or TileID.Emerald or TileID.Diamond or TileID.AmberStoneBlock or TileID.TreeAmethyst or TileID.TreeAmber or TileID.TreeDiamond or TileID.TreeEmerald or TileID.TreeSapphire or TileID.TreeTopaz or TileID.ExposedGems or TileID.Toilets)
                    {
                        if (type == TileID.ExposedGems && Framing.GetTileSafely(x, y).TileFrameX == 72)
                        {
                            RUBIES = true;
                            break;
                        }
                        if (type == TileID.Toilets && (Framing.GetTileSafely(x, y).TileFrameY < 1238 || Framing.GetTileSafely(x, y).TileFrameY > 1258))
                        {
                            RUBIES = true;
                            break;
                        }
                        gem = true;
                        break;
                    }
                }
            }
            if (RUBIES)
                gem = false;
            HasPiano = false;
            for (int x = left; x <= right; x++)
            {
                for (int y = top; y <= bottom; y++)
                {
                    int type = Main.tile[x, y].TileType;
                    if (type == TileID.Pianos || type == TileType<ElderWoodPianoTile>() || type == TileType<PetrifiedWoodPianoTile>() || type == TileType<ShadestonePianoTile>())
                    {
                        HasPiano = true;
                        return gem;
                    }
                }
            }
            return gem;
        }
        public override ITownNPCProfile TownNPCProfile() => new NewbProfile();
        public override string GetChat()
        {
            if (RedeBossDowned.downedNebuleus)
                Main.LocalPlayer.currentShoppingSettings.HappinessReport = "";
            Player player = Main.LocalPlayer;
            WeightedRandom<string> chat = new(Main.rand);
            if (RedeBossDowned.downedNebuleus)
            {
                if (RedeBossDowned.nebDeath > 6)
                {
                    chat.Add(Language.GetTextValue("Mods.Redemption.Dialogue.Fool.WokeDialogue1"));
                    chat.Add(Language.GetTextValue("Mods.Redemption.Dialogue.Fool.WokeDialogue2"));
                }
                chat.Add(Language.GetTextValue("Mods.Redemption.Dialogue.Fool.WokeDialogue3"));
                chat.Add(Language.GetTextValue("Mods.Redemption.Dialogue.Fool.WokeDialogue4"));
            }
            else
            {
                if (HasPiano)
                {
                    chat.Add(Language.GetTextValue("Mods.Redemption.Dialogue.Fool.PianoDialogue1"), 2);
                    chat.Add(Language.GetTextValue("Mods.Redemption.Dialogue.Fool.PianoDialogue2"), 2);
                }
                bool rember1 = Main.hardMode;
                bool rember2 = RedeBossDowned.downedSlayer || NPC.downedPlantBoss;
                if (!rember2 && BasePlayer.HasHelmet(player, ItemType<KingSlayerMask>(), true))
                    chat.Add(Language.GetTextValue("Mods.Redemption.Dialogue.Fool.KingSlayerMaskDialogue"));
                if (!rember2)
                {
                    if (player.RedemptionPlayerBuff().ChickenForm)
                        chat.Add(Language.GetTextValue("Mods.Redemption.Dialogue.Fool.ChickenDialogue"));
                    else
                        chat.Add(Language.GetTextValue("Mods.Redemption.Dialogue.Fool.1"));
                }
                if (!rember2 && !rember1)
                {
                    chat.Add(Language.GetTextValue("Mods.Redemption.Dialogue.Fool.2")); // 9.7%
                    chat.Add(Language.GetTextValue("Mods.Redemption.Dialogue.Fool.3"), 0.6); // 5.8%
                    chat.Add(Language.GetTextValue("Mods.Redemption.Dialogue.Fool.4"), 0.4); // 3.9%
                    chat.Add(Language.GetTextValue("Mods.Redemption.Dialogue.Fool.5"), 0.4);
                    chat.Add(Language.GetTextValue("Mods.Redemption.Dialogue.Fool.6"), 0.2); // 1.9%
                    chat.Add(Language.GetTextValue("Mods.Redemption.Dialogue.Fool.7"), 0.2);
                    chat.Add(Language.GetTextValue("Mods.Redemption.Dialogue.Fool.8"), 0.2);
                }
                if (rember1 && !rember2)
                {
                    chat.Add(Language.GetTextValue("Mods.Redemption.Dialogue.Fool.5"), 0.4);
                    chat.Add(Language.GetTextValue("Mods.Redemption.Dialogue.Fool.8"), 0.2);

                    chat.Add(Language.GetTextValue("Mods.Redemption.Dialogue.Fool.10"), 0.4);
                    chat.Add(Language.GetTextValue("Mods.Redemption.Dialogue.Fool.11"), 0.4);
                    chat.Add(Language.GetTextValue("Mods.Redemption.Dialogue.Fool.12"));
                    chat.Add(Language.GetTextValue("Mods.Redemption.Dialogue.Fool.13"), 0.4);
                    chat.Add(Language.GetTextValue("Mods.Redemption.Dialogue.Fool.14"), 0.4);
                    chat.Add(Language.GetTextValue("Mods.Redemption.Dialogue.Fool.15"), 0.6);
                    chat.Add(Language.GetTextValue("Mods.Redemption.Dialogue.Fool.16"), 0.6);
                    chat.Add(Language.GetTextValue("Mods.Redemption.Dialogue.Fool.17"), 0.8);
                }
                if (rember2)
                {
                    chat.Add(Language.GetTextValue("Mods.Redemption.Dialogue.Fool.18"));
                    chat.Add(Language.GetTextValue("Mods.Redemption.Dialogue.Fool.19"));
                    chat.Add(Language.GetTextValue("Mods.Redemption.Dialogue.Fool.20"));
                    chat.Add(Language.GetTextValue("Mods.Redemption.Dialogue.Fool.21"));
                    chat.Add(Language.GetTextValue("Mods.Redemption.Dialogue.Fool.22"));
                    chat.Add(Language.GetTextValue("Mods.Redemption.Dialogue.Fool.23"));
                    chat.Add(Language.GetTextValue("Mods.Redemption.Dialogue.Fool.24"));
                }

                if (RedeWorld.Alignment < 0)
                    chat.Add(Language.GetTextValue("Mods.Redemption.Dialogue.Fool.HuhDialogue"), 0.05); // 0.48%
            }
            return chat;
        }

        public override void SetChatButtons(ref string button, ref string button2)
        {
            if (!RedeBossDowned.downedNebuleus)
                button = Language.GetTextValue("LegacyInterface.28");
        }

        public override void OnChatButtonClicked(bool firstButton, ref string shopName)
        {
            if (firstButton)
                shopName = "Shop";
        }

        public override bool CanGoToStatue(bool toKingStatue) => true;
        public override void AddShops()
        {
            var npcShop = new NPCShop(Type)
                .Add(ItemID.DirtBlock)
                .Add(ItemID.MudBlock)
                .Add(ItemID.Amethyst, RedeConditions.DownedEarlyGameBossAndMoR)
                .Add(ItemID.Topaz, RedeConditions.DownedEarlyGameBossAndMoR)
                .Add(ItemID.Sapphire, RedeConditions.DownedEoCOrBoCOrKeeper)
                .Add(ItemID.Emerald, RedeConditions.DownedEoCOrBoCOrKeeper)
                .Add(new Item(ItemID.Geode) { shopCustomPrice = Item.buyPrice(0, 2) }, RedeConditions.DownedEoCOrBoCOrKeeper)
                .Add(ItemID.Ruby, RedeConditions.DownedSkeletronOrSeed)
                .Add(ItemID.Diamond, RedeConditions.DownedSkeletronOrSeed)
                .Add<OreBomb>(Condition.InBelowSurface)
                .Add<OrePowder>(Condition.InBelowSurface, Condition.Hardmode);
            npcShop.Register();
        }
        public override void ModifyActiveShop(string shopName, Item[] items)
        {
            foreach (Item item in items)
            {
                if (item == null || item.type == ItemID.None)
                    continue;

                if (item.type is ItemID.Geode && NPC.downedBoss3)
                    item.shopCustomPrice = Item.buyPrice(0, 1);
            }
        }
        public override int? PickEmote(Player closestPlayer, List<int> emoteList, WorldUIAnchor otherAnchor)
        {
            emoteList.Add(EmoteID.EmoteHappiness);
            emoteList.Add(EmoteID.EmoteLaugh);
            emoteList.Add(EmoteID.EmoteSilly);
            emoteList.Add(EmoteID.EmoteNote);
            emoteList.Add(EmoteID.EmoteConfused);
            emoteList.Add(EmoteID.ItemCog);
            emoteList.Add(EmoteID.BiomeRocklayer);
            emoteList.Add(EmoteID.CritterSkeleton);
            return base.PickEmote(closestPlayer, emoteList, otherAnchor);
        }
        public override void TownNPCAttackStrength(ref int damage, ref float knockback)
        {
            damage = 1;
            knockback = 7f;
        }
        public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
        {
            cooldown = 10;
            randExtraCooldown = 10;
        }

        public override void TownNPCAttackSwing(ref int itemWidth, ref int itemHeight)
        {
            itemWidth = itemHeight = 28;
        }
        public override void DrawTownAttackSwing(ref Texture2D item, ref Rectangle itemFrame, ref int itemSize, ref float scale, ref Vector2 offset)
        {
            Main.GetItemDrawFrame(ItemID.DirtRod, out item, out itemFrame);
            itemSize = 28;
            if (NPC.ai[1] > NPCID.Sets.AttackTime[NPC.type] * 0.66f)
                offset.Y = 12f;
        }
    }
    public class NewbProfile : ITownNPCProfile
    {
        public int RollVariation() => 0;
        public string GetNameForVariant(NPC npc) => npc.getNewNPCName();
        public Asset<Texture2D> GetTextureNPCShouldUse(NPC npc)
        {
            if (RedeBossDowned.downedNebuleus)
                return Request<Texture2D>("Redemption/NPCs/Friendly/TownNPCs/Newb_Serious");
            return Request<Texture2D>("Redemption/NPCs/Friendly/TownNPCs/Newb");
        }
        public int GetHeadTextureIndex(NPC npc)
        {
            if (RedeBossDowned.downedNebuleus)
                return Newb.HeadIndex2;
            return GetModHeadSlot("Redemption/NPCs/Friendly/TownNPCs/Newb_Head");
        }
    }
    public class NewbHomeButton : ChatButton
    {
        public override double Priority => 10;
        public override string Text(NPC npc, Player player) => Language.GetTextValue("Mods.Redemption.DialogueBox.HomeRequirements");
        public override string Description(NPC npc, Player player) => string.Empty;
        public override bool IsActive(NPC npc, Player player) => npc.type == NPCType<Newb>() && npc.homeless && !RedeBossDowned.downedNebuleus;
        public override void OnClick(NPC npc, Player player)
        {
            Main.npcChatText = Language.GetTextValue("Mods.Redemption.Dialogue.Fool.HomeDialogue");
            SoundEngine.PlaySound(SoundID.MenuTick);
        }
    }
}