using BattleTech.UI;
using BattleTech;
using System;
using System.Collections.Generic;
using System.Reflection;
using static System.Reflection.BindingFlags;

namespace Sheepy.BattleTechMod.AttackImprovementMod {
  using static Mod;
  using static HitLocation;
  using Localize;
  using CustomAmmoCategoriesPatches;
  using CustAmmoCategories;
  using IRBTModUtils;

  public class CalledShotPopUp : BattleModModule {

    private static string CalledShotHitChanceFormat = "{0:0}%";

    public override void CombatStartsOnce() {
      Type CalledShot = typeof(CombatHUDCalledShotPopUp);
      if (AIMSettings.ShowLocationInfoInCalledShot)
        Patch(CalledShot, "UpdateMechDisplay", null, "ShowCalledLocationHP");

      if (AIMSettings.CalledChanceFormat != null)
        CalledShotHitChanceFormat = AIMSettings.CalledChanceFormat;

      if (AIMSettings.FixBossHeadCalledShotDisplay) {
        currentHitTableProp = typeof(CombatHUDCalledShotPopUp).GetProperty("currentHitTable", NonPublic | Instance);
        if (currentHitTableProp == null)
          Error("Cannot find CombatHUDCalledShotPopUp.currentHitTable, boss head called shot display not fixed. Boss should still be immune from headshot.");
        else
          Patch(CalledShot, "UpdateMechDisplay", "FixBossHead", "CleanupBossHead");
      }

      if (AIMSettings.ShowRealMechCalledShotChance || AIMSettings.ShowRealVehicleCalledShotChance || AIMSettings.CalledChanceFormat != null) {
        Patch(CalledShot, "set_ShownAttackDirection", typeof(AttackDirection), null, "RecordAttackDirection");

        if (AIMSettings.ShowRealMechCalledShotChance || AIMSettings.CalledChanceFormat != null)
          Patch(CalledShot, "GetHitPercent", new Type[] { typeof(ArmorLocation), typeof(ArmorLocation) }, "OverrideHUDMechCalledShotPercent", null);

        if (AIMSettings.ShowRealVehicleCalledShotChance || AIMSettings.CalledChanceFormat != null)
          Patch(CalledShot, "GetHitPercent", new Type[] { typeof(VehicleChassisLocations), typeof(VehicleChassisLocations) }, "OverrideHUDVehicleCalledShotPercent", null);
      }
    }

    public override void CombatEnds() {
      title = null;
    }

    // ============ Hover Info ============

    private static TMPro.TextMeshProUGUI title;

    public static void ShowCalledLocationHP(CombatHUDCalledShotPopUp __instance) {
      try {
        if (title == null) {
          title = UnityEngine.GameObject.Find("calledShot_Title")?.GetComponent<TMPro.TextMeshProUGUI>();
          title.enableAutoSizing = false;
          if (title == null) return;
        }

        CombatHUDCalledShotPopUp me = __instance;
        ArmorLocation hoveredArmor = me.MechArmorDisplay.HoveredArmor;
        if (me.locationNameText.text.StartsWith("-")) {
          title.SetText(new Text("__/AIM.Called_Shot/__").ToString());
        } else if (me.DisplayedActor is Mech mech) {
          float hp = mech.GetCurrentStructure(MechStructureRules.GetChassisLocationFromArmorLocation(hoveredArmor));
          if (hp <= 0) {
            title.SetText(new Text("__/AIM.Called_Shot/__").ToString());
            me.locationNameText.SetText(new Text("__/AIM.choose_target/__").ToString(), ZeroObjects);
          } else {
            float mhp = mech.GetMaxStructure(MechStructureRules.GetChassisLocationFromArmorLocation(hoveredArmor)),
                  armour = mech.GetCurrentArmor(hoveredArmor), marmour = mech.GetMaxArmor(hoveredArmor);
            title.text = me.locationNameText.text;
            me.locationNameText.text = string.Format("{0:0}/{1:0} <#FFFFFF>{2:0}/{3:0}", hp, mhp, armour, marmour);
          }
        }
      } catch (Exception ex) { Error(ex); }
    }

    // ============ Game States ============

    private static float ActorCalledShotBonus { get { return HUD.SelectedActor.CalledShotBonusMultiplier; } }

    //private static AttackDirection AttackDirection;

    public static void RecordAttackDirection(AttackDirection value) {
      //AttackDirection = value;
    }

    // ============ Boss heads ============

    private static PropertyInfo currentHitTableProp;
    private static int head;

    public static void FixBossHead(CombatHUDCalledShotPopUp __instance) {
      if (__instance.DisplayedActor?.CanBeHeadShot ?? true) return;
      Dictionary<ArmorLocation, int> currentHitTable = (Dictionary<ArmorLocation, int>)currentHitTableProp.GetValue(__instance, null);
      if (currentHitTable == null || !currentHitTable.TryGetValue(ArmorLocation.Head, out head)) return;
      currentHitTable[ArmorLocation.Head] = 0;
    }

    public static void CleanupBossHead(CombatHUDCalledShotPopUp __instance) {
      if (head <= 0) return;
      Dictionary<ArmorLocation, int> currentHitTable = (Dictionary<ArmorLocation, int>)currentHitTableProp.GetValue(__instance, null);
      currentHitTable[ArmorLocation.Head] = head;
      head = 0;
    }

    // ============ HUD Override ============

    private static Object LastHitTable;
    private static int HitTableTotalWeight;
    private static int lastCalledShotLocation;

    private static bool CacheNeedRefresh(Object hitTable, int targetedLocation) {
      bool result = !Object.ReferenceEquals(hitTable, LastHitTable) || lastCalledShotLocation != targetedLocation;
      if (result) {
        LastHitTable = hitTable;
        lastCalledShotLocation = targetedLocation;
      }
      return result;
    }

    [Harmony.HarmonyPriority(Harmony.Priority.Low)]
    public static bool OverrideHUDMechCalledShotPercent(AttackDirection ___shownAttackDirection, Mech ___displayedMech, ref string __result, ArmorLocation location, ArmorLocation targetedLocation) {
      try {
        //CustomAmmoCategoriesLog.Log.AIM.TWL(0, $"OverrideHUDMechCalledShotPercent {___displayedMech.PilotableActorDef.ChassisID} {___shownAttackDirection} location:{location} targeted:{targetedLocation}");
        Dictionary<ArmorLocation, int> hitTable = null;//CustAmmoCategories.HitTableHelper.GetHitTable(___displayedMech, targetedLocation, AttackDirection);
        ICustomMech custMech = ___displayedMech as ICustomMech;
        if (custMech != null) {
          hitTable = (targetedLocation == ArmorLocation.None || !CallShotClustered || !AIMSettings.ShowRealMechCalledShotChance)
                                                   ? custMech.GetHitTable(___shownAttackDirection)
                                                   : custMech.GetHitTableCluster(___shownAttackDirection, targetedLocation);
        } else {
          hitTable = (targetedLocation == ArmorLocation.None || !CallShotClustered || !AIMSettings.ShowRealMechCalledShotChance)
                                                   ? Combat.HitLocation.GetMechHitTable(___shownAttackDirection)
                                                   : Combat.Constants.GetMechClusterTable(targetedLocation, ___shownAttackDirection);
        }
        //CustomAmmoCategoriesLog.Log.AIM.WL(1,"hitTable ");
        //foreach (var hit in hitTable) { CustomAmmoCategoriesLog.Log.AIM.W(1, $"{hit.Key}:{hit.Value}"); }; CustomAmmoCategoriesLog.Log.AIM.WL(0, "");
        if (CacheNeedRefresh(hitTable, (int)targetedLocation))
          HitTableTotalWeight = SumWeight(hitTable, targetedLocation, FixMultiplier(targetedLocation, ActorCalledShotBonus, (custMech != null ? ((custMech.isSquad == false) && (custMech.isVehicle == false)) : true), (custMech != null ? custMech.isVehicle : false)), scale);

        int local = TryGet(hitTable, location) * scale;
        if (location == targetedLocation)
          local = (int)(local * FixMultiplier(targetedLocation, ActorCalledShotBonus, (custMech != null ? ((custMech.isSquad == false) && (custMech.isVehicle == false)) : true), (custMech != null ? custMech.isVehicle : false)));
        //CustomAmmoCategoriesLog.Log.AIM.WL(1, $"result: {local}/{HitTableTotalWeight}");
        __result = FineTuneAndFormat(hitTable, location, local, AIMSettings.ShowRealMechCalledShotChance);
        return false;
      } catch (Exception ex) { return Error(ex); }
    }

    [Harmony.HarmonyPriority(Harmony.Priority.Low)]
    public static bool OverrideHUDVehicleCalledShotPercent(AttackDirection ___shownAttackDirection, ref string __result, VehicleChassisLocations location, VehicleChassisLocations targetedLocation) {
      try {
        Dictionary<VehicleChassisLocations, int> hitTable = Combat.HitLocation.GetVehicleHitTable(___shownAttackDirection);
        if (CacheNeedRefresh(hitTable, (int)targetedLocation))
          HitTableTotalWeight = SumWeight(hitTable, targetedLocation, FixMultiplier(targetedLocation, ActorCalledShotBonus), scale);

        int local = TryGet(hitTable, location) * scale;
        if (location == targetedLocation)
          local = (int)(local * FixMultiplier(targetedLocation, ActorCalledShotBonus));

        __result = FineTuneAndFormat(hitTable, location, local, AIMSettings.ShowRealVehicleCalledShotChance);
        return false;
      } catch (Exception ex) { return Error(ex); }
    }

    // ============ Subroutines ============

    private static string FineTuneAndFormat<T>(Dictionary<T, int> hitTable, T location, int local, bool simulate) {
      try {
        float perc = local * 100f / HitTableTotalWeight;
        return string.Format(CalledShotHitChanceFormat, perc);
      } catch (Exception ex) {
        Error(ex);
        return "ERR";
      }
    }
  }
}