using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;

namespace PurrplingMod.StateMachine.State
{
    class AvailableState : CompanionState
    {
        public AvailableState(CompanionStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Entry()
        {
            this.StateMachine.Manager.DialogueDriver.DialogueRequested += this.DialogueDriver_DialogueRequested;
            this.StateMachine.Manager.ModHelper.Events.GameLoop.DayEnding += this.GameLoop_DayEnding;

            this.StateMachine.Manager.Monitor.Log($"{this.StateMachine.Companion.Name} is now available to recruit!");
        }

        private void GameLoop_DayEnding(object sender, StardewModdingAPI.Events.DayEndingEventArgs e)
        {
            this.StateMachine.ResetStateMachine();
        }

        private void DialogueDriver_DialogueRequested(object sender, Driver.DialogueRequestArgs e)
        {
            if (e.WithWhom.CurrentDialogue.Count > 0 || e.WithWhom.Name != this.StateMachine.Companion.Name)
                return;

            GameLocation location = e.Initiator.currentLocation;
            location.createQuestionDialogue($"Ask {e.WithWhom?.displayName} to follow?", location.createYesNoResponses(), (_, answer) => {
                if (answer == "Yes")
                {
                    e.WithWhom.Halt();
                    e.WithWhom.facePlayer(e.Initiator);
                    this.StateMachine.Manager.DialogueDriver.DrawDialogue(e.WithWhom, "Adventure with me?#$b#Yes please!$h");
                }
            }, null);
        }

        public override void Exit()
        {
            this.StateMachine.Manager.DialogueDriver.DialogueRequested -= this.DialogueDriver_DialogueRequested;
            this.StateMachine.Manager.ModHelper.Events.GameLoop.DayEnding -= this.GameLoop_DayEnding;
        }
    }
}
