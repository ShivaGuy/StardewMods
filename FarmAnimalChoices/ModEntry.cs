using GenericModConfigMenu;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using ShivaGuy.Stardew.FarmAnimalChoices.Patch;

namespace ShivaGuy.Stardew.FarmAnimalChoices
{
    public class ModEntry : Mod
    {
        public static ModConfig Config { get; private set; }
        public static IMonitor Logger { get; private set; }

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            Logger = Monitor;

            var harmony = new Harmony(ModManifest.UniqueID);

            Patch_FarmAnimal.Apply(harmony);
            Patch_PurchaseAnimalsMenu.Apply(harmony);

            Helper.Events.GameLoop.GameLaunched += InitModConfigMenu;
        }

        private void InitModConfigMenu(object sender, GameLaunchedEventArgs evt)
        {
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

            if (configMenu == null)
                return;

            configMenu.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Progression Mode",
                tooltip: () => "Recommended to keep it enabled if you want to follow the progression set by the game.",
                getValue: () => Config.ProgressionMode,
                setValue: value => Config.ProgressionMode = value
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Void Chicken Cost",
                tooltip: () => "Cost of a Void Chicken in Marnie's shop",
                getValue: () => Config.VoidChicken,
                setValue: value => Config.VoidChicken = value
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Golden Chicken Cost",
                tooltip: () => "Cost of a Golden Chicken in Marnie's shop",
                getValue: () => Config.GoldenChicken,
                setValue: value => Config.GoldenChicken = value
            );
        }
    }
}
