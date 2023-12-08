using BattleTech;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using BattleTech.UI.Tooltips;
using CustomAmmoCategoriesLog;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace CustAmmoCategories {
  public static class PersistentMapClientHelper {
    public static string CLIENT_ID_STAT {
      get {
        if (PmcCore_CLIENT_ID_STAT != null) { return (string)PmcCore_CLIENT_ID_STAT.GetValue(null); }
        return "UNDEFINED";
      }
    }
    public static string CAREER_ID_STAT {
      get {
        if (PmcCore_CAREER_ID_STAT != null) { return (string)PmcCore_CAREER_ID_STAT.GetValue(null); }
        return "UNDEFINED";
      }
    }
    public static bool HasOnlineAPI {
      get {
        if (PmcCore == null) { return false; }
        object rtoApi = AccessTools.Field(PmcCore, "rtoApi").GetValue(null);
        if (rtoApi == null) { return false; }
        return true;
      }
    }
    public static string CLIENT_ID(this SimGameState sim) {
      var stat = sim.CompanyStats.GetStatistic(PersistentMapClientHelper.CLIENT_ID_STAT);
      if (stat == null) { return string.Empty; }
      return stat.Value<string>();
    }
    public static string ServerURL {
      get {
        if (PmcCore == null) { return string.Empty; }
        object settings = AccessTools.Field(PmcCore, "settings").GetValue(null);
        if (settings == null) { return string.Empty; }
        return Traverse.Create(settings).Field<string>("ServerURL").Value;
      }
    }
    public static string CAREER_ID(this SimGameState sim) {
      var stat = sim.CompanyStats.GetStatistic(PersistentMapClientHelper.CAREER_ID_STAT);
      if (stat == null) { return string.Empty; }
      return stat.Value<string>();
    }
    public static string RegistrationState {
      get {
        if (PmcCore == null) { return "UNDEFINED"; }
        //object rtoApi = AccessTools.Field(PmcCore, "rtoApi").GetValue(null);
        if (rtoApi == null) { return "UNDEFINED"; }
        try {
          return Traverse.Create(rtoApi).Method("RegistrationState").GetValue().ToString();
        }catch(Exception e) {
          Log.M?.TWL(0,e.ToString(),true);
          return "UNDEFINED";
        }
      }
    }
    public static void FloatAdd(string statname, float value) {
      if (PmcCore == null) { return; }
      if (rtoApi == null) { return; }
      try {
        Traverse.Create(rtoApi).Method("AlterStat", statname, PersistentMapClient_Features_RtoApi_MetaDataOp["FloatAdd"], value).GetValue();
      }catch(Exception e) {
        Log.M?.TWL(0,e.ToString(), true);
      }
    }
    public static float? FloatGet(string statname) {
      if (PmcCore == null) { return new float?(); }
      if (rtoApi == null) { return new float?(); }
      try {
        return Traverse.Create(rtoApi).Method("GetStat", statname, null).GetValue<float?>();
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
        return new float?();
      }
    }
    private static Type PmcCore = null;
    private static object _rtoApi = null;
    public static object rtoApi {
      get {
        if (PmcCore == null) { return null; }
        if (_rtoApi == null) { _rtoApi = AccessTools.Field(PmcCore, "rtoApi").GetValue(null); }
        return _rtoApi;
      }
    }
    //private static Type PersistentMapClient_Features_RtoApi_CareerState = null;
    private static Dictionary<string, object> PersistentMapClient_Features_RtoApi_CareerState = new Dictionary<string, object>();
    private static Dictionary<string, object> PersistentMapClient_Features_RtoApi_MetaDataOp = new Dictionary<string, object>();
    private static FieldInfo PmcCore_CLIENT_ID_STAT = null;
    private static FieldInfo PmcCore_CAREER_ID_STAT = null;
    private static Assembly assembly = null;
    public static void Init() {
     // UnityGameInstance.BattleTechGame.Combat.AllActors[0].GameRep.gameObject.FindObject<>();
      Log.M?.TWL(0, "PersistentMapClientHelper init");
      foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
        if (assembly.FullName.StartsWith("PersistentMapClient, Version=")) { PersistentMapClientHelper.assembly = assembly; break; }
      }
      if(assembly != null) {
        Log.M?.WL(1, "PersistentMapClient.dll found");
        PmcCore = assembly.GetType("PersistentMapClient.PmcCore");
        if(PmcCore != null) {
          Log.M?.WL(1, "PersistentMapClient.PmcCore found");
          PmcCore_CLIENT_ID_STAT = AccessTools.Field(PmcCore, nameof(CLIENT_ID_STAT));
          PmcCore_CAREER_ID_STAT = AccessTools.Field(PmcCore, nameof(CAREER_ID_STAT));
          if (PmcCore_CLIENT_ID_STAT != null) { Log.M?.WL(1, "PersistentMapClient.PmcCore.CLIENT_ID_STAT found"); }
          if (PmcCore_CAREER_ID_STAT != null) { Log.M?.WL(1, "PersistentMapClient.PmcCore.CAREER_ID_STAT found"); }
        }
        Type RtoApi = assembly.GetType("PersistentMapClient.Features.RtoApi");
        if (RtoApi != null) {
          Log.M?.WL(1, "PersistentMapClient.Features.RtoApi found");
          Type RtoApi_CareerState = RtoApi.GetNestedType("CareerState");
          if (RtoApi_CareerState != null) {
            Log.M?.WL(1, "PersistentMapClient.Features.RtoApi.CareerState found");
            foreach (object CareerState in Enum.GetValues(RtoApi_CareerState)) {
              Log.M?.WL(2, $"{CareerState.ToString()}");
              PersistentMapClient_Features_RtoApi_CareerState.Add(CareerState.ToString(), CareerState);
            }
          }
          Type RtoApi_MetaDataOp = RtoApi.GetNestedType("MetaDataOp");
          if (RtoApi_MetaDataOp != null) {
            Log.M?.WL(1, "PersistentMapClient.Features.RtoApi.MetaDataOp found");
            foreach (object MetaDataOp in Enum.GetValues(RtoApi_MetaDataOp)) {
              Log.M?.WL(2, $"{MetaDataOp.ToString()}");
              PersistentMapClient_Features_RtoApi_MetaDataOp.Add(MetaDataOp.ToString(), MetaDataOp);
            }
          }
        }
      }
    }
  }

}