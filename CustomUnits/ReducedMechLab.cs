using BattleTech.UI;
using HarmonyLib;
using System;
using UnityEngine;

namespace CustomUnits {
  public class ReducedMechLabLocationWidget: MonoBehaviour {
    public GameObject reducedLoadoutGo = null;
    public MechLabPanel mechLabPanel = null;
    public static ReducedMechLabLocationWidget Instantine(MechLabPanel mechLab) {
      GameObject widgetParent = UIManager.Instance.dataManager.PooledInstantiate("uixPrfPanl_SIM_mechBayUnitInfo-Widget", BattleTech.BattleTechResourceType.UIModulePrefabs);
      ReducedMechLabLocationWidget reducedLocationsWidget = mechLab.gameObject.AddComponent<ReducedMechLabLocationWidget>();
      reducedLocationsWidget.mechLabPanel = mechLab;
      LanceMechEquipmentList mechInventoryWidgetSrc = widgetParent.GetComponentInChildren<LanceMechEquipmentList>(true);
      reducedLocationsWidget.reducedLoadoutGo = GameObject.Instantiate(mechInventoryWidgetSrc.gameObject.transform.parent.gameObject);
      reducedLocationsWidget.reducedLoadoutGo.transform.SetParent(mechLab.centerTorsoWidget.transform.parent);

      UIManager.Instance.dataManager.PoolGameObject("uixPrfPanl_SIM_mechBayUnitInfo-Widget", widgetParent);
      return reducedLocationsWidget;
    }
    public void UpdateData() {
      if (mechLabPanel.originalMechDef.IsVehicle()) {
        MechLabLocationWidget[] locationWidgets = mechLabPanel.gameObject.GetComponentsInChildren<MechLabLocationWidget>(true);
        foreach (var widget in locationWidgets) {
          widget.gameObject.SetActive(false);
        }
        reducedLoadoutGo.SetActive(true);
      } else {
        reducedLoadoutGo.SetActive(false);
      }
    }
  }
  public static class MechLabPanel_InitWidgets_Reduced {
    public static void Prefix(MechLabPanel __instance) {
      try {
        MechLabLocationWidget[] locationWidgets = __instance.gameObject.GetComponentsInChildren<MechLabLocationWidget>(true);
        foreach (var widget in locationWidgets) {
          widget.gameObject.SetActive(true);
        }
      } catch (Exception e) {
        UIManager.logger.LogException(e);
        CustomUnits.Log.M?.TWL(0, e.ToString());
      }
    }
    public static void Postfix(MechLabPanel __instance) {
      try {
        ReducedMechLabLocationWidget widget = __instance.gameObject.GetComponent<ReducedMechLabLocationWidget>();
        if (widget == null) { widget = ReducedMechLabLocationWidget.Instantine(__instance); }
        widget.UpdateData();
      } catch (Exception e) {
        UIManager.logger.LogException(e);
        CustomUnits.Log.M?.TWL(0, e.ToString());
      }
    }
  }
}