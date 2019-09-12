using BattleTech;
using CustAmmoCategories;
using System.Collections.Generic;

namespace CleverGirlAIDamagePrediction {
  public class DamagePredictionRecord {
    public float Normal { get; set; }
    public float Heat { get; set; }
    public float Instability { get; set; }
    public float ClusterCoeff { get; set; }
    public List<int> PossibleHitLocations { get; set; }
    public List<EffectData> ApplyEffects { get; set; }
    public ICombatant Target { get; set; }
    public float ToHit { get; set; }
    public DamagePredictionRecord() {
      PossibleHitLocations = new List<int>();
      ApplyEffects = new List<EffectData>();
    }
  }
  public class WeaponFirePredictedEffect {
    public AmmunitionBox ammoBox { get; set; }
    public Weapon weapon { get; set; }
    public WeaponMode mode { get; set; }
    public List<DamagePredictionRecord> predictDamage { get; set; }
    public WeaponFirePredictedEffect() {
      ammoBox = null;
      weapon = null;
      mode = null;
      predictDamage = new List<DamagePredictionRecord>();
    }
  }
}