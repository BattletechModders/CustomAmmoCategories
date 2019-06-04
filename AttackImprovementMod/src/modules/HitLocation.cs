using BattleTech;
using Harmony;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Sheepy.BattleTechMod.AttackImprovementMod {
   using static ArmorLocation;
   using static Mod;

   public class HitLocation : BattleModModule {

      private const int SCALE = 1024; // Increase precisions of float to int conversions. Set it too high may cause overflow.
      internal static int scale = SCALE; // Actual scale. Determined by FixHitDistribution.

      internal static bool CallShotClustered = false; // True if clustering is enabled, OR is game is ver 1.0.4 or before

      private static float MechCalledShotMultiplier, VehicleCalledShotMultiplier;

      public override void CombatStartsOnce () {
         scale = Settings.FixHitDistribution ? SCALE : 1;
         CallShotClustered = Settings.CalledShotUseClustering;
         MechCalledShotMultiplier = (float) Settings.MechCalledShotMultiplier;
         VehicleCalledShotMultiplier = (float) Settings.VehicleCalledShotMultiplier;

         if ( Settings.FixCalledShotMultiplierSquare )
            Patch( typeof( AbstractActor ), "get_CalledShotBonusMultiplier", null, "FixCalledShotMultiplierSquare" );

         bool prefixMech    = MechCalledShotMultiplier    != 1 || Settings.CalledShotUseClustering,
              prefixVehicle = VehicleCalledShotMultiplier != 1;
         MethodInfo MechGetHit    = AttackLog.GetHitLocation( typeof( ArmorLocation ) ),
                    VehicleGetHit = AttackLog.GetHitLocation( typeof( VehicleChassisLocations ) );
         if ( prefixMech ) {
            Patch( typeof( BattleTech.HitLocation ), "GetMechHitTable", null, "RecordHitDirection" );
            Patch( MechGetHit, "PrefixMechCalledShot", null );
         }
         if ( prefixVehicle )
            Patch( VehicleGetHit, "PrefixVehicleCalledShot", null );
         if ( Settings.FixHitDistribution ) {
            ScaledMechHitTables = new Dictionary<Dictionary<ArmorLocation, int>, Dictionary<ArmorLocation, int>>();
            ScaledVehicleHitTables = new Dictionary<Dictionary<VehicleChassisLocations, int>, Dictionary<VehicleChassisLocations, int>>();
            Patch( MechGetHit, "ScaleMechHitTable", "RestoreHeadToScaledHitTable" );
            Patch( VehicleGetHit, "ScaleVehicleHitTable", null );
         }
      }

      private static bool ClusterChanceNeverMultiplyHead = true;
      private static float ClusterChanceOriginalLocationMultiplier = 1f, CalledShotBonusMultiplier = 2f;

      public override void CombatStarts () {
         ClusterChanceNeverMultiplyHead = CombatConstants.ToHit.ClusterChanceNeverMultiplyHead;
         ClusterChanceOriginalLocationMultiplier = CombatConstants.ToHit.ClusterChanceOriginalLocationMultiplier;
         CalledShotBonusMultiplier = CombatConstants.HitTables.CalledShotBonusMultiplier;

         if ( Settings.FixHitDistribution ) {
            foreach ( AttackDirection direction in Enum.GetValues( typeof( AttackDirection ) ) ) {
               if ( direction == AttackDirection.None ) continue;
               if ( direction != AttackDirection.ToProne ) {
                  Dictionary<VehicleChassisLocations, int> hitTableV = Combat.HitLocation.GetVehicleHitTable( direction );
                  ScaledVehicleHitTables.Add( hitTableV, ScaleHitTable( hitTableV ) );
               }
               Dictionary<ArmorLocation, int> hitTableM = Combat.HitLocation.GetMechHitTable( direction );
               ScaledMechHitTables.Add( hitTableM, ScaleHitTable( hitTableM ) );
               if ( direction != AttackDirection.FromArtillery )
                  foreach ( ArmorLocation armor in hitTableM.Keys ) {
                     if ( hitTableM[ armor ] <= 0 ) continue;
                     Dictionary<ArmorLocation, int> hitTableC = CombatConstants.GetMechClusterTable( armor, direction );
                     ScaledMechHitTables.Add( hitTableC, ScaleHitTable( hitTableC ) );
                  }
            }
         }
      }

      public override void CombatEnds () {
         CurrentHitDirection = AttackDirection.None;
         ScaledMechHitTables?.Clear();
         ScaledVehicleHitTables?.Clear();
      }

      // ============ UTILS ============

      internal static float FixMultiplier ( ArmorLocation location, float multiplier ) {
         if ( location == None ) return 0;
         if ( MechCalledShotMultiplier != 1 )
            multiplier *= MechCalledShotMultiplier;
         if ( location == Head && CallShotClustered && ClusterChanceNeverMultiplyHead )
            return multiplier * ClusterChanceOriginalLocationMultiplier;
         return multiplier;
      }

      internal static float FixMultiplier ( VehicleChassisLocations location, float multiplier ) {
         if ( location == VehicleChassisLocations.None ) return 0;
         if ( VehicleCalledShotMultiplier != 1 )
            multiplier *= VehicleCalledShotMultiplier;
         // ClusterChanceNeverMultiplyHead does not apply to Vehicle
         return multiplier;
      }

      // ============ Called Shot ============

      public static void FixCalledShotMultiplierSquare ( AbstractActor __instance, ref float __result ) {
         if ( CalledShotBonusMultiplier == 0 ) return;
         float selfBonus = __result / CalledShotBonusMultiplier;
         if ( selfBonus == 81 ) __result = CalledShotBonusMultiplier * 9;
         else if ( selfBonus == 5.76f ) __result = CalledShotBonusMultiplier * 2.4f;
      }

      private static AttackDirection CurrentHitDirection;
      private static Dictionary<Dictionary<ArmorLocation, int>, Dictionary<ArmorLocation, int>> ScaledMechHitTables;
      private static Dictionary<Dictionary<VehicleChassisLocations, int>, Dictionary<VehicleChassisLocations, int>> ScaledVehicleHitTables;

      public static void RecordHitDirection ( AttackDirection from ) {
         CurrentHitDirection = from;
      }

      public static void PrefixMechCalledShot ( ref Dictionary<ArmorLocation, int> hitTable, ArmorLocation bonusLocation, ref float bonusLocationMultiplier ) { try {
         bonusLocationMultiplier = FixMultiplier( bonusLocation, bonusLocationMultiplier );
         if ( Settings.CalledShotUseClustering && CurrentHitDirection != AttackDirection.None ) {
            if ( bonusLocation != ArmorLocation.None )
               hitTable = CombatConstants.GetMechClusterTable( bonusLocation, CurrentHitDirection );
            CurrentHitDirection = AttackDirection.None;
         }
      }                 catch ( Exception ex ) { Error( ex ); } }

      private static int head;

      public static void ScaleMechHitTable ( ref Dictionary<ArmorLocation, int> hitTable ) { try {
         if ( ! ScaledMechHitTables.TryGetValue( hitTable, out Dictionary<ArmorLocation, int> scaled ) )
            ScaledMechHitTables.Add( hitTable, scaled = ScaleHitTable( hitTable ) );
         else if ( ! hitTable.ContainsKey( Head ) && scaled.TryGetValue( Head, out head ) )
            scaled[ Head ] = 0;
         hitTable = scaled;
      }                 catch ( Exception ex ) { Error( ex ); } }

      [ HarmonyPriority( Priority.VeryLow / 2 ) ] // After attack log's LogMechHit
      public static void RestoreHeadToScaledHitTable ( Dictionary<ArmorLocation, int> hitTable ) {
         if ( head <= 0 ) return;
         hitTable[ Head ] = head;
         head = 0;
      }

      public static void PrefixVehicleCalledShot ( VehicleChassisLocations bonusLocation, ref float bonusLocationMultiplier ) { try {
         bonusLocationMultiplier = FixMultiplier( bonusLocation, bonusLocationMultiplier );
      }                 catch ( Exception ex ) { Error( ex ); } }

      public static void ScaleVehicleHitTable ( ref Dictionary<VehicleChassisLocations, int> hitTable ) { try {
         if ( ! ScaledVehicleHitTables.TryGetValue( hitTable, out Dictionary<VehicleChassisLocations, int> scaled ) )
            ScaledVehicleHitTables.Add( hitTable, scaled = ScaleHitTable( hitTable ) );
         hitTable = scaled;
      }                 catch ( Exception ex ) { Error( ex ); } }

      public static Dictionary<T, int> ScaleHitTable <T> ( Dictionary<T, int> input ) {
         Dictionary<T, int> output = new Dictionary<T, int>( input.Count );
         foreach ( var pair in input ) output.Add( pair.Key, pair.Value * SCALE );
         return output;
      }

      // ============ GetHitLocation ============

      internal static int SumWeight<T> ( Dictionary<T, int> hitTable, T bonusLocation, float bonusLocationMultiplier, int SCALE ) {
         int totalWeight = 0;
         foreach ( int weight in hitTable.Values ) totalWeight += weight;
         totalWeight *= SCALE;
         if ( bonusLocationMultiplier != 1 && hitTable.ContainsKey( bonusLocation ) )
            totalWeight += (int)( hitTable[ bonusLocation ] * ( bonusLocationMultiplier - 1 ) * SCALE );
         return totalWeight;
      }
   }
}