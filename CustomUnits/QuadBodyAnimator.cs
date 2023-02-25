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
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CustomUnits {
  public class QuadBodyAnimationData {
    public string VerticalRotate { get; set; }
    public string TurretAttach { get; set; }
    public string FrontLegsAttach { get; set; }
    public string DamageAnimator { get; set; }
    public string RTVFXTransform { get; set; }
    public string LTVFXTransform { get; set; }
    public string CTVFXTransform { get; set; }
    public string HEADVFXTransform { get; set; }
    public bool ShowFrontLegsAxis { get; set; }
    public List<string> JumpJetsSpawnPoints { get; set; }
    public List<string> HeadLightSpawnPoints { get; set; }
    public QuadBodyAnimationData() {
      VerticalRotate = string.Empty;
      TurretAttach = string.Empty;
      FrontLegsAttach = string.Empty;
      ShowFrontLegsAxis = false;
      DamageAnimator = string.Empty;
      RTVFXTransform = string.Empty;
      LTVFXTransform = string.Empty;
      CTVFXTransform = string.Empty;
      HEADVFXTransform = string.Empty;
      JumpJetsSpawnPoints = new List<string>();
      HeadLightSpawnPoints = new List<string>();
    }
  };
  //public class QuadBodyAnimation : GenericAnimatedComponent, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler {
  //  public QuadRepresentation parentRepresentation { get; set; }
  //  //public QuadRepresentationSimGame parentRepresentationSimGame { get; set; }
  //  public Transform VerticalRotate { get; set; }
  //  public Transform TurretAttach { get; set; }
  //  public Transform FrontLegsAttach { get; set; }
  //  public Transform RTVFXTransform { get; set; }
  //  public Transform LTVFXTransform { get; set; }
  //  public Transform CTVFXTransform { get; set; }
  //  public Transform HEADVFXTransform { get; set; }
  //  public List<Transform> JumpJetsSpawnPoints { get; set; }
  //  public List<Transform> HeadLightSpawnPoints { get; set; }
  //  public Animator DamageAnimator { get; set; }
  //  public float Angle { get; set; }
  //  public float HeightDiff { get; set; }
  //  public float DistDiff { get; set; }
  //  public bool ShowFrontLegsAxis { get; private set; }
  //  public Transform frontLegsAxis { get; private set; }
  //  public override bool StayOnDeath() { return true; }
  //  public override bool StayOnLocationDestruction() { return true; }

  //  private bool RepresentationInited;
  //  public QuadBodyAnimation():base() {
  //    JumpJetsSpawnPoints = new List<Transform>();
  //    HeadLightSpawnPoints = new List<Transform>();
  //  }
  //  public void OnPointerClick(PointerEventData eventData) {
  //    if (parentRepresentation == null) { return; }
  //    parentRepresentation.mechRep.OnPointerClick(eventData);
  //    //throw new System.NotImplementedException();
  //  }
  //  public void OnPointerEnter(PointerEventData eventData) {
  //    if (parentRepresentation == null) { return; }
  //    parentRepresentation.mechRep.OnPointerEnter(eventData);
  //    //throw new System.NotImplementedException();
  //  }
  //  public void OnPointerExit(PointerEventData eventData) {
  //    if (parentRepresentation == null) { return; }
  //    parentRepresentation.mechRep.OnPointerExit(eventData);
  //    //throw new System.NotImplementedException();
  //  }
  //  public void CollapseLocation(int location) {
  //    Mech mech = parent as Mech;
  //    TrooperSquad squad = parent as TrooperSquad;
  //    if ((mech != null)&&(squad == null)) {
  //      LocationDamageLevel LTDamageLevel = mech.LeftTorsoDamageLevel;
  //      LocationDamageLevel RTDamageLevel = mech.RightTorsoDamageLevel;
  //      if(mech.CenterTorsoDamageLevel == LocationDamageLevel.Destroyed) {
  //        LTDamageLevel = LocationDamageLevel.Destroyed; RTDamageLevel = LocationDamageLevel.Destroyed;
  //      }else
  //      if (mech.HeadDamageLevel == LocationDamageLevel.Destroyed) {
  //        LTDamageLevel = LocationDamageLevel.Destroyed; RTDamageLevel = LocationDamageLevel.Destroyed;
  //      }
  //      this.SetSideTorsoDamage(LTDamageLevel, RTDamageLevel);
  //    }
  //  }
  //  public void SetSideTorsoDamage(LocationDamageLevel leftTorsoDamage, LocationDamageLevel rightTorsoDamage) {
  //    if(this.DamageAnimator != null) {
  //      this.DamageAnimator.SetBool("RTDamage", rightTorsoDamage == LocationDamageLevel.Destroyed);
  //      this.DamageAnimator.SetBool("LTDamage", leftTorsoDamage == LocationDamageLevel.Destroyed);
  //    }
  //  }
  //  public void MoveVfxToSelfTransform(Transform src, Transform dest) {
  //    if (src == null) { return; }
  //    if (dest == null) { return; }
  //    HashSet<Transform> childs = new HashSet<Transform>();
  //    for (int t = 0; t < src.childCount; ++t) {
  //      Transform chTR = src.GetChild(t);
  //      if (chTR != this.transform) { childs.Add(chTR); };
  //    }
  //    foreach (Transform ch in childs) {
  //      Vector3 locPos = ch.localPosition;
  //      ch.SetParent(dest, false);
  //      ch.localPosition = locPos;
  //    }
  //  }
  //  public void InitRepresentationSimGame() {
  //    if(this.parentRepresentationSimGame != null) {
  //      LocationDamageLevel LTDamageLevel = this.parentRepresentationSimGame.RLegsRepresentation.mechDef.LeftTorso.CurrentInternalStructure <= 0f?LocationDamageLevel.Destroyed:LocationDamageLevel.Functional;
  //      LocationDamageLevel RTDamageLevel = this.parentRepresentationSimGame.RLegsRepresentation.mechDef.RightTorso.CurrentInternalStructure <= 0f ? LocationDamageLevel.Destroyed : LocationDamageLevel.Functional; ;
  //      if (this.parentRepresentationSimGame.RLegsRepresentation.mechDef.CenterTorso.CurrentInternalStructure <= 0f) {
  //        LTDamageLevel = LocationDamageLevel.Destroyed; RTDamageLevel = LocationDamageLevel.Destroyed;
  //      } else
  //      if (this.parentRepresentationSimGame.RLegsRepresentation.mechDef.Head.CurrentInternalStructure <= 0f) {
  //        LTDamageLevel = LocationDamageLevel.Destroyed; RTDamageLevel = LocationDamageLevel.Destroyed;
  //      }
  //      this.SetSideTorsoDamage(LTDamageLevel, RTDamageLevel);
  //    }
  //    RepresentationInited = true;
  //  }
  //  public void InitRepresentation() {
  //    if (frontLegsAxis != null) { GameObject.Destroy(frontLegsAxis.gameObject); frontLegsAxis = null; }
  //    frontLegsAxis = new GameObject("frontLegsAxis").transform;
  //    frontLegsAxis.SetParent(parentRepresentation.fLegsRep.LegsRep.vfxCenterTorsoTransform);
  //    Vector3 localPos = VerticalRotate.transform.position - parentRepresentation.mechRep.vfxCenterTorsoTransform.position;
  //    frontLegsAxis.localPosition = localPos;
  //    if (ShowFrontLegsAxis) {
  //      GameObject axis = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
  //      axis.transform.SetParent(frontLegsAxis);
  //      axis.transform.localPosition = Vector3.zero;
  //      axis.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
  //      axis.transform.localScale = new Vector3(1f, 6f, 1f);
  //    }
  //    if (TurretAttach != null) {
  //      MechTurretAnimation mechTurret = this.parentRepresentation.GetComponentInChildren<MechTurretAnimation>(true);
  //      if (mechTurret != null) {
  //        mechTurret.transform.SetParent(TurretAttach);
  //        mechTurret.transform.localPosition = Vector3.zero;
  //        mechTurret.transform.localRotation = Quaternion.identity;
  //      };
  //    }
  //    if (this.DamageAnimator != null) {
  //      this.DamageAnimator.SetBool("RTDamage", this.parentRepresentation.mechRep.parentMech.RightTorsoDamageLevel != LocationDamageLevel.Destroyed);
  //      this.DamageAnimator.SetBool("LTDamage", this.parentRepresentation.mechRep.parentMech.LeftTorsoDamageLevel != LocationDamageLevel.Destroyed);
  //    }
  //    MoveVfxToSelfTransform(this.parentRepresentation.mechRep.vfxRightTorsoTransform, RTVFXTransform);
  //    MoveVfxToSelfTransform(this.parentRepresentation.mechRep.vfxLeftTorsoTransform, LTVFXTransform);
  //    MoveVfxToSelfTransform(this.parentRepresentation.mechRep.vfxCenterTorsoTransform, CTVFXTransform);
  //    MoveVfxToSelfTransform(this.parentRepresentation.mechRep.vfxHeadTransform, HEADVFXTransform);
  //    if (RTVFXTransform != null) this.parentRepresentation.mechRep.vfxRightTorsoTransform = RTVFXTransform;
  //    if (LTVFXTransform != null) this.parentRepresentation.mechRep.vfxLeftTorsoTransform = LTVFXTransform;
  //    if (CTVFXTransform != null) this.parentRepresentation.mechRep.vfxCenterTorsoTransform = CTVFXTransform;
  //    if (HEADVFXTransform != null) this.parentRepresentation.mechRep.vfxHeadTransform = HEADVFXTransform;
  //    Log.TWL(0, "QuadBodyAnimation.InitRepresentation "+ this.JumpJetsSpawnPoints.Count+" "+ Core.Settings.CustomJumpJetsComponentPrefab);
  //    if((this.JumpJetsSpawnPoints.Count > 0)&&(string.IsNullOrEmpty(Core.Settings.CustomJumpJetsComponentPrefab) == false)) {
  //      List<JumpjetRepresentation> jumpjetReps = Traverse.Create(this.parentRepresentation.mechRep).Field<List<JumpjetRepresentation>>("jumpjetReps").Value;
  //      GameObject jumpJetSrcPrefab = this.parentRepresentation.mechRep.parentActor.Combat.DataManager.PooledInstantiate(Core.Settings.CustomJumpJetsComponentPrefab, BattleTechResourceType.Prefab);
  //      Log.WL(0, "jumpJetSrcPrefab:" + (jumpJetSrcPrefab == null ? "null" : jumpJetSrcPrefab.name));
  //      if (jumpJetSrcPrefab != null) {
  //        //jumpJetSrcPrefab.printComponents(1);
  //        Transform jumpJetSrc = jumpJetSrcPrefab.transform.FindRecursive(Core.Settings.CustomJumpJetsPrefabSrcObjectName);
  //        Log.WL(0, "jumpJetSrc:" + (jumpJetSrc == null ? "null" : jumpJetSrc.name));
  //        if (jumpJetSrc != null) {
  //          foreach (Transform jumpJetAttach in this.JumpJetsSpawnPoints) {
  //            GameObject jumpJetBase = new GameObject("jumpJet");
  //            jumpJetBase.transform.SetParent(jumpJetAttach);
  //            jumpJetBase.transform.localPosition = Vector3.zero;
  //            jumpJetBase.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
  //            GameObject jumpJet = GameObject.Instantiate(jumpJetSrc.gameObject);
  //            jumpJet.transform.SetParent(jumpJetBase.transform);
  //            jumpJet.transform.localPosition = Vector3.zero;
  //            jumpJet.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
  //            JumpjetRepresentation jRep = jumpJetBase.AddComponent<JumpjetRepresentation>();
  //            jRep.Init(this.parent, jumpJetAttach, true, false, this.parentRepresentation.mechRep.name);
  //            jumpjetReps.Add(jRep);
  //          }
  //        }
  //        this.parentRepresentation.mechRep.parentActor.Combat.DataManager.PoolGameObject(Core.Settings.CustomJumpJetsPrefabSrc, jumpJetSrcPrefab);
  //      }
  //    }
  //    if ((this.HeadLightSpawnPoints.Count > 0) && (string.IsNullOrEmpty(Core.Settings.CustomHeadlightComponentPrefab) == false)) {
  //      List<GameObject> headlightsReps = Traverse.Create(this.parentRepresentation.mechRep).Field<List<GameObject>>("headlightReps").Value;
  //      GameObject headlightSrcPrefab = this.parentRepresentation.mechRep.parentActor.Combat.DataManager.PooledInstantiate(Core.Settings.CustomHeadlightComponentPrefab, BattleTechResourceType.Prefab);
  //      Log.WL(0, "headlightSrcPrefab:" + (headlightSrcPrefab == null ? "null" : headlightSrcPrefab.name));
  //      if (headlightSrcPrefab != null) {
  //        //jumpJetSrcPrefab.printComponents(1);
  //        Transform headlightSrc = headlightSrcPrefab.transform.FindRecursive(Core.Settings.CustomHeadlightPrefabSrcObjectName);
  //        Log.WL(0, "headlightSrc:" + (headlightSrc == null ? "null" : headlightSrc.name));
  //        if (headlightSrc != null) {
  //          foreach (Transform headlightAttach in this.HeadLightSpawnPoints) {
  //            GameObject headlightBase = new GameObject("headlight");
  //            headlightBase.transform.SetParent(headlightAttach);
  //            headlightBase.transform.localPosition = Vector3.zero;
  //            headlightBase.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
  //            GameObject headlight = GameObject.Instantiate(headlightSrc.gameObject);
  //            headlight.transform.SetParent(headlightBase.transform);
  //            headlight.transform.localPosition = Vector3.zero;
  //            headlight.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
  //            headlightsReps.Add(headlightBase);
  //          }
  //        }
  //        this.parentRepresentation.mechRep.parentActor.Combat.DataManager.PoolGameObject(Core.Settings.CustomHeadlightComponentPrefab, headlightSrcPrefab);
  //      }
  //    }
  //    RepresentationInited = true;
  //  }
  //  public void Update() {
  //    if (VerticalRotate == null) { return; }
  //    if ((parentRepresentation == null) && (RepresentationInited == true)) { return; }
  //    if (parentRepresentation == null) {
  //      if (this.parent != null) {
  //        this.parentRepresentation = this.parent.GameRep.gameObject.GetComponent<QuadRepresentation>();
  //        if (this.parentRepresentation != null) {
  //          this.InitRepresentation();
  //        } else { RepresentationInited = true; }
  //      } else {
  //        this.parentRepresentationSimGame = this.gameObject.GetComponentInParent<QuadRepresentationSimGame>();
  //        if(this.parentRepresentationSimGame != null) {
  //          InitRepresentationSimGame();
  //        }
  //      }
  //      return;
  //    }
  //    VerticalRotate.LookAt(frontLegsAxis, Vector3.up);
  //  }
  //  public override void Init(ICombatant a, int loc, string data, string prefabName) {
  //    base.Init(a, loc, data, prefabName);
  //    QuadBodyAnimationData srdata = JsonConvert.DeserializeObject<QuadBodyAnimationData>(data);
  //    if (string.IsNullOrEmpty(srdata.VerticalRotate) == false) { VerticalRotate = this.transform.FindRecursive(srdata.VerticalRotate); }
  //    if (string.IsNullOrEmpty(srdata.TurretAttach) == false) { TurretAttach = this.transform.FindRecursive(srdata.TurretAttach); }
  //    if (string.IsNullOrEmpty(srdata.FrontLegsAttach) == false) { FrontLegsAttach = this.transform.FindRecursive(srdata.FrontLegsAttach); }
  //    if (string.IsNullOrEmpty(srdata.RTVFXTransform) == false) { RTVFXTransform = this.transform.FindRecursive(srdata.RTVFXTransform); }
  //    if (string.IsNullOrEmpty(srdata.LTVFXTransform) == false) { LTVFXTransform = this.transform.FindRecursive(srdata.LTVFXTransform); }
  //    if (string.IsNullOrEmpty(srdata.CTVFXTransform) == false) { CTVFXTransform = this.transform.FindRecursive(srdata.CTVFXTransform); }
  //    if (string.IsNullOrEmpty(srdata.HEADVFXTransform) == false) { HEADVFXTransform = this.transform.FindRecursive(srdata.HEADVFXTransform); }
  //    if (string.IsNullOrEmpty(srdata.DamageAnimator) == false) {
  //      Transform DamageAnimatorTR = this.transform.FindRecursive(srdata.DamageAnimator);
  //      if (DamageAnimatorTR != null) { DamageAnimator = DamageAnimatorTR.gameObject.GetComponent<Animator>(); }
  //    }
  //    Log.TWL(0, "QuadBodyAnimation.Init JumpJetsSpawnPoints:"+ srdata.JumpJetsSpawnPoints.Count);
  //    foreach(string JumpJetAttachPoint in srdata.JumpJetsSpawnPoints) {
  //      if (string.IsNullOrEmpty(JumpJetAttachPoint)) { continue; }
  //      Transform JumpJetAttachPointTR = this.transform.FindRecursive(JumpJetAttachPoint);
  //      Log.W(1, JumpJetAttachPoint);
  //      if (JumpJetAttachPointTR == null) { Log.WL(1, "not found"); continue; }
  //      Log.WL(1, "found:"+ JumpJetAttachPointTR.name);
  //      JumpJetsSpawnPoints.Add(JumpJetAttachPointTR);
  //    }
  //    Log.TWL(0, "QuadBodyAnimation.Init HeadLightSpawnPoints:" + srdata.HeadLightSpawnPoints.Count);
  //    foreach (string HeadLightAttachPoint in srdata.HeadLightSpawnPoints) {
  //      if (string.IsNullOrEmpty(HeadLightAttachPoint)) { continue; }
  //      Transform HeadLightAttachPointTR = this.transform.FindRecursive(HeadLightAttachPoint);
  //      Log.W(1, HeadLightAttachPoint);
  //      if (HeadLightAttachPointTR == null) { Log.WL(1, "not found"); continue; }
  //      Log.WL(1, "found:" + HeadLightAttachPointTR.name);
  //      HeadLightSpawnPoints.Add(HeadLightAttachPointTR);
  //    }
  //    ShowFrontLegsAxis = srdata.ShowFrontLegsAxis;
  //    parentRepresentation = null;
  //    RepresentationInited = false;
  //  }
  //}
}