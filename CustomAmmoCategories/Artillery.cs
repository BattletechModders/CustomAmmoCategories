using BattleTech;
using BattleTech.Save;
using BattleTech.UI;
using BattleTech.WeaponFilters;
using CleverGirlAIDamagePrediction;
using CustomAmmoCategoriesLog;
using CustomAmmoCategoriesPatches;
using HarmonyLib;
using Localize;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustAmmoCategories {
  public class SelectionStateArtilleryStrike: SelectionStateNoActionAvailable {
    public SelectionStateArtilleryStrike(CombatGameState Combat, CombatHUD HUD, CombatHUDActionButton FromButton, AbstractActor actor)
      : base(Combat, HUD, FromButton, actor) {
      this.TargetedCombatant = actor;
      this.SelectionType = SelectionType.Fire;
      this.PriorityLevel = SelectionPriorityLevel.NoActionAvailable;
      this.weapons = actor.GetArtilleryStrike(out var groundPos);
      if(this.weapons.Count == 0) {
        this.position = this.SelectedActor.CurrentPosition + this.SelectedActor.GameRep.thisTransform.forward;
      } else {
        this.position = groundPos;
      }
    }
    public override bool CanBackOut { get { return false; } }
    public override bool CanActorUseThisState(AbstractActor actor) => true;
    protected override bool showHeatWarnings => true;
    protected virtual List<Weapon> weapons { get; set; }
    protected virtual Vector3 position { get; set; }
    public override void OnAddToStack() {
      this.ShowButton(CombatHUDFireButton.FireMode.Fire,"ARTILLERY STRIKE");
      WeaponRangeIndicators.Instance.HideFiringArcs();
      WeaponRangeIndicators.Instance.HideTargetingLines(this.Combat);
      VisRangeIndicator.Instance.Hide();
    }
    public override void OnInactivate() {
      base.OnInactivate();
      if(this.SelectedActor.GameRep == null) { return; }
      if(this.SelectedActor.GameRep is MechRepresentation mechRep) { mechRep.ToggleRandomIdles(true); }
    }
    public override void OnResume() {
      base.OnResume();
      this.ShowButton(CombatHUDFireButton.FireMode.Fire, "ARTILLERY STRIKE");
    }
    public override void ProcessMousePos(Vector3 worldPos) {
    }
    protected virtual void PushInvocation(MessageCenterMessage invocation) {
      CombatSelectionHandler_TrySelectActor.SelectionForbidden = true;
      this.Combat.AttackDirector.isSequenceIsTerrainAttack(true);
      this.HUD.MechWarriorTray.ConfirmAbilities(AbilityDef.ActivationTiming.ConsumedByMovement);
      this.HUD.MechWarriorTray.ConfirmAbilities(AbilityDef.ActivationTiming.ConsumedByFiring);
      ReceiveMessageCenterMessage subscriber = (message => this.AssignOrders((message as AddSequenceToStackMessage).sequence));
      this.Combat.MessageCenter.AddSubscriber(MessageCenterMessageType.AddSequenceToStackMessage, subscriber);
      this.Combat.MessageCenter.PublishMessage(invocation);
      this.Combat.MessageCenter.RemoveSubscriber(MessageCenterMessageType.AddSequenceToStackMessage, subscriber);
      WeaponRangeIndicators.Instance.HideTargetingLines(this.Combat);
    }
    public override bool ProcessPressedButton(string button) {
      if((this.Orders == null)&&(this.weapons.Count != 0)) {
        var invocation = WeaponArtilleryHelper.CreateStrikeInvocation(this.SelectedActor);
        if(invocation == null) {
          Log.Combat?.WL(1, "No invocation created");
          return this.ProcessDoneWithMechButton(button) || base.ProcessPressedButton(button);
        }
        PushInvocation(invocation);
        //this.PublishInvocation(this.Combat.MessageCenter, (MessageCenterMessage)new MechStartupInvocation(selectedActor));
        return true;
      }
      return this.ProcessDoneWithMechButton(button) || base.ProcessPressedButton(button);
    }

    public override int ProjectedHeatForState {
      get {
        if(this.SelectedActor.isHasHeat() == false) { return 0; }
        WeaponArtilleryHelper.PrepareArtillery(this.weapons);
        int projectedHeatForState = 0;
        foreach(var weapon in this.weapons) {
            projectedHeatForState += (int)weapon.HeatGenerated;
        }
        return projectedHeatForState;
      }
    }
    public override bool CanDeselect {
      get {
        if(this.Combat.TurnDirector.IsInterleaved)
          return !this.SelectedActor.HasBegunActivation && !this.SelectedActor.StoodUpThisRound;
        if(!this.SelectedActor.HasBegunActivation)
          return true;
        return this.Orders != null && this.ConsumesFiring;
      }
    }

    protected void ShowButton(CombatHUDFireButton.FireMode mode, string additionalDetails = "") {
      WeaponArtilleryHelper.PrepareArtillery(this.weapons);
      this.HUD.ShowFireButton(mode, additionalDetails, this.showHeatWarnings);
      if(this.SelectedActor.GameRep != null) { return; }
      if(this.SelectedActor.GameRep is MechRepresentation mechRep) { mechRep.ToggleRandomIdles(false); }
      var weaponsList = this.SelectedActor.GetArtilleryStrike(out var groundPos);
      if(weaponsList.Count == 0) {
        this.SelectedActor.GameRep.FacePoint(true, this.SelectedActor.CurrentPosition + this.SelectedActor.GameRep.thisTransform.forward, false, 0.5f, -1, -1, false, null);
      } else {
        this.SelectedActor.GameRep.FacePoint(true, groundPos, false, 0.5f, -1, -1, false, null);
      }
      this.FromButton.ForceActive();
      this.HUD.AttackModeSelector.FireButton.SetState(ButtonState.Enabled, true);
    }
  }
  public class ArtilleryStrikeInfo {
    public CombatCustomReticle reticle = null;
    public PersistentFloatieMessage CountDownFloatie  = null;
    public LineRenderer targetingLine = null;
    public Vector3 position = Vector3.zero;
    public Weapon weapon = null;
    public AmmoModePair ammoMode = null;
    public ArtilleryStrikeInfo(Weapon weapon, Vector3 position) {
      this.weapon = weapon;
      this.position = position;
      this.ammoMode = weapon.getCurrentAmmoMode();
      GameObject reticleObj = weapon.parent.Combat.DataManager.PooledInstantiate(CombatCustomReticle.CustomPrefabName, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
      if(reticleObj != null) {
        this.reticle = reticleObj.GetComponent<CombatCustomReticle>();
        this.reticle.Init(PersistentFloatieHelper.HUD, this.position, null, weapon.ArtilleryReticleRadius());
        reticleObj.SetActive(true);
      } else {
        reticleObj = weapon.parent.Combat.DataManager.PooledInstantiate(CombatCustomReticle.PrefabName, BattleTechResourceType.UIModulePrefabs, new Vector3?(), new Quaternion?(), (Transform)null);
        CombatAuraReticle src = reticleObj.GetComponent<CombatAuraReticle>();
        this.reticle = reticleObj.AddComponent<CombatCustomReticle>();
        this.reticle.Copy(src);
        GameObject.Destroy(src);
        this.reticle.Init(PersistentFloatieHelper.HUD, this.position, null, weapon.ArtilleryReticleRadius());
      }
      this.reticle.auraRangeMatBright.color = weapon.ArtilleryReticleColor().Color;
      this.reticle.auraRangeMatDim.color = weapon.ArtilleryReticleColor().Color;
      this.CountDownFloatie = PersistentFloatieHelper.CreateFloatie(new Text($"{weapon.ArtilleryReticleText()}"), 18f, weapon.ArtilleryReticleColor().Color, null, this.position + Vector3.up * 10f);
      this.targetingLine = UnityEngine.Object.Instantiate<LineRenderer>(WeaponRangeIndicators.Instance.LineTemplate);
      this.targetingLine.transform.SetParent(WeaponRangeIndicators.Instance.transform);
      this.targetingLine.gameObject.SetActive(true);
      this.targetingLine.positionCount = WeaponRangeIndicators.Instance.MechJumpPoints;
      this.targetingLine.SetPositions(WeaponRangeIndicators.GetPointsForArcDodgeBuildings(WeaponRangeIndicators.Instance.MechJumpPoints, Mathf.Max(Mathf.Abs(position.y - weapon.parent.CurrentPosition.y) + 16f, 32f), weapon.parent.TargetPosition, position, UnityGameInstance.BattleTechGame.Combat.MapMetaData, WeaponRangeIndicators.Instance.MechJumpBuffer, WeaponRangeIndicators.Instance.MechJumpMaxArcHeight, WeaponRangeIndicators.Instance.MechJumpCheckFreq));
      this.targetingLine.startColor = weapon.ArtilleryReticleColor().Color;
      this.targetingLine.endColor = weapon.ArtilleryReticleColor().Color;
      this.targetingLine.material.color = weapon.ArtilleryReticleColor().Color;
      this.targetingLine.startWidth = 3f;
      this.targetingLine.endWidth = 0.5f;
    }
    public void Clear() {
      if(this.CountDownFloatie != null) {
        PersistentFloatieHelper.PoolFloatie(this.CountDownFloatie);
        this.CountDownFloatie = null;
      }
      if(this.reticle != null) {
        if(weapon != null) {
          weapon.parent.Combat.DataManager.PoolGameObject(CombatCustomReticle.CustomPrefabName, this.reticle.gameObject);
        } else {
          UnityGameInstance.BattleTechGame.DataManager.PoolGameObject(CombatCustomReticle.CustomPrefabName, this.reticle.gameObject);
        }
        this.reticle = null;
      }
      if(this.targetingLine != null) {
        GameObject.Destroy(this.targetingLine.gameObject);
        this.targetingLine = null;
      }
    }
  }
  public class ArtilleryUnitInfo {
    public AbstractActor unit;
    public bool AlreadyImmobilized { get; set; }
    public Dictionary<Weapon, bool> weaponsEnabledState { get; set; } = new Dictionary<Weapon, bool>();
    public ArtilleryUnitInfo(AbstractActor owner) {
      this.unit = owner;
      weaponsEnabledState = new Dictionary<Weapon, bool>();
      foreach(var weapon in owner.weapons) {
        weaponsEnabledState[weapon] = weapon.IsEnabled;
      }
      if(this.unit.GetTags().Contains("irbtmu_immobile_unit")) {
        AlreadyImmobilized = true;
      } else {
        AlreadyImmobilized = false;
        this.unit.EncounterTags.Add("irbtmu_immobile_unit");
      }
    }
    public void Clear() {
      foreach(var weapon in weaponsEnabledState) {
        if(weapon.Value == true) { weapon.Key.EnableWeapon(); };
      }
      if(this.AlreadyImmobilized == false) {
        this.unit.EncounterTags.Remove("irbtmu_immobile_unit");
      }
    }
  }
  public static class WeaponArtilleryHelper {
    private static Dictionary<Weapon, ArtilleryStrikeInfo> artilleryStrikes = new Dictionary<Weapon, ArtilleryStrikeInfo>();
    private static Dictionary<AbstractActor, ArtilleryUnitInfo> ActorsInArtilleryMode = new Dictionary<AbstractActor, ArtilleryUnitInfo>();
    public static float GetDistFromArtStrikeUncached(Vector3 pos) {
      float result = float.PositiveInfinity;
      foreach(var strike in artilleryStrikes) {
        strike.Value.weapon.ApplyAmmoMode(strike.Value.ammoMode);
        var dist = Vector3.Distance(pos, strike.Value.position);
        var artDist = strike.Value.weapon.ArtilleryReticleRadius();
        if(dist < artDist) {
          if((result > dist) || (float.IsPositiveInfinity(result))) { result = dist; }
        }
      }
      return result;
    }
    public static void Clear() {
      try {
        foreach(var strike in artilleryStrikes) {
          strike.Value.Clear();
        }
        artilleryStrikes.Clear();
        foreach(var info in ActorsInArtilleryMode) {
          info.Value.Clear();
        }
        ActorsInArtilleryMode.Clear();
      } catch(Exception e) {
        CombatGameState.gameInfoLogger.LogException(e);
      }
    }
    public static bool IsInArtilleryMode(this AbstractActor unit) {
      return ActorsInArtilleryMode.ContainsKey(unit);
    }
    public static void ToArtilleryMode(this AbstractActor unit) {
      if(ActorsInArtilleryMode.ContainsKey(unit)) { return; }
      ActorsInArtilleryMode.Add(unit, new ArtilleryUnitInfo(unit));
    }
    public static void FromArtilleryMode(this AbstractActor unit) {
      if(ActorsInArtilleryMode.TryGetValue(unit, out var info)) {
        ActorsInArtilleryMode.Remove(unit);
        info.Clear();
      }      
    }
    public static void AddArtilleryStrike(this Weapon weapon, Vector3 position) {
      if(weapon == null) { return; }
      if(artilleryStrikes.ContainsKey(weapon)) { artilleryStrikes[weapon].Clear(); artilleryStrikes.Remove(weapon); }
      artilleryStrikes[weapon] = new ArtilleryStrikeInfo(weapon, position);
    }
    public static List<Weapon> GetArtilleryStrike(this AbstractActor unit, out Vector3 position) {
      position = Vector3.zero;
      List<Weapon> result = new List<Weapon>();
      if(unit == null) { return result; }
      foreach(var weapon in artilleryStrikes) {
        if(weapon.Key.parent == unit) { result.Add(weapon.Key); position = weapon.Value.position; }
      }
      return result;
    }
    public static void PrepareArtillery(IEnumerable<Weapon> weapons) {
      foreach(var weapon in weapons) {
        if(artilleryStrikes.TryGetValue(weapon, out var info)) {
          weapon.ApplyAmmoMode(info.ammoMode);
        }
      }
    }
    public static void ClearArtillery(IEnumerable<Weapon> weapons) {
      foreach(var weapon in weapons) {
        if(artilleryStrikes.TryGetValue(weapon, out var info)) {
          info.Clear();
          artilleryStrikes.Remove(weapon);
        }
      }
    }
    public static bool IsArtillery(this Weapon weapon) {
      WeaponMode mode = weapon.mode();
      if (mode.IsArtillery != TripleBoolean.NotSet) { return mode.IsArtillery == TripleBoolean.True; };
      ExtAmmunitionDef ammo = weapon.ammo();
      if (ammo.IsArtillery != TripleBoolean.NotSet) { return ammo.IsArtillery == TripleBoolean.True; };
      return weapon.exDef().IsArtillery == TripleBoolean.True;
    }
    public static float ArtilleryReticleRadius(this Weapon weapon) {
      WeaponMode mode = weapon.mode();
      if(mode.IsArtillery == TripleBoolean.True) { return mode.ArtilleryReticleRadius; };
      ExtAmmunitionDef ammo = weapon.ammo();
      if(ammo.IsArtillery == TripleBoolean.True) { return ammo.ArtilleryReticleRadius; };
      return weapon.exDef().ArtilleryReticleRadius;
    }
    public static ColorTableJsonEntry ArtilleryReticleColor(this Weapon weapon) {
      WeaponMode mode = weapon.mode();
      if(mode.IsArtillery == TripleBoolean.True) { return mode.ArtilleryReticleColor; };
      ExtAmmunitionDef ammo = weapon.ammo();
      if(ammo.IsArtillery == TripleBoolean.True) { return ammo.ArtilleryReticleColor; };
      return weapon.exDef().ArtilleryReticleColor;
    }
    public static string ArtilleryReticleText(this Weapon weapon) {
      WeaponMode mode = weapon.mode();
      if(mode.IsArtillery == TripleBoolean.True) { return mode.ArtilleryReticleText; };
      ExtAmmunitionDef ammo = weapon.ammo();
      if(ammo.IsArtillery == TripleBoolean.True) { return ammo.ArtilleryReticleText; };
      return weapon.exDef().ArtilleryReticleText;
    }
    public static bool TrySelectPrefix(CombatSelectionHandler instance, AbstractActor actor, bool manualSelection) {
      if(actor == null 
        || !instance.Combat.LocalPlayerTeam.IsActive 
        || actor.HasBegunActivation && !actor.HasActivatedThisRound 
        || (instance.SelectedActor != null && !instance.IsSelectedActorChangeable)
        || actor.team != instance.Combat.LocalPlayerTeam 
        || actor == instance.SelectedActor) {
        return false;
      }
      if(actor.IsInArtilleryMode() == false) { return false; }
      if(actor.IsAvailableThisPhase == false) { return false; }
      if(actor.MovingToPosition != null) { return false; } //такого произойти не должно, 
      //поскольку в режиме артилерийского удара юнит иммобилизован, но лишняя проверка не бывает лишней
      if(actor.IsShutDown) { return false; }
      if(actor.IsProne) { return false; }

      if(instance.IsCommandButtonActive) {
        instance.UninvokeCommandTray();
      } else {
        instance.ClearSelectionStack();
      }
      instance.ActivatedAbilityButtons.Clear();
      instance.Combat.MessageCenter.PublishMessage(new ActorSelectedMessage(actor.GUID));
      if(manualSelection) {
        AudioEventManager.PlayPilotVO(VOEvents.Mech_Chosen, actor);
      }
      if(ActiveOrDefaultSettings.CloudSettings.autoCenterOnSelection) {
        CameraControl.Instance.SetMovingToGroundPos(actor.CurrentPosition);
      }
      SelectionState newState = new SelectionStateArtilleryStrike(instance.Combat, instance.HUD, instance.HUD.FireButton, actor);
      instance.addNewState(newState);
      newState.OnResume();
      instance.logSelectionStack();
      return true;
    }
    public static InvocationMessage CreateStrikeInvocation(AbstractActor unit) {
      unit.FromArtilleryMode();
      var actor = unit;
      var target = unit;
      var weaponsList = actor.GetArtilleryStrike(out var groundPos);
      if(weaponsList.Count == 0) { return null; }
      WeaponArtilleryHelper.PrepareArtillery(weaponsList);
      WeaponArtilleryHelper.ClearArtillery(weaponsList);
      Vector3 collisionWorldPos;
      LineOfFireLevel LOFLevel = LineOfFireLevel.LOFBlocked;
      LOFLevel = actor.Combat.LOFCache.GetLineOfFire(actor, actor.CurrentPosition, actor, groundPos, actor.CurrentRotation, out collisionWorldPos);
      Log.Combat?.WL(1, "Registering terrain attack to " + actor.GUID);
      CustomAmmoCategories.addTerrainHitPosition(actor, groundPos, LOFLevel < LineOfFireLevel.LOFObstructed);
      return new AttackInvocation(actor, target, weaponsList);
    }
  }
  [HarmonyPatch(typeof(AITeam))]
  [HarmonyPatch("makeInvocationFromOrders")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(OrderInfo) })]
  public static class AITeam_makeInvocationFromOrders {
    public static void Postfix(AITeam __instance, AbstractActor unit, OrderInfo order, ref InvocationMessage __result) {
      try {
        Log.Combat?.TWL(0, $"AITeam.makeInvocationFromOrders {__instance.Name} unit:{unit.PilotableActorDef.ChassisID}");
        if(__result is AttackInvocation attack) {
          Log.Combat?.WL(1, $"order is attack");
          bool is_artillery_strike = false;
          foreach(var subattack in attack.subAttackInvocations) {
            List<Weapon> subweapons = subattack.invocationWeapons.ToWeaponList(unit);
            foreach(var weapon in subweapons) {
              bool is_art = weapon.IsArtillery();
              Log.Combat?.WL(2, $"weapon:{weapon.defId} IsArtillery:{is_art}");
              if(is_art) {
                ICombatant target = __instance.Combat.FindCombatantByGUID(subattack.targetGUID);
                if(target != null) {
                  if(target == unit) {
                    var info = CustomAmmoCategories.getTerrinHitPosition(unit.GUID);
                    if(info == null) {
                      Log.Combat?.WL(3, $"ground position not found. Brace instead");
                      __result = new ReserveActorInvocation(unit, ReserveActorAction.DONE, __instance.Combat.TurnDirector.CurrentRound);
                      return;
                    }
                    Log.Combat?.WL(3, $"artillery strike ground {info.pos}");
                    is_artillery_strike = true; weapon.AddArtilleryStrike(info.pos);
                  } else {
                    Log.Combat?.WL(3, $"artillery strike:{target.CurrentPosition}");
                    is_artillery_strike = true; weapon.AddArtilleryStrike(target.CurrentPosition);
                  }
                }
              }
            }
          }
          if(is_artillery_strike) {
            unit.ToArtilleryMode();
            __result = new ReserveActorInvocation(unit, ReserveActorAction.DONE, __instance.Combat.TurnDirector.CurrentRound);
          }
        }else if(__result is ReserveActorInvocation reserve) {
          if((reserve.ReserveAction == ReserveActorAction.DONE)&&(unit.IsInArtilleryMode())) {
            var invocation = WeaponArtilleryHelper.CreateStrikeInvocation(unit);
            if(invocation != null) { __result = invocation; }            
          }
        }
      }catch(Exception e) {
        Log.Combat?.TWL(0,e.ToString());
        AITeam.attackLogger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(SelectionStateFire))]
  [HarmonyPatch("CreateFiringOrders")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class SelectionStateFire_CreateFiringOrders {
    private static CombatHUD HUD(this SelectionState state) {
      return Traverse.Create(state).Property<CombatHUD>("HUD").Value;
    }
    private static CombatGameState Combat(this SelectionState state) {
      return Traverse.Create(state).Property<CombatGameState>("Combat").Value;
    }
    public static void Prefix(ref bool __runOriginal, SelectionStateFire __instance, string button, ref bool __result) {
      try {
        if(__runOriginal == false) { return; }
        if(button != "BTN_FireConfirm") { return; }
        if(__instance.SelectedActor == null) { return; }
        if(__instance.HasTarget == false) { return; }
        Log.Combat?.TWL(0, $"SelectionStateFire.CreateFiringOrders {__instance.SelectedActor.PilotableActorDef.ChassisID}");
        var weapons = GenericAttack.Filter(__instance.SelectedActor, __instance.TargetedCombatant, true);
        bool is_artillery_strike = false;
        foreach(var weapon in weapons) {
          bool is_art = weapon.IsArtillery();
          Log.Combat?.WL(1, $"weapon:{weapon.defId} IsArtillery:{is_art}");
          if(is_art) {
            Log.Combat?.WL(2, $"artillery strike:{__instance.TargetedCombatant.CurrentPosition}");
            is_artillery_strike = true; weapon.AddArtilleryStrike(__instance.TargetedCombatant.CurrentPosition);
          }
        }
        if(is_artillery_strike) { __runOriginal = false; } else { return; }
        MessageCenterMessage reserveInvokation = new ReserveActorInvocation(__instance.SelectedActor, ReserveActorAction.DONE, __instance.Combat().TurnDirector.CurrentRound);
        __instance.HUD().MechWarriorTray.ConfirmAbilities(AbilityDef.ActivationTiming.ConsumedByMovement);
        __instance.HUD().MechWarriorTray.ConfirmAbilities(AbilityDef.ActivationTiming.ConsumedByFiring);
        ReceiveMessageCenterMessage subscriber = (ReceiveMessageCenterMessage)(message => __instance.AttackOrdersSet(message));
        __instance.Combat().MessageCenter.AddSubscriber(MessageCenterMessageType.AddSequenceToStackMessage, subscriber);
        __instance.Combat().MessageCenter.PublishMessage(reserveInvokation);
        __instance.SelectedActor.ToArtilleryMode();
        __instance.Combat().MessageCenter.RemoveSubscriber(MessageCenterMessageType.AddSequenceToStackMessage, subscriber);
        WeaponRangeIndicators.Instance.HideTargetingLines(__instance.Combat());
        __result = true;
      } catch(Exception e) {
        Log.Combat?.TWL(0, e.ToString());
        CombatHUD.uiLogger.LogException(e);
        __runOriginal = true;
      }
    }
  }
  [HarmonyPatch(typeof(SelectionStateFireMulti))]
  [HarmonyPatch("CreateFiringOrders")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class SelectionStateFireMulti_CreateFiringOrders {
    private static CombatHUD HUD(this SelectionState state) {
      return Traverse.Create(state).Property<CombatHUD>("HUD").Value;
    }
    private static CombatGameState Combat(this SelectionState state) {
      return Traverse.Create(state).Property<CombatGameState>("Combat").Value;
    }
    public static void Prefix(ref bool __runOriginal, SelectionStateFireMulti __instance, string button, ref bool __result) {
      try {
        if(__runOriginal == false) { return; }
        if(button != "BTN_FireConfirm") { return; }
        if(__instance.SelectedActor == null) { return; }
        if(__instance.AllTargetedCombatants.Count == 0) { return; }
        Log.Combat?.TWL(0, $"SelectionStateFireMulti.CreateFiringOrders {__instance.SelectedActor.PilotableActorDef.ChassisID}");
        bool is_artillery_strike = false;
        List<Weapon> weaponSubset = new List<Weapon>();
        for(int targetIndex = 0; targetIndex < __instance.AllTargetedCombatants.Count; ++targetIndex) {
          ICombatant targetedCombatant = __instance.AllTargetedCombatants[targetIndex];
          weaponSubset.Clear();
          foreach(var weapon in __instance.SelectedActor.Weapons) { 
            if(__instance.weaponTargetIndices[weapon] == targetIndex)
              weaponSubset.Add(weapon);
          }
          List<Weapon> weapons = GenericAttack.Filter(weaponSubset, targetedCombatant, true);
          foreach(var weapon in weapons) {
            bool is_art = weapon.IsArtillery();
            Log.Combat?.WL(1, $"weapon:{weapon.defId} IsArtillery:{is_art}");
            if(is_art) {
              Log.Combat?.WL(2, $"artillery strike:{targetedCombatant.CurrentPosition}");
              is_artillery_strike = true; weapon.AddArtilleryStrike(targetedCombatant.CurrentPosition);
            }
          }
        }
        if(is_artillery_strike) { __runOriginal = false; } else { return; }
        MessageCenterMessage reserveInvokation = new ReserveActorInvocation(__instance.SelectedActor, ReserveActorAction.DONE, __instance.Combat().TurnDirector.CurrentRound);
        __instance.HUD().MechWarriorTray.ConfirmAbilities(AbilityDef.ActivationTiming.ConsumedByMovement);
        __instance.HUD().MechWarriorTray.ConfirmAbilities(AbilityDef.ActivationTiming.ConsumedByFiring);
        ReceiveMessageCenterMessage subscriber = (ReceiveMessageCenterMessage)(message => __instance.AttackOrdersSet(message));
        __instance.Combat().MessageCenter.AddSubscriber(MessageCenterMessageType.AddSequenceToStackMessage, subscriber);
        __instance.Combat().MessageCenter.PublishMessage(reserveInvokation);
        __instance.Combat().MessageCenter.RemoveSubscriber(MessageCenterMessageType.AddSequenceToStackMessage, subscriber);
        WeaponRangeIndicators.Instance.HideTargetingLines(__instance.Combat());
        __result = true;
      } catch(Exception e) {
        Log.Combat?.TWL(0, e.ToString());
        CombatHUD.uiLogger.LogException(e);
        __runOriginal = true;
      }
    }
  }

  [HarmonyPatch(typeof(AITeam))]
  [HarmonyPatch("getInvocationForCurrentUnit")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class AITeam_getInvocationForCurrentUnit {
    public static void Prefix(ref bool __runOriginal, AITeam __instance, ref InvocationMessage __result) {
      try {
        if(__runOriginal == false) { return; }
        if(__instance.currentUnit == null) { return; }
        if(__instance.currentUnit.IsInArtilleryMode() == false) { return; }
        var invocation = WeaponArtilleryHelper.CreateStrikeInvocation(__instance.currentUnit);
        if(invocation != null) {
          Log.Combat?.TWL(0, $"AITeam.getInvocationForCurrentUnit artillery strike {__instance.currentUnit.PilotableActorDef.Description.Id}");
          __instance.Combat.AttackDirector.isSequenceIsTerrainAttack(true);
          __result = invocation;
          __runOriginal = false;
        }
        //__result = (InvocationMessage)new ReserveActorInvocation(__instance.currentUnit, ReserveActorAction.DONE, __instance.Combat.TurnDirector.CurrentRound);
      } catch(Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        AITeam.activationLogger.LogException(e);
        __runOriginal = true;
      }
    }
    public static void Postfix(AITeam __instance, ref InvocationMessage __result) {
      try {
        Log.Combat?.TWL(0, "AITeam.getInvocationForCurrentUnit " + (__result == null ? "null" : __result.GetType().Name));
      } catch(Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        AITeam.activationLogger.LogException(e);
      }
    }
  }

  [HarmonyPatch(typeof(CombatHUDFireButton))]
  [HarmonyPatch("Update")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatHUDFireButton_Update {
    public static void Prefix(ref bool __runOriginal, CombatHUDFireButton __instance) {
      try {
        if(__runOriginal == false) { return; }
        if(__instance.HUD.SelectionHandler.CombatInputBlocked) { return; }
        if(__instance.CurrentFireMode != CombatHUDFireButton.FireMode.Fire) { return; }
        if(__instance.HUD.SelectionHandler.ActiveState is SelectionStateArtilleryStrike artState) {
          __runOriginal = false;
          if(__instance.BaseState == ButtonState.Unavailable || __instance.BaseState == ButtonState.Disabled) {
            __instance.SetState(ButtonState.Enabled);
            return;
          }
        }
        //__result = (InvocationMessage)new ReserveActorInvocation(__instance.currentUnit, ReserveActorAction.DONE, __instance.Combat.TurnDirector.CurrentRound);
      } catch(Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        UIManager.logger.LogException(e);
        __runOriginal = true;
      }
    }
  }
  public static class AbstractActor_FlagForDeath_Atrillery {
    public static void Postfix(AbstractActor __instance) {
      try {
        if(__instance.IsInArtilleryMode()) {
          __instance.FromArtilleryMode();
          var weaponsList = __instance.GetArtilleryStrike(out var groundPos);
          if(weaponsList.Count == 0) { return; }
          WeaponArtilleryHelper.PrepareArtillery(weaponsList);
          WeaponArtilleryHelper.ClearArtillery(weaponsList);
        }
      } catch(Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        UIManager.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("GenerateOverheatedSequence")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MechHeatSequence) })]
  public static class Mech_GenerateOverheatedSequence {
    public static void Postfix(Mech __instance, MechHeatSequence previousSequence) {
      try {
        if(__instance.IsInArtilleryMode()) {
          __instance.FromArtilleryMode();
          var weaponsList = __instance.GetArtilleryStrike(out var groundPos);
          if(weaponsList.Count == 0) { return; }
          WeaponArtilleryHelper.PrepareArtillery(weaponsList);
          WeaponArtilleryHelper.ClearArtillery(weaponsList);
        }
      } catch(Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        UIManager.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("GenerateFallSequence")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(int), typeof(string), typeof(Vector2), typeof(SequenceFinished) })]
  public static class Mech_GenerateFallSequence {
    public static void Postfix(Mech __instance, int previousStackID, string sourceID, Vector2 attackDirection, SequenceFinished fallSequenceCompletedCallback) {
      try {
        if(__instance.IsInArtilleryMode()) {
          __instance.FromArtilleryMode();
          var weaponsList = __instance.GetArtilleryStrike(out var groundPos);
          if(weaponsList.Count == 0) { return; }
          WeaponArtilleryHelper.PrepareArtillery(weaponsList);
          WeaponArtilleryHelper.ClearArtillery(weaponsList);
        }
      } catch(Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        UIManager.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(SelectionState))]
  [HarmonyPatch("ProcessDoneWithMechButton")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class SelectionState_ProcessDoneWithMechButton {
    public static void Postfix(SelectionState __instance, string button, ref bool __result) {
      try {
        if(__result == false) { return; }
        if(__instance.SelectedActor.IsInArtilleryMode()) {
          __instance.SelectedActor.FromArtilleryMode();
          var weaponsList = __instance.SelectedActor.GetArtilleryStrike(out var groundPos);
          if(weaponsList.Count == 0) { return; }
          WeaponArtilleryHelper.PrepareArtillery(weaponsList);
          WeaponArtilleryHelper.ClearArtillery(weaponsList);
        }
      } catch(Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        UIManager.logger.LogException(e);
      }
    }
  }

  //[HarmonyPatch(typeof(SelectionStateDoneWithMech))]
  //[HarmonyPatch("ProcessPressedButton")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { typeof(string) })]
  //public static class SelectionStateDoneWithMech_ProcessPressedButton {
  //  private static CombatHUD HUD(this SelectionState state) {
  //    return Traverse.Create(state).Property<CombatHUD>("HUD").Value;
  //  }
  //  private static CombatGameState Combat(this SelectionState state) {
  //    return Traverse.Create(state).Property<CombatGameState>("Combat").Value;
  //  }
  //  private static void PushInvocation(SelectionStateDoneWithMech __instance, AbstractActor actor, MessageCenterMessage invocation) {
  //    CombatSelectionHandler_TrySelectActor.SelectionForbidden = true;
  //    actor.Combat.AttackDirector.isSequenceIsTerrainAttack(true);
  //    __instance.HUD().MechWarriorTray.ConfirmAbilities(AbilityDef.ActivationTiming.ConsumedByMovement);
  //    __instance.HUD().MechWarriorTray.ConfirmAbilities(AbilityDef.ActivationTiming.ConsumedByFiring);
  //    ReceiveMessageCenterMessage subscriber = (message => __instance.AssignOrders((message as AddSequenceToStackMessage).sequence));
  //    __instance.Combat().MessageCenter.AddSubscriber(MessageCenterMessageType.AddSequenceToStackMessage, subscriber);
  //    __instance.Combat().MessageCenter.PublishMessage(invocation);
  //    __instance.Combat().MessageCenter.RemoveSubscriber(MessageCenterMessageType.AddSequenceToStackMessage, subscriber);
  //    WeaponRangeIndicators.Instance.HideTargetingLines(__instance.Combat());
  //  }
  //  public static void Prefix(ref bool __runOriginal, SelectionStateDoneWithMech __instance, string button, ref bool __result) {
  //    try {
  //      if(__runOriginal == false) { return; }
  //      if(__instance.SelectedActor == null) { return; }
  //      if(__instance.SelectedActor.IsInArtilleryMode() == false) { return; }
  //      __runOriginal = false;
  //      __instance.SelectedActor.FromArtilleryMode();
  //      var actor = __instance.SelectedActor;
  //      var target = __instance.SelectedActor;
  //      var weaponsList = actor.GetArtilleryStrike(out var groundPos);
  //      if(weaponsList.Count == 0) { __runOriginal = true; return; }
  //      Vector3 collisionWorldPos;
  //      LineOfFireLevel LOFLevel = LineOfFireLevel.LOFBlocked;
  //      LOFLevel = actor.Combat.LOFCache.GetLineOfFire(actor, actor.CurrentPosition, actor, groundPos, actor.CurrentRotation, out collisionWorldPos);
  //      Log.Combat?.WL(1, "Registering terrain attack to " + actor.GUID);
  //      CustomAmmoCategories.addTerrainHitPosition(actor, groundPos, LOFLevel < LineOfFireLevel.LOFObstructed);
  //      WeaponArtilleryHelper.PerformArtillery(weaponsList);
  //      WeaponArtilleryHelper.ClearArtillery(weaponsList);
  //      var invocation = new AttackInvocation(actor, target, weaponsList);
  //      if(invocation == null) {
  //        Log.Combat?.WL(1, "No invocation created");
  //        __result = false;
  //        return;
  //      }
  //      MechRepresentation gameRep = actor.GameRep as MechRepresentation;
  //      if(gameRep != null) {
  //        Log.Combat?.WL(1, "ToggleRandomIdles false");
  //        gameRep.ToggleRandomIdles(false);
  //        gameRep.FacePoint(true, groundPos, false, 0.5f, -1, -1, false, (a, b) => { PushInvocation(__instance, actor, invocation); });
  //      } else {
  //        PushInvocation(__instance, actor, invocation);
  //      }
  //      __result = true;
  //    } catch(Exception e) {
  //      Log.Combat?.TWL(0, e.ToString());
  //      CombatHUD.uiLogger.LogException(e);
  //      __runOriginal = true;
  //    }
  //  }
  //}

}