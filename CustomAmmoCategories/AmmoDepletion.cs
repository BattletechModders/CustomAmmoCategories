/*  
 *  This file is part of CustomAmmoCategories.
 *  CustomAmmoCategories is free software: you can redistribute it and/or modify it under the terms of the 
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
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using CustAmmoCategoriesPatches;
using CustomAmmoCategoriesLog;
using CustomAmmoCategoriesPatches;
using CustomComponents.Patches;
using HarmonyLib;
using HBS;
using HBS.Data;
using IRBTModUtils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using UnityEngine;
using static BattleTech.Data.DataManager;

namespace CustAmmoCategories {
  [HarmonyPatch(typeof(Contract))]
  [HarmonyPatch("CompleteContract")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MissionResult), typeof(bool) })]
  public static class Contract_CompleteContractAmmoRestore {
    public static List<MechComponentDef> GetAllInventoryItemDefsUndamaged(this SimGameState sim) {
      List<MechComponentDef> result = new List<MechComponentDef>();
      foreach (string allInventoryString in sim.GetAllInventoryStrings()) {
        if (sim.CompanyStats.GetValue<int>(allInventoryString) >= 1) {
          string[] strArray = allInventoryString.Split('.');
          if (string.Compare(strArray[1], "MECHPART") != 0) {
            BattleTechResourceType techResourceType = (BattleTechResourceType)Enum.Parse(typeof(BattleTechResourceType), strArray[1]);
            if (techResourceType != BattleTechResourceType.MechDef && sim.DataManager.Exists(techResourceType, strArray[2])) {
              bool flag = strArray.Length > 3 && strArray[3].CompareTo("DAMAGED") == 0;
              if (flag) { continue; }
              MechComponentDef componentDef = sim.GetComponentDef(techResourceType, strArray[2]);
              result.Add(componentDef);
            }
          }
        }
      }
      return result;
    }
    private static void AddAmmo(this Dictionary<ExtAmmunitionDef, int> dict, ExtAmmunitionDef def, int count) {
      if (dict.ContainsKey(def)) { dict[def] += count; } else { dict.Add(def, count); };
    }
    public static void AutoRefillAmmo(this SimGameState sim) {
      Dictionary<ExtAmmunitionDef, int> ammoCapacity = new Dictionary<ExtAmmunitionDef, int>();
      List<MechComponentDef> inventory = sim.GetAllInventoryItemDefsUndamaged();
      foreach (MechComponentDef item in inventory) {
        AmmunitionBoxDef ammunitionBox = item as AmmunitionBoxDef;
        WeaponDef weapon = item as WeaponDef;
        if((ammunitionBox != null)&&(ammunitionBox.Ammo != null)) {
          ExtAmmunitionDef extAmmo = ammunitionBox.Ammo.extDef();
          if (extAmmo.AutoRefill == AutoRefilType.Automatic) { continue; }
          if (extAmmo.AutoRefill == AutoRefilType.Manual) { continue; }
          ammoCapacity.AddAmmo(extAmmo, ammunitionBox.Capacity);
        }else if(weapon != null) {
          ExtWeaponDef extWeapon = weapon.exDef();
          if (extWeapon == null) { continue; }
          foreach(var iammo in extWeapon.InternalAmmo) {
            ExtAmmunitionDef extAmmo = CustomAmmoCategories.findExtAmmo(iammo.Key);
            if (extAmmo.AutoRefill == AutoRefilType.Automatic) { continue; }
            if (extAmmo.AutoRefill == AutoRefilType.Manual) { continue; }
            ammoCapacity.AddAmmo(extAmmo, iammo.Value);
          }
        }
      }
      foreach(var mech in sim.ActiveMechs) {
        foreach(MechComponentRef item in mech.Value.Inventory) {
          if (item.DamageLevel == ComponentDamageLevel.Destroyed) { continue; }
          if (item.DamageLevel == ComponentDamageLevel.NonFunctional) { continue; }
          AmmunitionBoxDef ammunitionBox = item.Def as AmmunitionBoxDef;
          WeaponDef weapon = item.Def as WeaponDef;
          if ((ammunitionBox != null) && (ammunitionBox.Ammo != null)) {
            ExtAmmunitionDef extAmmo = ammunitionBox.Ammo.extDef();
            if (extAmmo.AutoRefill == AutoRefilType.Automatic) { continue; }
            if (extAmmo.AutoRefill == AutoRefilType.Manual) { continue; }
            ammoCapacity.AddAmmo(extAmmo, ammunitionBox.Capacity);
          } else if (weapon != null) {
            ExtWeaponDef extWeapon = weapon.exDef();
            if (extWeapon == null) { continue; }
            foreach (var iammo in extWeapon.InternalAmmo) {
              ExtAmmunitionDef extAmmo = CustomAmmoCategories.findExtAmmo(iammo.Key);
              if (extAmmo.AutoRefill == AutoRefilType.Automatic) { continue; }
              if (extAmmo.AutoRefill == AutoRefilType.Manual) { continue; }
              ammoCapacity.AddAmmo(extAmmo, iammo.Value);
            }
          }
        }
      }
      foreach (var mech in sim.ReadyingMechs) {
        foreach (MechComponentRef item in mech.Value.Inventory) {
          if (item.DamageLevel == ComponentDamageLevel.Destroyed) { continue; }
          if (item.DamageLevel == ComponentDamageLevel.NonFunctional) { continue; }
          AmmunitionBoxDef ammunitionBox = item.Def as AmmunitionBoxDef;
          WeaponDef weapon = item.Def as WeaponDef;
          if ((ammunitionBox != null)&&(ammunitionBox.Ammo != null)) {
            ExtAmmunitionDef extAmmo = ammunitionBox.Ammo.extDef();
            if (extAmmo.AutoRefill == AutoRefilType.Automatic) { continue; }
            if (extAmmo.AutoRefill == AutoRefilType.Manual) { continue; }
            ammoCapacity.AddAmmo(extAmmo, ammunitionBox.Capacity);
          } else if (weapon != null) {
            ExtWeaponDef extWeapon = weapon.exDef();
            if (extWeapon == null) { continue; }
            foreach (var iammo in extWeapon.InternalAmmo) {
              ExtAmmunitionDef extAmmo = CustomAmmoCategories.findExtAmmo(iammo.Key);
              if (extAmmo.AutoRefill == AutoRefilType.Automatic) { continue; }
              if (extAmmo.AutoRefill == AutoRefilType.Manual) { continue; }
              ammoCapacity.AddAmmo(extAmmo, iammo.Value);
            }
          }
        }
      }
      foreach (var ammocap in ammoCapacity) {
        if (ammocap.Key.AutoRefill == AutoRefilType.Automatic) { continue; }
        if (ammocap.Key.AutoRefill == AutoRefilType.Manual) { continue; }
        sim.SetAmmoCount(ammocap.Key.Id, ammocap.Value);
      }
    }
    public static void Prefix(Contract __instance) {
      Log.Combat?.TWL(0, "Contract.CompleteContract " + __instance.State);
      if (UnityGameInstance.BattleTechGame.Simulation == null) { Log.M.WL(1,"Simulation is null"); return; }
      try {
        if (__instance.State != Contract.ContractState.InProgress) { return; };
        UnityGameInstance.BattleTechGame.Simulation.FlushReserve();
        CombatGameState combat = __instance.BattleTechGame.Combat;
        List<AbstractActor> allActors = combat.AllActors;
        HashSet<string> playerGUIDS = new HashSet<string>();
        Log.Combat?.WL(1, "player GUIDS:");
        foreach (var playerMech in UnityGameInstance.BattleTechGame.Simulation.ActiveMechs) {
          playerGUIDS.Add(playerMech.Value.GUID);
          Log.Combat?.WL(2, playerMech.Value.GUID);
        }
        Log.Combat?.WL(1, "all actors:");
        foreach (AbstractActor actor in allActors) {
          Log.Combat?.WL(2, actor.DisplayName+" def.GUID:"+actor.PilotableActorDef.GUID+" team:"+actor.TeamId+"/"+ combat.LocalPlayerTeam.GUID);
          if (playerGUIDS.Contains(actor.PilotableActorDef.GUID) == false) { continue; }
          foreach (MechComponent component in actor.allComponents) {
            if (component.IsFunctional == false) { continue; }
            AmmunitionBox box = component as AmmunitionBox;
            Weapon weapon = component as Weapon;
            if(box != null) {
              if (box.ammoDef.extDef().AutoRefill == AutoRefilType.Automatic) { continue; }
              int ammocount = UnityGameInstance.BattleTechGame.Simulation.GetAmmoCount(box.ammoDef.Description.Id);
              UnityGameInstance.BattleTechGame.Simulation.SetAmmoCount(box.ammoDef.Description.Id, ammocount + box.CurrentAmmo);
              Log.Combat?.WL(2, "ammo:"+ box.ammoDef.Description.Id+"=>"+ UnityGameInstance.BattleTechGame.Simulation.GetAmmoCount(box.ammoDef.Description.Id));
            } else if(weapon != null) { 
              foreach(var stat in weapon.StatCollection) {
                if (stat.Key.StartsWith(Weapon_InternalAmmo.InternalAmmoName) == false) { continue; }
                string ammoId = stat.Key.Substring(Weapon_InternalAmmo.InternalAmmoName.Length);
                AmmunitionDef ammo = __instance.DataManager.AmmoDefs.Get(ammoId);
                if (ammo == null) { continue; }
                if (ammo.extDef().AutoRefill == AutoRefilType.Automatic) { continue; }
                int ammocount = UnityGameInstance.BattleTechGame.Simulation.GetAmmoCount(ammoId);
                UnityGameInstance.BattleTechGame.Simulation.SetAmmoCount(ammoId, ammocount + stat.Value.Value<int>());
                Log.Combat?.WL(2, "ammo:" + ammoId + "=>" + UnityGameInstance.BattleTechGame.Simulation.GetAmmoCount(ammoId));
              }
            }
          }
        }
        UnityGameInstance.BattleTechGame.Simulation.AutoRefillAmmo();
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString());
        Contract.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("AddItemStat")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string), typeof(string), typeof(bool) })]
  public static class SimGameState_AddItemStatString {
    public static bool Prefix(SimGameState __instance, string id, string type, bool damaged) {
      try {
        if (damaged == false) { return true; }
        Log.M?.TWL(0, "SimGameState.AddItemStat " + id + " type:" + type + " damaged:" + damaged);
        if (type == "AmmunitionBoxDef") {
          AmmunitionBoxDef ammoBox = __instance.DataManager.AmmoBoxDefs.Get(id);
          ExtAmmunitionDef extAmmo = ammoBox.Ammo.extDef();
          if (ammoBox == null) { return true; }
          if (extAmmo.AutoRefill == AutoRefilType.Automatic) { return true; }
          if (extAmmo.ammoOnlyBoxes.Contains(id) == false) {
            int ammocount = __instance.GetAmmoCount(ammoBox.AmmoID);
            __instance.SetAmmoCount(ammoBox.AmmoID, ammocount + ammoBox.Capacity);
            Log.M?.WL(1, "add ammo:" + ammoBox.AmmoID + "=>" + __instance.GetAmmoCount(ammoBox.AmmoID));
          } else {
            Log.M?.WL(1, "ammo only box detected");
            int ammocount = __instance.GetAmmoCount(ammoBox.AmmoID);
            int capacity = ammoBox.Capacity;
            if (capacity <= 0) { capacity = 1; }
            __instance.SetAmmoCount(ammoBox.AmmoID, ammocount + capacity);
            Log.M?.WL(1, "add ammo:" + ammoBox.AmmoID + "=>" + __instance.GetAmmoCount(ammoBox.AmmoID));
            return false;
          }
        } else if(type == "WeaponDef") {
          WeaponDef def = __instance.DataManager.WeaponDefs.Get(id);
          if (def == null) { return true; }
          ExtWeaponDef exDef = def.exDef();
          foreach (var iammo in exDef.InternalAmmo) {
            AmmunitionDef ammo = __instance.DataManager.AmmoDefs.Get(iammo.Key);
            if(ammo == null) { continue; };
            if (ammo.extDef().AutoRefill == AutoRefilType.Automatic) { continue; }
            int ammocount = __instance.GetAmmoCount(iammo.Key);
            __instance.SetAmmoCount(iammo.Key, ammocount + iammo.Value);
            Log.M?.WL(1, "add ammo:" + iammo.Key + "=>" + __instance.GetAmmoCount(iammo.Key));
          }
        }
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString());
        SimGameState.logger.LogException(e);
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("AddItemStat")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string), typeof(System.Type), typeof(bool) })]
  public static class SimGameState_AddItemStatType {
    public static bool Prefix(SimGameState __instance, string id, System.Type type, bool damaged) {
      try {
        if (damaged == false) { return true; }
        Log.M?.TWL(0, "SimGameState.AddItemStat " + id + " type:" + type + " damaged:" + damaged);
        if (type == typeof(AmmunitionBoxDef)) {
          AmmunitionBoxDef ammoBox = __instance.DataManager.AmmoBoxDefs.Get(id);
          if (ammoBox == null) { return true; }
          ExtAmmunitionDef extAmmo = ammoBox.Ammo.extDef();
          if (extAmmo.AutoRefill == AutoRefilType.Automatic) { return true; }
          if (extAmmo.ammoOnlyBoxes.Contains(id) == false) {
            int ammocount = __instance.GetAmmoCount(ammoBox.AmmoID);
            __instance.SetAmmoCount(ammoBox.AmmoID, ammocount + ammoBox.Capacity);
            Log.M?.WL(1, "add ammo:" + ammoBox.AmmoID + "=>" + __instance.GetAmmoCount(ammoBox.AmmoID));
          } else {
            Log.M?.WL(1, "ammo only box detected");
            int ammocount = __instance.GetAmmoCount(ammoBox.AmmoID);
            int capacity = ammoBox.Capacity;
            if (capacity <= 0) { capacity = 1; }
            __instance.SetAmmoCount(ammoBox.AmmoID, ammocount + capacity);
            Log.M?.WL(1, "add ammo:" + ammoBox.AmmoID + "=>" + __instance.GetAmmoCount(ammoBox.AmmoID));
            return false;
          }
        } else if (type == typeof(WeaponDef)) {
          WeaponDef def = __instance.DataManager.WeaponDefs.Get(id);
          if (def == null) { return true; }
          ExtWeaponDef exDef = def.exDef();
          foreach (var iammo in exDef.InternalAmmo) {
            AmmunitionDef ammo = __instance.DataManager.AmmoDefs.Get(iammo.Key);
            if (ammo == null) { continue; };
            if (ammo.extDef().AutoRefill == AutoRefilType.Automatic) { continue; }
            int ammocount = __instance.GetAmmoCount(iammo.Key);
            __instance.SetAmmoCount(iammo.Key, ammocount + iammo.Value);
            Log.M?.WL(1, "add ammo:" + iammo.Key + "=>" + __instance.GetAmmoCount(iammo.Key));
          }
        }
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString());
        SimGameState.logger.LogException(e);
      }
      return true;
    }
  }
  [HarmonyPatch()]
  public static class AmmunitionDef_OnLoadedWithJSON {
    private static Dictionary<string, string> ammoCustomSections = new Dictionary<string, string>();
    public static JObject getCustomSection(this AmmunitionBoxDef ammoDef) {
      if(ammoCustomSections.TryGetValue(ammoDef.AmmoID, out string result)) {
        return JObject.Parse(result);
      }
      return new JObject();
    }
    public static JObject getCustomSection(string ammoID) {
      if (ammoCustomSections.TryGetValue(ammoID, out string result)) {
        return JObject.Parse(result);
      }
      return new JObject();
    }
    public static bool isRegisredCustomSection(this AmmunitionDef ammoDef) {
      return ammoCustomSections.ContainsKey(ammoDef.Description.Id);
    }
    public static bool isRegisredCustomSection(string ammoID) {
      return ammoCustomSections.ContainsKey(ammoID);
    }
    public static bool isRegisredCustomSection(this AmmunitionBoxDef ammoDef) {
      return ammoCustomSections.ContainsKey(ammoDef.AmmoID);
    }
    public static MethodBase TargetMethod() {
      return AccessTools.Method(typeof(StringDataLoadRequest<WeaponDef>), "OnLoadedWithText");
    }
    private static readonly string GenericAmmunitionBoxSuffix = "_ContentAmmunitionBoxDef";
    public static string getGenericBox(this AmmunitionDef def) { return (def.Description.Id + GenericAmmunitionBoxSuffix); }
    public static void Prefix(FileLoadRequest __instance, string text, ref string __state) {
      Log.M?.TWL(0, "AmmunitionDefLoadRequest.OnLoadedWithJSON " + __instance.ResourceType + " " + __instance.ResourceId);
      if (__instance.ResourceType != BattleTechResourceType.AmmunitionDef) { return; }
      __state = text;
    }
    public static void Postfix(FileLoadRequest __instance, ref string __state) {
      if (__instance.ResourceType != BattleTechResourceType.AmmunitionDef) { return; }
      if (string.IsNullOrEmpty(__state)) { return; }
      AmmunitionDef ammunition = null;
      if (__instance is AmmunitionDefLoadRequest loadRequest) {
        ammunition = loadRequest.resource;
      }
      if (ammunition == null) { return; }
      Log.M?.TWL(0, "AmmunitionDef.OnLoadedWithText "+ammunition.Description.Id);
      JObject ammoJSON = null;
      if (ammunition.isRegisredCustomSection() == false) {
        try {
          ammoJSON = JObject.Parse(__state);
        } catch (Exception e) {
          throw new Exception("ammunition def " + ammunition.Description.Id + " parse error\n" + e.ToString());
        }
        if (ammoJSON["Custom"] == null) { ammoCustomSections.Add(ammunition.Description.Id, new JObject().ToString()); } else {
          Log.M?.WL(1,"Registering custom section");
          ammoCustomSections.Add(ammunition.Description.Id, ammoJSON["Custom"].ToString());
          if (ammunition.hasBoxesToReparce()) {
            Dictionary<string, string> boxesToReparce = ammunition.getBoxesToReparce();
            ammunition.clearBoxesToReparce();
            foreach(var repBox in boxesToReparce) {
              AmmunitionBoxDef boxDef = new AmmunitionBoxDef();
              boxDef.DataManager = __instance.dataManager;
              Log.M?.WL(1, "adding custom section to previous loaded box:"+ repBox.Key);
              boxDef.FromJSON(repBox.Value);
              __instance.TryLoadDependencies(boxDef as DataManager.ILoadDependencies);
              __instance.dataManager.ammoBoxDefs.Remove(boxDef.Description.Id);
              __instance.dataManager.ammoBoxDefs.Add(boxDef.Description.Id, boxDef);
            }
          }
        };
      }
      if (__instance.dataManager.AmmoBoxDefs.Exists(ammunition.getGenericBox()) == false) {
        JObject ammoBoxJSON = new JObject();
        if (ammoJSON == null) {
          try {
            ammoJSON = JObject.Parse(__state);
          } catch (Exception e) {
            throw new Exception("ammunition def " + ammunition.Description.Id + " parse error\n" + e.ToString());
          }
        }
        ammoBoxJSON["AmmoID"] = ammunition.Description.Id;
        ammoBoxJSON["Capacity"] = 0;
        ammoBoxJSON["Description"] = JObject.Parse(ammoJSON["Description"].ToString());
        ammoBoxJSON["Description"]["Id"] = ammunition.getGenericBox();
        if(CustomAmmoCategories.Settings.AmmoGenericBoxUINameAsName) ammoBoxJSON["Description"]["UIName"] = ammoBoxJSON["Description"]["Name"];
        ammoBoxJSON["ComponentType"] = "AmmunitionBox";
        ammoBoxJSON["ComponentSubType"] = "Ammunition";
        ammoBoxJSON["PrefabIdentifier"] = "";
        ammoBoxJSON["BattleValue"] = 0;
        ammoBoxJSON["InventorySize"] = 1;
        ammoBoxJSON["Tonnage"] = 1;
        ammoBoxJSON["AllowedLocations"] = "None";
        ammoBoxJSON["DisallowedLocations"] = "All";
        ammoBoxJSON["CriticalComponent"] = false;
        ammoBoxJSON["statusEffects"] = new JArray();
        ammoBoxJSON["ComponentTags"] = new JObject();
        ammoBoxJSON["ComponentTags"]["items"] = new JArray();
        ammoBoxJSON["ComponentTags"]["tagSetSourceFile"] = "";
        AmmunitionBoxDef boxDef = new AmmunitionBoxDef();
        boxDef.DataManager = __instance.dataManager;
        boxDef.FromJSON(ammoBoxJSON.ToString());
        __instance.TryLoadDependencies(boxDef as DataManager.ILoadDependencies);
        __instance.dataManager.ammoBoxDefs.Add(boxDef.Description.Id, boxDef);
      }
    }
  }
  [HarmonyPatch(typeof(AmmunitionBoxDef))]
  [HarmonyPatch("FromJSON")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class AmmunitionBoxDef_FromJSON {
    private static SpinLock spinLock = new SpinLock();
    private static Dictionary<string, Dictionary<string, string>> registerToReparce = new Dictionary<string, Dictionary<string, string>>();
    public static bool hasBoxesToReparce(this AmmunitionDef def) { return registerToReparce.ContainsKey(def.Description.Id); }
    public static Dictionary<string, string> getBoxesToReparce(this AmmunitionDef def) { return registerToReparce[def.Description.Id]; }
    public static void clearBoxesToReparce(this AmmunitionDef def) { registerToReparce.Remove(def.Description.Id); }
    public static void Prefix(AmmunitionBoxDef __instance, ref string json, ref string __state) {
      JObject boxJSON = null;
      __state = null;
      try {
        boxJSON = JObject.Parse(json);
      } catch(Exception e) {
        __state = e.ToString();
        return;
      }
      Log.M?.TWL(0, "AmmunitionBoxDef.FromJSON "+ boxJSON["Description"]["Id"]);
      string AmmoID = (string)boxJSON["AmmoID"];
      if (string.IsNullOrEmpty(AmmoID)) { return; }
      if (AmmunitionDef_OnLoadedWithJSON.isRegisredCustomSection(AmmoID)) {
        Log.M?.WL(1, "custom section exists");
        JsonMergeSettings msettings = new JsonMergeSettings {
          MergeArrayHandling = MergeArrayHandling.Union,
          MergeNullValueHandling = MergeNullValueHandling.Ignore
        };
        if (boxJSON["Custom"] == null) { boxJSON["Custom"] = AmmunitionDef_OnLoadedWithJSON.getCustomSection(AmmoID); } else { (boxJSON["Custom"] as JObject).Merge(AmmunitionDef_OnLoadedWithJSON.getCustomSection(AmmoID), msettings); };
        Log.M?.WL(1, "Merged custom section:"+ (boxJSON["Custom"] as JObject).ToString(Newtonsoft.Json.Formatting.Indented));
      } else {
        Log.M?.WL(1, "ammo custom section not exists");
        bool locked = false;
        try {
          spinLock.Enter(ref locked);
          if (registerToReparce.TryGetValue(AmmoID, out Dictionary<string, string> reparce) == false) {
            reparce = new Dictionary<string, string>();
            registerToReparce.Add(AmmoID, reparce);
          }
          if (reparce.ContainsKey((string)boxJSON["Description"]["Id"]) == false) {
            reparce.Add((string)boxJSON["Description"]["Id"], json);
          }
        }catch(Exception e) {
          if (locked) { spinLock.Exit(); locked = false; }
          throw e;
        }
        if (locked) { spinLock.Exit(); }
      }
    }
    public static void Postfix(AmmunitionBoxDef __instance, ref string __state) {
      if(__state != null) {
        throw new Exception("AmmunitionBoxDef parse error:" + __instance.Description.Id + "\n" + __state);
      }
    }
  }
  [HarmonyPatch(typeof(Shop))]
  [HarmonyPatch("GetAllInventoryShopItems")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class Shop_GetAllInventoryShopItems {
    public static List<AmmunitionBoxDef> GetAllInventoryAmmoCountDefs(this SimGameState sim) {
      List<AmmunitionBoxDef> result = new List<AmmunitionBoxDef>();
      foreach (string allInventoryString in sim.GetAllInventoryAmmoCountString()) {
        if (sim.CompanyStats.GetValue<int>(allInventoryString) >= 1) {
          string[] strArray = allInventoryString.Split('.');
          string ammoId = strArray[0];
          AmmunitionDef ammoDef = sim.DataManager.AmmoDefs.Get(ammoId);
          if (ammoDef == null) { continue; }
          ExtAmmunitionDef extAmmo = ammoDef.extDef();
          if (extAmmo.AutoRefill == AutoRefilType.Automatic) { continue; }
          AmmunitionBoxDef boxDef = sim.DataManager.AmmoBoxDefs.Get(ammoDef.getGenericBox());
          if (boxDef == null) { continue; }
          result.Add(boxDef);
        }
      }
      return result;
    }
    public static List<string> GetAllInventoryAmmoCountString(this SimGameState sim) {
      List<string> result = new List<string>();
      Dictionary<string, Statistic>.KeyCollection items = sim.CompanyStats.Items;
      foreach (string str in items) {
        if (str.Contains(string.Format("{0}.{1}", "AmmunitionDef", "AmmoCount"))) { result.Add(str); };
      }
      return result;
    }
    public static void Postfix(Shop __instance, ref List<ShopDefItem> __result) {
      try {
        SimGameState sim = __instance.Sim;
        foreach (AmmunitionBoxDef ammoBoxDef in sim.GetAllInventoryAmmoCountDefs()) {
          float cost = (float)ammoBoxDef.Description.Cost;
          int amount = sim.GetAmmoCount(ammoBoxDef.AmmoID);
          ShopDefItem shopDefItem = new ShopDefItem();
          shopDefItem.ID = ammoBoxDef.Description.Id;
          ComponentType componentType = ammoBoxDef.ComponentType;
          shopDefItem.Type = Shop.ComponentTypeToStopItemType(componentType);
          shopDefItem.SellCost = Mathf.FloorToInt(cost * sim.Constants.Finances.ShopSellModifier);
          shopDefItem.Count = amount;
          __result.Add(shopDefItem);
        }
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString());
      }
    }
  }
  [HarmonyPatch(typeof(SG_Shop_Screen))]
  [HarmonyPatch("AddShopInventory")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Shop) })]
  public static class SG_Shop_Screen_AddShopInventory {
    public static void Prefix(SG_Shop_Screen __instance, Shop shop) {
      try {
        if (__instance.isInBuyingState == false) { return; };
        HashSet<string> ammoToAdd = new HashSet<string>();
        foreach(var ammoGenBox in __instance.dm.AmmoDefs) {
          ExtAmmunitionDef extAmmo = ammoGenBox.Value.extDef();
          if (extAmmo.AutoRefill == AutoRefilType.Automatic) { continue; }
          if(extAmmo.AvailableOnPlanet.Count > 0) {
            StarSystem system = shop.system;
            if (system == null) { continue; }
            Log.M.TWL(0, "SG_Shop_Screen.AddShopInventory testing planet "+system.Def.Description.Id);
            bool containsAll = true;
            foreach(string tag in extAmmo.AvailableOnPlanet) {
              Log.M.WL(1, tag + " exists:" + system.Tags.Contains(tag));
              if (system.Tags.Contains(tag) == false) { containsAll = false; break; }
            }
            if (containsAll == false) { continue; }
          }
          if (__instance.dm.AmmoBoxDefs.Exists(ammoGenBox.Value.getGenericBox())) {
            ammoToAdd.Add(ammoGenBox.Value.getGenericBox());
          }
        }
        foreach (ShopDefItem itemDef in shop.ActiveInventory) {
          if (ammoToAdd.Contains(itemDef.GUID)) { ammoToAdd.Remove(itemDef.GUID); };
        }
        foreach (string ammoId in ammoToAdd) {
          AmmunitionBoxDef ammoBoxDef = __instance.dm.AmmoBoxDefs.Get(ammoId);
          ShopDefItem ammoShopDef = new ShopDefItem(ammoId, ShopItemType.AmmunitionBox, 0f, 1, true, false, ammoBoxDef.Description.Cost);
          shop.ActiveInventory.Add(ammoShopDef);
        }
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString());
      }
    }
  }
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("AddMech")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(int), typeof(MechDef), typeof(bool), typeof(bool), typeof(bool), typeof(string) })]
  public static class SimGameState_AddMech {
    public static void Prefix(SimGameState __instance, int idx, MechDef mech, bool active, bool forcePlacement, bool displayMechPopup, string mechAddedHeader) {
      Log.M.TWL(0, "SimGameState.AddMech " + mech.Description.Id);
      Thread.CurrentThread.pushActorDef(mech);
      try {
        if (active == false) { return; }
        foreach (MechComponentRef component in mech.Inventory) {
          if (component.DamageLevel == ComponentDamageLevel.Destroyed) { continue; }
          AmmunitionBoxDef ammoBox = component.Def as AmmunitionBoxDef;
          WeaponDef weaponDef = component.Def as WeaponDef;
          if (ammoBox != null) { 
            int ammocount = __instance.GetAmmoCount(ammoBox.AmmoID);
            if (ammoBox.Ammo == null) { continue; }
            if (ammoBox.Ammo.extDef().AutoRefill == AutoRefilType.Automatic) { continue; }
            __instance.SetAmmoCount(ammoBox.AmmoID, ammocount + ammoBox.Capacity);
            Log.M.WL(1, "add ammo:" + ammoBox.AmmoID + "=>" + __instance.GetAmmoCount(ammoBox.AmmoID));
          } else if (weaponDef != null) {
            WeaponDef def = weaponDef;
            if (def == null) { return; }
            ExtWeaponDef exDef = def.exDef();
            foreach (var iammo in exDef.InternalAmmo) {
              AmmunitionDef ammo = __instance.DataManager.AmmoDefs.Get(iammo.Key);
              if(ammo == null) { continue; }
              ExtAmmunitionDef ammoExtDef = ammo.extDef();
              if (ammoExtDef == null) { continue; }
              if (ammoExtDef.AutoRefill == AutoRefilType.Automatic) { continue; }
              int ammocount = __instance.GetAmmoCount(iammo.Key);
              __instance.SetAmmoCount(iammo.Key, ammocount + iammo.Value);
              Log.M.WL(1, "add ammo:" + iammo.Key + "=>" + __instance.GetAmmoCount(iammo.Key));
            }
          }
        }
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString());
      }
    }
    public static void Postfix(SimGameState __instance) {
      try {
        Thread.CurrentThread.clearActorDef();
      }catch(Exception e) {
        Log.M.TWL(0, e.ToString());
      }
    }
  }
  [HarmonyPatch(typeof(AmmunitionBox))]
  [HarmonyPatch("InitStats")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class AmmunitionBox_InitStats {
    public static bool isPlayerMech(this SimGameState sim, PilotableActorDef def) {
      if (sim.ActiveMechs == null) { return false; }
      foreach(var amech in sim.ActiveMechs) {
        if (amech.Value.GUID == def.GUID) { return true; }
      }
      return false;
    }
    public static void Postfix(AmmunitionBox __instance) {
      if (UnityGameInstance.BattleTechGame.Simulation == null) { return; }
      if (UnityGameInstance.BattleTechGame.Simulation.isPlayerMech(__instance.parent.PilotableActorDef) == false) { return; }
      Log.M.TWL(0, "AmmunitionBox.InitStats "+__instance.defId);
      if (__instance.ammoDef.extDef().AutoRefill == AutoRefilType.Automatic) { return; }
      int capacity = __instance.ammunitionBoxDef.Capacity;
      int ammocount = UnityGameInstance.BattleTechGame.Simulation.GetReservedAmmoCount(__instance.ammoDef.Description.Id);
      if (capacity > ammocount) { capacity = ammocount; }
      ammocount -= capacity;
      UnityGameInstance.BattleTechGame.Simulation.SetReservedAmmoCount(__instance.ammoDef.Description.Id, ammocount);
      Statistic stat = __instance.StatCollection.GetStatistic("CurrentAmmo");
      stat.SetValue<int>(capacity);
      Log.M.WL(1, "Updating ammo count for " + __instance.ammoDef.Description.Id + " to " + UnityGameInstance.BattleTechGame.Simulation.GetReservedAmmoCount(__instance.ammoDef.Description.Id));
    }
  }
  [HarmonyPatch(typeof(GameInstance))]
  [HarmonyPatch("LaunchContract")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Contract) })]
  public static class GameInstance_LaunchContract {
    public static void Prefix(GameInstance __instance) {
      try {
        if (__instance.Simulation == null) { return; }
        __instance.Simulation.ResetReserve();
      } catch(Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(LanceConfiguratorPanel), "OnConfirmClicked")]
  public static class LanceConfiguratorPanel_OnConfirmClicked {
    private static bool AmmoUsageShown = false;
    private static Dictionary<string, int> deployAmmoReserve = new Dictionary<string, int>();
    private static Dictionary<string, int> deployAmmoReserved = new Dictionary<string, int>();
    public static void FlushReserve(this SimGameState sim) {
      Log.M.TWL(0, "Flushing reserved ammo");
      foreach(var resAmmo in deployAmmoReserve) {
        int ammocount = sim.GetAmmoCount(resAmmo.Key);
        ammocount -= resAmmo.Value;
        if (ammocount < 0) { ammocount = 0; }
        sim.SetAmmoCount(resAmmo.Key, ammocount);
        Log.M.WL(1, resAmmo.Key + "=>"+sim.GetAmmoCount(resAmmo.Key));
      }
      deployAmmoReserve.Clear();
    }
    public static int GetReservedAmmoCount(this SimGameState sim, string ammoId) {
      if(deployAmmoReserved.TryGetValue(ammoId, out int result)) {
        return result;
      }
      return 0;
    }
    public static void SetReservedAmmoCount(this SimGameState sim, string ammoId, int count) {
      if (deployAmmoReserved.ContainsKey(ammoId)) {
        deployAmmoReserved[ammoId] = count;
      } else {
        deployAmmoReserved.Add(ammoId,count);
      }
    }
    public static void ResetReserve(this SimGameState sim) {
      deployAmmoReserved.Clear();
      foreach(var rammo in deployAmmoReserve) {
        deployAmmoReserved.Add(rammo.Key, rammo.Value);
      }
    }
    public static void AddInt(this Dictionary<string,int> dict, string key, int value) {
      if (dict.ContainsKey(key)) { dict[key] += value; } else { dict.Add(key, value); };
    }
    static void Prefix(ref bool __runOriginal, LanceConfiguratorPanel __instance) {
      Log.M.TWL(0, $"LanceConfiguratorPanel.OnConfirmClicked original:{__runOriginal}");
      try {
        if (__instance.sim == null) { return; }
        if (__instance.activeContract == null) { return; }
        if (AmmoUsageShown) { return; }
        HashSet<MechDef> decrementAmmoUnits = new HashSet<MechDef>();
        foreach (LanceLoadoutSlot slot in __instance.loadoutSlots) {
          if (slot.SelectedMech == null) { continue; }
          foreach (var playerMechs in __instance.sim.ActiveMechs) {
            if (slot.SelectedMech.MechDef.GUID == playerMechs.Value.GUID) {
              decrementAmmoUnits.Add(slot.SelectedMech.MechDef);
            }
          }
        }
        Dictionary<string, int> deployAmmoUsage = new Dictionary<string, int>();
        foreach (MechDef mechDef in decrementAmmoUnits) {
          foreach (MechComponentRef componentRef in mechDef.Inventory) {
            AmmunitionBoxDef ammunitionBox = componentRef.Def as AmmunitionBoxDef;
            WeaponDef weaponDef = componentRef.Def as WeaponDef;
            if (ammunitionBox != null) {
              if (ammunitionBox.Ammo.extDef().AutoRefill == AutoRefilType.Automatic) { continue; }
              deployAmmoUsage.AddInt(ammunitionBox.AmmoID, ammunitionBox.Capacity);
            } else if (weaponDef != null) {
              ExtWeaponDef extWeapon = weaponDef.exDef();
              foreach (var iammo in extWeapon.InternalAmmo) {
                AmmunitionDef ammoDef = __instance.dataManager.AmmoDefs.Get(iammo.Key);
                if (ammoDef == null) { continue; }
                if (ammoDef.extDef().AutoRefill == AutoRefilType.Automatic) { continue; }
                deployAmmoUsage.AddInt(iammo.Key, iammo.Value);
              }
            }
          }
        }
        StringBuilder content = new StringBuilder();
        deployAmmoReserve.Clear();
        foreach (var depAmmo in deployAmmoUsage) {
          AmmunitionDef ammoDef = __instance.sim.DataManager.AmmoDefs.Get(depAmmo.Key);
          if (ammoDef == null) { continue; }
          if (ammoDef.extDef().AutoRefill == AutoRefilType.Automatic) { continue; }
          int ammocount = __instance.sim.GetAmmoCount(depAmmo.Key);
          if (ammocount < depAmmo.Value) { content.Append("<color=red>"); } else { content.Append("<color=green>"); };
          content.AppendLine(ammoDef.Description.Name + " " + depAmmo.Value + " of " + ammocount + "</color>");
          deployAmmoReserve.Add(depAmmo.Key, Math.Min(depAmmo.Value, ammocount));
        }
        if (deployAmmoReserve.Count == 0) { return; }
        GenericPopupBuilder.Create("DEPLOY AMMO USAGE", content.ToString())
        .AddButton("OK", (Action)(() => {
          AmmoUsageShown = true;
          __instance.OnConfirmClicked();
          AmmoUsageShown = false;
        }), true)
        .AddButton("CANCEL", (Action)(() => {
          AmmoUsageShown = false;
        }), true)
        .AddFader(new UIColorRef?(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PopupBackfill), 0.0f, true).Render();
        __runOriginal = false; return;
      } catch (Exception e) {
        Log.M?.TWL(0,e.ToString(),true);
        UIManager.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(LanceMechEquipmentList))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("SetLoadout")]
  [HarmonyPatch(new Type[] { typeof(LocalizableText), typeof(UIColorRefTracker), typeof(Transform), typeof(ChassisLocations) })]
  public static class LanceMechEquipmentList_SetLoadout {
    public delegate ChassisLocations d_LanceMechEquipmentList_SetLoadout_Patch_MountedLocation(MechComponentRef componentRef);
    private static d_LanceMechEquipmentList_SetLoadout_Patch_MountedLocation i_LanceMechEquipmentList_SetLoadout_Patch_MountedLocation = null;
    public static ChassisLocations CC_MountedLocation(this MechComponentRef componentRef) {
      if (i_LanceMechEquipmentList_SetLoadout_Patch_MountedLocation != null) { return i_LanceMechEquipmentList_SetLoadout_Patch_MountedLocation(componentRef); }
      return componentRef.MountedLocation;
    }
    //private delegate void d_SetComponentRef(LanceMechEquipmentListItem item, MechComponentRef componentRef, MechDef mechDef);
    //private static d_SetComponentRef i_SetComponentRef = null;
    //public static void SetComponentRef(this LanceMechEquipmentListItem item, MechComponentRef componentRef, MechDef mechDef) {
    //  if (i_SetComponentRef == null) { return; }
    //  i_SetComponentRef(item, componentRef, mechDef);
    //}
    public static bool Prepare() {
      {
        Type LanceMechEquipmentList_SetLoadout_Patch = typeof(CustomComponents.Validator).Assembly.GetType("CustomComponents.Patches.LanceMechEquipmentList_SetLoadout_Patch");
        if (LanceMechEquipmentList_SetLoadout_Patch != null) {
          Log.M.TWL(0, "CustomComponents class CustomComponents.Patches.LanceMechEquipmentList_SetLoadout_Patch found");
          MethodInfo method = LanceMechEquipmentList_SetLoadout_Patch.GetMethod("MountedLocation", BindingFlags.Public | BindingFlags.Static);
          if (method != null) {
            Log.M.TWL(0, "CustomComponents method CustomComponents.Patches.LanceMechEquipmentList_SetLoadout_Patch.MountedLocation found");
            var dm = new DynamicMethod("CACLanceMechEquipmentList_SetLoadout_Patch_MountedLocation", typeof(ChassisLocations), new Type[] { typeof(MechComponentRef) });
            var gen = dm.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, method);
            gen.Emit(OpCodes.Ret);
            i_LanceMechEquipmentList_SetLoadout_Patch_MountedLocation = (d_LanceMechEquipmentList_SetLoadout_Patch_MountedLocation)dm.CreateDelegate(typeof(d_LanceMechEquipmentList_SetLoadout_Patch_MountedLocation));
          }
        }
      }
      return true;
    }
    public static bool Prefix(LanceMechEquipmentList __instance, LocalizableText headerLabel, UIColorRefTracker headerColor, Transform layoutParent, ChassisLocations location, ref UIColor __state) {
      try {
        if (UnityGameInstance.BattleTechGame.Simulation == null) { return true; }
        Log.M?.TWL(0, "LanceMechEquipmentList.SetLoadout "+ __instance.activeMech.Description.Id+" "+location);
        LocationLoadoutDef locationLoadoutDef = __instance.activeMech.GetLocationLoadoutDef(location);
        float currentArmor = locationLoadoutDef.CurrentArmor;
        float currentRearArmor = locationLoadoutDef.CurrentRearArmor;
        float assignedRearArmor = locationLoadoutDef.AssignedRearArmor;
        float internalStructure1 = locationLoadoutDef.CurrentInternalStructure;
        float internalStructure2 = __instance.activeMech.Chassis.GetLocationDef(location).InternalStructure;
        if ((double)internalStructure1 <= 0.0) {
          headerColor.SetUIColor(UIColor.Red);
        } else if ((double)currentArmor <= 0.0 || (double)assignedRearArmor > 0.0 && (double)currentRearArmor <= 0.0 || (double)internalStructure1 < (double)internalStructure2) {
          headerColor.SetUIColor(UIColor.Gold);
        } else {
          headerColor.SetUIColor(UIColor.White);
        }
        for (int index = 0; index < __instance.activeMech.Inventory.Length; ++index) {
          MechComponentRef componentRef = __instance.activeMech.Inventory[index];
          if (componentRef.CC_MountedLocation() == location) {
            GameObject gameObject = __instance.dataManager.PooledInstantiate("uixPrfPanl_LC_MechLoadoutItem", BattleTechResourceType.UIModulePrefabs, new Vector3?(), new Quaternion?(), (Transform)null);
            LanceMechEquipmentListItem component = gameObject.GetComponent<LanceMechEquipmentListItem>();
            UIColor bgColor = MechComponentRef.GetUIColor(componentRef);
            if (componentRef.DamageLevel == ComponentDamageLevel.Destroyed) { bgColor = UIColor.Disabled; }
            AmmunitionBoxDef ammoboxDef = componentRef.Def as AmmunitionBoxDef;
            WeaponDef weaponDef = componentRef.Def as WeaponDef;
            bool setDefaultName = true;
            if (ammoboxDef != null) {
              if (ammoboxDef.getAmmoCapacity(out int ammocount, out int capacity)) {
                setDefaultName = false;
                UIColor fgColor = UIColor.White;
                if (ammocount >= 0) {
                  string uiname = string.Format("{0} {1}/{2}", componentRef.Def.Description.UIName, Math.Min(capacity, ammocount), ammocount);
                  if (ammocount < (capacity / 2)) { fgColor = UIColor.Orange; } else
                  if (ammocount < capacity) { fgColor = UIColor.Gold; } else
                  if (ammocount >= capacity) { fgColor = UIColor.Green; }
                  if (componentRef.DamageLevel == ComponentDamageLevel.Functional) component.itemTextColor.SetUIColor(fgColor);
                } else {
                  string uiname = string.Format("{0} {1}", componentRef.Def.Description.UIName, capacity);
                  component.SetData(uiname, componentRef.DamageLevel, fgColor, bgColor);
                }
              }
            } else if(weaponDef != null) {
              if (weaponDef.getAmmoCapacity(out int ammocount, out int capacity)) {
                UIColor fgColor = UIColor.White;
                setDefaultName = false;
                if (ammocount >= 0) {
                  string uiname = string.Format("{0} {1}/{2}", componentRef.Def.Description.UIName, Math.Min(capacity, ammocount), ammocount);
                  if (ammocount < (capacity / 2)) { fgColor = UIColor.Orange; } else
                  if (ammocount < capacity) { fgColor = UIColor.Gold; } else
                  if (ammocount >= capacity) { fgColor = UIColor.Green; }
                  component.SetData(uiname, componentRef.DamageLevel, fgColor, bgColor);
                  if (componentRef.DamageLevel == ComponentDamageLevel.Functional) component.itemTextColor.SetUIColor(fgColor);
                } else {
                  string uiname = string.Format("{0} {1}", componentRef.Def.Description.UIName, capacity);
                  component.SetData(uiname, componentRef.DamageLevel, fgColor, bgColor);
                }
              }
            }
            if (setDefaultName) { component.SetData(componentRef.Def.Description.UIName, componentRef.DamageLevel, UIColor.White, bgColor); }
            component.SetComponentRef(componentRef, __instance.activeMech);
            component.SetTooltipData(componentRef.Def);
            gameObject.transform.SetParent(layoutParent, false);
            __instance.allComponents.Add(gameObject);
          }
        }
        return false;
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString());
        UIManager.logger.LogException(e);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(MechLabItemSlotElement))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("SetIconAndText")]
  [HarmonyPatch(new Type[] { })]
  public static class SharedVisibilityCache_RebuildCache {
    public static bool getAmmoCapacity(this AmmunitionBoxDef ammoBoxDef, out int ammocount, out int capacity) {
      ammocount = -1;
      capacity = -1;
      if (UnityGameInstance.BattleTechGame.Simulation == null) { return false; }
      if (ammoBoxDef.Ammo.extDef().AutoRefill != AutoRefilType.Automatic) {
        ammocount = UnityGameInstance.BattleTechGame.Simulation.GetAmmoCount(ammoBoxDef.AmmoID);
      } else {
        return false;
      }
      capacity = ammoBoxDef.Capacity;
      return true;
    }
    public static bool getAmmoCapacity(this WeaponDef weaponDef, out int ammocount, out int capacity) {
      ammocount = -1;
      capacity = -1;
      if (UnityGameInstance.BattleTechGame.Simulation == null) { return false; }
      if (weaponDef != null) {
        ExtWeaponDef extdef = weaponDef.exDef();
        if (extdef.AmmoCategory.BaseCategory.Is_NotSet) { return false; }
        if ((weaponDef.StartingAmmoCapacity == 0) && (extdef.InternalAmmo.Count == 0)) { return false; }
        HashSet<string> ammoIds = new HashSet<string>();
        capacity = 0;
        foreach (var iammo in extdef.InternalAmmo) {
          AmmunitionDef ammo = UnityGameInstance.BattleTechGame.DataManager.AmmoDefs.Get(iammo.Key);
          if (ammo == null) { continue; }
          capacity += iammo.Value;
          if (ammo.extDef().AutoRefill == AutoRefilType.Automatic) { continue; }
          ammoIds.Add(iammo.Key);
        }
        ammocount = ammoIds.Count > 0?0:-1;
        foreach (string ammoId in ammoIds) {
          ammocount += UnityGameInstance.BattleTechGame.Simulation.GetAmmoCount(ammoId);
        }
        return ammocount >= 0;
      }
      return false;
    }
    public static void Postfix(MechLabItemSlotElement __instance) {
      try {
        if (UnityGameInstance.BattleTechGame.Simulation == null) { return; }
        if (__instance.ammoBoxDef != null) {
          if (__instance.ammoBoxDef.getAmmoCapacity(out int ammocount, out int capacity)) {
            if (ammocount >= 0) {
              __instance.nameText.SetText("{0} {1}/{2}", (object)__instance.ammoBoxDef.Description.UIName, Math.Min(capacity, ammocount), ammocount);
            } else {
              __instance.nameText.SetText("{0} {1}", (object)__instance.ammoBoxDef.Description.UIName, capacity);
            }
          }
        }else
        if(__instance.weaponDef != null) {
          if (__instance.weaponDef.getAmmoCapacity(out int ammocount, out int capacity)) {
            if (ammocount >= 0) {
              __instance.nameText.SetText("{0} {1}/{2}", (object)__instance.weaponDef.Description.UIName, Math.Min(capacity, ammocount), ammocount);
            } else {
              __instance.nameText.SetText("{0} {1}", (object)__instance.weaponDef.Description.UIName, capacity);
            }
          }
        }
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString());
      }
    }
  }
  [HarmonyPatch(typeof(SimGameState), "Rehydrate")]
  public static class SimGameState_Rehydrate {
    public static string GetStatNameForAmmo(this AmmunitionBoxDef def) {
      return string.Format("{0}.{1}.{2}", def.AmmoID, "AmmunitionDef", "AmmoCount");
    }
    public static string GetStatNameForAmmo(string ammoId) {
      return string.Format("{0}.{1}.{2}", ammoId, "AmmunitionDef", "AmmoCount");
    }
    public static int GetAmmoCount(this SimGameState sim, string ammoId) {
      Statistic stat = sim.CompanyStats.GetStatistic(GetStatNameForAmmo(ammoId));
      if (stat == null) { return 0; }
      return stat.Value<int>();
    }
    public static void SetAmmoCount(this SimGameState sim, string ammoId, int value) {
      Statistic stat = sim.CompanyStats.GetStatistic(GetStatNameForAmmo(ammoId));
      if (stat == null) { stat = sim.CompanyStats.AddStatistic<int>(GetStatNameForAmmo(ammoId), 0); }
      stat.SetValue<int>(value);
    }
    private static void Postfix(SimGameState __instance, GameInstanceSave gameInstanceSave) {
      Log.M?.TWL(0, "SimGameState.Rehydrate");
      SimGameState_RehydrateJumpCost.Postfix(__instance);
      gameInstanceSave.RetriggerRestoreMechPostCombat(__instance);
      try {
        WeaponOrderSimGameHelper.InitSimGame(__instance);
        Dictionary<string, HashSet<AmmunitionBoxDef>> ammoBoxesSets = new Dictionary<string, HashSet<AmmunitionBoxDef>>();
        foreach (var ammoBox in __instance.DataManager.AmmoBoxDefs) {
          if (ammoBox.Value.Ammo == null) { continue; }
          if (ammoBox.Value.Ammo.extDef() == null) { continue; }
          if (ammoBox.Value.Ammo.extDef().AutoRefill == AutoRefilType.Automatic) { continue; }
          if(ammoBoxesSets.TryGetValue(ammoBox.Value.AmmoID,out HashSet<AmmunitionBoxDef> boxes) == false) {
            boxes = new HashSet<AmmunitionBoxDef>();
            ammoBoxesSets.Add(ammoBox.Value.AmmoID, boxes);
          }
          boxes.Add(ammoBox.Value);
        }
        foreach (var ammo in ammoBoxesSets) {
          string statName = GetStatNameForAmmo(ammo.Key);
          Log.M?.WL(1, statName+" boxes:"+ ammo.Value.Count);
          Statistic ammoCount = __instance.CompanyStats.GetStatistic(statName);
          if (ammoCount == null) {
            Log.M?.WL(2, "not exists");
            int ammocount = 0;
            foreach (var mech in __instance.ActiveMechs) {
              Log.M?.WL(2, "testing mech: " + mech.Value.Description.Id);
              foreach (var component in mech.Value.Inventory) {
                if (component.DamageLevel != ComponentDamageLevel.Functional) { continue; }
                WeaponDef weaponDef = component.Def as WeaponDef;
                if (weaponDef == null) { continue; }
                ExtWeaponDef extWeapon = weaponDef.exDef();
                if (extWeapon == null) { continue; }
                if ((weaponDef.StartingAmmoCapacity == 0) && (extWeapon.InternalAmmo.Count == 0)) { continue; }
                if (weaponDef.StartingAmmoCapacity != 0) { if (extWeapon.AmmoCategory.defaultAmmo().Id == ammo.Key) { ammocount += weaponDef.StartingAmmoCapacity; };  };
                foreach(var iammo in extWeapon.InternalAmmo) { if (iammo.Key == ammo.Key) { ammocount += iammo.Value; } }
                Log.M?.WL(3, "component: " + component.Def.Description.Id + ":" + component.DamageLevel);
              }
            }
            foreach (var mech in __instance.ReadyingMechs) {
              Log.M.WL(2, "testing mech: " + mech.Value.Description.Id);
              foreach (var component in mech.Value.Inventory) {
                if (component.DamageLevel != ComponentDamageLevel.Functional) { continue; }
                WeaponDef weaponDef = component.Def as WeaponDef;
                if (weaponDef == null) { continue; }
                ExtWeaponDef extWeapon = weaponDef.exDef();
                if (extWeapon == null) { continue; }
                if ((weaponDef.StartingAmmoCapacity == 0) && (extWeapon.InternalAmmo.Count == 0)) { continue; }
                if (weaponDef.StartingAmmoCapacity != 0) { if (extWeapon.AmmoCategory.defaultAmmo().Id == ammo.Key) { ammocount += weaponDef.StartingAmmoCapacity; }; };
                foreach (var iammo in extWeapon.InternalAmmo) { if (iammo.Key == ammo.Key) { ammocount += iammo.Value; } }
                Log.M?.WL(3, "component: " + component.Def.Description.Id + ":" + component.DamageLevel);
              }
            }

            foreach (AmmunitionBoxDef box in ammo.Value) {
              int box_count = __instance.GetItemCount(box.Description, box.GetType(), SimGameState.ItemCountType.UNDAMAGED_ONLY);
              Log.M?.WL(3, box.Description.Id +":"+box_count);
              ammocount += box_count * box.Capacity;
              foreach (var mech in __instance.ActiveMechs) {
                Log.M?.WL(2, "testing mech: "+ mech.Value.Description.Id);
                foreach (var component in mech.Value.Inventory) {
                  if (component.DamageLevel != ComponentDamageLevel.Functional) { continue; }
                  if (component.Def.Description.Id != box.Description.Id) { continue; };
                  Log.M?.WL(3, "component: " + component.Def.Description.Id + ":" + component.DamageLevel);
                  ammocount += box.Capacity; 
                }
              }
              foreach (var mech in __instance.ReadyingMechs) {
                Log.M?.WL(2, "testing mech: " + mech.Value.Description.Id);
                foreach (var component in mech.Value.Inventory) {
                  if (component.DamageLevel != ComponentDamageLevel.Functional) { continue; }
                  if (component.Def.Description.Id != box.Description.Id) { continue; }
                  Log.M?.WL(3, "component: " + component.Def.Description.Id + ":" + component.DamageLevel);
                  ammocount += box.Capacity;
                }
              }
            }
            ammoCount = __instance.CompanyStats.AddStatistic<int>(statName, 0);
            if(ammoCount == null) {
              Log.M?.WL(2, "fail to add stat:" + statName);
              continue;
            }
            Log.M?.WL(2, "ammocount:"+ ammocount);
            ammoCount.SetValue<int>(ammocount);
          }
          Log.M?.WL(2, "count:" + __instance.GetAmmoCount(ammo.Key));
        }
      }catch(Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
        SimGameState.logger.LogException(e);
      }
    }
  }
}