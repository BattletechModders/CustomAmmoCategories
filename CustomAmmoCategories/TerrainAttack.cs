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
using HarmonyLib;
using HBS;
using HBS.Collections;
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
      if (this.SelectedActor.GameRep == null) { return; };
      MechRepresentation gameRep = this.SelectedActor.GameRep as MechRepresentation;
      if (gameRep == null) { return; };
      Log.Combat?.WL(0,"SelectionStateCommandAttackGround.OnAddToStack ToggleRandomIdles false");
      gameRep.ToggleRandomIdles(false);
    }
    public override void OnInactivate() {
      Log.Combat?.TWL(0,"SelectionStateCommandAttackGround.OnInactivate HasActivated: "+ HasActivated);
      base.OnInactivate();
      CameraControl.Instance.ClearTargets();
      if (this.SelectedActor.GameRep == null) {
        Log.Combat?.WL(1, "GameRep is null");
        return;
      }
      MechRepresentation gameRep = this.SelectedActor.GameRep as MechRepresentation;
      if (gameRep == null) {
        Log.Combat?.WL(1, "not a mech");
        return;
      }
      Log.Combat?.WL(1,"ToggleRandomIdles "+!HasActivated);
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
      Log.Combat?.TWL(0,"Attack orders set:"+Orders.GetType().ToString()+" GUID:"+Orders.SequenceGUID);
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
            Log.Combat?.WL(1,"weapon " + weapon.UIName + " will NOT fire at " + groundPos + " IndirectFireCapable:" + weapon.IndirectFireCapable());
            continue;
          } else {
            Log.Combat?.WL(1, "weapon " + weapon.UIName + " will fire at " + groundPos + " IndirectFireCapable:" + weapon.IndirectFireCapable());
          }
        } else {
          bool ret = weapon.WillFireAtTargetFromPosition(target, actor.CurrentPosition);
          Log.Combat?.WL(1, "weapon " + weapon.UIName + " will fire at target " + target.DisplayName + " " + ret);
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
      bool is_artillery_strike = false;
      Vector3 position = groundPos;
      if(target != null) { position = target.CurrentPosition; }
      foreach(var weapon in weaponsList) {
        bool is_art = weapon.IsArtillery();
        Log.Combat?.WL(1, $"weapon:{weapon.defId} IsArtillery:{is_art}");
        if(is_art) {
          Log.Combat?.WL(2, $"artillery strike:{position}");
          is_artillery_strike = true; weapon.AddArtilleryStrike(position);
        }
      }
      MessageCenterMessage invocation = null;
      if(is_artillery_strike) {
        actor.ToArtilleryMode();
        invocation = new ReserveActorInvocation(actor, ReserveActorAction.DONE, this.Combat.TurnDirector.CurrentRound);
      } else {
        Vector3 collisionWorldPos;
        LineOfFireLevel LOFLevel = LineOfFireLevel.LOFBlocked;
        if(target != null) {
          LOFLevel = actor.Combat.LOFCache.GetLineOfFire(actor, actor.CurrentPosition, target, target.CurrentPosition, target.CurrentRotation, out collisionWorldPos);
        } else {
          LOFLevel = actor.Combat.LOFCache.GetLineOfFire(actor, actor.CurrentPosition, actor, groundPos, actor.CurrentRotation, out collisionWorldPos);
        }
        MechRepresentation gameRep = actor.GameRep as MechRepresentation;
        if(gameRep != null) {
          Log.Combat?.WL(1, "ToggleRandomIdles false");
          gameRep.ToggleRandomIdles(false);
        }
        if(target == null) { target = actor; };
        if(actor.GUID == target.GUID) {
          Log.Combat?.WL(1, "Registering terrain attack to " + actor.GUID);
          CustomAmmoCategories.addTerrainHitPosition(actor, groundPos, LOFLevel < LineOfFireLevel.LOFObstructed);
        } else {
          Log.Combat?.WL(1, "Registering friendly attack to " + new Text(target.DisplayName));
        }
        invocation = new AttackInvocation(this.SelectedActor, target, weaponsList);
        if(invocation == null) {
          Log.Combat?.WL(1, "No invocation created");
          return false;
        }
        CombatSelectionHandler_TrySelectActor.SelectionForbidden = true;
        this.Combat.AttackDirector.isSequenceIsTerrainAttack(true);
      }
      this.HUD.MechWarriorTray.ConfirmAbilities(AbilityDef.ActivationTiming.ConsumedByMovement);
      this.HUD.MechWarriorTray.ConfirmAbilities(AbilityDef.ActivationTiming.ConsumedByFiring);
      ReceiveMessageCenterMessage subscriber = (ReceiveMessageCenterMessage)(message => this.Orders = (message as AddSequenceToStackMessage).sequence);
      this.Combat.MessageCenter.AddSubscriber(MessageCenterMessageType.AddSequenceToStackMessage, subscriber);
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
      if (this.SelectedActor.GameRep != null) {
        MechRepresentation gameRep = this.SelectedActor.GameRep as MechRepresentation;
        if (gameRep != null) {
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
      Log.Combat?.TWL(0,"SelectionStateCommandTargetSinglePoint.ProcessMousePos " + this.FromButton.Ability.Def.Description.Id);
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
        UILookAndColorConstants LookAndColorConstants = slot.LookAndColorConstants;
        float AOERange = slot.DisplayedWeapon.AOERange();
        if (TargetedCombatant == null) {
          slot.ClearHitChance();
          if ((slot.DisplayedWeapon.isAMS()) || (isInFiringArc == false) || (distance > slot.DisplayedWeapon.MaxRange) || ((LOFLevel < LineOfFireLevel.LOFObstructed) && (slot.DisplayedWeapon.IndirectFireCapable() == false))) {
            Log.Combat?.WL(1, slot.DisplayedWeapon.UIName + ":disabled");
            slot.MainImage.color = LookAndColorConstants.WeaponSlotColors.UnavailableBGColor;
            //mShowTextColor.Invoke(slot, new object[2] { LookAndColorConstants.WeaponSlotColors.UnavailableTextColor, true });
            slot.ShowTextColor(LookAndColorConstants.WeaponSlotColors.UnavailableTextColor);
            slot.ToggleButton.childImage.color = LookAndColorConstants.WeaponSlotColors.UnavailableToggleColor;
          } else {
            Log.Combat?.WL(1, slot.DisplayedWeapon.UIName + ":enabled");
            slot.MainImage.color = LookAndColorConstants.WeaponSlotColors.AvailableBGColor;
            //mShowTextColor.Invoke(slot, new object[2] { LookAndColorConstants.WeaponSlotColors.AvailableTextColor, true });
            slot.ShowTextColor(LookAndColorConstants.WeaponSlotColors.AvailableTextColor);
            slot.ToggleButton.childImage.color = LookAndColorConstants.WeaponSlotColors.AvailableToggleColor;
            if (AOERange > result) { result = AOERange; };
          }
        } else {
          if ((slot.DisplayedWeapon.isAMS()) || (slot.DisplayedWeapon.WillFireAtTarget(TargetedCombatant) == false)) {
            Log.Combat?.WL(1, slot.DisplayedWeapon.UIName + ":disabled");
            slot.MainImage.color = LookAndColorConstants.WeaponSlotColors.UnavailableBGColor;
            //mShowTextColor.Invoke(slot, new object[2] { LookAndColorConstants.WeaponSlotColors.UnavailableTextColor, true });
            slot.ShowTextColor(LookAndColorConstants.WeaponSlotColors.UnavailableTextColor);
            slot.ToggleButton.childImage.color = LookAndColorConstants.WeaponSlotColors.UnavailableToggleColor;
            slot.ClearHitChance();
          } else {
            float hitChance = slot.DisplayedWeapon.GetToHitFromPosition(TargetedCombatant, 1, actor.CurrentPosition, TargetedCombatant.CurrentPosition, false, false, false);
            slot.SetHitChance(hitChance);
            Log.Combat?.WL(1, slot.DisplayedWeapon.UIName + ":enabled");
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
            //Log.Combat?.TWL(0, "SelectionStateCommandAttackGround.UpdateWeapons "+ NumPositionsLocked + " true range:"+range+" trg:"+(target==null?"null":(new Text(target.DisplayName).ToString())));
          }
          CombatTargetingReticle.Instance.UpdateReticle(worldPos, CircleRange, false);
          break;
        case 1:
          if (this.UpdateWeapons(targetPosition, out range)) {
            this.CircleRange = range;
            //Log.Combat?.TWL(0, "SelectionStateCommandAttackGround.UpdateWeapons " + NumPositionsLocked + " true range:" + range + " trg:" + (target == null ? "null" : (new Text(target.DisplayName).ToString())));
          }
          CombatTargetingReticle.Instance.UpdateReticle(this.targetPosition, CircleRange, false);
          break;
      }
      //Log.Combat?.TWL(0, "SelectionState_ProcessMousePos");
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
        Log.Combat?.WL(1, "TargetedCombatant.IsDead " + TargetedCombatant.IsDead);
        Log.Combat?.WL(1, "SelectedActor.CurrentPosition " + SelectedActor.CurrentPosition);
        Log.Combat?.WL(1, "TargetedCombatant.CurrentPosition " + TargetedCombatant.CurrentPosition);
        Log.Combat?.WL(1, "previewInfo.collisionPoint " + previewInfo.collisionPoint);
        Log.Combat?.WL(1, "SelectedActor.VisibilityToTargetUnit " + SelectedActor.VisibilityToTargetUnit(TargetedCombatant));
        Log.Combat?.WL(1, "previewInfo.availability "+ previewInfo.availability);
        Log.Combat?.WL(1, "previewInfo.LOFLevel " + previewInfo.LOFLevel);
      }
      this.SelectionState_ProcessMousePos(SelectedActor.CurrentPosition);
      for(int index=0;index < WeaponRangeIndicators.Instance.lines.Count; ++index) {
        LineRenderer line = WeaponRangeIndicators.Instance.lines[index];
        Vector3[] positions = new Vector3[line.positionCount];
        line.GetPositions(positions);
        Log.Combat?.W(2, "["+index+"] active:" + line.gameObject.activeInHierarchy);
        foreach (Vector3 pos in positions) { Log.Combat?.W(0," "+pos); }
        Log.Combat?.WL(0,"");
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
  public class VirtualCombatant :ICombatant {
    public AbstractActor parent { get; set; }
    public TaggedObjectType Type { get { return parent.Type; } }
    public string DisplayName { get { return parent.DisplayName; } }
    public TagSet EncounterTags { get; set; }
    public VirtualCombatant(AbstractActor p) { this.parent = p; this.StatCollection = new StatCollection(); EncounterTags = new TagSet(); }
    public CombatGameState Combat { get { return parent.Combat; } }
    public GameRepresentation GameRep { get { return parent.GameRep; } }
    public PilotableActorDef PilotableActorDef { get { return parent.PilotableActorDef; } }
    public bool IsTabTarget { get { return parent.IsTabTarget; } }
    public bool NeedsWorldActorElements { get { return parent.NeedsWorldActorElements; } }
    public bool IsForcedVisible { get; set; }
    public Vector3 CurrentPosition { get; set; }
    public Vector3 TargetPosition { get { return this.CurrentPosition; } }
    public Quaternion CurrentRotation { get; set; }
    public Vector3[] LOSTargetPositions { get { return this.parent.LOSSourcePositions; } }
    public Vector3[] GetLOSTargetPositions(Vector3 position, Quaternion rotation) { return new Vector3[] { position }; }
    public Team team { get { return this.parent.team; } }
    public void AddToTeam(Team team) { }
    public void RemoveFromTeam() { }
    public string uid { get { return parent.uid; } }
    public string LogDisplayName { get { return parent.LogDisplayName; } }
    public DescriptionDef Description { get { return parent.Description; } }
    public UnitType UnitType { get { return parent.UnitType; } }
    public bool IsPilotable { get { return false; } }
    public Pilot GetPilot() { return parent.GetPilot(); }
    public int LastTargetedPhaseNumber { get; set; }
    public Dictionary<string, int> LastTargetedPhaseNumberByAttackerGUID { get; set; }
    public StatCollection StatCollection { get; set; }
    public List<VFXEffect.StoredVFXEffectData> VFXDataFromLoad { get; set; }
    public bool CreateEffect(EffectData effect, Ability fromAbility, string sourceID, int stackItemUID, AbstractActor creator, bool skipLogging = false) { return false; }
    public bool CreateEffect(EffectData effect, Ability fromAbility, string sourceID, int stackItemUID, Team creator, bool skipLogging = false) { return false; }
    public void CancelEffect(Effect effect, bool skipLogging = false) { }
    public void OnEffectEnd(Effect effect) { }
    public bool IsInRegion(string regionGuid) { return false; }
    public float ArmorForLocation(int loc) { return 0f; }
    public float MaxArmorForLocation(int loc) { return 0f; }
    public float StructureForLocation(int loc) { return 0f; }
    public float MaxStructureForLocation(int loc) { return 0f; }
    public float StartingArmor { get { return 0f; } }
    public float CurrentArmor { get { return 0f; } }
    public float SummaryArmorMax { get { return 0f; } }
    public float SummaryArmorCurrent { get { return 0f; } }
    public float StartingStructure { get { return 0f; } }
    public float SummaryStructureMax { get { return 0f; } }
    public float SummaryStructureCurrent { get { return 0f; } }
    public bool IsAnyStructureExposed { get { return false; } }
    public float HealthAsRatio { get { return 1f; } }
    public bool IsDead { get { return false; } }
    public bool IsOperational { get { return true; } }
    public bool IsShutDown { get { return false; } }
    public bool IsProne { get { return false; } }
    public bool CanMove { get { return false; } }
    public float DodgeChance(AbstractActor attacker, Weapon weapon) { return 1f; }
    public void TakeWeaponDamage(WeaponHitInfo hitInfo, int hitLocation, Weapon weapon, float damageAmount, float directStructureDamage, int hitIndex, DamageType damageType) {
    }
    public void ResolveWeaponDamage(WeaponHitInfo hitInfo) {
    }
    public void ResolveWeaponDamage(WeaponHitInfo hitInfo, Weapon weapon, MeleeAttackType meleeAttackType) { }
    public void ResolveAttackSequence(string sourceID, int sequenceID, int stackItemID, AttackDirection attackDirection) { }
    public int GetHitLocation(AbstractActor attacker, float hitLocationRoll, int calledShotLocation, float bonusMultiplier) { return 0; }
    public int GetHitLocation(AbstractActor attacker, Vector3 attackPosition, float hitLocationRoll, int calledShotLocation, float bonusMultiplier) { return 0; }
    public List<int> GetPossibleHitLocations(AbstractActor attacker) { return new List<int>(); }
    public int GetAdjacentHitLocation(Vector3 attackPosition, float randomRoll, int previousHitLocation, float originalMultiplier, float adjacentMultiplier) { return 0; }
    public Vector3 GetImpactPosition(AbstractActor attacker, Vector3 attackPosition, Weapon weapon, ref int hitLocation, ref AttackDirection attackDirection, ref string secondaryTargetId, ref int secondaryHitLocation) { return this.CurrentPosition; }
    public bool IsFlaggedForDeath { get { return false; } }
    public string GUID { get { return parent.GUID; } }
    public void FlagForDeath(string reason, DeathMethod deathMethod, DamageType damageType, int location, int stackItemID, string attackerID, bool isSilent) { }
    public void HandleDeath(string attackerGUID) { }
    public void HandleKnockdown(int previousStackID, string sourceID, Vector2 attackDirection, SequenceFinished fallSequenceCompletedCallback) { }
    public void SetGuid(string newGuid) {
    }
  }

  public class TerrainHitInfo {
    public Vector3 pos;
    public bool indirect;
    public VirtualCombatant virtualCombatant;
    public TerrainHitInfo(AbstractActor owner, Vector3 pos, bool indirect) {
      this.pos = pos;
      this.indirect = indirect;
      virtualCombatant = new VirtualCombatant(owner);
      virtualCombatant.CurrentPosition = pos;
    }
    public void Update(Vector3 pos, bool indirect) {
      virtualCombatant.CurrentPosition = pos;
      this.indirect = indirect;
    }
  }
  public static partial class CustomAmmoCategories {
    public static Dictionary<string, TerrainHitInfo> terrainHitPositions = new Dictionary<string, TerrainHitInfo>();
    public static void addTerrainHitPosition(this AbstractActor unit, Vector3 pos, bool indirect) {
      SpawnVehicleDialogHelper.lastTerrainHitPosition = pos;
      if (CustomAmmoCategories.terrainHitPositions.ContainsKey(unit.GUID) == false) {
        CustomAmmoCategories.terrainHitPositions.Add(unit.GUID, new TerrainHitInfo(unit, pos, indirect));
      } else {
        CustomAmmoCategories.terrainHitPositions[unit.GUID].Update(pos, indirect);
      }
    }
    public static TerrainHitInfo getTerrinHitPosition(string GUID) {
      if (CustomAmmoCategories.terrainHitPositions.ContainsKey(GUID) == false) {
        return null;
      }
      return CustomAmmoCategories.terrainHitPositions[GUID];
    }
    public static void ClearTerrainPositions() {
      terrainHitPositions.Clear();
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
        Log.Combat?.TWL(0, "AttackDirector.CreateAttackSequence:" + __result.id);
        if (__instance.isSequenceIsTerrainAttack()) {
          Log.Combat?.WL(0, "registering as terrain");
          __result.registerAsTerrainAttack();
          __instance.isSequenceIsTerrainAttack(false);
        }
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        AttackDirector.attackLogger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(CameraControl))]
  [HarmonyPatch("ShowAttackCam")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ICombatant),typeof(ICombatant), typeof(Vector3), typeof(Vector3), typeof(CameraMovementType), typeof(int), typeof(int), typeof(bool), typeof(bool) })]
  public static class CameraControl_ShowAttackCam {
    public static void Prefix(ref bool __runOriginal, CameraControl __instance, ICombatant attacker, ref ICombatant target, Vector3 attackerPos,ref Vector3 targetPos,CameraMovementType moveType,int shotsFired,int shotsHit,bool isMelee,bool isMultiAttack, ref CameraSequence __result) {
      try {
        if(__runOriginal == false) { return; }
        if(isMelee) { return; }
        if(attacker != target) { return; }
        if(attacker == null) { return; }
        var pos = CustomAmmoCategories.getTerrinHitPosition(attacker.GUID);
        if(pos == null) { return; }
        target = pos.virtualCombatant;
        targetPos = pos.pos;
        return;
      } catch(Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        CameraControl.cameraLogger.LogException(e);
        __runOriginal = true;
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
        __instance.DataManager.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(Pilot))]
  [HarmonyPatch("InitAbilities")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(bool), typeof(bool) })]
  public static class Pilot_InitAbilities {
    public static void Postfix(Pilot __instance, bool ModifyStats, bool FromSave) {
      var log = __instance.Combat == null ? Log.M : Log.Combat;
      try {
        log?.TWL(0, "Pilot.InitAbilities " + __instance.Description.Id);
        if (__instance.pilotDef.DataManager.AbilityDefs.TryGet(CombatHUDMechwarriorTray_InitAbilityButtons.AbilityName, out AbilityDef abilityDef) == false) {
          return;
        }
        Ability ability = new Ability(abilityDef);
        AbilityStateInfo LoadedState = (AbilityStateInfo)null;
        CombatGameState Combat = __instance.Combat;
        if (__instance.SerializedAbilityStates != null)
          LoadedState = __instance.SerializedAbilityStates.FirstOrDefault<AbilityStateInfo>((Func<AbilityStateInfo, bool>)(x => x.AbilityDefID == abilityDef.Id));
        if (FromSave && LoadedState != null)
          ability.InitFromSave(Combat, LoadedState);
        else
          ability.Init(Combat);
        __instance.Abilities.Add(ability);
        __instance.ActiveAbilities.Add(ability);
      } catch (Exception e) {
        log?.TWL(0, e.ToString(), true);
        Pilot.pilotErrorLog.LogException(e);
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
        Log.M?.TWL(0, "PilotDef.GatherDependencies " + __instance.Description.Id);
        dependencyLoad.RequestResource(BattleTechResourceType.AbilityDef, CombatHUDMechwarriorTray_InitAbilityButtons.AbilityName);
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
        __instance.DataManager.logger.LogException(e);
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
        Log.Combat?.TWL(0, "AttackStackSequence.OnAttackBegin");
        CombatGameState Combat = (CombatGameState)typeof(SequenceBase).GetProperty("Combat", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
        AttackDirector.AttackSequence attackSequence = Combat.AttackDirector.GetAttackSequence((message as AttackSequenceBeginMessage).sequenceId);
        if (attackSequence == null) {
          Log.Combat?.WL(1, "Null sequence on attack begin!!!");
          return;
        }
        Log.Combat?.WL(1, "attackSequence.calledShotLocation:"+ attackSequence.calledShotLocation);
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        AttackStackSequence.attackLogger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(PilotableActorRepresentation))]
  [HarmonyPatch("FaceTarget")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(bool), typeof(ICombatant), typeof(float), typeof(int), typeof(int), typeof(bool), typeof(GameRepresentation.RotationCompleteDelegate) })]
  public static class PilotableActorRepresentation_ReturnToNeutralFacing {
    public static void Prefix(ref bool __runOriginal, PilotableActorRepresentation __instance, bool isParellelSequence, ICombatant target, float twistTime, int stackItemUID, int sequenceId, bool isMelee, GameRepresentation.RotationCompleteDelegate completeDelegate) {
      try {
        if (!__runOriginal) { return; }
        if (__instance.parentCombatant == null) { __runOriginal = false;  return; }
        if (target == null) { __runOriginal = false; return; }
        Log.Combat?.TWL(0, "PilotableActorRepresentation.FaceTarget " + new Text(__instance.parentCombatant.DisplayName).ToString() + "->" + new Text(target.DisplayName).ToString());
        if (target.GUID != __instance.parentCombatant.GUID) {
          return;
        }
        TerrainHitInfo terrainPos = CustomAmmoCategories.getTerrinHitPosition(__instance.parentCombatant.GUID);
        if (terrainPos != null) {
          Log.Combat?.WL(1, "terrain attack detected " + terrainPos.pos);
          __instance.FacePoint(isParellelSequence, terrainPos.pos, false, twistTime, stackItemUID, sequenceId, isMelee, completeDelegate);
          __runOriginal = false; return;
        }
      }catch(Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        AbstractActor.logger.LogException(e);
      }
      return;
    }
  }
  [HarmonyPatch(typeof(SelectionState))]
  [HarmonyPatch("CanDeselect")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class SelectionState_CanDeselect {
    public static void Prefix(ref bool __runOriginal,SelectionState __instance, ref bool __result) {
      if (!__runOriginal) { return; }
      CombatGameState Combat = Traverse.Create(__instance).Property<CombatGameState>("Combat").Value;
      Log.Combat?.WL(0,"SelectionState.CanDeselect");
      Log.Combat?.WL(1, "Combat.TurnDirector.IsInterleaved = " + Combat.TurnDirector.IsInterleaved);
      Log.Combat?.WL(1, "SelectedActor.HasBegunActivation = " + __instance.SelectedActor.HasBegunActivation);
      Log.Combat?.WL(1, "SelectedActor.StoodUpThisRound = " + __instance.SelectedActor.StoodUpThisRound);
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
      Log.Combat?.WL(0,"SelectionStateMoveBase.CreateMoveOrders.IsInterleaved");
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
      Log.Combat?.WL(0, "AbstractActorMovementInvocation.Invoke.AutoBrace");
      actor.AutoBrace = false;
    }
  }
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("ResolveAttackSequence")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string), typeof(int), typeof(int), typeof(AttackDirection) })]
  public static class AbstractActor_ResolveAttackSequence {
    public static void Prefix(ref bool __runOriginal, AbstractActor __instance, string sourceID, int sequenceID, int stackItemID, AttackDirection attackDirection) {
      if (!__runOriginal) { return; }
      AttackDirector.AttackSequence attackSequence = __instance.Combat.AttackDirector.GetAttackSequence(sequenceID);
      if (attackSequence == null) { return; }
      if (attackSequence.attacker.GUID == attackSequence.chosenTarget.GUID) {
        Log.Combat?.WL(0, "this is terrain attack, no evasive damage or effects");
        __runOriginal = false;
        return;
      };
    }
  }
  [HarmonyPatch(typeof(SelectionState))]
  [HarmonyPatch("GetNewSelectionStateByType")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(SelectionType), typeof(CombatGameState), typeof(CombatHUD), typeof(CombatHUDActionButton), typeof(AbstractActor) })]
  public static class SelectionState_GetNewSelectionStateByType {
    public static bool SelectionForbidden = false;
    public static void Prefix(ref bool __runOriginal, SelectionType type, CombatGameState Combat, CombatHUD HUD, CombatHUDActionButton FromButton, AbstractActor actor, ref SelectionState __result) {
      if (!__runOriginal) { return; }
      Log.Combat?.WL(0,"SelectionState.GetNewSelectionStateByType " + type + ":" + FromButton.GUID);
      if ((type == SelectionType.CommandTargetSinglePoint) && (FromButton.GUID == "ID_ATTACKGROUND")) {
        Log.Combat?.WL(1, "creating own selection state");
        HUD.RegisterAttackGroundTarget(null);
        __result = new SelectionStateCommandAttackGround(Combat, HUD, FromButton, actor);
        __runOriginal = false;
        return;
      }
      return;
    }
  }
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("HasLOFToTargetUnitAtTargetPosition")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ICombatant), typeof(float), typeof(Vector3), typeof(Quaternion), typeof(Vector3), typeof(Quaternion), typeof(bool) })]
  public static class AbstractActor_HasLOFToTargetUnitAtTargetPosition {
    public static bool SelectionForbidden = false;
    public static void Prefix(AbstractActor __instance, ICombatant targetUnit, float maxRange, Vector3 attackPosition, Quaternion attackRotation, Vector3 targetPosition, Quaternion targetRotation, bool isIndirectFireCapable, ref bool __state) {
      if(__instance.GUID == targetUnit.GUID) {
        __state = __instance.StatCollection.GetValue<bool>("IsIndirectImmune");
        __instance.StatCollection.Set<bool>("IsIndirectImmune", false);
      }
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
    public static void Prefix(ref bool __runOriginal, CombatSelectionHandler __instance, AbstractActor actor, bool manualSelection, ref bool __result) {
      if (!__runOriginal) { return; }
      if (CombatSelectionHandler_TrySelectActor.SelectionForbidden) { __runOriginal = false; __result = false; return; }
      if(WeaponArtilleryHelper.TrySelectPrefix(__instance, actor, manualSelection)) { __runOriginal = false; __result = true; return; }
    }
  }
  [HarmonyPatch(typeof(CombatHUD))]
  [HarmonyPatch("OnAttackEnd")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
  public static class CombatHUD_OnAttackEnd {
    public static AbstractActor needSelect = null;
    public static void Postfix(CombatHUD __instance, MessageCenterMessage message) {      
      Log.Combat?.TWL(0,"CombatHUD.OnAttackEnd. SelectionForbidden: "+ CombatSelectionHandler_TrySelectActor.SelectionForbidden);
      try {
        if (CombatSelectionHandler_TrySelectActor.SelectionForbidden == true) {
          AttackDirector.AttackSequence attackSequence = __instance.Combat.AttackDirector.GetAttackSequence((message as AttackSequenceEndMessage).sequenceId);
          AbstractActor attacker = null;
          if (attackSequence == null) {
            Log.Combat?.WL(0,"Can't find sequence with id " + (message as AttackSequenceEndMessage).sequenceId);
            attacker = CombatHUD_OnAttackEnd.needSelect;
          } else {
            attacker = attackSequence.attacker;
          }
          //Log.LogWrite("Terrain attack end.\n");
          CombatSelectionHandler_TrySelectActor.SelectionForbidden = false;
          if (attacker != null) {
            Log.Combat?.WL(1, "attacker " + attacker.DisplayName + ":" + attacker.GUID + ".");
            MechRepresentation gameRep = attacker.GameRep as MechRepresentation;
            if (gameRep != null) {
              Log.Combat?.WL(0, "CombatHUD.OnAttackEnd. ToggleRandomIdles true");
              gameRep.ReturnToNeutralFacing(true, 0.5f, -1, -1, null);
              gameRep.ToggleRandomIdles(true);
            }
            if (attacker.HasMovedThisRound || ((attacker.Combat.TurnDirector.IsInterleaved == true) && (attacker.CanMoveAfterShooting == false))) {
              Log.Combat?.WL(1, "no need to select. already done with it");
            } else {
              Log.Combat?.WL(1, "try to select");
              bool HasBegunActivation = attacker.HasBegunActivation;
              bool HasActivatedThisRound = attacker.HasActivatedThisRound;
              attacker.HasBegunActivation = false;
              attacker.HasActivatedThisRound = false;
              attacker.HasFiredThisRound = true;

              __instance.SelectionHandler?.DeselectActor(__instance.SelectedActor);
              __instance.SelectionHandler?.TrySelectActor(attacker, false);
              attacker.HasBegunActivation = HasBegunActivation;
              attacker.HasActivatedThisRound = HasActivatedThisRound;
              __instance.MechWarriorTray.ConfirmAbilities(AbilityDef.ActivationTiming.ConsumedByFiring);
              attacker = null;
            }
          }
        }
      }catch(Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        CombatHUD.gameLogicLogger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("HasIndirectLOFToTargetUnit")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Vector3), typeof(Quaternion), typeof(ICombatant), typeof(bool) })]
  public static class AbstractActor_HasIndirectLOFToTargetUnitTerrainAttack {
    private static bool Prefix(AbstractActor __instance, Vector3 attackPosition, Quaternion attackRotation, ICombatant targetUnit, bool enabledWeaponsOnly, bool __result) {
      Log.Combat?.TWL(0, "AbstractActor.HasIndirectLOFToTargetUnit "+new Text(__instance.DisplayName).ToString()+"->"+ new Text(targetUnit.DisplayName).ToString());
      if (__instance.GUID != targetUnit.GUID) {
        Log.Combat?.WL(1,"not terrain attack");
        return true;
      }
      TerrainHitInfo terrainPos = CustomAmmoCategories.getTerrinHitPosition(__instance.GUID);
      if (terrainPos == null) {
        Log.Combat?.WL(1, "terrain attack info not found");
        return true;
      }
      __result = terrainPos.indirect;
      Log.Combat?.WL(1, "terrain attack indirect:"+__result);
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
    private static void Prefix(WeaponRangeIndicators __instance, AbstractActor selectedActor, Vector3 position, Quaternion rotation, AbstractActor targetedActor, bool isMelee) {
      Log.Combat?.TWL(0, "WeaponRangeIndicators.ShowLineToTarget position:"+position+" actor:" + new Text(selectedActor.DisplayName).ToString() + " trg:" + (targetedActor == null ? "null" : (new Text(targetedActor.DisplayName).ToString())));
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
      Log.Combat?.TWL(0,"AttackDirector.OnAttackCompleteTA:" + attackCompleteMessage.sequenceId);
      int sequenceId = attackCompleteMessage.sequenceId;
      AttackDirector.AttackSequence attackSequence = __instance.GetAttackSequence(sequenceId);
      if (attackSequence == null) {
        return true;
      }
      if ((attackSequence.chosenTarget.GUID == attackSequence.attacker.GUID)||(attackSequence.isTerrainAttackSequence())) {
        Log.Combat?.WL(1,"terrain attack detected id:"+ attackSequence.id);
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
          Log.Combat?.WL(0,"ToggleRandomIdles true");
          gameRep.ToggleRandomIdles(true);
        }
        __state.HasFiredThisRound = true;
        if (__state.HasMovedThisRound || (__state.team.IsLocalPlayer == false) || ((__state.Combat.TurnDirector.IsInterleaved == true) && (__state.CanMoveAfterShooting == false))) {
          Log.Combat?.WL(1, "done with:" + __state.DisplayName + ":" + __state.GUID);
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
        Log.Combat?.WL(0,"On Fire detected terrain attack");
        typeof(WeaponEffect).GetField("t", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (object)0.0f);
        __instance.hitIndex = (hitIndex);
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
    //private delegate void ShowLineToTargetDelegate(WeaponRangeIndicators indicators, AbstractActor selectedActor, Vector3 position, Quaternion rotation, AbstractActor targetedActor, bool isMelee);
    //private static ShowLineToTargetDelegate ShowLineToTargetInvoker = null;
    //private delegate void HideAllLinesDelegate(WeaponRangeIndicators indicators, CombatGameState Combat);
    //private static HideAllLinesDelegate HideAllLinesInvoker = null;
    //private delegate Color GetShotQualityTextColorDelegate(CombatHUDWeaponSlot slot, float toHitChance);
    //private static GetShotQualityTextColorDelegate GetShotQualityTextColorInvoker = null;
    private delegate void ProcessMousePosDelegate(SelectionState state,Vector3 worldPos);
    private static ProcessMousePosDelegate ProcessMousePosInvoker = null;
    //private delegate void RecalcDelegate(FiringPreviewManager instance, AbstractActor attacker, ICombatant target, List<AbstractActor> allies, Vector3 position, Quaternion rotation, bool canRotate, bool isJump, float maxRange, float maxIndirectRange);
    //private static RecalcDelegate RecalcInvoker = null;
    public static bool Prepare() {
      try {
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
      } catch (Exception e) {
        Log.M?.TWL(0,e.ToString());
        return false;
      }
      return true;
    }
    public static void SelectionState_ProcessMousePos(this SelectionState state, Vector3 worldPos) {
      ProcessMousePosInvoker(state, worldPos);
    }
    public static ICombatant findFriendlyNearPos(this AbstractActor actor, Vector3 worldPos, bool ignoreAlt = false) {
      ICombatant result = null;
      float resultDist = 0f;
      if (ignoreAlt == false) { if (Input.GetKey(KeyCode.LeftAlt) == false) { return null; }; };
      Log.Combat?.WL(1,"findFriendlyNearPos " + worldPos);
      foreach (ICombatant friend in actor.Combat.GetAllCombatants()) {
        if (friend.IsDead) { continue; }
        if ((friend.team != null) && (actor.team != null)) {
          if (friend.team.IsEnemy(actor.team)) { continue; }
        }
        float distance = Vector3.Distance(friend.CurrentPosition, worldPos);
        Log.Combat?.WL(3, "friend:" + friend.DisplayName + " " + distance);
        if (distance > CustomAmmoCategories.Settings.TerrainFiendlyFireRadius) { continue; }
        if ((distance < resultDist) || (result == null)) { result = friend; resultDist = distance; }
      }
      Log.Combat?.WL(2, "result: " + (result == null ? "null" : result.DisplayName));
      return result;
    }
    public static bool WillFireToPosition(this Weapon weapon, Vector3 position) {
      if (weapon.CanFire == false) {
        Log.Combat?.WL(1, "weapon can not fire");
        return false;
      }
      bool result = weapon.combat.LOFCache.UnitHasLOFToTargetAtTargetPosition(weapon.parent, weapon.parent, weapon.MaxRange,
        weapon.parent.CurrentPosition, weapon.parent.CurrentRotation, position, weapon.parent.CurrentRotation, weapon.IndirectFireCapable());
      Log.Combat?.WL(1, "UnitHasLOFToTargetAtTargetPosition " + result + " max range:" + weapon.MaxRange);
      return result;
    }
    public static void ShowTextColor(this CombatHUDWeaponSlot slot, Color color) {
      slot.ShowTextColor(color, slot.GetWeaponCOILStateColor(color), color, true);
    }
    public static void ShowTextColor(this CombatHUDWeaponSlot slot, Color color, Color hitChanceColor) {
      slot.ShowTextColor(color, slot.GetWeaponCOILStateColor(color), hitChanceColor, true);
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
      if (gameRep != null) {
        Log.Combat?.WL(0,"ToggleRandomIdles false");
        gameRep.ToggleRandomIdles(false);
      }
      string actorGUID = HUD.SelectedActor.GUID;
      HUD.SelectionHandler.DeselectActor(HUD.SelectionHandler.SelectedActor);
      HUD.MechWarriorTray.HideAllChevrons();
      CombatSelectionHandler_TrySelectActor.SelectionForbidden = true;
      int seqId = actor.Combat.StackManager.NextStackUID;
      if (actor.GUID == target.GUID) {
        Log.Combat?.WL(0, "Registering terrain attack to " + seqId);
        CustomAmmoCategories.addTerrainHitPosition(actor, targetPosition, LOFLevel < LineOfFireLevel.LOFObstructed);
      } else {
        Log.Combat?.WL(0, "Registering friendly attack to " + seqId);
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
      Log.Combat?.WL(0,"CombatHUDActionButton.ActivateCommandAbility " + __instance.GUID);
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
      Log.Combat?.WL(0, "InitAttackGroundAbilityButton " + ability.Def.Description.Id);
      Log.Combat?.WL(1, "aDef.Targeting = " + ability.Def.Targeting);
      Log.Combat?.WL(1, "aDef.AbilityIcon = " + ability.Def.AbilityIcon);
      Log.Combat?.WL(1, "aDef.Description.Name = " + ability.Def.Description.Name);
      button.InitButton(
        CombatHUDMechwarriorTray.GetSelectionTypeFromTargeting(ability.Def.Targeting, false),
        ability, ability.Def.AbilityIcon,
        "ID_ATTACKGROUND",
        ability.Def.Description.Name,
        actor
      );
      Log.Combat?.WL(1, "init button success");
      Log.Combat?.WL(1, "button.GUID = " + button.GUID);
      button.isClickable = true;
      button.RefreshUIColors();
      Log.Combat?.WL(1, "finished");
    }
    public static void Postfix(CombatHUDMechwarriorTray __instance, AbstractActor actor) {
      Log.Combat?.WL(1, "CombatHUDMechwarriorTray.InitAbilityButtons");
      if (CustomAmmoCategories.Settings.ShowAttackGroundButton == false) { return; }
    }
  }
  [HarmonyPatch(typeof(CombatHUDMechwarriorTray))]
  [HarmonyPatch("ResetAbilityButtons")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor) })]
  public static class CombatHUDMechwarriorTray_ResetAbilityButtons {
    public static void ResetAttackGroundButton(this CombatHUDMechwarriorTray tray,AbstractActor actor, Ability ability, CombatHUDActionButton button, bool forceInactive) {
      Log.Combat?.WL(0,"ResetAttackGroundButton:" + actor.DisplayName);
      Log.Combat?.WL(1, "actor.HasActivatedThisRound:" + actor.HasActivatedThisRound);
      Log.Combat?.WL(1, "actor.MovingToPosition:" + (actor.MovingToPosition != null));
      Log.Combat?.WL(1, "actor.Combat.StackManager.IsAnyOrderActive:" + actor.Combat.StackManager.IsAnyOrderActive);
      Log.Combat?.WL(1, "actor.Combat.TurnDirector.IsInterleaved:" + actor.Combat.TurnDirector.IsInterleaved);
      Log.Combat?.WL(1, "forceInactive:" + forceInactive);
      tray.ResetAbilityButton(actor, button, ability, forceInactive);
      if (forceInactive) { button.DisableButton(); };
      if (actor.Combat.TurnDirector.IsInterleaved == false) {
        if (actor.HasFiredThisRound == false) {
          if (ability.IsActive == false) {
            if (ability.IsAvailable == true) {
              if (actor.IsShutDown == false) {
                Log.Combat?.WL(1, "ResetButtonIfNotActive:");
                Log.Combat?.WL(1, "IsAbilityActivated:" + button.IsAbilityActivated);
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
      Log.Combat?.WL(0, "ombatHUDMechwarriorTray.ResetAbilityButtons");
      return;
    }
  }
}