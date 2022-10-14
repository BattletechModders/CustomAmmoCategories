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
using Harmony;
using HBS.Util;
using MessagePack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace CustAmmoCategories {
  [System.AttributeUsage(System.AttributeTargets.Property)]
  public class StatCollectionFloatAttribute : System.Attribute {
    public StatCollectionFloatAttribute() {}
  }
  [MessagePackObject]
  public class DeferredEffectDef {
    [Key(0)]
    public string id { get; set; }
    [Key(1)]
    public int rounds { get; set; }
    [Key(2)]
    public string text { get; set; }
    [Key(3)]
    public string VFX { get; set; }
    [Key(4)]
    public string waitVFX { get; set; }
    [Key(5)]
    public CustomVector VFXscale { get; set; }
    [Key(6)]
    public CustomVector waitVFXscale { get; set; }
    [Key(7)]
    public string SFX { get; set; }
    [Key(8)]
    public float VFXtime { get; set; }
    [Key(9)]
    public float damageApplyTime { get; set; }
    [Key(10)]
    public List<EffectData> statusEffects { get; set; }
    [Key(11)]
    public float AOERange { get; set; }
    [Key(12)]
    public float AOEDamage { get; set; }
    [Key(13)]
    public float AOEHeatDamage { get; set; }
    [Key(14)]
    public float AOEInstability { get; set; }
    [Key(15)]
    public ColorTableJsonEntry RangeColor { get; set; }
    [Key(16)]
    public float FireTerrainChance { get; set; }
    [Key(17)]
    public int FireDurationWithoutForest { get; set; }
    [Key(18)]
    public int FireTerrainStrength { get; set; }
    [Key(19)]
    public int FireTerrainCellRadius { get; set; }
    [Key(20)]
    public string TerrainVFX { get; set; }
    [Key(21)]
    public string tempDesignMask { get; set; }
    [Key(22)]
    public int tempDesignMaskTurns { get; set; }
    [Key(23)]
    public int tempDesignMaskCellRadius { get; set; }
    [Key(24)]
    public CustomVector TerrainVFXScale { get; set; }
    [Key(25)]
    public bool statusEffectsRangeFalloff { get; set; }
    [Key(26)]
    public bool sticky { get; set; }
    [Key(27)]
    public float MinMissRadius { get; set; }
    [Key(28)]
    public float MaxMissRadius { get; set; }
    [Key(29)]
    public List<string> callMethod { get; set; }
    public DeferredEffectDef() {
      id = "id";
      rounds = 0;
      text = string.Empty;
      VFX = string.Empty;
      SFX = string.Empty;
      VFXtime = 10f;
      damageApplyTime = 5f;
      statusEffects = new List<EffectData>();
      callMethod = new List<string>();
      AOERange = 0f;
      AOEDamage = 0f;
      AOEHeatDamage = 0f;
      AOEInstability = 0f;
      waitVFXscale = new CustomVector(true);
      VFXscale = new CustomVector(true);
      RangeColor = new ColorTableJsonEntry();
      FireTerrainChance = 0f;
      FireDurationWithoutForest = 0;
      FireTerrainStrength = 0;
      FireTerrainCellRadius = 0;
      tempDesignMaskTurns = 0;
      TerrainVFXScale = new CustomVector(true);
      statusEffectsRangeFalloff = true;
      sticky = true;
      MinMissRadius = 5f;
      MaxMissRadius = 15f;
    }
    public void ParceEffects(string json) {
      try {
        statusEffects.Clear();
        JArray effects = JArray.Parse(json);
        foreach (JObject statusEffect in effects) {
          EffectData effect = new EffectData();
          JSONSerializationUtility.FromJSON<EffectData>(effect, statusEffect.ToString());
          statusEffects.Add(effect);
        }
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
    }
    public void debugPrint(LogFile log,int initiation) {
      log.WL(initiation, "rounds:"+rounds);
      log.WL(initiation, "VFX:" + VFX);
      log.WL(initiation, "VFXscale:" + VFXscale);
      log.WL(initiation, "SFX:" + SFX);
      log.WL(initiation, "VFXtime:" + VFXtime);
      log.WL(initiation, "AOERange:" + AOERange);
      log.WL(initiation, "AOEDamage:" + AOEDamage);
      log.WL(initiation, "AOEHeatDamage:" + AOEHeatDamage);
      log.WL(initiation, "AOEInstability:" + AOEInstability);
      log.WL(initiation, "statusEffects:" + statusEffects.Count);
      foreach (EffectData effect in statusEffects) {
        log.WL(initiation + 1, effect.Description.Id);
        log.WL(initiation + 1, effect.effectType.ToString());
        log.WL(initiation + 1, effect.statisticData.statName);
      }
    }
  }
  [MessagePackObject]
  public class ExtDefinitionParceInfo {
    [Key(0)]
    public object extDef { get; set; }
    [Key(1)]
    public string baseJson { get; set; }
    [Key(2)]
    public string errorStr { get; set; }
    public ExtDefinitionParceInfo() {
      extDef = null;
    }
  }
  public enum DamageFalloffType { NotSet, Quadratic, Cubic, SquareRoot, Log10, LogE, Exp, Linear }
  public static partial class CustomAmmoCategories {
    public static bool isImprovedBallistic(this Weapon weapon) { return weapon.exDef().ImprovedBallistic; }
    public static bool CantHitUnaffectedByPathing(this Weapon weapon) {
      ExtAmmunitionDef ammo = weapon.ammo();
      WeaponMode mode = weapon.mode();
      ExtWeaponDef wp = weapon.exDef();
      if(mode.CantHitUnaffecedByPathing != TripleBoolean.NotSet) {
        return mode.CantHitUnaffecedByPathing == TripleBoolean.True;
      }
      if (ammo.CantHitUnaffecedByPathing != TripleBoolean.NotSet) {
        return ammo.CantHitUnaffecedByPathing == TripleBoolean.True;
      }
      return wp.CantHitUnaffecedByPathing == TripleBoolean.True;
    }
    public static int MissileVolleySize(this Weapon weapon) {
      ExtAmmunitionDef ammo = weapon.ammo();
      WeaponMode mode = weapon.mode();
      ExtWeaponDef wp = weapon.exDef();
      if (mode.MissileVolleySize > 0) {
        return mode.MissileVolleySize;
      }
      if (ammo.MissileVolleySize > 0) {
        return ammo.MissileVolleySize;
      }
      return wp.MissileVolleySize;
    }
    public static CustomVector ProjectileScale(this Weapon weapon) {
      ExtAmmunitionDef ammo = weapon.ammo();
      WeaponMode mode = weapon.mode();
      ExtWeaponDef wp = weapon.exDef();
      if (mode.ProjectileScale.set) {
        return mode.ProjectileScale;
      }
      if (ammo.ProjectileScale.set) {
        return ammo.ProjectileScale;
      }
      return wp.ProjectileScale;
    }
    public static float FireAnimationSpeedMod(this Weapon weapon) {
      ExtAmmunitionDef ammo = weapon.ammo();
      WeaponMode mode = weapon.mode();
      ExtWeaponDef wp = weapon.exDef();
      return mode.FireAnimationSpeedMod * ammo.FireAnimationSpeedMod * wp.FireAnimationSpeedMod;
    }
    public static float PrefireAnimationSpeedMod(this Weapon weapon) {
      ExtAmmunitionDef ammo = weapon.ammo();
      WeaponMode mode = weapon.mode();
      ExtWeaponDef wp = weapon.exDef();
      return mode.PrefireAnimationSpeedMod * ammo.PrefireAnimationSpeedMod * wp.PrefireAnimationSpeedMod;
    }
    public static float APArmorShardsMod(this Weapon weapon) {
      ExtAmmunitionDef ammo = weapon.ammo();
      WeaponMode mode = weapon.mode();
      ExtWeaponDef wp = weapon.exDef();
      return mode.APArmorShardsMod + ammo.APArmorShardsMod + wp.APArmorShardsMod;
    }
    public static float DamageFalloffStartDistance(this Weapon weapon) {
      ExtAmmunitionDef ammo = weapon.ammo();
      WeaponMode mode = weapon.mode();
      ExtWeaponDef wp = weapon.exDef();
      return mode.DamageFalloffStartDistance + ammo.DamageFalloffStartDistance + wp.DamageFalloffStartDistance;
    }
    public static float DamageFalloffEndDistance(this Weapon weapon) {
      ExtAmmunitionDef ammo = weapon.ammo();
      WeaponMode mode = weapon.mode();
      ExtWeaponDef wp = weapon.exDef();
      return mode.DamageFalloffEndDistance + ammo.DamageFalloffEndDistance + wp.DamageFalloffEndDistance;
    }
    public static float APMaxArmorThickness(this Weapon weapon) {
      ExtAmmunitionDef ammo = weapon.ammo();
      WeaponMode mode = weapon.mode();
      ExtWeaponDef wp = weapon.exDef();
      return mode.APMaxArmorThickness + ammo.APMaxArmorThickness + wp.APMaxArmorThickness;
    }
    public static bool isAPCrit(this Weapon weapon) {
      ExtAmmunitionDef ammo = weapon.ammo();
      WeaponMode mode = weapon.mode();
      ExtWeaponDef wp = weapon.exDef();
      return (float.IsNaN(mode.APCriticalChanceMultiplier) == false)
        || (float.IsNaN(ammo.APCriticalChanceMultiplier) == false)
        || (float.IsNaN(wp.APCriticalChanceMultiplier) == false);
    }
    public static float APCriticalChanceMultiplier(this Weapon weapon) {
      ExtAmmunitionDef ammo = weapon.ammo();
      WeaponMode mode = weapon.mode();
      ExtWeaponDef wp = weapon.exDef();
      float result = (float.IsNaN(mode.APCriticalChanceMultiplier)?0f: mode.APCriticalChanceMultiplier) 
        + (float.IsNaN(ammo.APCriticalChanceMultiplier)?0f: ammo.APCriticalChanceMultiplier)
        + (float.IsNaN(wp.APCriticalChanceMultiplier)?0f: wp.APCriticalChanceMultiplier);
      if (weapon.parent != null) {
        if (weapon.parent.EvasivePipsCurrent > 0) {
          float evasiveMod = weapon.exDef().evasivePipsMods.APDamage + ammo.evasivePipsMods.APDamage + mode.evasivePipsMods.APDamage;
          if (Mathf.Abs(evasiveMod) > CustomAmmoCategories.Epsilon) result = result * Mathf.Pow((float)weapon.parent.EvasivePipsCurrent, evasiveMod);
        }
      }
      return result;
    }
    public static bool DamageNotDivided(this Weapon weapon) {
      ExtAmmunitionDef ammo = weapon.ammo();
      WeaponMode mode = weapon.mode();
      ExtWeaponDef wp = weapon.exDef();
      if (mode.DamageNotDivided != TripleBoolean.NotSet) { return mode.DamageNotDivided == TripleBoolean.True; }
      if (ammo.DamageNotDivided != TripleBoolean.NotSet) { return ammo.DamageNotDivided == TripleBoolean.True; }
      return wp.DamageNotDivided == TripleBoolean.True;
    }
    public static bool AMSShootsEveryAttack(this Weapon weapon) {
      WeaponMode mode = weapon.mode();
      ExtWeaponDef wp = weapon.exDef();
      if (mode.AMSShootsEveryAttack != TripleBoolean.NotSet) { return mode.AMSShootsEveryAttack == TripleBoolean.True; }
      return wp.AMSShootsEveryAttack == TripleBoolean.True;
    }
    public static bool AOEEffectsFalloff(this Weapon weapon) {
      ExtAmmunitionDef ammo = weapon.ammo();
      WeaponMode mode = weapon.mode();
      ExtWeaponDef wp = weapon.exDef();
      if (mode.AOEEffectsFalloff != TripleBoolean.NotSet) { return mode.AOEEffectsFalloff == TripleBoolean.True; }
      if (ammo.AOEEffectsFalloff != TripleBoolean.NotSet) { return ammo.AOEEffectsFalloff == TripleBoolean.True; }
      return wp.AOEEffectsFalloff == TripleBoolean.True;
    }
    public static bool isDamageVariation(this Weapon weapon) {
      ExtAmmunitionDef ammo = weapon.ammo();
      WeaponMode mode = weapon.mode();
      ExtWeaponDef wp = weapon.exDef();
      if (mode.isDamageVariation != TripleBoolean.NotSet) { return mode.isDamageVariation == TripleBoolean.True; }
      if (ammo.isDamageVariation != TripleBoolean.NotSet) { return ammo.isDamageVariation == TripleBoolean.True; }
      return wp.isDamageVariation == TripleBoolean.True;
    }
    public static bool isHeatVariation(this Weapon weapon) {
      ExtAmmunitionDef ammo = weapon.ammo();
      WeaponMode mode = weapon.mode();
      ExtWeaponDef wp = weapon.exDef();
      if (mode.isHeatVariation != TripleBoolean.NotSet) { return mode.isHeatVariation == TripleBoolean.True; }
      if (ammo.isHeatVariation != TripleBoolean.NotSet) { return ammo.isHeatVariation == TripleBoolean.True; }
      return wp.isHeatVariation == TripleBoolean.True;
    }
    public static bool isStabilityVariation(this Weapon weapon) {
      ExtAmmunitionDef ammo = weapon.ammo();
      WeaponMode mode = weapon.mode();
      ExtWeaponDef wp = weapon.exDef();
      if (mode.isStabilityVariation != TripleBoolean.NotSet) { return mode.isStabilityVariation == TripleBoolean.True; }
      if (ammo.isStabilityVariation != TripleBoolean.NotSet) { return ammo.isStabilityVariation == TripleBoolean.True; }
      return wp.isStabilityVariation == TripleBoolean.True;
    }
    public static bool TargetLegsOnly(this Weapon weapon) {
      WeaponMode mode = weapon.mode();
      ExtWeaponDef wp = weapon.exDef();
      if (mode.TargetMechLegsOnly != TripleBoolean.NotSet) { return mode.TargetMechLegsOnly == TripleBoolean.True; }
      return wp.TargetMechLegsOnly == TripleBoolean.True;
    }
    /*public static CustomVector MissileExplosionScale(this Weapon weapon) {
      ExtAmmunitionDef ammo = weapon.ammo();
      WeaponMode mode = weapon.mode();
      ExtWeaponDef wp = weapon.exDef();
      if (mode.MissileExplosionScale.set) {
        return mode.MissileExplosionScale;
      }
      if (ammo.MissileExplosionScale.set) {
        return ammo.MissileExplosionScale;
      }
      return wp.MissileExplosionScale;
    }*/
    public static float ColorSpeedChange(this Weapon weapon) {
      ExtAmmunitionDef ammo = weapon.ammo();
      WeaponMode mode = weapon.mode();
      ExtWeaponDef wp = weapon.exDef();
      if (ammo.ColorsTable.Count > 0) { return ammo.ColorSpeedChange; };
      if (mode.ColorSpeedChange > CustomAmmoCategories.Epsilon) {
        return mode.ColorSpeedChange;
      }
      if (ammo.ColorSpeedChange > CustomAmmoCategories.Epsilon) {
        return ammo.ColorSpeedChange;
      }
      return wp.ColorSpeedChange;
    }
    public static DamageFalloffType RangedDmgFalloffType(this Weapon weapon) {
      ExtAmmunitionDef ammo = weapon.ammo();
      WeaponMode mode = weapon.mode();
      ExtWeaponDef wp = weapon.exDef();
      if (mode.RangedDmgFalloffType != DamageFalloffType.NotSet) { return mode.RangedDmgFalloffType; };
      if (ammo.RangedDmgFalloffType != DamageFalloffType.NotSet) { return ammo.RangedDmgFalloffType; };
      return wp.RangedDmgFalloffType;
    }
    public static DamageFalloffType AoEDmgFalloffType(this Weapon weapon) {
      ExtAmmunitionDef ammo = weapon.ammo();
      WeaponMode mode = weapon.mode();
      ExtWeaponDef wp = weapon.exDef();
      if (mode.AoEDmgFalloffType != DamageFalloffType.NotSet) { return mode.AoEDmgFalloffType; };
      if (ammo.AoEDmgFalloffType != DamageFalloffType.NotSet) { return ammo.AoEDmgFalloffType; };
      return wp.AoEDmgFalloffType;
    }
    public static float RangedDmgFalloffType(this Weapon weapon, float value) {
      switch (weapon.RangedDmgFalloffType()) {
        case DamageFalloffType.Quadratic: return value * value;
        case DamageFalloffType.Cubic: return value * value * value;
        case DamageFalloffType.SquareRoot: return Mathf.Sqrt(value);
        case DamageFalloffType.Linear: return value;
        case DamageFalloffType.Log10: return Mathf.Log10(value);
        case DamageFalloffType.LogE: return Mathf.Log(value);
        case DamageFalloffType.Exp: return Mathf.Exp(value);
        default: return value * value;
      }
    }
    public static float AoEDmgFalloffType(this Weapon weapon, float value) {
      switch (weapon.AoEDmgFalloffType()) {
        case DamageFalloffType.Quadratic: return value * value;
        case DamageFalloffType.Cubic: return value * value * value;
        case DamageFalloffType.SquareRoot: return Mathf.Sqrt(value);
        case DamageFalloffType.Linear: return value;
        case DamageFalloffType.Log10: return Mathf.Log10(value+1f);
        case DamageFalloffType.LogE: return Mathf.Log(value+1f);
        case DamageFalloffType.Exp: return Mathf.Exp(value);
        default: return value;
      }
    }
    public static List<ColorTableJsonEntry> ColorsTable(this Weapon weapon) {
      ExtAmmunitionDef ammo = weapon.ammo();
      ExtWeaponDef wp = weapon.exDef();
      if (ammo.ColorsTable.Count > 0) {
        return ammo.ColorsTable;
      }
      return wp.ColorsTable;
    }
    public static ColorChangeRule colorChangeRule(this Weapon weapon) {
      ExtAmmunitionDef ammo = weapon.ammo();
      WeaponMode mode = weapon.mode();
      ExtWeaponDef wp = weapon.exDef();
      if(ammo.ColorsTable.Count > 0) { return ammo.ColorChangeRule; };
      if (mode.ColorChangeRule != ColorChangeRule.None) {
        return mode.ColorChangeRule;
      }
      if (ammo.ColorChangeRule != ColorChangeRule.None) {
        return ammo.ColorChangeRule;
      }
      return wp.ColorChangeRule;
    }
  }
  [SelfDocumentedClass("Weapons","Weapons", "WeaponDef"), MessagePackObject]
  public class ExtWeaponDef {
    public static readonly string StatisticAttributePrefix = "CAC_";
    public static readonly string StatisticModifierSuffix = "_Mod";
    public static void RemoveTag(ref JObject json, string tag) {
      Log.LogWrite("ExtWeaponDef.RemoveTag(" + tag + ")\n");
      if (json["ComponentTags"] == null) {
        Log.LogWrite(" no ComponentTags section\n");
        return;
      }
      if (json["ComponentTags"]["items"] == null) {
        Log.LogWrite(" no items section\n");
        return;
      }
      if (json["ComponentTags"]["items"].Type != JTokenType.Array) {
        Log.LogWrite(" no items not array\n");
        return;
      }
      JArray items = json["ComponentTags"]["items"] as JArray;
      if (items == null) {
        Log.LogWrite(" no items not converted array\n");
        return;
      }
      for (int t = 0; t < items.Count;) {
        Log.LogWrite(" tag:" + items[t] + "\n");
        if (((string)items[t]) != tag) { ++t; } else { items.RemoveAt(t); Log.LogWrite("  removed\n"); };
      }
    }
    [Key(0)]
    public string Id { get; set; } = string.Empty;
    [Key(1)]
    public HitGeneratorType HitGenerator { get; set; } = HitGeneratorType.NotSet;
    [Key(2)]
    public bool Streak { get; set; } = false;
    [StatCollectionFloat, Key(3)]
    public float DirectFireModifier { get; set; } = 0f;
    public float DirectFireModifierStat(Weapon weapon) { return weapon.GetStatisticFloat(nameof(DirectFireModifier)); }
    public float DirectFireModifierMod(Weapon weapon) { return weapon.GetStatisticMod(nameof(DirectFireModifier)); }
    [JsonIgnore, SkipDocumentation, Key(4)]
    public string baseModeId { get; set; } = WeaponMode.BASE_MODE_NAME;
    [Key(5)]
    public float FlatJammingChance { get; set; } = 0f;
    [Key(6)]
    public float AMSHitChance { get; set; } = 0f;
    [Key(7)]
    public float GunneryJammingBase { get; set; } = 0f;
    [Key(8)]
    public float GunneryJammingMult { get; set; } = 0f;
    [Key(9)]
    public TripleBoolean DamageOnJamming { get; set; } = TripleBoolean.NotSet;
    [Key(10)]
    public TripleBoolean DestroyOnJamming { get; set; } = TripleBoolean.NotSet;
    [Key(11)]
    public TripleBoolean AMSImmune { get; set; } = TripleBoolean.NotSet;
    [Key(12)]
    public bool AlternateDamageCalc { get; set; } = false;
    [Key(13)]
    public bool AlternateHeatDamageCalc { get; set; } = false;
    [Key(14)]
    public bool AlternateInstabilityCalc { get; set; } = false;
    [Key(15)]
    public bool AlternateAPDamageCalc { get; set; } = false;
    [Key(16)]
    public TripleBoolean IsAMS { get; set; } = TripleBoolean.NotSet;
    [Key(17)]
    public TripleBoolean IsAAMS { get; set; } = TripleBoolean.NotSet;
    [Key(18)]
    public TripleBoolean AMSShootsEveryAttack { get; set; } = TripleBoolean.NotSet;
    [StatCollectionFloat, Key(19)]
    public float SpreadRange { get; set; } = 0f;
    public float SpreadRangeStat(Weapon weapon) { return weapon.GetStatisticFloat(nameof(SpreadRange)); }
    public float SpreadRangeMod(Weapon weapon) { return weapon.GetStatisticMod(nameof(SpreadRange)); }
    [Key(20)]
    public TripleBoolean NotUseInMelee { get; set; } = TripleBoolean.NotSet;
    [SelfDocumentationTypeName("dictionary of string => WeaponMode"), SelfDocumentationDefaultValue("empty"), Key(21)]
    public Dictionary<string, WeaponMode> Modes { get; set; } = new Dictionary<string, WeaponMode>();
    [JsonIgnore, SelfDocumentationTypeName("string, id from BattleTech.AmmoCategoryEnumeration or CustomAmmo"), SelfDocumentationDefaultValue("NotSet"), IgnoreMember]
    public CustomAmmoCategory AmmoCategory { get; set; } = new CustomAmmoCategory();
    [Key(22), SkipDocumentation, JsonIgnore]
    public string AmmoCategoryID { get { return AmmoCategory.Id; } set { AmmoCategory = CustomAmmoCategories.find(value); } }
    [Key(23)]
    public TripleBoolean DisableClustering { get; set; } = TripleBoolean.NotSet;
    [Key(24)]
    public TripleBoolean AOECapable { get; set; } = TripleBoolean.NotSet;
    [Key(25)]
    public float AOERange { get; set; } = 0f;
    [Key(26)]
    public float AOEDamage { get; set; } = 0f;
    [Key(27)]
    public float AOEHeatDamage { get; set; } = 0f;
    [Key(28)]
    public float AOEInstability { get; set; } = 0f;
    [Key(29)]
    public string IFFDef { get; set; } = string.Empty;
    [Key(30)]
    public TripleBoolean HasShells { get; set; } = TripleBoolean.NotSet;
    [Key(31)]
    public float ShellsRadius { get; set; } = 0f;
    [Key(32)]
    public float MinShellsDistance { get; set; } = 30f;
    [Key(33)]
    public float MaxShellsDistance { get; set; } = 30f;
    [Key(34)]
    public TripleBoolean Unguided { get; set; } = TripleBoolean.NotSet;
    [Key(35)]
    public TripleBoolean AOEEffectsFalloff { get; set; } = TripleBoolean.NotSet;
    [Key(36)]
    public float ArmorDamageModifier { get; set; } = 1f;
    [Key(37)]
    public float ISDamageModifier { get; set; } = 1f;
    [Key(38)]
    public float FireTerrainChance { get; set; } = 0f;
    [Key(39)]
    public int FireDurationWithoutForest { get; set; } = 0;
    [Key(40)]
    public int FireTerrainStrength { get; set; } = 0;
    [Key(41)]
    public int FireTerrainCellRadius { get; set; } = 0;
    [Key(42)]
    public string AdditionalImpactVFX { get; set; } = string.Empty;
    [Key(43)]
    public TripleBoolean FireOnSuccessHit { get; set; } = TripleBoolean.NotSet;
    [Key(44)]
    public float AdditionalImpactVFXScaleX { get; set; } = 1f;
    [Key(45)]
    public float AdditionalImpactVFXScaleY { get; set; } = 1f;
    [Key(46)]
    public float AdditionalImpactVFXScaleZ { get; set; } = 1f;
    [Key(47)]
    public int ClearMineFieldRadius { get; set; } = 0;
    [Key(48)]
    public int Cooldown { get; set; } = 0;
    [SelfDocumentationDefaultValue("ImprovedBallisticByDefault value from settings"), Key(49)]
    public bool ImprovedBallistic { get; set; } = CustomAmmoCategories.Settings.ImprovedBallisticByDefault;
    [Key(50)]
    public TripleBoolean BallisticDamagePerPallet { get; set; } = TripleBoolean.NotSet;
    [Key(51)]
    public TripleBoolean StatusEffectsPerHit { get; set; } = TripleBoolean.NotSet;
    [Key(52)]
    public string AdditionalAudioEffect { get; set; } = string.Empty;
    [Key(53)]
    public float FireDelayMultiplier { get; set; } = 10f;
    [Key(54)]
    public float MissileFiringIntervalMultiplier { get; set; } = 1f;
    [Key(55)]
    public float MissileVolleyIntervalMultiplier { get; set; } = 1f;
    [Key(56)]
    public float ProjectileSpeedMultiplier { get; set; } = 1f;
    [Key(57)]
    public TripleBoolean CantHitUnaffecedByPathing { get; set; } = TripleBoolean.NotSet;
    [Key(58)]
    public int MissileVolleySize { get; set; } = 0;
    [SelfDocumentationDefaultValue("{ \"x\": 1.0,\"y\":1.0, \"z\":1.0 }"), SelfDocumentationTypeName("Vector"), Key(59)]
    public CustomVector ProjectileScale { get; set; } = new CustomVector(true);
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("array of { \"I\": <float intensivity>, \"C\":\"<html color string>\"}"), Key(60)]
    public List<ColorTableJsonEntry> ColorsTable { get; set; } = new List<ColorTableJsonEntry>();
    [Key(61)]
    public float ColorSpeedChange { get; set; } = 0f;
    [Key(62)]
    public ColorChangeRule ColorChangeRule { get; set; } = ColorChangeRule.None;
    [SelfDocumentationDefaultValue("undefined"), Key(63)]
    public float APCriticalChanceMultiplier { get; set; } = float.NaN;
    [Key(64)]
    public float APArmorShardsMod { get; set; } = 0f;
    [Key(65)]
    public float APMaxArmorThickness { get; set; } = 0f;
    [Key(66)]
    public TripleBoolean DamageNotDivided { get; set; } = TripleBoolean.NotSet;
    [Key(67)]
    public TripleBoolean isHeatVariation { get; set; } = TripleBoolean.NotSet;
    [Key(68)]
    public TripleBoolean isStabilityVariation { get; set; } = TripleBoolean.NotSet;
    [Key(69)]
    public TripleBoolean isDamageVariation { get; set; } = TripleBoolean.True;
    [Key(70)]
    public float DistantVariance { get; set; } = 0f;
    [Key(71)]
    public TripleBoolean DistantVarianceReversed { get; set; } = TripleBoolean.NotSet;
    [Key(72)]
    public float PrefireAnimationSpeedMod { get; set; } = 1f;
    [Key(73)]
    public float FireAnimationSpeedMod { get; set; } = 1f;
    [Key(74)]
    public TripleBoolean AlwaysIndirectVisuals { get; set; } = TripleBoolean.NotSet;
    [Key(75)]
    public float ForbiddenRange { get; set; } = 0f;
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("EvasivePipsMods structure"), Key(76)]
    public EvasivePipsMods evasivePipsMods { get; set; } = new EvasivePipsMods();
    [Key(77)]
    public float ShotsPerAmmo { get; set; } = 1f;
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("DeferredEffectDef structure"), Key(78)]
    public DeferredEffectDef deferredEffect { get; set; } = new DeferredEffectDef();
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("Dictionary of {\"<ammo id>\":<integer internal ammo amount>}"), Key(79)]
    public Dictionary<string, int> InternalAmmo { get; set; } = new Dictionary<string, int>();
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("List of mech chassis locations"), Key(80)]
    public List<ChassisLocations> blockWeaponsInMechLocations { get; set; } = new List<ChassisLocations>();
    [Key(81)]
    public bool blockWeaponsInInstalledLocation { get; set; } = false;
    [Key(82)]
    public bool CanBeBlocked { get; set; } = true;
    [Key(83)]
    public bool EjectWeapon { get; set; } = false;
    [JsonIgnore, SkipDocumentation, IgnoreMember]
    public bool isHaveInternalAmmo { get {
        foreach (var ia in InternalAmmo) { if (ia.Value > 0) { return true; }; }
        return false;
    } }
    [Key(84)]
    public string preFireSFX { get; set; } = string.Empty;
    [StatCollectionFloat(), Key(85)]
    public float MinMissRadius { get; set; } = 0f;
    [StatCollectionFloat(), Key(86)]
    public float MaxMissRadius { get; set; } = 0f;
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("Dictionary of {\"<tag name>\":<float modifier>}"), Key(87)]
    public Dictionary<string, float> TagsAccuracyModifiers { get; set; } = new Dictionary<string, float>();
    [StatCollectionFloat, Key(88)]
    public float AMSDamage { get; set; } = 1f;
    [StatCollectionFloat, Key(89)]
    public float MissileHealth { get; set; } = 1f;
    [SelfDocumentationDefaultValue("Linear"), Key(90)]
    public DamageFalloffType RangedDmgFalloffType { get; set; } = DamageFalloffType.NotSet;
    [Key(91)]
    public DamageFalloffType AoEDmgFalloffType { get; set; } = DamageFalloffType.NotSet;
    [Key(92)]
    public float DamageFalloffStartDistance { get; set; } = 0f;
    [Key(93)]
    public float DamageFalloffEndDistance { get; set; } = 0f;
    [Key(94)]
    public float BuildingsDamageModifier { get; set; } = 1f;
    [Key(95)]
    public float TurretDamageModifier { get; set; } = 1f;
    [Key(96)]
    public float VehicleDamageModifier { get; set; } = 1f;
    [Key(97)]
    public float VTOLDamageModifier { get; set; } = 1f;
    [Key(98)]
    public float MechDamageModifier { get; set; } = 1f;
    [Key(99)]
    public float QuadDamageModifier { get; set; } = 1f;
    [Key(100)]
    public float TrooperSquadDamageModifier { get; set; } = 1f;
    [Key(101)]
    public float AirMechDamageModifier { get; set; } = 1f;
    [Key(102)]
    public TripleBoolean TargetMechLegsOnly { get; set; } = TripleBoolean.NotSet;
    [Key(103)]
    public bool alwaysMiss { get; set; } = false;
    [Key(104)]
    public string fireSFX { get; set; } = string.Empty;
    public ExtWeaponDef() { }
  }
}

namespace CustomAmmoCategoriesPatches {
  [HarmonyPatch(typeof(WeaponDef))]
  [HarmonyPatch("FromJSON")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class BattleTech_WeaponDef_fromJSON_Patch {
    public static bool Prefix(WeaponDef __instance, ref string json, ref ExtDefinitionParceInfo __state) {
      CustomAmmoCategories.CustomCategoriesInit();
      if(__instance.Description != null) {
        ExtWeaponDef extWeaponDef = CustomPrewarm.Core.getDeserializedObject(BattleTechResourceType.WeaponDef, __instance.Description.Id, "CustomAmmoCategories") as ExtWeaponDef;
        if (extWeaponDef != null) {
          __state = new ExtDefinitionParceInfo();
          __state.baseJson = json;
          __state.extDef = extWeaponDef;
          __state.errorStr = string.Empty;
          return true;
        }
        Log.M.TWL(0, "WeaponDef.FromJSON Description:"+ __instance.Description.Id+ " is not null, but extWeaponDef not found");
      }
      //Log.LogWrite("WeaponDef fromJSON ");
      JObject defTemp = null;
      __state = new ExtDefinitionParceInfo();
      __state.baseJson = json;
      try {
        defTemp = JObject.Parse(json);
      } catch (Exception e) {
        __state.errorStr = e.ToString();
        return true;
      }
      try {
        string Id = (string)defTemp["Description"]["Id"];
        Log.LogWrite(Id + "\n");
        ExtWeaponDef extDef = null;
        if (CustomAmmoCategories.isRegistredWeapon(Id)) {
          extDef = CustomAmmoCategories.getExtWeaponDef(Id);
        }

        string AmmoCategory = "NotSet";
        if ((defTemp["AmmoCategory"] != null)&&(defTemp["ammoCategoryID"] == null)) {
          AmmoCategory = (string)defTemp["AmmoCategory"];
        } else {
          AmmoCategory = (string)defTemp["ammoCategoryID"];
        }
        if (defTemp["AmmoCategory"] != null) {
          defTemp.Remove("AmmoCategory");
        }
        if (CustomAmmoCategories.contains(AmmoCategory) == false) {
          Log.M.TWL(0, "Custom Ammo Categories list not contains "+AmmoCategory);
          CustomAmmoCategories.printItems();
          AmmoCategoryValue val = AmmoCategoryEnumeration.GetAmmoCategoryByName(AmmoCategory);
          if (val == null) {
            Log.M.WL(1, "AmmoCategoryEnumeration also not contains " + AmmoCategory+" fallback to NotSet");
            CustomAmmoCategories.printAmmo();
            AmmoCategory = "NotSet";
          } else {
            Log.M.WL(1, "Adding new value to Custom Ammo Categories list "+val.Name+":"+val.ID);
            CustomAmmoCategories.add(val);
          }
        }
        CustomAmmoCategory custCat = CustomAmmoCategories.find(AmmoCategory);
        defTemp["ammoCategoryID"] = custCat.BaseCategory.Name;
        Log.M.WL(1, "ammoCategoryID:" + (string)defTemp["ammoCategoryID"]);
        //CustomAmmoCategories.RegisterWeapon((string)defTemp["Description"]["Id"], custCat);
        if (extDef == null) {
          extDef = new ExtWeaponDef();
          extDef.Id = Id;
          extDef.AmmoCategory = custCat;
        };
        if (defTemp["Streak"] != null) {
          extDef.Streak = (bool)defTemp["Streak"];
          defTemp.Remove("Streak");
        }
        if (defTemp["HitGenerator"] != null) {
          try {
            extDef.HitGenerator = (HitGeneratorType)Enum.Parse(typeof(HitGeneratorType), (string)defTemp["HitGenerator"], true);
            CustomAmmoCategoriesLog.Log.LogWrite("HitGenerator is " + extDef.HitGenerator + "\n");
          } catch (Exception) {
            extDef.HitGenerator = HitGeneratorType.NotSet;
            CustomAmmoCategoriesLog.Log.LogWrite("Can't parce " + (string)defTemp["HitGenerator"] + " as HitGenerator\n");
          }
          defTemp.Remove("HitGenerator");
        }
        if (defTemp["APDamage"] != null) {
          defTemp["StructureDamage"] = (float)defTemp["APDamage"];
          defTemp.Remove("APDamage");
        }
        if (defTemp["ColorsTable"] != null) {
          extDef.ColorsTable = defTemp["ColorsTable"].ToObject<List<ColorTableJsonEntry>>();
          defTemp.Remove("ColorsTable");
        }
        if (defTemp["ChassisTagsAccuracyModifiers"] != null) {
          extDef.TagsAccuracyModifiers = JsonConvert.DeserializeObject<Dictionary<string, float>>(defTemp["ChassisTagsAccuracyModifiers"].ToString());
          Log.LogWrite((string)defTemp["Description"]["Id"] + " ChassisTagsAccuracyModifiers:\n");
          foreach (var tam in extDef.TagsAccuracyModifiers) {
            Log.LogWrite(" " + tam.Key + ":" + tam.Key);
          }
          defTemp.Remove("ChassisTagsAccuracyModifiers");
        }
        if (defTemp["IsAMS"] != null) {
          extDef.IsAMS = ((bool)defTemp["IsAMS"] == true) ? TripleBoolean.True : TripleBoolean.False;
          defTemp.Remove("IsAMS");
        }
        if (defTemp["IsAAMS"] != null) {
          extDef.IsAAMS = ((bool)defTemp["IsAAMS"] == true) ? TripleBoolean.True : TripleBoolean.False;
          if (extDef.IsAAMS == TripleBoolean.True) {
            extDef.IsAMS = TripleBoolean.True;
          }
          defTemp.Remove("IsAAMS");
        }
        if (defTemp["ImprovedBallistic"] != null) {
          extDef.ImprovedBallistic = (bool)defTemp["ImprovedBallistic"];
          if (extDef.ImprovedBallistic) {
            ExtWeaponDef.RemoveTag(ref defTemp, "wr-clustered_shots");
            extDef.DisableClustering = TripleBoolean.True;
          }
          defTemp.Remove("ImprovedBallistic");
        }
        if (defTemp["InternalAmmo"] != null) {
          extDef.InternalAmmo = JsonConvert.DeserializeObject<Dictionary<string, int>>(defTemp["InternalAmmo"].ToString());
          defTemp.Remove("InternalAmmo");
        }
        if (defTemp["blockWeaponsInMechLocations"] != null) {
          extDef.blockWeaponsInMechLocations = JsonConvert.DeserializeObject<List<ChassisLocations>>(defTemp["blockWeaponsInMechLocations"].ToString());
          defTemp.Remove("blockWeaponsInMechLocations");
        }
        if(defTemp["deferredEffect"] != null) {
          extDef.deferredEffect = JsonConvert.DeserializeObject<DeferredEffectDef>(defTemp["deferredEffect"].ToString());
          if(defTemp["deferredEffect"]["statusEffects"] != null) {
            extDef.deferredEffect.ParceEffects(defTemp["deferredEffect"]["statusEffects"].ToString());
          }
          defTemp.Remove("deferredEffect");
        }
        foreach (PropertyInfo prop in typeof(ExtWeaponDef).GetProperties()) {
          if (defTemp[prop.Name] == null) { continue; }
          if (prop.PropertyType == typeof(float)) {
            prop.SetValue(extDef, (float)defTemp[prop.Name]);
          } else if (prop.PropertyType == typeof(int)) {
            prop.SetValue(extDef, (int)defTemp[prop.Name]);
          } else if (prop.PropertyType == typeof(string)) {
            prop.SetValue(extDef, (string)defTemp[prop.Name]);
          } else if (prop.PropertyType == typeof(TripleBoolean)) {
            prop.SetValue(extDef, ((bool)defTemp[prop.Name] == true) ? TripleBoolean.True : TripleBoolean.False);
          } else if (prop.PropertyType == typeof(bool)) {
            prop.SetValue(extDef, (bool)defTemp[prop.Name]);
          } else if (prop.PropertyType == typeof(EvasivePipsMods)) {
            prop.SetValue(extDef, defTemp[prop.Name].ToObject<EvasivePipsMods>());
          } else if (prop.PropertyType == typeof(CustomVector)) {
            prop.SetValue(extDef, defTemp[prop.Name].ToObject<CustomVector>());
          } else if (prop.PropertyType.IsEnum) {
            prop.SetValue(extDef, Enum.Parse(prop.PropertyType, (string)defTemp[prop.Name]));
          } else { continue; }
          defTemp.Remove(prop.Name);
        }
        if (defTemp["Modes"] != null) {
          if (defTemp["Modes"].Type == JTokenType.Array) {
            extDef.Modes.Clear();
            JToken jWeaponModes = defTemp["Modes"];
            foreach (JObject jWeaponMode in jWeaponModes) {
              string ModeJSON = jWeaponMode.ToString();
              if (string.IsNullOrEmpty(ModeJSON)) { continue; };
              WeaponMode mode = new WeaponMode();
              mode.fromJSON(ModeJSON);
              if (mode.AmmoCategory == null) { mode.AmmoCategory = extDef.AmmoCategory; }
              //mode.AmmoCategory = extDef.AmmoCategory;
              CustomAmmoCategoriesLog.Log.LogWrite(" adding mode '" + mode.Id + "'\n");
              extDef.Modes.Add(mode.Id, mode);
              if (mode.isBaseMode == true) { extDef.baseModeId = mode.Id; }
            }
          }
          defTemp.Remove("Modes");
        }
        if (extDef.baseModeId == WeaponMode.NONE_MODE_NAME) {
          WeaponMode mode = new WeaponMode();
          mode.Id = WeaponMode.BASE_MODE_NAME;
          mode.AmmoCategory = extDef.AmmoCategory;
          extDef.baseModeId = mode.Id;
          extDef.Modes.Add(mode.Id, mode);
          CustomAmmoCategoriesLog.Log.LogWrite(" adding base only mode '" + mode.Id + "'\n");
        }
        defTemp["ammoCategoryID"] = custCat.BaseCategory.Name;
        //CustomAmmoCategoriesLog.Log.LogWrite("\n--------------RESULT----------------\n" + JsonConvert.SerializeObject(extDef,Formatting.Indented) + "\n----------------------------------\n");
        json = defTemp.ToString();
        __state.extDef = extDef;
      }catch(Exception e) {
        __state.errorStr = e.ToString();
      }
      return true;
    }
    public static void Postfix(WeaponDef __instance, ref string json, ref ExtDefinitionParceInfo __state) {
      if (__instance == null) { Log.M.TWL(0,"!WARNINIG! weaponDef is null. Very very wrong!",true); return; }
      try {
        EffectData[] effects = __instance.statusEffects;
        List<EffectData> tmpList = new List<EffectData>();
        foreach (EffectData effect in effects) {
          if ((effect.Description != null)) {
            if ((effect.Description.Id != null) && (effect.Description.Name != null)) {
              tmpList.Add(effect);
              continue;
            }
          }
          CustomAmmoCategoriesLog.Log.LogWrite("!Warning! null status effect detected at weapon " + __instance.Description.Id + ".\n");
        }
        if (tmpList.Count != effects.Length) {
          CustomAmmoCategoriesLog.Log.LogWrite("!Warning! null status effects detected at weapon " + __instance.Description.Id + ".Removing\n");
          PropertyInfo property = typeof(WeaponDef).GetProperty("statusEffects");
          property.DeclaringType.GetProperty("statusEffects");
          property.GetSetMethod(true).Invoke(__instance, new object[1] { (object)tmpList.ToArray() });
        }
        ExtWeaponDef extDef = null;
        if (__state == null) {
          Log.M.TWL(0, "!WARNINIG! ExtDefinitionParceInfo is null for " + __instance.Description.Id + ". Very very wrong!", true);
        } else {
          extDef = __state.extDef as ExtWeaponDef;
        }
        if (extDef == null) {
          Log.M.TWL(0, "!WARNINIG! ext. definition parce error for " + __instance.Description.Id + "\n" + __state.errorStr + "\n" + __state.baseJson, true);
          extDef = new ExtWeaponDef();
          extDef.Id = __instance.Description.Id;
          extDef.AmmoCategory = CustomAmmoCategories.find(__instance.AmmoCategoryValue.Name);
          WeaponMode mode = new WeaponMode();
          mode.Id = WeaponMode.BASE_MODE_NAME;
          mode.AmmoCategory = extDef.AmmoCategory;
          extDef.baseModeId = mode.Id;
          extDef.Modes.Add(mode.Id, mode);
        }
        CustomAmmoCategories.registerExtWeaponDef(__instance.Description.Id, extDef);
        if(__instance.Description.Id == "Weapon_Laser_TAG_Orbital") {
          Log.M.TWL(0, "deferredEffect print:" + __instance.Description.Id);
          extDef.deferredEffect.debugPrint(Log.M, 1);
        }
        if (__instance.StartingAmmoCapacity != 0) {
          if (extDef.AmmoCategory.isDefaultAmmo()) {
            ExtAmmunitionDef extAmmunition = extDef.AmmoCategory.defaultAmmo();
            if (extDef.InternalAmmo.ContainsKey(extAmmunition.Id) == false) { extDef.InternalAmmo.Add(extAmmunition.Id, __instance.StartingAmmoCapacity); }
            Traverse.Create(__instance).Property<int>("StartingAmmoCapacity").Value = 0;
          } else {
            __instance.RegisterForDefaultAmmoUpdate(extDef.AmmoCategory);
          }
        }
      } catch(Exception e) {
        Log.M.TWL(0,e.ToString(),true);
      }
    }
  }
}
