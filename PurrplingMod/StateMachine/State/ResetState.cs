using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;

namespace PurrplingMod.StateMachine.State
{
    internal class ResetState : CompanionState
    {
        public ResetState(CompanionStateMachine stateMachine, IModEvents events) : base(stateMachine, events)
        {
        }

        public void ReintegrateCompanionNPC()
        {
            NPC companion = this.StateMachine.Companion;

            Game1.fadeBlack();

            companion.controller = null;
            companion.Schedule = this.StateMachine.BackedupSchedule;
            companion.followSchedule = true;
            companion.farmerPassesThrough = false;

            this.DelayedWarp(companion, companion.DefaultMap, companion.DefaultPosition, 500, new Action(this.CompanionCleanUp));
        }

        private void CompanionCleanUp()
        {
            NPC companion = this.StateMachine.Companion;
            Farmer farmer = this.StateMachine.CompanionManager.Farmer;

            if (farmer.spouse.Equals(companion.Name))
            {
                companion.setTilePosition((companion.currentLocation as FarmHouse).getKitchenStandingSpot());
            }

            companion.checkSchedule(Game1.timeOfDay);
        }

        private async void DelayedWarp(NPC companion, string location, Vector2 tileLocation, int milliseconds, Action afterWarpAction)
        {
            await Task.Run(() => this.Timer(milliseconds));
            if (companion.currentLocation != null)
                Game1.warpCharacter(companion, location, tileLocation);
            afterWarpAction.Invoke();
        }

        private int Timer(int milliseconds)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            while (watch.ElapsedMilliseconds < milliseconds) ;
            return 0;
        }
    }
}
