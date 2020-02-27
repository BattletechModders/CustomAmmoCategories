using System;
using System.Collections.Generic;
using System.IO;
using BattleTech;
using CustomAmmoCategoriesLog;
using Localize;
/*
namespace CharlesB {
  public class HandleFall {
    private static List<string> phrases = new List<string>();
    private static bool fileLoaded; // for memoizing the phrases

    /// <summary>
    ///     displays a pithy floatie message over the supplied mech
    /// </summary>
    /// <param name="mech"></param>
    public static void Say(Mech mech) {
      if (!Settings.EnableKnockdownPhrases) return;
      if (!mech.IsFlaggedForKnockdown) return;

      if (!fileLoaded)
        try {
          var phraseFile = Path.Combine(Core.ModDirectory, "CharlesB_phrases.txt");
          if (!File.Exists(phraseFile)) {
            Log.CB.TWL(0, new FileNotFoundException($"Unable to locate {phraseFile}").ToString());
          }

          phrases = new List<string>();
          var reader = new StreamReader(phraseFile);
          using (reader) {
            while (!reader.EndOfStream) phrases.Add(reader.ReadLine());
          }

          fileLoaded = true;
        } catch (Exception e) {
          Log.CB.TWL(0, e.ToString());
        }

      var knockdownMessage = phrases[UnityEngine.Random.Range(0, phrases.Count - 1)];
      mech.Combat.MessageCenter.PublishMessage(new AddSequenceToStackMessage(
          new ShowActorInfoSequence(mech, new Text(knockdownMessage), FloatieMessage.MessageNature.Debuff, false))); // false leaves camera unlocked from floatie
    }
  }
}*/