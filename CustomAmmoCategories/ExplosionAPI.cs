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
using CustomAmmoCategoriesLog;
using CustomAmmoCategoriesPatches;
using HarmonyLib;
using Localize;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CustAmmoCategories {
  [HarmonyPatch(typeof(CombatHUD))]
  [HarmonyPatch("Update")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatHUD_Update {
    private static float t = 0f;
    public static void Postfix(CombatHUD __instance) {
      if (t > 5f) {
        ExplosionAPIHelper.Update(t);
        t = 0f;
      } else {
        t += Time.deltaTime;
      }
    }
  }
  public static class ExplosionAPIHelper{
    public class ExplosionWeaponData {
      public AbstractActor unit { get; set; }
      public BaseComponentRef componentRef { get; set; }
      public Weapon weapon { get; set; }
      public ExplosionWeaponData(AbstractActor unit) {
        this.unit = unit;
        if (unit is Mech mech) {
          MechComponentRef mechComponentRef = new MechComponentRef("Weapon_Laser_AI_Imaginary", "", ComponentType.Weapon, ChassisLocations.CenterTorso);
          mechComponentRef.DataManager = unit.Combat.DataManager;
          mechComponentRef.RefreshComponentDef();
          componentRef = mechComponentRef;
          this.weapon = new Weapon(mech, unit.Combat, mechComponentRef, unit.uid + "_Explosion");
        }else if(unit is Vehicle vehcile) {
          VehicleComponentRef vehicleComponentRef = new VehicleComponentRef("Weapon_Laser_AI_Imaginary", "", ComponentType.Weapon, VehicleChassisLocations.Front);
          vehicleComponentRef.DataManager = unit.Combat.DataManager;
          vehicleComponentRef.RefreshComponentDef();
          componentRef = vehicleComponentRef;
          this.weapon = new Weapon(vehcile, unit.Combat, vehicleComponentRef, unit.uid + "_Explosion");
        }else if(unit is Turret turret) {
          TurretComponentRef turretComponentRef = new TurretComponentRef("Weapon_Laser_AI_Imaginary", "", ComponentType.Weapon, VehicleChassisLocations.Front);
          turretComponentRef.DataManager = unit.Combat.DataManager;
          turretComponentRef.RefreshComponentDef();
          componentRef = turretComponentRef;
          this.weapon = new Weapon(turret, unit.Combat, turretComponentRef, unit.uid + "_Explosion");
        }
        this.weapon.EnableWeapon();
        this.weapon.ResetWeapon();
      }
    }
    private static Dictionary<AbstractActor, ExplosionWeaponData> exposionWeapon = new Dictionary<AbstractActor, ExplosionWeaponData>();
    private static CombatGameState combat;
    private static AbstractActor fakeActor;
    private static Weapon fakeWeapon;
    private static string GUID;
    private static bool Inited = false;
    private static Dictionary<ObjectSpawnDataSelf, float> explodeVFXdurations = new Dictionary<ObjectSpawnDataSelf, float>();
    public static void Update(float delta) {
      HashSet<ObjectSpawnDataSelf> toClear = new HashSet<ObjectSpawnDataSelf>();
      HashSet<ObjectSpawnDataSelf> toWatch = explodeVFXdurations.Keys.ToHashSet();
      foreach(ObjectSpawnDataSelf ex in toWatch) {
        explodeVFXdurations[ex] -= delta;
        if (explodeVFXdurations[ex] <= 0f) { toClear.Add(ex); }
      }
      foreach(ObjectSpawnDataSelf ex in toClear) {
        explodeVFXdurations.Remove(ex);
        ex.CleanupSelf();
      }
    }
    public static void Clear() {
      try {
        foreach (var ex in explodeVFXdurations) {
          ex.Key?.CleanupSelf();
        }
        explodeVFXdurations.Clear();
        fakeActor = null;
        combat = null;
        fakeWeapon = null;
        Inited = false;
        exposionWeapon.Clear();
      }catch(Exception e) {
        Log.M?.TWL(0,e.ToString(),true);
      }
    }
    public static void Init(CombatGameState combat) {
      try {
        Log.M.TWL(0, "ExplosionAPIHelper.Init");
        GUID = "FAKE_" + Guid.NewGuid().ToString();
        ExplosionAPIHelper.combat = combat;
        AbstractActor srcActor = null;
        foreach (AbstractActor actor in combat.AllActors) {
          if ((actor.TeamId == combat.LocalPlayerTeamGuid) && (actor.Weapons.Count > 0)
            && ((actor.UnitType == UnitType.Vehicle) || (actor.UnitType == UnitType.Mech))) { srcActor = actor; break; }
        };
        if (srcActor == null) { return; }
        Mech srcMech = srcActor as Mech;
        Vehicle srcVehicle = srcActor as Vehicle;
        if ((srcMech == null) && (srcVehicle == null)) { return; }
        if (srcMech != null) {
          fakeActor = new Mech(srcMech.MechDef, srcMech.pilot.pilotDef, new HBS.Collections.TagSet(), GUID, combat, srcMech.spawnerGUID, srcMech.CustomHeraldryDef);
          fakeActor.SetGuid(GUID);
          MechComponentRef cmpRef = new MechComponentRef("Weapon_Laser_AI_Imaginary", "", ComponentType.Weapon, ChassisLocations.CenterTorso);
          cmpRef.DataManager = fakeActor.Combat.DataManager;
          cmpRef.RefreshComponentDef();
          //cmpRef.ComponentDefID = srcMech.Weapons[0].defId;
          fakeWeapon = new Weapon(fakeActor as Mech, combat, cmpRef, GUID + ".Explosion");
          typeof(MechComponent).GetProperty("componentDef", BindingFlags.Instance | BindingFlags.Public).GetSetMethod(true).Invoke(fakeWeapon, new object[1] { (object)srcMech.Weapons[0].componentDef });
        }
        if (srcVehicle != null) {
          fakeActor = new Vehicle(srcVehicle.VehicleDef, srcVehicle.pilot.pilotDef, new HBS.Collections.TagSet(), GUID, combat, srcVehicle.spawnerGUID, srcVehicle.CustomHeraldryDef);
          fakeActor.SetGuid(GUID);
          VehicleComponentRef cmpRef = new VehicleComponentRef("Weapon_Laser_AI_Imaginary", "", ComponentType.Weapon, VehicleChassisLocations.Front);
          cmpRef.DataManager = fakeActor.Combat.DataManager;
          cmpRef.RefreshComponentDef();
          //cmpRef.ComponentDefID = srcVehicle.Weapons[0].defId;
          fakeWeapon = new Weapon(fakeActor as Vehicle, combat, cmpRef, GUID + ".Explosion");
          typeof(MechComponent).GetProperty("componentDef", BindingFlags.Instance | BindingFlags.Public).GetSetMethod(true).Invoke(fakeWeapon, new object[1] { (object)srcVehicle.Weapons[0].componentDef });
        }
        if ((fakeActor == null) || (fakeWeapon == null)) { return; }
        typeof(AbstractActor).GetField("_team", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(fakeActor, combat.LocalPlayerTeam);
        typeof(AbstractActor).GetField("_teamId", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(fakeActor, combat.LocalPlayerTeamGuid);
        Inited = true;
      }catch(Exception e) {
        Log.M.TWL(0,e.ToString(),true);
      }
    }
    public static DesignMaskDef TempDesignMask(string name) {
      if (string.IsNullOrEmpty(name)) { return null; };
      if (DynamicMapHelper.loadedMasksDef.ContainsKey(name) == false) { return null; }
      return DynamicMapHelper.loadedMasksDef[name];
    }
    public static void applyExplodeBurn(Weapon fakeWeapon,Vector3 pos, int fireRadius, int fireStrength, float fireChance, int fireDurationNoForest) {
      Log.LogWrite("Applying burn effect:"+pos + "\n");
      MapTerrainDataCellEx cell = combat.MapMetaData.GetCellAt(pos) as MapTerrainDataCellEx;
      if (cell == null) {
        CustomAmmoCategoriesLog.Log.LogWrite(" cell is not extended\n");
        return;
      }
      Log.LogWrite(" fire at " + pos + "\n");
      fireDurationNoForest = Mathf.RoundToInt((float)fireDurationNoForest * DynamicMapHelper.BiomeWeaponFireDuration());
      fireStrength = Mathf.RoundToInt((float)fireStrength * DynamicMapHelper.BiomeWeaponFireStrength());
      fireChance *= DynamicMapHelper.BiomeLitFireChance();

      if (fireRadius == 0) {
        cell.hexCell.TryBurnCellAsync(fakeWeapon, fireChance, fireStrength, fireDurationNoForest);
        //if (cell.hexCell.TryBurnCell(fakeWeapon, fireChance, fireStrength, fireDurationNoForest)) {
          //DynamicMapHelper.burningHexes.Add(cell.hexCell);
        //};
      } else {
        List<MapTerrainHexCell> affectedHexCells = MapTerrainHexCell.listHexCellsByCellRadius(cell, fireRadius);
        foreach (MapTerrainHexCell hexCell in affectedHexCells) {
          hexCell.TryBurnCellAsync(fakeWeapon, fireChance, fireStrength, fireDurationNoForest);
          //if (hexCell.TryBurnCell(fakeWeapon, fireChance, fireStrength, fireDurationNoForest)) {
          //DynamicMapHelper.burningHexes.Add(hexCell);
          //};
        }
      }
    }
    public static void applyImpactTempMask(Vector3 pos, string LongVFX, Vector3 scale, string designMask, int radius, int turns) {
      Log.M.TWL(0, "Applying explode long effect:" + LongVFX + "/" + designMask + "/" + pos);
      DesignMaskDef mask = TempDesignMask(designMask);
      MapTerrainDataCellEx cell = combat.MapMetaData.GetCellAt(pos) as MapTerrainDataCellEx;
      if (cell == null) {
        CustomAmmoCategoriesLog.Log.LogWrite(" cell is not extended\n");
        return;
      }
      //if (mask == null) { return; };
      if (radius == 0) {
        cell.hexCell.addTempTerrainVFX(combat, LongVFX, turns, scale);
        cell.hexCell.addDesignMaskAsync(mask, turns);
        //AsyncDesignMaskUpdater admu = new AsyncDesignMaskUpdater(cell.hexCell, mask, turns);
        //Thread designMaskApplyer = new Thread(new ThreadStart(admu.asyncAddMask));
        //designMaskApplyer.Start();
      } else {
        List<MapTerrainHexCell> affectedHexCells = MapTerrainHexCell.listHexCellsByCellRadius(cell, radius);
        foreach (MapTerrainHexCell hexCell in affectedHexCells) {
          hexCell.addTempTerrainVFX(combat, LongVFX, turns, scale);
          hexCell.addDesignMaskAsync(mask, turns);
        }
        //AsyncDesignMaskUpdater admu = new AsyncDesignMaskUpdater(affectedHexCells, mask, turns);
        //Thread designMaskApplyer = new Thread(new ThreadStart(admu.asyncAddMask));
        //designMaskApplyer.Start();
      }
    }
    public static void AoEPlayExplodeVFX(string VFX, Vector3 vfxScale, string SFX, Vector3 pos, float duration) {
      if (string.IsNullOrEmpty(SFX) == false) CustomSoundHelper.SpawnAudioEmitter(SFX, pos, false);
      if (string.IsNullOrEmpty(VFX)||(duration < CustomAmmoCategories.Epsilon)) { return; };
      ObjectSpawnDataSelf explodeObject = new ObjectSpawnDataSelf(VFX, pos, Quaternion.identity, vfxScale, true, false);
      explodeObject.SpawnSelf(combat);
      explodeVFXdurations.Add(explodeObject, duration);
    }
    public static void AoEExplode(string VFX,Vector3 vfxScale, float vfxDuration, string SFX, Vector3 pos, 
      float radius, float damage, float heat, float stability, List<EffectData> effects, bool effectsFalloff, int fireRadius, int fireStrength, float fireChance, int fireDurationNoForest, 
      string LongVFX, Vector3 longVFXScale, string designMask, int dmRadius, int turns
    ) {
      if (Inited == false) { return; };
      float Range = radius;
      float AoEDmg = damage;
      if (AoEDmg <= CustomAmmoCategories.Epsilon) { return; }
      if (Range <= CustomAmmoCategories.Epsilon) { return; }
      Log.M.TWL(0,"Spawning explode VFX");
      Log.LogWrite(" Range:" + Range + " Damage:" + AoEDmg + "\n");
      List<AoEExplosionRecord> AoEDamage = new List<AoEExplosionRecord>();
      //List<EffectData> effects = component.AoEExplosionEffects();
      int SequenceID = combat.StackManager.NextStackUID;
      foreach (ICombatant target in combat.GetAllLivingCombatants()) {
        if (target.IsDead) { continue; };
        if (target.isDropshipNotLanded()) { continue; };
        Vector3 CurrentPosition = target.CurrentPosition + Vector3.up * target.FlyingHeight();
        float distance = Vector3.Distance(CurrentPosition, pos);
        if (CustomAmmoCategories.Settings.DefaultAoEDamageMult[target.UnitType].Range < CustomAmmoCategories.Epsilon) { CustomAmmoCategories.Settings.DefaultAoEDamageMult[target.UnitType].Range = 1f; }
        distance /= CustomAmmoCategories.Settings.DefaultAoEDamageMult[target.UnitType].Range;
        if (distance > Range) { continue; };
        float HeatDamage = heat * (Range - distance) / Range;
        float Damage = AoEDmg * CustomAmmoCategories.Settings.DefaultAoEDamageMult[target.UnitType].Damage * (Range - distance) / Range;
        float StabDamage = stability * (Range - distance) / Range;
        foreach (EffectData effect in effects) {
          string effectID = string.Format("OnComponentAoEExplosionEffect_{0}_{1}", (object)fakeActor.GUID, (object)SequenceID);
          Log.LogWrite($"  Applying effectID:{effect.Description.Id} with effectDescId:{effect?.Description.Id} effectDescName:{effect?.Description.Name}\n");
          combat.EffectManager.CreateEffect(effect, effectID, -1, fakeActor, target, new WeaponHitInfo(), 0, false);
        }
        Mech mech = target as Mech;
        Vehicle vehicle = target as Vehicle;
        if (target.isHasHeat() == false) { Damage += HeatDamage; HeatDamage = 0f; };
        if (target.isHasStability() == false) { StabDamage = 0f; }
        HashSet<int> reachableLocations = new HashSet<int>();
        Dictionary<int, float> SpreadLocations = null;
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
        AoEExplosionRecord AoERecord = new AoEExplosionRecord(target);
        AoERecord.HeatDamage = HeatDamage;
        AoERecord.StabDamage = StabDamage;
        foreach (var hitLocation in AOELocationDamage) {
          float CurrentLocationDamage = Damage * hitLocation.Value;
          if (AoERecord.hitRecords.ContainsKey(hitLocation.Key)) {
            AoERecord.hitRecords[hitLocation.Key].Damage += CurrentLocationDamage;
          } else {
            Vector3 hitpos = target.getImpactPositionSimple(pos, hitLocation.Key);
            AoERecord.hitRecords.Add(hitLocation.Key,new AoEExplosionHitRecord(hitpos, CurrentLocationDamage));
          }
          Log.LogWrite("  location " + hitLocation + " damage " + AoERecord.hitRecords[hitLocation.Key].Damage + "\n");
        }
        AoEDamage.Add(AoERecord);
      }
      Log.LogWrite("AoE Damage result:\n");
      applyExplodeBurn(fakeWeapon, pos, fireRadius,fireStrength, fireChance, fireDurationNoForest);
      applyImpactTempMask(pos, LongVFX, longVFXScale, designMask, dmRadius, turns);
      AoEPlayExplodeVFX(VFX, vfxScale, SFX, pos, vfxDuration);
      var fakeHit = new WeaponHitInfo(-1, -1, -1, -1, fakeActor.GUID, fakeActor.GUID, -1, null, null, null, null, null, null
        , new AttackImpactQuality[1] { AttackImpactQuality.Solid }
        , new AttackDirection[1] { AttackDirection.FromArtillery }
        , new Vector3[1] { Vector3.zero }, null, null);
      for (int index = 0; index < AoEDamage.Count; ++index) {
        Log.LogWrite(" " + AoEDamage[index].target.DisplayName + ":" + AoEDamage[index].target.GUID + "\n");
        Log.LogWrite(" Heat:" + AoEDamage[index].HeatDamage + "\n");
        Log.LogWrite(" Instability:" + AoEDamage[index].StabDamage + "\n");
        fakeHit.targetId = AoEDamage[index].target.GUID;
        foreach (var AOEHitRecord in AoEDamage[index].hitRecords) {
          Log.LogWrite("  location:" + AOEHitRecord.Key + " pos:" + AOEHitRecord.Value.hitPosition + " dmg:" + AOEHitRecord.Value.Damage + "\n");
          float LocArmor = AoEDamage[index].target.ArmorForLocation(AOEHitRecord.Key);
          if ((double)LocArmor < (double)AOEHitRecord.Value.Damage) {
            combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(fakeActor.GUID, AoEDamage[index].target.GUID, new Text("{0}", new object[1]
            {
                      (object) (int) Mathf.Max(1f, AOEHitRecord.Value.Damage)
            }), combat.Constants.CombatUIConstants.floatieSizeMedium, FloatieMessage.MessageNature.StructureDamage, AOEHitRecord.Value.hitPosition.x, AOEHitRecord.Value.hitPosition.y, AOEHitRecord.Value.hitPosition.z));
          } else {
            combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(fakeActor.GUID, AoEDamage[index].target.GUID, new Text("{0}", new object[1]
            {
                      (object) (int) Mathf.Max(1f, AOEHitRecord.Value.Damage)
            }), combat.Constants.CombatUIConstants.floatieSizeMedium, FloatieMessage.MessageNature.ArmorDamage, AOEHitRecord.Value.hitPosition.x, AOEHitRecord.Value.hitPosition.y, AOEHitRecord.Value.hitPosition.z));
          }
          fakeHit.hitPositions[0] = AOEHitRecord.Value.hitPosition;
          AoEDamage[index].target.TakeWeaponDamage(fakeHit, AOEHitRecord.Key, fakeWeapon, AOEHitRecord.Value.Damage, 0f, 0, DamageType.AmmoExplosion);
        }
        AoEDamage[index].target.HandleDeath(fakeActor.GUID);
        Mech mech = AoEDamage[index].target as Mech;
        if (mech != null) {
          if (AoEDamage[index].HeatDamage > CustomAmmoCategories.Epsilon) {
            mech.AddExternalHeat("AoE Component explosion", Mathf.RoundToInt(AoEDamage[index].HeatDamage));
            mech.GenerateAndPublishHeatSequence(-1, true, false, fakeActor.GUID);
          }
          if (AoEDamage[index].StabDamage > CustomAmmoCategories.Epsilon) {
            mech.AddAbsoluteInstability(AoEDamage[index].StabDamage, StabilityChangeSource.Effect, fakeActor.GUID);
          }
          mech.HandleKnockdown(-1, fakeActor.GUID, Vector2.one, (SequenceFinished)null);
        }
      }
    }
    public static void LayMineField(MineFieldDef def, Vector3 pos) {
      CustomAmmoCategoriesLog.Log.LogWrite("Applying minefield:" + pos + "\n");
      if ((fakeWeapon == null) || (fakeActor == null)) { return; }
      MapTerrainDataCellEx cell = combat.MapMetaData.GetCellAt(pos) as MapTerrainDataCellEx;
      if (cell == null) {
        CustomAmmoCategoriesLog.Log.LogWrite(" cell is not extended\n");
        return;
      }
      CustomAmmoCategoriesLog.Log.LogWrite(" impact at " + pos + "\n");
      MineFieldDef mfd = def;
      if (mfd.InstallCellRange == 0) {
        //Log.LogWrite(" affected cell " + cell.hexCell.x + "," + cell.hexCell.y + ":" + mfd.Count + "\n");
        Log.LogWrite(" affected cell " + cell.hexCell.center + ":" + mfd.Count + "\n");
        cell.hexCell.addMineField(mfd, fakeActor, fakeWeapon);
      } else {
        List<MapTerrainHexCell> affectedHexCells = MapTerrainHexCell.listHexCellsByCellRadius(cell, mfd.InstallCellRange);
        foreach (MapTerrainHexCell hexCell in affectedHexCells) {
          //Log.LogWrite(" affected cell " + hexCell.x + "," + hexCell.y + ":" + mfd.Count + "\n");
          Log.LogWrite(" affected cell " + hexCell.center + ":" + mfd.Count + "\n");
          hexCell.addMineField(mfd, fakeActor, fakeWeapon);
        }
      }

    }
  }
}