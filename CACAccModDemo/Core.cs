using BattleTech;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace CACAccModDemo {
  public class Core {
    private static MethodInfo ToHitModifiersHelper_registerModifier = null;
    public static bool CACModifierHelperDetected() { return ToHitModifiersHelper_registerModifier != null; }
    public static float MyCACModifier(ToHit toHit, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPos, Vector3 targetPos, LineOfFireLevel lof, MeleeAttackType meleType, bool isCalled) {
      return 1f;
    }
    public static string MyCACModifierName(ToHit toHit, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPos, Vector3 targetPos, LineOfFireLevel lof, MeleeAttackType meleType, bool isCalled) {
      return "Cust. mod. "+attacker.DisplayName;
    }
    public static void registerCACModifier(string id, string name, bool ranged, bool melee,
      Func<ToHit, AbstractActor, Weapon, ICombatant, Vector3, Vector3, LineOfFireLevel, MeleeAttackType, bool, float> modifier,
      Func<ToHit, AbstractActor, Weapon, ICombatant, Vector3, Vector3, LineOfFireLevel, MeleeAttackType, bool, string> dname
    ) {
      if (ToHitModifiersHelper_registerModifier == null) { return; }
      ToHitModifiersHelper_registerModifier.Invoke(null, new object[] { id, name, ranged, melee, modifier, dname });
    }
    public static void detectCAC() {
      Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
      foreach (Assembly assembly in assemblies) {
        if (assembly.FullName.StartsWith("CustomAmmoCategories")) {
          Type helperType = assembly.GetType("CustAmmoCategories.ToHitModifiersHelper");
          if (helperType != null) {
            ToHitModifiersHelper_registerModifier = helperType.GetMethod("registerModifier", BindingFlags.Static | BindingFlags.Public);
            break;
          }
        }
      }
    }
    public static void FinishedLoading(List<string> loadOrder) {
      detectCAC();
      registerCACModifier("MYMOD DYN NAME", "MYMOD", true, false, MyCACModifier, null);
      registerCACModifier("MYMOD", "MYMOD", true, false, MyCACModifier, MyCACModifierName);
    }
    public static void Init(string directory, string settingsJson) {
    }
  }
}
