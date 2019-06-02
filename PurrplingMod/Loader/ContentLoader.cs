using Microsoft.Xna.Framework.Content;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.IO;

namespace PurrplingMod.Loader
{
    public class ContentLoader : IContentLoader
    {
        private readonly string assetsDir;
        private readonly IMonitor monitor;
        private readonly Dictionary<string, object> assetsMap;

        private IContentHelper Helper { get; }

        public ContentLoader(IContentHelper helper, string assetsDir, IMonitor monitor)
        {
            this.Helper = helper;
            this.assetsDir = assetsDir;
            this.assetsMap = new Dictionary<string, object>();
            this.monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
        }

        public bool CanLoad(string assetName)
        {
            string path = $"{this.assetsDir}/{assetName}.json";

            return File.Exists(path.Replace('/', Path.PathSeparator).Replace('\\', Path.PathSeparator));
        }

        public T Load<T>(string assetName)
        {
            if (this.assetsMap.TryGetValue(assetName, out object asset))
                return (T)asset;

            try
            {
                T newAsset = this.Helper.Load<T>($"{this.assetsDir}/{assetName}.json");

                this.assetsMap.Add(assetName, (object)newAsset);
                this.monitor.Log($"Loaded asset {assetName}", LogLevel.Info);

                return newAsset;
            }
            catch (ContentLoadException e)
            {
                this.monitor.Log($"Cannot load asset {assetName}", LogLevel.Error);
                throw e;
            }
        }

        public void InvalidateCache()
        {
            foreach (string assetName in this.assetsMap.Keys)
                this.Helper.InvalidateCache($"{this.assetsDir}/{assetName}.json");

            this.assetsMap.Clear();
        }
    }
}
