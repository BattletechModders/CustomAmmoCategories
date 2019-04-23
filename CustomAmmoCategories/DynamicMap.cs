using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BattleTech;
using BattleTech.Assetbundles;
using BattleTech.Data;
using BattleTech.Rendering;
using BattleTech.Rendering.Mood;
using BattleTech.Rendering.Trees;
using BattleTech.UI;
using CustAmmoCategories;
using CustomAmmoCategoriesPatches;
using Harmony;
using HBS.Util;
using Localize;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace CustAmmoCategories {
  public static partial class CustomAmmoCategories {
    /*public static DesignMaskDef getWeaponDesignImpactMask(Weapon weapon, TerrainMaskFlags terrainMaskFlags) {
      Dictionary<TerrainMaskFlags, string> dmaskids = new Dictionary<TerrainMaskFlags, string>();
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
        string CurrentAmmoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
        foreach (var sdm in extAmmoDef.SurfaceImpactDesignMaskId) {
          if (dmaskids.ContainsKey(sdm.Key) == false) {
            dmaskids.Add(sdm.Key, sdm.Value);
          }
        }
      }
      if (dmaskids.ContainsKey(terrainMaskFlags)) {
        if (DynamicMapHelper.loadedMasksDef.ContainsKey(dmaskids[terrainMaskFlags])) {
          return DynamicMapHelper.loadedMasksDef[dmaskids[terrainMaskFlags]];
        }
      }
      return null;
    }*/
    public static bool getWeaponBecomesDangerousOnImpact(Weapon weapon) {
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
        string CurrentAmmoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
        return extAmmoDef.SurfaceBecomeDangerousOnImpact == TripleBoolean.True;
      }
      return false;
    }
    public static int MineFieldRadius(this Weapon weapon) {
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
        string CurrentAmmoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
        return extAmmoDef.MineFieldRadius;
      }
      return 0;
    }
    public static int MineFieldCount(this Weapon weapon) {
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
        string CurrentAmmoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
        return extAmmoDef.MineFieldCount;
      }
      return 0;
    }
    public static float MineFieldHitChance(this Weapon weapon) {
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
        string CurrentAmmoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
        return extAmmoDef.MineFieldHitChance;
      }
      return 0f;
    }
    public static float MineFieldDamage(this Weapon weapon) {
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
        string CurrentAmmoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
        return extAmmoDef.MineFieldDamage;
      }
      return 0f;
    }
    public static float MineFieldInstability(this Weapon weapon) {
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
        string CurrentAmmoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
        return extAmmoDef.MineFieldInstability;
      }
      return 0f;
    }
    public static float MineFieldHeat(this Weapon weapon) {
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
        string CurrentAmmoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
        return extAmmoDef.MineFieldHeat;
      }
      return 0f;
    }
    public static ExtWeaponDef exDef(this Weapon weapon) {
      return CustomAmmoCategories.getExtWeaponDef(weapon.defId);
    }
    public static float FireTerrainChance(this Weapon weapon) {
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      float result = extWeapon.FireTerrainChance;
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
        string CurrentAmmoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
        result += extAmmoDef.FireTerrainChance;
      }
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == true) {
        string modeId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
        if (extWeapon.Modes.ContainsKey(modeId)) {
          WeaponMode mode = extWeapon.Modes[modeId];
          result += mode.FireTerrainChance;
        }
      }
      return result;
    }
    public static int FireDurationWithoutForest(this Weapon weapon) {
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      int result = extWeapon.FireDurationWithoutForest;
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
        string CurrentAmmoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
        result += extAmmoDef.FireDurationWithoutForest;
      }
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == true) {
        string modeId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
        if (extWeapon.Modes.ContainsKey(modeId)) {
          WeaponMode mode = extWeapon.Modes[modeId];
          result += mode.FireDurationWithoutForest;
        }
      }
      return result;
    }
    public static int FireTerrainStrength(this Weapon weapon) {
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      int result = extWeapon.FireTerrainStrength;
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
        string CurrentAmmoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
        result += extAmmoDef.FireTerrainStrength;
      }
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == true) {
        string modeId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
        if (extWeapon.Modes.ContainsKey(modeId)) {
          WeaponMode mode = extWeapon.Modes[modeId];
          result += mode.FireTerrainStrength;
        }
      }
      return result;
    }
    public static int ClearMineFieldRadius(this Weapon weapon) {
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      int result = extWeapon.ClearMineFieldRadius;
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
        string CurrentAmmoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
        result += extAmmoDef.ClearMineFieldRadius;
      }
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == true) {
        string modeId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
        if (extWeapon.Modes.ContainsKey(modeId)) {
          WeaponMode mode = extWeapon.Modes[modeId];
          result += mode.ClearMineFieldRadius;
        }
      }
      return result;
    }
    public static bool FireOnSuccessHit(this Weapon weapon) {
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      TripleBoolean result = extWeapon.FireOnSuccessHit;
      if (result == TripleBoolean.NotSet) {
        if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == true) {
          string modeId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
          if (extWeapon.Modes.ContainsKey(modeId)) {
            WeaponMode mode = extWeapon.Modes[modeId];
            result = mode.FireOnSuccessHit;
          }
        }
      }
      if (result == TripleBoolean.NotSet) {
        if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
          string CurrentAmmoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
          ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
          result = extAmmoDef.FireOnSuccessHit;
        }
      }
      return result == TripleBoolean.True;
    }
    public static int FireTerrainCellRadius(this Weapon weapon) {
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      int result = extWeapon.FireTerrainCellRadius;
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
        string CurrentAmmoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
        result += extAmmoDef.FireTerrainCellRadius;
      }
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == true) {
        string modeId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
        if (extWeapon.Modes.ContainsKey(modeId)) {
          WeaponMode mode = extWeapon.Modes[modeId];
          result += extWeapon.FireTerrainCellRadius;
        }
      }
      return result;
    }
    public static DesignMaskDef tempDesignMask(this Weapon weapon, out int turns, out string vfx, out Vector3 scale, out int radius) {
      turns = 0;
      vfx = string.Empty;
      scale = new Vector3();
      radius = 0;
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
        string CurrentAmmoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
        if (string.IsNullOrEmpty(extAmmoDef.tempDesignMaskOnImpact) == false) {
          if (DynamicMapHelper.loadedMasksDef.ContainsKey(extAmmoDef.tempDesignMaskOnImpact)) {
            turns = extAmmoDef.tempDesignMaskOnImpactTurns;
            vfx = extAmmoDef.LongVFXOnImpact;
            scale = new Vector3(extAmmoDef.LongVFXOnImpactScaleX, extAmmoDef.LongVFXOnImpactScaleY, extAmmoDef.LongVFXOnImpactScaleZ);
            radius = extAmmoDef.tempDesignMaskCellRadius;
            return DynamicMapHelper.loadedMasksDef[extAmmoDef.tempDesignMaskOnImpact];
          }
        }
      }
      return null;
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
        //this.spawnedObject.SetActive(false);
        //this.Combat.DataManager.PoolGameObject(this.prefabName, this.spawnedObject);
        //ParticleSystem component = this.spawnedObject.GetComponent<ParticleSystem>();
        //if ((UnityEngine.Object)component != (UnityEngine.Object)null) {
        //component.Stop(true);
        //}
        GameObject.Destroy(this.spawnedObject);
      } catch (Exception e) {
        CustomAmmoCategoriesLog.Log.LogWrite("Cleanup exception: " + e.ToString() + "\n", true);
        CustomAmmoCategoriesLog.Log.LogWrite("nulling spawned object directly\n", true);
        this.spawnedObject = null;
      }
      this.spawnedObject = null;
      CustomAmmoCategoriesLog.Log.LogWrite("Finish cleaning " + this.prefabName + "\n");
    }
    public void SpawnSelf(CombatGameState Combat) {
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
      gameObject.transform.position = this.worldPosition;
      gameObject.transform.localScale.Set(scale.x, scale.y, scale.z);
      if (!this.keepPrefabRotation)
        gameObject.transform.rotation = this.worldRotation;
      if (this.playFX) {
        ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
        if (component != null) {
          component.transform.localScale.Set(scale.x, scale.y, scale.z);
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
  }
  public class MineField {
    public float HitChance;
    public float damage;
    public float heat;
    public float instability;
    public float count;
    public AbstractActor owner;
    public Weapon weapon;
    public MineField(float hc, float d, float h, float i, int c, AbstractActor o, Weapon w) {
      HitChance = hc;
      damage = d;
      owner = o;
      weapon = w;
      heat = h;
      instability = i;
      count = c;
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
  public class MapTerrainHexCell {
    public int x;
    public int y;
    public int mapX;
    public int mapY;
    public List<MapTerrainDataCellEx> terrainCells;
    public Dictionary<string, tempTerrainVFXEffect> tempVFXEffects;
    public ObjectSpawnDataSelf burnEffect;
    public CombatGameState Combat;
    public Weapon burningWeapon;
    public bool isHasForest;
    public bool wasHasForest;
    public bool expandingThisTurn;
    public int burnEffectCounter;
    public void deleteTrees(HashSet<object> redrawTreeDatas) {
      foreach(MapTerrainDataCellEx cell in terrainCells) {
        CustomAmmoCategoriesLog.Log.LogWrite("Deleting trees at cell "+cell.x+":"+cell.y+" "+cell.mapMetaData.getWorldPos(new Point(cell.y,cell.x))+" count:"+ cell.trees.Count + "\n");
        foreach(CACDynamicTree tree in cell.trees) {
          List<object> redrawList = tree.delTree();
          foreach(object redrawItem in redrawList) {
            redrawTreeDatas.Add(redrawItem);
          }
        }
      }
    }
    public MapPoint mapPoint() {
      return new MapPoint(this.x, this.y);
    }
    public bool hasTempEffects() {
      return tempVFXEffects.Count > 0;
    }
    public void tempVFXTick() {
      CustomAmmoCategoriesLog.Log.LogWrite("tempVFXTick:" + this.x + ":" + this.y + "\n");
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
    public void addTempTerrainVFX(CombatGameState combat, string prefabVFX, DesignMaskDef addMask, int counter, Vector3 scale) {
      this.Combat = combat;
      CustomAmmoCategoriesLog.Log.LogWrite("addTempTerrainVFX(" + prefabVFX + "," + addMask.Id + "," + counter + ")\n");
      if (tempVFXEffects == null) {
        CustomAmmoCategoriesLog.Log.LogWrite(" tempVFXEffects is null\n");
        return;
      }
      if (string.IsNullOrEmpty(prefabVFX) == false) {
        if (tempVFXEffects.ContainsKey(prefabVFX) == true) {
          tempVFXEffects[prefabVFX].counter += counter;
        } else {
          Point p = new Point();
          p.X = this.mapY;
          p.Z = this.mapX;
          Vector3 pos = Combat.MapMetaData.getWorldPos(p);
          pos.y = Combat.MapMetaData.GetLerpedHeightAt(pos);
          tempTerrainVFXEffect tmpEffect = new tempTerrainVFXEffect(Combat, prefabVFX, pos, scale, counter);
          tempVFXEffects.Add(prefabVFX, tmpEffect);
          DynamicMapHelper.tempEffectHexes.Add(this.mapPoint());
        }
      }
      if (addMask != null) {
        foreach (MapTerrainDataCellEx cell in this.terrainCells) {
          if (cell == null) {
            CustomAmmoCategoriesLog.Log.LogWrite(" cell is null\n");
            return;
          }
          cell.AddDesignMask(addMask, counter);
        }
      }
    }
    //public static readonly string firePrefab = "vfxPrfPrtl_fireTerrain_lrgLoop";
    //public static readonly string firePrefab = "vfxPrfPrtl_artillerySmokeSignal_loop";
    public int CountTrees() {
      int result = 0;
      foreach (MapTerrainDataCellEx cell in this.terrainCells) {
        result += cell.trees.Count;
      }
      return result;
    }
    public void applyBurnOutVisuals() {
      burnEffect.CleanupSelf();
      Point p = new Point();
      p.X = this.mapY;
      p.Z = this.mapX;
      Vector3 pos = Combat.MapMetaData.getWorldPos(p);
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
      Point p = new Point();
      p.X = this.mapY;
      p.Z = this.mapX;
      Vector3 pos = Combat.MapMetaData.getWorldPos(p);
      pos.y = Combat.MapMetaData.GetLerpedHeightAt(pos);
      CustomAmmoCategoriesLog.Log.LogWrite("Spawning fire at " + pos + "\n");
      pos.x += CustomAmmoCategories.Settings.BurningOffsetX;
      pos.y += CustomAmmoCategories.Settings.BurningOffsetY;
      pos.z += CustomAmmoCategories.Settings.BurningOffsetZ;
      if (burnEffect != null) { burnEffect.CleanupSelf(); };
      burnEffect = DynamicMapHelper.SpawnFXObject(Combat, CustomAmmoCategories.Settings.BurningFX, pos, new Vector3(CustomAmmoCategories.Settings.BurningScaleX, CustomAmmoCategories.Settings.BurningScaleY, CustomAmmoCategories.Settings.BurningScaleZ));
      //HashSet<object> redrawTreeDatas = new HashSet<object>();
      //this.deleteTrees(redrawTreeDatas);
      //foreach (object redrawItem in redrawTreeDatas) {
      //CACDynamicTree.redrawTrees(redrawTreeDatas);
      //}
    }
    public void UpdateCellsBurn(Weapon weapon, int count, int strength) {
      foreach (MapTerrainDataCellEx cell in terrainCells) {
        cell.burnUpdate(weapon, count, strength);
      }
    }
    public void SetCellsBurn(Weapon weapon, int count, int countNoForest, int strength, int strengthNoForest) {
      foreach (MapTerrainDataCellEx cell in terrainCells) {
        CustomAmmoCategoriesLog.Log.LogWrite("SetCellsBurn:" + cell.x + ":" + cell.y + ":" + SplatMapInfo.IsForest(cell.terrainMask) + ":" + cell.CantHaveForest + "\n");
        if (SplatMapInfo.IsForest(cell.terrainMask) && (cell.CantHaveForest == false)) {
          if ((count == 0) || (strength == 0)) { continue; }
          cell.burnUpdate(weapon, count, strength);
        } else {
          if ((countNoForest == 0) || (strengthNoForest == 0)) { continue; }
          cell.burnUpdate(weapon, countNoForest, strengthNoForest);
        }
      }
    }
    public bool TryBurnCell(Weapon weapon) {
      CustomAmmoCategoriesLog.Log.LogWrite("Try burn cell " + weapon.UIName + " Chance:" + weapon.FireTerrainChance() + " hasForest:" + isHasForest + "\n");
      if (weapon.FireTerrainChance() > CustomAmmoCategories.Epsilon) {
        if ((weapon.FireDurationWithoutForest() <= 0) && (this.isHasForest == false)) {
          CustomAmmoCategoriesLog.Log.LogWrite(" no forest and no self burn\n");
          return false;
        }
        float roll = Random.Range(0f, 1f);
        if (roll > weapon.FireTerrainChance()) {
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
        if (burnEffectCounter < weapon.FireDurationWithoutForest()) {
          burnEffectCounter = weapon.FireDurationWithoutForest();
          this.UpdateCellsBurn(weapon, burnEffectCounter, weapon.FireTerrainStrength());
          this.expandingThisTurn = true;
          burningWeapon = weapon;
        }
        return false;
      }
      burnEffectCounter = weapon.FireDurationWithoutForest();
      if (isHasForest && (burnEffectCounter < CustomAmmoCategories.Settings.BurningForestTurns)) { burnEffectCounter = CustomAmmoCategories.Settings.BurningForestTurns; };
      if (burnEffectCounter <= 0) { return false; };
      int burnStrength = weapon.FireTerrainStrength();
      if (isHasForest && (burnStrength < CustomAmmoCategories.Settings.BurningForestStrength)) { burnStrength = CustomAmmoCategories.Settings.BurningForestStrength; };
      if (burnStrength <= 0) { return false; };
      burningWeapon = weapon;
      Combat = weapon.parent.Combat;
      expandingThisTurn = true;
      applyBurnVisuals();
      SetCellsBurn(weapon, burnEffectCounter, weapon.FireDurationWithoutForest(), burnStrength, weapon.FireTerrainStrength());
      isHasForest = false;
      return true;
    }
    public bool TryExpand(Weapon weapon) {
      CustomAmmoCategoriesLog.Log.LogWrite("  test expand:" + this.x + ":" + this.y + "\n");
      if (isHasForest == false) {
        CustomAmmoCategoriesLog.Log.LogWrite("  no forest\n");
        return false;
      };
      if (burnEffectCounter > 0) {
        CustomAmmoCategoriesLog.Log.LogWrite("  burning already\n");
        return false;
      };
      float roll = Random.Range(0f, 1f);
      if (roll > CustomAmmoCategories.Settings.BurningForestStrength) {
        CustomAmmoCategoriesLog.Log.LogWrite("  roll fail\n");
        return false;
      };
      burningWeapon = weapon;
      Combat = weapon.parent.Combat;
      applyBurnVisuals();
      burnEffectCounter = CustomAmmoCategories.Settings.BurningForestTurns;
      SetCellsBurn(weapon, burnEffectCounter, 0, CustomAmmoCategories.Settings.BurningForestStrength, 0);
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
      x = 0;
      y = 0;
      mapX = 0;
      mapY = 0;
      terrainCells = new List<MapTerrainDataCellEx>();
      tempVFXEffects = new Dictionary<string, tempTerrainVFXEffect>();
      burnEffect = null;
      isHasForest = false;
      wasHasForest = false;
      burnEffectCounter = 0;
      Combat = null;
      expandingThisTurn = false;
      burningWeapon = null;
    }
    public static List<MapTerrainHexCell> listHexCellsByCellRadius(MapTerrainDataCellEx ccell, int r) {
      HashSet<MapPoint> hexCells = new HashSet<MapPoint>();
      List<MapTerrainHexCell> result = new List<MapTerrainHexCell>();
      List<MapPoint> affectedCells = MapPoint.calcMapCircle(new MapPoint(ccell.x, ccell.y), r);
      foreach (MapPoint aCell in affectedCells) {
        if (aCell.x < 0) { continue; }
        if (aCell.y < 0) { continue; }
        if (aCell.x >= ccell.mapMetaData.mapTerrainDataCells.GetLength(0)) { continue; }
        if (aCell.y >= ccell.mapMetaData.mapTerrainDataCells.GetLength(1)) { continue; }
        MapTerrainDataCellEx cell = ccell.mapMetaData.mapTerrainDataCells[aCell.x, aCell.y] as MapTerrainDataCellEx;
        if (cell == null) { continue; }
        MapPoint hexCell = new MapPoint(cell.hexCell.x, cell.hexCell.y);
        if (hexCells.Contains(hexCell)) { continue; };
        hexCells.Add(hexCell);
        result.Add(cell.hexCell);
      }
      return result;
    }
  }
  public class MapTerrainDataCellEx : MapTerrainDataCell {
    public int x;
    public int y;
    public bool wasForest;
    public bool wasCustom;
    public bool wasRoad;
    public DesignMaskDef CustomDesignMask;
    public List<MineField> MineField;
    public int BurningCounter;
    public int BurningStrength;
    public Weapon BurningWeapon;
    public bool CantHaveForest;
    public MapTerrainHexCell hexCell;
    public Dictionary<string, int> tempDesignMaskCounters;
    public DesignMaskDef tempDesignMask;
    public List<CACDynamicTree> trees;
    public MapPoint mapPoint() {
      return new MapPoint(this.x, this.y);
    }
    public void AddDesignMask(DesignMaskDef addMask, int counter) {
      CustomAmmoCategoriesLog.Log.LogWrite("AddDesignMask(" + addMask.Id + "," + counter + "):" + this.x + ":" + this.y + "\n");
      if (counter <= 0) { return; }
      if (tempDesignMaskCounters.ContainsKey(addMask.Id) == true) {
        tempDesignMaskCounters[addMask.Id] += counter;
        CustomAmmoCategoriesLog.Log.LogWrite(" +time:" + tempDesignMaskCounters[addMask.Id] + "\n");
        return;
      }
      CustomAmmoCategoriesLog.Log.LogWrite(" new mask\n");
      List<string> maskId = this.tempDesignMaskCounters.Keys.ToList<string>();
      CustomAmmoCategoriesLog.Log.LogWrite(" already have masks:" + maskId.Count + "\n");
      maskId.Sort();
      DesignMaskDef curMask = this.mapMetaData.GetPriorityDesignMask(this);
      if (curMask != null) { if (maskId.Count == 0) { maskId.Add(curMask.Id); }; };
      CustomAmmoCategoriesLog.Log.LogWrite(" curmask " + ((curMask == null) ? "null" : curMask.Id) + ":" + this.terrainMask + "\n");
      tempDesignMask = CustomAmmoCategories.createDesignMask(maskId, curMask, addMask);
      CustomAmmoCategoriesLog.Log.LogWrite(" new mask " + ((tempDesignMask == null) ? "null" : tempDesignMask.Id) + "\n");
      if (curMask != null) {
        if (curMask.Id != tempDesignMask.Id) {
          this.tempDesignMaskCounters.Add(addMask.Id, counter);
        }
      } else {
        this.tempDesignMaskCounters.Add(addMask.Id, counter);
      }
      DynamicMapHelper.tempMaskCells.Add(this.mapPoint());
    }

    public void ReconstructTempDesignMask() {
      CustomAmmoCategoriesLog.Log.LogWrite("Reconstructin design mask:" + this.x + ":" + this.y + "\n");
      tempDesignMask = null;
      DesignMaskDef baseMask = this.mapMetaData.GetPriorityDesignMask(this);
      List<string> maskId = this.tempDesignMaskCounters.Keys.ToList<string>();
      if (maskId.Count == 0) {
        CustomAmmoCategoriesLog.Log.LogWrite(" no need temp mask\n");
        return;
      }
      if (baseMask != null) { maskId.Add(baseMask.Id); };
      maskId.Sort();
      string newMaskId = maskId.DesignMaskId();
      CustomAmmoCategoriesLog.Log.LogWrite(" current mask:" + newMaskId + "\n");
      if (CustomAmmoCategories.tempDesignMasksDefs.ContainsKey(newMaskId)) {
        CustomAmmoCategoriesLog.Log.LogWrite(" already in cache\n");
        tempDesignMask = CustomAmmoCategories.tempDesignMasksDefs[newMaskId];
        return;
      }
      CustomAmmoCategoriesLog.Log.LogWrite(" constructing\n");
      List<string> constructingMask = new List<string>();
      DesignMaskDef newDesignMask = null;
      if (baseMask != null) {
        newDesignMask = baseMask; constructingMask.Add(baseMask.Id); maskId.RemoveAt(0);
        CustomAmmoCategoriesLog.Log.LogWrite(" base mask(b):" + newDesignMask.Id + "\n");
      } else {
        do {
          if (DynamicMapHelper.loadedMasksDef.ContainsKey(maskId[0])) {
            constructingMask.Add(maskId[0]);
            newDesignMask = DynamicMapHelper.loadedMasksDef[maskId[0]];
            maskId.RemoveAt(0);
            CustomAmmoCategoriesLog.Log.LogWrite(" base mask(t):" + newDesignMask.Id + "\n");
            break;
          } else {
            maskId.RemoveAt(0);
          }
        } while (maskId.Count > 0);
      }
      while (maskId.Count > 0) {
        if (DynamicMapHelper.loadedMasksDef.ContainsKey(maskId[0])) {
          CustomAmmoCategoriesLog.Log.LogWrite(" adding mask to:" + newDesignMask.Id + " " + DynamicMapHelper.loadedMasksDef[maskId[0]].Id + "\n");
          newDesignMask = CustomAmmoCategories.createDesignMask(constructingMask, newDesignMask, DynamicMapHelper.loadedMasksDef[maskId[0]]);
          constructingMask.Add(maskId[0]);
          constructingMask.Sort();
          CustomAmmoCategoriesLog.Log.LogWrite(" result:" + newDesignMask.Id + "\n");
          maskId.RemoveAt(0);
        }
      }
      if (newDesignMask != null) {
        if (baseMask != null) {
          if (baseMask.Id != newDesignMask.Id) { this.tempDesignMask = newDesignMask; }
        } else {
          this.tempDesignMask = newDesignMask;
        }
      }
    }
    public void RemoveDesignMask(string id) {
      if (tempDesignMaskCounters.ContainsKey(id) == false) { return; }
      tempDesignMaskCounters.Remove(id);
      List<string> maskId = this.tempDesignMaskCounters.Keys.ToList<string>();
      if (maskId.Count == 0) { this.tempDesignMask = null; return; };
      this.ReconstructTempDesignMask();
    }
    public void tempMaskTick() {
      CustomAmmoCategoriesLog.Log.LogWrite("Temp mask tick:" + this.x + ":" + this.y + "\n");
      List<string> keys = this.tempDesignMaskCounters.Keys.ToList<string>();
      foreach (string tdm in keys) {
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
          }
        } else {
          this.AddTerrainMask(TerrainMaskFlags.Forest);
        }
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
      /*foreach(int treeIndex in this.trees) {
        TreeInstance tree = Terrain.activeTerrain.terrainData.treeInstances[treeIndex];
        tree.widthScale = 0f;
        tree.heightScale = 0f;
        Terrain.activeTerrain.terrainData.SetTreeInstance(treeIndex,tree);
      }*/
      //Terrain.activeTerrain.terrainData.treeInstances = new TreeInstance[0] { };
      CustomAmmoCategoriesLog.Log.LogWrite("burn cell " + this.x + ":" + this.y + ": is forest: " + SplatMapInfo.IsForest(this.terrainMask) + " cantforest:" + this.CantHaveForest + " trees count:"+ this.trees.Count + "\n");
      if (SplatMapInfo.IsForest(this.terrainMask)) {
        if (this.CantHaveForest) {
          if ((counter > 0) && (strength > 0)) {
            this.BurningWeapon = weapon;
            //this.wasForest = false;
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
          this.BurningCounter = (counter > CustomAmmoCategories.Settings.BurningForestTurns) ? counter : CustomAmmoCategories.Settings.BurningForestTurns;
          this.BurningStrength = (strength > CustomAmmoCategories.Settings.BurningForestStrength) ? strength : CustomAmmoCategories.Settings.BurningForestStrength;
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
      x = -1; y = -1; CustomDesignMask = null; MineField = new List<MineField>(); hexCell = null;
      BurningCounter = 0;
      BurningStrength = 0;
      BurningWeapon = null;
      wasForest = false;
      wasCustom = false;
      wasRoad = false;
      CantHaveForest = false;
      trees = new List<CACDynamicTree>();
      tempDesignMask = null;
      tempDesignMaskCounters = new Dictionary<string, int>();
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
  public static class DynamicMapHelper {
    public static Dictionary<string, DesignMaskDef> loadedMasksDef = new Dictionary<string, DesignMaskDef>();
    public static MapTerrainHexCell[,] hexGrid = null;
    public static List<MapTerrainHexCell> burningHexes = new List<MapTerrainHexCell>();
    public static HashSet<MapPoint> tempEffectHexes = new HashSet<MapPoint>();
    public static HashSet<MapPoint> tempMaskCells = new HashSet<MapPoint>();
    public static MapMetaData mapMetaData = null;
    public static void ClearTerrain() {
      CustomAmmoCategoriesLog.Log.LogWrite("ClearTerrain\n");
      DynamicMapHelper.burningHexes.Clear();
      DynamicMapHelper.tempEffectHexes.Clear();
      DynamicMapHelper.tempMaskCells.Clear();
      try {
        if (DynamicMapHelper.hexGrid != null) {
          int hmax = DynamicMapHelper.hexGrid.GetLength(0);
          int hmay = DynamicMapHelper.hexGrid.GetLength(1);
          CustomAmmoCategoriesLog.Log.LogWrite(" size:" + hmax + "x" + hmay + "\n");
          for (int hx = 0; hx < hmax; ++hx) {
            for (int hy = 0; hy < hmay; ++hy) {
              //CustomAmmoCategoriesLog.Log.LogWrite("  hex:" + hx + ":" + hy + "\n");
              try {
                MapTerrainHexCell hcell = DynamicMapHelper.hexGrid[hx, hy];
                if (hcell == null) {
                  CustomAmmoCategoriesLog.Log.LogWrite("  hex cell is null\n");
                  continue;
                }
                hcell.clearTempVFXs();
                if (hcell.burnEffect == null) {
                  //CustomAmmoCategoriesLog.Log.LogWrite("  no terrain effect ever setted. No clean needed\n");
                  continue;
                }
                if (hcell.burnEffect.spawnedObject == null) {
                  //CustomAmmoCategoriesLog.Log.LogWrite("  already clean\n");
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
      for (int index = 0; index < DynamicMapHelper.burningHexes.Count;++index) {
        MapTerrainHexCell hex = burningHexes[index];
        hex.FireTick();
        hex.expandingThisTurn = true;
      }
      for (int index = 0; index < DynamicMapHelper.burningHexes.Count; ++index) {
        MapTerrainHexCell hex = burningHexes[index];
        if (hex.expandingThisTurn == false) { continue; };
        int hx = hex.x;
        int hy = hex.y;
        CustomAmmoCategoriesLog.Log.LogWrite(" expanding:" + hx + ":" + hy + "\n");
        if (hx > 0) {
          if (DynamicMapHelper.hexGrid[hx - 1, hy].TryExpand(hex.burningWeapon)) { DynamicMapHelper.burningHexes.Add(DynamicMapHelper.hexGrid[hx - 1, hy]); }
          if (hy > 0) { if (DynamicMapHelper.hexGrid[hx - 1, hy - 1].TryExpand(hex.burningWeapon)) { DynamicMapHelper.burningHexes.Add(DynamicMapHelper.hexGrid[hx - 1, hy - 1]); }; };
        }
        if (hy > 0) { if (DynamicMapHelper.hexGrid[hx, hy - 1].TryExpand(hex.burningWeapon)) { DynamicMapHelper.burningHexes.Add(DynamicMapHelper.hexGrid[hx, hy - 1]); }; };
        if (hx < (DynamicMapHelper.hexGrid.GetLength(0) - 1)) {
          if (DynamicMapHelper.hexGrid[hx + 1, hy].TryExpand(hex.burningWeapon)) { DynamicMapHelper.burningHexes.Add(DynamicMapHelper.hexGrid[hx + 1, hy]); };
          if (hy > 0) { if (DynamicMapHelper.hexGrid[hx + 1, hy - 1].TryExpand(hex.burningWeapon)) { DynamicMapHelper.burningHexes.Add(DynamicMapHelper.hexGrid[hx + 1, hy - 1]); }; };
        }
        if (hy < (DynamicMapHelper.hexGrid.GetLength(1) - 1)) {
          if (DynamicMapHelper.hexGrid[hx, hy + 1].TryExpand(hex.burningWeapon)) { DynamicMapHelper.burningHexes.Add(DynamicMapHelper.hexGrid[hx, hy + 1]); }
        }
      }
      HashSet<MapTerrainHexCell> cleanTrees = new HashSet<MapTerrainHexCell>();
      for (int index = 0; index < DynamicMapHelper.burningHexes.Count;) {
        MapTerrainHexCell hex = burningHexes[index];
        if (hex.burnEffectCounter <= 0) { DynamicMapHelper.burningHexes.RemoveAt(index); cleanTrees.Add(hex); } else { ++index; }
      }
      HashSet<object> redrawTreeData = new HashSet<object>();
      foreach(MapTerrainHexCell hcell in cleanTrees) {
        hcell.deleteTrees(redrawTreeData);
      }
      CACDynamicTree.redrawTrees(redrawTreeData);
      DynamicTreesHelper.clearTrees();
    }
    public static void TempTick() {
      CustomAmmoCategoriesLog.Log.LogWrite("TempTick\n");
      HashSet<MapPoint> markDel = new HashSet<MapPoint>();
      foreach (var hc in DynamicMapHelper.tempEffectHexes) {
        if ((hc.x < 0) || (hc.y < 0) || (hc.x >= DynamicMapHelper.hexGrid.GetLength(0)) || (hc.y >= DynamicMapHelper.hexGrid.GetLength(1))) { continue; }
        MapTerrainHexCell hcell = DynamicMapHelper.hexGrid[hc.x, hc.y];
        hcell.tempVFXTick();
        if (hcell.hasTempEffects() == false) { markDel.Add(hcell.mapPoint()); };
      }
      foreach (var dp in markDel) {
        DynamicMapHelper.tempEffectHexes.Remove(dp);
      }
      markDel.Clear();
      if (DynamicMapHelper.mapMetaData == null) { return; };
      foreach (var hc in DynamicMapHelper.tempMaskCells) {
        if ((hc.x < 0) || (hc.y < 0) || (hc.x >= DynamicMapHelper.mapMetaData.mapTerrainDataCells.GetLength(0)) || (hc.y >= DynamicMapHelper.mapMetaData.mapTerrainDataCells.GetLength(1))) { continue; }
        MapTerrainDataCellEx cell = DynamicMapHelper.mapMetaData.mapTerrainDataCells[hc.x, hc.y] as MapTerrainDataCellEx;
        if (cell == null) { continue; }
        cell.tempMaskTick();
        if (cell.tempDesignMask == null) { markDel.Add(cell.mapPoint()); };
      }
      foreach (var dp in markDel) {
        DynamicMapHelper.tempMaskCells.Remove(dp);
      }
    }
    public static void initHexGrid(MapMetaData mapMetaData) {
      DynamicMapHelper.mapMetaData = mapMetaData;
      int hexStepX = (CustomAmmoCategories.Settings.BurningForestCellRadius * 3) / 2 + 1;
      int hexStepY = Mathf.RoundToInt((float)CustomAmmoCategories.Settings.BurningForestCellRadius * 0.866025f);
      List<MapPoint> hexPattern = MapPoint.createHexagon(0, 0, CustomAmmoCategories.Settings.BurningForestCellRadius);
      string biome = "";
      try {
        biome = mapMetaData.biomeDesignMask.Description.Id;
      } catch (Exception) {
        biome = "NotSet";
      }
      bool noForest = CustomAmmoCategories.Settings.NoForestBiomes.Contains(biome);
      CustomAmmoCategoriesLog.Log.LogWrite("Map biome:" + biome + " noForest:" + noForest + "\n");
      int hex_x = mapMetaData.mapTerrainDataCells.GetLength(0) / hexStepX;
      if ((mapMetaData.mapTerrainDataCells.GetLength(0) % hexStepX) != 0) { ++hex_x; }
      int hex_y = mapMetaData.mapTerrainDataCells.GetLength(1) / ((hexStepY * 2) - 1);
      if ((mapMetaData.mapTerrainDataCells.GetLength(1) % ((hexStepY * 2) - 1)) != 0) { ++hex_y; }
      DynamicMapHelper.hexGrid = new MapTerrainHexCell[hex_x, hex_y];
      for (int hix = 0; hix < hex_x; ++hix) {
        for (int hiy = 0; hiy < hex_y; ++hiy) {
          int hx = hix * hexStepX;
          int hy = hiy * hexStepY * 2 + (hx % 2) * hexStepY;
          MapTerrainHexCell hexCell = new MapTerrainHexCell();
          DynamicMapHelper.hexGrid[hix, hiy] = hexCell;
          hexCell.x = hix;
          hexCell.y = hiy;
          hexCell.mapX = hx;
          hexCell.mapY = hy;
          foreach (MapPoint hp in hexPattern) {
            int mx = hp.x + hx;
            int my = hp.y + hy;
            if (mx < 0) { continue; }
            if (my < 0) { continue; }
            if (mx >= mapMetaData.mapTerrainDataCells.GetLength(0)) { continue; }
            if (my >= mapMetaData.mapTerrainDataCells.GetLength(1)) { continue; }
            MapTerrainDataCellEx cell = mapMetaData.mapTerrainDataCells[mx, my] as MapTerrainDataCellEx;
            if (cell == null) { continue; };
            cell.hexCell = hexCell;
          }
        }
      }
      for (int mx = 0; mx < mapMetaData.mapTerrainDataCells.GetLength(0); ++mx) {
        for (int my = 0; my < mapMetaData.mapTerrainDataCells.GetLength(1); ++my) {
          MapTerrainDataCellEx cell = mapMetaData.mapTerrainDataCells[mx, my] as MapTerrainDataCellEx;
          if (cell == null) { continue; };
          if (cell.hexCell == null) {
            CustomAmmoCategoriesLog.Log.LogWrite("MapCell " + mx + ":" + my + " has no hex sell\n", true);
            continue;
          }
          cell.hexCell.terrainCells.Add(cell);
          if (noForest) {
            cell.hexCell.isHasForest = false; cell.hexCell.wasHasForest = false; cell.CantHaveForest = true;
          } else {
            if (SplatMapInfo.IsForest(cell.terrainMask) == true) { cell.hexCell.isHasForest = true; cell.hexCell.wasHasForest = true; cell.CantHaveForest = false; };
          }
        }
      }
      CustomAmmoCategoriesLog.Log.LogWrite("Hex Map:\n");
      for (int hx = 0; hx < DynamicMapHelper.hexGrid.GetLength(0); ++hx) {
        for (int hy = 0; hy < DynamicMapHelper.hexGrid.GetLength(1); ++hy) {
          CustomAmmoCategoriesLog.Log.LogWrite((DynamicMapHelper.hexGrid[hx, hy].isHasForest ? "F" : "-"));
        }
        CustomAmmoCategoriesLog.Log.LogWrite("\n");
      }
    }
    public static ObjectSpawnDataSelf SpawnFXObject(CombatGameState Combat, string prefabName, Vector3 pos, Vector3 scale) {
      //Vector3 groundPos = pos;
      //groundPos.y = Combat.MapMetaData.GetLerpedHeightAt(groundPos);
      ObjectSpawnDataSelf objectSpawnData = new ObjectSpawnDataSelf(prefabName, pos, Quaternion.identity, scale, true, false);
      /*if (objectSpawnData.spawnedObject == null) {
        CustomAmmoCategoriesLog.Log.LogWrite("trying to spawn null. investigating\n");
        VersionManifestEntry versionManifestEntry =  Combat.DataManager.ResourceLocator.EntryByID(prefabName, BattleTechResourceType.Prefab, false);
        if (versionManifestEntry == null) {
          CustomAmmoCategoriesLog.Log.LogWrite("Can't load version manifest for '" + prefabName + "'\n");
          Dictionary<BattleTechResourceType, Dictionary<string, VersionManifestEntry>> baseManifest =
            (Dictionary<BattleTechResourceType, Dictionary<string, VersionManifestEntry>>)typeof(BattleTechResourceLocator).GetField("baseManifest", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(Combat.DataManager.ResourceLocator);
          foreach (var manifests in baseManifest) {
            CustomAmmoCategoriesLog.Log.LogWrite(manifests.Key + ":\n");
            foreach (var manifest in manifests.Value) {
              CustomAmmoCategoriesLog.Log.LogWrite(" " + manifest.Key + "\n");
            }
          }
        } else {
          CustomAmmoCategoriesLog.Log.LogWrite("versionManifestEntry.IsResourcesAsset:"+ versionManifestEntry.IsResourcesAsset + "\n");
          PropertyInfo assetsManagerProp = typeof(DataManager).GetProperty("AssetBundleManager", BindingFlags.NonPublic | BindingFlags.Instance);
          if(assetsManagerProp != null) {
            AssetBundleManager manager = (AssetBundleManager)assetsManagerProp.GetGetMethod().Invoke(Combat.DataManager, null);
            if(manager != null) {
              CustomAmmoCategoriesLog.Log.LogWrite("manager tryied to load "+prefabName+","+ versionManifestEntry.AssetBundleName+"\n");
              //Dictionary<string, object> loadedBundles = (Dictionary<string, object>)typeof(AssetBundleManager).GetField("");
            } else {
              CustomAmmoCategoriesLog.Log.LogWrite("can't get manager\n");
            }
          } else {
            CustomAmmoCategoriesLog.Log.LogWrite("can't get property\n");
          }
          //.GetGetMethod();
        }
      } else {*/
      try {
        objectSpawnData.SpawnSelf(Combat);
        //objectSpawnData.spawnedObject.transform.localScale += scale;
      } catch (Exception e) {
        CustomAmmoCategoriesLog.Log.LogWrite("Spawn exception:" + e.ToString() + "\n");
        CustomAmmoCategoriesLog.Log.LogWrite("investigating\n");
        VersionManifestEntry versionManifestEntry = Combat.DataManager.ResourceLocator.EntryByID(prefabName, BattleTechResourceType.Prefab, false);
        if (versionManifestEntry == null) {
          CustomAmmoCategoriesLog.Log.LogWrite("Can't load version manifest for '" + prefabName + "'\n");
          Dictionary<BattleTechResourceType, Dictionary<string, VersionManifestEntry>> baseManifest =
            (Dictionary<BattleTechResourceType, Dictionary<string, VersionManifestEntry>>)typeof(BattleTechResourceLocator).GetField("baseManifest", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(Combat.DataManager.ResourceLocator);
          foreach (var manifests in baseManifest) {
            CustomAmmoCategoriesLog.Log.LogWrite(manifests.Key + ":\n");
            foreach (var manifest in manifests.Value) {
              CustomAmmoCategoriesLog.Log.LogWrite(" " + manifest.Key + "\n");
            }
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
      CustomAmmoCategoriesLog.Log.LogWrite("Applying burn effect:" + weapon.defId + " " + pos + "\n");
      MapTerrainDataCellEx cell = weapon.parent.Combat.MapMetaData.GetCellAt(pos) as MapTerrainDataCellEx;
      if (cell == null) {
        CustomAmmoCategoriesLog.Log.LogWrite(" cell is not extended\n");
        return;
      }
      CustomAmmoCategoriesLog.Log.LogWrite(" impact at " + pos + "\n");
      if (weapon.FireTerrainCellRadius() == 0) {
        if (cell.hexCell.TryBurnCell(weapon)) { DynamicMapHelper.burningHexes.Add(cell.hexCell); };
      } else {
        List<MapTerrainHexCell> affectedHexCells = MapTerrainHexCell.listHexCellsByCellRadius(cell, weapon.FireTerrainCellRadius());
        foreach (MapTerrainHexCell hexCell in affectedHexCells) {
          if (hexCell.TryBurnCell(weapon)) { DynamicMapHelper.burningHexes.Add(hexCell); };
        }
      }
    }
    public static void applyImpactTempMask(Weapon weapon, Vector3 pos) {
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
      if (mask == null) { return; };
      if (radius == 0) {
        cell.hexCell.addTempTerrainVFX(weapon.parent.Combat, vfx, mask, turns, scale);
      } else {
        List<MapTerrainHexCell> affectedHexCells = MapTerrainHexCell.listHexCellsByCellRadius(cell, radius);
        foreach (MapTerrainHexCell hexCell in affectedHexCells) {
          hexCell.addTempTerrainVFX(weapon.parent.Combat, vfx, mask, turns, scale);
        }
      }
    }
    public static void applyCleanMinefield(Weapon weapon, Vector3 pos) {
      CustomAmmoCategoriesLog.Log.LogWrite("Applying minefield:" + weapon.defId + " " + pos + "\n");
      MapTerrainDataCellEx cell = weapon.parent.Combat.MapMetaData.GetCellAt(pos) as MapTerrainDataCellEx;
      if (cell == null) {
        CustomAmmoCategoriesLog.Log.LogWrite(" cell is not extended\n");
        return;
      }
      CustomAmmoCategoriesLog.Log.LogWrite(" impact at " + pos + "\n");
      MapPoint C = new MapPoint(cell.x, cell.y);
      int affectedRaduis = weapon.ClearMineFieldRadius();
      if (affectedRaduis == 0) {
        CustomAmmoCategoriesLog.Log.LogWrite(" no mine field clean by radius\n");
        return;
      }
      List<MapPoint> affectedPoints = MapPoint.calcMapCircle(C, affectedRaduis);
      if (affectedPoints.Count <= 0) {
        CustomAmmoCategoriesLog.Log.LogWrite(" no affected points? O_o\n");
        return;
      }
      int xlimit = cell.mapMetaData.mapTerrainDataCells.GetLength(0) - 1;
      int ylimit = cell.mapMetaData.mapTerrainDataCells.GetLength(1) - 1;
      CustomAmmoCategoriesLog.Log.LogWrite(" circle center " + cell.x + "," + cell.y + "\n");
      foreach (MapPoint ap in affectedPoints) {
        CustomAmmoCategoriesLog.Log.LogWrite(" affected cell " + ap.x + "," + ap.y + "(" + (ap.x - cell.x) + "," + (ap.y - cell.y) + ")\n");
        if (ap.x < 0) { continue; };
        if (ap.y < 0) { continue; };
        if (ap.x > xlimit) { continue; };
        if (ap.y > ylimit) { continue; };
        MapTerrainDataCellEx acell = cell.mapMetaData.mapTerrainDataCells[ap.x, ap.y] as MapTerrainDataCellEx;
        if (acell == null) { continue; };
        CustomAmmoCategoriesLog.Log.LogWrite("  clear mine field\n");
        acell.MineField.Clear();
      }
    }

    public static void applyMineField(Weapon weapon, Vector3 pos) {
      CustomAmmoCategoriesLog.Log.LogWrite("Applying minefield:" + weapon.defId + " " + pos + "\n");
      MapTerrainDataCellEx cell = weapon.parent.Combat.MapMetaData.GetCellAt(pos) as MapTerrainDataCellEx;
      if (cell == null) {
        CustomAmmoCategoriesLog.Log.LogWrite(" cell is not extended\n");
        return;
      }
      CustomAmmoCategoriesLog.Log.LogWrite(" impact at " + pos + "\n");
      MapPoint C = new MapPoint(cell.x, cell.y);
      int affectedRaduis = weapon.MineFieldRadius();
      int minesCount = weapon.MineFieldCount();
      float heat = weapon.MineFieldHeat();
      float damageChance = weapon.MineFieldHitChance();
      float damage = weapon.MineFieldDamage();
      float instab = weapon.MineFieldInstability();
      if (affectedRaduis == 0) {
        CustomAmmoCategoriesLog.Log.LogWrite(" no mine field by radius\n");
        return;
      }
      if (minesCount == 0) {
        CustomAmmoCategoriesLog.Log.LogWrite(" no mine field by count\n");
        return;
      }
      if (damageChance < CustomAmmoCategories.Epsilon) {
        CustomAmmoCategoriesLog.Log.LogWrite(" no mine field by damage chance\n");
        return;
      }
      if ((damage < CustomAmmoCategories.Epsilon) && (heat < CustomAmmoCategories.Epsilon)) {
        CustomAmmoCategoriesLog.Log.LogWrite(" no mine field by damage\n");
        return;
      }
      List<MapPoint> affectedPoints = MapPoint.calcMapCircle(C, affectedRaduis);
      if (affectedPoints.Count <= 0) {
        CustomAmmoCategoriesLog.Log.LogWrite(" no affected points? O_o\n");
        return;
      }
      int xlimit = cell.mapMetaData.mapTerrainDataCells.GetLength(0) - 1;
      int ylimit = cell.mapMetaData.mapTerrainDataCells.GetLength(1) - 1;
      CustomAmmoCategoriesLog.Log.LogWrite(" circle center " + cell.x + "," + cell.y + "\n");
      foreach (MapPoint ap in affectedPoints) {
        CustomAmmoCategoriesLog.Log.LogWrite(" affected cell " + ap.x + "," + ap.y + "(" + (ap.x - cell.x) + "," + (ap.y - cell.y) + ")\n");
        if (ap.x < 0) { continue; };
        if (ap.y < 0) { continue; };
        if (ap.x > xlimit) { continue; };
        if (ap.y > ylimit) { continue; };
        MapTerrainDataCellEx acell = cell.mapMetaData.mapTerrainDataCells[ap.x, ap.y] as MapTerrainDataCellEx;
        if (acell == null) { continue; };
        CustomAmmoCategoriesLog.Log.LogWrite("  mine field " + damageChance + ":" + damage + ":" + weapon.parent.GUID + "\n");
        acell.MineField.Add(new MineField(damageChance, damage, heat, instab, minesCount, weapon.parent, weapon));
      }
      //bool dngerOnImpact = CustomAmmoCategories.getWeaponBecomesDangerousOnImpact(weapon);
      /*if (cell is MapTerrainDataCellEx) {
        MapTerrainDataCellEx ecell = cell as MapTerrainDataCellEx;
        CustomAmmoCategoriesLog.Log.LogWrite(" cell is extended cell ("+ecell.x+","+ecell.y+")\n");
        int startx = ecell.x - 4;if (startx < 0) { startx = 0; };
        int starty = ecell.y - 4; if (starty < 0) { starty = 0; };
        int endx = ecell.x + 4; if (endx >= ecell.mapMetaData.mapTerrainDataCells.GetLength(0)) { endx = ecell.mapMetaData.mapTerrainDataCells.GetLength(0) - 1; };
        int endy = ecell.y + 4; if (endy >= ecell.mapMetaData.mapTerrainDataCells.GetLength(1)) { endy = ecell.mapMetaData.mapTerrainDataCells.GetLength(1) - 1; };
        FootstepManager.Instance.AddScorch(pos, new Vector3(Random.Range(0.0f, 1f), 0.0f, Random.Range(0.0f, 1f)).normalized, new Vector3(25f, 25f, 25f), true);
        for (int x = startx; x < endx; ++x) {
          for (int y = starty; y < endy; ++y) {
            MapTerrainDataCellEx cecell = ecell.mapMetaData.mapTerrainDataCells[x,y] as MapTerrainDataCellEx;
            if (cecell == null) { continue; };
            CustomAmmoCategoriesLog.Log.LogWrite("  cell is extended cell (" + cecell.x + "," + cecell.y + ")\n");
            TerrainMaskFlags tFlags = MapMetaData.GetPriorityTerrainMaskFlags(cecell);
            DesignMaskDef dmask = CustomAmmoCategories.getWeaponDesignImpactMask(weapon, tFlags);
            if (dmask == null) {
              CustomAmmoCategoriesLog.Log.LogWrite(" no dmask\n");
              continue;
            }
            cecell.AddTerrainMask(TerrainMaskFlags.Custom);
            if (dngerOnImpact) { cecell.AddTerrainMask(TerrainMaskFlags.DangerousLocation); };
            cecell.CustomDesignMask = dmask;
            CustomAmmoCategoriesLog.Log.LogWrite("  updated to " + cecell.CustomDesignMask.Description.Name + "\n");
          }
        }
      }*/
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
      //CustomAmmoCategoriesLog.Log.LogWrite("VFX Loaded:"+key+":"+vfx.GetInstanceID()+"\n");
      //DynamicMapHelper.DataManager.PoolGameObject(key, vfx);
    }
    public static void LoadDesignMasks(DataManager dataManager) {
      DynamicMapHelper.DataManager = dataManager;
      LoadRequest loadRequest = dataManager.CreateLoadRequest((Action<LoadRequest>)null, false);
      foreach (string key in CustomAmmoCategories.Settings.DynamicDesignMasksDefs) {
        loadRequest.AddLoadRequest<DesignMaskDef>(BattleTechResourceType.DesignMaskDef, key, new Action<string, DesignMaskDef>(DynamicMapHelper.TrackLoadedMaskDef), false);
      }
      //DataManager.InjectedDependencyLoadRequest dependencyLoad = new DataManager.InjectedDependencyLoadRequest(dataManager);
      //dependencyLoad.RequestResource(BattleTechResourceType.Prefab, CustomAmmoCategories.Settings.BurningFX);
      //dependencyLoad.RegisterLoadCompleteCallback(new Action(VFXDepsLoaded));
      //dataManager.InjectDependencyLoader(dependencyLoad, 10U);
      //loadRequest.AddLoadRequest<GameObject>(BattleTechResourceType.Prefab, CustomAmmoCategories.Settings.BurningFX, new Action<string, GameObject>(DynamicMapHelper.VFXDepsLoaded), false);
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
    public static void inflictRegistredMovingDamageMech(Mech __instance) {
      CustomAmmoCategoriesLog.Log.LogWrite("inflictRegistredMovingDamageMech to " + __instance.DisplayName + ":" + __instance.GUID + "\n");
      if (MineFieldHelper.registredMovingDamage.ContainsKey(__instance.GUID) == false) {
        CustomAmmoCategoriesLog.Log.LogWrite("not exists moving damage " + __instance.DisplayName + ":" + __instance.GUID + "\n",true);
        return;
      }
      movingDamage mDmg = MineFieldHelper.registredMovingDamage[__instance.GUID];
      MineFieldHelper.registredMovingDamage.Remove(__instance.GUID);
      if (mDmg.mineFieldDamage > CustomAmmoCategories.Epsilon) {
        var fakeHit = new WeaponHitInfo(-1, -1, -1, -1, mDmg.weapon.parent.GUID, __instance.GUID, -1, null, null, null, null, null, null, new AttackImpactQuality[1] { AttackImpactQuality.Solid }, AttackDirection.FromArtillery, Vector2.zero, null);
        __instance.TakeWeaponDamage(fakeHit, (int)ArmorLocation.LeftLeg, mDmg.weapon, mDmg.mineFieldDamage / 2f, 0, DamageType.DFASelf);
        __instance.TakeWeaponDamage(fakeHit, (int)ArmorLocation.RightLeg, mDmg.weapon, mDmg.mineFieldDamage / 2f, 0, DamageType.DFASelf);
        __instance.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(__instance.GUID, __instance.GUID, "MINEFIELD DAMAGE " + mDmg.mineFieldDamage, FloatieMessage.MessageNature.CriticalHit));
      }
      if(mDmg.mineFieldHeat > 0) {
        __instance.AddExternalHeat("MineField", mDmg.mineFieldHeat);
        __instance.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(__instance.GUID, __instance.GUID, "+ " + mDmg.mineFieldHeat + " HEAT FROM MINEFIELD", FloatieMessage.MessageNature.Debuff));
      }
      if(mDmg.burnHeat > 0) {
        __instance.AddExternalHeat("BurningCell", mDmg.burnHeat);
        __instance.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(__instance.GUID, __instance.GUID, "+ " + mDmg.burnHeat + " HEAT FROM FIRE", FloatieMessage.MessageNature.Debuff));
      }
      if((mDmg.burnHeat > 0)||(mDmg.mineFieldHeat > 0)) {
        __instance.GenerateAndPublishHeatSequence(-1, true, false, __instance.GUID);
      }
      if (mDmg.mineFieldInstability > CustomAmmoCategories.Epsilon) {
        __instance.AddAbsoluteInstability(mDmg.mineFieldInstability, StabilityChangeSource.Attack, mDmg.weapon.parent.GUID);
      }
      if (mDmg.weapon != null) {
        bool needDone = false;
        __instance.CheckForInstability();
        if(__instance.IsFlaggedForDeath || __instance.IsFlaggedForKnockdown) {
          needDone = true;
        }
        __instance.HandleKnockdown(-1, mDmg.weapon.parent.GUID, Vector2.one, (SequenceFinished)null);
        __instance.HandleDeath(mDmg.weapon.parent.GUID);
        if (needDone) {
          __instance.HasFiredThisRound = true;
          __instance.HasMovedThisRound = true;
          __instance.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AddSequenceToStackMessage(__instance.DoneWithActor()));
        }
      }
    }
    public static void inflictRegistredMovingDamageVehicle(Vehicle __instance) {
      CustomAmmoCategoriesLog.Log.LogWrite("inflictRegistredMovingDamageMech to " + __instance.DisplayName + ":" + __instance.GUID + "\n");
      if (MineFieldHelper.registredMovingDamage.ContainsKey(__instance.GUID) == false) {
        CustomAmmoCategoriesLog.Log.LogWrite("not exists moving damage " + __instance.DisplayName + ":" + __instance.GUID + "\n", true);
        return;
      }
      movingDamage mDmg = MineFieldHelper.registredMovingDamage[__instance.GUID];
      MineFieldHelper.registredMovingDamage.Remove(__instance.GUID);
      float damage = mDmg.mineFieldDamage + (float)mDmg.mineFieldHeat + (float)mDmg.burnHeat;
      if (damage > CustomAmmoCategories.Epsilon) {
        var fakeHit = new WeaponHitInfo(-1, -1, -1, -1, mDmg.weapon.parent.GUID, __instance.GUID, -1, null, null, null, null, null, null, new AttackImpactQuality[1] { AttackImpactQuality.Solid }, AttackDirection.FromArtillery, Vector2.zero, null);
        __instance.TakeWeaponDamage(fakeHit, (int)VehicleChassisLocations.Front, mDmg.weapon, damage / 4f, 0, DamageType.Combat);
        __instance.TakeWeaponDamage(fakeHit, (int)VehicleChassisLocations.Rear, mDmg.weapon, damage / 4f, 0, DamageType.Combat);
        __instance.TakeWeaponDamage(fakeHit, (int)VehicleChassisLocations.Right, mDmg.weapon, damage / 4f, 0, DamageType.Combat);
        __instance.TakeWeaponDamage(fakeHit, (int)VehicleChassisLocations.Left, mDmg.weapon, damage / 4f, 0, DamageType.Combat);
        string msg = "";
        if ((mDmg.mineFieldDamage > CustomAmmoCategories.Epsilon) || (mDmg.mineFieldHeat > 0)) { msg = "MINE FIELD"; };
        if (mDmg.burnHeat > 0) { if (string.IsNullOrEmpty(msg) == false) { msg += " AND "; }; msg += " BURN"; };
        msg += " DAMAGE " + damage.ToString();
        __instance.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(__instance.GUID, __instance.GUID, msg, FloatieMessage.MessageNature.CriticalHit));
      }
      if (mDmg.weapon != null) {
        bool needDone = false;
        if (__instance.IsFlaggedForDeath) {
          needDone = true;
        }
        __instance.HandleDeath(mDmg.weapon.parent.GUID);
        if (needDone) {
          __instance.HasFiredThisRound = true;
          __instance.HasMovedThisRound = true;
          __instance.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AddSequenceToStackMessage(__instance.DoneWithActor()));
        }
      }
    }
    public static void registerMovingDamageFromPath(AbstractActor __instance, List<WayPoint> waypoints) {
      CustomAmmoCategoriesLog.Log.LogWrite("registerMovingDamageFromPath to " + __instance.DisplayName+":"+__instance.GUID+"\n");
      try {
        List<MapPoint> mapPoints = DynamicMapHelper.getVisitedPoints(__instance.Combat, waypoints);
        float damage = 0f;
        float heat = 0f;
        float instability = 0f;
        AbstractActor actor = null;
        Weapon weapon = null;
        int burnHeat = 0;
        int burnCount = 0;
        bool heatDamage = false;
        float rollMod = 1f;
        PathingCapabilitiesDef PathingCaps = (PathingCapabilitiesDef)typeof(Pathing).GetProperty("PathingCaps", BindingFlags.Instance | BindingFlags.NonPublic).GetGetMethod(true).Invoke(__instance.Pathing, null);
        CustomAmmoCategoriesLog.Log.LogWrite(" current pathing:" + PathingCaps.Description.Id + "\n");
        if (CustomAmmoCategories.Settings.MineFieldPathingMods.ContainsKey(PathingCaps.Description.Id)) {
          rollMod = CustomAmmoCategories.Settings.MineFieldPathingMods[PathingCaps.Description.Id];
        }
        CustomAmmoCategoriesLog.Log.LogWrite(" rol mod:" + rollMod + "\n");
        foreach (MapPoint mapPoint in mapPoints) {
          MapTerrainDataCellEx cell = __instance.Combat.MapMetaData.mapTerrainDataCells[mapPoint.x, mapPoint.y] as MapTerrainDataCellEx;
          if (cell == null) { continue; };
          CustomAmmoCategoriesLog.Log.LogWrite(" cell:" + cell.x + ":" + cell.y + " mf:" + cell.MineField.Count + " burn:" + cell.BurningStrength + "/" + burnHeat + "/" + burnCount + "\n");
          foreach (MineField mineField in cell.MineField) {
            if (mineField.count <= 0) { continue; };
            CustomAmmoCategoriesLog.Log.LogWrite("  mine field:" + (mineField.HitChance * rollMod) + ":" + mineField.damage + "\n");
            float roll = Random.Range(0f, 1f);
            if (roll < (mineField.HitChance * rollMod)) {
              CustomAmmoCategoriesLog.Log.LogWrite("   hit:" + roll + "/" + (mineField.HitChance * rollMod) + ":" + damage + "\n");
              damage += mineField.damage;
              heat += mineField.heat;
              instability += mineField.instability;
              actor = mineField.owner;
              weapon = mineField.weapon;
              mineField.count -= 1;
            }
          }
          if (cell.BurningStrength > 0) {
            burnHeat += cell.BurningStrength; ++burnCount;
            if (weapon == null) { weapon = cell.BurningWeapon; };
          };
        }
        if (damage > CustomAmmoCategories.Epsilon) {
          CustomAmmoCategoriesLog.Log.LogWrite(" taking damage:" + damage + "\n");
          damage = Mathf.Round(damage);
          //var fakeHit = new WeaponHitInfo(-1, -1, -1, -1, actor.GUID, __instance.GUID, -1, null, null, null, null, null, null, new AttackImpactQuality[1] { AttackImpactQuality.Solid }, AttackDirection.FromArtillery, Vector2.zero, null);
          //__instance.TakeWeaponDamage(fakeHit, (int)ArmorLocation.LeftLeg, weapon, damage / 2f, 0, DamageType.DFASelf);
          //__instance.TakeWeaponDamage(fakeHit, (int)ArmorLocation.RightLeg, weapon, damage / 2f, 0, DamageType.DFASelf);
          //__instance.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(__instance.GUID, __instance.GUID, "MINEFIELD DAMAGE " + damage, FloatieMessage.MessageNature.CriticalHit));
        }
        //if (instability > CustomAmmoCategories.Epsilon) {
          //__instance.AddAbsoluteInstability(instability, StabilityChangeSource.Attack, actor.GUID);
        //}
        //__instance.CheckForInstability();
        int iheat = Mathf.RoundToInt(heat);
        if (iheat > 0) {
          heatDamage = true;
          //__instance.AddExternalHeat("MineField", iheat);
          CustomAmmoCategoriesLog.Log.LogWrite(" heat from minefield:" + iheat + "\n");
          //__instance.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(__instance.GUID, __instance.GUID, "+ " + heat + " HEAT FROM MINEFIELD", FloatieMessage.MessageNature.Debuff));
        }
        CustomAmmoCategoriesLog.Log.LogWrite(" burning heat:" + burnHeat + "/" + burnCount + "\n");
        burnHeat = Mathf.RoundToInt((float)burnHeat / (float)burnCount);
        if (burnHeat > 0) {
          CustomAmmoCategoriesLog.Log.LogWrite(" heat from fire:" + burnHeat + "\n");
          //__instance.AddExternalHeat("BurningCell", burnHeat);
          //__instance.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(__instance.GUID, __instance.GUID, "+ " + burnHeat + " HEAT FROM FIRE", FloatieMessage.MessageNature.Debuff));
        }
        movingDamage movingDamage = new movingDamage();
        movingDamage.burnHeat = burnHeat;
        movingDamage.mineFieldDamage = damage;
        movingDamage.mineFieldInstability = instability;
        movingDamage.mineFieldHeat = iheat;
        movingDamage.weapon = weapon;
        if (MineFieldHelper.registredMovingDamage.ContainsKey(__instance.GUID) == false) {
          MineFieldHelper.registredMovingDamage.Add(__instance.GUID, movingDamage);
        } else {
          CustomAmmoCategoriesLog.Log.LogWrite(" Strange behavior. Moving damage already registred for "+__instance.DisplayName+":"+__instance.GUID+"\n", true);
        }
        if (heatDamage) {
          //__instance.GenerateAndPublishHeatSequence(-1, true, false, __instance.GUID);
        }
        //if (actor != null) {
        //__instance.HandleKnockdown(-1, actor.GUID, Vector2.one, (SequenceFinished)null);
        //__instance.HandleDeath(actor.GUID);
        //__instance.HasFiredThisRound = true;
        //__instance.HasMovedThisRound = true;
        //__instance.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AddSequenceToStackMessage(__instance.DoneWithActor()));
        //}
      } catch (Exception e) {
        CustomAmmoCategoriesLog.Log.LogWrite(" Exception:" + e + "\n", true);
      }
    }
    public static void registerJumpingDamageFrom(Mech __instance, Vector3 finalPosition) {
      CustomAmmoCategoriesLog.Log.LogWrite("Mech.OnJumpComplete\n");
      MapTerrainDataCellEx ccell = __instance.Combat.MapMetaData.GetCellAt(finalPosition) as MapTerrainDataCellEx;
      if (ccell == null) { return; };
      List<MapPoint> mapPoints = MapPoint.calcMapCircle(ccell.mapPoint(), CustomAmmoCategories.Settings.JumpLandingMineAttractRadius);
      float damage = 0f;
      float heat = 0f;
      float instability = 0f;
      AbstractActor actor = null;
      Weapon weapon = null;
      int burnHeat = 0;
      int burnCount = 0;
      bool heatDamage = false;
      float rollMod = 1f;
      PathingCapabilitiesDef PathingCaps = (PathingCapabilitiesDef)typeof(Pathing).GetProperty("PathingCaps", BindingFlags.Instance | BindingFlags.NonPublic).GetGetMethod(true).Invoke(__instance.Pathing, null);
      CustomAmmoCategoriesLog.Log.LogWrite(" current pathing:" + PathingCaps.Description.Id + "\n");
      if (CustomAmmoCategories.Settings.MineFieldPathingMods.ContainsKey(PathingCaps.Description.Id)) {
        rollMod = CustomAmmoCategories.Settings.MineFieldPathingMods[PathingCaps.Description.Id];
      }
      CustomAmmoCategoriesLog.Log.LogWrite(" rol mod:" + rollMod + "\n");
      foreach (MapPoint mapPoint in mapPoints) {
        MapTerrainDataCellEx cell = __instance.Combat.MapMetaData.mapTerrainDataCells[mapPoint.x, mapPoint.y] as MapTerrainDataCellEx;
        if (cell == null) { continue; };
        CustomAmmoCategoriesLog.Log.LogWrite(" cell:" + cell.x + ":" + cell.y + " mf:" + cell.MineField + " burn:" + cell.BurningStrength + "/" + burnHeat + "/" + burnCount + "\n");
        foreach (MineField mineField in cell.MineField) {
          if (mineField.count <= 0) { continue; };
          CustomAmmoCategoriesLog.Log.LogWrite("  mine field:" + (mineField.HitChance * rollMod) + ":" + mineField.damage + "\n");
          float roll = Random.Range(0f, 1f);
          if (roll < (mineField.HitChance * rollMod)) {
            CustomAmmoCategoriesLog.Log.LogWrite("   hit:" + roll + "/" + (mineField.HitChance * rollMod) + ":" + damage + "\n");
            damage += mineField.damage;
            heat += mineField.heat;
            instability += mineField.instability;
            actor = mineField.owner;
            weapon = mineField.weapon;
            mineField.count -= 1;
          }
        }
        if (cell.BurningStrength > 0) { burnHeat += cell.BurningStrength; ++burnCount; };
      }
      if (damage > CustomAmmoCategories.Epsilon) {
        CustomAmmoCategoriesLog.Log.LogWrite(" taking damage:" + damage + "\n");
        damage = Mathf.Round(damage);
        //var fakeHit = new WeaponHitInfo(-1, -1, -1, -1, actor.GUID, __instance.GUID, -1, null, null, null, null, null, null, new AttackImpactQuality[1] { AttackImpactQuality.Solid }, AttackDirection.FromArtillery, Vector2.zero, null);
        //__instance.TakeWeaponDamage(fakeHit, (int)ArmorLocation.LeftLeg, weapon, damage / 2f, 0, DamageType.DFASelf);
        //__instance.TakeWeaponDamage(fakeHit, (int)ArmorLocation.RightLeg, weapon, damage / 2f, 0, DamageType.DFASelf);
        //__instance.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(__instance.GUID, __instance.GUID, "MINEFIELD DAMAGE " + damage, FloatieMessage.MessageNature.CriticalHit));
      }
      if (instability > CustomAmmoCategories.Epsilon) {
        //__instance.AddAbsoluteInstability(instability, StabilityChangeSource.Attack, actor.GUID);
      }
      //__instance.CheckForInstability();
      //__instance.HandleKnockdown(-1, actor.GUID, Vector2.one, (SequenceFinished)null);
      //__instance.HandleDeath(actor.GUID);
      int iheat = Mathf.RoundToInt(heat);
      if (iheat > 0) {
        heatDamage = true;
        //__instance.AddExternalHeat("MineField", iheat);
        CustomAmmoCategoriesLog.Log.LogWrite(" heat from minefield:" + iheat + "\n");
        //__instance.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(__instance.GUID, __instance.GUID, "+ " + heat + " HEAT FROM MINEFIELD", FloatieMessage.MessageNature.Debuff));
      }
      CustomAmmoCategoriesLog.Log.LogWrite(" burning heat:" + burnHeat + "/" + burnCount + "\n");
      burnHeat = Mathf.RoundToInt((float)burnHeat / (float)burnCount);
      if (burnHeat > 0) {
        CustomAmmoCategoriesLog.Log.LogWrite(" heat from fire:" + burnHeat + "\n");
        //__instance.AddExternalHeat("BurningCell", burnHeat);
        //__instance.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(__instance.GUID, __instance.GUID, "+ " + burnHeat + " HEAT FROM FIRE", FloatieMessage.MessageNature.Debuff));
      }
      if (heatDamage) {
        //__instance.GenerateAndPublishHeatSequence(-1, true, false, __instance.GUID);
      }
      movingDamage movingDamage = new movingDamage();
      movingDamage.burnHeat = burnHeat;
      movingDamage.mineFieldDamage = damage;
      movingDamage.mineFieldInstability = instability;
      movingDamage.mineFieldHeat = iheat;
      movingDamage.weapon = weapon;
      if (MineFieldHelper.registredMovingDamage.ContainsKey(__instance.GUID) == false) {
        MineFieldHelper.registredMovingDamage.Add(__instance.GUID, movingDamage);
      } else {
        CustomAmmoCategoriesLog.Log.LogWrite(" Strange behavior. Moving damage already registred for " + __instance.DisplayName + ":" + __instance.GUID + "\n", true);
      }
    }
    public static void applyMoveMineFieldDamageToVehicle(Vehicle __instance, List<WayPoint> waypoints) {
      CustomAmmoCategoriesLog.Log.LogWrite("Vehicle.OnMoveOrSprintComplete\n");
      List<MapPoint> mapPoints = DynamicMapHelper.getVisitedPoints(__instance.Combat, waypoints);
      float damage = 0f;
      bool isBurning = false;
      bool isMineField = false;
      int burnHeat = 0;
      int burnCount = 0;
      //float heat = 0f;
      AbstractActor actor = null;
      Weapon weapon = null;
      float rollMod = 1f;
      PathingCapabilitiesDef PathingCaps = (PathingCapabilitiesDef)typeof(Pathing).GetProperty("PathingCaps", BindingFlags.Instance | BindingFlags.NonPublic).GetGetMethod(true).Invoke(__instance.Pathing, null);
      CustomAmmoCategoriesLog.Log.LogWrite(" current pathing:" + PathingCaps.Description.Id + "\n");
      if (CustomAmmoCategories.Settings.MineFieldPathingMods.ContainsKey(PathingCaps.Description.Id)) {
        rollMod = CustomAmmoCategories.Settings.MineFieldPathingMods[PathingCaps.Description.Id];
      }
      CustomAmmoCategoriesLog.Log.LogWrite(" rol mod:" + rollMod + "\n");
      foreach (MapPoint mapPoint in mapPoints) {
        MapTerrainDataCellEx cell = __instance.Combat.MapMetaData.mapTerrainDataCells[mapPoint.x, mapPoint.y] as MapTerrainDataCellEx;
        if (cell == null) { continue; };
        CustomAmmoCategoriesLog.Log.LogWrite(" cell:" + cell.x + ":" + cell.y + "\n");
        foreach (MineField mineField in cell.MineField) {
          if (mineField.count <= 0) { continue; };
          CustomAmmoCategoriesLog.Log.LogWrite("  mine field:" + (mineField.HitChance * rollMod) + ":" + mineField.damage + "\n");
          float roll = Random.Range(0f, 1f);
          if (roll < (mineField.HitChance * rollMod)) {
            CustomAmmoCategoriesLog.Log.LogWrite("   hit:" + roll + "/" + (mineField.HitChance * rollMod) + ":" + damage + "\n");
            damage += mineField.damage;
            damage += mineField.heat;
            actor = mineField.owner;
            weapon = mineField.weapon;
            mineField.count -= 1;
          }
        }
        if (damage > CustomAmmoCategories.Epsilon) { isMineField = true; };
        if (cell.BurningStrength > 0) {
          burnHeat += cell.BurningStrength; ++burnCount;
          if (weapon == null) { weapon = cell.BurningWeapon; actor = cell.BurningWeapon.parent; };
        };
      }
      burnHeat = Mathf.RoundToInt((float)burnHeat / (float)burnCount);
      if (burnHeat > 0) {
        CustomAmmoCategoriesLog.Log.LogWrite(" moving from fire damage " + burnHeat + "\n");
        isBurning = true;
        damage += burnHeat;
      }

      if (damage > CustomAmmoCategories.Epsilon) {
        CustomAmmoCategoriesLog.Log.LogWrite(" taking damage:" + damage + "\n");
        damage = Mathf.Round(damage);
        var fakeHit = new WeaponHitInfo(-1, -1, -1, -1, actor.GUID, __instance.GUID, -1, null, null, null, null, null, null, new AttackImpactQuality[1] { AttackImpactQuality.Solid }, AttackDirection.FromArtillery, Vector2.zero, null);
        __instance.TakeWeaponDamage(fakeHit, (int)VehicleChassisLocations.Front, weapon, damage / 4f, 0, DamageType.Combat);
        __instance.TakeWeaponDamage(fakeHit, (int)VehicleChassisLocations.Rear, weapon, damage / 4f, 0, DamageType.Combat);
        __instance.TakeWeaponDamage(fakeHit, (int)VehicleChassisLocations.Right, weapon, damage / 4f, 0, DamageType.Combat);
        __instance.TakeWeaponDamage(fakeHit, (int)VehicleChassisLocations.Left, weapon, damage / 4f, 0, DamageType.Combat);
        string msg = "";
        if (isMineField) { msg = "MINE FIELD"; };
        if (isBurning) { if (string.IsNullOrEmpty(msg) == false) { msg += " AND "; }; msg += " BURN"; };
        msg += " DAMAGE " + damage.ToString();
        __instance.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(__instance.GUID, __instance.GUID, msg, FloatieMessage.MessageNature.CriticalHit));
        __instance.HandleDeath(actor.GUID);
      }
      //int iheat = Mathf.RoundToInt(heat);
      //if (iheat > 0) {
      //__instance.AddExternalHeat("MineField", iheat);
      //__instance.GenerateAndPublishHeatSequence(-1, false, false, __instance.GUID);
      //__instance.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(__instance.GUID, __instance.GUID, "+ " + heat + " HEAT FROM MINEFIELD", FloatieMessage.MessageNature.Debuff));
      //}
    }
  }
}

namespace CustomAmmoCategoriesPatches {
  [HarmonyPatch(typeof(MechMeleeSequence))]
  [HarmonyPatch("setState")]
  [HarmonyPatch(MethodType.Normal)]
  public static class MechMeleeSequence_setState {
    private static bool Prefix(MechMeleeSequence __instance, ref IConvertible newState) {
      //CustomAmmoCategoriesLog.Log.LogWrite("MechMeleeSequence.setState "+((newState == null)?"null":"not null")+"\n");
      //try { CustomAmmoCategoriesLog.Log.LogWrite(" " + newState.GetType().ToString()); } catch (Exception e) { CustomAmmoCategoriesLog.Log.LogWrite(" " + e.ToString()); }
      return true;
    }
  }
  [HarmonyPatch(typeof(MechMeleeSequence))]
  [HarmonyPatch("BuildMeleeDirectorSequence")]
  [HarmonyPatch(MethodType.Normal)]
  public static class MechMeleeSequence_BuildMeleeDirectorSequence {
    private static bool Prefix(MechMeleeSequence __instance) {
      CustomAmmoCategoriesLog.Log.LogWrite("MechMeleeSequence.BuildMeleeDirectorSequence\n");
      if (MineFieldHelper.hasRegistredMovingDamage(__instance.OwningMech)) {
        CustomAmmoCategoriesLog.Log.LogWrite(" damaged on path. Executing empty melee sequnce\n");
        List<Weapon> requestedWeapons = (List<Weapon>)typeof(MechMeleeSequence).GetField("requestedWeapons", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
        requestedWeapons.Clear();
        ActorMovementSequence moveSequence = (ActorMovementSequence)typeof(MechMeleeSequence).GetField("moveSequence", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
        AttackStackSequence meleeSequence = new AttackStackSequence((AbstractActor)__instance.OwningMech, __instance.MeleeTarget, moveSequence.FinalPos, Quaternion.LookRotation(moveSequence.FinalHeading), new List<Weapon>(), MeleeAttackType.NotSet, 0, -1);
        typeof(MechMeleeSequence).GetField("meleeSequence", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, meleeSequence);
        meleeSequence.willConsumeFiring = false;
        meleeSequence.hasOwningSequence = true;
        meleeSequence.RootSequenceGUID = __instance.RootSequenceGUID;
        if (__instance.cameraSequence == null) {
          return false;
        }
        meleeSequence.SetCamera(__instance.cameraSequence, __instance.MessageIndex);
        return false;
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(MechMeleeSequence))]
  [HarmonyPatch("ExecuteMelee")]
  [HarmonyPatch(MethodType.Normal)]
  public static class MechMeleeSequence_ExecuteMelee {
    private static void Postfix(MechMeleeSequence __instance) {
      CustomAmmoCategoriesLog.Log.LogWrite("MechMeleeSequence.ExecuteMelee.\n");
      if (MineFieldHelper.hasRegistredMovingDamage(__instance.OwningMech)) {
        CustomAmmoCategoriesLog.Log.LogWrite(" damaged on path. executing OnMeleeReady().\n");
        __instance.OnMeleeReady(null);
      }
    }
  }
  [HarmonyPatch(typeof(MechMeleeSequence))]
  [HarmonyPatch("CompleteOrders")]
  [HarmonyPatch(MethodType.Normal)]
  public static class MechMeleeSequence_CompleteOrders {
    private static void Postfix(MechMeleeSequence __instance) {
      CustomAmmoCategoriesLog.Log.LogWrite("MechMeleeSequence.CompleteOrders.\n");
      MineFieldHelper.inflictRegistredMovingDamageMech(__instance.OwningMech);
    }
  }
  [HarmonyPatch(typeof(MechMeleeSequence))]
  [HarmonyPatch("GenerateMeleePath")]
  [HarmonyPatch(MethodType.Normal)]
  public static class MechMeleeSequence_GenerateMeleePath {
    private static void Postfix(MechMeleeSequence __instance) {
      CustomAmmoCategoriesLog.Log.LogWrite("MechMeleeSequence.GenerateMeleePath.\n");
      ActorMovementSequence moveSequence = (ActorMovementSequence)typeof(MechMeleeSequence).GetField("moveSequence", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
      List<WayPoint> Waypoints = (List<WayPoint>)typeof(ActorMovementSequence).GetProperty("Waypoints", BindingFlags.Instance | BindingFlags.NonPublic).GetGetMethod(true).Invoke(moveSequence, new object[0] { });
      MineFieldHelper.registerMovingDamageFromPath(__instance.OwningMech, Waypoints);
      if (__instance.OwningMech.hasRegistredMovingDamage()) {
        CustomAmmoCategoriesLog.Log.LogWrite(" damaged in movement. Melee animation interrupt\n");
        __instance.owningActor.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(__instance.owningActor.GUID, __instance.owningActor.GUID, "MELEE ATTACK INTERUPTED", FloatieMessage.MessageNature.CriticalHit));
        typeof(ActorMovementSequence).GetField("meleeTarget", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(moveSequence, null);
      }
    }
  }
  [HarmonyPatch(typeof(ActorMovementSequence))]
  [HarmonyPatch("CompleteOrders")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class ActorMovementSequence_CompleteOrders {
    private static void Postfix(ActorMovementSequence __instance) {
      CustomAmmoCategoriesLog.Log.LogWrite("ActorMovementSequence.CompleteOrders "+__instance.meleeType+"\n");
      if (__instance.meleeType == MeleeAttackType.NotSet) {
        CustomAmmoCategoriesLog.Log.LogWrite(" not meele\n");
        List<WayPoint> Waypoints = (List<WayPoint>)typeof(ActorMovementSequence).GetProperty("Waypoints", BindingFlags.Instance | BindingFlags.NonPublic).GetGetMethod(true).Invoke(__instance, new object[0] { });
        MineFieldHelper.registerMovingDamageFromPath(__instance.OwningActor, Waypoints);
        if (__instance.OwningMech != null) { MineFieldHelper.inflictRegistredMovingDamageMech(__instance.OwningMech); };
        if (__instance.OwningVehicle != null) { MineFieldHelper.inflictRegistredMovingDamageVehicle(__instance.OwningVehicle); }
      } else {
        CustomAmmoCategoriesLog.Log.LogWrite(" meele\n");
      }
    }
  }
  [HarmonyPatch(typeof(MechJumpSequence))]
  [HarmonyPatch("CompleteOrders")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechJumpSequence_CompleteOrders {
    private static void Postfix(MechJumpSequence __instance) {
      CustomAmmoCategoriesLog.Log.LogWrite("MechJumpSequence.CompleteOrders\n");
      ICombatant DFATarget = (ICombatant)typeof(MechJumpSequence).GetField("DFATarget", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
      MineFieldHelper.registerJumpingDamageFrom(__instance.OwningMech, __instance.FinalPos);
      if (DFATarget == null) {
        CustomAmmoCategoriesLog.Log.LogWrite(" not DFA\n");
        if (__instance.OwningMech != null) { MineFieldHelper.inflictRegistredMovingDamageMech(__instance.OwningMech); };
      } else {
        CustomAmmoCategoriesLog.Log.LogWrite(" DFA\n");
      }
    }
  }
  [HarmonyPatch(typeof(MechDFASequence))]
  [HarmonyPatch("CompleteOrders")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechDFASequence_CompleteOrders {
    private static void Postfix(MechDFASequence __instance) {
      CustomAmmoCategoriesLog.Log.LogWrite("MechDFASequence.CompleteOrders\n");
      MineFieldHelper.inflictRegistredMovingDamageMech(__instance.OwningMech);
    }
  }
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
            __instance.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(__instance.GUID, __instance.GUID, "+ " + cell.BurningStrength + " HEAT FROM STANDING IN FIRE", FloatieMessage.MessageNature.Debuff));
            __instance.CheckForInstability();
            __instance.HandleKnockdown(-1, actor.GUID, Vector2.one, (SequenceFinished)null);
            __instance.HandleDeath(actor.GUID);
          } else
          if (vehicle != null) {
            float damage = (float)cell.BurningStrength;
            Weapon weapon = cell.BurningWeapon;
            var fakeHit = new WeaponHitInfo(-1, -1, -1, -1, actor.GUID, __instance.GUID, -1, null, null, null, null, null, null, new AttackImpactQuality[1] { AttackImpactQuality.Solid }, AttackDirection.FromArtillery, Vector2.zero, null);
            __instance.TakeWeaponDamage(fakeHit, (int)VehicleChassisLocations.Front, weapon, damage / 4f, 0, DamageType.Combat);
            __instance.TakeWeaponDamage(fakeHit, (int)VehicleChassisLocations.Rear, weapon, damage / 4f, 0, DamageType.Combat);
            __instance.TakeWeaponDamage(fakeHit, (int)VehicleChassisLocations.Right, weapon, damage / 4f, 0, DamageType.Combat);
            __instance.TakeWeaponDamage(fakeHit, (int)VehicleChassisLocations.Left, weapon, damage / 4f, 0, DamageType.Combat);
            __instance.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(__instance.GUID, __instance.GUID, "DAMAGE FROM STANDING IN FIRE", FloatieMessage.MessageNature.CriticalHit));
            __instance.HandleDeath(actor.GUID);
          }
        }
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(MoveStatusPreview))]
  [HarmonyPatch("DisplayPreviewStatus")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(Vector3), typeof(MoveType) })]
  public static class MoveStatusPreview_DisplayPreviewStatus {
    public static string getMineFieldStringMoving(AbstractActor actor) {
      StringBuilder result = new StringBuilder();
      List<WayPoint> waypointsFromPath = ActorMovementSequence.ExtractWaypointsFromPath(actor, actor.Pathing.CurrentPath, actor.Pathing.ResultDestination, (ICombatant)actor.Pathing.CurrentMeleeTarget, actor.Pathing.MoveType);
      List<MapPoint> mapPoints = DynamicMapHelper.getVisitedPoints(actor.Combat, waypointsFromPath);
      float damage = 0f;
      float heat = 0f;
      int minefieldCells = 0;
      int minefields = 0;
      float rollMod = 1f;
      PathingCapabilitiesDef PathingCaps = (PathingCapabilitiesDef)typeof(Pathing).GetProperty("PathingCaps", BindingFlags.Instance | BindingFlags.NonPublic).GetGetMethod(true).Invoke(actor.Pathing, null);
      //CustomAmmoCategoriesLog.Log.LogWrite(" current pathing:" + PathingCaps.Description.Id + "\n");
      if (CustomAmmoCategories.Settings.MineFieldPathingMods.ContainsKey(PathingCaps.Description.Id)) {
        rollMod = CustomAmmoCategories.Settings.MineFieldPathingMods[PathingCaps.Description.Id];
      }
      //CustomAmmoCategoriesLog.Log.LogWrite(" rol mod:" + rollMod + "\n");
      foreach (MapPoint mapPoint in mapPoints) {
        MapTerrainDataCellEx cell = actor.Combat.MapMetaData.mapTerrainDataCells[mapPoint.x, mapPoint.y] as MapTerrainDataCellEx;
        if (cell == null) { continue; };
        bool isMinefieldCell = false;
        foreach (MineField mineField in cell.MineField) {
          if (mineField.count <= 0) { continue; };
          damage += mineField.damage * mineField.HitChance * rollMod;
          heat += mineField.heat * mineField.HitChance * rollMod;
          minefields += 1;
          if (isMinefieldCell == false) { isMinefieldCell = true; minefieldCells += 1; };
        }
      }
      if ((damage > CustomAmmoCategories.Epsilon) || (heat > CustomAmmoCategories.Epsilon)) {
        damage = Mathf.Round(damage);
        heat = Mathf.Round(heat);
        result.Append("MINE FIELD ");
        //result.Append("TOTAL CELLS/MINE FIELD CELLS:"+mapPoints.Count+"\n");
        //result.Append("CELLS WITH MINE FIELDS PATH:" + minefieldCells + "\n");
        //result.Append("MINE FIELDS IN PATH:" + minefields + "\n");
        result.Append("POSSIBLE DAMAGE/HEAT:" + damage + "/" + heat);
      }
      return result.ToString();
    }
    public static string getMineFieldStringJumping(AbstractActor actor, MapTerrainDataCellEx ccell) {
      if (ccell == null) { return string.Empty; };
      StringBuilder result = new StringBuilder();
      List<MapPoint> mapPoints = MapPoint.calcMapCircle(ccell.mapPoint(), CustomAmmoCategories.Settings.JumpLandingMineAttractRadius);
      float damage = 0f;
      float heat = 0f;
      int minefieldCells = 0;
      int minefields = 0;
      float rollMod = 1f;
      PathingCapabilitiesDef PathingCaps = (PathingCapabilitiesDef)typeof(Pathing).GetProperty("PathingCaps", BindingFlags.Instance | BindingFlags.NonPublic).GetGetMethod(true).Invoke(actor.Pathing, null);
      //CustomAmmoCategoriesLog.Log.LogWrite(" current pathing:" + PathingCaps.Description.Id + "\n");
      if (CustomAmmoCategories.Settings.MineFieldPathingMods.ContainsKey(PathingCaps.Description.Id)) {
        rollMod = CustomAmmoCategories.Settings.MineFieldPathingMods[PathingCaps.Description.Id];
      }
      //CustomAmmoCategoriesLog.Log.LogWrite(" rol mod:" + rollMod + "\n");
      foreach (MapPoint mapPoint in mapPoints) {
        MapTerrainDataCellEx cell = actor.Combat.MapMetaData.mapTerrainDataCells[mapPoint.x, mapPoint.y] as MapTerrainDataCellEx;
        if (cell == null) { continue; };
        bool isMinefieldCell = false;
        foreach (MineField mineField in cell.MineField) {
          if (mineField.count <= 0) { continue; };
          damage += mineField.damage * mineField.HitChance * rollMod;
          heat += mineField.heat * mineField.HitChance * rollMod;
          minefields += 1;
          if (isMinefieldCell == false) { isMinefieldCell = true; minefieldCells += 1; };
        }
      }
      if ((damage > CustomAmmoCategories.Epsilon) || (heat > CustomAmmoCategories.Epsilon)) {
        damage = Mathf.Round(damage);
        heat = Mathf.Round(heat);
        result.Append("JUMP TO MINE FIELD ");
        //result.Append("TOTAL CELLS/MINE FIELD CELLS:"+mapPoints.Count+"\n");
        //result.Append("CELLS WITH MINE FIELDS PATH:" + minefieldCells + "\n");
        //result.Append("MINE FIELDS IN PATH:" + minefields + "\n");
        result.Append("POSSIBLE DAMAGE/HEAT:" + damage + "/" + heat);
      }
      return result.ToString();
    }
    private static void Postfix(MoveStatusPreview __instance, AbstractActor actor, Vector3 worldPos, MoveType moveType) {
      List<MapEncounterLayerDataCell> cells = new List<MapEncounterLayerDataCell>();
      CombatHUD HUD = (CombatHUD)typeof(MoveStatusPreview).GetProperty("HUD", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance, null);
      CombatHUDInfoSidePanel sidePanel = (CombatHUDInfoSidePanel)typeof(MoveStatusPreview).GetProperty("sidePanel", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance, null);
      cells.Add(HUD.Combat.EncounterLayerData.GetCellAt(worldPos));
      __instance.PreviewStatusPanel.ShowPreviewStatuses(actor, cells, moveType);
      DesignMaskDef priorityDesignMask = actor.Combat.MapMetaData.GetPriorityDesignMask(cells[0].relatedTerrainCell);
      MapTerrainDataCellEx cell = cells[0].relatedTerrainCell as MapTerrainDataCellEx;
      bool flag1 = SplatMapInfo.IsDropshipLandingZone(cells[0].relatedTerrainCell.terrainMask);
      bool flag2 = SplatMapInfo.IsDangerousLocation(cells[0].relatedTerrainCell.terrainMask);
      string minefieldText = string.Empty;
      if (moveType == MoveType.Jumping) {
        minefieldText = getMineFieldStringJumping(actor, cell);
      } else {
        minefieldText = getMineFieldStringMoving(actor);
      }
      if (cell != null) {
        if (cell.BurningCounter > 0) {
          if (cell.BurningCounter > 0) {
            if (string.IsNullOrEmpty(minefieldText) == false) { minefieldText += "\n"; };
            minefieldText += "BURN STRENGTH/DURATION " + cell.BurningStrength + "/" + cell.BurningCounter;
          }
        }
      }
      if (priorityDesignMask != null || flag1 || flag2) {
        Text description = new Text();
        Text title = new Text();
        if (flag2) {
          title = new Text(HUD.Combat.Constants.CombatUIConstants.DangerousLocationDesc.Name, new object[0]);
          description = new Text(HUD.Combat.Constants.CombatUIConstants.DangerousLocationDesc.Details, new object[0]);
          if (flag1)
            description = new Text("{0}\n{1}", new object[2]
            {
              (object) description,
              (object) HUD.Combat.Constants.CombatUIConstants.DrophipLocationDesc.Details
            });
        } else if (flag1) {
          title = new Text(HUD.Combat.Constants.CombatUIConstants.DrophipLocationDesc.Name, new object[0]);
          description = new Text(HUD.Combat.Constants.CombatUIConstants.DrophipLocationDesc.Details, new object[0]);
          if (flag2)
            description = new Text("{0}\n{1}", new object[2]
            {
              (object) description,
              (object) HUD.Combat.Constants.CombatUIConstants.DangerousLocationDesc.Details
            });
        } else if (priorityDesignMask != null) {
          title = new Text(priorityDesignMask.Description.Name, new object[0]);
          description = new Text((string)typeof(MoveStatusPreview).GetMethod("GetDesignMaskDetails", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance, new object[1] { (object)priorityDesignMask }), new object[0]);
        }
        Text warningText = new Text(minefieldText);
        sidePanel.ForceShowSingleFrame(title, description, warningText, flag1 || flag2);
      } else
      if (string.IsNullOrEmpty(minefieldText) == false) {
        sidePanel.ForceShowSingleFrame(new Text("MINE FIELD"), new Text(minefieldText), (Text)null, true);
      }
      switch (moveType) {
        case MoveType.Walking:
          __instance.MoveTypeText.SetText(HUD.MoveButton.Tooltip.text, new object[0]);
          break;
        case MoveType.Sprinting:
          __instance.MoveTypeText.SetText(HUD.SprintButton.Tooltip.text, new object[0]);
          break;
        case MoveType.Backward:
          break;
        case MoveType.Jumping:
          __instance.MoveTypeText.SetText(HUD.JumpButton.Tooltip.text, new object[0]);
          break;
        case MoveType.Melee:
          __instance.MoveTypeText.SetText(HUD.MoveButton.Tooltip.text, new object[0]);
          break;
        default:
          __instance.MoveTypeText.SetText(string.Empty, new object[0]);
          break;
      }
    }
  }
  [HarmonyPatch(typeof(MapMetaData))]
  [HarmonyPatch("GetPriorityDesignMask")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MapTerrainDataCell) })]
  public static class MapMetaData_GetPriorityDesignMask {
    private static void Postfix(MapMetaData __instance, MapTerrainDataCell cell, ref DesignMaskDef __result) {
      MapTerrainDataCellEx excell = cell as MapTerrainDataCellEx;
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
    }
  }
  [HarmonyPatch(typeof(MapTerrainDataCell))]
  [HarmonyPatch("GetAudioSurfaceType")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MapTerrainDataCell_GetAudioSurfaceType {
    private static void Postfix(MapTerrainDataCell __instance, ref AudioSwitch_surface_type __result) {
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
  /*[HarmonyPatch(typeof(FootstepManager))]
  [HarmonyPatch("AddScorch")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Vector3), typeof(Vector3), typeof(Vector3), typeof(bool) })]
  public static class FootstepManager_AddScorch {
    public static List<bool> strongScorches = new List<bool>();
    static void Postfix(FootstepManager __instance, Vector3 position, Vector3 forward, Vector3 scale, bool persistent) {
      FootstepManager_AddScorch.strongScorches.Add(persistent);
    }
  }
  [HarmonyPatch(typeof(FootstepManager))]
  [HarmonyPatch("ProcessScorches")]
  [HarmonyPatch(MethodType.Normal)]
  public static class FootstepManager_ProcessScorches {
    static void Postfix(FootstepManager __instance) {
      float[] _scorchAlphas = (float[])typeof(FootstepManager).GetField("_scorchAlphas", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
      for(int t = 0; t < _scorchAlphas.Length; ++t) {
        if (t >= FootstepManager_AddScorch.strongScorches.Count) { break; }
        if(FootstepManager_AddScorch.strongScorches[t] == true) {
          _scorchAlphas[t] = 0f;
        }
      }
      Shader.SetGlobalFloatArray("_BT_ScorchAlpha", _scorchAlphas);
    }
  }*/
  [HarmonyPatch(typeof(MapMetaData))]
  [HarmonyPatch("Load")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(SerializationStream) })]
  public static class MapMetaData_Load {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetConstructor = AccessTools.Constructor(typeof(MapTerrainDataCell));
      var replacConstructor = AccessTools.Constructor(typeof(MapTerrainDataCellEx));
      return Transpilers.MethodReplacer(instructions, targetConstructor, replacConstructor);
    }
    static void Postfix(MapMetaData __instance, SerializationStream stream) {
      int xmax = __instance.mapTerrainDataCells.GetLength(0);
      int ymax = __instance.mapTerrainDataCells.GetLength(1);
      CustomAmmoCategoriesLog.Log.LogWrite("MapMetaData.Load " + xmax + " X " + ymax + " \n");
      for (int x = 0; x < xmax; ++x) {
        for (int y = 0; y < ymax; ++y) {
          if (__instance.mapTerrainDataCells[x, y] is MapTerrainDataCellEx) {
            //CustomAmmoCategoriesLog.Log.LogWrite(" " + x + " X " + y + " is ext cell\n");
            (__instance.mapTerrainDataCells[x, y] as MapTerrainDataCellEx).x = x;
            (__instance.mapTerrainDataCells[x, y] as MapTerrainDataCellEx).y = y;
          }
        }
      }
      if (Terrain.activeTerrain == null) {
        CustomAmmoCategoriesLog.Log.LogWrite(" active terrain is null \n");
      } else {
        /*CustomAmmoCategoriesLog.Log.LogWrite(" trees:" + Terrain.activeTerrain.terrainData.treeInstanceCount + ":"+Terrain.activeTerrain.name+"\n");
        //Terrain.activeTerrain.terrainData.treeInstances = new TreeInstance[0] { };
        int treesCount = Terrain.activeTerrain.terrainData.treeInstanceCount;
        TreeInstance[] treeInstances = Terrain.activeTerrain.terrainData.treeInstances;
        TerrainData data = Terrain.activeTerrain.terrainData;
        float width = data.size.x / 2;
        float height = data.size.z / 2;
        float y = data.size.y;
        //int errorTrees = 0;
        for (int t = 0; t < treesCount; ++t) {
          Vector3 worldTreePos = Vector3.Scale(treeInstances[t].position, data.size) + Terrain.activeTerrain.transform.position;
          MapTerrainDataCellEx cell =  __instance.GetCellAt(worldTreePos) as MapTerrainDataCellEx;
          if (cell == null) { continue; };
          //if (SplatMapInfo.IsForest(cell.terrainMask) == false) {
            //CustomAmmoCategoriesLog.Log.LogWrite("  tree:"+ worldTreePos + ":"+ treeInstances[t].prototypeIndex+ ":"+ treeInstances[t].widthScale+ ":"+ treeInstances[t].heightScale+ " at cell "+cell.x+":"+cell.y+" which have no forest\n");
            //++errorTrees;
            //treeInstances[t].widthScale = 0f;
            //treeInstances[t].heightScale = 0f;
            //data.SetTreeInstance(t, treeInstances[t]);
            //continue;
          //}
          cell.trees.Add(t);
        }
        CustomAmmoCategoriesLog.Log.LogWrite(" trees inited.\n");*/
        DynamicMapHelper.initHexGrid(__instance);
      }
    }
  }
  [HarmonyPatch(typeof(TurnDirector))]
  [HarmonyPatch("EndCurrentRound")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class TurnDirector_EndCurrentRound {
    public static void Postfix(TurnDirector __instance) {
      DynamicMapHelper.FireTick();
      DynamicMapHelper.TempTick();
    }
  }
  /*[HarmonyPatch(typeof(QuadTree))]
  [HarmonyPatch("Insert")]
  [HarmonyPatch(MethodType.Normal)]
  public static class QuadTree_Insert {
    public static int counter = 0;
    public static void Postfix(QuadTree __instance) {
      CustomAmmoCategoriesLog.Log.LogWrite("QuadTree.Insert:"+counter+"\n");
      ++counter;
    }
  }*/
  [HarmonyPatch(typeof(TreeContainer))]
  [HarmonyPatch("GatherTrees")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class TreeContainer_GatherTrees {
    public static void Postfix(TreeContainer __instance) {
      CustomAmmoCategoriesLog.Log.LogWrite("TreeContainer_GatherTrees.Postfix\n");
    }
  }
  /*[HarmonyPatch(typeof(FootstepManager))]
  [HarmonyPatch("scorchMaterial")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class FootstepManager_scorchMaterial {
    public static Material ScorchMaterial = null;
    public static bool Prefix(FootstepManager __instance, ref Material __result) {
      Material _scorchMaterial = (Material)typeof(FootstepManager).GetField("_scorchMaterial", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
      if (_scorchMaterial != null) { __result = _scorchMaterial; return false; };
      CustomAmmoCategoriesLog.Log.LogWrite("FootstepManager.scorchMaterial init\n");
      if (FootstepManager_scorchMaterial.ScorchMaterial == null) {
        _scorchMaterial = Resources.Load<Material>("Decals/ScorchMaterial");
      } else {
        _scorchMaterial = FootstepManager_scorchMaterial.ScorchMaterial;
      }
      CustomAmmoCategoriesLog.Log.LogWrite(" scorch material:"+_scorchMaterial.name+"\n");
      _scorchMaterial.enableInstancing = true;
      //_scorchMaterial.EnableKeyword("_DYNAMIC");
      __result = _scorchMaterial;
      typeof(FootstepManager).GetField("_scorchMaterial", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, _scorchMaterial);
      return false;
    }
  }*/
  [HarmonyPatch(typeof(BTCustomRenderer))]
  [HarmonyPatch("DrawDecals")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Camera) })]
  public static class BTCustomRenderer_DrawDecals {
    public static Material ScorchMaterial = null;
    public static FieldInfo deferredDecalsBufferField = null;
    public static FieldInfo skipDecalsField = null;
    public static FieldInfo effectsQualityField = null;
    public static MethodInfo UseCameraMethod = null;
    public static readonly int maxArraySize = 1000;
    public static List<List<Matrix4x4>> Scorches = new List<List<Matrix4x4>>();
    public static void Clear() {
      BTCustomRenderer_DrawDecals.Scorches.Clear();
    }
    public static void AddScorch(Vector3 position, Vector3 forward, Vector3 scale) {
      if (CustomAmmoCategories.Settings.DontShowScorchTerrain == true) { return; }
      if (BTCustomRenderer_DrawDecals.Scorches.Count == 0) {
        BTCustomRenderer_DrawDecals.Scorches.Add(new List<Matrix4x4>());
      }else
      if (BTCustomRenderer_DrawDecals.Scorches[BTCustomRenderer_DrawDecals.Scorches.Count - 1].Count > BTCustomRenderer_DrawDecals.maxArraySize) {
        BTCustomRenderer_DrawDecals.Scorches.Add(new List<Matrix4x4>());
      }
      Quaternion rotation = Quaternion.LookRotation(forward);
      rotation = Quaternion.Euler(0.0f, rotation.eulerAngles.y, 0.0f);
      Matrix4x4 trs = Matrix4x4.TRS(position,rotation,scale);
      BTCustomRenderer_DrawDecals.Scorches[BTCustomRenderer_DrawDecals.Scorches.Count - 1].Add(trs);
    }
    public static bool Prepare() {
      CustomAmmoCategoriesLog.Log.LogWrite("BTCustomRenderer_DrawDecals prepare\n"); ;
      BTCustomRenderer_DrawDecals.ScorchMaterial = Resources.Load<Material>("Decals/ScorchMaterial");
      if(BTCustomRenderer_DrawDecals.ScorchMaterial == null) {
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
      BTCustomRenderer_DrawDecals.ScorchMaterial.SetFloat("_AffectTree",0f);
      BTCustomRenderer_DrawDecals.ScorchMaterial.SetTexture("_MainTex", terrainTexture);
      BTCustomRenderer_DrawDecals.ScorchMaterial.enableInstancing = true;
      BTCustomRenderer_DrawDecals.UseCameraMethod = typeof(BTCustomRenderer).GetMethod("UseCamera", BindingFlags.Instance | BindingFlags.NonPublic);
      if(BTCustomRenderer_DrawDecals.UseCameraMethod == null) {
        CustomAmmoCategoriesLog.Log.LogWrite("Fail to get UseCamera method\n"); ;
        return false;
      }
      CustomAmmoCategoriesLog.Log.LogWrite("Success get UseCamera method\n");
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
      object customCommandBuffers = BTCustomRenderer_DrawDecals.UseCameraMethod.Invoke(__instance,new object[1] { (object)camera });
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
        for(int index1=0;index1 < BTCustomRenderer_DrawDecals.Scorches.Count; ++index1) {
          Matrix4x4[] matrices2 = BTCustomRenderer_DrawDecals.Scorches[index1].ToArray();
          int scorches = matrices2.Length;
          deferredDecalsBuffer.DrawMeshInstanced(BTDecal.DecalMesh.DecalMeshFull, 0, BTCustomRenderer_DrawDecals.ScorchMaterial, 0, matrices2, scorches, (MaterialPropertyBlock)null);
        }
      }
    }
  }
  [HarmonyPatch(typeof(FootstepManager))]
  [HarmonyPatch("Instance")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class FootstepManager_Instance {
    public static bool Prefix(ref FootstepManager __result) {
      typeof(FootstepManager).GetField("maxDecals", BindingFlags.Static | BindingFlags.Public).SetValue(null, 1023);
      return true;
    }
  }
  [HarmonyPatch(typeof(CombatGameState))]
  [HarmonyPatch("OnCombatGameDestroyed")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatGameState_OnCombatGameDestroyedMap {
    public static bool Prefix(CombatGameState __instance) {
      DynamicMapHelper.ClearTerrain();
      MineFieldHelper.registredMovingDamage.Clear();
      BTCustomRenderer_DrawDecals.Clear();
      DynamicTreesHelper.Clean();
      CACDynamicTree.allCACTrees.Clear();
      return true;
    }
  }
  [HarmonyPatch(typeof(MapMetaDataExporter))]
  [HarmonyPatch("GenerateTerrainData")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Terrain), typeof(bool) })]
  public static class MapMetaDataExporter_GenerateTerrainData {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetConstructor = AccessTools.Constructor(typeof(MapTerrainDataCell));
      var replacConstructor = AccessTools.Constructor(typeof(MapTerrainDataCellEx));
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