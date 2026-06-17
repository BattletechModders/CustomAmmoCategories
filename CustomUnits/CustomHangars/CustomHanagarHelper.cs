using BattleTech;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using HBS.Extensions;
using IRBTModUtils;
using SVGImporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace CustomUnits.CustomHangars
{
    public static class CustomHangarExtensions
    {
        public static int GetFirstFreeMechBay(this SimGameState sim, MechDef mech, int? default_position = null)
        {
            Log.M?.TWL(0, "SimGameState.AddMech.GetFirstFreeMechBay_MD " + mech.Description.Id +
                " IsVehicle:" + mech.IsVehicle() + " IsInFakeDef:" + mech.Description.Id.IsInFakeDef() + " IsInFakeChassis:" + mech.ChassisID.IsInFakeChassis());
            if (mech == null) { return sim.GetFirstFreeMechBay(); }

            // Check for constraints on the hangar size
            int maxPods = CustomHangarHelper.MaxPodsConstraint(mech);
            if (maxPods == -1) { maxPods = sim.GetMaxActiveMechs(); }
            Log.M?.TWL(1, $"MaxPods: {maxPods}");

            // Check for units that fit the base hangar
            if (mech.GetHangarShift() == 0)
            {
                Thread.CurrentThread.SetFlag("GetFirstFreeMechBay_original");
                int result = default_position.HasValue ? default_position.Value : sim.GetFirstFreeMechBay();
                Thread.CurrentThread.ClearFlag("GetFirstFreeMechBay_original");

                Log.M?.TWL(1, $"baseHangar: result: {result} vs MaxPods: {maxPods}");
                // If the bay index would equal maxPods, reject it
                return result < maxPods ? result : -1;
            }
            else
            {
                // Check for units that have a custom hangar
                int maxActiveMechs = maxPods + mech.GetHangarShift();
                int minActiveMechs = mech.GetHangarShift();
                for (int key = minActiveMechs; key < maxActiveMechs; ++key)
                {
                    if (sim.ActiveMechs.ContainsKey(key)) { continue; }
                    if (sim.ReadyingMechs.ContainsKey(key)) { continue; }
                    return key;
                }
            }
            return -1;
        }

        public static int GetFirstFreeMechBay(this SimGameState sim, ChassisDef chassis, int? default_position = null)
        {
            Log.M?.TWL(0, "SimGameState.AddMech.GetFirstFreeMechBay_CD " + chassis.Description.Id +
                " IsVehicle:" + chassis.IsVehicle() + " IsInFakeChassis:" + chassis.Description.Id.IsInFakeChassis());
            if (chassis == null) { return sim.GetFirstFreeMechBay(); }

            // Check for constraints on the hangar size
            int maxPods = CustomHangarHelper.MaxPodsConstraint(chassis);
            if (maxPods == -1) { maxPods = sim.GetMaxActiveMechs(); }

            if (chassis.GetHangarShift() == 0)
            {
                Thread.CurrentThread.SetFlag("GetFirstFreeMechBay_original");
                int result = default_position.HasValue ? default_position.Value : sim.GetFirstFreeMechBay();
                Thread.CurrentThread.ClearFlag("GetFirstFreeMechBay_original");

                Log.M?.TWL(1, $"baseHangar: result: {result} vs MaxPods: {maxPods}");
                // If the bay index would equal maxPods, reject it
                return result < maxPods ? result : -1;
            }

            int maxActiveMechs = maxPods + chassis.GetHangarShift();
            int minActiveMechs = chassis.GetHangarShift();
            for (int key = minActiveMechs; key < maxActiveMechs; ++key)
            {
                if (sim.ActiveMechs.ContainsKey(key)) { continue; }
                if (sim.ReadyingMechs.ContainsKey(key)) { continue; }
                return key;
            }
            return -1;
        }
    }

    public static class CustomHangarHelper
    {
        public const string HANGAR_ID_BASE = "BASE_HANGER";

        // Keyed by def.Description.Id
        private static Dictionary<string, CustomHangarDef> hangars = new Dictionary<string, CustomHangarDef>();
        private static List<CustomHangarDef> f_list_hangars = null;
        private static Dictionary<string, CustomHangarConstraint> hangarConstraints = new Dictionary<string, CustomHangarConstraint>();
        public record CustomHangarConstraint
        {
            public int MaxAvailableUnits; // Replaces companyStats.GetValue<int>(Constants.Story.MechBayPodsID) * Constants.Story.MaxMechsPerPod;
        }

        // Extension used from Core::FinishedLoading
        public static void Register(this CustomHangarDef def)
        {
            if (hangars.ContainsKey(def.Description.Id)) { hangars[def.Description.Id] = def; } else { hangars.Add(def.Description.Id, def); }
        }

        public static CustomHangarDef HangarDef(this ChassisDef chassis)
        {
            foreach (var hangar in hangars)
            {
                if (chassis.ChassisTags.ContainsAll(hangar.Value.tags)) { return hangar.Value; }
            }
            return null;
        }

        public static List<CustomHangarDef> listHangars
        {
            get
            {
                if (f_list_hangars == null)
                {
                    f_list_hangars = hangars.Values.ToList();
                    f_list_hangars.Sort((x, y) => { return x.PositionShift - y.PositionShift; });
                }
                return f_list_hangars;
            }
        }

        // Hangar 'shift' is a positional index that determines where the boundary between hanger types lies. CU ships with vehicle bay at 100, ba bay at 200.
        public static int GetHangarShift(this MechDef mechDef)
        {
            if (mechDef.DataManager == null) { mechDef.DataManager = UnityGameInstance.BattleTechGame.DataManager; }
            if (mechDef.Chassis == null) { mechDef.Chassis = mechDef.DataManager.ChassisDefs.Get(mechDef.ChassisID); }
            ;
            if (mechDef.Chassis == null) { throw new Exception(mechDef.Description.Id + " absent chassis " + mechDef.ChassisID); }
            CustomHangarDef def = mechDef.Chassis.HangarDef();
            return def == null ? 0 : def.PositionShift;
        }

        public static int GetHangarShift(this ChassisDef chassisDef)
        {
            CustomHangarDef def = chassisDef.HangarDef();
            return def == null ? 0 : def.PositionShift;
        }
        public static int MaxPositionShift
        {
            get
            {
                return listHangars.Count == 0 ? 0 : listHangars[listHangars.Count - 1].PositionShift;
            }
        }

        public static int FallbackVehicleShift()
        {
            if (hangars.TryGetValue(CustomHangarDef.DEFAULT_VEHICLE_HANGAR_ID, out CustomHangarDef def))
            {
                return def.PositionShift;
            }
            return 0;
        }

        public static int MaxPodsConstraint(MechDef mechDef)
        {
            CustomHangarDef def = mechDef?.Chassis?.HangarDef();
            if (def == null) { return MaxPodsConstraintForDefaultHangar(); }
            bool keyExists = hangarConstraints.TryGetValue(def.Description.Id, out CustomHangarConstraint constraint);
            Log.M?.WL(0, $"Returning constraint of {constraint?.MaxAvailableUnits} for {def.Description.Id}");
            return keyExists ? constraint.MaxAvailableUnits : -1;
        }

        public static int MaxPodsConstraint(ChassisDef chassisDef)
        {
            CustomHangarDef def = chassisDef?.HangarDef();
            if (def == null) { return MaxPodsConstraintForDefaultHangar();  }
            bool keyExists = hangarConstraints.TryGetValue(def.Description.Id, out CustomHangarConstraint constraint);
            Log.M?.WL(0, $"Returning constraint of {constraint?.MaxAvailableUnits} for {def.Description.Id}");
            return keyExists ? constraint.MaxAvailableUnits : -1;
        }

        public static int MaxPodsConstraint(CustomHangarInfo hangarInfo)
        {
            if (hangarInfo == null || hangarInfo?.definition == null)
            {
                return MaxPodsConstraintForDefaultHangar();
            }

            bool keyExists = hangarConstraints.TryGetValue(hangarInfo.definition.Description.Id, out CustomHangarConstraint constraint);
            return keyExists ? constraint.MaxAvailableUnits : -1;
        }

        public static int MaxPodsConstraintForDefaultHangar()
        {
            bool keyExists = hangarConstraints.TryGetValue(HANGAR_ID_BASE, out CustomHangarConstraint constraint);
            Log.M?.WL(0, $"Returning defaultHangar constraint of {constraint?.MaxAvailableUnits}");

            return keyExists ? constraint.MaxAvailableUnits : -1;
        }

        // Invoke after CU FinishedLoading, supplying constraints for each loaded CustomHangarDef. 
        public static void SetConstraints(Dictionary<string, CustomHangarConstraint> constraints, string modSource)
        {
            Log.M?.TWL(0, $"Registering CustomHangarConstraints from mod: {modSource}");
            if (constraints == null || constraints.Keys.Count == 0)
            {
                Log.M?.WL(0, "Constraints were empty, skipping!");
            }

            Dictionary<string, CustomHangarConstraint> newConstraints = new Dictionary<string, CustomHangarConstraint>();
            foreach (KeyValuePair<string, CustomHangarConstraint> kvp in constraints)
            {
                //if (kvp.Value.MaxUnitsPerPod > 6)
                //{
                //    Log.M?.WL(0, $"Invalid constraint with MaxUnitsPerPod = {kvp.Value.MaxUnitsPerPod}, skipping.");
                //    continue;
                //}

                if (CustomHangarHelper.hangars.ContainsKey(kvp.Key))
                {
                    Log.M?.WL(0, $"Constraint {kvp.Value} applied to CustomHangarDef id: {kvp.Key}.");
                    newConstraints[kvp.Key] = kvp.Value;
                }
                else if (String.Equals(kvp.Key, CustomHangarHelper.HANGAR_ID_BASE))
                {
                    Log.M?.WL(0, $"Constraint {kvp.Value} applied for default hangar");
                    newConstraints[CustomHangarHelper.HANGAR_ID_BASE] = kvp.Value;
                }
                else
                {
                    Log.M?.WL(0, $"CustomHangarDef id: {kvp.Key} was not found, ignoring supplied constraint.");
                    continue;
                }
            }

            CustomHangarHelper.hangarConstraints = newConstraints;
        }

        // Work through the various bays and set extraneous pods to demonstrate empty values.
        //   Also update the banner text from Unavailable to something nicer
        public static void RefreshHanagarUIForConstraints(CustomBaysUICaster customBaysUICaster, CustomHangarInfo hangarInfo)
        {
            if (customBaysUICaster == null) { Log.M?.WL(0, $"CustomBaysUICaster is null, skipping."); return; }

            // Determine the max counts
            int maxPods = CustomHangarHelper.MaxPodsConstraintForDefaultHangar();
            if (hangarInfo != null) { maxPods = CustomHangarHelper.MaxPodsConstraint(hangarInfo); }
            Log.M?.WL(0, $"Applying maxPods constraint: {maxPods} to UI elements.");

            // customBaysUICaster at 'layout_tabs' level, mechBaySlots at 'layout_content' level
            // Find the sibling layout_content element
            GameObject layoutContentGO = customBaysUICaster.gameObject.transform.parent.gameObject.FindFirstChildNamed("layout_content");
            if (layoutContentGO == null) { Log.M?.WL(0, "layout_content not found, cannot continue!"); return; }

            List<MechBayRowWidget> mechBayRows = layoutContentGO.GetComponentsInChildren<MechBayRowWidget>().ToList();
            if (layoutContentGO == null) { Log.M?.WL(0, "rows not found, cannot continue!"); return; }

            int remainingPods = maxPods;
            foreach (MechBayRowWidget mbrw in mechBayRows) // Unity *should* apply correct ordering!
            {
                Log.M?.WL(0, $"dropSlot row ====");

                bool rowEnabled = remainingPods > 0;

                // Set the unavailable text
                GameObject unavailableGO = mbrw.gameObject.FindFirstChildNamed("UNAVAILABLE");
                LocalizableText unavailableText = unavailableGO.GetComponentInChildren<LocalizableText>();
                // TODO: Make this a setting / hangar value?
                unavailableText.text = Core.Settings.MechBayPods.UpgradeBannerText;

                // Iterate dropslots
                GameObject DropSlots = mbrw.gameObject.FindFirstChildNamed("DropSlots");
                // TODO: We really should pull this from sim.Constants.MaxMechsPerPod but I am struggling to think of how to get that ref from here

                foreach (Transform mbDropSlotT in DropSlots.transform)
                {
                    if (!rowEnabled)
                    {
                        mbDropSlotT.gameObject.SetActive(false);
                        Log.M?.WL(0, $"Row disabled, setting dropSlot inactive");
                        continue;
                    }
                    else
                    {
                        mbDropSlotT.gameObject.SetActive(true);
                    }

                    GameObject iconStatusGO = mbDropSlotT.gameObject.FindFirstChildNamed("icon_status_unused?");
                    SVGImage iconStatusImg = iconStatusGO.GetComponent<SVGImage>();
                    if (remainingPods <= 0)
                    {
                        // Disable
                        customBaysUICaster.SimGameState.RequestItem<SVGAsset>(
                            Core.Settings.MechBayPods.UnavailableIcon,
                            delegate (SVGAsset asset) { iconStatusImg.vectorGraphics = asset; },
                            BattleTechResourceType.SVGAsset);

                        iconStatusImg.color = Core.Settings.MechBayPods.UnavailableColor;
                        Log.M?.WL(0, $"Marked pod unavailable, remainingPods: {remainingPods}");
                    }
                    else
                    {
                        // Enable
                        customBaysUICaster.SimGameState.RequestItem<SVGAsset>(
                            Core.Settings.MechBayPods.AvailableIcon,
                            delegate (SVGAsset asset) { iconStatusImg.vectorGraphics = asset; },
                            BattleTechResourceType.SVGAsset);
                        iconStatusImg.color = Core.Settings.MechBayPods.AvailableColor;
                        Log.M?.WL(0, $"Marked pod available, remainingPods: {remainingPods}");
                    }
                    remainingPods--;
                }

            }


            // layout_content //// uixPrfPanl_SIM_mechBays-Widget-MANAGED 
            //   search for MechBayRowWidget component
            //     UNAVAILABLE as child under MBRW
            //     Representation / bg_fill / DropSlots contains all slots
            //       MechBayDragDropSlot component to find
            //       frame (1) / icon_status_unused? is the icon
            // In each layout_content 


        }
    }
     
}