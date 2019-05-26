using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public Dictionary<string, CompanionStateMachine> PossibleCompanions { get; private set; }
        public CompanionManager(IModHelper helper, IMonitor monitor)
        {
            helper.Events.GameLoop.SaveLoaded += this.GameLoop_SaveLoaded;
            helper.Events.GameLoop.ReturnedToTitle += this.GameLoop_ReturnedToTitle;

            this.ModHelper = helper;
            this.Monitor = monitor;
            this.DialogueDriver = new DialogueDriver(helper);
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

        private void GameLoop_ReturnedToTitle(object sender, StardewModdingAPI.Events.ReturnedToTitleEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            this.InitializeCompanions();
        }
    }
}
