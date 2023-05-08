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
  using HarmonyLib;

  public class CalledShotPopUp : BattleModModule {

    private static string CalledShotHitChanceFormat = "{0:0}%";

    public override void ModStarts() {
      try {
        Type CalledShot = typeof(CombatHUDCalledShotPopUp);
        if (AIMSettings.ShowLocationInfoInCalledShot)
          Patch(CalledShot, "UpdateMechDisplay", null, "ShowCalledLocationHP");

        if (AIMSettings.CalledChanceFormat != null)
          CalledShotHitChanceFormat = AIMSettings.CalledChanceFormat;

        if (AIMSettings.FixBossHeadCalledShotDisplay) {
          Patch(CalledShot, "UpdateMechDisplay", "FixBossHead", "CleanupBossHead");
        }

        if (AIMSettings.ShowRealMechCalledShotChance || AIMSettings.ShowRealVehicleCalledShotChance || AIMSettings.CalledChanceFormat != null) {
          Patch(CalledShot, "set_ShownAttackDirection", typeof(AttackDirection), null, "RecordAttackDirection");

          if (AIMSettings.ShowRealMechCalledShotChance || AIMSettings.CalledChanceFormat != null)
            Patch(CalledShot, "GetHitPercent", new Type[] { typeof(ArmorLocation), typeof(ArmorLocation) }, "OverrideHUDMechCalledShotPercent", null);

          if (AIMSettings.ShowRealVehicleCalledShotChance || AIMSettings.CalledChanceFormat != null)
            Patch(CalledShot, "GetHitPercent", new Type[] { typeof(VehicleChassisLocations), typeof(VehicleChassisLocations) }, "OverrideHUDVehicleCalledShotPercent", null);
        }
      }catch(Exception e) {
        Error(e.ToString());
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

    private static int head;

    public static void FixBossHead(CombatHUDCalledShotPopUp __instance) {
      if (__instance.DisplayedActor?.CanBeHeadShot ?? true) return;
      Dictionary<ArmorLocation, int> currentHitTable = __instance.currentHitTable;
      if (currentHitTable == null || !currentHitTable.TryGetValue(ArmorLocation.Head, out head)) return;
      currentHitTable[ArmorLocation.Head] = 0;
    }

    public static void CleanupBossHead(CombatHUDCalledShotPopUp __instance) {
      if (head <= 0) return;
      Dictionary<ArmorLocation, int> currentHitTable = __instance.currentHitTable;
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

    [HarmonyLib.HarmonyPriority(HarmonyLib.Priority.Low)]
    public static bool OverrideHUDMechCalledShotPercent(CombatHUDCalledShotPopUp __instance, ref string __result, ArmorLocation location, ArmorLocation targetedLocation) {
      try {
        Dictionary<ArmorLocation, int> hitTable = null;
        ICustomMech custMech = __instance.displayedMech as ICustomMech;
        if (custMech != null) {
          hitTable = (targetedLocation == ArmorLocation.None || !CallShotClustered || !AIMSettings.ShowRealMechCalledShotChance)
                                                   ? custMech.GetHitTable(__instance.shownAttackDirection)
                                                   : custMech.GetHitTableCluster(__instance.shownAttackDirection, targetedLocation);
        } else {
          hitTable = (targetedLocation == ArmorLocation.None || !CallShotClustered || !AIMSettings.ShowRealMechCalledShotChance)
                                                   ? Combat.HitLocation.GetMechHitTable(__instance.shownAttackDirection)
                                                   : Combat.Constants.GetMechClusterTable(targetedLocation, __instance.shownAttackDirection);
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

    [HarmonyLib.HarmonyPriority(HarmonyLib.Priority.Low)]
    public static bool OverrideHUDVehicleCalledShotPercent(CombatHUDCalledShotPopUp __instance, ref string __result, VehicleChassisLocations location, VehicleChassisLocations targetedLocation) {
      try {
        Dictionary<VehicleChassisLocations, int> hitTable = Combat.HitLocation.GetVehicleHitTable(__instance.shownAttackDirection);
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