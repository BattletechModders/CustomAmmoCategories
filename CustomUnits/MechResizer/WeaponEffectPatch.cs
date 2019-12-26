using BattleTech;
using Harmony;
using UnityEngine;
using static MechResizer.MechResizer;

namespace MechResizer
{
    [HarmonyPatch(typeof(WeaponEffect), "PlayProjectile")]
    public static class WeaponEffectPlayProjectilePatch
    {
        static bool Prefix(WeaponEffect __instance)
        {
            var multiplier = SizeMultiplier.Get(__instance.weapon.weaponDef);
            var ogTransform = Traverse.Create(__instance).Field("projectileTransform").GetValue<Transform>();
            ogTransform.localScale = multiplier;
            Traverse.Create(__instance).Field("projectileTransform").SetValue(ogTransform);
            return true;
        }
    }

//    [HarmonyPatch(typeof(WeaponEffect), "PlayMuzzleFlash")]
//    public static class WeaponEffectPlayMuzzleFlashPatch
//    {
//        static bool Prefix(WeaponEffect __instance)
//        {
//            Logger.Debug("my boyfriend don't need em");
//            if (__instance.muzzleFlashVFXPrefab != null)
//            {
//                Logger.Debug("woo?");
//                GameObject gameObject = __instance.weapon.parent.Combat.DataManager.PooledInstantiate(__instance.muzzleFlashVFXPrefab.name, BattleTechResourceType.Prefab, null, null, null);
//                ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
//                AutoPoolObject autoPoolObject = gameObject.GetComponent<AutoPoolObject>();
//                if (autoPoolObject == null)
//                {
//                    autoPoolObject = gameObject.AddComponent<AutoPoolObject>();
//                }
//                autoPoolObject.Init(__instance.weapon.parent.Combat.DataManager, __instance.muzzleFlashVFXPrefab.name, component);
//                component.Stop(true);
//                component.Clear(true);
//                var startingTransform = Traverse.Create(__instance).Field("startingTransform").GetValue<Transform>();
//                //component.transform.parent = __instance.startingTransform;
//                component.scalingMode = ParticleSystemScalingMode.Hierarchy;
//                component.transform.parent = startingTransform;
//                component.transform.localPosition = Vector3.zero;
//                component.transform.LookAt(Traverse.Create(__instance).Field("endPos").GetValue<Vector3>());
//                component.transform.localScale = new Vector3(10f, 10f, 10f);
//                BTCustomRenderer.SetVFXMultiplier(component);
//                component.Play(true);
//                BTLightAnimator componentInChildren = gameObject.GetComponentInChildren<BTLightAnimator>(true);
//                if (componentInChildren != null)
//                {
//                    componentInChildren.StopAnimation();
//                    componentInChildren.PlayAnimation();
//                }
//            }
//
//            return false;
////            if (__instance.muzzleFlashVFXPrefab != null)
////            {
////                Logger.Debug("woo?");
////                var ogTransform = Traverse.Create(__instance).Field("projectileTransform").GetValue<Transform>();
////                ogTransform.localScale = new Vector3(100f,100f,100f);
////                Traverse.Create(__instance).Field("projectileTransform").SetValue(ogTransform);
////
////            }
////            return true;
//        }
//    }
}