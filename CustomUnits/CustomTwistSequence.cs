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
using HBS.Math;
using HBS.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomUnits {
  public class CustomTwistSequence : MultiSequence {
    public AbstractActor owningActor;
    private PilotableActorRepresentation actorRep;
    private CustomMechRepresentation customRep;
    private Animator defaultAnim;
    private CustomTwistSequence.TwistState state;
    private float timeInCurrentState;
    private bool isBodyRotation;
    private bool isUpdateRotation;
    private Vector3 startingForward;
    private Vector3 desiredLookDirection;
    private float startAngle;
    private float desiredAngle;
    private float angleDifference;
    private Quaternion startingRotation;
    private Quaternion desiredRotation;
    private float t;
    public float twistTime = 0.5f;
    private float twistRate;
    private int attackStackItemUID;
    private int attackSequenceId;
    private bool skipTwist;
    private GameRepresentation.RotationCompleteDelegate rotationComplete;
    private bool ordersAreComplete;
    private VTOLBodyAnimation vtolAnim;
    private CustomTwistAnimation customTwistAnim;
    public CustomTwistSequence(AbstractActor actor, Vector3 lookAt, bool isLookVector, bool isBodyRotation, float twistDuration, int stackItemUID, int sequenceId, GameRepresentation.RotationCompleteDelegate completeDelegate) : base(actor.Combat) {
      Log.TWL(0, "CustomTwistSequence " + actor.DisplayName + " isBodyRotation:" + isBodyRotation + " lookAt:" + lookAt);
      this.isUpdateRotation = isBodyRotation;
      this.owningActor = actor;
      this.actorRep = actor.GameRep as PilotableActorRepresentation;
      this.customRep = actor.GameRep as CustomMechRepresentation;
      Log.WL(1, "current representation:"+ this.actorRep.name);
      this.defaultAnim = this.actorRep.thisAnimator;
      CustomTwistAnimation custAnimator = this.actorRep.GetComponent<CustomTwistAnimation>();
      if (custAnimator != null) {
        if(custAnimator.HasTurret) { this.defaultAnim = null; }
      }
      this.isBodyRotation = isBodyRotation;
      this.vtolAnim = this.owningActor.VTOLAnimation();
      this.customTwistAnim = this.actorRep.gameObject.GetComponent<CustomTwistAnimation>();
      this.attackStackItemUID = stackItemUID;
      this.attackSequenceId = sequenceId;
      this.twistTime = twistDuration;
      this.rotationComplete = completeDelegate;
      this.desiredLookDirection = !isLookVector ? lookAt - this.owningActor.CurrentPosition : lookAt;
      this.startingForward = this.actorRep.thisTransform.forward;
      this.desiredAngle = NvMath.AngleSigned(this.startingForward, this.desiredLookDirection, Vector3.up);
      this.desiredAngle /= 90f;
      this.startAngle = this.actorRep.currentTwistAngle;
      this.angleDifference = this.desiredAngle - this.startAngle;
      if (this.isBodyRotation) {
        this.startingRotation = this.actorRep.thisTransform.rotation;
        this.desiredRotation = this.desiredLookDirection.sqrMagnitude > Core.Epsilon ? Quaternion.LookRotation(this.desiredLookDirection) : this.actorRep.thisTransform.rotation;
        this.skipTwist = (double)Mathf.Abs(Quaternion.Angle(startingRotation, desiredRotation)) < 0.0500000007450581;
      } else {
        this.skipTwist = (double)Mathf.Abs(this.angleDifference) < 0.0500000007450581;
      }
      if ((double)this.twistTime <= 0.0) { this.twistTime = 1f; }
      this.twistRate = 1f / this.twistTime;
      Log.WL(1, "isBodyRotation:" + this.isBodyRotation 
        + " isUpdateRotation:" + this.isUpdateRotation 
        + " skipTwist:" + this.skipTwist+ " twistRate:"+ this.twistRate
        + " startAngle:" + this.startAngle + " desiredAngle:" + this.desiredAngle
        + " startingRotation:" + this.startingRotation+ " desiredRotation:"+ this.desiredRotation
        );
      //Log.WL(1,Environment.StackTrace);
    }

    private void update() {
      this.timeInCurrentState += Time.deltaTime;
      switch (this.state) {
        case CustomTwistSequence.TwistState.MeleeFacing:
          this.t += this.twistRate * Time.deltaTime;
          if ((double)this.t >= 1.0) { this.t = 1f; }
          this.actorRep.thisTransform.rotation = Quaternion.Lerp(this.startingRotation, this.desiredRotation, this.t);
          if (isUpdateRotation) {
            this.owningActor.OnPositionUpdate(this.actorRep.thisTransform.position, this.actorRep.thisTransform.rotation, this.SequenceGUID, true, (List<DesignMaskDef>)null, true);
          }
          if ((double)this.t < 1.0) { break; };
          this.setState(CustomTwistSequence.TwistState.Finished);
        break;
        case CustomTwistSequence.TwistState.RangedTwisting:
          this.t += this.twistRate * Time.deltaTime;
          if ((double)this.t >= 1.0) { this.t = 1f; }
          this.actorRep.currentTwistAngle = this.startAngle + this.angleDifference * this.t;
          if (this.customRep != null) {
            this.customRep.Twist(this.actorRep.currentTwistAngle);
          }else{
            if (defaultAnim != null) { this.defaultAnim.SetFloat("Twist", this.actorRep.currentTwistAngle); };
          }
          if (vtolAnim != null) { vtolAnim.twist(this.actorRep.currentTwistAngle); }
          if (customTwistAnim != null) { customTwistAnim.twist(this.actorRep.currentTwistAngle); }
          if ((double)this.t < 1.0) { break; }
          this.setState(CustomTwistSequence.TwistState.Finished);
        break;
      }
    }

    public override bool IsComplete => base.IsComplete && this.ordersAreComplete;

    private void setState(CustomTwistSequence.TwistState newState) {
      if (this.state == newState)
        return;
      this.state = newState;
      this.timeInCurrentState = 0.0f;
      this.t = 0.0f;
      switch (newState) {
        case CustomTwistSequence.TwistState.MeleeFacing:
        int num1 = (int)WwiseManager.PostEvent<AudioEventList_torso>(AudioEventList_torso.torso_rotate_start, this.owningActor.GameRep.audioObject);
        break;
        case CustomTwistSequence.TwistState.RangedTwisting:
        int num2 = (int)WwiseManager.PostEvent<AudioEventList_torso>(AudioEventList_torso.torso_rotate_start, this.owningActor.GameRep.audioObject);
        break;
        case CustomTwistSequence.TwistState.Finished:
        int num3 = (int)WwiseManager.PostEvent<AudioEventList_torso>(AudioEventList_torso.torso_rotate_stop, this.owningActor.GameRep.audioObject);
        this.ordersAreComplete = true;
        break;
      }
    }

    public override System.Type DesiredParentType => typeof(AttackStackSequence);

    public override void OnComplete() {
      if (this.rotationComplete != null)
        this.rotationComplete(this.attackStackItemUID, this.attackSequenceId);
      base.OnComplete();
    }

    public override void OnUpdate() {
      this.update();
      base.OnUpdate();
    }

    public override void OnAdded() {
      base.OnAdded();
      if (this.skipTwist) {
        this.setState(CustomTwistSequence.TwistState.Finished);
        base.OnUpdate();
      } else if (this.isBodyRotation)
        this.setState(CustomTwistSequence.TwistState.MeleeFacing);
      else
        this.setState(CustomTwistSequence.TwistState.RangedTwisting);
    }

    public override int Size() => base.Size();

    public override bool ShouldSave() => base.ShouldSave();

    public override void Save(SerializationStream stream) => base.Save(stream);

    public override void Load(SerializationStream stream) => base.Load(stream);

    public override void LoadComplete() => base.LoadComplete();

    public enum TwistState {
      None,
      MeleeFacing,
      RangedTwisting,
      Finished,
    }
  }
}