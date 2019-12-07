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
using CustomAmmoCategoriesPathes;
using UIWidgets;
using InControl;
using BattleTech.Rendering;
using CustomAmmoCategoriesPatches;
using CustomAmmoCategoriesLog;

namespace CustomAmmoCategoriesPatches {
  [HarmonyPatch(typeof(CombatHUDActionButton))]
  [HarmonyPatch("ExecuteClick")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatHUDActionButton_ExecuteClick {

    public static bool Prefix(CombatHUDActionButton __instance) { try {
        Log.LogWrite("CombatHUDActionButton.ExecuteClick '" + __instance.GUID + "'/'" + CombatHUD.ButtonID_Move + "' " + (__instance.GUID == CombatHUD.ButtonID_Move) + "\n");
        CombatHUD HUD = (CombatHUD)typeof(CombatHUDActionButton).GetProperty("HUD", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance, null);
        bool modifyers = (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
        if (modifyers) {
          Log.LogWrite(" button GUID:" + __instance.GUID + "\n");
          if (__instance.Ability != null) {
            CustomAmmoCategoriesLog.Log.LogWrite(" button ability:" + __instance.Ability.Def.Description.Id + "\n");
          } else {
            Log.LogWrite(" button ability:null\n");
          }
          SelectionType selectionType = (SelectionType)typeof(CombatHUDActionButton).GetProperty("SelectionType", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance, null);
          Log.LogWrite(" selection type:" + selectionType + "\n");
          if (__instance.GUID == "ID_ATTACKGROUND") {
            SpawnVehicleDialogHelper.Dialog.Render(HUD.Combat);
          }
          return true;
        }
        return true;
      } catch (Exception e) { Log.M.WL(e.ToString()); return true; }}
  }
  [HarmonyPatch(typeof(CombatHUDWeaponSlot))]
  [HarmonyPatch("OnPointerDown")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(PointerEventData) })]
  public static class CombatHUDWeaponSlot_OnPointerDown {
    //private static int test_int = 0;
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
      bool trigger_mode = clickXrel > ((width / 3.0f) * 2.0f);
      bool trigger_ammo = clickXrel > ((width / 3.0f));
      bool modifyers = (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
      CustomAmmoCategoriesLog.Log.LogWrite("  instance.l = " + __instance.transform.position.x + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  instance.t = " + __instance.transform.position.y + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  instance.r = " + (__instance.transform.position.x + width) + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  instance.b = " + (__instance.transform.position.y + height) + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  position.x = " + eventData.position.x + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  position.y = " + eventData.position.y + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  modifyers = " + (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  worldClickPos.x = " + worldClickPos.x + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  worldClickPos.y = " + worldClickPos.y + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  clickXrel = " + clickXrel + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  width = " + width + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  trigger_ammo = " + trigger_ammo + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  trigger_mode = " + trigger_mode + "\n");


      if (modifyers) {
        /*if (__instance.DisplayedWeapon.canBeAMS()) {
          Log.LogWrite("Can be AMS\n");
          Vector3 pos = __instance.DisplayedWeapon.parent.Combat.AllEnemies[0].CurrentPosition;
          Log.LogWrite("Can be AMS firing at " + pos + "\n");
          if (test_int == 0) {
            for (int tt = 0; tt < 10; ++tt) {
              int amsfi = __instance.DisplayedWeapon.AMS().AddHitPosition(pos);
              Log.LogWrite(" index:" + amsfi + "\n");
            }
          }
          Log.LogWrite(" fire: " + test_int + "\n");
          if (test_int < 10) {
            __instance.DisplayedWeapon.AMS().Fire(test_int);
            Log.LogWrite(" fired\n");
            ++test_int;
          }
        } else {*/
          CustomAmmoCategories.EjectAmmo(__instance.DisplayedWeapon, __instance);
        //}
        __instance.RefreshDisplayedWeapon((ICombatant)null);
        return false;
      }
      /*if(trigger_ammo || trigger_mode) {
        ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(__instance.DisplayedWeapon.defId);
        if (extWeapon.IsAMS) {
          CustomAmmoCategories.testFireAMS(__instance.DisplayedWeapon);
          return false;
        }
      }*/
      if (trigger_mode) {
        //if ((CustomAmmoCategories.IsJammed(__instance.DisplayedWeapon) == false)
        //  && (CustomAmmoCategories.isWRJammed(__instance.DisplayedWeapon) == false)
        //  && (CustomAmmoCategories.IsCooldown(__instance.DisplayedWeapon) <= 0)) {
        CustomAmmoCategories.CycleMode(__instance.DisplayedWeapon);
        //}
        __instance.RefreshDisplayedWeapon((ICombatant)null);
        return false;
      } else
      if (trigger_ammo) {
        //if ((CustomAmmoCategories.IsJammed(__instance.DisplayedWeapon) == false)
        //  && (CustomAmmoCategories.isWRJammed(__instance.DisplayedWeapon) == false)
        //  && (CustomAmmoCategories.IsCooldown(__instance.DisplayedWeapon) <= 0)) {
        CustomAmmoCategories.CycleAmmo(__instance.DisplayedWeapon);
        //}
        __instance.RefreshDisplayedWeapon((ICombatant)null);
        return false;
      };
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
      bool modifyers = (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
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
      if (modifyers) {
        return false;
      }
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
      if (ammoBoxDef.Ammo.AmmoCategoryValue.IsFlamer) { //patch for energy weapon ammo
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
          if (weapon.AmmoCategoryValue.Is_NotSet) { continue; }
          if (weapon.ammoBoxes.Count <= 0) { continue; }
          if (weapon.CurrentAmmo > 0) { continue; }
          CustomAmmoCategories.CycleAmmoBest(weapon);
        }
      }
    }
  }
  /*[HarmonyPatch(typeof(CombatHUDWeaponSlot))]
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
  }*/

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
      //AttackSequenceWatchDogHelper.StartWatchDogThread();
      CustomAmmoCategories.ActorsEjectedAmmo.Clear();
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
  [HarmonyPatch(typeof(MechValidationRules))]
  [HarmonyPatch("ValidateMechHasAppropriateAmmo")]
  [HarmonyPatch(MethodType.Normal)]
  public static class MechValidationRules_ValidateMechHasAppropriateAmmo {
    public static bool Prefix(DataManager dataManager, MechDef mechDef, MechValidationLevel validationLevel, WorkOrderEntry_MechLab baseWorkOrder, ref Dictionary<MechValidationType, List<Text>> errorMessages) {
      Dictionary<string, WeaponDef> weapons = new Dictionary<string, WeaponDef>();
      Dictionary<string, AmmunitionDef> ammos = new Dictionary<string, AmmunitionDef>();
      CustomAmmoCategoriesLog.Log.LogWrite("Start Mech Validation " + mechDef.Name + "\n");
      string testString = "";
      if (Strings.Initialized) {
        Strings.GetTranslationFor("CT DESTROYED", out testString);
        CustomAmmoCategoriesLog.Log.LogWrite("Checking ... " + testString + "\n");
        if (string.IsNullOrEmpty(testString) == false) {
          if (testString.Contains((string)$"РЕАКТОР")) {
            throw new Exception("Вы используете несовместимую версию локализации. Если вы хотите использовать Custom Ammo Categories, не используйте Russian translation fix. По вопросам совместимости Custom Ammo Categories вы можете обратиться к автору Russian translation fix");
          }
        }
        CustomAmmoCategoriesLog.Log.LogWrite("Check passed\n");
      }
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
        if (weaponDef.Value.StartingAmmoCapacity > 0) { continue; };
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
          args[2] = new Text("__/CAC.MissingAmmo/__", new object[1] { (object)name });
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
          args[2] = new Text("__/CAC.ExtraAmmo/__", new object[1] { (object)name });
          method.Invoke(obj: null, parameters: args);
          errorMessages = (Dictionary<MechValidationType, List<Text>>)args[0];
        }
      }
      return false;
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
        __instance.weapon.clearImpactVFX();
        WeaponEffect currentEffect = CustomAmmoCategories.getWeaponEffect(__instance.weapon);
        if (currentEffect == null) { return true; }
        ExtWeaponDef extWeaponDef = CustomAmmoCategories.getExtWeaponDef(__instance.weapon.Description.Id);
        if (hitInfo.numberOfShots == 0) {
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
      Statistic stat = __instance.weapon.StatCollection.GetStatistic(CustomAmmoCategories.GUIDStatisticName);
      if (stat == null) { return; }
      CustomAmmoCategoriesLog.Log.LogWrite("  weapon GUID is set\n");
      //if (CustomAmmoCategories.checkExistance(__instance.weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == false) { return; }
      //CustomAmmoCategoriesLog.Log.LogWrite("  weapon ammoId is set\n");
      string wGUID = stat.Value<string>();
      CustomAmmoCategories.resetWeaponEffects(wGUID);
      return;
    }
  }
  [HarmonyPatch(typeof(CombatGameState))]
  [HarmonyPatch("ShutdownCombatState")]
  [HarmonyPatch(MethodType.Normal)]
  public static class CombatGameState_ShutdownCombatState {
    public static void Postfix(CombatGameState __instance) {
      CustomAmmoCategoriesLog.Log.LogWrite("CombatGameState.ShutdownCombatState\n");
      CustomAmmoCategories.clearAllWeaponEffects();
      return;
    }
  }
}

namespace CustAmmoCategories {
  public class CustomVector {
    public float x { get { return fx; } set { set = true; fx = value; } }
    public float y { get { return fy; } set { set = true; fy = value; } }
    public float z { get { return fz; } set { set = true; fz = value; } }
    [JsonIgnore]
    public bool set { get; private set; }
    [JsonIgnore]
    public float fx;
    [JsonIgnore]
    public float fy;
    [JsonIgnore]
    public float fz;
    public CustomVector() {
      fx = 0f; fy = 0f; fz = 0f; set = false;
    }
    public CustomVector(bool one) {
      set = false;
      if (one) {
        fx = 1f; fy = 1f; fz = 1f;
      } else {
        fx = 0f; fy = 0f; fz = 0f;
      }
    }
    public CustomVector(float x, float y, float z) {
      this.x = fx; this.y = y; this.z = z; set = true;
    }
    public CustomVector(CustomVector b) {
      this.fx = b.fx; this.fy = b.fy; this.fz = b.fz; this.set = b.set;
    }
    public override string ToString() {
      return ("set " + this.set + " (" + x.ToString() + "," + y.ToString() + "," + z.ToString() + ")");
    }
    public Vector3 vector { get { return new Vector3(this.x, this.y, this.z); } }
  }
  public class CustomAmmoCategoryRecord {
    public string Id { get; set; }
    public string BaseCategory { get; set; }
  }
  public class CustomAmmoCategory {
    public string Id { get; set; }
    public int Index { get; set; }
    private int BaseCategoryID;
    private static AmmoCategoryValue fallbackCategory = new AmmoCategoryValue();
    public AmmoCategoryValue BaseCategory {
      get {
        AmmoCategoryValue result = AmmoCategoryEnumeration.GetAmmoCategoryByID(BaseCategoryID);
        if (result == null) { return CustomAmmoCategory.fallbackCategory; };
        return result;
      }
      set {
        if (value == null) { BaseCategoryID = 0; return; };
        BaseCategoryID = value.ID;
      }
    }
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
      BaseCategoryID = 0;
      Id = "NotSet";
    }
    public CustomAmmoCategory(CustomAmmoCategoryRecord record) {
      AmmoCategoryValue val = AmmoCategoryEnumeration.GetAmmoCategoryByName(record.BaseCategory);
      if (val == null) {
        BaseCategoryID = 0;
        Id = "NotSet";
      } else { 
        this.BaseCategoryID = val.ID;
        Id = record.Id;
      }
    }
  }
  public enum HitGeneratorType {
    NotSet,
    Individual,
    Cluster,
    AOE,
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
    public static CustomAmmoCategory NotSetCustomAmmoCategoty;
    public static ExtAmmunitionDef DefaultAmmo;
    public static ExtWeaponDef DefaultWeapon;
    public static WeaponMode DefaultWeaponMode;
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
      result.attackDirections = new AttackDirection[successShots];
      result.secondaryHitLocations = new int[successShots];
      result.secondaryTargetIds = new string[successShots];
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
          result.attackDirections[successShots] = hitInfo.attackDirections[index];
          result.secondaryHitLocations[successShots] = 0;
          result.secondaryTargetIds[successShots] = null;
          ++successShots;
        }
      }
      return result;
    }
    public static void registerExtWeaponDef(string defId, ExtWeaponDef def) {
      if (CustomAmmoCategories.ExtWeaponDef.ContainsKey(defId) == false) { 
        CustomAmmoCategories.ExtWeaponDef.Add(defId, def);
      } else {
        CustomAmmoCategories.ExtWeaponDef[defId] = def;
      }
    }
    public static bool isRegistredWeapon(string defId) {
      return CustomAmmoCategories.ExtWeaponDef.ContainsKey(defId);
    }
    public static ExtWeaponDef getExtWeaponDef(string defId) {
      if (CustomAmmoCategories.ExtWeaponDef.ContainsKey(defId)) {
        return ExtWeaponDef[defId];
      } else {
        Log.LogWrite("WARNING!" + defId + " is not registed\n", true);
        return CustomAmmoCategories.DefaultWeapon;
      }
    }

    /*public static WeaponMode getWeaponMode(Weapon weapon) {
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == false) {
        return CustomAmmoCategories.DefaultWeaponMode;
      }
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      if (extWeapon.Modes.Count == 0) { return CustomAmmoCategories.DefaultWeaponMode; };
      string ModeId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
      if (extWeapon.Modes.ContainsKey(ModeId) == false) { return CustomAmmoCategories.DefaultWeaponMode; };
      return extWeapon.Modes[ModeId];
    }*/

    public static EffectData[] StatusEffects(this Weapon weapon) {
      //CustomAmmoCategoriesLog.Log.LogWrite("getWeaponStatusEffects " + weapon.UIName + "\n");
      List<EffectData> result = new List<EffectData>();
      result.AddRange(weapon.weaponDef.statusEffects);
      result.AddRange(weapon.ammo().statusEffects);
      result.AddRange(weapon.mode().statusEffects);
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

    /*public static void testFireAMS(Weapon weapon) {
      CombatGameState combat = (CombatGameState)typeof(MechComponent).GetField("combat", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(weapon);
      List<AbstractActor> enemies = combat.GetAllEnemiesOf(weapon.parent);
      if (enemies.Count > 0) {
        CustomAmmoCategoriesLog.Log.LogWrite("AMS test enemy " + enemies[0].DisplayName + "\n");
        CustomAmmoCategories.FireAMS(weapon, enemies[0].CurrentPosition);
      } else {
        CustomAmmoCategoriesLog.Log.LogWrite("AMS test no enemies\n");
      }
    }*/

    /*public static void FireAMS(Weapon weapon, Vector3 target) {
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      if (extWeapon.IsAMS) {
        CustomAmmoCategoriesLog.Log.LogWrite("AMS found " + weapon.defId + "\n");
        string wGUID = weapon.StatCollection.GetStatistic(CustomAmmoCategories.GUIDStatisticName).Value<string>();
        BallisticEffect ballisticEffect = weapon.weaponRep.WeaponEffect as BallisticEffect;
        LaserEffect LaserEffect = weapon.weaponRep.WeaponEffect as LaserEffect;
        if (ballisticEffect != null) {
          CustomAmmoCategoriesLog.Log.LogWrite("ballistic effect found " + weapon.defId + "\n");
          CustomAmmoCategories.AMSFire(ballisticEffect, target);
        } else
        if (LaserEffect != null) {
          CustomAmmoCategoriesLog.Log.LogWrite("laser effect found " + weapon.defId + "\n");
          CustomAmmoCategories.AMSFire(LaserEffect, target);
        } else {
          CustomAmmoCategoriesLog.Log.LogWrite("ams effect not found " + weapon.defId + " " + wGUID + "\n");
        }
      } else {
        CustomAmmoCategoriesLog.Log.LogWrite("no AMS detected " + weapon.defId + "\n");
      }
    }*/
    public static void InitWeaponEffects(WeaponRepresentation weaponRepresentation, Weapon weapon) {
      Log.LogWrite("InitWeaponEffects "+weapon.defId+":"+weapon.parent.DisplayName+"\n");
      if (weapon.StatCollection.ContainsStatistic(CustomAmmoCategories.GUIDStatisticName) == false) {
        Log.LogWrite(" no GUID\n");
        return;
      }
      string wGUID = weapon.StatCollection.GetStatistic(CustomAmmoCategories.GUIDStatisticName).Value<string>();
      if (CustomAmmoCategories.WeaponEffects.ContainsKey(wGUID) == true) {
        Log.LogWrite(" already contains GUID:"+wGUID+"\n");
        return;
      }
      WeaponEffects[wGUID] = new Dictionary<string, WeaponEffect>();
      List<string> avaibleAmmo = CustomAmmoCategories.getAvaibleAmmo(weapon);
      foreach (string ammoId in avaibleAmmo) {
        ExtAmmunitionDef extAmmo = CustomAmmoCategories.findExtAmmo(ammoId);
        if (string.IsNullOrEmpty(extAmmo.WeaponEffectID)) {
          Log.LogWrite("  "+ammoId+".WeaponEffectID is empty\n");
          continue;
        }
        if (extAmmo.WeaponEffectID == weapon.weaponDef.WeaponEffectID) {
          Log.LogWrite("  " + ammoId + ".WeaponEffectID "+ extAmmo.WeaponEffectID + " same as per weapon def "+ weapon.weaponDef.WeaponEffectID + "\n");
          continue;
        }
        if (WeaponEffects[wGUID].ContainsKey(extAmmo.WeaponEffectID) == true) {
          Log.LogWrite("  " + ammoId + ".WeaponEffectID " + extAmmo.WeaponEffectID + " already inited\n");
          continue;
        }
        Log.LogWrite("  adding predefined weapon effect "+ wGUID + "."+ extAmmo.WeaponEffectID + "\n");
        WeaponEffects[wGUID][extAmmo.WeaponEffectID] = CustomAmmoCategories.InitWeaponEffect(weaponRepresentation, weapon, extAmmo.WeaponEffectID);
      }
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      foreach (var mode in extWeapon.Modes) {
        if (string.IsNullOrEmpty(mode.Value.WeaponEffectID)) {
          Log.LogWrite("  mode:" + mode.Key + ".WeaponEffectID is empty\n");
          continue;
        }
        if (mode.Value.WeaponEffectID == weapon.weaponDef.WeaponEffectID) {
          Log.LogWrite("  mode:" + mode.Key + ".WeaponEffectID " + mode.Value.WeaponEffectID + " same as per weapon def " + weapon.weaponDef.WeaponEffectID + "\n");
          continue;
        }
        if (WeaponEffects[wGUID].ContainsKey(mode.Value.WeaponEffectID) == true) {
          Log.LogWrite("  mode:" + mode.Key + ".WeaponEffectID " + mode.Value.WeaponEffectID + " already inited\n");
          continue;
        }
        Log.LogWrite("  adding predefined weapon effect " + wGUID + "." + mode.Value.WeaponEffectID + "\n");
        WeaponEffects[wGUID][mode.Value.WeaponEffectID] = CustomAmmoCategories.InitWeaponEffect(weaponRepresentation, weapon, mode.Value.WeaponEffectID);
      }
      /*if (string.IsNullOrEmpty(CustomAmmoCategories.ShellsWeaponEffectId) == false) {
        if (WeaponEffects[wGUID].ContainsKey(CustomAmmoCategories.ShellsWeaponEffectId) == false) {
          WeaponEffects[wGUID][CustomAmmoCategories.ShellsWeaponEffectId] = CustomAmmoCategories.InitWeaponEffect(weaponRepresentation, weapon, CustomAmmoCategories.ShellsWeaponEffectId);
        }
      }*/
    }

    public static AmmunitionBox getAmmunitionBox(Weapon weapon, int aGUID) {
      if ((aGUID >= 0) && (aGUID < weapon.ammoBoxes.Count)) {
        return weapon.ammoBoxes[aGUID];
      }
      return null;
    }
    public static void RegisterExtAmmoDef(string defId, ExtAmmunitionDef extAmmoDef) {
      CustomAmmoCategoriesLog.Log.LogWrite("Registring extAmmoDef " + defId + " D/H/A " + extAmmoDef.ProjectilesPerShot + "/" + extAmmoDef.HeatDamagePerShot + "/" + extAmmoDef.AccuracyModifier + "\n");
      if (ExtAmmunitionDef.ContainsKey(defId) == false) {
        ExtAmmunitionDef.Add(defId,extAmmoDef);
      } else {
        Log.M.WL("already registred");
      }
    }
    public static ExtAmmunitionDef findExtAmmo(string ammoDefId) {
      if (string.IsNullOrEmpty(ammoDefId)) { return CustomAmmoCategories.DefaultAmmo; };
      if (CustomAmmoCategories.ExtAmmunitionDef.ContainsKey(ammoDefId)) {
        return CustomAmmoCategories.ExtAmmunitionDef[ammoDefId];
      }
      return CustomAmmoCategories.DefaultAmmo;
    }
    public static void CycleAmmoBest(Weapon weapon) {
      if (weapon.StatCollection.ContainsStatistic(CustomAmmoCategories.AmmoIdStatName) == false) {
        weapon.StatCollection.AddStatistic<string>(CustomAmmoCategories.AmmoIdStatName, CustomAmmoCategories.findBestAmmo(weapon));
      } else {
        weapon.StatCollection.Set<string>(CustomAmmoCategories.AmmoIdStatName, CustomAmmoCategories.findBestAmmo(weapon));
      }
      weapon.ClearAmmoModeCache();
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
      if (boxAmmoCategory.Index == CustomAmmoCategories.NotSetCustomAmmoCategoty.Index) { boxAmmoCategory = CustomAmmoCategories.find(ammoDef.AmmoCategoryValue.Name); }
      return boxAmmoCategory;
    }
    public static void CycleAmmo(Weapon weapon) {
      CustomAmmoCategoriesLog.Log.LogWrite("Cycle Ammo\n");
      if (weapon.ammoBoxes.Count == 0) {
        CustomAmmoCategoriesLog.Log.LogWrite(" no ammo boxes\n");
        return;
      };
      if (weapon.StatCollection.ContainsStatistic(CustomAmmoCategories.AmmoIdStatName) == false) {
        CustomAmmoCategoriesLog.Log.LogWrite(" Current weapon is not set\n");
        weapon.StatCollection.AddStatistic<string>(CustomAmmoCategories.AmmoIdStatName, CustomAmmoCategories.findBestAmmo(weapon));
        weapon.ClearAmmoModeCache();
        return;
      }
      string CurrentAmmo = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
      List<string> AvaibleAmmo = CustomAmmoCategories.getAvaibleAmmo(weapon);
      int CurrentAmmoIndex = AvaibleAmmo.IndexOf(CurrentAmmo);
      if (CurrentAmmoIndex < 0) {
        weapon.StatCollection.Set<string>(CustomAmmoCategories.AmmoIdStatName, CustomAmmoCategories.findBestAmmo(weapon));
        weapon.ClearAmmoModeCache();
        return;
      }
      CustomAmmoCategory weaponAmmoCategory = weapon.CustomAmmoCategory();
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
          weapon.ClearAmmoModeCache();
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
      if (weapon.StatCollection.ContainsStatistic(CustomAmmoCategories.WeaponModeStatisticName) == false) {
        weapon.StatCollection.AddStatistic<string>(CustomAmmoCategories.WeaponModeStatisticName, extWeapon.baseModeId);
        CustomAmmoCategories.CycleAmmoBest(weapon);
        return;
      }
      string modeId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
      CustomAmmoCategory oldWeaponAmmoCategory = weapon.CustomAmmoCategory();
      List<WeaponMode> avaibleModes = weapon.AvaibleModes();
      if (avaibleModes.Count == 0) { return; };
      int nextIndex = 0;
      for (int t = 0; t < avaibleModes.Count; ++t) {
        if (avaibleModes[t].Id == modeId) {
          nextIndex = (t + 1) % avaibleModes.Count;
          break;
        }
      }
      modeId = avaibleModes[nextIndex].Id;
      weapon.StatCollection.Set<string>(CustomAmmoCategories.WeaponModeStatisticName, modeId);
      CustomAmmoCategory newWeaponAmmoCategory = weapon.CustomAmmoCategory();
      if (oldWeaponAmmoCategory.Index != newWeaponAmmoCategory.Index) {
        CustomAmmoCategories.CycleAmmoBest(weapon);
      } else {
        weapon.ClearAmmoModeCache();
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
        if (weapon.StatCollection.ContainsStatistic(CustomAmmoCategories.WeaponModeStatisticName) == false) {
          weapon.StatCollection.AddStatistic<string>(CustomAmmoCategories.WeaponModeStatisticName, extWeapon.baseModeId);
          CustomAmmoCategoriesLog.Log.LogWrite(" Add to weapon stat collection " + extWeapon.baseModeId + "\n");
          weapon.ClearAmmoModeCache();
        }
      }
      if (weapon.ammoBoxes.Count > 0) {
        if (weapon.StatCollection.ContainsStatistic(CustomAmmoCategories.AmmoIdStatName) == false) {
          weapon.StatCollection.AddStatistic<string>(CustomAmmoCategories.AmmoIdStatName, CustomAmmoCategories.findBestAmmo(weapon));
          CustomAmmoCategoriesLog.Log.LogWrite(" Add to weapon stat collection " + weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>() + "\n");
          weapon.ClearAmmoModeCache();
        }
      }
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
      CustomAmmoCategory weaponAmmoCategory = CustomAmmoCategories.find(weapon.AmmoCategoryValue.Name);
      WeaponMode mode = weapon.mode();
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
        if (boxAmmoCategory == CustomAmmoCategories.NotSetCustomAmmoCategoty) { boxAmmoCategory = CustomAmmoCategories.find(weapon.ammoBoxes[index].ammoDef.AmmoCategoryValue.Name); }
        if (boxAmmoCategory == CustomAmmoCategories.NotSetCustomAmmoCategoty) {
          CustomAmmoCategoriesLog.Log.LogWrite("WARNING! ammunition box " + weapon.ammoBoxes[index].defId + " has no ammo category\n", true);
          continue;
        }
        if (weaponAmmoCategory != boxAmmoCategory) { continue; };
        if (AIBattleValue < extAmmo.AIBattleValue) {
          AIBattleValue = extAmmo.AIBattleValue;
          result = weapon.ammoBoxes[index].ammoDef.Description.Id;
        }
      }
      if (string.IsNullOrEmpty(result)) {
        CustomAmmoCategoriesLog.Log.LogWrite("WARNING! no ammo box for category " + weaponAmmoCategory.Id + ". Fallback\n", true);
        foreach (var ammo in CustomAmmoCategories.ExtAmmunitionDef) {
          if (ammo.Value.AmmoCategory.Index == weaponAmmoCategory.Index) {
            result = ammo.Key;
            break;
          }
        }
        CustomAmmoCategoriesLog.Log.LogWrite("Default ammo id is used " + result + "\n");
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
      CustomAmmoCategories.amsWeapons = new Dictionary<string, Weapon>();
      //AmmoCategoryEnumeration.Instance.RefreshStaticData();
      foreach (var baseAmmoCat in AmmoCategoryEnumeration.AmmoCategoryList) {
        CustomAmmoCategory itm = new CustomAmmoCategory();
        itm.BaseCategory = baseAmmoCat;
        itm.Id = baseAmmoCat.Name;
        itm.Index = baseAmmoCat.ID;
        items[itm.Id] = itm;
        if (itm.Index == 0) { NotSetCustomAmmoCategoty = itm; };
      }

      DirectoryInfo di = new DirectoryInfo(CustomAmmoCategoriesLog.Log.BaseDirectory);
      CustomAmmoCategoriesLog.Log.LogWrite("Parent:" + di.Parent.FullName + "\n");
      string[] subdirectoryEntries = Directory.GetDirectories(di.Parent.FullName);
      foreach (string modDir in subdirectoryEntries) {
        string filename = Path.Combine(modDir, "CustomAmmoCategories.json");
        if (File.Exists(filename) == false) { continue; }
        CustomAmmoCategoriesLog.Log.LogWrite(filename + "\n");
        try {
          string json = File.ReadAllText(filename);
          List<CustomAmmoCategoryRecord> tmp = JsonConvert.DeserializeObject<List<CustomAmmoCategoryRecord>>(json);
          CustomAmmoCategoriesLog.Log.LogWrite(" custom ammo categories:\n");
          foreach (var itm in tmp) {
            CustomAmmoCategory cat = new CustomAmmoCategory(itm);
            cat.Index = items.Count;
            items[itm.Id] = cat;
            Log.LogWrite("  '" + cat.Id + "'= (" + cat.Index + "/" + cat.Id + "/" + cat.BaseCategory.Name + ")\n");
          }
        } catch (Exception e) {
          Log.LogWrite(e.ToString() + "\n");
        }
      }
      //string assemblyFile = (new System.Uri(Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath;
      //string filename = Path.Combine(CustomAmmoCategoriesLog.Log.BaseDirectory, "CustomAmmoCategories.json");
      //filename = Path.Combine(filename, "CustomAmmoCategories.json");
      CustomAmmoCategoriesLog.Log.LogWrite("consolidated:\n");
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
  public class BurnedTreesSettings {
    public string Mesh { get; set; }
    public string BumpMap { get; set; }
    public string MainTex { get; set; }
    public string OcculusionMap { get; set; }
    public string Transmission { get; set; }
    public string MetallicGlossMap { get; set; }
    public float BurnedTreeScale { get; set; }
    public float DecalScale { get; set; }
    public string DecalTexture { get; set; }
    public BurnedTreesSettings() {
      Mesh = "envMdlTree_deadWood_polar_frozen_shapeA_LOD0";
      BumpMap = "envTxrTree_treesVaried_polar_frozen_nrm";
      MainTex = "envTxrTree_treesVaried_polar_frozen_alb";
      OcculusionMap = "envTxrTree_treesVaried_polar_frozen_amb";
      Transmission = "envTxrTree_treesVaried_polar_frozen_trs";
      MetallicGlossMap = "envTxrTree_treesVaried_polar_frozen_mtl";
      BurnedTreeScale = 2f;
      DecalScale = 40f;
      DecalTexture = "envTxrDecl_terrainDmgSmallBlack_alb";
    }
  }
  public class AmmoCookoffSettings {
    public bool Enabled { get; set; }
    public float OverheatChance { get; set; }
    public float ShutdownHeatChance { get; set; }
    public bool UseHBSMercySetting { get; set; }
    public AmmoCookoffSettings() {
      Enabled = false;
      OverheatChance = 10;
      ShutdownHeatChance = 25;
      UseHBSMercySetting = true;
    }
  }
  public static class UnitUnaffectionsActorStats {
    public static readonly string DesignMasksActorStat = "CUDesignMasksUnaffected";
    public static readonly string PathingActorStat = "CUPathingUnaffected";
    public static readonly string MoveCostActorStat = "CUMoveCost";
    public static readonly string MoveCostBiomeActorStat = "CUMoveCostBiomeUnaffected";
    public static readonly string FireActorStat = "CUFireActorStatUnaffected";
    public static readonly string LandminesActorStat = "CULandminesUnaffected";
    public static readonly string AOEHeightActorStat = "CUAOEHeight";
    public static bool UnaffectedDesignMasks(this ICombatant unit) {
      if (unit.StatCollection.ContainsStatistic(DesignMasksActorStat) == false) { return false; };
      return unit.StatCollection.GetStatistic(DesignMasksActorStat).Value<bool>();
    }
    public static bool UnaffectedPathing(this ICombatant unit) {
      try {
        if (unit.StatCollection.ContainsStatistic(PathingActorStat) == false) { return false; };
        return unit.StatCollection.GetStatistic(PathingActorStat).Value<bool>();
      } catch (Exception) {
        return false;
      }
    }
    public static bool UnaffectedFire(this ICombatant unit) {
      if (unit.StatCollection.ContainsStatistic(FireActorStat) == false) { return false; };
      return unit.StatCollection.GetStatistic(FireActorStat).Value<bool>();
    }
    public static bool UnaffectedLandmines(this ICombatant unit) {
      if (unit.StatCollection.ContainsStatistic(LandminesActorStat) == false) { return false; };
      return unit.StatCollection.GetStatistic(LandminesActorStat).Value<bool>();
    }
    public static float AoEHeightFix(this ICombatant unit) {
      if (unit.StatCollection.ContainsStatistic(AOEHeightActorStat) == false) { return 0f; };
      return unit.StatCollection.GetStatistic(AOEHeightActorStat).Value<float>();
    }
    public static string CustomMoveCostKey(this ICombatant unit) {
      if (unit.StatCollection.ContainsStatistic(MoveCostActorStat) == false) { return string.Empty; };
      return unit.StatCollection.GetStatistic(MoveCostActorStat).Value<string>();
    }
    public static bool UnaffectedMoveCostBiome(this ICombatant unit) {
      if (unit.StatCollection.ContainsStatistic(MoveCostBiomeActorStat) == false) { return false; };
      return unit.StatCollection.GetStatistic(MoveCostBiomeActorStat).Value<bool>();
    }
  }
  public class Settings {
    public bool debugLog { get; set; }
    public bool forbiddenRangeEnable { get; set; }
    public bool AmmoCanBeExhausted { get; set; }
    public bool Joke { get; set; }
    public float ClusterAIMult { get; set; }
    public float PenetrateAIMult { get; set; }
    public float JamAIAvoid { get; set; }
    public float DamageJamAIAvoid { get; set; }
    public bool modHTTPServer { get; set; }
    public string modHTTPListen { get; set; }
    public string WeaponRealizerStandalone { get; set; }
    public string AIMStandalone { get; set; }
    public List<string> DynamicDesignMasksDefs { get; set; }
    public string BurningTerrainDesignMask { get; set; }
    public string BurningForestDesignMask { get; set; }
    public string BurningFX { get; set; }
    public string BurnedFX { get; set; }
    public float BurningScaleX { get; set; }
    public float BurningScaleY { get; set; }
    public float BurningScaleZ { get; set; }
    public float BurnedScaleX { get; set; }
    public float BurnedScaleY { get; set; }
    public float BurnedScaleZ { get; set; }
    public float BurnedOffsetX { get; set; }
    public float BurnedOffsetY { get; set; }
    public float BurnedOffsetZ { get; set; }
    public float BurningOffsetX { get; set; }
    public float BurningOffsetY { get; set; }
    public float BurningOffsetZ { get; set; }
    public string BurnedForestDesignMask { get; set; }
    public int BurningForestCellRadius { get; set; }
    public int BurningForestTurns { get; set; }
    public int BurningForestStrength { get; set; }
    public float BurningForestBaseExpandChance { get; set; }
    public List<string> AdditinalAssets { get; set; }
    public bool DontShowNotDangerouceJammMessages { get; set; }
    public List<string> NoForestBiomes { get; set; }
    public Dictionary<string, float> ForestBurningDurationBiomeMult { get; set; }
    public Dictionary<string, float> WeaponBurningDurationBiomeMult { get; set; }
    public Dictionary<string, float> ForestBurningStrengthBiomeMult { get; set; }
    public Dictionary<string, float> WeaponBurningStrengthBiomeMult { get; set; }
    public Dictionary<string, float> LitFireChanceBiomeMult { get; set; }
    public Dictionary<string, float> MineFieldPathingMods { get; set; }
    public int JumpLandingMineAttractRadius { get; set; }
    public int AttackSequenceMaxLength { get; set; }
    public BurnedTreesSettings BurnedTrees { get; set; }
    public bool DontShowBurnedTrees { get; set; }
    public bool DontShowBurnedTreesTemporary { get; set; }
    public bool DontShowScorchTerrain { get; set; }
    public float AAMSAICoeff { get; set; }
    public bool AIPeerToPeerNodeEnabled { get; set; }
    public bool AIPeerToPeerFirewallPierceThrough { get; set; }
    public string WeaponRealizerSettings { get; set; }
    public AmmoCookoffSettings AmmoCookoff { get; set; }
    //public bool WaterHeightFix { get; set; }
    public float TerrainFiendlyFireRadius { get; set; }
    public bool AdvancedCirtProcessing { get; set; }
    public bool DestroyedComponentsCritTrap { get; set; }
    public bool CritLocationTransfer { get; set; }
    public float APMinCritChance { get; set; }
    public string RemoveFromCritRollStatName { get; set; }
    public bool SpawnMenuEnabled { get; set; }
    public bool NoCritFloatieMessage { set { FNoCritFloatieMessage = value ? TripleBoolean.True : TripleBoolean.False; } }
    [JsonIgnore]
    public TripleBoolean FNoCritFloatieMessage { get; private set; }
    [JsonIgnore]
    public TripleBoolean MechEngineerDetected { get; set; }
    public void MechEngineerDetect() {
      Log.M.WL(0, "Detecting MechEngineer:");
      if (MechEngineerDetected != TripleBoolean.NotSet) { return; }
      foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
        Log.M.WL(1, assembly.GetName().Name);
        if (assembly.GetName().Name == "MechEngineer") { MechEngineerDetected = TripleBoolean.True; return; }
      }
      MechEngineerDetected = TripleBoolean.False;
    }
    public bool NoCritFloatie() {
      if (FNoCritFloatieMessage != TripleBoolean.NotSet) { return FNoCritFloatieMessage != TripleBoolean.False; }
      MechEngineerDetect();
      return MechEngineerDetected != TripleBoolean.False;
    }
    public HashSet<Strings.Culture> patchWeaponSlotsOverflowCultures { get; private set; }
    public List<Strings.Culture> PatchWeaponSlotsOverflowCultures {
      set {
        foreach (Strings.Culture culture in value) {
          patchWeaponSlotsOverflowCultures.Add(culture);
        }
      }
    }
    public int FiringPreviewRecalcTrottle { get; set; }
    public int SelectionStateMoveBaseProcessMousePosTrottle { get; set; }
    public int UpdateReticleTrottle { get; set; }
    Settings() {
      debugLog = true;
      modHTTPServer = true;
      forbiddenRangeEnable = true;
      Joke = false;
      AmmoCanBeExhausted = true;
      ClusterAIMult = 0.2f;
      PenetrateAIMult = 0.4f;
      JamAIAvoid = 1.0f;
      DamageJamAIAvoid = 2.0f;
      WeaponRealizerStandalone = "";
      modHTTPListen = "http://localhost:65080";
      DynamicDesignMasksDefs = new List<string>();
      BurningForestDesignMask = "DesignMaskBurningForest";
      BurnedForestDesignMask = "DesignMaskBurnedForest";
      BurningTerrainDesignMask = "DesignMaskBurningTerrain";
      BurningForestCellRadius = 3;
      BurningForestTurns = 3;
      BurningForestStrength = 5;
      BurningForestBaseExpandChance = 0.5f;
      BurningFX = "vfxPrfPrtl_fireTerrain_lrgLoop";
      BurnedFX = "vfxPrfPrtl_miningSmokePlume_lrg_loop";
      BurningScaleX = 1f;
      BurningScaleY = 1f;
      BurningScaleZ = 1f;
      BurnedScaleX = 1f;
      BurnedScaleY = 1f;
      BurnedScaleZ = 1f;
      BurnedOffsetX = 0f;
      BurnedOffsetY = 0f;
      BurnedOffsetZ = 0f;
      BurningOffsetX = 0f;
      BurningOffsetY = 0f;
      BurningOffsetZ = 0f;
      AttackSequenceMaxLength = 15000;
      AdditinalAssets = new List<string>();
      DontShowNotDangerouceJammMessages = false;
      NoForestBiomes = new List<string>();
      ForestBurningDurationBiomeMult = new Dictionary<string, float>();
      WeaponBurningDurationBiomeMult = new Dictionary<string, float>();
      ForestBurningStrengthBiomeMult = new Dictionary<string, float>();
      WeaponBurningStrengthBiomeMult = new Dictionary<string, float>();
      LitFireChanceBiomeMult = new Dictionary<string, float>();
      MineFieldPathingMods = new Dictionary<string, float>();
      JumpLandingMineAttractRadius = 2;
      BurnedTrees = new BurnedTreesSettings();
      DontShowBurnedTrees = false;
      DontShowScorchTerrain = false;
      AIPeerToPeerNodeEnabled = false;
      AIPeerToPeerFirewallPierceThrough = false;
      AAMSAICoeff = 0.2f;
      WeaponRealizerSettings = "WeaponRealizerSettings.json";
      WeaponRealizerStandalone = "WeaponRealizer.dll";
      AIMStandalone = "AttackImprovementMod.dll";
      AmmoCookoff = new AmmoCookoffSettings();
      DontShowBurnedTreesTemporary = false;
      //WaterHeightFix = true;
      //TerrainFiendlyFireRadius = 10f;
      AdvancedCirtProcessing = true;
      DestroyedComponentsCritTrap = true;
      CritLocationTransfer = true;
      APMinCritChance = 0.1f;
      RemoveFromCritRollStatName = "IgnoreDamage";
      FNoCritFloatieMessage = TripleBoolean.NotSet;
      MechEngineerDetected = TripleBoolean.NotSet;
      SpawnMenuEnabled = false;
      patchWeaponSlotsOverflowCultures = new HashSet<Strings.Culture>();
      FiringPreviewRecalcTrottle = 500;
      SelectionStateMoveBaseProcessMousePosTrottle = 4;
      UpdateReticleTrottle = 8;
    }
  }
}

namespace CACMain {
  public static class Core {
    public static Dictionary<string, GameObject> AdditinalFXObjects = new Dictionary<string, GameObject>();
    public static Dictionary<string, Mesh> AdditinalMeshes = new Dictionary<string, Mesh>();
    public static Dictionary<string, Texture2D> AdditinalTextures = new Dictionary<string, Texture2D>();
    public static Dictionary<string, Material> AdditinalMaterials = new Dictionary<string, Material>();
    public static Dictionary<string, Shader> AdditinalShaders = new Dictionary<string, Shader>();
    public static Dictionary<string, AudioClip> AdditinalAudio = new Dictionary<string, AudioClip>();
    public static Mesh findMech(string name) {
      if (Core.AdditinalMeshes.ContainsKey(name)) { return Core.AdditinalMeshes[name]; };
      return null;
    }
    public static Texture2D findTexture(string name) {
      if (Core.AdditinalTextures.ContainsKey(name)) { return Core.AdditinalTextures[name]; };
      return null;
    }
    public static GameObject findPrefab(string name) {
      if (Core.AdditinalFXObjects.ContainsKey(name)) { return Core.AdditinalFXObjects[name]; };
      return null;
    }
    public static void FinishedLoading(List<string> loadOrder) {
      Log.M.TWL(0, "FinishedLoading", true);
      try {
        CustomAmmoCategories.CustomCategoriesInit();
      }catch(Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
    }
    public static void Init(string directory, string settingsJson) {
      //SavesForm savesForm = new SavesForm();
      CustomAmmoCategoriesLog.Log.BaseDirectory = directory;
      string settings_filename = Path.Combine(CustomAmmoCategoriesLog.Log.BaseDirectory, "CustomAmmoCategoriesSettings.json");
      //settings_filename = Path.Combine(settings_filename, "CustomAmmoCategoriesSettings.json");
      CustomAmmoCategories.Settings = JsonConvert.DeserializeObject<CustAmmoCategories.Settings>(File.ReadAllText(settings_filename));
      //CustomAmmoCategories.Settings.debugLog = true;
      CustomAmmoCategoriesLog.Log.InitLog();
      CustomAmmoCategoriesLog.Log.LogWrite("Initing... " + directory + " version: " + Assembly.GetExecutingAssembly().GetName().Version + "\n", true);
      string CharlesBSettings = Path.Combine(directory, "CharlesB_settings.json");
      if (File.Exists(CharlesBSettings)) {
        CharlesB.Core.Init(directory, File.ReadAllText(CharlesBSettings));
      }
      //uint testEventId0 = 0;
      try {
        Dictionary<string, uint> audioEvents = (Dictionary<string, uint>)typeof(WwiseManager).GetField("guidIdMap",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(SceneSingletonBehavior<WwiseManager>.Instance);
        Log.LogWrite("audioEvents:\n", true);
        foreach (var aEvent in audioEvents) {
          Log.LogWrite(" '" +aEvent.Key+"':"+aEvent.Value+"\n");
        }
        //    testEventId0 = (uint)EnumValueToEventId.Invoke(SceneSingletonBehavior<WwiseManager>.Instance, new object[1] { val });
      } catch(Exception e) {
        Log.LogWrite("Can't audioEvents " + e.ToString()+"\n",true);
      }
      //CustomAmmoCategories.Settings.MechEngineerDetect();
      WeaponRealizer.Settings WRSettings = null;
      string WRSettingsFile = Path.Combine(directory, CustomAmmoCategories.Settings.WeaponRealizerSettings);
      if (File.Exists(WRSettingsFile) == false) {
        Log.LogWrite("WeaponRealizer settings not exists\n");
        Log.flush();
        WRSettings = new WeaponRealizer.Settings();
      } else {
        try {
          Log.LogWrite("Initing WR\n");
          string WRSettingsContent = File.ReadAllText(WRSettingsFile);
          WRSettings = JsonConvert.DeserializeObject<WeaponRealizer.Settings>(WRSettingsContent);
        } catch (Exception ex) {
          Log.LogWrite(ex + "\n");
          WRSettings = new WeaponRealizer.Settings();
        }
      }
      WeaponRealizer.Core.Init(directory, WRSettings);
      /*
      if (string.IsNullOrEmpty(CustomAmmoCategories.Settings.WeaponRealizerStandalone) == false) {
        CustomAmmoCategoriesLog.Log.LogWrite("standalone WeaponRealizer detected\n");
        string WRPath = Path.Combine(directory, CustomAmmoCategories.Settings.WeaponRealizerStandalone);
        string WRSettingsFile = Path.Combine(directory, CustomAmmoCategories.Settings.WeaponRealizerSettings);
        WeaponRealizer.Settings WRSettings = null;
        if (File.Exists(WRSettingsFile) == false) {
          Log.LogWrite("WeaponRealizer settings not exists\n");
          Log.flush();
          WRSettings = new WeaponRealizer.Settings();
        }
        if (File.Exists(WRPath) == false) {
          Log.LogWrite("WeaponRealizer.dll not exists. I will not load\n");
          Log.flush();
          throw new Exception("WeaponRealizer.dll not exists. I will not load");
        } else {
          CustomAmmoCategoriesLog.Log.LogWrite(WRPath + " - exists. Loading assembly.\n");
          Assembly.LoadFile(WRPath);
          CustomAmmoCategoriesLog.Log.LogWrite("Initing WR\n");
          string WRSettingsContent = File.ReadAllText(WRSettingsFile);
          WRSettings = new WeaponRealizer.Settings();
          try {
            WRSettings = JsonConvert.DeserializeObject<WeaponRealizer.Settings>(WRSettingsContent);
          } catch (Exception ex) {
            Log.LogWrite(ex + "\n");
            WRSettings = new WeaponRealizer.Settings();
          }
          CustomAmmoCategoriesLog.Log.LogWrite("Initing WR:\n" + WRSettingsContent + "\n");
          //typeof(WeaponRealizer.Core).GetMethod("Init", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[2] { (object)directory, (object)settingsJson });
        }
      }*/
      if (string.IsNullOrEmpty(CustomAmmoCategories.Settings.AIMStandalone) == false) {
        string AIMPath = Path.Combine(directory, CustomAmmoCategories.Settings.AIMStandalone);
        if (File.Exists(AIMPath) == false) {
          Log.LogWrite("AttackImprovementMod.dll not exists. I will not load\n");
          Log.flush();
          throw new Exception("AttackImprovementMod.dll not exists. I will not load");
        } else {
          Log.LogWrite(AIMPath + " - exists. Loading assembly.\n");
          Assembly AIM = Assembly.LoadFile(AIMPath);
          Log.LogWrite("Initing AIM\n");
          Type AIMMod = AIM.GetType("Sheepy.BattleTechMod.AttackImprovementMod.Mod");
          AIMMod.GetField("failToLoad",BindingFlags.Static|BindingFlags.Public).SetValue(null, false);
          AIMMod.GetMethod("Init", BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[2] { directory, string.Empty});
          //Sheepy.BattleTechMod.AttackImprovementMod.Mod.failToLoad = false;
          //Sheepy.BattleTechMod.AttackImprovementMod.Mod.Init(directory, string.Empty);
        }
      }
      //typeof(BattleTech.AttackDirectorHelpers.MessageCoordinator).GetField("logger", BindingFlags.Static | BindingFlags.Public).SetValue(null, (object)HBS.Logging.Logger.GetLogger("CombatLog.MechImpacts", HBS.Logging.LogLevel.Debug));
      try {
        string apath = Path.Combine(directory, "assets");
        CustomAmmoCategoriesLog.Log.LogWrite("additional assets:" + CustomAmmoCategories.Settings.AdditinalAssets.Count + "\n");
        foreach (string assetName in CustomAmmoCategories.Settings.AdditinalAssets) {
          string path = Path.Combine(apath, assetName);
          if (File.Exists(path)) {
            var assetBundle = AssetBundle.LoadFromFile(path);
            if (assetBundle != null) {
              CustomAmmoCategoriesLog.Log.LogWrite("asset " + path + ":" + assetBundle.name + " loaded\n");
              UnityEngine.GameObject[] objects = assetBundle.LoadAllAssets<GameObject>();
              CustomAmmoCategoriesLog.Log.LogWrite("FX objects:\n");
              foreach (var obj in objects) {
                CustomAmmoCategoriesLog.Log.LogWrite(" " + obj.name + "\n");
                AdditinalFXObjects.Add(obj.name, obj);
              }
              UnityEngine.Texture2D[] textures = assetBundle.LoadAllAssets<Texture2D>();
              CustomAmmoCategoriesLog.Log.LogWrite("Textures:\n");
              foreach (var tex in textures) {
                CustomAmmoCategoriesLog.Log.LogWrite(" " + tex.name + "\n");
                AdditinalTextures.Add(tex.name, tex);
              }
              UnityEngine.Mesh[] meshes = assetBundle.LoadAllAssets<Mesh>();
              CustomAmmoCategoriesLog.Log.LogWrite("Meshes:\n");
              foreach (var msh in meshes) {
                CustomAmmoCategoriesLog.Log.LogWrite(" " + msh.name + "\n");
                AdditinalMeshes.Add(msh.name, msh);
              }
              UnityEngine.Material[] materials = assetBundle.LoadAllAssets<Material>();
              CustomAmmoCategoriesLog.Log.LogWrite("Materials:\n");
              foreach (var mat in materials) {
                CustomAmmoCategoriesLog.Log.LogWrite(" " + mat.name + "\n");
                if (AdditinalMaterials.ContainsKey(mat.name) == false) {
                  AdditinalMaterials.Add(mat.name, mat);
                }
              }
              UnityEngine.Shader[] shaders = assetBundle.LoadAllAssets<Shader>();
              CustomAmmoCategoriesLog.Log.LogWrite("Shaders:\n");
              foreach (var shdr in shaders) {
                CustomAmmoCategoriesLog.Log.LogWrite(" " + shdr.name + "\n");
                if (AdditinalShaders.ContainsKey(shdr.name) == false) {
                  AdditinalShaders.Add(shdr.name, shdr);
                }
              }
              UnityEngine.AudioClip[] audio = assetBundle.LoadAllAssets<AudioClip>();
              CustomAmmoCategoriesLog.Log.LogWrite("Audio:\n");
              foreach (var au in audio) {
                CustomAmmoCategoriesLog.Log.LogWrite(" " + au.name + "\n");
                if (AdditinalAudio.ContainsKey(au.name) == false) {
                  AdditinalAudio.Add(au.name, au);
                }
              }
            } else {
              CustomAmmoCategoriesLog.Log.LogWrite("fail to load:" + path + "\n");
            }
          } else {
            CustomAmmoCategoriesLog.Log.LogWrite("not exists:" + path + "\n");
          }
        }
        //(typeof(FootstepManager)).GetField("_scorchMaterial", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(
        //FootstepManager.Instance, AdditinalMaterials["SolidColor"]
        //);
        //FootstepManager_scorchMaterial.ScorchMaterial = AdditinalMaterials["bullethole-decal"];
        var harmony = HarmonyInstance.Create("io.mission.modrepuation");
        //Assembly.LoadFile(Path.Combine(directory,"CACPatches.dll"));
        //harmony.PatchAll(Assembly.)
        //var ancorType = AccessTools.TypeByName("MechComponent_UIName");
        //if (ancorType == null) {
        //  CustomAmmoCategoriesLog.Log.LogWrite("Can't find ancor type\n");
        //} else {
        //CustomAmmoCategoriesLog.Log.LogWrite("Ancor type found "+ancorType.Assembly.FullName+"\n");
        //harmony.PatchAll(ancorType.Assembly);
        //}
        harmony.PatchAll(Assembly.GetExecutingAssembly());
        InternalClassPathes.PatchInternalClasses(harmony);
        Thread thread = new Thread(ThreadWork.DoWork);
        thread.Start();
        //Profiler.Init();
      } catch (Exception e) {
        CustomAmmoCategoriesLog.Log.LogWrite(e.ToString() + "\n");
      }
    }
  }
}
