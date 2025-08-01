using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Redemption.BaseExtension;
using Terraria.DataStructures;
using System.Collections.Generic;
using Terraria.ID;
using System.Linq;
using Terraria.Enums;
using Redemption.NPCs.Minibosses.Calavia;
using Terraria.Audio;
using Redemption.Items.Weapons.PreHM.Melee;
using Redemption.NPCs.Friendly.SpiritSummons;
using Redemption.NPCs.Critters;
using Redemption.Helpers;
using Terraria.Localization;

namespace Redemption.Globals
{
    public class RedeProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public bool TechnicallyMelee;
        public bool IsHammer;
        public bool IsAxe;
        public bool IsSpear;
        public bool RitDagger;
        public bool EnergyBased;
        public bool ParryBlacklist;
        public bool friendlyHostile;
        public int DissolveTimer;
        public float ReflectDamageIncrease;
        public Rectangle swordHitbox;
        public override void SetDefaults(Projectile projectile)
        {
            if (ProjectileLists.IsTechnicallyMelee.Contains(projectile.type))
                TechnicallyMelee = true;
        }
        public override void ModifyHitNPC(Projectile projectile, Terraria.NPC target, ref Terraria.NPC.HitModifiers modifiers)
        {
            if (ReflectDamageIncrease is 0)
                return;
            modifiers.FinalDamage *= ReflectDamageIncrease;
        }
        public override void AI(Projectile projectile)
        {
            if (ArenaSystem.ArenaActive && projectile.aiStyle == 7 && !projectile.Hitbox.Intersects(ArenaSystem.ArenaBoundsWorld))
                projectile.Kill();
        }
        public static bool Decapitation(Terraria.NPC target, ref int damage, ref bool crit, int chance = 200)
        {
            bool humanoid = NPCLists.SkeletonHumanoid.Contains(target.type) || NPCLists.Humanoid.Contains(target.type);
            if (target.life < target.lifeMax && target.life < damage * 100 && humanoid)
            {
                if (Main.rand.NextBool(chance))
                {
                    DecapitationEffect(target, ref crit);
                    return true;
                }
            }
            return false;
        }
        public static void DecapitationEffect(Terraria.NPC target, ref bool crit)
        {
            RedeDraw.SpawnExplosion(new Vector2(target.Center.X, target.position.Y + target.height / 4), Color.Orange, shakeAmount: 0, scale: .5f, noDust: true, rot: Main.rand.NextFloat(-0.1f, 0.1f), tex: "Redemption/Textures/SwordClash");

            if (!Main.dedServ)
                SoundEngine.PlaySound(CustomSounds.Slash2.WithPitchOffset(.3f), target.position);

            CombatText.NewText(target.getRect(), Color.Orange, Language.GetTextValue("Mods.Redemption.StatusMessage.Other.Decapitated"));
            target.Redemption().decapitated = true;
            crit = true;
            target.StrikeInstantKill();

            RedeQuest.SetBonusDiscovered(RedeQuest.Bonuses.Slash, false);
            RedeQuest.SetBonusDiscovered(RedeQuest.Bonuses.Axe);
        }
        public static void DecapitationEffect(Terraria.NPC target, ref Terraria.NPC.HitModifiers modifiers)
        {
            RedeDraw.SpawnExplosion(new Vector2(target.Center.X, target.position.Y + target.height / 4), Color.Orange, shakeAmount: 0, scale: .5f, noDust: true, rot: Main.rand.NextFloat(-0.1f, 0.1f), tex: "Redemption/Textures/SwordClash");

            if (!Main.dedServ)
                SoundEngine.PlaySound(CustomSounds.Slash2.WithPitchOffset(.3f), target.position);

            CombatText.NewText(target.getRect(), Color.Orange, Language.GetTextValue("Mods.Redemption.StatusMessage.Other.Decapitated"));
            target.Redemption().decapitated = true;
            modifiers.SetInstantKill();
            modifiers.SetCrit();

            RedeQuest.SetBonusDiscovered(RedeQuest.Bonuses.Slash, false);
            RedeQuest.SetBonusDiscovered(RedeQuest.Bonuses.Axe);
        }

        public static bool SwordClashFriendly(Projectile projectile, Projectile target, Entity player, ref bool parried, int frame = 5)
        {
            Rectangle targetHitbox = target.Hitbox;
            if (target.Redemption().swordHitbox != default)
                targetHitbox = target.Redemption().swordHitbox;

            if (projectile.frame == frame && !parried && projectile.Redemption().swordHitbox.Intersects(targetHitbox) && target.type == ModContent.ProjectileType<Calavia_BladeOfTheMountain>() && target.frame >= 4 && target.frame <= 5)
            {
                if (player is Terraria.Player p)
                {
                    p.immune = true;
                    p.immuneTime = 60;
                    p.AddBuff(BuffID.ParryDamageBuff, 120);
                }
                player.velocity.X += 4 * player.RightOfDir(target);
                RedeDraw.SpawnExplosion(RedeHelper.CenterPoint(projectile.Center, target.Center), Color.White, shakeAmount: 0, scale: 1f, noDust: true, rot: Main.rand.NextFloat(MathHelper.PiOver4, 3 * MathHelper.PiOver4), tex: "Redemption/Textures/SwordClash");
                SoundEngine.PlaySound(CustomSounds.SwordClash, projectile.position);
                DustHelper.DrawCircle(RedeHelper.CenterPoint(projectile.Center, target.Center), DustID.SilverCoin, 1, 4, 4, nogravity: true);
                parried = true;

                RedeQuest.SetBonusDiscovered(RedeQuest.Bonuses.Clash);
                return true;
            }
            return false;
        }
        public static bool SwordClashHostile(Projectile projectile, Projectile target, Terraria.NPC npc, ref bool parried)
        {
            Rectangle targetHitbox = target.Hitbox;
            if (target.Redemption().swordHitbox != default)
                targetHitbox = target.Redemption().swordHitbox;

            if (!parried && projectile.Redemption().swordHitbox.Intersects(targetHitbox) &&
                ((target.type == ModContent.ProjectileType<Zweihander_SlashProj>() && target.frame is 4 or 3) ||
                ((target.type == ModContent.ProjectileType<BladeOfTheMountain_Slash>() ||
                target.type == ModContent.ProjectileType<Calavia_SS_BladeOfTheMountain>() ||
                target.type == ModContent.ProjectileType<SwordSlicer_Slash>()) && target.frame is 5 or 4) ||
                (target.type == ModContent.ProjectileType<KeepersClaw_Slash>() && target.frame is 2)))
            {
                npc.velocity.X += 4 * npc.RightOfDir(target);
                SoundEngine.PlaySound(CustomSounds.SwordClash, projectile.position);
                RedeDraw.SpawnExplosion(RedeHelper.CenterPoint(projectile.Center, target.Center), Color.White, shakeAmount: 0, scale: 1f, noDust: true, rot: Main.rand.NextFloat(MathHelper.PiOver4, 3 * MathHelper.PiOver4), tex: "Redemption/Textures/SwordClash");
                DustHelper.DrawCircle(RedeHelper.CenterPoint(projectile.Center, target.Center), DustID.SilverCoin, 1, 4, 4, nogravity: true);
                parried = true;
                return true;
            }
            return false;
        }
        public static void HoldOutProjBasics(Projectile proj, Terraria.Player player, Vector2 vector, Vector2 target = default)
        {
            if (target == default)
                target = Main.MouseWorld;
            if (Main.myPlayer == proj.owner)
            {
                float scaleFactor6 = 1f;
                if (player.inventory[player.selectedItem].shoot == proj.type)
                    scaleFactor6 = player.inventory[player.selectedItem].shootSpeed * proj.scale;
                Vector2 vector13 = target - vector;
                vector13.Normalize();
                if (vector13.HasNaNs())
                    vector13 = Vector2.UnitX * player.direction;
                vector13 *= scaleFactor6;
                if (vector13.X != proj.velocity.X || vector13.Y != proj.velocity.Y)
                    proj.netUpdate = true;

                proj.velocity = vector13;
                if (player.noItems || player.CCed || player.dead || !player.active)
                    proj.Kill();
                proj.netUpdate = true;
            }
        }
        public static void HoldOutProj_SlowTurn(Projectile proj, Terraria.Player player, Vector2 vector, float responsiveness)
        {
            if (Main.myPlayer == proj.owner)
            {
                float scaleFactor6 = 1f;
                if (player.inventory[player.selectedItem].shoot == proj.type)
                    scaleFactor6 = player.inventory[player.selectedItem].shootSpeed * proj.scale;

                Vector2 targetVector = Main.MouseWorld - vector;
                targetVector.Normalize();

                if (targetVector.HasNaNs())
                    targetVector = Vector2.UnitX * player.direction;

                if (targetVector.X != proj.velocity.X || targetVector.Y != proj.velocity.Y)
                    proj.netUpdate = true;

                proj.velocity = Vector2.Normalize(Vector2.Lerp(proj.velocity, targetVector, responsiveness));
                proj.velocity *= scaleFactor6;

                if (player.noItems || player.CCed || player.dead || !player.active)
                    proj.Kill();

                proj.netUpdate = true;
            }
        }

        public static Dictionary<int, (Entity entity, IEntitySource source)> projOwners = new();
        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Entity attacker = null;
            if (source is EntitySource_ItemUse item && projectile.friendly && !projectile.hostile)
                attacker = item.Entity;
            else if (source is EntitySource_Buff buff && projectile.friendly && !projectile.hostile)
                attacker = buff.Entity;
            else if (source is EntitySource_ItemUse_WithAmmo itemAmmo && projectile.friendly && !projectile.hostile)
                attacker = itemAmmo.Entity;
            else if (source is EntitySource_Mount mount && projectile.friendly && !projectile.hostile)
                attacker = mount.Entity;
            else if (source is EntitySource_Parent parent)
            {
                if (parent.Entity is Projectile proj)
                    attacker = Main.player[proj.owner];
                else
                    attacker = parent.Entity;
            }
            if (attacker != null)
            {
                if (projOwners.ContainsKey(projectile.whoAmI))
                    projOwners.Remove(projectile.whoAmI);
                projOwners.Add(projectile.whoAmI, (attacker, source));
            }
        }
        #region Wasteland Conversion
        public override void PostAI(Projectile projectile)
        {
            if (projectile.type is ProjectileID.VilePowder or ProjectileID.ViciousPowder)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    Terraria.NPC npc = Main.npc[i];
                    if (!npc.active || npc.ModNPC == null || npc.ModNPC is not Chicken || !projectile.Hitbox.Intersects(npc.Hitbox))
                        continue;

                    if (projectile.type is ProjectileID.VilePowder)
                        npc.Transform(ModContent.NPCType<CorruptChicken>());
                    else
                        npc.Transform(ModContent.NPCType<ViciousChicken>());
                }
            }
            if ((projectile.type != 145 && projectile.type != 147 && projectile.type != 149 && projectile.type != 146) || projectile.owner != Main.myPlayer)
                return;
            int x = (int)(projectile.Center.X / 16f);
            int y = (int)(projectile.Center.Y / 16f);

            Tile tile = Main.tile[x, y];
            int type = tile.TileType;
            int wallType = tile.WallType;

            for (int i = x - 1; i <= x + 1; i++)
            {
                for (int j = y - 1; j <= y + 1; j++)
                {
                    if (!WorldGen.InWorld(x, y, 1))
                        return;

                    if (projectile.type == 145 || projectile.type == 10)
                    {
                        if (Main.tile[x, y] != null)
                        {
                            if (type == ModContent.TileType<Tiles.Tiles.IrradiatedDirtTile>())
                                tile.TileType = 0;
                            else if (type == ModContent.TileType<Tiles.Tiles.IrradiatedSnowTile>())
                                tile.TileType = 147;
                            else if (type == ModContent.TileType<Tiles.Tiles.IrradiatedLivingWoodTile>())
                                tile.TileType = 191;
                        }
                        if (Main.tile[x, y] != null)
                        {
                            if (wallType == ModContent.WallType<Walls.IrradiatedDirtWallTile>())
                                Main.tile[x, y].WallType = 2;
                        }
                    }
                    if (projectile.type == 147)
                    {
                        if (Main.tile[x, y] != null)
                        {
                            if (type == ModContent.TileType<Tiles.Tiles.IrradiatedDirtTile>())
                                tile.TileType = 0;
                            else if (type == ModContent.TileType<Tiles.Tiles.IrradiatedSnowTile>())
                                tile.TileType = 147;
                            else if (type == ModContent.TileType<Tiles.Tiles.IrradiatedLivingWoodTile>())
                                tile.TileType = 191;
                        }
                        if (Main.tile[x, y] != null)
                        {
                            if (wallType == ModContent.WallType<Walls.IrradiatedDirtWallTile>())
                                Main.tile[x, y].WallType = 2;
                        }
                    }
                    if (projectile.type == 149)
                    {
                        if (Main.tile[x, y] != null)
                        {
                            if (type == ModContent.TileType<Tiles.Tiles.IrradiatedDirtTile>())
                                tile.TileType = 0;
                            else if (type == ModContent.TileType<Tiles.Tiles.IrradiatedSnowTile>())
                                tile.TileType = 147;
                            else if (type == ModContent.TileType<Tiles.Tiles.IrradiatedLivingWoodTile>())
                                tile.TileType = 191;
                        }
                        if (Main.tile[x, y] != null)
                        {
                            if (wallType == ModContent.WallType<Walls.IrradiatedDirtWallTile>())
                                Main.tile[x, y].WallType = 2;
                        }
                    }
                    if (projectile.type == 146)
                    {
                        if (Main.tile[x, y] != null)
                        {
                            if (type == ModContent.TileType<Tiles.Tiles.IrradiatedDirtTile>())
                                tile.TileType = 0;
                            else if (type == ModContent.TileType<Tiles.Tiles.IrradiatedSnowTile>())
                                tile.TileType = 147;
                            else if (type == ModContent.TileType<Tiles.Tiles.IrradiatedLivingWoodTile>())
                                tile.TileType = 191;
                        }
                        if (Main.tile[x, y] != null)
                        {
                            if (wallType == ModContent.WallType<Walls.IrradiatedDirtWallTile>())
                                Main.tile[x, y].WallType = 2;
                        }
                    }
                    NetMessage.SendTileSquare(-1, i, j, 1, 1);
                }
            }
            #endregion
        }
    }
    public abstract class TrueMeleeProjectile : ModProjectile
    {
        public float SetSwingSpeed(float speed)
        {
            Terraria.Player player = Main.player[Projectile.owner];
            return speed / player.GetAttackSpeed(DamageClass.Melee);
        }

        public virtual void SetSafeDefaults() { }

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.ownerHitCheck = true;
            Projectile.Redemption().TechnicallyMelee = true;
            SetSafeDefaults();
        }
    }
    public abstract class LaserProjectile : ModProjectile
    {
        public float AITimer
        {
            get => Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }
        public float Frame
        {
            get => Projectile.localAI[1];
            set => Projectile.localAI[1] = value;
        }
        public float LaserLength = 0;
        public float LaserScale = 0;
        public int LaserSegmentLength = 10;
        public int LaserWidth = 20;
        public int LaserEndSegmentLength = 22;

        public Vector2 endPoint;
        public bool NewCollision;

        public const float FirstSegmentDrawDist = 7;

        public int MaxLaserLength = 2000;
        public int maxLaserFrames = 1;
        public int LaserFrameDelay = 5;
        public bool StopsOnTiles = true;

        public virtual void SetSafeStaticDefaults() { }

        public override void SetStaticDefaults()
        {
            SetSafeStaticDefaults();
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 2400;
        }

        public virtual void SetSafeDefaults() { }

        public override void SetDefaults()
        {
            Projectile.width = LaserWidth;
            Projectile.height = LaserWidth;
            Projectile.Redemption().ParryBlacklist = true;
            SetSafeDefaults();
        }
        public virtual void EndpointTileCollision()
        {
            for (LaserLength = FirstSegmentDrawDist; LaserLength < MaxLaserLength; LaserLength += LaserSegmentLength)
            {
                Vector2 start = Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation) * LaserLength;
                if (!Collision.CanHitLine(Projectile.Center, 1, 1, start, 1, 1))
                {
                    LaserLength -= LaserSegmentLength;
                    break;
                }
            }
        }
        public void CheckHits()
        {
            Terraria.Player player = Main.player[Projectile.owner];
            // done manually for clients that aren't the Projectile owner since onhit methods are clientside
            foreach (Terraria.NPC NPC in Main.npc.Where(n => n.active &&
                 !n.dontTakeDamage &&
                 !n.townNPC &&
                 n.immune[player.whoAmI] <= 0 &&
                 Colliding(new Rectangle(), n.Hitbox) == true))
            {
                OnHitNPC(NPC, new Terraria.NPC.HitInfo() { Damage = 0 }, 0);
            }
        }
        public override void CutTiles()
        {
            DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
            Vector2 unit = new Vector2(1.5f, 0).RotatedBy(Projectile.rotation);
            Utils.PlotTileLine(Projectile.Center, Projectile.Center + unit * LaserLength, (Projectile.width + 16) * Projectile.scale, DelegateMethods.CutTiles);
        }
        public virtual void CastLights(Vector3 color)
        {
            // Cast a light along the line of the Laser
            DelegateMethods.v3_1 = color;
            Utils.PlotTileLine(Projectile.Center, Projectile.Center + new Vector2(1f, 0).RotatedBy(Projectile.rotation) * LaserLength, 26, DelegateMethods.CastLight);
        }
        public float LengthSetting(Projectile projectile, Vector2 endpoint)
        {
            float hitscanBeamLength = (endpoint - projectile.Center).Length();
            LaserLength = MathHelper.Lerp(LaserLength, hitscanBeamLength, 1f);
            return LaserLength;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (NewCollision)
            {
                if (Helper.CheckLinearCollision(Projectile.Center, endPoint, targetHitbox, out Vector2 collisionPoint))
                    return true;
            }
            else
            {
                Vector2 unit = new Vector2(1.5f, 0).RotatedBy(Projectile.rotation);
                float point = 0f;
                // Run an AABB versus Line check to look for collisions
                if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center,
                    Projectile.Center + unit * LaserLength, Projectile.width * LaserScale, ref point))
                    return true;
            }
            return false;
        }
    }
}