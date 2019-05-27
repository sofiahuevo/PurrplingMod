using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Locations;
using System.Collections.Generic;
using PurrplingMod.Manager;
using PurrplingMod.Loader;
using PurrplingMod.Driver;

namespace PurrplingMod
{
    /// <summary>The mod entry point.</summary>
    public class PurrplingMod : Mod
    {
        private CompanionManager companionManager;
        public static DialogueDriver DialogueDriver { get; private set; }
        public static HintDriver HintDriver { get; private set; }
        public static IMonitor Mon { get; private set; }
        public static IModEvents Events { get; private set; }

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            PurrplingMod.Events = helper.Events;
            PurrplingMod.Mon = this.Monitor;

            DialogueDriver = new DialogueDriver();
            HintDriver = new HintDriver();

            ContentLoader loader = new ContentLoader(helper.Content, "assets");
            loader.Load("CompanionDispositions.json");

            this.companionManager = new CompanionManager(loader.ContentAssetsMap);
        }
    }
}