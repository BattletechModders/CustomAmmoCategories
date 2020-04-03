using BattleTech;
using BattleTech.UI;
using CustAmmoCategories;
using CustomAmmoCategoriesLog;
using Harmony;
using Localize;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;

namespace CustAmmoCategoriesPatches {
  [HarmonyPatch(typeof(CombatHUD))]
  [HarmonyPatch("ShowTarget")]
  [HarmonyPatch(MethodType.Normal)]
  public static class CombatHUD_ShowTarget {
    private static bool fSidePanelTargetShown = false;
    private static bool fSidePanelNeedToBeRefreshed = true;
    public static void SidePanelTargetRefresh(this CombatHUD HUD) { fSidePanelNeedToBeRefreshed = true; }
    public static bool isSidePanelTargetRefresh(this CombatHUD HUD) { return fSidePanelNeedToBeRefreshed; }
    public static bool SidePanelTargetShown(this CombatHUD HUD) { return fSidePanelTargetShown; }
    public static void SidePanelTargetShown(this CombatHUD HUD, bool value) { fSidePanelTargetShown = value; if (value == false) { fSidePanelNeedToBeRefreshed = false; }; }
    public static void ShowSidePanelTargetInfo(this CombatHUD __instance) {
      Text description = new Text();
      Text title = new Text();
      title.Append("__/INFO/__");
      description.Append(__instance.ShowNumericInfo().ToString());
      description.Append("__/TARGET/__:\n");
      description.Append(__instance.ShowTargetNumericInfo().ToString());
      __instance.SidePanel.ForceShowSingleFrame(title, description, new Text(), false);
      fSidePanelNeedToBeRefreshed = false;
    }
    public static void Postfix(CombatHUD __instance, ICombatant target) {
      if (__instance.ShowSidePanelActorInfo()) {
        __instance.ShowSidePanelTargetInfo();
      }
      fSidePanelTargetShown = true;
    }
  }
  [HarmonyPatch(typeof(CombatHUD))]
  [HarmonyPatch("StopShowingTarget")]
  [HarmonyPatch(MethodType.Normal)]
  public static class CombatHUD_StopShowingTarget {
    public static void Postfix(CombatHUD __instance, ICombatant target) {
      if (__instance.SidePanelTargetShown()) {
        __instance.SidePanelTargetShown(false);
        __instance.SidePanel.ForceHide();
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDActorInfo))]
  [HarmonyPatch("RefreshPredictedHeatInfo")]
  [HarmonyPatch(MethodType.Normal)]
  public static class CombatHUD_RefreshPredictedHeatInfo {
    public static void Postfix(CombatHUDActorInfo __instance) {
      if (__instance.HUD.SidePanelTargetShown()) {
        __instance.HUD.SidePanelTargetRefresh();
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDActorInfo))]
  [HarmonyPatch("RefreshPredictedStabilityInfo")]
  [HarmonyPatch(MethodType.Normal)]
  public static class CombatHUD_RefreshPredictedStabilityInfo {
    public static void Postfix(CombatHUDActorInfo __instance) {
      if (__instance.HUD.SidePanelTargetShown()) {
        __instance.HUD.SidePanelTargetRefresh();
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDMechTray))]
  [HarmonyPatch("Update")]
  [HarmonyPatch(MethodType.Normal)]
  public static class CombatHUDMechTray_Update {
    public static void Postfix(CombatHUDMechTray __instance, CombatHUD ___HUD) {
      if (___HUD.SidePanelTargetShown()&&(___HUD.isSidePanelTargetRefresh())) {
        if (___HUD.ShowSidePanelActorInfo()&&(___HUD.SidePanel.IsHovered == false)) {
          ___HUD.ShowSidePanelTargetInfo();
        }
      }
    }
  }
  [HarmonyPatch(typeof(MoveStatusPreview))]
  [HarmonyPatch("DisplayPreviewStatus")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(Vector3), typeof(MoveType) })]
  public static class MoveStatusPreview_DisplayPreviewStatus {
    public static Dictionary<AbstractActor, string> AdditionalTitles = new Dictionary<AbstractActor, string>();
    public static Dictionary<AbstractActor, string> AdditionalDescritions = new Dictionary<AbstractActor, string>();
    private static bool ShowActorInfo = true;
    public static void ShowSidePanelActorInfo(this CombatHUD HUD, bool value) {
      ShowActorInfo = value;
    }
    public static bool ShowSidePanelActorInfo(this CombatHUD HUD) {
      return ShowActorInfo;
    }
    public static void getAdditionalStringMoving(AbstractActor actor, out string title, out string description) {
      if (AdditionalTitles.ContainsKey(actor) == false) { title = string.Empty; } else { title = AdditionalTitles[actor]; }
      if (AdditionalDescritions.ContainsKey(actor) == false) { description = string.Empty; } else { description = AdditionalDescritions[actor]; }
    }
    public static void setAdditionalStringMoving(AbstractActor actor, string title, string description) {
      if (AdditionalTitles.ContainsKey(actor) == false) { AdditionalTitles.Add(actor, title); } else { AdditionalTitles[actor] = title; }
      if (AdditionalDescritions.ContainsKey(actor) == false) { AdditionalDescritions.Add(actor, description); } else { AdditionalDescritions[actor] = description; }
    }
    public static void getMineFieldStringMoving(AbstractActor actor, out string minefield, out string burnterrain) {
      StringBuilder result = new StringBuilder();
      List<WayPoint> waypointsFromPath = ActorMovementSequence.ExtractWaypointsFromPath(actor, actor.Pathing.CurrentPath, actor.Pathing.ResultDestination, (ICombatant)actor.Pathing.CurrentMeleeTarget, actor.Pathing.MoveType);
      List<MapPoint> mapPoints = DynamicMapHelper.getVisitedPoints(actor.Combat, waypointsFromPath);
      int minefieldCells = 0;
      int minefields = 0;
      int burnCells = 0;
      foreach (MapPoint mapPoint in mapPoints) {
        MapTerrainDataCellEx cell = actor.Combat.MapMetaData.mapTerrainDataCells[mapPoint.x, mapPoint.y] as MapTerrainDataCellEx;
        if (cell == null) {
          //Log.LogWrite("not extended cell "+mapPoint.x+","+mapPoint.y+"\n",true);
          continue;
        };
        bool isMinefieldCell = false;
        //Log.LogWrite(" hexCell: "+cell.hexCell.x+","+cell.hexCell.y+":"+cell.hexCell.MineField.Count+"\n");
        foreach (MineField mineField in cell.hexCell.MineField) {
          if (mineField.count <= 0) { continue; };
          minefields += 1;
          if (isMinefieldCell == false) { isMinefieldCell = true; minefieldCells += 1; };
        }
        if (cell.BurningStrength > 0) { burnCells += 1; };
      }
      if (minefields > 0) {
        minefield = "__/CAC.MINEFIELDONTHEWAY/__";
      } else {
        minefield = string.Empty;
      }
      if (burnCells > 0) {
        burnterrain = "__/CAC.FLAMESONTHEWAY/__";
      } else {
        burnterrain = string.Empty;
      }
    }
    public static void getMineFieldStringJumping(AbstractActor actor, MapTerrainDataCellEx ccell, out string minefield, out string burnterrain) {
      if (ccell == null) {
        minefield = string.Empty;
        burnterrain = string.Empty;
        return;
      };
      StringBuilder result = new StringBuilder();
      List<MapPoint> mapPoints = MapPoint.calcMapCircle(ccell.mapPoint(), CustomAmmoCategories.Settings.JumpLandingMineAttractRadius);
      int minefieldCells = 0;
      int minefields = 0;
      //CustomAmmoCategoriesLog.Log.LogWrite(" rol mod:" + rollMod + "\n");
      foreach (MapPoint mapPoint in mapPoints) {
        MapTerrainDataCellEx cell = actor.Combat.MapMetaData.mapTerrainDataCells[mapPoint.x, mapPoint.y] as MapTerrainDataCellEx;
        if (cell == null) { continue; };
        bool isMinefieldCell = false;
        foreach (MineField mineField in cell.hexCell.MineField) {
          if (mineField.count <= 0) { continue; };
          minefields += 1;
          if (isMinefieldCell == false) { isMinefieldCell = true; minefieldCells += 1; };
        }
      }
      if (minefields > 0) {
        minefield = "__/CAC.JUMPTOMINEFIELD/__";
      } else {
        minefield = string.Empty;
      }
      if (ccell.BurningStrength > 0) {
        burnterrain = "__/CAC.JUMPTOFLAMES/__";
      } else {
        burnterrain = string.Empty;
      }
    }
    private static PropertyInfo pHUD;
    private static PropertyInfo pSidePanel;
    private static PropertyInfo pTargetWorldPos;
    private static FieldInfo fShownForSingleFrame;
    private static FieldInfo fuiManager;
    private delegate CombatHUDHeatMeter d_HeatMeter(CombatHUDMechTray tray);
    private static d_HeatMeter i_HeatMeter = null;
    public static CombatHUDHeatMeter HeatMeter(this CombatHUDMechTray tray) { return i_HeatMeter(tray); }
    public static UIManager uiManager(this UIModule module) { return (UIManager)fuiManager.GetValue(module); }
    private delegate int d_GetProjectedHeat(CombatHUDHeatDisplay heatDisplay,Mech mech);
    private static d_GetProjectedHeat i_GetProjectedHeat = null;
    public static int GetProjectedHeat(this CombatHUDHeatDisplay heatDisplay, Mech mech) { return i_GetProjectedHeat(heatDisplay, mech); }
    private static Text FormatPrediction(string label, float from, float to) {
      return new Text("{0} {1:0} >> {2:0;(0)}", label, from, to);
    }
    private static Text FormatMeter(string label, float from, float max) {
      return new Text("{0} {1:0}/{2:0}", label, from, max);
    }
    private static Text GetBasicInfo(ICombatant target) {
      if (target is Mech mech) {
        int jets = mech.WorkingJumpjets;
        string weight = mech.weightClass.ToString();
        if (jets > 0)
          switch (mech.weightClass) {
            case WeightClass.LIGHT: weight = "__/AIM.LIGHTMECH/__"; break;
            case WeightClass.MEDIUM: weight = "__/AIM.MEDIUMMECH/__"; break;
            case WeightClass.HEAVY: weight = "__/AIM.HEAVYMECH/__"; break;
            case WeightClass.ASSAULT: weight = "__/AIM.ASSAULTMECH/__"; break;
          }
        string ton = ((int)mech.tonnage) + "T " + weight;
        return jets > 0 ? new Text("{0}, {1} __/AIM.JETS/__", ton, jets) : new Text(ton);

      } else if (target is Vehicle vehicle) {
        string weight = vehicle.weightClass.ToString();
        switch (vehicle.weightClass) {
          case WeightClass.LIGHT: weight = "__/AIM.LIGHTMECH/__"; break;
          case WeightClass.MEDIUM: weight = "__/AIM.MEDIUMMECH/__"; break;
          case WeightClass.HEAVY: weight = "__/AIM.HEAVYMECH/__"; break;
          case WeightClass.ASSAULT: weight = "__/AIM.ASSAULTMECH/__"; break;
        }
        return new Text("{0}T {1}", (int)vehicle.tonnage, weight);// ((int)vehicle.tonnage) + "T " + vehicle.weightClass;
      } else if (target is Turret turret) {
        string weight = turret.TurretDef.Chassis.weightClass.ToString();
        switch (turret.TurretDef.Chassis.weightClass) {
          case WeightClass.LIGHT: weight = "__/AIM.LIGHTMECH/__"; break;
          case WeightClass.MEDIUM: weight = "__/AIM.MEDIUMMECH/__"; break;
          case WeightClass.HEAVY: weight = "__/AIM.HEAVYMECH/__"; break;
          case WeightClass.ASSAULT: weight = "__/AIM.ASSAULTMECH/__"; break;
        }
        return new Text(weight);
      }
      return new Text("");
    }
    private static float DeduceLockedAngle(float spareMove, Pathing pathing, ref float maxMove) {
      float start = pathing.lockedAngle, end = pathing.ResultAngle;
      if (start == end) return spareMove; // Not yet turned
      float left = start - end, right = end - start;
      if (left < 0) left += 360;
      if (right < 0) right += 360;
      float angle = Math.Min(left, right);
      float free = pathing.MoveType == MoveType.Walking ? 45f : 22.5f; // Free turning
      angle -= free;
      if (angle <= 0) return spareMove; // Within free turning
      return spareMove - angle * pathing.MaxCost / (180 - free) / 2; // Reversed from Pathint.GetAngleAvailable
    }
    private static void GetPreviewNumbers(CombatHUD HUD, AbstractActor actor, ref float heat, ref float stab, ref string movement) {
      Mech mech = actor as Mech;
      if (mech != null) {
        heat = HUD.MechTray.ActorInfo.HeatDisplay.GetProjectedHeat(mech);
        heat = Math.Min(heat, mech.MaxHeat); if (heat < 0f) { heat = 0f; }
        stab = (int)HUD.SelectionHandler.ActiveState.ProjectedStabilityForState;
      }
      string moveType = null;
      float spareMove = 0, maxMove = 0;
      try {
        if (HUD.SelectionHandler.ActiveState is SelectionStateMove move) {
          maxMove = move is SelectionStateSprint sprint ? actor.MaxSprintDistance : actor.MaxWalkDistance;
          actor.Pathing.CurrentGrid.GetPathTo(move.PreviewPos, actor.Pathing.CurrentDestination, maxMove, null, out spareMove, out Vector3 ResultDestination, out float lockedAngle, false, 0f, 0f, 0f, true, false);
          spareMove = DeduceLockedAngle(spareMove, actor.Pathing, ref maxMove);
          moveType = move is SelectionStateSprint ? HUD.uiManager().UILookAndColorConstants.Tooltip_Sprint : HUD.uiManager().UILookAndColorConstants.Tooltip_Move;
        } else if ((HUD.SelectionHandler.ActiveState is SelectionStateJump jump)&&(mech != null)) {
          maxMove = mech.JumpDistance;
          spareMove = maxMove - Vector3.Distance(jump.PreviewPos, actor.CurrentPosition);
          moveType = HUD.uiManager().UILookAndColorConstants.Tooltip_Jump;
        }
      } catch (Exception ex) { Log.M.TWL(0, ex.ToString(), true); }

      if (moveType != null) {
        movement = FormatMeter(moveType, spareMove, maxMove) + "\n";
      }
    }
    public static Text ShowNumericInfo(this CombatHUD HUD) {
      StringBuilder text = new StringBuilder(100);
      AbstractActor actor = HUD.SelectedActor;
      try {
        if (actor == null) return new Text(text.ToString());
        string prefix = null, numbers = null, postfix = null;
        text.Append("<size=100%><b>");
        {
          if (actor is Mech mech) {
            //string heatStr = "CH:" + mech.CurrentHeat;
            float heat = mech.CurrentHeat, stab = mech.CurrentStability;
            GetPreviewNumbers(HUD, actor, ref heat, ref stab, ref postfix);
            numbers = FormatPrediction("__/AIM.HEAT/__", mech.CurrentHeat, heat) + "\n"
                    + FormatPrediction("__/AIM.STABILITY/__", mech.CurrentStability, stab) + "\n";
          } else {
            float heat = 0f, stab = 0f;
            GetPreviewNumbers(HUD, actor, ref heat, ref stab, ref postfix);
          }
          text.Append(GetBasicInfo(actor)).Append('\n');
          text.Append(prefix).Append(numbers).Append(postfix);
        }
        text.Append("</b></size>");

      } catch (Exception ex) { Log.M.TWL(0, ex.ToString(), true); }
      return new Text(text.ToString());
    }
    public static Text ShowTargetNumericInfo(this CombatHUD HUD) {
      StringBuilder text = new StringBuilder(100);
      ICombatant target = HUD.SelectedTarget;
      try {
        if (target == null) { return new Text(text.ToString()); }
        if (HUD.TargetingComputer.ActorInfo.DetailsDisplay.gameObject.activeSelf == false) { return new Text("?????"); }
        string prefix = null, numbers = null, postfix = null;
        text.Append("<size=100%><b>");
        {
          AbstractActor actor = target as AbstractActor;
          if(actor != null) {
            postfix = "__/SPEED/__:"+actor.MaxWalkDistance+"/"+actor.MaxSprintDistance;
            postfix = "__/DISTANCE/__:" + Mathf.Round(Vector3.Distance(HUD.SelectedActor.CurrentPosition,actor.CurrentPosition));
            Pilot pilot = actor.GetPilot();
            if (pilot != null) {
              foreach (Ability ability in pilot.Abilities) {
                if (ability?.Def == null || !ability.Def.IsPrimaryAbility) continue;
                text.Append(ability.Def.Description?.Name).Append("\n");
              }
              if (text.Length <= 0) text.Append("__/AIM.NO_SKILL/__");
            }
          }
          if (target is Mech mech) {
            //string heatStr = "CH:" + mech.CurrentHeat;
            numbers = FormatMeter("__/AIM.HEAT/__", mech.CurrentHeat, mech.MaxHeat) + "\n"
                    + FormatMeter("__/AIM.STABILITY/__", mech.CurrentStability, mech.MaxStability) + "\n";
          }
          text.Append(GetBasicInfo(target)).Append('\n');
          text.Append(prefix).Append(numbers).Append(postfix);
        }
        text.Append("</b></size>");

      } catch (Exception ex) { Log.M.TWL(0, ex.ToString(), true); }
      return new Text(text.ToString());
    }
    public static bool Prepare() {
      {
        MethodInfo method = typeof(CombatHUDMechTray).GetProperty("HeatMeter", BindingFlags.NonPublic | BindingFlags.Instance).GetMethod;
        var dm = new DynamicMethod("CUHeatMeter_get", typeof(CombatHUDHeatMeter), new Type[] { typeof(CombatHUDMechTray) }, typeof(CombatHUDMechTray));
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Call, method);
        gen.Emit(OpCodes.Ret);
        i_HeatMeter = (d_HeatMeter)dm.CreateDelegate(typeof(d_HeatMeter));
      }
      {
        MethodInfo method = typeof(CombatHUDHeatDisplay).GetMethod("GetProjectedHeat", BindingFlags.NonPublic | BindingFlags.Instance);
        var dm = new DynamicMethod("CUGetProjectedHeat", typeof(int), new Type[] { typeof(CombatHUDHeatDisplay),typeof(Mech) }, typeof(CombatHUDHeatDisplay));
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Call, method);
        gen.Emit(OpCodes.Ret);
        i_GetProjectedHeat = (d_GetProjectedHeat)dm.CreateDelegate(typeof(d_GetProjectedHeat));
      }
      fuiManager = typeof(UIModule).GetField("uiManager", BindingFlags.Instance | BindingFlags.NonPublic);
      if (fuiManager == null) {
        Log.M.TWL(0, "Can't find UIModule.uiManager");
        return false;
      }
      pHUD = typeof(MoveStatusPreview).GetProperty("HUD", BindingFlags.Instance | BindingFlags.NonPublic);
      if (pHUD == null) {
        Log.M.TWL(0, "Can't find MoveStatusPreview.HUD");
        return false;
      }
      pSidePanel = typeof(MoveStatusPreview).GetProperty("sidePanel", BindingFlags.Instance | BindingFlags.NonPublic);
      if (pSidePanel == null) {
        Log.M.TWL(0, "Can't find MoveStatusPreview.sidePanel");
        return false;
      }
      fShownForSingleFrame = typeof(CombatHUDInfoSidePanel).GetField("shownForSingleFrame", BindingFlags.Instance | BindingFlags.NonPublic);
      if (fShownForSingleFrame == null) {
        Log.M.TWL(0, "Can't find CombatHUDInfoSidePanel.shownForSingleFrame");
        return false;
      }
      pTargetWorldPos = typeof(MoveStatusPreview).GetProperty("TargetWorldPos", BindingFlags.Instance | BindingFlags.NonPublic);
      if (pTargetWorldPos == null) {
        Log.M.TWL(0, "Can't find MoveStatusPreview.TargetWorldPos");
        return false;
      }
      return true;
    }
    public static Vector3 TargetWorldPos(this MoveStatusPreview pr) {
      return (Vector3)pTargetWorldPos.GetValue(pr);
    }
    public static void TargetWorldPos(this MoveStatusPreview pr, Vector3 val) {
      pTargetWorldPos.SetValue(pr, val);
    }
    public static CombatHUD HUD(this MoveStatusPreview pr) {
      return (CombatHUD)pHUD.GetValue(pr, null);
    }
    public static CombatHUDInfoSidePanel sidePanel(this MoveStatusPreview pr) {
      return (CombatHUDInfoSidePanel)pSidePanel.GetValue(pr, null);
    }
    public static void shownForSingleFrame(this CombatHUDInfoSidePanel pr, bool val) {
      fShownForSingleFrame.SetValue(pr, val);
    }
    //private class TerrainSidePanelData {
    //public Text title;
    //public Text description;
    //public TerrainSidePanelData() { }
    //}
    //private static Dictionary<MapTerrainDataCell, TerrainSidePanelData> cacheSidePanelInfoData = new Dictionary<MapTerrainDataCell, TerrainSidePanelData>();
    //private static MapTerrainDataCell lastDisplayedCell = null;
    //private static int firstCounter = 100;
    private static bool Prefix(MoveStatusPreview __instance, AbstractActor actor, Vector3 worldPos, MoveType moveType) {
      /*if (firstCounter > 0) {
        __instance.sidePanel().ForceShowSingleFrame(new Text("TITLE"), new Text("DESCRIPTION"), null, false);
        firstCounter -= 1;
      } else {
        __instance.sidePanel().shownForSingleFrame(true);
      }*/
      //__instance.sidePanel().ForceShowSingleFrame(new Text("TITLE"), new Text ("DESCRIPTION"), null, false);
      //return true;
      __instance.TargetWorldPos(worldPos);
      List<MapEncounterLayerDataCell> cells = new List<MapEncounterLayerDataCell>();
      CombatHUD HUD = __instance.HUD();
      CombatHUDInfoSidePanel sidePanel = __instance.sidePanel();
      cells.Add(HUD.Combat.EncounterLayerData.GetCellAt(worldPos));
      MapTerrainDataCell relatedTerrainCell = cells[0].relatedTerrainCell;
#if BT1_8
      __instance.PreviewStatusPanel.ShowPreviewStatuses(actor, relatedTerrainCell, moveType, worldPos);
#else
      __instance.PreviewStatusPanel.ShowPreviewStatuses(actor, cells, moveType, worldPos);
#endif
      DesignMaskDef priorityDesignMask = actor.Combat.MapMetaData.GetPriorityDesignMask(relatedTerrainCell);
      MapTerrainDataCellEx cell = relatedTerrainCell as MapTerrainDataCellEx;
      bool isDropshipZone = SplatMapInfo.IsDropshipLandingZone(relatedTerrainCell.terrainMask);
      bool isDangerZone = SplatMapInfo.IsDangerousLocation(relatedTerrainCell.terrainMask);
      bool isDropPodZone = SplatMapInfo.IsDropPodLandingZone(relatedTerrainCell.terrainMask);
      Text description = new Text();
      Text title = new Text();
      bool empty = true;
      if (priorityDesignMask != null) {
        description.Append(priorityDesignMask.Description.Details);
        title.Append(priorityDesignMask.Description.Name);
        empty = false;
      }
      if (isDangerZone) {
        if (empty == false) { description.Append("\n"); };
        description.Append("<color=#ff0000ff>");
        description.Append(HUD.Combat.Constants.CombatUIConstants.DangerousLocationDesc.Details);
        description.Append("</color>");
        if (empty == false) { title.Append(" "); };
        title.Append("<color=#ff0000ff>");
        title.Append(HUD.Combat.Constants.CombatUIConstants.DangerousLocationDesc.Name);
        title.Append("</color>");
        empty = false;
      }
      if (isDropshipZone) {
        if (empty == false) { description.Append("\n"); };
        description.Append("<color=#ff0000ff>");
        description.Append(HUD.Combat.Constants.CombatUIConstants.DrophipLocationDesc.Details);
        description.Append("</color>");
        if (empty == false) { title.Append(" "); };
        title.Append("<color=#ff0000ff>");
        title.Append(HUD.Combat.Constants.CombatUIConstants.DrophipLocationDesc.Name);
        title.Append("</color>");
        empty = false;
      }
      if (isDropPodZone) {
        if (empty == false) { description.Append("\n"); };
        description.Append("<color=#ff0000ff>");
        description.Append(HUD.Combat.Constants.CombatUIConstants.DropPodLocationDesc.Details);
        description.Append("</color>");
        if (empty == false) { title.Append(" "); };
        title.Append("<color=#ff0000ff>");
        title.Append(HUD.Combat.Constants.CombatUIConstants.DropPodLocationDesc.Name);
        title.Append("</color>");
        empty = false;
      }
      string minefieldText = string.Empty;
      string burnText = string.Empty;
      if (moveType == MoveType.Jumping) {
        getMineFieldStringJumping(actor, cell, out minefieldText, out burnText);
      } else {
        getMineFieldStringMoving(actor, out minefieldText, out burnText);
      }
      if (string.IsNullOrEmpty(minefieldText) == false) {
        if (empty == false) { description.Append("\n"); };
        description.Append("<color=#ff0000ff>");
        description.Append(minefieldText);
        description.Append("</color>");
        if (empty == false) { title.Append(" "); };
        title.Append("<color=#ff0000ff>");
        title.Append("__/CAC.MINEFIELD/__");
        title.Append("</color>");
        empty = false;
      }
      if (string.IsNullOrEmpty(burnText) == false) {
        if (empty == false) { description.Append("\n"); };
        description.Append("<color=#ff0000ff>");
        description.Append(burnText);
        description.Append("</color>");
        if (empty == false) { title.Append(" "); };
        title.Append("<color=#ff0000ff>");
        title.Append("__/CAC.FLAMES/__");
        title.Append("</color>");
        empty = false;
      }
      /*string addDescr = string.Empty;
      string addTitle = string.Empty;
      if (moveType != MoveType.Jumping) {
        getAdditionalStringMoving(actor, out addTitle, out addDescr);
        if (string.IsNullOrEmpty(addDescr) == false) {
          if (empty == false) { description.Append("\n"); };
          description.Append(addDescr);
          if (empty) { title.Append(addTitle); };
          empty = false;
        }
      }*/
      if (HUD.ShowSidePanelActorInfo()) {
        if (empty) { title.Append("INFO"); } else { description.Append("\n"); };
        description.Append(HUD.ShowNumericInfo().ToString());
        empty = false;
      }
      if (empty == false) {
        Text warningText = null;
#if BT1_8
        sidePanel.ForceShowPersistant(title, description, warningText, false);
#else
        sidePanel.ForceShowSingleFrame(title, description, warningText, false);
#endif
      } else {
#if BT1_8
        sidePanel.ForceHide();
#endif
      }
      switch (moveType) {
        case MoveType.Walking:
          __instance.MoveTypeText.SetText(HUD.MoveButton.Tooltip.text, new object[0]);
          break;
        case MoveType.Sprinting:
          __instance.MoveTypeText.SetText(HUD.SprintButton.Tooltip.text, new object[0]);
          break;
        case MoveType.Backward:
          break;
        case MoveType.Jumping:
          __instance.MoveTypeText.SetText(HUD.JumpButton.Tooltip.text, new object[0]);
          break;
        case MoveType.Melee:
          __instance.MoveTypeText.SetText(HUD.MoveButton.Tooltip.text, new object[0]);
          break;
        default:
          __instance.MoveTypeText.SetText(string.Empty, new object[0]);
          break;
      }
      return false;
    }
  }
}