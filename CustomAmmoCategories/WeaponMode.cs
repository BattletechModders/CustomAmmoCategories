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

//namespace CustAmmoCategoriesPatches {
//  [HarmonyPatch(typeof(Mech))]
//  [HarmonyPatch("ApplyHeatSinks")]
//  [HarmonyPatch(MethodType.Normal)]
//  [HarmonyPatch(new Type[] { typeof(int) })]
//  public static class Mech_ApplyHeatSinksModesLock {
//    public static void Postfix(Mech __instance, int stackID) {
//      Log.M.TWL(0,"Mech.ApplyHeatSinks:" + __instance.DisplayName + ":" + __instance.GUID);
//      foreach(Weapon weapon in __instance.Weapons) {
//        List<WeaponMode> modes = weapon.AvaibleModes();
//        Log.M.WL(1,"avaible modes count:"+modes.Count);
//        if (modes.Count == 0) {
//          Log.M.WL(1, "no modes avaible. disable weapon");
//          weapon.NoModeToFire(true);
//          continue;
//        };
//        Log.M.WL(1, "at least one mode avaible. enable weapon");
//        weapon.NoModeToFire(false);
//        WeaponMode mode = weapon.mode();
//        if (mode.Lock.isAvaible(weapon) == false) {
//          Log.M.WL(1, "current mode:"+mode.Id+" not avaible. Cycling.");
//          CustomAmmoCategories.CycleMode(weapon);
//        }
//      }
//    }
//  }
//};

namespace CustAmmoCategories {
  public static partial class CustomAmmoCategories {
    //public static readonly string NoModeToFireStatisticName = "CACNoModeToFire";
    public static List<WeaponMode> AvaibleModes(this Weapon weapon) {
      List<WeaponMode> result = new List<WeaponMode>();
      ExtWeaponDef extWeapon = weapon.exDef();
      foreach(var mode in extWeapon.Modes) {
        if (mode.Value.Lock.isAvaible(weapon)) { result.Add(mode.Value); };
      }
      return result;
    }
    //public static bool NoModeToFire(this Weapon weapon) {
    //  Statistic stat = weapon.StatCollection.GetStatistic(NoModeToFireStatisticName);
    //  if (stat == null) { return false; }
    //  return stat.Value<bool>();
    //}
    //public static void NoModeToFire(this Weapon weapon,bool value) {
    //  if (weapon.StatCollection.ContainsStatistic(NoModeToFireStatisticName) == false) {
    //    weapon.StatCollection.AddStatistic<bool>(NoModeToFireStatisticName, false);
    //  }
    //  weapon.StatCollection.Set<bool>(NoModeToFireStatisticName, value);
    //}
  }
  public class ModeLockSetting {
    public float Low { get; set; }
    public float High { get; set; }
    public ModeLockSetting() {
      Low = float.NaN;
      High = float.NaN;
    }
    public bool isSet() {
      if (float.IsNaN(Low)) { return false; }
      if (float.IsNaN(High)) { return false; }
      if (float.IsInfinity(Low)) { return false; }
      if (float.IsInfinity(High)) { return false; }
      return true;
    }
  }
  public class ModeLockSettings {
    public ModeLockSetting HeatLevel { get; set; }
    public ModeLockSetting OverheatLevel { get; set; }
    public ModeLockSetting MaxheatLevel { get; set; }
    public ModeLockSettings() {
      HeatLevel = new ModeLockSetting();
      OverheatLevel = new ModeLockSetting();
      MaxheatLevel = new ModeLockSetting();
    }
    public bool isAvaible(Weapon weapon) {
      Mech mech = weapon.parent as Mech;
      if (mech == null) { return true; }
      if (HeatLevel.isSet()) {
        if((mech.CurrentHeat < HeatLevel.Low) || (mech.CurrentHeat > HeatLevel.High)){ return false; };
      }
      if (OverheatLevel.isSet()) {
        float level = mech.CurrentHeat / mech.OverheatLevel;
        if ((level < OverheatLevel.Low) || (level > OverheatLevel.High)){ return false; };
      }
      if (MaxheatLevel.isSet()) {
        float level = mech.CurrentHeat / mech.MaxHeat;
        if ((level < MaxheatLevel.Low) || (level > MaxheatLevel.High)) { return false; };
      }
      return true;
    } 
  }
  [SelfDocumentedClass("Weapons", "Weapons", "WeaponMode")]
  public class WeaponMode {
    public static string BASE_MODE_NAME = "B";
    public static string NONE_MODE_NAME = "!NONE!";
    public string UIName { get; set; } = WeaponMode.BASE_MODE_NAME;
    public string Id { get; set; } = WeaponMode.NONE_MODE_NAME;
    public string Name { get; set; } = WeaponMode.BASE_MODE_NAME;
    public string Description { get; set; } = string.Empty;
    public float AccuracyModifier { get; set; } = 0f;
    public float DirectFireModifier { get; set; } = 0f;
    public float DamagePerShot { get; set; } = 0f;
    public float HeatDamagePerShot { get; set; } = 0f;
    public int HeatGenerated { get; set; } = 0;
    public float CriticalChanceMultiplier { get; set; } = 0f;
    public int ShotsWhenFired { get; set; } = 0;
    public float ShotsWhenFiredMod { get; set; } = 1f;
    [SkipDocumentation]
    public int AIBattleValue { get; set; } = 100;
    public int ProjectilesPerShot { get; set; } = 0;
    [SelfDocumentationTypeName("List of statusEffects"), SelfDocumentationDefaultValue("empty")]
    public List<EffectData> statusEffects { get; set; } = new List<EffectData>();
    public float MinRange { get; set; } = 0f;
    public float MaxRange { get; set; } = 0f;
    public float LongRange { get; set; } = 0f;
    public float ShortRange { get; set; } = 0f;
    public float ForbiddenRange { get; set; } = 0f;
    public float MediumRange { get; set; } = 0f;
    public int RefireModifier { get; set; } = 0;
    public int AttackRecoil { get; set; } = 0;
    public int Cooldown { get; set; } = 0;
    [SkipDocumentation]
    public float AIHitChanceCap { get; set; }
    public float Instability { get; set; } = 0f;
    public float FlatJammingChance { get; set; } = 0f;
    public float GunneryJammingBase { get; set; } = 0f;
    public float GunneryJammingMult { get; set; } = 0f;
    public float DistantVariance { get; set; } = 0f;
    public float SpreadRange { get; set; } = 0f;
    public TripleBoolean DistantVarianceReversed { get; set; } = TripleBoolean.NotSet;
    public float DamageVariance { get; set; } = 0f;
    public string WeaponEffectID { get; set; } = string.Empty;
    public float EvasivePipsIgnored { get; set; } = 0f;
    public TripleBoolean IndirectFireCapable { get; set; } = TripleBoolean.NotSet;
    public TripleBoolean DamageOnJamming { get; set; } = TripleBoolean.NotSet;
    public TripleBoolean DestroyOnJamming { get; set; } = TripleBoolean.NotSet;
    public TripleBoolean AOECapable { get; set; } = TripleBoolean.NotSet;
    public TripleBoolean AOEEffectsFalloff { get; set; } = TripleBoolean.NotSet;
    public HitGeneratorType HitGenerator { get; set; } = HitGeneratorType.NotSet;
    public TripleBoolean AlwaysIndirectVisuals { get; set; } = TripleBoolean.NotSet;
    [SkipDocumentation]
    public bool isBaseMode { get; set; } = false;
    public float DamageMultiplier { get; set; } = 1f;
    public float HeatMultiplier { get; set; } = 1f;
    public float InstabilityMultiplier { get; set; } = 1f;
    public float AMSHitChance { get; set; } = 0f;
    [JsonIgnore, SelfDocumentationTypeName("string, id from BattleTech.AmmoCategoryEnumeration or CustomAmmo"), SelfDocumentationDefaultValue("empty")]
    public CustomAmmoCategory AmmoCategory { get; set; } = null;
    public string IFFDef { get; set; } = string.Empty;
    public TripleBoolean IsAMS { get; set; } = TripleBoolean.NotSet;
    public TripleBoolean IsAAMS { get; set; } = TripleBoolean.NotSet;
    public TripleBoolean HasShells { get; set; } = TripleBoolean.NotSet;
    public float ShellsRadius { get; set; } = 0f;
    public float MinShellsDistance { get; set; } = 30f;
    public float MaxShellsDistance { get; set; } = 30f;
    public TripleBoolean Unguided { get; set; } = TripleBoolean.NotSet;
    public float ArmorDamageModifier { get; set; } = 1f;
    public float ISDamageModifier { get; set; } = 1f;
    public float FireTerrainChance { get; set; } = 0f;
    public int FireDurationWithoutForest { get; set; } = 0;
    public int FireTerrainStrength { get; set; } = 0;
    public TripleBoolean FireOnSuccessHit { get; set; } = TripleBoolean.NotSet;
    public int FireTerrainCellRadius { get; set; } = 0;
    public string AdditionalImpactVFX { get; set; } = string.Empty;
    public float AdditionalImpactVFXScaleX { get; set; } = 1f;
    public float AdditionalImpactVFXScaleY { get; set; } = 1f;
    public float AdditionalImpactVFXScaleZ { get; set; } = 1f;
    public int ClearMineFieldRadius { get; set; } = 0;
    public TripleBoolean BallisticDamagePerPallet { get; set; } = TripleBoolean.NotSet;
    public string AdditionalAudioEffect { get; set; } = string.Empty;
    public TripleBoolean Streak { get; set; } = TripleBoolean.NotSet;
    public float FireDelayMultiplier { get; set; } = 1f;
    public float MissileFiringIntervalMultiplier { get; set; } = 1f;
    public float MissileVolleyIntervalMultiplier { get; set; } = 1f;
    public float ProjectileSpeedMultiplier { get; set; } = 1f;
    public TripleBoolean CantHitUnaffecedByPathing { get; set; } = TripleBoolean.NotSet;
    public int MissileVolleySize { get; set; } = 0;
    [SelfDocumentationDefaultValue("{ \"x\": 1.0,\"y\":1.0, \"z\":1.0 }"), SelfDocumentationTypeName("Vector")]
    public CustomVector ProjectileScale { get; set; } = new CustomVector(true);
    [SelfDocumentationDefaultValue("{ \"x\": 1.0,\"y\":1.0, \"z\":1.0 }"), SelfDocumentationTypeName("Vector")]
    public CustomVector MissileExplosionScale { get; set; } = new CustomVector(true);
    public float ColorSpeedChange { get; set; } = 0f;
    public ColorChangeRule ColorChangeRule { get; set; } = ColorChangeRule.None;
    public float APDamage { get; set; } = 0f;
    public float APDamageMultiplier { get; set; } = 1f;
    [SelfDocumentationDefaultValue("undefined")]
    public float APCriticalChanceMultiplier { get; set; } = float.NaN;
    public float APArmorShardsMod { get; set; } = 0f;
    public float APMaxArmorThickness { get; set; } = 0f;
    public TripleBoolean DamageNotDivided { get; set; } = TripleBoolean.NotSet;
    public TripleBoolean isHeatVariation { get; set; } = TripleBoolean.NotSet;
    public TripleBoolean isStabilityVariation { get; set; } = TripleBoolean.NotSet;
    public TripleBoolean isDamageVariation { get; set; } = TripleBoolean.NotSet;
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("ModeLockSettings")]
    public ModeLockSettings Lock { get; set; } = new ModeLockSettings();
    public float ClusteringModifier { get; set; } = 0f;
    public float PrefireAnimationSpeedMod { get; set; } = 1f;
    public float FireAnimationSpeedMod { get; set; } = 1f;
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("EvasivePipsMods structure")]
    public EvasivePipsMods evasivePipsMods { get; set; } = new EvasivePipsMods();
    public float ShotsPerAmmo { get; set; } = 1f;
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("DeferredEffectDef structure")]
    public DeferredEffectDef deferredEffect { get; set; } = new DeferredEffectDef();
    public string preFireSFX { get; set; } = string.Empty;
    public float MinMissRadius { get; set; } = 0f;
    public float MaxMissRadius { get; set; } = 0f;
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("Dictionary of {\"<tag name>\":<float modifier>}")]
    public Dictionary<string, float> TagsAccuracyModifiers { get; set; } = new Dictionary<string, float>();
    public TripleBoolean AMSImmune { get; set; } = TripleBoolean.NotSet;
    public float AMSDamage { get; set; } = 0f;
    public float MissileHealth { get; set; } = 0f;
    public DamageFalloffType RangedDmgFalloffType { get; set; } = DamageFalloffType.NotSet;
    public DamageFalloffType AoEDmgFalloffType { get; set; } = DamageFalloffType.NotSet;
    public float DamageFalloffStartDistance { get; set; } = 0f;
    public float DamageFalloffEndDistance { get; set; } = 0f;
    public TripleBoolean AMSShootsEveryAttack { get; set; } = TripleBoolean.NotSet;
    public TripleBoolean TargetMechLegsOnly { get; set; } = TripleBoolean.NotSet;
    public float HeatGeneratedModifier { get; set; } = 1f;
    public MeleeAttackType meleeAttackType { get; set; } = MeleeAttackType.NotSet;
    public float BuildingsDamageModifier { get; set; } = 1f;
    public float TurretDamageModifier { get; set; } = 1f;
    public float VehicleDamageModifier { get; set; } = 1f;
    public float VTOLDamageModifier { get; set; } = 1f;
    public float MechDamageModifier { get; set; } = 1f;
    public float QuadDamageModifier { get; set; } = 1f;
    public float TrooperSquadDamageModifier { get; set; } = 1f;
    public float AirMechDamageModifier { get; set; } = 1f;
    public WeaponMode() {
      //Id = WeaponMode.NONE_MODE_NAME;
      //UIName = WeaponMode.BASE_MODE_NAME;
      //AccuracyModifier = 0;
      //DirectFireModifier = 0;
      //DamagePerShot = 0;
      //HeatDamagePerShot = 0;
      //HeatGenerated = 0;
      //ProjectilesPerShot = 0;
      //ShotsWhenFired = 0;
      //ShotsWhenFiredMod = 1f;
      //CriticalChanceMultiplier = 0;
      //MinRange = 0;
      //MaxRange = 0;
      //LongRange = 0;
      //MaxRange = 0;
      //ShortRange = 0;
      //MediumRange = 0;
      //AIBattleValue = 100;
      //RefireModifier = 0;
      //Instability = 0;
      //AttackRecoil = 0;
      //EvasivePipsIgnored = 0;
      //FlatJammingChance = 0;
      //DistantVariance = 0;
      //DamageVariance = 0;
      //Cooldown = 0;
      //AIHitChanceCap = 0;
      //ForbiddenRange = 0;
      //GunneryJammingBase = 0;
      //GunneryJammingMult = 0;
      //AMSHitChance = 0f;
      //SpreadRange = 0f;
      //ShellsRadius = 0f;
      //HasShells = TripleBoolean.NotSet;
      //AlwaysIndirectVisuals = TripleBoolean.NotSet;
      //DamageMultiplier = 1.0f;
      //HeatMultiplier = 1.0f;
      //InstabilityMultiplier = 1.0f;
      //DamageOnJamming = TripleBoolean.NotSet;
      //DestroyOnJamming = TripleBoolean.NotSet;
      //DistantVarianceReversed = TripleBoolean.NotSet;
      //IndirectFireCapable = TripleBoolean.NotSet;
      //AOECapable = TripleBoolean.NotSet;
      //WeaponEffectID = "";
      //HitGenerator = HitGeneratorType.NotSet;
      //isBaseMode = false;
      //statusEffects = new List<EffectData>();
      //AmmoCategory = null;
      //IFFDef = "";
      //Unguided = TripleBoolean.NotSet;
      //MinShellsDistance = 30f;
      //MaxShellsDistance = 30f;
      //ArmorDamageModifier = 1f;
      //ISDamageModifier = 1f;
      //HeatGeneratedModifier = 1f;
      //FireTerrainChance = 0f;
      //FireDurationWithoutForest = 0;
      //FireTerrainStrength = 0;
      //FireTerrainCellRadius = 0;
      //AdditionalImpactVFXScaleX = 1f;
      //AdditionalImpactVFXScaleY = 1f;
      //AdditionalImpactVFXScaleZ = 1f;
      //FireOnSuccessHit = TripleBoolean.NotSet;
      //ClearMineFieldRadius = 0;
      //IsAMS = TripleBoolean.NotSet;
      //IsAAMS = TripleBoolean.NotSet;
      //BallisticDamagePerPallet = TripleBoolean.NotSet;
      //AdditionalAudioEffect = string.Empty;
      //Streak = TripleBoolean.NotSet;
      //MissileFiringIntervalMultiplier = 1f;
      //MissileVolleyIntervalMultiplier = 1f;
      //ProjectileSpeedMultiplier = 1f;
      //FireDelayMultiplier = 1f;
      //CantHitUnaffecedByPathing = TripleBoolean.NotSet;
      //MissileVolleySize = 0;
      //ProjectileScale = new CustomVector(true);
      //MissileExplosionScale = new CustomVector(true);
      //ColorSpeedChange = 0f;
      //ColorChangeRule = ColorChangeRule.None;
      //APDamage = 0f;
      //APCriticalChanceMultiplier = float.NaN;
      //APArmorShardsMod = 0f;
      //APMaxArmorThickness = 0f;
      //DamageNotDivided = TripleBoolean.NotSet;
      //isHeatVariation = TripleBoolean.NotSet;
      //isDamageVariation = TripleBoolean.NotSet;
      //isStabilityVariation = TripleBoolean.NotSet;
      //Lock = new ModeLockSettings();
      //APDamageMultiplier = 1f;
      //ClusteringModifier = 0f;
      //AOEEffectsFalloff = TripleBoolean.NotSet;
      //PrefireAnimationSpeedMod = 1f;
      //FireAnimationSpeedMod = 1f;
      //evasivePipsMods = new EvasivePipsMods();
      //ShotsPerAmmo = 1f;
      //deferredEffect = new DeferredEffectDef();
      //preFireSFX = string.Empty;
      //MinMissRadius = 0f;
      //MaxMissRadius = 0f;
      //TagsAccuracyModifiers = new Dictionary<string, float>();
      //AMSImmune = TripleBoolean.NotSet;
      //AMSDamage = 0f;
      //MissileHealth = 0f;
      //RangedDmgFalloffType = DamageFalloffType.NotSet;
      //AoEDmgFalloffType = DamageFalloffType.NotSet;
      //DamageFalloffStartDistance = 0f;
      //DamageFalloffEndDistance = 0f;
      //AMSShootsEveryAttack = TripleBoolean.NotSet;
      //TargetMechLegsOnly = TripleBoolean.NotSet;
      //Description = string.Empty;
      //Name = WeaponMode.BASE_MODE_NAME;
      //meleeAttackType = MeleeAttackType.NotSet;
      //BuildingsDamageModifier = 1f;
      //TurretDamageModifier = 1f;
      //VehicleDamageModifier = 1f;
      //MechDamageModifier = 1f;
      //QuadDamageModifier = 1f;
      //TrooperSquadDamageModifier = 1f;
      //AirMechDamageModifier = 1f;
      //VTOLDamageModifier = 1f;
    }
    public void fromJSON(string json) {
      JObject jWeaponMode = JObject.Parse(json);
      if (jWeaponMode["Id"] != null) {
        this.Id = (string)jWeaponMode["Id"];
      }
      if (jWeaponMode["UIName"] != null) {
        this.UIName = (string)jWeaponMode["UIName"];
      }
      if (jWeaponMode["Description"] != null) {
        this.Description = (string)jWeaponMode["Description"];
      }
      if (jWeaponMode["Name"] != null) {
        this.Name = (string)jWeaponMode["Name"];
      } else {
        this.Name = this.UIName;
      }
      if (jWeaponMode["AccuracyModifier"] != null) {
        this.AccuracyModifier = (float)jWeaponMode["AccuracyModifier"];
      }
      if (jWeaponMode["DamagePerShot"] != null) {
        this.DamagePerShot = (float)jWeaponMode["DamagePerShot"];
      }
      if (jWeaponMode["ClusteringModifier"] != null) {
        this.ClusteringModifier = (float)jWeaponMode["ClusteringModifier"];
      }
      if (jWeaponMode["HeatDamagePerShot"] != null) {
        this.HeatDamagePerShot = (float)jWeaponMode["HeatDamagePerShot"];
      }
      if (jWeaponMode["HeatGenerated"] != null) {
        this.HeatGenerated = (int)jWeaponMode["HeatGenerated"];
      }
      if (jWeaponMode["ProjectilesPerShot"] != null) {
        this.ProjectilesPerShot = (int)jWeaponMode["ProjectilesPerShot"];
      }
      if (jWeaponMode["ShotsWhenFired"] != null) {
        this.ShotsWhenFired = (int)jWeaponMode["ShotsWhenFired"];
      }
      if (jWeaponMode["ShotsWhenFiredMod"] != null) {
        this.ShotsWhenFiredMod = (float)jWeaponMode["ShotsWhenFiredMod"];
      }
      if (jWeaponMode["CriticalChanceMultiplier"] != null) {
        this.CriticalChanceMultiplier = (float)jWeaponMode["CriticalChanceMultiplier"];
      }
      if (jWeaponMode["FireDelayMultiplier"] != null) {
        this.FireDelayMultiplier = (float)jWeaponMode["FireDelayMultiplier"];
      }
      if (jWeaponMode["HeatGeneratedModifier"] != null) {
        this.HeatGeneratedModifier = (float)jWeaponMode["HeatGeneratedModifier"];
      }
      if (jWeaponMode["ProjectileSpeedMultiplier"] != null) {
        this.ProjectileSpeedMultiplier = (float)jWeaponMode["ProjectileSpeedMultiplier"];
      }
      if (jWeaponMode["AIBattleValue"] != null) {
        this.AIBattleValue = (int)jWeaponMode["AIBattleValue"];
      }
      if (jWeaponMode["FireAnimationSpeedMod"] != null) {
        this.FireAnimationSpeedMod = (float)jWeaponMode["FireAnimationSpeedMod"];
      }
      if (jWeaponMode["PrefireAnimationSpeedMod"] != null) {
        this.PrefireAnimationSpeedMod = (float)jWeaponMode["PrefireAnimationSpeedMod"];
      }
      if (jWeaponMode["MinRange"] != null) {
        this.MinRange = (float)jWeaponMode["MinRange"];
      }
      if (jWeaponMode["ShotsPerAmmo"] != null) {
        this.ShotsPerAmmo = (float)jWeaponMode["ShotsPerAmmo"];
      }
      if (jWeaponMode["Streak"] != null) {
        this.Streak = ((bool)jWeaponMode["Streak"] == true) ? TripleBoolean.True : TripleBoolean.False;
      }
      if (jWeaponMode["IsAMS"] != null) {
        this.IsAMS = ((bool)jWeaponMode["IsAMS"] == true) ? TripleBoolean.True : TripleBoolean.False;
      }
      if (jWeaponMode["AMSImmune"] != null) {
        this.AMSImmune = ((bool)jWeaponMode["AMSImmune"] == true) ? TripleBoolean.True : TripleBoolean.False;
      }
      if (jWeaponMode["AMSDamage"] != null) {
        this.AMSDamage = (float)jWeaponMode["AMSDamage"];
      }
      if (jWeaponMode["MissileHealth"] != null) {
        this.MissileHealth = (float)jWeaponMode["MissileHealth"];
      }
      if (jWeaponMode["IsAAMS"] != null) {
        this.IsAAMS = ((bool)jWeaponMode["IsAAMS"] == true) ? TripleBoolean.True : TripleBoolean.False;
        if (this.IsAAMS == TripleBoolean.True) {
          this.IsAMS = TripleBoolean.True;
        }
      }
      if (jWeaponMode["AMSShootsEveryAttack"] != null) {
        this.AMSShootsEveryAttack = ((bool)jWeaponMode["AMSShootsEveryAttack"] == true) ? TripleBoolean.True : TripleBoolean.False;
      }
      if (jWeaponMode["isDamageVariation"] != null) {
        this.isDamageVariation = ((bool)jWeaponMode["isDamageVariation"] == true) ? TripleBoolean.True : TripleBoolean.False;
      }
      if (jWeaponMode["isHeatVariation"] != null) {
        this.isHeatVariation = ((bool)jWeaponMode["isHeatVariation"] == true) ? TripleBoolean.True : TripleBoolean.False;
      }
      if (jWeaponMode["isStabilityVariation"] != null) {
        this.isStabilityVariation = ((bool)jWeaponMode["isStabilityVariation"] == true) ? TripleBoolean.True : TripleBoolean.False;
      }
      if (jWeaponMode["AOEEffectsFalloff"] != null) {
        this.AOEEffectsFalloff = ((bool)jWeaponMode["AOEEffectsFalloff"] == true) ? TripleBoolean.True : TripleBoolean.False;
      }
      if (jWeaponMode["APDamage"] != null) {
        this.APDamage = (float)jWeaponMode["APDamage"];
      }
      if (jWeaponMode["APDamageMultiplier"] != null) {
        this.APDamageMultiplier = (float)jWeaponMode["APDamageMultiplier"];
      }
      if (jWeaponMode["APCriticalChanceMultiplier"] != null) {
        this.APCriticalChanceMultiplier = (float)jWeaponMode["APCriticalChanceMultiplier"];
      }
      if (jWeaponMode["APArmorShardsMod"] != null) {
        this.APArmorShardsMod = (float)jWeaponMode["APArmorShardsMod"];
      }
      if (jWeaponMode["APMaxArmorThickness"] != null) {
        this.APMaxArmorThickness = (float)jWeaponMode["APMaxArmorThickness"];
      }
      if (jWeaponMode["MinMissRadius"] != null) {
        this.MinMissRadius = (float)jWeaponMode["MinMissRadius"];
      }
      if (jWeaponMode["MaxMissRadius"] != null) {
        this.MaxMissRadius = (float)jWeaponMode["MaxMissRadius"];
      }
      if (jWeaponMode["MaxRange"] != null) {
        this.MaxRange = (float)jWeaponMode["MaxRange"];
      }
      if (jWeaponMode["ShortRange"] != null) {
        this.ShortRange = (float)jWeaponMode["ShortRange"];
      }
      if (jWeaponMode["ForbiddenRange"] != null) {
        this.ForbiddenRange = (float)jWeaponMode["ForbiddenRange"];
      }
      if (jWeaponMode["ProjectileScale"] != null) {
        this.ProjectileScale = jWeaponMode["ProjectileScale"].ToObject<CustomVector>();
      }
      if (jWeaponMode["Lock"] != null) {
        this.Lock = jWeaponMode["Lock"].ToObject<ModeLockSettings>();
      }
      if (jWeaponMode["MissileExplosionScale"] != null) {
        this.MissileExplosionScale = jWeaponMode["MissileExplosionScale"].ToObject<CustomVector>();
      }
      if (jWeaponMode["MediumRange"] != null) {
        this.MediumRange = (float)jWeaponMode["MediumRange"];
      }
      if (jWeaponMode["LongRange"] != null) {
        this.LongRange = (float)jWeaponMode["LongRange"];
      }
      if (jWeaponMode["SpreadRange"] != null) {
        this.SpreadRange = (float)jWeaponMode["SpreadRange"];
      }
      if (jWeaponMode["ArmorDamageModifier"] != null) {
        this.ArmorDamageModifier = (float)jWeaponMode["ArmorDamageModifier"];
      }
      if (jWeaponMode["MissileFiringIntervalMultiplier"] != null) {
        this.MissileFiringIntervalMultiplier = (float)jWeaponMode["MissileFiringIntervalMultiplier"];
      }
      if (jWeaponMode["MissileVolleyIntervalMultiplier"] != null) {
        this.MissileVolleyIntervalMultiplier = (float)jWeaponMode["MissileVolleyIntervalMultiplier"];
      }
      if (jWeaponMode["ISDamageModifier"] != null) {
        this.ISDamageModifier = (float)jWeaponMode["ISDamageModifier"];
      }
      if (jWeaponMode["RefireModifier"] != null) {
        this.RefireModifier = (int)jWeaponMode["RefireModifier"];
      }
      if (jWeaponMode["isBaseMode"] != null) {
        this.isBaseMode = (bool)jWeaponMode["isBaseMode"];
      }
      if (jWeaponMode["Instability"] != null) {
        this.Instability = (float)jWeaponMode["Instability"];
      }
      if (jWeaponMode["IFFDef"] != null) {
        this.IFFDef = (string)jWeaponMode["IFFDef"];
      }
      if (jWeaponMode["AttackRecoil"] != null) {
        this.AttackRecoil = (int)jWeaponMode["AttackRecoil"];
      }
      if (jWeaponMode["AdditionalAudioEffect"] != null) {
        this.AdditionalAudioEffect = (string)jWeaponMode["AdditionalAudioEffect"];
      }
      if (jWeaponMode["preFireSFX"] != null) {
        this.preFireSFX = (string)jWeaponMode["preFireSFX"];
      }
      if (jWeaponMode["WeaponEffectID"] != null) {
        this.WeaponEffectID = (string)jWeaponMode["WeaponEffectID"];
      }
      if (jWeaponMode["MissileVolleySize"] != null) {
        this.MissileVolleySize = (int)jWeaponMode["MissileVolleySize"];
      }
      if (jWeaponMode["AdditionalImpactVFXScaleX"] != null) {
        this.AdditionalImpactVFXScaleX = (float)jWeaponMode["AdditionalImpactVFXScaleX"];
      }
      if (jWeaponMode["BallisticDamagePerPallet"] != null) {
        this.BallisticDamagePerPallet = ((bool)jWeaponMode["BallisticDamagePerPallet"] == true) ? TripleBoolean.True : TripleBoolean.False;
      }
      if (jWeaponMode["AdditionalImpactVFXScaleY"] != null) {
        this.AdditionalImpactVFXScaleY = (float)jWeaponMode["AdditionalImpactVFXScaleY"];
      }
      if (jWeaponMode["AdditionalImpactVFXScaleZ"] != null) {
        this.AdditionalImpactVFXScaleZ = (float)jWeaponMode["AdditionalImpactVFXScaleZ"];
      }
      if (jWeaponMode["EvasivePipsIgnored"] != null) {
        this.EvasivePipsIgnored = (float)jWeaponMode["EvasivePipsIgnored"];
      }
      if (jWeaponMode["IndirectFireCapable"] != null) {
        this.IndirectFireCapable = ((bool)jWeaponMode["IndirectFireCapable"] == true) ? TripleBoolean.True : TripleBoolean.False;
      }
      if (jWeaponMode["ColorSpeedChange"] != null) {
        this.ColorSpeedChange = (float)jWeaponMode["ColorSpeedChange"];
      }
      if (jWeaponMode["ChassisTagsAccuracyModifiers"] != null) {
        this.TagsAccuracyModifiers = JsonConvert.DeserializeObject<Dictionary<string, float>>(jWeaponMode["ChassisTagsAccuracyModifiers"].ToString());
        Log.LogWrite((string)jWeaponMode["Id"] + " ChassisTagsAccuracyModifiers:\n");
        foreach (var tam in this.TagsAccuracyModifiers) {
          Log.LogWrite(" " + tam.Key + ":" + tam.Key);
        }
      }
      if (jWeaponMode["ColorChangeRule"] != null) {
        this.ColorChangeRule = (ColorChangeRule)Enum.Parse(typeof(ColorChangeRule), (string)jWeaponMode["ColorChangeRule"]);
      }
      if (jWeaponMode["HasShells"] != null) {
        this.HasShells = ((bool)jWeaponMode["HasShells"] == true) ? TripleBoolean.True : TripleBoolean.False;
      }
      if (jWeaponMode["DamageNotDivided"] != null) {
        this.DamageNotDivided = ((bool)jWeaponMode["DamageNotDivided"] == true) ? TripleBoolean.True : TripleBoolean.False;
      }
      if (jWeaponMode["CantHitUnaffecedByPathing"] != null) {
        this.CantHitUnaffecedByPathing = ((bool)jWeaponMode["CantHitUnaffecedByPathing"] == true) ? TripleBoolean.True : TripleBoolean.False;
      }
      if (jWeaponMode["ShellsRadius"] != null) {
        this.ShellsRadius = (float)jWeaponMode["ShellsRadius"];
      }
      if (jWeaponMode["MinShellsDistance"] != null) {
        this.MinShellsDistance = (float)jWeaponMode["MinShellsDistance"];
      }
      if (jWeaponMode["MaxShellsDistance"] != null) {
        this.MaxShellsDistance = (float)jWeaponMode["MaxShellsDistance"];
      }
      if (jWeaponMode["DirectFireModifier"] != null) {
        this.DirectFireModifier = (float)jWeaponMode["DirectFireModifier"];
      }
      if (jWeaponMode["evasivePipsMods"] != null) {
        this.evasivePipsMods = jWeaponMode["evasivePipsMods"].ToObject<EvasivePipsMods>();
      }
      if (jWeaponMode["DistantVariance"] != null) {
        this.DistantVariance = (float)jWeaponMode["DistantVariance"];
      }
      if (jWeaponMode["DamageMultiplier"] != null) {
        this.DamageMultiplier = (float)jWeaponMode["DamageMultiplier"];
      }
      if (jWeaponMode["HeatMultiplier"] != null) {
        this.HeatMultiplier = (float)jWeaponMode["HeatMultiplier"];
      }
      if (jWeaponMode["InstabilityMultiplier"] != null) {
        this.InstabilityMultiplier = (float)jWeaponMode["InstabilityMultiplier"];
      }
      if (jWeaponMode["DistantVarianceReversed"] != null) {
        this.DistantVarianceReversed = ((bool)jWeaponMode["DistantVarianceReversed"] == true) ? TripleBoolean.True : TripleBoolean.False;
      }
      if (jWeaponMode["DamageVariance"] != null) {
        this.DamageVariance = (float)jWeaponMode["DamageVariance"];
      }
      if (jWeaponMode["FlatJammingChance"] != null) {
        this.FlatJammingChance = (float)jWeaponMode["FlatJammingChance"];
      }
      if (jWeaponMode["ClearMineFieldRadius"] != null) {
        this.ClearMineFieldRadius = (int)jWeaponMode["ClearMineFieldRadius"];
      }
      if (jWeaponMode["AMSHitChance"] != null) {
        this.AMSHitChance = (float)jWeaponMode["AMSHitChance"];
      }
      if (jWeaponMode["GunneryJammingBase"] != null) {
        this.GunneryJammingBase = (float)jWeaponMode["GunneryJammingBase"];
      }
      if (jWeaponMode["GunneryJammingMult"] != null) {
        this.GunneryJammingMult = (float)jWeaponMode["GunneryJammingMult"];
      }
      if (jWeaponMode["AIHitChanceCap"] != null) {
        this.AIHitChanceCap = (float)jWeaponMode["AIHitChanceCap"];
      }
      if (jWeaponMode["Cooldown"] != null) {
        this.Cooldown = (int)jWeaponMode["Cooldown"];
      }
      if (jWeaponMode["FireTerrainChance"] != null) {
        this.FireTerrainChance = (float)jWeaponMode["FireTerrainChance"];
      }
      if (jWeaponMode["FireDurationWithoutForest"] != null) {
        this.FireDurationWithoutForest = (int)jWeaponMode["FireDurationWithoutForest"];
      }
      if (jWeaponMode["FireTerrainStrength"] != null) {
        this.FireTerrainStrength = (int)jWeaponMode["FireTerrainStrength"];
      }
      if (jWeaponMode["FireOnSuccessHit"] != null) {
        this.FireOnSuccessHit = ((bool)jWeaponMode["FireOnSuccessHit"] == true) ? TripleBoolean.True : TripleBoolean.False;
      }
      if (jWeaponMode["FireTerrainCellRadius"] != null) {
        this.FireTerrainCellRadius = (int)jWeaponMode["FireTerrainCellRadius"];
      }
      if (jWeaponMode["AdditionalImpactVFX"] != null) {
        this.AdditionalImpactVFX = (string)jWeaponMode["AdditionalImpactVFX"];
      }
      if (jWeaponMode["AmmoCategory"] != null) {
        this.AmmoCategory = CustomAmmoCategories.find((string)jWeaponMode["AmmoCategory"]);
      }
      if (jWeaponMode["DamageOnJamming"] != null) {
        this.DamageOnJamming = ((bool)jWeaponMode["DamageOnJamming"] == true) ? TripleBoolean.True : TripleBoolean.False;
      }
      if (jWeaponMode["DestroyOnJamming"] != null) {
        this.DestroyOnJamming = ((bool)jWeaponMode["DestroyOnJamming"] == true) ? TripleBoolean.True : TripleBoolean.False;
      }
      if (jWeaponMode["RangedDmgFalloffType"] != null) {
        this.RangedDmgFalloffType = (DamageFalloffType)Enum.Parse(typeof(DamageFalloffType), (string)jWeaponMode["RangedDmgFalloffType"]);
      }
      if (jWeaponMode["AoEDmgFalloffType"] != null) {
        this.AoEDmgFalloffType = (DamageFalloffType)Enum.Parse(typeof(DamageFalloffType), (string)jWeaponMode["AoEDmgFalloffType"]);
      }
      if (jWeaponMode["AOECapable"] != null) {
        this.AOECapable = ((bool)jWeaponMode["AOECapable"] == true) ? TripleBoolean.True : TripleBoolean.False; ;
      }
      if (jWeaponMode["AlwaysIndirectVisuals"] != null) {
        this.AlwaysIndirectVisuals = ((bool)jWeaponMode["AlwaysIndirectVisuals"] == true) ? TripleBoolean.True : TripleBoolean.False;
      }
      if (jWeaponMode["Unguided"] != null) {
        this.Unguided = ((bool)jWeaponMode["Unguided"] == true) ? TripleBoolean.True : TripleBoolean.False;
        if (this.Unguided == TripleBoolean.True) {
          //this.AlwaysIndirectVisuals = TripleBoolean.False;
          //this.IndirectFireCapable = TripleBoolean.False;
        }
      }
      if (jWeaponMode["deferredEffect"] != null) {
        this.deferredEffect = JsonConvert.DeserializeObject<DeferredEffectDef>(jWeaponMode["deferredEffect"].ToString());
        if (jWeaponMode["deferredEffect"]["statusEffects"] != null) {
          this.deferredEffect.ParceEffects(jWeaponMode["deferredEffect"]["statusEffects"].ToString());
        }
      }
      if (jWeaponMode["HitGenerator"] != null) {
        try {
          this.HitGenerator = (HitGeneratorType)Enum.Parse(typeof(HitGeneratorType), (string)jWeaponMode["HitGenerator"], true);
        } catch (Exception) {
          this.HitGenerator = HitGeneratorType.NotSet;
        }
        jWeaponMode.Remove("HitGenerator");
      }
      foreach (PropertyInfo prop in typeof(WeaponMode).GetProperties()) {
        if (jWeaponMode[prop.Name] == null) { continue; }
        if (prop.DeclaringType == typeof(float)) {
          prop.SetValue(this, (float)jWeaponMode[prop.Name]);
        } else if (prop.DeclaringType == typeof(int)) {
          prop.SetValue(this, (int)jWeaponMode[prop.Name]);
        } else if (prop.DeclaringType == typeof(string)) {
          prop.SetValue(this, (string)jWeaponMode[prop.Name]);
        } else if (prop.DeclaringType == typeof(TripleBoolean)) {
          prop.SetValue(this, ((bool)jWeaponMode[prop.Name] == true) ? TripleBoolean.True : TripleBoolean.False);
        } else if (prop.DeclaringType == typeof(bool)) {
          prop.SetValue(this, (bool)jWeaponMode[prop.Name]);
        } else if (prop.DeclaringType == typeof(EvasivePipsMods)) {
          prop.SetValue(this, jWeaponMode[prop.Name].ToObject<EvasivePipsMods>());
        } else if (prop.DeclaringType == typeof(CustomVector)) {
          prop.SetValue(this, jWeaponMode[prop.Name].ToObject<CustomVector>());
        } else if (prop.DeclaringType.IsEnum) {
          prop.SetValue(this, Enum.Parse(prop.DeclaringType, (string)jWeaponMode[prop.Name]));
        } else { continue; }
      }
      if (jWeaponMode["statusEffects"] != null) {

        if (jWeaponMode["statusEffects"].Type == JTokenType.Array) {
          this.statusEffects.Clear();
          JToken statusEffects = jWeaponMode["statusEffects"];
          foreach (JObject statusEffect in statusEffects) {
            EffectData effect = new EffectData();
            JSONSerializationUtility.FromJSON<EffectData>(effect, statusEffect.ToString());
            this.statusEffects.Add(effect);
          }
        }
      }
    }
  }
}
