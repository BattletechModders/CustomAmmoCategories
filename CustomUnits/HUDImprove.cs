using BattleTech;
using BattleTech.Data;
using BattleTech.Save;
using BattleTech.Save.Test;
using BattleTech.Serialization;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using BattleTech.UI.Tooltips;
using CustAmmoCategoriesPatches;
using DG.Tweening;
using Harmony;
using HBS;
using HBS.Collections;
using Localize;
using Newtonsoft.Json;
using SVGImporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CustomUnits {
  public class LanceConfigurationState {
    public int lanceId;
    public int lanceStartPos;
    public void Clear() { lanceId = 0; lanceStartPos = 0; }
    public LanceConfigurationState() { lanceId = 0; lanceStartPos = 0; }
  }
  public class LanceConfigutationNextSlot : LanceConfigutationSwitchBtn {
    public override void InitVisuals() {
      if ((caption != null)&&(tooltip != null)) {
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
        screen.UnitWidgets()[index].gameObject.SetActive((index >= screen.CurViewStartPosition())&&(index < (screen.CurViewStartPosition() + 4)));
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
          if(bttn2_Text != null) {
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
    public Dictionary<string,int> loadout;
    public PlayerLancesLoadout() { loadout = new Dictionary<string,int>(); }
  }
  [HarmonyPatch(typeof(SimGameState), "Dehydrate")]
  public static class SimGameState_Dehydrate {
    static void Prefix(SimGameState __instance, SerializableReferenceContainer references) {
      Log.TWL(0, "SimGameState.Dehydrate");
      try {
        Log.WL(1, "playerLanceLoadout(" + CustomLanceHelper.playerLanceLoadout.loadout.Count + "):");
        foreach (var guid in CustomLanceHelper.playerLanceLoadout.loadout) {
          Log.WL(2, "GUID:" + guid.Key+"->"+guid.Value);
        }
        Statistic playerMechContent = __instance.CompanyStats.GetStatistic(CustomLanceHelper.SaveReferenceName);
        if(playerMechContent == null) {
          __instance.CompanyStats.AddStatistic<string>(CustomLanceHelper.SaveReferenceName,JsonConvert.SerializeObject(CustomLanceHelper.playerLanceLoadout));
        } else {
          playerMechContent.SetValue<string>(JsonConvert.SerializeObject(CustomLanceHelper.playerLanceLoadout));
        }
        //references.AddItem<PlayerAllyLanceContent>(CustomLanceHelper.SaveReferenceName, CustomLanceHelper.playerAllyContent);
      }catch(Exception e) {
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
        Log.WL(1, "playerLanceLoadout(" + CustomLanceHelper.playerLanceLoadout.loadout.Count+ "):");
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
  public static class SimGameState_SaveLastLance{
    public static bool Prefix(SimGameState __instance, LanceConfiguration config,ref List<string> ___LastUsedMechs,ref List<string> ___LastUsedPilots) {
      ___LastUsedMechs = new List<string>();
      ___LastUsedPilots = new List<string>();
      foreach (SpawnableUnit lanceUnit in config.GetLanceUnits("bf40fd39-ccf9-47c4-94a6-061809681140")) {
        string str1 = "";
        string str2 = "";
        SpawnableUnit spawnableUnit = lanceUnit;
        if (spawnableUnit != null) {
          if (spawnableUnit.Unit != null && spawnableUnit.Pilot != null) {
            str1 = spawnableUnit.Unit.GUID;
            str2 = spawnableUnit.Pilot.Description.Id;
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
      return false;
    }
  }
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("GetLastLance")]
  [HarmonyPatch(MethodType.Normal)]
  public static class SimGameState_GetLastLance {
    public static bool Prefix(SimGameState __instance, ref LanceConfiguration __result, ref List<string> ___LastUsedMechs, ref List<string> ___LastUsedPilots) {
      LanceConfiguration lanceConfiguration = new LanceConfiguration();
      if (___LastUsedMechs != null && ___LastUsedPilots != null) {
        for (int index = 0; index < 4; ++index) {
          string id = string.Empty;
          if (___LastUsedMechs.Count > index)
            id = ___LastUsedMechs[index];
          string pilotID = string.Empty;
          if (___LastUsedPilots.Count > index)
            pilotID = ___LastUsedPilots[index];
          MechDef mechById = __instance.GetMechByID(id);
          PilotDef pilotDef = __instance.GetPilot(pilotID)?.pilotDef;
          if (mechById != null || pilotDef != null) {
            lanceConfiguration.AddUnit("bf40fd39-ccf9-47c4-94a6-061809681140", (PilotableActorDef)mechById, pilotDef);
          } else {
            lanceConfiguration.AddUnit("bf40fd39-ccf9-47c4-94a6-061809681140", string.Empty, string.Empty, UnitType.UNDEFINED);
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
    public static void MissionControlDetected() { missionControlDetected = true; }
    public static void setLancesCount(int size) {
      if (size <= 0) { size = 1; };
      if(lancesData.Count > size) {
        lancesData.RemoveRange(size, lancesData.Count - size);
      }else if(lancesData.Count < size) {
        for(int index=lancesData.Count;index < size; ++index) {
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
    public static void setOverallDeployCount(int value) { f_overallDeployCount = Mathf.Min(value,fullSlotsCount()); }
    public static void playerControl(int mechs, int vehicles) { f_playerControlMechs = mechs; f_playerControlVehicles = vehicles; }
    public static int overallDeployCount() { return missionControlDetected?f_overallDeployCount:4; }
    public static int playerControlVehicles() { return missionControlDetected?f_playerControlVehicles:-1; }
    public static int playerControlMechs() { return missionControlDetected?f_playerControlMechs:-1; }

    public static int allowLanceSize(int lanceid) {
      if (fallbackAllow > 0) { return lanceid == 0 ? fallbackAllow : 0; }
      return lancesData[lanceid].allow;
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
  }
  [HarmonyPatch(typeof(AAR_UnitsResult_Screen), "InitializeData")]
  public static class AAR_UnitsResult_Screen_InitializeSkirmishData {
    private static FieldInfo f_UnitWidgets = typeof(AAR_UnitsResult_Screen).GetField("UnitWidgets", BindingFlags.Instance | BindingFlags.NonPublic);
    public static List<AAR_UnitStatusWidget> UnitWidgets(this AAR_UnitsResult_Screen screen) { return (List<AAR_UnitStatusWidget>)f_UnitWidgets.GetValue(screen); }
    private static int startUnitPosition = 0;
    public static int CurViewStartPosition(this AAR_UnitsResult_Screen screen) { return startUnitPosition; }
    public static void CurViewStartPosition(this AAR_UnitsResult_Screen screen, int value) { startUnitPosition = value; }
    static void Prefix(AAR_UnitsResult_Screen __instance, ref List<AAR_UnitStatusWidget> ___UnitWidgets) {
      Log.TWL(0, "AAR_UnitsResult_Screen.InitializeData", true);
      RectTransform rectTR = __instance.GetComponent<RectTransform>();
      Vector3[] corners = new Vector3[4];
      rectTR.GetLocalCorners(corners);

      Transform buttonPanel_SKIRMISH = __instance.transform.FindRecursive("buttonPanel-SKIRMISH");
      Transform buttonPanel = __instance.transform.FindRecursive("buttonPanel");
      if ((buttonPanel_SKIRMISH != null)&&(buttonPanel != null)) {
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
    static void Prefix(AAR_SkirmishResult_Screen __instance,ref List<AAR_UnitStatusWidget> ___UnitWidgets) {
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
    static void Postfix(AAR_SkirmishResult_Screen __instance,ref List<AAR_UnitStatusWidget> ___UnitWidgets, ref List<UnitResult> ___PlayerUnitResults, ref List<UnitResult> ___OpponentUnitResults, ref int ___numUnits, ref int ___numOpponentUnits,ref Contract ___theContract,ref SimGameState ___simState,ref DataManager ___dm) {
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
          if (___UnitResults[index].pilot.pilotDef.isVehicleCrew() == false) {
            ___UnitWidgets[index].FillInData(experienceEarned);
          } else {
            ___UnitWidgets[index].FillInData(0);
          }
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
      if (__instance.isVehicleCrew()) { value = 0; };
      Log.TWL(0, "PilotDef.SetUnspentExperience isVehicleCrew:"+ __instance.isVehicleCrew()+" value:"+value);
      return true;
    }
  }
  [HarmonyPatch(typeof(Pilot), "AddExperience")]
  public static class Pilot_AddExperience {
    static bool Prefix(Pilot __instance, ref int value) {
      if (__instance.pilotDef.isVehicleCrew()) {
        value = 0;
        __instance.StatCollection.Set("ExperienceUnspent",0);
      };
      Log.TWL(0, "Pilot.AddExperience isVehicleCrew:" + __instance.pilotDef.isVehicleCrew() + " value:" + value+" unspent:"+__instance.UnspentXP);
      return true;
    }
  }
  [HarmonyPatch(typeof(LanceConfiguratorPanel), "OnConfirmClicked")]
  public static class LanceConfiguratorPanel_OnConfirmClicked {
    static bool Prefix(LanceConfiguratorPanel __instance, ref LanceLoadoutSlot[] ___loadoutSlots) {
      int overallSlotsCount = 0;
      foreach(LanceLoadoutSlot slot in ___loadoutSlots){
        if (slot.SelectedMech != null) { ++overallSlotsCount; };
      }
      if (overallSlotsCount > CustomLanceHelper.overallDeployCount()) {
        GenericPopupBuilder.Create("Lance Cannot Be Deployed", Strings.T("Deploying units count {0} exceed limit {1}",overallSlotsCount,CustomLanceHelper.overallDeployCount())).AddFader(new UIColorRef?(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PopupBackfill), 0.0f, true).Render();
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
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Method(typeof(PilotDef), "SetUnspentExperience");
      var replacementMethod = AccessTools.Method(typeof(SimGameState_ResolveCompleteContract), nameof(SetUnspentExperience));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }

    public static void SetUnspentExperience(this PilotDef pilotDef, int value) {
      Log.TWL(0, "SimGameState.ResolveCompleteContract.PilotDef.SetUnspentExperience "+ pilotDef.Description.Id +" crew:"+pilotDef.isVehicleCrew()+" value:"+value);
      if (pilotDef.isVehicleCrew()) {
        pilotDef.SetUnspentExperience(0);
      } else {
        pilotDef.SetUnspentExperience(value);
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
      List<LoadoutContent> spawnMechList = new List<LoadoutContent>();
      List<LoadoutContent> spawnVehicleList = new List<LoadoutContent>();
      foreach (SpawnableUnit unit in lanceUnits) {
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
        bool isVehicle = false;
        if (forcedMech != null) { isVehicle = forcedMech.MechDef.Chassis.IsFake(forcedMech.MechDef.ChassisID); } else
        if (forcedPilot != null) { isVehicle = forcedPilot.Pilot.pilotDef.isVehicleCrew(); }
        if (isVehicle) {
          spawnVehicleList.Add(new LoadoutContent(forcedMech, forcedPilot));
        } else {
          spawnMechList.Add(new LoadoutContent(forcedMech, forcedPilot));
        }
      }
      int mech_index = 0;
      int vehcile_index = 0;
      for (int index = 0; index < ___loadoutSlots.Length; ++index) {
        LanceLoadoutSlot loadoutSlot = ___loadoutSlots[index];
        if (loadoutSlot.curLockState == LanceLoadoutSlot.LockState.Full) { continue; }
        IMechLabDraggableItem forcedMech = (IMechLabDraggableItem)null;
        SGBarracksRosterSlot forcedPilot = (SGBarracksRosterSlot)null;
        if (loadoutSlot.isForVehicle() && (vehcile_index < spawnVehicleList.Count)) {
          forcedMech = spawnVehicleList[vehcile_index].mech;
          forcedPilot = spawnVehicleList[vehcile_index].pilot;
          ++vehcile_index;
        } else if (mech_index < spawnMechList.Count) {
          forcedMech = spawnMechList[mech_index].mech;
          forcedPilot = spawnMechList[mech_index].pilot;
          ++mech_index;
        }
        if (loadoutSlot.isMechLocked) { forcedMech = null; }
        if (loadoutSlot.isPilotLocked) { forcedPilot = null; }
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
    public static void Prefix(LanceLoadoutSlot __instance, LanceConfiguratorPanel LC, SimGameState sim, DataManager dataManager, bool useDragAndDrop, ref bool locked, float minTonnage, float maxTonnage) {
      Log.TWL(0, "LanceLoadoutSlot.SetData " + __instance.GetInstanceID());
      try {
        bool overrideLocked = __instance.isOverrideLockedState();
        if (overrideLocked == false) { return; }
        int lanceid = __instance.LanceId();
        int lancepos = __instance.LancePos();
        Log.WL(1, "lanceid:" + lanceid + " lancepos:" + lancepos + " maxTonnage:" + maxTonnage + " locked:" + locked + " allow:" + (lanceid == -1 ? "unlim" : CustomLanceHelper.allowLanceSize(lanceid).ToString()));
        if ((lanceid == -1) || (lancepos == -1)) { return; }
        if (maxTonnage == 0f) { return; }
        if (locked == true) { return; }
        locked = CustomLanceHelper.allowLanceSize(lanceid) <= lancepos;
        Log.WL(1, "locked:" + locked + " allow:" + CustomLanceHelper.allowLanceSize(lanceid));
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
    public static bool isVehicleCrew(this PilotDef pilotDef) { return pilotDef.PilotTags.Contains("pilot_vehicle_crew"); }
    public static bool Prefix(LanceLoadoutSlot __instance, IMechLabDraggableItem item, bool validate, bool __result, LanceConfiguratorPanel ___LC) {
      Log.TWL(0, "LanceLoadoutSlot.OnAddItem " + __instance.GetInstanceID());
      try {
        if ((item.ItemType != MechLabDraggableItemType.Mech) && (item.ItemType != MechLabDraggableItemType.Pilot)) { return true; }
        bool isVehicle = __instance.isForVehicle();
        if (item.ItemType == MechLabDraggableItemType.Mech) {
          LanceLoadoutMechItem lanceLoadoutMechItem = item as LanceLoadoutMechItem;
          if (lanceLoadoutMechItem.ChassisDef.IsFake(lanceLoadoutMechItem.MechDef.ChassisID)) {
            if (isVehicle == false) {
              if (___LC != null) { ___LC.ReturnItem(item); }
              __result = false;
              GenericPopupBuilder.Create("Cannot place vehicle", Strings.T("Vehicle cannot be placed to mech slot")).AddFader(new UIColorRef?(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PopupBackfill), 0.0f, true).Render();
              return false;
            }
          } else {
            if (isVehicle == true) {
              if (___LC != null) { ___LC.ReturnItem(item); }
              __result = false;
              GenericPopupBuilder.Create("Cannot place mech", Strings.T("Mech cannot be placed to vehicle slot")).AddFader(new UIColorRef?(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PopupBackfill), 0.0f, true).Render();
              return false;
            }
          }
        } else if (item.ItemType == MechLabDraggableItemType.Pilot) {
          SGBarracksRosterSlot barracksRosterSlot = item as SGBarracksRosterSlot;
          bool isVehicleCrew = barracksRosterSlot.Pilot.pilotDef.isVehicleCrew();
          if (isVehicleCrew) {
            if (isVehicle == false) {
              if (___LC != null) { ___LC.ReturnItem(item); }
              __result = false;
              GenericPopupBuilder.Create("Cannot place pilot", Strings.T("Vehicle pilot cannot be placed to mechwarrior slot")).AddFader(new UIColorRef?(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PopupBackfill), 0.0f, true).Render();
              return false;
            }
          } else {
            if (isVehicle == true) {
              if (___LC != null) { ___LC.ReturnItem(item); }
              __result = false;
              GenericPopupBuilder.Create("Cannot place pilot", Strings.T("Mechwarrior cannot be placed to vehicle pilot slot")).AddFader(new UIColorRef?(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PopupBackfill), 0.0f, true).Render();
              return false;
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
    public static Dictionary<LanceLoadoutSlot, int> slotsLancePos = new Dictionary<LanceLoadoutSlot, int>();
    public static Dictionary<LanceLoadoutSlot, bool> isFillerSlot = new Dictionary<LanceLoadoutSlot, bool>();
    public static bool isForVehicle(this LanceLoadoutSlot slot) { if (isVehicleSlot.TryGetValue(slot, out bool result)) { return result; } else { return false; }; }
    public static void isForVehicle(this LanceLoadoutSlot slot, bool val) { if (isVehicleSlot.ContainsKey(slot) == false) { isVehicleSlot.Add(slot, val); } else { isVehicleSlot[slot] = val; }; }
    public static bool isOverrideLockedState(this LanceLoadoutSlot slot) { if (isOverLockedState.TryGetValue(slot, out bool result)) { return result; } else { return false; }; }
    public static void isOverrideLockedState(this LanceLoadoutSlot slot, bool val) { if (isOverLockedState.ContainsKey(slot) == false) { isOverLockedState.Add(slot, val); } else { isOverLockedState[slot] = val; }; }
    public static bool isFiller(this LanceLoadoutSlot slot) { if (isFillerSlot.TryGetValue(slot, out bool result)) { return result; } else { return true; }; }
    public static int LanceId(this LanceLoadoutSlot slot) { if (slotsLanceIds.TryGetValue(slot, out int result)) { return result; } else { return -1; }; }
    public static void LanceId(this LanceLoadoutSlot slot, int val) { if (slotsLanceIds.ContainsKey(slot) == false) { slotsLanceIds.Add(slot, val); } else { slotsLanceIds[slot] = val; }; }
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
          } else if (contract.Override.maxNumberOfPlayerUnits < 4) {
            CustomLanceHelper.setFallbackAllow(contract.Override.maxNumberOfPlayerUnits);
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
      HUD.ShowSidePanelActorInfo(!HUD.ShowSidePanelActorInfo());
    }
    public void LateUpdate() {
      if (image != null) {
        image.color = HUD.ShowSidePanelActorInfo() ? Color.white : Color.grey;
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
    private int curLanceId = 0;
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
      for(int index = lanceIcons.Count; index < Core.Settings.LancesIcons.Count; ++index) {
        string iconid = Core.Settings.LancesIcons[index];
        SVGAsset icon = HUD.Combat.DataManager.GetObjectOfType<SVGAsset>(iconid, BattleTechResourceType.SVGAsset);
        if (icon != null) {
          Log.WL(1, "icon " + iconid + " found");
          lanceIcons.Add(icon);
        } else {
          Log.WL(1, "icon " + iconid + " not found");
        }
      }
      HUD.MechWarriorTray.SetLanceSwitcher(this);
      curLanceId = HUD.MechWarriorTray.CurrentLance();
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
      __instance.mwCommandButton(___HUD).SetActive(true);
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
      __instance.mwCommandButton(___HUD).SetActive(true);
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
    public static bool Prefix(CombatHUDMechwarriorTray __instance, Team team, Team ___displayedTeam) {
      ___displayedTeam = team;
      Log.TWL(0, "CombatHUDMechwarriorTray.RefreshTeam");
      try {
        Dictionary<int, CustomLanceInstance> avaibleSlots = new Dictionary<int, CustomLanceInstance>();
        foreach (var lance in __instance.exPortraitHolders()) {
          foreach (var slot in lance) {
            avaibleSlots.Add(slot.index, slot);
            slot.holder.SetActive(false);
          }
        }
        Dictionary<int, AbstractActor> predefinedActors = new Dictionary<int, AbstractActor>();
        List<AbstractActor> mechs = new List<AbstractActor>();
        List<AbstractActor> vehicles = new List<AbstractActor>();
        foreach (AbstractActor unit in ___displayedTeam.units) {
          string defGUID = string.Empty;
          bool isVehicle = false;
          if (unit.UnitType == UnitType.Mech) {
            defGUID = (unit as Mech).MechDef.GUID;
          } else if (unit.UnitType == UnitType.Vehicle) {
            defGUID = (unit as Vehicle).VehicleDef.GUID;
            isVehicle = false;
          } else {
            continue;
          }
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
            if (__instance.exPortraitHolders()[lance_id][lance_index].portrait.DisplayedActor != null) { nonemptyLance = lance_id; break; }
          }
          if (nonemptyLance != -1) { break; }
        }
        if (nonemptyLance < 0) { nonemptyLance = 0; }
        //if (nonemptyLance != 0) { __instance.HideLance(0); };
        __instance.ShowLance(nonemptyLance);
        Log.WL(1, "success");
      }catch(Exception e) {
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
  [HarmonyPatch(typeof(CombatHUDMechwarriorTray))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
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
    public static void Prefix(CombatHUDMechwarriorTray __instance, CombatGameState Combat, CombatHUD HUD) {
      List<GameObject> holders = new List<GameObject>();
      holders.AddRange(__instance.PortraitHolders);
      List<HBSDOTweenToggle> tweensHolders = new List<HBSDOTweenToggle>();
      tweensHolders.AddRange(__instance.portraitTweens);
      GameObject srcHolder = holders[0];

      GameObject srcTween = tweensHolders[0].gameObject;
      HorizontalLayoutGroup layout = srcHolder.transform.parent.gameObject.GetComponent<HorizontalLayoutGroup>();
      layout.childAlignment = TextAnchor.MiddleLeft;
      int all_lances_size = CustomLanceHelper.fullSlotsCount();
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
        lance.Add(new CustomLanceInstance(holder, portrait, index, lance_index == 0, lance_id, lance_index, CustomLanceHelper.lanceVehicle(lance_id)));
        ++lance_index;
        if (lance_index >= CustomLanceHelper.lanceSize(lance_id)) { ++lance_id; };
      }
    }
  }

}