namespace PurrplingMod.Loader
{
    public interface IContentLoader
    {
        bool CanLoad(string assetName);
        T Load<T>(string assetName);
        void InvalidateCache();
    }
}