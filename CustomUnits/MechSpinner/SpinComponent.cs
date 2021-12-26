using UnityEngine;
using UnityEngine.EventSystems;

//from mech spinner by mpstark

namespace MechSpin {
  public class SpinComponent : MonoBehaviour {
    public float BleedPerSec = 450f;
    public float SpeedMultiplier = -360f;
    private float _curSpeed;
    private float _numDegrees;

    public void Update() {
      if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject()) {
        _curSpeed = SpeedMultiplier * Input.GetAxis("Mouse X");
        _numDegrees = 0;
      }

      var absoluteSpeed = Mathf.Abs(_curSpeed);
      var bleed = BleedPerSec * Time.deltaTime;
      if (absoluteSpeed <= bleed) {
        _curSpeed = 0f;
        return;
      }

      absoluteSpeed -= bleed;
      if (_curSpeed < 0)
        _curSpeed = -1 * absoluteSpeed;
      else
        _curSpeed = absoluteSpeed;

      var deltaAngle = _curSpeed * Time.deltaTime;
      transform.Rotate(0, deltaAngle, 0);

      _numDegrees += Mathf.Abs(deltaAngle);
      if (_numDegrees >= 360f) {
        Main.AddToCounter(Mathf.FloorToInt(_numDegrees / 360f));
        _numDegrees %= 360;
      }
    }
  }
}
