using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PurrplingMod.Loader;
using PurrplingMod.Manager;
using PurrplingMod.StateMachine.State;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace PurrplingMod.StateMachine
{
    public interface ICompanionStateMachine
    {
        NPC Companion { get; }
        ICompanionManager CompanionManager { get; }
        string Name { get; }
        Dictionary<int, ICompanionState> States { get; }
        IDialogueManager DialogueManager { get; }
        IMonitor Monitor { get; }

        event EventHandler ResolvingDialogueRequest;

        void Dispose();
        void NewDaySetup();
        void ResetStateMachine();
        void ResolveDialogueRequest();
        bool canDialogueRequestResolve();
    }

    internal class CompanionStateMachine : ICompanionStateMachine
    {
        public enum StateName
        {
            RESET,
            AVAILABLE,
            RECRUITED,
            UNAVAILABLE,
        }
        public ICompanionManager CompanionManager { get; private set; }
        public NPC Companion { get; private set; }
        public IMonitor Monitor { get; }
        public IDialogueManager DialogueManager { get; private set; }
        public Dictionary<int, ICompanionState> States { get; private set; }
        private ContentLoader.AssetsContent assets;
        private ICompanionState currentState;

        public event EventHandler ResolvingDialogueRequest;

        public CompanionStateMachine(CompanionManager manager, NPC companion, ContentLoader.AssetsContent assets, IModEvents events, IMonitor monitor = null)
        {
            this.CompanionManager = manager;
            this.Companion = companion;
            this.DialogueManager = new DialogueManager(companion);
            this.States = new Dictionary<int, ICompanionState>()
            {
                [(int)StateName.RESET] = new ResetState(this, events),
                [(int)StateName.AVAILABLE] = new AvailableState(this, events),
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

        private void ChangeState(int state)
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
            this.DialogueManager.SetupDialogues(this.assets.dialogues);
            this.ChangeState((int)StateName.AVAILABLE);
        }

        public void ResetStateMachine()
        {
            this.ChangeState((int)StateName.RESET);
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
