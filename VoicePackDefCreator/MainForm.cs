using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VoicePackDefCreator {
  public partial class MainForm : Form {
    public MainForm() {
      InitializeComponent();
    }

    private void button_Click(object sender, EventArgs e) {
      if(openFileDialog.ShowDialog() == DialogResult.OK) {
        string[] lines = File.ReadAllLines(openFileDialog.FileName);
        if(lines[0].Contains("Event\tID\tName") == false) {
          MessageBox.Show("Wrong file format");
          return;
        }
        Regex regex = new Regex("[\\t]([0-9]+)[\\t]([a-zA-Z0-9\\\\_\\\\-]+)[\\t].*");
        Dictionary<string, uint> events = new Dictionary<string, uint>();
        Dictionary<string, AudioSwitch_dialog_lines_pilots> enums = new Dictionary<string, AudioSwitch_dialog_lines_pilots>();
        foreach(AudioSwitch_dialog_lines_pilots val in Enum.GetValues(typeof(AudioSwitch_dialog_lines_pilots))) {
          enums.Add(val.ToString(), val);
        }
        string stopall_event_name = string.Empty;
        Dictionary<AudioSwitch_dialog_lines_pilots, List<string>> voicdef_light = new Dictionary<AudioSwitch_dialog_lines_pilots, List<string>>();
        Dictionary<AudioSwitch_dialog_lines_pilots, List<string>> voicdef_dark = new Dictionary<AudioSwitch_dialog_lines_pilots, List<string>>();
        for (int t = 1; t < lines.Length; ++t) {
          string line = lines[t];
          if (line.Contains("Audio source file")) { break; }
          Match match = regex.Match(line);
          if(match.Groups.Count >= 3) {
            string eventname = match.Groups[2].Value;
            uint eventid = uint.Parse(match.Groups[1].Value);
            if (events.ContainsKey(eventname) == false) { events.Add(eventname,eventid); };
            if (eventname.Contains("stop_all")) { stopall_event_name = eventname; }
            foreach(var en in enums) {
              if (eventname.Contains(en.Key)) {
                Dictionary<AudioSwitch_dialog_lines_pilots, List<string>> voicdef = voicdef_light;
                if (eventname.Contains("_sad") || eventname.Contains("_dark")) { voicdef = voicdef_dark; };
                if (voicdef.ContainsKey(en.Value) == false) { voicdef.Add(en.Value,new List<string>()); }
                voicdef[en.Value].Add(eventname);
              }
            }
          }
        }
        SoundBankDef sbdef = new SoundBankDef();
        sbdef.filename = Path.GetFileNameWithoutExtension(Path.GetFileName(openFileDialog.FileName))+".bnk";
        sbdef.name = Path.GetFileNameWithoutExtension(openFileDialog.FileName)+"_soundbank";
        sbdef.events = events;
        VoicePackDef vpdef = new VoicePackDef();
        vpdef.name = Path.GetFileNameWithoutExtension(openFileDialog.FileName) + "_voicepack";
        vpdef.vobank = sbdef.name;
        vpdef.light_phrases = voicdef_light;
        vpdef.dark_phrases = voicdef_dark;
        vpdef.stop_event = stopall_event_name;
        File.WriteAllText(Path.Combine(Path.GetDirectoryName(openFileDialog.FileName),sbdef.name+".json"),JsonConvert.SerializeObject(sbdef,Formatting.Indented));
        File.WriteAllText(Path.Combine(Path.GetDirectoryName(openFileDialog.FileName), vpdef.name + ".json"), JsonConvert.SerializeObject(vpdef, Formatting.Indented));
        MessageBox.Show("Done");
      }
    }
  }
  public class VoicePackDef {
    public string name { get; set; }
    public string vobank { get; set; }
    public string baseVoice { get; set; }
    public string stop_event { get; set; }
    public string gender { get; set; }
    public Dictionary<AudioSwitch_dialog_lines_pilots, List<string>> dark_phrases { get; set; }
    public Dictionary<AudioSwitch_dialog_lines_pilots, List<string>> light_phrases { get; set; }
    public VoicePackDef() {
      dark_phrases = new Dictionary<AudioSwitch_dialog_lines_pilots, List<string>>();
      light_phrases = new Dictionary<AudioSwitch_dialog_lines_pilots, List<string>>();
      vobank = string.Empty;
      gender = "Male";
      baseVoice = "m_bear01";
    }
  }
  public class SoundBankDef {
    public string name { get; set; }
    public string filename { get; set; }
    public string type { get; set; }
    public Dictionary<string, uint> events { get; set; }
    public SoundBankDef() { events = new Dictionary<string, uint>(); type = "Voice"; }
  }
  public enum AudioSwitch_dialog_lines_pilots {
    target_alpha = 30, // 0x0000001E
    acquired_object = 41, // 0x00000029
    acquired_vip = 42, // 0x0000002A
    airstrike_them = 43, // 0x0000002B
    airstrike_us = 44, // 0x0000002C
    ammo_gone_ac10 = 45, // 0x0000002D
    ammo_gone_ac2 = 46, // 0x0000002E
    ammo_gone_ac20 = 47, // 0x0000002F
    ammo_gone_ac5 = 48, // 0x00000030
    ammo_gone_guass = 49, // 0x00000031
    ammo_gone_lrm = 50, // 0x00000032
    ammo_gone_mg = 51, // 0x00000033
    ammo_gone_multiple = 52, // 0x00000034
    ammo_gone_srm = 53, // 0x00000035
    artillery_them = 54, // 0x00000036
    artillery_us = 55, // 0x00000037
    building_destroyed = 56, // 0x00000038
    chatter = 57, // 0x00000039
    chatter_arid = 58, // 0x0000003A
    chatter_frozen = 59, // 0x0000003B
    chatter_jungle = 60, // 0x0000003C
    chatter_martian = 61, // 0x0000003D
    chatter_urban = 62, // 0x0000003E
    chatter_verdant = 63, // 0x0000003F
    chosen = 64, // 0x00000040
    done = 65, // 0x00000041
    drone_deployed = 66, // 0x00000042
    enemy_critical = 67, // 0x00000043
    enemy_kill = 68, // 0x00000044
    enemy_vehicle_kill = 70, // 0x00000046
    friendly_destroyed = 71, // 0x00000047
    friendly_killed = 72, // 0x00000048
    generic = 73, // 0x00000049
    hit_barely = 74, // 0x0000004A
    hit_critical = 75, // 0x0000004B
    hit_hard = 76, // 0x0000004C
    hit_internal = 77, // 0x0000004D
    jump = 78, // 0x0000004E
    mech_down = 79, // 0x0000004F
    mech_limping = 80, // 0x00000050
    mech_up = 81, // 0x00000051
    move = 82, // 0x00000052
    overheat_shutdown = 83, // 0x00000053
    overheat_warning = 84, // 0x00000054
    pilot_death = 85, // 0x00000055
    pilot_demoralized = 86, // 0x00000056
    pilot_hit = 87, // 0x00000057
    pilot_inspired = 88, // 0x00000058
    pilot_wakes = 89, // 0x00000059
    pvp_start = 90, // 0x0000005A
    reserve = 91, // 0x0000005B
    retreat = 92, // 0x0000005C
    sprint = 93, // 0x0000005D
    support_used = 94, // 0x0000005E
    target_dfa = 95, // 0x0000005F
    target_fire = 96, // 0x00000060
    target_melee = 97, // 0x00000061
    target_missed = 98, // 0x00000062
    target_multi = 99, // 0x00000063
    target_rear = 100, // 0x00000064
    warning_flank = 101, // 0x00000065
    weapon_lost = 102, // 0x00000066
    weapon_lost_all = 103, // 0x00000067
    enemy_presence_first = 104, // 0x00000068
    enemy_presence_more = 105, // 0x00000069
    mech_powerup = 106, // 0x0000006A
    turret_destroyed = 107, // 0x0000006B
    target_structure = 108, // 0x0000006C
    ammo_gone_flamer = 109, // 0x0000006D
    ammo_gone_generic = 110, // 0x0000006E
    pilot_ejecting = 111, // 0x0000006F
    sensor_lock_onthem = 112, // 0x00000070
    sensor_lock_onus = 113, // 0x00000071
    pilot_hurt = 114 // 0x00000072
  }

}
