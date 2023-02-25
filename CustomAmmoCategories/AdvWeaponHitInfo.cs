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
using CustomAmmoCategoriesLog;
using CustomAmmoCategoriesPatches;
using FluffyUnderware.Curvy;
using HarmonyLib;
using Localize;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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
    public int UID { get; private set; }
    public AdvWeaponHitInfo parent;
    private int f_hitLocation;
    public int hitLocation { get { return f_hitLocation; }
      set {
        f_hitLocation = value;
        if (f_hitLocation == 0) { hitLocationStr = "Air"; return; }
        if (f_hitLocation == 65536) { hitLocationStr = "Ground"; return; }
        Mech mech = target as Mech;
        Vehicle vehicle = target as Vehicle;
        Turret turret = target as Turret;
        if (mech != null) { ArmorLocation loc = (ArmorLocation)f_hitLocation; hitLocationStr = loc.ToString(); } else
        if (vehicle != null) { VehicleChassisLocations loc = (VehicleChassisLocations)f_hitLocation; hitLocationStr = loc.ToString(); } else
        if (turret != null) { BuildingLocation loc = (BuildingLocation)f_hitLocation; hitLocationStr = loc.ToString(); } else {
          hitLocationStr = "Structure";
        }
      }
    }
    public string hitLocationStr { get; private set; }
    public Vector3 hitPosition;
    public Vector3 startPosition;
    public AttackImpactQuality impactQuality { get; set; }
    public AttackDirection attackDirection { get; set; }
    public float Damage;
    public float APDamage;
    public float Heat;
    public float Stability;
    public float EffectsMod;
    private WeaponHitInfo? f_fakeHitInfo;
    public WeaponHitInfo? fakeHitInfo {
      get {
        if (f_fakeHitInfo.HasValue) { return f_fakeHitInfo; }
        WeaponHitInfo value = new WeaponHitInfo();
        value.attackDirections = new AttackDirection[1] { AttackDirection.None };
        value.stackItemUID = parent.Sequence.stackItemUID;
        value.attackSequenceId = parent.Sequence.id;
        value.attackGroupIndex = parent.groupIdx;
        value.attackWeaponIndex = parent.weaponIdx;
        value.attackerId = parent.Sequence.attacker.GUID; ;
        value.targetId = target.GUID;
        value.numberOfShots = 1;
        value.secondaryTargetIds = new string[] { null };
        value.secondaryHitLocations = new int[] { 0 };
        value.toHitRolls = new float[] { this.hitRoll };
        value.locationRolls = new float[] { locationRoll };
        value.dodgeRolls = new float[] { dodgeRoll };
        value.dodgeSuccesses = new bool[] { false };
        value.hitLocations = new int[] { this.hitLocation };
        value.hitVariance = new int[] { 0 };
        value.hitQualities = new AttackImpactQuality[] { this.impactQuality };
        value.attackDirections = new AttackDirection[] { attackDirection };
        value.hitPositions = new Vector3[] { this.hitPosition };
        f_fakeHitInfo = value;
        return f_fakeHitInfo;
      }
    }
    public bool isImplemented { get; set; }
    public bool isTargetResitanceApplied { get; set; }
    public float hitRoll { get; set; }
    public float locationRoll { get; set; }
    public float dodgeRoll { get; set; }
    public float correctedRoll { get; set; }
    public GameObject trajectoryObject;
    public CurvySpline trajectorySpline;
    public Vector3[] trajectory;
    public int hitIndex;
    public ICombatant target;
    public float projectileSpeed;
    public bool isAOE { get; set; }
    public bool isFragMain { get { return fragInfo.separated && (fragInfo.isFragPallet == false); } }
    public bool isAOEproc { get; set; }
    public int AOEKey;
    public SplineGenerationInfo trajectoryInfo;
    public InterceptableInfo interceptInfo;
    public FragInfo fragInfo;
    public bool isStray;
    public static CombatGameState Combat = null;
    public AttackSequenceImpactMessage impactMessage { get; set; }
    public AdvHitMessage advHitMessage { get; set; }
    public void setVisualsState() { if (advHitMessage != null) { advHitMessage.isVisuals = true; } }
    public void setApplyState() { if (advHitMessage != null) { advHitMessage.isApplied = true; } }
    //public CombatGameState Combat { get { return AdvWeaponHitInfoRec.FCombat; } set { AdvWeaponHitInfoRec.FCombat = value; } }
    public bool isMiss { get { return (this.hitLocation == 0) || (this.hitLocation == 65536); } }
    public bool isHit { get { return (this.hitLocation != 0) && (this.hitLocation != 65536); } }
    public void ClearTrajectory() {
      if (trajectorySpline != null) { GameObject.Destroy(trajectorySpline); trajectorySpline = null; }
      if (trajectoryObject != null) { GameObject.Destroy(trajectoryObject); trajectoryObject = null; }
    }
    public bool CanBeImpemented() {
      return true;
    }
    public bool VarianceApplyied { get; private set; }
    public static void PredictDamage(Weapon weapon, ICombatant target, Vector3 attackPos, out float dmg, out float ap, out float heat, out float stability, out int shots) {
      dmg = weapon.DamagePerShotFromPosition(MeleeAttackType.NotSet,attackPos,target);
      ap = weapon.StructureDamagePerShot * (dmg / weapon.DamagePerShot);
      heat = weapon.HeatDamagePerShot;
      stability = weapon.Instability();
      shots = weapon.ShotsWhenFired;
      if (target == null) { return; }
      float realDamage = dmg;
      float rawDamage = dmg;
      float rawAPDamage = ap;
      float rawHeat = heat;
      float rawStability = stability;
      if (realDamage >= 1.0f) {
        if (weapon.DistantVariance() > CustomAmmoCategories.Epsilon) {
          if (weapon.DistantVarianceReversed() == false) {
            realDamage = CustomAmmoCategories.WeaponDamageDistance(attackPos, target, weapon, realDamage, rawDamage, false);
          } else {
            realDamage = CustomAmmoCategories.WeaponDamageRevDistance(attackPos, target, weapon, realDamage, rawDamage, false);
          }
        }
        if (realDamage >= 1.0f) {
          realDamage = WeaponRealizer.Calculator.ApplyAllDamageModifiers(attackPos, target, weapon, realDamage, false, false);
        }
      }
      if (float.IsNaN(realDamage)) { realDamage = 0.1f; }
      if (float.IsInfinity(realDamage)) { realDamage = 0.1f; }
      if (realDamage < CustomAmmoCategories.Epsilon) { realDamage = 0.1f; }
      if (weapon.isDamageVariation()) {
        dmg = realDamage;
        rawAPDamage *= rawDamage > CustomAmmoCategories.Epsilon ? (realDamage / rawDamage) : 0f;
        ap = rawAPDamage;
      }
      if (weapon.isHeatVariation()) {
        rawHeat *= rawDamage > CustomAmmoCategories.Epsilon ? (realDamage / rawDamage) : 0f;
      }
      if (weapon.isStabilityVariation()) {
        rawStability *= rawDamage > CustomAmmoCategories.Epsilon ? (realDamage / rawDamage) : 0f;
      }
      heat = rawHeat;
      stability = rawStability;
      if (target.isHasStability() == false) { stability = 0f; }
      if ((target.isHasHeat() == false) && (rawHeat >= 0.5f)) {
        float heatAsNormal = target.HeatDamage(rawHeat);
        dmg += heatAsNormal;
        heat = 0f;
      }
      heat *= target.IncomingHeatMult();
      stability *= target.IncomingStabilityMult();
      ap *= target.APDamageMult();
      if (target is AbstractActor actor) {
        stability *= actor.StatCollection.GetValue<float>("ReceivedInstabilityMultiplier") * actor.EntrenchedMultiplier;
        LineOfFireLevel lineOfFireLevel = weapon.parent.VisibilityCache.VisibilityToTarget((ICombatant)actor).LineOfFireLevel;
        float aDamage = actor.GetAdjustedDamage(dmg, weapon.WeaponCategoryValue, actor.occupiedDesignMask, lineOfFireLevel, true);
        dmg = actor.GetAdjustedDamageForMelee(aDamage, weapon.WeaponCategoryValue);
        float aAPDamage = actor.GetAdjustedDamage(ap, weapon.WeaponCategoryValue, actor.occupiedDesignMask, lineOfFireLevel, true);
        ap = actor.GetAdjustedDamageForMelee(aAPDamage, weapon.WeaponCategoryValue);
      }
      if (target.isAPProtected()) { ap = 0f; }
    }
    public void ApplyVariance(AttackSequenceImpactMessage iMessage) {
      if (iMessage != null) this.impactMessage = iMessage;
      if (VarianceApplyied) { return; }
      VarianceApplyied = true;
      try {
        Log.M.TWL(0, "Damage variance: " + hitIndex + "/" + parent.hits.Count +" UID:"+this.UID);
        Log.M.WL(1, "attacker = " + parent.Sequence.attacker.DisplayName);
        Log.M.WL(1, "target = " + new Text(target.DisplayName).ToString());
        Log.M.WL(1, "location = (" + hitLocation + ")" + hitLocationStr);
        Log.M.WL(1, "weapon = " + new Text(parent.weapon.UIName).ToString());
        Log.M.WL(1, "damage = " + this.Damage + "/" + this.APDamage);
        Log.M.WL(1, "heat = " + this.Heat);
        Log.M.WL(1, "stability = " + this.Stability);
        Log.M.WL(1, "isAOE = " + (isAOE));
        Log.M.WL(1, "isFragMain = " + (isFragMain));
        if (isAOE == true) {
          Log.M.WL(1, "this is AOE - no variance");
          return;
        }
        if (this.interceptInfo.Intercepted) {
          Log.M.WL(1, "intercepted");
          return;
        }
        if ((hitLocation == 0) || (hitLocation == 65536)) {
          Log.M.WL(1, "no hit");
          return;
        }
        DamageModifiers mods = this.parent.weapon.GetDamageModifiers(this.parent.Sequence.attackPosition, this.target, this.parent.Sequence.IsBreachingShot?TripleBoolean.True:TripleBoolean.False);
        string description = string.Empty;
        mods.Calculate(this.hitLocation, ref this.Damage, ref this.APDamage, ref this.Heat, ref this.Stability, ref description, true, false);
        Log.M.TWL(0, description);
        this.target.AddComulativeDamage(this.hitLocation, this.Damage);
        Log.LogWrite("  real damage = " + this.Damage + "\n");
        Log.LogWrite("  real APdamage = " + this.APDamage + "\n");
        Log.LogWrite("  real heat = " + this.Heat + "\n");
        Log.LogWrite("  real stability = " + this.Stability + "\n");
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
    }
    public bool ImpactPlayed { get; private set; }
    public void PlayImpact() {
      if (ImpactPlayed) { return; }
      ImpactPlayed = true;
      try {
        Log.M.TWL(0, "Play impact: " + parent.weapon.defId + " " + this.hitIndex + "/"+this.parent.hits.Count);
        if (isAOE == true) {
          Log.M.WL(1, "this is AOE impact processing for them");
          return;
        }
        if (isFragMain) {
          if (fragInfo.separated) {
            Log.M.WL(1, "frag grenade separated. No impact processing");
            this.hitLocation = 0;
            return;
          }
        }
        if (interceptInfo.Intercepted) {
          Log.M.WL(1, "Projectile intercepted. No additional impact. No minefield");
          this.hitLocation = 0;
          return;
        };
        if (this.hitLocation == 0) {
          this.hitLocation = 65536;
          Log.M.WL(1, "Tie location to ground");
        }
        parent.weapon.SpawnAdditionalImpactEffect(hitPosition);
        if (hitLocation == 65536) {
          DynamicMapHelper.applyMineField(parent.weapon, hitPosition);
          DynamicMapHelper.applyImpactBurn(parent.weapon, hitPosition);
          DynamicMapHelper.applyImpactTempMask(parent.weapon, hitPosition);
          DynamicMapHelper.applyCleanMinefield(parent.weapon, hitPosition);
          parent.weapon.CreateDifferedEffect(hitPosition);
        } else
        if (hitLocation != 0) {
          if (parent.weapon.FireOnSuccessHit()) {
            DynamicMapHelper.applyImpactBurn(parent.weapon, target.CurrentPosition);
            DynamicMapHelper.applyImpactTempMask(parent.weapon, target.CurrentPosition);
            DynamicMapHelper.applyCleanMinefield(parent.weapon, target.CurrentPosition);
          }
          parent.weapon.CreateDifferedEffect(target);
        }
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
    }
    public void Apply() {
      if (this.isHit) {
        if (this.isImplemented == true) { return; }
        if (this.impactMessage == null) { return; }
        Log.M.TWL(0, $"Applying damage {this.parent.weapon.defId} to {this.target.DisplayName} UID:{this.UID} isAPProtected:{this.target.isAPProtected()}/{this.target.isAPProtected(this.hitLocation)}");
        this.isImplemented = true;
        this.setApplyState();
        //this.ApplyTargetResistance();
        float damage = this.Damage;
        float apdmg = this.APDamage;
        float locArmor = this.target.ArmorForLocation(this.hitLocation);
        float locStructure = this.target.StructureForLocation(this.hitLocation);
        this.parent.Sequence.FlagAttackDidDamage(this.target.GUID);
        this.parent.Sequence.attackCompletelyMissed(false);
        this.parent.Sequence.cumulativeDamage += (damage + apdmg);
        this.parent.resolve(this.target).cumulativeDamage += (damage + apdmg);
        float critAPchance = this.parent.weapon.isAPCrit() ? this.parent.weapon.APCriticalChanceMultiplier() : float.NaN;
        Log.M.WL(1,"crit testing - damage:" + damage + " armor:" + locArmor +" structure:" + locStructure + " ap dmg:" + apdmg + " ap crit chance:" + critAPchance);
        if (damage > locArmor) {
          if ((this.isAOE == false)||(CustomAmmoCategories.Settings.AoECanCrit == true)) {
            Log.LogWrite("  crit to location armor breach:" + this.hitLocation + "\n");
            this.parent.resolve(this.target).AddCrit(this.hitLocation, this.target);
          } else {
            Log.LogWrite("  AoE can't inflict crits due to settings\n");
          }
        } else if (this.isAOE == false) {
          if ((this.target.isAPProtected() == false)&&(this.target.isAPProtected(this.hitLocation) == false)) {
            if ((apdmg > CustomAmmoCategories.Epsilon) || this.parent.weapon.isAPCrit()) {
              Log.LogWrite("  crit to location armor pierce:" + this.hitLocation + "\n");
              this.parent.resolve(this.target).AddCrit(this.hitLocation, this.target);
            } else {
              Log.LogWrite("  ap damage is zero and weapon not cause AP crits:" + this.hitLocation + "\n");
            }
          } else {
            Log.LogWrite("  target is AP crit protected:" + this.hitLocation + "\n");
          }
        } else {
          Log.LogWrite("  AoE can't inflict armor pierce crits:" + this.hitLocation + "\n");
        }

        Log.LogWrite(" resolve damage. arm: "+ locArmor + " dmg:" + damage + " ap:" + apdmg + "\n");
        this.target.TakeWeaponDamage(impactMessage.hitInfo, this.hitLocation, this.parent.weapon, damage, apdmg, hitIndex, DamageType.Weapon);
        //this.parent.resolve(this.target).AddHit(this.hitLocation, this.EffectsMod, this.isAOE);
        this.parent.resolve(this.target).AddHeat(this.Heat);
        Log.LogWrite(" Added heat:" + this.Heat + " overall: " + this.parent.resolve(this.target).Heat + "\n");
        this.parent.resolve(this.target).AddInstability(this.Stability);
        Log.LogWrite(" Added instability:" + this.Stability + " overall: " + this.parent.resolve(this.target).Stability + "\n");
        this.target.HandleDeath(this.parent.Sequence.attacker.GUID);
      }
      this.parent.Sequence.messageCoordinator().MessageComplete((MessageCenterMessage)impactMessage);
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
      isImplemented = false;
      impactMessage = null;
      impactQuality = AttackImpactQuality.Solid;
      attackDirection = AttackDirection.None;
      UID = Random.Range(0, 2147483647);
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
    private int f_armorLocation;
    private int f_structurerLocation;
    public int armorLocation {
      get { return f_armorLocation; }
      set {
        f_armorLocation = value;
        if (f_armorLocation == 0) { armorLocationStr = "none"; return; }
        if (f_armorLocation == 65536) { armorLocationStr = "none"; return; }
        Mech mech = unit as Mech;
        Vehicle vehicle = unit as Vehicle;
        Turret turret = unit as Turret;
        if (mech != null) { ArmorLocation loc = (ArmorLocation)f_armorLocation; armorLocationStr = loc.ToString(); } else
        if (vehicle != null) { VehicleChassisLocations loc = (VehicleChassisLocations)f_armorLocation; armorLocationStr = loc.ToString(); } else
        if (turret != null) { BuildingLocation loc = (BuildingLocation)f_armorLocation; armorLocationStr = loc.ToString(); } else {
          armorLocationStr = "Structure";
        }
      }
    }
    public int structureLocation {
      get { return f_structurerLocation; }
      set {
        f_structurerLocation = value;
        if (f_structurerLocation == 0) { structureLocationStr = "none"; return; }
        if (f_structurerLocation == 65536) { structureLocationStr = "none"; return; }
        Mech mech = unit as Mech;
        Vehicle vehicle = unit as Vehicle;
        Turret turret = unit as Turret;
        if (mech != null) { ChassisLocations loc = (ChassisLocations)f_structurerLocation; structureLocationStr = loc.ToString(); } else
        if (vehicle != null) { VehicleChassisLocations loc = (VehicleChassisLocations)f_structurerLocation; structureLocationStr = loc.ToString(); } else
        if (turret != null) { BuildingLocation loc = (BuildingLocation)f_structurerLocation; structureLocationStr = loc.ToString(); } else {
          structureLocationStr = "Structure";
        }
      }
    }
    public string armorLocationStr { get; private set; }
    public string structureLocationStr { get; private set; }
    public float armorOnHit;
    public float structureOnHit;
    public bool isApplied;
    public bool isSuccess { get; set; } = false;
    public float critChance { get; set; }
    public AbstractActor unit;
    public MechComponent component;
    public AdvCritLocationInfo(int aLoc, AbstractActor unit) {
      this.unit = unit;
      this.armorLocation = aLoc;
      this.structureLocation = 0;
      this.component = null;
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
    public List<string> floatieMessages;
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
      floatieMessages = new List<string>();
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
    public static AdvWeaponHitInfo initGenericAdvInfo(this WeaponHitInfo hitInfo, float hitChance, AttackDirector.AttackSequence sequence, CombatGameState combat, bool indirectFire) {
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
      result.jammInfo = new JammInfo(result.weapon);
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
        hit.hitRoll = hitInfo.toHitRolls[hitIndex];
        hit.correctedRoll = sequence.GetCorrectedRoll(hit.hitRoll, sequence.attacker.team);
        hit.Damage = weapon.DamagePerShot;
        hit.APDamage = weapon.StructureDamagePerShot;
        hit.Heat = weapon.HeatDamagePerShot;
        hit.Stability = weapon.Instability();
        hit.impactQuality = hitInfo.hitQualities[hitIndex];
        hit.attackDirection = hitInfo.attackDirections[hitIndex];
        hit.dodgeRoll = hitInfo.dodgeRolls[hitIndex];
        hit.locationRoll = hitInfo.locationRolls[hitIndex];
        if ((damagePerPallet == true) && (damagePerNotDiv == false)) {
          hit.Damage /= (float)weapon.ProjectilesPerShot;
          hit.Heat /= (float)weapon.ProjectilesPerShot;
          hit.Stability /= (float)weapon.ProjectilesPerShot;
          hit.APDamage /= (float)weapon.ProjectilesPerShot;
        }
        Log.M.WL(2, "dmg:" + hit.Damage + " heat:" + hit.Heat + " stab:" + hit.Stability + " ap:" + hit.APDamage);
        hit.target = sequence.chosenTarget;
        hit.hitLocation = hitInfo.hitLocations[hitIndex];
        if (string.IsNullOrEmpty(hitInfo.secondaryTargetIds[hitIndex]) == false) {
          ICombatant target = combat.FindCombatantByGUID(hitInfo.secondaryTargetIds[hitIndex]);
          if (target != null) {
            hit.target = target;
            hit.hitLocation = hitInfo.secondaryHitLocations[hitIndex];
            hit.isStray = true;
          }
        }
        if (hit.target.isSpawnProtected() || sequence.attacker.isSpawnProtected()) {
          hit.hitLocation = 0;
          hit.isStray = false;
        }
        if (launcher != null) {
          hit.trajectoryInfo.CurveFrequency = launcher.missileCurveFrequency;
          hit.trajectoryInfo.CurveStrength = launcher.missileCurveStrength;
          hit.trajectoryInfo.isSRM = launcher.isSRM;
          if (weapon.Unguided()) {
            hit.trajectoryInfo.type = TrajectoryType.Unguided;
          } else {
            if ((sequence.indirectFire && weapon.IndirectFireCapable()) || CustomAmmoCategories.AlwaysIndirectVisuals(weapon)) {
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
            if ((sequence.indirectFire && weapon.IndirectFireCapable()) || CustomAmmoCategories.AlwaysIndirectVisuals(weapon)) {
              hit.trajectoryInfo.type = TrajectoryType.Indirect;
            } else {
              hit.trajectoryInfo.type = TrajectoryType.Direct;
            }
          }
        } else
        if (msBallistic != null) {
          if ((sequence.indirectFire && weapon.IndirectFireCapable()) || CustomAmmoCategories.AlwaysIndirectVisuals(weapon)) {
            hit.trajectoryInfo.type = TrajectoryType.Indirect;
          } else {
            hit.trajectoryInfo.type = TrajectoryType.Unguided;
          }
        }
        if ((launcher != null) || (msLauncher != null)) {
          hit.interceptInfo.AMSImunne = weapon.AMSImmune();
          hit.interceptInfo.missileHealth = weapon.MissileHealth();
          hit.interceptInfo.AMSHitChance = weapon.AMSHitChance();
          Log.LogWrite(" missile launcher. AMS imunne:" + hit.interceptInfo.AMSImunne + " health:" + hit.interceptInfo.missileHealth + "\n");
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
  public class AdvHitMessage{
    public AdvWeaponHitInfoRec advRec { get; private set; }
    public AdvWeaponHitInfo advInfo { get; private set; }
    public bool isResolve { get; private set; }
    public bool isApplied { get; set; }
    public bool isVisuals { get; set; }
    public AdvHitMessage(AdvWeaponHitInfoRec r, AdvWeaponHitInfo i, bool ir) {
      this.advRec = r;
      this.advInfo = i;
      this.isResolve = ir;
      if (isResolve) {
        advInfo.advHitMessage = this;
      } else {
        advRec.advHitMessage = this;
      }
    }
    public int Index() {
      if (this.advInfo == null) { return -1; }
      if (AdvWeaponHitInfo.advHitMessagesMap.TryGetValue(this.advInfo.attackSequenceId, out var indexes)) {
        if (indexes.TryGetValue(this, out int result)) { return result; }
      }
      return -1;
    }
    public void Apply(bool pending) {
      if (isApplied == false) {
        isApplied = true;
        if (isResolve == false) {
          this.advRec.Apply();
        } else {
          this.advInfo.Apply();
        }
      }
      if(pending) this.TryApplyPending();
    }
    public bool CanBeApplied() {
      if (isApplied) { return false; }
      if (isVisuals == false) { return false; }
      int index = this.Index();
      if (index < 0) { return false; }
      if (AdvWeaponHitInfo.advHitMessages.TryGetValue(this.advInfo.attackSequenceId, out List<AdvHitMessage> list) == false) {
        return false;
      }
      if (index == 0) { return true; }
      return list[index-1].isApplied;
    }
    public void TryApplyPending() {
      if (AdvWeaponHitInfo.advHitMessages.TryGetValue(this.advInfo.attackSequenceId, out List<AdvHitMessage> list) == false) {
        return;
      }
      for(int index = 1; index < list.Count;++index) {
        if (list[index].isApplied) { continue; }
        if (list[index].isVisuals == false) { continue; }
        if (list[index].CanBeApplied()) { list[index].Apply(false); }
      }
    }
  }
  public class JammInfo {
    public float chance { get; set; } = 0f;
    public string description { get; set; } = string.Empty;
    public bool damage { get; private set; }
    public bool destroy { get; private set; }
    public int cooldown { get; private set; }
    public JammInfo(Weapon w) {
      this.chance = w.FlatJammingChance(out string descr);
      this.description = descr;
      this.damage = w.DamageOnJamming();
      this.destroy = w.DestroyOnJamming();
      this.cooldown = w.Cooldown();
    }
  }
  public class AdvWeaponHitInfo {
    public static Dictionary<int, Dictionary<int, Dictionary<int, AdvWeaponHitInfo>>> advancedWeaponHitInfo = new Dictionary<int, Dictionary<int, Dictionary<int, AdvWeaponHitInfo>>>();
    public static Dictionary<int, Dictionary<AdvHitMessage, int>> advHitMessagesMap = new Dictionary<int, Dictionary<AdvHitMessage, int>>();
    public static Dictionary<int, List<AdvHitMessage>> advHitMessages = new Dictionary<int, List<AdvHitMessage>>();
    public JammInfo jammInfo { get; set; } = null;
    private static CombatGameState FCombat = null;
    public CombatGameState Combat { get { return AdvWeaponHitInfo.FCombat; } }
    public AttackDirector Director { get { return AdvWeaponHitInfo.FCombat.AttackDirector; } }
    public AttackDirector.AttackSequence Sequence;
    public Dictionary<ICombatant, AdvWeaponResolveInfo> resolveInfo;
    public List<AdvWeaponHitInfoRec> hits;
    public int attackSequenceId;
    public int groupIdx { get; set; }
    public int weaponIdx { get; set; }
    public Weapon weapon { get; set; }
    public AttackSequenceResolveDamageMessage resolveDamageMessage { get; set; }
    public AdvHitMessage advHitMessage { get; set; }
    public void setApplyState() { if (advHitMessage != null) { advHitMessage.isApplied = true; } }
    public void setVisualState() { if (advHitMessage != null) { advHitMessage.isVisuals = true; } }
    public bool isResolved { get; set; }
    //public WeaponEffect weaponEffect { get; set; };
    public float hitChance;
    private WeaponHitInfo? f_fakeHitInfo;
    public WeaponHitInfo? fakeHitInfo {
      get {
        if (f_fakeHitInfo.HasValue) { return f_fakeHitInfo; }
        WeaponHitInfo value = new WeaponHitInfo();
        value.attackDirections = new AttackDirection[1] { AttackDirection.None };
        value.stackItemUID = Sequence.stackItemUID;
        value.attackSequenceId = Sequence.id;
        value.attackGroupIndex = groupIdx;
        value.attackWeaponIndex = weaponIdx;
        value.attackerId = Sequence.attacker.GUID; ;
        value.targetId = Sequence.chosenTarget.GUID;
        value.numberOfShots = hits.Count;
        value.secondaryTargetIds = new string[value.numberOfShots]; for (int i = 0; i < hits.Count; ++i) { if (hits[i].target.GUID == value.targetId) { value.secondaryTargetIds[i] = null; } else { value.secondaryTargetIds[i] = hits[i].target.GUID; } };
        value.secondaryHitLocations = new int[value.numberOfShots]; for (int i = 0; i < hits.Count; ++i) { if (hits[i].target.GUID == value.targetId) { value.secondaryHitLocations[i] = 0; } else { value.secondaryHitLocations[i] = hits[i].hitLocation; } };
        value.toHitRolls = new float[value.numberOfShots]; for (int i = 0; i < hits.Count; ++i) { value.toHitRolls[i] = hits[i].hitRoll; };
        value.locationRolls = new float[value.numberOfShots]; for (int i = 0; i < hits.Count; ++i) { value.locationRolls[i] = hits[i].locationRoll; };
        value.dodgeRolls = new float[value.numberOfShots]; for (int i = 0; i < hits.Count; ++i) { value.dodgeRolls[i] = hits[i].dodgeRoll; };
        value.dodgeSuccesses = new bool[value.numberOfShots]; for (int i = 0; i < hits.Count; ++i) { value.dodgeSuccesses[i] = false; };
        value.hitLocations = new int[value.numberOfShots]; for (int i = 0; i < hits.Count; ++i) { if (hits[i].target.GUID == value.targetId) { value.hitLocations[i] = hits[i].hitLocation; } else { value.hitLocations[i] = 0; } };
        value.hitVariance = new int[value.numberOfShots]; for (int i = 0; i < hits.Count; ++i) { value.hitVariance[i] = 0; };
        value.hitQualities = new AttackImpactQuality[value.numberOfShots]; for (int i = 0; i < hits.Count; ++i) { value.hitQualities[i] = hits[i].impactQuality; };
        value.attackDirections = new AttackDirection[value.numberOfShots]; for (int i = 0; i < hits.Count; ++i) { value.attackDirections[i] = hits[i].attackDirection; };
        value.hitPositions = new Vector3[value.numberOfShots]; for (int i = 0; i < hits.Count; ++i) { value.hitPositions[i] = hits[i].hitPosition; };
        f_fakeHitInfo = value;
        return f_fakeHitInfo;
      }
    }
    public AdvWeaponResolveInfo resolve(ICombatant target) {
      if (resolveInfo.ContainsKey(target) == false) { resolveInfo.Add(target, new AdvWeaponResolveInfo(this)); }
      return resolveInfo[target];
    }
    public void FillResolveHitInfo() {
      foreach(AdvWeaponHitInfoRec hit in hits) {
        if (hit.isHit) {
          this.resolve(hit.target).AddHit(hit.hitLocation, hit.EffectsMod, hit.isAOE);
        }
      }
    }
    public void ApplyMantanceStats() {
      int realshots = 0;
      foreach (var hit in hits) {
        if (hit.isAOE) { continue; }
        if (hit.fragInfo.isFragPallet) { continue; }
        ++realshots;
      }
      Statistic stat = this.weapon.StatCollection.GetOrCreateStatisic<int>(CustomAmmoCategories.Settings.OverrallShootsCountWeaponStat,0);
      stat.SetValue<int>(stat.Value<int>() + realshots);
      Log.M.TWL(0, "ApplyMantanceStats "+this.attackSequenceId+":" + this.weapon.defId + " => " + stat.Value<int>());
    }
    public void ApplyHitEffects() {
      foreach (var trg in this.resolveInfo) {
        AdvWeaponResolveInfo advRes = trg.Value;
        ICombatant target = trg.Key;
        Weapon weapon = this.weapon;
        WeaponHitInfo hitInfo = this.fakeHitInfo.Value;
        bool effectPerHit = weapon.StatusEffectsPerHit();
        Log.M.TWL(0, "ApplyHitEffects:" + this.weapon.defId + " " + target.DisplayName);
        if ((advRes.hitLocations.Count > 0) && (!target.IsDead) && (target != null)) {
          EffectData[] effects = weapon.StatusEffects();
          foreach (EffectData statusEffect in effects) {
            if (statusEffect.targetingData.effectTriggerType == EffectTriggerType.OnHit) {
              string effectID = string.Format("OnHitEffect_{0}_{1}", (object)this.Sequence.attacker.GUID, (object)this.attackSequenceId);
              if (statusEffect.Description == null || statusEffect.Description.Id == null || statusEffect.Description.Name == null) {
                CustomAmmoCategoriesLog.Log.LogWrite($"WARNING: EffectID:{effectID} has broken effectDescId:{statusEffect?.Description.Id} effectDescName:{statusEffect?.Description.Name}! SKIPPING\n", true);
              } else {
                int effectsApply = 0;
                foreach (EffectsLocaltion HitLocation in advRes.hitLocations) {
                  float roll = Random.Range(0f, 1f);
                  if (roll > HitLocation.effectsMod) {
                    Log.LogWrite($"Roll fail\n");
                    continue;
                  }
                  ++effectsApply;
                  Log.LogWrite($"Applying effectID:{effectID} with effectDescId:{statusEffect?.Description.Id} effectDescName:{statusEffect?.Description.Name}\n");
                  this.Sequence.Director.Combat.EffectManager.CreateEffect(statusEffect, effectID, this.Sequence.stackItemUID, (ICombatant)this.Sequence.attacker, target, hitInfo, HitLocation.location, false);
                  if (effectPerHit == false) { break; }
                }
                if (target != null) {
                  if (effectsApply > 0) {
                    if (effectPerHit == false) {
                      //this.Sequence.Director.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(this.Sequence.attacker.GUID, target.GUID, statusEffect.Description.Name, FloatieMessage.MessageNature.Debuff));
                      advRes.floatieMessages.Add(statusEffect.Description.Name);
                    } else {
                      //this.Sequence.Director.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(this.Sequence.attacker.GUID, target.GUID, statusEffect.Description.Name + " x " + effectsApply.ToString(), FloatieMessage.MessageNature.Debuff));
                      advRes.floatieMessages.Add(statusEffect.Description.Name + " x " + effectsApply.ToString());
                    }
                  }
                }
              }
            }
          }
          AbstractActor targetActor = target as AbstractActor;
          if (targetActor != null) {
            List<EffectData> effectsForTriggerType = targetActor.GetComponentStatusEffectsForTriggerType(EffectTriggerType.OnDamaged);
            for (int index = 0; index < effectsForTriggerType.Count; ++index) {
              targetActor.Combat.EffectManager.CreateEffect(effectsForTriggerType[index], string.Format("OnDamagedEffect_{0}_{1}", (object)target.GUID, (object)hitInfo.attackSequenceId), this.Sequence.stackItemUID, target, (ICombatant)this.Sequence.attacker, hitInfo, advRes.hitLocations[0].location, false);
            }
          }
        }
      }
    }
    public void Apply() {
      if (resolveDamageMessage == null) { return; }
      WeaponHitInfo weaponHitInfo = this.fakeHitInfo.Value;
      Log.M.TWL(0, "Resolve weapon damage "+this.weapon.defId);
      ResolveDamageHelper.ResolveWeaponDamageAdv(ref weaponHitInfo);
      this.Sequence.messageCoordinator().MessageComplete((MessageCenterMessage)resolveDamageMessage);
      this.setApplyState();
    }
    public static void printExpectedMessages(int sequenceId) {
      Log.M.TWL(0, "printExpectedMessages "+sequenceId);
      if (AdvWeaponHitInfo.advHitMessages.TryGetValue(sequenceId, out List<AdvHitMessage> expList)) {
        for (int index = 0; index < expList.Count; ++index) {
          AdvHitMessage aMessage = expList[index];
          Log.M.W(1, "["+ aMessage.Index()+ "] R:"+aMessage.isResolve);
          if (aMessage.advInfo != null) {
            Log.M.W(" advInfo " + aMessage.advInfo.weapon.defId+"/"+aMessage.advInfo.weapon.ammo().Id+"/"+aMessage.advInfo.weapon.mode().Id);
          } else {
            Log.M.W(" advInfo:null ");
          }
          if (aMessage.advRec != null) {
            Log.M.W(" advRec loc:"+aMessage.advRec.hitLocationStr+" aoe:"+aMessage.advRec.isAOE+" fragMain:"+aMessage.advRec.isFragMain+" fragPallet:"+aMessage.advRec.fragInfo.isFragPallet);
          } else {
            Log.M.W(" advRec:null");
          }
          Log.M.WL("");
        }
      }
    }
    public static void printApplyState(int sequenceId) {
      string result = string.Empty;
      if (AdvWeaponHitInfo.advHitMessages.TryGetValue(sequenceId, out List<AdvHitMessage> expList)) {
        for (int index = 0; index < expList.Count; ++index) {
          AdvHitMessage aMessage = expList[index];
          result += (aMessage.isVisuals ? "V" : "_");
          result += (aMessage.isApplied ? "A" : "_");
        }
      }
      Log.M.TWL(0, "printApplyState "+sequenceId+" "+result);
    }
    public static void AddToExpected(AdvWeaponHitInfo advInfo) {
      if(AdvWeaponHitInfo.advHitMessages.TryGetValue(advInfo.attackSequenceId, out var expList) == false) {
        expList = new List<AdvHitMessage>();
        AdvWeaponHitInfo.advHitMessages.Add(advInfo.attackSequenceId, expList);
      }
      if (AdvWeaponHitInfo.advHitMessagesMap.TryGetValue(advInfo.attackSequenceId, out var expMap) == false) {
        expMap = new Dictionary<AdvHitMessage, int>();
        AdvWeaponHitInfo.advHitMessagesMap.Add(advInfo.attackSequenceId, expMap);
      }
      bool atLeastOneHit = false;
      for(int hitIndex = 0; hitIndex < advInfo.hits.Count; ++hitIndex) {
        AdvWeaponHitInfoRec advRec = advInfo.hits[hitIndex];
        if (advRec.isHit == false) { continue; }
        AdvHitMessage aMsg = new AdvHitMessage(advRec, advInfo, false);
        expList.Add(aMsg);
        expMap.Add(aMsg, expList.Count - 1);
        atLeastOneHit = true;
      }
      if (advInfo.weapon.isStreak() && (atLeastOneHit == false)) { return; }
      AdvHitMessage arMsg = new AdvHitMessage(null, advInfo, true);
      expList.Add(arMsg);
      expMap.Add(arMsg, expList.Count - 1);
    }
    private static ExcelPackage attackLog = null;
    private static int damageLogIndex = 0;
    private static int critLogIndex = 0;
    private static string attackLogFileName = null;
    public static void ClearAttackLog() {
      if(attackLog != null) { attackLog.Save(); attackLog = null; damageLogIndex = 0; critLogIndex = 0; }
    }
    public static void FlushInfo(AttackDirector.AttackSequence sequence) {
      try {
        if (sequence == null) { return; }
        WeaponHitInfo?[][] weaponHitInfo = Traverse.Create(sequence).Field<WeaponHitInfo?[][]>("weaponHitInfo").Value;
        for (int groupIndex = 0; groupIndex < weaponHitInfo.Length; ++groupIndex) {
          for (int weaponIndex = 0; weaponIndex < weaponHitInfo[groupIndex].Length; ++weaponIndex) {
            if (weaponHitInfo[groupIndex][weaponIndex].HasValue == false) {
              Log.M.TWL(0, "WeaponHitInfo at grp:" + groupIndex + " index:" + weaponIndex + " is null. How?!", true);
              continue;
            }
            AdvWeaponHitInfo advInfo = weaponHitInfo[groupIndex][weaponIndex].Value.advInfo();
            CombatStatisticHelper.ProcessAttack(advInfo);
          }
        }
        if (CustomAmmoCategories.Settings.AttackLogWrite == false) { return; }
        ExcelWorksheet damageWS = null;
        ExcelWorksheet critWS = null;
        if(attackLog == null) {
          attackLog = new ExcelPackage();
          attackLogFileName = Path.Combine(CustomAmmoCategories.Settings.directory, "AttacksLogs");
          if(Directory.Exists(attackLogFileName) == false) { Directory.CreateDirectory(attackLogFileName); }
          if(Directory.Exists(attackLogFileName) == false) { return; }
          attackLogFileName = Path.Combine(attackLogFileName, "log_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".xlsx");
          if(File.Exists(attackLogFileName)) { return; }
          damageWS = attackLog.Workbook.Worksheets.Add("damage");
          critWS = attackLog.Workbook.Worksheets.Add("crit");
          damageWS.Cells[1, 1].Value = "attack id";
          damageWS.Cells[1, 2].Value = "hit id";
          damageWS.Cells[1, 3].Value = "attack end time";
          damageWS.Cells[1, 4].Value = "attacker";
          damageWS.Cells[1, 5].Value = "weapon";
          damageWS.Cells[1, 6].Value = "ammo";
          damageWS.Cells[1, 7].Value = "mode";
          damageWS.Cells[1, 8].Value = "target";
          damageWS.Cells[1, 9].Value = "hit index";
          damageWS.Cells[1, 10].Value = "is applied";
          damageWS.Cells[1, 11].Value = "location";
          damageWS.Cells[1, 12].Value = "called shot location";
          damageWS.Cells[1, 13].Value = "hit roll";
          damageWS.Cells[1, 14].Value = "corrected roll";
          damageWS.Cells[1, 15].Value = "hit chance";
          damageWS.Cells[1, 16].Value = "damage";
          damageWS.Cells[1, 17].Value = "ap damage";
          damageWS.Cells[1, 18].Value = "heat";
          damageWS.Cells[1, 19].Value = "stability";
          damageWS.Cells[1, 20].Value = "is intecepted";
          damageWS.Cells[1, 21].Value = "intecepted by";
          damageWS.Cells[1, 22].Value = "is AoE";
          damageWS.Cells[1, 23].Value = "is frag separated";
          damageWS.Cells[1, 24].Value = "is frag pallet";
          damageWS.Cells[1, 25].Value = "frags pallets count";
          damageLogIndex = 2;
          critWS.Cells[1, 1].Value = "attack id";
          critWS.Cells[1, 2].Value = "attack end time";
          critWS.Cells[1, 3].Value = "attacker";
          critWS.Cells[1, 4].Value = "weapon";
          critWS.Cells[1, 5].Value = "ammo";
          critWS.Cells[1, 6].Value = "mode";
          critWS.Cells[1, 7].Value = "target";
          critWS.Cells[1, 8].Value = "location id";
          critWS.Cells[1, 9].Value = "location";
          critWS.Cells[1, 10].Value = "struct id";
          critWS.Cells[1, 11].Value = "struct";
          critWS.Cells[1, 12].Value = "armor on hit";
          critWS.Cells[1, 13].Value = "structure in hit";
          critWS.Cells[1, 14].Value = "crit chance";
          critWS.Cells[1, 15].Value = "component";
          critLogIndex = 2;
          attackLog.SaveAs(new FileInfo(attackLogFileName));
        } else {
          damageWS = attackLog.Workbook.Worksheets["damage"];
          critWS = attackLog.Workbook.Worksheets["crit"];
        }
        if(damageWS == null) { return; }
        string time = DateTime.Now.ToString("HH-mm-ss");
          //(WeaponHitInfo?[][])typeof(AttackDirector.AttackSequence).GetField("weaponHitInfo", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sequence);
        DamageModifiersCache.ClearComulativeDamage();
        Log.M.TWL(0, "flushing info:" + sequence.id);
        for (int groupIndex = 0; groupIndex < weaponHitInfo.Length; ++groupIndex) {
          for (int weaponIndex = 0; weaponIndex < weaponHitInfo[groupIndex].Length; ++weaponIndex) {
            if (weaponHitInfo[groupIndex][weaponIndex].HasValue == false) {
              Log.M.TWL(0, "WeaponHitInfo at grp:" + groupIndex + " index:" + weaponIndex + " is null. How?!", true);
              continue;
            }
            AdvWeaponHitInfo advInfo = weaponHitInfo[groupIndex][weaponIndex].Value.advInfo();
            foreach (AdvWeaponHitInfoRec advRec in advInfo.hits) {
              Mech mech = advRec.target as Mech;
              Vehicle vehicle = advRec.target as Vehicle;
              Turret turret = advRec.target as Turret;
              string calledShotLocation = "None";
              if((advInfo.Sequence.calledShotLocation != 0)&&(advInfo.Sequence.calledShotLocation != 65536)) {
                if (mech != null) { calledShotLocation = ((ArmorLocation)advInfo.Sequence.calledShotLocation).ToString(); }else
                if (vehicle != null) { calledShotLocation = ((VehicleChassisLocations)advInfo.Sequence.calledShotLocation).ToString(); } else
                if (turret != null) { calledShotLocation = ((BuildingLocation)advInfo.Sequence.calledShotLocation).ToString(); } else {
                  calledShotLocation = "Structure";
                }
              }
              damageWS.Cells[damageLogIndex, 1].Value = advInfo.Sequence.id;
              damageWS.Cells[damageLogIndex, 2].Value = advRec.UID;
              damageWS.Cells[damageLogIndex, 3].Value = time;
              damageWS.Cells[damageLogIndex, 4].Value = new Text(advInfo.Sequence.attacker.DisplayName).ToString();
              damageWS.Cells[damageLogIndex, 5].Value = advInfo.weapon.UIName;
              damageWS.Cells[damageLogIndex, 6].Value = new Text(advInfo.weapon.ammo().UIName).ToString();
              damageWS.Cells[damageLogIndex, 7].Value = new Text(advInfo.weapon.mode().UIName).ToString();
              damageWS.Cells[damageLogIndex, 8].Value = new Text(advRec.target.DisplayName).ToString();
              damageWS.Cells[damageLogIndex, 9].Value = advRec.hitIndex;
              damageWS.Cells[damageLogIndex, 10].Value = advRec.isImplemented;
              damageWS.Cells[damageLogIndex, 11].Value = "(" + advRec.hitLocation.ToString() + ")" + advRec.hitLocationStr.ToString();
              damageWS.Cells[damageLogIndex, 12].Value = "(" + advInfo.Sequence.calledShotLocation + ")" + calledShotLocation;
              damageWS.Cells[damageLogIndex, 13].Value = advRec.hitRoll;
              damageWS.Cells[damageLogIndex, 14].Value = advRec.correctedRoll;
              damageWS.Cells[damageLogIndex, 15].Value = advInfo.hitChance;
              damageWS.Cells[damageLogIndex, 16].Value = advRec.Damage.ToString();
              damageWS.Cells[damageLogIndex, 17].Value = advRec.APDamage.ToString();
              damageWS.Cells[damageLogIndex, 18].Value = advRec.Heat.ToString();
              damageWS.Cells[damageLogIndex, 19].Value = advRec.Stability.ToString();
              damageWS.Cells[damageLogIndex, 20].Value = advRec.interceptInfo.Intercepted;
              damageWS.Cells[damageLogIndex, 21].Value = (advRec.interceptInfo.InterceptedAMS == null ? "none" : advRec.interceptInfo.InterceptedAMS.UIName.ToString());
              damageWS.Cells[damageLogIndex, 22].Value = advRec.isAOE;
              damageWS.Cells[damageLogIndex, 23].Value = advRec.fragInfo.separated;
              damageWS.Cells[damageLogIndex, 24].Value = advRec.fragInfo.isFragPallet;
              damageWS.Cells[damageLogIndex, 25].Value = advRec.fragInfo.fragsCount;
              ++damageLogIndex;
            }
          }
        }
        if(critWS == null) { return; }
        for (int groupIndex = 0; groupIndex < weaponHitInfo.Length; ++groupIndex) {
          for (int weaponIndex = 0; weaponIndex < weaponHitInfo[groupIndex].Length; ++weaponIndex) {
            if (weaponHitInfo[groupIndex][weaponIndex].HasValue == false) {
              Log.M.TWL(0, "WeaponHitInfo at grp:" + groupIndex + " index:" + weaponHitInfo + " is null. How?! Again weapon without representation?! You realy should stop it!", true);
              continue;
            }
            AdvWeaponHitInfo advInfo = weaponHitInfo[groupIndex][weaponIndex].Value.advInfo();
            foreach (var aCrits in advInfo.resolveInfo) {
              foreach (var advCrit in aCrits.Value.Crits) {
                critWS.Cells[critLogIndex, 1].Value = advInfo.Sequence;
                critWS.Cells[critLogIndex, 2].Value = time;
                critWS.Cells[critLogIndex, 3].Value = new Text(advInfo.Sequence.attacker.DisplayName).ToString();
                critWS.Cells[critLogIndex, 4].Value = advInfo.weapon.UIName;
                critWS.Cells[critLogIndex, 5].Value = new Text(advInfo.weapon.ammo().UIName).ToString();
                critWS.Cells[critLogIndex, 6].Value = new Text(advInfo.weapon.mode().UIName).ToString();
                critWS.Cells[critLogIndex, 7].Value = new Text(advCrit.unit.DisplayName).ToString();
                critWS.Cells[critLogIndex, 8].Value = advCrit.armorLocation;
                critWS.Cells[critLogIndex, 9].Value = advCrit.armorLocationStr;
                critWS.Cells[critLogIndex, 10].Value = advCrit.structureLocation;
                critWS.Cells[critLogIndex, 11].Value = advCrit.structureLocationStr;
                critWS.Cells[critLogIndex, 12].Value = advCrit.armorOnHit;
                critWS.Cells[critLogIndex, 13].Value = advCrit.structureOnHit;
                critWS.Cells[critLogIndex, 14].Value = advCrit.critChance;
                critWS.Cells[critLogIndex, 15].Value = (advCrit.component == null ? "none" : new Text(advCrit.component.UIName).ToString());
                ++critLogIndex;
              }
            }
          }
        }
        attackLog.Save();
      }catch(Exception e) {
        Log.M.TWL(0,e.ToString(),true);
      }
    }
    public static void Sanitize(int sequenceId) {
      if (AdvWeaponHitInfo.advHitMessages.TryGetValue(sequenceId, out List<AdvHitMessage> list) == false) {
        return;
      }
      for (int index = 1; index < list.Count; ++index) {
        if (list[index].isApplied) { continue; }
        if (list[index].CanBeApplied()) { list[index].Apply(false); }
      }
    }
    public static void Clear(int sequenceId) {
      if (AdvWeaponHitInfo.advancedWeaponHitInfo.ContainsKey(sequenceId)) {
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
      if (AdvWeaponHitInfo.advHitMessagesMap.ContainsKey(sequenceId)) {
        AdvWeaponHitInfo.advHitMessagesMap.Remove(sequenceId);
      }
      if (AdvWeaponHitInfo.advHitMessages.ContainsKey(sequenceId)) {
        AdvWeaponHitInfo.advHitMessages.Remove(sequenceId);
      }
    }
    public static bool CanProcessMessage() {
      return false;
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
      resolveDamageMessage = null;
      isResolved = false;
    }
    public void AppendAoEHit(int primeIndex, float fulldamage, float Damage, float Heat, float Stability, ICombatant target, Vector3 position, int location) {
      Log.M?.WL(0,"AdvInfo.AppendAoEHit:" + primeIndex + "\n");
      AdvWeaponHitInfoRec hit = new AdvWeaponHitInfoRec(this);
      int hitIndex = this.hits.Count;
      hit.hitIndex = hitIndex;
      hit.hitPosition = position;
      hit.Damage = Damage * target.AoEDamageMult(location);
      Log.M?.WL(1, $"hit: {hitIndex} effective damage:{hit.Damage}");
      hit.APDamage = 0f;
      hit.Heat = Heat;
      hit.Stability = Stability;
      hit.target = target;
      hit.hitLocation = location;
      hit.projectileSpeed = 0f;
      hit.isAOE = true;
      hit.AOEKey = primeIndex;
      hit.dodgeRoll = 0f;
      hit.locationRoll = 0f;
      hit.impactQuality = AttackImpactQuality.Solid;
      hit.attackDirection = AttackDirection.FromArtillery;
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
        hit.hitIndex = hitIndex;
        hit.hitPosition = hitInfo.hitPositions[hitIndex];
        hit.fragInfo.isFragPallet = true;
        hit.fragInfo.fragMainHitIndex = advRec.hitIndex;
        hit.impactQuality = hitInfo.hitQualities[hitIndex];
        hit.attackDirection = hitInfo.attackDirections[hitIndex];
        hit.Damage = weapon.DamagePerShot/(float)weapon.ProjectilesPerShot;
        hit.APDamage = weapon.StructureDamagePerShot/(float)weapon.ProjectilesPerShot;
        hit.Heat = weapon.HeatDamagePerShot/(float)weapon.ProjectilesPerShot;
        hit.Stability = weapon.Instability()/(float)weapon.ProjectilesPerShot;
        hit.hitRoll = hitInfo.toHitRolls[hitIndex];
        hit.dodgeRoll = hitInfo.dodgeRolls[hitIndex];
        hit.locationRoll = hitInfo.locationRolls[hitIndex];
        hit.correctedRoll = this.Sequence.GetCorrectedRoll(hit.hitRoll, this.Sequence.attacker.team);
        hit.target = null;
        if (string.IsNullOrEmpty(hitInfo.secondaryTargetIds[hitIndex]) == false) {
          ICombatant trg = Combat.FindCombatantByGUID(hitInfo.secondaryTargetIds[hitIndex]);
          if (trg != null) {
            hit.target = trg;
            hit.hitLocation = hitInfo.secondaryHitLocations[hitIndex];
          }
        }
        if (hit.target == null) {
          ICombatant trg = Combat.FindCombatantByGUID(hitInfo.targetId);
          if (trg != null) {
            hit.target = trg;
            hit.hitLocation = hitInfo.hitLocations[hitIndex];
          }
        }
        if (hit.target == null) {
          hit.target = this.Sequence.chosenTarget;
          hit.hitLocation = hitInfo.hitLocations[hitIndex];
        }
        if (hit.target.isSpawnProtected() || weapon.parent.isSpawnProtected()) {
          hit.hitLocation = 0;
        }
        Log.M.WL(1, "h:"+hitIndex+" loc:("+hit.hitLocation+")"+hit.hitLocationStr+" trg:"+hit.target);
        hit.trajectoryInfo.type = TrajectoryType.Unguided;
        hit.GenerateTrajectory(advRec.hitPosition);
        hit.projectileSpeed = 0f;
        this.hits.Add(hit);
      }
    }
  }
}