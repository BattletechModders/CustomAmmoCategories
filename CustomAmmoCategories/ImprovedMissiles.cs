/*  
 *  This file is part of CustomAmmoCategories.
 *  CustomAmmoCategories is free software: you can redistribute it and/or modify it under the terms of the 
 *  GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, 
 *  or (at your option) any later version. CustomAmmoCategories is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 *  See the GNU Lesser General Public License for more details.
 *  You should have received a copy of the GNU Lesser General Public License along with CustomAmmoCategories. 
 *  If not, see <https://www.gnu.org/licenses/>. 
*/
using BattleTech;
using BattleTech.Rendering;
using CustAmmoCategories;
using CustomAmmoCategoriesLog;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using CustomAmmoCategoriesHelper;
using Random = UnityEngine.Random;

namespace CustAmmoCategoriesPatches {
  [HarmonyPatch(typeof(MissileLauncherEffect))]
  [HarmonyPatch("SetupMissiles")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MissileLauncherEffect_SetupMissiles {
    public static void Postfix(MissileLauncherEffect __instance) {
      Log.Combat?.WL(0,"MissileLauncherEffect.SetupMissiles " + __instance.weapon.defId);
      if (__instance.weapon.isImprovedBallistic() == false) { return; };
      BaseHardPointAnimationController animation = __instance.weapon.HardpointAnimator();
      if (animation != null) { animation.PrefireAnimation(__instance.hitInfo.hitPositions[0], __instance.isIndirect); };
      float firingIntervalM = __instance.weapon.MissileFiringIntervalMultiplier();
      float volleyIntervalM = __instance.weapon.MissileVolleyIntervalMultiplier();
      if (firingIntervalM > CustomAmmoCategories.Epsilon) {
        if (__instance.firingInterval > CustomAmmoCategories.Epsilon) {
          Log.Combat?.W(1, "firingIntervalRate " + __instance.firingIntervalRate + " -> ");
          __instance.firingIntervalRate = (1f / (__instance.firingInterval * firingIntervalM));
          Log.Combat?.WL(1, __instance.firingIntervalRate.ToString());
        }
      }
      if (volleyIntervalM > CustomAmmoCategories.Epsilon) {
        if (__instance.volleyInterval > CustomAmmoCategories.Epsilon) {
          Log.Combat?.W(1, "volleyIntervalRate " + __instance.volleyIntervalRate + " -> ");
          __instance.volleyIntervalRate = (1f / (__instance.volleyInterval * volleyIntervalM));
          Log.Combat?.WL(1, __instance.volleyIntervalRate.ToString());
        }
      }
    }
  }
  public class MissileScaleInfo {
    public Vector3 projectile;
    public MissileScaleInfo(Vector3 pr) {this.projectile = pr;}
  }
  [HarmonyPatch(typeof(MissileLauncherEffect))]
  [HarmonyPatch("Update")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MissileLauncherEffect_Update {
    //public static Dictionary<MissileEffect, MissileScaleInfo> originalScale = new Dictionary<MissileEffect, MissileScaleInfo>();
    public static bool Prefix(MissileLauncherEffect __instance) {
      if ((__instance.currentState == WeaponEffect.WeaponEffectState.PreFiring)&&(__instance.t() >= 1f)) {
        if (__instance.weapon.isImprovedBallistic() == false) { return true; }
        BaseHardPointAnimationController animation = __instance.weapon.HardpointAnimator();
        if (animation == null) { return true; }
        if (animation.isPrefireAnimCompleete()) { return true; }
        return false;
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(WeaponEffect))]
  [HarmonyPatch("InitProjectile")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MissileEffect_InitProjectile {
    public static void Postfix(WeaponEffect __instance) {
      Log.O.TWL(0, "Init projectile:" + __instance.GetType().ToString());
      Log.O.WL(1, "object:" + __instance.GetType().ToString());
      Log.O.printComponents(__instance.gameObject,1);
      Log.O.WL(1, "projectile:");
      Log.O.printComponents(__instance.projectile, 0);
      MissileEffect missile = __instance as MissileEffect;
      if (missile == null) { return; };
      Log.Combat?.WL(0,"MissileEffect.InitProjectile " + __instance.weapon.defId);
      if (__instance.weapon.isImprovedBallistic() == false) { return; };
      if (missile.projectile != null) {
        ColorChangeRule colorChangeRule = __instance.weapon.colorChangeRule();
        List<ColorTableJsonEntry> colorsTable = __instance.weapon.ColorsTable();
        int colorIndex = -1;
        if (colorsTable.Count > 0) {
          switch (colorChangeRule) {
            case ColorChangeRule.Linear: colorIndex = 0; break;
            case ColorChangeRule.Random: colorIndex = Random.Range(0, colorsTable.Count); break;
            case ColorChangeRule.RandomOnce: colorIndex = Random.Range(0, colorsTable.Count); break;
            case ColorChangeRule.None: colorIndex = -1; break;
            default:
              colorIndex = ((int)colorChangeRule - (int)ColorChangeRule.t0) % colorsTable.Count;
              break;
          }
        }
        Component[] components = missile.projectile.GetComponentsInChildren<Component>();
        foreach(Component component in components) {
          Log.Combat?.WL(0, component.name+":"+component.GetType().ToString());
        }
        ParticleSystem[] psyss = missile.projectile.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem psys in psyss) {
          psys.RegisterRestoreScale();
          var main = psys.main;
          main.scalingMode = ParticleSystemScalingMode.Hierarchy;
          Log.Combat?.WL(0, psys.name + ":" + psys.main.scalingMode);
        }
        ParticleSystemRenderer[] renderers = missile.projectile.GetComponentsInChildren<ParticleSystemRenderer>();
        foreach (ParticleSystemRenderer renderer in renderers) {
          Log.Combat?.WL(0, renderer.name + ": materials");
          foreach (Material material in renderer.materials) {
            if (colorIndex >= 0) {
              if (material.name.StartsWith("vfxMatPrtl_missileTrail_alpha")) {
                material.RegisterRestoreColor();
                material.SetColor("_ColorBB", colorsTable[colorIndex].Color);
              }
            }
            Log.Combat?.WL(2, material.name + ": " + material.shader + ": "+material.GetColor("_ColorBB"));
          }
        }
        Log.Combat?.W(1, "missile.projectileTransform.localScale " + missile.projectile.transform.localScale + " -> ");
        CustomVector scale = __instance.weapon.ProjectileScale();
        if (scale.set) {
          missile.projectile.RegisterRestoreScale();
          missile.projectile.transform.localScale = scale.vector;
        }
        Log.Combat?.WL(1, missile.projectile.transform.localScale.ToString());
      }
    }
  }
  [HarmonyPatch(typeof(MissileLauncherEffect))]
  [HarmonyPatch("ClearMissiles")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MissileLauncherEffect_ClearMissiles {
    public static bool Prefix(MissileLauncherEffect __instance) {
      Log.Combat?.WL(0,"MissileEffect.ClearMissiles " + __instance.weapon.defId);
      if (__instance.weapon.isImprovedBallistic() == false) { return true; };
      //foreach(MissileEffect missile in __instance.missiles) {
      //missile.restoreScale();
      //}
      BaseHardPointAnimationController animation = __instance.weapon.HardpointAnimator();
      if (animation != null) { animation.PostfireAnimation(); };
      Log.Combat?.WL(1, "clearing volley info");
      __instance.ClearVolleyInfo();
      return true;
    }
  }
  [HarmonyPatch(typeof(MissileEffect))]
  [HarmonyPatch("OnComplete")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MissileEffect_OnComplete {
    public static void Postfix(MissileEffect __instance) {
      Log.Combat?.WL(0, "MissileEffect.OnComplete " + __instance.weapon.defId);
      if (__instance.weapon.isImprovedBallistic() == false) { return; };
      //__instance.restoreScale();
      return;
    }
  }
  [HarmonyPatch(typeof(WeaponEffect))]
  [HarmonyPatch("DestroyFlimsyObjects")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MissileEffect_DestroyFlimsyObjects {
    public static void Prefix(ref bool __runOriginal, MissileEffect __instance) {
      if (!__runOriginal) { return; }
      Log.Combat?.WL(0, "MissileEffect.DestroyFlimsyObjects " + __instance.weapon.defId);
      AdvWeaponHitInfoRec cached = __instance.hitInfo.advRec(__instance.hitIndex);
      if (cached == null) { return; }
      if (cached.interceptInfo.Intercepted) {
        Log.Combat?.WL(1, "intercepted. not DestroyFlimsyObjects");
        __runOriginal = false;
        return;
      }
      return;
    }
  }
  [HarmonyPatch(typeof(MissileEffect))]
  [HarmonyPatch("PlayImpact")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MissileEffect_PlayImpact {
    public static void Postfix(MissileEffect __instance) {
      Log.Combat?.WL(0, "MissileEffect.PlayImpact " + __instance.weapon.defId);
      AdvWeaponHitInfoRec cached = __instance.hitInfo.advRec(__instance.hitIndex);
      if (cached == null) { return; }
      if (cached.interceptInfo.Intercepted) {
        string str1 = "_" + __instance.impactVFXVariations[Random.Range(0, __instance.impactVFXVariations.Length)];
        string str2 = string.Empty;
        string eVFX = string.Format("{0}{1}{2}", (object)__instance.impactVFXBase, (object)str1, (object)str2);
        Log.Combat?.WL(1, "intercepted. Spawning explosive VFX " + eVFX);
        __instance.SpawnImpactExplosion(eVFX);
      }
    }
  }
  [HarmonyPatch(typeof(MissileEffect))]
  [HarmonyPatch("SpawnImpactExplosion")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class MissileEffect_SpawnImpactExplosion {
    public static bool Prefix(MissileEffect __instance, string explosionName) {
      Log.Combat?.WL(0, "MissileEffect.SpawnImpactExplosion " + __instance.weapon.defId);
      if (__instance.weapon.isImprovedBallistic() == false) { return true; }
      GameObject gameObject = __instance.weapon.parent.Combat.DataManager.PooledInstantiate(explosionName, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
      if (gameObject == null) {
        Log.Combat?.WL(0, "Missile impact had an invalid explosion prefab : " + explosionName,true);
      } else {
        __instance.ScaleWeaponEffect(gameObject);
        ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
        BTLight componentInChildren1 = gameObject.GetComponentInChildren<BTLight>(true);
        BTWindZone componentInChildren2 = gameObject.GetComponentInChildren<BTWindZone>(true);
        component.Stop(true);
        component.Clear(true);
        component.transform.position = __instance.endPos();
        component.transform.LookAt(__instance.preFireEndPos);
        BTCustomRenderer.SetVFXMultiplier(component);
        component.Play(true);
        if (componentInChildren1 != null) {
          componentInChildren1.contributeVolumetrics = true;
          componentInChildren1.volumetricsMultiplier = 1000f;
          componentInChildren1.intensity = __instance.impactLightIntensity;
          componentInChildren1.FadeIntensity(0.0f, 0.5f);
          componentInChildren1.RefreshLightSettings(true);
        }
        if (componentInChildren2 != null)
          componentInChildren2.PlayAnimCurve();
        AutoPoolObject autoPoolObject = gameObject.GetComponent<AutoPoolObject>();
        if (autoPoolObject == null)
          autoPoolObject = gameObject.AddComponent<AutoPoolObject>();
        autoPoolObject.Init(__instance.weapon.parent.Combat.DataManager, explosionName, component);
        gameObject.transform.rotation = Random.rotationUniform;
        if (__instance.isSRM) {
          int num1 = (int)WwiseManager.PostEvent<AudioEventList_explosion>(AudioEventList_explosion.explosion_large, __instance.projectileAudioObject(), (AkCallbackManager.EventCallback)null, (object)null);
        } else {
          int num2 = (int)WwiseManager.PostEvent<AudioEventList_explosion>(AudioEventList_explosion.explosion_medium, __instance.projectileAudioObject(), (AkCallbackManager.EventCallback)null, (object)null);
        }
        __instance.DestroyFlimsyObjects();
      }
      return false;
    }
  }
  public class MissileLauncherVolleyInfo {
    public int missileVolleyId;
    public int misileVolleySize;
    public MissileLauncherVolleyInfo(int vs) {
      missileVolleyId = 0;
      misileVolleySize = vs;
    }
  }
  [HarmonyPatch(typeof(MissileLauncherEffect))]
  [HarmonyPatch("FireNextMissile")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MissileLauncherEffect_FireNextMissile {
    private static Dictionary<MissileLauncherEffect, MissileLauncherVolleyInfo> launcherVolleyInfo = new Dictionary<MissileLauncherEffect, MissileLauncherVolleyInfo>();
    public static MissileLauncherVolleyInfo volleyInfo(this MissileLauncherEffect launcher) {
      if (launcherVolleyInfo.ContainsKey(launcher) == false) {
        int volleySize = launcher.weapon.MissileVolleySize();
        if (volleySize == 0) { volleySize = launcher.numberOfEmitters(); };
        launcherVolleyInfo.Add(launcher,new MissileLauncherVolleyInfo(volleySize));
      }
      return launcherVolleyInfo[launcher];
    }
    public static void ClearVolleyInfo(this MissileLauncherEffect launcher) {
      if (launcherVolleyInfo.ContainsKey(launcher) != false) {
        launcherVolleyInfo.Remove(launcher);
      }
    }
    public static void FireNextVolley_I(this MissileLauncherEffect launcher) {
      Log.Combat?.WL(0, "MissileLauncherEffect.FireNextVolley improved " + launcher.weapon.defId);
      launcher.t(0f);// this.t = 0.0f;
      launcher.rate(launcher.volleyIntervalRate); // this.rate = this.volleyIntervalRate;
      launcher.volleyInfo().missileVolleyId = 0; //this.emitterIndex = 0;
      if ((double)launcher.rate() >= 0.00999999977648258)
        return;
      launcher.FireNextMissile();
    }
    public static void Prefix(ref bool __runOriginal, MissileLauncherEffect __instance) {
      if (!__runOriginal) { return; }
      Log.Combat?.WL(0,"MissileLauncherEffect.FireNextMissile " + __instance.weapon.defId);
      try {
        if (__instance.weapon.isImprovedBallistic() == false) { return; }
        __instance.emitterIndex(__instance.emitterIndex() % __instance.numberOfEmitters());
        Log.Combat?.WL(1, "hitIndex: " + __instance.hitIndex + "/" + __instance.hitInfo.numberOfShots + " emmiter: " + __instance.emitterIndex() + "/" + __instance.numberOfEmitters() + " volley: " + __instance.volleyInfo().missileVolleyId + "/" + __instance.volleyInfo().misileVolleySize);
        if (__instance.hitIndex < __instance.hitInfo.numberOfShots && __instance.volleyInfo().missileVolleyId < __instance.volleyInfo().misileVolleySize) {
          __instance.LaunchMissile();
          ++(__instance.volleyInfo().missileVolleyId);
        }
        if (__instance.hitIndex >= __instance.hitInfo.numberOfShots) {
          if (__instance.isSRM) {
            int num1 = (int)WwiseManager.PostEvent<AudioEventList_srm>(AudioEventList_srm.srm_launcher_end, __instance.parentAudioObject(), (AkCallbackManager.EventCallback)null, (object)null);
          } else {
            int num2 = (int)WwiseManager.PostEvent<AudioEventList_lrm>(AudioEventList_lrm.lrm_launcher_end, __instance.parentAudioObject(), (AkCallbackManager.EventCallback)null, (object)null);
          }
          __instance.currentState = WeaponEffect.WeaponEffectState.WaitingForImpact;
        } else {
          if (__instance.volleyInfo().missileVolleyId >= __instance.volleyInfo().misileVolleySize) {
            __instance.FireNextVolley_I();
          }
        }
        __runOriginal = false; return;
      }catch(Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        CombatGameState.gameInfoLogger.LogException(e);
      }
    }
  }
}