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
using BattleTech.Rendering.UrbanWarfare;
using CustAmmoCategories;
using HarmonyLib;
using HBS;
using HBS.Math;
using Localize;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

namespace CustomUnits {
  [HarmonyPatch(typeof(ActorMovementSequence))]
  [HarmonyPatch("MoveTowardWaypoint")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Vector3) })]
  public static class ActorMovementSequence_MoveTowardWaypoint {
    public static void Postfix(ActorMovementSequence __instance, MoveType ___moveType) {
      VTOLBodyAnimation bodyAnimation = __instance.owningActor.VTOLAnimation();
      if (bodyAnimation == null) { return; }
      if (bodyAnimation.bodyAnimator == null) { return; }
      if (___moveType == MoveType.Backward) {
        bodyAnimation.bodyAnimator.SetFloat("forward", 0f);
        bodyAnimation.bodyAnimator.SetFloat("backward", 1f);
      } else {
        bodyAnimation.bodyAnimator.SetFloat("forward", 1f);
        bodyAnimation.bodyAnimator.SetFloat("backward", 0f);
      }
    }
  }
  [HarmonyPatch(typeof(ActorTwistSequence), "update")]
  public static class ActorTwistSequence_update {
    public static object TwistState_RangedTwisting = Enum.Parse(typeof(ActorTwistSequence).GetField("state", BindingFlags.Instance | BindingFlags.NonPublic).FieldType, "RangedTwisting");
    public static object TwistState_Finished = Enum.Parse(typeof(ActorTwistSequence).GetField("state", BindingFlags.Instance | BindingFlags.NonPublic).FieldType, "Finished");
    public static object TwistState_MeleeFacing = Enum.Parse(typeof(ActorTwistSequence).GetField("state", BindingFlags.Instance | BindingFlags.NonPublic).FieldType, "MeleeFacing");
    private static FieldInfo f_state = typeof(ActorTwistSequence).GetField("state", BindingFlags.Instance | BindingFlags.NonPublic);
    public static object state(this ActorTwistSequence seq) { return f_state.GetValue(seq); }
    static void Prefix(ActorTwistSequence __instance, ref object __state) {
      __state = __instance.state();
    }
    static void Postfix(ActorTwistSequence __instance, float ___t, ref object __state, PilotableActorRepresentation ___actorRep) {
      if (__state.ToString() != TwistState_RangedTwisting.ToString()) { return; }
      VTOLBodyAnimation vtolAnim = __instance.owningActor.VTOLAnimation();
      if (vtolAnim != null) {
        vtolAnim.twist(___actorRep.currentTwistAngle);
      }
      CustomTwistAnimation customTwist = ___actorRep.gameObject.GetComponent<CustomTwistAnimation>();
      if(customTwist != null) {
        customTwist.twist(___actorRep.currentTwistAngle);
      }
      //Log.TWL(0, "ActorTwistSequence.update " + ___actorRep.currentTwistAngle);
      
      //Log.TWL(0, "ActorTwistSequence.update "+ ___actorRep.currentTwistAngle);
    }
  }
  public class VTOLFallStoper : MonoBehaviour {
    private Animator bodyAnimator = null;
    private Animator engineAnimator = null;
    private SphereCollider collider = null;
    protected string AudioEventExplode;
    private GenericAnimatedComponent parent;
    public void Init(string explodeSound, VTOLBodyAnimation parent) {
      this.parent = parent;
      AudioEventExplode = explodeSound;
      collider = this.GetComponent<SphereCollider>();
      this.gameObject.layer = LayerMask.NameToLayer("VFXPhysics");
      bodyAnimator = parent.bodyAnimator;
      engineAnimator = parent.engineAnimator;
    }
    private void spawnExplosion() {
      string str2 = string.Empty;
      string str1 = "";
      MapTerrainDataCell cellAt = this.parent.parent.Combat.MapMetaData.GetCellAt(this.transform.position);
      if (cellAt != null) {
        str2 = cellAt.GetVFXNameModifier();
        switch (cellAt.GetAudioSurfaceType()) {
          case AudioSwitch_surface_type.dirt:
            str1 = "_dirt";
            break;
          case AudioSwitch_surface_type.metal:
            str1 = "_metal";
            break;
          case AudioSwitch_surface_type.snow:
            str1 = "_snow";
            break;
          case AudioSwitch_surface_type.wood:
            str1 = "_wood";
            break;
          case AudioSwitch_surface_type.brush:
            str1 = "_brush";
            break;
          case AudioSwitch_surface_type.concrete:
            str1 = "_concrete";
            break;
          case AudioSwitch_surface_type.debris_glass:
            str1 = "_debris_glass";
            break;
          case AudioSwitch_surface_type.gravel:
            str1 = "_gravel";
            break;
          case AudioSwitch_surface_type.ice:
            str1 = "_ice";
            break;
          case AudioSwitch_surface_type.lava:
            str1 = "_lava";
            break;
          case AudioSwitch_surface_type.mud:
            str1 = "_mud";
            break;
          case AudioSwitch_surface_type.sand:
            str1 = "_sand";
            break;
          case AudioSwitch_surface_type.water_deep:
          case AudioSwitch_surface_type.water_shallow:
            str1 = "_water";
            break;
          default:
            str1 = "_dirt";
            break;
        }
      }
      this.SpawnDestroyExplosion(string.Format("{0}{1}{2}", "vfxPrfPrtl_weaponLRMExplosion", str1, str2));
      float num3 = UnityEngine.Random.Range(20f, 25f);
      FootstepManager.Instance.AddScorch(this.transform.position, new Vector3(Random.Range(0.0f, 1f), 0.0f, Random.Range(0.0f, 1f)).normalized, new Vector3(num3, num3, num3), true);
    }

    protected virtual void DestroyFlimsyObjects() {
      foreach (Collider collider in Physics.OverlapSphere(this.transform.position, 15f, -5, QueryTriggerInteraction.Ignore)) {
        Vector3 normalized = (collider.transform.position - this.transform.position).normalized;
        float num = 100f + this.parent.parent.Combat.Constants.ResolutionConstants.FlimsyDestructionForceMultiplier;
        DestructibleObject component1 = collider.gameObject.GetComponent<DestructibleObject>();
        DestructibleUrbanFlimsy component2 = collider.gameObject.GetComponent<DestructibleUrbanFlimsy>();
        if ((UnityEngine.Object)component1 != (UnityEngine.Object)null && component1.isFlimsy) {
          component1.TakeDamage(this.transform.position, normalized, num);
          component1.Collapse(normalized, num);
        }
        if ((UnityEngine.Object)component2 != (UnityEngine.Object)null)
          component2.PlayDestruction(normalized, num);
      }
    }
    private void SpawnDestroyExplosion(string explosionName) {
      Log.TWL(0, "VTOLFallStoper.SpawnDestroyExplosion " + explosionName);
      GameObject gameObject = parent.parent.Combat.DataManager.PooledInstantiate(explosionName, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
      if (gameObject == null) {
        Log.TWL(0,"Exploded vehicle have wrong explosion VFX: " + explosionName);
      } else {
        gameObject.ScaleEffect(new CustAmmoCategories.CustomVector(5f,5f,5f));
        ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
        BTLight componentInChildren1 = gameObject.GetComponentInChildren<BTLight>(true);
        BTWindZone componentInChildren2 = gameObject.GetComponentInChildren<BTWindZone>(true);
        component.Stop(true);
        component.Clear(true);
        component.transform.position = this.transform.position;
        component.transform.rotation = this.transform.rotation;
        BTCustomRenderer.SetVFXMultiplier(component);
        component.Play(true);
        if (componentInChildren1 != null) {
          componentInChildren1.contributeVolumetrics = true;
          componentInChildren1.volumetricsMultiplier = 1000f;
          componentInChildren1.intensity = 10f;
          componentInChildren1.FadeIntensity(0.0f, 0.5f);
          componentInChildren1.RefreshLightSettings(true);
        }
        if (componentInChildren2 != null) {
          componentInChildren2.PlayAnimCurve();
        }
        AutoPoolObject autoPoolObject = gameObject.GetComponent<AutoPoolObject>();
        if (autoPoolObject == null) {
          autoPoolObject = gameObject.AddComponent<AutoPoolObject>();
        }
        autoPoolObject.Init(this.parent.parent.Combat.DataManager, explosionName, component);
        gameObject.transform.rotation = UnityEngine.Random.rotationUniform;
        this.DestroyFlimsyObjects();
      }
    }
    private void OnTriggerEnter(Collider other) {
      if (other.gameObject.layer != LayerMask.NameToLayer("Terrain")) { return; }
      Log.TWL(0, "VTOLFallStoper reach ground");
      bodyAnimator.SetBool("fall", false);
      this.spawnExplosion();
      engineAnimator.SetFloat("rotate", 0.0f);
      this.parent.Clear();
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
  public class AttachInfoAnimator {
    public bool HasRecoil { get; protected set; }
    public bool HasVertical { get; protected set; }
    public bool HasVerticalUp { get; protected set; }
    public bool HasIndirect { get; protected set; }
    public bool HasNormal { get; protected set; }
    public bool HasEmiterIndex { get; protected set; }
    private int RecoilHash;
    private int VerticalHash;
    private int VerticalUpHash;
    private int IndirectHash;
    private int NormalHash;
    private int EmiterIndexHash;
    public float animationRate;
    public Animator animator { get; protected set; }
    public float Recoil {
      set {
        if (this.animator == null) { return; }
        if (this.HasRecoil == false) { return; }
        animator.SetFloat(RecoilHash, value);
      }
    }
    public float ToFireNormal {
      set {
        if (this.animator == null) { return; }
        if (this.HasNormal == false) { return; }
        animator.SetFloat(NormalHash, value);
        Log.TWL(0, animator.gameObject.name + " NormalHash" + value);
      }
    }
    public float Indirect {
      set {
        if (this.animator == null) { return; }
        if (this.HasIndirect == false) { return; }
        animator.SetFloat(IndirectHash, value);
      }
    }
    public float Vertical {
      set {
        if (this.animator == null) { return; }
        if (this.HasVertical == false) { return; }
        animator.SetFloat(VerticalHash, value);
        Log.TWL(0,animator.gameObject.name+ " VerticalHash"+value);
      }
    }
    public float VerticalUp {
      set {
        if (this.animator == null) { return; }
        if (this.HasVerticalUp == false) { return; }
        animator.SetFloat(VerticalUpHash, value);
      }
    }
    public int EmiterIndex {
      set {
        if (this.animator == null) { return; }
        if (this.HasEmiterIndex == false) { return; }
        animator.SetInteger(EmiterIndexHash, value);
      }
    }
    public AttachInfoAnimator(Animator anim, float rate) {
      this.animator = anim;
      this.animationRate = rate;
      HasRecoil = false;
      HasVertical = false;
      HasIndirect = false;
      HasNormal = false;
      HasEmiterIndex = false;
      HasVerticalUp = false;
      RecoilHash = Animator.StringToHash("recoil");
      VerticalHash = Animator.StringToHash("vertical"); ;
      VerticalUpHash = Animator.StringToHash("vertical_up"); ;
      IndirectHash = Animator.StringToHash("indirect"); ;
      NormalHash = Animator.StringToHash("to_fire_normal"); ;
      EmiterIndexHash = Animator.StringToHash("emiter_index"); ;
      if (this.animator != null) {
        AnimatorControllerParameter[] parameters = this.animator.parameters;
        for (int index = 0; index < parameters.Length; ++index) {
          if (parameters[index].nameHash == this.RecoilHash)
            this.HasRecoil = true;
          if (parameters[index].nameHash == this.VerticalHash)
            this.HasVertical = true;
          if (parameters[index].nameHash == this.VerticalUpHash)
            this.HasVerticalUp = true;
          if (parameters[index].nameHash == this.IndirectHash)
            this.HasIndirect = true;
          if (parameters[index].nameHash == this.NormalHash)
            this.HasNormal = true;
          if (parameters[index].nameHash == this.EmiterIndexHash)
            this.HasEmiterIndex = true;
        }
      }

    }
  };
  public class AttachInfo {
    private static HashSet<Animator> AnimatorsInPosition = new HashSet<Animator>();
    //public VTOLBodyAnimation parent { get; private set; }
    public string location { get; private set; }
    public string Name { get; set; }
    public HardpointAttachType type { get; private set; }
    public Transform main { get; private set; }
    public Transform attach { get; private set; }
    public HashSet<AttachInfoAnimator> animators { get; private set; }
    public bool hideIfEmpty { get; private set; }
    public bool noRecoil { get; private set; }
    private float recoilValue;
    private float recoilDelta;
    public float RecoilValue { set { foreach(AttachInfoAnimator anim in animators) { anim.Recoil = value; } } }
    public void Update(float t) {
      if(recoilDelta == 0f) { return; };
      recoilValue += recoilDelta * t;
      if (recoilValue < 0f) {
        recoilValue = 0f; recoilDelta = 0f;
        Log.TWL(0, "AttachInfo.Recoiled "+main.name+" up");
      };
      if (recoilValue > 1f) {
        recoilValue = 1f; recoilDelta = -1f;
        Log.TWL(0, "AttachInfo.Recoiled " + main.name + " down");
      };
      this.RecoilValue = recoilValue;
      //animator.SetFloat("recoil",recoilValue);
    }
    public void Recoil() {
      try {
        Log.TWL(0, "AttachInfo.Recoil:" + main.name + " no recoil:" + noRecoil);
        if (noRecoil == false) { recoilDelta = 10f; }
      }catch(Exception e) {
        Log.TWL(0, "AttachInfo.Recoil "+this.Name+" exception:"+e.ToString(), true);
      }
    }
    public HashSet<MechComponent> weapons { get; private set; }
    public HashSet<ComponentRepresentation> bayComponents { get; private set; }
    public void Prefire(Weapon weapon,Vector3 target,bool indirect) {
      Log.WL(1, "AttachInfo.Prefire");
      foreach (AttachInfoAnimator anim in this.animators) {
        if (anim.animator == null) { continue; }
        Log.WL(2, "animator:"+anim.animator.name);
        bool inPos = AnimatorsInPosition.Contains(anim.animator);
        if (inPos) { continue; }
        if (indirect) {
          anim.ToFireNormal = 0.98f;
          anim.Indirect = 1f;
        } else {
          anim.ToFireNormal = 1f;
          anim.Indirect = 0.98f;
          if (this.attach != null) {
            //Vector3 desiredLookDirection = target - attach.position;
            Vector3 firePosition = weapon.parent.CurrentPosition + Vector3.up * weapon.parent.FlyingHeight();
            float distance = Vector3.Distance(firePosition, target);
            float heightDiff = target.y - firePosition.y;
            float angle = (heightDiff > 0f?1f:-1f)* Mathf.Acos(Mathf.Abs(heightDiff)/distance) * Mathf.Rad2Deg;
            //float angle = NvMath.AngleSigned(attach.forward, desiredLookDirection.normalized, Vector3.right);
            Log.WL(3, "angle:" + angle);
            angle /= 90f;
            Log.WL(3, "vertical anim:" + angle);
            //if (angle < 0f) { angle = 0f; }
            if (angle < 0f) {
              anim.VerticalUp = angle;
              anim.VerticalUp = 0f;
            } else {
              anim.Vertical = 0f;
              anim.VerticalUp = Mathf.Abs(angle);
            }
          } else {
            anim.Vertical = 0.5f;
            anim.VerticalUp = 0f;
          }
        }
        AnimatorsInPosition.Add(anim.animator);
      }
    }
    public void Postfire() {
      Log.WL(1, "AttachInfo.Postfire animator");
      foreach (AttachInfoAnimator anim in this.animators) {
        if (anim.animator == null) { continue; }
        Log.WL(2, "animator:" + anim.animator.name);
        anim.ToFireNormal = 0.98f;
        anim.Indirect = 0.98f;
        AnimatorsInPosition.Remove(anim.animator);
      }
    }
    public void AddHardpointAnimators(WeaponRepresentation weaponRep) {
      CustomHardpointRepresentation customHardpointRep = weaponRep.gameObject.GetComponent<CustomHardpointRepresentation>();
      if (customHardpointRep == null) { return; }
      if (customHardpointRep.def == null) { return; }
      foreach (var anim in customHardpointRep.def.animators) {
        Transform animTR = weaponRep.transform.FindRecursive(anim.Key);
        if (animTR == null) { continue; }
        Animator animAnimator = animTR.GetComponent<Animator>();
        if (animAnimator == null) { continue; }
        this.animators.Add(new AttachInfoAnimator(animAnimator, anim.Value));
      }
    }
    public void AddHardpointAnimators(ComponentRepresentation compRep) {
      CustomHardpointRepresentation customHardpointRep = compRep.gameObject.GetComponent<CustomHardpointRepresentation>();
      if (customHardpointRep == null) { return; }
      if (customHardpointRep.def == null) { return; }
      foreach (var anim in customHardpointRep.def.animators) {
        Transform animTR = compRep.transform.FindRecursive(anim.Key);
        if (animTR == null) { continue; }
        Animator animAnimator = animTR.GetComponent<Animator>();
        if (animAnimator == null) { continue; }
        this.animators.Add(new AttachInfoAnimator(animAnimator, anim.Value));
      }
    }
    public AttachInfo(VTOLBodyAnimation parent, string location, HardpointAttachType type, AttachInfoRecord rec) {
      //this.parent = parent;
      this.animators = new HashSet<AttachInfoAnimator>();
      this.location = location;
      this.type = type;
      main = parent.transform.FindRecursive(rec.visuals);
      Animator animator = null;
      if (main != null) {
        if (string.IsNullOrEmpty(rec.animator) == false) {
          Transform animTr = parent.transform.FindRecursive(rec.animator);
          if (animTr != null) {
            animator = animTr.GetComponent<Animator>();
          }
        }
        if (animator == null) {
          animator = main.GetComponentInChildren<Animator>();
        }
        if (animator != null) { animator.enabled = true; }
        attach = main.transform.FindRecursive(rec.attach);
      }
      if (animator != null) { this.animators.Add(new AttachInfoAnimator(animator, 1f)); }
      weapons = new HashSet<MechComponent>();
      bayComponents = new HashSet<ComponentRepresentation>();
      hideIfEmpty = rec.hideIfEmpty;
      noRecoil = rec.noRecoil;
      recoilValue = 0f;
      recoilDelta = 0f;
      //inPosition = false;
    }
    public AttachInfo(GameObject parent, AttachInfoRecord rec) {
      //this.parent = parent;
      this.animators = new HashSet<AttachInfoAnimator>();
      this.location = location;
      this.type = rec.type;
      if (parent != null) {
        main = parent.transform.FindRecursive(rec.visuals);
        foreach (var animName in rec.Animators) {
          if (string.IsNullOrEmpty(animName.Key)) { continue; }
          Transform animTR = parent.transform.FindRecursive(animName.Key);
          if (animTR == null) { continue; }
          Animator animator = animTR.gameObject.GetComponent<Animator>();
          if (animator == null) { continue; }
          this.animators.Add(new AttachInfoAnimator(animator,animName.Value));
        }
        attach = parent.transform.FindRecursive(rec.attach);
      }
      weapons = new HashSet<MechComponent>();
      bayComponents = new HashSet<ComponentRepresentation>();
      hideIfEmpty = rec.hideIfEmpty;
      noRecoil = rec.noRecoil;
      recoilValue = 0f;
      recoilDelta = 0f;
      //inPosition = false;
    }
    public AttachInfo(MechTurretAnimation parent, MechTurretMountData data) {
      this.location = data.Location.ToString();
      //this.parent = parent;
      this.Name = data.Name;
      this.main = parent.transform;
      Animator animator = null;
      if (string.IsNullOrEmpty(this.Name)) { this.Name = data.Location.ToString(); }
      Transform anim = parent.gameObject.transform.FindRecursive(data.Animator);
      if (anim != null) { animator = anim.gameObject.GetComponent<Animator>(); }
      Transform attach = parent.gameObject.transform.FindRecursive(data.AttachTo);
      if (attach != null) { this.attach = attach; } else { this.attach = parent.gameObject.transform; }
      if (animator != null) { this.animators.Add(new AttachInfoAnimator(animator, 1f)); }
      hideIfEmpty = true;
      noRecoil = false;
      recoilValue = 0f;
      recoilDelta = 0f;
    }
    public float LowestPrefireRate {
      get {
        float result = 1000f;
        foreach(AttachInfoAnimator anim in animators) {
          if (anim.animator == null) { continue; }
          if ((anim.HasIndirect == false) && (anim.HasNormal == false) && (anim.HasVertical == false)) { continue; }
          if (result < anim.animationRate) { result = anim.animationRate; }
        }
        return result;
      }
    }
    public float LowestFireRate {
      get {
        float result = 1000f;
        foreach (AttachInfoAnimator anim in animators) {
          if (anim.animator == null) { continue; }
          if ((anim.HasEmiterIndex == false)) { continue; }
          if (result < anim.animationRate) { result = anim.animationRate; }
        }
        return result;
      }
    }
  }
  public class VTOLBodyAnimationData {
    public string explode_sound { get; set; }
    public string bodyAnimator { get; set; }
    public string bodyAttach { get; set; }
    public string engineAnimator { get; set; }
    public List<string> lights { get; set; }
    public Dictionary<string,Dictionary<HardpointAttachType, AttachInfoRecord>> attachInfo { get; set; }
    public VTOLBodyAnimationData() {
      explode_sound = string.Empty;
      bodyAnimator = string.Empty;
      engineAnimator = string.Empty;
      bodyAttach = string.Empty;
      lights = new List<string>();
      attachInfo = new Dictionary<string, Dictionary<HardpointAttachType, AttachInfoRecord>>();
    }
  };
  public static class VTOLBodyAnimationHelper {
    private static Dictionary<ICombatant, VTOLBodyAnimation> bodyAnimationsCache = new Dictionary<ICombatant, VTOLBodyAnimation>();
    private static Dictionary<Weapon, AttachInfo> weaponsAttachInfo = new Dictionary<Weapon, AttachInfo>();
    public static AttachInfo attachInfo(this Weapon weapon) {
      if (weaponsAttachInfo.TryGetValue(weapon, out AttachInfo info)) {
        return info;
      };
      return null;
    }
    public static void attachInfo(this Weapon weapon, AttachInfo info) {
      if (weaponsAttachInfo.ContainsKey(weapon)) { weaponsAttachInfo[weapon] = info; return; }
      weaponsAttachInfo.Add(weapon,info);
    }
    public static VTOLBodyAnimation VTOLAnimation(this ICombatant combatant) {
      if (bodyAnimationsCache.TryGetValue(combatant, out VTOLBodyAnimation anim)) {
        return anim;
      } else {
        return null;
      }
    }
    public static void VTOLAnimation(this ICombatant combatant, VTOLBodyAnimation anim) {
      if (bodyAnimationsCache.ContainsKey(combatant) == false) { bodyAnimationsCache.Add(combatant, anim); } else { bodyAnimationsCache[combatant] = anim; };
    }
    public static void Clear() { bodyAnimationsCache.Clear(); }
  }
  public class VTOLBodyAnimation: GenericAnimatedComponent, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler {
    public Animator bodyAnimator { get; private set; }
    public Transform bodyAttach { get; private set; }
    public Animator engineAnimator { get; private set; }
    public List<Transform> lights { get; private set; }
    private HashSet<AttachInfo> recoilState;
    public Dictionary<string, Dictionary<HardpointAttachType, AttachInfo>> attachInfo { get; private set; }
    private VTOLFallStoper fallStopper = null;
    public override bool StayOnDeath() { return true; }
    public override bool KeepPosOnDeath() { return true; }
    public override bool StayOnLocationDestruction() { return true; }
    private MechRepresentationSimGame findParentSimGameRep(Transform parent) {
      MechRepresentationSimGame result = parent.GetComponent<MechRepresentationSimGame>();
      if (result != null) { return result; }
      if (parent.parent == null) { return null; };
      return findParentSimGameRep(parent.parent);
    }
    public void ResolveAttachPoints() {
      Log.TWL(0, "VTOLBodyAnimation.ResolveAttachPoints "+this.transform.name);
      Dictionary<Transform, HashSet<AttachInfo>> infos = new Dictionary<Transform, HashSet<AttachInfo>>();
      foreach(var attaches in attachInfo) {
        Log.WL(1, "location:" + attaches.Key);
        foreach (var info in attaches.Value) {
          Log.WL(2, "type:" + info.Key.ToString()+" weapons:"+ info.Value.weapons.Count+"/"+ info.Value.bayComponents.Count);
          if (info.Value.main == null) { continue; }
          if (infos.ContainsKey(info.Value.main) == false) { infos.Add(info.Value.main, new HashSet<AttachInfo>()); }
          infos[info.Value.main].Add(info.Value);
          /*if (info.Value.hideIfEmpty) {
            info.Value.main.gameObject.SetActive((info.Value.weapons.Count > 0) || (info.Value.bayComponents.Count > 0));
          } else {
            info.Value.main.gameObject.SetActive(true);
          }*/
        }
      }
      foreach(var info in infos) {
        bool hideIfEmpty = true;
        bool isEmpty = true;
        foreach (var attach in info.Value) {
          if (attach.hideIfEmpty == false) { hideIfEmpty = false; }
          Log.WL(2, "main:" + info.Key.name + " attach:"+ attach.main.name+ "/"+attach.type+" weapons:" + attach.weapons.Count + "/" + attach.bayComponents.Count);
          if ((attach.weapons.Count > 0) || (attach.bayComponents.Count > 0)) { isEmpty = false; }
        }
        Log.WL(1, "main:" + info.Key.name+" hideIfEmpty:"+hideIfEmpty+" isEmpty:"+isEmpty);
        if (hideIfEmpty == false) { info.Key.gameObject.SetActive(true); } else {
          info.Key.gameObject.SetActive(!isEmpty);
        }
      }
    }
    public void RealiginLights() {
      Log.TWL(0, "VTOLBodyAnimation.RealiginLights "+this.lights.Count);
      if(this.parent != null) {
        Log.WL(1, "in battle");
        BTLight[] btlights = this.parent.GameRep.GetComponentsInChildren<BTLight>();
        Log.WL(1, "BTLights:" + btlights.Length);
        int count = Mathf.Min(btlights.Length, this.lights.Count);
        for (int index = 0; index < count; ++index) {
          BTLight btlight = btlights[index];
          Transform light = this.lights[index];
          btlight.transform.SetParent(light);
          btlight.transform.localPosition = Vector3.zero;
        }
      } else {
        Log.WL(1,"in bay");
        MechRepresentationSimGame simParent = this.findParentSimGameRep(this.transform);
        if (simParent == null) {
          Log.WL(1, "MechRepresentationSimGame not found"); return;
        }
        BTLight[] btlights = simParent.GetComponentsInChildren<BTLight>();
        Log.WL(1, "BTLights:"+btlights.Length);
        int count = Mathf.Min(btlights.Length,this.lights.Count);
        for (int index = 0; index < count; ++index) {
          BTLight btlight = btlights[index];
          Transform light = this.lights[index];
          btlight.transform.SetParent(light);
          btlight.transform.localPosition = Vector3.zero;
        }
      }
    }
    public void Update() {
      float t = Time.deltaTime;
      foreach (AttachInfo info in recoilState) { info.Update(t); }
    }
    public Transform GetAttachTransform(string location,HardpointAttachType type) {
      if(attachInfo.TryGetValue(location, out Dictionary<HardpointAttachType,AttachInfo> locAttaches)) {
        if(locAttaches.TryGetValue(type, out AttachInfo info)) {
          return info.attach;
        }
      }
      return null;
    }
    public AttachInfo GetAttachInfo(string location, HardpointAttachType type) {
      if (attachInfo.TryGetValue(location, out Dictionary<HardpointAttachType, AttachInfo> locAttaches)) {
        if (locAttaches.TryGetValue(type, out AttachInfo info)) {
          return info;
        }
      }
      return null;
    }
    public void twist(float value) {
      if (bodyAnimator != null) {
        value *= (90f / 120f);
        if (value >= 0f) { bodyAnimator.SetFloat("TwistL", 0f); bodyAnimator.SetFloat("TwistR", value); }else
        if (value <= 0f) { bodyAnimator.SetFloat("TwistL", Mathf.Abs(value)); bodyAnimator.SetFloat("TwistR", 0f); }
      }
    }
    public override void OnDeath() { bodyAnimator.SetBool("fall", true); engineAnimator.SetFloat("rotate",0.5f); base.OnDeath(); }
    public override void Init(ICombatant a, int loc, string data, string prefabName) {
      base.Init(a, loc, data, prefabName);
      this.recoilState = new HashSet<AttachInfo>();
      lights = new List<Transform>();
      VTOLBodyAnimationData srdata = JsonConvert.DeserializeObject<VTOLBodyAnimationData>(data);
      if (string.IsNullOrEmpty(srdata.bodyAnimator) == false) {
        Transform baHolder = this.transform.FindRecursive(srdata.bodyAnimator);
        if (baHolder != null) { bodyAnimator = baHolder.gameObject.GetComponent<Animator>(); }
      }
      if (string.IsNullOrEmpty(srdata.bodyAttach) == false) {
        this.bodyAttach = this.transform.FindRecursive(srdata.bodyAttach);
      }
      if (string.IsNullOrEmpty(srdata.engineAnimator) == false) {
        Transform eaHolder = this.transform.FindRecursive(srdata.engineAnimator);
        if (eaHolder != null) { engineAnimator = eaHolder.gameObject.GetComponent<Animator>(); }
      }
      foreach (string light in srdata.lights) {
        if (string.IsNullOrEmpty(light) == false) {
          Transform lightTr = this.transform.FindRecursive(light);
          if (lightTr != null) { this.lights.Add(lightTr); }
        }
      }
      if (engineAnimator != null) {
        if(a != null) {
          engineAnimator.SetFloat("rotate", 1f);
        } else {
          engineAnimator.SetFloat("rotate", 0f);
        }
      };
      Log.WL(1, "VTOLBodyAnimation.Init " + this.gameObject.name + " engineAnimator:" + (engineAnimator != null ? " rotating:" + engineAnimator.GetFloat("rotate") : "null"));
      this.attachInfo = new Dictionary<string, Dictionary<HardpointAttachType, AttachInfo>>();
      Log.WL(1, "srdata.attachInfo:" + srdata.attachInfo.Count);
      foreach (var att_infs in srdata.attachInfo) {
        this.attachInfo.Add(att_infs.Key,new Dictionary<HardpointAttachType, AttachInfo>());
        Log.WL(2, "location:" + att_infs.Key);
        foreach (var att_inf in att_infs.Value) {
          Log.WL(3, "type:" + att_inf.Key.ToString()+" "+ att_inf.Value.visuals);
          AttachInfo info = new AttachInfo(this, att_infs.Key, att_inf.Key, att_inf.Value);
          this.recoilState.Add(info);
          this.attachInfo[att_infs.Key].Add(att_inf.Key, info);
        }
      }
      if (bodyAnimator != null) {
        fallStopper = bodyAnimator.gameObject.GetComponent<VTOLFallStoper>();
        if (fallStopper == null) { fallStopper = bodyAnimator.gameObject.AddComponent<VTOLFallStoper>(); };
        fallStopper.Init(srdata.explode_sound, this);
      }
      if (a != null) { a.VTOLAnimation(this); }
      this.RealiginLights();
    }

    public void OnPointerClick(PointerEventData eventData) {
      Log.TWL(0, "VTOLBodyAnimation.OnPointerClick");
      this.parent.GameRep.OnPointerClick(eventData);
    }

    public void OnPointerEnter(PointerEventData eventData) {
      Log.TWL(0, "VTOLBodyAnimation.OnPointerEnter");
      this.parent.GameRep.OnPointerEnter(eventData);
    }

    public void OnPointerExit(PointerEventData eventData) {
      Log.TWL(0, "VTOLBodyAnimation.OnPointerExit");
      this.parent.GameRep.OnPointerExit(eventData);
    }
  }
}
