using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public DialogueDriver(IModHelper helper)
        {
            helper.Events.GameLoop.UpdateTicking += this.Update;
        }

        public Dialogue CurrentDialogue
        {
            get { return this.currentDialogue; }
        }

        public NPC CurrentSpeaker
        {
            get { return this.currentSpeaker; }
        }

        private void Update(object sender, UpdateTickingEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            this.WatchDialogue();
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
}
