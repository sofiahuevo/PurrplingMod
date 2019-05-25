using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace PurrplingMod
{
    public class DialogueDriver
    {
        private Dialogue currentDialogue;
        private NPC currentSpeaker;

        public event EventHandler<DialogueChangedArgs> DialogueChanged;
        public event EventHandler<DialogueEndedArgs> DialogueEnded;
        public event EventHandler<SpeakerChangedArgs> SpeakerChanged;
        public event EventHandler<DialogueRequestArgs> DialogueRequested;

        public DialogueDriver(IModHelper helper)
        {
            helper.Events.GameLoop.UpdateTicking += this.Update;
            helper.Events.Input.ButtonPressed += this.HandleAction;
        }

        public Dialogue CurrentDialogue
        {
            get { return this.currentDialogue; }
        }

        public NPC CurrentSpeaker
        {
            get { return this.currentSpeaker; }
        }

        public void RequestDialogue(Farmer who, NPC withWhom, int requestId)
        {
            if (this.DialogueRequested == null)
                return;

            DialogueRequestArgs args = new DialogueRequestArgs()
            {
                Initiator = who,
                WithWhom = withWhom,
                RequestId = requestId,
            };

            this.DialogueRequested(this, args);
        }

        public void DrawDialogue(NPC speaker)
        {
            Game1.drawDialogue(speaker);
        }

        public void DrawDialogue(NPC speaker, string dialogue)
        {
            Game1.drawDialogue(speaker, dialogue);
        }

        private void Update(object sender, UpdateTickingEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            this.WatchDialogue();
        }

        private void HandleAction(object sender, ButtonPressedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet or player can't move
            if (!Context.IsWorldReady || !Context.CanPlayerMove)
                return;

            Farmer farmer = Game1.player;
            Rectangle farmerBox = Game1.player.GetBoundingBox();
            bool giftableObjectInHands = farmer.ActiveObject != null && farmer.ActiveObject.canBeGivenAsGift();
            bool actionButtonPressed = e.Button.IsActionButton() || e.Button.IsUseToolButton();

            farmerBox.Inflate(64, 64);

            if (giftableObjectInHands)
                return;

            foreach (NPC npc in farmer.currentLocation.characters) {
                Rectangle npcBox = npc.GetBoundingBox();
                Rectangle spriteBox = npc.Sprite.SourceRect;
                bool isNpcAtCursorTile = Helper.IsNPCAtTile(farmer.currentLocation, e.Cursor.Tile, npc)
                                         || Helper.IsNPCAtTile(farmer.currentLocation, e.Cursor.Tile + new Vector2(0f, 1f), npc)
                                         || Helper.IsNPCAtTile(farmer.currentLocation, e.Cursor.GrabTile, npc);

                

                if (actionButtonPressed && farmerBox.Intersects(npcBox) && isNpcAtCursorTile)
                {
                    if (this.CanRequestDialog(farmer, npc))
                        this.RequestDialogue(farmer, npc, 0);
                    break;
                }
            }

        }

        private bool CanRequestDialog(Farmer farmer, NPC npc)
        {
            bool forbidden = false;
            NPC spouse = farmer.getSpouse();

            if (spouse != null && spouse.Name == npc.Name)
            {
                // Kiss married spouse first if facing kissable, then request a dialog
                bool flag = spouse.isMarried() && farmer.isMarried() && !Helper.SpouseHasBeenKissedToday(spouse);
                forbidden = flag && npc.FacingDirection == 3 || flag && npc.FacingDirection == 1;
            }

            return !forbidden;
        }

        private void WatchDialogue()
        {
            // Check if speaker is changed
            if (Game1.currentSpeaker != this.currentSpeaker)
            {
                if (Game1.currentSpeaker != null)
                    this.OnSpeakerChange(this.currentSpeaker, Game1.currentSpeaker);
                this.currentSpeaker = Game1.currentSpeaker;

                if (this.currentSpeaker == null)
                {
                    // Dialogue ended
                    this.OnEndDialogue(this.currentDialogue);
                    this.currentDialogue = null;
                    return;
                }
            }

            if (this.currentSpeaker == null)
                return; // Nobody speaking, no dialogue can be changed

            Dialogue dialogue = null;

            if (this.currentSpeaker.CurrentDialogue?.Count > 0)
                dialogue = this.currentSpeaker.CurrentDialogue.Peek();

            // Check if dialogue is changed
            if (this.currentDialogue != dialogue)
            {
                this.OnChangeDialogue(this.currentDialogue, dialogue, this.currentSpeaker.CurrentDialogue?.Count == 1);
                this.currentDialogue = dialogue;
            }
        }

        private void OnChangeDialogue(Dialogue previousDialogue, Dialogue currentDialogue, bool isLastDialogue = false)
        {
            if (this.DialogueChanged == null)
                return;

            DialogueChangedArgs args = new DialogueChangedArgs
            {
                PreviousDialogue = previousDialogue,
                CurrentDialogue = currentDialogue,
                IsLastDialogue = isLastDialogue
            };

            this.DialogueChanged(this, args);
        }

        private void OnEndDialogue(Dialogue previousDialogue)
        {
            if (this.DialogueEnded == null)
                return;

            DialogueEndedArgs args = new DialogueEndedArgs
            {
                PreviousDialogue = previousDialogue
            };

            this.DialogueEnded(this, args);
        }

        private void OnSpeakerChange(NPC previousSpeaker, NPC currentSpeaker)
        {
            if (this.SpeakerChanged == null)
                return;

            SpeakerChangedArgs args = new SpeakerChangedArgs()
            {
                CurrentSpeaker = currentSpeaker,
                PreviousSpeaker = previousSpeaker
            };

            this.SpeakerChanged(this, args);
        }
    }

    public class DialogueChangedArgs : EventArgs
    {
        public Dialogue CurrentDialogue { get; set; }
        public Dialogue PreviousDialogue { get; set; }
        public bool IsLastDialogue { get; set; }
    }

    public class DialogueEndedArgs : EventArgs
    {
        public Dialogue PreviousDialogue { get; set; }
    }

    public class SpeakerChangedArgs : EventArgs
    {
        public NPC CurrentSpeaker { get; set; }
        public NPC PreviousSpeaker { get; set; }
    }

    public class DialogueRequestArgs : EventArgs
    {
        public Farmer Initiator { get; set; }
        public NPC WithWhom { get; set; }
        public int RequestId { get; set; }
    }
}
