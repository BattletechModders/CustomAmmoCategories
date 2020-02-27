using BattleTech;
using CleverGirlAIDamagePrediction;
using CustomAmmoCategoriesLog;
using Harmony;
using Localize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CustAmmoCategories {
  public static class BraceNode_Tick {
    private static Dictionary<AbstractActor, int> bracedWithFireRound = new Dictionary<AbstractActor, int>();
    private static bool isBracedWithFireThisRound(this AbstractActor unit) {
      if(bracedWithFireRound.TryGetValue(unit,out int round)) {
        return round == unit.Combat.TurnDirector.CurrentRound;
      } else {
        return false;
      }
    }
    private static void BracedWithFireThisRound(this AbstractActor unit) {
      if (bracedWithFireRound.ContainsKey(unit) == false) { bracedWithFireRound.Add(unit, unit.Combat.TurnDirector.CurrentRound); } else {
        bracedWithFireRound[unit] = unit.Combat.TurnDirector.CurrentRound;
      }
    }
    public static void Clear() { bracedWithFireRound.Clear(); }
    public static bool Prefix(LeafBehaviorNode __instance,ref string ___name,ref BehaviorTree ___tree, AbstractActor ___unit,ref BehaviorNodeState ___currentState, ref BehaviorTreeResults __result) {
      try {
        if (CustomAmmoCategories.Settings.extendedBraceBehavior == false) { return false; }
        Log.M.TWL(0, "BraceNode.Tick()");
        Log.M.WL(1, "name:" + ___name);
        Log.M.WL(1, "unit:" + new Text(___unit.DisplayName));
        Log.M.WL(1, "type:" + ___unit.UnitType);
        Log.M.WL(1, "HasFiredThisRound:" + ___unit.HasFiredThisRound+"/"+___unit.isBracedWithFireThisRound()+"("+___unit.Combat.TurnDirector.CurrentRound+")");
        if (___unit.isBracedWithFireThisRound()) {
          ___unit.HasFiredThisRound = true;
          return true;
        };
        if (___unit.HasFiredThisRound) { return true; };
        Mech mech = ___unit as Mech;
        if (mech != null) {
          Log.M.WL(1, "");
          float heatLevelForMech = AIUtil.GetAcceptableHeatLevelForMech(mech);
          Log.M.WL(1, "heat:" + mech.CurrentHeat + "/" + heatLevelForMech);
          if (mech.CurrentHeat > heatLevelForMech) { return true; }
          Log.M.WL(1, "unsteady:" + mech.IsUnsteady);
          if (mech.IsUnsteady) { return true; }
        }
        Log.M.WL(1, "HasAnyContactWithEnemy:" + ___unit.HasAnyContactWithEnemy);
        if (___unit.HasAnyContactWithEnemy == false) { return true; }
        List<AbstractActor> VisibleEnemyUnits = ___unit.GetVisibleEnemyUnits();
        Log.M.WL(1, "VisibleEnemyUnits:" + VisibleEnemyUnits.Count);
        if (VisibleEnemyUnits.Count > 0) { return true; }
        List<Weapon> AOEweapons = new List<Weapon>();
        List<Weapon> MFweapons = new List<Weapon>();
        List<AbstractActor> detectedTargets = ___unit.GetDetectedEnemyUnits();
        Log.M.WL(1, "DetectedEnemyUnits:" + detectedTargets.Count);
        if (detectedTargets.Count == 0) { return true; }
        AbstractActor nearestEnemy = null;
        float nearestDistance = 0f;
        foreach(AbstractActor target in detectedTargets) {
          float distance = Vector3.Distance(target.CurrentPosition, ___unit.CurrentPosition);
          if ((nearestDistance == 0f) || (nearestDistance > distance)) { nearestDistance = distance; nearestEnemy = target; }
        }
        if (nearestEnemy == null) { return true; }

        foreach (Weapon weapon in ___unit.Weapons) {
          if (weapon.IsFunctional == false) { continue; }
          if (weapon.IsJammed()) { continue; }
          if (weapon.IsCooldown() > 0) { continue; }
          if (weapon.NoModeToFire()) { continue; };
          if (weapon.isBlocked()) { continue; };
          if (weapon.isCantNormalFire()) { continue; };
          List<AmmoModePair> FiringMethods = weapon.getAvaibleFiringMethods();
          if (FiringMethods.Count == 0) { continue; }
          foreach (AmmoModePair ammoMode in FiringMethods) {
            weapon.ApplyAmmoMode(ammoMode);
            if (weapon.CanFire == false) { continue; }
            if (CustomAmmoCategories.IndirectFireCapable(weapon) == false) { continue; }
            if (weapon.MaxRange < nearestDistance) { continue; }
            if (weapon.AOECapable() && (weapon.AOERange() > CustomAmmoCategories.Epsilon)) {
              AOEweapons.Add(weapon);
              break;
            }
            if (weapon.InstallMineField()) {
              MFweapons.Add(weapon);
              break;
            }
          }
        }
        List<Weapon> weaponsList = new List<Weapon>();
        weaponsList.AddRange(AOEweapons);
        weaponsList.AddRange(MFweapons);
        Log.M.WL(1, "Capable weapons:"+weaponsList.Count);
        if (weaponsList.Count == 0) { return true; }
        AttackOrderInfo orderInfo = new AttackOrderInfo(___unit, false, false);
        foreach (Weapon weapon in weaponsList) {
          AmmoModePair ammoMode = weapon.getCurrentAmmoMode();
          Log.M.WL(2, weapon.defId+" mode:"+ ammoMode.modeId+" ammo:"+ammoMode.ammoId);
          orderInfo.AddWeapon(weapon);
        }
        //MechRepresentation gameRep = ___unit.GameRep as MechRepresentation;
        //if ((UnityEngine.Object)gameRep != (UnityEngine.Object)null) {
        //Log.M.WL(1,"ToggleRandomIdles false");
        //gameRep.ToggleRandomIdles(false);
        //}
        string actorGUID = ___unit.GUID;
        //int seqId = ___unit.Combat.StackManager.NextStackUID;
        Log.M.WL(1, "Registering terrain attack to " + actorGUID);
        CustomAmmoCategories.addTerrainHitPosition(___unit, nearestEnemy.CurrentPosition, true);
        //AttackDirector.AttackSequence attackSequence = ___unit.Combat.AttackDirector.CreateAttackSequence(seqId, ___unit, ___unit, ___unit.CurrentPosition, ___unit.CurrentRotation, 0, weaponsList, MeleeAttackType.NotSet, 0, false);
        //attackSequence.indirectFire = true;
        //Log.M.WL(1, "attackSequence.indirectFire " + attackSequence.indirectFire);
        //___unit.Combat.AttackDirector.PerformAttack(attackSequence);
        __result = new BehaviorTreeResults(BehaviorNodeState.Success);
        __result.orderInfo = orderInfo;
        __result.debugOrderString = "CAC improved brace";
        __result.behaviorTrace = "CAC improved brace";
        ___unit.BracedWithFireThisRound();
        //___unit.HasFiredThisRound = true;
        return false;
      }catch(Exception e) {
        Log.M.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }

}
