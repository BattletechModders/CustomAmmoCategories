using BattleTech;
using BattleTech.Data;
using BattleTech.UI;
using Harmony;
using HBS;
using HBS.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CustomVoices {
  [HarmonyPatch(typeof(CombatHUDButtonBase))]
  [HarmonyPatch("OnClick")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatHUDActionButton_ExecuteClick {
    private static GenericPopup popup = null;
    public static bool Prefix(CombatHUDActionButton __instance) {
      Log.M.TWL(0,"CombatHUDActionButton.ExecuteClick '" + __instance.GUID + "'/'" + CombatHUD.ButtonID_Sprint + "' " + (__instance.GUID == CombatHUD.ButtonID_Sprint) + "\n");
      CombatHUD HUD = (CombatHUD)typeof(CombatHUDActionButton).GetProperty("HUD", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance, null);
      if (__instance.GUID == CombatHUD.ButtonID_Sprint) {
        Log.M.WL(1,"button is sprint");
        bool modifyers = (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
        if (modifyers) {
          Log.M.WL(1,"ctrl is pressed");
          if (HUD.SelectedActor != null) {
            Log.M.WL(1, "actor is selected");
            if (HUD.SelectedActor is Mech) {
              Log.M.WL(1, "mech is selected");
              Dictionary<AudioSwitch_dialog_lines_pilots, VOEvents> voEvents = new Dictionary<AudioSwitch_dialog_lines_pilots, VOEvents>();
              foreach (VOEvents voEvent in Enum.GetValues(typeof(VOEvents))) {
                VOEntryData voEntryData = AudioEventManager.AudioConstants.GetVOEntryData(voEvent);
                if (voEvents.ContainsKey(voEntryData.switchValue) == false) { voEvents.Add(voEntryData.switchValue, voEvent); };
              };
              List<AudioSwitch_dialog_lines_pilots> dlgs = new List<AudioSwitch_dialog_lines_pilots>();
              foreach (AudioSwitch_dialog_lines_pilots dlgEvent in Enum.GetValues(typeof(AudioSwitch_dialog_lines_pilots))) {
                dlgs.Add(dlgEvent);
              };
              StringBuilder text = new StringBuilder();
              int curIndex = 0;
              int pageSize = 10;
              for(int index = curIndex - curIndex % pageSize ; index < curIndex - curIndex % pageSize + pageSize; ++index) {
                if (index >= dlgs.Count) { break; }
                if (index == curIndex) { text.Append("->"); };
                text.Append(dlgs[index].ToString());
                if (voEvents.ContainsKey(dlgs[index])) { text.Append(" - "+ voEvents[dlgs[index]].ToString()); };
                text.AppendLine();
              }
              
              popup = GenericPopupBuilder.Create("audio pack", text.ToString())
                .AddButton("X", (Action)(() => {  }), true)
                .AddButton("->", (Action)(() => {
                  if (curIndex < (dlgs.Count - 1)) {
                    ++curIndex;
                    text.Clear();
                    for (int index = curIndex - curIndex % pageSize; index < curIndex - curIndex % pageSize + pageSize; ++index) {
                      if (index >= dlgs.Count) { break; }
                      if (index == curIndex) { text.Append("->"); };
                      text.Append(dlgs[index].ToString());
                      if (voEvents.ContainsKey(dlgs[index])) { text.Append(" - " + voEvents[dlgs[index]].ToString()); };
                      text.AppendLine();
                    }
                    if (popup != null) popup.TextContent = text.ToString();
                  }
                }),false)
                .AddButton("<-", (Action)(() => {
                  if (curIndex > 0) {
                    --curIndex;
                    text.Clear();
                    for (int index = curIndex - curIndex % pageSize; index < curIndex - curIndex % pageSize + pageSize; ++index) {
                      if (index >= dlgs.Count) { break; }
                      if (index == curIndex) { text.Append("->"); };
                      text.Append(dlgs[index].ToString());
                      if (voEvents.ContainsKey(dlgs[index])) { text.Append(" - " + voEvents[dlgs[index]].ToString()); };
                      text.AppendLine();
                    }
                    if (popup != null) popup.TextContent = text.ToString();
                  }
                }), false).AddButton("P",(Action)(()=> {
                  if (voEvents.ContainsKey(dlgs[curIndex])) {
                    AudioEventManager.PlayPilotVO(voEvents[dlgs[curIndex]], HUD.SelectedActor, (AkCallbackManager.EventCallback)null, (object)null, true);
                  }
                } ),false).IsNestedPopupWithBuiltInFader().SetAlwaysOnTop().Render();
            }
          }
          return false;
        }
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(DataManager))]
  [HarmonyPatch("GenderedOptionsListDefs")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class DataManager_GenderedOptionsListDefs {
    private static bool voicesAdded = false;
    public static bool Prefix(DataManager __instance, ref DictionaryStore<GenderedOptionsListDef> ___genderedOptionsListDefs) {
      if (voicesAdded == false) {
        Log.M.TWL(0, "DataManager.GenderedOptionsListDefs");
        voicesAdded = true;
        SimGameState simulation = LazySingletonBehavior<UnityGameInstance>.Instance.Game.Simulation;
        GenderedOptionsListDef genderedOptionsListDef = ___genderedOptionsListDefs.Get(simulation.Constants.Pilot.PilotVoices);
        foreach (var def in Core.extVoicePacks) {
          genderedOptionsListDef.optionsList[def.Value.gender].Add(def.Value.name);
        }
        Log.M.WL(1, "gender voices:");
        foreach (var glist in genderedOptionsListDef.optionsList) {
          Log.M.WL(2, glist.Key + ":");
          foreach (var gitm in glist.Value) {
            Log.M.WL(3, gitm);
          }
        }
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(WwiseManager))]
  [HarmonyPatch("SetSwitchById")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(uint), typeof(uint), typeof(AkGameObj) })]
  public static class WwiseManager_SetSwitchById {
    private static HashSet<AkGameObj> darklightSwitches = new HashSet<AkGameObj>();
    private static uint darkLightGroup = uint.MaxValue;
    private static uint darkValue = uint.MaxValue;
    private static uint lightValue = uint.MaxValue;
    public static bool isDarkTheme(this AkGameObj obj) { return darklightSwitches.Contains(obj); }
    public static void Postfix(WwiseManager __instance, uint switchGroup, uint switchId, AkGameObj sourceObject) {
      //Log.M.TWL(0, "WwiseManager.SetSwitchById grp:"+ switchGroup+" id:"+switchId);
      if (darkLightGroup == uint.MaxValue) { darkLightGroup = __instance.EnumTypeToSwitchGroup<AudioSwitch_dialog_dark_light>(AudioSwitch_dialog_dark_light.dark); }
      if (darkValue == uint.MaxValue) { darkValue = __instance.EnumValueToSwitchId<AudioSwitch_dialog_dark_light>(AudioSwitch_dialog_dark_light.dark); }
      if (lightValue == uint.MaxValue) { lightValue = __instance.EnumValueToSwitchId<AudioSwitch_dialog_dark_light>(AudioSwitch_dialog_dark_light.light); }
      //Log.M.WL(1, "darklight group:"+ darkLightGroup+" darkId:"+darkValue+" lightId:"+lightValue);
      if (switchGroup == darkLightGroup) {
        if (switchId == darkValue) {
          Log.M.TWL(0, sourceObject.name + ":darkTheme");
          darklightSwitches.Add(sourceObject);
        } else {
          Log.M.TWL(0, sourceObject.name + ":lightTheme");
          darklightSwitches.Remove(sourceObject);
        }
      }
    }
  }
  [HarmonyPatch(typeof(Pilot))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(CombatGameState), typeof(AbstractActor) })]
  public static class Pilot_Init {
    private static PropertyInfo p_Combat = typeof(Pilot).GetProperty("Combat", BindingFlags.Instance | BindingFlags.NonPublic);
    private static FieldInfo f__parentActor = typeof(Pilot).GetField("_parentActor", BindingFlags.Instance | BindingFlags.NonPublic);
    private static MethodInfo m_InitStats = typeof(Pilot).GetMethod("InitStats", BindingFlags.Instance | BindingFlags.NonPublic);
    public static void Combat(this Pilot pilot, CombatGameState Combat) { p_Combat.SetValue(pilot, Combat); }
    public static void _parentActor(this Pilot pilot, AbstractActor _parentActor) { f__parentActor.SetValue(pilot, _parentActor); }
    public static void InitStats(this Pilot pilot) { m_InitStats.Invoke(pilot, new object[] { }); }
    public static bool Prefix(Pilot __instance, CombatGameState Combat, AbstractActor pilotedActor) {
      //float scale = CustomAmmoCategories.Settings.bloodSettings.DecalScale[];
      Log.M.TWL(0, "Pilot.Init " + __instance.pilotDef.Description.Id);
      try {
        __instance.Combat(Combat);
        __instance._parentActor(pilotedActor);
        __instance.InitStats();
        __instance.pilotVoice = AudioSwitch_dialog_character_type_pilots.f_pro01;
        if (string.IsNullOrEmpty(__instance.pilotDef.Voice) || Utilities.EnumTryParse<AudioSwitch_dialog_character_type_pilots>(__instance.pilotDef.Voice, out __instance.pilotVoice)) {
        } else {
          if (Core.TryGetExtVoice(__instance.pilotDef.Voice, out VoicePackDef extVoice)) {
            __instance.pilotVoice = extVoice.baseVoice;
          } else {
            Log.M.TWL(0, "Can't find external voice " + __instance.pilotDef.Voice, true);
          }
        }
        return false;
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  public static class VOSoundBankLoadHelper {
    private static FieldInfo f_loadedBanks = typeof(WwiseManager).GetField("loadedBanks", BindingFlags.Instance | BindingFlags.NonPublic);
    public static List<LoadedAudioBank> loadedBanks(this WwiseManager manager) {
      return (List<LoadedAudioBank>)f_loadedBanks.GetValue(manager);
    }
    public static void LoadVOSoundBank(ModTek.SoundBankDef def) {
      if (!SceneSingletonBehavior<WwiseManager>.HasInstance) { return; }
      SceneSingletonBehavior<WwiseManager>.Instance.loadedBanks().Add(new LoadedAudioBank(def.name, true, false));
      SceneSingletonBehavior<WwiseManager>.Instance.voBanks.Add(def.name);
    }
  }
  [HarmonyPatch(typeof(SGBarracksDossierPanel))]
  [HarmonyPatch("PlayVO")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class SGBarracksDossierPanel_PlayVO {
    private static FieldInfo f_lastVOWasLight = typeof(SGBarracksDossierPanel).GetField("lastVOWasLight", BindingFlags.Static | BindingFlags.NonPublic);
    public static bool lastVOWasLight() { return (bool)f_lastVOWasLight.GetValue(null); }
    public static void lastVOWasLight(bool value) { f_lastVOWasLight.SetValue(null, value); }
    private static string prevStopEvent = string.Empty;
    public static bool Prefix(string voice, ref bool __result) {
      //float scale = CustomAmmoCategories.Settings.bloodSettings.DecalScale[];
      Log.M.TWL(0, "SGBarracksDossierPanel.PlayVO " + voice);
      uint ret = uint.MaxValue;
      if (string.IsNullOrEmpty(prevStopEvent) == false) {
        ret = WwiseManager.PostEvent(prevStopEvent, WwiseManager.GlobalAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
        Log.M.WL(1, "playing prev stop event:" + prevStopEvent + ":" + ret);
        prevStopEvent = string.Empty;
      }
      if (string.IsNullOrEmpty(voice)) { Log.M.WL(1, "empty voice"); return true; }
      if (Utilities.EnumTryParse<AudioSwitch_dialog_character_type_pilots>(voice, out AudioSwitch_dialog_character_type_pilots enumValue)) { Log.M.WL(1, "voice is default"); return true; }
      if (Core.TryGetExtVoice(voice, out VoicePackDef voicePack)) {
        if (string.IsNullOrEmpty(voicePack.vobank)) { Log.M.WL(1, "empty vobank"); return true; };
        if (ModTek.ModTek.soundBanks.ContainsKey(voicePack.vobank) == false) { Log.M.WL(1, "unknown sound bank"); return true; }
        if (ModTek.ModTek.soundBanks[voicePack.vobank].loaded == false) {
          Log.M.WL(1, "bank is not loaded");
          VOSoundBankLoadHelper.LoadVOSoundBank(ModTek.ModTek.soundBanks[voicePack.vobank]);
          if (ModTek.ModTek.soundBanks[voicePack.vobank].loaded == false) {
            Log.M.WL(1, "still bank is not loaded");
            return true;
          } else {
            Log.M.WL(1, "bank is loaded");
          }
        }
        if (string.IsNullOrEmpty(voicePack.stop_event) == false) {
          ret = WwiseManager.PostEvent(voicePack.stop_event, WwiseManager.GlobalAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
          Log.M.WL(1, "playing stop event:" + voicePack.stop_event + ":" + ret);
          prevStopEvent = voicePack.stop_event;
        }
        WwiseManager.PostEvent<AudioEventList_vo>(AudioEventList_vo.vo_stop_pilots, WwiseManager.GlobalAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
        WwiseManager.SetSwitch<AudioSwitch_dialog_character_type_pilots>(voicePack.baseVoice, WwiseManager.GlobalAudioObject);
        //WwiseManager.SetSwitch<AudioSwitch_dialog_lines_pilots>(AudioSwitch_dialog_lines_pilots.chosen, WwiseManager.GlobalAudioObject);

        if (SGBarracksDossierPanel_PlayVO.lastVOWasLight()) {
          WwiseManager.SetSwitch<AudioSwitch_dialog_dark_light>(AudioSwitch_dialog_dark_light.dark, WwiseManager.GlobalAudioObject);
        } else {
          WwiseManager.SetSwitch<AudioSwitch_dialog_dark_light>(AudioSwitch_dialog_dark_light.light, WwiseManager.GlobalAudioObject);
        }
        SGBarracksDossierPanel_PlayVO.lastVOWasLight(!SGBarracksDossierPanel_PlayVO.lastVOWasLight());
        string eventId = voicePack.getPhrase(WwiseManager.GlobalAudioObject.isDarkTheme(), AudioSwitch_dialog_lines_pilots.chosen);
        if(string.IsNullOrEmpty(eventId) == false) { 
          ret = WwiseManager.PostEvent(eventId, WwiseManager.GlobalAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
          Log.M.WL(1, "playing event:" + eventId + ":" + ret);
        } else {
          WwiseManager.SetSwitch<AudioSwitch_dialog_lines_pilots>(AudioSwitch_dialog_lines_pilots.chosen, WwiseManager.GlobalAudioObject);
          int num2 = (int)WwiseManager.PostEvent<AudioEventList_vo>(AudioEventList_vo.vo_play_pilots, WwiseManager.GlobalAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
        }
      } else {
        return true;
      }
      __result = true;
      return false;
    }
  }
  [HarmonyPatch(typeof(SG_HiringHall_DetailPanel))]
  [HarmonyPatch("PlayPilotSelectionVO")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Pilot) })]
  public static class SG_HiringHall_DetailPanel_PlayPilotSelectionVO {
    private static FieldInfo f_lastVOWasLight = typeof(SG_HiringHall_DetailPanel).GetField("lastVOWasLight", BindingFlags.Instance | BindingFlags.NonPublic);
    public static bool lastVOWasLight(this SG_HiringHall_DetailPanel panel) { return (bool)f_lastVOWasLight.GetValue(panel); }
    public static void lastVOWasLight(this SG_HiringHall_DetailPanel panel, bool value) { f_lastVOWasLight.SetValue(panel, value); }
    private static string prevStopEvent = string.Empty;
    public static bool Prefix(SG_HiringHall_DetailPanel __instance, Pilot p) {
      //float scale = CustomAmmoCategories.Settings.bloodSettings.DecalScale[];
      Log.M.TWL(0, "SG_HiringHall_DetailPanel.PlayPilotSelectionVO " + p.Description.Id + ":" + p.pilotDef.Voice);
      uint ret = uint.MaxValue;
      if (string.IsNullOrEmpty(prevStopEvent) == false) {
        ret = WwiseManager.PostEvent(prevStopEvent, WwiseManager.GlobalAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
        Log.M.WL(1, "playing prev stop event:" + prevStopEvent + ":" + ret);
        prevStopEvent = string.Empty;
      }
      //if (p.pilotDef.Description.Id == "pilot_backer_SwansonA") { p.pilotDef.SetVoice("tex_voice"); };
      string voice = p.pilotDef.Voice;
      if (string.IsNullOrEmpty(voice)) { Log.M.WL(1, "empty voice"); return true; }
      if (Utilities.EnumTryParse<AudioSwitch_dialog_character_type_pilots>(voice, out AudioSwitch_dialog_character_type_pilots enumValue)) { Log.M.WL(1, "voice is default"); return true; }
      if (Core.TryGetExtVoice(voice, out VoicePackDef voicePack)) {
        if (string.IsNullOrEmpty(voicePack.vobank)) { Log.M.WL(1, "empty vobank"); return true; };
        if (ModTek.ModTek.soundBanks.ContainsKey(voicePack.vobank) == false) { Log.M.WL(1, "unknown sound bank"); return true; }
        if (ModTek.ModTek.soundBanks[voicePack.vobank].loaded == false) {
          Log.M.WL(1, "bank is not loaded");
          VOSoundBankLoadHelper.LoadVOSoundBank(ModTek.ModTek.soundBanks[voicePack.vobank]);
          if (ModTek.ModTek.soundBanks[voicePack.vobank].loaded == false) {
            Log.M.WL(1, "still bank is not loaded");
            return true;
          } else {
            Log.M.WL(1, "bank is loaded");
          }
        }
        if (string.IsNullOrEmpty(voicePack.stop_event) == false) {
          ret = WwiseManager.PostEvent(voicePack.stop_event, WwiseManager.GlobalAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
          Log.M.WL(1, "playing stop event:" + voicePack.stop_event + ":" + ret);
          prevStopEvent = voicePack.stop_event;
        }
        WwiseManager.PostEvent<AudioEventList_vo>(AudioEventList_vo.vo_stop_pilots, WwiseManager.GlobalAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
        WwiseManager.SetSwitch<AudioSwitch_dialog_character_type_pilots>(voicePack.baseVoice, WwiseManager.GlobalAudioObject);
        //WwiseManager.SetSwitch<AudioSwitch_dialog_lines_pilots>(AudioSwitch_dialog_lines_pilots.chosen, WwiseManager.GlobalAudioObject);

        if (__instance.lastVOWasLight()) {
          Log.M.WL(1, "switch dark");
          WwiseManager.SetSwitch<AudioSwitch_dialog_dark_light>(AudioSwitch_dialog_dark_light.dark, WwiseManager.GlobalAudioObject);
        } else {
          Log.M.WL(1, "switch light");
          WwiseManager.SetSwitch<AudioSwitch_dialog_dark_light>(AudioSwitch_dialog_dark_light.light, WwiseManager.GlobalAudioObject);
        }
        __instance.lastVOWasLight(!__instance.lastVOWasLight());
        Log.M.WL(1, "lastVOWasLight:"+ __instance.lastVOWasLight());
        string eventId = voicePack.getPhrase(WwiseManager.GlobalAudioObject.isDarkTheme(), AudioSwitch_dialog_lines_pilots.chosen);
        if (string.IsNullOrEmpty(eventId) == false) {
          ret = WwiseManager.PostEvent(eventId, WwiseManager.GlobalAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
          Log.M.WL(1, "playing event:" + eventId + ":" + ret);
        } else {
          WwiseManager.SetSwitch<AudioSwitch_dialog_lines_pilots>(AudioSwitch_dialog_lines_pilots.chosen, WwiseManager.GlobalAudioObject);
          int num2 = (int)WwiseManager.PostEvent<AudioEventList_vo>(AudioEventList_vo.vo_play_pilots, WwiseManager.GlobalAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
        }
      } else {
        return true;
      }
      return false;
    }
  }
  [HarmonyPatch(typeof(AudioEventManager))]
  [HarmonyPatch("InterruptPilotVOForTeam")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Team), typeof(AbstractActor) })]
  public static class AudioEventManager_InterruptPilotVOForTeam {
    public static void InterruptPilotVO(string voice, AkGameObj audioObject) {
      Log.M.TWL(0, "AudioEventManager.InterruptPilotVO " + voice);
      if (string.IsNullOrEmpty(voice)) { Log.M.WL(1, "empty voice"); return; }
      if (Utilities.EnumTryParse<AudioSwitch_dialog_character_type_pilots>(voice, out AudioSwitch_dialog_character_type_pilots enumValue)) { Log.M.WL(1, "voice is default"); return; }
      if (Core.TryGetExtVoice(voice, out VoicePackDef voicePack)) {
        if (string.IsNullOrEmpty(voicePack.vobank)) { Log.M.WL(1, "empty vobank"); return; };
        if (ModTek.ModTek.soundBanks.ContainsKey(voicePack.vobank) == false) { Log.M.WL(1, "unknown sound bank"); return; }
        if (ModTek.ModTek.soundBanks[voicePack.vobank].loaded == false) {
          Log.M.WL(1, "bank is not loaded");
          VOSoundBankLoadHelper.LoadVOSoundBank(ModTek.ModTek.soundBanks[voicePack.vobank]);
          if (ModTek.ModTek.soundBanks[voicePack.vobank].loaded == false) {
            Log.M.WL(1, "still bank is not loaded");
            return;
          } else {
            Log.M.WL(1, "bank is loaded");
          }
        }
        if(string.IsNullOrEmpty(voicePack.stop_event) == false) {
          uint ret = WwiseManager.PostEvent(voicePack.stop_event, audioObject, (AkCallbackManager.EventCallback)null, (object)null);
          Log.M.WL(1, "playing stop event:" + voicePack.stop_event + ":" + ret);
        }
      }
    }
    public static void Postfix(Team team, AbstractActor actorToIgnore) {
      //float scale = CustomAmmoCategories.Settings.bloodSettings.DecalScale[];
      Log.M.TWL(0, "AudioEventManager.InterruptPilotVOForTeam " + team.DisplayName);
      if (!team.LocalPlayerControlsTeam) { return; };
      for (int index = 0; index < team.unitCount; ++index) {
        AbstractActor unit = team.units[index];
        if (unit != actorToIgnore && (UnityEngine.Object)unit.GameRep != (UnityEngine.Object)null && (UnityEngine.Object)unit.GameRep.audioObject != (UnityEngine.Object)null) {
          AudioEventManager_InterruptPilotVOForTeam.InterruptPilotVO(unit.GetPilot().pilotDef.Voice,unit.GameRep.audioObject);
        }
      }
    }
  }
  public static class PlayPilotVOPatch {
    private static MethodInfo m_IsPlayingVO = typeof(PilotRepresentation).GetProperty("IsPlayingVO", BindingFlags.Instance | BindingFlags.Public).GetSetMethod(true);
    private static MethodInfo m_CurrentVOPriority = typeof(PilotRepresentation).GetProperty("CurrentVOPriority", BindingFlags.Instance | BindingFlags.Public).GetSetMethod(true);
    public static void IsPlayingVO(this PilotRepresentation pr, bool value) {
      m_IsPlayingVO.Invoke(pr, new object[] { value });
    }
    public static void CurrentVOPriority(this PilotRepresentation pr, int value) {
      m_CurrentVOPriority.Invoke(pr, new object[] { value });
    }
    public static bool Prefix(PilotRepresentation __instance, ref bool ___startedVOStatic, AudioSwitch_dialog_lines_pilots VOEnumValue, int priority, AkCallbackManager.EventCallback callback, object in_cookie) {
      try {
        Log.M.TWL(0, "PilotRepresentation.PlayPilotVO " + __instance.pilot.pilotDef.Description.Id + " voice:" + __instance.pilot.pilotDef.Voice + " phrase:" + VOEnumValue);
        string voice = __instance.pilot.pilotDef.Voice;
        if (string.IsNullOrEmpty(voice)) { Log.M.WL(1, "empty voice"); return true; }
        if (Utilities.EnumTryParse<AudioSwitch_dialog_character_type_pilots>(voice, out AudioSwitch_dialog_character_type_pilots enumValue)) { Log.M.WL(1, "voice is default"); return true; }
        if (Core.TryGetExtVoice(voice, out VoicePackDef voicePack)) {
          if (string.IsNullOrEmpty(voicePack.vobank)) { Log.M.WL(1, "empty vobank"); return true; };
          if (ModTek.ModTek.soundBanks.ContainsKey(voicePack.vobank) == false) { Log.M.WL(1, "unknown sound bank"); return true; }
          if (ModTek.ModTek.soundBanks[voicePack.vobank].loaded == false) {
            Log.M.WL(1, "bank is not loaded");
            VOSoundBankLoadHelper.LoadVOSoundBank(ModTek.ModTek.soundBanks[voicePack.vobank]);
            if (ModTek.ModTek.soundBanks[voicePack.vobank].loaded == false) {
              Log.M.WL(1, "still bank is not loaded");
              return true;
            } else {
              Log.M.WL(1, "bank is loaded");
            }
          }
          uint ret = uint.MaxValue;
          string eventId = voicePack.getPhrase(__instance.audioObject.isDarkTheme(), VOEnumValue);
          if (string.IsNullOrEmpty(eventId) == false) {
            //ret = WwiseManager.PostEvent(voicePack.dark_phrases[AudioSwitch_dialog_lines_pilots.chosen], WwiseManager.GlobalAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
            if (!___startedVOStatic) {
              ___startedVOStatic = true;
              int num = (int)WwiseManager.PostEvent<AudioEventList_vo>(AudioEventList_vo.vo_static_start_pilot, __instance.audioObject, (AkCallbackManager.EventCallback)null, (object)null);
            }
            AudioEventManager.InterruptPilotVOForTeam(__instance.pilot.ParentActor.team, (AbstractActor)null);
            ret = WwiseManager.PostEvent(eventId, __instance.audioObject, callback != null ? callback : new AkCallbackManager.EventCallback(__instance.AudioCallback), in_cookie);
            Log.M.WL(1, "playing event:" + eventId + ":" + ret);
          } else {
            return true;
          }
          if (ret != 0) {
            __instance.IsPlayingVO(true);
            __instance.CurrentVOPriority(priority);
            Log.M.WL(1, "event has been played");
          } else {
            __instance.IsPlayingVO(true);
            __instance.CurrentVOPriority(priority);
            __instance.AudioCallback(null, AkCallbackType.AK_EndOfEvent, null);
            Log.M.WL(1, "event has not been played");
          }
        } else {
          Log.M.WL(1, "can't found voice pack");
          return true;
        }
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
        return true;
      }
      return false;
    }
  }
  public class VoicePackDef {
    [JsonIgnore]
    private string fuiname;
    public string name { get; set; }
    public string uiname { get { if (string.IsNullOrEmpty(fuiname)) { return name; } else { return fuiname; } } set { fuiname = value; } }
    public string vobank { get; set; }
    public AudioSwitch_dialog_character_type_pilots baseVoice { get; set; }
    public string stop_event { get; set; }
    public Gender gender { get; set; }
    public Dictionary<AudioSwitch_dialog_lines_pilots, List<string>> dark_phrases { get; set; }
    public Dictionary<AudioSwitch_dialog_lines_pilots, List<string>> light_phrases { get; set; }
    public override string ToString() { return uiname; }
    public string getPhrase(bool darkMood, AudioSwitch_dialog_lines_pilots val) {
      Log.M.TWL(0, "VoicePackDef.getPhrase mood:"+darkMood+" phrase:"+val);
      Dictionary<AudioSwitch_dialog_lines_pilots, List<string>> phrases = darkMood ? dark_phrases : light_phrases;
      bool swithcMood = true;
      if (phrases.ContainsKey(val)) {
        if (phrases[val].Count > 0) {
          foreach (string res in phrases[val]) { if (string.IsNullOrEmpty(res) == false) { swithcMood = false; break; } }
        }
      }
      if (swithcMood) { darkMood = !darkMood; phrases = darkMood ? dark_phrases : light_phrases; };
      Log.M.WL(1, "resulting mood:" + darkMood);
      int watchdog = 0;
      if (phrases.ContainsKey(val) == false) { Log.M.WL(1,"can't find"); return string.Empty; }
      do {
        int roll = Random.Range(0,phrases[val].Count);
        Log.M.WL(1, "roll:" + roll+"/"+ phrases[val].Count);
        if (roll < phrases[val].Count) {
          if (string.IsNullOrEmpty(phrases[val][roll]) == false) { return phrases[val][roll]; };
        }
        ++watchdog;
      } while (watchdog < 10);
      return string.Empty;
    }
    public VoicePackDef() {
      dark_phrases = new Dictionary<AudioSwitch_dialog_lines_pilots, List<string>>();
      light_phrases = new Dictionary<AudioSwitch_dialog_lines_pilots, List<string>>();
      vobank = string.Empty;
      gender = Gender.Male;
      baseVoice = AudioSwitch_dialog_character_type_pilots.m_bear01;
    }
  }
  public class Settings {
    public bool debugLog { get; set; }
    public Settings() { debugLog = false; }
  }
  public static class Core {
    public static Dictionary<string, VoicePackDef> extVoicePacks = new Dictionary<string, VoicePackDef>();
    public static bool TryGetExtVoice(string name, out VoicePackDef voicePack) {
      return extVoicePacks.TryGetValue(name, out voicePack);
    }
    public static Settings settings { get; set; }
    public static void FinishedLoading(List<string> loadOrder, Dictionary<string, Dictionary<string, VersionManifestEntry>> customResources) {
      Log.M.TWL(0, "FinishedLoading", true);
      try {
        foreach (var customResource in customResources) {
          Log.M.WL(1, "customResource:" + customResource.Key);
          if (customResource.Key != nameof(VoicePackDef)) { continue; }
          foreach (var resource in customResource.Value) {
            Log.M.WL(2, "resource:" + resource.Key + "=" + resource.Value.FilePath);
            try {
              VoicePackDef def = JsonConvert.DeserializeObject<VoicePackDef>(File.ReadAllText(resource.Value.FilePath));
              if (extVoicePacks.ContainsKey(def.name) == false) {
                extVoicePacks.Add(def.name, def);
              } else {
                extVoicePacks[def.name] = def;
              }
            } catch (Exception e) {
              Log.M.TWL(0, resource.Value.FilePath + "\n" + e.ToString(), true);
            }
          }
        }
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
    }
    public static void Init(string directory, string settingsJson) {
      Log.BaseDirectory = directory;
      Core.settings = JsonConvert.DeserializeObject<CustomVoices.Settings>(settingsJson);
      Log.InitLog();
      Log.M.TWL(0, "Initing... " + directory + " version: " + Assembly.GetExecutingAssembly().GetName().Version + "\n", true);
      try {
        var harmony = HarmonyInstance.Create("ru.mission.customvoices");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
        MethodInfo bPlayPilotVO = typeof(PilotRepresentation).GetMethod("PlayPilotVO");
        MethodInfo miPlayPilotVO = bPlayPilotVO.MakeGenericMethod(new Type[] { typeof(AudioSwitch_dialog_lines_pilots) });
        MethodInfo pPlayPilotVO = typeof(PlayPilotVOPatch).GetMethod("Prefix");
        harmony.Patch(miPlayPilotVO, new HarmonyMethod(pPlayPilotVO));
      } catch (Exception e) {
        Log.LogWrite(e.ToString() + "\n");
      }
    }
  }
}
