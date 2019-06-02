using StardewValley;
using System.Collections.Generic;

namespace PurrplingMod.Utils
{
    internal static class DialogueHelper
    {
        public static bool GetDialogueString(NPC n, string key, out string text)
        {
            Dictionary<string, string> dialogues = n.Dialogue;

            if (dialogues.TryGetValue(key, out string dialogueString))
            {
                text = dialogueString;
                return true;
            }

            text = n.Name + "." + key;
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

        public static void SetupDialogues(NPC n, Dictionary<string, string> dialogues)
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
