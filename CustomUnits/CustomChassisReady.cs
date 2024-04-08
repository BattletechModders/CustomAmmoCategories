/*  
 *  This file is part of CustomUnits.
 *  CustomUnits is free software: you can redistribute it and/or modify it under the terms of the 
 *  GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, 
 *  or (at your option) any later version. CustomAmmoCategories is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 *  See the GNU Lesser General Public License for more details.
 *  You should have received a copy of the GNU Lesser General Public License along with CustomAmmoCategories. 
 *  If not, see <https://www.gnu.org/licenses/>. 
*/
using BattleTech;
using BattleTech.Data;
using BattleTech.Save;
using BattleTech.Save.Core;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using CustomComponents;
using CustomSalvage;
using HarmonyLib;
using IRBTModUtils;
using Localize;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUnits {
  public static class SaveLayoutHelper {
    public static string BaseDirectory { get; set; }
    public static void Init(string dir) {
      BaseDirectory = Path.Combine(dir,"user_layouts");
      if (Directory.Exists(BaseDirectory) == false) { Directory.CreateDirectory(BaseDirectory); }
    }
    public static void Save(SavedLoadout layout) {
      try {
        string path = Path.Combine(BaseDirectory, layout.ChassisId + "_" + layout.Name + ".json");
        File.WriteAllText(path, JsonConvert.SerializeObject(layout, Formatting.Indented));
      }catch(Exception e) {
        Log.M?.TWL(0,e.ToString(),true);
        UnityGameInstance.logger.LogException(e);
      }
    }
    public static List<SavedLoadout> Load(string ChassisId) {
      List<SavedLoadout> result = new List<SavedLoadout>();
      try {
        string[] files = Directory.GetFiles(BaseDirectory, ChassisId + "_*.json", SearchOption.TopDirectoryOnly);
        foreach (string f in files) {
          try {
            result.Add(JsonConvert.DeserializeObject<SavedLoadout>(File.ReadAllText(f)));
          } catch (Exception e) {
            Log.M?.TWL(0, e.ToString(), true);
          }
        }
      }catch(Exception e) {
        Log.E?.TWL(0, e.ToString(), true);
        UIManager.logger.LogException(e);
      }
      return result;
    }
    public static List<SavedLoadout> LoadAll(HashSet<string> chassisIds) {
      List<SavedLoadout> result = new List<SavedLoadout>();
      foreach(string chassisId in chassisIds) {
        result.AddRange(Load(chassisId));
      }
      return result;
    }
  }
  public class SavedInventoryItem {
    public string ComponentID { get; set; }
    public ComponentType componentType { get; set; }
    public ChassisLocations location { get; set; }
    public SavedInventoryItem(string id, ComponentType cType, ChassisLocations loc) {
      this.ComponentID = id;
      this.location = loc;
      this.componentType = cType;
    }
  }
  public class SavedLoadout {
    public string ChassisId { get; set; }
    public string Name { get; set; }
    public List<SavedInventoryItem> inventory { get; set; } = new List<SavedInventoryItem>();
    public void FillFromMech(string name, MechDef mechDef) {
      this.Name = name;
      this.ChassisId = mechDef.ChassisID;
      inventory.Clear();
      foreach (MechComponentRef componentRef in mechDef.Inventory) {
        if (componentRef == null) { continue; }
        if (componentRef.IsFixed) { continue; }
        if (componentRef.Def == null) { continue; }
        if (componentRef.DamageLevel < ComponentDamageLevel.Functional) { continue; }
        inventory.Add(new SavedInventoryItem(componentRef.ComponentDefID, componentRef.ComponentDefType, componentRef.MountedLocation));
      }
    }
  }
  public class SavedLoadouts {
    public Dictionary<string,SavedLoadout> loadouts { get; set; } = new Dictionary<string, SavedLoadout>();
  }
  public class MechLabPanelFillAs: MonoBehaviour {
    public static MechLabPanelFillAs Instance = null;
    public static readonly string INVENTORY_POPULATE_FLAG = "ML_INVENTORY_POPULATE";
    public static readonly string SAVED_LOADOUTS_STATISTIC= "CU_SAVED_LOADOUTS";
    public MechLabPanel mechLabPanel { get; set; } = null;
    public GameObject restoreButtonObj { get; set; } = null;
    public GameObject saveButtonObj { get; set; } = null;
    public HBSDOTweenButton restoreButton { get; set; } = null;
    public HBSDOTweenButton saveButton { get; set; } = null;
    private static MethodInfo get_RawInventoryPerfFix;
    private static MethodInfo get_GetRawInventoryCustomFilter;
    public static void InitMechLabInventoryAccess() {
      foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
        if(assembly.FullName.StartsWith("BattletechPerformanceFix, Version=")) {
          Type MechLabFixPublic = assembly.GetType("BattletechPerformanceFix.MechlabFix.MechLabFixPublic");
          if (MechLabFixPublic != null) {
            Log.M?.TWL(0, $"{MechLabFixPublic.FullName} found");
            PropertyInfo prop = AccessTools.Property(MechLabFixPublic, "RawInventory");
            if(prop != null) {
              Log.M?.WL(1, $"{prop.Name} found");
              get_RawInventoryPerfFix = prop.GetMethod;
            } else {
              Log.M?.WL(1, $"BattletechPerformanceFix.MechlabFix.MechLabFixPublic.RawInventory not found");
            }
          } else {
            Log.M?.TWL(0, $"BattletechPerformanceFix.MechlabFix.MechLabFixPublic not found");
          }
        }
        if(assembly.FullName.StartsWith("CustomFilters, Version=")) {
          Type MechLabFixPublic = assembly.GetType("CustomFilters.MechLabScrolling.MechLabFixPublic");
          if (MechLabFixPublic != null) {
            Log.M?.TWL(0, $"{MechLabFixPublic.FullName} found");
            get_GetRawInventoryCustomFilter = AccessTools.Method(MechLabFixPublic, "GetRawInventory");
            if (get_GetRawInventoryCustomFilter != null) {
              Log.M?.WL(1, $"{get_GetRawInventoryCustomFilter.Name} found");
            } else {
              Log.M?.WL(1, $"CustomFilters.MechLabScrolling.MechLabFixPublic.GetRawInventory not found");
            }
          } else {
            Log.M?.TWL(0, $"CustomFilters.MechLabScrolling.MechLabFixPublic not found");
          }
        }
      }
    }
    public HashSet<ListElementController_BASE_NotListView> inventory { get; set; } = new HashSet<ListElementController_BASE_NotListView>();
    public void Clear() {
      inventory.Clear();
    }
    public HashSet<string> gatherCompatibleChassis(ChassisDef chassis) {
      HashSet<string> result = new HashSet<string>();
      result.Add(chassis.Description.Id);
      AssemblyVariant baseAssemblyVariant = chassis.GetComponent<AssemblyVariant>();
      if ((baseAssemblyVariant != null)&&(baseAssemblyVariant.Exclude == false)&&(baseAssemblyVariant.Include == true)) {
        foreach (var dmChassis in this.mechLabPanel.dataManager.ChassisDefs) {
          AssemblyVariant assemblyVariant = dmChassis.Value.GetComponent<AssemblyVariant>();
          if (assemblyVariant == null) { continue; }
          if (assemblyVariant.Exclude) { continue; }
          if (assemblyVariant.Include == false) { continue; }
          if (assemblyVariant.PrefabID != baseAssemblyVariant.PrefabID) { continue; }
          if (dmChassis.Value.Tonnage != chassis.Tonnage) { continue; }
          result.Add(dmChassis.Value.Description.Id);
        }
      }
      return result;
    }
    public List<SavedInventoryItem> GetSavedInventoryItems(MechDef mechDef) {
      List<SavedInventoryItem> inventory = new List<SavedInventoryItem>();
      foreach (MechComponentRef componentRef in mechDef.Inventory) {
        if (componentRef == null) { continue; }
        if (componentRef.IsFixed) { continue; }
        if (componentRef.Def == null) { continue; }
        if (componentRef.DamageLevel < ComponentDamageLevel.Functional) { continue; }
        inventory.Add(new SavedInventoryItem(componentRef.ComponentDefID, componentRef.ComponentDefType, componentRef.MountedLocation));
      }
      return inventory;
    }
    public MechComponentDef GetComponentDef(string id, ComponentType compType) {
      try {
        switch (compType) {
          case ComponentType.AmmunitionBox: return this.mechLabPanel.dataManager.AmmoBoxDefs.Get(id);
          case ComponentType.HeatSink: return this.mechLabPanel.dataManager.HeatSinkDefs.Get(id);
          case ComponentType.JumpJet: return this.mechLabPanel.dataManager.JumpJetDefs.Get(id);
          case ComponentType.Upgrade: return this.mechLabPanel.dataManager.UpgradeDefs.Get(id);
          case ComponentType.Weapon: return this.mechLabPanel.dataManager.WeaponDefs.Get(id);
          default: return null;
        }
      } catch(Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
      }
      return null;
    }
    public Dictionary<string, List<SavedInventoryItem>> gatherCompatibleInventories(ChassisDef chassis) {
      Dictionary<string, List<SavedInventoryItem>> result = new Dictionary<string, List<SavedInventoryItem>>();
      HashSet<string> chassisIDs = this.gatherCompatibleChassis(chassis);
      foreach(var mech in this.mechLabPanel.dataManager.MechDefs) {
        if (chassisIDs.Contains(mech.Value.ChassisID) == false) { continue; }
        string name = mech.Value.Description.UIName;
        if (string.IsNullOrEmpty(name)) { name = mech.Value.Name; }
        name = "Stock " + name;
        List<SavedInventoryItem> inventory = this.GetSavedInventoryItems(mech.Value);
        string temp_name = name;
        int counter = 0;
        while (counter < 10) {
          if (result.ContainsKey(temp_name) == false) { result.Add(temp_name, inventory); break; }
          ++counter;
          temp_name = name + " V" + counter;
        }
      }
      List<SavedLoadout> userLayouts = SaveLayoutHelper.LoadAll(chassisIDs);
      foreach (SavedLoadout userConfig in userLayouts) {
        if(this.mechLabPanel.dataManager.ChassisDefs.TryGet(userConfig.ChassisId, out ChassisDef confChassis)) {
          string name = confChassis.VariantName + " " + userConfig.Name;
          string temp_name = name;
          int counter = 0;
          while (counter < 10) {
            if (result.ContainsKey(temp_name) == false) { result.Add(temp_name, new List<SavedInventoryItem>(userConfig.inventory)); break; }
            ++counter;
            temp_name = name + " V" + counter;
          }
        }
      }
      return result;
    }
    public bool CheckWidgetNonStripped(MechLabLocationWidget widget, string name) {
      try {
        List<MechLabItemSlotElement> localInventory = widget.localInventory;
        foreach (MechLabItemSlotElement item in localInventory) {
          if (item.ComponentRef.IsFixed == false) {
            return false;
          }
        }
        return true;
      }catch(Exception e) {
        Log.Combat?.TWL(0,e.ToString(),true);
      }
      return false;
    }
    public bool CheckWidgetsNonStripped() {
      try {
        if (mechLabPanel == null) { return false; }
        if (this.CheckWidgetNonStripped(this.mechLabPanel.headWidget, "head") == false) { return false; }
        if (this.CheckWidgetNonStripped(this.mechLabPanel.centerTorsoWidget, "CT") == false) { return false; }
        if (this.CheckWidgetNonStripped(this.mechLabPanel.leftTorsoWidget, "LT") == false) { return false; }
        if (this.CheckWidgetNonStripped(this.mechLabPanel.rightTorsoWidget, "RT") == false) { return false; }
        if (this.CheckWidgetNonStripped(this.mechLabPanel.leftArmWidget, "LA") == false) { return false; }
        if (this.CheckWidgetNonStripped(this.mechLabPanel.rightArmWidget, "RA") == false) { return false; }
        if (this.CheckWidgetNonStripped(this.mechLabPanel.leftLegWidget, "LL") == false) { return false; }
        if (this.CheckWidgetNonStripped(this.mechLabPanel.rightLegWidget, "RL") == false) { return false; }
        return true;
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
      return false;
    }
    public MechLabLocationWidget getLocationWidget(ChassisLocations location) {
      switch (location) {
        case ChassisLocations.Head: return this.mechLabPanel.headWidget;
        case ChassisLocations.CenterTorso: return this.mechLabPanel.centerTorsoWidget;
        case ChassisLocations.LeftTorso: return this.mechLabPanel.leftTorsoWidget;
        case ChassisLocations.RightTorso: return this.mechLabPanel.rightTorsoWidget;
        case ChassisLocations.LeftArm: return this.mechLabPanel.leftArmWidget;
        case ChassisLocations.RightArm: return this.mechLabPanel.rightArmWidget;
        case ChassisLocations.LeftLeg: return this.mechLabPanel.leftLegWidget;
        case ChassisLocations.RightLeg: return this.mechLabPanel.rightLegWidget;
        default: return null;
      }
    }
    public void ValidateWidget(ListElementController_BASE_NotListView item) {
      MechLabInventoryWidget inventoryWidget = Traverse.Create(this.mechLabPanel).Field<MechLabInventoryWidget>("inventoryWidget").Value;
      if (item.ItemWidget != null) {
        if (item.ItemWidget.controller == item) {
          //inventoryWidget.OnRemoveItem(item.ItemWidget, true);
          return;
        }
      }
      if (item is ListElementController_InventoryWeapon_NotListView weaponItem) {
        GameObject gameObject = item.dataManager.PooledInstantiate(ListElementController_BASE_NotListView.INVENTORY_ELEMENT_PREFAB_NotListView, BattleTechResourceType.UIModulePrefabs);
        if (gameObject != null) {
          weaponItem.ItemWidget = gameObject.GetComponent<InventoryItemElement_NotListView>();
          weaponItem.ItemWidget.SetData((ListElementController_BASE_NotListView)item, inventoryWidget, weaponItem.quantity);
          if (weaponItem.componentRef != null)
            weaponItem.ItemWidget.ComponentRef = weaponItem.componentRef;
          weaponItem.SetupLook(weaponItem.ItemWidget);
        }
      } else
      if (item is ListElementController_InventoryGear_NotListView gearItem) {
        GameObject gameObject = item.dataManager.PooledInstantiate(ListElementController_BASE_NotListView.INVENTORY_ELEMENT_PREFAB_NotListView, BattleTechResourceType.UIModulePrefabs);
        if (gameObject != null) {
          gearItem.ItemWidget = gameObject.GetComponent<InventoryItemElement_NotListView>();
          gearItem.ItemWidget.SetData((ListElementController_BASE_NotListView)item, inventoryWidget, gearItem.quantity);
          if (gearItem.componentRef != null)
            gearItem.ItemWidget.ComponentRef = gearItem.componentRef;
          gearItem.SetupLook(gearItem.ItemWidget);
        }
      }
      //inventoryWidget.OnRemoveItem(item.ItemWidget, true);
    }
    public Localize.Text DropToLocation(ListElementController_BASE_NotListView item, MechLabLocationWidget locationWidget) {
      //this.mechLabPanel.OnItemGrab(item.ItemWidget, null);
      Log.M?.TWL(0, "DropToLocation "+(item.componentDef!=null?item.componentDef.Description.Id:"null")+" location:"+locationWidget.loadout.Location);
      Localize.Text result = null;
      if (item.componentDef == null) {
        Log.Combat?.WL(1, "definition is null");
        return result;
      }
      Log.M?.WL(1, "widget:" + item.ItemWidget == null ? "null" : item.ItemWidget.ComponentRef.ComponentDefID);
      //bool widgetControllerNormal = false;
      if ((item.ItemWidget != null) && (item.ItemWidget.controller == item)) {
        Log.Combat?.WL(2, "widget controller is normal");
        //widgetControllerNormal = true;
      } else {
        Log.M?.WL(2, "widget controller is bad");
        return result;
      }
      MechLabItemSlotElement old_dragItem = Traverse.Create(this.mechLabPanel).Field<MechLabItemSlotElement>("dragItem").Value;
      MechComponentRef componentRef = null;
      if (item is ListElementController_InventoryWeapon_NotListView weaponItem) {
        componentRef = weaponItem.componentRef;
      } else
      if (item is ListElementController_InventoryGear_NotListView gearItem) {
        componentRef = gearItem.componentRef;
      }
      Log.Combat?.WL(1, "componentRef:"+ componentRef.ComponentDefID+" location:"+ componentRef.MountedLocation);
      MechLabPanel_ShowDropErrorMessage.errorMessage = null;
      MechLabInventoryWidget inventoryWidget = this.mechLabPanel.inventoryWidget;
      this.mechLabPanel.dragItem = this.mechLabPanel.CreateMechComponentItem(item.ItemWidget.ComponentRef, true, item.ItemWidget.ComponentRef.MountedLocation, item.ItemWidget.DropParent, item.ItemWidget);
      locationWidget.OnMechLabDrop(null, MechLabDropTargetType.NOT_SET);
      this.mechLabPanel.dragItem = old_dragItem;
      if (MechLabPanel_ShowDropErrorMessage.errorMessage == null) {
      } else {
        result = MechLabPanel_ShowDropErrorMessage.errorMessage;
        MechLabPanel_ShowDropErrorMessage.errorMessage = null;
      }
      inventoryWidget.OnRemoveItem(item.ItemWidget, true);
      Log.M?.WL(1, "quantity:" + item.quantity);
      return result;
    }
    public static int ComponentRefWeight(MechComponentRef componentRef) {
      if (componentRef.Def == null) { return 1000; };
      if (componentRef.Def.Description.Id.StartsWith("emod_structureslots")) { return 900; }
      if (componentRef.Def.Description.Id.StartsWith("emod_kit")) { return 200; }
      if (componentRef.Def.Description.Id.StartsWith("emod_engineslots")) { return 100; }
      if (componentRef.Def.Description.Id.StartsWith("emod_")) { return 150; }
      if (componentRef.ComponentDefType == ComponentType.HeatSink) { return 90; }
      if (componentRef.ComponentDefType == ComponentType.Upgrade) { return 80; }
      if (componentRef.ComponentDefType == ComponentType.JumpJet) { return 70; }
      if (componentRef.ComponentDefType == ComponentType.Weapon) { return 60; }
      if (componentRef.ComponentDefType == ComponentType.AmmunitionBox) { return 60; }
      return 1000;
    }
    public void SaveLoadoutName(string name) {
      Log.M?.TWL(0,"SaveLoadoutName:"+name,true);
      List<SavedInventoryItem> inventory = new List<SavedInventoryItem>();
      foreach (MechComponentRef componentRef in this.mechLabPanel.activeMechInventory) {
        if (componentRef == null) { continue; }
        if (componentRef.IsFixed) { continue; }
        if (componentRef.Def == null) { continue; }
        if (componentRef.DamageLevel < ComponentDamageLevel.Functional) { continue; }
        inventory.Add(new SavedInventoryItem(componentRef.ComponentDefID, componentRef.ComponentDefType, componentRef.MountedLocation));
        Log.M?.WL(1, componentRef.ComponentDefID+":"+componentRef.ComponentDefType+":"+componentRef.MountedLocation);
      }
      SavedLoadout loadout = new SavedLoadout();
      loadout.ChassisId = this.mechLabPanel.originalMechDef.ChassisID;
      loadout.inventory = inventory;
      loadout.Name = name;
      SaveLayoutHelper.Save(loadout);
    }
    public static IEnumerable<ListElementController_BASE_NotListView> BTPerfFixRawInventory(MechLabPanel panel) {
      IEnumerable<ListElementController_BASE_NotListView> result = null;
      if (get_GetRawInventoryCustomFilter != null) { result = (IEnumerable<ListElementController_BASE_NotListView>)get_GetRawInventoryCustomFilter.Invoke(null, new object[] { panel.inventoryWidget }); };
      if (result != null) { return result; }
      if (get_RawInventoryPerfFix != null) { result = (IEnumerable<ListElementController_BASE_NotListView>)get_RawInventoryPerfFix.Invoke(null, new object[] { }); }
      if (result != null) { return result; }
      result = panel.inventoryWidget.localInventory as IEnumerable<ListElementController_BASE_NotListView>;
      return result;
      //Type stateCarrier = typeof(BattletechPerformanceFix.Main).Assembly.GetType("BattletechPerformanceFix.MechlabFix.MechLabFixPublic");
      //if(stateCarrier == null) {
      //  stateCarrier = typeof(BattletechPerformanceFix.Main).Assembly.GetType("BattletechPerformanceFix.MechlabFix.MechLabFixFeature");
      //}
      //return Traverse.Create(AccessTools.Field(stateCarrier, "state").GetValue(null)).Field<List<ListElementController_BASE_NotListView>>("rawInventory").Value;
    }
    public void RestoreAs(string name,List<SavedInventoryItem> inventory) {
      try {
        Log.M?.TWL(0, "RestoreAs:"+ name);
        StringBuilder missingItems = new StringBuilder();
        Log.M?.WL(1, "inventory:" + this.inventory.Count);
        foreach (ListElementController_BASE_NotListView invItem in this.inventory) {
          Log.M?.WL(2, (invItem.componentDef!=null?invItem.componentDef.Description.Id:"null") + ":" + invItem.quantity + " widget:"+ (invItem.ItemWidget == null ? "null" : invItem.ItemWidget.gameObject.name));
        }
        List<MechComponentRef> components = new List<MechComponentRef>();
        foreach (SavedInventoryItem item in inventory) {
          //MechComponentDef def = this.GetComponentDef(item.ComponentID, item.componentType);
          //if (def == null) { continue; }
          MechComponentRef compRef = new MechComponentRef(string.Empty, string.Empty, item.componentType, item.location);
          compRef.DataManager = this.mechLabPanel.dataManager;
          compRef.ComponentDefID = item.ComponentID;
          //if (componentRef.Def.Description.Id.StartsWith("Weapon_SRM_SRM2_STREAK")) { continue; }
          components.Add(compRef);
        }
        //components.Sort((a, b) => {
        //  int a_weight = ComponentRefWeight(a);
        //  int b_weight = ComponentRefWeight(b);
        //  if (a_weight != b_weight) { return a_weight.CompareTo(b_weight); }
        //  return 0;
        //});
        Log.M?.WL(1, "equipment:" + components.Count);
        foreach (MechComponentRef componentRef in components) {
          Log.Combat?.WL(2, componentRef.ComponentDefID+":"+ componentRef.MountedLocation);
        }

        var lab_inventory = BTPerfFixRawInventory(this.mechLabPanel);
        if(lab_inventory == null) {
          GenericPopup popup = GenericPopupBuilder.Create("ERROR", "Can't find inventory").IsNestedPopupWithBuiltInFader().SetAlwaysOnTop().Render();
          return;
        }
        foreach (MechComponentRef componentRef in components) {
          //Log.WL(1, "searching:" + componentRef.Def.Description.Id);
          MechLabLocationWidget locationWidget = this.getLocationWidget(componentRef.MountedLocation);
          if (locationWidget == null) { continue; }
          //Log.WL(1, "location widget:" +(locationWidget == null?"null":locationWidget.gameObject.name));
          ListElementController_BASE_NotListView foundItem = null;
          //Log.WL(1, "searching:" + this.inventory.Count);
          foreach (ListElementController_BASE_NotListView invItem in lab_inventory) {
            if (invItem.componentDef == null) { /*Log.WL(2, invItem.GetType().Name + " has no componentDef");*/ continue; }
            //Log.WL(2, invItem.componentDef.Description.Id + ":"+ invItem.quantity);
            if (invItem.quantity <= 0) { continue; }
            if (invItem.componentDef.Description.Id == componentRef.Def.Description.Id) {
              foundItem = invItem;
            }
          }
          string componentName = new Localize.Text(string.IsNullOrEmpty(componentRef.Def.Description.UIName) ? componentRef.Def.Description.Name : componentRef.Def.Description.UIName).ToString();
          if(foundItem == null) {
            missingItems.AppendLine(componentName +" absent in storage");
            continue;
          }
          Log.M?.WL(1, "found:" + componentRef.Def.Description.Id+":"+ componentRef.MountedLocation + " count:"+ foundItem.quantity);
          if(foundItem.ItemWidget != null) {
            Log.M?.WL(2, "widget controller:" + (foundItem.ItemWidget.controller == null ? "null" : foundItem.ItemWidget.controller.componentDef.Description.Id));
          }
          ValidateWidget(foundItem);
          Localize.Text msg = DropToLocation(foundItem, locationWidget);
          if(msg != null) {
            missingItems.AppendLine(componentName+":"+msg.ToString());
          }
          Log.M?.WL(1,"Drop finished");
        }
        if(missingItems.Length > 0) {
          GenericPopup popup = GenericPopupBuilder.Create("MISSING ITEMS", new Localize.Text(missingItems.ToString()).ToString()).IsNestedPopupWithBuiltInFader().SetAlwaysOnTop().Render();
        }
      } catch (Exception e) {
        Log.E?.TWL(0, e.ToString(), true);
        UIManager.logger.LogException(e);
      }
    }
    public bool CheckDismout() {
      MechLabDismountWidget dismountWidget = Traverse.Create(this.mechLabPanel).Field<MechLabDismountWidget>("dismountWidget").Value;
      return dismountWidget.localInventory.Count == 0;
    }
    public void OnRestoreAs() {
      try {
        GenericPopup popup = null;
        if (CheckWidgetsNonStripped() == false) {
          popup = GenericPopupBuilder.Create("NEED TO BE STRIPPED", "Unit's equipment should be stripped before restore").IsNestedPopupWithBuiltInFader().SetAlwaysOnTop().Render();
          return;
        }
        if (CheckDismout() == false) {
          popup = GenericPopupBuilder.Create("DISMOUNT EQUIPMENT", "Unit's equipment dismounting should be confirmed").IsNestedPopupWithBuiltInFader().SetAlwaysOnTop().Render();
          return;
        }
        var inventories = this.gatherCompatibleInventories(this.mechLabPanel.originalMechDef.Chassis);
        List<KeyValuePair<string, List<SavedInventoryItem>>> invList = new List<KeyValuePair<string, List<SavedInventoryItem>>>();
        foreach (var inv in inventories) {
          invList.Add(new KeyValuePair<string, List<SavedInventoryItem>>(inv.Key, inv.Value));
        }
        if (invList.Count == 0) {
          popup = GenericPopupBuilder.Create("NO STOCK MECHS", "No stock mechs for this chassis").IsNestedPopupWithBuiltInFader().SetAlwaysOnTop().Render();
          return;
        }
        StringBuilder text = new StringBuilder();
        int curIndex = 0;
        int pageSize = 10;
        for (int index = curIndex - curIndex % pageSize; index < curIndex - curIndex % pageSize + pageSize; ++index) {
          if (index >= invList.Count) { break; }
          if (index == curIndex) { text.Append("<size=150%><color=orange>"); };
          text.Append(invList[index].Key);
          if (index == curIndex) { text.Append("</size></color>"); };
          text.AppendLine();
        }
        popup = GenericPopupBuilder.Create("AVAILABLE FOR CHASSIS", text.ToString())
          .AddButton("X", (Action)(() => { }), true)
          .AddButton("->", (Action)(() => {
            try {
              if (curIndex < (invList.Count - 1)) {
                ++curIndex;
                text.Clear();
                for (int index = curIndex - curIndex % pageSize; index < curIndex - curIndex % pageSize + pageSize; ++index) {
                  if (index >= invList.Count) { break; }
                  if (index == curIndex) { text.Append("<size=150%><color=orange>"); };
                  text.Append(invList[index].Key);
                  if (index == curIndex) { text.Append("</size></color>"); };
                  text.AppendLine();
                }
                if (popup != null) popup.TextContent = text.ToString();
              }
            }catch(Exception e) {
              Log.E?.TWL(0,e.ToString(),true);
              UIManager.logger.LogException(e);
            }
          }), false)
          .AddButton("<-", (Action)(() => {
            try {
              if (curIndex > 0) {
                --curIndex;
                text.Clear();
                for (int index = curIndex - curIndex % pageSize; index < curIndex - curIndex % pageSize + pageSize; ++index) {
                  if (index >= invList.Count) { break; }
                  if (index == curIndex) { text.Append("<size=150%><color=orange>"); };
                  text.Append(invList[index].Key);
                  if (index == curIndex) { text.Append("</size></color>"); };
                  text.AppendLine();
                }
                if (popup != null) popup.TextContent = text.ToString();
              }
            }catch(Exception e) {
              Log.E?.TWL(0,e.ToString(),true);
              UIManager.logger.LogException(e);
            }
          }), false).AddButton("R", (Action)(() => {
            this.RestoreAs(invList[curIndex].Key, invList[curIndex].Value);
          }), true).IsNestedPopupWithBuiltInFader().SetAlwaysOnTop().Render();
      }catch(Exception e) {
        Log.E?.TWL(0, e.ToString(), true);
        UIManager.logger.LogException(e);
      }
    }
    public void OnSaveAs() {
      try {
        GenericPopup popup = null;
        if (CheckWidgetsNonStripped()) {
          popup = GenericPopupBuilder.Create("NO NON-FIXED EQUIPMENT", "Unit's have no non-fixed equipment").IsNestedPopupWithBuiltInFader().SetAlwaysOnTop().Render();
          return;
        }
        //if (CheckDismout() == false) {
        //  popup = GenericPopupBuilder.Create("DISMOUNT EQUIPMENT", "Unit's equipment dismounting should be confirmed").IsNestedPopupWithBuiltInFader().SetAlwaysOnTop().Render();
        //  return;
        //}
        popup = GenericPopupBuilder.Create("SAVE CURRENT LAYOUT AS", "Input name for layout").AddInput("name",this.SaveLoadoutName).IsNestedPopupWithBuiltInFader().SetAlwaysOnTop().Render();
      } catch (Exception e) {
        Log.E?.TWL(0, e.ToString(), true);
      }
    }
    public void Awake() {
      try {
        if (restoreButton == null) {
          if (mechLabPanel == null) { mechLabPanel = this.gameObject.GetComponent<MechLabPanel>(); }
          if (mechLabPanel == null) { return; }
          GameObject stripDestroyedBtnObj = Traverse.Create(mechLabPanel).Field<GameObject>("stripDestroyedBtnObj").Value;
          if (stripDestroyedBtnObj == null) { return; }
          restoreButtonObj = GameObject.Instantiate(stripDestroyedBtnObj);
          restoreButtonObj.name = "uixPrfBttn_BASE_iconActionButton-MANAGED-restore-as";
          restoreButtonObj.transform.localScale = stripDestroyedBtnObj.transform.localScale;
          restoreButtonObj.transform.SetParent(stripDestroyedBtnObj.transform.parent);
          LocalizableText restore_text = restoreButtonObj.GetComponentInChildren<LocalizableText>(true);
          restore_text.SetText("RESTORE AS");
          restoreButton = restoreButtonObj.GetComponentInChildren<HBSDOTweenButton>(true);
          restoreButton.OnClicked = new UnityEngine.Events.UnityEvent();
          restoreButton.OnClicked.AddListener(new UnityEngine.Events.UnityAction(this.OnRestoreAs));

          saveButtonObj = GameObject.Instantiate(stripDestroyedBtnObj);
          saveButtonObj.name = "uixPrfBttn_BASE_iconActionButton-MANAGED-save-as";
          saveButtonObj.transform.localScale = stripDestroyedBtnObj.transform.localScale;
          saveButtonObj.transform.SetParent(stripDestroyedBtnObj.transform.parent);
          LocalizableText save_text = saveButtonObj.GetComponentInChildren<LocalizableText>(true);
          save_text.SetText("SAVE AS");
          saveButton = saveButtonObj.GetComponentInChildren<HBSDOTweenButton>(true);
          saveButton.OnClicked = new UnityEngine.Events.UnityEvent();
          saveButton.OnClicked.AddListener(new UnityEngine.Events.UnityAction(this.OnSaveAs));

          GridLayoutGroup grid = stripDestroyedBtnObj.GetComponentInParent<GridLayoutGroup>();
          if (grid != null) {
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 6;
          }
        }
        restoreButtonObj?.SetActive(mechLabPanel.IsSimGame);
        saveButtonObj?.SetActive(mechLabPanel.IsSimGame);
      } catch (Exception e) {
        Log.E?.TWL(0,e.ToString(),true);
        UIManager.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(SaveManager))]
  [HarmonyPatch(MethodType.Constructor)]
  [HarmonyPatch(new Type[] { typeof(MessageCenter) })]
  public static class SaveManager_Constructor {
    public static void Postfix(SaveManager __instance) {
      Log.M?.TWL(0, "SaveManager:" + Traverse.Create(Traverse.Create(Traverse.Create(__instance).Field<SaveSystem>("saveSystem").Value).Field<WriteLocation>("localWriteLocation").Value).Field<string>("rootPath").Value);
      //FixedMechDefHelper.Init(Path.GetDirectoryName(Traverse.Create(Traverse.Create(Traverse.Create(__instance).Field<SaveSystem>("saveSystem").Value).Field<WriteLocation>("localWriteLocation").Value).Field<string>("rootPath").Value));
      SaveLayoutHelper.Init(Path.GetDirectoryName(Traverse.Create(Traverse.Create(Traverse.Create(__instance).Field<SaveSystem>("saveSystem").Value).Field<WriteLocation>("localWriteLocation").Value).Field<string>("rootPath").Value));
      SaveDropLayoutHelper.Init(Path.GetDirectoryName(Traverse.Create(Traverse.Create(Traverse.Create(__instance).Field<SaveSystem>("saveSystem").Value).Field<WriteLocation>("localWriteLocation").Value).Field<string>("rootPath").Value));
    }
  }

  [HarmonyPatch(typeof(MechLabPanel))]
  [HarmonyPatch("InitWidgets")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyAfter("MechEngineer.Features.MechLabSlots")]
  [HarmonyPatch(new Type[] { })]
  public static class MechLabPanel_InitWidgets {
    public static void Prefix(ref bool __runOriginal, MechLabPanel __instance) {
      if (__runOriginal == false) { return; }
      MechLabPanel_InitWidgets_Reduced.Prefix(__instance);
    }
    public static void Postfix(MechLabPanel __instance) {
      try {
        Log.M?.TWL(0, "MechLabPanel.InitWidgets");
        MechLabPanelFillAs.Instance = __instance.gameObject.GetComponent<MechLabPanelFillAs>();
        if (MechLabPanelFillAs.Instance == null) { MechLabPanelFillAs.Instance = __instance.gameObject.AddComponent<MechLabPanelFillAs>(); }
        MechLabPanel_InitWidgets_Reduced.Postfix(__instance);
      } catch (Exception e) {
        Log.E?.TWL(0, e.ToString(), true);
        MechLabPanel.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(MechLabPanel))]
  [HarmonyPatch("OnRequestResourcesComplete")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(LoadRequest) })]
  public static class MechLabPanel_OnRequestResourcesComplete {
    public static void Prefix(MechLabPanel __instance, LoadRequest request) {
      try {
        Log.M?.TWL(0, "MechLabPanel.OnRequestResourcesComplete prefix");
        if (__instance.IsSimGame && (MechLabPanelFillAs.Instance != null)) {
          Thread.CurrentThread.SetFlag(MechLabPanelFillAs.INVENTORY_POPULATE_FLAG);
        }
      } catch (Exception e) {
        Log.E?.TWL(0, e.ToString(), true);
        MechLabPanel.logger.LogException(e);
      }
    }
    public static void Postfix(MechLabPanel __instance, LoadRequest request) {
      try {
        Log.M?.TWL(0, "MechLabPanel.OnRequestResourcesComplete postfix");
        if (__instance.IsSimGame && (MechLabPanelFillAs.Instance != null)) {
          Thread.CurrentThread.ClearFlag(MechLabPanelFillAs.INVENTORY_POPULATE_FLAG);
        }
      } catch (Exception e) {
        Log.E?.TWL(0, e.ToString(), true);
        MechLabPanel.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(MechLabPanel))]
  [HarmonyPatch("ConfirmRevertMech")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechLabPanel_ConfirmRevertMech {
    public static void Prefix(MechLabPanel __instance) {
      try {
        Log.M?.TWL(0, "MechLabPanel.ConfirmRevertMech prefix");
        if (__instance.IsSimGame && (MechLabPanelFillAs.Instance != null)) {
          Thread.CurrentThread.SetFlag(MechLabPanelFillAs.INVENTORY_POPULATE_FLAG);
        }
      } catch (Exception e) {
        Log.E?.TWL(0, e.ToString(), true);
        MechLabPanel.logger.LogException(e);
      }
    }
    public static void Postfix(MechLabPanel __instance) {
      try {
        Log.M?.TWL(0, "MechLabPanel.ConfirmRevertMech postfix");
        if (__instance.IsSimGame && (MechLabPanelFillAs.Instance != null)) {
          Thread.CurrentThread.ClearFlag(MechLabPanelFillAs.INVENTORY_POPULATE_FLAG);
        }
      } catch (Exception e) {
        Log.E?.TWL(0, e.ToString(), true);
        MechLabPanel.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(MechLabPanel))]
  [HarmonyPatch("ExitMechLab")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechLabPanel_ExitMechLab {
    public static void Postfix(MechLabPanel __instance) {
      try {
        Log.M?.TWL(0, "MechLabPanel.ExitMechLab postfix");
        if (MechLabPanelFillAs.Instance != null) {
          MechLabPanelFillAs.Instance.Clear();
        }
      } catch (Exception e) {
        Log.E?.TWL(0, e.ToString(), true);
        MechLabPanel.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(ListElementController_InventoryWeapon_NotListView))]
  [HarmonyPatch(MethodType.Constructor)]
  [HarmonyPatch(new Type[] { })]
  public static class ListElementController_InventoryWeapon_NotListView_Constructor {
    public static void Postfix(ListElementController_InventoryWeapon_NotListView __instance) {
      try {
        if (Thread.CurrentThread.isFlagSet(MechLabPanelFillAs.INVENTORY_POPULATE_FLAG) && (MechLabPanelFillAs.Instance != null)) {
          //Log.M?.TWL(0, "ListElementController_InventoryWeapon_NotListView constructor");
          MechLabPanelFillAs.Instance.inventory.Add(__instance);
        }
      } catch (Exception e) {
        Log.E?.TWL(0, e.ToString(), true);
        MechLabPanel.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(ListElementController_InventoryGear_NotListView))]
  [HarmonyPatch(MethodType.Constructor)]
  [HarmonyPatch(new Type[] { })]
  public static class ListElementController_InventoryGear_NotListView_Constructor {
    public static void Postfix(ListElementController_InventoryGear_NotListView __instance) {
      try {
        if (Thread.CurrentThread.isFlagSet(MechLabPanelFillAs.INVENTORY_POPULATE_FLAG) && (MechLabPanelFillAs.Instance != null)) {
          //Log.M?.TWL(0, "ListElementController_InventoryGear_NotListView constructor");
          MechLabPanelFillAs.Instance.inventory.Add(__instance);
        }
      } catch (Exception e) {
        Log.E?.TWL(0, e.ToString(), true);
        MechLabPanel.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(MechLabPanel))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("ShowDropErrorMessage")]
  [HarmonyPatch(new Type[] { typeof(Localize.Text) })]
  public static class MechLabPanel_ShowDropErrorMessage {
    public static Localize.Text errorMessage = null;
    public static void Postfix(MechLabPanel __instance, Localize.Text msg) {
      try {
        Log.M?.TWL(0, "ShowDropErrorMessage:"+msg.ToString());
        errorMessage = msg;
      } catch (Exception e) {
        Log.E?.TWL(0, e.ToString(), true);
        MechLabPanel.logger.LogException(e);
      }
    }
  }
}