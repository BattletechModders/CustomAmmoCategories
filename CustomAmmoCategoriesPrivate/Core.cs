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
using CustAmmoCategories;
using CustomAmmoCategoriesLog;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CustomAmmoCategoriesPrivate{
  //[HarmonyPatch(typeof(AITeam))]
  //[HarmonyPatch("think")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] {  })]
  //public static class AITeam_think {
  //  public static bool Prefix(AITeam __instance) {
  //    try {
  //      if (!__instance.ThinksOnThisMachine) { return false; }
  //      __instance.CheckWaitingForNotificationCompletionTimeout();
  //      if (__instance.currentUnit != null && __instance.currentUnit.HasActivatedThisRound) {
  //        __instance.isComplete = true;
  //        AITeam.activationLogger.LogDebug((object)"[think] AI Team isComplete (HasActivatedThisRound)");
  //      } else if (__instance.pendingInvocations != null && __instance.pendingInvocations.Count != 0) {
  //        if (__instance.WaitingForNotificationCompletion) { return false; }
  //        for (int index = 0; index < __instance.pendingInvocations.Count; ++index) {
  //          InvocationMessage pendingInvocation = __instance.pendingInvocations[index];
  //          if (pendingInvocation is ReserveActorInvocation) {
  //            __instance.isComplete = true;
  //            AITeam.activationLogger.LogDebug((object)"[think] AI Team isComplete (ReserveActorInvocation)");
  //          }
  //          __instance.Combat.MessageCenter.PublishMessage((MessageCenterMessage)pendingInvocation);
  //        }
  //        __instance.pendingInvocations.Clear();
  //      } else {
  //        InvocationMessage invocationForCurrentUnit = __instance.getInvocationForCurrentUnit();
  //        if (invocationForCurrentUnit != null) {
  //          if (__instance.WaitingForNotificationCompletion) {
  //            __instance.pendingInvocations.Add(invocationForCurrentUnit);
  //          } else {
  //            if (invocationForCurrentUnit is ReserveActorInvocation) {
  //              __instance.isComplete = true;
  //              AITeam.activationLogger.LogDebug((object)"[think] AI Team isComplete (ReserveActorInvocation)");
  //            }
  //            AIUtil.LogAI("AI sending reserve invocation with current phase: " + (object)__instance.Combat.TurnDirector.CurrentPhase, "AI.TurnOrder");
  //            __instance.Combat.MessageCenter.PublishMessage((MessageCenterMessage)invocationForCurrentUnit);
  //          }
  //        }
  //        AIUtil.LogAI(string.Format("AI thinking done after {0} seconds", (object)(__instance.Combat.BattleTechGame.Time - __instance.planningStartTime)));
  //        if (__instance.currentUnit != null) {
  //          Log.P?.TWL(0, "AI " + (__instance.currentUnit.PilotableActorDef==null?"unknown": __instance.currentUnit.PilotableActorDef.ChassisID) + " thinking done " + (__instance.Combat.BattleTechGame.Time - __instance.planningStartTime) + " s. movementCandidateLocations:" + __instance.currentUnit.BehaviorTree.movementCandidateLocations.Count, true);
  //        }
  //      }
  //    } catch (Exception e) {
  //      Log.P?.TWL(0, e.ToString());
  //    }
  //    return false;
  //  }
  //}
  //[HarmonyPatch(typeof(MoveDestination))]
  //[HarmonyPatch(MethodType.Constructor)]
  //[HarmonyPatch(new Type[] { typeof(PathNode), typeof(MoveType) })]
  //public static class MoveDestination_Contstructor {
  //  public static void Postfix(MoveDestination __instance, PathNode pathNode, MoveType moveType) {
  //    try {
  //      Log.P?.TWL(0, "MoveDestination " + moveType);
  //      Log.P?.WL(0, Environment.StackTrace, true);
  //    } catch (Exception e) {
  //      Log.P?.TWL(0, e.ToString());
  //    }
  //  }
  //}

  public static class Core {
    public static Harmony harmony { get; set; } = null;
    internal static string GetAssemblyName(this MethodBase Method) {
      Type declaringType = Method.DeclaringType;
      return (object)declaringType == null ? (string)null : declaringType.Assembly.GetName().Name;
    }
    internal static string GetFullName(this MethodBase Method) {
      Type declaringType = Method.DeclaringType;
      return ((object)declaringType != null ? declaringType.FullName : (string)null) + "." + Method.Name;
    }
    public static FieldInfo TempModTekDirectory { get; set; } = null;
    internal static void WritePatches(this StreamWriter writer, string title, IList<Patch> patches, bool checkSkip = false) {
      if (((ICollection<Patch>)patches).Count != 0)
        writer.WriteLine("\t" + title + ":");
      using (IEnumerator<Patch> enumerator = ((IEnumerable<Patch>)((IEnumerable<Patch>)patches).OrderBy<Patch, Patch>((Func<Patch, Patch>)(x => x))).GetEnumerator()) {
        while (((IEnumerator)enumerator).MoveNext()) {
          Patch current = enumerator.Current;
          string assemblyName = (current.PatchMethod).GetAssemblyName();
          string fullName = (current.PatchMethod).GetFullName();
          writer.WriteLine("\t\t" + assemblyName + " (" + (string)current.owner + ") " + fullName);
        }
      }
    }
    public static void PrintHarmonySummary_postfix() {
      try {
        if (TempModTekDirectory == null) { return; }
        string tempModTekDirectory = TempModTekDirectory.GetValue(null) as string;
        Log.P?.TWL(0, $"ModTek.Features.Manifest.Mods.ModDefsDatabase.FinishedLoadingMods path:{tempModTekDirectory}", true);
        if (string.IsNullOrEmpty(tempModTekDirectory)) { return; }
        string harmonySummaryPath = Path.Combine(tempModTekDirectory, "harmony_summary_v2.log");
        using (StreamWriter writer = File.CreateText(harmonySummaryPath)) {
          writer.WriteLine(string.Format("Harmony2 Patched Methods (after ModTek startup) -- {0}", (object)DateTime.Now));
          writer.WriteLine("!!!NOTE CptMoore: it is not correct all the time, its dangerous and unhelpful to use");
          writer.WriteLine("you had been warned");
          writer.WriteLine();
          writer.WriteLine("Format as follows:");
          writer.WriteLine("{assembly name of patched method}");
          writer.WriteLine("{name of patched method}");
          writer.WriteLine("\t{Harmony patch type}");
          writer.WriteLine("\t\t{assembly name of patch} ({Harmony id of patch}) {method name of patch}");
          writer.WriteLine();
          writer.WriteLine();
          foreach (MethodBase Method in (IEnumerable<MethodBase>)Harmony.GetAllPatchedMethods().Where<MethodBase>((Func<MethodBase, bool>)(m => m.DeclaringType != (Type)null)).OrderBy<MethodBase, string>((Func<MethodBase, string>)(m => m.DeclaringType.Assembly.GetName().Name)).ThenBy<MethodBase, string>((Func<MethodBase, string>)(m => m.DeclaringType.FullName)).ThenBy<MethodBase, string>((Func<MethodBase, string>)(m => m.Name))) {
            var patchInfo = Harmony.GetPatchInfo(Method);
            if (patchInfo != null) {
              writer.WriteLine(Method.GetAssemblyName());
              writer.WriteLine(Method.GetFullName() + ":");
              writer.WritePatches("Prefixes", (IList<Patch>)patchInfo.Prefixes, true);
              writer.WritePatches("Transpilers", (IList<Patch>)patchInfo.Transpilers, false);
              writer.WritePatches("Postfixes", (IList<Patch>)patchInfo.Postfixes, false);
              writer.WriteLine();
            }
          }
        }
      } catch (Exception e) {
        Log.P?.WL(0, e.ToString(), true);
      }
    }
    public static void Init() {
      Core.harmony = new Harmony("io.mission.customammocategories.private");
      Log.P?.TWL(0, "Initing " + Assembly.GetExecutingAssembly().GetName(), true);
      try {

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
          if (assembly.FullName.StartsWith("ModTek.Common, Version=")) {
            var FilePaths = assembly.GetType("ModTek.Common.Globals.CommonPaths");
            if (FilePaths != null) {
              Log.P?.WL(1, $"ModTek.Misc.FilePaths found");
              TempModTekDirectory = AccessTools.Field(FilePaths, "DotModTekDirectory");
            }
            if (TempModTekDirectory != null) {
              Log.P?.WL(1, $"ModTek.Common.Globals.CommonPaths.DotModTekDirectory {TempModTekDirectory}", true);
            } else {
              Log.P?.WL(1, $"ModTek.Common.Globals.CommonPaths.DotModTekDirectory not found", true);
              continue;
            }
          }
          if (assembly.FullName.StartsWith("ModTek, Version=")) { 
            var ModDefsDatabase = assembly.GetType("ModTek.Features.Manifest.Mods.ModDefsDatabase");
            if (ModDefsDatabase != null) {
              Log.P?.WL(1, $"ModTek.Features.Manifest.Mods.ModDefsDatabase found",true);
              var PrintHarmonySummary_original = AccessTools.Method(ModDefsDatabase, "FinishedLoadingMods");
              if(PrintHarmonySummary_original != null) {
                Log.P?.WL(1, $"ModTek.Features.Manifest.Mods.ModDefsDatabase.FinishedLoadingMods found",true);
                harmony.Patch(PrintHarmonySummary_original, null, new HarmonyMethod(AccessTools.Method(typeof(Core), nameof(PrintHarmonySummary_postfix))), null, null, null);
              }
            } else {
              Log.P?.WL(1, $"ModTek.Features.Manifest.Mods.ModDefsDatabase not found",true);
            }
            break;
          }
        }
      } catch (Exception e) {
        Log.P?.WL(0, e.ToString(), true);
      }
    }
    public static void FinishedLoading() {
      Log.P?.TWL(0, "FinishedLoading " + Assembly.GetExecutingAssembly().GetName(), true);
      try {
        harmony.PatchAll(Assembly.GetExecutingAssembly());
        Core.harmony.Patch(AIPathingLimiter.BehaviorTreeUpdate(), new HarmonyMethod(AIPathingLimiter.BehaviorTreeUpdatePrefix()), new HarmonyMethod(AIPathingLimiter.BehaviorTreeUpdatePostfix()));
        Core.harmony.Patch(AIPathingLimiter.GenerateMoveCandidatesNode(), null, new HarmonyMethod(AIPathingLimiter.PostfixFilter()));
        Core.harmony.Patch(AIPathingLimiter.GenerateSprintMoveCandidatesNode(), null, new HarmonyMethod(AIPathingLimiter.PostfixFilter()));
        Core.harmony.Patch(AIPathingLimiter.GenerateForwardMoveCandidatesNode(), null, new HarmonyMethod(AIPathingLimiter.PostfixFilter()));
        Core.harmony.Patch(AIPathingLimiter.GenerateReverseMoveCandidatesNode(), null, new HarmonyMethod(AIPathingLimiter.PostfixFilter()));
        Core.harmony.Patch(AIPathingLimiter.GenerateJumpMoveCandidatesNode(), null, new HarmonyMethod(AIPathingLimiter.PostfixFilter()));
        //Core.harmony.Patch(AIPathingLimiter.GenerateMoveCandidatesNode(), new HarmonyMethod(AIPathingLimiter.PrefixMethod()), new HarmonyMethod(AIPathingLimiter.PostfixMethod()));
        //Core.harmony.Patch(AIPathingLimiter.GenerateSprintMoveCandidatesNode(), new HarmonyMethod(AIPathingLimiter.PrefixMethod()), new HarmonyMethod(AIPathingLimiter.PostfixMethod()));
        //Core.harmony.Patch(AIPathingLimiter.GenerateForwardMoveCandidatesNode(), new HarmonyMethod(AIPathingLimiter.PrefixMethod()), new HarmonyMethod(AIPathingLimiter.PostfixMethod()));
        //Core.harmony.Patch(AIPathingLimiter.GenerateReverseMoveCandidatesNode(), new HarmonyMethod(AIPathingLimiter.PrefixMethod()), new HarmonyMethod(AIPathingLimiter.PostfixMethod()));
        //Core.harmony.Patch(AIPathingLimiter.GenerateJumpMoveCandidatesNode(), new HarmonyMethod(AIPathingLimiter.PrefixMethod()), new HarmonyMethod(AIPathingLimiter.PostfixMethod()));
        Core.harmony.Patch(AIPathingLimiter.HasDirectLOFToAnyHostileFromReachableLocationsNode(), new HarmonyMethod(AIPathingLimiter.PrefixMethod()), new HarmonyMethod(AIPathingLimiter.PostfixMethod()));
        Core.harmony.Patch(AIPathingLimiter.HasLOFToAnyHostileFromReachableLocationsNode(), new HarmonyMethod(AIPathingLimiter.PrefixMethod()), new HarmonyMethod(AIPathingLimiter.PostfixMethod()));
        Core.harmony.Patch(AIPathingLimiter.CalcHighFidelityMaxExpectedDamageToHostile(), new HarmonyMethod(AIPathingLimiter.PrefixMethod()), new HarmonyMethod(AIPathingLimiter.PostfixMethod()));
        Core.harmony.Patch(AIPathingLimiter.GetSampledPathNodes(), new HarmonyMethod(AIPathingLimiter.GetSampledPathNodesPrefix()), new HarmonyMethod(AIPathingLimiter.GetSampledPathNodesPostfix()));
      }catch(Exception e) {
        Log.P?.TWL(0, e.ToString(), true);
      }
    }
  }
}
