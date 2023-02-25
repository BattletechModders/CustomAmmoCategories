using BattleTech;
using BattleTech.UI;
using CustomLog;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CustomProfiler {
  /*[HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("StealthPipsPreviewFromActorMovement")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(Vector3) })]
  public static class AbstractActor_StealthPipsPreviewFromActorMovement {
    private static int CacheTurn = -1;
    private static int CachePhase = -1;
    private static Dictionary<AbstractActor, Dictionary<AbstractActor, Dictionary<Vector3, int>>> stealthCache = new Dictionary<AbstractActor, Dictionary<AbstractActor, Dictionary<Vector3, int>>>();
    public static void ClearCache(int turn,int phase) {
      stealthCache = new Dictionary<AbstractActor, Dictionary<AbstractActor, Dictionary<Vector3, int>>>();
      CacheTurn = turn;
      CachePhase = phase;
    }
    public static bool isInCache(this AbstractActor instance, AbstractActor movingActor, Vector3 previewPos) {
      if (stealthCache.ContainsKey(instance) == false) { return false; }
      if (stealthCache[instance].ContainsKey(movingActor) == false) { return false; }
      if (stealthCache[instance][movingActor].ContainsKey(previewPos) == false) { return false; }
      return true;
    }
    public static int getFromCache(this AbstractActor instance, AbstractActor movingActor, Vector3 previewPos) {
      return stealthCache[instance][movingActor][previewPos];
    }
    public static void setToCache(this AbstractActor instance, AbstractActor movingActor, Vector3 previewPos,int result) {
      if (stealthCache.ContainsKey(instance) == false) { stealthCache.Add(instance, new Dictionary<AbstractActor, Dictionary<Vector3, int>>()); };
      if (stealthCache[instance].ContainsKey(movingActor) == false) { stealthCache[instance].Add(movingActor, new Dictionary<Vector3, int>()); };
      if (stealthCache[instance][movingActor].ContainsKey(previewPos) == false) { stealthCache[instance][movingActor].Add(previewPos, result); } else {
        stealthCache[instance][movingActor][previewPos] = result;
      }
    }
    public static bool Prefix(AbstractActor __instance, AbstractActor movingActor, Vector3 previewPos,ref int __result) {
      if((CacheTurn != __instance.Combat.TurnDirector.CurrentRound)||(CachePhase != __instance.Combat.TurnDirector.CurrentPhase)) {
        ClearCache(__instance.Combat.TurnDirector.CurrentRound, __instance.Combat.TurnDirector.CurrentPhase);
      }
      if (__instance.isInCache(movingActor, previewPos)) {
        __result = __instance.getFromCache(movingActor, previewPos);
        return false;
      }
      return true;
    }
    public static void Postfix(AbstractActor __instance, AbstractActor movingActor, Vector3 previewPos, ref int __result) {
      __instance.setToCache(movingActor,previewPos,__result);
    }
  }*/
  [HarmonyPatch(typeof(SelectionStateMove))]
  [HarmonyPatch("ProcessMousePos")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Vector3) })]
  [HarmonyPriority(Priority.First)]
  public static class SelectionStateMove_ProcessMousePos {
    private static Action<SelectionStateMove, Vector3> SelectionStateMoveBase_ProcessMousePos = null;
    private static MethodInfo mRecalcPossibleTargets;
    private static int ProfilerCounter = 0;
    //public static int counter = 0;
    private static Dictionary<string, TimeSpan> profileTime = new Dictionary<string, TimeSpan>();
    private static Stopwatch TrottleSW = new Stopwatch();
    private static Vector3 prevPreviewPos = Vector3.zero;
    private static Vector3 prevWordPos = Vector3.zero;
    private static Stopwatch overall = new Stopwatch();
    public static bool Prepare() {
      var method = typeof(SelectionStateMoveBase).GetMethod("ProcessMousePos", AccessTools.all);
      var dm = new DynamicMethod("CACProcessMousePos", null, new Type[] { typeof(SelectionStateMove), typeof(Vector3) }, typeof(SelectionStateMove));
      var gen = dm.GetILGenerator();
      gen.Emit(OpCodes.Ldarg_0);
      gen.Emit(OpCodes.Ldarg_1);
      gen.Emit(OpCodes.Call, method);
      gen.Emit(OpCodes.Ret);
      SelectionStateMoveBase_ProcessMousePos = (Action<SelectionStateMove, Vector3>)dm.CreateDelegate(typeof(Action<SelectionStateMove, Vector3>));
      mRecalcPossibleTargets = typeof(SelectionStateMove).GetMethod("RecalcPossibleTargets", BindingFlags.Instance | BindingFlags.NonPublic);
      profileTime.Clear();
      profileTime.Add("Overall", TimeSpan.Zero);
      profileTime.Add("RecalcPossibleTargets", TimeSpan.Zero);
      profileTime.Add("SelectionStateMoveBase.ProcessMousePos", TimeSpan.Zero);
      profileTime.Add("Pathing.SelectMeleeDest", TimeSpan.Zero);
      profileTime.Add("Pathing.Update", TimeSpan.Zero);
      profileTime.Add("FiringPreview.Recalc", TimeSpan.Zero);
      profileTime.Add("Instance.UpdateReticle", TimeSpan.Zero);
      return true;
    }
    public static void RecalcPossibleTargets(this SelectionStateMove ss) {
      mRecalcPossibleTargets.Invoke(ss, null);
    }
    public static bool Prefix(SelectionStateMove __instance, Vector3 worldPos, bool ___hasFinishedPathing) {
      /*if (CustomAmmoCategories.Settings.ProcessMousePosTrottle <= 0) { return true; }
      try {
        if (__instance.HasDestination) {
          if (timer.IsRunning) {
            timer.Stop();
            timer.Reset();
            return true;
          }
          return true;
        }
        if (timer.IsRunning == false) {
          timer.Restart();
          return true;
        }
        timer.Stop();
        if (timer.ElapsedMilliseconds < CustomAmmoCategories.Settings.ProcessMousePosTrottle) {
          timer.Start();
          return false;
        }
        timer.Restart();
        return true;
      } catch (Exception e) {
        Log.M.TWL(0,e.ToString());
      }*/
      if (overall.IsRunning == false) { overall.Restart(); };
      Stopwatch stopwatch = new Stopwatch();
      stopwatch.Start();
      __instance.RecalcPossibleTargets();
      TimeSpan tmr = TimeSpan.Zero;
      stopwatch.Stop(); profileTime["RecalcPossibleTargets"] += (stopwatch.Elapsed - tmr); tmr = stopwatch.Elapsed; stopwatch.Start();
      //if(CustomAmmoCategories.Settings.SelectionStateMoveBaseProcessMousePosTrottle == 0) {
      //}else if ((counter % CustomAmmoCategories.Settings.SelectionStateMoveBaseProcessMousePosTrottle) == 0) {
      //SelectionStateMoveBase_ProcessMousePos(__instance, worldPos);
      //}
      //base.ProcessMousePos(worldPos);
      if (__instance.Orders != null) { return false; };
      if (__instance.HasTarget && !__instance.HasDestination) {
        __instance.SelectedActor.Pathing.SelectMeleeDest(worldPos);
        stopwatch.Stop(); profileTime["Pathing.SelectMeleeDest"] += (stopwatch.Elapsed - tmr); tmr = stopwatch.Elapsed; stopwatch.Start();
        __instance.SelectedActor.Pathing.Update(worldPos, true);
        stopwatch.Stop(); profileTime["Pathing.Update"] += (stopwatch.Elapsed - tmr); tmr = stopwatch.Elapsed; stopwatch.Start();
      } else if (__instance.PotentialMeleeTarget == null) {
        __instance.SelectedActor.Pathing.Update(worldPos, true);
        stopwatch.Stop(); profileTime["Pathing.Update"] += (stopwatch.Elapsed - tmr); tmr = stopwatch.Elapsed; stopwatch.Start();
      }
      bool trottle = false;
      //if (CustomAmmoCategories.Settings.FiringPreviewRecalcTrottle == 0) {
      //trottle = false;
      prevWordPos = worldPos;
      /*} else {
        if (prevPreviewPos == __instance.PreviewPos) {
          if (TrottleSW.ElapsedMilliseconds > CustomAmmoCategories.Settings.FiringPreviewRecalcTrottle) {
            trottle = false;
            if (TrottleSW.IsRunning) { prevWordPos = worldPos; }
            TrottleSW.Stop();
          } else {
            if (TrottleSW.IsRunning == false) { TrottleSW.Restart(); };
          }
        } else {
          TrottleSW.Stop();
          TrottleSW.Reset();
          prevPreviewPos = __instance.PreviewPos;
        }
      }*/
      if (trottle == false) SelectionStateMoveBase_ProcessMousePos(__instance, prevWordPos);
      stopwatch.Stop(); profileTime["SelectionStateMoveBase.ProcessMousePos"] += (stopwatch.Elapsed - tmr); tmr = stopwatch.Elapsed; stopwatch.Start();
      if (___hasFinishedPathing) {
        if (trottle == false) __instance.FiringPreview.Recalc(__instance.SelectedActor, __instance.PreviewPos, __instance.PreviewRot, true, false);
        stopwatch.Stop(); profileTime["FiringPreview.Recalc"] += (stopwatch.Elapsed - tmr); tmr = stopwatch.Elapsed; stopwatch.Start();
      }
      //if(CustomAmmoCategories.Settings.UpdateReticleTrottle == 0) {
      //if (Vector3.Distance(worldPos, prevWordPos) > 30.0f) { prevWordPos = worldPos; };
      if (trottle == false) CombatMovementReticle.Instance.UpdateReticle(__instance.SelectedActor, prevWordPos, false, __instance.PotentialMeleeTarget != null, __instance.HasTarget);
      //}else if((counter % CustomAmmoCategories.Settings.UpdateReticleTrottle) == 0){
      //CombatMovementReticle.Instance.UpdateReticle(__instance.SelectedActor, worldPos, false, __instance.PotentialMeleeTarget != null, __instance.HasTarget);
      //}
      //counter = (counter + 1) % 1000;
      stopwatch.Stop(); profileTime["Instance.UpdateReticle"] += (stopwatch.Elapsed - tmr); tmr = stopwatch.Elapsed;
      profileTime["Overall"] += stopwatch.Elapsed;
      ProfilerCounter += 1;
      if (ProfilerCounter > 100) {
        overall.Stop();
        Log.P.TWL(0, "SelectionStateMove.ProcessMousePos:(" + overall.Elapsed.TotalMilliseconds + ")" + profileTime["Overall"].TotalMilliseconds);
        Log.P.WL(1, "RecalcPossibleTargets:" + profileTime["RecalcPossibleTargets"].TotalMilliseconds);
        Log.P.WL(1, "Pathing.SelectMeleeDest:" + profileTime["Pathing.SelectMeleeDest"].TotalMilliseconds);
        Log.P.WL(1, "Pathing.Update:" + profileTime["Pathing.Update"].TotalMilliseconds);
        Log.P.WL(1, "SelectionStateMoveBase.ProcessMousePos:" + profileTime["SelectionStateMoveBase.ProcessMousePos"].TotalMilliseconds);
        Log.P.WL(1, "FiringPreview.Recalc:" + profileTime["FiringPreview.Recalc"].TotalMilliseconds);
        Log.P.WL(1, "Instance.UpdateReticle:" + profileTime["Instance.UpdateReticle"].TotalMilliseconds, true);
        profileTime["Overall"] = TimeSpan.Zero;
        profileTime["RecalcPossibleTargets"] = TimeSpan.Zero;
        profileTime["SelectionStateMoveBase.ProcessMousePos"] = TimeSpan.Zero;
        profileTime["Pathing.SelectMeleeDest"] = TimeSpan.Zero;
        profileTime["Pathing.Update"] = TimeSpan.Zero;
        profileTime["FiringPreview.Recalc"] = TimeSpan.Zero;
        profileTime["Instance.UpdateReticle"] = TimeSpan.Zero;
        overall.Restart();
        ProfilerCounter = 0;
      }
      return false;
    }
  }

  [HarmonyPatch(typeof(FiringPreviewManager))]
  [HarmonyPatch("Recalc")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(Vector3), typeof(Quaternion), typeof(bool), typeof(bool) })]
  [HarmonyPriority(Priority.First)]
  public static class FiringPreviewManager_Recalc {
    private static int ProfilerCounter = 0;
    //public static int counter = 0;
    private static Dictionary<string, TimeSpan> profileTime = new Dictionary<string, TimeSpan>();
    private static Stopwatch overall = new Stopwatch();
    private static MethodInfo mRecalc;
    private static MethodInfo mHasLOS;
    private static MethodInfo mCanRotateToFace;
    private static FieldInfo fCombat;
    private static FieldInfo fFireInfo;
    private static int tabTargets = 0;
    public static bool Prepare() {
      mRecalc = typeof(FiringPreviewManager).GetMethod("Recalc", BindingFlags.Instance | BindingFlags.NonPublic);
      mHasLOS = typeof(FiringPreviewManager).GetMethod("HasLOS", BindingFlags.Instance | BindingFlags.NonPublic);
      mCanRotateToFace = typeof(FiringPreviewManager).GetMethod("CanRotateToFace", BindingFlags.Instance | BindingFlags.NonPublic);
      fCombat = typeof(FiringPreviewManager).GetField("combat", BindingFlags.Instance | BindingFlags.NonPublic);
      fFireInfo = typeof(FiringPreviewManager).GetField("fireInfo", BindingFlags.Instance | BindingFlags.NonPublic);
      profileTime.Clear();
      profileTime.Add("Overall", TimeSpan.Zero);
      profileTime.Add("ClearAllInfo", TimeSpan.Zero);
      profileTime.Add("GetAllTabTargets", TimeSpan.Zero);
      profileTime.Add("GetAllAlliesOf", TimeSpan.Zero);
      profileTime.Add("GetLongestRangeWeapon", TimeSpan.Zero);
      profileTime.Add("Recalc", TimeSpan.Zero);
      profileTime.Add(" VisibilityToTargetUnit", TimeSpan.Zero);
      profileTime.Add(" HasLOS", TimeSpan.Zero);
      profileTime.Add(" GetLineOfFire", TimeSpan.Zero);
      profileTime.Add(" IsInFiringArc", TimeSpan.Zero);
      profileTime.Add(" CanRotateToFace", TimeSpan.Zero);
      profileTime.Add(" fireInfo.Add", TimeSpan.Zero);
      return true;
    }
    public static bool HasLOS(this FiringPreviewManager instance, AbstractActor attacker, ICombatant target, Vector3 position, List<AbstractActor> allies) {
      return (bool)mHasLOS.Invoke(instance, new object[] { attacker, target, position, allies });
    }
    public static bool CanRotateToFace(this FiringPreviewManager instance, AbstractActor attacker, ICombatant target, Vector3 position, Quaternion rotation, bool isJump) {
      return (bool)mCanRotateToFace.Invoke(instance, new object[] { attacker, target, position, rotation, isJump });
    }
    public static CombatGameState combat(this FiringPreviewManager instance) {
      return (CombatGameState)fCombat.GetValue(instance);
    }
    public static Dictionary<ICombatant, FiringPreviewManager.PreviewInfo> fireInfo(this FiringPreviewManager instance) {
      return (Dictionary<ICombatant, FiringPreviewManager.PreviewInfo>)fFireInfo.GetValue(instance);
    }
    public static void Recalc(this FiringPreviewManager instance, AbstractActor attacker, ICombatant target, List<AbstractActor> allies, Vector3 position, Quaternion rotation, bool canRotate, bool isJump, float maxRange, float maxIndirectRange) {
      Stopwatch stopwatch = new Stopwatch();
      TimeSpan tmr = TimeSpan.Zero;
      stopwatch.Start();
      if (target.IsDead || attacker.VisibilityToTargetUnit(target) < VisibilityLevel.Blip0Minimum) {
        stopwatch.Stop(); profileTime[" VisibilityToTargetUnit"] += (stopwatch.Elapsed - tmr); tmr = stopwatch.Elapsed;
        return;
      }
      stopwatch.Stop(); profileTime[" VisibilityToTargetUnit"] += (stopwatch.Elapsed - tmr); tmr = stopwatch.Elapsed; stopwatch.Start();
      float num1 = Vector3.Distance(position, target.CurrentPosition);
      if (!instance.HasLOS(attacker, target, position, allies)) {
        stopwatch.Stop(); profileTime[" HasLOS"] += (stopwatch.Elapsed - tmr); tmr = stopwatch.Elapsed;
        return;
      }
      stopwatch.Stop(); profileTime[" HasLOS"] += (stopwatch.Elapsed - tmr); tmr = stopwatch.Elapsed; stopwatch.Start();
      Vector3 collisionWorldPos;
      LineOfFireLevel lineOfFire = instance.combat().LOS.GetLineOfFire(attacker, position, target, target.CurrentPosition, target.CurrentRotation, out collisionWorldPos);
      stopwatch.Stop(); profileTime[" GetLineOfFire"] += (stopwatch.Elapsed - tmr); tmr = stopwatch.Elapsed; stopwatch.Start();
      int num2 = lineOfFire > LineOfFireLevel.LOFBlocked ? 1 : 0;
      bool flag1 = attacker.IsInFiringArc(target, position, rotation);
      stopwatch.Stop(); profileTime[" IsInFiringArc"] += (stopwatch.Elapsed - tmr); tmr = stopwatch.Elapsed; stopwatch.Start();
      bool flag2 = canRotate && instance.CanRotateToFace(attacker, target, position, rotation, isJump);
      stopwatch.Stop(); profileTime[" CanRotateToFace"] += (stopwatch.Elapsed - tmr); tmr = stopwatch.Elapsed; stopwatch.Start();
      if (num2 != 0) {
        if ((double)num1 < (double)maxRange) {
          if (flag1) {
            instance.fireInfo().Add(target, new FiringPreviewManager.PreviewInfo(FiringPreviewManager.TargetAvailability.PossibleDirect, lineOfFire, collisionWorldPos));
          } else if (flag2) {
            instance.fireInfo().Add(target, new FiringPreviewManager.PreviewInfo(FiringPreviewManager.TargetAvailability.NeedRotationDirect, lineOfFire, collisionWorldPos));
          } else {
            instance.fireInfo().Add(target, new FiringPreviewManager.PreviewInfo(FiringPreviewManager.TargetAvailability.BeyondRotation, lineOfFire, collisionWorldPos));
          }
        } else {
          instance.fireInfo().Add(target, new FiringPreviewManager.PreviewInfo(FiringPreviewManager.TargetAvailability.BeyondMaxRange, lineOfFire, collisionWorldPos));
        }
        stopwatch.Stop(); profileTime[" fireInfo.Add"] += (stopwatch.Elapsed - tmr); tmr = stopwatch.Elapsed; stopwatch.Start();
      } else {
        bool flag3 = false;
        AbstractActor abstractActor = target as AbstractActor;
        if (abstractActor != null) {
          flag3 = abstractActor.HasIndirectFireImmunity;
        }
        if (((double)num1 >= (double)maxIndirectRange ? 0 : (!flag3 ? 1 : 0)) == 0) {
          return;
        }
        if (flag1) {
          instance.fireInfo().Add(target, new FiringPreviewManager.PreviewInfo(FiringPreviewManager.TargetAvailability.PossibleIndirect, lineOfFire, collisionWorldPos));
        } else {
          if (!flag2) {
            return;
          }
          instance.fireInfo().Add(target, new FiringPreviewManager.PreviewInfo(FiringPreviewManager.TargetAvailability.NeedRotationIndirect, lineOfFire, collisionWorldPos));
        }
      }
    }
    public static bool Prefix(FiringPreviewManager __instance, AbstractActor attacker, Vector3 position, Quaternion rotation, bool canRotate, bool isJump, CombatGameState ___combat) {
      if (overall.IsRunning == false) { overall.Restart(); };
      Stopwatch stopwatch = new Stopwatch();
      TimeSpan tmr = TimeSpan.Zero;
      stopwatch.Start();
      __instance.ClearAllInfo();
      stopwatch.Stop(); profileTime["ClearAllInfo"] += (stopwatch.Elapsed - tmr); tmr = stopwatch.Elapsed; stopwatch.Start();
      List<ICombatant> allTabTargets = ___combat.GetAllTabTargets(attacker);
      stopwatch.Stop(); profileTime["GetAllTabTargets"] += (stopwatch.Elapsed - tmr); tmr = stopwatch.Elapsed; stopwatch.Start();
      List<AbstractActor> allAlliesOf = ___combat.GetAllAlliesOf(attacker);
      stopwatch.Stop(); profileTime["GetAllAlliesOf"] += (stopwatch.Elapsed - tmr); tmr = stopwatch.Elapsed; stopwatch.Start();
      Weapon longestRangeWeapon1 = attacker.GetLongestRangeWeapon(false, false);
      stopwatch.Stop(); profileTime["GetLongestRangeWeapon"] += (stopwatch.Elapsed - tmr); tmr = stopwatch.Elapsed; stopwatch.Start();
      float maxRange = longestRangeWeapon1 == null ? 0.0f : longestRangeWeapon1.MaxRange;
      Weapon longestRangeWeapon2 = attacker.GetLongestRangeWeapon(false, true);
      stopwatch.Stop(); profileTime["GetLongestRangeWeapon"] += (stopwatch.Elapsed - tmr); tmr = stopwatch.Elapsed; stopwatch.Start();
      float maxIndirectRange = longestRangeWeapon2 == null ? 0.0f : longestRangeWeapon2.MaxRange;
      for (int index = 0; index < allTabTargets.Count; ++index) {
        __instance.Recalc(attacker, allTabTargets[index], allAlliesOf, position, rotation, canRotate, isJump, maxRange, maxIndirectRange);
      }
      stopwatch.Stop(); profileTime["Recalc"] += (stopwatch.Elapsed - tmr); tmr = stopwatch.Elapsed; stopwatch.Start();
      profileTime["Overall"] += stopwatch.Elapsed;
      ProfilerCounter += 1;
      tabTargets += allTabTargets.Count;
      if (ProfilerCounter > 100) {
        overall.Stop();
        Log.P.TWL(0, "FiringPreview.Recalc:(" + overall.Elapsed.TotalMilliseconds + ")" + profileTime["Overall"].TotalMilliseconds);
        Log.P.WL(1, "ClearAllInfo:" + profileTime["ClearAllInfo"].TotalMilliseconds);
        Log.P.WL(1, "GetAllTabTargets:" + profileTime["GetAllTabTargets"].TotalMilliseconds);
        Log.P.WL(1, "GetAllAlliesOf:" + profileTime["GetAllAlliesOf"].TotalMilliseconds);
        Log.P.WL(1, "GetLongestRangeWeapons:" + profileTime["GetLongestRangeWeapon"].TotalMilliseconds);
        Log.P.WL(1, "Recalc:" + profileTime["Recalc"].TotalMilliseconds);
        Log.P.WL(1, " VisibilityToTargetUnit:" + profileTime[" VisibilityToTargetUnit"].TotalMilliseconds);
        Log.P.WL(1, " HasLOS:" + profileTime[" HasLOS"].TotalMilliseconds);
        Log.P.WL(1, " GetLineOfFire:" + profileTime[" GetLineOfFire"].TotalMilliseconds);
        Log.P.WL(1, " IsInFiringArc:" + profileTime[" IsInFiringArc"].TotalMilliseconds);
        Log.P.WL(1, " CanRotateToFace:" + profileTime[" CanRotateToFace"].TotalMilliseconds);
        Log.P.WL(1, " fireInfo.Add:" + profileTime[" fireInfo.Add"].TotalMilliseconds);
        Log.P.WL(1, "tabTargets:" + (float)tabTargets / 100.0f, true);
        tabTargets = 0;
        profileTime["Overall"] = TimeSpan.Zero;
        profileTime["ClearAllInfo"] = TimeSpan.Zero;
        profileTime["GetAllTabTargets"] = TimeSpan.Zero;
        profileTime["GetAllAlliesOf"] = TimeSpan.Zero;
        profileTime["GetLongestRangeWeapons"] = TimeSpan.Zero;
        profileTime["Recalc"] = TimeSpan.Zero;
        profileTime[" VisibilityToTargetUnit"] = TimeSpan.Zero;
        profileTime[" HasLOS"] = TimeSpan.Zero;
        profileTime[" GetLineOfFire"] = TimeSpan.Zero;
        profileTime[" IsInFiringArc"] = TimeSpan.Zero;
        profileTime[" CanRotateToFace"] = TimeSpan.Zero;
        profileTime[" fireInfo.Add"] = TimeSpan.Zero;
        overall.Restart();
        ProfilerCounter = 0;
      }
      return false;
    }
  }

  public static class Core {

    public static void Init(string directory, string settingsJson) {
      Log.BaseDirectory = directory;
      Log.InitLog();
      Log.LogWrite("Initing... " + directory + " version: " + Assembly.GetExecutingAssembly().GetName().Version + "\n", true);
      Profiler.Init();
    }
  }
}
