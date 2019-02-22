using BattleTech;
using CustAmmoCategories;
using Harmony;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CustAmmoCategories {
  public class ExtWeaponDef {
    public HitGeneratorType HitGenerator { get; set; }
    public bool StreakEffect { get; set; }
    public float DirectFireModifier { get; set; }
    public string baseModeId { get; set; }
    public float FlatJammingChance { get; set; }
    public float GunneryJammingBase { get; set; }
    public float GunneryJammingMult { get; set; }
    public TripleBoolean DamageOnJamming { get; set; }
    public Dictionary<string, WeaponMode> Modes { get; set; }
    public CustomAmmoCategory AmmoCategory { get; set; }
    public TripleBoolean DisableClustering { get; set; }
    public ExtWeaponDef() {
      StreakEffect = false;
      HitGenerator = HitGeneratorType.NotSet;
      DirectFireModifier = 0;
      FlatJammingChance = 0;
      GunneryJammingBase = 0;
      GunneryJammingMult = 0;
      baseModeId = WeaponMode.NONE_MODE_NAME;
      DisableClustering = TripleBoolean.True;
      DamageOnJamming = TripleBoolean.NotSet;
      AmmoCategory = new CustomAmmoCategory();
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
        } catch (Exception e) {
          extDef.HitGenerator = HitGeneratorType.NotSet;
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
      if (defTemp["DisableClustering"] != null) {
        extDef.DisableClustering = ((bool)defTemp["DisableClustering"] == true) ? TripleBoolean.True : TripleBoolean.False;
        defTemp.Remove("DisableClustering");
      }
      if (defTemp["DamageOnJamming"] != null) {
        extDef.DamageOnJamming = ((bool)defTemp["DamageOnJamming"] == true) ? TripleBoolean.True : TripleBoolean.False;
        defTemp.Remove("DamageOnJamming");
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
