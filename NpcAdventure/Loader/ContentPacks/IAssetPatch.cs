using StardewModdingAPI;

namespace NpcAdventure.Loader.ContentPacks
{
    public interface IAssetPatch
    {
        string Action { get; }
        string FromFile { get; }
        string LogName { get; }
        string Target { get; }

        IManifest Source { get; }

        bool FromAssetExists();
        T LoadData<T>();
    }
}