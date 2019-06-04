using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BattleTech;
using Harmony;
using Localize;
using UnityEngine;
using Random = UnityEngine.Random;

namespace WeaponRealizer
{
    internal static class StatisticHelper
    {
        internal static Statistic GetOrCreateStatisic<StatisticType>(StatCollection collection, string statName,
            StatisticType defaultValue)
        {
            Statistic statistic = collection.GetStatistic(statName);

            if (statistic == null)
            {
                statistic = collection.AddStatistic<StatisticType>(statName, defaultValue);
            }

            return statistic;
        }
    }

    [HarmonyPatch(typeof(AbstractActor), "OnActivationEnd", MethodType.Normal)]
    static class JammingEnabler
    {
        public static bool Prepare()
        {
            return Core.ModSettings.Jamming;
        }

        static void Prefix(AbstractActor __instance)
        {
            var actor = __instance;
            if (actor.IsShutDown) return;

            foreach (Weapon weapon in actor.Weapons)
            {
                if (!IsJammable(weapon)) continue;
                if (IsJammed(weapon))
                {
                    var removedJam = AttemptToRemoveJam(actor, weapon);
                    Logger.Debug($"Removed Jam? {removedJam}");
                }
                else if (weapon.roundsSinceLastFire == 0) // fired this round
                {
                    var addedJam = AttemptToAddJam(actor, weapon);
                    Logger.Debug($"Added Jam? {addedJam}");
                }
            }
        }

        private static float GetRefireModifier(Weapon weapon)
        {
            if (weapon.RefireModifier > 0 && weapon.roundsSinceLastFire < 2)
                return (float) weapon.RefireModifier;
            return 0.0f;
        }

        private static bool AttemptToAddJam(AbstractActor actor, Weapon weapon)
        {
            // TODO: can we exponentially increase refiremodifier?
            // LadyAlekto: every turn fired, the refiremodifier gets added
            // LadyAlekto: either as toggle or global
            // LadyAlekto: and when you brace a turn, it resets
            // LadyAlekto: brace as in "dont shoot"
            var refireModifier = GetRefireModifier(weapon);
            var roll = Random.Range(1, 100);
            var skill = actor.SkillGunnery;
            var mitigationRoll = Random.Range(2, 11);
            var multiplier = JamMultipliers[weapon.defId];
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"damaged: {weapon.DamageLevel}");
            sb.AppendLine($"refiremod: {refireModifier}");
            sb.AppendLine($"roll: {roll}");
            sb.AppendLine($"gunneryskill: {skill}");
            sb.AppendLine($"mitigationRoll: {mitigationRoll}");
            sb.AppendLine($"multiplier: {multiplier}");
            if (roll >= refireModifier * multiplier)
            {
                Logger.Debug(sb.ToString());
                return false;
            }

            if (skill >= mitigationRoll)
            {
                Logger.Debug(sb.ToString());
                return false;
            }

            Logger.Debug(sb.ToString());
            AddJam(actor, weapon);
            return true;
        }

        private static bool AttemptToRemoveJam(AbstractActor actor, Weapon weapon)
        {
            var skill = actor.SkillGunnery;
            var mitigationRoll = Random.Range(1, 10);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"gunneryskill: {skill}");
            sb.AppendLine($"mitigationRoll: {mitigationRoll}");
            if (skill >= mitigationRoll)
            {
                Logger.Debug(sb.ToString());
                RemoveJam(actor, weapon);
                return true;
            }

            Logger.Debug(sb.ToString());
            return false;
        }

        private const string TemporarilyDisabledStatisticName = "TemporarilyDisabled";
        private const string DamageableWeaponTag = "wr-damage_when_jam";
        internal const string JammedWeaponStatisticName = "WR-JammedWeapon";
        private const string JammableWeaponTag = "wr-jammable_weapon";

        private static void AddJam(AbstractActor actor, Weapon weapon)
        {
            if (!DamagesWhenJams(weapon))
            {
                weapon.StatCollection.Set<bool>(JammedWeaponStatisticName, true);
                weapon.StatCollection.Set<bool>(TemporarilyDisabledStatisticName, true);
                actor.Combat.MessageCenter.PublishMessage(
                    new AddSequenceToStackMessage(
                        new ShowActorInfoSequence(actor, $"{weapon.Name} Jammed!", FloatieMessage.MessageNature.Debuff,
                            true)));
            }
            else
            {
                var isDestroying = weapon.DamageLevel != ComponentDamageLevel.Functional;
                var damageLevel = isDestroying ? ComponentDamageLevel.Destroyed : ComponentDamageLevel.Penalized;
                var fakeHit = new WeaponHitInfo(-1, -1, -1, -1, string.Empty, string.Empty, -1, null, null, null, null, null, null, null, null, null, null, null);
                weapon.DamageComponent(fakeHit, damageLevel, true);
                var message = isDestroying
                    ? $"{weapon.Name} misfire: Destroyed!"
                    : $"{weapon.Name} misfire: Damaged!";
                actor.Combat.MessageCenter.PublishMessage(
                    new AddSequenceToStackMessage(
                        new ShowActorInfoSequence(actor, message, FloatieMessage.MessageNature.Debuff, true)));
            }

        }

        private static void RemoveJam(AbstractActor actor, Weapon weapon)
        {
            weapon.StatCollection.Set<bool>(JammedWeaponStatisticName, false);
            weapon.StatCollection.Set<bool>(TemporarilyDisabledStatisticName, false);
            actor.Combat.MessageCenter.PublishMessage(
                new AddSequenceToStackMessage(
                    new ShowActorInfoSequence(actor, $"{weapon.Name} Unjammed!", FloatieMessage.MessageNature.Buff,
                        true)));
        }

        private static bool IsJammed(Weapon weapon)
        {
            var statistic =
                StatisticHelper.GetOrCreateStatisic<bool>(weapon.StatCollection, JammedWeaponStatisticName, false);
            return statistic.Value<bool>();
        }

        private static bool IsJammable(Weapon weapon)
        {
            return weapon.weaponDef.ComponentTags.Contains(JammableWeaponTag) && HasJamMultiplier(weapon);
        }

        private static readonly Dictionary<string, float> JamMultipliers = new Dictionary<string, float>();

        private static bool HasJamMultiplier(Weapon weapon)
        {
            if (!JamMultipliers.ContainsKey(weapon.defId))
                JamMultipliers[weapon.defId] = ParseBaseMultiplier(weapon);
            return JamMultipliers[weapon.defId] > Calculator.Epsilon;
        }

        private static bool DamagesWhenJams(Weapon weapon)
        {
            return weapon.weaponDef.ComponentTags.Contains(DamageableWeaponTag);
        }

        private const string JamMultiplierTagPrefix = "wr-jam_chance_multiplier";
        private static readonly char[] TagDelimiter = new char[] {'-'};

        private static float ParseBaseMultiplier(Weapon weapon)
        {
            if (!weapon.weaponDef.ComponentTags.Any(tag =>
                tag.StartsWith(JamMultiplierTagPrefix, StringComparison.InvariantCultureIgnoreCase)))
                return 0.0f;
            var rawTag = weapon.weaponDef.ComponentTags.First(tag =>
                tag.StartsWith(JamMultiplierTagPrefix, StringComparison.InvariantCultureIgnoreCase));
            var multiplier =
                rawTag == JamMultiplierTagPrefix
                    ? Core.ModSettings.JamChanceMultiplier
                    : float.Parse(rawTag.Split(TagDelimiter, 3).Last());
            return multiplier;
        }
    }

    [HarmonyPatch(typeof(MechComponent), "UIName", MethodType.Getter)]
    static class JammedWeaponDisplayChanger
    {
        public static bool Prepare()
        {
            return Core.ModSettings.Jamming;
        }

        public static void Postfix(MechComponent __instance, ref Text __result)
        {
            if (!__instance.IsFunctional) return;
            if (__instance.GetType() != typeof(Weapon)) return;
            if (!StatisticHelper
                .GetOrCreateStatisic<bool>(__instance.StatCollection, JammingEnabler.JammedWeaponStatisticName, false)
                .Value<bool>()) return;
            __result.Append(" (JAM)", new object[0]);
        }
    }
}