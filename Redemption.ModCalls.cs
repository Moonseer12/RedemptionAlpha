using Redemption.BaseExtension;
using Redemption.Globals;
using Redemption.Globals.NPC;
using Redemption.Globals.World;
using Redemption.Textures.Elements;
using Redemption.WorldGeneration;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace Redemption
{
    partial class Redemption
    {
        public override object Call(params object[] args)
        {
            try
            {
                string code = args[0].ToString();

                switch (code)
                {
                    case "AbominationnClearEvents":
                        bool eventOccurring = FowlMorningWorld.FowlMorningActive;
                        bool canClearEvents = Convert.ToBoolean(args[1]);
                        if (eventOccurring && canClearEvents)
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                FowlMorningWorld.FowlMorningActive = false;
                                FowlMorningWorld.ChickPoints = 0;
                                FowlMorningWorld.ChickWave = 0;

                                if (Main.netMode == NetmodeID.Server)
                                    NetMessage.SendData(MessageID.WorldData);
                            }

                            FowlMorningWorld.SendInfoPacket();
                        }
                        return eventOccurring;
                }
            }
            catch (Exception e)
            {
                Logger.Error("Call Error: " + e.StackTrace + e.Message);
            }

            if (args is null)
                throw new ArgumentNullException(nameof(args), "Arguments cannot be null!");
            if (args.Length == 0)
                throw new ArgumentException("Arguments cannot be empty!");

            if (args[0] is string content)
            {
                switch (content)
                {
                    #region Elements and Bonus Calls
                    case "addElementNPC":
                        if (args[1] is not int elementID)
                            throw new Exception($"Expected an argument of type int when setting element ID, but got type {args[1].GetType().Name} instead.");
                        if (args[2] is not int npcID)
                            throw new Exception($"Expected an argument of type int when setting NPC type, but got type {args[2].GetType().Name} instead.");
                        return elementID switch
                        {
                            1 => ElementID.NPCArcane[npcID] = true,
                            2 => ElementID.NPCFire[npcID] = true,
                            3 => ElementID.NPCWater[npcID] = true,
                            4 => ElementID.NPCIce[npcID] = true,
                            5 => ElementID.NPCEarth[npcID] = true,
                            6 => ElementID.NPCWind[npcID] = true,
                            7 => ElementID.NPCThunder[npcID] = true,
                            8 => ElementID.NPCHoly[npcID] = true,
                            9 => ElementID.NPCShadow[npcID] = true,
                            10 => ElementID.NPCNature[npcID] = true,
                            11 => ElementID.NPCPoison[npcID] = true,
                            12 => ElementID.NPCBlood[npcID] = true,
                            13 => ElementID.NPCPsychic[npcID] = true,
                            14 => ElementID.NPCCelestial[npcID] = true,
                            15 => ElementID.NPCExplosive[npcID] = true,
                            _ => false,
                        };
                    case "addElementItem":
                        if (args[1] is not int elementID2)
                            throw new Exception($"Expected an argument of type int when setting element ID, but got type {args[1].GetType().Name} instead.");
                        if (args[2] is not int itemID)
                            throw new Exception($"Expected an argument of type int when setting Item type, but got type {args[2].GetType().Name} instead.");

                        if (args.Length > 3)
                        {
                            if (args[3] is bool projInheritsFromItem)
                                ElementID.ProjectilesInheritElements[itemID] = projInheritsFromItem;
                            else
                                throw new Exception($"Expected an argument of type bool when setting projectile inheriting, but got type {args[3].GetType().Name} instead.");
                        }

                        return elementID2 switch
                        {
                            1 => ElementID.ItemArcane[itemID] = true,
                            2 => ElementID.ItemFire[itemID] = true,
                            3 => ElementID.ItemWater[itemID] = true,
                            4 => ElementID.ItemIce[itemID] = true,
                            5 => ElementID.ItemEarth[itemID] = true,
                            6 => ElementID.ItemWind[itemID] = true,
                            7 => ElementID.ItemThunder[itemID] = true,
                            8 => ElementID.ItemHoly[itemID] = true,
                            9 => ElementID.ItemShadow[itemID] = true,
                            10 => ElementID.ItemNature[itemID] = true,
                            11 => ElementID.ItemPoison[itemID] = true,
                            12 => ElementID.ItemBlood[itemID] = true,
                            13 => ElementID.ItemPsychic[itemID] = true,
                            14 => ElementID.ItemCelestial[itemID] = true,
                            15 => ElementID.ItemExplosive[itemID] = true,
                            _ => false,
                        };
                    case "addElementProj":
                        if (args[1] is not int elementID3)
                            throw new Exception($"Expected an argument of type int when setting element ID, but got type {args[1].GetType().Name} instead.");
                        if (args[2] is not int projID)
                            throw new Exception($"Expected an argument of type int when setting Projectile type, but got type {args[2].GetType().Name} instead.");

                        if (args.Length > 3)
                        {
                            if (args[3] is bool projInheritsFromProj)
                                ElementID.ProjectilesInheritElementsFromThis[projID] = projInheritsFromProj;
                            else
                                throw new Exception($"Expected an argument of type bool when setting projectile inheriting, but got type {args[3].GetType().Name} instead.");
                        }

                        return elementID3 switch
                        {
                            1 => ElementID.ProjArcane[projID] = true,
                            2 => ElementID.ProjFire[projID] = true,
                            3 => ElementID.ProjWater[projID] = true,
                            4 => ElementID.ProjIce[projID] = true,
                            5 => ElementID.ProjEarth[projID] = true,
                            6 => ElementID.ProjWind[projID] = true,
                            7 => ElementID.ProjThunder[projID] = true,
                            8 => ElementID.ProjHoly[projID] = true,
                            9 => ElementID.ProjShadow[projID] = true,
                            10 => ElementID.ProjNature[projID] = true,
                            11 => ElementID.ProjPoison[projID] = true,
                            12 => ElementID.ProjBlood[projID] = true,
                            13 => ElementID.ProjPsychic[projID] = true,
                            14 => ElementID.ProjCelestial[projID] = true,
                            15 => ElementID.ProjExplosive[projID] = true,
                            _ => false,
                        };
                    case "elementOverrideItem":
                        if (args[1] is not Item item)
                            throw new Exception($"Expected an argument of type Item, but got type {args[1].GetType().Name} instead.");
                        if (args[2] is not int elementID4)
                            throw new Exception($"Expected an argument of type int when setting Elemental type, but got type {args[2].GetType().Name} instead.");
                        if (args[3] is not sbyte overrideID)
                            throw new Exception($"Expected an argument of type sbyte when setting override behaviour, but got type {args[3].GetType().Name} instead. (1 = Add Element, -1 = Remove Element)");

                        item.GetGlobalItem<ElementalItem>().OverrideElement[elementID4] = overrideID;
                        break;
                    case "elementOverrideNPC":
                        if (args[1] is not NPC npc)
                            throw new Exception($"Expected an argument of type NPC, but got type {args[1].GetType().Name} instead.");
                        if (args[2] is not int elementID5)
                            throw new Exception($"Expected an argument of type int when setting Elemental type, but got type {args[2].GetType().Name} instead.");
                        if (args[3] is not sbyte overrideID2)
                            throw new Exception($"Expected an argument of type sbyte when setting override behaviour, but got type {args[3].GetType().Name} instead. (1 = Add Element, -1 = Remove Element)");

                        npc.GetGlobalNPC<ElementalNPC>().OverrideElement[elementID5] = overrideID2;
                        break;
                    case "elementOverrideProj":
                        if (args[1] is not Projectile proj)
                            throw new Exception($"Expected an argument of type Projectile, but got type {args[1].GetType().Name} instead.");
                        if (args[2] is not int elementID6)
                            throw new Exception($"Expected an argument of type int when setting Elemental type, but got type {args[2].GetType().Name} instead.");
                        if (args[3] is not sbyte overrideID3)
                            throw new Exception($"Expected an argument of type sbyte when setting override behaviour, but got type {args[3].GetType().Name} instead. (1 = Add Element, -1 = Remove Element)");

                        proj.GetGlobalProjectile<ElementalProjectile>().OverrideElement[elementID6] = overrideID3;
                        break;
                    case "hasElementItem":
                        if (args[1] is not Item item3)
                            throw new Exception($"Expected an argument of type Item, but got type {args[1].GetType().Name} instead.");
                        if (args[2] is not int elementID9)
                            throw new Exception($"Expected an argument of type int when setting Elemental type, but got type {args[2].GetType().Name} instead.");

                        return ElementID.HasElementItem(item3, elementID9);
                    case "hasElementNPC":
                        if (args[1] is not NPC npc3)
                            throw new Exception($"Expected an argument of type NPC, but got type {args[1].GetType().Name} instead.");
                        if (args[2] is not int elementID10)
                            throw new Exception($"Expected an argument of type int when setting Elemental type, but got type {args[2].GetType().Name} instead.");

                        return ElementID.HasElement(npc3, elementID10);
                    case "hasElementProj":
                        if (args[1] is not Projectile proj1)
                            throw new Exception($"Expected an argument of type Projectile, but got type {args[1].GetType().Name} instead.");
                        if (args[2] is not int elementID11)
                            throw new Exception($"Expected an argument of type int when setting Elemental type, but got type {args[2].GetType().Name} instead.");

                        return ElementID.HasElement(proj1, elementID11);
                    case "getFirstElementItem":
                        if (args[1] is not Item item4)
                            throw new Exception($"Expected an argument of type Item, but got type {args[1].GetType().Name} instead.");
                        if (args[2] is not bool ignoreExplosive)
                            throw new Exception($"Expected an argument of type bool when setting if to ignore explosive, but got type {args[2].GetType().Name} instead.");

                        return ElementID.GetFirstElement(item4, ignoreExplosive);
                    case "getFirstElementNPC":
                        if (args[1] is not NPC npc5)
                            throw new Exception($"Expected an argument of type NPC, but got type {args[1].GetType().Name} instead.");
                        if (args[2] is not bool ignoreExplosive2)
                            throw new Exception($"Expected an argument of type bool when setting if to ignore explosive, but got type {args[2].GetType().Name} instead.");

                        return ElementID.GetFirstElement(npc5, ignoreExplosive2);
                    case "getFirstElementProj":
                        if (args[1] is not Projectile proj2)
                            throw new Exception($"Expected an argument of type Projectile, but got type {args[1].GetType().Name} instead.");
                        if (args[2] is not bool ignoreExplosive3)
                            throw new Exception($"Expected an argument of type bool when setting if to ignore explosive, but got type {args[2].GetType().Name} instead.");

                        return ElementID.GetFirstElement(proj2, ignoreExplosive3);
                    case "elementMultiplier":
                        if (args[1] is not NPC npc2)
                            throw new Exception($"Expected an argument of type NPC, but got type {args[1].GetType().Name} instead.");
                        if (args[2] is not int elementID7)
                            throw new Exception($"Expected an argument of type int when setting Elemental type, but got type {args[2].GetType().Name} instead.");
                        if (args[3] is not float multiplier)
                            throw new Exception($"Expected an argument of type float when setting multiplier value, but got type {args[3].GetType().Name} instead.");

                        if (npc2.TryGetGlobalNPC<ElementalNPC>(out ElementalNPC elementNPC))
                        {
                            elementNPC.OverrideMultiplier[elementID7] *= multiplier;
                            if (args.Length > 4)
                            {
                                if (args[4] is bool noSetMultipliers)
                                {
                                    if (!noSetMultipliers)
                                        ElementalNPC.SetElementalMultipliers(npc2, ref npc2.GetGlobalNPC<ElementalNPC>().elementDmg);
                                }
                                else
                                    throw new Exception($"Expected an argument of type bool when disabling setting multipliers, but got type {args[4].GetType().Name} instead.");
                            }
                            else
                                ElementalNPC.SetElementalMultipliers(npc2, ref npc2.GetGlobalNPC<ElementalNPC>().elementDmg);
                        }
                        break;
                    case "uncapBossElementMultiplier":
                        if (args[1] is not NPC npc4)
                            throw new Exception($"Expected an argument of type NPC, but got type {args[1].GetType().Name} instead.");
                        if (args[2] is not bool uncapped)
                            throw new Exception($"Expected an argument of type bool when setting uncapped multiplier for boss, but got type {args[2].GetType().Name} instead.");

                        if (npc4.TryGetGlobalNPC<ElementalNPC>(out ElementalNPC elementNPC2))
                            return elementNPC2.uncappedBossMultiplier = uncapped;
                        return false;
                    case "hideElementIcon":
                        if (args[1] is not Item item2)
                            throw new Exception($"Expected an argument of type Item, but got type {args[1].GetType().Name} instead.");
                        if (args[2] is not int elementID8)
                            throw new Exception($"Expected an argument of type int when setting Elemental type, but got type {args[2].GetType().Name} instead.");
                        if (args[3] is not bool hidden)
                            throw new Exception($"Expected an argument of type bool when setting if hidden or not, but got type {args[3].GetType().Name} instead.");

                        item2.Redemption().HideElementTooltip[elementID8] = hidden;
                        break;
                    case "addItemToBluntSwing": // Disables automatic Slash Bonus
                        if (args[1] is not int ItemID)
                            throw new Exception($"Expected an argument of type int when setting Projectile type, but got type {args[1].GetType().Name} instead.");
                        ItemLists.BluntSwing.Add(ItemID);
                        break;
                    case "addNPCToElementTypeList":
                        if (args[1] is not string typeString)
                            throw new Exception($"Expected an argument of type string when setting Type Name, but got type {args[1].GetType().Name} instead.");
                        if (args[2] is not int NPCID)
                            throw new Exception($"Expected an argument of type int when setting NPC type, but got type {args[2].GetType().Name} instead.");
                        switch (typeString)
                        {
                            case "Skeleton":
                                NPCLists.Skeleton.Add(NPCID);
                                break;
                            case "SkeletonHumanoid":
                                NPCLists.SkeletonHumanoid.Add(NPCID);
                                break;
                            case "Humanoid": // Doesn't have to include SkeletonHumanoid
                                NPCLists.Humanoid.Add(NPCID);
                                break;
                            case "Undead":
                                NPCLists.Undead.Add(NPCID);
                                break;
                            case "Spirit":
                                NPCLists.Spirit.Add(NPCID);
                                break;
                            case "Plantlike":
                                NPCLists.Plantlike.Add(NPCID);
                                break;
                            case "Demon":
                                NPCLists.Demon.Add(NPCID);
                                break;
                            case "Cold":
                                NPCLists.Cold.Add(NPCID);
                                break;
                            case "Hot":
                                NPCLists.Hot.Add(NPCID);
                                break;
                            case "Wet":
                                NPCLists.Wet.Add(NPCID);
                                break;
                            case "Dragonlike":
                                NPCLists.Dragonlike.Add(NPCID);
                                break;
                            case "Inorganic":
                                NPCLists.Inorganic.Add(NPCID);
                                break;
                            case "Robotic": // Also add these into Inorganic
                                NPCLists.Robotic.Add(NPCID);
                                break;
                            case "Armed":
                                NPCLists.Armed.Add(NPCID);
                                break;
                            case "Hallowed":
                                NPCLists.Hallowed.Add(NPCID);
                                break;
                            case "Dark":
                                NPCLists.Dark.Add(NPCID);
                                break;
                            case "Blood":
                                NPCLists.Blood.Add(NPCID);
                                break;
                            case "Slime":
                                NPCLists.IsSlime.Add(NPCID);
                                break;
                        }
                        break;
                    case "decapitation":
                        if (args[1] is not NPC target)
                            throw new Exception($"Expected an argument of type NPC when setting target, but got type {args[1].GetType().Name} instead.");
                        if (args[2] is not int damageDone)
                            throw new Exception($"Expected an argument of type int when setting damageDone ref, but got type {args[2].GetType().Name} instead.");
                        if (args[3] is not bool crit)
                            throw new Exception($"Expected an argument of type bool when setting crit ref, but got type {args[3].GetType().Name} instead.");

                        int c = 200;
                        if (args.Length > 4)
                        {
                            if (args[4] is int chance)
                            {
                                c = chance;
                            }
                            else
                                throw new Exception($"Expected an argument of type int when setting chance, but got type {args[4].GetType().Name} instead.");
                        }

                        return RedeProjectile.Decapitation(target, ref damageDone, ref crit, c);
                    case "setSlashBonus":
                        if (args[1] is not Item slashItem)
                            throw new Exception($"Expected an argument of type Item when setting item to get the bonus, but got type {args[1].GetType().Name} instead.");
                        if (args.Length > 2)
                        {
                            if (args[2] is bool isSlash)
                                return slashItem.Redemption().TechnicallySlash = isSlash;
                            else
                                throw new Exception($"Expected an argument of type bool when setting slash bonus, but got type {args[2].GetType().Name} instead.");
                        }

                        return slashItem.Redemption().TechnicallySlash = true;
                    case "setAxeBonus":
                        if (args[1] is not Item axeItem)
                            throw new Exception($"Expected an argument of type Item when setting item to get the bonus, but got type {args[1].GetType().Name} instead.");
                        if (args.Length > 2)
                        {
                            if (args[2] is bool isAxe)
                                return axeItem.Redemption().TechnicallyAxe = isAxe;
                            else
                                throw new Exception($"Expected an argument of type bool when setting axe bonus, but got type {args[2].GetType().Name} instead.");
                        }

                        return axeItem.Redemption().TechnicallyAxe = true;
                    case "setHammerBonus":
                        if (args[1] is not Item hammerItem)
                            throw new Exception($"Expected an argument of type Item when setting item to get the bonus, but got type {args[1].GetType().Name} instead.");
                        if (args.Length > 2)
                        {
                            if (args[2] is bool isHammer)
                                return hammerItem.Redemption().TechnicallyHammer = isHammer;
                            else
                                throw new Exception($"Expected an argument of type bool when setting hammer bonus, but got type {args[2].GetType().Name} instead.");
                        }

                        return hammerItem.Redemption().TechnicallyHammer = true;
                    case "setHammerProj":
                        if (args[1] is not Projectile hammerProj)
                            throw new Exception($"Expected an argument of type Projectile when setting projectile to be hammer, but got type {args[1].GetType().Name} instead.");
                        if (args.Length > 2)
                        {
                            if (args[2] is bool isHammer)
                                return hammerProj.Redemption().IsHammer = isHammer;
                            else
                                throw new Exception($"Expected an argument of type bool when setting if projectile is hammer, but got type {args[2].GetType().Name} instead.");
                        }

                        return hammerProj.Redemption().IsHammer = true;
                    case "setAxeProj":
                        if (args[1] is not Projectile axeProj)
                            throw new Exception($"Expected an argument of type Projectile when setting projectile to be axe, but got type {args[1].GetType().Name} instead.");
                        if (args.Length > 2)
                        {
                            if (args[2] is bool isAxe)
                                return axeProj.Redemption().IsAxe = isAxe;
                            else
                                throw new Exception($"Expected an argument of type bool when setting if projectile is axe, but got type {args[2].GetType().Name} instead.");
                        }

                        return axeProj.Redemption().IsAxe = true;
                    case "setSpearProj":
                        if (args[1] is not Projectile spearProj)
                            throw new Exception($"Expected an argument of type Projectile when setting projectile to be spear, but got type {args[1].GetType().Name} instead.");
                        if (args.Length > 2)
                        {
                            if (args[2] is bool isSpear)
                                return spearProj.Redemption().IsSpear = isSpear;
                            else
                                throw new Exception($"Expected an argument of type bool when setting if projectile is spear, but got type {args[2].GetType().Name} instead.");
                        }

                        return spearProj.Redemption().IsSpear = true;
                    case "increaseElementalResistance":
                        if (args[1] is not Player player)
                            throw new Exception($"Expected an argument of type Player when setting player, but got type {args[1].GetType().Name} instead.");
                        if (args[2] is not int elementID12)
                            throw new Exception($"Expected an argument of type int when setting element ID, but got type {args[1].GetType().Name} instead.");
                        if (args[3] is not float multiplier1)
                            throw new Exception($"Expected an argument of type int when setting increase value, but got type {args[1].GetType().Name} instead.");

                        player.RedemptionPlayerBuff().ElementalResistance[elementID12] += multiplier1;
                        break;
                    case "increaseElementalDamage":
                        if (args[1] is not Player player2)
                            throw new Exception($"Expected an argument of type Player when setting player, but got type {args[1].GetType().Name} instead.");
                        if (args[2] is not int elementID13)
                            throw new Exception($"Expected an argument of type int when setting element ID, but got type {args[1].GetType().Name} instead.");
                        if (args[3] is not float multiplier2)
                            throw new Exception($"Expected an argument of type int when setting increase value, but got type {args[1].GetType().Name} instead.");

                        player2.RedemptionPlayerBuff().ElementalDamage[elementID13] += multiplier2;
                        break;
                    #endregion
                    #region Guard Points
                    case "setGuardPoints":
                        if (args[1] is not NPC guardNpc)
                            throw new Exception($"Expected an argument of type NPC, but got type {args[1].GetType().Name} instead.");
                        if (args[2] is not int amt)
                            throw new Exception($"Expected an argument of type int when setting amount, but got type {args[2].GetType().Name} instead.");

                        if (guardNpc.TryGetGlobalNPC(out GuardNPC GuardNPC))
                        {
                            GuardNPC.GuardBroken = false;
                            GuardNPC.GuardPoints = amt;
                        }
                        break;
                    case "checkGuardPoints":
                        if (args[1] is not NPC guardNpc2)
                            throw new Exception($"Expected an argument of type NPC, but got type {args[1].GetType().Name} instead.");
                        if (args[2] is not NPC.HitModifiers modifiers)
                            throw new Exception($"Expected an argument of type NPC.HitModifiers, but got type {args[2].GetType().Name} instead.");
                        if (args[3] is not float dmgReduce)
                            throw new Exception($"Expected an argument of type float for damage reduction value, but got type {args[3].GetType().Name} instead.");
                        if (args[4] is not bool noSound)
                            throw new Exception($"Expected an argument of type bool for no guard hit sound, but got type {args[4].GetType().Name} instead.");
                        if (args[5] is not int dustType)
                            throw new Exception($"Expected an argument of type int for dust type on break, but got type {args[5].GetType().Name} instead.");
                        if (args[6] is not int dustAmt)
                            throw new Exception($"Expected an argument of type int for dust amount on break, but got type {args[6].GetType().Name} instead.");
                        if (args[7] is not float dustScale)
                            throw new Exception($"Expected an argument of type float for dust scale on break, but got type {args[7].GetType().Name} instead.");
                        if (args[8] is not int breakDamage)
                            throw new Exception($"Expected an argument of type int for damage on break, but got type {args[8].GetType().Name} instead.");

                        SoundStyle sound = SoundID.DD2_WitherBeastCrystalImpact;
                        if (args.Length > 9)
                        {
                            if (args[9] is SoundStyle newSound)
                                sound = newSound;
                            else
                                throw new Exception($"Expected an argument of type SoundStyle, but got type {args[9].GetType().Name} instead.");
                        }
                        SoundStyle breakSound = default;
                        if (args.Length > 10)
                        {
                            if (args[10] is SoundStyle newSound)
                                breakSound = newSound;
                            else
                                throw new Exception($"Expected an argument of type SoundStyle, but got type {args[10].GetType().Name} instead.");
                        }

                        if (guardNpc2.TryGetGlobalNPC(out GuardNPC GuardNPC2))
                        {
                            if (GuardNPC2.GuardPoints >= 0)
                            {
                                modifiers.DisableCrit();
                                modifiers.ModifyHitInfo += (ref NPC.HitInfo n) => GuardNPC2.GuardHit(ref n, guardNpc2, sound, dmgReduce, noSound, dustType, breakSound, dustAmt, dustScale, breakDamage);
                                return true;
                            }
                            else
                                return false;
                        }
                        return false;
                    case "breakGuardPoints":
                        if (args[1] is not NPC guardNpc3)
                            throw new Exception($"Expected an argument of type NPC, but got type {args[1].GetType().Name} instead.");
                        if (args[2] is not int dustType2)
                            throw new Exception($"Expected an argument of type int for dust type, but got type {args[2].GetType().Name} instead.");
                        if (args[3] is not int dustAmt2)
                            throw new Exception($"Expected an argument of type int for dust amount, but got type {args[3].GetType().Name} instead.");
                        if (args[4] is not float dustScale2)
                            throw new Exception($"Expected an argument of type float for dust scale, but got type {args[4].GetType().Name} instead.");
                        if (args[5] is not int breakDamage2)
                            throw new Exception($"Expected an argument of type int for damage, but got type {args[5].GetType().Name} instead.");

                        SoundStyle breakSound2 = CustomSounds.GuardBreak;
                        if (args.Length > 6)
                        {
                            if (args[6] is SoundStyle newSound)
                                breakSound2 = newSound;
                            else
                                throw new Exception($"Expected an argument of type SoundStyle, but got type {args[6].GetType().Name} instead.");
                        }

                        if (guardNpc3.TryGetGlobalNPC(out GuardNPC GuardNPC3))
                        {
                            GuardNPC3.GuardPoints = 0;
                            GuardNPC3.GuardBreakCheck(guardNpc3, dustType2, breakSound2, dustAmt2, dustScale2, breakDamage2);
                        }
                        return false;
                    case "getGuardPoints":
                        if (args[1] is not NPC guardNpc4)
                            throw new Exception($"Expected an argument of type NPC, but got type {args[1].GetType().Name} instead.");

                        if (guardNpc4.TryGetGlobalNPC(out GuardNPC GuardNPC4))
                            return GuardNPC4.GuardPoints;
                        return 0;
                    case "isGuardBroken":
                        if (args[1] is not NPC guardNpc5)
                            throw new Exception($"Expected an argument of type NPC, but got type {args[1].GetType().Name} instead.");

                        if (guardNpc5.TryGetGlobalNPC(out GuardNPC GuardNPC5))
                            return GuardNPC5.GuardBroken;
                        return true;
                    #endregion
                    case "RaveyardActive":
                        return RedeWorld.SkeletonInvasion;
                    case "alignment":
                        return RedeWorld.Alignment;
                    case "bastionPos":
                        return RedeGen.BastionVector;
                    case "UGPortalPos":
                        return RedeGen.gathicPortalVector;
                    case "goldenGatewayPos":
                        return RedeGen.GoldenGatewayVector;
                    case "hallOfHeroesPos":
                        return RedeGen.HallOfHeroesVector;
                    case "natureShrinePos":
                        return RedeGen.JoShrinePoint;
                    case "labPos":
                        return RedeGen.LabVector;
                    case "surfacePortalPos":
                        return RedeGen.newbCaveVector;
                    case "slayerShipPos":
                        return RedeGen.slayerShipVector;
                    case "spiritAssassinPoint":
                        return RedeGen.SpiritAssassinPoint;
                    case "spiritCommonGuardPoint":
                        return RedeGen.SpiritCommonGuardPoint;
                    case "spiritOldManPoint":
                        return RedeGen.SpiritOldManPoint;
                    case "hangingTiedPoint":
                        return RedeGen.HangingTiedPoint;
                    case "spiritOldLadyPoint":
                        return RedeGen.SpiritOldLadyPoint;
                    case "spiritDruidPoint":
                        return RedeGen.SpiritDruidPoint;
                }
            }
            /*
            // In SetStaticDefaults() of ModItem, ModProjectile, or ModNPC
            if (!ModLoader.TryGetMod("Redemption", out var redemption))
                return;

            // For ModItem
            redemption.Call("addElementItem", 13, Type); // Psychic element ID
            // For ModProjectile
            redemption.Call("addElementProj", 4, Type); // Ice element ID
            // For ModNPC
            redemption.Call("addElementNPC", 6, Type); // Wind element ID
            
            1 = Arcane
            2 = Fire
            3 = Water
            4 = Ice
            5 = Earth
            6 = Wind
            7 = Thunder
            8 = Holy
            9 = Shadow
            10 = Nature
            11 = Poison
            12 = Blood
            13 = Psychic
            14 = Celestial
            15 = Explosive
            */
            return false;
        }
    }
}