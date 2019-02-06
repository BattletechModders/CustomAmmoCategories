using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using System.Reflection;

namespace HarmonyTest
{
    public class HarmonyTestClass
    {
        public int HarmonyTestIntProperty
        {
            get {
                System.Console.WriteLine("original HarmonyTestIntProperty");
                Random rnd = new Random();
                return rnd.Next(0, 100);
            }
        }
        public bool HarmonyTestBoolProperty
        {
            get
            {
                System.Console.WriteLine("original HarmonyTestBoolProperty");
                Random rnd = new Random();
                return rnd.Next(0, 100) > 50;
            }

        }
    }
    [HarmonyPatch(typeof(HarmonyTestClass))]
    [HarmonyPatch("HarmonyTestIntProperty")]
    [HarmonyPatch(MethodType.Getter)]
    [HarmonyPatch(new Type[] { })]
    public static class HarmonyTestClass_HarmonyTestIntProperty
    {
        public static void Postfix(HarmonyTestClass __instance)
        {
            System.Console.WriteLine("patched HarmonyTestIntProperty");
        }
    }
    [HarmonyPatch(typeof(HarmonyTestClass))]
    [HarmonyPatch("HarmonyTestBoolProperty")]
    [HarmonyPatch(MethodType.Getter)]
    [HarmonyPatch(new Type[] { })]
    public static class HarmonyTestClass_HarmonyTestBoolProperty
    {
        public static void Postfix(HarmonyTestClass __instance)
        {
            System.Console.WriteLine("patched HarmonyTestBoolProperty");
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var harmony = HarmonyInstance.Create("io.mission.harmonytest");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            HarmonyTestClass test = new HarmonyTestClass();
            Console.WriteLine("HarmonyTestIntProperty = " + test.HarmonyTestIntProperty); 
            Console.WriteLine("HarmonyTestBoolProperty = " + test.HarmonyTestBoolProperty);
        }
    }
}
