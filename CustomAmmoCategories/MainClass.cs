using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Harmony;
using HBS.Util;
using BattleTech;
using BattleTech.UI;
using HBS.Collections;
using Localize;
using BattleTech.Data;
using UnityEngine.EventSystems;
using CustAmmoCategories;
using UnityEngine;
using HBS;

namespace CustomAmmoCategoriesLog
{
    public static class Log
    {
        //private static string m_assemblyFile;
        private static string m_logfile;
        public static string BaseDirectory;
        public static void InitLog()
        {
            //Log.m_assemblyFile = (new System.Uri(Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath;
            Log.m_logfile = Path.Combine(BaseDirectory, "CustomAmmoCategories.log");
            //Log.m_logfile = Path.Combine(Log.m_logfile, "CustomAmmoCategories.log");
            File.Delete(Log.m_logfile);
        }
        public static void LogWrite(string line)
        {
            if (CustomAmmoCategories.Settings.debugLog)
            {
                File.AppendAllText(Log.m_logfile, line);
            }
        }
    }
}

namespace CustomAmmoCategoriesPatches
{
    [HarmonyPatch(typeof(CombatHUDWeaponSlot))]
    [HarmonyPatch("OnPointerDown")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(PointerEventData) })]
    public static class CombatHUDWeaponSlot_OnPointerDown
    {
        public static bool Prefix(CombatHUDWeaponSlot __instance, PointerEventData eventData)
        {

            CustomAmmoCategoriesLog.Log.LogWrite("CombatHUDWeaponSlot.OnPointerDown\n");
            if (eventData.button != PointerEventData.InputButton.Left) {return true;}
            if (__instance.weaponSlotType != CombatHUDWeaponSlot.WeaponSlotType.Normal) { return true; }
            if (__instance.DisplayedWeapon == null) { return true; }
            Camera mainUiCamera = LazySingletonBehavior<UIManager>.Instance.UICamera;
            if (mainUiCamera == null)
            {
                CustomAmmoCategoriesLog.Log.LogWrite("  can't get UI camera\n");
            }
            Vector3 worldClickPos = mainUiCamera.ScreenToWorldPoint(eventData.position);
            Vector3[] corners = new Vector3[4];
            __instance.GetComponent<RectTransform>().GetWorldCorners(corners);
            float width = corners[2].x - corners[0].x;
            float height = __instance.GetComponent<RectTransform>().rect.height;
            float clickXrel = worldClickPos.x - __instance.transform.position.x;
            bool trigger = clickXrel > (width / 2.0f);
            CustomAmmoCategoriesLog.Log.LogWrite("  instance.l = " + __instance.transform.position.x + "\n");
            CustomAmmoCategoriesLog.Log.LogWrite("  instance.t = " + __instance.transform.position.y + "\n");
            CustomAmmoCategoriesLog.Log.LogWrite("  instance.r = " + (__instance.transform.position.x + width) + "\n");
            CustomAmmoCategoriesLog.Log.LogWrite("  instance.b = " + (__instance.transform.position.y + height) + "\n");
            CustomAmmoCategoriesLog.Log.LogWrite("  position.x = " + eventData.position.x + "\n");
            CustomAmmoCategoriesLog.Log.LogWrite("  position.y = " + eventData.position.y + "\n");
            CustomAmmoCategoriesLog.Log.LogWrite("  worldClickPos.x = " + worldClickPos.x + "\n");
            CustomAmmoCategoriesLog.Log.LogWrite("  worldClickPos.y = " + worldClickPos.y + "\n");
            CustomAmmoCategoriesLog.Log.LogWrite("  clickXrel = " + clickXrel + "\n");
            CustomAmmoCategoriesLog.Log.LogWrite("  width = " + width + "\n");
            CustomAmmoCategoriesLog.Log.LogWrite("  trigger = " + trigger + "\n");
            if (trigger)
            {
                CustomAmmoCategories.CycleAmmo(__instance.DisplayedWeapon);
                __instance.RefreshDisplayedWeapon((ICombatant)null);
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(CombatHUDWeaponSlot))]
    [HarmonyPatch("OnPointerUp")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(PointerEventData) })]
    public static class CombatHUDWeaponSlot_OnPointerUp
    {
        public static bool Prefix(CombatHUDWeaponSlot __instance, PointerEventData eventData)
        {

            Camera mainUiCamera = LazySingletonBehavior<UIManager>.Instance.UICamera;
            if (mainUiCamera == null)
            {
                CustomAmmoCategoriesLog.Log.LogWrite("  can't get UI camera\n");
            }
            Vector3 worldClickPos = mainUiCamera.ScreenToWorldPoint(eventData.position);
            Vector3[] corners = new Vector3[4];
            __instance.GetComponent<RectTransform>().GetWorldCorners(corners);
            float width = corners[2].x - corners[0].x;
            float height = __instance.GetComponent<RectTransform>().rect.height;
            float clickXrel = worldClickPos.x - __instance.transform.position.x;
            bool trigger = clickXrel > (width / 2.0f);
            CustomAmmoCategoriesLog.Log.LogWrite("  instance.l = " + __instance.transform.position.x + "\n");
            CustomAmmoCategoriesLog.Log.LogWrite("  instance.t = " + __instance.transform.position.y + "\n");
            CustomAmmoCategoriesLog.Log.LogWrite("  instance.r = " + (__instance.transform.position.x + width) + "\n");
            CustomAmmoCategoriesLog.Log.LogWrite("  instance.b = " + (__instance.transform.position.y + height) + "\n");
            CustomAmmoCategoriesLog.Log.LogWrite("  position.x = " + eventData.position.x + "\n");
            CustomAmmoCategoriesLog.Log.LogWrite("  position.y = " + eventData.position.y + "\n");
            CustomAmmoCategoriesLog.Log.LogWrite("  worldClickPos.x = " + worldClickPos.x + "\n");
            CustomAmmoCategoriesLog.Log.LogWrite("  worldClickPos.y = " + worldClickPos.y + "\n");
            CustomAmmoCategoriesLog.Log.LogWrite("  clickXrel = " + clickXrel + "\n");
            CustomAmmoCategoriesLog.Log.LogWrite("  width = " + width + "\n");
            CustomAmmoCategoriesLog.Log.LogWrite("  trigger = " + trigger + "\n");
            if (trigger)
            {
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(CombatHUDWeaponSlot))]
    [HarmonyPatch("RefreshHighlighted")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { })]
    public static class CombatHUDWeaponSlot_RefreshHighlighted
    {
        public static bool Prefix(CombatHUDWeaponSlot __instance)
        {
            if (__instance.DisplayedWeapon == null) { return false; };
            return true;
        }
    }

    [HarmonyPatch(typeof(MechComponent))]
    [HarmonyPatch("UIName")]
    [HarmonyPatch(MethodType.Getter)]
    [HarmonyPatch(new Type[] { })]
    public static class MechComponent_UIName
    {
        public static bool Prefix(MechComponent __instance, ref Text __result)
        {
            //__instance.WeaponText.SetText("Weapon Name");
            if (__instance.componentType != ComponentType.Weapon) { return true; }
            __result = new Text(!string.IsNullOrEmpty(__instance.componentDef.Description.UIName) ? __instance.componentDef.Description.UIName : __instance.Name, new object[0]);
            if(CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.AmmoIdStatName) == false){return true;}
            string CurrentAmmoId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
            if (__instance.parent.Combat.DataManager.AmmoDefs.Exists(CurrentAmmoId) == false) { return true; }
            string ammoBoxName = "";
            if (string.IsNullOrEmpty(__instance.parent.Combat.DataManager.AmmoDefs.Get(CurrentAmmoId).Description.UIName))
            {
                ammoBoxName = __instance.parent.Combat.DataManager.AmmoDefs.Get(CurrentAmmoId).Description.Name;
            }
            else
            {
                ammoBoxName = __instance.parent.Combat.DataManager.AmmoDefs.Get(CurrentAmmoId).Description.UIName;
            }
            __result.Append("({0})", new object[1] { (object)ammoBoxName });
            return false;
        }
    }

    [HarmonyPatch(typeof(Weapon))]
    [HarmonyPatch("DamagePerShot")]
    [HarmonyPatch(MethodType.Getter)]
    [HarmonyPatch(new Type[] { })]
    public static class Weapon_DamagePerShot
    {
        public static bool Prefix(Weapon __instance, ref float __result)
        {
            if (__instance.AmmoCategory == AmmoCategory.NotSet) { return true; }
            if (__instance.ammoBoxes.Count <= 0) { return true; }
            if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.AmmoIdStatName) == false) { return true; }
            string CurrentAmmoId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
            //CustomCategoriesLog.LogWrite("get modified DamagePerShot\n");
            //CustomCategoriesLog.LogWrite(" ammo id "+ammoDefId+"\n");
            ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
            //CustomCategoriesLog.LogWrite(" extAmmoDef.DamagePerShot " + extAmmoDef.DamagePerShot + "\n");
            __result = __instance.StatCollection.GetValue<float>("DamagePerShot") + extAmmoDef.DamagePerShot;
            //CustomCategoriesLog.LogWrite(" result " + __result + "\n");
            return false;
        }
    }
    [HarmonyPatch(typeof(Weapon))]
    [HarmonyPatch("HeatDamagePerShot")]
    [HarmonyPatch(MethodType.Getter)]
    [HarmonyPatch(new Type[] { })]
    public static class Weapon_HeatDamagePerShot
    {
        public static bool Prefix(Weapon __instance, ref float __result)
        {
            if (__instance.AmmoCategory == AmmoCategory.NotSet) { return true; }
            if (__instance.ammoBoxes.Count <= 0) { return true; }
            if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.AmmoIdStatName) == false) { return true; }
            string CurrentAmmoId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
            //CustomCategoriesLog.LogWrite("get modified DamagePerShot\n");
            //CustomCategoriesLog.LogWrite(" ammo id "+ammoDefId+"\n");
            ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
            //CustomCategoriesLog.LogWrite("get modified DamagePerShot\n");
            //CustomCategoriesLog.LogWrite(" ammo id "+ammoDefId+"\n");
            //CustomCategoriesLog.LogWrite(" extAmmoDef.HeatDamagePerShot " + extAmmoDef.HeatDamagePerShot + "\n");
            __result = __instance.StatCollection.GetValue<float>("HeatDamagePerShot") + extAmmoDef.HeatDamagePerShot;
            //CustomCategoriesLog.LogWrite(" result " + __result + "\n");
            return false;
        }
    }
    [HarmonyPatch(typeof(Weapon))]
    [HarmonyPatch("ShotsWhenFired")]
    [HarmonyPatch(MethodType.Getter)]
    [HarmonyPatch(new Type[] { })]
    public static class Weapon_ShotsWhenFired
    {
        public static bool Prefix(Weapon __instance, ref int __result)
        {
            if (__instance.AmmoCategory == AmmoCategory.NotSet) { return true; }
            if (__instance.ammoBoxes.Count <= 0) { return true; }
            if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.AmmoIdStatName) == false) { return true; }
            string CurrentAmmoId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
            //CustomCategoriesLog.LogWrite("get modified DamagePerShot\n");
            //CustomCategoriesLog.LogWrite(" ammo id "+ammoDefId+"\n");
            ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
            //CustomCategoriesLog.LogWrite("get modified DamagePerShot\n");
            //CustomCategoriesLog.LogWrite(" ammo id "+ammoDefId+"\n");
            //CustomCategoriesLog.LogWrite(" extAmmoDef.HeatDamagePerShot " + extAmmoDef.HeatDamagePerShot + "\n");
            __result = __instance.StatCollection.GetValue<int>("ShotsWhenFired") + extAmmoDef.ShotsWhenFired;
            //CustomCategoriesLog.LogWrite(" result " + __result + "\n");
            return false;
        }
    }
    [HarmonyPatch(typeof(Weapon))]
    [HarmonyPatch("ProjectilesPerShot")]
    [HarmonyPatch(MethodType.Getter)]
    [HarmonyPatch(new Type[] { })]
    public static class Weapon_ProjectilesPerShot
    {
        public static bool Prefix(Weapon __instance, ref int __result)
        {
            if (__instance.AmmoCategory == AmmoCategory.NotSet) { return true; }
            if (__instance.ammoBoxes.Count <= 0) { return true; }
            if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.AmmoIdStatName) == false) { return true; }
            string CurrentAmmoId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
            //CustomCategoriesLog.LogWrite("get modified DamagePerShot\n");
            //CustomCategoriesLog.LogWrite(" ammo id "+ammoDefId+"\n");
            ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
            //CustomCategoriesLog.LogWrite(" extAmmoDef.HeatDamagePerShot " + extAmmoDef.HeatDamagePerShot + "\n");
            __result = __instance.StatCollection.GetValue<int>("ProjectilesPerShot") + extAmmoDef.ProjectilesPerShot;
            //CustomCategoriesLog.LogWrite(" result " + __result + "\n");
            return false;
        }
    }
    [HarmonyPatch(typeof(Weapon))]
    [HarmonyPatch("CriticalChanceMultiplier")]
    [HarmonyPatch(MethodType.Getter)]
    [HarmonyPatch(new Type[] { })]
    public static class Weapon_CriticalChanceMultiplier
    {
        public static bool Prefix(Weapon __instance, ref float __result)
        {
            if (__instance.AmmoCategory == AmmoCategory.NotSet) { return true; }
            if (__instance.ammoBoxes.Count <= 0) { return true; }
            if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.AmmoIdStatName) == false) { return true; }
            string CurrentAmmoId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
            //CustomCategoriesLog.LogWrite("get modified DamagePerShot\n");
            //CustomCategoriesLog.LogWrite(" ammo id "+ammoDefId+"\n");
            ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
            __result = __instance.StatCollection.GetValue<float>("CriticalChanceMultiplier") + extAmmoDef.CriticalChanceMultiplier;
            return false;
        }
    }
    [HarmonyPatch(typeof(Weapon))]
    [HarmonyPatch("AccuracyModifier")]
    [HarmonyPatch(MethodType.Getter)]
    [HarmonyPatch(new Type[] { })]
    public static class Weapon_AccuracyModifier
    {
        public static bool Prefix(Weapon __instance, ref float __result)
        {
            //CustomAmmoCategoriesLog.Log.LogWrite("get AccuracyModifier\n");
            if (__instance.AmmoCategory == AmmoCategory.NotSet) { return true; }
            if (__instance.ammoBoxes.Count <= 0) { return true; }
            if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.AmmoIdStatName) == false) { return true; }
            string CurrentAmmoId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
            //CustomCategoriesLog.LogWrite("get modified DamagePerShot\n");
            //CustomCategoriesLog.LogWrite(" ammo id "+ammoDefId+"\n");
            ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
            __result = __instance.StatCollection.GetValue<float>("AccuracyModifier") + extAmmoDef.AccuracyModifier;
            return false;
        }
    }
    [HarmonyPatch(typeof(Weapon))]
    [HarmonyPatch("MinRange")]
    [HarmonyPatch(MethodType.Getter)]
    [HarmonyPatch(new Type[] { })]
    public static class Weapon_MinRange
    {
        public static bool Prefix(Weapon __instance, ref float __result)
        {
            //CustomAmmoCategoriesLog.Log.LogWrite("get MinRange\n");
            if (__instance.AmmoCategory == AmmoCategory.NotSet) { return true; }
            if (__instance.ammoBoxes.Count <= 0) { return true; }
            if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.AmmoIdStatName) == false) { return true; }
            string CurrentAmmoId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
            //CustomCategoriesLog.LogWrite("get modified DamagePerShot\n");
            //CustomCategoriesLog.LogWrite(" ammo id "+ammoDefId+"\n");
            ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
            __result = __instance.StatCollection.GetValue<float>("MinRange") + extAmmoDef.MinRange;
            return false;
        }
    }
    [HarmonyPatch(typeof(Weapon))]
    [HarmonyPatch("MaxRange")]
    [HarmonyPatch(MethodType.Getter)]
    [HarmonyPatch(new Type[] { })]
    public static class Weapon_MaxRange
    {
        public static bool Prefix(Weapon __instance, ref float __result)
        {
            //CustomAmmoCategoriesLog.Log.LogWrite("get MaxRange\n");
            if (__instance.AmmoCategory == AmmoCategory.NotSet) { return true; }
            if (__instance.ammoBoxes.Count <= 0) { return true; }
            if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.AmmoIdStatName) == false) { return true; }
            string CurrentAmmoId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
            //CustomCategoriesLog.LogWrite("get modified DamagePerShot\n");
            //CustomCategoriesLog.LogWrite(" ammo id "+ammoDefId+"\n");
            ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
            __result = __instance.StatCollection.GetValue<float>("MaxRange") + extAmmoDef.MaxRange;
            return false;
        }
    }
    [HarmonyPatch(typeof(Weapon))]
    [HarmonyPatch("ShortRange")]
    [HarmonyPatch(MethodType.Getter)]
    [HarmonyPatch(new Type[] { })]
    public static class Weapon_ShortRange
    {
        public static bool Prefix(Weapon __instance, ref float __result)
        {
            //CustomAmmoCategoriesLog.Log.LogWrite("get ShortRange\n");
            if (__instance.AmmoCategory == AmmoCategory.NotSet) { return true; }
            if (__instance.ammoBoxes.Count <= 0) { return true; }
            if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.AmmoIdStatName) == false) { return true; }
            string CurrentAmmoId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
            //CustomCategoriesLog.LogWrite("get modified DamagePerShot\n");
            //CustomCategoriesLog.LogWrite(" ammo id "+ammoDefId+"\n");
            ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
            __result = __instance.StatCollection.GetValue<float>("ShortRange") + extAmmoDef.ShortRange;
            return false;
        }
    }
    [HarmonyPatch(typeof(Weapon))]
    [HarmonyPatch("MediumRange")]
    [HarmonyPatch(MethodType.Getter)]
    [HarmonyPatch(new Type[] { })]
    public static class Weapon_MediumRange
    {
        public static bool Prefix(Weapon __instance, ref float __result)
        {
            //CustomAmmoCategoriesLog.Log.LogWrite("get MediumRange\n");
            if (__instance.AmmoCategory == AmmoCategory.NotSet) { return true; }
            if (__instance.ammoBoxes.Count <= 0) { return true; }
            if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.AmmoIdStatName) == false) { return true; }
            string CurrentAmmoId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
            //CustomCategoriesLog.LogWrite("get modified DamagePerShot\n");
            //CustomCategoriesLog.LogWrite(" ammo id "+ammoDefId+"\n");
            ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
            __result = __instance.StatCollection.GetValue<float>("MediumRange") + extAmmoDef.MediumRange;
            return false;
        }
    }
    [HarmonyPatch(typeof(Weapon))]
    [HarmonyPatch("LongRange")]
    [HarmonyPatch(MethodType.Getter)]
    [HarmonyPatch(new Type[] { })]
    public static class Weapon_LongRange
    {
        public static bool Prefix(Weapon __instance, ref float __result)
        {
            //CustomAmmoCategoriesLog.Log.LogWrite("get LongRange\n");
            if (__instance.AmmoCategory == AmmoCategory.NotSet) { return true; }
            if (__instance.ammoBoxes.Count <= 0) { return true; }
            if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.AmmoIdStatName) == false) { return true; }
            string CurrentAmmoId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
            //CustomCategoriesLog.LogWrite("get modified DamagePerShot\n");
            //CustomCategoriesLog.LogWrite(" ammo id "+ammoDefId+"\n");
            ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
            __result = __instance.StatCollection.GetValue<float>("LongRange") + extAmmoDef.LongRange;
            return false;
        }
    }
    [HarmonyPatch(typeof(Weapon))]
    [HarmonyPatch("HeatGenerated")]
    [HarmonyPatch(MethodType.Getter)]
    [HarmonyPatch(new Type[] { })]
    public static class Weapon_HeatGenerated
    {
        public static bool Prefix(Weapon __instance, ref float __result)
        {
            //CustomAmmoCategoriesLog.Log.LogWrite("get HeatGenerated\n");
            if (__instance.AmmoCategory == AmmoCategory.NotSet) { return true; }
            if (__instance.ammoBoxes.Count <= 0) { return true; }
            if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.AmmoIdStatName) == false) { return true; }
            string CurrentAmmoId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
            //CustomCategoriesLog.LogWrite("get modified DamagePerShot\n");
            //CustomCategoriesLog.LogWrite(" ammo id "+ammoDefId+"\n");
            //ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
            CombatGameState combat = (CombatGameState)typeof(MechComponent).GetField("combat", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            AmmunitionDef ammoDef = combat.DataManager.AmmoDefs.Get(CurrentAmmoId);
            __result = (float)((double)(__instance.StatCollection.GetValue<float>("HeatGenerated") + ammoDef.HeatGenerated) * (double)combat.Constants.Heat.GlobalHeatIncreaseMultiplier * (__instance.parent != null ? (double)__instance.parent.StatCollection.GetValue<float>("WeaponHeatMultiplier") : 1.0));
            return false;
        }
    }
    [HarmonyPatch(typeof(Weapon))]
    [HarmonyPatch("RefireModifier")]
    [HarmonyPatch(MethodType.Getter)]
    [HarmonyPatch(new Type[] { })]
    public static class Weapon_RefireModifier
    {
        public static bool Prefix(Weapon __instance, ref int __result)
        {
            //CustomAmmoCategoriesLog.Log.LogWrite("get RefireModifier\n");
            if (__instance.AmmoCategory == AmmoCategory.NotSet) { return true; }
            if (__instance.ammoBoxes.Count <= 0) { return true; }
            if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.AmmoIdStatName) == false) { return true; }
            string CurrentAmmoId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
            //CustomCategoriesLog.LogWrite("get modified DamagePerShot\n");
            //CustomCategoriesLog.LogWrite(" ammo id "+ammoDefId+"\n");
            ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
            __result = __instance.StatCollection.GetValue<int>("RefireModifier") + extAmmoDef.RefireModifier;
            return false;
        }
    }
    [HarmonyPatch(typeof(Weapon))]
    [HarmonyPatch("IndirectFireCapable")]
    [HarmonyPatch(MethodType.Getter)]
    [HarmonyPatch(new Type[] { })]
    public static class Weapon_IndirectFireCapable
    {
        public static bool Prefix(Weapon __instance, ref bool __result)
        {
            if (__instance.AmmoCategory == AmmoCategory.NotSet) { return true; }
            if (__instance.ammoBoxes.Count <= 0) { return true; }
            __result = CustomAmmoCategories.getIndirectFireCapable(__instance);
            return false;
        }
    }
    [HarmonyPatch(typeof(Weapon))]
    [HarmonyPatch("AOECapable")]
    [HarmonyPatch(MethodType.Getter)]
    [HarmonyPatch(new Type[] { })]
    public static class Weapon_AOECapable
    {
        public static bool Prefix(Weapon __instance, ref bool __result)
        {
            if (__instance.AmmoCategory == AmmoCategory.NotSet) { return true; }
            if (__instance.ammoBoxes.Count <= 0) { return true; }
            if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.AmmoIdStatName) == false) { return true; }
            string CurrentAmmoId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
            //CustomCategoriesLog.LogWrite("get modified DamagePerShot\n");
            //CustomCategoriesLog.LogWrite(" ammo id "+ammoDefId+"\n");
            ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
            if (extAmmoDef.AOECapable < 0)
            {
                __result = __instance.weaponDef.AOECapable;
            }
            else
            {
                __result = extAmmoDef.AOECapable != 0;
            }
            return false;
        }
    }
    [HarmonyPatch(typeof(Weapon))]
    [HarmonyPatch("Instability")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { })]
    public static class Weapon_Instability
    {
        public static bool Prefix(Weapon __instance, ref float __result)
        {
            if (__instance.AmmoCategory == AmmoCategory.NotSet) { return true; }
            if (__instance.ammoBoxes.Count <= 0) { return true; }
            if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.AmmoIdStatName) == false) { return true; }
            string CurrentAmmoId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
            //CustomCategoriesLog.LogWrite("get modified DamagePerShot\n");
            //CustomCategoriesLog.LogWrite(" ammo id "+ammoDefId+"\n");
            ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
            __result = __instance.StatCollection.GetValue<float>("Instability") + extAmmoDef.Instability;
            return false;
        }
    }
    [HarmonyPatch(typeof(Weapon))]
    [HarmonyPatch("WillFire")]
    [HarmonyPatch(MethodType.Getter)]
    [HarmonyPatch(new Type[] { })]
    public static class Weapon_WillFire
    {
        public static bool Prefix(Weapon __instance)
        {
            return true;
        }
    }
    [HarmonyPatch(typeof(AttackDirector.AttackSequence))]
    [HarmonyPatch("Cleanup")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] {})]
    public static class AttackSequence_Cleanup
    {
        public static void Postfix(AttackDirector.AttackSequence __instance)
        {
            CustomAmmoCategoriesLog.Log.LogWrite("AttackDirector.AttackSequence.Cleanup\n");
            List<List<Weapon>> sortedWeapons = ((List<List<Weapon>>)typeof(AttackDirector.AttackSequence).GetField("sortedWeapons", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance));
            foreach(List<Weapon> weapons in sortedWeapons) {
                foreach(Weapon weapon in weapons)
                {
                    CustomAmmoCategoriesLog.Log.LogWrite("  weapon " + weapon.Name + "\n");
                    if (weapon.AmmoCategory == AmmoCategory.NotSet) { continue; }
                    if (weapon.ammoBoxes.Count <= 0) { continue; }
                    if (weapon.CurrentAmmo > 0) { continue; }
                    CustomAmmoCategories.CycleAmmoBest(weapon);
                }
            }
            //string wGUID = weapon.parent.GUID + "." + weapon.uid;
            //CustomAmmoCategoriesLog.Log.LogWrite("  wGUID " + wGUID + "\n");
            /*string aGUID = "";
            if (CustomAmmoCategories.findPlayerWeaponAmmoGUID(wGUID, ref aGUID) == false) { return; }
            if ((aGUID < 0) || (aGUID >= weapon.ammoBoxes.Count)) { return; }
            CustomAmmoCategoriesLog.Log.LogWrite("  player weapon found. Ammo " + weapon.ammoBoxes[aGUID].CurrentAmmo + "\n");
            if ((weapon.ammoBoxes[aGUID].CurrentAmmo > 0) && (weapon.ammoBoxes[aGUID].IsFunctional == true)) {
                CustomAmmoCategoriesLog.Log.LogWrite("  Cycling not needed\n");
                return;
            }
            CustomAmmoCategories.CycleAmmo(weapon,true);*/
        }
    }
    [HarmonyPatch(typeof(CombatHUDWeaponSlot))]
    [HarmonyPatch("RefreshDisplayedWeapon")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(ICombatant) })]
    public static class CombatHUDWeaponSlot_RefreshDisplayedWeapon
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "ShotsWhenFired").GetGetMethod();
            var replacementMethod = AccessTools.Method(typeof(CombatHUDWeaponSlot_RefreshDisplayedWeapon),
                nameof(ShotsWhenFiredDisplayOverider));
            return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
        }

        private static int ShotsWhenFiredDisplayOverider(Weapon weapon)
        {
            return weapon.ShotsWhenFired * weapon.ProjectilesPerShot;
        }
    }

    [HarmonyPatch(typeof(CombatGameState))]
    [HarmonyPatch("RebuildAllLists")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { })]
    public static class CombatGameState_RebuildAllLists
    {
        public static void Postfix(CombatGameState __instance)
        {
            CustomAmmoCategoriesLog.Log.LogWrite("CombatGameState.RebuildAllLists\n");
            //CustomAmmoCategories.ClearPlayerWeapons();
            foreach (var unit in __instance.AllActors)
            {
                //if (unit is Mech)
                //{
                    CustomAmmoCategoriesLog.Log.LogWrite("  " + unit.DisplayName + "\n");
                    foreach (var Weapon in unit.Weapons)
                    {
                        CustomAmmoCategories.RegisterPlayerWeapon(Weapon);
                    }
                //}
            }
        }
    }
    [HarmonyPatch(typeof(CombatGameState))]
    [HarmonyPatch("OnCombatGameDestroyed")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { })]
    public static class CombatGameState_OnCombatGameDestroyed
    {
        public static void Postfix(CombatGameState __instance)
        {
            CustomAmmoCategoriesLog.Log.LogWrite("CombatGameState.OnCombatGameDestroyed\n");
            //CustomAmmoCategories.clearAllWeaponEffects();
        }
    }
    [HarmonyPatch(typeof(CombatHUD))]
    [HarmonyPatch("Init")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(CombatGameState) })]
    public static class CombatHUD_Init
    {
        public static bool Prefix(CombatHUD __instance,CombatGameState Combat)
        {
            CustomAmmoCategoriesLog.Log.LogWrite("pre CombatHUD.Init\n");
            //CustomAmmoCategories.ClearPlayerWeapons();
            foreach (var unit in Combat.AllActors)
            {
                CustomAmmoCategoriesLog.Log.LogWrite("  " + unit.DisplayName + "\n");
                foreach (var Weapon in unit.Weapons)
                {
                    CustomAmmoCategories.RegisterPlayerWeapon(Weapon);
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Weapon))]
    [HarmonyPatch("DecrementAmmo")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(int) })]
    public static class Weapon_DecrementAmmo
    {
        public static bool Prefix(Weapon __instance, int stackItemUID, ref int __result)
        {
            __result = CustomAmmoCategories.DecrementAmmo(__instance,stackItemUID,0);
            return false;
        }
    }

    [HarmonyPatch(typeof(AmmunitionDef))]
    [HarmonyPatch("FromJSON")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(string) })]
    public static class BattleTech_AmmunitionDef_fromJSON_Patch
    {
        public static bool Prefix(AmmunitionDef __instance, ref string json)
        {
            CustomAmmoCategoriesLog.Log.LogWrite("AmmunitionDef fromJSON\n");
            JObject defTemp = JObject.Parse(json);
            CustomAmmoCategory custCat = CustomAmmoCategories.find((string)defTemp["Category"]);
            CustomAmmoCategories.RegisterAmmunition((string)defTemp["Description"]["Id"], custCat);
            defTemp["Category"] = custCat.BaseCategory.ToString();
            ExtAmmunitionDef extAmmoDef = new ExtAmmunitionDef();
            if (defTemp["AccuracyModifier"] != null)
            {
                extAmmoDef.AccuracyModifier = (float)defTemp["AccuracyModifier"];
                defTemp.Remove("AccuracyModifier");
            }
            if (defTemp["DamagePerShot"] != null)
            {
                extAmmoDef.DamagePerShot = (float)defTemp["DamagePerShot"];
                defTemp.Remove("DamagePerShot");
            }
            if (defTemp["HeatDamagePerShot"] != null)
            {
                extAmmoDef.HeatDamagePerShot = (float)defTemp["HeatDamagePerShot"];
                defTemp.Remove("HeatDamagePerShot");
            }
            if (defTemp["ProjectilesPerShot"] != null)
            {
                extAmmoDef.ProjectilesPerShot = (int)defTemp["ProjectilesPerShot"];
                defTemp.Remove("ProjectilesPerShot");
            }
            if (defTemp["ShotsWhenFired"] != null)
            {
                extAmmoDef.ShotsWhenFired = (int)defTemp["ShotsWhenFired"];
                defTemp.Remove("ShotsWhenFired");
            }
            if (defTemp["CriticalChanceMultiplier"] != null)
            {
                extAmmoDef.CriticalChanceMultiplier = (float)defTemp["CriticalChanceMultiplier"];
                defTemp.Remove("CriticalChanceMultiplier");
            }
            if (defTemp["AIBattleValue"] != null)
            {
                extAmmoDef.AIBattleValue = (int)defTemp["AIBattleValue"];
                defTemp.Remove("AIBattleValue");
            }
            if (defTemp["MinRange"] != null)
            {
                extAmmoDef.MinRange = (float)defTemp["MinRange"];
                defTemp.Remove("MinRange");
            }
            if (defTemp["MaxRange"] != null)
            {
                extAmmoDef.MaxRange = (float)defTemp["MaxRange"];
                defTemp.Remove("MaxRange");
            }
            if (defTemp["ShortRange"] != null)
            {
                extAmmoDef.ShortRange = (float)defTemp["ShortRange"];
                defTemp.Remove("ShortRange");
            }
            if (defTemp["MediumRange"] != null)
            {
                extAmmoDef.MediumRange = (float)defTemp["MediumRange"];
                defTemp.Remove("MediumRange");
            }
            if (defTemp["LongRange"] != null)
            {
                extAmmoDef.LongRange = (float)defTemp["LongRange"];
                defTemp.Remove("LongRange");
            }
            if (defTemp["RefireModifier"] != null)
            {
                extAmmoDef.RefireModifier = (int)defTemp["RefireModifier"];
                defTemp.Remove("RefireModifier");
            }
            if (defTemp["Instability"] != null)
            {
                extAmmoDef.Instability = (float)defTemp["Instability"];
                defTemp.Remove("Instability");
            }
            if (defTemp["AttackRecoil"] != null)
            {
                extAmmoDef.AttackRecoil = (int)defTemp["AttackRecoil"];
                defTemp.Remove("AttackRecoil");
            }
            if (defTemp["WeaponEffectID"] != null)
            {
                extAmmoDef.WeaponEffectID = (string)defTemp["WeaponEffectID"];
                defTemp.Remove("WeaponEffectID");
            }
            if (defTemp["EvasivePipsIgnored"] != null)
            {
                extAmmoDef.EvasivePipsIgnored = (float)defTemp["EvasivePipsIgnored"];
                defTemp.Remove("EvasivePipsIgnored");
            }
            if (defTemp["IndirectFireCapable"] != null)
            {
                extAmmoDef.IndirectFireCapable = ((bool)defTemp["IndirectFireCapable"] == true)?1:0;
                defTemp.Remove("IndirectFireCapable");
            }
            if (defTemp["DirectFireModifier"] != null)
            {
                extAmmoDef.DirectFireModifier = (float)defTemp["DirectFireModifier"];
                defTemp.Remove("DirectFireModifier");
            }
            if (defTemp["AOECapable"] != null)
            {
                extAmmoDef.AOECapable = ((bool)defTemp["IndirectFireCapable"] == true) ? 1 : 0;
                defTemp.Remove("AOECapable");
            }
            if (defTemp["HitGenerator"] != null)
            {
                try
                {
                    extAmmoDef.HitGenerator = (HitGeneratorType)Enum.Parse(typeof(HitGeneratorType), (string)defTemp["HitGenerator"], true);
                }
                catch (Exception e)
                {
                    extAmmoDef.HitGenerator = HitGeneratorType.NotSet;
                }
                defTemp.Remove("HitGenerator");
            }
            if (defTemp["statusEffects"] != null)
            {

                if (defTemp["statusEffects"].Type == JTokenType.Array)
                {
                    List<EffectData> tmpList = new List<EffectData>();
                    JToken statusEffects = defTemp["statusEffects"];
                    foreach (JObject statusEffect in statusEffects)
                    {
                        EffectData effect = new EffectData();
                        JSONSerializationUtility.FromJSON<EffectData>(effect, statusEffect.ToString());
                        tmpList.Add(effect);
                    }
                    extAmmoDef.statusEffects = tmpList.ToArray();
                }
                //extAmmoDef.statusEffects = JsonConvert.DeserializeObject<EffectData[]>(defTemp["statusEffects"].ToString());
                //JSONSerializationUtility.FromJSON<EffectData[]>(extAmmoDef.statusEffects, defTemp["statusEffects"].ToString());
                //CustomAmmoCategoriesLog.Log.LogWrite(JsonConvert.SerializeObject(extAmmoDef.statusEffects)+"\n");
                defTemp.Remove("statusEffects");
            }
            CustomAmmoCategories.RegisterExtAmmoDef((string)defTemp["Description"]["Id"], extAmmoDef);
            json = defTemp.ToString();
            return true;
        }
        public static void Postfix(AmmunitionDef __instance)
        {
            EffectData[] effects = CustomAmmoCategories.findExtAmmo(__instance.Description.Id).statusEffects;
            List<EffectData> tmpList = new List<EffectData>();
            CustomAmmoCategoriesLog.Log.LogWrite("Checking on null status effects " + __instance.Description.Id + " "+effects.Length+".\n");
            foreach (EffectData effect in effects)
            {
                if((effect.Description != null))
                {
                    if((effect.Description.Id != null)&&(effect.Description.Name != null))
                    {
                        tmpList.Add(effect);
                        continue;
                    }else
                    {
                        if (effect.Description.Id == null)
                        {
                            CustomAmmoCategoriesLog.Log.LogWrite("!Warning! effect id is null " + __instance.Description.Id + ".\n");
                        }
                        if (effect.Description.Name == null)
                        {
                            CustomAmmoCategoriesLog.Log.LogWrite("!Warning! effect name is null " + __instance.Description.Id + ".\n");
                        }
                    }
                }
                else
                {
                    CustomAmmoCategoriesLog.Log.LogWrite("!Warning! effect description is null " + __instance.Description.Id + ".\n");
                }
                CustomAmmoCategoriesLog.Log.LogWrite("!Warning! null status effect detected at ammo "+ __instance .Description.Id+ ".\n");
            }
            if(tmpList.Count != effects.Length)
            {
                CustomAmmoCategoriesLog.Log.LogWrite("!Warning! null ("+(effects.Length - tmpList.Count)+"/"+effects.Length+") status effects detected at ammo " + __instance.Description.Id + ".Removing\n");
                CustomAmmoCategories.findExtAmmo(__instance.Description.Id).statusEffects = tmpList.ToArray();
            }
        }
    }
    
    [HarmonyPatch(typeof(WeaponDef))]
    [HarmonyPatch("FromJSON")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(string) })]
    public static class BattleTech_WeaponDef_fromJSON_Patch
    {
        public static bool Prefix(WeaponDef __instance, ref string json)
        {
            CustomAmmoCategoriesLog.Log.LogWrite("WeaponDef fromJSON\n");
            JObject defTemp = JObject.Parse(json);
            CustomAmmoCategory custCat = CustomAmmoCategories.find((string)defTemp["AmmoCategory"]);
            CustomAmmoCategories.RegisterWeapon((string)defTemp["Description"]["Id"], custCat);
            ExtWeaponDef extDef = new ExtWeaponDef();
            if (defTemp["Streak"] != null)
            {
                extDef.StreakEffect = (bool)defTemp["Streak"];
                defTemp.Remove("Streak");
            }
            if (defTemp["HitGenerator"] != null)
            {
                try
                {
                    extDef.HitGenerator = (HitGeneratorType)Enum.Parse(typeof(HitGeneratorType), (string)defTemp["HitGenerator"], true);
                }
                catch (Exception e)
                {
                    extDef.HitGenerator = HitGeneratorType.NotSet;
                }
                defTemp.Remove("HitGenerator");
            }
            if (defTemp["DirectFireModifier"] != null)
            {
                extDef.DirectFireModifier = (float)defTemp["DirectFireModifier"];
                defTemp.Remove("DirectFireModifier");
            }
            CustomAmmoCategories.registerExtWeaponDef((string)defTemp["Description"]["Id"], extDef);
            defTemp["AmmoCategory"] = custCat.BaseCategory.ToString();
            //CustomAmmoCategoriesLog.Log.LogWrite("\n--------------ORIG----------------\n" + json + "\n----------------------------------\n");
            //CustomAmmoCategoriesLog.Log.LogWrite("\n--------------MOD----------------\n" + defTemp.ToString() + "\n----------------------------------\n");
            json = defTemp.ToString();
            return true;
        }
        public static void Postfix(WeaponDef __instance)
        {
            EffectData[] effects = __instance.statusEffects;
            List<EffectData> tmpList = new List<EffectData>();
            foreach (EffectData effect in effects)
            {
                if ((effect.Description != null))
                {
                    if ((effect.Description.Id != null) && (effect.Description.Name != null))
                    {
                        tmpList.Add(effect);
                        continue;
                    }
                }
                CustomAmmoCategoriesLog.Log.LogWrite("!Warning! null status effect detected at weapon " + __instance.Description.Id + ".\n");
            }
            if (tmpList.Count != effects.Length)
            {
                CustomAmmoCategoriesLog.Log.LogWrite("!Warning! null status effects detected at weapon " + __instance.Description.Id + ".Removing\n");
                PropertyInfo property = typeof(WeaponDef).GetProperty("statusEffects");
                property.DeclaringType.GetProperty("statusEffects");
                property.GetSetMethod(true).Invoke(__instance, new object[1] { (object)tmpList.ToArray() });
            }
        }
    }

    [HarmonyPatch(typeof(AbstractActor))]
    [HarmonyPatch("CalcAndSetAlphaStrikesRemaining")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { })]
    public static class AbstractActor_CalcAlphaStrikesRem_Patch
    {
        public static bool Prefix(AbstractActor __instance)
        {
            if (__instance.ammoBoxes.Count < 1)
                return false;
            Dictionary<int, int> dictionary1 = new Dictionary<int, int>();
            for (int index1 = 0; index1 < __instance.Weapons.Count; ++index1)
            {
                int ammoCategory = CustomAmmoCategories.findWeaponRealCategory(__instance.Weapons[index1].Description.Id, __instance.Weapons[index1].AmmoCategory.ToString()).Index;
                //ammoCategory ammoCategory = this.Weapons[index1].AmmoCategory;
                if (ammoCategory != 0)
                {
                    if (dictionary1.ContainsKey(ammoCategory))
                    {
                        Dictionary<int, int> dictionary2;
                        int index2;
                        (dictionary2 = dictionary1)[index2 = ammoCategory] = dictionary2[index2] + __instance.Weapons[index1].ShotsWhenFired;
                    }
                    else
                        dictionary1[ammoCategory] = __instance.Weapons[index1].ShotsWhenFired;
                }
            }
            Dictionary<int, int> dictionary3 = new Dictionary<int, int>();
            for (int index1 = 0; index1 < __instance.ammoBoxes.Count; ++index1)
            {
                int ammoCategory = CustomAmmoCategories.findAmunitionRealCategory(__instance.ammoBoxes[index1].ammoDef.Description.Id, __instance.ammoBoxes[index1].ammoCategory.ToString()).Index;
                if (dictionary3.ContainsKey(ammoCategory))
                {
                    Dictionary<int, int> dictionary2;
                    int index2;
                    (dictionary2 = dictionary3)[index2 = ammoCategory] = dictionary2[index2] + __instance.ammoBoxes[index1].CurrentAmmo;
                }
                else
                    dictionary3[ammoCategory] = __instance.ammoBoxes[index1].CurrentAmmo;
            }
            Dictionary<int, float> dictionary4 = new Dictionary<int, float>();
            foreach (KeyValuePair<int, int> keyValuePair in dictionary1)
            {
                int key = keyValuePair.Key;
                dictionary4[key] = dictionary3.ContainsKey(key) ? (float)dictionary3[key] / (float)dictionary1[key] : 0.0f;
            }
            for (int index = 0; index < __instance.Weapons.Count; ++index)
            {
                int ammoCategory = CustomAmmoCategories.findWeaponRealCategory(__instance.Weapons[index].Description.Id, __instance.Weapons[index].AmmoCategory.ToString()).Index;
                if (dictionary4.ContainsKey(ammoCategory))
                    __instance.Weapons[index].AlphaStrikesRemaining = dictionary4[ammoCategory] + (float)__instance.Weapons[index].InternalAmmo;
            }
            return false;
        }
    }
    [HarmonyPatch(typeof(Weapon))]
    [HarmonyPatch("SetAmmoBoxes")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(List<AmmunitionBox>) })]
    public static class Weapon_SetAmmoBoxes
    {
        public static bool Prefix(Weapon __instance, List<AmmunitionBox> ammoBoxes)
        {
            CustomAmmoCategoriesLog.Log.LogWrite("Weapon SetAmmoBoxes " + __instance.Description.Id + "\n");
            int weaponAmmoCategory = CustomAmmoCategories.findWeaponRealCategory(__instance.Description.Id, __instance.AmmoCategory.ToString()).Index;
            List<AmmunitionBox> ammunitionBoxList = new List<AmmunitionBox>();
            CustomAmmoCategoriesLog.Log.LogWrite("  Weapon Ammo Catgory " + weaponAmmoCategory.ToString() + "\n");
            foreach (AmmunitionBox ammoBox in ammoBoxes)
            {
                int ammoBoxAmmoCategory = CustomAmmoCategories.findAmunitionRealCategory(ammoBox.ammoDef.Description.Id, ammoBox.ammoCategory.ToString()).Index;
                CustomAmmoCategoriesLog.Log.LogWrite("  Ammunition Box " + ammoBox.ammoDef.Description.Id + " - " + ammoBoxAmmoCategory + "\n");

                if (ammoBoxAmmoCategory == weaponAmmoCategory)
                    ammunitionBoxList.Add(ammoBox);
            }
            __instance.ammoBoxes = ammunitionBoxList;
            return false;
        }
    }
    [HarmonyPatch(typeof(Weapon))]
    [HarmonyPatch("CurrentAmmo")]
    [HarmonyPatch(MethodType.Getter)]
    [HarmonyPatch(new Type[] { })]
    public static class Weapon_CurrentAmmo
    {
        public static bool Prefix(Weapon __instance, ref int __result)
        {
            //CustomCategoriesLog.LogWrite("Weapon CurrentAmmo " + __instance.Description.Id + "\n");
            if (CustomAmmoCategories.checkExistance(__instance.StatCollection,CustomAmmoCategories.AmmoIdStatName) == false) { return true; }
            string CurrentAmmoId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
            __result = __instance.InternalAmmo;
            //CustomCategoriesLog.LogWrite("  internal ammo "+ __result.ToString());
            for(int index = 0; index < __instance.ammoBoxes.Count;++index)
            {
                //CustomCategoriesLog.LogWrite("  AmmoBox " + __instance.ammoBoxes[index].Description.Id+" "+ __instance.ammoBoxes[index].CurrentAmmo+"\n");
                if (__instance.ammoBoxes[index].CurrentAmmo <= 0) { continue; }
                if (__instance.ammoBoxes[index].IsFunctional == false) { continue; }
                if (__instance.ammoBoxes[index].ammoDef.Description.Id != CurrentAmmoId) { continue; };
                __result += __instance.ammoBoxes[index].CurrentAmmo;
            }
            //CustomCategoriesLog.LogWrite("  Result " + __result.ToString()+"\n");
            return false;
        }
    }
    /*
    [HarmonyPatch(typeof(WeaponDef))]
    [HarmonyPatch("AmmoCategoryToAmmoId")]
    [HarmonyPatch(MethodType.Getter)]
    public static class WeaponDef_AmmoCategoryToAmmoId_get
    {
        public static bool Prefix(WeaponDef __instance, ref string __result)
        {
            if (__instance.AmmoCategory == AmmoCategory.NotSet)
            {
                __result = string.Empty;
                return false;
            }
            else
            {
                CustomAmmoCategory cuCat = CustomAmmoCategories.findWeaponRealCategory(__instance.Description.Id, __instance.AmmoCategory.ToString());
                if (cuCat.BaseCategory == AmmoCategory.NotSet)
                {
                    __result = string.Empty;
                    return false;
                }
                else
                {
                    __result = "Ammunition_" + cuCat.Id;
                    return false;
                }
            }
        }
    }
    [HarmonyPatch(typeof(WeaponDef))]
    [HarmonyPatch("AmmoCategoryToAmmoBoxId")]
    [HarmonyPatch(MethodType.Getter)]
    public static class WeaponDef_AmmoCategoryToAmmoBoxId_get
    {
        public static bool Prefix(WeaponDef __instance, ref string __result)
        {
            if (__instance.AmmoCategory == AmmoCategory.NotSet)
            {
                __result = string.Empty;
                return false;
            }
            else
            {
                CustomAmmoCategory cuCat = CustomAmmoCategories.findWeaponRealCategory(__instance.Description.Id, __instance.AmmoCategory.ToString());
                if (cuCat.BaseCategory == AmmoCategory.NotSet)
                {
                    __result = string.Empty;
                    return false;
                }
                else
                {
                    __result = "Ammo_AmmunitionBox_Generic_" + cuCat.Id;
                    return false;
                }
            }
        }
    }*/

    [HarmonyPatch(typeof(MechValidationRules))]
    [HarmonyPatch("ValidateMechHasAppropriateAmmo")]
    [HarmonyPatch(MethodType.Normal)]
    public static class MechValidationRules_ValidateMechHasAppropriateAmmo
    {
        public static bool Prefix(DataManager dataManager, MechDef mechDef, MechValidationLevel validationLevel, WorkOrderEntry_MechLab baseWorkOrder, ref Dictionary<MechValidationType, List<Text>> errorMessages)
        {
            List<int> ammoCategoryList1 = new List<int>();
            List<int> ammoCategoryList2 = new List<int>();
            CustomAmmoCategoriesLog.Log.LogWrite("Start Mech Validation\n");
            for (int index = 0; index < mechDef.Inventory.Length; ++index)
            {
                MechComponentRef mechComponentRef = mechDef.Inventory[index];
                mechComponentRef.RefreshComponentDef();
                if (mechComponentRef.ComponentDefType == ComponentType.Weapon)
                {
                    if (mechComponentRef.DamageLevel == ComponentDamageLevel.Functional || mechComponentRef.DamageLevel == ComponentDamageLevel.NonFunctional || MechValidationRules.MechComponentUnderMaintenance(mechComponentRef, validationLevel, baseWorkOrder))
                    {
                        WeaponDef def = mechComponentRef.Def as WeaponDef;
                        CustomAmmoCategory cuCat = CustomAmmoCategories.findWeaponRealCategory(def.Description.Id, def.AmmoCategory.ToString());
                        int weaponAmmoType = cuCat.Index;
                        CustomAmmoCategoriesLog.Log.LogWrite(" You have weapon " + def.Description.Id + " with ammo category:" + cuCat.Id + " index:" + weaponAmmoType.ToString() + "\n");

                        if (def.AmmoCategory != AmmoCategory.NotSet && def.AmmoCategory != AmmoCategory.Flamer && !ammoCategoryList1.Contains(weaponAmmoType))
                        {
                            ammoCategoryList1.Add(weaponAmmoType);
                            CustomAmmoCategoriesLog.Log.LogWrite("  Add index to requred ammo category: " + weaponAmmoType.ToString() + "\n");
                        }
                    }
                }
                else if (mechComponentRef.ComponentDefType == ComponentType.AmmunitionBox && (mechComponentRef.DamageLevel == ComponentDamageLevel.Functional || MechValidationRules.MechComponentUnderMaintenance(mechComponentRef, validationLevel, baseWorkOrder)))
                {
                    AmmunitionBoxDef def = mechComponentRef.Def as AmmunitionBoxDef;
                    def.refreshAmmo(dataManager);
                    CustomAmmoCategory cuCat = CustomAmmoCategories.findWeaponRealCategory(def.Ammo.Description.Id, def.Ammo.Category.ToString());
                    int amunitionAmmoType = CustomAmmoCategories.findAmunitionRealCategory(def.Ammo.Description.Id, def.Ammo.Category.ToString()).Index;
                    CustomAmmoCategoriesLog.Log.LogWrite("  You have ammunition " + def.Ammo.Description.Id + " with ammo category " + cuCat.Id + " index:" + amunitionAmmoType.ToString() + "\n");
                    if (!ammoCategoryList2.Contains(amunitionAmmoType))
                    {
                        ammoCategoryList2.Add(amunitionAmmoType);
                        CustomAmmoCategoriesLog.Log.LogWrite("  Add index to requred ammo category: " + amunitionAmmoType.ToString() + "\n");
                    }
                }
            }
            CustomAmmoCategoriesLog.Log.LogWrite("Ammunition Box categories indexes:");
            foreach (int ammoCategory in ammoCategoryList1)
            {
                CustomAmmoCategoriesLog.Log.LogWrite(" " + ammoCategory.ToString());
            }
            CustomAmmoCategoriesLog.Log.LogWrite("\nWeapon categories indexes:");
            foreach (int ammoCategory in ammoCategoryList2)
            {
                CustomAmmoCategoriesLog.Log.LogWrite(" " + ammoCategory.ToString());
            }
            CustomAmmoCategoriesLog.Log.LogWrite("\n");
            foreach (int ammoCategory in ammoCategoryList1)
            {
                if (!ammoCategoryList2.Contains(ammoCategory))
                {
                    var method = typeof(MechValidationRules).GetMethod("AddErrorMessage", BindingFlags.Static | BindingFlags.NonPublic);
                    object[] args = new object[3];
                    args[0] = errorMessages;
                    args[1] = MechValidationType.AmmoMissing;
                    CustomAmmoCategory cuCat = CustomAmmoCategories.findByIndex(ammoCategory);
                    args[2] = new Text("MISSING AMMO: This 'Mech does not have an undamaged {0} Ammo Bin ", new object[1] { (object)cuCat.Id });
                    method.Invoke(obj: null, parameters: args);
                    errorMessages = (Dictionary<MechValidationType, List<Text>>)args[0];
                }
            }
            foreach (int ammoCategory in ammoCategoryList2)
            {
                if (!ammoCategoryList1.Contains(ammoCategory))
                {

                    var method = typeof(MechValidationRules).GetMethod("AddErrorMessage", BindingFlags.Static | BindingFlags.NonPublic);
                    object[] args = new object[3];
                    args[0] = errorMessages;
                    args[1] = MechValidationType.AmmoUnneeded;
                    CustomAmmoCategory cuCat = CustomAmmoCategories.findByIndex(ammoCategory);
                    args[2] = new Text("EXTRA AMMO: This 'Mech carries unusable {0} Ammo", new object[1] { (object)cuCat.Id });
                    method.Invoke(obj: null, parameters: args);
                    errorMessages = (Dictionary<MechValidationType, List<Text>>)args[0];
                }
            }
            return false;
        }
    }
    [HarmonyPatch(typeof(WeaponRepresentation))]
    [HarmonyPatch("Init")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(Weapon), typeof(Transform), typeof(bool), typeof(string), typeof(int) })]
    public static class WeaponRepresentation_Init
    {
        public static void Postfix(WeaponRepresentation __instance, Weapon weapon, Transform parentTransform, bool isParented, string parentDisplayName, int mountedLocation)
        {
            string wGUID;
            if(CustomAmmoCategories.checkExistance(weapon.StatCollection,CustomAmmoCategories.GUIDStatisticName) == false)
            {
                wGUID = Guid.NewGuid().ToString();
                weapon.StatCollection.AddStatistic<string>(CustomAmmoCategories.GUIDStatisticName,wGUID);
            }else
            {
                wGUID = weapon.StatCollection.GetStatistic(CustomAmmoCategories.GUIDStatisticName).Value<string>();
            }
            CustomAmmoCategories.ClearWeaponEffects(wGUID);
            CustomAmmoCategories.InitWeaponEffects(__instance,weapon);
        }
    }
    [HarmonyPatch(typeof(WeaponRepresentation))]
    [HarmonyPatch("PlayWeaponEffect")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(WeaponHitInfo) })]
    public static class WeaponRepresentation_PlayWeaponEffect
    {
        public static bool Prefix(WeaponRepresentation __instance, WeaponHitInfo hitInfo)
        {
            CustomAmmoCategoriesLog.Log.LogWrite("WeaponRepresentation.PlayWeaponEffect\n");
            try
            {
                if (__instance.weapon == null) { return true; }
                CustomAmmoCategoriesLog.Log.LogWrite("  weapon is set\n");
                if (CustomAmmoCategories.checkExistance(__instance.weapon.StatCollection, CustomAmmoCategories.GUIDStatisticName) == false) { return true; }
                CustomAmmoCategoriesLog.Log.LogWrite("  weapon GUID is set\n");
                if (CustomAmmoCategories.checkExistance(__instance.weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == false) { return true; }
                CustomAmmoCategoriesLog.Log.LogWrite("  weapon ammoId is set\n");
                string wGUID = __instance.weapon.StatCollection.GetStatistic(CustomAmmoCategories.GUIDStatisticName).Value<string>();
                string ammoId = __instance.weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
                string weaponEffectId = CustomAmmoCategories.findExtAmmo(ammoId).WeaponEffectID;
                WeaponEffect currentEffect = (WeaponEffect)null;
                if (string.IsNullOrEmpty(weaponEffectId)) {
                    currentEffect = __instance.WeaponEffect;
                    weaponEffectId = __instance.weapon.weaponDef.WeaponEffectID;
                }
                if (weaponEffectId == __instance.weapon.weaponDef.WeaponEffectID) {
                    currentEffect = __instance.WeaponEffect;
                    weaponEffectId = __instance.weapon.weaponDef.WeaponEffectID;
                };
                if (currentEffect == (WeaponEffect)null)
                {
                    currentEffect = CustomAmmoCategories.getWeaponEffect(wGUID, weaponEffectId);
                }
                CustomAmmoCategoriesLog.Log.LogWrite("  weapon weaponEffectId is set " + wGUID + " " + __instance.weapon.Name + " " + ammoId + " " + weaponEffectId + "\n");
                if (currentEffect == null) { return true; }
                CustomAmmoCategoriesLog.Log.LogWrite("  weapon weaponEffect is set\n");
                ExtWeaponDef extWeaponDef = CustomAmmoCategories.getExtWeaponDef(__instance.weapon.Description.Id);
                if (extWeaponDef.StreakEffect == true)
                {
                    CustomAmmoCategoriesLog.Log.LogWrite("  streak effect\n");
                    WeaponHitInfo streakHitInfo = CustomAmmoCategories.getSuccessOnly(hitInfo);
                    CustomAmmoCategories.ReturnNoFireHeat(__instance.weapon,hitInfo.stackItemUID,streakHitInfo.numberOfShots);
                    if(streakHitInfo.numberOfShots == 0)
                    {
                        CustomAmmoCategoriesLog.Log.LogWrite("  no success hits\n");
                        currentEffect.currentState = WeaponEffect.WeaponEffectState.Complete;
                        currentEffect.subEffect = false;
                        currentEffect.hitInfo = hitInfo;
                        PropertyInfo property = typeof(WeaponEffect).GetProperty("FiringComplete");
                        property.DeclaringType.GetProperty("FiringComplete");
                        property.GetSetMethod(true).Invoke(currentEffect, new object[1] { (object)false });
                        typeof(WeaponEffect).GetMethod("PublishNextWeaponMessage", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(currentEffect,new object[0]);
                        currentEffect.PublishWeaponCompleteMessage();
                    }
                    else
                    {
                        CustomAmmoCategoriesLog.Log.LogWrite("  streak fire\n");
                        currentEffect.Fire(streakHitInfo, 0, 0);
                        CustomAmmoCategories.DecrementAmmo(__instance.weapon,streakHitInfo.stackItemUID,streakHitInfo.numberOfShots);
                    }
                }
                else
                {
                    CustomAmmoCategoriesLog.Log.LogWrite("  normal fire\n");
                    currentEffect.Fire(hitInfo, 0, 0);
                }
                CustomAmmoCategoriesLog.Log.LogWrite("  fired\n");
                return false;
            }catch(Exception e)
            {
                CustomAmmoCategoriesLog.Log.LogWrite("Exception:"+e.ToString()+"\nfallbak to default\n");
                return true;
            }
        }
    }
    [HarmonyPatch(typeof(WeaponRepresentation))]
    [HarmonyPatch("ResetWeaponEffect")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { })]
    public static class WeaponRepresentation_ResetWeaponEffect
    {
        public static void Postfix(WeaponRepresentation __instance)
        {
            CustomAmmoCategoriesLog.Log.LogWrite("WeaponRepresentation.ResetWeaponEffect\n");
            if (__instance.weapon == null) { return; }
            CustomAmmoCategoriesLog.Log.LogWrite("  weapon is set\n");
            if (CustomAmmoCategories.checkExistance(__instance.weapon.StatCollection, CustomAmmoCategories.GUIDStatisticName) == false) { return; }
            CustomAmmoCategoriesLog.Log.LogWrite("  weapon GUID is set\n");
            if (CustomAmmoCategories.checkExistance(__instance.weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == false) { return; }
            CustomAmmoCategoriesLog.Log.LogWrite("  weapon ammoId is set\n");
            string wGUID = __instance.weapon.StatCollection.GetStatistic(CustomAmmoCategories.GUIDStatisticName).Value<string>();
            CustomAmmoCategories.resetWeaponEffects(wGUID);
            return;
        }
    }
    [HarmonyPatch(typeof(CombatGameState))]
    [HarmonyPatch("ShutdownCombatState")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { })]
    public static class CombatGameState_ShutdownCombatState
    {
        public static void Postfix(CombatGameState __instance)
        {
            CustomAmmoCategoriesLog.Log.LogWrite("CombatGameState.ShutdownCombatState\n");
            CustomAmmoCategories.clearAllWeaponEffects();
            return;
        }
    }
}

namespace CustAmmoCategories
{
    public class CustomAmmoCategory
    {
        public string Id { get; set; }
        public int Index { get; set; }
        public BattleTech.AmmoCategory BaseCategory { get; set; }
        public CustomAmmoCategory()
        {
            Index = 0;
            BaseCategory = AmmoCategory.NotSet;
            Id = "NotSet";
        }
    }
    public enum HitGeneratorType
    {
        NotSet,
        Individual,
        Cluster,
        Streak
    }
    public class ExtAmmunitionDef
    {
        public float AccuracyModifier { get; set; }
        public float DirectFireModifier { get; set; }
        public float DamagePerShot { get; set; }
        public float HeatDamagePerShot { get; set; }
        public float CriticalChanceMultiplier { get; set; }
        public int ShotsWhenFired { get; set; }
        public int AIBattleValue { get; set; }
        public int ProjectilesPerShot { get; set; }
        public EffectData[] statusEffects { get; set; }
        public float MinRange { get; set; }
        public float MaxRange { get; set; }
        public float LongRange { get; set; }
        public float ShortRange { get; set; }
        public float MediumRange { get; set; }
        public int RefireModifier { get; set; }
        public int AttackRecoil { get; set; }
        public float Instability { get; set; }
        public string WeaponEffectID { get; set; }
        public float EvasivePipsIgnored { get; set; }
        public int IndirectFireCapable { get; set; }
        public int AOECapable { get; set; }
        public HitGeneratorType HitGenerator {get;set;}
        public ExtAmmunitionDef()
        {
            AccuracyModifier = 0;
            DirectFireModifier = 0;
            DamagePerShot = 0;
            HeatDamagePerShot = 0;
            ProjectilesPerShot = 0;
            ShotsWhenFired = 0;
            CriticalChanceMultiplier = 0;
            MinRange = 0;
            MaxRange = 0;
            LongRange = 0;
            MaxRange = 0;
            ShortRange = 0;
            MediumRange = 0;
            AIBattleValue = 100;
            RefireModifier = 0;
            Instability = 0;
            AttackRecoil = 0;
            EvasivePipsIgnored = 0;
            IndirectFireCapable = -1;
            AOECapable = -1;
            WeaponEffectID = "";
            HitGenerator = HitGeneratorType.NotSet;
            statusEffects = new EffectData[0] { };
        }
    }
    public class ExtWeaponDef
    {
        public HitGeneratorType HitGenerator { get; set; }
        public bool StreakEffect { get; set; }
        public float DirectFireModifier { get; set; }
        public ExtWeaponDef()
        {
            StreakEffect = false;
            HitGenerator = HitGeneratorType.NotSet;
            DirectFireModifier = 0;
        }
    }
    public class WeaponAmmoInfo
    {
        public List<string> AvaibleAmmo { get; set; }
        public int CurrentAmmoIndex { get; set; }
        public string CurrentAmmo
        {
            get
            {
                return AvaibleAmmo[CurrentAmmoIndex];
            }
            set
            {
                int newCurrentAmmoIndex = AvaibleAmmo.IndexOf(value);
                if (newCurrentAmmoIndex >= 0) { CurrentAmmoIndex = newCurrentAmmoIndex; };
            }
        }
        public WeaponAmmoInfo(Weapon weapon)
        {
            AvaibleAmmo = new List<string>();
            int AIBattleValue = CustomAmmoCategories.findExtAmmo(weapon.ammoBoxes[0].ammoDef.Description.Id).AIBattleValue;
            CurrentAmmoIndex = 0;
            for (int t = 0; t < weapon.ammoBoxes.Count; ++t)
            {
                if (AvaibleAmmo.Contains(weapon.ammoBoxes[t].ammoDef.Description.Id) == false)
                {
                    AvaibleAmmo.Add(weapon.ammoBoxes[t].ammoDef.Description.Id);
                    ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(weapon.ammoBoxes[t].ammoDef.Description.Id);
                    if(AIBattleValue < extAmmoDef.AIBattleValue)
                    {
                        AIBattleValue = extAmmoDef.AIBattleValue;
                        CurrentAmmoIndex = t;
                    }
                }
            }
        }
    }
    public static partial class CustomAmmoCategories
    {
        public static string AmmoIdStatName = "CurrentAmmoId";
        public static string GUIDStatisticName = "WeaponGUID";
        public static string StreakStatisticName = "Streak";
        private static Dictionary<string, CustomAmmoCategory> items;
        private static Dictionary<string, CustomAmmoCategory> AmmunitionDef;
        private static Dictionary<string, ExtAmmunitionDef> ExtAmmunitionDef;
        private static Dictionary<string, ExtWeaponDef> ExtWeaponDef;
        private static Dictionary<string, CustomAmmoCategory> WeaponDef;
        private static Dictionary<string, Dictionary<string, WeaponEffect>> WeaponEffects;
        //private static Dictionary<string, WeaponAmmoInfo> WeaponAmmo;
        private static CustomAmmoCategory NotSetCustomAmmoCategoty;
        private static ExtAmmunitionDef DefaultAmmo;
        private static ExtWeaponDef DefaultWeapon;
        public static Settings Settings;
        public static float getDirectFireModifier(Weapon weapon)
        {
            float result = CustomAmmoCategories.getExtWeaponDef(weapon.weaponDef.Description.Id).DirectFireModifier;
            if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName))
            {
                string ammoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
                result += CustomAmmoCategories.findExtAmmo(ammoId).DirectFireModifier;
            }
            return result;
        }
        public static WeaponHitInfo getSuccessOnly(WeaponHitInfo hitInfo)
        {
            int successShots = 0;
            for (int index=0;index < hitInfo.numberOfShots; ++index)
            {
                if((hitInfo.hitLocations[index] != 0)&&(hitInfo.hitLocations[index] != 65536))
                {
                    ++successShots;
                }
            }
            WeaponHitInfo result = new WeaponHitInfo();
            result.attackerId = hitInfo.attackerId;
            result.targetId = hitInfo.targetId;
            result.numberOfShots = successShots;
            result.stackItemUID = hitInfo.stackItemUID;
            result.attackSequenceId = hitInfo.attackSequenceId;
            result.attackGroupIndex = hitInfo.attackGroupIndex;
            result.attackWeaponIndex = hitInfo.attackWeaponIndex;
            result.toHitRolls = new float[successShots];
            result.locationRolls = new float[successShots];
            result.dodgeRolls = new float[successShots];
            result.dodgeSuccesses = new bool[successShots];
            result.hitLocations = new int[successShots];
            result.hitPositions = new Vector3[successShots];
            result.hitVariance = new int[successShots];
            result.hitQualities = new AttackImpactQuality[successShots];
            successShots = 0;
            for (int index = 0; index < hitInfo.numberOfShots; ++index)
            {
                if ((hitInfo.hitLocations[index] != 0) && (hitInfo.hitLocations[index] != 65536))
                {
                    result.toHitRolls[successShots] = hitInfo.toHitRolls[index];
                    result.locationRolls[successShots] = hitInfo.locationRolls[index];
                    result.dodgeRolls[successShots] = hitInfo.dodgeRolls[index];
                    result.dodgeSuccesses[successShots] = hitInfo.dodgeSuccesses[index];
                    result.hitLocations[successShots] = hitInfo.hitLocations[index];
                    result.hitPositions[successShots] = hitInfo.hitPositions[index];
                    result.hitVariance[successShots] = hitInfo.hitVariance[index];
                    result.hitQualities[successShots] = hitInfo.hitQualities[index];
                    ++successShots;
                }
            }
            return result;
        }
        public static void registerExtWeaponDef(string defId,ExtWeaponDef def)
        {
            ExtWeaponDef[defId] = def;
        }
        public static ExtWeaponDef getExtWeaponDef(string defId)
        {
            if (CustomAmmoCategories.ExtWeaponDef.ContainsKey(defId))
            {
                return ExtWeaponDef[defId];
            }else
            {
                return CustomAmmoCategories.DefaultWeapon;
            }
        }
        public static bool getIndirectFireCapable(Weapon weapon)
        {
            //CustomAmmoCategoriesLog.Log.LogWrite("get IndirectFireCapable " + weapon.UIName + "\n");
            if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == false)
            {
                //CustomAmmoCategoriesLog.Log.LogWrite("  weapon has no ammo\n");
                return weapon.weaponDef.IndirectFireCapable;
            }
            string ammoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
            ExtAmmunitionDef extAmmo = CustomAmmoCategories.findExtAmmo(ammoId);
            if (extAmmo.IndirectFireCapable < 0)
            {
                //CustomAmmoCategoriesLog.Log.LogWrite("  ammo not modifying IndirectFireCapable\n");
                return weapon.weaponDef.IndirectFireCapable;
            }
            //CustomAmmoCategoriesLog.Log.LogWrite("  modified IndirectFireCapable\n");
            return extAmmo.IndirectFireCapable != 0;
        }
        public static float getWeaponEvasivePipsIgnored(Weapon weapon)
        {
            //CustomAmmoCategoriesLog.Log.LogWrite("getWeaponEvasivePipsIgnored " + weapon.UIName + "\n");
            if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == false)
            {
                //CustomAmmoCategoriesLog.Log.LogWrite("  weapon has no ammo\n");
                return weapon.weaponDef.EvasivePipsIgnored;
            }
            string ammoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
            ExtAmmunitionDef extAmmo = CustomAmmoCategories.findExtAmmo(ammoId);
            //CustomAmmoCategoriesLog.Log.LogWrite("  modified EvasivePipsIgnored\n");
            return weapon.weaponDef.EvasivePipsIgnored + extAmmo.EvasivePipsIgnored;
        }
        public static int getWeaponAttackRecoil(Weapon weapon)
        {
            CustomAmmoCategoriesLog.Log.LogWrite("getWeaponAttackRecoil " + weapon.UIName + "\n");
            if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == false)
            {
                CustomAmmoCategoriesLog.Log.LogWrite("  weapon has no ammo\n");
                return weapon.weaponDef.AttackRecoil;
            }
            string ammoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
            ExtAmmunitionDef extAmmo = CustomAmmoCategories.findExtAmmo(ammoId);
            CustomAmmoCategoriesLog.Log.LogWrite("  modified AttackRecoil\n");
            return weapon.weaponDef.AttackRecoil+extAmmo.AttackRecoil;
        }
        public static EffectData[] getWeaponStatusEffects(Weapon weapon)
        {
            CustomAmmoCategoriesLog.Log.LogWrite("getWeaponStatusEffects " + weapon.UIName + "\n");
            if(CustomAmmoCategories.checkExistance(weapon.StatCollection,CustomAmmoCategories.AmmoIdStatName) == false)
            {
                CustomAmmoCategoriesLog.Log.LogWrite("  weapon has no ammo\n");
                return weapon.weaponDef.statusEffects;
            }
            string ammoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
            ExtAmmunitionDef extAmmo = CustomAmmoCategories.findExtAmmo(ammoId);
            if (extAmmo.statusEffects.Length == 0) {
                CustomAmmoCategoriesLog.Log.LogWrite("  ammo has no additional status effects\n");
                return weapon.weaponDef.statusEffects;
            }
            if(weapon.weaponDef.statusEffects.Length == 0)
            {
                CustomAmmoCategoriesLog.Log.LogWrite("  weapon has no additional status effects\n");
                return extAmmo.statusEffects;
            }
            EffectData[] result = new EffectData[weapon.weaponDef.statusEffects.Length+extAmmo.statusEffects.Length];
            weapon.weaponDef.statusEffects.CopyTo(result, 0);
            extAmmo.statusEffects.CopyTo(result, weapon.weaponDef.statusEffects.Length);
            CustomAmmoCategoriesLog.Log.LogWrite("  concatinating weapon and ammo status effects\n");
            return result;
        }
        public static void clearAllWeaponEffects()
        {
            CustomAmmoCategories.WeaponEffects.Clear();
        }
        public static void resetWeaponEffects(string wGUID)
        {
            if (CustomAmmoCategories.WeaponEffects.ContainsKey(wGUID) == false) { return; }
            foreach (var weaponEffect in CustomAmmoCategories.WeaponEffects[wGUID])
            {
                if(weaponEffect.Value != (UnityEngine.Object)null)
                {
                    weaponEffect.Value.Reset();
                }
            }
        }
        public static WeaponEffect getWeaponEffect(string wGUID,string weaponEffectId)
        {
            if (CustomAmmoCategories.WeaponEffects.ContainsKey(wGUID) == false) { return null; }
            if (CustomAmmoCategories.WeaponEffects[wGUID].ContainsKey(weaponEffectId) == false) { return null; }
            return CustomAmmoCategories.WeaponEffects[wGUID][weaponEffectId];
        }
        public static void ClearWeaponEffects(string wGUID)
        {
            if (CustomAmmoCategories.WeaponEffects.ContainsKey(wGUID)) { WeaponEffects.Remove(wGUID); };
        }
        public static void InitWeaponEffects(WeaponRepresentation weaponRepresentation,Weapon weapon)
        {
            if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.GUIDStatisticName) == false) { return; }
            string wGUID = weapon.StatCollection.GetStatistic(CustomAmmoCategories.GUIDStatisticName).Value<string>();
            if (CustomAmmoCategories.WeaponEffects.ContainsKey(wGUID) == true) { return; }
            WeaponEffects[wGUID] = new Dictionary<string, WeaponEffect>();
            List<string> avaibleAmmo = CustomAmmoCategories.getAvaibleAmmo(weapon);
            foreach(string ammoId in avaibleAmmo)
            {
                ExtAmmunitionDef extAmmo = CustomAmmoCategories.findExtAmmo(ammoId);
                if (string.IsNullOrEmpty(extAmmo.WeaponEffectID)) { continue; }
                if (extAmmo.WeaponEffectID == weapon.weaponDef.WeaponEffectID) { continue; }
                if (WeaponEffects[wGUID].ContainsKey(extAmmo.WeaponEffectID) == true) { continue; }
                WeaponEffects[wGUID][extAmmo.WeaponEffectID] = CustomAmmoCategories.InitWeaponEffect(weaponRepresentation,weapon,extAmmo.WeaponEffectID);
            }
        }
        public static WeaponEffect InitWeaponEffect(WeaponRepresentation weaponRepresentation, Weapon weapon, string weaponEffectId)
        {
            GameObject gameObject = (GameObject)null;
            WeaponEffect result = (WeaponEffect)null;
            if (!string.IsNullOrEmpty(weaponEffectId))
            {
                gameObject = weaponRepresentation.parentCombatant.Combat.DataManager.PooledInstantiate(weaponEffectId, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
            }
            if ((UnityEngine.Object)gameObject == (UnityEngine.Object)null)
            {
                CustomAmmoCategoriesLog.Log.LogWrite(string.Format("Error instantiating WeaponEffect [{0}], Weapon [{1}]]\n", (object)weaponEffectId, (object)weapon.Name));
            }
            else
            {
                gameObject.transform.parent = weaponRepresentation.transform;
                gameObject.transform.localPosition = Vector3.zero;
                gameObject.transform.rotation = Quaternion.identity;
                result = gameObject.GetComponent<WeaponEffect>();
                if ((UnityEngine.Object)result == (UnityEngine.Object)null)
                {
                    CustomAmmoCategoriesLog.Log.LogWrite(string.Format("Error finding WeaponEffect on GO [{0}], Weapon [{1}]\n", (object)weaponEffectId, (object)weapon.Name));
                }
                else
                {
                    result.Init(weapon);
                }
            }
            CustomAmmoCategoriesLog.Log.LogWrite("Success init weapon effect "+ weaponEffectId+" for "+weapon.Name+"\n");
            return result;
        }

        public static AmmunitionBox getAmmunitionBox(Weapon weapon, int aGUID)
        {
            if ((aGUID >= 0) && (aGUID < weapon.ammoBoxes.Count))
            {
                return weapon.ammoBoxes[aGUID];
            }
            return null;
        }
        public static void RegisterExtAmmoDef(string defId, ExtAmmunitionDef extAmmoDef)
        {
            CustomAmmoCategoriesLog.Log.LogWrite("Registring extAmmoDef " + defId + " D/H/A " + extAmmoDef.DamagePerShot + "/" + extAmmoDef.HeatDamagePerShot + "/" + extAmmoDef.AccuracyModifier + "\n");
            ExtAmmunitionDef[defId] = extAmmoDef;
        }
        public static ExtAmmunitionDef findExtAmmo(string ammoDefId)
        {
            if (CustomAmmoCategories.ExtAmmunitionDef.ContainsKey(ammoDefId))
            {
                return CustomAmmoCategories.ExtAmmunitionDef[ammoDefId];
            }
            return CustomAmmoCategories.DefaultAmmo;
        }
        public static void CycleAmmoBest(Weapon weapon)
        {
            if (weapon.ammoBoxes.Count == 0)
            {
                return;
            };
            if(CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == false){
                weapon.StatCollection.AddStatistic<string>(CustomAmmoCategories.AmmoIdStatName, CustomAmmoCategories.findBestAmmo(weapon));
            }else{
                weapon.StatCollection.Set<string>(CustomAmmoCategories.AmmoIdStatName, CustomAmmoCategories.findBestAmmo(weapon));
            }
        }
        public static List<string> getAvaibleAmmo(Weapon weapon)
        {
            List<string> result = new List<string>();
            for(int index=0;index < weapon.ammoBoxes.Count; ++index)
            {
                if(result.IndexOf(weapon.ammoBoxes[index].ammoDef.Description.Id) < 0)
                {
                    result.Add(weapon.ammoBoxes[index].ammoDef.Description.Id);
                }
            }
            return result;
        }
        public static void CycleAmmo(Weapon weapon)
        {
            CustomAmmoCategoriesLog.Log.LogWrite("Cycle Ammo\n");
            if (weapon.ammoBoxes.Count == 0)
            {
                CustomAmmoCategoriesLog.Log.LogWrite(" no ammo boxes\n");
                return;
            };
            if(CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == false)
            {
                CustomAmmoCategoriesLog.Log.LogWrite(" Current weapon is not set\n");
                weapon.StatCollection.AddStatistic<string>(CustomAmmoCategories.AmmoIdStatName, CustomAmmoCategories.findBestAmmo(weapon));
                return;
            }
            string CurrentAmmo = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
            List<string> AvaibleAmmo = CustomAmmoCategories.getAvaibleAmmo(weapon);
            int CurrentAmmoIndex = AvaibleAmmo.IndexOf(CurrentAmmo);
            if(CurrentAmmoIndex < 0)
            {
                weapon.StatCollection.Set<string>(CustomAmmoCategories.AmmoIdStatName, CustomAmmoCategories.findBestAmmo(weapon));
                return;
            }
            for (int ti = 1; ti <= AvaibleAmmo.Count; ++ti)
            {
                string tempAmmo = AvaibleAmmo[(ti+ CurrentAmmoIndex)%AvaibleAmmo.Count];
                for (int t = 0; t < weapon.ammoBoxes.Count; ++t)
                {
                    int cur_index = t;
                    //CustomAmmoCategoriesLog.Log.LogWrite(" test ammoBox index " + cur_index.ToString() + "\n");
                    if (weapon.ammoBoxes[cur_index].IsFunctional == false)
                    {
                        //CustomAmmoCategoriesLog.Log.LogWrite("   broken\n");
                        continue;
                    }
                    if (weapon.ammoBoxes[cur_index].CurrentAmmo <= 0)
                    {
                        //CustomAmmoCategoriesLog.Log.LogWrite("   depleeded\n");
                        continue;
                    }
                    if (weapon.ammoBoxes[cur_index].ammoDef.Description.Id != tempAmmo)
                    {
                        //CustomAmmoCategoriesLog.Log.LogWrite("   different ammo\n");
                        continue;
                    }
                    CustomAmmoCategoriesLog.Log.LogWrite("   cycled to " + tempAmmo + "\n");
                    weapon.StatCollection.Set<string>(CustomAmmoCategories.AmmoIdStatName, tempAmmo);
                    return;
                }
            }
        }
        public static void RegisterPlayerWeapon(Weapon weapon) 
        {
            CustomAmmoCategoriesLog.Log.LogWrite("RegisterPlayerWeapon\n");
            if (weapon == null) {
                CustomAmmoCategoriesLog.Log.LogWrite("  Weapon is NULL WTF?!\n");
                return;
            }
            if (weapon.weaponDef == null) {
                CustomAmmoCategoriesLog.Log.LogWrite("  WeaponDef is NULL\n");
                return;
            }
            if (weapon.parent == null)
            {
                CustomAmmoCategoriesLog.Log.LogWrite("  Parent is NULL\n");
                return;
            }
            if (weapon.AmmoCategory == AmmoCategory.NotSet)
            {
                CustomAmmoCategoriesLog.Log.LogWrite(" AmmoCategory not set - energy weapon\n");
                return;
            }
            else
            {
                CustomAmmoCategoriesLog.Log.LogWrite(" AmmoCategory is set " + weapon.AmmoCategory.ToString() + "\n");
            }
            if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == false)
            {
                weapon.StatCollection.AddStatistic<string>(CustomAmmoCategories.AmmoIdStatName, CustomAmmoCategories.findBestAmmo(weapon));
                CustomAmmoCategoriesLog.Log.LogWrite(" Add to weapon stat collection " + weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>() + "\n");
            }
        }
        public static bool checkExistance(StatCollection statCollection,string statName)
        {
            return ((Dictionary<string, Statistic>)typeof(StatCollection).GetField("stats", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(statCollection)).ContainsKey(statName);
        }
        public static CustomAmmoCategory findByIndex(int index)
        {
            foreach (var item in CustomAmmoCategories.items)
            {
                if (item.Value.Index == index)
                {
                    return item.Value;
                }
            }
            return NotSetCustomAmmoCategoty;
        }
        public static CustomAmmoCategory findWeaponRealCategory(string id, string def)
        {
            if (CustomAmmoCategories.WeaponDef.ContainsKey(id))
            {
                return CustomAmmoCategories.WeaponDef[id];
            }
            else
            {
                if (CustomAmmoCategories.items.ContainsKey(def))
                {
                    return CustomAmmoCategories.items[def];
                }
                return CustomAmmoCategories.NotSetCustomAmmoCategoty;
            }
        }
        public static CustomAmmoCategory findAmunitionRealCategory(string id, string def)
        {
            if (CustomAmmoCategories.AmmunitionDef.ContainsKey(id))
            {
                return CustomAmmoCategories.AmmunitionDef[id];
            }
            else
            {
                if (CustomAmmoCategories.items.ContainsKey(def))
                {
                    return CustomAmmoCategories.items[def];
                }
                return CustomAmmoCategories.NotSetCustomAmmoCategoty;
            }
        }
        public static void RegisterAmmunition(string id, CustomAmmoCategory custCat)
        {
            CustomAmmoCategoriesLog.Log.LogWrite("RegisterAmmunition CustomAmmoCategory(" + custCat.Id + "/" + custCat.BaseCategory.ToString() + ") for " + id + "\n");
            CustomAmmoCategories.AmmunitionDef[id] = custCat;
        }
        public static void RegisterWeapon(string id, CustomAmmoCategory custCat)
        {
            CustomAmmoCategoriesLog.Log.LogWrite("RegisterWeapon CustomAmmoCategory(" + custCat.Id + "/" + custCat.BaseCategory.ToString() + ") for " + id + "\n");
            CustomAmmoCategories.WeaponDef[id] = custCat;
        }
        public static string findBestAmmo(Weapon weapon)
        {
            if (weapon.ammoBoxes.Count <= 0) { return ""; };
            string result = "";
            int AIBattleValue = 0;
            for(int index=0;index < weapon.ammoBoxes.Count; ++index)
            {
                if (result == weapon.ammoBoxes[index].Description.Id) { continue; }
                if (weapon.ammoBoxes[index].CurrentAmmo <= 0) { continue; }
                if (weapon.ammoBoxes[index].IsFunctional == false) { continue; }
                ExtAmmunitionDef extAmmo = CustomAmmoCategories.findExtAmmo(weapon.ammoBoxes[index].ammoDef.Description.Id);
                if ( AIBattleValue < extAmmo.AIBattleValue)
                {
                    AIBattleValue = extAmmo.AIBattleValue;
                    result = weapon.ammoBoxes[index].ammoDef.Description.Id;
                }
            }
            if (string.IsNullOrEmpty(result))
            {
                result = weapon.ammoBoxes[0].ammoDef.Description.Id;
            }
            return result;
        }
        public static void CustomCategoriesInit()
        {
            CustomAmmoCategories.items = new Dictionary<string, CustomAmmoCategory>();
            CustomAmmoCategories.AmmunitionDef = new Dictionary<string, CustomAmmoCategory>();
            CustomAmmoCategories.WeaponDef = new Dictionary<string, CustomAmmoCategory>();
            CustomAmmoCategories.WeaponEffects = new Dictionary<string, Dictionary<string, WeaponEffect>>();
            //CustomAmmoCategories.WeaponAmmo = new Dictionary<string, WeaponAmmoInfo>();
            CustomAmmoCategories.ExtAmmunitionDef = new Dictionary<string, ExtAmmunitionDef>();
            CustomAmmoCategories.ExtWeaponDef = new Dictionary<string, ExtWeaponDef>();
            CustomAmmoCategories.DefaultAmmo = new ExtAmmunitionDef();
            CustomAmmoCategories.DefaultWeapon = new ExtWeaponDef();
            //string assemblyFile = (new System.Uri(Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath;
            string filename = Path.Combine(CustomAmmoCategoriesLog.Log.BaseDirectory, "CustomAmmoCategories.json");
            //filename = Path.Combine(filename, "CustomAmmoCategories.json");
            string json = File.ReadAllText(filename);
            List<CustomAmmoCategory> tmp = JsonConvert.DeserializeObject<List<CustomAmmoCategory>>(json);
            CustomAmmoCategoriesLog.Log.LogWrite("Custom ammo categories:\n");
            foreach (AmmoCategory base_cat in Enum.GetValues(typeof(AmmoCategory)))
            {
                CustomAmmoCategory itm = new CustomAmmoCategory();
                itm.BaseCategory = base_cat;
                itm.Id = base_cat.ToString();
                itm.Index = (int)base_cat;
                items[itm.Id] = itm;
                if (itm.Index == 0) { NotSetCustomAmmoCategoty = itm; };
            }
            foreach (var itm in tmp)
            {
                itm.Index = items.Count;
                items[itm.Id] = itm;
            }
            CustomAmmoCategoriesLog.Log.LogWrite("Custom ammo categories:\n");
            foreach (var itm in items)
            {
                CustomAmmoCategoriesLog.Log.LogWrite("  '" + itm.Key + "'= (" + itm.Value.Index + "/" + itm.Value.Id + "/" + itm.Value.BaseCategory.ToString() + ")\n");
            }
        }
        public static CustomAmmoCategory find(string id)
        {
            if (CustomAmmoCategories.items.ContainsKey(id))
            {
                return CustomAmmoCategories.items[id];
            }
            else
            {
                return NotSetCustomAmmoCategoty;
            }
        }
    }
}

namespace CustAmmoCategories
{
    public class ThreadWork
    {
        public class CDefItem
        {
            public string name { get; set; }
            public int price { get; set; }
            public BattleTech.ShopItemType type { get; set; }
            public int count { get; set; }
        }
        public class CSetReputation
        {
            public string faction { get; set; }
            public int reputation { get; set; }
        }
        private static string GetMimeType(string ext)
        {
            switch (ext)
            {
                case ".html": return "text/html";
                case ".htm": return "text/html";
                case ".txt": return "text/plain";
                case ".jpe": return "image/jpeg";
                case ".jpeg": return "image/jpeg";
                case ".jpg": return "image/jpeg";
                case ".js": return "application/javascript";
                case ".png": return "image/png";
                case ".gif": return "image/gif";
                case ".bmp": return "image/bmp";
                case ".ico": return "image/x-icon";
            }
            return "application/octed-stream";
        }
        private static void SendError(ref HttpListenerResponse response, int Code, string text)
        {
            // Получаем строку вида "200 OK"
            // HttpStatusCode хранит в себе все статус-коды HTTP/1.1
            response.StatusCode = Code;
            response.ContentType = "text/html";
            string Html = "<html><body><h1>" + text + "</h1></body></html>";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(Html);
            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;
            // Закроем соединение
            output.Write(buffer, 0, buffer.Length);
            output.Close();
            response.Close();
        }
        private static void SendResponce(ref HttpListenerResponse response, object jresp)
        {
            // Получаем строку вида "200 OK"
            // HttpStatusCode хранит в себе все статус-коды HTTP/1.1
            response.StatusCode = 200;
            response.ContentType = "application/json";
            string Html = JsonConvert.SerializeObject(jresp);
            // Приведем строку к виду массива байт
            byte[] Buffer = Encoding.UTF8.GetBytes(Html);
            // Отправим его клиенту
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(Html);
            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;
            // Закроем соединение
            output.Write(buffer, 0, buffer.Length);
            output.Close();
            response.Close();
        }

        public static void DoWork()
        {
            CustomAmmoCategoriesLog.Log.LogWrite("Initing http server "+ CustomAmmoCategories.Settings.modHTTPServer + "...\n");
            if (CustomAmmoCategories.Settings.modHTTPServer == false) { return; }
            HttpListener listener = new HttpListener();
            CustomAmmoCategoriesLog.Log.LogWrite("Prefix " + CustomAmmoCategories.Settings.modHTTPListen + "...\n");
            listener.Prefixes.Add(CustomAmmoCategories.Settings.modHTTPListen);
            listener.Start();
            string assemblyFile = (new System.Uri(Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath;
            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;
                string filename = request.Url.AbsolutePath;
                if (filename == "/") { filename = "index.html"; };
                filename = Path.Combine(CustomAmmoCategoriesLog.Log.BaseDirectory, Path.GetFileName(filename));
                CustomAmmoCategoriesLog.Log.LogWrite("Base directory "+ CustomAmmoCategoriesLog.Log.BaseDirectory + " Access '" + filename +"'\n");
                if (File.Exists(filename))
                {
                    CustomAmmoCategoriesLog.Log.LogWrite("File exists\n");
                    response.ContentType = GetMimeType(Path.GetExtension(filename));
                    FileStream FS;
                    try
                    {
                        FS = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                    }
                    catch (Exception)
                    {
                        // Если случилась ошибка, посылаем клиенту ошибку 500
                        SendError(ref response, 500, "Не могу открыть " + filename);
                        return;
                    }
                    Stream output = response.OutputStream;
                    byte[] Buffer = new byte[1024];
                    // Переменная для хранения количества байт, принятых от клиента
                    int Count = 0;
                    // Пока не достигнут конец файла
                    while (FS.Position < FS.Length)
                    {
                        // Читаем данные из файла
                        Count = FS.Read(Buffer, 0, Buffer.Length);
                        // И передаем их клиенту
                        output.Write(Buffer, 0, Count);
                    }
                    // Закроем файл и соединение
                    FS.Close();
                    output.Close();
                    response.Close();
                    CustomAmmoCategoriesLog.Log.LogWrite("File out\n");
                    continue;
                }
                CustomAmmoCategoriesLog.Log.LogWrite("Get data:'"+ Path.GetFileName(filename) + "'\n");
                if (Path.GetFileName(filename) == "getreputation")
                {
                    CustomAmmoCategoriesLog.Log.LogWrite("Запрос на получение репутации\n");
                    System.Collections.Generic.Dictionary<string, string> jresp = new Dictionary<string, string>();
                    BattleTech.GameInstance gameInstance = BattleTech.UnityGameInstance.BattleTechGame;
                    CustomAmmoCategoriesLog.Log.LogWrite("Получен gameInstance\n");
                    if (gameInstance == null)
                    {
                        jresp["error"] = "Не могу получить инстанс игры";
                        SendResponce(ref response, jresp);
                        continue;
                    }
                    BattleTech.SimGameState gameState = gameInstance.Simulation;
                    CustomAmmoCategoriesLog.Log.LogWrite("Получен gameState\n");
                    if (gameState == null)
                    {
                        jresp["error"] = "Не могу получить состояние симулятора. Скорее всего не загружено сохранение";
                        SendResponce(ref response, jresp);
                        continue;
                    }
                    if ((gameState.SimGameMode != BattleTech.SimGameState.SimGameType.CAREER) && (gameState.SimGameMode != BattleTech.SimGameState.SimGameType.KAMEA_CAMPAIGN))
                    {
                        jresp["error"] = "Неправильный режим компании:" + gameState.SimGameMode.ToString();
                    }
                    var factions = gameState.FactionsDict;
                    CustomAmmoCategoriesLog.Log.LogWrite("Получен список фракций\n");
                    foreach (var pFaction in factions)
                    {
                        jresp[pFaction.Key.ToString()] = gameState.GetRawReputation(pFaction.Key).ToString();
                    }
                    SendResponce(ref response, jresp);
                    continue;
                }
                if (Path.GetFileName(filename) == "setreputation")
                {
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(request.InputStream, request.ContentEncoding))
                    {
                        string data = reader.ReadToEnd();
                        CSetReputation setrep = JsonConvert.DeserializeObject<CSetReputation>(data);
                        CustomAmmoCategoriesLog.Log.LogWrite("Запрос на установку репутации\n");
                        System.Collections.Generic.Dictionary<string, string> jresp = new Dictionary<string, string>();
                        BattleTech.GameInstance gameInstance = BattleTech.UnityGameInstance.BattleTechGame;
                        CustomAmmoCategoriesLog.Log.LogWrite("Получен gameInstance\n");
                        if (gameInstance == null)
                        {
                            jresp["error"] = "Не могу получить инстанс игры";
                            SendResponce(ref response, jresp);
                            continue;
                        }
                        BattleTech.SimGameState gameState = gameInstance.Simulation;
                        CustomAmmoCategoriesLog.Log.LogWrite("Получен gameState\n");
                        if (gameState == null)
                        {
                            jresp["error"] = "Не могу получить состояние симулятора. Скорее всего не загружено сохранение";
                            SendResponce(ref response, jresp);
                            continue;
                        }
                        if ((gameState.SimGameMode != BattleTech.SimGameState.SimGameType.CAREER) && (gameState.SimGameMode != BattleTech.SimGameState.SimGameType.KAMEA_CAMPAIGN))
                        {
                            jresp["error"] = "Неправильный режим компании:" + gameState.SimGameMode.ToString();
                        }
                        var factions = gameState.FactionsDict;
                        CustomAmmoCategoriesLog.Log.LogWrite("Получен список фракций\n");
                        foreach (var pFaction in factions)
                        {
                            if (setrep.faction.Equals(pFaction.Key.ToString()))
                            {
                                gameState.SetReputation(pFaction.Key, setrep.reputation, BattleTech.StatCollection.StatOperation.Set);
                            }
                            jresp[pFaction.Key.ToString()] = gameState.GetRawReputation(pFaction.Key).ToString();
                        }
                        SendResponce(ref response, jresp);
                        continue;
                    }
                }
                if (Path.GetFileName(filename) == "listitems")
                {
                    CustomAmmoCategoriesLog.Log.LogWrite("Запрос на перечисление пилотов предмета\n");
                    System.Collections.Generic.Dictionary<string, string> jresp = new Dictionary<string, string>();
                    BattleTech.GameInstance gameInstance = BattleTech.UnityGameInstance.BattleTechGame;
                    CustomAmmoCategoriesLog.Log.LogWrite("Получен gameInstance\n");
                    if (gameInstance == null)
                    {
                        jresp["error"] = "Не могу получить инстанс игры";
                        SendResponce(ref response, jresp);
                        continue;
                    }
                    //gameInstance.DataManager
                    /*BattleTech.SimGameState gameState = gameInstance.Simulation;
                   CustomAmmoCategoriesLog.Log.LogWrite("Получен gameState\n");
                    if (gameState == null)
                    {
                        jresp["error"] = "Не могу получить состояние симулятора. Скорее всего не загружено сохранение";
                        SendResponce(ref response, jresp);
                        continue;
                    }
                    if ((gameState.SimGameMode != BattleTech.SimGameState.SimGameType.CAREER) && (gameState.SimGameMode != BattleTech.SimGameState.SimGameType.KAMEA_CAMPAIGN))
                    {
                        jresp["error"] = "Неправильный режим компании:" + gameState.SimGameMode.ToString();
                    }*/
                    BattleTech.Data.DataManager dataManager = gameInstance.DataManager;
                    if (dataManager == null)
                    {
                        jresp["error"] = "Не могу получить дата менеджер";
                        SendResponce(ref response, jresp);
                        continue;
                    }
                    List<CDefItem> items = new List<CDefItem>();
                    List<CDefItem> weapons = new List<CDefItem>();
                    foreach (var wDef in dataManager.WeaponDefs)
                    {
                        CDefItem itm = new CDefItem();
                        itm.name = wDef.Key;
                        itm.price = wDef.Value.Description.Cost;
                        itm.type = BattleTech.ShopItemType.Weapon;
                        itm.count = 1;
                        weapons.Add(itm);
                    }
                    weapons.Sort((x, y) => x.name.CompareTo(y.name));
                    List<CDefItem> amunitionBoxes = new List<CDefItem>();
                    foreach (var abDef in dataManager.AmmoBoxDefs)
                    {
                        CDefItem itm = new CDefItem();
                        itm.name = abDef.Key;
                        itm.price = abDef.Value.Description.Cost;
                        itm.type = BattleTech.ShopItemType.AmmunitionBox;
                        itm.count = 1;
                        amunitionBoxes.Add(itm);
                    }
                    amunitionBoxes.Sort((x, y) => x.name.CompareTo(y.name));
                    List<CDefItem> heatSinks = new List<CDefItem>();
                    foreach (var hsDef in dataManager.HeatSinkDefs)
                    {
                        CDefItem itm = new CDefItem();
                        itm.name = hsDef.Key;
                        itm.price = hsDef.Value.Description.Cost;
                        itm.type = BattleTech.ShopItemType.HeatSink;
                        itm.count = 1;
                        heatSinks.Add(itm);
                    }
                    heatSinks.Sort((x, y) => x.name.CompareTo(y.name));
                    List<CDefItem> jumpJets = new List<CDefItem>();
                    foreach (var jjDef in dataManager.JumpJetDefs)
                    {
                        CDefItem itm = new CDefItem();
                        itm.name = jjDef.Key;
                        itm.price = jjDef.Value.Description.Cost;
                        itm.type = BattleTech.ShopItemType.JumpJet;
                        itm.count = 1;
                        jumpJets.Add(itm);
                    }
                    jumpJets.Sort((x, y) => x.name.CompareTo(y.name));
                    List<CDefItem> upgrades = new List<CDefItem>();
                    foreach (var uDef in dataManager.UpgradeDefs)
                    {
                        CDefItem itm = new CDefItem();
                        itm.name = uDef.Key;
                        itm.price = uDef.Value.Description.Cost;
                        itm.type = BattleTech.ShopItemType.Upgrade;
                        itm.count = 1;
                        upgrades.Add(itm);
                    }
                    upgrades.Sort((x, y) => x.name.CompareTo(y.name));
                    List<CDefItem> mechs = new List<CDefItem>();
                    foreach (var mDef in dataManager.MechDefs)
                    {
                        CDefItem itm = new CDefItem();
                        itm.name = mDef.Key;
                        itm.price = mDef.Value.Description.Cost;
                        itm.type = BattleTech.ShopItemType.Mech;
                        itm.count = 1;
                        mechs.Add(itm);
                    }
                    mechs.Sort((x, y) => x.name.CompareTo(y.name));
                    items.AddRange(weapons);
                    items.AddRange(amunitionBoxes);
                    items.AddRange(heatSinks);
                    items.AddRange(upgrades);
                    items.AddRange(jumpJets);
                    items.AddRange(mechs);
                    SendResponce(ref response, items);
                    continue;
                }
                if (Path.GetFileName(filename) == "getchassisjson")
                {
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(request.InputStream, request.ContentEncoding))
                    {
                        CustomAmmoCategoriesLog.Log.LogWrite("Запрос на получение файла шасси\n");
                        System.Collections.Generic.Dictionary<string, string> jresp = new Dictionary<string, string>();
                        BattleTech.GameInstance gameInstance = BattleTech.UnityGameInstance.BattleTechGame;
                        CustomAmmoCategoriesLog.Log.LogWrite("Получен gameInstance\n");
                        if (gameInstance == null)
                        {
                            jresp["error"] = "Не могу получить инстанс игры";
                            SendResponce(ref response, jresp);
                            continue;
                        }
                        BattleTech.SimGameState gameState = gameInstance.Simulation;
                        CustomAmmoCategoriesLog.Log.LogWrite("Получен gameState\n");
                        if (gameState == null)
                        {
                            jresp["error"] = "Не могу получить состояние симулятора. Скорее всего не загружено сохранение";
                            SendResponce(ref response, jresp);
                            continue;
                        }
                        if ((gameState.SimGameMode != BattleTech.SimGameState.SimGameType.CAREER) && (gameState.SimGameMode != BattleTech.SimGameState.SimGameType.KAMEA_CAMPAIGN))
                        {
                            jresp["error"] = "Неправильный режим компании:" + gameState.SimGameMode.ToString();
                        }
                        BattleTech.Data.DataManager dataManager = gameInstance.DataManager;
                        if (dataManager == null)
                        {
                            jresp["error"] = "Не могу получить дата менеджер";
                            SendResponce(ref response, jresp);
                            continue;
                        }
                        string data = reader.ReadToEnd();
                        data = "mechdef_cyclops_CP-10-Z";
                        string jchassi = "{}";
                        bool chassi_found = false;
                        foreach (var mDef in dataManager.MechDefs)
                        {
                            if (mDef.Key == data)
                            {
                                jchassi = mDef.Value.Chassis.ToJSON();
                                chassi_found = true;
                            }
                        }
                        if (chassi_found == false)
                        {
                            jresp["error"] = "Не могу найти такое шасси";
                            SendResponce(ref response, jresp);
                            continue;
                        }
                        response.StatusCode = 200;
                        response.ContentType = "application/json";
                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(jchassi);
                        response.ContentLength64 = buffer.Length;
                        Stream output = response.OutputStream;
                        // Закроем соединение
                        output.Write(buffer, 0, buffer.Length);
                        output.Close();
                        response.Close();
                        continue;
                    }
                }
                if (Path.GetFileName(filename) == "additem")
                {
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(request.InputStream, request.ContentEncoding))
                    {
                        CustomAmmoCategoriesLog.Log.LogWrite("Запрос на добавление предмета\n");
                        System.Collections.Generic.Dictionary<string, string> jresp = new Dictionary<string, string>();
                        BattleTech.GameInstance gameInstance = BattleTech.UnityGameInstance.BattleTechGame;
                        CustomAmmoCategoriesLog.Log.LogWrite("Получен gameInstance\n");
                        if (gameInstance == null)
                        {
                            jresp["error"] = "Не могу получить инстанс игры";
                            SendResponce(ref response, jresp);
                            continue;
                        }
                        BattleTech.SimGameState gameState = gameInstance.Simulation;
                        CustomAmmoCategoriesLog.Log.LogWrite("Получен gameState\n");
                        if (gameState == null)
                        {
                            jresp["error"] = "Не могу получить состояние симулятора. Скорее всего не загружено сохранение";
                            SendResponce(ref response, jresp);
                            continue;
                        }
                        if ((gameState.SimGameMode != BattleTech.SimGameState.SimGameType.CAREER) && (gameState.SimGameMode != BattleTech.SimGameState.SimGameType.KAMEA_CAMPAIGN))
                        {
                            jresp["error"] = "Неправильный режим компании:" + gameState.SimGameMode.ToString();
                        }
                        string data = reader.ReadToEnd();
                        CDefItem itm = JsonConvert.DeserializeObject<CDefItem>(data);
                        try
                        {
                            if (itm.type == BattleTech.ShopItemType.Mech)
                            {
                                gameState.CurSystem.SystemShop.ActiveInventory.Add(new BattleTech.ShopDefItem(itm.name, BattleTech.ShopItemType.Mech, 0.0f, itm.count, false, false, itm.price));
                                gameState.AddFunds(itm.price);
                            }
                            else
                            {
                                gameState.AddFromShopDefItem(new BattleTech.ShopDefItem(itm.name, itm.type, 0.0f, itm.count, false, false, itm.price));
                            }
                            jresp["success"] = "yes";
                        }
                        catch (Exception e)
                        {
                            jresp["error"] = e.ToString();
                        }

                        //gameState.PilotRoster.ElementAt(0).AddAbility("");
                        SendResponce(ref response, jresp);
                        continue;
                    }

                    //gameState.AddFromShopDefItem(new BattleTech.ShopDefItem("Weapon_Laser_MediumLaser_2-Magna", BattleTech.ShopItemType.Weapon, 0.0f, 6, false, false, 80000));
                    //gameState.AddFromShopDefItem(new BattleTech.ShopDefItem("Weapon_SRM_SRM6_3-Valiant", BattleTech.ShopItemType.Weapon, 0.0f, 4, false, false, 140000));
                    //gameState.AddFromShopDefItem(new BattleTech.ShopDefItem("Gear_TargetingTrackingSystem_Hartford_S2000", BattleTech.ShopItemType.Upgrade, 0.0f, 8, false, false, 2060000));
                    //gameState.AddFromShopDefItem(new BattleTech.ShopDefItem("Gear_TargetingTrackingSystem_RCA_InstaTrac-XII", BattleTech.ShopItemType.Upgrade, 0.0f, 8, false, false, 2460000));
                    //gameState.AddFromShopDefItem(new BattleTech.ShopDefItem("Gear_TargetingTrackingSystem_Kallon_Lock-On", BattleTech.ShopItemType.Upgrade, 0.0f, 8, false, false, 3080000));
                    //gameState.AddFromShopDefItem(new BattleTech.ShopDefItem("Gear_Cockpit_Majesty_M_M_MagestrixAlpha", BattleTech.ShopItemType.Upgrade, 0.0f, 2, false, false, 1520000));
                    //gameState.AddFromShopDefItem(new BattleTech.ShopDefItem("Gear_Cockpit_StarCorps_Dalban", BattleTech.ShopItemType.Upgrade, 0.0f, 6, false, false, 940000));
                    //gameState.AddFromShopDefItem(new BattleTech.ShopDefItem("Gear_HeatSink_Generic_Double", BattleTech.ShopItemType.HeatSink, 0.0f, 20, false, false, 630000));
                    //gameState.AddFromShopDefItem(new BattleTech.ShopDefItem("Gear_HeatSink_Generic_Thermal-Exchanger-I", BattleTech.ShopItemType.HeatSink, 0.0f, 20, false, false, 360000));
                    //gameState.AddFromShopDefItem(new BattleTech.ShopDefItem("Gear_HeatSink_Generic_Thermal-Exchanger-II", BattleTech.ShopItemType.HeatSink, 0.0f, 20, false, false, 540000));
                    //gameState.AddFromShopDefItem(new BattleTech.ShopDefItem("Gear_HeatSink_Generic_Thermal-Exchanger-III", BattleTech.ShopItemType.HeatSink, 0.0f, 20, false, false, 720000));
                    //gameState.AddFromShopDefItem(new BattleTech.ShopDefItem("Weapon_Gauss_Gauss_2-M9", BattleTech.ShopItemType.Weapon, 0.0f, 15, false, false, 4440000));
                    //gameState.AddFromShopDefItem(new BattleTech.ShopDefItem("Weapon_PPC_PPCER_2-TiegartMagnum", BattleTech.ShopItemType.Weapon, 0.0f, 15, false, false, 2790000));
                    //gameState.AddFromShopDefItem(new BattleTech.ShopDefItem("Weapon_LRM_LRM20_3-Zeus", BattleTech.ShopItemType.Weapon, 0.0f, 15, false, false, 400000));
                    //gameState.AddFromShopDefItem(new BattleTech.ShopDefItem("Weapon_LRM_LRM10_3-Zeus", BattleTech.ShopItemType.Weapon, 0.0f, 10, false, false, 120000));
                    //gameState.AddFromShopDefItem(new BattleTech.ShopDefItem("Weapon_LRM_LRM5_3-Zeus", BattleTech.ShopItemType.Weapon, 0.0f, 20, false, false, 120000));
                    //gameState.AddFromShopDefItem(new BattleTech.ShopDefItem("Ammo_AmmunitionBox_Generic_GAUSS", BattleTech.ShopItemType.AmmunitionBox, 0.0f, 15, false, false, 50000));
                    //gameState.CurSystem.SystemShop.ActiveInventory.Add(new BattleTech.ShopDefItem("mechdef_kingcrab_KGC-0000", BattleTech.ShopItemType.MechPart, 0.0f, 1, false, false, 1270000));
                    //gameState.CurSystem.SystemShop.ActiveInventory.Add(new BattleTech.ShopDefItem("mechdef_atlas_AS7-D", BattleTech.ShopItemType.MechPart, 0.0f, 1, false, false, 1300000));
                    //gameState.CurSystem.SystemShop.ActiveInventory.Add(new BattleTech.ShopDefItem("mechdef_highlander_HGN-733P", BattleTech.ShopItemType.MechPart, 0.0f, 1, false, false, 1120000));
                    //gameState.AddFromShopDefItem(new BattleTech.ShopDefItem("mechdef_atlas_AS7-D-HT", BattleTech.ShopItemType.MechPart, 0.0f, 2, false, false, 2060000));
                    //gameState.CurSystem.SystemShop.ActiveInventory.Add(new BattleTech.ShopDefItem("mechdef_atlas_AS7-D-HT", BattleTech.ShopItemType.MechPart, 0.0f, 1, false, false, 2060000));
                    //gameState.PilotRoster.ElementAt(0).AddAbility("");
                }
                if (Path.GetFileName(filename) == "test")
                {
                    System.Collections.Generic.Dictionary<string, string> jresp = new Dictionary<string, string>();
                    BattleTech.GameInstance gameInstance = BattleTech.UnityGameInstance.BattleTechGame;
                    CustomAmmoCategoriesLog.Log.LogWrite("Получен gameInstance\n");
                    if (gameInstance == null)
                    {
                        jresp["error"] = "Не могу получить инстанс игры";
                        SendResponce(ref response, jresp);
                        continue;
                    }
                    //gameInstance.DataManager.
                    foreach (var descr in gameInstance.DataManager.BaseDescriptionDefs)
                    {
                        jresp[descr.Key] = descr.Value.ToJSON();
                    }
                    SendResponce(ref response, jresp);
                    continue;
                }
                if (Path.GetFileName(filename) == "uppilots")
                {
                    CustomAmmoCategoriesLog.Log.LogWrite("Запрос на список пилотов\n");
                    Dictionary<string, Dictionary<string, string>> jresp = new Dictionary<string, Dictionary<string, string>>();
                    BattleTech.GameInstance gameInstance = BattleTech.UnityGameInstance.BattleTechGame;
                    CustomAmmoCategoriesLog.Log.LogWrite("Получен gameInstance\n");
                    if (gameInstance == null)
                    {
                        jresp["error"] = new Dictionary<string, string>();
                        jresp["error"]["string"] = "Не могу получить инстанс игры";
                        SendResponce(ref response, jresp);
                        continue;
                    }
                    BattleTech.SimGameState gameState = gameInstance.Simulation;
                    CustomAmmoCategoriesLog.Log.LogWrite("Получен gameState\n");
                    if (gameState == null)
                    {
                        jresp["error"] = new Dictionary<string, string>();
                        jresp["error"]["string"] = "Не могу получить состояние симулятора. Скорее всего не загружено сохранение";
                        SendResponce(ref response, jresp);
                        continue;
                    }
                    if ((gameState.SimGameMode != BattleTech.SimGameState.SimGameType.CAREER) && (gameState.SimGameMode != BattleTech.SimGameState.SimGameType.KAMEA_CAMPAIGN))
                    {
                        jresp["error"] = new Dictionary<string, string>();
                        jresp["error"]["string"] = "Неправильный режим компании:" + gameState.SimGameMode.ToString();
                        SendResponce(ref response, jresp);
                        continue;
                    }
                    CustomAmmoCategoriesLog.Log.LogWrite("Информация о командире ...\n");
                    jresp[gameState.Commander.Callsign] = new Dictionary<string, string>();
                    jresp[gameState.Commander.Callsign]["name"] = gameState.Commander.FirstName;
                    CustomAmmoCategoriesLog.Log.LogWrite("...получена\n");
                    BattleTech.AbilityDef DefGu5 = gameState.DataManager.AbilityDefs.Get("AbilityDefGu5");
                    BattleTech.AbilityDef DefP5 = gameState.DataManager.AbilityDefs.Get("AbilityDefP5");
                    BattleTech.AbilityDef DefP8 = gameState.DataManager.AbilityDefs.Get("AbilityDefP8");
                    BattleTech.Ability Gu = new BattleTech.Ability(DefGu5);
                    BattleTech.Ability P5 = new BattleTech.Ability(DefP5);
                    BattleTech.Ability P8 = new BattleTech.Ability(DefP8);
                    gameState.Commander.Abilities.Add(Gu);
                    gameState.Commander.Abilities.Add(P5);
                    gameState.Commander.Abilities.Add(P8);
                    gameState.Commander.PassiveAbilities.Add(Gu);
                    gameState.Commander.PassiveAbilities.Add(P5);
                    gameState.Commander.PassiveAbilities.Add(P8);
                    foreach (var ability in gameState.Commander.Abilities)
                    {
                        jresp[gameState.Commander.Callsign][ability.Def.Id] = "1";
                    }
                    foreach (var pilot in gameState.PilotRoster)
                    {
                        jresp[pilot.Callsign] = new Dictionary<string, string>();
                        jresp[pilot.Callsign]["name"] = pilot.FirstName;
                        Gu = new BattleTech.Ability(DefGu5);
                        P5 = new BattleTech.Ability(DefP5);
                        P8 = new BattleTech.Ability(DefP8);
                        pilot.Abilities.Add(Gu);
                        pilot.Abilities.Add(P5);
                        pilot.Abilities.Add(P8);
                        pilot.PassiveAbilities.Add(Gu);
                        pilot.PassiveAbilities.Add(P5);
                        pilot.PassiveAbilities.Add(P8);
                        //pilot.StatCollection.AddStatistic
                        foreach (var ability in pilot.Abilities)
                        {
                            jresp[pilot.Callsign][ability.Def.Id] = "1";
                        }
                    }
                    CustomAmmoCategoriesLog.Log.LogWrite("формирование ответа\n");
                    SendResponce(ref response, jresp);
                    continue;
                }
                if (Path.GetFileName(filename) == "listpilots")
                {
                    CustomAmmoCategoriesLog.Log.LogWrite("Запрос на список пилотов\n");
                    Dictionary<string, Dictionary<string, string>> jresp = new Dictionary<string, Dictionary<string, string>>();
                    BattleTech.GameInstance gameInstance = BattleTech.UnityGameInstance.BattleTechGame;
                    CustomAmmoCategoriesLog.Log.LogWrite("Получен gameInstance\n");
                    if (gameInstance == null)
                    {
                        jresp["error"] = new Dictionary<string, string>();
                        jresp["error"]["string"] = "Не могу получить инстанс игры";
                        SendResponce(ref response, jresp);
                        continue;
                    }
                    BattleTech.SimGameState gameState = gameInstance.Simulation;
                    CustomAmmoCategoriesLog.Log.LogWrite("Получен gameState\n");
                    if (gameState == null)
                    {
                        jresp["error"] = new Dictionary<string, string>();
                        jresp["error"]["string"] = "Не могу получить состояние симулятора. Скорее всего не загружено сохранение";
                        SendResponce(ref response, jresp);
                        continue;
                    }
                    if ((gameState.SimGameMode != BattleTech.SimGameState.SimGameType.CAREER) && (gameState.SimGameMode != BattleTech.SimGameState.SimGameType.KAMEA_CAMPAIGN))
                    {
                        jresp["error"] = new Dictionary<string, string>();
                        jresp["error"]["string"] = "Неправильный режим компании:" + gameState.SimGameMode.ToString();
                        SendResponce(ref response, jresp);
                        continue;
                    }
                    CustomAmmoCategoriesLog.Log.LogWrite("Информация о командире ...\n");
                    jresp[gameState.Commander.Callsign] = new Dictionary<string, string>();
                    jresp[gameState.Commander.Callsign]["name"] = gameState.Commander.FirstName;
                    CustomAmmoCategoriesLog.Log.LogWrite("...получена\n");

                    foreach (var ability in gameState.Commander.Abilities)
                    {
                        jresp[gameState.Commander.Callsign][ability.Def.Id] = "1";
                    }
                    foreach (var pilot in gameState.PilotRoster)
                    {
                        jresp[pilot.Callsign] = new Dictionary<string, string>();
                        jresp[pilot.Callsign]["name"] = pilot.FirstName;
                        foreach (var ability in pilot.Abilities)
                        {
                            jresp[pilot.Callsign][ability.Def.Id] = "1";
                        }
                    }
                    CustomAmmoCategoriesLog.Log.LogWrite("формирование ответа\n");
                    SendResponce(ref response, jresp);
                    continue;
                }
                CustomAmmoCategoriesLog.Log.LogWrite("Неизвестный запрос\n");
                SendError(ref response, 400, "Не могу найти " + filename);
            }
        }
    }
    public class Settings
    {
        public bool debugLog { get; set; }
        public bool modHTTPServer { get; set; }
        public string modHTTPListen { get; set; }
        Settings()
        {
            debugLog = true;
            modHTTPServer = true;
            modHTTPListen = "http://localhost:65080";
        }
    }
}

namespace CustomAmmoCategoriesInit
{
    public static partial class Core
    {
        public static void Init(string directory, string settingsJson)
        {
            //SavesForm savesForm = new SavesForm();
            CustomAmmoCategoriesLog.Log.BaseDirectory = directory;
            CustomAmmoCategoriesLog.Log.InitLog();
            string settings_filename = Path.Combine(CustomAmmoCategoriesLog.Log.BaseDirectory, "CustomAmmoCategoriesSettings.json");
            //settings_filename = Path.Combine(settings_filename, "CustomAmmoCategoriesSettings.json");
            CustomAmmoCategories.Settings = JsonConvert.DeserializeObject<CustAmmoCategories.Settings>(File.ReadAllText(settings_filename));
            CustomAmmoCategoriesLog.Log.LogWrite("Initing...\n");
            try
            {
                CustomAmmoCategories.CustomCategoriesInit();
                var harmony = HarmonyInstance.Create("io.mission.modrepuation");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                Thread thread = new Thread(ThreadWork.DoWork);
                thread.Start();
            }
            catch (Exception e)
            {
                CustomAmmoCategoriesLog.Log.LogWrite(e.ToString() + "\n");
            }
        }
    }
}
