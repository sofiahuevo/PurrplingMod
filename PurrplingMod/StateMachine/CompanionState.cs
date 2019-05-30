using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurrplingMod.StateMachine
{
    public interface ICompanionState
    {
        void Entry();
        void Exit();
    }

    internal abstract class CompanionState : ICompanionState
    {
        public CompanionStateMachine StateMachine { get; private set; }
        protected IModEvents Events { get; }

        public CompanionState(CompanionStateMachine stateMachine, IModEvents events)
        {
            this.StateMachine = stateMachine ?? throw new Exception("State Machine must be set!");
            this.Events = events;
        }

        public abstract void Entry();
        public abstract void Exit();
    }
}
