using BattleTech;
using BattleTech.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CustomAmmoCategoriesHelper{
  public static class Core {
    public static void DestroyFlimsyObjects(this WeaponEffect effect) { effect.DestroyFlimsyObjects(); }
    public static void OnComplete(this WeaponEffect effect) { effect.OnComplete(); }
    public static CombatGameState Combat(this WeaponEffect effect) { return effect.Combat; }
    public static int emitterIndex(this WeaponEffect effect) { return effect.emitterIndex; }
    public static void emitterIndex(this WeaponEffect effect, int val) { effect.emitterIndex = val; }
    public static int numberOfEmitters(this WeaponEffect effect) { return effect.numberOfEmitters; }
    public static AkGameObj parentAudioObject(this WeaponEffect effect) { return effect.parentAudioObject; }
    public static Transform startingTransform(this WeaponEffect effect) { return effect.startingTransform; }
    public static Vector3 startPos(this WeaponEffect effect) { return effect.startPos; }
    public static void startPos(this WeaponEffect effect, Vector3 val) { effect.startPos = val; }
    public static Vector3 endPos(this WeaponEffect effect) { return effect.endPos; }
    public static void endPos(this WeaponEffect effect, Vector3 val) { effect.endPos = val; }
    public static Vector3 currentPos(this WeaponEffect effect) { return effect.currentPos; }
    public static float t(this WeaponEffect effect) { return effect.t; }
    public static void t(this WeaponEffect effect, float val) { effect.t = val; }
    public static float rate(this WeaponEffect effect) { return effect.rate; }
    public static void rate(this WeaponEffect effect, float val) { effect.rate = val; }
    public static void FiringComplete(this WeaponEffect effect, bool val) { effect.FiringComplete = val; }
    public static float preFireRate(this WeaponEffect effect) { return effect.preFireRate; }
    public static float duration(this WeaponEffect effect) { return effect.duration; }
    public static string activeProjectileName(this WeaponEffect effect) { return effect.activeProjectileName; }
    public static Transform projectileTransform(this WeaponEffect effect) { return effect.projectileTransform; }
    public static ParticleSystem projectileParticles(this WeaponEffect effect) { return effect.projectileParticles; }
    public static AkGameObj projectileAudioObject(this WeaponEffect effect) { return effect.projectileAudioObject; }
    public static GameObject projectileMeshObject(this WeaponEffect effect) { return effect.projectileMeshObject; }
    public static GameObject projectileLightObject(this WeaponEffect effect) { return effect.projectileLightObject; }
    public static bool hasSentNextWeaponMessage(this WeaponEffect effect) { return effect.hasSentNextWeaponMessage; }
    public static void hasSentNextWeaponMessage(this WeaponEffect effect, bool val) { effect.hasSentNextWeaponMessage = val; }
    public static float attackSequenceNextDelayTimer(this WeaponEffect effect) { return effect.attackSequenceNextDelayTimer; }
    public static bool ShouldShowWeaponsUI(this SelectionState state) { return state.ShouldShowWeaponsUI; }
    public static bool ShouldShowTargetingLines(this SelectionState state) { return state.ShouldShowTargetingLines; }
    public static bool shouldUseMultiTargetLines(this SelectionState state) { return state.shouldUseMultiTargetLines; }
    public static void ResetAbilityButton_public(this CombatHUDMechwarriorTray tray, AbstractActor actor, CombatHUDActionButton button, Ability ability, bool forceInactive) {
      tray.ResetAbilityButton(actor, button, ability, forceInactive);
    }
  }
}
