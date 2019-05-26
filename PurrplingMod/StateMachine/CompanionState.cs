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
        public CompanionState(CompanionStateMachine stateMachine)
        {
            this.StateMachine = stateMachine;
        }

        public abstract void Entry();
        public abstract void Exit();
        public virtual bool CanTransitionTo(ICompanionState newState)
        {
            return true;
        }
    }
}
