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
        private DialogueDriver DialogueDriver { get; set; }
        private HintDriver HintDriver { get; set; }

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.SaveLoaded += this.GameLoop_SaveLoaded;
            helper.Events.GameLoop.ReturnedToTitle += this.GameLoop_ReturnedToTitle;
            helper.Events.GameLoop.DayEnding += this.GameLoop_DayEnding;

            this.DialogueDriver = new DialogueDriver(helper.Events);
            this.HintDriver = new HintDriver(helper.Events);

            ContentLoader loader = new ContentLoader(helper.Content, "assets", this.Monitor);
            loader.Load("CompanionDispositions.json");

            this.companionManager = new CompanionManager(loader.ContentAssetsMap, this.DialogueDriver, this.HintDriver, helper.Events, this.Monitor);
        }

        private void GameLoop_DayEnding(object sender, DayEndingEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void GameLoop_ReturnedToTitle(object sender, StardewModdingAPI.Events.ReturnedToTitleEventArgs e)
        {
            this.companionManager.UninitializeCompanions();
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            this.companionManager.InitializeCompanions();
        }
    }
}