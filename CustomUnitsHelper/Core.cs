/*  
 *  This file is part of CustomUnitsHelper.
 *  CustomUnitsHelper is free software: you can redistribute it and/or modify it under the terms of the 
 *  GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, 
 *  or (at your option) any later version. CustomAmmoCategories is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 *  See the GNU Lesser General Public License for more details.
 *  You should have received a copy of the GNU Lesser General Public License along with CustomAmmoCategories. 
 *  If not, see <https://www.gnu.org/licenses/>. 
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace CustomUnits {
  public class DebugLog {
    private StreamWriter logfile { get; set; } = null;
    private static DebugLog instance = null;
    public static DebugLog Instance {
      get {
        if (instance == null) { instance = new DebugLog(); }
        return instance;
      }
    }
    public DebugLog() {
      string curAssemblyFolder = new System.Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath;
      logfile = new StreamWriter(Path.Combine(Path.GetDirectoryName(curAssemblyFolder), "CustomUnitsHelper.log"));
    }
    public void LogWrite(int initiation, string line, bool eol = false, bool timestamp = false) {
      string init = new string(' ', initiation);
      string prefix = String.Empty;
      if (timestamp) { prefix = DateTime.Now.ToString("[HH:mm:ss.fff]"); }
      if (initiation > 0) { prefix += init; };
      if (eol) {
        LogWrite(prefix + line + "\n");
      } else {
        LogWrite(prefix + line);
      }
    }
    public void LogWrite(string line) {
      this.logfile.WriteLine(line); this.logfile.Flush();
    }
    public void W(string line) {
      this.LogWrite(line);
    }
    public void WL(string line) {
      line += "\n"; W(line);
    }
    public void W(int initiation, string line) {
      string init = new string(' ', initiation);
      line = init + line; W(line);
    }
    public void WL(int initiation, string line) {
      string init = new string(' ', initiation);
      line = init + line; WL(line);
    }
    public void TW(int initiation, string line) {
      string init = new string(' ', initiation);
      line = "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "]" + init + line;
      W(line);
    }
    public void TWL(int initiation, string line) {
      string init = new string(' ', initiation);
      line = "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "]" + init + line;
      WL(line);
    }
  }
  public static class TransformsHelper {
    public static T FindClass<T>(this GameObject obj, string name) where T : Component {
      foreach(T child in obj.GetComponentsInChildren<T>(true)) {
        if (child.transform.name == name) { return child; }
      }
      return null;
    }
    public static T FindParent<T>(this GameObject obj, string name) where T : Component {
      Transform tr = obj.transform;
      while(tr != null) {
        if (tr.name == name) { return tr.gameObject.GetComponent<T>(); }
        tr = tr.parent;
      }
      return null;
    }
    public static Transform rootTransform(this Transform tr) {
      while (tr.parent != null) { tr = tr.parent; }
      return tr;
    }
    public static Transform j_Root(this Transform tr) {
      while (tr.parent != null) { tr = tr.parent; if (tr.name == "j_Root") { return tr; } }
      return null;
    }
  }
  public interface IEnableOnMove {
    void Init();
    void Enable();
    void Disable();
  }
  public interface IOnRepresentationInit {
    void Init(GameObject representation);
  }
  public interface IOnFootFallReceiver {
    void OnCustomFootFall(Transform foot);
  }
  public class CustomAnimationEventReceiver: MonoBehaviour {
    public void OnAudioEvent(string name) {
      
    }
  }
  public class DelayedDisable : MonoBehaviour, IEnableOnMove {
    [NonSerialized]
    private float t;
    public void Disable() {
      t = 1f;
    }
    public virtual void DisableReal() {
      this.enabled = false;
    }
    public virtual void Update() {
      if (float.IsNaN(t)) { return; }
      if (t >= 0f) { t -= Time.deltaTime; return; }
      t = float.NaN;
      this.DisableReal();
    }
    public virtual void Enable() {
      this.enabled = true;
    }
    public virtual void Init() {
    }
  }
  public class ScorpionBodyTerrainAligner: MonoBehaviour, IEnableOnMove {
    [SerializeField]
    private CustomUnitsGroundLedar m_FrontPos;
    [SerializeField]
    private CustomUnitsGroundLedar m_RearPos;
    [SerializeField]
    private ScorpionPelvisFollower m_PelvisFollower;
    private Transform j_Root;
    private float heightToKeep;
    public Transform FrontPos { get { return m_FrontPos == null ? null:m_FrontPos.targetTranform; } set { m_FrontPos = value.gameObject.GetComponent<CustomUnitsGroundLedar>(); } }
    public Transform RearPos { get { return m_RearPos == null ? null:m_RearPos.targetTranform; } set { m_RearPos = value.gameObject.GetComponent<CustomUnitsGroundLedar>(); } }
    public void Init() {
      this.j_Root = this.transform.j_Root();
      heightToKeep = (this.m_RearPos.transform.position - this.j_Root.position).y;
    }
    public void OnEnable() { this.Init(); if (m_PelvisFollower != null) { m_PelvisFollower.enabled = false; } }
    public void Disable() { if (m_PelvisFollower != null) { m_PelvisFollower.enabled = true; }; this.enabled = false; }
    public void Awake() { this.Init(); }
    public void LateUpdate() {
      if (m_FrontPos == null) { return; }
      if (m_RearPos == null) { return; }
      if (m_FrontPos.targetTranform == null) { return; }
      if (m_RearPos.targetTranform == null) { return; }
      Vector3 frontToRearGround =  m_FrontPos.targetTranform.position - m_RearPos.targetTranform.position;
      if (frontToRearGround.sqrMagnitude < 0.0001f) { return; }
      this.transform.rotation = Quaternion.LookRotation(frontToRearGround, Vector3.up);
      Vector3 pos = this.transform.position;
      pos.y += (heightToKeep - (m_RearPos.transform.position - this.j_Root.position).y);
    }
    public void Enable() { this.Init(); if (m_PelvisFollower != null) { m_PelvisFollower.enabled = false; }; this.enabled = true; }
  }
  [ExecuteInEditMode]
  public class CustomUnitsGroundLedar : MonoBehaviour, IOnRepresentationInit {
    public enum LedarHeightBehavior { KeepToRootBone, TieToGround, KeepHeight, KeepPosOnGround }
    [SerializeField]
    private Transform m_targetTransform;
    [SerializeField]
    private Transform m_keepDistTransform;
    [SerializeField]
    private float m_keepDistance = 1f;
    [SerializeField]
    private LedarHeightBehavior m_ledarBehavior = LedarHeightBehavior.KeepToRootBone;
    [SerializeField]
    private float m_TargetMinSpeed = 10f;
    [SerializeField]
    private float m_OverGroundHeight = 0f;
    [SerializeField]
    private Vector3 m_upLegVector = Vector3.zero;
    [SerializeField]
    private Transform m_animateTranform = null;
    [SerializeField]
    private float m_animateState = 0f;
    [NonSerialized]
    public bool EnableRaycast = true;
    [NonSerialized]
    public Vector3 targetPosition;
    [NonSerialized]
    public Vector3 onGroundPos;
    [NonSerialized]
    public Vector3 onGroundNorm;
    public float currentDistToTarget = 0f;
    public Vector3 currentPosToTarget;
    public Vector3 maxPosToTarget;

    public float currentFixedDistToTarget = 0f;
    public Vector3 currentFixedPosToTarget;
    public Vector3 maxFixedPosToTarget;

    public IOnFootFallReceiver FootFallReceiver { get; set; } = null;
    public bool fixTargetPos { get; private set; } = false;
    public float fixTargetPosDistTreshold { get; set; } = 10f;
    public float fixTargetPosTimeTreshold { get; set; } = 3f;
    public Vector3 fixedTargetPos { get; private set; }
    public Vector3 fixedTargetNorm { get; private set; }

    public Vector3 currentTargetPos { get; private set; }
    public Vector3 prevTargetPos { get; private set; }
    public Vector3 realTargetPos { get; private set; }
    public Transform targetTranform { get { return m_targetTransform; } set { m_targetTransform = value; } }
    public Transform rootTransform { get; private set; }
    public Transform j_Root { get; private set; }
    public int ikLayersMask { get; set; } = Physics.DefaultRaycastLayers;
    public void OnEnable() { this.Init(); }
    [SerializeField]
    private float inFixedPositionDuration = 0f;
    public void Init() {
      if (Application.isEditor == false) {
        this.ikLayersMask = LayerMask.GetMask("Terrain", "Obstruction");
      } else {
        this.ikLayersMask = Physics.DefaultRaycastLayers;
      };
      this.FootFallReceiver = this.GetComponentInParent<IOnFootFallReceiver>();
      this.rootTransform = this.transform.rootTransform();
      this.j_Root = this.transform.j_Root();
      this.UpdateGroundPos(true);
      this.UpdateTargetPos();
      fixedTargetPos = this.targetPosition;
      prevTargetPos = this.targetPosition;
      currentTargetPos = this.targetPosition;
      this.m_targetTransform.position = this.targetPosition;
      fixTargetPos = false;
      inFixedPositionDuration = 0f;
    }
    public void Awake() { this.Init(); }
    public void UpdateGroundPos(bool forced = false) {
      if ((this.EnableRaycast == false)&&(forced == false)) { return; }
      Transform thisTransform = this.m_animateTranform != null ? this.m_animateTranform : this.transform;
      Vector3 rayPos = thisTransform.position + Vector3.up * 20f;
      RaycastHit[] raycastHitArray = Physics.RaycastAll(new Ray(thisTransform.position + Vector3.up * 20f, Vector3.down), 40f, this.ikLayersMask);
      RaycastHit? raycastHit = new RaycastHit?();
      for (int index = 0; index < raycastHitArray.Length; ++index) {
        if (this.selfRepColliders.Contains(raycastHitArray[index].collider)) { continue; }
        if (!raycastHit.HasValue) {
          raycastHit = new RaycastHit?(raycastHitArray[index]);
        } else if (raycastHit.Value.point.y < raycastHitArray[index].point.y) {
          raycastHit = new RaycastHit?(raycastHitArray[index]);
        }
      }
      if (raycastHit.HasValue) {
        this.onGroundPos = raycastHit.Value.point;
        onGroundNorm = raycastHit.Value.normal;
      }
      this.onGroundPos.x = thisTransform.position.x; this.onGroundPos.z = thisTransform.position.z;
    }
    public void UpdateTargetPos() {
      Transform thisTransform = this.m_animateTranform != null ? this.m_animateTranform : this.transform;
      switch (m_ledarBehavior) {
        case LedarHeightBehavior.KeepToRootBone: {
          targetPosition = onGroundPos;
          targetPosition.y += (thisTransform.position - this.j_Root.position).y;
          targetPosition = thisTransform.InverseTransformPoint(targetPosition);
          targetPosition += Vector3.up * this.m_OverGroundHeight;
          targetPosition = thisTransform.TransformPoint(targetPosition);
        };  break;
        case LedarHeightBehavior.KeepPosOnGround: {
          targetPosition = onGroundPos;
          targetPosition.y += (thisTransform.position - this.j_Root.position).y;
          targetPosition = thisTransform.InverseTransformPoint(targetPosition);
          targetPosition += Vector3.up * this.m_OverGroundHeight;
          targetPosition = thisTransform.TransformPoint(targetPosition);
        }; break;
        case LedarHeightBehavior.TieToGround: {
          targetPosition = onGroundPos;
        }; break;
        case LedarHeightBehavior.KeepHeight: {
          targetPosition = onGroundPos;
          targetPosition.y += (thisTransform.position - this.rootTransform.position).y;
          targetPosition = thisTransform.InverseTransformPoint(targetPosition);
          targetPosition += Vector3.up * this.m_OverGroundHeight;
          targetPosition = thisTransform.TransformPoint(targetPosition);
        }; break;
      }
    }
    public void LateUpdate() {
      //base.Update();
      if (m_targetTransform == null) { return; }
      if (j_Root == null) { return; }
      if (rootTransform == null) { this.rootTransform = this.transform.rootTransform(); }
      if (this.transform.lossyScale.sqrMagnitude < 0.001f) { return; }
      if (m_animateTranform != null) { this.m_animateTranform.localPosition = Vector3.Lerp(Vector3.zero, this.m_upLegVector, this.m_animateState); }
      this.UpdateGroundPos();
      this.UpdateTargetPos();
      if (m_ledarBehavior != LedarHeightBehavior.KeepPosOnGround) {
        this.m_targetTransform.position = this.targetPosition;
        this.m_targetTransform.rotation = Quaternion.FromToRotation(this.transform.up, this.onGroundNorm);
      } else {
        if (m_keepDistTransform != null && m_keepDistance >= 1f) {
          currentPosToTarget = m_keepDistTransform.InverseTransformPoint(this.targetPosition);
          currentDistToTarget = currentPosToTarget.magnitude;
          if (currentDistToTarget > m_keepDistance) {
            maxPosToTarget = currentPosToTarget.normalized * m_keepDistance;
            targetPosition = m_keepDistTransform.TransformPoint(maxPosToTarget);
            currentPosToTarget = m_keepDistTransform.InverseTransformPoint(this.targetPosition);
            currentDistToTarget = currentPosToTarget.magnitude;
          }
        }
        Transform thisTransform = this.m_animateTranform != null ? this.m_animateTranform : this.transform;
        if ((fixTargetPos == false) && (Mathf.Abs(this.j_Root.InverseTransformPoint(thisTransform.position).y) < 0.01f)) {
          fixTargetPos = true;
          this.fixedTargetPos = this.targetPosition;
          inFixedPositionDuration = 0f;
          this.FootFallReceiver?.OnCustomFootFall(this.m_targetTransform);
        } else if((fixTargetPos == true)&& (Mathf.Abs(this.j_Root.InverseTransformPoint(thisTransform.position).y) > 0.01f)) {
          fixTargetPos = false;
          inFixedPositionDuration = 0f;
        }
        if (fixTargetPos == false) { this.fixedTargetPos = this.targetPosition; inFixedPositionDuration = 0f; } else {
          inFixedPositionDuration += Time.deltaTime;
          if(inFixedPositionDuration > this.fixTargetPosTimeTreshold) { fixedTargetPos = this.targetPosition; inFixedPositionDuration = 0f; } else {
            if (m_keepDistTransform != null && m_keepDistance >= 1f) {
              currentFixedPosToTarget = m_keepDistTransform.InverseTransformPoint(this.fixedTargetPos);
              currentFixedDistToTarget = currentFixedPosToTarget.magnitude;
              if (currentFixedDistToTarget > m_keepDistance) {
                maxFixedPosToTarget = currentFixedPosToTarget.normalized * m_keepDistance;
                fixedTargetPos = m_keepDistTransform.TransformPoint(maxFixedPosToTarget);
              }
            }
          }
        }
        float needDist = Vector3.Distance(this.currentTargetPos, this.fixedTargetPos);
        float t = this.m_TargetMinSpeed * Time.deltaTime;
        if (needDist < t) {
          this.currentTargetPos = this.fixedTargetPos;
        } else { 
          float targetSpeed = Vector3.Distance(this.prevTargetPos, this.targetPosition);
          t = Mathf.Clamp01(Mathf.Max(targetSpeed * 2f, t) / needDist);
          this.currentTargetPos = Vector3.Lerp(this.currentTargetPos, this.fixedTargetPos, t);
        }
        this.m_targetTransform.position = this.currentTargetPos;
      }
      this.prevTargetPos = this.targetPosition;
    }
    public HashSet<Collider> selfRepColliders = new HashSet<Collider>();
    public void Init(GameObject representation) {
      foreach(Collider col in representation.GetComponentsInChildren<Collider>(true)) {
        selfRepColliders.Add(col);
      }
      this.Init();
    }
  }
  [ExecuteInEditMode]
  public class ScorpionPelvisFollower : MonoBehaviour {
    public Vector3 pelvisDefaultPos;
    public Vector3 pelvisDefaultRot;
    public Vector3 thisDefaultPos;
    public Vector3 thisDefaultRot;
    public Transform j_Pelvis;
    public void Awake() { this.Init(); }
    public void OnEnable() { this.Init(); }
    public void Init() {
      Transform j_Root = this.gameObject.FindParent<Transform>("j_Root");
      if (j_Root == null) { return; }
      this.j_Pelvis = j_Root.gameObject.FindClass<Transform>("j_Pelvis");
      pelvisDefaultPos = j_Pelvis.localPosition;
      pelvisDefaultRot = j_Pelvis.localRotation.eulerAngles;
      thisDefaultPos = this.transform.localPosition;
      thisDefaultRot = this.transform.localRotation.eulerAngles;
    }
    public void LateUpdate() {
      this.transform.localPosition = thisDefaultPos + (j_Pelvis.localPosition - pelvisDefaultPos);
    }
  }
  [ExecuteInEditMode]
  public class ScorpionLegSolver : MonoBehaviour {
    public Transform Thigh;
    public Transform Calf;
    public Transform Foot;
    public Transform Target;
    private Vector3 thighRot = Vector3.zero;
    private Vector3 calfRot = Vector3.zero;
    private Vector3 footRot = Vector3.zero;
    private float a;
    private float b;
    private float c;
    public float beta;
    public float gamma;
    public float alpha;
    public float alpha_max;
    public float alpha_min;
    public float toH;
    public void Awake() { }
    public void LateUpdate() {
      if (Target == null) { return; }
      if (Calf == null) { return; }
      if (Foot == null) { return; }
      if (Thigh == null) { return; }
      //Vector3 toLegTarget = this.transform.InverseTransformPoint(Target.position);
      Vector3 toLegTarget = Target.transform.position - this.transform.position;
      if (toLegTarget.sqrMagnitude < 0.001f) { return; }
      thighRot = Quaternion.LookRotation(toLegTarget, Vector3.up).eulerAngles;
      toH = thighRot.x;
      //thighRot.y += 360f;
      //thighRot.y = Mathf.Clamp(thighRot.y, alpha_min, alpha_max);
      //alpha = thighRot.y;
      thighRot.z = 0f;
      thighRot.x = 0f;
      //Thigh.localRotation = Quaternion.Euler(thighRot);
      Thigh.transform.rotation = Quaternion.Euler(thighRot);
      thighRot = Thigh.transform.localRotation.eulerAngles;
      thighRot.y = Mathf.Clamp(thighRot.y, alpha_min, alpha_max);
      alpha = thighRot.y;
      Thigh.localRotation = Quaternion.Euler(thighRot);
      a = Vector3.Distance(Thigh.position, Calf.position);
      b = Vector3.Distance(Calf.position, Foot.position);
      c = Vector3.Distance(Thigh.position, Target.position);
      if (c >= (a + b)) {
        thighRot.x = toH;
        Thigh.rotation = Quaternion.Euler(thighRot);
        Calf.localRotation = Quaternion.Euler(Vector3.zero);
        return;
      }
      beta = Mathf.Rad2Deg * Mathf.Acos((a * a + c * c - b * b) / (2 * a * c));
      gamma = Mathf.Rad2Deg * Mathf.Acos((a * a + b * b - c * c) / (2 * a * b));
      thighRot = Thigh.rotation.eulerAngles;
      thighRot.x = 0f - (beta - toH);
      Thigh.rotation = Quaternion.Euler(thighRot);
      calfRot.x = 180f - gamma;
      //calfRot.y = 0f;
      //calfRot.z = 0f;
      Calf.localRotation = Quaternion.Euler(calfRot);
      // Quaternion.FromToRotation()
      Foot.transform.LookAt(Foot.transform.position + Vector3.down * 1f, Vector3.up);
      //Foot.transform.localRotation = Quaternion.identity;
      //Vector3 footRot = Foot.transform.rotation.eulerAngles;
      //footRot.x = 0f;
      //Foot.transform.rotation = Quaternion.Euler(footRot);
      footRot = Foot.transform.localRotation.eulerAngles;
      footRot.y = 0f;
      footRot.z = 0f;
      Foot.transform.localRotation = Quaternion.Euler(footRot);
    }
  }
}
