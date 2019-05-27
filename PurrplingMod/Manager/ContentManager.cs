using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurrplingMod.Manager
{
    public class ContentManager
    {
        private readonly string dispositionsFile;
        private readonly string assetsDir;
        public Dictionary<string, ContentAssets> AssetsRegistry { get; }
        private IModHelper ModHelper { get; }
        private IMonitor Monitor { get; }

        public ContentManager(IModHelper helper, IMonitor monitor, string assetsDir)
        {
            this.ModHelper = helper;
            this.Monitor = monitor;
            this.assetsDir = assetsDir;
            this.AssetsRegistry = new Dictionary<string, ContentAssets>();
        }

        public void Load(string dispositionsFile)
        {
            
            List<string> dispositions = this.ModHelper.Content.Load<List<string>>(this.assetsDir + "/" + dispositionsFile);

            foreach (string disposition in dispositions)
            {
                ContentAssets assets = new ContentAssets();
                this.LoadContentAssets(disposition, ref assets);
                this.AssetsRegistry.Add(disposition, assets);
            }
        }

        private void LoadContentAssets(string disposition, ref ContentAssets assets)
        {
            this.Monitor.Log($"Loading content assets for {disposition}", LogLevel.Info);
            assets.dialogues = this.ModHelper.Content.Load<Dictionary<string, string>>($"{this.assetsDir}/Dialogue/{disposition}.json");
        }

        public class ContentAssets
        {
            public Dictionary<string, string> dialogues;
        }
    }
}
