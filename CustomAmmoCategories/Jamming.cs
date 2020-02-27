using BattleTech;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CustAmmoCategories;
using Random = UnityEngine.Random;
using UnityEngine;
using CustomAmmoCategoriesLog;
using Localize;


//This part of code is modified code of original WeaponRealizer by Joel Meador under MIT LICENSE

namespace CustAmmoCategories {
  public static class StatisticHelper {
    public static Statistic GetOrCreateStatisic<StatisticType>(this StatCollection collection, string statName, StatisticType defaultValue) {
      Statistic statistic = collection.GetStatistic(statName);
      if (statistic == null) {
        statistic = collection.AddStatistic<StatisticType>(statName, defaultValue);
      }
      return statistic;
    }
  }
  public class JammMessage {
    public AbstractActor actor;
    public string message;
    public JammMessage(AbstractActor act, string msg) {
      actor = act;
      message = msg;
    }
  }
  public static partial class CustomAmmoCategories {
    private static Queue<JammMessage> jammQueue = new Queue<JammMessage>();
    public static Queue<Weapon> jammAMSQueue = new Queue<Weapon>();
    public static Queue<AmmunitionBox> ammoExposionQueue = new Queue<AmmunitionBox>();
    public static readonly string AmmoBoxGUID = "CACAmmoBoxGUID";
    public static void AddToExposionCheck(AmmunitionBox box) {
      if (CustomAmmoCategories.Settings.AmmoCanBeExhausted == false) { return; };
      if (box == null) {
        CustomAmmoCategoriesLog.Log.LogWrite("AddToExposionCheck " + box.defId + " is not ammo box\n");
        return;
      }
      if (box.IsFunctional == false) {
        CustomAmmoCategoriesLog.Log.LogWrite("AddToExposionCheck " + box.defId + " is not functional\n");
      }
      ExtAmmunitionDef extAmmo = CustomAmmoCategories.findExtAmmo(box.ammoDef.Description.Id);
      if (extAmmo.AmmoCategory.Index == CustomAmmoCategories.NotSetCustomAmmoCategoty.Index) {
        CustomAmmoCategoriesLog.Log.LogWrite("AddToExposionCheck " + box.defId + " has no ammo category\n");
        return;
      }
      if (extAmmo.CanBeExhaustedAt < CustomAmmoCategories.Epsilon) {
        CustomAmmoCategoriesLog.Log.LogWrite("AddToExposionCheck " + box.ammoDef.Description.Id + " not exposing\n");
        return;
      }
      ammoExposionQueue.Enqueue(box);
    }
    public static void createAmmoBoxGUID(AmmunitionBox box, string GUID = "") {
      if (box.StatCollection.ContainsStatistic(CustomAmmoCategories.AmmoBoxGUID) == false) {
        if (string.IsNullOrEmpty(GUID)) { GUID = Guid.NewGuid().ToString(); }
        box.StatCollection.AddStatistic<string>(CustomAmmoCategories.AmmoBoxGUID, GUID);
      }
    }
    public static string getAmmoBoxGUID(AmmunitionBox box) {
      if (box.StatCollection.ContainsStatistic(CustomAmmoCategories.AmmoBoxGUID) == false) {
        string GUID = Guid.NewGuid().ToString();
        CustomAmmoCategories.createAmmoBoxGUID(box, GUID);
        return GUID;
      }
      return box.StatCollection.GetStatistic(CustomAmmoCategories.AmmoBoxGUID).Value<string>();
    }
    public static void prosessExposion() {
      while (CustomAmmoCategories.ammoExposionQueue.Count > 0) {
        AmmunitionBox ammoBox = CustomAmmoCategories.ammoExposionQueue.Dequeue();
        HashSet<string> checkedGUIDs = new HashSet<string>();
        try {
          if (ammoBox.IsFunctional == false) {
            CustomAmmoCategoriesLog.Log.LogWrite("prosessExposion " + ammoBox.defId + " is not functional\n");
          }
          ExtAmmunitionDef extAmmo = CustomAmmoCategories.findExtAmmo(ammoBox.ammoDef.Description.Id);
          if (extAmmo.AmmoCategory.Index == CustomAmmoCategories.NotSetCustomAmmoCategoty.Index) {
            CustomAmmoCategoriesLog.Log.LogWrite("prosessExposion " + ammoBox.defId + " has no ammo category\n");
            continue;
          }
          if (extAmmo.CanBeExhaustedAt < CustomAmmoCategories.Epsilon) {
            CustomAmmoCategoriesLog.Log.LogWrite("prosessExposion " + ammoBox.ammoDef.Description.Id + " not exposing\n");
            continue;
          }
          string GUID = CustomAmmoCategories.getAmmoBoxGUID(ammoBox);
          if (checkedGUIDs.Contains(GUID)) {
            CustomAmmoCategoriesLog.Log.LogWrite("prosessExposion GUID " + GUID + " already checked\n");
            continue;
          }
          checkedGUIDs.Add(GUID);
          float curAmmo = (float)ammoBox.CurrentAmmo / (float)ammoBox.AmmoCapacity;
          if (curAmmo < extAmmo.CanBeExhaustedAt) {
            float rollBorder = (extAmmo.CanBeExhaustedAt - curAmmo) / extAmmo.CanBeExhaustedAt;
            float exposedRoll = Random.Range(0f, 1f);
            CustomAmmoCategoriesLog.Log.LogWrite("roll " + exposedRoll + " " + rollBorder + "\n");
            if (exposedRoll <= rollBorder) {
              ammoBox.StatCollection.Set<ComponentDamageLevel>("DamageLevel", ComponentDamageLevel.Destroyed);
              ammoBox.parent.Combat.MessageCenter.PublishMessage(
                  new AddSequenceToStackMessage(
                      new ShowActorInfoSequence(ammoBox.parent, new Text("__/CAC.AMMOBOXEXHAUSTED/__", ammoBox.UIName), FloatieMessage.MessageNature.Debuff, true)));
            }
          } else {
            CustomAmmoCategoriesLog.Log.LogWrite("prosessExposion curAmmo " + curAmmo + " is not less than border " + extAmmo.CanBeExhaustedAt + "\n");
          }
        } catch (Exception e) {
          CustomAmmoCategoriesLog.Log.LogWrite("prosessExposion exception " + e.ToString() + "\n", true);
        }
      }
    }
    public static void prosessJummingMessages() {
      while (jammQueue.Count > 0) {
        JammMessage message = jammQueue.Dequeue();
        CustomAmmoCategoriesLog.Log.LogWrite("Publishing jamm message for " + message.actor.DisplayName + " " + message.message + "\n");
        message.actor.Combat.MessageCenter.PublishMessage(
            new AddSequenceToStackMessage(
                new ShowActorInfoSequence(message.actor, message.message, FloatieMessage.MessageNature.Debuff, true)));
      }
      prosessExposion();
    }
    public static void addJamMessage(AbstractActor actor, string message) {
      jammQueue.Enqueue(new JammMessage(actor, message));
    }
  }

  [HarmonyPatch(typeof(AbstractActor), "OnActivationEnd", MethodType.Normal)]
  public static class unJammingEnabler {
    public static bool Prefix(AbstractActor __instance) {
      var actor = __instance;
      CustomAmmoCategories.ClearEjection(actor);
      foreach (Weapon weapon in actor.Weapons) {
        //ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
        //if (extWeapon.IsAMS == true) { CustomAmmoCategories.setWeaponAMSShootsCount(weapon, 0); };
        if (weapon.roundsSinceLastFire <= 0) {
          weapon.setCantAMSFire(true);
          continue;
        };
        weapon.setCantAMSFire(false);
        if (CustomAmmoCategories.IsCooldown(weapon) > 0) {
          var removedJam = CustomAmmoCategories.AttemptToRemoveCooldown(actor, weapon);
          CustomAmmoCategoriesLog.Log.LogWrite($"Removed cooldown? {removedJam}\n");
        }
      }
      if (actor.IsShutDown) return true;

      foreach (Weapon weapon in actor.Weapons) {
        if (weapon.roundsSinceLastFire <= 0) { continue; };
        if (CustomAmmoCategories.IsJammed(weapon)) {
          var removedJam = CustomAmmoCategories.AttemptToRemoveJam(actor, weapon);
          CustomAmmoCategoriesLog.Log.LogWrite($"Removed Jam? {removedJam}\n");
        }
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(Weapon))]
  [HarmonyPatch("HasAmmo")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class JammingRealizer {
    public static void Postfix(Weapon __instance, ref bool __result) {
      if (__result == false) { return; }
      if (__instance.IsJammed()) { __result = false; }
      if (__instance.IsCooldown() > 0) { __result = false; }
      if (__instance.isAMS() && __instance.isCantAMSFire()) { __result = false; };
      if ((__instance.isAMS() == false) && __instance.isCantNormalFire()) { __result = false; };
      if (__instance.NoModeToFire()) { __result = false; };
      if (__instance.isBlocked()) { __result = false; };
    }
  }
  [HarmonyPatch(typeof(AttackDirector))]
  [HarmonyPatch("OnAttackComplete")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
  public static class JammingEnabler {
    public static bool Prefix(AttackDirector __instance, MessageCenterMessage message) {
      AttackCompleteMessage attackCompleteMessage = (AttackCompleteMessage)message;
      int sequenceId = attackCompleteMessage.sequenceId;
      AttackDirector.AttackSequence attackSequence = __instance.GetAttackSequence(sequenceId);
      if (attackSequence == null) {
        return true;
      }
      //JammingEnabler.jammQueue.Enqueue(attackSequence.attacker);
      //AbstractActor actor = attackSequence.target as AbstractActor;
      //JammingEnabler.jammAMS();
      JammingEnabler.jamm(attackSequence.attacker);
      return true;
    }
    public static void jammAMS() {
      CustomAmmoCategoriesLog.Log.LogWrite($"Jamming AMS sequence\n");
      while (CustomAmmoCategories.jammAMSQueue.Count > 0) {
        Weapon weapon = CustomAmmoCategories.jammAMSQueue.Dequeue();
        int shootsCount = weapon.AMSShootsCount();
        Log.LogWrite(" " + weapon.parent.DisplayName + ":" + weapon.parent.GUID + ":" + weapon.UIName + " shots:" + shootsCount + "\n");
        if (shootsCount <= 0) {
          CustomAmmoCategoriesLog.Log.LogWrite($" AMS was idle\n");
          continue;
        }
        weapon.AMSShootsCount(0);
        weapon.setCantNormalFire(true);
        float flatJammingChance = weapon.FlatJammingChance();
        CustomAmmoCategoriesLog.Log.LogWrite($"  flatJammingChance " + flatJammingChance + "\n");
        if (flatJammingChance > CustomAmmoCategories.Epsilon) {
          CustomAmmoCategoriesLog.Log.LogWrite($"  Try jamm weapon " + weapon.UIName + "\n");
          float Roll = Random.Range(0.0f, 1.0f);
          CustomAmmoCategoriesLog.Log.LogWrite($" Jamming chance " + flatJammingChance + " roll " + Roll + "\n");
          if (Roll < flatJammingChance) {
            CustomAmmoCategoriesLog.Log.LogWrite($" Jammed!\n");
            CustomAmmoCategories.AddJam(weapon.parent, weapon);
          }
        }
        if (weapon.Cooldown() > 0) {
          weapon.Cooldown(weapon.Cooldown(), false);
        }
      }
    }
    public static void jamm(AbstractActor actor) {
      CustomAmmoCategoriesLog.Log.LogWrite($"Try jamm weapon\n");
      //while (JammingEnabler.jammQueue.Count > 0) {
      //AbstractActor actor = JammingEnabler.jammQueue.Dequeue();
      CustomAmmoCategoriesLog.Log.LogWrite($"Try jamm weapon of " + actor.DisplayName + "\n");
      foreach (Weapon weapon in actor.Weapons) {
        CustomAmmoCategoriesLog.Log.LogWrite($"  weapon " + weapon.UIName + " rounds since last fire " + weapon.roundsSinceLastFire + "\n");
        if (weapon.roundsSinceLastFire > 0) {
          continue;
        }
        float flatJammingChance = weapon.FlatJammingChance();
        CustomAmmoCategoriesLog.Log.LogWrite($"  flatJammingChance " + flatJammingChance + "\n");
        if (flatJammingChance > CustomAmmoCategories.Epsilon) {
          CustomAmmoCategoriesLog.Log.LogWrite($"  Try jamm weapon " + weapon.UIName + "\n");
          float Roll = Random.Range(0.0f, 1.0f);
          CustomAmmoCategoriesLog.Log.LogWrite($" Jamming chance " + flatJammingChance + " roll " + Roll + "\n");
          if (Roll < flatJammingChance) {
            CustomAmmoCategoriesLog.Log.LogWrite($" Jammed!\n");
            CustomAmmoCategories.AddJam(actor, weapon);
          }
        }
        if (weapon.Cooldown() > 0) {
          weapon.Cooldown(weapon.Cooldown(), true);
        }
      }
      //}
    }
    public static void Postfix(AttackDirector __instance, MessageCenterMessage message) {
      System.Threading.Timer timer = null;
      timer = new System.Threading.Timer((obj) => {
        CustomAmmoCategories.prosessJummingMessages();
        timer.Dispose();
      }, null, 1500, System.Threading.Timeout.Infinite);
    }
  }
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("InitEffectStats")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class AbstractActor_InitEffectStats {
    public static void Postfix(AbstractActor __instance) {
      Log.LogWrite("AbstractActor.InitEffectStats " + __instance.DisplayName + ":" + __instance.GUID + "\n");
      __instance.FlatJammChance(0f);
    }
  }

  [HarmonyPatch(typeof(Weapon))]
  [HarmonyPatch("InitStats")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class Weapon_InitStatsJamm {
    public static void Postfix(Weapon __instance) {
      Log.LogWrite("Weapon.InitStats " + __instance.defId + ":" + __instance.parent.GUID + "\n");
      __instance.FlatJammChanceStat(0f);
    }
  }

  public static partial class CustomAmmoCategories {
    public static string JammedWeaponStatisticName = "CAC-JammedWeapon";
    public static string CooldownWeaponStatisticName = "CAC-CooldownWeapon";
    public static string NoNormalFireStatisticName = "CAC-NoNormalFire";
    public static string NoAMSFireStatisticName = "CAC-AMSFire";
    public static string FlatJammingChanceStatisticName = "CACFlatJammingChance";
    //public static string TemporarilyDisabledStatisticName = "TemporarilyDisabled";
    public static float Epsilon = 0.001f;
    public static float FlatJammChance(this ICombatant unit) {
      Statistic stat = unit.StatCollection.GetStatistic(FlatJammingChanceStatisticName);
      if (stat == null) { return 0f; }
      return stat.Value<float>();
    }
    public static void FlatJammChance(this ICombatant unit, float val) {
      if (unit.StatCollection.ContainsStatistic(FlatJammingChanceStatisticName) == false) {
        unit.StatCollection.AddStatistic<float>(FlatJammingChanceStatisticName, val);
      } else {
        unit.StatCollection.Set<float>(FlatJammingChanceStatisticName, val);
      }
    }
    public static float FlatJammChanceStat(this Weapon weapon) {
      Statistic stat = weapon.StatCollection.GetStatistic(FlatJammingChanceStatisticName);
      if (stat == null) { return 0f; }
      return stat.Value<float>();
    }
    public static void FlatJammChanceStat(this Weapon weapon, float val) {
      if (weapon.StatCollection.ContainsStatistic(FlatJammingChanceStatisticName) == false) {
        weapon.StatCollection.AddStatistic<float>(FlatJammingChanceStatisticName, val);
      } else {
        weapon.StatCollection.Set<float>(FlatJammingChanceStatisticName, val);
      }
    }
    public static bool isCantNormalFire(this Weapon weapon) {
      var statistic = StatisticHelper.GetOrCreateStatisic<bool>(weapon.StatCollection, NoNormalFireStatisticName, false);
      return statistic.Value<bool>();
    }
    public static bool isCantAMSFire(this Weapon weapon) {
      var statistic = StatisticHelper.GetOrCreateStatisic<bool>(weapon.StatCollection, NoAMSFireStatisticName, false);
      return statistic.Value<bool>();
    }
    public static void setCantNormalFire(this Weapon weapon, bool value) {
      if (weapon.StatCollection.ContainsStatistic(CustomAmmoCategories.NoNormalFireStatisticName) == false) {
        weapon.StatCollection.AddStatistic<bool>(CustomAmmoCategories.NoNormalFireStatisticName, value);
      } else {
        weapon.StatCollection.Set<bool>(CustomAmmoCategories.NoNormalFireStatisticName, value);
      }
    }
    public static void setCantAMSFire(this Weapon weapon, bool value) {
      if (weapon.StatCollection.ContainsStatistic(CustomAmmoCategories.NoAMSFireStatisticName) == false) {
        weapon.StatCollection.AddStatistic<bool>(CustomAmmoCategories.NoAMSFireStatisticName, value);
      } else {
        weapon.StatCollection.Set<bool>(CustomAmmoCategories.NoAMSFireStatisticName, value);
      }
    }
    public static void AddJam(AbstractActor actor, Weapon weapon) {
      bool damage = weapon.DamageOnJamming();
      bool destroy = weapon.DestroyOnJamming();
      if ((damage == false)&&(destroy == false)) {
        if (weapon.StatCollection.ContainsStatistic(CustomAmmoCategories.JammedWeaponStatisticName) == false) {
          weapon.StatCollection.AddStatistic<bool>(CustomAmmoCategories.JammedWeaponStatisticName, false);
        }
        weapon.StatCollection.Set<bool>(CustomAmmoCategories.JammedWeaponStatisticName, true);
        //weapon.StatCollection.Set<bool>(CustomAmmoCategories.TemporarilyDisabledStatisticName, true);
        if (CustomAmmoCategories.Settings.DontShowNotDangerouceJammMessages == false) {
          CustomAmmoCategories.addJamMessage(actor, $"{weapon.UIName} __/CAC.Jammed/__!");
        }
        //actor.Combat.MessageCenter.PublishMessage(
        //    new AddSequenceToStackMessage(
        //        new ShowActorInfoSequence(actor, $"{weapon.Name} Jammed!", FloatieMessage.MessageNature.Debuff,
        //            true)));
      } else {
        var isDestroying = weapon.DamageLevel != ComponentDamageLevel.Functional;
        if (destroy == true) { isDestroying = true; };
        var damageLevel = isDestroying ? ComponentDamageLevel.Destroyed : ComponentDamageLevel.Penalized;
        var fakeHit = new WeaponHitInfo(-1, -1, -1, -1, string.Empty, string.Empty, -1, null, null, null, null, null, null, null, null, null, null, null);
        weapon.DamageComponent(fakeHit, damageLevel, true);
        var message = isDestroying
            ? $"{weapon.UIName} __/CAC.misfire/__: __/CAC.Destroyed/__!"
            : $"{weapon.UIName} __/CAC.misfire/__: __/CAC.Damaged/__!";
        CustomAmmoCategories.addJamMessage(actor, message);
        //actor.Combat.MessageCenter.PublishMessage(
        //    new AddSequenceToStackMessage(
        //        new ShowActorInfoSequence(actor, message, FloatieMessage.MessageNature.Debuff, true)));
      }
    }
    public static void Cooldown(this Weapon weapon, int rounds, bool message) {
      if (weapon.StatCollection.ContainsStatistic(CustomAmmoCategories.CooldownWeaponStatisticName) == false) {
        weapon.StatCollection.AddStatistic<int>(CustomAmmoCategories.CooldownWeaponStatisticName, 0);
      }
      if (rounds > 0) {
        weapon.StatCollection.Set<int>(CustomAmmoCategories.CooldownWeaponStatisticName, rounds);
        //weapon.StatCollection.Set<bool>(CustomAmmoCategories.TemporarilyDisabledStatisticName, true);
        if (message) {
          if (CustomAmmoCategories.Settings.DontShowNotDangerouceJammMessages == false) {
            CustomAmmoCategories.addJamMessage(weapon.parent, $"{weapon.UIName} __/CAC.Cooldown/__!");
          }
        }
      }
    }
    public static bool IsJammed(this Weapon weapon) {
      var statistic = StatisticHelper.GetOrCreateStatisic<bool>(weapon.StatCollection, JammedWeaponStatisticName, false);
      return statistic.Value<bool>();
    }
    public static int IsCooldown(this Weapon weapon) {
      var statistic = StatisticHelper.GetOrCreateStatisic<int>(weapon.StatCollection, CooldownWeaponStatisticName, 0);
      return statistic.Value<int>();
    }
    public static float FlatJammingChance(this Weapon weapon) {
      ExtWeaponDef def = weapon.exDef();
      ExtAmmunitionDef ammo = weapon.ammo();
      WeaponMode mode = weapon.mode();
      float result = weapon.FlatJammChanceStat();
      if (weapon.parent != null) {
        if (weapon.parent.EvasivePipsCurrent > 0) {
          float evasiveMod = def.evasivePipsMods.FlatJammingChance + ammo.evasivePipsMods.FlatJammingChance + mode.evasivePipsMods.FlatJammingChance;
          if (Mathf.Abs(evasiveMod) > CustomAmmoCategories.Epsilon) result = result * Mathf.Pow((float)weapon.parent.EvasivePipsCurrent, evasiveMod);
        }
      }
      result += weapon.parent.FlatJammChance();
      float mult = 0;
      float baseval = 0f;
      result += ammo.FlatJammingChance;
      mult += ammo.GunneryJammingMult;
      if (ammo.GunneryJammingBase > 0f) { baseval = ammo.GunneryJammingBase; };
      result += def.FlatJammingChance;
      mult += def.GunneryJammingMult;
      if ((def.GunneryJammingBase > 0f) && (baseval == 0f)) { baseval = def.GunneryJammingBase; };
      result += mode.FlatJammingChance;
      mult += mode.GunneryJammingMult;
      if ((mode.GunneryJammingBase > 0f) && (baseval == 0f)) { baseval = mode.GunneryJammingBase; };
      if (weapon.parent != null) {
        if (baseval == 0f) { baseval = 5f; }
        result += ((baseval - weapon.parent.SkillGunnery) * mult);
      }
      return result;
    }
    public static bool DamageOnJamming(this Weapon weapon) {
      WeaponMode mode = weapon.mode();
      if (mode.DamageOnJamming != TripleBoolean.NotSet) { return mode.DamageOnJamming == TripleBoolean.True; }
      return weapon.exDef().DamageOnJamming == TripleBoolean.True;
    }
    public static bool DestroyOnJamming(this Weapon weapon) {
      WeaponMode mode = weapon.mode();
      if (mode.DestroyOnJamming != TripleBoolean.NotSet) { return mode.DestroyOnJamming == TripleBoolean.True; }
      return weapon.exDef().DestroyOnJamming == TripleBoolean.True;
    }

    public static int Cooldown(this Weapon weapon) {
      return weapon.exDef().Cooldown + weapon.mode().Cooldown;
    }
    public static bool AttemptToRemoveJam(AbstractActor actor, Weapon weapon) {
      var skill = actor.SkillGunnery;
      var mitigationRoll = Random.Range(1, 10);
      StringBuilder sb = new StringBuilder();
      sb.AppendLine($"gunneryskill: {skill}");
      sb.AppendLine($"mitigationRoll: {mitigationRoll}");
      if (skill >= mitigationRoll) {
        CustomAmmoCategoriesLog.Log.LogWrite(sb.ToString() + "\n");
        RemoveJam(actor, weapon);
        return true;
      }

      CustomAmmoCategoriesLog.Log.LogWrite(sb.ToString() + "\n");
      return false;
    }
    public static bool AttemptToRemoveCooldown(AbstractActor actor, Weapon weapon) {
      int cooldown = weapon.StatCollection.GetStatistic(CustomAmmoCategories.CooldownWeaponStatisticName).Value<int>();
      if (cooldown <= 1) {
        weapon.StatCollection.Set<int>(CustomAmmoCategories.CooldownWeaponStatisticName, 0);
        //if (weapon.IsJammed() == false) { weapon.StatCollection.Set<bool>(TemporarilyDisabledStatisticName, false); };
        CustomAmmoCategoriesLog.Log.LogWrite($" Weapon " + weapon.UIName + " - operational\n");
        /*actor.Combat.MessageCenter.PublishMessage(
            new AddSequenceToStackMessage(
                new ShowActorInfoSequence(actor, $"{weapon.Name} Ready!", FloatieMessage.MessageNature.Buff,
                    true)));*/
        return true;
      } else {
        weapon.StatCollection.Set<int>(CustomAmmoCategories.CooldownWeaponStatisticName, cooldown - 1);
        //weapon.StatCollection.Set<bool>(TemporarilyDisabledStatisticName, true);
        CustomAmmoCategoriesLog.Log.LogWrite($" Weapon " + weapon.UIName + " - cooldown " + (cooldown - 1) + "\n");
        return false;
      }
    }
    private static void RemoveJam(AbstractActor actor, Weapon weapon) {
      weapon.StatCollection.Set<bool>(JammedWeaponStatisticName, false);
      //if (weapon.IsCooldown() <= 0) { weapon.StatCollection.Set<bool>(TemporarilyDisabledStatisticName, false); };
      actor.Combat.MessageCenter.PublishMessage(
          new AddSequenceToStackMessage(
              new ShowActorInfoSequence(actor, $"{weapon.UIName} __/CAC.Unjammed/__!", FloatieMessage.MessageNature.Buff,
                  true)));
    }
  }
}
