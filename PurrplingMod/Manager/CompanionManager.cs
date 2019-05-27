using System;
using System.Collections.Generic;
using PurrplingMod.StateMachine;
using StardewValley;
using StardewModdingAPI;
using PurrplingMod.Driver;
using PurrplingMod.Loader;

namespace PurrplingMod.Manager
{
    public class CompanionManager
    {
        public Dictionary<string, CompanionStateMachine> PossibleCompanions { get; private set; }

        public Dictionary<string, ContentLoader.ContentAssets> AssetsRegistry { get; }

        public Farmer Leader {
            get
            {
                if (Context.IsWorldReady)
                    return Game1.player;
                return null;
            }
        }

        public CompanionManager(Dictionary<string, ContentLoader.ContentAssets> assetsRegistry)
        {
            PurrplingMod.Events.GameLoop.SaveLoaded += this.GameLoop_SaveLoaded;
            PurrplingMod.Events.GameLoop.ReturnedToTitle += this.GameLoop_ReturnedToTitle;

            this.PossibleCompanions = new Dictionary<string, CompanionStateMachine>();
            this.AssetsRegistry = assetsRegistry;
        }

        private void InitializeCompanions()
        {
            foreach (string npcName in this.AssetsRegistry.Keys)
            {
                NPC companion = Game1.getCharacterFromName(npcName, true);

                if (companion == null)
                    throw new Exception($"Can't find NPC with name '{npcName}'");

                this.PossibleCompanions.Add(npcName, new CompanionStateMachine(this, companion, this.AssetsRegistry[npcName]));
            }

            PurrplingMod.Mon.Log($"Initalized {this.PossibleCompanions.Count} companions.", LogLevel.Info);
        }

        private void UninitializeCompanions()
        {
            foreach (KeyValuePair<string, CompanionStateMachine> companionKv in this.PossibleCompanions)
            {
                companionKv.Value.Dispose();
                PurrplingMod.Mon.Log($"{companionKv.Key} disposed!");
            }

            this.PossibleCompanions.Clear();
            PurrplingMod.Mon.Log("Companions uninitialized", LogLevel.Info);
        }

        private void GameLoop_ReturnedToTitle(object sender, StardewModdingAPI.Events.ReturnedToTitleEventArgs e)
        {
            this.UninitializeCompanions();
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            this.InitializeCompanions();
        }
    }
}
