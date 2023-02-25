using BattleTech;
using CustomAmmoCategoriesLog;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace CustAmmoCategories {
  public static partial class CustomAmmoCategories {
    public static WeaponEffect InitWeaponEffect(WeaponRepresentation weaponRepresentation, Weapon weapon, string weaponEffectId) {
      GameObject gameObject = (GameObject)null;
      WeaponEffect result = (WeaponEffect)null;
      try {
        if (!string.IsNullOrEmpty(weaponEffectId)) {
          gameObject = weaponRepresentation.parentCombatant.Combat.DataManager.PooledInstantiate(weaponEffectId, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
        }
        if ((UnityEngine.Object)gameObject == (UnityEngine.Object)null) {
          CustomAmmoCategoriesLog.Log.LogWrite(string.Format("Error instantiating WeaponEffect [{0}], Weapon [{1}]]\n", (object)weaponEffectId, (object)weapon.Name));
        } else {
          Log.O.printComponents(gameObject, 0);
          gameObject.transform.parent = weaponRepresentation.transform;
          gameObject.transform.localPosition = Vector3.zero;
          gameObject.transform.rotation = Quaternion.identity;
          result = gameObject.GetComponent<WeaponEffect>();
          if ((UnityEngine.Object)result == (UnityEngine.Object)null) {
            CustomAmmoCategoriesLog.Log.LogWrite(string.Format("Error finding WeaponEffect on GO [{0}], Weapon [{1}]\n", (object)weaponEffectId, (object)weapon.Name));
          } else {
            BallisticEffect bWE = result as BallisticEffect;
            LaserEffect lWE = result as LaserEffect;
            PPCEffect pWE = result as PPCEffect;
            LBXEffect lbWE = result as LBXEffect;
            if (bWE != null) {
              if (weapon.isImprovedBallistic()) {
                Log.LogWrite("alternate ballistic needed\n");
                MultiShotBallisticEffect msbWE = bWE.gameObject.AddComponent<MultiShotBallisticEffect>();
                msbWE.Init(bWE);
                GameObject.Destroy(bWE);
                Log.LogWrite("Alternate ballistic effect inited\n");
                result = msbWE;
              }
            } else
            if (lWE != null) {
              if (weapon.isImprovedBallistic()) {
                Log.LogWrite("alternate laser needed\n");
                MultiShotLaserEffect mslWE = lWE.gameObject.AddComponent<MultiShotLaserEffect>();
                mslWE.Init(lWE, weaponEffectId);
                GameObject.Destroy(lWE);
                Log.LogWrite("Alternate laser effect inited\n");
                result = mslWE;
              }
            } else
            if (pWE != null) {
              if (weapon.isImprovedBallistic()) {
                Log.LogWrite("alternate PPC needed\n");
                MultiShotPPCEffect mspWE = pWE.gameObject.AddComponent<MultiShotPPCEffect>();
                mspWE.Init(pWE, weaponEffectId);
                GameObject.Destroy(pWE);
                Log.LogWrite("Alternate PPC effect inited\n");
                result = mspWE;
              }
            } else
            if (lbWE != null) {
              if (weapon.isImprovedBallistic()) {
                Log.LogWrite("alternate LBX needed\n");
                MultiShotLBXBallisticEffect mlbWE = lbWE.gameObject.AddComponent<MultiShotLBXBallisticEffect>();
                mlbWE.Init(lbWE, weaponEffectId);
                GameObject.Destroy(lbWE);
                Log.LogWrite("Alternate LBX effect inited\n");
                result = mlbWE;
              }
            }
            /* else
          if (mWE != null) {
            if (weapon.isImprovedBallistic()) {
              Log.LogWrite("alternate MissileLauncher needed\n");
              MultiShotMissileLauncherEffect msmWE = mWE.gameObject.AddComponent<MultiShotMissileLauncherEffect>();
              msmWE.Init(mWE);
              GameObject.Destroy(mWE);
              Log.LogWrite("Alternate MissileLauncher effect inited\n");
              result = msmWE;
            }
          }*/
            result.Init(weapon);
          }
        }
        Log.LogWrite("Success init weapon effect " + weaponEffectId + " for " + weapon.defId + "\n");
      } catch (Exception e) {
        Log.LogWrite(e.ToString() + "\n", true);
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
          //MissileLauncherEffect mWE = __instance.WeaponEffect as MissileLauncherEffect;
          LBXEffect lbWE = __instance.WeaponEffect as LBXEffect;
          if (bWE != null) {
            if (weapon.isImprovedBallistic()) {
              CustomAmmoCategoriesLog.Log.LogWrite("alternate ballistic needed\n");
              MultiShotBallisticEffect msbWE = bWE.gameObject.AddComponent<MultiShotBallisticEffect>();
              msbWE.Init(bWE);
              GameObject.Destroy(bWE);
              typeof(WeaponRepresentation).GetField("weaponEffect", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, msbWE);
              msbWE.Init(__instance.weapon);
              CustomAmmoCategoriesLog.Log.LogWrite("Alternate ballistic effect inited\n");
            }
          } else
          if (lWE != null) {
            if (weapon.isImprovedBallistic()) {
              Log.LogWrite("alternate laser needed\n");
              MultiShotLaserEffect mslWE = lWE.gameObject.AddComponent<MultiShotLaserEffect>();
              mslWE.Init(lWE, weapon.weaponDef.WeaponEffectID);
              GameObject.Destroy(lWE);
              typeof(WeaponRepresentation).GetField("weaponEffect", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, mslWE);
              mslWE.Init(__instance.weapon);
              Log.LogWrite("Alternate laser effect inited\n");
            }
          } else
          if (pWE != null) {
            if (weapon.isImprovedBallistic()) {
              Log.LogWrite("alternate PPC needed\n");
              MultiShotPPCEffect mspWE = pWE.gameObject.AddComponent<MultiShotPPCEffect>();
              mspWE.Init(pWE, weapon.weaponDef.WeaponEffectID);
              GameObject.Destroy(pWE);
              typeof(WeaponRepresentation).GetField("weaponEffect", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, mspWE);
              mspWE.Init(__instance.weapon);
              Log.LogWrite("Alternate PPC effect inited\n");
            }
          } else
          if (lbWE != null) {
            if (weapon.isImprovedBallistic()) {
              Log.LogWrite("alternate LBX needed\n");
              MultiShotLBXBallisticEffect mlbWE = lbWE.gameObject.AddComponent<MultiShotLBXBallisticEffect>();
              mlbWE.Init(lbWE, weapon.weaponDef.WeaponEffectID);
              GameObject.Destroy(lbWE);
              typeof(WeaponRepresentation).GetField("weaponEffect", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, lbWE);
              mlbWE.Init(__instance.weapon);
              Log.LogWrite("Alternate LBX effect inited\n");
            }
          }
          /* else
        if (mWE != null) {
          if (weapon.isImprovedBallistic()) {
            Log.LogWrite("alternate MissileLauncher needed\n");
            MultiShotMissileLauncherEffect msmWE = mWE.gameObject.AddComponent<MultiShotMissileLauncherEffect>();
            msmWE.Init(mWE);
            GameObject.Destroy(mWE);
            typeof(WeaponRepresentation).GetField("weaponEffect", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, msmWE);
            msmWE.Init(__instance.weapon);
            Log.LogWrite("Alternate MissileLauncher effect inited\n");
          }
        }*/
        }
        //CustomAmmoCategories.ClearWeaponShellEffects(wGUID);
        CustomAmmoCategories.InitWeaponEffects(__instance, weapon);
        //CustomAmmoCategories.registerShellsEffects(__instance, weapon);
      } catch (Exception e) {
        Log.LogWrite(e.ToString() + "\n", true);
      }
    }
  }
}