using BetterDialogue.UI;
using Redemption.UI.Dialect;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;

namespace Redemption.NPCs
{
    public abstract class HangingButtonBase : ChatButton
    {
        public abstract int YOffset { get; }
        public abstract int NPCType { get; }
        public virtual bool RightSide => false;
        public virtual bool ActiveRequirements => true;
        public virtual bool RevealRequirements => true;
        public virtual bool DisableText => false;

        public override double Priority => 200.0;
        public override void ModifyPosition(NPC npc, Player player, ref Vector2 position)
        {
            RedeGlobalButton.HangingButtonPosition(this, npc, player, ref position, YOffset, RightSide);
        }
        public override bool IsActive(NPC npc, Player player) => npc.type == NPCType && ActiveRequirements;

        public virtual string NewText(NPC npc, Player player) => "";
        public override string Text(NPC npc, Player player)
        {
            return RevealRequirements ? (DisableText ? "..." : NewText(npc, player)) : "???";
        }
        public virtual Color? NewColor(NPC npc, Player player) => null;
        public override Color? OverrideColor(NPC npc, Player player)
        {
            return !RevealRequirements || DisableText ? Color.Gray : NewColor(npc, player);
        }
        public virtual void NewOnClick(NPC npc, Player player) { }
        public override void OnClick(NPC npc, Player player)
        {
            if (!RevealRequirements || DisableText)
                return;

            NewOnClick(npc, player);
        }
    }
    public abstract class TalkButtonBase : ChatButton
    {
        protected abstract int YOffset { get; }
        protected abstract bool LeftSide { get; }
        protected abstract string DialogueType { get; }
        protected abstract bool VisibleRequirement { get; }
        protected abstract int NPCType { get; }

        public override double Priority => 200.0;
        public override void ModifyPosition(NPC npc, Player player, ref Vector2 position)
        {
            int textLength = (int)FontAssets.MouseText.Value.MeasureString(ChatButtonLoader.GetText(this, npc, player)).X;
            position.X = (Main.screenWidth / 2) - 150 - (textLength / 2) + (LeftSide ? 0 : 300);
            position.Y += 56 + (46 * YOffset);
        }
        public override string Text(NPC npc, Player player) => VisibleRequirement ? Language.GetTextValue("Mods.Redemption.DialogueBox." + DialogueType) : "???";
        public override bool IsActive(NPC npc, Player player) => npc.type == NPCType && RedeGlobalButton.talkActive;

        public override Color? OverrideColor(NPC npc, Player player) => VisibleRequirement ? null : Color.Gray;
        public override void OnClick(NPC npc, Player player)
        {
            if (!VisibleRequirement)
                return;
            OnSafeClick(npc, player);
            SoundEngine.PlaySound(SoundID.Chat);
            Main.npcChatText = Language.GetTextValue("Mods.Redemption.Dialogue." + DialogueType);
        }
        public virtual void OnSafeClick(NPC npc, Player player) { }
    }
}