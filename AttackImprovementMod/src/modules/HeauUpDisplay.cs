using BattleTech.UI;
using BattleTech;
using Localize;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Sheepy.BattleTechMod.AttackImprovementMod {
  using static Mod;
  using static System.Reflection.BindingFlags;

  public class HeauUpDisplay : BattleModModule {

    private static HeauUpDisplay instance;

    private static Color?[] NameplateColours;
    private static Color?[] FloatingArmorColours;

    public override void ModStarts() {
      instance = this;
      NameplateColours = ParseColours(AIMSettings.NameplateColourPlayer, AIMSettings.NameplateColourEnemy, AIMSettings.NameplateColourAlly);
      if (AIMSettings.ShowEnemyWounds != null || AIMSettings.ShowAllyHealth != null || AIMSettings.ShowPlayerHealth != null) {
        Patch(typeof(CombatHUDActorNameDisplay), "RefreshInfo", typeof(VisibilityLevel), null, "ShowPilotWounds");
        Patch(typeof(Pilot), "InjurePilot", null, "RefreshPilotNames");
      }
      if (NameplateColours != null) {
        Info("Nameplate colours: {0}", NameplateColours);
        Patch(typeof(CombatHUDNumFlagHex), "OnActorChanged", null, "SetNameplateColor");
      }
      FloatingArmorColours = ParseColours(AIMSettings.FloatingArmorColourPlayer, AIMSettings.FloatingArmorColourEnemy, AIMSettings.FloatingArmorColourAlly);
      if (FloatingArmorColours != null) {
        Info("Armor colours: {0}", FloatingArmorColours);
        BarOwners = new Dictionary<CombatHUDPipBar, ICombatant>();
        Patch(typeof(CombatHUDPipBar), "ShowValue", new Type[] { typeof(float), typeof(Color), typeof(Color), typeof(Color), typeof(bool) }, "SetArmorBarColour", null);
        Patch(typeof(CombatHUDNumFlagHex), "OnActorChanged", "SetArmorBarOwner", null);
      }

      if (AIMSettings.ShowMeleeTerrain) { // Considered transpiler but we'll only save one method call. Not worth trouble?
        Patch(typeof(CombatMovementReticle), "DrawPath", null, "ShowMeleeTerrainText");
        Patch(typeof(CombatMovementReticle), "drawJumpPath", null, "ShowDFATerrainText");
      }
      if (AIMSettings.SpecialTerrainDotSize != 1 || AIMSettings.NormalTerrainDotSize != 1)
        Patch(typeof(MovementDotMgr.MovementDot).GetConstructors()[0], null, "ScaleMovementDot");
      if (AIMSettings.BoostTerrainDotColor)
        Patch(typeof(CombatMovementReticle), "Awake", null, "ColourMovementDot");
    }

    public override void CombatStarts() {
      if (AIMSettings.MovementPreviewRadius > 0) {
        MovementConstants con = CombatConstants.MoveConstants;
        con.ExperimentalHexRadius = AIMSettings.MovementPreviewRadius;
        CombatConstants.MoveConstants = con;
      }
      if (AIMSettings.FunctionKeySelectPC)
        Combat.MessageCenter.AddSubscriber(MessageCenterMessageType.KeyPressedMessage, KeyPressed);
      if (AIMSettings.ConsolidateWeaponCheevons) {
        // If we want to consolidate weapon damage, need to overwrite CombatHUDWeaponTickMarks.UpdateTicksShown to not depends on GetValidSlots
        CombatUIConstantsDef uiConst = CombatConstants.CombatUIConstants;
        uiConst.collapseWeaponTypesInTickMarks = true;
        CombatConstants.CombatUIConstants = uiConst;
      }
    }

    public override void CombatEnds() {
      BarOwners?.Clear();
      if (AIMSettings.FunctionKeySelectPC)
        Combat.MessageCenter.RemoveSubscriber(MessageCenterMessageType.KeyPressedMessage, KeyPressed);
    }

    // ============ Keyboard Input ============

    public static void KeyPressed(MessageCenterMessage message) {
      try {
        if (Combat == null) return;
        string key = (message as KeyPressedMessage)?.KeyCode;
        switch (key) {
          case "F1": SelectPC(0, InControl.Key.F1); break;
          case "F2": SelectPC(1, InControl.Key.F2); break;
          case "F3": SelectPC(2, InControl.Key.F3); break;
          case "F4": SelectPC(3, InControl.Key.F4); break;
        }
      } catch (Exception ex) { Error(ex); }
    }

    private static void SelectPC(int index, InControl.Key key) {
      if (BTInput.Instance?.FindActionBoundto(new InControl.KeyBindingSource(key)) != null) return;
      List<AbstractActor> units = Combat?.LocalPlayerTeam?.units;
      if (units == null || index >= units.Count || units[index] == null) return;
      HUD.MechWarriorTray.FindPortraitForActor(units[index].GUID).OnClicked();
    }

    // ============ Floating Nameplate ============

    public static bool IsCallout { get; private set; }
    private static List<Action<bool>> CalloutListener;

    public static void HookCalloutToggle(Action<bool> listener) {
      if (CalloutListener == null) {
        instance.Patch(typeof(CombatSelectionHandler), "ProcessInput", null, "ToggleCallout");
        CalloutListener = new List<Action<bool>>();
      }
      if (!CalloutListener.Contains(listener))
        CalloutListener.Add(listener);
    }

    public static void ToggleCallout() {
      try {
        if (IsCallout != IsCalloutPressed) {
          IsCallout = IsCalloutPressed;
          foreach (Action<bool> listener in CalloutListener)
            listener(IsCallout);
        }
      } catch (Exception ex) { Error(ex); }
    }

    // ============ Floating Nameplate ============

    public static void ShowPilotWounds(CombatHUDActorNameDisplay __instance, VisibilityLevel visLevel) {
      try {
        AbstractActor actor = __instance.DisplayedCombatant as AbstractActor;
        Pilot pilot = actor?.GetPilot();
        Team team = actor?.team;
        TMPro.TextMeshProUGUI textbox = __instance.PilotNameText;
        if (AnyNull<object>(pilot, team, textbox, Combat) || pilot.Injuries <= 0) return;
        string format = null;
        object[] args = new object[] { null, pilot.Injuries, pilot.Health - pilot.Injuries, pilot.Health };
        if (team == Combat.LocalPlayerTeam) {
          format = AIMSettings.ShowPlayerHealth;
        } else if (team.IsFriendly(Combat.LocalPlayerTeam)) {
          format = AIMSettings.ShowAllyHealth;
        } else if (visLevel == VisibilityLevel.LOSFull) {
          format = AIMSettings.ShowEnemyWounds;
          args[2] = args[3] = "?";
        }
        if (format != null)
          textbox.text = textbox.text + "</uppercase><size=80%>" + Translate(format, args);
      } catch (Exception ex) { Error(ex); }
    }

    public static void RefreshPilotNames(Pilot __instance) {
      try {
        if (__instance.IsIncapacitated) return;
        AbstractActor actor = Combat.AllActors.First(e => e.GetPilot() == __instance);
        if (actor == null) return;
        HUD.InWorldMgr.GetNumFlagForCombatant(actor)?.ActorInfo?.NameDisplay?.RefreshInfo();
        if (HUD.SelectedTarget == actor)
          HUD.TargetingComputer.ActorInfo.NameDisplay.RefreshInfo();
      } catch (Exception ex) { Error(ex); }
    }

    // Colours are Player, Enemy, and Ally
    private static Color? GetTeamColour(ICombatant owner, Color?[] Colours) {
      Team team = owner?.team;
      if (team == null || owner.IsDead || Colours == null || Colours.Length < 3) return null;

      if (team.IsLocalPlayer) return Colours[0];
      if (team.IsEnemy(BattleTechGame?.Combat?.LocalPlayerTeam)) return Colours[1];
      if (team.IsFriendly(BattleTechGame?.Combat?.LocalPlayerTeam)) return Colours[2];
      return null;
    }

    public static void SetNameplateColor(CombatHUDNumFlagHex __instance) {
      try {
        Color? colour = GetTeamColour(__instance.DisplayedCombatant, NameplateColours);
        if (!colour.HasValue) return;
        CombatHUDActorNameDisplay names = __instance.ActorInfo?.NameDisplay;
        if (names == null) return;
        names.PilotNameText.faceColor = colour.Value;
        if (colour != Color.black) {
          names.MechNameText.outlineWidth = 0.2f;
          names.MechNameText.outlineColor = Color.black;
        }
        names.MechNameText.faceColor = colour.Value;
      } catch (Exception ex) { Error(ex); }
    }

    private static Dictionary<CombatHUDPipBar, ICombatant> BarOwners;

    public static void SetArmorBarOwner(CombatHUDNumFlagHex __instance) {
      try {
        ICombatant owner = __instance.DisplayedCombatant;
        CombatHUDArmorBarPips bar = __instance.ActorInfo?.ArmorBar;
        if (bar == null) return;
        if (owner != null) {
          BarOwners[bar] = owner;
          bar.RefreshUIColors();
        } else if (BarOwners.ContainsKey(bar))
          BarOwners.Remove(bar);
      } catch (Exception ex) { Error(ex); }
    }

    public static void SetArmorBarColour(CombatHUDPipBar __instance, ref Color shownColor) {
      if (!(__instance is CombatHUDArmorBarPips me) || !BarOwners.TryGetValue(__instance, out ICombatant owner)) return;
      Color? colour = GetTeamColour(owner, FloatingArmorColours);
      if (colour.HasValue)
        shownColor = colour.Value;
    }


    public static void ShowMeleeTerrainText(CombatMovementReticle __instance, AbstractActor actor, bool isMelee) {
      if (isMelee) ShowTerrainText(__instance, actor, "Melee");
    }

    public static void ShowDFATerrainText(CombatMovementReticle __instance, AbstractActor actor, bool isMelee) {
      if (isMelee) ShowTerrainText(__instance, actor, "DFA");
    }

    public static void ShowTerrainText(CombatMovementReticle __instance, AbstractActor actor, string action) {
      try {
        Pathing pathing = actor.Pathing;
        if (pathing == null || pathing.CurrentPath.IsNullOrEmpty()) return;
        __instance.UpdateStatusPreview(actor, pathing.ResultDestination + actor.HighestLOSPosition, pathing.MoveType);
        __instance.StatusPreview.MoveTypeText.text = Translate(action);
      } catch (Exception ex) { Error(ex); }
    }

    public static void ScaleMovementDot(MovementDotMgr.MovementDot __instance, MovementDotMgr.DotType type) {
      try {
        float size = (float)(type == MovementDotMgr.DotType.Normal ? AIMSettings.NormalTerrainDotSize : AIMSettings.SpecialTerrainDotSize);
        if (size == 1) return;
        Vector3 scale = __instance.dotObject.transform.localScale;
        scale.x *= size;
        scale.y *= size;
        __instance.dotObject.transform.localScale = scale;
      } catch (Exception ex) { Error(ex); }
    }

    public static void ColourMovementDot(CombatMovementReticle __instance) {
      try {
        BrightenGameObject(__instance.forestDotTemplate);
        BrightenGameObject(__instance.waterDotTemplate);
        BrightenGameObject(__instance.roughDotTemplate);
        BrightenGameObject(__instance.roadDotTemplate);
        BrightenGameObject(__instance.specialDotTemplate);
        BrightenGameObject(__instance.dangerousDotTemplate);
      } catch (Exception ex) { Error(ex); }
    }

    private static void BrightenGameObject(GameObject obj) {
      MeshRenderer mesh = TryGet(obj?.GetComponents<MeshRenderer>(), 0, null, "MovementDot MeshRenderer");
      if (mesh == null) return;
      Color.RGBToHSV(mesh.sharedMaterial.color, out float H, out float S, out float V);
      mesh.sharedMaterial.color = Color.HSVToRGB(H, 1, 1);
    }
  }
}