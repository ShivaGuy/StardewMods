using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using System;
using static ShivaGuy.Stardew.FarmAnimalChoices.ModEntry;

namespace ShivaGuy.Stardew.FarmAnimalChoices.Patch
{
    internal static class Patch_FarmAnimal
    {
        private static string animalType = "";

        private static bool AnimalIsColored(string type)
        {
            return (type.Contains("Chicken") && (type.Contains("White") || type.Contains("Brown") || type.Contains("Blue")))
                || (type.Contains("Cow") && (type.Contains("White") || type.Contains("Brown")));
        }

        private static bool SimilarAnimals(string type1, string type2)
        {
            return (type1.Contains("Chicken") && type2.Contains("Chicken"))
                || (type1.Contains("Cow") && type2.Contains("Cow"))
                || (type1.Equals(type2));
        }

        public static void Apply(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Constructor(typeof(FarmAnimal), new Type[] { typeof(string), typeof(long), typeof(long) }),
                prefix: new HarmonyMethod(typeof(Patch_FarmAnimal), nameof(Patch_FarmAnimal.ctor_Prefix)),
                postfix: new HarmonyMethod(typeof(Patch_FarmAnimal), nameof(Patch_FarmAnimal.ctor_Postfix)));
        }

        public static void ctor_Prefix(string type)
        {
            // Avoid type reset by aedenthorn.BulkAnimalPurchase
            if (AnimalIsColored(type))
                animalType = type;
        }

        public static void ctor_Postfix(FarmAnimal __instance)
        {
            if (!AnimalIsColored(__instance.type.Value))
                return;

            if (SimilarAnimals(__instance.type.Value, animalType) && !animalType.Equals(__instance.type.Value))
            {
                Logger.Log($"Changing animal type from {__instance.type.Value} to {animalType}");
                __instance.type.Value = animalType;
                __instance.reloadData();
            }
        }
    }
}
