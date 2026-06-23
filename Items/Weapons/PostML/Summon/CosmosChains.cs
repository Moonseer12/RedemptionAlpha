using Microsoft.Xna.Framework.Graphics;
using Redemption.BaseExtension;
using Redemption.Buffs.NPCBuffs;
using Redemption.Globals;
using Redemption.Items.Materials.PostML;
using Redemption.Projectiles.Minions;
using Redemption.Rarities;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Redemption.Items.Weapons.PostML.Summon
{
    public class CosmosChains : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 30;
            Item.DefaultToWhip(ProjectileType<CosmosChains_Proj>(), 220, 6, 6, 28);
            Item.shootSpeed = 6;
            Item.rare = RarityType<CosmicRarity>();
            Item.channel = true;
            Item.value = Item.buyPrice(1, 0, 0, 0);
        }
        public override bool MeleePrefix() => true;
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.RainbowWhip)
                .AddIngredient<LifeFragment>(7)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    public class CosmosChains_Proj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Chains of the Cosmos");
            ProjectileID.Sets.IsAWhip[Type] = true;
            ElementID.ProjCelestial[Type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.DefaultToWhip();

            Projectile.WhipSettings.Segments = 32;
            Projectile.WhipSettings.RangeMultiplier = 2f;
            Projectile.Redemption().TechnicallyMelee = true;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];
            if (player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[ProjectileType<ChainsCosmicEye>()] < 4)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), target.Center + RedeHelper.PolarVector(Main.rand.Next(150, 351), RedeHelper.RandomRotation()), Vector2.Zero, ProjectileType<ChainsCosmicEye>(), (int)(Projectile.damage * 0.5f), Projectile.knockBack, player.whoAmI, target.whoAmI);
            }
            player.MinionAttackTargetNPC = target.whoAmI;
            target.AddBuff(BuffType<CosmosChainsDebuff>(), 180);
        }
        private int soundTimer;
        public override void PostAI()
        {
            if (soundTimer++ == 18)
                SoundEngine.PlaySound(SoundID.Item125 with { Volume = .5f }, Projectile.position);
        }
        private static void DrawLine(List<Vector2> list)
        {
            Texture2D texture = TextureAssets.FishingLine.Value;
            Rectangle frame = texture.Frame();
            Vector2 origin = new(frame.Width / 2, 2);

            Vector2 pos = list[0];
            for (int i = 0; i < list.Count - 1; i++)
            {
                Vector2 element = list[i];
                Vector2 diff = list[i + 1] - element;

                float rotation = diff.ToRotation() - MathHelper.PiOver2;
                Color color = Lighting.GetColor(element.ToTileCoordinates(), new(Main.DiscoR, Main.DiscoG, Main.DiscoB, 0));
                Vector2 scale = new(1, (diff.Length() + 2) / frame.Height);

                Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, SpriteEffects.None, 0);

                pos += diff;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            List<Vector2> list = new();
            Projectile.FillWhipControlPoints(Projectile, list);

            DrawLine(list);

            SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Main.instance.LoadProjectile(Type);
            Texture2D texture = TextureAssets.Projectile[Type].Value;

            Vector2 pos = list[0];

            for (int i = 0; i < list.Count - 1; i++)
            {
                Rectangle frame = new(0, 0, 16, 26);
                Vector2 origin = new(8, 8);
                float scale = 1;

                if (i == list.Count - 2)
                {
                    frame.Y = 118;
                    frame.Height = 20;

                    #region Dusts
                    // For a more impactful look, this scales the tip of the whip up when fully extended, and down when curled up.
                    Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out int _, out float _);
                    float t = soundTimer / timeToFlyOut;
                    scale = MathHelper.Lerp(0.4f, 1.3f, Utils.GetLerpValue(0.1f, 0.7f, t, true) * Utils.GetLerpValue(0.9f, 0.7f, t, true));

                    float dustChance = Utils.GetLerpValue(0.1f, 0.7f, t, clamped: true) * Utils.GetLerpValue(0.9f, 0.7f, t, clamped: true);

                    // Spawn dust
                    if (dustChance > 0.5f && Main.rand.NextFloat() < dustChance * 0.7f)
                    {
                        Vector2 outwardsVector = list[^2].DirectionTo(list[^1]).SafeNormalize(Vector2.Zero);
                        Dust dust = Dust.NewDustDirect(list[^1] - texture.Size() / 2, texture.Width, texture.Height, DustID.AncientLight, 0f, 0f, 100, default, Main.rand.NextFloat(1f, 1.5f));

                        dust.noGravity = true;
                        dust.velocity *= Main.rand.NextFloat() * 0.8f;
                        dust.velocity += outwardsVector * 0.8f;
                        dust.shader = GameShaders.Armor.GetSecondaryShader(77, Main.player[Projectile.owner]);

                    }
                    #endregion
                }
                else if (i > 10)
                {
                    frame.Y = 90;
                    frame.Height = 20;
                }
                else if (i > 5)
                {
                    frame.Y = 62;
                    frame.Height = 20;
                }
                else if (i > 0)
                {
                    frame.Y = 34;
                    frame.Height = 20;
                }

                Vector2 element = list[i];
                Vector2 diff = list[i + 1] - element;

                float rotation = diff.ToRotation() - MathHelper.PiOver2;
                Color color = new(Main.DiscoR, Main.DiscoG, Main.DiscoB, 160);

                Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, flip, 0);

                pos += diff;
            }
            return false;
        }
    }
}
