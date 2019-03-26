using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using BattleTech;
using System.Reflection;
using CustomAmmoCategoriesPatches;

namespace CustomAmmoCategoriesPathes {
  public static class InternalClassPathes {
    public static void PatchInternalClasses(HarmonyInstance harmony) {
      var original = typeof(Weapon).Assembly.GetType("DoAnyMovesYieldLOFToAnyHostileNode").GetMethod("Tick", BindingFlags.NonPublic | BindingFlags.Instance);
      var transpliter = typeof(DoAnyMovesYieldLOFToAnyHostileNode_Tick).GetMethod("Transpiler");
      harmony.Patch(original, null,null, new HarmonyMethod(transpliter));
      original = typeof(Weapon).Assembly.GetType("HighestPriorityMoveCandidateIsAttackNode").GetMethod("Tick", BindingFlags.NonPublic | BindingFlags.Instance);
      transpliter = typeof(HighestPriorityMoveCandidateIsAttackNode_Tick).GetMethod("Transpiler");
      harmony.Patch(original, null, null, new HarmonyMethod(transpliter));
      original = typeof(Weapon).Assembly.GetType("MoveTowardsHighestPriorityMoveCandidateNode").GetMethod("Tick", BindingFlags.NonPublic | BindingFlags.Instance);
      transpliter = typeof(MoveTowardsHighestPriorityMoveCandidateNode_Tick).GetMethod("Transpiler");
      harmony.Patch(original, null, null, new HarmonyMethod(transpliter));
      original = typeof(Weapon).Assembly.GetType("MeleeWithHighestPriorityEnemyNode").GetMethod("Tick", BindingFlags.NonPublic | BindingFlags.Instance);
      transpliter = typeof(MeleeWithHighestPriorityEnemyNode_Tick).GetMethod("Transpiler");
      harmony.Patch(original, null, null, new HarmonyMethod(transpliter));
      var types = typeof(WeaponRealizer.Core).Assembly.GetTypes();
      foreach (var tp in types) {
        CustomAmmoCategoriesLog.Log.LogWrite("FoundType:"+tp.FullName+"\n");
      }
      var enabler = typeof(WeaponRealizer.Core).Assembly.GetType("WeaponRealizer.ClusteredShotRandomCacheEnabler");
      if (enabler == null) {
        CustomAmmoCategoriesLog.Log.LogWrite("Can't find WeaponRealizer.ClusteredShotRandomCacheEnabler in " + typeof(WeaponRealizer.Core).Assembly.FullName + "\n");
      }
      original = enabler.GetMethod("ShotsWhenFiredRandomizerOverider", BindingFlags.NonPublic | BindingFlags.Static);
      if (original == null) {
        CustomAmmoCategoriesLog.Log.LogWrite("Can't find ShotsWhenFiredRandomizerOverider in " + enabler.FullName+"\n");
      }
      //var enemyHarmony = HarmonyInstance.Create("com.joelmeador.WeaponRealizer");
      //var enemyPatchedMethods = enemyHarmony.GetPatchedMethods();
      //foreach(var ePatchedmethod in enemyPatchedMethods) {
        //CustomAmmoCategoriesLog.Log.LogWrite
      //}
      //var postfix = typeof(WRClusteredShotRandomCacheEnabler_IsClustered).GetMethod("Prefix",BindingFlags.Static);
      //harmony.Patch(original, new HarmonyMethod(postfix));
    }
  }
}
