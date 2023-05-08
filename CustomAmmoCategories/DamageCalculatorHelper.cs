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
using BattleTech.UI;
using CustomAmmoCategoriesLog;
using CustomAmmoCategoriesPatches;
using Localize;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CustAmmoCategories {
  public enum DamageModifierType { Normal,AP,Heat,Stability }
  public class DamageModifier {
    public string Name;
    public DamageModifierType DmgType;
    public bool ShowInUI;
    public float Modifier;
    public bool Recalculate;
    public bool isDirectValue;
    public Func<Weapon, Vector3, ICombatant, bool, int, float, float, float, float, float> calc;
    public DamageModifier(string name, DamageModifierType type, bool uiShow, bool isDirect, float val, Func<Weapon, Vector3, ICombatant, bool, int, float, float, float, float, float> calc) {
      this.Name = name;
      this.DmgType = type;
      this.ShowInUI = uiShow;
      this.isDirectValue = isDirect;
      this.Modifier = val;
      this.calc = calc;
      this.Recalculate = calc != null;
    }
  }
  public class DamageModifiers {
    public List<DamageModifier> modifiers;
    public float Damage;
    public float AP;
    public float Heat;
    public float Stability;
    public string Description;
    public bool IsBreachingShot;
    public Weapon weapon;
    public Vector3 attackPos;
    public ICombatant target;
    public bool isCalculated;
    public float simpleVariationRoll;
    public static float JumpingWeaponDamageModifier(Weapon weapon, Vector3 attackPosition, ICombatant target, bool IsBreachingShot, int location, float dmg, float ap, float heat, float stab) {
      float jumpDmgMod = 1f;
      if (weapon.parent.UnitType == UnitType.Mech && weapon.parent.HasJumpedThisRound) {
        jumpDmgMod = weapon.StatCollection.GetValue<float>("JumpingWeaponDamageModifier");
      }
      return jumpDmgMod;
    }
    public static float armorDamageModifier(Weapon weapon, Vector3 attackPosition, ICombatant target, bool IsBreachingShot, int location, float dmg, float ap, float heat, float stab) {
      if (location <= 0) { return 1f; }
      if (location >= 65535) { return 1f; }
      if (dmg < CustomAmmoCategories.Epsilon) { return weapon.ArmorDmgMult(); };
      float restLocArmor = target.ArmorForLocation(location) - target.GetComulativeDamage(location);
      if (restLocArmor < 0f) { return weapon.ISDmgMult(); }
      float fullDamage = dmg * weapon.ArmorDmgMult();
      if (restLocArmor >= fullDamage) { return weapon.ArmorDmgMult(); }
      if (fullDamage < CustomAmmoCategories.Epsilon) { return weapon.ArmorDmgMult(); };
      float ISPart = ((fullDamage - restLocArmor) / fullDamage) * dmg;
      fullDamage = restLocArmor + ISPart * weapon.ISDmgMult();
      return fullDamage / dmg;
    }
    public float SimpleVariation(Weapon weapon, Vector3 attackPosition, ICombatant target, bool IsBreachingShot, int location, float dmg, float ap, float heat, float stab) {
      Log.Combat?.TWL(0,"Simple damage variance for weapon " + weapon.UIName + "\n");
      var damagePerShot = weapon.DamagePerShot;
      var variance = weapon.DamageVariance();
      if (float.IsNaN(simpleVariationRoll)) {
        simpleVariationRoll = NormalDistribution.Random(
          new VarianceBounds(
              damagePerShot - variance,
              damagePerShot + variance,
              CustomAmmoCategories.getWRSettings().StandardDeviationSimpleVarianceMultiplier * variance
          ));
      }
      var variantDamage = simpleVariationRoll / damagePerShot;

      var sb = new StringBuilder();
      sb.AppendLine($" roll: {simpleVariationRoll}");
      sb.AppendLine($" damagePerShot: {damagePerShot}");
      sb.AppendLine($" variance: {variance}");
      sb.AppendLine($" result: {variantDamage}");
      Log.Combat?.WL(0,sb.ToString());
      return variantDamage;
    }
    public static float DamageReductionMultiplierAll(Weapon weapon, Vector3 attackPosition, ICombatant target, bool IsBreachingShot, int location, float dmg, float ap, float heat, float stab) {
      return target.StatCollection.GetValue<float>("DamageReductionMultiplierAll") * target.DamageReductionMultiplierAll(location);
    }
    public static float DamageReductionMultiplierType(Weapon weapon, Vector3 attackPosition, ICombatant target, bool IsBreachingShot, int location, float dmg, float ap, float heat, float stab) {
      float typeDmgResistanceMod = 1f;
      if (!string.IsNullOrEmpty(weapon.WeaponCategoryValue.DamageReductionMultiplierStat))
        typeDmgResistanceMod = target.StatCollection.GetValue<float>(weapon.WeaponCategoryValue.DamageReductionMultiplierStat);
      return typeDmgResistanceMod * target.DamageReductionMultiplier(weapon.WeaponCategoryValue.DamageReductionMultiplierStat, location);
    }
    public static float iHeatMod(Weapon weapon, Vector3 attackPosition, ICombatant target, bool IsBreachingShot, int location, float dmg, float ap, float heat, float stab) {
      return target.IncomingHeatMult();
    }
    public static float iStabMod(Weapon weapon, Vector3 attackPosition, ICombatant target, bool IsBreachingShot, int location, float dmg, float ap, float heat, float stab) {
      float result = target.IncomingStabilityMult();
      AbstractActor actorTarget = target as AbstractActor;
      if (actorTarget != null) {
        result *= (actorTarget.StatCollection.GetValue<float>("ReceivedInstabilityMultiplier") * actorTarget.EntrenchedMultiplier);
      }
      return result;
    }
    public static float iAPMod(Weapon weapon, Vector3 attackPosition, ICombatant target, bool IsBreachingShot, int location, float dmg, float ap, float heat, float stab) {
      return target.APDamageMult() * target.APDamageMult(location);
    }
    public static float iAPImmune(Weapon weapon, Vector3 attackPosition, ICombatant target, bool IsBreachingShot, int location, float dmg, float ap, float heat, float stab) {
      return (target.isAPProtected() || target.isAPProtected(location)) ? 0f : 1f;
    }
    public static float HeatToNormal(Weapon weapon, Vector3 attackPosition, ICombatant target, bool IsBreachingShot, int location, float dmg, float ap, float heat, float stab) {
      return target.isHasHeat()?0f:heat;
    }
    public static float SanitizeDamage(float dmg) {
      if (float.IsNaN(dmg)) { return 0f; };
      if (float.IsInfinity(dmg)) { return 0f; };
      if (dmg < 0f) { return 0f; }
      return dmg;
    }
    public void Calculate(int location, ref float damage,ref float ap, ref float heat, ref float stability, ref string descr, bool force, bool ui) {
      if (force) { isCalculated = false; }
      if (isCalculated) {
        damage = this.Damage;
        ap = this.AP;
        heat = this.Heat;
        stability = this.Stability;
        descr = this.Description;
        return;
      }
      StringBuilder ds = new StringBuilder();
      StringBuilder dds = new StringBuilder();
      StringBuilder ads = new StringBuilder();
      StringBuilder hds = new StringBuilder();
      StringBuilder sds = new StringBuilder();
      ds.AppendLine("Base damage (Normal/AP/Heat/Instability)".UI());
      ds.AppendLine(" " + damage+"/"+ap+"/"+heat+"/"+stability);
      if (ui == false) { ds.AppendLine("target: " + this.target.DisplayName); };
      foreach (DamageModifier mod in modifiers) {
        StringBuilder actds = null;
        switch (mod.DmgType) {
          case DamageModifierType.Normal: actds = dds; break;
          case DamageModifierType.AP: actds = ads; break;
          case DamageModifierType.Heat: actds = hds; break;
          case DamageModifierType.Stability: actds = sds; break;
        }
        if (actds == null) { continue; }
        if ((mod.ShowInUI == false) && (ui == true)) {
          if (mod.Recalculate) { actds.Append("<color=yellow>"); } else { actds.Append("<color=green>"); }
          if (mod.isDirectValue) { actds.Append(" " + mod.Name + ": +??"); } else { actds.Append(" " + mod.Name + ": x??"); };
          actds.AppendLine("</color>");
          continue;
        }
        if (mod.Recalculate) { mod.Modifier = mod.calc(this.weapon, this.attackPos, this.target, this.IsBreachingShot, location, damage, ap, heat, stability); }
        if (float.IsNaN(mod.Modifier)) { continue; }
        if (ui) {
          if (mod.isDirectValue && (mod.Modifier < CustomAmmoCategories.Epsilon)) { continue; };
          if ((mod.isDirectValue == false) && (Mathf.Abs(mod.Modifier - 1f) < CustomAmmoCategories.Epsilon)) { continue; };
        }
        ref float actval = ref damage;
        switch (mod.DmgType) {
          case DamageModifierType.Normal: actval = ref damage; break;
          case DamageModifierType.AP: actval = ref ap; break;
          case DamageModifierType.Heat: actval = ref heat; break;
          case DamageModifierType.Stability: actval = ref stability; break;
        }
        if (ui) if (mod.Recalculate) { actds.Append("<color=yellow>"); } else { actds.Append("<color=green>"); }
        if (mod.isDirectValue) { actval += mod.Modifier; actds.Append(" " + mod.Name + ": " + (mod.Modifier >= 0f ? "+" : "-") + mod.Modifier); } else { actval *= mod.Modifier; actds.Append(" " + mod.Name + ": x" + Math.Round(mod.Modifier, 2)); };
        if (ui) { actds.AppendLine("</color>"); } else { actds.AppendLine(); };
      }
      isCalculated = true;
      damage = SanitizeDamage(damage);
      heat = SanitizeDamage(heat);
      stability = SanitizeDamage(stability);
      ap = SanitizeDamage(ap);
      this.Damage = damage;
      this.AP = ap;
      this.Heat = heat;
      this.Stability = stability;
      ds.AppendLine("Damage modifiers");
      ds.Append(dds.ToString());
      ds.AppendLine("AP modifiers");
      ds.Append(ads.ToString());
      ds.AppendLine("Heat modifiers");
      ds.Append(hds.ToString());
      ds.AppendLine("Instability modifiers");
      ds.Append(sds.ToString());
      ds.AppendLine("Resulting damage (Normal/AP/Heat/Instability)".UI());
      ds.AppendLine(" " + Math.Round(damage,1) + "/" + Math.Round(ap, 1) + "/" + Math.Round(heat, 1) + "/" + Math.Round(stability, 1) + "\n");
      descr = ds.ToString();
      this.Description = ds.ToString();
    }
    public DamageModifiers(Weapon weapon, Vector3 attackPosition, ICombatant target, bool _IsBreachingShot) {
      this.IsBreachingShot = _IsBreachingShot;
      this.weapon = weapon;
      this.attackPos = attackPosition;
      this.target = target;
      isCalculated = false;
      modifiers = new List<DamageModifier>();
      float distMod = 1f;
      if (weapon.DistantVariance() > CustomAmmoCategories.Epsilon) {
        distMod = weapon.DistantVarianceReversed() ? weapon.RevDistanceDamageMod(attackPosition, target) : weapon.DistanceDamageMod(attackPosition, target);
      }
      float overHeatMod = weapon.TargetOverheatModifier(target);
      if (weapon.BreachingShot()) { this.IsBreachingShot = true; }
      AttackImpactQuality attackImpactQuality = weapon.IgnoreCover() ? AttackImpactQuality.Solid
        : weapon.parent.Combat.ToHit.GetBlowQuality(weapon.parent, attackPosition, weapon, target, weapon.WeaponCategoryValue.IsMelee?MeleeAttackType.MeleeWeapon:MeleeAttackType.NotSet, this.IsBreachingShot);
      float qualityMultiplier = weapon.parent.Combat.ToHit.GetBlowQualityMultiplier(attackImpactQuality);
      modifiers.Add(new DamageModifier("Distance".UI(), DamageModifierType.Normal, true, false, (weapon.isDamageVariation() ? distMod : 1f), null));
      modifiers.Add(new DamageModifier("Distance".UI(), DamageModifierType.AP, true, false, (weapon.isDamageVariation() ? distMod : 1f), null));
      modifiers.Add(new DamageModifier("Distance".UI(), DamageModifierType.Heat, true, false, (weapon.isHeatVariation() ? distMod : 1f), null));
      modifiers.Add(new DamageModifier("Distance".UI(), DamageModifierType.Stability, true, false, (weapon.isStabilityVariation() ? distMod : 1f), null));
      if (weapon.DamageVariance() > CustomAmmoCategories.Epsilon) {
        simpleVariationRoll = float.NaN;
        if (weapon.isDamageVariation()) {
          modifiers.Add(new DamageModifier("Variation".UI() + " +-" + weapon.DamageVariance(), DamageModifierType.Normal, false, false, float.NaN, SimpleVariation));
          modifiers.Add(new DamageModifier("Variation".UI() + " +-" + weapon.DamageVariance(), DamageModifierType.AP, false, false, float.NaN, SimpleVariation));
        }
        if (weapon.isHeatVariation()) modifiers.Add(new DamageModifier("Variation".UI() + " +-" + weapon.DamageVariance(), DamageModifierType.Heat, false, false, float.NaN, SimpleVariation));
        if (weapon.isStabilityVariation()) modifiers.Add(new DamageModifier("Variation".UI() + " +-" + weapon.DamageVariance(), DamageModifierType.Stability, false, false, float.NaN, SimpleVariation));
      }
      foreach(var eMode in DamageModifiersCache.eModesNameDelegates) {
        string name = eMode.Value(weapon);
        if (string.IsNullOrEmpty(name)) { continue; }
        if(DamageModifiersCache.eModesDamageDelegates.TryGetValue(eMode.Key,out Func<Weapon,float> funcD)) {
          if (funcD != null) {
            modifiers.Add(new DamageModifier(name, DamageModifierType.Normal, true, false, funcD(weapon), null));
          }
        }
        if (DamageModifiersCache.eModesAPDelegates.TryGetValue(eMode.Key, out Func<Weapon, float> funcAP)) {
          if (funcAP != null) {
            modifiers.Add(new DamageModifier(name, DamageModifierType.AP, true, false, funcAP(weapon), null));
          }
        }
        if (DamageModifiersCache.eModesHeatDelegates.TryGetValue(eMode.Key, out Func<Weapon, float> funcH)) {
          if (funcH != null) {
            modifiers.Add(new DamageModifier(name, DamageModifierType.Heat, true, false, funcH(weapon), null));
          }
        }
        if (DamageModifiersCache.eModesStabilityDelegates.TryGetValue(eMode.Key, out Func<Weapon, float> funcS)) {
          if (funcS != null) {
            modifiers.Add(new DamageModifier(name, DamageModifierType.Normal, true, false, funcS(weapon), null));
          }
        }
      }      
      modifiers.Add(new DamageModifier("Overheat".UI(), DamageModifierType.Normal, true, false, (weapon.isDamageVariation() ? overHeatMod : 1f), null));
      modifiers.Add(new DamageModifier("Overheat".UI(), DamageModifierType.AP, true, false, (weapon.isDamageVariation() ? overHeatMod : 1f), null));
      modifiers.Add(new DamageModifier("Overheat".UI(), DamageModifierType.Heat, true, false, (weapon.isHeatVariation() ? overHeatMod : 1f), null));
      modifiers.Add(new DamageModifier("Overheat".UI(), DamageModifierType.Stability, true, false, (weapon.isStabilityVariation() ? overHeatMod : 1f), null));
      modifiers.Add(new DamageModifier("Blow quality".UI(), DamageModifierType.Normal, true, false, qualityMultiplier, null));
      modifiers.Add(new DamageModifier("Blow quality".UI(), DamageModifierType.AP, true, false, qualityMultiplier, null));
      modifiers.Add(new DamageModifier("Blow quality".UI(), DamageModifierType.Heat, true, false, 1f, null));
      float globalDmgMod = weapon.parent.Combat.Constants.CombatValueMultipliers.GlobalDamageMultiplier;
      modifiers.Add(new DamageModifier("Has jumped".UI(), DamageModifierType.Normal, true, false, float.NaN, DamageModifiers.JumpingWeaponDamageModifier));
      modifiers.Add(new DamageModifier("Global settings".UI(), DamageModifierType.Normal, true, false, globalDmgMod, null));
      modifiers.Add(new DamageModifier("Global settings".UI(), DamageModifierType.AP, true, false, globalDmgMod, null));
      DesignMaskDef priorityDesignMaskAtPos = weapon.parent.Combat.MapMetaData.GetPriorityDesignMaskAtPos(attackPosition);
      float dmgFromMod = weapon.GetMaskDamageMultiplier(priorityDesignMaskAtPos);
      float dmgBiomeMod = weapon.GetMaskDamageMultiplier(weapon.parent.Combat.MapMetaData.biomeDesignMask);
      if (weapon.parent.Combat.MapMetaData.biomeDesignMask != null) {
        modifiers.Add(new DamageModifier("Biome".UI() + " " + new Text(weapon.parent.Combat.MapMetaData.biomeDesignMask.Description.Name).ToString(), DamageModifierType.Normal, true, false, dmgBiomeMod, null));
        modifiers.Add(new DamageModifier("Biome".UI() + " " + new Text(weapon.parent.Combat.MapMetaData.biomeDesignMask.Description.Name).ToString(), DamageModifierType.AP, true, false, dmgBiomeMod, null));
      }
      if (weapon.parent.UnaffectedDesignMasks()) { priorityDesignMaskAtPos = null; }
      if (priorityDesignMaskAtPos != null) {
        modifiers.Add(new DamageModifier("From".UI() + " " + new Text(priorityDesignMaskAtPos.Description.Name).ToString(), DamageModifierType.Normal, true, false, dmgFromMod, null));
        modifiers.Add(new DamageModifier("From".UI() + " " + new Text(priorityDesignMaskAtPos.Description.Name).ToString(), DamageModifierType.AP, true, false, dmgFromMod, null));
      }

      priorityDesignMaskAtPos = weapon.parent.Combat.MapMetaData.GetPriorityDesignMaskAtPos(target.CurrentPosition);
      if (target.UnaffectedDesignMasks()) { priorityDesignMaskAtPos = null; };
      if ((priorityDesignMaskAtPos != null) && ((target as AbstractActor) != null)) {
        float dmgToMod = weapon.GetMaskTakenDamageMultiplier(priorityDesignMaskAtPos);
        modifiers.Add(new DamageModifier("To".UI() + " " + new Text(priorityDesignMaskAtPos.Description.Name).ToString(), DamageModifierType.Normal, true, false, dmgToMod, null));
        modifiers.Add(new DamageModifier("To".UI() + " " + new Text(priorityDesignMaskAtPos.Description.Name).ToString(), DamageModifierType.AP, true, false, dmgToMod, null));
      }

      AbstractActor actor = target as AbstractActor;
      if (actor != null) {
        modifiers.Add(new DamageModifier("Target resistance".UI(), DamageModifierType.Normal, true, false, float.NaN, DamageReductionMultiplierAll));
        modifiers.Add(new DamageModifier("Target resistance".UI(), DamageModifierType.AP, true, false, float.NaN, DamageReductionMultiplierAll));

        modifiers.Add(new DamageModifier("Target resistance".UI() + " " + new Text(weapon.WeaponCategoryValue.FriendlyName).ToString(), DamageModifierType.Normal, true, false, float.NaN, DamageReductionMultiplierType));
        modifiers.Add(new DamageModifier("Target resistance".UI() + " " + new Text(weapon.WeaponCategoryValue.FriendlyName).ToString(), DamageModifierType.AP, true, false, float.NaN, DamageReductionMultiplierType));
      }

      LineOfFireLevel lineOfFireLevel = weapon.parent.Combat.LOS.GetLineOfFire(weapon.parent, attackPosition, target, target.CurrentPosition, target.CurrentRotation, out Vector3 collisionPos);
      float lofMod = 1f;
      switch (lineOfFireLevel) {
        case LineOfFireLevel.LOFBlocked:
        lofMod = weapon.parent.Combat.Constants.ToHit.DamageResistanceIndirectFire;
        break;
        case LineOfFireLevel.LOFObstructed:
        lofMod = weapon.parent.Combat.Constants.ToHit.DamageResistanceObstructed;
        break;
      }
      modifiers.Add(new DamageModifier("Line Of fire".UI(), DamageModifierType.Normal, true, false, lofMod, null));
      modifiers.Add(new DamageModifier("Line Of fire".UI(), DamageModifierType.AP, true, false, lofMod, null));

      modifiers.Add(new DamageModifier("Heat resistance".UI(), DamageModifierType.Heat, true, false, float.NaN, iHeatMod));
      modifiers.Add(new DamageModifier("Heat scale".UI(), DamageModifierType.Heat, true, false, target.ScaleIncomingHeat(), null));
      modifiers.Add(new DamageModifier("Instability resistance".UI(), DamageModifierType.Stability, true, false, float.NaN, iStabMod));
      modifiers.Add(new DamageModifier("AP resistance".UI(), DamageModifierType.AP, true, false, float.NaN, iAPMod));
      modifiers.Add(new DamageModifier("AP immune".UI(), DamageModifierType.AP, true, false, float.NaN, iAPImmune));
      modifiers.Add(new DamageModifier("Instability immune".UI(), DamageModifierType.Stability, true, false, target.isHasStability() ? 1f : 0f, null));

      if (target.UnitType == UnitType.Turret) { 
        modifiers.Add(new DamageModifier("Target type".UI(), DamageModifierType.Normal, true, false, weapon.WeaponCategoryValue.TurretDamageMultiplier, null));
        modifiers.Add(new DamageModifier("Target type".UI(), DamageModifierType.AP, true, false, weapon.WeaponCategoryValue.TurretDamageMultiplier, null));
      }else
      if ((target.UnitType == UnitType.Vehicle)||(target.FakeVehicle())) {
        modifiers.Add(new DamageModifier("Target type".UI(), DamageModifierType.Normal, true, false, weapon.WeaponCategoryValue.VehicleDamageMultiplier, null));
        modifiers.Add(new DamageModifier("Target type".UI(), DamageModifierType.AP, true, false, weapon.WeaponCategoryValue.VehicleDamageMultiplier, null));
      }
      if(target is BattleTech.Building) {
        if (WeaponRealizer.Core.ModSettings.HeatDamageAppliesToBuildingAsNormalDamage) {
          modifiers.Add(new DamageModifier("Building".UI(), DamageModifierType.Heat, true, false, WeaponRealizer.Core.ModSettings.HeatDamageApplicationToBuildingMultiplier, null));
        }
      }else
      if((target is Vehicle) || target.FakeVehicle()) {
        if (WeaponRealizer.Core.ModSettings.HeatDamageAppliesToVehicleAsNormalDamage) {
          modifiers.Add(new DamageModifier("Vehicle".UI(), DamageModifierType.Heat, true, false, WeaponRealizer.Core.ModSettings.HeatDamageApplicationToVehicleMultiplier, null));
        }
      } else
      if (target is Turret) {
        if (WeaponRealizer.Core.ModSettings.HeatDamageAppliesToTurretAsNormalDamage) {
          modifiers.Add(new DamageModifier("Turret".UI(), DamageModifierType.Heat, true, false, WeaponRealizer.Core.ModSettings.HeatDamageApplicationToTurretMultiplier, null));
        }
      }

      modifiers.Add(new DamageModifier("IS damage".UI(), DamageModifierType.AP, true, false, weapon.ISDmgMult(), null));
      modifiers.Add(new DamageModifier("Armor damage".UI()+"(x"+Math.Round(weapon.ArmorDmgMult())+")", DamageModifierType.Normal, false, false, float.NaN, armorDamageModifier));

      foreach(var dmgMod in DamageModifiersCache.damageModifiers) {
        string name = string.Empty;
        if (dmgMod.Value.modname != null) { name = dmgMod.Value.modname(weapon,attackPosition,target,this.IsBreachingShot,(int)ArmorLocation.None,0f,0f,0f,0f); }
        if (string.IsNullOrEmpty(name)) { name = dmgMod.Value.modnameStatic; }
        if (string.IsNullOrEmpty(name)) { name = dmgMod.Key; }
        if (dmgMod.Value.isStatic) {
          if (dmgMod.Value.isNormal) {
            modifiers.Add(new DamageModifier(name.UI(), DamageModifierType.Normal, true, false, dmgMod.Value.modifier(weapon, attackPosition, target, IsBreachingShot, (int)ArmorLocation.None, 0f, 0f, 0f, 0f), null));
          }
          if (dmgMod.Value.isAP) {
            modifiers.Add(new DamageModifier(name.UI(), DamageModifierType.AP, true, false, dmgMod.Value.modifier(weapon, attackPosition, target, IsBreachingShot, (int)ArmorLocation.None, 0f, 0f, 0f, 0f), null));
          }
          if (dmgMod.Value.isStability) {
            modifiers.Add(new DamageModifier(name.UI(), DamageModifierType.Stability, true, false, dmgMod.Value.modifier(weapon, attackPosition, target, IsBreachingShot, (int)ArmorLocation.None, 0f, 0f, 0f, 0f), null));
          }
          if (dmgMod.Value.isHeat) {
            modifiers.Add(new DamageModifier(name.UI(), DamageModifierType.Heat, true, false, dmgMod.Value.modifier(weapon, attackPosition, target, IsBreachingShot, (int)ArmorLocation.None, 0f, 0f, 0f, 0f), null));
          }
        } else {
          if (dmgMod.Value.isNormal) {
            modifiers.Add(new DamageModifier(name.UI(), DamageModifierType.Normal, true, false, float.NaN, dmgMod.Value.modifier));
          }
          if (dmgMod.Value.isAP) {
            modifiers.Add(new DamageModifier(name.UI(), DamageModifierType.AP, true, false, float.NaN, dmgMod.Value.modifier));
          }
          if (dmgMod.Value.isStability) {
            modifiers.Add(new DamageModifier(name.UI(), DamageModifierType.Stability, true, false, float.NaN, dmgMod.Value.modifier));
          }
          if (dmgMod.Value.isHeat) {
            modifiers.Add(new DamageModifier(name.UI(), DamageModifierType.Heat, true, false, float.NaN, dmgMod.Value.modifier));
          }
        }
      }
      modifiers.Add(new DamageModifier("Heat to damage".UI(), DamageModifierType.Normal, true, true, float.NaN, HeatToNormal));
      modifiers.Add(new DamageModifier("Heat to damage".UI(), DamageModifierType.Heat, true, false, target.isHasHeat()?1f:0f, null));
    }
  }
  public class DamageModifierDelegate {
    public string id { get; private set; }
    public Func<Weapon, Vector3, ICombatant, bool, int, float, float, float, float, float> modifier { get; private set; }
    public Func<Weapon, Vector3, ICombatant, bool, int, float, float, float, float, string> modname { get; private set; }
    public string modnameStatic { get; private set; }
    public bool isStatic;
    public bool isNormal;
    public bool isAP;
    public bool isHeat;
    public bool isStability;
    public DamageModifierDelegate(string id, string staticName, bool isStatic, bool isNormal, bool isAP, bool isHeat, bool isStability, Func<Weapon, Vector3, ICombatant, bool, int, float, float, float, float, float> modifier, Func<Weapon, Vector3, ICombatant, bool, int, float, float, float, float, string> modname) {
      this.id = id;
      this.modnameStatic = staticName;
      this.isStatic = isStatic;
      this.isNormal = isNormal;
      this.isAP = isAP;
      this.isHeat = isHeat;
      this.isStability = isStability;
      this.modifier = modifier;
      this.modname = modname;
    }
  }
  public static class DamageModifiersCache {
    private static Dictionary<ICombatant, Dictionary<int, float>> comulativeDamageCache = new Dictionary<ICombatant, Dictionary<int, float>>();
    public static Dictionary<string, Func<Weapon, string>> eModesNameDelegates = new Dictionary<string, Func<Weapon, string>>();
    public static Dictionary<string, Func<Weapon, float>> eModesDamageDelegates = new Dictionary<string, Func<Weapon, float>>();
    public static Dictionary<string, Func<Weapon, float>> eModesAPDelegates = new Dictionary<string, Func<Weapon, float>>();
    public static Dictionary<string, Func<Weapon, float>> eModesHeatDelegates = new Dictionary<string, Func<Weapon, float>>();
    public static Dictionary<string, Func<Weapon, float>> eModesStabilityDelegates = new Dictionary<string, Func<Weapon, float>>();
    public static Dictionary<string, DamageModifierDelegate> damageModifiers = new Dictionary<string, DamageModifierDelegate>();
    private static int CacheTurn = 0;
    private static Dictionary<Weapon, Dictionary<bool, Dictionary<ExtAmmunitionDef, Dictionary<WeaponMode, Dictionary<Vector3, Dictionary<ICombatant, DamageModifiers>>>>>> modifiersCache = new Dictionary<Weapon, Dictionary<bool, Dictionary<ExtAmmunitionDef, Dictionary<WeaponMode, Dictionary<Vector3, Dictionary<ICombatant, DamageModifiers>>>>>>();
    private static CombatHUD HUD = null;
    public static void Init(CombatHUD HUD) {
      DamageModifiersCache.HUD = HUD;
    }
    public static void RegisterDamageModifier(string id, string staticName, bool isStatic, bool isNormal, bool isAP, bool isHeat, bool isStability, Func<Weapon, Vector3, ICombatant, bool, int, float, float, float, float, float> modifier, Func<Weapon, Vector3, ICombatant, bool, int, float, float, float, float, string> modname) {
      if (damageModifiers.ContainsKey(id)) {
        damageModifiers[id] = new DamageModifierDelegate(id,staticName,isStatic,isNormal,isAP,isHeat,isStability,modifier,modname);
      } else {
        damageModifiers.Add(id,new DamageModifierDelegate(id, staticName, isStatic, isNormal, isAP, isHeat, isStability, modifier, modname));
      }
    }
    public static void Clear() {
      modifiersCache.Clear();
      comulativeDamageCache.Clear();
    }
    public static void ClearDamageCache(this Weapon weapon) {
      if (modifiersCache.ContainsKey(weapon)) { modifiersCache.Remove(weapon); }
    }
    public static void RegisterExternalModes(string id, Func<Weapon, string> nameDelegate,Func<Weapon,float> damageDelegate, Func<Weapon, float> apDelegate, Func<Weapon, float> heatDelegate, Func<Weapon, float> stabilityDelegate) {
      if (eModesNameDelegates.ContainsKey(id)) { eModesNameDelegates[id] = nameDelegate; } else { eModesNameDelegates.Add(id, nameDelegate); };
      if (eModesDamageDelegates.ContainsKey(id)) { eModesDamageDelegates[id] = damageDelegate; } else { eModesDamageDelegates.Add(id, damageDelegate); };
      if (eModesAPDelegates.ContainsKey(id)) { eModesAPDelegates[id] = apDelegate; } else { eModesAPDelegates.Add(id, apDelegate); };
      if (eModesHeatDelegates.ContainsKey(id)) { eModesHeatDelegates[id] = heatDelegate; } else { eModesHeatDelegates.Add(id, heatDelegate); };
      if (eModesStabilityDelegates.ContainsKey(id)) { eModesStabilityDelegates[id] = stabilityDelegate; } else { eModesStabilityDelegates.Add(id, stabilityDelegate); };
    }
    public static void ClearComulativeDamage() {
      comulativeDamageCache.Clear();
    }
    public static float GetComulativeDamage(this ICombatant target, int location) {
      if(comulativeDamageCache.TryGetValue(target,out Dictionary<int, float> targetDamage) == false) {
        return 0f;
      }
      if(targetDamage.TryGetValue(location, out float result) == false) {
        return 0f;
      }
      return result;
    }
    public static void AddComulativeDamage(this ICombatant target, int location, float damage) {
      if (comulativeDamageCache.TryGetValue(target, out Dictionary<int, float> targetDamage) == false) {
        targetDamage = new Dictionary<int, float>();
        comulativeDamageCache.Add(target, targetDamage);
      }
      if (targetDamage.ContainsKey(location) == false) {
        targetDamage.Add(location,damage);
        return;
      }
      targetDamage[location] += damage;
    }
    public static float GetMaskTakenDamageMultiplier(this Weapon weapon, DesignMaskDef designMask) {
      if (designMask == null) { return 1f; }
      float b = designMask.allDamageTakenMultiplier;
      if (!string.IsNullOrEmpty(weapon.WeaponCategoryValue.DesignMaskString)) {
        string designMaskString = weapon.WeaponCategoryValue.DesignMaskString;
        if (!(designMaskString == "Support")) {
          if (!(designMaskString == "Ballistic")) {
            if (!(designMaskString == "Energy")) {
              if (!(designMaskString == "Melee")) {
                if (designMaskString == "Missile") {
                  b *= designMask.missileDamageTakenMultiplier;
                }
              }
            } else {
              b *= designMask.energyDamageTakenMultiplier;
            }
          } else {
            b *= designMask.ballisticDamageTakenMultiplier;
          }
        } else {
          b *= designMask.antipersonnelDamageTakenMultiplier;
        }
      }
      return b;
    }
    public static DamageModifiers GetDamageModifiers(this Weapon weapon, Vector3 attackPosition, ICombatant target, TripleBoolean breachingShot = TripleBoolean.NotSet) {
      if(weapon.parent.Combat.TurnDirector.CurrentRound != CacheTurn) {
        modifiersCache.Clear();
        CacheTurn = weapon.parent.Combat.TurnDirector.CurrentRound;
      }
      ExtAmmunitionDef ammo = weapon.ammo();
      WeaponMode mode = weapon.mode();
      bool isBreachingShot = false;
      if (breachingShot == TripleBoolean.NotSet) {
        if (weapon.IsEnabled) {
          if (weapon.parent.HasBreachingShotAbility) {
            isBreachingShot = true;
            foreach (Weapon wp in weapon.parent.Weapons) {
              if ((wp.Type != WeaponType.Melee) && (wp.IsEnabled) && (wp != weapon)) { isBreachingShot = false; break; }
            }
          }
        }
      } else {
        isBreachingShot = breachingShot == TripleBoolean.True;
      }
      if (modifiersCache.TryGetValue(weapon, out Dictionary<bool, Dictionary<ExtAmmunitionDef, Dictionary<WeaponMode, Dictionary<Vector3, Dictionary<ICombatant, DamageModifiers>>>>> bMods) == false) {
        bMods = new Dictionary<bool, Dictionary<ExtAmmunitionDef, Dictionary<WeaponMode, Dictionary<Vector3, Dictionary<ICombatant, DamageModifiers>>>>>();
        modifiersCache.Add(weapon, bMods);
      }
      if (bMods.TryGetValue(isBreachingShot, out Dictionary<ExtAmmunitionDef, Dictionary<WeaponMode, Dictionary<Vector3, Dictionary<ICombatant, DamageModifiers>>>> wMods) == false) {
        wMods = new Dictionary<ExtAmmunitionDef, Dictionary<WeaponMode, Dictionary<Vector3, Dictionary<ICombatant, DamageModifiers>>>>();
        bMods.Add(isBreachingShot, wMods);
      }
      if(wMods.TryGetValue(ammo, out Dictionary<WeaponMode, Dictionary<Vector3, Dictionary<ICombatant, DamageModifiers>>> waMods) == false) {
        waMods = new Dictionary<WeaponMode, Dictionary<Vector3, Dictionary<ICombatant, DamageModifiers>>>();
        wMods.Add(ammo, waMods);
      }
      if (waMods.TryGetValue(mode, out  Dictionary<Vector3, Dictionary<ICombatant, DamageModifiers>> wamMods) == false) {
        wamMods = new Dictionary<Vector3, Dictionary<ICombatant, DamageModifiers>>();
        waMods.Add(mode, wamMods);
      }
      if(wamMods.TryGetValue(attackPosition, out Dictionary<ICombatant, DamageModifiers> wampMods) == false) {
        wampMods = new Dictionary<ICombatant, DamageModifiers>();
        wamMods.Add(attackPosition, wampMods);
      }
      if(wampMods.TryGetValue(target, out DamageModifiers result) == false) {
        result = new DamageModifiers(weapon,attackPosition,target, isBreachingShot);
        wampMods.Add(target, result);
      }
      return result;
    }
  }
}