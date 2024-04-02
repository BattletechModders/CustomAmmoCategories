﻿/*  
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
using CustomAmmoCategoriesPatches;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CleverGirlAIDamagePrediction {
  public class DamagePredictionRecord {
    public float Normal { get; set; }
    public float AP { get; set; }
    public float Heat { get; set; }
    public float Instability { get; set; }
    public float HitsCount { get; set; }
    public bool isAoE { get; set; }
    //public float ClusterCoeff { get; set; }
    public List<int> PossibleHitLocations { get; set; }
    public List<EffectData> ApplyEffects { get; set; }
    public ICombatant Target { get; set; }
    public float ToHit { get; set; }
    public DamagePredictionRecord() {
      PossibleHitLocations = new List<int>();
      ApplyEffects = new List<EffectData>();
      isAoE = false;
    }
    public string ToString(int i = 0) {
      StringBuilder result = new StringBuilder();
      string it = new String(' ',i);
      result.Append(it + "Normal:" + Normal+"\n");
      result.Append(it + "AP:" + AP + "\n");
      result.Append(it + "Heat:" + Heat + "\n");
      result.Append(it + "Instability:" + Instability + "\n");
      result.Append(it + "HitsCount:" + HitsCount + "\n");
      result.Append(it + "isAoE:" + isAoE + "\n");
      result.Append(it + "PossibleHitLocations:");foreach (int l in PossibleHitLocations) { result.Append(l+" "); };result.Append("\n");
      result.Append(it + "ApplyEffects:"+ ApplyEffects.Count+"\n");
      result.Append(it + "Target:" + Target.DisplayName + ":"+Target.GUID+"\n");
      result.Append(it + "ToHit:" + ToHit + "\n\n");
      return result.ToString();
    }
  }
  public class WeaponFirePredictedEffect {
    public AmmunitionDef ammo { get; set; }
    public ExtAmmunitionDef exAmmo { get; set; }
    public Weapon weapon { get; set; }
    public WeaponMode mode { get; set; }
    public bool isAMS { get; set; }
    public bool isAAMS { get; set; }
    public int ammoUsage { get; set; }
    public int avaibleAmmo { get; set; }
    public float JammChance { get; set; }
    public int Cooldown { get; set; }
    public bool DamageOnJamm { get; set; }
    public bool DestroyOnJamm { get; set; }
    public List<DamagePredictionRecord> predictDamage { get; set; }
    public string ToString(int i = 0) {
      StringBuilder result = new StringBuilder();
      string it = new String(' ', i);
      result.Append(it + "ammo:" + ((ammo != null)?ammo.Description.Id:"null") + "\n");
      result.Append(it + "exAmmo:" + exAmmo.AmmoCategory.Id + "\n");
      result.Append(it + "weapon:" + weapon.Description.Id + "\n");
      result.Append(it + "mode:" + mode.Id + "\n");
      result.Append(it + "isAMS:" + isAMS + "\n");
      result.Append(it + "isAAMS:" + isAAMS + "\n");
      result.Append(it + "ammoUsage:" + ammoUsage + "\n");
      result.Append(it + "avaibleAmmo:" + avaibleAmmo + "\n");
      result.Append(it + "predictDamage:" + predictDamage.Count + "\n");
      foreach(DamagePredictionRecord dmg in predictDamage) {
        result.Append(dmg.ToString(i+1));
      }
      return result.ToString();
    }
    public WeaponFirePredictedEffect() {
      ammo = null;
      exAmmo = null;
      weapon = null;
      mode = null;
      predictDamage = new List<DamagePredictionRecord>();
    }
    public void NormalDamageProc(Vector3 attackPos, ICombatant target) {
      Log.Combat?.TWL(0, "WeaponFirePredictedEffect.NormalDamageProc "+this.weapon.defId+" trg:"+target.DisplayName);
      DamagePredictionRecord inital = new DamagePredictionRecord();
      inital.Target = target;
      bool damagePerPallet = weapon.DamagePerPallet();
      bool damagePerNotDiv = weapon.DamageNotDivided();
      inital.PossibleHitLocations = target.GetPossibleHitLocations(attackPos);
      inital.Normal = this.weapon.DamagePerShot;
      inital.Heat = this.weapon.HeatDamagePerShot;
      inital.Instability = this.weapon.Instability();
      inital.AP = this.weapon.StructureDamagePerShot;
      if ((damagePerPallet == true) && (damagePerNotDiv == false)) {
        inital.Normal /= (float)weapon.ProjectilesPerShot;
        inital.Heat /= (float)weapon.ProjectilesPerShot;
        inital.Instability /= (float)weapon.ProjectilesPerShot;
        inital.AP /= (float)weapon.ProjectilesPerShot;
      }
      inital.HitsCount = weapon.DecrementAmmo(-1);
      inital.ToHit = weapon.GetToHitFromPosition(target, 1, attackPos, target.CurrentPosition, false, false, false);
      EffectData[] effects = weapon.StatusEffects();
      foreach (EffectData statusEffect in effects) {
        if (statusEffect.targetingData.effectTriggerType == EffectTriggerType.OnHit) {
          inital.ApplyEffects.Add(statusEffect);
        }
      }
      Log.Combat?.WL(0, inital.ToString(1));
      this.predictDamage.Add(inital);
    }
    public void StrayProc(DamagePredictionRecord inital, Vector3 attackPos) {
      Log.Combat?.TWL(0, "WeaponFirePredictedEffect.NormalDamageProc " + this.weapon.defId + " trg:" + inital.Target.DisplayName);
      float SpreadRange = this.weapon.StrayRange();
      if (SpreadRange <= CustomAmmoCategories.Epsilon) { Log.Combat?.WL("No stray"); return; }
      float divider = SpreadRange;
      Dictionary<ICombatant, float> possibleTargets = new Dictionary<ICombatant, float>();
      foreach (ICombatant target in this.weapon.parent.Combat.GetAllCombatants()) {
        if (target.IsDead) { continue; }
        if (target.GUID == inital.Target.GUID) { continue; }
        float distance = SpreadRange - Vector3.Distance(target.CurrentPosition, inital.Target.CurrentPosition);
        if (distance <= 0f) { continue; }
        possibleTargets.Add(target, distance);
        divider += distance;
      }
      float mainHitsCount = inital.HitsCount;
      inital.HitsCount = mainHitsCount * (SpreadRange / divider);
      foreach (var trg in possibleTargets) {
        DamagePredictionRecord stray = new DamagePredictionRecord();
        stray.Target = trg.Key;
        stray.PossibleHitLocations = trg.Key.GetPossibleHitLocations(attackPos);
        stray.Normal = inital.Normal;
        stray.Heat = inital.Heat;
        stray.Instability = inital.Instability;
        stray.ToHit = weapon.GetToHitFromPosition(trg.Key, 1, attackPos, trg.Key.CurrentPosition, false, false, false);
        stray.AP = inital.AP;
        stray.HitsCount = mainHitsCount * (trg.Value / divider);
        stray.ApplyEffects.AddRange(inital.ApplyEffects);
        Log.M.WL(0, stray.ToString(1));
        this.predictDamage.Add(stray);
      }
    }
    public void StrayProc(Vector3 attackPos) {
      List<DamagePredictionRecord> rec = new List<DamagePredictionRecord>();
      rec.AddRange(this.predictDamage);
      foreach (DamagePredictionRecord dmg in rec) {
        this.StrayProc(dmg, attackPos);
      }
    }
    public void ShellsProc(Vector3 attackPos) {
      Log.Combat?.TWL(0, "WeaponFirePredictedEffect.ShellsProc " + this.weapon.defId + "/"+this.weapon.UIName);
      if (weapon.HasShells() == false) { Log.M.WL(0, "No shells"); return; }
      List<DamagePredictionRecord> rec = new List<DamagePredictionRecord>();
      rec.AddRange(this.predictDamage);
      foreach (DamagePredictionRecord dmg in rec) {
        this.ShellsProc(dmg, attackPos);
      }
    }
    public void AoEProc(Vector3 attackPos) {
      Log.Combat?.TWL(0, "WeaponFirePredictedEffect.AoEProc " + this.weapon.defId + "/" + this.weapon.UIName);
      if (this.weapon.AOECapable() == false) { return; }
      if (this.weapon.AOERange() <= CustomAmmoCategories.Epsilon) { return; }
      List<DamagePredictionRecord> rec = new List<DamagePredictionRecord>();
      rec.AddRange(this.predictDamage);
      foreach (DamagePredictionRecord dmg in rec) {
        this.AoEProc(dmg, attackPos);
      }
    }
    public void AoEProc(DamagePredictionRecord inital, Vector3 attackPos) {
      float AoERange = weapon.AOERange();
      Dictionary<ICombatant, float> possibleTargets = new Dictionary<ICombatant, float>();
      foreach (ICombatant target in this.weapon.parent.Combat.GetAllCombatants()) {
        if (target.IsDead) { continue; }
        float distance = AoERange - Vector3.Distance(target.CurrentPosition, inital.Target.CurrentPosition);
        if (distance <= 0f) { continue; }
        possibleTargets.Add(target, distance);
      }
      float AoEDmg = CustomAmmoCategories.AOEDamage(weapon) * inital.HitsCount;
      float AoEHeat = CustomAmmoCategories.AOEHeatDamage(weapon) * inital.HitsCount;
      float AoEStab = weapon.AOEInstability() * inital.HitsCount;
      foreach (var trg in possibleTargets) {
        DamagePredictionRecord aoe = new DamagePredictionRecord();
        aoe.PossibleHitLocations = trg.Key.GetPossibleHitLocations(inital.Target.TargetPosition);
        float aoeFalloff = trg.Value / AoERange;
        aoe.Target = trg.Key;
        aoe.Normal = AoEDmg * aoeFalloff;
        aoe.Heat = AoEHeat * aoeFalloff;
        aoe.Instability = AoEStab * aoeFalloff;
        aoe.ToHit = 1f;
        aoe.AP = 0f;
        aoe.HitsCount = 1f;
        aoe.ApplyEffects.AddRange(inital.ApplyEffects);
        aoe.isAoE = true;
        this.predictDamage.Add(aoe);
      }
    }
    public void ShellsProc(DamagePredictionRecord inital,Vector3 attackPos) {
      Log.Combat?.TWL(0, "WeaponFirePredictedEffect.ShellsProc " + this.weapon.defId + " trg:" + inital.Target.DisplayName);
      float sMin = this.weapon.MinShellsDistance();
      float sep_distance = Vector3.Distance(attackPos, inital.Target.CurrentPosition);
      bool FragSeparated = sep_distance >= sMin;
      if (FragSeparated) {
        inital.Normal /= (float)weapon.ProjectilesPerShot;
        inital.Heat /= (float)weapon.ProjectilesPerShot;
        inital.Instability /= (float)weapon.ProjectilesPerShot;
        inital.AP /= (float)weapon.ProjectilesPerShot;
        inital.HitsCount *= weapon.ProjectilesPerShot;
      } else {
        float unsepDmbMod = this.weapon.UnseparatedDamageMult();
        inital.Normal *= unsepDmbMod;
        inital.Heat *= unsepDmbMod;
        inital.Instability *= unsepDmbMod;
        inital.AP *= unsepDmbMod;
        return;
      }
      float SpreadRange = this.weapon.ShellsRadius();
      if (SpreadRange <= CustomAmmoCategories.Epsilon) { return; }
      float divider = SpreadRange;
      Dictionary<ICombatant, float> possibleTargets = new Dictionary<ICombatant, float>();
      foreach (ICombatant target in this.weapon.parent.Combat.GetAllCombatants()) {
        if (target.IsDead) { continue; }
        if (target.GUID == inital.Target.GUID) { continue; }
        float distance = SpreadRange - Vector3.Distance(target.CurrentPosition, inital.Target.CurrentPosition);
        if (distance <= 0f) { continue; }
        possibleTargets.Add(target, distance);
        divider += distance;
      }
      float mainHitsCount = inital.HitsCount;
      inital.HitsCount = mainHitsCount * (SpreadRange / divider);
      foreach (var trg in possibleTargets) {
        DamagePredictionRecord stray = new DamagePredictionRecord();
        stray.Target = trg.Key;
        stray.PossibleHitLocations = trg.Key.GetPossibleHitLocations(attackPos);
        stray.Normal = inital.Normal;
        stray.Heat = inital.Heat;
        stray.Instability = inital.Instability;
        stray.ToHit = weapon.GetToHitFromPosition(trg.Key, 1, attackPos, trg.Key.CurrentPosition, false, false, false);
        stray.AP = inital.AP;
        stray.HitsCount = mainHitsCount * (trg.Value / divider);
        stray.ApplyEffects.AddRange(inital.ApplyEffects);
        Log.M.WL(0, stray.ToString(1));
        this.predictDamage.Add(stray);
      }
    }
    public void DamageVarianceProc(Vector3 attackPos, DamagePredictionRecord inital) {
      if (inital.isAoE) { return; }
      DamageModifiers mods = weapon.GetDamageModifiers(attackPos, inital.Target);
      float dmg = inital.Normal;
      float ap = inital.AP;
      float heat = inital.Heat;
      float stability = inital.Instability;
      string descr = string.Empty;
      mods.Calculate(-1, ref dmg, ref ap, ref heat, ref stability, ref descr, false, true);
      if (weapon.DamagePerShot > CustomAmmoCategories.Epsilon) { inital.Normal = (dmg / this.weapon.DamagePerShot) * inital.Normal; }
      if (weapon.StructureDamagePerShot > CustomAmmoCategories.Epsilon) { inital.AP = (ap / this.weapon.StructureDamagePerShot) * inital.AP; }
      if (weapon.HeatDamagePerShot > CustomAmmoCategories.Epsilon) { inital.Heat = (heat / this.weapon.HeatDamagePerShot) * inital.Heat; }
      if (weapon.Instability() > CustomAmmoCategories.Epsilon) { inital.Instability = (stability / this.weapon.Instability()) * inital.Instability; }
    }
    public void DamageVarianceProc(Vector3 attackPos) {
      foreach (DamagePredictionRecord dmg in this.predictDamage) {
        this.DamageVarianceProc(attackPos, dmg);
      }
    }
  }
  public static class CleverGirlHelper {
    public static List<int> GetPossibleHitLocations(this ICombatant target, Vector3 attackPosition) {
      switch (target.UnitType) {
        case UnitType.Mech: return target.Combat.HitLocation.GetPossibleHitLocations(attackPosition, target as Mech);
        case UnitType.Vehicle: return target.Combat.HitLocation.GetPossibleHitLocations(attackPosition, target as Vehicle);
      }
      List<int> result = new List<int>();
      result.Add(1);
      return result;
    }
    public static AmmoModePair getCurrentAmmoMode(this Weapon weapon) {
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      string currentMode = extWeapon.baseModeId;
      Statistic stat = weapon.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName);
      if (stat != null) { currentMode = stat.Value<string>(); }
      stat = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName);
      string currentAmmo = string.Empty;
      if (stat != null) { currentAmmo = stat.Value<string>(); }
      return new AmmoModePair(currentAmmo, currentMode);
    }
    public static void ApplyAmmoMode(this Weapon weapon, AmmoModePair ammoMode) {
      CustomAmmoCategories.applyWeaponAmmoMode(weapon, ammoMode.modeId, ammoMode.ammoId);
    }
    public static List<AmmoModePair> getAvaibleFiringMethods(this Weapon weapon) {
      List<AmmoModePair> result = new List<AmmoModePair>();
      WeaponExtendedInfo info = weapon.info();
      List<WeaponMode> modes = weapon.AvaibleModes();
      if (info.modes.Count < 1) {
        return result;
      }
      foreach (WeaponMode mode in modes) {
        HashSet<string> ammos = CustomAmmoCategories.getWeaponAvaibleAmmoForMode(weapon, mode.Id);
        foreach (string ammoId in ammos) { result.Add(new AmmoModePair(ammoId, mode.Id)); }
      }
      return result;
    }
    public static Dictionary<AmmoModePair, WeaponFirePredictedEffect> gatherDamagePrediction(this Weapon weapon, Vector3 attackPos, ICombatant target) {
      Dictionary<AmmoModePair, WeaponFirePredictedEffect> result = new Dictionary<AmmoModePair, WeaponFirePredictedEffect>();
      if (weapon.parent.isSpawnProtected() || target.isSpawnProtected()) { return result; }
      AmmoModePair curAmmoMode = weapon.getCurrentAmmoMode();
      List<AmmoModePair> avaibleAmmoModes = weapon.getAvaibleFiringMethods();
      foreach (AmmoModePair ammoMode in avaibleAmmoModes) {
        weapon.ApplyAmmoMode(ammoMode);
        result.Add(ammoMode, weapon.CalcPredictedEffect(attackPos, target));
      }
      weapon.ApplyAmmoMode(curAmmoMode);
      Log.Combat?.TWL(0, "gatherDamagePrediction");
      foreach (var r in result) {
        Log.Combat?.WL(0, r.Key.ToString());
        Log.Combat?.WL(0, r.Value.ToString(1));
      }
      weapon.ResetTempAmmo();
      return result;
    }
    public static WeaponFirePredictedEffect CalcPredictedEffect(this Weapon weapon, Vector3 attackPos, ICombatant target) {
      WeaponFirePredictedEffect result = new WeaponFirePredictedEffect();
      try {
        result.weapon = weapon;
        result.exAmmo = weapon.ammo();
        if (result.exAmmo.AmmoCategory.BaseCategory.Is_NotSet == false) {
          result.ammo = weapon.parent.Combat.DataManager.AmmoDefs.Get(result.exAmmo.Id);
          result.avaibleAmmo = weapon.CurrentAmmo;
        } else {
          result.avaibleAmmo = -1;
        }
        result.mode = weapon.mode();
        result.isAMS = weapon.isAMS();
        result.isAAMS = weapon.isAAMS();
        result.ammoUsage = weapon.ShotsWhenFired;
        result.JammChance = weapon.FlatJammingChance(out string jdescr);
        result.DamageOnJamm = weapon.DamageOnJamming();
        result.DestroyOnJamm = weapon.DestroyOnJamming();
        result.Cooldown = weapon.Cooldown();
        result.NormalDamageProc(attackPos, target);
        result.StrayProc(attackPos);
        result.ShellsProc(attackPos);
        result.DamageVarianceProc(attackPos);
        result.AoEProc(attackPos);
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        AttackDirector.damageLogger.LogException(e);
      }
      return result;
    }
  }
}