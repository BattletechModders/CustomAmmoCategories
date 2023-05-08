using BattleTech;
using CustomAmmoCategoriesLog;
using HarmonyLib;
using HBS.Logging;
using System;
using System.Reflection;
using UnityEngine;

namespace CustAmmoCategories {
  public static partial class CustomAmmoCategories {
    public static readonly ILog AttackSequence_logger = HBS.Logging.Logger.GetLogger("CombatLog.AttackSequence");
    public static WeaponEffect InitWeaponEffect(WeaponRepresentation weaponRepresentation, Weapon weapon, string weaponEffectId) {
      GameObject gameObject = null;
      WeaponEffect result = null;
      try {
        if (!string.IsNullOrEmpty(weaponEffectId)) {
          gameObject = weaponRepresentation.parentCombatant.Combat.DataManager.PooledInstantiate(weaponEffectId, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
        }
        if (gameObject == null) {
          Log.Combat?.WL(0, $"Error instantiating WeaponEffect [{weaponEffectId}], Weapon [{weapon.Name}]]");
          AttackSequence_logger.LogError($"Exception: instantiating WeaponEffect [{weaponEffectId}], Weapon [{weapon.Name}]]");
        } else {
          Log.O.printComponents(gameObject, 0);
          gameObject.transform.parent = weaponRepresentation.transform;
          gameObject.transform.localPosition = Vector3.zero;
          gameObject.transform.rotation = Quaternion.identity;
          result = gameObject.GetComponent<WeaponEffect>();
          if (result == null) {
            Log.Combat?.WL(0, string.Format("Error finding WeaponEffect on GO [{0}], Weapon [{1}]\n", (object)weaponEffectId, (object)weapon.Name));
          } else {
            BallisticEffect bWE = result as BallisticEffect;
            LaserEffect lWE = result as LaserEffect;
            PPCEffect pWE = result as PPCEffect;
            LBXEffect lbWE = result as LBXEffect;
            if (bWE != null) {
              if (weapon.isImprovedBallistic()) {
                //Log.Combat?.WL(0, "alternate ballistic needed");
                MultiShotBallisticEffect msbWE = bWE.gameObject.AddComponent<MultiShotBallisticEffect>();
                msbWE.Init(bWE);
                GameObject.Destroy(bWE);
                //Log.Combat?.WL(0, "Alternate ballistic effect inited");
                result = msbWE;
              }
            } else
            if (lWE != null) {
              if (weapon.isImprovedBallistic()) {
                //Log.Combat?.WL(0, "alternate laser needed");
                MultiShotLaserEffect mslWE = lWE.gameObject.AddComponent<MultiShotLaserEffect>();
                mslWE.Init(lWE, weaponEffectId);
                GameObject.Destroy(lWE);
                //Log.Combat?.WL(0, "Alternate laser effect inited");
                result = mslWE;
              }
            } else
            if (pWE != null) {
              if (weapon.isImprovedBallistic()) {
                //Log.Combat?.WL(0, "alternate PPC needed");
                MultiShotPPCEffect mspWE = pWE.gameObject.AddComponent<MultiShotPPCEffect>();
                mspWE.Init(pWE, weaponEffectId);
                GameObject.Destroy(pWE);
                //Log.Combat?.WL(0, "Alternate PPC effect inited");
                result = mspWE;
              }
            } else
            if (lbWE != null) {
              if (weapon.isImprovedBallistic()) {
                //Log.Combat?.WL(0, "alternate LBX needed");
                MultiShotLBXBallisticEffect mlbWE = lbWE.gameObject.AddComponent<MultiShotLBXBallisticEffect>();
                mlbWE.Init(lbWE, weaponEffectId);
                GameObject.Destroy(lbWE);
                //Log.Combat?.WL(0, "Alternate LBX effect inited");
                result = mlbWE;
              }
            }
            result.Init(weapon);
          }
        }
        //Log.Combat?.WL(0, $"Success init weapon effect {weaponEffectId} for {weapon.defId}");
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        AttackSequence_logger.LogException(e);
      }
      return result;
    }
  }
  [HarmonyPatch(typeof(WeaponRepresentation))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Weapon), typeof(Transform), typeof(bool), typeof(string), typeof(int) })]
  public static class WeaponRepresentation_Init {
    public static void Postfix(WeaponRepresentation __instance, Weapon weapon, Transform parentTransform, bool isParented, string parentDisplayName, int mountedLocation) {
      try {
        string wGUID;
        if (weapon.StatCollection.ContainsStatistic(CustomAmmoCategories.GUIDStatisticName) == false) {
          wGUID = Guid.NewGuid().ToString();
          weapon.StatCollection.AddStatistic<string>(CustomAmmoCategories.GUIDStatisticName, wGUID);
        } else {
          wGUID = weapon.StatCollection.GetStatistic(CustomAmmoCategories.GUIDStatisticName).Value<string>();
        }
        CustomAmmoCategories.ClearWeaponEffects(wGUID);
        if (__instance.WeaponEffect != null) {
          Log.O.printComponents(__instance.WeaponEffect.gameObject,0);
          BallisticEffect bWE = __instance.WeaponEffect as BallisticEffect;
          LaserEffect lWE = __instance.WeaponEffect as LaserEffect;
          PPCEffect pWE = __instance.WeaponEffect as PPCEffect;
          LBXEffect lbWE = __instance.WeaponEffect as LBXEffect;
          if (bWE != null) {
            if (weapon.isImprovedBallistic()) {
              //Log.Combat?.WL(0, "alternate ballistic needed");
              MultiShotBallisticEffect msbWE = bWE.gameObject.AddComponent<MultiShotBallisticEffect>();
              msbWE.Init(bWE);
              GameObject.Destroy(bWE);
              __instance.weaponEffect = msbWE;
              msbWE.Init(__instance.weapon);
              //Log.Combat?.WL(0, "Alternate ballistic effect inited");
            }
          } else
          if (lWE != null) {
            if (weapon.isImprovedBallistic()) {
              //Log.Combat?.WL(0, "alternate laser needed");
              MultiShotLaserEffect mslWE = lWE.gameObject.AddComponent<MultiShotLaserEffect>();
              mslWE.Init(lWE, weapon.weaponDef.WeaponEffectID);
              GameObject.Destroy(lWE);
              __instance.weaponEffect = mslWE;
              mslWE.Init(__instance.weapon);
              //Log.Combat?.WL(0, "Alternate laser effect inited");
            }
          } else
          if (pWE != null) {
            if (weapon.isImprovedBallistic()) {
              //Log.Combat?.WL(0, "alternate PPC needed");
              MultiShotPPCEffect mspWE = pWE.gameObject.AddComponent<MultiShotPPCEffect>();
              mspWE.Init(pWE, weapon.weaponDef.WeaponEffectID);
              GameObject.Destroy(pWE);
              __instance.weaponEffect = mspWE;
              mspWE.Init(__instance.weapon);
              //Log.Combat?.WL(0, "Alternate PPC effect inited");
            }
          } else
          if (lbWE != null) {
            if (weapon.isImprovedBallistic()) {
              //Log.Combat?.WL(0, "alternate LBX needed");
              MultiShotLBXBallisticEffect mlbWE = lbWE.gameObject.AddComponent<MultiShotLBXBallisticEffect>();
              mlbWE.Init(lbWE, weapon.weaponDef.WeaponEffectID);
              GameObject.Destroy(lbWE);
              __instance.weaponEffect = mlbWE;
              mlbWE.Init(__instance.weapon);
              //Log.Combat?.WL(0, "Alternate LBX effect inited");
            }
          }
        }
        CustomAmmoCategories.InitWeaponEffects(__instance, weapon);
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        CustomAmmoCategories.AttackSequence_logger.LogException(e);
      }
    }
  }
}