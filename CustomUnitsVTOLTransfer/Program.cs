using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomUnitsVTOLTransfer {
  public class VTOL_replace {
    public string PrefabIdentifier;
    public string HardpointDataDefID;
    public string PrefabBase;
  }
  class Program {
    static void Main(string[] args) {
      string mainPath = Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase),"..");
      if (args.Length > 0) { mainPath = args[0]; }
      string[] jsons = Directory.GetFiles(mainPath,"*.json",SearchOption.AllDirectories);
      //HashSet<string> VTOLBody_prefabs = new HashSet<string>();
      Dictionary<string, VTOL_replace> VTOLBody_prefab_repaces = new Dictionary<string, VTOL_replace>();
      VTOLBody_prefab_repaces.Add("warrior_body", new VTOL_replace() { PrefabIdentifier = "chrprfmech_warrior_vtol", HardpointDataDefID = "hardpointdatadef_warriorvtol", PrefabBase = "warriorvtol" });
      VTOLBody_prefab_repaces.Add("yellowjacket_body", new VTOL_replace() { PrefabIdentifier = "chrprfmech_yellowjacket_vtol", HardpointDataDefID = "hardpointdatadef_yellowjacket", PrefabBase = "yellowjacket" });
      VTOLBody_prefab_repaces.Add("strix_body", new VTOL_replace() { PrefabIdentifier = "chrprfmech_strix_vtol", HardpointDataDefID = "hardpointdatadef_strix", PrefabBase = "strix" });
      VTOLBody_prefab_repaces.Add("chrprfvhcl_urbicopter", new VTOL_replace() { PrefabIdentifier = "chrprfmech_urbiecopter_vtol", HardpointDataDefID = "hardpointdatadef_urbiecopter", PrefabBase = "urbiecopter" });
      foreach (string fname in jsons) {
        if (Path.GetFileName(fname) == "mod.json") { continue; }
        if (Path.GetFileName(fname) == "modstate.json") { continue; }
        //try {
        string content = File.ReadAllText(fname);
        JObject json = null;
        try {
          json = JObject.Parse(File.ReadAllText(fname));
        } catch (Exception e) {
          continue;
        }
        if (json["PrefabIdentifier"] == null) { continue; }
        if (json["CustomParts"] == null) { continue; }
        if (json["CustomParts"]["CustomParts"] == null) { continue; }
        JArray CustomParts = (JArray)json["CustomParts"]["CustomParts"];
        string VTOLBody_prefab = string.Empty;
        foreach (JToken CustomPart in CustomParts) {
          if (CustomPart["AnimationType"] == null) { continue; }
          string AnimationType = (string)CustomPart["AnimationType"];
          if (string.IsNullOrEmpty(AnimationType)) { continue; }
          //Console.WriteLine(" "+ AnimationType);
          if (AnimationType != "VTOLBody") { continue; }
          if (CustomPart["prefab"] == null) { continue; }
          VTOLBody_prefab = (string)CustomPart["prefab"];
          break;
          //VTOLBody_prefabs.Add((string)CustomPart["prefab"]);
        }
        if (VTOLBody_prefab_repaces.TryGetValue(VTOLBody_prefab, out VTOL_replace prefab_replace) == false) { continue; }

        //if ((string)json["PrefabIdentifier"] != "chrprfvhcl_warriorvtol") { continue; }
        //if ((string)json["PrefabIdentifier"] == "chrprfvhcl_warriorvtol") { json["PrefabIdentifier"] = "chrprfmech_warrior_vtol"; }
        json["PrefabIdentifier"] = prefab_replace.PrefabIdentifier;
        json["HardpointDataDefID"] = prefab_replace.HardpointDataDefID;
        json["PrefabBase"] = prefab_replace.PrefabBase;
        if (json["CustomParts"]["CustomParts"] != null) { (json["CustomParts"] as JObject).Remove("CustomParts"); }
        if (json["CustomParts"]["HighestLOSPosition"] != null) { (json["CustomParts"] as JObject).Remove("HighestLOSPosition"); }
        if (json["CustomParts"]["HangarTransforms"] != null) { (json["CustomParts"] as JObject).Remove("HangarTransforms"); }
        if (json["CustomParts"]["TurretAttach"] != null) { (json["CustomParts"] as JObject).Remove("TurretAttach"); }
        if (json["CustomParts"]["BodyAttach"] != null) { (json["CustomParts"] as JObject).Remove("BodyAttach"); }
        if (json["CustomParts"]["TurretLOS"] != null) { (json["CustomParts"] as JObject).Remove("TurretLOS"); }
        if (json["CustomParts"]["LeftSideLOS"] != null) { (json["CustomParts"] as JObject).Remove("LeftSideLOS"); }
        if (json["CustomParts"]["TurretLOS"] != null) { (json["CustomParts"] as JObject).Remove("TurretLOS"); }
        if (json["CustomParts"]["RightSideLOS"] != null) { (json["CustomParts"] as JObject).Remove("RightSideLOS"); }
        if (json["CustomParts"]["leftVFXTransform"] != null) { (json["CustomParts"] as JObject).Remove("leftVFXTransform"); }
        if (json["CustomParts"]["rightVFXTransform"] != null) { (json["CustomParts"] as JObject).Remove("rightVFXTransform"); }
        if (json["CustomParts"]["rearVFXTransform"] != null) { (json["CustomParts"] as JObject).Remove("rearVFXTransform"); }
        if (json["CustomParts"]["thisTransform"] != null) { (json["CustomParts"] as JObject).Remove("thisTransform"); }
        Console.WriteLine(fname);
        //File.WriteAllText(Path.Combine(Path.GetDirectoryName(fname),Path.GetFileNameWithoutExtension(fname)+".old~"), content);
        File.WriteAllText(fname, json.ToString(Newtonsoft.Json.Formatting.Indented));
        //} catch (Exception e) {
          //Console.WriteLine(fname);
          //Console.WriteLine(e.ToString());
        //}
      }
      //foreach(string prefab in VTOLBody_prefabs) {
        //Console.WriteLine(prefab);
      //}
      Console.WriteLine("Finished!");
      Console.ReadKey();

    }
  }
}
