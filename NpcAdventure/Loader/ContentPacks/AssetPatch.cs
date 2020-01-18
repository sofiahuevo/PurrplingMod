using NpcAdventure.Model;
using StardewModdingAPI;
using System.Collections.Generic;

namespace NpcAdventure.Loader.ContentPacks
{
    /// <summary>
    /// Asset patch for mod's original asset
    /// </summary>
    internal class AssetPatch : IAssetPatch
    {
        private readonly ContentPackData.DataChanges meta;
        private readonly ManagedContentPack contentPack;

        public AssetPatch(ContentPackData.DataChanges meta, ManagedContentPack contentPack, string logName)
        {
            this.meta = meta;
            this.contentPack = contentPack;
            this.LogName = logName;
            this.Source = contentPack.Pack.Manifest;
        }

        public string Action { get => this.meta.Action; }
        public string Target { get => this.meta.Target; }
        public string LogName { get; private set; }
        public string FromFile { get => this.meta.FromFile; }

        public IManifest Source { get; }

        /// <summary>
        /// Load content pack patch data
        /// </summary>
        /// <returns></returns>
        public T LoadData<T>()
        {
            return this.contentPack.Load<T>(this.meta.FromFile);
        }

        public bool FromAssetExists()
        {
            return !string.IsNullOrEmpty(this.meta.FromFile) && this.contentPack.HasFile(this.meta.FromFile);
        }
    }
}