using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PurrplingMod.Controller;
using PurrplingMod.Internal;
using PurrplingMod.Utils;
using StardewModdingAPI.Events;
using StardewValley;

namespace PurrplingMod.StateMachine.State
{
    internal class RecruitedState : CompanionState, IRequestedDialogueCreator
    {
        private FollowController followController;

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

            this.Events.GameLoop.UpdateTicking += this.GameLoop_UpdateTicking;
            this.Events.GameLoop.TimeChanged += this.GameLoop_TimeChanged;
            this.CanCreateDialogue = true;
        }

        private void GameLoop_TimeChanged(object sender, TimeChangedEventArgs e)
        {
            if (e.NewTime >= 2200)
            {
                NPC companion = this.StateMachine.Companion;
                Game1.drawDialogue(companion, DialogueHelper.GetDialogueString(companion, "companionDismissAuto"));
                this.StateMachine.Dismiss(true);
            }
        }

        private void GameLoop_UpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            this.StateMachine.Companion.movementPause = 0;

            this.followController.Update(e);
        }

        public override void Exit()
        {
            this.CanCreateDialogue = false;
            this.Events.GameLoop.UpdateTicking -= this.GameLoop_UpdateTicking;
            this.Events.GameLoop.TimeChanged -= this.GameLoop_TimeChanged;

            this.followController = null;
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
                    Game1.drawDialogue(companion, DialogueHelper.GetDialogueString(companion, "companionDismiss"));
                    this.StateMachine.Dismiss();
                    break;
            }
        }
    }
}
