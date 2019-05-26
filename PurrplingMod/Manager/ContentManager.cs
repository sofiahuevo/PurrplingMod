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

        public string GetDialogueString(string companionName, string key)
        {
            Dictionary<string, string> dialogues = this.assets[companionName].dialogues;

            if (dialogues.TryGetValue(key, out string dialogueString))
                return dialogueString;

            return companionName + "." + key;
        }
        public class ContentAssets
        {
            public Dictionary<string, string> dialogues;
        }
    }
}
