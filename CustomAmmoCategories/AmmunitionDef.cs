using BattleTech;
using CustAmmoCategories;
using CustomAmmoCategoriesLog;
using Harmony;
using HBS.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEngine;

namespace CustAmmoCategories {
  [SelfDocumentedClass("Weapons", "Weapons", "EvasivePipsMods")]
  public class EvasivePipsMods {
    public float Damage { get; set; } = 0f;
    public float APDamage { get; set; } = 0f;
    public float Heat { get; set; } = 0f;
    public float Instablility { get; set; } = 0f;
    public float GeneratedHeat { get; set; } = 0f;
    public float FlatJammingChance { get; set; } = 0f;
    public float MinRange { get; set; } = 0f;
    public float ShortRange { get; set; } = 0f;
    public float MediumRange { get; set; } = 0f;
    public float LongRange { get; set; } = 0f;
    public float MaxRange { get; set; } = 0f;
    public float AOERange { get; set; } = 0f;
    public float AOEDamage { get; set; } = 0f;
    public float AOEHeatDamage { get; set; } = 0f;
    public float AOEInstability { get; set; } = 0f;
    public float RefireModifier { get; set; } = 0f;
    public float APCriticalChanceMultiplier { get; set; } = 0f;
    public float AccuracyModifier { get; set; } = 0f;
    public float DamageVariance { get; set; } = 0f;
    public float CriticalChanceMultiplier { get; set; } = 0f;
    public EvasivePipsMods() {
      //this.Damage = 0f;
      //this.APDamage = 0f;
      //this.Heat = 0f;
      //this.Instablility = 0f;
      //this.GeneratedHeat = 0f;
      //this.FlatJammingChance = 0f;
      //this.MinRange = 0f;
      //this.ShortRange = 0f;
      //this.MediumRange = 0f;
      //this.LongRange = 0f;
      //this.MaxRange = 0f;
      //this.AOERange = 0f;
      //this.AOEDamage = 0f;
      //this.AOEHeatDamage = 0f;
      //this.AOEInstability = 0f;
      //this.RefireModifier = 0f;
      //this.APCriticalChanceMultiplier = 0f;
      //this.AccuracyModifier = 0f;
      //this.DamageVariance = 0f;
      //this.CriticalChanceMultiplier = 0f;
    }
  }
  public enum MinefieldBurnReaction {
    None,Destroy,LooseElectronic,Explode
  }
  [SelfDocumentedClass("Weapons", "Weapons", "MineFieldDef")]
  public class MineFieldDef {
    public string UIName { get; set; } = string.Empty;
    public float Damage { get; set; } = 0f;
    public float Heat { get; set; } = 0f;
    public float Instability { get; set; } = 0f;
    public float AoEDamage { get; set; } = 0f;
    public float AoEHeat { get; set; } = 0f;
    public float AoEInstability { get; set; } = 0f;
    public float AoERange { get; set; } = 0f;
    public float Chance { get; set; } = 0f;
    public int Count { get; set; } = 0;
    public string VFXprefab { get; set; } = string.Empty;
    public float VFXScaleX { get; set; } = 1f;
    public float VFXScaleY { get; set; } = 1f;
    public float VFXScaleZ { get; set; } = 1f;
    public float VFXOffsetX { get; set; } = 0f;
    public float VFXOffsetY { get; set; } = 0f;
    public float VFXOffsetZ { get; set; } = 0f;
    public float VFXMinDistance { get; set; } = 20f;
    public int InstallCellRange { get; set; } = 0;
    public string SFX { get; set; } = string.Empty;
    [SelfDocumentationTypeName("List of statusEffects"), SelfDocumentationDefaultValue("empty")]
    public List<EffectData> statusEffects { get; set; } = new List<EffectData>();
    public float FireTerrainChance { get; set; } = 0f;
    public int FireDurationWithoutForest { get; set; } = 0;
    public int FireTerrainStrength { get; set; } = 0;
    public int FireTerrainCellRadius { get; set; } = 0;
    public string LongVFXOnImpact { get; set; } = string.Empty;
    public string tempDesignMaskOnImpact { get; set; } = string.Empty;
    public int tempDesignMaskOnImpactTurns { get; set; } = 0;
    public float LongVFXOnImpactScaleX { get; set; } = 0f;
    public float LongVFXOnImpactScaleY { get; set; } = 0f;
    public float LongVFXOnImpactScaleZ { get; set; } = 0f;
    public int tempDesignMaskCellRadius { get; set; } = 0;
    public int stealthLevel { get; set; } = 1;
    public int IFFLevel { get; set; } = 1;
    public string Icon { get; set; } = "bomb";
    public MinefieldBurnReaction burnReaction { get; set; } = MinefieldBurnReaction.Destroy;
    public DamageFalloffType AoEDmgFalloffType { get; set; } = DamageFalloffType.Linear;
    public float mAoEDmgFalloffType(float value) {
      switch (this.AoEDmgFalloffType) {
        case DamageFalloffType.Quadratic: return value * value;
        case DamageFalloffType.Cubic: return value * value * value;
        case DamageFalloffType.SquareRoot: return Mathf.Sqrt(value);
        case DamageFalloffType.Linear: return value;
        case DamageFalloffType.Log10: return Mathf.Log10(value);
        case DamageFalloffType.LogE: return Mathf.Log(value);
        case DamageFalloffType.Exp: return Mathf.Exp(value);
        default: return value;
      }
    }
    public MineFieldDef() {
      //Damage = 0f;
      //Heat = 0f;
      //Instability = 0f;
      //AoEDamage = 0f;
      //AoEHeat = 0f;
      //AoEInstability = 0f;
      //AoERange = 0f;
      //Chance = 0f;
      //VFXprefab = string.Empty;
      //VFXScaleX = 1f;
      //VFXScaleY = 1f;
      //VFXScaleZ = 1f;
      //VFXOffsetX = 0f;
      //VFXOffsetY = 0f;
      //VFXOffsetZ = 0f;
      //VFXMinDistance = 20f;
      //InstallCellRange = 0;
      //SFX = string.Empty;
      //Count = 0;
      //FireTerrainChance = 0f;
      //FireDurationWithoutForest = 0;
      //FireTerrainStrength = 0;
      //FireTerrainCellRadius = 0;
      //LongVFXOnImpact = string.Empty;
      //tempDesignMaskOnImpact = string.Empty;
      //tempDesignMaskOnImpactTurns = 0;
      //LongVFXOnImpactScaleX = 0f;
      //LongVFXOnImpactScaleY = 0f;
      //LongVFXOnImpactScaleZ = 0f;
      //tempDesignMaskCellRadius = 0;
      //AoEDmgFalloffType = DamageFalloffType.Linear;
      //statusEffects = new List<EffectData>();
      //UIName = string.Empty;
      //stealthLevel = 1;
      //IFFLevel = 1;
      //Icon = "bomb";
      //burnReaction = MinefieldBurnReaction.Destroy;
    }
    public void fromJSON(JToken json) {
      if (json["Damage"] != null) { Damage = (float)json["Damage"]; };
      if (json["stealthLevel"] != null) { stealthLevel = (int)json["stealthLevel"]; };
      if (json["IFFLevel"] != null) { IFFLevel = (int)json["IFFLevel"]; };
      if (json["UIName"] != null) { UIName = (string)json["UIName"]; };
      if (json["Icon"] != null) { Icon = (string)json["Icon"]; };
      if (json["Heat"] != null) { Heat = (float)json["Heat"]; };
      if (json["Instability"] != null) { Instability = (float)json["Instability"]; };
      if (json["AOERange"] != null) { AoERange = (float)json["AOERange"]; };
      if (json["AOEDamage"] != null) { AoEDamage = (float)json["AOEDamage"]; };
      if (json["AOEHeat"] != null) { AoEHeat = (float)json["AOEHeat"]; };
      if (json["AOEInstability"] != null) { AoEInstability = (float)json["AOEInstability"]; };
      if (json["Chance"] != null) { Chance = (float)json["Chance"]; };
      if (json["VFXMinDistance"] != null) { VFXMinDistance = (float)json["VFXMinDistance"]; if (VFXMinDistance < 20f) { VFXMinDistance = 20f; } };
      if (json["VFXprefab"] != null) { VFXprefab = (string)json["VFXprefab"]; };
      if (json["VFXScaleX"] != null) { VFXScaleX = (float)json["VFXScaleX"]; };
      if (json["VFXScaleY"] != null) { VFXScaleY = (float)json["VFXScaleY"]; };
      if (json["VFXScaleZ"] != null) { VFXScaleZ = (float)json["VFXScaleZ"]; };
      if (json["VFXOffsetX"] != null) { VFXOffsetX = (float)json["VFXOffsetX"]; };
      if (json["VFXOffsetY"] != null) { VFXOffsetY = (float)json["VFXOffsetY"]; };
      if (json["VFXOffsetZ"] != null) { VFXOffsetZ = (float)json["VFXOffsetZ"]; };
      if (json["InstallCellRange"] != null) { InstallCellRange = (int)json["InstallCellRange"]; };
      if (json["Count"] != null) { Count = (int)json["Count"]; };
      if (json["SFX"] != null) { SFX = (string)json["SFX"]; };
      if (json["FireTerrainChance"] != null) { FireTerrainChance = (float)json["FireTerrainChance"]; };
      if (json["FireDurationWithoutForest"] != null) { FireDurationWithoutForest = (int)json["FireDurationWithoutForest"]; };
      if (json["FireTerrainStrength"] != null) { FireTerrainStrength = (int)json["FireTerrainStrength"]; };
      if (json["FireTerrainCellRadius"] != null) { FireTerrainCellRadius = (int)json["FireTerrainCellRadius"]; };
      if (json["LongVFXOnImpact"] != null) { LongVFXOnImpact = (string)json["LongVFXOnImpact"]; };
      if (json["tempDesignMaskOnImpact"] != null) { tempDesignMaskOnImpact = (string)json["tempDesignMaskOnImpact"]; };
      if (json["tempDesignMaskOnImpactTurns"] != null) { tempDesignMaskOnImpactTurns = (int)json["tempDesignMaskOnImpactTurns"]; };
      if (json["LongVFXOnImpactScaleX"] != null) { LongVFXOnImpactScaleX = (float)json["LongVFXOnImpactScaleX"]; };
      if (json["LongVFXOnImpactScaleY"] != null) { LongVFXOnImpactScaleY = (float)json["LongVFXOnImpactScaleY"]; };
      if (json["LongVFXOnImpactScaleZ"] != null) { LongVFXOnImpactScaleZ = (float)json["LongVFXOnImpactScaleZ"]; };
      if (json["tempDesignMaskCellRadius"] != null) { tempDesignMaskCellRadius = (int)json["tempDesignMaskCellRadius"]; };
      if (json["burnReaction"] != null) { burnReaction = (MinefieldBurnReaction)Enum.Parse(typeof(MinefieldBurnReaction), (string)json["burnReaction"]); };
      if (json["statusEffects"] != null) {
        if (json["statusEffects"].Type == JTokenType.Array) {
          JToken jStatusEffects = json["statusEffects"];
          foreach (JObject jse in jStatusEffects) {
            EffectData effect = new EffectData();
            JSONSerializationUtility.FromJSON<EffectData>(effect, jse.ToString());
            this.statusEffects.Add(effect);
          }
        }
      }
    }
    public DesignMaskDef TempDesignMask() {
      if (string.IsNullOrEmpty(this.tempDesignMaskOnImpact)) { return null; };
      if (DynamicMapHelper.loadedMasksDef.ContainsKey(this.tempDesignMaskOnImpact) == false) { return null; }
      return DynamicMapHelper.loadedMasksDef[this.tempDesignMaskOnImpact];
    }
  }
  public enum AutoRefilType { Manual,Shop,Automatic }
  [SelfDocumentedClass("Weapons", "Weapons", "AmmunitionDef")]
  public class ExtAmmunitionDef {
    [SkipDocumentation]
    public string Id { get; set; } = "NotSet";
    public string Name { get; set; } = string.Empty;
    public string UIName { get; set; } = string.Empty;
    public float AccuracyModifier { get; set; } = 0f;
    public float DirectFireModifier { get; set; } = 0f;
    public float DamagePerShot { get; set; } = 0f;
    public float HeatDamagePerShot { get; set; } = 0f;
    public float CriticalChanceMultiplier { get; set; } = 0f;
    public int ShotsWhenFired { get; set; } = 0;
    public float ShotsWhenFiredMod { get; set; } = 1f;
    [SkipDocumentation]
    public int AIBattleValue { get; set; }
    public int ProjectilesPerShot { get; private set; } = 0;
    [SelfDocumentationTypeName("List of statusEffects"), SelfDocumentationDefaultValue("empty")]
    public EffectData[] statusEffects { get; set; } = new EffectData[] { };
    public float MinRange { get; set; } = 0f;
    public float MaxRange { get; set; } = 0f;
    public float LongRange { get; set; } = 0f;
    public float ShortRange { get; set; } = 0f;
    public float ForbiddenRange { get; set; } = 0f;
    public float MediumRange { get; set; } = 0f;
    public int RefireModifier { get; set; } = 0;
    public int AttackRecoil { get; set; } = 0;
    public float Instability { get; set; } = 0f;
    public string WeaponEffectID { get; set; } = string.Empty;
    public float EvasivePipsIgnored { get; set; } = 0f;
    public TripleBoolean IndirectFireCapable { get; set; } = TripleBoolean.NotSet;
    public TripleBoolean AOECapable { get; set; } = TripleBoolean.NotSet;
    public HitGeneratorType HitGenerator { get; set; } = HitGeneratorType.NotSet;
    public float FlatJammingChance { get; set; } = 0f;
    public float AMSHitChance { get; set; } = 0f;
    public float GunneryJammingBase { get; set; } = 0f;
    public float GunneryJammingMult { get; set; } = 0f;
    public float DistantVariance { get; set; } = 0f;
    public TripleBoolean DistantVarianceReversed { get; set; } = TripleBoolean.NotSet;
    public float DamageVariance { get; set; } = 0f;
    public float DamageMultiplier { get; set; } = 1f;
    public float HeatMultiplier { get; set; } = 1f;
    public float InstabilityMultiplier { get; set; } = 1f;
    [JsonIgnore, SelfDocumentationTypeName("string, id from BattleTech.AmmoCategoryEnumeration or CustomAmmo"), SelfDocumentationDefaultValue("NotSet")]
    public CustomAmmoCategory AmmoCategory { get; set; } = CustomAmmoCategories.NotSetCustomAmmoCategoty;
    public float SpreadRange { get; set; } = 0f;
    public float AOERange { get; set; } = 0f;
    public float AOEDamage { get; set; } = 0f;
    public float AOEHeatDamage { get; set; } = 0f;
    public float AOEInstability { get; set; } = 0f;
    public TripleBoolean AOEEffectsFalloff { get; set; } = TripleBoolean.NotSet;
    public TripleBoolean AlwaysIndirectVisuals { get; set; } = TripleBoolean.NotSet;
    public string IFFDef { get; set; } = string.Empty;
    public string LongVFXOnImpact { get; set; } = string.Empty;
    public string tempDesignMaskOnImpact { get; set; } = string.Empty;
    public int tempDesignMaskOnImpactTurns { get; set; } = 0;
    public float LongVFXOnImpactScaleX { get; set; } = 1f;
    public float LongVFXOnImpactScaleY { get; set; } = 1f;
    public float LongVFXOnImpactScaleZ { get; set; } = 1f;
    public int tempDesignMaskCellRadius { get; set; } = 0;
    public TripleBoolean HasShells { get; set; } = TripleBoolean.NotSet;
    public float ShellsRadius { get; set; } = 0f;
    public float MinShellsDistance { get; set; } = 30f;
    public float MaxShellsDistance { get; set; } = 30f;
    public float UnseparatedDamageMult { get; set; } = 1f;
    public float ArmorDamageModifier { get; set; } = 1f;
    public float ISDamageModifier { get; set; } = 1f;
    public float HeatGeneratedModifier { get; set; } = 1f;
    public float CanBeExhaustedAt { get; set; } = 0f;
    //public Dictionary<TerrainMaskFlags,string> SurfaceImpactDesignMaskId { get; set; }
    public TripleBoolean SurfaceBecomeDangerousOnImpact { get; set; } = TripleBoolean.NotSet;
    public TripleBoolean Unguided { get; set; } = TripleBoolean.NotSet;
    public int ClearMineFieldRadius { get; set; } = 0;
    public float FireTerrainChance { get; set; } = 0f;
    public int FireDurationWithoutForest { get; set; } = 0;
    public int FireTerrainStrength { get; set; } = 0;
    public int FireTerrainCellRadius { get; set; } = 0;
    public string AdditionalImpactVFX { get; set; } = string.Empty;
    public float AdditionalImpactVFXScaleX { get; set; } = 1f;
    public float AdditionalImpactVFXScaleY { get; set; } = 1f;
    public float AdditionalImpactVFXScaleZ { get; set; } = 1f;
    public TripleBoolean FireOnSuccessHit { get; set; } = TripleBoolean.NotSet;
    public TripleBoolean IsAMS { get; set; } = TripleBoolean.NotSet;
    public TripleBoolean IsAAMS { get; set; } = TripleBoolean.NotSet;
    public TripleBoolean BallisticDamagePerPallet { get; set; } = TripleBoolean.NotSet;
    public string AdditionalAudioEffect { get; set; } = string.Empty;
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("MineFieldDef structure")]
    public MineFieldDef MineField { get; set; } = new MineFieldDef();
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("Dictionary of {\"<tag name>\":<float modifier>}")]
    public Dictionary<string, float> TagsAccuracyModifiers { get; set; } = new Dictionary<string, float>();
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
    public void ProjectilesPerShotSet(int value) {
      this.ProjectilesPerShot = value;
    }
    public float ColorSpeedChange { get; set; } = 0f;
    public ColorChangeRule ColorChangeRule { get; set; } = ColorChangeRule.None;
    public float APDamage { get; set; } = 0f;
    public float APDamageMultiplier { get; set; } = 1f;
    [SelfDocumentationDefaultValue("undefined")]
    public float APCriticalChanceMultiplier { get; set; } = float.NaN;
    public float APArmorShardsMod { get; set; } = 0f;
    public float APMaxArmorThickness { get; set; } = 0f;
    public TripleBoolean DamageNotDivided { get; set; } = TripleBoolean.NotSet;
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("array of { \"I\": <float intensivity>, \"C\":\"<html color string>\"}")]
    public List<ColorTableJsonEntry> ColorsTable { get; set; } = new List<ColorTableJsonEntry>();
    public TripleBoolean isHeatVariation { get; set; } = TripleBoolean.NotSet;
    public TripleBoolean isStabilityVariation { get; set; } = TripleBoolean.NotSet;
    public TripleBoolean isDamageVariation { get; set; } = TripleBoolean.NotSet;
    public float ClusteringModifier { get; set; } = 0f;
    public float PrefireAnimationSpeedMod { get; set; } = 1f;
    public float FireAnimationSpeedMod { get; set; } = 1f;
    public float HeatGenerated { get; set; } = 0f;
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("EvasivePipsMods structure")]
    public EvasivePipsMods evasivePipsMods { get; set; } = new EvasivePipsMods();
    public float ShotsPerAmmo { get; set; } = 1f;
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("DeferredEffectDef structure")]
    public DeferredEffectDef deferredEffect { get; set; } = new DeferredEffectDef();
    public string preFireSFX { get; set; } = string.Empty;
    public bool HideIfOnlyVariant { get; set; } = false;
    public float MinMissRadius { get; set; } = 0f;
    public float MaxMissRadius { get; set; } = 0f;
    public TripleBoolean AMSImmune { get; set; } = TripleBoolean.NotSet;
    public float AMSDamage { get; set; } = 0f;
    public float MissileHealth { get; set; } = 0f;
    public DamageFalloffType RangedDmgFalloffType { get; set; } = DamageFalloffType.NotSet;
    public DamageFalloffType AoEDmgFalloffType { get; set; } = DamageFalloffType.NotSet;
    public float DamageFalloffStartDistance { get; set; } = 0f;
    public float DamageFalloffEndDistance { get; set; } = 0f;
    public AutoRefilType AutoRefill { get; set; } = AutoRefilType.Automatic;
    [SkipDocumentation]
    public HashSet<string> ammoOnlyBoxes { get; set; } = new HashSet<string>();
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("list of strings")]
    public HashSet<string> AvailableOnPlanet { get; set; } = new HashSet<string>();
    public float BuildingsDamageModifier { get; set; } = 1f;
    public float TurretDamageModifier { get; set; } = 1f;
    public float VehicleDamageModifier { get; set; } = 1f;
    public float MechDamageModifier { get; set; } = 1f;
    public float QuadDamageModifier { get; set; } = 1f;
    public float TrooperSquadDamageModifier { get; set; } = 1f;
    public float AirMechDamageModifier { get; set; } = 1f;
    public float VTOLDamageModifier { get; set; } = 1f;
    public ExtAmmunitionDef() {
      //Id = "NotSet";
      //Name = string.Empty;
      //UIName = string.Empty;
      //AccuracyModifier = 0;
      //DirectFireModifier = 0;
      //DamagePerShot = 0;
      //HeatDamagePerShot = 0;
      //ProjectilesPerShot = 0;
      //ShotsWhenFired = 0;
      //ShotsWhenFiredMod = 1f;
      //CriticalChanceMultiplier = 0;
      //DamageMultiplier = 1.0f;
      //HeatMultiplier = 1.0f;
      //InstabilityMultiplier = 1.0f;
      //MinRange = 0;
      //MaxRange = 0;
      //LongRange = 0;
      //AMSHitChance = 0f;
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
      //DistantVarianceReversed = TripleBoolean.NotSet;
      //DamageVariance = 0;
      //ForbiddenRange = 0;
      //GunneryJammingBase = 0;
      //GunneryJammingMult = 0;
      //AOERange = 0;
      //AOEDamage = 0;
      //AOEHeatDamage = 0;
      //AOEInstability = 0f;
      //SpreadRange = 0;
      //IndirectFireCapable = TripleBoolean.NotSet;
      //AlwaysIndirectVisuals = TripleBoolean.NotSet;
      //AOECapable = TripleBoolean.NotSet;
      //WeaponEffectID = "";
      //IFFDef = "";
      //HitGenerator = HitGeneratorType.NotSet;
      //statusEffects = new EffectData[0] { };
      //AmmoCategory = new CustomAmmoCategory();
      //HasShells = TripleBoolean.NotSet;
      //ShellsRadius = 0f;
      //MinShellsDistance = 30f;
      //MaxShellsDistance = 30f;
      //UnseparatedDamageMult = 1f;
      //ISDamageModifier = 1f;
      //ArmorDamageModifier = 1f;
      //HeatGeneratedModifier = 1f;
      //CanBeExhaustedAt = 0f;
      ////SurfaceImpactDesignMaskId = new Dictionary<TerrainMaskFlags, string>();
      //Unguided = TripleBoolean.NotSet;
      //FireTerrainChance = 0f;
      //FireDurationWithoutForest = 0;
      //FireTerrainStrength = 0;
      //FireTerrainCellRadius = 0;
      //AdditionalImpactVFX = string.Empty;
      //AdditionalImpactVFXScaleX = 1f;
      //AdditionalImpactVFXScaleY = 1f;
      //AdditionalImpactVFXScaleZ = 1f;
      //LongVFXOnImpactScaleX = 1f;
      //LongVFXOnImpactScaleY = 1f;
      //LongVFXOnImpactScaleZ = 1f;
      //FireOnSuccessHit = TripleBoolean.NotSet;
      //LongVFXOnImpact = string.Empty;
      //tempDesignMaskOnImpact = string.Empty;
      //tempDesignMaskOnImpactTurns = 0;
      //tempDesignMaskCellRadius = 0;
      //ClearMineFieldRadius = 0;
      //IsAMS = TripleBoolean.NotSet;
      //IsAAMS = TripleBoolean.NotSet;
      //BallisticDamagePerPallet = TripleBoolean.NotSet;
      //AdditionalAudioEffect = string.Empty;
      //TagsAccuracyModifiers = new Dictionary<string, float>();
      //Streak = TripleBoolean.NotSet;
      //FireDelayMultiplier = 1f;
      //MissileFiringIntervalMultiplier = 1f;
      //MissileVolleyIntervalMultiplier = 1f;
      //ProjectileSpeedMultiplier = 1f;
      //MineField = new MineFieldDef();
      //CantHitUnaffecedByPathing = TripleBoolean.NotSet;
      //MissileVolleySize = 0;
      //ProjectileScale = new CustomVector(true);
      //MissileExplosionScale = new CustomVector(true);
      //ColorSpeedChange = 0f;
      //ColorChangeRule = ColorChangeRule.None;
      //ColorsTable = new List<ColorTableJsonEntry>();
      //APDamage = 0f;
      //APCriticalChanceMultiplier = float.NaN;
      //APArmorShardsMod = 0f;
      //APMaxArmorThickness = 0f;
      //DamageNotDivided = TripleBoolean.NotSet;
      //isHeatVariation = TripleBoolean.NotSet;
      //isDamageVariation = TripleBoolean.NotSet;
      //isStabilityVariation = TripleBoolean.NotSet;
      //APDamageMultiplier = 1f;
      //ClusteringModifier = 0f;
      //AOEEffectsFalloff = TripleBoolean.NotSet;
      //PrefireAnimationSpeedMod = 1f;
      //FireAnimationSpeedMod = 1f;
      //HeatGenerated = 0f;
      //evasivePipsMods = new EvasivePipsMods();
      //ShotsPerAmmo = 1f;
      //this.deferredEffect = new DeferredEffectDef();
      //preFireSFX = string.Empty;
      //HideIfOnlyVariant = false;
      //MinMissRadius = 0f;
      //MaxMissRadius = 0f;
      //AMSImmune = TripleBoolean.NotSet;
      //AMSDamage = 0f;
      //MissileHealth = 0f;
      //RangedDmgFalloffType = DamageFalloffType.NotSet;
      //AoEDmgFalloffType = DamageFalloffType.NotSet;
      //DamageFalloffStartDistance = 0f;
      //DamageFalloffEndDistance = 0f;
      //AutoRefill = AutoRefilType.Automatic;
      //ammoOnlyBoxes = new HashSet<string>();
      //AvailableOnPlanet = new HashSet<string>();
      //BuildingsDamageModifier = 1f;
      //TurretDamageModifier = 1f;
      //VehicleDamageModifier = 1f;
      //MechDamageModifier = 1f;
      //QuadDamageModifier = 1f;
      //TrooperSquadDamageModifier = 1f;
      //AirMechDamageModifier = 1f;
      //VTOLDamageModifier = 1f;
    }
  }
}

namespace CustomAmmoCategoriesPatches {
  [HarmonyPatch(typeof(AmmunitionDef))]
  [HarmonyPatch("FromJSON")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class BattleTech_AmmunitionDef_fromJSON_Patch {
    private static Dictionary<string, HashSet<string>> ammunitionsDefs = new Dictionary<string, HashSet<string>>();
    private static SpinLock ammunitionsDefs_lock = new SpinLock();
    private static ConcurrentDictionary<string, ExtAmmunitionDef> defaultAmmunitions = new ConcurrentDictionary<string, ExtAmmunitionDef>();
    private static ConcurrentDictionary<string, string> originals = new ConcurrentDictionary<string, string>();
    public static string getOriginal(this AmmunitionDef def) { if (originals.TryGetValue(def.Description.Id, out string result)) { return result; } else { return def.ToJSON(); } }
    public static void setOriginal(this AmmunitionDef def, string json) {
      originals.AddOrUpdate(def.Description.Id, json, (k,v)=> { return json; });
      //if (originals.ContainsKey(def.Description.Id)) { originals[def.Description.Id] = json; } else { originals.Add(def.Description.Id, json);
      //}
    }
    public static bool Prefix(AmmunitionDef __instance, ref string json, ref ExtDefinitionParceInfo __state) {
      CustomAmmoCategories.CustomCategoriesInit();
      Log.LogWrite("AmmunitionDef fromJSON ");
      __state = new ExtDefinitionParceInfo();
      __state.baseJson = json;
      JObject defTemp = null;
      try {
        defTemp = JObject.Parse(json);
      } catch (Exception e) {
        __state.errorStr = e.ToString();
        return true;
      }
      try {
        CustomAmmoCategoriesLog.Log.LogWrite(defTemp["Description"]["Id"] + "\n");
        string AmmoCategory = "NotSet";
        if ((defTemp["Category"] != null)&&(defTemp["ammoCategoryID"] == null)) {
          AmmoCategory = (string)defTemp["Category"];
          //defTemp["ammoCategoryID"] = AmmoCategory;
        } else {
          AmmoCategory = (string)defTemp["ammoCategoryID"];
        }
        if (defTemp["Category"] != null) {
          defTemp.Remove("Category");
        }
        if (CustomAmmoCategories.contains(AmmoCategory) == false) {
          Log.M.TWL(0, "Custom Ammo Categories list not contains " + AmmoCategory);
          AmmoCategoryValue val = AmmoCategoryEnumeration.GetAmmoCategoryByName(AmmoCategory);
          if (val == null) {
            Log.M.WL(1, "AmmoCategoryEnumeration also not contains " + AmmoCategory + " fallback to NotSet");
            AmmoCategory = "NotSet";
          } else {
            Log.M.WL(1, "Adding new value to Custom Ammo Categories list " + val.Name + ":" + val.ID);
            CustomAmmoCategories.add(val);
          }
        }
        CustomAmmoCategory custCat = CustomAmmoCategories.find(AmmoCategory);
        //CustomAmmoCategories.RegisterAmmunition((string)defTemp["Description"]["Id"], custCat);
        defTemp["ammoCategoryID"] = custCat.BaseCategory.Name;
        Log.M.WL(1, "ammoCategoryID:" + (string)defTemp["ammoCategoryID"]);
        ExtAmmunitionDef extAmmoDef = new ExtAmmunitionDef();
        extAmmoDef.Id = (string)defTemp["Description"]["Id"];
        extAmmoDef.Name = (string)defTemp["Description"]["Name"];
        extAmmoDef.UIName = (string)defTemp["Description"]["UIName"];
        extAmmoDef.AmmoCategory = custCat;
        //if (defTemp["AccuracyModifier"] != null) {
        //  extAmmoDef.AccuracyModifier = (float)defTemp["AccuracyModifier"];
        //  defTemp.Remove("AccuracyModifier");
        //}
        //if (defTemp["HeatGenerated"] != null) {
        //  extAmmoDef.HeatGenerated = (float)defTemp["HeatGenerated"];
        //}
        //if (defTemp["DamagePerShot"] != null) {
        //  extAmmoDef.DamagePerShot = (float)defTemp["DamagePerShot"];
        //  defTemp.Remove("DamagePerShot");
        //}
        //if (defTemp["ClusteringModifier"] != null) {
        //  extAmmoDef.ClusteringModifier = (float)defTemp["ClusteringModifier"];
        //  defTemp.Remove("ClusteringModifier");
        //}
        //if (defTemp["HeatDamagePerShot"] != null) {
        //  extAmmoDef.HeatDamagePerShot = (float)defTemp["HeatDamagePerShot"];
        //  defTemp.Remove("HeatDamagePerShot");
        //}
        //if (defTemp["ProjectilesPerShot"] != null) {
        //  extAmmoDef.ProjectilesPerShotSet((int)defTemp["ProjectilesPerShot"]);
        //  defTemp.Remove("ProjectilesPerShot");
        //}
        //if (defTemp["ShotsWhenFired"] != null) {
        //  extAmmoDef.ShotsWhenFired = (int)defTemp["ShotsWhenFired"];
        //  defTemp.Remove("ShotsWhenFired");
        //}
        //if (defTemp["CriticalChanceMultiplier"] != null) {
        //  extAmmoDef.CriticalChanceMultiplier = (float)defTemp["CriticalChanceMultiplier"];
        //  defTemp.Remove("CriticalChanceMultiplier");
        //}
        //if (defTemp["FireDelayMultiplier"] != null) {
        //  extAmmoDef.FireDelayMultiplier = (float)defTemp["FireDelayMultiplier"];
        //  defTemp.Remove("FireDelayMultiplier");
        //}
        //if (defTemp["MissileFiringIntervalMultiplier"] != null) {
        //  extAmmoDef.MissileFiringIntervalMultiplier = (float)defTemp["MissileFiringIntervalMultiplier"];
        //  defTemp.Remove("MissileFiringIntervalMultiplier");
        //}
        //if (defTemp["MissileVolleyIntervalMultiplier"] != null) {
        //  extAmmoDef.MissileVolleyIntervalMultiplier = (float)defTemp["MissileVolleyIntervalMultiplier"];
        //  defTemp.Remove("MissileVolleyIntervalMultiplier");
        //}
        //if (defTemp["ProjectileSpeedMultiplier"] != null) {
        //  extAmmoDef.ProjectileSpeedMultiplier = (float)defTemp["ProjectileSpeedMultiplier"];
        //  defTemp.Remove("ProjectileSpeedMultiplier");
        //}
        //if (defTemp["FireAnimationSpeedMod"] != null) {
        //  extAmmoDef.FireAnimationSpeedMod = (float)defTemp["FireAnimationSpeedMod"];
        //  defTemp.Remove("FireAnimationSpeedMod");
        //}
        //if (defTemp["PrefireAnimationSpeedMod"] != null) {
        //  extAmmoDef.PrefireAnimationSpeedMod = (float)defTemp["PrefireAnimationSpeedMod"];
        //  defTemp.Remove("PrefireAnimationSpeedMod");
        //}
        //if (defTemp["AIBattleValue"] != null) {
        //  extAmmoDef.AIBattleValue = (int)defTemp["AIBattleValue"];
        //  defTemp.Remove("AIBattleValue");
        //}
        //if (defTemp["ColorSpeedChange"] != null) {
        //  extAmmoDef.ColorSpeedChange = (float)defTemp["ColorSpeedChange"];
        //  defTemp.Remove("ColorSpeedChange");
        //}
        //if (defTemp["ColorChangeRule"] != null) {
        //  extAmmoDef.ColorChangeRule = (ColorChangeRule)Enum.Parse(typeof(ColorChangeRule), (string)defTemp["ColorChangeRule"]);
        //  defTemp.Remove("ColorChangeRule");
        //}
        //if (defTemp["APDamage"] != null) {
        //  extAmmoDef.APDamage = (float)defTemp["APDamage"];
        //  defTemp.Remove("APDamage");
        //}
        //if (defTemp["ShotsPerAmmo"] != null) {
        //  extAmmoDef.ShotsPerAmmo = (float)defTemp["ShotsPerAmmo"];
        //  defTemp.Remove("ShotsPerAmmo");
        //}
        //if (defTemp["evasivePipsMods"] != null) {
        //  extAmmoDef.evasivePipsMods = defTemp["evasivePipsMods"].ToObject<EvasivePipsMods>();
        //  defTemp.Remove("evasivePipsMods");
        //}
        //if (defTemp["APCriticalChanceMultiplier"] != null) {
        //  extAmmoDef.APCriticalChanceMultiplier = (float)defTemp["APCriticalChanceMultiplier"];
        //  defTemp.Remove("APCriticalChanceMultiplier");
        //}
        //if (defTemp["APArmorShardsMod"] != null) {
        //  extAmmoDef.APArmorShardsMod = (float)defTemp["APArmorShardsMod"];
        //  defTemp.Remove("APArmorShardsMod");
        //}
        //if (defTemp["APMaxArmorThickness"] != null) {
        //  extAmmoDef.APMaxArmorThickness = (float)defTemp["APMaxArmorThickness"];
        //  defTemp.Remove("APMaxArmorThickness");
        //}
        //if (defTemp["MinRange"] != null) {
        //  extAmmoDef.MinRange = (float)defTemp["MinRange"];
        //  defTemp.Remove("MinRange");
        //}
        //if (defTemp["MaxRange"] != null) {
        //  extAmmoDef.MaxRange = (float)defTemp["MaxRange"];
        //  defTemp.Remove("MaxRange");
        //}
        //if (defTemp["DamageFalloffStartDistance"] != null) {
        //  extAmmoDef.DamageFalloffStartDistance = (float)defTemp["DamageFalloffStartDistance"];
        //  defTemp.Remove("DamageFalloffStartDistance");
        //}
        //if (defTemp["DamageFalloffEndDistance"] != null) {
        //  extAmmoDef.DamageFalloffEndDistance = (float)defTemp["DamageFalloffEndDistance"];
        //  defTemp.Remove("DamageFalloffEndDistance");
        //}
        //if (defTemp["ShortRange"] != null) {
        //  extAmmoDef.ShortRange = (float)defTemp["ShortRange"];
        //  defTemp.Remove("ShortRange");
        //}
        //if (defTemp["MediumRange"] != null) {
        //  extAmmoDef.MediumRange = (float)defTemp["MediumRange"];
        //  defTemp.Remove("MediumRange");
        //}
        //if (defTemp["LongRange"] != null) {
        //  extAmmoDef.LongRange = (float)defTemp["LongRange"];
        //  defTemp.Remove("LongRange");
        //}
        //if (defTemp["ForbiddenRange"] != null) {
        //  extAmmoDef.ForbiddenRange = (float)defTemp["ForbiddenRange"];
        //  defTemp.Remove("ForbiddenRange");
        //}
        //if (defTemp["HideIfOnlyVariant"] != null) {
        //  extAmmoDef.HideIfOnlyVariant = (bool)defTemp["HideIfOnlyVariant"];
        //  defTemp.Remove("HideIfOnlyVariant");
        //}
        //if (defTemp["ProjectileScale"] != null) {
        //  extAmmoDef.ProjectileScale = defTemp["ProjectileScale"].ToObject<CustomVector>();
        //  defTemp.Remove("ProjectileScale");
        //}
        //if (defTemp["MissileExplosionScale"] != null) {
        //  extAmmoDef.MissileExplosionScale = defTemp["MissileExplosionScale"].ToObject<CustomVector>();
        //  defTemp.Remove("MissileExplosionScale");
        //}
        //if (defTemp["CanBeExhaustedAt"] != null) {
        //  extAmmoDef.CanBeExhaustedAt = (float)defTemp["CanBeExhaustedAt"];
        //  defTemp.Remove("CanBeExhaustedAt");
        //}
        //if (defTemp["MineFieldRadius"] != null) {
        //  extAmmoDef.MineField.InstallCellRange = (int)defTemp["MineFieldRadius"];
        //  defTemp.Remove("MineFieldRadius");
        //}
        //if (defTemp["ClearMineFieldRadius"] != null) {
        //  extAmmoDef.ClearMineFieldRadius = (int)defTemp["ClearMineFieldRadius"];
        //  defTemp.Remove("ClearMineFieldRadius");
        //}
        if(defTemp["ammoOnlyBoxes"] != null) {
          extAmmoDef.ammoOnlyBoxes = new HashSet<string>();
          JArray boxes = defTemp["ammoOnlyBoxes"] as JArray;
          if(boxes != null) {
            foreach(string box in boxes) { extAmmoDef.ammoOnlyBoxes.Add(box); }
          }
        }
        if (defTemp["AvailableOnPlanet"] != null) {
          extAmmoDef.AvailableOnPlanet = new HashSet<string>();
          JArray tags = defTemp["AvailableOnPlanet"] as JArray;
          if (tags != null) {
            foreach (string tag in tags) { extAmmoDef.AvailableOnPlanet.Add(tag); }
          }
        }
        if (extAmmoDef.MineField == null) { extAmmoDef.MineField = new MineFieldDef(); }
        if (defTemp["MineFieldHitChance"] != null) {
          extAmmoDef.MineField.Chance = (float)defTemp["MineFieldHitChance"];
          defTemp.Remove("MineFieldHitChance");
        }
        if (defTemp["MineFieldDamage"] != null) {
          extAmmoDef.MineField.Damage = (float)defTemp["MineFieldDamage"];
          defTemp.Remove("MineFieldDamage");
        }
        if (defTemp["MineFieldVFX"] != null) {
          extAmmoDef.MineField.VFXprefab = (string)defTemp["MineFieldVFX"];
          defTemp.Remove("MineFieldVFX");
        }
        if (defTemp["MineFieldSFX"] != null) {
          extAmmoDef.MineField.SFX = (string)defTemp["MineFieldSFX"];
          defTemp.Remove("MineFieldSFX");
        }
        if (defTemp["MineFieldFXMinRange"] != null) {
          extAmmoDef.MineField.VFXMinDistance = (float)defTemp["MineFieldFXMinRange"];
          if (extAmmoDef.MineField.VFXMinDistance < 20f) { extAmmoDef.MineField.VFXMinDistance = 20f; };
          defTemp.Remove("MineFieldFXMinRange");
        }
        //if (defTemp["BallisticDamagePerPallet"] != null) {
        //  extAmmoDef.BallisticDamagePerPallet = ((bool)defTemp["BallisticDamagePerPallet"] == true) ? TripleBoolean.True : TripleBoolean.False;
        //  defTemp.Remove("BallisticDamagePerPallet");
        //}
        //if (defTemp["AOEEffectsFalloff"] != null) {
        //  extAmmoDef.AOEEffectsFalloff = ((bool)defTemp["AOEEffectsFalloff"] == true) ? TripleBoolean.True : TripleBoolean.False;
        //  defTemp.Remove("AOEEffectsFalloff");
        //}
        //if (defTemp["CantHitUnaffecedByPathing"] != null) {
        //  extAmmoDef.CantHitUnaffecedByPathing = ((bool)defTemp["CantHitUnaffecedByPathing"] == true) ? TripleBoolean.True : TripleBoolean.False;
        //  defTemp.Remove("CantHitUnaffecedByPathing");
        //}
        //if (defTemp["AdditionalImpactVFXScaleX"] != null) {
        //  extAmmoDef.AdditionalImpactVFXScaleX = (float)defTemp["AdditionalImpactVFXScaleX"];
        //  defTemp.Remove("AdditionalImpactVFXScaleX");
        //}
        //if (defTemp["AdditionalImpactVFXScaleY"] != null) {
        //  extAmmoDef.AdditionalImpactVFXScaleY = (float)defTemp["AdditionalImpactVFXScaleY"];
        //  defTemp.Remove("AdditionalImpactVFXScaleY");
        //}
        //if (defTemp["AdditionalImpactVFXScaleZ"] != null) {
        //  extAmmoDef.AdditionalImpactVFXScaleZ = (float)defTemp["AdditionalImpactVFXScaleZ"];
        //  defTemp.Remove("AdditionalImpactVFXScaleZ");
        //}
        //if (defTemp["LongVFXOnImpactScaleX"] != null) {
        //  extAmmoDef.LongVFXOnImpactScaleX = (float)defTemp["LongVFXOnImpactScaleX"];
        //  defTemp.Remove("LongVFXOnImpactScaleX");
        //}
        //if (defTemp["LongVFXOnImpactScaleY"] != null) {
        //  extAmmoDef.LongVFXOnImpactScaleX = (float)defTemp["LongVFXOnImpactScaleY"];
        //  defTemp.Remove("LongVFXOnImpactScaleY");
        //}
        //if (defTemp["LongVFXOnImpactScaleZ"] != null) {
        //  extAmmoDef.LongVFXOnImpactScaleX = (float)defTemp["LongVFXOnImpactScaleZ"];
        //  defTemp.Remove("LongVFXOnImpactScaleZ");
        //}
        //if (defTemp["tempDesignMaskCellRadius"] != null) {
        //  extAmmoDef.tempDesignMaskCellRadius = (int)defTemp["tempDesignMaskCellRadius"];
        //  defTemp.Remove("tempDesignMaskCellRadius");
        //}
        //if (defTemp["MineFieldHeat"] != null) {
        //  extAmmoDef.MineField.Heat = (float)defTemp["MineFieldHeat"];
        //  defTemp.Remove("MineFieldHeat");
        //}
        //if (defTemp["FireTerrainChance"] != null) {
        //  extAmmoDef.FireTerrainChance = (float)defTemp["FireTerrainChance"];
        //  defTemp.Remove("FireTerrainChance");
        //}
        //if (defTemp["FireDurationWithoutForest"] != null) {
        //  extAmmoDef.FireDurationWithoutForest = (int)defTemp["FireDurationWithoutForest"];
        //  defTemp.Remove("FireDurationWithoutForest");
        //}
        //if (defTemp["FireTerrainStrength"] != null) {
        //  extAmmoDef.FireTerrainStrength = (int)defTemp["FireTerrainStrength"];
        //  defTemp.Remove("FireTerrainStrength");
        //}
        //if (defTemp["FireTerrainCellRadius"] != null) {
        //  extAmmoDef.FireTerrainCellRadius = (int)defTemp["FireTerrainCellRadius"];
        //  defTemp.Remove("FireTerrainCellRadius");
        //}
        //if (defTemp["MissileVolleySize"] != null) {
        //  extAmmoDef.MissileVolleySize = (int)defTemp["MissileVolleySize"];
        //  defTemp.Remove("MissileVolleySize");
        //}
        //if (defTemp["AdditionalImpactVFX"] != null) {
        //  extAmmoDef.AdditionalImpactVFX = (string)defTemp["AdditionalImpactVFX"];
        //  defTemp.Remove("AdditionalImpactVFX");
        //}
        //if (defTemp["FireOnSuccessHit"] != null) {
        //  extAmmoDef.FireOnSuccessHit = ((bool)defTemp["FireOnSuccessHit"] == true) ? TripleBoolean.True : TripleBoolean.False;
        //  defTemp.Remove("FireOnSuccessHit");
        //}
        //if (defTemp["ColorsTable"] != null) {
        //  extAmmoDef.ColorsTable = defTemp["ColorsTable"].ToObject<List<ColorTableJsonEntry>>();
        //  defTemp.Remove("ColorsTable");
        //}
        //if (defTemp["isDamageVariation"] != null) {
        //  extAmmoDef.isDamageVariation = ((bool)defTemp["isDamageVariation"] == true) ? TripleBoolean.True : TripleBoolean.False;
        //  defTemp.Remove("isDamageVariation");
        //}
        //if (defTemp["isHeatVariation"] != null) {
        //  extAmmoDef.isHeatVariation = ((bool)defTemp["isHeatVariation"] == true) ? TripleBoolean.True : TripleBoolean.False;
        //  defTemp.Remove("isHeatVariation");
        //}
        //if (defTemp["isStabilityVariation"] != null) {
        //  extAmmoDef.isStabilityVariation = ((bool)defTemp["isStabilityVariation"] == true) ? TripleBoolean.True : TripleBoolean.False;
        //  defTemp.Remove("isStabilityVariation");
        //}
        //if (defTemp["Streak"] != null) {
        //  extAmmoDef.Streak = ((bool)defTemp["Streak"] == true) ? TripleBoolean.True : TripleBoolean.False;
        //  defTemp.Remove("Streak");
        //}
        //if (defTemp["MineFieldCount"] != null) {
        //  extAmmoDef.MineField.Count = (int)defTemp["MineFieldCount"];
        //  defTemp.Remove("MineFieldCount");
        //}
        //if (defTemp["MineFieldInstability"] != null) {
        //  extAmmoDef.MineField.Instability = (float)defTemp["MineFieldInstability"];
        //  defTemp.Remove("MineFieldInstability");
        //}
        //if (defTemp["IFFDef"] != null) {
        //  extAmmoDef.IFFDef = (string)defTemp["IFFDef"];
        //  defTemp.Remove("IFFDef");
        //}
        //if (defTemp["AdditionalAudioEffect"] != null) {
        //  extAmmoDef.AdditionalAudioEffect = (string)defTemp["AdditionalAudioEffect"];
        //  defTemp.Remove("AdditionalAudioEffect");
        //}

        /*if (defTemp["SurfaceImpactDesignMaskId"] != null) {
          extAmmoDef.SurfaceImpactDesignMaskId = JsonConvert.DeserializeObject<Dictionary<TerrainMaskFlags, string>>(defTemp["SurfaceImpactDesignMaskId"].ToString());
          foreach (var sdm in extAmmoDef.SurfaceImpactDesignMaskId) {
            CustomAmmoCategoriesLog.Log.LogWrite(" TerrainFlag:"+sdm.Key+" Mask:"+sdm.Value+"\n");
          }
          defTemp.Remove("SurfaceImpactDesignMaskId");
        }*/
        //if (defTemp["AOERange"] != null) {
        //  extAmmoDef.AOERange = (float)defTemp["AOERange"];
        //  defTemp.Remove("AOERange");
        //}
        //if (defTemp["SpreadRange"] != null) {
        //  extAmmoDef.SpreadRange = (float)defTemp["SpreadRange"];
        //  defTemp.Remove("SpreadRange");
        //}
        //if (defTemp["UnseparatedDamageMult"] != null) {
        //  extAmmoDef.UnseparatedDamageMult = (float)defTemp["UnseparatedDamageMult"];
        //  defTemp.Remove("UnseparatedDamageMult");
        //}
        //if (defTemp["RefireModifier"] != null) {
        //  extAmmoDef.RefireModifier = (int)defTemp["RefireModifier"];
        //  defTemp.Remove("RefireModifier");
        //}
        //if (defTemp["Instability"] != null) {
        //  extAmmoDef.Instability = (float)defTemp["Instability"];
        //  defTemp.Remove("Instability");
        //}
        //if (defTemp["AttackRecoil"] != null) {
        //  extAmmoDef.AttackRecoil = (int)defTemp["AttackRecoil"];
        //  defTemp.Remove("AttackRecoil");
        //}
        //if (defTemp["WeaponEffectID"] != null) {
        //  extAmmoDef.WeaponEffectID = (string)defTemp["WeaponEffectID"];
        //  defTemp.Remove("WeaponEffectID");
        //}
        //if (defTemp["tempDesignMaskOnImpact"] != null) {
        //  extAmmoDef.tempDesignMaskOnImpact = (string)defTemp["tempDesignMaskOnImpact"];
        //  defTemp.Remove("tempDesignMaskOnImpact");
        //}
        //if (defTemp["LongVFXOnImpact"] != null) {
        //  extAmmoDef.LongVFXOnImpact = (string)defTemp["LongVFXOnImpact"];
        //  defTemp.Remove("LongVFXOnImpact");
        //}
        //if (defTemp["tempDesignMaskOnImpactTurns"] != null) {
        //  extAmmoDef.tempDesignMaskOnImpactTurns = (int)defTemp["tempDesignMaskOnImpactTurns"];
        //  defTemp.Remove("tempDesignMaskOnImpactTurns");
        //}
        //if (defTemp["EvasivePipsIgnored"] != null) {
        //  extAmmoDef.EvasivePipsIgnored = (float)defTemp["EvasivePipsIgnored"];
        //  defTemp.Remove("EvasivePipsIgnored");
        //}
        //if (defTemp["FlatJammingChance"] != null) {
        //  extAmmoDef.FlatJammingChance = (float)defTemp["FlatJammingChance"];
        //  defTemp.Remove("FlatJammingChance");
        //}
        //if (defTemp["AMSHitChance"] != null) {
        //  extAmmoDef.AMSHitChance = (float)defTemp["AMSHitChance"];
        //  defTemp.Remove("AMSHitChance");
        //}
        if (defTemp["IsAMS"] != null) {
          extAmmoDef.IsAMS = ((bool)defTemp["IsAMS"] == true) ? TripleBoolean.True : TripleBoolean.False;
          defTemp.Remove("IsAMS");
        }
        if (defTemp["IsAAMS"] != null) {
          extAmmoDef.IsAAMS = ((bool)defTemp["IsAAMS"] == true) ? TripleBoolean.True : TripleBoolean.False;
          if (extAmmoDef.IsAAMS == TripleBoolean.True) {
            extAmmoDef.IsAMS = TripleBoolean.True;
          }
          defTemp.Remove("IsAAMS");
        }
        //if (defTemp["AOEDamage"] != null) {
        //  extAmmoDef.AOEDamage = (float)defTemp["AOEDamage"];
        //  defTemp.Remove("AOEDamage");
        //}
        //if (defTemp["AOEHeatDamage"] != null) {
        //  extAmmoDef.AOEHeatDamage = (float)defTemp["AOEHeatDamage"];
        //  defTemp.Remove("AOEHeatDamage");
        //}
        //if (defTemp["AOEInstability"] != null) {
        //  extAmmoDef.AOEInstability = (float)defTemp["AOEInstability"];
        //  defTemp.Remove("AOEInstability");
        //}
        //if (defTemp["AMSHitChance"] != null) {
        //  extAmmoDef.AMSHitChance = (float)defTemp["AMSHitChance"];
        //  defTemp.Remove("AMSHitChance");
        //}
        //if (defTemp["ArmorDamageModifier"] != null) {
        //  extAmmoDef.ArmorDamageModifier = (float)defTemp["ArmorDamageModifier"];
        //}
        //if (defTemp["ISDamageModifier"] != null) {
        //  extAmmoDef.ISDamageModifier = (float)defTemp["ISDamageModifier"];
        //}
        //if (defTemp["GunneryJammingBase"] != null) {
        //  extAmmoDef.GunneryJammingBase = (float)defTemp["GunneryJammingBase"];
        //  defTemp.Remove("GunneryJammingBase");
        //}
        //if (defTemp["GunneryJammingMult"] != null) {
        //  extAmmoDef.GunneryJammingMult = (float)defTemp["GunneryJammingMult"];
        //  defTemp.Remove("GunneryJammingMult");
        //}
        //if (defTemp["IndirectFireCapable"] != null) {
        //  extAmmoDef.IndirectFireCapable = ((bool)defTemp["IndirectFireCapable"] == true) ? TripleBoolean.True : TripleBoolean.False;
        //  defTemp.Remove("IndirectFireCapable");
        //}
        //if (defTemp["HasShells"] != null) {
        //  extAmmoDef.HasShells = ((bool)defTemp["HasShells"] == true) ? TripleBoolean.True : TripleBoolean.False;
        //  defTemp.Remove("HasShells");
        //}
        //if (defTemp["ShellsRadius"] != null) {
        //  extAmmoDef.ShellsRadius = (float)defTemp["ShellsRadius"];
        //  defTemp.Remove("ShellsRadius");
        //}
        //if (defTemp["MinShellsDistance"] != null) {
        //  extAmmoDef.MinShellsDistance = (float)defTemp["MinShellsDistance"];
        //  defTemp.Remove("MinShellsDistance");
        //}
        //if (defTemp["MaxShellsDistance"] != null) {
        //  extAmmoDef.MaxShellsDistance = (float)defTemp["MaxShellsDistance"];
        //  defTemp.Remove("MaxShellsDistance");
        //}
        //if (defTemp["DirectFireModifier"] != null) {
        //  extAmmoDef.DirectFireModifier = (float)defTemp["DirectFireModifier"];
        //  defTemp.Remove("DirectFireModifier");
        //}
        //if (defTemp["DistantVariance"] != null) {
        //  extAmmoDef.DistantVariance = (float)defTemp["DistantVariance"];
        //  defTemp.Remove("DistantVariance");
        //}
        //if (defTemp["DamageNotDivided"] != null) {
        //  extAmmoDef.DamageNotDivided = ((bool)defTemp["DamageNotDivided"] == true) ? TripleBoolean.True : TripleBoolean.False;
        //  defTemp.Remove("DamageNotDivided");
        //}
        //if (defTemp["AlwaysIndirectVisuals"] != null) {
        //  extAmmoDef.AlwaysIndirectVisuals = ((bool)defTemp["AlwaysIndirectVisuals"] == true) ? TripleBoolean.True : TripleBoolean.False;
        //  defTemp.Remove("AlwaysIndirectVisuals");
        //}
        //if (defTemp["Unguided"] != null) {
        //  extAmmoDef.Unguided = ((bool)defTemp["Unguided"] == true) ? TripleBoolean.True : TripleBoolean.False;
        //  defTemp.Remove("Unguided");
        //}
        //if (defTemp["AOECapable"] != null) {
        //  extAmmoDef.AOECapable = ((bool)defTemp["AOECapable"] == true) ? TripleBoolean.True : TripleBoolean.False;
        //  defTemp.Remove("AOECapable");
        //}
        //if (defTemp["preFireSFX"] != null) {
        //  extAmmoDef.preFireSFX = (string)defTemp["preFireSFX"];
        //  defTemp.Remove("preFireSFX");
        //}
        //if (defTemp["DamageMultiplier"] != null) {
        //  extAmmoDef.DamageMultiplier = (float)defTemp["DamageMultiplier"];
        //  defTemp.Remove("DamageMultiplier");
        //}
        //if (defTemp["MinMissRadius"] != null) {
        //  extAmmoDef.MinMissRadius = (float)defTemp["MinMissRadius"];
        //  defTemp.Remove("MinMissRadius");
        //}
        //if (defTemp["MaxMissRadius"] != null) {
        //  extAmmoDef.MaxMissRadius = (float)defTemp["MaxMissRadius"];
        //  defTemp.Remove("MaxMissRadius");
        //}
        //if (defTemp["APDamageMultiplier"] != null) {
        //  extAmmoDef.APDamageMultiplier = (float)defTemp["APDamageMultiplier"];
        //  defTemp.Remove("APDamageMultiplier");
        //}
        //if (defTemp["HeatMultiplier"] != null) {
        //  extAmmoDef.HeatMultiplier = (float)defTemp["HeatMultiplier"];
        //  defTemp.Remove("HeatMultiplier");
        //}
        //if (defTemp["InstabilityMultiplier"] != null) {
        //  extAmmoDef.InstabilityMultiplier = (float)defTemp["InstabilityMultiplier"];
        //  defTemp.Remove("InstabilityMultiplier");
        //}
        //if (defTemp["DistantVarianceReversed"] != null) {
        //  extAmmoDef.DistantVarianceReversed = ((bool)defTemp["DistantVarianceReversed"] == true) ? TripleBoolean.True : TripleBoolean.False;
        //  defTemp.Remove("DistantVarianceReversed");
        //}
        //if (defTemp["DamageVariance"] != null) {
        //  extAmmoDef.DamageVariance = (float)defTemp["DamageVariance"];
        //  defTemp.Remove("DamageVariance");
        //}
        //if (defTemp["AMSImmune"] != null) {
        //  extAmmoDef.AMSImmune = ((bool)defTemp["AMSImmune"] == true) ? TripleBoolean.True : TripleBoolean.False;
        //  defTemp.Remove("AMSImmune");
        //}
        //if (defTemp["AMSDamage"] != null) {
        //  extAmmoDef.AMSDamage = (float)defTemp["AMSDamage"];
        //  defTemp.Remove("AMSDamage");
        //}
        //if (defTemp["MissileHealth"] != null) {
        //  extAmmoDef.MissileHealth = (float)defTemp["MissileHealth"];
        //  defTemp.Remove("MissileHealth");
        //}
        //if (defTemp["RangedDmgFalloffType"] != null) {
        //  extAmmoDef.RangedDmgFalloffType = (DamageFalloffType)Enum.Parse(typeof(DamageFalloffType), (string)defTemp["RangedDmgFalloffType"]);
        //  defTemp.Remove("RangedDmgFalloffType");
        //}
        //if (defTemp["AoEDmgFalloffType"] != null) {
        //  extAmmoDef.AoEDmgFalloffType = (DamageFalloffType)Enum.Parse(typeof(DamageFalloffType), (string)defTemp["AoEDmgFalloffType"]);
        //  defTemp.Remove("AoEDmgFalloffType");
        //}
        if (defTemp["ChassisTagsAccuracyModifiers"] != null) {
          extAmmoDef.TagsAccuracyModifiers = JsonConvert.DeserializeObject<Dictionary<string, float>>(defTemp["ChassisTagsAccuracyModifiers"].ToString());
          Log.LogWrite((string)defTemp["Description"]["Id"] + " ChassisTagsAccuracyModifiers:\n");
          foreach (var tam in extAmmoDef.TagsAccuracyModifiers) {
            Log.LogWrite(" " + tam.Key + ":" + tam.Key);
          }
          defTemp.Remove("ChassisTagsAccuracyModifiers");
        }
        //if (defTemp["HitGenerator"] != null) {
        //  try {
        //    extAmmoDef.HitGenerator = (HitGeneratorType)Enum.Parse(typeof(HitGeneratorType), (string)defTemp["HitGenerator"], true);
        //  } catch (Exception) {
        //    extAmmoDef.HitGenerator = HitGeneratorType.NotSet;
        //  }
        //  defTemp.Remove("HitGenerator");
        //}
        Log.M.TWL(0, "extAmmoDef:"+ extAmmoDef.Id);
        foreach (PropertyInfo prop in typeof(ExtAmmunitionDef).GetProperties()) {
          if (defTemp[prop.Name] == null) { continue; }
          Log.M.WL(1, "property:" + prop.Name+" "+ prop.PropertyType.ToString());
          if (prop.PropertyType == typeof(float)) {
            prop.SetValue(extAmmoDef, (float)defTemp[prop.Name]);
            Log.M.WL(2, "float:"+prop.GetValue(extAmmoDef)+" "+ defTemp[prop.Name]);
          } else if (prop.PropertyType == typeof(int)) {
            prop.SetValue(extAmmoDef, (int)defTemp[prop.Name]);
          } else if (prop.PropertyType == typeof(string)) {
            prop.SetValue(extAmmoDef, (string)defTemp[prop.Name]);
          } else if (prop.PropertyType == typeof(TripleBoolean)) {
            prop.SetValue(extAmmoDef, ((bool)defTemp[prop.Name] == true) ? TripleBoolean.True : TripleBoolean.False);
          } else if (prop.PropertyType == typeof(bool)) {
            prop.SetValue(extAmmoDef, (bool)defTemp[prop.Name]);
          } else if (prop.PropertyType == typeof(EvasivePipsMods)) {
            prop.SetValue(extAmmoDef, defTemp[prop.Name].ToObject<EvasivePipsMods>());
          } else if (prop.PropertyType == typeof(CustomVector)) {
            prop.SetValue(extAmmoDef, defTemp[prop.Name].ToObject<CustomVector>());
          } else if (prop.PropertyType.IsEnum) {
            prop.SetValue(extAmmoDef, Enum.Parse(prop.PropertyType, (string)defTemp[prop.Name]));
          } else { continue; }
          defTemp.Remove(prop.Name);
        }
        if (defTemp["MineField"] != null) {
          extAmmoDef.MineField.fromJSON(defTemp["MineField"]);
          defTemp.Remove("MineField");
        }
        if (defTemp["deferredEffect"] != null) {
          extAmmoDef.deferredEffect = JsonConvert.DeserializeObject<DeferredEffectDef>(defTemp["deferredEffect"].ToString());
          if (defTemp["deferredEffect"]["statusEffects"] != null) {
            extAmmoDef.deferredEffect.ParceEffects(defTemp["deferredEffect"]["statusEffects"].ToString());
          }
          defTemp.Remove("deferredEffect");
        }
        if (defTemp["statusEffects"] != null) {
          if (defTemp["statusEffects"].Type == JTokenType.Array) {
            List<EffectData> tmpList = new List<EffectData>();
            JToken statusEffects = defTemp["statusEffects"];
            foreach (JObject statusEffect in statusEffects) {
              EffectData effect = new EffectData();
              JSONSerializationUtility.FromJSON<EffectData>(effect, statusEffect.ToString());
              tmpList.Add(effect);
            }
            extAmmoDef.statusEffects = tmpList.ToArray();
          }
          //extAmmoDef.statusEffects = JsonConvert.DeserializeObject<EffectData[]>(defTemp["statusEffects"].ToString());
          //JSONSerializationUtility.FromJSON<EffectData[]>(extAmmoDef.statusEffects, defTemp["statusEffects"].ToString());
          //CustomAmmoCategoriesLog.Log.LogWrite(JsonConvert.SerializeObject(extAmmoDef.statusEffects)+"\n");
          defTemp.Remove("statusEffects");
        }
        //CustomAmmoCategories.RegisterExtAmmoDef((string)defTemp["Description"]["Id"], extAmmoDef);
        json = defTemp.ToString(Formatting.Indented);
        //Log.LogWrite(json + "\n");
        CustomAmmoCategoriesLog.Log.LogWrite("\n--------------RESULT----------------\n" + JsonConvert.SerializeObject(extAmmoDef, Formatting.Indented) + "\n----------------------------------\n");
        __state.extDef = extAmmoDef;
      } catch (Exception e) {
        __state.errorStr = e.ToString();
      }
      return true;
    }
    public static HashSet<string> ammunitions(string AmmoCategoryName) {
      if (ammunitionsDefs.ContainsKey(AmmoCategoryName)) { return ammunitionsDefs[AmmoCategoryName]; }
      return new HashSet<string>();
    }
    public static bool isDefaultAmmo(this CustomAmmoCategory cat) {
      return defaultAmmunitions.ContainsKey(cat.Id);
    }
    private static Dictionary<string, HashSet<WeaponDef>> toDefaultAmmoUpdate = new Dictionary<string, HashSet<WeaponDef>>();
    private static SpinLock toDefaultAmmoUpdate_spinlock = new SpinLock();
    public static void UpdateDefaultAmmo(this CustomAmmoCategory cat) {
      bool locked = false;
      try {
        toDefaultAmmoUpdate_spinlock.Enter(ref locked);
        if (toDefaultAmmoUpdate.TryGetValue(cat.Id, out HashSet<WeaponDef> wDefs)) {
          foreach (WeaponDef wDef in wDefs) {
            if (wDef.StartingAmmoCapacity != 0) {
              ExtAmmunitionDef extAmmunition = cat.defaultAmmo();
              ExtWeaponDef extWeapon = wDef.exDef();
              if (extWeapon.InternalAmmo.ContainsKey(extAmmunition.Id) == false) { extWeapon.InternalAmmo.Add(extAmmunition.Id, wDef.StartingAmmoCapacity); }
              Traverse.Create(wDef).Property<int>("StartingAmmoCapacity").Value = 0;
            }
          }
          toDefaultAmmoUpdate.Remove(cat.Id);
        }
      } catch(Exception e){
        if (locked) { toDefaultAmmoUpdate_spinlock.Exit(); locked = false; }
        throw e;
      }
      if (locked) { toDefaultAmmoUpdate_spinlock.Exit(); locked = false; }
    }
    public static void RegisterForDefaultAmmoUpdate(this WeaponDef weaponDef, CustomAmmoCategory cat) {
      if(toDefaultAmmoUpdate.TryGetValue(cat.Id, out HashSet<WeaponDef> wDefs) == false) {
        wDefs = new HashSet<WeaponDef>();
        toDefaultAmmoUpdate.Add(cat.Id, wDefs);
      }
      wDefs.Add(weaponDef);
    }
    public static ExtAmmunitionDef defaultAmmo(this CustomAmmoCategory cat) {
      if (defaultAmmunitions.ContainsKey(cat.Id)) { return defaultAmmunitions[cat.Id]; };
      return CustomAmmoCategories.DefaultAmmo;
    }
    public static void Postfix(AmmunitionDef __instance, ref ExtDefinitionParceInfo __state) {
      if (__instance == null) { Log.M.TWL(0, "!WARNINIG! weaponDef is null. Very very wrong!", true); return; }
      try {
        ExtAmmunitionDef extAmmoDef = null;
        if (__state == null) {
          Log.M.TWL(0, "!WARNINIG! ExtDefinitionParceInfo is null for " + __instance.Description.Id + ". Very very wrong!", true);
        } else {
          extAmmoDef = __state.extDef as ExtAmmunitionDef;
        }
        __instance.setOriginal(__state.baseJson);
        if (extAmmoDef == null) {
          Log.M.TWL(0, "!WARNINIG! ext. definition parce error for " + __instance.Description.Id + "\n" + __state.errorStr + "\n" + __state.baseJson, true);
          extAmmoDef = new ExtAmmunitionDef();
          extAmmoDef.Id = __instance.Description.Id;
          extAmmoDef.Name = __instance.Description.Name;
          extAmmoDef.UIName = __instance.Description.UIName;
          extAmmoDef.AmmoCategory = CustomAmmoCategories.find(__instance.AmmoCategoryValue.Name);
        }
        EffectData[] effects = extAmmoDef.statusEffects;
        List<EffectData> tmpList = new List<EffectData>();
        CustomAmmoCategoriesLog.Log.LogWrite("Checking on null status effects " + __instance.Description.Id + " " + effects.Length + ".\n");
        foreach (EffectData effect in effects) {
          if ((effect.Description != null)) {
            if ((effect.Description.Id != null) && (effect.Description.Name != null)) {
              tmpList.Add(effect);
              continue;
            } else {
              if (effect.Description.Id == null) {
                CustomAmmoCategoriesLog.Log.LogWrite("!Warning! effect id is null " + __instance.Description.Id + ".\n");
              }
              if (effect.Description.Name == null) {
                CustomAmmoCategoriesLog.Log.LogWrite("!Warning! effect name is null " + __instance.Description.Id + ".\n");
              }
            }
          } else {
            CustomAmmoCategoriesLog.Log.LogWrite("!Warning! effect description is null " + __instance.Description.Id + ".\n");
          }
          CustomAmmoCategoriesLog.Log.LogWrite("!Warning! null status effect detected at ammo " + __instance.Description.Id + ".\n");
        }
        if (tmpList.Count != effects.Length) {
          CustomAmmoCategoriesLog.Log.LogWrite("!Warning! null (" + (effects.Length - tmpList.Count) + "/" + effects.Length + ") status effects detected at ammo " + __instance.Description.Id + ".Removing\n");
          extAmmoDef.statusEffects = tmpList.ToArray();
        }
        if (defaultAmmunitions.ContainsKey(extAmmoDef.AmmoCategory.Id) == false) {
          defaultAmmunitions.TryAdd(extAmmoDef.AmmoCategory.Id, extAmmoDef);
          extAmmoDef.AmmoCategory.UpdateDefaultAmmo();
        };
        CustomAmmoCategories.RegisterExtAmmoDef(extAmmoDef.Id, extAmmoDef);
        if (__instance.AmmoCategoryValue != null) {
          bool locked = false;
          try {
            ammunitionsDefs_lock.Enter(ref locked);
            if (ammunitionsDefs.ContainsKey(__instance.AmmoCategoryValue.Name) == false) { ammunitionsDefs.Add(__instance.AmmoCategoryValue.Name, new HashSet<string>()); }
            if (ammunitionsDefs[__instance.AmmoCategoryValue.Name].Contains(__instance.Description.Id) == false) { ammunitionsDefs[__instance.AmmoCategoryValue.Name].Add(__instance.Description.Id); }
          }catch(Exception e) {
            if (locked) { ammunitionsDefs_lock.Exit(); locked = false; }
            throw e;
          }
          if (locked) { ammunitionsDefs_lock.Exit(); locked = false; }
        }
        if(extAmmoDef.AutoRefill != AutoRefilType.Automatic) {
          extAmmoDef.ammoOnlyBoxes.Add(__instance.getGenericBox());
        } else {
          extAmmoDef.ammoOnlyBoxes.Clear();
        }
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
    }
  }
}
