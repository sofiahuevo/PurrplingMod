using NpcAdventure.StateMachine;
using System;
using System.Collections.Generic;
using static NpcAdventure.StateMachine.CompanionStateMachine;

namespace NpcAdventure.Events
{
    public interface ICompanionEvents
    {
        event EventHandler Initialized;
        event EventHandler Uninitialized;
        event EventHandler<ICompanionStateChangedEventArgs> CompanionStateChanged;
        event EventHandler<ICompanionReadyForNewDayEventArgs> ReadyForNewDay;
    }

    public interface ICompanionStateChangedEventArgs
    {
        CompanionStateMachine Csm { get; }
        StateFlag OldStateFlag { get; }
        StateFlag NewStateFlag { get; }
    }

    public interface ICompanionReadyForNewDayEventArgs
    {
        Dictionary<string, CompanionStateMachine> ReadyCompanions { get; }
    }
}
