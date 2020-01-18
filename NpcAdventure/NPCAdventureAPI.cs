using NpcAdventure.Loader;
using NpcAdventure.Loader.ContentPacks;
using NpcAdventure.Model;
using NpcAdventure.StateMachine;
using NpcAdventure.StateMachine.State;
using NpcAdventure.Story;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using System.Linq;

namespace NpcAdventure
{
    public interface INPCAdventureAPI
    {
        IGameMaster GameMaster { get; }
        IEnumerable<ICompanionStateMachine> Companions { get; }
        void AddCompanion(NPC npc, CompanionMetaData metadata, Dictionary<StateFlag, ICompanionState> stateHandlers, IContentLoader contentLoader = null, IMonitor monitor = null);
        void AddCompanion(NPC npc, CompanionMetaData metadata, IContentLoader contentLoader = null, IMonitor monitor = null);
        void AddPatch(IAssetPatch patch);
        ICompanionStateMachine GetCompanion(string name);
        IEnumerable<IAssetPatch> GetPatches(IManifest manifest);
    }

    public class NPCAdventureAPI : INPCAdventureAPI
    {
       
        internal NPCAdventureAPI(NpcAdventureMod mod)
        {
            this.mod = mod;
            this.GameMaster = mod.GameMaster;
        }

        private readonly NpcAdventureMod mod;

        public IGameMaster GameMaster { get; set; }

        public IEnumerable<ICompanionStateMachine> Companions => this.mod
            .CompanionManager?
            .PossibleCompanions
            .Select(pair => (ICompanionStateMachine)pair.Value);

        public ICompanionStateMachine GetCompanion(string name)
        {
            return this.mod.CompanionManager.PossibleCompanions[name];
        }

        public void AddCompanion(NPC npc, CompanionMetaData metadata, Dictionary<StateFlag, ICompanionState> stateHandlers, IContentLoader contentLoader = null, IMonitor monitor = null)
        {
            var csm = new CompanionStateMachine(this.mod.CompanionManager, npc, metadata, contentLoader ?? this.mod.ContentLoader, this.mod.Helper.Reflection, monitor ?? this.mod.Monitor);
            
            csm.Setup(stateHandlers);
            this.mod.CompanionManager.PossibleCompanions.Add(csm.Name, csm);
        }

        public void AddCompanion(NPC npc, CompanionMetaData metadata, IContentLoader contentLoader = null, IMonitor monitor = null)
        {
            var csm = new CompanionStateMachine(this.mod.CompanionManager, npc, metadata, contentLoader ?? this.mod.ContentLoader, this.mod.Helper.Reflection, monitor ?? this.mod.Monitor);
            var stateHandlers = new Dictionary<StateFlag, ICompanionState>()
            {
                [StateFlag.RESET] = new ResetState(csm, this.mod.Helper.Events, monitor),
                [StateFlag.AVAILABLE] = new AvailableState(csm, this.mod.Helper.Events, monitor),
                [StateFlag.RECRUITED] = new RecruitedState(csm, this.mod.Helper.Events, this.mod.SpecialEvents, monitor),
                [StateFlag.UNAVAILABLE] = new UnavailableState(csm, this.mod.Helper.Events, monitor),
            };

            csm.Setup(stateHandlers);
            this.mod.CompanionManager.PossibleCompanions.Add(csm.Name, csm);
        }

        public void AddPatch(IAssetPatch patch)
        {
            this.mod.ContentLoader.ContentPackProvider.patches.Add(patch);
        }

        public IEnumerable<IAssetPatch> GetPatches(IManifest manifest)
        {
            return this.mod
                .ContentLoader
                .ContentPackProvider
                .patches
                .Where(p => p.Source.UniqueID.Equals(manifest.UniqueID));
        }
    }
}
