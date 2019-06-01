using System;
using System.Collections.Generic;
using PurrplingMod.Internal;
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

        public StateFlag CurrentStateFlag { get; private set; }
        public Dictionary<int, SchedulePathDescription> BackedupSchedule { get; internal set; }
        public bool RecruitedToday { get; private set; }

        private void ChangeState(StateFlag stateFlag)
        {
            if (!this.States.TryGetValue(stateFlag, out ICompanionState newState))
                throw new InvalidStateException($"Unknown state: {stateFlag}");

            if (this.currentState == newState)
                return;

            if (this.currentState != null)
            {
                this.currentState.Exit();
            }

            newState.Entry();
            this.currentState = newState;
            this.Monitor.Log($"{this.Name} changed state: {this.CurrentStateFlag.ToString()} -> {stateFlag.ToString()}");
            this.CurrentStateFlag = stateFlag;
        }

        public void NewDaySetup()
        {
            if (this.CurrentStateFlag != StateFlag.RESET)
                throw new InvalidStateException($"State machine {this.Name} must be in reset state!");

            DialogueHelper.SetupDialogues(this.Companion, this.assets.dialogues);
            this.RecruitedToday = false;
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

        internal void Dismiss()
        {
            this.ResetStateMachine();
            (this.currentState as ResetState).ReintegrateCompanionNPC();
            this.BackedupSchedule = null;
            this.ChangeState(StateFlag.UNAVAILABLE);
            this.CompanionManager.CompanionDissmised();
        }

        public void Recruit()
        {
            this.BackedupSchedule = this.Companion.Schedule;
            this.RecruitedToday = true;

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
            if (!this.CanDialogueRequestResolve())
                return;

            (this.currentState as IRequestedDialogueCreator).CreateRequestedDialogue();
        }

        public bool CanDialogueRequestResolve()
        {
            return this.currentState is IRequestedDialogueCreator dcreator && dcreator.CanCreateDialogue;
        }
    }

    class InvalidStateException : Exception
    {
        public InvalidStateException(string message) : base(message)
        {
        }
    }
}
