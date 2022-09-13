/*  
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
using BattleTech.Rendering;
using Harmony;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CustomUnits {
  [HarmonyPatch(typeof(GameRepresentation))]
  [HarmonyPatch("propertyBlock")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class GameRepresentation_propertyBlock {
    public static bool Prefix(GameRepresentation __instance, ref PropertyBlockManager __result) {
      try {
        //Log.TWL(0, "PilotableActorRepresentation.InitPaintScheme :" + __instance.GetType().ToString());
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          __result = custMechRep.__propertyBlock;
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(GameRepresentation))]
  [HarmonyPatch("IsTargetable")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class GameRepresentation_IsTargetable_get {
    public static bool Prefix(GameRepresentation __instance, ref bool __result) {
      try {
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          __result = custMechRep.__IsTargetable;
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(GameRepresentation))]
  [HarmonyPatch("IsTargetable")]
  [HarmonyPatch(MethodType.Setter)]
  [HarmonyPatch(new Type[] { })]
  public static class GameRepresentation_IsTargetable_set {
    public static bool Prefix(GameRepresentation __instance, bool value) {
      try {
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          custMechRep.__IsTargetable = value;
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(GameRepresentation))]
  [HarmonyPatch("IsTargeted")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class GameRepresentation_IsTargeted_get {
    public static bool Prefix(GameRepresentation __instance, ref bool __result) {
      try {
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          __result = custMechRep.__IsTargeted;
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(GameRepresentation))]
  [HarmonyPatch("IsTargeted")]
  [HarmonyPatch(MethodType.Setter)]
  [HarmonyPatch(new Type[] { })]
  public static class GameRepresentation_IsTargeted_set {
    public static bool Prefix(GameRepresentation __instance, bool value) {
      try {
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          custMechRep.__IsTargeted = value;
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(GameRepresentation))]
  [HarmonyPatch("IsAvailable")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class GameRepresentation_IsAvailable_get {
    public static bool Prefix(GameRepresentation __instance, ref bool __result) {
      try {
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          __result = custMechRep.__IsAvailable;
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(GameRepresentation))]
  [HarmonyPatch("IsAvailable")]
  [HarmonyPatch(MethodType.Setter)]
  [HarmonyPatch(new Type[] { })]
  public static class GameRepresentation_IsAvailable_set {
    public static bool Prefix(GameRepresentation __instance, bool value) {
      try {
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          custMechRep.__IsAvailable = value;
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(GameRepresentation))]
  [HarmonyPatch("IsSelected")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class GameRepresentation_IsSelected_get {
    public static bool Prefix(GameRepresentation __instance, ref bool __result) {
      try {
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          __result = custMechRep.__IsSelected;
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(GameRepresentation))]
  [HarmonyPatch("IsSelected")]
  [HarmonyPatch(MethodType.Setter)]
  [HarmonyPatch(new Type[] { })]
  public static class GameRepresentation_IsSelected_set {
    public static bool Prefix(GameRepresentation __instance, bool value) {
      try {
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          custMechRep.__IsSelected = value;
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(GameRepresentation))]
  [HarmonyPatch("IsHovered")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class GameRepresentation_IsHovered_get {
    public static bool Prefix(GameRepresentation __instance, ref bool __result) {
      try {
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          __result = custMechRep.__IsHovered;
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(GameRepresentation))]
  [HarmonyPatch("IsHovered")]
  [HarmonyPatch(MethodType.Setter)]
  [HarmonyPatch(new Type[] { })]
  public static class GameRepresentation_IsHovered_set {
    public static bool Prefix(GameRepresentation __instance, bool value) {
      try {
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          custMechRep.__IsHovered = value;
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(GameRepresentation))]
  [HarmonyPatch("IsDead")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class GameRepresentation_IsDead_get {
    public static bool Prefix(GameRepresentation __instance, ref bool __result) {
      try {
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          __result = custMechRep.__IsDead;
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(GameRepresentation))]
  [HarmonyPatch("IsDead")]
  [HarmonyPatch(MethodType.Setter)]
  [HarmonyPatch(new Type[] { })]
  public static class GameRepresentation_IsDead_set {
    public static bool Prefix(GameRepresentation __instance, bool value) {
      try {
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          custMechRep.__IsDead = value;
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(GameRepresentation))]
  [HarmonyPatch("VisibleToPlayer")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class GameRepresentation_VisibleToPlayer {
    public static bool Prefix(GameRepresentation __instance, ref bool __result) {
      try {
        //Log.TWL(0, "PilotableActorRepresentation.InitPaintScheme :" + __instance.GetType().ToString());
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          __result = custMechRep._VisibleToPlayer;
          return false;
        }
        if (__instance.parentCombatant is CustomMech custMech) {
          if (custMech.ForcedVisible) { __result = true; return false; }
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(GameRepresentation))]
  [HarmonyPatch("Awake")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class GameRepresentation_Awake {
    public static bool Prefix(GameRepresentation __instance) {
      try {
        //Log.TWL(0, "PilotableActorRepresentation.InitPaintScheme :" + __instance.GetType().ToString());
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          custMechRep._Awake();
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(GameRepresentation))]
  [HarmonyPatch("OnDestroy")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class GameRepresentation_OnDestroy {
    public static bool Prefix(GameRepresentation __instance) {
      try {
        //Log.TWL(0, "PilotableActorRepresentation.InitPaintScheme :" + __instance.GetType().ToString());
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          custMechRep._OnDestroy();
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(GameRepresentation))]
  [HarmonyPatch("InitHighlighting")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class GameRepresentation_InitHighlighting {
    public static bool Prefix(GameRepresentation __instance) {
      try {
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          custMechRep._InitHighlighting();
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(GameRepresentation))]
  [HarmonyPatch("SetHighlightColor")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(CombatGameState), typeof(Team) })]
  public static class GameRepresentation_SetHighlightColor {
    public static bool Prefix(GameRepresentation __instance, CombatGameState combat, Team team) {
      try {
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          custMechRep._SetHighlightColor(combat, team);
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(GameRepresentation))]
  [HarmonyPatch("FadeIn")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(float) })]
  public static class GameRepresentation_FadeIn {
    public static bool Prefix(GameRepresentation __instance, float length) {
      try {
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          custMechRep._FadeIn(length);
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(GameRepresentation))]
  [HarmonyPatch("FadeOut")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(float) })]
  public static class GameRepresentation_FadeOut {
    public static bool Prefix(GameRepresentation __instance, float length) {
      try {
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          custMechRep._FadeOut(length);
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(GameRepresentation))]
  [HarmonyPatch("StopManualPersistentVFX")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class GameRepresentation_StopManualPersistentVFX {
    public static bool Prefix(GameRepresentation __instance, string vfxName) {
      try {
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          custMechRep._StopManualPersistentVFX(vfxName);
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(GameRepresentation))]
  [HarmonyPatch("PauseAllPersistentVFX")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class GameRepresentation_PauseAllPersistentVFX {
    public static bool Prefix(GameRepresentation __instance) {
      try {
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          custMechRep._PauseAllPersistentVFX();
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(GameRepresentation))]
  [HarmonyPatch("ResumeAllPersistentVFX")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class GameRepresentation_ResumeAllPersistentVFX {
    public static bool Prefix(GameRepresentation __instance) {
      try {
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          custMechRep._ResumeAllPersistentVFX();
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(GameRepresentation))]
  [HarmonyPatch("StopAllPersistentVFXAttachedToLocation")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(int) })]
  public static class GameRepresentation_StopAllPersistentVFXAttachedToLocation {
    public static bool Prefix(GameRepresentation __instance, int location) {
      try {
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          custMechRep._StopAllPersistentVFXAttachedToLocation(location);
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  public partial class CustomMechRepresentation {
    private static readonly float hightlightDeadIntensity = -1f;
    private static readonly float highlightOffIntensity = 0.0f;
    private static readonly float highlightDefaultIntensity = 0.5f;
    private static readonly float highlightSelectedIntensity = 1f;
    public virtual Collider[] GameRepresentation_AllRaycastColliders {
      get {
        if (this._allRaycastColliders == null)
          this._allRaycastColliders = this.gameObject.GetComponentsInChildren<Collider>(true);
        return this._allRaycastColliders;
      }
    }
    public virtual CustomPropertyBlockManager __propertyBlock {
      get {
        if ((UnityEngine.Object)this._propertyBlock == (UnityEngine.Object)null)
          this._propertyBlock = this.GetComponentInChildren<CustomPropertyBlockManager>();
        return this._propertyBlock as CustomPropertyBlockManager;
      }
    }
    public virtual bool isSlaveVisible(CustomMechRepresentation slave) {
      if (slave.parentRepresentation != this) { return false; }
      return this._VisibleToPlayer;
    }
    public virtual bool ForcedVisible { get; set; } = false;
    public virtual bool _VisibleToPlayer {
      get {
        if (this.parentCombatant == null || this.parentCombatant.Combat == null) { return false; }
        if (isSlave) { if (this.parentRepresentation != null) { return this.parentRepresentation.isSlaveVisible(this); }  }
        if (this.custMech.ForcedVisible) { return true; }
        return this.parentCombatant.team.IsFriendly(this.parentCombatant.Combat.LocalPlayerTeam) || this.parentCombatant.Combat.LocalPlayerTeam.VisibilityToTarget((ICombatant)(this.parentCombatant as AbstractActor)) == VisibilityLevel.LOSFull;
      }
    }
    public virtual void _Awake() {
      this.thisGameObject = this.transform.gameObject;
      this.thisTransform = this.transform;
      this.thisCharacterController = this.GetComponent<CharacterController>();
      this.thisAnimator = this.GetComponent<Animator>();
      this.thisIKController = this.GetComponent<InverseKinematic>();
      this.audioObject = this.GetComponent<AkGameObj>();
    }
    public virtual void _OnDestroy() {
      this.OnCombatGameDestroyed();
      if ((UnityEngine.Object)this.audioObject != (UnityEngine.Object)null)
        AkSoundEngine.StopAll(this.audioObject.gameObject);
      this.persistentVFXParticles.Clear();
      if (this.pilotRep != null)
        this.pilotRep.gameRep = (GameRepresentation)null;
      this.pilotRep = (PilotRepresentation)null;
      this._propertyBlock = (PropertyBlockManager)null;
      this.renderers.Clear();
      this._parentCombatant = (ICombatant)null;
      this._parentActor = (AbstractActor)null;
    }
    public virtual void GameRepresentation_OnCombatGameDestroyed() {
      if (this._Combat == null) { return; } 
      this._Combat.MessageCenter.RemoveSubscriber(MessageCenterMessageType.ActorMoveBegin, new ReceiveMessageCenterMessage(this._OnMovementBegin));
      this._Combat.MessageCenter.RemoveSubscriber(MessageCenterMessageType.ActorMoveEnd, new ReceiveMessageCenterMessage(this._OnMovementEnd));
    }
    public virtual void GameRepresentation_Init(ICombatant actor, Transform parentTransform, bool isParented, bool leaveMyParentTransformOutOfThis, string parentDisplayName) {
      if (actor != null) {
        this._Combat = actor.Combat;
        this._parentCombatant = actor;
        this._parentActor = actor as AbstractActor;
      }
      if (!leaveMyParentTransformOutOfThis) {
        this.parentTransform = (Transform)null;
        this.thisTransform.parent = (Transform)null;
      }
      if ((UnityEngine.Object)parentTransform != (UnityEngine.Object)null) {
        if (isParented) {
          this.parentTransform = parentTransform;
          this.thisTransform.parent = parentTransform;
          this.thisTransform.localPosition = Vector3.zero;
          this.thisTransform.localRotation = Quaternion.identity;
          this.thisTransform.localScale = Vector3.one;
        } else {
          this.thisTransform.localPosition = parentTransform.localPosition;
          this.thisTransform.localRotation = parentTransform.localRotation;
          this.thisTransform.localScale = parentTransform.localScale;
        }
      }
      this.persistentDmgList = actor == null ? new List<string>() : new List<string>((IEnumerable<string>)actor.Combat.Constants.VFXNames.persistentDamageNames);
      if (!((UnityEngine.Object)this.audioObject != (UnityEngine.Object)null))
        return;
      this.audioObject.listenerMask = 0;
      Pilot pilot = this.parentCombatant.GetPilot();
      if (pilot == null)
        return;
      if (!string.IsNullOrEmpty(pilot.pilotDef.Voice)) {
        WwiseManager.SetSwitch<AudioSwitch_dialog_character_type_pilots>(pilot.pilotVoice, this.audioObject);
        WwiseManager.SetSwitch<AudioSwitch_dialog_dark_light>(AudioSwitch_dialog_dark_light.light, this.audioObject);
      }
      this.pilotRep = new PilotRepresentation(this, pilot, this.audioObject);
    }
    protected virtual void GameRepresentation_Update() {
      if (isSlave) { return; }
      if (this._parentActor == null || !this._parentActor.IsTeleportedOffScreen || !EncounterLayerParent.encounterBegan)
        return;
      if ((double)this.timePlacedOffScreen < (double)this.timeLimitOffScreen) {
        this.timePlacedOffScreen += Time.deltaTime;
      } else {
        GameRepresentation.initLogger.LogError((object)"ENGAGING SAFETY TELEPORT", (UnityEngine.Object)this);
        this._Combat.ItemRegistry.GetItemByGUID<UnitSpawnPointGameLogic>(this._parentActor.spawnerGUID).TeleportUnitToSpawnPoint(this._parentActor);
      }
    }
    public virtual void GameRepresentation_LateUpdate() {
    }
    public virtual void GameRepresentation_OnPlayerVisibilityChanged(VisibilityLevel newLevel) {
    }
    public virtual void GameRepresentation_SetVFXColliderEnabled(bool isEnabled) {
    }
    public bool _isTargetable { get { return Traverse.Create(this).Property<bool>("isTargetable").Value; } set { Traverse.Create(this).Property<bool>("isTargetable").Value = value; } }
    public bool _isTargeted { get { return Traverse.Create(this).Property<bool>("isTargeted").Value; } set { Traverse.Create(this).Property<bool>("isTargeted").Value = value; } }
    public bool _isAvailable { get { return Traverse.Create(this).Property<bool>("isAvailable").Value; } set { Traverse.Create(this).Property<bool>("isAvailable").Value = value; } }
    public bool _isSelected { get { return Traverse.Create(this).Property<bool>("isSelected").Value; } set { Traverse.Create(this).Property<bool>("isSelected").Value = value; } }
    public bool _isHovered { get { return Traverse.Create(this).Property<bool>("isHovered").Value; } set { Traverse.Create(this).Property<bool>("isHovered").Value = value; } }
    public bool _isDead { get { return Traverse.Create(this).Property<bool>("isDead").Value; } set { Traverse.Create(this).Property<bool>("isDead").Value = value; } }
    public virtual bool __IsTargetable {
      get => this._isTargetable;
      set {
        this._isTargetable = value;
        foreach (CustomMechRepresentation slave in slaveRepresentations) { slave.__IsTargetable = value; }
        this._refreshHighlight();
      }
    }
    public virtual bool __IsTargeted {
      get => this._isTargeted;
      set {
        this._isTargeted = value;
        this._refreshHighlight();
        foreach (CustomMechRepresentation slave in slaveRepresentations) { slave.__IsTargeted = value; }
      }
    }
    public virtual bool __IsAvailable {
      get => this._isAvailable;
      set {
        this._isAvailable = value;
        foreach (CustomMechRepresentation slave in slaveRepresentations) { slave.__IsAvailable = value; }
        this._refreshHighlight();
      }
    }
    public virtual bool __IsSelected {
      get => this._isSelected;
      set {
        this._isSelected = value;
        foreach (CustomMechRepresentation slave in slaveRepresentations) { slave.__IsSelected = value; }
        this.refreshHighlight();
      }
    }
    public virtual bool __IsHovered {
      get => this._isHovered;
      set {
        this._isHovered = value;
        foreach (CustomMechRepresentation slave in slaveRepresentations) { slave.__IsHovered = value; }
        this._refreshHighlight();
      }
    }
    public virtual bool __IsDead {
      get => this._isDead;
      set {
        this._isDead = value;
        foreach (CustomMechRepresentation slave in slaveRepresentations) { slave.__IsDead = value; }
        this._refreshHighlight();
      }
    }
    public virtual void _InitHighlighting() {
      Log.TWL(0, "CustomMechRepresentation._InitHighlighting "+this.gameObject.name+" "+(this.customPropertyBlock == null?"null": this.customPropertyBlock.gameObject.name));
      this.edgeHighlight = this.GetComponent<MechEdgeSelection>();
      if (this.edgeHighlight != null) { Traverse.Create(this.edgeHighlight).Field<PropertyBlockManager>("propertyManager").Value = this.customPropertyBlock; }
      foreach (CustomMechRepresentation slave in slaveRepresentations) {
        slave._InitHighlighting();
      }
      if (isSlave == false) { this._refreshHighlight(); }
    }
    public virtual void _SetHighlightColor(CombatGameState combat, Team team) {
      if (this.edgeHighlight != null) {
        if (team.LocalPlayerControlsTeam) {
          this.edgeHighlight.SetTeam(0);
        } else {
          switch (combat.HostilityMatrix.GetHostilityOfLocalPlayer(team)) {
            case Hostility.FRIENDLY:
            this.edgeHighlight.SetTeam(1);
            break;
            case Hostility.ENEMY:
            this.edgeHighlight.SetTeam(3);
            break;
            default:
            this.edgeHighlight.SetTeam(2);
            break;
          }
        }
      }
      foreach (CustomMechRepresentation slave in slaveRepresentations) { slave._SetHighlightColor(combat, team); }
    }
    public virtual void _RefreshEdgeCache() {
      if (this.__propertyBlock != null) { this.__propertyBlock.UpdateCache(); }
      if (this.edgeHighlight != null) { this.edgeHighlight.RefreshCache(); }
      foreach (CustomMechRepresentation slave in slaveRepresentations) { slave._RefreshEdgeCache(); }
    }
    public virtual void _FadeIn(float length) {
      if (this.__propertyBlock != null) {
        Log.TWL(0, "GameRepresentation._FadeIn");
        bool MeshRendererCache_hasNulls = false;
        foreach (MeshRenderer renderer in this.__propertyBlock.MeshRendererCache) {
          Log.WL(1, "MeshRenderer:" + (renderer == null ? "null" : renderer.gameObject.name));
          if (renderer == null) { MeshRendererCache_hasNulls = true;  break; }
        }
        if (MeshRendererCache_hasNulls) {
          List<MeshRenderer> MeshRendererCache = new List<MeshRenderer>();
          foreach (MeshRenderer renderer in this.__propertyBlock.MeshRendererCache) {
            if (renderer != null) { MeshRendererCache.Add(renderer); }
          }
          Traverse.Create(this.__propertyBlock).Field<MeshRenderer[]>("meshRendererCache").Value = MeshRendererCache.ToArray();
        }
        bool SkinnedRendererCache_hasNulls = false;
        foreach (SkinnedMeshRenderer renderer in this.__propertyBlock.SkinnedRendererCache) {
          Log.WL(1, "SkinnedMeshRenderer:" + (renderer == null ? "null" : renderer.gameObject.name));
          if (renderer == null) { SkinnedRendererCache_hasNulls = true; break; }
        }
        if (SkinnedRendererCache_hasNulls) {
          List<SkinnedMeshRenderer> SkinnedRendererCache = new List<SkinnedMeshRenderer>();
          foreach (SkinnedMeshRenderer renderer in this.__propertyBlock.SkinnedRendererCache) {
            if (renderer != null) { SkinnedRendererCache.Add(renderer); }
          }
          Traverse.Create(this.__propertyBlock).Field<SkinnedMeshRenderer[]>("skinnedRendererCache").Value = SkinnedRendererCache.ToArray();
        }
        this.__propertyBlock.FadeIn(length);
      }
      foreach (CustomMechRepresentation slave in slaveRepresentations) { slave._FadeIn(length); }
    }
    public virtual void _FadeOut(float length) {
      if (this.__propertyBlock != null) {
        this.__propertyBlock.FadeOut(length);
      }
      this.StartCoroutine(this.TurnOffWhenFaded(length));
      foreach (CustomMechRepresentation slave in slaveRepresentations) { slave._FadeOut(length); }
    }
    private IEnumerator TurnOffWhenFaded(float seconds) {
      yield return new WaitForSeconds(seconds);
      this.gameObject.SetActive(false);
      yield break;
    }
    public virtual void _refreshHighlight() {
      if (this._isDead)
        this._SetHighlightIntensity(GameRepresentation.HighlightType.Dead);
      else if (this._isSelected)
        this._SetHighlightIntensity(GameRepresentation.HighlightType.Selected);
      else if (this._isTargeted)
        this._SetHighlightIntensity(GameRepresentation.HighlightType.Targeted);
      else if (this._isHovered)
        this._SetHighlightIntensity(GameRepresentation.HighlightType.Hovered);
      else if (this._isTargetable)
        this._SetHighlightIntensity(GameRepresentation.HighlightType.AvailableTarget);
      else if (this._isAvailable)
        this._SetHighlightIntensity(GameRepresentation.HighlightType.AvailableSelection);
      else
        this._SetHighlightIntensity(GameRepresentation.HighlightType.Default);
    }
    protected virtual void _SetHighlightAlpha(float alpha) {
      if (this.edgeHighlight != null) {
        if ((double)Mathf.Abs(this.edgeHighlight.Alpha - alpha) <= 1.0 / 1000.0)
          return;
        this.edgeHighlight.Alpha = alpha;
        this._refreshHighlight();
      }
    }
    protected virtual void _SetHighlightIntensity(GameRepresentation.HighlightType value) {
      if ((UnityEngine.Object)this.edgeHighlight == (UnityEngine.Object)null || value == this.currentHighlight)
        return;
      this.currentHighlight = value;
      switch (value) {
        case GameRepresentation.HighlightType.AvailableSelection:
        case GameRepresentation.HighlightType.AvailableTarget:
        this.edgeHighlight.isSelected = false;
        this.edgeHighlight.Intensity = CustomMechRepresentation.highlightDefaultIntensity;
        break;
        case GameRepresentation.HighlightType.Selected:
        case GameRepresentation.HighlightType.Targeted:
        case GameRepresentation.HighlightType.Hovered:
        this.edgeHighlight.Intensity = CustomMechRepresentation.highlightSelectedIntensity;
        this.edgeHighlight.isSelected = true;
        break;
        case GameRepresentation.HighlightType.Dead:
        this.edgeHighlight.Intensity = CustomMechRepresentation.hightlightDeadIntensity;
        this.edgeHighlight.isSelected = false;
        break;
        default:
        this.edgeHighlight.Intensity = CustomMechRepresentation.highlightOffIntensity;
        this.edgeHighlight.isSelected = false;
        break;
      }
    }
    protected virtual void _InitWindZone() {
      if (!((UnityEngine.Object)this.windZone != (UnityEngine.Object)null))
        return;
      this.windZone.gameObject.SetActive(false);
    }
    public virtual void _OnMovementBegin(MessageCenterMessage message) {
      if (this._Combat.FindActorByGUID((message as ActorMoveBeginMessage).affectedObjectGuid) != this.parentCombatant)
        return;
      this.StartCoroutine(this.BlendWind(this.baseWindIntensity, 0.5f));
    }

    public virtual void _OnMovementEnd(MessageCenterMessage message) {
      if (this._Combat.FindActorByGUID((message as ActorMoveEndMessage).affectedObjectGuid) != this.parentCombatant)
        return;
      this.StartCoroutine(this.BlendWind(0.0f, 2f));
    }

    private IEnumerator BlendWind(float target, float duration) {
      float currentWind = this.windZone.windMain;
      float t = 0.0f;
      while ((double)t < (double)duration) {
        this.windZone.windMain = Mathf.Lerp(currentWind, target, Mathf.Clamp01(t / duration));
        t += Time.deltaTime;
        yield return (object)null;
      }
    }

    public virtual void GameRepresentation_FaceTarget(
      bool isParellelSequence,
      ICombatant target,
      float twistTime,
      int stackItemUID,
      int sequenceId,
      bool isMelee,
      GameRepresentation.RotationCompleteDelegate completeDelegate) {
    }

    public virtual void GameRepresentation_FacePoint(
      bool isParellelSequence,
      Vector3 lookAt,
      bool isLookVector,
      float twistTime,
      int stackItemUID,
      int sequenceId,
      bool isMelee,
      GameRepresentation.RotationCompleteDelegate completeDelegate) {
    }

    public virtual void GameRepresentation_ReturnToNeutralFacing(
      bool isParellelSequence,
      float twistTime,
      int stackItemUID,
      int sequenceId,
      GameRepresentation.RotationCompleteDelegate completeDelegate) {
    }

    public virtual void GameRepresentation_PlayFireAnim(AttackSourceLimb sourceLimb, int recoilStrength) {
    }

    public virtual void GameRepresentation_PlayMeleeAnim(int meleeHeight) {
    }

    public virtual void GameRepresentation_PlayImpactAnim(WeaponHitInfo hitInfo,int hitIndex, Weapon weapon,MeleeAttackType meleeType,float cumulativeDamage) {
    }

    public virtual void GameRepresentation_PlayJumpLaunchAnim() {
    }

    public virtual void GameRepresentation_PlayFallingAnim(Vector2 direction) {
    }

    public virtual void GameRepresentation_UpdateJumpAirAnim(float forward, float side) {
    }

    public virtual void GameRepresentation_PlayJumpLandAnim(bool isDFA) {
    }

    public virtual void GameRepresentation_PlayKnockdownAnim(Vector2 attackDirection) {
    }

    public virtual void GameRepresentation_PlayStandAnim() {
    }

    public virtual void GameRepresentation_PlayShutdownAnim() {
    }

    public virtual void GameRepresentation_PlayStartupAnim() {
    }

    public virtual void GameRepresentation_HandleDeath(DeathMethod deathMethod, int location) {
      this.gameObject.layer = LayerMask.NameToLayer("UI_IgnoreMouseEvents");
      if (deathMethod == DeathMethod.DespawnedNoMessage || deathMethod == DeathMethod.DespawnedEscaped)
        this.FadeOut(1f);
      this.__IsDead = true;
    }

    public virtual void GameRepresentation_OnFootFall(int leftFoot) {
    }

    public virtual void GameRepresentation_OnAudioEvent(string audioEvent) {
    }

    public virtual void GameRepresentation_OnVFXEvent(AnimationEvent animEvent) {
    }

    public virtual void GameRepresentation_OnDeath() {
    }

    public virtual void GameRepresentation_OnGroundImpact() {
    }

    public virtual void GameRepresentation_OnMeleeImpact(AnimationEvent animEvent) {
    }

    public virtual void GameRepresentation_OnJumpLand() {
    }

    public virtual Vector3 GameRepresentation_GetHitPosition(int location) => this.thisTransform.position;

    public virtual Vector3 GameRepresentation_GetMissPosition(
      Vector3 attackOrigin,
      Weapon weapon,
      NetworkRandom random) {
      return this.thisTransform.position;
    }

    public virtual Transform GameRepresentation_GetVFXTransform(int location) => this.thisTransform;

    public virtual void GameRepresentation_PlayVFX(
      int location,
      string vfxName,
      bool attached,
      Vector3 lookAtPos,
      bool oneShot,
      float duration) {
    }

    public virtual ParticleSystem GameRepresentation_PlayVFXAt(Transform parentTransform,Vector3 offset,string vfxName,bool attached,Vector3 lookAtPos,bool oneShot,float duration) {
      if (string.IsNullOrEmpty(vfxName)) return (ParticleSystem)null;
      GameObject gameObject = this.parentCombatant.Combat.DataManager.PooledInstantiate(vfxName, BattleTechResourceType.Prefab);
      if ((UnityEngine.Object)gameObject == (UnityEngine.Object)null) {
        GameRepresentation.initLogger.LogError((object)("Error instantiating VFX " + vfxName), (UnityEngine.Object)this);
        return (ParticleSystem)null;
      }
      ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
      ParticleSystem.MainModule main = component.main;
      main.scalingMode = ParticleSystemScalingMode.Hierarchy;
      //Log.TWL(0, "GameRepresentation_PlayVFXAt "+this.gameObject.name+ gameObject.name+" "+ main.scalingMode);
      component.Stop(true); 
      component.Clear(true);
      Transform transform = gameObject.transform;
      transform.SetParent((Transform)null);
      BTWindZone componentInChildren1 = gameObject.GetComponentInChildren<BTWindZone>(true);
      if ((UnityEngine.Object)componentInChildren1 != (UnityEngine.Object)null && componentInChildren1.enabled)
        componentInChildren1.ResetZero();
      BTLightAnimator componentInChildren2 = gameObject.GetComponentInChildren<BTLightAnimator>(true);
      if (attached) {
        transform.SetParent(parentTransform, false);
        transform.localPosition = offset;
      } else {
        transform.localPosition = Vector3.zero;
        if ((UnityEngine.Object)parentTransform != (UnityEngine.Object)null)
          transform.position = parentTransform.position;
        transform.position += offset;
      }
      if (lookAtPos != Vector3.zero)
        transform.LookAt(lookAtPos);
      else
        transform.localRotation = Quaternion.identity;
      transform.localScale = Vector3.one;
      if(this.customRep != null) {
        if(this.customRep.CustomDefinition != null) {
          transform.localScale = this.customRep.CustomDefinition.vfxScale.vector;
        }
      }
      //Log.WL(1, "transform.localScale " + transform.localScale);
      if (oneShot) {
        AutoPoolObject autoPoolObject = gameObject.GetComponent<AutoPoolObject>();
        if ((UnityEngine.Object)autoPoolObject == (UnityEngine.Object)null)
          autoPoolObject = gameObject.AddComponent<AutoPoolObject>();
        if ((double)duration > 0.0)
          autoPoolObject.Init(this.parentCombatant.Combat.DataManager, vfxName, duration);
        else
          autoPoolObject.Init(this.parentCombatant.Combat.DataManager, vfxName, component);
      } else {
        List<ParticleSystem> particleSystemList = (List<ParticleSystem>)null;
        if (this.persistentVFXParticles.TryGetValue(vfxName, out particleSystemList)) {
          particleSystemList.Add(component);
          this.persistentVFXParticles[vfxName] = particleSystemList;
        } else {
          particleSystemList = new List<ParticleSystem>();
          particleSystemList.Add(component);
          this.persistentVFXParticles[vfxName] = particleSystemList;
        }
      }
      BTCustomRenderer.SetVFXMultiplier(component);
      component.Play(true);
      if ((UnityEngine.Object)componentInChildren1 != (UnityEngine.Object)null)
        componentInChildren1.PlayAnimCurve();
      if ((UnityEngine.Object)componentInChildren2 != (UnityEngine.Object)null)
        componentInChildren2.PlayAnimation();
      return component;
    }

    public virtual void GameRepresentation_PlayPersistentDamageVFX(int location) {
    }

    public virtual void GameRepresentation_PlayComponentDestroyedVFX(int location, Vector3 attackDirection) {
    }

    public virtual void GameRepresentation_PlayDeathVFX(DeathMethod deathMethod, int location) {
    }

    public virtual void _StopManualPersistentVFX(string vfxName) {
      List<ParticleSystem> particleSystemList = new List<ParticleSystem>();
      if (!this.persistentVFXParticles.TryGetValue(vfxName, out particleSystemList))
        return;
      for (int index = 0; index < particleSystemList.Count; ++index) {
        ParticleSystem particles = particleSystemList[index];
        AutoPoolObject autoPoolObject = particles.gameObject.GetComponent<AutoPoolObject>();
        if ((UnityEngine.Object)autoPoolObject == (UnityEngine.Object)null)
          autoPoolObject = particles.gameObject.AddComponent<AutoPoolObject>();
        autoPoolObject.Init(this.parentCombatant.Combat.DataManager, vfxName, particles);
        particles.Stop(true);
      }
      particleSystemList.Clear();
      this.persistentVFXParticles.Remove(vfxName);
    }

    public virtual void _PauseAllPersistentVFX() {
      foreach (string key in this.persistentVFXParticles.Keys) {
        foreach (ParticleSystem particleSystem in this.persistentVFXParticles[key])
          particleSystem.Stop(true);
      }
    }

    public virtual void _ResumeAllPersistentVFX() {
      foreach (string key in this.persistentVFXParticles.Keys) {
        foreach (ParticleSystem vfxParent in this.persistentVFXParticles[key]) {
          BTCustomRenderer.SetVFXMultiplier(vfxParent);
          vfxParent.Play(true);
        }
      }
    }

    public virtual void _StopAllPersistentVFXAttachedToLocation(int location) {
      Transform vfxTransform = this.GetVFXTransform(location);
      List<string> stringList = new List<string>((IEnumerable<string>)this.persistentVFXParticles.Keys);
      for (int index1 = stringList.Count - 1; index1 >= 0; --index1) {
        string str = stringList[index1];
        List<ParticleSystem> particleSystemList = (List<ParticleSystem>)null;
        if (this.persistentVFXParticles.TryGetValue(str, out particleSystemList)) {
          for (int index2 = particleSystemList.Count - 1; index2 >= 0; --index2) {
            if ((UnityEngine.Object)particleSystemList[index2].transform.parent == (UnityEngine.Object)vfxTransform) {
              ParticleSystem particles = particleSystemList[index2];
              AutoPoolObject autoPoolObject = particles.gameObject.GetComponent<AutoPoolObject>();
              if ((UnityEngine.Object)autoPoolObject == (UnityEngine.Object)null)
                autoPoolObject = particles.gameObject.AddComponent<AutoPoolObject>();
              autoPoolObject.Init(this.parentCombatant.Combat.DataManager, str, particles);
              particles.Stop(true);
            }
          }
          if (particleSystemList.Count < 1) {
            particleSystemList.Clear();
            this.persistentVFXParticles.Remove(str);
          } else
            this.persistentVFXParticles[str] = particleSystemList;
        }
      }
    }
    public virtual MechDestructibleObject GameRepresentation_GetDestructibleObject(int location) => (MechDestructibleObject)null;
    public override void OnPointerClick(PointerEventData eventData) {
      if (this.parentCombatant == null)
        return;
      if (eventData.button == PointerEventData.InputButton.Left) {
        this.parentCombatant.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new ActorClickedMessage(this.parentCombatant.GUID));
      } else {
        if (eventData.button != PointerEventData.InputButton.Right)
          return;
        this.parentCombatant.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new ActorRightClickedMessage(this.parentCombatant.GUID));
      }
    }
    public override void OnPointerEnter(PointerEventData eventData) {
      if (this.parentCombatant == null)
        return;
      this.parentCombatant.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new ActorHoveredMessage(this.parentCombatant.GUID));
    }
    public override void OnPointerExit(PointerEventData eventData) {
      if (this.parentCombatant == null)
        return;
      this.parentCombatant.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new ActorUnHoveredMessage(this.parentCombatant.GUID));
    }
  }
}