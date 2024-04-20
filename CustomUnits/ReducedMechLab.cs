using BattleTech;
using BattleTech.Data;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using BattleTech.UI.Tooltips;
using CustAmmoCategories;
using CustomComponents.Changes;
using HarmonyLib;
using IRBTModUtils;
using Localize;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CustomComponents.Changes {
  public class Change_Add_Reduced : IChange_Apply, IChange_Optimize {
    private MechComponentRef item;
    private MechLabItemSlotElement slot;
    public bool Applied { get; private set; }
    public string ItemID { get; set; }
    public string SimGameUID { get; set; }
    public ComponentType Type { get; set; }
    public ChassisLocations Location { get; set; }

    public bool Initial { get; set; }

    public void AdjustChange(InventoryOperationState state) {
      if (item == null) {
        return;
      }

      foreach (var add_handler in item.GetComponents<IOnAdd>()) {
        add_handler.OnAdd(Location, state);
      }
    }

    public void PreviewApply(InventoryOperationState state) {
      InvItem i = null;
      if (!Applied) {
        i = new InvItem(DefaultHelper.CreateRef(ItemID, Type, Location), Location);
        item = i.Item;
      }

      if (i != null) {
        state.Inventory.Add(i);
      }
    }

    public void ApplyToInventory(MechDef mech, List<MechComponentRef> inventory) {
      if (Applied) {
        return;
      }

      var r = DefaultHelper.CreateRef(ItemID, Type, Location);
      if (r.Def.CCFlags().Default) {
        inventory.Add(r);
      }
    }

    public void ApplyToMechlab() {
      if (Applied) {
        return;
      }

      var mechLab = MechLabHelper.CurrentMechLab;
      var lhelper = mechLab.GetLocationHelper(Location);
      if (lhelper == null) {
        return;
      }

      if (slot != null) {
        if (mechLab.InSimGame) {
          var subEntry = mechLab.MechLab.Sim.CreateComponentInstallWorkOrder(
              mechLab.MechLab.baseWorkOrder.MechID,
              slot.ComponentRef, Location, slot.MountedLocation);
          mechLab.MechLab.baseWorkOrder.AddSubEntry(subEntry);
        }
      } else {
        slot = DefaultHelper.CreateSlot(ItemID, Type);
      }

      lhelper.widget.OnAddItem(slot, true);
      slot.MountedLocation = Location;

    }

    public void DoOptimization(List<IChange_Apply> current) {
      for (var i = current.Count - 2; i >= 0; i--) {
        var change = current[i];
        if (!change.Initial && change is Change_Remove_Reduced remove && !remove.Applied && remove.Location == Location && remove.ItemID == ItemID && remove.SimGameUID == this.SimGameUID) {
          Log.InventoryOperations.Trace?.Log($"--- OPT {this}, {current[i]}");
          current.RemoveAt(i);
          current.Remove(this);
          return;
        }
      }
    }
    public Change_Add_Reduced(MechLabItemSlotElement slot, ChassisLocations location) {
      this.slot = slot;
      ItemID = slot.ComponentRef.ComponentDefID;
      SimGameUID = slot.ComponentRef.SimGameUID;
      Type = slot.ComponentRef.ComponentDefType;
      Location = location;
    }
    public override string ToString() {
      return $"Change_Add_Reduced {ItemID}:{SimGameUID} => {Location}";
    }
  }
  public class Change_Remove_Reduced : IChange_Apply, IChange_Optimize, IChange {
    private MechComponentRef item;
    public bool Initial { get; set; }
    public bool Applied { get; private set; }
    public string ItemID { get; set; }
    public string SimGameUID { get; set; }
    public ChassisLocations Location { get; set; }
    public void AdjustChange(InventoryOperationState state) {
      if (item == null) {
        return;
      }

      foreach (var rem_handler in item.GetComponents<IOnRemove>()) {
        rem_handler.OnRemove(Location, state);
      }
    }

    public void PreviewApply(InventoryOperationState state) {
      InvItem i = null;

      if (!Applied) {
        i = state.Inventory.FirstOrDefault((x) => { return x.Location == Location && x.Item.ComponentDefID == ItemID && x.Item.SimGameUID == SimGameUID; });
        item = i?.Item;
      }

      if (i == null) {
        return;
      }

      state.Inventory.Remove(i);
    }

    public void ApplyToInventory(MechDef mech, List<MechComponentRef> inventory) {
      if (Applied) {
        return;
      }

      var item = inventory.FirstOrDefault(i => i.MountedLocation == Location && i.ComponentDefID == ItemID && i.SimGameUID == SimGameUID);
      if (item != null) {
        inventory.Remove(item);
      }
    }

    public void ApplyToMechlab() {
      if (Applied) {
        return;
      }

      var lhelper = MechLabHelper.CurrentMechLab.GetLocationHelper(Location);
      if (lhelper == null) {
        return;
      }

      var item = lhelper.LocalInventory.FirstOrDefault(i =>
          i.ComponentRef.ComponentDefID == ItemID && i.ComponentRef.SimGameUID == this.SimGameUID &&
          !i.ComponentRef.IsModuleFixed(MechLabHelper.CurrentMechLab.ActiveMech));
      if (item != null) {
        lhelper.widget.OnRemoveItem(item, true);
        if (item.ComponentRef.Def.CCFlags().Default) {
          item.thisCanvasGroup.blocksRaycasts = true;
          MechLabHelper.CurrentMechLab.MechLab.dataManager.PoolGameObject(MechLabPanel.MECHCOMPONENT_ITEM_PREFAB, item.gameObject);
        } else {
          MechLabHelper.CurrentMechLab.MechLab.ForceItemDrop(item);
        }
      }
    }

    public void DoOptimization(List<IChange_Apply> current) {
      for (var i = current.Count - 2; i >= 0; i--) {
        var change = current[i];
        if (!change.Initial && change is Change_Add add && !add.Applied && add.Location == Location && add.ItemID == ItemID) {
          Log.InventoryOperations.Trace?.Log($"--- OPT {this}, {current[i]}");
          current.RemoveAt(i);
          current.Remove(this);
          return;
        }
      }
    }

    public Change_Remove_Reduced() {
    }

    public Change_Remove_Reduced(MechComponentRef item, ChassisLocations location, bool already_applied = false) {
      ItemID = item.ComponentDefID;
      SimGameUID = item.SimGameUID;
      Location = location;
      if (already_applied) {
        Applied = true;
        this.item = item;
      }
    }
    public override string ToString() {
      return $"Change_Remove {ItemID}:{SimGameUID} =X {Location}";
    }
  }
}

namespace CustomUnits {
  [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
  public class NextVehiclesEdit : CustomSettings.NextSettingValueUI {
    public override void Next(object settings, CustomSettings.ModSettingElement ui) {
      Log.M?.TWL(0, $"NextVehiclesEdit.Next {settings.GetType()}");
      if (settings is CustomUnits.CUSettings set) {
        if (UnityGameInstance.BattleTechGame.Simulation != null) {
          GenericPopupBuilder.Create(GenericPopupType.Error, "You can't change this setting if save is loaded").AddFader().CancelOnEscape().Render();
          return;
        }
        if (CustomPrewarm.Core.PrewarmInProcess) {
          GenericPopupBuilder.Create(GenericPopupType.Error, "You can't change this setting while cache still gathering").AddFader().CancelOnEscape().Render();
          return;
        }
        if (Core.Settings.VehcilesPartialEditable) {
          GenericPopupBuilder.Create(GenericPopupType.Error, "Once enabled it can't be disabled").AddFader().CancelOnEscape().Render();
          return;
        }
        GenericPopupBuilder.Create(GenericPopupType.Info, "You will not be able to disable this feature. Hope you've read option's description. Are you sure?")
          .AddButton("Yes", () => {
            set.VehcilesPartialEditable = true;
            Core.Settings.VehcilesPartialEditable = true;
            ui.parent.hasUnsavedChanges = true;
            ui.UpdateValue();
            CustomPrewarm.MainMenu_ShowRefreshingSaves.ResetCache();
          }, true).AddButton("No", () => { }, true).AddFader().Render();
      }
    }
    public NextVehiclesEdit() {
    }
  }
  public static class ReducedComponentRefInfoHelper {
    private static Dictionary<string, ReducedComponentRefInfo> data = new Dictionary<string, ReducedComponentRefInfo>();
    private static string REDUSED_INFO_STAT_NAME = "CU_REDUSED_INFO";
    public static readonly string VEHICLES_EDITABLE_STAT_NAME = "CU_VEHICLES_EDITABLE";
    private static Dictionary<string, float> CarryLeftOver_cache = new Dictionary<string, float>();
    public static float CarryLeftOver(this BaseComponentRef componentRef) {
      string id = CustomComponents.Database.Identifier(componentRef.Def);
      if (CarryLeftOver_cache.TryGetValue(id, out var result)) { return result; }
      result = float.NaN;
      var customs = CustomComponents.Database.Shared.GetOrCreateCustomsList(CustomComponents.Database.Identifier(componentRef.Def));
      Log.M?.TWL(0, $"ReducedComponentRefInfoHelper.CarryLeftOver {componentRef.Def.Description.Id}");
      foreach (var custom in customs) {
        Log.M?.WL(1, $"{custom.GetType().ToString()}");
        if (custom.GetType().ToString() == "MechEngineer.Features.CustomCapacities.CapacityModCustom") {
          Log.M?.WL(2, $"Collection:{Traverse.Create(custom).Property<string>("Collection").Value}");
          if (Traverse.Create(custom).Property<string>("Collection").Value == "CarryLeftOver") {
            result = Traverse.Create(custom).Property<float>("Quantity").Value;
            CarryLeftOver_cache.Add(id, result);
            return result;
          }
        }
      }
      CarryLeftOver_cache.Add(id, result);
      return result;
    }
    public static bool isEditable(this BaseComponentRef componentRef) {
      if ((componentRef.ComponentDefType != ComponentType.AmmunitionBox) && (componentRef.ComponentDefType != ComponentType.Weapon)) { return false; }
      if (componentRef.Def == null) { return false; }
      if ((string.IsNullOrEmpty(componentRef.SimGameUID) == false) && (componentRef.SimGameUID.StartsWith("FixedEquipment-"))) { return false; }
      if (CustomComponents.FlagsExtentions.Flags<CustomComponents.CCFlags>(componentRef.Def).NoSalvage) { return false; }
      return true;
    }
    public static void Save(SimGameState sim) {
      sim.CompanyStats.GetOrCreateStatisic<string>(REDUSED_INFO_STAT_NAME, "{}").SetValue(JsonConvert.SerializeObject(data));
      if (Core.Settings.VehcilesPartialEditable) {
        sim.CompanyStats.GetOrCreateStatisic<bool>(VEHICLES_EDITABLE_STAT_NAME, false).SetValue<bool>(true);
      }
    }
    public static void Load(SimGameState sim) {
      data = JsonConvert.DeserializeObject<Dictionary<string, ReducedComponentRefInfo>>(sim.CompanyStats.GetOrCreateStatisic<string>(REDUSED_INFO_STAT_NAME, "{}").Value<string>());
    }
    public static ReducedComponentRefInfo reducedInfo(this MechComponentRef componentRef) {
      if (string.IsNullOrEmpty(componentRef.SimGameUID)) {
        return new ReducedComponentRefInfo(componentRef.SimGameUID, componentRef.ComponentDefID);
      }
      if (data.TryGetValue(componentRef.SimGameUID, out var result)) {
        return result;
      }
      result = new ReducedComponentRefInfo(componentRef.SimGameUID, componentRef.ComponentDefID);
      data.Add(componentRef.SimGameUID, result);
      return result;
    }
    public static ReducedComponentRefInfo reducedInfo(string simgameUID) {
      if (data.TryGetValue(simgameUID, out var result)) {
        return result;
      }
      return null;
    }
    public static string description(Strings.Culture culture) {
      StringBuilder sb = new StringBuilder();
      switch (culture) {
        case Strings.Culture.CULTURE_RU_RU:
          sb.AppendLine("С включением машинки становится возможным частично модифицировать.");
          sb.Append("Частично - значит: нельзя добавлять оборудование, нельзя убирать оборудование, кроме боеприпасов и пушек. ");
          sb.Append("При этом когда вынимаешь оборудование на его месте сразу появляется такое же только в уничтоженном состоянии. ");
          sb.Append("На место бункера с патронами или пушки можно поставить другой бункер или пушку, для этого новое оборудование следует переташить поверх текущего. ");
          sb.Append("Новое оборудование не должно быть большим по размеру или более тяжелым. Для пушек еще проверятся соотвествие типу и генерируемое тепло. ");
          sb.Append("Появляется необходимость ремонтировать машинки после боя, если им был нанесен урон.");
          sb.Append("Появляется возможность убрать машинку на склад - при этом из нее вытаскиваются патроны и пушки.");
          sb.Append("При восстановлении со склада или сборки из частей машинка восстанавливается с уничтоженными пушками и патронами.");
          sb.Append("При попадении вражеской машины в сальваг из нее выпадают только пушки и патроны, если не были уничтожены");
          break;
        default:
          sb.AppendLine("Enabling this option allows vehicles to be partially edited. ");
          sb.Append("Partially means you can't add or remove equipment only replace existing one and only weapons and ammo boxes");
          sb.Append("If you remove component (weapon or ammo box) from vehicle it will be immediatly replaced to component of same type in destroyed state. ");
          sb.Append("You can replace or repair (if destroyed) existing ammo box or weapon by draging new component over existing one. ");
          sb.Append("New equipment should be same or less size, tonnage and for weapons - same category and generate less or equal heat. ");
          sb.Append("If enabled your vehicles will not be automatically repaired after combat. ");
          sb.Append("If enabled you will be able to store vehicle, ammo boxes and weapon will removed from vehicle and added to your storage. ");
          sb.Append("On restore vehicles will come with destroyed ammo boxes and weapons. ");
          sb.Append("On enemy vehicle salvage only ammo boxes and weapons will accessible. ");
          break;
      }
      return sb.ToString();
    }
  }
  public class ReducedComponentRefInfo {
    public string SimGameUID { get; set; } = string.Empty;
    public string originalDefinition { get; set; } = string.Empty;
    public string originalSimUID { get; set; } = string.Empty;
    public ReducedComponentRefInfo(string simid, string origdef) {
      this.SimGameUID = simid;
      this.originalDefinition = origdef;
      this.originalSimUID = simid;
    }
  }
  public static class MechLabPanel_InitWidgets_Reduced {
    public static void Prefix(MechLabPanel __instance) {
      //Log.M?.TWL(0, "MechLabPanel.InitWidgets prefix");
      //MechLabLocationWidget[] locationWidgets = __instance.gameObject.GetComponentsInChildren<MechLabLocationWidget>(true);
      //foreach (var widget in locationWidgets) {
      //  Log.M?.WL(1, $"{widget.gameObject.name}:{widget.GetInstanceID()} parent:{widget.transform.parent.name}");
      //  widget.gameObject.SetActive(true);
      //}
    }
    public static void Postfix(MechLabPanel __instance) {
      try {
        //ReducedMechLabLocationWidget widget = __instance.gameObject.GetComponent<ReducedMechLabLocationWidget>();
        //if (widget == null) { widget = ReducedMechLabLocationWidget.Instantine(__instance); }
        //Log.M?.TWL(0, "MechLabPanel.InitWidgets postfix");
        //MechLabLocationWidget[] locationWidgets = __instance.gameObject.GetComponentsInChildren<MechLabLocationWidget>(true);
        //foreach (var widget in locationWidgets) {
        //  Log.M?.WL(1, $"{widget.gameObject.name}:{widget.GetInstanceID()} parent:{widget.transform.parent.name}");
        //  widget.gameObject.SetActive(true);
        //}
      } catch (Exception e) {
        UIManager.logger.LogException(e);
        CustomUnits.Log.M?.TWL(0, e.ToString());
      }
    }
  }
  [HarmonyPatch(typeof(MechLabPanel))]
  [HarmonyPatch("LoadMech")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyAfter("MechEngineer.Features.MechLabSlots")]
  [HarmonyPatch(new Type[] { typeof(MechDef) })]
  public static class MechLabPanel_LoadMech {
    public static void FixVehicleLocationsSlots(this MechDef mechDef) {
      Dictionary<ChassisLocations, int> locationsSizes = new Dictionary<ChassisLocations, int>();
      Log.M?.TWL(0, $"FixLocationsSlots {mechDef.ChassisID}");
      foreach (var component in mechDef.Inventory) {
        int InventorySize = 1;
        if (component == null) { continue; }
        if (component.Def == null) { continue; }
        switch (component.Def.ComponentType) {
          case ComponentType.AmmunitionBox:
          case ComponentType.Weapon:
            //if (component.DamageLevel == ComponentDamageLevel.Destroyed) { component.IsFixed = false; }
            InventorySize = component.Def.InventorySize;
            break;
        }
        if (component.Def.InventorySize == 0) { continue; }
        if (component.Def.ComponentType == ComponentType.Upgrade) {
          if (component.MountedLocation != ChassisLocations.CenterTorso) {
            if (component.Def.AllowedLocations == ChassisLocations.CenterTorso) {
              if (component.Def.DisallowedLocations == ChassisLocations.All) {
                continue;
              }
            }
          }
        }
        Log.M?.WL(1, $"{component.ComponentDefID}:{component.MountedLocation}");
        if (locationsSizes.ContainsKey(component.MountedLocation)) {
          locationsSizes[component.MountedLocation] += InventorySize;
        } else {
          locationsSizes.Add(component.MountedLocation, InventorySize);
        }
      }
      for (int index = 0; index < mechDef.Chassis.Locations.Length; ++index) {
        if (locationsSizes.TryGetValue(mechDef.Chassis.Locations[index].Location, out var invSize)) {
          mechDef.Chassis.Locations[index].InventorySlots = invSize;
        }
      }
      mechDef.Chassis.refreshLocationReferences();
      foreach (var locationSize in locationsSizes) {
        Log.M?.WL(1, $" {locationSize.Key} {mechDef.Chassis.GetLocationDef(locationSize.Key).InventorySlots}");
      }
    }
    public static void Prefix(ref bool __runOriginal, MechLabPanel __instance, MechDef newMechDef) {
      try {
        if (Core.Settings.VehcilesPartialEditable == false) { return; }
        if (__runOriginal == false) { return; }
        Log.M?.TWL(0, "MechLabPanel.LoadMech");
        //MechLabLocationWidget[] locationWidgets = __instance.gameObject.GetComponentsInChildren<MechLabLocationWidget>(true);
        //foreach (var widget in locationWidgets) {
        //  Log.M?.WL(1, $"{widget.gameObject.name}:{widget.GetInstanceID()} parent:{widget.transform.parent.name}");
        //  widget.gameObject.SetActive(true);
        //}
        if (newMechDef.IsVehicle() == false) { return; }
        newMechDef.FixVehicleLocationsSlots();
        newMechDef.PushMechLab();
      } catch (Exception e) {
        UIManager.logger.LogException(e);
        CustomUnits.Log.M?.TWL(0, e.ToString());
      }
    }
    public static void Postfix(MechLabPanel __instance, MechDef newMechDef) {
      try {
        if (Core.Settings.VehcilesPartialEditable == false) { return; }
        if (newMechDef.IsVehicle() == false) { return; }
        newMechDef.PopMechLab();
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
        UIManager.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(MechLabMechInfoWidget), "RefreshInfo")]
  [HarmonyAfter("MechEngineer.Features.CustomCapacities")]
  public static class MechLabMechInfoWidget_RefreshInfo_Patch {
    [HarmonyPostfix]
    [HarmonyWrapSafe]
    public static void Postfix(MechLabMechInfoWidget __instance) {
      RedusedMechLabMechInfoWidget redused = __instance.gameObject.GetComponent<RedusedMechLabMechInfoWidget>();
      if (redused == null) { redused = RedusedMechLabMechInfoWidget.Instantine(__instance); }
      redused.Init(__instance.mechLab.originalMechDef);
    }
  }
  public class CUExitApplication : MonoBehaviour {
    public GenericPopup popup = null;
    public float T = 0f;
    public void Update() {
      if (T > 0f) { T -= Time.deltaTime; return; } else
      if (popup == null) {
        this.popup = GenericPopupBuilder.Create(GenericPopupType.Error, "To be able to use this save you should enable editable vehicles").AddButton("Exit application", () => { this.popup = null; }, true).SetOnClose(() => { this.popup = null; Application.Quit(); }).AddFader().CancelOnEscape().Render();
      }
    }
  }
  [HarmonyPatch(typeof(SGLeftNavDrawer), "Init")]
  public static class SGLeftNavDrawer_Init {
    [HarmonyPostfix]
    [HarmonyWrapSafe]
    public static void Postfix(SGLeftNavDrawer __instance, SimGameState theSim, SGRoomManager theManager) {
      try {
        var reduced_enabled = __instance.simState.CompanyStats.GetStatistic(ReducedComponentRefInfoHelper.VEHICLES_EDITABLE_STAT_NAME);
        if (reduced_enabled != null) {
          if (reduced_enabled.Value<bool>() == true) {
            if (Core.Settings.VehcilesPartialEditable == false) {
              CUExitApplication exiter = __instance.gameObject.GetComponent<CUExitApplication>();
              if (exiter == null) { exiter = __instance.gameObject.AddComponent<CUExitApplication>(); };
              exiter.T = 1f;
            }
          }
        }
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString());
        UIManager.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(MechLabItemSlotElement))]
  [HarmonyPatch("SetSpacers")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechLabItemSlotElement_SetSpacers {
    public static void Postfix(MechLabItemSlotElement __instance) {
      try {
        if (Core.Settings.VehcilesPartialEditable == false) { return; }
        MechDef mechDef = null;
        if (__instance.dropParent is MechLabLocationWidget locationWidget) {
          if (locationWidget.parentDropTarget is MechLabPanel mechLabPanel) {
            mechDef = mechLabPanel.activeMechDef;
          }
        }
        if (mechDef == null) { return; }
        if (mechDef.IsVehicle() == false) { return; }
        if (__instance.ComponentRef.isEditable()) {
          __instance.SetDraggable(__instance.ComponentRef.DamageLevel != ComponentDamageLevel.Destroyed);
        } else {
          __instance.SetDraggable(false);
          for (int index = 0; index < __instance.spacers.Count; ++index) {
            __instance.spacers[index].SetActive(false);
          }
          __instance.thisRectTransform.sizeDelta = new Vector2(__instance.thisRectTransform.sizeDelta.x, 32f * 1f);
        }
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
        UIManager.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(MechLabLocationWidget), "OnRemoveItem")]
  [HarmonyBefore("io.github.denadan.CustomComponents")]
  public static class MechLabLocationWidget_OnRemoveItem {
    public static bool IN_REPLACE_STATE { get; set; } = false;
    public static void Postfix(MechLabLocationWidget __instance, IMechLabDraggableItem item, ref bool __result) {
      try {
        if (Core.Settings.VehcilesPartialEditable == false) { return; }
        if (__result == false) { return; }
        if (__instance.mechLab.originalMechDef.IsVehicle() == false) { return; }
        MechLabItemSlotElement mechComponent = item as MechLabItemSlotElement;
        Log.M?.TWL(0, $"MechLabLocationWidget.OnRemoveItem {mechComponent.ComponentRef.ComponentDefID}:{mechComponent.ComponentRef.DamageLevel} IN_REPLACE_STATE:{IN_REPLACE_STATE}");
        if (IN_REPLACE_STATE) { IN_REPLACE_STATE = false; return; }
        if (mechComponent.ComponentRef.DamageLevel == ComponentDamageLevel.Destroyed) { return; }
        MechComponentRef removedRef = new MechComponentRef(item.ComponentRef);
        removedRef.DamageLevel = ComponentDamageLevel.Destroyed;
        removedRef.MountedLocation = __instance.loadout.Location;
        __instance.mechLab.activeMechInventory.Add(removedRef);
        //removedRef.SetSimGameUID(removedRef.reducedInfo().originalSimGameUID);
        removedRef.ComponentDefID = removedRef.reducedInfo().originalDefinition;
        MechLabItemSlotElement mechComponentItem = __instance.mechLab.CreateMechComponentItem(removedRef, false, __instance.loadout.Location, __instance);
        if (mechComponentItem != null) {
          __instance.localInventory.Add(mechComponentItem);
          __instance.usedSlots = 0;
          mechComponentItem.gameObject.transform.SetParent(__instance.inventoryParent, false);
          mechComponentItem.gameObject.transform.localScale = Vector3.one;
          mechComponentItem.SetDraggable(false);
        }
        List<MechComponentRef> inventory = __instance.mechLab.activeMechDef.inventory.ToList();
        inventory.Add(removedRef);
        __instance.mechLab.activeMechDef.SetInventory(inventory.ToArray());
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
        UIManager.logger.LogException(e);
      }
    }
  }
  //[HarmonyPatch(typeof(WorkOrderEntry_InstallComponent))]
  //[HarmonyPatch(MethodType.Constructor)]
  //[HarmonyPatch(new Type[] { typeof(string), typeof(string), typeof(string), typeof(MechComponentRef), typeof(int), typeof(string), typeof(ChassisLocations), typeof(ChassisLocations), typeof(int), typeof(int), typeof(string) })]
  //public static class WorkOrderEntry_InstallComponent_Constructor {
  //  public static bool IN_REPLACE_STATE = false;
  //  public static void Prefix(ref string ID) {
  //    try {
  //      if (IN_REPLACE_STATE) {
  //        //IN_REPLACE_STATE = false;
  //        ID = "NO_ADD_REMOVED_" + ID;
  //      }
  //    } catch (Exception e) {
  //      Log.M?.TWL(0, e.ToString(), true);
  //      UIManager.logger.LogException(e);
  //    }
  //  }
  //  public static void Postfix(WorkOrderEntry_InstallComponent __instance) {
  //    Log.M?.TWL(0, $"WorkOrderEntry_InstallComponent.Constructor {__instance.ID} {__instance.MechComponentRef.ComponentDefID}:{__instance.MechComponentRef.SimGameUID} prev:{__instance.PreviousLocation} cur:{__instance.DesiredLocation}");
  //    Log.M?.WL(0,Environment.StackTrace.ToString());
  //  }
  //}
  [HarmonyPatch(typeof(MechLabLocationWidget), "StripEquipment")]
  [HarmonyBefore("io.github.denadan.CustomComponents")]
  internal static class MechLabLocationWidget_StripEquipment_Patch {
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static void Prefix(ref bool __runOriginal, MechLabLocationWidget __instance) {
      if (Core.Settings.VehcilesPartialEditable == false) { return; }
      if (__runOriginal == false) { return; }
      if (__instance.mechLab.Initialized == false) { return; }
      if (__instance.mechLab.activeMechDef.IsVehicle() == false) { return; }
      try {
        Log.M?.TWL(0, $"StripEquipment in {__instance.loadout.Location}");
        CustomComponents.LocationHelper locationHelper = CustomComponents.MechLabHelper.CurrentMechLab.GetLocationHelper(__instance.loadout.Location);
        bool isControlPressed = CustomComponents.InputHelper.IsControlPressed;
        Queue<IChange> start_changes = new Queue<IChange>();
        foreach (MechLabItemSlotElement labItemSlotElement in locationHelper.LocalInventory) {
          if (isControlPressed && labItemSlotElement.ComponentRef.ComponentDefType != ComponentType.Weapon) { continue; }
          if (labItemSlotElement.ComponentRef.IsFixed) { continue; }
          if (labItemSlotElement.ComponentRef.DamageLevel == ComponentDamageLevel.Destroyed) { continue; }
          if ((labItemSlotElement.ComponentRef.ComponentDefType != ComponentType.Weapon) && (labItemSlotElement.ComponentRef.ComponentDefType != ComponentType.AmmunitionBox)) { continue; }

          start_changes.Enqueue(new Change_Remove(labItemSlotElement.ComponentRef.ComponentDefID, __instance.loadout.Location));
          Log.M?.WL(1, "- remove " + labItemSlotElement.ComponentRef.ComponentDefID);
        }
        CustomComponents.InventoryOperationState inventoryOperationState = new CustomComponents.InventoryOperationState(start_changes, CustomComponents.MechLabHelper.CurrentMechLab.ActiveMech);
        inventoryOperationState.DoChanges();
        inventoryOperationState.ApplyMechlab();
        __runOriginal = false;
      } catch (Exception e) {
        UIManager.logger.LogException(e);
        Log.M?.TWL(0, e.ToString());
      }
    }
  }
  [HarmonyPatch(typeof(MechLabLocationWidget), "RepairAll")]
  [HarmonyBefore("io.github.denadan.CustomComponents")]
  internal static class MechLabLocationWidget_RepairAll {
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static void Prefix(ref bool __runOriginal, MechLabLocationWidget __instance, bool forceRepairStructure, bool validate) {
      if (Core.Settings.VehcilesPartialEditable == false) { return; }
      if (__runOriginal == false) { return; }
      if (__instance.mechLab.Initialized == false) { return; }
      if (__instance.mechLab.activeMechDef.IsVehicle() == false) { return; }
      if (__instance.IsSimGame == false) { return; }
      try {
        Log.M?.TWL(0, $"RepairAll {__instance.mechLab.activeMechDef.ChassisID} in {__instance.loadout.Location}");
        if (forceRepairStructure || __instance.loadout.CurrentInternalStructure > 0.0) { __instance.RepairStructure(validate); }
        //CustomComponents.LocationHelper locationHelper = CustomComponents.MechLabHelper.CurrentMechLab.GetLocationHelper(__instance.loadout.Location);
        foreach (MechLabItemSlotElement labItemSlotElement in __instance.localInventory) {
          Log.M?.WL(1, $"{labItemSlotElement.componentRef.ComponentDefID}:{labItemSlotElement.ComponentRef.ComponentDefType}:{labItemSlotElement.ComponentRef.DamageLevel}:{labItemSlotElement.ComponentRef.SimGameUID}");
          if (labItemSlotElement.ComponentRef.DamageLevel == ComponentDamageLevel.Functional) { continue; }
          if (labItemSlotElement.ComponentRef.DamageLevel == ComponentDamageLevel.Installing) { continue; }
          if (labItemSlotElement.ComponentRef.DamageLevel != ComponentDamageLevel.Destroyed) {
            labItemSlotElement.RepairComponent(true);
            Log.M?.WL(2, $"repair");
            continue;
          }
          //if ((string.IsNullOrEmpty(labItemSlotElement.ComponentRef.SimGameUID) == false) && (labItemSlotElement.ComponentRef.SimGameUID.StartsWith("FixedEquipment-"))) {
          //  //if (labItemSlotElement.ComponentRef.IsFixed) { labItemSlotElement.ComponentRef.IsFixed = false; }
          //  labItemSlotElement.ComponentRef.DamageLevel = ComponentDamageLevel.Penalized;
          //  labItemSlotElement.RepairComponent(true);
          //  Log.M?.WL(2, $"repair");
          //  continue;
          //}
          if (labItemSlotElement.ComponentRef.isEditable()) {
            continue;
          }
          //if ((labItemSlotElement.ComponentRef.ComponentDefType == ComponentType.Weapon) || (labItemSlotElement.ComponentRef.ComponentDefType == ComponentType.AmmunitionBox)) {
          //  continue;
          //}
          labItemSlotElement.ComponentRef.DamageLevel = ComponentDamageLevel.Penalized;
          labItemSlotElement.RepairComponent(true);
          Log.M?.WL(2, $"repair");
        }
        __runOriginal = false;
      } catch (Exception e) {
        UIManager.logger.LogException(e);
        Log.M?.TWL(0, e.ToString());
      }
    }
  }
  [HarmonyPatch(typeof(MechValidationRules))]
  [HarmonyPatch("ValidateMechCanBeFielded")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyAfter("io.github.denadan.CustomComponents")]
  public static class MechValidationRules_ValidateMechCanBeFielded {
    public static void Postfix(SimGameState sim, MechDef mechDef, ref bool __result) {
      try {
        Log.M?.TW(0, "MechValidationRules.ValidateMechCanBeFielded");
        if (mechDef == null) { Log.Combat?.WL(1, "mechDef is null"); return; };
        Log.M?.W(1, mechDef.Description.Id);
        bool isVehicle = mechDef.IsVehicle();
        Log.M?.WL(1, $"{mechDef.ChassisID}:{mechDef.GUID} vehicle:{isVehicle} result:{__result}");
        if (Core.Settings.VehcilesPartialEditable && isVehicle) {
          if (__result == false) { return; }
          bool inMaintaince = MechValidationRules.ValidateSimGameMechNotInMaintenance(sim, mechDef) == false;
          Log.M?.WL(1, "inMaintaince = " + inMaintaince);
          bool badStructure = MechValidationRules.ValidateMechStructureSimple(mechDef) == false;
          Log.M?.WL(1, "badStructure = " + badStructure);
          bool badWeapon = (MechValidationRules.ValidateMechPosessesWeaponsSimple(mechDef) == false) || (mechDef.CheckVehicleSimple() == false);
          Log.M?.WL(1, "badWeapon = " + badWeapon);
          __result = (inMaintaince || badStructure || badWeapon) == false;
          Log.M?.WL(1, "CanBeFielded:" + __result);
        } else {
          if (__result == true) { return; }
          bool inMaintaince = MechValidationRules.ValidateSimGameMechNotInMaintenance(sim, mechDef) == false;
          Log.M?.WL(1, "inMaintaince = " + inMaintaince);
          bool badStructure = MechValidationRules.ValidateMechStructureSimple(mechDef) == false;
          Log.M?.WL(1, "badStructure = " + badStructure);
          bool badWeapon = MechValidationRules.ValidateMechPosessesWeaponsSimple(mechDef) == false;
          Log.M?.WL(1, "badWeapon = " + badWeapon);
          __result = (inMaintaince || badStructure || badWeapon) == false;
          Log.M?.WL(1, "CanBeFielded:" + __result);
        }
      } catch (Exception e) {
        Log.E?.TWL(0, e.ToString());
        UnityGameInstance.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(MechValidationRules), "ValidateMechDef")]
  [HarmonyAfter("io.github.denadan.CustomComponents")]
  public static class MechValidationRulesValidate_ValidateMech_Patch {
    public static void Postfix(ref Dictionary<MechValidationType, List<Localize.Text>> __result, MechValidationLevel validationLevel, DataManager dataManager, MechDef mechDef, WorkOrderEntry_MechLab baseWorkOrder) {
      try {
        if (mechDef.IsVehicle() == false) { return; }
        if (Core.Settings.VehcilesPartialEditable == false) { return; }
        __result = MechValidationRules.InitializeValidationResults();
        switch (validationLevel) {
          case MechValidationLevel.Basic:
            MechValidationRules.ValidateMechStructure(mechDef, validationLevel, baseWorkOrder, ref __result);
            MechValidationRules.ValidateMechPosessesWeapons(dataManager, mechDef, validationLevel, baseWorkOrder, ref __result);
            MechValidationRules.ValidateMechHasAppropriateAmmo(dataManager, mechDef, validationLevel, baseWorkOrder, ref __result);
            MechValidationRules.ValidateMechJumpjetCount(dataManager, mechDef, ref __result);
            break;
          case MechValidationLevel.Full:
            MechValidationRules.ValidateMechChassis(dataManager, mechDef, ref __result);
            MechValidationRules.ValidateMechStructure(mechDef, validationLevel, baseWorkOrder, ref __result);
            MechValidationRules.ValidateMechPosessesWeapons(dataManager, mechDef, validationLevel, baseWorkOrder, ref __result);
            MechValidationRules.ValidateMechHasAppropriateAmmo(dataManager, mechDef, validationLevel, baseWorkOrder, ref __result);
            MechValidationRules.ValidateMechJumpjetCount(dataManager, mechDef, ref __result);
            break;
          case MechValidationLevel.FullStock:
            MechValidationRules.ValidateMechChassis(dataManager, mechDef, ref __result);
            MechValidationRules.ValidateMechIsStockMechDef(dataManager, mechDef, ref __result);
            MechValidationRules.ValidateMechStructure(mechDef, validationLevel, baseWorkOrder, ref __result);
            MechValidationRules.ValidateMechPosessesWeapons(dataManager, mechDef, validationLevel, baseWorkOrder, ref __result);
            MechValidationRules.ValidateMechHasAppropriateAmmo(dataManager, mechDef, validationLevel, baseWorkOrder, ref __result);
            MechValidationRules.ValidateMechJumpjetCount(dataManager, mechDef, ref __result);
            break;
          case MechValidationLevel.MechLab:
            MechValidationRules.ValidateMechStructure(mechDef, validationLevel, baseWorkOrder, ref __result);
            MechValidationRules.ValidateMechPosessesWeapons(dataManager, mechDef, validationLevel, baseWorkOrder, ref __result);
            MechValidationRules.ValidateMechHasAppropriateAmmo(dataManager, mechDef, validationLevel, baseWorkOrder, ref __result);
            MechValidationRules.ValidateMechJumpjetCount(dataManager, mechDef, ref __result);
            break;
        }
      } catch (Exception e) {
        MechLabPanel.logger.LogException(e);
      }
    }
  }
  //[HarmonyPatch(typeof(MechLabItemSlotElement), "RepairComponent")]
  //[HarmonyBefore("io.github.denadan.CustomComponents")]
  //internal static class MechLabItemSlotElement_RepairComponent {
  //  [HarmonyPrefix]
  //  [HarmonyWrapSafe]
  //  public static void Prefix(MechLabItemSlotElement __instance) {
  //    try {
  //      if (Core.Settings.VehcilesPartialEditable == false) { return; }
  //      Log.M?.TWL(0, $"RepairComponent {__instance.componentRef.ComponentDefID}:{__instance.componentRef.SimGameUID}");
  //      Log.M?.WL(0,Environment.StackTrace.ToString());
  //    } catch (Exception e) {
  //      UIManager.logger.LogException(e);
  //      Log.M?.TWL(0, e.ToString());
  //    }
  //  }
  //}
  [HarmonyPatch(typeof(MechLabLocationWidget), "RepairStructure")]
  [HarmonyBefore("io.github.denadan.CustomComponents")]
  public static class MechLabLocationWidget_RepairStructure {
    public static void Prefix(ref bool __runOriginal, MechLabLocationWidget __instance, bool validate, ref Dictionary<string, ComponentDamageLevel> __state) {
      __state = null;
      if (Core.Settings.VehcilesPartialEditable == false) { return; }
      if (__runOriginal == false) { return; }
      if (__instance.mechLab.Initialized == false) { return; }
      if (__instance.mechLab.activeMechDef.IsVehicle() == false) { return; }
      if (__instance.IsSimGame == false) { return; }
      try {
        __runOriginal = false;
        Log.M?.TWL(0, $"RepairStructure {__instance.mechLab.activeMechDef.ChassisID} in {__instance.loadout.Location}");
        __state = new Dictionary<string, ComponentDamageLevel>();
        foreach (MechLabItemSlotElement labItemSlotElement in __instance.localInventory) {
          Log.M?.WL(1, $"{labItemSlotElement.componentRef.ComponentDefID}:{labItemSlotElement.ComponentRef.ComponentDefType}:{labItemSlotElement.ComponentRef.DamageLevel}:{labItemSlotElement.ComponentRef.SimGameUID}");
          if (labItemSlotElement.ComponentRef.isEditable() == false) { continue; }
          //if (string.IsNullOrEmpty(labItemSlotElement.ComponentRef.SimGameUID)) { continue; }
          //if (labItemSlotElement.ComponentRef.SimGameUID.StartsWith("FixedEquipment-")) { continue; }
          __state[labItemSlotElement.ComponentRef.SimGameUID] = labItemSlotElement.ComponentRef.DamageLevel;
        }
        __instance.needsRepair = false;
        __instance.damagedOverlay.SetActive(false);
        __instance.destroyedOverlay.SetActive(false);
        float structureCount = Mathf.Max(0.0f, __instance.chassisLocationDef.InternalStructure - __instance.loadout.CurrentInternalStructure);
        __instance.loadout.CurrentInternalStructure = __instance.chassisLocationDef.InternalStructure;
        __instance.structureText.SetText(string.Format("{0} / {1}", (int)__instance.loadout.CurrentInternalStructure, (int)__instance.chassisLocationDef.InternalStructure));
        __instance.AutoRepairFixedEquipment();
        if (!validate || structureCount <= 0f) { return; }
        __instance.mechLab.FlagAsModified();
        __instance.mechLab.baseWorkOrder.AddSubEntry(__instance.Sim.CreateMechRepairWorkOrder(__instance.mechLab.activeMechDef.GUID, __instance.loadout.Location, (int)structureCount));
      } catch (Exception e) {
        __runOriginal = true;
        UIManager.logger.LogException(e);
        Log.M?.TWL(0, e.ToString());
      }
    }
    public static void Postfix(MechLabLocationWidget __instance, ref Dictionary<string, ComponentDamageLevel> __state) {
      if (Core.Settings.VehcilesPartialEditable == false) { return; }
      if (__instance.mechLab.Initialized == false) { return; }
      if (__instance.mechLab.activeMechDef.IsVehicle() == false) { return; }
      if (__instance.IsSimGame == false) { return; }
      if (__state == null) { return; }
      try {
        foreach (MechLabItemSlotElement labItemSlotElement in __instance.localInventory) {
          //if (string.IsNullOrEmpty(labItemSlotElement.ComponentRef.SimGameUID)) { continue; }
          //if (labItemSlotElement.ComponentRef.SimGameUID.StartsWith("FixedEquipment-")) { continue; }
          //if((labItemSlotElement.ComponentRef.ComponentDefType == ComponentType.AmmunitionBox) || (labItemSlotElement.ComponentRef.ComponentDefType == ComponentType.Weapon)) {
          if (labItemSlotElement.ComponentRef.isEditable()) {
            if (__state.TryGetValue(labItemSlotElement.ComponentRef.SimGameUID, out var damage)) {
              if (damage == ComponentDamageLevel.Destroyed) {
                labItemSlotElement.ComponentRef.DamageLevel = ComponentDamageLevel.Destroyed;
                Log.M?.WL(1, $"restore {labItemSlotElement.componentRef.ComponentDefID}:{labItemSlotElement.ComponentRef.ComponentDefType}:{labItemSlotElement.ComponentRef.DamageLevel}:{labItemSlotElement.ComponentRef.SimGameUID}");
              }
            }
          }
        }
      } catch (Exception e) {
        UIManager.logger.LogException(e);
        Log.M?.TWL(0, e.ToString());
      }
    }
  }
  [HarmonyPatch(typeof(MechLabLocationWidget), "StripDestroyedComponents")]
  [HarmonyBefore("io.github.denadan.CustomComponents")]
  internal static class MechLabLocationWidget_StripDestroyedComponents {
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static void Prefix(ref bool __runOriginal, MechLabLocationWidget __instance) {
      if (Core.Settings.VehcilesPartialEditable == false) { return; }
      if (__runOriginal == false) { return; }
      if (__instance.mechLab.Initialized == false) { return; }
      if (__instance.mechLab.activeMechDef.IsVehicle() == false) { return; }
      if (__instance.IsSimGame == false) { return; }
      try {
        Log.M?.TWL(0, $"StripDestroyedComponents {__instance.mechLab.activeMechDef.ChassisID} in {__instance.loadout.Location}");
        __runOriginal = false;
      } catch (Exception e) {
        UIManager.logger.LogException(e);
        Log.M?.TWL(0, e.ToString());
      }
    }
  }
  [HarmonyPatch(typeof(MechLabLocationWidget), "AutoRepairFixedEquipment")]
  [HarmonyBefore("io.github.denadan.CustomComponents")]
  internal static class MechLabLocationWidget_AutoRepairFixedEquipment {
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static void Prefix(ref bool __runOriginal, MechLabLocationWidget __instance) {
      if (Core.Settings.VehcilesPartialEditable == false) { return; }
      if (__runOriginal == false) { return; }
      if (__instance.mechLab.Initialized == false) { return; }
      if (__instance.mechLab.activeMechDef.IsVehicle() == false) { return; }
      try {
        Log.M?.TWL(0, $"AutoRepairFixedEquipment {__instance.mechLab.activeMechDef.ChassisID} in {__instance.loadout.Location}");
        foreach (var item in __instance.localInventory) {
          if (item.ComponentRef.DamageLevel == ComponentDamageLevel.Functional) { continue; }
          if (item.ComponentRef.DamageLevel == ComponentDamageLevel.Installing) { continue; }
          //if (string.IsNullOrEmpty(item.componentRef.SimGameUID)) { continue;  }
          //if (item.componentRef.SimGameUID.StartsWith("FixedEquipment-") == false) { continue; }
          if (item.ComponentRef.isEditable()) { continue; }
          if (item.ComponentRef.DamageLevel == ComponentDamageLevel.Destroyed) { item.ComponentRef.DamageLevel = ComponentDamageLevel.Penalized; }
          item.RepairComponent(true);
        }
        __runOriginal = false;
      } catch (Exception e) {
        UIManager.logger.LogException(e);
        Log.M?.TWL(0, e.ToString());
      }
    }
  }
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("StripMech")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(int), typeof(MechDef) })]
  public static class SimGameState_StripMech {
    public static void Prefix(ref bool __runOriginal, SimGameState __instance, int baySlot, MechDef def) {
      if (Core.Settings.VehcilesPartialEditable == false) { return; }
      if (!__runOriginal) { return; }
      try {
        if (def == null || baySlot > 0 && !__instance.ActiveMechs.ContainsKey(baySlot)) { return; }
        if (def.IsVehicle() == false) { return; }
        Log.M?.TWL(0, $"SimGameState.StripMech {def.ChassisID}");
        foreach (MechComponentRef mechComponentRef in def.Inventory) {
          Log.M?.WL(1, $"SimGameState.StripMech {mechComponentRef.ComponentDefID}:{mechComponentRef.ComponentDefType}:{mechComponentRef.DamageLevel}:{mechComponentRef.SimGameUID}");
          if (mechComponentRef.DamageLevel == ComponentDamageLevel.Destroyed) { continue; }
          //if ((string.IsNullOrEmpty(mechComponentRef.SimGameUID) == false) && (mechComponentRef.SimGameUID.StartsWith("FixedEquipment-"))) { continue; }
          //if (mechComponentRef.Def == null) { continue; }
          //if((mechComponentRef.ComponentDefType == ComponentType.AmmunitionBox) || (mechComponentRef.ComponentDefType == ComponentType.Weapon)) {
          if (mechComponentRef.isEditable()) {
            bool damaged = mechComponentRef.DamageLevel != ComponentDamageLevel.Functional && mechComponentRef.DamageLevel != ComponentDamageLevel.Installing;
            __instance.AddItemStat(mechComponentRef.ComponentDefID, SimGameState.GetTypeFromComponent(mechComponentRef.Def), damaged);
            Log.M?.WL(2, "to storage");
          }
        }
        if (!__instance.ActiveMechs.ContainsKey(baySlot)) { return; }
        __instance.ActiveMechs.Remove(baySlot);
      } catch (Exception e) {
        SimGameState.logger.LogException(e);
        Log.M?.TWL(0, e.ToString());
      }
      __runOriginal = false;
      return;
    }
  }
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("UnreadyMech")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyBefore("LewdableTanks")]
  [HarmonyPatch(new Type[] { typeof(int), typeof(MechDef) })]
  public static class SimGameState_UnreadyMech {
    public static void Prefix(ref bool __runOriginal, SimGameState __instance, int baySlot, MechDef def) {
      if (Core.Settings.VehcilesPartialEditable == false) { return; }
      if (!__runOriginal) { return; }
      try {
        if (def == null) { return; }
        if (def.IsVehicle() == false) { return; }
        __instance.StripMech(baySlot, def);
        __instance.AddItemStat(def.Chassis.Description.Id, def.GetType(), false);
        __runOriginal = false;
      } catch (Exception e) {
        SimGameState.logger.LogException(e);
        Log.M?.TWL(0, e.ToString());
      }
    }
  }
  public static class MechBayPanel_OnReadyMech_Reduced {
    public static void Prefix(ref bool __runOriginal, MechBayPanel __instance, MechBayChassisUnitElement chassisElement) {
      try {
        if (Core.Settings.VehcilesPartialEditable == false) { return; }
        if (chassisElement == null) { return; }
        if (chassisElement.ChassisDef == null) { return; }
        if (chassisElement.ChassisDef.IsVehicle() == false) { return; }
        __runOriginal = false;
        Log.M?.TWL(0, $" MechBayPanel.OnReadyMech {chassisElement.ChassisDef.Description.Id}");
        int firstFreeBaySlot = __instance.bayGroupWidget.GetFirstFreeBaySlot();
        if (firstFreeBaySlot < 0) {
          MechBayPanel.logger.LogWarning("Unable to get free unit bay slot to ready chassis " + chassisElement.ChassisDef.Description.Id);
        } else {
          __instance.Sim.ReadyMech(firstFreeBaySlot, chassisElement.ChassisDef.Description.Id);
          __instance.selectedChassis = null;
          __instance.RefreshData();
          __instance.ViewBays();
          __instance.SelectMech(__instance.bayGroupWidget.GetMechUnitForSlot(firstFreeBaySlot), true);
        }
      } catch (Exception e) {
        __runOriginal = true;
        MechBayPanel.logger.LogException(e);
        Log.M?.TWL(0, e.ToString());
      }
    }
  }
  [HarmonyPatch(typeof(SimGameState), "ReadyMech")]
  [HarmonyBefore("io.github.denadan.CustomComponents")]
  public static class SimGameState_ReadyMech_Patch {
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static void Prefix(ref bool __runOriginal, SimGameState __instance, int baySlot, string id) {
      if (Core.Settings.VehcilesPartialEditable == false) { return; }
      if (!__runOriginal) { return; }
      try {
        string origId = id;
        id = __instance.GetItemStatID(id, typeof(MechDef));
        Log.M?.TWL(0, $"SimGameState.ReadyMech {id}");
        string[] strArray = id.Split('.');
        if ((BattleTechResourceType)Enum.Parse(typeof(BattleTechResourceType), strArray[1]) != BattleTechResourceType.MechDef || !__instance.DataManager.Exists(BattleTechResourceType.ChassisDef, strArray[2])) {
          return;
        }
        ChassisDef chassis = __instance.DataManager.ChassisDefs.Get(strArray[2]);
        Log.M?.WL(1, $"chassis def found isVehicle:{chassis.IsVehicle()}");
        if (chassis.IsVehicle() == false) { return; }
        if (__instance.ScrapInactiveMech(origId, false) == false) { return; }
        string stockMechId = chassis.GetStockMechId();
        if (string.IsNullOrEmpty(stockMechId)) {
          stockMechId = chassis.Description.Id.Replace("vehiclechassisdef", "vehicledef");
        }
        Log.M?.WL(1, $"stockMechId {stockMechId}");
        if (__instance.DataManager.MechDefs.TryGet(stockMechId, out var stockMech) == false) { __runOriginal = false; return; }
        string simGameUid = __instance.GenerateSimGameUID();
        MechDef mech = new MechDef(chassis, simGameUid, stockMech);
        List<MechComponentRef> inventory = new List<MechComponentRef>();
        foreach (var stockitem in stockMech.Inventory) {
          MechComponentRef item = new MechComponentRef(stockitem);
          if (string.IsNullOrEmpty(item.SimGameUID)) { item.SimGameUID = __instance.GenerateSimGameUID(); }
          inventory.Add(item);
          //if (item.SimGameUID.StartsWith("FixedEquipment-")) { continue; }
          //if((item.ComponentDefType == ComponentType.AmmunitionBox) || (item.ComponentDefType == ComponentType.Weapon)) {
          if (item.isEditable()) {
            item.DamageLevel = ComponentDamageLevel.Destroyed;
          }
        }
        mech.SetInventory(inventory.ToArray());
        foreach (var item in mech.inventory) {
          Log.M?.WL(1, $"{item.ComponentDefID}:{item.MountedLocation}:{item.DamageLevel}:{item.SimGameUID}");
        }
        WorkOrderEntry_ReadyMech entry = new WorkOrderEntry_ReadyMech(string.Format("ReadyMech-{0}", simGameUid), Strings.T("Readying 'Vehicle - {0}", chassis.Description.Name), __instance.Constants.Story.MechReadyTime, baySlot, mech, Strings.T(__instance.Constants.Story.MechReadiedWorkOrderCompletedText, (object)chassis.Description.Name));
        __instance.MechLabQueue.Add(entry);
        __instance.ReadyingMechs[baySlot] = mech;
        __instance.RoomManager.AddWorkQueueEntry(entry);
        __instance.UpdateMechLabWorkQueue(false);
        AudioEventManager.PlayAudioEvent("audioeventdef_simgame_vo_barks", "workqueue_readymech", WwiseManager.GlobalAudioObject);
        __runOriginal = false;
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString());
        SimGameState.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(MechLabLocationWidget), "OnMechLabDrop")]
  [HarmonyBefore("io.github.denadan.CustomComponents")]
  public static class MechLabLocationWidget_OnAddItem {
    public static string CheckReplaceBox(this DataManager dataManager, MechComponentRef box1, MechComponentRef box2) {
      if (dataManager.AmmoBoxDefs.TryGet(box1.reducedInfo().originalDefinition, out var origbox) == false) { return $"DataManager error '{box1.reducedInfo().originalDefinition}'"; }
      AmmunitionBoxDef newbox = box2.Def as AmmunitionBoxDef;
      if (newbox == null) { return $"Not an ammunition box '{box2.ComponentDefID}'"; }
      if (origbox.InventorySize < newbox.InventorySize) {
        return $"This ammunition box can't fit, should be no more than {origbox.InventorySize} slots";
      }
      if (origbox.Tonnage < newbox.Tonnage) {
        return $"This ammunition box is too heavy, should be no more than {origbox.Tonnage} tons";
      }
      return string.Empty;
    }
    public static string CheckReplaceWeapon(this DataManager dataManager, MechComponentRef box1, MechComponentRef box2) {
      if (dataManager.WeaponDefs.TryGet(box1.reducedInfo().originalDefinition, out var origweapon) == false) { return $"DataManager error '{box1.reducedInfo().originalDefinition}'"; }
      Log.M?.TWL(0, $"CheckReplaceWeapon {box1.ComponentDefID}:{box1.SimGameUID} orig:{origweapon.Description.Id} replacing by {box2.ComponentDefID}:{box2.SimGameUID}");
      WeaponDef newweapon = box2.Def as WeaponDef;
      if (newweapon == null) { return $"Not a weapon '{box2.ComponentDefID}'"; }
      if (origweapon.InventorySize < newweapon.InventorySize) {
        return $"This weapon can't fit, should be no more than {origweapon.InventorySize} slots";
      }
      if (origweapon.WeaponCategoryValue != newweapon.WeaponCategoryValue) {
        return $"Weapon have wrong category, should be {origweapon.WeaponCategoryValue.FriendlyName}";
      }
      float origCarryLeftOver = box1.CarryLeftOver();
      float newCarryLeftOver = box2.CarryLeftOver();
      if(float.IsNaN(origCarryLeftOver) == false) {
        if (float.IsNaN(newCarryLeftOver)) {
          return $"This weapon can't be mounted this way";
        } else if (origCarryLeftOver < newCarryLeftOver) {
          return $"This weapon too heavy, should be no more than {origCarryLeftOver} tons";
        }
      } else
      if (origweapon.Tonnage < newweapon.Tonnage) {
        return $"This weapon too heavy, should be no more than {origweapon.Tonnage} tons";
      }
      if (origweapon.WeaponCategoryValue.IsEnergy) {
        if (origweapon.HeatGenerated < newweapon.HeatGenerated) {
          return $"Weapon is too hot, should generate no more than {origweapon.HeatGenerated} heat";
        }
      }
      return string.Empty;
    }
    public static void Prefix(MechLabLocationWidget __instance, ref bool __runOriginal, PointerEventData eventData) {
      try {
        if (Core.Settings.VehcilesPartialEditable == false) { return; }
        if (__runOriginal == false) { return; }
        if (__instance.mechLab.Initialized == false) { return; }
        if (__instance.mechLab.activeMechDef.IsVehicle() == false) { return; }
        string error = string.Empty;
        MechLabDropTargetType dropParentType = MechLabDropTargetType.NOT_SET;
        MechLabItemSlotElement dragItem = __instance.mechLab.DragItem as MechLabItemSlotElement;
        if (dragItem.DropParent != null) { dropParentType = dragItem.DropParent.dropTargetType; }
        Log.M?.TWL(0, $"MechLabLocationWidget.OnMechLabDrop {dragItem.ComponentRef.ComponentDefID}:{dragItem.ComponentRef.DamageLevel}:{dropParentType}");
        if (MechLabItemSlotElementHover.hoveredElement == null) {
          error = "Drag to component you want to replace";
          goto drop_error;
        }
        MechLabItemSlotElement hoverItem = MechLabItemSlotElementHover.hoveredElement;
        ChassisLocations location = __instance.loadout.Location;
        if (hoverItem.ComponentRef.Def.ComponentType != dragItem.ComponentRef.Def.ComponentType) {
          error = "Can't replace component of different type";
          goto drop_error;
        }
        //if ((string.IsNullOrEmpty(hoverItem.ComponentRef.SimGameUID) == false)&&(hoverItem.ComponentRef.SimGameUID.StartsWith("FixedEquipment-"))) {
        if (hoverItem.ComponentRef.isEditable() == false) {
          error = "Can't replace fixed equipment";
          goto drop_error;
        }
        ReducedComponentRefInfo rhoverInfo = hoverItem.ComponentRef.reducedInfo();
        ReducedComponentRefInfo rdragInfo = dragItem.ComponentRef.reducedInfo();
        if(dropParentType == MechLabDropTargetType.Dismount) {
          if(rhoverInfo.originalSimUID != rdragInfo.originalSimUID) {
            error = "You can only return dismounted component to place it originally was";
            goto drop_error;
          }
        }
        if (dragItem.ComponentRef.Def.ComponentType == ComponentType.AmmunitionBox) {
          error = CheckReplaceBox(__instance.mechLab.dataManager, hoverItem.ComponentRef, dragItem.ComponentRef);
          if (string.IsNullOrEmpty(error) == false) { goto drop_error; }
        } else if (dragItem.ComponentRef.Def.ComponentType == ComponentType.Weapon) {
          error = CheckReplaceWeapon(__instance.mechLab.dataManager, hoverItem.ComponentRef, dragItem.ComponentRef);
          if (string.IsNullOrEmpty(error) == false) { goto drop_error; }
        } else {
          error = "only ammunition boxes and weapons can be replaced";
          goto drop_error;
        }
        Log.M?.WL(1, $"hover {hoverItem.ComponentRef.ComponentDefID}:{hoverItem.ComponentRef.DamageLevel}");
        rdragInfo.originalSimUID = rhoverInfo.originalSimUID;
        rdragInfo.originalDefinition = rhoverInfo.originalDefinition;
        Log.M?.WL(1, $"{dragItem.ComponentRef.SimGameUID} now have orig:{dragItem.ComponentRef.reducedInfo().originalDefinition}:{dragItem.ComponentRef.reducedInfo().originalSimUID}");
        Queue<IChange> changeQueue = new Queue<IChange>();
        if (hoverItem.ComponentRef.DamageLevel == ComponentDamageLevel.Destroyed) {
          __instance.mechLab.activeMechInventory.Remove(hoverItem.ComponentRef);
          __instance.localInventory.Remove(hoverItem);
          List<MechComponentRef> inventory = __instance.mechLab.activeMechDef.Inventory.ToList();
          inventory.Remove(hoverItem.ComponentRef);
          __instance.mechLab.activeMechDef.SetInventory(inventory.ToArray());
          //WorkOrderEntry_InstallComponent_Constructor.IN_REPLACE_STATE = true;
          __instance.mechLab.OnRemoveItem(hoverItem, true);
          //WorkOrderEntry_InstallComponent_Constructor.IN_REPLACE_STATE = false;
          hoverItem.gameObject.transform.SetParent(null, false);
          __instance.mechLab.dataManager.PoolGameObject(MechLabPanel.MECHCOMPONENT_ITEM_PREFAB, hoverItem.gameObject);
        } else {
          MechLabLocationWidget_OnRemoveItem.IN_REPLACE_STATE = true;
          //WorkOrderEntry_InstallComponent_Constructor.IN_REPLACE_STATE = true;
          changeQueue.Enqueue(new Change_Remove_Reduced(hoverItem.componentRef, __instance.loadout.Location, false));
        }
        //CustomComponents.LocationHelper locationHelper = CustomComponents.MechLabHelper.CurrentMechLab.GetLocationHelper(__instance.loadout.Location);
        //Log.M?.WL(1, $"hover {hoverItem.ComponentRef.ComponentDefID}:{hoverItem.ComponentRef.DamageLevel} id:{hoverItem.GetInstanceID()}");
        //locationHelper.widget.OnRemoveItem(hoverItem, true);
        //dragItem.ComponentRef.SetSimGameUID(hoverItem.ComponentRef.SimGameUID);
        //CustomComponents.MechLabHelper.CurrentMechLab.MechLab.ForceItemDrop(hoverItem);
        //MechLabLocationWidget_OnRemoveItem.IN_REPLACE_STATE = false;
        changeQueue.Enqueue(new Change_Add_Reduced(dragItem, __instance.loadout.Location));
        CustomComponents.InventoryOperationState inventoryOperationState = new CustomComponents.InventoryOperationState(changeQueue, __instance.mechLab.activeMechDef);
        inventoryOperationState.DoChanges();
        inventoryOperationState.ApplyMechlab();
        MechLabLocationWidget_OnRemoveItem.IN_REPLACE_STATE = false;
        //CustomComponents.MechLabHelper.CurrentMechLab.RefreshHardpoints();
        //WorkOrderEntry_InstallComponent_Constructor.IN_REPLACE_STATE = false;
        __instance.mechLab.ClearDragItem(true);
        dragItem.SetSpacers();
        __instance.RefreshHardpointData();
        __instance.mechLab.ValidateLoadout(false);
        __runOriginal = false;
        return;
        drop_error:
        __instance.mechLab.ForceItemDrop(dragItem);
        __instance.mechLab.OnDrop(eventData);
        __instance.mechLab.ShowDropErrorMessage(new Localize.Text(error));
        __runOriginal = false;
        return;
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
        UIManager.logger.LogException(e);
        __runOriginal = true;
      }
    }
  }
  //[HarmonyPatch(typeof(MechLabItemSlotElement), "OnPointerEnter")]
  //public static class MechLabItemSlotElement_OnPointerEnter {
  //  public static MechLabItemSlotElement hoveredElement = null;
  //  public static void Postfix(MechLabItemSlotElement __instance) {
  //    try {
  //      Log.M?.TWL(0,$"MechLabItemSlotElement.OnPointerEnter {__instance.ComponentRef.ComponentDefID}");
  //      hoveredElement = __instance;
  //    } catch (Exception e) {
  //      Log.M?.TWL(0, e.ToString(), true);
  //      UIManager.logger.LogException(e);
  //    }
  //  }
  //}
  //[HarmonyPatch(typeof(MechLabItemSlotElement), "OnPointerExit")]
  //public static class MechLabItemSlotElement_OnPointerExit {
  //  public static void Postfix(MechLabItemSlotElement __instance) {
  //    try {
  //      if (MechLabItemSlotElement_OnPointerEnter.hoveredElement == __instance) {
  //        Log.M?.TWL(0, $"MechLabItemSlotElement.OnPointerExit {__instance.ComponentRef.ComponentDefID}");
  //        MechLabItemSlotElement_OnPointerEnter.hoveredElement = null;
  //      };
  //    } catch (Exception e) {
  //      Log.M?.TWL(0, e.ToString(), true);
  //      UIManager.logger.LogException(e);
  //    }
  //  }
  //}
  public class MechLabItemSlotElementHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    public static MechLabItemSlotElement hoveredElement = null;
    public MechLabItemSlotElement parent = null;
    public void OnPointerEnter(PointerEventData eventData) {
      try {
        Log.M?.TWL(0, $"MechLabItemSlotElement.OnPointerEnter {parent.ComponentRef.ComponentDefID}");
        hoveredElement = parent;
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
        UIManager.logger.LogException(e);
      }
    }
    public void OnPointerExit(PointerEventData eventData) {
      try {
        if (hoveredElement == parent) {
          Log.M?.TWL(0, $"MechLabItemSlotElement.OnPointerExit {parent.ComponentRef.ComponentDefID}");
          hoveredElement = null;
        };
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
        UIManager.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(MechLabPanel), "CreateMechComponentItem")]
  public static class MechLabPanel_CreateMechComponentItem {
    public static void Postfix(MechLabPanel __instance, ref MechLabItemSlotElement __result) {
      try {
        if (__result != null) {
          __result.enabled = true;
          MechLabItemSlotElementHover hover = __result.tooltip.gameObject.GetComponent<MechLabItemSlotElementHover>();
          if (hover == null) { hover = __result.tooltip.gameObject.AddComponent<MechLabItemSlotElementHover>(); }
          hover.parent = __result;
        }
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
        UIManager.logger.LogException(e);
      }
    }
  }

  //[HarmonyPatch(typeof(CustomComponents.Validator), "ValidateSize")]
  //public static class Validator_ValidateSize {
  //  public static void Postfix(MechLabItemSlotElement drop_item, List<CustomComponents.InvItem> new_inventory, ref string __result) {
  //    try {
  //      if (Core.Settings.VehcilesPartialEditable == false) { return; }
  //      var mechDef = CustomComponents.MechLabHelper.CurrentMechLab.ActiveMech;
  //      if (mechDef == null) { return; }
  //      if (mechDef.IsVehicle() == false) { return; }
  //      if (MechLabItemSlotElementHover.hoveredElement == null) {
  //        __result = "Drag to component you want to replace";
  //        return;
  //      }
  //      if (MechLabItemSlotElementHover.hoveredElement.ComponentRef.Def.ComponentType != drop_item.ComponentRef.Def.ComponentType) {
  //        __result = "Can't replace component of different type";
  //        return;
  //      }
  //      if (MechLabItemSlotElementHover.hoveredElement.ComponentRef.IsFixed) {
  //        __result = "Can't replace fixed equipment";
  //        return;
  //      }
  //      if (drop_item.ComponentRef.Def.ComponentType == ComponentType.AmmunitionBox) {
  //        __result = string.Empty;
  //        return;
  //      } else if (drop_item.ComponentRef.Def.ComponentType == ComponentType.Weapon) {
  //        __result = string.Empty;
  //        return;
  //      } else {
  //        __result = "only ammunition boxes and weapons can be replaced";
  //        return;
  //      }
  //    } catch (Exception e) {
  //      Log.M?.TWL(0, e.ToString(), true);
  //      UIManager.logger.LogException(e);
  //    }
  //  }
  //}

  public static class ChassisHandler_MakeMech {
    private static Type ChassisHandler = null;
    public static readonly string IN_MAKE_MECH_FLAGNAME = "CU_IN_MAKE_MECH";
    public static void Prepare() {
      foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
        if (assembly.FullName.StartsWith("CustomSalvage, Version=")) {
          ChassisHandler = assembly.GetType("CustomSalvage.ChassisHandler");
          if (ChassisHandler != null) {
            Core.HarmonyInstance.Patch(ChassisHandler.GetMethod("MakeMech", BindingFlags.Static | BindingFlags.NonPublic),
              new HarmonyMethod(typeof(ChassisHandler_MakeMech).GetMethod("Prefix", BindingFlags.Static | BindingFlags.Public)),
              new HarmonyMethod(typeof(ChassisHandler_MakeMech).GetMethod("Postfix", BindingFlags.Static | BindingFlags.Public))
              );
            break;
          }
        }
      }
    }
    public static void Prefix() {
      try {
        if (Core.Settings.VehcilesPartialEditable == false) { return; }
        Thread.CurrentThread.SetFlag(IN_MAKE_MECH_FLAGNAME);
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
        MechLabPanel.logger.LogException(e);
      }
    }
    public static void Postfix() {
      try {
        if (Core.Settings.VehcilesPartialEditable == false) { return; }
        if (Thread.CurrentThread.isFlagSet(IN_MAKE_MECH_FLAGNAME)) {
          Thread.CurrentThread.ClearFlag(IN_MAKE_MECH_FLAGNAME);
        }
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
        MechLabPanel.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(MechDef))]
  [HarmonyPatch(MethodType.Constructor)]
  [HarmonyPatch(new Type[] { typeof(MechDef), typeof(string), typeof(bool) })]
  public static class MechDef_Constructor {
    public static void Postfix(MechDef __instance, MechDef def, string newGUID, bool copyInventory) {
      try {
        if (Core.Settings.VehcilesPartialEditable == false) { return; }
        if (Thread.CurrentThread.isFlagSet(ChassisHandler_MakeMech.IN_MAKE_MECH_FLAGNAME) == false) { return; };
        if (__instance.IsVehicle() == false) { return; }
        Log.M?.TWL(0, $"MechDef.Constructor from MakeMech {__instance.ChassisID}");
        foreach (var item in __instance.Inventory) {
          //if ((string.IsNullOrEmpty(item.SimGameUID) == false) && (item.SimGameUID.StartsWith("FixedEquipment-"))) { continue; }
          //if((item.ComponentDefType == ComponentType.AmmunitionBox) || (item.ComponentDefType == ComponentType.Weapon)) {
          if (item.isEditable()) {
            item.DamageLevel = ComponentDamageLevel.Destroyed;
            Log.M?.WL(1, $"{item.ComponentDefID}:{item.ComponentDefType}:{item.DamageLevel}");
          }
        }
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
        MechLabPanel.logger.LogException(e);
      }
    }
  }

  public static class LewdableTanks_Patches_Contract_CompleteContract_Patch {
    public static Type LewdableTanks_Patches_Contract_CompleteContract = null;
    public static void Prepare() {
      foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
        if (assembly.FullName.StartsWith("LewdableTanks, Version=")) {
          Log.M?.TWL(0, $"LewdableTanks found {assembly.FullName}");
          LewdableTanks_Patches_Contract_CompleteContract = assembly.GetType("LewdableTanks.Patches.Contract_CompleteContract");
          if (LewdableTanks_Patches_Contract_CompleteContract != null) {
            Log.M?.WL(1, $"LewdableTanks.Patches.Contract_CompleteContract found");
            Core.HarmonyInstance.Patch(LewdableTanks_Patches_Contract_CompleteContract.GetMethod("Postfix", BindingFlags.Static | BindingFlags.Public),
              new HarmonyMethod(typeof(LewdableTanks_Patches_Contract_CompleteContract_Patch).GetMethod("Prefix", BindingFlags.Static | BindingFlags.Public))
              );
            break;
          }
        }
      }
    }
    public static void Prefix(ref bool __runOriginal) {
      if (Core.Settings.VehcilesPartialEditable) {
        Log.M?.TWL(0, "LewdableTanks.Patches.Contract_CompleteContract.Prefix preventing from launch");
        __runOriginal = false;
      }
    }
  }
  public static class MechEngineer_Features_CustomCapacities_CustomCapacitiesFeature {
    public static Type type = null;
    public static void Prepare() {
      foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
        if (assembly.FullName.StartsWith("MechEngineer, Version=")) {
          Log.M?.TWL(0, $"MechEngineer found {assembly.FullName}");
          type = assembly.GetType("MechEngineer.Features.CustomCapacities.CustomCapacitiesFeature");
          if (type != null) {
            Log.M?.WL(1, $"MechEngineer.Features.CustomCapacities.CustomCapacitiesFeature found");
            Core.HarmonyInstance.Patch(type.GetMethod("CalculateCustomCapacityResults", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public),
              new HarmonyMethod(typeof(MechEngineer_Features_CustomCapacities_CustomCapacitiesFeature).GetMethod("Postfix", BindingFlags.Static | BindingFlags.Public))
              );
            break;
          }
        }
      }
    }
    public static void Postfix(MechDef mechDef, ref bool show) {
      if (Core.Settings.VehcilesPartialEditable == false) { return; }
      if (mechDef.IsVehicle() == false) { return; }
      Log.M?.TWL(0, "MechEngineer.Features.CustomCapacities.CustomCapacitiesFeature.CalculateCustomCapacityResults hide");
      show = false;
    }
  }
  [HarmonyPatch(typeof(Contract))]
  [HarmonyPatch("CompleteContract")]
  [HarmonyBefore("LewdableTanks")]
  public static class Contract_CompleteContract_Partial {
    [HarmonyPostfix]
    [HarmonyWrapSafe]
    public static void Postfix(Contract __instance) {
      if (Core.Settings.VehcilesPartialEditable == false) { return; }
      foreach (MechDef mech in __instance.PlayerUnitResults.Where<UnitResult>((Func<UnitResult, bool>)(i => !i.mechLost)).Where<UnitResult>((Func<UnitResult, bool>)(i => i.mech.IsVehicle())).Select<UnitResult, MechDef>((Func<UnitResult, MechDef>)(i => i.mech))) {
        foreach (BaseComponentRef baseComponentRef in mech.Inventory) {
          if (baseComponentRef.DamageLevel != ComponentDamageLevel.Destroyed) { continue; }
          //if ((string.IsNullOrEmpty(baseComponentRef.SimGameUID) == false) && (baseComponentRef.SimGameUID.StartsWith("FixedEquipment-"))) {
          if (baseComponentRef.isEditable() == false) {
            baseComponentRef.DamageLevel = ComponentDamageLevel.Penalized;
            continue;
          }
          //if((baseComponentRef.ComponentDefType == ComponentType.AmmunitionBox) || (baseComponentRef.ComponentDefType == ComponentType.Weapon)) {
          //continue;
          //}
          //baseComponentRef.DamageLevel = ComponentDamageLevel.Penalized;
        }
      }
    }
  }
  //[HarmonyPatch(typeof(CustomComponents.InventoryOperationState), "ApplyInventory")]
  //public static class InventoryOperationState_ApplyInventory {
  //  public static void Prefix(CustomComponents.InventoryOperationState __instance) {
  //    try {
  //      Log.M?.TWL(0, $"InventoryOperationState.ApplyInventory prefix {__instance.Mech.ChassisID}");
  //    } catch (Exception e) {
  //      Log.M?.TWL(0, e.ToString(), true);
  //      UIManager.logger.LogException(e);
  //    }
  //  }
  //  public static void Postfix(CustomComponents.InventoryOperationState __instance) {
  //    try {

  //    } catch (Exception e) {
  //      Log.M?.TWL(0, e.ToString(), true);
  //      UIManager.logger.LogException(e);
  //    }
  //  }
  //}
  [HarmonyPatch(typeof(SimGameState), "ML_InstallComponent")]
  [HarmonyAfter("io.github.denadan.CustomComponents")]
  public static class SimGameState_ML_InstallComponent {
    //public static void Prefix(WorkOrderEntry_InstallComponent order, SimGameState __instance) {
    //  try {
    //    Log.M?.TWL(0, $"ML_InstallComponent prefix {order.MechComponentRef.ComponentDefID} PreviousLocation:{order.PreviousLocation}");
    //    //if (!order.IsMechLabComplete) { return; }
    //    MechDef mechById = __instance.GetMechByID(order.MechID);
    //    if (mechById == null) { return; }
    //    Log.M?.WL(1, $"{mechById.ChassisID}:{mechById.GetHashCode()} inventory: {mechById.Inventory.Length}");
    //    if (order.PreviousLocation == 0) { return; }
    //    if (mechById.IsVehicle() == false) { return; }
    //    foreach(var component in mechById.Inventory) {
    //      Log.M?.WL(2, $"{component.ComponentDefID}:{component.MountedLocation}:{component.SimGameUID} {component.DamageLevel}");
    //    }
    //  } catch (Exception e) {
    //    Log.M?.TWL(0, e.ToString(), true);
    //    UIManager.logger.LogException(e);
    //  }
    //}
    //public static void Prefix(WorkOrderEntry_InstallComponent order, SimGameState __instance) {
    //  try {
    //    Log.M?.TWL(0, $"ML_InstallComponent prefix {order.ID} {order.MechComponentRef.ComponentDefID}:{order.ComponentSimGameUID} PreviousLocation:{order.PreviousLocation}");
    //    if (order.IsMechLabComplete) { return; }
    //    MechDef mechById = __instance.GetMechByID(order.MechID);
    //    if (mechById == null) { return; }
    //    Log.M?.WL(1, $"{mechById.ChassisID}:{mechById.GetHashCode()} inventory: {mechById.Inventory.Length}");
    //    if (mechById.IsVehicle() == false) { return; }
    //    if (order.PreviousLocation == 0) {

    //      List<MechComponentRef> inventory = new List<MechComponentRef>();
    //      foreach (var item in mechById.Inventory) {
    //        if (item.SimGameUID != order.ComponentSimGameUID) { inventory.Add(item); continue; }
    //        Log.M?.WL(1, $"found component with same id: {item.ComponentDefID}:{item.DamageLevel}:{item.SimGameUID} removing");
    //      }
    //      mechById.SetInventory(inventory.ToArray());
    //    }
    //  } catch (Exception e) {
    //    Log.M?.TWL(0, e.ToString(), true);
    //    UIManager.logger.LogException(e);
    //  }
    //}
    public static bool CheckVehicleSimple(this MechDef mechDef) {
      if (Core.Settings.VehcilesPartialEditable == false) { return true; }
      if (mechDef.IsVehicle() == false) { return true; }
      Log.M?.TWL(0, $"CheckVehicleSimple {mechDef.ChassisID}");
      string stockMechId = mechDef.Chassis.GetStockMechId();
      Log.M?.WL(1, $"stockId:{stockMechId}");
      if (string.IsNullOrEmpty(stockMechId)) { return true; }
      if (UnityGameInstance.BattleTechGame.DataManager.MechDefs.TryGet(stockMechId, out var stockMechDef) == false) { return true; }
      Dictionary<ChassisLocations, List<MechComponentRef>> stockInvenotry = new Dictionary<ChassisLocations, List<MechComponentRef>>();
      Dictionary<ChassisLocations, List<MechComponentRef>> curInvenotry = new Dictionary<ChassisLocations, List<MechComponentRef>>();
      foreach (var compRef in stockMechDef.Inventory) {
        if (stockInvenotry.ContainsKey(compRef.MountedLocation) == false) { stockInvenotry.Add(compRef.MountedLocation, new List<MechComponentRef>()); }
        stockInvenotry[compRef.MountedLocation].Add(compRef);
      }
      foreach (var compRef in mechDef.Inventory) {
        if (curInvenotry.ContainsKey(compRef.MountedLocation) == false) { curInvenotry.Add(compRef.MountedLocation, new List<MechComponentRef>()); }
        curInvenotry[compRef.MountedLocation].Add(compRef);
      }
      foreach (var location in stockInvenotry) {
        List<MechComponentRef> stockAmmoBoxes = new List<MechComponentRef>();
        foreach(var compRef in location.Value) {
          if (compRef.isEditable() == false) { continue; }
          if (compRef.ComponentDefType != ComponentType.AmmunitionBox) { continue; }
          stockAmmoBoxes.Add(compRef);
        }
        if (curInvenotry.TryGetValue(location.Key, out var curloc)) {
          List<MechComponentRef> curAmmoBoxes = new List<MechComponentRef>();
          foreach (var compRef in curloc) {
            if (compRef.isEditable() == false) { continue; }
            if (compRef.ComponentDefType != ComponentType.AmmunitionBox) { continue; }
            curAmmoBoxes.Add(compRef);
          }
          Log.M?.WL(1, $"location:{location.Key.toFakeVehicleChassis()} stockboxes:{stockAmmoBoxes.Count} curboxes:{curAmmoBoxes.Count}");
          if (curAmmoBoxes.Count != stockAmmoBoxes.Count) { return false; }
        }
      }
      foreach (var location in stockInvenotry) {
        Dictionary<WeaponCategoryValue, List<MechComponentRef>> stockWeapons = new Dictionary<WeaponCategoryValue, List<MechComponentRef>>();
        foreach (var compRef in location.Value) {
          if (compRef.isEditable() == false) { continue; }
          if (compRef.ComponentDefType != ComponentType.Weapon) { continue; }
          if (stockWeapons.ContainsKey((compRef.Def as WeaponDef).WeaponCategoryValue) == false) { stockWeapons.Add((compRef.Def as WeaponDef).WeaponCategoryValue, new List<MechComponentRef>()); }
          stockWeapons[(compRef.Def as WeaponDef).WeaponCategoryValue].Add(compRef);
        }
        if (curInvenotry.TryGetValue(location.Key, out var curloc)) {
          Dictionary<WeaponCategoryValue, List<MechComponentRef>> curWeapons = new Dictionary<WeaponCategoryValue, List<MechComponentRef>>();
          foreach (var compRef in curloc) {
            if (compRef.isEditable() == false) { continue; }
            if (compRef.ComponentDefType != ComponentType.Weapon) { continue; }
            if (curWeapons.ContainsKey((compRef.Def as WeaponDef).WeaponCategoryValue) == false) { curWeapons.Add((compRef.Def as WeaponDef).WeaponCategoryValue, new List<MechComponentRef>()); }
            curWeapons[(compRef.Def as WeaponDef).WeaponCategoryValue].Add(compRef);
            Log.M?.WL(1, $"{compRef.MountedLocation.toFakeVehicleChassis()} {compRef.ComponentDefID}:{compRef.ComponentDefType}:{compRef.SimGameUID} {curWeapons[(compRef.Def as WeaponDef).WeaponCategoryValue].Count}");
          }
          foreach (var stockWeapon in stockWeapons) {
            if (curWeapons.ContainsKey(stockWeapon.Key) == false) { return false; }
            Log.M?.WL(1, $"location:{location.Key.toFakeVehicleChassis()} category:{stockWeapon.Key} stockweapons:{stockWeapon.Value.Count} curweapons:{curWeapons[stockWeapon.Key].Count}");
            if (curWeapons[stockWeapon.Key].Count != stockWeapon.Value.Count) { return false; }
          }          
        }
      }
      return true;
    }
    public static void RevertToStock(this MechDef mechDef, SimGameState sim) {
      if (Core.Settings.VehcilesPartialEditable == false) { return; }
      if (mechDef.IsVehicle() == false) { return; }
      string stockMechId = mechDef.Chassis.GetStockMechId();
      if (string.IsNullOrEmpty(stockMechId)) { return; }
      if (UnityGameInstance.BattleTechGame.DataManager.MechDefs.TryGet(stockMechId, out var stockMechDef) == false) { return; }
      foreach(var compRef in mechDef.Inventory) {
        if (compRef.DamageLevel == ComponentDamageLevel.Destroyed) { continue; }
        if (compRef.isEditable() == false) { continue; }
        if (sim != null) {
          sim.AddItemStat(compRef.ComponentDefID, SimGameState.GetTypeFromComponent(compRef.ComponentDefType), compRef.DamageLevel != ComponentDamageLevel.Functional && compRef.DamageLevel != ComponentDamageLevel.Installing);
        }
      }
      List<MechComponentRef> inventory = new List<MechComponentRef>();
      foreach (var compRef in stockMechDef.Inventory) {
        var item = new MechComponentRef(compRef);
        item.SimGameUID = sim.GenerateSimGameUID();
        if (item.isEditable()) {
          item.DamageLevel = ComponentDamageLevel.Destroyed;
        }
        inventory.Add(item);
      }
      mechDef.SetInventory(inventory.ToArray());
    }
    public static void Postfix(WorkOrderEntry_InstallComponent order, SimGameState __instance) {
      try {
        if (Core.Settings.VehcilesPartialEditable == false) { return; }
        Log.M?.TWL(0, $"ML_InstallComponent postfix {order.ID} {order.MechComponentRef.ComponentDefID}:{order.ComponentSimGameUID} PreviousLocation:{order.PreviousLocation}");
        if (!order.IsMechLabComplete) { return; }
        MechDef mechById = __instance.GetMechByID(order.MechID);
        if (mechById == null) { return; }
        Log.M?.WL(1, $"{mechById.ChassisID}:{mechById.GetHashCode()} inventory: {mechById.Inventory.Length}");
        if (mechById.IsVehicle() == false) { return; }
        if (order.PreviousLocation != 0) {
          if (order.MechComponentRef.DamageLevel == ComponentDamageLevel.Destroyed) { return; }
          MechComponentRef removedRef = new MechComponentRef(order.MechComponentRef);
          removedRef.MountedLocation = order.PreviousLocation;
          removedRef.DamageLevel = ComponentDamageLevel.Destroyed;
          removedRef.IsFixed = false;
          List<MechComponentRef> inventory = mechById.Inventory.ToList();
          inventory.Add(removedRef);

          mechById.SetInventory(inventory.ToArray());
          Log.M?.WL(1, $"add component in damaged state {removedRef.ComponentDefID} inventory: {mechById.Inventory.Length}");
          foreach (var component in mechById.Inventory) {
            Log.M?.WL(2, $"{component.ComponentDefID}:{component.MountedLocation}:{component.SimGameUID} {component.DamageLevel}");
          }
        } else {
          var info = ReducedComponentRefInfoHelper.reducedInfo(order.ComponentSimGameUID);
          if (info == null) {
            Log.M?.WL(1, $"reduced info not found");
            return;
          }
          Log.M?.WL(1, $"reduced info found. remove {info.originalSimUID}");
          List<MechComponentRef> inventory = mechById.Inventory.ToList();
          MechComponentRef removeRef = inventory.FirstOrDefault((x) => { return x.SimGameUID == info.originalSimUID; });
          if (removeRef == null) {
            Log.M?.WL(1, $"component not found");
            return;
          } else {
            Log.M?.WL(1, $"component found");
            inventory.Remove(removeRef);
          }
          info.originalSimUID = order.ComponentSimGameUID;
          mechById.SetInventory(inventory.ToArray());
          foreach (var component in mechById.Inventory) {
            Log.M?.WL(2, $"{component.ComponentDefID}:{component.MountedLocation}:{component.SimGameUID} {component.DamageLevel}");
          }
        }
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
        UIManager.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(WorkOrderEntry_InstallComponent))]
  [HarmonyPatch(MethodType.Constructor)]
  [HarmonyPatch(new Type[] { typeof(string), typeof(string), typeof(string), typeof(MechComponentRef), typeof(int), typeof(string), typeof(ChassisLocations), typeof(ChassisLocations), typeof(int), typeof(int), typeof(string) })]
  public static class WorkOrderEntry_InstallComponent_Constructor {
    public static void Postfix(WorkOrderEntry_InstallComponent __instance, string ID, string Description, string mechSimGameUID, MechComponentRef component, int techCost, string componentSimGameUID, ChassisLocations newLocation, ChassisLocations oldLocation, int hardpointSlot, int cbillCost, string ToastDescription) {
      try {
        var sim = UnityGameInstance.BattleTechGame.Simulation;
        if (sim == null) { return; }
        MechDef mechById = sim.GetMechByID(mechSimGameUID);
        if (mechById == null) { return; }
        if (mechById.IsVehicle() == false) { return; }
        Log.M?.TWL(0, $"WorkOrderEntry_InstallComponent.constructor  {component.ComponentDefID}:{component.SimGameUID}");
        string statename = component.DamageLevel == ComponentDamageLevel.Destroyed? " Destroyed": "";
        string typename = newLocation != ChassisLocations.None ? Strings.T("Install") : Strings.T("Remove");
        string locationname = newLocation != ChassisLocations.None ? newLocation.toFakeVehicleChassis().ToString() : "";
        __instance.Description = Strings.T("{0}{1} {2} Component - {3}", typename, statename, locationname, component.Def.Description.Name);
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
        UIManager.logger.LogException(e);
      }
    }
  }
  //[HarmonyPatch(typeof(CustomComponents.Changes.Change_Remove), "ApplyToInventory")]
  //public static class Change_Remove_ApplyToInventory {
  //  public static void Prefix(CustomComponents.Changes.Change_Remove __instance, MechDef mech, List<MechComponentRef> inventory) {
  //    try {
  //      Log.M?.TWL(0, $"Change_Remove.ApplyToInventory prefix {mech.ChassisID} {__instance}");
  //      MechComponentRef mechComponentRef = inventory.FirstOrDefault<MechComponentRef>((Func<MechComponentRef, bool>)(i => i.MountedLocation == __instance.Location && i.ComponentDefID == __instance.ItemID));
  //      if (mechComponentRef != null) {
  //        Log.M?.WL(1, $"found {mechComponentRef.ComponentDefID}:{mechComponentRef.DamageLevel}");
  //      }
  //    } catch (Exception e) {
  //      Log.M?.TWL(0, e.ToString(), true);
  //      UIManager.logger.LogException(e);
  //    }
  //  }
  //  public static void Postfix(CustomComponents.InventoryOperationState __instance) {
  //    try {
  //    } catch (Exception e) {
  //      Log.M?.TWL(0, e.ToString(), true);
  //      UIManager.logger.LogException(e);
  //    }
  //  }
  //}

  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("GetLongChassisLocation")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ChassisLocations) })]
  public static class Mech_GetLongChassisLocation {
    public static readonly string IN_MECHLAB_INIT_FLAG = "CU_IN_MECHLAB";
    public static void PushMechLab(this MechDef mechDef) {
      Thread.CurrentThread.SetFlag(IN_MECHLAB_INIT_FLAG);
      Thread.CurrentThread.pushActorDef(mechDef);
    }
    public static void PopMechLab(this MechDef mechDef) {
      Thread.CurrentThread.clearActorDef();
      Thread.CurrentThread.ClearFlag(IN_MECHLAB_INIT_FLAG);
    }
    public static void Postfix(ChassisLocations location, ref Localize.Text __result) {
      try {
        if (Thread.CurrentThread.isFlagSet(IN_MECHLAB_INIT_FLAG)) {
          MechDef mechDef = Thread.CurrentThread.currentMechDef();
          if (mechDef != null) {
            if (mechDef.IsVehicle()) {
              __result = new Localize.Text(Vehicle.GetLongChassisLocation(location.toVehicleLocation()));
            }
          }
        }
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
        UIManager.logger.LogException(e);
      }
    }
  }
  public class AutoDisable : MonoBehaviour {
    public bool autoDisable = false;
    public void Update() {
      if (autoDisable) { this.gameObject.SetActive(false); }
    }
  }
  public class RedusedMechLabMechInfoWidget : MonoBehaviour {
    public LocalizableText totalTonnage = null;
    public LocalizableText remainingTonnage = null;
    public UIColorRefTracker totalTonnageColor = null;
    public UIColorRefTracker remainingTonnageColor = null;
    public GameObject layout_tonnage = null;
    public GameObject custom_capacities = null;
    //public AutoDisable custom_capacities_autodisable = null;
    public GameObject layout_hardpoints = null;
    public GameObject redused_tonnage = null;
    public MechLabMechInfoWidget src = null;
    public bool inited = false;
    public string tonnageText = string.Empty;
    public GameObject originalModified = null;
    public GameObject redusedModified = null;
    public static RedusedMechLabMechInfoWidget Instantine(MechLabMechInfoWidget src) {
      RedusedMechLabMechInfoWidget result = src.gameObject.AddComponent<RedusedMechLabMechInfoWidget>();
      result.src = src;
      result.layout_tonnage = src.remainingTonnage.gameObject.transform.parent.gameObject;
      result.layout_hardpoints = src.hardpoints[0].gameObject.transform.parent.gameObject;
      var custom_capacities = src.gameObject.FindComponent<Image>("custom_capacities");
      if (custom_capacities != null) { result.custom_capacities = custom_capacities.gameObject; }
      //if(result.custom_capacities != null) {
      //  result.custom_capacities_autodisable = result.custom_capacities.GetComponent<AutoDisable>();
      //  if(result.custom_capacities_autodisable == null) {
      //    result.custom_capacities_autodisable = result.custom_capacities.AddComponent<AutoDisable>();
      //  }
      //}
      result.originalModified = src.mechLab.modifiedIcon;
      result.redused_tonnage = GameObject.Instantiate(result.layout_tonnage);
      result.redusedModified = result.redused_tonnage.FindComponent<Image>("MODIFIED").gameObject;
      result.redused_tonnage.name = "redused_tonnage";
      result.redused_tonnage.transform.SetParent(result.layout_tonnage.transform.parent);
      //result.redused_tonnage.transform.SetAsFirstSibling();
      result.redused_tonnage.transform.localScale = Vector3.one;
      result.remainingTonnage = result.redused_tonnage.FindComponent<LocalizableText>("txt_tonsRemaining");
      result.remainingTonnageColor = result.remainingTonnage.gameObject.GetComponent<UIColorRefTracker>();
      result.totalTonnage = result.redused_tonnage.FindComponent<LocalizableText>("txt_tonsNumber");
      result.totalTonnageColor = result.totalTonnage.gameObject.GetComponent<UIColorRefTracker>();
      result.redused_tonnage.SetActive(false);
      return result;
    }
    public float T = 0f;
    public void Sanitize() {
      T = 1f;
      try {
        var locations = src.mechLab.gameObject.GetComponentsInChildren<MechLabLocationWidget>(true);
        foreach (var location in locations) {
          for (int t = 0; t < location.inventoryParent.childCount; ++t) {
            Transform invTr = location.inventoryParent.GetChild(t);
            if (invTr == null) { continue; }
            MechLabItemSlotElement slot = invTr.gameObject.GetComponent<MechLabItemSlotElement>();
            if (slot == null) {
              UIManager.logger.LogError($"{location.loadout.Location} have item {invTr.name} without MechLabItemSlotElement. Should not be there!");
              continue;
            }
            if (slot.enabled == false) {
              UIManager.logger.LogError($"{location.loadout.Location} someone had disabled MechLabItemSlotElement {invTr.name}");
              slot.enabled = true;
            }
            MechLabItemSlotElementHover hover = slot.tooltip.gameObject.GetComponent<MechLabItemSlotElementHover>();
            if (hover == null) { hover = slot.tooltip.gameObject.AddComponent<MechLabItemSlotElementHover>(); }
            hover.parent = slot;
            if (hover.enabled == false) {
              UIManager.logger.LogError($"{location.loadout.Location} someone had disabled MechLabItemSlotElementHover {invTr.name}");
              hover.enabled = true;
            }
            //hover.enabled = true;
          }
          //var tooltips
        }
      } catch (Exception e) {
        UIManager.logger.LogException(e);
      }
    }
    public void Update() {
      if (T <= 0f) {
        this.Sanitize();
      } else {
        T -= Time.deltaTime;
      }
      //if (inited) { return; }
      //this.totalTonnage.text = this.tonnageText;
      //this.remainingTonnage.text = $"---";
      //this.inited = true;
    }
    public void Init(MechDef mechDef) {
      if (this.custom_capacities == null) {
        var custom_capacities = src.gameObject.FindComponent<Image>("custom_capacities");
        if (custom_capacities != null) { this.custom_capacities = custom_capacities.gameObject; }
      }
      if (mechDef.IsVehicle()) {
        this.custom_capacities?.SetActive(false);
        //if(custom_capacities_autodisable != null) this.custom_capacities_autodisable.autoDisable = true;
        this.layout_hardpoints.SetActive(false);
        this.layout_tonnage.SetActive(false);
        this.redused_tonnage.SetActive(true);
        this.tonnageText = $"{mechDef.Chassis.Tonnage}";
        this.totalTonnage.text = $"{mechDef.Chassis.Tonnage}";
        this.remainingTonnage.text = $"---";
        this.inited = false;
        if (this.src.mechLab.modifiedIcon != this.redusedModified) {
          this.redusedModified.SetActive(this.src.mechLab.modifiedIcon.activeSelf);
          this.src.mechLab.modifiedIcon = this.redusedModified;
        }
      } else {
        //if (custom_capacities_autodisable != null) this.custom_capacities_autodisable.autoDisable = false;
        this.custom_capacities?.SetActive(true);
        this.layout_hardpoints.SetActive(true);
        this.layout_tonnage.SetActive(true);
        this.redused_tonnage.SetActive(false);
        if (this.src.mechLab.modifiedIcon != this.originalModified) {
          this.originalModified.SetActive(this.src.mechLab.modifiedIcon.activeSelf);
          this.src.mechLab.modifiedIcon = this.originalModified;
        }
        this.inited = false;
      }
      this.Sanitize();
    }
  }
  //[HarmonyPatch(typeof(MechLabMechInfoWidget))]
  //[HarmonyPatch("CalculateTonnage")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyBefore("MechEngineer.Features.OverrideTonnage")]
  //public static class MechLabMechInfoWidget_CalculateTonnage {
  //  public static void Prefix(ref bool __runOriginal, MechLabMechInfoWidget __instance) {
  //    try {
  //      if (__runOriginal == false) { return; }
  //      if (__instance.mechLab.originalMechDef == null) { return; }
  //      if (__instance.mechLab.originalMechDef.IsVehicle() == false) { return; }
  //      __runOriginal = false;
  //      Log.M?.TWL(0,$"MechLabMechInfoWidget.CalculateTonnage {__instance.mechLab.originalMechDef.ChassisID} vehicle:{__instance.mechLab.originalMechDef.IsVehicle()}");
  //      __instance.currentTonnage = __instance.mechLab.originalMechDef.Chassis.Tonnage;
  //      __instance.totalTonnage.text = $"{__instance.mechLab.originalMechDef.Chassis.Tonnage}";
  //      __instance.remainingTonnage.text = "-----";
  //      __instance.remainingTonnageColor.SetUIColor(UIColor.White);
  //      __instance.totalTonnageColor.SetUIColor(UIColor.WhiteHalf);
  //      HBSTooltip tooltip = __instance.remainingTonnage.transform.parent.gameObject.GetComponent<HBSTooltip>();
  //      if (tooltip != null) {
  //        tooltip.SetDefaultStateData("TONNAGE".GetTooltipStateData());
  //      }
  //    } catch (Exception e) {
  //      Log.M?.TWL(0, e.ToString(), true);
  //      UIManager.logger.LogException(e);
  //    }
  //  }
  //}

}