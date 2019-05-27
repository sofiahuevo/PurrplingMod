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
            PurrplingMod.DialogueDriver.DialogueRequested += this.DialogueDriver_DialogueRequested;
            PurrplingMod.Events.GameLoop.DayEnding += this.GameLoop_DayEnding;
            PurrplingMod.HintDriver.CheckHint += this.HintDriver_CheckHint;

            PurrplingMod.Mon.Log($"{this.StateMachine.Companion.Name} is now available to recruit!");
        }

        private void HintDriver_CheckHint(object sender, Driver.CheckHintArgs e)
        {
            if (e.Npc?.Name == this.StateMachine.Companion.Name && Helper.CanRequestDialog(this.Leader, e.Npc))
            {
                PurrplingMod.HintDriver.ShowHint = Driver.HintDriver.Hint.DIALOGUE;
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
            DialogueManager dialogueManager = this.StateMachine.DialogueManager;

            if (leader.getFriendshipHeartLevelForNPC(n.Name) <= 4)
                PurrplingMod.DialogueDriver.DrawDialogue(n, dialogueManager.GetDialogueString("companionRejected"));
            else if (Game1.timeOfDay > 2200 && !Helper.IsSpouseMarriedToFarmer(n, leader))
                PurrplingMod.DialogueDriver.DrawDialogue(n, dialogueManager.GetDialogueString("companionRejectedNight"));
            else
            {
                PurrplingMod.DialogueDriver.DrawDialogue(n, dialogueManager.GetDialogueString("companionAccepted"));
            }
        }

        public override void Exit()
        {
            PurrplingMod.DialogueDriver.DialogueRequested -= this.DialogueDriver_DialogueRequested;
            PurrplingMod.Events.GameLoop.DayEnding -= this.GameLoop_DayEnding;
        }
    }
}
