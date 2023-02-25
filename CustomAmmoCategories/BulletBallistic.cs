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
using FluffyUnderware.Curvy;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;

namespace CustAmmoCategories {
  public class BulletCurvyInfo {
    public CurvySpline UnitySpline;
    public GameObject splineObject;
    public Vector3[] spline;
    public Vector3 startPos;
    public Vector3 endPos;
    public ICombatant target;
    public BulletCurvyInfo() {
      splineObject = new GameObject();
      this.UnitySpline = splineObject.AddComponent<CurvySpline>(); ;
    }
    public void freeResources() {
      if (splineObject != null) {
        GameObject.Destroy(UnitySpline);
        GameObject.Destroy(splineObject);
      }
    }
  }
  public static partial class CustomAmmoCategories {
    public static Action<BulletEffect> WeaponEffect_UpdateBullet;
    public static Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, BulletCurvyInfo>>>> BulletCurveCache = new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, BulletCurvyInfo>>>>();
    public static BulletCurvyInfo getCachedBulletCurve(WeaponHitInfo hitInfo, int hitIndex) {
      if (CustomAmmoCategories.BulletCurveCache == null) { return null; };
      if (CustomAmmoCategories.BulletCurveCache.ContainsKey(hitInfo.attackSequenceId) == false) { return null; }
      if (CustomAmmoCategories.BulletCurveCache[hitInfo.attackSequenceId].ContainsKey(hitInfo.attackGroupIndex) == false) { return null; }
      if (CustomAmmoCategories.BulletCurveCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex].ContainsKey(hitInfo.attackWeaponIndex) == false) { return null; }
      if (CustomAmmoCategories.BulletCurveCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex][hitInfo.attackWeaponIndex].ContainsKey(hitIndex) == false) { return null; }
      return CustomAmmoCategories.BulletCurveCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex][hitInfo.attackWeaponIndex][hitIndex];
    }

    /*public static void generateBulletCurveCache(Weapon weapon, WeaponHitInfo hitInfo, int hitIndex) {
      WeaponEffect currentEffect = CustomAmmoCategories.getWeaponEffect(weapon);
      if (currentEffect == null) { return; }
      Transform startingTransform = weapon.weaponRep.vfxTransforms[0];
      CombatGameState Combat = (CombatGameState)typeof(WeaponEffect).GetField("Combat", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(currentEffect);
      BulletCurvyInfo cachedCurve = new BulletCurvyInfo();
      cachedCurve.startPos = startingTransform.position;
      //SpreadHitRecord spreadCacheRecord = CustomAmmoCategories.getSpreadCache(hitInfo, hitIndex);
      string targetGUID = hitInfo.targetId;
      if (spreadCacheRecord != null) { targetGUID = spreadCacheRecord.targetGUID; };
      ICombatant combatantByGuid = Combat.FindCombatantByGUID(targetGUID);
      cachedCurve.target = combatantByGuid;
      int hitLocation = hitInfo.hitLocations[hitIndex];
      if (combatantByGuid != null) {
        string secondaryTargetId = (string)null;
        int secondaryHitLocation = 0;
        cachedCurve.endPos = combatantByGuid.GetImpactPosition(weapon.weaponRep.parentCombatant as AbstractActor, cachedCurve.startPos, weapon, ref hitLocation, ref hitInfo.attackDirections[hitIndex], ref secondaryTargetId, ref secondaryHitLocation);
        hitInfo.hitPositions[hitIndex] = cachedCurve.endPos;
        if (spreadCacheRecord != null) {
          spreadCacheRecord.hitInfo.hitPositions[spreadCacheRecord.internalIndex] = cachedCurve.endPos;
          CustomAmmoCategoriesLog.Log.LogWrite("Altering spread position. target " + combatantByGuid.DisplayName + " " + combatantByGuid.GUID + " " + spreadCacheRecord.hitInfo.hitPositions[spreadCacheRecord.internalIndex] + "\n");
        }
      } else {
        cachedCurve.endPos = hitInfo.hitPositions[hitIndex];
      }

      cachedCurve.spline = CustomAmmoCategories.GenerateIndirectMissilePath(1f, 2, true, hitInfo.hitLocations[hitIndex], cachedCurve.startPos, cachedCurve.endPos, Combat);
      cachedCurve.UnitySpline.Interpolation = CurvyInterpolation.Bezier;
      cachedCurve.UnitySpline.Clear();
      cachedCurve.UnitySpline.Closed = false;
      cachedCurve.UnitySpline.Add(cachedCurve.spline);
      cachedCurve.UnitySpline.Refresh();
      if (CustomAmmoCategories.BulletCurveCache.ContainsKey(hitInfo.attackSequenceId) == false) {
        CustomAmmoCategories.BulletCurveCache.Add(hitInfo.attackSequenceId, new Dictionary<int, Dictionary<int, Dictionary<int, BulletCurvyInfo>>>());
      }
      if (CustomAmmoCategories.BulletCurveCache[hitInfo.attackSequenceId].ContainsKey(hitInfo.attackGroupIndex) == false) {
        CustomAmmoCategories.BulletCurveCache[hitInfo.attackSequenceId].Add(hitInfo.attackGroupIndex, new Dictionary<int, Dictionary<int, BulletCurvyInfo>>());
      };
      if (CustomAmmoCategories.BulletCurveCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex].ContainsKey(hitInfo.attackWeaponIndex) == false) {
        CustomAmmoCategories.BulletCurveCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex].Add(hitInfo.attackWeaponIndex, new Dictionary<int, BulletCurvyInfo>());
      };
      CustomAmmoCategories.BulletCurveCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex][hitInfo.attackWeaponIndex].Add(hitIndex, cachedCurve);
    }*/
  }
}
