using BattleTech;
using CustAmmoCategories;
using CustomAmmoCategoriesLog;
using Harmony;
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
      int thishitIndex = (int)typeof(WeaponEffect).GetField("hitIndex", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
      Log.LogWrite("MissileEffect.Fire "+ hitInfo.attackWeaponIndex + "-"+ thishitIndex + "\n");
      AdvWeaponHitInfoRec advRec = __instance.hitInfo.advRec(hitIndex);
      if (advRec == null) {
        Log.LogWrite(" no advanced record.");
        return;
      }
      if (advRec.fragInfo.separated && (advRec.fragInfo.fragStartHitIndex >= 0) && (advRec.fragInfo.fragsCount > 0)) {
        Log.LogWrite(" frag projectile separated.");
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
    public static bool Prefix(WeaponEffect __instance) {
      int hitIndex = (int)typeof(WeaponEffect).GetField("hitIndex", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
      FragWeaponEffect fWe = __instance.fragEffect();
      if (fWe != null) {
        Log.LogWrite("WeaponEffect.PublishWeaponCompleteMessage " + __instance.hitInfo.attackWeaponIndex + ":" + hitIndex + " has frag sub effect. Complete:" + fWe.FiringComplete+"\n");
        return fWe.FiringComplete;
      }
      return true;
    }
    public static void Postfix(WeaponEffect __instance) {
      int hitIndex = (int)typeof(WeaponEffect).GetField("hitIndex", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
      FragWeaponEffect fWe = __instance.fragEffect();
      if (fWe != null) {
        Log.LogWrite("WeaponEffect.PublishWeaponCompleteMessage " + __instance.hitInfo.attackWeaponIndex + ":" + hitIndex + " has frag sub effect. Complete:" + fWe.FiringComplete + "\n");
        if(fWe.FiringComplete == true) {
          Log.LogWrite(" unregistring frag effect\n");
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
      Log.LogWrite("RegisterFragWeaponEffect for "+we.gameObject.name+" "+ prefabName + "\n");
      if (CustomAmmoCategories.fragWeaponEffects.ContainsKey(we)) { return; }
      GameObject gameObject = we.weapon.parent.Combat.DataManager.PooledInstantiate(prefabName, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
      FragBallisticEffect fbWE = null;
      if (gameObject != null) {
        Log.LogWrite(" getted from pool: " + gameObject.GetInstanceID() + "\n");
        fbWE = gameObject.GetComponent<FragBallisticEffect>();
        if (fbWE != null) {
          fbWE.parentWeaponEffect = we;
          fbWE.Init(we.weapon);
          CustomAmmoCategories.fragWeaponEffects.Add(we, fbWE);
        }
      }
      if (fbWE == null) {
        Log.LogWrite(" not in pool. instansing.\n");
        GameObject ogameObject = we.weapon.parent.Combat.DataManager.PooledInstantiate("WeaponEffect-Weapon_AC2", BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
        if ((UnityEngine.Object)ogameObject == (UnityEngine.Object)null) {
          CustomAmmoCategoriesLog.Log.LogWrite(string.Format("Error instantiating WeaponEffect [{0}], Weapon [{1}]]\n", (object)"WeaponEffect-Weapon_AC2", (object)we.weapon.Name));
        } else {
          gameObject = GameObject.Instantiate(ogameObject);
          GameObject.Destroy(ogameObject);
          gameObject.transform.parent = we.weaponRep.transform;
          gameObject.transform.localPosition = Vector3.zero;
          gameObject.transform.rotation = Quaternion.identity;
          WeaponEffect result = gameObject.GetComponent<WeaponEffect>();
          if ((UnityEngine.Object)result == (UnityEngine.Object)null) {
            Log.LogWrite(string.Format("Error finding WeaponEffect on GO [{0}], Weapon [{1}]\n", (object)"WeaponEffect-Weapon_AC2", (object)we.weapon.Name));
          } else {
            BallisticEffect bWE = result as BallisticEffect;
            if (bWE != null) {
              Log.LogWrite("Found ballistic " + "WeaponEffect-Weapon_AC2" + "/" + we.weapon.Name + "\n");
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
        Log.LogWrite("unregisterFragEffect. Returning to pool: "+prefabName+" "+fWe.gameObject+"\n");
        we.weapon.parent.Combat.DataManager.PoolGameObject(prefabName, fWe.gameObject);
        //GameObject.Destroy(fWe.gameObject);
      }
    }
    public class FragMainEffect : MonoBehaviour {
    }
  }
}
