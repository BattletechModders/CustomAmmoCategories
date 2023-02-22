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
    private static MethodInfo DamageModifiersCache_RegisterDamageModifier = null;
    public static bool CACModifierHelperDetected() { return ToHitModifiersHelper_registerModifier != null; }
    public static bool CACDamageModifierHelperDetected() { return DamageModifiersCache_RegisterDamageModifier != null; }
    public static float MyCACModifier(ToHit toHit, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPos, Vector3 targetPos, LineOfFireLevel lof, MeleeAttackType meleType, bool isCalled) {
      return 1f;
    }
    public static string MyCACModifierName(ToHit toHit, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPos, Vector3 targetPos, LineOfFireLevel lof, MeleeAttackType meleType, bool isCalled) {
      return "Cust. mod. " + attacker.DisplayName;
    }
    public static float MyDmgCACModifier(Weapon weapon, Vector3 attackPosition, ICombatant target, bool IsBreachingShot, int location, float dmg, float ap, float heat, float stab) {
      return 0f;
    }
    public static string MyDmgCACModifierName(Weapon weapon, Vector3 attackPosition, ICombatant target, bool IsBreachingShot, int location, float dmg, float ap, float heat, float stab) {
      return "Cust. mod. " + target.DisplayName;
    }
    public static void registerCACModifier(string id, string name, bool ranged, bool melee,
      Func<ToHit, AbstractActor, Weapon, ICombatant, Vector3, Vector3, LineOfFireLevel, MeleeAttackType, bool, float> modifier,
      Func<ToHit, AbstractActor, Weapon, ICombatant, Vector3, Vector3, LineOfFireLevel, MeleeAttackType, bool, string> dname
    ) {
      if (ToHitModifiersHelper_registerModifier == null) { return; }
      ToHitModifiersHelper_registerModifier.Invoke(null, new object[] { id, name, ranged, melee, modifier, dname });
    }
    public static void registerCACDmgModifier(string id, string staticName, bool isStatic, bool isNormal, bool isAP, bool isHeat, bool isStability,
      Func<Weapon, Vector3, ICombatant, bool, int, float, float, float, float, float> modifier,
      Func<Weapon, Vector3, ICombatant, bool, int, float, float, float, float, string> modname
    ) {
      if (DamageModifiersCache_RegisterDamageModifier == null) { return; }
      DamageModifiersCache_RegisterDamageModifier.Invoke(null, new object[] { id, staticName, isStatic, isNormal, isAP, isHeat, isStability, modifier, modname });
    }
    public static void detectCAC() {
      Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
      foreach (Assembly assembly in assemblies) {
        if (assembly.FullName.StartsWith("CustomAmmoCategories")) {
          Type helperType = assembly.GetType("CustAmmoCategories.ToHitModifiersHelper");
          if (helperType != null) {
            ToHitModifiersHelper_registerModifier = helperType.GetMethod("registerModifier", BindingFlags.Static | BindingFlags.Public);
          }
          Type dmgHelperType = assembly.GetType("CustAmmoCategories.DamageModifiersCache");
          if (dmgHelperType != null) {
            DamageModifiersCache_RegisterDamageModifier = dmgHelperType.GetMethod("RegisterDamageModifier", BindingFlags.Static | BindingFlags.Public);
          }
        }
      }
    }
    public static void FinishedLoading(List<string> loadOrder) {
      detectCAC();
      registerCACModifier("MYMOD DYN NAME", "MYMOD", true, false, MyCACModifier, null);
      registerCACModifier("MYMOD", "MYMOD", true, false, MyCACModifier, MyCACModifierName);
      registerCACDmgModifier("MYMOD", "MYMOD", false, true, false, false, false, MyDmgCACModifier, MyDmgCACModifierName);
    }
    public static void Init(string directory, string settingsJson) {
    }
  }
}
