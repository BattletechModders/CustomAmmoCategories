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
using BattleTech.UI;
using CustAmmoCategories;
using CustAmmoCategoriesPatches;
using CustomAmmoCategoriesLog;
using Harmony;
using Localize;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;

namespace CustAmmoCategories {
  public static class PathingInfoHelper {
    private static Func<AbstractActor, float> MaxMoveDelegate = null;
    private static Func<AbstractActor, float> CostLeftDelegate = null;
    public static float MaxMoveDistance(this AbstractActor unit) {
      if (MaxMoveDelegate == null) { return unit.Pathing.MaxCost; }
      return MaxMoveDelegate(unit);
    }
    public static float MoveCostLeft(this AbstractActor unit) {
      if (CostLeftDelegate == null) { return unit.Pathing.CostLeft; }
      return CostLeftDelegate(unit);
    }
    public static void RegisterMaxMoveDeligate(Func<AbstractActor, float> maxcost) {
      MaxMoveDelegate = maxcost;
    }
    public static void RegisterMoveCostLeft(Func<AbstractActor, float> costleft) {
      CostLeftDelegate = costleft;
    }
  }
  public static class CombatHUDInfoSidePanelHelper {
    private static Dictionary<AbstractActor, Text> externalSelfInfo = new Dictionary<AbstractActor, Text>();
    private static Dictionary<AbstractActor, Dictionary<ICombatant, Text>> externalTargetsInfo = new Dictionary<AbstractActor, Dictionary<ICombatant, Text>>();
    public static void SetSelfInfo(AbstractActor actor, Text text) {
      if (externalSelfInfo.ContainsKey(actor) == false) {
        externalSelfInfo.Add(actor, text);
      } else {
        externalSelfInfo[actor] = text;
      }
      CombatHUD_Init.HUD().RefreshSidePanelInfo();
    }
    public static void SetTargetInfo(AbstractActor actor,ICombatant target, Text text) {
      if (externalTargetsInfo.TryGetValue(actor, out Dictionary<ICombatant, Text> targetsInfo) == false) {
        targetsInfo = new Dictionary<ICombatant, Text>();
        externalTargetsInfo.Add(actor, targetsInfo);
      }
      if (targetsInfo.ContainsKey(target) == false) {
        targetsInfo.Add(target, text);
      } else {
        targetsInfo[target] = text;
      }
      CombatHUD_Init.HUD().RefreshSidePanelInfo();
    }
    public static Text getSelfInfo(this AbstractActor unit) {
      if(externalSelfInfo.TryGetValue(unit, out Text info) == false) {
        info = new Text("?????????");
        externalSelfInfo.Add(unit, info);
        return info;
      }
      return info;
    }
    public static Text getTargetInfo(this AbstractActor unit, ICombatant target) {
      if (externalTargetsInfo.TryGetValue(unit, out Dictionary<ICombatant, Text> targetsInfo) == false) {
        targetsInfo = new Dictionary<ICombatant, Text>();
        externalTargetsInfo.Add(unit, targetsInfo);
      }
      if (targetsInfo.TryGetValue(target, out Text info) == false) {
        info = new Text("?????????");
        targetsInfo.Add(target, info);
        return info;
      }
      return info;
    }
    public static void Clear() {
      externalSelfInfo.Clear();
      externalTargetsInfo.Clear();
    }
  }
}

namespace CustAmmoCategoriesPatches {
  [HarmonyPatch(typeof(CombatHUDTargetingComputer))]
  [HarmonyPatch("RefreshActorInfo")]
  [HarmonyPatch(MethodType.Normal)]
  public static class CombatHUDTargetingComputer_RefreshActorInfo {
    public static void Postfix(CombatHUDTargetingComputer __instance, CombatHUD ___HUD) {
      Log.M.TWL(0, "CombatHUDTargetingComputer.RefreshActorInfo " + (__instance.ActivelyShownCombatant == null ? "null" : (new Text(__instance.ActivelyShownCombatant.DisplayName).ToString())));
      ___HUD.RefreshSidePanelInfo();
    }
  }
  [HarmonyPatch(typeof(CombatHUDInfoSidePanel))]
  [HarmonyPatch("Update")]
  [HarmonyPatch(MethodType.Normal)]
  public static class CombatHUDInfoSidePanel_Update {
    private static bool fSidePanelNeedToBeRefreshed = true;
    private static bool fTextNeedToBeRefreshed = true;
    private static PropertyInfo p_forceShown = typeof(CombatHUDInfoSidePanel).GetProperty("forceShown",BindingFlags.NonPublic|BindingFlags.Instance);
    private static PropertyInfo p_stayShown = typeof(CombatHUDInfoSidePanel).GetProperty("stayShown", BindingFlags.NonPublic | BindingFlags.Instance);
    private static Text description = new Text();
    private static Text title = new Text();
    public static bool forceShown(this CombatHUDInfoSidePanel panel) { return (bool)p_forceShown.GetValue(panel); }
    public static bool stayShown(this CombatHUDInfoSidePanel panel) { return (bool)p_stayShown.GetValue(panel); }
    private static bool m_infoPanelShowState = true;
    public static void SidePanelInit(this CombatHUD HUD) { m_infoPanelShowState = CustomAmmoCategories.Settings.InfoPanelDefaultState; }
    public static void InfoPanelShowState(this CombatHUD HUD, bool state) { m_infoPanelShowState = state; fSidePanelNeedToBeRefreshed = true; }
    public static void RefreshSidePanelInfo(this CombatHUD HUD) { fSidePanelNeedToBeRefreshed = true; }
    public static bool InfoPanelShowState(this CombatHUD HUD) { return m_infoPanelShowState; }
    public static void UpdateInfoText(this CombatHUD __instance) {
      if (__instance.SelectedActor == null) {
        fSidePanelNeedToBeRefreshed = false;
        return;
      }
      description = new Text();
      title = new Text();
      bool empty = true;
      if (__instance.SelectionHandler != null) {
        SelectionStateMove moveState = __instance.SelectionHandler.ActiveState as SelectionStateMove;
        SelectionStateJump jumpState = __instance.SelectionHandler.ActiveState as SelectionStateJump;
        if (moveState != null) {
          empty = __instance.appendTerrainText(__instance.SelectedActor, moveState.PreviewPos, MoveType.Walking, ref title, ref description);
          //moveState.PreviewPos
        } else if (jumpState != null) {
          empty = __instance.appendTerrainText(__instance.SelectedActor, jumpState.PreviewPos, MoveType.Jumping, ref title, ref description);
        }
      }
      if (empty) { title.Append("INFO"); } else { description.Append("\n"); };
      if (CustomAmmoCategories.Settings.SidePanelInfoSelfExternal) {
        description.Append(__instance.SelectedActor.getSelfInfo().ToString());
      } else {
        description.Append(__instance.ShowNumericInfo().ToString());
      }
      if (__instance.TargetingComputer.ActivelyShownCombatant != null) {
        description.Append("\n__/TARGET/__:\n");
        description.Append(__instance.ShowTargetNumericInfo().ToString());
      }
      if(CombatHUDMiniMap.instance != null) {
        if (CombatHUDMiniMap.instance.Hovered) {
          description.Append("\nMinimap click to toggle size fixing. Double click to move camera");
        }
      }
      fSidePanelNeedToBeRefreshed = false;
    }
    public static void Prefix(CombatHUDInfoSidePanel __instance,ref bool ___shownForSingleFrame) {
      if (BTInput.Instance.DynamicActions.Enabled == false) { return; }
      if (__instance.HUD() == null) { return; }
      if (m_infoPanelShowState == false) { return; }
      if (__instance.HUD().SelectedActor == null) { return; }
      if (__instance.HUD().SelectedActor.IsDeployDirector()) { return; }
      if (__instance.HUD().SelectionHandler.ActiveState.Orders != null) { return; }
      if (__instance.IsHovered) { fTextNeedToBeRefreshed = true; return; }
      if (__instance.forceShown() || ___shownForSingleFrame || __instance.stayShown()) { fTextNeedToBeRefreshed = true; return; }
      if (fSidePanelNeedToBeRefreshed) { fTextNeedToBeRefreshed = true; __instance.HUD().UpdateInfoText(); }
      if (fTextNeedToBeRefreshed) { __instance.ForceShowSingleFrame(title, description, null, false); fTextNeedToBeRefreshed = false; } else {
        ___shownForSingleFrame = true;
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUD))]
  [HarmonyPatch("OnActorHovered")]
  [HarmonyPatch(MethodType.Normal)]
  public static class CombatHUD_OnActorHovered {
    public static void Postfix(CombatHUD __instance) {
      if (CustomAmmoCategories.Settings.SidePanelInfoTargetExternal == false) {
        __instance.RefreshSidePanelInfo();
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDActorInfo))]
  [HarmonyPatch("RefreshPredictedHeatInfo")]
  [HarmonyPatch(MethodType.Normal)]
  public static class CombatHUD_RefreshPredictedHeatInfo {
    public static void Postfix(CombatHUDActorInfo __instance) {
      if (CustomAmmoCategories.Settings.SidePanelInfoSelfExternal == false) {
        __instance.HUD.RefreshSidePanelInfo();
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDActorInfo))]
  [HarmonyPatch("RefreshPredictedStabilityInfo")]
  [HarmonyPatch(MethodType.Normal)]
  public static class CombatHUD_RefreshPredictedStabilityInfo {
    public static void Postfix(CombatHUDActorInfo __instance) {
      if (CustomAmmoCategories.Settings.SidePanelInfoSelfExternal == false) {
        __instance.HUD.RefreshSidePanelInfo();
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
      int burnCells = 0;
      float burnStrength = 0;
      StringBuilder minefieldStr = new StringBuilder();
      Dictionary<MineField, int> mfRolls = new Dictionary<MineField, int>();
      foreach (MapPoint mapPoint in mapPoints) {
        MapTerrainDataCellEx cell = actor.Combat.MapMetaData.mapTerrainDataCells[mapPoint.x, mapPoint.y] as MapTerrainDataCellEx;
        if (cell == null) { continue; };
        foreach (MineField mineField in cell.hexCell.MineFields) {
          if (mineField.count <= 0) { continue; };
          if (mfRolls.ContainsKey(mineField)) { mfRolls[mineField] += 1; } else { mfRolls.Add(mineField,1); }
        }
        if (cell.BurningStrength > 0) { burnCells += 1; burnStrength += cell.BurningStrength; };
      }
      if (actor.UnaffectedLandmines()) { mfRolls.Clear(); }
      if (actor.UnaffectedFire()) { burnCells = 0; }
      Dictionary<MineFieldDef, Dictionary<bool, Dictionary<bool, int>>> mfDefRolls = new Dictionary<MineFieldDef, Dictionary<bool, Dictionary<bool, int>>>();
      Dictionary<MineFieldDef, string> names = new Dictionary<MineFieldDef, string>();
      foreach (var mfRoll in mfRolls) {
        MineFieldStealthLevel sLvl = mfRoll.Key.stealthLevel(actor.team);
        if (sLvl == MineFieldStealthLevel.Invisible) { continue; }
        bool iff = mfRoll.Key.getIFFLevel(actor);
        string name = string.Empty;
        bool info = sLvl != MineFieldStealthLevel.Partial;
        int count = Math.Min(mfRoll.Key.count, mfRoll.Value);
        if(mfDefRolls.TryGetValue(mfRoll.Key.Def, out Dictionary<bool, Dictionary<bool, int>> mfInfo) == false) {
          mfInfo = new Dictionary<bool, Dictionary<bool, int>>();
          mfDefRolls.Add(mfRoll.Key.Def, mfInfo);
        }
        if(mfInfo.TryGetValue(info, out Dictionary<bool, int> mfIffs) == false) {
          mfIffs = new Dictionary<bool, int>();
          mfInfo.Add(info,mfIffs);
        }
        if (mfIffs.ContainsKey(iff) == false) { mfIffs.Add(iff, count); } else { mfIffs[iff] += count; };
        if (names.ContainsKey(mfRoll.Key.Def) == false) { names.Add(mfRoll.Key.Def, mfRoll.Key.UIName); };
      }
      foreach(var mfDefRoll in mfDefRolls) {
        MineFieldDef def = mfDefRoll.Key;
        foreach(var mfDefInfo in mfDefRoll.Value) {
          bool info = mfDefInfo.Key;
          foreach(var mfDefIff in mfDefInfo.Value) {
            bool iff = mfDefIff.Key;
            int count = mfDefIff.Value;
            if (minefieldStr.Length > 0) { minefieldStr.Append(", "); }
            if (iff) { minefieldStr.Append("<color=green>"); } else { minefieldStr.Append("<color=red>"); }
            if (info) {
              minefieldStr.Append(new Text(names[def]).ToString());
              minefieldStr.Append("("+def.Damage+")");
            } else {
              minefieldStr.Append("__/CAC.LANDMINE_UNKNOWN/__");
            }
            minefieldStr.Append(" x"+count);
            minefieldStr.Append("</color>");
          }
        }
      }
      minefield = minefieldStr.ToString();
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
      if (actor.UnaffectedLandmines() == false) {
        StringBuilder result = new StringBuilder();
        List<MapPoint> mapPoints = MapPoint.calcMapCircle(ccell.mapPoint(), CustomAmmoCategories.Settings.JumpLandingMineAttractRadius);
        HashSet<MapTerrainHexCell> hexes = new HashSet<MapTerrainHexCell>();
        foreach (MapPoint mapPoint in mapPoints) {
          MapTerrainDataCellEx cell = actor.Combat.MapMetaData.mapTerrainDataCells[mapPoint.x, mapPoint.y] as MapTerrainDataCellEx;
          if (cell == null) { continue; };
          hexes.Add(cell.hexCell);
        }
        StringBuilder minefieldStr = new StringBuilder();
        Dictionary<MineField, int> mfRolls = new Dictionary<MineField, int>();
        foreach (MineField mineField in ccell.hexCell.MineFields) {
          if (mineField.count <= 0) { continue; };
          if (mfRolls.ContainsKey(mineField)) { mfRolls[mineField] += mineField.count; } else { mfRolls.Add(mineField, mineField.count); }
        }
        Dictionary<MineFieldDef, Dictionary<bool, Dictionary<bool, int>>> mfDefRolls = new Dictionary<MineFieldDef, Dictionary<bool, Dictionary<bool, int>>>();
        Dictionary<MineFieldDef, string> names = new Dictionary<MineFieldDef, string>();
        foreach (var mfRoll in mfRolls) {
          MineFieldStealthLevel sLvl = mfRoll.Key.stealthLevel(actor.team);
          if (sLvl == MineFieldStealthLevel.Invisible) { continue; }
          bool iff = mfRoll.Key.getIFFLevel(actor);
          string name = string.Empty;
          bool info = sLvl != MineFieldStealthLevel.Partial;
          int count = Math.Min(mfRoll.Key.count, mfRoll.Value);
          if (mfDefRolls.TryGetValue(mfRoll.Key.Def, out Dictionary<bool, Dictionary<bool, int>> mfInfo) == false) {
            mfInfo = new Dictionary<bool, Dictionary<bool, int>>();
            mfDefRolls.Add(mfRoll.Key.Def, mfInfo);
          }
          if (mfInfo.TryGetValue(info, out Dictionary<bool, int> mfIffs) == false) {
            mfIffs = new Dictionary<bool, int>();
            mfInfo.Add(info, mfIffs);
          }
          if (mfIffs.ContainsKey(iff) == false) { mfIffs.Add(iff, count); } else { mfIffs[iff] += count; };
          if (names.ContainsKey(mfRoll.Key.Def) == false) { names.Add(mfRoll.Key.Def, mfRoll.Key.UIName); };
        }
        PathingCapabilitiesDef PathingCaps = Traverse.Create(actor.Pathing).Property<PathingCapabilitiesDef>("PathingCaps").Value;
        float rollMod = 1f;
        if (CustomAmmoCategories.Settings.MineFieldPathingMods.ContainsKey(PathingCaps.Description.Id)) {
          rollMod = CustomAmmoCategories.Settings.MineFieldPathingMods[PathingCaps.Description.Id];
        }
        foreach (var mfDefRoll in mfDefRolls) {
          MineFieldDef def = mfDefRoll.Key;
          foreach (var mfDefInfo in mfDefRoll.Value) {
            bool info = mfDefInfo.Key;
            foreach (var mfDefIff in mfDefInfo.Value) {
              bool iff = mfDefIff.Key;
              int count = mfDefIff.Value;
              if (minefieldStr.Length > 0) { minefieldStr.Append(", "); }
              if (iff || actor.UnaffectedLandmines()) { minefieldStr.Append("<color=green>"); } else { minefieldStr.Append("<color=red>"); }
              if (info) {
                minefieldStr.Append(new Text(names[def]).ToString());
                minefieldStr.Append("(" + def.Damage + " " + Mathf.Round(def.Chance * rollMod * 100.0f) + "%)");
              } else {
                minefieldStr.Append("__/CAC.LANDMINE_UNKNOWN/__");
              }
              minefieldStr.Append(" x" + count);
              minefieldStr.Append("</color>");
            }
          }
        }
        minefield = minefieldStr.ToString();
      } else {
        minefield = string.Empty;
      }
      if ((ccell.BurningStrength > 0)&&(actor.UnaffectedFire() == false)) {
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
          //maxMove = move is SelectionStateSprint sprint ? actor.MaxSprintDistance : actor.MaxWalkDistance;
          maxMove = actor.MaxMoveDistance();
          //actor.Pathing.CurrentGrid.GetPathTo(move.PreviewPos, actor.Pathing.CurrentDestination, maxMove, null, out spareMove, out Vector3 ResultDestination, out float lockedAngle, false, 0f, 0f, 0f, true, false);
          spareMove = actor.MoveCostLeft();
          //spareMove = DeduceLockedAngle(spareMove, actor.Pathing, ref maxMove);
          moveType = move is SelectionStateSprint ? HUD.uiManager().UILookAndColorConstants.Tooltip_Sprint : HUD.uiManager().UILookAndColorConstants.Tooltip_Move;
        } else if ((HUD.SelectionHandler.ActiveState is SelectionStateJump jump)&&(mech != null)) {
          maxMove = mech.JumpDistance;
          spareMove = maxMove - Vector3.Distance(jump.PreviewPos, actor.CurrentPosition);
          moveType = HUD.uiManager().UILookAndColorConstants.Tooltip_Jump;
        }
      } catch (Exception ex) { Log.M.TWL(0, ex.ToString(), true); }

      if (moveType != null) {
        movement = "\n" + FormatMeter(moveType, spareMove, maxMove);
      }
    }
    public static Text ShowNumericInfo(this CombatHUD HUD) {
      StringBuilder text = new StringBuilder(100);
      AbstractActor actor = HUD.SelectedActor;
      try {
        if (actor == null) return new Text(text.ToString());
        string numbers = null, postfix = null;
        text.Append("<size=100%><b>");
        {
          Mech mech = actor as Mech;
          if (mech != null) {
            //string heatStr = "CH:" + mech.CurrentHeat;
            float heat = mech.CurrentHeat, stab = mech.CurrentStability;
            GetPreviewNumbers(HUD, actor, ref heat, ref stab, ref postfix);
            numbers = FormatPrediction("\n__/AIM.HEAT/__", mech.CurrentHeat, heat) + "\n"
                    + FormatPrediction("__/AIM.STABILITY/__", mech.CurrentStability, stab);
          } else {
            float heat = 0f, stab = 0f;
            GetPreviewNumbers(HUD, actor, ref heat, ref stab, ref postfix);
          }
          text.Append(GetBasicInfo(actor));
          text.Append(numbers).Append(postfix);
        }
        text.Append("</b></size>");

      } catch (Exception ex) { Log.M.TWL(0, ex.ToString(), true); }
      return new Text(text.ToString());
    }
    public static Text ShowTargetNumericInfo(this CombatHUD HUD) {
      StringBuilder text = new StringBuilder(100);
      ICombatant target = HUD.TargetingComputer.ActivelyShownCombatant;
      if (target == null) { target = HUD.HoveredCombatant; }
      try {
        if (target == null) { return new Text(text.ToString()); }
        if (CustomAmmoCategories.Settings.SidePanelInfoTargetExternal) {
          return HUD.SelectedActor.getTargetInfo(target);
        }
        //if (HUD.TargetingComputer.ActorInfo.DetailsDisplay.gameObject.activeSelf == false) { return new Text("?????"); }
        string numbers = null, postfix = null;
        text.Append("<size=100%><b>");
        {
          AbstractActor actor = target as AbstractActor;
          if(actor != null) {
            postfix = "\n__/SPEED/__:"+actor.MaxWalkDistance+"/"+actor.MaxSprintDistance;
            Vector3 pos = HUD.SelectedActor.CurrentPosition;
            if (HUD.SelectionHandler != null) {
              SelectionStateMove moveState = HUD.SelectionHandler.ActiveState as SelectionStateMove;
              SelectionStateJump jumpState = HUD.SelectionHandler.ActiveState as SelectionStateJump;
              if (moveState != null) {
                pos = moveState.PreviewPos;
                //moveState.PreviewPos
              } else if (jumpState != null) {
                pos = jumpState.PreviewPos;
              }
            }
            postfix = "\n__/DISTANCE/__:" + Mathf.Round(Vector3.Distance(pos, actor.CurrentPosition));
            Pilot pilot = actor.GetPilot();
            if (pilot != null) {
              bool first = true;
              foreach (Ability ability in pilot.Abilities) {
                if (ability?.Def == null || !ability.Def.IsPrimaryAbility) continue;
                if (first) { first = false; } else { text.Append("\n"); }
                text.Append(ability.Def.Description?.Name);
              }
              if (text.Length <= 0) text.Append("\n__/AIM.NO_SKILL/__");
            }
          }
          if (target is Mech mech) {
            //string heatStr = "CH:" + mech.CurrentHeat;
            numbers = FormatMeter("\n__/AIM.HEAT/__", mech.CurrentHeat, mech.MaxHeat) + "\n"
                    + FormatMeter("__/AIM.STABILITY/__", mech.CurrentStability, mech.MaxStability);
          }
          text.Append(GetBasicInfo(target));
          text.Append(numbers).Append(postfix);
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
    public static bool appendTerrainText(this CombatHUD HUD,AbstractActor actor, Vector3 worldPos, MoveType moveType, ref Text title, ref Text description) {
      List<MapEncounterLayerDataCell> cells = new List<MapEncounterLayerDataCell>();
      cells.Add(HUD.Combat.EncounterLayerData.GetCellAt(worldPos));
      MapTerrainDataCell relatedTerrainCell = cells[0].relatedTerrainCell;
      DesignMaskDef priorityDesignMask = actor.Combat.MapMetaData.GetPriorityDesignMask(relatedTerrainCell);
      if (actor.UnaffectedDesignMasks()) { priorityDesignMask = null; }
      MapTerrainDataCellEx cell = relatedTerrainCell as MapTerrainDataCellEx;
      bool isDropshipZone = SplatMapInfo.IsDropshipLandingZone(relatedTerrainCell.terrainMask);
      bool isDangerZone = SplatMapInfo.IsDangerousLocation(relatedTerrainCell.terrainMask);
      bool isDropPodZone = SplatMapInfo.IsDropPodLandingZone(relatedTerrainCell.terrainMask);
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
        description.Append(minefieldText);
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
      return empty;
    }
    private static string externalMoveTypeText = string.Empty;
    private static string originalMoveTypeText = string.Empty;
    public static void ClearMoveTypeText(this CombatHUD hud) { externalMoveTypeText = string.Empty; CombatMovementReticle.Instance.StatusPreview.MoveTypeText.SetText(originalMoveTypeText); }
    public static void SetExMoveTypeText(this CombatHUD hud,string info) { externalMoveTypeText = info; CombatMovementReticle.Instance.StatusPreview.MoveTypeText.SetText(externalMoveTypeText); }
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
      __instance.PreviewStatusPanel.ShowPreviewStatuses(actor, relatedTerrainCell, moveType, worldPos);
      DesignMaskDef priorityDesignMask = actor.Combat.MapMetaData.GetPriorityDesignMask(relatedTerrainCell);
      MapTerrainDataCellEx cell = relatedTerrainCell as MapTerrainDataCellEx;
      Text description = new Text();
      Text title = new Text();
      bool empty = HUD.appendTerrainText(actor,worldPos,moveType,ref title,ref description);
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
      if (HUD.InfoPanelShowState()) {
        if (empty) { title.Append("INFO"); } else { description.Append("\n"); };
        if (CustomAmmoCategories.Settings.SidePanelInfoSelfExternal) {
          description.Append(actor.getSelfInfo().ToString());
        } else {
          description.Append(HUD.ShowNumericInfo().ToString());
        }
        if (HUD.TargetingComputer.ActivelyShownCombatant != null) {
          description.Append("\n__/TARGET/__:\n");
          description.Append(HUD.ShowTargetNumericInfo().ToString());
        }
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
        case MoveType.Walking: originalMoveTypeText = HUD.MoveButton.Tooltip.text; break;
        case MoveType.Sprinting: originalMoveTypeText = HUD.SprintButton.Tooltip.text; break;
        case MoveType.Backward: break;
        case MoveType.Jumping: originalMoveTypeText = HUD.JumpButton.Tooltip.text;  break;
        case MoveType.Melee: originalMoveTypeText = HUD.MoveButton.Tooltip.text; break;
        default: originalMoveTypeText = string.Empty; break;
      }
      if (string.IsNullOrEmpty(externalMoveTypeText)) {
        __instance.MoveTypeText.SetText(originalMoveTypeText, new object[0]);
      } else {
        __instance.MoveTypeText.SetText(externalMoveTypeText, new object[0]);
      }
      return false;
    }
  }
}