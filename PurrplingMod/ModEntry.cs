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
        private ContentManager contentManager;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.contentManager = new ContentManager(helper, this.Monitor, "assets");
            this.companionManager = new CompanionManager(helper, this.Monitor);

            this.contentManager.Load("CompanionDispositions.json");

            this.companionManager.AssetsRegistry = this.contentManager.AssetsRegistry;
        }

        
    }
}