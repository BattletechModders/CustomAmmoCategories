using CustomUnits;
using UnityEngine;

namespace CustomUnitsHelper
{

  // Terrain aligner that relies upon a cross-shaped input from CustomUnitsGroundLedar to operate. Designed to keep multi-hex scale vehicles
  //   aligned to the average of the terrain, instead of the central hex they actually occuupy
  public class MultihexVehicleTerrainAligner : MonoBehaviour, IEnableOnMove
  {
    [SerializeField]
    private CustomUnitsGroundLedar m_FrontPos;
    [SerializeField]
    private CustomUnitsGroundLedar m_RearPos;
    [SerializeField]
    private CustomUnitsGroundLedar m_LeftPos;
    [SerializeField]
    private CustomUnitsGroundLedar m_RightPos;

    [SerializeField]
    private MultihexVehicleBodyFollower m_bodyFollower;

    // Internals
    private Transform j_Root;

    public Transform FrontPos 
    { 
      get { return m_FrontPos == null ? null : m_FrontPos.targetTranform; } 
      set { m_FrontPos = value.gameObject.GetComponent<CustomUnitsGroundLedar>(); } 
    }
    public Transform RearPos 
    { 
      get { return m_RearPos == null ? null : m_RearPos.targetTranform; } 
      set { m_RearPos = value.gameObject.GetComponent<CustomUnitsGroundLedar>(); } 
    }
    public Transform LeftPos 
    { 
      get { return m_LeftPos == null ? null : m_LeftPos.targetTranform; } 
      set { m_LeftPos = value.gameObject.GetComponent<CustomUnitsGroundLedar>(); } 
    }
    public Transform RightPos 
    { 
      get { return m_RightPos == null ? null : m_RightPos.targetTranform; } 
      set { m_RightPos = value.gameObject.GetComponent<CustomUnitsGroundLedar>(); } 
    }

    public void Init()
    {
      this.j_Root = this.transform.j_Root();
    }

    public void OnEnable()
    {
      this.Init();
      if (m_bodyFollower != null) { m_bodyFollower.enabled = false; }
    }
    public void Enable()
    {
      this.Init();
      if (m_bodyFollower != null)
      {
        m_bodyFollower.enabled = false;
      }
      this.enabled = true;
    }

    public void Disable()
    {
      if (m_bodyFollower != null) { m_bodyFollower.enabled = true; }; 
      this.enabled = false;
    }

    public void Awake() { this.Init(); }

    public void LateUpdate()
    {
      if (m_FrontPos == null) { return; }
      if (m_RearPos == null) { return; }
      if (m_LeftPos == null) { return; }
      if (m_RightPos == null) { return; }

      if (m_FrontPos.targetTranform == null) { return; }
      if (m_RearPos.targetTranform == null) { return; }
      if (m_LeftPos.targetTranform == null) { return; }
      if (m_RightPos.targetTranform == null) { return; }

      DebugLog.Instance.LogWrite($"currentPos: {this.transform.position}  currentRot: {this.transform.rotation}");

      Vector3 xAxisMidpoint = Vector3.Lerp(m_LeftPos.targetTranform.position, m_RightPos.targetTranform.position, 0.5f);
      DebugLog.Instance.LogWrite($"leftPos: {m_LeftPos.targetTranform.position}  rightPos: {m_RightPos.targetTranform.position}  xAxisMidpoint: {xAxisMidpoint}");

      Vector3 zAxisMidpoint = Vector3.Lerp(m_FrontPos.targetTranform.position, m_RearPos.targetTranform.position, 0.5f);
      DebugLog.Instance.LogWrite($"frontPos: {m_FrontPos.targetTranform.position}  rearPos: {m_RearPos.targetTranform.position}  zAxisMidpoint: {zAxisMidpoint}");

      // dot-product the union of the two axial lines to find the 'up' direction. Should be at least 5-10 units off ground for best outcome
      Vector3 mergedCenterPoint = Vector3.Lerp(xAxisMidpoint, zAxisMidpoint, 0.5f);

      Vector3 newPos = this.transform.position;
      float newHeight = zAxisMidpoint.y - this.j_Root.transform.position.y;
      newPos.y = this.j_Root.position.y + newHeight;
      DebugLog.Instance.LogWrite($"newHeight: {newHeight} = zAxisMidpoint.y: {zAxisMidpoint.y} - j_root.y: {this.j_Root.transform.position.y} => newPos: {newPos}");
      this.transform.position = newPos;

      // Align with the front target, but set the look direction 'up' through the 
      this.transform.rotation.SetLookRotation(m_FrontPos.targetTranform.position, mergedCenterPoint);
    }

  }

  // Follower that will update the j_Body transform after the LateUpdate() call on the MultihexVehicleTerrainAligner finishes. This should sync the rotation and position
  //   of the j_Body transform with the values on the MultihexVehicleTerrainAligner GO
  [ExecuteInEditMode]
  public class MultihexVehicleBodyFollower : MonoBehaviour
  {
    public Vector3 bodyDefaultPos;
    public Vector3 bodyDefaultRot;

    public Vector3 thisDefaultPos;
    public Vector3 thisDefaultRot;
    public Transform j_Body;

    public void Awake() { this.Init(); }

    public void OnEnable() { this.Init(); }

    public void Init()
    {
      Transform j_Root = this.gameObject.FindParent<Transform>("j_Root");
      if (j_Root == null) { return; }
      this.j_Body = j_Root.gameObject.FindClass<Transform>("j_Body");

      bodyDefaultPos = j_Body.localPosition;
      bodyDefaultRot = j_Body.localRotation.eulerAngles;

      thisDefaultPos = this.transform.localPosition;
      thisDefaultRot = this.transform.localRotation.eulerAngles;
    }
    public void LateUpdate()
    {
      this.transform.localPosition = thisDefaultPos + (j_Body.localPosition - bodyDefaultPos);
    }
  }

}
