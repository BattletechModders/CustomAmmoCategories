using BattleTech;
using CustAmmoCategories;
using Harmony;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CustAmmoCategories {
  public class ExtWeaponDef {
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
    public bool IsAMS { get; set; }
    public bool IsAAMS { get; set; }
    public bool AMSShootsEveryAttack { get; set; }
    public float SpreadRange { get; set; }
    public TripleBoolean NotUseInMelee { get; set; }
    public Dictionary<string, WeaponMode> Modes { get; set; }
    public CustomAmmoCategory AmmoCategory { get; set; }
    public TripleBoolean DisableClustering { get; set; }
    public bool AMSShootedThisAttackSequence { get; set; }
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
    //public TripleBoolean AOECapable { get; set; }
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
      IsAMS = false;
      IsAAMS = false;
      AMSShootedThisAttackSequence = false;
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
      if (defTemp["AlternateDamageCalc"] != null) {
        extDef.AlternateDamageCalc = (bool)defTemp["AlternateDamageCalc"];
        defTemp.Remove("AlternateDamageCalc");
      }
      if (defTemp["AMSShootsEveryAttack"] != null) {
        extDef.AMSShootsEveryAttack = (bool)defTemp["AMSShootsEveryAttack"];
        defTemp.Remove("AMSShootsEveryAttack");
      }
      if (defTemp["AlternateHeatDamageCalc"] != null) {
        extDef.AlternateHeatDamageCalc = (bool)defTemp["AlternateHeatDamageCalc"];
        defTemp.Remove("AlternateHeatDamageCalc");
      }
      if (defTemp["IsAMS"] != null) {
        extDef.IsAMS = (bool)defTemp["IsAMS"];
        if (extDef.IsAMS) {
          extDef.StreakEffect = true;
        }
        defTemp.Remove("IsAMS");
      }
      if (defTemp["IsAAMS"] != null) {
        extDef.IsAAMS = (bool)defTemp["IsAAMS"];
        if (extDef.IsAAMS) {
          extDef.IsAMS = true;
          extDef.StreakEffect = true;
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
      //CustomAmmoCategoriesLog.Log.LogWrite("\n--------------ORIG----------------\n" + json + "\n----------------------------------\n");
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
