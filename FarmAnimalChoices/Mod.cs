using Microsoft.Xna.Framework;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewObject = StardewValley.Object;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace FarmAnimalChoices
{
    public class Mod : StardewModdingAPI.Mod
    {
        public override void Entry(IModHelper helper)
        {
            Patch.Initialize(Monitor);

            Harmony.DEBUG = true;

            new Harmony(ModManifest.UniqueID).Patch(
                original: AccessTools.Constructor(typeof(PurchaseAnimalsMenu), new Type[] { typeof(List<StardewObject>) }),
                prefix: new HarmonyMethod(typeof(FarmAnimalChoices.Patch), nameof(Patch.PurchaseAnimalsMenu__Prefix)),
                postfix: new HarmonyMethod(typeof(FarmAnimalChoices.Patch), nameof(Patch.PurchaseAnimalsMenu__Postfix))
            );
        }
    }

    internal static class Patch
    {
        private static IMonitor? Monitor { get; set; }
        public static void Initialize(IMonitor monitor)
        {
            Monitor = monitor;
        }
        public static void PurchaseAnimalsMenu__Prefix(List<StardewObject> stock)
        {
            for (int i = 0; i < stock.Count; i++)
            {
                Monitor?.Log($"Prefix: {stock[i].Name}", LogLevel.Debug);
            }
        }

        public static void PurchaseAnimalsMenu__Postfix(PurchaseAnimalsMenu __instance)
        {
            for (int i = 0; i < __instance.animalsToPurchase.Count; i++)
            {
                Monitor?.Log($"Postfix: {__instance.animalsToPurchase[i].item.Name}", LogLevel.Debug);
            }
        }
    }
}
