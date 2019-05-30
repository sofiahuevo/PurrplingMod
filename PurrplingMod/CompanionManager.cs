using System;
using System.Collections.Generic;
using PurrplingMod.StateMachine;
using StardewValley;
using StardewModdingAPI;
using PurrplingMod.Driver;
using PurrplingMod.Loader;
using StardewModdingAPI.Events;
using PurrplingMod.Utils;

namespace PurrplingMod
{
    internal class CompanionManager
    {
        private readonly DialogueDriver dialogueDriver;
        private readonly HintDriver hintDriver;
        private readonly IMonitor monitor;
        private readonly IModEvents events;
        private Dictionary<string, CompanionStateMachine> PossibleCompanions { get; set; }
        private Dictionary<string, ContentLoader.AssetsContent> AssetsRegistry { get; }

        public Farmer Farmer
        {
            get
            {
                if (Context.IsWorldReady)
                    return Game1.player;
                return null;
            }
        }

        public CompanionManager(Dictionary<string, ContentLoader.AssetsContent> assetsRegistry,
                                DialogueDriver dialogueDriver,
                                HintDriver hintDriver,
                                IModEvents events,
                                IMonitor monitor)
        {
            this.dialogueDriver = dialogueDriver ?? throw new ArgumentNullException(nameof(dialogueDriver));
            this.hintDriver = hintDriver ?? throw new ArgumentNullException(nameof(hintDriver));
            this.monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
            this.events = events;
            this.PossibleCompanions = new Dictionary<string, CompanionStateMachine>();
            this.AssetsRegistry = assetsRegistry;

            this.dialogueDriver.DialogueRequested += this.DialogueDriver_DialogueRequested;
            this.hintDriver.CheckHint += this.HintDriver_CheckHint;
        }

        private void HintDriver_CheckHint(object sender, CheckHintArgs e)
        {
            if (e.Npc == null)
                return;

            if (this.PossibleCompanions.TryGetValue(e.Npc.Name, out CompanionStateMachine csm)
                && csm.Name == e.Npc?.Name
                && csm.canDialogueRequestResolve()
                && e.Npc.CurrentDialogue.Count == 0
                && Helper.CanRequestDialog(this.Farmer, e.Npc))
            {
                this.hintDriver.ShowHint(HintDriver.Hint.DIALOGUE);
            }
        }

        private void DialogueDriver_DialogueRequested(object sender, DialogueRequestArgs e)
        {
            if (this.PossibleCompanions.TryGetValue(e.WithWhom.Name, out CompanionStateMachine csm) && csm.Name == e.WithWhom.Name)
            {
                csm.ResolveDialogueRequest();
            }
        }

        public void ResetStateMachines()
        {
            foreach (var companionKv in this.PossibleCompanions)
                companionKv.Value.ResetStateMachine();
        }

        public void InitializeCompanions()
        {
            foreach (string npcName in this.AssetsRegistry.Keys)
            {
                NPC companion = Game1.getCharacterFromName(npcName, true);

                if (companion == null)
                    throw new Exception($"Can't find NPC with name '{npcName}'");

                this.PossibleCompanions.Add(npcName, new CompanionStateMachine(this, companion, this.AssetsRegistry[npcName], this.events, this.monitor));
            }

            this.monitor.Log($"Initalized {this.PossibleCompanions.Count} companions.", LogLevel.Info);
        }

        public void UninitializeCompanions()
        {
            foreach (var companionKv in this.PossibleCompanions)
            {
                companionKv.Value.Dispose();
                this.monitor.Log($"{companionKv.Key} disposed!");
            }

            this.PossibleCompanions.Clear();
            this.monitor.Log("Companions uninitialized", LogLevel.Info);
        }
    }
}
