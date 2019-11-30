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
using System.Text;

namespace CustAmmoCategories {
  public class MineFieldDef {
    public float Damage { get; set; }
    public float Heat { get; set; }
    public float Instability { get; set; }
    public float AoEDamage { get; set; }
    public float AoEHeat { get; set; }
    public float AoEInstability { get; set; }
    public float AoERange { get; set; }
    public float Chance { get; set; }
    public int Count { get; set; }
    public string VFXprefab { get; set; }
    public float VFXScaleX { get; set; }
    public float VFXScaleY { get; set; }
    public float VFXScaleZ { get; set; }
    public float VFXOffsetX { get; set; }
    public float VFXOffsetY { get; set; }
    public float VFXOffsetZ { get; set; }
    public float VFXMinDistance { get; set; }
    public int InstallCellRange { get; set; }
    public CustomAudioSource SFX { get; set; }
    public List<EffectData> statusEffects { get; set; }
    public float FireTerrainChance { get; set; }
    public int FireDurationWithoutForest { get; set; }
    public int FireTerrainStrength { get; set; }
    public int FireTerrainCellRadius { get; set; }
    public string LongVFXOnImpact { get; set; }
    public string tempDesignMaskOnImpact { get; set; }
    public int tempDesignMaskOnImpactTurns { get; set; }
    public float LongVFXOnImpactScaleX { get; set; }
    public float LongVFXOnImpactScaleY { get; set; }
    public float LongVFXOnImpactScaleZ { get; set; }
    public int tempDesignMaskCellRadius { get; set; }
    public MineFieldDef() {
      Damage = 0f;
      Heat = 0f;
      Instability = 0f;
      AoEDamage = 0f;
      AoEHeat = 0f;
      AoEInstability = 0f;
      AoERange = 0f;
      Chance = 0f;
      VFXprefab = string.Empty;
      VFXScaleX = 1f;
      VFXScaleY = 1f;
      VFXScaleZ = 1f;
      VFXOffsetX = 0f;
      VFXOffsetY = 0f;
      VFXOffsetZ = 0f;
      VFXMinDistance = 20f;
      InstallCellRange = 0;
      SFX = null;
      Count = 0;
      FireTerrainChance = 0f;
      FireDurationWithoutForest = 0;
      FireTerrainStrength = 0;
      FireTerrainCellRadius = 0;
      LongVFXOnImpact = string.Empty;
      tempDesignMaskOnImpact = string.Empty;
      tempDesignMaskOnImpactTurns = 0;
      LongVFXOnImpactScaleX = 0f;
      LongVFXOnImpactScaleY = 0f;
      LongVFXOnImpactScaleZ = 0f;
      tempDesignMaskCellRadius = 0;
      statusEffects = new List<EffectData>();
    }
    public void fromJSON(JToken json) {
      if (json["Damage"] != null) { Damage = (float)json["Damage"]; };
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
      if (json["SFX"] != null) { SFX = new CustomAudioSource((string)json["SFX"]); };
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
  public class ExtAmmunitionDef {
    [JsonIgnore]
    public string Id { get; set; }
    public float AccuracyModifier { get; set; }
    public float DirectFireModifier { get; set; }
    public float DamagePerShot { get; set; }
    public float HeatDamagePerShot { get; set; }
    public float CriticalChanceMultiplier { get; set; }
    public int ShotsWhenFired { get; set; }
    public int AIBattleValue { get; set; }
    public int ProjectilesPerShot { get; private set; }
    public EffectData[] statusEffects { get; set; }
    public float MinRange { get; set; }
    public float MaxRange { get; set; }
    public float LongRange { get; set; }
    public float ShortRange { get; set; }
    public float ForbiddenRange { get; set; }
    public float MediumRange { get; set; }
    public int RefireModifier { get; set; }
    public int AttackRecoil { get; set; }
    public float Instability { get; set; }
    public string WeaponEffectID { get; set; }
    public float EvasivePipsIgnored { get; set; }
    public TripleBoolean IndirectFireCapable { get; set; }
    public TripleBoolean AOECapable { get; set; }
    public HitGeneratorType HitGenerator { get; set; }
    public float FlatJammingChance { get; set; }
    public float AMSHitChance { get; set; }
    public float GunneryJammingBase { get; set; }
    public float GunneryJammingMult { get; set; }
    public float DistantVariance { get; set; }
    public TripleBoolean DistantVarianceReversed { get; set; }
    public float DamageVariance { get; set; }
    public float DamageMultiplier { get; set; }
    public float HeatMultiplier { get; set; }
    public float InstabilityMultiplier { get; set; }
    public CustomAmmoCategory AmmoCategory { get; set; }
    public float SpreadRange { get; set; }
    public float AOERange { get; set; }
    public float AOEDamage { get; set; }
    public float AOEHeatDamage { get; set; }
    public float AOEInstability { get; set; }
    public TripleBoolean AOEEffectsFalloff { get; set; }
    public TripleBoolean AlwaysIndirectVisuals { get; set; }
    public string IFFDef { get; set; }
    public string LongVFXOnImpact { get; set; }
    public string tempDesignMaskOnImpact { get; set; }
    public int tempDesignMaskOnImpactTurns { get; set; }
    public float LongVFXOnImpactScaleX { get; set; }
    public float LongVFXOnImpactScaleY { get; set; }
    public float LongVFXOnImpactScaleZ { get; set; }
    public int tempDesignMaskCellRadius { get; set; }
    public TripleBoolean HasShells { get; set; }
    public float ShellsRadius { get; set; }
    public float MinShellsDistance { get; set; }
    public float MaxShellsDistance { get; set; }
    public float UnseparatedDamageMult { get; set; }
    public float ArmorDamageModifier { get; set; }
    public float ISDamageModifier { get; set; }
    public float CanBeExhaustedAt { get; set; }
    //public Dictionary<TerrainMaskFlags,string> SurfaceImpactDesignMaskId { get; set; }
    public TripleBoolean SurfaceBecomeDangerousOnImpact { get; set; }
    public TripleBoolean Unguided { get; set; }
    public int ClearMineFieldRadius { get; set; }
    public float FireTerrainChance { get; set; }
    public int FireDurationWithoutForest { get; set; }
    public int FireTerrainStrength { get; set; }
    public int FireTerrainCellRadius { get; set; }
    public string AdditionalImpactVFX { get; set; }
    public float AdditionalImpactVFXScaleX { get; set; }
    public float AdditionalImpactVFXScaleY { get; set; }
    public float AdditionalImpactVFXScaleZ { get; set; }
    public TripleBoolean FireOnSuccessHit { get; set; }
    public TripleBoolean IsAMS { get; set; }
    public TripleBoolean IsAAMS { get; set; }
    public TripleBoolean BallisticDamagePerPallet { get; set; }
    public CustomAudioSource AdditionalAudioEffect { get; set; }
    public MineFieldDef MineField { get; set; }
    public Dictionary<string, float> TagsAccuracyModifiers { get; set; }
    public TripleBoolean Streak { get; set; }
    public float FireDelayMultiplier { get; set; }
    public float MissileFiringIntervalMultiplier { get; set; }
    public float MissileVolleyIntervalMultiplier { get; set; }
    public float ProjectileSpeedMultiplier { get; set; }
    public TripleBoolean CantHitUnaffecedByPathing { get; set; }
    public int MissileVolleySize { get; set; }
    public CustomVector ProjectileScale { get; set; }
    public CustomVector MissileExplosionScale { get; set; }
    public void ProjectilesPerShotSet(int value) {
      this.ProjectilesPerShot = value;
    }
    public float ColorSpeedChange { get; set; }
    public ColorChangeRule ColorChangeRule { get; set; }
    public float APDamage { get; set; }
    public float APDamageMultiplier { get; set; }
    public float APCriticalChanceMultiplier { get; set; }
    public float APArmorShardsMod { get; set; }
    public float APMaxArmorThickness { get; set; }
    public TripleBoolean DamageNotDivided { get; set; }
    public List<ColorTableJsonEntry> ColorsTable { get; set; }
    public TripleBoolean isHeatVariation { get; set; }
    public TripleBoolean isStabilityVariation { get; set; }
    public TripleBoolean isDamageVariation { get; set; }
    public float ClusteringModifier { get; set;}
    public float PrefireAnimationSpeedMod { get; set; }
    public float FireAnimationSpeedMod { get; set; }
    public ExtAmmunitionDef() {
      Id = "NotSet";
      AccuracyModifier = 0;
      DirectFireModifier = 0;
      DamagePerShot = 0;
      HeatDamagePerShot = 0;
      ProjectilesPerShot = 0;
      ShotsWhenFired = 0;
      CriticalChanceMultiplier = 0;
      DamageMultiplier = 1.0f;
      HeatMultiplier = 1.0f;
      InstabilityMultiplier = 1.0f;
      MinRange = 0;
      MaxRange = 0;
      LongRange = 0;
      AMSHitChance = 0f;
      MaxRange = 0;
      ShortRange = 0;
      MediumRange = 0;
      AIBattleValue = 100;
      RefireModifier = 0;
      Instability = 0;
      AttackRecoil = 0;
      EvasivePipsIgnored = 0;
      FlatJammingChance = 0;
      DistantVariance = 0;
      DistantVarianceReversed = TripleBoolean.NotSet;
      DamageVariance = 0;
      ForbiddenRange = 0;
      GunneryJammingBase = 0;
      GunneryJammingMult = 0;
      AOERange = 0;
      AOEDamage = 0;
      AOEHeatDamage = 0;
      AOEInstability = 0f;
      SpreadRange = 0;
      IndirectFireCapable = TripleBoolean.NotSet;
      AlwaysIndirectVisuals = TripleBoolean.NotSet;
      AOECapable = TripleBoolean.NotSet;
      WeaponEffectID = "";
      IFFDef = "";
      HitGenerator = HitGeneratorType.NotSet;
      statusEffects = new EffectData[0] { };
      AmmoCategory = new CustomAmmoCategory();
      HasShells = TripleBoolean.NotSet;
      ShellsRadius = 0f;
      MinShellsDistance = 30f;
      MaxShellsDistance = 30f;
      UnseparatedDamageMult = 1f;
      ISDamageModifier = 1f;
      ArmorDamageModifier = 1f;
      CanBeExhaustedAt = 0f;
      //SurfaceImpactDesignMaskId = new Dictionary<TerrainMaskFlags, string>();
      Unguided = TripleBoolean.NotSet;
      FireTerrainChance = 0f;
      FireDurationWithoutForest = 0;
      FireTerrainStrength = 0;
      FireTerrainCellRadius = 0;
      AdditionalImpactVFX = string.Empty;
      AdditionalImpactVFXScaleX = 1f;
      AdditionalImpactVFXScaleY = 1f;
      AdditionalImpactVFXScaleZ = 1f;
      LongVFXOnImpactScaleX = 1f;
      LongVFXOnImpactScaleY = 1f;
      LongVFXOnImpactScaleZ = 1f;
      FireOnSuccessHit = TripleBoolean.NotSet;
      LongVFXOnImpact = string.Empty;
      tempDesignMaskOnImpact = string.Empty;
      tempDesignMaskOnImpactTurns = 0;
      tempDesignMaskCellRadius = 0;
      ClearMineFieldRadius = 0;
      IsAMS = TripleBoolean.NotSet;
      IsAAMS = TripleBoolean.NotSet;
      BallisticDamagePerPallet = TripleBoolean.NotSet;
      AdditionalAudioEffect = null;
      TagsAccuracyModifiers = new Dictionary<string, float>();
      Streak = TripleBoolean.NotSet;
      FireDelayMultiplier = 1f;
      MissileFiringIntervalMultiplier = 1f;
      MissileVolleyIntervalMultiplier = 1f;
      ProjectileSpeedMultiplier = 1f;
      MineField = new MineFieldDef();
      CantHitUnaffecedByPathing = TripleBoolean.NotSet;
      MissileVolleySize = 0;
      ProjectileScale = new CustomVector(true);
      MissileExplosionScale = new CustomVector(true);
      ColorSpeedChange = 0f;
      ColorChangeRule = ColorChangeRule.None;
      ColorsTable = new List<ColorTableJsonEntry>();
      APDamage = 0f;
      APCriticalChanceMultiplier = float.NaN;
      APArmorShardsMod = 0f;
      APMaxArmorThickness = 0f;
      DamageNotDivided = TripleBoolean.NotSet;
      isHeatVariation = TripleBoolean.NotSet;
      isDamageVariation = TripleBoolean.NotSet;
      isStabilityVariation = TripleBoolean.NotSet;
      APDamageMultiplier = 1f;
      ClusteringModifier = 0f;
      AOEEffectsFalloff = TripleBoolean.NotSet;
      PrefireAnimationSpeedMod = 1f;
      FireAnimationSpeedMod = 1f;
    }
  }
}

namespace CustomAmmoCategoriesPatches {
  [HarmonyPatch(typeof(AmmunitionDef))]
  [HarmonyPatch("FromJSON")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class BattleTech_AmmunitionDef_fromJSON_Patch {
    public static bool Prefix(AmmunitionDef __instance, ref string json) {
      Log.LogWrite("AmmunitionDef fromJSON ");
      JObject defTemp = null;
      try {
        defTemp = JObject.Parse(json);
      }catch(Exception e) {
        Log.LogWrite("Exception:" + e.ToString() + "\nWhile reading json:"+json+"\n", true);
      }

      CustomAmmoCategoriesLog.Log.LogWrite(defTemp["Description"]["Id"] + "\n");
      string AmmoCategory = "NotSet";
      if (defTemp["Category"] != null) {
        AmmoCategory = (string)defTemp["Category"];
        defTemp.Remove("Category");
        //defTemp["ammoCategoryID"] = AmmoCategory;
      } else {
        AmmoCategory = (string)defTemp["ammoCategoryID"];
      }
      CustomAmmoCategory custCat = CustomAmmoCategories.find(AmmoCategory);
      //CustomAmmoCategories.RegisterAmmunition((string)defTemp["Description"]["Id"], custCat);
      defTemp["ammoCategoryID"] = custCat.BaseCategory.Name;
      ExtAmmunitionDef extAmmoDef = new ExtAmmunitionDef();
      extAmmoDef.Id = (string)defTemp["Description"]["Id"];
      extAmmoDef.AmmoCategory = custCat;
      if (defTemp["AccuracyModifier"] != null) {
        extAmmoDef.AccuracyModifier = (float)defTemp["AccuracyModifier"];
        defTemp.Remove("AccuracyModifier");
      }
      if (defTemp["DamagePerShot"] != null) {
        extAmmoDef.DamagePerShot = (float)defTemp["DamagePerShot"];
        defTemp.Remove("DamagePerShot");
      }
      if (defTemp["ClusteringModifier"] != null) {
        extAmmoDef.ClusteringModifier = (float)defTemp["ClusteringModifier"];
        defTemp.Remove("ClusteringModifier");
      }
      if (defTemp["HeatDamagePerShot"] != null) {
        extAmmoDef.HeatDamagePerShot = (float)defTemp["HeatDamagePerShot"];
        defTemp.Remove("HeatDamagePerShot");
      }
      if (defTemp["ProjectilesPerShot"] != null) {
        extAmmoDef.ProjectilesPerShotSet((int)defTemp["ProjectilesPerShot"]);
        defTemp.Remove("ProjectilesPerShot");
      }
      if (defTemp["ShotsWhenFired"] != null) {
        extAmmoDef.ShotsWhenFired = (int)defTemp["ShotsWhenFired"];
        defTemp.Remove("ShotsWhenFired");
      }
      if (defTemp["CriticalChanceMultiplier"] != null) {
        extAmmoDef.CriticalChanceMultiplier = (float)defTemp["CriticalChanceMultiplier"];
        defTemp.Remove("CriticalChanceMultiplier");
      }
      if (defTemp["FireDelayMultiplier"] != null) {
        extAmmoDef.FireDelayMultiplier = (float)defTemp["FireDelayMultiplier"];
        defTemp.Remove("FireDelayMultiplier");
      }
      if (defTemp["MissileFiringIntervalMultiplier"] != null) {
        extAmmoDef.MissileFiringIntervalMultiplier = (float)defTemp["MissileFiringIntervalMultiplier"];
        defTemp.Remove("MissileFiringIntervalMultiplier");
      }
      if (defTemp["MissileVolleyIntervalMultiplier"] != null) {
        extAmmoDef.MissileVolleyIntervalMultiplier = (float)defTemp["MissileVolleyIntervalMultiplier"];
        defTemp.Remove("MissileVolleyIntervalMultiplier");
      }
      if (defTemp["ProjectileSpeedMultiplier"] != null) {
        extAmmoDef.ProjectileSpeedMultiplier = (float)defTemp["ProjectileSpeedMultiplier"];
        defTemp.Remove("ProjectileSpeedMultiplier");
      }
      if (defTemp["FireAnimationSpeedMod"] != null) {
        extAmmoDef.FireAnimationSpeedMod = (float)defTemp["FireAnimationSpeedMod"];
        defTemp.Remove("FireAnimationSpeedMod");
      }
      if (defTemp["PrefireAnimationSpeedMod"] != null) {
        extAmmoDef.PrefireAnimationSpeedMod = (float)defTemp["PrefireAnimationSpeedMod"];
        defTemp.Remove("PrefireAnimationSpeedMod");
      }
      if (defTemp["AIBattleValue"] != null) {
        extAmmoDef.AIBattleValue = (int)defTemp["AIBattleValue"];
        defTemp.Remove("AIBattleValue");
      }
      if (defTemp["ColorSpeedChange"] != null) {
        extAmmoDef.ColorSpeedChange = (float)defTemp["ColorSpeedChange"];
        defTemp.Remove("ColorSpeedChange");
      }
      if (defTemp["ColorChangeRule"] != null) {
        extAmmoDef.ColorChangeRule = (ColorChangeRule)Enum.Parse(typeof(ColorChangeRule), (string)defTemp["ColorChangeRule"]);
        defTemp.Remove("ColorChangeRule");
      }
      if (defTemp["APDamage"] != null) {
        extAmmoDef.APDamage = (float)defTemp["APDamage"];
        defTemp.Remove("APDamage");
      }
      if (defTemp["APCriticalChanceMultiplier"] != null) {
        extAmmoDef.APCriticalChanceMultiplier = (float)defTemp["APCriticalChanceMultiplier"];
        defTemp.Remove("APCriticalChanceMultiplier");
      }
      if (defTemp["APArmorShardsMod"] != null) {
        extAmmoDef.APArmorShardsMod = (float)defTemp["APArmorShardsMod"];
        defTemp.Remove("APArmorShardsMod");
      }
      if (defTemp["APMaxArmorThickness"] != null) {
        extAmmoDef.APMaxArmorThickness = (float)defTemp["APMaxArmorThickness"];
        defTemp.Remove("APMaxArmorThickness");
      }
      if (defTemp["MinRange"] != null) {
        extAmmoDef.MinRange = (float)defTemp["MinRange"];
        defTemp.Remove("MinRange");
      }
      if (defTemp["MaxRange"] != null) {
        extAmmoDef.MaxRange = (float)defTemp["MaxRange"];
        defTemp.Remove("MaxRange");
      }
      if (defTemp["ShortRange"] != null) {
        extAmmoDef.ShortRange = (float)defTemp["ShortRange"];
        defTemp.Remove("ShortRange");
      }
      if (defTemp["MediumRange"] != null) {
        extAmmoDef.MediumRange = (float)defTemp["MediumRange"];
        defTemp.Remove("MediumRange");
      }
      if (defTemp["LongRange"] != null) {
        extAmmoDef.LongRange = (float)defTemp["LongRange"];
        defTemp.Remove("LongRange");
      }
      if (defTemp["ForbiddenRange"] != null) {
        extAmmoDef.ForbiddenRange = (float)defTemp["ForbiddenRange"];
        defTemp.Remove("ForbiddenRange");
      }
      if (defTemp["ProjectileScale"] != null) {
        extAmmoDef.ProjectileScale = defTemp["ProjectileScale"].ToObject<CustomVector>();
        defTemp.Remove("ProjectileScale");
      }
      if (defTemp["MissileExplosionScale"] != null) {
        extAmmoDef.MissileExplosionScale = defTemp["MissileExplosionScale"].ToObject<CustomVector>();
        defTemp.Remove("MissileExplosionScale");
      }
      if (defTemp["CanBeExhaustedAt"] != null) {
        extAmmoDef.CanBeExhaustedAt = (float)defTemp["CanBeExhaustedAt"];
        defTemp.Remove("CanBeExhaustedAt");
      }
      if (defTemp["MineFieldRadius"] != null) {
        extAmmoDef.MineField.InstallCellRange = (int)defTemp["MineFieldRadius"];
        defTemp.Remove("MineFieldRadius");
      }
      if (defTemp["ClearMineFieldRadius"] != null) {
        extAmmoDef.ClearMineFieldRadius = (int)defTemp["ClearMineFieldRadius"];
        defTemp.Remove("ClearMineFieldRadius");
      }
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
        extAmmoDef.MineField.SFX = new CustomAudioSource((string)defTemp["MineFieldSFX"]);
        defTemp.Remove("MineFieldSFX");
      }
      if (defTemp["MineFieldFXMinRange"] != null) {
        extAmmoDef.MineField.VFXMinDistance = (float)defTemp["MineFieldFXMinRange"];
        if (extAmmoDef.MineField.VFXMinDistance < 20f) { extAmmoDef.MineField.VFXMinDistance = 20f; };
        defTemp.Remove("MineFieldFXMinRange");
      }
      if (defTemp["BallisticDamagePerPallet"] != null) {
        extAmmoDef.BallisticDamagePerPallet = ((bool)defTemp["BallisticDamagePerPallet"] == true) ? TripleBoolean.True : TripleBoolean.False;
        defTemp.Remove("BallisticDamagePerPallet");
      }
      if (defTemp["AOEEffectsFalloff"] != null) {
        extAmmoDef.AOEEffectsFalloff = ((bool)defTemp["AOEEffectsFalloff"] == true) ? TripleBoolean.True : TripleBoolean.False;
        defTemp.Remove("AOEEffectsFalloff");
      }
      if (defTemp["CantHitUnaffecedByPathing"] != null) {
        extAmmoDef.CantHitUnaffecedByPathing = ((bool)defTemp["CantHitUnaffecedByPathing"] == true) ? TripleBoolean.True : TripleBoolean.False;
        defTemp.Remove("CantHitUnaffecedByPathing");
      }
      if (defTemp["AdditionalImpactVFXScaleX"] != null) {
        extAmmoDef.AdditionalImpactVFXScaleX = (float)defTemp["AdditionalImpactVFXScaleX"];
        defTemp.Remove("AdditionalImpactVFXScaleX");
      }
      if (defTemp["AdditionalImpactVFXScaleY"] != null) {
        extAmmoDef.AdditionalImpactVFXScaleY = (float)defTemp["AdditionalImpactVFXScaleY"];
        defTemp.Remove("AdditionalImpactVFXScaleY");
      }
      if (defTemp["AdditionalImpactVFXScaleZ"] != null) {
        extAmmoDef.AdditionalImpactVFXScaleZ = (float)defTemp["AdditionalImpactVFXScaleZ"];
        defTemp.Remove("AdditionalImpactVFXScaleZ");
      }
      if (defTemp["LongVFXOnImpactScaleX"] != null) {
        extAmmoDef.LongVFXOnImpactScaleX = (float)defTemp["LongVFXOnImpactScaleX"];
        defTemp.Remove("LongVFXOnImpactScaleX");
      }
      if (defTemp["LongVFXOnImpactScaleY"] != null) {
        extAmmoDef.LongVFXOnImpactScaleX = (float)defTemp["LongVFXOnImpactScaleY"];
        defTemp.Remove("LongVFXOnImpactScaleY");
      }
      if (defTemp["LongVFXOnImpactScaleZ"] != null) {
        extAmmoDef.LongVFXOnImpactScaleX = (float)defTemp["LongVFXOnImpactScaleZ"];
        defTemp.Remove("LongVFXOnImpactScaleZ");
      }
      if (defTemp["tempDesignMaskCellRadius"] != null) {
        extAmmoDef.tempDesignMaskCellRadius = (int)defTemp["tempDesignMaskCellRadius"];
        defTemp.Remove("tempDesignMaskCellRadius");
      }
      if (defTemp["MineFieldHeat"] != null) {
        extAmmoDef.MineField.Heat = (float)defTemp["MineFieldHeat"];
        defTemp.Remove("MineFieldHeat");
      }
      if (defTemp["FireTerrainChance"] != null) {
        extAmmoDef.FireTerrainChance = (float)defTemp["FireTerrainChance"];
        defTemp.Remove("FireTerrainChance");
      }
      if (defTemp["FireDurationWithoutForest"] != null) {
        extAmmoDef.FireDurationWithoutForest = (int)defTemp["FireDurationWithoutForest"];
        defTemp.Remove("FireDurationWithoutForest");
      }
      if (defTemp["FireTerrainStrength"] != null) {
        extAmmoDef.FireTerrainStrength = (int)defTemp["FireTerrainStrength"];
        defTemp.Remove("FireTerrainStrength");
      }
      if (defTemp["FireTerrainCellRadius"] != null) {
        extAmmoDef.FireTerrainCellRadius = (int)defTemp["FireTerrainCellRadius"];
        defTemp.Remove("FireTerrainCellRadius");
      }
      if (defTemp["MissileVolleySize"] != null) {
        extAmmoDef.MissileVolleySize = (int)defTemp["MissileVolleySize"];
        defTemp.Remove("MissileVolleySize");
      }
      if (defTemp["AdditionalImpactVFX"] != null) {
        extAmmoDef.AdditionalImpactVFX = (string)defTemp["AdditionalImpactVFX"];
        defTemp.Remove("AdditionalImpactVFX");
      }
      if (defTemp["FireOnSuccessHit"] != null) {
        extAmmoDef.FireOnSuccessHit = ((bool)defTemp["FireOnSuccessHit"] == true) ? TripleBoolean.True : TripleBoolean.False;
        defTemp.Remove("FireOnSuccessHit");
      }
      if (defTemp["ColorsTable"] != null) {
        extAmmoDef.ColorsTable = defTemp["ColorsTable"].ToObject<List<ColorTableJsonEntry>>();
        defTemp.Remove("ColorsTable");
      }
      if (defTemp["isDamageVariation"] != null) {
        extAmmoDef.isDamageVariation = ((bool)defTemp["isDamageVariation"] == true) ? TripleBoolean.True : TripleBoolean.False;
        defTemp.Remove("isDamageVariation");
      }
      if (defTemp["isHeatVariation"] != null) {
        extAmmoDef.isHeatVariation = ((bool)defTemp["isHeatVariation"] == true) ? TripleBoolean.True : TripleBoolean.False;
        defTemp.Remove("isHeatVariation");
      }
      if (defTemp["isStabilityVariation"] != null) {
        extAmmoDef.isStabilityVariation = ((bool)defTemp["isStabilityVariation"] == true) ? TripleBoolean.True : TripleBoolean.False;
        defTemp.Remove("isStabilityVariation");
      }
      if (defTemp["Streak"] != null) {
        extAmmoDef.Streak = ((bool)defTemp["Streak"] == true) ? TripleBoolean.True : TripleBoolean.False;
        defTemp.Remove("Streak");
      }
      if (defTemp["MineFieldCount"] != null) {
        extAmmoDef.MineField.Count = (int)defTemp["MineFieldCount"];
        defTemp.Remove("MineFieldCount");
      }
      if (defTemp["MineFieldInstability"] != null) {
        extAmmoDef.MineField.Instability = (float)defTemp["MineFieldInstability"];
        defTemp.Remove("MineFieldInstability");
      }
      if (defTemp["IFFDef"] != null) {
        extAmmoDef.IFFDef = (string)defTemp["IFFDef"];
        defTemp.Remove("IFFDef");
      }
      if (defTemp["AdditionalAudioEffect"] != null) {
        extAmmoDef.AdditionalAudioEffect = new CustomAudioSource((string)defTemp["AdditionalAudioEffect"]);
        defTemp.Remove("AdditionalAudioEffect");
      }
      /*if (defTemp["SurfaceImpactDesignMaskId"] != null) {
        extAmmoDef.SurfaceImpactDesignMaskId = JsonConvert.DeserializeObject<Dictionary<TerrainMaskFlags, string>>(defTemp["SurfaceImpactDesignMaskId"].ToString());
        foreach (var sdm in extAmmoDef.SurfaceImpactDesignMaskId) {
          CustomAmmoCategoriesLog.Log.LogWrite(" TerrainFlag:"+sdm.Key+" Mask:"+sdm.Value+"\n");
        }
        defTemp.Remove("SurfaceImpactDesignMaskId");
      }*/
      if (defTemp["AOERange"] != null) {
        extAmmoDef.AOERange = (float)defTemp["AOERange"];
        defTemp.Remove("AOERange");
      }
      if (defTemp["SpreadRange"] != null) {
        extAmmoDef.SpreadRange = (float)defTemp["SpreadRange"];
        defTemp.Remove("SpreadRange");
      }
      if (defTemp["UnseparatedDamageMult"] != null) {
        extAmmoDef.UnseparatedDamageMult = (float)defTemp["UnseparatedDamageMult"];
        defTemp.Remove("UnseparatedDamageMult");
      }
      if (defTemp["RefireModifier"] != null) {
        extAmmoDef.RefireModifier = (int)defTemp["RefireModifier"];
        defTemp.Remove("RefireModifier");
      }
      if (defTemp["Instability"] != null) {
        extAmmoDef.Instability = (float)defTemp["Instability"];
        defTemp.Remove("Instability");
      }
      if (defTemp["AttackRecoil"] != null) {
        extAmmoDef.AttackRecoil = (int)defTemp["AttackRecoil"];
        defTemp.Remove("AttackRecoil");
      }
      if (defTemp["WeaponEffectID"] != null) {
        extAmmoDef.WeaponEffectID = (string)defTemp["WeaponEffectID"];
        defTemp.Remove("WeaponEffectID");
      }
      if (defTemp["tempDesignMaskOnImpact"] != null) {
        extAmmoDef.tempDesignMaskOnImpact = (string)defTemp["tempDesignMaskOnImpact"];
        defTemp.Remove("tempDesignMaskOnImpact");
      }
      if (defTemp["LongVFXOnImpact"] != null) {
        extAmmoDef.LongVFXOnImpact = (string)defTemp["LongVFXOnImpact"];
        defTemp.Remove("LongVFXOnImpact");
      }
      if (defTemp["tempDesignMaskOnImpactTurns"] != null) {
        extAmmoDef.tempDesignMaskOnImpactTurns = (int)defTemp["tempDesignMaskOnImpactTurns"];
        defTemp.Remove("tempDesignMaskOnImpactTurns");
      }
      if (defTemp["EvasivePipsIgnored"] != null) {
        extAmmoDef.EvasivePipsIgnored = (float)defTemp["EvasivePipsIgnored"];
        defTemp.Remove("EvasivePipsIgnored");
      }
      if (defTemp["FlatJammingChance"] != null) {
        extAmmoDef.FlatJammingChance = (float)defTemp["FlatJammingChance"];
        defTemp.Remove("FlatJammingChance");
      }
      if (defTemp["AMSHitChance"] != null) {
        extAmmoDef.AMSHitChance = (float)defTemp["AMSHitChance"];
        defTemp.Remove("AMSHitChance");
      }
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
      if (defTemp["AOEDamage"] != null) {
        extAmmoDef.AOEDamage = (float)defTemp["AOEDamage"];
        defTemp.Remove("AOEDamage");
      }
      if (defTemp["AOEHeatDamage"] != null) {
        extAmmoDef.AOEHeatDamage = (float)defTemp["AOEHeatDamage"];
        defTemp.Remove("AOEHeatDamage");
      }
      if (defTemp["AOEInstability"] != null) {
        extAmmoDef.AOEInstability = (float)defTemp["AOEInstability"];
        defTemp.Remove("AOEInstability");
      }
      if (defTemp["AMSHitChance"] != null) {
        extAmmoDef.AMSHitChance = (float)defTemp["AMSHitChance"];
        defTemp.Remove("AMSHitChance");
      }
      if (defTemp["ArmorDamageModifier"] != null) {
        extAmmoDef.ArmorDamageModifier = (float)defTemp["ArmorDamageModifier"];
      }
      if (defTemp["ISDamageModifier"] != null) {
        extAmmoDef.ISDamageModifier = (float)defTemp["ISDamageModifier"];
      }
      if (defTemp["GunneryJammingBase"] != null) {
        extAmmoDef.GunneryJammingBase = (float)defTemp["GunneryJammingBase"];
        defTemp.Remove("GunneryJammingBase");
      }
      if (defTemp["GunneryJammingMult"] != null) {
        extAmmoDef.GunneryJammingMult = (float)defTemp["GunneryJammingMult"];
        defTemp.Remove("GunneryJammingMult");
      }
      if (defTemp["IndirectFireCapable"] != null) {
        extAmmoDef.IndirectFireCapable = ((bool)defTemp["IndirectFireCapable"] == true) ? TripleBoolean.True : TripleBoolean.False;
        defTemp.Remove("IndirectFireCapable");
      }
      if (defTemp["HasShells"] != null) {
        extAmmoDef.HasShells = ((bool)defTemp["HasShells"] == true) ? TripleBoolean.True : TripleBoolean.False;
        defTemp.Remove("HasShells");
      }
      if (defTemp["ShellsRadius"] != null) {
        extAmmoDef.ShellsRadius = (float)defTemp["ShellsRadius"];
        defTemp.Remove("ShellsRadius");
      }
      if (defTemp["MinShellsDistance"] != null) {
        extAmmoDef.MinShellsDistance = (float)defTemp["MinShellsDistance"];
        defTemp.Remove("MinShellsDistance");
      }
      if (defTemp["MaxShellsDistance"] != null) {
        extAmmoDef.MaxShellsDistance = (float)defTemp["MaxShellsDistance"];
        defTemp.Remove("MaxShellsDistance");
      }
      if (defTemp["DirectFireModifier"] != null) {
        extAmmoDef.DirectFireModifier = (float)defTemp["DirectFireModifier"];
        defTemp.Remove("DirectFireModifier");
      }
      if (defTemp["DistantVariance"] != null) {
        extAmmoDef.DistantVariance = (float)defTemp["DistantVariance"];
        defTemp.Remove("DistantVariance");
      }
      if (defTemp["DamageNotDivided"] != null) {
        extAmmoDef.DamageNotDivided = ((bool)defTemp["DamageNotDivided"] == true) ? TripleBoolean.True : TripleBoolean.False;
        defTemp.Remove("DamageNotDivided");
      }
      if (defTemp["AlwaysIndirectVisuals"] != null) {
        extAmmoDef.AlwaysIndirectVisuals = ((bool)defTemp["AlwaysIndirectVisuals"] == true) ? TripleBoolean.True : TripleBoolean.False;
        defTemp.Remove("AlwaysIndirectVisuals");
      }
      if (defTemp["Unguided"] != null) {
        extAmmoDef.Unguided = ((bool)defTemp["Unguided"] == true) ? TripleBoolean.True : TripleBoolean.False;
        defTemp.Remove("Unguided");
        if (extAmmoDef.Unguided == TripleBoolean.True) {
          //extAmmoDef.AlwaysIndirectVisuals = TripleBoolean.False;
          //extAmmoDef.IndirectFireCapable = TripleBoolean.False;
        }
      }
      if (defTemp["AOECapable"] != null) {
        extAmmoDef.AOECapable = ((bool)defTemp["AOECapable"] == true) ? TripleBoolean.True : TripleBoolean.False;
        //if (extAmmoDef.AOECapable == TripleBoolean.True) {
          //extAmmoDef.AlwaysIndirectVisuals = TripleBoolean.True;
        //}
        defTemp.Remove("AOECapable");
      }
      if (defTemp["DamageMultiplier"] != null) {
        extAmmoDef.DamageMultiplier = (float)defTemp["DamageMultiplier"];
        defTemp.Remove("DamageMultiplier");
      }
      if (defTemp["APDamageMultiplier"] != null) {
        extAmmoDef.APDamageMultiplier = (float)defTemp["APDamageMultiplier"];
        defTemp.Remove("APDamageMultiplier");
      }
      if (defTemp["HeatMultiplier"] != null) {
        extAmmoDef.HeatMultiplier = (float)defTemp["HeatMultiplier"];
        defTemp.Remove("HeatMultiplier");
      }
      if (defTemp["InstabilityMultiplier"] != null) {
        extAmmoDef.InstabilityMultiplier = (float)defTemp["InstabilityMultiplier"];
        defTemp.Remove("InstabilityMultiplier");
      }
      if (defTemp["DistantVarianceReversed"] != null) {
        extAmmoDef.DistantVarianceReversed = ((bool)defTemp["DistantVarianceReversed"] == true) ? TripleBoolean.True : TripleBoolean.False;
        defTemp.Remove("DistantVarianceReversed");
      }
      if (defTemp["DamageVariance"] != null) {
        extAmmoDef.DamageVariance = (float)defTemp["DamageVariance"];
        defTemp.Remove("DamageVariance");
      }
      if (defTemp["ChassisTagsAccuracyModifiers"] != null) {
        extAmmoDef.TagsAccuracyModifiers = JsonConvert.DeserializeObject<Dictionary<string, float>>(defTemp["ChassisTagsAccuracyModifiers"].ToString());
        Log.LogWrite((string)defTemp["Description"]["Id"]+ " ChassisTagsAccuracyModifiers:\n");
        foreach (var tam in extAmmoDef.TagsAccuracyModifiers) {
          Log.LogWrite(" "+tam.Key+":"+tam.Key);
        }
        defTemp.Remove("ChassisTagsAccuracyModifiers");
      }
      if (defTemp["HitGenerator"] != null) {
        try {
          extAmmoDef.HitGenerator = (HitGeneratorType)Enum.Parse(typeof(HitGeneratorType), (string)defTemp["HitGenerator"], true);
        } catch (Exception) {
          extAmmoDef.HitGenerator = HitGeneratorType.NotSet;
        }
        defTemp.Remove("HitGenerator");
      }
      if (defTemp["MineField"] != null) {
        extAmmoDef.MineField.fromJSON(defTemp["MineField"]);
        defTemp.Remove("MineField");
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
      CustomAmmoCategories.RegisterExtAmmoDef((string)defTemp["Description"]["Id"], extAmmoDef);
      json = defTemp.ToString(Formatting.Indented);
      Log.LogWrite(json + "\n");
      return true;
    }
    public static void Postfix(AmmunitionDef __instance) {
      EffectData[] effects = CustomAmmoCategories.findExtAmmo(__instance.Description.Id).statusEffects;
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
        CustomAmmoCategories.findExtAmmo(__instance.Description.Id).statusEffects = tmpList.ToArray();
      }
    }
  }
}
