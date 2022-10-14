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
      return weapon.info().avaibleModes();
      //List<WeaponMode> result = new List<WeaponMode>();
      //ExtWeaponDef extWeapon = weapon.exDef();
      //foreach(var mode in extWeapon.Modes) {
      //  if (mode.Value.Lock.isAvaible(weapon)) { result.Add(mode.Value); };
      //}
      //return result;
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
  [MessagePackObject]
  public class ModeLockSetting {
    [Key(0)]
    public float Low { get; set; }
    [Key(1)]
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
  [MessagePackObject]
  public class ModeLockSettings {
    [Key(0)]
    public ModeLockSetting HeatLevel { get; set; }
    [Key(1)]
    public ModeLockSetting OverheatLevel { get; set; }
    [Key(2)]
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
  [SelfDocumentedClass("Weapons", "Weapons", "WeaponMode"), MessagePackObject]
  public class WeaponMode {
    public static string BASE_MODE_NAME = "B";
    public static string NONE_MODE_NAME = "!NONE!";
    [Key(0)]
    public string UIName { get; set; } = WeaponMode.BASE_MODE_NAME;
    [Key(1)]
    public string Id { get; set; } = WeaponMode.NONE_MODE_NAME;
    [Key(2)]
    public string Name { get; set; } = WeaponMode.BASE_MODE_NAME;
    [Key(3)]
    public string Description { get; set; } = string.Empty;
    [Key(4)]
    public float AccuracyModifier { get; set; } = 0f;
    [Key(5)]
    public float DirectFireModifier { get; set; } = 0f;
    [Key(6)]
    public float DamagePerShot { get; set; } = 0f;
    [Key(7)]
    public float HeatDamagePerShot { get; set; } = 0f;
    [Key(8)]
    public int HeatGenerated { get; set; } = 0;
    [Key(9)]
    public float CriticalChanceMultiplier { get; set; } = 0f;
    [Key(10)]
    public int ShotsWhenFired { get; set; } = 0;
    [Key(11)]
    public float ShotsWhenFiredMod { get; set; } = 1f;
    [SkipDocumentation, IgnoreMember]
    public int AIBattleValue { get; set; } = 100;
    [Key(12)]
    public int ProjectilesPerShot { get; set; } = 0;
    [SelfDocumentationTypeName("List of statusEffects"), SelfDocumentationDefaultValue("empty"), Key(13)]
    public List<EffectData> statusEffects { get; set; } = new List<EffectData>();
    [Key(14)]
    public float MinRange { get; set; } = 0f;
    [Key(15)]
    public float MaxRange { get; set; } = 0f;
    [Key(16)]
    public float LongRange { get; set; } = 0f;
    [Key(17)]
    public float ShortRange { get; set; } = 0f;
    [Key(18)]
    public float ForbiddenRange { get; set; } = 0f;
    [Key(19)]
    public float MediumRange { get; set; } = 0f;
    [Key(20)]
    public int RefireModifier { get; set; } = 0;
    [Key(21)]
    public int AttackRecoil { get; set; } = 0;
    [Key(22)]
    public int Cooldown { get; set; } = 0;
    [SkipDocumentation, Key(23)]
    public float AIHitChanceCap { get; set; }
    [Key(24)]
    public float Instability { get; set; } = 0f;
    [Key(25)]
    public float FlatJammingChance { get; set; } = 0f;
    [Key(26)]
    public float GunneryJammingBase { get; set; } = 0f;
    [Key(27)]
    public float GunneryJammingMult { get; set; } = 0f;
    [Key(28)]
    public float DistantVariance { get; set; } = 0f;
    [Key(29)]
    public float SpreadRange { get; set; } = 0f;
    [Key(30)]
    public TripleBoolean DistantVarianceReversed { get; set; } = TripleBoolean.NotSet;
    [Key(31)]
    public float DamageVariance { get; set; } = 0f;
    [Key(32)]
    public string WeaponEffectID { get; set; } = string.Empty;
    [Key(33)]
    public float EvasivePipsIgnored { get; set; } = 0f;
    [Key(34)]
    public TripleBoolean IndirectFireCapable { get; set; } = TripleBoolean.NotSet;
    [Key(35)]
    public TripleBoolean DamageOnJamming { get; set; } = TripleBoolean.NotSet;
    [Key(36)]
    public TripleBoolean DestroyOnJamming { get; set; } = TripleBoolean.NotSet;
    [Key(37)]
    public TripleBoolean AOECapable { get; set; } = TripleBoolean.NotSet;
    [Key(38)]
    public TripleBoolean AOEEffectsFalloff { get; set; } = TripleBoolean.NotSet;
    [Key(39)]
    public HitGeneratorType HitGenerator { get; set; } = HitGeneratorType.NotSet;
    [Key(40)]
    public TripleBoolean AlwaysIndirectVisuals { get; set; } = TripleBoolean.NotSet;
    [SkipDocumentation, Key(41)]
    public bool isBaseMode { get; set; } = false;
    [Key(42)]
    public float DamageMultiplier { get; set; } = 1f;
    [Key(43)]
    public float HeatMultiplier { get; set; } = 1f;
    [Key(44)]
    public float InstabilityMultiplier { get; set; } = 1f;
    [Key(45)]
    public float AMSHitChance { get; set; } = 0f;
    [JsonIgnore, SelfDocumentationTypeName("string, id from BattleTech.AmmoCategoryEnumeration or CustomAmmo"), SelfDocumentationDefaultValue("empty"), IgnoreMember]
    public CustomAmmoCategory AmmoCategory { get; set; } = null;
    [Key(46), SkipDocumentation, JsonIgnore]
    public string AmmoCategoryID { get { return AmmoCategory == null ? string.Empty : AmmoCategory.Id; } set { AmmoCategory = string.IsNullOrEmpty(value) ? null : CustomAmmoCategories.find(value); } }
    [Key(47)]
    public string IFFDef { get; set; } = string.Empty;
    [Key(48)]
    public TripleBoolean IsAMS { get; set; } = TripleBoolean.NotSet;
    [Key(49)]
    public TripleBoolean IsAAMS { get; set; } = TripleBoolean.NotSet;
    [Key(50)]
    public TripleBoolean HasShells { get; set; } = TripleBoolean.NotSet;
    [Key(51)]
    public float ShellsRadius { get; set; } = 0f;
    [Key(52)]
    public float MinShellsDistance { get; set; } = 30f;
    [Key(53)]
    public float MaxShellsDistance { get; set; } = 30f;
    [Key(54)]
    public TripleBoolean Unguided { get; set; } = TripleBoolean.NotSet;
    [Key(55)]
    public float ArmorDamageModifier { get; set; } = 1f;
    [Key(56)]
    public float ISDamageModifier { get; set; } = 1f;
    [Key(57)]
    public float FireTerrainChance { get; set; } = 0f;
    [Key(58)]
    public int FireDurationWithoutForest { get; set; } = 0;
    [Key(59)]
    public int FireTerrainStrength { get; set; } = 0;
    [Key(60)]
    public TripleBoolean FireOnSuccessHit { get; set; } = TripleBoolean.NotSet;
    [Key(61)]
    public int FireTerrainCellRadius { get; set; } = 0;
    [Key(62)]
    public string AdditionalImpactVFX { get; set; } = string.Empty;
    [Key(63)]
    public float AdditionalImpactVFXScaleX { get; set; } = 1f;
    [Key(64)]
    public float AdditionalImpactVFXScaleY { get; set; } = 1f;
    [Key(65)]
    public float AdditionalImpactVFXScaleZ { get; set; } = 1f;
    [Key(66)]
    public int ClearMineFieldRadius { get; set; } = 0;
    [Key(67)]
    public TripleBoolean BallisticDamagePerPallet { get; set; } = TripleBoolean.NotSet;
    [Key(68)]
    public string AdditionalAudioEffect { get; set; } = string.Empty;
    [Key(69)]
    public TripleBoolean Streak { get; set; } = TripleBoolean.NotSet;
    [Key(70)]
    public float FireDelayMultiplier { get; set; } = 1f;
    [Key(71)]
    public float MissileFiringIntervalMultiplier { get; set; } = 1f;
    [Key(72)]
    public float MissileVolleyIntervalMultiplier { get; set; } = 1f;
    [Key(73)]
    public float ProjectileSpeedMultiplier { get; set; } = 1f;
    [Key(74)]
    public TripleBoolean CantHitUnaffecedByPathing { get; set; } = TripleBoolean.NotSet;
    [Key(75)]
    public int MissileVolleySize { get; set; } = 0;
    [SelfDocumentationDefaultValue("{ \"x\": 1.0,\"y\":1.0, \"z\":1.0 }"), SelfDocumentationTypeName("Vector"), Key(76)]
    public CustomVector ProjectileScale { get; set; } = new CustomVector(true);
    [SelfDocumentationDefaultValue("{ \"x\": 1.0,\"y\":1.0, \"z\":1.0 }"), SelfDocumentationTypeName("Vector"), Key(77)]
    public CustomVector MissileExplosionScale { get; set; } = new CustomVector(true);
    [Key(78)]
    public float ColorSpeedChange { get; set; } = 0f;
    [Key(79)]
    public ColorChangeRule ColorChangeRule { get; set; } = ColorChangeRule.None;
    [Key(80)]
    public float APDamage { get; set; } = 0f;
    [Key(81)]
    public float APDamageMultiplier { get; set; } = 1f;
    [SelfDocumentationDefaultValue("undefined"), Key(82)]
    public float APCriticalChanceMultiplier { get; set; } = float.NaN;
    [Key(83)]
    public float APArmorShardsMod { get; set; } = 0f;
    [Key(84)]
    public float APMaxArmorThickness { get; set; } = 0f;
    [Key(85)]
    public TripleBoolean DamageNotDivided { get; set; } = TripleBoolean.NotSet;
    [Key(86)]
    public TripleBoolean isHeatVariation { get; set; } = TripleBoolean.NotSet;
    [Key(87)]
    public TripleBoolean isStabilityVariation { get; set; } = TripleBoolean.NotSet;
    [Key(88)]
    public TripleBoolean isDamageVariation { get; set; } = TripleBoolean.NotSet;
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("ModeLockSettings"), Key(89)]
    public ModeLockSettings Lock { get; set; } = new ModeLockSettings();
    [Key(90)]
    public float ClusteringModifier { get; set; } = 0f;
    [Key(91)]
    public float PrefireAnimationSpeedMod { get; set; } = 1f;
    [Key(92)]
    public float FireAnimationSpeedMod { get; set; } = 1f;
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("EvasivePipsMods structure"), Key(93)]
    public EvasivePipsMods evasivePipsMods { get; set; } = new EvasivePipsMods();
    [Key(94)]
    public float ShotsPerAmmo { get; set; } = 1f;
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("DeferredEffectDef structure"), Key(95)]
    public DeferredEffectDef deferredEffect { get; set; } = new DeferredEffectDef();
    [Key(96)]
    public string preFireSFX { get; set; } = string.Empty;
    [Key(97)]
    public float MinMissRadius { get; set; } = 0f;
    [Key(98)]
    public float MaxMissRadius { get; set; } = 0f;
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("Dictionary of {\"<tag name>\":<float modifier>}"), Key(99)]
    public Dictionary<string, float> TagsAccuracyModifiers { get; set; } = new Dictionary<string, float>();
    [Key(100)]
    public TripleBoolean AMSImmune { get; set; } = TripleBoolean.NotSet;
    [Key(101)]
    public float AMSDamage { get; set; } = 0f;
    [Key(102)]
    public float MissileHealth { get; set; } = 0f;
    [Key(103)]
    public DamageFalloffType RangedDmgFalloffType { get; set; } = DamageFalloffType.NotSet;
    [Key(104)]
    public DamageFalloffType AoEDmgFalloffType { get; set; } = DamageFalloffType.NotSet;
    [Key(105)]
    public float DamageFalloffStartDistance { get; set; } = 0f;
    [Key(106)]
    public float DamageFalloffEndDistance { get; set; } = 0f;
    [Key(107)]
    public TripleBoolean AMSShootsEveryAttack { get; set; } = TripleBoolean.NotSet;
    [Key(108)]
    public TripleBoolean TargetMechLegsOnly { get; set; } = TripleBoolean.NotSet;
    [Key(109)]
    public float HeatGeneratedModifier { get; set; } = 1f;
    [Key(110)]
    public MeleeAttackType meleeAttackType { get; set; } = MeleeAttackType.NotSet;
    [Key(111)]
    public float BuildingsDamageModifier { get; set; } = 1f;
    [Key(112)]
    public float TurretDamageModifier { get; set; } = 1f;
    [Key(113)]
    public float VehicleDamageModifier { get; set; } = 1f;
    [Key(114)]
    public float VTOLDamageModifier { get; set; } = 1f;
    [Key(115)]
    public float MechDamageModifier { get; set; } = 1f;
    [Key(116)]
    public float QuadDamageModifier { get; set; } = 1f;
    [Key(117)]
    public float TrooperSquadDamageModifier { get; set; } = 1f;
    [Key(118)]
    public float AirMechDamageModifier { get; set; } = 1f;
    [Key(119)]
    public string fireSFX { get; set; } = string.Empty;
    [IgnoreMember, JsonIgnore]
    public bool Disabeld { get; set; } = false;
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
