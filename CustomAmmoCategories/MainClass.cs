using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Harmony;
using HBS.Util;
using BattleTech;
using BattleTech.UI;
using HBS.Collections;
using Localize;
using BattleTech.Data;
using UnityEngine.EventSystems;
using CustAmmoCategories;
using UnityEngine;
using HBS;
using System.Threading;

namespace CustomAmmoCategoriesLog {
  public static class Log {
    //private static string m_assemblyFile;
    private static string m_logfile;
    public static string BaseDirectory;
    public static void InitLog() {
      //Log.m_assemblyFile = (new System.Uri(Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath;
      Log.m_logfile = Path.Combine(BaseDirectory, "CustomAmmoCategories.log");
      //Log.m_logfile = Path.Combine(Log.m_logfile, "CustomAmmoCategories.log");
      File.Delete(Log.m_logfile);
    }
    public static void LogWrite(string line) {
      if (CustomAmmoCategories.Settings.debugLog) {
        File.AppendAllText(Log.m_logfile, line);
      }
    }
  }
}

namespace CustomAmmoCategoriesPatches {
  [HarmonyPatch(typeof(CombatHUDWeaponSlot))]
  [HarmonyPatch("OnPointerDown")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(PointerEventData) })]
  public static class CombatHUDWeaponSlot_OnPointerDown {
    public static bool Prefix(CombatHUDWeaponSlot __instance, PointerEventData eventData) {

      CustomAmmoCategoriesLog.Log.LogWrite("CombatHUDWeaponSlot.OnPointerDown\n");
      if (eventData.button != PointerEventData.InputButton.Left) { return true; }
      if (__instance.weaponSlotType != CombatHUDWeaponSlot.WeaponSlotType.Normal) { return true; }
      if (__instance.DisplayedWeapon == null) { return true; }
      Camera mainUiCamera = LazySingletonBehavior<UIManager>.Instance.UICamera;
      if (mainUiCamera == null) {
        CustomAmmoCategoriesLog.Log.LogWrite("  can't get UI camera\n");
      }
      Vector3 worldClickPos = mainUiCamera.ScreenToWorldPoint(eventData.position);
      Vector3[] corners = new Vector3[4];
      __instance.GetComponent<RectTransform>().GetWorldCorners(corners);
      float width = corners[2].x - corners[0].x;
      float height = __instance.GetComponent<RectTransform>().rect.height;
      float clickXrel = worldClickPos.x - __instance.transform.position.x;
      bool trigger_ammo = clickXrel > ((width / 3.0f)*2.0f);
      bool trigger_mode = clickXrel > ((width / 3.0f));
      CustomAmmoCategoriesLog.Log.LogWrite("  instance.l = " + __instance.transform.position.x + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  instance.t = " + __instance.transform.position.y + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  instance.r = " + (__instance.transform.position.x + width) + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  instance.b = " + (__instance.transform.position.y + height) + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  position.x = " + eventData.position.x + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  position.y = " + eventData.position.y + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  worldClickPos.x = " + worldClickPos.x + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  worldClickPos.y = " + worldClickPos.y + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  clickXrel = " + clickXrel + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  width = " + width + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  trigger_ammo = " + trigger_ammo + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  trigger_mode = " + trigger_mode + "\n");
      if (trigger_ammo) {
        if ((CustomAmmoCategories.IsJammed(__instance.DisplayedWeapon) == false) 
          && (CustomAmmoCategories.isWRJammed(__instance.DisplayedWeapon) == false)
          && (CustomAmmoCategories.IsCooldown(__instance.DisplayedWeapon) <= 0)) {
          CustomAmmoCategories.CycleAmmo(__instance.DisplayedWeapon);
        }
        __instance.RefreshDisplayedWeapon((ICombatant)null);
        return false;
      } else
      if (trigger_mode) {
        if ((CustomAmmoCategories.IsJammed(__instance.DisplayedWeapon) == false)
          && (CustomAmmoCategories.isWRJammed(__instance.DisplayedWeapon) == false)
          && (CustomAmmoCategories.IsCooldown(__instance.DisplayedWeapon) <= 0)) {
          CustomAmmoCategories.CycleMode(__instance.DisplayedWeapon);
        }
        __instance.RefreshDisplayedWeapon((ICombatant)null);
        return false;
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(CombatHUDWeaponSlot))]
  [HarmonyPatch("OnPointerUp")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(PointerEventData) })]
  public static class CombatHUDWeaponSlot_OnPointerUp {
    public static bool Prefix(CombatHUDWeaponSlot __instance, PointerEventData eventData) {

      Camera mainUiCamera = LazySingletonBehavior<UIManager>.Instance.UICamera;
      if (mainUiCamera == null) {
        CustomAmmoCategoriesLog.Log.LogWrite("  can't get UI camera\n");
      }
      Vector3 worldClickPos = mainUiCamera.ScreenToWorldPoint(eventData.position);
      Vector3[] corners = new Vector3[4];
      __instance.GetComponent<RectTransform>().GetWorldCorners(corners);
      float width = corners[2].x - corners[0].x;
      float height = __instance.GetComponent<RectTransform>().rect.height;
      float clickXrel = worldClickPos.x - __instance.transform.position.x;
      bool trigger = clickXrel > (width / 3.0f);
      CustomAmmoCategoriesLog.Log.LogWrite("  instance.l = " + __instance.transform.position.x + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  instance.t = " + __instance.transform.position.y + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  instance.r = " + (__instance.transform.position.x + width) + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  instance.b = " + (__instance.transform.position.y + height) + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  position.x = " + eventData.position.x + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  position.y = " + eventData.position.y + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  worldClickPos.x = " + worldClickPos.x + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  worldClickPos.y = " + worldClickPos.y + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  clickXrel = " + clickXrel + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  width = " + width + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  trigger = " + trigger + "\n");
      if (trigger) {
        return false;
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(CombatHUDWeaponSlot))]
  [HarmonyPatch("RefreshHighlighted")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatHUDWeaponSlot_RefreshHighlighted {
    public static bool Prefix(CombatHUDWeaponSlot __instance) {
      if (__instance.DisplayedWeapon == null) { return false; };
      return true;
    }
  }

  [HarmonyPatch(typeof(MechLabPanel))]
  [HarmonyPatch("MechCanUseAmmo")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AmmunitionBoxDef) })]
  public static class MechLabPanel_MechCanUseAmmo {
    public static void Postfix(MechLabPanel __instance, AmmunitionBoxDef ammoBoxDef,ref bool __result) {
      if(ammoBoxDef.Ammo.Category == AmmoCategory.Flamer) { //patch for energy weapon ammo
        if(CustomAmmoCategories.findExtAmmo(ammoBoxDef.Ammo.Description.Id).AmmoCategory.Id != "Flamer") {
          MechLabMechInfoWidget mechInfoWidget = (MechLabMechInfoWidget)(typeof(MechLabPanel)).GetField("mechInfoWidget", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
          __result = mechInfoWidget.totalEnergyHardpoints > 0;
        }
      }
    }
  }
  [HarmonyPatch(typeof(Weapon))]
  [HarmonyPatch("WillFire")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class Weapon_WillFire {
    public static bool Prefix(Weapon __instance) {
      return true;
    }
  }
  [HarmonyPatch(typeof(AttackDirector.AttackSequence))]
  [HarmonyPatch("Cleanup")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class AttackSequence_Cleanup {
    public static void Postfix(AttackDirector.AttackSequence __instance) {
      CustomAmmoCategoriesLog.Log.LogWrite("AttackDirector.AttackSequence.Cleanup\n");
      List<List<Weapon>> sortedWeapons = ((List<List<Weapon>>)typeof(AttackDirector.AttackSequence).GetField("sortedWeapons", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance));
      foreach (List<Weapon> weapons in sortedWeapons) {
        foreach (Weapon weapon in weapons) {
          CustomAmmoCategoriesLog.Log.LogWrite("  weapon " + weapon.Name + "\n");
          if (weapon.AmmoCategory == AmmoCategory.NotSet) { continue; }
          if (weapon.ammoBoxes.Count <= 0) { continue; }
          if (weapon.CurrentAmmo > 0) { continue; }
          CustomAmmoCategories.CycleAmmoBest(weapon);
        }
      }
      //string wGUID = weapon.parent.GUID + "." + weapon.uid;
      //CustomAmmoCategoriesLog.Log.LogWrite("  wGUID " + wGUID + "\n");
      /*string aGUID = "";
      if (CustomAmmoCategories.findPlayerWeaponAmmoGUID(wGUID, ref aGUID) == false) { return; }
      if ((aGUID < 0) || (aGUID >= weapon.ammoBoxes.Count)) { return; }
      CustomAmmoCategoriesLog.Log.LogWrite("  player weapon found. Ammo " + weapon.ammoBoxes[aGUID].CurrentAmmo + "\n");
      if ((weapon.ammoBoxes[aGUID].CurrentAmmo > 0) && (weapon.ammoBoxes[aGUID].IsFunctional == true)) {
          CustomAmmoCategoriesLog.Log.LogWrite("  Cycling not needed\n");
          return;
      }
      CustomAmmoCategories.CycleAmmo(weapon,true);*/
    }
  }
  [HarmonyPatch(typeof(CombatHUDWeaponSlot))]
  [HarmonyPatch("RefreshDisplayedWeapon")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ICombatant) })]
  public static class CombatHUDWeaponSlot_RefreshDisplayedWeapon {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "ShotsWhenFired").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(CombatHUDWeaponSlot_RefreshDisplayedWeapon),
          nameof(ShotsWhenFiredDisplayOverider));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }

    private static int ShotsWhenFiredDisplayOverider(Weapon weapon) {
      return weapon.ShotsWhenFired * weapon.ProjectilesPerShot;
    }
  }

  [HarmonyPatch(typeof(CombatGameState))]
  [HarmonyPatch("RebuildAllLists")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatGameState_RebuildAllLists {
    public static void Postfix(CombatGameState __instance) {
      CustomAmmoCategoriesLog.Log.LogWrite("CombatGameState.RebuildAllLists\n");
      //CustomAmmoCategories.ClearPlayerWeapons();
      foreach (var unit in __instance.AllActors) {
        //if (unit is Mech)
        //{
        CustomAmmoCategoriesLog.Log.LogWrite("  " + unit.DisplayName + "\n");
        foreach (var Weapon in unit.Weapons) {
          CustomAmmoCategories.RegisterPlayerWeapon(Weapon);
        }
        //}
      }
    }
  }
  [HarmonyPatch(typeof(CombatGameState))]
  [HarmonyPatch("OnCombatGameDestroyed")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatGameState_OnCombatGameDestroyed {
    public static void Postfix(CombatGameState __instance) {
      CustomAmmoCategoriesLog.Log.LogWrite("CombatGameState.OnCombatGameDestroyed\n");
      //CustomAmmoCategories.clearAllWeaponEffects();
    }
  }
  [HarmonyPatch(typeof(CombatHUD))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(CombatGameState) })]
  public static class CombatHUD_Init {
    public static bool Prefix(CombatHUD __instance, CombatGameState Combat) {
      CustomAmmoCategoriesLog.Log.LogWrite("pre CombatHUD.Init\n");
      //CustomAmmoCategories.ClearPlayerWeapons();
      foreach (var unit in Combat.AllActors) {
        CustomAmmoCategoriesLog.Log.LogWrite("  " + unit.DisplayName + "\n");
        foreach (var Weapon in unit.Weapons) {
          CustomAmmoCategories.RegisterPlayerWeapon(Weapon);
        }
      }
      return true;
    }
  }

  [HarmonyPatch(typeof(Weapon))]
  [HarmonyPatch("DecrementAmmo")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(int) })]
  public static class Weapon_DecrementAmmo {
    public static bool Prefix(Weapon __instance, int stackItemUID, ref int __result) {
      __result = CustomAmmoCategories.DecrementAmmo(__instance, stackItemUID, 0);
      return false;
    }
  }

  [HarmonyPatch(typeof(AmmunitionDef))]
  [HarmonyPatch("FromJSON")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class BattleTech_AmmunitionDef_fromJSON_Patch {
    public static bool Prefix(AmmunitionDef __instance, ref string json) {
      CustomAmmoCategoriesLog.Log.LogWrite("AmmunitionDef fromJSON\n");
      JObject defTemp = JObject.Parse(json);
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
      if (defTemp["EvasivePipsIgnored"] != null) {
        extAmmoDef.EvasivePipsIgnored = (float)defTemp["EvasivePipsIgnored"];
        defTemp.Remove("EvasivePipsIgnored");
      }
      if (defTemp["FlatJammingChance"] != null) {
        extAmmoDef.FlatJammingChance = (float)defTemp["FlatJammingChance"];
        defTemp.Remove("FlatJammingChance");
      }
      if (defTemp["IndirectFireCapable"] != null) {
        extAmmoDef.IndirectFireCapable = ((bool)defTemp["IndirectFireCapable"] == true) ? TripleBoolean.True : TripleBoolean.False;
        defTemp.Remove("IndirectFireCapable");
      }
      if (defTemp["DirectFireModifier"] != null) {
        extAmmoDef.DirectFireModifier = (float)defTemp["DirectFireModifier"];
        defTemp.Remove("DirectFireModifier");
      }
      if (defTemp["AOECapable"] != null) {
        extAmmoDef.AOECapable = ((bool)defTemp["IndirectFireCapable"] == true) ? TripleBoolean.True : TripleBoolean.False;
        defTemp.Remove("AOECapable");
      }
      if (defTemp["DistantVariance"] != null) {
        extAmmoDef.DistantVariance = (float)defTemp["DistantVariance"];
        defTemp.Remove("DistantVariance");
      }
      if (defTemp["DistantVarianceReversed"] != null) {
        extAmmoDef.DistantVarianceReversed = ((bool)defTemp["IndirectFireCapable"] == true) ? TripleBoolean.True : TripleBoolean.False;
        defTemp.Remove("DistantVarianceReversed");
      }
      if (defTemp["DamageVariance"] != null) {
        extAmmoDef.DamageVariance = (float)defTemp["DamageVariance"];
        defTemp.Remove("DamageVariance");
      }
      if (defTemp["HitGenerator"] != null) {
        try {
          extAmmoDef.HitGenerator = (HitGeneratorType)Enum.Parse(typeof(HitGeneratorType), (string)defTemp["HitGenerator"], true);
        } catch (Exception e) {
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

  [HarmonyPatch(typeof(WeaponDef))]
  [HarmonyPatch("FromJSON")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class BattleTech_WeaponDef_fromJSON_Patch {
    public static bool Prefix(WeaponDef __instance, ref string json) {
      CustomAmmoCategoriesLog.Log.LogWrite("WeaponDef fromJSON\n");
      JObject defTemp = JObject.Parse(json);
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
      if (defTemp["DirectFireModifier"] != null) {
        extDef.DirectFireModifier = (float)defTemp["DirectFireModifier"];
        defTemp.Remove("DirectFireModifier");
      }
      if (defTemp["DamageOnJamming"] != null) {
        extDef.DamageOnJamming = ((bool)defTemp["DamageOnJamming"] == true)?TripleBoolean.True:TripleBoolean.False;
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
            CustomAmmoCategoriesLog.Log.LogWrite(" adding mode '"+mode.Id+"'");
            extDef.Modes.Add(mode.Id, mode);
            if (mode.isBaseMode == true) { extDef.baseModeId = mode.Id; }
          }
        }
        defTemp.Remove("Modes");
      }
      if (extDef.baseModeId == WeaponMode.NONE_MODE_NAME) {
        WeaponMode mode = new WeaponMode();
        mode.Id = WeaponMode.BASE_MODE_NAME;
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

  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("CalcAndSetAlphaStrikesRemaining")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class AbstractActor_CalcAlphaStrikesRem_Patch {
    public static bool Prefix(AbstractActor __instance) {
      if (__instance.ammoBoxes.Count < 1)
        return false;
      Dictionary<int, int> dictionary1 = new Dictionary<int, int>();
      for (int index1 = 0; index1 < __instance.Weapons.Count; ++index1) {
        int ammoCategory = CustomAmmoCategories.getExtWeaponDef(__instance.Weapons[index1].defId).AmmoCategory.Index;
          //CustomAmmoCategories.findWeaponRealCategory(__instance.Weapons[index1].Description.Id, __instance.Weapons[index1].AmmoCategory.ToString()).Index;
        //ammoCategory ammoCategory = this.Weapons[index1].AmmoCategory;
        if (ammoCategory != 0) {
          if (dictionary1.ContainsKey(ammoCategory)) {
            Dictionary<int, int> dictionary2;
            int index2;
            (dictionary2 = dictionary1)[index2 = ammoCategory] = dictionary2[index2] + __instance.Weapons[index1].ShotsWhenFired;
          } else
            dictionary1[ammoCategory] = __instance.Weapons[index1].ShotsWhenFired;
        }
      }
      Dictionary<int, int> dictionary3 = new Dictionary<int, int>();
      for (int index1 = 0; index1 < __instance.ammoBoxes.Count; ++index1) {
        int ammoCategory = CustomAmmoCategories.findExtAmmo(__instance.ammoBoxes[index1].ammoDef.Description.Id).AmmoCategory.Index;
          //CustomAmmoCategories.findAmunitionRealCategory(__instance.ammoBoxes[index1].ammoDef.Description.Id, __instance.ammoBoxes[index1].ammoCategory.ToString()).Index;
        if (dictionary3.ContainsKey(ammoCategory)) {
          Dictionary<int, int> dictionary2;
          int index2;
          (dictionary2 = dictionary3)[index2 = ammoCategory] = dictionary2[index2] + __instance.ammoBoxes[index1].CurrentAmmo;
        } else
          dictionary3[ammoCategory] = __instance.ammoBoxes[index1].CurrentAmmo;
      }
      Dictionary<int, float> dictionary4 = new Dictionary<int, float>();
      foreach (KeyValuePair<int, int> keyValuePair in dictionary1) {
        int key = keyValuePair.Key;
        dictionary4[key] = dictionary3.ContainsKey(key) ? (float)dictionary3[key] / (float)dictionary1[key] : 0.0f;
      }
      for (int index = 0; index < __instance.Weapons.Count; ++index) {
        int ammoCategory = CustomAmmoCategories.getExtWeaponDef(__instance.Weapons[index].defId).AmmoCategory.Index;
          //CustomAmmoCategories.findWeaponRealCategory(__instance.Weapons[index].Description.Id, __instance.Weapons[index].AmmoCategory.ToString()).Index;
        if (dictionary4.ContainsKey(ammoCategory))
          __instance.Weapons[index].AlphaStrikesRemaining = dictionary4[ammoCategory] + (float)__instance.Weapons[index].InternalAmmo;
      }
      return false;
    }
  }
  [HarmonyPatch(typeof(Weapon))]
  [HarmonyPatch("SetAmmoBoxes")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(List<AmmunitionBox>) })]
  public static class Weapon_SetAmmoBoxes {
    public static bool Prefix(Weapon __instance, List<AmmunitionBox> ammoBoxes) {
      CustomAmmoCategoriesLog.Log.LogWrite("Weapon SetAmmoBoxes " + __instance.Description.Id + "\n");
      CustomAmmoCategory weaponAmmoCategory = CustomAmmoCategories.getExtWeaponDef(__instance.defId).AmmoCategory;
      //CustomAmmoCategories.findWeaponRealCategory(__instance.Description.Id, __instance.AmmoCategory.ToString()).Index;
      List<AmmunitionBox> ammunitionBoxList = new List<AmmunitionBox>();
      CustomAmmoCategoriesLog.Log.LogWrite("  Weapon Ammo Catgory " + weaponAmmoCategory.Id + "\n");
      foreach (AmmunitionBox ammoBox in ammoBoxes) {
        CustomAmmoCategory ammoBoxAmmoCategory = CustomAmmoCategories.findExtAmmo(ammoBox.ammoDef.Description.Id).AmmoCategory;
          //CustomAmmoCategories.findAmunitionRealCategory(ammoBox.ammoDef.Description.Id, ammoBox.ammoCategory.ToString()).Index;
        CustomAmmoCategoriesLog.Log.LogWrite("  Ammunition Box " + ammoBox.ammoDef.Description.Id + " - " + ammoBoxAmmoCategory + "\n");

        if (ammoBoxAmmoCategory.Index == weaponAmmoCategory.Index)
          ammunitionBoxList.Add(ammoBox);
      }
      __instance.ammoBoxes = ammunitionBoxList;
      return false;
    }
  }
  [HarmonyPatch(typeof(Weapon))]
  [HarmonyPatch("CurrentAmmo")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class Weapon_CurrentAmmo {
    public static bool Prefix(Weapon __instance, ref int __result) {
      //CustomCategoriesLog.LogWrite("Weapon CurrentAmmo " + __instance.Description.Id + "\n");
      if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.AmmoIdStatName) == false) { return true; }
      string CurrentAmmoId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
      __result = __instance.InternalAmmo;
      //CustomCategoriesLog.LogWrite("  internal ammo "+ __result.ToString());
      for (int index = 0; index < __instance.ammoBoxes.Count; ++index) {
        //CustomCategoriesLog.LogWrite("  AmmoBox " + __instance.ammoBoxes[index].Description.Id+" "+ __instance.ammoBoxes[index].CurrentAmmo+"\n");
        if (__instance.ammoBoxes[index].CurrentAmmo <= 0) { continue; }
        if (__instance.ammoBoxes[index].IsFunctional == false) { continue; }
        if (__instance.ammoBoxes[index].ammoDef.Description.Id != CurrentAmmoId) { continue; };
        __result += __instance.ammoBoxes[index].CurrentAmmo;
      }
      //CustomCategoriesLog.LogWrite("  Result " + __result.ToString()+"\n");
      return false;
    }
  }
  /*
  [HarmonyPatch(typeof(WeaponDef))]
  [HarmonyPatch("AmmoCategoryToAmmoId")]
  [HarmonyPatch(MethodType.Getter)]
  public static class WeaponDef_AmmoCategoryToAmmoId_get
  {
      public static bool Prefix(WeaponDef __instance, ref string __result)
      {
          if (__instance.AmmoCategory == AmmoCategory.NotSet)
          {
              __result = string.Empty;
              return false;
          }
          else
          {
              CustomAmmoCategory cuCat = CustomAmmoCategories.findWeaponRealCategory(__instance.Description.Id, __instance.AmmoCategory.ToString());
              if (cuCat.BaseCategory == AmmoCategory.NotSet)
              {
                  __result = string.Empty;
                  return false;
              }
              else
              {
                  __result = "Ammunition_" + cuCat.Id;
                  return false;
              }
          }
      }
  }
  [HarmonyPatch(typeof(WeaponDef))]
  [HarmonyPatch("AmmoCategoryToAmmoBoxId")]
  [HarmonyPatch(MethodType.Getter)]
  public static class WeaponDef_AmmoCategoryToAmmoBoxId_get
  {
      public static bool Prefix(WeaponDef __instance, ref string __result)
      {
          if (__instance.AmmoCategory == AmmoCategory.NotSet)
          {
              __result = string.Empty;
              return false;
          }
          else
          {
              CustomAmmoCategory cuCat = CustomAmmoCategories.findWeaponRealCategory(__instance.Description.Id, __instance.AmmoCategory.ToString());
              if (cuCat.BaseCategory == AmmoCategory.NotSet)
              {
                  __result = string.Empty;
                  return false;
              }
              else
              {
                  __result = "Ammo_AmmunitionBox_Generic_" + cuCat.Id;
                  return false;
              }
          }
      }
  }*/

  [HarmonyPatch(typeof(MechValidationRules))]
  [HarmonyPatch("ValidateMechHasAppropriateAmmo")]
  [HarmonyPatch(MethodType.Normal)]
  public static class MechValidationRules_ValidateMechHasAppropriateAmmo {
    public static bool Prefix(DataManager dataManager, MechDef mechDef, MechValidationLevel validationLevel, WorkOrderEntry_MechLab baseWorkOrder, ref Dictionary<MechValidationType, List<Text>> errorMessages) {
      List<int> ammoCategoryList1 = new List<int>();
      List<int> ammoCategoryList2 = new List<int>();
      CustomAmmoCategoriesLog.Log.LogWrite("Start Mech Validation\n");
      for (int index = 0; index < mechDef.Inventory.Length; ++index) {
        MechComponentRef mechComponentRef = mechDef.Inventory[index];
        mechComponentRef.RefreshComponentDef();
        if (mechComponentRef.ComponentDefType == ComponentType.Weapon) {
          if (mechComponentRef.DamageLevel == ComponentDamageLevel.Functional || mechComponentRef.DamageLevel == ComponentDamageLevel.NonFunctional || MechValidationRules.MechComponentUnderMaintenance(mechComponentRef, validationLevel, baseWorkOrder)) {
            WeaponDef def = mechComponentRef.Def as WeaponDef;
            CustomAmmoCategory cuCat = CustomAmmoCategories.getExtWeaponDef(def.Description.Id).AmmoCategory;
            int weaponAmmoType = cuCat.Index;
            CustomAmmoCategoriesLog.Log.LogWrite(" You have weapon " + def.Description.Id + " with ammo category:" + cuCat.Id + " index:" + weaponAmmoType.ToString() + "\n");

            if (def.AmmoCategory != AmmoCategory.NotSet && (cuCat.Id != AmmoCategory.Flamer.ToString()) && !ammoCategoryList1.Contains(weaponAmmoType)) {
              ammoCategoryList1.Add(weaponAmmoType);
              CustomAmmoCategoriesLog.Log.LogWrite("  Add index to requred ammo category: " + weaponAmmoType.ToString() + "\n");
            }
          }
        } else if (mechComponentRef.ComponentDefType == ComponentType.AmmunitionBox && (mechComponentRef.DamageLevel == ComponentDamageLevel.Functional || MechValidationRules.MechComponentUnderMaintenance(mechComponentRef, validationLevel, baseWorkOrder))) {
          AmmunitionBoxDef def = mechComponentRef.Def as AmmunitionBoxDef;
          def.refreshAmmo(dataManager);
          CustomAmmoCategory cuCat = CustomAmmoCategories.getExtWeaponDef(def.Description.Id).AmmoCategory;
          //CustomAmmoCategories.findWeaponRealCategory(def.Ammo.Description.Id, def.Ammo.Category.ToString());
          int amunitionAmmoType = CustomAmmoCategories.findExtAmmo(def.Ammo.Description.Id).AmmoCategory.Index;
          CustomAmmoCategoriesLog.Log.LogWrite("  You have ammunition " + def.Ammo.Description.Id + " with ammo category " + cuCat.Id + " index:" + amunitionAmmoType.ToString() + "\n");
          if (!ammoCategoryList2.Contains(amunitionAmmoType)) {
            ammoCategoryList2.Add(amunitionAmmoType);
            CustomAmmoCategoriesLog.Log.LogWrite("  Add index to requred ammo category: " + amunitionAmmoType.ToString() + "\n");
          }
        }
      }
      CustomAmmoCategoriesLog.Log.LogWrite("Ammunition Box categories indexes:");
      foreach (int ammoCategory in ammoCategoryList1) {
        CustomAmmoCategoriesLog.Log.LogWrite(" " + ammoCategory.ToString());
      }
      CustomAmmoCategoriesLog.Log.LogWrite("\nWeapon categories indexes:");
      foreach (int ammoCategory in ammoCategoryList2) {
        CustomAmmoCategoriesLog.Log.LogWrite(" " + ammoCategory.ToString());
      }
      CustomAmmoCategoriesLog.Log.LogWrite("\n");
      foreach (int ammoCategory in ammoCategoryList1) {
        if (!ammoCategoryList2.Contains(ammoCategory)) {
          var method = typeof(MechValidationRules).GetMethod("AddErrorMessage", BindingFlags.Static | BindingFlags.NonPublic);
          object[] args = new object[3];
          args[0] = errorMessages;
          args[1] = MechValidationType.AmmoMissing;
          CustomAmmoCategory cuCat = CustomAmmoCategories.findByIndex(ammoCategory);
          args[2] = new Text("MISSING AMMO: This 'Mech does not have an undamaged {0} Ammo Bin ", new object[1] { (object)cuCat.Id });
          method.Invoke(obj: null, parameters: args);
          errorMessages = (Dictionary<MechValidationType, List<Text>>)args[0];
        }
      }
      foreach (int ammoCategory in ammoCategoryList2) {
        if (!ammoCategoryList1.Contains(ammoCategory)) {

          var method = typeof(MechValidationRules).GetMethod("AddErrorMessage", BindingFlags.Static | BindingFlags.NonPublic);
          object[] args = new object[3];
          args[0] = errorMessages;
          args[1] = MechValidationType.AmmoUnneeded;
          CustomAmmoCategory cuCat = CustomAmmoCategories.findByIndex(ammoCategory);
          args[2] = new Text("EXTRA AMMO: This 'Mech carries unusable {0} Ammo", new object[1] { (object)cuCat.Id });
          method.Invoke(obj: null, parameters: args);
          errorMessages = (Dictionary<MechValidationType, List<Text>>)args[0];
        }
      }
      return false;
    }
  }
  [HarmonyPatch(typeof(WeaponRepresentation))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Weapon), typeof(Transform), typeof(bool), typeof(string), typeof(int) })]
  public static class WeaponRepresentation_Init {
    public static void Postfix(WeaponRepresentation __instance, Weapon weapon, Transform parentTransform, bool isParented, string parentDisplayName, int mountedLocation) {
      string wGUID;
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.GUIDStatisticName) == false) {
        wGUID = Guid.NewGuid().ToString();
        weapon.StatCollection.AddStatistic<string>(CustomAmmoCategories.GUIDStatisticName, wGUID);
      } else {
        wGUID = weapon.StatCollection.GetStatistic(CustomAmmoCategories.GUIDStatisticName).Value<string>();
      }
      CustomAmmoCategories.ClearWeaponEffects(wGUID);
      CustomAmmoCategories.InitWeaponEffects(__instance, weapon);
    }
  }
  [HarmonyPatch(typeof(WeaponRepresentation))]
  [HarmonyPatch("PlayWeaponEffect")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(WeaponHitInfo) })]
  public static class WeaponRepresentation_PlayWeaponEffect {
    public static bool Prefix(WeaponRepresentation __instance, WeaponHitInfo hitInfo) {
      CustomAmmoCategoriesLog.Log.LogWrite("WeaponRepresentation.PlayWeaponEffect\n");
      try {
        if (__instance.weapon == null) { return true; }
        CustomAmmoCategoriesLog.Log.LogWrite("  weapon is set\n");
        if (CustomAmmoCategories.checkExistance(__instance.weapon.StatCollection, CustomAmmoCategories.GUIDStatisticName) == false) { return true; }
        CustomAmmoCategoriesLog.Log.LogWrite("  weapon GUID is set\n");
        if (CustomAmmoCategories.checkExistance(__instance.weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == false) { return true; }
        CustomAmmoCategoriesLog.Log.LogWrite("  weapon ammoId is set\n");
        string wGUID = __instance.weapon.StatCollection.GetStatistic(CustomAmmoCategories.GUIDStatisticName).Value<string>();
        string ammoId = __instance.weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        WeaponMode weaponMode = CustomAmmoCategories.getWeaponMode(__instance.weapon);
        string weaponEffectId = weaponMode.WeaponEffectID;
        WeaponEffect currentEffect = (WeaponEffect)null;
        if (string.IsNullOrEmpty(weaponEffectId)) {
          currentEffect = __instance.WeaponEffect;
          weaponEffectId = CustomAmmoCategories.findExtAmmo(ammoId).WeaponEffectID;
          if (string.IsNullOrEmpty(weaponEffectId)) {
            weaponEffectId = __instance.weapon.weaponDef.WeaponEffectID;
          }
        }
        if (weaponEffectId == __instance.weapon.weaponDef.WeaponEffectID) {
          currentEffect = __instance.WeaponEffect;
          weaponEffectId = __instance.weapon.weaponDef.WeaponEffectID;
        };
        if (currentEffect == (WeaponEffect)null) {
          currentEffect = CustomAmmoCategories.getWeaponEffect(wGUID, weaponEffectId);
        }
        CustomAmmoCategoriesLog.Log.LogWrite("  weapon weaponEffectId is set " + wGUID + " " + __instance.weapon.Name + " ammo:" + ammoId + " mode:" + weaponMode.UIName + " " + weaponEffectId + "\n");
        if (currentEffect == null) { return true; }
        CustomAmmoCategoriesLog.Log.LogWrite("  weapon weaponEffect is set\n");
        ExtWeaponDef extWeaponDef = CustomAmmoCategories.getExtWeaponDef(__instance.weapon.Description.Id);
        if (extWeaponDef.StreakEffect == true) {
          CustomAmmoCategoriesLog.Log.LogWrite("  streak effect\n");
          WeaponHitInfo streakHitInfo = CustomAmmoCategories.getSuccessOnly(hitInfo);
          CustomAmmoCategories.ReturnNoFireHeat(__instance.weapon, hitInfo.stackItemUID, streakHitInfo.numberOfShots);
          if (streakHitInfo.numberOfShots == 0) {
            CustomAmmoCategoriesLog.Log.LogWrite("  no success hits\n");
            currentEffect.currentState = WeaponEffect.WeaponEffectState.Complete;
            currentEffect.subEffect = false;
            currentEffect.hitInfo = hitInfo;
            PropertyInfo property = typeof(WeaponEffect).GetProperty("FiringComplete");
            property.DeclaringType.GetProperty("FiringComplete");
            property.GetSetMethod(true).Invoke(currentEffect, new object[1] { (object)false });
            typeof(WeaponEffect).GetMethod("PublishNextWeaponMessage", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(currentEffect, new object[0]);
            currentEffect.PublishWeaponCompleteMessage();
          } else {
            CustomAmmoCategoriesLog.Log.LogWrite("  streak fire\n");
            currentEffect.Fire(streakHitInfo, 0, 0);
            CustomAmmoCategories.DecrementAmmo(__instance.weapon, streakHitInfo.stackItemUID, streakHitInfo.numberOfShots);
          }
        } else {
          CustomAmmoCategoriesLog.Log.LogWrite("  normal fire\n");
          currentEffect.Fire(hitInfo, 0, 0);
        }
        CustomAmmoCategoriesLog.Log.LogWrite("  fired\n");
        return false;
      } catch (Exception e) {
        CustomAmmoCategoriesLog.Log.LogWrite("Exception:" + e.ToString() + "\nfallbak to default\n");
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(WeaponRepresentation))]
  [HarmonyPatch("ResetWeaponEffect")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class WeaponRepresentation_ResetWeaponEffect {
    public static void Postfix(WeaponRepresentation __instance) {
      CustomAmmoCategoriesLog.Log.LogWrite("WeaponRepresentation.ResetWeaponEffect\n");
      if (__instance.weapon == null) { return; }
      CustomAmmoCategoriesLog.Log.LogWrite("  weapon is set\n");
      if (CustomAmmoCategories.checkExistance(__instance.weapon.StatCollection, CustomAmmoCategories.GUIDStatisticName) == false) { return; }
      CustomAmmoCategoriesLog.Log.LogWrite("  weapon GUID is set\n");
      if (CustomAmmoCategories.checkExistance(__instance.weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == false) { return; }
      CustomAmmoCategoriesLog.Log.LogWrite("  weapon ammoId is set\n");
      string wGUID = __instance.weapon.StatCollection.GetStatistic(CustomAmmoCategories.GUIDStatisticName).Value<string>();
      CustomAmmoCategories.resetWeaponEffects(wGUID);
      return;
    }
  }
  [HarmonyPatch(typeof(CombatGameState))]
  [HarmonyPatch("ShutdownCombatState")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatGameState_ShutdownCombatState {
    public static void Postfix(CombatGameState __instance) {
      CustomAmmoCategoriesLog.Log.LogWrite("CombatGameState.ShutdownCombatState\n");
      CustomAmmoCategories.clearAllWeaponEffects();
      return;
    }
  }
}

namespace CustAmmoCategories {
  public class CustomAmmoCategory {
    public string Id { get; set; }
    public int Index { get; set; }
    public BattleTech.AmmoCategory BaseCategory { get; set; }
    public override bool Equals(object o) {
      if(o is CustomAmmoCategory) {
        return this.Index == (o as CustomAmmoCategory).Index;
      }
      return false;
    }
    public override int GetHashCode() {
      return this.Index;
    }
    public static bool operator ==(CustomAmmoCategory a, CustomAmmoCategory b) {
      return a.Index == b.Index;
    }
    public static bool operator !=(CustomAmmoCategory a, CustomAmmoCategory b) {
      return a.Index != b.Index;
    }
    public CustomAmmoCategory() {
      Index = 0;
      BaseCategory = AmmoCategory.NotSet;
      Id = "NotSet";
    }
  }
  public enum HitGeneratorType {
    NotSet,
    Individual,
    Cluster,
    Streak
  }

  public enum TripleBoolean {
    NotSet,
    True,
    False
  }

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
    public float DistantVariance { get; set; }
    public TripleBoolean DistantVarianceReversed { get; set; }
    public float DamageVariance { get; set; }
    public CustomAmmoCategory AmmoCategory { get; set; }
    public ExtAmmunitionDef() {
      AccuracyModifier = 0;
      DirectFireModifier = 0;
      DamagePerShot = 0;
      HeatDamagePerShot = 0;
      ProjectilesPerShot = 0;
      ShotsWhenFired = 0;
      CriticalChanceMultiplier = 0;
      MinRange = 0;
      MaxRange = 0;
      LongRange = 0;
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
      IndirectFireCapable = TripleBoolean.NotSet;
      AOECapable = TripleBoolean.NotSet;
      WeaponEffectID = "";
      HitGenerator = HitGeneratorType.NotSet;
      statusEffects = new EffectData[0] { };
      AmmoCategory = new CustomAmmoCategory();
    }
  }
  public class ExtWeaponDef {
    public HitGeneratorType HitGenerator { get; set; }
    public bool StreakEffect { get; set; }
    public float DirectFireModifier { get; set; }
    public string baseModeId { get; set; }
    public float FlatJammingChance { get; set; }
    public TripleBoolean DamageOnJamming { get; set; }
    public Dictionary<string, WeaponMode> Modes { get; set; }
    public CustomAmmoCategory AmmoCategory { get; set; }
    public ExtWeaponDef() {
      StreakEffect = false;
      HitGenerator = HitGeneratorType.NotSet;
      DirectFireModifier = 0;
      FlatJammingChance = 0;
      baseModeId = WeaponMode.NONE_MODE_NAME;
      DamageOnJamming = TripleBoolean.NotSet;
      AmmoCategory = new CustomAmmoCategory();
      Modes = new Dictionary<string, WeaponMode>();
    }
  }
  public class WeaponAmmoInfo {
    public List<string> AvaibleAmmo { get; set; }
    public int CurrentAmmoIndex { get; set; }
    public string CurrentAmmo {
      get {
        return AvaibleAmmo[CurrentAmmoIndex];
      }
      set {
        int newCurrentAmmoIndex = AvaibleAmmo.IndexOf(value);
        if (newCurrentAmmoIndex >= 0) { CurrentAmmoIndex = newCurrentAmmoIndex; };
      }
    }
    public WeaponAmmoInfo(Weapon weapon) {
      AvaibleAmmo = new List<string>();
      int AIBattleValue = CustomAmmoCategories.findExtAmmo(weapon.ammoBoxes[0].ammoDef.Description.Id).AIBattleValue;
      CurrentAmmoIndex = 0;
      for (int t = 0; t < weapon.ammoBoxes.Count; ++t) {
        if (AvaibleAmmo.Contains(weapon.ammoBoxes[t].ammoDef.Description.Id) == false) {
          AvaibleAmmo.Add(weapon.ammoBoxes[t].ammoDef.Description.Id);
          ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(weapon.ammoBoxes[t].ammoDef.Description.Id);
          if (AIBattleValue < extAmmoDef.AIBattleValue) {
            AIBattleValue = extAmmoDef.AIBattleValue;
            CurrentAmmoIndex = t;
          }
        }
      }
    }
  }
  public static partial class CustomAmmoCategories {
    public static string AmmoIdStatName = "CurrentAmmoId";
    public static string GUIDStatisticName = "WeaponGUID";
    public static string StreakStatisticName = "Streak";
    public static string WeaponModeStatisticName = "CAC-WeaponMode";
    private static Dictionary<string, CustomAmmoCategory> items;
    //private static Dictionary<string, CustomAmmoCategory> AmmunitionDef;
    private static Dictionary<string, ExtAmmunitionDef> ExtAmmunitionDef;
    private static Dictionary<string, ExtWeaponDef> ExtWeaponDef;
    //private static Dictionary<string, CustomAmmoCategory> WeaponDef;
    private static Dictionary<string, Dictionary<string, WeaponEffect>> WeaponEffects;
    //private static Dictionary<string, WeaponAmmoInfo> WeaponAmmo;
    private static CustomAmmoCategory NotSetCustomAmmoCategoty;
    private static ExtAmmunitionDef DefaultAmmo;
    private static ExtWeaponDef DefaultWeapon;
    private static WeaponMode DefaultWeaponMode;
    public static Settings Settings;
    public static float getDirectFireModifier(Weapon weapon) {
      float result = CustomAmmoCategories.getExtWeaponDef(weapon.weaponDef.Description.Id).DirectFireModifier;
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName)) {
        string ammoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        result += CustomAmmoCategories.findExtAmmo(ammoId).DirectFireModifier;
      }
      return result;
    }
    public static WeaponHitInfo getSuccessOnly(WeaponHitInfo hitInfo) {
      int successShots = 0;
      for (int index = 0; index < hitInfo.numberOfShots; ++index) {
        if ((hitInfo.hitLocations[index] != 0) && (hitInfo.hitLocations[index] != 65536)) {
          ++successShots;
        }
      }
      WeaponHitInfo result = new WeaponHitInfo();
      result.attackerId = hitInfo.attackerId;
      result.targetId = hitInfo.targetId;
      result.numberOfShots = successShots;
      result.stackItemUID = hitInfo.stackItemUID;
      result.attackSequenceId = hitInfo.attackSequenceId;
      result.attackGroupIndex = hitInfo.attackGroupIndex;
      result.attackWeaponIndex = hitInfo.attackWeaponIndex;
      result.toHitRolls = new float[successShots];
      result.locationRolls = new float[successShots];
      result.dodgeRolls = new float[successShots];
      result.dodgeSuccesses = new bool[successShots];
      result.hitLocations = new int[successShots];
      result.hitPositions = new Vector3[successShots];
      result.hitVariance = new int[successShots];
      result.hitQualities = new AttackImpactQuality[successShots];
      successShots = 0;
      for (int index = 0; index < hitInfo.numberOfShots; ++index) {
        if ((hitInfo.hitLocations[index] != 0) && (hitInfo.hitLocations[index] != 65536)) {
          result.toHitRolls[successShots] = hitInfo.toHitRolls[index];
          result.locationRolls[successShots] = hitInfo.locationRolls[index];
          result.dodgeRolls[successShots] = hitInfo.dodgeRolls[index];
          result.dodgeSuccesses[successShots] = hitInfo.dodgeSuccesses[index];
          result.hitLocations[successShots] = hitInfo.hitLocations[index];
          result.hitPositions[successShots] = hitInfo.hitPositions[index];
          result.hitVariance[successShots] = hitInfo.hitVariance[index];
          result.hitQualities[successShots] = hitInfo.hitQualities[index];
          ++successShots;
        }
      }
      return result;
    }
    public static void registerExtWeaponDef(string defId, ExtWeaponDef def) {
      ExtWeaponDef[defId] = def;
    }
    public static ExtWeaponDef getExtWeaponDef(string defId) {
      if (CustomAmmoCategories.ExtWeaponDef.ContainsKey(defId)) {
        return ExtWeaponDef[defId];
      } else {
        return CustomAmmoCategories.DefaultWeapon;
      }
    }

    public static WeaponMode getWeaponMode(Weapon weapon) {
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == false) {
        return CustomAmmoCategories.DefaultWeaponMode;
      }
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      if (extWeapon.Modes.Count == 0) { return CustomAmmoCategories.DefaultWeaponMode; };
      string ModeId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
      if (extWeapon.Modes.ContainsKey(ModeId) == false) { return CustomAmmoCategories.DefaultWeaponMode; };
      return extWeapon.Modes[ModeId];
    }

    public static EffectData[] getWeaponStatusEffects(Weapon weapon) {
      CustomAmmoCategoriesLog.Log.LogWrite("getWeaponStatusEffects " + weapon.UIName + "\n");
      List<EffectData> result = new List<EffectData>();
      if(weapon.weaponDef.statusEffects.Length > 0) { result.AddRange(weapon.weaponDef.statusEffects); };
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == false) {
        CustomAmmoCategoriesLog.Log.LogWrite("  weapon has no ammo\n");
      } else {
        string ammoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        ExtAmmunitionDef extAmmo = CustomAmmoCategories.findExtAmmo(ammoId);
        if (extAmmo.statusEffects.Length == 0) {
          CustomAmmoCategoriesLog.Log.LogWrite("  ammo has no additional status effects\n");
        }else {
          CustomAmmoCategoriesLog.Log.LogWrite("  add "+extAmmo.statusEffects.Length+" status effects\n");
          result.AddRange(extAmmo.statusEffects);
        }
      }
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == false) {
        CustomAmmoCategoriesLog.Log.LogWrite("  weapon has no mode\n");
      } else {
        WeaponMode mode = CustomAmmoCategories.getWeaponMode(weapon);
        if(mode.statusEffects.Count == 0) {
          CustomAmmoCategoriesLog.Log.LogWrite("  mode has no additional status effects\n");
        }else {
          CustomAmmoCategoriesLog.Log.LogWrite("  add " + mode.statusEffects.Count + " status effects\n");
          result.AddRange(mode.statusEffects);
        }
      }
      return result.ToArray();
    }
    public static void clearAllWeaponEffects() {
      CustomAmmoCategories.WeaponEffects.Clear();
    }
    public static void resetWeaponEffects(string wGUID) {
      if (CustomAmmoCategories.WeaponEffects.ContainsKey(wGUID) == false) { return; }
      foreach (var weaponEffect in CustomAmmoCategories.WeaponEffects[wGUID]) {
        if (weaponEffect.Value != (UnityEngine.Object)null) {
          weaponEffect.Value.Reset();
        }
      }
    }
    public static WeaponEffect getWeaponEffect(string wGUID, string weaponEffectId) {
      if (CustomAmmoCategories.WeaponEffects.ContainsKey(wGUID) == false) { return null; }
      if (CustomAmmoCategories.WeaponEffects[wGUID].ContainsKey(weaponEffectId) == false) { return null; }
      return CustomAmmoCategories.WeaponEffects[wGUID][weaponEffectId];
    }
    public static void ClearWeaponEffects(string wGUID) {
      if (CustomAmmoCategories.WeaponEffects.ContainsKey(wGUID)) { WeaponEffects.Remove(wGUID); };
    }
    public static void InitWeaponEffects(WeaponRepresentation weaponRepresentation, Weapon weapon) {
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.GUIDStatisticName) == false) { return; }
      string wGUID = weapon.StatCollection.GetStatistic(CustomAmmoCategories.GUIDStatisticName).Value<string>();
      if (CustomAmmoCategories.WeaponEffects.ContainsKey(wGUID) == true) { return; }
      WeaponEffects[wGUID] = new Dictionary<string, WeaponEffect>();
      List<string> avaibleAmmo = CustomAmmoCategories.getAvaibleAmmo(weapon);
      foreach (string ammoId in avaibleAmmo) {
        ExtAmmunitionDef extAmmo = CustomAmmoCategories.findExtAmmo(ammoId);
        if (string.IsNullOrEmpty(extAmmo.WeaponEffectID)) { continue; }
        if (extAmmo.WeaponEffectID == weapon.weaponDef.WeaponEffectID) { continue; }
        if (WeaponEffects[wGUID].ContainsKey(extAmmo.WeaponEffectID) == true) { continue; }
        WeaponEffects[wGUID][extAmmo.WeaponEffectID] = CustomAmmoCategories.InitWeaponEffect(weaponRepresentation, weapon, extAmmo.WeaponEffectID);
      }
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      foreach(var mode in extWeapon.Modes) {
        if (string.IsNullOrEmpty(mode.Value.WeaponEffectID)) { continue; }
        if (mode.Value.WeaponEffectID == weapon.weaponDef.WeaponEffectID) { continue; }
        if (WeaponEffects[wGUID].ContainsKey(mode.Value.WeaponEffectID) == true) { continue; }
        WeaponEffects[wGUID][mode.Value.WeaponEffectID] = CustomAmmoCategories.InitWeaponEffect(weaponRepresentation, weapon, mode.Value.WeaponEffectID);
      }
    }
    public static WeaponEffect InitWeaponEffect(WeaponRepresentation weaponRepresentation, Weapon weapon, string weaponEffectId) {
      GameObject gameObject = (GameObject)null;
      WeaponEffect result = (WeaponEffect)null;
      if (!string.IsNullOrEmpty(weaponEffectId)) {
        gameObject = weaponRepresentation.parentCombatant.Combat.DataManager.PooledInstantiate(weaponEffectId, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
      }
      if ((UnityEngine.Object)gameObject == (UnityEngine.Object)null) {
        CustomAmmoCategoriesLog.Log.LogWrite(string.Format("Error instantiating WeaponEffect [{0}], Weapon [{1}]]\n", (object)weaponEffectId, (object)weapon.Name));
      } else {
        gameObject.transform.parent = weaponRepresentation.transform;
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.rotation = Quaternion.identity;
        result = gameObject.GetComponent<WeaponEffect>();
        if ((UnityEngine.Object)result == (UnityEngine.Object)null) {
          CustomAmmoCategoriesLog.Log.LogWrite(string.Format("Error finding WeaponEffect on GO [{0}], Weapon [{1}]\n", (object)weaponEffectId, (object)weapon.Name));
        } else {
          result.Init(weapon);
        }
      }
      CustomAmmoCategoriesLog.Log.LogWrite("Success init weapon effect " + weaponEffectId + " for " + weapon.Name + "\n");
      return result;
    }

    public static AmmunitionBox getAmmunitionBox(Weapon weapon, int aGUID) {
      if ((aGUID >= 0) && (aGUID < weapon.ammoBoxes.Count)) {
        return weapon.ammoBoxes[aGUID];
      }
      return null;
    }
    public static void RegisterExtAmmoDef(string defId, ExtAmmunitionDef extAmmoDef) {
      CustomAmmoCategoriesLog.Log.LogWrite("Registring extAmmoDef " + defId + " D/H/A " + extAmmoDef.DamagePerShot + "/" + extAmmoDef.HeatDamagePerShot + "/" + extAmmoDef.AccuracyModifier + "\n");
      ExtAmmunitionDef[defId] = extAmmoDef;
    }
    public static ExtAmmunitionDef findExtAmmo(string ammoDefId) {
      if (CustomAmmoCategories.ExtAmmunitionDef.ContainsKey(ammoDefId)) {
        return CustomAmmoCategories.ExtAmmunitionDef[ammoDefId];
      }
      return CustomAmmoCategories.DefaultAmmo;
    }
    public static void CycleAmmoBest(Weapon weapon) {
      if (weapon.ammoBoxes.Count == 0) {
        return;
      };
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == false) {
        weapon.StatCollection.AddStatistic<string>(CustomAmmoCategories.AmmoIdStatName, CustomAmmoCategories.findBestAmmo(weapon));
      } else {
        weapon.StatCollection.Set<string>(CustomAmmoCategories.AmmoIdStatName, CustomAmmoCategories.findBestAmmo(weapon));
      }
    }
    public static List<string> getAvaibleAmmo(Weapon weapon) {
      List<string> result = new List<string>();
      for (int index = 0; index < weapon.ammoBoxes.Count; ++index) {
        if (result.IndexOf(weapon.ammoBoxes[index].ammoDef.Description.Id) < 0) {
          result.Add(weapon.ammoBoxes[index].ammoDef.Description.Id);
        }
      }
      return result;
    }
    public static void CycleAmmo(Weapon weapon) {
      CustomAmmoCategoriesLog.Log.LogWrite("Cycle Ammo\n");
      if (weapon.ammoBoxes.Count == 0) {
        CustomAmmoCategoriesLog.Log.LogWrite(" no ammo boxes\n");
        return;
      };
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == false) {
        CustomAmmoCategoriesLog.Log.LogWrite(" Current weapon is not set\n");
        weapon.StatCollection.AddStatistic<string>(CustomAmmoCategories.AmmoIdStatName, CustomAmmoCategories.findBestAmmo(weapon));
        return;
      }
      string CurrentAmmo = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
      List<string> AvaibleAmmo = CustomAmmoCategories.getAvaibleAmmo(weapon);
      int CurrentAmmoIndex = AvaibleAmmo.IndexOf(CurrentAmmo);
      if (CurrentAmmoIndex < 0) {
        weapon.StatCollection.Set<string>(CustomAmmoCategories.AmmoIdStatName, CustomAmmoCategories.findBestAmmo(weapon));
        return;
      }
      for (int ti = 1; ti <= AvaibleAmmo.Count; ++ti) {
        string tempAmmo = AvaibleAmmo[(ti + CurrentAmmoIndex) % AvaibleAmmo.Count];
        for (int t = 0; t < weapon.ammoBoxes.Count; ++t) {
          int cur_index = t;
          //CustomAmmoCategoriesLog.Log.LogWrite(" test ammoBox index " + cur_index.ToString() + "\n");
          if (weapon.ammoBoxes[cur_index].IsFunctional == false) {
            //CustomAmmoCategoriesLog.Log.LogWrite("   broken\n");
            continue;
          }
          if (weapon.ammoBoxes[cur_index].CurrentAmmo <= 0) {
            //CustomAmmoCategoriesLog.Log.LogWrite("   depleeded\n");
            continue;
          }
          if (weapon.ammoBoxes[cur_index].ammoDef.Description.Id != tempAmmo) {
            //CustomAmmoCategoriesLog.Log.LogWrite("   different ammo\n");
            continue;
          }
          CustomAmmoCategoriesLog.Log.LogWrite("   cycled to " + tempAmmo + "\n");
          weapon.StatCollection.Set<string>(CustomAmmoCategories.AmmoIdStatName, tempAmmo);
          return;
        }
      }
    }

    public static void CycleMode(Weapon weapon) {
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      if(extWeapon.Modes.Count <= 1) {
        CustomAmmoCategoriesLog.Log.LogWrite(" no weapon modes\n");
        return;
      }
      if(CustomAmmoCategories.checkExistance(weapon.StatCollection,CustomAmmoCategories.WeaponModeStatisticName) == false) {
        weapon.StatCollection.AddStatistic<string>(CustomAmmoCategories.WeaponModeStatisticName, extWeapon.baseModeId);
        return;
      }
      string modeId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
      int nextIndex = 0;
      for(int t = 0; t < extWeapon.Modes.Count; ++t) {
        if(extWeapon.Modes.ElementAt(t).Key == modeId) {
          nextIndex = (t + 1) % extWeapon.Modes.Count;
          break;
        }
      }
      modeId = extWeapon.Modes.ElementAt(nextIndex).Key;
      weapon.StatCollection.Set<string>(CustomAmmoCategories.WeaponModeStatisticName, modeId);
    }

    public static void RegisterPlayerWeapon(Weapon weapon) {
      CustomAmmoCategoriesLog.Log.LogWrite("RegisterPlayerWeapon\n");
      if (weapon == null) {
        CustomAmmoCategoriesLog.Log.LogWrite("  Weapon is NULL WTF?!\n");
        return;
      }
      if (weapon.weaponDef == null) {
        CustomAmmoCategoriesLog.Log.LogWrite("  WeaponDef is NULL\n");
        return;
      }
      if (weapon.parent == null) {
        CustomAmmoCategoriesLog.Log.LogWrite("  Parent is NULL\n");
        return;
      }
      if (weapon.AmmoCategory != AmmoCategory.NotSet) {
        CustomAmmoCategoriesLog.Log.LogWrite(" AmmoCategory is set " + weapon.AmmoCategory.ToString() + "\n");
        if (weapon.ammoBoxes.Count > 0) {
          if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == false) {
            weapon.StatCollection.AddStatistic<string>(CustomAmmoCategories.AmmoIdStatName, CustomAmmoCategories.findBestAmmo(weapon));
            CustomAmmoCategoriesLog.Log.LogWrite(" Add to weapon stat collection " + weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>() + "\n");
          }
        }
      }
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      if (extWeapon.Modes.Count > 0) {
        CustomAmmoCategoriesLog.Log.LogWrite(" Weapon modes count " + extWeapon.Modes.Count + "\n");
        if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == false) {
          weapon.StatCollection.AddStatistic<string>(CustomAmmoCategories.WeaponModeStatisticName, extWeapon.baseModeId);
          CustomAmmoCategoriesLog.Log.LogWrite(" Add to weapon stat collection " + extWeapon.baseModeId + "\n");
        }
      }
    }
    public static bool checkExistance(StatCollection statCollection, string statName) {
      return ((Dictionary<string, Statistic>)typeof(StatCollection).GetField("stats", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(statCollection)).ContainsKey(statName);
    }
    public static CustomAmmoCategory findByIndex(int index) {
      foreach (var item in CustomAmmoCategories.items) {
        if (item.Value.Index == index) {
          return item.Value;
        }
      }
      return NotSetCustomAmmoCategoty;
    }
    /*public static CustomAmmoCategory findWeaponRealCategory(string id, string def) {
      if (CustomAmmoCategories.WeaponDef.ContainsKey(id)) {
        return CustomAmmoCategories.WeaponDef[id];
      } else {
        if (CustomAmmoCategories.items.ContainsKey(def)) {
          return CustomAmmoCategories.items[def];
        }
        return CustomAmmoCategories.NotSetCustomAmmoCategoty;
      }
    }*/
    /*public static CustomAmmoCategory findAmunitionRealCategory(string id, string def) {
      if (CustomAmmoCategories.AmmunitionDef.ContainsKey(id)) {
        return CustomAmmoCategories.AmmunitionDef[id];
      } else {
        if (CustomAmmoCategories.items.ContainsKey(def)) {
          return CustomAmmoCategories.items[def];
        }
        return CustomAmmoCategories.NotSetCustomAmmoCategoty;
      }
    }*/
    /*public static void RegisterAmmunition(string id, CustomAmmoCategory custCat) {
      CustomAmmoCategoriesLog.Log.LogWrite("RegisterAmmunition CustomAmmoCategory(" + custCat.Id + "/" + custCat.BaseCategory.ToString() + ") for " + id + "\n");
      CustomAmmoCategories.AmmunitionDef[id] = custCat;
    }*/
    /*public static void RegisterWeapon(string id, CustomAmmoCategory custCat) {
      CustomAmmoCategoriesLog.Log.LogWrite("RegisterWeapon CustomAmmoCategory(" + custCat.Id + "/" + custCat.BaseCategory.ToString() + ") for " + id + "\n");
      CustomAmmoCategories.WeaponDef[id] = custCat;
    }*/
    public static string findBestAmmo(Weapon weapon) {
      if (weapon.ammoBoxes.Count <= 0) { return ""; };
      string result = "";
      int AIBattleValue = 0;
      for (int index = 0; index < weapon.ammoBoxes.Count; ++index) {
        if (result == weapon.ammoBoxes[index].Description.Id) { continue; }
        if (weapon.ammoBoxes[index].CurrentAmmo <= 0) { continue; }
        if (weapon.ammoBoxes[index].IsFunctional == false) { continue; }
        ExtAmmunitionDef extAmmo = CustomAmmoCategories.findExtAmmo(weapon.ammoBoxes[index].ammoDef.Description.Id);
        if (AIBattleValue < extAmmo.AIBattleValue) {
          AIBattleValue = extAmmo.AIBattleValue;
          result = weapon.ammoBoxes[index].ammoDef.Description.Id;
        }
      }
      if (string.IsNullOrEmpty(result)) {
        result = weapon.ammoBoxes[0].ammoDef.Description.Id;
      }
      return result;
    }
    public static void CustomCategoriesInit() {
      CustomAmmoCategories.items = new Dictionary<string, CustomAmmoCategory>();
      //CustomAmmoCategories.AmmunitionDef = new Dictionary<string, CustomAmmoCategory>();
      //CustomAmmoCategories.WeaponDef = new Dictionary<string, CustomAmmoCategory>();
      CustomAmmoCategories.WeaponEffects = new Dictionary<string, Dictionary<string, WeaponEffect>>();
      //CustomAmmoCategories.WeaponAmmo = new Dictionary<string, WeaponAmmoInfo>();
      CustomAmmoCategories.ExtAmmunitionDef = new Dictionary<string, ExtAmmunitionDef>();
      CustomAmmoCategories.ExtWeaponDef = new Dictionary<string, ExtWeaponDef>();
      CustomAmmoCategories.DefaultAmmo = new ExtAmmunitionDef();
      CustomAmmoCategories.DefaultWeapon = new ExtWeaponDef();
      CustomAmmoCategories.DefaultWeaponMode = new WeaponMode();
      //string assemblyFile = (new System.Uri(Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath;
      string filename = Path.Combine(CustomAmmoCategoriesLog.Log.BaseDirectory, "CustomAmmoCategories.json");
      //filename = Path.Combine(filename, "CustomAmmoCategories.json");
      string json = File.ReadAllText(filename);
      List<CustomAmmoCategory> tmp = JsonConvert.DeserializeObject<List<CustomAmmoCategory>>(json);
      CustomAmmoCategoriesLog.Log.LogWrite("Custom ammo categories:\n");
      foreach (AmmoCategory base_cat in Enum.GetValues(typeof(AmmoCategory))) {
        CustomAmmoCategory itm = new CustomAmmoCategory();
        itm.BaseCategory = base_cat;
        itm.Id = base_cat.ToString();
        itm.Index = (int)base_cat;
        items[itm.Id] = itm;
        if (itm.Index == 0) { NotSetCustomAmmoCategoty = itm; };
      }
      foreach (var itm in tmp) {
        itm.Index = items.Count;
        items[itm.Id] = itm;
      }
      CustomAmmoCategoriesLog.Log.LogWrite("Custom ammo categories:\n");
      foreach (var itm in items) {
        CustomAmmoCategoriesLog.Log.LogWrite("  '" + itm.Key + "'= (" + itm.Value.Index + "/" + itm.Value.Id + "/" + itm.Value.BaseCategory.ToString() + ")\n");
      }
    }
    public static CustomAmmoCategory find(string id) {
      if (CustomAmmoCategories.items.ContainsKey(id)) {
        return CustomAmmoCategories.items[id];
      } else {
        return NotSetCustomAmmoCategoty;
      }
    }
  }
}

namespace CustAmmoCategories {
  public class Settings {
    public bool debugLog { get; set; }
    public bool modHTTPServer { get; set; }
    public string modHTTPListen { get; set; }
    Settings() {
      debugLog = true;
      modHTTPServer = true;
      modHTTPListen = "http://localhost:65080";
    }
  }
}

namespace CustomAmmoCategoriesInit {
  public static partial class Core {
    public static void Init(string directory, string settingsJson) {
      //SavesForm savesForm = new SavesForm();
      CustomAmmoCategoriesLog.Log.BaseDirectory = directory;
      CustomAmmoCategoriesLog.Log.InitLog();
      string settings_filename = Path.Combine(CustomAmmoCategoriesLog.Log.BaseDirectory, "CustomAmmoCategoriesSettings.json");
      //settings_filename = Path.Combine(settings_filename, "CustomAmmoCategoriesSettings.json");
      CustomAmmoCategories.Settings = JsonConvert.DeserializeObject<CustAmmoCategories.Settings>(File.ReadAllText(settings_filename));
      CustomAmmoCategoriesLog.Log.LogWrite("Initing...\n");
      try {
        CustomAmmoCategories.CustomCategoriesInit();
        var harmony = HarmonyInstance.Create("io.mission.modrepuation");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
        Thread thread = new Thread(ThreadWork.DoWork);
        thread.Start();
      } catch (Exception e) {
        CustomAmmoCategoriesLog.Log.LogWrite(e.ToString() + "\n");
      }
    }
  }
}
