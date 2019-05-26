using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurrplingMod.StateMachine
{
    public interface ICompanionState
    {
        void Entry();
        void Exit();
    }
}
