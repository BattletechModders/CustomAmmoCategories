using BattleTech;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using CustomAmmoCategoriesLog;
using Harmony;
using HBS;
using Localize;
using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace CustAmmoCategoriesPatches {
  public class TextRotateToCamera: MonoBehaviour {
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
      }
    //  Log.M.TWL(0, "PersistentFloatieMessage.Update "+(parentTransform.position + offset) + "->"+ this.transform.position);
    }
    public void Init(CombatHUD HUD, Transform parent, Vector3 offset) {
      this.HUD = HUD;
      //this.combatHUDInWorldElementMgr = combatHUDInWorldElementMgr;
      Text = gameObject.GetComponent<TextMeshProUGUI>();
      object node = typeof(UIManager).GetField("inWorldNode", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(LazySingletonBehavior<UIManager>.Instance);
      this.transform.SetParent((Transform)node.GetType().GetField("nodeTransform").GetValue(node),false);
      parentTransform = parent;
      this.offset = offset;
      if (parentTransform != null) { this.transform.position = HUD.GetInWorldScreenPos(parentTransform.position + offset); } else {
        this.transform.position = HUD.GetInWorldScreenPos(parentTransform.position + offset);
      }
      //this.transform.position = combatHUDInWorldElementMgr.GetInWorldScreenPos(parentTransform.position + offset);
      if (Text == null) {
        CanvasRenderer canvas = gameObject.GetComponentInChildren<CanvasRenderer>();
        if (canvas != null) {
          Text = canvas.gameObject.AddComponent<TextMeshProUGUI>();
          //FontInited = false;
          Text.font = HUD.SidePanel.WarningText.font;
          Text.overflowMode = TextOverflowModes.Overflow;
          Text.enableWordWrapping = false;
          Text.alignment = TextAlignmentOptions.Center;
        }
      }
      TextRotateToCamera rt = gameObject.GetComponent<TextRotateToCamera>();
      if(rt == null) {
        rt = gameObject.AddComponent<TextRotateToCamera>();
      }
    }
  }
  public static class PersistentFloatieHelper {
    private static CombatHUD HUD = null;
    //private static CombatHUDInWorldElementMgr combatHUDInWorldElementMgr = null;
    public static GameObject CreateFloatie(Text text,float size,Color color,Transform parent,Vector3 offset) {
      GameObject obj = null;
      try {
        obj = HUD.Combat.DataManager.PooledInstantiate("PersistentMessage", BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
        obj.layer = LayerMask.NameToLayer("UI_InWorld");
        PersistentFloatieMessage floatie = obj.GetComponent<PersistentFloatieMessage>();
        if(floatie == null) {
          floatie = obj.AddComponent<PersistentFloatieMessage>();
          floatie.Init(PersistentFloatieHelper.HUD, parent, offset);
        }
        if (floatie.Text != null) {
          floatie.Text.color = color;
          floatie.Text.fontSize = size;
          floatie.Text.SetText(text.ToString());
        }
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
      return obj;
    }
    public static void Init(CombatHUD HUD, CombatHUDInWorldElementMgr combatHUDInWorldElementMgr) {
      PersistentFloatieHelper.HUD = HUD;
      //PersistentFloatieHelper.combatHUDInWorldElementMgr = combatHUDInWorldElementMgr;
      List<AbstractActor> actors = HUD.Combat.AllActors;
      Log.M.TWL(0, "PersistentFloatieHelper.Init");
      foreach(AbstractActor actor in actors) {
        try {
          Transform pt = null;
          Mech mech = actor as Mech;
          Vehicle vehicle = actor as Vehicle;
          if (mech != null) { pt = mech.GetAttachTransform(ChassisLocations.Head); };
          if (vehicle != null) { pt = vehicle.GetAttachTransform(VehicleChassisLocations.Turret); };
          if (pt == null) {continue; }
          float up = pt.position.y - actor.GameRep.transform.position.y;
          //GameObject obj = PersistentFloatieHelper.CreateFloatie(new Text("INFO"),24f,Color.white, actor.GameRep.transform, (Vector3.up * 10f) + (Vector3.up)*up);
          //Log.M.printComponents(obj, 1);
        } catch (Exception e) {
          Log.M.TWL(0, e.ToString(), true);
        }
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUD))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(CombatGameState) })]
  public static class CombatHUD_Init_Postfix {
    public static void Postfix(CombatHUD __instance, CombatGameState Combat) {
      PersistentFloatieHelper.Init(__instance,null);
    }
  }
}