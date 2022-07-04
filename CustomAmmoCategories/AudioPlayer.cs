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
using HBS;
using System;

namespace CustAmmoCategories {
  public class CustomAudioSource {
    public uint id { get; set; }
    public string name { get; set; }
    public string filename { get; set; }
    public uint play(AkGameObj sourceObject) {
      if(id > 0) {
        return SceneSingletonBehavior<WwiseManager>.Instance.PostEventById(id, sourceObject);
      } else if (string.IsNullOrEmpty(name) == false) {
        return SceneSingletonBehavior<WwiseManager>.Instance.PostEventByName(name, sourceObject);
      }
      return 0;
    }
    public void fromStr(string src) {
      id = 0;
      name = string.Empty;
      filename = string.Empty;
      string[] sp1 = src.Split(':');
      if (sp1.Length != 2) {
        throw new Exception("Wrong audio string format '" + src + "'");
      };
      if (sp1[0] == "none") {

      }else if (sp1[0] == "id") {
        id = UInt32.Parse(sp1[1]);
      } else if (sp1[0] == "enum") {
        string[] sp2 = sp1[1].Split('.');
        if (sp2.Length != 2) { throw new Exception("Wrong audio enum format '" + sp1[1] + "'"); }
        Type enumType = typeof(AudioEventList_explosion).Assembly.GetType(sp2[0], false);
        if (enumType == null) { throw new Exception("Can't find type '" + sp2[0] + "'"); }
        if (enumType.IsEnum == false) { throw new Exception(sp2[0] + " is not enum"); }
        object enumValue = null;
        foreach (var value in Enum.GetValues(enumType)) {
          if (value.ToString() == sp2[1]) { enumValue = value; break; };
        }
        if (enumValue == null) { throw new Exception("Can't find value " + sp2[1] + " in enum " + sp2[0]); };
        var EnumValueToEventId = typeof(WwiseManager).GetMethod("EnumValueToEventId").MakeGenericMethod(enumType);
        id = (uint)EnumValueToEventId.Invoke(SceneSingletonBehavior<WwiseManager>.Instance, new object[1] { enumValue });
      } else if (sp1[0] == "name") {
        name = sp1[1];
      } else if (sp1[0] == "file") {
        filename = sp1[1];
      } else {
        throw new Exception("Unknows source '" + sp1[0] + "'");
      }
    }
    public CustomAudioSource(string src) {
      fromStr(src);
    }
  }
}