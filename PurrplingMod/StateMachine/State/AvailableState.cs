using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PurrplingMod.Driver;
using PurrplingMod.Manager;
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
            this.StateMachine.Manager.HintDriver.CheckHint += this.HintDriver_CheckHint;

            this.StateMachine.Manager.Monitor.Log($"{this.StateMachine.Companion.Name} is now available to recruit!");
        }

        private void HintDriver_CheckHint(object sender, Driver.CheckHintArgs e)
        {
            if (e.Npc?.Name == this.StateMachine.Companion.Name && Helper.CanRequestDialog(this.Leader, e.Npc))
            {
                this.StateMachine.Manager.HintDriver.ShowHint = Driver.HintDriver.Hint.DIALOGUE;
            }
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
                    this.ResolveAsk(e.WithWhom, e.Initiator);
                }
            }, null);
        }

        private void ResolveAsk(NPC n, Farmer leader)
        {
            DialogueDriver driver = this.StateMachine.Manager.DialogueDriver;
            DialogueManager dialogueManager = this.StateMachine.DialogueManager;

            if (leader.getFriendshipHeartLevelForNPC(n.Name) <= 4)
                driver.DrawDialogue(n, dialogueManager.GetDialogueString("companionRejected"));
            else if (Game1.timeOfDay > 2200 && !Helper.IsSpouseMarriedToFarmer(n, leader))
                driver.DrawDialogue(n, dialogueManager.GetDialogueString("companionRejectedNight"));
            else
            {
                driver.DrawDialogue(n, dialogueManager.GetDialogueString("companionAccepted"));
            }
        }

        public override void Exit()
        {
            this.StateMachine.Manager.DialogueDriver.DialogueRequested -= this.DialogueDriver_DialogueRequested;
            this.StateMachine.Manager.ModHelper.Events.GameLoop.DayEnding -= this.GameLoop_DayEnding;
        }
    }
}
