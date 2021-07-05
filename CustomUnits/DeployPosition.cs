using BattleTech;
using BattleTech.Data;
using BattleTech.Designed;
using BattleTech.Framework;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using CustAmmoCategories;
using CustomAmmoCategoriesPatches;
using FogOfWar;
using Harmony;
using HBS;
using HBS.Collections;
using InControl;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using TMPro;
using UnityEngine;

namespace CustomUnits {
  public class CombatHUDDeployAutoActivator: MonoBehaviour {
    public CombatHUDActionButton button { get; set; }
    public CombatHUD HUD { get; set; }
    public CombatHUDDeployAutoActivator() {
      button = null;
    }
    public void Init(CombatHUDActionButton parent, CombatHUD HUD) {
      this.button = parent;
      this.HUD = HUD;
    }
    public void Update() {
      if (button == null) { return; }
      if (HUD == null) { return; }
      if (button.Ability == null) { return; }
      if (button.IsActive) { return; }
      if (HUD.SelectedActor == null) { return; }
      if (HUD.SelectedActor.IsDead) { return; }
      if (HUD.SelectedActor.IsDeployDirector() == false) { return; }
      button.OnClick();
    }
  }
  public class TextedCircle: MonoBehaviour {
    public GameObject circle { get; set; }
    public TextMeshPro text { get; set; }
    public CombatHUD HUD { get; set; }
    public void Init(CombatHUD HUD) {
      this.HUD = HUD;
      circle = GameObject.Instantiate(CombatTargetingReticle.Instance.Circles[0]);
      circle.SetActive(true);
      circle.transform.SetParent(this.transform);
      Vector3 localScale = circle.transform.localScale;
      localScale.x = 1f * 2f;
      localScale.z = 1f * 2f;
      circle.transform.localScale = localScale;
      circle.transform.localPosition = Vector3.zero;
      GameObject textGO = new GameObject("Text");
      textGO.transform.SetParent(this.transform);
      text = textGO.AddComponent<TextMeshPro>();
      text.font = HUD.SidePanel.WarningText.font;
      text.fontSize = Core.Settings.DeployLabelFontSize;
      text.autoSizeTextContainer = true;
      text.alignment = TextAlignmentOptions.Center;
      textGO.transform.localPosition = Vector3.up * Core.Settings.DeployLabelHeight;
      //text.SetText("!TEXT!");
    }
    public void Update(Vector3 pos, float radius, string t) {
      Vector3 localScale = circle.transform.localScale;
      localScale.x = radius * 2f;
      localScale.z = radius * 2f;
      circle.transform.localScale = localScale;
      this.transform.position = pos;
      if (this.text.text != t) { this.text.SetText(t); }
    }
  }
  public static class TargetingCirclesHelper {
    private static List<TextedCircle> circles = new List<TextedCircle>();
    public static GameObject rootReticle = null;
    public static CombatHUD HUD = null;
    public static void Clear() {
      circles.Clear();
      GameObject.Destroy(rootReticle); rootReticle = null;
      HUD = null;
    }
    public static void InitCircles(int count, CombatHUD iHUD) {
      if (rootReticle == null) { rootReticle = new GameObject("TargetingCircles"); }
      if (count <= circles.Count) { return; }
      TargetingCirclesHelper.HUD = iHUD;
      while (circles.Count < count) {
        GameObject circleGO = new GameObject("Circle");
        TextedCircle circle = circleGO.AddComponent<TextedCircle>();
        circle.Init(HUD);
        circles.Add(circle);
      }
    }
    public static void HideAllCircles() {
      foreach (TextedCircle circle in circles) { circle.gameObject.SetActive(false); }
    }
    public static void HideRoot() {
      rootReticle.SetActive(false);
    }
    public static void ShowRoot() {
      rootReticle.SetActive(true);
    }
    public static void ShowCirclesCount(int count) {
      for (int t = 0; t < circles.Count; ++t) {
        if (t >= count) { if (circles[t].gameObject.activeSelf) { circles[t].gameObject.SetActive(false); } } else {
          if (circles[t].gameObject.activeSelf == false) { circles[t].gameObject.SetActive(true); }
        }
      }
    }
    public static void UpdateCircle(int index, Vector3 pos, float radius, string text) {
      if (index < 0) { return; }
      if (index >= circles.Count) { InitCircles(index + 1, HUD); };
      if (index >= circles.Count) { return; }
      //if (rootReticle.activeSelf == false) { rootReticle.SetActive(true); }
      //if(circles[index].activeSelf == false) { circles[index].SetActive(true); }
      circles[index].Update(pos, radius, text);
    }
  }
  public class DeployPosition {
    public PilotableActorDef def { get; set; }
    public UnitSpawnPointGameLogic spawnPoint { get; set; }
    public int lanceid { get; set; }
    public int posid { get; set; }
    public Vector3? position { get; set; }
  }
  public class SelectionStateCommandDeploy : SelectionStateCommandTargetSinglePoint {
    private bool HasActivated = false;
    //private List<Vector3> targetPositions;
    //public float CircleRange = 50f;
    public Vector3 goodPos;
    public SelectionStateCommandDeploy(CombatGameState Combat, CombatHUD HUD, CombatHUDActionButton FromButton, AbstractActor actor) : base(Combat, HUD, FromButton) {
      this.SelectedActor = actor;
      HasActivated = false;
    }
#if BT_PUBLIC_ASSEMBLY
    public override bool ShouldShowWeaponsUI { get { return false; } }
    public override bool ShouldShowTargetingLines { get { return false; } }
    public override bool ShouldShowFiringArcs { get { return false; } }
    public override bool showHeatWarnings { get { return false; } }
#else
    protected override bool ShouldShowWeaponsUI { get { return false; } }
    protected override bool ShouldShowTargetingLines { get { return false; } }
    protected override bool ShouldShowFiringArcs { get { return false; } }
    protected override bool showHeatWarnings { get { return false; } }
#endif
    public override bool ConsumesMovement { get { return false; } }
    public override bool ConsumesFiring { get { return false; } }
    public override bool CanBackOut { get { return NumPositionsLocked > 0; } }
    public virtual bool HasCalledShot { get { return false; } }
    public virtual bool NeedsCalledShot { get { return false; } }
    public override bool CanActorUseThisState(AbstractActor actor) { return actor.IsDeployDirector(); }
    public override bool CanDeselect { get { return false; } }
    private int NeedPositionsCount { get; set; }
    private List<DeployPosition> deployPositions;
    public void FillDeployPositionsInfo() {
      if (PlayerLanceSpawnerGameLogic_OnEnterActive.deployLoadRequest == null) {
        deployPositions.Clear();
        return;
      }
      Dictionary<int, PilotableActorDef> definitions = new Dictionary<int, PilotableActorDef>();
      for(int t=0;t< PlayerLanceSpawnerGameLogic_OnEnterActive.deployLoadRequest.playerLanceSpawner.unitSpawnerCount; ++t) {
        UnitSpawnPointGameLogic sp = PlayerLanceSpawnerGameLogic_OnEnterActive.deployLoadRequest.playerLanceSpawner.unitSpawnPointGameLogicList[t];
        if (PlayerLanceSpawnerGameLogic_OnEnterActive.deployLoadRequest.originalUnitTypes.TryGetValue(sp, out UnitType tp) == false) { continue; }
        if ((t == 0) && (tp == UnitType.Mech)) { definitions.Add(t,PlayerLanceSpawnerGameLogic_OnEnterActive.deployLoadRequest.originalMechDef); continue; }
        if (tp == UnitType.Mech) { definitions.Add(t,Traverse.Create(sp).Field<MechDef>("mechDefOverride").Value); }else
        if (tp == UnitType.Vehicle) { definitions.Add(t,Traverse.Create(sp).Field<VehicleDef>("vehicleDefOverride").Value); };
      }
      Dictionary<int,DeployPosition> positions = new Dictionary<int, DeployPosition>();
      Dictionary<int, DeployPosition> avaibleSlots = new Dictionary<int, DeployPosition>();
      {
        int index = 0;
        for (int lanceid = 0; lanceid < CustomLanceHelper.lancesCount(); ++lanceid) {
          for (int lancepos = 0; lancepos < CustomLanceHelper.lanceSize(lanceid); ++lancepos) {
            DeployPosition position = new DeployPosition();
            position.lanceid = lanceid;
            position.posid = lancepos;
            position.position = null;
            positions.Add(index, position);
            avaibleSlots.Add(index, position);
            ++index;
          }
        }
      }
      for (int t = 0; t < PlayerLanceSpawnerGameLogic_OnEnterActive.deployLoadRequest.playerLanceSpawner.unitSpawnerCount; ++t) {
        if (definitions.TryGetValue(t, out PilotableActorDef def) == false) { continue; }
        string defGUID = def.GUID;
        if (string.IsNullOrEmpty(defGUID) == false) {
          if (CustomLanceHelper.playerLanceLoadout.loadout.TryGetValue(defGUID, out int slotIndex)) {
            if (avaibleSlots.TryGetValue(slotIndex, out DeployPosition slot)) {
              slot.def = def;
              slot.spawnPoint = PlayerLanceSpawnerGameLogic_OnEnterActive.deployLoadRequest.playerLanceSpawner.unitSpawnPointGameLogicList[t];
              avaibleSlots.Remove(slotIndex);
              definitions.Remove(t);
              continue;
            }
          }
        }
      }
      for (int spawnid = 0; spawnid < PlayerLanceSpawnerGameLogic_OnEnterActive.deployLoadRequest.playerLanceSpawner.unitSpawnerCount; ++spawnid) {
        if (definitions.TryGetValue(spawnid, out PilotableActorDef def) == false) { continue; }
        for(int slotid = 0; slotid < CustomLanceHelper.fullSlotsCount(); ++slotid) {
          if (avaibleSlots.TryGetValue(slotid, out DeployPosition slot) == false) { continue; }
          slot.def = def;
          slot.spawnPoint = PlayerLanceSpawnerGameLogic_OnEnterActive.deployLoadRequest.playerLanceSpawner.unitSpawnPointGameLogicList[spawnid];
          avaibleSlots.Remove(slotid);
          definitions.Remove(spawnid);
          break;
        }
      }
      deployPositions = new List<DeployPosition>();
      Log.TWL(0,"fill deploy positions:");
      for(int t = 0; t < CustomLanceHelper.fullSlotsCount(); ++t) {
        if (positions.TryGetValue(t, out DeployPosition pos) == false) { continue; }
        if ((pos.def == null) || (pos.spawnPoint == null)) { continue; }
        Log.WL(1,t+":"+ (pos.def == null?"null":pos.def.Description.Id)+" lance:"+pos.lanceid+" pos:"+pos.posid+" spawn:"+(pos.spawnPoint == null?"null":pos.spawnPoint.GUID));
        deployPositions.Add(pos);
      }
      NeedPositionsCount = deployPositions.Count;
    }
    public override void OnAddToStack() {
      NumPositionsLocked = 0;
      TargetingCirclesHelper.InitCircles(
        PlayerLanceSpawnerGameLogic_OnEnterActive.deployLoadRequest != null? PlayerLanceSpawnerGameLogic_OnEnterActive.deployLoadRequest.playerLanceSpawner.unitSpawnerCount : 0,
        this.HUD
      );
      TargetingCirclesHelper.ShowRoot();
      TargetingCirclesHelper.ShowCirclesCount(0);
      this.ResetCache();
      Log.TWL(0, "SelectionStateCommandDeploy OnAddToStack:"+ NeedPositionsCount);
      this.FillDeployPositionsInfo();
      base.OnAddToStack();
    }
    public override void OnInactivate() {
      Log.TWL(0, "SelectionStateCommandDeploy.OnInactivate HasActivated: " + HasActivated);
      TargetingCirclesHelper.HideRoot();
      TargetingCirclesHelper.ShowCirclesCount(0);
      NumPositionsLocked = 0;
      NeedPositionsCount = 0;
      deployPositions.Clear();
      this.ResetCache();
      base.OnInactivate();
      CombatHUDMechwarriorTrayEx trayEx = this.HUD.MechWarriorTray.gameObject.GetComponent<CombatHUDMechwarriorTrayEx>();
    }
    public void AttackOrdersSet(MessageCenterMessage message) {
      this.Orders = (message as AddSequenceToStackMessage).sequence;
      //Log.TWL(0, "Attack orders set:" + Orders.GetType().ToString() + " GUID:" + Orders.SequenceGUID);
    }
    public override bool ProcessPressedButton(string button) {
      this.HasActivated = button == "BTN_FireConfirm";
      if (button != "BTN_FireConfirm") {
        return base.ProcessPressedButton(button);
      }
      if (this.Orders != null)
        return false;
      this.HideFireButton(false);
      CombatTargetingReticle.Instance.HideReticle();
      TargetingCirclesHelper.HideRoot();
      TargetingCirclesHelper.ShowCirclesCount(0);
      Mech deployDirector = HUD.SelectedActor as Mech;
      if (PlayerLanceSpawnerGameLogic_OnEnterActive.deployLoadRequest != null) {
        PlayerLanceSpawnerGameLogic_OnEnterActive.deployLoadRequest.RestoreAndSpawn(this.deployPositions, deployDirector, this.HUD);
        //PlayerLanceSpawnerGameLogic_OnEnterActive.Clear();
      }
      return true;
    }
    public override void BackOut() {
      if (this.NumPositionsLocked > this.deployPositions.Count) { this.NumPositionsLocked = this.deployPositions.Count; }
      if (this.NumPositionsLocked > 0) {
        this.HideFireButton(false);
        this.deployPositions[this.NumPositionsLocked-1].position = null;
        --this.NumPositionsLocked;
        TargetingCirclesHelper.ShowRoot();
        TargetingCirclesHelper.ShowCirclesCount(this.NumPositionsLocked);
        this.ResetCache();
        Log.TWL(0, "SelectionStateCommandDeploy.BackOut:" + this.NumPositionsLocked);
      } else {
        Debug.LogError((object)"Tried to back out of 1-point command state while backout unavailable");
      }
    }
    public void AddDeployPosition(Vector3 worldPos) {
      worldPos.y = HUD.Combat.MapMetaData.GetLerpedHeightAt(worldPos);
      worldPos = HUD.Combat.HexGrid.GetClosestPointOnGrid(worldPos);
      this.deployPositions[this.NumPositionsLocked].position = worldPos;
      ++this.NumPositionsLocked;
    }
    public void AddLancePosition(Vector3 worldPos) {
      int curLance = this.deployPositions[this.NumPositionsLocked].lanceid;
      int size = 0;
      for(int t = this.NumPositionsLocked+1; t < deployPositions.Count; ++t) {
        if (this.deployPositions[t].lanceid != curLance) { size = t - this.NumPositionsLocked; break; }
      }
      if (size == 0) { size = this.deployPositions.Count - this.NumPositionsLocked; }
      for (int t = 0; t < size; ++t) {
        Vector3 rndPos = worldPos;
        float nearest = 9999f;
        do {
          rndPos = worldPos;
          float radius = UnityEngine.Random.Range(0.5f * Core.Settings.DeploySpawnRadius, Core.Settings.DeploySpawnRadius);
          float direction = Mathf.Deg2Rad * UnityEngine.Random.Range(0f, 360f);
          //radius *= UnityEngine.Random.Range(targetRep.parentCombatant.Combat.Constants.ResolutionConstants.MissOffsetHorizontalMin, targetRep.parentCombatant.Combat.Constants.ResolutionConstants.MissOffsetHorizontalMax);
          //Vector2 vector2 = UnityEngine.Random.insideUnitCircle.normalized * radius;
          rndPos.x += Mathf.Sin(direction) * radius;
          rndPos.z += Mathf.Cos(direction) * radius;
          rndPos.y = HUD.Combat.MapMetaData.GetLerpedHeightAt(rndPos);
          rndPos = HUD.Combat.HexGrid.GetClosestPointOnGrid(rndPos);
          nearest = 9999f;
          foreach (DeployPosition depPos in deployPositions) {
            if (depPos.position.HasValue == false) { continue; }
            float dist = Vector3.Distance(depPos.position.Value, rndPos); if (nearest > dist) { nearest = dist; };
          };
          Log.WL(1, rndPos.ToString() + " distance:" + Vector3.Distance(rndPos, worldPos) + " nearest:" + nearest + " rejected:" + (nearest < Core.Settings.DeploySpawnRadius / 4f), true);
        } while (nearest < Core.Settings.DeploySpawnRadius / 4f);
        this.deployPositions[this.NumPositionsLocked + t].position = rndPos;
      }
      this.NumPositionsLocked += size;
    }
    public override bool ProcessLeftClick(Vector3 worldPos) {
      //if (this.NumPositionsLocked != 0) { return false; }
      //float originalDist = Vector3.Distance(worldPos, PlayerLanceSpawnerGameLogic_OnEnterActive.deployLoadRequest.playerLanceSpawner.Position);
      //float originalDist = Vector3.Distance(worldPos, this.HUD.SelectedActor.CurrentPosition);
      //List<Vector3> enemies = this.Combat.ActiveContract.deplayedEnemySpawnPositions();
      //float nearesEnemyDist = 9999f;
      //foreach (Vector3 unit in enemies) {
      //  float dist = Vector3.Distance(worldPos, unit);
      //  if ((nearesEnemyDist == 9999f) || (dist < nearesEnemyDist)) { nearesEnemyDist = dist; }
      //}
      //if ((originalDist < Core.Settings.DeployMaxDistanceFromOriginal) || (nearesEnemyDist > Core.Settings.DeployMinDistanceFromEnemy)) {
      if (lastViewedPosition.HasValue == false) { return false; }
      if (this.CheckPosition(lastViewedPosition.Value)) {
        if (this.NumPositionsLocked < this.deployPositions.Count) {
          this.targetPosition = lastViewedPosition.Value;
          if (Input.GetKey(KeyCode.LeftAlt)) { this.AddDeployPosition(lastViewedPosition.Value); } else { this.AddLancePosition(lastViewedPosition.Value); }
          TargetingCirclesHelper.ShowCirclesCount(this.NumPositionsLocked);
          this.ResetCache();
        }
        if (this.NumPositionsLocked >= this.deployPositions.Count) {
          this.ShowFireButton(CombatHUDFireButton.FireMode.Confirm, Ability.ProcessDetailString(this.FromButton.Ability).ToString(true));
          CombatTargetingReticle.Instance.HideReticle();
        }
        return true;
      } else {
        return false;
      }
      //} else {
      //  GenericPopupBuilder popup = GenericPopupBuilder.Create(GenericPopupType.Info, "Drop point too close to enemy position "+ Mathf.Round(nearesEnemyDist)+" < "+ Core.Settings.DeployMinDistanceFromEnemy+"\nand too far from suggested deploy "+Mathf.Round(originalDist)+" > "+ Core.Settings.DeployMaxDistanceFromOriginal);
      //  popup.AddButton("Ok", (Action)null, true, (PlayerAction)null);
      //  popup.IsNestedPopupWithBuiltInFader().CancelOnEscape().Render();
      //  return false;
      //}
    }
    public override Vector3 PreviewPos {
      get {
        return this.SelectedActor.CurrentPosition;
      }
    }
    private Dictionary<Vector3, bool> positionsCache = new Dictionary<Vector3, bool>();
    public void ResetCache() { positionsCache.Clear(); }
    public bool CheckPosition(Vector3 worldPos) {
      if (positionsCache.TryGetValue(worldPos, out bool result)) { return result; }
      result = true;
      Log.TWL(0, "SelectionStateCommandDeploy.CheckPosition "+worldPos);
      Vector3 f = worldPos + Vector3.forward * Core.Settings.DeploySpawnRadius;
      Vector3 b = worldPos + Vector3.back * Core.Settings.DeploySpawnRadius;
      Vector3 l = worldPos + Vector3.left * Core.Settings.DeploySpawnRadius;
      Vector3 r = worldPos + Vector3.right * Core.Settings.DeploySpawnRadius;
      if (HUD.Combat.EncounterLayerData.IsInEncounterBounds(f) == false) { result = false; } else
      if (HUD.Combat.EncounterLayerData.IsInEncounterBounds(b) == false) { result = false; } else
      if (HUD.Combat.EncounterLayerData.IsInEncounterBounds(l) == false) { result = false; } else
      if (HUD.Combat.EncounterLayerData.IsInEncounterBounds(r) == false) { result = false; };
      if (result == false) {
        Log.WL(1, "out of bounds");
        positionsCache.Add(worldPos, false); return false;
      }
      float originalDist = Vector3.Distance(worldPos, this.HUD.SelectedActor.CurrentPosition);
      Log.WL(1, "original dist:"+originalDist+"/"+ Core.Settings.DeployMaxDistanceFromOriginal);
      if (originalDist < Core.Settings.DeployMaxDistanceFromOriginal) { positionsCache.Add(worldPos, true); return true; }
      bool checkLancematesDistance = PlayerLanceSpawnerGameLogic_OnEnterActive.deployLoadRequest.originalSpawnMethod == SpawnUnitMethodType.ViaLeopardDropship;
      if (checkLancematesDistance) {
        Log.WL(1, "checking lancemates");
        Vector3? centerPoint = null;
        int count = 0;
        foreach (DeployPosition pos in deployPositions) {
          if (pos.position.HasValue == false) { continue; }
          Log.WL(2, "mate:"+ pos.position.Value);
          if (centerPoint == null) { centerPoint = pos.position.Value; count = 1; continue; }
          Vector3 tmp = centerPoint.Value;
          tmp.x += pos.position.Value.x;
          tmp.z += pos.position.Value.z;
          ++count;
          centerPoint = tmp;
        }
        if (centerPoint != null) {
          Vector3 tmp = centerPoint.Value;
          tmp.x = tmp.x / count;
          tmp.z = tmp.z / count;
          tmp = HUD.Combat.HexGrid.GetClosestPointOnGrid(tmp);
          Log.WL(1, "center point:" + tmp);
          float matesDistance = Vector3.Distance(tmp,worldPos);
          //Log.WL(1, worldPos + " centerPoint:" + tmp+" distance:"+matesDistance);
          if (matesDistance > Core.Settings.DeployMaxDistanceFromMates) {
            positionsCache.Add(worldPos, false);
            Log.WL(1, "too far from mates:"+matesDistance);
            return false;
          }
        }
      }
      List<Vector3> enemies = this.Combat.ActiveContract.delayedEnemySpawnPositions();
      float nearesEnemyDist = 9999f;
      foreach (Vector3 unit in enemies) {
        float dist = Vector3.Distance(worldPos, unit);
        if ((nearesEnemyDist == 9999f) || (dist < nearesEnemyDist)) { nearesEnemyDist = dist; }
      }
      result = nearesEnemyDist > Core.Settings.DeployMinDistanceFromEnemy;
      positionsCache.Add(worldPos,result);
      return result;
    }
    public Vector3? lastViewedPosition;
    public override void ProcessMousePos(Vector3 worldPos) {
      //switch (this.NumPositionsLocked) {
      //  case 0: {
      //    CombatTargetingReticle.Instance.UpdateReticle(worldPos, Core.Settings.DeploySpawnRadius, false);
      //  };break;
      //  case 1:
      //    CombatTargetingReticle.Instance.UpdateReticle(this.targetPosition, Core.Settings.DeploySpawnRadius, false);
      //  break;
      //}
      //Log.TWL(0, "SelectionState_ProcessMousePos");
      TargetingCirclesHelper.ShowCirclesCount(this.NumPositionsLocked);
      for (int index = 0; index < this.NumPositionsLocked; ++index) {
        if (deployPositions[index].position.HasValue) {
          string text = String.Empty;
          MechDef def = deployPositions[index].def as MechDef;
          if (def != null) { text = def.Chassis.VariantName; } else {
            text = deployPositions[index].def.Description.Name;
          }
          TargetingCirclesHelper.UpdateCircle(index, deployPositions[index].position.Value, Core.Settings.DeploySingleCircleRadiusTarget, text);
        }
      }
      if (this.NumPositionsLocked < deployPositions.Count) {
        worldPos = HUD.Combat.HexGrid.GetClosestPointOnGrid(worldPos);
        if (this.CheckPosition(worldPos)) {
          float range = Input.GetKey(KeyCode.LeftAlt) ? Core.Settings.DeploySingleCircleRadius : Core.Settings.DeploySpawnRadius;
          CombatTargetingReticle.Instance.UpdateReticle(worldPos, range, false);
          lastViewedPosition = worldPos;
        }
      }
      this.SelectionState_ProcessMousePos(SelectedActor.CurrentPosition);
    }
    public override int ProjectedHeatForState { get { return 0; } }
  };
  [HarmonyPatch(typeof(MessageCenter))]
  [HarmonyPatch("PublishMessage")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
  public static class MissionFailedMessage_Constructor {
    public static void Postfix(MessageCenter __instance, MessageCenterMessage message) {
      if ((message.MessageType != MessageCenterMessageType.OnMissionSucceeded)
       && (message.MessageType != MessageCenterMessageType.OnMissionFailed)
       && (message.MessageType != MessageCenterMessageType.OnMissionRetreat)
      ) { return; }
      Log.TWL(0, "MessageCenter.PublishMessage " + message.MessageType);
      Log.WL(0, Environment.StackTrace);
    }
  }
  [HarmonyPatch(typeof(SelectionState))]
  [HarmonyPatch("GetNewSelectionStateByType")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(SelectionType), typeof(CombatGameState), typeof(CombatHUD), typeof(CombatHUDActionButton), typeof(AbstractActor) })]
  public static class SelectionState_GetNewSelectionStateByType {
    public static bool SelectionForbidden = false;
    public static bool Prefix(SelectionType type, CombatGameState Combat, CombatHUD HUD, CombatHUDActionButton FromButton, AbstractActor actor, ref SelectionState __result) {
      Log.TWL(0,"SelectionState.GetNewSelectionStateByType " + type + ":" + FromButton.GUID);
      if ((type == SelectionType.CommandTargetSinglePoint) && (FromButton.GUID == PlayerLanceSpawnerGameLogic_OnEnterActive.DeployAbilityDefID)) {
        Log.WL(1,"creating own selection state");
        __result = new SelectionStateCommandDeploy(Combat, HUD, FromButton, actor);
        return false;
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(PathingManager))]
  [HarmonyPatch("removeDeadPaths")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class PathingManager_removeDeadPaths {
    public static bool SelectionForbidden = false;
    public static bool Prefix(PathingManager __instance) {
      for (int index = __instance.BlockingPaths.Count - 1; index >= 0; --index) {
        if (__instance.BlockingPaths[index].OwningActor == null)
          __instance.BlockingPaths.RemoveAt(index);
      }
      for (int index = __instance.PathsToBuild.Count - 1; index >= 0; --index) {
        if (__instance.PathsToBuild[index].OwningActor == null)
          __instance.PathsToBuild.RemoveAt(index);
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(Pathing))]
  [HarmonyPatch("ResetPathGridIfTouching")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(List<Rect>), typeof(Vector3), typeof(float), typeof(AbstractActor) })]
  public static class Pathing_ResetPathGridIfTouching {
    public static bool SelectionForbidden = false;
    public static bool Prefix(Pathing __instance, List<Rect> Rectangles, Vector3 origin, float beginAngle, AbstractActor actor) {
      PathingCapabilitiesDef PathingCaps = Traverse.Create(__instance).Property<PathingCapabilitiesDef>("PathingCaps").Value;
      if((PathingCaps == null)&&(actor.PathingCaps != null)) {
        Traverse.Create(__instance).Property<PathingCapabilitiesDef>("PathingCaps").Value = actor.PathingCaps;
      }
      Log.TWL(0, "Pathing.ResetPathGridIfTouching " + actor.DisplayName + " caps:" + (actor.PathingCaps == null ? "null" : actor.PathingCaps.Description.Id) + " pathing:" + (PathingCaps == null ? "null" : PathingCaps.Description.Id));
      return true;
    }
  }
  [HarmonyPatch(typeof(TriggerSpawn))]
  [HarmonyPatch(MethodType.Constructor)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class TriggerSpawn_Constructor {
    public static void Postfix(TriggerSpawn __instance, string spawnerGUID) {
      Log.TWL(0, "TriggerSpawn("+ spawnerGUID + ")");
      Log.WL(0,Environment.StackTrace);
    }
  }
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("VisibilityToTargetUnit")]
  [HarmonyPatch(new Type[] { typeof(ICombatant) })]
  public static class AbstractActor_VisibilityToTargetUnit {
    public static void Postfix(AbstractActor __instance, ICombatant targetUnit, ref VisibilityLevel __result) {
      if (__result == VisibilityLevel.None) { return; }
      if (__instance.IsDeployDirector()) { __result = VisibilityLevel.None; return; }
      if (targetUnit.IsDeployDirector()) { __result = VisibilityLevel.None; return; }
    }
  }
  [HarmonyPatch(typeof(SharedVisibilityCache))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("RebuildCache")]
  [HarmonyPatch(new Type[] { typeof(List<ICombatant>) })]
  public static class SharedVisibilityCache_RebuildCache {
    public static void Prefix(SharedVisibilityCache __instance, ref List<ICombatant> allLivingCombatants) {
      try {
        for(int index = 0; index < allLivingCombatants.Count; ++index) {
          if (allLivingCombatants[index].IsDeployDirector()) { allLivingCombatants.RemoveAt(index); break; }
        }
      }catch(Exception e) {
        Log.TWL(0,e.ToString());
      }
    }
  }
  [HarmonyPatch(typeof(VisibilityCache))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("RebuildCache")]
  [HarmonyPatch(new Type[] { typeof(List<ICombatant>) })]
  public static class VisibilityCache_RebuildCache {
    public static bool Prefix(VisibilityCache __instance, ref List<ICombatant> allLivingCombatants) {
      try {
        if (Traverse.Create(__instance).Property<AbstractActor>("OwningActor").Value.IsDeployDirector()) { return false; }
        for (int index = 0; index < allLivingCombatants.Count; ++index) {
          if (allLivingCombatants[index].IsDeployDirector()) { allLivingCombatants.RemoveAt(index); break; }
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(VisibilityCache))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("UpdateCacheReciprocal")]
  [HarmonyPatch(new Type[] { typeof(List<ICombatant>) })]
  public static class VisibilityCache_UpdateCacheReciprocal {
    public static bool Prefix(VisibilityCache __instance, ref List<ICombatant> allLivingCombatants) {
      try {
        if (Traverse.Create(__instance).Property<AbstractActor>("OwningActor").Value.IsDeployDirector()) { return false; }
        for (int index = 0; index < allLivingCombatants.Count; ++index) {
          if (allLivingCombatants[index].IsDeployDirector()) { allLivingCombatants.RemoveAt(index); break; }
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(UnitSpawnPointGameLogic))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("SpawnMech")]
  [HarmonyPatch(new Type[] { typeof(MechDef), typeof(PilotDef), typeof(Team), typeof(Lance), typeof(HeraldryDef) })]
  public static class UnitSpawnPointGameLogic_SpawnMech {
    public static void Postfix(UnitSpawnPointGameLogic __instance, MechDef mDef, PilotDef pilot, Team team, Lance lance, HeraldryDef customHeraldryDef, Mech __result) {
      if (__result.IsDeployDirector()) {
        __result.GameRep.gameObject.SetActive(false);
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDWeaponPanel))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("ShowWeaponsUpTo")]
  [HarmonyPatch(new Type[] { typeof(int) })]
  public static class CombatHUDWeaponPanel_ShowWeaponsUpTo {
    public static bool Prefix(CombatHUDWeaponPanel __instance) {
      if (__instance.DisplayedActor == null) { return true; }
      if (__instance.DisplayedActor.IsDeployDirector() && (__instance.DisplayedActor.IsDead == false)) {
        __instance.gameObject.SetActive(false);
        return false;
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(CombatHUDMechTray))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("Update")]
  [HarmonyPatch(new Type[] { })]
  public static class CombatHUDMechTray_UpdateDirector {
    public static bool Prefix(CombatHUDMechTray __instance) {
      if (__instance.DisplayedActor == null) { return true; }
      if (__instance.DisplayedActor.IsDeployDirector() && (__instance.DisplayedActor.IsDead == false)) {
        __instance.gameObject.SetActive(false);
        return false;
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(TurnDirector))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("OnDropshipAnimationComplete")]
  [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
  public static class TurnDirector_OnDropshipAnimationComplete {
    public static void Postfix(TurnDirector __instance, MessageCenterMessage message) {
      Mech deployDirector = null;
      foreach(AbstractActor unit in __instance.Combat.LocalPlayerTeam.units) {
        if (unit.IsDead) { continue; }
        if (unit.IsDeployDirector() == false) { continue; }
        deployDirector = unit as Mech;
        break;
      }
      if(deployDirector != null) {
        if (__instance.Combat.LocalPlayerTeam.unitCount > 1) {
          deployDirector.HasActivatedThisRound = false;
          deployDirector.HUD().SelectionHandler.DeselectActor(deployDirector);
          deployDirector.HUD().MechWarriorTray.RefreshTeam(__instance.Combat.LocalPlayerTeam);
          __instance.Combat.StackManager.DEBUG_SequenceStack.Clear();
          typeof(TurnDirector).GetMethod("QueuePilotChatter", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance, new object[] { });
          List<AbstractActor> allActors = __instance.Combat.TurnDirector.Combat.AllActors;
          List<ICombatant> livingCombatants = __instance.Combat.TurnDirector.Combat.GetAllLivingCombatants();
          int num = 1;
          AuraCache.UpdateAllAuras(allActors, num != 0);
          for (int index = 0; index < __instance.Combat.TurnDirector.NumTurnActors; ++index) {
            Team turnActor = __instance.Combat.TurnDirector.TurnActors[index] as Team;
            if (turnActor != null)
              turnActor.RebuildVisibilityCacheAllUnits(livingCombatants);
          }
          __instance.Combat.TurnDirector.Combat.AllActors[0].UpdateVisibilityCache(livingCombatants);
          __instance.Combat.TurnDirector.StartFirstRound();
          //__instance.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new DialogComplete(DialogueGameLogic.missionStartDialogueGuid));
          //EncounterLayerParent.EnqueueLoadAwareMessage((MessageCenterMessage)new DespawnActorMessage(PlayerLanceSpawnerGameLogic_OnEnterActive.deployLoadRequest.playerLanceSpawner.encounterObjectGuid, deployDirector.GUID, DeathMethod.DespawnedNoMessage));
        }
      }
    }
  }
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("DespawnActor")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
  public static class AbstractActor_DespawnActor {
    public static void Postfix(AbstractActor __instance, MessageCenterMessage message, ref string ____teamId, ref Team ____team) {
      try {
        DespawnActorMessage despawnActorMessage = message as DespawnActorMessage;
        if (despawnActorMessage == null) { return; }
        if (!(despawnActorMessage.affectedObjectGuid == __instance.GUID)) { return; }
        Log.TWL(0, "AbstractActor.DespawnActor " + __instance.DisplayName + ":" + despawnActorMessage.deathMethod);
        if (__instance.TeamId != __instance.Combat.LocalPlayerTeamGuid) { return; };
        if (__instance.IsDeployDirector() == false) { return; }
        __instance.HUD().SelectionHandler.DeselectActor(__instance);
        foreach(Team team in __instance.Combat.Teams) {
          if (team == null) { continue; }

        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(CombatSelectionHandler))]
  [HarmonyPatch("TrySelectActor")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(bool) })]
  public static class AbstractActor_TrySelectActor {
    public static void Postfix(CombatSelectionHandler __instance, AbstractActor actor, bool manualSelection) {
      if (actor == null) { return; }
      if(actor.Combat.LocalPlayerTeam.unitCount == 1) {
        if (actor.Combat.LocalPlayerTeam.units[0].IsDeployDirector()) {
          return;
        }
      }
      try {
        Mech deployDirector = null;
        foreach (AbstractActor unit in actor.Combat.LocalPlayerTeam.units) {
          if (unit.IsDead) { continue; }
          if (unit.IsDeployDirector() == false) { continue; }
          deployDirector = unit as Mech;
          break;
        }
        if (deployDirector != null) {
          Log.TWL(0, "CombatSelectionHandler.TrySelectActor phase:" + deployDirector.Combat.TurnDirector.CurrentPhase + " is avaible:" + deployDirector.IsAvailableThisPhase);
          if (deployDirector.IsAvailableThisPhase) {
            deployDirector.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AddSequenceToStackMessage(deployDirector.DoneWithActor()));
          } else {
            EncounterLayerParent.EnqueueLoadAwareMessage((MessageCenterMessage)new DespawnActorMessage(PlayerLanceSpawnerGameLogic_OnEnterActive.deployLoadRequest.playerLanceSpawner.encounterObjectGuid, deployDirector.GUID, DeathMethod.DespawnedNoMessage));
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("InitStats")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class Mech_InitStats {
    public static void Postfix(Mech __instance) {
      try {
        if (__instance.IsDeployDirector()) {
          __instance.Initiative = 1;
          __instance.StatCollection.Set<int>("BaseInitiative", 1);
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(FogOfWarSystem))]
  [HarmonyPatch("OnUnitSpawn")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
  public static class FogOfWarSystem_OnUnitSpawn {
    public static bool Prefix(FogOfWarSystem __instance, MessageCenterMessage message, List<AbstractActor> ___viewers) {
      try {
        Log.TWL(0, "FogOfWarSystem.OnUnitSpawn");
        Log.WL(1, "viewers:");
        foreach (AbstractActor viewer in ___viewers) {
          Log.WL(2, viewer.DisplayName + ":" + viewer.GUID);
        }
        ITaggedItem itemByGuid = __instance.Combat.ItemRegistry.GetItemByGUID((message as UnitSpawnedMessage).affectedObjectGuid);
        if (itemByGuid == null) { return true; }
        AbstractActor unit = itemByGuid as AbstractActor;
        if (unit == null) { return true;  };
        Log.WL(1, "unit:" + unit.DisplayName + " " + unit.GUID + " IsDeployDirector:" + unit.IsDeployDirector());
        if (unit.IsDeployDirector()) {
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
    public static void Postfix(FogOfWarSystem __instance, MessageCenterMessage message, List<AbstractActor> ___viewers, List<FogOfWarRevealatron> ___revealatrons) {
      Log.TWL(0, "FogOfWarSystem.OnUnitSpawn");
      Log.WL(1, "viewers:");
      foreach (AbstractActor viewer in ___viewers) {
        Log.WL(2, viewer.DisplayName+":"+viewer.GUID);
      }
      Log.WL(1, "revealatrons:");
      foreach (FogOfWarRevealatron revealatron in ___revealatrons) {
        Log.WL(2, revealatron.name+":"+revealatron.GUID);
      }
    }
  }
  [HarmonyPatch(typeof(FogOfWarSystem))]
  [HarmonyPatch("AddViewer")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor) })]
  public static class FogOfWarSystem_AddViewer {
    public static bool Prefix(FogOfWarSystem __instance, AbstractActor unit, List<AbstractActor> ___viewers) {
      try {
        Log.TWL(0, "FogOfWarSystem.AddViewer "+ unit.DisplayName+":"+ unit.GUID+ " IsDeployDirector:"+ unit.IsDeployDirector());
        Log.WL(1, "viewers:");
        foreach (AbstractActor viewer in ___viewers) {
          Log.WL(2, viewer.DisplayName + ":" + viewer.GUID);
        }
        if (unit.IsDeployDirector()) {
          //__instance.WipeToValue(FogOfWarState.Revealed);
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
    public static void Postfix(FogOfWarSystem __instance, List<AbstractActor> ___viewers, List<FogOfWarRevealatron> ___revealatrons) {
      Log.TWL(0, "FogOfWarSystem.AddViewer");
      Log.WL(1, "viewers:");
      foreach (AbstractActor viewer in ___viewers) {
        Log.WL(2, viewer.DisplayName + ":" + viewer.GUID);
      }
      Log.WL(1, "revealatrons:");
      foreach (FogOfWarRevealatron revealatron in ___revealatrons) {
        Log.WL(2, revealatron.name + ":" + revealatron.GUID);
      }
    }
  }
  [HarmonyPatch(typeof(CombatSelectionHandler))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("DeselectActor")]
  [HarmonyPatch(new Type[] { typeof(AbstractActor) })]
  public static class CombatSelectionHandler_DeselectActor {
    public static bool Prefix(CombatSelectionHandler __instance, AbstractActor actor) {
      if (actor == null) { return true; }
      if (actor.Combat == null) { return true; }
      if (actor.Combat.LocalPlayerTeam == null) { return true; }
      if (actor.Combat.LocalPlayerTeam.unitCount != 1) { return true; }
      if (actor.IsDeployDirector() && (actor.IsDead == false)) { return false; }
      return true;
    }
  }
  [HarmonyPatch(typeof(FogOfWarSystem))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("WipeToValue")]
  [HarmonyPatch(new Type[] { typeof(FogOfWarState) })]
  public static class FogOfWarSystem_WipeToValue {
    private static bool HideFogOfWar = false;
    public static void NormalFoW() { HideFogOfWar = false; }
    public static void RevealFoW() { HideFogOfWar = true; }
    public static bool Prefix(FogOfWarSystem __instance, ref FogOfWarState fowState) {
      if (HideFogOfWar) { fowState = FogOfWarState.Revealed; }
      return true;
    }
  }
  [HarmonyPatch(typeof(GameInstance))]
  [HarmonyPatch("LaunchContract")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Contract), typeof(string) })]
  public static class GameInstance_LaunchContract {
    public static bool SpawnDelayed = false;
    public static bool isManualSpawn(this Contract contract) { return SpawnDelayed; }
    public static void ClearManualSpawn(this Contract contract) { SpawnDelayed = false; }
    public static void Prefix(GameInstance __instance, Contract contract, string playerGUID) {
      SpawnDelayed = false;
      if (Core.Settings.DeployManual == false) { return; }
      if (contract.SimGameContract == false) { return; }
      if (contract.IsFlashpointCampaignContract) { return; }
      if (contract.IsFlashpointContract) { return; }
      if (contract.IsRestorationContract) { return; }
      if (contract.IsStoryContract) { return; }
      if (Core.Settings.DeployForbidContractTypes.Contains(contract.ContractTypeValue.Name)) { return; }
      SpawnDelayed = true;
    }
  }
  [HarmonyPatch(typeof(LanceSpawnerGameLogic))]
  [HarmonyPatch("OnEnterActive")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class LanceSpawnerGameLogic_OnEnterActive {
    public static HashSet<LanceSpawnerGameLogic> delayedSpawners = new HashSet<LanceSpawnerGameLogic>();
    public static List<Vector3> delayedEnemySpawnPositions(this Contract contract) {
      List<Vector3> result = new List<Vector3>();
      foreach(LanceSpawnerGameLogic spawner in delayedSpawners) {
        try {
          if (spawner == null) { continue; }
          Team team = spawner.Combat.TurnDirector.GetTurnActorByUniqueId(spawner.teamDefinitionGuid) as Team;
          if (team == null) { continue; }
          if (team.IsEnemy(spawner.Combat.LocalPlayerTeam) == false) { continue; }
          foreach (UnitSpawnPointGameLogic spawnPoint in spawner.unitSpawnPointGameLogicList) {
            if (spawnPoint.unitType == UnitType.UNDEFINED) { continue; }
            result.Add(spawnPoint.Position);
          }
        }catch(Exception e) {
          Log.TWL(0, e.ToString(), true);
        }
      }
      return result;
    }
    public static void SetSpawnerReady(this LanceSpawnerGameLogic spawner) {
      if (delayedSpawners.Contains(spawner)) {
        delayedSpawners.Remove(spawner);
        Log.TWL(0, "Delayed spawner ready: " + spawner.DisplayName + " objectives ready state:"+spawner.Combat.ActiveContract.isObjectivesReady());
      }
    }
    public static void ActivateDelayed(this Contract contract) {
      HashSet<LanceSpawnerGameLogic> spawners = new HashSet<LanceSpawnerGameLogic>();
      foreach (LanceSpawnerGameLogic sp in delayedSpawners) { spawners.Add(sp); }
      foreach (LanceSpawnerGameLogic sp in spawners) { sp.OnEnterActive(); }
    }
    public static bool isObjectivesReady(this Contract contract) { return (delayedSpawners.Count == 0)&&(contract.isManualSpawn() == false); }
    public static bool Prefix(LanceSpawnerGameLogic __instance) {
      if (__instance.Combat.ActiveContract == null) { return true; }
      if (__instance.Combat.ActiveContract.isManualSpawn() == false) { return true; }
      if (__instance.unitSpawnPointGameLogicList.Length == 0) { return true; }
      if (__instance.unitSpawnPointGameLogicList[0].mechDefId == CACConstants.DeployMechDefID) { return true; }
      delayedSpawners.Add(__instance);
      return false;
    }
  }
  [HarmonyPatch(typeof(LanceSpawnerGameLogic))]
  [HarmonyPatch("OnUnitSpawnComplete")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class LanceSpawnerGameLogic_OnUnitSpawnCompleteManual {
    public static void Postfix(LanceSpawnerGameLogic __instance) {
      if(__instance.Combat.ActiveContract.isManualSpawn() == false) {
        __instance.SetSpawnerReady();
      }
    }
  }
  [HarmonyPatch(typeof(ContractObjectiveGameLogic))]
  [HarmonyPatch("Update")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class ContractObjectiveGameLogic_Update {
    public static bool Prefix(ContractObjectiveGameLogic __instance) {
      if (__instance.Combat == null) { return true; }
      if (__instance.Combat.ActiveContract == null) { return true; }
      if (__instance.Combat.ActiveContract.isObjectivesReady() == false) { return false; }
      return true;
    }
  }
  [HarmonyPatch(typeof(ObjectiveGameLogic))]
  [HarmonyPatch("Update")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class ObjectiveGameLogic_Update {
    public static bool Prefix(ContractObjectiveGameLogic __instance) {
      if (__instance.Combat == null) { return true; }
      if (__instance.Combat.ActiveContract == null) { return true; }
      if (__instance.Combat.ActiveContract.isObjectivesReady() == false) { return false; }
      return true;
    }
  }
  [HarmonyPatch(typeof(ObjectiveGameLogic))]
  [HarmonyPatch("GetTaggedCombatants")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(CombatGameState), typeof(TagSet), typeof(string) })]
  public static class ObjectiveGameLogic_GetTaggedCombatants0 {
    public static void Postfix(CombatGameState combatGameState, TagSet requiredTags, string lanceGuid, ref List<ICombatant> __result) {
      for(int index = 0; index < __result.Count; ++index) {
        if (__result[index].IsDeployDirector()) { __result.RemoveAt(index); break; }
      }
    }
  }
  [HarmonyPatch(typeof(ObjectiveGameLogic))]
  [HarmonyPatch("GetTaggedCombatants")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(CombatGameState), typeof(TagSet) })]
  public static class ObjectiveGameLogic_GetTaggedCombatants1 {
    public static void Postfix(CombatGameState combatGameState, TagSet requiredTags, ref List<ICombatant> __result) {
      for (int index = 0; index < __result.Count; ++index) {
        if (__result[index].IsDeployDirector()) { __result.RemoveAt(index); break; }
      }
    }
  }

  public class DeployDirectorLoadRequest {
    public PlayerLanceSpawnerGameLogic playerLanceSpawner;
    public UnitSpawnPointGameLogic spawnPoint;
    public Vector3 originalLocalPos;
    public MechDef originalMechDef;
    public PilotDef originalPilotDef;
    public Dictionary<UnitSpawnPointGameLogic,UnitType> originalUnitTypes;
    public SpawnUnitMethodType originalSpawnMethod;
    public Mech deployDirector;
    private bool isSpawned;
    public CombatHUD HUD;
    public void RestoreAndSpawn(List<DeployPosition> positions, Mech deployDirector, CombatHUD HUD) {
      if (isSpawned) { return; }
      isSpawned = true;
      this.deployDirector = deployDirector;
      FogOfWarSystem_WipeToValue.NormalFoW();
      LazySingletonBehavior<FogOfWarView>.Instance.FowSystem.WipeToValue(HUD.Combat.EncounterLayerData.startingFogOfWarVisibility);
      this.HUD = HUD;
      foreach (var origUnitType in originalUnitTypes) {
        origUnitType.Key.unitType = origUnitType.Value;
      }
      Traverse.Create(spawnPoint).Field<PilotDef>("pilotDefOverride").Value = originalPilotDef;
      spawnPoint.pilotDefId = originalPilotDef.Description.Id;
      if (spawnPoint.unitType == UnitType.Mech) {
        Traverse.Create(spawnPoint).Field<MechDef>("mechDefOverride").Value = originalMechDef;
        spawnPoint.mechDefId = originalMechDef.Description.Id;
      } else {
        Traverse.Create(spawnPoint).Field<MechDef>("mechDefOverride").Value = null;
        spawnPoint.mechDefId = string.Empty;
      }
      spawnPoint.LocalPosition = originalLocalPos;
      //playerLanceSpawner.Position = pos;
      float centerX = 0f;
      float centerZ = 0f;
      HashSet<Vector3> deployPositions = new HashSet<Vector3>();
      Log.TWL(0, "DeployDirectorLoadRequest.RestoreAndSpawn start pos generation",true);
      for(int index = 0; index < positions.Count; ++index) {
        centerX += positions[index].position.Value.x;
        centerZ += positions[index].position.Value.z;
      }
      Vector3 pos = new Vector3(centerX / positions.Count, 0f, centerZ / positions.Count);
      pos.y = HUD.Combat.MapMetaData.GetLerpedHeightAt(pos);
      playerLanceSpawner.Position = pos;
      //for (int index = 0; index < positions.Count; ++index) {
      //  if (index > playerLanceSpawner.unitSpawnPointGameLogicList.Length) { break; }
      //  centerX += positions[index].x;
      //  centerZ += positions[index].z;
      //  playerLanceSpawner.unitSpawnPointGameLogicList[index].Position = positions[index];
      //}
      for (int index = 0; index < positions.Count; ++index) {
        positions[index].spawnPoint.Position = positions[index].position.Value;
      }
        /*foreach(UnitSpawnPointGameLogic sp in playerLanceSpawner.unitSpawnPointGameLogicList) {
          Vector3 rndPos = pos;
          float nearest = 9999f;
          do {
            rndPos = pos;
            float radius = UnityEngine.Random.Range(0.5f * Core.Settings.DeploySpawnRadius, Core.Settings.DeploySpawnRadius);
            float direction = Mathf.Deg2Rad * UnityEngine.Random.Range(0f, 360f);
            //radius *= UnityEngine.Random.Range(targetRep.parentCombatant.Combat.Constants.ResolutionConstants.MissOffsetHorizontalMin, targetRep.parentCombatant.Combat.Constants.ResolutionConstants.MissOffsetHorizontalMax);
            //Vector2 vector2 = UnityEngine.Random.insideUnitCircle.normalized * radius;
            rndPos.x += Mathf.Sin(direction) * radius;
            rndPos.z += Mathf.Cos(direction) * radius;
            rndPos.y = HUD.Combat.MapMetaData.GetLerpedHeightAt(rndPos);
            rndPos = HUD.Combat.HexGrid.GetClosestPointOnGrid(rndPos);
            nearest = 9999f;
            foreach (Vector3 depPos in deployPositions) { float dist = Vector3.Distance(depPos, rndPos); if (nearest > dist) { nearest = dist; }; };
            Log.WL(1, rndPos.ToString() + " distance:" + Vector3.Distance(rndPos, pos) + " nearest:"+nearest+" rejected:"+(nearest < Core.Settings.DeploySpawnRadius / 4f), true);
          } while (nearest < Core.Settings.DeploySpawnRadius/4f);
          deployPositions.Add(rndPos);
          sp.Position = rndPos;
        }*/
      playerLanceSpawner.spawnUnitsOnActivation = true;
      playerLanceSpawner.spawnMethod = originalSpawnMethod;
      this.playerLanceSpawner.Combat.ActiveContract.ClearManualSpawn();
      playerLanceSpawner.LanceSpawnerGameLogic_OnEnterActive();
      this.playerLanceSpawner.Combat.ActiveContract.ActivateDelayed();
    }
    public void MechDependenciesLoaded() {
      Log.TWL(0, "DeployDirectorLoadRequest.MechDependenciesLoaded");
      playerLanceSpawner.LanceSpawnerGameLogic_OnEnterActive();
    }
    public void PilotDependenciesLoaded() {
      Log.TWL(0, "DeployDirectorLoadRequest.PilotDependenciesLoaded");
      MechDef mechDef = playerLanceSpawner.Combat.DataManager.GetObjectOfType<MechDef>(CACConstants.DeployMechDefID, BattleTechResourceType.MechDef);
      if (mechDef != null) {
        Log.WL(0, "mechDef present");
        spawnPoint.mechDefId = mechDef.Description.Id;
        Traverse.Create(spawnPoint).Field<MechDef>("mechDefOverride").Value = mechDef;
        if (mechDef.DependenciesLoaded(1000u) == false) {
          Log.WL(0, "mechDef do not have all dependencies");
          DataManager.InjectedDependencyLoadRequest dependencyLoad = new DataManager.InjectedDependencyLoadRequest(playerLanceSpawner.Combat.DataManager);
          mechDef.GatherDependencies(playerLanceSpawner.Combat.DataManager, (DataManager.DependencyLoadRequest)dependencyLoad, 1000U);
          dependencyLoad.RegisterLoadCompleteCallback(new Action(this.MechDependenciesLoaded));
          playerLanceSpawner.Combat.DataManager.InjectDependencyLoader(dependencyLoad, 1000U);
          return;
        }
      } else {
        Log.WL(0, "mechDef not present");
        LoadRequest request = playerLanceSpawner.Combat.DataManager.CreateLoadRequest(null, false);
        request.AddLoadRequest<MechDef>(BattleTechResourceType.MechDef, CACConstants.DeployMechDefID, this.DirectorMechDefLoaded);
        request.ProcessRequests(1000u);
        return;
      }
      playerLanceSpawner.LanceSpawnerGameLogic_OnEnterActive();
    }
    public void DirectorPilotDefLoaded(string id, PilotDef pilotDef) {
      Log.TWL(0, "DeployDirectorLoadRequest.DirectorPilotDefLoaded "+id);
      spawnPoint.pilotDefId = pilotDef.Description.Id;
      Traverse.Create(spawnPoint).Field<PilotDef>("pilotDefOverride").Value = pilotDef;
      if (pilotDef.DependenciesLoaded(1000u) == false) {
        Log.WL(0, "not all dependencies loaded. Injecting loading");
        DataManager.InjectedDependencyLoadRequest dependencyLoad = new DataManager.InjectedDependencyLoadRequest(playerLanceSpawner.Combat.DataManager);
        pilotDef.GatherDependencies(playerLanceSpawner.Combat.DataManager, (DataManager.DependencyLoadRequest)dependencyLoad, 1000U);
        dependencyLoad.RegisterLoadCompleteCallback(new Action(this.PilotDependenciesLoaded));
        playerLanceSpawner.Combat.DataManager.InjectDependencyLoader(dependencyLoad, 1000U);
        return;
      }
      MechDef mechDef = playerLanceSpawner.Combat.DataManager.GetObjectOfType<MechDef>(CACConstants.DeployMechDefID, BattleTechResourceType.MechDef);
      if (mechDef != null) {
        Log.WL(0, "mechDef present");
        spawnPoint.mechDefId = mechDef.Description.Id;
        Traverse.Create(spawnPoint).Field<MechDef>("mechDefOverride").Value = mechDef;
        if (mechDef.DependenciesLoaded(1000u) == false) {
          Log.WL(0, "mechDef do not have all dependencies");
          DataManager.InjectedDependencyLoadRequest dependencyLoad = new DataManager.InjectedDependencyLoadRequest(playerLanceSpawner.Combat.DataManager);
          mechDef.GatherDependencies(playerLanceSpawner.Combat.DataManager, (DataManager.DependencyLoadRequest)dependencyLoad, 1000U);
          dependencyLoad.RegisterLoadCompleteCallback(new Action(this.MechDependenciesLoaded));
          playerLanceSpawner.Combat.DataManager.InjectDependencyLoader(dependencyLoad, 1000U);
          return;
        }
      } else {
        Log.WL(0, "mechDef not present");
        LoadRequest request = playerLanceSpawner.Combat.DataManager.CreateLoadRequest(null, false);
        request.AddLoadRequest<MechDef>(BattleTechResourceType.MechDef, CACConstants.DeployMechDefID, this.DirectorMechDefLoaded);
        request.ProcessRequests(1000u);
        return;
      }
      playerLanceSpawner.LanceSpawnerGameLogic_OnEnterActive();
    }
    public void DirectorMechDefLoaded(string id, MechDef def) {
      Log.TWL(0, "DeployDirectorLoadRequest.DirectorMechDefLoaded " + id);
      Traverse.Create(spawnPoint).Field<MechDef>("mechDefOverride").Value = def;
      if(def.DependenciesLoaded(1000u) == false) {
        Log.WL(0, "not all dependencies loaded. Injecting loading");
        DataManager.InjectedDependencyLoadRequest dependencyLoad = new DataManager.InjectedDependencyLoadRequest(playerLanceSpawner.Combat.DataManager);
        def.GatherDependencies(playerLanceSpawner.Combat.DataManager, (DataManager.DependencyLoadRequest)dependencyLoad, 1000U);
        dependencyLoad.RegisterLoadCompleteCallback(new Action(this.MechDependenciesLoaded));
        playerLanceSpawner.Combat.DataManager.InjectDependencyLoader(dependencyLoad, 1000U);
      } else {
        Log.WL(0, "dependencies loaded. Processing.");
        playerLanceSpawner.LanceSpawnerGameLogic_OnEnterActive();
      }      
    }
    public DeployDirectorLoadRequest(PlayerLanceSpawnerGameLogic playerLanceSpawner) {
      Log.TWL(0, "DeployDirectorLoadRequest.DeployDirectorLoadRequest");
      isSpawned = false;
      FogOfWarSystem_WipeToValue.RevealFoW();
      UnitSpawnPointGameLogic[] pointGameLogicList = playerLanceSpawner.unitSpawnPointGameLogicList;
      this.playerLanceSpawner = playerLanceSpawner;
      this.spawnPoint = pointGameLogicList[0];
      this.originalSpawnMethod = playerLanceSpawner.spawnMethod;
      this.originalUnitTypes = new Dictionary<UnitSpawnPointGameLogic, UnitType>();
      this.originalLocalPos = this.spawnPoint.LocalPosition;
      //this.spawnPoint.Position = Vector3.zero;
      for (int index = 0; index < pointGameLogicList.Length; ++index) {
        originalUnitTypes.Add(pointGameLogicList[index], pointGameLogicList[index].unitType);
        pointGameLogicList[index].unitType = UnitType.UNDEFINED;
      }
      this.originalMechDef = Traverse.Create(spawnPoint).Field<MechDef>("mechDefOverride").Value;
      this.originalPilotDef = Traverse.Create(spawnPoint).Field<PilotDef>("pilotDefOverride").Value;
      playerLanceSpawner.spawnMethod = SpawnUnitMethodType.InstantlyAtSpawnPoint;
      spawnPoint.unitType = UnitType.Mech;
      PilotDef pilotDef = playerLanceSpawner.Combat.DataManager.GetObjectOfType<PilotDef>(PlayerLanceSpawnerGameLogic_OnEnterActive.DeployPilotDefID, BattleTechResourceType.PilotDef);
      if(pilotDef != null) {
        Log.WL(0, "pilotDef present");
        spawnPoint.pilotDefId = pilotDef.Description.Id;
        Traverse.Create(spawnPoint).Field<PilotDef>("pilotDefOverride").Value = pilotDef;
        if (pilotDef.DependenciesLoaded(1000u) == false) {
          Log.WL(0, "not all dependencies loaded. Injecting loading");
          DataManager.InjectedDependencyLoadRequest dependencyLoad = new DataManager.InjectedDependencyLoadRequest(playerLanceSpawner.Combat.DataManager);
          pilotDef.GatherDependencies(playerLanceSpawner.Combat.DataManager, (DataManager.DependencyLoadRequest)dependencyLoad, 1000U);
          dependencyLoad.RegisterLoadCompleteCallback(new Action(this.PilotDependenciesLoaded));
          playerLanceSpawner.Combat.DataManager.InjectDependencyLoader(dependencyLoad, 1000U);
          return;
        }
      } else {
        Log.WL(0, "pilotDef not present");
        LoadRequest request = playerLanceSpawner.Combat.DataManager.CreateLoadRequest(null, false);
        request.AddLoadRequest<PilotDef>(BattleTechResourceType.PilotDef, PlayerLanceSpawnerGameLogic_OnEnterActive.DeployPilotDefID, this.DirectorPilotDefLoaded);
        request.ProcessRequests(1000u);
        return;
      }
      MechDef mechDef = playerLanceSpawner.Combat.DataManager.GetObjectOfType<MechDef>(CACConstants.DeployMechDefID, BattleTechResourceType.MechDef);
      if(mechDef != null) {
        Log.WL(0, "mechDef present");
        spawnPoint.mechDefId = mechDef.Description.Id;
        Traverse.Create(spawnPoint).Field<MechDef>("mechDefOverride").Value = mechDef;
        if (mechDef.DependenciesLoaded(1000u) == false) {
          Log.WL(0, "not all dependencies loaded. Injecting loading");
          DataManager.InjectedDependencyLoadRequest dependencyLoad = new DataManager.InjectedDependencyLoadRequest(playerLanceSpawner.Combat.DataManager);
          mechDef.GatherDependencies(playerLanceSpawner.Combat.DataManager, (DataManager.DependencyLoadRequest)dependencyLoad, 1000U);
          dependencyLoad.RegisterLoadCompleteCallback(new Action(this.MechDependenciesLoaded));
          playerLanceSpawner.Combat.DataManager.InjectDependencyLoader(dependencyLoad, 1000U);
          return;
        }
      } else {
        Log.WL(0, "mechDef not present");
        LoadRequest request = playerLanceSpawner.Combat.DataManager.CreateLoadRequest(null, false);
        request.AddLoadRequest<MechDef>(BattleTechResourceType.MechDef, CACConstants.DeployMechDefID, this.DirectorMechDefLoaded);
        request.ProcessRequests(1000u);
        return;
      }
      Log.WL(0, "LanceSpawnerGameLogic_OnEnterActive()");
      playerLanceSpawner.LanceSpawnerGameLogic_OnEnterActive();
    }
  }
  [HarmonyPatch(typeof(PlayerLanceSpawnerGameLogic))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("OnEnterActive")]
  [HarmonyPatch(new Type[] { })]
  public static class PlayerLanceSpawnerGameLogic_OnEnterActive {
    public delegate void d_LanceSpawnerGameLogic_OnEnterActive(LanceSpawnerGameLogic spawner);
    private static d_LanceSpawnerGameLogic_OnEnterActive i_LanceSpawnerGameLogic_OnEnterActive = null;
    //public static readonly string DeployMechDefID = "mechdef_deploy_director";
    public static readonly string DeployPilotDefID = "pilot_deploy_director";
    public static readonly string DeployAbilityDefID = "AbilityDefCU_DeploySetPosition";
    public static DeployDirectorLoadRequest deployLoadRequest = null;
    //public static void Clear() { deployLoadRequest = null; }
    public static void ResetDeployButton(this CombatHUDMechwarriorTray tray, AbstractActor actor, Ability ability, CombatHUDActionButton button, bool forceInactive) {
      CustomAmmoCategoriesLog.Log.LogWrite("ResetDeployButton:" + actor.DisplayName + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite(" actor.HasActivatedThisRound:" + actor.HasActivatedThisRound + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite(" actor.MovingToPosition:" + (actor.MovingToPosition != null) + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite(" actor.Combat.StackManager.IsAnyOrderActive:" + actor.Combat.StackManager.IsAnyOrderActive + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite(" actor.Combat.TurnDirector.IsInterleaved:" + actor.Combat.TurnDirector.IsInterleaved + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite(" forceInactive:" + forceInactive + "\n");
      tray.ResetAbilityButton(actor, button, ability, forceInactive);
      if (forceInactive) { button.DisableButton(); };
      if (actor.Combat.TurnDirector.IsInterleaved == false) {
        if (actor.HasFiredThisRound == false) {
          if (ability.IsActive == false) {
            if (ability.IsAvailable == true) {
              if (actor.IsShutDown == false) {
                CustomAmmoCategoriesLog.Log.LogWrite(" ResetButtonIfNotActive:\n");
                CustomAmmoCategoriesLog.Log.LogWrite(" IsAbilityActivated:" + button.IsAbilityActivated + "\n");
                if (actor.MovingToPosition == null) { button.ResetButtonIfNotActive(actor); };
              }
            }
          }
        } else {
          button.DisableButton();
        }
      }
    }
    public static bool Prepare() {
      Log.TWL(0, "PlayerLanceSpawnerGameLogic.OnEnterActive.Prepare");
      {
        MethodInfo OnEnterActive = typeof(LanceSpawnerGameLogic).GetMethod("OnEnterActive", BindingFlags.Instance | BindingFlags.Public);
        var dm = new DynamicMethod("CACOnEnterActive", null, new Type[] { typeof(LanceSpawnerGameLogic) }, typeof(LanceSpawnerGameLogic));
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Call, OnEnterActive);
        gen.Emit(OpCodes.Ret);
        i_LanceSpawnerGameLogic_OnEnterActive = (d_LanceSpawnerGameLogic_OnEnterActive)dm.CreateDelegate(typeof(d_LanceSpawnerGameLogic_OnEnterActive));
      }
      return true;
    }
    public static void LanceSpawnerGameLogic_OnEnterActive(this LanceSpawnerGameLogic spawner) {
      i_LanceSpawnerGameLogic_OnEnterActive(spawner);
    }
    public static bool Prefix(PlayerLanceSpawnerGameLogic __instance) {
      Log.TWL(0, "PlayerLanceSpawnerGameLogic.OnEnterActive");
      if (__instance.teamDefinitionGuid != __instance.Combat.LocalPlayerTeamGuid) { return true; }
      if (__instance.Combat.ActiveContract.isManualSpawn() == false) { return true; }
      deployLoadRequest = new DeployDirectorLoadRequest(__instance);
      return false;
      //return true;
    }
  }
}