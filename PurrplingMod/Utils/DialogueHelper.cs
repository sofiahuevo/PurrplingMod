using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PurrplingMod.Utils
{
    internal static class DialogueHelper
    {
        private static bool FetchDialogueString(Dictionary<string, string> dialogues, string key, out string text)
        {
            var keys = from _key in dialogues.Keys
                       where _key.StartsWith(key)
                       select _key;

            if (keys.Count() > 0)
            {
                int i = Game1.random.Next(keys.Count() + 1);

                if (i > 0 && dialogues.TryGetValue($"{key}{i}", out text))
                    return true;
            }

            if (dialogues.TryGetValue(key, out text))
                return true;

            text = key;

            return false;
        }

        public static bool GetDialogueString(NPC n, string key, out string text)
        {
            if (FetchDialogueString(n.Dialogue, key, out text))
                return true;

            text = $"{n.Name}.{text}";

            return false;
        }

        public static string GetDialogueString(NPC n, string key)
        {
            GetDialogueString(n, key, out string text);
            return text;
        }

        public static bool GetDialogueStringByLocation(NPC n, string key, GameLocation location, out string text)
        {
            return GetDialogueString(n, $"{key}_{location.Name}", out text);
        }

        public static bool GetBubbleString(Dictionary<string, string> bubbles, NPC n, GameLocation l, out string text)
        {
            if (FetchDialogueString(bubbles, $"{l.Name}_{n.Name}", out text))
            {
                text = string.Format(text, Game1.player?.Name, n.Name);

                return true;
            }

            return false;
        }

        public static Dialogue GenerateDialogueByLocation(NPC n, GameLocation l, string key)
        {
            if (GetDialogueStringByLocation(n, key, l, out string text))
                return new Dialogue(text, n);

            return null;
        }

        public static void SetupCompanionDialogues(NPC n, Dictionary<string, string> dialogues)
        {
            foreach (var pair in dialogues)
                n.Dialogue[pair.Key] = pair.Value;
        }

        public static void DrawDialogue(Dialogue dialogue)
        {
            NPC speaker = dialogue.speaker;

            speaker.CurrentDialogue.Push(dialogue);
            Game1.drawDialogue(speaker);
        }
    }
}
