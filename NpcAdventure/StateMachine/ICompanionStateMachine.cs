using NpcAdventure.Model;
using StardewValley;
using StardewValley.Objects;
using System.Collections.Generic;

namespace NpcAdventure.StateMachine
{
    public interface ICompanionStateMachine
    {
        Chest Bag { get; }
        bool CanSuggestToday { get; }
        NPC Companion { get; }
        StateFlag CurrentStateFlag { get; }
        string Name { get; }
        bool RecruitedToday { get; }
        bool SuggestedToday { get; }
        Dictionary<StateFlag, ICompanionState> States { get; }
        bool CheckAction(Farmer who, GameLocation location);
        void DumpBagInFarmHouse();
        ICompanionState GetCurrentState();
        bool HasSkill(string skill);
        bool HasSkills(params string[] skills);
        bool HasSkillsAny(params string[] skills);
        void MakeAvailable();
        void MakeUnavailable();
        void Recruit();
        void ResetStateMachine();
    }
}