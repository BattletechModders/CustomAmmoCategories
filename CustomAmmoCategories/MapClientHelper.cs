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
  public class OnlineCareerIDWidget: MonoBehaviour {
    public SGHeaderWidget parent = null;
    public HBSTooltip tooltip = null;
    public LocalizableText text = null;
    public HBSDOTweenButton button = null;
    public RectTransform source = null;
    public RectTransform rect = null;
    public static OnlineCareerIDWidget Instantine(SGHeaderWidget __instance) {
      GameObject careerModeArea = Traverse.Create(__instance).Field<GameObject>("careerModeArea").Value;
      GameObject careerIdArea = GameObject.Instantiate(careerModeArea);
      careerIdArea.transform.SetParent(careerModeArea.transform.parent);
      careerIdArea.transform.localScale = Vector3.one;
      RectTransform careerModeAreaRT = careerModeArea.GetComponent<RectTransform>();
      RectTransform careerIdAreaRT = careerIdArea.GetComponent<RectTransform>();
      careerIdAreaRT.pivot = new Vector2(0f, 1.1f);
      OnlineCareerIDWidget widget = careerIdArea.AddComponent<OnlineCareerIDWidget>();
      widget.parent = __instance;
      careerIdArea.FindObject<RectTransform>("warningState", false).gameObject.SetActive(false);
      careerIdArea.FindObject<RectTransform>("completedState", false).gameObject.SetActive(false);
      careerIdArea.FindObject<RectTransform>("normalState", false).gameObject.SetActive(true);
      widget.tooltip = careerIdArea.FindObject<HBSTooltip>("OBJ_CareerButton2", false);
      widget.text = careerIdArea.FindObject<LocalizableText>("daysLeftText", false);
      widget.button = careerIdArea.FindObject<HBSDOTweenButton>("OBJ_CareerButton2", false);
      widget.button.OnClicked = new UnityEngine.Events.UnityEvent();
      widget.button.OnClicked.AddListener(new UnityAction(widget.OnClicked));
      widget.rect = careerIdAreaRT;
      widget.source = careerModeAreaRT;
      return widget;
    }
    public void Update() {
      if ((this.rect != null) && (this.source != null)) {
        this.rect.position = this.source.position;
      }
    }
    public void OnClicked() {
      Log.M?.TWL(0, "OnlineCareerIDWidget.OnClicked");
      GenericPopup popup = GenericPopupBuilder.Create("ONLINE ID CLIPBOARD", $"What would you like to copy to clipboard?\nCAREER ID:{this.simGame.CAREER_ID()}\nCAREER LINK:{(string.Format(CustomAmmoCategories.Settings.MapOnlineClientLink, this.simGame.CAREER_ID()))}")
        .AddButton("CLOSE", (Action)(() => {
        }), true, BTInput.Instance.Key_Escape())
        .AddButton("CAREER ID", (Action)(() => {
          GUIUtility.systemCopyBuffer = this.simGame.CAREER_ID();
          Log.M?.TWL(0, $"OnlineCareerIDWidget.OnClicked CAREER_ID:{GUIUtility.systemCopyBuffer}");
        }), true, BTInput.Instance.Key_Return())
        .AddButton("CAREER LINK", (Action)(() => {
          GUIUtility.systemCopyBuffer = string.Format(CustomAmmoCategories.Settings.MapOnlineClientLink, this.simGame.CAREER_ID());
          Log.M?.TWL(0, $"OnlineCareerIDWidget.OnClicked CLIENT_ID:{GUIUtility.systemCopyBuffer}");
        }), true, BTInput.Instance.Key_None())
        .IsNestedPopupWithBuiltInFader().SetAlwaysOnTop().Render();
    }
    public SimGameState simGame;
    public void Init(SimGameState simGame) {
      this.simGame = simGame;
      this.gameObject.SetActive(PersistentMapClientHelper.HasOnlineAPI);
      if (this.source.gameObject.activeSelf) {
        this.rect.pivot = new Vector2(0f, 1.1f);
      } else {
        this.rect.pivot = new Vector2(0f, 0f);
      }
      StringBuilder lines = new StringBuilder();
      lines.AppendLine($"ONLINE STATE:{PersistentMapClientHelper.RegistrationState}");
      lines.AppendLine($"CLIENT ID:{simGame.CLIENT_ID()}");
      lines.AppendLine($"CAREER ID:{simGame.CAREER_ID()}");
      lines.AppendLine($"Click to copy to clipboard");
      this.tooltip.SetDefaultStateData(new BaseDescriptionDef("CAREER_ID","ONLINE ID", lines.ToString(), string.Empty).GetTooltipStateData());
      //string shortId = simGame.CAREER_ID();// "82b93e31-4cab-4d97-af78-c926105dc2dd"
      this.text.SetText($"ONLINE STATE:{PersistentMapClientHelper.RegistrationState}");
    }
  }

  [HarmonyPatch(typeof(SGHeaderWidget))]
  [HarmonyPatch("SetCompanyCrest")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class SGHeaderWidget_SetCompanyCrest {
    public static void Postfix(SGHeaderWidget __instance) {
      if (CustomAmmoCategories.Settings.MapOnlineClientDrawWidget == false) { return; }
      Log.M?.TWL(0, "SGHeaderWidget.SetCompanyCrest");
      OnlineCareerIDWidget careerIDWidget = __instance.gameObject.GetComponentInChildren<OnlineCareerIDWidget>(true);
      if(careerIDWidget == null) {
        careerIDWidget = OnlineCareerIDWidget.Instantine(__instance);
      }
      careerIDWidget.Init(__instance.simState);
    }
  }
  [HarmonyPatch(typeof(SGHeaderWidget))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(SimGameState) })]
  public static class SGHeaderWidget_Init {
    public static void Postfix(SGHeaderWidget __instance, SimGameState simGame) {
      if (CustomAmmoCategories.Settings.MapOnlineClientDrawWidget == false) { return; }
      Log.M?.TWL(0, "SGHeaderWidget.Init");
      OnlineCareerIDWidget careerIDWidget = __instance.gameObject.GetComponentInChildren<OnlineCareerIDWidget>(true);
      if (careerIDWidget == null) {
        careerIDWidget = OnlineCareerIDWidget.Instantine(__instance);
      }
      careerIDWidget.Init(simGame);
    }
  }

}