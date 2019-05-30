using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI.Events;

namespace PurrplingMod.StateMachine.State
{
    internal class ResetState : CompanionState
    {
        public ResetState(CompanionStateMachine stateMachine, IModEvents events) : base(stateMachine, events)
        {
        }

        public override void Entry()
        {
            this.Events.GameLoop.DayStarted += this.GameLoop_DayStarted;
            this.StateMachine.Monitor.Log($"Reset {this.StateMachine.Name}");
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            this.StateMachine.NewDaySetup();
        }

        public override void Exit()
        {
            this.Events.GameLoop.DayStarted -= this.GameLoop_DayStarted;
        }
    }
}
