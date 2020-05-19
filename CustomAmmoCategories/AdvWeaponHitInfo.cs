using BattleTech;
using CustomAmmoCategoriesLog;
using FluffyUnderware.Curvy;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CustAmmoCategories {
  public enum TrajectoryType { Unguided, Direct, Indirect };
  public class SplineGenerationInfo {
    public float CurveStrength;
    public int CurveFrequency;
    public bool isSRM;
    public TrajectoryType type;
    public SplineGenerationInfo() {
      CurveStrength = 0f;
      CurveFrequency = 2;
      isSRM = false;
      type = TrajectoryType.Unguided;
    }
    public Vector3[] generateTrajectory(int hitLocation, Vector3 startPos, Vector3 endPos) {
      Vector3[] result = null;
      switch (this.type) {
        case TrajectoryType.Indirect:
          result = CustomAmmoCategories.GenerateIndirectMissilePath(this.CurveStrength, this.CurveFrequency, this.isSRM, hitLocation, startPos, endPos, AdvWeaponHitInfoRec.Combat); break;
        case TrajectoryType.Direct:
          result = CustomAmmoCategories.GenerateMissilePath(this.CurveStrength, this.CurveFrequency, this.isSRM, hitLocation, startPos, endPos, AdvWeaponHitInfoRec.Combat); break;
        default:
          result = CustomAmmoCategories.GenerateDirectMissilePath(this.CurveStrength, this.CurveFrequency, this.isSRM, hitLocation, startPos, endPos, AdvWeaponHitInfoRec.Combat); break;
      }
      return result;
    }
  }
  public class InterceptableInfo {
    public bool Intercepted;
    public float InterceptedT;
    public List<AMSShoot> AMSShoots;
    public float AMSHitChance;
    public int AMSShootIndex;
    public Weapon InterceptedAMS;
    public bool AMSImunne;
    public float missileHealth;
    public InterceptableInfo() {
      Intercepted = false;
      InterceptedT = 2.0f;
      AMSShootIndex = 0;
      AMSHitChance = 0f;
      AMSShoots = new List<AMSShoot>();
      AMSImunne = true;
      missileHealth = 0f;
      InterceptedAMS = null;
    }
    public float getAMSShootT() {
      if ((AMSShootIndex >= 0) && (AMSShootIndex < AMSShoots.Count)) {
        return 0.0f - (2.0f + AMSShoots[AMSShootIndex].t);
      }
      return Intercepted ? -1.0f : 0.0f;
    }
    public AMSShoot getAMSShoot() {
      if ((AMSShootIndex >= 0) && (AMSShootIndex < AMSShoots.Count)) {
        return AMSShoots[AMSShootIndex];
      }
      return null;
    }
    public void nextAMSShoot() {
      ++AMSShootIndex;
    }
  }
  public class FragInfo {
    public bool separated;
    public bool isFragPallet;
    public int fragMainHitIndex;
    public int fragStartHitIndex;
    public int fragsCount;
    public FragInfo() {
      separated = false;
      isFragPallet = false;
      fragMainHitIndex = -1;
      fragStartHitIndex = -1;
      fragsCount = 0;
    }
  }
  public class AdvWeaponHitInfoRec {
    public AdvWeaponHitInfo parent;
    public int hitLocation;
    public Vector3 hitPosition;
    public Vector3 startPosition;
    public float Damage;
    public float APDamage;
    public float Heat;
    public float Stability;
    public float EffectsMod;
    public GameObject trajectoryObject;
    public CurvySpline trajectorySpline;
    public Vector3[] trajectory;
    public int hitIndex;
    public ICombatant target;
    public float projectileSpeed;
    public bool isAOE { get; set; }
    public bool isAOEproc { get; set; }
    public int AOEKey;
    public SplineGenerationInfo trajectoryInfo;
    public InterceptableInfo interceptInfo;
    public FragInfo fragInfo;
    public bool isStray;
    public static CombatGameState Combat = null;
    //public CombatGameState Combat { get { return AdvWeaponHitInfoRec.FCombat; } set { AdvWeaponHitInfoRec.FCombat = value; } }
    public bool isMiss { get { return (this.hitLocation == 0) || (this.hitLocation == 65536); } }
    public bool isHit { get { return (this.hitLocation != 0) && (this.hitLocation != 65536); } }
    public void ClearTrajectory() {
      if (trajectorySpline != null) { GameObject.Destroy(trajectorySpline); trajectorySpline = null; }
      if (trajectoryObject != null) { GameObject.Destroy(trajectoryObject); trajectoryObject = null; }
    }
    public AdvWeaponHitInfoRec(AdvWeaponHitInfo parent) {
      this.parent = parent;
      EffectsMod = 1f;
      hitLocation = 0;
      hitPosition = Vector3.zero;
      startPosition = Vector3.zero;
      Damage = 0f;
      APDamage = 0f;
      Heat = 0f;
      Stability = 0f;
      trajectoryObject = new GameObject("trajectoryObject");
      trajectorySpline = trajectoryObject.AddComponent<CurvySpline>();
      trajectory = new Vector3[0] { };
      hitIndex = -1;
      target = null;
      projectileSpeed = 0f;
      interceptInfo = new InterceptableInfo();
      fragInfo = new FragInfo();
      isAOE = false;
      AOEKey = -1;
      isStray = false;
      isAOEproc = false;
      //weaponEffect = null;
      trajectoryInfo = new SplineGenerationInfo();
    }
    public void GenerateTrajectory() {
      if (this.parent.weapon.weaponRep != null) {
        try {
          if (this.parent.weapon.weaponRep.vfxTransforms != null) {
            if (this.parent.weapon.weaponRep.vfxTransforms.Length != 0) {
              this.GenerateTrajectory(this.parent.weapon.weaponRep.vfxTransforms[hitIndex % this.parent.weapon.weaponRep.vfxTransforms.Length].position);
            } else {
              Log.M.TWL(0, "WARNING! vfxTransforms has 0 elements. Fixing", true);
              this.parent.weapon.weaponRep.vfxTransforms = new Transform[1] { this.parent.weapon.weaponRep.gameObject.transform };
            }
          } else {
            Log.M.TWL(0, "WARNING! vfxTransforms is null. Fixing", true);
            this.parent.weapon.weaponRep.vfxTransforms = new Transform[1] { this.parent.weapon.weaponRep.gameObject.transform };
          }
        } catch (Exception e) {
          this.parent.weapon.weaponRep.vfxTransforms = new Transform[1] { this.parent.weapon.weaponRep.gameObject.transform };
          Log.M.TWL(0, e.ToString(), true);
        }
      } else {
        this.GenerateTrajectory(this.parent.weapon.parent.CurrentPosition);
      }
    }
    public void GenerateTrajectory(Vector3 startPos) {
      this.startPosition = startPos;
      trajectory = this.trajectoryInfo.generateTrajectory(this.hitLocation, this.startPosition, this.hitPosition);
      this.trajectorySpline.Interpolation = CurvyInterpolation.Bezier;
      this.trajectorySpline.Clear();
      this.trajectorySpline.Closed = false;
      this.trajectorySpline.Add(this.trajectory);
      this.trajectorySpline.Refresh();
    }
  }
  public class AdvCritLocationInfo {
    public int armorLocation;
    public float armorOnHit;
    public float structureOnHit;
    public AdvCritLocationInfo(int aLoc, AbstractActor unit) {
      this.armorLocation = aLoc;
      this.armorOnHit = unit.ArmorForLocation(aLoc);
      this.structureOnHit = unit.StructureForLocation(aLoc);
    }
  }
  public class EffectsLocaltion {
    public int location { get; set; }
    public float effectsMod { get; set; }
    public bool isAOE { get; set; }
    public EffectsLocaltion(int l, float chance, bool aoe) {
      location = l;
      effectsMod = chance;
      isAOE = aoe;
    }
  }
  public class AdvWeaponResolveInfo {
    public float Heat;
    public float Stability;
    public int hitsCount;
    public List<AdvCritLocationInfo> Crits;
    public List<EffectsLocaltion> hitLocations;
    public float cumulativeDamage;
    public AdvWeaponHitInfo parent;
    public void AddCrit(int aLoc, ICombatant unit) {
      AbstractActor actor = unit as AbstractActor;
      if (actor == null) { return; }
      Crits.Add(new AdvCritLocationInfo(aLoc, actor));
    }
    public void AddHit(int location, float chance, bool aoe) { ++hitsCount; hitLocations.Add(new EffectsLocaltion(location, chance, aoe)); }
    public void AddHeat(float val) {
      this.Heat += val;
    }
    public void AddInstability(float val) {
      this.Stability += val;
    }
    public AdvWeaponResolveInfo(AdvWeaponHitInfo parent) {
      this.parent = parent;
      Heat = 0f;
      Stability = 0f;
      hitsCount = 0;
      Crits = new List<AdvCritLocationInfo>();
      hitLocations = new List<EffectsLocaltion>();
    }
  }
  public static partial class AdvWeaponHitInfoHelper {
    public static bool isAdvanced(this WeaponHitInfo hitInfo) {
      if (AdvWeaponHitInfo.advancedWeaponHitInfo.ContainsKey(hitInfo.attackSequenceId) == false) { return false; }
      if (AdvWeaponHitInfo.advancedWeaponHitInfo[hitInfo.attackSequenceId].ContainsKey(hitInfo.attackGroupIndex) == false) { return false; }
      if (AdvWeaponHitInfo.advancedWeaponHitInfo[hitInfo.attackSequenceId][hitInfo.attackGroupIndex].ContainsKey(hitInfo.attackWeaponIndex) == false) { return false; }
      return true;
    }
    public static AdvWeaponHitInfo advInfo(this WeaponHitInfo hitInfo) {
      if (hitInfo.isAdvanced() == false) { return null; };
      return AdvWeaponHitInfo.advancedWeaponHitInfo[hitInfo.attackSequenceId][hitInfo.attackGroupIndex][hitInfo.attackWeaponIndex];
    }
    public static AdvWeaponHitInfoRec advRec(this WeaponHitInfo hitInfo, int hitIndex) {
      if (hitInfo.isAdvanced() == false) { return null; };
      if (hitIndex < 0) { return null; }
      AdvWeaponHitInfo advInfo = hitInfo.advInfo();
      if (hitIndex >= advInfo.hits.Count) { return null; };
      return advInfo.hits[hitIndex];
    }
    public static List<AdvWeaponHitInfoRec> Interceptables(this AttackDirector.AttackSequence sequence) {
      Log.LogWrite("AttackSequence.Interceptables:" + sequence.id + "\n");
      List<AdvWeaponHitInfoRec> result = new List<AdvWeaponHitInfoRec>();
      if (AdvWeaponHitInfo.advancedWeaponHitInfo.ContainsKey(sequence.id) == false) {
        Log.LogWrite(" not adv info for sequence\n");
        return result;
      }
      foreach (var group in AdvWeaponHitInfo.advancedWeaponHitInfo[sequence.id]) {
        foreach (var weapon in group.Value) {
          foreach (var hit in weapon.Value.hits) {
            if (hit.interceptInfo.AMSImunne == false) { result.Add(hit); };
          }
        }
      }
      return result;
    }
    public static void ResetUsedRandomCache(this AttackDirector.AttackSequence sequence, int grpIdx, int weaponIdx, int randomUsed = 0, int varianceUsed = 0) {
      if (grpIdx < 0) { return; }
      if (weaponIdx < 0) { return; }
      try {
        int[][] randomCacheValuesUsed = (int[][])typeof(AttackDirector.AttackSequence).GetField("randomCacheValuesUsed", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(sequence);
        if (grpIdx >= randomCacheValuesUsed.Length) { return; }
        if (weaponIdx >= randomCacheValuesUsed[grpIdx].Length) { return; }
        randomCacheValuesUsed[grpIdx][weaponIdx] = randomUsed;
        typeof(AttackDirector.AttackSequence).GetField("randomCacheValuesUsed", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(sequence, randomCacheValuesUsed);

        int[][] varianceCacheValuesUsed = (int[][])typeof(AttackDirector.AttackSequence).GetField("varianceCacheValuesUsed", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(sequence);
        if (grpIdx >= varianceCacheValuesUsed.Length) { return; }
        if (weaponIdx >= varianceCacheValuesUsed[grpIdx].Length) { return; }
        varianceCacheValuesUsed[grpIdx][weaponIdx] = varianceUsed;
        typeof(AttackDirector.AttackSequence).GetField("varianceCacheValuesUsed", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(sequence, varianceCacheValuesUsed);
      } catch (Exception e) {
        Log.LogWrite(e.ToString() + "\n", true);
      }
    }
    public static void GetUsedRandomCache(this AttackDirector.AttackSequence sequence, int grpIdx, int weaponIdx, out int randomUsed, out int varianceUsed) {
      randomUsed = 0; varianceUsed = 0;
      if (grpIdx < 0) { return; }
      if (weaponIdx < 0) { return; }
      try {
        int[][] randomCacheValuesUsed = (int[][])typeof(AttackDirector.AttackSequence).GetField("randomCacheValuesUsed", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(sequence);
        if (grpIdx >= randomCacheValuesUsed.Length) { return; }
        if (weaponIdx >= randomCacheValuesUsed[grpIdx].Length) { return; }
        randomUsed = randomCacheValuesUsed[grpIdx][weaponIdx];
        int[][] varianceCacheValuesUsed = (int[][])typeof(AttackDirector.AttackSequence).GetField("varianceCacheValuesUsed", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(sequence);
        if (grpIdx >= varianceCacheValuesUsed.Length) { return; }
        if (weaponIdx >= varianceCacheValuesUsed[grpIdx].Length) { return; }
        varianceUsed = varianceCacheValuesUsed[grpIdx][weaponIdx];
      } catch (Exception e) {
        Log.LogWrite(e.ToString() + "\n", true);
      }
    }
    public static WeaponHitInfo CreateWeaponHitInfo(this AttackDirector.AttackSequence sequence, ICombatant target, Weapon weapon, int numberOfShots, int groupIdx, int weaponIdx) {
      WeaponHitInfo hitInfo = new WeaponHitInfo();
      hitInfo.attackerId = sequence.attacker.GUID;
      hitInfo.targetId = target.GUID;
      hitInfo.numberOfShots = numberOfShots;
      hitInfo.stackItemUID = sequence.stackItemUID;
      hitInfo.attackSequenceId = sequence.id;
      hitInfo.attackGroupIndex = groupIdx;
      hitInfo.attackWeaponIndex = weaponIdx;
      hitInfo.toHitRolls = new float[numberOfShots];
      hitInfo.locationRolls = new float[numberOfShots];
      hitInfo.dodgeRolls = new float[numberOfShots];
      hitInfo.dodgeSuccesses = new bool[numberOfShots];
      hitInfo.hitLocations = new int[numberOfShots];
      hitInfo.hitPositions = new Vector3[numberOfShots];
      hitInfo.hitVariance = new int[numberOfShots];
      hitInfo.hitQualities = new AttackImpactQuality[numberOfShots];
      hitInfo.secondaryTargetIds = new string[numberOfShots];
      hitInfo.secondaryHitLocations = new int[numberOfShots];
      hitInfo.attackDirections = new AttackDirection[numberOfShots];
      return hitInfo;
    }
    public static AdvWeaponHitInfo initGenericAdvInfo(this WeaponHitInfo hitInfo, float hitChance, AttackDirector.AttackSequence sequence, CombatGameState combat) {
      Log.LogWrite("initGenericAdvInfo: " + hitInfo.numberOfShots + "\n");
      if (hitInfo.isAdvanced() == true) {
        Log.LogWrite(" already exists\n");
        return hitInfo.advInfo();
      }
      AdvWeaponHitInfo result = new AdvWeaponHitInfo(combat, hitInfo, hitChance);
      AdvWeaponHitInfoRec.Combat = combat;
      //AttackDirector.AttackSequence sequence = combat.AttackDirector.GetAttackSequence(hitInfo.attackSequenceId);
      if (sequence == null) {
        Log.LogWrite(" sequence is null\n");
        return null;
      };
      Weapon weapon = sequence.GetWeapon(hitInfo.attackGroupIndex, hitInfo.attackWeaponIndex);
      if (weapon == null) {
        Log.LogWrite(" weapon is null\n");
        return null;
      }
      WeaponEffect effect = weapon.getWeaponEffect();
      if (effect == null) {
        Log.LogWrite(" effect is null\n");
        //return null;
      }
      result.weapon = weapon;
      //result.weaponEffect = effect;
      result.Sequence = sequence;
      MissileLauncherEffect launcher = effect as MissileLauncherEffect;
      MultiShotMissileLauncherEffect msLauncher = effect as MultiShotMissileLauncherEffect;
      MultiShotBallisticEffect msBallistic = effect as MultiShotBallisticEffect;
      LaserEffect laser = effect as LaserEffect;
      MultiShotLaserEffect msLaser = effect as MultiShotLaserEffect;
      FlamerEffect flamer = effect as FlamerEffect;
      float projectileSpeedMult = weapon.ProjectileSpeedMultiplier();
      bool damagePerPallet = weapon.DamagePerPallet();
      bool damagePerNotDiv = weapon.DamageNotDivided();
      for (int hitIndex = 0; hitIndex < hitInfo.numberOfShots; ++hitIndex) {
        AdvWeaponHitInfoRec hit = new AdvWeaponHitInfoRec(result);
        Log.LogWrite(" hit: " + hitIndex + "\n");
        hit.hitIndex = hitIndex;
        hit.hitPosition = hitInfo.hitPositions[hitIndex];
        hit.Damage = weapon.DamagePerShotAdjusted(weapon.parent.occupiedDesignMask);
#if BT1_8
        hit.APDamage = weapon.StructureDamagePerShotAdjusted(weapon.parent.occupiedDesignMask);
#else
        hit.APDamage = weapon.APDamage() * (hit.Damage / weapon.DamagePerShot);
#endif
        hit.Heat = weapon.HeatDamagePerShotAdjusted(hitInfo.hitQualities[hitIndex]);
        hit.Stability = weapon.Instability();
        if ((damagePerPallet == true) && (damagePerNotDiv == false)) {
          hit.Damage /= (float)weapon.ProjectilesPerShot;
          hit.Heat /= (float)weapon.ProjectilesPerShot;
          hit.Stability /= (float)weapon.ProjectilesPerShot;
          hit.APDamage /= (float)weapon.ProjectilesPerShot;
        }
        Log.M.WL(2, "dmg:" + hit.Damage + " heat:" + hit.Heat + " stab:" + hit.Stability + " ap:" + hit.APDamage);
        hit.hitLocation = hitInfo.hitLocations[hitIndex];
        hit.target = sequence.chosenTarget;
        if (string.IsNullOrEmpty(hitInfo.secondaryTargetIds[hitIndex]) == false) {
          ICombatant target = combat.FindCombatantByGUID(hitInfo.secondaryTargetIds[hitIndex]);
          if (target != null) {
            hit.hitLocation = hitInfo.secondaryHitLocations[hitIndex];
            hit.target = target;
            hit.isStray = true;
          }
        }
        if (launcher != null) {
          hit.trajectoryInfo.CurveFrequency = launcher.missileCurveFrequency;
          hit.trajectoryInfo.CurveStrength = launcher.missileCurveStrength;
          hit.trajectoryInfo.isSRM = launcher.isSRM;
          if (weapon.Unguided()) {
            hit.trajectoryInfo.type = TrajectoryType.Unguided;
          } else {
            if (sequence.indirectFire || CustomAmmoCategories.AlwaysIndirectVisuals(weapon)) {
              hit.trajectoryInfo.type = TrajectoryType.Indirect;
            } else {
              hit.trajectoryInfo.type = TrajectoryType.Direct;
            }
          }
        } else
        if (msLauncher != null) {
          hit.trajectoryInfo.CurveFrequency = launcher.missileCurveFrequency;
          hit.trajectoryInfo.CurveStrength = launcher.missileCurveStrength;
          hit.trajectoryInfo.isSRM = launcher.isSRM;
          if (weapon.Unguided()) {
            hit.trajectoryInfo.type = TrajectoryType.Unguided;
          } else {
            if (sequence.indirectFire || CustomAmmoCategories.AlwaysIndirectVisuals(weapon)) {
              hit.trajectoryInfo.type = TrajectoryType.Indirect;
            } else {
              hit.trajectoryInfo.type = TrajectoryType.Direct;
            }
          }
        } else
        if (msBallistic != null) {
          if (sequence.indirectFire || CustomAmmoCategories.AlwaysIndirectVisuals(weapon)) {
            hit.trajectoryInfo.type = TrajectoryType.Indirect;
          } else {
            hit.trajectoryInfo.type = TrajectoryType.Unguided;
          }
        }
        if ((launcher != null) || (msLauncher != null)) {
          hit.interceptInfo.AMSImunne = weapon.AMSImmune();
          hit.interceptInfo.missileHealth = weapon.MissileHealth();
          hit.interceptInfo.AMSHitChance = weapon.AMSHitChance();
          Log.LogWrite(" missile launcher. AMS imunne:" + hit.interceptInfo.AMSImunne + " health:"+ hit.interceptInfo.missileHealth + "\n");
        } else {
          Log.LogWrite(" not missile launcher\n");
        }
        hit.GenerateTrajectory();
        if (effect == null) {
          hit.projectileSpeed = 100f;
        } else
        if ((laser == null) && (msLaser == null) && (flamer == null)) {
          float distance = hit.trajectorySpline.Length;
          float duration = 1f;
          if (effect.projectileSpeed > CustomAmmoCategories.Epsilon) {
            duration = distance / effect.projectileSpeed;
          }
          if (duration > 4f) { duration = 4f; };
          hit.projectileSpeed = distance / duration;
          hit.projectileSpeed = Random.Range(0.9f, 1.1f) * hit.projectileSpeed;
        } else {
          hit.projectileSpeed = effect.projectileSpeed;
        }
        hit.projectileSpeed *= projectileSpeedMult;
        result.hits.Add(hit);
      }
      if (AdvWeaponHitInfo.advancedWeaponHitInfo.ContainsKey(hitInfo.attackSequenceId) == false) { AdvWeaponHitInfo.advancedWeaponHitInfo.Add(hitInfo.attackSequenceId, new Dictionary<int, Dictionary<int, AdvWeaponHitInfo>>()); }
      if (AdvWeaponHitInfo.advancedWeaponHitInfo[hitInfo.attackSequenceId].ContainsKey(hitInfo.attackGroupIndex) == false) { AdvWeaponHitInfo.advancedWeaponHitInfo[hitInfo.attackSequenceId].Add(hitInfo.attackGroupIndex, new Dictionary<int, AdvWeaponHitInfo>()); }
      AdvWeaponHitInfo.advancedWeaponHitInfo[hitInfo.attackSequenceId][hitInfo.attackGroupIndex].Add(hitInfo.attackWeaponIndex, result);
      Log.LogWrite("initGenericAdvInfo: " + hitInfo.attackSequenceId + "\n");
      return result;
    }
  }
  public class AdvWeaponHitInfo {
    public static Dictionary<int, Dictionary<int, Dictionary<int, AdvWeaponHitInfo>>> advancedWeaponHitInfo = new Dictionary<int, Dictionary<int, Dictionary<int, AdvWeaponHitInfo>>>();
    private static CombatGameState FCombat = null;
    public CombatGameState Combat { get { return AdvWeaponHitInfo.FCombat; } }
    public AttackDirector Director { get { return AdvWeaponHitInfo.FCombat.AttackDirector; } }
    public AttackDirector.AttackSequence Sequence;
    public Dictionary<ICombatant, AdvWeaponResolveInfo> resolveInfo;
    public List<AdvWeaponHitInfoRec> hits;
    public int attackSequenceId;
    public int groupIdx;
    public int weaponIdx;
    public Weapon weapon;
    //public WeaponEffect weaponEffect { get; set; };
    public float hitChance;
    public AdvWeaponResolveInfo resolve(ICombatant target) {
      if (resolveInfo.ContainsKey(target) == false) { resolveInfo.Add(target, new AdvWeaponResolveInfo(this)); }
      return resolveInfo[target];
    }
    public static void Clear(int sequenceId) {
      if (AdvWeaponHitInfo.advancedWeaponHitInfo.ContainsKey(sequenceId) == false) { return; };
      Dictionary<int, Dictionary<int, AdvWeaponHitInfo>> seqAdvInfo = AdvWeaponHitInfo.advancedWeaponHitInfo[sequenceId];
      AdvWeaponHitInfo.advancedWeaponHitInfo.Remove(sequenceId);
      List<AdvWeaponHitInfo> advInfos = new List<AdvWeaponHitInfo>();
      foreach (var group in seqAdvInfo) { foreach (var wpn in group.Value) { advInfos.Add(wpn.Value); } };
      foreach (AdvWeaponHitInfo advInfo in advInfos) {
        foreach (AdvWeaponHitInfoRec advRec in advInfo.hits) {
          advRec.ClearTrajectory();
        }
      }
    }
    public AdvWeaponHitInfo(CombatGameState combat, WeaponHitInfo hitInfo, float hitChance) {
      this.hitChance = hitChance;
      AdvWeaponHitInfo.FCombat = combat;
      this.Sequence = null;
      this.hits = new List<AdvWeaponHitInfoRec>();
      this.attackSequenceId = hitInfo.attackSequenceId;
      this.groupIdx = hitInfo.attackGroupIndex;
      this.weaponIdx = hitInfo.attackWeaponIndex;
      this.resolveInfo = new Dictionary<ICombatant, AdvWeaponResolveInfo>();
    }
    public void AppendAoEHit(int primeIndex, float fulldamage, float Damage, float Heat, float Stability, ICombatant target, Vector3 position, int location) {
      Log.LogWrite("AdvInfo.AppendFrags:" + primeIndex + "\n");
      AdvWeaponHitInfoRec hit = new AdvWeaponHitInfoRec(this);
      int hitIndex = this.hits.Count;
      Log.LogWrite(" hit: " + hitIndex + "\n");
      hit.hitIndex = hitIndex;
      hit.hitPosition = position;
      hit.hitLocation = location;
      hit.Damage = Damage;
      hit.APDamage = 0f;
      hit.Heat = Heat;
      hit.Stability = Stability;
      hit.target = target;
      hit.projectileSpeed = 0f;
      hit.isAOE = true;
      hit.AOEKey = primeIndex;
      if (this.weapon.AOEEffectsFalloff()) {
        hit.EffectsMod = Damage / fulldamage;
      } else {
        hit.EffectsMod = 1f;
      }
      this.hits[primeIndex].isAOEproc = true;
      this.hits.Add(hit);
    }
    public void AppendFrags(int fragHitIndex, WeaponHitInfo hitInfo) {
      Log.LogWrite("AdvInfo.AppendFrags:" + hitInfo.numberOfShots + "\n");
      AdvWeaponHitInfoRec advRec = this.hits[fragHitIndex];
      advRec.fragInfo.fragStartHitIndex = this.hits.Count;
      advRec.fragInfo.fragsCount = hitInfo.numberOfShots;
      for (int hitIndex = 0; hitIndex < hitInfo.numberOfShots; ++hitIndex) {
        AdvWeaponHitInfoRec hit = new AdvWeaponHitInfoRec(this);
        Log.LogWrite(" hit: " + hitIndex + "\n");
        hit.hitIndex = hitIndex;
        hit.hitPosition = hitInfo.hitPositions[hitIndex];
        hit.fragInfo.isFragPallet = true;
        hit.fragInfo.fragMainHitIndex = advRec.hitIndex;
        hit.Damage = weapon.DamagePerShotAdjusted(weapon.parent.occupiedDesignMask);
#if BT1_8
        hit.APDamage = weapon.StructureDamagePerShotAdjusted(weapon.parent.occupiedDesignMask);
#else
        hit.APDamage = weapon.APDamage();
#endif
        hit.Damage /= (float)weapon.ProjectilesPerShot;
        hit.APDamage /= (float)weapon.ProjectilesPerShot;
        hit.Heat = weapon.HeatDamagePerShotAdjusted(hitInfo.hitQualities[hitIndex]) / (float)hitInfo.numberOfShots;
        hit.Stability = weapon.Instability() / (float)hitInfo.numberOfShots; ;
        hit.hitLocation = hitInfo.hitLocations[hitIndex];
        hit.target = null;
        if (string.IsNullOrEmpty(hitInfo.secondaryTargetIds[hitIndex]) == false) {
          ICombatant target = Combat.FindCombatantByGUID(hitInfo.secondaryTargetIds[hitIndex]);
          if (target != null) {
            hit.hitLocation = hitInfo.secondaryHitLocations[hitIndex];
            hit.target = target;
          }
        }
        if (hit.target == null) {
          ICombatant target = Combat.FindCombatantByGUID(hitInfo.targetId);
          if (target != null) {
            hit.target = target;
          }
        }
        if (hit.target == null) {
          hit.target = this.Sequence.chosenTarget;
        }
        hit.trajectoryInfo.type = TrajectoryType.Unguided;
        hit.GenerateTrajectory(advRec.hitPosition);
        hit.projectileSpeed = 0f;
        this.hits.Add(hit);
      }
    }
  }
}