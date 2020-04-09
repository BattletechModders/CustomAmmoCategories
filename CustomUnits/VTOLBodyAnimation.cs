using BattleTech;
using HBS;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CustomUnits {
  public class VTOLFallStoper : MonoBehaviour {
    private Animator thisAnimator = null;
    private SphereCollider collider = null;
    protected string AudioEventExplode;
    private GenericAnimatedComponent parent;
    public void Init(string explodeSound, GenericAnimatedComponent parent) {
      this.parent = parent;
      AudioEventExplode = explodeSound;
      collider = this.GetComponent<SphereCollider>();
      this.gameObject.layer = LayerMask.NameToLayer("VFXPhysics");
      thisAnimator = this.GetComponentInChildren<Animator>();
    }
    private void OnTriggerEnter(Collider other) {
      if (other.gameObject.layer != LayerMask.NameToLayer("Terrain")) { return; }
      Log.TWL(0, "VTOLFallStoper reach ground");
      thisAnimator.SetBool("fall", false);
      if (string.IsNullOrEmpty(AudioEventExplode) == false) {
        if (SceneSingletonBehavior<WwiseManager>.HasInstance) {
          uint soundid = SceneSingletonBehavior<WwiseManager>.Instance.PostEventByName(AudioEventExplode, this.parent.parent.GameRep.audioObject, (AkCallbackManager.EventCallback)null, (object)null);
          Log.TWL(0, "Explode playing sound by id (" + AudioEventExplode + "):" + soundid);
        } else {
          Log.TWL(0, "Can't play (" + AudioEventExplode + ")");
        }
      }
    }
  }
  public class VTOLBodyAnimationData {
    public string explode_sound { get; set; }
    public VTOLBodyAnimationData() {
      explode_sound = string.Empty;
    }
  };
  public class VTOLBodyAnimation: GenericAnimatedComponent {
    private Animator thisAnimator = null;
    private VTOLFallStoper fallStopper = null;
    public override bool StayOnDeath() { return true; }
    public override bool KeepPosOnDeath() { return true; }
    public override void OnDeath() { thisAnimator.SetBool("fall", true); base.OnDeath(); }
    public override void Init(ICombatant a, int loc, string data, string prefabName) {
      base.Init(a, loc, data, prefabName);
      VTOLBodyAnimationData srdata = JsonConvert.DeserializeObject<VTOLBodyAnimationData>(data);

      thisAnimator = this.GetComponentInChildren<Animator>();
      if (thisAnimator != null) {
        if (a != null) {
          thisAnimator.SetBool("stop", false);
        } else {
          thisAnimator.SetBool("stop", true);
        }
      };
      Log.WL(1,"VTOLBodyAnimation.Init " + this.gameObject.name+" thisAnimator:"+(thisAnimator != null?" stopped:"+ thisAnimator.GetBool("stop") : "null"));
      fallStopper = thisAnimator.gameObject.GetComponent<VTOLFallStoper>();
      if (fallStopper == null) { fallStopper = thisAnimator.gameObject.AddComponent<VTOLFallStoper>(); };
      fallStopper.Init(srdata.explode_sound, this);
    }
  }
}
