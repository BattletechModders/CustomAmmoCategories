using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Harmony;

namespace WeaponRealizer
{
    [HarmonyPatch(typeof(BallisticEffect), "OnComplete", MethodType.Normal)]
    [HarmonyPatch(new Type[] {})]
    static class BallisticEffectOnCompleteMultifirePatch
    {
        private static Action<BallisticEffect> WeaponEffect_OnComplete;
        private static FastInvokeHandler BallisticEffect_OnImpact;
        public static bool Prepare()
        {
            if (!Core.ModSettings.BallisticNumberOfShots && !Core.ModSettings.ClusteredBallistics)
                return false;
            BuildWeaponEffectOnComplete();
            BuildBallisticEffectOnImpact();
            return true;
        }

        private static void BuildWeaponEffectOnComplete()
        {
            // build a call to WeaponEffect.OnComplete() so it can be called
            // a la base.OnComplete() from the context of a BallisticEffect
            // https://blogs.msdn.microsoft.com/rmbyers/2008/08/16/invoking-a-virtual-method-non-virtually/
            // https://docs.microsoft.com/en-us/dotnet/api/system.activator?view=netframework-3.5
            // https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.dynamicmethod.-ctor?view=netframework-3.5#System_Reflection_Emit_DynamicMethod__ctor_System_String_System_Type_System_Type___System_Type_
            // https://stackoverflow.com/a/4358250/1976
            var method = typeof(WeaponEffect).GetMethod("OnComplete", AccessTools.all);
            var dm = new DynamicMethod("WRWeaponEffectOnComplete", null, new Type[] {typeof(BallisticEffect)}, typeof(BallisticEffect));
            var gen = dm.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, method);
            gen.Emit(OpCodes.Ret);
            WeaponEffect_OnComplete = (Action<BallisticEffect>) dm.CreateDelegate(typeof(Action<BallisticEffect>));
        }

        private static void BuildBallisticEffectOnImpact()
        {
            // OnImpact is protected
            var mi = AccessTools.Method(typeof(BallisticEffect), "OnImpact", new Type[] {typeof(float)});
            BallisticEffect_OnImpact = MethodInvoker.GetHandler(mi);
        }

        static bool Prefix(ref int ___hitIndex, BallisticEffect __instance)
        {
            var damage = __instance.weapon.DamagePerShotAdjusted(__instance.weapon.parent.occupiedDesignMask);
            BallisticEffect_OnImpact.Invoke(__instance, new object[] {damage});
            if (___hitIndex >= __instance.hitInfo.toHitRolls.Length - 1)
            {
                WeaponEffect_OnComplete(__instance);
                return false;
            }

            ___hitIndex++;
            if (ShouldFire(__instance, ___hitIndex))
            {
                __instance.Fire(__instance.hitInfo, ___hitIndex, 0);
            }
            return false;
        }

        // we only fire when multishot ballistics are enabled and we're not in clustered mode
        private static bool ShouldFire(BallisticEffect effect, int hitIndex)
        {
            return Core.ModSettings.BallisticNumberOfShots &&
                   hitIndex % effect.weapon.ProjectilesPerShot == 0;
        }

        private static readonly Dictionary<int, bool> _isClustered = new Dictionary<int, bool>();
        private static bool IsClustered(BallisticEffect effect)
        {
            var effectId = effect.GetInstanceID();
            if (!_isClustered.ContainsKey(effectId))
            {
                _isClustered[effectId] =
                    Core.ModSettings.ClusteredBallistics &&
                    effect.weapon.weaponDef.ComponentTags.Contains(ClusteredShotEnabler.CLUSTER_TAG);
            }
            return _isClustered[effectId];
        }
    }

    [HarmonyPatch(typeof(LaserEffect), "OnComplete", MethodType.Normal)]
    [HarmonyPatch(new Type[] {})]
    static class LaserEffectOnCompleteMultifirePatch
    {
        public static bool Prepare()
        {
            return Core.ModSettings.LaserNumberOfShots;
        }

        static void Postfix(ref int ___hitIndex, LaserEffect __instance)
        {
            if (___hitIndex >= __instance.hitInfo.numberOfShots - 1) return;
            ___hitIndex++;
            __instance.Fire(__instance.hitInfo, ___hitIndex);
        }
    }


    // HAHA: patching a shot right into the end of Update didn't work
    // HAHA: patching weapon representation to call fire number of shots times doesn't work
    
    // how-to?
    // if the laser has 1 shot, don't prefix shit
    // if the laser has more than one shot, then we need to find everywhere that interacts with the laser
    //   animation, and use arrays for each aspect
    // for laser color, we need to edit
    // before fire, setup more data structure like firing interval
    //
}