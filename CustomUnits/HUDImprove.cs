﻿/*  
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
using BattleTech.Framework;
using BattleTech.Save;
using BattleTech.Save.Test;
using BattleTech.Serialization;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using BattleTech.UI.Tooltips;
using CustAmmoCategories;
using CustAmmoCategoriesPatches;
using CustomAmmoCategoriesHelper;
using CustomAmmoCategoriesPatches;
using DG.Tweening;
using HarmonyLib;
using HBS;
using HBS.Collections;
using Localize;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SVGImporter;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CustomUnits {
  public class AbilityDefEx {
    public int Priority { get; set; } = 100;
    public bool CanBeUsedInShutdown { get; set; } = false;
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
      //state.lanceStartPos += 4;
      //int maxStartPos = CustomLanceHelper.lanceSize(state.lanceId) - 4;
      //if (maxStartPos < 0) { maxStartPos = 0; };
      //if (state.lanceStartPos > maxStartPos) { state.lanceStartPos = maxStartPos; }
      //LanceConfiguratorPanel_SetData.updateSlots(this.customLayout);
      base.OnClick();
    }
  }
  public class LanceConfigutationNextLance : LanceConfigutationSwitchBtn {
    public override void InitVisuals() {
      if ((caption != null) && (tooltip != null)) {
        tooltip.SetString("Switch Lance");
        caption.SetText(customLayout.LayoutDef.dropLances[customLayout.currentLanceIndex].description.Name);
        base.InitVisuals();
      }
    }
    public override void OnClick() {
      customLayout.currentLanceIndex = (customLayout.currentLanceIndex + 1) % customLayout.LayoutDef.dropLances.Count;
      caption.SetText(customLayout.LayoutDef.dropLances[customLayout.currentLanceIndex].description.Name);
      customLayout.UpdateSlots();
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
      //state.lanceStartPos -= 4;
      //if (state.lanceStartPos < 0) { state.lanceStartPos = 0; };
      //LanceConfiguratorPanel_SetData.updateSlots(this.customLayout);
      base.OnClick();
    }

  }
  public class LanceConfigutationSwitchBtn : CUCustomButton {
    //public LanceConfigurationState state;
    public ShuffleLanceSlotsLayout customLayout { get; set; }
    public void Init(ShuffleLanceSlotsLayout customLayout) {
      //this.state = state;
      this.customLayout = customLayout;
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
      if (this.screen.CurViewStartPosition() > this.screen.UnitWidgets.Count - 4) { this.screen.CurViewStartPosition(this.screen.UnitWidgets.Count - 4); }
      if (this.screen.CurViewStartPosition() < 0) { this.screen.CurViewStartPosition(0); }
      for (int index = 0; index < screen.UnitWidgets.Count; ++index) {
        screen.UnitWidgets[index].gameObject.SetActive((index >= screen.CurViewStartPosition()) && (index < (screen.CurViewStartPosition() + 4)));
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
      if (this.screen.CurViewStartPosition() > this.screen.UnitWidgets.Count - 4) { this.screen.CurViewStartPosition(this.screen.UnitWidgets.Count - 4); }
      if (this.screen.CurViewStartPosition() < 0) { this.screen.CurViewStartPosition(0); }
      for (int index = 0; index < screen.UnitWidgets.Count; ++index) {
        screen.UnitWidgets[index].gameObject.SetActive((index >= screen.CurViewStartPosition()) && (index < (screen.CurViewStartPosition() + 4)));
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
      //Log.M?.TWL(0, "LanceConfigutationSwitchBtn.OnPointerClick");
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
      Log.M?.TWL(0, "CUCustomButton.Awake");
      try {
        GameObject bttn2_Fill = this.transform.Find("bttn2_Fill").gameObject;
        btnColor = bttn2_Fill.GetComponent<SVGImage>();
        btnColor.color = UIManager.Instance.UIColorRefs.lightGray;
        GameObject bttn2_border = bttn2_Fill.transform.FindRecursive("bttn2_border").gameObject;
        borderColor = bttn2_border.GetComponent<SVGImage>();
        borderColor.color = UIManager.Instance.UIColorRefs.orange;
        HBSTooltip hbsTooltip = this.gameObject.GetComponent<HBSTooltip>();
        if (hbsTooltip != null) {
          tooltip = hbsTooltip.defaultStateData;
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
        Log.M?.TWL(0, e.ToString());
        UIManager.logger.LogException(e);
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
    public Dictionary<string, int> loadout { get; set; }
    public PlayerLancesLoadout() { loadout = new Dictionary<string, int>(); }
  }
  [HarmonyPatch(typeof(SimGameState), "Dehydrate")]
  public static class SimGameState_Dehydrate {
    static void Prefix(SimGameState __instance, SerializableReferenceContainer references) {
      Log.M?.TWL(0, "SimGameState.Dehydrate");
      try {
        Log.M?.WL(1, "playerLanceLoadout(" + CustomLanceHelper.playerLanceLoadout.loadout.Count + "):");
        foreach (var guid in CustomLanceHelper.playerLanceLoadout.loadout) {
          Log.M?.WL(2, "GUID:" + guid.Key + "->" + guid.Value);
        }
        Statistic playerMechContent = __instance.CompanyStats.GetStatistic(CustomLanceHelper.SaveReferenceName);
        if (playerMechContent == null) {
          __instance.CompanyStats.AddStatistic<string>(CustomLanceHelper.SaveReferenceName, JsonConvert.SerializeObject(CustomLanceHelper.playerLanceLoadout));
        } else {
          playerMechContent.SetValue<string>(JsonConvert.SerializeObject(CustomLanceHelper.playerLanceLoadout));
        }
        if (__instance.LastUsedMechs == null) { __instance.LastUsedMechs = new List<string>(); }
        if (__instance.LastUsedPilots == null) { __instance.LastUsedPilots = new List<string>(); }
        for (int i = 0; i < __instance.LastUsedPilots.Count; ++i) {
          if (__instance.LastUsedPilots[i] == null) {
            __instance.LastUsedPilots[i] = string.Empty;
          }
        }
        for (int i = 0; i < __instance.LastUsedMechs.Count; ++i) {
          if (__instance.LastUsedMechs[i] == null) {
            __instance.LastUsedMechs[i] = string.Empty;
          }
        }
        //references.AddItem<PlayerAllyLanceContent>(CustomLanceHelper.SaveReferenceName, CustomLanceHelper.playerAllyContent);
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
        SimGameState.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(SimGameState), "Rehydrate")]
  public static class SimGameState_Rehydrate {
    static void Postfix(SimGameState __instance, GameInstanceSave gameInstanceSave) {
      Log.M?.TWL(0, "SimGameState.Rehydrate");
      try {
        Statistic playerMechContent = __instance.CompanyStats.GetStatistic(CustomLanceHelper.SaveReferenceName);
        if (playerMechContent == null) {
          CustomLanceHelper.playerLanceLoadout.loadout.Clear();
        } else {
          CustomLanceHelper.playerLanceLoadout = JsonConvert.DeserializeObject<PlayerLancesLoadout>(playerMechContent.Value<string>());
        }
        //CustomLanceHelper.playerAllyContent = references.GetItem<PlayerAllyLanceContent>(CustomLanceHelper.SaveReferenceName);
        Log.M?.WL(1, "playerLanceLoadout(" + CustomLanceHelper.playerLanceLoadout.loadout.Count + "):");
        foreach (var guid in CustomLanceHelper.playerLanceLoadout.loadout) {
          Log.M?.WL(2, "GUID:" + guid.Key + "->" + guid.Value);
        }
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
        SimGameState.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("FillContractLance")]
  [HarmonyPatch(MethodType.Normal)]
  public static class SimGameState_FillContractLance {
    public static void Prefix(ref bool __runOriginal, SimGameState __instance) {
      if (!__runOriginal) { return; }
      Log.M?.TWL(0, "SimGameState.FillContractLance");
      Contract selectedContract = __instance.SelectedContract;
      LanceConfiguration lanceConfiguration = __instance.RoomManager.CmdCenterRoom.lanceConfigBG.LC.CreateLanceConfiguration();
      __instance.SaveLastLance(lanceConfiguration);
      LanceConfiguration clearLanceConfiguration = lanceConfiguration.clearConfig();
      Log.M?.WL(1, "clearLanceConfiguration valid:" + clearLanceConfiguration.LanceValid);
      foreach (string key in clearLanceConfiguration.Lances.Keys) {
        if (!string.IsNullOrEmpty(key)) {
          SpawnableUnit[] lanceUnits = clearLanceConfiguration.GetLanceUnits(key);
          selectedContract.Lances.AddUnits((IEnumerable<SpawnableUnit>)lanceUnits);
        }
      }
      __runOriginal = false;
      return;
    }
  }
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("OnLanceConfiguratorAccept")]
  [HarmonyPatch(MethodType.Normal)]
  public static class SimGameState_OnLanceConfiguratorAccept {
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
    public static void Prefix(ref bool __runOriginal, SimGameState __instance) {
      if (!__runOriginal) { return; }
      Log.M?.TWL(0, "SimGameState.OnLanceConfiguratorAccept");
      LanceConfiguration lanceConfiguration = __instance.RoomManager.CmdCenterRoom.lanceConfigBG.LC.CreateLanceConfiguration();
      Log.M?.WL(1, "lanceConfiguration created " + (lanceConfiguration == null ? "null" : "not null"));
      __instance.SaveLastLance(lanceConfiguration);
      LanceConfiguration clearLanceConfiguration = lanceConfiguration.clearConfig();
      Log.M?.WL(1, "clearLanceConfiguration valid:" + clearLanceConfiguration.LanceValid);
      foreach (var lance in clearLanceConfiguration.Lances) {
        foreach (var unit in lance.Value) {
          Log.M?.WL(2, "unit:" + unit.PilotId + ":" + unit.UnitId + ":" + unit.CanLoad());
        }
      }
      __instance.OnContractReady(clearLanceConfiguration);
      __runOriginal = false;
      return;
    }
  }
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("GetLastLance")]
  [HarmonyPatch(MethodType.Normal)]
  public static class SimGameState_GetLastLance {
    public static void Prefix(ref bool __runOriginal, SimGameState __instance, ref LanceConfiguration __result) {
      LanceConfiguration lanceConfiguration = new LanceConfiguration();
      Log.M?.TWL(0, "SimGameState.GetLastLance");
      if ((__instance.LastUsedMechs == null) || (__instance.LastUsedPilots == null)) {
        Log.M?.WL(1, "LastUsed arrays is null");
        __result = lanceConfiguration;
        __runOriginal = false;
        return;
      }
      Log.M?.WL(1, "pilots:");
      for (int i = 0; i < __instance.LastUsedPilots.Count; ++i) {
        Log.M?.WL(2, "[" + i + "] " + __instance.LastUsedPilots[i]);
      }
      Log.M?.WL(1, "units:");
      for (int i = 0; i < __instance.LastUsedMechs.Count; ++i) {
        Log.M?.WL(2, "[" + i + "] " + __instance.LastUsedMechs[i]);
      }
      if (__instance.LastUsedMechs != null && __instance.LastUsedPilots != null) {
        int counter = Mathf.Max(__instance.LastUsedMechs.Count, __instance.LastUsedPilots.Count);
        for (int index = 0; index < counter; ++index) {
          string id = string.Empty;
          if (__instance.LastUsedMechs.Count > index) { id = __instance.LastUsedMechs[index]; };
          string pilotID = string.Empty;
          if (__instance.LastUsedPilots.Count > index) { pilotID = __instance.LastUsedPilots[index]; }
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
      __runOriginal = false;
      return;
    }
  }
  public static class CustomLanceHelper {
    public static readonly string SaveReferenceName = "PlayedAllyLanceContent";
    public static PlayerLancesLoadout playerLanceLoadout { get; set; } = new PlayerLancesLoadout();
    public static List<HotDropDefinition> hotdropLayout { get; set; } = new List<HotDropDefinition>();
    public static void HotDrop(List<Vector3> dropPositions, string spawnerGUID) {
      EncounterLayerParent encounterLayerParent = UnityGameInstance.BattleTechGame.Combat.EncounterLayerData.GetComponentInParent<EncounterLayerParent>();
      HotDropManager hotdropManager = encounterLayerParent.GetComponent<HotDropManager>();
      hotdropManager.HotDrop(dropPositions, spawnerGUID);
    }
    public static void PushDropLayout(string id, List<List<string>> layout, int maxUnits, List<string> names) {
      Log.M?.TWL(0, "CustomLanceHelper.PushDropLayout id:" + id + " maxUnits:" + maxUnits + " layout:" + layout.Count + " names:" + (names == null ? "null" : "not null"));
      for (int t = 0; t < layout.Count; ++t) {
        foreach (string dropdef in layout[t]) {
          Log.M?.WL(1, "[" + t + "]:" + dropdef);
        }
      }
      if (names == null) { names = new List<string>(); }
      if (layout.Count == 0) { return; }
      DropSlotsDef newlayout = new DropSlotsDef();
      newlayout.Description = new DropDescriptionDef();
      newlayout.Description.Id = id;
      newlayout.Description.Name = id;
      newlayout.Description.Details = id;
      newlayout.OverallUnits = maxUnits;
      for (int t = 0; t < layout.Count; ++t) {
        List<string> lance = layout[t];
        DropLanceDef newLance = new DropLanceDef();
        newLance.Description = new DropDescriptionDef();
        newLance.Description.Id = newlayout.Description.Id + "_lance_" + t;
        newLance.Description.Name = t < names.Count ? names[t] : "LANCE " + t;
        Log.M?.WL(1, "newLance[" + t + "].Name=" + newLance.description.Name);
        newLance.dropSlots = new List<DropSlotDef>();
        newLance.DropSlots = new List<string>();
        foreach (string slotid in lance) {
          DropSlotDef slot = DropSystemHelper.getSlot(slotid);
          newLance.dropSlots.Add(slot);
          newLance.DropSlots.Add(slotid);
        }
        for (int tt = newLance.dropSlots.Count; tt < 4; ++tt) {
          newLance.dropSlots.Add(DropSystemHelper.fallbackDisabledSlot);
        }
        newLance.Register();
        newlayout.dropLances.Add(newLance);
        newlayout.DropLances.Add(newLance.Description.Id);
      }
      if (Core.Settings.forcedLance.Count != 0) {
        List<string> lance = Core.Settings.forcedLance;
        DropLanceDef newLance = new DropLanceDef();
        newLance.Description = new DropDescriptionDef();
        newLance.Description.Id = newlayout.Description.Id + "_lance_" + layout.Count;
        newLance.Description.Name = "LANCE " + layout.Count;
        newLance.dropSlots = new List<DropSlotDef>();
        newLance.DropSlots = new List<string>();
        foreach (string slotid in lance) {
          DropSlotDef slot = DropSystemHelper.getSlot(slotid);
          newLance.dropSlots.Add(slot);
          newLance.DropSlots.Add(slotid);
        }
        for (int tt = newLance.dropSlots.Count; tt < 4; ++tt) {
          newLance.dropSlots.Add(DropSystemHelper.fallbackDisabledSlot);
        }
        newLance.Register();
        newlayout.dropLances.Add(newLance);
        newlayout.DropLances.Add(newLance.Description.Id);
      }
      newlayout.Register();
      if (UnityGameInstance.BattleTechGame.Simulation != null) {
        UnityGameInstance.BattleTechGame.Simulation.CompanyStats.GetOrCreateStatisic<string>(DropSystemHelper.CURRENT_DROP_LAYOUT_STAT_NAME, "fallback_layout").SetValue<string>(newlayout.Description.Id);
      }
    }
    public static bool MissionControlDetected { get; set; } = false;
    public static void setLancesCount(int size) { return; }
    public static void setLanceData(int lanceid, int size, int allow, bool is_vehicle) { return; }
    public static void setOverallDeployCount(int value) { return; }
    public static void playerControl(int mechs, int vehicles) { return;/*f_playerControlMechs = mechs; f_playerControlVehicles = vehicles;*/}
    public static Transform FindRecursive(this Transform transform, string checkName) {
      foreach (Transform t in transform) {
        if (t.name == checkName) return t;
        Transform possibleTransform = FindRecursive(t, checkName);
        if (possibleTransform != null) return possibleTransform;
      }
      return null;
    }
    public static T FindComponent<T>(this GameObject go, string checkName) where T : Component {
      T[] components = go.GetComponentsInChildren<T>(true);
      foreach (T t in components) {
        if (t.transform.name == checkName) return t;
      }
      return null;
    }
    public static Transform FindTopLevelChild(this Transform transform, string checkName) {
      Transform[] trs = transform.GetComponentsInChildren<Transform>(true);
      foreach (Transform tr in trs) {
        if (tr.parent != transform) { continue; }
        if (tr.name != checkName) { continue; }
        return tr;
      }
      return null;
    }
    public static string GetRelativePath(this Transform transform, Transform parent) {
      string result = string.Empty;
      Transform temp = transform;
      while ((temp != parent) && (temp != null)) {
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
    private static int startUnitPosition = 0;
    public static int CurViewStartPosition(this AAR_UnitsResult_Screen screen) { return startUnitPosition; }
    public static void CurViewStartPosition(this AAR_UnitsResult_Screen screen, int value) { startUnitPosition = value; }
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Method(typeof(AAR_UnitsResult_Screen), "InitializeWidgets");
      var replacementMethod = AccessTools.Method(typeof(AAR_UnitsResult_Screen_InitializeSkirmishData), nameof(InitializeWidgets));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    static void InitializeWidgets(this AAR_UnitsResult_Screen rscreen) {
      Log.M?.TWL(0, "AAR_UnitsResult_Screen.InitializeWidgets", true);
      return;
    }
    static void Prefix(AAR_UnitsResult_Screen __instance) {
      Log.M?.TWL(0, "AAR_UnitsResult_Screen.InitializeData", true);
      try {
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
        int unitWidgetsCount = Mathf.CeilToInt((float)UnityGameInstance.BattleTechGame.Simulation.currentLayout().slotsCount / 4.0f) * 4;
        AAR_UnitStatusWidget src = __instance.UnitWidgets[0];
        Log.M?.WL(0, "unitWidgetsCount:" + unitWidgetsCount);
        for (int index = __instance.UnitWidgets.Count; index < unitWidgetsCount; ++index) {
          AAR_UnitStatusWidget dest = GameObject.Instantiate(src).GetComponent<AAR_UnitStatusWidget>();
          dest.transform.SetParent(src.transform.parent);
          dest.transform.localScale = src.transform.localScale;
          __instance.UnitWidgets.Add(dest);
        }
        for (int index = 0; index < __instance.UnitWidgets.Count; ++index) {
          __instance.UnitWidgets[index].gameObject.SetActive(index < 4);
        }
        startUnitPosition = 0;
      }catch(Exception e) {
        Log.E?.TWL(0,e.ToString(), true);
        UIManager.logger.LogException(e);
      }
    }
    static void Postfix(AAR_UnitsResult_Screen __instance) {
      try {
        __instance.UnitResults.Clear();
        for (int index = 0; index < __instance.UnitWidgets.Count; ++index) {
          if (index < __instance.numUnits) {
            __instance.UnitResults.Add(__instance.theContract.PlayerUnitResults[index]);
          } else {
            __instance.UnitResults.Add((UnitResult)null);
          }
        }
        for (int index = 0; index < __instance.UnitWidgets.Count; ++index) {
          if (index < __instance.numUnits && __instance.UnitResults[index] != null) {
            __instance.UnitWidgets[index].InitData(__instance.UnitResults[index], __instance.simState, __instance.simState.DataManager, __instance.theContract);
          } else {
            __instance.UnitWidgets[index].InitData((UnitResult)null, __instance.simState, __instance.simState.DataManager, __instance.theContract);
          }
        }
      }catch(Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
        UIManager.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(AAR_SkirmishResult_Screen), "InitializeSkirmishData")]
  public static class AAR_SkirmishResult_Screen_InitializeSkirmishData {
    private static int startUnitPosition = 0;
    public static int CurViewStartPosition(this AAR_SkirmishResult_Screen screen) { return startUnitPosition; }
    public static void CurViewStartPosition(this AAR_SkirmishResult_Screen screen, int value) { startUnitPosition = value; }
    static void Prefix(AAR_SkirmishResult_Screen __instance) {
      Log.M?.TWL(0, "AAR_SkirmishResult_Screen.InitializeSkirmishData", true);
      try {
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
        int unitWidgetsCount = Mathf.CeilToInt((float)UnityGameInstance.BattleTechGame.Simulation.currentLayout().slotsCount / 4.0f) * 4;
        AAR_UnitStatusWidget src = __instance.UnitWidgets[0];
        Log.M?.WL(0, "unitWidgetsCount:" + unitWidgetsCount);
        for (int index = __instance.UnitWidgets.Count; index < unitWidgetsCount; ++index) {
          AAR_UnitStatusWidget dest = GameObject.Instantiate(src).GetComponent<AAR_UnitStatusWidget>();
          dest.transform.SetParent(src.transform.parent);
          dest.transform.localScale = src.transform.localScale;
          __instance.UnitWidgets.Add(dest);
        }
        for (int index = 0; index < __instance.UnitWidgets.Count; ++index) {
          __instance.UnitWidgets[index].gameObject.SetActive(index < 4);
        }
        startUnitPosition = 0;
      }catch(Exception e) {
        Log.M?.TWL(0,e.ToString(),true);
        UIManager.logger.LogException(e);
      }
    }
    static void Postfix(AAR_SkirmishResult_Screen __instance) {
      try {
        __instance.PlayerUnitResults.Clear();
        for (int index = 0; index < __instance.UnitWidgets.Count; ++index) {
          if (index < __instance.numUnits) {
            __instance.PlayerUnitResults.Add(__instance.theContract.PlayerUnitResults[index]);
          } else {
            __instance.PlayerUnitResults.Add((UnitResult)null);
          }
        }
        __instance.OpponentUnitResults.Clear();
        for (int index = 0; index < __instance.UnitWidgets.Count; ++index) {
          if (index < __instance.numOpponentUnits) {
            __instance.OpponentUnitResults.Add(__instance.theContract.OpponentUnitResults[index]);
          } else {
            __instance.OpponentUnitResults.Add((UnitResult)null);
          }
        }
        for (int index = 0; index < __instance.UnitWidgets.Count; ++index) {
          if (index < __instance.numUnits && __instance.PlayerUnitResults[index] != null) {
            __instance.UnitWidgets[index].InitData(__instance.PlayerUnitResults[index], __instance.simState, __instance.dm, __instance.theContract);
          } else {
            __instance.UnitWidgets[index].InitData((UnitResult)null, __instance.simState, __instance.dm, __instance.theContract);
          }
        }
      }catch(Exception e) {
        Log.M?.TWL(0,e.ToString(), true);
        UIManager.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(AAR_UnitsResult_Screen), "FillInData")]
  public static class AAR_UnitsResult_Screen_InitializeWidgets {
    static void Prefix(ref bool __runOriginal,AAR_UnitsResult_Screen __instance) {
      Log.M?.TWL(0, "AAR_UnitsResult_Screen.FillInData");
      if (!__runOriginal) { return; }
      try {
        int experienceEarned = __instance.theContract.ExperienceEarned;
        for (int index = 0; index < __instance.UnitWidgets.Count; ++index) {
          __instance.UnitWidgets[index].SetMechIconValueTextActive(false);
          if (__instance.UnitResults[index] != null) {
            __instance.UnitWidgets[index].SetNoUnitDeployedOverlayActive(false);
            //if (___UnitResults[index].pilot.pilotDef.isVehicleCrew() == false) {
            //if (___UnitResults[index].mech.IsChassisFake() == false) {
            __instance.UnitWidgets[index].FillInData(experienceEarned);
            //} else {
            //___UnitWidgets[index].FillInData(0);
            //}
          } else {
            __instance.UnitWidgets[index].SetNoUnitDeployedOverlayActive(true);
          }
        }
      }catch(Exception e) {
        Log.E?.TWL(0, e.ToString(), true);
        UIManager.logger.LogException(e);
      }
      __runOriginal = false;
      return;
    }
  }
  [HarmonyPatch(typeof(AAR_SkirmishResult_Screen), "ShowMyResults")]
  public static class AAR_SkirmishResult_Screen_ShowMyResults {
    static void Prefix(ref bool __runOriginal, AAR_SkirmishResult_Screen __instance) {
      Log.M?.TWL(0, "AAR_SkirmishResult_Screen.ShowMyResults");
      try {
        for (int index = 0; index < __instance.UnitWidgets.Count; ++index) {
          __instance.UnitWidgets[index].SetMechIconValueTextActive(false);
          if (__instance.PlayerUnitResults[index] != null) {
            __instance.UnitWidgets[index].SetNoUnitDeployedOverlayActive(false);
            __instance.UnitWidgets[index].SetUnitData(__instance.PlayerUnitResults[index]);
            __instance.UnitWidgets[index].FillInSkirmishData();
          } else {
            __instance.UnitWidgets[index].SetNoUnitDeployedOverlayActive(true);
          }
        }
      }catch(Exception e) {
        Log.M?.TWL(0, e.ToString(), true);

      }
      __runOriginal = false; return;
    }
  }
  [HarmonyPatch(typeof(LanceConfiguratorPanel), "ValidateLanceTonnage")]
  public static class LanceConfiguratorPanel_ValidateLanceTonnage {
    public static void Prefix(ref bool __runOriginal,LanceConfiguratorPanel __instance, ref bool __result) {
      Log.M?.TWL(0, "LanceConfiguratorPanel.ValidateLanceTonnage maxTonnage:" + __instance.lanceMaxTonnage);
      try {
        if (!__runOriginal) { return; }
        List<MechDef> mechs = new List<MechDef>();
        bool flag1 = true;
        for (int index = 0; index < __instance.maxUnits; ++index) {
          LanceLoadoutSlot loadoutSlot = __instance.loadoutSlots[index];
          if (loadoutSlot.SelectedMech != null) {
            if (!MechValidationRules.MechTonnageWithinRange(loadoutSlot.SelectedMech.MechDef, __instance.slotMinTonnages[index], __instance.slotMaxTonnages[index])) {
              flag1 = false;
              if (__instance.slotMinTonnages[index] >= 0f && __instance.slotMaxTonnages[index] >= 0f)
                __instance.lanceErrorText.Append("Lance slot {0} requires a 'Unit between {1} and {2} Tons\n", (object)index, (object)__instance.slotMinTonnages[index], (object)__instance.slotMaxTonnages[index]);
              else if ((double)__instance.slotMinTonnages[index] >= 0.0)
                __instance.lanceErrorText.Append("Lance slot {0} requires a 'Unit over {1} Tons\n", (object)index, (object)__instance.slotMinTonnages[index]);
              else if ((double)__instance.slotMaxTonnages[index] >= 0.0)
                __instance.lanceErrorText.Append("Lance slot {0} requires a 'Unit under {1} Tons\n", (object)index, (object)__instance.slotMaxTonnages[index]);
            }
            DropSlotDef def = DropSystemHelper.currentLayout(UnityGameInstance.BattleTechGame.Simulation).GetSlotByIndex(index);
            if (def != null) { if (def.UseMaxUnits == false) { continue; } }
            mechs.Add(loadoutSlot.SelectedMech.MechDef);
          } else if (__instance.slotMinTonnages[index] >= 0.0) {
            flag1 = false;
            if ((double)__instance.slotMinTonnages[index] >= 0.0 && (double)__instance.slotMaxTonnages[index] >= 0.0)
              __instance.lanceErrorText.Append("Lance slot {0} requires a 'Unit between {1} and {2} Tons\n", (object)index, (object)__instance.slotMinTonnages[index], (object)__instance.slotMaxTonnages[index]);
            else if ((double)__instance.slotMinTonnages[index] >= 0.0)
              __instance.lanceErrorText.Append("Lance slot {0} requires a 'Unit over {1} Tons\n", (object)index, (object)__instance.slotMinTonnages[index]);
          }
        }
        bool flag2 = MechValidationRules.LanceTonnageWithinRange(mechs, __instance.lanceMinTonnage, __instance.lanceMaxTonnage);
        if (!flag2) {
          if ((double)__instance.lanceMinTonnage >= 0.0 && (double)__instance.lanceMaxTonnage >= 0.0)
            __instance.lanceErrorText.Append("Total Lance tonnage must be between {0} and {1} Tons\n", (object)__instance.lanceMinTonnage, (object)__instance.lanceMaxTonnage);
          else if ((double)__instance.lanceMinTonnage >= 0.0)
            __instance.lanceErrorText.Append("Total Lance tonnage must be greater than {0} Tons\n", (object)__instance.lanceMinTonnage);
          else if ((double)__instance.lanceMaxTonnage >= 0.0)
            __instance.lanceErrorText.Append("Total Lance tonnage must be less than {0} Tons\n", (object)__instance.lanceMaxTonnage);
        }
        __result = flag1 & flag2;
        __runOriginal = false; return;
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
        UIManager.logger.LogException(e);
        return;
      }
    }
  }
  [HarmonyPatch(typeof(LanceConfiguratorPanel), "ValidateLance")]
  public static class LanceConfiguratorPanel_ValidateLance {
    public static void RefreshLanceInfo(this LanceHeaderWidget header, bool lanceValid, Localize.Text errorText, List<MechDef> mechs, int currentUnits, int maxUnits) {
      Log.M?.TWL(0, "RefreshLanceInfo:" + maxUnits);
      header.RefreshLanceInfo(lanceValid, errorText, mechs);
      if (lanceValid) {
        header.simReadyLanceText.SetText("Lance Ready {0} units of {1}", currentUnits, maxUnits);
      }
    }
    public static void Prefix(ref bool __runOriginal, LanceConfiguratorPanel __instance, ref bool __result) {
      Log.M?.TWL(0, "LanceConfiguratorPanel.ValidateLance");
      if (!__runOriginal) { return; }
      try {
        __instance.lanceValid = false;
        __instance.lanceErrorText = new Localize.Text();
        int filledSlots = 0;
        int partialSlots = 0;
        int emptySlots = 0;
        int countedSlots = 0;
        __instance.currentLanceValue = 0;
        List<MechDef> mechs = new List<MechDef>();
        //List<UnitResult> unitResultList = new List<UnitResult>();
        bool flag = __instance.ValidateLanceTonnage(); //__instance.ValidateLanceTonnage();
        int overallSlotsCount = UnityGameInstance.BattleTechGame.Simulation.currentLayout().OverallUnits;
        Log.M?.WL(1, "overallSlotsCount:" + overallSlotsCount);
        if (__instance.activeContract != null) {
          if (__instance.activeContract.IsFlashpointCampaignContract || __instance.activeContract.IsFlashpointContract) {
            overallSlotsCount = Mathf.Min(overallSlotsCount, __instance.activeContract.Override.maxNumberOfPlayerUnits);
          } else {
            if ((__instance.activeContract.Override.maxNumberOfPlayerUnits != 4) || (Core.Settings.CountContractMaxUnits4AsUnlimited == false)) {
              overallSlotsCount = Mathf.Min(overallSlotsCount, __instance.activeContract.Override.maxNumberOfPlayerUnits);
            }
          }
        }
        if (CustomLanceHelper.MissionControlDetected == false) { if (overallSlotsCount > 4) { overallSlotsCount = 4; } }
        overallSlotsCount = Mathf.Min(overallSlotsCount, __instance.maxUnits);
        Log.M?.WL(1, "overallSlotsCount:" + overallSlotsCount);
        //overallSlotsCount = 4;
        for (int index = 0; index < __instance.maxUnits; ++index) {
          LanceLoadoutSlot loadoutSlot = __instance.loadoutSlots[index];
          DropSlotDef def = DropSystemHelper.currentLayout(UnityGameInstance.BattleTechGame.Simulation).GetSlotByIndex(index);
          bool skip = false;
          if (CustomLanceHelper.MissionControlDetected) {
            if (def.UseMaxUnits == false) { skip = true; }
          } else {
            if ((def.UseMaxUnits == false) && (def.HotDrop == true)) { skip = true; }
          }
          if (loadoutSlot.SelectedMech != null) {
            if (skip == false) {
              __instance.currentLanceValue += loadoutSlot.SelectedMech.MechDef.Description.Cost;
              mechs.Add(loadoutSlot.SelectedMech.MechDef);
            }
          }
          if (loadoutSlot.SelectedMech != null && loadoutSlot.SelectedPilot != null) {
            ++filledSlots;
            countedSlots += (skip ? 0 : 1);
          } else if ((loadoutSlot.SelectedMech != null && loadoutSlot.SelectedPilot == null) || (loadoutSlot.SelectedMech == null && loadoutSlot.SelectedPilot != null)) {
            ++partialSlots;
          } else {
            ++emptySlots;
          }
        }
        if (__instance.maxLanceValue < 0) {
          __instance.lanceValid = true;
        } else {
          __instance.lanceValid = __instance.currentLanceValue <= __instance.maxLanceValue;
          if (!__instance.lanceValid)
            __instance.lanceErrorText.Append("Lance budget exceeds limit\n", (object[])Array.Empty<object>());
        }
        if (filledSlots < 1 || partialSlots > 0) {
          __instance.lanceValid = false;
          if (filledSlots < 1)
            __instance.lanceErrorText.Append("Lance must not be empty\n", (object[])Array.Empty<object>());
          else
            __instance.lanceErrorText.Append("Lance slots require both a 'unit and pilot\n", (object[])Array.Empty<object>());
        }
        if (!__instance.allowUnevenLances && countedSlots < overallSlotsCount) {
          __instance.lanceValid = false;
          __instance.lanceErrorText.Append("Lance must have at least {0} units in initial drop\n", overallSlotsCount);
        }
        if (countedSlots > overallSlotsCount) {
          __instance.lanceValid = false;
          __instance.lanceErrorText.Append("You are not allowed to drop {0} units, only {1}\n", countedSlots, overallSlotsCount);
        }
        Log.M?.WL(1, "overallSlotsCount:" + overallSlotsCount);
        __instance.headerWidget.RefreshLanceInfo(__instance.lanceValid & flag, __instance.lanceErrorText, mechs, countedSlots, overallSlotsCount);
        __instance.RefreshLanceInitiative();
        __result = __instance.lanceValid & flag;
        __runOriginal = false;
        return;
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
        UIManager.logger.LogException(e);
        return;
      }
    }
  }
  [HarmonyPatch(typeof(LanceConfiguratorPanel), "ContinueConfirmAudioCallback")]
  public static class LanceConfiguratorPanel_ContinueConfirmAudioCallback {
    public static void Prefix(ref bool __runOriginal, LanceConfiguratorPanel __instance) {
      Log.M?.TWL(0, $"LanceConfiguratorPanel.ContinueConfirmAudioCallback {(__instance.confirmCB==null?"null":__instance.confirmCB.ToString())} original:{__runOriginal}");
    }
  }

  [HarmonyPatch(typeof(LanceConfiguratorPanel), "OnConfirmClicked")]
  public static class LanceConfiguratorPanel_OnConfirmClicked {
    static void Prefix(ref bool __runOriginal, LanceConfiguratorPanel __instance) {
      Log.M?.TWL(0, $"LanceConfiguratorPanel.OnConfirmClicked original:{__runOriginal}");
      try {
        if ((__instance.sim != null) && (__instance.activeContract != null)) {
          bool badBiome = false;
          string forbidTag = "NoBiome_" + __instance.activeContract.ContractBiome;
          Log.M?.WL(1, "SimGameState exists and activeContract too. Biome tag:" + forbidTag);
          foreach (LanceLoadoutSlot slot in __instance.loadoutSlots) {
            if (slot.SelectedMech != null) {
              foreach (MechComponentRef component in slot.SelectedMech.MechDef.Inventory) {
                if (component.Def.ComponentTags.Contains(forbidTag)) { badBiome = true; break; }
              }
            };
          }
          if (badBiome) {
            Localize.Text text = new Localize.Text("__/DROP.BAD_BIOME.TITLE/__");
            Localize.Text message = new Localize.Text("__/DROP.BAD_BIOME.MESSAGE/__");
            __instance.sim.interruptQueue.QueuePauseNotification(text.ToString(true), message.ToString(true), __instance.sim.GetCrewPortrait(SimGameCrew.Crew_Yang), "notification_mechreadycomplete", (Action)(() => {
            }), "Continue", (Action)null, (string)null);
            __runOriginal = false; return;
          }
        }
        int overallSlotsCount = UnityGameInstance.BattleTechGame.Simulation.currentLayout().OverallUnits;
        if (__instance.activeContract != null) {
          if (__instance.activeContract.IsFlashpointCampaignContract || __instance.activeContract.IsFlashpointContract) {
            overallSlotsCount = __instance.activeContract.Override.maxNumberOfPlayerUnits;
          } else {
            if ((__instance.activeContract.Override.maxNumberOfPlayerUnits != 4) || (Core.Settings.CountContractMaxUnits4AsUnlimited == false)) {
              overallSlotsCount = __instance.activeContract.Override.maxNumberOfPlayerUnits;
            }
          }
        }
        if (CustomLanceHelper.MissionControlDetected == false) { if (overallSlotsCount > 4) { overallSlotsCount = 4; } }
        int deployCount = 0;
        for (int t = 0; t < __instance.loadoutSlots.Length; ++t) {
          if (__instance.loadoutSlots[t].SelectedMech == null) { continue; }
          DropSlotDef def = DropSystemHelper.currentLayout(UnityGameInstance.BattleTechGame.Simulation).GetSlotByIndex(t);
          if (CustomLanceHelper.MissionControlDetected) {
            if (def.UseMaxUnits == false) { continue; }
          } else {
            if ((def.UseMaxUnits == false) && (def.HotDrop == true)) { continue; }
          }
          ++deployCount;
        }
        if (overallSlotsCount < deployCount) {
          GenericPopupBuilder.Create("Lance Cannot Be Deployed", Strings.T("Deploying units count {0} exceed limit {1}", deployCount, overallSlotsCount)).AddFader(new UIColorRef?(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PopupBackfill), 0.0f, true).Render();
          __runOriginal = false;return;
        }
      }catch(Exception e) {
        Log.E?.TWL(0, e.ToString());
        UIManager.logger.LogException(e);
      }
      return;
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
      Log.M?.TWL(0, "SimGameState.ResolveCompleteContract.PilotDef.SetUnspentExperience " + pilotDef.Description.Id + " PlayerUnitResults:" + (PlayerUnitResults == null ? "null" : PlayerUnitResults.Count.ToString()) + " value:" + value);
      if (PlayerUnitResults == null) {
        pilotDef.SetUnspentExperience(value);
      } else {
        UnitResult pilotResult = null;
        foreach (UnitResult result in PlayerUnitResults) {
          if (result.pilot.Description.Id == pilotDef.Description.Id) {
            pilotResult = result; break;
          }
        }
        if (pilotResult == null) {
          Log.M?.WL(1, "cant find pilot result for:" + pilotDef.Description.Id);
          pilotDef.SetUnspentExperience(value);
        } else {
          Log.M?.WL(1, "success find result for:" + pilotDef.Description.Id + " unit:" + pilotResult.mech.ChassisID + " idFake:" + pilotResult.mech.IsVehicle());
          pilotDef.SetUnspentExperience(value);
        }
      }
    }
  }
  [HarmonyPatch(typeof(AAR_SkirmishResult_Screen), "ShowOpponentResults")]
  public static class AAR_SkirmishResult_Screen_ShowOpponentResults {
    static void Prefix(ref bool __runOriginal,AAR_SkirmishResult_Screen __instance) {
      Log.M?.TWL(0, "AAR_SkirmishResult_Screen.ShowOpponentResults");
      if (!__runOriginal) { return; }
      for (int index = 0; index < __instance.UnitWidgets.Count; ++index) {
        __instance.UnitWidgets[index].SetMechIconValueTextActive(false);
        if (__instance.OpponentUnitResults[index] != null) {
          __instance.UnitWidgets[index].SetNoUnitDeployedOverlayActive(false);
          __instance.UnitWidgets[index].SetUnitData(__instance.OpponentUnitResults[index]);
          __instance.UnitWidgets[index].FillInSkirmishData();
        } else {
          __instance.UnitWidgets[index].SetNoUnitDeployedOverlayActive(true);
        }
      }
      __runOriginal = false;
      return;
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
    private static HashSet<string> MechDefsGUIDsTracker { get; set; } = new HashSet<string>();
    public static void ClearMechDefGUIDsTracker(this LanceConfiguratorPanel lc) { MechDefsGUIDsTracker.Clear(); }
    public static bool AddMechDefGUIDsTracker(this LanceConfiguratorPanel lc, string GUID) { return MechDefsGUIDsTracker.Add(GUID); }
    public static void Prefix(ref bool __runOriginal, LanceConfiguratorPanel __instance, LanceConfiguration config) {
      if (!__runOriginal) { return; }
      Log.M?.TWL(0, "LanceConfiguratorPanel.LoadLanceConfiguration prefix:");
      __instance.ClearMechDefGUIDsTracker();
      try {
        for (int i = 0; i < __instance.loadoutSlots.Length; ++i) {
          Pilot pilot = null;
          MechDef mech = null;
          if (__instance.loadoutSlots[i].SelectedPilot != null) { pilot = __instance.loadoutSlots[i].SelectedPilot.Pilot; }
          if (__instance.loadoutSlots[i].SelectedMech != null) { mech = __instance.loadoutSlots[i].SelectedMech.MechDef; }
          Log.M?.WL(1, $"[{i}] state:{__instance.loadoutSlots[i].curLockState} pilot:{((pilot != null) ? (pilot.Description.Id + "(" + pilot.Callsign + ")") : "null")}  unit:{((mech != null) ? mech.ChassisID + "(" + mech.GUID + ")" : "null")}");
        }
        __instance.headerWidget.LanceName = Strings.T(__instance.oldLanceName);
        SpawnableUnit[] lanceUnits = config.GetLanceUnits(__instance.playerGUID);
        for (int i = 0; i < lanceUnits.Length; ++i) {
          Log.M?.WL(1, $"[{i}] pilot:{lanceUnits[i].PilotId} UnitId:{lanceUnits[i].UnitId} Unit:{(lanceUnits[i].Unit==null?"null": lanceUnits[i].Unit.GUID)}");
        }
        List<LoadoutContent> spawnMechList = new List<LoadoutContent>();
        List<LoadoutContent> spawnVehicleList = new List<LoadoutContent>();
        int count = Mathf.Min(lanceUnits.Length, __instance.loadoutSlots.Length);
        Log.M?.WL(1, "filling list");
        for (int i = 0; i < count; ++i) {
          try {
            SpawnableUnit unit = lanceUnits[i];
            IMechLabDraggableItem forcedMech = (IMechLabDraggableItem)null;
            if (unit.Unit != null) { forcedMech = __instance.mechListWidget.GetMechDefByGUID(unit.Unit.GUID); }
            if (forcedMech == null) { forcedMech = __instance.mechListWidget.GetInventoryItem(unit.UnitId); }
            if (forcedMech != null && !MechValidationRules.ValidateMechCanBeFielded(__instance.Sim, forcedMech.MechDef)) {
              forcedMech = (IMechLabDraggableItem)null;
            }
            Log.M?.WL(2, $"[{i}] pilot:{unit.PilotId} UnitId:{unit.UnitId} Unit:{(unit.Unit == null ? "null" : unit.Unit.GUID)}");
            if ((forcedMech != null) && (forcedMech.MechDef != null)) { Log.M?.WL(3, $"{forcedMech.MechDef.ChassisID}"); }
            SGBarracksRosterSlot forcedPilot = null;
            forcedPilot = __instance.pilotListWidget.GetPilot(unit.PilotId);
            if (forcedPilot != null) {
              if (forcedPilot.Pilot.CanPilot == false) {
                forcedPilot = (SGBarracksRosterSlot)null;
              }
            }
            LanceLoadoutSlot loadoutSlot = __instance.loadoutSlots[i];
            if (loadoutSlot.isMechLocked) { forcedMech = null; }
            if (loadoutSlot.isPilotLocked) { forcedPilot = null; }
            if (forcedPilot != null) { __instance.pilotListWidget.RemovePilot(__instance.availablePilots.Find((Predicate<Pilot>)(x => x.Description.Id == forcedPilot.Pilot.Description.Id))); }
            loadoutSlot.SetLockedData(forcedMech, forcedPilot, false);
          }catch(Exception e) {
            Log.M?.TWL(0,e.ToString(),true);
            UIManager.logger.LogException(e);
          }
        }
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString());
        UIManager.logger.LogException(e);
      }
       __runOriginal = false;
      return;
    }
    public static void Postfix(LanceConfiguratorPanel __instance, LanceConfiguration config) {
      Log.M?.TWL(0, "LanceConfiguratorPanel.LoadLanceConfiguration postfix:");
      try {
        for (int i = 0; i < __instance.loadoutSlots.Length; ++i) {
          Pilot pilot = null;
          MechDef mech = null;
          if (__instance.loadoutSlots[i].SelectedPilot != null) { pilot = __instance.loadoutSlots[i].SelectedPilot.Pilot; }
          if (__instance.loadoutSlots[i].SelectedMech != null) { mech = __instance.loadoutSlots[i].SelectedMech.MechDef; }
          Log.M?.WL(1, $"[{i}] state:{__instance.loadoutSlots[i].curLockState} pilot:{((pilot != null) ? (pilot.Description.Id + "(" + pilot.Callsign + ")") : "null")}  unit:{((mech != null) ? mech.ChassisID + "(" + mech.GUID + ")" : "null")}");
        }
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString());
        UIManager.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(LanceConfiguratorPanel), "CreateLanceDef")]
  public static class LanceConfiguratorPanel_CreateLanceDef {
    static void Prefix(ref bool __runOriginal, LanceConfiguratorPanel __instance, string lanceId, ref LanceDef __result) {
      Log.M?.TWL(0, "LanceConfiguratorPanel.CreateLanceDef " + lanceId + " slots:" + __instance.loadoutSlots.Length, true);
      if (!__runOriginal) { return; }
      try {
        string lanceName = __instance.headerWidget.LanceName;
        DescriptionDef description = new DescriptionDef(lanceId, lanceName, "", "", __instance.currentLanceValue, 0.0f, false, "", "", "");
        TagSet lanceTags = new TagSet(new string[4]
        {
        "lance_type_custom",
        "lance_release",
        "lance_bracket_skirmish",
        MechValidationRules.GetLanceBracketTag(__instance.currentLanceValue)
        });
        List<LanceDef.Unit> unitList = new List<LanceDef.Unit>();
        for (int index = 0; index < __instance.loadoutSlots.Length; ++index) {
          LanceLoadoutSlot loadoutSlot = __instance.loadoutSlots[index];
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
      }catch(Exception e) {
        Log.M?.TWL(0,e.ToString(),true);
        UIManager.logger.LogException(e);
      }
      __runOriginal = false;
      return;
    }
    static void Postfix(LanceConfiguratorPanel __instance, string lanceId, ref LanceDef __result) {
      LanceLoadoutSlot[] loadoutSlots = __instance.loadoutSlots;
      Log.M?.TWL(0, "LanceConfiguratorPanel.CreateLanceDef result:", true);
      Log.M?.WL(0, __result.ToJSON());
    }
  }
  [HarmonyPatch(typeof(LancePreviewPanel), "OnLanceConfiguratorConfirm")]
  public static class LancePreviewPanel_OnLanceConfiguratorConfirm {
    static void Prefix(LancePreviewPanel __instance) {
      Log.M?.TWL(0, "LancePreviewPanel.OnLanceConfiguratorConfirm", true);
    }
  }
  [HarmonyPatch(typeof(SkirmishUnitsAndLances), "AddOrUpdateLanceDef")]
  public static class SkirmishUnitsAndLances_AddOrUpdateLanceDef {
    static void Prefix(ref bool __runOriginal, SkirmishUnitsAndLances __instance, LanceDef lanceDef) {
      if (!__runOriginal) { return; }
      __runOriginal = false;
      Log.M?.TWL(0, "LancePreviewPanel.AddOrUpdateLanceDef");
      try {
        Log.M?.WL(1, $"lanceDef:{lanceDef.Description.Id}:{lanceDef.Description.Name}");
        SkirmishUnitsAndLances.VersionedLanceDef versionedLanceDef = __instance.FindVersionedLanceDef(lanceDef.Description.Id, __instance.currentHash);
        if (versionedLanceDef == null) {
          versionedLanceDef = new SkirmishUnitsAndLances.VersionedLanceDef(__instance.currentHash, lanceDef);
          __instance.customLances.Add(versionedLanceDef);
          __instance.MemoryStore.Add(VersionManifestEntry.CreateMemoryEntry(lanceDef.Description.Id, BattleTechResourceType.LanceDef.ToString(), DateTime.UtcNow));
        } else
          versionedLanceDef.LanceDef = lanceDef;
        versionedLanceDef.LanceDef.LanceTags.Add("lance_type_custom");
        Log.M?.WL(1, $"customLances:{__instance.customLances.Count}");
      } catch (Exception e) {
        Log.M?.TWL(0,e.ToString(),true);
      }
    }
  }
  [HarmonyPatch(typeof(LancePreviewPanel), "RefreshLanceList")]
  public static class LancePreviewPanel_RefreshLanceList {
    static void Prefix(ref bool __runOriginal, LancePreviewPanel __instance) {
      if (!__runOriginal) { return; }
      __runOriginal = false;
      Log.M?.TWL(0, "LancePreviewPanel.RefreshLanceList");
      try {
        __instance.customLances = ActiveOrDefaultSettings.CloudSettings.CustomUnitsAndLances.GetValidLances();
        __instance.allLanceDefs = new List<LanceDef>();
        for (int index = __instance.customLances.Count - 1; index >= 0; --index) {
          LanceDef customLance = __instance.customLances[index];
          Log.M?.WL(1, $"testing custom lance: {customLance.Description.Id}:{customLance.Description.Name}");
          if(customLance.Description.Cost > __instance.maxLanceValue) {
            __instance.customLances.RemoveAt(index);
            Log.M?.WL(2, $"cost override {customLance.Description.Cost} > {__instance.maxLanceValue}");
            continue;
          }
          if(MechValidationRules.LanceIsValidForSkirmish(customLance, !__instance.allowUnevenLances, true) == false) {
            __instance.customLances.RemoveAt(index);
            Log.M?.WL(2, $"lance is not valid for skirmish");
            continue;
          }
          if (__instance.stockMechsOnly && !MechValidationRules.LanceHasStockMechs(customLance)) {
            Log.M?.WL(2, $"lance has stock mech which is forbbiden");
            __instance.customLances.RemoveAt(index);
            continue;
          }
        }
        __instance.allLanceDefs.AddRange((IEnumerable<LanceDef>)__instance.customLances);
        __instance.allLanceDefs.AddRange((IEnumerable<LanceDef>)__instance.stockLances);
        if (__instance.lastUsedLanceDef != null && MechValidationRules.LanceIsValidForSkirmish(__instance.lastUsedLanceDef, !__instance.allowUnevenLances, true) && (__instance.stockMechsOnly && MechValidationRules.LanceHasStockMechs(__instance.lastUsedLanceDef) || !__instance.stockMechsOnly))
          __instance.allLanceDefs.Insert(0, __instance.lastUsedLanceDef);
        __instance.allLanceDefs.Insert(0, LancePreviewPanel.RANDOM_LANCE);
        __instance.PopulateDropdown();
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(LancePreviewPanel), "SaveLance")]
  public static class LancePreviewPanel_SaveLance {
    static void Finalizer(LancePreviewPanel __instance, Exception __exception) {
      Log.M?.TWL(0, "LancePreviewPanel.SaveLance Finalizer");
      if(__exception != null) {
        Log.M.WL(0, __exception.ToString(), true);
      }
      Log.M?.WL(1, $"customLances:{__instance.customLances.Count}");
      foreach(var lance in __instance.customLances) {
        Log.M?.WL(2, $"{lance.Description.Id}:{lance.Description.Name}");
      }
    }
  }
  [HarmonyPatch(typeof(LancePreviewPanel), "OnLanceConfiguratorCancel")]
  public static class LancePreviewPanel_OnLanceConfiguratorCancel {
    static void Prefix(LancePreviewPanel __instance) {
      Log.M?.TWL(0, "LancePreviewPanel.OnLanceConfiguratorCancel", true);
    }
  }
  [HarmonyPatch(typeof(LancePreviewPanel), "SetData")]
  public static class LancePreviewPanel_SetData {
    static void Prefix(LancePreviewPanel __instance, ref int maxUnits) {
      try {
        maxUnits = UnityGameInstance.BattleTechGame.Simulation.currentLayout().slotsCount; //CustomLanceHelper.fullSlotsCount();
        GameObject srcSlot = __instance.loadoutSlots[0].gameObject;
        List<LanceLoadoutSlot> slots = new List<LanceLoadoutSlot>();
        slots.AddRange(__instance.loadoutSlots);
        for (int t = __instance.loadoutSlots.Length; t < maxUnits; ++t) {
          GameObject newSlot = GameObject.Instantiate(srcSlot, srcSlot.transform.parent);
          newSlot.name = $"lanceSlot1 ({t})";
          slots.Add(newSlot.GetComponent<LanceLoadoutSlot>());
        }
        __instance.loadoutSlots = slots.ToArray();
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString());
        UIManager.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(ApplicationConstants))]
  [HarmonyPatch("FromJSON")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class ApplicationConstants_FromJSON {
    public static void Postfix(ApplicationConstants __instance) {
      Log.M?.TWL(0, "ApplicationConstants.FromJSON. PrewarmRequests:");
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
        prewarmRequests.Add(new PrewarmRequest(BattleTechResourceType.SVGAsset, Core.Settings.PilotingIcon)); CustomSvgCache.RegisterSVG(Core.Settings.PilotingIcon);
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
        __instance.PrewarmRequests = prewarmRequests.ToArray();
        //typeof(ApplicationConstants).GetProperty("PrewarmRequests", BindingFlags.Instance | BindingFlags.Public).GetSetMethod(true).Invoke(__instance, new object[] { prewarmRequests.ToArray() });
        foreach (PrewarmRequest preq in __instance.PrewarmRequests) {
          Log.M?.WL(1, preq.ResourceType + ":" + preq.ResourceID);
        }
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
        UnityGameInstance.logger.LogException(e);
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
      Log.Combat?.WL(0,"CombatHUDActorInfoHover.OnPointerClick called." + data.position);
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
      Transform MechTray_MechNameBackground = HUD.MechTray.gameObject.transform.Find("MechTray_MechNameBackground");
      if (MechTray_MechNameBackground == null) { Log.Combat?.TWL(0, "Exception: can't find MechTray_MechNameBackground", true); return; }
      image = MechTray_MechNameBackground.GetComponent<SVGImage>();
    }
  }
  public class LanceSwitcher : EventTrigger {
    public static List<SVGAsset> lanceIcons = new List<SVGAsset>();
    public CombatHUD HUD;
    public SVGImage image;
    private bool Hovered = false;
    private bool prevState = false;
    public override void OnPointerEnter(PointerEventData data) {
      Log.M?.WL(0,"LanceSwitcher.OnPointerEnter called." + data.position);
      Hovered = true;
      if (image != null) { image.color = Color.white; }
    }
    public override void OnPointerExit(PointerEventData data) {
      //SidePanel.ForceHide();
      Hovered = false;
      Log.M?.WL(0, "LanceSwitcher.OnPointerExit called." + data.position);
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
      Log.M?.WL(0, "LanceSwitcher.OnPointerClick called." + data.position);
    }
    public void Init(CombatHUD HUD, SVGImage image) {
      this.HUD = HUD;
      this.image = image;
      //lanceIcons = new List<SVGAsset>();
      Log.M?.TWL(0, "LanceSwitcher.Init");
      for (int index = lanceIcons.Count; index < Core.Settings.LancesIcons.Count; ++index) {
        string iconid = Core.Settings.LancesIcons[index];
        SVGAsset icon = CustomSvgCache.get(iconid, HUD.Combat.DataManager); //HUD.Combat.DataManager.GetObjectOfType<SVGAsset>(iconid, BattleTechResourceType.SVGAsset);
        if (icon != null) {
          Log.M?.WL(1, "icon " + iconid + " found");
          lanceIcons.Add(icon);
        } else {
          Log.M?.WL(1, "icon " + iconid + " not found");
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
      this.moraleBar = MechWarriorTray.moraleDisplay;
    }
  }
  [HarmonyPatch(typeof(HUDMechArmorReadout))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  public static class HUDMechArmorReadout_Init {
    public static void Postfix(HUDMechArmorReadout __instance) {
      Log.M?.TWL(0, "HUDMechArmorReadout.Init gameObject:" + __instance.gameObject.name + " parent:" + __instance.gameObject.transform.parent.gameObject.name);
    }
  }
  [HarmonyPatch(typeof(CombatHUD))]
  [HarmonyPatch("updateHUDElements")]
  [HarmonyPatch(MethodType.Normal)]
  public static class CombatHUD_updateHUDElements {
    public static void Postfix(CombatHUD __instance, AbstractActor actor) {
      if (actor == null) { return; }
      int lanceId = actor.LanceId();
      Log.M?.TWL(0, "CombatHUD.updateHUDElements " + actor.DisplayName + " lanceId:" + lanceId);
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
        Log.M?.TWL(0, "f_mwCommandButton init");
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
            Log.M?.WL(1, "cmd_NameRect can't find");
          }
          Transform cmd_uses_leftBG = uixPrfBttn_commandButton.transform.Find("cmd_uses_leftBG");
          if (cmd_uses_leftBG != null) {
            cmd_uses_leftBG.gameObject.SetActive(false);
          } else {
            Log.M?.WL(1, "cmd_uses_leftBG can't find");
          }
          Transform cmd_IconBG = uixPrfBttn_commandButton.transform.Find("cmd_IconBG");
          if (cmd_IconBG != null) {
            SVGImage svg = cmd_IconBG.GetComponent<SVGImage>();
            svg.vectorGraphics = LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.DoneWithMechButtonIcon;
            LanceSwitcher lanceSwitcher = f_mwCommandButton.GetComponent<LanceSwitcher>();
            if (lanceSwitcher == null) { lanceSwitcher = f_mwCommandButton.AddComponent<LanceSwitcher>(); }
            if (lanceSwitcher != null) { lanceSwitcher.Init(HUD, svg); };
          } else {
            Log.M?.WL(1, "cmd_IconBG can't find");
          }
        } else {
          Log.M?.WL(1, "uixPrfBttn_commandButton can't find");
        }
      }
      return f_mwCommandButton;
    }
    public static void Postfix(CombatHUDMechwarriorTray __instance) {
      GameObject btn = __instance.mwCommandButton(__instance.HUD);
      if (DeployManualHelper.IsInManualSpawnSequence) {
        btn.SetActive(false); return;
      }
      btn.SetActive(true);
    }
  }
  [HarmonyPatch(typeof(CombatHUDMechwarriorTray))]
  [HarmonyPatch("OnActorDeselected")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class CombatHUDMechwarriorTray_OnActorDeselected {
    public static void Postfix(CombatHUDMechwarriorTray __instance) {
      //__instance.mwCommandButton(___HUD).SetActive(false);
    }
  }
  [HarmonyPatch(typeof(CombatHUDMechwarriorTray))]
  [HarmonyPatch("OnCommandTrayInvoked")]
  [HarmonyPatch(MethodType.Normal)]
  public static class CombatHUDMechwarriorTray_OnCommandTrayInvoked {
    public static void Postfix(CombatHUDMechwarriorTray __instance) {
      GameObject btn = __instance.mwCommandButton(__instance.HUD);
      if (__instance.HUD.Combat.LocalPlayerTeam.unitCount == 1) {
        if (__instance.HUD.Combat.LocalPlayerTeam.units[0].IsDeployDirector()) {
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
    public static void Postfix(CombatHUDMechwarriorTray __instance) {
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
      Transform MechTray_MechNameBackground = __instance.gameObject.transform.Find("MechTray_MechNameBackground");
      if (MechTray_MechNameBackground != null) {
        CombatHUDActorInfoHover hover = MechTray_MechNameBackground.gameObject.GetComponent<CombatHUDActorInfoHover>();
        if (hover == null) { hover = MechTray_MechNameBackground.gameObject.AddComponent<CombatHUDActorInfoHover>(); }
        hover.Init(HUD);
      } else {
        Log.M?.TWL(0, "Exception: can't find MechTray_MechNameBackground", true);
      }
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
    public static void Prefix(ref bool __runOriginal, CombatHUDMechwarriorTray __instance, Team team) {
      __instance.displayedTeam = team;
      Log.Combat?.TWL(0, "CombatHUDMechwarriorTray.RefreshTeam");
      actorLanceIds.Clear();
      fHUD = __instance.HUD;
      try {
        Log.Combat?.WL(1, "Team:" + team.units.Count);
        foreach (AbstractActor unit in team.units) {
          Log.Combat?.WL(2, unit.DisplayName + " " + unit.PilotableActorDef.Description.Id + " GUID:" + unit.PilotableActorDef.GUID);
        }
        SimGameState sim = UnityGameInstance.BattleTechGame.Simulation;
        if (sim != null) {
          try {
            Log.Combat?.WL(1, "PlayerBayUnits:" + sim.ActiveMechs.Count);
            foreach (var unit in sim.ActiveMechs) {
              Log.Combat?.WL(2, unit.Key.ToString() + ":" + unit.Value.Description.Id + " GUID:" + unit.Value.GUID);
            }
          } catch (Exception e) {
            Log.Combat?.TWL(0, e.ToString(), true);
            UIManager.logger.LogException(e);
          }
        }
        Dictionary<int, CustomLanceInstance> avaibleSlots = new Dictionary<int, CustomLanceInstance>();
        foreach (var lance in __instance.exPortraitHolders()) {
          foreach (var slot in lance) {
            avaibleSlots.Add(slot.index, slot);
            slot.holder.SetActive(false);
          }
        }
        if (DeployManualHelper.IsInManualSpawnSequence) {
          avaibleSlots[0].portrait.DisplayedActor = DeployManualHelper.deployDirector;
          avaibleSlots[0].holder.SetActive(true);
          for (int index = 1; index < avaibleSlots.Count; ++index) {
            avaibleSlots[index].holder.SetActive(false);
          }
          //__instance.mwCommandButton(___HUD)?.SetActive(false);
          __instance.HUD.SelectionHandler.TrySelectActor(DeployManualHelper.deployDirector, true);
          __runOriginal = false; return;
        }
        Dictionary<int, AbstractActor> predefinedActors = new Dictionary<int, AbstractActor>();
        List<AbstractActor> units = new List<AbstractActor>();
        Log.Combat?.WL(1, "player lance layout");
        foreach (var plLayout in CustomLanceHelper.playerLanceLoadout.loadout) {
          Log.Combat?.WL(2, plLayout.Key + ":" + plLayout.Value);
        }
        Log.Combat?.WL(1, "avaible slots");
        foreach (var slot in avaibleSlots) {
          Log.Combat?.WL(2, slot.Key + " lance:" + slot.Value.lance_index + " index:" + slot.Value.lance_id + " head:" + slot.Value.isHead);
        }
        foreach (AbstractActor unit in __instance.displayedTeam.units) {
          string defGUID = string.Empty;
          if (unit.IsDeployDirector()) { continue; }
          defGUID = unit.PilotableActorDef.GUID + "_" + unit.PilotableActorDef.Description.Id + "_" + unit.GetPilot().Description.Id;
          Log.Combat?.WL(1, unit.DisplayName + " tags:" + unit.EncounterTags.ContentToString() + " defGUID:" + defGUID);
          if (string.IsNullOrEmpty(defGUID) == false) {
            if (CustomLanceHelper.playerLanceLoadout.loadout.TryGetValue(defGUID, out int slotIndex)) {
              if (avaibleSlots.TryGetValue(slotIndex, out CustomLanceInstance slot)) {
                slot.portrait.DisplayedActor = unit;
                slot.holder.SetActive(false);
                Log.Combat?.WL(1, new Localize.Text(unit.DisplayName).ToString() + " lance:" + slot.lance_id + " pos:" + slot.lance_index);
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
            Log.Combat?.WL(1, new Localize.Text(unit.DisplayName).ToString() + " escort unit detected. lance:" + slot.lance_id + " pos:" + slot.lance_index);
            avaibleSlots.Remove(slotIndex);
            continue;
          }
          units.Add(unit);
        }
        HashSet<int> restSlots = avaibleSlots.Keys.ToHashSet();
        foreach (int slotIndex in restSlots) {
          if (units.Count == 0) { break; }
          avaibleSlots[slotIndex].portrait.DisplayedActor = units[0];
          avaibleSlots[slotIndex].holder.SetActive(false);
          units.RemoveAt(0);
          avaibleSlots.Remove(slotIndex);
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
        //__instance.mwCommandButton(___HUD)?.SetActive(true);
        __instance.ShowLance(nonemptyLance);
        Log.Combat?.WL(1, "success");
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        UIManager.logger.LogException(e);
      }
      __runOriginal = false; return;
    }
  }
  public class CustomLanceInstance {
    public bool isHead { get; set; }
    public int index { get; set; }
    public int lance_id { get; set; }
    public int lance_index { get; set; }
    public bool service { get; set; }
    public GameObject holder { get; set; }
    public CombatHUDPortrait portrait { get; set; }
    public CustomLanceInstance(GameObject h, CombatHUDPortrait p, int index, bool head, int lanceid, int lance_pos, bool service) {
      holder = h; portrait = p; this.index = index; isHead = head;
      this.lance_id = lanceid;
      this.lance_index = lance_pos;
      this.service = service;
    }
  }
  [HarmonyPatch(typeof(CombatHUDButtonBase))]
  [HarmonyPatch("OnPointerEnter")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(PointerEventData) })]
  public static class CombatHUDButtonBase_OnPointerEnter {
    //private static bool toggleState = false;
    public static void Postfix(CombatHUDButtonBase __instance, PointerEventData eventData) {
      Log.Combat?.TWL(0, "CombatHUDButtonBase.OnPointerEnter " + __instance.GUID);
      CombatHUDMechwarriorTrayEx trayEx = __instance.HUD.MechWarriorTray.gameObject.GetComponent<CombatHUDMechwarriorTrayEx>();
      if (trayEx == null) {
        Log.Combat?.WL(1, "no trayEx");
        return;
      }
      CombatHUDActionButton instance = __instance as CombatHUDActionButton;
      if (instance == null) {
        Log.Combat?.WL(1, "not a CombatHUDActionButton");
        return;
      }
      if (trayEx.actionButtons.Contains(instance) == false) {
        Log.Combat?.WL(1, "not int actionButtons");
        return;
      };
      foreach (CombatHUDActionButton btn in trayEx.actionButtons) {
        Log.Combat?.WL(1, "exiting:" + btn.GUID);
        if (btn == __instance) { continue; }
        btn.Tooltip.color = Color.clear;
        btn.timeSinceColorsChanged = 0.14f;
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
      Log.Combat?.TWL(0, "CombatHUDButtonBase.OnPointerExit " + __instance.GUID);
    }
  }
  public partial class CombatHUDMechwarriorTrayEx : MonoBehaviour {
    public HashSet<CombatHUDActionButton> actionButtons;
    public List<CombatHUDActionButton> AbilitiesButtons;
    public List<CanvasGroup> AbilitiesCanvasGroup = new List<CanvasGroup>();
    //public CanvasGroup PassiveAbilitiesCanvasGroup;
    public List<CombatHUDActionButton> FirstActiveAbilities = new List<CombatHUDActionButton>();
    //public List<CombatHUDActionButton> PassiveAbilitiesButtons;
    public List<CombatHUDActionButton> NormalButtons;
    public CombatHUDActionButton ShowAbilitiesButton;
    //public CombatHUDActionButton ShowPassiveAbilitiesButton;
    //public CombatHUDActionButton HideAbilitiesButton;
    //public CombatHUDActionButton HidePassiveAbilitiesButton;
    public List<GameObject> AbilitiesButtonsLayouts = new List<GameObject>();
    //public GameObject PassiveButtonsLayout;
    public bool AttackModeSelectorPosInited;
    public Vector3 AttackModeSelectorUpPos;
    public Vector3 AttackModeSelectorDownPos;
    public int filledRows { get; set; } = 0;
    private bool ui_inited;
    public static readonly int HOTKEY_BUTTONS_COUNT = 10;
    public static readonly int BUTTONS_ROWS_COUNT = 10;
    public static float BUTTONS_SPACING = 10f;
    public static readonly string SHOW_ACTIVE_BUTTON_GUID = "BTN_ShowActiveButtons";
    public static readonly string HIDE_ACTIVE_BUTTON_GUID = "BTN_HideActiveButtons";
    public static readonly string SHOW_PASSIVE_BUTTON_GUID = "BTN_ShowPassiveButtons";
    public static readonly string HIDE_PASSIVE_BUTTON_GUID = "BTN_HidePassiveButtons";
    public static readonly string SHOW_ACTIVE_BUTTON_NAME = "ABIL.";
    public static readonly string SHOW_PASSIVE_BUTTON_NAME = "PAS.ABIL.";
    public CombatHUD HUD;
    public CombatGameState Combat;
    public CombatHUDMechwarriorTray mechWarriorTray;
    public void initAttackModeSelectorPos(int rows) {
      if (AttackModeSelectorPosInited) { return; }
      AttackModeSelectorPosInited = true;
      AttackModeSelectorUpPos = AttackModeSelectorDownPos;
      for (int t = 0; t < rows; ++t) {
        if (t >= AbilitiesButtonsLayouts.Count) { break; }
        RectTransform selfTransform = AbilitiesButtonsLayouts[t].GetComponent<RectTransform>();
        AttackModeSelectorUpPos.y += (selfTransform.sizeDelta.y + BUTTONS_SPACING);
      }
    }
    public void ShowActiveAbilities(int rows) {
      AttackModeSelectorPosInited = false;
      for (int t = 0; t < AbilitiesButtonsLayouts.Count; ++t) {
        AbilitiesButtonsLayouts[t].SetActive(t < rows);
      }
      for (int index = 0; index < HOTKEY_BUTTONS_COUNT; ++index) {
        this.NormalButtons[index].ButtonAction = null;
      }
      for (int index = 0; index < (HOTKEY_BUTTONS_COUNT - 1); ++index) {
        this.AbilitiesButtons[index].SetHotKey(Core.Settings.DisableHotKeys ? null : BTInput.Instance.Combat_Abilities()[index], true);
      }
      UpdateExtButtons();
      initAttackModeSelectorPos(rows);
    }
    public void UpdateExtButtons() {
      if (HUD.SelectedActor != null) {
        //HideAbilitiesButton.Init(Combat, HUD, Core.Settings.DisableHotKeys ? null : BTInput.Instance.Combat_DoneWithMech(), true);
        //HideAbilitiesButton.isClickable = true;
        //HideAbilitiesButton.RefreshColors(HUD.SelectedActor);
        //HidePassiveAbilitiesButton.Init(Combat, HUD, Core.Settings.DisableHotKeys ? null : BTInput.Instance.Combat_DoneWithMech(), true);
        //HideActiveAbilitiesButton.isClickable = true;
        //HideActiveAbilitiesButton.RefreshColors(HUD.SelectedActor);
      }
    }
    //public void ShowPassiveAbilities() {
    //  ActiveButtonsLayout.SetActive(false);
    //  PassiveButtonsLayout.SetActive(true);
    //  for (int index = 0; index < BUTTONS_COUNT; ++index) {
    //    this.NormalButtons[index].ButtonAction = null;
    //  }
    //  for (int index = 0; index < (BUTTONS_COUNT - 1); ++index) {
    //    this.PassiveAbilitiesButtons[index].SetHotKey(Core.Settings.DisableHotKeys ? null : BTInput.Instance.Combat_Abilities()[index], true);
    //  }
    //  UpdateExtButtons();
    //  initAttackModeSelectorPos();
    //}
    public void HideAbilities() {
      for (int t = 0; t < AbilitiesButtonsLayouts.Count; ++t) {
        AbilitiesButtonsLayouts[t].SetActive(false);
      }
      for (int index = 0; index < HOTKEY_BUTTONS_COUNT; ++index) {
        if (index == 9) {
          this.NormalButtons[index].SetHotKey(Core.Settings.DisableHotKeys ? null : BTInput.Instance.Combat_DoneWithMech(), true);
        } else {
          this.NormalButtons[index].SetHotKey(Core.Settings.DisableHotKeys ? null : BTInput.Instance.Combat_Abilities()[index], true);
        }
      }
    }
    //public void HidePassiveAbilities() {
    //  PassiveButtonsLayout.SetActive(false);
    //  for (int index = 0; index < BUTTONS_COUNT; ++index) {
    //    if (index == 9) {
    //      this.NormalButtons[index].SetHotKey(Core.Settings.DisableHotKeys ? null : BTInput.Instance.Combat_DoneWithMech(), true);
    //    } else {
    //      this.NormalButtons[index].SetHotKey(Core.Settings.DisableHotKeys ? null : BTInput.Instance.Combat_Abilities()[index], true);
    //    }
    //  }
    //}
    public CombatHUDMechwarriorTrayEx() {
      AbilitiesButtons = new List<CombatHUDActionButton>();
      //PassiveAbilitiesButtons = new List<CombatHUDActionButton>();
      NormalButtons = new List<CombatHUDActionButton>();
      ui_inited = false;
      AttackModeSelectorPosInited = false;
      actionButtons = new HashSet<CombatHUDActionButton>();
    }
    public void initUI() {
      RectTransform portraits = mechWarriorTray.gameObject.transform.FindRecursive("mwPortraitLayoutGroup") as RectTransform;
      //AttackModeSelectorDownPos = HUD.AttackModeSelector.transform.localPosition;
      //BUTTONS_SPACING = ;
      Vector3 locPos = Vector3.zero;
      locPos.y = portraits.localPosition.y + (portraits.sizeDelta.y / 2f);
      for (int t = 0; t < AbilitiesButtonsLayouts.Count; ++t) {
        RectTransform selfTransform = AbilitiesButtonsLayouts[t].GetComponent<RectTransform>();
        locPos.y += (((t == 0) ? 0f : BUTTONS_SPACING) + selfTransform.sizeDelta.y);
        AbilitiesButtonsLayouts[t].transform.localPosition = locPos;
        //PassiveButtonsLayout.transform.localPosition = locPos;
      }
      this.InitSutdownButtonUI();
      ui_inited = true;
    }
    public bool Active {
      get {
        return AbilitiesButtonsLayouts[0].activeSelf;
      }
    }
    public void Update() {
      if (ui_inited == false) {
        initUI();
      }
      if (HUD != null) {
        if (HUD.SelectedActor == null) {
          foreach (var buttons in AbilitiesButtonsLayouts) { buttons.SetActive(false); }
        } else {
          if (HUD.AttackModeSelector.Visible) {
            if (AttackModeSelectorDownPos == Vector3.zero) {
              AttackModeSelectorDownPos = HUD.AttackModeSelector.transform.localPosition;
            } else {
              if (AttackModeSelectorPosInited == false) { initAttackModeSelectorPos(filledRows); }
              if (this.AttackModeSelectorPosInited) {
                if (this.Active) {
                  HUD.AttackModeSelector.transform.localPosition = AttackModeSelectorUpPos;
                } else {
                  HUD.AttackModeSelector.transform.localPosition = AttackModeSelectorDownPos;
                }
              }
            }
          }
        }
      }
    }
    public void Init(CombatHUDMechwarriorTray mechWarriorTray, CombatHUD HUD, CombatGameState Combat) {
      this.mechWarriorTray = mechWarriorTray;
      this.HUD = HUD;
      this.Combat = Combat;
      for (int index = 0; index < AbilitiesButtons.Count; ++index) {
        CombatHUDSidePanelHoverElement panelHoverElement = AbilitiesButtons[index].gameObject.GetComponentInChildren<CombatHUDSidePanelHoverElement>(true);
        if ((UnityEngine.Object)panelHoverElement == (UnityEngine.Object)null) {
          panelHoverElement = AbilitiesButtons[index].gameObject.AddComponent<CombatHUDSidePanelHoverElement>();
        }
        panelHoverElement.Init(HUD);
        if (index < 9) {
          AbilitiesButtons[index].Init(Combat, HUD, BTInput.Instance.Combat_Abilities()[index], true);
        } else if (index == 9) {
          AbilitiesButtons[index].Init(Combat, HUD, BTInput.Instance.Combat_DoneWithMech(), true);
        } else {
          AbilitiesButtons[index].Init(Combat, HUD, null, true);
        }
        //ActiveAbilitiesButtons[index].gameObject.transform.parent.gameObject.SetActive(false);
        //panelHoverElement = PassiveAbilitiesButtons[index].gameObject.GetComponentInChildren<CombatHUDSidePanelHoverElement>(true);
        //if ((UnityEngine.Object)panelHoverElement == (UnityEngine.Object)null) {
        //  panelHoverElement = PassiveAbilitiesButtons[index].gameObject.AddComponent<CombatHUDSidePanelHoverElement>();
        //}
        //panelHoverElement.Init(HUD);
        //if (index < 9) {
        //  PassiveAbilitiesButtons[index].Init(Combat, HUD, BTInput.Instance.Combat_Abilities()[index], true);
        //}
        //PassiveAbilitiesButtons[index].gameObject.transform.parent.gameObject.SetActive(false);
      }
      //HideAbilitiesButton.Init(Combat, HUD, BTInput.Instance.Combat_DoneWithMech(), true);
      //HidePassiveAbilitiesButton.Init(Combat, HUD, BTInput.Instance.Combat_DoneWithMech(), true);
      //HideActiveAbilitiesButton.gameObject.transform.parent.gameObject.SetActive(false);
      //HidePassiveAbilitiesButton.gameObject.transform.parent.gameObject.SetActive(false);
    }
    public void Instantine(CombatHUDMechwarriorTray mechWarriorTray, CombatHUD HUD) {
      Transform buttons = mechWarriorTray.gameObject.transform.FindRecursive("mwt_ActionButtonsLayout");
      CombatHUDActionButton[] normalButtons = buttons.gameObject.GetComponentsInChildren<CombatHUDActionButton>(true);
      for (int index = 0; index < HOTKEY_BUTTONS_COUNT; ++index) {
        CombatHUDActionButton abtn = normalButtons[index];
        NormalButtons.Add(abtn);
      }
      for (int row = 0; row < BUTTONS_ROWS_COUNT; ++row) {
        GameObject activeBtns = GameObject.Instantiate(buttons.gameObject);
        GameObject AT_OuterFrameL = activeBtns.FindRecursive("AT_OuterFrameL");
        if (AT_OuterFrameL != null) { GameObject.Destroy(AT_OuterFrameL); }
        GameObject AT_OuterFrameR = activeBtns.FindRecursive("AT_OuterFrameR");
        if (AT_OuterFrameR != null) { GameObject.Destroy(AT_OuterFrameR); }
        GameObject bracerR = activeBtns.FindRecursive("braceR (1)");
        if (bracerR != null) { GameObject.Destroy(bracerR); }
        GameObject bracerL = activeBtns.FindRecursive("braceL (1)");
        if (bracerL != null) { GameObject.Destroy(bracerL); }
        CanvasGroup abilitiesCanvasGroup = activeBtns.GetComponent<CanvasGroup>();
        abilitiesCanvasGroup.interactable = true;
        abilitiesCanvasGroup.blocksRaycasts = true;
        this.AbilitiesCanvasGroup.Add(abilitiesCanvasGroup);
        activeBtns.transform.SetParent(buttons.transform.parent);
        activeBtns.transform.localScale = Vector3.one;
        this.AbilitiesButtonsLayouts.Add(activeBtns);
        //ActiveButtonsLayout = activeBtns;
        //GameObject passiveBtns = GameObject.Instantiate(buttons.gameObject);
        //PassiveAbilitiesCanvasGroup = passiveBtns.GetComponent<CanvasGroup>();
        //PassiveAbilitiesCanvasGroup.interactable = true;
        //PassiveAbilitiesCanvasGroup.blocksRaycasts = true;
        //passiveBtns.transform.SetParent(buttons.transform.parent);
        //passiveBtns.transform.localScale = Vector3.one;
        //PassiveButtonsLayout = passiveBtns;
        CombatHUDActionButton[] activeButtons = activeBtns.GetComponentsInChildren<CombatHUDActionButton>(true);
        for (int index = 0; index < activeButtons.Length; ++index) {
          CombatHUDActionButton abtn = activeButtons[index];
          //if ((index == 9)&&(row == 0)) {
          //HideAbilitiesButton = abtn;
          //} else {
          AbilitiesButtons.Add(abtn);
          //}
          actionButtons.Add(abtn);
        }
        //CombatHUDActionButton[] passiveButtons = passiveBtns.GetComponentsInChildren<CombatHUDActionButton>(true);
        //for (int index = 0; index < BUTTONS_COUNT; ++index) {
        //  CombatHUDActionButton abtn = passiveButtons[index];
        //  if (index == 9) {
        //    HidePassiveAbilitiesButton = abtn;
        //  } else {
        //    PassiveAbilitiesButtons.Add(abtn);
        //  }
        //  actionButtons.Add(abtn);
        //}
      }
      this.InstantineShutdownButton(HUD);
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
    public static void HideLanceSwitcher() {
      LanceSwitcher?.gameObject.SetActive(false);
    }
    public static void ShowLanceSwitcher() {
      LanceSwitcher?.gameObject.SetActive(true);
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
        Log.M?.TWL(0, "AddTween:" + toggle.gameObject.name + " anim:" + anim.buttonState + "->" + anim.endValueFloat);
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
    public static int GetCargoPlayerUnitsCount(this CombatGameState Combat) {
      Log.Combat?.TWL(0, "GetCargoPlayerUnitsCount");
      foreach(var lance in Combat.ActiveContract.Lances.lanceConfigurations) {
        Log.Combat?.WL(1, $"{lance.Key}");
        for(int t = 0; t < lance.Value.Count; ++t) {
          Log.Combat?.WL(2, $"[{t}]{lance.Value[t].UnitId}:{lance.Value[t].PilotId}");
        }
      }
      var playerUnits = Combat.ActiveContract.Lances.GetLanceUnits(Combat.LocalPlayerTeamGuid);
      int result = 0;
      foreach(var unit in playerUnits) {
        if ((unit.Unit == null)||(unit.Pilot == null)) { continue; }
        var cargoUnits = Combat.ActiveContract.Lances.GetLanceUnits(unit.GetSlotGUID());
        Log.Combat?.WL(1, $"unit:{unit.GetSlotGUID()}");
        foreach (var cargoUnit in cargoUnits) {
          if ((cargoUnit.Unit == null) || (cargoUnit.Pilot == null)) { continue; }
          Log.Combat?.WL(2, $"{cargoUnit.UnitId}:{cargoUnit.PilotId}");
          ++result;
        }
      }
      return result;
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
      int all_lances_size = UnityGameInstance.BattleTechGame.Simulation.currentLayout().slotsCount + Core.Settings.ConvoyUnitsCount + Combat.GetCargoPlayerUnitsCount();
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

    public static void Prefix(ref bool __runOriginal, CombatHUDMechwarriorTray __instance, CombatGameState Combat, CombatHUD HUD) {
      try {
        if (!__runOriginal) { return; }
        InitAdditinalPortraits(__instance, Combat, HUD);
        __instance.Combat = Combat;
        __instance.HUD = HUD;
        __instance.Portraits = new CombatHUDPortrait[__instance.PortraitHolders.Length];
        for (int index = 0; index < __instance.PortraitHolders.Length; ++index) {
          __instance.Portraits[index] = __instance.PortraitHolders[index].GetComponentInChildren<CombatHUDPortrait>(true);
          __instance.Portraits[index].Init(Combat, HUD, __instance.PortraitHolders[index].GetComponent<LayoutElement>(), __instance.portraitTweens[index]);
        }
        CombatHUDMechwarriorTrayEx trayEx = __instance.gameObject.GetComponent<CombatHUDMechwarriorTrayEx>();
        if (trayEx == null) {
          trayEx = __instance.gameObject.AddComponent<CombatHUDMechwarriorTrayEx>();
          trayEx.Instantine(__instance, HUD);
        }
        __instance.ActionButtons = new CombatHUDActionButton[__instance.ActionButtonHolders.Length];
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
        __instance.FireButton = __instance.ActionButtons[0];
        __instance.MoveButton = __instance.ActionButtons[1];
        __instance.SprintButton = __instance.ActionButtons[2];
        __instance.JumpButton = __instance.ActionButtons[3];
        __instance.DoneWithMechButton = __instance.ActionButtons[9];
        __instance.EjectButton = __instance.ActionButtons[10];
        trayEx.ShutdownBtn = __instance.ActionButtons[11];
        __instance.MeleeButtonHolder.GetComponentInChildren<CombatHUDActionButton>(true).Init(Combat, HUD, BTInput.Instance.Key_Return(), true);
        __instance.DFAButtonHolder.GetComponentInChildren<CombatHUDActionButton>(true).Init(Combat, HUD, BTInput.Instance.Key_Return(), true);
        __instance.CommandButton = __instance.CommandButtonHolder.GetComponentInChildren<CombatHUDActionButton>(true);
        __instance.CommandButton.Init(Combat, HUD, BTInput.Instance.Combat_CommandAbility(), true);
        __instance.EquipmentButton = __instance.EquipmentButtonHolder.GetComponentInChildren<CombatHUDActionButton>(true);
        __instance.EquipmentButton.Init(Combat, HUD, BTInput.Instance.Key_None(), true);
        __instance.DoneWithMechButton.Init(Combat, HUD, BTInput.Instance.Combat_DoneWithMech(), true);
        __instance.EjectButton.Init(Combat, HUD, BTInput.Instance.Key_None(), true);
        trayEx.ShutdownBtn?.Init(Combat, HUD, BTInput.Instance.Key_None(), true);
        __instance.otherTurnIndicator.Init(HUD);
        List<CombatHUDActionButton> AbilityButtons = new List<CombatHUDActionButton>();
        trayEx.ShowAbilitiesButton = __instance.ActionButtons[6];
        //trayEx.ShowPassiveAbilitiesButton = __instance.ActionButtons[6];
        //Traverse.Create(__instance).Property<CombatHUDActionButton[]>("AbilityButtons").Value[0] = __instance.ActionButtons[4];
        //Traverse.Create(__instance).Property<CombatHUDActionButton[]>("AbilityButtons").Value[1] = __instance.ActionButtons[5];
        AbilityButtons.Add(__instance.ActionButtons[4]);
        AbilityButtons.Add(__instance.ActionButtons[5]);
        trayEx.FirstActiveAbilities.Add(__instance.ActionButtons[4]);
        trayEx.FirstActiveAbilities.Add(__instance.ActionButtons[5]);
        CombatHUDDeployAutoActivator autoActivator = trayEx.FirstActiveAbilities[0].gameObject.GetComponent<CombatHUDDeployAutoActivator>();
        if (autoActivator == null) {
          autoActivator = trayEx.FirstActiveAbilities[0].gameObject.AddComponent<CombatHUDDeployAutoActivator>();
        }
        autoActivator.Init(trayEx.FirstActiveAbilities[0], HUD);
        for (int index = 0; index < trayEx.AbilitiesButtons.Count; ++index) {
          AbilityButtons.Add(trayEx.AbilitiesButtons[index]);
          //Traverse.Create(__instance).Property<CombatHUDActionButton[]>("AbilityButtons").Value[index+1] = trayEx.AbilitiesButtons[index];
        }
        __instance.AbilityButtons = AbilityButtons.ToArray();
        //for (int index = 0; index < trayEx.PassiveAbilitiesButtons.Count; ++index) {
        //  Traverse.Create(__instance).Property<CombatHUDActionButton[]>("AbilityButtons").Value[index + trayEx.ActiveAbilitiesButtons.Count+1] = trayEx.PassiveAbilitiesButtons[index];
        //}

        __instance.MoraleButtons = new CombatHUDActionButton[2];
        for (int index = 0; index < 2; ++index) {
          __instance.MoraleButtons[index] = __instance.ActionButtons[index + 7];
        }
        __instance.RestartButton = __instance.RestartButtonHolder.GetComponentInChildren<CombatHUDActionButton>(true);
        __instance.RestartButton.Init(Combat, HUD, BTInput.Instance.Key_Return(), true);
        __instance.RestartButton.InitButton(SelectionType.None, (Ability)null, LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.RestartButtonIcon, "BTN_Restart", LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.Tooltip_Restart, (AbstractActor)null);
        __instance.RestartButton.Text.SetText("RESTART", (object[])Array.Empty<object>());
        __instance.RestartButtonHolder.gameObject.SetActive(false);
        __instance.moraleDisplay = __instance.GetComponentInChildren<CombatHUDMoraleBar>(true);
        __instance.moraleDisplay.Init(Combat, HUD);
        __instance.SubscribeMessages(true);
        __instance.transform.localPosition = __instance.TrayPosDown;
        __instance.targetTrayPos = __instance.TrayPosDown;
        __instance.lastTrayPos = __instance.TrayPosDown;
        __runOriginal = false;
        return;
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        return;
      }
    }

    public static void Postfix(CombatHUDMechwarriorTray __instance, CombatGameState Combat, CombatHUD HUD) {
      int lance_index = 0;
      int lance_id = 0;
      lancesPortraitHolders.Clear();
      List<CustomLanceInstance> lance = null;
      DropSlotsDef layout = UnityGameInstance.BattleTechGame.Simulation.currentLayout();
      for (int index = 0; index < __instance.Portraits.Length; ++index) {
        GameObject holder = __instance.PortraitHolders[index];
        holder.SetActive(false);
        CombatHUDPortrait portrait = __instance.Portraits[index];
        if (lancesPortraitHolders.Count <= lance_id) {
          lance = new List<CustomLanceInstance>();
          lancesPortraitHolders.Add(lance);
          lance_index = 0;
        }
        //bool lanceVehicle = lance_id < CustomLanceHelper.lancesCount() ? CustomLanceHelper.lanceVehicle(lance_id) : true;
        int lanceSize = Core.Settings.PreferedLanceSize;
        if (lance_id < layout.dropLances.Count) { lanceSize = layout.dropLances[lance_id].dropSlots.Count; } else
          if (lance_id == layout.dropLances.Count) { lanceSize = Core.Settings.ConvoyUnitsCount; }
        lance.Add(new CustomLanceInstance(holder, portrait, index, lance_index == 0, lance_id, lance_index, lance_id >= layout.dropLances.Count));
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
    public static void Prefix(ref bool __runOriginal, CombatHUDMechwarriorTray __instance, AbstractActor actor) {
      if (!__runOriginal) { return; }
      CombatHUDMechwarriorTrayEx trayEx = __instance.gameObject.GetComponent<CombatHUDMechwarriorTrayEx>();
      if (trayEx == null) { return; }
      try {
        Pilot pilot = actor.GetPilot();
        //if (pilot == null) { return false; }
        if (pilot != null) {
          List<Ability> activeList = new List<Ability>((IEnumerable<Ability>)pilot.ActiveAbilities);
          List<Ability> passiveList = pilot.PassiveAbilities.FindAll((Predicate<Ability>)(x => x.Def.DisplayParams == AbilityDef.DisplayParameters.ShowInMWTRay));
          activeList.Sort((x, y) => {
            return y.Def.exDef().Priority.CompareTo(x.Def.exDef().Priority);
          });
          activeList.AddRange(passiveList);
          Log.Combat?.TWL(0, "activeList:" + activeList.Count);
          foreach (Ability ability in activeList) {
            Log.Combat?.WL(1, ability.Def.Description.Id + ":" + ability.Def.exDef().Priority);
          }
          if ((actor.IsDead == false) && (actor.IsDeployDirector())) {
            //for (int t = 0; t < trayEx.FirstActiveAbilities.Count; ++t) { }
            trayEx.FirstActiveAbilities[0].InitButton(CombatHUDMechwarriorTray.GetSelectionTypeFromTargeting(activeList[0].Def.Targeting, false), activeList[0], activeList[0].Def.AbilityIcon, activeList[0].Def.Description.Id, activeList[0].Def.Description.Name, actor);
            trayEx.FirstActiveAbilities[0].isClickable = true;
            //}
            trayEx.HideAbilities();
            //trayEx.HidePassiveAbilities();
            foreach (CombatHUDActionButton btn in trayEx.NormalButtons) {
              if (btn == trayEx.FirstActiveAbilities[0]) { continue; }
              btn.gameObject.SetActive(false);
            }
            __runOriginal = false; return;
          } else {
            foreach (CombatHUDActionButton btn in trayEx.NormalButtons) {
              btn.gameObject.SetActive(true);
            }
          }
          List<CombatHUDActionButton> avaibleButtons = new List<CombatHUDActionButton>();
          if (activeList.Count <= 3) {
            avaibleButtons.AddRange(trayEx.FirstActiveAbilities);
            avaibleButtons.Add(trayEx.ShowAbilitiesButton);
            trayEx.filledRows = 0;
          } else {
            avaibleButtons.AddRange(trayEx.FirstActiveAbilities);
            avaibleButtons.AddRange(trayEx.AbilitiesButtons);
            int buttons_in_rows = activeList.Count - 2;
            //trayEx.filledRows = 1;
            //if (buttons_in_rows > (CombatHUDMechwarriorTrayEx.HOTKEY_BUTTONS_COUNT - 1)) {
            //buttons_in_rows -= (CombatHUDMechwarriorTrayEx.HOTKEY_BUTTONS_COUNT - 1);
            trayEx.filledRows = (buttons_in_rows / CombatHUDMechwarriorTrayEx.HOTKEY_BUTTONS_COUNT);
            trayEx.filledRows += (buttons_in_rows % CombatHUDMechwarriorTrayEx.HOTKEY_BUTTONS_COUNT == 0) ? 0 : 1;
            //}
          }
          __instance.AbilityButtons = avaibleButtons.ToArray();
          trayEx.AttackModeSelectorPosInited = false;
          for (int t = 0; t < avaibleButtons.Count; ++t) {
            if (t < activeList.Count) {
              if (activeList[t].Def.Description.Id == CustomAmmoCategoriesPatches.CombatHUDMechwarriorTray_InitAbilityButtons.AbilityName) {
                CustomAmmoCategoriesPatches.CombatHUDMechwarriorTray_InitAbilityButtons.InitAttackGroundAbilityButton(actor, activeList[t], avaibleButtons[t]);
              } else {
                avaibleButtons[t].InitButton(CombatHUDMechwarriorTray.GetSelectionTypeFromTargeting(activeList[t].Def.Targeting, false), activeList[t], activeList[t].Def.AbilityIcon, activeList[t].Def.Description.Id, activeList[t].Def.Description.Name, actor);
                avaibleButtons[t].isClickable = (activeList[t].Def.ActivationTime != AbilityDef.ActivationTiming.Passive);
                avaibleButtons[t].RefreshUIColors();
              }
            } else {
              avaibleButtons[t].InitButton(SelectionType.None, (Ability)null, (SVGAsset)null, "", "", (AbstractActor)null);
              avaibleButtons[t].isClickable = false;
              avaibleButtons[t].RefreshUIColors();
            }
          }
          UILookAndColorConstants andColorConstants = LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants;
          if (trayEx.filledRows > 0) {
            trayEx.ShowAbilitiesButton.InitButton(SelectionType.None, (Ability)null,
              (string.IsNullOrEmpty(Core.Settings.ShowActiveAbilitiesIcon) ? andColorConstants.SprintButtonIcon : CustomSvgCache.get(Core.Settings.ShowActiveAbilitiesIcon, trayEx.HUD.Combat.DataManager))
              , CombatHUDMechwarriorTrayEx.SHOW_ACTIVE_BUTTON_GUID, CombatHUDMechwarriorTrayEx.SHOW_ACTIVE_BUTTON_NAME, actor);
            trayEx.ShowAbilitiesButton.isClickable = true;
            trayEx.ShowAbilitiesButton.RefreshUIColors();
          }
        }
        if (actor.team.CommandAbilities.Count > 0) {
          Ability commandAbility = actor.team.CommandAbilities[0];
          __instance.CommandButton.InitButton(CombatHUDMechwarriorTray.GetSelectionTypeFromTargeting(commandAbility.Def.Targeting, false), commandAbility, commandAbility.Def.AbilityIcon, commandAbility.Def.Description.Id, commandAbility.Def.Description.Name, (AbstractActor)null);
        } else {
          __instance.CommandButton.InitButton(SelectionType.None, (Ability)null, (SVGAsset)null, "", "", (AbstractActor)null);
        }
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        UIManager.logger.LogException(e);
      }
      __runOriginal = false; return;
    }
  }
  [HarmonyPatch(typeof(CombatHUDMechwarriorTray))]
  [HarmonyPatch("RefreshActionHotKeys")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatHUDMechwarriorTray_RefreshActionHotKeys {
    public static void Prefix(ref bool __runOriginal, CombatHUDMechwarriorTray __instance) {
      Log.Combat?.WL(0,"CombatHUDMechwarriorTray.RefreshActionHotKeys");
      if (!__runOriginal) { return; }
      try {
        CombatHUDMechwarriorTrayEx trayEx = __instance.gameObject.GetComponent<CombatHUDMechwarriorTrayEx>();
        if (trayEx == null) { return; }
        if (trayEx.Active) {
          for (int index = 0; index < trayEx.NormalButtons.Count; ++index) {
            if (index < 9) {
              trayEx.NormalButtons[index].SetHotKey(null, true);
            }
          }
          __instance.DoneWithMechButton.SetHotKey(null, true);
          for (int index = 0; index < trayEx.AbilitiesButtons.Count; ++index) {
            if (index < 9) {
              trayEx.AbilitiesButtons[index].SetHotKey(Core.Settings.DisableHotKeys ? null : BTInput.Instance.Combat_Abilities()[index], true);
            } else if (index == 9) {
              trayEx.AbilitiesButtons[index].SetHotKey(Core.Settings.DisableHotKeys ? null : BTInput.Instance.Combat_DoneWithMech(), true);
            }
          }
        } else {
          for (int index = 0; index < trayEx.NormalButtons.Count; ++index) {
            if (index < 9) {
              trayEx.NormalButtons[index].SetHotKey(Core.Settings.DisableHotKeys ? null : BTInput.Instance.Combat_Abilities()[index], true);
            }
          }
          __instance.DoneWithMechButton.SetHotKey(Core.Settings.DisableHotKeys ? null : BTInput.Instance.Combat_DoneWithMech(), true);
        }
      }catch(Exception e) {
        Log.ECombat?.TWL(0,e.ToString(),true);
        UIManager.logger.LogException(e);
      }
      __runOriginal = false; return;
    }
  }
  [HarmonyPatch(typeof(SelectionState))]
  [HarmonyPatch("ProcessDoneWithMechButton")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class SelectionState_ProcessDoneWithMechButton {
    public static void Prefix(ref bool __runOriginal, SelectionState __instance, string button, ref bool __result) {
      Log.Combat?.TWL(0,$"SelectionState.ProcessDoneWithMechButton {button}");
      if (!__runOriginal) { return; }
      try {
        if ((button != "BTN_DoneWithMech") || __instance.Orders != null) {
          Log.Combat?.WL(1, "button is not BTN_DoneWithMech or order is available");
          return;
        }
        if (__instance.SelectedActor == null) {
          Log.Combat?.WL(1, "SelectedActor is null");
          return;
        }
        Log.Combat?.WL(1,$"{__instance.SelectedActor.PilotableActorDef.ChassisID} HasFiredThisRound:{__instance.SelectedActor.HasFiredThisRound}");
        if (__instance.SelectedActor.HasFiredThisRound == false) {
          Log.Combat?.WL(1, "actor has not fired this round");
          return;
        }
        if(__instance.SelectedActor.CanMoveAfterShooting == false) {
          Log.Combat?.WL(1, "actor can't move after shooting");
          return;
        }
        //if (__instance.SelectedActor.HasActivatedThisRound == false) {
        //  Log.WL(1, "actor has not been activated this round");
        //  return true;
        //}
        if (__instance.SelectedActor.IsAvailableThisPhase == false) {
          Log.Combat?.WL(1, "actor has not available this phase");
          return;
        }
        if (__instance.SelectedActor.Combat.StackManager.IsAnyOrderActive && (__instance.SelectedActor.Combat.TurnDirector.IsInterleaved)) {
          Log.Combat?.WL(1, "have active orders in interleaved mode");
          return;
        }
        CombatHUD HUD = Traverse.Create(__instance).Property<CombatHUD>("HUD").Value;
        if (HUD.MechWarriorTray.MoveButton.IsAvailable == false) {
          Log.Combat?.WL(1, "move button is not available");
          return;
        }
        if (__instance.SelectedActor.EncounterTags.Contains(Core.Settings.ConvoyDenyMoveTag)) {
          Log.Combat?.WL(1, "convoy unit awaiting for an extraction");
          return;
        }
        if (string.IsNullOrEmpty(__instance.SelectedActor.MoveRestrictedRegionID) == false) {
          Log.Combat?.WL(1, "move restricted for actor");
          return;
        }
        if (__instance.SelectedActor.Pathing.ArePathGridsComplete == false) {
          Log.Combat?.WL(1, $"{__instance.SelectedActor.PilotableActorDef.ChassisID} ArePathGridsComplete is false");
          __result = false;
          GenericPopupBuilder.Create("CAN'T DONE", $"UNIT CAN MOVE, THUS SHOULD MOVE").AddFader().SetAlwaysOnTop().Render();
          __runOriginal = false; return;
        }
        int moveCandidates = __instance.SelectedActor.Pathing.CurrentGrid.GetSampledPathNodes().Count;
        Log.Combat?.WL(1, $"{__instance.SelectedActor.PilotableActorDef.ChassisID} moveCandidates:{moveCandidates}");
        if (moveCandidates > 1) {
          __result = false;
          GenericPopupBuilder.Create("CAN'T DONE", $"UNIT CAN MOVE, THUS SHOULD MOVE").AddFader().SetAlwaysOnTop().Render();
          __runOriginal = false; return;
        }
      } catch (Exception e) {
        Log.Combat?.TWL(0,e.ToString(),true);
        UIManager.logger.LogException(e);
      }
      return;
    }
  }
  [HarmonyPatch(typeof(CombatHUDMechwarriorTray))]
  [HarmonyPatch("ResetMechwarriorButtons")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor) })]
  public static class CombatHUDMechwarriorTray_ResetMechwarriorButtons {
    public static void Postfix(CombatHUDMechwarriorTray __instance, AbstractActor actor) {
      Log.Combat?.TWL(0, "CombatHUDMechwarriorTray.ResetMechwarriorButtons");
      if (actor != null) {
        if (__instance.DoneWithMechButton.IsAvailable == false) {
          if (actor.IsAvailableThisPhase && ((actor.Combat.StackManager.IsAnyOrderActive == false) || (actor.Combat.TurnDirector.IsInterleaved == false))) {
            __instance.DoneWithMechButton.ResetButtonIfNotActive(actor);
          }
        }
      }
      CombatHUDMechwarriorTrayEx trayEx = __instance.gameObject.GetComponent<CombatHUDMechwarriorTrayEx>();
      if((actor != null) && (actor.IsInArtilleryMode())) {
        __instance.MoveButton.DisableButton();
        __instance.SprintButton.DisableButton();
        __instance.JumpButton.DisableButton();
        __instance.MoraleButtons[0].DisableButton();
        __instance.MoraleButtons[1].DisableButton();
        for(int index = 0; index < __instance.AbilityButtons.Length; ++index) { 
          __instance.AbilityButtons[index].DisableButton();
        }
        trayEx?.ShowAbilitiesButton.DisableButton();
        trayEx?.HideAbilities();
        trayEx?.ShutdownBtn?.DisableButton();
        return;
      }
      if (trayEx == null) { return; }
      if (actor == null) {
        trayEx.ShowAbilitiesButton.DisableButton();
        //trayEx.ShowPassiveAbilitiesButton.DisableButton();
        //trayEx.HideAbilitiesButton.DisableButton();
        //trayEx.HidePassiveAbilitiesButton.DisableButton();
        trayEx.HideAbilities();
        //trayEx.HidePassiveAbilities();
        trayEx.ShutdownBtn?.DisableButton();
      } else {
        UILookAndColorConstants andColorConstants = LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants;
        if (actor.IsShutDown) __instance.DoneWithMechButton.InitButton(SelectionType.DoneWithMech, null, andColorConstants.SprintButtonIcon, "BTN_DoneWithMech", andColorConstants.Tooltip_DoneWithMechNoBrace, actor);
        Ability shutdownAbility = actor.ShutdownAbility();
        if (shutdownAbility != null) {
          trayEx.ShutdownBtn?.InitButton(CombatHUDMechwarriorTray.GetSelectionTypeFromTargeting(shutdownAbility.Def.Targeting, false), shutdownAbility, shutdownAbility.Def.AbilityIcon, shutdownAbility.Def.Description.Id, shutdownAbility.Def.Description.Name, actor);
        }
        if (actor.HasActivatedThisRound || !actor.IsAvailableThisPhase || __instance.Combat.StackManager.IsAnyOrderActive && __instance.Combat.TurnDirector.IsInterleaved) {
          trayEx.ShutdownBtn?.DisableButton();
          if (actor.IsShutDown) __instance.DoneWithMechButton.DisableButton();
        } else {
          trayEx.ShutdownBtn?.ResetButtonIfNotActive(actor);
          if (actor.IsShutDown) __instance.DoneWithMechButton.ResetButtonIfNotActive(actor);
        }
        if (actor.IsShutDown) { trayEx.ShutdownBtn?.DisableButton(); }
        if (trayEx.filledRows > 0) {
          trayEx.ShowAbilitiesButton.InitButton(SelectionType.None, (Ability)null,
            (string.IsNullOrEmpty(Core.Settings.ShowActiveAbilitiesIcon) ? andColorConstants.SprintButtonIcon : CustomSvgCache.get(Core.Settings.ShowActiveAbilitiesIcon, trayEx.HUD.Combat.DataManager))
            , CombatHUDMechwarriorTrayEx.SHOW_ACTIVE_BUTTON_GUID, CombatHUDMechwarriorTrayEx.SHOW_ACTIVE_BUTTON_NAME, actor);
          trayEx.ShowAbilitiesButton.isClickable = true;
          trayEx.ShowAbilitiesButton.RefreshUIColors();
        }
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDActionButton))]
  [HarmonyPatch("ExecuteClick")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatHUDActionButton_ExecuteClickAbilities {
    public static void Prefix(ref bool __runOriginal, CombatHUDActionButton __instance) {
      Log.Combat?.WL(0,"CombatHUDActionButton.ExecuteClick " + __instance.GUID);
      try {
        CombatHUD HUD = __instance.HUD;
        CombatHUDMechwarriorTrayEx trayEx = HUD.MechWarriorTray.gameObject.GetComponent<CombatHUDMechwarriorTrayEx>();
        if (trayEx == null) { return; };
        if (__instance.GUID == CombatHUDMechwarriorTrayEx.SHOW_ACTIVE_BUTTON_GUID) {
          if (trayEx.Active == false) {
            trayEx.ShowActiveAbilities(trayEx.filledRows);
            trayEx.ShowAbilitiesButton.ForceActive();
          } else {
            trayEx.HideAbilities();
            trayEx.ShowAbilitiesButton.ForceInactiveIfEnabled();
          }
          //trayEx.ShowPassiveAbilitiesButton.ForceInactiveIfEnabled();
          Log.Combat?.WL(1, "ShowActiveAbilities");
          __runOriginal = false; return;
        } else
        if (__instance.GUID == CombatHUDMechwarriorTrayEx.SHOW_PASSIVE_BUTTON_GUID) {
          //trayEx.ShowPassiveAbilities();
          //trayEx.ShowPassiveAbilitiesButton.ForceActive();
          //trayEx.ShowActiveAbilitiesButton.ForceInactiveIfEnabled();
          Log.Combat?.WL(1, "ShowPassiveAbilities");
          __runOriginal = false; return;
        } else
        if (__instance.GUID == CombatHUDMechwarriorTrayEx.HIDE_PASSIVE_BUTTON_GUID) {
          //trayEx.HidePassiveAbilities();
          //trayEx.ShowPassiveAbilitiesButton.ForceInactiveIfEnabled();
          //trayEx.ShowActiveAbilitiesButton.ForceInactiveIfEnabled();
          Log.Combat?.WL(1, "HidePassiveAbilities");
          __runOriginal = false; return;
        } else
        if (__instance.GUID == CombatHUDMechwarriorTrayEx.HIDE_ACTIVE_BUTTON_GUID) {
          trayEx.HideAbilities();
          //trayEx.ShowPassiveAbilitiesButton.ForceInactiveIfEnabled();
          trayEx.ShowAbilitiesButton.ForceInactiveIfEnabled();
          Log.Combat?.WL(1, "HideActiveAbilities");
          __runOriginal = false; return;
        }
      }catch(Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        UIManager.logger.LogException(e);
      }
      return;
    }
  }
  [HarmonyPatch(typeof(CombatHUDMechwarriorTray))]
  [HarmonyPatch("ResetAbilityButtons")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor) })]
  public static class CombatHUDMechwarriorTray_ResetAbilityButtons {
    public static void Prefix(ref bool __runOriginal, CombatHUDMechwarriorTray __instance, AbstractActor actor) {
      Log.Combat?.TWL(0, "CombatHUDMechwarriorTray.ResetAbilityButtons");
      CombatHUDMechwarriorTrayEx trayEx = __instance.gameObject.GetComponent<CombatHUDMechwarriorTrayEx>();
      if (trayEx == null) { return; }
      try {
        Pilot pilot = actor.GetPilot();
        bool forceInactive = actor.HasActivatedThisRound || actor.MovingToPosition != null || __instance.Combat.StackManager.IsAnyOrderActive && __instance.Combat.TurnDirector.IsInterleaved;
        if (pilot == null) { __runOriginal = false; return; };
        List<Ability> activeList = new List<Ability>((IEnumerable<Ability>)pilot.ActiveAbilities);
        List<Ability> passiveList = new List<Ability>();
        passiveList.AddRange((IEnumerable<Ability>)pilot.PassiveAbilities.FindAll((Predicate<Ability>)(x => x.Def.DisplayParams == AbilityDef.DisplayParameters.ShowInMWTRay)));
        activeList.Sort((x, y) => {
          return y.Def.exDef().Priority.CompareTo(x.Def.exDef().Priority);
        });
        activeList.AddRange(passiveList);
        Log.Combat?.WL(1, "activeList:" + activeList.Count);
        foreach (Ability ability in activeList) {
          Log.Combat?.WL(2, ability.Def.Description.Id + ":" + ability.Def.exDef().Priority);
        }
        if ((actor.IsDead == false) && (actor.IsDeployDirector())) {
          __instance.ResetDeployButton(actor, activeList[0], trayEx.FirstActiveAbilities[0], forceInactive);
          __runOriginal = false; return;
        }
        List<CombatHUDActionButton> avaibleButtons = new List<CombatHUDActionButton>();
        if (activeList.Count <= 3) {
          avaibleButtons.AddRange(trayEx.FirstActiveAbilities);
          avaibleButtons.Add(trayEx.ShowAbilitiesButton);
        } else {
          avaibleButtons.AddRange(trayEx.FirstActiveAbilities);
          avaibleButtons.AddRange(trayEx.AbilitiesButtons);
        }
        for (int t = 0; t < avaibleButtons.Count; ++t) {
          if (t < activeList.Count) {
            if (activeList[t].Def.Description.Id == CustomAmmoCategoriesPatches.CombatHUDMechwarriorTray_InitAbilityButtons.AbilityName) {
              __instance.ResetAttackGroundButton(actor, activeList[t], avaibleButtons[t], forceInactive);
            } else {
              __instance.ResetAbilityButton_public(actor, avaibleButtons[t], activeList[t], forceInactive);
            }
          } else {
            __instance.ResetAbilityButton_public(actor, avaibleButtons[t], (Ability)null, false);
          }
        }
        if (actor.team.CommandAbilities.Count > 0) {
          __instance.ResetAbilityButton_public(actor, __instance.CommandButton, actor.team.CommandAbilities[0], forceInactive);
        } else {
          __instance.ResetAbilityButton_public(actor, __instance.CommandButton, (Ability)null, false);
        }
        __runOriginal = false; return;
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        UIManager.logger.LogException(e);
        return;
      }
    }
    public static void Postfix(CombatHUDMechwarriorTray __instance, AbstractActor actor) {
      CombatHUDMechwarriorTrayEx trayEx = __instance.gameObject.GetComponent<CombatHUDMechwarriorTrayEx>();
      if (trayEx != null) {
        if (trayEx.filledRows > 0) trayEx.ShowAbilitiesButton.ResetButtonIfNotActive(actor);
        //trayEx.ShowPassiveAbilitiesButton.ResetButtonIfNotActive(actor);
        //trayEx.HideAbilitiesButton.ResetButtonIfNotActive(actor);
        //trayEx.HidePassiveAbilitiesButton.ResetButtonIfNotActive(actor);
      }
    }
  }
  [HarmonyPatch(typeof(AbilityDef))]
  [HarmonyPatch("FromJSON")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class AbilityDef_FromJSON {
    private static ConcurrentDictionary<string, AbilityDefEx> exDefinitions = new ConcurrentDictionary<string, AbilityDefEx>();
    private static AbilityDefEx empty = new AbilityDefEx();
    public static AbilityDefEx exDef(this AbilityDef aDef) {
      if (exDefinitions.TryGetValue(aDef.Id, out AbilityDefEx res)) {
        return res;
      }
      return empty;
    }
    public static void Prefix(AbilityDef __instance, ref string json, ref AbilityDefEx __state) {
      //Log.LogWrite("AbilityDef.FromJSON");
      JObject jAbilityDef = JObject.Parse(json);
      __state = new AbilityDefEx();
      //__state.Priority = 0;
      if (jAbilityDef["Priority"] != null) { __state.Priority = (int)jAbilityDef["Priority"]; jAbilityDef.Remove("Priority"); }
      if (jAbilityDef["CanBeUsedInShutdown"] != null) { __state.CanBeUsedInShutdown = (bool)jAbilityDef["CanBeUsedInShutdown"]; jAbilityDef.Remove("CanBeUsedInShutdown"); }
      json = jAbilityDef.ToString(Formatting.Indented);
    }
    public static void Postfix(AbilityDef __instance, string json, AbilityDefEx __state) {
      exDefinitions.AddOrUpdate(__instance.Id, __state, (k, v) => { return __state; });
      //if (exDefinitions.ContainsKey(__instance.Id)) { exDefinitions[__instance.Id] = __state; } else { exDefinitions.Add(__instance.Id, __state); }
    }
  }
}