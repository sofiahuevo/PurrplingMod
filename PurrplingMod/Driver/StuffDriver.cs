using PurrplingMod.Model;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurrplingMod.Driver
{
    class StuffDriver
    {
        public List<BagDumpInfo> DumpedBags { get; set; }
        public IMonitor Monitor { get; }

        public StuffDriver(IModEvents events, IDataHelper dataHelper, IMonitor monitor)
        {
            events.GameLoop.Saving += this.GameLoop_Saving;
            events.GameLoop.SaveLoaded += this.GameLoop_SaveLoaded;
            events.GameLoop.DayStarted += this.GameLoop_DayStarted;

            this.DataHelper = dataHelper;
            this.DumpedBags = new List<BagDumpInfo>();
            this.Monitor = monitor;
        }

        public IDataHelper DataHelper { get; }

        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            try
            {
                List<BagDumpInfo> dumpedBags = this.DataHelper.ReadSaveData<List<BagDumpInfo>>("dumped-bags");
                this.DumpedBags = dumpedBags ?? new List<BagDumpInfo>();
                this.Monitor.Log("Dumped bags loaded from save file", LogLevel.Info);
            }
            catch (InvalidOperationException ex)
            {
                this.Monitor.Log($"Error while loading dumped bag from savefile: {ex.Message}");
            }
        }

        private void GameLoop_Saving(object sender, SavingEventArgs e)
        {
            try
            {
                this.DataHelper.WriteSaveData("dumped-bags", this.DumpedBags ?? new List<BagDumpInfo>());
                this.Monitor.Log("Dumped bags successfully saved to savefile.", LogLevel.Info);
            }
            catch (InvalidOperationException ex)
            {
                this.Monitor.Log($"Error while saving dumped bags: {ex.Message}", LogLevel.Error);
            }
        }
    }
}
