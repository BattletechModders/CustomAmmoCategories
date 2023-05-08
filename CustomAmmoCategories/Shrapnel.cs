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
        Log.Combat?.WL(1,"interpolating pos: " + t + "(" + Mathf.Abs(t - 1f) + ") dfs:" + distanceFromStart + "/" + sMin + " dtt:" + distanceToTarget + "/" + sMax);
        if (distanceFromStart >= sMin) {
          if (distanceToTarget <= sMax) { rudeAproachFound = true; break; };
        }
      }
      Log.Combat?.WL(1, "result: " + t + "(" + Mathf.Abs(t - 1.1f) + "):" + rudeAproachFound);
      if (rudeAproachFound) {
        Log.Combat?.WL(1, "rude approach found:" + t);
        for (float tt = t - 0.1f; tt <= t; tt += 0.01f) {
          Vector3 missilePos = UnitySpline.InterpolateByDistance(UnitySpline.Length * tt);
          float distanceFromStart = Vector3.Distance(startPos, missilePos);
          float distanceToTarget = Vector3.Distance(targetPos, missilePos);
          Log.Combat?.WL(1, "interpolating precisely pos: " + tt + " dfs:" + distanceFromStart + "/" + sMin + " dtt:" + distanceToTarget + "/" + sMax);
          if (distanceFromStart >= sMin) {
            if (distanceToTarget <= sMax) {
              Log.Combat?.WL(1, "position found:" + missilePos);
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
      Log.Combat?.WL(1, "interpolating from " + startPos + " to " + endPos + " target: " + targetPos);
      for (t = 0.1f; Mathf.Abs(t - 1.1f) > 0.001f; t += 0.1f) {
        Vector3 missilePos = Vector3.Lerp(startPos, endPos, t);
        float distanceFromStart = Vector3.Distance(startPos, missilePos);
        float distanceToTarget = Vector3.Distance(targetPos, missilePos);
        Log.Combat?.WL(1, "interpolating pos: " + t + " dfs:" + distanceFromStart + "/" + sMin + " dtt:" + distanceToTarget + "/" + sMax);
        if (distanceFromStart >= sMin) {
          if (distanceToTarget <= sMax) { rudeAproachFound = true; break; };
        }
      }
      Log.Combat?.WL(1, "result: " + t + "(" + Mathf.Abs(t - 1.1f) + "):" + rudeAproachFound);
      if (rudeAproachFound) {
        Log.Combat?.WL(1, "rude approach found:" + t);
        for (float tt = t - 0.1f; tt <= t; tt += 0.01f) {
          Vector3 missilePos = Vector3.Lerp(startPos, endPos, tt);
          float distanceFromStart = Vector3.Distance(startPos, missilePos);
          float distanceToTarget = Vector3.Distance(targetPos, missilePos);
          Log.Combat?.WL(1, "interpolating precisely pos: " + tt + " dfs:" + distanceFromStart + "/" + sMin + " dtt:" + distanceToTarget + "/" + sMax);
          if (distanceFromStart >= sMin) {
            if (distanceToTarget <= sMax) {
              Log.Combat?.WL(1, "position found:" + missilePos);
              return missilePos;
            };
          }
        }
      }
      Log.Combat?.WL(1, "no separation");
      return Vector3.zero;
    }
    public static void printHitPositions(this WeaponHitInfo hitInfo) {
      Log.Combat?.WL(1, "WeaponPositions:" + hitInfo.attackWeaponIndex);
      for (int t = 0; t < hitInfo.numberOfShots; ++t) {
        Log.Combat?.WL(1, hitInfo.hitPositions[t].ToString());
      }
    }
  }
  namespace CustomAmmoCategoriesPatches {
    [HarmonyPatch(typeof(AttackDirector.AttackSequence), "GenerateRandomCache")]
    [HarmonyPriority(Priority.First)]
    public static class ClusteredShotRandomCacheEnabler {
      public static bool Prefix(AttackDirector.AttackSequence __instance) {
        List<List<Weapon>> sortedWeapons = __instance.sortedWeapons;
        Dictionary<string, bool> _isClustered = WeaponRealizer.ClusteredShotRandomCacheEnabler._isClustered;
        foreach (var weaponsGroup in sortedWeapons) {
          foreach (var weapon in weaponsGroup) {
            //ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
            if (weapon.HasShells() || weapon.DamagePerPallet()) {
              Log.Combat?.WL(0,"GenerateRandomCache " + weapon.defId + " tie to ClusterRandomCache true");
              _isClustered[weapon.defId] = true;
            }
          }
        }
        return true;
      }
    }
  }
}