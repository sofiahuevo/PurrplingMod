using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
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
            helper.Events.GameLoop.DayStarted += this.GameLoop_DayStarted;

            this.DialogueDriver = new DialogueDriver(helper.Events);
            this.HintDriver = new HintDriver(helper.Events);

            ContentLoader loader = new ContentLoader(helper.Content, "assets", this.Monitor);
            loader.Load("CompanionDispositions.json");

            this.companionManager = new CompanionManager(loader.ContentAssetsMap, this.DialogueDriver, this.HintDriver, this.Monitor);
        }

        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            this.companionManager.NewDaySetup();
        }

        private void GameLoop_DayEnding(object sender, DayEndingEventArgs e)
        {
            this.companionManager.ResetStateMachines();
        }

        private void GameLoop_ReturnedToTitle(object sender, StardewModdingAPI.Events.ReturnedToTitleEventArgs e)
        {
            this.companionManager.UninitializeCompanions();
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            this.companionManager.InitializeCompanions(this.Helper.Events);
        }
    }
}