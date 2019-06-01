using PurrplingMod.Utils;
using PurrplingMod.Internal;
using StardewModdingAPI.Events;
using StardewValley;

namespace PurrplingMod.StateMachine.State
{
    internal class AvailableState : CompanionState, IRequestedDialogueCreator
    {
        public bool CanCreateDialogue { get; private set; }

        public AvailableState(CompanionStateMachine stateMachine, IModEvents events) : base(stateMachine, events) {}

        public override void Entry()
        {
            this.CanCreateDialogue = true;
        }

        public override void Exit()
        {
            this.CanCreateDialogue = false;
        }

        private void ResolveAsk(NPC n, Farmer leader)
        {
            if (leader.getFriendshipHeartLevelForNPC(n.Name) <= 4)
                Game1.drawDialogue(n, DialogueHelper.GetDialogueString(n, "companionRejected"));
            else if (Game1.timeOfDay >= 2200 && !Helper.IsSpouseMarriedToFarmer(n, leader))
            {
                Game1.drawDialogue(n, DialogueHelper.GetDialogueString(n, "companionRejectedNight"));
                this.StateMachine.MakeUnavailable();
            }
            else
            {
                Game1.drawDialogue(n, DialogueHelper.GetDialogueString(n, "companionAccepted"));
                this.StateMachine.Recruit();
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
                    this.ResolveAsk(this.StateMachine.Companion, leader);
                }
            }, null);
        }
    }
}
