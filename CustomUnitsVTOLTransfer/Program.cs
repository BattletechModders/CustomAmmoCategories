using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomUnitsVTOLTransfer {
  class Program {
    static void Main(string[] args) {
      string mainPath = Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase),"..");
      if (args.Length > 0) { mainPath = args[0]; }
      string[] jsons = Directory.GetFiles(mainPath,"*.json",SearchOption.AllDirectories);
      foreach(string fname in jsons) {
        if (Path.GetFileName(fname) == "mod.json") { continue; }
        if (Path.GetFileName(fname) == "modstate.json") { continue; }
        try {
          string content = File.ReadAllText(fname);
          JObject json = JObject.Parse(File.ReadAllText(fname));
          if (json["PrefabIdentifier"] == null) { continue; }
          if (json["CustomParts"] == null) { continue; }
          if ((string)json["PrefabIdentifier"] != "chrprfvhcl_warriorvtol") { continue; }
          if ((string)json["PrefabIdentifier"] == "chrprfvhcl_warriorvtol") { json["PrefabIdentifier"] = "chrprfmech_warrior_vtol"; }
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
          File.WriteAllText(Path.Combine(Path.GetDirectoryName(fname),Path.GetFileNameWithoutExtension(fname)+".old~"), content);
          File.WriteAllText(fname, json.ToString(Newtonsoft.Json.Formatting.Indented));
        } catch (Exception e) {
          Console.WriteLine(fname);
          Console.WriteLine(e.ToString());
        }
      }
      Console.WriteLine("Finished!");
      Console.ReadKey();

    }
  }
}
