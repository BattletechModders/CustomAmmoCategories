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
  public class CustomSoundBankDef {
    public string name { get; set; }
    public string filename { get; set; }
    public Dictionary<string, uint> events { get; set; }
    public CustomSoundBankDef() { events = new Dictionary<string, uint>(); }
  }
  public static class CustomSoundHelper {
    private static FieldInfo f_activeEventId = typeof(AudioEmitterObject).GetField("activeEventId",BindingFlags.Instance|BindingFlags.NonPublic);
    private static FieldInfo f_audioObject = typeof(AudioEmitterObject).GetField("audioObject", BindingFlags.Instance | BindingFlags.NonPublic);
    private static FieldInfo f_isPlaying = typeof(AudioEmitterObject).GetField("isPlaying", BindingFlags.Instance | BindingFlags.NonPublic);
    private static FieldInfo f_ActiveShortAudioEmitters = typeof(AudioEventManager).GetField("ActiveShortAudioEmitters", BindingFlags.Static | BindingFlags.NonPublic);
    private static FieldInfo f_ActiveLongAudioEmitters = typeof(AudioEventManager).GetField("ActiveLongAudioEmitters", BindingFlags.Static | BindingFlags.NonPublic);
    private static FieldInfo f_guidIdMap = typeof(WwiseManager).GetField("guidIdMap", BindingFlags.Instance | BindingFlags.NonPublic);
    public static Dictionary<string, CustomSoundBankDef> soundBanksDefs = new Dictionary<string, CustomSoundBankDef>();
    private static Dictionary<string, uint> customSoundEvents = new Dictionary<string, uint>();
    public static Dictionary<string, uint> guidIdMap(this WwiseManager manager) {
      return (Dictionary<string, uint>)f_guidIdMap.GetValue(manager);
    }
    public static uint eventNameToId(string name) {
      if (customSoundEvents.ContainsKey(name)) { return customSoundEvents[name]; }
      return 0;
    }
    public static void registerEvents(this CustomSoundBankDef bank) {
      foreach(var ev in bank.events) {
        if (customSoundEvents.ContainsKey(ev.Key) == false) {
          Log.S.WL(2, "sound event:" + ev.Key + ":" + ev.Value);
          customSoundEvents.Add(ev.Key,ev.Value);
          if (SceneSingletonBehavior<WwiseManager>.Instance.guidIdMap().ContainsKey(ev.Key) == false) {
            SceneSingletonBehavior<WwiseManager>.Instance.guidIdMap().Add(ev.Key, ev.Value);
          } else {
            SceneSingletonBehavior<WwiseManager>.Instance.guidIdMap()[ev.Key] = ev.Value;
          }
        };
      }
    }
    public static void InitCustomSoundBanks(string directory) {
      string dir = Path.Combine(directory, CustomAmmoCategories.Settings.customSoundBanks);
      if (Directory.Exists(dir)) {
        string[] fileEntries = Directory.GetFiles(dir, "*.json");        
        foreach (string fileName in fileEntries) {
          try{
            CustomSoundBankDef def = JsonConvert.DeserializeObject<CustomSoundBankDef>(File.ReadAllText(fileName));
            soundBanksDefs.Add(def.name, def);
            def.filename = Path.Combine(dir, def.filename);
          } catch(Exception e) {
            Log.M.TWL(0,fileName+"\n"+e.ToString(),true);
          }
        }
      }
      var temp = AkSoundEngine.GetMajorMinorVersion();
      var temp2 = AkSoundEngine.GetSubminorBuildVersion();
      string m_WwiseVersionString = "Wwise v" + (temp >> 16) + "." + (temp & 0xFFFF);
      if (temp2 >> 16 != 0)
        m_WwiseVersionString += "." + (temp2 >> 16);

      m_WwiseVersionString += " Build " + (temp2 & 0xFFFF);
      Log.S.TWL(0, "WwiseManager.LoadCombatBanks AK_SOUNDBANK_VERSION:" + AkSoundEngine.AK_SOUNDBANK_VERSION + " " + m_WwiseVersionString, true);
      if (SceneSingletonBehavior<WwiseManager>.HasInstance) {
        List<LoadedAudioBank> loadedBanks = (List<LoadedAudioBank>)typeof(WwiseManager).GetField("loadedBanks", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(SceneSingletonBehavior<WwiseManager>.Instance);
        foreach (var bank in CustomSoundHelper.soundBanksDefs) {
          loadedBanks.Add(new LoadedAudioBank(bank.Key, true, false));
        }
      } else {
        Log.S.TWL(0, "WwiseManager not inited");
      }
    }
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


namespace CustAmmoCategoriesPatches {
  [HarmonyPatch(typeof(WwiseManager))]
  [HarmonyPatch("LoadDefaultBanks")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class WwiseManager_LoadCombatBanks {
    public static void Postfix(WwiseManager __instance, ref List<LoadedAudioBank> ___loadedBanks) {
    }
  }
  [HarmonyPatch(typeof(LoadedAudioBank))]
  [HarmonyPatch("LoadBankExternal")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class LoadedAudioBank_LoadBankExternal {
    public static bool Prefix(LoadedAudioBank __instance, ref AKRESULT __result, ref uint ___id) {
      Log.S.TWL(0, "LoadedAudioBank.LoadBankExternal "+__instance.name);
      if (CustomSoundHelper.soundBanksDefs.ContainsKey(__instance.name) == false) { return false; }
      var uri = new System.Uri(CustomSoundHelper.soundBanksDefs[__instance.name].filename).AbsoluteUri;
      Log.S.WL(1, uri);
      WWW www = new WWW(uri);
      while (!www.isDone) { Thread.Sleep(25); }
      Log.S.WL(1, "loaded");
      try {
        uint id = uint.MaxValue;
        __result = AkSoundEngine.LoadBank(GCHandle.Alloc((object)www.bytes, GCHandleType.Pinned).AddrOfPinnedObject(), (uint)www.bytes.Length, out id);
        ___id = id;
        if (__result == AKRESULT.AK_Success) { CustomSoundHelper.soundBanksDefs[__instance.name].registerEvents(); };
      } catch {
        __result = AKRESULT.AK_Fail;
      }
      Log.S.WL(1, "Result:" + __result + " id:" + ___id + " length:" + www.bytes.Length);
      return false;
    }
  }
}