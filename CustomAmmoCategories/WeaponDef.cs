using BattleTech;
using CustAmmoCategories;
using CustomAmmoCategoriesLog;
using Harmony;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CustAmmoCategories {
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
    public static float APDamage(this Weapon weapon) {
      ExtAmmunitionDef ammo = weapon.ammo();
      WeaponMode mode = weapon.mode();
      ExtWeaponDef wp = weapon.exDef();
      return mode.APDamage + ammo.APDamage + wp.APDamage;
    }
    public static float APArmorShardsMod(this Weapon weapon) {
      ExtAmmunitionDef ammo = weapon.ammo();
      WeaponMode mode = weapon.mode();
      ExtWeaponDef wp = weapon.exDef();
      return mode.APArmorShardsMod + ammo.APArmorShardsMod + wp.APArmorShardsMod;
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
      return (float.IsNaN(mode.APCriticalChanceMultiplier)?0f: mode.APCriticalChanceMultiplier) 
        + (float.IsNaN(ammo.APCriticalChanceMultiplier)?0f: ammo.APCriticalChanceMultiplier)
        + (float.IsNaN(wp.APCriticalChanceMultiplier)?0f: wp.APCriticalChanceMultiplier);
    }
    public static bool DamageNotDivided(this Weapon weapon) {
      ExtAmmunitionDef ammo = weapon.ammo();
      WeaponMode mode = weapon.mode();
      ExtWeaponDef wp = weapon.exDef();
      if (mode.DamageNotDivided != TripleBoolean.NotSet) { return mode.DamageNotDivided == TripleBoolean.True; }
      if (ammo.DamageNotDivided != TripleBoolean.NotSet) { return ammo.DamageNotDivided == TripleBoolean.True; }
      return wp.DamageNotDivided == TripleBoolean.True;
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
  public class ExtWeaponDef {
    public static void RemoveTag(ref JObject json,string tag) {
      Log.LogWrite("ExtWeaponDef.RemoveTag("+tag+")\n");
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
      for(int t = 0; t < items.Count;) {
        Log.LogWrite(" tag:"+ items[t] + "\n");
        if (((string)items[t]) != tag) { ++t; } else {items.RemoveAt(t); Log.LogWrite("  removed\n");};
      }
    }
    public string Id { get; set; }
    public HitGeneratorType HitGenerator { get; set; }
    public bool StreakEffect { get; set; }
    public float DirectFireModifier { get; set; }
    public string baseModeId { get; set; }
    public float FlatJammingChance { get; set; }
    public float AMSHitChance { get; set; }
    public float GunneryJammingBase { get; set; }
    public float GunneryJammingMult { get; set; }
    public TripleBoolean DamageOnJamming { get; set; }
    public TripleBoolean DestroyOnJamming { get; set; }
    public TripleBoolean AMSImmune { get; set; }
    public bool AlternateDamageCalc { get; set; }
    public bool AlternateHeatDamageCalc { get; set; }
    public bool AlternateInstabilityCalc { get; set; }
    public TripleBoolean IsAMS { get; set; }
    public TripleBoolean IsAAMS { get; set; }
    public bool AMSShootsEveryAttack { get; set; }
    public float SpreadRange { get; set; }
    public TripleBoolean NotUseInMelee { get; set; }
    public Dictionary<string, WeaponMode> Modes { get; set; }
    public CustomAmmoCategory AmmoCategory { get; set; }
    public TripleBoolean DisableClustering { get; set; }
    public TripleBoolean AOECapable { get; set; }
    public float AOERange { get; set; }
    public float AOEDamage { get; set; }
    public float AOEHeatDamage { get; set; }
    public float AOEInstability { get; set; }
    public string IFFDef { get; set; }
    //public string ShrapnelWeaponEffectID { get; set; }
    public TripleBoolean HasShells { get; set; }
    public float ShellsRadius { get; set; }
    public float MinShellsDistance { get; set; }
    public float MaxShellsDistance { get; set; }
    public TripleBoolean Unguided { get; set; }
    public float ArmorDamageModifier { get; set; }
    public float ISDamageModifier { get; set; }
    //public TripleBoolean AOECapable { get; set; }
    public float FireTerrainChance { get; set; }
    public int FireDurationWithoutForest { get; set; }
    public int FireTerrainStrength { get; set; }
    public int FireTerrainCellRadius { get; set; }
    public string AdditionalImpactVFX { get; set; }
    public TripleBoolean FireOnSuccessHit { get; set; }
    public float AdditionalImpactVFXScaleX { get; set; }
    public float AdditionalImpactVFXScaleY { get; set; }
    public float AdditionalImpactVFXScaleZ { get; set; }
    public int ClearMineFieldRadius { get; set; }
    public int Cooldown { get; set; }
    public bool ImprovedBallistic { get; set; }
    public TripleBoolean BallisticDamagePerPallet { get; set; }
    public TripleBoolean StatusEffectsPerHit { get; set; }
    public CustomAudioSource AdditionalAudioEffect { get; set; }
    public float FireDelayMultiplier { get; set; }
    public float MissileFiringIntervalMultiplier { get; set; }
    public float MissileVolleyIntervalMultiplier { get; set; }
    public float ProjectileSpeedMultiplier { get; set; }
    public TripleBoolean CantHitUnaffecedByPathing { get; set; }
    public int MissileVolleySize { get; set; }
    public CustomVector ProjectileScale { get; set; }
    //public CustomVector MissileExplosionScale { get; set; }
    public List<ColorTableJsonEntry> ColorsTable { get; set; }
    public float ColorSpeedChange { get; set; }
    public ColorChangeRule ColorChangeRule { get; set; }
    public float APDamage { get; set; }
    public float APCriticalChanceMultiplier { get; set; }
    public float APArmorShardsMod { get; set; }
    public float APMaxArmorThickness { get; set; }
    public TripleBoolean DamageNotDivided { get; set; }
    public TripleBoolean isHeatVariation { get; set; }
    public TripleBoolean isStabilityVariation { get; set; }
    public TripleBoolean isDamageVariation { get; set; }
    public float DistantVariance { get; set; }
    public TripleBoolean DistantVarianceReversed { get; set; }
    public ExtWeaponDef() {
      Id = string.Empty;
      StreakEffect = false;
      HitGenerator = HitGeneratorType.NotSet;
      DirectFireModifier = 0;
      FlatJammingChance = 0;
      GunneryJammingBase = 0;
      GunneryJammingMult = 0;
      AMSHitChance = 0f;
      SpreadRange = 0f;
      AlternateDamageCalc = false;
      AMSImmune = TripleBoolean.NotSet;
      //AOECapable = TripleBoolean.NotSet;
      IsAMS = TripleBoolean.NotSet;
      IsAAMS = TripleBoolean.NotSet;
      AlternateHeatDamageCalc = false;
      AlternateInstabilityCalc = false;
      AMSShootsEveryAttack = false;
      baseModeId = WeaponMode.NONE_MODE_NAME;
      DisableClustering = TripleBoolean.True;
      NotUseInMelee = TripleBoolean.NotSet;
      DamageOnJamming = TripleBoolean.NotSet;
      DestroyOnJamming = TripleBoolean.NotSet;
      AmmoCategory = new CustomAmmoCategory();
      AOECapable = TripleBoolean.NotSet;
      AOERange = 0f;
      AOEInstability = 0f;
      AOEHeatDamage = 0f;
      IFFDef = "";
      //ShrapnelWeaponEffectID = "";
      HasShells = TripleBoolean.NotSet;
      ShellsRadius = 0f;
      MinShellsDistance = 30f;
      MaxShellsDistance = 30f;
      Unguided = TripleBoolean.NotSet;
      Modes = new Dictionary<string, WeaponMode>();
      ArmorDamageModifier = 1f;
      ISDamageModifier = 1f;
      FireTerrainChance = 0f;
      FireDurationWithoutForest = 0;
      FireTerrainStrength = 0;
      FireOnSuccessHit = TripleBoolean.NotSet;
      FireTerrainCellRadius = 0;
      AdditionalImpactVFXScaleX = 1f;
      AdditionalImpactVFXScaleY = 1f;
      AdditionalImpactVFXScaleZ = 1f;
      AdditionalImpactVFX = string.Empty;
      ClearMineFieldRadius = 0;
      Cooldown = 0;
      ImprovedBallistic = false;
      BallisticDamagePerPallet = TripleBoolean.NotSet;
      StatusEffectsPerHit = TripleBoolean.NotSet;
      AdditionalAudioEffect = null;
      MissileFiringIntervalMultiplier = 1f;
      MissileVolleyIntervalMultiplier = 1f;
      ProjectileSpeedMultiplier = 1f;
      FireDelayMultiplier = 10f;
      CantHitUnaffecedByPathing = TripleBoolean.NotSet;
      MissileVolleySize = 0;
      ProjectileScale = new CustomVector(true);
      //MissileExplosionScale = new CustomVector(true);
      ColorsTable = new List<ColorTableJsonEntry>();
      ColorSpeedChange = 0f;
      ColorChangeRule = ColorChangeRule.None;
      APDamage = 0f;
      APCriticalChanceMultiplier = float.NaN;
      APArmorShardsMod = 0f;
      APMaxArmorThickness = 0f;
      DamageNotDivided = TripleBoolean.NotSet;
      isHeatVariation = TripleBoolean.NotSet;
      isDamageVariation = TripleBoolean.True;
      isStabilityVariation = TripleBoolean.NotSet;
      DistantVariance = 0f;
      DistantVarianceReversed = TripleBoolean.NotSet;
    }
  }
}

namespace CustomAmmoCategoriesPatches {
  [HarmonyPatch(typeof(WeaponDef))]
  [HarmonyPatch("FromJSON")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class BattleTech_WeaponDef_fromJSON_Patch {
    public static bool Prefix(WeaponDef __instance, ref string json) {
      CustomAmmoCategoriesLog.Log.LogWrite("WeaponDef fromJSON ");
      JObject defTemp = JObject.Parse(json);
      string Id = (string)defTemp["Description"]["Id"];
      Log.LogWrite(Id+"\n");
      ExtWeaponDef extDef = null;
      if (CustomAmmoCategories.isRegistredWeapon(Id)) {
        extDef = CustomAmmoCategories.getExtWeaponDef(Id);
      }
      CustomAmmoCategory custCat = CustomAmmoCategories.find((string)defTemp["AmmoCategory"]);
      //CustomAmmoCategories.RegisterWeapon((string)defTemp["Description"]["Id"], custCat);
      if (extDef == null) {
        extDef = new ExtWeaponDef();
        extDef.Id = Id;
        extDef.AmmoCategory = custCat;
      };
      if (defTemp["Streak"] != null) {
        extDef.StreakEffect = (bool)defTemp["Streak"];
        defTemp.Remove("Streak");
      }
      if (defTemp["HitGenerator"] != null) {
        try {
          extDef.HitGenerator = (HitGeneratorType)Enum.Parse(typeof(HitGeneratorType), (string)defTemp["HitGenerator"], true);
          CustomAmmoCategoriesLog.Log.LogWrite("HitGenerator is "+ extDef.HitGenerator + "\n");
        } catch (Exception) {
          extDef.HitGenerator = HitGeneratorType.NotSet;
          CustomAmmoCategoriesLog.Log.LogWrite("Can't parce " + (string)defTemp["HitGenerator"] + " as HitGenerator\n");
        }
        defTemp.Remove("HitGenerator");
      }
      if (defTemp["FlatJammingChance"] != null) {
        extDef.FlatJammingChance = (float)defTemp["FlatJammingChance"];
        defTemp.Remove("FlatJammingChance");
      }
      if (defTemp["FireTerrainChance"] != null) {
        extDef.FireTerrainChance = (float)defTemp["FireTerrainChance"];
        defTemp.Remove("FireTerrainChance");
      }
      if (defTemp["FireDurationWithoutForest"] != null) {
        extDef.FireDurationWithoutForest = (int)defTemp["FireDurationWithoutForest"];
        defTemp.Remove("FireDurationWithoutForest");
      }
      if (defTemp["FireTerrainStrength"] != null) {
        extDef.FireTerrainStrength = (int)defTemp["FireTerrainStrength"];
        defTemp.Remove("FireTerrainStrength");
      }
      if (defTemp["FireOnSuccessHit"] != null) {
        extDef.FireOnSuccessHit = ((bool)defTemp["FireOnSuccessHit"] == true) ? TripleBoolean.True : TripleBoolean.False;
        defTemp.Remove("FireOnSuccessHit");
      }
      if (defTemp["DistantVarianceReversed"] != null) {
        extDef.DistantVarianceReversed = ((bool)defTemp["DistantVarianceReversed"] == true) ? TripleBoolean.True : TripleBoolean.False;
        defTemp.Remove("DistantVarianceReversed");
      }
      if (defTemp["GunneryJammingBase"] != null) {
        extDef.GunneryJammingBase = (float)defTemp["GunneryJammingBase"];
        defTemp.Remove("GunneryJammingBase");
      }
      if (defTemp["FireDelayMultiplier"] != null) {
        extDef.FireDelayMultiplier = (float)defTemp["FireDelayMultiplier"];
        defTemp.Remove("FireDelayMultiplier");
      }
      if (defTemp["MissileFiringIntervalMultiplier"] != null) {
        extDef.MissileFiringIntervalMultiplier = (float)defTemp["MissileFiringIntervalMultiplier"];
        defTemp.Remove("MissileFiringIntervalMultiplier");
      }
      if (defTemp["MissileVolleyIntervalMultiplier"] != null) {
        extDef.MissileVolleyIntervalMultiplier = (float)defTemp["MissileVolleyIntervalMultiplier"];
        defTemp.Remove("MissileVolleyIntervalMultiplier");
      }
      if (defTemp["ProjectileSpeedMultiplier"] != null) {
        extDef.ProjectileSpeedMultiplier = (float)defTemp["ProjectileSpeedMultiplier"];
        defTemp.Remove("ProjectileSpeedMultiplier");
      }
      if (defTemp["GunneryJammingMult"] != null) {
        extDef.GunneryJammingMult = (float)defTemp["GunneryJammingMult"];
        defTemp.Remove("GunneryJammingMult");
      }
      if (defTemp["DirectFireModifier"] != null) {
        extDef.DirectFireModifier = (float)defTemp["DirectFireModifier"];
        defTemp.Remove("DirectFireModifier");
      }
      if (defTemp["SpreadRange"] != null) {
        extDef.SpreadRange = (float)defTemp["SpreadRange"];
        defTemp.Remove("SpreadRange");
      }
      if (defTemp["ArmorDamageModifier"] != null) {
        extDef.ArmorDamageModifier = (float)defTemp["ArmorDamageModifier"];
        defTemp.Remove("ArmorDamageModifier");
      }
      if (defTemp["ISDamageModifier"] != null) {
        extDef.ISDamageModifier = (float)defTemp["ISDamageModifier"];
        defTemp.Remove("ISDamageModifier");
      }
      if (defTemp["AlternateDamageCalc"] != null) {
        extDef.AlternateDamageCalc = (bool)defTemp["AlternateDamageCalc"];
        defTemp.Remove("AlternateDamageCalc");
      }
      if (defTemp["MissileVolleySize"] != null) {
        extDef.MissileVolleySize = (int)defTemp["MissileVolleySize"];
        defTemp.Remove("MissileVolleySize");
      }
      if (defTemp["AMSShootsEveryAttack"] != null) {
        extDef.AMSShootsEveryAttack = (bool)defTemp["AMSShootsEveryAttack"];
        defTemp.Remove("AMSShootsEveryAttack");
      }
      if (defTemp["APDamage"] != null) {
        extDef.APDamage = (float)defTemp["APDamage"];
        defTemp.Remove("APDamage");
      }
      if (defTemp["APCriticalChanceMultiplier"] != null) {
        extDef.APCriticalChanceMultiplier = (float)defTemp["APCriticalChanceMultiplier"];
        defTemp.Remove("APCriticalChanceMultiplier");
      }
      if (defTemp["APArmorShardsMod"] != null) {
        extDef.APArmorShardsMod = (float)defTemp["APArmorShardsMod"];
        defTemp.Remove("APArmorShardsMod");
      }
      if (defTemp["APMaxArmorThickness"] != null) {
        extDef.APMaxArmorThickness = (float)defTemp["APMaxArmorThickness"];
        defTemp.Remove("APMaxArmorThickness");
      }
      if (defTemp["AdditionalImpactVFXScaleX"] != null) {
        extDef.AdditionalImpactVFXScaleX = (float)defTemp["AdditionalImpactVFXScaleX"];
        defTemp.Remove("AdditionalImpactVFXScaleX");
      }
      if (defTemp["AdditionalImpactVFXScaleY"] != null) {
        extDef.AdditionalImpactVFXScaleY = (float)defTemp["AdditionalImpactVFXScaleY"];
        defTemp.Remove("AdditionalImpactVFXScaleY");
      }
      if (defTemp["ProjectileScale"] != null) {
        extDef.ProjectileScale = defTemp["ProjectileScale"].ToObject<CustomVector>();
        defTemp.Remove("ProjectileScale");
      }
      if (defTemp["DamageNotDivided"] != null) {
        extDef.DamageNotDivided = ((bool)defTemp["DamageNotDivided"] == true) ? TripleBoolean.True : TripleBoolean.False;
        defTemp.Remove("DamageNotDivided");
      }
      if (defTemp["isDamageVariation"] != null) {
        extDef.isDamageVariation = ((bool)defTemp["isDamageVariation"] == true) ? TripleBoolean.True : TripleBoolean.False;
        defTemp.Remove("isDamageVariation");
      }
      if (defTemp["isHeatVariation"] != null) {
        extDef.isHeatVariation = ((bool)defTemp["isHeatVariation"] == true) ? TripleBoolean.True : TripleBoolean.False;
        defTemp.Remove("isHeatVariation");
      }
      if (defTemp["isStabilityVariation"] != null) {
        extDef.isStabilityVariation = ((bool)defTemp["isStabilityVariation"] == true) ? TripleBoolean.True : TripleBoolean.False;
        defTemp.Remove("isStabilityVariation");
      }
      //if (defTemp["MissileExplosionScale"] != null) {
      //extDef.MissileExplosionScale = defTemp["MissileExplosionScale"].ToObject<CustomVector>();
      //defTemp.Remove("MissileExplosionScale");
      //}
      if (defTemp["DistantVariance"] != null) {
        extDef.DistantVariance = (float)defTemp["DistantVariance"];
        defTemp.Remove("DistantVariance");
      }
      Log.M.WL(1, "extDef.DistantVariance:" + extDef.DistantVariance);
      if (defTemp["ColorsTable"] != null) {
        extDef.ColorsTable = defTemp["ColorsTable"].ToObject<List<ColorTableJsonEntry>>();
        defTemp.Remove("ColorsTable");
      }
      if (defTemp["ColorSpeedChange"] != null) {
        extDef.ColorSpeedChange = (float)defTemp["ColorSpeedChange"];
        defTemp.Remove("ColorSpeedChange");
      }
      if (defTemp["ColorChangeRule"] != null) {
        extDef.ColorChangeRule = (ColorChangeRule)Enum.Parse(typeof(ColorChangeRule),(string)defTemp["ColorChangeRule"]);
        defTemp.Remove("ColorChangeRule");
      }
      if (defTemp["AdditionalImpactVFXScaleZ"] != null) {
        extDef.AdditionalImpactVFXScaleZ = (float)defTemp["AdditionalImpactVFXScaleZ"];
        defTemp.Remove("AdditionalImpactVFXScaleZ");
      }
      if (defTemp["AlternateHeatDamageCalc"] != null) {
        extDef.AlternateHeatDamageCalc = (bool)defTemp["AlternateHeatDamageCalc"];
        defTemp.Remove("AlternateHeatDamageCalc");
      }
      if (defTemp["AlternateInstabilityCalc"] != null) {
        extDef.AlternateInstabilityCalc = (bool)defTemp["AlternateInstabilityCalc"];
        defTemp.Remove("AlternateInstabilityCalc");
      }
      if (defTemp["FireTerrainCellRadius"] != null) {
        extDef.FireTerrainCellRadius = (int)defTemp["FireTerrainCellRadius"];
        defTemp.Remove("FireTerrainCellRadius");
      }
      if (defTemp["ClearMineFieldRadius"] != null) {
        extDef.ClearMineFieldRadius = (int)defTemp["ClearMineFieldRadius"];
        defTemp.Remove("ClearMineFieldRadius");
      }
      if (defTemp["Cooldown"] != null) {
        extDef.Cooldown = (int)defTemp["Cooldown"];
        defTemp.Remove("Cooldown");
      }
      if (defTemp["AdditionalImpactVFX"] != null) {
        extDef.AdditionalImpactVFX = (string)defTemp["AdditionalImpactVFX"];
        defTemp.Remove("AdditionalImpactVFX");
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
      if (defTemp["AMSHitChance"] != null) {
        extDef.AMSHitChance = (float)defTemp["AMSHitChance"];
        defTemp.Remove("AMSHitChance");
      }
      if (defTemp["DisableClustering"] != null) {
        extDef.DisableClustering = ((bool)defTemp["DisableClustering"] == true) ? TripleBoolean.True : TripleBoolean.False;
        defTemp.Remove("DisableClustering");
      }
      if (defTemp["AOECapable"] != null) {
        extDef.AOECapable = ((bool)defTemp["AOECapable"] == true) ? TripleBoolean.True : TripleBoolean.False;
      }
      if (defTemp["HasShells"] != null) {
        extDef.HasShells = ((bool)defTemp["HasShells"] == true) ? TripleBoolean.True : TripleBoolean.False;
        defTemp.Remove("HasShells");
      }
      if (defTemp["CantHitUnaffecedByPathing"] != null) {
        extDef.CantHitUnaffecedByPathing = ((bool)defTemp["CantHitUnaffecedByPathing"] == true) ? TripleBoolean.True : TripleBoolean.False;
        defTemp.Remove("CantHitUnaffecedByPathing");
      }
      if (defTemp["ImprovedBallistic"] != null) {
        extDef.ImprovedBallistic = (bool)defTemp["ImprovedBallistic"];
        if (extDef.ImprovedBallistic) {
          ExtWeaponDef.RemoveTag(ref defTemp, "wr-clustered_shots");
          extDef.DisableClustering = TripleBoolean.True;
        }
        defTemp.Remove("ImprovedBallistic");
      }
      if (defTemp["BallisticDamagePerPallet"] != null) {
        extDef.BallisticDamagePerPallet = ((bool)defTemp["BallisticDamagePerPallet"] == true) ? TripleBoolean.True : TripleBoolean.False;
        defTemp.Remove("BallisticDamagePerPallet");
      }
      if (defTemp["StatusEffectsPerHit"] != null) {
        extDef.StatusEffectsPerHit = ((bool)defTemp["StatusEffectsPerHit"] == true) ? TripleBoolean.True : TripleBoolean.False;
        defTemp.Remove("StatusEffectsPerHit");
      }
      if (defTemp["MinShellsDistance"] != null) {
        extDef.MinShellsDistance = (float)defTemp["MinShellsDistance"];
        defTemp.Remove("MinShellsDistance");
      }
      if (defTemp["MaxShellsDistance"] != null) {
        extDef.MaxShellsDistance = (float)defTemp["MaxShellsDistance"];
        defTemp.Remove("MaxShellsDistance");
      }
      if (defTemp["ShellsRadius"] != null) {
        extDef.ShellsRadius = (float)defTemp["ShellsRadius"];
        defTemp.Remove("ShellsRadius");
      }
      if (defTemp["MinShellsDistance"] != null) {
        extDef.ShellsRadius = (float)defTemp["ShellsRadius"];
        defTemp.Remove("ShellsRadius");
      }
      if (defTemp["AOERange"] != null) {
        extDef.AOERange = (float)defTemp["AOERange"];
        defTemp.Remove("AOERange");
      }
      if (defTemp["AOEDamage"] != null) {
        extDef.AOEDamage = (float)defTemp["AOEDamage"];
        defTemp.Remove("AOEDamage");
      }
      if (defTemp["AOEHeatDamage"] != null) {
        extDef.AOEHeatDamage = (float)defTemp["AOEHeatDamage"];
        defTemp.Remove("AOEHeatDamage");
      }
      if (defTemp["AOEInstability"] != null) {
        extDef.AOEInstability = (float)defTemp["AOEInstability"];
        defTemp.Remove("AOEInstability");
      }
      if (defTemp["Unguided"] != null) {
        extDef.Unguided = ((bool)defTemp["Unguided"] == true) ? TripleBoolean.True : TripleBoolean.False;
        defTemp.Remove("Unguided");
      }
      if (defTemp["NotUseInMelee"] != null) {
        extDef.NotUseInMelee = ((bool)defTemp["NotUseInMelee"] == true) ? TripleBoolean.True : TripleBoolean.False;
        defTemp.Remove("NotUseInMelee");
      }
      if (defTemp["AdditionalAudioEffect"] != null) {
        extDef.AdditionalAudioEffect = new CustomAudioSource((string)defTemp["AdditionalAudioEffect"]);
        defTemp.Remove("AdditionalAudioEffect");
      }
      if (defTemp["DamageOnJamming"] != null) {
        extDef.DamageOnJamming = ((bool)defTemp["DamageOnJamming"] == true) ? TripleBoolean.True : TripleBoolean.False;
        defTemp.Remove("DamageOnJamming");
      }
      if (defTemp["DestroyOnJamming"] != null) {
        extDef.DestroyOnJamming = ((bool)defTemp["DestroyOnJamming"] == true) ? TripleBoolean.True : TripleBoolean.False;
        defTemp.Remove("DestroyOnJamming");
      }
      if (defTemp["AMSImmune"] != null) {
        extDef.AMSImmune = ((bool)defTemp["AMSImmune"] == true) ? TripleBoolean.True : TripleBoolean.False;
        defTemp.Remove("AMSImmune");
      }
      if (defTemp["IFFDef"] != null) {
        extDef.IFFDef = (string)defTemp["IFFDef"];
        defTemp.Remove("IFFDef");
      }
      //if (defTemp["ShrapnelWeaponEffectID"] != null) {
      //  extDef.ShrapnelWeaponEffectID = (string)defTemp["ShrapnelWeaponEffectID"];
      //  defTemp.Remove("ShrapnelWeaponEffectID");
      //}
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
      CustomAmmoCategories.registerExtWeaponDef((string)defTemp["Description"]["Id"], extDef);
      defTemp["AmmoCategory"] = custCat.BaseCategory.ToString();
      CustomAmmoCategoriesLog.Log.LogWrite("\n--------------ORIG----------------\n" + json + "\n----------------------------------\n");
      //CustomAmmoCategoriesLog.Log.LogWrite("\n--------------MOD----------------\n" + defTemp.ToString() + "\n----------------------------------\n");
      json = defTemp.ToString();
      return true;
    }
    public static void Postfix(WeaponDef __instance) {
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
    }
  }
}
