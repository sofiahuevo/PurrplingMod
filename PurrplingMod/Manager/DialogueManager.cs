using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurrplingMod.Manager
{
    class DialogueManager
    {
        private NPC companion;

        public DialogueManager(NPC companion)
        {
            this.companion = companion;
        }

        public bool GetDialogueString(string key, out string text)
        {
            Dictionary<string, string> dialogues = this.companion.Dialogue;

            if (dialogues.TryGetValue(key, out string dialogueString))
            {
                text = dialogueString;
                return true;
            }

            text = this.companion.Name + "." + key;
            return false;
        }

        public string GetDialogueString(string key)
        {
            this.GetDialogueString(key, out string text);
            return text;
        }

        public void SetupDialogues(Dictionary<string, string> dialogues)
        {
            foreach (var pair in dialogues)
                this.companion.Dialogue[pair.Key] = pair.Value;
        }
    }
}
