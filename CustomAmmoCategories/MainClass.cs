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
    public static void LogWrite(string line,bool isCritical = false) {
      if ((CustomAmmoCategories.Settings.debugLog)||(isCritical)) {
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
      bool trigger_ammo = clickXrel > ((width / 3.0f) * 2.0f);
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
    public static void Postfix(MechLabPanel __instance, AmmunitionBoxDef ammoBoxDef, ref bool __result) {
      if (ammoBoxDef.Ammo.Category == AmmoCategory.Flamer) { //patch for energy weapon ammo
        if (CustomAmmoCategories.findExtAmmo(ammoBoxDef.Ammo.Description.Id).AmmoCategory.Id != "Flamer") {
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
      if (weapon.weaponDef.ComponentTags.Contains("wr-clustered_shots") || (CustomAmmoCategories.getWeaponDisabledClustering(weapon) == false)) {
        return weapon.ShotsWhenFired * weapon.ProjectilesPerShot;
      } else {
        return weapon.ShotsWhenFired;
      }
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



  [HarmonyPatch(typeof(Weapon))]
  [HarmonyPatch("SetAmmoBoxes")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(List<AmmunitionBox>) })]
  public static class Weapon_SetAmmoBoxes {
    public static bool Prefix(Weapon __instance, List<AmmunitionBox> ammoBoxes) {
      CustomAmmoCategoriesLog.Log.LogWrite("Weapon SetAmmoBoxes " + __instance.Description.Id + "\n");
      CustomAmmoCategory weaponAmmoCategory = CustomAmmoCategories.getExtWeaponDef(__instance.defId).AmmoCategory;
      List<AmmunitionBox> ammunitionBoxList = new List<AmmunitionBox>();
      foreach (AmmunitionBox ammoBox in ammoBoxes) {
        if (CustomAmmoCategories.isWeaponCanUseAmmo(__instance.weaponDef, ammoBox.ammoDef)) {
          CustomAmmoCategoriesLog.Log.LogWrite("  Ammunition Box " + ammoBox.ammoDef.Description.Id + "\n");
          ammunitionBoxList.Add(ammoBox);
        }
      }
      __instance.ammoBoxes = ammunitionBoxList;
      //if(__instance.ammoBoxes)
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

  /*[HarmonyPatch(typeof(MechValidationRules))]
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
  }*/
  [HarmonyPatch(typeof(MechValidationRules))]
  [HarmonyPatch("ValidateMechHasAppropriateAmmo")]
  [HarmonyPatch(MethodType.Normal)]
  public static class MechValidationRules_ValidateMechHasAppropriateAmmo {
    public static bool Prefix(DataManager dataManager, MechDef mechDef, MechValidationLevel validationLevel, WorkOrderEntry_MechLab baseWorkOrder, ref Dictionary<MechValidationType, List<Text>> errorMessages) {
      Dictionary<string, WeaponDef> weapons = new Dictionary<string, WeaponDef>();
      Dictionary<string, AmmunitionDef> ammos = new Dictionary<string, AmmunitionDef>();
      CustomAmmoCategoriesLog.Log.LogWrite("Start Mech Validation "+mechDef.Name+"\n");
      for (int index = 0; index < mechDef.Inventory.Length; ++index) {
        MechComponentRef mechComponentRef = mechDef.Inventory[index];
        mechComponentRef.RefreshComponentDef();
        if (mechComponentRef.ComponentDefType == ComponentType.Weapon) {
          if (mechComponentRef.DamageLevel == ComponentDamageLevel.Functional || mechComponentRef.DamageLevel == ComponentDamageLevel.NonFunctional || MechValidationRules.MechComponentUnderMaintenance(mechComponentRef, validationLevel, baseWorkOrder)) {
            WeaponDef def = mechComponentRef.Def as WeaponDef;
            if (weapons.ContainsKey(def.Description.Id) == false) { weapons.Add(def.Description.Id, def); }
          }
        } else
        if (mechComponentRef.ComponentDefType == ComponentType.AmmunitionBox) {
          if ((mechComponentRef.DamageLevel == ComponentDamageLevel.Functional || MechValidationRules.MechComponentUnderMaintenance(mechComponentRef, validationLevel, baseWorkOrder))) {
            AmmunitionBoxDef def = mechComponentRef.Def as AmmunitionBoxDef;
            def.refreshAmmo(dataManager);
            if (ammos.ContainsKey(def.Description.Id) == false) { ammos.Add(def.Description.Id, def.Ammo); }
          }
        }
      }
      foreach (var weaponDef in weapons) {
        bool weaponHasAmmo = false;
        if (CustomAmmoCategories.isWeaponCanShootNoAmmo(weaponDef.Value)) { continue; }
        foreach (var ammoDef in ammos) {
          if (CustomAmmoCategories.isWeaponCanUseAmmo(weaponDef.Value, ammoDef.Value)) {
            weaponHasAmmo = true;
            break;
          }
        }
        if (weaponHasAmmo == false) {
          var method = typeof(MechValidationRules).GetMethod("AddErrorMessage", BindingFlags.Static | BindingFlags.NonPublic);
          object[] args = new object[3];
          args[0] = errorMessages;
          args[1] = MechValidationType.AmmoMissing;
          string name = string.IsNullOrEmpty(weaponDef.Value.Description.UIName) ? weaponDef.Value.Description.Name : weaponDef.Value.Description.UIName;
          args[2] = new Text("MISSING AMMO: This 'Mech does not have an undamaged Ammo Bin for {0}", new object[1] { (object)name });
          method.Invoke(obj: null, parameters: args);
          errorMessages = (Dictionary<MechValidationType, List<Text>>)args[0];
        }
      }
      foreach (var ammoDef in ammos) {
        bool ammoIsUsed = false;
        foreach (var weaponDef in weapons) {
          if (CustomAmmoCategories.isWeaponCanUseAmmo(weaponDef.Value, ammoDef.Value)) {
            ammoIsUsed = true;
            break;
          }
        }
        if (ammoIsUsed == false) {
          var method = typeof(MechValidationRules).GetMethod("AddErrorMessage", BindingFlags.Static | BindingFlags.NonPublic);
          object[] args = new object[3];
          args[0] = errorMessages;
          args[1] = MechValidationType.AmmoUnneeded;
          string name = string.IsNullOrEmpty(ammoDef.Value.Description.UIName) ? ammoDef.Value.Description.Name : ammoDef.Value.Description.UIName;
          args[2] = new Text("EXTRA AMMO: This 'Mech carries unusable {0} Ammo", new object[1] { (object)name });
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
      if (o == null) { return false; };
      if (o is CustomAmmoCategory) {
        return this.Index == (o as CustomAmmoCategory).Index;
      }
      return false;
    }
    public override int GetHashCode() {
      return this.Index;
    }
    public static bool operator ==(CustomAmmoCategory a, CustomAmmoCategory b) {
      if (((object)a == null) && ((object)b == null)) { return true; };
      if ((object)a == null) { return false; };
      if ((object)b == null) { return false; };
      return a.Index == b.Index;
    }
    public static bool operator !=(CustomAmmoCategory a, CustomAmmoCategory b) {
      if (((object)a == null) && ((object)b == null)) { return false; };
      if ((object)a == null) { return true; };
      if ((object)b == null) { return true; };
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

  public static partial class CustomAmmoCategories {
    public static string AmmoIdStatName = "CurrentAmmoId";
    public static string GUIDStatisticName = "WeaponGUID";
    public static string StreakStatisticName = "Streak";
    public static string WeaponModeStatisticName = "CAC-WeaponMode";
    private static Dictionary<string, CustomAmmoCategory> items;
    private static Dictionary<string, ExtAmmunitionDef> ExtAmmunitionDef;
    private static Dictionary<string, ExtWeaponDef> ExtWeaponDef;
    private static Dictionary<string, Dictionary<string, WeaponEffect>> WeaponEffects;
    private static CustomAmmoCategory NotSetCustomAmmoCategoty;
    private static ExtAmmunitionDef DefaultAmmo;
    private static ExtWeaponDef DefaultWeapon;
    private static WeaponMode DefaultWeaponMode;
    public static Settings Settings;
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
      if (weapon.weaponDef.statusEffects.Length > 0) { result.AddRange(weapon.weaponDef.statusEffects); };
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == false) {
        CustomAmmoCategoriesLog.Log.LogWrite("  weapon has no ammo\n");
      } else {
        string ammoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        ExtAmmunitionDef extAmmo = CustomAmmoCategories.findExtAmmo(ammoId);
        if (extAmmo.statusEffects.Length == 0) {
          CustomAmmoCategoriesLog.Log.LogWrite("  ammo has no additional status effects\n");
        } else {
          CustomAmmoCategoriesLog.Log.LogWrite("  add " + extAmmo.statusEffects.Length + " status effects\n");
          result.AddRange(extAmmo.statusEffects);
        }
      }
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == false) {
        CustomAmmoCategoriesLog.Log.LogWrite("  weapon has no mode\n");
      } else {
        WeaponMode mode = CustomAmmoCategories.getWeaponMode(weapon);
        if (mode.statusEffects.Count == 0) {
          CustomAmmoCategoriesLog.Log.LogWrite("  mode has no additional status effects\n");
        } else {
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
      foreach (var mode in extWeapon.Modes) {
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
      if (string.IsNullOrEmpty(ammoDefId)) { return CustomAmmoCategories.DefaultAmmo; };
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
    public static CustomAmmoCategory getAmmoAmmoCategory(AmmunitionDef ammoDef) {
      ExtAmmunitionDef extAmmo = CustomAmmoCategories.findExtAmmo(ammoDef.Description.Id);
      CustomAmmoCategory boxAmmoCategory = extAmmo.AmmoCategory;
      if (boxAmmoCategory.Index == CustomAmmoCategories.NotSetCustomAmmoCategoty.Index) { boxAmmoCategory = CustomAmmoCategories.find(ammoDef.Category.ToString()); }
      return boxAmmoCategory;
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
      CustomAmmoCategory weaponAmmoCategory = CustomAmmoCategories.getWeaponCustomAmmoCategory(weapon);
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
          CustomAmmoCategory boxAmmoCategory = CustomAmmoCategories.getAmmoAmmoCategory(weapon.ammoBoxes[t].ammoDef);
          if (boxAmmoCategory != weaponAmmoCategory) {
            continue;
          }
          CustomAmmoCategoriesLog.Log.LogWrite("   cycled to " + tempAmmo + "\n");
          weapon.StatCollection.Set<string>(CustomAmmoCategories.AmmoIdStatName, tempAmmo);
          return;
        }
      }
    }
    /*public static CustomAmmoCategory getWeaponAmmoCategory(Weapon weapon) {
      CustomAmmoCategoriesLog.Log.LogWrite("getWeaponAmmoCategory("+weapon.defId+")\n");
      CustomAmmoCategory weaponAmmoCategory = CustomAmmoCategories.NotSetCustomAmmoCategoty;
      //WeaponMode mode = CustomAmmoCategories.getWeaponMode(weapon);
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      //if (mode.AmmoCategory != null) { weaponAmmoCategory = mode.AmmoCategory; } else
      if (extWeapon.AmmoCategory.Index != CustomAmmoCategories.NotSetCustomAmmoCategoty.Index) { weaponAmmoCategory = extWeapon.AmmoCategory; };
      if (weaponAmmoCategory.Index == CustomAmmoCategories.NotSetCustomAmmoCategoty.Index) { weaponAmmoCategory = CustomAmmoCategories.find(weapon.AmmoCategory.ToString()); }
      CustomAmmoCategoriesLog.Log.LogWrite(" "+weaponAmmoCategory.Id+"\n");
      return weaponAmmoCategory;
    }*/
    public static void CycleMode(Weapon weapon) {
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      if (extWeapon.Modes.Count <= 1) {
        CustomAmmoCategoriesLog.Log.LogWrite(" no weapon modes\n");
        return;
      }
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == false) {
        weapon.StatCollection.AddStatistic<string>(CustomAmmoCategories.WeaponModeStatisticName, extWeapon.baseModeId);
        CustomAmmoCategories.CycleAmmoBest(weapon);
        return;
      }
      string modeId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
      CustomAmmoCategory oldWeaponAmmoCategory = CustomAmmoCategories.getWeaponCustomAmmoCategory(weapon);
      int nextIndex = 0;
      for (int t = 0; t < extWeapon.Modes.Count; ++t) {
        if (extWeapon.Modes.ElementAt(t).Key == modeId) {
          nextIndex = (t + 1) % extWeapon.Modes.Count;
          break;
        }
      }
      modeId = extWeapon.Modes.ElementAt(nextIndex).Key;
      weapon.StatCollection.Set<string>(CustomAmmoCategories.WeaponModeStatisticName, modeId);
      CustomAmmoCategory newWeaponAmmoCategory = CustomAmmoCategories.getWeaponCustomAmmoCategory(weapon);
      if (oldWeaponAmmoCategory.Index != newWeaponAmmoCategory.Index) {
        CustomAmmoCategories.CycleAmmoBest(weapon);
      }
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
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      if (extWeapon.Modes.Count > 0) {
        CustomAmmoCategoriesLog.Log.LogWrite(" Weapon modes count " + extWeapon.Modes.Count + "\n");
        if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == false) {
          weapon.StatCollection.AddStatistic<string>(CustomAmmoCategories.WeaponModeStatisticName, extWeapon.baseModeId);
          CustomAmmoCategoriesLog.Log.LogWrite(" Add to weapon stat collection " + extWeapon.baseModeId + "\n");
        }
      }
      if (weapon.ammoBoxes.Count > 0) {
        if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == false) {
          weapon.StatCollection.AddStatistic<string>(CustomAmmoCategories.AmmoIdStatName, CustomAmmoCategories.findBestAmmo(weapon));
          CustomAmmoCategoriesLog.Log.LogWrite(" Add to weapon stat collection " + weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>() + "\n");
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
      CustomAmmoCategory weaponAmmoCategory = CustomAmmoCategories.find(weapon.AmmoCategory.ToString());
      WeaponMode mode = CustomAmmoCategories.getWeaponMode(weapon);
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      if (mode.AmmoCategory != null) { weaponAmmoCategory = mode.AmmoCategory; } else
      if (extWeapon.AmmoCategory != CustomAmmoCategories.NotSetCustomAmmoCategoty) { weaponAmmoCategory = extWeapon.AmmoCategory; };
      if (weaponAmmoCategory == CustomAmmoCategories.NotSetCustomAmmoCategoty) { return ""; };
      for (int index = 0; index < weapon.ammoBoxes.Count; ++index) {
        if (result == weapon.ammoBoxes[index].Description.Id) { continue; }
        if (weapon.ammoBoxes[index].CurrentAmmo <= 0) { continue; }
        if (weapon.ammoBoxes[index].IsFunctional == false) { continue; }
        ExtAmmunitionDef extAmmo = CustomAmmoCategories.findExtAmmo(weapon.ammoBoxes[index].ammoDef.Description.Id);
        CustomAmmoCategory boxAmmoCategory = extAmmo.AmmoCategory;
        if (boxAmmoCategory == CustomAmmoCategories.NotSetCustomAmmoCategoty) { boxAmmoCategory = CustomAmmoCategories.find(weapon.ammoBoxes[index].ammoDef.Category.ToString()); }
        if (boxAmmoCategory == CustomAmmoCategories.NotSetCustomAmmoCategoty) {
          CustomAmmoCategoriesLog.Log.LogWrite("WARNING! ammunition box " + weapon.ammoBoxes[index].defId + " has no ammo category\n");
          continue;
        }
        if (weaponAmmoCategory != boxAmmoCategory) { continue; };
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
    public bool forbiddenRangeEnable { get; set; }
    public float ClusterAIMult { get; set; }
    public float PenetrateAIMult { get; set; }
    public bool modHTTPServer { get; set; }
    public string modHTTPListen { get; set; }
    Settings() {
      debugLog = true;
      modHTTPServer = true;
      forbiddenRangeEnable = true;
      ClusterAIMult = 0.2f;
      PenetrateAIMult = 0.4f;
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
