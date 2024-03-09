using UnityEngine;

namespace CustomUnitsHelper
{

  public static class TransformsHelper
  {
    public static T FindClass<T>(this GameObject obj, string name) where T : Component
    {
      foreach (T child in obj.GetComponentsInChildren<T>(true))
      {
        if (child.transform.name == name) { return child; }
      }
      return null;
    }

    public static T FindParent<T>(this GameObject obj, string name) where T : Component
    {
      Transform tr = obj.transform;
      while (tr != null)
      {
        if (tr.name == name) { return tr.gameObject.GetComponent<T>(); }
        tr = tr.parent;
      }
      return null;
    }

    public static Transform rootTransform(this Transform tr)
    {
      while (tr.parent != null) { tr = tr.parent; }
      return tr;
    }

    public static Transform j_Root(this Transform tr)
    {
      while (tr.parent != null) { tr = tr.parent; if (tr.name == "j_Root") { return tr; } }
      return null;
    }

  }
}
