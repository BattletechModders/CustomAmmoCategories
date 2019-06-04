using BattleTech;
using Harmony;
using UnityEngine;

namespace WeaponRealizer
{
    [HarmonyPatch(typeof(Weapon), nameof(Weapon.RefireModifier), MethodType.Getter)]
    static class DamageAltersWeaponRefireModifier
    {
        static bool Prepare()
        {
            return Core.ModSettings.DamageAltersWeaponRefireModifier &&
                   Core.ModSettings.DamagedWeaponRefireModifierMultiplier != 1;
        }

        static void Postfix(Weapon __instance, ref int __result)
        {
            if (__instance.DamageLevel > ComponentDamageLevel.Functional)
                __result = Mathf.CeilToInt(__result * Core.ModSettings.DamagedWeaponRefireModifierMultiplier);
        }
    }
}