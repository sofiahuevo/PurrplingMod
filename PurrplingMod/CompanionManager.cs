using System;
using System.Collections.Generic;
using PurrplingMod.StateMachine;
using StardewValley;
using StardewModdingAPI;
using PurrplingMod.Driver;

namespace PurrplingMod
{
    public class CompanionManager
    {
        public IModHelper ModHelper { get; private set; }
        public IMonitor Monitor { get; private set; }
        public DialogueDriver DialogueDriver { get; private set; }
        public HintDriver HintDriver { get; private set; }
        public Dictionary<string, CompanionStateMachine> PossibleCompanions { get; private set; }
        public Farmer Leader { get; set; }
        public CompanionManager(IModHelper helper, IMonitor monitor)
        {
            helper.Events.GameLoop.SaveLoaded += this.GameLoop_SaveLoaded;
            helper.Events.GameLoop.ReturnedToTitle += this.GameLoop_ReturnedToTitle;

            this.ModHelper = helper;
            this.Monitor = monitor;
            this.DialogueDriver = new DialogueDriver(helper);
            this.HintDriver = new HintDriver(helper);
            this.PossibleCompanions = new Dictionary<string, CompanionStateMachine>();
        }

        private void InitializeCompanions()
        {
            string[] npcNames = {
                "Abigail", "Maru", "Shane", "Leah", "Haley", "Emily",
                "Penny", "Alex", "Sam", "Sebastian", "Elliott", "Harvey"
            };

            foreach (string npcName in npcNames)
            {
                NPC companion = Game1.getCharacterFromName(npcName, true);
                if (companion == null)
                    throw new Exception($"Can't find NPC with name '{npcName}'");

                this.PossibleCompanions.Add(npcName, new CompanionStateMachine(this, companion));
            }

            this.Monitor.Log($"Initalized {this.PossibleCompanions.Count} companions.", LogLevel.Info);
        }

        private void UninitializeCompanions()
        {
            foreach (KeyValuePair<string, CompanionStateMachine> companionKv in this.PossibleCompanions)
            {
                companionKv.Value.Dispose();
                this.Monitor.Log($"{companionKv.Key} disposed!");
            }

            this.PossibleCompanions.Clear();
            this.Monitor.Log("Companions uninitialized", LogLevel.Info);
        }

        private void GameLoop_ReturnedToTitle(object sender, StardewModdingAPI.Events.ReturnedToTitleEventArgs e)
        {
            this.UninitializeCompanions();
            this.Leader = null;
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            this.Leader = Game1.player;
            this.InitializeCompanions();
        }
    }
}
