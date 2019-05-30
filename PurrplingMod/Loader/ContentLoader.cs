using StardewModdingAPI;
using System;
using System.Collections.Generic;

namespace PurrplingMod.Loader
{
    public class ContentLoader
    {
        private readonly string assetsDir;
        private readonly IMonitor monitor;

        public Dictionary<string, AssetsContent> ContentAssetsMap { get; }
        private IContentHelper Helper { get; }

        public ContentLoader(IContentHelper helper, string assetsDir, IMonitor monitor)
        {
            this.Helper = helper;
            this.assetsDir = assetsDir;
            this.monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
            this.ContentAssetsMap = new Dictionary<string, AssetsContent>();
        }

        public void Load(string dispositionsFile)
        {
           
            List<string> dispositions = this.Helper.Load<List<string>>(this.assetsDir + "/" + dispositionsFile);

            this.monitor.Log("Loading content assets", LogLevel.Info);

            foreach (string disposition in dispositions)
            {
                AssetsContent assets = new AssetsContent();
                this.LoadContentAssets(disposition, ref assets);
                this.ContentAssetsMap.Add(disposition, assets);
            }

            this.monitor.Log("Content assets loaded", LogLevel.Info);
        }

        private void LoadContentAssets(string disposition, ref AssetsContent assets)
        {
            this.monitor.Log($"Loading content assets for {disposition}");
            assets.dialogues = this.Helper.Load<Dictionary<string, string>>($"{this.assetsDir}/Dialogue/{disposition}.json");
        }

        public class AssetsContent
        {
            public Dictionary<string, string> dialogues;
        }
    }
}
