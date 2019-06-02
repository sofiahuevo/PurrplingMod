using PurrplingMod.Utils;
using PurrplingMod.StateMachine.StateFeatures;
using StardewModdingAPI.Events;
using StardewValley;

namespace PurrplingMod.StateMachine.State
{
    internal class AvailableState : CompanionState, IRequestedDialogueCreator, IDialogueDetector
    {
        private Dialogue acceptalDialogue;
        private Dialogue rejectionDialogue;

        public bool CanCreateDialogue { get; private set; }

        public AvailableState(CompanionStateMachine stateMachine, IModEvents events) : base(stateMachine, events) {}

        public override void Entry()
        {
            this.CanCreateDialogue = true;
        }

        public override void Exit()
        {
            this.CanCreateDialogue = false;
            this.acceptalDialogue = null;
            this.rejectionDialogue = null;
        }

        private void ReactOnAsk(NPC n, Farmer leader)
        {
            if (leader.getFriendshipHeartLevelForNPC(n.Name) <= 4 || Game1.timeOfDay >= 2200)
            {
                Dialogue rejectionDialogue = new Dialogue(
                    DialogueHelper.GetDialogueString(
                        n, Game1.timeOfDay >= 2200 ? "companionRejectedNight" : "companionRejected"), n);

                this.rejectionDialogue = rejectionDialogue;
                DialogueHelper.DrawDialogue(rejectionDialogue);
            }
            else
            {
                Dialogue acceptalDialogue = new Dialogue(DialogueHelper.GetDialogueString(n, "companionAccepted"), n);

                this.acceptalDialogue = acceptalDialogue;
                DialogueHelper.DrawDialogue(acceptalDialogue);
            }
        }

        public void CreateRequestedDialogue()
        {
            Farmer leader = this.StateMachine.CompanionManager.Farmer;
            GameLocation location = this.StateMachine.CompanionManager.Farmer.currentLocation;
            location.createQuestionDialogue($"Ask {this.StateMachine.Companion.displayName} to follow?", location.createYesNoResponses(), (_, answer) => {
                if (answer == "Yes")
                {
                    this.StateMachine.Companion.Halt();
                    this.StateMachine.Companion.facePlayer(leader);
                    this.ReactOnAsk(this.StateMachine.Companion, leader);
                }
            }, null);
        }

        public void OnDialogueSpeaked(Dialogue speakedDialogue)
        {
            if (speakedDialogue == this.acceptalDialogue)
            {
                this.StateMachine.Recruit();
            }
            else if (speakedDialogue == this.rejectionDialogue)
            {
                this.StateMachine.MakeUnavailable();
            }
        }
    }
}
