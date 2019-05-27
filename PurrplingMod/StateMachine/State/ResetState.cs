using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurrplingMod.StateMachine.State
{
    internal class ResetState : CompanionState
    {
        public ResetState(CompanionStateMachine stateMachine) : base(stateMachine)
        {
        }
        public override void Entry()
        {
            PurrplingMod.Events.GameLoop.DayStarted += this.GameLoop_DayStarted;

            PurrplingMod.Mon.Log($"Reset {this.StateMachine.Companion.Name}");
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            this.StateMachine.NewDaySetup();
        }

        public override void Exit()
        {
            PurrplingMod.Events.GameLoop.DayStarted -= this.GameLoop_DayStarted;
        }
    }
}
