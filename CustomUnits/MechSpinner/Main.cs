using System.Collections;
using System.Reflection;
using BattleTech;
using HarmonyLib;
using HBS.Logging;
using TMPro;
using UnityEngine;
using Logger = HBS.Logging.Logger;
// ReSharper disable AccessToStaticMemberViaDerivedType
//from mech spinner by mpstark
namespace MechSpin {
  public static class Main {

    internal static GameObject CounterGameObject;
    internal static TextMeshProUGUI CounterText;
    internal static string StatName = "mechSpin_num_spins";
    internal static float CelebrateTime = 5f;
    internal static bool IsCelebrating;
    // CODE QUALITY IS BAD YEAH I KNOW
    internal static void SetupSpin(GameObject mech) {
      if (mech.GetComponent<SpinComponent>() == null)
        mech.AddComponent<SpinComponent>();
    }

    internal static void SetupUI(Transform panelTransform) {
      if (CounterGameObject != null) {
        GameObject.Destroy(CounterGameObject);
        CounterText = null;
        CounterGameObject = null;
      }

      CounterGameObject = new GameObject("MechSpin-Counter");
      CounterGameObject.AddComponent<RectTransform>();
      CounterText = CounterGameObject.AddComponent<TextMeshProUGUI>();
      CounterText.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 100);
      CounterText.alignment = TextAlignmentOptions.Left;

      CounterGameObject.transform.SetParent(panelTransform);

      var fonts = Resources.FindObjectsOfTypeAll(typeof(TMP_FontAsset));
      foreach (var o in fonts) {
        var font = (TMP_FontAsset)o;
        if (font.name == "UnitedSansReg-Medium SDF") {
          CounterText.font = font;
          CounterText.fontSize = 20;
          break;
        }
      }

        ((RectTransform)CounterGameObject.transform).anchoredPosition = new Vector3(-670, -240, 0);
      RefreshCounter();
    }

    internal static void RefreshCounter() {
      var simGame = UnityGameInstance.BattleTechGame.Simulation;
      if (simGame == null)
        return;

      if (!simGame.CompanyStats.ContainsStatistic(StatName))
        simGame.CompanyStats.AddStatistic(StatName, 0);

      var currentSpins = simGame.CompanyStats.GetValue<int>(StatName);
      CounterText.text = $"{currentSpins}";

      if (simGame.CameraController != null && currentSpins != 0
              && currentSpins % 100 == 0 && !IsCelebrating)
        simGame.CameraController.StartCoroutine(Celebrate());
    }

    internal static void AddToCounter(int spins) {
      var simGame = UnityGameInstance.BattleTechGame.Simulation;
      if (simGame == null || spins <= 0)
        return;

      if (!simGame.CompanyStats.ContainsStatistic(StatName))
        simGame.CompanyStats.AddStatistic(StatName, 0);

      var currentSpins = simGame.CompanyStats.GetValue<int>(StatName);
      simGame.CompanyStats.Set(StatName, currentSpins + spins);

      RefreshCounter();
    }

    internal static IEnumerator Celebrate() {
      IsCelebrating = true;
      CounterText.fontSize = 60;
      ((RectTransform)CounterGameObject.transform).anchoredPosition = new Vector3(-670, -230, 0);

      var t = 0f;
      while (t <= CelebrateTime) {
        CounterText.color = HSBColor.ToColor(new HSBColor(Mathf.PingPong(Time.time * 1f, 1), 1, 1));
        t += Time.deltaTime;
        yield return null;
      }

      CounterText.fontSize = 20;
      ((RectTransform)CounterGameObject.transform).anchoredPosition = new Vector3(-670, -240, 0);
      CounterText.color = Color.white;
      IsCelebrating = false;
    }
  }
}
