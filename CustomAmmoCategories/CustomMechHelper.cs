using BattleTech;
using Localize;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustAmmoCategories {
  public static class CustomMechHelper {
    public static List<Action<Mech, Transform>> InitGameRepPrefixes = new List<Action<Mech, Transform>>();
    public static List<Action<Mech, Transform>> InitGameRepPostfixes = new List<Action<Mech, Transform>>();
    public static void RegisterInitGameRepPrefix(Action<Mech, Transform> prefix) { InitGameRepPrefixes.Add(prefix); }
    public static void RegisterInitGameRepPostfix(Action<Mech, Transform> postfix) { InitGameRepPostfixes.Add(postfix); }

  }
}