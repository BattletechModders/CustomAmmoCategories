using BattleTech;
using System.Collections.Generic;
using System.Threading;

namespace CustAmmoCategories {
  //public static class StackDataHelper {
  //  //private static Dictionary<int, List<AbstractActor>> stack = new Dictionary<int, List<AbstractActor>>();
  //  //private static Dictionary<int, List<Weapon>> stackWeapon = new Dictionary<int, List<Weapon>>();
  //  //private static Dictionary<int, List<int>> attackSequenceId = new Dictionary<int, List<int>>();
  //  //private static Dictionary<int, List<PilotableActorDef>> stackMechDef = new Dictionary<int, List<PilotableActorDef>>();
  //  public static readonly string ABSTRACT_ACTOR_NAME = "AbstractActor";
  //  public static readonly string WEAPON_NAME = "Weapon";
  //  public static readonly string ATTACK_SEQUENCE_NAME = "attackSequence_id";
  //  public static readonly string MECH_DEF_NAME = "MechDef";
  //  private static Dictionary<int, Dictionary<string, Stack<object>>> stack = new Dictionary<int, Dictionary<string, Stack<object>>>();
  //  private static Dictionary<int, Dictionary<string, int>> stackFlags = new Dictionary<int, Dictionary<string, int>>();
  //  public static bool isFlagSet(this Thread thread,string flagName) {
  //    if(stackFlags.TryGetValue(thread.ManagedThreadId, out Dictionary<string, int> flags)) {
  //      if(flags.TryGetValue(flagName, out int count)) {
  //        return count > 0;
  //      }
  //    }
  //    return false;
  //  }
  //  public static void SetFlag(this Thread thread, string flagName) {
  //    if (stackFlags.TryGetValue(thread.ManagedThreadId, out Dictionary<string, int> flags) == false) {
  //      flags = new Dictionary<string, int>();
  //      stackFlags.Add(thread.ManagedThreadId, flags);
  //    }
  //    if (flags.TryGetValue(flagName, out int count)) {
  //      flags[flagName] = count + 1;
  //    } else {
  //      flags.Add(flagName, 1);
  //    }
  //  }
  //  public static void ClearFlag(this Thread thread, string flagName) {
  //    if (stackFlags.TryGetValue(thread.ManagedThreadId, out Dictionary<string, int> flags) == false) {
  //      flags = new Dictionary<string, int>();
  //      stackFlags.Add(thread.ManagedThreadId, flags);
  //    }
  //    if (flags.TryGetValue(flagName, out int count)) {
  //      flags[flagName] = count <= 1? 0 : count - 1;
  //    } else {
  //      flags.Add(flagName, 0);
  //    }
  //  }
  //  public static void Clear() { stack.Clear(); stackFlags.Clear(); }
  //  public static void pushToStack<T>(this Thread thread, string name, T payload) {
  //    if(stack.TryGetValue(thread.ManagedThreadId, out Dictionary<string, Stack<object>> thread_stack) == false) {
  //      thread_stack = new Dictionary<string, Stack<object>>();
  //      stack.Add(thread.ManagedThreadId, thread_stack);
  //    }
  //    if(thread_stack.TryGetValue(name,out Stack<object> data_stack) == false) {
  //      data_stack = new Stack<object>();
  //      thread_stack.Add(name, data_stack);
  //    }
  //    data_stack.Push(payload);
  //  }
  //  public static void popFromStack<T>(this Thread thread, string name) {
  //    if (stack.TryGetValue(thread.ManagedThreadId, out Dictionary<string, Stack<object>> thread_stack) == false) {
  //      thread_stack = new Dictionary<string, Stack<object>>();
  //      stack.Add(thread.ManagedThreadId, thread_stack);
  //    }
  //    if (thread_stack.TryGetValue(name, out Stack<object> data_stack) == false) {
  //      data_stack = new Stack<object>();
  //      thread_stack.Add(name, data_stack);
  //    }
  //    data_stack.Pop();
  //  }
  //  public static T peekFromStack<T>(this Thread thread, string name) {
  //    if (stack.TryGetValue(thread.ManagedThreadId, out Dictionary<string, Stack<object>> thread_stack) == false) {
  //      thread_stack = new Dictionary<string, Stack<object>>();
  //      stack.Add(thread.ManagedThreadId, thread_stack);
  //    }
  //    if (thread_stack.TryGetValue(name, out Stack<object> data_stack) == false) {
  //      data_stack = new Stack<object>();
  //      thread_stack.Add(name, data_stack);
  //    }
  //    if(data_stack.TryPeek(out object data)) {
  //      return (T)data;
  //    }
  //    return default(T);
  //  }
  //  public static int currentAttackSequence(this Thread thread) {
  //    return peekFromStack<int>(thread, ATTACK_SEQUENCE_NAME);
  //  }
  //  public static Mech currentMech(this Thread thread) {
  //    return peekFromStack<Mech>(thread, ABSTRACT_ACTOR_NAME);
  //  }
  //  public static Weapon currentWeapon(this Thread thread) {
  //    return peekFromStack<Weapon>(thread, WEAPON_NAME);
  //  }
  //  public static MechDef currentMechDef(this Thread thread) {
  //    return peekFromStack<MechDef>(thread, MECH_DEF_NAME);
  //  }
  //  public static PilotableActorDef currentPilotableActorDef(this Thread thread) {
  //    return peekFromStack<PilotableActorDef>(thread, MECH_DEF_NAME);
  //  }
  //  public static AbstractActor currentActor(this Thread thread) {
  //    return peekFromStack<AbstractActor>(thread, ABSTRACT_ACTOR_NAME);
  //  }
  //  public static void pushAttackSequenceId(this Thread thread, int id) {
  //    pushToStack<int>(thread, ATTACK_SEQUENCE_NAME, id);
  //  }
  //  public static void popAttackSequenceId(this Thread thread) {
  //    popFromStack<int>(thread, ATTACK_SEQUENCE_NAME);
  //  }
  //  public static void pushActor(this Thread thread, AbstractActor actor) {
  //    pushToStack<AbstractActor>(thread, ABSTRACT_ACTOR_NAME, actor);
  //  }
  //  public static void clearActor(this Thread thread) {
  //    popFromStack<AbstractActor>(thread, ABSTRACT_ACTOR_NAME);
  //  }
  //  public static void pushWeapon(this Thread thread, Weapon weapon) {
  //    pushToStack(thread, WEAPON_NAME, weapon);
  //  }
  //  public static void clearWeapon(this Thread thread) {
  //    popFromStack<Weapon>(thread, WEAPON_NAME);
  //  }
  //  public static void pushActorDef(this Thread thread, PilotableActorDef def) {
  //    pushToStack(thread, MECH_DEF_NAME, def);
  //  }
  //  public static void clearActorDef(this Thread thread) {
  //    popFromStack<PilotableActorDef>(thread, MECH_DEF_NAME);
  //  }
  //}
}