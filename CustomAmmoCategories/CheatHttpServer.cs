﻿/*  
 *  This file is part of CustomAmmoCategories.
 *  CustomAmmoCategories is free software: you can redistribute it and/or modify it under the terms of the 
 *  GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, 
 *  or (at your option) any later version. CustomAmmoCategories is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 *  See the GNU Lesser General Public License for more details.
 *  You should have received a copy of the GNU Lesser General Public License along with CustomAmmoCategories. 
 *  If not, see <https://www.gnu.org/licenses/>. 
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Reflection;
using BattleTech;
using CustomAmmoCategoriesLog;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using BattleTech.UI;
using UnityEngine;

namespace CustAmmoCategories {
  [HarmonyPatch(typeof(UIManager))]
  [HarmonyPatch("Awake")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  internal static class UIManager_Awake {
    public static void Postfix(UIManager __instance) {
      Log.M?.TWL(0, "UIManager.Awake");
      try {
        if(UnityGameInstance_UpdateCACHTTP.Instance == null) {
          UnityGameInstance_UpdateCACHTTP.Instance = __instance.gameObject.GetComponent<UnityGameInstance_UpdateCACHTTP>();
          if (UnityGameInstance_UpdateCACHTTP.Instance == null) {
            UnityGameInstance_UpdateCACHTTP.Instance = __instance.gameObject.AddComponent<UnityGameInstance_UpdateCACHTTP>();
          }
        }
      } catch (Exception ex) {
        Log.M?.TWL(0, ex.ToString(), true);
      }
    }
  }
  public class UnityGameInstance_UpdateCACHTTP: MonoBehaviour {
    private static Queue<CACHTTPRequestItem> httpRequests = new Queue<CACHTTPRequestItem>();
    public static UnityGameInstance_UpdateCACHTTP Instance = null;
    public static CACHTTPRequestItem SendCACRequest(string name, string input) {
      CACHTTPRequestItem request = new CACHTTPRequestItem(name, input);
      httpRequests.Enqueue(request);
      return request;
    }
    public  void getreputation(CACHTTPRequestItem request) {
      Log.M.TWL(0, "Запрос на получение репутации");
      System.Collections.Generic.Dictionary<string, string> jresp = new Dictionary<string, string>();
      BattleTech.GameInstance gameInstance = BattleTech.UnityGameInstance.BattleTechGame;
      Log.M.WL(1, "Получен gameInstance");
      if (gameInstance == null) {
        jresp["error"] = "Не могу получить инстанс игры";
        request.ready(jresp);
        return;
      }
      BattleTech.SimGameState gameState = gameInstance.Simulation;
      CustomAmmoCategoriesLog.Log.LogWrite("Получен gameState\n");
      if (gameState == null) {
        jresp["error"] = "Не могу получить состояние симулятора. Скорее всего не загружено сохранение";
        //SendResponce(ref response, jresp);
        request.ready(jresp);
        return;
      }
      if ((gameState.SimGameMode != BattleTech.SimGameState.SimGameType.CAREER) && (gameState.SimGameMode != BattleTech.SimGameState.SimGameType.KAMEA_CAMPAIGN)) {
        jresp["error"] = "Неправильный режим компании:" + gameState.SimGameMode.ToString();
        request.ready(jresp);
        return;
      }
      var factions = FactionEnumeration.FactionList;
      CustomAmmoCategoriesLog.Log.LogWrite("FactionEnumeration\n");
      foreach (var pFaction in factions) {
        jresp[pFaction.Name] = gameState.GetRawReputation(pFaction).ToString();
        CustomAmmoCategoriesLog.Log.M.WL(1, pFaction.Name + ":" + pFaction.ID);
      }
      foreach (var faction in gameState.DataManager.Factions) {
        CustomAmmoCategoriesLog.Log.M.WL(1, faction.Key + ":" + faction.Value.ID + ":" + faction.Value.FactionValue.Name);
      }
      CustomAmmoCategoriesLog.Log.M.WL(0, "WeaponCategoryEnumeration");
      foreach (EnumValue ev in WeaponCategoryEnumeration.WeaponCategoryList) {
        CustomAmmoCategoriesLog.Log.M.WL(1, ev.Name + ":" + ev.ID);
      }
      CustomAmmoCategoriesLog.Log.M.WL(0, "AmmoCategoryEnumeration");
      foreach (EnumValue ev in AmmoCategoryEnumeration.AmmoCategoryList) {
        CustomAmmoCategoriesLog.Log.M.WL(1, ev.Name + ":" + ev.ID);
      }
      CustomAmmoCategoriesLog.Log.M.WL(0, "AmmunitionTypeEnumeration");
      foreach (EnumValue ev in AmmunitionTypeEnumeration.AmmunitionTypeList) {
        CustomAmmoCategoriesLog.Log.M.WL(1, ev.Name + ":" + ev.ID);
      }
      CustomAmmoCategoriesLog.Log.M.WL(0, "ContractTypeEnumeration");
      foreach (EnumValue ev in ContractTypeEnumeration.ContractTypeValueList) {
        CustomAmmoCategoriesLog.Log.M.WL(1, ev.Name + ":" + ev.ID);
      }
      request.ready(jresp);
    }
    public  void endcontract(CACHTTPRequestItem request) {
      CustomAmmoCategoriesLog.Log.LogWrite("Запрос на завершение контракта\n");
      System.Collections.Generic.Dictionary<string, string> jresp = new Dictionary<string, string>();
      BattleTech.GameInstance gameInstance = BattleTech.UnityGameInstance.BattleTechGame;
      CustomAmmoCategoriesLog.Log.LogWrite("Получен gameInstance\n");
      if (gameInstance == null) {
        jresp["error"] = "Не могу получить инстанс игры";
        request.ready(jresp);
        //SendResponce(ref response, jresp);
        return;
      }
      CombatGameState combat = gameInstance.Combat;
      if (combat == null) {
        jresp["error"] = "Не могу получить состояние битвы";
        request.ready(jresp);
        //SendResponce(ref response, jresp);
        return;
      }
      Contract contract = combat.ActiveContract;
      if (contract == null) {
        jresp["error"] = "Не могу получить текущий контракт";
        request.ready(jresp);
        //SendResponce(ref response, jresp);
        return;
      }
      try {
        typeof(TurnDirector).GetProperty("TheMissionResult").GetSetMethod(true).Invoke(combat.TurnDirector, new object[1] { MissionResult.Victory });
        jresp["success"] = "yes";
        Log.M.WL(1, "isMissionOver:" + combat.TurnDirector.IsMissionOver);
        Log.M.WL(1, "CanEndMission:" + combat.StackManager.CanEndMission());
      } catch (Exception e) {
        jresp["error"] = e.ToString();
      }
      //gameState.PilotRoster.ElementAt(0).AddAbility("");
      request.ready(jresp);
      //SendResponce(ref response, jresp);
    }
    public  void setreputation(CACHTTPRequestItem request) {
      ThreadWork.CSetReputation setrep = JsonConvert.DeserializeObject<ThreadWork.CSetReputation>(request.input);
      CustomAmmoCategoriesLog.Log.LogWrite("Запрос на установку репутации\n");
      System.Collections.Generic.Dictionary<string, string> jresp = new Dictionary<string, string>();
      BattleTech.GameInstance gameInstance = BattleTech.UnityGameInstance.BattleTechGame;
      CustomAmmoCategoriesLog.Log.LogWrite("Получен gameInstance\n");
      if (gameInstance == null) {
        jresp["error"] = "Не могу получить инстанс игры";
        request.ready(jresp);
        //SendResponce(ref response, jresp);
        return;
      }
      BattleTech.SimGameState gameState = gameInstance.Simulation;
      CustomAmmoCategoriesLog.Log.LogWrite("Получен gameState\n");
      if (gameState == null) {
        jresp["error"] = "Не могу получить состояние симулятора. Скорее всего не загружено сохранение";
        request.ready(jresp);
        //SendResponce(ref response, jresp);
        return;
      }
      if ((gameState.SimGameMode != BattleTech.SimGameState.SimGameType.CAREER) && (gameState.SimGameMode != BattleTech.SimGameState.SimGameType.KAMEA_CAMPAIGN)) {
        jresp["error"] = "Неправильный режим компании:" + gameState.SimGameMode.ToString();
      }
      var factions = FactionEnumeration.FactionList;
      CustomAmmoCategoriesLog.Log.LogWrite("Получен список фракций\n");
      foreach (var pFaction in factions) {
        if (setrep.faction.Equals(pFaction.Name)) {
          gameState.SetReputation(pFaction, setrep.reputation, BattleTech.StatCollection.StatOperation.Set);
        }
        jresp[pFaction.Name] = gameState.GetRawReputation(pFaction).ToString();
      }
      request.ready(jresp);
    }
    public  void listitems(CACHTTPRequestItem request) {
      Log.M.TWL(0,"listitems");
      System.Collections.Generic.Dictionary<string, string> jresp = new Dictionary<string, string>();
      BattleTech.GameInstance gameInstance = BattleTech.UnityGameInstance.BattleTechGame;
      CustomAmmoCategoriesLog.Log.LogWrite("get gameInstance\n");
      if (gameInstance == null) {
        jresp["error"] = "gameInstance is null";
        request.ready(jresp);
        return;
      }
      BattleTech.Data.DataManager dataManager = gameInstance.DataManager;
      if (dataManager == null) {
        jresp["error"] = "DataManager is null";
        request.ready(jresp);
        return;
      }
      List<ThreadWork.CDefItem> items = new List<ThreadWork.CDefItem>();
      List<ThreadWork.CDefItem> weapons = new List<ThreadWork.CDefItem>();
      Log.M.WL(1, "dataManager.WeaponDefs:"+ dataManager.WeaponDefs.Count);
      foreach (var wDef in dataManager.WeaponDefs) {
        ThreadWork.CDefItem itm = new ThreadWork.CDefItem();
        itm.name = wDef.Key;
        itm.price = wDef.Value.Description.Cost;
        itm.type = BattleTech.ShopItemType.Weapon;
        itm.count = 1;
        weapons.Add(itm);
      }
      weapons.Sort((x, y) => x.name.CompareTo(y.name));
      List<ThreadWork.CDefItem> amunitionBoxes = new List<ThreadWork.CDefItem>();
      Log.M.WL(1, "dataManager.AmmoBoxDefs:" + dataManager.AmmoBoxDefs.Count);
      foreach (var abDef in dataManager.AmmoBoxDefs) {
        ThreadWork.CDefItem itm = new ThreadWork.CDefItem();
        itm.name = abDef.Key;
        itm.price = abDef.Value.Description.Cost;
        itm.type = BattleTech.ShopItemType.AmmunitionBox;
        itm.count = 1;
        amunitionBoxes.Add(itm);
      }
      amunitionBoxes.Sort((x, y) => x.name.CompareTo(y.name));
      List<ThreadWork.CDefItem> heatSinks = new List<ThreadWork.CDefItem>();
      Log.M.WL(1, "dataManager.HeatSinkDefs:" + dataManager.HeatSinkDefs.Count);
      foreach (var hsDef in dataManager.HeatSinkDefs) {
        ThreadWork.CDefItem itm = new ThreadWork.CDefItem();
        itm.name = hsDef.Key;
        itm.price = hsDef.Value.Description.Cost;
        itm.type = BattleTech.ShopItemType.HeatSink;
        itm.count = 1;
        heatSinks.Add(itm);
      }
      heatSinks.Sort((x, y) => x.name.CompareTo(y.name));
      List<ThreadWork.CDefItem> jumpJets = new List<ThreadWork.CDefItem>();
      Log.M.WL(1, "dataManager.JumpJetDefs:" + dataManager.JumpJetDefs.Count);
      foreach (var jjDef in dataManager.JumpJetDefs) {
        ThreadWork.CDefItem itm = new ThreadWork.CDefItem();
        itm.name = jjDef.Key;
        itm.price = jjDef.Value.Description.Cost;
        itm.type = BattleTech.ShopItemType.JumpJet;
        itm.count = 1;
        jumpJets.Add(itm);
      }
      jumpJets.Sort((x, y) => x.name.CompareTo(y.name));
      List<ThreadWork.CDefItem> upgrades = new List<ThreadWork.CDefItem>();
      Log.M.WL(1, "dataManager.UpgradeDefs:" + dataManager.UpgradeDefs.Count);
      foreach (var uDef in dataManager.UpgradeDefs) {
        ThreadWork.CDefItem itm = new ThreadWork.CDefItem();
        itm.name = uDef.Key;
        itm.price = uDef.Value.Description.Cost;
        itm.type = BattleTech.ShopItemType.Upgrade;
        itm.count = 1;
        upgrades.Add(itm);
      }
      upgrades.Sort((x, y) => x.name.CompareTo(y.name));
      List<ThreadWork.CDefItem> mechs = new List<ThreadWork.CDefItem>();
      Log.M.WL(1, "dataManager.MechDefs:" + dataManager.MechDefs.Count);
      foreach (var mDef in dataManager.MechDefs) {
        ThreadWork.CDefItem itm = new ThreadWork.CDefItem();
        itm.name = mDef.Key;
        itm.price = mDef.Value.Description.Cost;
        itm.type = BattleTech.ShopItemType.Mech;
        itm.count = 1;
        mechs.Add(itm);
      }
      mechs.Sort((x, y) => x.name.CompareTo(y.name));
      items.AddRange(weapons);
      items.AddRange(amunitionBoxes);
      items.AddRange(heatSinks);
      items.AddRange(upgrades);
      items.AddRange(jumpJets);
      items.AddRange(mechs);
      //items.ToDictionary();
      Log.M.WL(1, "result:" + items.Count);
      request.ready(items);
    }
    public  void listpilots(CACHTTPRequestItem request) {
      Log.M.TWL(0, "listpilots");
      System.Collections.Generic.Dictionary<string, string> jresp = new Dictionary<string, string>();
      BattleTech.GameInstance gameInstance = BattleTech.UnityGameInstance.BattleTechGame;
      CustomAmmoCategoriesLog.Log.LogWrite("get gameInstance\n");
      if (gameInstance == null) {
        jresp["error"] = "gameInstance is null";
        request.ready(jresp);
        return;
      }
      BattleTech.Data.DataManager dataManager = gameInstance.DataManager;
      if (dataManager == null) {
        jresp["error"] = "DataManager is null";
        request.ready(jresp);
        return;
      }
      List<string> pilots = new List<string>();
      foreach(var pilot in dataManager.PilotDefs) {
        pilots.Add(pilot.Key);
      }
      pilots.Sort();
      Log.M.WL(1, "result:" + pilots.Count);
      request.ready(pilots);
    }
    public  void additem(CACHTTPRequestItem request) {
      Log.M.TWL(0,"Запрос на добавление предмета");
      System.Collections.Generic.Dictionary<string, string> jresp = new Dictionary<string, string>();
      BattleTech.GameInstance gameInstance = BattleTech.UnityGameInstance.BattleTechGame;
      Log.M.WL(1,"Получен gameInstance");
      if (gameInstance == null) {
        jresp["error"] = "Не могу получить инстанс игры";
        request.ready(jresp);
        return;
      }
      BattleTech.SimGameState gameState = gameInstance.Simulation;
      CustomAmmoCategoriesLog.Log.LogWrite("Получен gameState\n");
      if (gameState == null) {
        jresp["error"] = "Не могу получить состояние симулятора. Скорее всего не загружено сохранение";
        request.ready(jresp);
        return;
      }
      if ((gameState.SimGameMode != BattleTech.SimGameState.SimGameType.CAREER) && (gameState.SimGameMode != BattleTech.SimGameState.SimGameType.KAMEA_CAMPAIGN)) {
        jresp["error"] = "Неправильный режим компании:" + gameState.SimGameMode.ToString();
      }
      ThreadWork.CDefItem itm = JsonConvert.DeserializeObject<ThreadWork.CDefItem>(request.input);
      try {
        Log.M.WL(2, itm.name+":"+ itm.type);
        if (itm.type == BattleTech.ShopItemType.Mech) {
          if (itm.count == 1) {
            MechDef mech = new MechDef(gameState.DataManager.MechDefs.Get(itm.name), gameState.GenerateSimGameUID());
            Log.M.WL(2, itm.name + ":" + itm.type);
            gameState.AddMech(-1, mech, true, false, true, "'Unit Add Cheat");
          } else {
            itm.type = ShopItemType.MechPart;
            gameState.AddFromShopDefItem(new BattleTech.ShopDefItem(itm.name, itm.type, 0.0f, itm.count, false, false, itm.price));
          }
          //  gameState.MessageCenter.PublishMessage((MessageCenterMessage)new SimGamePurchaseMessage(item, cost, transactionType));

          //  //gameState.CurSystem.SystemShop.ActiveInventory.Add(new BattleTech.ShopDefItem(itm.name, BattleTech.ShopItemType.Mech, 0.0f, itm.count, false, false, itm.price));
          //  //gameState.AddFunds(itm.price * 2);
        } else {
          gameState.AddFromShopDefItem(new BattleTech.ShopDefItem(itm.name, itm.type, 0.0f, itm.count, false, false, itm.price));
        }
        jresp["success"] = "yes";
      } catch (Exception e) {
        jresp["error"] = e.ToString();
      }

      //gameState.PilotRoster.ElementAt(0).AddAbility("");
      request.ready(jresp);
    }
    public void addpilot(CACHTTPRequestItem request) {
      Log.M.TWL(0, "add pilot request");
      System.Collections.Generic.Dictionary<string, string> jresp = new Dictionary<string, string>();
      BattleTech.GameInstance gameInstance = BattleTech.UnityGameInstance.BattleTechGame;
      Log.M.WL(1, "Получен gameInstance");
      if (gameInstance == null) {
        jresp["error"] = "Не могу получить инстанс игры";
        request.ready(jresp);
        return;
      }
      BattleTech.SimGameState gameState = gameInstance.Simulation;
      CustomAmmoCategoriesLog.Log.LogWrite("Получен gameState\n");
      if (gameState == null) {
        jresp["error"] = "Не могу получить состояние симулятора. Скорее всего не загружено сохранение";
        request.ready(jresp);
        return;
      }
      if ((gameState.SimGameMode != BattleTech.SimGameState.SimGameType.CAREER) && (gameState.SimGameMode != BattleTech.SimGameState.SimGameType.KAMEA_CAMPAIGN)) {
        jresp["error"] = "Неправильный режим компании:" + gameState.SimGameMode.ToString();
      }
      JObject json = JObject.Parse(request.input);
      Log.M.WL(1,"pilot:"+ (string)json["pilotdef"]);
      try {
        //gameState.AddPilotToRoster((string)json["pilotdef"]);
        gameState.RequestItem<PilotDef>((string)json["pilotdef"], (Action<PilotDef>)(obj =>
        {
          if (obj == null) { return; }
          gameState.CurSystem.AddAvailablePilot(obj,true);
        }), BattleTechResourceType.PilotDef);
        jresp["success"] = "yes";
      } catch (Exception e) {
        jresp["error"] = e.ToString();
      }

      //gameState.PilotRoster.ElementAt(0).AddAbility("");
      request.ready(jresp);
    }
    public void setattackdirection(CACHTTPRequestItem request) {
      JObject responce = new JObject();
      BattleTech.GameInstance gameInstance = BattleTech.UnityGameInstance.BattleTechGame;
      Log.M.WL(1, "Получен gameInstance");
      if (gameInstance == null) {
        responce["error"] = new JObject();
        responce["error"]["str"] = "Can't get game instance";
        request.ready(responce);
        return;
      }
      if (gameInstance.Combat == null) {
        responce["error"] = new JObject();
        responce["error"]["str"] = "Can't get combat";
        request.ready(responce);
        return;
      }
      JObject json = JObject.Parse(request.input);
      Log.M.WL(1, $"actor: {(string)json["actor"]} direction: {(string)json["dir"]}");
      try {
        //gameState.AddPilotToRoster((string)json["pilotdef"]);
        AbstractActor actor = gameInstance.Combat.FindActorByGUID((string)json["actor"]);
        if (actor == null) {
          responce["error"] = new JObject();
          responce["error"]["str"] = $"actor {(string)json["actor"]} not found";
          request.ready(responce);
          return;
        }
        if(Enum.TryParse<AttackDirection>((string)json["dir"], out var attackDirection)) {
          actor.SetAttackDirection(attackDirection);
        } else {
          responce["error"] = new JObject();
          responce["error"]["str"] = $"wrong attack direction value {(string)json["dir"]}";
          request.ready(responce);
          return;
        }
        responce["success"] = "yes";
      } catch (Exception e) {
        responce["error"] = e.ToString();
      }
      request.ready(responce);
    }
    public void battledump(CACHTTPRequestItem request) {
      Log.M.TWL(0, "battle dump request");
      JObject responce = new JObject();
      BattleTech.GameInstance gameInstance = BattleTech.UnityGameInstance.BattleTechGame;
      Log.M.WL(1, "Получен gameInstance");
      if (gameInstance == null) {
        responce["error"] = new JObject();
        responce["error"]["str"] = "Can't get game instance";
        request.ready(responce);
        return;
      }
      if(gameInstance.Combat == null) {
        responce["error"] = new JObject();
        responce["error"]["str"] = "Can't get combat";
        request.ready(responce);
        return;
      }
      JArray AllActors = new JArray();
      foreach (AbstractActor unit in UnityGameInstance.BattleTechGame.Combat.AllActors) {
        JObject junit = new JObject();
        junit["DisplayName"] = unit.DisplayName;
        junit["GUID"] = unit.GUID;
        junit["ChassisID"] = unit.PilotableActorDef.ChassisID;
        junit["DefID"] = unit.PilotableActorDef.Description.Id;
        junit["DefGUID"] = unit.PilotableActorDef.GUID;
        junit["OccupiedDesignMaskID"] = unit.OccupiedDesignMaskID;
        junit["Team"] = unit.team.DisplayName + ":" + unit.TeamId + ":" + (unit.Combat.LocalPlayerTeamGuid == unit.TeamId ? "Player" : "AI");
        JArray StatCollection = new JArray();
        List<string> stats = new List<string>();
        foreach(var stat in unit.StatCollection) {
          stats.Add(stat.Key+"="+stat.Value.CurrentValue);
        }
        stats.Sort();
        foreach (var stat in stats) { StatCollection.Add(stat); }
        junit["StatCollection"] = StatCollection;
        JArray allComponents = new JArray();
        foreach(var component in unit.allComponents) {
          JObject jcomp = new JObject();
          jcomp["defId"] = component.defId;
          jcomp["location"] = component.Location;
          jcomp["type"] = component.baseComponentRef.ComponentDefType.ToString();
          JArray compStatCollection = new JArray();
          List<string> comp_stats = new List<string>();
          foreach (var compstat in component.StatCollection) {
            comp_stats.Add(compstat.Key + "=" + compstat.Value.CurrentValue);
          }
          comp_stats.Sort();
          foreach (var compstat in comp_stats) { compStatCollection.Add(compstat); }
          jcomp["StatCollection"] = compStatCollection;
          allComponents.Add(jcomp);
        }
        junit["inventory"] = allComponents;
        AllActors.Add(junit);
      };
      responce["units"] = AllActors;
      request.ready(responce);
    }
    public void generateflashpoint(CACHTTPRequestItem request) {
      System.Collections.Generic.Dictionary<string, string> jresp = new Dictionary<string, string>();
      Log.M?.TWL(0,$"generateflashpoint");
      try {
        JObject data = JObject.Parse(request.input);
        BattleTech.GameInstance gameInstance = BattleTech.UnityGameInstance.BattleTechGame;
        CustomAmmoCategoriesLog.Log.LogWrite("Получен gameInstance\n");
        if (gameInstance == null) {
          jresp["error"] = "Не могу получить инстанс игры";
          goto exit;
        }
        BattleTech.SimGameState gameState = gameInstance.Simulation;
        CustomAmmoCategoriesLog.Log.LogWrite("Получен gameState\n");
        if (gameState == null) {
          jresp["error"] = "Не могу получить состояние симулятора. Скорее всего не загружено сохранение";
          goto exit;
        }
        if ((gameState.SimGameMode != BattleTech.SimGameState.SimGameType.CAREER) && (gameState.SimGameMode != BattleTech.SimGameState.SimGameType.KAMEA_CAMPAIGN)) {
          jresp["error"] = "Неправильный режим компании:" + gameState.SimGameMode.ToString();
          goto exit;
        }
        gameState.GenerateFlashpointCommand((string)data["flashpointId"], (string)data["systemId"]);
        jresp["success"] = "yes";
      }catch(Exception e) {
        jresp["error"] = e.ToString();
      }
    exit:
      request.ready(jresp);
    }
    public void LateUpdate() {
      try {
        //Online.OnlineClientHelper.KeepAlive();
        //Log.M.TWL(0, "UnityGameInstance_UpdateCACHTTP.LateUpdate "+ httpRequests.Count);
        if (httpRequests.Count == 0) { return; }
        CACHTTPRequestItem request = httpRequests.Dequeue();
        MethodInfo method = typeof(UnityGameInstance_UpdateCACHTTP).GetMethod(request.name,BindingFlags.Instance|BindingFlags.Public);
        if (method != null) { method.Invoke(this, new object[] { request }); return; }
        request.ready("not implemented");
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
    }
  }

  public class CACHTTPRequestItem {
    public string name { get; private set; }
    public string input { get; private set; }
    public object output { get; private set; }
    public bool is_ready { get; private set; }
    public CACHTTPRequestItem(string name,string input) {
      this.name = name;
      this.input = input;
      is_ready = false;
      output = string.Empty;
    }
    public void ready(object output) {
      this.output = output;
      this.is_ready = true;
    }
  }
  public class ThreadWork {
    public class CDefItem {
      public string name { get; set; }
      public int price { get; set; }
      public BattleTech.ShopItemType type { get; set; }
      public int count { get; set; }
    }
    public class CSetReputation {
      public string faction { get; set; }
      public int reputation { get; set; }
    }
    public class CGenerateFlashpoint {
      public string flashpointId { get; set; }
      public int systemId { get; set; }
    }
    private static string GetMimeType(string ext) {
      switch (ext) {
        case ".html": return "text/html";
        case ".htm": return "text/html";
        case ".txt": return "text/plain";
        case ".jpe": return "image/jpeg";
        case ".jpeg": return "image/jpeg";
        case ".jpg": return "image/jpeg";
        case ".js": return "application/javascript";
        case ".png": return "image/png";
        case ".gif": return "image/gif";
        case ".bmp": return "image/bmp";
        case ".ico": return "image/x-icon";
      }
      return "application/octed-stream";
    }
    private static void SendError(ref HttpListenerResponse response, int Code, string text) {
      // Получаем строку вида "200 OK"
      // HttpStatusCode хранит в себе все статус-коды HTTP/1.1
      response.StatusCode = Code;
      response.ContentType = "text/html";
      string Html = "<html><body><h1>" + text + "</h1></body></html>";
      byte[] buffer = System.Text.Encoding.UTF8.GetBytes(Html);
      response.ContentLength64 = buffer.Length;
      Stream output = response.OutputStream;
      // Закроем соединение
      output.Write(buffer, 0, buffer.Length);
      output.Close();
      response.Close();
    }
    private static void SendResponce(ref HttpListenerResponse response, object jresp) {
      // Получаем строку вида "200 OK"
      // HttpStatusCode хранит в себе все статус-коды HTTP/1.1
      response.StatusCode = 200;
      response.ContentType = "application/json";
      string Html = JsonConvert.SerializeObject(jresp);
      // Приведем строку к виду массива байт
      //byte[] Buffer = Encoding.UTF8.GetBytes(Html);
      // Отправим его клиенту
      byte[] buffer = System.Text.Encoding.UTF8.GetBytes(Html);
      response.ContentLength64 = buffer.Length;
      Stream output = response.OutputStream;
      // Закроем соединение
      output.Write(buffer, 0, buffer.Length);
      output.Close();
      response.Close();
    }
    public static void DoWork() {
      CustomAmmoCategoriesLog.Log.LogWrite("Initing http server " + CustomAmmoCategories.Settings.modHTTPServer + "...\n");
      if (CustomAmmoCategories.Settings.modHTTPServer == false) { return; }
      HttpListener listener = new HttpListener();
      CustomAmmoCategoriesLog.Log.LogWrite("Prefix " + CustomAmmoCategories.Settings.modHTTPListen + "...\n");
      listener.Prefixes.Add(CustomAmmoCategories.Settings.modHTTPListen);
      listener.Start();
      string assemblyFile = (new System.Uri(Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath;
      while (true) {
        HttpListenerContext context = listener.GetContext();
        HttpListenerRequest request = context.Request;
        HttpListenerResponse response = context.Response;
        string filename = request.Url.AbsolutePath;
        if (filename == "/") { filename = "index.html"; };
        filename = Path.Combine(CustomAmmoCategoriesLog.Log.BaseDirectory, Path.GetFileName(filename));
        Log.M.TWL(0,"Base directory " + CustomAmmoCategoriesLog.Log.BaseDirectory + " Access '" + filename);
        if (File.Exists(filename)) {
          CustomAmmoCategoriesLog.Log.LogWrite("File exists\n");
          response.ContentType = GetMimeType(Path.GetExtension(filename));
          FileStream FS;
          try {
            FS = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
          } catch (Exception) {
            // Если случилась ошибка, посылаем клиенту ошибку 500
            SendError(ref response, 500, "Не могу открыть " + filename);
            return;
          }
          Stream output = response.OutputStream;
          byte[] Buffer = new byte[1024];
          // Переменная для хранения количества байт, принятых от клиента
          int Count = 0;
          // Пока не достигнут конец файла
          while (FS.Position < FS.Length) {
            // Читаем данные из файла
            Count = FS.Read(Buffer, 0, Buffer.Length);
            // И передаем их клиенту
            output.Write(Buffer, 0, Count);
          }
          // Закроем файл и соединение
          FS.Close();
          output.Close();
          response.Close();
          CustomAmmoCategoriesLog.Log.LogWrite("File out\n");
          continue;
        }
        string data = string.Empty;
        if (request.HttpMethod == "POST") {
          using (System.IO.StreamReader reader = new System.IO.StreamReader(request.InputStream, request.ContentEncoding)) {
            data = reader.ReadToEnd();
          }
        } else {
          Dictionary<string, string> query = new Dictionary<string, string>();
          for (int i = 0; i < request.QueryString.Count; i++) {
            query.Add(request.QueryString.GetKey(i), request.QueryString.Get(i));
          }
          data = JsonConvert.SerializeObject(query);
        }
        Log.M.WL(1,"Get data:'" + Path.GetFileName(filename));
        CACHTTPRequestItem cac_request = UnityGameInstance_UpdateCACHTTP.SendCACRequest(Path.GetFileName(filename), data);
        while (cac_request.is_ready == false) { Thread.Sleep(10); }
        SendResponce(ref response, cac_request.output);
        continue;
      }
    }
  }
}
