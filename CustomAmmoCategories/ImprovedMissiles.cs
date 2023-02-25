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
using Random = UnityEngine.Random;

namespace CustAmmoCategoriesPatches {
  /*[HarmonyPatch(typeof(MissileEffect))]
  [HarmonyPatch("PlayProjectile")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MissileEffect_Init {
    private static FieldInfo fParentLauncher = null;
    private static bool Prepare() {
      fParentLauncher = typeof(MissileEffect).GetField("parentLauncher", BindingFlags.Instance | BindingFlags.NonPublic);
      if (fParentLauncher == null) {
        Log.LogWrite("Can't find MissileEffect.parentLauncher", true);
        return false;
      }
      return true;
    }
    public static MissileLauncherEffect parentLauncher(this MissileEffect missile) {
      if (fParentLauncher == null) { return null; }
      return (MissileLauncherEffect)(fParentLauncher.GetValue(missile));
    }
    //public static HashSet<WeaponEffect> weaponEffects = new HashSet<WeaponEffect>();
    //public static void Clear() { weaponEffects.Clear(); }
    public static void Postfix(MissileEffect __instance) {
      Log.LogWrite("MissileEffect.PlayProjectile " + __instance.weapon.defId + " "+__instance.name+"\n");
      try {
        if (__instance.weapon.isImprovedBallistic() == false) { return; };
          Log.LogWrite(" projectileSpeed " + __instance.projectileSpeed + " -> ");
          __instance.projectileSpeed = parentLauncher.projectileSpeed * weapon.ProjectileSpeedMultiplier();
          float max = __instance.projectileSpeed * 0.1f;
          __instance.projectileSpeed += Random.Range(-max, max);
          Log.LogWrite(" " + __instance.projectileSpeed + "\n");
        }
      }catch(Exception e) {
        Log.LogWrite(e.ToString() + "\n");
      }
    }
  }*/
  [HarmonyPatch(typeof(MissileLauncherEffect))]
  [HarmonyPatch("SetupMissiles")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MissileLauncherEffect_SetupMissiles {
    private static FieldInfo ffiringIntervalRate = null;
    private static FieldInfo fvolleyIntervalRate = null;
    private static bool Prepare() {
      ffiringIntervalRate = typeof(MissileLauncherEffect).GetField("firingIntervalRate", BindingFlags.Instance | BindingFlags.NonPublic);
      if (ffiringIntervalRate == null) {
        Log.LogWrite("Can't find MissileLauncherEffect.firingIntervalRate", true);
        return false;
      }
      fvolleyIntervalRate = typeof(MissileLauncherEffect).GetField("volleyIntervalRate", BindingFlags.Instance | BindingFlags.NonPublic);
      if (fvolleyIntervalRate == null) {
        Log.LogWrite("Can't find MissileLauncherEffect.volleyIntervalRate", true);
        return false;
      }
      return true;
    }
    public static float firingIntervalRate(this MissileLauncherEffect launcher) {
      if (ffiringIntervalRate == null) { return 0f; }
      return (float)ffiringIntervalRate.GetValue(launcher);
    }
    public static float volleyIntervalRate(this MissileLauncherEffect launcher) {
      if (fvolleyIntervalRate == null) { return 0f; }
      return (float)fvolleyIntervalRate.GetValue(launcher);
    }
    public static void firingIntervalRate(this MissileLauncherEffect launcher, float value) {
      if (ffiringIntervalRate == null) { return; }
      ffiringIntervalRate.SetValue(launcher, value);
    }
    public static void volleyIntervalRate(this MissileLauncherEffect launcher, float value) {
      if (fvolleyIntervalRate == null) { return; }
      fvolleyIntervalRate.SetValue(launcher, value);
    }
    //public static HashSet<WeaponEffect> weaponEffects = new HashSet<WeaponEffect>();
    //public static void Clear() { weaponEffects.Clear(); }
    public static void Postfix(MissileLauncherEffect __instance, bool ___isIndirect) {
      Log.LogWrite("MissileLauncherEffect.SetupMissiles " + __instance.weapon.defId + "\n");
      if (__instance.weapon.isImprovedBallistic() == false) { return; };
      BaseHardPointAnimationController animation = __instance.weapon.HardpointAnimator();
      if (animation != null) { animation.PrefireAnimation(__instance.hitInfo.hitPositions[0], ___isIndirect); };
      float firingIntervalM = __instance.weapon.MissileFiringIntervalMultiplier();
      float volleyIntervalM = __instance.weapon.MissileVolleyIntervalMultiplier();
      if (firingIntervalM > CustomAmmoCategories.Epsilon) {
        if (__instance.firingInterval > CustomAmmoCategories.Epsilon) {
          Log.LogWrite(" firingIntervalRate " + __instance.firingIntervalRate() + " -> ");
          __instance.firingIntervalRate(1f / (__instance.firingInterval * firingIntervalM));
          Log.LogWrite(" " + __instance.firingIntervalRate() + "\n");
        }
      }
      if (volleyIntervalM > CustomAmmoCategories.Epsilon) {
        if (__instance.volleyInterval > CustomAmmoCategories.Epsilon) {
          Log.LogWrite(" volleyIntervalRate " + __instance.volleyIntervalRate() + " -> ");
          __instance.volleyIntervalRate(1f / (__instance.volleyInterval * volleyIntervalM));
          Log.LogWrite(" " + __instance.volleyIntervalRate() + "\n");
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
    private static FieldInfo fprojectileTransform = null;
    //public static Dictionary<MissileEffect, MissileScaleInfo> originalScale = new Dictionary<MissileEffect, MissileScaleInfo>();
    private static bool Prepare() {
      fprojectileTransform = typeof(WeaponEffect).GetField("projectileTransform", BindingFlags.Instance | BindingFlags.NonPublic);
      if (fprojectileTransform == null) {
        Log.LogWrite("Can't find WeaponEffect.projectileTransform", true);
        return false;
      }
      return true;
    }
    public static Transform projectileTransform(this MissileEffect missile) {
      if (fprojectileTransform == null) { return null; }
      return (Transform)fprojectileTransform.GetValue(missile);
    }
    /*public static void restoreScale(this MissileEffect missile) {
      if (MissileEffect_InitProjectile.originalScale.ContainsKey(missile) == false) { return; }
      Log.LogWrite("MissileEffect.restoreScale " + missile.weapon.defId + "\n");
      Log.LogWrite(" missile.projectileTransform.localScale " + missile.projectileTransform().localScale + " -> ");
      Vector3 scale = MissileEffect_InitProjectile.originalScale[missile].projectile;
      missile.projectileTransform().localScale = scale;
      Log.LogWrite(" " + missile.projectileTransform().localScale + "\n");
      MissileEffect_InitProjectile.originalScale.Remove(missile);
    }*/
    public static void Postfix(WeaponEffect __instance) {
      Log.O.TWL(0, "Init projectile:" + __instance.GetType().ToString());
      Log.O.WL(1, "object:" + __instance.GetType().ToString());
      Log.O.printComponents(__instance.gameObject,1);
      Log.O.WL(1, "projectile:");
      Log.O.printComponents(__instance.projectile, 0);
      MissileEffect missile = __instance as MissileEffect;
      if (missile == null) { return; };
      Log.LogWrite("MissileEffect.InitProjectile " + __instance.weapon.defId + "\n");
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
          Log.LogWrite(" "+component.name+":"+component.GetType().ToString()+"\n");
        }
        ParticleSystem[] psyss = missile.projectile.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem psys in psyss) {
          psys.RegisterRestoreScale();
          var main = psys.main;
          main.scalingMode = ParticleSystemScalingMode.Hierarchy;
          Log.LogWrite(" " + psys.name + ":" + psys.main.scalingMode + "\n");
        }
        ParticleSystemRenderer[] renderers = missile.projectile.GetComponentsInChildren<ParticleSystemRenderer>();
        foreach (ParticleSystemRenderer renderer in renderers) {
          Log.LogWrite(" " + renderer.name + ": materials\n");
          foreach (Material material in renderer.materials) {
            if (colorIndex >= 0) {
              if (material.name.StartsWith("vfxMatPrtl_missileTrail_alpha")) {
                material.RegisterRestoreColor();
                material.SetColor("_ColorBB", colorsTable[colorIndex].Color);
              }
            }
            Log.LogWrite("  " + material.name + ": " + material.shader + ": "+material.GetColor("_ColorBB")+"\n");
          }
        }
        Log.LogWrite(" missile.projectileTransform.localScale " + missile.projectile.transform.localScale + " -> ");
        CustomVector scale = __instance.weapon.ProjectileScale();
        if (scale.set) {
          missile.projectile.RegisterRestoreScale();
          missile.projectile.transform.localScale = scale.vector;
        }
        Log.LogWrite(" " + missile.projectile.transform.localScale + "\n");
      }
    }
  }
  [HarmonyPatch(typeof(MissileLauncherEffect))]
  [HarmonyPatch("ClearMissiles")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MissileLauncherEffect_ClearMissiles {
    public static bool Prefix(MissileLauncherEffect __instance) {
      Log.LogWrite("MissileEffect.ClearMissiles " + __instance.weapon.defId + "\n");
      if (__instance.weapon.isImprovedBallistic() == false) { return true; };
      //foreach(MissileEffect missile in __instance.missiles) {
      //missile.restoreScale();
      //}
      BaseHardPointAnimationController animation = __instance.weapon.HardpointAnimator();
      if (animation != null) { animation.PostfireAnimation(); };
      Log.LogWrite(" clearing volley info\n");
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
      Log.LogWrite("MissileEffect.OnComplete " + __instance.weapon.defId + "\n");
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
    public static bool Prefix(MissileEffect __instance, int ___hitIndex) {
      Log.LogWrite("MissileEffect.DestroyFlimsyObjects " + __instance.weapon.defId + "\n");
      AdvWeaponHitInfoRec cached = __instance.hitInfo.advRec(___hitIndex);
      if (cached == null) { return true; }
      if (cached.interceptInfo.Intercepted) {
        Log.LogWrite(" intercepted. not DestroyFlimsyObjects\n");
        return false;
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(MissileEffect))]
  [HarmonyPatch("PlayImpact")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MissileEffect_PlayImpact {
    public static void Postfix(MissileEffect __instance, int ___hitIndex) {
      Log.LogWrite("MissileEffect.PlayImpact " + __instance.weapon.defId + "\n");
      AdvWeaponHitInfoRec cached = __instance.hitInfo.advRec(___hitIndex);
      if (cached == null) { return; }
      if (cached.interceptInfo.Intercepted) {
        string str1 = "_" + __instance.impactVFXVariations[Random.Range(0, __instance.impactVFXVariations.Length)];
        string str2 = string.Empty;
        string eVFX = string.Format("{0}{1}{2}", (object)__instance.impactVFXBase, (object)str1, (object)str2);
        Log.LogWrite(" intercepted. Spawning explosive VFX "+eVFX+"\n");
        typeof(MissileEffect).GetMethod("SpawnImpactExplosion", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance,new object[1] { eVFX });
      }
    }
  }
  /*[HarmonyPatch(typeof(MissileEffect))]
  [HarmonyPatch("Fire")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MissileEffect_Fire {
    public static void Postfix(MissileEffect __instance, int ___hitIndex) {
      Log.LogWrite("MissileEffect.Fire " + __instance.weapon.defId + "\n");
      AdvWeaponHitInfoRec cached = __instance.hitInfo.advRec(___hitIndex);
      if (cached == null) { return; }
      if (cached.interceptInfo.Intercepted) {
        string str1 = "_" + __instance.impactVFXVariations[Random.Range(0, __instance.impactVFXVariations.Length)];
        string str2 = string.Empty;
        string eVFX = string.Format("{0}{1}{2}", (object)__instance.impactVFXBase, (object)str1, (object)str2);
        Log.LogWrite(" intercepted. Spawning explosive VFX " + eVFX + "\n");
        typeof(MissileEffect).GetMethod("SpawnImpactExplosion", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance, new object[1] { eVFX });
      }
    }
  }*/
  [HarmonyPatch(typeof(MissileEffect))]
  [HarmonyPatch("SpawnImpactExplosion")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class MissileEffect_SpawnImpactExplosion {
    //public static Dictionary<string, Vector3> defaultExplosiveScale = new Dictionary<string, Vector3>();
    public static bool Prefix(MissileEffect __instance, string explosionName, int ___hitIndex, AkGameObj ___projectileAudioObject,Vector3 ___endPos,Vector3 ___preFireEndPos,bool ___isSRM) {
      Log.LogWrite("MissileEffect.SpawnImpactExplosion " + __instance.weapon.defId + "\n");
      if (__instance.weapon.isImprovedBallistic() == false) { return true; }
      GameObject gameObject = __instance.weapon.parent.Combat.DataManager.PooledInstantiate(explosionName, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
      if ((UnityEngine.Object)gameObject == (UnityEngine.Object)null) {
        Log.LogWrite("Missile impact had an invalid explosion prefab : " + explosionName+"\n",true);
      } else {
        __instance.ScaleWeaponEffect(gameObject);
        ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
        BTLight componentInChildren1 = gameObject.GetComponentInChildren<BTLight>(true);
        BTWindZone componentInChildren2 = gameObject.GetComponentInChildren<BTWindZone>(true);
        component.Stop(true);
        component.Clear(true);
        /*if(defaultExplosiveScale.ContainsKey(explosionName) == false) {
          defaultExplosiveScale.Add(explosionName, component.transform.localScale);
        }*/
        //component.transform.localScale = defaultExplosiveScale[explosionName];
        /*CustomVector scale = __instance.weapon.MissileExplosionScale();
        if (scale.set) {
          Log.LogWrite(" updating explosive "+explosionName+" scale "+ gameObject.transform.localScale+" -> ");
          gameObject.transform.localScale = scale.vector;
          Log.LogWrite(" " + gameObject.transform.localScale + "\n");
        };*/
        component.transform.position = ___endPos;
        component.transform.LookAt(___preFireEndPos);
        BTCustomRenderer.SetVFXMultiplier(component);
        component.Play(true);
        if ((UnityEngine.Object)componentInChildren1 != (UnityEngine.Object)null) {
          componentInChildren1.contributeVolumetrics = true;
          componentInChildren1.volumetricsMultiplier = 1000f;
          componentInChildren1.intensity = __instance.impactLightIntensity;
          componentInChildren1.FadeIntensity(0.0f, 0.5f);
          componentInChildren1.RefreshLightSettings(true);
        }
        if ((UnityEngine.Object)componentInChildren2 != (UnityEngine.Object)null)
          componentInChildren2.PlayAnimCurve();
        AutoPoolObject autoPoolObject = gameObject.GetComponent<AutoPoolObject>();
        if ((UnityEngine.Object)autoPoolObject == (UnityEngine.Object)null)
          autoPoolObject = gameObject.AddComponent<AutoPoolObject>();
        autoPoolObject.Init(__instance.weapon.parent.Combat.DataManager, explosionName, component);
        gameObject.transform.rotation = Random.rotationUniform;
        if (___isSRM) {
          int num1 = (int)WwiseManager.PostEvent<AudioEventList_explosion>(AudioEventList_explosion.explosion_large, ___projectileAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
        } else {
          int num2 = (int)WwiseManager.PostEvent<AudioEventList_explosion>(AudioEventList_explosion.explosion_medium, ___projectileAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
        }
        typeof(WeaponEffect).GetMethod("DestroyFlimsyObjects", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[0] { });
        //__instance.DestroyFlimsyObjects();
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
    private static FieldInfo fnumberOfEmitters = null;
    private static MethodInfo mLaunchMissile = null;
    private static MethodInfo mFireNextMissile = null;
    private static FieldInfo ft = null;
    private static FieldInfo frate = null;
    public static bool Prepare() {
      fnumberOfEmitters = typeof(WeaponEffect).GetField("numberOfEmitters", BindingFlags.Instance | BindingFlags.NonPublic);
      if (fnumberOfEmitters == null) {
        Log.LogWrite("Can't find WeaponEffect.numberOfEmitters", true);
        return false;
      }
      ft = typeof(WeaponEffect).GetField("t", BindingFlags.Instance | BindingFlags.NonPublic);
      if (ft == null) {
        Log.LogWrite("Can't find WeaponEffect.t", true);
        return false;
      }
      frate = typeof(WeaponEffect).GetField("rate", BindingFlags.Instance | BindingFlags.NonPublic);
      if (frate == null) {
        Log.LogWrite("Can't find WeaponEffect.rate", true);
        return false;
      }
      mLaunchMissile = typeof(MissileLauncherEffect).GetMethod("LaunchMissile", BindingFlags.Instance | BindingFlags.NonPublic);
      if (mLaunchMissile == null) {
        Log.LogWrite("Can't find MissileLauncherEffect.LaunchMissile", true);
        return false;
      }
      mFireNextMissile = typeof(MissileLauncherEffect).GetMethod("FireNextMissile", BindingFlags.Instance | BindingFlags.NonPublic);
      if (mFireNextMissile == null) {
        Log.LogWrite("Can't find MissileLauncherEffect.FireNextMissile", true);
        return false;
      }
      return true;
    }
    public static void t(this MissileLauncherEffect launcher, float value) {
      if (ft == null) { return; }
      ft.SetValue(launcher,value);
    }
    public static float t(this MissileLauncherEffect launcher) {
      if (ft == null) { return 0f; }
      return (float)ft.GetValue(launcher);
    }
    public static void rate(this WeaponEffect launcher, float value) {
      if (frate == null) { return; }
      frate.SetValue(launcher, value);
    }
    public static float rate(this WeaponEffect launcher) {
      if (frate == null) { return 0f; }
      return (float)frate.GetValue(launcher);
    }
    public static int numberOfEmitters(this MissileLauncherEffect launcher) {
      if (fnumberOfEmitters == null) { return 0; }
      return (int)fnumberOfEmitters.GetValue(launcher);
    }
    public static void LaunchMissile(this MissileLauncherEffect launcher) {
      if (mLaunchMissile == null) { return; }
      mLaunchMissile.Invoke(launcher, new object[0] { });
    }
    public static void FireNextMissile(this MissileLauncherEffect launcher) {
      if (mFireNextMissile == null) { return; }
      mFireNextMissile.Invoke(launcher, new object[0] { });
    }
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
    public static void FireNextVolley(this MissileLauncherEffect launcher) {
      Log.LogWrite("MissileLauncherEffect.FireNextVolley improved " + launcher.weapon.defId + "\n");
      launcher.t(0f);// this.t = 0.0f;
      launcher.rate(launcher.volleyIntervalRate()); // this.rate = this.volleyIntervalRate;
      launcher.volleyInfo().missileVolleyId = 0; //this.emitterIndex = 0;
      if ((double)launcher.rate() >= 0.00999999977648258)
        return;
      launcher.FireNextMissile();
    }
    public static bool Prefix(MissileLauncherEffect __instance, int ___hitIndex, ref int ___emitterIndex, AkGameObj ___parentAudioObject) {
      Log.LogWrite("MissileLauncherEffect.FireNextMissile " + __instance.weapon.defId + "\n");
      if (__instance.weapon.isImprovedBallistic() == false) { return true; }
      ___emitterIndex = ___emitterIndex % __instance.numberOfEmitters();
      Log.LogWrite(" hitIndex: "+___hitIndex+"/"+__instance.hitInfo.numberOfShots+" emmiter: "+___emitterIndex+"/"+ __instance.numberOfEmitters() + " volley: "+__instance.volleyInfo().missileVolleyId+"/"+__instance.volleyInfo().misileVolleySize+"\n");
      if (___hitIndex < __instance.hitInfo.numberOfShots && __instance.volleyInfo().missileVolleyId < __instance.volleyInfo().misileVolleySize) {
        __instance.LaunchMissile();
        ++(__instance.volleyInfo().missileVolleyId);
      }
      if (___hitIndex >= __instance.hitInfo.numberOfShots) {
        if (__instance.isSRM) {
          int num1 = (int)WwiseManager.PostEvent<AudioEventList_srm>(AudioEventList_srm.srm_launcher_end, ___parentAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
        } else {
          int num2 = (int)WwiseManager.PostEvent<AudioEventList_lrm>(AudioEventList_lrm.lrm_launcher_end, ___parentAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
        }
        __instance.currentState = WeaponEffect.WeaponEffectState.WaitingForImpact;
      } else {
        if (__instance.volleyInfo().missileVolleyId >= __instance.volleyInfo().misileVolleySize) {
          __instance.FireNextVolley();
        }
      }
      return false;
    }
  }
}