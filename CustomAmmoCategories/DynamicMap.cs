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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using BattleTech;
using BattleTech.Assetbundles;
using BattleTech.Data;
using BattleTech.Rendering;
using BattleTech.Rendering.Mood;
using BattleTech.Rendering.Trees;
using BattleTech.Rendering.UI;
using BattleTech.Rendering.UrbanWarfare;
using BattleTech.UI;
using CustAmmoCategories;
using CustAmmoCategoriesPatches;
using CustomAmmoCategoriesLog;
using CustomAmmoCategoriesPatches;
using HarmonyLib;
using HBS.Util;
using IRBTModUtils;
using Localize;
using SVGImporter;
using TB.ComponentModel;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace CustAmmoCategories {
  public static partial class CustomAmmoCategories {
    public static Vector3 GetBuildingHitPosition(this LineOfSight LOS, AbstractActor attacker, BattleTech.Building target, Vector3 attackPosition, float weaponRange, Vector3 origHitPosition) {
      Vector3 a = origHitPosition;
      Vector3 vector3_1 = attackPosition + attacker.HighestLOSPosition;
      string guid = target.GUID;
      Vector3 collisionWorldPos = Vector3.zero;
      bool flag = false;
      if ((UnityEngine.Object)target.BuildingRep == (UnityEngine.Object)null)
        return a;
      foreach (Collider allRaycastCollider in target.GameRep.AllRaycastColliders) {
        if (LOS.HasLineOfFire(vector3_1, allRaycastCollider.bounds.center, guid, weaponRange, out collisionWorldPos)) {
          a = allRaycastCollider.bounds.center;
          flag = true;
          break;
        }
      }
      for (int index1 = 0; index1 < target.LOSTargetPositions.Length; ++index1) {
        if (LOS.HasLineOfFire(vector3_1, target.LOSTargetPositions[index1], guid, weaponRange, out collisionWorldPos)) {
          if (flag) {
            Vector3 end = Vector3.Lerp(a, target.LOSTargetPositions[index1], UnityEngine.Random.Range(0.0f, 0.15f));
            if (LOS.HasLineOfFire(vector3_1, end, guid, weaponRange, out collisionWorldPos))
              a = end;
          } else {
            Vector3 vector3_2 = a;
            for (int index2 = 0; index2 < 10; ++index2) {
              vector3_2 = Vector3.Lerp(vector3_2, target.LOSTargetPositions[index1], UnityEngine.Random.Range(0.1f, 0.6f));
              if (LOS.HasLineOfFire(vector3_1, vector3_2, guid, weaponRange, out collisionWorldPos)) {
                a = vector3_2;
                flag = true;
                break;
              }
            }
            if (!flag) {
              a = target.LOSTargetPositions[index1];
              flag = true;
            }
          }
        }
      }
      Ray ray = new Ray(vector3_1, a - vector3_1);
      foreach (Collider allRaycastCollider in target.GameRep.AllRaycastColliders) {
        GameObject gameObject = allRaycastCollider.gameObject;
        bool activeSelf = gameObject.activeSelf;
        gameObject.SetActive(true);
        RaycastHit hitInfo;
        if (allRaycastCollider.Raycast(ray, out hitInfo, 1000f)) {
          gameObject.SetActive(activeSelf);
          return hitInfo.point;
        }
        gameObject.SetActive(activeSelf);
      }
      return a;
    }
    public static Vector3 getImpactPositionSimple(this ICombatant initialTarget, AbstractActor attacker, Vector3 attackPosition, int hitLocation) {
      Vector3 impactPoint = initialTarget.CurrentPosition;
      AttackDirection attackDirection = AttackDirection.FromFront;
      if ((UnityEngine.Object)initialTarget.GameRep != (UnityEngine.Object)null) {
        impactPoint = initialTarget.GameRep.GetHitPosition(hitLocation);
        attackDirection = initialTarget.Combat.HitLocation.GetAttackDirection(attackPosition, initialTarget);
        if (initialTarget.UnitType == UnitType.Building) {
          impactPoint = attacker.Combat.LOS.GetBuildingHitPosition(attacker, initialTarget as BattleTech.Building, attackPosition, 100f, impactPoint);
        } else {
          Vector3 origin = attackPosition + attacker.HighestLOSPosition;
          Vector3 vector3_2 = impactPoint - origin;
          Ray ray2 = new Ray(origin, vector3_2.normalized);
          foreach (Collider allRaycastCollider in initialTarget.GameRep.AllRaycastColliders) {
            RaycastHit hitInfo;
            if (allRaycastCollider.Raycast(ray2, out hitInfo, vector3_2.magnitude)) {
              impactPoint = hitInfo.point;
              break;
            }
          }
        }
      }
      return impactPoint;
    }
    public static Vector3 getImpactPositionSimple(this ICombatant initialTarget, Vector3 attackPosition, int hitLocation) {
      Vector3 impactPoint = initialTarget.CurrentPosition;
      AttackDirection attackDirection = AttackDirection.FromFront;
      if ((UnityEngine.Object)initialTarget.GameRep != (UnityEngine.Object)null) {
        impactPoint = initialTarget.GameRep.GetHitPosition(hitLocation);
        attackDirection = initialTarget.Combat.HitLocation.GetAttackDirection(attackPosition, initialTarget);
        Vector3 origin = attackPosition;
        Vector3 vector3_2 = impactPoint - origin;
        Ray ray2 = new Ray(origin, vector3_2.normalized);
        foreach (Collider allRaycastCollider in initialTarget.GameRep.AllRaycastColliders) {
          RaycastHit hitInfo;
          if (allRaycastCollider.Raycast(ray2, out hitInfo, vector3_2.magnitude)) {
            impactPoint = hitInfo.point;
            break;
          }
        }
      }
      return impactPoint;
    }
    public static bool BecomesDangerousOnImpact(this Weapon weapon) {
      return weapon.ammo().SurfaceBecomeDangerousOnImpact == TripleBoolean.True;
    }
    public static bool InstallMineField(this Weapon weapon) {
      return weapon.ammo().MineField.Count > 0;
    }
    public static MineFieldDef MineFieldDef(this Weapon weapon) {
      return weapon.ammo().MineField;
    }
    public static float FireTerrainChance(this Weapon weapon) {
      return (weapon.exDef().FireTerrainChance + weapon.ammo().FireTerrainChance + weapon.mode().FireTerrainChance) * DynamicMapHelper.BiomeLitFireChance();
    }
    public static int FireDurationWithoutForest(this Weapon weapon) {
      return Mathf.RoundToInt((weapon.exDef().FireDurationWithoutForest + weapon.ammo().FireDurationWithoutForest + weapon.mode().FireDurationWithoutForest) * DynamicMapHelper.BiomeWeaponFireDuration());
    }
    public static int FireTerrainStrength(this Weapon weapon) {
      return Mathf.RoundToInt((weapon.exDef().FireTerrainStrength + weapon.ammo().FireTerrainStrength + weapon.mode().FireTerrainStrength) * DynamicMapHelper.BiomeWeaponFireStrength());
    }
    public static int ClearMineFieldRadius(this Weapon weapon) {
      return weapon.exDef().ClearMineFieldRadius + weapon.ammo().ClearMineFieldRadius + weapon.mode().ClearMineFieldRadius;
    }
    public static bool FireOnSuccessHit(this Weapon weapon) {
      WeaponMode mode = weapon.mode();
      if (mode.FireOnSuccessHit != TripleBoolean.NotSet) { return mode.FireOnSuccessHit == TripleBoolean.True; }
      ExtAmmunitionDef ammo = weapon.ammo();
      if (ammo.FireOnSuccessHit != TripleBoolean.NotSet) { return ammo.FireOnSuccessHit == TripleBoolean.True; }
      return weapon.exDef().FireOnSuccessHit == TripleBoolean.True;
    }
    public static int FireTerrainCellRadius(this Weapon weapon) {
      return weapon.exDef().FireTerrainCellRadius + weapon.ammo().FireTerrainCellRadius + weapon.mode().FireTerrainCellRadius;
    }
    public static DesignMaskDef tempDesignMask(this Weapon weapon, out int turns, out string vfx, out Vector3 scale, out int radius) {
      turns = 0;
      vfx = string.Empty;
      scale = new Vector3();
      radius = 0;
      ExtAmmunitionDef ammo = weapon.ammo();
      if (string.IsNullOrEmpty(ammo.tempDesignMaskOnImpact) == false) {
        if (DynamicMapHelper.loadedMasksDef.ContainsKey(ammo.tempDesignMaskOnImpact)) {
          turns = ammo.tempDesignMaskOnImpactTurns;
          vfx = ammo.LongVFXOnImpact;
          scale = new Vector3(ammo.LongVFXOnImpactScaleX, ammo.LongVFXOnImpactScaleY, ammo.LongVFXOnImpactScaleZ);
          radius = ammo.tempDesignMaskCellRadius;
          return DynamicMapHelper.loadedMasksDef[ammo.tempDesignMaskOnImpact];
        }
      }
      return null;
    }
  }
  public class DynMapBurnHexRequest {
    public MapTerrainHexCell hex { get; private set; }
    public Weapon weapon { get; private set; }
    public float FireTerrainChance { get; private set; }
    public int FireTerrainStrength { get; private set; }
    public int FireDurationWithoutForest { get; private set; }
    public DynMapBurnHexRequest(MapTerrainHexCell hex, Weapon weapon, float FireTerrainChance, int FireTerrainStrength, int FireDurationWithoutForest) {
      this.hex = hex; this.weapon = weapon; this.FireTerrainChance = FireTerrainChance; this.FireTerrainStrength = FireTerrainStrength; this.FireDurationWithoutForest = FireDurationWithoutForest;
    }
    public void Invoke() {
      if(hex.TryBurnCellSync(weapon, FireTerrainChance, FireTerrainStrength, FireDurationWithoutForest)) {
        DynamicMapHelper.burningHexes.Add(hex);
      }
    }
  }
  public class DynMapManipulateMineField {
    public MapTerrainHexCell hex { get; private set; }
    //public MineField mineField { get; private set; }
    public MineFieldDef definition { get; private set; }
    public AbstractActor owner { get; private set; }
    public Weapon weapon { get; private set; }
    public int ClearCount { get; private set; }
    public float ClearChance { get; private set; }
    public DynMapManipulateMineField(MapTerrainHexCell hex, MineFieldDef definition, AbstractActor owner, Weapon weapon, int ClearCount, float ClearChance) {
      this.hex = hex; this.definition = definition; this.owner = owner; this.weapon = weapon; this.ClearCount = ClearCount; this.ClearChance = ClearChance;
    }
    public void Invoke() {
      if (definition != null) { hex.AddMineFieldSync(new MineField(hex,definition,owner,weapon)); }
      if(ClearCount > 0) {
        hex.ClearMineFieldSync(ClearCount, ClearChance);
      }
    }
  }
  public class DynMapVFXRequest {
    public MapTerrainHexCell hex { get; private set; }
    public CombatGameState combat { get; private set; }
    public string prefabVFX { get; private set; }
    public int counter { get; private set; }
    public Vector3 scale { get; private set; }
    public DynMapVFXRequest(MapTerrainHexCell hex, CombatGameState combat, string prefabVFX, int counter, Vector3 scale) {
      this.hex = hex; this.combat = combat; this.prefabVFX = prefabVFX; this.counter = counter; this.scale = scale;
    }
    public void Invoke() {
      hex.addTempTerrainVFXSync(combat, prefabVFX, counter, scale);
    }
  }
  public class DyncamicMapAsyncProcessor: MonoBehaviour {
    private Queue<DynMapBurnHexRequest> burnRequests;
    private Queue<AsyncDesignMaskApplyRecord> asyncTerrainDesignMaskQueue;
    private Queue<DynMapVFXRequest> VFXRequests;
    private Queue<DynMapManipulateMineField> MineFieldRequests;
    public void Init() {
      burnRequests.Clear();
      asyncTerrainDesignMaskQueue.Clear();
    }
    public void TryBurnCellAsync(MapTerrainHexCell hex,Weapon weapon, float FireTerrainChance, int FireTerrainStrength, int FireDurationWithoutForest) {
      burnRequests.Enqueue(new DynMapBurnHexRequest(hex, weapon, FireTerrainChance, FireTerrainStrength, FireDurationWithoutForest));
    }
    public void addDesignMaskAsync(MapTerrainHexCell hex, DesignMaskDef dm, int counter) {
      if (dm == null) { return; }
      asyncTerrainDesignMaskQueue.Enqueue(new AsyncDesignMaskApplyRecord(dm, hex, counter));
    }
    public void addTempTerrainVFX(MapTerrainHexCell hex, CombatGameState combat, string prefabVFX, int counter, Vector3 scale) {
      VFXRequests.Enqueue(new DynMapVFXRequest(hex,combat, prefabVFX, counter, scale));
    }
    public void addMineField(MapTerrainHexCell hex, MineFieldDef definition, AbstractActor owner, Weapon weapon) {
      MineFieldRequests.Enqueue(new DynMapManipulateMineField(hex, definition,owner,weapon, 0,0f));
    }
    public void clearMineField(MapTerrainHexCell hex, int count, float chance) {
      MineFieldRequests.Enqueue(new DynMapManipulateMineField(hex, null, null, null, count, chance));
    }
    public DyncamicMapAsyncProcessor() {
      burnRequests = new Queue<DynMapBurnHexRequest>();
      asyncTerrainDesignMaskQueue = new Queue<AsyncDesignMaskApplyRecord>();
      VFXRequests = new Queue<DynMapVFXRequest>();
      MineFieldRequests = new Queue<DynMapManipulateMineField>();
    }
    public void Update() {
      if(burnRequests.Count > 0) {
        DynMapBurnHexRequest burnRequest = burnRequests.Dequeue();
        burnRequest.Invoke();
      }
      if (asyncTerrainDesignMaskQueue.Count > 0) {
        AsyncDesignMaskApplyRecord arec = asyncTerrainDesignMaskQueue.Dequeue();
        Log.F.TWL(0, "async add design mask:" + arec.designMask.Id + " to " + arec.hexCell.center);
        arec.hexCell.addTempTerrainMask(arec.designMask, arec.counter);
      }
      if (VFXRequests.Count > 0) {
        DynMapVFXRequest VFXreq = VFXRequests.Dequeue();
        VFXreq.Invoke();
      }
      if (MineFieldRequests.Count > 0) {
        DynMapManipulateMineField MFReq = MineFieldRequests.Dequeue();
        MFReq.Invoke();
      }
    }
  }
  public static class DynamicMapAsyncProcHelper {
    private static DyncamicMapAsyncProcessor processor = null;
    public static int MinefieldDetectionLevel(this AbstractActor actor) {
      return Mathf.RoundToInt(actor.StatCollection.GetOrCreateStatisic<float>(CustomAmmoCategories.Settings.MinefieldDetectorStatName, 1.0f).Value<float>());
    }
    public static void Init(CombatHUD HUD) {
      if (DynamicMapAsyncProcHelper.processor != null) { GameObject.Destroy(DynamicMapAsyncProcHelper.processor); DynamicMapAsyncProcHelper.processor = null; }
      DynamicMapAsyncProcHelper.processor = HUD.gameObject.GetComponent<DyncamicMapAsyncProcessor>();
      if (DynamicMapAsyncProcHelper.processor == null) {
        DynamicMapAsyncProcHelper.processor = HUD.gameObject.AddComponent<DyncamicMapAsyncProcessor>();
      }
      DynamicMapAsyncProcHelper.processor.Init();
    }
    public static void TryBurnCellAsync(this MapTerrainHexCell hex, Weapon weapon) {
      TryBurnCellAsync(hex, weapon, weapon.FireTerrainChance(), weapon.FireTerrainStrength(), weapon.FireDurationWithoutForest());
    }
    public static void TryBurnCellAsync(this MapTerrainHexCell hex, Weapon weapon, float FireTerrainChance, int FireTerrainStrength, int FireDurationWithoutForest) {
      if (processor == null) { return; }
      processor.TryBurnCellAsync(hex, weapon, FireTerrainChance, FireTerrainStrength, FireDurationWithoutForest);
    }
    public static void addTempTerrainVFX(this MapTerrainHexCell hex, CombatGameState combat, string prefabVFX, int counter, Vector3 scale) {
      if (processor == null) { return; }
      processor.addTempTerrainVFX(hex,combat,prefabVFX,counter,scale);
    }
    public static void addDesignMaskAsync(this MapTerrainHexCell hex, DesignMaskDef dm, int counter) {
      if (processor == null) { return; }
      processor.addDesignMaskAsync(hex,dm,counter);
    }
    public static void addMineField(this MapTerrainHexCell hex, MineFieldDef definition, AbstractActor owner, Weapon weapon) {
      if (processor == null) { return; }
      processor.addMineField(hex, definition, owner, weapon);
    }
    public static void clearMineField(this MapTerrainHexCell hex, int count, float chance) {
      if (processor == null) { return; }
      processor.clearMineField(hex, count, chance);
    }

  }
  public class ObjectSpawnDataSelf : ObjectSpawnData {
    public bool keepPrefabRotation;
    public Vector3 scale;
    public string prefabStringName;
    public CombatGameState Combat;
    public ObjectSpawnDataSelf(string prefabName, Vector3 worldPosition, Quaternion worldRotation, Vector3 scale, bool playFX, bool autoPoolObject) :
      base(prefabName, worldPosition, worldRotation, playFX, autoPoolObject) {
      keepPrefabRotation = false;
      this.scale = scale;
      this.Combat = null;
    }
    public void DisableObjects(HashSet<string> names) {
      if (this.spawnedObject == null) { return; }
      Transform[] trs = this.spawnedObject.GetComponentsInChildren<Transform>(true);
      foreach(var tr in trs) {
        if (names.Contains(tr.name)) { tr.gameObject.SetActive(false); }
      }
    }
    public void CleanupSelf() {
      if (this == null) {
        CustomAmmoCategoriesLog.Log.LogWrite("Cleaning null?!!!\n", true);
        return;
      }
      CustomAmmoCategoriesLog.Log.LogWrite("Cleaning up " + this.prefabName + "\n");
      if (Combat == null) {
        CustomAmmoCategoriesLog.Log.LogWrite("Trying cleanup object " + this.prefabName + " never spawned\n", true);
        return;
      }
      if (this.spawnedObject == null) {
        CustomAmmoCategoriesLog.Log.LogWrite("Trying cleanup object " + this.prefabName + " already cleaned\n", true);
        return;
      }
      try {
        GameObject.Destroy(this.spawnedObject);
      } catch (Exception e) {
        CustomAmmoCategoriesLog.Log.LogWrite("Cleanup exception: " + e.ToString() + "\n", true);
        CustomAmmoCategoriesLog.Log.LogWrite("nulling spawned object directly\n", true);
        this.spawnedObject = null;
      }
      this.spawnedObject = null;
      CustomAmmoCategoriesLog.Log.LogWrite("Finish cleaning " + this.prefabName + "\n");
    }
    public void SpawnSelf(CombatGameState Combat, Transform parentTransform = null) {
      this.Combat = Combat;
      GameObject gameObject = Combat.DataManager.PooledInstantiate(this.prefabName, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
      if ((UnityEngine.Object)gameObject == (UnityEngine.Object)null) {
        CustomAmmoCategoriesLog.Log.LogWrite("Can't find " + prefabName + " in in-game prefabs\n");
        if (CACMain.Core.AdditinalFXObjects.ContainsKey(prefabName)) {
          CustomAmmoCategoriesLog.Log.LogWrite("Found in additional prefabs\n");
          gameObject = GameObject.Instantiate(CACMain.Core.AdditinalFXObjects[prefabName]);
        } else {
          CustomAmmoCategoriesLog.Log.LogWrite(" can't spawn prefab " + this.prefabName + " it is absent in pool,in-game assets and external assets\n", true);
          return;
        }
      }
      Log.LogWrite("SpawnSelf: " + this.prefabName + "\n");
      Component[] components = gameObject.GetComponentsInChildren<Component>();
      foreach (Component cmp in components) {
        if (cmp == null) { continue; };
        Log.LogWrite(" " + cmp.name + ":" + cmp.GetType().ToString() + "\n");
        ParticleSystem ps = cmp as ParticleSystem;
        if (ps != null) {
          var main = ps.main;
          main.scalingMode = ParticleSystemScalingMode.Hierarchy;
          Log.LogWrite("  " + ps.main.scalingMode.ToString() + "\n");
        }
      }
      if (parentTransform != null) { gameObject.transform.SetParent(parentTransform,true); }
      gameObject.transform.position = this.worldPosition;
      gameObject.transform.localScale = new Vector3(this.scale.x, this.scale.y, this.scale.z);
      Log.LogWrite("scale:"+ gameObject.transform.localScale+"\n");
      if (!this.keepPrefabRotation)
        gameObject.transform.rotation = this.worldRotation;
      if (this.playFX) {
        ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
        if (component != null) {
          //component.transform.localScale.Set(scale.x, scale.y, scale.z);
          gameObject.SetActive(true);
          component.Stop(true);
          component.Clear(true);
          component.transform.position = this.worldPosition;
          if (!this.keepPrefabRotation)
            component.transform.rotation = this.worldRotation;
          BTCustomRenderer.SetVFXMultiplier(component);
          component.Play(true);
        }
      }
      this.spawnedObject = gameObject;
    }
    public static ParticleSystem playVFXAt(CombatGameState Combat, string prefab, Vector3 pos, Vector3 scale, Vector3 lookAtPos) {
      GameObject gameObject = Combat.DataManager.PooledInstantiate(prefab, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
      if ((UnityEngine.Object)gameObject == (UnityEngine.Object)null) {
        CustomAmmoCategoriesLog.Log.LogWrite("Can't find " + prefab + " in in-game prefabs\n");
        if (CACMain.Core.AdditinalFXObjects.ContainsKey(prefab)) {
          CustomAmmoCategoriesLog.Log.LogWrite("Found in additional prefabs\n");
          gameObject = GameObject.Instantiate(CACMain.Core.AdditinalFXObjects[prefab]);
        } else {
          CustomAmmoCategoriesLog.Log.LogWrite(" can't spawn prefab " + prefab + " it is absent in pool,in-game assets and external assets\n", true);
          return null;
        }
      }
      Log.LogWrite("playVFXAt: " + prefab + "\n");
      Component[] components = gameObject.GetComponentsInChildren<Component>();
      foreach (Component cmp in components) {
        if (cmp == null) { continue; };
        Log.LogWrite(" " + cmp.name + ":" + cmp.GetType().ToString() + "\n");
        ParticleSystem ps = cmp as ParticleSystem;
        if (ps != null) {
          var main = ps.main;
          main.scalingMode = ParticleSystemScalingMode.Hierarchy;
          Log.LogWrite("  " + ps.main.scalingMode.ToString() + "\n");
        }
      }
      gameObject.transform.position = pos;
      gameObject.transform.localScale = new Vector3(scale.x, scale.y, scale.z);
      if (lookAtPos != Vector3.zero)
        gameObject.transform.LookAt(lookAtPos);
      else
        gameObject.transform.localRotation = Quaternion.identity;
      ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
      if (component != null) {
        gameObject.SetActive(true);
        component.Stop(true);
        component.Clear(true);
        component.transform.position = pos;
        if (lookAtPos != Vector3.zero)
          component.transform.LookAt(lookAtPos);
        else
          component.transform.localRotation = Quaternion.identity;
        BTCustomRenderer.SetVFXMultiplier(component);
        component.Play(true);
      }
      return component;
    }
  }
  public enum MineFieldStealthLevel {
    Full,Partial,Invisible
  }
  public class MineField {
    public int count;
    public string UIName;
    public AbstractActor owner;
    public Weapon weapon;
    public MineFieldDef Def;
    public Team team;
    public int IFFLevel;
    public int stealthLvl;
    public MapTerrainHexCell hex;
    public MineField(MapTerrainHexCell hex,MineFieldDef d, AbstractActor o, Weapon w) {
      this.hex = hex;
      owner = o;
      weapon = w;
      Def = d;
      count = this.Def.Count;
      float misFire = d.MisfireOnDeployChance;
      if (misFire > 0f) {
        int functioning = 0;
        for (int i = 0; i < d.Count; i++) {
          float roll = Random.Range(0f, 1f);
          if (roll > misFire) { 
            functioning++;
          }
        }
        Log.F.WL(0, $"{Def.Count - functioning} mines misfired, only deploying {functioning}");
        count = functioning;
      }
      IFFLevel = d.IFFLevel;
      stealthLvl = d.stealthLevel;
      team = o.team;
      UIName = d.UIName;
      if (string.IsNullOrEmpty(UIName)) { UIName = w.ammo().UIName; }
      if (string.IsNullOrEmpty(UIName)) { UIName = w.ammo().Name; }
      if (string.IsNullOrEmpty(UIName)) { UIName = "Generic"; }
    }
    public bool getIFFLevel(AbstractActor unit) {
      if (IFFLevel <= 0) { return false; }
      if(unit.team == null) { return this.IFFLevel < unit.mineFieldIFFLevel(); }
      if(this.team == null) { return this.IFFLevel < unit.mineFieldIFFLevel(); }
      if (this.team.IsEnemy(unit.team)) { return this.IFFLevel < unit.mineFieldIFFLevel(); }
      return true;
    }
    public MineFieldStealthLevel stealthLevel(Team iteam) {
      if (IFFLevel > 0) {
        if ((iteam != null) && (team != null)) {
          if (this.team.IsFriendly(iteam)) {
            return MineFieldStealthLevel.Full;
          }
        }
      }
      if (iteam == null) {
        return stealthLvl>0?MineFieldStealthLevel.Invisible:MineFieldStealthLevel.Partial;
      }
      int reduction = this.hex.mineFieldStealthReduction(iteam);
      if ((reduction <= 0)&&(stealthLvl > 0)) { return MineFieldStealthLevel.Invisible; }
      int effectiveLevel = stealthLvl - this.hex.mineFieldStealthReduction(iteam);
      if (effectiveLevel <= 0) { return MineFieldStealthLevel.Full; }
      if (effectiveLevel <= 1) { return MineFieldStealthLevel.Partial; }
      return MineFieldStealthLevel.Invisible;
    }
  }
  public class tempTerrainVFXEffect {
    public int counter;
    public ObjectSpawnDataSelf vfx;
    public void tick() {
      if (counter > 1) {
        --counter;
        return;
      }
      counter = 0;
      try {
        if (vfx != null) {
          vfx.CleanupSelf();
          vfx = null;
        }
      } catch (Exception e) {
        CustomAmmoCategoriesLog.Log.LogWrite("Fail to clean temp terrain effect " + e.ToString() + "\n", true);
      }
    }
    public tempTerrainVFXEffect(CombatGameState combat, string vfxPrefab, Vector3 pos, Vector3 scale, int counter) {
      vfx = new ObjectSpawnDataSelf(vfxPrefab, pos, Quaternion.identity, scale, true, false);
      this.counter = counter;
      vfx.SpawnSelf(combat);
    }
  }
  public class MineFieldCollider : MonoBehaviour {
    public SphereCollider collider;
    public MapTerrainHexCell hex;
    public void Init(MapTerrainHexCell hex) {
      this.hex = hex;
      this.collider = gameObject.GetComponent<SphereCollider>();
    }
    public void OnTriggerEnter(Collider other) {
      MineFieldDetector detector = other.gameObject.GetComponent<MineFieldDetector>();
      if (detector == false) { return; }
      Log.M.TWL(0, "MineFieldCollider.OnTriggerEnter " + other.name+" detector:"+(detector==null?"false":"true"));
      hex.AddMineFieldStealthReductor(detector.owner);
      hex.UpdateIndicatorVisibility();
    }
    public void OnTriggerExit(Collider other) {
      MineFieldDetector detector = other.gameObject.GetComponent<MineFieldDetector>();
      if (detector == false) { return; }
      Log.M.TWL(0, "MineFieldCollider.OnTriggerExit " + other.name + " detector:" + (detector == null ? "false" : "true"));
      hex.DelMineFieldStealthReductor(detector.owner);
      hex.UpdateIndicatorVisibility();
    }
  }
  public class MineFieldDetector: MonoBehaviour {
    public AbstractActor owner;
    public void Init(AbstractActor o) {
      this.owner = o;
    }
  }
  public class MapTerrainHexCell {
    //public int x;
    //public int y;
    //public int mapX;
    //public int mapY;
    public Vector3 center;
    public MapTerrainDataCellEx centerCell;
    public List<MapTerrainDataCellEx> terrainCells;
    public Dictionary<string, tempTerrainVFXEffect> tempVFXEffects;
    public ObjectSpawnDataSelf burnEffect;
    public CombatGameState Combat;
    public Weapon burningWeapon;
    public bool isHasForest;
    public bool wasHasForest;
    public bool expandingThisTurn;
    public int burnEffectCounter;
    public List<MineField> MineFields;
    public GameObject mineFieldVFX;
    private HashSet<AbstractActor> minefieldStealthReductors;
    private Dictionary<Team, int> minefieldStealthReductionCache;
    public void UpdateIndicatorVisibility() {
      if (mineFieldVFX == null) { return; }
      bool hasVisibleMineFields = false;
      Log.M.TWL(0, "UpdateIndicatorVisibility "+this.center);
      //int reduction = this.mineFieldStealthReduction(Combat.LocalPlayerTeam);
      foreach(MineField mf in MineFields) {
        if (mf.count <= 0) { continue; }
        MineFieldStealthLevel level = mf.stealthLevel(Combat.LocalPlayerTeam);
        Log.M.WL(1, "stealth level "+level+" IFF:"+mf.IFFLevel);
        if (level != MineFieldStealthLevel.Invisible) { hasVisibleMineFields = true; break; }
      }
      if (hasVisibleMineFields) {
        UIMovementDot dot = mineFieldVFX.GetComponentInChildren<UIMovementDot>(true);
        Log.M.WL(1, "making cell visible " + (dot==null?"null":"not null"));
        if (dot != null) { dot.gameObject.SetActive(true); }
      } else {
        UIMovementDot dot = mineFieldVFX.GetComponentInChildren<UIMovementDot>(true);
        Log.M.WL(1, "making cell hidden " + (dot == null ? "null" : "not null"));
        if (dot != null) { dot.gameObject.SetActive(false); }
      }
    }
    public void AddMineFieldStealthReductor(AbstractActor actor) {
      minefieldStealthReductionCache.Clear();
      minefieldStealthReductors.Add(actor);
      Log.M.TWL(0, "AddMineFieldStealthReductor "+actor.DisplayName);
      Log.M.WL(1, "local player reduction: " + this.mineFieldStealthReduction(Combat.LocalPlayerTeam));
    }
    public void DelMineFieldStealthReductor(AbstractActor actor) {
      minefieldStealthReductionCache.Clear();
      minefieldStealthReductors.Remove(actor);
      Log.M.TWL(0, "DelMineFieldStealthReductor " + actor.DisplayName);
      Log.M.WL(1, "local player reduction: " + this.mineFieldStealthReduction(Combat.LocalPlayerTeam));
    }
    public int mineFieldStealthReduction(Team team) {
      if (team == null) { return 0; }
      if (minefieldStealthReductionCache.TryGetValue(team,out int reduction)) {
        return reduction;
      }
      foreach (AbstractActor actor in minefieldStealthReductors) {
        if (actor.team == null) { continue; }
        if (actor.team.IsFriendly(team) == false) { continue; }
        int curRed = actor.MinefieldDetectionLevel();
        if (reduction < curRed) { reduction = curRed; }
      }
      minefieldStealthReductionCache.Add(team, reduction);
      return reduction;
    }

    public void AddMineFieldSync(MineField mf)
    {
      if (mf.Def.ShouldAddToExistingFields && this.MineFields.Any(x => x.Def.MinefieldDefID == mf.Def.MinefieldDefID && x.team == mf.team)) { 
        MineField mineField = this.MineFields.First(x => x.Def.MinefieldDefID == mf.Def.MinefieldDefID && x.team == mf.team);
        Log.F.WL(0, $"Found existing minefield with ID {mf.Def.MinefieldDefID} for team {mf.team.DisplayName}; adding {mf.Def.Count} to current mineField count of {mineField.count}");
        mineField.count += mf.count;
        return;
      } 
      this.MineFields.Add(mf);
      if(mineFieldVFX == null) {
        //Point p = new Point();
        //p.X = this.mapY;
        //p.Z = this.mapX;
        Vector3 pos = this.center;
        pos.y = Combat.MapMetaData.GetLerpedHeightAt(pos);
        mineFieldVFX = new GameObject("minefield");
        mineFieldVFX.SetActive(false);
        mineFieldVFX.transform.SetParent(CombatMovementReticle.Instance.transform);
        GameObject mfCollider = new GameObject("minefieldCollider");
        mfCollider.AddComponent<Rigidbody>();
        mfCollider.GetComponent<Rigidbody>().name = "minefieldCollider";
        mfCollider.GetComponent<Rigidbody>().isKinematic = true;
        mfCollider.GetComponent<Rigidbody>().useGravity = false;
        mfCollider.gameObject.layer = LayerMask.NameToLayer("NoCollision");
        SphereCollider collider = mfCollider.AddComponent<SphereCollider>();
        collider.name = "minefieldCollider";
        collider.radius = 0.1f;//Combat.HexGrid.HexWidth;
        collider.isTrigger = true;
        collider.enabled = true;
        mfCollider.transform.SetParent(mineFieldVFX.transform);
        mfCollider.transform.localPosition = new Vector3(0f, -0.1f, 0f);
        mfCollider.AddComponent<MineFieldCollider>();
        mfCollider.GetComponent<MineFieldCollider>().Init(this);

        //GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //sphere.transform.SetParent(mineFieldVFX.transform);
        //sphere.transform.localPosition = Vector3.zero;
        //sphere.transform.localScale = Vector3.one * ((float)CustomAmmoCategories.Settings.BurningForestCellRadius*2.0f) * 4.0f;

        GameObject indicator = GameObject.Instantiate(CombatMovementReticle.Instance.dangerousDotTemplate);
        indicator.transform.SetParent(mineFieldVFX.transform);
        indicator.transform.localRotation = Quaternion.Euler(90f, 0.0f, 0.0f);
        indicator.transform.localScale *= 2f;
        indicator.SetActive(false);
        mineFieldVFX.transform.position = pos;
        indicator.transform.localPosition = new Vector3(0f, 3f, 0f);
        mineFieldVFX.SetActive(true);
      } else {
        //UIMovementDot dot = mineFieldVFX.GetComponentInChildren<UIMovementDot>();
        //mineFieldVFX.SetActive(true);
      }
      UpdateIndicatorVisibility();
      //SVGRenderer renderer = mineFieldVFX.GetComponentInChildren<SVGRenderer>();
      //if (renderer != null) { renderer.vectorGraphics = CustomSvgCache.get(mf.Def.Icon, Combat.DataManager); }
    }
    public void ClearMineFieldSync(int count,float chance) {
      this.MineFields.Clear();
      UpdateIndicatorVisibility();
    }
    public void deleteTrees(HashSet<object> redrawTreeDatas) {
      foreach (MapTerrainDataCellEx cell in terrainCells) {
        CustomAmmoCategoriesLog.Log.LogWrite("Deleting trees at cell " + cell.x + ":" + cell.y + " " + cell.mapMetaData.getWorldPos(new Point(cell.y, cell.x)) + " count:" + cell.trees.Count + "\n");
        foreach (CACDynamicTree tree in cell.trees) {
          List<object> redrawList = tree.delTree();
          foreach (object redrawItem in redrawList) {
            redrawTreeDatas.Add(redrawItem);
          }
        }
      }
    }
    //public MapPoint mapPoint() {
      //return new MapPoint(this.x, this.y);
    //}
    public bool hasTempEffects() {
      return tempVFXEffects.Count > 0;
    }
    public void tempVFXTick() {
      CustomAmmoCategoriesLog.Log.LogWrite("tempVFXTick:" + this.center + "\n");
      HashSet<string> delVFXs = new HashSet<string>();
      foreach (var tvfx in this.tempVFXEffects) {
        tvfx.Value.tick();
        if (tvfx.Value.counter <= 0) { delVFXs.Add(tvfx.Key); };
      }
      foreach (string dvfx in delVFXs) {
        tempVFXEffects.Remove(dvfx);
      }
    }
    public void clearTempVFXs() {
      foreach (var tvfx in this.tempVFXEffects) {
        try {
          if (tvfx.Value.vfx != null) {
            tvfx.Value.vfx.CleanupSelf();
          }
        } catch (Exception e) {
          CustomAmmoCategoriesLog.Log.LogWrite("Fail to clear vfx:" + tvfx.Key + ":" + e.ToString() + "\n");
        }
      }
    }
    public void addTempTerrainVFXSync(CombatGameState combat, string prefabVFX, int counter, Vector3 scale) {
      this.Combat = combat;
      Log.M.WL(0,"addTempTerrainVFX(" + prefabVFX + "," + counter + ")");
      if (tempVFXEffects == null) {
        Log.M.WL(1,"tempVFXEffects is null");
        return;
      }
      if (string.IsNullOrEmpty(prefabVFX) == false) {
        if (tempVFXEffects.ContainsKey(prefabVFX) == true) {
          tempVFXEffects[prefabVFX].counter += counter;
        } else {
          //Point p = new Point();
          //p.X = this.mapY;
          //p.Z = this.mapX;
          Vector3 pos = this.center;//Combat.MapMetaData.getWorldPos(p);
          pos.y = Combat.MapMetaData.GetLerpedHeightAt(pos);
          Log.M.WL(1, "position:"+ pos);
          tempTerrainVFXEffect tmpEffect = new tempTerrainVFXEffect(Combat, prefabVFX, pos, scale, counter);
          tempVFXEffects.Add(prefabVFX, tmpEffect);
          DynamicMapHelper.tempEffectHexes.Add(this);
        }
      }
    }
    public void addTempTerrainMask(DesignMaskDef addMask, int counter) {
      if (addMask != null) {
        CustomAmmoCategoriesLog.Log.LogWrite("addTempTerrainMask(" + addMask.Description.Id + "," + counter + ")\n");
        foreach (MapTerrainDataCellEx cell in this.terrainCells) {
          if (cell == null) { continue; }
          cell.AddDesignMask(addMask, counter);
        }
      }
    }
    public int CountTrees() {
      int result = 0;
      foreach (MapTerrainDataCellEx cell in this.terrainCells) {
        result += cell.trees.Count;
      }
      return result;
    }
    public void applyBurnOutVisuals() {
      burnEffect.CleanupSelf();
      //Point p = new Point();
      //p.X = this.mapY;
      //p.Z = this.mapX;
      Vector3 pos = this.center;//Combat.MapMetaData.getWorldPos(p);
      pos.y = Combat.MapMetaData.GetLerpedHeightAt(pos);
      float scale = CustomAmmoCategories.Settings.BurnedTrees.DecalScale;
      if (wasHasForest) {
        pos.x += CustomAmmoCategories.Settings.BurnedOffsetX;
        pos.y += CustomAmmoCategories.Settings.BurnedOffsetY;
        pos.z += CustomAmmoCategories.Settings.BurnedOffsetZ;
        BTCustomRenderer_DrawDecals.AddScorch(pos, new Vector3(1f, 0f, 0f).normalized, new Vector3(scale, scale, scale));
      } else {
        BTCustomRenderer_DrawDecals.AddScorch(pos, new Vector3(1f, 0f, 0f).normalized, new Vector3(scale, scale, scale));
      }
    }
    public void applyBurnVisuals() {
      //Point p = new Point();
      //p.X = this.mapY;
      //p.Z = this.mapX;
      Vector3 pos = this.center;//Combat.MapMetaData.getWorldPos(p);
      pos.y = Combat.MapMetaData.GetLerpedHeightAt(pos);
      CustomAmmoCategoriesLog.Log.LogWrite("Spawning fire at " + pos + "\n");
      pos.x += CustomAmmoCategories.Settings.BurningOffsetX;
      pos.y += CustomAmmoCategories.Settings.BurningOffsetY;
      pos.z += CustomAmmoCategories.Settings.BurningOffsetZ;
      if (burnEffect != null) { burnEffect.CleanupSelf(); };
      if (CustomAmmoCategories.Settings.ForceBuildinBurningFX == false) {
        burnEffect = DynamicMapHelper.SpawnFXObject(Combat, CustomAmmoCategories.Settings.BurningFX, pos, new Vector3(CustomAmmoCategories.Settings.BurningScaleX, CustomAmmoCategories.Settings.BurningScaleY, CustomAmmoCategories.Settings.BurningScaleZ));
      } else {
        burnEffect = DynamicMapHelper.SpawnFXObject(Combat, CustomAmmoCategories.Settings.BuildinBurningFX, pos, new Vector3(CustomAmmoCategories.Settings.BuildinBurningScaleX, CustomAmmoCategories.Settings.BuildinBurningScaleY, CustomAmmoCategories.Settings.BuildinBurningScaleZ));
        if (CustomAmmoCategories.Settings.BuildinBurningFXDisableSmoke) {
          burnEffect.DisableObjects(CustomAmmoCategories.Settings.BuildinBurningFXSmokeObjects);
        }
      }
    }
    public void UpdateCellsBurn(Weapon weapon, int count, int strength) {
      foreach (MapTerrainDataCellEx cell in terrainCells) {
        cell.burnUpdate(weapon, count, strength);
      }
    }
    public void SetCellsBurn(Weapon weapon, int count, int countNoForest, int strength, int strengthNoForest) {
      foreach (MineField mf in MineFields) {
        if (mf.count <= 0) { continue; }
        if (mf.Def.burnReaction == MinefieldBurnReaction.None) { continue; }
        switch (mf.Def.burnReaction) {
          case MinefieldBurnReaction.Destroy: mf.count = 0; break;
          case MinefieldBurnReaction.Explode: mf.count = 0; break;
          case MinefieldBurnReaction.LooseElectronic: mf.IFFLevel = 0; mf.stealthLvl = 1; break;
        }
      }
      this.UpdateIndicatorVisibility();
      foreach (MapTerrainDataCellEx cell in terrainCells) {
        CustomAmmoCategoriesLog.Log.LogWrite("SetCellsBurn:" + cell.x + ":" + cell.y + ":" + SplatMapInfo.IsForest(cell.terrainMask) + ":" + cell.CantHaveForest + "\n");
        if (SplatMapInfo.IsForest(cell.terrainMask) && (cell.CantHaveForest == false)) {
          if ((count == 0) || (strength == 0)) { continue; }
          cell.burnUpdate(weapon, count, strength);
          PersistentMapClientHelper.FloatAdd("BurnDamage", count * strength);
        } else {
          if ((countNoForest == 0) || (strengthNoForest == 0)) { continue; }
          cell.burnUpdate(weapon, countNoForest, strengthNoForest);
          PersistentMapClientHelper.FloatAdd("BurnDamage", countNoForest * strengthNoForest);
        }
      }
    }
    public bool TryBurnCellSync(Weapon weapon, float FireTerrainChance, int FireTerrainStrength, int FireDurationWithoutForest) {
      CustomAmmoCategoriesLog.Log.LogWrite("Try burn cell " + weapon.Name + " Chance:" + FireTerrainChance + " hasForest:" + isHasForest + "\n");
      if (FireTerrainChance > CustomAmmoCategories.Epsilon) {
        if ((FireDurationWithoutForest <= 0) && (this.isHasForest == false)) {
          CustomAmmoCategoriesLog.Log.LogWrite(" no forest and no self burn\n");
          return false;
        }
        float roll = Random.Range(0f, 1f);
        if (roll > FireTerrainChance) {
          CustomAmmoCategoriesLog.Log.LogWrite(" roll fail:" + roll + "\n");
          return false;
        } else {
          CustomAmmoCategoriesLog.Log.LogWrite(" roll success:" + roll + "\n");
        }
      } else {
        return false;
      }
      if (burnEffectCounter > 0) {
        CustomAmmoCategoriesLog.Log.LogWrite(" already burning\n");
        if (burnEffectCounter < FireDurationWithoutForest) {
          burnEffectCounter = FireDurationWithoutForest;
          this.UpdateCellsBurn(weapon, burnEffectCounter, FireTerrainStrength);
          this.expandingThisTurn = true;
          burningWeapon = weapon;
        }
        return false;
      }
      burnEffectCounter = FireDurationWithoutForest;
      if (isHasForest && (burnEffectCounter < DynamicMapHelper.BurnForestDuration())) { burnEffectCounter = DynamicMapHelper.BurnForestDuration(); };
      if (burnEffectCounter <= 0) { return false; };
      int burnStrength = FireTerrainStrength;
      if (isHasForest && (burnStrength < DynamicMapHelper.BurnForestStrength())) { burnStrength = DynamicMapHelper.BurnForestStrength(); };
      if (burnStrength <= 0) { return false; };
      burningWeapon = weapon;
      Combat = weapon.parent.Combat;
      expandingThisTurn = true;
      applyBurnVisuals();
      SetCellsBurn(weapon, burnEffectCounter, FireDurationWithoutForest, burnStrength, FireTerrainStrength);
      isHasForest = false;
      return true;
    }
    public bool TryExpand(Weapon weapon) {
      CustomAmmoCategoriesLog.Log.LogWrite("  test expand:" + this.center + "\n");
      if (isHasForest == false) {
        CustomAmmoCategoriesLog.Log.LogWrite("  no forest\n");
        return false;
      };
      if (burnEffectCounter > 0) {
        CustomAmmoCategoriesLog.Log.LogWrite("  burning already\n");
        return false;
      };
      float roll = Random.Range(0f, 1f);
      if (roll > DynamicMapHelper.FireExpandChance()) {
        CustomAmmoCategoriesLog.Log.LogWrite("  roll fail\n");
        return false;
      };
      burningWeapon = weapon;
      Combat = weapon.parent.Combat;
      applyBurnVisuals();
      burnEffectCounter = DynamicMapHelper.BurnForestDuration();
      SetCellsBurn(weapon, burnEffectCounter, 0, DynamicMapHelper.BurnForestStrength(), 0);
      isHasForest = false;
      expandingThisTurn = false;
      return true;
    }
    public bool isBurning() {
      return burnEffectCounter > 0;
    }
    public bool FireTick() {
      foreach (MapTerrainDataCellEx cell in terrainCells) { cell.burnTick(); };
      if (this.burnEffectCounter > 1) {
        --this.burnEffectCounter;
        return false;
      }
      if (this.burnEffectCounter <= 0) { return false; };
      this.burnEffectCounter = 0;
      this.applyBurnOutVisuals();
      return true;
    }
    public MapTerrainHexCell() {
      //x = 0;
      //y = 0;
      //mapX = 0;
      //mapY = 0;
      center = Vector3.zero;
      terrainCells = new List<MapTerrainDataCellEx>();
      tempVFXEffects = new Dictionary<string, tempTerrainVFXEffect>();
      burnEffect = null;
      isHasForest = false;
      wasHasForest = false;
      burnEffectCounter = 0;
      Combat = null;
      expandingThisTurn = false;
      burningWeapon = null;
      MineFields = new List<MineField>();
      mineFieldVFX = null;
      minefieldStealthReductors = new HashSet<AbstractActor>();
      minefieldStealthReductionCache = new Dictionary<Team, int>();
      centerCell = null;
    }
    public static List<MapTerrainHexCell> listHexCellsByCellRadius(MapTerrainDataCellEx ccell, int r) {
      //HashSet<MapPoint> hexCells = new HashSet<MapPoint>();
      HashSet<MapTerrainHexCell> result = new HashSet<MapTerrainHexCell>();
      List<MapPoint> affectedCells = MapPoint.calcMapCircle(new MapPoint(ccell.x, ccell.y), r);
      foreach (MapPoint aCell in affectedCells) {
        if (aCell.x < 0) { continue; }
        if (aCell.y < 0) { continue; }
        if (aCell.x >= ccell.mapMetaData.mapTerrainDataCells.GetLength(0)) { continue; }
        if (aCell.y >= ccell.mapMetaData.mapTerrainDataCells.GetLength(1)) { continue; }
        MapTerrainDataCellEx cell = ccell.mapMetaData.mapTerrainDataCells[aCell.x, aCell.y] as MapTerrainDataCellEx;
        if (cell == null) { continue; }
        //MapPoint hexCell = new MapPoint(cell.hexCell.x, cell.hexCell.y);
        //if (hexCells.Contains(hexCell)) { continue; };
        //hexCells.Add(hexCell);
        if (cell.hexCell == null) { continue; }
        result.Add(cell.hexCell);
      }
      return result.ToList();
    }
  }
  public class MapTerrainDataCellEx : MapTerrainDataCell {
    public static bool ApplyPendingMasks { get; set; } = false;
    public int x;
    public int y;
    public float realTerrainHeight;
    public TerrainMaskFlags pendingMasks;
    //public float RealHeight;
    public bool waterLevelCached;
    public bool wasForest;
    public bool wasCustom;
    public bool wasRoad;
    public DesignMaskDef CustomDesignMask;
    public int BurningCounter;
    public int BurningStrength;
    public Weapon BurningWeapon;
    public bool CantHaveForest;
    public MapTerrainHexCell hexCell;
    public Dictionary<DesignMaskDef, int> tempDesignMaskCounters;
    public DesignMaskDef tempDesignMask;
    public List<CACDynamicTree> trees;
    public MapPoint mapPoint() {
      return new MapPoint(this.x, this.y);
    }
    public Point GetPoint() {
      return new Point(this.y, this.x);
    }
    public Vector3 WorldPos() {
      return this.mapMetaData.getWorldPos(this.GetPoint());
    }
    public void DesignMaskDeepCopyData(DesignMaskDef source, DesignMaskDef dest) {
      dest.hideInUI = source.hideInUI;
      dest.moveCostMechLight = source.moveCostMechLight;
      dest.moveCostMechMedium = source.moveCostMechMedium;
      dest.moveCostMechHeavy = source.moveCostMechHeavy;
      dest.moveCostMechAssault = source.moveCostMechAssault;
      dest.moveCostTrackedLight = source.moveCostTrackedLight;
      dest.moveCostTrackedMedium = source.moveCostTrackedMedium;
      dest.moveCostTrackedHeavy = source.moveCostTrackedHeavy;
      dest.moveCostTrackedAssault = source.moveCostTrackedAssault;
      dest.moveCostWheeledLight = source.moveCostWheeledLight;
      dest.moveCostWheeledMedium = source.moveCostWheeledMedium;
      dest.moveCostWheeledHeavy = source.moveCostWheeledHeavy;
      dest.moveCostWheeledAssault = source.moveCostWheeledAssault;
      dest.moveCostSprintMultiplier = source.moveCostSprintMultiplier;
      dest.stabilityDamageMultiplier = source.stabilityDamageMultiplier;
      dest.visibilityMultiplier = source.visibilityMultiplier;
      dest.visibilityHeight = source.visibilityHeight;
      dest.signatureMultiplier = source.signatureMultiplier;
      dest.sensorRangeMultiplier = source.sensorRangeMultiplier;
      dest.targetabilityModifier = source.targetabilityModifier;
      dest.meleeTargetabilityModifier = source.meleeTargetabilityModifier;
      dest.grantsGuarded = source.grantsGuarded;
      dest.grantsEvasive = source.grantsEvasive;
      dest.toHitFromModifier = source.toHitFromModifier;
      dest.heatSinkMultiplier = source.heatSinkMultiplier;
      dest.heatPerTurn = source.heatPerTurn;
      dest.legStructureDamageMin = source.legStructureDamageMin;
      dest.legStructureDamageMax = source.legStructureDamageMax;
      dest.canBurn = source.canBurn;
      dest.canExplode = source.canExplode;
      dest.allDamageDealtMultiplier = source.allDamageDealtMultiplier;
      dest.allDamageTakenMultiplier = source.allDamageTakenMultiplier;
      dest.antipersonnelDamageDealtMultiplier = source.antipersonnelDamageDealtMultiplier;
      dest.antipersonnelDamageTakenMultiplier = source.antipersonnelDamageTakenMultiplier;
      dest.energyDamageDealtMultiplier = source.energyDamageDealtMultiplier;
      dest.energyDamageTakenMultiplier = source.energyDamageTakenMultiplier;
      dest.ballisticDamageDealtMultiplier = source.ballisticDamageDealtMultiplier;
      dest.ballisticDamageTakenMultiplier = source.ballisticDamageTakenMultiplier;
      dest.missileDamageDealtMultiplier = source.missileDamageDealtMultiplier;
      dest.missileDamageTakenMultiplier = source.missileDamageTakenMultiplier;
      dest.audioSwitchSurfaceType = source.audioSwitchSurfaceType;
      dest.audioSwitchRainingSurfaceType = source.audioSwitchRainingSurfaceType;
      dest.customBiomeAudioSurfaceType = source.customBiomeAudioSurfaceType;
      dest.stickyEffect = source.stickyEffect;
      if (CustomAmmoCategories.tempDesignMasksStickyEffects.ContainsKey(dest.Id) == false) {
        CustomAmmoCategories.tempDesignMasksStickyEffects.Add(dest.Id, new List<EffectData>());
      }
      CustomDesignMaskInfo parent_customDesignMaskInfo = source.GetCustomDesignMaskInfo();
      CustomDesignMaskInfo new_customDesignMaskInfo = new CustomDesignMaskInfo(parent_customDesignMaskInfo);
      CustomAmmoCategories.customDesignMaskInfo.AddOrUpdate(dest.Id, new_customDesignMaskInfo, (k, v) => { return new_customDesignMaskInfo; });
    }
    public void DesignMaskDeepAppendData(DesignMaskDef addMask, DesignMaskDef dest) {
      dest.hideInUI = addMask.hideInUI;
      dest.moveCostMechLight += (addMask.moveCostMechLight - 1f);
      dest.moveCostMechMedium += (addMask.moveCostMechMedium - 1f);
      dest.moveCostMechHeavy += (addMask.moveCostMechHeavy - 1f);
      dest.moveCostMechAssault += (addMask.moveCostMechAssault - 1f);
      dest.moveCostTrackedLight += (addMask.moveCostTrackedLight - 1f);
      dest.moveCostTrackedMedium += (addMask.moveCostTrackedMedium - 1f);
      dest.moveCostTrackedHeavy += (addMask.moveCostTrackedHeavy - 1f);
      dest.moveCostTrackedAssault += (addMask.moveCostTrackedAssault - 1f);
      dest.moveCostWheeledLight += (addMask.moveCostWheeledLight - 1f);
      dest.moveCostWheeledMedium += (addMask.moveCostWheeledMedium - 1f);
      dest.moveCostWheeledHeavy += (addMask.moveCostWheeledHeavy - 1f);
      dest.moveCostWheeledAssault += (addMask.moveCostWheeledAssault - 1f);
      dest.moveCostSprintMultiplier += (addMask.moveCostSprintMultiplier - 1f);
      dest.stabilityDamageMultiplier += (addMask.stabilityDamageMultiplier - 1f);
      dest.visibilityMultiplier += (addMask.visibilityMultiplier - 1f);
      dest.visibilityHeight += (addMask.visibilityHeight - 1f);
      dest.signatureMultiplier += (addMask.signatureMultiplier - 1f);
      dest.sensorRangeMultiplier += (addMask.sensorRangeMultiplier - 1f);
      dest.targetabilityModifier += (addMask.targetabilityModifier - 1f);
      dest.meleeTargetabilityModifier += (addMask.meleeTargetabilityModifier - 1f);
      dest.grantsGuarded = dest.grantsGuarded ? true : addMask.grantsGuarded;
      dest.grantsEvasive = dest.grantsEvasive ? true : addMask.grantsEvasive;
      dest.toHitFromModifier += addMask.toHitFromModifier;
      dest.heatSinkMultiplier += (addMask.heatSinkMultiplier - 1f);
      dest.heatPerTurn += addMask.heatPerTurn;
      dest.legStructureDamageMin += addMask.legStructureDamageMin;
      dest.legStructureDamageMax += addMask.legStructureDamageMax;
      dest.canBurn = dest.canBurn ? true : addMask.canBurn;
      dest.canExplode = dest.canExplode ? true : addMask.canExplode;
      dest.allDamageDealtMultiplier += (addMask.allDamageDealtMultiplier - 1f);
      dest.allDamageTakenMultiplier += (addMask.allDamageTakenMultiplier - 1f);
      dest.antipersonnelDamageDealtMultiplier += (addMask.antipersonnelDamageDealtMultiplier - 1f);
      dest.antipersonnelDamageTakenMultiplier += (addMask.antipersonnelDamageTakenMultiplier - 1f);
      dest.energyDamageDealtMultiplier += (addMask.energyDamageDealtMultiplier - 1f);
      dest.energyDamageTakenMultiplier += (addMask.energyDamageTakenMultiplier - 1f);
      dest.ballisticDamageDealtMultiplier += (addMask.ballisticDamageDealtMultiplier - 1f);
      dest.ballisticDamageTakenMultiplier += (addMask.ballisticDamageTakenMultiplier - 1f);
      dest.missileDamageDealtMultiplier += (addMask.missileDamageDealtMultiplier - 1f);
      dest.missileDamageTakenMultiplier += (addMask.missileDamageTakenMultiplier - 1f);
      //dest.audioSwitchSurfaceType += source.audioSwitchSurfaceType;
      //dest.audioSwitchRainingSurfaceType += source.audioSwitchRainingSurfaceType;
      //dest.customBiomeAudioSurfaceType += source.customBiomeAudioSurfaceType;
      if ((dest.stickyEffect == null) || (dest.stickyEffect.effectType == EffectType.NotSet)) {
        dest.stickyEffect = addMask.stickyEffect;
      } else if((addMask.stickyEffect != null)&&(addMask.stickyEffect.effectType != EffectType.NotSet)) { 
        if (CustomAmmoCategories.tempDesignMasksStickyEffects.TryGetValue(dest.Id, out var stickyEffects) == false) {
          stickyEffects = new List<EffectData>();
          CustomAmmoCategories.tempDesignMasksStickyEffects.Add(dest.Id, stickyEffects);
        }
        stickyEffects.Add(addMask.stickyEffect);
      }
      CustomDesignMaskInfo addMaskCustomInfo = addMask.GetCustomDesignMaskInfo();
      CustomDesignMaskInfo destCustomInfo = addMask.GetCustomDesignMaskInfo();
      if(addMaskCustomInfo != null) {
        if(destCustomInfo == null) {
          destCustomInfo = new CustomDesignMaskInfo(addMaskCustomInfo);
          CustomAmmoCategories.customDesignMaskInfo.AddOrUpdate(dest.Id, destCustomInfo, (k, v) => { return destCustomInfo; });
        } else {
          destCustomInfo.Merge(addMaskCustomInfo);
        }
      }
    }
    private static MethodInfo designMaskDefs_Add = null;
    public DesignMaskDef CreateMask(DesignMaskDef baseMask, HashSet<DesignMaskDef> additionalMasks) {
      try {
        StringBuilder id = new StringBuilder();
        StringBuilder name = new StringBuilder();
        StringBuilder details = new StringBuilder();
        string icon = string.Empty;
        if (baseMask != null) {
          name.Append(baseMask.Description.Name);
          id.Append(baseMask.Description.Id);
          details.Append(baseMask.Description.Details);
          icon = baseMask.Description.Icon;
        }
        foreach (DesignMaskDef addMask in additionalMasks) {
          if (string.IsNullOrEmpty(icon)) { icon = addMask.Description.Icon; }
          id.Append(addMask.Id);
          if (name.Length > 0) { name.Append(" "); }; name.Append(addMask.Description.Name);
          if (details.Length > 0) { details.Append("\n"); }; details.Append(addMask.Description.Details);
        }
        Log.M.TWL(0, "CreateMask:"+id);
        if (this.hexCell.Combat.DataManager.DesignMaskDefs.TryGet(id.ToString(), out DesignMaskDef result)) {
          Log.M.WL(1, "found in data manager");
          return result;
        } else {
          Log.M.WL(1, "DesignMask not found in data manager");
          Log.M.WL(2, "name:" + name.ToString());
          Log.M.WL(2, "details:" + details.ToString());
        }
        result = new DesignMaskDef();
        Traverse.Create(result.Description).Property<string>("Id").Value = id.ToString();
        Traverse.Create(result.Description).Property<string>("Name").Value = name.ToString();
        Traverse.Create(result.Description).Property<string>("Details").Value = details.ToString();
        Traverse.Create(result.Description).Property<string>("Icon").Value = icon;
        bool inited = false;
        if(baseMask != null) {
          inited = true;
          Log.M.WL(1, "setup base mask as "+ baseMask.Id);
          DesignMaskDeepCopyData(baseMask, result);
        }
        foreach(DesignMaskDef tempMask in additionalMasks) {
          try {
            if (inited == false) {
              inited = true;
              Log.M.WL(1, "setup base mask as " + tempMask.Id);
              DesignMaskDeepCopyData(tempMask, result);
            } else {
              Log.M.WL(1, "append mask as " + tempMask.Id);
              DesignMaskDeepAppendData(tempMask, result);
            }
          }catch(Exception e) {
            Log.M.TWL(0, e.ToString(), true);
          }
        }
        Log.M.WL(1, "registering in data manager");
        if (designMaskDefs_Add == null) {
          designMaskDefs_Add = this.hexCell.Combat.DataManager.DesignMaskDefs.GetType().GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);
        }
        if (designMaskDefs_Add == null) {
          Log.M.WL(0, "!!can't find add method!!",true);
        } else {
          designMaskDefs_Add.Invoke(this.hexCell.Combat.DataManager.DesignMaskDefs, new object[] { result.Id, result });
        }
        Log.M.WL(0,"Resulting mask:\n"+result.ToJSON());
        return result;
      } catch (Exception e) {
        Log.M.TWL(0,e.ToString(),true);
      }
      return null;
    }
    public void AddDesignMask(DesignMaskDef addMask, int counter) {
      Log.M.TWL(0,"AddDesignMask(" + addMask.Id + "," + counter + "):" + this.x + ":" + this.y);
      if (counter <= 0) { return; }
      if (tempDesignMaskCounters.ContainsKey(addMask) == true) {
        tempDesignMaskCounters[addMask] += counter;
        Log.M.WL(1,"+time:" + tempDesignMaskCounters[addMask]);
        return;
      }
      Log.M.WL(1,"new mask");
      this.tempDesignMaskCounters.Add(addMask, counter);
      DesignMaskDef tempMask = this.tempDesignMask;
      this.tempDesignMask = null;
      DesignMaskDef baseDesignMask = this.mapMetaData.GetPriorityDesignMask(this);
      DesignMaskDef newDesignMask = this.CreateMask(baseDesignMask, this.tempDesignMaskCounters.Keys.ToHashSet());
      if(newDesignMask != null) {
        this.tempDesignMask = newDesignMask;
      } else {
        this.tempDesignMask = tempMask;
        this.tempDesignMaskCounters.Remove(addMask);
      }
      //List<string> maskId = this.tempDesignMaskCounters.Keys.ToList<string>();
      //CustomAmmoCategoriesLog.Log.LogWrite(" already have masks:" + maskId.Count + "\n");
      //maskId.Sort();
      //DesignMaskDef curMask = this.mapMetaData.GetPriorityDesignMask(this);
      //if (curMask != null) { if (maskId.Count == 0) { maskId.Add(curMask.Id); }; };
      //CustomAmmoCategoriesLog.Log.LogWrite(" curmask " + ((curMask == null) ? "null" : curMask.Id) + ":" + this.terrainMask + "\n");
      //tempDesignMask = CustomAmmoCategories.createDesignMask(maskId, curMask, addMask);
      //CustomAmmoCategoriesLog.Log.LogWrite(" new mask " + ((tempDesignMask == null) ? "null" : tempDesignMask.Id) + "\n");
      //if (curMask != null) {
      //  if (curMask.Id != tempDesignMask.Id) {
      //    this.tempDesignMaskCounters.Add(addMask.Id, counter);
      //  }
      //} else {
      //  this.tempDesignMaskCounters.Add(addMask.Id, counter);
      //}
      DynamicMapHelper.tempMaskCells.Add(this.mapPoint());
    }
    public void ReconstructTempDesignMask() {
      try {
        Log.M.TWL(0, "Reconstructing design mask:" + this.x + ":" + this.y);
        if (this.tempDesignMaskCounters.Count == 0) {
          Log.M.WL(1, "no reconstruction needed. Nullify temp mask");
          this.tempDesignMask = null;
          return;
        }
        DesignMaskDef tempMask = this.tempDesignMask;
        this.tempDesignMask = null;
        DesignMaskDef baseDesignMask = this.mapMetaData.GetPriorityDesignMask(this);
        DesignMaskDef newDesignMask = this.CreateMask(baseDesignMask, this.tempDesignMaskCounters.Keys.ToHashSet());
        if (newDesignMask != null) {
          this.tempDesignMask = newDesignMask;
        } else {
          this.tempDesignMask = tempMask;
        }
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
    }
    //public void RemoveDesignMask(string id) {
    //  if (tempDesignMaskCounters.ContainsKey(id) == false) { return; }
    //  tempDesignMaskCounters.Remove(id);
    //  List<string> maskId = this.tempDesignMaskCounters.Keys.ToList<string>();
    //  if (maskId.Count == 0) { this.tempDesignMask = null; return; };
    //  this.ReconstructTempDesignMask();
    //}
    public void tempMaskTick() {
      Log.M.TWL(0,"Temp mask tick:" + this.x + ":" + this.y);
      HashSet<DesignMaskDef> keys = this.tempDesignMaskCounters.Keys.ToHashSet();
      foreach (DesignMaskDef tdm in keys) {
        if (this.tempDesignMaskCounters.ContainsKey(tdm) == false) { continue; };
        int counter = this.tempDesignMaskCounters[tdm];
        CustomAmmoCategoriesLog.Log.LogWrite(" " + tdm + ":" + counter + "\n");
        if (counter > 1) { this.tempDesignMaskCounters[tdm] = counter - 1; continue; };
        this.tempDesignMaskCounters[tdm] = 0;
        this.tempDesignMaskCounters.Remove(tdm);
      }
      this.ReconstructTempDesignMask();
    }
    public List<MapTerrainDataCellEx> getNearestCells() {
      List<MapTerrainDataCellEx> result = new List<MapTerrainDataCellEx>();
      if (this.x > 0) {
        result.Add(this.mapMetaData.mapTerrainDataCells[this.x - 1, this.y] as MapTerrainDataCellEx);
        if (this.y > 0) { result.Add(this.mapMetaData.mapTerrainDataCells[this.x - 1, this.y - 1] as MapTerrainDataCellEx); };
        if (this.y < (this.mapMetaData.mapTerrainDataCells.GetLength(1) - 1)) { result.Add(this.mapMetaData.mapTerrainDataCells[this.x - 1, this.y + 1] as MapTerrainDataCellEx); };
      };
      if (this.y > 0) {
        result.Add(this.mapMetaData.mapTerrainDataCells[this.x, this.y - 1] as MapTerrainDataCellEx);
        if (this.x < (this.mapMetaData.mapTerrainDataCells.GetLength(0) - 1)) {
          result.Add(this.mapMetaData.mapTerrainDataCells[this.x + 1, this.y - 1] as MapTerrainDataCellEx);
        };
      };
      if (this.x < (this.mapMetaData.mapTerrainDataCells.GetLength(0) - 1)) {
        result.Add(this.mapMetaData.mapTerrainDataCells[this.x + 1, this.y] as MapTerrainDataCellEx);
      }
      if (this.y < (this.mapMetaData.mapTerrainDataCells.GetLength(1) - 1)) {
        result.Add(this.mapMetaData.mapTerrainDataCells[this.x, this.y + 1] as MapTerrainDataCellEx);
        if (this.x < (this.mapMetaData.mapTerrainDataCells.GetLength(0) - 1)) { result.Add(this.mapMetaData.mapTerrainDataCells[this.x + 1, this.y + 1] as MapTerrainDataCellEx); };
      };
      return result;
    }
    public void burnTick() {
      if (this.BurningCounter > 1) {
        --this.BurningCounter;
        return;
      }
      if (this.BurningCounter == 0) { return; };
      this.BurningCounter = 0;
      this.BurningStrength = 0;
      this.CustomDesignMask = null;
      if (this.wasCustom == false) { this.RemoveTerrainMask(TerrainMaskFlags.Custom); };
      if (this.wasRoad == true) { this.AddTerrainMask(TerrainMaskFlags.Road); };
      if (this.wasForest == true) {
        if (this.CantHaveForest == false) {
          if (DynamicMapHelper.loadedMasksDef.ContainsKey(CustomAmmoCategories.Settings.BurnedForestDesignMask) == true) {
            this.AddTerrainMask(TerrainMaskFlags.Custom);
            this.CustomDesignMask = DynamicMapHelper.loadedMasksDef[CustomAmmoCategories.Settings.BurnedForestDesignMask];
            if (CombatHUDMiniMap.instance != null) { CombatHUDMiniMap.instance.AddBurned(this); }
          }
        } else {
          this.AddTerrainMask(TerrainMaskFlags.Forest);
          if (CombatHUDMiniMap.instance != null) { CombatHUDMiniMap.instance.AddRestore(this); }
        }
      } else {
        if (CombatHUDMiniMap.instance != null) { CombatHUDMiniMap.instance.AddRestore(this); }
      }
      this.ReconstructTempDesignMask();
    }
    public void burnUpdate(Weapon weapon, int counter, int strength) {
      if (this.BurningCounter > 0) {
        if (this.BurningCounter < counter) { this.BurningCounter = counter; this.BurningWeapon = weapon; };
        if (this.BurningStrength < strength) { this.BurningStrength = strength; this.BurningWeapon = weapon; };
      } else {
        this.burn(weapon, counter, strength);
      }
    }
    public void burn(Weapon weapon, int counter, int strength) {
      CustomAmmoCategoriesLog.Log.LogWrite("burn cell " + this.x + ":" + this.y + ": is forest: " + SplatMapInfo.IsForest(this.terrainMask) + " cantforest:" + this.CantHaveForest + " trees count:" + this.trees.Count + "\n");
      if (SplatMapInfo.IsForest(this.terrainMask)) {
        if (this.CantHaveForest) {
          if ((counter > 0) && (strength > 0)) {
            this.BurningWeapon = weapon;
            this.wasForest = true;
            this.wasCustom = false;
            this.wasRoad = false;
            this.AddTerrainMask(TerrainMaskFlags.Custom);
            this.RemoveTerrainMask(TerrainMaskFlags.Forest);
            if (DynamicMapHelper.loadedMasksDef.ContainsKey(CustomAmmoCategories.Settings.BurningTerrainDesignMask) == true) {
              this.CustomDesignMask = DynamicMapHelper.loadedMasksDef[CustomAmmoCategories.Settings.BurningTerrainDesignMask];
              this.ReconstructTempDesignMask();
            }
            this.BurningCounter = counter;
            this.BurningStrength = strength;
          }
        } else {
          this.BurningWeapon = weapon;
          this.RemoveTerrainMask(TerrainMaskFlags.Forest);
          this.wasForest = true;
          this.wasCustom = false;
          this.wasRoad = false;
          if (DynamicMapHelper.loadedMasksDef.ContainsKey(CustomAmmoCategories.Settings.BurningForestDesignMask) == true) {
            this.AddTerrainMask(TerrainMaskFlags.Custom);
            this.CustomDesignMask = DynamicMapHelper.loadedMasksDef[CustomAmmoCategories.Settings.BurningForestDesignMask];
            this.ReconstructTempDesignMask();
          }
          this.BurningCounter = (counter > DynamicMapHelper.BurnForestDuration()) ? counter : DynamicMapHelper.BurnForestDuration();
          this.BurningStrength = (strength > DynamicMapHelper.BurnForestStrength()) ? strength : DynamicMapHelper.BurnForestStrength();
        }
      } else
      if ((counter > 0) && (strength > 0)) {
        this.BurningWeapon = weapon;
        //this.wasForest = false;
        this.wasCustom = SplatMapInfo.IsCustom(this.terrainMask);
        this.wasRoad = SplatMapInfo.IsRoad(this.terrainMask);
        this.AddTerrainMask(TerrainMaskFlags.Custom);
        this.RemoveTerrainMask(TerrainMaskFlags.Road);
        if (DynamicMapHelper.loadedMasksDef.ContainsKey(CustomAmmoCategories.Settings.BurningTerrainDesignMask) == true) {
          this.CustomDesignMask = DynamicMapHelper.loadedMasksDef[CustomAmmoCategories.Settings.BurningTerrainDesignMask];
          this.ReconstructTempDesignMask();
        }
        this.BurningCounter = counter;
        this.BurningStrength = strength;
      }
      if(CombatHUDMiniMap.instance != null) { CombatHUDMiniMap.instance.AddBurning(this); }
    }
    public AudioSwitch_surface_type GetAudioSurfaceTypeEx() {
      TerrainMaskFlags terrainMaskFlags = MapMetaData.GetPriorityTerrainMaskFlags(this);
      bool flag1 = MoodController.HasInstance && MoodController.Instance.IsRaining();
      bool flag2 = this.mapEncounterLayerDataCell.GetTopmostBuilding() != null;
      DesignMaskDef designMask = this.CustomDesignMask;
      if (this.tempDesignMask != null) { designMask = this.tempDesignMask; };
      return !flag2 ? (terrainMaskFlags != TerrainMaskFlags.DestroyedBuilding ? (!flag1 ? designMask.audioSwitchSurfaceType : designMask.audioSwitchRainingSurfaceType) : AudioSwitch_surface_type.debris_glass) : AudioSwitch_surface_type.concrete;
    }
    public string GetVFXNameModifierEx() {
      if (this.tempDesignMask != null) { return this.tempDesignMask.vfxNameModifier; };
      return this.CustomDesignMask.vfxNameModifier;
    }
    public MapTerrainDataCellEx() {
      x = -1; y = -1; CustomDesignMask = null; hexCell = null;
      realTerrainHeight = float.NaN;
      waterLevelCached = false;
      //RealHeight = float.NaN;
      BurningCounter = 0;
      BurningStrength = 0;
      BurningWeapon = null;
      wasForest = false;
      wasCustom = false;
      wasRoad = false;
      CantHaveForest = false;
      trees = new List<CACDynamicTree>();
      tempDesignMask = null;
      tempDesignMaskCounters = new Dictionary<DesignMaskDef, int>();
    }
  }
  public class MapPoint {
    public int x;
    public int y;
    public static int offcet = 4096;
    public MapPoint(int X, int Y) {
      this.x = X;
      this.y = Y;
    }
    public override int GetHashCode() {
      return this.y * MapPoint.offcet + this.x;
    }
    public override bool Equals(object obj) {
      MapPoint mp = obj as MapPoint;
      if ((object)mp == null) { return false; };
      return (this.x == mp.x) && (this.y == mp.y);
    }
    public static void Swap(ref int a, ref int b) {
      a = a ^ b;
      b = a ^ b;
      a = a ^ b;
    }
    public static List<MapPoint> BresenhamLine(int x0, int y0, int x1, int y1) {
      List<MapPoint> result = new List<MapPoint>();
      var steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0); // Проверяем рост отрезка по оси икс и по оси игрек
                                                         // Отражаем линию по диагонали, если угол наклона слишком большой
      if (steep) {
        Swap(ref x0, ref y0); // Перетасовка координат вынесена в отдельную функцию для красоты
        Swap(ref x1, ref y1);
      }
      // Если линия растёт не слева направо, то меняем начало и конец отрезка местами
      if (x0 > x1) {
        Swap(ref x0, ref x1);
        Swap(ref y0, ref y1);
      }
      int dx = x1 - x0;
      int dy = Math.Abs(y1 - y0);
      int error = dx / 2; // Здесь используется оптимизация с умножением на dx, чтобы избавиться от лишних дробей
      int ystep = (y0 < y1) ? 1 : -1; // Выбираем направление роста координаты y
      int y = y0;
      for (int x = x0; x <= x1; x++) {
        result.Add(new MapPoint(steep ? y : x, steep ? x : y)); // Не забываем вернуть координаты на место
        error -= dy;
        if (error < 0) {
          y += ystep;
          error += dx;
        }
      }
      return result;
    }
    public static List<MapPoint> createHexagon(int x, int y, int r) {
      List<MapPoint> result = new List<MapPoint>();
      int dx = (int)((float)r / 2f);
      int dy = (int)Math.Round((float)r * 0.86025f);
      List<MapPoint> line = BresenhamLine(x + dx, y + dy, x + r, y);
      foreach (var point in line) {
        int tdx = point.x - x;
        int tdy = point.y - y;
        for (int tx = x - tdx; tx <= point.x; ++tx) {
          result.Add(new MapPoint(tx, point.y));
          result.Add(new MapPoint(tx, y - tdy));
        }
      }
      return result;
    }
    public static List<MapPoint> calcMapCircle(MapPoint C, int R) {
      HashSet<MapPoint> result = new HashSet<MapPoint>();
      int x = 0, y = R, gap = 0, delta = (2 - 2 * R);
      while (y >= 0) {
        for (int tx = C.x - x; tx <= C.x + x; ++tx) {
          MapPoint tmp = new MapPoint(tx, C.y + y);
          if (result.Contains(tmp) == false) { result.Add(tmp); };
          tmp = new MapPoint(tx, C.y - y);
          if (result.Contains(tmp) == false) { result.Add(tmp); };
        }
        gap = 2 * (delta + y) - 1;
        if (delta < 0 && gap <= 0) {
          x++;
          delta += 2 * x + 1;
          continue;
        }
        if (delta > 0 && gap > 0) {
          y--;
          delta -= 2 * y + 1;
          continue;
        }
        x++;
        delta += 2 * (x - y);
        y--;
      }
      return result.ToList<MapPoint>();
    }
  }
  public class VFXPoolGameObject {
    public string name;
    public GameObject obj;
    public VFXPoolGameObject(string n, GameObject o) { name = n; obj = o; }
  }
  /*public class ActorMineFieldVFX {
    public Vector3 lastVFXPos;
    public List<VFXPoolGameObject> fXPoolGameObjects;
    public ActorMineFieldVFX() { lastVFXPos = Vector3.zero; fXPoolGameObjects = new List<VFXPoolGameObject>(); }
  }*/
  public static partial class DynamicMapHelper {
    public static readonly string MINEFIELD_TRIGGER_PROBABILITY_STATISTIC_NAME = "CACMinefieldMult";
    public static Dictionary<string, DesignMaskDef> loadedMasksDef = new Dictionary<string, DesignMaskDef>();
    public static Dictionary<Vector3, MapTerrainHexCell> hexGrid = new Dictionary<Vector3, MapTerrainHexCell>();
    //public static MapTerrainHexCell[,] hexGrid = null;
    public static List<MapTerrainHexCell> burningHexes = new List<MapTerrainHexCell>();
    public static HashSet<MapTerrainHexCell> tempEffectHexes = new HashSet<MapTerrainHexCell>();
    public static HashSet<MapPoint> tempMaskCells = new HashSet<MapPoint>();
    public static MapMetaData mapMetaData = null;
    //public static Dictionary<ICombatant, ActorMineFieldVFX> lastMineFieldFXPlayedPosition = new Dictionary<ICombatant, ActorMineFieldVFX>();
    public static string CurrentBiome = "";
    public static float BiomeWeaponFireDuration() {
      float result = 1f;
      if (CustomAmmoCategories.Settings.WeaponBurningDurationBiomeMult.ContainsKey(DynamicMapHelper.CurrentBiome)) {
        Log.LogWrite(" biome mult:" + CustomAmmoCategories.Settings.WeaponBurningDurationBiomeMult[DynamicMapHelper.CurrentBiome] + "\n");
        result = CustomAmmoCategories.Settings.WeaponBurningDurationBiomeMult[DynamicMapHelper.CurrentBiome];
      }
      return result;
    }
    public static float BiomeWeaponFireStrength() {
      float result = 1f;
      if (CustomAmmoCategories.Settings.WeaponBurningStrengthBiomeMult.ContainsKey(DynamicMapHelper.CurrentBiome)) {
        Log.LogWrite(" biome mult:" + CustomAmmoCategories.Settings.WeaponBurningStrengthBiomeMult[DynamicMapHelper.CurrentBiome] + "\n");
        result = CustomAmmoCategories.Settings.WeaponBurningStrengthBiomeMult[DynamicMapHelper.CurrentBiome];
      }
      return result;
    }
    public static float BiomeLitFireChance() {
      float result = 1f;
      if (CustomAmmoCategories.Settings.LitFireChanceBiomeMult.ContainsKey(DynamicMapHelper.CurrentBiome)) {
        Log.LogWrite(" biome mult:" + CustomAmmoCategories.Settings.LitFireChanceBiomeMult[DynamicMapHelper.CurrentBiome] + "\n");
        result = CustomAmmoCategories.Settings.LitFireChanceBiomeMult[DynamicMapHelper.CurrentBiome];
      }
      return result;
    }
    public static int BurnForestDuration() {
      float result = CustomAmmoCategories.Settings.BurningForestTurns;
      Log.LogWrite("BurnForestDuration.Base:" + result + "\n");
      if (CustomAmmoCategories.Settings.ForestBurningDurationBiomeMult.ContainsKey(DynamicMapHelper.CurrentBiome)) {
        Log.LogWrite(" biome mult:" + CustomAmmoCategories.Settings.ForestBurningDurationBiomeMult[DynamicMapHelper.CurrentBiome] + "\n");
        result *= CustomAmmoCategories.Settings.ForestBurningDurationBiomeMult[DynamicMapHelper.CurrentBiome];
      }
      Log.LogWrite(" effective duration:" + Mathf.RoundToInt(result) + "\n");
      return Mathf.RoundToInt(result);
    }
    public static int BurnForestStrength() {
      float result = CustomAmmoCategories.Settings.BurningForestStrength;
      Log.LogWrite("BurnForestStrength.Base:" + result + "\n");
      if (CustomAmmoCategories.Settings.ForestBurningStrengthBiomeMult.ContainsKey(DynamicMapHelper.CurrentBiome)) {
        Log.LogWrite(" biome mult:" + CustomAmmoCategories.Settings.ForestBurningStrengthBiomeMult[DynamicMapHelper.CurrentBiome] + "\n");

        result *= CustomAmmoCategories.Settings.ForestBurningStrengthBiomeMult[DynamicMapHelper.CurrentBiome];
      }
      Log.LogWrite(" effective strength:" + Mathf.RoundToInt(result) + "\n");
      return Mathf.RoundToInt(result);
    }
    public static float FireExpandChance() {
      float result = CustomAmmoCategories.Settings.BurningForestBaseExpandChance;
      Log.LogWrite("FireExpandChance.Base:" + result + "\n");
      if (CustomAmmoCategories.Settings.LitFireChanceBiomeMult.ContainsKey(DynamicMapHelper.CurrentBiome)) {
        Log.LogWrite(" biome mult:" + CustomAmmoCategories.Settings.LitFireChanceBiomeMult[DynamicMapHelper.CurrentBiome] + "\n");
        result *= CustomAmmoCategories.Settings.LitFireChanceBiomeMult[DynamicMapHelper.CurrentBiome];
      }
      Log.LogWrite(" effective chance:" + result + "\n");
      return result;
    }
    public static void ClearTerrain() {
      CustomAmmoCategoriesLog.Log.M?.TWL(0,"ClearTerrain");
      DynamicMapHelper.burningHexes.Clear();
      DynamicMapHelper.tempEffectHexes.Clear();
      DynamicMapHelper.tempMaskCells.Clear();
      try {
        if (DynamicMapHelper.hexGrid != null) {
          CustomAmmoCategoriesLog.Log.LogWrite(" size:" + DynamicMapHelper.hexGrid.Count + "\n");
          {
            foreach(var vhcell in DynamicMapHelper.hexGrid) {
                try {
                MapTerrainHexCell hcell = vhcell.Value;//DynamicMapHelper.hexGrid[hx, hy];
                if (hcell == null) {
                  CustomAmmoCategoriesLog.Log.LogWrite("  hex cell is null\n");
                  continue;
                }
                hcell.clearTempVFXs();
                if (hcell.burnEffect == null) {
                  continue;
                }
                if (hcell.burnEffect.spawnedObject == null) {
                  continue;
                }
                hcell.burnEffect.CleanupSelf();
              } catch (Exception e) {
                CustomAmmoCategoriesLog.Log.LogWrite("  fail clean hex cell:" + e.ToString() + "\n");
              }
            }
          }
          CustomAmmoCategoriesLog.Log.LogWrite("  nulling hex matrix\n");
          DynamicMapHelper.hexGrid = null;
        }
      } catch (Exception e) {
        CustomAmmoCategoriesLog.Log.LogWrite("Fail to clean:" + e.ToString() + "\n");
      }
    }
    public static void FireTick() {
      CustomAmmoCategoriesLog.Log.LogWrite("FireTick\n");
      for (int index = 0; index < DynamicMapHelper.burningHexes.Count; ++index) {
        MapTerrainHexCell hex = burningHexes[index];
        hex.FireTick();
        hex.expandingThisTurn = true;
      }
      for (int index = 0; index < DynamicMapHelper.burningHexes.Count; ++index) {
        MapTerrainHexCell hex = burningHexes[index];
        if (hex.expandingThisTurn == false) { continue; };
        List<Vector3> hexesNerby = UnityGameInstance.BattleTechGame.Combat.HexGrid.GetGridPointsAroundPointWithinRadius(hex.center,1);
        foreach(Vector3 hexPosNerby in hexesNerby) {
          CustomAmmoCategoriesLog.Log.LogWrite(" expanding:" + hexPosNerby + "\n");
          if(DynamicMapHelper.hexGrid.TryGetValue(hexPosNerby,out MapTerrainHexCell hexNear) == false) {
            CustomAmmoCategoriesLog.Log.LogWrite("  can't find hex\n");
            continue;
          }
          CustomAmmoCategoriesLog.Log.LogWrite("  hex found\n");
          if (hexNear.TryExpand(hex.burningWeapon)) { DynamicMapHelper.burningHexes.Add(hexNear); }
        }
      }
      HashSet<MapTerrainHexCell> cleanTrees = new HashSet<MapTerrainHexCell>();
      for (int index = 0; index < DynamicMapHelper.burningHexes.Count;) {
        MapTerrainHexCell hex = burningHexes[index];
        if (hex.burnEffectCounter <= 0) { DynamicMapHelper.burningHexes.RemoveAt(index); cleanTrees.Add(hex); } else { ++index; }
      }
      HashSet<object> redrawTreeData = new HashSet<object>();
      foreach (MapTerrainHexCell hcell in cleanTrees) {
        hcell.deleteTrees(redrawTreeData);
      }
      CACDynamicTree.redrawTrees(redrawTreeData);
      DynamicTreesHelper.clearTrees();
    }
    public static void TempTick() {
      CustomAmmoCategoriesLog.Log.LogWrite("TempTick\n");
      HashSet<MapTerrainHexCell> markDel = new HashSet<MapTerrainHexCell>();
      foreach (var hc in DynamicMapHelper.tempEffectHexes) {
        //if ((hc.x < 0) || (hc.y < 0) || (hc.x >= DynamicMapHelper.hexGrid.GetLength(0)) || (hc.y >= DynamicMapHelper.hexGrid.GetLength(1))) { continue; }
        MapTerrainHexCell hcell = hc;//DynamicMapHelper.hexGrid[hc.x, hc.y];
        hcell.tempVFXTick();
        if (hcell.hasTempEffects() == false) { markDel.Add(hcell); };
      }
      foreach (var dp in markDel) {
        DynamicMapHelper.tempEffectHexes.Remove(dp);
      }
      HashSet<MapPoint> cellsDel = new HashSet<MapPoint>();
      if (DynamicMapHelper.mapMetaData == null) { return; };
      foreach (var hc in DynamicMapHelper.tempMaskCells) {
        if ((hc.x < 0) || (hc.y < 0) || (hc.x >= DynamicMapHelper.mapMetaData.mapTerrainDataCells.GetLength(0)) || (hc.y >= DynamicMapHelper.mapMetaData.mapTerrainDataCells.GetLength(1))) { continue; }
        MapTerrainDataCellEx cell = DynamicMapHelper.mapMetaData.mapTerrainDataCells[hc.x, hc.y] as MapTerrainDataCellEx;
        if (cell == null) { continue; }
        cell.tempMaskTick();
        if (cell.tempDesignMask == null) { cellsDel.Add(cell.mapPoint()); };
      }
      foreach (var dp in cellsDel) {
        DynamicMapHelper.tempMaskCells.Remove(dp);
      }
    }
    public static void initHexGrid(MapMetaData mapMetaData, HexGrid hexGrid) {
      DynamicMapHelper.mapMetaData = mapMetaData;
      DynamicMapHelper.CurrentBiome = "";
      try {
        DynamicMapHelper.CurrentBiome = Traverse.Create(mapMetaData).Field<string>("biomeDesignMaskName").Value;
      } catch (Exception) {
        DynamicMapHelper.CurrentBiome = "NotSet";
      }      
      bool noForest = Traverse.Create(mapMetaData).Field<string>("forestDesignMaskName").Value.Contains("Forest") == false;
      //CustomAmmoCategories.Settings.NoForestBiomes.Contains(DynamicMapHelper.CurrentBiome);
      Log.M.TWL(0,"Map biome:" + DynamicMapHelper.CurrentBiome + " noForest:" + noForest+" hex grid:"+(hexGrid == null?"null":"not null"));
      //Log.LogWrite(" stack:" + Environment.StackTrace + "\n");
      //HexGrid hexGrid = UnityGameInstance.BattleTechGame.Combat.HexGrid;
      DynamicMapHelper.hexGrid = new Dictionary<Vector3, MapTerrainHexCell>();
      for (int mx = 0; mx < mapMetaData.mapTerrainDataCells.GetLength(0); ++mx) {
        for (int my = 0; my < mapMetaData.mapTerrainDataCells.GetLength(1); ++my) {
          MapTerrainDataCellEx cell = mapMetaData.mapTerrainDataCells[mx, my] as MapTerrainDataCellEx;
          Vector3 hexPos = hexGrid.GetClosestPointOnGrid(cell.WorldPos());
          if(DynamicMapHelper.hexGrid.TryGetValue(hexPos,out MapTerrainHexCell hexCell) == false) {
            hexCell = new MapTerrainHexCell();
            hexCell.center = hexPos;
            hexCell.centerCell = mapMetaData.GetCellAt(hexCell.center) as MapTerrainDataCellEx;
            hexCell.Combat = UnityGameInstance.BattleTechGame.Combat;
            DynamicMapHelper.hexGrid.Add(hexPos, hexCell);
          }
          cell.hexCell = hexCell;
          hexCell.terrainCells.Add(cell);
          if (noForest) {
            cell.hexCell.isHasForest = false; cell.hexCell.wasHasForest = false; cell.CantHaveForest = true;
          } else {
            if (SplatMapInfo.IsForest(cell.terrainMask) == true) { cell.hexCell.isHasForest = true; cell.hexCell.wasHasForest = true; cell.CantHaveForest = false; };
          }
        }
      }
    }
    public static ObjectSpawnDataSelf SpawnFXObject(CombatGameState Combat, string prefabName, Vector3 pos, Vector3 scale) {
      ObjectSpawnDataSelf objectSpawnData = new ObjectSpawnDataSelf(prefabName, pos, Quaternion.identity, scale, true, false);
      try {
        objectSpawnData.SpawnSelf(Combat);
        //objectSpawnData.spawnedObject.transform.localScale += scale;
      } catch (Exception e) {
        CustomAmmoCategoriesLog.Log.LogWrite("Spawn exception:" + e.ToString() + "\n");
        CustomAmmoCategoriesLog.Log.LogWrite("investigating\n");
        VersionManifestEntry versionManifestEntry = Combat.DataManager.ResourceLocator.EntryByID(prefabName, BattleTechResourceType.Prefab, false);
        if (versionManifestEntry == null) {
          CustomAmmoCategoriesLog.Log.LogWrite("Can't load version manifest for '" + prefabName + "'\n");
          var entries = Combat.DataManager.ResourceLocator.AllEntries();
          foreach (var entry in entries) { // not necessary for ModTek v2 as it dumps the manifest under .modtek/Manifest.csv, kept here for ModTek v0.8 support
            CustomAmmoCategoriesLog.Log.LogWrite( $"{entry.Type} {entry.Id}:\n");
          }
        } else {
          CustomAmmoCategoriesLog.Log.LogWrite("versionManifestEntry.IsResourcesAsset:" + versionManifestEntry.IsResourcesAsset + "\n");
          PropertyInfo assetsManagerProp = typeof(DataManager).GetProperty("AssetBundleManager", BindingFlags.NonPublic | BindingFlags.Instance);
          if (assetsManagerProp != null) {
            MethodInfo methodInfo = assetsManagerProp.GetGetMethod(true);
            if (methodInfo == null) {
              CustomAmmoCategoriesLog.Log.LogWrite("can't get methodInfo\n");
            } else {
              AssetBundleManager manager = (AssetBundleManager)methodInfo.Invoke(Combat.DataManager, new object[0] { });
              if (manager != null) {
                CustomAmmoCategoriesLog.Log.LogWrite("manager tryied to load " + prefabName + "," + versionManifestEntry.AssetBundleName + "\n");
                System.Collections.IDictionary loadedBundles = (System.Collections.IDictionary)typeof(AssetBundleManager).GetField("loadedBundles", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(manager);
                if (loadedBundles != null) {
                  CustomAmmoCategoriesLog.Log.LogWrite("loadedBundles:" + loadedBundles.GetType().ToString() + ":" + loadedBundles.Count + "\n");
                  System.Collections.IEnumerator en = loadedBundles.Keys.GetEnumerator();
                  en.Reset();
                  do {
                    CustomAmmoCategoriesLog.Log.LogWrite(" " + en.Current + "\n");
                  } while (en.MoveNext());
                }
              } else {
                CustomAmmoCategoriesLog.Log.LogWrite("can't get manager\n");
              }
            }
          } else {
            CustomAmmoCategoriesLog.Log.LogWrite("can't get property\n");
          }

          //.GetGetMethod();
        }
      }
      //}
      return objectSpawnData;
    }
    public static void applyImpactBurn(Weapon weapon, Vector3 pos) {
      if (weapon.parent.isSpawnProtected() && CustomAmmoCategories.Settings.SpawnProtectionAffectsBurningTerrain) { return; }
      CustomAmmoCategoriesLog.Log.LogWrite("Applying burn effect:" + weapon.defId + " " + pos + "\n");
      MapTerrainDataCellEx cell = weapon.parent.Combat.MapMetaData.GetCellAt(pos) as MapTerrainDataCellEx;
      if (cell == null) {
        CustomAmmoCategoriesLog.Log.LogWrite(" cell is not extended\n");
        return;
      }
      CustomAmmoCategoriesLog.Log.LogWrite(" impact at " + pos + "\n");
      if (weapon.FireTerrainCellRadius() == 0) {
        cell.hexCell.TryBurnCellAsync(weapon);
        //if (cell.hexCell.TryBurnCell(weapon)) { DynamicMapHelper.burningHexes.Add(cell.hexCell); };
      } else {
        List<MapTerrainHexCell> affectedHexCells = MapTerrainHexCell.listHexCellsByCellRadius(cell, weapon.FireTerrainCellRadius());
        foreach (MapTerrainHexCell hexCell in affectedHexCells) {
          hexCell.TryBurnCellAsync(weapon);
          //if (hexCell.TryBurnCell(weapon)) { DynamicMapHelper.burningHexes.Add(hexCell); };
        }
      }
    }
    public static void applyImpactTempMask(Weapon weapon, Vector3 pos) {
      if (weapon.parent.isSpawnProtected() && CustomAmmoCategories.Settings.SpawnProtectionAffectsDesignMasks) { return; }
      CustomAmmoCategoriesLog.Log.LogWrite("Applying long effect:" + weapon.defId + " " + pos + "\n");
      MapTerrainDataCellEx cell = weapon.parent.Combat.MapMetaData.GetCellAt(pos) as MapTerrainDataCellEx;
      if (cell == null) {
        CustomAmmoCategoriesLog.Log.LogWrite(" cell is not extended\n");
        return;
      }
      CustomAmmoCategoriesLog.Log.LogWrite(" impact at " + pos + "\n");
      int turns = 0;
      string vfx = string.Empty;
      Vector3 scale;
      int radius = 0;
      DesignMaskDef mask = weapon.tempDesignMask(out turns, out vfx, out scale, out radius);
      if (radius == 0) {
        cell.hexCell.addTempTerrainVFX(weapon.parent.Combat, vfx, turns, scale);
        cell.hexCell.addDesignMaskAsync(mask, turns);

      } else {
        List<MapTerrainHexCell> affectedHexCells = MapTerrainHexCell.listHexCellsByCellRadius(cell, radius);
        foreach (MapTerrainHexCell hexCell in affectedHexCells) {
          hexCell.addTempTerrainVFX(weapon.parent.Combat, vfx, turns, scale);
          hexCell.addDesignMaskAsync(mask, turns);
        }
      }
    }
    public static void applyCleanMinefield(Weapon weapon, Vector3 pos) {
      CustomAmmoCategoriesLog.Log.LogWrite("Applying minefield clear:" + weapon.defId + " " + pos + "\n");
      MapTerrainDataCellEx cell = weapon.parent.Combat.MapMetaData.GetCellAt(pos) as MapTerrainDataCellEx;
      if (cell == null) {
        CustomAmmoCategoriesLog.Log.LogWrite(" cell is not extended\n");
        return;
      }
      if (weapon.ClearMineFieldRadius() == 0) { return; };
      if (weapon.ClearMineFieldRadius() == 1) {
        //Log.LogWrite(" affected cell " + cell.hexCell.x + "," + cell.hexCell.y + "\n");
        Log.LogWrite(" affected cell " + cell.hexCell.center + "\n");
        cell.hexCell.clearMineField(1, 0f);
          //MineField.Clear();
      } else {
        List<MapTerrainHexCell> affectedHexCells = MapTerrainHexCell.listHexCellsByCellRadius(cell, weapon.ClearMineFieldRadius());
        foreach (MapTerrainHexCell hexCell in affectedHexCells) {
          //Log.LogWrite(" affected cell " + hexCell.x + "," + hexCell.y + "\n");
          Log.LogWrite(" affected cell " + hexCell.center + "\n");
          hexCell.clearMineField(1, 0f);
          //hexCell.MineField.Clear();
        }
      }
    }
    public static void applyMineField(Weapon weapon, Vector3 pos) {
      if (weapon.parent.isSpawnProtected() && CustomAmmoCategories.Settings.SpawnProtectionAffectsMinelayers) {
        Log.M.WL(0, "Applying minefield:" + weapon.defId + " " + pos + " neares hex: " + weapon.parent.Combat.HexGrid.GetClosestPointOnGrid(pos)+" but attacker is spawn protected");
        return;
      }
      CustomAmmoCategoriesLog.Log.M.WL(0,"Applying minefield:" + weapon.defId + " " + pos + " neares hex: "+weapon.parent.Combat.HexGrid.GetClosestPointOnGrid(pos));
      MapTerrainDataCellEx cell = weapon.parent.Combat.MapMetaData.GetCellAt(pos) as MapTerrainDataCellEx;
      if (cell == null) {
        CustomAmmoCategoriesLog.Log.LogWrite(" cell is not extended\n");
        return;
      }
      CustomAmmoCategoriesLog.Log.LogWrite(" impact at " + pos + "\n");
      if (weapon.InstallMineField() == false) { return; }
      MineFieldDef mfd = weapon.MineFieldDef();
      if (mfd.InstallCellRange == 0) {
        //Log.LogWrite(" affected cell " + cell.hexCell.x + "," + cell.hexCell.y + ":" + mfd.Count + "\n");
        Log.LogWrite(" affected cell " + cell.hexCell.center +":" + mfd.Count + "\n");
        cell.hexCell.addMineField(mfd, weapon.parent, weapon);
      } else {
        List<MapTerrainHexCell> affectedHexCells = MapTerrainHexCell.listHexCellsByCellRadius(cell, mfd.InstallCellRange);
        foreach (MapTerrainHexCell hexCell in affectedHexCells) {
          //Log.LogWrite(" affected cell " + hexCell.x + "," + hexCell.y + ":" + mfd.Count + "\n");
          Log.LogWrite(" affected cell " + hexCell.center + ":" + mfd.Count + "\n");
          hexCell.addMineField(mfd, weapon.parent, weapon);
        }
      }
    }
    public static void TrackLoadedMaskDef(string key, DesignMaskDef mask) {
      CustomAmmoCategoriesLog.Log.LogWrite("Dynamic design mask loaded:" + key + " = " + mask.Description.Name + "\n");
      if (DynamicMapHelper.loadedMasksDef.ContainsKey(key)) {
        DynamicMapHelper.loadedMasksDef[key] = mask;
      } else {
        DynamicMapHelper.loadedMasksDef.Add(key, mask);
      }
    }
    public static DataManager DataManager = null;
    public static void VFXDepsLoaded(string key, GameObject vfx) {
    }
    public static void LoadDesignMasks(DataManager dataManager) {
      DynamicMapHelper.DataManager = dataManager;
      LoadRequest loadRequest = dataManager.CreateLoadRequest((Action<LoadRequest>)null, false);
      foreach (string key in CustomAmmoCategories.Settings.DynamicDesignMasksDefs) {
        loadRequest.AddLoadRequest<DesignMaskDef>(BattleTechResourceType.DesignMaskDef, key, new Action<string, DesignMaskDef>(DynamicMapHelper.TrackLoadedMaskDef), false);
      }
      loadRequest.ProcessRequests(10U);
    }
    public static List<MapPoint> getVisitedPoints(CombatGameState combat, List<WayPoint> waypoints) {
      HashSet<MapPoint> result = new HashSet<MapPoint>();
      if (waypoints == null || waypoints.Count == 0)
        return result.ToList<MapPoint>();
      int length1 = combat.MapMetaData.mapTerrainDataCells.GetLength(0);
      int length2 = combat.MapMetaData.mapTerrainDataCells.GetLength(1);
      for (int index1 = waypoints.Count - 1; index1 > 0; --index1) {
        List<Point> points = BresenhamLineUtil.BresenhamLine(new Point(combat.MapMetaData.GetXIndex(waypoints[index1].Position.x), combat.MapMetaData.GetZIndex(waypoints[index1].Position.z)), new Point(combat.MapMetaData.GetXIndex(waypoints[index1 - 1].Position.x), combat.MapMetaData.GetZIndex(waypoints[index1 - 1].Position.z)));
        for (int index2 = 0; index2 < points.Count; ++index2) {
          if (points[index2].Z >= 0 && points[index2].Z < length1 && (points[index2].X >= 0 && points[index2].X < length2)) {
            MapTerrainDataCellEx cell = combat.MapMetaData.mapTerrainDataCells[points[index2].Z, points[index2].X] as MapTerrainDataCellEx;
            if (cell != null) {
              MapPoint mapPoint = new MapPoint(cell.x, cell.y);
              if (result.Contains(mapPoint) == false) { result.Add(mapPoint); };
            }
          }
        }
      }
      return result.ToList<MapPoint>();
    }
    public static HashSet<MapTerrainDataCellEx> getVisitedCells(CombatGameState combat, List<WayPoint> waypoints) {
      HashSet<MapTerrainDataCellEx> result = new HashSet<MapTerrainDataCellEx>();
      if (waypoints == null || waypoints.Count == 0) { return result; };
      int length1 = combat.MapMetaData.mapTerrainDataCells.GetLength(0);
      int length2 = combat.MapMetaData.mapTerrainDataCells.GetLength(1);
      for (int index1 = waypoints.Count - 1; index1 > 0; --index1) {
        List<Point> points = BresenhamLineUtil.BresenhamLine(new Point(combat.MapMetaData.GetXIndex(waypoints[index1].Position.x), combat.MapMetaData.GetZIndex(waypoints[index1].Position.z)), new Point(combat.MapMetaData.GetXIndex(waypoints[index1 - 1].Position.x), combat.MapMetaData.GetZIndex(waypoints[index1 - 1].Position.z)));
        for (int index2 = 0; index2 < points.Count; ++index2) {
          if (points[index2].Z >= 0 && points[index2].Z < length1 && (points[index2].X >= 0 && points[index2].X < length2)) {
            MapTerrainDataCellEx cell = combat.MapMetaData.mapTerrainDataCells[points[index2].Z, points[index2].X] as MapTerrainDataCellEx;
            if (cell != null) {
              if (result.Contains(cell) == false) { result.Add(cell); };
            }
          }
        }
      }
      return result;
    }
    public static List<MapTerrainCellWaypoint> getVisitedWaypoints(CombatGameState combat, List<WayPoint> waypoints) {
      HashSet<MapTerrainDataCellEx> tst = new HashSet<MapTerrainDataCellEx>();
      List<MapTerrainCellWaypoint> result = new List<MapTerrainCellWaypoint>();
      if (waypoints == null || waypoints.Count == 0) { return result; };
      int length1 = combat.MapMetaData.mapTerrainDataCells.GetLength(0);
      int length2 = combat.MapMetaData.mapTerrainDataCells.GetLength(1);
      for (int index1 = waypoints.Count - 1; index1 > 0; --index1) {
        List<Point> points = BresenhamLineUtil.BresenhamLine(new Point(combat.MapMetaData.GetXIndex(waypoints[index1].Position.x), combat.MapMetaData.GetZIndex(waypoints[index1].Position.z)), new Point(combat.MapMetaData.GetXIndex(waypoints[index1 - 1].Position.x), combat.MapMetaData.GetZIndex(waypoints[index1 - 1].Position.z)));
        for (int index2 = 0; index2 < points.Count; ++index2) {
          if (points[index2].Z >= 0 && points[index2].Z < length1 && (points[index2].X >= 0 && points[index2].X < length2)) {
            MapTerrainDataCellEx cell = combat.MapMetaData.mapTerrainDataCells[points[index2].Z, points[index2].X] as MapTerrainDataCellEx;
            if (cell != null) {
              if (tst.Contains(cell) == false) {
                MapTerrainCellWaypoint waypoint = new MapTerrainCellWaypoint(cell, waypoints[index1 - 1]);
                tst.Add(cell);
                result.Add(waypoint);
              };
            }
          }
        }
      }
      return result;
    }
    public static HashSet<MapTerrainHexCell> getVisitedHexes(CombatGameState combat, List<WayPoint> waypoints) {
      HashSet<MapTerrainHexCell> result = new HashSet<MapTerrainHexCell>();
      if (waypoints == null || waypoints.Count == 0) { return result; };
      int length1 = combat.MapMetaData.mapTerrainDataCells.GetLength(0);
      int length2 = combat.MapMetaData.mapTerrainDataCells.GetLength(1);
      for (int index1 = waypoints.Count - 1; index1 > 0; --index1) {
        List<Point> points = BresenhamLineUtil.BresenhamLine(new Point(combat.MapMetaData.GetXIndex(waypoints[index1].Position.x), combat.MapMetaData.GetZIndex(waypoints[index1].Position.z)), new Point(combat.MapMetaData.GetXIndex(waypoints[index1 - 1].Position.x), combat.MapMetaData.GetZIndex(waypoints[index1 - 1].Position.z)));
        for (int index2 = 0; index2 < points.Count; ++index2) {
          if (points[index2].Z >= 0 && points[index2].Z < length1 && (points[index2].X >= 0 && points[index2].X < length2)) {
            MapTerrainDataCellEx cell = combat.MapMetaData.mapTerrainDataCells[points[index2].Z, points[index2].X] as MapTerrainDataCellEx;
            if (cell != null) {
              if (result.Contains(cell.hexCell) == false) { result.Add(cell.hexCell); };
            }
          }
        }
      }
      return result;
    }
  }
  public class movingDamage {
    public float mineFieldDamage;
    public int mineFieldHeat;
    public float mineFieldInstability;
    public int burnHeat;
    public Weapon weapon;
    public movingDamage() {
      mineFieldDamage = 0f;
      mineFieldHeat = 0;
      mineFieldInstability = 0f;
      burnHeat = 0;
      weapon = null;
    }
  }
  public static class MineFieldHelper {
    public static Dictionary<string, movingDamage> registredMovingDamage = new Dictionary<string, movingDamage>();
    public static bool hasRegistredMovingDamage(this AbstractActor actor) {
      Mech mech = actor as Mech;
      Vehicle vehicle = actor as Vehicle;
      if ((mech == null) && (vehicle == null)) { return false; };
      if (MineFieldHelper.registredMovingDamage.ContainsKey(actor.GUID) == false) { return false; };
      movingDamage mDmg = MineFieldHelper.registredMovingDamage[actor.GUID];
      if (mech != null) { return mDmg.mineFieldDamage > CustomAmmoCategories.Epsilon; };
      if (vehicle != null) { return (mDmg.mineFieldDamage > CustomAmmoCategories.Epsilon) || (mDmg.mineFieldHeat > 0) || (mDmg.burnHeat > 0); };
      return false;
    }
    public static void registerMovingDamageFromPath(AbstractActor __instance, List<WayPoint> waypoints) {
      Log.LogWrite("registerMovingDamageFromPath to " + __instance.DisplayName + ":" + __instance.GUID + "\n");
    }
    public static void registerJumpingDamageFrom(Mech __instance, Vector3 finalPosition) {
    }
  }
}

namespace CustomAmmoCategoriesPatches {
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("OnActivationEnd")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string), typeof(int) })]
  public static class AbstractActor_OnActivationEndFire {
    private static bool Prefix(AbstractActor __instance, string sourceID, int stackItemID) {
      MapTerrainDataCellEx cell = __instance.Combat.MapMetaData.GetCellAt(__instance.CurrentPosition) as MapTerrainDataCellEx;
      if (cell == null) { return true; };
      Mech mech = __instance as Mech;
      Vehicle vehicle = __instance as Vehicle;
      if (cell.BurningStrength > 0) {
        if (__instance.HasMovedThisRound == false) {
          AbstractActor actor = cell.BurningWeapon.parent;
          CustomAmmoCategoriesLog.Log.LogWrite(" heat from standing in fire:" + cell.BurningStrength + "\n");
          if (mech != null) {
            __instance.AddExternalHeat("BurningCell", cell.BurningStrength);
            __instance.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(__instance.GUID, __instance.GUID, "+ " + cell.BurningStrength + " __/CAC.HEATFROMSTANDINGINFIRE/__", FloatieMessage.MessageNature.Debuff));
            __instance.CheckForInstability();
            __instance.HandleKnockdown(-1, actor.GUID, Vector2.one, (SequenceFinished)null);
            __instance.HandleDeath(actor.GUID);
          } else
          if (vehicle != null) {
            float damage = (float)cell.BurningStrength;
            Weapon weapon = cell.BurningWeapon;
            var fakeHit = new WeaponHitInfo(-1, -1, -1, -1, actor.GUID, __instance.GUID, -1, null, null, null, null, null, null, new AttackImpactQuality[1] { AttackImpactQuality.Solid }, new AttackDirection[1] { AttackDirection.FromArtillery }, null, null, null);
            __instance.TakeWeaponDamage(fakeHit, (int)VehicleChassisLocations.Front, weapon, damage / 4f, 0f, 0, DamageType.Combat);
            __instance.TakeWeaponDamage(fakeHit, (int)VehicleChassisLocations.Rear, weapon, damage / 4f, 0f, 0, DamageType.Combat);
            __instance.TakeWeaponDamage(fakeHit, (int)VehicleChassisLocations.Right, weapon, damage / 4f, 0f, 0, DamageType.Combat);
            __instance.TakeWeaponDamage(fakeHit, (int)VehicleChassisLocations.Left, weapon, damage / 4f, 0f, 0, DamageType.Combat);
            __instance.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(__instance.GUID, __instance.GUID, "__/CAC.DAMAGEFROMSTANDINGINFIRE/__", FloatieMessage.MessageNature.CriticalHit));
            __instance.HandleDeath(actor.GUID);
          }
        }
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(MapMetaData))]
  [HarmonyPatch("GetPriorityDesignMask")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MapTerrainDataCell) })]
  public static class MapMetaData_GetPriorityDesignMask {
    private static void Postfix(MapMetaData __instance, MapTerrainDataCell cell, ref DesignMaskDef __result) {
      try {
        MapTerrainDataCellEx excell = cell as MapTerrainDataCellEx;
        if(excell != null)
        if (Thread.CurrentThread.isFlagSet(ActorMovementSequence_UpdateSticky.HIDE_DESIGN_MASK_FLAG)) {
          //Log.M?.TWL(0, "MapMetaData.GetPriorityDesignMask x:"+ excell?.x+" y:"+excell.y+" mask skipped");
          __result = null;
          return;
        }
        if (excell != null) {
          if (excell.tempDesignMask != null) {
            __result = excell.tempDesignMask;
            return;
          }
          if (excell.CustomDesignMask == null) {
            return;
          };
          TerrainMaskFlags terrainMaskFlags = MapMetaData.GetPriorityTerrainMaskFlags(cell);
          if (SplatMapInfo.IsCustom(terrainMaskFlags) == false) {
            return;
          }
          __result = excell.CustomDesignMask;
        }
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(MapTerrainDataCell))]
  [HarmonyPatch("GetAudioSurfaceType")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MapTerrainDataCell_GetAudioSurfaceType {
    private static void Postfix(MapTerrainDataCell __instance, ref AudioSwitch_surface_type __result) {
      try {
        MapTerrainDataCellEx excell = __instance as MapTerrainDataCellEx;
        if (excell != null) {
          if (excell.tempDesignMask != null) {
            __result = excell.GetAudioSurfaceTypeEx();
            return;
          }
          if (excell.CustomDesignMask == null) { return; };
          TerrainMaskFlags terrainMaskFlags = MapMetaData.GetPriorityTerrainMaskFlags(__instance);
          if (SplatMapInfo.IsCustom(terrainMaskFlags) == false) {
            return;
          }
          __result = excell.GetAudioSurfaceTypeEx();
        }
      }catch(Exception e) {
        Log.M?.TWL(0,e.ToString(),true);
      }
    }
  }
  [HarmonyPatch(typeof(MapTerrainDataCell))]
  [HarmonyPatch("GetVFXNameModifier")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MapTerrainDataCell_GetVFXNameModifier {
    static void Postfix(MapTerrainDataCell __instance, ref string __result) {
      MapTerrainDataCellEx excell = __instance as MapTerrainDataCellEx;
      if (excell != null) {
        if (excell.tempDesignMask != null) {
          __result = excell.GetVFXNameModifierEx();
          return;
        }
        if (excell.CustomDesignMask == null) { return; };
        TerrainMaskFlags terrainMaskFlags = MapMetaData.GetPriorityTerrainMaskFlags(__instance);
        if (SplatMapInfo.IsCustom(terrainMaskFlags) == false) {
          return;
        }
        __result = excell.GetVFXNameModifierEx();
      }
    }
  }
  [HarmonyPatch(typeof(MapMetaData))]
  [HarmonyPatch("Load")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(SerializationStream), typeof(DataManager) })]
  public static class MapMetaData_LoadData {
    static void Postfix(MapMetaData __instance, SerializationStream stream, DataManager dataManager) {
      DynamicMapHelper.LoadDesignMasks(dataManager);
    }
  }
  //[HarmonyPatch(typeof(HexGrid))]
  //[HarmonyPatch(MethodType.Constructor)]
  //[HarmonyPatch(new Type[] { typeof(CombatGameState) })]
  //public static class HexGrid_Constructor {
  //  public static void Postfix(HexGrid __instance, CombatGameState combat) {
  //    Log.M.TWL(0, "HexGrid.Constructor combat:"+(combat == null?"null":"not null"));
  //    try {
  //      if (combat != null) {
  //        DynamicMapHelper.initHexGrid(combat.MapMetaData, __instance);
  //      }
  //    } catch(Exception e) {
  //      Log.M?.TWL(0,e.ToString(),true);
  //    }
  //  }
  //}
  [HarmonyPatch(typeof(MapMetaData))]
  [HarmonyPatch("Load")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(SerializationStream) })]
  public static class MapMetaData_Load {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      List<ConstructorInfo> baseConstructors = AccessTools.GetDeclaredConstructors(typeof(MapTerrainDataCell));
      List<ConstructorInfo> replaceConstructors = AccessTools.GetDeclaredConstructors(typeof(MapTerrainDataCellEx));
      ConstructorInfo replacConstructor = null;
      Log.M.TWL(0, "MapMetaData.Load.Transpiler");
      Log.M.WL(1, "MapTerrainDataCell constructors:"+ baseConstructors.Count);
      foreach(ConstructorInfo info in baseConstructors) {
        Log.M.WL(2,"parameters:"+info.GetParameters().Length);
      }
      Log.M.WL(1, "MapTerrainDataCellEx constructors:" + replaceConstructors.Count);
      foreach (ConstructorInfo info in replaceConstructors) {
        Log.M.WL(2, "parameters:" + info.GetParameters().Length);
        Log.M.WL(2, "IsPublic:" + info.IsPublic);
        Log.M.WL(2, "Name:" + info.Name);
        if (info.IsPublic) { replacConstructor = info; }
      }
      var targetConstructor = AccessTools.Constructor(typeof(MapTerrainDataCell));
      //replacConstructor = AccessTools.Constructor(typeof(MapTerrainDataCellEx));
      return Transpilers.MethodReplacer(instructions, targetConstructor, replacConstructor);
    }
    static void Postfix(MapMetaData __instance, SerializationStream stream) {
      int xmax = __instance.mapTerrainDataCells.GetLength(0);
      int ymax = __instance.mapTerrainDataCells.GetLength(1);
      CustomAmmoCategoriesLog.Log.LogWrite("MapMetaData.Load " + xmax + " X " + ymax + " \n");
      Log.M?.WL(0,Environment.StackTrace);
      for (int x = 0; x < xmax; ++x) {
        for (int y = 0; y < ymax; ++y) {
          MapTerrainDataCellEx ecell = __instance.mapTerrainDataCells[x, y] as MapTerrainDataCellEx;
          if (ecell != null) {
            ecell.x = x;
            ecell.y = y;
            ecell.realTerrainHeight = ecell.terrainHeight;
            ecell.pendingMasks = ecell.terrainMask;
          }
        }
      }
      CACMain.Core.Call_MapMetadata_Load_Postfixes(__instance);
      if (Terrain.activeTerrain == null) {
        CustomAmmoCategoriesLog.Log.LogWrite(" active terrain is null \n");
      } else {
        DynamicMapHelper.initHexGrid(__instance, UnityGameInstance.BattleTechGame.Combat.HexGrid);
      }
    }
  }
  [HarmonyPatch(typeof(TurnDirector))]
  [HarmonyPatch("EndCurrentRound")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class TurnDirector_EndCurrentRound {
    public static void Postfix(TurnDirector __instance) {
      try {
        DynamicMapHelper.FireTick();
        DynamicMapHelper.TempTick();
        BlockWeaponsHelpers.EjectAIBlocking(__instance.Combat);
      } catch(Exception e) {
        Log.M?.TWL(0,e.ToString(),true);
      }
    }
  }
  [HarmonyPatch(typeof(TreeContainer))]
  [HarmonyPatch("GatherTrees")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class TreeContainer_GatherTrees {
    public static void Postfix(TreeContainer __instance) {
      CustomAmmoCategoriesLog.Log.LogWrite("TreeContainer_GatherTrees.Postfix\n");
    }
  }
  [HarmonyPatch(typeof(BTCustomRenderer))]
  [HarmonyPatch("DrawDecals")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Camera) })]
  public static class BTCustomRenderer_DrawDecals {
    public static Material ScorchMaterial = null;
    public static Material BloodMaterial = null;
    public static FieldInfo deferredDecalsBufferField = null;
    public static FieldInfo skipDecalsField = null;
    public static FieldInfo effectsQualityField = null;
    //public static MethodInfo UseCameraMethod = null;
    public delegate object d_UseCameraMethod(BTCustomRenderer renderer, Camera camera);
    public static d_UseCameraMethod i_UseCameraMethod = null;
    public static object UseCameraMethod(this BTCustomRenderer renderer,Camera camera) { return i_UseCameraMethod(renderer,camera); }
    public static readonly int maxArraySize = 1000;
    public static List<List<Matrix4x4>> Scorches = new List<List<Matrix4x4>>();
    public static List<List<Matrix4x4>> Bloods = new List<List<Matrix4x4>>();
    public static void Clear() {
      BTCustomRenderer_DrawDecals.Scorches.Clear();
      BTCustomRenderer_DrawDecals.Bloods.Clear();
    }
    public static void AddScorch(Vector3 position, Vector3 forward, Vector3 scale) {
      if (CustomAmmoCategories.Settings.DontShowScorchTerrain == true) { return; }
      if (BTCustomRenderer_DrawDecals.Scorches.Count == 0) {
        BTCustomRenderer_DrawDecals.Scorches.Add(new List<Matrix4x4>());
      } else
      if (BTCustomRenderer_DrawDecals.Scorches[BTCustomRenderer_DrawDecals.Scorches.Count - 1].Count > BTCustomRenderer_DrawDecals.maxArraySize) {
        BTCustomRenderer_DrawDecals.Scorches.Add(new List<Matrix4x4>());
      }
      Quaternion rotation = forward.sqrMagnitude > CustomAmmoCategories.Epsilon?Quaternion.LookRotation(forward):Quaternion.identity;
      rotation = Quaternion.Euler(0.0f, rotation.eulerAngles.y, 0.0f);
      Matrix4x4 trs = Matrix4x4.TRS(position, rotation, scale);
      BTCustomRenderer_DrawDecals.Scorches[BTCustomRenderer_DrawDecals.Scorches.Count - 1].Add(trs);
    }
    public static void AddBlood(Vector3 position, Vector3 forward, Vector3 scale) {
      if (CustomAmmoCategories.Settings.DontShowScorchTerrain == true) { return; }
      if (BTCustomRenderer_DrawDecals.Bloods.Count == 0) {
        BTCustomRenderer_DrawDecals.Bloods.Add(new List<Matrix4x4>());
      } else
      if (BTCustomRenderer_DrawDecals.Bloods[BTCustomRenderer_DrawDecals.Bloods.Count - 1].Count > BTCustomRenderer_DrawDecals.maxArraySize) {
        BTCustomRenderer_DrawDecals.Bloods.Add(new List<Matrix4x4>());
      }
      Quaternion rotation = forward.sqrMagnitude > CustomAmmoCategories.Epsilon ? Quaternion.LookRotation(forward) : Quaternion.identity;
      rotation = Quaternion.Euler(0.0f, rotation.eulerAngles.y, 0.0f);
      Matrix4x4 trs = Matrix4x4.TRS(position, rotation, scale);
      BTCustomRenderer_DrawDecals.Bloods[BTCustomRenderer_DrawDecals.Bloods.Count - 1].Add(trs);
    }
    public static bool Prepare() {
      CustomAmmoCategoriesLog.Log.LogWrite("BTCustomRenderer_DrawDecals prepare\n"); ;
      BTCustomRenderer_DrawDecals.ScorchMaterial = Resources.Load<Material>("Decals/ScorchMaterial");
      if (BTCustomRenderer_DrawDecals.ScorchMaterial == null) {
        CustomAmmoCategoriesLog.Log.LogWrite("Fail to load scorch material\n");
        return false;
      }
      CustomAmmoCategoriesLog.Log.LogWrite("Scorch material success loaded\n"); ;
      BTCustomRenderer_DrawDecals.ScorchMaterial = UnityEngine.Object.Instantiate(FootstepManager.Instance.scorchMaterial);
      if (BTCustomRenderer_DrawDecals.ScorchMaterial == null) {
        CustomAmmoCategoriesLog.Log.LogWrite("Fail to copy scorch material\n");
        return false;
      }
      CustomAmmoCategoriesLog.Log.LogWrite("Scorch material success copied\n"); ;
      BTCustomRenderer_DrawDecals.ScorchMaterial.DisableKeyword("_ALPHABLEND_ON");
      CustomAmmoCategoriesLog.Log.LogWrite("Alphablend disabled.\n"); ;
      Texture2D terrainTexture = CACMain.Core.findTexture(CustomAmmoCategories.Settings.BurnedTrees.DecalTexture);
      CustomAmmoCategoriesLog.Log.LogWrite("Testing texture\n"); ;
      if (terrainTexture == null) {
        CustomAmmoCategoriesLog.Log.LogWrite("Fail to load texture\n");
        return false;
      }
      CustomAmmoCategoriesLog.Log.LogWrite("Success loaded texture\n"); ;
      BTCustomRenderer_DrawDecals.ScorchMaterial.SetFloat("_AffectTree", 0f);
      BTCustomRenderer_DrawDecals.ScorchMaterial.SetTexture("_MainTex", terrainTexture);
      BTCustomRenderer_DrawDecals.ScorchMaterial.enableInstancing = true;



      //BTCustomRenderer_DrawDecals.UseCameraMethod = typeof(BTCustomRenderer).GetMethod("UseCamera", BindingFlags.Instance | BindingFlags.NonPublic);
      //if (BTCustomRenderer_DrawDecals.UseCameraMethod == null) {
        //CustomAmmoCategoriesLog.Log.LogWrite("Fail to get UseCamera method\n"); ;
        //return false;
      //}


      {
        Type BTCustomRenderer_CustomCommandBuffers = typeof(BTCustomRenderer).GetNestedType("CustomCommandBuffers", BindingFlags.NonPublic);
        MethodInfo UseCameraMethod = typeof(BTCustomRenderer).GetMethod("UseCamera", BindingFlags.Instance | BindingFlags.NonPublic);
        var dm = new DynamicMethod("CACUseCameraMethod", BTCustomRenderer_CustomCommandBuffers, new Type[] {typeof(BTCustomRenderer), typeof(Camera) }, typeof(BTCustomRenderer));
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Call, UseCameraMethod);
        gen.Emit(OpCodes.Ret);
        i_UseCameraMethod = (d_UseCameraMethod)dm.CreateDelegate(typeof(d_UseCameraMethod));
      }

      BTCustomRenderer_DrawDecals.BloodMaterial = UnityEngine.Object.Instantiate(FootstepManager.Instance.scorchMaterial);
      if (BTCustomRenderer_DrawDecals.BloodMaterial == null) {
        CustomAmmoCategoriesLog.Log.LogWrite("Fail to copy blood material\n");
        return false;
      }
      CustomAmmoCategoriesLog.Log.LogWrite("Blood material success copied\n"); ;
      BTCustomRenderer_DrawDecals.BloodMaterial.DisableKeyword("_ALPHABLEND_ON");
      CustomAmmoCategoriesLog.Log.LogWrite("Alphablend disabled.\n"); ;
      Texture2D bloodTexture = CACMain.Core.findTexture(CustomAmmoCategories.Settings.bloodSettings.DecalTexture);
      CustomAmmoCategoriesLog.Log.LogWrite("Testing texture. " + CustomAmmoCategories.Settings.bloodSettings.DecalTexture + "\n"); ;
      if (bloodTexture == null) {
        CustomAmmoCategoriesLog.Log.LogWrite("Fail to load texture\n");
        return false;
      }
      CustomAmmoCategoriesLog.Log.LogWrite("Success loaded texture\n"); ;
      BTCustomRenderer_DrawDecals.BloodMaterial.SetFloat("_AffectTree", 0f);
      BTCustomRenderer_DrawDecals.BloodMaterial.SetTexture("_MainTex", bloodTexture);
      //BTCustomRenderer_DrawDecals.BloodMaterial.color = Color.red;
      BTCustomRenderer_DrawDecals.BloodMaterial.enableInstancing = true;

      //BTCustomRenderer_DrawDecals.UseCameraMethod = typeof(BTCustomRenderer).GetMethod("UseCamera", BindingFlags.Instance | BindingFlags.NonPublic);
      //if (BTCustomRenderer_DrawDecals.UseCameraMethod == null) {
        //CustomAmmoCategoriesLog.Log.LogWrite("Fail to get UseCamera method\n"); ;
        //return false;
      //}
      //CustomAmmoCategoriesLog.Log.LogWrite("Success get UseCamera method\n");
      BTCustomRenderer_DrawDecals.deferredDecalsBufferField = typeof(BTCustomRenderer).Assembly.GetType("BattleTech.Rendering.BTCustomRenderer+CustomCommandBuffers").GetField("deferredDecalsBuffer", BindingFlags.Instance | BindingFlags.Public);
      if (BTCustomRenderer_DrawDecals.deferredDecalsBufferField == null) {
        CustomAmmoCategoriesLog.Log.LogWrite("Fail to get deferredDecalsBuffer field\n"); ;
        return false;
      }
      BTCustomRenderer_DrawDecals.skipDecalsField = typeof(BTCustomRenderer).GetField("skipDecals", BindingFlags.Instance | BindingFlags.NonPublic);
      if (BTCustomRenderer_DrawDecals.skipDecalsField == null) {
        CustomAmmoCategoriesLog.Log.LogWrite("Fail to get skipDecals field\n"); ;
        return false;
      }
      BTCustomRenderer_DrawDecals.effectsQualityField = typeof(BTCustomRenderer).GetField("effectsQuality", BindingFlags.Static | BindingFlags.NonPublic);
      if (BTCustomRenderer_DrawDecals.effectsQualityField == null) {
        CustomAmmoCategoriesLog.Log.LogWrite("Fail to get effectsQuality field\n"); ;
        return false;
      }
      CustomAmmoCategoriesLog.Log.LogWrite(" success\n"); ;
      return true;
    }
    public static void Postfix(BTCustomRenderer __instance, Camera camera) {
      object customCommandBuffers = __instance.UseCameraMethod(camera); //BTCustomRenderer_DrawDecals.UseCameraMethod.Invoke(__instance, new object[1] { (object)camera });
      if (customCommandBuffers == null)
        return;
      CommandBuffer deferredDecalsBuffer = (CommandBuffer)BTCustomRenderer_DrawDecals.deferredDecalsBufferField.GetValue(customCommandBuffers);
      bool skipDecals = (bool)BTCustomRenderer_DrawDecals.skipDecalsField.GetValue(__instance);
      int effectsQuality = (int)BTCustomRenderer_DrawDecals.effectsQualityField.GetValue(null);
      if (!skipDecals) {
        BTDecal.DecalController.ProcessCommandBuffer(deferredDecalsBuffer, camera);
      }
      if (!Application.isPlaying || effectsQuality <= 0)
        return;
      if (BTCustomRenderer_DrawDecals.Scorches.Count > 0) {
        //CustomAmmoCategoriesLog.Log.LogWrite("draw scorches:"+ BTCustomRenderer_DrawDecals.Scorches.Count+ "\n"); ;
        for (int index1 = 0; index1 < BTCustomRenderer_DrawDecals.Scorches.Count; ++index1) {
          Matrix4x4[] matrices2 = BTCustomRenderer_DrawDecals.Scorches[index1].ToArray();
          int scorches = matrices2.Length;
          deferredDecalsBuffer.DrawMeshInstanced(BTDecal.DecalMesh.DecalMeshFull, 0, BTCustomRenderer_DrawDecals.ScorchMaterial, 0, matrices2, scorches, (MaterialPropertyBlock)null);
        }
      }
      if (BTCustomRenderer_DrawDecals.Bloods.Count > 0) {
        //CustomAmmoCategoriesLog.Log.LogWrite("draw scorches:"+ BTCustomRenderer_DrawDecals.Scorches.Count+ "\n"); ;
        for (int index1 = 0; index1 < BTCustomRenderer_DrawDecals.Bloods.Count; ++index1) {
          Matrix4x4[] matrices2 = BTCustomRenderer_DrawDecals.Bloods[index1].ToArray();
          int scorches = matrices2.Length;
          deferredDecalsBuffer.DrawMeshInstanced(BTDecal.DecalMesh.DecalMeshFull, 0, BTCustomRenderer_DrawDecals.BloodMaterial, 0, matrices2, scorches, (MaterialPropertyBlock)null);
        }
      }
    }
  }
  [HarmonyPatch(typeof(DestructibleUrbanFlimsy))]
  [HarmonyPatch("PlayDestructionVFX")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class DestructibleUrbanFlimsy_PlayDestructionVFX {
    private static HashSet<DestructibleUrbanFlimsy> isPlaySound = new HashSet<DestructibleUrbanFlimsy>();
    public static bool isPlayBloodSound(this DestructibleUrbanFlimsy obj) {
      return isPlaySound.Contains(obj);
    }
    public static void markPlayBloodSound(this DestructibleUrbanFlimsy obj) {
      isPlaySound.Add(obj);
    }
    public static void unmarkPlayBloodSound(this DestructibleUrbanFlimsy obj) {
      isPlaySound.Remove(obj);
    }
    public static void Postfix(DestructibleUrbanFlimsy __instance) {
      //float scale = CustomAmmoCategories.Settings.bloodSettings.DecalScale[];
      Log.M.TWL(0, "DestructibleUrbanFlimsy.PlayDestructionVFX");
      if (CustomAmmoCategories.Settings.bloodSettings.DecalScales.TryGetValue(__instance.flimsyType, out float scale) == false) {
        Log.M.WL(1,"Can't find scale for "+ __instance.flimsyType);
        return;
      }
      float roll = Random.Range(0f, 1f);
      if (roll > CustomAmmoCategories.Settings.bloodSettings.DrawBloodChance) {
        Log.M.WL(1, "roll fail "+roll +" > "+ CustomAmmoCategories.Settings.bloodSettings.DrawBloodChance);
        return;
      }
      switch (__instance.flimsyType) {
        case FlimsyDestructType.vehicleFiery:
        case FlimsyDestructType.smallVehicle:
        case FlimsyDestructType.mediumVehicle:
        case FlimsyDestructType.largeVehicle: {
            Log.M.TWL(0, "Add blood decal " + __instance.transform.position + " scale:" + scale + " name:" + __instance.transform.name);
            BTCustomRenderer_DrawDecals.AddBlood(__instance.transform.position, new Vector3(1f, 0f, 0f).normalized, new Vector3(scale, scale, scale));
            if (DestructibleUrbanFlimsy.Combat != null) { __instance.markPlayBloodSound(); }
          }; break;
      }
    }
  }
  [HarmonyPatch(typeof(DestructibleUrbanFlimsy))]
  [HarmonyPatch("PlayDestructionAudio")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class DestructibleUrbanFlimsy_PlayDestructionAudio {
    public static bool Prefix(DestructibleUrbanFlimsy __instance) {
      //float scale = CustomAmmoCategories.Settings.bloodSettings.DecalScale[];
      Log.S.TWL(0, "DestructibleUrbanFlimsy.PlayDestructionAudio");
      if (__instance.isPlayBloodSound()) {
        Log.S.WL(1, "playing blood sound");
        __instance.unmarkPlayBloodSound();
        if (CustomAmmoCategories.Settings.screamsIds.Count > 0) {
          string screamId = CustomAmmoCategories.Settings.screamsIds[Random.Range(0, CustomAmmoCategories.Settings.screamsIds.Count)];
          Log.S.WL(1, "scream id:"+screamId);
          CustomSoundHelper.SpawnAudioEmitter(screamId, __instance.thisTransform.position, false);
        }
        return false;
      }
      return true;
    }
  }

  [HarmonyPatch(typeof(PilotableActorRepresentation))]
  [HarmonyPatch("RefreshSurfaceType")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(bool) })]
  public static class PilotableActorRepresentation_RefreshSurfaceType {
    public static void Postfix(PilotableActorRepresentation __instance) {
    }
  }
  [HarmonyPatch(typeof(DataManager))]
  [HarmonyPatch("PooledInstantiate")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string), typeof(BattleTechResourceType), typeof(Vector3?), typeof(Quaternion?), typeof(Transform) })]
  public static class DataManager_PooledInstantiate {
    private static PropertyInfo pGameObjectPool = null;
    private static PropertyInfo pAssetBundleManager = null;
    private static List<Func<BattleTechResourceType, string, string>> IdFilters = new List<Func<BattleTechResourceType, string, string>>();
    private static List<Action<BattleTechResourceType,string, string, GameObject>> PoolPostProcessors = new List<Action<BattleTechResourceType, string, string, GameObject>>();
    public static void RegisterIdFilter(Func<BattleTechResourceType, string, string> filter) {
      IdFilters.Add(filter);
    }
    private static string InvokeIdFilters(BattleTechResourceType resourceType, string id) {
      foreach(Func<BattleTechResourceType, string, string> filter in IdFilters) {
        id = filter(resourceType, id);
      }
      return id;
    }
    public static bool Prepare() {
      pGameObjectPool = typeof(DataManager).GetProperty("GameObjectPool", BindingFlags.Instance | BindingFlags.NonPublic);
      if (pGameObjectPool == null) {
        Log.M.TWL(0, "DataManager.PooledInstantiate prepare can't find GameObjectPool", true);
        return false;
      }
      pAssetBundleManager = typeof(DataManager).GetProperty("AssetBundleManager", BindingFlags.Instance | BindingFlags.NonPublic);
      if (pAssetBundleManager == null) {
        Log.M.TWL(0, "DataManager.PooledInstantiate prepare can't find AssetBundleManager", true);
        return false;
      }
      return true;
    }
    public static PrefabCache GameObjectPool(this DataManager dataManager) {
      return (PrefabCache)pGameObjectPool.GetValue(dataManager, null);
    }
    public static AssetBundleManager AssetBundleManager(this DataManager dataManager) {
      return (AssetBundleManager)pAssetBundleManager.GetValue(dataManager, null);
    }
    public static Dictionary<string, LinkedList<GameObject>> gameObjectPool(this PrefabCache cache) {
      return Traverse.Create(cache).Field<Dictionary<string, LinkedList<GameObject>>>("gameObjectPool").Value;
    }
    public static Dictionary<string, PrefabCache.RST> gameObjectRST(this PrefabCache cache) {
      return Traverse.Create(cache).Field<Dictionary<string, PrefabCache.RST>>("gameObjectRST").Value;
    }
    public static Dictionary<string, UnityEngine.Object> prefabPool(this PrefabCache cache) {
      return Traverse.Create(cache).Field<Dictionary<string, UnityEngine.Object>>("prefabPool").Value;
    }
    public static void CreateObjectRST(this PrefabCache cache,string id, GameObject obj) {
      if (cache.gameObjectRST().ContainsKey(id)) { return; }
      PrefabCache.RST rst = new PrefabCache.RST(obj);
      cache.gameObjectRST().Add(id, rst);
    }
    public static GameObject PooledInstantiateEx(this PrefabCache cache,string id, Vector3? position = null, Quaternion? rotation = null, Transform parent = null, bool forceInstantiate = false) {
      if (forceInstantiate == false) {
        GameObject go = null;
        if(cache.gameObjectPool().TryGetValue(id, out LinkedList<GameObject> linkedList)) {
          do {
            if (linkedList.Count == 0) { break; }
            go = linkedList.First.Value;
            linkedList.RemoveFirst();
            if(go == null) {
              Log.M.TWL(0,"Some moron pooled null as "+id+" rot in hell!!!",true);
              continue;
            }
            try {
              go.transform.SetParent((Transform)null);
            } catch(Exception e) {
              go = null;
              Log.M.TWL(0, "Some moron pooled disposed object as " + id + " rot in hell!!!", true);
              Log.M.TWL(0, e.ToString(), true);
            }
          } while ((go == null)&&(linkedList.Count > 0));
        }
        if (go != null) {
          go.transform.SetParent((Transform)null);
          cache.gameObjectRST()[id].Apply(go);
          go.SetActive(true);
          if ((UnityEngine.Object)parent != (UnityEngine.Object)null) {
            Scene scene1 = go.scene;
            Scene scene2 = parent.gameObject.scene;
            if (scene1.name != scene2.name && scene2.name != "DontDestroyOnLoad")
              SceneManager.MoveGameObjectToScene(go, parent.gameObject.scene);
            go.transform.SetParent(parent);
          } else {
            go.transform.SetParent((Transform)null);
            if (position.HasValue)
              go.transform.position = position.Value;
            if (rotation.HasValue)
              go.transform.rotation = rotation.Value;
            SceneManager.MoveGameObjectToScene(go, SceneManager.GetActiveScene());
          }
          return go;
        }
      }
      UnityEngine.Object original;
      if (!cache.prefabPool().TryGetValue(id, out original)) { return (GameObject)null; };
      GameObject go1;
      if (position.HasValue || rotation.HasValue) {
        Transform transform = (original as GameObject).transform;
        go1 = (GameObject)UnityEngine.Object.Instantiate(original, position.HasValue ? position.Value : transform.position, rotation.HasValue ? rotation.Value : transform.rotation);
      } else
        go1 = (GameObject)UnityEngine.Object.Instantiate(original);
      cache.CreateObjectRST(id, go1);
      if ((UnityEngine.Object)parent != (UnityEngine.Object)null) {
        Scene scene1 = go1.scene;
        Scene scene2 = parent.gameObject.scene;
        if (scene1.name != scene2.name && scene2.name != "DontDestroyOnLoad")
          SceneManager.MoveGameObjectToScene(go1, parent.gameObject.scene);
        go1.transform.SetParent(parent);
      } else {
        go1.transform.SetParent((Transform)null);
        SceneManager.MoveGameObjectToScene(go1, SceneManager.GetActiveScene());
      }
      return go1;
    }
    public static bool Prefix(DataManager __instance, string id, BattleTechResourceType resourceType, Vector3? position, Quaternion? rotation, Transform parent, ref GameObject __result) {
      Log.LogWrite("DataManager.PooledInstantiate prefix " + id + "\n");
      try {
        if ((UnityEngine.Object)__instance.GameObjectPool() == (UnityEngine.Object)null) { __result = null; return false; }
        if (!__instance.GameObjectPool().IsPrefabInPool(id)) {
          VersionManifestEntry versionManifestEntry = __instance.ResourceLocator.EntryByID(id, resourceType, false);
          if (versionManifestEntry != null) {
            if (versionManifestEntry.IsResourcesAsset)
              __instance.GameObjectPool().AddPrefabToPool(id, Resources.Load(versionManifestEntry.ResourcesLoadPath));
            else if (versionManifestEntry.IsAssetBundled) {
              GameObject gameObject = (UnityEngine.Object)__instance.AssetBundleManager() != (UnityEngine.Object)null ? __instance.AssetBundleManager().GetAssetFromBundle<GameObject>(id, versionManifestEntry.AssetBundleName) : (GameObject)null;
              if ((UnityEngine.Object)gameObject != (UnityEngine.Object)null)
                __instance.GameObjectPool().AddPrefabToPool(id, (UnityEngine.Object)gameObject);
            }
          }
        }
        if (!__instance.GameObjectPool().IsPrefabInPool(id)) { __result = null; return false; }
        __result = __instance.GameObjectPool().PooledInstantiateEx(id, position, rotation, parent, false);
        return false;
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString());
        return true;
      }
    }
    public static void Postfix(DataManager __instance, string id, BattleTechResourceType resourceType, ref GameObject __result) {
      try {
        if (resourceType != BattleTechResourceType.Prefab) { return; }
        Log.LogWrite("DataManager.PooledInstantiate prefab " + id + "\n");
        if ((UnityEngine.Object)__result == (UnityEngine.Object)null) {
          CustomAmmoCategoriesLog.Log.LogWrite("Can't find " + id + " in in-game prefabs\n");
          if (CACMain.Core.AdditinalFXObjects.ContainsKey(id)) {
            CustomAmmoCategoriesLog.Log.LogWrite("Found in additional prefabs\n");
            __result = GameObject.Instantiate(CACMain.Core.AdditinalFXObjects[id]);
            __result.RestoreScaleColor();
          } else {
            CustomAmmoCategoriesLog.Log.LogWrite(" can't spawn prefab " + id + " it is absent in pool,in-game assets and external assets\n", true);
            return;
          }
        } else {
          __result.RestoreScaleColor();
        }
      } catch (Exception e) { Log.LogWrite(e.ToString() + "\n", true); }
    }
  }
  [HarmonyPatch(typeof(MapMetaDataExporter))]
  [HarmonyPatch("GenerateTerrainData")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Terrain), typeof(bool) })]
  public static class MapMetaDataExporter_GenerateTerrainData {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetConstructor = AccessTools.Constructor(typeof(MapTerrainDataCell));
      ConstructorInfo replacConstructor = null; 
      foreach(ConstructorInfo info in AccessTools.GetDeclaredConstructors(typeof(MapTerrainDataCellEx))) {
        if (info.IsPublic == true) { replacConstructor = info; }
      }
      return Transpilers.MethodReplacer(instructions, targetConstructor, replacConstructor);
    }
    static void Postfix(MapMetaDataExporter __instance, Terrain terrain, bool force) {
      int xmax = __instance.mapMetaData.mapTerrainDataCells.GetLength(0);
      int ymax = __instance.mapMetaData.mapTerrainDataCells.GetLength(1);
      CustomAmmoCategoriesLog.Log.LogWrite("MapMetaDataExporter.GenerateTerrainData " + xmax + " X " + ymax + "\n");
      for (int x = 0; x < xmax; ++x) {
        for (int y = 0; y < ymax; ++y) {
          if (__instance.mapMetaData.mapTerrainDataCells[x, y] is MapTerrainDataCellEx) {
            //CustomAmmoCategoriesLog.Log.LogWrite(" " + x + " X " + y + " is ext cell\n");
            (__instance.mapMetaData.mapTerrainDataCells[x, y] as MapTerrainDataCellEx).x = x;
            (__instance.mapMetaData.mapTerrainDataCells[x, y] as MapTerrainDataCellEx).y = y;
          }
        }
      }
    }
  }
}
