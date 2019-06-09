using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using PurrplingMod.Loader;
using PurrplingMod.Driver;
using System.Collections.Generic;
using StardewValley.Objects;
using StardewValley;
using System.Xml;
using System.IO;
using Microsoft.Xna.Framework;

namespace PurrplingMod
{
    /// <summary>The mod entry point.</summary>
    public class PurrplingMod : Mod
    {
        private CompanionManager companionManager;
        private ContentLoader contentLoader;
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
            helper.Events.GameLoop.GameLaunched += this.GameLoop_GameLaunched;
            helper.Events.GameLoop.Saving += this.GameLoop_Saving;

            this.DialogueDriver = new DialogueDriver(helper.Events);
            this.HintDriver = new HintDriver(helper.Events);
            this.contentLoader = new ContentLoader(helper.Content, "assets", this.Monitor);
            this.companionManager = new CompanionManager(this.DialogueDriver, this.HintDriver, this.Monitor);
        }

        private void GameLoop_Saving(object sender, SavingEventArgs e)
        {
            foreach (var csm in this.companionManager.PossibleCompanions)
            {
                if (csm.Value.Bag.items.Count == 0)
                    continue;

                Vector2 chestPosition = csm.Value.DumpBag();
                this.Helper.Data.WriteSaveData($"dumpedbagtile_{csm.Key}", new Tuple<float, float>(chestPosition.X, chestPosition.Y));
            }
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            /* Preload assets to cache */
            this.Monitor.Log("Preloading assets...", LogLevel.Info);

            string[] dispositions = this.contentLoader.Load<string[]>("CompanionDispositions");

            this.contentLoader.LoadStrings("Strings/Strings");
            this.contentLoader.LoadStrings("Strings/SpeechBubbles");

            foreach (string npcName in dispositions)
            {
                this.contentLoader.LoadStrings($"Dialogue/{npcName}");
            }

            this.Monitor.Log("Assets preloaded!", LogLevel.Info);
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
            this.companionManager.InitializeCompanions(this.contentLoader, this.Helper.Events);
            foreach (var csm in this.companionManager.PossibleCompanions)
            {
                this.Monitor.Log($"{this.Helper.Data.ReadSaveData<Tuple<float, float>>($"dumpedbagtile_{csm.Key}")}");
            }
        }
    }
}