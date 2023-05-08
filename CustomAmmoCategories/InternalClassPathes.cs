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
using System.Text;
using HarmonyLib;
using BattleTech;
using System.Reflection;
using CustomAmmoCategoriesPatches;
using CustAmmoCategories;

namespace CustomAmmoCategoriesPathes {
  public static class InternalClassPathes {
    public static void PatchInternalClasses(Harmony harmony) {
      var original = typeof(Weapon).Assembly.GetType("DoAnyMovesYieldLOFToAnyHostileNode").GetMethod("Tick", BindingFlags.NonPublic | BindingFlags.Instance);
      var transpliter = typeof(DoAnyMovesYieldLOFToAnyHostileNode_Tick).GetMethod("Transpiler");
      harmony.Patch(original, null,null, new HarmonyMethod(transpliter));
      original = typeof(Weapon).Assembly.GetType("HighestPriorityMoveCandidateIsAttackNode").GetMethod("Tick", BindingFlags.NonPublic | BindingFlags.Instance);
      transpliter = typeof(HighestPriorityMoveCandidateIsAttackNode_Tick).GetMethod("Transpiler");
      harmony.Patch(original, null, null, new HarmonyMethod(transpliter));
      //original = typeof(Weapon).Assembly.GetType("MoveTowardsHighestPriorityMoveCandidateNode").GetMethod("Tick", BindingFlags.NonPublic | BindingFlags.Instance);
      //transpliter = typeof(MoveTowardsHighestPriorityMoveCandidateNode_Tick).GetMethod("Transpiler");
      //harmony.Patch(original, null, null, new HarmonyMethod(transpliter));
      //original = typeof(Weapon).Assembly.GetType("MeleeWithHighestPriorityEnemyNode").GetMethod("Tick", BindingFlags.NonPublic | BindingFlags.Instance);
      //transpliter = typeof(MeleeWithHighestPriorityEnemyNode_Tick).GetMethod("Transpiler");
      //harmony.Patch(original, null, null, new HarmonyMethod(transpliter));
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
      //original = typeof(Weapon).Assembly.GetType("BattleTech.Rendering.Trees.QuadTreeData").GetMethod("Insert", BindingFlags.NonPublic | BindingFlags.Instance);
      //var postfix = typeof(DynamicTreesHelper).GetMethod("OnInsert");
      //harmony.Patch(original, null, new HarmonyMethod(postfix));
      //original = typeof(Weapon).Assembly.GetType("BattleTech.Rendering.Trees.QuadTreeData").GetMethod("SetupFullArray", BindingFlags.NonPublic | BindingFlags.Instance);
      //var prefix = typeof(DynamicTreesHelper).GetMethod("SetupFullArray");
      //harmony.Patch(original, new HarmonyMethod(prefix));
      //original = typeof(Weapon).Assembly.GetType("BattleTech.Rendering.Trees.QuadTreeData").GetMethod("SetupComputeBuffer", BindingFlags.NonPublic | BindingFlags.Instance);
      //prefix = typeof(DynamicTreesHelper).GetMethod("SetupComputeBuffer");
      //harmony.Patch(original, new HarmonyMethod(prefix));
      //original = typeof(Weapon).Assembly.GetType("BattleTech.Rendering.Trees.QuadTreeData").GetMethod("GenerateCombinedMesh", BindingFlags.NonPublic | BindingFlags.Instance);
      //prefix = typeof(DynamicTreesHelper).GetMethod("GenerateCombinedMesh");
      //harmony.Patch(original, new HarmonyMethod(prefix));
      //original = typeof(Weapon).Assembly.GetType("BattleTech.Rendering.Trees.QuadTreeData").GetMethod("GenerateMesh", BindingFlags.NonPublic | BindingFlags.Instance);
      //prefix = typeof(DynamicTreesHelper).GetMethod("GenerateMesh");
      //harmony.Patch(original, new HarmonyMethod(prefix));

      original = typeof(Weapon).Assembly.GetType("BraceNode").GetMethod("Tick", BindingFlags.NonPublic | BindingFlags.Instance);
      var prefix = typeof(BraceNode_Tick).GetMethod("Prefix");
      harmony.Patch(original, new HarmonyMethod(prefix));
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
