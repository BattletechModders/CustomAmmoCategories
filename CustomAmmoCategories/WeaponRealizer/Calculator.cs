﻿using BattleTech;
using CustomAmmoCategoriesLog;
using UnityEngine;

namespace WeaponRealizer {
  public static partial class Calculator {
    internal const float Epsilon = 0.0001f;

    public static float ApplyDamageModifiers(Vector3 attackPos, ICombatant target, Weapon weapon, float rawDamage, bool log) {
      return ApplyAllDamageModifiers(attackPos, target, weapon, rawDamage, true, log);
    }

    public static float ApplyAllDamageModifiers(Vector3 attackPos, ICombatant target, Weapon weapon, float rawDamage, bool calculateRandomComponent, bool log) {
      var damage = rawDamage;

      if (calculateRandomComponent && SimpleVariance.IsApplicable(weapon)) {
        Log.M.WL("WR simple variance:" + weapon.defId + " rawDamage:" + rawDamage );
        damage = SimpleVariance.Calculate(weapon, rawDamage, log);
      }

      if (DistanceBasedVariance.IsApplicable(weapon)) {
        Log.M.WL("WR distance variance:" + weapon.defId +" target:" +target.DisplayName+ " damage:" +damage+ " rawDamage:" + rawDamage );
        damage = DistanceBasedVariance.Calculate(attackPos, target, weapon, damage, rawDamage, log);
      }

      if (ReverseDistanceBasedVariance.IsApplicable(weapon)) {
        Log.M.WL("WR reverse distance variance:" + weapon.defId + " target:" + target.DisplayName + " damage:" + damage + " rawDamage:" + rawDamage);
        damage = ReverseDistanceBasedVariance.Calculate(attackPos, target, weapon, damage, rawDamage,log);
      }

      if (weapon.IsOverheatModApplicable()) {
        Log.M.WL("WR overheat muilt:" + weapon.defId + " target:" + target.DisplayName + " damage:" + damage + " rawDamage:" + rawDamage);
        damage = OverheatMultiplier.Calculate(weapon.parent, target, weapon, damage,log);
      }

      //if (HeatDamageModifier.IsApplicable(weapon))
      //{
      // TODO: this can't work becuse the values don't get ingested from weapondef
      // damage = HeatDamageModifier.Calculate(weapon, damage);
      //}

      //if (HeatAsNormalDamage.IsApplicable(weapon)) {
        //Log.M.WL("WR heat as normal damage:" + weapon.defId + " target:" + target.DisplayName + " damage:" + damage + " rawDamage:" + rawDamage + " heatdamage:" + heatDamage);
        //damage = HeatAsNormalDamage.Calculate(target, weapon, damage, rawDamage, heatDamage);
      //}

      return damage;
    }
  }
}