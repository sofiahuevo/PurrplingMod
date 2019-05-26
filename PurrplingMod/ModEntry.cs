using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Locations;
using System.Collections.Generic;

namespace PurrplingMod
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        private CompanionManager companionManager;
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.companionManager = new CompanionManager(helper, this.Monitor);
        }

        /*private void DialogueDriver_DialogueRequested(object sender, DialogueRequestArgs e)
        {
            if (e.WithWhom.CurrentDialogue.Count > 0)
                return;

            GameLocation location = e.Initiator.currentLocation;
            location.createQuestionDialogue($"Ask {e.WithWhom?.displayName} to follow?", location.createYesNoResponses(), (_, answer) => {
                if (answer == "Yes")
                {
                    e.WithWhom.Halt();
                    e.WithWhom.facePlayer(e.Initiator);
                    this.dialogueDriver.DrawDialogue(e.WithWhom, "Adventure with me?#$b#Yes please!$h");
                    this.followController.follower = e.WithWhom;
                }
                this.Monitor.Log($"Farmer asked for follow: {answer}");
            }, null);
        }*/
    }
}