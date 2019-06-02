using System.Collections.Generic;

namespace PurrplingMod.Loader
{
    public interface IContentLoader
    {
        bool CanLoad(string assetName);
        T Load<T>(string assetName);
        Dictionary<string, string> LoadStrings(string stringsAssetName);
        void InvalidateCache();
    }
}