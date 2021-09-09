using BattleTech;
using Localize;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustAmmoCategories {
  public interface ICustomMech {
    bool isSquad { get; }
    bool isVehicle { get; }
    bool isQuad { get; }
    HashSet<ArmorLocation> GetDFASelfDamageLocations();
    HashSet<ArmorLocation> GetLandmineDamageArmorLocations();
    HashSet<ArmorLocation> GetBurnDamageArmorLocations();
    Dictionary<ArmorLocation, int> GetHitTable(AttackDirection from);
    Dictionary<int, float> GetAOESpreadArmorLocations();
    List<int> GetAOEPossibleHitLocations(Vector3 attackPos);
    Text GetLongArmorLocation(ArmorLocation location);
    ArmorLocation GetAdjacentLocations(ArmorLocation location);
    Dictionary<ArmorLocation, int> GetClusterTable(ArmorLocation originalLocation, Dictionary<ArmorLocation, int> hitTable);
    Dictionary<ArmorLocation, int> GetHitTableCluster(AttackDirection from, ArmorLocation originalLocation);
  }
  //public static class CustomMechHelper {
  //  public static List<Action<Mech>> prefixes_Mech_InitGameRep = new List<Action<Mech>>();
  //  public static List<Action<Mech>> postfixes_Mech_InitGameRep = new List<Action<Mech>>();
  //  public static List<Action<MechRepresentationSimGame>> prefixes_MechRepresentationSimGame_LoadWeapon = new List<Action<MechRepresentationSimGame>>();
  //  public static List<Action<MechRepresentationSimGame>> postfixes_MechRepresentationSimGame_LoadWeapon = new List<Action<MechRepresentationSimGame>>();
  //  public static void RegisterMechInitGameRepPrefix(Action<Mech> prefix) {
  //    prefixes_Mech_InitGameRep.Add(prefix);
  //  }
  //  public static void RegisterMechInitGameRepPostfix(Action<Mech> postfix) {
  //    postfixes_Mech_InitGameRep.Add(postfix);
  //  }
  //  public static void RegisterMechRepresentationSimGameLoadWeaponPrefix(Action<MechRepresentationSimGame> prefix) {
  //    prefixes_MechRepresentationSimGame_LoadWeapon.Add(prefix);
  //  }
  //  public static void RegisterMechRepresentationSimGameLoadWeaponPostfix(Action<MechRepresentationSimGame> postfix) {
  //    postfixes_MechRepresentationSimGame_LoadWeapon.Add(postfix);
  //  }
  //  public static void MechInitGameRep_prefixes(this Mech mech) {
  //    foreach (Action<Mech> prefix in prefixes_Mech_InitGameRep) { prefix(mech); }
  //  }
  //  public static void MechInitGameRep_postfixes(this Mech mech) {
  //    foreach (Action<Mech> postfix in postfixes_Mech_InitGameRep) { postfix(mech); }
  //  }
  //  public static void MechRepresentationSimGameLoadWeapon_prefixes(this MechRepresentationSimGame mech) {
  //    foreach (Action<MechRepresentationSimGame> prefix in prefixes_MechRepresentationSimGame_LoadWeapon) { prefix(mech); }
  //  }
  //  public static void MechRepresentationSimGameLoadWeapon_postfixes(this MechRepresentationSimGame mech) {
  //    foreach (Action<MechRepresentationSimGame> postfix in postfixes_MechRepresentationSimGame_LoadWeapon) { postfix(mech); }
  //  }
  //}
}