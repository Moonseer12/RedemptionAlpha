using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Redemption.Base;
using Redemption.BaseExtension;
using Redemption.Buffs.Debuffs;
using Redemption.Globals;
using Redemption.Items.Accessories.HM;
using Redemption.Items.Placeable.Furniture.Misc;
using ReLogic.Content;
using System.Net.Security;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace Redemption.Biomes
{
    public class WastelandPurityBiome : ModBiome
    {
        public override int BiomeTorchItemType => ModContent.ItemType<WastelandTorch>();
        public override int BiomeCampfireItemType => ModContent.ItemType<WastelandCampfire>();
        public override ModWaterStyle WaterStyle => ModContent.Find<ModWaterStyle>("Redemption/WastelandWaterStyle");

        public override ModUndergroundBackgroundStyle UndergroundBackgroundStyle => ModContent.Find<ModUndergroundBackgroundStyle>("Redemption/WastelandUndergroundBackgroundStyle");
        public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.Find<ModSurfaceBackgroundStyle>("Redemption/WastelandSurfaceBackgroundStyle");

        public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/Music/Wasteland");

        public override string BestiaryIcon => "Redemption/Textures/Bestiary/Wasteland";
        public override string BackgroundPath => "Redemption/Textures/MapBackgrounds/PurityWastelandMap1";
        public override Color? BackgroundColor => base.BackgroundColor;
        public override string MapBackground => BackgroundPath;
        public override void SpecialVisuals(Player player, bool isActive)
        {
            bool fogSafe = BasePlayer.HasAccessory(player, ModContent.ItemType<GasMask>(), true, false) ||
                player.RedemptionRad().protectionLevel >= 2;
            if (isActive)
            {
                if (!player.InModBiome<WastelandCorruptionBiome>() && !player.InModBiome<WastelandCrimsonBiome>())
                    SkyManager.Instance.Activate("MoR:WastelandSky");
                Filters.Scene["MoR:FogOverlay"]?.GetShader().UseOpacity(fogSafe ? 0.25f : 0.3f).UseIntensity(fogSafe ? 0.6f : 1f)
                .UseColor(Color.DarkOliveGreen).UseImage(ModContent.Request<Texture2D>("Redemption/Effects/Perlin", AssetRequestMode.ImmediateLoad).Value);
                player.ManageSpecialBiomeVisuals("MoR:FogOverlay", isActive);
            }
            else
                SkyManager.Instance.Deactivate("MoR:WastelandSky");
            player.ManageSpecialBiomeVisuals("MoR:WastelandSky", isActive, player.Center);
        }
        public override void OnInBiome(Player player)
        {
            if (Main.raining)
            {
                if ((player.ZoneOverworldHeight || player.ZoneSkyHeight) && player.RedemptionRad().protectionLevel is 0)
                    player.AddBuff(ModContent.BuffType<HeavyRadiationDebuff>(), 30);
                else
                    player.AddBuff(ModContent.BuffType<RadioactiveFalloutDebuff>(), 30);

                player.RedemptionRad().Irradiate(.001f, 0, 1, 1, 500);
            }
            else
                player.AddBuff(ModContent.BuffType<RadioactiveFalloutDebuff>(), 30);
        }

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Wasteland");
        }

        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeMedium;

        public override bool IsBiomeActive(Player player)
        {
            return ModContent.GetInstance<RedeTileCount>().WastelandTileCount >= 200;
        }
    }
    public class WastelandSnowBiome : ModBiome
    {
        public override ModWaterStyle WaterStyle => ModContent.Find<ModWaterStyle>("Redemption/WastelandWaterStyle");

        public override ModUndergroundBackgroundStyle UndergroundBackgroundStyle => ModContent.Find<ModUndergroundBackgroundStyle>("Redemption/WastelandSnowUndergroundBackgroundStyle");
        public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.Find<ModSurfaceBackgroundStyle>("Redemption/WastelandSnowBackgroundStyle");

        public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/Music/Wasteland");

        public override string BestiaryIcon => "Redemption/Textures/Bestiary/WastelandSnow";
        public override string BackgroundPath => "Redemption/Textures/MapBackgrounds/SnowWastelandMap1";
        public override Color? BackgroundColor => base.BackgroundColor;
        public override string MapBackground => BackgroundPath;

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Snow Wasteland");
        }
        public override void SpecialVisuals(Player player, bool isActive)
        {
            if (isActive)
                SkyManager.Instance.Activate("MoR:WastelandSnowSky");
            else
                SkyManager.Instance.Deactivate("MoR:WastelandSnowSky");
        }
        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

        public override bool IsBiomeActive(Player player)
        {
            return ModContent.GetInstance<RedeTileCount>().WastelandSnowTileCount >= 200;
        }
    }
    public class WastelandDesertBiome : ModBiome
    {
        public override ModWaterStyle WaterStyle => ModContent.Find<ModWaterStyle>("Redemption/WastelandWaterStyle");

        public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.Find<ModSurfaceBackgroundStyle>("Redemption/WastelandDesertBackgroundStyle");

        public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/Music/Wasteland");

        public override string BestiaryIcon => "Redemption/Textures/Bestiary/WastelandDesert";
        public override string BackgroundPath => "Redemption/Textures/MapBackgrounds/DesertWastelandMap1";
        public override Color? BackgroundColor => base.BackgroundColor;
        public override string MapBackground => BackgroundPath;

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Desert Wasteland");
        }

        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

        public override bool IsBiomeActive(Player player)
        {
            return ModContent.GetInstance<RedeTileCount>().WastelandDesertTileCount >= 300;
        }
    }
    public class WastelandCorruptionBiome : ModBiome
    {
        public override ModWaterStyle WaterStyle => ModContent.Find<ModWaterStyle>("Redemption/WastelandWaterStyle");

        public override ModUndergroundBackgroundStyle UndergroundBackgroundStyle => ModContent.Find<ModUndergroundBackgroundStyle>("Redemption/WastelandCorruptUndergroundBackgroundStyle");
        public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.Find<ModSurfaceBackgroundStyle>("Redemption/WastelandCorruptBackgroundStyle");

        public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/Music/Wasteland");

        public override string BestiaryIcon => "Redemption/Textures/Bestiary/WastelandCorrupt";
        public override string BackgroundPath => "Redemption/Textures/MapBackgrounds/CorruptionWastelandMap1";
        public override Color? BackgroundColor => base.BackgroundColor;
        public override string MapBackground => BackgroundPath;

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Corrupt Wasteland");
        }
        public override void SpecialVisuals(Player player, bool isActive)
        {
            if (isActive)
                SkyManager.Instance.Activate("MoR:WastelandCorruptSky");
            else
                SkyManager.Instance.Deactivate("MoR:WastelandCorruptSky");
        }
        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

        public override bool IsBiomeActive(Player player)
        {
            return ModContent.GetInstance<RedeTileCount>().WastelandCorruptTileCount >= 200;
        }
    }
    public class WastelandCrimsonBiome : ModBiome
    {
        public override ModWaterStyle WaterStyle => ModContent.Find<ModWaterStyle>("Redemption/WastelandWaterStyle");

        public override ModUndergroundBackgroundStyle UndergroundBackgroundStyle => ModContent.Find<ModUndergroundBackgroundStyle>("Redemption/WastelandCrimsonUndergroundBackgroundStyle");
        public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.Find<ModSurfaceBackgroundStyle>("Redemption/WastelandCrimsonBackgroundStyle");

        public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/Music/Wasteland");

        public override string BestiaryIcon => "Redemption/Textures/Bestiary/WastelandCrimson";
        public override string BackgroundPath => "Redemption/Textures/MapBackgrounds/CrimsonWastelandMap1";
        public override Color? BackgroundColor => base.BackgroundColor;
        public override string MapBackground => BackgroundPath;

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Crimson Wasteland");
        }
        public override void SpecialVisuals(Player player, bool isActive)
        {
            if (isActive)
                SkyManager.Instance.Activate("MoR:WastelandCrimsonSky");
            else
                SkyManager.Instance.Deactivate("MoR:WastelandCrimsonSky");
        }
        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

        public override bool IsBiomeActive(Player player)
        {
            return ModContent.GetInstance<RedeTileCount>().WastelandCrimsonTileCount >= 200;
        }
    }
}
