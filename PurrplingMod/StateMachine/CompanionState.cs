using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurrplingMod.StateMachine
{
    public abstract class CompanionState: ICompanionState
    {
        public CompanionStateMachine StateMachine { get; private set; }

        public Farmer Leader
        {
            get
            {
                return this.StateMachine.Manager?.Leader;
            }
        }
        public CompanionState(CompanionStateMachine stateMachine)
        {
            this.StateMachine = stateMachine ?? throw new Exception("State Machine must be set!");
        }

        public abstract void Entry();
        public abstract void Exit();
    }
}
