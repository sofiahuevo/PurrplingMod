using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PurrplingMod.StateMachine.State;
using StardewValley;

namespace PurrplingMod.StateMachine
{
    public class CompanionStateMachine
    {
        public enum StateName
        {
            RESET,
            AVAILABLE,
            RECRUITED,
            UNAVAILABLE,
        }
        public CompanionManager Manager { get; private set; }
        public NPC Companion { get; private set; }
        public Dictionary<StateName, ICompanionState> States { get; private set; }
        private StateName currentStatePtr;
        public CompanionStateMachine(CompanionManager manager, NPC companion)
        {
            this.Manager = manager;
            this.Companion = companion;
            this.States = new Dictionary<StateName, ICompanionState>()
            {
                [StateName.RESET] = new ResetState(this),
            };
            this.ResetStateMachine();
        }

        public bool ChangeState(StateName stateName)
        {
            if (!this.States.TryGetValue(this.currentStatePtr, out ICompanionState currentState) || !this.States.TryGetValue(stateName, out ICompanionState newState))
                return false;

            if (!currentState.CanTransitionTo(newState))
                return false;

            currentState.Exit();
            newState.Entry();
            this.currentStatePtr = stateName;

            return true;
        }

        private void ResetStateMachine()
        {
            if (this.States.TryGetValue(this.currentStatePtr, out ICompanionState oldState))
                oldState.Exit();
            this.currentStatePtr = StateName.RESET;
            this.States[this.currentStatePtr].Entry();
        }
    }
}
