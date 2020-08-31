using BattleTech;
using BattleTech.Data;
using SVGImporter;
using System;
using System.Collections.Generic;

namespace CustAmmoCategories {
  public class SVGImageLoadDelegate {
    public string id;
    public DataManager dataManager;
    public void onLoad() {

    }
  }
  public static class CustomSvgCache {
    private static Dictionary<string, SVGAsset> cache = new Dictionary<string, SVGAsset>();
    private static Dictionary<string, HashSet<SVGImage>> defferedRequests = new Dictionary<string, HashSet<SVGImage>>();
    public static void IconLoaded(string id, DataManager dataManager) {
      SVGAsset icon = get(id, dataManager);
      if (icon == null) { return; }
      if(defferedRequests.TryGetValue(id, out HashSet<SVGImage> images)) {
        foreach(SVGImage image in images) {
          image.vectorGraphics = icon;
        }
        defferedRequests.Remove(id);
      }
    }
    public static void setIcon(SVGImage img, string id, DataManager dataManager) {
      if (img == null) { return; }
      SVGAsset icon = get(id,dataManager);
      if (icon != null) { img.vectorGraphics = icon; return; }
      if(defferedRequests.TryGetValue(id, out HashSet<SVGImage> images) == false) {
        images = new HashSet<SVGImage>();
        defferedRequests.Add(id, images);
        return;
      }
      images.Add(img);
      SVGImageLoadDelegate dl = new SVGImageLoadDelegate();
      dl.id = id;
      DataManager.InjectedDependencyLoadRequest dependencyLoad = new DataManager.InjectedDependencyLoadRequest(dataManager);
      dependencyLoad.RequestResource(BattleTechResourceType.SVGAsset, id);
      dependencyLoad.RegisterLoadCompleteCallback(new Action(dl.onLoad));
      dataManager.InjectDependencyLoader(dependencyLoad, 1000U);
    }
    public static SVGAsset get(string id, DataManager dataManager) {
      if(CustomSvgCache.cache.TryGetValue(id,out SVGAsset result)) {
        return result;
      }
      result = dataManager.GetObjectOfType<SVGAsset>(id,BattleTechResourceType.SVGAsset);
      if(result != null) {
        cache.Add(id, result);
      }
      return result;
    }
  }
}