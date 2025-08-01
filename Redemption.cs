global using Microsoft.Xna.Framework;
global using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Redemption.Backgrounds.Skies;
using Redemption.Base;
using Redemption.BaseExtension;
using Redemption.Buffs.Debuffs;
using Redemption.CrossMod;
using Redemption.Effects.PrimitiveTrails;
using Redemption.Effects.RenderTargets;
using Redemption.Globals;
using Redemption.Globals.Player;
using Redemption.Globals.World;
using Redemption.Items.Accessories.HM;
using Redemption.Items.Armor.PostML.Shinkite;
using Redemption.Items.Armor.PostML.Vorti;
using Redemption.Items.Armor.PreHM.DragonLead;
using Redemption.Items.Armor.Vanity.Dev;
using Redemption.Items.Donator.Arche;
using Redemption.Items.Donator.BLT;
using Redemption.Items.Donator.Uncon;
using Redemption.Items.Usable;
using Redemption.Items.Usable.Summons;
using Redemption.NPCs.Friendly;
using Redemption.NPCs.Friendly.TownNPCs;
using Redemption.NPCs.Lab;
using Redemption.NPCs.Lab.Janitor;
using Redemption.NPCs.Lab.Volt;
using Redemption.NPCs.Minibosses.Calavia;
using Redemption.UI;
using Redemption.UI.ChatUI;
using Redemption.WorldGeneration.Misc;
using Redemption.WorldGeneration.Soulless;
using ReLogic.Content;
using SubworldLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Chat;
using Terraria.GameContent;
using Terraria.GameContent.Shaders;
using Terraria.GameContent.UI;
using Terraria.Graphics;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;
using static Redemption.Globals.RedeNet;

namespace Redemption
{
    public partial class Redemption : Mod
    {
        public static Redemption Instance { get; private set; }

        public const string Abbreviation = "MoR";
        public const string EMPTY_TEXTURE = "Redemption/Empty";
        public const string PLACEHOLDER_TEXTURE = "Redemption/Placeholder";
        public Vector2 cameraOffset;
        public Rectangle currentScreen;
        public static int grooveTimer;

        public static ModKeybind RedeSpecialAbility { get; private set; }
        public static ModKeybind RedeSpiritwalkerAbility { get; private set; }
        public static ModKeybind RedeSkipDialogue { get; private set; }

        public static bool AprilFools => DateTime.Now is DateTime { Month: 4, Day: 1 };
        public static bool FinlandDay => DateTime.Now is DateTime { Month: 12, Day: 6 };

        public static BasicEffect basicEffect;
        public static RenderTargetManager Targets;
        public static Effect GlowTrailShader;
        public static TrailManager TrailManager;

        public static Effect nebSkyEffect;

        private List<ILoadable> _loadCache;

        public static int AntiqueDorulCurrencyId;
        public static int dragonLeadCapeID;
        public static int shinkiteCapeID;
        public static int mercenaryCapeID;
        public static int archeFemLegID;
        public static int archeMaleLegID;
        public static int unconFemLegID;
        public static int unconMaleLegID;
        public static int unconFemLeg2ID;
        public static int unconMaleLeg2ID;
        public static int halmFemLegID;
        public static int halmMaleLegID;

        public Redemption()
        {
            Instance = this;
            MusicSkipsVolumeRemap = true;
        }

        public override void Load()
        {
            LoadCache();

            #region Dialect Support
            BetterDialogue.BetterDialogue.SupportedNPCs.Add(NPCType<Fallen>());
            BetterDialogue.BetterDialogue.SupportedNPCs.Add(NPCType<Daerel>());
            BetterDialogue.BetterDialogue.SupportedNPCs.Add(NPCType<Zephos>());
            BetterDialogue.BetterDialogue.SupportedNPCs.Add(NPCType<TreebarkDryad>());
            BetterDialogue.BetterDialogue.SupportedNPCs.Add(NPCType<SoullessPortal>());
            BetterDialogue.BetterDialogue.SupportedNPCs.Add(NPCType<JustANormalToaster>());
            BetterDialogue.BetterDialogue.SupportedNPCs.Add(NPCType<HazmatCorpse_Ghost>());
            BetterDialogue.BetterDialogue.SupportedNPCs.Add(NPCType<Noza_NPC>());
            BetterDialogue.BetterDialogue.SupportedNPCs.Add(NPCType<Newb>());
            BetterDialogue.BetterDialogue.SupportedNPCs.Add(NPCType<SkullDiggerFriendly>());
            BetterDialogue.BetterDialogue.SupportedNPCs.Add(NPCType<SkullDiggerFriendly_Spirit>());
            BetterDialogue.BetterDialogue.SupportedNPCs.Add(NPCType<TBot>());
            BetterDialogue.BetterDialogue.SupportedNPCs.Add(NPCType<KS3Sitting>());
            BetterDialogue.BetterDialogue.SupportedNPCs.Add(NPCType<ForestNymph_Friendly>());
            BetterDialogue.BetterDialogue.SupportedNPCs.Add(NPCType<JanitorBot_NPC>());
            BetterDialogue.BetterDialogue.SupportedNPCs.Add(NPCType<Calavia_NPC>());
            BetterDialogue.BetterDialogue.SupportedNPCs.Add(NPCType<SpiritAssassin>());
            BetterDialogue.BetterDialogue.SupportedNPCs.Add(NPCType<SpiritCommonGuard>());
            BetterDialogue.BetterDialogue.SupportedNPCs.Add(NPCType<SpiritDruid>());
            BetterDialogue.BetterDialogue.SupportedNPCs.Add(NPCType<SpiritGathicMan>());
            BetterDialogue.BetterDialogue.SupportedNPCs.Add(NPCType<SpiritNiricLady>());
            BetterDialogue.BetterDialogue.SupportedNPCs.Add(NPCType<SpiritWalkerMan>());
            BetterDialogue.BetterDialogue.SupportedNPCs.Add(NPCType<ProtectorVolt_NPC>());
            BetterDialogue.BetterDialogue.RegisterShoppableNPC(NPCType<Fallen>());
            BetterDialogue.BetterDialogue.RegisterShoppableNPC(NPCType<Daerel>());
            BetterDialogue.BetterDialogue.RegisterShoppableNPC(NPCType<Zephos>());
            BetterDialogue.BetterDialogue.RegisterShoppableNPC(NPCType<TreebarkDryad>());
            BetterDialogue.BetterDialogue.RegisterShoppableNPC(NPCType<Newb>());
            BetterDialogue.BetterDialogue.RegisterShoppableNPC(NPCType<TBot>());
            BetterDialogue.BetterDialogue.RegisterShoppableNPC(NPCType<KS3Sitting>());
            BetterDialogue.BetterDialogue.RegisterShoppableNPC(NPCType<JanitorBot_NPC>());
            #endregion

            #region Add Equip Textures
            dragonLeadCapeID = EquipLoader.AddEquipTexture(this, "Redemption/Items/Armor/PreHM/DragonLead/DragonLeadRibplate_Back", EquipType.Back, GetInstance<DragonLeadRibplate>());
            shinkiteCapeID = EquipLoader.AddEquipTexture(this, "Redemption/Items/Armor/PostML/Shinkite/ShinkiteChestplate_Back", EquipType.Back, GetInstance<ShinkiteChestplate>());
            mercenaryCapeID = EquipLoader.AddEquipTexture(this, "Redemption/Items/Donator/BLT/MercenarysChestplate_Back", EquipType.Back, GetInstance<MercenarysChestplate>());
            archeMaleLegID = EquipLoader.AddEquipTexture(this, "Redemption/Items/Donator/Arche/ArchePatreonVanityLegs_Legs", EquipType.Legs, GetInstance<ArchePatreonVanityLegs>());
            archeFemLegID = EquipLoader.AddEquipTexture(this, "Redemption/Items/Donator/Arche/ArchePatreonVanityLegs_FemaleLegs", EquipType.Legs, GetInstance<ArchePatreonVanityLegs>(), "ArchePatreonVanityLegs_Female");
            unconMaleLegID = EquipLoader.AddEquipTexture(this, "Redemption/Items/Donator/Uncon/UnconLegs_Legs", EquipType.Legs, GetInstance<UnconLegs>());
            unconFemLegID = EquipLoader.AddEquipTexture(this, "Redemption/Items/Donator/Uncon/UnconLegs_FemaleLegs", EquipType.Legs, GetInstance<UnconLegs>(), "UnconLegs_Female");
            unconMaleLeg2ID = EquipLoader.AddEquipTexture(this, "Redemption/Items/Donator/Uncon/UnconLegs2_Legs", EquipType.Legs, GetInstance<UnconLegs2>());
            unconFemLeg2ID = EquipLoader.AddEquipTexture(this, "Redemption/Items/Donator/Uncon/UnconLegs2_FemaleLegs", EquipType.Legs, GetInstance<UnconLegs2>(), "UnconLegs2_Female");
            halmMaleLegID = EquipLoader.AddEquipTexture(this, "Redemption/Items/Armor/Vanity/Dev/HallamLeggings_Legs", EquipType.Legs, GetInstance<HallamLeggings>());
            halmFemLegID = EquipLoader.AddEquipTexture(this, "Redemption/Items/Armor/Vanity/Dev/HallamLeggings_FemaleLegs", EquipType.Legs, GetInstance<HallamLeggings>(), "HallamLeggings_Female");
            #endregion

            if (!Main.dedServ)
            {
                TrailManager = new TrailManager(this);
                AdditiveCallManager.Load();

                int width = Main.graphics.GraphicsDevice.Viewport.Width;
                int height = Main.graphics.GraphicsDevice.Viewport.Height;
                Vector2 zoom = Main.GameViewMatrix.Zoom;
                Matrix view = Matrix.CreateLookAt(Vector3.Zero, Vector3.UnitZ, Vector3.Up) * Matrix.CreateTranslation(width / 2, height / -2, 0) * Matrix.CreateRotationZ(MathHelper.Pi) * Matrix.CreateScale(zoom.X, zoom.Y, 1f);
                Matrix projection = Matrix.CreateOrthographic(width, height, 0, 1000);

                AssetRequestMode immLoad = AssetRequestMode.ImmediateLoad;
                Main.QueueMainThreadAction(() =>
                {
                    basicEffect = new BasicEffect(Main.graphics.GraphicsDevice)
                    {
                        VertexColorEnabled = true,
                        View = view,
                        Projection = projection
                    };
                    #region PremultiplyTextures
                    Texture2D bubbleTex = Request<Texture2D>("Redemption/Textures/BubbleShield", immLoad).Value;
                    PremultiplyTexture(ref bubbleTex);
                    Texture2D portalTex = Request<Texture2D>("Redemption/Textures/PortalTex", immLoad).Value;
                    PremultiplyTexture(ref portalTex);
                    Texture2D soullessPortal = Request<Texture2D>("Redemption/NPCs/Friendly/SoullessPortal", immLoad).Value;
                    PremultiplyTexture(ref soullessPortal);
                    Texture2D holyGlowTex = Request<Texture2D>("Redemption/Textures/WhiteGlow", immLoad).Value;
                    PremultiplyTexture(ref holyGlowTex);
                    Texture2D whiteFlareTex = Request<Texture2D>("Redemption/Textures/WhiteFlare", immLoad).Value;
                    PremultiplyTexture(ref whiteFlareTex);
                    Texture2D whiteOrbTex = Request<Texture2D>("Redemption/Textures/WhiteOrb", immLoad).Value;
                    PremultiplyTexture(ref whiteOrbTex);
                    Texture2D whiteLightBeamTex = Request<Texture2D>("Redemption/Textures/WhiteLightBeam", immLoad).Value;
                    PremultiplyTexture(ref whiteLightBeamTex);
                    Texture2D transitionTex = Request<Texture2D>("Redemption/Textures/TransitionTex", immLoad).Value;
                    PremultiplyTexture(ref transitionTex);
                    Texture2D staticBallTex = Request<Texture2D>("Redemption/Textures/StaticBall", immLoad).Value;
                    PremultiplyTexture(ref staticBallTex);
                    Texture2D iceMistTex = Request<Texture2D>("Redemption/Textures/IceMist", immLoad).Value;
                    PremultiplyTexture(ref iceMistTex);
                    Texture2D glowDustTex = Request<Texture2D>("Redemption/Dusts/GlowDust", immLoad).Value;
                    PremultiplyTexture(ref glowDustTex);
                    Texture2D AkkaHealingSpiritTex = Request<Texture2D>("Redemption/NPCs/Bosses/ADD/AkkaHealingSpirit", immLoad).Value;
                    PremultiplyTexture(ref AkkaHealingSpiritTex);
                    Texture2D AkkaIslandWarningTex = Request<Texture2D>("Redemption/NPCs/Bosses/ADD/AkkaIslandWarning", immLoad).Value;
                    PremultiplyTexture(ref AkkaIslandWarningTex);
                    Texture2D SunTex = Request<Texture2D>("Redemption/Textures/Sun", immLoad).Value;
                    PremultiplyTexture(ref SunTex);
                    Texture2D DarkSoulTex = Request<Texture2D>("Redemption/Textures/DarkSoulTex", immLoad).Value;
                    PremultiplyTexture(ref DarkSoulTex);
                    Texture2D TornadoTex = Request<Texture2D>("Redemption/Textures/TornadoTex", immLoad).Value;
                    PremultiplyTexture(ref TornadoTex);
                    Texture2D SpiritPortalTex = Request<Texture2D>("Redemption/Textures/SpiritPortalTex", immLoad).Value;
                    PremultiplyTexture(ref SpiritPortalTex);
                    Texture2D BigFlare = Request<Texture2D>("Redemption/Textures/BigFlare", immLoad).Value;
                    PremultiplyTexture(ref BigFlare);
                    Texture2D BubbleShield2 = Request<Texture2D>("Redemption/Textures/BubbleShield2", immLoad).Value;
                    PremultiplyTexture(ref BubbleShield2);

                    Texture2D purityWastelandBG3Tex = Request<Texture2D>("Redemption/Backgrounds/PurityWastelandBG3", immLoad).Value;
                    PremultiplyTexture(ref purityWastelandBG3Tex);
                    Texture2D wastelandCrimsonBG3Tex = Request<Texture2D>("Redemption/Backgrounds/WastelandCrimsonBG3", immLoad).Value;
                    PremultiplyTexture(ref wastelandCrimsonBG3Tex);
                    Texture2D wastelandCorruptionBG3Tex = Request<Texture2D>("Redemption/Backgrounds/WastelandCorruptionBG3", immLoad).Value;
                    PremultiplyTexture(ref wastelandCorruptionBG3Tex);
                    Texture2D ruinedKingdomSurfaceClose_MenuTex = Request<Texture2D>("Redemption/Backgrounds/RuinedKingdomSurfaceClose_Menu", immLoad).Value;
                    PremultiplyTexture(ref ruinedKingdomSurfaceClose_MenuTex);
                    Texture2D ruinedKingdomSurfaceFar_MenuTex = Request<Texture2D>("Redemption/Backgrounds/RuinedKingdomSurfaceFar_Menu", immLoad).Value;
                    PremultiplyTexture(ref ruinedKingdomSurfaceFar_MenuTex);
                    Texture2D ruinedKingdomSurfaceMid_MenuTex = Request<Texture2D>("Redemption/Backgrounds/RuinedKingdomSurfaceMid_Menu", immLoad).Value;
                    PremultiplyTexture(ref ruinedKingdomSurfaceMid_MenuTex);
                    Texture2D UkkoCloudsTex = Request<Texture2D>("Redemption/Backgrounds/Skies/UkkoClouds", immLoad).Value;
                    PremultiplyTexture(ref UkkoCloudsTex);
                    Texture2D UkkoSkyBeamTex = Request<Texture2D>("Redemption/Backgrounds/Skies/UkkoSkyBeam", immLoad).Value;
                    PremultiplyTexture(ref UkkoSkyBeamTex);
                    Texture2D UkkoSkyBoltTex = Request<Texture2D>("Redemption/Backgrounds/Skies/UkkoSkyBolt", immLoad).Value;
                    PremultiplyTexture(ref UkkoSkyBoltTex);
                    Texture2D UkkoSkyFlashTex = Request<Texture2D>("Redemption/Backgrounds/Skies/UkkoSkyFlash", immLoad).Value;
                    PremultiplyTexture(ref UkkoSkyFlashTex);
                    Texture2D UkkoBarrierFogTex = Request<Texture2D>("Redemption/Textures/UkkoBarrier_Fog", immLoad).Value;
                    PremultiplyTexture(ref UkkoBarrierFogTex);
                    Texture2D SkyTex = Request<Texture2D>("Redemption/Backgrounds/Skies/SkyTex", immLoad).Value;
                    PremultiplyTexture(ref SkyTex);
                    Texture2D SkyTex2 = Request<Texture2D>("Redemption/Backgrounds/Skies/SkyTex2", immLoad).Value;
                    PremultiplyTexture(ref SkyTex2);
                    Texture2D SkyTex3 = Request<Texture2D>("Redemption/Backgrounds/Skies/SkyTex3", immLoad).Value;
                    PremultiplyTexture(ref SkyTex3);
                    Texture2D WastelandCorruptSkyTex = Request<Texture2D>("Redemption/Backgrounds/Skies/WastelandCorruptSkyTex", immLoad).Value;
                    PremultiplyTexture(ref WastelandCorruptSkyTex);
                    Texture2D WastelandCrimsonSkyTex = Request<Texture2D>("Redemption/Backgrounds/Skies/WastelandCrimsonSkyTex", immLoad).Value;
                    PremultiplyTexture(ref WastelandCrimsonSkyTex);
                    #endregion
                });

                if (Main.netMode != NetmodeID.Server)
                {
                    nebSkyEffect = Request<Effect>("Redemption/Effects/nebSky", AssetRequestMode.ImmediateLoad).Value;
                }

                Filters.Scene["MoR:OOSky"] = new Filter(new ScreenShaderData("FilterMiniTower").UseColor(0.2f, 0f, 0f).UseOpacity(0.2f), EffectPriority.VeryHigh);
                SkyManager.Instance["MoR:OOSky"] = new OOSky();
                Filters.Scene["MoR:NebP1"] = new Filter(new ScreenShaderData("FilterMiniTower").UseColor(0.2f, 0f, 0.3f).UseOpacity(0.5f), EffectPriority.VeryHigh);
                SkyManager.Instance["MoR:NebP1"] = new NebSky();
                Filters.Scene["MoR:NebP2"] = new Filter(new ScreenShaderData("FilterMiniTower").UseColor(0.2f, 0f, 0.3f).UseOpacity(0.5f), EffectPriority.VeryHigh);
                SkyManager.Instance["MoR:NebP2"] = new NebSky2();
                Filters.Scene["MoR:Ukko"] = new Filter(new ScreenShaderData("FilterMiniTower").UseColor(0.2f, 0.1f, 0f).UseOpacity(0.3f), EffectPriority.VeryHigh);
                SkyManager.Instance["MoR:Ukko"] = new UkkoClouds();
            }
            Filters.Scene["MoR:WastelandSky"] = new Filter(new ScreenShaderData("FilterMiniTower").UseColor(0f, 0.2f, 0f).UseOpacity(0.5f), EffectPriority.High);
            SkyManager.Instance["MoR:WastelandSky"] = new WastelandSky();
            SkyManager.Instance["MoR:WastelandSnowSky"] = new WastelandSnowSky();
            SkyManager.Instance["MoR:WastelandCorruptSky"] = new WastelandCorruptSky();
            SkyManager.Instance["MoR:WastelandCrimsonSky"] = new WastelandCrimsonSky();

            Filters.Scene["MoR:SpiritSky"] = new Filter(new ScreenShaderData("FilterMiniTower").UseColor(0.4f, 0.8f, 0.8f), EffectPriority.VeryHigh);
            Filters.Scene["MoR:IslandEffect"] = new Filter(new ScreenShaderData("FilterMiniTower").UseColor(0.4f, 0.4f, 0.4f).UseOpacity(0.5f), EffectPriority.VeryHigh);
            SkyManager.Instance["MoR:RuinedKingdomSky"] = new RuinedKingdomSky();
            Filters.Scene["MoR:SoullessSky"] = new Filter(new ScreenShaderData("FilterMiniTower").UseColor(0f, 0f, 0f).UseOpacity(0.35f), EffectPriority.High);
            Filters.Scene["MoR:FowlMorningSky"] = new Filter(new ScreenShaderData("FilterMiniTower").UseColor(0.7f, 0.3f, 0.02f).UseOpacity(0.3f), EffectPriority.High);
            Filters.Scene["MoR:ThornSky"] = new Filter(new ScreenShaderData("FilterMiniTower").UseColor(0.2f, 0.25f, 0.15f).UseOpacity(0.7f), EffectPriority.High);

            Filters.Scene["MoR:Shake"] = new Filter(new MoonLordScreenShaderData("FilterMoonLordShake", aimAtPlayer: false), EffectPriority.VeryHigh);

            RedeSpecialAbility = KeybindLoader.RegisterKeybind(this, "SpecialAbilityKey", Keys.F);
            RedeSpiritwalkerAbility = KeybindLoader.RegisterKeybind(this, "SpiritWalkerKey", Keys.K);
            RedeSkipDialogue = KeybindLoader.RegisterKeybind(this, "SkipDialogueKey", Keys.Back);
            AntiqueDorulCurrencyId = CustomCurrencyManager.RegisterCurrency(new AntiqueDorulCurrency(ItemType<AncientGoldCoin>(), 999L, "Antique Doruls"));
        }
        public override void Unload()
        {
            if (_loadCache != null)
            {
                foreach (ILoadable loadable in _loadCache)
                    loadable.Unload();

                _loadCache = null;
            }
            else
                Logger.Warn("load cache was null, ILoadable's may not have been unloaded...");

            TrailManager = null;
            AdditiveCallManager.Unload();
        }
        public override void PostSetupContent()
        {
            WeakReferences.PerformModSupport();
            if (!Main.dedServ)
            {
                Main.QueueMainThreadAction(() =>
                {
                    OnHeadDraw.RegisterHeads();
                    OnLegDraw.RegisterLegs();
                    OnBodyDraw.RegisterBodies();
                });
            }
        }
        public static void PremultiplyTexture(ref Texture2D texture)
        {
            Color[] buffer = new Color[texture.Width * texture.Height];
            texture.GetData(buffer);
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = Color.FromNonPremultiplied(
                        buffer[i].R, buffer[i].G, buffer[i].B, buffer[i].A);
            }
            texture.SetData(buffer);
        }
        private void LoadCache()
        {
            _loadCache = new List<ILoadable>();
            foreach (Type type in Code.GetTypes())
            {
                if (!type.IsAbstract && type.GetInterfaces().Contains(typeof(ILoadable)))
                    _loadCache.Add(Activator.CreateInstance(type) as ILoadable);
            }

            _loadCache.Sort((x, y) => x.Priority > y.Priority ? 1 : -1);

            for (int i = 0; i < _loadCache.Count; ++i)
            {
                if (Main.dedServ && !_loadCache[i].LoadOnDedServer)
                    continue;

                _loadCache[i].Load();
            }
        }

        public ModPacket GetPacket(ModMessageType type, int capacity)
        {
            ModPacket packet = GetPacket(capacity + 1);
            packet.Write((byte)type);
            return packet;
        }
        public static ModPacket WriteToPacket(ModPacket packet, byte msg, params object[] param)
        {
            packet.Write(msg);

            for (int m = 0; m < param.Length; m++)
            {
                object obj = param[m];
                if (obj is bool boolean) packet.Write(boolean);
                else
                if (obj is byte @byte) packet.Write(@byte);
                else
                if (obj is int @int) packet.Write(@int);
                else
                if (obj is float single) packet.Write(single);
                else
                if (obj is Vector2 vector) { packet.Write(vector.X); packet.Write(vector.Y); }
            }
            return packet;
        }
        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            ModMessageType msgType = (ModMessageType)reader.ReadByte();
            switch (msgType)
            {
                case ModMessageType.BossSpawnFromClient:
                    if (Main.netMode == NetmodeID.Server)
                    {
                        int bossType = reader.ReadInt32();
                        int npcCenterX = reader.ReadInt32();
                        int npcCenterY = reader.ReadInt32();

                        if (NPC.AnyNPCs(bossType))
                            return;

                        int npcID = NPC.NewNPC(Entity.GetSource_NaturalSpawn(), npcCenterX, npcCenterY, bossType);
                        Main.npc[npcID].netUpdate = true;
                        ChatHelper.BroadcastChatMessage(NetworkText.FromKey("Announcement.HasAwoken", Main.npc[npcID].GetTypeNetName()), new Color(175, 75, 255));
                    }
                    break;
                case ModMessageType.SpawnNPCFromClient:
                    if (Main.netMode == NetmodeID.Server)
                    {
                        int npcIndex = reader.ReadInt32();
                        int npcCenterX = reader.ReadInt32();
                        int npcCenterY = reader.ReadInt32();
                        float ai0 = reader.ReadSingle();
                        float ai1 = reader.ReadSingle();
                        float ai2 = reader.ReadSingle();

                        int npcID = NPC.NewNPC(Entity.GetSource_NaturalSpawn(), npcCenterX, npcCenterY, npcIndex, 0, ai0, ai1, ai2);
                        Main.npc[npcID].netUpdate = true;
                    }
                    break;
                case ModMessageType.SpawnTrail:
                    int projindex = reader.ReadInt32();

                    if (Main.netMode == NetmodeID.Server)
                    {
                        //If received by the server, send to all clients instead
                        WriteToPacket(Instance.GetPacket(), (byte)ModMessageType.SpawnTrail, projindex).Send();
                        break;
                    }

                    if (Main.projectile[projindex].ModProjectile is IManualTrailProjectile trailProj)
                        trailProj.DoTrailCreation(TrailManager);
                    break;
                case ModMessageType.StartFowlMorning:
                    FowlMorningWorld.FowlMorningActive = true;
                    FowlMorningWorld.ChickArmyStart();
                    break;
                case ModMessageType.FowlMorningData:
                    FowlMorningWorld.HandlePacket(reader);
                    break;
                case ModMessageType.SyncRedeQuestFromClient:
                    RedeQuest.ReceiveSyncDataFromClient(reader, whoAmI);
                    break;
                case ModMessageType.SyncRedeWorldFromClient:
                    RedeWorld.ReceiveSyncDataFromClient(reader, whoAmI);
                    break;
                case ModMessageType.SyncAlignment:
                    RedeWorld.ReceiveSyncAlignment(reader, whoAmI);
                    break;
                case ModMessageType.SyncChaliceDialogue:
                    ChaliceAlignmentUI.ReceiveSyncChaliceDialogue(reader, whoAmI);
                    break;
                case ModMessageType.TitleCardFromServer:
                    TitleCard.ReceiveTitleCardFromServer(reader, whoAmI);
                    break;
                case ModMessageType.SyncRedePlayer:
                    RedePlayer.ReceiveSyncPlayer(reader, whoAmI);
                    break;
                case ModMessageType.RequestArena:
                    ArenaSystem.HandleRequestArena(reader);
                    break;
                case ModMessageType.SyncArena:
                    ArenaSystem.HandleSyncArena(reader);
                    break;
                case ModMessageType.RequestSyncArena:
                    ArenaSystem.HandleRequestSyncArena(whoAmI);
                    break;
            }
        }
        public static void SpawnBossFromClient(byte whoAmI, int type, int x, int y) => WriteToPacket(Instance.GetPacket(), (byte)ModMessageType.BossSpawnFromClient, whoAmI, type, x, y).Send();
    }
    public class RedeSystem : ModSystem
    {
        public static RedeSystem Instance { get; private set; }
        public RedeSystem()
        {
            Instance = this;
        }

        public static bool Silence;
        public override void PreUpdateNPCs()
        {
            Silence = false;
        }

        public UserInterface DialogueUILayer;
        public MoRDialogueUI DialogueUIElement;

        public UserInterface ChaliceUILayer;
        public ChaliceAlignmentUI ChaliceUIElement;

        public UserInterface TitleUILayer;
        public TitleCard TitleCardUIElement;

        public UserInterface NukeUILayer;
        public NukeDetonationUI NukeUIElement;

        public UserInterface AMemoryUILayer;
        public AMemoryUIState AMemoryUIElement;

        public UserInterface TextBubbleUILayer;
        public ChatUI TextBubbleUIElement;

        public UserInterface YesNoUILayer;
        public YesNoUI YesNoUIElement;

        public UserInterface TradeUILayer;
        public TradeUI TradeUIElement;

        public UserInterface AlignmentButtonUILayer;
        public AlignmentButton AlignmentButtonUIElement;

        public UserInterface SpiritWalkerButtonUILayer;
        public SpiritWalkerButton SpiritWalkerButtonUIElement;

        public UserInterface ElementPanelUILayer;
        public ElementPanelUI ElementPanelUIElement;

        public override void Load()
        {
            RedeDetours.Initialize();
            if (!Main.dedServ)
            {
                TitleUILayer = new UserInterface();
                TitleCardUIElement = new TitleCard();
                TitleUILayer.SetState(TitleCardUIElement);

                DialogueUILayer = new UserInterface();
                DialogueUIElement = new MoRDialogueUI();
                DialogueUILayer.SetState(DialogueUIElement);

                ChaliceUILayer = new UserInterface();
                ChaliceUIElement = new ChaliceAlignmentUI();
                ChaliceUILayer.SetState(ChaliceUIElement);

                NukeUILayer = new UserInterface();
                NukeUIElement = new NukeDetonationUI();
                NukeUILayer.SetState(NukeUIElement);

                AMemoryUILayer = new UserInterface();
                AMemoryUIElement = new AMemoryUIState();
                AMemoryUILayer.SetState(AMemoryUIElement);

                TextBubbleUILayer = new UserInterface();
                TextBubbleUIElement = new ChatUI();
                TextBubbleUILayer.SetState(TextBubbleUIElement);

                YesNoUILayer = new UserInterface();
                YesNoUIElement = new YesNoUI();
                YesNoUILayer.SetState(YesNoUIElement);

                TradeUILayer = new UserInterface();
                TradeUIElement = new TradeUI();
                TradeUILayer.SetState(TradeUIElement);

                AlignmentButtonUILayer = new UserInterface();
                AlignmentButtonUIElement = new AlignmentButton();
                AlignmentButtonUILayer.SetState(AlignmentButtonUIElement);

                SpiritWalkerButtonUILayer = new UserInterface();
                SpiritWalkerButtonUIElement = new SpiritWalkerButton();
                SpiritWalkerButtonUILayer.SetState(SpiritWalkerButtonUIElement);

                ElementPanelUILayer = new UserInterface();
                ElementPanelUIElement = new ElementPanelUI();
                ElementPanelUILayer.SetState(ElementPanelUIElement);
            }
        }
        public override void Unload()
        {
            RedeDetours.Uninitialize();
        }
        public override void ModifyLightingBrightness(ref float scale)
        {
            if (GetInstance<RedeTileCount>().WastelandCrimsonTileCount >= 50 || GetInstance<RedeTileCount>().WastelandCorruptTileCount >= 50)
                scale = .9f;
        }
        public override void PreUpdateItems()
        {
            if (Main.netMode != NetmodeID.Server)
                Redemption.TrailManager.UpdateTrails();
        }
        public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
        {
            RedeTileCount tileCount = GetInstance<RedeTileCount>();
            if (NPC.downedMechBossAny && tileCount.WastelandTileCount > 0)
            {
                float Strength = tileCount.WastelandTileCount / 200f;
                Strength = Math.Min(Strength, 1f);

                int sunR = backgroundColor.R;
                int sunG = backgroundColor.G;
                int sunB = backgroundColor.B;
                sunR -= (int)(40f * Strength * (backgroundColor.R / 255f));
                sunB -= (int)(40f * Strength * (backgroundColor.B / 255f));
                sunG -= (int)(30f * Strength * (backgroundColor.G / 255f));
                sunR = Utils.Clamp(sunR, 15, 255);
                sunG = Utils.Clamp(sunG, 15, 255);
                sunB = Utils.Clamp(sunB, 15, 255);
                backgroundColor.R = (byte)sunR;
                backgroundColor.G = (byte)sunG;
                backgroundColor.B = (byte)sunB;
            }
            if (SubworldSystem.IsActive<CSub>())
            {
                backgroundColor.R = 15;
                backgroundColor.G = 15;
                backgroundColor.B = 15;
                tileColor.R = 15;
                tileColor.G = 15;
                tileColor.B = 15;
            }
        }
        public override void ModifyTransformMatrix(ref SpriteViewMatrix Transform)
        {
            if (Main.gameMenu || RedeConfigClient.Instance.CameraLockDisable)
                return;

            Player player = Main.LocalPlayer;
            ScreenPlayer screenPlayer = player.GetModPlayer<ScreenPlayer>();

            if (screenPlayer.timedZoomDurationMax > 0 && screenPlayer.timedZoom != Vector2.Zero)
            {
                float lerpAmount = MathHelper.Lerp(0, MathHelper.PiOver2, screenPlayer.timedZoomTime / screenPlayer.timedZoomTimeMax);

                Vector2 idealScreenZoom = screenPlayer.timedZoom;
                Transform.Zoom = Vector2.Lerp(new Vector2(Main.GameViewMatrix.Zoom.X), idealScreenZoom, (float)Math.Sin(lerpAmount));
            }
            if (screenPlayer.customZoom > 1)
                Transform.Zoom = new Vector2(screenPlayer.customZoom * Main.GameViewMatrix.Zoom.X);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            // MP Warning
            if (Main.netMode != NetmodeID.SinglePlayer && !RedeConfigClient.Instance.DisableMPWarning)
            {
                int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Ruler"));
                LegacyGameInterfaceLayer StunUI = new("Redemption: MP Warning",
                    delegate
                    {
                        DrawMPWarning(Main.spriteBatch);
                        return true;
                    },
                    InterfaceScaleType.UI);
                layers.Insert(index, StunUI);
            }
            BuffPlayer bP = Main.LocalPlayer.GetModPlayer<BuffPlayer>();
            if (Main.LocalPlayer.active && !Main.LocalPlayer.dead && Main.LocalPlayer.HasBuff<StunnedDebuff>())
            {
                int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Ruler"));
                LegacyGameInterfaceLayer StunUI = new("Redemption: Stun UI",
                    delegate
                    {
                        DrawStunStars(Main.spriteBatch);
                        return true;
                    },
                    InterfaceScaleType.Game);
                layers.Insert(index, StunUI);
            }
            if (Main.LocalPlayer.active && !Main.LocalPlayer.dead && BasePlayer.HasAccessory(Main.LocalPlayer, ItemType<PocketShieldGenerator>(), true, true))
            {
                int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Ruler"));
                LegacyGameInterfaceLayer ShieldGaugeUI = new("Redemption: Shield Gauge UI",
                    delegate
                    {
                        DrawShieldGenGauge(Main.spriteBatch);
                        return true;
                    },
                    InterfaceScaleType.Game);
                layers.Insert(index, ShieldGaugeUI);
            }
            EnergyPlayer eP = Main.LocalPlayer.GetModPlayer<EnergyPlayer>();
            if (Main.LocalPlayer.active && !Main.LocalPlayer.dead && eP.statEnergy < eP.energyMax)
            {
                int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Ruler"));
                LegacyGameInterfaceLayer EnergyGaugeUI = new("Redemption: Energy Gauge UI",
                    delegate
                    {
                        DrawEnergyGauge(Main.spriteBatch);
                        return true;
                    },
                    InterfaceScaleType.Game);
                layers.Insert(index, EnergyGaugeUI);
            }
            if (Main.LocalPlayer.active && !Main.LocalPlayer.dead && Main.LocalPlayer.HeldItem.CountsAsClass<DamageClasses.RitualistClass>())
            {
                int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Ruler"));
                LegacyGameInterfaceLayer SpiritGaugeUI = new("Redemption: Spirit Gauge UI",
                    delegate
                    {
                        DrawSpiritGauge(Main.spriteBatch);
                        return true;
                    },
                    InterfaceScaleType.Game);
                layers.Insert(index, SpiritGaugeUI);
            }
            if (Main.LocalPlayer.active && !Main.LocalPlayer.dead && NPC.downedGolemBoss && Main.LocalPlayer.HeldItem.type == ItemType<OmegaTransmitter>())
            {
                int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Ruler"));
                LegacyGameInterfaceLayer OmegaTransmitterUI = new("Redemption: Omega Transmitter UI",
                    delegate
                    {
                        DrawOmegaTransmitterText(Main.spriteBatch);
                        return true;
                    },
                    InterfaceScaleType.Game);
                layers.Insert(index, OmegaTransmitterUI);
            }
            if (YesNoUI.Visible && (!Main.playerInventory || YesNoUIElement.Player.whoAmI != Main.myPlayer))
            {
                int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Ruler"));
                LegacyGameInterfaceLayer ChoiceTextUI = new("Redemption: Choice Text UI",
                    delegate
                    {
                        YesNoUI.DrawChoiceText(Main.spriteBatch);
                        return true;
                    },
                    InterfaceScaleType.UI);
                layers.Insert(index, ChoiceTextUI);
            }
            if (Main.LocalPlayer.Redemption().slayerCursor)
            {
                int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Interface Logic 4"));
                LegacyGameInterfaceLayer SlayerCursorUI = new("Redemption: Slayer Cursor UI",
                    delegate
                    {
                        DrawSlayerCursor(Main.spriteBatch);
                        return true;
                    },
                    InterfaceScaleType.UI);
                layers.Insert(index, SlayerCursorUI);
            }
            if (RedeWorld.SkeletonInvasion)
            {
                int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
                if (index >= 0)
                {
                    LegacyGameInterfaceLayer SkeleUI = new("Redemption: SkeleInvasion",
                        delegate
                        {
                            DrawSkeletonInvasionUI(Main.spriteBatch);
                            return true;
                        },
                        InterfaceScaleType.UI);
                    layers.Insert(index, SkeleUI);
                }
            }
            if (FowlMorningWorld.FowlMorningActive)
            {
                int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
                if (index >= 0)
                {
                    LegacyGameInterfaceLayer FowlUI = new("Redemption: FowlMorning",
                        delegate
                        {
                            DrawFowlMorningUI(Main.spriteBatch);
                            return true;
                        },
                        InterfaceScaleType.UI);
                    layers.Insert(index, FowlUI);
                }
            }
            layers.Insert(layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text")), new LegacyGameInterfaceLayer("GUI Menus",
                delegate
                {
                    return true;
                }, InterfaceScaleType.UI));
            int MouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (MouseTextIndex != -1)
            {
                AddInterfaceLayer(layers, AMemoryUILayer, AMemoryUIElement, MouseTextIndex, AMemoryUIState.Visible, "Lab Photo");
                AddInterfaceLayer(layers, ChaliceUILayer, ChaliceUIElement, MouseTextIndex + 1, ChaliceAlignmentUI.Visible, "Chalice");
                AddInterfaceLayer(layers, DialogueUILayer, DialogueUIElement, MouseTextIndex + 2, MoRDialogueUI.Visible, "Dialogue");
                AddInterfaceLayer(layers, TitleUILayer, TitleCardUIElement, MouseTextIndex + 3, TitleCard.Showing, "Title Card");
                AddInterfaceLayer(layers, NukeUILayer, NukeUIElement, MouseTextIndex + 4, NukeDetonationUI.Visible, "Nuke UI");
                AddInterfaceLayer(layers, TextBubbleUILayer, TextBubbleUIElement, MouseTextIndex + 5, ChatUI.Visible, "Text Bubble");
                AddInterfaceLayer(layers, YesNoUILayer, YesNoUIElement, MouseTextIndex + 6, YesNoUI.Visible, "Yes No Choice");
                AddInterfaceLayer(layers, TradeUILayer, TradeUIElement, MouseTextIndex + 7, TradeUI.Visible, "Trade");
                AddInterfaceLayer(layers, ElementPanelUILayer, ElementPanelUIElement, MouseTextIndex + 8, ElementPanelUI.Visible, "Element Panel");
                AddInterfaceLayer(layers, SpiritWalkerButtonUILayer, SpiritWalkerButtonUIElement, MouseTextIndex + 8, Main.LocalPlayer.RedemptionAbility().Spiritwalker && Main.playerInventory, "Spirit Walker Button");
                AddInterfaceLayer(layers, AlignmentButtonUILayer, AlignmentButtonUIElement, MouseTextIndex + 8, RedeWorld.alignmentGiven && Main.playerInventory, "Alignment Button");
            }
        }
        public override void UpdateUI(GameTime gameTime)
        {
            if (AMemoryUILayer?.CurrentState != null && AMemoryUIState.Visible)
                AMemoryUILayer.Update(gameTime);
            if (ElementPanelUILayer?.CurrentState != null && ElementPanelUI.Visible)
                ElementPanelUILayer.Update(gameTime);
            if (NukeUILayer?.CurrentState != null && NukeDetonationUI.Visible)
                NukeUILayer.Update(gameTime);
            if (TradeUILayer?.CurrentState != null && TradeUI.Visible)
                TradeUILayer.Update(gameTime);
            if (YesNoUILayer?.CurrentState != null && YesNoUI.Visible)
                YesNoUILayer.Update(gameTime);
            if (AlignmentButtonUILayer?.CurrentState != null && RedeWorld.alignmentGiven && Main.playerInventory)
                AlignmentButtonUILayer.Update(gameTime);
        }
        public static void AddInterfaceLayer(List<GameInterfaceLayer> layers, UserInterface userInterface, UIState state, int index, bool visible, string customName = null) //Code created by Scalie
        {
            string name;
            if (customName == null)
            {
                name = state.ToString();
            }
            else
            {
                name = customName;
            }
            layers.Insert(index, new LegacyGameInterfaceLayer("Redemption: " + name,
                delegate
                {
                    if (visible)
                    {
                        userInterface.Update(Main._drawInterfaceGameTime);
                        state.Draw(Main.spriteBatch);
                    }
                    return true;
                }, InterfaceScaleType.UI));
        }

        public static void DrawMPWarning(SpriteBatch spriteBatch)
        {
            Player player = Main.LocalPlayer;

            string text = (string)Redemption.Instance.GetLocalization("StatusMessage.Other.MPWarning");
            Vector2 CenterPosition = new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f);
            int centerX = (int)CenterPosition.X;
            int centerY = (int)CenterPosition.Y;
            int textLength = (int)FontAssets.MouseText.Value.MeasureString(text).X;
            int textHeight = (int)FontAssets.MouseText.Value.MeasureString(text).Y;
            Vector2 textpos = new(centerX - (textLength / 2f), centerY - (textHeight / 2f));

            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, text, textpos, Colors.RarityRed, 0, Vector2.Zero, Vector2.One);
        }
        public static void DrawSpiritGauge(SpriteBatch spriteBatch)
        {
            Player player = Main.LocalPlayer;
            RitualistPlayer rP = player.GetModPlayer<RitualistPlayer>();

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Texture2D timerBar = ModContent.Request<Texture2D>("Redemption/UI/SpiritGauge").Value;
            Texture2D timerBarInner = ModContent.Request<Texture2D>("Redemption/UI/SpiritGauge_Fill").Value;
            float timerMax = rP.SpiritGaugeMax;
            int timerProgress = (int)(timerBarInner.Width * (rP.SpiritGauge / timerMax));
            Vector2 drawPos = player.Center + new Vector2(0, 32) - Main.screenPosition;
            spriteBatch.Draw(timerBar, drawPos, null, Color.White, 0f, timerBar.Size() / 2f, 1f, SpriteEffects.None, 0f);
            spriteBatch.Draw(timerBarInner, drawPos, new Rectangle?(new Rectangle(0, 0, timerProgress, timerBarInner.Height)), Color.White, 0f, timerBarInner.Size() / 2f, 1f, SpriteEffects.None, 0f);

            Texture2D timerBar2 = ModContent.Request<Texture2D>("Redemption/UI/SpiritGaugeSmall").Value;
            Texture2D timerBarInner2 = ModContent.Request<Texture2D>("Redemption/UI/SpiritGaugeSmall_Fill").Value;
            float timerMax2 = rP.SpiritGaugeCDMax;
            int timerProgress2 = (int)(timerBarInner2.Width * (rP.SpiritGaugeCD / timerMax2));
            Vector2 drawPos2 = player.Center + new Vector2(0, 41) - Main.screenPosition;
            spriteBatch.Draw(timerBar2, drawPos2, null, Color.White, 0f, timerBar2.Size() / 2f, 1f, SpriteEffects.None, 0f);
            spriteBatch.Draw(timerBarInner2, drawPos2, new Rectangle?(new Rectangle(0, 0, timerProgress2, timerBarInner2.Height)), Color.White, 0f, timerBarInner2.Size() / 2f, 1f, SpriteEffects.None, 0f);

            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, (rP.SpiritLevel + 1).ToString(), player.Center + new Vector2(-46, 36) - Main.screenPosition, Color.White, 0, Vector2.Zero, Vector2.One);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.UIScaleMatrix);
        }
        public static void DrawShieldGenGauge(SpriteBatch spriteBatch)
        {
            Player player = Main.LocalPlayer;
            BuffPlayer bP = player.GetModPlayer<BuffPlayer>();

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Texture2D timerBar = Request<Texture2D>("Redemption/UI/ShieldGauge").Value;
            Texture2D timerBarInner = Request<Texture2D>("Redemption/UI/ShieldGauge_Fill").Value;
            Texture2D timerBarInner2 = Request<Texture2D>("Redemption/UI/ShieldGauge_Fill2").Value;
            float timerMax = 200;
            int timerProgress = (int)(timerBarInner.Width * (bP.shieldGeneratorLife / timerMax));
            int timerProgress2 = (int)(timerBarInner.Width * (bP.shieldGeneratorCD / 3600f));
            Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter, true);
            Vector2 drawPos = playerCenter - new Vector2(0, 60) - Main.screenPosition;
            spriteBatch.Draw(timerBar, drawPos, null, Color.White, 0f, timerBar.Size() / 2f, 1f, SpriteEffects.None, 0f);
            if (bP.shieldGeneratorCD <= 0)
                spriteBatch.Draw(timerBarInner, drawPos, new Rectangle?(new Rectangle(0, 0, timerProgress, timerBarInner.Height)), Color.White, 0f, timerBarInner.Size() / 2f, 1f, SpriteEffects.None, 0f);
            spriteBatch.Draw(timerBarInner2, drawPos, new Rectangle?(new Rectangle(0, 0, timerProgress2, timerBarInner.Height)), Color.White * .5f, 0f, timerBarInner.Size() / 2f, 1f, SpriteEffects.None, 0f);

            Texture2D shieldTex = Request<Texture2D>("Redemption/Textures/BubbleShield").Value;
            Texture2D overlay = Request<Texture2D>("Redemption/Textures/BubbleShield_Overlay").Value;
            Vector2 drawOrigin = new(shieldTex.Width / 2, shieldTex.Height / 2);

            if (bP.shieldGeneratorCD <= 0)
            {
                spriteBatch.Draw(overlay, playerCenter - Main.screenPosition, null, RedeColor.FadeColour1 with { A = 0 } * ((float)bP.shieldGeneratorLife / 200) * (bP.shieldGeneratorAlpha + 0.3f), 0, drawOrigin, .5f, 0, 0);
                spriteBatch.Draw(shieldTex, playerCenter - Main.screenPosition, null, Color.White * ((float)bP.shieldGeneratorLife / 200) * (bP.shieldGeneratorAlpha + 0.3f), 0, drawOrigin, .5f, 0, 0);
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.UIScaleMatrix);
        }
        public static void DrawEnergyGauge(SpriteBatch spriteBatch)
        {
            Player player = Main.LocalPlayer;
            EnergyPlayer eP = player.GetModPlayer<EnergyPlayer>();

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Texture2D timerBar = Request<Texture2D>("Redemption/UI/EnergyGauge").Value;
            Texture2D timerBarInner = Request<Texture2D>("Redemption/UI/EnergyGauge_Fill").Value;
            float timerMax = eP.energyMax;
            int timerProgress = (int)(timerBarInner.Height * (eP.statEnergy / timerMax));
            Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter, true);
            Vector2 drawPos = playerCenter + new Vector2(40, 0) - Main.screenPosition;
            spriteBatch.Draw(timerBar, drawPos, null, Color.White * 0.75f, 0f, timerBar.Size() / 2f, 1f, SpriteEffects.None, 0f);
            spriteBatch.Draw(timerBarInner, drawPos, new Rectangle?(new Rectangle(0, 0, timerBarInner.Width, timerProgress)), RedeColor.EnergyPulse * 0.75f, MathHelper.Pi, timerBarInner.Size() / 2f, 1f, SpriteEffects.None, 0f);

            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, ((int)(eP.statEnergy / timerMax * 100)).ToString() + "%", playerCenter + new Vector2(30, -36) - Main.screenPosition, Color.White * 0.75f, 0, Vector2.Zero, Vector2.One * 0.75f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.UIScaleMatrix);
        }
        public static void DrawStunStars(SpriteBatch spriteBatch)
        {
            Player player = Main.LocalPlayer;
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Texture2D starTex = Request<Texture2D>("Redemption/Textures/StunVisual").Value;
            int height = starTex.Height / 4;
            int y = height * player.RedemptionPlayerBuff().stunFrame;
            Vector2 drawOrigin = new(starTex.Width / 2, height / 2);

            spriteBatch.Draw(starTex, player.Center - new Vector2(0, 34) - Main.screenPosition, new Rectangle?(new Rectangle(0, y, starTex.Width, height)), Color.White * ((255 - Main.BlackFadeIn) / 255f), 0, drawOrigin, 1, 0, 0);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.UIScaleMatrix);
        }
        public static void DrawOmegaTransmitterText(SpriteBatch spriteBatch)
        {
            Player player = Main.LocalPlayer;
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, "Right-click to switch Prototype", player.Center + new Vector2(-118, 36) - Main.screenPosition, Color.Red, 0, Vector2.Zero, Vector2.One);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.UIScaleMatrix);
        }
        public static void DrawSlayerCursor(SpriteBatch spriteBatch)
        {
            Texture2D texture = Request<Texture2D>("Redemption/Textures/SlayerCursor").Value;
            Vector2 drawOrigin = new(texture.Width / 2, texture.Height / 2);
            float scale = BaseUtility.MultiLerp(Main.LocalPlayer.miscCounter % 100 / 100f, 1f, 0.9f, 1f);

            spriteBatch.Draw(texture, Main.MouseWorld - Main.screenPosition, null, Color.White, 0, drawOrigin, scale, SpriteEffects.None, 0);
        }
        #region Skele Invasion UI
        public static void DrawSkeletonInvasionUI(SpriteBatch spriteBatch)
        {
            float alpha = .5f;
            Texture2D backGround1 = TextureAssets.ColorBar.Value;
            Texture2D progressColor = TextureAssets.ColorBar.Value;
            Texture2D InvIcon = Request<Texture2D>("Redemption/Items/Armor/Vanity/EpidotrianSkull").Value;
            float scmp = .875f;
            Color descColor = new(77, 39, 135);
            Color waveColor = new(255, 241, 51);
            const int offsetX = 20;
            const int offsetY = 20;
            int width = (int)(200f * scmp);
            int height = (int)(52f * scmp);
            Rectangle waveBackground = Utils.CenteredRectangle(new Vector2(Main.screenWidth - offsetX - 100f, Main.screenHeight - offsetY - 23f), new Vector2(width, height));
            Utils.DrawInvBG(spriteBatch, waveBackground, new Color(63, 65, 151, 255) * 0.785f);
            float cleared = (float)Main.time / 16200;
            string waveText = "Until Party's Over: " + Math.Round(100 * cleared) + "%";
            Utils.DrawBorderString(spriteBatch, waveText, new Vector2(waveBackground.X + waveBackground.Width / 2, waveBackground.Y + 1), Color.White, scmp, 0.5f, -0.1f);
            Rectangle waveProgressBar = Utils.CenteredRectangle(new Vector2(waveBackground.X + waveBackground.Width * 0.5f, waveBackground.Y + waveBackground.Height * 0.75f), new Vector2(progressColor.Width, progressColor.Height));
            Rectangle waveProgressAmount = new(0, 0, (int)(progressColor.Width * MathHelper.Clamp(cleared, 0f, 1f)), progressColor.Height);
            Vector2 offset = new((waveProgressBar.Width - (int)(waveProgressBar.Width * scmp)) * 0.5f, (waveProgressBar.Height - (int)(waveProgressBar.Height * scmp)) * 0.5f);
            spriteBatch.Draw(backGround1, waveProgressBar.Location.ToVector2() + offset, null, Color.White * alpha, 0f, new Vector2(0f), scmp, SpriteEffects.None, 0f);
            spriteBatch.Draw(backGround1, waveProgressBar.Location.ToVector2() + offset, waveProgressAmount, waveColor, 0f, new Vector2(0f), scmp, SpriteEffects.None, 0f);
            const int internalOffset = 6;
            Vector2 descSize = new Vector2(154, 40) * scmp;
            Rectangle barrierBackground = Utils.CenteredRectangle(new Vector2(Main.screenWidth - offsetX - 100f, Main.screenHeight - offsetY - 19f), new Vector2(width, height));
            Rectangle descBackground = Utils.CenteredRectangle(new Vector2(barrierBackground.X + barrierBackground.Width * 0.5f, barrierBackground.Y - internalOffset - descSize.Y * 0.5f), descSize * .8f);
            Utils.DrawInvBG(spriteBatch, descBackground, descColor * alpha);
            int descOffset = (descBackground.Height - 20) / 2;
            Rectangle icon = new(descBackground.X + descOffset + 5, descBackground.Y + descOffset, 18, 20);
            spriteBatch.Draw(InvIcon, icon, Color.White);
            Utils.DrawBorderString(spriteBatch, "Raveyard", new Vector2(barrierBackground.X + barrierBackground.Width * 0.5f, barrierBackground.Y - internalOffset - descSize.Y * 0.5f), Color.White, scmp, 0.4f, 0.4f);
        }
        #endregion
        #region Fowl Morning UI
        public static void DrawFowlMorningUI(SpriteBatch spriteBatch)
        {
            float alpha = .5f;
            Texture2D backGround1 = TextureAssets.ColorBar.Value;
            Texture2D progressColor = TextureAssets.ColorBar.Value;
            Texture2D InvIcon = Request<Texture2D>("Redemption/Gores/Boss/FowlEmperor_Crown").Value;
            float scmp = .875f;
            Color descColor = new(104, 70, 6);
            Color waveColor = new(255, 241, 51);
            const int offsetX = 20;
            const int offsetY = 40;
            int width = (int)(200f * scmp);
            int height = (int)(52f * scmp);
            Rectangle waveBackground = Utils.CenteredRectangle(new Vector2(Main.screenWidth - offsetX - 100f, Main.screenHeight - offsetY - 23f), new Vector2(width, height));
            Utils.DrawInvBG(spriteBatch, waveBackground, new Color(63, 65, 151, 255) * 0.785f);
            float cleared = FowlMorningWorld.ChickPoints / (float)FowlMorningNPC.maxPoints;
            string waveText = "Wave " + (FowlMorningWorld.ChickWave + 1) + ": " + Math.Round(100 * cleared) + "%";
            Utils.DrawBorderString(spriteBatch, waveText, new Vector2(waveBackground.X + waveBackground.Width / 2, waveBackground.Y + 3), Color.White, 1, 0.5f, -0.1f);
            Rectangle waveProgressBar = Utils.CenteredRectangle(new Vector2(waveBackground.X + waveBackground.Width * 0.5f, waveBackground.Y + waveBackground.Height * 0.75f), new Vector2(progressColor.Width, progressColor.Height));
            Rectangle waveProgressAmount = new(0, 0, (int)(progressColor.Width * MathHelper.Clamp(cleared, 0f, 1f)), progressColor.Height);
            Vector2 offset = new((waveProgressBar.Width - (int)(waveProgressBar.Width * scmp)) * 0.5f, (waveProgressBar.Height - (int)(waveProgressBar.Height * scmp)) * 0.5f);
            spriteBatch.Draw(backGround1, waveProgressBar.Location.ToVector2() + offset, null, Color.Black, 0f, new Vector2(0f), scmp, SpriteEffects.None, 0f);
            spriteBatch.Draw(backGround1, waveProgressBar.Location.ToVector2() + offset, waveProgressAmount, waveColor, 0f, new Vector2(0f), scmp, SpriteEffects.None, 0f);
            const int internalOffset = -1;
            Vector2 descSize = new Vector2(188, 50) * scmp;
            Rectangle barrierBackground = Utils.CenteredRectangle(new Vector2(Main.screenWidth - offsetX - 100f, Main.screenHeight - offsetY - 19f), new Vector2(width, height));
            Rectangle descBackground = Utils.CenteredRectangle(new Vector2(barrierBackground.X + barrierBackground.Width * 0.5f, barrierBackground.Y - internalOffset - descSize.Y * 0.5f), descSize * .8f);
            Utils.DrawInvBG(spriteBatch, descBackground, descColor * alpha);
            int descOffset = (descBackground.Height - (int)(32f * scmp)) / 2;
            Rectangle icon = new(descBackground.X + descOffset, descBackground.Y + descOffset, 22, 24);
            spriteBatch.Draw(InvIcon, icon, Color.White);
            Utils.DrawBorderString(spriteBatch, "Fowl Morning", new Vector2(barrierBackground.X + barrierBackground.Width * 0.5f, barrierBackground.Y - internalOffset - descSize.Y * 0.5f), Color.White, scmp, 0.4f, 0.4f);
        }
        #endregion
    }
}
