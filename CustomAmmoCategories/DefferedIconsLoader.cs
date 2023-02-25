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
using BattleTech.Data;
using CustomAmmoCategoriesLog;
using HarmonyLib;
using SVGImporter;
using System;
using System.Collections.Generic;
using System.IO;

namespace CustAmmoCategories {
  public class SVGImageLoadDelegate {
    public string id;
    public DataManager dataManager;
    public void onLoad() {
      CustomSvgCache.IconLoaded(id,dataManager);
    }
  }
  [HarmonyPatch(typeof(DataManager))]
  [HarmonyPatch("PrewarmComplete")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(LoadRequest) })]
  public static class DataManager_PrewarmComplete {
    public static void Postfix(DataManager __instance, LoadRequest batch) {
      Log.M.TWL(0, "DataManager.PrewarmComplete");
      CustomSvgCache.flushRegistredSVGs(__instance);
    }
  }
  [HarmonyPatch(typeof(SVGCache))]
  [HarmonyPatch("Clear")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class SVGCache_Clear {
    public static bool Prefix(SVGCache __instance) {
      Log.M.TWL(0, "SVGCache.Clear NoSVGCacheClear:"+ CustomAmmoCategories.Settings.NoSVGCacheClear);
      return CustomAmmoCategories.Settings.NoSVGCacheClear == false;
    }
  }

  public static class CustomSvgCache {
    private static Dictionary<string, SVGAsset> cache = new Dictionary<string, SVGAsset>();
    private static Dictionary<string, HashSet<SVGImage>> defferedRequests = new Dictionary<string, HashSet<SVGImage>>();
    private static HashSet<string> registredSVGIcons = new HashSet<string>();
    public static void RegisterSVG(string id) {
      registredSVGIcons.Add(id);
    }
    public static void flushRegistredSVGs(DataManager dataManager) {
      foreach(string id in registredSVGIcons) {
        if (CustomSvgCache.cache.ContainsKey(id)) { continue; }
        SVGAsset icon = dataManager.GetObjectOfType<SVGAsset>(id,BattleTechResourceType.SVGAsset);
        if (icon != null) {
          Log.M.WL(1, "cache icon:"+id);
          CustomSvgCache.cache.Add(id,icon);
        };
      }
    }
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
      Log.M.TWL(0, "CustomSvgCache.setIcon "+id+" img:"+img.gameObject.name);
      SVGAsset icon = get(id,dataManager);
      if (icon != null) { img.vectorGraphics = icon; return; }
      try { 
      //if(defferedRequests.TryGetValue(id, out HashSet<SVGImage> images) == false) {
      //  images = new HashSet<SVGImage>();
      //  defferedRequests.Add(id, images);
      //  return;
      //}
      //images.Add(img);
        VersionManifestEntry entry = dataManager.ResourceLocator.EntryByID(id, BattleTechResourceType.SVGAsset);
        if(entry == null) {
          Log.M.TWL(0, id+" not found in SVG manifest");
          return;
        }
        SVGAsset svg = SVGAsset.Load(File.ReadAllText(entry.FilePath));
        if(svg != null) {
          Traverse.Create(dataManager).Property<SVGCache>("SVGCache").Value.AddSVGAsset(id, svg);
          CustomSvgCache.cache.Add(id, svg);
          Log.M.TWL(0, "Success load SVG:" + id + " " + entry.FilePath);
          img.vectorGraphics = svg;
        } else {
          Log.M.TWL(0, "Fail to load SVG:"+id+" "+ entry.FilePath);
        }
      }catch(Exception e) {
        Log.M.TWL(0, e.ToString());
      }
      //SVGImageLoadDelegate dl = new SVGImageLoadDelegate();
      //dl.id = id;
      //DataManager.InjectedDependencyLoadRequest dependencyLoad = new DataManager.InjectedDependencyLoadRequest(dataManager);
      //dependencyLoad.RequestResource(BattleTechResourceType.SVGAsset, id);
      //dependencyLoad.RegisterLoadCompleteCallback(new Action(dl.onLoad));
      //dataManager.InjectDependencyLoader(dependencyLoad, 1000U);
    }
    public static SVGAsset get(string id, DataManager dataManager) {
      Log.M.TWL(0, $"CustomSvgCache.get {id}");
      if (string.IsNullOrEmpty(id)) {
        Log.M?.WL(0,"Requested icon with empty name");
        Log.M?.WL(0, Environment.StackTrace);
        return null;
      }
      if(CustomSvgCache.cache.TryGetValue(id,out SVGAsset result)) {
        Log.M.WL(1, "found in cache");
        return result;
      }
      Log.M.WL(1, "cache content:");
      foreach(var icon in cache) {
        Log.M.WL(2, icon.Key + ":" + (icon.Value == null ? "null" : "not null"));
      }
      result = dataManager.GetObjectOfType<SVGAsset>(id,BattleTechResourceType.SVGAsset);
      if(result != null) {
        Log.M.WL(1, "found in data manager");
        cache.Add(id, result);
      }
      return result;
    }
  }
}