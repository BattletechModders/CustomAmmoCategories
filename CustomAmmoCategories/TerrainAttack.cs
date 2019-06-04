using BattleTech;
using BattleTech.UI;
using CustAmmoCategories;
using CustomAmmoCategoriesLog;
using Harmony;
using InControl;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace CustAmmoCategories {
  public class SelectionStateCommandAttackGround : SelectionStateCommandTargetSinglePoint {
    public SelectionStateCommandAttackGround(CombatGameState Combat, CombatHUD HUD, CombatHUDActionButton FromButton, AbstractActor actor) : base(Combat, HUD, FromButton) {
      this.SelectedActor = actor;
    }
    public override void OnAddToStack() {
      base.OnAddToStack();
      CameraControl.Instance.SetTargets(this.AllPossibleTargets);
      if (!((UnityEngine.Object)this.SelectedActor.GameRep != (UnityEngine.Object)null))
        return;
      MechRepresentation gameRep = this.SelectedActor.GameRep as MechRepresentation;
      if (!((UnityEngine.Object)gameRep != (UnityEngine.Object)null))
        return;
      gameRep.ToggleRandomIdles(false);
    }

    public override void OnInactivate() {
      base.OnInactivate();
      CameraControl.Instance.ClearTargets();
      if (!((UnityEngine.Object)this.SelectedActor.GameRep != (UnityEngine.Object)null))
        return;
      MechRepresentation gameRep = this.SelectedActor.GameRep as MechRepresentation;
      if (!((UnityEngine.Object)gameRep != (UnityEngine.Object)null))
        return;
      gameRep.ToggleRandomIdles(true);
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
  }
  public static partial class CustomAmmoCategories {
    public static Dictionary<int, Vector3> terrainHitPositions = new Dictionary<int, Vector3>();
    public static void addTerrainHitPosition(int seqId, Vector3 pos) {
      if (CustomAmmoCategories.terrainHitPositions.ContainsKey(seqId) == false) {
        CustomAmmoCategories.terrainHitPositions.Add(seqId, pos);
      }
    }
    public static Vector3 getTerrinHitPosition(int seqId) {
      if (CustomAmmoCategories.terrainHitPositions.ContainsKey(seqId) == false) {
        return Vector3.zero;
      }
      return CustomAmmoCategories.terrainHitPositions[seqId];
    }
  }
}

namespace CustomAmmoCategoriesPatches {
  [HarmonyPatch(typeof(SelectionState))]
  [HarmonyPatch("CanDeselect")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class SelectionState_CanDeselect {
    public static bool Prefix(SelectionState __instance,ref bool __result) {
      CombatGameState Combat = (CombatGameState)typeof(SelectionState).GetProperty("Combat", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance,null);
      CustomAmmoCategoriesLog.Log.LogWrite("SelectionState.CanDeselect\n");
      CustomAmmoCategoriesLog.Log.LogWrite(" Combat.TurnDirector.IsInterleaved = "+ Combat.TurnDirector.IsInterleaved + "\n");
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
        CustomAmmoCategoriesLog.Log.LogWrite("this is terrain attack, no evasive damage or effects");
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
      CustomAmmoCategoriesLog.Log.LogWrite("SelectionState.GetNewSelectionStateByType "+type+":"+FromButton.GUID+"\n");
      if((type == SelectionType.CommandTargetSinglePoint)&&(FromButton.GUID == "ID_ATTACKGROUND")) {
        CustomAmmoCategoriesLog.Log.LogWrite(" creating own selection state\n");
        __result = new SelectionStateCommandAttackGround(Combat, HUD, FromButton, actor);
        return false;
      }
      return true;
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
    public static int doneAfterAttack = -1;
    public static void Postfix(CombatHUD __instance, MessageCenterMessage message) {
      if (CombatSelectionHandler_TrySelectActor.SelectionForbidden == true) {
        AttackDirector.AttackSequence attackSequence = __instance.Combat.AttackDirector.GetAttackSequence((message as AttackSequenceEndMessage).sequenceId);
        if (attackSequence != null) {
          CustomAmmoCategoriesLog.Log.LogWrite("Terrain attack end.\n");
          CombatSelectionHandler_TrySelectActor.SelectionForbidden = false;
          attackSequence.attacker.HasFiredThisRound = true;
          if (attackSequence.attacker.HasMovedThisRound || ((attackSequence.attacker.Combat.TurnDirector.IsInterleaved == true)&&(attackSequence.attacker.CanMoveAfterShooting == false))) {
            //__instance.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AddSequenceToStackMessage(attackSequence.attacker.DoneWithActor()));
            CombatHUD_OnAttackEnd.doneAfterAttack = attackSequence.id;
            CustomAmmoCategoriesLog.Log.LogWrite("Store done with actor:"+ CombatHUD_OnAttackEnd.doneAfterAttack + "\n");
          } else {
            Mech mech = attackSequence.attacker as Mech;
            if (mech != null) {
              mech.GenerateAndPublishHeatSequence(attackSequence.id, true, false, mech.GUID);
            };
            typeof(CombatHUD).GetMethod("OnActorSelected", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[1] { (object)attackSequence.attacker });
            __instance.SelectionHandler.TrySelectActor(attackSequence.attacker, false);
            __instance.MechWarriorTray.ConfirmAbilities(AbilityDef.ActivationTiming.ConsumedByFiring);
          }
          JammingEnabler.jammAMS();
          JammingEnabler.jamm(attackSequence.attacker);
          //__instance.Combat.AttackDirector.RemoveAttackSequence(attackSequence.id);
        }
      }
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
      CustomAmmoCategoriesLog.Log.LogWrite("AttackDirector.OnAttackCompleteTA:" + CombatHUD_OnAttackEnd.doneAfterAttack + "/" + attackCompleteMessage.sequenceId + "\n");
      int sequenceId = attackCompleteMessage.sequenceId;
      AttackDirector.AttackSequence attackSequence = __instance.GetAttackSequence(sequenceId);
      if (attackSequence == null) {
        return true;
      }
      if (attackSequence.id == CombatHUD_OnAttackEnd.doneAfterAttack) {
        __state = attackSequence.attacker; CombatHUD_OnAttackEnd.doneAfterAttack = -1;
        CustomAmmoCategoriesLog.Log.LogWrite(" need to done with:"+__state.DisplayName+":"+__state.GUID+"\n");
      };
      return true;
    }
    public static void Postfix(AttackDirector __instance, MessageCenterMessage message, ref AbstractActor __state) {
      if(__state != null) {
        CustomAmmoCategoriesLog.Log.LogWrite(" done with:" + __state.DisplayName + ":" + __state.GUID + "\n");
        __instance.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AddSequenceToStackMessage(__state.DoneWithActor()));
      }
    }
  }
    public static class WeaponEffect_FireTerrain {
    public static bool Prefix(WeaponEffect __instance, WeaponHitInfo hitInfo, int hitIndex, int emitterIndex) {
      if (hitInfo.attackerId == hitInfo.targetId) {
        CustomAmmoCategoriesLog.Log.LogWrite("On Fire detected terrain attack\n");
        typeof(WeaponEffect).GetField("t", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (object)0.0f);
        typeof(WeaponEffect).GetField("hitIndex", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (object)hitIndex);
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
  [HarmonyPatch(typeof(CombatHUDActionButton))]
  [HarmonyPatch("ActivateCommandAbility")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string), typeof(Vector3) })]
  public static class CombatHUDActionButton_ActivateCommandAbility {
    public static void Postfix(CombatHUDActionButton __instance, string teamGUID, Vector3 targetPosition) {
      CustomAmmoCategoriesLog.Log.LogWrite("CombatHUDActionButton.ActivateCommandAbility " + __instance.GUID + "\n");
      if (__instance.GUID == "ID_ATTACKGROUND") {
        GenericPopupBuilder popup = null;
        CombatHUD HUD = (CombatHUD)typeof(CombatHUDActionButton).GetProperty("HUD", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance, null);
        AbstractActor actor = HUD.SelectedActor;
        if (actor == null) { return; };
        if (actor.IsTargetPositionInFiringArc(actor, actor.CurrentPosition, actor.CurrentRotation, targetPosition) == false) {
          popup = GenericPopupBuilder.Create(GenericPopupType.Info, "Selected position is not in firing arc");
          popup.AddButton("Ok", (Action)null, true, (PlayerAction)null);
          popup.IsNestedPopupWithBuiltInFader().CancelOnEscape().Render();
          return;
        }
        StringBuilder text = new StringBuilder();
        text.Append("Some of selected weapons can't fire:\n");
        float distance = Mathf.Round(Vector3.Distance(actor.CurrentPosition, targetPosition));
        bool cantFire = false;
        Vector3 collisionWorldPos;
        LineOfFireLevel LOFLevel = actor.Combat.LOFCache.GetLineOfFire(actor, actor.CurrentPosition, actor, targetPosition, actor.CurrentRotation, out collisionWorldPos);
        List<Weapon> weaponsList = new List<Weapon>();
        foreach (Weapon weapon in actor.Weapons) {
          if (weapon.IsFunctional == false) { continue; }
          if (weapon.IsEnabled == false) { continue; };
          text.Append(weapon.UIName + " " + distance + "/" + weapon.MaxRange);
          if (distance > weapon.MaxRange) {
            text.Append(" OUT OF RANGE!");
            cantFire = true;
          } else
          if (LOFLevel < LineOfFireLevel.LOFObstructed) {
            if (weapon.isIndirectFireCapable() == false) {
              text.Append(" NO LINE OF FIRE");
              cantFire = true;
            }
          }
          weaponsList.Add(weapon);
          text.Append("\n");
        }
        if (cantFire) {
          popup = GenericPopupBuilder.Create(GenericPopupType.Info, text.ToString());
          popup.AddButton("Ok", (Action)null, true, (PlayerAction)null);
          popup.IsNestedPopupWithBuiltInFader().CancelOnEscape().Render();
          return;
        }
        //CustomAmmoCategoriesLog.Log.LogWrite("Orders is null:"+(HUD.SelectionHandler.ActiveState.Orders==null));
        MechRepresentation gameRep = actor.GameRep as MechRepresentation;
        if ((UnityEngine.Object)gameRep != (UnityEngine.Object)null) {
          gameRep.ToggleRandomIdles(false);
        }
        string actorGUID = HUD.SelectedActor.GUID;
        HUD.SelectionHandler.DeselectActor(HUD.SelectionHandler.SelectedActor);
        HUD.MechWarriorTray.HideAllChevrons();
        CombatSelectionHandler_TrySelectActor.SelectionForbidden = true;
        int seqId = actor.Combat.StackManager.NextStackUID;
        CustomAmmoCategoriesLog.Log.LogWrite("Registering terrain attack to " + seqId + "\n");
        CustomAmmoCategories.addTerrainHitPosition(seqId, targetPosition);
        AttackDirector.AttackSequence attackSequence = actor.Combat.AttackDirector.CreateAttackSequence(seqId, actor, actor, actor.CurrentPosition, actor.CurrentRotation, 0, weaponsList, MeleeAttackType.NotSet, 0, false);
        attackSequence.indirectFire = LOFLevel < LineOfFireLevel.LOFObstructed;
        actor.Combat.AttackDirector.PerformAttack(attackSequence);
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
    }
  }
  [HarmonyPatch(typeof(CombatHUDMechwarriorTray))]
  [HarmonyPatch("InitAbilityButtons")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor) })]
  public static class CombatHUDMechwarriorTray_InitAbilityButtons {
    public static Dictionary<string,Ability> attackGroundAbilities = new Dictionary<string, Ability>();
    public static void Postfix(CombatHUDMechwarriorTray __instance, AbstractActor actor) {
      Log.LogWrite("CombatHUDMechwarriorTray.InitAbilityButtons\n");
      AbilityDef aDef = null;
      if (actor.Combat.DataManager.AbilityDefs.TryGet("AbilityDefCAC_AttackGround", out aDef)) {
        Log.LogWrite(" AbilityDef geted\n");
        Ability gaAbility = null;
        if (CombatHUDMechwarriorTray_InitAbilityButtons.attackGroundAbilities.ContainsKey(actor.GUID) == false) {
          Log.LogWrite(" need create new ability\n");
          gaAbility = new Ability(aDef);
          gaAbility.Init(actor.Combat);
          CombatHUDMechwarriorTray_InitAbilityButtons.attackGroundAbilities.Add(actor.GUID,gaAbility);
        } else {
          Log.LogWrite(" ability exists\n");
          gaAbility = CombatHUDMechwarriorTray_InitAbilityButtons.attackGroundAbilities[actor.GUID];
        }
        Log.LogWrite(" geting buttons\n");
        CombatHUDActionButton[] AbilityButtons = (CombatHUDActionButton[])typeof(CombatHUDMechwarriorTray).GetProperty("AbilityButtons", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance, null);
        Log.LogWrite(" AbilityButtons.Length = "+ AbilityButtons.Length + "\n");
        Log.LogWrite(" aDef.Targeting = " + aDef.Targeting + "\n");
        Log.LogWrite(" aDef.AbilityIcon = " + aDef.AbilityIcon + "\n");
        Log.LogWrite(" aDef.Description.Name = " + aDef.Description.Name + "\n");
        Log.LogWrite(" AbilityButtons[AbilityButtons.Length - 1].GUID = " + AbilityButtons[AbilityButtons.Length - 1].GUID + "\n");
        AbilityButtons[AbilityButtons.Length - 1].InitButton(
          (SelectionType)typeof(CombatHUDMechwarriorTray).GetMethod("GetSelectionTypeFromTargeting", BindingFlags.Static | BindingFlags.Public).Invoke(
            null, new object[2] { (object)aDef.Targeting, (object)false }
          ),
          gaAbility, aDef.AbilityIcon,
          "ID_ATTACKGROUND",
          aDef.Description.Name,
          actor
        );
        Log.LogWrite(" init button success\n");
        AbilityButtons[AbilityButtons.Length - 1].isClickable = true;
        AbilityButtons[AbilityButtons.Length - 1].RefreshUIColors();
        Log.LogWrite("finished\n");
      } else {
        Log.LogWrite("Can't find AbilityDefCAC_AttackGround\n");
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
        CustomAmmoCategoriesLog.Log.LogWrite(" actor.MovingToPosition:" + (actor.MovingToPosition!=null) + "\n");
        CustomAmmoCategoriesLog.Log.LogWrite(" actor.Combat.StackManager.IsAnyOrderActive:" + actor.Combat.StackManager.IsAnyOrderActive + "\n");
        CustomAmmoCategoriesLog.Log.LogWrite(" actor.Combat.TurnDirector.IsInterleaved:" + actor.Combat.TurnDirector.IsInterleaved + "\n");
        CustomAmmoCategoriesLog.Log.LogWrite(" forceInactive:"+forceInactive+"\n");
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
        CustomAmmoCategoriesLog.Log.LogWrite("Can't find AbilityDefCAC_AttackGround\n");
      }
    }
  }
  [HarmonyPatch(typeof(CombatGameState))]
  [HarmonyPatch("OnCombatGameDestroyed")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatGameState_OnCombatGameDestroyedTAttack {
    public static bool Prefix(CombatGameState __instance) {
      CombatHUDMechwarriorTray_InitAbilityButtons.attackGroundAbilities.Clear();
      CustomAmmoCategories.terrainHitPositions.Clear();
      ASWatchdog.EndWatchDogThread();
      return true;
    }
  }
}