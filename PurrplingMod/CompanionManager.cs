using System;
using System.Collections.Generic;
using System.Linq;
using PurrplingMod.StateMachine;
using StardewValley;
using StardewModdingAPI;
using PurrplingMod.Driver;
using PurrplingMod.Loader;
using StardewModdingAPI.Events;
using PurrplingMod.Utils;
using PurrplingMod.StateMachine.State;
using static PurrplingMod.StateMachine.CompanionStateMachine;

namespace PurrplingMod
{
    internal class CompanionManager
    {
        private readonly DialogueDriver dialogueDriver;
        private readonly HintDriver hintDriver;
        private readonly IMonitor monitor;
        public Dictionary<string, CompanionStateMachine> PossibleCompanions { get; }

        public Farmer Farmer
        {
            get
            {
                if (Context.IsWorldReady)
                    return Game1.player;
                return null;
            }
        }

        public CompanionManager(DialogueDriver dialogueDriver, HintDriver hintDriver, IMonitor monitor)
        {
            this.dialogueDriver = dialogueDriver ?? throw new ArgumentNullException(nameof(dialogueDriver));
            this.hintDriver = hintDriver ?? throw new ArgumentNullException(nameof(hintDriver));
            this.monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
            this.PossibleCompanions = new Dictionary<string, CompanionStateMachine>();

            this.dialogueDriver.DialogueRequested += this.DialogueDriver_DialogueRequested;
            this.dialogueDriver.DialogueChanged += this.DialogueDriver_DialogueChanged;
            this.hintDriver.CheckHint += this.HintDriver_CheckHint;
        }

        private void DialogueDriver_DialogueChanged(object sender, DialogueChangedArgs e)
        {
            NPC n = e.PreviousDialogue?.speaker;

            if (e.PreviousDialogue == null || n == null)
                return;

            if (this.PossibleCompanions.TryGetValue(n.Name, out CompanionStateMachine csm))
            {
                csm.DialogueSpeaked(e.PreviousDialogue);
            }
        }

        private void HintDriver_CheckHint(object sender, CheckHintArgs e)
        {
            if (e.Npc == null)
                return;

            if (this.PossibleCompanions.TryGetValue(e.Npc.Name, out CompanionStateMachine csm)
                && csm.Name == e.Npc?.Name
                && csm.CanDialogueRequestResolve()
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

        internal void CompanionRecuited(string companionName)
        {
            foreach (var csmKv in this.PossibleCompanions)
            {
                if (csmKv.Value.Name != companionName)
                    csmKv.Value.MakeUnavailable();
            }
        }

        public void ResetStateMachines()
        {
            foreach (var companionKv in this.PossibleCompanions)
                companionKv.Value.ResetStateMachine();
        }

        public void NewDaySetup()
        {
            try
            {
                foreach (var companionKv in this.PossibleCompanions)
                    companionKv.Value.NewDaySetup();
            }
            catch (InvalidStateException e)
            {
                this.monitor.Log($"Error while trying to setup new day: {e.Message}");
                this.monitor.ExitGameImmediately(e.Message);
            }
        }

        internal void CompanionDissmised(bool keepUnavailable = false)
        {
            foreach (var csmKv in this.PossibleCompanions)
            {
                if (keepUnavailable)
                    csmKv.Value.MakeUnavailable();
                else if (!csmKv.Value.RecruitedToday)
                    csmKv.Value.MakeAvailable();
            }
        }

        public void InitializeCompanions(IContentLoader loader, IModEvents gameEvents)
        {
            string[] dispositions = loader.Load<string[]>("CompanionDispositions");

            foreach (string npcName in dispositions)
            {
                NPC companion = Game1.getCharacterFromName(npcName, true);

                if (companion == null)
                    throw new Exception($"Can't find NPC with name '{npcName}'");

                CompanionStateMachine csm = new CompanionStateMachine(this, companion, loader, this.monitor);
                Dictionary<StateFlag, ICompanionState> stateHandlers = new Dictionary<StateFlag, ICompanionState>()
                {
                    [StateFlag.RESET] = new ResetState(csm, gameEvents),
                    [StateFlag.AVAILABLE] = new AvailableState(csm, gameEvents),
                    [StateFlag.RECRUITED] = new RecruitedState(csm, gameEvents),
                    [StateFlag.UNAVAILABLE] = new UnavailableState(csm, gameEvents),
                };

                csm.Setup(stateHandlers);
                this.PossibleCompanions.Add(npcName, csm);
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
