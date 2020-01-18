using StardewValley;

namespace NpcAdventure.StateMachine.StateFeatures
{
    public interface IDialogueDetector
    {
        void OnDialogueSpeaked(Dialogue speakedDialogue);
    }
}