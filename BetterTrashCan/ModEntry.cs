using System;
using StardewModdingAPI;
using HarmonyLib;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Tools;
using StardewObject = StardewValley.Object;

namespace BetterTrashCan
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            Patch.Initialize(Helper.ReadConfig<ModConfig>(), Monitor);

            var original = AccessTools.Method(typeof(Utility), nameof(Utility.getTrashReclamationPrice));
            var prefix = new HarmonyMethod(typeof(Patch), nameof(Patch.Prefix));

            new Harmony(ModManifest.UniqueID).Patch(original, prefix);
        }
    }

    internal static class Patch
    {
        private static ModConfig Config;
        private static IMonitor Monitor;

        internal static void Initialize(ModConfig config, IMonitor monitor)
        {
            Config = config;
            Monitor = monitor;
        }

        internal static bool Prefix(Item i, Farmer f, out int __result)
        {
            __result = -1;

            double sellPercentage = f.trashCanLevel / 4.0;

            if (Config.progression == Progression.Exponential)
                sellPercentage = f.trashCanLevel > 0 ? Math.Pow(3.164, f.trashCanLevel) / 100 : 0;

            if (i.canBeTrashed() && i is not Wallpaper && i is not Furniture)
            {
                if (i is StardewObject obj && !obj.bigCraftable.Value)
                {
                    __result = (int)(i.Stack * (obj.sellToStorePrice(-1L) * sellPercentage));
                }
                else if (i is MeleeWeapon || i is Ring || i is Boots)
                {
                    __result = (int)(i.Stack * (i.salePrice() * sellPercentage));
                }
            }

            return false;
        }
    }
}
