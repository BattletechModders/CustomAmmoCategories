/*  
 *  This file is part of CustomAmmoCategories.
 *  CustomAmmoCategories is free software: you can redistribute it and/or modify it under the terms of the 
 *  GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, 
 *  or (at your option) any later version. CustomAmmoCategories is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 *  See the GNU Lesser General Public License for more details.
 *  You should have received a copy of the GNU Lesser General Public License along with CustomAmmoCategories. 
 *  If not, see <https://www.gnu.org/licenses/>. 
*/
using BattleTech;
using CustAmmoCategories;
using CustomAmmoCategoriesLog;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace CustAmmoCategoriesPatches {
  [HarmonyPatch(typeof(MissileEffect))]
  [HarmonyPatch("Fire")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(WeaponHitInfo), typeof(int), typeof(int), typeof(bool) })]
  public static class MissileEffect_FireShells {
    public static void Postfix(MissileEffect __instance, ref WeaponHitInfo hitInfo, int hitIndex, int emitterIndex, bool isIndirect) {
      int thishitIndex = __instance.hitIndex;
      Log.Combat?.WL(0, "MissileEffect.Fire " + hitInfo.attackWeaponIndex + "-"+ thishitIndex);
      AdvWeaponHitInfoRec advRec = __instance.hitInfo.advRec(hitIndex);
      if (advRec == null) {
        Log.Combat?.WL(1, "no advanced record.");
        return;
      }
      if (advRec.fragInfo.separated && (advRec.fragInfo.fragStartHitIndex >= 0) && (advRec.fragInfo.fragsCount > 0)) {
        Log.Combat?.WL(1, "frag projectile separated.");
        __instance.RegisterFragWeaponEffect();
      }
      return;
    }
  }
  [HarmonyPatch(typeof(WeaponEffect))]
  [HarmonyPatch("PublishWeaponCompleteMessage")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class WeaponEffect_PublishWeaponCompleteMessage {
    public static void Prefix(ref bool __runOriginal,WeaponEffect __instance) {
      if (!__runOriginal) { return; }
      int hitIndex = __instance.hitIndex;
      FragWeaponEffect fWe = __instance.fragEffect();
      if (fWe != null) {
        Log.Combat?.WL(0, "WeaponEffect.PublishWeaponCompleteMessage " + __instance.hitInfo.attackWeaponIndex + ":" + hitIndex + " has frag sub effect. Complete:" + fWe.FiringComplete);
        __runOriginal = fWe.FiringComplete; return;
      }
      return;
    }
    public static void Postfix(WeaponEffect __instance) {
      int hitIndex = __instance.hitIndex;
      FragWeaponEffect fWe = __instance.fragEffect();
      if (fWe != null) {
        Log.Combat?.WL(0, "WeaponEffect.PublishWeaponCompleteMessage " + __instance.hitInfo.attackWeaponIndex + ":" + hitIndex + " has frag sub effect. Complete:" + fWe.FiringComplete);
        if(fWe.FiringComplete == true) {
          Log.Combat?.WL(1, "unregistring frag effect");
          __instance.unregisterFragEffect();
        }
      }
    }
  }
}

namespace CustAmmoCategories {
  public static partial class CustomAmmoCategories {
    private static Dictionary<WeaponEffect, FragWeaponEffect> fragWeaponEffects = new Dictionary<WeaponEffect, FragWeaponEffect>();
    public static readonly string FragPrefabMainPrefabPrefix = "_FRAGMAIN_";
    public static readonly string FragPrefabMainPrefab = "WeaponEffect-Weapon_AC2";
    public static void RegisterFragWeaponEffect(this WeaponEffect we) {
      string prefabName = CustomAmmoCategories.FragPrefabMainPrefabPrefix + CustomAmmoCategories.FragPrefabMainPrefab;
      Log.Combat?.WL(0, "RegisterFragWeaponEffect for "+we.gameObject.name+" "+ prefabName);
      if (CustomAmmoCategories.fragWeaponEffects.ContainsKey(we)) { return; }
      GameObject gameObject = we.weapon.parent.Combat.DataManager.PooledInstantiate(prefabName, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
      FragBallisticEffect fbWE = null;
      if (gameObject != null) {
        Log.Combat?.WL(1, "getted from pool: " + gameObject.GetInstanceID());
        fbWE = gameObject.GetComponent<FragBallisticEffect>();
        if (fbWE != null) {
          fbWE.parentWeaponEffect = we;
          fbWE.Init(we.weapon);
          CustomAmmoCategories.fragWeaponEffects.Add(we, fbWE);
        }
      }
      if (fbWE == null) {
        Log.Combat?.WL(1, "not in pool. instansing.");
        GameObject ogameObject = we.weapon.parent.Combat.DataManager.PooledInstantiate("WeaponEffect-Weapon_AC2", BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
        if (ogameObject == null) {
          Log.Combat?.WL(0, string.Format("Error instantiating WeaponEffect [{0}], Weapon [{1}]]", (object)"WeaponEffect-Weapon_AC2", (object)we.weapon.Name));
        } else {
          gameObject = GameObject.Instantiate(ogameObject);
          GameObject.Destroy(ogameObject);
          gameObject.transform.parent = we.weaponRep.transform;
          gameObject.transform.localPosition = Vector3.zero;
          gameObject.transform.rotation = Quaternion.identity;
          WeaponEffect result = gameObject.GetComponent<WeaponEffect>();
          if (result == null) {
            Log.Combat?.WL(0, string.Format("Error finding WeaponEffect on GO [{0}], Weapon [{1}]\n", (object)"WeaponEffect-Weapon_AC2", (object)we.weapon.Name));
          } else {
            BallisticEffect bWE = result as BallisticEffect;
            if (bWE != null) {
              Log.Combat?.WL(0, "Found ballistic " + "WeaponEffect-Weapon_AC2" + "/" + we.weapon.Name);
              bWE.Init(we.weapon);
              fbWE = gameObject.AddComponent<FragBallisticEffect>();
              fbWE.Init(bWE);
              fbWE.parentWeaponEffect = we;
              GameObject.Destroy(result);
              result = null;
              fbWE.Init(we.weapon);
              CustomAmmoCategories.fragWeaponEffects.Add(we, fbWE);
            }
          }
        }
      }
    }
    public static FragWeaponEffect fragEffect(this WeaponEffect we) {
      if (CustomAmmoCategories.fragWeaponEffects.ContainsKey(we)) {
        return CustomAmmoCategories.fragWeaponEffects[we];
      }
      return null;
    }
    public static void unregisterFragEffect(this WeaponEffect we) {
      if (CustomAmmoCategories.fragWeaponEffects.ContainsKey(we)) {
        FragWeaponEffect fWe = CustomAmmoCategories.fragWeaponEffects[we];
        CustomAmmoCategories.fragWeaponEffects.Remove(we);
        string prefabName = CustomAmmoCategories.FragPrefabMainPrefabPrefix + CustomAmmoCategories.FragPrefabMainPrefab;
        fWe.Reset();
        Log.Combat?.WL(0, "unregisterFragEffect. Returning to pool: " + prefabName+" "+fWe.gameObject);
        we.weapon.parent.Combat.DataManager.PoolGameObject(prefabName, fWe.gameObject);
        //GameObject.Destroy(fWe.gameObject);
      }
    }
    public class FragMainEffect : MonoBehaviour {
    }
  }
}
