using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
  }
}
