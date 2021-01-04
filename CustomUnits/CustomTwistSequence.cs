using BattleTech;
using HBS.Math;
using HBS.Util;
using System.Collections.Generic;
using UnityEngine;

namespace CustomUnits {
  public class CustomTwistSequence : MultiSequence {
    public AbstractActor owningActor;
    private PilotableActorRepresentation actorRep;
    private AlternateMechRepresentation altRep;
    private Animator actorAnim;
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
      AlternateMechRepresentations altReps = actor.GameRep.GetComponent<AlternateMechRepresentations>();
      this.altRep = null;
      if (altReps != null) {
        this.actorRep = altReps.currentRepresentation;
        this.altRep = altReps.currentAltRep;
      } else {
        this.actorRep = actor.GameRep as PilotableActorRepresentation;
      }
      Log.WL(1, "current representation:"+ this.actorRep.name);
      this.actorAnim = this.actorRep.thisAnimator;
      CustomTwistAnimation custAnimator = this.actorRep.GetComponent<CustomTwistAnimation>();
      if (custAnimator != null) {
        if(custAnimator.mechTurret != null) {
          if(custAnimator.mechTurret.turnAnimator != null) {
            this.actorAnim = custAnimator.mechTurret.turnAnimator;
            Log.WL(1, "mech turret found:" + this.actorAnim.name);
          }
        }
      }
      QuadRepresentation quadRepresentation = this.actorRep.GetComponent<QuadRepresentation>();
      this.isBodyRotation = isBodyRotation;
      if ((quadRepresentation != null)&&(this.actorAnim == this.actorRep.thisAnimator)) {
        this.isBodyRotation = true;
        Log.WL(1, "quad without turret");
      }
      if (altReps != null) {
        if (altReps.isHovering) {
          Log.WL(1, "hovering mech");
          this.isBodyRotation = true;
        }
      }
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
        this.desiredRotation = Quaternion.LookRotation(this.desiredLookDirection);
        this.skipTwist = (double)Mathf.Abs(Quaternion.Angle(startingRotation, desiredRotation)) < 0.0500000007450581;
      } else {
        this.skipTwist = (double)Mathf.Abs(this.angleDifference) < 0.0500000007450581;
      }
      if ((double)this.twistTime <= 0.0) { this.twistTime = 1f; }
      this.twistRate = 1f / this.twistTime;
      Log.WL(1, "isBodyRotation:" + this.isBodyRotation+ " isUpdateRotation:" + this.isUpdateRotation + " skipTwist:" + this.skipTwist+ " twistRate:"+ this.twistRate+ " startingRotation:"+ this.startingRotation+ " desiredRotation:"+ this.desiredRotation);
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
          this.actorAnim.SetFloat("Twist", this.actorRep.currentTwistAngle);
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