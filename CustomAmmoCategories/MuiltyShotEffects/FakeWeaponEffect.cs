using BattleTech;
using UnityEngine;

namespace CustAmmoCategories {
  public class FakeWeaponEffect: WeaponEffect {
    public override void Init(Weapon weapon) {
      this.weapon = weapon;
      this.weaponRep = weapon.weaponRep;
      this.Combat = weapon.parent.Combat;
      this.numberOfEmitters = 1;
      hitIndex = -1;
      subEffect = false;
      parentAudioObject = null;
      projectileAudioObject = null;
      startingTransform = null;
      attackSequenceNextDelayMin = 0.5f;
      attackSequenceNextDelayMax = 1f;
      preFireDuration = 0.5f;
      preFireRate = 1f;
      projectileSpeed = 1f;
      preFireVFXPrefab = null;
      muzzleFlashVFXPrefab = null;
      projectilePrefab = null;
      projectile = null;
      projectileTransform = null;
      activeProjectileName = string.Empty;
      projectileParticles = null;
      projectileMeshObject = null;
      projectileLightObject = null;
      impactVFXVariations = null;
      armorDamageVFXName = string.Empty;
      structureDamageVFXName = string.Empty;
      terrainHitVFXBase = string.Empty;
      buildingHitOverlayVFXName = string.Empty;
      shotsDestroyFlimsyObjects = false;
    }
    public override void InitProjectile() {
    }
    public override void Fire(WeaponHitInfo hitInfo, int hitIndex = 0, int emitterIndex = 0) {
      this.t = 0.0f;
      this.hitIndex = 0;
      this.emitterIndex = 0;
      this.hitInfo = hitInfo;
      this.startingTransform = null;
      this.startPos = Vector3.zero;
      this.endPos = Vector3.zero;
      this.currentPos = this.startPos;
      this.FiringComplete = false;
      this.InitProjectile();
      this.PlayPreFire();
    }
    protected override void PlayPreFire() {
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
    protected override void PlayMuzzleFlash() {
    }
    protected override void PlayProjectile() {
      this.t = 0.0f;
      this.currentState = WeaponEffect.WeaponEffectState.Firing;
    }
  }
}