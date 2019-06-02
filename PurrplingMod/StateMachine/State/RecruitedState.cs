using PurrplingMod.StateMachine.StateFeatures;
using PurrplingMod.Controller;
using PurrplingMod.Utils;
using StardewModdingAPI.Events;
using StardewValley;

namespace PurrplingMod.StateMachine.State
{
    internal class RecruitedState : CompanionState, IRequestedDialogueCreator, IDialogueDetector
    {
        private FollowController followController;
        private Dialogue dismissalDialogue;

        public bool CanCreateDialogue { get; private set; }

        public RecruitedState(CompanionStateMachine stateMachine, IModEvents events) : base(stateMachine, events)
        {
        }

        public override void Entry()
        {
            this.followController = new FollowController();
            this.followController.leader = this.StateMachine.CompanionManager.Farmer;
            this.followController.follower = this.StateMachine.Companion;

            this.StateMachine.Companion.faceTowardFarmerTimer = 0;
            this.StateMachine.Companion.movementPause = 0;
            this.StateMachine.Companion.temporaryController = null;
            this.StateMachine.Companion.controller = null;

            this.Events.GameLoop.UpdateTicked += this.GameLoop_UpdateTicking;
            this.Events.GameLoop.TimeChanged += this.GameLoop_TimeChanged;
            this.CanCreateDialogue = true;
        }

        private void GameLoop_TimeChanged(object sender, TimeChangedEventArgs e)
        {
            if (e.NewTime >= 2200)
            {
                NPC companion = this.StateMachine.Companion;
                Dialogue dismissalDialogue = new Dialogue(DialogueHelper.GetDialogueString(companion, "companionDismissAuto"), companion);
                this.dismissalDialogue = dismissalDialogue;
                DialogueHelper.DrawDialogue(dismissalDialogue);
            }
        }

        private void GameLoop_UpdateTicking(object sender, UpdateTickedEventArgs e)
        {
            this.StateMachine.Companion.movementPause = 0;

            this.followController.Update(e);
        }

        public override void Exit()
        {
            this.CanCreateDialogue = false;
            this.Events.GameLoop.UpdateTicked -= this.GameLoop_UpdateTicking;
            this.Events.GameLoop.TimeChanged -= this.GameLoop_TimeChanged;

            this.followController = null;
            this.dismissalDialogue = null;
        }

        public void CreateRequestedDialogue()
        {
            Farmer leader = this.StateMachine.CompanionManager.Farmer;
            GameLocation location = this.StateMachine.CompanionManager.Farmer.currentLocation;
            Response[] responses =
            {
                new Response("bag", "Can I use your bag?"),
                new Response("dismiss", "You are free today. Thank you for support, bye"),
                new Response("nothing", "(Nothing)"),
            };

            location.createQuestionDialogue($"What you do want?", responses, (_, answer) => {
                if (answer != "nothing")
                {
                    this.StateMachine.Companion.Halt();
                    this.StateMachine.Companion.facePlayer(leader);
                    this.ResolveAsk(this.StateMachine.Companion, leader, answer);
                }
            }, this.StateMachine.Companion);
        }

        private void ResolveAsk(NPC companion, Farmer leader, string action)
        {
            switch (action)
            {
                case "dismiss":
                    Dialogue dismissalDialogue = new Dialogue(DialogueHelper.GetDialogueString(companion, "companionDismiss"), companion);
                    this.dismissalDialogue = dismissalDialogue;
                    DialogueHelper.DrawDialogue(dismissalDialogue);
                    break;
            }
        }

        public void OnDialogueSpeaked(Dialogue speakedDialogue)
        {
            if (speakedDialogue == this.dismissalDialogue)
            {
                // After companion speaked a dismissal dialogue dismiss (unrecruit) companion who speaked that
                this.StateMachine.Dismiss(Game1.timeOfDay >= 2200);
            }
        }
    }
}
