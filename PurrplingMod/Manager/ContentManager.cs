using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurrplingMod.Manager
{
    public class ContentManager
    {
        private readonly Dictionary<string, ContentAssets> assets;

        public ContentManager(Dictionary<string, ContentAssets> assets)
        {
            this.assets = assets;
        }

        public bool GetDialogueString(string companionName, string key, out string text)
        {
            Dictionary<string, string> dialogues = this.assets[companionName].dialogues;

            if (dialogues.TryGetValue(key, out string dialogueString)) {
                text = dialogueString;
                return true;
            }

            text = companionName + "." + key;
            return false;
        }

        public string GetDialogueString(string companionName, string key)
        {
            this.GetDialogueString(companionName, key, out string text);
            return text;
        }
        public class ContentAssets
        {
            public Dictionary<string, string> dialogues;
        }
    }
}
