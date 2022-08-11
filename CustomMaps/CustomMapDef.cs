using HBS.Collections;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CustomMaps {
  public class CustomMapDef {
    public string Id { get; set; }
    public string FriendlyName { get; set; }
    [JsonIgnore]
    public TagSet tags { get; private set; } = new TagSet();
    public List<string> Tags {
      set {
        foreach (string tag in value) { tags.Add(tag); }
      }
    }
    public string BasedOn { get; set; } = string.Empty;
  }
}