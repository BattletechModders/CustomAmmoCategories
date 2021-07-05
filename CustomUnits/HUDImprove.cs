using BattleTech;
using BattleTech.Data;
using BattleTech.Framework;
using BattleTech.Save;
using BattleTech.Save.Test;
using BattleTech.Serialization;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using BattleTech.UI.Tooltips;
using CustAmmoCategories;
using CustAmmoCategoriesPatches;
using CustomAmmoCategoriesPatches;
using DG.Tweening;
using Harmony;
using HBS;
using HBS.Collections;
using Localize;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SVGImporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CustomUnits {
  public class AbilityDefEx {
    public int Priority { get; set; }
  }
  public class LanceConfigurationState {
    public int lanceId;
    public int lanceStartPos;
    public void Clear() { lanceId = 0; lanceStartPos = 0; }
    public LanceConfigurationState() { lanceId = 0; lanceStartPos = 0; }
  }
  public class LanceConfigutationNextSlot : LanceConfigutationSwitchBtn {
    public override void InitVisuals() {
      if ((caption != null) && (tooltip != null)) {
        tooltip.SetString("next slot");
        caption.SetText(">>");
        base.InitVisuals();
      }
    }
    public override void OnClick() {
      state.lanceStartPos += 4;
      int maxStartPos = CustomLanceHelper.lanceSize(state.lanceId) - 4;
      if (maxStartPos < 0) { maxStartPos = 0; };
      if (state.lanceStartPos > maxStartPos) { state.lanceStartPos = maxStartPos; }
      LanceConfiguratorPanel_SetData.updateSlots();
      base.OnClick();
    }
  }
  public class LanceConfigutationNextLance : LanceConfigutationSwitchBtn {
    public override void InitVisuals() {
      if ((caption != null) && (tooltip != null)) {
        tooltip.SetString("switch lance");
        caption.SetText("Lance " + (state.lanceId + 1));
        base.InitVisuals();
      }
    }
    public override void OnClick() {
      state.lanceId = (state.lanceId + 1) % CustomLanceHelper.lancesCount();
      caption.SetText("Lance " + (state.lanceId + 1));
      state.lanceStartPos = 0;
      LanceConfiguratorPanel_SetData.updateSlots();
      base.OnClick();
    }
  }
  public class LanceConfigutationPrevSlot : LanceConfigutationSwitchBtn {
    public override void InitVisuals() {
      if ((caption != null) && (tooltip != null)) {
        tooltip.SetString("prev slot");
        caption.SetText("<<");
        base.InitVisuals();
      }

    }
    public override void OnClick() {
      state.lanceStartPos -= 4;
      if (state.lanceStartPos < 0) { state.lanceStartPos = 0; };
      LanceConfiguratorPanel_SetData.updateSlots();
      base.OnClick();
    }

  }
  public class LanceConfigutationSwitchBtn : CUCustomButton {
    public LanceConfigurationState state;
    public void Init(LanceConfigurationState state) {
      this.state = state;
      this.Init();
    }
    public override void Init() {
      base.Init();
    }
  }
  public class AARResultScreenButton : CUCustomButton {
    public AAR_UnitsResult_Screen screen;
    public int value = 0;
    private float t;
    public override void InitVisuals() {
      if (caption != null) {
        this.transform.localScale = Vector3.one * 0.8f;
        if (value > 0) {
          caption.SetText("");
        } else {
          caption.SetText("");
        }
        base.InitVisuals();
      }
      t = 0f;
    }
    public void Init(AAR_UnitsResult_Screen screen, int value) {
      this.screen = screen;
      this.value = value;
      this.Init();
    }
    public override void UpdateSelf() {
      if (t < 0.5f) { t += Time.deltaTime; return; } else {
        if (t < 100.0f) {
          btnColor.color = UIManager.Instance.UIColorRefs.lightGray;
          borderColor.color = UIManager.Instance.UIColorRefs.orange;
          t = 100.0f;
        }
      };
    }
    public override void OnClick() {
      this.screen.CurViewStartPosition(this.screen.CurViewStartPosition() + value);
      if (this.screen.CurViewStartPosition() > this.screen.UnitWidgets().Count - 4) { this.screen.CurViewStartPosition(this.screen.UnitWidgets().Count - 4); }
      if (this.screen.CurViewStartPosition() < 0) { this.screen.CurViewStartPosition(0); }
      for (int index = 0; index < screen.UnitWidgets().Count; ++index) {
        screen.UnitWidgets()[index].gameObject.SetActive((index >= screen.CurViewStartPosition()) && (index < (screen.CurViewStartPosition() + 4)));
      }
    }
    public override void Init() {
      base.Init();
    }
  }
  public class AARSkirmishScreenButton : CUCustomButton {
    public AAR_SkirmishResult_Screen screen;
    public int value = 0;
    private float t;
    public override void InitVisuals() {
      if (caption != null) {
        this.transform.localScale = Vector3.one * 0.8f;
        if (value > 0) {
          caption.SetText(">>");
        } else {
          caption.SetText("<<");
        }
        base.InitVisuals();
      }
      t = 0f;
    }
    public void Init(AAR_SkirmishResult_Screen screen, int value) {
      this.screen = screen;
      this.value = value;
      this.Init();
    }
    public override void UpdateSelf() {
      if (t < 0.5f) { t += Time.deltaTime; return; } else {
        if (t < 100.0f) {
          btnColor.color = UIManager.Instance.UIColorRefs.lightGray;
          borderColor.color = UIManager.Instance.UIColorRefs.orange;
          t = 100.0f;
        }
      };
    }
    public override void OnClick() {
      this.screen.CurViewStartPosition(this.screen.CurViewStartPosition() + value);
      if (this.screen.CurViewStartPosition() > this.screen.UnitWidgets().Count - 4) { this.screen.CurViewStartPosition(this.screen.UnitWidgets().Count - 4); }
      if (this.screen.CurViewStartPosition() < 0) { this.screen.CurViewStartPosition(0); }
      for (int index = 0; index < screen.UnitWidgets().Count; ++index) {
        screen.UnitWidgets()[index].gameObject.SetActive((index >= screen.CurViewStartPosition()) && (index < (screen.CurViewStartPosition() + 4)));
      }
    }
    public override void Init() {
      base.Init();
    }
  }
  public class CUCustomButton : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler {
    public SVGImage btnColor;
    public SVGImage borderColor;
    public HBSTooltipStateData tooltip;
    public LocalizableText caption;
    protected bool inited = false;
    public virtual void InitVisuals() {
      btnColor.color = UIManager.Instance.UIColorRefs.lightGray;
      borderColor.color = UIManager.Instance.UIColorRefs.orange;
      inited = true;
    }
    public virtual void Init() {
      inited = false;
    }
    public virtual void OnClick() {

    }
    public void OnPointerClick(PointerEventData eventData) {
      Log.TWL(0, "LanceConfigutationSwitchBtn.OnPointerClick");
      OnClick();
    }

    public void OnPointerEnter(PointerEventData eventData) {
      btnColor.color = Color.white;
      borderColor.color = UIManager.Instance.UIColorRefs.white;
    }

    public void OnPointerExit(PointerEventData eventData) {
      btnColor.color = UIManager.Instance.UIColorRefs.lightGray;
      borderColor.color = UIManager.Instance.UIColorRefs.orange;
    }
    public virtual void UpdateSelf() {

    }
    public void Update() {
      if (inited == false) {
        InitVisuals();
      }
      UpdateSelf();
    }
    protected void Awake() {
      Log.TWL(0, "CUCustomButton.Awake");
      try {
        GameObject bttn2_Fill = this.transform.Find("bttn2_Fill").gameObject;
        btnColor = bttn2_Fill.GetComponent<SVGImage>();
        btnColor.color = UIManager.Instance.UIColorRefs.lightGray;
        GameObject bttn2_border = bttn2_Fill.transform.FindRecursive("bttn2_border").gameObject;
        borderColor = bttn2_border.GetComponent<SVGImage>();
        borderColor.color = UIManager.Instance.UIColorRefs.orange;
        HBSTooltip hbsTooltip = this.gameObject.GetComponent<HBSTooltip>();
        if (hbsTooltip != null) {
          tooltip = (HBSTooltipStateData)typeof(HBSTooltip).GetField("defaultStateData", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(hbsTooltip);
        } else {
          tooltip = null;
        }
        Transform bttn2_Text_optional = this.transform.FindRecursive("bttn2_Text-optional");
        if (bttn2_Text_optional != null) {
          caption = bttn2_Text_optional.gameObject.GetComponent<LocalizableText>();
        } else {
          Transform bttn2_Text = this.transform.FindRecursive("bttn2_Text");
          if (bttn2_Text != null) {
            caption = bttn2_Text.gameObject.GetComponent<LocalizableText>();
          }
        }
        //InitVisuals();
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
      }
      inited = false;
    }
  }
  public class CustomLanceDef {
    public int size { get; set; }
    public int allow { get; set; }
    public bool is_vehicle { get; set; }
    public CustomLanceDef() { size = 0; allow = 0; is_vehicle = false; }
    public CustomLanceDef(int s, int a, bool v) { size = s; allow = a; is_vehicle = v; }
  }
  public class PlayerLancesLoadout {
    public Dictionary<string, int> loadout;
    public PlayerLancesLoadout() { loadout = new Dictionary<string, int>(); }
  }
  [HarmonyPatch(typeof(SimGameState), "Dehydrate")]
  public static class SimGameState_Dehydrate {
    static void Prefix(SimGameState __instance, SerializableReferenceContainer references, ref List<string> ___LastUsedMechs, ref List<string> ___LastUsedPilots) {
      Log.TWL(0, "SimGameState.Dehydrate");
      try {
        Log.WL(1, "playerLanceLoadout(" + CustomLanceHelper.playerLanceLoadout.loadout.Count + "):");
        foreach (var guid in CustomLanceHelper.playerLanceLoadout.loadout) {
          Log.WL(2, "GUID:" + guid.Key + "->" + guid.Value);
        }
        Statistic playerMechContent = __instance.CompanyStats.GetStatistic(CustomLanceHelper.SaveReferenceName);
        if (playerMechContent == null) {
          __instance.CompanyStats.AddStatistic<string>(CustomLanceHelper.SaveReferenceName, JsonConvert.SerializeObject(CustomLanceHelper.playerLanceLoadout));
        } else {
          playerMechContent.SetValue<string>(JsonConvert.SerializeObject(CustomLanceHelper.playerLanceLoadout));
        }
        if (___LastUsedMechs == null) { ___LastUsedMechs = new List<string>(); }
        if (___LastUsedPilots == null) { ___LastUsedPilots = new List<string>(); }
        for (int i = 0; i < ___LastUsedPilots.Count; ++i) {
          if (___LastUsedPilots[i] == null) {
            ___LastUsedPilots[i] = string.Empty;
          }
        }
        for (int i = 0; i < ___LastUsedMechs.Count; ++i) {
          if (___LastUsedMechs[i] == null) {
            ___LastUsedMechs[i] = string.Empty;
          }
        }
        //references.AddItem<PlayerAllyLanceContent>(CustomLanceHelper.SaveReferenceName, CustomLanceHelper.playerAllyContent);
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(SimGameState), "Rehydrate")]
  public static class SimGameState_Rehydrate {
    static void Postfix(SimGameState __instance, GameInstanceSave gameInstanceSave) {
      Log.TWL(0, "SimGameState.Rehydrate");
      try {
        Statistic playerMechContent = __instance.CompanyStats.GetStatistic(CustomLanceHelper.SaveReferenceName);
        if (playerMechContent == null) {
          CustomLanceHelper.playerLanceLoadout.loadout.Clear();
        } else {
          CustomLanceHelper.playerLanceLoadout = JsonConvert.DeserializeObject<PlayerLancesLoadout>(playerMechContent.Value<string>());
        }
        //CustomLanceHelper.playerAllyContent = references.GetItem<PlayerAllyLanceContent>(CustomLanceHelper.SaveReferenceName);
        Log.WL(1, "playerLanceLoadout(" + CustomLanceHelper.playerLanceLoadout.loadout.Count + "):");
        foreach (var guid in CustomLanceHelper.playerLanceLoadout.loadout) {
          Log.WL(2, "GUID:" + guid.Key + "->" + guid.Value);
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("SaveLastLance")]
  [HarmonyPatch(MethodType.Normal)]
  public static class SimGameState_SaveLastLance {
    public static bool Prefix(SimGameState __instance, LanceConfiguration config, ref List<string> ___LastUsedMechs, ref List<string> ___LastUsedPilots) {
      ___LastUsedMechs = new List<string>();
      ___LastUsedPilots = new List<string>();
      Log.TWL(0, "SimGameState.SaveLastLance");
      if (config == null) {
        Log.WL(0, "no config - no to save");
        return false;
      }
      foreach (SpawnableUnit lanceUnit in config.GetLanceUnits("bf40fd39-ccf9-47c4-94a6-061809681140")) {
        string str1 = "";
        string str2 = "";
        SpawnableUnit spawnableUnit = lanceUnit;
        if (spawnableUnit != null) {
          if ((spawnableUnit.Unit == null) && (spawnableUnit.Pilot != null)) { continue; }
          if ((spawnableUnit.Unit != null) && (spawnableUnit.Pilot == null)) { continue; }
          if (spawnableUnit.Unit != null && spawnableUnit.Pilot != null) {
            str1 = spawnableUnit.Unit.GUID;
            str2 = spawnableUnit.Pilot.Description.Id;
          } else {
            ___LastUsedMechs.Add(string.Empty);
            ___LastUsedPilots.Add(string.Empty);
          }
          if (!string.IsNullOrEmpty(str1) && !string.IsNullOrEmpty(str2)) {
            ___LastUsedMechs.Add(str1);
            ___LastUsedPilots.Add(str2);
          }
        } else {
          ___LastUsedMechs.Add(string.Empty);
          ___LastUsedPilots.Add(string.Empty);
        }
      }
      Log.WL(1, "pilots:");
      for (int i = 0; i < ___LastUsedPilots.Count; ++i) {
        Log.WL(2, "[" + i + "] " + ___LastUsedPilots[i]);
      }
      Log.WL(1, "units:");
      for (int i = 0; i < ___LastUsedMechs.Count; ++i) {
        Log.WL(2, "[" + i + "] " + ___LastUsedMechs[i]);
      }
      return false;
    }
  }
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("FillContractLance")]
  [HarmonyPatch(MethodType.Normal)]
  public static class SimGameState_FillContractLance {
    public static bool Prefix(SimGameState __instance) {
      Log.TWL(0, "SimGameState.FillContractLance");
      Contract selectedContract = __instance.SelectedContract;
      LanceConfiguration lanceConfiguration = __instance.RoomManager.CmdCenterRoom.lanceConfigBG.LC.CreateLanceConfiguration();
      __instance.SaveLastLance(lanceConfiguration);
      LanceConfiguration clearLanceConfiguration = lanceConfiguration.clearConfig();
      Log.WL(1, "clearLanceConfiguration valid:" + clearLanceConfiguration.LanceValid);
      foreach (string key in clearLanceConfiguration.Lances.Keys) {
        if (!string.IsNullOrEmpty(key)) {
          SpawnableUnit[] lanceUnits = clearLanceConfiguration.GetLanceUnits(key);
          selectedContract.Lances.AddUnits((IEnumerable<SpawnableUnit>)lanceUnits);
        }
      }
      return false;
    }
  }
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("OnLanceConfiguratorAccept")]
  [HarmonyPatch(MethodType.Normal)]
  public static class SimGameState_OnLanceConfiguratorAccept {
    private static MethodInfo m_SaveLastLance = typeof(SimGameState).GetMethod("SaveLastLance", BindingFlags.NonPublic | BindingFlags.Instance);
    public static void SaveLastLance(this SimGameState sim, LanceConfiguration lanceConfiguration) {
      m_SaveLastLance.Invoke(sim, new object[] { lanceConfiguration });
    }
    private static MethodInfo m_OnContractReady = typeof(SimGameState).GetMethod("OnContractReady", BindingFlags.NonPublic | BindingFlags.Instance);
    public static void OnContractReady(this SimGameState sim, LanceConfiguration lanceConfiguration) {
      m_OnContractReady.Invoke(sim, new object[] { lanceConfiguration });
    }
    public static LanceConfiguration clearConfig(this LanceConfiguration lanceConfiguration) {
      LanceConfiguration result = new LanceConfiguration();
      foreach (var lance in lanceConfiguration.Lances) {
        foreach (var unit in lance.Value) {
          if (unit.Unit == null) { continue; }
          if (unit.Pilot == null) { continue; }
          result.AddUnit(unit);
        }
      }
      return result;
    }
    public static bool Prefix(SimGameState __instance) {
      Log.TWL(0, "SimGameState.OnLanceConfiguratorAccept");
      LanceConfiguration lanceConfiguration = __instance.RoomManager.CmdCenterRoom.lanceConfigBG.LC.CreateLanceConfiguration();
      Log.WL(1, "lanceConfiguration created " + (lanceConfiguration == null ? "null" : "not null"));
      __instance.SaveLastLance(lanceConfiguration);
      LanceConfiguration clearLanceConfiguration = lanceConfiguration.clearConfig();
      Log.WL(1, "clearLanceConfiguration valid:" + clearLanceConfiguration.LanceValid);
      foreach (var lance in clearLanceConfiguration.Lances) {
        foreach (var unit in lance.Value) {
          Log.WL(2, "unit:" + unit.PilotId + ":" + unit.UnitId + ":" + unit.CanLoad());
        }
      }
      __instance.OnContractReady(clearLanceConfiguration);
      return false;
    }
  }
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("GetLastLance")]
  [HarmonyPatch(MethodType.Normal)]
  public static class SimGameState_GetLastLance {
    public static bool Prefix(SimGameState __instance, ref LanceConfiguration __result, ref List<string> ___LastUsedMechs, ref List<string> ___LastUsedPilots) {
      LanceConfiguration lanceConfiguration = new LanceConfiguration();
      Log.TWL(0, "SimGameState.GetLastLance");
      if ((___LastUsedMechs == null) || (___LastUsedPilots == null)) {
        Log.WL(1, "LastUsed arrays is null");
        __result = lanceConfiguration;
        return false;
      }
      Log.WL(1, "pilots:");
      for (int i = 0; i < ___LastUsedPilots.Count; ++i) {
        Log.WL(2, "[" + i + "] " + ___LastUsedPilots[i]);
      }
      Log.WL(1, "units:");
      for (int i = 0; i < ___LastUsedMechs.Count; ++i) {
        Log.WL(2, "[" + i + "] " + ___LastUsedMechs[i]);
      }
      if (___LastUsedMechs != null && ___LastUsedPilots != null) {
        int counter = Mathf.Max(___LastUsedMechs.Count, ___LastUsedPilots.Count);
        for (int index = 0; index < counter; ++index) {
          string id = string.Empty;
          if (___LastUsedMechs.Count > index) { id = ___LastUsedMechs[index]; };
          string pilotID = string.Empty;
          if (___LastUsedPilots.Count > index) { pilotID = ___LastUsedPilots[index]; }
          MechDef mechById = __instance.GetMechByID(id);
          PilotDef pilotDef = __instance.GetPilot(pilotID)?.pilotDef;
          if (mechById != null || pilotDef != null) {
            lanceConfiguration.AddUnit("bf40fd39-ccf9-47c4-94a6-061809681140", (PilotableActorDef)mechById, pilotDef);
          } else {
            lanceConfiguration.AddUnit("bf40fd39-ccf9-47c4-94a6-061809681140", string.Empty, string.Empty, UnitType.Mech);
          }
        }
      }
      __result = lanceConfiguration;
      return false;
    }
  }
  public static class CustomLanceHelper {
    public static readonly string SaveReferenceName = "PlayedAllyLanceContent";
    public static PlayerLancesLoadout playerLanceLoadout = new PlayerLancesLoadout();
    private static List<CustomLanceDef> lancesData = new List<CustomLanceDef>(new CustomLanceDef[] { new CustomLanceDef(4, 4, false) });
    //private static List<int> allowLanceSizes = new List<int>(new int[] { 5, 3 });
    private static int fallbackAllow = -1;
    private static int f_overallDeployCount = 4;
    private static int f_playerControlMechs = -1;
    private static int f_playerControlVehicles = -1;
    private static bool missionControlDetected = false;
    private static int mechBaysCount = 3;
    private static int vehicleBaysCount = 0;
    private static int rowsPerList = 3;
    public static int RowsPerList(this MechBayPanel panel) { return rowsPerList; }
    public static int MechBaysCount(this MechBayPanel panel) { return mechBaysCount; }
    public static int VehicleBaysCount(this MechBayPanel panel) { return Core.Settings.ShowVehicleBays ? vehicleBaysCount : 0; }
    public static int RowsPerList(this MechBayRowGroupWidget panel) { return rowsPerList; }
    public static int MechBaysCount(this MechBayRowGroupWidget panel) { return mechBaysCount; }
    public static int VehicleBaysCount(this MechBayRowGroupWidget panel) { return Core.Settings.ShowVehicleBays ? vehicleBaysCount : 0; }
    public static void BaysCount(int count) { mechBaysCount = count; vehicleBaysCount = count; }
    public static void MissionControlDetected() { missionControlDetected = true; }
    public static void setLancesCount(int size) {
      if (size <= 0) { size = 1; };
      if (lancesData.Count > size) {
        lancesData.RemoveRange(size, lancesData.Count - size);
      } else if (lancesData.Count < size) {
        for (int index = lancesData.Count; index < size; ++index) {
          lancesData.Add(new CustomLanceDef());
        }
      }
    }
    public static void setLanceData(int lanceid, int size, int allow, bool is_vehicle) {
      if (lanceid >= lancesData.Count) { return; }
      if (size < 4) { size = 4; }
      if (allow < 0) { allow = 0; }
      lancesData[lanceid].size = size;
      lancesData[lanceid].allow = allow;
      lancesData[lanceid].is_vehicle = is_vehicle;
    }
    public static void setOverallDeployCount(int value) { f_overallDeployCount = Mathf.Min(value, fullSlotsCount()); }
    public static void playerControl(int mechs, int vehicles) { f_playerControlMechs = mechs; f_playerControlVehicles = vehicles; }
    public static int overallDeployCount() { return missionControlDetected ? f_overallDeployCount : 4; }
    public static int playerControlVehicles() { return missionControlDetected ? f_playerControlVehicles : -1; }
    public static int playerControlMechs() { return missionControlDetected ? f_playerControlMechs : -1; }

    public static int allowLanceSize(int lanceid) {
      if (fallbackAllow > 0) {
        Log.TWL(0, "allowLanceSize:"+lanceid+" allow value:"+ fallbackAllow);
        int result = fallbackAllow;
        for (int t = 0; t < lanceid; ++t) {
          result -= lancesData[t].allow;
          Log.WL(1,"Lance:"+t+" allow:"+ lancesData[t].allow+" result:"+ result);
          if (result <= 0) { return 0; }
        }
        return result > lancesData[lanceid].allow ? lancesData[lanceid].allow : result;
      } else {
        return lancesData[lanceid].allow;
      }
    }
    public static void setFallbackAllow(int value) { fallbackAllow = value; }
    public static void allowLanceSize(int lanceid, int size) { lancesData[lanceid].allow = size; }
    public static int lancesCount() { return lancesData.Count; }
    public static bool lanceVehicle(int lanceid) { return lancesData[lanceid].is_vehicle; }
    public static int lanceSize(int lanceid) { return lancesData[lanceid].size; }
    public static int fullSlotsCount() { int result = 0; foreach (CustomLanceDef def in lancesData) { result += def.size; }; return result; }
    public static Transform FindRecursive(this Transform transform, string checkName) {
      foreach (Transform t in transform) {
        if (t.name == checkName) return t;
        Transform possibleTransform = FindRecursive(t, checkName);
        if (possibleTransform != null) return possibleTransform;
      }
      return null;
    }
    public static string GetRelativePath(this Transform transform, Transform parent) {
      string result = string.Empty;
      Transform temp = transform;
      while((temp != parent)&&(temp != null)) {
        result = result + "." + temp.name;
        temp = temp.parent;
      }
      return result;
    }
    public static Transform FindByPath(this Transform parent, string path) {
      Transform[] transforms = parent.GetComponentsInChildren<Transform>(true);
      Transform result = null;
      foreach (Transform transform in transforms) { if (transform.GetRelativePath(parent) == path) { result = transform; break; } }
      return result;
    }
  }
  [HarmonyPatch(typeof(AAR_UnitsResult_Screen), "InitializeData")]
  public static class AAR_UnitsResult_Screen_InitializeSkirmishData {
    private static FieldInfo f_UnitWidgets = typeof(AAR_UnitsResult_Screen).GetField("UnitWidgets", BindingFlags.Instance | BindingFlags.NonPublic);
    public static List<AAR_UnitStatusWidget> UnitWidgets(this AAR_UnitsResult_Screen screen) { return (List<AAR_UnitStatusWidget>)f_UnitWidgets.GetValue(screen); }
    private static int startUnitPosition = 0;
    public static int CurViewStartPosition(this AAR_UnitsResult_Screen screen) { return startUnitPosition; }
    public static void CurViewStartPosition(this AAR_UnitsResult_Screen screen, int value) { startUnitPosition = value; }
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Method(typeof(AAR_UnitsResult_Screen), "InitializeWidgets");
      var replacementMethod = AccessTools.Method(typeof(AAR_UnitsResult_Screen_InitializeSkirmishData), nameof(InitializeWidgets));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    static void InitializeWidgets(this AAR_UnitsResult_Screen rscreen) {
      Log.TWL(0, "AAR_UnitsResult_Screen.InitializeWidgets", true);
      return;
    }
    static void Prefix(AAR_UnitsResult_Screen __instance, ref List<AAR_UnitStatusWidget> ___UnitWidgets) {
      Log.TWL(0, "AAR_UnitsResult_Screen.InitializeData", true);
      RectTransform rectTR = __instance.GetComponent<RectTransform>();
      Vector3[] corners = new Vector3[4];
      rectTR.GetLocalCorners(corners);

      Transform buttonPanel_SKIRMISH = __instance.transform.FindRecursive("buttonPanel-SKIRMISH");
      Transform buttonPanel = __instance.transform.FindRecursive("buttonPanel");
      if ((buttonPanel_SKIRMISH != null) && (buttonPanel != null)) {
        Transform lancePanel = __instance.transform.FindRecursive("lancePanel");
        if (lancePanel == null) {
          lancePanel = GameObject.Instantiate(buttonPanel_SKIRMISH.gameObject).transform;
          lancePanel.gameObject.name = "lancePanel";
          lancePanel.transform.SetParent(buttonPanel_SKIRMISH.transform.parent);
          lancePanel.gameObject.SetActive(true);
          Vector3 pos = Vector3.zero;
          pos.x = (corners[1].x + corners[2].x) / 2f;
          pos.y = buttonPanel.localPosition.y;
          pos.z = buttonPanel.localPosition.z;
          lancePanel.localPosition = pos;
          Transform Rematch_button2_custom = lancePanel.FindRecursive("Rematch_button2-custom");
          HBSDOTweenButton btn = Rematch_button2_custom.GetComponent<HBSDOTweenButton>();
          if (btn != null) { GameObject.Destroy(btn); };
          Rematch_button2_custom.gameObject.AddComponent<AARResultScreenButton>().Init(__instance, -4);
          Transform Finished_button2_custom = lancePanel.FindRecursive("Finished_button2-custom");
          btn = Finished_button2_custom.GetComponent<HBSDOTweenButton>();
          if (btn != null) { GameObject.Destroy(btn); };
          Finished_button2_custom.gameObject.AddComponent<AARResultScreenButton>().Init(__instance, 4);
        } else {
          Transform Rematch_button2_custom = lancePanel.FindRecursive("Rematch_button2-custom");
          Rematch_button2_custom.gameObject.GetComponent<AARResultScreenButton>().Init(__instance, -4);
          Transform Finished_button2_custom = lancePanel.FindRecursive("Finished_button2-custom");
          Finished_button2_custom.gameObject.GetComponent<AARResultScreenButton>().Init(__instance, 4);
        }
        //HorizontalLayoutGroup layout = buttonPanel_SKIRMISH.transform.parent.gameObject.GetComponent<HorizontalLayoutGroup>();
      }
      int unitWidgetsCount = Mathf.CeilToInt((float)CustomLanceHelper.fullSlotsCount() / 4.0f) * 4;
      AAR_UnitStatusWidget src = ___UnitWidgets[0];
      Log.WL(0, "unitWidgetsCount:" + unitWidgetsCount);
      for (int index = ___UnitWidgets.Count; index < unitWidgetsCount; ++index) {
        AAR_UnitStatusWidget dest = GameObject.Instantiate(src).GetComponent<AAR_UnitStatusWidget>();
        dest.transform.SetParent(src.transform.parent);
        dest.transform.localScale = src.transform.localScale;
        ___UnitWidgets.Add(dest);
      }
      for (int index = 0; index < ___UnitWidgets.Count; ++index) {
        ___UnitWidgets[index].gameObject.SetActive(index < 4);
      }
      startUnitPosition = 0;
    }
    static void Postfix(AAR_SkirmishResult_Screen __instance, ref List<AAR_UnitStatusWidget> ___UnitWidgets, ref List<UnitResult> ___UnitResults, ref int ___numUnits, ref Contract ___theContract, ref SimGameState ___simState) {
      ___UnitResults.Clear();
      for (int index = 0; index < ___UnitWidgets.Count; ++index) {
        if (index < ___numUnits) {
          ___UnitResults.Add(___theContract.PlayerUnitResults[index]);
        } else {
          ___UnitResults.Add((UnitResult)null);
        }
      }
      for (int index = 0; index < ___UnitWidgets.Count; ++index) {
        if (index < ___numUnits && ___UnitResults[index] != null) {
          ___UnitWidgets[index].InitData(___UnitResults[index], ___simState, ___simState.DataManager, ___theContract);
        } else {
          ___UnitWidgets[index].InitData((UnitResult)null, ___simState, ___simState.DataManager, ___theContract);
        }
      }
    }
  }
  [HarmonyPatch(typeof(AAR_SkirmishResult_Screen), "InitializeSkirmishData")]
  public static class AAR_SkirmishResult_Screen_InitializeSkirmishData {
    private static FieldInfo f_UnitWidgets = typeof(AAR_SkirmishResult_Screen).GetField("UnitWidgets", BindingFlags.Instance | BindingFlags.NonPublic);
    public static List<AAR_UnitStatusWidget> UnitWidgets(this AAR_SkirmishResult_Screen screen) { return (List<AAR_UnitStatusWidget>)f_UnitWidgets.GetValue(screen); }
    private static int startUnitPosition = 0;
    public static int CurViewStartPosition(this AAR_SkirmishResult_Screen screen) { return startUnitPosition; }
    public static void CurViewStartPosition(this AAR_SkirmishResult_Screen screen, int value) { startUnitPosition = value; }
    static void Prefix(AAR_SkirmishResult_Screen __instance, ref List<AAR_UnitStatusWidget> ___UnitWidgets) {
      Log.TWL(0, "AAR_SkirmishResult_Screen.InitializeSkirmishData", true);
      Transform unitDone_btn = __instance.transform.FindRecursive("uixPrfBttn_BASE_button2-done");
      if (unitDone_btn != null) {
        HorizontalLayoutGroup layout = unitDone_btn.transform.parent.gameObject.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 20f;
        Transform unitUp_btn = __instance.transform.FindRecursive("uixPrfBttn_BASE_button2-prev");
        if (unitUp_btn == null) {
          unitUp_btn = GameObject.Instantiate(unitDone_btn).transform;
          unitUp_btn.name = "uixPrfBttn_BASE_button2-prev";
          unitUp_btn.gameObject.name = "uixPrfBttn_BASE_button2-prev";
          unitUp_btn.SetParent(unitDone_btn.transform.parent);
          unitUp_btn.SetSiblingIndex(unitDone_btn.transform.GetSiblingIndex() - 1);
          unitUp_btn.gameObject.AddComponent<AARSkirmishScreenButton>().Init(__instance, -4);
        } else {
          unitUp_btn.gameObject.GetComponent<AARSkirmishScreenButton>().Init(__instance, -4);
        }
        Transform unitDown_btn = __instance.transform.FindRecursive("uixPrfBttn_BASE_button2-next");
        if (unitDown_btn == null) {
          unitDown_btn = GameObject.Instantiate(unitDone_btn).transform;
          unitDown_btn.name = "uixPrfBttn_BASE_button2-next";
          unitDown_btn.gameObject.name = "uixPrfBttn_BASE_button2-next";
          unitDown_btn.SetParent(unitDone_btn.transform.parent);
          unitDown_btn.SetSiblingIndex(unitDone_btn.transform.GetSiblingIndex() + 1);
          unitDown_btn.gameObject.AddComponent<AARSkirmishScreenButton>().Init(__instance, 4);
        } else {
          unitDown_btn.gameObject.GetComponent<AARSkirmishScreenButton>().Init(__instance, 4);
        }
      }
      int unitWidgetsCount = Mathf.CeilToInt((float)CustomLanceHelper.fullSlotsCount() / 4.0f) * 4;
      AAR_UnitStatusWidget src = ___UnitWidgets[0];
      Log.WL(0, "unitWidgetsCount:" + unitWidgetsCount);
      for (int index = ___UnitWidgets.Count; index < unitWidgetsCount; ++index) {
        AAR_UnitStatusWidget dest = GameObject.Instantiate(src).GetComponent<AAR_UnitStatusWidget>();
        dest.transform.SetParent(src.transform.parent);
        dest.transform.localScale = src.transform.localScale;
        ___UnitWidgets.Add(dest);
      }
      for (int index = 0; index < ___UnitWidgets.Count; ++index) {
        ___UnitWidgets[index].gameObject.SetActive(index < 4);
      }
      startUnitPosition = 0;
    }
    static void Postfix(AAR_SkirmishResult_Screen __instance, ref List<AAR_UnitStatusWidget> ___UnitWidgets, ref List<UnitResult> ___PlayerUnitResults, ref List<UnitResult> ___OpponentUnitResults, ref int ___numUnits, ref int ___numOpponentUnits, ref Contract ___theContract, ref SimGameState ___simState, ref DataManager ___dm) {
      ___PlayerUnitResults.Clear();
      for (int index = 0; index < ___UnitWidgets.Count; ++index) {
        if (index < ___numUnits) {
          ___PlayerUnitResults.Add(___theContract.PlayerUnitResults[index]);
        } else {
          ___PlayerUnitResults.Add((UnitResult)null);
        }
      }
      ___OpponentUnitResults.Clear();
      for (int index = 0; index < ___UnitWidgets.Count; ++index) {
        if (index < ___numOpponentUnits) {
          ___OpponentUnitResults.Add(___theContract.OpponentUnitResults[index]);
        } else {
          ___OpponentUnitResults.Add((UnitResult)null);
        }
      }
      for (int index = 0; index < ___UnitWidgets.Count; ++index) {
        if (index < ___numUnits && ___PlayerUnitResults[index] != null) {
          ___UnitWidgets[index].InitData(___PlayerUnitResults[index], ___simState, ___dm, ___theContract);
        } else {
          ___UnitWidgets[index].InitData((UnitResult)null, ___simState, ___dm, ___theContract);
        }
      }
    }
  }
  [HarmonyPatch(typeof(AAR_UnitsResult_Screen), "FillInData")]
  public static class AAR_UnitsResult_Screen_InitializeWidgets {
    static bool Prefix(AAR_UnitsResult_Screen __instance, ref List<AAR_UnitStatusWidget> ___UnitWidgets, ref List<UnitResult> ___UnitResults, ref Contract ___theContract) {
      Log.TWL(0, "AAR_UnitsResult_Screen.FillInData");
      int experienceEarned = ___theContract.ExperienceEarned;
      for (int index = 0; index < ___UnitWidgets.Count; ++index) {
        ___UnitWidgets[index].SetMechIconValueTextActive(false);
        if (___UnitResults[index] != null) {
          ___UnitWidgets[index].SetNoUnitDeployedOverlayActive(false);
          //if (___UnitResults[index].pilot.pilotDef.isVehicleCrew() == false) {
          //if (___UnitResults[index].mech.IsChassisFake() == false) {
          ___UnitWidgets[index].FillInData(experienceEarned);
          //} else {
          //___UnitWidgets[index].FillInData(0);
          //}
        } else {
          ___UnitWidgets[index].SetNoUnitDeployedOverlayActive(true);
        }
      }
      return false;
    }
  }
  [HarmonyPatch(typeof(AAR_SkirmishResult_Screen), "ShowMyResults")]
  public static class AAR_SkirmishResult_Screen_ShowMyResults {
    static bool Prefix(LancePreviewPanel __instance, ref List<AAR_UnitStatusWidget> ___UnitWidgets, ref List<UnitResult> ___PlayerUnitResults) {
      Log.TWL(0, "AAR_SkirmishResult_Screen.ShowMyResults");
      for (int index = 0; index < ___UnitWidgets.Count; ++index) {
        ___UnitWidgets[index].SetMechIconValueTextActive(false);
        if (___PlayerUnitResults[index] != null) {
          ___UnitWidgets[index].SetNoUnitDeployedOverlayActive(false);
          ___UnitWidgets[index].SetUnitData(___PlayerUnitResults[index]);
          ___UnitWidgets[index].FillInSkirmishData();
        } else {
          ___UnitWidgets[index].SetNoUnitDeployedOverlayActive(true);
        }
      }
      return false;
    }
  }
  [HarmonyPatch(typeof(PilotDef), "SetUnspentExperience")]
  public static class PilotDef_SetUnspentExperience {
    static bool Prefix(PilotDef __instance, ref int value) {
      //if (__instance.canPilotMech() == false) { value = 0; };
      Log.TWL(0, "PilotDef.SetUnspentExperience canPilotMech:" + __instance.canPilotMech() + " value:" + value);
      return true;
    }
  }
  [HarmonyPatch(typeof(Pilot), "AddExperience")]
  public static class Pilot_AddExperience {
    static bool Prefix(Pilot __instance, ref int value) {
      //if (__instance.pilotDef.canPilotMech() == false) {
      //value = 0;
      //__instance.StatCollection.Set("ExperienceUnspent",0);
      //};
      Log.TWL(0, "Pilot.AddExperience canPilotMech:" + __instance.pilotDef.canPilotMech() + " value:" + value + " unspent:" + __instance.UnspentXP);
      return true;
    }
  }
  [HarmonyPatch(typeof(LanceConfiguratorPanel), "OnConfirmClicked")]
  public static class LanceConfiguratorPanel_OnConfirmClicked {
    private static FieldInfo f_interruptQueue = typeof(SimGameState).GetField("interruptQueue", BindingFlags.Instance | BindingFlags.NonPublic);
    public static SimGameInterruptManager interruptQueue(this SimGameState sim) { return (SimGameInterruptManager)f_interruptQueue.GetValue(sim); }
    static bool Prefix(LanceConfiguratorPanel __instance, ref LanceLoadoutSlot[] ___loadoutSlots) {
      Log.TWL(0, "LanceConfiguratorPanel.OnConfirmClicked");
      if ((__instance.sim != null) && (__instance.activeContract != null)) {
        bool badBiome = false;
        string forbidTag = "NoBiome_" + __instance.activeContract.ContractBiome;
        Log.WL(1, "SimGameState exists and activeContract too. Biome tag:" + forbidTag);
        foreach (LanceLoadoutSlot slot in ___loadoutSlots) {
          if (slot.SelectedMech != null) {
            //if (slot.SelectedMech.MechDef.MechTags.Contains(forbidTag)) { badBiome = true; break; }
            foreach (MechComponentRef component in slot.SelectedMech.MechDef.Inventory) {
              if (component.Def.ComponentTags.Contains(forbidTag)) { badBiome = true; break; }
            }
          };
        }
        if (badBiome) {
          Localize.Text text = new Localize.Text("HEY VONKER!!");
          Localize.Text message = new Localize.Text("I.C.E. engines not working in vacuum. VTOLs and hovers also can't operate in thin atmosphere or without it");
          __instance.sim.interruptQueue().QueuePauseNotification(text.ToString(true), message.ToString(true), __instance.sim.GetCrewPortrait(SimGameCrew.Crew_Yang), "notification_mechreadycomplete", (Action)(() => {
          }), "Continue", (Action)null, (string)null);
          return false;
        }
      }
      int overallSlotsCount = 0;
      if (overallSlotsCount > CustomLanceHelper.overallDeployCount()) {
        GenericPopupBuilder.Create("Lance Cannot Be Deployed", Strings.T("Deploying units count {0} exceed limit {1}", overallSlotsCount, CustomLanceHelper.overallDeployCount())).AddFader(new UIColorRef?(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PopupBackfill), 0.0f, true).Render();
        return false;
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("ResolveCompleteContract")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class SimGameState_ResolveCompleteContract {
    private static List<UnitResult> PlayerUnitResults = null;
    public static bool Prefix(SimGameState __instance) {
      PlayerUnitResults = __instance.CompletedContract.PlayerUnitResults;
      return true;
    }
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Method(typeof(PilotDef), "SetUnspentExperience");
      var replacementMethod = AccessTools.Method(typeof(SimGameState_ResolveCompleteContract), nameof(SetUnspentExperience));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }

    public static void SetUnspentExperience(this PilotDef pilotDef, int value) {
      Log.TWL(0, "SimGameState.ResolveCompleteContract.PilotDef.SetUnspentExperience " + pilotDef.Description.Id + " PlayerUnitResults:" + (PlayerUnitResults == null ? "null" : PlayerUnitResults.Count.ToString()) + " crew:" + pilotDef.canPilotMech() + " value:" + value);
      if (PlayerUnitResults == null) {
        //if (pilotDef.canPilotMech() == false) {
        //pilotDef.SetUnspentExperience(0);
        //} else {
        pilotDef.SetUnspentExperience(value);
        //}
      } else {
        UnitResult pilotResult = null;
        foreach (UnitResult result in PlayerUnitResults) {
          if (result.pilot.Description.Id == pilotDef.Description.Id) {
            pilotResult = result; break;
          }
        }
        if (pilotResult == null) {
          Log.WL(1, "cant find pilot result for:" + pilotDef.Description.Id);
          //if (pilotDef.canPilotMech() == false) {
          //pilotDef.SetUnspentExperience(0);
          //} else {
          pilotDef.SetUnspentExperience(value);
          //}
        } else {
          Log.WL(1, "success find result for:" + pilotDef.Description.Id + " unit:" + pilotResult.mech.ChassisID + " idFake:" + pilotResult.mech.IsVehicle());
          //if (pilotResult.mech.IsChassisFake()) {
          //pilotDef.SetUnspentExperience(0);
          //} else {
          pilotDef.SetUnspentExperience(value);
          //}
        }
      }
    }
  }
  [HarmonyPatch(typeof(AAR_SkirmishResult_Screen), "ShowOpponentResults")]
  public static class AAR_SkirmishResult_Screen_ShowOpponentResults {
    static bool Prefix(LancePreviewPanel __instance, ref List<AAR_UnitStatusWidget> ___UnitWidgets, ref List<UnitResult> ___OpponentUnitResults) {
      Log.TWL(0, "AAR_SkirmishResult_Screen.ShowOpponentResults");
      for (int index = 0; index < ___UnitWidgets.Count; ++index) {
        ___UnitWidgets[index].SetMechIconValueTextActive(false);
        if (___OpponentUnitResults[index] != null) {
          ___UnitWidgets[index].SetNoUnitDeployedOverlayActive(false);
          ___UnitWidgets[index].SetUnitData(___OpponentUnitResults[index]);
          ___UnitWidgets[index].FillInSkirmishData();
        } else {
          ___UnitWidgets[index].SetNoUnitDeployedOverlayActive(true);
        }
      }
      return false;
    }
  }
  [HarmonyPatch(typeof(LanceConfiguratorPanel))]
  [HarmonyPatch("LoadLanceConfiguration")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(LanceConfiguration) })]
  public static class LanceConfiguratorPanel_LoadLanceConfiguration {
    internal class LoadoutContent {
      public IMechLabDraggableItem mech;
      public SGBarracksRosterSlot pilot;
      public LoadoutContent(IMechLabDraggableItem m, SGBarracksRosterSlot p) { mech = m; pilot = p; }
    }
    public static bool Prefix(LanceConfiguratorPanel __instance, LanceConfiguration config, LanceLoadoutSlot[] ___loadoutSlots, LanceHeaderWidget ___headerWidget) {
      Log.TWL(0, "LanceConfiguratorPanel.LoadLanceConfiguration:");
      ___headerWidget.LanceName = Strings.T(__instance.oldLanceName);
      SpawnableUnit[] lanceUnits = config.GetLanceUnits("");
      for (int i = 0; i < lanceUnits.Length; ++i) {
        Log.WL(1, "[" + i + "] pilot:" + lanceUnits[i].PilotId + " unit:" + lanceUnits[i].UnitId);
      }
      List<LoadoutContent> spawnMechList = new List<LoadoutContent>();
      List<LoadoutContent> spawnVehicleList = new List<LoadoutContent>();
      int count = Mathf.Min(lanceUnits.Length, ___loadoutSlots.Length);
      for (int i = 0; i < count; ++i) {
        SpawnableUnit unit = lanceUnits[i];
        IMechLabDraggableItem forcedMech = (IMechLabDraggableItem)null;
        if (unit.Unit != null) { forcedMech = __instance.mechListWidget.GetMechDefByGUID(unit.Unit.GUID); }
        if (forcedMech == null) { forcedMech = __instance.mechListWidget.GetInventoryItem(unit.UnitId); }
        if (forcedMech != null && !MechValidationRules.ValidateMechCanBeFielded(__instance.Sim, forcedMech.MechDef)) {
          forcedMech = (IMechLabDraggableItem)null;
        }
        SGBarracksRosterSlot forcedPilot = null;
        forcedPilot = __instance.pilotListWidget.GetPilot(unit.PilotId);
        if (forcedPilot != null) {
          if (forcedPilot.Pilot.CanPilot == false) {
            forcedPilot = (SGBarracksRosterSlot)null;
          }
          //  __instance.pilotListWidget.RemovePilot(__instance.availablePilots.Find((Predicate<Pilot>)(x => x.Description.Id == unit.PilotId)));
        }
        LanceLoadoutSlot loadoutSlot = ___loadoutSlots[i];
        if (loadoutSlot.isMechLocked) { forcedMech = null; }
        if (loadoutSlot.isPilotLocked) { forcedPilot = null; }
        if (loadoutSlot.isForVehicle()) {
          if (forcedMech != null) { if (forcedMech.MechDef.IsVehicle() == false) { forcedMech = null; }; };
          if (forcedPilot != null) { if (forcedPilot.Pilot.pilotDef.canPilotVehicle() == false) { forcedPilot = null; }; };
        } else {
          if ((forcedMech != null) && (forcedPilot != null)) {
            if (forcedMech.MechDef.IsVehicle() && (forcedPilot.Pilot.pilotDef.canPilotVehicle() == false)) { forcedPilot = null; }
            if ((forcedMech.MechDef.IsVehicle() == false) && (forcedPilot.Pilot.pilotDef.canPilotMech() == false)) { forcedPilot = null; }
          }
        }
        if (forcedPilot != null) { __instance.pilotListWidget.RemovePilot(__instance.availablePilots.Find((Predicate<Pilot>)(x => x.Description.Id == forcedPilot.Pilot.Description.Id))); }
        loadoutSlot.SetLockedData(forcedMech, forcedPilot, false);
      }
      return false;
    }
  }
  [HarmonyPatch(typeof(LanceConfiguratorPanel), "CreateLanceDef")]
  public static class LanceConfiguratorPanel_CreateLanceDef {
    static bool Prefix(LanceConfiguratorPanel __instance, string lanceId, LanceLoadoutSlot[] ___loadoutSlots, LanceHeaderWidget ___headerWidget, ref LanceDef __result) {
      Log.TWL(0, "LanceConfiguratorPanel.CreateLanceDef " + lanceId + " slots:" + ___loadoutSlots.Length, true);
      string lanceName = ___headerWidget.LanceName;
      DescriptionDef description = new DescriptionDef(lanceId, lanceName, "", "", __instance.currentLanceValue, 0.0f, false, "", "", "");
      TagSet lanceTags = new TagSet(new string[4]
      {
        "lance_type_custom",
        "lance_release",
        "lance_bracket_skirmish",
        MechValidationRules.GetLanceBracketTag(__instance.currentLanceValue)
      });
      List<LanceDef.Unit> unitList = new List<LanceDef.Unit>();
      for (int index = 0; index < ___loadoutSlots.Length; ++index) {
        LanceLoadoutSlot loadoutSlot = ___loadoutSlots[index];
        if ((UnityEngine.Object)loadoutSlot.SelectedMech != (UnityEngine.Object)null && (UnityEngine.Object)loadoutSlot.SelectedPilot != (UnityEngine.Object)null) {
          unitList.Add(new LanceDef.Unit() {
            unitType = UnitType.Mech,
            unitId = loadoutSlot.SelectedMech.MechDef.Description.Id,
            pilotId = loadoutSlot.SelectedPilot.Pilot.pilotDef.Description.Id
          });
        } else {
          unitList.Add(new LanceDef.Unit() {
            unitType = UnitType.Mech,
            unitId = string.Empty,
            pilotId = string.Empty
          });
        }
      }
      __result = new LanceDef(description, 0, lanceTags, unitList.ToArray());
      return false;
    }
    static void Postfix(LanceConfiguratorPanel __instance, string lanceId, ref LanceDef __result) {
      LanceLoadoutSlot[] loadoutSlots = (LanceLoadoutSlot[])AccessTools.Field(typeof(LanceConfiguratorPanel), "loadoutSlots").GetValue(__instance);
      Log.TWL(0, "LanceConfiguratorPanel.CreateLanceDef result:", true);
      Log.WL(0, __result.ToJSON());
    }
  }
  [HarmonyPatch(typeof(LancePreviewPanel), "OnLanceConfiguratorConfirm")]
  public static class LancePreviewPanel_OnLanceConfiguratorConfirm {
    static void Prefix(LancePreviewPanel __instance) {
      Log.TWL(0, "LancePreviewPanel.OnLanceConfiguratorConfirm", true);
    }
  }
  [HarmonyPatch(typeof(LancePreviewPanel), "OnLanceConfiguratorCancel")]
  public static class LancePreviewPanel_OnLanceConfiguratorCancel {
    static void Prefix(LancePreviewPanel __instance) {
      Log.TWL(0, "LancePreviewPanel.OnLanceConfiguratorCancel", true);
    }
  }
  [HarmonyPatch(typeof(LanceLoadoutSlot))]
  [HarmonyPatch("SetData")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(LanceConfiguratorPanel), typeof(SimGameState), typeof(DataManager), typeof(bool), typeof(bool), typeof(float), typeof(float) })]
  public static class LanceLoadoutSlot_SetData {
    public static void Prefix(LanceLoadoutSlot __instance, LanceConfiguratorPanel LC, SimGameState sim, DataManager dataManager, bool useDragAndDrop, ref bool locked, ref float minTonnage, ref float maxTonnage) {
      Log.TWL(0, "LanceLoadoutSlot.SetData " + __instance.GetInstanceID());
      try {
        bool overrideLocked = __instance.isOverrideLockedState();
        if (overrideLocked == false) { return; }
        int lanceid = __instance.LanceId();
        int lancepos = __instance.LancePos();
        int index = __instance.slotIndex();
        Log.WL(1, "index:" + index + " lanceid:" + lanceid + " lancepos:" + lancepos + " maxTonnage:" + maxTonnage + " locked:" + locked + " allow:" + (lanceid == -1 ? "unlim" : CustomLanceHelper.allowLanceSize(lanceid).ToString()));
        if ((lanceid == -1) || (lancepos == -1)) { return; }
        if (maxTonnage == 0f) { return; }
        if (locked == true) { return; }
        locked = CustomLanceHelper.allowLanceSize(lanceid) <= lancepos;
        Log.WL(1, "locked:" + locked + " allow:" + CustomLanceHelper.allowLanceSize(lanceid));
        if (index >= 4) {
          Log.WL(1, "extended slot detected");
          if (locked == false) {
            locked = LC.slotMaxTonnages[0] == 0f;
          }
          minTonnage = LC.slotMinTonnages[0];
          maxTonnage = LC.slotMaxTonnages[0];
          Log.WL(1, "min max tonnage override. Min:" + minTonnage + " Max:" + maxTonnage + " lock state:" + locked);
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(LancePreviewPanel), "SetData")]
  public static class LancePreviewPanel_SetData {
    static void Prefix(LancePreviewPanel __instance, ref int maxUnits) {
      try {
        maxUnits = CustomLanceHelper.fullSlotsCount();
        GameObject srcSlot = __instance.loadoutSlots[0].gameObject;
        List<LanceLoadoutSlot> slots = new List<LanceLoadoutSlot>();
        slots.AddRange(__instance.loadoutSlots);
        for (int t = __instance.loadoutSlots.Length; t < maxUnits; ++t) {
          GameObject newSlot = GameObject.Instantiate(srcSlot, srcSlot.transform.parent);
          slots.Add(newSlot.GetComponent<LanceLoadoutSlot>());
        }
        __instance.loadoutSlots = slots.ToArray();
        int lance_id = 0;
        int lance_index = 0;
        for (int index = 0; index < slots.Count; ++index) {
          slots[index].isForVehicle(CustomLanceHelper.lanceVehicle(lance_id));
          slots[index].LanceId(lance_id);
          slots[index].LancePos(lance_index);
          slots[index].slotIndex(index);
          slots[index].isOverrideLockedState(false);
          ++lance_index;
          if (lance_index >= CustomLanceHelper.lanceSize(lance_id)) { ++lance_id; lance_index = 0; };
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
      }
    }
  }
  [HarmonyPatch(typeof(LanceLoadoutSlot))]
  [HarmonyPatch("OnAddItem")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(IMechLabDraggableItem), typeof(bool) })]
  public static class LanceLoadoutSlot_OnAddItem {
    public static bool canPilotVehicle(this PilotDef pilotDef) { return pilotDef.PilotTags.Contains(Core.Settings.CanPilotVehicleTag); }
    public static bool canPilotMech(this PilotDef pilotDef) { return !pilotDef.PilotTags.Contains(Core.Settings.CannotPilotMechTag); }
    public static bool Prefix(LanceLoadoutSlot __instance, IMechLabDraggableItem item, bool validate, bool __result, LanceConfiguratorPanel ___LC) {
      Log.TWL(0, "LanceLoadoutSlot.OnAddItem " + __instance.GetInstanceID());
      try {
        if ((item.ItemType != MechLabDraggableItemType.Mech) && (item.ItemType != MechLabDraggableItemType.Pilot)) { return true; }
        bool isVehicle = __instance.isForVehicle();
        if (item.ItemType == MechLabDraggableItemType.Mech) {
          LanceLoadoutMechItem lanceLoadoutMechItem = item as LanceLoadoutMechItem;
          if (isVehicle) {
            if (lanceLoadoutMechItem.MechDef.IsVehicle() == false) {
              if (___LC != null) { ___LC.ReturnItem(item); }
              __result = false;
              GenericPopupBuilder.Create("Cannot place mech", Strings.T("Mech cannot be placed to vehicle slot")).AddFader(new UIColorRef?(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PopupBackfill), 0.0f, true).Render();
              return false;
            }
          } else if (__instance.SelectedPilot != null) {
            if (lanceLoadoutMechItem.MechDef.IsVehicle() == false) {
              if (__instance.SelectedPilot.Pilot.pilotDef.canPilotMech() == false) {
                if (___LC != null) { ___LC.ReturnItem(item); }
                __result = false;
                GenericPopupBuilder.Create("Cannot place mech", Strings.T("This pilot can't pilot mech")).AddFader(new UIColorRef?(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PopupBackfill), 0.0f, true).Render();
                return false;
              }
            } else {
              if (__instance.SelectedPilot.Pilot.pilotDef.canPilotVehicle() == false) {
                if (___LC != null) { ___LC.ReturnItem(item); }
                __result = false;
                GenericPopupBuilder.Create("Cannot place mech", Strings.T("This pilot can't pilot vehicle")).AddFader(new UIColorRef?(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PopupBackfill), 0.0f, true).Render();
                return false;
              }
            }
          }
        } else if (item.ItemType == MechLabDraggableItemType.Pilot) {
          SGBarracksRosterSlot barracksRosterSlot = item as SGBarracksRosterSlot;
          bool canPilotVehicle = barracksRosterSlot.Pilot.pilotDef.canPilotVehicle();
          bool canPilotMech = barracksRosterSlot.Pilot.pilotDef.canPilotMech();
          if (isVehicle) {
            if (canPilotVehicle == false) {
              if (___LC != null) { ___LC.ReturnItem(item); }
              __result = false;
              GenericPopupBuilder.Create("Cannot place pilot", Strings.T("This pilot can't pilot vehicles")).AddFader(new UIColorRef?(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PopupBackfill), 0.0f, true).Render();
              return false;
            }
          } else {
            if (__instance.SelectedMech != null) {
              if (__instance.SelectedMech.MechDef.IsVehicle() == false) {
                if (canPilotMech == false) {
                  if (___LC != null) { ___LC.ReturnItem(item); }
                  __result = false;
                  GenericPopupBuilder.Create("Cannot place pilot", Strings.T("This pilot can't pilot mech")).AddFader(new UIColorRef?(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PopupBackfill), 0.0f, true).Render();
                  return false;
                }
              } else if (canPilotVehicle == false) {
                if (___LC != null) { ___LC.ReturnItem(item); }
                __result = false;
                GenericPopupBuilder.Create("Cannot place pilot", Strings.T("This pilot can't pilot vehicles")).AddFader(new UIColorRef?(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PopupBackfill), 0.0f, true).Render();
                return false;
              }
            }
          }
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(LanceConfiguratorPanel), "SetData")]
  public static class LanceConfiguratorPanel_SetData {
    private static List<List<LanceLoadoutSlot>> lanceSlots = new List<List<LanceLoadoutSlot>>();
    private static List<LanceLoadoutSlot> fillerSlots = new List<LanceLoadoutSlot>();
    private static LanceConfigurationState lanceConfigState = new LanceConfigurationState();
    private static FieldInfo f_loadoutSlots = typeof(LanceConfiguratorPanel).GetField("loadoutSlots", BindingFlags.Instance | BindingFlags.NonPublic);
    public static LanceLoadoutSlot[] loadoutSlots(this LanceConfiguratorPanel panel) { return (LanceLoadoutSlot[])f_loadoutSlots.GetValue(panel); }
    public static Dictionary<LanceLoadoutSlot, bool> isVehicleSlot = new Dictionary<LanceLoadoutSlot, bool>();
    public static Dictionary<LanceLoadoutSlot, bool> isOverLockedState = new Dictionary<LanceLoadoutSlot, bool>();
    public static Dictionary<LanceLoadoutSlot, int> slotsLanceIds = new Dictionary<LanceLoadoutSlot, int>();
    public static Dictionary<LanceLoadoutSlot, int> slotsIndexes = new Dictionary<LanceLoadoutSlot, int>();
    public static Dictionary<LanceLoadoutSlot, int> slotsLancePos = new Dictionary<LanceLoadoutSlot, int>();
    public static Dictionary<LanceLoadoutSlot, bool> isFillerSlot = new Dictionary<LanceLoadoutSlot, bool>();
    public static bool isForVehicle(this LanceLoadoutSlot slot) { if (isVehicleSlot.TryGetValue(slot, out bool result)) { return result; } else { return false; }; }
    public static void isForVehicle(this LanceLoadoutSlot slot, bool val) { if (isVehicleSlot.ContainsKey(slot) == false) { isVehicleSlot.Add(slot, val); } else { isVehicleSlot[slot] = val; }; }
    public static bool isOverrideLockedState(this LanceLoadoutSlot slot) { if (isOverLockedState.TryGetValue(slot, out bool result)) { return result; } else { return false; }; }
    public static void isOverrideLockedState(this LanceLoadoutSlot slot, bool val) { if (isOverLockedState.ContainsKey(slot) == false) { isOverLockedState.Add(slot, val); } else { isOverLockedState[slot] = val; }; }
    public static bool isFiller(this LanceLoadoutSlot slot) { if (isFillerSlot.TryGetValue(slot, out bool result)) { return result; } else { return true; }; }
    public static int LanceId(this LanceLoadoutSlot slot) { if (slotsLanceIds.TryGetValue(slot, out int result)) { return result; } else { return -1; }; }
    public static void LanceId(this LanceLoadoutSlot slot, int val) { if (slotsLanceIds.ContainsKey(slot) == false) { slotsLanceIds.Add(slot, val); } else { slotsLanceIds[slot] = val; }; }
    public static int slotIndex(this LanceLoadoutSlot slot) { if (slotsIndexes.TryGetValue(slot, out int result)) { return result; } else { return -1; }; }
    public static void slotIndex(this LanceLoadoutSlot slot, int val) { if (slotsIndexes.ContainsKey(slot) == false) { slotsIndexes.Add(slot, val); } else { slotsIndexes[slot] = val; }; }
    public static int LancePos(this LanceLoadoutSlot slot) { if (slotsLancePos.TryGetValue(slot, out int result)) { return result; } else { return -1; }; }
    public static void LancePos(this LanceLoadoutSlot slot, int val) { if (slotsLancePos.ContainsKey(slot) == false) { slotsLancePos.Add(slot, val); } else { slotsLancePos[slot] = val; }; }
    public static void loadoutSlots(this LanceConfiguratorPanel panel, LanceLoadoutSlot[] value) { f_loadoutSlots.SetValue(panel, value); }
    public static void updateSlots() {
      Log.TWL(0, "LanceConfiguratorPanel.updateSlots " + lanceConfigState.lanceId + "-" + lanceConfigState.lanceStartPos);
      for (int lid = 0; lid < lanceSlots.Count; ++lid) {
        if (lanceConfigState.lanceId != lid) {
          Log.WL(1, "lanceId " + lid);
          foreach (LanceLoadoutSlot slot in lanceSlots[lid]) {
            Log.WL(2, slot.gameObject.name + " disable");
            slot.gameObject.SetActive(false);
          };
          continue;
        };
        int showedCount = 0;
        Log.WL(1, "lanceId " + lid);
        for (int lindex = 0; lindex < lanceSlots[lid].Count; ++lindex) {
          LanceLoadoutSlot slot = lanceSlots[lid][lindex];
          if (lindex < lanceConfigState.lanceStartPos) {
            Log.WL(2, slot.gameObject.name + " disable");
            slot.gameObject.SetActive(false);
            continue;
          };
          if (lindex > (lanceConfigState.lanceStartPos + 3)) {
            Log.WL(2, slot.gameObject.name + " disable");
            slot.gameObject.SetActive(false);
            continue;
          };
          Log.WL(2, slot.gameObject.name + " enable");
          slot.gameObject.SetActive(true);
          ++showedCount;
        }
        Log.WL(1, "filler");
        for (int t = 0; t < fillerSlots.Count; ++t) {
          Log.WL(1, fillerSlots[t].name + " " + (t > showedCount ? "enable" : "disable"));
          fillerSlots[t].gameObject.SetActive(t > showedCount);
        }
      }
    }
    static void Prefix(LanceConfiguratorPanel __instance, ref int maxUnits, Contract contract) {
      try {
        Log.TWL(0, "LanceConfiguratorPanel.SetData");
        lanceConfigState.Clear();
        fillerSlots.Clear();
        Transform buttons_tr = __instance.transform.FindRecursive("lanceSwitchButtons-layout");
        if (buttons_tr == null) {
          Log.WL(1, "lanceSwitchButtons-layout not found");
          buttons_tr = __instance.transform.FindRecursive("lanceSaveButtons-layout");
          if (buttons_tr == null) {
            Log.WL(1, "lanceSaveButtons-layout not found");
            return;
          };
          Transform DeployBttn_layout = __instance.transform.FindRecursive("DeployBttn-layout");
          Transform nbtn = GameObject.Instantiate(buttons_tr.gameObject).transform;
          nbtn.SetParent(buttons_tr.parent);
          nbtn.localPosition = buttons_tr.localPosition;
          if (DeployBttn_layout != null) {
            Vector3 pos = nbtn.localPosition;
            pos.y = DeployBttn_layout.localPosition.y;
            nbtn.localPosition = pos;
          }
          buttons_tr = nbtn;
          buttons_tr.gameObject.name = "lanceSwitchButtons-layout";
          buttons_tr.Find("uixPrfBttn_BASE_button2-MANAGED-delete").gameObject.AddComponent<LanceConfigutationNextLance>().Init(lanceConfigState);
          buttons_tr.Find("uixPrfBttn_BASE_button2-MANAGED-save").gameObject.AddComponent<LanceConfigutationPrevSlot>().Init(lanceConfigState);
          buttons_tr.Find("uixPrfBttn_BASE_button2-MANAGED-copy (1)").gameObject.AddComponent<LanceConfigutationNextSlot>().Init(lanceConfigState);
        } else {
          buttons_tr.Find("uixPrfBttn_BASE_button2-MANAGED-delete").gameObject.GetComponent<LanceConfigutationNextLance>().Init(lanceConfigState);
          buttons_tr.Find("uixPrfBttn_BASE_button2-MANAGED-save").gameObject.GetComponent<LanceConfigutationPrevSlot>().Init(lanceConfigState);
          buttons_tr.Find("uixPrfBttn_BASE_button2-MANAGED-copy (1)").gameObject.GetComponent<LanceConfigutationNextSlot>().Init(lanceConfigState);
        }
        buttons_tr.gameObject.SetActive(true);
        LanceLoadoutSlot[] loadoutSlots = (LanceLoadoutSlot[])typeof(LanceConfiguratorPanel).GetField("loadoutSlots", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
        float[] slotMaxTonnages = (float[])AccessTools.Field(typeof(LanceConfiguratorPanel), "slotMaxTonnages").GetValue(__instance);
        float[] slotMinTonnages = (float[])AccessTools.Field(typeof(LanceConfiguratorPanel), "slotMinTonnages").GetValue(__instance);
        List<float> listMaxTonnages = slotMaxTonnages.ToList();
        List<float> listMinTonnages = slotMinTonnages.ToList();
        Log.WL(0, "loadoutSlots:" + loadoutSlots.Length + "/" + CustomLanceHelper.fullSlotsCount());
        List<LanceLoadoutSlot> slots = new List<LanceLoadoutSlot>();
        slots.AddRange(loadoutSlots);
        //if (loadoutSlots.Length >= CustomLanceHelper.fullSlotsCount()) { return; }
        GameObject lanceSlotSrc = loadoutSlots[0].gameObject;
        for (int t = loadoutSlots.Length; t < CustomLanceHelper.fullSlotsCount() + 4; ++t) {
          GameObject lanceSlotNew = GameObject.Instantiate(lanceSlotSrc);
          lanceSlotNew.transform.SetParent(lanceSlotSrc.transform.parent);
          lanceSlotNew.name = "lanceSlot" + (t + 1).ToString();
          slots.Add(lanceSlotNew.GetComponent<LanceLoadoutSlot>());
          Log.WL(0, lanceSlotNew.name + " parent:" + lanceSlotNew.transform.parent.name);
          listMaxTonnages.Add(listMaxTonnages[0]);
          listMinTonnages.Add(listMinTonnages[0]);
        }
        for (int t = 0; t < slots.Count; ++t) { slots[t].gameObject.SetActive(false); }
        int lance_index = 0;
        int lance_id = 0;
        lanceSlots.Clear();
        List<LanceLoadoutSlot> lance = null;
        int maxLanceSize = CustomLanceHelper.fullSlotsCount();
        for (int index = 0; index < slots.Count; ++index) {
          LanceLoadoutSlot slot = slots[index];
          if (index >= maxLanceSize) { fillerSlots.Add(slot); continue; }
          if (lanceSlots.Count <= lance_id) {
            lance = new List<LanceLoadoutSlot>();
            lanceSlots.Add(lance);
            lance_index = 0;
          }
          lance.Add(slot);
          ++lance_index;
          if (lance_index >= CustomLanceHelper.lanceSize(lance_id)) { ++lance_id; };
        }
        maxUnits = maxLanceSize;
        if (contract == null) { CustomLanceHelper.setFallbackAllow(-1); } else {
          if (contract.IsFlashpointContract | contract.IsFlashpointCampaignContract) {
            CustomLanceHelper.setFallbackAllow(contract.Override.maxNumberOfPlayerUnits);
          } else if (contract.Override.maxNumberOfPlayerUnits > 0) {
            if (Core.Settings.CountContractMaxUnits4AsUnlimited && (contract.Override.maxNumberOfPlayerUnits == 4)) {
              CustomLanceHelper.setFallbackAllow(-1);
            } else {
              CustomLanceHelper.setFallbackAllow(contract.Override.maxNumberOfPlayerUnits);
            }
          } else {
            CustomLanceHelper.setFallbackAllow(-1);
          }
        }
        //maxUnits = 4;
        loadoutSlots = slots.ToArray();
        Log.WL(1, "loadoutSlots:" + slots.Count + "/" + maxUnits);
        __instance.loadoutSlots(slots.ToArray());
        AccessTools.Field(typeof(LanceConfiguratorPanel), "slotMaxTonnages").SetValue(__instance, listMaxTonnages.ToArray());
        AccessTools.Field(typeof(LanceConfiguratorPanel), "slotMinTonnages").SetValue(__instance, listMinTonnages.ToArray());
        //AccessTools.Field(typeof(LanceConfiguratorPanel), "loadoutSlots").SetValue(__instance, ___loadoutSlots);
        for (int lanceid = 0; lanceid < lanceSlots.Count; ++lanceid) {
          Log.WL(1, "lance " + lanceid);
          for (int lancepos = 0; lancepos < lanceSlots[lanceid].Count; ++lancepos) {
            Log.WL(2, "slot[" + lancepos + "]" + lanceSlots[lanceid][lancepos].gameObject.name + ":" + lanceSlots[lanceid][lancepos].GetInstanceID());
            lanceSlots[lanceid][lancepos].gameObject.transform.localScale = Vector3.one;
            if (isVehicleSlot.ContainsKey(lanceSlots[lanceid][lancepos])) { isVehicleSlot[lanceSlots[lanceid][lancepos]] = CustomLanceHelper.lanceVehicle(lanceid); } else {
              isVehicleSlot.Add(lanceSlots[lanceid][lancepos], CustomLanceHelper.lanceVehicle(lanceid));
            }
            if (slotsLanceIds.ContainsKey(lanceSlots[lanceid][lancepos])) { slotsLanceIds[lanceSlots[lanceid][lancepos]] = lanceid; } else {
              slotsLanceIds.Add(lanceSlots[lanceid][lancepos], lanceid);
            }
            if (slotsLancePos.ContainsKey(lanceSlots[lanceid][lancepos])) { slotsLancePos[lanceSlots[lanceid][lancepos]] = lancepos; } else {
              slotsLancePos.Add(lanceSlots[lanceid][lancepos], lancepos);
            }
            lanceSlots[lanceid][lancepos].transform.FindRecursive("mechDragLabel").FindRecursive("dragText").gameObject.GetComponent<LocalizableText>().SetText(
              CustomLanceHelper.lanceVehicle(lanceid) ? "Drag Vehicle HERE" : "Drag BattleMech HERE"
            );
            lanceSlots[lanceid][lancepos].transform.FindRecursive("mwSlot").FindRecursive("dragText").gameObject.GetComponent<LocalizableText>().SetText(
              CustomLanceHelper.lanceVehicle(lanceid) ? "Drag crew here" : "Drag mechwarrior here"
            );
            lanceSlots[lanceid][lancepos].isOverrideLockedState(true);
          }
        }
        Log.WL(1, "filler");
        for (int lancepos = 0; lancepos < fillerSlots.Count; ++lancepos) {
          Log.WL(2, "slot[" + lancepos + "]" + fillerSlots[lancepos].gameObject.name + ":" + fillerSlots[lancepos].GetInstanceID());
          fillerSlots[lancepos].gameObject.transform.localScale = Vector3.one;
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
    public static void Postfix(LanceConfiguratorPanel __instance) {
      Log.TWL(0, "LanceConfiguratorPanel.SetData:" + __instance.maxUnits + "/" + __instance.loadoutSlots().Length);
      updateSlots();
    }
  }
  [HarmonyPatch(typeof(ApplicationConstants))]
  [HarmonyPatch("FromJSON")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class ApplicationConstants_FromJSON {
    public static void Postfix(ApplicationConstants __instance) {
      Log.TWL(0, "ApplicationConstants.FromJSON. PrewarmRequests:");
      try {
        List<PrewarmRequest> prewarmRequests = new List<PrewarmRequest>();
        prewarmRequests.AddRange(__instance.PrewarmRequests);
        foreach (string icon in Core.Settings.LancesIcons) {
          prewarmRequests.Add(new PrewarmRequest(BattleTechResourceType.SVGAsset, icon));
          CustomSvgCache.RegisterSVG(icon);
        }
        prewarmRequests.Add(new PrewarmRequest(BattleTechResourceType.SVGAsset, Core.Settings.VehicleFrontArmorIcon)); CustomSvgCache.RegisterSVG(Core.Settings.VehicleFrontArmorIcon);
        prewarmRequests.Add(new PrewarmRequest(BattleTechResourceType.SVGAsset, Core.Settings.VehicleFrontArmorOutlineIcon)); CustomSvgCache.RegisterSVG(Core.Settings.VehicleFrontArmorOutlineIcon);
        prewarmRequests.Add(new PrewarmRequest(BattleTechResourceType.SVGAsset, Core.Settings.VehicleFrontStructureIcon)); CustomSvgCache.RegisterSVG(Core.Settings.VehicleFrontStructureIcon);
        prewarmRequests.Add(new PrewarmRequest(BattleTechResourceType.SVGAsset, Core.Settings.VehicleRearArmorIcon)); CustomSvgCache.RegisterSVG(Core.Settings.VehicleRearArmorIcon);
        prewarmRequests.Add(new PrewarmRequest(BattleTechResourceType.SVGAsset, Core.Settings.VehicleRearArmorOutlineIcon)); CustomSvgCache.RegisterSVG(Core.Settings.VehicleRearArmorOutlineIcon);
        prewarmRequests.Add(new PrewarmRequest(BattleTechResourceType.SVGAsset, Core.Settings.VehicleRearStructureIcon)); CustomSvgCache.RegisterSVG(Core.Settings.VehicleRearStructureIcon);
        prewarmRequests.Add(new PrewarmRequest(BattleTechResourceType.SVGAsset, Core.Settings.VehicleLeftArmorIcon)); CustomSvgCache.RegisterSVG(Core.Settings.VehicleLeftArmorIcon);
        prewarmRequests.Add(new PrewarmRequest(BattleTechResourceType.SVGAsset, Core.Settings.VehicleLeftArmorOutlineIcon)); CustomSvgCache.RegisterSVG(Core.Settings.VehicleLeftArmorOutlineIcon);
        prewarmRequests.Add(new PrewarmRequest(BattleTechResourceType.SVGAsset, Core.Settings.VehicleLeftStructureIcon)); CustomSvgCache.RegisterSVG(Core.Settings.VehicleLeftStructureIcon);
        prewarmRequests.Add(new PrewarmRequest(BattleTechResourceType.SVGAsset, Core.Settings.VehicleRightArmorIcon)); CustomSvgCache.RegisterSVG(Core.Settings.VehicleRightArmorIcon);
        prewarmRequests.Add(new PrewarmRequest(BattleTechResourceType.SVGAsset, Core.Settings.VehicleRightArmorOutlineIcon)); CustomSvgCache.RegisterSVG(Core.Settings.VehicleRightArmorOutlineIcon);
        prewarmRequests.Add(new PrewarmRequest(BattleTechResourceType.SVGAsset, Core.Settings.VehicleRightStructureIcon)); CustomSvgCache.RegisterSVG(Core.Settings.VehicleRightStructureIcon);
        prewarmRequests.Add(new PrewarmRequest(BattleTechResourceType.SVGAsset, Core.Settings.VehicleTurretArmorIcon)); CustomSvgCache.RegisterSVG(Core.Settings.VehicleTurretArmorIcon);
        prewarmRequests.Add(new PrewarmRequest(BattleTechResourceType.SVGAsset, Core.Settings.VehicleTurretArmorOutlineIcon)); CustomSvgCache.RegisterSVG(Core.Settings.VehicleTurretArmorOutlineIcon);
        prewarmRequests.Add(new PrewarmRequest(BattleTechResourceType.SVGAsset, Core.Settings.VehicleTurretStructureIcon)); CustomSvgCache.RegisterSVG(Core.Settings.VehicleTurretStructureIcon);

        prewarmRequests.Add(new PrewarmRequest(BattleTechResourceType.SVGAsset, Core.Settings.SquadStructureIcon)); CustomSvgCache.RegisterSVG(Core.Settings.SquadStructureIcon);
        prewarmRequests.Add(new PrewarmRequest(BattleTechResourceType.SVGAsset, Core.Settings.SquadArmorOutlineIcon)); CustomSvgCache.RegisterSVG(Core.Settings.SquadArmorOutlineIcon);
        prewarmRequests.Add(new PrewarmRequest(BattleTechResourceType.SVGAsset, Core.Settings.SquadArmorIcon)); CustomSvgCache.RegisterSVG(Core.Settings.MechBaySwitchIconUp);
        prewarmRequests.Add(new PrewarmRequest(BattleTechResourceType.SVGAsset, Core.Settings.MechBaySwitchIconMech)); CustomSvgCache.RegisterSVG(Core.Settings.MechBaySwitchIconMech);
        prewarmRequests.Add(new PrewarmRequest(BattleTechResourceType.SVGAsset, Core.Settings.MechBaySwitchIconVehicle)); CustomSvgCache.RegisterSVG(Core.Settings.MechBaySwitchIconVehicle);
        prewarmRequests.Add(new PrewarmRequest(BattleTechResourceType.SVGAsset, Core.Settings.MechBaySwitchIconUp)); CustomSvgCache.RegisterSVG(Core.Settings.MechBaySwitchIconUp);
        prewarmRequests.Add(new PrewarmRequest(BattleTechResourceType.SVGAsset, Core.Settings.MechBaySwitchIconDown)); CustomSvgCache.RegisterSVG(Core.Settings.MechBaySwitchIconDown);
        if (string.IsNullOrEmpty(Core.Settings.ShowActiveAbilitiesIcon) == false) {
          prewarmRequests.Add(new PrewarmRequest(BattleTechResourceType.SVGAsset, Core.Settings.ShowActiveAbilitiesIcon)); CustomSvgCache.RegisterSVG(Core.Settings.ShowActiveAbilitiesIcon);
        }
        if (string.IsNullOrEmpty(Core.Settings.ShowPassiveAbilitiesIcon) == false) {
          prewarmRequests.Add(new PrewarmRequest(BattleTechResourceType.SVGAsset, Core.Settings.ShowPassiveAbilitiesIcon)); CustomSvgCache.RegisterSVG(Core.Settings.ShowPassiveAbilitiesIcon);
        }
        if (string.IsNullOrEmpty(Core.Settings.HideActiveAbilitiesIcon) == false) {
          prewarmRequests.Add(new PrewarmRequest(BattleTechResourceType.SVGAsset, Core.Settings.HideActiveAbilitiesIcon)); CustomSvgCache.RegisterSVG(Core.Settings.HideActiveAbilitiesIcon);
        }
        if (string.IsNullOrEmpty(Core.Settings.HidePassiveAbilitiesIcon) == false) {
          prewarmRequests.Add(new PrewarmRequest(BattleTechResourceType.SVGAsset, Core.Settings.HidePassiveAbilitiesIcon)); CustomSvgCache.RegisterSVG(Core.Settings.HidePassiveAbilitiesIcon);
        }
        typeof(ApplicationConstants).GetProperty("PrewarmRequests", BindingFlags.Instance | BindingFlags.Public).GetSetMethod(true).Invoke(__instance, new object[] { prewarmRequests.ToArray() });
        foreach (PrewarmRequest preq in __instance.PrewarmRequests) {
          Log.WL(1, preq.ResourceType + ":" + preq.ResourceID);
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  public class CombatHUDActorInfoHover : EventTrigger {
    public SVGImage image;
    public CombatHUD HUD;
    public override void OnPointerEnter(PointerEventData data) {
      //Log.LogWrite("CombatHUDActorInfoHover.OnPointerEnter called." + data.position + "\n");
      //if (image != null) { image.color = Color.white; }
    }
    public override void OnPointerExit(PointerEventData data) {
      //SidePanel.ForceHide();
      //Log.LogWrite("CombatHUDActorInfoHover.OnPointerExit called." + data.position + "\n");
      //if (image != null) { image.color = Color.grey; }
    }
    public override void OnPointerClick(PointerEventData data) {
      Log.LogWrite("CombatHUDActorInfoHover.OnPointerClick called." + data.position + "\n");
      //HUD.ShowSidePanelActorInfo();
      HUD.InfoPanelShowState(!HUD.InfoPanelShowState());
    }
    public void LateUpdate() {
      if (image != null) {
        image.color = HUD.InfoPanelShowState() ? Color.white : Color.grey;
      }
    }
    public void Init(CombatHUD HUD) {
      this.HUD = HUD;
      Transform MechTray_GreebleWedge = HUD.MechTray.gameObject.transform.Find("MechTray_GreebleWedge");
      if (MechTray_GreebleWedge == null) { Log.TWL(0, "Exception: can't find MechTray_GreebleWedge", true); return; }
      Transform MechTray_GreebleDots = MechTray_GreebleWedge.Find("MechTray_GreebleDots");
      if (MechTray_GreebleDots == null) { Log.TWL(0, "Exception: can't find MechTray_GreebleDots", true); return; }
      image = MechTray_GreebleDots.GetComponent<SVGImage>();
    }
  }
  public class LanceSwitcher : EventTrigger {
    public static List<SVGAsset> lanceIcons = new List<SVGAsset>();
    public CombatHUD HUD;
    public SVGImage image;
    private bool Hovered = false;
    private bool prevState = false;
    public override void OnPointerEnter(PointerEventData data) {
      Log.LogWrite("LanceSwitcher.OnPointerEnter called." + data.position + "\n");
      Hovered = true;
      if (image != null) { image.color = Color.white; }
    }
    public override void OnPointerExit(PointerEventData data) {
      //SidePanel.ForceHide();
      Hovered = false;
      Log.LogWrite("LanceSwitcher.OnPointerExit called." + data.position + "\n");
      if (image != null) { image.color = Color.grey; }
    }
    public void Update() {
      if (Hovered == false) { return; }
      bool state = Input.GetKey(KeyCode.Mouse0);
      if (state == prevState) { return; }
      prevState = state;
      if (state) {
        int curLanceId = HUD.MechWarriorTray.CurrentLance();
        HUD.MechWarriorTray.HideLance(curLanceId);
        int prevLanceId = curLanceId;
        while (true) {
          curLanceId = (curLanceId + 1) % HUD.MechWarriorTray.exPortraitHolders().Count;
          if (curLanceId != prevLanceId) {
            bool lanceEmpty = true;
            foreach (CustomLanceInstance slot in HUD.MechWarriorTray.exPortraitHolders()[curLanceId]) {
              if (slot.portrait.DisplayedActor != null) { lanceEmpty = false; break; }
            }
            if (lanceEmpty) { continue; }
          }
          HUD.MechWarriorTray.ShowLance(curLanceId);
          break;
        }
      }
    }
    public override void OnPointerClick(PointerEventData data) {
      Log.LogWrite("LanceSwitcher.OnPointerClick called." + data.position + "\n");
    }
    public void Init(CombatHUD HUD, SVGImage image) {
      this.HUD = HUD;
      this.image = image;
      //lanceIcons = new List<SVGAsset>();
      Log.TWL(0, "LanceSwitcher.Init");
      for (int index = lanceIcons.Count; index < Core.Settings.LancesIcons.Count; ++index) {
        string iconid = Core.Settings.LancesIcons[index];
        SVGAsset icon = CustomSvgCache.get(iconid, HUD.Combat.DataManager); //HUD.Combat.DataManager.GetObjectOfType<SVGAsset>(iconid, BattleTechResourceType.SVGAsset);
        if (icon != null) {
          Log.WL(1, "icon " + iconid + " found");
          lanceIcons.Add(icon);
        } else {
          Log.WL(1, "icon " + iconid + " not found");
        }
      }
      HUD.MechWarriorTray.SetLanceSwitcher(this);
      int curLanceId = HUD.MechWarriorTray.CurrentLance();
      if (lanceIcons.Count > 0) { image.vectorGraphics = lanceIcons[curLanceId % lanceIcons.Count]; }
    }
  }
  public class HUDPosUpdater : MonoBehaviour {
    public CombatHUDMechwarriorTray MechWarriorTray;
    public CombatHUDMoraleBar moraleBar;
    public HUDPosUpdater() { MechWarriorTray = null; moraleBar = null; }
    public void Update() {
      if (moraleBar == null) { return; }
      if (MechWarriorTray == null) { return; }
      RectTransform rtr = moraleBar.gameObject.GetComponent<RectTransform>();
      RectTransform prt = MechWarriorTray.PortraitHolders[0].gameObject.transform.parent.gameObject.GetComponent<RectTransform>();
      Vector3[] pcorners = new Vector3[4];
      prt.GetWorldCorners(pcorners);
      Vector3 pos = rtr.position;
      pos.x = pcorners[0].x - 10f;
      rtr.position = pos;
    }
    public void Init(CombatHUDMechwarriorTray MechWarriorTray) {
      this.MechWarriorTray = MechWarriorTray;
      this.moraleBar = (CombatHUDMoraleBar)typeof(CombatHUDMechwarriorTray).GetProperty("moraleDisplay", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(MechWarriorTray, new object[0] { });
    }
  }
  [HarmonyPatch(typeof(HUDMechArmorReadout))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  public static class HUDMechArmorReadout_Init {
    public static void Postfix(HUDMechArmorReadout __instance) {
      Log.TWL(0, "HUDMechArmorReadout.Init gameObject:" + __instance.gameObject.name + " parent:" + __instance.gameObject.transform.parent.gameObject.name);
    }
  }
  [HarmonyPatch(typeof(CombatHUD))]
  [HarmonyPatch("updateHUDElements")]
  [HarmonyPatch(MethodType.Normal)]
  public static class CombatHUD_updateHUDElements {
    public static void Postfix(CombatHUD __instance, AbstractActor actor) {
      if (actor == null) { return; }
      int lanceId = actor.LanceId();
      Log.TWL(0, "CombatHUD.updateHUDElements " + actor.DisplayName + " lanceId:" + lanceId);
      if (lanceId >= 0) {
        if (__instance.MechWarriorTray.CurrentLance() != lanceId) {
          __instance.MechWarriorTray.HideLance(__instance.MechWarriorTray.CurrentLance());
          __instance.MechWarriorTray.ShowLance(lanceId);
        }
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDMechwarriorTray))]
  [HarmonyPatch("OnActorSelected")]
  [HarmonyPatch(MethodType.Normal)]
  public static class CombatHUDMechwarriorTray_OnActorSelected {
    private static GameObject f_mwCommandButton = null;
    public static GameObject mwCommandButton(this CombatHUDMechwarriorTray instance, CombatHUD HUD) {
      if (f_mwCommandButton == null) {
        Log.TWL(0, "f_mwCommandButton init");
        f_mwCommandButton = instance.PortraitHolders[0].transform.parent.Find("mwCommandButton").gameObject;
        CombatHUDActionButton aBtn = f_mwCommandButton.GetComponentInChildren<CombatHUDActionButton>();
        GameObject.Destroy(aBtn);
        CombatHUDTooltipHoverElement hEl = f_mwCommandButton.GetComponentInChildren<CombatHUDTooltipHoverElement>();
        GameObject.Destroy(hEl);
        Transform uixPrfBttn_commandButton = f_mwCommandButton.transform.GetChild(0);
        if (uixPrfBttn_commandButton != null) {
          Transform cmd_NameRect = uixPrfBttn_commandButton.transform.Find("cmd_NameRect");
          if (cmd_NameRect != null) {
            cmd_NameRect.gameObject.SetActive(false);
          } else {
            Log.WL(1, "cmd_NameRect can't find");
          }
          Transform cmd_uses_leftBG = uixPrfBttn_commandButton.transform.Find("cmd_uses_leftBG");
          if (cmd_uses_leftBG != null) {
            cmd_uses_leftBG.gameObject.SetActive(false);
          } else {
            Log.WL(1, "cmd_uses_leftBG can't find");
          }
          Transform cmd_IconBG = uixPrfBttn_commandButton.transform.Find("cmd_IconBG");
          if (cmd_IconBG != null) {
            SVGImage svg = cmd_IconBG.GetComponent<SVGImage>();
            svg.vectorGraphics = LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.DoneWithMechButtonIcon;
            LanceSwitcher lanceSwitcher = f_mwCommandButton.GetComponent<LanceSwitcher>();
            if (lanceSwitcher == null) { lanceSwitcher = f_mwCommandButton.AddComponent<LanceSwitcher>(); }
            if (lanceSwitcher != null) { lanceSwitcher.Init(HUD, svg); };
          } else {
            Log.WL(1, "cmd_IconBG can't find");
          }
        } else {
          Log.WL(1, "uixPrfBttn_commandButton can't find");
        }
      }
      return f_mwCommandButton;
    }
    public static void Postfix(CombatHUDMechwarriorTray __instance, CombatHUD ___HUD) {
      GameObject btn = __instance.mwCommandButton(___HUD);
      if(___HUD.Combat.LocalPlayerTeam.unitCount == 1) {
        if (___HUD.Combat.LocalPlayerTeam.units[0].IsDeployDirector()) {
          btn.SetActive(false); return;
        }
      }
      btn.SetActive(true);
    }
  }
  [HarmonyPatch(typeof(CombatHUDMechwarriorTray))]
  [HarmonyPatch("OnActorDeselected")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class CombatHUDMechwarriorTray_OnActorDeselected {
    public static void Postfix(CombatHUDMechwarriorTray __instance, CombatHUD ___HUD) {
      //__instance.mwCommandButton(___HUD).SetActive(false);
    }
  }
  [HarmonyPatch(typeof(CombatHUDMechwarriorTray))]
  [HarmonyPatch("OnCommandTrayInvoked")]
  [HarmonyPatch(MethodType.Normal)]
  public static class CombatHUDMechwarriorTray_OnCommandTrayInvoked {
    public static void Postfix(CombatHUDMechwarriorTray __instance, CombatHUD ___HUD) {
      GameObject btn = __instance.mwCommandButton(___HUD);
      if (___HUD.Combat.LocalPlayerTeam.unitCount == 1) {
        if (___HUD.Combat.LocalPlayerTeam.units[0].IsDeployDirector()) {
          btn.SetActive(false); return;
        }
      }
      btn.SetActive(true);
    }
  }
  [HarmonyPatch(typeof(CombatHUDMechwarriorTray))]
  [HarmonyPatch("OnCommandTrayDismissed")]
  [HarmonyPatch(MethodType.Normal)]
  public static class CombatHUDMechwarriorTray_OnCommandTrayDismissed {
    public static void Postfix(CombatHUDMechwarriorTray __instance, CombatHUD ___HUD) {
      //__instance.mwCommandButton(___HUD).SetActive(false);
    }
  }
  [HarmonyPatch(typeof(CombatHUDMechwarriorTray))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  public static class CombatHUDMechwarriorTray_Init_Postfix {
    public static void Postfix(CombatHUDMechwarriorTray __instance, CombatGameState Combat, CombatHUD HUD) {
      HUDPosUpdater posU = __instance.gameObject.GetComponent<HUDPosUpdater>();
      if (posU == null) { posU = __instance.gameObject.AddComponent<HUDPosUpdater>(); }
      posU.Init(__instance);
    }
  }
  [HarmonyPatch(typeof(CombatHUDMechTray))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  public static class CombatHUDMechTray_Init {

    public static void Postfix(CombatHUDMechTray __instance, MessageCenter messageCenter, CombatHUD HUD) {
      Transform MechTrayBGImage = __instance.gameObject.transform.Find("MechTrayBGImage");
      if (MechTrayBGImage == null) { Log.TWL(0, "Exception: can't find MechTrayBGImage", true); return; }
      CombatHUDActorInfoHover hover = MechTrayBGImage.gameObject.GetComponent<CombatHUDActorInfoHover>();
      if (hover == null) { hover = MechTrayBGImage.gameObject.AddComponent<CombatHUDActorInfoHover>(); }
      hover.Init(HUD);
      /*CombatHUDActorInfoHover hover = MechTray_GreebleDots.gameObject.GetComponent<CombatHUDActorInfoHover>();
      if (hover == null) { hover = MechTray_GreebleDots.gameObject.AddComponent<CombatHUDActorInfoHover>(); }
      hover.Init(HUD);
      hover = MechTray_GreebleWedge.gameObject.GetComponent<CombatHUDActorInfoHover>();
      if (hover == null) { hover = MechTray_GreebleWedge.gameObject.AddComponent<CombatHUDActorInfoHover>(); }
      hover.Init(HUD);*/
    }
  }
  [HarmonyPatch(typeof(HBSDOTweenToggle))]
  [HarmonyPatch("Enabled_OnEnter")]
  [HarmonyPatch(MethodType.Normal)]
  public static class HBSDOTweenToggle_Enabled_OnEnter {
    public static void Postfix(HBSDOTweenToggle __instance) {
      //Log.TWL(0, "HBSDOTweenToggle.Enabled_OnEnter " + __instance.gameObject.name);
      foreach (DOTweenAnimation anim in __instance.OnEnabledTweens) {
        //Log.WL(1, "anim:" + anim.target + " type:" + anim.animationType + " val:" + anim.endValueFloat);
      }
    }
  }
  [HarmonyPatch(typeof(HBSDOTweenToggle))]
  [HarmonyPatch("Selected_OnEnter")]
  [HarmonyPatch(MethodType.Normal)]
  public static class HBSDOTweenToggle_Selected_OnEnter {
    public static void Postfix(HBSDOTweenToggle __instance) {
      //Log.TWL(0, "HBSDOTweenToggle.Selected_OnEnter " + __instance.gameObject.name);
      foreach (DOTweenAnimation anim in __instance.OnSelectedTweens) {
        //Log.WL(1, "anim:" + anim.target + " type:" + anim.animationType + " val:" + anim.endValueFloat);
      }
    }
  }
  [HarmonyPatch(typeof(HBSDOTweenToggle))]
  [HarmonyPatch("Highlighted_OnEnter")]
  [HarmonyPatch(MethodType.Normal)]
  public static class HBSDOTweenToggle_Highlighted_OnEnter {
    public static void Postfix(HBSDOTweenToggle __instance) {
      //Log.TWL(0, "HBSDOTweenToggle.Highlighted_OnEnter " + __instance.gameObject.name);
      foreach (DOTweenAnimation anim in __instance.OnHighlightedTweens) {
        //Log.WL(1, "anim:" + anim.target + " type:" + anim.animationType + " val:" + anim.endValueFloat);
      }
    }
  }
  [HarmonyPatch(typeof(HBSDOTweenToggle))]
  [HarmonyPatch("Disabled_OnEnter")]
  [HarmonyPatch(MethodType.Normal)]
  public static class HBSDOTweenToggle_Disabled_OnEnter {
    public static void Postfix(HBSDOTweenToggle __instance) {
      //Log.TWL(0, "HBSDOTweenToggle.Disabled_OnEnter " + __instance.gameObject.name);
      foreach (DOTweenAnimation anim in __instance.OnDisabledTweens) {
        //Log.WL(1, "anim:" + anim.target + " type:" + anim.animationType + " val:" + anim.endValueFloat);
      }
    }
  }
  [HarmonyPatch(typeof(ShortcutExtensions))]
  [HarmonyPatch("DOScale")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Transform), typeof(Vector3), typeof(float) })]
  public static class ShortcutExtensions_DOScale {
    public static void Prefix(Transform target, Vector3 endValue, float duration) {
      //Log.TWL(0, "ShortcutExtensions.DOScale " + target.gameObject.name+" endValue:"+endValue+" duration:"+duration);
    }
  }
  [HarmonyPatch(typeof(CombatHUDMechwarriorTray))]
  [HarmonyPatch("RefreshTeam")]
  [HarmonyPatch(MethodType.Normal)]
  public static class CombatHUDMechwarriorTray_RefreshTeam {
    private static Dictionary<AbstractActor, int> actorLanceIds = new Dictionary<AbstractActor, int>();
    public static void Clear() {
      fHUD = null;
      actorLanceIds.Clear();
    }
    public static int LanceId(this AbstractActor actor) {
      if (actorLanceIds.TryGetValue(actor, out int lid)) {
        return lid;
      }
      return -1;
    }
    private static CombatHUD fHUD = null;
    public static CombatHUD HUD(this LanceSpawnerGameLogic spawner) { return fHUD; }
    public static CombatHUD HUD(this AbstractActor spawner) { return fHUD; }
    public static bool Prefix(CombatHUDMechwarriorTray __instance, Team team, Team ___displayedTeam, CombatHUD ___HUD) {
      ___displayedTeam = team;
      Log.TWL(0, "CombatHUDMechwarriorTray.RefreshTeam");
      actorLanceIds.Clear();
      fHUD = ___HUD;
      try {
        Log.WL(1, "Team:" +team.units.Count);
        foreach(AbstractActor unit in team.units) {
          Log.WL(2, unit.DisplayName + " " + unit.PilotableActorDef.Description.Id + " GUID:" + unit.PilotableActorDef.GUID);
        }
        SimGameState sim = UnityGameInstance.BattleTechGame.Simulation;
        if (sim != null) {
          try {
            Log.WL(1, "PlayerBayUnits:" + sim.ActiveMechs.Count);
            foreach (var unit in sim.ActiveMechs) {
              Log.WL(2, unit.Key.ToString() + ":" + unit.Value.Description.Id + " GUID:" + unit.Value.GUID);
            }
          }catch(Exception e) {
            Log.TWL(0, e.ToString(), true);
          }
        }
        Dictionary<int, CustomLanceInstance> avaibleSlots = new Dictionary<int, CustomLanceInstance>();
        foreach (var lance in __instance.exPortraitHolders()) {
          foreach (var slot in lance) {
            avaibleSlots.Add(slot.index, slot);
            slot.holder.SetActive(false);
          }
        }
        if ((team.unitCount == 1) && (team.units[0].IsDeployDirector())) {
          avaibleSlots[0].portrait.DisplayedActor = team.units[0];
          avaibleSlots[0].holder.SetActive(true);
          for(int index=1;index< avaibleSlots.Count; ++index) {
            avaibleSlots[index].holder.SetActive(false);
          }
          ___HUD.SelectionHandler.TrySelectActor(team.units[0],true);
          return false;
        }
        Dictionary<int, AbstractActor> predefinedActors = new Dictionary<int, AbstractActor>();
        List<AbstractActor> mechs = new List<AbstractActor>();
        List<AbstractActor> vehicles = new List<AbstractActor>();
        Log.WL(1, "player lance layout");
        foreach (var plLayout in CustomLanceHelper.playerLanceLoadout.loadout) {
          Log.WL(2, plLayout.Key + ":" + plLayout.Value);
        }
        Log.WL(1, "avaible slots");
        foreach (var slot in avaibleSlots) {
          Log.WL(2, slot.Key + " lance:" + slot.Value.lance_index + " index:" + slot.Value.lance_id + " head:" + slot.Value.isHead);
        }
        foreach (AbstractActor unit in ___displayedTeam.units) {
          string defGUID = string.Empty;
          if (unit.IsDeployDirector()) { continue; }
          bool isVehicle = false;
          if (unit.UnitType == UnitType.Mech) {
            defGUID = (unit as Mech).MechDef.GUID;
          } else if (unit.UnitType == UnitType.Vehicle) {
            defGUID = (unit as Vehicle).VehicleDef.GUID;
            isVehicle = false;
          } else {
            continue;
          }
          Log.WL(1, unit.DisplayName + " tags:" + unit.EncounterTags.ContentToString() + " defGUID:" + defGUID);
          if (string.IsNullOrEmpty(defGUID) == false) {
            if (CustomLanceHelper.playerLanceLoadout.loadout.TryGetValue(defGUID, out int slotIndex)) {
              if (avaibleSlots.TryGetValue(slotIndex, out CustomLanceInstance slot)) {
                slot.portrait.DisplayedActor = unit;
                slot.holder.SetActive(false);
                Log.WL(1, new Localize.Text(unit.DisplayName).ToString() + " lance:" + slot.lance_id + " pos:" + slot.lance_index);
                avaibleSlots.Remove(slotIndex);
                continue;
              }
            }
          };
          if (unit.EncounterTags.Contains(Core.Settings.PlayerControlConvoyTag)) {
            int slotIndex = avaibleSlots.Last().Key;
            CustomLanceInstance slot = avaibleSlots.Last().Value;
            slot.portrait.DisplayedActor = unit;
            slot.holder.SetActive(false);
            Log.WL(1, new Localize.Text(unit.DisplayName).ToString() + " escort unit detected. lance:" + slot.lance_id + " pos:" + slot.lance_index);
            avaibleSlots.Remove(slotIndex);
            continue;
          }
          if (isVehicle) { vehicles.Add(unit); } else { mechs.Add(unit); }
        }
        HashSet<int> restSlots = avaibleSlots.Keys.ToHashSet();
        foreach (int slotIndex in restSlots) {
          if (avaibleSlots[slotIndex].isVehicle && (vehicles.Count > 0)) {
            avaibleSlots[slotIndex].portrait.DisplayedActor = vehicles[0];
            avaibleSlots[slotIndex].holder.SetActive(false);
            vehicles.RemoveAt(0);
            avaibleSlots.Remove(slotIndex);
            continue;
          }
          if ((avaibleSlots[slotIndex].isVehicle == false) && (mechs.Count > 0)) {
            avaibleSlots[slotIndex].portrait.DisplayedActor = mechs[0];
            avaibleSlots[slotIndex].holder.SetActive(false);
            mechs.RemoveAt(0);
            avaibleSlots.Remove(slotIndex);
            continue;
          }
        }
        foreach (var slot in avaibleSlots) {
          slot.Value.portrait.DisplayedActor = null;
        }
        int nonemptyLance = -1;
        for (int lance_id = 0; lance_id < __instance.exPortraitHolders().Count; ++lance_id) {
          for (int lance_index = 0; lance_index < __instance.exPortraitHolders()[lance_id].Count; ++lance_index) {
            if (__instance.exPortraitHolders()[lance_id][lance_index].portrait.DisplayedActor != null) {
              actorLanceIds.Add(__instance.exPortraitHolders()[lance_id][lance_index].portrait.DisplayedActor, lance_id);
              if (nonemptyLance == -1) {
                nonemptyLance = lance_id;
              }
            }
          }
        }
        if (nonemptyLance < 0) { nonemptyLance = 0; }
        //if (nonemptyLance != 0) { __instance.HideLance(0); };
        __instance.ShowLance(nonemptyLance);
        Log.WL(1, "success");
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
      return false;
    }
  }
  public class CustomLanceInstance {
    public bool isHead { get; set; }
    public int index { get; set; }
    public int lance_id { get; set; }
    public int lance_index { get; set; }
    public bool isVehicle { get; set; }
    public GameObject holder { get; set; }
    public CombatHUDPortrait portrait { get; set; }
    public CustomLanceInstance(GameObject h, CombatHUDPortrait p, int index, bool head, int lanceid, int lance_pos, bool isVehicle) {
      holder = h; portrait = p; this.index = index; isHead = head;
      this.lance_id = lanceid;
      this.lance_index = lance_pos;
      this.isVehicle = isVehicle;
    }
  }
  [HarmonyPatch(typeof(CombatHUDButtonBase))]
  [HarmonyPatch("OnPointerEnter")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(PointerEventData) })]
  public static class CombatHUDButtonBase_OnPointerEnter {
    //private static bool toggleState = false;
    public static void Postfix(CombatHUDButtonBase __instance, PointerEventData eventData) {
      Log.TWL(0, "CombatHUDButtonBase.OnPointerEnter " + __instance.GUID);
      CombatHUDMechwarriorTrayEx trayEx = Traverse.Create(__instance).Property<CombatHUD>("HUD").Value.MechWarriorTray.gameObject.GetComponent<CombatHUDMechwarriorTrayEx>();
      if (trayEx == null) {
        Log.WL(1, "no trayEx");
        return;
      }
      CombatHUDActionButton instance = __instance as CombatHUDActionButton;
      if (instance == null) {
        Log.WL(1, "not a CombatHUDActionButton");
        return;
      }
      if (trayEx.actionButtons.Contains(instance) == false) {
        Log.WL(1, "not int actionButtons");
        return;
      };
      foreach (CombatHUDActionButton btn in trayEx.actionButtons) {
        Log.WL(1, "exiting:" + btn.GUID);
        if (btn == __instance) { continue; }
        btn.Tooltip.color = Color.clear;
        Traverse.Create(btn).Field<float>("timeSinceColorsChanged").Value = 0.14f;
        btn.OnPointerExit(eventData);
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDButtonBase))]
  [HarmonyPatch("OnPointerExit")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(PointerEventData) })]
  public static class CombatHUDButtonBase_OnPointerExit {
    //private static bool toggleState = false;
    public static void Postfix(CombatHUDButtonBase __instance, PointerEventData eventData) {
      Log.TWL(0, "CombatHUDButtonBase.OnPointerExit " + __instance.GUID);
    }
  }
  public class CombatHUDMechwarriorTrayEx : MonoBehaviour {
    public HashSet<CombatHUDActionButton> actionButtons;
    public List<CombatHUDActionButton> ActiveAbilitiesButtons;
    public CanvasGroup ActiveAbilitiesCanvasGroup;
    public CanvasGroup PassiveAbilitiesCanvasGroup;
    public CombatHUDActionButton FirstActiveAbility;
    public List<CombatHUDActionButton> PassiveAbilitiesButtons;
    public List<CombatHUDActionButton> NormalButtons;
    public CombatHUDActionButton ShowActiveAbilitiesButton;
    public CombatHUDActionButton ShowPassiveAbilitiesButton;
    public CombatHUDActionButton HideActiveAbilitiesButton;
    public CombatHUDActionButton HidePassiveAbilitiesButton;
    public GameObject ActiveButtonsLayout;
    public GameObject PassiveButtonsLayout;
    public bool AttackModeSelectorPosInited;
    public Vector3 AttackModeSelectorUpPos;
    public Vector3 AttackModeSelectorDownPos;
    private bool ui_inited;
    public static readonly int BUTTONS_COUNT = 10;
    public static readonly string SHOW_ACTIVE_BUTTON_GUID = "BTN_ShowActiveButtons";
    public static readonly string HIDE_ACTIVE_BUTTON_GUID = "BTN_HideActiveButtons";
    public static readonly string SHOW_PASSIVE_BUTTON_GUID = "BTN_ShowPassiveButtons";
    public static readonly string HIDE_PASSIVE_BUTTON_GUID = "BTN_HidePassiveButtons";
    public static readonly string SHOW_ACTIVE_BUTTON_NAME = "ACT.ABIL.";
    public static readonly string SHOW_PASSIVE_BUTTON_NAME = "PAS.ABIL.";
    public CombatHUD HUD;
    public CombatGameState Combat;
    public CombatHUDMechwarriorTray mechWarriorTray;
    public void initAttackModeSelectorPos() {
      if (AttackModeSelectorPosInited) { return; }
      AttackModeSelectorPosInited = true;
      RectTransform selfTransform = ActiveButtonsLayout.GetComponent<RectTransform>();
      AttackModeSelectorDownPos = HUD.AttackModeSelector.transform.localPosition;
      AttackModeSelectorUpPos = HUD.AttackModeSelector.transform.localPosition;
      AttackModeSelectorUpPos.y += selfTransform.sizeDelta.y;
    }
    public void ShowActiveAbilities() {
      ActiveButtonsLayout.SetActive(true);
      PassiveButtonsLayout.SetActive(false);
      for (int index = 0; index < BUTTONS_COUNT; ++index) {
        this.NormalButtons[index].ButtonAction = null;
      }
      for (int index = 0; index < (BUTTONS_COUNT - 1); ++index) {
        this.ActiveAbilitiesButtons[index].SetHotKey(Core.Settings.DisableHotKeys ? null : BTInput.Instance.Combat_Abilities()[index], true);
      }
      UpdateExtButtons();
      initAttackModeSelectorPos();
    }
    public void UpdateExtButtons() {
      if (HUD.SelectedActor != null) {
        HideActiveAbilitiesButton.Init(Combat, HUD, Core.Settings.DisableHotKeys ? null : BTInput.Instance.Combat_DoneWithMech(), true);
        HideActiveAbilitiesButton.isClickable = true;
        HideActiveAbilitiesButton.RefreshColors(HUD.SelectedActor);
        HidePassiveAbilitiesButton.Init(Combat, HUD, Core.Settings.DisableHotKeys ? null : BTInput.Instance.Combat_DoneWithMech(), true);
        HideActiveAbilitiesButton.isClickable = true;
        HideActiveAbilitiesButton.RefreshColors(HUD.SelectedActor);
      }
    }
    public void ShowPassiveAbilities() {
      ActiveButtonsLayout.SetActive(false);
      PassiveButtonsLayout.SetActive(true);
      for (int index = 0; index < BUTTONS_COUNT; ++index) {
        this.NormalButtons[index].ButtonAction = null;
      }
      for (int index = 0; index < (BUTTONS_COUNT - 1); ++index) {
        this.PassiveAbilitiesButtons[index].SetHotKey(Core.Settings.DisableHotKeys ? null : BTInput.Instance.Combat_Abilities()[index], true);
      }
      UpdateExtButtons();
      initAttackModeSelectorPos();
    }
    public void HideActiveAbilities() {
      ActiveButtonsLayout.SetActive(false);
      for (int index = 0; index < BUTTONS_COUNT; ++index) {
        if (index == 9) {
          this.NormalButtons[index].SetHotKey(Core.Settings.DisableHotKeys ? null : BTInput.Instance.Combat_DoneWithMech(), true);
        } else {
          this.NormalButtons[index].SetHotKey(Core.Settings.DisableHotKeys ? null : BTInput.Instance.Combat_Abilities()[index], true);
        }
      }
    }
    public void HidePassiveAbilities() {
      PassiveButtonsLayout.SetActive(false);
      for (int index = 0; index < BUTTONS_COUNT; ++index) {
        if (index == 9) {
          this.NormalButtons[index].SetHotKey(Core.Settings.DisableHotKeys ? null : BTInput.Instance.Combat_DoneWithMech(), true);
        } else {
          this.NormalButtons[index].SetHotKey(Core.Settings.DisableHotKeys ? null : BTInput.Instance.Combat_Abilities()[index], true);
        }
      }
    }
    public CombatHUDMechwarriorTrayEx() {
      ActiveAbilitiesButtons = new List<CombatHUDActionButton>();
      PassiveAbilitiesButtons = new List<CombatHUDActionButton>();
      NormalButtons = new List<CombatHUDActionButton>();
      ui_inited = false;
      AttackModeSelectorPosInited = false;
      actionButtons = new HashSet<CombatHUDActionButton>();
    }
    public void initUI() {
      RectTransform portraits = mechWarriorTray.gameObject.transform.FindRecursive("mwPortraitLayoutGroup") as RectTransform;
      RectTransform selfTransform = ActiveButtonsLayout.GetComponent<RectTransform>();
      Vector3 locPos = Vector3.zero;
      locPos.y = portraits.localPosition.y + (portraits.sizeDelta.y / 2f) + selfTransform.sizeDelta.y;
      ActiveButtonsLayout.transform.localPosition = locPos;
      PassiveButtonsLayout.transform.localPosition = locPos;
    }
    public void Update() {
      if (ui_inited == false) {
        initUI();
      }
      if (HUD != null) {
        if (HUD.SelectedActor == null) {
          ActiveButtonsLayout.SetActive(false);
          PassiveButtonsLayout.SetActive(false);
        } else {
          if (this.AttackModeSelectorPosInited) {
            if (ActiveButtonsLayout.activeSelf || PassiveButtonsLayout.activeSelf) {
              HUD.AttackModeSelector.transform.localPosition = AttackModeSelectorUpPos;
            }
          }
        }
      }
    }
    public void Init(CombatHUDMechwarriorTray mechWarriorTray, CombatHUD HUD, CombatGameState Combat) {
      this.mechWarriorTray = mechWarriorTray;
      this.HUD = HUD;
      this.Combat = Combat;
      for (int index = 0; index < (BUTTONS_COUNT - 1); ++index) {
        CombatHUDSidePanelHoverElement panelHoverElement = ActiveAbilitiesButtons[index].gameObject.GetComponentInChildren<CombatHUDSidePanelHoverElement>(true);
        if ((UnityEngine.Object)panelHoverElement == (UnityEngine.Object)null) {
          panelHoverElement = ActiveAbilitiesButtons[index].gameObject.AddComponent<CombatHUDSidePanelHoverElement>();
        }
        panelHoverElement.Init(HUD);
        if (index < 9) {
          ActiveAbilitiesButtons[index].Init(Combat, HUD, BTInput.Instance.Combat_Abilities()[index], true);
        }
        //ActiveAbilitiesButtons[index].gameObject.transform.parent.gameObject.SetActive(false);
        panelHoverElement = PassiveAbilitiesButtons[index].gameObject.GetComponentInChildren<CombatHUDSidePanelHoverElement>(true);
        if ((UnityEngine.Object)panelHoverElement == (UnityEngine.Object)null) {
          panelHoverElement = PassiveAbilitiesButtons[index].gameObject.AddComponent<CombatHUDSidePanelHoverElement>();
        }
        panelHoverElement.Init(HUD);
        if (index < 9) {
          PassiveAbilitiesButtons[index].Init(Combat, HUD, BTInput.Instance.Combat_Abilities()[index], true);
        }
        //PassiveAbilitiesButtons[index].gameObject.transform.parent.gameObject.SetActive(false);
      }
      HideActiveAbilitiesButton.Init(Combat, HUD, BTInput.Instance.Combat_DoneWithMech(), true);
      HidePassiveAbilitiesButton.Init(Combat, HUD, BTInput.Instance.Combat_DoneWithMech(), true);
      //HideActiveAbilitiesButton.gameObject.transform.parent.gameObject.SetActive(false);
      //HidePassiveAbilitiesButton.gameObject.transform.parent.gameObject.SetActive(false);
    }
    public void Instantine(CombatHUDMechwarriorTray mechWarriorTray) {
      Transform buttons = mechWarriorTray.gameObject.transform.FindRecursive("mwt_ActionButtonsLayout");
      GameObject activeBtns = GameObject.Instantiate(buttons.gameObject);
      ActiveAbilitiesCanvasGroup = activeBtns.GetComponent<CanvasGroup>();
      ActiveAbilitiesCanvasGroup.interactable = true;
      ActiveAbilitiesCanvasGroup.blocksRaycasts = true;
      activeBtns.transform.SetParent(buttons.transform.parent);
      activeBtns.transform.localScale = Vector3.one;
      ActiveButtonsLayout = activeBtns;
      GameObject passiveBtns = GameObject.Instantiate(buttons.gameObject);
      PassiveAbilitiesCanvasGroup = passiveBtns.GetComponent<CanvasGroup>();
      PassiveAbilitiesCanvasGroup.interactable = true;
      PassiveAbilitiesCanvasGroup.blocksRaycasts = true;
      passiveBtns.transform.SetParent(buttons.transform.parent);
      passiveBtns.transform.localScale = Vector3.one;
      PassiveButtonsLayout = passiveBtns;
      CombatHUDActionButton[] normalButtons = buttons.gameObject.GetComponentsInChildren<CombatHUDActionButton>(true);
      for (int index = 0; index < BUTTONS_COUNT; ++index) {
        CombatHUDActionButton abtn = normalButtons[index];
        NormalButtons.Add(abtn);
      }
      CombatHUDActionButton[] activeButtons = activeBtns.GetComponentsInChildren<CombatHUDActionButton>(true);
      for (int index = 0; index < BUTTONS_COUNT; ++index) {
        CombatHUDActionButton abtn = activeButtons[index];
        if (index == 9) {
          HideActiveAbilitiesButton = abtn;
        } else {
          ActiveAbilitiesButtons.Add(abtn);
        }
        actionButtons.Add(abtn);
      }
      CombatHUDActionButton[] passiveButtons = passiveBtns.GetComponentsInChildren<CombatHUDActionButton>(true);
      for (int index = 0; index < BUTTONS_COUNT; ++index) {
        CombatHUDActionButton abtn = passiveButtons[index];
        if (index == 9) {
          HidePassiveAbilitiesButton = abtn;
        } else {
          PassiveAbilitiesButtons.Add(abtn);
        }
        actionButtons.Add(abtn);
      }
    }
  };
  [HarmonyPatch(typeof(CombatHUDMechwarriorTray))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPriority(Priority.First)]
  public static class CombatHUDMechwarriorTray_Init {
    private static List<List<CustomLanceInstance>> lancesPortraitHolders = new List<List<CustomLanceInstance>>();
    public static List<List<CustomLanceInstance>> exPortraitHolders(this CombatHUDMechwarriorTray tray) { return lancesPortraitHolders; }
    public static int lancesCount(this CombatHUDMechwarriorTray tray) { return lancesPortraitHolders.Count; }
    private static LanceSwitcher LanceSwitcher = null;
    private static int currentLance = 0;
    public static int CurrentLance(this CombatHUDMechwarriorTray tray) { return currentLance; }
    public static void SetLanceSwitcher(this CombatHUDMechwarriorTray tray, LanceSwitcher lanceSwitcher) { LanceSwitcher = lanceSwitcher; }
    //public static int lancesCount(this CombatHUD HUD) { return lancesPortraitHolders.Count; }
    public static void ShowLance(this CombatHUDMechwarriorTray tray, int lanceid) {
      if (lancesPortraitHolders.Count == 0) { return; }
      lanceid = lanceid % tray.lancesCount();
      currentLance = lanceid;
      foreach (CustomLanceInstance li in lancesPortraitHolders[lanceid]) {
        li.holder.SetActive(true);
      }
      if (LanceSwitcher != null) {
        if (LanceSwitcher.lanceIcons.Count > 0) {
          LanceSwitcher.image.vectorGraphics = LanceSwitcher.lanceIcons[lanceid % LanceSwitcher.lanceIcons.Count];
        }
      }
    }
    public static void HideLance(this CombatHUDMechwarriorTray tray, int lanceid) {
      if (lancesPortraitHolders.Count == 0) { return; }
      lanceid = lanceid % tray.lancesCount();
      foreach (CustomLanceInstance li in lancesPortraitHolders[lanceid]) {
        li.holder.SetActive(false);
      }
    }
    public static void AddTween(this HBSDOTweenToggle toggle, DOTweenAnimation anim) {
      if (anim.isHBSButton) {
        Log.TWL(0, "AddTween:" + toggle.gameObject.name + " anim:" + anim.buttonState + "->" + anim.endValueFloat);
        switch (anim.buttonState) {
          case ButtonState.Enabled:
          toggle.OnEnabledTweens.Add(anim);
          break;
          case ButtonState.Selected:
          toggle.OnSelectedTweens.Add(anim);
          break;
          case ButtonState.Highlighted:
          toggle.OnHighlightedTweens.Add(anim);
          break;
          case ButtonState.Pressed:
          toggle.OnPressedTweens.Add(anim);
          break;
          case ButtonState.Unavailable:
          toggle.OnUnavailableTweens.Add(anim);
          break;
          case ButtonState.Disabled:
          toggle.OnDisabledTweens.Add(anim);
          break;
          default:
          break;
        }
      }
    }
    public static void InitAdditinalPortraits(CombatHUDMechwarriorTray __instance, CombatGameState Combat, CombatHUD HUD) {
      List<GameObject> holders = new List<GameObject>();
      holders.AddRange(__instance.PortraitHolders);
      List<HBSDOTweenToggle> tweensHolders = new List<HBSDOTweenToggle>();
      tweensHolders.AddRange(__instance.portraitTweens);
      GameObject srcHolder = holders[0];

      GameObject srcTween = tweensHolders[0].gameObject;
      HorizontalLayoutGroup layout = srcHolder.transform.parent.gameObject.GetComponent<HorizontalLayoutGroup>();
      layout.childAlignment = TextAnchor.MiddleLeft;
      int all_lances_size = CustomLanceHelper.fullSlotsCount() + Core.Settings.ConvoyUnitsCount;
      for (int t = __instance.PortraitHolders.Length; t < all_lances_size; ++t) {
        GameObject newHolder = GameObject.Instantiate(srcHolder);
        newHolder.name = "mwPortraitHolder" + (t + 1).ToString();
        newHolder.transform.SetParent(srcHolder.transform.parent);
        newHolder.transform.SetSiblingIndex(t);
        GameObject newTween = GameObject.Instantiate(srcTween);
        newTween.name = "HookforAndy_mwButton" + (t + 1).ToString();
        HBSDOTweenToggle btn = newTween.GetComponent<HBSDOTweenToggle>();
        newTween.transform.SetParent(srcTween.transform.parent);
        newTween.transform.SetSiblingIndex(srcTween.transform.GetSiblingIndex() + t);
        btn.TweenObjects[0] = newHolder;
        btn.OnEnabledTweens.Clear();
        btn.OnSelectedTweens.Clear();
        btn.OnHighlightedTweens.Clear();
        btn.OnPressedTweens.Clear();
        btn.OnUnavailableTweens.Clear();
        btn.OnDisabledTweens.Clear();
        List<DOTweenAnimation> doTweenAnimationList = new List<DOTweenAnimation>();
        for (int index = 0; index < btn.TweenObjects.Count; ++index) {
          if (btn.TweenObjects[index] != null) {
            doTweenAnimationList.AddRange((IEnumerable<DOTweenAnimation>)btn.TweenObjects[index].GetComponents<DOTweenAnimation>());
          }
        }
        for (int index = 0; index < doTweenAnimationList.Count; ++index) {
          btn.AddTween(doTweenAnimationList[index]);
        }
        holders.Add(newHolder);
        btn.SetState(ButtonState.Enabled, true);
        //if (lanceid == 0) { newHolder.SetActive(true); }else{ newHolder.SetActive(false); };
        tweensHolders.Add(btn);
        newHolder.transform.localScale = Vector3.one;
        newTween.transform.localScale = Vector3.one;
      }
      __instance.PortraitHolders = holders.ToArray();
      __instance.portraitTweens = tweensHolders.ToArray();
    }

    public static bool Prefix(CombatHUDMechwarriorTray __instance, CombatGameState Combat, CombatHUD HUD
      , ref CombatGameState ___Combat, ref CombatHUD ___HUD, ref CombatHUDPortrait[] ___Portraits
    ) {
      try {
        InitAdditinalPortraits(__instance, Combat, HUD);
        ___Combat = Combat;
        ___HUD = HUD;
        ___Portraits = new CombatHUDPortrait[__instance.PortraitHolders.Length];
        for (int index = 0; index < __instance.PortraitHolders.Length; ++index) {
          ___Portraits[index] = __instance.PortraitHolders[index].GetComponentInChildren<CombatHUDPortrait>(true);
          ___Portraits[index].Init(Combat, HUD, __instance.PortraitHolders[index].GetComponent<LayoutElement>(), __instance.portraitTweens[index]);
        }
        CombatHUDMechwarriorTrayEx trayEx = __instance.gameObject.GetComponent<CombatHUDMechwarriorTrayEx>();
        if (trayEx == null) {
          trayEx = __instance.gameObject.AddComponent<CombatHUDMechwarriorTrayEx>();
          trayEx.Instantine(__instance);
        }
        Traverse.Create(__instance).Property<CombatHUDActionButton[]>("ActionButtons").Value = new CombatHUDActionButton[__instance.ActionButtonHolders.Length];
        for (int index = 0; index < __instance.ActionButtonHolders.Length; ++index) {
          __instance.ActionButtons[index] = __instance.ActionButtonHolders[index].GetComponentInChildren<CombatHUDActionButton>(true);
          CombatHUDSidePanelHoverElement panelHoverElement = __instance.ActionButtons[index].GetComponentInChildren<CombatHUDSidePanelHoverElement>(true);
          if ((UnityEngine.Object)panelHoverElement == (UnityEngine.Object)null)
            panelHoverElement = __instance.ActionButtons[index].gameObject.AddComponent<CombatHUDSidePanelHoverElement>();
          panelHoverElement.Init(HUD);
          if (index < 9)
            __instance.ActionButtons[index].Init(Combat, HUD, BTInput.Instance.Combat_Abilities()[index], true);
        }
        trayEx.Init(__instance, HUD, Combat);
        Traverse.Create(__instance).Property<CombatHUDActionButton>("FireButton").Value = __instance.ActionButtons[0];
        Traverse.Create(__instance).Property<CombatHUDActionButton>("MoveButton").Value = __instance.ActionButtons[1];
        Traverse.Create(__instance).Property<CombatHUDActionButton>("SprintButton").Value = __instance.ActionButtons[2];
        Traverse.Create(__instance).Property<CombatHUDActionButton>("JumpButton").Value = __instance.ActionButtons[3];
        Traverse.Create(__instance).Property<CombatHUDActionButton>("DoneWithMechButton").Value = __instance.ActionButtons[9];
        Traverse.Create(__instance).Property<CombatHUDActionButton>("EjectButton").Value = __instance.ActionButtons[10];
        __instance.MeleeButtonHolder.GetComponentInChildren<CombatHUDActionButton>(true).Init(Combat, HUD, BTInput.Instance.Key_Return(), true);
        __instance.DFAButtonHolder.GetComponentInChildren<CombatHUDActionButton>(true).Init(Combat, HUD, BTInput.Instance.Key_Return(), true);
        Traverse.Create(__instance).Property<CombatHUDActionButton>("CommandButton").Value = __instance.CommandButtonHolder.GetComponentInChildren<CombatHUDActionButton>(true);
        __instance.CommandButton.Init(Combat, HUD, BTInput.Instance.Combat_CommandAbility(), true);
        Traverse.Create(__instance).Property<CombatHUDActionButton>("EquipmentButton").Value = __instance.EquipmentButtonHolder.GetComponentInChildren<CombatHUDActionButton>(true);
        __instance.EquipmentButton.Init(Combat, HUD, BTInput.Instance.Key_None(), true);
        __instance.DoneWithMechButton.Init(Combat, HUD, BTInput.Instance.Combat_DoneWithMech(), true);
        __instance.EjectButton.Init(Combat, HUD, BTInput.Instance.Key_None(), true);
        __instance.otherTurnIndicator.Init(HUD);

        Traverse.Create(__instance).Property<CombatHUDActionButton[]>("AbilityButtons").Value = new CombatHUDActionButton[trayEx.ActiveAbilitiesButtons.Count + trayEx.PassiveAbilitiesButtons.Count + 1];
        trayEx.ShowActiveAbilitiesButton = __instance.ActionButtons[5];
        trayEx.ShowPassiveAbilitiesButton = __instance.ActionButtons[6];
        Traverse.Create(__instance).Property<CombatHUDActionButton[]>("AbilityButtons").Value[0] = __instance.ActionButtons[4];
        trayEx.FirstActiveAbility = __instance.ActionButtons[4];
        CombatHUDDeployAutoActivator autoActivator = trayEx.FirstActiveAbility.gameObject.GetComponent<CombatHUDDeployAutoActivator>();
        if(autoActivator == null) {
          autoActivator = trayEx.FirstActiveAbility.gameObject.AddComponent<CombatHUDDeployAutoActivator>();
        }
        autoActivator.Init(trayEx.FirstActiveAbility, HUD);
        for (int index = 0; index < trayEx.ActiveAbilitiesButtons.Count; ++index) {
          Traverse.Create(__instance).Property<CombatHUDActionButton[]>("AbilityButtons").Value[index+1] = trayEx.ActiveAbilitiesButtons[index];
        }
        for (int index = 0; index < trayEx.PassiveAbilitiesButtons.Count; ++index) {
          Traverse.Create(__instance).Property<CombatHUDActionButton[]>("AbilityButtons").Value[index + trayEx.ActiveAbilitiesButtons.Count+1] = trayEx.PassiveAbilitiesButtons[index];
        }

        Traverse.Create(__instance).Property<CombatHUDActionButton[]>("MoraleButtons").Value = new CombatHUDActionButton[2];
        for (int index = 0; index < 2; ++index) {
          Traverse.Create(__instance).Property<CombatHUDActionButton[]>("MoraleButtons").Value[index] = __instance.ActionButtons[index + 7];
        }
        Traverse.Create(__instance).Property<CombatHUDActionButton>("RestartButton").Value = __instance.RestartButtonHolder.GetComponentInChildren<CombatHUDActionButton>(true);
        __instance.RestartButton.Init(Combat, HUD, BTInput.Instance.Key_Return(), true);
        __instance.RestartButton.InitButton(SelectionType.None, (Ability)null, LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.RestartButtonIcon, "BTN_Restart", LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.Tooltip_Restart, (AbstractActor)null);
        __instance.RestartButton.Text.SetText("RESTART", (object[])Array.Empty<object>());
        __instance.RestartButtonHolder.gameObject.SetActive(false);
        Traverse.Create(__instance).Property<CombatHUDMoraleBar>("moraleDisplay").Value = __instance.GetComponentInChildren<CombatHUDMoraleBar>(true);
        Traverse.Create(__instance).Property<CombatHUDMoraleBar>("moraleDisplay").Value.Init(Combat, HUD);
        typeof(CombatHUDMechwarriorTray).GetMethod("SubscribeMessages", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance, new object[] { true });
        __instance.transform.localPosition = __instance.TrayPosDown;
        Traverse.Create(__instance).Property<Vector3>("targetTrayPos").Value = __instance.TrayPosDown;
        Traverse.Create(__instance).Property<Vector3>("lastTrayPos").Value = __instance.TrayPosDown;
        return false;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }

    public static void Postfix(CombatHUDMechwarriorTray __instance, CombatGameState Combat, CombatHUD HUD, CombatHUDPortrait[] ___Portraits) {
      int lance_index = 0;
      int lance_id = 0;
      lancesPortraitHolders.Clear();
      List<CustomLanceInstance> lance = null;
      for (int index = 0; index < ___Portraits.Length; ++index) {
        GameObject holder = __instance.PortraitHolders[index];
        holder.SetActive(false);
        CombatHUDPortrait portrait = ___Portraits[index];
        if (lancesPortraitHolders.Count <= lance_id) {
          lance = new List<CustomLanceInstance>();
          lancesPortraitHolders.Add(lance);
          lance_index = 0;
        }
        bool lanceVehicle = lance_id < CustomLanceHelper.lancesCount() ? CustomLanceHelper.lanceVehicle(lance_id) : true;
        int lanceSize = lance_id < CustomLanceHelper.lancesCount() ? CustomLanceHelper.lanceSize(lance_id) : Core.Settings.ConvoyUnitsCount;
        lance.Add(new CustomLanceInstance(holder, portrait, index, lance_index == 0, lance_id, lance_index, lanceVehicle));
        ++lance_index;
        if (lance_index >= lanceSize) { ++lance_id; };
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDMechwarriorTray))]
  [HarmonyPatch("InitAbilityButtons")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor) })]
  public static class CombatHUDMechwarriorTray_InitAbilityButtons {
    public static bool Prefix(CombatHUDMechwarriorTray __instance, AbstractActor actor) {
      CombatHUDMechwarriorTrayEx trayEx = __instance.gameObject.GetComponent<CombatHUDMechwarriorTrayEx>();
      if (trayEx == null) { return true; }
      Pilot pilot = actor.GetPilot();
      if (pilot != null) {
        List<Ability> activeList = new List<Ability>((IEnumerable<Ability>)pilot.ActiveAbilities);
        activeList.Sort((x, y) => {
          return y.Def.exDef().Priority - x.Def.exDef().Priority;
        });
        if((actor.IsDead == false) && (actor.IsDeployDirector())) {
          trayEx.FirstActiveAbility.InitButton(CombatHUDMechwarriorTray.GetSelectionTypeFromTargeting(activeList[0].Def.Targeting, false), activeList[0], activeList[0].Def.AbilityIcon, activeList[0].Def.Description.Id, activeList[0].Def.Description.Name, actor);
          trayEx.FirstActiveAbility.isClickable = true;
          trayEx.HideActiveAbilities();
          trayEx.HidePassiveAbilities();
          foreach(CombatHUDActionButton btn in trayEx.NormalButtons) {
            if (btn == trayEx.FirstActiveAbility) { continue; }
            btn.gameObject.SetActive(false);
          }
          return false;
        } else {
          foreach (CombatHUDActionButton btn in trayEx.NormalButtons) {
            btn.gameObject.SetActive(true);
          }
        }
        if (activeList.Count > 0) {
          if (activeList[0].Def.Description.Id == CustomAmmoCategoriesPatches.CombatHUDMechwarriorTray_InitAbilityButtons.AbilityName) {
            CustomAmmoCategoriesPatches.CombatHUDMechwarriorTray_InitAbilityButtons.InitAttackGroundAbilityButton(actor, activeList[0], trayEx.FirstActiveAbility);
          } else {
            trayEx.FirstActiveAbility.InitButton(CombatHUDMechwarriorTray.GetSelectionTypeFromTargeting(activeList[0].Def.Targeting, false), activeList[0], activeList[0].Def.AbilityIcon, activeList[0].Def.Description.Id, activeList[0].Def.Description.Name, actor);
            trayEx.FirstActiveAbility.isClickable = true;
          }
          activeList.RemoveAt(0);
        } else {
          trayEx.FirstActiveAbility.InitButton(SelectionType.None, (Ability)null, (SVGAsset)null, "", "", (AbstractActor)null);
          trayEx.FirstActiveAbility.isClickable = false;
          trayEx.FirstActiveAbility.RefreshUIColors();
        }
        for (int index = 0; index < activeList.Count; ++index) {
          if (index >= trayEx.ActiveAbilitiesButtons.Count) { break; }
          if (activeList[index].Def.Description.Id == CustomAmmoCategoriesPatches.CombatHUDMechwarriorTray_InitAbilityButtons.AbilityName) {
            CustomAmmoCategoriesPatches.CombatHUDMechwarriorTray_InitAbilityButtons.InitAttackGroundAbilityButton(actor, activeList[index], trayEx.ActiveAbilitiesButtons[index]);
          } else {
            trayEx.ActiveAbilitiesButtons[index].InitButton(CombatHUDMechwarriorTray.GetSelectionTypeFromTargeting(activeList[index].Def.Targeting, false), activeList[index], activeList[index].Def.AbilityIcon, activeList[index].Def.Description.Id, activeList[index].Def.Description.Name, actor);
            trayEx.ActiveAbilitiesButtons[index].isClickable = true;
          }
        }
        for (int index = activeList.Count; index < trayEx.ActiveAbilitiesButtons.Count; ++index) {
          trayEx.ActiveAbilitiesButtons[index].InitButton(SelectionType.None, (Ability)null, (SVGAsset)null, "", "", (AbstractActor)null);
          trayEx.ActiveAbilitiesButtons[index].isClickable = false;
          trayEx.ActiveAbilitiesButtons[index].RefreshUIColors();
        }
        List<Ability> passiveList = pilot.PassiveAbilities.FindAll((Predicate<Ability>)(x => x.Def.DisplayParams == AbilityDef.DisplayParameters.ShowInMWTRay));
        for (int index = 0; index < passiveList.Count; ++index) {
          if (index >= trayEx.PassiveAbilitiesButtons.Count) { break; }
          trayEx.PassiveAbilitiesButtons[index].InitButton(SelectionType.None, passiveList[index], passiveList[index].Def.AbilityIcon, passiveList[index].Def.Description.Id, passiveList[index].Def.Description.Name, actor);
          trayEx.PassiveAbilitiesButtons[index].isClickable = false;
          trayEx.PassiveAbilitiesButtons[index].RefreshUIColors();
        }
        for (int index = passiveList.Count; index < trayEx.PassiveAbilitiesButtons.Count; ++index) {
          trayEx.PassiveAbilitiesButtons[index].InitButton(SelectionType.None, (Ability)null, (SVGAsset)null, "", "", (AbstractActor)null);
          trayEx.PassiveAbilitiesButtons[index].isClickable = false;
          trayEx.PassiveAbilitiesButtons[index].RefreshUIColors();
        }
      }
      UILookAndColorConstants andColorConstants = LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants;
      trayEx.ShowActiveAbilitiesButton.InitButton(SelectionType.None, (Ability)null, 
        (string.IsNullOrEmpty(Core.Settings.ShowActiveAbilitiesIcon)? andColorConstants.SprintButtonIcon: CustomSvgCache.get(Core.Settings.ShowActiveAbilitiesIcon, trayEx.HUD.Combat.DataManager))
        , CombatHUDMechwarriorTrayEx.SHOW_ACTIVE_BUTTON_GUID, CombatHUDMechwarriorTrayEx.SHOW_ACTIVE_BUTTON_NAME, actor);
      trayEx.ShowActiveAbilitiesButton.isClickable = true;
      trayEx.ShowActiveAbilitiesButton.RefreshUIColors();
      trayEx.ShowPassiveAbilitiesButton.InitButton(SelectionType.None, (Ability)null,
        (string.IsNullOrEmpty(Core.Settings.ShowPassiveAbilitiesIcon) ? andColorConstants.MoveButtonIcon : CustomSvgCache.get(Core.Settings.ShowPassiveAbilitiesIcon, trayEx.HUD.Combat.DataManager))
        , CombatHUDMechwarriorTrayEx.SHOW_PASSIVE_BUTTON_GUID, CombatHUDMechwarriorTrayEx.SHOW_PASSIVE_BUTTON_NAME, actor);
      trayEx.ShowPassiveAbilitiesButton.isClickable = true;
      trayEx.ShowPassiveAbilitiesButton.RefreshUIColors();
      trayEx.HideActiveAbilitiesButton.InitButton(SelectionType.None, (Ability)null,
        (string.IsNullOrEmpty(Core.Settings.HideActiveAbilitiesIcon) ? andColorConstants.SprintButtonIcon : CustomSvgCache.get(Core.Settings.HideActiveAbilitiesIcon, trayEx.HUD.Combat.DataManager))
        , CombatHUDMechwarriorTrayEx.HIDE_ACTIVE_BUTTON_GUID, "RETURN", actor);
      trayEx.HideActiveAbilitiesButton.isClickable = true;
      trayEx.HideActiveAbilitiesButton.RefreshUIColors();
      trayEx.HidePassiveAbilitiesButton.InitButton(SelectionType.None, (Ability)null,
        (string.IsNullOrEmpty(Core.Settings.HidePassiveAbilitiesIcon) ? andColorConstants.MoveButtonIcon : CustomSvgCache.get(Core.Settings.HidePassiveAbilitiesIcon, trayEx.HUD.Combat.DataManager))
        , CombatHUDMechwarriorTrayEx.HIDE_PASSIVE_BUTTON_GUID, "RETURN", actor);
      trayEx.HidePassiveAbilitiesButton.isClickable = true;
      trayEx.HidePassiveAbilitiesButton.RefreshUIColors();

      if (actor.team.CommandAbilities.Count > 0) {
        Ability commandAbility = actor.team.CommandAbilities[0];
        __instance.CommandButton.InitButton(CombatHUDMechwarriorTray.GetSelectionTypeFromTargeting(commandAbility.Def.Targeting, false), commandAbility, commandAbility.Def.AbilityIcon, commandAbility.Def.Description.Id, commandAbility.Def.Description.Name, (AbstractActor)null);
      } else {
        __instance.CommandButton.InitButton(SelectionType.None, (Ability)null, (SVGAsset)null, "", "", (AbstractActor)null);
      }
      return false;
    }
  }
  [HarmonyPatch(typeof(CombatHUDMechwarriorTray))]
  [HarmonyPatch("RefreshActionHotKeys")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatHUDMechwarriorTray_RefreshActionHotKeys {
    public static bool Prefix(CombatHUDMechwarriorTray __instance) {
      Log.LogWrite("CombatHUDMechwarriorTray.RefreshActionHotKeys\n");
      CombatHUDMechwarriorTrayEx trayEx = __instance.gameObject.GetComponent<CombatHUDMechwarriorTrayEx>();
      if (trayEx == null) { return true; }
      if (trayEx.ActiveButtonsLayout.activeSelf) {
        for (int index = 0; index < trayEx.NormalButtons.Count; ++index) {
          if (index < 9) {
            trayEx.NormalButtons[index].SetHotKey(null, true);
          }
        }
        __instance.DoneWithMechButton.SetHotKey(null, true);
        for (int index = 0; index < trayEx.ActiveAbilitiesButtons.Count; ++index) {
          if (index < 9) {
            trayEx.ActiveAbilitiesButtons[index].SetHotKey(Core.Settings.DisableHotKeys?null:BTInput.Instance.Combat_Abilities()[index], true);
          }
        }
        trayEx.HideActiveAbilitiesButton.SetHotKey(Core.Settings.DisableHotKeys ? null : BTInput.Instance.Combat_DoneWithMech(), true);
      } else
      if (trayEx.PassiveButtonsLayout.activeSelf) {
        for (int index = 0; index < trayEx.NormalButtons.Count; ++index) {
          if (index < 9) {
            trayEx.NormalButtons[index].SetHotKey(Core.Settings.DisableHotKeys ? null : BTInput.Instance.Combat_Abilities()[index], true);
          }
        }
        __instance.DoneWithMechButton.SetHotKey(null, true);
        trayEx.HidePassiveAbilitiesButton.SetHotKey(Core.Settings.DisableHotKeys ? null : BTInput.Instance.Combat_DoneWithMech(), true);
      } else {
        for (int index = 0; index < trayEx.NormalButtons.Count; ++index) {
          if (index < 9) {
            trayEx.NormalButtons[index].SetHotKey(Core.Settings.DisableHotKeys ? null : BTInput.Instance.Combat_Abilities()[index], true);
          }
        }
        __instance.DoneWithMechButton.SetHotKey(Core.Settings.DisableHotKeys ? null : BTInput.Instance.Combat_DoneWithMech(), true);
      }
      return false;
    }
  }
  [HarmonyPatch(typeof(CombatHUDMechwarriorTray))]
  [HarmonyPatch("ResetMechwarriorButtons")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor) })]
  public static class CombatHUDMechwarriorTray_ResetMechwarriorButtons {
    public static void Postfix(CombatHUDMechwarriorTray __instance, AbstractActor actor) {
      Log.LogWrite("CombatHUDMechwarriorTray.ResetMechwarriorButtons\n");
      CombatHUDMechwarriorTrayEx trayEx = __instance.gameObject.GetComponent<CombatHUDMechwarriorTrayEx>();
      if (trayEx == null) { return; }
      if (actor == null) {
        trayEx.ShowActiveAbilitiesButton.DisableButton();
        trayEx.ShowPassiveAbilitiesButton.DisableButton();
        trayEx.HideActiveAbilitiesButton.DisableButton();
        trayEx.HidePassiveAbilitiesButton.DisableButton();
        trayEx.HideActiveAbilities();
        trayEx.HidePassiveAbilities();
      } else {
        UILookAndColorConstants andColorConstants = LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants;
        trayEx.ShowActiveAbilitiesButton.InitButton(SelectionType.None, (Ability)null,
          (string.IsNullOrEmpty(Core.Settings.ShowActiveAbilitiesIcon) ? andColorConstants.SprintButtonIcon : CustomSvgCache.get(Core.Settings.ShowActiveAbilitiesIcon, trayEx.HUD.Combat.DataManager))
          , CombatHUDMechwarriorTrayEx.SHOW_ACTIVE_BUTTON_GUID, CombatHUDMechwarriorTrayEx.SHOW_ACTIVE_BUTTON_NAME, actor);
        trayEx.ShowActiveAbilitiesButton.isClickable = true;
        trayEx.ShowActiveAbilitiesButton.RefreshUIColors();
        trayEx.ShowPassiveAbilitiesButton.InitButton(SelectionType.None, (Ability)null,
          (string.IsNullOrEmpty(Core.Settings.ShowPassiveAbilitiesIcon) ? andColorConstants.MoveButtonIcon : CustomSvgCache.get(Core.Settings.ShowPassiveAbilitiesIcon, trayEx.HUD.Combat.DataManager))
          , CombatHUDMechwarriorTrayEx.SHOW_PASSIVE_BUTTON_GUID, CombatHUDMechwarriorTrayEx.SHOW_PASSIVE_BUTTON_NAME, actor);
        trayEx.ShowPassiveAbilitiesButton.isClickable = true;
        trayEx.ShowPassiveAbilitiesButton.RefreshUIColors();
        trayEx.HideActiveAbilitiesButton.InitButton(SelectionType.None, (Ability)null,
          (string.IsNullOrEmpty(Core.Settings.HideActiveAbilitiesIcon) ? andColorConstants.SprintButtonIcon : CustomSvgCache.get(Core.Settings.HideActiveAbilitiesIcon, trayEx.HUD.Combat.DataManager))
          , CombatHUDMechwarriorTrayEx.HIDE_ACTIVE_BUTTON_GUID, "RETURN", actor);
        trayEx.HideActiveAbilitiesButton.isClickable = true;
        trayEx.HideActiveAbilitiesButton.RefreshUIColors();
        trayEx.HidePassiveAbilitiesButton.InitButton(SelectionType.None, (Ability)null,
          (string.IsNullOrEmpty(Core.Settings.HidePassiveAbilitiesIcon) ? andColorConstants.MoveButtonIcon : CustomSvgCache.get(Core.Settings.HidePassiveAbilitiesIcon, trayEx.HUD.Combat.DataManager))
          , CombatHUDMechwarriorTrayEx.HIDE_PASSIVE_BUTTON_GUID, "RETURN", actor);
        trayEx.HidePassiveAbilitiesButton.isClickable = true;
        trayEx.HidePassiveAbilitiesButton.RefreshUIColors();
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDActionButton))]
  [HarmonyPatch("ExecuteClick")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatHUDActionButton_ExecuteClickAbilities {
    public static bool Prefix(CombatHUDActionButton __instance) {
      Log.LogWrite("CombatHUDActionButton.ExecuteClick "+ __instance.GUID + "\n");
      CombatHUD HUD = Traverse.Create(__instance).Property<CombatHUD>("HUD").Value;
      CombatHUDMechwarriorTrayEx trayEx = HUD.MechWarriorTray.gameObject.GetComponent<CombatHUDMechwarriorTrayEx>();
      if (trayEx == null) { return true; };
      if (__instance.GUID == CombatHUDMechwarriorTrayEx.SHOW_ACTIVE_BUTTON_GUID) {
        trayEx.ShowActiveAbilities();
        trayEx.ShowActiveAbilitiesButton.ForceActive();
        trayEx.ShowPassiveAbilitiesButton.ForceInactiveIfEnabled();
        Log.LogWrite(" ShowActiveAbilities\n");
        return false;
      } else
      if (__instance.GUID == CombatHUDMechwarriorTrayEx.SHOW_PASSIVE_BUTTON_GUID) {
        trayEx.ShowPassiveAbilities();
        trayEx.ShowPassiveAbilitiesButton.ForceActive();
        trayEx.ShowActiveAbilitiesButton.ForceInactiveIfEnabled();
        Log.LogWrite(" ShowPassiveAbilities\n");
        return false;
      } else
      if (__instance.GUID == CombatHUDMechwarriorTrayEx.HIDE_PASSIVE_BUTTON_GUID) {
        trayEx.HidePassiveAbilities();
        trayEx.ShowPassiveAbilitiesButton.ForceInactiveIfEnabled();
        trayEx.ShowActiveAbilitiesButton.ForceInactiveIfEnabled();
        Log.LogWrite(" HidePassiveAbilities\n");
        return false;
      } else
      if (__instance.GUID == CombatHUDMechwarriorTrayEx.HIDE_ACTIVE_BUTTON_GUID) {
        trayEx.HideActiveAbilities();
        trayEx.ShowPassiveAbilitiesButton.ForceInactiveIfEnabled();
        trayEx.ShowActiveAbilitiesButton.ForceInactiveIfEnabled();
        Log.LogWrite(" HideActiveAbilities\n");
        return false;
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(CombatHUDMechwarriorTray))]
  [HarmonyPatch("ResetAbilityButtons")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor) })]
  public static class CombatHUDMechwarriorTray_ResetAbilityButtons {
    public static bool Prefix(CombatHUDMechwarriorTray __instance, AbstractActor actor, CombatGameState ___Combat) {
      Log.TWL(0, "CombatHUDMechwarriorTray.ResetAbilityButtons");
      CombatHUDMechwarriorTrayEx trayEx = __instance.gameObject.GetComponent<CombatHUDMechwarriorTrayEx>();
      if (trayEx == null) { return true; }
      try {
        Pilot pilot = actor.GetPilot();
        bool forceInactive = actor.HasActivatedThisRound || actor.MovingToPosition != null || ___Combat.StackManager.IsAnyOrderActive && ___Combat.TurnDirector.IsInterleaved;
        if (pilot == null) { return false; };
        List<Ability> activeList = new List<Ability>((IEnumerable<Ability>)pilot.ActiveAbilities);
        activeList.Sort((x, y) => {
          return y.Def.exDef().Priority - x.Def.exDef().Priority;
        });
        if((actor.IsDead == false) && (actor.IsDeployDirector())) {
          __instance.ResetDeployButton(actor, activeList[0], trayEx.FirstActiveAbility, forceInactive);
          return false;
        }
        List<Ability> passiveList = new List<Ability>();
        passiveList.AddRange((IEnumerable<Ability>)pilot.PassiveAbilities.FindAll((Predicate<Ability>)(x => x.Def.DisplayParams == AbilityDef.DisplayParameters.ShowInMWTRay)));
        if (activeList.Count > 0) {
          if (activeList[0].Def.Description.Id == CustomAmmoCategoriesPatches.CombatHUDMechwarriorTray_InitAbilityButtons.AbilityName) {
            __instance.ResetAttackGroundButton(actor, activeList[0], trayEx.FirstActiveAbility, forceInactive);
          } else {
            __instance.ResetAbilityButton(actor, trayEx.FirstActiveAbility, activeList[0], forceInactive);
          }
          activeList.RemoveAt(0);
        } else {
          __instance.ResetAbilityButton(actor, trayEx.FirstActiveAbility, (Ability)null, false);
        }
        for (int index = 0; index < activeList.Count; ++index) {
          if (activeList[index].Def.Description.Id == CustomAmmoCategoriesPatches.CombatHUDMechwarriorTray_InitAbilityButtons.AbilityName) {
            __instance.ResetAttackGroundButton(actor, activeList[index], trayEx.ActiveAbilitiesButtons[index], forceInactive);
          } else {
            __instance.ResetAbilityButton(actor, trayEx.ActiveAbilitiesButtons[index], activeList[index], forceInactive);
          }
        }
        for (int index = activeList.Count; index < trayEx.ActiveAbilitiesButtons.Count; ++index) {
          __instance.ResetAbilityButton(actor, trayEx.ActiveAbilitiesButtons[index], (Ability)null, false);
        }
        for (int index = 0; index < passiveList.Count; ++index) {
          __instance.ResetAbilityButton(actor, trayEx.PassiveAbilitiesButtons[index], passiveList[index], forceInactive);
        }
        for (int index = passiveList.Count; index < trayEx.PassiveAbilitiesButtons.Count; ++index) {
          __instance.ResetAbilityButton(actor, trayEx.PassiveAbilitiesButtons[index], (Ability)null, false);
        }
        if (actor.team.CommandAbilities.Count > 0) {
          __instance.ResetAbilityButton(actor, __instance.CommandButton, actor.team.CommandAbilities[0], forceInactive);
        } else {
          __instance.ResetAbilityButton(actor, __instance.CommandButton, (Ability)null, false);
        }
        return false;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
    public static void Postfix(CombatHUDMechwarriorTray __instance, AbstractActor actor) {
      CombatHUDMechwarriorTrayEx trayEx = __instance.gameObject.GetComponent<CombatHUDMechwarriorTrayEx>();
      if (trayEx != null) {
        trayEx.ShowActiveAbilitiesButton.ResetButtonIfNotActive(actor);
        trayEx.ShowPassiveAbilitiesButton.ResetButtonIfNotActive(actor);
        trayEx.HideActiveAbilitiesButton.ResetButtonIfNotActive(actor);
        trayEx.HidePassiveAbilitiesButton.ResetButtonIfNotActive(actor);
      }
    }
  }
  [HarmonyPatch(typeof(AbilityDef))]
  [HarmonyPatch("FromJSON")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class AbilityDef_FromJSON {
    private static Dictionary<string, AbilityDefEx> exDefinitions = new Dictionary<string, AbilityDefEx>();
    private static AbilityDefEx empty = new AbilityDefEx();
    public static AbilityDefEx exDef(this AbilityDef aDef) {
      if (exDefinitions.TryGetValue(aDef.Id, out AbilityDefEx res)) {
        return res;
      }
      return empty;
    }
    public static void Prefix(AbilityDef __instance, ref string json, ref AbilityDefEx __state) {
      Log.LogWrite("AbilityDef.FromJSON");
      JObject jAbilityDef = JObject.Parse(json);
      __state = new AbilityDefEx();
      __state.Priority = 0;
      if (jAbilityDef["Priority"] != null) { __state.Priority = (int)jAbilityDef["Priority"]; }
    }
    public static void Postfix(AbilityDef __instance, string json, ref AbilityDefEx __state) {
      if (exDefinitions.ContainsKey(__instance.Id)) { exDefinitions[__instance.Id] = __state; } else { exDefinitions.Add(__instance.Id, __state); }
    }
  }
}