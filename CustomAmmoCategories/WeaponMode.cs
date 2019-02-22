using BattleTech;
using HBS.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustAmmoCategories {
  public class WeaponMode {
    public static string BASE_MODE_NAME = "B";
    public static string NONE_MODE_NAME = "!NONE!";
    public string UIName { get; set; }
    public string Id { get; set; }
    public float AccuracyModifier { get; set; }
    public float DirectFireModifier { get; set; }
    public float DamagePerShot { get; set; }
    public float HeatDamagePerShot { get; set; }
    public int HeatGenerated { get; set; }
    public float CriticalChanceMultiplier { get; set; }
    public int ShotsWhenFired { get; set; }
    public int AIBattleValue { get; set; }
    public int ProjectilesPerShot { get; set; }
    public List<EffectData> statusEffects { get; set; }
    public float MinRange { get; set; }
    public float MaxRange { get; set; }
    public float LongRange { get; set; }
    public float ShortRange { get; set; }
    public float ForbiddenRange { get; set; }
    public float MediumRange { get; set; }
    public int RefireModifier { get; set; }
    public int AttackRecoil { get; set; }
    public int Cooldown { get; set; }
    public float AIHitChanceCap { get; set; }
    public float Instability { get; set; }
    public float FlatJammingChance { get; set; }
    public float GunneryJammingBase { get; set; }
    public float GunneryJammingMult { get; set; }
    public float DistantVariance { get; set; }
    public TripleBoolean DistantVarianceReversed { get; set; }
    public float DamageVariance { get; set; }
    public string WeaponEffectID { get; set; }
    public float EvasivePipsIgnored { get; set; }
    public TripleBoolean IndirectFireCapable { get; set; }
    public TripleBoolean DamageOnJamming { get; set; }
    public TripleBoolean AOECapable { get; set; }
    public HitGeneratorType HitGenerator { get; set; }
    public TripleBoolean AlwaysIndirectVisuals { get; set; }
    public bool isBaseMode { get; set; }
    public float DamageMultiplier { get; set; }
    public CustomAmmoCategory AmmoCategory { get; set; }
    public WeaponMode() {
      Id = WeaponMode.NONE_MODE_NAME;
      UIName = WeaponMode.BASE_MODE_NAME;
      AccuracyModifier = 0;
      DirectFireModifier = 0;
      DamagePerShot = 0;
      HeatDamagePerShot = 0;
      HeatGenerated = 0;
      ProjectilesPerShot = 0;
      ShotsWhenFired = 0;
      CriticalChanceMultiplier = 0;
      MinRange = 0;
      MaxRange = 0;
      LongRange = 0;
      MaxRange = 0;
      ShortRange = 0;
      MediumRange = 0;
      AIBattleValue = 100;
      RefireModifier = 0;
      Instability = 0;
      AttackRecoil = 0;
      EvasivePipsIgnored = 0;
      FlatJammingChance = 0;
      DistantVariance = 0;
      DamageVariance = 0;
      Cooldown = 0;
      AIHitChanceCap = 0;
      ForbiddenRange = 0;
      GunneryJammingBase = 0;
      GunneryJammingMult = 0;
      AlwaysIndirectVisuals = TripleBoolean.NotSet;
      DamageMultiplier = 1.0f;
      DamageOnJamming = TripleBoolean.NotSet;
      DistantVarianceReversed = TripleBoolean.NotSet;
      IndirectFireCapable = TripleBoolean.NotSet;
      AOECapable = TripleBoolean.NotSet;
      WeaponEffectID = "";
      HitGenerator = HitGeneratorType.NotSet;
      isBaseMode = false;
      statusEffects = new List<EffectData>();
      AmmoCategory = null;
    }
    public void fromJSON(string json) {
      JObject jWeaponMode = JObject.Parse(json);
      if (jWeaponMode["Id"] != null) {
        this.Id = (string)jWeaponMode["Id"];
      }
      if (jWeaponMode["UIName"] != null) {
        this.UIName = (string)jWeaponMode["UIName"];
      }
      if (jWeaponMode["AccuracyModifier"] != null) {
        this.AccuracyModifier = (float)jWeaponMode["AccuracyModifier"];
      }
      if (jWeaponMode["DamagePerShot"] != null) {
        this.DamagePerShot = (float)jWeaponMode["DamagePerShot"];
      }
      if (jWeaponMode["HeatDamagePerShot"] != null) {
        this.HeatDamagePerShot = (float)jWeaponMode["HeatDamagePerShot"];
      }
      if (jWeaponMode["HeatGenerated"] != null) {
        this.HeatGenerated = (int)jWeaponMode["HeatGenerated"];
      }
      if (jWeaponMode["ProjectilesPerShot"] != null) {
        this.ProjectilesPerShot = (int)jWeaponMode["ProjectilesPerShot"];
      }
      if (jWeaponMode["ShotsWhenFired"] != null) {
        this.ShotsWhenFired = (int)jWeaponMode["ShotsWhenFired"];
      }
      if (jWeaponMode["CriticalChanceMultiplier"] != null) {
        this.CriticalChanceMultiplier = (float)jWeaponMode["CriticalChanceMultiplier"];
      }
      if (jWeaponMode["AIBattleValue"] != null) {
        this.AIBattleValue = (int)jWeaponMode["AIBattleValue"];
      }
      if (jWeaponMode["MinRange"] != null) {
        this.MinRange = (float)jWeaponMode["MinRange"];
      }
      if (jWeaponMode["MaxRange"] != null) {
        this.MaxRange = (float)jWeaponMode["MaxRange"];
      }
      if (jWeaponMode["ShortRange"] != null) {
        this.ShortRange = (float)jWeaponMode["ShortRange"];
      }
      if (jWeaponMode["ForbiddenRange"] != null) {
        this.ForbiddenRange = (float)jWeaponMode["ForbiddenRange"];
      }
      if (jWeaponMode["MediumRange"] != null) {
        this.MediumRange = (float)jWeaponMode["MediumRange"];
      }
      if (jWeaponMode["LongRange"] != null) {
        this.LongRange = (float)jWeaponMode["LongRange"];
      }
      if (jWeaponMode["RefireModifier"] != null) {
        this.RefireModifier = (int)jWeaponMode["RefireModifier"];
      }
      if (jWeaponMode["isBaseMode"] != null) {
        this.isBaseMode = (bool)jWeaponMode["isBaseMode"];
      }
      if (jWeaponMode["Instability"] != null) {
        this.Instability = (float)jWeaponMode["Instability"];
      }
      if (jWeaponMode["AttackRecoil"] != null) {
        this.AttackRecoil = (int)jWeaponMode["AttackRecoil"];
      }
      if (jWeaponMode["WeaponEffectID"] != null) {
        this.WeaponEffectID = (string)jWeaponMode["WeaponEffectID"];
      }
      if (jWeaponMode["EvasivePipsIgnored"] != null) {
        this.EvasivePipsIgnored = (float)jWeaponMode["EvasivePipsIgnored"];
      }
      if (jWeaponMode["IndirectFireCapable"] != null) {
        this.IndirectFireCapable = ((bool)jWeaponMode["IndirectFireCapable"] == true) ? TripleBoolean.True : TripleBoolean.False;
      }
      if (jWeaponMode["DirectFireModifier"] != null) {
        this.DirectFireModifier = (float)jWeaponMode["DirectFireModifier"];
      }
      if (jWeaponMode["DistantVariance"] != null) {
        this.DistantVariance = (float)jWeaponMode["DistantVariance"];
      }
      if (jWeaponMode["DamageMultiplier"] != null) {
        this.DamageMultiplier = (float)jWeaponMode["DamageMultiplier"];
      }
      if (jWeaponMode["DistantVarianceReversed"] != null) {
        this.DistantVarianceReversed = ((bool)jWeaponMode["DistantVarianceReversed"] == true) ? TripleBoolean.True : TripleBoolean.False;
      }
      if (jWeaponMode["DamageVariance"] != null) {
        this.DamageVariance = (float)jWeaponMode["DamageVariance"];
      }
      if (jWeaponMode["FlatJammingChance"] != null) {
        this.FlatJammingChance = (float)jWeaponMode["FlatJammingChance"];
      }
      if (jWeaponMode["GunneryJammingBase"] != null) {
        this.GunneryJammingBase = (float)jWeaponMode["GunneryJammingBase"];
      }
      if (jWeaponMode["GunneryJammingMult"] != null) {
        this.GunneryJammingMult = (float)jWeaponMode["GunneryJammingMult"];
      }
      if (jWeaponMode["AIHitChanceCap"] != null) {
        this.AIHitChanceCap = (float)jWeaponMode["AIHitChanceCap"];
      }
      if (jWeaponMode["Cooldown"] != null) {
        this.Cooldown = (int)jWeaponMode["Cooldown"];
      }
      if (jWeaponMode["AmmoCategory"] != null) {
        this.AmmoCategory = CustomAmmoCategories.find((string)jWeaponMode["AmmoCategory"]);
      }
      if (jWeaponMode["DamageOnJamming"] != null) {
        this.DamageOnJamming = ((bool)jWeaponMode["DamageOnJamming"] == true) ? TripleBoolean.True : TripleBoolean.False;
      }
      if (jWeaponMode["AOECapable"] != null) {
        this.AOECapable = ((bool)jWeaponMode["AOECapable"] == true) ? TripleBoolean.True : TripleBoolean.False; ;
      }
      if (jWeaponMode["AlwaysIndirectVisuals"] != null) {
        this.AlwaysIndirectVisuals = ((bool)jWeaponMode["AlwaysIndirectVisuals"] == true) ? TripleBoolean.True : TripleBoolean.False;
      }
      if (jWeaponMode["HitGenerator"] != null) {
        try {
          this.HitGenerator = (HitGeneratorType)Enum.Parse(typeof(HitGeneratorType), (string)jWeaponMode["HitGenerator"], true);
        } catch (Exception e) {
          this.HitGenerator = HitGeneratorType.NotSet;
        }
        jWeaponMode.Remove("HitGenerator");
      }
      if (jWeaponMode["statusEffects"] != null) {

        if (jWeaponMode["statusEffects"].Type == JTokenType.Array) {
          this.statusEffects.Clear();
          JToken statusEffects = jWeaponMode["statusEffects"];
          foreach (JObject statusEffect in statusEffects) {
            EffectData effect = new EffectData();
            JSONSerializationUtility.FromJSON<EffectData>(effect, statusEffect.ToString());
            this.statusEffects.Add(effect);
          }
        }
      }
    }
  }
}
