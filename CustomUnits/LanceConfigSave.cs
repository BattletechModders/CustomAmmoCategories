/*  
 *  This file is part of CustomUnits.
 *  CustomUnits is free software: you can redistribute it and/or modify it under the terms of the 
 *  GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, 
 *  or (at your option) any later version. CustomAmmoCategories is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 *  See the GNU Lesser General Public License for more details.
 *  You should have received a copy of the GNU Lesser General Public License along with CustomAmmoCategories. 
 *  If not, see <https://www.gnu.org/licenses/>. 
*/
using BattleTech;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using BattleTech.UI.Tooltips;
using Harmony;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace CustomUnits {
  public static class SaveDropLayoutHelper {
    public static string BaseDirectory { get; set; }
    public static void Init(string dir) {
      BaseDirectory = Path.Combine(dir, "user_drop_layouts");
      if (Directory.Exists(BaseDirectory) == false) { Directory.CreateDirectory(BaseDirectory); }
    }
    public static void Save(SavedLanceLayout layout) {
      try {
        string path = Path.Combine(BaseDirectory, layout.name + ".json");
        File.WriteAllText(path, JsonConvert.SerializeObject(layout, Formatting.Indented));
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
    public static List<SavedLanceLayout> LoadAll() {
      List<SavedLanceLayout> result = new List<SavedLanceLayout>();
      try {
        string[] files = Directory.GetFiles(BaseDirectory, "*.json", SearchOption.TopDirectoryOnly);
        foreach (string f in files) {
          try {
            result.Add(JsonConvert.DeserializeObject<SavedLanceLayout>(File.ReadAllText(f)));
          } catch (Exception e) {
            Log.TWL(0, e.ToString(), true);
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
      return result;
    }
  }
  public class SavedLanceSlot {
    public string mech { get; set; } = string.Empty;
    public string pilot { get; set; } = string.Empty;
  }
  public class SavedLanceLayout {
    public string name { get; set; } = string.Empty;
    [JsonIgnore]
    public bool clear { get; set; } = false;
    public List<SavedLanceSlot> items { get; set; } = new List<SavedLanceSlot>();
  }
  public class LanceConfigSaver: MonoBehaviour {
    public LanceConfiguratorPanel parent { get; set; } = null;
    public HBSDOTweenButton button { get; set; } = null;
    public void Init(LanceConfiguratorPanel parent) {
      this.parent = parent;
      this.button = this.gameObject.GetComponent<HBSDOTweenButton>();
      this.button.OnClicked = new UnityEngine.Events.UnityEvent();
      this.button.OnClicked.AddListener(new UnityEngine.Events.UnityAction(this.OnClicked));
      List<GameObject> tweenObjects = Traverse.Create(this.button).Field<List<GameObject>>("tweenObjects").Value;
      tweenObjects[0] = this.gameObject.FindComponent<Transform>("bttn2_Fill").gameObject;
      tweenObjects[1] = this.gameObject.FindComponent<Transform>("bttn2_border").gameObject;
      tweenObjects[2] = this.gameObject.FindComponent<Transform>("bttn2_Text-optional").gameObject;
      LocalizableText title = this.gameObject.GetComponentInChildren<LocalizableText>();
      title.SetText("SAVE");
      HBSTooltip tooltip = this.gameObject.GetComponentInChildren<HBSTooltip>();
      tooltip.SetDefaultStateData(TooltipUtilities.GetStateDataFromObject((object)"Save current drop layout"));
    }
    public void SaveLoadoutName(string name) {
      if (string.IsNullOrEmpty(name)) { return; }
      name = name.Replace('\\', '_');
      name = name.Replace(':', '_');
      Log.TWL(0, "LanceConfigSaver.SaveLoadoutName " + name);
      LanceLoadoutSlot[] loadoutSlots = Traverse.Create(this.parent).Field<LanceLoadoutSlot[]>("loadoutSlots").Value;
      SavedLanceLayout savedLanceLayout = new SavedLanceLayout();
      savedLanceLayout.name = name;
      for (int t = 0; t < loadoutSlots.Length; ++t) {
        MechDef mechDef = null;
        Pilot pilot = null;
        if ((loadoutSlots[t].SelectedMech != null)&&(loadoutSlots[t].isMechLocked == false)&&(loadoutSlots[t].isFullLocked == false)) {
          if(loadoutSlots[t].SelectedMech.MechDef != null) {
            mechDef = loadoutSlots[t].SelectedMech.MechDef;
          }
        }
        if((loadoutSlots[t].SelectedPilot != null)&& (loadoutSlots[t].isPilotLocked == false) && (loadoutSlots[t].isFullLocked == false)) {
          if (loadoutSlots[t].SelectedPilot.Pilot != null) {
            pilot = loadoutSlots[t].SelectedPilot.Pilot;
          }
        }
        SavedLanceSlot slot = new SavedLanceSlot();
        slot.mech = mechDef == null ? string.Empty : mechDef.Description.Id;
        slot.pilot = pilot == null ? string.Empty : pilot.Description.Id;
        savedLanceLayout.items.Add(slot);
        Log.WL(1, "["+t+"] mech:"+(mechDef == null?"null":mechDef.Description.Id)+" pilot:"+(pilot==null?"null":pilot.Description.Id)+" isMechLocked:"+ loadoutSlots[t].isMechLocked+" isPilotLocked:"+loadoutSlots[t].isPilotLocked);
      }
      SaveDropLayoutHelper.Save(savedLanceLayout);
    }
    public void OnClicked() {
      GenericPopup popup = null;
      popup = GenericPopupBuilder.Create("SAVE CURRENT LAYOUT AS", "Input name for layout").AddInput("name", this.SaveLoadoutName).IsNestedPopupWithBuiltInFader().SetAlwaysOnTop().Render();
    }
  }
  public class LanceConfigLoader : MonoBehaviour {
    public LanceConfiguratorPanel parent { get; set; } = null;
    public HBSDOTweenButton button { get; set; } = null;
    public void Init(LanceConfiguratorPanel parent) {
      this.parent = parent;
      this.button = this.gameObject.GetComponent<HBSDOTweenButton>();
      this.button.OnClicked = new UnityEngine.Events.UnityEvent();
      this.button.OnClicked.AddListener(new UnityEngine.Events.UnityAction(this.OnClicked));
      List<GameObject> tweenObjects = Traverse.Create(this.button).Field<List<GameObject>>("tweenObjects").Value;
      tweenObjects[0] = this.gameObject.FindComponent<Transform>("bttn2_Fill").gameObject;
      tweenObjects[1] = this.gameObject.FindComponent<Transform>("bttn2_border").gameObject;
      tweenObjects[2] = this.gameObject.FindComponent<Transform>("bttn2_Text-optional").gameObject;
      LocalizableText title = this.gameObject.GetComponentInChildren<LocalizableText>();
      title.SetText("LOAD");
      HBSTooltip tooltip = this.gameObject.GetComponentInChildren<HBSTooltip>();
      tooltip.SetDefaultStateData(TooltipUtilities.GetStateDataFromObject((object)"Save current drop layout"));
    }
    public void RestoreAs(SavedLanceLayout layout) {
      LanceLoadoutSlot[] loadoutSlots = Traverse.Create(this.parent).Field<LanceLoadoutSlot[]>("loadoutSlots").Value;
      StringBuilder errors = new StringBuilder();
      if (layout.clear) {
        for (int t = 0; t < loadoutSlots.Length; ++t) {
          LanceLoadoutSlot slot = loadoutSlots[t];
          if (slot.isFullLocked) { continue; }
          slot.OnClearSlotsClicked();
        }
        return;
      }
      for (int t = 0; t < loadoutSlots.Length; ++t) {
        if (t >= layout.items.Count) { break; }
        SavedLanceSlot saved = layout.items[t];
        CustomLanceSlot customSlot = loadoutSlots[t].gameObject.GetComponent<CustomLanceSlot>();
        LanceLoadoutSlot slot = loadoutSlots[t];
        IMechLabDraggableItem forcedMech = null;
        SGBarracksRosterSlot forcedPilot = null;
        if (slot.isFullLocked) { continue; }
        if(slot.isMechLocked == false) {
          if(string.IsNullOrEmpty(saved.mech) == false) {
            forcedMech = this.parent.mechListWidget.GetInventoryItem(saved.mech);
            if(forcedMech != null) {
              if (MechValidationRules.ValidateMechCanBeFielded(this.parent.Sim, forcedMech.MechDef) == false) {
                errors.AppendLine(forcedMech.MechDef.Description.Name + " can't be fielded");
                forcedMech = null;
              } 
              if ((customSlot != null)&&(forcedMech != null)) {
                if (forcedMech.MechDef.Chassis.CanBeDropedInto(customSlot.slotDef, out string title, out string message) == false) {
                  errors.AppendLine(message);
                  forcedMech = null;
                }
              }
              float minTonnage = Traverse.Create(slot).Field<float>("minTonnage").Value;
              float maxTonnage = Traverse.Create(slot).Field<float>("maxTonnage").Value;
              if ((forcedMech != null)&&(minTonnage > 0f)) {
                if(forcedMech.MechDef.Chassis.Tonnage < minTonnage) {
                  errors.AppendLine(forcedMech.MechDef.Description.Name + " too low tonnage");
                  forcedMech = null;
                }
              }
              if ((forcedMech != null) && (maxTonnage > 0f)) {
                if (forcedMech.MechDef.Chassis.Tonnage > maxTonnage) {
                  errors.AppendLine(forcedMech.MechDef.Description.Name + " too big tonnage");
                  forcedMech = null;
                }
              }
            }
          }
        }
        if (slot.isPilotLocked == false) {
          if (string.IsNullOrEmpty(saved.pilot) == false) {
            forcedPilot = this.parent.pilotListWidget.GetPilot(saved.pilot);
            if (forcedPilot != null) {
              if (forcedPilot.Pilot.CanPilot == false) {
                errors.AppendLine(forcedPilot.Pilot.Callsign + " can't pilot");
                forcedPilot = null;
              }
            }
          }
        }
        if (slot.isPilotLocked) { forcedPilot = slot.SelectedPilot; }
        if (slot.isMechLocked) { forcedMech = slot.SelectedMech; }
        if ((slot.isPilotLocked) && (slot.isMechLocked == false) && (forcedMech != null) && (forcedPilot != null)) {
          if(forcedMech.MechDef.Chassis.CanBePilotedBy(forcedPilot.Pilot.pilotDef, out string title, out string message) == false) {
            errors.AppendLine(message);
            forcedMech = null;
          }
        }
        if ((slot.isPilotLocked == false) && (forcedMech != null) && (forcedPilot != null)) {
          if (forcedMech.MechDef.Chassis.CanBePilotedBy(forcedPilot.Pilot.pilotDef, out string title, out string message) == false) {
            errors.AppendLine(message);
            forcedPilot = null;
          }
        }
        if (slot.isPilotLocked) { forcedPilot = null; } else if (forcedPilot != null) {
          this.parent.pilotListWidget.RemovePilot(this.parent.availablePilots.Find((Predicate<Pilot>)(x => x.Description.Id == forcedPilot.Pilot.Description.Id)));
        }
        if (slot.isMechLocked) { forcedMech = null; } else if (forcedMech != null) {
          
        }
        slot.SetLockedData(forcedMech, forcedPilot, false);
      }
      if(errors.Length > 0) {
        GenericPopup popup = GenericPopupBuilder.Create("APPLY LAYOUT WARNINGS", errors.ToString()).IsNestedPopupWithBuiltInFader().SetAlwaysOnTop().Render();
      }
    }
    public void OnClicked() {
      GenericPopup popup = null;
      var invList = SaveDropLayoutHelper.LoadAll();
      invList.Sort((a, b) => { return a.name.CompareTo(b.name); });
      SavedLanceLayout clear = new SavedLanceLayout();
      clear.name = "CLEAR";
      clear.clear = true;
      invList.Insert(0, clear);
      if (invList.Count == 0) {
        popup = GenericPopupBuilder.Create("NO SAVED LAYOUTS", "No saved layouts").IsNestedPopupWithBuiltInFader().SetAlwaysOnTop().Render();
        return;
      }
      StringBuilder text = new StringBuilder();
      int curIndex = 0;
      int pageSize = 10;
      for (int index = curIndex - curIndex % pageSize; index < curIndex - curIndex % pageSize + pageSize; ++index) {
        if (index >= invList.Count) { break; }
        if (index == curIndex) { text.Append("<size=150%><color=orange>"); };
        text.Append(invList[index].name);
        if (index == curIndex) { text.Append("</size></color>"); };
        text.AppendLine();
      }
      popup = GenericPopupBuilder.Create("AVAILABLE LAYOUTS", text.ToString())
        .AddButton("X", (Action)(() => { }), true)
        .AddButton("->", (Action)(() => {
          try {
            if (curIndex < (invList.Count - 1)) {
              ++curIndex;
              text.Clear();
              for (int index = curIndex - curIndex % pageSize; index < curIndex - curIndex % pageSize + pageSize; ++index) {
                if (index >= invList.Count) { break; }
                if (index == curIndex) { text.Append("<size=150%><color=orange>"); };
                text.Append(invList[index].name);
                if (index == curIndex) { text.Append("</size></color>"); };
                text.AppendLine();
              }
              if (popup != null) popup.TextContent = text.ToString();
            }
          } catch (Exception e) {
            Log.TWL(0, e.ToString(), true);
          }
        }), false)
        .AddButton("<-", (Action)(() => {
          try {
            if (curIndex > 0) {
              --curIndex;
              text.Clear();
              for (int index = curIndex - curIndex % pageSize; index < curIndex - curIndex % pageSize + pageSize; ++index) {
                if (index >= invList.Count) { break; }
                if (index == curIndex) { text.Append("<size=150%><color=orange>"); };
                text.Append(invList[index].name);
                if (index == curIndex) { text.Append("</size></color>"); };
                text.AppendLine();
              }
              if (popup != null) popup.TextContent = text.ToString();
            }
          } catch (Exception e) {
            Log.TWL(0, e.ToString(), true);
          }
        }), false).AddButton("APPLY", (Action)(() => {
          this.RestoreAs(invList[curIndex]);
        }), true).IsNestedPopupWithBuiltInFader().SetAlwaysOnTop().Render();
    }
  }
}