using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PurrplingMod.Controller;
using StardewModdingAPI.Events;

namespace PurrplingMod.StateMachine.State
{
    internal class RecruitedState : CompanionState
    {
        private FollowController followController;

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
            this.StateMachine.Monitor.Log($"{this.StateMachine.Name} is now RECRUITED!");
        }

        private void GameLoop_UpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            this.StateMachine.Companion.movementPause = 0;

            this.followController.Update(e);
        }

        public override void Exit()
        {
            this.Events.GameLoop.UpdateTicking -= this.GameLoop_UpdateTicking;

            this.followController = null;
        }
    }
}
