using HarmonyLib;
using StardewModdingAPI;

namespace NpcAdventure.Internal.Patching
{
    internal interface IPatch
    {
        string Name { get; }
        bool Applied { get; }
        void Apply(Harmony harmony, IMonitor monitor);
    }
}
