using BattleTech;
using CustAmmoCategories;
using CustomAmmoCategoriesLog;
using Harmony;
using HBS.Util;
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
  public class DeferredEffectDef {
    public string id { get; set; }
    public int rounds { get; set; }
    public string text { get; set; }
    public string VFX { get; set; }
    public string waitVFX { get; set; }
    public CustomVector VFXscale { get; set; }
    public CustomVector waitVFXscale { get; set; }
    public string SFX { get; set; }
    public float VFXtime { get; set; }
    public float damageApplyTime { get; set; }
    public List<EffectData> statusEffects { get; set; }
    public float AOERange { get; set; }
    public float AOEDamage { get; set; }
    public float AOEHeatDamage { get; set; }
    public float AOEInstability { get; set; }
    public ColorTableJsonEntry RangeColor { get; set; }
    public float FireTerrainChance { get; set; }
    public int FireDurationWithoutForest { get; set; }
    public int FireTerrainStrength { get; set; }
    public int FireTerrainCellRadius { get; set; }
    public string TerrainVFX { get; set; }
    public string tempDesignMask { get; set; }
    public int tempDesignMaskTurns { get; set; }
    public int tempDesignMaskCellRadius { get; set; }
    public CustomVector TerrainVFXScale { get; set; }
    public bool statusEffectsRangeFalloff { get; set; }
    public bool sticky { get; set; }
    public float MinMissRadius { get; set; }
    public float MaxMissRadius { get; set; }
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
  public class ExtDefinitionParceInfo {
    public object extDef { get; set; }
    public string baseJson { get; set; }
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
  [SelfDocumentedClass("Weapons","Weapons", "WeaponDef")]
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
    public string Id { get; set; } = string.Empty;
    public HitGeneratorType HitGenerator { get; set; } = HitGeneratorType.NotSet;
    public bool Streak { get; set; } = false;
    [StatCollectionFloat]
    public float DirectFireModifier { get; set; } = 0f;
    public float DirectFireModifierStat(Weapon weapon) { return weapon.GetStatisticFloat(nameof(DirectFireModifier)); }
    public float DirectFireModifierMod(Weapon weapon) { return weapon.GetStatisticMod(nameof(DirectFireModifier)); }
    [JsonIgnore, SkipDocumentation]
    public string baseModeId { get; set; } = WeaponMode.BASE_MODE_NAME;
    public float FlatJammingChance { get; set; } = 0f;
    public float AMSHitChance { get; set; } = 0f;
    public float GunneryJammingBase { get; set; } = 0f;
    public float GunneryJammingMult { get; set; } = 0f;
    public TripleBoolean DamageOnJamming { get; set; } = TripleBoolean.NotSet;
    public TripleBoolean DestroyOnJamming { get; set; } = TripleBoolean.NotSet;
    public TripleBoolean AMSImmune { get; set; } = TripleBoolean.NotSet;
    public bool AlternateDamageCalc { get; set; } = false;
    public bool AlternateHeatDamageCalc { get; set; } = false;
    public bool AlternateInstabilityCalc { get; set; } = false;
    public bool AlternateAPDamageCalc { get; set; } = false;
    public TripleBoolean IsAMS { get; set; } = TripleBoolean.NotSet;
    public TripleBoolean IsAAMS { get; set; } = TripleBoolean.NotSet;
    public TripleBoolean AMSShootsEveryAttack { get; set; } = TripleBoolean.NotSet;
    [StatCollectionFloat]
    public float SpreadRange { get; set; } = 0f;
    public float SpreadRangeStat(Weapon weapon) { return weapon.GetStatisticFloat(nameof(SpreadRange)); }
    public float SpreadRangeMod(Weapon weapon) { return weapon.GetStatisticMod(nameof(SpreadRange)); }
    public TripleBoolean NotUseInMelee { get; set; } = TripleBoolean.NotSet;
    [SelfDocumentationTypeName("dictionary of string => WeaponMode"), SelfDocumentationDefaultValue("empty")]
    public Dictionary<string, WeaponMode> Modes { get; set; } = new Dictionary<string, WeaponMode>();
    [JsonIgnore, SelfDocumentationTypeName("string, id from BattleTech.AmmoCategoryEnumeration or CustomAmmo"), SelfDocumentationDefaultValue("NotSet")]
    public CustomAmmoCategory AmmoCategory { get; set; } = new CustomAmmoCategory();
    public TripleBoolean DisableClustering { get; set; } = TripleBoolean.NotSet;
    public TripleBoolean AOECapable { get; set; } = TripleBoolean.NotSet;
    public float AOERange { get; set; } = 0f;
    public float AOEDamage { get; set; } = 0f;
    public float AOEHeatDamage { get; set; } = 0f;
    public float AOEInstability { get; set; } = 0f;
    public string IFFDef { get; set; } = string.Empty;
    public TripleBoolean HasShells { get; set; } = TripleBoolean.NotSet;
    public float ShellsRadius { get; set; } = 0f;
    public float MinShellsDistance { get; set; } = 30f;
    public float MaxShellsDistance { get; set; } = 30f;
    public TripleBoolean Unguided { get; set; } = TripleBoolean.NotSet;
    public TripleBoolean AOEEffectsFalloff { get; set; } = TripleBoolean.NotSet;
    public float ArmorDamageModifier { get; set; } = 1f;
    public float ISDamageModifier { get; set; } = 1f;
    public float FireTerrainChance { get; set; } = 0f;
    public int FireDurationWithoutForest { get; set; } = 0;
    public int FireTerrainStrength { get; set; } = 0;
    public int FireTerrainCellRadius { get; set; } = 0;
    public string AdditionalImpactVFX { get; set; } = string.Empty;
    public TripleBoolean FireOnSuccessHit { get; set; } = TripleBoolean.NotSet;
    public float AdditionalImpactVFXScaleX { get; set; } = 1f;
    public float AdditionalImpactVFXScaleY { get; set; } = 1f;
    public float AdditionalImpactVFXScaleZ { get; set; } = 1f;
    public int ClearMineFieldRadius { get; set; } = 0;
    public int Cooldown { get; set; } = 0;
    [SelfDocumentationDefaultValue("ImprovedBallisticByDefault value from settings")]
    public bool ImprovedBallistic { get; set; } = CustomAmmoCategories.Settings.ImprovedBallisticByDefault;
    public TripleBoolean BallisticDamagePerPallet { get; set; } = TripleBoolean.NotSet;
    public TripleBoolean StatusEffectsPerHit { get; set; } = TripleBoolean.NotSet;
    public string AdditionalAudioEffect { get; set; } = string.Empty;
    public float FireDelayMultiplier { get; set; } = 10f;
    public float MissileFiringIntervalMultiplier { get; set; } = 1f;
    public float MissileVolleyIntervalMultiplier { get; set; } = 1f;
    public float ProjectileSpeedMultiplier { get; set; } = 1f;
    public TripleBoolean CantHitUnaffecedByPathing { get; set; } = TripleBoolean.NotSet;
    public int MissileVolleySize { get; set; } = 0;
    [SelfDocumentationDefaultValue("{ \"x\": 1.0,\"y\":1.0, \"z\":1.0 }"), SelfDocumentationTypeName("Vector")]
    public CustomVector ProjectileScale { get; set; } = new CustomVector(true);
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("array of { \"I\": <float intensivity>, \"C\":\"<html color string>\"}")]
    public List<ColorTableJsonEntry> ColorsTable { get; set; } = new List<ColorTableJsonEntry>();
    public float ColorSpeedChange { get; set; } = 0f;
    public ColorChangeRule ColorChangeRule { get; set; } = ColorChangeRule.None;
    [SelfDocumentationDefaultValue("undefined")]
    public float APCriticalChanceMultiplier { get; set; } = float.NaN;
    public float APArmorShardsMod { get; set; } = 0f;
    public float APMaxArmorThickness { get; set; } = 0f;
    public TripleBoolean DamageNotDivided { get; set; } = TripleBoolean.NotSet;
    public TripleBoolean isHeatVariation { get; set; } = TripleBoolean.NotSet;
    public TripleBoolean isStabilityVariation { get; set; } = TripleBoolean.NotSet;
    public TripleBoolean isDamageVariation { get; set; } = TripleBoolean.NotSet;
    public float DistantVariance { get; set; } = 0f;
    public TripleBoolean DistantVarianceReversed { get; set; } = TripleBoolean.NotSet;
    public float PrefireAnimationSpeedMod { get; set; } = 1f;
    public float FireAnimationSpeedMod { get; set; } = 1f;
    public TripleBoolean AlwaysIndirectVisuals { get; set; } = TripleBoolean.NotSet;
    public float ForbiddenRange { get; set; } = 0f;
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("EvasivePipsMods structure")]
    public EvasivePipsMods evasivePipsMods { get; set; } = new EvasivePipsMods();
    public float ShotsPerAmmo { get; set; } = 1f;
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("DeferredEffectDef structure")]
    public DeferredEffectDef deferredEffect { get; set; } = new DeferredEffectDef();
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("Dictionary of {\"<ammo id>\":<integer internal ammo amount>}")]
    public Dictionary<string, int> InternalAmmo { get; set; } = new Dictionary<string, int>();
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("List of mech chassis locations")]
    public List<ChassisLocations> blockWeaponsInMechLocations { get; set; } = new List<ChassisLocations>();
    public bool blockWeaponsInInstalledLocation { get; set; } = false;
    public bool CanBeBlocked { get; set; } = true;
    public bool EjectWeapon { get; set; } = false;
    [JsonIgnore, SkipDocumentation]
    public bool isHaveInternalAmmo { get {
        foreach (var ia in InternalAmmo) { if (ia.Value > 0) { return true; }; }
        return false;
    } }
    public string preFireSFX { get; set; } = string.Empty;
    [StatCollectionFloat()]
    public float MinMissRadius { get; set; } = 0f;
    [StatCollectionFloat()]
    public float MaxMissRadius { get; set; } = 0f;
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("Dictionary of {\"<tag name>\":<float modifier>}")]
    public Dictionary<string, float> TagsAccuracyModifiers { get; set; } = new Dictionary<string, float>();
    [StatCollectionFloat]
    public float AMSDamage { get; set; } = 0f;
    [StatCollectionFloat]
    public float MissileHealth { get; set; } = 1f;
    [SelfDocumentationDefaultValue("Linear")]
    public DamageFalloffType RangedDmgFalloffType { get; set; } = DamageFalloffType.NotSet;
    public DamageFalloffType AoEDmgFalloffType { get; set; } = DamageFalloffType.NotSet;
    public float DamageFalloffStartDistance { get; set; } = 0f;
    public float DamageFalloffEndDistance { get; set; } = 0f;
    public float BuildingsDamageModifier { get; set; } = 1f;
    public float TurretDamageModifier { get; set; } = 1f;
    public float VehicleDamageModifier { get; set; } = 1f;
    public float VTOLDamageModifier { get; set; } = 1f;
    public float MechDamageModifier { get; set; } = 1f;
    public float QuadDamageModifier { get; set; } = 1f;
    public float TrooperSquadDamageModifier { get; set; } = 1f;
    public float AirMechDamageModifier { get; set; } = 1f;
    public TripleBoolean TargetMechLegsOnly { get; set; } = TripleBoolean.NotSet;
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
      Log.LogWrite("WeaponDef fromJSON ");
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
        CustomAmmoCategoriesLog.Log.LogWrite("\n--------------RESULT----------------\n" + JsonConvert.SerializeObject(extDef,Formatting.Indented) + "\n----------------------------------\n");
        json = defTemp.ToString();
        __state.extDef = extDef;
      }catch(Exception e) {
        __state.errorStr = e.ToString();
      }
      return true;
    }
    public static void Postfix(WeaponDef __instance, ref ExtDefinitionParceInfo __state) {
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
