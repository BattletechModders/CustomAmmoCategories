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
using BattleTech.Rendering;
using BattleTech.UI;
using DG.Tweening;
using UnityEngine;

namespace CustAmmoCategories {
  public class CombatCustomReticle : MonoBehaviour {
    public static string PrefabName = "AuraReticle";
    public static string CustomPrefabName = "CustomAuraReticle";
    public string VisRange = "_Range1";
    public string SensorRange = "_Range2";
    public Vector3 groundOffset = new Vector3(0.0f, 2f, 0.0f);
    public Material auraRangeMatDim;
    public Material auraRangeMatBright;
    public Material auraRangeMatDimEnemy;
    public Material auraRangeMatBrightEnemy;
    public BTUIDecal auraRangeDecal;
    public Material activeProbeMatDim;
    public Material activeProbeMatBright;
    public BTUIDecal activeProbeDecal;
    public DOTweenAnimation apSpinAnim;
    public HBSDOTweenButton auraProjectionTweens;
    public HBSDOTweenButton auraReceptionTweens;
    public GameObject auraRangeHolder;
    public GameObject activeProbeRangeHolder;
    public GameObject auraIndicatorAlly;
    public GameObject auraIndicatorEnemy;
    public GameObject mortarTargetIndicator;
    private int visRangeInt;
    private int sensorRangeInt;
    private bool currentAuraIsBright;
    private float currentAuraRange;
    public float AuraRange { get; set; }
    private CombatHUD HUD;
    private Transform thisTransform;
    private Transform parentTransform;
    private Vector3 offset;

    private GameObject auraRangeScaledObject { get; set; }
    private GameObject activeProbeRangeScaledObject { get; set; }
    private ButtonState DesiredAuraProjectionState {
      get {
        return ButtonState.Enabled;
      }
    }
    private ButtonState DesiredAuraReceptionState {
      get {
        return ButtonState.Enabled;
      }
    }
    public void Copy(CombatAuraReticle src) {
      this.VisRange = src.VisRange;
      this.SensorRange = src.SensorRange;
      this.groundOffset = src.groundOffset;
      this.auraRangeMatDim = src.auraRangeMatDim;
      this.auraRangeMatBright = src.auraRangeMatBright;
      this.auraRangeMatDimEnemy = src.auraRangeMatDimEnemy;
      this.auraRangeMatBrightEnemy = src.auraRangeMatBrightEnemy;
      this.auraRangeDecal = src.auraRangeDecal;
      this.activeProbeMatDim = src.activeProbeMatDim;
      this.activeProbeMatBright = src.activeProbeMatBright;
      this.activeProbeDecal = src.activeProbeDecal;
      this.apSpinAnim = src.apSpinAnim;
      this.auraProjectionTweens = src.auraProjectionTweens;
      this.auraReceptionTweens = src.auraReceptionTweens;
      this.auraRangeHolder = src.auraRangeHolder;
      this.activeProbeRangeHolder = src.activeProbeRangeHolder;
      this.auraIndicatorAlly = src.auraIndicatorAlly;
      this.auraIndicatorEnemy = src.auraIndicatorEnemy;
      this.mortarTargetIndicator = src.mortarTargetIndicator;
      this.currentAuraRange = 0f;
    }
    public void Init(CombatHUD HUD, Vector3 offset, Transform parentTransform, float Range) {
      this.HUD = HUD;
      this.thisTransform = this.transform;
      this.auraRangeDecal = this.auraRangeHolder.GetComponentInChildren<BTUIDecal>(true);
      this.auraRangeScaledObject = this.auraRangeDecal.gameObject;
      this.activeProbeRangeScaledObject = this.activeProbeRangeHolder.GetComponentInChildren<BTUIDecal>(true).gameObject;
      this.mortarTargetIndicator.SetActive(false);
      this.auraIndicatorAlly.SetActive(true);
      this.auraIndicatorEnemy.SetActive(false);
      this.visRangeInt = Shader.PropertyToID(this.VisRange);
      this.sensorRangeInt = Shader.PropertyToID(this.SensorRange);
      this.currentAuraRange = -1f;
      //this.currentAPRange = -1f;
      this.currentAuraIsBright = false;
      //this.currentAPIsBright = false;
      this.offset = offset;
      this.parentTransform = parentTransform;
      AuraRange = Range;
      this.UpdatePosition();
    }
    public void RefreshAuraColors(bool isBright) {
      if (this.currentAuraIsBright == isBright) { return; }
      this.currentAuraIsBright = isBright;
      this.auraRangeDecal.DecalMaterial = this.auraRangeMatBright;
    }
    private void UpdatePosition() {
      if (this.parentTransform != null) { this.thisTransform.position = parentTransform.position + this.groundOffset + this.offset; } else {
        this.thisTransform.position = this.offset + this.groundOffset;
      }
    }
    private void LateUpdate() {
      this.RefreshAuraIndicators();
    }
    public void RefreshAuraIndicators() {
      this.UpdatePosition();
      ButtonState auraProjectionState = this.DesiredAuraProjectionState;
      this.auraProjectionTweens.SetState(auraProjectionState, true);
      this.RefreshAuraRange(auraProjectionState);
      this.RefreshAuraColors(true);
      this.auraReceptionTweens.SetState(this.DesiredAuraReceptionState, true);
    }
    private void RefreshAuraRange(ButtonState auraProjectionState) {
      if (auraProjectionState == ButtonState.Disabled) {
        this.auraRangeScaledObject.SetActive(false);
      } else {
        this.auraRangeScaledObject.SetActive(true);
        float b = this.AuraRange;
        if (!Mathf.Approximately(this.currentAuraRange, b))
          this.auraRangeScaledObject.transform.localScale = new Vector3(b * 2f, 1f, b * 2f);
        this.currentAuraRange = b;
      }
    }
  }
}