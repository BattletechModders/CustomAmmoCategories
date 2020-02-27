using BattleTech;
using BattleTech.Data;
using BattleTech.UI;
using CustAmmoCategories;
using CustomAmmoCategoriesLog;
using CustomAmmoCategoriesPatches;
using Harmony;
using InControl;
using Localize;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace CustAmmoCategories {
  public class SelectionStateCommandAttackGround : SelectionStateCommandTargetSinglePoint {
    private bool HasActivated = false;
    public float CircleRange = 10f;
    public SelectionStateCommandAttackGround(CombatGameState Combat, CombatHUD HUD, CombatHUDActionButton FromButton, AbstractActor actor) : base(Combat, HUD, FromButton) {
      this.SelectedActor = actor;
      HasActivated = false;
    }
    public override void OnAddToStack() {
      base.OnAddToStack();
      CameraControl.Instance.SetTargets(this.AllPossibleTargets);
      if (!((UnityEngine.Object)this.SelectedActor.GameRep != (UnityEngine.Object)null))
        return;
      MechRepresentation gameRep = this.SelectedActor.GameRep as MechRepresentation;
      if (!((UnityEngine.Object)gameRep != (UnityEngine.Object)null))
        return;
      Log.LogWrite("SelectionStateCommandAttackGround.OnAddToStack ToggleRandomIdles false\n");
      gameRep.ToggleRandomIdles(false);
    }

    public override void OnInactivate() {
      Log.M.TWL(0,"SelectionStateCommandAttackGround.OnInactivate HasActivated: "+ HasActivated);
      base.OnInactivate();
      CameraControl.Instance.ClearTargets();
      if (this.SelectedActor.GameRep == null) {
        Log.M.WL(1, "GameRep is null");
        return;
      }
      MechRepresentation gameRep = this.SelectedActor.GameRep as MechRepresentation;
      if (gameRep == null) {
        Log.M.WL(1, "not a mech");
        return;
      }
      Log.M.WL(1,"ToggleRandomIdles "+!HasActivated);
      if (HasActivated) {
        gameRep.ToggleRandomIdles(false);
        gameRep.FacePoint(true, this.targetPosition, false, 0.5f, -1, -1, false, (GameRepresentation.RotationCompleteDelegate)null);
      } else {
        gameRep.ReturnToNeutralFacing(true, 0.5f, -1, -1, null);
        gameRep.ToggleRandomIdles(true);
      }
    }
    public override bool ProcessPressedButton(string button) {
      this.HasActivated = button == "BTN_FireConfirm";
      return base.ProcessPressedButton(button);
    }
    public override bool ProcessLeftClick(Vector3 worldPos) {
      if (this.NumPositionsLocked != 0)
        return false;
      this.targetPosition = worldPos;// this.GetValidTargetPosition(worldPos);
      //this.GetTargetedActors(this.targetPosition, this.FromButton.Ability.Def.FloatParam1);
      //this.SetTargetHighlights(true);
      ++this.NumPositionsLocked;
      this.ShowFireButton(CombatHUDFireButton.FireMode.Confirm, Ability.ProcessDetailString(this.FromButton.Ability).ToString(true));
      if ((UnityEngine.Object)this.SelectedActor.GameRep != (UnityEngine.Object)null) {
        MechRepresentation gameRep = this.SelectedActor.GameRep as MechRepresentation;
        if ((UnityEngine.Object)gameRep != (UnityEngine.Object)null) {
          gameRep.ToggleRandomIdles(false);
          this.SelectedActor.GameRep.FacePoint(true, this.targetPosition, false, 0.5f, -1, -1, false, (GameRepresentation.RotationCompleteDelegate)null);
        }
      }
      return true;
    }

    public override void ProcessMousePos(Vector3 worldPos) {
      float range = this.UpdateWeapons(worldPos);
      if (float.IsNaN(range) == false) { this.CircleRange = range; };
      switch (this.NumPositionsLocked) {
        case 0:
#if BT1_8
          CombatTargetingReticle.Instance.UpdateReticle(worldPos, CircleRange, false);
#else
          CombatTargetingReticle.Instance.UpdateReticle(worldPos, CircleRange);
#endif
          break;
        case 1:
#if BT1_8
          CombatTargetingReticle.Instance.UpdateReticle(this.targetPosition, CircleRange, false);
#else
          CombatTargetingReticle.Instance.UpdateReticle(this.targetPosition, CircleRange);
#endif
          break;
      }      
    }
    public override int ProjectedHeatForState {
      get {
        Mech selectedActor = this.SelectedActor as Mech;
        if (selectedActor != null) {
          int num = 0;
          for (int index = 0; index < selectedActor.Weapons.Count; ++index) {
            if (selectedActor.Weapons[index].IsEnabled && selectedActor.Weapons[index].WillFire)
              num += (int)selectedActor.Weapons[index].HeatGenerated;
          }
          return num;
        }
        return 0;
      }
    }
  };
  public class TerrainHitInfo {
    public Vector3 pos;
    public bool indirect;
    public TerrainHitInfo(Vector3 pos, bool indirect) {
      this.pos = pos;
      this.indirect = indirect;
    }
  }
  public static partial class CustomAmmoCategories {
    public static Dictionary<string, TerrainHitInfo> terrainHitPositions = new Dictionary<string, TerrainHitInfo>();
    public static void addTerrainHitPosition(this AbstractActor unit, Vector3 pos, bool indirect) {
      SpawnVehicleDialogHelper.lastTerrainHitPosition = pos;
      if (CustomAmmoCategories.terrainHitPositions.ContainsKey(unit.GUID) == false) {
        CustomAmmoCategories.terrainHitPositions.Add(unit.GUID, new TerrainHitInfo(pos, indirect));
      } else {
        CustomAmmoCategories.terrainHitPositions[unit.GUID] = new TerrainHitInfo(pos, indirect);
      }
    }
    public static TerrainHitInfo getTerrinHitPosition(string GUID) {
      if (CustomAmmoCategories.terrainHitPositions.ContainsKey(GUID) == false) {
        return null;
      }
      return CustomAmmoCategories.terrainHitPositions[GUID];
    }
  }
}

namespace CustomAmmoCategoriesPatches {
  /*[HarmonyPatch(typeof(SelectionStateCommandTargetSinglePoint))]
  [HarmonyPatch("GetValidTargetPosition")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Vector3) })]
  public static class SelectionStateCommandTargetSinglePoint_GetValidTargetPosition {
    public static bool SelectionForbidden = false;
    public static bool Prefix(SelectionStateCommandTargetSinglePoint __instance, Vector3 worldPos, Vector3 __result) {
      Log.LogWrite("SelectionStateCommandTargetSinglePoint.GetValidTargetPosition: "+ worldPos + "\n");
      SelectionStateCommandAttackGround sscAG = __instance as SelectionStateCommandAttackGround;
      if (sscAG == null) { return true; }
      __result = worldPos;
      return false;
    }
  }*/
  [HarmonyPatch(typeof(PilotableActorRepresentation))]
  [HarmonyPatch("FaceTarget")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(bool), typeof(ICombatant), typeof(float), typeof(int), typeof(int), typeof(bool), typeof(GameRepresentation.RotationCompleteDelegate) })]
  public static class PilotableActorRepresentation_ReturnToNeutralFacing {
    public static bool Prefix(PilotableActorRepresentation __instance, bool isParellelSequence, ICombatant target, float twistTime, int stackItemUID, int sequenceId, bool isMelee, GameRepresentation.RotationCompleteDelegate completeDelegate) {
      try {
        if (__instance.parentCombatant == null) { return false; }
        if (target == null) { return false; }
        Log.M.TWL(0, "PilotableActorRepresentation.FaceTarget " + new Text(__instance.parentCombatant.DisplayName).ToString() + "->" + new Text(target.DisplayName).ToString());
        if (target.GUID != __instance.parentCombatant.GUID) {
          return true;
        }
        TerrainHitInfo terrainPos = CustomAmmoCategories.getTerrinHitPosition(__instance.parentCombatant.GUID);
        if (terrainPos != null) {
          Log.M.WL(1, "terrain attack detected " + terrainPos.pos);
          __instance.FacePoint(isParellelSequence, terrainPos.pos, false, twistTime, stackItemUID, sequenceId, isMelee, completeDelegate);
          return false;
        }
      }catch(Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(SelectionState))]
  [HarmonyPatch("CanDeselect")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class SelectionState_CanDeselect {
    public static bool Prefix(SelectionState __instance, ref bool __result) {
      CombatGameState Combat = (CombatGameState)typeof(SelectionState).GetProperty("Combat", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance, null);
      CustomAmmoCategoriesLog.Log.LogWrite("SelectionState.CanDeselect\n");
      CustomAmmoCategoriesLog.Log.LogWrite(" Combat.TurnDirector.IsInterleaved = " + Combat.TurnDirector.IsInterleaved + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite(" SelectedActor.HasBegunActivation = " + __instance.SelectedActor.HasBegunActivation + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite(" SelectedActor.StoodUpThisRound = " + __instance.SelectedActor.StoodUpThisRound + "\n");
      //if (!Combat.TurnDirector.IsInterleaved) {
      //  __result = !__instance.SelectedActor.StoodUpThisRound;
      //  return false;
      //}
      if (!__instance.SelectedActor.HasBegunActivation) {
        __result = !__instance.SelectedActor.StoodUpThisRound;
        return false;
      }
      __result = false;
      return false;
    }
  }
  [HarmonyPatch(typeof(SelectionStateMoveBase))]
  [HarmonyPatch("CreateMoveOrders")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(bool) })]
  public static class SelectionStateMoveBase_CreateMoveOrders {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(TurnDirector), "IsInterleaved").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(SelectionStateMoveBase_CreateMoveOrders), nameof(IsInterleaved));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }

    private static bool IsInterleaved(TurnDirector director) {
      CustomAmmoCategoriesLog.Log.LogWrite("SelectionStateMoveBase.CreateMoveOrders.IsInterleaved\n");
      return true;
    }
  }
  [HarmonyPatch(typeof(AbstractActorMovementInvocation))]
  [HarmonyPatch("Invoke")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(CombatGameState) })]
  public static class AbstractActorMovementInvocation_Invoke {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertySetter = AccessTools.Property(typeof(AbstractActor), "AutoBrace").GetSetMethod();
      var replacementMethod = AccessTools.Method(typeof(AbstractActorMovementInvocation_Invoke), nameof(AutoBrace));
      return Transpilers.MethodReplacer(instructions, targetPropertySetter, replacementMethod);
    }

    private static void AutoBrace(AbstractActor actor, bool value) {
      CustomAmmoCategoriesLog.Log.LogWrite("AbstractActorMovementInvocation.Invoke.AutoBrace\n");
      actor.AutoBrace = false;
    }
  }
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("ResolveAttackSequence")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string), typeof(int), typeof(int), typeof(AttackDirection) })]
  public static class AbstractActor_ResolveAttackSequence {
    public static bool Prefix(AbstractActor __instance, string sourceID, int sequenceID, int stackItemID, AttackDirection attackDirection) {
      AttackDirector.AttackSequence attackSequence = __instance.Combat.AttackDirector.GetAttackSequence(sequenceID);
      if (attackSequence == null) { return true; }
      if (attackSequence.attacker.GUID == attackSequence.chosenTarget.GUID) {
        Log.LogWrite("this is terrain attack, no evasive damage or effects\n");
        return false;
      };
      return true;
    }
  }
  [HarmonyPatch(typeof(SelectionState))]
  [HarmonyPatch("GetNewSelectionStateByType")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(SelectionType), typeof(CombatGameState), typeof(CombatHUD), typeof(CombatHUDActionButton), typeof(AbstractActor) })]
  public static class SelectionState_GetNewSelectionStateByType {
    public static bool SelectionForbidden = false;
    public static bool Prefix(SelectionType type, CombatGameState Combat, CombatHUD HUD, CombatHUDActionButton FromButton, AbstractActor actor, ref SelectionState __result) {
      CustomAmmoCategoriesLog.Log.LogWrite("SelectionState.GetNewSelectionStateByType " + type + ":" + FromButton.GUID + "\n");
      if ((type == SelectionType.CommandTargetSinglePoint) && (FromButton.GUID == "ID_ATTACKGROUND")) {
        CustomAmmoCategoriesLog.Log.LogWrite(" creating own selection state\n");
        __result = new SelectionStateCommandAttackGround(Combat, HUD, FromButton, actor);
        return false;
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("HasLOFToTargetUnitAtTargetPosition")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ICombatant), typeof(float), typeof(Vector3), typeof(Quaternion), typeof(Vector3), typeof(Quaternion), typeof(bool) })]
  public static class AbstractActor_HasLOFToTargetUnitAtTargetPosition {
    public static bool SelectionForbidden = false;
    public static bool Prefix(AbstractActor __instance, ICombatant targetUnit, float maxRange, Vector3 attackPosition, Quaternion attackRotation, Vector3 targetPosition, Quaternion targetRotation, bool isIndirectFireCapable, ref bool __state) {
      if(__instance.GUID == targetUnit.GUID) {
        __state = __instance.StatCollection.GetValue<bool>("IsIndirectImmune");
        __instance.StatCollection.Set<bool>("IsIndirectImmune", false);
      }
      return true;
    }
    public static void Postfix(AbstractActor __instance, ICombatant targetUnit, float maxRange, Vector3 attackPosition, Quaternion attackRotation, Vector3 targetPosition, Quaternion targetRotation, bool isIndirectFireCapable, ref bool __state) {
      if (__instance.GUID == targetUnit.GUID) {
        __instance.StatCollection.Set<bool>("IsIndirectImmune", __state);
      }
    }
  }
  [HarmonyPatch(typeof(CombatSelectionHandler))]
  [HarmonyPatch("TrySelectActor")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(bool) })]
  public static class CombatSelectionHandler_TrySelectActor {
    public static bool SelectionForbidden = false;
    public static bool Prefix(CombatSelectionHandler __instance, AbstractActor actor, bool manualSelection) {
      if (CombatSelectionHandler_TrySelectActor.SelectionForbidden) { return false; }
      return true;
    }
  }
  [HarmonyPatch(typeof(CombatHUD))]
  [HarmonyPatch("OnAttackEnd")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
  public static class CombatHUD_OnAttackEnd {
    public static AbstractActor needSelect = null;
    public static void Postfix(CombatHUD __instance, MessageCenterMessage message) {
      Log.LogWrite("CombatHUD.OnAttackEnd. SelectionForbidden: "+ CombatSelectionHandler_TrySelectActor.SelectionForbidden + "\n");
      if (CombatSelectionHandler_TrySelectActor.SelectionForbidden == true) {
        AttackDirector.AttackSequence attackSequence = __instance.Combat.AttackDirector.GetAttackSequence((message as AttackSequenceEndMessage).sequenceId);
        AbstractActor attacker = null;
        if (attackSequence == null) {
          Log.LogWrite("Can't find sequence with id "+ (message as AttackSequenceEndMessage).sequenceId + "\n");
          attacker = CombatHUD_OnAttackEnd.needSelect;
        } else {
          attacker = attackSequence.attacker;
        }
        //Log.LogWrite("Terrain attack end.\n");
        CombatSelectionHandler_TrySelectActor.SelectionForbidden = false;
        if (attacker != null) {
          Log.LogWrite(" attacker " + attacker.DisplayName + ":" + attacker.GUID + ".\n");
          MechRepresentation gameRep = attacker.GameRep as MechRepresentation;
          if (gameRep != null) {
            Log.LogWrite("CombatHUD.OnAttackEnd. ToggleRandomIdles true\n");
            gameRep.ReturnToNeutralFacing(true, 0.5f, -1, -1, null);
            gameRep.ToggleRandomIdles(true);
          }
          if (attacker.HasMovedThisRound || ((attacker.Combat.TurnDirector.IsInterleaved == true) && (attacker.CanMoveAfterShooting == false))) {
            Log.LogWrite(" no need to select. already done with it\n");
          } else {
            Log.LogWrite(" try to select\n");
            bool HasBegunActivation = attacker.HasBegunActivation;
            bool HasActivatedThisRound = attacker.HasActivatedThisRound;
            attacker.HasBegunActivation = false;
            attacker.HasActivatedThisRound = false;
            MethodInfo OnActorSelected = typeof(CombatHUD).GetMethod("OnActorSelected", BindingFlags.NonPublic | BindingFlags.Instance);
            if(OnActorSelected != null) {
              Log.M.TWL(0, "CombatHUD.OnActorSelected is null");
            } else {
              OnActorSelected.Invoke(__instance, new object[] { attacker });
            }
            __instance.SelectionHandler.TrySelectActor(attacker, false);
            attacker.HasBegunActivation = HasBegunActivation;
            attacker.HasActivatedThisRound = HasActivatedThisRound;
            __instance.MechWarriorTray.ConfirmAbilities(AbilityDef.ActivationTiming.ConsumedByFiring);
            attacker = null;
          }
        }
        //__instance.Combat.AttackDirector.RemoveAttackSequence(attackSequence.id);

      }
    }
  }
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("HasIndirectLOFToTargetUnit")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Vector3), typeof(Quaternion), typeof(ICombatant), typeof(bool) })]
  public static class AbstractActor_HasIndirectLOFToTargetUnitTerrainAttack {
    private static bool Prefix(AbstractActor __instance, Vector3 attackPosition, Quaternion attackRotation, ICombatant targetUnit, bool enabledWeaponsOnly, bool __result) {
      Log.M.TWL(0, "AbstractActor.HasIndirectLOFToTargetUnit "+new Text(__instance.DisplayName).ToString()+"->"+ new Text(targetUnit.DisplayName).ToString());
      if (__instance.GUID != targetUnit.GUID) {
        Log.M.WL(1,"not terrain attack");
        return true;
      }
      TerrainHitInfo terrainPos = CustomAmmoCategories.getTerrinHitPosition(__instance.GUID);
      if (terrainPos == null) {
        Log.M.WL(1, "terrain attack info not found");
        return true;
      }
      __result = terrainPos.indirect;
      Log.M.WL(1, "terrain attack indirect:"+__result);
      return false;
    }
  }
  [HarmonyPatch(typeof(AttackDirector))]
  [HarmonyPatch("OnAttackComplete")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPriority(Priority.Last)]
  [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
  public static class AttackDirector_OnAttackCompleteTA {
    public static bool Prefix(AttackDirector __instance, MessageCenterMessage message, ref AbstractActor __state) {
      __state = null;
      AttackCompleteMessage attackCompleteMessage = (AttackCompleteMessage)message;
      Log.M.TWL(0,"AttackDirector.OnAttackCompleteTA:" + attackCompleteMessage.sequenceId + "\n");
      int sequenceId = attackCompleteMessage.sequenceId;
      AttackDirector.AttackSequence attackSequence = __instance.GetAttackSequence(sequenceId);
      if (attackSequence == null) {
        return true;
      }
      if (attackSequence.chosenTarget.GUID == attackSequence.attacker.GUID) {
        Log.LogWrite(" terrain attack detected\n");
        __state = attackSequence.attacker;
        Mech mech = __state as Mech;
        if (mech != null) {
          mech.GenerateAndPublishHeatSequence(attackSequence.id, true, false, mech.GUID);
        };
      };
      return true;
    }
    public static void Postfix(AttackDirector __instance, MessageCenterMessage message, ref AbstractActor __state) {
      if (__state != null) {
        MechRepresentation gameRep = __state.GameRep as MechRepresentation;
        if (!((UnityEngine.Object)gameRep != (UnityEngine.Object)null)) {
          Log.LogWrite("ToggleRandomIdles true\n");
          gameRep.ToggleRandomIdles(true);
        }
        __state.HasFiredThisRound = true;
        if (__state.HasMovedThisRound || (__state.team.IsLocalPlayer == false) || ((__state.Combat.TurnDirector.IsInterleaved == true) && (__state.CanMoveAfterShooting == false))) {
          //__instance.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AddSequenceToStackMessage(attackSequence.attacker.DoneWithActor()));
          Log.LogWrite(" done with:" + __state.DisplayName + ":" + __state.GUID + "\n");
          __instance.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AddSequenceToStackMessage(__state.DoneWithActor()));
          CombatHUD_OnAttackEnd.needSelect = null;
        } else {
          CombatHUD_OnAttackEnd.needSelect = __state;
        }
        JammingEnabler.jammAMS();
        JammingEnabler.jamm(__state);
        SpawnVehicleDialogHelper.SpawnSelected(-1);
      }
    }
  }
  public static class WeaponEffect_FireTerrain {
    public static bool Prefix(WeaponEffect __instance, WeaponHitInfo hitInfo, int hitIndex, int emitterIndex) {
      if (hitInfo.attackerId == hitInfo.targetId) {
        CustomAmmoCategoriesLog.Log.LogWrite("On Fire detected terrain attack\n");
        typeof(WeaponEffect).GetField("t", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (object)0.0f);
        __instance.HitIndex(hitIndex);
        typeof(WeaponEffect).GetField("emitterIndex", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (object)emitterIndex);
        __instance.hitInfo = hitInfo;
        typeof(WeaponEffect).GetField("startingTransform", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (object)__instance.weaponRep.vfxTransforms[emitterIndex]);
        Transform startingTransform = (Transform)typeof(WeaponEffect).GetField("startingTransform", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
        typeof(WeaponEffect).GetField("startPos", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (object)startingTransform.position);
        typeof(WeaponEffect).GetField("endPos", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (object)hitInfo.hitPositions[hitIndex]);
        Vector3 startPos = (Vector3)typeof(WeaponEffect).GetField("startPos", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
        typeof(WeaponEffect).GetField("currentPos", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (object)startPos);
        PropertyInfo property = typeof(WeaponEffect).GetProperty("FiringComplete");
        property.DeclaringType.GetProperty("FiringComplete");
        property.GetSetMethod(true).Invoke(__instance, new object[1] { (object)false });
        __instance.InitProjectile();
        __instance.currentState = WeaponEffect.WeaponEffectState.PreFiring;
        return false;
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(SelectionStateCommandTargetSinglePoint))]
  [HarmonyPatch("ProcessMousePos")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Vector3) })]
  public static class SelectionStateCommandTargetSinglePoint_ProcessMousePos {
    private static PropertyInfo pHUD = null;
    private static Vector3 prevPosition = Vector3.zero;
    private static PropertyInfo pLookAndColorConstants = null;
    private static PropertyInfo pNumPositionsLocked = null;
    private static PropertyInfo pTargetPosition = null;
    private static MethodInfo mShowTextColor = null;
#if BT1_8
    private static MethodInfo mGetWeaponCOILStateColor = null;
#endif
    private static MethodInfo mGetShotQualityTextColor = null;
    private static FieldInfo fCombat = null;
    private static float timeCounter = 0f;
    public static bool Prepare() {
      try {
        pHUD = typeof(SelectionState).GetProperty("HUD", BindingFlags.Instance | BindingFlags.NonPublic);
        if(pHUD == null) {
          Log.LogWrite("Can't find SelectionState.HUD\n");
          return false;
        }
        pNumPositionsLocked = typeof(SelectionStateCommandTargetSinglePoint).GetProperty("NumPositionsLocked", BindingFlags.Instance | BindingFlags.NonPublic);
        if (pNumPositionsLocked == null) {
          Log.LogWrite("Can't find SelectionState.NumPositionsLocked\n");
          return false;
        }
        pLookAndColorConstants = typeof(CombatHUDWeaponSlot).GetProperty("LookAndColorConstants", BindingFlags.Instance | BindingFlags.NonPublic);
        if (pLookAndColorConstants == null) {
          Log.LogWrite("Can't find CombatHUDWeaponSlot.LookAndColorConstants\n");
          return false;
        }
        pTargetPosition = typeof(SelectionStateCommandTargetSinglePoint).GetProperty("targetPosition", BindingFlags.Instance | BindingFlags.NonPublic);
        if (pTargetPosition == null) {
          Log.LogWrite("Can't find CombatHUDWeaponSlot.targetPosition\n");
          return false;
        }
#if BT1_8
        mShowTextColor = typeof(CombatHUDWeaponSlot).GetMethod("ShowTextColor", BindingFlags.Instance | BindingFlags.NonPublic);
#else
        mShowTextColor = typeof(CombatHUDWeaponSlot).GetMethod("ShowTextColor", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[3] { typeof(Color), typeof(Color), typeof(bool) }, null);
#endif
        if (mShowTextColor == null) {
          Log.LogWrite("Can't find CombatHUDWeaponSlot.ShowTextColor\n");
          return false;
        }
#if BT1_8
        mGetWeaponCOILStateColor = typeof(CombatHUDWeaponSlot).GetMethod("GetWeaponCOILStateColor", BindingFlags.Instance | BindingFlags.NonPublic);
        if (mGetWeaponCOILStateColor == null) {
          Log.LogWrite("Can't find CombatHUDWeaponSlot.GetWeaponCOILStateColor\n");
          return false;
        }
#endif
        mGetShotQualityTextColor = typeof(CombatHUDWeaponSlot).GetMethod("GetShotQualityTextColor", BindingFlags.Instance | BindingFlags.NonPublic);
        if (mGetShotQualityTextColor == null) {
          Log.LogWrite("Can't find CombatHUDWeaponSlot.GetShotQualityTextColor\n");
          return false;
        }
        fCombat = typeof(Weapon).GetField("combat", BindingFlags.Instance | BindingFlags.NonPublic);
        if (fCombat == null) {
          Log.LogWrite("Can't find Weapon.combat\n");
          return false;
        }
      } catch (Exception e) {
        Log.LogWrite(e.ToString() + "\n");
        return false;
      }
      return true;
    }
    public static CombatGameState Combat(this Weapon weapon) {
      return (CombatGameState)fCombat.GetValue(weapon);
    }
    public static bool WillFireToPosition(this Weapon weapon, Vector3 position) {
      if (weapon.IsDisabled || !weapon.HasAmmo) {
        Log.M.WL(1, "weapon disabled or have no ammo");
        return false;
      }
      bool result = weapon.Combat().LOFCache.UnitHasLOFToTargetAtTargetPosition(weapon.parent, weapon.parent, weapon.MaxRange,
        weapon.parent.CurrentPosition, weapon.parent.CurrentRotation, position, weapon.parent.CurrentRotation, weapon.IndirectFireCapable());
      Log.M.WL(1, "UnitHasLOFToTargetAtTargetPosition "+result+" max range:"+ weapon.MaxRange);
      return result;
    }
    public static ICombatant findFriendlyNearPos(AbstractActor actor,Vector3 worldPos) {
      ICombatant result = null;
      float resultDist = 0f;
      Log.LogWrite(" findFriendlyNearPos " + worldPos + "\n");
      foreach (AbstractActor friend in actor.Combat.GetAllAlliesOf(actor)) {
        if (friend.IsDead) { continue; }
        float distance = Vector3.Distance(friend.CurrentPosition, worldPos);
        Log.LogWrite("  friend:"+friend.DisplayName+" "+distance+"\n");
        if (distance > CustomAmmoCategories.Settings.TerrainFiendlyFireRadius) { continue; }
        if ((distance < resultDist) || (result == null)) { result = friend; resultDist = distance; }
      }
      Log.LogWrite("  result: " + (result==null?"null":result.DisplayName) + "\n");
      return result;
    }
#if BT1_8
    public static void ShowTextColor(this CombatHUDWeaponSlot slot, Color color) {
      Color damageColor = (Color)mGetWeaponCOILStateColor.Invoke(slot, new object[1] { color });
      mShowTextColor.Invoke(slot, new object[4] { color, damageColor, color, true });
    }
    public static void ShowTextColor(this CombatHUDWeaponSlot slot, Color color, Color hitChanceColor) {
      Color damageColor = (Color)mGetWeaponCOILStateColor.Invoke(slot, new object[1] { color });
      mShowTextColor.Invoke(slot, new object[4] { color, damageColor, hitChanceColor, true });
    }
#else
    public static void ShowTextColor(this CombatHUDWeaponSlot slot, Color color) {
      mShowTextColor.Invoke(slot, new object[3] { color, color, true });
    }
    public static void ShowTextColor(this CombatHUDWeaponSlot slot, Color color, Color hitChanceColor) {
      mShowTextColor.Invoke(slot, new object[3] { color, hitChanceColor, true });
    }
#endif
    public static float UpdateWeapons(this SelectionStateCommandTargetSinglePoint __instance, Vector3 worldPos) {
      if (__instance.FromButton.Ability.Def.Description.Id != CombatHUDMechwarriorTray_InitAbilityButtons.AbilityName) { return float.NaN; };
      int NumPositionsLocked = (int)pNumPositionsLocked.GetValue(__instance, null);
      Vector3 targetPos = worldPos;
      if (NumPositionsLocked > 0) {
        targetPos = (Vector3)pTargetPosition.GetValue(__instance, null);
      }
      float distance = Vector3.Distance(prevPosition, targetPos);
      timeCounter += Time.deltaTime;
      if ((distance < 10f) && (timeCounter < 0.5f)) { return float.NaN; }
      float result = __instance.FromButton.Ability.Def.FloatParam1;
      timeCounter = 0f;
      prevPosition = targetPos;
      Log.LogWrite("SelectionStateCommandTargetSinglePoint.ProcessMousePos " + __instance.FromButton.Ability.Def.Description.Id + "\n");
      CombatHUD HUD = (CombatHUD)pHUD.GetValue(__instance, null);
      List<CombatHUDWeaponSlot> wSlots = HUD.WeaponPanel.DisplayedWeaponSlots;
      AbstractActor actor = __instance.SelectedActor;
      Vector3 groundPos = targetPos;
      groundPos.y = actor.Combat.MapMetaData.GetCellAt(targetPos).cachedHeight;
      distance = Vector3.Distance(actor.CurrentPosition, groundPos);
      Vector3 collisionWorldPos = Vector3.zero;
      LineOfFireLevel LOFLevel = LineOfFireLevel.LOFBlocked;
      bool isInFiringArc = actor.IsTargetPositionInFiringArc(actor, actor.CurrentPosition, actor.CurrentRotation, groundPos);
      ICombatant target = null;
      if (isInFiringArc) {
        target = findFriendlyNearPos(actor, groundPos);
        if (target == null) {
          LOFLevel = actor.Combat.LOFCache.GetLineOfFire(actor, actor.CurrentPosition, actor, groundPos, actor.CurrentRotation, out collisionWorldPos);
        } else {
          LOFLevel = actor.Combat.LOFCache.GetLineOfFire(actor, actor.CurrentPosition, target, target.CurrentPosition, target.CurrentRotation, out collisionWorldPos);
        }
      }
      foreach (CombatHUDWeaponSlot slot in wSlots) {
        if (slot.DisplayedWeapon == null) { continue; }
        if ((slot.weaponSlotType == CombatHUDWeaponSlot.WeaponSlotType.Melee) || (slot.weaponSlotType == CombatHUDWeaponSlot.WeaponSlotType.DFA)) { continue; }
        if (slot.DisplayedWeapon.IsEnabled == false) { continue; }
        if (slot.DisplayedWeapon.HasAmmo == false) { continue; }
        UILookAndColorConstants LookAndColorConstants = (UILookAndColorConstants)pLookAndColorConstants.GetValue(slot, null);
        float AOERange = slot.DisplayedWeapon.AOERange();
        if (target == null) {
          slot.ClearHitChance();
          if ((slot.DisplayedWeapon.isAMS()) || (isInFiringArc == false) || (distance > slot.DisplayedWeapon.MaxRange) || ((LOFLevel < LineOfFireLevel.LOFObstructed) && (slot.DisplayedWeapon.IndirectFireCapable() == false))) {
            Log.LogWrite(" " + slot.DisplayedWeapon.UIName + ":disabled\n");
            slot.MainImage.color = LookAndColorConstants.WeaponSlotColors.UnavailableBGColor;
            //mShowTextColor.Invoke(slot, new object[2] { LookAndColorConstants.WeaponSlotColors.UnavailableTextColor, true });
            slot.ShowTextColor(LookAndColorConstants.WeaponSlotColors.UnavailableTextColor);
            slot.ToggleButton.childImage.color = LookAndColorConstants.WeaponSlotColors.UnavailableToggleColor;
          } else {
            Log.LogWrite(" " + slot.DisplayedWeapon.UIName + ":enabled\n");
            slot.MainImage.color = LookAndColorConstants.WeaponSlotColors.AvailableBGColor;
            //mShowTextColor.Invoke(slot, new object[2] { LookAndColorConstants.WeaponSlotColors.AvailableTextColor, true });
            slot.ShowTextColor(LookAndColorConstants.WeaponSlotColors.AvailableTextColor);
            slot.ToggleButton.childImage.color = LookAndColorConstants.WeaponSlotColors.AvailableToggleColor;
            if (AOERange > result) { result = AOERange; };
          }
        } else {
          if ((slot.DisplayedWeapon.isAMS()) || (slot.DisplayedWeapon.WillFireAtTarget(target))) {
            Log.LogWrite(" " + slot.DisplayedWeapon.UIName + ":disabled\n");
            slot.MainImage.color = LookAndColorConstants.WeaponSlotColors.UnavailableBGColor;
            //mShowTextColor.Invoke(slot, new object[2] { LookAndColorConstants.WeaponSlotColors.UnavailableTextColor, true });
            slot.ShowTextColor(LookAndColorConstants.WeaponSlotColors.UnavailableTextColor);
            slot.ToggleButton.childImage.color = LookAndColorConstants.WeaponSlotColors.UnavailableToggleColor;
            slot.ClearHitChance();
          } else {
            float hitChance = slot.DisplayedWeapon.GetToHitFromPosition(target, 1, actor.CurrentPosition, target.CurrentPosition, false, false, false);
            slot.SetHitChance(hitChance);
            Log.LogWrite(" " + slot.DisplayedWeapon.UIName + ":enabled\n");
            slot.MainImage.color = LookAndColorConstants.WeaponSlotColors.AvailableBGColor;
            Color hitChanceColor = (Color)mGetShotQualityTextColor.Invoke(slot, new object[1] { hitChance });
            //mShowTextColor.Invoke(slot, new object[3] { LookAndColorConstants.WeaponSlotColors.SelectedTextColor, hitChanceColor, true });
            slot.ShowTextColor(LookAndColorConstants.WeaponSlotColors.SelectedTextColor,hitChanceColor);
            //this.ShowTextColor(this.LookAndColorConstants.WeaponSlotColors.SelectedTextColor, this.GetShotQualityTextColor(this.HitChance), true);
            //mShowTextColor.Invoke(slot, new object[2] { LookAndColorConstants.WeaponSlotColors.AvailableTextColor, true });
            slot.ToggleButton.childImage.color = LookAndColorConstants.WeaponSlotColors.AvailableToggleColor;
            if (AOERange > result) { result = AOERange; };
          }
        }
      }
      return result;
    }
    public static void Postfix(SelectionStateCommandTargetSinglePoint __instance, Vector3 worldPos) {
      //__instance.UpdateWeapons(worldPos);
    }
  }
  public class TerrainAttackDeligate {
    public AbstractActor actor;
    public CombatHUD HUD;
    public LineOfFireLevel LOFLevel;
    public ICombatant target;
    public Vector3 targetPosition;
    public List<Weapon> weaponsList;
    public TerrainAttackDeligate(AbstractActor actor, CombatHUD HUD, LineOfFireLevel LOF, ICombatant target, Vector3 targetPosition, List<Weapon> weaponsList) {
      this.actor = actor;
      this.HUD = HUD;
      this.LOFLevel = LOF;
      this.target = target;
      this.targetPosition = targetPosition;
      this.weaponsList = weaponsList;
    }
    public void PerformAttack() {
      MechRepresentation gameRep = actor.GameRep as MechRepresentation;
      if ((UnityEngine.Object)gameRep != (UnityEngine.Object)null) {
        Log.LogWrite("ToggleRandomIdles false\n");
        gameRep.ToggleRandomIdles(false);
      }
      string actorGUID = HUD.SelectedActor.GUID;
      HUD.SelectionHandler.DeselectActor(HUD.SelectionHandler.SelectedActor);
      HUD.MechWarriorTray.HideAllChevrons();
      CombatSelectionHandler_TrySelectActor.SelectionForbidden = true;
      int seqId = actor.Combat.StackManager.NextStackUID;
      if (actor.GUID == target.GUID) {
        Log.LogWrite("Registering terrain attack to " + seqId + "\n");
        CustomAmmoCategories.addTerrainHitPosition(actor, targetPosition, LOFLevel < LineOfFireLevel.LOFObstructed);
      } else {
        Log.LogWrite("Registering friendly attack to " + seqId + "\n");
      }
      AttackDirector.AttackSequence attackSequence = actor.Combat.AttackDirector.CreateAttackSequence(seqId, actor, target, actor.CurrentPosition, actor.CurrentRotation, 0, weaponsList, MeleeAttackType.NotSet, 0, false);
      attackSequence.indirectFire = LOFLevel < LineOfFireLevel.LOFObstructed;
      actor.Combat.AttackDirector.PerformAttack(attackSequence);
    }
  }
  [HarmonyPatch(typeof(CombatHUDActionButton))]
  [HarmonyPatch("ActivateCommandAbility")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string), typeof(Vector3) })]
  public static class CombatHUDActionButton_ActivateCommandAbility {
    public static void Postfix(CombatHUDActionButton __instance, string teamGUID, Vector3 targetPosition) {
      Log.LogWrite("CombatHUDActionButton.ActivateCommandAbility " + __instance.GUID + "\n");
      if (__instance.GUID == "ID_ATTACKGROUND") {
        GenericPopupBuilder popup = null;
        CombatHUD HUD = (CombatHUD)typeof(CombatHUDActionButton).GetProperty("HUD", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance, null);
        AbstractActor actor = HUD.SelectedActor;
        if (actor == null) { return; };
        StringBuilder text = new StringBuilder();
        Vector3 groundPos = targetPosition;
        groundPos.y = actor.Combat.MapMetaData.GetCellAt(targetPosition).cachedHeight;
        float distance = Mathf.Round(Vector3.Distance(actor.CurrentPosition, groundPos));
        //bool cantFire = false;
        Vector3 collisionWorldPos;
        List<Weapon> weaponsList = new List<Weapon>();
        ICombatant target = SelectionStateCommandTargetSinglePoint_ProcessMousePos.findFriendlyNearPos(actor, groundPos);
        foreach (Weapon weapon in actor.Weapons) {
          if (weapon.IsFunctional == false) { continue; }
          if (weapon.IsEnabled == false) { continue; };
          if (weapon.isAMS()) { continue; }
          if(target == null) {
            if (weapon.WillFireToPosition(groundPos) == false) {
              Log.LogWrite(" weapon " + weapon.UIName + " will NOT fire at "+groundPos+ " IndirectFireCapable:" + weapon.IndirectFireCapable()+"\n");
              continue;
            } else {
              Log.LogWrite(" weapon " + weapon.UIName + " will fire at " + groundPos + " IndirectFireCapable:" + weapon.IndirectFireCapable() + "\n");
            }
          } else {
            bool ret = weapon.WillFireAtTargetFromPosition(target,actor.CurrentPosition);
            Log.LogWrite(" weapon " + weapon.UIName + " will fire at target "+target.DisplayName+" "+ret+"\n");
            if (ret == false) { continue; }
          }
          weaponsList.Add(weapon);
        }
        if(weaponsList.Count == 0) {
          popup = GenericPopupBuilder.Create(GenericPopupType.Info, "No weapon can fire:\n" + text.ToString());
          popup.AddButton("Ok", (Action)null, true, (PlayerAction)null);
          popup.IsNestedPopupWithBuiltInFader().CancelOnEscape().Render();
          return;
        }
        LineOfFireLevel LOFLevel = LineOfFireLevel.LOFBlocked;
        if (target != null) {
          LOFLevel = actor.Combat.LOFCache.GetLineOfFire(actor, actor.CurrentPosition, target, target.CurrentPosition, target.CurrentRotation, out collisionWorldPos);
        } else {
          LOFLevel = actor.Combat.LOFCache.GetLineOfFire(actor, actor.CurrentPosition, actor, groundPos, actor.CurrentRotation, out collisionWorldPos);
        }
        MechRepresentation gameRep = actor.GameRep as MechRepresentation;
        if ((UnityEngine.Object)gameRep != (UnityEngine.Object)null) {
          Log.LogWrite("ToggleRandomIdles false\n");
          gameRep.ToggleRandomIdles(false);
        }
        string actorGUID = HUD.SelectedActor.GUID;
        HUD.SelectionHandler.DeselectActor(HUD.SelectionHandler.SelectedActor);
        HUD.MechWarriorTray.HideAllChevrons();
        CombatSelectionHandler_TrySelectActor.SelectionForbidden = true;
        int seqId = actor.Combat.StackManager.NextStackUID;
        if (target == null) { target = actor; };
        if (actor.GUID == target.GUID) {
          Log.LogWrite("Registering terrain attack to " + actor.GUID + "\n");
          CustomAmmoCategories.addTerrainHitPosition(actor, groundPos, LOFLevel < LineOfFireLevel.LOFObstructed);
        } else {
          Log.LogWrite("Registering friendly attack to " + seqId + "\n");
        }
        AttackDirector.AttackSequence attackSequence = actor.Combat.AttackDirector.CreateAttackSequence(seqId, actor, target, actor.CurrentPosition, actor.CurrentRotation, 0, weaponsList, MeleeAttackType.NotSet, 0, false);
        attackSequence.indirectFire = LOFLevel < LineOfFireLevel.LOFObstructed;
        Log.LogWrite(" attackSequence.indirectFire " + attackSequence.indirectFire + " LOS "+ LOFLevel + "\n");
        actor.Combat.AttackDirector.PerformAttack(attackSequence);

        //CustomAmmoCategoriesLog.Log.LogWrite("Orders is null:"+(HUD.SelectionHandler.ActiveState.Orders==null));
        //MessageCenterMessage message1 = (MessageCenterMessage)new AttackInvocation(actor, null, weaponsList, MeleeAttackType.NotSet, 0);
        //HUD.MechWarriorTray.ConfirmAbilities(AbilityDef.ActivationTiming.ConsumedByMovement);
        //HUD.MechWarriorTray.ConfirmAbilities(AbilityDef.ActivationTiming.ConsumedByFiring);
        //ReceiveMessageCenterMessage subscriber = (ReceiveMessageCenterMessage)(message => this.Orders = (message as AddSequenceToStackMessage).sequence);
        //this.Combat.MessageCenter.AddSubscriber(MessageCenterMessageType.AddSequenceToStackMessage, subscriber);
        //this.Combat.MessageCenter.PublishMessage(message1);
        //this.Combat.MessageCenter.RemoveSubscriber(MessageCenterMessageType.AddSequenceToStackMessage, subscriber);
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDMechwarriorTray))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(CombatGameState), typeof(CombatHUD) })]
  public static class CombatHUDMechwarriorTray_Init {

    public static void Postfix(CombatHUDMechwarriorTray __instance, CombatGameState Combat, CombatHUD HUD) {
      try {
        CustomAmmoCategoriesLog.Log.LogWrite("adding button\n");
        GameObject[] ActionButtonHolders = new GameObject[__instance.ActionButtonHolders.Length + 1];
        __instance.ActionButtonHolders.CopyTo(ActionButtonHolders, 0);
        CombatHUDActionButton[] ActionButtons = new CombatHUDActionButton[__instance.ActionButtons.Length + 1];
        __instance.ActionButtons.CopyTo(ActionButtons, 0);
        CombatHUDActionButton[] oldAbilityButtons = (CombatHUDActionButton[])typeof(CombatHUDMechwarriorTray).GetProperty("AbilityButtons", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance, null);
        CombatHUDActionButton[] AbilityButtons = new CombatHUDActionButton[oldAbilityButtons.Length + 1];
        oldAbilityButtons.CopyTo(AbilityButtons, 0);

        GameObject sourceButtonObject = __instance.ActionButtonHolders[9];
        CombatHUDActionButton sourceButton = sourceButtonObject.GetComponentInChildren<CombatHUDActionButton>(true);
        GameObject newButtonObject = GameObject.Instantiate(sourceButtonObject, sourceButtonObject.transform.parent);
        CombatHUDActionButton newButton = newButtonObject.GetComponentInChildren<CombatHUDActionButton>(true);
        ActionButtonHolders[__instance.ActionButtonHolders.Length] = newButtonObject;
        Vector3[] corners = new Vector3[4];
        sourceButtonObject.GetComponent<RectTransform>().GetWorldCorners(corners);
        float width = corners[2].x - corners[0].x;
        newButtonObject.transform.localPosition.Set(
          newButtonObject.transform.localPosition.x + width,
          newButtonObject.transform.localPosition.y,
          newButtonObject.transform.localPosition.z
        );
        CombatHUDSidePanelHoverElement panelHoverElement = newButton.GetComponentInChildren<CombatHUDSidePanelHoverElement>(true);
        if ((UnityEngine.Object)panelHoverElement == (UnityEngine.Object)null) {
          panelHoverElement = newButton.gameObject.AddComponent<CombatHUDSidePanelHoverElement>();
        }
        panelHoverElement.Init(HUD);
        newButton.Init(Combat, HUD, BTInput.Instance.Key_None(), true);
        ActionButtonHolders[__instance.ActionButtonHolders.Length] = newButtonObject;
        ActionButtons[__instance.ActionButtons.Length] = newButton;
        AbilityButtons[oldAbilityButtons.Length] = newButton;

        __instance.ActionButtonHolders = ActionButtonHolders;
        PropertyInfo ActionButtonsProperty = typeof(CombatHUDMechwarriorTray).GetProperty("ActionButtons");
        //ActionButtonsProperty.DeclaringType.GetProperty("ActionButtons");
        ActionButtonsProperty.GetSetMethod(true).Invoke(__instance, new object[1] { (object)ActionButtons });
        typeof(CombatHUDMechwarriorTray).GetProperty("AbilityButtons", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, AbilityButtons, null);
      }catch(Exception e){ Log.M.WL(e.ToString(),true); };
    }
  }
  public class InitAbilityButtonsDelegate {
    public CombatHUDMechwarriorTray __instance;
    public AbstractActor actor;
    public AbilityDef abilityDef;
    public InitAbilityButtonsDelegate(CombatHUDMechwarriorTray hud,AbstractActor unit) {
      __instance = hud;
      actor = unit;
      abilityDef = null;
    }
    public void OnAbilityLoad() {
      abilityDef = actor.Combat.DataManager.AbilityDefs.Get(CombatHUDMechwarriorTray_InitAbilityButtons.AbilityName);
      Log.LogWrite("InitAbilityButtonsDelegate.OnAbilityLoad "+abilityDef.Description.Id+"\n");
      if (abilityDef.DependenciesLoaded(1000u)) {
        Log.LogWrite(" dependencies fully loaded\n");
        OnAbilityFullLoad();
      } else {
        Log.LogWrite(" request dependencies\n");
        DataManager.InjectedDependencyLoadRequest dependencyLoad = new DataManager.InjectedDependencyLoadRequest(actor.Combat.DataManager);
        abilityDef.GatherDependencies(actor.Combat.DataManager, dependencyLoad, 1000U);
        dependencyLoad.RegisterLoadCompleteCallback(new Action(OnAbilityFullLoad));
        actor.Combat.DataManager.InjectDependencyLoader(dependencyLoad, 1000U);
      }
    }
    public void OnAbilityFullLoad() {
      Ability gaAbility = null;
      Log.LogWrite("InitAbilityButtonsDelegate.OnAbilityFullLoad " + abilityDef.Description.Id + "\n");
      if (CombatHUDMechwarriorTray_InitAbilityButtons.attackGroundAbilities.ContainsKey(actor.GUID) == false) {
        Log.LogWrite(" need create new ability\n");
        gaAbility = new Ability(abilityDef);
        gaAbility.Init(actor.Combat);
        CombatHUDMechwarriorTray_InitAbilityButtons.attackGroundAbilities.Add(actor.GUID, gaAbility);
      } else {
        Log.LogWrite(" ability exists\n");
        gaAbility = CombatHUDMechwarriorTray_InitAbilityButtons.attackGroundAbilities[actor.GUID];
      }
      Log.LogWrite(" geting buttons\n");
      CombatHUDActionButton[] AbilityButtons = (CombatHUDActionButton[])typeof(CombatHUDMechwarriorTray).GetProperty("AbilityButtons", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance, null);
      Log.LogWrite(" AbilityButtons.Length = " + AbilityButtons.Length + "\n");
      Log.LogWrite(" aDef.Targeting = " + abilityDef.Targeting + "\n");
      Log.LogWrite(" aDef.AbilityIcon = " + abilityDef.AbilityIcon + "\n");
      Log.LogWrite(" aDef.Description.Name = " + abilityDef.Description.Name + "\n");
      Log.LogWrite(" AbilityButtons[AbilityButtons.Length - 1].GUID = " + AbilityButtons[AbilityButtons.Length - 1].GUID + "\n");
      AbilityButtons[AbilityButtons.Length - 1].InitButton(
        (SelectionType)typeof(CombatHUDMechwarriorTray).GetMethod("GetSelectionTypeFromTargeting", BindingFlags.Static | BindingFlags.Public).Invoke(
          null, new object[2] { (object)abilityDef.Targeting, (object)false }
        ),
        gaAbility, abilityDef.AbilityIcon,
        "ID_ATTACKGROUND",
        abilityDef.Description.Name,
        actor
      );
      Log.LogWrite(" init button success\n");
      AbilityButtons[AbilityButtons.Length - 1].isClickable = true;
      AbilityButtons[AbilityButtons.Length - 1].RefreshUIColors();
      Log.LogWrite("finished\n");
    }
  }
  [HarmonyPatch(typeof(CombatHUDMechwarriorTray))]
  [HarmonyPatch("InitAbilityButtons")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor) })]
  public static class CombatHUDMechwarriorTray_InitAbilityButtons {
    public static readonly string AbilityName = "AbilityDefCAC_AttackGround";
    public static Dictionary<string, Ability> attackGroundAbilities = new Dictionary<string, Ability>();

    public static void Postfix(CombatHUDMechwarriorTray __instance, AbstractActor actor) {
      Log.LogWrite("CombatHUDMechwarriorTray.InitAbilityButtons\n");
      AbilityDef aDef = null;
      InitAbilityButtonsDelegate id = new InitAbilityButtonsDelegate(__instance, actor);
      if (actor.Combat.DataManager.AbilityDefs.TryGet("AbilityDefCAC_AttackGround", out aDef) == false) {
        Log.LogWrite(" requesting ability def loading\n");
        DataManager.InjectedDependencyLoadRequest dependencyLoad = new DataManager.InjectedDependencyLoadRequest(actor.Combat.DataManager);
        dependencyLoad.RequestResource(BattleTechResourceType.AbilityDef, AbilityName);
        dependencyLoad.RegisterLoadCompleteCallback(new Action(id.OnAbilityLoad));
        actor.Combat.DataManager.InjectDependencyLoader(dependencyLoad, 1000U);
      } else {
        Log.LogWrite(" ability def already loaded\n");
        id.OnAbilityLoad();
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDMechwarriorTray))]
  [HarmonyPatch("ResetAbilityButtons")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor) })]
  public static class CombatHUDMechwarriorTray_ResetAbilityButtons {
    public static void Postfix(CombatHUDMechwarriorTray __instance, AbstractActor actor) {
      CustomAmmoCategoriesLog.Log.LogWrite("CombatHUDMechwarriorTray.ResetAbilityButtons\n");
      AbilityDef aDef = null;
      if (actor.Combat.DataManager.AbilityDefs.TryGet("AbilityDefCAC_AttackGround", out aDef)) {
        Ability gaAbility = null;
        if (CombatHUDMechwarriorTray_InitAbilityButtons.attackGroundAbilities.ContainsKey(actor.GUID) == false) {
          gaAbility = new Ability(aDef);
          gaAbility.Init(actor.Combat);
          CombatHUDMechwarriorTray_InitAbilityButtons.attackGroundAbilities.Add(actor.GUID, gaAbility);
        } else {
          gaAbility = CombatHUDMechwarriorTray_InitAbilityButtons.attackGroundAbilities[actor.GUID];
        }
        bool forceInactive = actor.HasActivatedThisRound || actor.MovingToPosition != null || actor.Combat.StackManager.IsAnyOrderActive && actor.Combat.TurnDirector.IsInterleaved;
        CustomAmmoCategoriesLog.Log.LogWrite(" actor.HasActivatedThisRound:" + actor.HasActivatedThisRound + "\n");
        CustomAmmoCategoriesLog.Log.LogWrite(" actor.MovingToPosition:" + (actor.MovingToPosition != null) + "\n");
        CustomAmmoCategoriesLog.Log.LogWrite(" actor.Combat.StackManager.IsAnyOrderActive:" + actor.Combat.StackManager.IsAnyOrderActive + "\n");
        CustomAmmoCategoriesLog.Log.LogWrite(" actor.Combat.TurnDirector.IsInterleaved:" + actor.Combat.TurnDirector.IsInterleaved + "\n");
        CustomAmmoCategoriesLog.Log.LogWrite(" forceInactive:" + forceInactive + "\n");
        CombatHUDActionButton[] AbilityButtons = (CombatHUDActionButton[])typeof(CombatHUDMechwarriorTray).GetProperty("AbilityButtons", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance, null);
        typeof(CombatHUDMechwarriorTray).GetMethod("ResetAbilityButton", BindingFlags.NonPublic | BindingFlags.Instance, null,
          new Type[] { typeof(AbstractActor), typeof(CombatHUDActionButton), typeof(Ability), typeof(bool) }, null
        ).Invoke(__instance, new object[4] { (object)actor, AbilityButtons[AbilityButtons.Length - 1], gaAbility, forceInactive });
        if (forceInactive) { AbilityButtons[AbilityButtons.Length - 1].DisableButton(); };
        if (actor.Combat.TurnDirector.IsInterleaved == false) {
          if (actor.HasFiredThisRound == false) {
            if (gaAbility.IsActive == false) {
              if (gaAbility.IsAvailable == true) {
                if (actor.IsShutDown == false) {
                  CustomAmmoCategoriesLog.Log.LogWrite(" ResetButtonIfNotActive:\n");
                  CustomAmmoCategoriesLog.Log.LogWrite(" IsAbilityActivated:" + AbilityButtons[AbilityButtons.Length - 1].IsAbilityActivated + "\n");
                  if (actor.MovingToPosition == null) { AbilityButtons[AbilityButtons.Length - 1].ResetButtonIfNotActive(actor); };
                }
              }
            }
          } else {
            AbilityButtons[AbilityButtons.Length - 1].DisableButton();
          }
        }
        //__instance.ResetAbilityButton(actor, this.AbilityButtons[index], abilityList[index], forceInactive);

      } else {
        CustomAmmoCategoriesLog.Log.LogWrite("Can't find AbilityDef CAC_AttackGround\n");
      }
    }
  }
}