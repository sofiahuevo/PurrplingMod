using System;
using System.Collections.Generic;
using PurrplingMod.Loader;
using PurrplingMod.StateMachine.State;
using PurrplingMod.Utils;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace PurrplingMod.StateMachine
{

    internal class CompanionStateMachine
    {
        public enum StateFlag
        {
            RESET,
            AVAILABLE,
            RECRUITED,
            UNAVAILABLE,
        }
        public CompanionManager CompanionManager { get; private set; }
        public NPC Companion { get; private set; }
        public IMonitor Monitor { get; }
        public Dictionary<StateFlag, ICompanionState> States { get; private set; }
        private ContentLoader.AssetsContent assets;
        private ICompanionState currentState;

        public event EventHandler ResolvingDialogueRequest;

        public CompanionStateMachine(CompanionManager manager, NPC companion, ContentLoader.AssetsContent assets, IModEvents events, IMonitor monitor = null)
        {
            this.CompanionManager = manager;
            this.Companion = companion;
            this.States = new Dictionary<StateFlag, ICompanionState>()
            {
                [StateFlag.RESET] = new ResetState(this, events),
                [StateFlag.AVAILABLE] = new AvailableState(this, events),
                [StateFlag.RECRUITED] = new RecruitedState(this, events),
                [StateFlag.UNAVAILABLE] = new UnavailableState(this, events),
            };
            this.assets = assets;
            this.Monitor = monitor;
            this.ResetStateMachine();
        }

        public string Name
        {
            get
            {
                return this.Companion.Name;
            }
        }

        private void ChangeState(StateFlag state)
        {
            if (!this.States.TryGetValue(state, out ICompanionState newState))
                throw new Exception($"Unknown state: {state}");

            if (this.currentState == newState)
                return;

            if (this.currentState != null)
            {
                this.currentState.Exit();
            }

            newState.Entry();
            this.currentState = newState;
        }

        public void NewDaySetup()
        {
            DialogueHelper.SetupDialogues(this.Companion, this.assets.dialogues);
            this.MakeAvailable();
        }

        public void MakeAvailable()
        {
            this.ChangeState(StateFlag.AVAILABLE);
        }

        public void MakeUnavailable()
        {
            this.ChangeState(StateFlag.UNAVAILABLE);
        }

        public void ResetStateMachine()
        {
            this.ChangeState(StateFlag.RESET);
        }

        public void Recruit()
        {
            this.ChangeState(StateFlag.RECRUITED);
            this.CompanionManager.CompanionRecuited(this.Companion.Name);
        }

        public void Dispose()
        {
            if (this.currentState != null)
                this.currentState.Exit();

            this.States.Clear();
            this.States = null;
            this.currentState = null;
            this.Companion = null;
            this.CompanionManager = null;
        }

        public void ResolveDialogueRequest()
        {
            this.ResolvingDialogueRequest?.Invoke(this, new EventArgs());
        }

        public bool canDialogueRequestResolve()
        {
            return this.ResolvingDialogueRequest != null;
        }
    }
}
