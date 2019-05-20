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
    public HitGeneratorType HitGenerator { get; set; }
    public bool StreakEffect { get; set; }
    public float DirectFireModifier { get; set; }
    public string baseModeId { get; set; }
    public float FlatJammingChance { get; set; }
    public float AMSHitChance { get; set; }
    public float GunneryJammingBase { get; set; }
    public float GunneryJammingMult { get; set; }
    public TripleBoolean DamageOnJamming { get; set; }
    public TripleBoolean AMSImmune { get; set; }
    public bool AlternateDamageCalc { get; set; }
    public bool AlternateHeatDamageCalc { get; set; }
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
    public ExtWeaponDef() {
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
      AMSShootsEveryAttack = false;
      baseModeId = WeaponMode.NONE_MODE_NAME;
      DisableClustering = TripleBoolean.True;
      NotUseInMelee = TripleBoolean.NotSet;
      DamageOnJamming = TripleBoolean.NotSet;
      AmmoCategory = new CustomAmmoCategory();
      AOECapable = TripleBoolean.NotSet;
      AOERange = 0f;
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
      CustomAmmoCategoriesLog.Log.LogWrite(defTemp["Description"]["Id"]+"\n");
      CustomAmmoCategory custCat = CustomAmmoCategories.find((string)defTemp["AmmoCategory"]);
      //CustomAmmoCategories.RegisterWeapon((string)defTemp["Description"]["Id"], custCat);
      ExtWeaponDef extDef = new ExtWeaponDef();
      extDef.AmmoCategory = custCat;
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
      if (defTemp["GunneryJammingBase"] != null) {
        extDef.GunneryJammingBase = (float)defTemp["GunneryJammingBase"];
        defTemp.Remove("GunneryJammingBase");
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
      if (defTemp["AMSShootsEveryAttack"] != null) {
        extDef.AMSShootsEveryAttack = (bool)defTemp["AMSShootsEveryAttack"];
        defTemp.Remove("AMSShootsEveryAttack");
      }
      if (defTemp["AdditionalImpactVFXScaleX"] != null) {
        extDef.AdditionalImpactVFXScaleX = (float)defTemp["AdditionalImpactVFXScaleX"];
        defTemp.Remove("AdditionalImpactVFXScaleX");
      }
      if (defTemp["AdditionalImpactVFXScaleY"] != null) {
        extDef.AdditionalImpactVFXScaleY = (float)defTemp["AdditionalImpactVFXScaleY"];
        defTemp.Remove("AdditionalImpactVFXScaleY");
      }
      if (defTemp["AdditionalImpactVFXScaleZ"] != null) {
        extDef.AdditionalImpactVFXScaleZ = (float)defTemp["AdditionalImpactVFXScaleZ"];
        defTemp.Remove("AdditionalImpactVFXScaleZ");
      }
      if (defTemp["AlternateHeatDamageCalc"] != null) {
        extDef.AlternateHeatDamageCalc = (bool)defTemp["AlternateHeatDamageCalc"];
        defTemp.Remove("AlternateHeatDamageCalc");
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
      if (defTemp["ImprovedBallistic"] != null) {
        extDef.ImprovedBallistic = (bool)defTemp["ImprovedBallistic"];
        if (extDef.ImprovedBallistic) {
          ExtWeaponDef.RemoveTag(ref defTemp, "wr-clustered_shots");
          extDef.DisableClustering = TripleBoolean.True;
        }
        defTemp.Remove("ImprovedBallistic");
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
      if (defTemp["Unguided"] != null) {
        extDef.Unguided = ((bool)defTemp["Unguided"] == true) ? TripleBoolean.True : TripleBoolean.False;
        defTemp.Remove("Unguided");
      }
      if (defTemp["NotUseInMelee"] != null) {
        extDef.NotUseInMelee = ((bool)defTemp["NotUseInMelee"] == true) ? TripleBoolean.True : TripleBoolean.False;
        defTemp.Remove("NotUseInMelee");
      }
      if (defTemp["DamageOnJamming"] != null) {
        extDef.DamageOnJamming = ((bool)defTemp["DamageOnJamming"] == true) ? TripleBoolean.True : TripleBoolean.False;
        defTemp.Remove("DamageOnJamming");
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
      }
      CustomAmmoCategories.registerExtWeaponDef((string)defTemp["Description"]["Id"], extDef);
      defTemp["AmmoCategory"] = custCat.BaseCategory.ToString();
      CustomAmmoCategoriesLog.Log.LogWrite("\n--------------ORIG----------------\n" + json + "\n----------------------------------\n");
      CustomAmmoCategoriesLog.Log.LogWrite("\n--------------MOD----------------\n" + defTemp.ToString() + "\n----------------------------------\n");
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
