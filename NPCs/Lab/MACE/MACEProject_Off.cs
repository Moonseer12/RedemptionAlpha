using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using Terraria.GameContent;

namespace Redemption.NPCs.Lab.MACE
{
    public class MACEProject_Off : ModNPC
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("MACE Project");
            NPCID.Sets.DontDoHardmodeScaling[Type] = true;

            NPCID.Sets.NPCBestiaryDrawModifiers value = new(0)
            {
                Hide = true
            };

            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
        }
        public override void SetDefaults()
        {
            NPC.width = 118;
            NPC.height = 140;
            NPC.damage = 0;
            NPC.lifeMax = 250;
            NPC.knockBackResist = 0;
            NPC.aiStyle = -1;
            NPC.noGravity = true;
            NPC.noTileCollide = false;
            NPC.dontTakeDamage = true;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[NPC.type].Value;
            Texture2D trolleyTex = ModContent.Request<Texture2D>("Redemption/NPCs/Lab/CraneTrolley").Value;
            Texture2D jawTex = ModContent.Request<Texture2D>("Redemption/NPCs/Lab/MACEProjectJaw_Off").Value;

            Vector2 drawCenterTrolley = new(NPC.Center.X, NPC.Center.Y + 10);
            Rectangle rect = new(0, 0, trolleyTex.Width, trolleyTex.Height);
            Main.spriteBatch.Draw(trolleyTex, drawCenterTrolley - screenPos, new Rectangle?(rect), drawColor, NPC.rotation, new Vector2(trolleyTex.Width / 2f, trolleyTex.Height / 2f), NPC.scale, SpriteEffects.None, 0);

            Vector2 drawCenterJaw = new(NPC.Center.X - 1, NPC.Center.Y + 60);
            Rectangle rect2 = new(0, 0, jawTex.Width, jawTex.Height);
            Main.spriteBatch.Draw(jawTex, drawCenterJaw - screenPos, new Rectangle?(rect2), drawColor, NPC.rotation, new Vector2(jawTex.Width / 2f, jawTex.Height / 2f), NPC.scale, SpriteEffects.None, 0);

            spriteBatch.Draw(texture, NPC.Center - screenPos, NPC.frame, drawColor, NPC.rotation, NPC.frame.Size() / 2, NPC.scale, SpriteEffects.None, 0);
            return false;
        }
        public override bool CanHitPlayer(Player target, ref int cooldownSlot) => false;
    }
}