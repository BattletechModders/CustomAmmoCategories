using BattleTech;
using UnityEngine;

public class AdvHitInfoRecord {
  public int hitLocation;
  public Vector3 hitPosition;
  public float hitDamage;
  public int hitHeat;
  public int hitIndex;
  public ICombatant target;
  public Weapon weapon;
  public bool isAoE;
  public bool needAoEProcessing;
  public int attackSequenceId;
  public int groupIdx;
  public int weaponIdx;
  public static CombatGameState Combat = null;
}