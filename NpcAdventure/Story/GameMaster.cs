using NpcAdventure.NetCode;
using NpcAdventure.Story.Messaging;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using static NpcAdventure.NetCode.NetEvents;

namespace NpcAdventure.Story
{
    internal class GameMaster : IGameMaster
    {
        private readonly IDataHelper dataHelper;

        public event EventHandler<IGameMasterEventArgs> MessageReceived;

        private List<IScenario> Scenarios { get; set; }

        internal IMonitor Monitor { get; }

        public GameMasterState Data { get; private set; }

        public GameMasterMode Mode { get; private set; }
        public StoryHelper StoryHelper { get; private set; }

        private NetEvents netEvents;

        public GameMaster(IModHelper helper, StoryHelper storyHelper, IMonitor monitor, NetEvents netEvents)
        {
            this.dataHelper = helper.Data;
            this.StoryHelper = storyHelper;
            this.Monitor = monitor;
            this.Scenarios = new List<IScenario>();
            this.netEvents = netEvents;
        }

        internal void Initialize()
        {
            if (Context.IsMainPlayer)
            {
                this.Data = this.dataHelper.ReadSaveData<GameMasterState>("story") ?? new GameMasterState();
            }
            else
            {
                this.Data = new GameMasterState();
                this.netEvents.FireEvent(new GameMasterStateSyncRequest());
            }

            foreach (var scenario in this.Scenarios)
            {
                scenario.Initialize();
            }

            this.Mode = Context.IsMainPlayer ? GameMasterMode.MASTER : GameMasterMode.SLAVE;
            this.Monitor.Log($"Game master initialized in mode: {this.Mode.ToString()}", LogLevel.Info);
        }

        public void RegisterScenario(IScenario scenario)
        {
            scenario.GameMaster = this;
            this.Scenarios.Add(scenario);
        }

        internal void Uninitialize()
        {
            if (this.Mode == GameMasterMode.OFFLINE)
                return;

            foreach (var scenario in this.Scenarios)
            {
                scenario.Dispose();
            }

            this.Mode = GameMasterMode.OFFLINE;
            this.Monitor.Log("Game master uninitialized!", LogLevel.Info);
        }

        internal void SaveData()
        {
            if (this.Mode == GameMasterMode.MASTER)
                this.dataHelper.WriteSaveData("story", this.Data);
            else
                this.SyncData();
        }

        public void SyncData()
        {
            if (!Context.IsMultiplayer || this.Mode == GameMasterMode.OFFLINE)
                return; // Nothing to sync in singleplayer game or game master is not initialized

            if (this.Mode == GameMasterMode.MASTER)
            {
                foreach(var kv in this.Data.EligiblePlayers)
                {
                    this.netEvents.FireEvent(new GameMasterStateSyncResponse(kv.Key, kv.Value), null, true);
                }
            } 

            if (this.Mode == GameMasterMode.SLAVE)
            {
                this.netEvents.FireEvent(new GameMasterStateSyncResponse(Game1.player.UniqueMultiplayerID, this.Data.GetPlayerState()), null, true);
            }
        }

        /// <summary>
        /// Send event message to game master's listeners (like scenarios)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public void SendEventMessage(IGameMasterMessage message)
        {
            if (this.Mode == GameMasterMode.OFFLINE)
                return;

           this.MessageReceived?.Invoke(this, new GameMasterEventArgs()
                {
                    Message = message,
                    Player = Game1.player,
                    IsLocal = true,
                }
            );

            this.SyncData();
        }

        private class GameMasterEventArgs : IGameMasterEventArgs
        {
            public IGameMasterMessage Message { get; set; }
            public Farmer Player { get; set; }
            public bool IsLocal { get; set; }
        }
    }
}
