using NpcAdventure.Story;
using StardewModdingAPI;
using StardewValley;

namespace NpcAdventure
{
    internal class Commander
    {
        private readonly NpcAdventureMod npcAdventureMod;
        private readonly IMonitor monitor;

        private Commander(NpcAdventureMod npcAdventureMod)
        {
            this.npcAdventureMod = npcAdventureMod;
            this.monitor = npcAdventureMod.Monitor;
            this.SetupCommands(npcAdventureMod.Helper.ConsoleCommands);
        }

        internal void SetupCommands(ICommandHelper consoleCommands)
        {
            if (!this.npcAdventureMod.Config.EnableDebug)
                return;

            consoleCommands.Add("npcadventure_eligible", "Make player eligible to recruit a companion (server or singleplayer only)", this.Eligible);
            consoleCommands.Add("npcadventure_contentpacks", "List installed content packs and which patches are applied", this.ContentPacks);
            this.monitor.Log("Registered debug commands", LogLevel.Info);
        }

        private void Eligible(string command, string[] args)
        {
            if (Context.IsWorldReady && Context.IsMainPlayer && this.npcAdventureMod.GameMaster.Mode == GameMasterMode.MASTER)
            {
                this.npcAdventureMod.GameMaster.Data.GetPlayerState(Game1.player).isEligible = true;
                this.npcAdventureMod.GameMaster.SyncData();
                this.monitor.Log("Player is now eligible for recruit companion.", LogLevel.Info);
            } else
            {
                this.monitor.Log("Can't eligible player when game is not loaded, in non-adventure mode or not running on server!", LogLevel.Alert);
            }
        }

        private void ContentPacks(string command, string[] args)
        {
            switch(args[0])
            {
                case "list":
                    this.monitor.Log("List of installed content packs for NPC Adventures:\n", LogLevel.Info);
                    foreach(var pack in this.npcAdventureMod.Helper.ContentPacks.GetOwned())
                    {
                        this.monitor.Log($"{pack.Manifest.Name} v{pack.Manifest.Version} by {pack.Manifest.Author} ({pack.Manifest.UniqueID})", LogLevel.Info);
                    }
                    break;
                case "patches":
                    this.monitor.Log("Applied patches from content packs to NPC Adventures:\n", LogLevel.Info);
                    foreach (var patch in this.npcAdventureMod.ContentLoader.ContentPackProvider.patches)
                    {
                        this.monitor.Log($"{patch.LogName} action {patch.Action} patching {patch.Target}", LogLevel.Info);
                    }
                    break;
                default:
                    this.monitor.Log($"Unknown subcommand: {args[0]}", LogLevel.Info);
                    break;
            }
        }

        public static Commander Register(NpcAdventureMod mod)
        {
            return new Commander(mod);
        }
    }
}
