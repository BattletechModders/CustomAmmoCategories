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
using BattleTech.Data;
using BattleTech.UI;
using CustAmmoCategories;
using CustomAmmoCategoriesLog;
using CustomAmmoCategoriesPatches;
using CustomAmmoCategoriesPathes;
using Harmony;
using HBS;
using InControl;
using Localize;
using SVGImporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Text = Localize.Text;

namespace CustAmmoCategories {
  public class SelectionStateCommandAttackGround : SelectionStateCommandTargetSinglePoint {
    private bool HasActivated = false;
    public float CircleRange = 10f;
    public SelectionStateCommandAttackGround(CombatGameState Combat, CombatHUD HUD, CombatHUDActionButton FromButton, AbstractActor actor) : base(Combat, HUD, FromButton) {
      this.SelectedActor = actor;
      HasActivated = false;
    }
    protected override bool ShouldShowWeaponsUI {
      get {
        return true;
      }
    }
    protected override bool ShouldShowTargetingLines {
      get {
        return true;
      }
    }
    protected override bool ShouldShowFiringArcs {
      get {
        return false;
      }
    }
    public override bool ConsumesMovement {
      get {
        return false;
      }
    }
    public override bool ConsumesFiring {
      get {
        return true;
      }
    }
    public override bool CanBackOut {
      get {
        if (this.Orders == null) {
          return true;
        }
        return false;
      }
    }
    protected override bool showHeatWarnings {
      get {
        return true;
      }
    }
    public virtual bool HasCalledShot {
      get {
        return false;
      }
    }
    public virtual bool NeedsCalledShot {
      get {
        return false;
      }
    }
    public override bool CanActorUseThisState(AbstractActor actor) {
      return true;
    }
    public override bool CanDeselect {
      get {
        if (!base.CanDeselect)
          return false;
        if (!this.SelectedActor.HasMovedThisRound)
          return !this.SelectedActor.CanMoveAfterShooting;
        return true;
      }
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
    public void AttackOrdersSet(MessageCenterMessage message) {
      this.Orders = (message as AddSequenceToStackMessage).sequence;
      Log.M.TWL(0,"Attack orders set:"+Orders.GetType().ToString()+" GUID:"+Orders.SequenceGUID);
    }
    public override bool ProcessPressedButton(string button) {
      this.HasActivated = button == "BTN_FireConfirm";
      if (button != "BTN_FireConfirm") {
        return base.ProcessPressedButton(button);
      }
      if (this.Orders != null)
        return false;
      this.HideFireButton(false);
      //ExplosionAPIHelper.AoEExplode("WFX_Nuke", Vector3.one * 50f, 20f, "big_explosion", this.targetPosition, 100f, 2000f, 100f, 100f, new List<EffectData>(), false, 3, 40, 1f, 5, string.Empty, Vector3.zero, string.Empty, 0, 0);
      //return true;
      GenericPopupBuilder popup = null;
      StringBuilder text = new StringBuilder();
      List<Weapon> weaponsList = new List<Weapon>();
      ICombatant target = this.TargetedCombatant;
      Vector3 groundPos = this.targetPosition;
      AbstractActor actor = this.SelectedActor;
      foreach (Weapon weapon in actor.Weapons) {
        if (weapon.IsFunctional == false) { continue; }
        if (weapon.IsEnabled == false) { continue; };
        if (weapon.isAMS()) { continue; }
        if (target == null) {
          if (weapon.WillFireToPosition(groundPos) == false) {
            Log.LogWrite(" weapon " + weapon.UIName + " will NOT fire at " + groundPos + " IndirectFireCapable:" + weapon.IndirectFireCapable() + "\n");
            continue;
          } else {
            Log.LogWrite(" weapon " + weapon.UIName + " will fire at " + groundPos + " IndirectFireCapable:" + weapon.IndirectFireCapable() + "\n");
          }
        } else {
          bool ret = weapon.WillFireAtTargetFromPosition(target, actor.CurrentPosition);
          Log.LogWrite(" weapon " + weapon.UIName + " will fire at target " + target.DisplayName + " " + ret + "\n");
          if (ret == false) { continue; }
        }
        weaponsList.Add(weapon);
      }
      if (weaponsList.Count == 0) {
        popup = GenericPopupBuilder.Create(GenericPopupType.Info, "No weapon can fire:\n" + text.ToString());
        popup.AddButton("Ok", (Action)null, true, (PlayerAction)null);
        popup.IsNestedPopupWithBuiltInFader().CancelOnEscape().Render();
        return false;
      }
      Vector3 collisionWorldPos;
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
      if (target == null) { target = actor; };
      if (actor.GUID == target.GUID) {
        Log.LogWrite("Registering terrain attack to " + actor.GUID + "\n");
        CustomAmmoCategories.addTerrainHitPosition(actor, groundPos, LOFLevel < LineOfFireLevel.LOFObstructed);
      } else {
        Log.LogWrite("Registering friendly attack to " + new Text(target.DisplayName) + "\n");
      }
      AttackInvocation invocation = new AttackInvocation(this.SelectedActor, target, weaponsList);
      if (invocation == null) {
        Debug.Log((object)"No invocation created");
        return false;
      }
      this.HUD.MechWarriorTray.ConfirmAbilities(AbilityDef.ActivationTiming.ConsumedByMovement);
      this.HUD.MechWarriorTray.ConfirmAbilities(AbilityDef.ActivationTiming.ConsumedByFiring);
      ReceiveMessageCenterMessage subscriber = (ReceiveMessageCenterMessage)(message => this.Orders = (message as AddSequenceToStackMessage).sequence);
      this.Combat.MessageCenter.AddSubscriber(MessageCenterMessageType.AddSequenceToStackMessage, subscriber);
      CombatSelectionHandler_TrySelectActor.SelectionForbidden = true;
      this.Combat.AttackDirector.isSequenceIsTerrainAttack(true);
      this.Combat.MessageCenter.PublishMessage(invocation);
      this.Combat.MessageCenter.RemoveSubscriber(MessageCenterMessageType.AddSequenceToStackMessage, subscriber);
      WeaponRangeIndicators.Instance.HideTargetingLines(this.Combat);
      return true;
    }
    public override bool ProcessLeftClick(Vector3 worldPos) {
      if (this.NumPositionsLocked != 0)
        return false;
      this.targetPosition = worldPos;// this.GetValidTargetPosition(worldPos);
      //this.GetTargetedActors(this.targetPosition, this.FromButton.Ability.Def.FloatParam1);
      //this.SetTargetHighlights(true);
      ++this.NumPositionsLocked;
      this.HUD.RegisterAttackGroundTarget(this.TargetedCombatant);
      this.ShowFireButton(CombatHUDFireButton.FireMode.Confirm, Ability.ProcessDetailString(this.FromButton.Ability).ToString(true));
      if ((UnityEngine.Object)this.SelectedActor.GameRep != (UnityEngine.Object)null) {
        MechRepresentation gameRep = this.SelectedActor.GameRep as MechRepresentation;
        if ((UnityEngine.Object)gameRep != (UnityEngine.Object)null) {
          gameRep.ToggleRandomIdles(false);
        }
        this.SelectedActor.GameRep.FacePoint(true, this.targetPosition, false, 0.5f, -1, -1, false, (GameRepresentation.RotationCompleteDelegate)null);
      }
      return true;
    }
    private Vector3 prevPosition = Vector3.zero;
    private float timeCounter = 0f;
    public bool UpdateWeapons(Vector3 worldPos, out float range) {
      if (this.FromButton.Ability.Def.Description.Id != CombatHUDMechwarriorTray_InitAbilityButtons.AbilityName) { range = float.NaN; return false; };
      int NumPositionsLocked = this.NumPositionsLocked;
      Vector3 targetPos = worldPos;
      if (NumPositionsLocked > 0) {
        targetPos = this.targetPosition;
      }
      float distance = Vector3.Distance(prevPosition, targetPos);
      timeCounter += Time.deltaTime;
      if ((distance < 10f) && (timeCounter < 0.5f)) { range = float.NaN; return false; }
      float result = this.FromButton.Ability.Def.FloatParam1;
      timeCounter = 0f;
      prevPosition = targetPos;
      Log.M.TWL(0,"SelectionStateCommandTargetSinglePoint.ProcessMousePos " + this.FromButton.Ability.Def.Description.Id);
      CombatHUD HUD = this.HUD;
      List<CombatHUDWeaponSlot> wSlots = HUD.WeaponPanel.DisplayedWeaponSlots;
      AbstractActor actor = this.SelectedActor;
      Vector3 groundPos = targetPos;
      groundPos.y = actor.Combat.MapMetaData.GetCellAt(targetPos).cachedHeight;
      distance = Vector3.Distance(actor.CurrentPosition, groundPos);
      Vector3 collisionWorldPos = Vector3.zero;
      LineOfFireLevel LOFLevel = LineOfFireLevel.LOFBlocked;
      bool isInFiringArc = actor.IsTargetPositionInFiringArc(actor, actor.CurrentPosition, actor.CurrentRotation, groundPos);
      if (isInFiringArc) {
        if (NumPositionsLocked == 0) {
          TargetedCombatant = actor.findFriendlyNearPos(groundPos);
        }
      }
      if (TargetedCombatant == null) {
        LOFLevel = actor.Combat.LOFCache.GetLineOfFire(actor, actor.CurrentPosition, actor, groundPos, actor.CurrentRotation, out collisionWorldPos);
      } else {
        LOFLevel = actor.Combat.LOFCache.GetLineOfFire(actor, actor.CurrentPosition, TargetedCombatant, TargetedCombatant.CurrentPosition, TargetedCombatant.CurrentRotation, out collisionWorldPos);
      }
      foreach (CombatHUDWeaponSlot slot in wSlots) {
        if (slot.DisplayedWeapon == null) { continue; }
        if ((slot.weaponSlotType == CombatHUDWeaponSlot.WeaponSlotType.Melee) || (slot.weaponSlotType == CombatHUDWeaponSlot.WeaponSlotType.DFA)) { continue; }
        if (slot.DisplayedWeapon.IsEnabled == false) { continue; }
        if (slot.DisplayedWeapon.HasAmmo == false) { continue; }
        UILookAndColorConstants LookAndColorConstants = slot.LookAndColorConstants();
        float AOERange = slot.DisplayedWeapon.AOERange();
        if (TargetedCombatant == null) {
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
          if ((slot.DisplayedWeapon.isAMS()) || (slot.DisplayedWeapon.WillFireAtTarget(TargetedCombatant) == false)) {
            Log.LogWrite(" " + slot.DisplayedWeapon.UIName + ":disabled\n");
            slot.MainImage.color = LookAndColorConstants.WeaponSlotColors.UnavailableBGColor;
            //mShowTextColor.Invoke(slot, new object[2] { LookAndColorConstants.WeaponSlotColors.UnavailableTextColor, true });
            slot.ShowTextColor(LookAndColorConstants.WeaponSlotColors.UnavailableTextColor);
            slot.ToggleButton.childImage.color = LookAndColorConstants.WeaponSlotColors.UnavailableToggleColor;
            slot.ClearHitChance();
          } else {
            float hitChance = slot.DisplayedWeapon.GetToHitFromPosition(TargetedCombatant, 1, actor.CurrentPosition, TargetedCombatant.CurrentPosition, false, false, false);
            slot.SetHitChance(hitChance);
            Log.LogWrite(" " + slot.DisplayedWeapon.UIName + ":enabled\n");
            slot.MainImage.color = LookAndColorConstants.WeaponSlotColors.AvailableBGColor;
            Color hitChanceColor = slot.GetShotQualityTextColor(hitChance);//(Color)mGetShotQualityTextColor.Invoke(slot, new object[1] { hitChance });
            //mShowTextColor.Invoke(slot, new object[3] { LookAndColorConstants.WeaponSlotColors.SelectedTextColor, hitChanceColor, true });
            slot.ShowTextColor(LookAndColorConstants.WeaponSlotColors.SelectedTextColor, hitChanceColor);
            //this.ShowTextColor(this.LookAndColorConstants.WeaponSlotColors.SelectedTextColor, this.GetShotQualityTextColor(this.HitChance), true);
            //mShowTextColor.Invoke(slot, new object[2] { LookAndColorConstants.WeaponSlotColors.AvailableTextColor, true });
            slot.ToggleButton.childImage.color = LookAndColorConstants.WeaponSlotColors.AvailableToggleColor;
            if (AOERange > result) { result = AOERange; };
          }
        }
      }
      range = result;
      return true;
    }
    public override Vector3 PreviewPos {
      get {
        return this.SelectedActor.CurrentPosition;
      }
    }
    public override void ProcessMousePos(Vector3 worldPos) {
      float range = float.NaN;
      ICombatant target = null;
      switch (this.NumPositionsLocked) {
        case 0:
          if(this.UpdateWeapons(worldPos,out range)) {
            this.CircleRange = range;
            Log.M.TWL(0, "SelectionStateCommandAttackGround.UpdateWeapons "+ NumPositionsLocked + " true range:"+range+" trg:"+(target==null?"null":(new Text(target.DisplayName).ToString())));
          }
          CombatTargetingReticle.Instance.UpdateReticle(worldPos, CircleRange, false);
          break;
        case 1:
          if (this.UpdateWeapons(targetPosition, out range)) {
            this.CircleRange = range;
            Log.M.TWL(0, "SelectionStateCommandAttackGround.UpdateWeapons " + NumPositionsLocked + " true range:" + range + " trg:" + (target == null ? "null" : (new Text(target.DisplayName).ToString())));
          }
          CombatTargetingReticle.Instance.UpdateReticle(this.targetPosition, CircleRange, false);
          break;
      }
      Log.M.TWL(0, "SelectionState_ProcessMousePos");
      if (this.TargetedCombatant == null) {
        this.HUD.SelectionHandler.ActiveState.FiringPreview.ClearAllInfo();
      } else {
        this.HUD.SelectionHandler.ActiveState.FiringPreview.ClearAllInfo();
        List<AbstractActor> allAlliesOf = this.Combat.GetAllAlliesOf(SelectedActor);
        Weapon longestRangeWeapon1 = this.SelectedActor.GetLongestRangeWeapon(false, false);
        float maxRange = longestRangeWeapon1 == null ? 0.0f : longestRangeWeapon1.MaxRange;
        Weapon longestRangeWeapon2 = this.SelectedActor.GetLongestRangeWeapon(false, true);
        float maxIndirectRange = longestRangeWeapon2 == null ? 0.0f : longestRangeWeapon2.MaxRange;
        this.HUD.SelectionHandler.ActiveState.FiringPreview.Recalc(SelectedActor, TargetedCombatant, allAlliesOf, SelectedActor.CurrentPosition, SelectedActor.CurrentRotation, false, false, maxRange, maxIndirectRange);
        FiringPreviewManager.PreviewInfo previewInfo = this.HUD.SelectionHandler.ActiveState.FiringPreview.GetPreviewInfo(TargetedCombatant);
        Log.M.WL(1, "TargetedCombatant.IsDead " + TargetedCombatant.IsDead);
        Log.M.WL(1, "SelectedActor.CurrentPosition " + SelectedActor.CurrentPosition);
        Log.M.WL(1, "TargetedCombatant.CurrentPosition " + TargetedCombatant.CurrentPosition);
        Log.M.WL(1, "previewInfo.collisionPoint " + previewInfo.collisionPoint);
        Log.M.WL(1, "SelectedActor.VisibilityToTargetUnit " + SelectedActor.VisibilityToTargetUnit(TargetedCombatant));
        Log.M.WL(1, "previewInfo.availability "+ previewInfo.availability);
        Log.M.WL(1, "previewInfo.LOFLevel " + previewInfo.LOFLevel);
      }
      this.SelectionState_ProcessMousePos(SelectedActor.CurrentPosition);
      for(int index=0;index < WeaponRangeIndicators.Instance.lines().Count; ++index) {
        LineRenderer line = WeaponRangeIndicators.Instance.lines()[index];
        Vector3[] positions = new Vector3[line.positionCount];
        line.GetPositions(positions);
        Log.M.W(2, "["+index+"] active:" + line.gameObject.activeInHierarchy);
        foreach (Vector3 pos in positions) { Log.M.W(0," "+pos); }
        Log.M.WL(0,"");
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
  [HarmonyPatch(typeof(AttackDirector))]
  [HarmonyPatch("CreateAttackSequence")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(int), typeof(AbstractActor), typeof(ICombatant), typeof(Vector3), typeof(Quaternion), typeof(int), typeof(List<Weapon>), typeof(MeleeAttackType), typeof(int), typeof(bool) })]
  public static class AttackDirector_CreateAttackSequence {
    private static bool f_next_attack_sequence_terrain = false;
    public static bool isSequenceIsTerrainAttack(this AttackDirector attackDirector) { return f_next_attack_sequence_terrain; }
    public static void isSequenceIsTerrainAttack(this AttackDirector attackDirector,bool value) { f_next_attack_sequence_terrain = value; }
    public static void Postfix(AttackDirector __instance,int stackItemUID, AbstractActor attacker, ICombatant target, Vector3 attackPosition, Quaternion attackRotation, int attackSequenceIdx, List<Weapon> selectedWeapons, MeleeAttackType meleeAttackType, int calledShotLocation, bool isMoraleAttack,ref AttackDirector.AttackSequence __result) {
      try {
        Log.M.TWL(0, "AttackDirector.CreateAttackSequence:" + __result.id);
        if (__instance.isSequenceIsTerrainAttack()) {
          Log.M.WL(0, "registering as terrain");
          __result.registerAsTerrainAttack();
          __instance.isSequenceIsTerrainAttack(false);
        }
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(PilotDef))]
  [HarmonyPatch("DependenciesLoaded")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(uint) })]
  public static class PilotDef_DependenciesLoaded {
    public static void Postfix(PilotDef __instance, uint loadWeight, ref bool __result) {
      try {
        Log.M.TWL(0, "PilotDef.DependenciesLoaded " + __instance.Description.Id);
        if (__result == false) { return; }
        if (__instance.DataManager.AbilityDefs.TryGet(CombatHUDMechwarriorTray_InitAbilityButtons.AbilityName, out AbilityDef aDef) == false) {
          __result = false;
        }
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(Pilot))]
  [HarmonyPatch("InitAbilities")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(bool), typeof(bool) })]
  public static class Pilot_InitAbilities {
    public static void Postfix(Pilot __instance, bool ModifyStats, bool FromSave, List<AbilityStateInfo> ___SerializedAbilityStates) {
      try {
        Log.M.TWL(0, "Pilot.InitAbilities " + __instance.Description.Id);
        if (__instance.pilotDef.DataManager.AbilityDefs.TryGet(CombatHUDMechwarriorTray_InitAbilityButtons.AbilityName, out AbilityDef abilityDef) == false) {
          return;
        }
        Ability ability = new Ability(abilityDef);
        AbilityStateInfo LoadedState = (AbilityStateInfo)null;
        CombatGameState Combat = Traverse.Create(__instance).Property<CombatGameState>("Combat").Value;
        if (___SerializedAbilityStates != null)
          LoadedState = ___SerializedAbilityStates.FirstOrDefault<AbilityStateInfo>((Func<AbilityStateInfo, bool>)(x => x.AbilityDefID == abilityDef.Id));
        if (FromSave && LoadedState != null)
          ability.InitFromSave(Combat, LoadedState);
        else
          ability.Init(Combat);
        __instance.Abilities.Add(ability);
        __instance.ActiveAbilities.Add(ability);
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(PilotDef))]
  [HarmonyPatch("GatherDependencies")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(DataManager), typeof(DataManager.DependencyLoadRequest), typeof(uint) })]
  public static class PilotDef_GatherDependencies {
    public static void Postfix(PilotDef __instance, DataManager dataManager, DataManager.DependencyLoadRequest dependencyLoad, uint activeRequestWeight) {
      try {
        Log.M.TWL(0, "PilotDef.GatherDependencies " + __instance.Description.Id);
        dependencyLoad.RequestResource(BattleTechResourceType.AbilityDef, CombatHUDMechwarriorTray_InitAbilityButtons.AbilityName);
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(AttackStackSequence))]
  [HarmonyPatch("OnAttackBegin")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
  public static class AttackStackSequence_OnAttackBegin {
    public static void Postfix(AttackStackSequence __instance, MessageCenterMessage message) {
      try {
        Log.M.TWL(0, "AttackStackSequence.OnAttackBegin");
        CombatGameState Combat = (CombatGameState)typeof(SequenceBase).GetProperty("Combat", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
        AttackDirector.AttackSequence attackSequence = Combat.AttackDirector.GetAttackSequence((message as AttackSequenceBeginMessage).sequenceId);
        if (attackSequence == null) {
          Log.M.WL(1, "Null sequence on attack begin!!!");
          return;
        }
        Log.M.WL(1, "attackSequence.calledShotLocation:"+ attackSequence.calledShotLocation);
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
    }
  }
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
      //if (!__instance.SelectedActor.HasBegunActivation) {
      //__result = !__instance.SelectedActor.StoodUpThisRound;
      //return false;
      //}
      //__result = false;
      //return false;
      return true;
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
        HUD.RegisterAttackGroundTarget(null);
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
      Log.M.TWL(0,"CombatHUD.OnAttackEnd. SelectionForbidden: "+ CombatSelectionHandler_TrySelectActor.SelectionForbidden);
      try {
        if (CombatSelectionHandler_TrySelectActor.SelectionForbidden == true) {
          AttackDirector.AttackSequence attackSequence = __instance.Combat.AttackDirector.GetAttackSequence((message as AttackSequenceEndMessage).sequenceId);
          AbstractActor attacker = null;
          if (attackSequence == null) {
            Log.LogWrite("Can't find sequence with id " + (message as AttackSequenceEndMessage).sequenceId + "\n");
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
              attacker.HasFiredThisRound = true;

              __instance.SelectionHandler?.DeselectActor(__instance.SelectedActor);
              //MethodInfo OnActorSelected = typeof(CombatHUD).GetMethod("OnActorSelected", BindingFlags.NonPublic | BindingFlags.Instance);
              //MethodInfo OnActorDeselected = typeof(CombatHUD).GetMethod("OnActorDeselected", BindingFlags.NonPublic | BindingFlags.Instance);
              //OnActorDeselected.Invoke(__instance, new object[] { __instance.SelectedActor });
              //OnActorSelected.Invoke(__instance, new object[] { attacker });
              __instance.SelectionHandler?.TrySelectActor(attacker, false);
              attacker.HasBegunActivation = HasBegunActivation;
              attacker.HasActivatedThisRound = HasActivatedThisRound;
              __instance.MechWarriorTray.ConfirmAbilities(AbilityDef.ActivationTiming.ConsumedByFiring);
              attacker = null;
            }
          }
          //__instance.Combat.AttackDirector.RemoveAttackSequence(attackSequence.id);

        }
      }catch(Exception e) {
        Log.M.TWL(0, e.ToString(), true);
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
  //[HarmonyPatch(typeof(WeaponRangeIndicators))]
  //[HarmonyPatch("UpdateTargetingLines")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(Vector3), typeof(Quaternion), typeof(bool), typeof(ICombatant), typeof(bool), typeof(List<ICombatant>), typeof(bool) })]
  //public static class WeaponRangeIndicators_UpdateTargetingLines {
  //  private static bool Prefix(WeaponRangeIndicators __instance, AbstractActor selectedActor, Vector3 position, Quaternion rotation, bool isPositionLocked, ICombatant targetedActor, bool useMultiFire, List<ICombatant> lockedTargets, bool isMelee) {
  //    Log.M.TWL(0, "WeaponRangeIndicators.UpdateTargetingLines position:" + position + " actor:" + new Text(selectedActor.DisplayName).ToString() + " trg:" + (targetedActor==null?"null":(new Text(targetedActor.DisplayName).ToString())));
  //    return true;
  //  }
  //}
  [HarmonyPatch(typeof(WeaponRangeIndicators))]
  [HarmonyPatch("ShowLineToTarget")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(Vector3), typeof(Quaternion), typeof(AbstractActor), typeof(bool) })]
  public static class WeaponRangeIndicators_ShowLineToTarget {
    private static bool Prefix(WeaponRangeIndicators __instance, AbstractActor selectedActor, Vector3 position, Quaternion rotation, AbstractActor targetedActor, bool isMelee) {
      Log.M.TWL(0, "WeaponRangeIndicators.ShowLineToTarget position:"+position+" actor:" + new Text(selectedActor.DisplayName).ToString() + " trg:" + (targetedActor == null ? "null" : (new Text(targetedActor.DisplayName).ToString())));
      return true;
    }
  }
  [HarmonyPatch(typeof(FiringPreviewManager))]
  [HarmonyPatch("HasLOS")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(ICombatant), typeof(Vector3), typeof(List<AbstractActor>) })]
  public static class FiringPreviewManager_HasLOS {
    private static void Postfix(FiringPreviewManager __instance, AbstractActor attacker, ICombatant target, Vector3 position, List<AbstractActor> allies, ref bool __result) {
      if (__result == true) { return; }
      if (target == null) { return; }
      if (target.team == null) { return; }
      if (attacker == null) { return; }
      if (attacker.team == null) { return; }
      if (attacker.team.IsFriendly(target.team)) { __result = true; return; }
    }
  }
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("VisibilityToTargetUnit")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ICombatant) })]
  public static class AbstractActor_VisibilityToTargetUnit {
    private static void Postfix(AbstractActor __instance, ICombatant targetUnit, ref VisibilityLevel __result) {
      if (__result > VisibilityLevel.None) { return; }
      if (targetUnit == null) { return; }
      if (targetUnit.team == null) { return; }
      if (__instance == null) { return; }
      if (__instance.team == null) { return; }
      if (__instance.team.IsFriendly(targetUnit.team)) { __result = VisibilityLevel.LOSFull; return; }
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
      //return true;
      AttackCompleteMessage attackCompleteMessage = (AttackCompleteMessage)message;
      Log.M.TWL(0,"AttackDirector.OnAttackCompleteTA:" + attackCompleteMessage.sequenceId + "\n");
      int sequenceId = attackCompleteMessage.sequenceId;
      AttackDirector.AttackSequence attackSequence = __instance.GetAttackSequence(sequenceId);
      if (attackSequence == null) {
        return true;
      }
      if ((attackSequence.chosenTarget.GUID == attackSequence.attacker.GUID)||(attackSequence.isTerrainAttackSequence())) {
        Log.LogWrite(" terrain attack detected id:"+ attackSequence.id+ "\n");
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
        if (gameRep != null) {
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
        //JammingEnabler.jamm(__state);
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
    private static PropertyInfo pLookAndColorConstants = null;
    private static PropertyInfo pNumPositionsLocked = null;
    private static PropertyInfo pTargetPosition = null;
    private static MethodInfo mShowTextColor = null;
#if BT1_8
    private static MethodInfo mGetWeaponCOILStateColor = null;
#endif
    private static MethodInfo mGetShotQualityTextColor = null;
    private static FieldInfo fCombat = null;
    private delegate void ShowLineToTargetDelegate(WeaponRangeIndicators indicators, AbstractActor selectedActor, Vector3 position, Quaternion rotation, AbstractActor targetedActor, bool isMelee);
    private static ShowLineToTargetDelegate ShowLineToTargetInvoker = null;
    private delegate void HideAllLinesDelegate(WeaponRangeIndicators indicators, CombatGameState Combat);
    private static HideAllLinesDelegate HideAllLinesInvoker = null;
    private delegate void HideUnusedElementsDelegate(WeaponRangeIndicators indicators);
    private static HideUnusedElementsDelegate HideUnusedElementsInvoker = null;
    private delegate Color GetShotQualityTextColorDelegate(CombatHUDWeaponSlot slot, float toHitChance);
    private static GetShotQualityTextColorDelegate GetShotQualityTextColorInvoker = null;
    private delegate void ProcessMousePosDelegate(SelectionState state,Vector3 worldPos);
    private static ProcessMousePosDelegate ProcessMousePosInvoker = null;
    private delegate void RecalcDelegate(FiringPreviewManager instance, AbstractActor attacker, ICombatant target, List<AbstractActor> allies, Vector3 position, Quaternion rotation, bool canRotate, bool isJump, float maxRange, float maxIndirectRange);
    private static RecalcDelegate RecalcInvoker = null;
    private static FieldInfo f_lines = null;
    public static bool Prepare() {
      try {
        f_lines = typeof(WeaponRangeIndicators).GetField("lines",BindingFlags.Instance|BindingFlags.NonPublic);
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
        {
          MethodInfo method = typeof(WeaponRangeIndicators).GetMethod("ShowLineToTarget",BindingFlags.NonPublic|BindingFlags.Instance);
          var dm = new DynamicMethod("CACShowLineToTarget", null, new Type[] { typeof(WeaponRangeIndicators), typeof(AbstractActor), typeof(Vector3), typeof(Quaternion), typeof(AbstractActor), typeof(bool) }, typeof(WeaponRangeIndicators));
          var gen = dm.GetILGenerator();
          gen.Emit(OpCodes.Ldarg_0);
          gen.Emit(OpCodes.Ldarg_1);
          gen.Emit(OpCodes.Ldarg_2);
          gen.Emit(OpCodes.Ldarg_3);
          gen.Emit(OpCodes.Ldarg_S, 4);
          gen.Emit(OpCodes.Ldarg_S, 5);
          gen.Emit(OpCodes.Call, method);
          gen.Emit(OpCodes.Ret);
          ShowLineToTargetInvoker = (ShowLineToTargetDelegate)dm.CreateDelegate(typeof(ShowLineToTargetDelegate));
        }
        {
          MethodInfo method = typeof(WeaponRangeIndicators).GetMethod("HideAllLines", BindingFlags.NonPublic | BindingFlags.Instance);
          var dm = new DynamicMethod("CACHideAllLines", null, new Type[] { typeof(WeaponRangeIndicators), typeof(CombatGameState) }, typeof(WeaponRangeIndicators));
          var gen = dm.GetILGenerator();
          gen.Emit(OpCodes.Ldarg_0);
          gen.Emit(OpCodes.Ldarg_1);
          gen.Emit(OpCodes.Call, method);
          gen.Emit(OpCodes.Ret);
          HideAllLinesInvoker = (HideAllLinesDelegate)dm.CreateDelegate(typeof(HideAllLinesDelegate));
        }
        {
          MethodInfo method = typeof(WeaponRangeIndicators).GetMethod("HideUnusedElements", BindingFlags.NonPublic | BindingFlags.Instance);
          var dm = new DynamicMethod("CACHideUnusedElements", null, new Type[] { typeof(WeaponRangeIndicators) }, typeof(WeaponRangeIndicators));
          var gen = dm.GetILGenerator();
          gen.Emit(OpCodes.Ldarg_0);
          gen.Emit(OpCodes.Call, method);
          gen.Emit(OpCodes.Ret);
          HideUnusedElementsInvoker = (HideUnusedElementsDelegate)dm.CreateDelegate(typeof(HideUnusedElementsDelegate));
        }
        {
          MethodInfo method = typeof(CombatHUDWeaponSlot).GetMethod("GetShotQualityTextColor", BindingFlags.NonPublic | BindingFlags.Instance);
          var dm = new DynamicMethod("CACGetShotQualityTextColor", typeof(Color), new Type[] { typeof(CombatHUDWeaponSlot),typeof(float) }, typeof(CombatHUDWeaponSlot));
          var gen = dm.GetILGenerator();
          gen.Emit(OpCodes.Ldarg_0);
          gen.Emit(OpCodes.Ldarg_1);
          gen.Emit(OpCodes.Call, method);
          gen.Emit(OpCodes.Ret);
          GetShotQualityTextColorInvoker = (GetShotQualityTextColorDelegate)dm.CreateDelegate(typeof(GetShotQualityTextColorDelegate));
        }
        {
          MethodInfo method = typeof(SelectionState).GetMethod("ProcessMousePos", BindingFlags.Public | BindingFlags.Instance);
          var dm = new DynamicMethod("CACProcessMousePos", null, new Type[] { typeof(SelectionState), typeof(Vector3) }, typeof(SelectionState));
          var gen = dm.GetILGenerator();
          gen.Emit(OpCodes.Ldarg_0);
          gen.Emit(OpCodes.Ldarg_1);
          gen.Emit(OpCodes.Call, method);
          gen.Emit(OpCodes.Ret);
          ProcessMousePosInvoker = (ProcessMousePosDelegate)dm.CreateDelegate(typeof(ProcessMousePosDelegate));
        }
        {
          MethodInfo method = typeof(FiringPreviewManager).GetMethod("Recalc", BindingFlags.NonPublic | BindingFlags.Instance);
          var dm = new DynamicMethod("CACRecalc", null, new Type[] { typeof(FiringPreviewManager), typeof(AbstractActor), typeof(ICombatant), typeof(List<AbstractActor>), typeof(Vector3), typeof(Quaternion), typeof(bool), typeof(bool), typeof(float), typeof(float) }, typeof(FiringPreviewManager));
          var gen = dm.GetILGenerator();
          gen.Emit(OpCodes.Ldarg_0);
          gen.Emit(OpCodes.Ldarg_1);
          gen.Emit(OpCodes.Ldarg_2);
          gen.Emit(OpCodes.Ldarg_3);
          gen.Emit(OpCodes.Ldarg_S, 4);
          gen.Emit(OpCodes.Ldarg_S, 5);
          gen.Emit(OpCodes.Ldarg_S, 6);
          gen.Emit(OpCodes.Ldarg_S, 7);
          gen.Emit(OpCodes.Ldarg_S, 8);
          gen.Emit(OpCodes.Ldarg_S, 9);
          gen.Emit(OpCodes.Call, method);
          gen.Emit(OpCodes.Ret);
          RecalcInvoker = (RecalcDelegate)dm.CreateDelegate(typeof(RecalcDelegate));
        }
      } catch (Exception e) {
        Log.LogWrite(e.ToString() + "\n");
        return false;
      }
      return true;
    }
    public static List<LineRenderer> lines(this WeaponRangeIndicators wri) {
      return (List<LineRenderer>)f_lines.GetValue(wri);
    }
    public static void Recalc(this FiringPreviewManager instance, AbstractActor attacker, ICombatant target, List<AbstractActor> allies, Vector3 position, Quaternion rotation, bool canRotate, bool isJump, float maxRange, float maxIndirectRange) {
      RecalcInvoker(instance, attacker, target, allies, position, rotation, canRotate, isJump, maxRange, maxIndirectRange);
    }
    public static CombatGameState Combat(this Weapon weapon) {
      return (CombatGameState)fCombat.GetValue(weapon);
    }
    public static Color GetShotQualityTextColor(this CombatHUDWeaponSlot slot, float toHitChance) {
      return GetShotQualityTextColorInvoker(slot, toHitChance);
    }
    public static void SelectionState_ProcessMousePos(this SelectionState state, Vector3 worldPos) {
      ProcessMousePosInvoker(state, worldPos);
    }
    public static ICombatant findFriendlyNearPos(this AbstractActor actor, Vector3 worldPos, bool ignoreAlt = false) {
      ICombatant result = null;
      float resultDist = 0f;
      if (ignoreAlt == false) { if (Input.GetKey(KeyCode.LeftAlt) == false) { return null; }; };
      Log.LogWrite(" findFriendlyNearPos " + worldPos + "\n");
      foreach (ICombatant friend in actor.Combat.GetAllCombatants()) {
        if (friend.IsDead) { continue; }
        if ((friend.team != null) && (actor.team != null)) {
          if (friend.team.IsEnemy(actor.team)) { continue; }
        }
        float distance = Vector3.Distance(friend.CurrentPosition, worldPos);
        Log.LogWrite("  friend:" + friend.DisplayName + " " + distance + "\n");
        if (distance > CustomAmmoCategories.Settings.TerrainFiendlyFireRadius) { continue; }
        if ((distance < resultDist) || (result == null)) { result = friend; resultDist = distance; }
      }
      Log.LogWrite("  result: " + (result == null ? "null" : result.DisplayName) + "\n");
      return result;
    }
    public static UILookAndColorConstants LookAndColorConstants(this CombatHUDWeaponSlot slot) {
      return (UILookAndColorConstants)pLookAndColorConstants.GetValue(slot, null);
    }
    //Color hitChanceColor = (Color)mGetShotQualityTextColor.Invoke(slot, new object[1] { hitChance });
    //public static UILookAndColorConstants LookAndColorConstants(this CombatHUDWeaponSlot slot) {
    //return (UILookAndColorConstants)pLookAndColorConstants.GetValue(slot, null);
    //}
    public static bool WillFireToPosition(this Weapon weapon, Vector3 position) {
      if (weapon.CanFire == false) {
        Log.M.WL(1, "weapon can not fire");
        return false;
      }
      bool result = weapon.Combat().LOFCache.UnitHasLOFToTargetAtTargetPosition(weapon.parent, weapon.parent, weapon.MaxRange,
        weapon.parent.CurrentPosition, weapon.parent.CurrentRotation, position, weapon.parent.CurrentRotation, weapon.IndirectFireCapable());
      Log.M.WL(1, "UnitHasLOFToTargetAtTargetPosition " + result + " max range:" + weapon.MaxRange);
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
    private static ICombatant attackGroundTarget = null;
    private static HashSet<int> terrainAttacksSequenceIds = new HashSet<int>();
    public static bool isTerrainAttackSequence(this AttackDirector.AttackSequence seq) { return terrainAttacksSequenceIds.Contains(seq.id); }
    public static void registerAsTerrainAttack(this AttackDirector.AttackSequence seq) { terrainAttacksSequenceIds.Add(seq.id); }
    public static void RegisterAttackGroundTarget(this CombatHUD HUD, ICombatant target) { attackGroundTarget = target; }
    public static void Postfix(CombatHUDActionButton __instance, string teamGUID, Vector3 targetPosition) {
      Log.LogWrite("CombatHUDActionButton.ActivateCommandAbility " + __instance.GUID + "\n");
      return;
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
    //public void OnAbilityLoad() {
    //  abilityDef = actor.Combat.DataManager.AbilityDefs.Get(CombatHUDMechwarriorTray_InitAbilityButtons.AbilityName);
    //  Log.LogWrite("InitAbilityButtonsDelegate.OnAbilityLoad "+abilityDef.Description.Id+"\n");
    //  if (abilityDef.DependenciesLoaded(1000u)) {
    //    Log.LogWrite(" dependencies fully loaded\n");
    //    OnAbilityFullLoad();
    //  } else {
    //    Log.LogWrite(" request dependencies\n");
    //    DataManager.InjectedDependencyLoadRequest dependencyLoad = new DataManager.InjectedDependencyLoadRequest(actor.Combat.DataManager);
    //    abilityDef.GatherDependencies(actor.Combat.DataManager, dependencyLoad, 1000U);
    //    dependencyLoad.RegisterLoadCompleteCallback(new Action(OnAbilityFullLoad));
    //    actor.Combat.DataManager.InjectDependencyLoader(dependencyLoad, 1000U);
    //  }
    //}
  }
  [HarmonyPatch(typeof(CombatHUDMechwarriorTray))]
  [HarmonyPatch("InitAbilityButtons")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor) })]
  public static class CombatHUDMechwarriorTray_InitAbilityButtons {
    public static readonly string AbilityName = "AbilityDefCAC_AttackGround";
    public static Dictionary<string, Ability> attackGroundAbilities = new Dictionary<string, Ability>();
    public static void InitAttackGroundAbilityButton(AbstractActor actor, Ability ability, CombatHUDActionButton button) {
      Log.LogWrite("InitAttackGroundAbilityButton " + ability.Def.Description.Id + "\n");
      Log.LogWrite(" aDef.Targeting = " + ability.Def.Targeting + "\n");
      Log.LogWrite(" aDef.AbilityIcon = " + ability.Def.AbilityIcon + "\n");
      Log.LogWrite(" aDef.Description.Name = " + ability.Def.Description.Name + "\n");
      button.InitButton(
        (SelectionType)typeof(CombatHUDMechwarriorTray).GetMethod("GetSelectionTypeFromTargeting", BindingFlags.Static | BindingFlags.Public).Invoke(
          null, new object[2] { (object)ability.Def.Targeting, (object)false }
        ),
        ability, ability.Def.AbilityIcon,
        "ID_ATTACKGROUND",
        ability.Def.Description.Name,
        actor
      );
      Log.LogWrite(" init button success\n");
      Log.LogWrite(" button.GUID = " + button.GUID + "\n");
      button.isClickable = true;
      button.RefreshUIColors();
      Log.LogWrite("finished\n");
    }
    public static void Postfix(CombatHUDMechwarriorTray __instance, AbstractActor actor) {
      Log.LogWrite("CombatHUDMechwarriorTray.InitAbilityButtons\n");
      if (CustomAmmoCategories.Settings.ShowAttackGroundButton == false) { return; }
      //AbilityDef aDef = null;
      //InitAbilityButtonsDelegate id = new InitAbilityButtonsDelegate(__instance, actor);
      //if (actor.Combat.DataManager.AbilityDefs.TryGet("AbilityDefCAC_AttackGround", out aDef) == false) {
        //Log.LogWrite(" requesting ability def loading\n");
        //DataManager.InjectedDependencyLoadRequest dependencyLoad = new DataManager.InjectedDependencyLoadRequest(actor.Combat.DataManager);
        //dependencyLoad.RequestResource(BattleTechResourceType.AbilityDef, AbilityName);
        //dependencyLoad.RegisterLoadCompleteCallback(new Action(id.OnAbilityLoad));
        //actor.Combat.DataManager.InjectDependencyLoader(dependencyLoad, 1000U);
      //} else {
        //Log.LogWrite(" ability def already loaded\n");
        //id.OnAbilityLoad();
      //}
    }
  }
  [HarmonyPatch(typeof(CombatHUDMechwarriorTray))]
  [HarmonyPatch("ResetAbilityButtons")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor) })]
  public static class CombatHUDMechwarriorTray_ResetAbilityButtons {
    public delegate void d_ResetAbilityButton(CombatHUDMechwarriorTray tray, AbstractActor actor, CombatHUDActionButton button, Ability ability, bool forceInactive);
    private static d_ResetAbilityButton i_ResetAbilityButton = null;
    public static bool Prepare() {
      {
        MethodInfo ResetAbilityButton = typeof(CombatHUDMechwarriorTray).GetMethod("ResetAbilityButton", BindingFlags.Instance | BindingFlags.NonPublic);
        var dm = new DynamicMethod("CACResetAbilityButton", null, new Type[] { typeof(CombatHUDMechwarriorTray), typeof(AbstractActor), typeof(CombatHUDActionButton), typeof(Ability), typeof(bool) }, typeof(CombatHUDMechwarriorTray));
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Ldarg_2);
        gen.Emit(OpCodes.Ldarg_3);
        gen.Emit(OpCodes.Ldarg_S, 4);
        gen.Emit(OpCodes.Call, ResetAbilityButton);
        gen.Emit(OpCodes.Ret);
        i_ResetAbilityButton = (d_ResetAbilityButton)dm.CreateDelegate(typeof(d_ResetAbilityButton));
      }
      return true;
    }
    public static void ResetAbilityButton(this CombatHUDMechwarriorTray tray, AbstractActor actor, CombatHUDActionButton button, Ability ability, bool forceInactive) {
      i_ResetAbilityButton(tray, actor, button, ability, forceInactive);
    }
    public static void ResetAttackGroundButton(this CombatHUDMechwarriorTray tray,AbstractActor actor, Ability ability, CombatHUDActionButton button, bool forceInactive) {
      CustomAmmoCategoriesLog.Log.LogWrite("ResetAttackGroundButton:" + actor.DisplayName + "\n");
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
    public static void Postfix(CombatHUDMechwarriorTray __instance, AbstractActor actor) {
      CustomAmmoCategoriesLog.Log.LogWrite("CombatHUDMechwarriorTray.ResetAbilityButtons\n");
      return;
      //AbilityDef aDef = null;
      //if (actor.Combat.DataManager.AbilityDefs.TryGet("AbilityDefCAC_AttackGround", out aDef)) {
      //  Ability gaAbility = null;
      //  if (CombatHUDMechwarriorTray_InitAbilityButtons.attackGroundAbilities.ContainsKey(actor.GUID) == false) {
      //    gaAbility = new Ability(aDef);
      //    gaAbility.Init(actor.Combat);
      //    CombatHUDMechwarriorTray_InitAbilityButtons.attackGroundAbilities.Add(actor.GUID, gaAbility);
      //  } else {
      //    gaAbility = CombatHUDMechwarriorTray_InitAbilityButtons.attackGroundAbilities[actor.GUID];
      //  }
      //  bool forceInactive = actor.HasActivatedThisRound || actor.MovingToPosition != null || actor.Combat.StackManager.IsAnyOrderActive && actor.Combat.TurnDirector.IsInterleaved;
      //  CustomAmmoCategoriesLog.Log.LogWrite(" actor.HasActivatedThisRound:" + actor.HasActivatedThisRound + "\n");
      //  CustomAmmoCategoriesLog.Log.LogWrite(" actor.MovingToPosition:" + (actor.MovingToPosition != null) + "\n");
      //  CustomAmmoCategoriesLog.Log.LogWrite(" actor.Combat.StackManager.IsAnyOrderActive:" + actor.Combat.StackManager.IsAnyOrderActive + "\n");
      //  CustomAmmoCategoriesLog.Log.LogWrite(" actor.Combat.TurnDirector.IsInterleaved:" + actor.Combat.TurnDirector.IsInterleaved + "\n");
      //  CustomAmmoCategoriesLog.Log.LogWrite(" forceInactive:" + forceInactive + "\n");
      //  CombatHUDActionButton[] AbilityButtons = (CombatHUDActionButton[])typeof(CombatHUDMechwarriorTray).GetProperty("AbilityButtons", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance, null);
      //  typeof(CombatHUDMechwarriorTray).GetMethod("ResetAbilityButton", BindingFlags.NonPublic | BindingFlags.Instance, null,
      //    new Type[] { typeof(AbstractActor), typeof(CombatHUDActionButton), typeof(Ability), typeof(bool) }, null
      //  ).Invoke(__instance, new object[4] { (object)actor, AbilityButtons[AbilityButtons.Length - 1], gaAbility, forceInactive });
      //  if (forceInactive) { AbilityButtons[AbilityButtons.Length - 1].DisableButton(); };
      //  if (actor.Combat.TurnDirector.IsInterleaved == false) {
      //    if (actor.HasFiredThisRound == false) {
      //      if (gaAbility.IsActive == false) {
      //        if (gaAbility.IsAvailable == true) {
      //          if (actor.IsShutDown == false) {
      //            CustomAmmoCategoriesLog.Log.LogWrite(" ResetButtonIfNotActive:\n");
      //            CustomAmmoCategoriesLog.Log.LogWrite(" IsAbilityActivated:" + AbilityButtons[AbilityButtons.Length - 1].IsAbilityActivated + "\n");
      //            if (actor.MovingToPosition == null) { AbilityButtons[AbilityButtons.Length - 1].ResetButtonIfNotActive(actor); };
      //          }
      //        }
      //      }
      //    } else {
      //      AbilityButtons[AbilityButtons.Length - 1].DisableButton();
      //    }
      //  }
      //} else {
      //  CustomAmmoCategoriesLog.Log.LogWrite("Can't find AbilityDef CAC_AttackGround\n");
      //}
    }
  }
}