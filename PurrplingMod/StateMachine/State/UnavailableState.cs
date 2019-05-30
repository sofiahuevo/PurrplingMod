using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI.Events;

namespace PurrplingMod.StateMachine.State
{
    class UnavailableState : CompanionState
    {
        public UnavailableState(CompanionStateMachine stateMachine, IModEvents events) : base(stateMachine, events)
        {
        }

        public override void Entry()
        {
            this.StateMachine.Monitor.Log($"{this.StateMachine.Name} is now UNAVAILABLE!");
            // pass
        }

        public override void Exit()
        {
            // pass
        }
    }
}
