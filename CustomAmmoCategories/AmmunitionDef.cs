using BattleTech;
using CustAmmoCategories;
using Harmony;
using HBS.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CustAmmoCategories { 
  public class ExtAmmunitionDef {
    public float AccuracyModifier { get; set; }
    public float DirectFireModifier { get; set; }
    public float DamagePerShot { get; set; }
    public float HeatDamagePerShot { get; set; }
    public float CriticalChanceMultiplier { get; set; }
    public int ShotsWhenFired { get; set; }
    public int AIBattleValue { get; set; }
    public int ProjectilesPerShot { get; set; }
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
    public CustomAmmoCategory AmmoCategory { get; set; }
    public float SpreadRange { get; set; }
    public float AOERange { get; set; }
    public float AOEDamage { get; set; }
    public float AOEHeatDamage { get; set; }
    public float AOEInstability { get; set; }
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
    public int MineFieldRadius { get; set; }
    public float MineFieldHitChance { get; set; }
    public float MineFieldDamage { get; set; }
    public float MineFieldHeat { get; set; }
    public float MineFieldInstability { get; set; }
    public int MineFieldCount { get; set; }
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
    public ExtAmmunitionDef() {
      AccuracyModifier = 0;
      DirectFireModifier = 0;
      DamagePerShot = 0;
      HeatDamagePerShot = 0;
      ProjectilesPerShot = 0;
      ShotsWhenFired = 0;
      CriticalChanceMultiplier = 0;
      DamageMultiplier = 1.0f;
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
      MineFieldRadius = 0;
      MineFieldHitChance = 0f;
      MineFieldDamage = 0f;
      MineFieldHeat = 0f;
      MineFieldCount = 0;
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
      MineFieldInstability = 0f;
      ClearMineFieldRadius = 0;
      IsAMS = TripleBoolean.NotSet;
      IsAAMS = TripleBoolean.NotSet;
      BallisticDamagePerPallet = TripleBoolean.NotSet;
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
      CustomAmmoCategoriesLog.Log.LogWrite("AmmunitionDef fromJSON ");
      JObject defTemp = JObject.Parse(json);
      CustomAmmoCategoriesLog.Log.LogWrite(defTemp["Description"]["Id"]+"\n");
      CustomAmmoCategory custCat = CustomAmmoCategories.find((string)defTemp["Category"]);
      //CustomAmmoCategories.RegisterAmmunition((string)defTemp["Description"]["Id"], custCat);
      defTemp["Category"] = custCat.BaseCategory.ToString();
      ExtAmmunitionDef extAmmoDef = new ExtAmmunitionDef();
      extAmmoDef.AmmoCategory = custCat;
      if (defTemp["AccuracyModifier"] != null) {
        extAmmoDef.AccuracyModifier = (float)defTemp["AccuracyModifier"];
        defTemp.Remove("AccuracyModifier");
      }
      if (defTemp["DamagePerShot"] != null) {
        extAmmoDef.DamagePerShot = (float)defTemp["DamagePerShot"];
        defTemp.Remove("DamagePerShot");
      }
      if (defTemp["HeatDamagePerShot"] != null) {
        extAmmoDef.HeatDamagePerShot = (float)defTemp["HeatDamagePerShot"];
        defTemp.Remove("HeatDamagePerShot");
      }
      if (defTemp["ProjectilesPerShot"] != null) {
        extAmmoDef.ProjectilesPerShot = (int)defTemp["ProjectilesPerShot"];
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
      if (defTemp["AIBattleValue"] != null) {
        extAmmoDef.AIBattleValue = (int)defTemp["AIBattleValue"];
        defTemp.Remove("AIBattleValue");
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
      if (defTemp["CanBeExhaustedAt"] != null) {
        extAmmoDef.CanBeExhaustedAt = (float)defTemp["CanBeExhaustedAt"];
        defTemp.Remove("CanBeExhaustedAt");
      }
      if (defTemp["MineFieldRadius"] != null) {
        extAmmoDef.MineFieldRadius = (int)defTemp["MineFieldRadius"];
        defTemp.Remove("MineFieldRadius");
      }
      if (defTemp["ClearMineFieldRadius"] != null) {
        extAmmoDef.ClearMineFieldRadius = (int)defTemp["ClearMineFieldRadius"];
        defTemp.Remove("ClearMineFieldRadius");
      }
      if (defTemp["MineFieldHitChance"] != null) {
        extAmmoDef.MineFieldHitChance = (float)defTemp["MineFieldHitChance"];
        defTemp.Remove("MineFieldHitChance");
      }
      if (defTemp["MineFieldDamage"] != null) {
        extAmmoDef.MineFieldDamage = (float)defTemp["MineFieldDamage"];
        defTemp.Remove("MineFieldDamage");
      }
      if (defTemp["BallisticDamagePerPallet"] != null) {
        extAmmoDef.BallisticDamagePerPallet = ((bool)defTemp["BallisticDamagePerPallet"] == true) ? TripleBoolean.True : TripleBoolean.False;
        defTemp.Remove("BallisticDamagePerPallet");
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
        extAmmoDef.MineFieldHeat = (float)defTemp["MineFieldHeat"];
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
      if (defTemp["AdditionalImpactVFX"] != null) {
        extAmmoDef.AdditionalImpactVFX = (string)defTemp["AdditionalImpactVFX"];
        defTemp.Remove("AdditionalImpactVFX");
      }
      if (defTemp["FireOnSuccessHit"] != null) {
        extAmmoDef.FireOnSuccessHit = ((bool)defTemp["FireOnSuccessHit"] == true) ? TripleBoolean.True : TripleBoolean.False;
        defTemp.Remove("FireOnSuccessHit");
      }
      if (defTemp["MineFieldCount"] != null) {
        extAmmoDef.MineFieldCount = (int)defTemp["MineFieldCount"];
        defTemp.Remove("MineFieldCount");
      }
      if (defTemp["MineFieldInstability"] != null) {
        extAmmoDef.MineFieldInstability = (float)defTemp["MineFieldInstability"];
        defTemp.Remove("MineFieldInstability");
      }
      if (defTemp["IFFDef"] != null) {
        extAmmoDef.IFFDef = (string)defTemp["IFFDef"];
        defTemp.Remove("IFFDef");
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
      if (defTemp["AlwaysIndirectVisuals"] != null) {
        extAmmoDef.AlwaysIndirectVisuals = ((bool)defTemp["AlwaysIndirectVisuals"] == true) ? TripleBoolean.True : TripleBoolean.False;
        defTemp.Remove("AlwaysIndirectVisuals");
      }
      if (defTemp["Unguided"] != null) {
        extAmmoDef.Unguided = ((bool)defTemp["Unguided"] == true) ? TripleBoolean.True : TripleBoolean.False;
        defTemp.Remove("Unguided");
        if(extAmmoDef.Unguided == TripleBoolean.True) {
          extAmmoDef.AlwaysIndirectVisuals = TripleBoolean.False;
          extAmmoDef.IndirectFireCapable = TripleBoolean.False;
        }
      }
      if (defTemp["AOECapable"] != null) {
        extAmmoDef.AOECapable = ((bool)defTemp["AOECapable"] == true) ? TripleBoolean.True : TripleBoolean.False;
        if (extAmmoDef.AOECapable == TripleBoolean.True) {
          extAmmoDef.AlwaysIndirectVisuals = TripleBoolean.True;
        }
        defTemp.Remove("AOECapable");
      }
      if (defTemp["DamageMultiplier"] != null) {
        extAmmoDef.DamageMultiplier = (float)defTemp["DamageMultiplier"];
        defTemp.Remove("DamageMultiplier");
      }
      if (defTemp["DistantVarianceReversed"] != null) {
        extAmmoDef.DistantVarianceReversed = ((bool)defTemp["DistantVarianceReversed"] == true) ? TripleBoolean.True : TripleBoolean.False;
        defTemp.Remove("DistantVarianceReversed");
      }
      if (defTemp["DamageVariance"] != null) {
        extAmmoDef.DamageVariance = (float)defTemp["DamageVariance"];
        defTemp.Remove("DamageVariance");
      }
      if (defTemp["HitGenerator"] != null) {
        try {
          extAmmoDef.HitGenerator = (HitGeneratorType)Enum.Parse(typeof(HitGeneratorType), (string)defTemp["HitGenerator"], true);
        } catch (Exception) {
          extAmmoDef.HitGenerator = HitGeneratorType.NotSet;
        }
        defTemp.Remove("HitGenerator");
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
      json = defTemp.ToString();
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
