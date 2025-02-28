﻿using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AncientScepter
{
    /// <summary>
    /// Contains utilities for working with RoR2.SkillDef and RoR2.SkillFamily.
    /// </summary>
    public static class SkillUtil
    {
        //TODO: replace active skilldefs when overrides happen

        /// <summary>Calls RecalculateValues on all GenericSkill instances (on living CharacterBodies) which have the target SkillDef.</summary>
        /// <param name="targetDef">The SkillDef to attempt to update.</param>
        public static void GlobalUpdateSkillDef(SkillDef targetDef)
        {
            MiscUtil.AliveList().ForEach(cb =>
            {
                if (!cb.hasBody) return;
                var sloc = cb.GetBody().skillLocator;
                if (!sloc) return;
                for (var i = 0; i < sloc.skillSlotCount; i++)
                {
                    var tsk = sloc.GetSkillAtIndex(i);
                    if (tsk.skillDef == targetDef)
                        tsk.RecalculateValues();
                }
            });
        }

        /// <summary>
        /// Attempts to look up a name in the BodyCatalog, then find the SkillFamily instance used for one of its slots (by index). Returns null on failure.
        /// </summary>
        /// <param name="bodyName">The body name to search for. Case sensitive.</param>
        /// <param name="slotIndex">The skillslot index to search for. Must be positive and within range of the target body's skillslot count.</param>
        /// <returns>The resultant SkillFamily if lookup was successful, null otherwise.</returns>
        public static SkillFamily FindSkillFamilyFromBody(string bodyName, int slotIndex)
        {
            var targetBodyIndex = BodyCatalog.FindBodyIndex(bodyName);
            if (targetBodyIndex == BodyIndex.None)
            {
                AncientScepterMain._logger.LogError($"FindSkillFamilyFromBody: Couldn't find body with name {bodyName}");
                return null;
            }
            var allSlots = BodyCatalog.GetBodyPrefabSkillSlots(targetBodyIndex);
            if (slotIndex < 0 || slotIndex > allSlots.Length)
            {
                AncientScepterMain._logger.LogError($"FindSkillFamilyFromBody: Skill slot index {slotIndex} is invalid for body with name {bodyName}");
                return null;
            }
            return BodyCatalog.GetBodyPrefabSkillSlots(targetBodyIndex)[slotIndex].skillFamily;
        }

        /// <summary>
        /// Attempts to look up a name in the BodyCatalog, then find the SkillFamily instance used for one of its slots (by slot enum type). Returns null on failure.
        /// </summary>
        /// <param name="bodyName">The body name to search for. Case sensitive.</param>
        /// <param name="slot">The skillslot name to search for.</param>
        /// <returns>The resultant SkillFamily if lookup was successful, null otherwise.</returns>
        public static SkillFamily FindSkillFamilyFromBody(string bodyName, SkillSlot slot)
        {
            var targetBodyIndex = BodyCatalog.FindBodyIndex(bodyName);
            if (targetBodyIndex == BodyIndex.None)
            {
                AncientScepterMain._logger.LogError($"FindSkillFamilyFromBody: Couldn't find body with name {bodyName}");
                return null;
            }
            //var allSlots = BodyCatalog.GetBodyPrefabSkillSlots(targetBodyIndex);
            var skLoc = BodyCatalog.GetBodyPrefab(targetBodyIndex).GetComponentInChildren<SkillLocator>();
            if (!skLoc)
            {
                AncientScepterMain._logger.LogError($"FindSkillFamilyFromBody: Body with name {bodyName} has no SkillLocator");
                return null;
            }
            foreach (var skillInstance in BodyCatalog.GetBodyPrefabSkillSlots(targetBodyIndex))
            {
                var targetSlot = skLoc.FindSkillSlot(skillInstance);
                if (targetSlot == slot) return skillInstance.skillFamily;
            }
            AncientScepterMain._logger.LogError($"FindSkillFamilyFromBody: Body with name {bodyName} has no skill in slot {slot}");
            return null;
        }

        /// <summary>
        /// Replaces the first instance of a certain variant SkillDef in a SkillFamily with a different one, via direct access to the SkillFamily. Fails if origDef is not found.
        /// </summary>
        /// <param name="targetFamily">The SkillFamily to perform a replacement on.</param>
        /// <param name="origDef">The SkillDef to replace.</param>
        /// <param name="newDef">The SkillDef which will replace OrigDef.</param>
        public static void ReplaceVariant(this SkillFamily targetFamily, SkillDef origDef, SkillDef newDef)
        {
            var ind = Array.FindIndex(targetFamily.variants, x => x.skillDef == origDef);
            if (ind < 0)
            {
                AncientScepterMain._logger.LogError($"SkillFamily.OverrideVariant: couldn't find target skilldef {origDef} in family {targetFamily}");
                return;
            }
            targetFamily.variants[ind].skillDef = newDef;
        }

        /// <summary>
        /// Replaces the first instance of a certain variant SkillDef in a SkillFamily with a different one, via body+slot lookup. Fails if origDef is not found, or if the body+slot lookup does.
        /// </summary>
        /// <param name="targetBodyName">The body name to search for. Case sensitive.</param>
        /// <param name="targetSlot">The skillslot index to search for. Must be positive and within range of the target body's skillslot count.</param>
        /// <param name="origDef">The SkillDef to replace.</param>
        /// <param name="newDef">The SkillDef which will replace OrigDef.</param>
        public static void ReplaceVariant(string targetBodyName, int targetSlot, SkillDef origDef, SkillDef newDef)
        {
            var targetFamily = FindSkillFamilyFromBody(targetBodyName, targetSlot);
            if (targetFamily != null)
                targetFamily.ReplaceVariant(origDef, newDef);
            else
                AncientScepterMain._logger.LogError("Failed to OverrideVariant for bodyname+slot (target not found)");
        }

        /// <summary>
        /// Replaces the first instance of a certain variant SkillDef in a SkillFamily with a different one, via body+slotname lookup. Fails if origDef is not found, or if the body+slotname lookup does.
        /// </summary>
        /// <param name="targetBodyName">The body name to search for. Case sensitive.</param>
        /// <param name="targetSlot">The skillslot name to search for.</param>
        /// <param name="origDef">The SkillDef to replace.</param>
        /// <param name="newDef">The SkillDef which will replace OrigDef.</param>
        public static void ReplaceVariant(string targetBodyName, SkillSlot targetSlot, SkillDef origDef, SkillDef newDef)
        {
            var targetFamily = FindSkillFamilyFromBody(targetBodyName, targetSlot);
            if (targetFamily != null)
                targetFamily.ReplaceVariant(origDef, newDef);
            else
                AncientScepterMain._logger.LogError("Failed to OverrideVariant for bodyname+slotname (target not found)");
        }

        /// <summary>
        /// Adds a new variant to a SkillFamily, via direct access to the SkillFamily.
        /// </summary>
        /// <param name="targetFamily">The SkillFamily to perform an addition on.</param>
        /// <param name="newDef">The SkillDef to add.</param>
        /// <param name="unlockableName">The name of the unlockable which should restrict access to this variant until unlocked. Default value results in no unlockable.</param>
        [Obsolete]
        public static void AddVariant(this SkillFamily targetFamily, SkillDef newDef, string unlockableName = "")
        {
            Array.Resize(ref targetFamily.variants, targetFamily.variants.Length + 1);
            targetFamily.variants[targetFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = newDef,
                viewableNode = new ViewablesCatalog.Node(newDef.skillNameToken, false, null),
                unlockableName = unlockableName
            };
        }

        /// <summary>
        /// Adds a new variant to a SkillFamily, via direct access to the SkillFamily.
        /// </summary>
        /// <param name="targetFamily">The SkillFamily to perform an addition on.</param>
        /// <param name="newDef">The SkillDef to add.</param>
        /// <param name="unlockableName">The name of the unlockable which should restrict access to this variant until unlocked. Default value results in no unlockable.</param>
        public static void AddVariant(this SkillFamily targetFamily, SkillDef newDef, UnlockableDef unlockableDef = null)
        {
            Array.Resize(ref targetFamily.variants, targetFamily.variants.Length + 1);
            targetFamily.variants[targetFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = newDef,
                viewableNode = new ViewablesCatalog.Node(newDef.skillNameToken, false, null),
                unlockableDef = unlockableDef
            };
        }

        /// <summary>
        /// Adds a new variant to a SkillFamily, via body+slot lookup. Fails if body+slot lookup does.
        /// </summary>
        /// <param name="targetBodyName">The body name to search for. Case sensitive.</param>
        /// <param name="targetSlot">The skillslot index to search for. Must be positive and within range of the target body's skillslot count.</param>
        /// <param name="newDef">The SkillDef to add.</param>
        /// <param name="unlockableName">The name of the unlockable which should restrict access to this variant until unlocked. Default value results in no unlockable.</param>
        public static void AddVariant(string targetBodyName, int targetSlot, SkillDef newDef, string unlockableName = "")
        {
            var targetFamily = FindSkillFamilyFromBody(targetBodyName, targetSlot);
            if (targetFamily != null)
                targetFamily.AddVariant(newDef, unlockableName);
            else
                AncientScepterMain._logger.LogError("Failed to AddVariant for bodyname+slot (target not found)");
        }

        /// <summary>
        /// Adds a new variant to a SkillFamily, via body+slotname lookup. Fails if body+slotname lookup does.
        /// </summary>
        /// <param name="targetBodyName">The body name to search for. Case sensitive.</param>
        /// <param name="targetSlot">The skillslot name to search for.</param>
        /// <param name="newDef">The SkillDef to add.</param>
        /// <param name="unlockableName">The name of the unlockable which should restrict access to this variant until unlocked. Default value results in no unlockable.</param>
        public static void AddVariant(string targetBodyName, SkillSlot targetSlot, SkillDef newDef, string unlockableName = "")
        {
            var targetFamily = FindSkillFamilyFromBody(targetBodyName, targetSlot);
            if (targetFamily != null)
                targetFamily.AddVariant(newDef, unlockableName);
            else
                AncientScepterMain._logger.LogError("Failed to AddVariant for bodyname+slotname (target not found)");
        }

        /// <summary>
        /// Removes an existing variant from a SkillFamily, via direct access to the SkillFamily. Fails if targetDef is not found.
        /// </summary>
        /// <param name="targetFamily">The SkillFamily to perform a removal on.</param>
        /// <param name="targetDef">The SkillDef to remove.</param>
        public static void RemoveVariant(this SkillFamily targetFamily, SkillDef targetDef)
        {
            var trimmedVariants = new List<SkillFamily.Variant>(targetFamily.variants);
            var oldLen = trimmedVariants.Count;
            trimmedVariants.RemoveAll(x => x.skillDef == targetDef);
            if (trimmedVariants.Count - oldLen == 0) AncientScepterMain._logger.LogError($"SkillFamily.RemoveVariant: Couldn't find SkillDef {targetDef} for removal from SkillFamily {targetFamily}");
            targetFamily.variants = trimmedVariants.ToArray();
        }

        /// <summary>
        /// Removes an existing variant from a SkillFamily, via body+slot lookup. Fails if targetDef is not found, or if body+slot lookup does.
        /// </summary>
        /// <param name="targetBodyName">The body name to search for. Case sensitive.</param>
        /// <param name="targetSlot">The skillslot index to search for. Must be positive and within range of the target body's skillslot count.</param>
        /// <param name="targetDef">The SkillDef to remove.</param>
        public static void RemoveVariant(string targetBodyName, int targetSlot, SkillDef targetDef)
        {
            var targetFamily = FindSkillFamilyFromBody(targetBodyName, targetSlot);
            if (targetFamily != null)
                targetFamily.RemoveVariant(targetDef);
            else
                AncientScepterMain._logger.LogError("Failed to RemoveVariant for bodyname+slot (target not found)");
        }

        /// <summary>
        /// Removes an existing variant from a SkillFamily, via body+slotname lookup. Fails if targetDef is not found, or if body+slotname lookup does.
        /// </summary>
        /// <param name="targetBodyName">The body name to search for. Case sensitive.</param>
        /// <param name="targetSlot">The skillslot name to search for.</param>
        /// <param name="targetDef">The SkillDef to remove.</param>
        public static void RemoveVariant(string targetBodyName, SkillSlot targetSlot, SkillDef targetDef)
        {
            var targetFamily = FindSkillFamilyFromBody(targetBodyName, targetSlot);
            if (targetFamily != null)
                targetFamily.RemoveVariant(targetDef);
            else
                AncientScepterMain._logger.LogError("Failed to RemoveVariant for bodyname+slotname (target not found)");
        }

        /// <summary>
        /// Clones an existing SkillDef into a new instance with the same values.
        /// </summary>
        /// <param name="oldDef">The SkillDef instance to clone.</param>
        /// <returns>A clone of oldDef, shallow changes to which will not affect the original.</returns>
        public static SkillDef CloneSkillDef(SkillDef oldDef)
        {
            var newDef = ScriptableObject.CreateInstance<SkillDef>();

            newDef.skillName = oldDef.skillName;
            newDef.skillNameToken = oldDef.skillNameToken;
            newDef.skillDescriptionToken = oldDef.skillDescriptionToken;
            newDef.icon = oldDef.icon;

            newDef.activationStateMachineName = oldDef.activationStateMachineName;
            newDef.activationState = oldDef.activationState;

            newDef.interruptPriority = oldDef.interruptPriority;

            newDef.baseRechargeInterval = oldDef.baseRechargeInterval;
            newDef.baseMaxStock = oldDef.baseMaxStock;
            newDef.rechargeStock = oldDef.rechargeStock;
            newDef.requiredStock = oldDef.requiredStock;
            newDef.stockToConsume = oldDef.stockToConsume;
            newDef.beginSkillCooldownOnSkillEnd = oldDef.beginSkillCooldownOnSkillEnd;
            newDef.fullRestockOnAssign = oldDef.fullRestockOnAssign;
            newDef.dontAllowPastMaxStocks = oldDef.dontAllowPastMaxStocks;

            newDef.resetCooldownTimerOnUse = oldDef.resetCooldownTimerOnUse;

            newDef.isCombatSkill = oldDef.isCombatSkill;

            newDef.cancelSprintingOnActivation = oldDef.cancelSprintingOnActivation;
            newDef.canceledFromSprinting = oldDef.canceledFromSprinting;
            newDef.forceSprintDuringState = oldDef.forceSprintDuringState;

            newDef.mustKeyPress = oldDef.mustKeyPress;

            newDef.keywordTokens = oldDef.keywordTokens;

            return newDef;
        }

        /// <summary>
        /// Clones an existing RailgunSkillDef into a new instance with the same values.
        /// </summary>
        /// <param name="oldDef">The RailgunSkillDef instance to clone.</param>
        /// <returns>A clone of oldDef, shallow changes to which will not affect the original.</returns>
        public static RailgunSkillDef CloneSkillDef(RailgunSkillDef oldDef)
        {
            var newDef = ScriptableObject.CreateInstance<RailgunSkillDef>();

            newDef.skillName = oldDef.skillName;
            newDef.skillNameToken = oldDef.skillNameToken;
            newDef.skillDescriptionToken = oldDef.skillDescriptionToken;
            newDef.icon = oldDef.icon;

            newDef.activationStateMachineName = oldDef.activationStateMachineName;
            newDef.activationState = oldDef.activationState;

            newDef.interruptPriority = oldDef.interruptPriority;

            newDef.baseRechargeInterval = oldDef.baseRechargeInterval;
            newDef.baseMaxStock = oldDef.baseMaxStock;
            newDef.rechargeStock = oldDef.rechargeStock;
            newDef.requiredStock = oldDef.requiredStock;
            newDef.stockToConsume = oldDef.stockToConsume;
            newDef.beginSkillCooldownOnSkillEnd = oldDef.beginSkillCooldownOnSkillEnd;
            newDef.fullRestockOnAssign = oldDef.fullRestockOnAssign;
            newDef.dontAllowPastMaxStocks = oldDef.dontAllowPastMaxStocks;

            newDef.resetCooldownTimerOnUse = oldDef.resetCooldownTimerOnUse;

            newDef.isCombatSkill = oldDef.isCombatSkill;

            newDef.cancelSprintingOnActivation = oldDef.cancelSprintingOnActivation;
            newDef.canceledFromSprinting = oldDef.canceledFromSprinting;
            newDef.forceSprintDuringState = oldDef.forceSprintDuringState;

            newDef.mustKeyPress = oldDef.mustKeyPress;

            newDef.keywordTokens = oldDef.keywordTokens;

            newDef.restockOnReload = oldDef.restockOnReload;
            newDef.offlineIcon = oldDef.offlineIcon;

            return newDef;
        }


        /// <summary>
        /// Clones an existing VoidSurvivorSkillDef into a new instance with the same values.
        /// </summary>
        /// <param name="oldDef">The SkillDef instance to clone.</param>
        /// <returns>A clone of oldDef, shallow changes to which will not affect the original.</returns>
        public static VoidSurvivorSkillDef CloneSkillDef(VoidSurvivorSkillDef oldDef)
        {
            var newDef = ScriptableObject.CreateInstance<VoidSurvivorSkillDef>();

            newDef.skillName = oldDef.skillName;
            newDef.skillNameToken = oldDef.skillNameToken;
            newDef.skillDescriptionToken = oldDef.skillDescriptionToken;
            newDef.icon = oldDef.icon;

            newDef.activationStateMachineName = oldDef.activationStateMachineName;
            newDef.activationState = oldDef.activationState;

            newDef.interruptPriority = oldDef.interruptPriority;

            newDef.baseRechargeInterval = oldDef.baseRechargeInterval;
            newDef.baseMaxStock = oldDef.baseMaxStock;
            newDef.rechargeStock = oldDef.rechargeStock;
            newDef.requiredStock = oldDef.requiredStock;
            newDef.stockToConsume = oldDef.stockToConsume;
            newDef.beginSkillCooldownOnSkillEnd = oldDef.beginSkillCooldownOnSkillEnd;
            newDef.fullRestockOnAssign = oldDef.fullRestockOnAssign;
            newDef.dontAllowPastMaxStocks = oldDef.dontAllowPastMaxStocks;

            newDef.resetCooldownTimerOnUse = oldDef.resetCooldownTimerOnUse;

            newDef.isCombatSkill = oldDef.isCombatSkill;

            newDef.cancelSprintingOnActivation = oldDef.cancelSprintingOnActivation;
            newDef.canceledFromSprinting = oldDef.canceledFromSprinting;
            newDef.forceSprintDuringState = oldDef.forceSprintDuringState;

            newDef.mustKeyPress = oldDef.mustKeyPress;

            newDef.keywordTokens = oldDef.keywordTokens;

            newDef.minimumCorruption = oldDef.minimumCorruption;
            newDef.maximumCorruption = oldDef.maximumCorruption;
            return newDef;
        }
    }
}