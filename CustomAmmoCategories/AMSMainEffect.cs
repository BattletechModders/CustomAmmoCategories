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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using CustomAmmoCategoriesLog;
using HarmonyLib;
using CustAmmoCategories;

namespace CustAmmoCategories {
  public static class AMSWeaponEffectStaticHelper {
    private static Dictionary<Weapon, AMSMainEffect> amsEffects = new Dictionary<Weapon, AMSMainEffect>();
    public static void InitAMS(this Weapon weapon) {
      if (AMSWeaponEffectStaticHelper.amsEffects.ContainsKey(weapon) == false) {
        AMSMainEffect amsMain = new AMSMainEffect(weapon);
        AMSWeaponEffectStaticHelper.amsEffects.Add(weapon, amsMain);
      } else {
        Log.LogWrite("  "+weapon.defId+" already inited as AMS");
      }
    }
    public static bool canBeAMS(this Weapon weapon) {
      WeaponExtendedInfo info = weapon.info();
      if (info.extDef.IsAMS == TripleBoolean.True) { return true; }
      foreach(var mode in info.modes) {
        if (mode.Value.IsAMS == TripleBoolean.True) { return true; }
      }
      foreach (var ammobox in weapon.ammoBoxes) {
        ExtAmmunitionDef ammo = CustomAmmoCategories.findExtAmmo(ammobox.ammoDef.Description.Id);
        if (ammo.IsAMS == TripleBoolean.True) { return true; }
      }
      return false;
      //return extWeapon.IsAAMS ==  || extWeapon.IsAMS;
    }
    public static bool isAMS(this Weapon weapon) {
      //Log.LogWrite(weapon.UIName + ".isAMS\n");
      ExtWeaponDef extWeapon = weapon.exDef();
      WeaponMode mode = weapon.mode();
      //Log.LogWrite(" mode:"+mode.UIName+"\n");
      ExtAmmunitionDef ammo = weapon.ammo();
      //Log.LogWrite(" ammoCat:" + ammo.AmmoCategory.ToString() + "\n");
      //Log.LogWrite(" mode ams override:" + mode.IsAMS + "/" + mode.IsAAMS + "\n");
      //Log.LogWrite(" ammo ams override:" + ammo.IsAMS + "/" + ammo.IsAAMS + "\n");
      //Log.LogWrite(" weapon override:" + extWeapon.IsAMS + "/" + extWeapon.IsAAMS + "\n");
      if ((mode.IsAMS != TripleBoolean.NotSet) || (mode.IsAAMS != TripleBoolean.NotSet)) {
        if (mode.IsAAMS == TripleBoolean.True) { return true; }
        if (mode.IsAMS == TripleBoolean.True) { return true; }
        return false;
      }
      if ((ammo.IsAMS != TripleBoolean.NotSet) || (ammo.IsAAMS != TripleBoolean.NotSet)) {
        if (ammo.IsAAMS == TripleBoolean.True) { return true; }
        if (ammo.IsAMS == TripleBoolean.True) { return true; }
        return false;
      }
      if ((extWeapon.IsAMS != TripleBoolean.NotSet) || (extWeapon.IsAAMS != TripleBoolean.NotSet)) {
        if (extWeapon.IsAAMS == TripleBoolean.True) { return true; }
        if (extWeapon.IsAMS == TripleBoolean.True) { return true; }
        return false;
      }
      return false;
    }
    public static bool isAAMS(this Weapon weapon) {
      ExtWeaponDef extWeapon = weapon.exDef();
      WeaponMode mode = weapon.mode();
      ExtAmmunitionDef ammo = weapon.ammo();
      if (mode.IsAAMS != TripleBoolean.NotSet) {
        if (mode.IsAAMS == TripleBoolean.True) { return true; }
        return false;
      }
      if (ammo.IsAAMS != TripleBoolean.NotSet) {
        if (ammo.IsAAMS == TripleBoolean.True) { return true; }
        return false;
      }
      if (extWeapon.IsAAMS != TripleBoolean.NotSet) {
        if (extWeapon.IsAAMS == TripleBoolean.True) { return true; }
        return false;
      }
      return false;
    }
    public static AMSMainEffect AMS(this Weapon weapon) {
      if (AMSWeaponEffectStaticHelper.amsEffects.ContainsKey(weapon) == false) {weapon.InitAMS();}
      return AMSWeaponEffectStaticHelper.amsEffects[weapon];
    }
    public static AMSWeaponEffect InitAMSWeaponEffect(this Weapon weapon) {
      string prefabName = AMSMultiShotWeaponEffect.AMSPrefabPrefix + weapon.getWeaponEffectID();
      Log.LogWrite("AMSWeaponEffect.InitAMSWeaponEffect getting from pool:" + prefabName + "\n");
      GameObject AMSgameObject = weapon.parent.Combat.DataManager.PooledInstantiate(prefabName, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
      AMSWeaponEffect amsComponent = null;
      if (AMSgameObject != null) {
        Log.LogWrite(" getted from pool: " + AMSgameObject.GetInstanceID() + "\n");
        amsComponent = AMSgameObject.GetComponent<AMSWeaponEffect>();
        if (amsComponent != null) {
          amsComponent.Init(weapon);
          return amsComponent;
        }
      }
      Log.LogWrite(" not in pool. instansing.\n");
      if (weapon.weaponRep == null) {
        Log.LogWrite("WARNING! Has no weapon representation no AMS effects!\n");
        return null;
      }
      GameObject gameObject = weapon.parent.Combat.DataManager.PooledInstantiate(weapon.getWeaponEffectID(), BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
      if (gameObject == null) {
        Log.LogWrite("WARNING! Fail to instantine "+ weapon.getWeaponEffectID() + "!\n");
        return null;
      }
      WeaponEffect we = gameObject.GetComponent<WeaponEffect>();
      if (we == null) {
        Log.LogWrite("WARNING! Has no weapon effect. No main weapon effect no AMS fire effect!\n");
        return null;
      }
      we.Init(weapon);
      gameObject = GameObject.Instantiate(we.gameObject);
      weapon.parent.Combat.DataManager.PoolGameObject(weapon.getWeaponEffectID(), we.gameObject);
      AMSWeaponEffect amsWE = gameObject.GetComponent<AMSWeaponEffect>();
      if (amsWE != null) {
        Log.LogWrite("WARNING! AMS weapon effect already installed!This shouldn't happend!\n");
        return amsWE;
      }
      we = gameObject.GetComponent<WeaponEffect>();
      BurstBallisticEffect bbWE = we as BurstBallisticEffect;
      BallisticEffect bWE = we as BallisticEffect;
      LaserEffect lWE = we as LaserEffect;
      MissileLauncherEffect mlWE = we as MissileLauncherEffect;
      if (bbWE != null) {
        Log.LogWrite("AMS burst ballistic\n");
        AMSBurstBallisticEffect AMSbbWE = gameObject.AddComponent<AMSBurstBallisticEffect>();
        AMSbbWE.Init(bbWE);
        AMSbbWE.Init(weapon);
        GameObject.Destroy(we);
        return AMSbbWE;
      } else
      if (bWE != null) {
        Log.LogWrite("AMS ballistic\n");
        AMSBallisticEffect AMSbWE = gameObject.AddComponent<AMSBallisticEffect>();
        AMSbWE.Init(bWE);
        AMSbWE.Init(weapon);
        GameObject.Destroy(we);
        return AMSbWE;
      } else 
      if(lWE != null) {
        Log.LogWrite("AMS laser\n");
        AMSLaserEffect AMSlWE = gameObject.AddComponent<AMSLaserEffect>();
        AMSlWE.Init(lWE);
        AMSlWE.Init(weapon);
        GameObject.Destroy(we);
        return AMSlWE;
      } else
      if (mlWE != null) {
        Log.LogWrite("AMS missile launcher\n");
        AMSMissileLauncherEffect AMSmlWE = gameObject.AddComponent<AMSMissileLauncherEffect>();
        AMSmlWE.Init(mlWE);
        AMSmlWE.Init(weapon);
        GameObject.Destroy(we);
        return AMSmlWE;
      } else { 
        Log.LogWrite("WARNING! Unknown " + we.GetType().ToString() + " AMS weapon effect!\n");
        return null;
      }
    }
    public static void Clear(bool full = true) {
      try {
        foreach (var ams in AMSWeaponEffectStaticHelper.amsEffects) {
          ams.Value.Clear();
        }
        if (full) AMSWeaponEffectStaticHelper.amsEffects.Clear();
      }catch(Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
  }
  public class AMSMainEffect {
    //private HashSet<int> effectsIDs;
    private List<AMSWeaponEffect> singleShotsEffects;
    private AMSMultiShotWeaponEffect multyShotEffect;
    private List<Vector3> hitPositions;
    private Weapon weapon;
    public virtual float calculateInterceptCorrection(int shotIdx,float curPath, float pathLenth, float distance, float missileProjectileSpeed) {
      if (shotIdx < 0) { return curPath / pathLenth; }
      if (this.multyShotEffect == null) {
        if (shotIdx >= this.singleShotsEffects.Count) { return curPath / pathLenth; }
        if (this.singleShotsEffects[shotIdx] == null) { return curPath / pathLenth; }
        return this.singleShotsEffects[shotIdx].calculateInterceptCorrection(curPath,pathLenth,distance,missileProjectileSpeed);
      } else {
        return this.multyShotEffect.calculateInterceptCorrection(shotIdx, curPath, pathLenth, distance, missileProjectileSpeed);
      }
    }
    public AMSMainEffect(Weapon w) {
      this.singleShotsEffects = new List<AMSWeaponEffect>();
      this.multyShotEffect = null;
      //this.effectsIDs = new HashSet<int>();
      this.hitPositions = new List<Vector3>();
      this.weapon = w;
    }
    public int AddHitPosition(Vector3 pos) {
      Log.LogWrite("AMSMainEffect.AddHitPosition\n");
      this.hitPositions.Add(pos);
      if(this.multyShotEffect == null) {
        AMSWeaponEffect AMSwe = this.weapon.InitAMSWeaponEffect();
        AMSMultiShotWeaponEffect AMSmwe = AMSwe as AMSMultiShotWeaponEffect;
        if (AMSmwe == null) {
          Log.LogWrite(" single shot "+((AMSwe==null)?"fail":"success") +"\n");
          this.singleShotsEffects.Add(AMSwe);
        } else {
          this.multyShotEffect = AMSmwe;
        }
      }
      if(this.multyShotEffect != null) {
        Log.LogWrite(" multishot\n");
        this.multyShotEffect.AddBullet();
      }
      return this.hitPositions.Count - 1;
    }
    public void Fire(int index) {
      Log.LogWrite(" AMS.Fire "+index+"/"+ hitPositions.Count+"\n");
      if (index < 0) {
        return;}
      if (index >= hitPositions.Count) {
        return;
      }
      if (this.multyShotEffect == null) {
        Log.LogWrite(" single shot "+ singleShotsEffects.Count + "\n");
        if (index >= singleShotsEffects.Count) { return; }
        AMSWeaponEffect we = this.singleShotsEffects[index];
        if (we == null) {
          Log.LogWrite(" single shot not inited\n");
          return;
        }
        Log.LogWrite(" single shot state:"+ we.currentState + "\n");
        if ((we.currentState == WeaponEffect.WeaponEffectState.Complete) || (we.currentState == WeaponEffect.WeaponEffectState.NotStarted)) {
          we.Fire(this.hitPositions.ToArray(), index);
        } else {

        }
      } else {
        if (multyShotEffect == null) { return; }
        if (index >= multyShotEffect.BulletsCount()) { return; }
        multyShotEffect.Fire(this.hitPositions.ToArray(), index);
      }
    }
    public void Clear() {
      Log.LogWrite("AMSMMainEffect.Clear\n");
      string prefabName = AMSMultiShotWeaponEffect.AMSPrefabPrefix + weapon.getWeaponEffectID();
      foreach (var effect in this.singleShotsEffects) {
        if (effect == null) { continue;  }
        effect.Reset();
        Log.LogWrite(" returning to pool " + prefabName + " " + effect.gameObject.GetInstanceID() + "\n");
        this.weapon.parent.Combat.DataManager.PoolGameObject(prefabName,effect.gameObject);
        //GameObject.Destroy(effect.gameObject);
      }
      singleShotsEffects.Clear();
      hitPositions.Clear();
      if (this.multyShotEffect != null) {
        this.multyShotEffect.Reset();
        this.multyShotEffect.ClearBullets();
        Log.LogWrite(" returning to pool " + prefabName + " " + multyShotEffect.gameObject.GetInstanceID() + "\n");
        this.weapon.parent.Combat.DataManager.PoolGameObject(prefabName, multyShotEffect.gameObject);
        this.multyShotEffect = null;
      };
    }
  }
}

namespace CustAmmoCategoriesPatches {
  [HarmonyPatch(typeof(TurnDirector))]
  [HarmonyPatch("BeginNewRound")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(int) })]
  public static class TurnDirector_BeginNewRound {
    public static bool Prefix(TurnDirector __instance, int round) {
      foreach (AbstractActor unit in __instance.Combat.AllActors) {
        foreach(Weapon weapon in unit.Weapons) {
          if (weapon.AMSShootsCount() <= 0) { weapon.setCantNormalFire(false); };
        }
      }
      JammingEnabler.jammAMS();
      return true;
    }
  }
  [HarmonyPatch(typeof(WeaponRepresentation))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Weapon), typeof(Transform), typeof(bool), typeof(string), typeof(int) })]
  public static class WeaponRepresentation_InitAMS {
    public static void Postfix(WeaponRepresentation __instance, Weapon weapon, Transform parentTransform, bool isParented, string parentDisplayName, int mountedLocation) {
      if (weapon.canBeAMS()) { weapon.InitAMS(); };
    }
  }
}