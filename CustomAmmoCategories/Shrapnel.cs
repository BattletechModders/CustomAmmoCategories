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
using BattleTech.Rendering;
using CustAmmoCategories;
using CustomAmmoCategoriesLog;
using FluffyUnderware.Curvy;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using WeaponRealizer;
using Random = UnityEngine.Random;
using Settings = WeaponRealizer.Settings;

namespace CustAmmoCategories {
  public class ShrapnelHitRecord {
    public int projectileHitIndex;
    public int shellsHitIndex;
    public int count;
    public bool isSeparated;
    public BallisticEffect shellsSubEffect;
    public WeaponHitInfo hitInfo;
    public Vector3 startPosition;
    public HashSet<int> bulletsInFly;
    //public GameObject startTransformObject;
    public ShrapnelHitRecord(int pHitIndex, bool separated) {
      projectileHitIndex = pHitIndex;
      isSeparated = separated;
      count = 0;
      bulletsInFly = new HashSet<int>();
    }
    public void Assign(int hHitIndex, int hitsCount, ref WeaponHitInfo hInfo, Weapon weapon) {
      shellsHitIndex = hHitIndex;
      startPosition = hInfo.hitPositions[this.projectileHitIndex];
      this.count = hitsCount;
      this.hitInfo = new WeaponHitInfo();
      this.hitInfo.attackerId = hInfo.attackerId;
      this.hitInfo.targetId = hInfo.targetId;
      this.hitInfo.numberOfShots = hitsCount;
      this.hitInfo.stackItemUID = hInfo.stackItemUID;
      this.hitInfo.attackSequenceId = hInfo.attackSequenceId;
      this.hitInfo.attackGroupIndex = hInfo.attackGroupIndex;
      this.hitInfo.attackWeaponIndex = hInfo.attackWeaponIndex;
      this.hitInfo.attackDirections = hInfo.attackDirections;
      this.hitInfo.secondaryTargetIds = hInfo.secondaryTargetIds;
      this.hitInfo.secondaryHitLocations = hInfo.secondaryHitLocations;
      this.hitInfo.toHitRolls = new float[hitsCount];
      this.hitInfo.locationRolls = new float[hitsCount];
      this.hitInfo.dodgeRolls = new float[hitsCount];
      this.hitInfo.dodgeSuccesses = new bool[hitsCount];
      this.hitInfo.hitLocations = new int[hitsCount];
      this.hitInfo.hitPositions = new Vector3[hitsCount];
      this.hitInfo.hitVariance = new int[hitsCount];
      this.hitInfo.hitQualities = new AttackImpactQuality[hitsCount];
      this.hitInfo.secondaryHitLocations = new int[hitsCount];
      this.hitInfo.secondaryTargetIds = new string[hitsCount];
      this.hitInfo.attackDirections = new AttackDirection[hitsCount];
      for (int t = 0; t < hitsCount; ++t) {
        this.hitInfo.toHitRolls[t] = hInfo.toHitRolls[hHitIndex + t];
        this.hitInfo.locationRolls[t] = hInfo.locationRolls[hHitIndex + t];
        this.hitInfo.dodgeRolls[t] = hInfo.dodgeRolls[hHitIndex + t];
        this.hitInfo.dodgeSuccesses[t] = hInfo.dodgeSuccesses[hHitIndex + t];
        this.hitInfo.hitLocations[t] = hInfo.hitLocations[hHitIndex + t];
        this.hitInfo.hitPositions[t] = hInfo.hitPositions[hHitIndex + t];
        this.hitInfo.hitVariance[t] = hInfo.hitVariance[hHitIndex + t];
        this.hitInfo.hitQualities[t] = hInfo.hitQualities[hHitIndex + t];
        this.hitInfo.secondaryHitLocations[t] = 0;
        this.hitInfo.secondaryTargetIds[t] = null;
        this.hitInfo.attackDirections[t] = hInfo.attackDirections[hHitIndex + t];
      }
      shellsSubEffect = null;// CustomAmmoCategories.popWeaponShellsEffect(weapon);
                             //if (shellsSubEffect != null) {
                             //startTransformObject = new GameObject();
                             //startTransformObject.transform.parent = weapon.weaponRep.vfxTransforms[0].parent;
                             //startTransformObject.transform.rotation = weapon.weaponRep.vfxTransforms[0].rotation;
                             //startTransformObject.transform.position = hInfo.hitPositions[projectileHitIndex];
                             //startTransformObject.transform.localScale = weapon.weaponRep.vfxTransforms[0].localScale;
                             // } else {
                             //startTransformObject = null;
                             //}
    }
    public void freeResources() {
      if (shellsSubEffect != null) {
        //Weapon weapon = this.shellsSubEffect.weapon;
        //shellsSubEffect.Reset();
        //typeof(WeaponEffect).GetField("startingTransform", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(shellsSubEffect, weapon.weaponRep.vfxTransforms[0]);
        //GameObject.Destroy(startTransformObject,1f);
        //startTransformObject = null;
        // CustomAmmoCategories.pushWeaponShellsEffect(shellsSubEffect);
      }
    }
  }
  public class ShellHitRecord {
    public int realHitIndex;
    public bool compleeted;
    public WeaponHitInfo hitInfo;
    public BallisticEffect shellsSubEffect;
    public BulletEffect shellSubEffect;
    public ShellHitRecord(int hi, int c) {
      //hitIndex = hi;
      //count = c;
      shellSubEffect = null;
    }
  }
  public static partial class CustomAmmoCategories {
    public static Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, ShrapnelHitRecord>>>> ShrapnelHitsRecord = new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, ShrapnelHitRecord>>>>();
    public static Dictionary<string, BallisticEffect> _shellsEffect = new Dictionary<string, BallisticEffect>();
    //public static Dictionary<string, List<BallisticEffect>> _shellsEffectsStorage = new Dictionary<string, List<BallisticEffect>>();
    //public static Dictionary<string, Queue<BallisticEffect>> _shellsEffectsQueue = new Dictionary<string, Queue<BallisticEffect>>();
    public static readonly string ShellsWeaponEffectId = "WeaponEffect-Weapon_AC2";
    public static void freeShrapnellResources(int attackSequenceId) {
      if (CustomAmmoCategories.ShrapnelHitsRecord.ContainsKey(attackSequenceId) == false) { return; };
      BallisticEffect shellsEffect = null;
      foreach (var groups in CustomAmmoCategories.ShrapnelHitsRecord) {
        foreach (var weapons in groups.Value) {
          foreach (var weapon in weapons.Value) {
            foreach (var hit in weapon.Value) {
              if (hit.Value.shellsSubEffect != null) {
                if (shellsEffect == null) { shellsEffect = hit.Value.shellsSubEffect; };
              }
              hit.Value.freeResources();
            }
          }
        }
      }
      if (shellsEffect != null) { shellsEffect.Reset(); }
      //CustomAmmoCategories.ShrapnelHitsRecord.Remove(attackSequenceId);
    }
    public static bool HasShells(this Weapon weapon) {
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      if (extWeapon.ImprovedBallistic == false) { return false; };
      WeaponMode mode = weapon.mode();
      if (mode.HasShells != TripleBoolean.NotSet) { return mode.HasShells == TripleBoolean.True; }
      ExtAmmunitionDef ammo = weapon.ammo();
      if (ammo.HasShells != TripleBoolean.NotSet) { return ammo.HasShells == TripleBoolean.True; }
      return extWeapon.HasShells == TripleBoolean.True;
    }
    public static float ShellsRadius(this Weapon weapon) {
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      if (extWeapon.ImprovedBallistic == false) { return 0f; };
      WeaponMode mode = weapon.mode();
      if (mode.HasShells != TripleBoolean.NotSet) { return mode.ShellsRadius; }
      ExtAmmunitionDef ammo = weapon.ammo();
      if (ammo.HasShells != TripleBoolean.NotSet) { return ammo.ShellsRadius; }
      if (extWeapon.HasShells != TripleBoolean.NotSet) { return extWeapon.ShellsRadius; }
      return 0f;
    }
    public static float UnseparatedDamageMult(this Weapon weapon) {
      return weapon.ammo().UnseparatedDamageMult;
    }
    public static float MinShellsDistance(this Weapon weapon) {
      WeaponMode mode = weapon.mode();
      if (mode.HasShells != TripleBoolean.NotSet) { return Mathf.Max(30f,mode.MinShellsDistance); }
      ExtAmmunitionDef ammo = weapon.ammo();
      if (ammo.HasShells != TripleBoolean.NotSet) { return Mathf.Max(30f, ammo.MinShellsDistance); }
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      if (extWeapon.HasShells != TripleBoolean.NotSet) { return Mathf.Max(30f, extWeapon.MinShellsDistance); }
      return 30f;
    }
    public static float MaxShellsDistance(this Weapon weapon) {
      WeaponMode mode = weapon.mode();
      if (mode.HasShells != TripleBoolean.NotSet) { return Mathf.Max(30f, mode.MaxShellsDistance); }
      ExtAmmunitionDef ammo = weapon.ammo();
      if (ammo.HasShells != TripleBoolean.NotSet) { return Mathf.Max(30f, ammo.MaxShellsDistance); }
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      if (extWeapon.HasShells != TripleBoolean.NotSet) { return Mathf.Max(30f, extWeapon.MaxShellsDistance); }
      return 30f;
    }
    /*public static BallisticEffect popWeaponShellsEffect(Weapon weapon) {
      CustomAmmoCategoriesLog.Log.LogWrite("popWeaponShellsEffect " + weapon.defId + "\n");
      string wGUID = weapon.StatCollection.GetStatistic(CustomAmmoCategories.GUIDStatisticName).Value<string>();
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.GUIDStatisticName) == false) { return null; }
      CustomAmmoCategoriesLog.Log.LogWrite(" GUID " + wGUID + "\n");
      if (CustomAmmoCategories._shellsEffectsQueue.ContainsKey(wGUID) == false) { return null; };
      if (CustomAmmoCategories._shellsEffectsQueue[wGUID].Count == 0) { return null; }
      BallisticEffect result = CustomAmmoCategories._shellsEffectsQueue[wGUID].Dequeue();
      CustomAmmoCategoriesLog.Log.LogWrite(" Dequeue shells effect success. Rest in queue:" + CustomAmmoCategories._shellsEffectsQueue[wGUID].Count + "\n");
      return result;
    }
    public static void ClearWeaponShellEffects(string wGUID) {
      if (CustomAmmoCategories._shellsEffectsStorage.ContainsKey(wGUID)) { CustomAmmoCategories._shellsEffectsStorage.Remove(wGUID); };
      if (CustomAmmoCategories._shellsEffectsQueue.ContainsKey(wGUID)) { CustomAmmoCategories._shellsEffectsQueue.Remove(wGUID); };
    }
    public static void pushWeaponShellsEffect(BallisticEffect effect) {
      Weapon weapon = effect.weapon;
      CustomAmmoCategoriesLog.Log.LogWrite("pushWeaponShellsEffect " + effect.weapon.defId + "\n");
      string wGUID = weapon.StatCollection.GetStatistic(CustomAmmoCategories.GUIDStatisticName).Value<string>();
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.GUIDStatisticName) == false) { return; }
      CustomAmmoCategoriesLog.Log.LogWrite(" GUID " + wGUID + "\n");
      if (CustomAmmoCategories._shellsEffectsQueue.ContainsKey(wGUID) == false) { return; };
      CustomAmmoCategories._shellsEffectsQueue[wGUID].Enqueue(effect);
      CustomAmmoCategoriesLog.Log.LogWrite(" Enqueue weapon effects. Now in queue:" + CustomAmmoCategories._shellsEffectsQueue[wGUID].Count + "\n");
    }*/

    /*public static BallisticEffect getWeaponShellsEffect(Weapon weapon) {
       if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.GUIDStatisticName) == false) { return null; }
       string wGUID = weapon.StatCollection.GetStatistic(CustomAmmoCategories.GUIDStatisticName).Value<string>();
       CustomAmmoCategoriesLog.Log.LogWrite(" weapon GUID:" + wGUID + "\n");
       if (CustomAmmoCategories._shellsEffect.ContainsKey(wGUID) == false) { return null; }
       return CustomAmmoCategories._shellsEffect[wGUID] as BallisticEffect;
     }*/
    /*public static void registerShellsEffects(WeaponRepresentation weaponRep, Weapon weapon) {
      CustomAmmoCategoriesLog.Log.LogWrite("Registering shellsEffects for " + weapon.defId + "\n");
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.GUIDStatisticName) == false) { return; }
      string wGUID = weapon.StatCollection.GetStatistic(CustomAmmoCategories.GUIDStatisticName).Value<string>();
      CustomAmmoCategoriesLog.Log.LogWrite(" weapon GUID:" + wGUID + "\n");
      if (CustomAmmoCategories._shellsEffect.ContainsKey(wGUID) == true) { return; }
      //List<BallisticEffect> shellsEffects = new List<BallisticEffect>();
      //Queue<BallisticEffect> shellsQueue = new Queue<BallisticEffect>();
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      if (extWeapon.Modes.Count < 1) {
        CustomAmmoCategoriesLog.Log.LogWrite("WARNING! " + weapon.defId + " has no modes. Even base mode. This means something is very very wrong\n", true);
        return;
      }
      int maxShots = 0;
      foreach (var mode in extWeapon.Modes) {
        HashSet<string> ammos = CustomAmmoCategories.getWeaponAvaibleAmmoForMode(weapon, mode.Value.Id);
        foreach (string ammo in ammos) {
          CustomAmmoCategories.applyWeaponAmmoMode(weapon, mode.Key, ammo);
          if (weapon.HasShells() == false) { continue; };
          int curShots = weapon.ProjectilesPerShot * weapon.ShotsWhenFired;
          if (maxShots < curShots) { maxShots = curShots; };
        }
      }
      CustomAmmoCategories.applyWeaponAmmoMode(weapon, extWeapon.baseModeId, "");
      CustomAmmoCategories.CycleAmmoBest(weapon);
      if (maxShots > 0) {
        //if(CustomAmmoCategories.WeaponEffects[wGUID].ContainsKey(CustomAmmoCategories.ShellsWeaponEffectId) == false) {
        BallisticEffect shellsEffect = CustomAmmoCategories.InitWeaponEffect(weaponRep, weapon, CustomAmmoCategories.ShellsWeaponEffectId) as BallisticEffect;
        CombatGameState Combat = (CombatGameState)typeof(WeaponEffect).GetField("Combat", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(shellsEffect);
        //Combat.DataManager.PrecachePrefab(shellsEffect.bulletPrefab.name, BattleTechResourceType.Prefab, maxShots);
        CustomAmmoCategories._shellsEffect.Add(wGUID, shellsEffect);
        //}
        CustomAmmoCategoriesLog.Log.LogWrite(" shell effect " + shellsEffect.bulletPrefab.name + " registered count:" + maxShots + "\n");
      }
      /*for (int index = 0; index < maxShots; ++index) {
        BallisticEffect shellsEffect = CustomAmmoCategories.InitWeaponEffect(weaponRep, weapon, CustomAmmoCategories.ShellsWeaponEffectId) as BallisticEffect;
        if (shellsEffect == null) {
          CustomAmmoCategoriesLog.Log.LogWrite("WARNING! " + CustomAmmoCategories.ShellsWeaponEffectId + " fail to init or no ballistic\n", true);
          return;
        }
        shellsEffects.Add(shellsEffect);
        shellsQueue.Enqueue(shellsEffect);
      }*/
    //_shellsEffectsStorage.Add(wGUID, shellsEffects);
    //_shellsEffectsQueue.Add(wGUID, shellsQueue);
    //CustomAmmoCategoriesLog.Log.LogWrite(" shells effects registred:" + shellsEffects.Count + "\n");
    /*}*/
    //public static Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, ShellHitRecord>>>> ShellHitsRecord = new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, ShellHitRecord>>>>();
    public static CurvySpline generateSpline(bool isDirect, bool isIndirect, Vector3 startPos, Vector3 endPos, MissileLauncherEffect missileLauncherEffect, int hitLocation) {
      Vector3[] spline;
      if (isDirect) {
        spline = CustomAmmoCategories.GenerateDirectMissilePath(
          missileLauncherEffect.missileCurveStrength,
          missileLauncherEffect.missileCurveFrequency,
          missileLauncherEffect.isSRM,
          hitLocation, startPos, endPos, missileLauncherEffect.weapon.parent.Combat);
      } else {
        if (isIndirect) {
          spline = CustomAmmoCategories.GenerateIndirectMissilePath(
            missileLauncherEffect.missileCurveStrength,
            missileLauncherEffect.missileCurveFrequency,
            missileLauncherEffect.isSRM,
            hitLocation, startPos, endPos, missileLauncherEffect.weapon.parent.Combat);
        } else {
          spline = CustomAmmoCategories.GenerateMissilePath(
            missileLauncherEffect.missileCurveStrength,
            missileLauncherEffect.missileCurveFrequency,
            missileLauncherEffect.isSRM,
            hitLocation, startPos, endPos, missileLauncherEffect.weapon.parent.Combat);
        }
      }
      GameObject splineObject = new GameObject();
      CurvySpline UnitySpline = splineObject.AddComponent<CurvySpline>();
      UnitySpline.Interpolation = CurvyInterpolation.Bezier;
      UnitySpline.Clear();
      UnitySpline.Closed = false;
      UnitySpline.Add(spline);
      UnitySpline.Refresh();
      return UnitySpline;
    }
    public static Vector3 interpolateSeparationPosition(CurvySpline UnitySpline, Vector3 startPos, Vector3 targetPos, float sMin, float sMax) {
      float t = 0f;
      bool rudeAproachFound = false;
      for (t = 0.1f; Mathf.Abs(t - 1.1f) > 0.001f; t += 0.1f) {
        Vector3 missilePos = UnitySpline.InterpolateByDistance(UnitySpline.Length * t);
        float distanceFromStart = Vector3.Distance(startPos, missilePos);
        float distanceToTarget = Vector3.Distance(targetPos, missilePos);
        Log.LogWrite(" interpolating pos: " + t + "(" + Mathf.Abs(t - 1f) + ") dfs:" + distanceFromStart + "/" + sMin + " dtt:" + distanceToTarget + "/" + sMax + "\n");
        if (distanceFromStart >= sMin) {
          if (distanceToTarget <= sMax) { rudeAproachFound = true; break; };
        }
      }
      Log.LogWrite(" result: " + t + "(" + Mathf.Abs(t - 1.1f) + "):" + rudeAproachFound + "\n");
      if (rudeAproachFound) {
        Log.LogWrite(" rude approach found:" + t + "\n");
        for (float tt = t - 0.1f; tt <= t; tt += 0.01f) {
          Vector3 missilePos = UnitySpline.InterpolateByDistance(UnitySpline.Length * tt);
          float distanceFromStart = Vector3.Distance(startPos, missilePos);
          float distanceToTarget = Vector3.Distance(targetPos, missilePos);
          Log.LogWrite(" interpolating precisely pos: " + tt + " dfs:" + distanceFromStart + "/" + sMin + " dtt:" + distanceToTarget + "/" + sMax + "\n");
          if (distanceFromStart >= sMin) {
            if (distanceToTarget <= sMax) {
              Log.LogWrite(" position found:" + missilePos + "\n");
              return missilePos;
            };
          }
        }
      }
      return Vector3.zero;
    }
    public static Vector3 interpolateSeparationPosition(Vector3 endPos, Vector3 startPos, Vector3 targetPos, float sMin, float sMax) {
      float t = 0f;
      bool rudeAproachFound = false;
      Log.LogWrite(" interpolating from " + startPos + " to " + endPos + " target: " + targetPos + "\n");
      for (t = 0.1f; Mathf.Abs(t - 1.1f) > 0.001f; t += 0.1f) {
        Vector3 missilePos = Vector3.Lerp(startPos, endPos, t);
        float distanceFromStart = Vector3.Distance(startPos, missilePos);
        float distanceToTarget = Vector3.Distance(targetPos, missilePos);
        Log.LogWrite(" interpolating pos: " + t + " dfs:" + distanceFromStart + "/" + sMin + " dtt:" + distanceToTarget + "/" + sMax + "\n");
        if (distanceFromStart >= sMin) {
          if (distanceToTarget <= sMax) { rudeAproachFound = true; break; };
        }
      }
      Log.LogWrite(" result: " + t + "(" + Mathf.Abs(t - 1.1f) + "):" + rudeAproachFound + "\n");
      if (rudeAproachFound) {
        Log.LogWrite(" rude approach found:" + t + "\n");
        for (float tt = t - 0.1f; tt <= t; tt += 0.01f) {
          Vector3 missilePos = Vector3.Lerp(startPos, endPos, tt);
          float distanceFromStart = Vector3.Distance(startPos, missilePos);
          float distanceToTarget = Vector3.Distance(targetPos, missilePos);
          Log.LogWrite(" interpolating precisely pos: " + tt + " dfs:" + distanceFromStart + "/" + sMin + " dtt:" + distanceToTarget + "/" + sMax + "\n");
          if (distanceFromStart >= sMin) {
            if (distanceToTarget <= sMax) {
              Log.LogWrite(" position found:" + missilePos + "\n");
              return missilePos;
            };
          }
        }
      }
      Log.LogWrite(" no separation\n");
      return Vector3.zero;
    }
    public static void printHitPositions(this WeaponHitInfo hitInfo) {
      Log.LogWrite("WeaponPositions:" + hitInfo.attackWeaponIndex + "\n");
      for (int t = 0; t < hitInfo.numberOfShots; ++t) {
        Log.LogWrite(" " + hitInfo.hitPositions[t] + "\n");
      }
    }

    /*public static void shrapnellEarlyExplode(CombatGameState combat, Weapon weapon, ref WeaponHitInfo hitInfo, int hitIndex, bool isIndirect, bool isDirect, ICombatant target) {
      WeaponEffect weaponEffect = CustomAmmoCategories.getWeaponEffect(weapon);
      CustomAmmoCategoriesLog.Log.LogWrite("Shrapnel early explode. HitIndex:" + hitIndex + "\n");
      if (CustomAmmoCategories.ShrapnelHitsRecord.ContainsKey(hitInfo.attackSequenceId) == false) {
        CustomAmmoCategories.ShrapnelHitsRecord.Add(hitInfo.attackSequenceId, new Dictionary<int, Dictionary<int, Dictionary<int, ShrapnelHitRecord>>>());
      };
      if (CustomAmmoCategories.ShrapnelHitsRecord[hitInfo.attackSequenceId].ContainsKey(hitInfo.attackGroupIndex) == false) {
        CustomAmmoCategories.ShrapnelHitsRecord[hitInfo.attackSequenceId].Add(hitInfo.attackGroupIndex, new Dictionary<int, Dictionary<int, ShrapnelHitRecord>>());
      };
      if (CustomAmmoCategories.ShrapnelHitsRecord[hitInfo.attackSequenceId][hitInfo.attackGroupIndex].ContainsKey(hitInfo.attackWeaponIndex) == false) {
        CustomAmmoCategories.ShrapnelHitsRecord[hitInfo.attackSequenceId][hitInfo.attackGroupIndex].Add(hitInfo.attackWeaponIndex, new Dictionary<int, ShrapnelHitRecord>());
      };
      int numberOfEmitters = (int)typeof(WeaponEffect).GetField("numberOfEmitters", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(weaponEffect);
      int emitterIndex = hitIndex % numberOfEmitters;
      Transform startingTransform = weapon.weaponRep.vfxTransforms[emitterIndex];
      bool separated = false;
      float sMin = CustomAmmoCategories.getWeaponMinShellsDistance(weapon);
      float sMax = CustomAmmoCategories.getWeaponMaxShellsDistance(weapon);
      Vector3 oldHitPos = hitInfo.hitPositions[hitIndex];
      Vector3 startPos = startingTransform.position;
      Vector3 targetPos = Vector3.zero;
      if (target.GUID != hitInfo.attackerId) {
        targetPos = target.CurrentPosition;
      } else {
        TerrainHitInfo terrainHitInfo = CustomAmmoCategories.getTerrinHitPosition(hitInfo.stackItemUID);
        if (terrainHitInfo != null) {
          targetPos = terrainHitInfo.pos;
          CustomAmmoCategoriesLog.Log.LogWrite(" Ground attack detected:" + targetPos + "\n");
        }
      }

      if (CustomAmmoCategories.getWeaponAlwaysIndirectVisuals(weapon) == true) { isIndirect = true; };
      MissileLauncherEffect missileLauncherEffect = weaponEffect as MissileLauncherEffect;
      MultiShotBallisticEffect msBallistic = weaponEffect as MultiShotBallisticEffect;
      if ((missileLauncherEffect != null)||(msBallistic != null)) {
        AdvWeaponHitInfoRec missile = hitInfo.advRec(hitIndex);
        if (missile != null) {
          Log.LogWrite(" interpolating separate pos by spline\n");
          CustomAmmoCategoriesLog.Log.LogWrite(" spline length:" + missile.trajectorySpline.Length + ". Min separation distance:" + sMin + "\n");
          Vector3 separationPos = interpolateSeparationPosition(missile.trajectorySpline, startPos, targetPos, sMin, sMax);
          if (separationPos != Vector3.zero) {
            separated = true;
            missile.endPos = separationPos;
            hitInfo.hitPositions[hitIndex] = separationPos;
            missile.hitLocation = 0;
            missile.regenerateMissilepath(isDirect,isIndirect);
          }
        } else {
          Log.LogWrite(" interpolating separate pos by spline. no pre-generated\n");
          CurvySpline UnitySpline = null;
          if (missileLauncherEffect != null) {
            UnitySpline = CustomAmmoCategories.generateSpline(isDirect, isIndirect,
              startPos, hitInfo.hitPositions[hitIndex], missileLauncherEffect, hitInfo.hitLocations[hitIndex]);
          } else 
          if(msBallistic != null) {
            if (isIndirect == true) {
              UnitySpline = msBallistic.generateSimpleIndirectSpline(startPos, hitInfo.hitPositions[hitIndex], hitInfo.hitLocations[hitIndex]);
            }
          }
          Vector3 separationPos = Vector3.zero;
          if (UnitySpline != null) {
            CustomAmmoCategoriesLog.Log.LogWrite(" spline length:" + UnitySpline.Length + ". Min separation distance:" + sMin + "\n");
            separationPos = interpolateSeparationPosition(UnitySpline, startPos, targetPos, sMin, sMax);
            GameObject.Destroy(UnitySpline.gameObject);
          } else {
            CustomAmmoCategoriesLog.Log.LogWrite(" distance length:" + Vector3.Distance(startPos, targetPos) + ". Min separation distance:" + sMin + "\n");
            separationPos = interpolateSeparationPosition(hitInfo.hitPositions[hitIndex], startPos, targetPos, sMin, sMax);
          }
          if (separationPos != Vector3.zero) {
            separated = true;
            hitInfo.hitPositions[hitIndex] = separationPos;
          }
        }
      } else {
        Vector3 separationPos = interpolateSeparationPosition(hitInfo.hitPositions[hitIndex], startPos, targetPos, sMin, sMax);
        if (separationPos != Vector3.zero) {
          separated = true;
          hitInfo.hitPositions[hitIndex] = separationPos;
        }
      }
      if (separated == true) {
        hitInfo.hitLocations[hitIndex] = 0;
        SpreadHitRecord spreadHitRecord = CustomAmmoCategories.getSpreadCache(hitInfo, hitIndex);
        if (spreadHitRecord != null) {
          spreadHitRecord.hitInfo.hitLocations[spreadHitRecord.internalIndex] = hitInfo.hitLocations[hitIndex];
          spreadHitRecord.hitInfo.hitPositions[spreadHitRecord.internalIndex] = hitInfo.hitPositions[hitIndex];
        }
      }
      if (CustomAmmoCategories.ShrapnelHitsRecord[hitInfo.attackSequenceId][hitInfo.attackGroupIndex][hitInfo.attackWeaponIndex].ContainsKey(hitIndex) == false) {
        CustomAmmoCategoriesLog.Log.LogWrite(" Shrapnel early explode: " + hitIndex + " separated:" + separated + ". Old HitPosition: "+oldHitPos+" New hitPosition: "+hitInfo.hitPositions[hitIndex]+"\n");
        CustomAmmoCategories.ShrapnelHitsRecord[hitInfo.attackSequenceId][hitInfo.attackGroupIndex][hitInfo.attackWeaponIndex].Add(hitIndex, new ShrapnelHitRecord(hitIndex, separated));
      }
      hitInfo.printHitPositions();
    }
    public static ShrapnelHitRecord getShrapnelCache(WeaponHitInfo hitInfo, int hitIndex) {
      if (CustomAmmoCategories.ShrapnelHitsRecord.ContainsKey(hitInfo.attackSequenceId) == false) {
        return null;
      };
      if (CustomAmmoCategories.ShrapnelHitsRecord[hitInfo.attackSequenceId].ContainsKey(hitInfo.attackGroupIndex) == false) {
        return null;
      };
      if (CustomAmmoCategories.ShrapnelHitsRecord[hitInfo.attackSequenceId][hitInfo.attackGroupIndex].ContainsKey(hitInfo.attackWeaponIndex) == false) {
        return null;
      };
      if (CustomAmmoCategories.ShrapnelHitsRecord[hitInfo.attackSequenceId][hitInfo.attackGroupIndex][hitInfo.attackWeaponIndex].ContainsKey(hitIndex) == false) {
        return null;
      };
      return CustomAmmoCategories.ShrapnelHitsRecord[hitInfo.attackSequenceId][hitInfo.attackGroupIndex][hitInfo.attackWeaponIndex][hitIndex];
    }
    public static readonly float FragHitInfoIndicator = -9.0f;
    public static void consolidateShrapnelHitInfo(Weapon weapon, ref WeaponHitInfo hitInfo, Dictionary<string, SpreadHitInfo> shrapnellHitInfo, int projectilesPerShot, float dodgedDamage) {
      CustomAmmoCategoriesLog.Log.LogWrite("Consolidating ShrapnelHitInfo\n");
      if (CustomAmmoCategories.SpreadCache.ContainsKey(hitInfo.attackSequenceId) == false) {
        CustomAmmoCategories.SpreadCache.Add(hitInfo.attackSequenceId, new Dictionary<int, Dictionary<int, Dictionary<int, SpreadHitRecord>>>());
      };
      if (CustomAmmoCategories.SpreadCache[hitInfo.attackSequenceId].ContainsKey(hitInfo.attackGroupIndex) == false) {
        CustomAmmoCategories.SpreadCache[hitInfo.attackSequenceId].Add(hitInfo.attackGroupIndex, new Dictionary<int, Dictionary<int, SpreadHitRecord>>());
      };
      if (CustomAmmoCategories.SpreadCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex].ContainsKey(hitInfo.attackWeaponIndex) == false) {
        CustomAmmoCategories.SpreadCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex].Add(hitInfo.attackWeaponIndex, new Dictionary<int, SpreadHitRecord>());
      };
      Dictionary<string, float> shrapnelCounters = new Dictionary<string, float>();
      Dictionary<string, float> shrapnelAdds = new Dictionary<string, float>();
      Dictionary<string, int> shrapnelIndexes = new Dictionary<string, int>();
      float longestArrayCount = 0;
      int consolidateHitsCount = hitInfo.numberOfShots;
      foreach (var sharapnelHit in shrapnellHitInfo) {
        if (sharapnelHit.Value.hitInfo.numberOfShots > longestArrayCount) { longestArrayCount = sharapnelHit.Value.hitInfo.numberOfShots; }
        consolidateHitsCount += sharapnelHit.Value.hitInfo.numberOfShots;
        CustomAmmoCategoriesLog.Log.LogWrite(" " + sharapnelHit.Key + " " + sharapnelHit.Value.hitInfo.numberOfShots + "/" + consolidateHitsCount + "\n");
        shrapnelCounters[sharapnelHit.Key] = 0f;
        shrapnelAdds[sharapnelHit.Key] = (float)sharapnelHit.Value.hitInfo.numberOfShots;
        shrapnelIndexes[sharapnelHit.Key] = 0;
        for(int hi=0;hi< sharapnelHit.Value.hitInfo.dodgeRolls.Length; ++hi) {
          shrapnellHitInfo[sharapnelHit.Key].hitInfo.dodgeRolls[hi] = CustomAmmoCategories.FragHitInfoIndicator;
        }
      }
      foreach (var sharapnelHit in shrapnellHitInfo) {
        shrapnelAdds[sharapnelHit.Key] /= (float)longestArrayCount;
      }
      float[] oldtoHitRolls = hitInfo.toHitRolls;
      float[] oldlocationRolls = hitInfo.locationRolls;
      float[] olddodgeRolls = hitInfo.dodgeRolls;
      bool[] olddodgeSuccesses = hitInfo.dodgeSuccesses;
      int[] oldhitLocations = hitInfo.hitLocations;
      Vector3[] oldhitPositions = hitInfo.hitPositions;
      int[] oldhitVariance = hitInfo.hitVariance;
      AttackImpactQuality[] oldhitQualities = hitInfo.hitQualities;
      string[] oldsecondaryTargetIds = hitInfo.secondaryTargetIds;
      int[] oldsecondaryHitLocations = hitInfo.secondaryHitLocations;
      AttackDirection[] oldattackDirections = hitInfo.attackDirections;

      hitInfo.toHitRolls = new float[consolidateHitsCount];
      hitInfo.locationRolls = new float[consolidateHitsCount];
      hitInfo.dodgeRolls = new float[consolidateHitsCount];
      hitInfo.dodgeSuccesses = new bool[consolidateHitsCount];
      hitInfo.hitLocations = new int[consolidateHitsCount];
      hitInfo.hitPositions = new Vector3[consolidateHitsCount];
      hitInfo.hitVariance = new int[consolidateHitsCount];
      hitInfo.hitQualities = new AttackImpactQuality[consolidateHitsCount];
      hitInfo.secondaryTargetIds = new string[consolidateHitsCount];
      hitInfo.secondaryHitLocations = new int[consolidateHitsCount];
      hitInfo.attackDirections = new AttackDirection[consolidateHitsCount];
      CustomAmmoCategoriesLog.Log.LogWrite(" new hits count:" + consolidateHitsCount + "\n");
      oldtoHitRolls.CopyTo(hitInfo.toHitRolls, 0);
      oldlocationRolls.CopyTo(hitInfo.locationRolls, 0);
      olddodgeRolls.CopyTo(hitInfo.dodgeRolls, 0);
      olddodgeSuccesses.CopyTo(hitInfo.dodgeSuccesses, 0);
      oldhitLocations.CopyTo(hitInfo.hitLocations, 0);
      oldhitPositions.CopyTo(hitInfo.hitPositions, 0);
      oldhitVariance.CopyTo(hitInfo.hitVariance, 0);
      oldhitQualities.CopyTo(hitInfo.hitQualities, 0);
      oldsecondaryTargetIds.CopyTo(hitInfo.secondaryTargetIds, 0);
      oldsecondaryHitLocations.CopyTo(hitInfo.secondaryHitLocations, 0);
      oldattackDirections.CopyTo(hitInfo.attackDirections, 0);
      int shrpnelHitIndex = hitInfo.numberOfShots;
      int hitIndex = hitInfo.numberOfShots;
      int realHits = hitInfo.numberOfShots;
      //hitInfo.numberOfShots = consolidateHitsCount;
      bool hasData = false;
      do {
        foreach (var shrapnelIndex in shrapnelIndexes) {
          shrapnelCounters[shrapnelIndex.Key] += shrapnelAdds[shrapnelIndex.Key];
        }
        foreach (var shrapnelAdd in shrapnelAdds) {
          int internalIndex = shrapnelIndexes[shrapnelAdd.Key];
          if (internalIndex < shrapnellHitInfo[shrapnelAdd.Key].hitInfo.numberOfShots) { hasData = true; }
          if (shrapnelCounters[shrapnelAdd.Key] >= 1.0f) {
            shrapnelCounters[shrapnelAdd.Key] -= 1.0f;
            if (internalIndex < shrapnellHitInfo[shrapnelAdd.Key].hitInfo.numberOfShots) {
              if ((hitIndex - realHits) % projectilesPerShot == 0) { CustomAmmoCategoriesLog.Log.LogWrite("\n"); }
              CustomAmmoCategoriesLog.Log.LogWrite(" hitIndex:" + hitIndex + "/" + consolidateHitsCount + " internal:" + internalIndex + "/" + shrapnellHitInfo[shrapnelAdd.Key].hitInfo.numberOfShots + " target:" + shrapnellHitInfo[shrapnelAdd.Key].targetGUID + "\n");
              hitInfo.toHitRolls[hitIndex] = shrapnellHitInfo[shrapnelAdd.Key].hitInfo.toHitRolls[internalIndex];
              hitInfo.locationRolls[hitIndex] = shrapnellHitInfo[shrapnelAdd.Key].hitInfo.locationRolls[internalIndex];
              hitInfo.dodgeRolls[hitIndex] = shrapnellHitInfo[shrapnelAdd.Key].hitInfo.dodgeRolls[internalIndex];
              hitInfo.dodgeSuccesses[hitIndex] = shrapnellHitInfo[shrapnelAdd.Key].hitInfo.dodgeSuccesses[internalIndex];
              hitInfo.hitLocations[hitIndex] = shrapnellHitInfo[shrapnelAdd.Key].hitInfo.hitLocations[internalIndex];
              hitInfo.hitPositions[hitIndex] = shrapnellHitInfo[shrapnelAdd.Key].hitInfo.hitPositions[internalIndex];
              hitInfo.hitVariance[hitIndex] = shrapnellHitInfo[shrapnelAdd.Key].hitInfo.hitVariance[internalIndex];
              hitInfo.hitQualities[hitIndex] = shrapnellHitInfo[shrapnelAdd.Key].hitInfo.hitQualities[internalIndex];
              hitInfo.secondaryHitLocations[hitIndex] = 0;
              hitInfo.secondaryTargetIds[hitIndex] = null;
              hitInfo.attackDirections[hitIndex] = shrapnellHitInfo[shrapnelAdd.Key].hitInfo.attackDirections[internalIndex]; ;

              if (CustomAmmoCategories.SpreadCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex][hitInfo.attackWeaponIndex].ContainsKey(hitIndex) == false) {
                CustomAmmoCategories.SpreadCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex][hitInfo.attackWeaponIndex].Add(hitIndex, new SpreadHitRecord(shrapnellHitInfo[shrapnelAdd.Key].targetGUID, shrapnellHitInfo[shrapnelAdd.Key].hitInfo, internalIndex, dodgedDamage));
              }
              ++shrapnelIndexes[shrapnelAdd.Key];
              ++hitIndex;
            }
          }
        }
      } while ((hasData == true) && (hitIndex < consolidateHitsCount));
      for (int hi = 0; hi < realHits; ++hi) {
        CachedMissileCurve missile = CustomAmmoCategories.getCachedMissileCurve(hitInfo, hi);
        if (missile != null) {
          if (missile.Intercepted) {
            CustomAmmoCategoriesLog.Log.LogWrite(" hitIndex:" + hi + " missile intecepted. No shrapnel info\n");
            continue;
          };
        }
        ShrapnelHitRecord shellsInfo = CustomAmmoCategories.getShrapnelCache(hitInfo, hi);
        if (shellsInfo != null) {
          if (shellsInfo.isSeparated == false) {
            CustomAmmoCategoriesLog.Log.LogWrite(" hitIndex:" + hi + " projectile not separated. No shrapnel info\n");
            continue;
          }
        }
        if (CustomAmmoCategories.ShrapnelHitsRecord[hitInfo.attackSequenceId][hitInfo.attackGroupIndex][hitInfo.attackWeaponIndex].ContainsKey(hi) == true) {
          CustomAmmoCategoriesLog.Log.LogWrite(" hitIndex:" + hi + " shrapnel Info:" + shrpnelHitIndex + "(" + projectilesPerShot + ")\n");
          CustomAmmoCategories.ShrapnelHitsRecord[hitInfo.attackSequenceId][hitInfo.attackGroupIndex][hitInfo.attackWeaponIndex][hi].Assign(shrpnelHitIndex, projectilesPerShot, ref hitInfo, weapon);
          for(int shellHitIndex = 0; shellHitIndex < projectilesPerShot; ++shellHitIndex) {
            int effHI = shrpnelHitIndex + shellHitIndex;
            CustomAmmoCategories.ShrapnelHitsRecord[hitInfo.attackSequenceId][hitInfo.attackGroupIndex][hitInfo.attackWeaponIndex].Add(
              effHI,
              CustomAmmoCategories.ShrapnelHitsRecord[hitInfo.attackSequenceId][hitInfo.attackGroupIndex][hitInfo.attackWeaponIndex][hi]
            );
            CustomAmmoCategoriesLog.Log.LogWrite("  seqId:" + hitInfo.attackSequenceId + " grp:" + hitInfo.attackGroupIndex + " wId:" + hitInfo.attackWeaponIndex + " hi:" + effHI + ")\n");
          }
          shrpnelHitIndex += projectilesPerShot;
        }
      }
    }*/
    /*public static Dictionary<string, SpreadHitInfo> prepareShrapnelHitInfo(AttackDirector.AttackSequence instance, ref WeaponHitInfo hitInfo, Weapon weapon, int groupIdx, int weaponIdx, int numberOfShots, float dogleDamage) {
      CustomAmmoCategoriesLog.Log.LogWrite("prepareShrapnelHitInfo\n");
      Dictionary<string, SpreadHitInfo> result = new Dictionary<string, SpreadHitInfo>();
      List<ICombatant> combatants = new List<ICombatant>();
      List<ICombatant> allCombatants = instance.Director.Combat.GetAllCombatants();
      string IFFDef = CustomAmmoCategories.getWeaponIFFTransponderDef(weapon);
      if (string.IsNullOrEmpty(IFFDef)) { combatants.AddRange(allCombatants); } else {
        HashSet<string> combatantsGuids = new HashSet<string>();
        List<AbstractActor> enemies = instance.Director.Combat.GetAllEnemiesOf(instance.attacker);
        foreach (ICombatant combatant in enemies) {
          if (combatantsGuids.Contains(combatant.GUID) == false) {
            combatants.Add(combatant);
            combatantsGuids.Add(combatant.GUID);
          }
        }
        foreach (ICombatant combatant in allCombatants) {
          if (combatant.GUID == instance.attacker.GUID) { continue; }
          if (combatantsGuids.Contains(combatant.GUID) == true) { continue; }
          if (CustomAmmoCategories.isCombatantHaveIFFTransponder(combatant, IFFDef) == false) {
            combatants.Add(combatant);
            combatantsGuids.Add(combatant.GUID);
          }
        }
      }
      float spreadDistance = CustomAmmoCategories.getWeaponShellsRadius(weapon);
      Dictionary<string, int> spreadCounts = new Dictionary<string, int>();
      for (int hitIndex = 0; hitIndex < numberOfShots; ++hitIndex) {
        CustomAmmoCategoriesLog.Log.LogWrite(" hitIndex:" + hitIndex + "\n");
        AdvWeaponHitInfoRec missile = hitInfo.advRec(hitIndex);
        if (missile != null) {
          if (missile.interceptInfo.Intercepted) {
            CustomAmmoCategoriesLog.Log.LogWrite("  intercepted\n");
            continue;
          };
        }
        /*ShrapnelHitRecord shellsInfo = CustomAmmoCategories.getShrapnelCache(hitInfo, hitIndex);
        if (shellsInfo != null) {
          if (shellsInfo.isSeparated == false) {
            CustomAmmoCategoriesLog.Log.LogWrite("  not separated\n");
            continue;
          }
        }
        List<ICombatant> spreadCombatants = new List<ICombatant>();
        float spreadRNDMax = spreadDistance;
        List<float> spreadBorders = new List<float>();
        spreadBorders.Add(spreadRNDMax);
        ICombatant currentTarget = instance.chosenTarget;
        SpreadHitRecord spreadHitRecord = CustomAmmoCategories.getSpreadCache(hitInfo, hitIndex);
        if (spreadHitRecord != null) {
          currentTarget = instance.Director.Combat.FindCombatantByGUID(spreadHitRecord.targetGUID);
          if (currentTarget == null) { currentTarget = instance.chosenTarget; };
        }
        spreadCombatants.Add(currentTarget);
        if (spreadCounts.ContainsKey(currentTarget.GUID) == false) { spreadCounts[currentTarget.GUID] = 0; };
        foreach (ICombatant combatant in combatants) {
          if (combatant.IsDead) { continue; };
          if (combatant.GUID == currentTarget.GUID) { continue; }
          float distance = Vector3.Distance(combatant.CurrentPosition, currentTarget.CurrentPosition);
          if (distance < spreadDistance) {
            spreadRNDMax += (spreadDistance - distance);
            spreadBorders.Add(spreadRNDMax);
            spreadCombatants.Add(combatant);
            if (spreadCounts.ContainsKey(combatant.GUID) == false) { spreadCounts[combatant.GUID] = 0; };
          };
        }
        int numberOfShells = weapon.ProjectilesPerShot;
        for (int shellIndex = 0; shellIndex < numberOfShells; ++shellIndex) {
          float roll = Random.Range(0f, spreadRNDMax);
          int combatantIndex = 0;
          for (int targetIndex = 0; targetIndex < spreadBorders.Count; ++targetIndex) {
            if (roll <= spreadBorders[targetIndex]) {
              combatantIndex = targetIndex;
              break;
            }
          }
          spreadCounts[spreadCombatants[combatantIndex].GUID] += 1;
        }
      }
      CustomAmmoCategoriesLog.Log.LogWrite(" shrapnel result\n");
      int spreadSumm = 0;
      foreach (var spreadCount in spreadCounts) {
        ICombatant combatant = instance.Director.Combat.FindCombatantByGUID(spreadCount.Key);
        if (combatant == null) { continue; }
        if (spreadCount.Value == 0) { continue; };
        spreadSumm += spreadCount.Value;
        WeaponHitInfo ShrapnellHitInfo = new WeaponHitInfo();
        ShrapnellHitInfo.numberOfShots = spreadCount.Value;
        CustomAmmoCategoriesLog.Log.LogWrite(" " + combatant.DisplayName + " " + combatant.GUID + " " + ShrapnellHitInfo.numberOfShots + "\n");
        ShrapnellHitInfo.attackerId = instance.attacker.GUID;
        ShrapnellHitInfo.targetId = combatant.GUID;
        ShrapnellHitInfo.stackItemUID = instance.stackItemUID;
        ShrapnellHitInfo.attackSequenceId = instance.id;
        ShrapnellHitInfo.attackGroupIndex = groupIdx;
        ShrapnellHitInfo.attackWeaponIndex = weaponIdx;
        ShrapnellHitInfo.toHitRolls = new float[ShrapnellHitInfo.numberOfShots];
        ShrapnellHitInfo.locationRolls = new float[ShrapnellHitInfo.numberOfShots];
        ShrapnellHitInfo.dodgeRolls = new float[ShrapnellHitInfo.numberOfShots];
        ShrapnellHitInfo.dodgeSuccesses = new bool[ShrapnellHitInfo.numberOfShots];
        ShrapnellHitInfo.hitLocations = new int[ShrapnellHitInfo.numberOfShots];
        ShrapnellHitInfo.hitPositions = new Vector3[ShrapnellHitInfo.numberOfShots];
        ShrapnellHitInfo.hitVariance = new int[ShrapnellHitInfo.numberOfShots];
        ShrapnellHitInfo.hitQualities = new AttackImpactQuality[ShrapnellHitInfo.numberOfShots];
        ShrapnellHitInfo.secondaryTargetIds = new string[ShrapnellHitInfo.numberOfShots];
        ShrapnellHitInfo.secondaryHitLocations = new int[ShrapnellHitInfo.numberOfShots];
        ShrapnellHitInfo.attackDirections = new AttackDirection[ShrapnellHitInfo.numberOfShots];
        result.Add(combatant.GUID, new SpreadHitInfo(combatant.GUID, ShrapnellHitInfo, dogleDamage));
      }
      return result;
    }*/
    /*public static bool hasShellsInfo(ref WeaponHitInfo hitInfo) {
      if (CustomAmmoCategories.ShrapnelHitsRecord.ContainsKey(hitInfo.attackSequenceId) == false) { return false; }
      if (CustomAmmoCategories.ShrapnelHitsRecord[hitInfo.attackSequenceId].ContainsKey(hitInfo.attackGroupIndex) == false) { return false; }
      if (CustomAmmoCategories.ShrapnelHitsRecord[hitInfo.attackSequenceId][hitInfo.attackGroupIndex].ContainsKey(hitInfo.attackWeaponIndex) == false) { return false; }
      return true;
    }*/
    /*public static void setupShellsBullets(Weapon weapon,ref WeaponHitInfo hitInfo) {
      CustomAmmoCategoriesLog.Log.LogWrite("SetupShellBullets "+weapon.defId+" grp:"+hitInfo.attackGroupIndex+" weapon:"+hitInfo.attackWeaponIndex+"\n");
      if(CustomAmmoCategories.hasShellsInfo(ref hitInfo) == false) {
        CustomAmmoCategoriesLog.Log.LogWrite(" no shells info generated\n");
        return;
      }
      BallisticEffect shellsEffect = CustomAmmoCategories.getWeaponShellsEffect(weapon);
      if (shellsEffect == null) {
        CustomAmmoCategoriesLog.Log.LogWrite(" can't get shells effect\n");
        return;
      }
      typeof(BallisticEffect).GetField("currentBullet", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(shellsEffect, (object)0);
      shellsEffect.shotDelay = 0.01f;
      float rate = 1f / shellsEffect.shotDelay;
      typeof(WeaponEffect).GetField("rate", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(shellsEffect, (object)rate);

      typeof(BallisticEffect).GetMethod("ClearBullets", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(shellsEffect, new object[0] { });
      CombatGameState Combat = (CombatGameState)typeof(WeaponEffect).GetField("Combat", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(shellsEffect);
      List<BulletEffect> bullets = (List<BulletEffect>)typeof(BallisticEffect).GetField("bullets", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(shellsEffect);
      WeaponEffect currentEffect = CustomAmmoCategories.getWeaponEffect(weapon);
      if (currentEffect != null) {
        if (currentEffect is MissileLauncherEffect) {
          MissileLauncherEffect launcherEffect = currentEffect as MissileLauncherEffect;
          CustomAmmoCategoriesLog.Log.LogWrite("current effect is missile launcher effect\n");
          CustomAmmoCategoriesLog.Log.LogWrite("missiles:" + launcherEffect.missiles.Count + "\n");
          foreach (var missile in launcherEffect.missiles) {
            CustomAmmoCategoriesLog.Log.LogWrite(" missile:" + missile.GetInstanceID() + "\n");
          }
        }
      } else {
        CustomAmmoCategoriesLog.Log.LogWrite("can't get current weapon effect\n");
      }
      //int fullBulletsCount = 0;
      HashSet<int> lastBulletUID = new HashSet<int>();
      for (int hitIndex = 0;hitIndex < hitInfo.numberOfShots; ++hitIndex) {
        CustomAmmoCategoriesLog.Log.LogWrite(" hitIndex:"+hitIndex+"\n");
        ShrapnelHitRecord shrapnelHitRecord = null;
          //CustomAmmoCategories.getShrapnelCache(hitInfo, hitIndex);
        if (shrapnelHitRecord == null) {
          CustomAmmoCategoriesLog.Log.LogWrite("  no shells hit record\n");
          continue;
        };
        if (shrapnelHitRecord.isSeparated == false) {
          CustomAmmoCategoriesLog.Log.LogWrite("  not separated\n");
          continue;
        }
        if (shrapnelHitRecord.projectileHitIndex != hitIndex) {
          CustomAmmoCategoriesLog.Log.LogWrite("  bad hit index: "+ shrapnelHitRecord.projectileHitIndex + "\n");
          continue;
        }
        for (int index = 0; index < shrapnelHitRecord.count; ++index) {
          int effectiveHitIndex = shrapnelHitRecord.shellsHitIndex + index;
          CustomAmmoCategoriesLog.Log.LogWrite("  shellHitIndex:"+effectiveHitIndex+"\n");
          //ShrapnelHitRecord shellHitRecord = CustomAmmoCategories.getShrapnelCache(hitInfo, effectiveHitIndex);
          //if(shellHitRecord == null) {
          //  CustomAmmoCategoriesLog.Log.LogWrite("  can't find shell hit record\n");
          //  continue;
          //}
          CustomAmmoCategoriesLog.Log.LogWrite("  shellBulletInit "+shellsEffect.bulletPrefab.name+" hitIndex:" + (shrapnelHitRecord.shellsHitIndex+index) + " location:" + hitInfo.hitLocations[effectiveHitIndex] + "\n");
          GameObject gameObject = Combat.DataManager.PooledInstantiate(shellsEffect.bulletPrefab.name, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
          if ((UnityEngine.Object)gameObject == (UnityEngine.Object)null) {
            CustomAmmoCategoriesLog.Log.LogWrite("Error instantiating BulletObject " + shellsEffect.bulletPrefab.name + "\n", true);
            continue;
          }
          gameObject.transform.parent = (Transform)null;
          if (lastBulletUID.Contains(gameObject.GetInstanceID())) {
            CustomAmmoCategoriesLog.Log.LogWrite("bullets list already contains:"+ gameObject.GetInstanceID() + "\n");
            gameObject = GameObject.Instantiate(gameObject);
            CustomAmmoCategoriesLog.Log.LogWrite("creating new one with id:" + gameObject.GetInstanceID() + "\n");
            lastBulletUID.Add(gameObject.GetInstanceID());
          } else {
            CustomAmmoCategoriesLog.Log.LogWrite("bullets list not contains:" + gameObject.GetInstanceID() + "\n");
            lastBulletUID.Add(gameObject.GetInstanceID());
          }
          BulletEffect component = gameObject.GetComponent<BulletEffect>();
          if ((UnityEngine.Object)component == (UnityEngine.Object)null) {
            CustomAmmoCategoriesLog.Log.LogWrite("Error finding BulletEffect on GO " + shellsEffect.bulletPrefab.name + "\n", true);
            continue;
          }
          //component.projectileSpeed = Vector3.Distance(shrapnelHitRecord.hitInfo.hitPositions[index],shrapnelHitRecord.startPosition)/4.0f;
          component.projectileSpeed = 100.0f;
          //CustomAmmoCategoriesLog.Log.LogWrite(" speed: " + component.projectileSpeed + "\n");
          PropertyInfo property = typeof(WeaponEffect).GetProperty("FiringComplete");
          property.DeclaringType.GetProperty("FiringComplete");
          property.GetSetMethod(true).Invoke(component, new object[1] { (object)false });
          CustomAmmoCategoriesLog.Log.LogWrite(" add bullet uid:" + component.GetInstanceID()+ "\n");
          bullets.Add(component);
          component.Init(shellsEffect.weapon, shellsEffect);
        }
      }
      /*if(fullBulletsCount > 0) {
        CustomAmmoCategoriesLog.Log.LogWrite("Initing bullets:"+fullBulletsCount+"\n");
        CustomAmmoCategoriesPatches.BallisticEffect_SetupBulletsShell.SetupMissilesCount = fullBulletsCount;
        shellsEffect.hitIndex = 0;
        typeof(BallisticEffect).GetMethod("SetupBullets", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(shellsEffect, new object[0] { });
      }
      foreach (var bullet in bullets) {
        CustomAmmoCategoriesLog.Log.LogWrite(" bullet:" + bullet.GetInstanceID() + "\n");
      }
    }*/

    /*public static BallisticEffect getWeaponShellEffect(Weapon weapon) {
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.GUIDStatisticName) == false) { return null; };
      string wGUID = weapon.StatCollection.GetStatistic(CustomAmmoCategories.GUIDStatisticName).Value<string>();
      if (CustomAmmoCategories.WeaponEffects.ContainsKey(wGUID) == false) { return null; }
      if (CustomAmmoCategories.WeaponEffects[wGUID].ContainsKey(extWeapon.ShrapnelWeaponEffectID) == false) { return null; };
      return CustomAmmoCategories.WeaponEffects[wGUID][extWeapon.ShrapnelWeaponEffectID] as BallisticEffect;
    }*/
    /*public static void shrapnelFireShells(WeaponHitInfo hitInfo, int hitIndex, ShrapnelHitRecord shrapnelHitRecord, Weapon weapon) {
      CustomAmmoCategoriesLog.Log.LogWrite("shrapnelFireShells " + hitInfo.attackGroupIndex + " " + hitInfo.attackWeaponIndex + " hitIndex:"+hitIndex+" count:" + shrapnelHitRecord.count + "\n");
      if (shrapnelHitRecord.isSeparated == false) { return; };
      BallisticEffect shellsEffect = CustomAmmoCategories.getWeaponShellsEffect(weapon);
      if (shellsEffect == null) {
        CustomAmmoCategoriesLog.Log.LogWrite(" can't get shells effect\n");
        return;
      }
      shrapnelHitRecord.shellsSubEffect = shellsEffect;
      shellsEffect.hitInfo = hitInfo;
      CustomAmmoCategoriesLog.Log.LogWrite("shrapnelFireShells " + hitInfo.attackGroupIndex + " " + hitInfo.attackWeaponIndex + " count:" + shellsEffect.hitInfo.numberOfShots + " " + shrapnelHitRecord.shellsSubEffect.bulletPrefab.name + "\n");

      shellsEffect.subEffect = true;
      typeof(WeaponEffect).GetField("t", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(shellsEffect, (object)0.0f);
      shellsEffect.hitIndex = hitIndex;
      typeof(WeaponEffect).GetField("emitterIndex", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(shellsEffect, (object)0);
      typeof(WeaponEffect).GetField("startingTransform", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(shellsEffect, (object)shellsEffect.weaponRep.vfxTransforms[0]);
      Transform startingTransform = (Transform)typeof(WeaponEffect).GetField("startingTransform", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(shellsEffect);
      typeof(WeaponEffect).GetField("startPos", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(shellsEffect, (object)shrapnelHitRecord.startPosition);
      typeof(WeaponEffect).GetField("endPos", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(shellsEffect, (object)hitInfo.hitPositions[hitIndex]);
      Vector3 startPos = (Vector3)typeof(WeaponEffect).GetField("startPos", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(shellsEffect);
      typeof(WeaponEffect).GetField("currentPos", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(shellsEffect, (object)startPos);
      PropertyInfo property = typeof(WeaponEffect).GetProperty("FiringComplete");
      property.DeclaringType.GetProperty("FiringComplete");
      property.GetSetMethod(true).Invoke(shellsEffect, new object[1] { (object)false });
      shellsEffect.InitProjectile();
      shellsEffect.currentState = WeaponEffect.WeaponEffectState.PreFiring;

      typeof(BallisticEffect).GetMethod("PlayPreFire", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(shellsEffect, new object[0] { });

    }
  }*/
  }


  namespace CustomAmmoCategoriesPatches {
    [HarmonyPatch(typeof(AttackDirector.AttackSequence), "GenerateRandomCache")]
    [HarmonyPriority(Priority.First)]
    public static class ClusteredShotRandomCacheEnabler {
      public static bool Prefix(AttackDirector.AttackSequence __instance) {
        List<List<Weapon>> sortedWeapons = (List<List<Weapon>>)typeof(AttackDirector.AttackSequence).GetField("sortedWeapons", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
        Dictionary<string, bool> _isClustered = (Dictionary<string, bool>)typeof(WeaponRealizer.Core).Assembly.GetType("WeaponRealizer.ClusteredShotRandomCacheEnabler").GetField("_isClustered", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
        foreach (var weaponsGroup in sortedWeapons) {
          foreach (var weapon in weaponsGroup) {
            //ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
            if (weapon.HasShells() || weapon.DamagePerPallet()) {
              CustomAmmoCategoriesLog.Log.LogWrite("GenerateRandomCache " + weapon.defId + " tie to ClusterRandomCache true\n");
              _isClustered[weapon.defId] = true;
            }
          }
        }
        return true;
      }
    }
    /*[HarmonyPatch(typeof(BallisticEffect), "OnImpact", MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(float) })]
    public static class BallisticEffect_OnImpact {
      public static bool Prefix(BallisticEffect __instance, ref float hitDamage) {
        if (__instance.subEffect) {
          CustomAmmoCategoriesLog.Log.LogWrite("BallisticEffect OnImpact from " + __instance.weapon.defId + " is shell subeffect. No hitDamage\n");
          hitDamage = 0f;
        };
        return true;
      }
    }*/
    /*[HarmonyPatch(typeof(MissileLauncherEffect), "AllMissilesComplete", MethodType.Normal)]
    [HarmonyPatch(new Type[] { })]
    public static class MissileLauncherEffect_AllMissilesComplete {
      public static void Postfix(MissileLauncherEffect __instance, ref bool __result) {
        if (__result == true) {
          for (int hitIndex = 0; hitIndex < __instance.hitInfo.numberOfShots; ++hitIndex) {
            ShrapnelHitRecord shrapnelHitRecord = CustomAmmoCategories.getShrapnelCache(__instance.hitInfo, hitIndex);
            if (shrapnelHitRecord != null) {
              if (shrapnelHitRecord.shellsSubEffect != null) {
                List<BulletEffect> bullets = (List<BulletEffect>)typeof(BallisticEffect).GetField("bullets", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(shrapnelHitRecord.shellsSubEffect);
                foreach(var bullet in bullets) {
                  if (bullet.currentState != WeaponEffect.WeaponEffectState.Complete) { __result = false; return; };
                }
              }
            }
          }
          CustomAmmoCategoriesLog.Log.LogWrite("MissileLauncherEffect_AllMissilesComplete all bullets complete\n");
        }
      }
    }
    [HarmonyPatch(typeof(BallisticEffect), "AllBulletsComplete", MethodType.Normal)]
    [HarmonyPatch(new Type[] { })]
    public static class BallisticEffect_AllBulletsComplete {
      public static void Postfix(BallisticEffect __instance, ref bool __result) {
        if (__result == true) {
          if (__instance.subEffect == false) {
            for (int hitIndex = 0; hitIndex < __instance.hitInfo.numberOfShots; ++hitIndex) {
              ShrapnelHitRecord shrapnelHitRecord = CustomAmmoCategories.getShrapnelCache(__instance.hitInfo, hitIndex);
              if (shrapnelHitRecord != null) {
                if (shrapnelHitRecord.shellsSubEffect != null) {
                  List<BulletEffect> bullets = (List<BulletEffect>)typeof(BallisticEffect).GetField("bullets", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(shrapnelHitRecord.shellsSubEffect);
                  foreach (var bullet in bullets) {
                    if (bullet.currentState != WeaponEffect.WeaponEffectState.Complete) { __result = false; return; };
                  }
                }
              }
            }
            CustomAmmoCategoriesLog.Log.LogWrite("BallisticEffect_AllBulletsComplete all bullets complete\n");
          }
        }
      }
    }*/
    /*
    [HarmonyPatch(typeof(BulletEffect))]
    [HarmonyPatch("Fire")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(WeaponHitInfo), typeof(int), typeof(int) })]
    public static class BulletEffect_FireShell {
      private static Action<BulletEffect, WeaponHitInfo, int, int> WeaponEffect_Fire;
      public static bool Prepare() {
        // build a call to WeaponEffect.OnComplete() so it can be called
        // a la base.OnComplete() from the context of a BallisticEffect
        // https://blogs.msdn.microsoft.com/rmbyers/2008/08/16/invoking-a-virtual-method-non-virtually/
        // https://docs.microsoft.com/en-us/dotnet/api/system.activator?view=netframework-3.5
        // https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.dynamicmethod.-ctor?view=netframework-3.5#System_Reflection_Emit_DynamicMethod__ctor_System_String_System_Type_System_Type___System_Type_
        // https://stackoverflow.com/a/4358250/1976
        var method = typeof(WeaponEffect).GetMethod("Fire", AccessTools.all);
        var dm = new DynamicMethod("CACBulletEffectWeaponEffectFire", null, new Type[] { typeof(BulletEffect), typeof(WeaponHitInfo), typeof(int), typeof(int) }, typeof(BulletEffect));
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Ldarg_2);
        gen.Emit(OpCodes.Ldarg_3);
        gen.Emit(OpCodes.Call, method);
        gen.Emit(OpCodes.Ret);
        WeaponEffect_Fire = (Action<BulletEffect, WeaponHitInfo, int, int>)dm.CreateDelegate(typeof(Action<BulletEffect, WeaponHitInfo, int, int>));
        return true;
      }

      public static void WeaponEffectFire(BulletEffect __instance, WeaponHitInfo hitInfo, int hitIndex, int emitterIndex) {
        CustomAmmoCategoriesLog.Log.LogWrite("Weapon Effect Fire as shell effect\n");
        //int pHitIndex = __instance.hitIndex;
        //int hitIndex = __instance.hitIndex;
        //CustomAmmoCategoriesLog.Log.LogWrite(" parent HitIndex:" + pHitIndex + "\n");
        ShrapnelHitRecord shrapnelHitRecord = CustomAmmoCategories.getShrapnelCache(hitInfo, hitIndex);
        BulletEffect bullet = __instance as BulletEffect;
        typeof(WeaponEffect).GetField("t", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (object)0.0f);
        __instance.hitIndex = hitIndex;
        typeof(WeaponEffect).GetField("emitterIndex", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (object)emitterIndex);
        __instance.hitInfo = hitInfo;
        Transform startingTransform = __instance.weaponRep.vfxTransforms[emitterIndex];
        typeof(WeaponEffect).GetField("startingTransform", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (object)startingTransform);
        if (shrapnelHitRecord == null) {
          CustomAmmoCategoriesLog.Log.LogWrite(" can't find shrapnel hit record. fallback\n");
          typeof(WeaponEffect).GetField("startPos", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (object)startingTransform.position);
        } else {
          typeof(WeaponEffect).GetField("startPos", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (object)shrapnelHitRecord.startPosition);
        }
        typeof(WeaponEffect).GetField("endPos", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (object)hitInfo.hitPositions[hitIndex]);
        Vector3 startPos = (Vector3)typeof(WeaponEffect).GetField("startPos", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
        typeof(WeaponEffect).GetField("currentPos", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (object)startPos);
        PropertyInfo bulletProperty = typeof(WeaponEffect).GetProperty("FiringComplete");
        bulletProperty.DeclaringType.GetProperty("FiringComplete");
        bulletProperty.GetSetMethod(true).Invoke(__instance, new object[1] { (object)false });
        __instance.InitProjectile();
        __instance.currentState = WeaponEffect.WeaponEffectState.PreFiring;
        CustomAmmoCategoriesLog.Log.LogWrite(" start pos is altered. Was:" + startingTransform.position + " become:" + startPos + "\n");

      }*/
    /*
    public static bool Prefix(BulletEffect __instance, WeaponHitInfo hitInfo, int hitIndex, int emitterIndex) {
      __instance.hitInfo = hitInfo;
      CustomAmmoCategoriesLog.Log.LogWrite("Bullet Effect Fire " + __instance.weapon.defId + "/"+__instance.GetInstanceID() + " seq:" + __instance.hitInfo.attackSequenceId + " grp:" + __instance.hitInfo.attackGroupIndex + " " + __instance.hitInfo.attackWeaponIndex + " " + hitIndex + "\n");
      BallisticEffect parentLauncher = (BallisticEffect)typeof(BulletEffect).GetField("parentLauncher", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
      if (parentLauncher.subEffect) {
        CustomAmmoCategoriesLog.Log.LogWrite(" Parent is sub effect - shell effect. Using Start pos instead of starting transform\n");
        //WeaponEffect_Fire.Invoke(__instance, hitInfo, hitIndex, emitterIndex);
        BulletEffect_FireShell.WeaponEffectFire(__instance, hitInfo, hitIndex, emitterIndex);
        Vector3 startPos = (Vector3)typeof(WeaponEffect).GetField("startPos", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
        Vector3 endPos = (Vector3)typeof(WeaponEffect).GetField("endPos", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
        float num = Vector3.Distance(startPos, endPos);
        float duration = 1f;
        if ((double)__instance.projectileSpeed > 0.0) {
          duration = num / __instance.projectileSpeed;
        }
        if ((double)duration > 4.0) {
          duration = 4f;
        }
        float rate = 1f / duration;
        typeof(WeaponEffect).GetField("rate", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (object)rate);
        typeof(WeaponEffect).GetField("duration", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (object)duration);
        typeof(BulletEffect).GetMethod("PlayPreFire", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance, new object[0] { });
        return false;
      } else {
        BulletCurvyInfo curvyInfo = CustomAmmoCategories.getCachedBulletCurve(hitInfo,hitIndex);
        if(curvyInfo != null) {
          CustomAmmoCategoriesLog.Log.LogWrite(" cached bullet trajectory found\n");
          //WeaponEffect_Fire.Invoke(__instance, hitInfo, hitIndex, emitterIndex);
          float num = curvyInfo.UnitySpline.Length;
          float duration = 1f;
          if ((double)__instance.projectileSpeed > 0.0) {
            duration = num / __instance.projectileSpeed;
          }
          if ((double)duration > 4.0) {
            duration = 4f;
          }
          float rate = 1f / duration;
          GameObject.Destroy(curvyInfo.UnitySpline); curvyInfo.UnitySpline = null;
          GameObject.Destroy(curvyInfo.splineObject); curvyInfo.splineObject = null;
          curvyInfo.UnitySpline = __instance.gameObject.GetComponent<CurvySpline>();
          if (curvyInfo.UnitySpline == null) {
            curvyInfo.UnitySpline = __instance.gameObject.AddComponent<CurvySpline>();
          }
          curvyInfo.UnitySpline.Interpolation = CurvyInterpolation.Bezier;
          curvyInfo.UnitySpline.Clear();
          curvyInfo.UnitySpline.Closed = false;
          curvyInfo.UnitySpline.Add(curvyInfo.spline);
          curvyInfo.UnitySpline.Refresh();
          typeof(WeaponEffect).GetField("rate", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (object)rate);
          typeof(WeaponEffect).GetField("duration", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (object)duration);
          typeof(BulletEffect).GetMethod("PlayPreFire", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance, new object[0] { });
          return false;
        }
      }
      return true;
    }
  }*/
    /*
    [HarmonyPatch(typeof(WeaponEffect))]
    [HarmonyPatch("Fire")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(WeaponHitInfo), typeof(int), typeof(int) })]
    [HarmonyPriority(Priority.Normal)]
    public static class WeaponEffect_FireShell {
      public static bool Prefix(WeaponEffect __instance, WeaponHitInfo hitInfo, int hitIndex, int emitterIndex) {
        __instance.hitInfo = hitInfo;
        CustomAmmoCategoriesLog.Log.LogWrite("Weapon Effect Fire "+__instance.weapon.defId+ "/"+__instance.GetInstanceID() + " seq:" + __instance.hitInfo.attackSequenceId + " grp:" + __instance.hitInfo.attackGroupIndex + " " + __instance.hitInfo.attackWeaponIndex + " " + hitIndex + "\n");
        __instance.hitInfo = hitInfo;
        if (__instance is BulletEffect) {
          BallisticEffect parentLauncher = (BallisticEffect)typeof(BulletEffect).GetField("parentLauncher", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
          if (parentLauncher.subEffect) {
            return false;
          }
        }
        if((__instance is BulletEffect)||(__instance is BallisticEffect)) {
          if (CustomAmmoCategories.getWeaponHasShells(__instance.weapon)) {
            CustomAmmoCategoriesLog.Log.LogWrite("Weapon Effect Fire for bullet with shells effect. HitIndex:" + hitIndex + "\n");
            //int pHitIndex = __instance.hitIndex;
            //int hitIndex = __instance.hitIndex;
            //CustomAmmoCategoriesLog.Log.LogWrite(" parent HitIndex:" + pHitIndex + "\n");
            BulletEffect bullet = __instance as BulletEffect;
            typeof(WeaponEffect).GetField("t", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (object)0.0f);
            __instance.hitIndex = hitIndex
            typeof(WeaponEffect).GetField("emitterIndex", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (object)emitterIndex);
            __instance.hitInfo = hitInfo;
            Transform startingTransform = __instance.weaponRep.vfxTransforms[emitterIndex];
            typeof(WeaponEffect).GetField("startingTransform", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (object)startingTransform);
            typeof(WeaponEffect).GetField("startPos", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (object)startingTransform.position);
            typeof(WeaponEffect).GetField("endPos", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (object)hitInfo.hitPositions[hitIndex]);
            Vector3 startPos = (Vector3)typeof(WeaponEffect).GetField("startPos", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
            Vector3 endPos = (Vector3)typeof(WeaponEffect).GetField("endPos", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
            typeof(WeaponEffect).GetField("currentPos", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (object)startPos);
            PropertyInfo bulletProperty = typeof(WeaponEffect).GetProperty("FiringComplete");
            bulletProperty.DeclaringType.GetProperty("FiringComplete");
            bulletProperty.GetSetMethod(true).Invoke(__instance, new object[1] { (object)false });
            __instance.InitProjectile();
            __instance.currentState = WeaponEffect.WeaponEffectState.PreFiring;
            CustomAmmoCategoriesLog.Log.LogWrite(" end pos is altered. Become:" + endPos + "\n");
            return false;
          }
        }
        return WeaponEffect_FireTerrain.Prefix(__instance,hitInfo,hitIndex,emitterIndex);
      }
    }*/
    /*
    [HarmonyPatch(typeof(BallisticEffect))]
    [HarmonyPatch("FireNextBullet")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { })]
    [HarmonyPriority(Priority.Normal)]
    public static class BallisticEffect_FireNextBulletShell {
      private static Stopwatch watchdog = new Stopwatch();
      public static bool Prefix(BallisticEffect __instance) {
        if (__instance.subEffect) {
          int hitIndex = __instance.hitIndex;
          CustomAmmoCategoriesLog.Log.LogWrite("Ballistic Effect FireNextBullet as shell effect. hitIndex:" + hitIndex + "/" + __instance.hitInfo.numberOfShots + "\n");
          ShrapnelHitRecord shrapnelHitRecord = CustomAmmoCategories.getShrapnelCache(__instance.hitInfo, hitIndex);
          if (shrapnelHitRecord == null) {
            CustomAmmoCategoriesLog.Log.LogWrite(" can't find shrapnel hit record. fallback\n");
            return true;
          }
          List<BulletEffect> bullets = (List<BulletEffect>)typeof(BallisticEffect).GetField("bullets", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
          int currentBullet = (int)typeof(BallisticEffect).GetField("currentBullet", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
          int currentBulletBorder = shrapnelHitRecord.shellsHitIndex + shrapnelHitRecord.count - __instance.hitInfo.numberOfShots;
          CustomAmmoCategoriesLog.Log.LogWrite("BulletBorder " + currentBulletBorder + "=" + shrapnelHitRecord.shellsHitIndex + "+" + shrapnelHitRecord.count + "-" + __instance.hitInfo.numberOfShots + "\n");
          CustomAmmoCategoriesLog.Log.LogWrite("FireNextBullet shells effect " + currentBullet + "/" + currentBulletBorder + "/" + bullets.Count + " "+ BallisticEffect_FireNextBulletShell.watchdog.ElapsedMilliseconds + "\n");
          if ((currentBullet < 0) || (currentBullet >= currentBulletBorder) || (currentBullet >= bullets.Count)) {
            BallisticEffect_FireNextBulletShell.watchdog.Stop();
            if(BallisticEffect_FireNextBulletShell.watchdog.ElapsedMilliseconds > 3000) {
              CustomAmmoCategoriesLog.Log.LogWrite("It takes too long, need fallback and clean\n",true);
              foreach(var bullet in bullets) {
                bullet.currentState = WeaponEffect.WeaponEffectState.Complete;
              }
              BallisticEffect_FireNextBulletShell.watchdog.Reset();
            }
            BallisticEffect_FireNextBulletShell.watchdog.Start();
            return false;
          }
          try {
            BallisticEffect_FireNextBulletShell.watchdog.Stop();
            BallisticEffect_FireNextBulletShell.watchdog.Reset();
            BallisticEffect_FireNextBulletShell.watchdog.Start();
            BulletEffect bullet = bullets[currentBullet];
            bullet.bulletIdx = currentBullet;
            int bulletHitIndex = currentBullet + __instance.hitInfo.numberOfShots;
            bullet.Fire(__instance.hitInfo, bulletHitIndex, 0);
            bullet.hitIndex = bulletHitIndex;
            CustomAmmoCategoriesLog.Log.LogWrite(" other bullets state (" + bullets.Count + "):\n");
            for (int bulletIdx = 0; bulletIdx < bullets.Count; ++bulletIdx) {
              float t = (float)typeof(WeaponEffect).GetField("t", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(bullets[bulletIdx]);
              int bulletIndex = bullets[bulletIdx].hitIndex;
              CustomAmmoCategoriesLog.Log.LogWrite("  uid:" + bullets[bulletIdx].GetInstanceID() + " idx:" + bullets[bulletIdx].bulletIdx + " hitIndex:" + bulletIndex + " t:" + t + " state:" + bullets[bulletIdx].currentState + "\n");
            }
            typeof(BallisticEffect).GetMethod("PlayMuzzleFlash", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance, new object[0] { });
            ++currentBullet;
          }catch(Exception e) {
            CustomAmmoCategoriesLog.Log.LogWrite("FireNextBullet exception:"+e+"\n");
            CustomAmmoCategoriesLog.Log.LogWrite("Removing current bullet "+currentBullet+"\n");
            bullets.RemoveAt(currentBullet);
          }
          typeof(WeaponEffect).GetField("t", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (object)1.0f);
          typeof(BallisticEffect).GetField("currentBullet", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (object)currentBullet);
          if (currentBullet < bullets.Count) { return false; }
        __instance.currentState = WeaponEffect.WeaponEffectState.WaitingForImpact;
          return false;
        }else
        if (CustomAmmoCategories.getWeaponHasShells(__instance.weapon)) {
          int hitIndex = __instance.hitIndex
          CustomAmmoCategoriesLog.Log.LogWrite("Ballistic Effect FireNextBullet with shell effect. hitIndex:" + hitIndex + "/" + __instance.hitInfo.numberOfShots + "\n");
          List<BulletEffect> bullets = (List<BulletEffect>)typeof(BallisticEffect).GetField("bullets", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
          int currentBullet = (int)typeof(BallisticEffect).GetField("currentBullet", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
          if (currentBullet < 0 || currentBullet >= __instance.hitInfo.numberOfShots) { return false; }
          BulletEffect bullet = bullets[currentBullet];
          bullet.bulletIdx = currentBullet;
          int bulletHitIndex = currentBullet;
          bullet.Fire(__instance.hitInfo, bulletHitIndex, 0);
          bullet.hitIndex = bulletHitIndex;
          typeof(BallisticEffect).GetMethod("PlayMuzzleFlash", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance, new object[0] { });
          ++currentBullet;
          typeof(WeaponEffect).GetField("t", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (object)1.0f);
          typeof(BallisticEffect).GetField("currentBullet", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (object)currentBullet);
          if (currentBullet < __instance.hitInfo.numberOfShots) { return false; }
          __instance.currentState = WeaponEffect.WeaponEffectState.WaitingForImpact;
          return false;
        }
        return true;
      }
    }*/
    /*
    [HarmonyPatch(typeof(BallisticEffect))]
    [HarmonyPatch("OnBulletImpact")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(BulletEffect) })]
    [HarmonyPriority(Priority.Normal)]
    public static class BallisticEffect_OnBulletImpactShell {
      public static bool Prefix(BallisticEffect __instance, BulletEffect bullet) {
        if (__instance.subEffect) {
          int hitIndex = bullet.hitIndex;
          CustomAmmoCategoriesLog.Log.LogWrite("Ballistic Effect OnBulletImpact as shell effect. bullet hitIndex:" + hitIndex + "\n");
          ShrapnelHitRecord shrapnelHitRecord = CustomAmmoCategories.getShrapnelCache(__instance.hitInfo, hitIndex);
          if (shrapnelHitRecord == null) {
            CustomAmmoCategoriesLog.Log.LogWrite(" can't find shrapnel hit record. fallback\n");
            return true;
          }
          //int bulletHitIndex = bullet.hitIndex;
          //CustomAmmoCategoriesLog.Log.LogWrite("Shell bullet hit detected hitIndex:" + bulletHitIndex + "/" + shrapnelHitRecord.hitInfo.numberOfShots);
          CustomAmmoCategoriesLog.Log.LogWrite(" location:" + __instance.hitInfo.hitLocations[hitIndex] + "\n");
          if (__instance.hitInfo.hitLocations[hitIndex] == 0 || __instance.hitInfo.hitLocations[hitIndex] == 65536) { return false; };
          CombatGameState Combat = (CombatGameState)typeof(WeaponEffect).GetField("Combat", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
          //Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AttackSequenceImpactMessage(__instance.hitInfo, bulletHitIndex, __instance.weapon.DamagePerShotAdjusted(__instance.weapon.parent.occupiedDesignMask)));
          //typeof(WeaponEffect).GetMethod("OnImpact", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[1] { (object)__instance.weapon.DamagePerShotAdjusted(__instance.weapon.parent.occupiedDesignMask) });
          //int effectiveHitIndex = shrapnelHitRecord.shellsHitIndex + bulletHitIndex;
          SpreadHitRecord spreadRecord = CustomAmmoCategories.getSpreadCache(__instance.hitInfo, hitIndex);
          string targetGUID = __instance.hitInfo.targetId;
          if (spreadRecord != null) { targetGUID = spreadRecord.targetGUID; };
          AbstractActor combatantByGuid = Combat.FindCombatantByGUID(targetGUID) as AbstractActor;
          if (combatantByGuid == null || !((UnityEngine.Object)combatantByGuid.GameRep != (UnityEngine.Object)null)) { return false; }
          combatantByGuid.GameRep.PlayImpactAnim(__instance.hitInfo, hitIndex, __instance.weapon, MeleeAttackType.NotSet, 0.0f);
          return false;
        } else {
          if (CustomAmmoCategories.getWeaponHasShells(__instance.weapon)) {
            CustomAmmoCategoriesLog.Log.LogWrite(" main effect with shell\n");
            int hitIndex = bullet.hitIndex;
            WeaponHitInfo hitInfo = __instance.hitInfo;
            if (hitInfo.attackSequenceId == 0) {
              CustomAmmoCategoriesLog.Log.LogWrite(" strange behavior. Ballistic effect without hitInfo\n");
              CustomAmmoCategoriesLog.Log.LogWrite(" instance:" + bullet.GetInstanceID() + " seqID:" + bullet.hitInfo.attackSequenceId + " grp:" + bullet.hitInfo.attackGroupIndex + " wId:" + bullet.hitInfo.attackWeaponIndex + " hi:" + hitIndex + "\n");
              hitInfo = bullet.hitInfo;
              if(hitInfo.attackSequenceId == 0) {
                CustomAmmoCategoriesLog.Log.LogWrite("CRITICAL! Can't found hit info\n",true);
                CustomAmmoCategoriesLog.Log.LogWrite(" instance:" + bullet.GetInstanceID() + " seqID:" + bullet.hitInfo.attackSequenceId + " grp:" + bullet.hitInfo.attackGroupIndex + " wId:" + bullet.hitInfo.attackWeaponIndex + " hi:" + hitIndex + "\n");
                return false;
              }
              __instance.hitInfo = hitInfo;
            }
            ShrapnelHitRecord shrapnelHitRecord = CustomAmmoCategories.getShrapnelCache(hitInfo, hitIndex);
            if(shrapnelHitRecord == null) {
              CustomAmmoCategoriesLog.Log.LogWrite(" can't find shrapnel hit record. fallback\n");
              CustomAmmoCategoriesLog.Log.LogWrite(" instance:" + bullet.GetInstanceID() + " seqID:"+ hitInfo.attackSequenceId+" grp:"+ hitInfo.attackGroupIndex+" wId:"+ hitInfo.attackWeaponIndex+" hi:"+hitIndex+"\n");
              return true;
            }
            CustomAmmoCategoriesLog.Log.LogWrite("OnImpact shrapnel Hit info found:" + hitIndex + "\n");
            float hitDamage = __instance.weapon.DamagePerShotAdjusted(__instance.weapon.parent.occupiedDesignMask);
            CombatGameState Combat = (CombatGameState)typeof(WeaponEffect).GetField("Combat", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
            if (shrapnelHitRecord.isSeparated == true) {
              CustomAmmoCategories.shrapnelFireShells(hitInfo, hitIndex, shrapnelHitRecord, __instance.weapon);
              //CustomAmmoCategories.shrapnelFireShells(__instance.hitInfo, hitIndex, shrapnelHitRecord);
              CustomAmmoCategoriesLog.Log.LogWrite(" applying shells damage:" + hitIndex + " dmg:" + hitDamage + "\n");
              float effectiveSeparatedDamage = hitDamage / (float)shrapnelHitRecord.count;
              for (int shHitIndex = 0; shHitIndex < shrapnelHitRecord.count; ++shHitIndex) {
                int shrapnelHitIndex = (shHitIndex + shrapnelHitRecord.shellsHitIndex);
                CustomAmmoCategoriesLog.Log.LogWrite("  shellsHitIndex = " + shrapnelHitIndex + " dmg:" + effectiveSeparatedDamage + "\n");
                Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AttackSequenceImpactMessage(hitInfo, shrapnelHitIndex, effectiveSeparatedDamage));
              }
              return false;
            } else {
              CustomAmmoCategoriesLog.Log.LogWrite(" location:" + hitInfo.hitLocations[hitIndex] + "\n");
              if (hitInfo.hitLocations[hitIndex] == 0 || hitInfo.hitLocations[hitIndex] == 65536) { return false; };
              float unsepDmgM = CustomAmmoCategories.getWeaponUnseparatedDamageMult(__instance.weapon);
              CustomAmmoCategoriesLog.Log.LogWrite(" not separated. Lower damage. Was:" + hitDamage);
              hitDamage *= unsepDmgM;
              CustomAmmoCategoriesLog.Log.LogWrite(" become:" + hitDamage + "\n");
              Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AttackSequenceImpactMessage(hitInfo, hitIndex, hitDamage));
              //Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AttackSequenceImpactMessage(__instance.hitInfo, bulletHitIndex, __instance.weapon.DamagePerShotAdjusted(__instance.weapon.parent.occupiedDesignMask)));
              //typeof(WeaponEffect).GetMethod("OnImpact", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[1] { (object)__instance.weapon.DamagePerShotAdjusted(__instance.weapon.parent.occupiedDesignMask) });
              //int effectiveHitIndex = shrapnelHitRecord.shellsHitIndex + bulletHitIndex;
              SpreadHitRecord spreadRecord = CustomAmmoCategories.getSpreadCache(bullet.hitInfo, hitIndex);
              string targetGUID = bullet.hitInfo.targetId;
              if (spreadRecord != null) { targetGUID = spreadRecord.targetGUID; };
              AbstractActor combatantByGuid = Combat.FindCombatantByGUID(targetGUID) as AbstractActor;
              if (combatantByGuid == null || !((UnityEngine.Object)combatantByGuid.GameRep != (UnityEngine.Object)null)) { return false; }
              combatantByGuid.GameRep.PlayImpactAnim(bullet.hitInfo, hitIndex, __instance.weapon, MeleeAttackType.NotSet, 0.0f);
              return false;
            }
          }
        }
        return true;
      }
    }*/
    /*
    [HarmonyPatch(typeof(WeaponEffect))]
    [HarmonyPatch("PlayMuzzleFlash")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { })]
    [HarmonyPriority(Priority.Normal)]
    public static class WeaponEffect_PlayMuzzleFlash {
      public static bool Prefix(WeaponEffect __instance) {
        if (__instance is BallisticEffect) {
          if (__instance.subEffect == false) {
            return true;
          }
        } else
        if (__instance is BulletEffect) {
          BallisticEffect parentLauncher = (BallisticEffect)typeof(BulletEffect).GetField("parentLauncher", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
          if (parentLauncher.subEffect == false) {
            return true;
          }
        } else {
          return true;
        }
        int hitIndex = __instance.hitIndex;
        ShrapnelHitRecord shrapnelHitRecord = CustomAmmoCategories.getShrapnelCache(__instance.hitInfo, hitIndex);
        CustomAmmoCategoriesLog.Log.LogWrite("WeaponEffect.PlayMuzzleFlash as shell effect\n");
        if (shrapnelHitRecord == null) {
          CustomAmmoCategoriesLog.Log.LogWrite(" can't find shrapnel hit record. fallback\n");
          return true;
        }
        if (!((UnityEngine.Object)__instance.muzzleFlashVFXPrefab != (UnityEngine.Object)null)) {
          return false;
        }
        GameObject gameObject = __instance.weapon.parent.Combat.DataManager.PooledInstantiate(__instance.muzzleFlashVFXPrefab.name, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
        ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
        AutoPoolObject autoPoolObject = gameObject.GetComponent<AutoPoolObject>();
        if ((UnityEngine.Object)autoPoolObject == (UnityEngine.Object)null) {
          autoPoolObject = gameObject.AddComponent<AutoPoolObject>();
        }
        autoPoolObject.Init(__instance.weapon.parent.Combat.DataManager, __instance.muzzleFlashVFXPrefab.name, component);
        component.Stop(true);
        component.Clear(true);
        Transform startingTransform = (Transform)typeof(WeaponEffect).GetField("startingTransform", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
        component.transform.parent = startingTransform;
        component.transform.localPosition = shrapnelHitRecord.startPosition - startingTransform.position;
        CustomAmmoCategoriesLog.Log.LogWrite(" altered PlayMuzzleFlash pos. Was:" + startingTransform.position + " become:" + component.transform.position + "\n");
        Vector3 endPos = (Vector3)typeof(WeaponEffect).GetField("endPos", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
        component.transform.LookAt(endPos);
        BTCustomRenderer.SetVFXMultiplier(component);
        component.Play(true);
        BTLightAnimator componentInChildren = gameObject.GetComponentInChildren<BTLightAnimator>(true);
        if (!((UnityEngine.Object)componentInChildren != (UnityEngine.Object)null)) { return false; }
        componentInChildren.StopAnimation();
        componentInChildren.PlayAnimation();
        return false;
      }
    }
    [HarmonyPatch(typeof(WeaponEffect))]
    [HarmonyPatch("PlayPreFire")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { })]
    [HarmonyPriority(Priority.Normal)]
    public static class WeaponEffect_PlayPreFire {
      public static bool Prefix(WeaponEffect __instance) {
        CustomAmmoCategoriesLog.Log.LogWrite("WeaponEffect.PlayPreFire\n");
        if (__instance is BallisticEffect) {
          if (__instance.subEffect == false) {
            CustomAmmoCategoriesLog.Log.LogWrite("Ballistic effect but not subeffect\n");
            return true;
          }
        } else
        if (__instance is BulletEffect) {
          BallisticEffect parentLauncher = (BallisticEffect)typeof(BulletEffect).GetField("parentLauncher", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
          if (parentLauncher.subEffect == false) {
            CustomAmmoCategoriesLog.Log.LogWrite("Bullet effect but parent launcher not subeffect\n");
            return true;
          }
        } else {
          CustomAmmoCategoriesLog.Log.LogWrite("Nor ballistic nor bullet effect can't be shell\n");
          return true;
        }
        int hitIndex = __instance.hitIndex;
        ShrapnelHitRecord shrapnelHitRecord = CustomAmmoCategories.getShrapnelCache(__instance.hitInfo, hitIndex);
        CustomAmmoCategoriesLog.Log.LogWrite("WeaponEffect.PlayPreFire as shell effect\n");
        if (shrapnelHitRecord == null) {
          CustomAmmoCategoriesLog.Log.LogWrite(" can't find shrapnel hit record. fallback\n");
          return true;
        }
        if ((UnityEngine.Object)__instance.preFireVFXPrefab != (UnityEngine.Object)null) {
          GameObject gameObject = __instance.weapon.parent.Combat.DataManager.PooledInstantiate(__instance.preFireVFXPrefab.name, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
          ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
          AutoPoolObject autoPoolObject = gameObject.GetComponent<AutoPoolObject>();
          if ((UnityEngine.Object)autoPoolObject == (UnityEngine.Object)null) {
            autoPoolObject = gameObject.AddComponent<AutoPoolObject>();
          }
          autoPoolObject.Init(__instance.weapon.parent.Combat.DataManager, __instance.preFireVFXPrefab.name, component);
          component.Stop(true);
          component.Clear(true);
          component.transform.parent = (Transform)null;
          component.transform.position = shrapnelHitRecord.startPosition;
          Vector3 endPos = (Vector3)typeof(WeaponEffect).GetField("endPos", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
          component.transform.LookAt(endPos);
          BTCustomRenderer.SetVFXMultiplier(component);
          component.Play(true);
          if ((double)__instance.preFireDuration <= 0.0) {
            __instance.preFireDuration = component.main.duration;
          }
        }
        if (!string.IsNullOrEmpty(__instance.preFireSFX)) {
          AkGameObj parentAudioObject = (AkGameObj)typeof(WeaponEffect).GetField("parentAudioObject", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
          int num = (int)WwiseManager.PostEvent(__instance.preFireSFX, parentAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
        }
        float preFireRate = (double)__instance.preFireDuration <= 0.0 ? 1000f : 1f / __instance.preFireDuration;
        typeof(WeaponEffect).GetField("preFireRate", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (object)preFireRate);
        if ((double)__instance.attackSequenceNextDelayMin <= 0.0 && (double)__instance.attackSequenceNextDelayMax <= 0.0) {
          __instance.attackSequenceNextDelayMax = __instance.preFireDuration;
        }
        if ((double)__instance.attackSequenceNextDelayMax <= 0.0) {
          __instance.attackSequenceNextDelayMax = 0.05f;
        }
        if ((double)__instance.attackSequenceNextDelayMin >= (double)__instance.attackSequenceNextDelayMax) {
          __instance.attackSequenceNextDelayMin = __instance.attackSequenceNextDelayMax;
        }
        float attackSequenceNextDelayTimer = UnityEngine.Random.Range(__instance.attackSequenceNextDelayMin, __instance.attackSequenceNextDelayMax);
        typeof(WeaponEffect).GetField("attackSequenceNextDelayTimer", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (object)attackSequenceNextDelayTimer);
        typeof(WeaponEffect).GetField("t", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (object)0f);
        __instance.currentState = WeaponEffect.WeaponEffectState.PreFiring;
        return false;
      }
    }
    [HarmonyPatch(typeof(WeaponEffect))]
    [HarmonyPatch("PlayProjectile")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { })]
    [HarmonyPriority(Priority.First)]
    public static class WeaponEffect_PlayProjectileShell {
      public static bool Prefix(WeaponEffect __instance) {
        CustomAmmoCategoriesLog.Log.LogWrite("WeaponEffect.PlayProjectile shell\n");
        if (__instance is BallisticEffect) {
          if (__instance.subEffect == false) {
            CustomAmmoCategoriesLog.Log.LogWrite("Ballistic effect but not subeffect\n");
            return WeaponEffect_PlayProjectile.Prefix(__instance);
          }
        } else
        if (__instance is BulletEffect) {
          BallisticEffect parentLauncher = (BallisticEffect)typeof(BulletEffect).GetField("parentLauncher", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
          if (parentLauncher.subEffect == false) {
            CustomAmmoCategoriesLog.Log.LogWrite("Bullet effect but parent launcher not subeffect\n");
            return WeaponEffect_PlayProjectile.Prefix(__instance);
          }
        } else {
          CustomAmmoCategoriesLog.Log.LogWrite("Nor ballistic nor bullet effect can't be shell\n");
          return WeaponEffect_PlayProjectile.Prefix(__instance);
        }
        CustomAmmoCategoriesLog.Log.LogWrite("WeaponEffect.PlayProjectile as shell effect\n");
        int hitIndex = __instance.hitIndex;
        ShrapnelHitRecord shrapnelHitRecord = CustomAmmoCategories.getShrapnelCache(__instance.hitInfo, hitIndex);
        if (shrapnelHitRecord == null) {
          CustomAmmoCategoriesLog.Log.LogWrite(" can't find shrapnel hit record. fallback\n");
          return WeaponEffect_PlayProjectile.Prefix(__instance);
        }
        if (__instance is BulletEffect) {
          CustomAmmoCategoriesLog.Log.LogWrite(" add bullet in fly:" + hitIndex + "\n");
          BallisticEffect parentLauncher = (BallisticEffect)typeof(BulletEffect).GetField("parentLauncher", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
          shrapnelHitRecord.bulletsInFly.Add(hitIndex);
          List<BulletEffect> bullets = (List<BulletEffect>)typeof(BallisticEffect).GetField("bullets", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(parentLauncher);
          CustomAmmoCategoriesLog.Log.LogWrite(" other bullets state (" + bullets.Count + "):\n");
          for (int bulletIdx = 0; bulletIdx < bullets.Count; ++bulletIdx) {
            float t = (float)typeof(WeaponEffect).GetField("t", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(bullets[bulletIdx]);
            int bulletIndex = bullets[bulletIdx].hitIndex;
            CustomAmmoCategoriesLog.Log.LogWrite(" idx:" + bullets[bulletIdx].bulletIdx + " hitIndex:" + bulletIndex + " t:" + t + " state:" + bullets[bulletIdx].currentState + "\n");
          }
        }
        typeof(WeaponEffect).GetField("t", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (object)0f);
        __instance.currentState = WeaponEffect.WeaponEffectState.Firing;
        GameObject projectileMeshObject = (GameObject)typeof(WeaponEffect).GetField("projectileMeshObject", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
        GameObject projectileLightObject = (GameObject)typeof(WeaponEffect).GetField("projectileLightObject", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
        ParticleSystem projectileParticles = (ParticleSystem)typeof(WeaponEffect).GetField("projectileParticles", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
        Transform projectileTransform = (Transform)typeof(WeaponEffect).GetField("projectileTransform", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
        if ((UnityEngine.Object)projectileMeshObject != (UnityEngine.Object)null) {
          projectileMeshObject.SetActive(true);
        }
        if ((UnityEngine.Object)projectileLightObject != (UnityEngine.Object)null) {
          projectileLightObject.SetActive(true);
        }
        if ((UnityEngine.Object)projectileParticles != (UnityEngine.Object)null) {
          projectileParticles.Stop(true);
          projectileParticles.Clear(true);
        }
        projectileTransform.position = shrapnelHitRecord.startPosition;
        Vector3 endPos = (Vector3)typeof(WeaponEffect).GetField("endPos", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
        projectileTransform.LookAt(endPos);
        typeof(WeaponEffect).GetField("startPos", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (object)shrapnelHitRecord.startPosition);
        if ((UnityEngine.Object)projectileParticles != (UnityEngine.Object)null) {
          BTCustomRenderer.SetVFXMultiplier(projectileParticles);
          projectileParticles.Play(true);
          BTLightAnimator componentInChildren = projectileParticles.GetComponentInChildren<BTLightAnimator>(true);
          if ((UnityEngine.Object)componentInChildren != (UnityEngine.Object)null) {
            componentInChildren.StopAnimation();
            componentInChildren.PlayAnimation();
          }
        }
        if (!__instance.AllowMissSkipping || __instance.hitInfo.hitLocations[hitIndex] != 0 && __instance.hitInfo.hitLocations[hitIndex] != 65536) {
          return false;
        }
        __instance.PublishWeaponCompleteMessage();
        return false;
      }
    }
    [HarmonyPatch(typeof(BulletEffect))]
    [HarmonyPatch("OnComplete")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { })]
    [HarmonyPriority(Priority.HigherThanNormal)]
    public static class BulletEffect_OnCompleteShell {
      public static void Postfix(WeaponEffect __instance) {
        CustomAmmoCategoriesLog.Log.LogWrite("BulletEffect.OnComplete shell\n");
        int hitIndex = __instance.hitIndex;
        BallisticEffect parentLauncher = (BallisticEffect)typeof(BulletEffect).GetField("parentLauncher", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
        if (parentLauncher.subEffect == false) {
          CustomAmmoCategoriesLog.Log.LogWrite("Bullet effect but parent launcher not subeffect\n");
          return;
        }
        int pHitIndex = parentLauncher.hitIndex;
        ShrapnelHitRecord shrapnelHitRecord = CustomAmmoCategories.getShrapnelCache(__instance.hitInfo, pHitIndex);
        if (shrapnelHitRecord == null) {
          CustomAmmoCategoriesLog.Log.LogWrite(" can't find shrapnel hit record. fallback\n");
          return;
        }
        CustomAmmoCategoriesLog.Log.LogWrite(" remove bullet from fly:" + hitIndex + "\n");
        List<BulletEffect> bullets = (List<BulletEffect>)typeof(BallisticEffect).GetField("bullets", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(parentLauncher);
        CustomAmmoCategoriesLog.Log.LogWrite(" other bullets state (" + bullets.Count + "):\n");
        for (int bulletIdx = 0; bulletIdx < bullets.Count; ++bulletIdx) {
          float t = (float)typeof(WeaponEffect).GetField("t", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(bullets[bulletIdx]);
          int bulletIndex = bullets[bulletIdx].hitIndex;
          CustomAmmoCategoriesLog.Log.LogWrite(" idx:" + bullets[bulletIdx].bulletIdx + " hitIndex:" + bulletIndex + " t:" + t + " state:" + bullets[bulletIdx].currentState + "\n");
        }
        shrapnelHitRecord.bulletsInFly.Remove(hitIndex);
        //for (int bulletIdx = 0; bulletIdx < bullets.Count; ++bulletIdx) {
        //}
      }
    }
    [HarmonyPatch(typeof(BulletEffect))]
    [HarmonyPatch("ImpactPrecacheCount")]
    [HarmonyPatch(MethodType.Getter)]
    [HarmonyPatch(new Type[] { })]
    public static class BulletEffect_ImpactPrecacheCount {
      public static void Postfix(BulletEffect __instance, ref int __result) {
        __result = 20;
      }
    }
    [HarmonyPatch(typeof(BallisticEffect))]
    [HarmonyPatch("ImpactPrecacheCount")]
    [HarmonyPatch(MethodType.Getter)]
    [HarmonyPatch(new Type[] { })]
    public static class BallisticEffect_ImpactPrecacheCount {
      public static void Postfix(BallisticEffect __instance, ref int __result) {
        __result = 20;
      }
    }
    [HarmonyPatch(typeof(MissileLauncherEffect))]
    [HarmonyPatch("Fire")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(WeaponHitInfo), typeof(int), typeof(int) })]
    public static class MissileLauncherEffect_FireShells {
      public static void Postfix(MissileLauncherEffect __instance, ref WeaponHitInfo hitInfo, int hitIndex, int emitterIndex) {
        if (CustomAmmoCategories.getWeaponHasShells(__instance.weapon)) {
          if (hitIndex == 0) {
            CustomAmmoCategories.setupShellsBullets(__instance.weapon, ref hitInfo);
          }
        }
        return;
      }
    }
    [HarmonyPatch(typeof(BallisticEffect))]
    [HarmonyPatch("Fire")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(WeaponHitInfo), typeof(int), typeof(int) })]
    public static class BallisticEffect_FireShells {
      public static void Postfix(MissileLauncherEffect __instance, ref WeaponHitInfo hitInfo, int hitIndex, int emitterIndex) {
        if (CustomAmmoCategories.getWeaponHasShells(__instance.weapon)) {
          if (__instance.subEffect == false) {
            if (hitIndex == 0) {
              CustomAmmoCategoriesLog.Log.LogWrite("setup shell bullets " + hitInfo.attackGroupIndex + " " + hitInfo.attackWeaponIndex + " " + hitIndex + "\n");
              CustomAmmoCategories.setupShellsBullets(__instance.weapon, ref hitInfo);
            }
          }
        }
        return;
      }
    }
    [HarmonyPatch(typeof(BallisticEffect))]
    [HarmonyPatch("OnComplete")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { })]
    [HarmonyBefore("com.joelmeador.WeaponRealizer")]
    [HarmonyPriority(Priority.First)]
    public static class BallisticEffect_OnCompleteShell {
      private static Action<BallisticEffect> WeaponEffect_OnComplete;
      public static bool Prepare() {
        BuildWeaponEffectOnComplete();
        return true;
      }
      private static void BuildWeaponEffectOnComplete() {
        // build a call to WeaponEffect.OnComplete() so it can be called
        // a la base.OnComplete() from the context of a BallisticEffect
        // https://blogs.msdn.microsoft.com/rmbyers/2008/08/16/invoking-a-virtual-method-non-virtually/
        // https://docs.microsoft.com/en-us/dotnet/api/system.activator?view=netframework-3.5
        // https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.dynamicmethod.-ctor?view=netframework-3.5#System_Reflection_Emit_DynamicMethod__ctor_System_String_System_Type_System_Type___System_Type_
        // https://stackoverflow.com/a/4358250/1976
        var method = typeof(WeaponEffect).GetMethod("OnComplete", AccessTools.all);
        var dm = new DynamicMethod("CACWeaponEffectOnComplete", null, new Type[] { typeof(BallisticEffect) }, typeof(BallisticEffect));
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Call, method);
        gen.Emit(OpCodes.Ret);
        WeaponEffect_OnComplete = (Action<BallisticEffect>)dm.CreateDelegate(typeof(Action<BallisticEffect>));
      }
      public static bool Prefix(BallisticEffect __instance) {
        //int hitIndex = __instance.hitIndex;
        if (__instance.subEffect) {
          CustomAmmoCategoriesLog.Log.LogWrite("BallisticEffect OnComplete shell effect - no further complete\n");
          WeaponEffect_OnComplete(__instance);
          return false;
        }else
        if (CustomAmmoCategories.getWeaponHasShells(__instance.weapon)) {
          CustomAmmoCategoriesLog.Log.LogWrite("BallisticEffect OnComplete with shells effect. No impact needed\n");
          WeaponEffect_OnComplete(__instance);
          return false;
        }
        return true;
      }
    }
    [HarmonyPatch(typeof(BallisticEffect))]
    [HarmonyPatch("SetupBullets")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { })]
    [HarmonyPriority(Priority.First)]
    public static class BallisticEffect_SetupBulletsShell {
      static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
        var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "ProjectilesPerShot").GetGetMethod();
        var replacementMethod = AccessTools.Method(typeof(BallisticEffect_SetupBulletsShell), nameof(ProjectilesPerShot));
        return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
      }

      private static int ProjectilesPerShot(Weapon weapon) {
        if (CustomAmmoCategories.getWeaponHasShells(weapon)) {
          return weapon.ShotsWhenFired;
        }
        return weapon.ProjectilesPerShot;
        //CustomAmmoCategoriesLog.Log.LogWrite("get AIUtil_UnitHasLOFToTargetFromPosition IndirectFireCapable\n");
      }
      public static bool Prefix(BallisticEffect __instance) {
        if (CustomAmmoCategories.getWeaponHasShells(__instance.weapon) == false) {return true;}
        CustomAmmoCategoriesLog.Log.LogWrite("SetupBullets as for bullets with shell effects\n");
        typeof(BallisticEffect).GetField("currentBullet", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (object)0);
        if ((double)__instance.shotDelay <= 0.0) {
          __instance.shotDelay = 0.5f;
        }
        float rate = 1f / __instance.shotDelay;
        typeof(WeaponEffect).GetField("rate", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (object)rate);
        typeof(BallisticEffect).GetMethod("ClearBullets", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance, new object[0] { });
        CombatGameState Combat = (CombatGameState)typeof(WeaponEffect).GetField("Combat", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
        List<BulletEffect> bullets = (List<BulletEffect>)typeof(BallisticEffect).GetField("bullets", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
        HashSet<int> lastBulletUID = new HashSet<int>();
        for (int index = 0; index < __instance.hitInfo.numberOfShots; ++index) {
          GameObject gameObject = Combat.DataManager.PooledInstantiate(__instance.bulletPrefab.name, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
          if ((UnityEngine.Object)gameObject == (UnityEngine.Object)null) {
            CustomAmmoCategoriesLog.Log.LogWrite("Error instantiating BulletObject " + __instance.bulletPrefab.name + "\n", true);
            continue;
          }
          gameObject.transform.parent = (Transform)null;
          if (lastBulletUID.Contains(gameObject.GetInstanceID())) {
            CustomAmmoCategoriesLog.Log.LogWrite("bullets list already contains:" + gameObject.GetInstanceID() + "\n");
            gameObject = GameObject.Instantiate(gameObject);
            CustomAmmoCategoriesLog.Log.LogWrite("creating new one with id:" + gameObject.GetInstanceID() + "\n");
            lastBulletUID.Add(gameObject.GetInstanceID());
          } else {
            CustomAmmoCategoriesLog.Log.LogWrite("bullets list not contains:" + gameObject.GetInstanceID() + "\n");
            lastBulletUID.Add(gameObject.GetInstanceID());
          }
          BulletEffect component = gameObject.GetComponent<BulletEffect>();
          if ((UnityEngine.Object)component == (UnityEngine.Object)null) {
            CustomAmmoCategoriesLog.Log.LogWrite("Error finding BulletEffect on GO " + __instance.bulletPrefab.name + "\n", true);
            continue;
          }
          CustomAmmoCategoriesLog.Log.LogWrite(" add bullet uid:" + component.GetInstanceID() + "\n");
          bullets.Add(component);
          component.Init(__instance.weapon, __instance);
        }
        return false;
      }
    }
    [HarmonyPatch(typeof(WeaponEffect))]
    [HarmonyPatch("InitProjectile")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { })]
    public static class WeaponEffect_InitProjectileShells {
      public static bool Prefix(WeaponEffect __instance) {
        if ((UnityEngine.Object)__instance.projectilePrefab != (UnityEngine.Object)null && (UnityEngine.Object)__instance.projectile != (UnityEngine.Object)null) {
          string activeProjectileName = (string)typeof(WeaponEffect).GetField("activeProjectileName", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
          if (string.IsNullOrEmpty(activeProjectileName)) {
            if (string.IsNullOrEmpty(__instance.projectilePrefab.name) == false) {
              typeof(WeaponEffect).GetField("activeProjectileName", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, __instance.projectilePrefab.name);
            } else {
              CustomAmmoCategoriesLog.Log.LogWrite("This should not happend. Projectile prefab is null! Projectile should not be inited\n",true);
              return false;
            }
          }
        }
        return true;
      }
    }*/
  }
}