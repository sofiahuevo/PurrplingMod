using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Locations;
using System.Collections.Generic;
using PurrplingMod.Manager;

namespace PurrplingMod
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        private CompanionManager companionManager;
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            List<string> dispositions = helper.Content.Load<List<string>>("assets/CompanionDispositions.json");
            Dictionary<string, ContentManager.ContentAssets> assets = new Dictionary<string, ContentManager.ContentAssets>();

            foreach (string disposition in dispositions)
            {
                assets.Add(disposition, this.LoadContentAssets(disposition));
            }

            this.companionManager = new CompanionManager(helper, this.Monitor)
            {
                ContentManager = new ContentManager(assets)
            };
        }

        private ContentManager.ContentAssets LoadContentAssets(string disposition)
        {
            this.Monitor.Log($"Loading content assets for {disposition}", LogLevel.Info);
            return new ContentManager.ContentAssets()
            {
                dialogues = this.Helper.Content.Load<Dictionary<string, string>>($"assets/Dialogue/{disposition}.json"),
            };
        }
    }
}