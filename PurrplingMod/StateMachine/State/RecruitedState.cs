using PurrplingMod.StateMachine.StateFeatures;
using PurrplingMod.Controller;
using PurrplingMod.Utils;
using StardewModdingAPI.Events;
using StardewValley;
using System.Collections.Generic;
using System.Linq;

namespace PurrplingMod.StateMachine.State
{
    internal class RecruitedState : CompanionState, IRequestedDialogueCreator, IDialogueDetector
    {
        private FollowController followController;
        private Dialogue dismissalDialogue;
        private Dialogue currentLocationDialogue;

        public bool CanCreateDialogue { get; private set; }

        public RecruitedState(CompanionStateMachine stateMachine, IModEvents events) : base(stateMachine, events) {}

        public override void Entry()
        {
            this.followController = new FollowController();
            this.followController.leader = this.StateMachine.CompanionManager.Farmer;
            this.followController.follower = this.StateMachine.Companion;

            this.StateMachine.Companion.faceTowardFarmerTimer = 0;
            this.StateMachine.Companion.movementPause = 0;
            this.StateMachine.Companion.followSchedule = false;
            this.StateMachine.Companion.temporaryController = null;
            this.StateMachine.Companion.controller = null;

            this.Events.GameLoop.UpdateTicked += this.GameLoop_UpdateTicked;
            this.Events.GameLoop.TimeChanged += this.GameLoop_TimeChanged;
            this.Events.Player.Warped += this.Player_Warped;

            Buff buff = new Buff(0, 0, 0, 0, 2, 0, 0, 0, 0, 1, 0, 0, 30, this.StateMachine.Companion.Name, this.StateMachine.Companion.displayName);
            buff.description = "Abbynka";

            Game1.buffsDisplay.addOtherBuff(buff);
            Game1.buffsDisplay.syncIcons();

            if (DialogueHelper.GetVariousDialogueString(this.StateMachine.Companion, "companionRecruited", out string dialogueText))
                this.StateMachine.Companion.setNewDialogue(dialogueText);
            this.CanCreateDialogue = true;
        }

        public override void Exit()
        {
            this.CanCreateDialogue = false;

            this.Events.GameLoop.UpdateTicked -= this.GameLoop_UpdateTicked;
            this.Events.GameLoop.TimeChanged -= this.GameLoop_TimeChanged;
            this.Events.Player.Warped -= this.Player_Warped;

            this.followController = null;
            this.dismissalDialogue = null;
        }

        private void GameLoop_TimeChanged(object sender, TimeChangedEventArgs e)
        {
            if (e.NewTime >= 2200)
            {
                NPC companion = this.StateMachine.Companion;
                Dialogue dismissalDialogue = new Dialogue(DialogueHelper.GetDialogueString(companion, "companionDismissAuto"), companion);
                this.dismissalDialogue = dismissalDialogue;
                this.StateMachine.Companion.doEmote(24);
                this.StateMachine.Companion.updateEmote(Game1.currentGameTime);
                DialogueHelper.DrawDialogue(dismissalDialogue);
            }
        }

        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            this.StateMachine.Companion.movementPause = 0;

            this.followController.Update(e);
        }

        private void Player_Warped(object sender, WarpedEventArgs e)
        {
            NPC companion = this.StateMachine.Companion;
            Farmer farmer = this.StateMachine.CompanionManager.Farmer;
            Dictionary<string, string> bubbles = this.StateMachine.ContentLoader.LoadStrings("Strings/SpeechBubbles");

            // Warp companion to farmer if it's needed
            if (companion.currentLocation != e.NewLocation)
                Game1.warpCharacter(companion, e.NewLocation, farmer.Position);

            // Show above head bubble text for location
            if (DialogueHelper.GetBubbleString(bubbles, companion, e.NewLocation, out string bubble))
                companion.showTextAboveHead(bubble, preTimer: 250);

            // Push new location dialogue
            this.TryPushLocationDialogue(e.NewLocation);
        }

        private bool TryPushLocationDialogue(GameLocation location)
        {
            NPC companion = this.StateMachine.Companion;
            Dialogue newDialogue = DialogueHelper.GenerateDialogue(companion, location, "companion");
            Stack<Dialogue> temp = new Stack<Dialogue>(this.StateMachine.Companion.CurrentDialogue.Count);

            if ((newDialogue == null && this.currentLocationDialogue == null) || (newDialogue != null && newDialogue.Equals(this.currentLocationDialogue)))
                return false;

            // Remove old location dialogue
            while (this.StateMachine.Companion.CurrentDialogue.Count > 0)
            {
                Dialogue d = this.StateMachine.Companion.CurrentDialogue.Pop();

                if (!d.Equals(this.currentLocationDialogue))
                    temp.Push(d);
            }

            while (temp.Count > 0)
                this.StateMachine.Companion.CurrentDialogue.Push(temp.Pop());

            this.currentLocationDialogue = newDialogue;

            if (newDialogue != null)
            {
                this.StateMachine.Companion.CurrentDialogue.Push(newDialogue); // Push new location dialogue
                return true;
            }

            return false;
        }

        public void CreateRequestedDialogue()
        {
            Farmer leader = this.StateMachine.CompanionManager.Farmer;
            GameLocation location = this.StateMachine.CompanionManager.Farmer.currentLocation;
            string question = this.StateMachine.ContentLoader.LoadString("Strings/Strings:recruitedWant");
            Response[] responses =
            {
                new Response("bag", this.StateMachine.ContentLoader.LoadString("Strings/Strings:recruitedWant.bag")),
                new Response("dismiss", this.StateMachine.ContentLoader.LoadString("Strings/Strings:recruitedWant.dismiss")),
                new Response("nothing", this.StateMachine.ContentLoader.LoadString("Strings/Strings:recruitedWant.nothing")),
            };

            location.createQuestionDialogue(question, responses, (_, answer) => {
                if (answer != "nothing")
                {
                    this.StateMachine.Companion.Halt();
                    this.StateMachine.Companion.facePlayer(leader);
                    this.ReactOnAsk(this.StateMachine.Companion, leader, answer);
                }
            }, this.StateMachine.Companion);
        }

        private void ReactOnAsk(NPC companion, Farmer leader, string action)
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
