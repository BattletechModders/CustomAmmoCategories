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
using CustAmmoCategories;
using CustomAmmoCategoriesLog;
using HarmonyLib;
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
    public static void PostEventById(this AudioEmitterObject audioEmitter, uint eventId, Vector3 worldPos, object in_pCookie = null) {
      if (SceneSingletonBehavior<WwiseManager>.HasInstance) {
        audioEmitter.activeEventId = (SceneSingletonBehavior<WwiseManager>.Instance.PostEventById(eventId, audioEmitter.audioObject, (AkCallbackManager.EventCallback)null, (object)null));
        Log.S?.TWL(0, "Playing sound by id:" + audioEmitter.activeEventId);
      } else {
        Log.S?.TWL(0, "Can't play");
      }
      audioEmitter.isPlaying = (true);
    }
    public static void PostEventByName(this AudioEmitterObject audioEmitter, string eventName, Vector3 worldPos, object in_pCookie = null) {
      if (SceneSingletonBehavior<WwiseManager>.HasInstance) {
        audioEmitter.activeEventId = (SceneSingletonBehavior<WwiseManager>.Instance.PostEventByName(eventName, audioEmitter.audioObject, (AkCallbackManager.EventCallback)null, (object)null));
        Log.S?.TWL(0, "Playing sound by id:" + audioEmitter.activeEventId);
      } else {
        Log.S?.TWL(0, "Can't play");
      }
      audioEmitter.isPlaying = (true);
    }
    public static void SpawnAudioEmitter(string eventName, Vector3 worldPos, bool persistentSound) {
      CombatGameState combat = UnityGameInstance.BattleTechGame.Combat;
      Log.S?.TWL(0, "SpawnAudioEmitter by id:" + eventName);
      if (CustomVoices.AudioEngine.isInAudioManifest(eventName)) {
        Log.S?.WL(1, $"custom sound at {worldPos}");
        GameObject emmiter = new GameObject($"{eventName}_emitter");
        emmiter.transform.position = worldPos;
        CustomVoices.AudioObject audioEmiter = emmiter.AddComponent<CustomVoices.AudioObject>();
        if (persistentSound) {
          audioEmiter.Play(eventName, true);
        } else {
          audioEmiter.Play(eventName, false, new List<CustomVoices.AudioEngine.AudioChannelEvent>() { new CustomVoices.AudioEngine.AudioChannelEvent(0.0f, () => {
            Log.S?.TWL(0, $"destroy emitter {emmiter.name}");
            GameObject.Destroy(emmiter);
          }) });
        }
      } else {
        if (!persistentSound && AudioEventManager.ActiveShortAudioEmitters.Count >= AudioEventManager.AudioConstants.maxShortAudioEmitters)
          return;
        AudioEmitterObject audioEmitterObject;
        if (persistentSound && AudioEventManager.ActiveLongAudioEmitters.Count >= AudioEventManager.AudioConstants.maxLongAudioEmitters) {
          audioEmitterObject = AudioEventManager.ActiveLongAudioEmitters[0];
          AudioEventManager.ActiveLongAudioEmitters.RemoveAt(0);
          audioEmitterObject.gameObject.transform.position = worldPos;
        } else {
          GameObject gameObject = combat.DataManager.PooledInstantiate(AudioEventManager.AudioConstants.audioEmitterPrefab, BattleTechResourceType.Prefab, new Vector3?(worldPos), new Quaternion?(), (Transform)null);
          gameObject.transform.position = worldPos;
          audioEmitterObject = gameObject.GetComponent<AudioEmitterObject>();
        }
        if (audioEmitterObject == null)
          return;
        if (persistentSound)
          AudioEventManager.ActiveLongAudioEmitters.Add(audioEmitterObject);
        else
          AudioEventManager.ActiveShortAudioEmitters.Add(audioEmitterObject);
        audioEmitterObject.PostEventByName(eventName, worldPos, persistentSound);
      }
    }
  }
}
