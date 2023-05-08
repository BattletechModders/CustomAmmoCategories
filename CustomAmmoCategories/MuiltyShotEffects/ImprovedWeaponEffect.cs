using BattleTech;
using BattleTech.Rendering;
using BattleTech.Rendering.UrbanWarfare;
using HBS.Logging;
using UnityEngine;

namespace CustAmmoCategories {
  public class ImprovedParentWeaponEffect: MonoBehaviour {
    protected static readonly ILog logger = HBS.Logging.Logger.GetLogger("CombatLog.AttackSequence");
    protected CombatGameState Combat;
    public WeaponHitInfo hitInfo;
    public int hitIndex;
    protected int emitterIndex;
    protected int numberOfEmitters;
    public bool subEffect;
    public WeaponEffect.WeaponEffectState currentState;
    public WeaponRepresentation weaponRep;
    public Weapon weapon;
    protected AkGameObj parentAudioObject;
    protected Transform startingTransform;
    protected Vector3 startPos;
    protected Vector3 endPos;
    protected Vector3 currentPos;
    protected float t;
    public float attackSequenceNextDelayMin;
    public float attackSequenceNextDelayMax;
    protected float attackSequenceNextDelayTimer;
    private bool hasSentNextWeaponMessage;
    public float preFireDuration;
    protected float preFireRate;
    protected float duration;
    protected float rate;
    public float projectileSpeed;
    public AudioSwitch_weapon_type weaponImpactType;
    public GameObject preFireVFXPrefab;
    public GameObject muzzleFlashVFXPrefab;
    public GameObject projectilePrefab;
    public GameObject projectile;
    protected string activeProjectileName;
    protected Transform projectileTransform;
    protected ParticleSystem projectileParticles;
    protected AkGameObj projectileAudioObject;
    protected GameObject projectileMeshObject;
    protected GameObject projectileLightObject;
    public string impactVFXBase = "";
    public string[] impactVFXVariations;
    public string armorDamageVFXName;
    public string structureDamageVFXName;
    public string terrainHitVFXBase;
    public string buildingHitOverlayVFXName;
    public bool shotsDestroyFlimsyObjects;
    public string preFireSFX = "";
    public virtual bool Active => this.currentState != WeaponEffect.WeaponEffectState.NotStarted && this.currentState != WeaponEffect.WeaponEffectState.Complete;
    public virtual bool FiringComplete { get; protected set; }
    public virtual bool AllowMissSkipping {
      get => false;
      protected set {
      }
    }
    protected virtual int ImpactPrecacheCount => 1;
    protected virtual void Awake() {
      this.currentState = WeaponEffect.WeaponEffectState.NotStarted;
      this.hasSentNextWeaponMessage = false;
      this.AllowMissSkipping = true;
    }
    protected virtual void Start() {
      if (this.duration <= CustomAmmoCategories.Epsilon)
        this.duration = 1f;
      this.rate = 1f / this.duration;
    }
    protected virtual void OnDestroy() {
      if (this.projectileAudioObject == null) { return; }
      AkSoundEngine.StopAll(this.projectileAudioObject.gameObject);
    }
    public virtual void Init(Weapon weapon) {
      this.weapon = weapon;
      this.weaponRep = weapon.weaponRep;
      this.Combat = weapon.parent.Combat;
      this.numberOfEmitters = this.weaponRep.vfxTransforms.Length;
      if (this.projectilePrefab != null)
        this.Combat.DataManager.PrecachePrefabAsync(this.projectilePrefab.name, BattleTechResourceType.Prefab, 1);
      if (this.preFireVFXPrefab != null)
        this.Combat.DataManager.PrecachePrefabAsync(this.preFireVFXPrefab.name, BattleTechResourceType.Prefab, 1);
      if (this.muzzleFlashVFXPrefab != null)
        this.Combat.DataManager.PrecachePrefabAsync(this.muzzleFlashVFXPrefab.name, BattleTechResourceType.Prefab, 1);
      if (string.IsNullOrEmpty(this.armorDamageVFXName) == false) {
        this.Combat.DataManager.PrecachePrefabAsync(this.armorDamageVFXName + "_sm", BattleTechResourceType.Prefab, 1);
        this.Combat.DataManager.PrecachePrefabAsync(this.armorDamageVFXName + "_lrg", BattleTechResourceType.Prefab, 1);
      }
      if (string.IsNullOrEmpty(this.structureDamageVFXName) == false) {
        this.Combat.DataManager.PrecachePrefabAsync(this.structureDamageVFXName + "_sm", BattleTechResourceType.Prefab, 1);
        this.Combat.DataManager.PrecachePrefabAsync(this.structureDamageVFXName + "_lrg", BattleTechResourceType.Prefab, 1);
      }
      if (string.IsNullOrEmpty(this.buildingHitOverlayVFXName) == false)
        this.Combat.DataManager.PrecachePrefabAsync(this.buildingHitOverlayVFXName, BattleTechResourceType.Prefab, 1);
      EffectData[] statusEffects = weapon.weaponDef.statusEffects;
      for (int index = 0; index < statusEffects.Length; ++index) {
        if (statusEffects[index].effectType == EffectType.VFXEffect)
          this.Combat.DataManager.PrecachePrefabAsync(statusEffects[index].vfxData.vfxName, BattleTechResourceType.Prefab, 1);
      }
      this.PreCacheImpacts();
    }
    protected virtual void PreCacheImpacts() {
      if (string.IsNullOrEmpty(this.impactVFXBase))
        return;
      string id = string.Format("{0}_crit", (object)this.impactVFXBase);
      if (this.Combat.DataManager.ResourceEntryExists(BattleTechResourceType.Prefab, id))
        this.Combat.DataManager.PrecachePrefabAsync(id, BattleTechResourceType.Prefab, this.ImpactPrecacheCount);
      if (this.impactVFXVariations == null)
        return;
      for (int index = 0; index < this.impactVFXVariations.Length; ++index)
        this.Combat.DataManager.PrecachePrefabAsync(string.Format("{0}_{1}", (object)this.impactVFXBase, (object)this.impactVFXVariations[index]), BattleTechResourceType.Prefab, this.ImpactPrecacheCount);
    }
    public virtual void InitProjectile() {
      if (this.projectilePrefab != null && this.projectile != null)
        this.weapon.parent.Combat.DataManager.PoolGameObject(this.activeProjectileName, this.projectile);
      if (this.projectilePrefab != null) {
        this.activeProjectileName = this.projectilePrefab.name;
        this.projectile = this.Combat.DataManager.PooledInstantiate(this.activeProjectileName, BattleTechResourceType.Prefab);
      }
      this.projectileParticles = this.projectile.GetComponent<ParticleSystem>();
      this.projectileTransform = this.projectile.transform;
      MeshRenderer componentInChildren1 = this.projectile.GetComponentInChildren<MeshRenderer>(true);
      if (componentInChildren1 != null) {
        this.projectileMeshObject = componentInChildren1.gameObject;
        this.projectileMeshObject.SetActive(false);
      }
      BTLight componentInChildren2 = this.projectile.GetComponentInChildren<BTLight>(true);
      if (componentInChildren2 != null) {
        this.projectileLightObject = componentInChildren2.gameObject;
        this.projectileLightObject.SetActive(false);
      }
      this.projectileAudioObject = this.projectile.GetComponent<AkGameObj>();
      if (this.projectileAudioObject == null)
        this.projectileAudioObject = this.projectile.AddComponent<AkGameObj>();
      this.projectileAudioObject.listenerMask = 0;
      this.projectileAudioObject.isEnvironmentAware = false;
      WwiseManager.SetSwitch<AudioSwitch_weapon_type>(this.weaponImpactType, this.projectileAudioObject);
      this.parentAudioObject = !(this.weapon.parent.GameRep != null) || !(this.weapon.parent.GameRep.audioObject != null) ? this.projectileAudioObject : this.weapon.parent.GameRep.audioObject;
      WwiseManager.SetSwitch<AudioSwitch_weapon_type>(this.weaponImpactType, this.parentAudioObject);
      if (this.hitInfo.ShotHitLocation(this.hitIndex) != 0 && this.hitInfo.ShotHitLocation(this.hitIndex) != 65536) {
        WwiseManager.SetSwitch<AudioSwitch_mech_hit_or_miss>(AudioSwitch_mech_hit_or_miss.mech_hit, this.parentAudioObject);
        WwiseManager.SetSwitch<AudioSwitch_mech_hit_or_miss>(AudioSwitch_mech_hit_or_miss.mech_hit, this.projectileAudioObject);
      } else {
        WwiseManager.SetSwitch<AudioSwitch_mech_hit_or_miss>(AudioSwitch_mech_hit_or_miss.mech_miss, this.parentAudioObject);
        WwiseManager.SetSwitch<AudioSwitch_mech_hit_or_miss>(AudioSwitch_mech_hit_or_miss.mech_miss, this.projectileAudioObject);
      }
      if (!(this.weapon.parent is Mech parent))
        return;
      AudioSwitch_mech_weight_type switchEnumValue = AudioSwitch_mech_weight_type.b_medium;
      switch (parent.MechDef.Chassis.weightClass) {
        case WeightClass.LIGHT:
        switchEnumValue = AudioSwitch_mech_weight_type.a_light;
        break;
        case WeightClass.MEDIUM:
        switchEnumValue = AudioSwitch_mech_weight_type.b_medium;
        break;
        case WeightClass.HEAVY:
        switchEnumValue = AudioSwitch_mech_weight_type.c_heavy;
        break;
        case WeightClass.ASSAULT:
        switchEnumValue = AudioSwitch_mech_weight_type.d_assault;
        break;
      }
      WwiseManager.SetSwitch<AudioSwitch_mech_weight_type>(switchEnumValue, this.projectileAudioObject);
    }
    public virtual void Fire(WeaponHitInfo hitInfo, int hitIndex = 0, int emitterIndex = 0) {
      this.t = 0.0f;
      this.hitIndex = hitIndex;
      this.emitterIndex = emitterIndex;
      this.hitInfo = hitInfo;
      this.startingTransform = this.weaponRep.vfxTransforms[emitterIndex];
      this.startPos = this.startingTransform.position;
      if (hitInfo.DidShotHitChosenTarget(hitIndex)) {
        ICombatant combatantByGuid = this.Combat.FindCombatantByGUID(hitInfo.ShotTargetId(hitIndex));
        if (combatantByGuid != null) {
          string secondaryTargetId = (string)null;
          int secondaryHitLocation = 0;
          hitInfo.hitPositions[hitIndex] = combatantByGuid.GetImpactPosition(this.weaponRep.parentCombatant as AbstractActor, this.startPos, this.weapon, ref hitInfo.hitLocations[hitIndex], ref hitInfo.attackDirections[hitIndex], ref secondaryTargetId, ref secondaryHitLocation);
        }
      }
      this.endPos = hitInfo.hitPositions[hitIndex];
      this.currentPos = this.startPos;
      this.FiringComplete = false;
      this.InitProjectile();
      this.currentState = WeaponEffect.WeaponEffectState.PreFiring;
    }
    protected virtual void PlayPreFire() {
      if (this.preFireVFXPrefab != null) {
        GameObject gameObject = this.weapon.parent.Combat.DataManager.PooledInstantiate(this.preFireVFXPrefab.name, BattleTechResourceType.Prefab);
        ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
        AutoPoolObject autoPoolObject = gameObject.GetComponent<AutoPoolObject>();
        if (autoPoolObject == null)
          autoPoolObject = gameObject.AddComponent<AutoPoolObject>();
        autoPoolObject.Init(this.weapon.parent.Combat.DataManager, this.preFireVFXPrefab.name, component);
        component.Stop(true);
        component.Clear(true);
        component.transform.parent = (Transform)null;
        component.transform.position = this.startingTransform.position;
        component.transform.LookAt(this.endPos);
        BTCustomRenderer.SetVFXMultiplier(component);
        component.Play(true);
        if ((double)this.preFireDuration <= 0.0)
          this.preFireDuration = component.main.duration;
      }
      if (!string.IsNullOrEmpty(this.preFireSFX)) {
        int num = (int)WwiseManager.PostEvent(this.preFireSFX, this.parentAudioObject);
      }
      this.preFireRate = (double)this.preFireDuration <= 0.0 ? 1000f : 1f / this.preFireDuration;
      if ((double)this.attackSequenceNextDelayMin <= 0.0 && (double)this.attackSequenceNextDelayMax <= 0.0)
        this.attackSequenceNextDelayMax = this.preFireDuration;
      if ((double)this.attackSequenceNextDelayMax <= 0.0)
        this.attackSequenceNextDelayMax = 0.05f;
      if ((double)this.attackSequenceNextDelayMin >= (double)this.attackSequenceNextDelayMax)
        this.attackSequenceNextDelayMin = this.attackSequenceNextDelayMax;
      this.attackSequenceNextDelayTimer = UnityEngine.Random.Range(this.attackSequenceNextDelayMin, this.attackSequenceNextDelayMax);
      this.t = 0.0f;
      this.currentState = WeaponEffect.WeaponEffectState.PreFiring;
    }

    protected virtual void PlayMuzzleFlash() {
      if (this.muzzleFlashVFXPrefab == null)
        return;
      GameObject gameObject = this.weapon.parent.Combat.DataManager.PooledInstantiate(this.muzzleFlashVFXPrefab.name, BattleTechResourceType.Prefab);
      ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
      AutoPoolObject autoPoolObject = gameObject.GetComponent<AutoPoolObject>();
      if (autoPoolObject == null)
        autoPoolObject = gameObject.AddComponent<AutoPoolObject>();
      autoPoolObject.Init(this.weapon.parent.Combat.DataManager, this.muzzleFlashVFXPrefab.name, component);
      component.Stop(true);
      component.Clear(true);
      component.transform.parent = this.startingTransform;
      component.transform.localPosition = Vector3.zero;
      component.transform.LookAt(this.endPos);
      BTCustomRenderer.SetVFXMultiplier(component);
      component.Play(true);
      BTLightAnimator componentInChildren = gameObject.GetComponentInChildren<BTLightAnimator>(true);
      if (componentInChildren == null)
        return;
      componentInChildren.StopAnimation();
      componentInChildren.PlayAnimation();
    }
    protected virtual void PlayProjectile() {
      this.t = 0.0f;
      this.currentState = WeaponEffect.WeaponEffectState.Firing;
      if (this.projectileMeshObject != null)
        this.projectileMeshObject.SetActive(true);
      if ((UnityEngine.Object)this.projectileLightObject != (UnityEngine.Object)null)
        this.projectileLightObject.SetActive(true);
      if ((UnityEngine.Object)this.projectileParticles != (UnityEngine.Object)null) {
        this.projectileParticles.Stop(true);
        this.projectileParticles.Clear(true);
      }
      this.projectileTransform.position = this.startingTransform.position;
      this.projectileTransform.LookAt(this.endPos);
      this.startPos = this.startingTransform.position;
      if ((UnityEngine.Object)this.projectileParticles != (UnityEngine.Object)null) {
        BTCustomRenderer.SetVFXMultiplier(this.projectileParticles);
        this.projectileParticles.Play(true);
        BTLightAnimator componentInChildren = this.projectileParticles.GetComponentInChildren<BTLightAnimator>(true);
        if ((UnityEngine.Object)componentInChildren != (UnityEngine.Object)null) {
          componentInChildren.StopAnimation();
          componentInChildren.PlayAnimation();
        }
      }
      if ((UnityEngine.Object)this.weapon.parent.GameRep != (UnityEngine.Object)null) {
        int num;
        switch ((ChassisLocations)this.weapon.Location) {
          case ChassisLocations.LeftArm:
          num = 1;
          break;
          case ChassisLocations.RightArm:
          num = 2;
          break;
          default:
          num = 0;
          break;
        }
        this.weapon.parent.GameRep.PlayFireAnim((AttackSourceLimb)num, this.weapon.weaponDef.AttackRecoil);
      }
      if (!this.AllowMissSkipping || this.hitInfo.DidShotHitChosenTarget(this.hitIndex))
        return;
      this.PublishWeaponCompleteMessage();
    }

    protected virtual void PlayImpact() {
      if (this.hitInfo.DidShotHitAnything(this.hitIndex) && !string.IsNullOrEmpty(this.impactVFXBase)) {
        string str1 = "";
        AbstractActor actorByGuid = this.Combat.FindActorByGUID(this.hitInfo.ShotTargetId(this.hitIndex));
        if (actorByGuid != null && this.hitInfo.ShotHitLocation(this.hitIndex) != 65536 && (double)this.weapon.DamagePerShotAdjusted(this.weapon.parent.occupiedDesignMask) > (double)actorByGuid.ArmorForLocation(this.hitInfo.ShotHitLocation(this.hitIndex)))
          str1 = "_crit";
        else if (this.impactVFXVariations != null && this.impactVFXVariations.Length != 0)
          str1 = "_" + this.impactVFXVariations[UnityEngine.Random.Range(0, this.impactVFXVariations.Length)];
        string str2 = string.Format("{0}{1}", (object)this.impactVFXBase, (object)str1);
        GameObject gameObject = this.weapon.parent.Combat.DataManager.PooledInstantiate(str2, BattleTechResourceType.Prefab);
        if (gameObject == null) {
          logger.LogError((object)(this.weapon.Name + " WeaponEffect.PlayImpact had an invalid VFX name: " + str2));
        } else {
          ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
          component.Stop(true);
          component.Clear(true);
          component.transform.position = this.endPos;
          component.transform.LookAt(this.startingTransform.position);
          BTCustomRenderer.SetVFXMultiplier(component);
          component.Play(true);
          BTLightAnimator componentInChildren = gameObject.GetComponentInChildren<BTLightAnimator>(true);
          if ((UnityEngine.Object)componentInChildren != (UnityEngine.Object)null) {
            componentInChildren.StopAnimation();
            componentInChildren.PlayAnimation();
          }
          AutoPoolObject autoPoolObject = gameObject.GetComponent<AutoPoolObject>();
          if ((UnityEngine.Object)autoPoolObject == (UnityEngine.Object)null)
            autoPoolObject = gameObject.AddComponent<AutoPoolObject>();
          autoPoolObject.Init(this.weapon.parent.Combat.DataManager, str2, component);
        }
      }
      this.PlayImpactDamageOverlay();
      if ((UnityEngine.Object)this.projectileMeshObject != (UnityEngine.Object)null)
        this.projectileMeshObject.SetActive(false);
      if ((UnityEngine.Object)this.projectileLightObject != (UnityEngine.Object)null)
        this.projectileLightObject.SetActive(false);
      this.OnImpact();
    }

    protected virtual void PlayTerrainImpactVFX() {
      MapTerrainDataCell cellAt = this.weapon.parent.Combat.MapMetaData.GetCellAt(this.hitInfo.hitPositions[this.hitIndex]);
      if (cellAt == null)
        return;
      string str1 = "";
      string str2;
      if (!string.IsNullOrEmpty(this.terrainHitVFXBase)) {
        string vfxNameModifier = cellAt.GetVFXNameModifier();
        string str3;
        switch (cellAt.GetAudioSurfaceType()) {
          case AudioSwitch_surface_type.dirt:
          str3 = "dirt" + vfxNameModifier;
          break;
          case AudioSwitch_surface_type.metal:
          str3 = "metal";
          break;
          case AudioSwitch_surface_type.snow:
          str3 = "snow";
          break;
          case AudioSwitch_surface_type.wood:
          str3 = "wood";
          break;
          case AudioSwitch_surface_type.brush:
          str3 = "brush";
          break;
          case AudioSwitch_surface_type.concrete:
          str3 = "concrete" + vfxNameModifier;
          break;
          case AudioSwitch_surface_type.debris_glass:
          str3 = "debris_glass" + vfxNameModifier;
          break;
          case AudioSwitch_surface_type.gravel:
          str3 = "gravel";
          break;
          case AudioSwitch_surface_type.ice:
          str3 = "ice";
          break;
          case AudioSwitch_surface_type.lava:
          str3 = "lava";
          break;
          case AudioSwitch_surface_type.mud:
          str3 = "mud";
          break;
          case AudioSwitch_surface_type.sand:
          str3 = "sand";
          break;
          case AudioSwitch_surface_type.water_deep:
          case AudioSwitch_surface_type.water_shallow:
          str3 = "water";
          break;
          default:
          str3 = "dirt";
          break;
        }
        str2 = string.Format("{0}_{1}", (object)this.terrainHitVFXBase, (object)str3);
      } else {
        if (this.impactVFXVariations != null && this.impactVFXVariations.Length != 0)
          str1 = "_" + this.impactVFXVariations[UnityEngine.Random.Range(0, this.impactVFXVariations.Length)];
        str2 = string.Format("{0}{1}", (object)this.impactVFXBase, (object)str1);
      }
      GameObject gameObject = this.weapon.parent.Combat.DataManager.PooledInstantiate(str2, BattleTechResourceType.Prefab);
      if (gameObject == null) {
        logger.LogError((object)(this.weapon.Name + " WeaponEffect.PlayTerrainImpactVFX had an invalid VFX name: " + str2));
      } else {
        ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
        component.Stop(true);
        component.Clear(true);
        component.transform.position = this.endPos;
        component.transform.LookAt(this.startingTransform.position);
        BTCustomRenderer.SetVFXMultiplier(component);
        component.Play(true);
        AutoPoolObject autoPoolObject = gameObject.GetComponent<AutoPoolObject>();
        if ((UnityEngine.Object)autoPoolObject == (UnityEngine.Object)null)
          autoPoolObject = gameObject.AddComponent<AutoPoolObject>();
        autoPoolObject.Init(this.weapon.parent.Combat.DataManager, str2, component);
      }
    }

    protected virtual void PlayImpactDamageOverlay() {
      if (!this.hitInfo.DidShotHitAnything(this.hitIndex))
        return;
      ICombatant combatantByGuid1 = this.Combat.FindCombatantByGUID(this.hitInfo.ShotTargetId(this.hitIndex));
      if (combatantByGuid1 != null && (UnityEngine.Object)combatantByGuid1.GameRep != (UnityEngine.Object)null) {
        if (combatantByGuid1.UnitType == UnitType.Building) {
          this.PlayBuildingHitOverlayVFX(combatantByGuid1);
          this.PlayTerrainImpactVFX();
        } else {
          bool flag = false;
          string str1;
          if ((double)this.weapon.DamagePerShotAdjusted(this.weapon.parent.occupiedDesignMask) > (double)combatantByGuid1.ArmorForLocation(this.hitInfo.ShotHitLocation(this.hitIndex))) {
            str1 = this.structureDamageVFXName;
            flag = true;
          } else
            str1 = this.armorDamageVFXName;
          string str2 = "_sm";
          if (combatantByGuid1 is Mech mech && (mech.weightClass == WeightClass.ASSAULT || mech.weightClass == WeightClass.HEAVY))
            str2 = "_lrg";
          string vfxName = string.Format("{0}{1}", (object)str1, (object)str2);
          ChassisLocations fromArmorLocation = MechStructureRules.GetChassisLocationFromArmorLocation((ArmorLocation)this.hitInfo.ShotHitLocation(this.hitIndex));
          Transform vfxTransform = combatantByGuid1.GameRep.GetVFXTransform((int)fromArmorLocation);
          combatantByGuid1.GameRep.PlayVFXAt(vfxTransform, this.endPos, vfxName, true, this.startPos, !flag, -1f);
        }
      } else {
        ICombatant combatantByGuid2 = this.Combat.FindCombatantByGUID(this.hitInfo.targetId);
        if (combatantByGuid2 != null && (UnityEngine.Object)combatantByGuid2.GameRep != (UnityEngine.Object)null && (combatantByGuid2.UnitType == UnitType.Building && combatantByGuid2.UnitType == UnitType.Building))
          this.PlayBuildingHitOverlayVFX(combatantByGuid2);
        this.PlayTerrainImpactVFX();
        this.DestroyFlimsyObjects();
      }
    }

    private void PlayBuildingHitOverlayVFX(ICombatant buildingTarget) {
      DestructibleUrbanBuilding component = buildingTarget.GameRep.GetComponent<DestructibleUrbanBuilding>();
      if ((UnityEngine.Object)component != (UnityEngine.Object)null)
        buildingTarget.GameRep.PlayVFXAt(component.buildingWholeGameObject.transform, component.buildingWholeGameObject.transform.InverseTransformPoint(this.endPos), this.buildingHitOverlayVFXName, true, this.startPos, true, -1f);
      else
        buildingTarget.GameRep.PlayVFXAt((Transform)null, this.endPos, this.buildingHitOverlayVFXName, false, this.startPos, true, -1f);
    }

    protected virtual void PlayImpactAudio() {
      if (!this.hitInfo.DidShotHitAnything(this.hitIndex))
        return;
      AudioSwitch_surface_type switchEnumValue = AudioSwitch_surface_type.dirt;
      ICombatant hitInfoTarget = this.weapon.parent.Combat.AttackDirector.GetHitInfoTarget(this.hitInfo, this.hitIndex);
      if (hitInfoTarget != null) {
        if (hitInfoTarget is AbstractActor abstractActor) {
          switchEnumValue = AudioSwitch_surface_type.metal;
          if ((double)abstractActor.ArmorForLocation(this.hitInfo.ShotHitLocation(this.hitIndex)) <= 0.0)
            switchEnumValue = AudioSwitch_surface_type.mech_internal_structure;
        } else
          switchEnumValue = AudioSwitch_surface_type.concrete;
      } else {
        MapTerrainDataCell cellAt = this.weapon.parent.Combat.MapMetaData.GetCellAt(this.hitInfo.hitPositions[this.hitIndex]);
        if (cellAt != null)
          switchEnumValue = cellAt.GetAudioSurfaceType();
      }
      WwiseManager.SetSwitch<AudioSwitch_surface_type>(switchEnumValue, this.projectileAudioObject);
      int num = (int)WwiseManager.PostEvent<AudioEventList_impact>(AudioEventList_impact.impact_weapon, this.projectileAudioObject);
    }

    protected virtual void DestroyFlimsyObjects() {
      if (!this.shotsDestroyFlimsyObjects)
        return;
      foreach (Collider collider in Physics.OverlapSphere(this.endPos, 15f, -5, QueryTriggerInteraction.Ignore)) {
        Vector3 normalized = (collider.transform.position - this.endPos).normalized;
        float num = this.weapon.DamagePerShot + this.Combat.Constants.ResolutionConstants.FlimsyDestructionForceMultiplier;
        DestructibleObject component1 = collider.gameObject.GetComponent<DestructibleObject>();
        DestructibleUrbanFlimsy component2 = collider.gameObject.GetComponent<DestructibleUrbanFlimsy>();
        if ((UnityEngine.Object)component1 != (UnityEngine.Object)null && component1.isFlimsy) {
          component1.TakeDamage(this.endPos, normalized, num);
          component1.Collapse(normalized, num);
        }
        if ((UnityEngine.Object)component2 != (UnityEngine.Object)null)
          component2.PlayDestruction(normalized, num);
      }
    }

    protected virtual void Update() {
      if (this.currentState == WeaponEffect.WeaponEffectState.PreFiring) {
        if ((double)this.t <= 1.0)
          this.t += this.preFireRate * this.Combat.StackManager.GetProgressiveAttackDeltaTime(this.t);
        if ((double)this.t >= 1.0)
          this.OnPreFireComplete();
      }
      if (this.currentState == WeaponEffect.WeaponEffectState.Firing && (double)this.t <= 1.0)
        this.t += this.rate * this.Combat.StackManager.GetProgressiveAttackDeltaTime(this.t);
      if (!this.Active || this.subEffect || (this.weapon.WeaponCategoryValue.IsMelee || (double)this.attackSequenceNextDelayTimer <= 0.0))
        return;
      this.attackSequenceNextDelayTimer -= this.Combat.StackManager.GetProgressiveAttackDeltaTime(0.01f);
      if ((double)this.attackSequenceNextDelayTimer > 0.0)
        return;
      this.PublishNextWeaponMessage();
    }

    protected virtual void LateUpdate() {
    }

    protected virtual void OnPreFireComplete() {
    }

    protected virtual void OnImpact(float hitDamage = 0.0f, float structureDamage = 0.0f) => this.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AttackSequenceImpactMessage(this.hitInfo, this.hitIndex, hitDamage, structureDamage));

    protected virtual void OnComplete() {
      if (this.currentState == WeaponEffect.WeaponEffectState.Complete)
        return;
      this.currentState = WeaponEffect.WeaponEffectState.Complete;
      if (!this.subEffect)
        this.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AttackSequenceResolveDamageMessage(this.hitInfo));
      this.PublishNextWeaponMessage();
      this.PublishWeaponCompleteMessage();
      if (!((UnityEngine.Object)this.projectilePrefab != (UnityEngine.Object)null))
        return;
      AutoPoolObject autoPoolObject = this.projectile.GetComponent<AutoPoolObject>();
      if ((UnityEngine.Object)autoPoolObject == (UnityEngine.Object)null)
        autoPoolObject = this.projectile.AddComponent<AutoPoolObject>();
      autoPoolObject.Init(this.weapon.parent.Combat.DataManager, this.activeProjectileName, 4f);
      this.projectile = (GameObject)null;
    }

    protected virtual void PublishNextWeaponMessage() {
      if (!this.subEffect && !this.hasSentNextWeaponMessage)
        this.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AttackSequenceWeaponPreFireCompleteMessage(this.hitInfo.stackItemUID, this.hitInfo.attackSequenceId, this.hitInfo.attackGroupIndex, this.hitInfo.attackWeaponIndex));
      this.attackSequenceNextDelayTimer = -1f;
      this.hasSentNextWeaponMessage = true;
    }

    public virtual void PublishWeaponCompleteMessage() {
      if (!this.subEffect && !this.FiringComplete)
        this.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AttackSequenceWeaponCompleteMessage(this.hitInfo.stackItemUID, this.hitInfo.attackSequenceId, this.hitInfo.attackGroupIndex, this.hitInfo.attackWeaponIndex));
      this.FiringComplete = true;
    }

    public virtual void Reset() {
      this.currentState = WeaponEffect.WeaponEffectState.NotStarted;
      this.hasSentNextWeaponMessage = false;
      if ((UnityEngine.Object)this.projectileMeshObject != (UnityEngine.Object)null)
        this.projectileMeshObject.SetActive(false);
      if (!((UnityEngine.Object)this.projectileLightObject != (UnityEngine.Object)null))
        return;
      this.projectileLightObject.SetActive(false);
    }

  }
}