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
using BattleTech.UI;
using CustAmmoCategories;
using CustomAmmoCategoriesLog;
using CustomAmmoCategoriesPatches;
using Harmony;
using IRBTModUtils;
using Localize;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CustAmmoCategories {
  public static class DeferredEffectHelper {
    private static Dictionary<string, Action<Weapon, Vector3>> defferedEffectsCallbacks = new Dictionary<string, Action<Weapon, Vector3>>();
    private static HashSet<DeferredEffect> deferredEffects = new HashSet<DeferredEffect>();
    private static HashSet<DeferredEffect> playingEffects = new HashSet<DeferredEffect>();
    private static HashSet<DeferredEffect> clearEffects = new HashSet<DeferredEffect>();
    private static int CurrentRound = -1;
    public static void RegisterCallback(string id, Action<Weapon, Vector3> callback) {
      if (defferedEffectsCallbacks.ContainsKey(id)) {
        defferedEffectsCallbacks[id] = callback;
      } else {
        defferedEffectsCallbacks.Add(id, callback);
      }
    }
    public static void CallDefferedMethod(this Weapon weapon, string id, Vector3 pos) {
      if (defferedEffectsCallbacks.TryGetValue(id, out Action<Weapon, Vector3> callback)) {
        callback(weapon, pos);
      }
    }
    public static DeferredEffectDef DeferredEffect(this Weapon weapon) {
      WeaponMode mode = weapon.mode();
      if (mode.deferredEffect.rounds > 0) { return mode.deferredEffect; }
      ExtAmmunitionDef ammo = weapon.ammo();
      if (ammo.deferredEffect.rounds > 0) { return ammo.deferredEffect; }
      return weapon.exDef().deferredEffect;
    }
    public static void addToClear(DeferredEffect effect) {
      clearEffects.Add(effect);
    }
    public static void ClearEffectsClear() {
      if (clearEffects.Count == 0) { return; }
      HashSet<DeferredEffect> effects = clearEffects.ToHashSet();
      foreach (DeferredEffect effect in effects) {
        playingEffects.Remove(effect);
        clearEffects.Remove(effect);
        effect.Clear();
        effect.gameObject.SetActive(false);
        GameObject.Destroy(effect);
      }
    }
    public static void UpdateDefferedEffects(int round) {
      if (CurrentRound == round) { return; }
      CurrentRound = round;
      Log.M.WL(1, "DeferredEffectHelper.UpdateDefferedEffects "+CurrentRound);
      HashSet<DeferredEffect> transferEffects = new HashSet<DeferredEffect>();
      foreach (DeferredEffect effect in deferredEffects) {
        Log.M.WL(1, effect.definition.id+" remain:"+ effect.RoundsRemain(round));
        if (effect.RoundsRemain(round) <= 0) { transferEffects.Add(effect); effect.PlayEffect(); effect.gameObject.SetActive(true); } else {
          effect.UpdateText(CurrentRound);
        }
      }
      foreach (DeferredEffect effect in transferEffects) {
        deferredEffects.Remove(effect);
        playingEffects.Add(effect);
      }
    }
    public static bool HasUnApplyedEffects() {
      //TODO подумать насчет конкурентного доступа
      foreach (DeferredEffect effect in playingEffects) {
        if (effect.DamageApplyied == false) { return true; }
      }
      return false;
    }
    public static void Clear() {
      Log.M.TWL(0, "DeferredEffectHelper.Clear");
      try {
        Log.M.WL(1, "playingEffects:" + playingEffects.Count);
        foreach (DeferredEffect effect in playingEffects) {
          if (effect == null) { continue; }
          try {
            if (effect.gameObject == null) effect.gameObject.SetActive(false);
            effect.Clear();
          } catch (Exception e) { Log.M?.TWL(0, e.ToString(), true); }
          if (effect.gameObject != null) {
            GameObject.Destroy(effect.gameObject);
          }
        }
        playingEffects.Clear();
        Log.M.WL(1, "deferredEffects:" + deferredEffects.Count);
        foreach (DeferredEffect effect in deferredEffects) {
          if (effect == null) { continue; }
          if (effect.gameObject != null) { effect.gameObject.SetActive(false); };
          try { effect.Clear(); } catch (Exception e) { Log.M.TWL(0, e.ToString(), true); };
        }
        deferredEffects.Clear();
        CurrentRound = -1;
      }catch(Exception e) {
        Log.M?.TWL(0,e.ToString(),true);
      }
    }
    public static void CreateDifferedEffect(this Weapon weapon, ICombatant target) {
      Log.M.TWL(0, "CreateDifferedEffect " + weapon.defId + " target:" + new Text(target.DisplayName).ToString());
      DeferredEffectDef def = weapon.DeferredEffect();
      Log.M.WL(1, "rounds:"+def.rounds + " current round:" + weapon.parent.Combat.TurnDirector.CurrentRound);
      if (def.rounds == 0) { return; };
      GameObject obj = new GameObject();
      obj.SetActive(false);
      DeferredEffect effect = obj.AddComponent<DeferredEffect>();
      effect.Init(weapon, def, weapon.parent.Combat.TurnDirector.CurrentRound, target);
      deferredEffects.Add(effect);
    }
    public static void CreateDifferedEffect(this Weapon weapon, Vector3 worldPos) {
      Log.M.TWL(0, "CreateDifferedEffect " + weapon.defId + " pos:" + worldPos);
      DeferredEffectDef def = weapon.DeferredEffect();
      Log.M.WL(1, "rounds:" + def.rounds+" current round:"+ weapon.parent.Combat.TurnDirector.CurrentRound);
      if (def.rounds == 0) { return; };
      GameObject obj = new GameObject();
      obj.SetActive(false);
      DeferredEffect effect = obj.AddComponent<DeferredEffect>();
      effect.Init(weapon, def, weapon.parent.Combat.TurnDirector.CurrentRound, worldPos);
      deferredEffects.Add(effect);
    }
  }
  public class DeferredEffectAncor: MonoBehaviour {
    public Transform ancor { get; set; }
    public Vector3 offset { get; set; }
    public void Awake() {
      ancor = null;
      offset = Vector3.zero;
    }
    public void Update() {
      if (ancor != null) { this.transform.position = ancor.position + offset; }
    }
  }
  public class DeferredEffect: MonoBehaviour {
    private int SequenceId;
    public DeferredEffectDef definition { get; private set; }
    private int RoundStarted;
    public int RoundsRemain(int currentRound) { if (definition == null) { return 0; } else { return definition.rounds - (currentRound - this.RoundStarted); }; }
    public bool Playing { get; private set; }
    public bool DamageApplyied { get; private set; }
    private float t { get; set; }
    public Weapon weapon { get; set; }
    public ObjectSpawnDataSelf vfx { get; set; }
    public ObjectSpawnDataSelf wVfx { get; set; }
    public PersistentFloatieMessage CountDownFloatie { get; set; }
    public Transform ancor { get; private set; }
    public Vector3 offset;
    public CombatCustomReticle reticle { get; private set; }
    public DesignMaskDef getTerrainDesignMask() {
      string maskName = definition.tempDesignMask;
      if (string.IsNullOrEmpty(maskName)) { return null; };
      if (DynamicMapHelper.loadedMasksDef.ContainsKey(maskName) == false) { return null; }
      return DynamicMapHelper.loadedMasksDef[maskName];
    }
    public void applyTempMask() {
      Vector3 worldPos = offset;
      if (ancor != null) { worldPos += ancor.position; };
      Log.M.TWL(0,"DeferredEffect.applyTempMask:" + definition.id + " pos:"+worldPos+"\n");
      MapTerrainDataCellEx cell = weapon.parent.Combat.MapMetaData.GetCellAt(worldPos) as MapTerrainDataCellEx;
      if (cell == null) {
        Log.LogWrite(" cell is not extended\n");
        return;
      }
      Log.LogWrite(" impact at " + worldPos + "\n");
      int turns = definition.tempDesignMaskTurns;
      string vfx = definition.TerrainVFX;
      Vector3 scale = definition.VFXscale.vector;
      int radius = definition.tempDesignMaskCellRadius;
      DesignMaskDef mask = this.getTerrainDesignMask();
      if (radius == 0) {
        if(string.IsNullOrEmpty(vfx) == false)cell.hexCell.addTempTerrainVFX(weapon.parent.Combat, vfx, turns, scale);
        if (mask != null) cell.hexCell.addDesignMaskAsync(mask, turns);
      } else {
        List<MapTerrainHexCell> affectedHexCells = MapTerrainHexCell.listHexCellsByCellRadius(cell, radius);
        foreach (MapTerrainHexCell hexCell in affectedHexCells) {
          if (string.IsNullOrEmpty(vfx) == false) hexCell.addTempTerrainVFX(weapon.parent.Combat, vfx, turns, scale);
          if (mask != null) hexCell.addDesignMaskAsync(mask, turns);
        }
      }
    }
    public void ApplyBurn() {
      Log.M.TWL(0, "DeferredEffect.ApplyBurnEffect:" + definition.id + "\n");
      Vector3 worldPos = offset;
      if (ancor != null) { worldPos += ancor.position; };
      MapTerrainDataCellEx cell = weapon.parent.Combat.MapMetaData.GetCellAt(worldPos) as MapTerrainDataCellEx;
      if (cell == null) {
        CustomAmmoCategoriesLog.Log.LogWrite(" cell is not extended\n");
        return;
      }
      Log.LogWrite(" fire at " + worldPos + "\n");
      if (definition.FireTerrainCellRadius == 0) {
        cell.hexCell.TryBurnCellAsync(weapon, definition.FireTerrainChance, definition.FireTerrainStrength, definition.FireDurationWithoutForest);
        //if (cell.hexCell.TryBurnCell(weapon, definition.FireTerrainChance, definition.FireTerrainStrength, definition.FireDurationWithoutForest)) {
          //DynamicMapHelper.burningHexes.Add(cell.hexCell);
        //};
      } else {
        List<MapTerrainHexCell> affectedHexCells = MapTerrainHexCell.listHexCellsByCellRadius(cell, definition.FireTerrainCellRadius);
        foreach (MapTerrainHexCell hexCell in affectedHexCells) {
          hexCell.TryBurnCellAsync(weapon, definition.FireTerrainChance, definition.FireTerrainStrength, definition.FireDurationWithoutForest);
          //if (hexCell.TryBurnCell(weapon, definition.FireTerrainChance, definition.FireTerrainStrength, definition.FireDurationWithoutForest)) {
            //DynamicMapHelper.burningHexes.Add(hexCell);
          //};
        }
      }

    }
    //public Spaw
    public void Awake() { t = 0f; DamageApplyied = false; }
    public void Clear() {
      Log.M.WL(2, "DeferredEffect.Clear "+this.definition.id);
      if (vfx != null) {
        Log.M.WL(3, "vfx");
        vfx.CleanupSelf(); vfx = null;
      }
      if (wVfx != null) {
        Log.M.WL(3, "wVfx");
        wVfx.CleanupSelf(); wVfx = null;
      }
      if (CountDownFloatie != null) {
        Log.M.WL(3, "CountDownFloatie");
        PersistentFloatieHelper.PoolFloatie(CountDownFloatie); CountDownFloatie = null;
      };
      if (this.reticle != null) {
        Log.M.WL(3, "reticle");
        weapon.parent.Combat.DataManager.PoolGameObject(CombatCustomReticle.CustomPrefabName, this.reticle.gameObject); this.reticle = null;
      }
    }
    public void PlayEffect() {
      if (definition == null) { return; }
      t = 0f;
      Playing = true;
      Vector3 pos = offset;
      if (ancor != null) { pos += ancor.position; };
      Log.M.TWL(0, "DeferredEffect.PlayEffect vfx:"+definition.VFX+" sfx:"+ definition.SFX);
      applyCallbacks();
      if (string.IsNullOrEmpty(definition.VFX) == false) {
        vfx = new ObjectSpawnDataSelf(definition.VFX, pos, Quaternion.identity, definition.VFXscale.vector, true, false);
        vfx.SpawnSelf(weapon.parent.Combat);
      } else {
        vfx = null;
      }
      if (string.IsNullOrEmpty(definition.SFX) == false) {
        CustomSoundHelper.SpawnAudioEmitter(definition.SFX, pos, false);
      }
      if (CountDownFloatie != null) { PersistentFloatieHelper.PoolFloatie(CountDownFloatie); CountDownFloatie = null; };
      if (this.reticle != null) { weapon.parent.Combat.DataManager.PoolGameObject(CombatCustomReticle.CustomPrefabName,this.reticle.gameObject); this.reticle = null; }
      if (wVfx != null) { wVfx.CleanupSelf(); wVfx = null; }
    }
    public void LateUpdate() {
      if ((wVfx != null)&&(ancor != null)&&(wVfx.spawnedObject != null)) { wVfx.spawnedObject.transform.position = ancor.position + offset; };
      //Log.M.TWL(0, "DeferredEffect.LateUpdate t:"+t+"/"+definition.VFXtime+" Playing:"+Playing);
      if (Playing == false) { return; }
      if (definition == null) { Playing = false; return; }
      t += Time.deltaTime;
      if (t > definition.VFXtime) {
        Log.M.TWL(0, "DeferredEffect.LateUpdate effect finished");
        if (vfx != null) { vfx.CleanupSelf(); vfx = null; }
        Playing = false;
        DeferredEffectHelper.addToClear(this);
      }
      if (t > definition.damageApplyTime) {
        if (DamageApplyied == false) {
          DamageApplyied = true;
          ApplyBurn();
          applyTempMask();
          applyAOEDamage();
        }
      }
    }
    //public void 
    public void UpdateText(int currentRound) {
      if (definition == null) { return; }
      if (CountDownFloatie != null) {
        CountDownFloatie.Text.SetText(new Text(definition.text + ":" + this.RoundsRemain(currentRound)).ToString());
      }
    }
    private void Init(Weapon weapon, DeferredEffectDef def, int currentRound) {
      this.weapon = weapon;
      definition = def;
      RoundStarted = currentRound;
      t = 0f;
      DamageApplyied = false;
      SequenceId = weapon.parent.Combat.StackManager.NextStackUID;
    }
    public void InitReticle() {
      if (definition.AOERange > 10f) {
        GameObject reticleObj = weapon.parent.Combat.DataManager.PooledInstantiate(CombatCustomReticle.CustomPrefabName, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
        if (reticleObj != null) {
          this.reticle = reticleObj.GetComponent<CombatCustomReticle>();
          this.reticle.Init(PersistentFloatieHelper.HUD, this.offset, ancor, definition.AOERange);
          reticleObj.SetActive(true);
        } else {
          reticleObj = weapon.parent.Combat.DataManager.PooledInstantiate(CombatCustomReticle.PrefabName, BattleTechResourceType.UIModulePrefabs, new Vector3?(), new Quaternion?(), (Transform)null);
          CombatAuraReticle src = reticleObj.GetComponent<CombatAuraReticle>();
          this.reticle = reticleObj.AddComponent<CombatCustomReticle>();
          this.reticle.Copy(src);
          GameObject.Destroy(src);
          this.reticle.Init(PersistentFloatieHelper.HUD, this.offset, ancor, definition.AOERange);
        }
        this.reticle.auraRangeMatBright.color = this.definition.RangeColor.Color;
        this.reticle.auraRangeMatDim.color = this.definition.RangeColor.Color;
      } else {
        this.reticle = null;
      }
      if (string.IsNullOrEmpty(definition.waitVFX) == false) {
        Vector3 pos = offset;
        if (ancor != null) { pos += ancor.position; };
        wVfx = new ObjectSpawnDataSelf(definition.waitVFX, pos, Quaternion.identity, definition.waitVFXscale.vector, true, false);
        wVfx.SpawnSelf(weapon.parent.Combat);
        Log.M.WL(1, "wVfx:" + wVfx.spawnedObject.name + ":" + wVfx.spawnedObject.transform.position);
        if (ancor != null) {
          Log.M.WL(2, "ancor:"+ancor.name);
          DeferredEffectAncor defAncor = wVfx.spawnedObject.AddComponent<DeferredEffectAncor>();
          defAncor.ancor = ancor;
          defAncor.offset = offset;
        }
      } else {
        wVfx = null;
      }
    }
    public void Init(Weapon weapon, DeferredEffectDef def, int currentRound, ICombatant ancor) {
      Log.M.WL(1, "DeferredEffect.Init");
      this.Init(weapon, def, currentRound);
      if (definition.sticky) {
        Log.M.WL(2, "effect is sticky: "+ ancor.GameRep.transform.name);
        this.ancor = ancor.GameRep.transform;
        this.offset = Vector3.zero;
      } else {
        this.ancor = null;
        this.offset = ancor.GameRep.transform.position;
      }
      CountDownFloatie = PersistentFloatieHelper.CreateFloatie(new Text(def.text + ":" + def.rounds.ToString()), 18f, this.definition.RangeColor.Color, this.ancor, Vector3.up * 10f);
      vfx = null;
      InitReticle();
    }
    public void Init(Weapon weapon, DeferredEffectDef def, int currentRound, Vector3 worldPos) {
      Log.M.WL(1, "DeferredEffect.Init");
      this.Init(weapon, def, currentRound);
      this.ancor = null;
      this.offset = worldPos;
      this.offset.y = weapon.parent.Combat.MapMetaData.GetLerpedHeightAt(worldPos);
      CountDownFloatie = PersistentFloatieHelper.CreateFloatie(new Text(def.text + ":"+def.rounds.ToString()),18f, this.definition.RangeColor.Color, this.ancor,offset+Vector3.up * 10f);
      vfx = null;
      InitReticle();
    }
    public void addEffect(ICombatant target, EffectData effect, float distance) {
      Log.M.WL(1, $"Applying effectID:{effect.Description.Id} with effectDescId:{effect?.Description.Id} effectDescName:{effect?.Description.Name}");
      if (definition.statusEffectsRangeFalloff) {
        float chance = (definition.AOERange - distance) / definition.AOERange;
        float roll = Random.Range(0f,1f);
        if (roll > chance) { Log.M.WL(2, "falloff roll fail"); return; }
      }
      Log.M.WL(2, "falloff roll success");
      string effectID = string.Format("OnDeferredHitEffect_{0}_{1}", (object)weapon.parent.GUID, (object)this.SequenceId);
      weapon.parent.Combat.EffectManager.CreateEffect(effect, effectID, this.SequenceId, weapon.parent, target, new WeaponHitInfo(), 0, false);
    }
    public void applyCallbacks() {
      if (definition == null) { return; }
      Vector3 pos = this.offset;
      if (ancor != null) { pos += ancor.position; }
      pos.y = weapon.parent.Combat.MapMetaData.GetLerpedHeightAt(pos);
      foreach(string callback_id in definition.callMethod) {
        weapon.CallDefferedMethod(callback_id, pos);
      }
    }
    public void applyAOEDamage() {
      if (definition == null) { return; }
      float Range = definition.AOERange;
      float AoEDmg = definition.AOEDamage;
      if (AoEDmg <= CustomAmmoCategories.Epsilon) { return; }
      if (Range <= CustomAmmoCategories.Epsilon) { return; }
      Log.M.TWL(0,"AoE explosion " + definition.id);
      Log.M.WL(1, " Range:" + Range + " Damage:" + AoEDmg);
      Vector3 pos = this.offset;
      if (ancor != null) { pos += ancor.position; }
      pos.y = weapon.parent.Combat.MapMetaData.GetLerpedHeightAt(pos);
      Dictionary<ICombatant, AoEExplosionRecord> AoEDamage = new Dictionary<ICombatant, AoEExplosionRecord>();
      foreach (ICombatant target in weapon.parent.Combat.GetAllLivingCombatants()) {
        if (target.IsDead) { continue; };
        if (target.isDropshipNotLanded()) { continue; };
        Vector3 CurrentPosition = target.CurrentPosition + Vector3.up * target.FlyingHeight();
        float distance = Vector3.Distance(CurrentPosition, pos);
        Log.LogWrite(" " + new Text(target.DisplayName).ToString() + ":" + target.GUID + " " + distance + "(" + CustomAmmoCategories.Settings.DefaultAoEDamageMult[target.UnitType].Range + ")\n");
        if (CustomAmmoCategories.Settings.DefaultAoEDamageMult[target.UnitType].Range < CustomAmmoCategories.Epsilon) { CustomAmmoCategories.Settings.DefaultAoEDamageMult[target.UnitType].Range = 1f; }
        distance /= CustomAmmoCategories.Settings.DefaultAoEDamageMult[target.UnitType].Range;
        target.TagAoEModifiers(out float tagAoEModRange, out float tagAoEDamage);
        if (tagAoEModRange < CustomAmmoCategories.Epsilon) { tagAoEModRange = 1f; }
        if (tagAoEDamage < CustomAmmoCategories.Epsilon) { tagAoEDamage = 1f; }
        distance /= tagAoEDamage;
        if (distance > definition.AOERange) { continue; };
        foreach (var effect in definition.statusEffects) { addEffect(target, effect, distance); };
        float HeatDamage = definition.AOEHeatDamage * CustomAmmoCategories.Settings.DefaultAoEDamageMult[target.UnitType].Damage * tagAoEDamage * (definition.AOERange - distance) / definition.AOERange;
        float Damage = definition.AOEDamage * CustomAmmoCategories.Settings.DefaultAoEDamageMult[target.UnitType].Damage * tagAoEDamage * (definition.AOERange - distance) / definition.AOERange;
        float StabDamage = definition.AOEInstability * CustomAmmoCategories.Settings.DefaultAoEDamageMult[target.UnitType].Damage * tagAoEDamage * (definition.AOERange - distance) / definition.AOERange;
        if (target.isHasHeat() == false) { Damage += HeatDamage; HeatDamage = 0f; }
        if (target.isHasStability() == false) { StabDamage = 0f; }
        Mech mech = target as Mech;
        Vehicle vehicle = target as Vehicle;
        HashSet<int> reachableLocations = new HashSet<int>();
        Dictionary<int, float> SpreadLocations = null;
        ICustomMech custMech = target as ICustomMech;
        if (custMech != null) {
          List<int> hitLocations = custMech.GetAOEPossibleHitLocations(pos);
          foreach (int loc in hitLocations) { reachableLocations.Add(loc); }
          SpreadLocations = custMech.GetAOESpreadArmorLocations();
        } else
        if (mech != null) {
          List<int> hitLocations = mech.GetAOEPossibleHitLocations(pos);//unit.Combat.HitLocation.GetPossibleHitLocations(pos, mech);
          foreach (int loc in hitLocations) { reachableLocations.Add(loc); }
          SpreadLocations = mech.GetAOESpreadArmorLocations();
        } else
        if (vehicle != null) {
          List<int> hitLocations = vehicle.Combat.HitLocation.GetPossibleHitLocations(pos, vehicle);
          if (CustomAmmoCategories.VehicleLocations == null) { CustomAmmoCategories.InitHitLocationsAOE(); };
          foreach (int loc in hitLocations) { reachableLocations.Add(loc); }
          SpreadLocations = CustomAmmoCategories.VehicleLocations;
        } else {
          List<int> hitLocations = new List<int>() { 1 };
          if (CustomAmmoCategories.OtherLocations == null) { CustomAmmoCategories.InitHitLocationsAOE(); };
          foreach (int loc in hitLocations) { reachableLocations.Add(loc); }
          SpreadLocations = CustomAmmoCategories.OtherLocations;
        }
        float locationsCoeff = 0f;
        foreach (var sLoc in SpreadLocations) {
          if (reachableLocations.Contains(sLoc.Key)) { locationsCoeff += sLoc.Value; }
        }
        Dictionary<int, float> AOELocationDamage = new Dictionary<int, float>();
        Log.M.W(2, "Location spread:");
        foreach (var sLoc in SpreadLocations) {
          if (reachableLocations.Contains(sLoc.Key) == false) { continue; }
          if (sLoc.Value < CustomAmmoCategories.Epsilon) { continue; }
          AOELocationDamage.Add(sLoc.Key, sLoc.Value / locationsCoeff);
          string lname = sLoc.Key.ToString();
          if (mech != null) { lname = ((ArmorLocation)sLoc.Key).ToString(); } else
          if (vehicle != null) { lname = ((VehicleChassisLocations)sLoc.Key).ToString(); } else
            lname = ((BuildingLocation)sLoc.Key).ToString();
          Log.M.W(1, lname + ":" + sLoc.Value / locationsCoeff);
        }
        Log.M.WL(0, "");
        if (AoEDamage.ContainsKey(target) == false) { AoEDamage.Add(target, new AoEExplosionRecord(target)); };
        AoEExplosionRecord AoERecord = AoEDamage[target];
        AoERecord.HeatDamage += HeatDamage;
        AoERecord.StabDamage += StabDamage;
        foreach (var hitLocation in AOELocationDamage) {
          float CurrentLocationDamage = Damage * hitLocation.Value;
          if (CurrentLocationDamage < CustomAmmoCategories.Epsilon) { continue; }
          if (AoERecord.hitRecords.ContainsKey(hitLocation.Key)) {
            AoERecord.hitRecords[hitLocation.Key].Damage += CurrentLocationDamage;
          } else {
            Vector3 hitPos = target.getImpactPositionSimple(weapon.parent, pos, hitLocation.Key);
            AoERecord.hitRecords.Add(hitLocation.Key,new AoEExplosionHitRecord(hitPos, CurrentLocationDamage));
          }
        }
      }
      HashSet<Mech> heatSequence = new HashSet<Mech>();
      HashSet<Mech> instabilitySequence = new HashSet<Mech>();
      HashSet<ICombatant> deathSequence = new HashSet<ICombatant>();
      var fakeHit = new WeaponHitInfo(-1, -1, -1, -1, this.weapon.parent.GUID, weapon.parent.GUID, -1, null, null, null, null, null, null
            , new AttackImpactQuality[1] { AttackImpactQuality.Solid }
            , new AttackDirection[1] { AttackDirection.FromArtillery }
            , new Vector3[1] { pos }, null, null);
      Log.M.WL(1, "Applying deferred damage");
      foreach (var mfdmgs in AoEDamage) {
        fakeHit.targetId = mfdmgs.Value.target.GUID;
        ICombatant target = mfdmgs.Key;
        foreach (var mfdmg in mfdmgs.Value.hitRecords) {
          float LocArmor = target.ArmorForLocation(mfdmg.Key);
          if ((double)LocArmor < (double)mfdmg.Value.Damage) {
            Log.M.WL(2, "floatie message structure "+ mfdmg.Value.Damage);
            weapon.parent.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(this.weapon.parent.GUID, target.GUID, new Text("{0}", new object[1]
            {
                      (object) (int) Mathf.Max(1f, mfdmg.Value.Damage)
            }), weapon.parent.Combat.Constants.CombatUIConstants.floatieSizeMedium, FloatieMessage.MessageNature.StructureDamage, mfdmg.Value.hitPosition.x, mfdmg.Value.hitPosition.y, mfdmg.Value.hitPosition.z));
          } else {
            Log.M.WL(2, "floatie message armor " + mfdmg.Value.Damage);
            weapon.parent.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(this.weapon.parent.GUID, target.GUID, new Text("{0}", new object[1]
            {
                      (object) (int) Mathf.Max(1f, mfdmg.Value.Damage)
            }), weapon.parent.Combat.Constants.CombatUIConstants.floatieSizeMedium, FloatieMessage.MessageNature.ArmorDamage, mfdmg.Value.hitPosition.x, mfdmg.Value.hitPosition.y, mfdmg.Value.hitPosition.z));
          }
          Log.M.WL(2, "take weapon damage "+mfdmg.Value.Damage);
          target.TakeWeaponDamage(fakeHit, mfdmg.Key, this.weapon, mfdmg.Value.Damage, 0f, 0, DamageType.AmmoExplosion);
        }
        if (mfdmgs.Value.hitRecords.Count > 0) {
          Log.M.WL(2, "floatie message explosion");
          deathSequence.Add(mfdmgs.Value.target);
        }
        Mech trgmech = mfdmgs.Value.target as Mech;
        if (trgmech != null) {
          if (mfdmgs.Value.HeatDamage > CustomAmmoCategories.Epsilon) {
            Log.M.WL(2, "AddExternalHeat "+ mfdmgs.Value.HeatDamage);
            trgmech.AddExternalHeat("DefferedEffectHeat", Mathf.RoundToInt(mfdmgs.Value.HeatDamage));
            //trgmech.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(this.weapon.parent.GUID, weapon.parent.GUID, new Text("__/CAC.HEATFROMLANDMINES/__", Mathf.RoundToInt(mfdmgs.Value.HeatDamage)), FloatieMessage.MessageNature.Debuff));
            heatSequence.Add(trgmech);
          }
          if (mfdmgs.Value.StabDamage > CustomAmmoCategories.Epsilon) {
            Log.M.WL(2, "AddAbsoluteInstability " + mfdmgs.Value.StabDamage);
            trgmech.AddAbsoluteInstability(mfdmgs.Value.StabDamage, StabilityChangeSource.Moving, this.weapon.parent.GUID);
            //trgmech.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(this.weapon.parent.GUID, target.GUID, new Text("__/CAC.INSTABILITYFROMLANDMINES/__", mfdmgs.Value.StabDamage), FloatieMessage.MessageNature.Debuff));
            instabilitySequence.Add(trgmech);
          }
        }
      }
      Log.M.WL(1, "Applying handling procedures");
      foreach (Mech trgmech in heatSequence) {
        Log.M.WL(2, "GenerateAndPublishHeatSequence");
        trgmech.GenerateAndPublishHeatSequence(this.SequenceId, true, false, this.weapon.parent.GUID);
      }
      foreach (Mech trgmech in instabilitySequence) {
        Log.M.WL(2, "HandleKnockdown");
        trgmech.HandleKnockdown(-1, "DEFFERED", this.weapon.parent.CurrentPosition, null);
      }
      foreach (ICombatant trg in deathSequence) {
        Log.M.WL(2, "HandleDeath");
        trg.HandleDeath(this.weapon.parent.GUID);
      }
    }
  }
}

namespace CustAmmoCategoriesPatches {
  [HarmonyPatch(typeof(TurnDirector))]
  [HarmonyPatch("Update")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class TurnDirector_Update {
    public static bool Prefix(TurnDirector __instance, bool ___needsToEndRound) {
      try {
        if (__instance.IsMissionOver) { return true; }
        DeferredEffectHelper.ClearEffectsClear();
        if (___needsToEndRound) {
          DeferredEffectHelper.UpdateDefferedEffects(__instance.CurrentRound+1);
        }
      }catch(Exception e) {
        Log.M.TWL(0,e.ToString(),true);
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(TurnDirector))]
  [HarmonyPatch("CanAdvanceTurns")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class TurnDirector_CanAdvanceTurns {
    public static void Postfix(TurnDirector __instance,ref bool __result) {
      if (__result == false) { return; }
      try {
        if (DeferredEffectHelper.HasUnApplyedEffects()) { __result = false; }
      }catch(Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
    }
  }

}