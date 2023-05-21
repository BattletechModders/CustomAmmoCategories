using System;
using System.Globalization;
using BattleTech;
using CustomUnits;
using HBS.Collections;
using UnityEngine;

namespace MechResizer {
  static class TagUnderstander {
    public static bool ReadSizeFromTags(TagSet tags, out Vector3? size, out string rawTag) {
      size = null;
      rawTag = null;
      try {
        rawTag = FindSizeTag(tags);
        if (string.IsNullOrEmpty(rawTag)) return false;

        var parts = rawTag.Split('-');

        // for tags of style 'MR-Resize-N`
        if (parts.Length == 3) {
          var resizeNumber = float.Parse(parts[2], CultureInfo.InvariantCulture);
          size = new Vector3(resizeNumber, resizeNumber, resizeNumber);
          Log.Combat?.TWL(0, $"size from singular tag: [{size.Value.x},{size.Value.y},{size.Value.z}]");
          return true;
        }

        // for tags of style 'MR-Resize-X-Y-Z`
        if (parts.Length == 5) {
          var resizeX = float.Parse(parts[2], CultureInfo.InvariantCulture);
          var resizeY = float.Parse(parts[3], CultureInfo.InvariantCulture);
          var resizeZ = float.Parse(parts[4], CultureInfo.InvariantCulture);
          size = new Vector3(resizeX, resizeY, resizeZ);
          Log.Combat?.TWL(0, $"size from multi-tag: [{size.Value.x},{size.Value.y},{size.Value.z}]");
          return true;
        }
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        AbstractActor.logger.LogException(e);
      }

      return false;
    }

    private static string FindSizeTag(TagSet tags) {
      if (tags == null || tags.Count == 0) {
        Log.Combat?.TWL(0, "Found no tags");
        return null;
      }
      foreach (var t in tags) {
        if (!t.StartsWith("MR-Resize-", ignoreCase: true, culture: CultureInfo.InvariantCulture)) {
          Log.Combat?.WL(1," tag "+t+" is not starting from MR-Resize-");
          continue;
        }
        Log.Combat?.TWL(0, $"found a tag in for loop: {t}");
        return t;
      }

      return null;
    }
  }
}