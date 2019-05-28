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

/// <summary> Mod entry and static service container
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

        /// <summary>The mod entry point, called after the mod is first loaded. Initalizes services</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            // Propagate game events and Monitor to static fields (must be set first!)
            PurrplingMod.Events = helper.Events;
            PurrplingMod.Mon = this.Monitor;

            // Initalize and propagate drivers. They accessing to PurrplingMod static fields
            PurrplingMod.DialogueDriver = new DialogueDriver();
            PurrplingMod.HintDriver = new HintDriver();

            ContentLoader loader = new ContentLoader(helper.Content, "assets");
            loader.Load("CompanionDispositions.json");

            // Companion manager subscribe game events automatically (accessing to PurrplingMod static fields)
            this.companionManager = new CompanionManager(loader.ContentAssetsMap);
        }
    }
}
