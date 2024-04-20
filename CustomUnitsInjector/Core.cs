using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil.Cil;

namespace CustomUnitsInjector {
  public class Settings {
    public bool debugLog { get; set; } = true;
  }
  internal static class Injector {
    public static Settings settings { get; set; } = new Settings();
    public static string AssemblyDirectory {
      get {
        string codeBase = Assembly.GetExecutingAssembly().CodeBase;
        UriBuilder uri = new UriBuilder(codeBase);
        string path = Uri.UnescapeDataString(uri.Path);
        return Path.GetDirectoryName(path);
      }
    }
    internal static AssemblyDefinition game { get; set; } = null;
    public static void Inject(IAssemblyResolver resolver) {
      Log.BaseDirectory = AssemblyDirectory;
      Log.InitLog();
      Log.Err?.TWL(0, $"ComponentDefInjector initing {Assembly.GetExecutingAssembly().GetName().Version}", true);
      try {
        game = resolver.Resolve(new AssemblyNameReference("Assembly-CSharp", null));
        if (game == null) {
          Log.Err?.WL(1, "can't resolve main game assembly", true);
          return;
        }
        TypeDefinition LocationDef = game.MainModule.GetType("BattleTech.LocationDef");
        if (LocationDef == null) {
          Log.Err?.WL(1, "can't resolve BattleTech.LocationDef type", true);
          return;
        }
        Log.M?.WL(1, "fields before:");
        foreach (var field in LocationDef.Fields) {
          field.IsInitOnly = false;
          Log.M?.WL(2, $"{field.Name} readonly:{field.IsInitOnly}");
        }
        FieldDefinition _test_field = new FieldDefinition("_test_field", Mono.Cecil.FieldAttributes.Public, game.MainModule.ImportReference(typeof(string)));
        LocationDef.Fields.Add(_test_field);

      } catch (Exception e) {
        Log.Err?.TWL(0, e.ToString(), true);
      }
    }
  }
}
