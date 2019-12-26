using BattleTech;
using CustAmmoCategories;
using CustomAmmoCategoriesLog;
using Harmony;
using HBS;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

namespace CustAmmoCategories {
  public static class CustomSoundHelper {
    private static FieldInfo f_activeEventId = typeof(AudioEmitterObject).GetField("activeEventId",BindingFlags.Instance|BindingFlags.NonPublic);
    private static FieldInfo f_audioObject = typeof(AudioEmitterObject).GetField("audioObject", BindingFlags.Instance | BindingFlags.NonPublic);
    private static FieldInfo f_isPlaying = typeof(AudioEmitterObject).GetField("isPlaying", BindingFlags.Instance | BindingFlags.NonPublic);
    private static FieldInfo f_ActiveShortAudioEmitters = typeof(AudioEventManager).GetField("ActiveShortAudioEmitters", BindingFlags.Static | BindingFlags.NonPublic);
    private static FieldInfo f_ActiveLongAudioEmitters = typeof(AudioEventManager).GetField("ActiveLongAudioEmitters", BindingFlags.Static | BindingFlags.NonPublic);
    public static void activeEventId(this AudioEmitterObject audioEmitter, uint value) {
      f_activeEventId.SetValue(audioEmitter, value);
    }
    public static uint activeEventId(this AudioEmitterObject audioEmitter) {
      return (uint)f_activeEventId.GetValue(audioEmitter);
    }
    public static void audioObject(this AudioEmitterObject audioEmitter, AkGameObj value) {
      f_audioObject.SetValue(audioEmitter, value);
    }
    public static AkGameObj audioObject(this AudioEmitterObject audioEmitter) {
      return (AkGameObj)f_audioObject.GetValue(audioEmitter);
    }
    public static void isPlaying(this AudioEmitterObject audioEmitter, bool value) {
      f_isPlaying.SetValue(audioEmitter, value);
    }
    public static bool isPlaying(this AudioEmitterObject audioEmitter) {
      return (bool)f_isPlaying.GetValue(audioEmitter);
    }
    public static void PostEventById(this AudioEmitterObject audioEmitter, uint eventId, Vector3 worldPos, object in_pCookie = null) {
      if (SceneSingletonBehavior<WwiseManager>.HasInstance) {
        audioEmitter.activeEventId(SceneSingletonBehavior<WwiseManager>.Instance.PostEventById(eventId, audioEmitter.audioObject(), (AkCallbackManager.EventCallback)null, (object)null));
        Log.S.TWL(0, "Playing sound by id:" + audioEmitter.activeEventId());
      } else {
        Log.S.TWL(0, "Can't play");
      }
      audioEmitter.isPlaying(true);
    }
    public static void PostEventByName(this AudioEmitterObject audioEmitter, string eventName, Vector3 worldPos, object in_pCookie = null) {
      if (SceneSingletonBehavior<WwiseManager>.HasInstance) {
        audioEmitter.activeEventId(SceneSingletonBehavior<WwiseManager>.Instance.PostEventByName(eventName, audioEmitter.audioObject(), (AkCallbackManager.EventCallback)null, (object)null));
        Log.S.TWL(0, "Playing sound by id:" + audioEmitter.activeEventId());
      } else {
        Log.S.TWL(0, "Can't play");
      }
      audioEmitter.isPlaying(true);
    }
    public static List<AudioEmitterObject> ActiveShortAudioEmitters() {
      return (List<AudioEmitterObject>)f_ActiveShortAudioEmitters.GetValue(null);
    }
    public static List<AudioEmitterObject> ActiveLongAudioEmitters() {
      return (List<AudioEmitterObject>)f_ActiveLongAudioEmitters.GetValue(null);
    }
    public static void SpawnAudioEmitter(string eventName, Vector3 worldPos, bool persistentSound){
      CombatGameState combat = UnityGameInstance.BattleTechGame.Combat;
      Log.S.TWL(0, "SpawnAudioEmitter by id:" + eventName);
      if (!persistentSound && ActiveShortAudioEmitters().Count >= AudioEventManager.AudioConstants.maxShortAudioEmitters)
        return;
      AudioEmitterObject audioEmitterObject;
      if (persistentSound && ActiveLongAudioEmitters().Count >= AudioEventManager.AudioConstants.maxLongAudioEmitters) {
        audioEmitterObject = ActiveLongAudioEmitters()[0];
        ActiveLongAudioEmitters().RemoveAt(0);
        audioEmitterObject.gameObject.transform.position = worldPos;
      } else {
        GameObject gameObject = combat.DataManager.PooledInstantiate(AudioEventManager.AudioConstants.audioEmitterPrefab, BattleTechResourceType.Prefab, new Vector3?(worldPos), new Quaternion?(), (Transform)null);
        gameObject.transform.position = worldPos;
        audioEmitterObject = gameObject.GetComponent<AudioEmitterObject>();
      }
      if ((UnityEngine.Object)audioEmitterObject == (UnityEngine.Object)null)
        return;
      if (persistentSound)
        ActiveLongAudioEmitters().Add(audioEmitterObject);
      else
        ActiveShortAudioEmitters().Add(audioEmitterObject);
      audioEmitterObject.PostEventByName(eventName, worldPos, (object)persistentSound);
    }
  }
}
