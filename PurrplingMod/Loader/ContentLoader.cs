using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurrplingMod.Loader
{
    public class ContentLoader
    {
        private readonly string assetsDir;
        public Dictionary<string, AssetsContent> ContentAssetsMap { get; }
        private IContentHelper Helper { get; }

        public ContentLoader(IContentHelper helper, string assetsDir)
        {
            this.Helper = helper;
            this.assetsDir = assetsDir;
            this.ContentAssetsMap = new Dictionary<string, AssetsContent>();
        }

        public void Load(string dispositionsFile)
        {
           
            List<string> dispositions = this.Helper.Load<List<string>>(this.assetsDir + "/" + dispositionsFile);

            PurrplingMod.Mon.Log("Loading content assets", LogLevel.Info);

            foreach (string disposition in dispositions)
            {
                AssetsContent assets = new AssetsContent();
                this.LoadContentAssets(disposition, ref assets);
                this.ContentAssetsMap.Add(disposition, assets);
            }

            PurrplingMod.Mon.Log("Content assets loaded", LogLevel.Info);
        }

        private void LoadContentAssets(string disposition, ref AssetsContent assets)
        {
            PurrplingMod.Mon.Log($"Loading content assets for {disposition}");
            assets.dialogues = this.Helper.Load<Dictionary<string, string>>($"{this.assetsDir}/Dialogue/{disposition}.json");
        }

        public class AssetsContent
        {
            public Dictionary<string, string> dialogues;
        }
    }
}
