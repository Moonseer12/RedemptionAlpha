using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace Redemption
{
    public enum ElementDamageTooltipEnum
    {
        Disabled,
        ShowAdded,
        ShowTotalled
    }
    public class RedeConfigClient : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        public static RedeConfigClient Instance;

        [DefaultValue(ElementDamageTooltipEnum.ShowTotalled)]
        [DrawTicks]
        public ElementDamageTooltipEnum ElementalDamageTooltipStyle;

        //[Label("Disable Zelda-Styled Boss Titles")]
        //[Tooltip("Disables the Legend of Zelda-style boss introduction text for bosses.")]
        public bool NoBossIntroText;

        //[Label("Disable Camera Lock")]
        //[Tooltip("Disables the locked camera during cutscenes, along with the invincibility and vignette effect\n" +
        //    "Not recommended, at least on a first playthrough, as it can ruin the feel of things.")]
        public bool CameraLockDisable;

        [Range(0f, 1f)]
        [DefaultValue(1f)]
        [Slider]
        //[Label("Screen Shake Intensity")]
        //[Tooltip("Reduce to decrease the intensity of screen shaking effects, 0 will disable it entirely")]
        public float ShakeIntensity;

        public bool NoFlyBuzz;

        public bool DialogueInChat;

        public bool LegacyNebSky;

        public bool DisableMPWarning;
    }
    public class RedeConfigServer : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        public static RedeConfigServer Instance;

        [Range(0, 1.5f)]
        [DefaultValue(0)]
        [Slider]
        //[Label("Extra Dialogue Wait Time")]
        //[Tooltip("Increases duration of dialogue text persisting before moving on, increase if you're a slow reader\n" +
        //    "Value is in seconds.")]
        public float DialogueWaitTime;

        [Range(-0.05f, 0.05f)]
        [DefaultValue(0)]
        [Slider]
        [DrawTicks]
        //[Label("Dialogue Text Speed")]
        //[Tooltip("Only recommend changing if the dialogue speed is too slow or fast normally.\n" +
        //    "This is how much less time measured in seconds for each letter to appear, higher = faster.\n" +
        //    "The faster you make it, the harder it is to read before finishing, so also adjust Extra Dialogue Wait Time")]
        public float DialogueSpeed;

        //[Label("Disable Elements")]
        //[Tooltip("Disables elemental resistances and damage")]
        public bool ElementDisable;

        //[Label("Disable Guard Points on Vanilla Enemies")]
        //[Tooltip("Disables Guard Points on certain vanilla enemies")]
        public bool VanillaGuardPointsDisable;

        //[Label("No Patient Zero Build-Up")]
        //[Tooltip("Makes the boss begin the fight instantly, mainly for no-hitters. (This will cause the boss's pulse effect to not sync well with the music)")]
        public bool NoPZBuildUp;

        public bool FindNewStructureCoords;

        [Header("$Mods.Redemption.Configs.RedeConfigServer.FunniHeader")]
        //[Label("Oops! All spider!")]
        //[Tooltip("Generating a new world will cause all stone blocks to become infested stone")]
        public bool FunniSpiders;

        //[Label("Alright, who set off the nuke?")]
        //[Tooltip("Generating a new world will have a Wasteland biome naturally generate (This can break progression, obviously)")]
        public bool FunniWasteland;

        //[Label("Liden at home:")]
        //[Tooltip("Generating a new world will convert every tile into it's irradiated version (This can break progression, obviously)")]
        public bool FunniAllWasteland;

        //[Label("The Janitor's Last Stand")]
        //[Tooltip("Generating a new world will create a clean lab with safe laboratory tiles and no sludge (This can break progression, obviously)")]
        public bool FunniJanitor;

        //[Label("Ancient World")]
        //[Tooltip("Generating a new world will convert tiles to their ancient or gathic version, and gathic tombs will gen more frequently")]
        public bool FunniAncient;
    }
}