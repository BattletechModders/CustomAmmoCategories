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
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using CustAmmoCategories;
using CustomAmmoCategoriesLog;
using HarmonyLib;
using HBS;
using Localize;
using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace CustAmmoCategories {
  public class TextRotateToCamera : MonoBehaviour {
    void Update() {
      //Vector3 v = Camera.main.transform.position - transform.position;
      //v.x = v.z = 0.0f;
      //transform.LookAt(Camera.main.transform.position - v);
      //transform.Rotate(0, 180, 0);
    }
  }
  public class PersistentFloatieMessage : MonoBehaviour {
    public TextMeshProUGUI Text { get; set; }
    public Transform parentTransform { get; set; }
    public Vector3 offset { get; set; }
    public CombatHUD HUD { get; set; }
    //public CombatHUDInWorldElementMgr combatHUDInWorldElementMgr { get; set; }
    //private bool FontInited = false;
    public void LateUpdate() {
      if (parentTransform != null) {
        this.transform.position = HUD.GetInWorldScreenPos(parentTransform.position + offset);
      } else {
        this.transform.position = HUD.GetInWorldScreenPos(offset);
      }
      //  Log.M.TWL(0, "PersistentFloatieMessage.Update "+(parentTransform.position + offset) + "->"+ this.transform.position);
    }
    public void Init(CombatHUD HUD, Transform parent, Vector3 offset) {
      this.HUD = HUD;
      //this.combatHUDInWorldElementMgr = combatHUDInWorldElementMgr;
      this.Text = gameObject.GetComponent<TextMeshProUGUI>();
      if (this.Text == null) {
        TextMeshProUGUI[] texts = gameObject.GetComponentsInChildren<TextMeshProUGUI>();
        if (texts.Length > 0) { this.Text = texts[0]; }
      }
      var node = LazySingletonBehavior<UIManager>.Instance.inWorldNode;
      this.transform.SetParent(node.nodeTransform, false);
      parentTransform = parent;
      this.offset = offset;
      if (parentTransform != null) { this.transform.position = HUD.GetInWorldScreenPos(parentTransform.position + offset); } else {
        this.transform.position = HUD.GetInWorldScreenPos(offset);
      }
      //this.transform.position = combatHUDInWorldElementMgr.GetInWorldScreenPos(parentTransform.position + offset);
      if (this.Text == null) {
        CanvasRenderer canvas = gameObject.GetComponentInChildren<CanvasRenderer>();
        if (canvas != null) {
          this.Text = canvas.gameObject.AddComponent<TextMeshProUGUI>();
        } else {
          this.Text = gameObject.AddComponent<TextMeshProUGUI>();
        }
        //FontInited = false;
        this.Text.font = HUD.SidePanel.WarningText.font;
        this.Text.overflowMode = TextOverflowModes.Overflow;
        this.Text.enableWordWrapping = false;
        this.Text.alignment = TextAlignmentOptions.Center;
      }
      TextRotateToCamera rt = gameObject.GetComponent<TextRotateToCamera>();
      if (rt == null) {
        rt = gameObject.AddComponent<TextRotateToCamera>();
      }
      this.Text.raycastTarget = false;
    }
  }
  public static class PersistentFloatieHelper {
    private static HashSet<PersistentFloatieMessage> allFloaties = new HashSet<PersistentFloatieMessage>();
    public static CombatHUD HUD { get; private set; } = null;
    //private static CombatHUDInWorldElementMgr combatHUDInWorldElementMgr = null;
    public static PersistentFloatieMessage CreateFloatie(Text text, float size, Color color, Transform parent, Vector3 offset) {
      GameObject obj = null;
      PersistentFloatieMessage floatie = null;
      try {
        obj = HUD.Combat.DataManager.PooledInstantiate("PersistentMessage", BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
        if (obj == null) { obj = new GameObject("PersistentMessage"); }
        obj.layer = LayerMask.NameToLayer("UI_InWorld");
        floatie = obj.GetComponent<PersistentFloatieMessage>();
        if (floatie == null) {
          floatie = obj.AddComponent<PersistentFloatieMessage>();
          floatie.Init(PersistentFloatieHelper.HUD, parent, offset);
        }
        if (floatie.Text != null) {
          floatie.Text.color = color;
          floatie.Text.fontSize = size;
          floatie.Text.SetText(text.ToString());
        }
        obj.SetActive(true);
        allFloaties.Add(floatie);
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        CombatHUD.uiLogger.LogException(e);
      }
      return floatie;
    }
    public static void Clear() {
      Log.Combat?.TWL(0, "PersistentFloatieHelper.Clear:"+ allFloaties.Count);
      try {
        foreach(PersistentFloatieMessage msg in allFloaties) {
          try {
            Log.Combat?.WL(1, "message:" + msg.Text.text);
            GameObject.Destroy(msg.gameObject);
          } catch(Exception e) {
            Log.Combat?.TWL(0, e.ToString(), true);
            CombatHUD.uiLogger.LogException(e);
          }
        }
        allFloaties.Clear();
      }catch(Exception e) {
        Log.Combat?.TWL(0,e.ToString());
        CombatHUD.uiLogger.LogException(e);
      }
    }
    public static void PoolFloatie(PersistentFloatieMessage msg) {
      Log.Combat?.TWL(0, "PersistentFloatieHelper.PoolFloatie");
      if (msg == null) { return; }
      if (msg.gameObject == null) { return; }
      allFloaties.Remove(msg);
      msg.gameObject.SetActive(false);
      try {
        Log.Combat?.WL(1, "message:" + msg.Text.text);
      }catch(Exception) {
        Log.Combat?.WL(1, "message without text? who am i to jugge");
      }
      PersistentFloatieHelper.HUD.Combat.DataManager.PoolGameObject("PersistentMessage", msg.gameObject);
    }
    public static void Init(CombatHUD HUD) {
      PersistentFloatieHelper.HUD = HUD;
      //PersistentFloatieHelper.combatHUDInWorldElementMgr = combatHUDInWorldElementMgr;
      List<AbstractActor> actors = HUD.Combat.AllActors;
      Log.Combat?.TWL(0, "PersistentFloatieHelper.Init");
      foreach (AbstractActor actor in actors) {
        try {
          Transform pt = null;
          Mech mech = actor as Mech;
          Vehicle vehicle = actor as Vehicle;
          if (mech != null) { pt = mech.GetAttachTransform(ChassisLocations.Head); };
          if (vehicle != null) { pt = vehicle.GetAttachTransform(VehicleChassisLocations.Turret); };
          if (pt == null) { continue; }
          float up = pt.position.y - actor.GameRep.transform.position.y;
          //GameObject obj = PersistentFloatieHelper.CreateFloatie(new Text("INFO"),24f,Color.white, actor.GameRep.transform, (Vector3.up * 10f) + (Vector3.up)*up);
          //Log.M.printComponents(obj, 1);
        } catch (Exception e) {
          Log.Combat?.TWL(0, e.ToString(), true);
          CombatHUD.uiLogger.LogException(e);
        }
      }
    }
  }
}
