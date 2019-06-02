using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurrplingMod.StateMachine.StateFeatures
{
    interface IRequestedDialogueCreator
    {
        bool CanCreateDialogue { get; }
        void CreateRequestedDialogue();
    }
}
