using StardewValley;

namespace PurrplingMod.StateMachine.StateFeatures
{
    internal interface IDialogueDetector
    {
        void OnDialogueSpeaked(Dialogue speakedDialogue);
    }
}