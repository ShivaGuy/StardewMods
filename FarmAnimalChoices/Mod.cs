using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using HarmonyLib;
using StardewModdingAPI;
using StardewObject = StardewValley.Object;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace ShivaGuy.Stardew.FarmAnimalChoices
{
    public class Mod : StardewModdingAPI.Mod
    {
        public override void Entry(IModHelper helper)
        {
            PurchaseAnimalsMenu__Patch.Initialize(Monitor);

            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
                original: AccessTools.Constructor(typeof(PurchaseAnimalsMenu), new Type[] { typeof(List<StardewObject>) }),
                prefix: new HarmonyMethod(typeof(PurchaseAnimalsMenu__Patch), nameof(PurchaseAnimalsMenu__Patch.ctor__Prefix)),
                postfix: new HarmonyMethod(typeof(PurchaseAnimalsMenu__Patch), nameof(PurchaseAnimalsMenu__Patch.ctor__Postfix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(PurchaseAnimalsMenu), nameof(PurchaseAnimalsMenu.getAnimalTitle)),
                prefix: new HarmonyMethod(typeof(PurchaseAnimalsMenu__Patch), nameof(PurchaseAnimalsMenu__Patch.getAnimalTitle__Prefix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(PurchaseAnimalsMenu), nameof(PurchaseAnimalsMenu.getAnimalDescription)),
                prefix: new HarmonyMethod(typeof(PurchaseAnimalsMenu__Patch), nameof(PurchaseAnimalsMenu__Patch.getAnimalDescription__Prefix))
            );
        }
    }

    internal static class PurchaseAnimalsMenu__Patch
    {
        private static IMonitor? Monitor { get; set; }
        private static readonly int MaxCols = 3;
        private static readonly Dictionary<string, string> FarmAnimalsData = Game1.content.Load<Dictionary<string, string>>("Data/FarmAnimals");
        private static readonly Texture2D WhiteChickenTexture = Game1.content.Load<Texture2D>("Animals/White Chicken");
        private static readonly Texture2D BrownChickenTexture = Game1.content.Load<Texture2D>("Animals/Brown Chicken");
        private static readonly Texture2D BlueChickenTexture = Game1.content.Load<Texture2D>("Animals/Blue Chicken");
        private static readonly Texture2D VoidChickenTexture = Game1.content.Load<Texture2D>("Animals/Void Chicken");
        private static readonly Texture2D GoldenChickenTexture = Game1.content.Load<Texture2D>("Animals/Golden Chicken");
        private static readonly Texture2D DuckTexture = Game1.content.Load<Texture2D>("Animals/Duck");
        private static readonly string CoopIsRequired = Game1.content.LoadString("Strings/StringsFromCSFiles:Utility.cs.5926");
        private static readonly string BigCoopIsRequired = Game1.content.LoadString("Strings/StringsFromCSFiles:Utility.cs.5940");
        private static readonly string BarnIsRequired = Game1.content.LoadString("Strings/StringsFromCSFiles:Utility.cs.5931");
        private static readonly string ChickenDescription = (
            Game1.content.LoadString("Strings/StringsFromCSFiles:PurchaseAnimalsMenu.cs.11334") +
            Environment.NewLine +
            Game1.content.LoadString("Strings/StringsFromCSFiles:PurchaseAnimalsMenu.cs.11335"));
        private static readonly string CowDescription = (Game1.content.LoadString("Strings/StringsFromCSFiles:PurchaseAnimalsMenu.cs.11343") +
            Environment.NewLine +
            Game1.content.LoadString("Strings/StringsFromCSFiles:PurchaseAnimalsMenu.cs.11344")
        );

        private static readonly string ItIsSecret = /* ...It's a secret. */
            Game1.content.LoadString("Characters/Dialogue/Abigail:winter_Sat6").Split("#$b#")[1].Split('$')[0];
        private static int CalcLeftNeighborId(int id) { return (id % MaxCols) == 0 ? -1 : id - 1; }
        private static int CalcRightNeighborId(int id) { return (id % MaxCols) == (MaxCols - 1) ? -1 : id + 1; }
        private static int CalcTopNeighborId(int id) { return id - MaxCols; }
        private static int CalcBottomNeighborId(int id) { return id + MaxCols; }

        private static string? GetAnimalName(string key)
        {
            FarmAnimalsData.TryGetValue(key, out string? rawData);
            return rawData?.Split('/')?[25];
        }

        public static void Initialize(IMonitor monitor)
        {
            Monitor = monitor;
        }

        public static bool getAnimalTitle__Prefix(string name, ref string __result)
        {
            if (!name.EndsWith(" Chicken") && !name.EndsWith(" Cow"))
                return true;

            __result = GetAnimalName(name) ?? name;
            return false;
        }

        public static bool getAnimalDescription__Prefix(string name, ref string __result)
        {
            if (name.EndsWith(" Chicken"))
                __result = ChickenDescription;
            else if (name.EndsWith(" Cow"))
                __result = CowDescription;
            else
                return true;
            return false;
        }

        public static void ctor__Prefix(List<StardewObject> stock)
        {
            int totalItems = stock.Count;
            foreach (var item in stock)
            {
                switch (item.Name)
                {
                    case "Chicken":
                        totalItems += 2; // brown, blue chicken
                        break;
                    case "Dairy Cow":
                        totalItems += 1; // brown cow
                        break;
                }
            }
            totalItems += 2; // void, golden chicken
            PurchaseAnimalsMenu.menuHeight = (totalItems / MaxCols) * 85 + 64;
        }

        public static void ctor__Postfix(PurchaseAnimalsMenu __instance)
        {
            List<ClickableTextureComponent> animalsToPurchase = new();
            List<Point> textureOffset = new();

            bool deluxeCoopConstructed = Game1.getFarm().isBuildingConstructed("Deluxe Coop");
            bool bigCoopConstructed = Game1.getFarm().isBuildingConstructed("Big Coop") || deluxeCoopConstructed;
            bool coopConstructed = Game1.getFarm().isBuildingConstructed("Coop") || bigCoopConstructed;

            bool deluxeBarnConstructed = Game1.getFarm().isBuildingConstructed("Deluxe Barn");
            bool bigBarnConstructed = Game1.getFarm().isBuildingConstructed("Big Barn") || deluxeBarnConstructed;
            bool barnConstructed = Game1.getFarm().isBuildingConstructed("Barn") || bigBarnConstructed;

            Rectangle calculatedLater = new Rectangle();

            int itemCount = 0;

            for (int i = 0; i < __instance.animalsToPurchase.Count; i++, itemCount++)
            {
                var cc = __instance.animalsToPurchase[i];

                switch (cc.item.Name)
                {
                    case "Chicken":

                        // white chicken
                        StardewObject whiteChicken = new(100, 1, false, price: 400)
                        {
                            Name = "White Chicken",
                            Type = coopConstructed ? null : CoopIsRequired,
                            displayName = "White Chicken"
                        };
                        animalsToPurchase.Add(new ClickableTextureComponent(
                            whiteChicken.salePrice().ToString(),
                            bounds: calculatedLater,
                            label: null,
                            hoverText: whiteChicken.Name,
                            texture: WhiteChickenTexture,
                            sourceRect: new Rectangle(0, 0, 16, 16),
                            scale: 4.0f,
                            whiteChicken.Type == null)
                        {
                            item = whiteChicken
                        });
                        textureOffset.Add(new Point(32, 0));

                        // brown chicken
                        StardewObject brownChicken = new(100, 1, false, price: 400)
                        {
                            Name = "Brown Chicken",
                            Type = coopConstructed ? null : CoopIsRequired,
                            displayName = "Brown Chicken"
                        };
                        animalsToPurchase.Add(new ClickableTextureComponent(
                            brownChicken.salePrice().ToString(),
                            bounds: calculatedLater,
                            label: null,
                            hoverText: brownChicken.Name,
                            texture: BrownChickenTexture,
                            sourceRect: new Rectangle(0, 0, 16, 16),
                            scale: 4.0f,
                            brownChicken.Type == null)
                        {
                            item = brownChicken
                        });
                        textureOffset.Add(new Point(32, 0));

                        // blue chicken
                        StardewObject blueChicken = new(100, 1, false, price: 2500)
                        {
                            Name = "Blue Chicken",
                            Type = coopConstructed && /* player has seen Shane's 8 heart even */ Game1.player.eventsSeen.Contains(3900074)
                                    ? null : CoopIsRequired + Environment.NewLine + "& " + ItIsSecret,
                            displayName = "Blue Chicken"
                        };
                        animalsToPurchase.Add(new ClickableTextureComponent(
                            blueChicken.salePrice().ToString(),
                            bounds: calculatedLater,
                            label: null,
                            hoverText: blueChicken.Name,
                            texture: BlueChickenTexture,
                            sourceRect: new Rectangle(0, 0, 16, 16),
                            scale: 4.0f,
                            blueChicken.Type == null)
                        {
                            item = blueChicken
                        });
                        textureOffset.Add(new Point(32, 0));
                        break;

                    case "Dairy Cow":

                        // white cow
                        StardewObject whiteCow = new(100, 1, false, 750)
                        {
                            Name = "White Cow",
                            Type = barnConstructed ? null : BarnIsRequired,
                            displayName = "White Cow"
                        };
                        animalsToPurchase.Add(new ClickableTextureComponent(
                            whiteCow.salePrice().ToString(),
                            bounds: calculatedLater,
                            label: null,
                            hoverText: whiteCow.Name,
                            texture: Game1.mouseCursors,
                            sourceRect: new Rectangle(32, 448, 32, 16),
                            scale: 4.0f,
                            whiteCow.Type == null)
                        {
                            item = whiteCow
                        });
                        textureOffset.Add(new Point(0, 0));

                        // brown cow
                        StardewObject brownCow = new(100, 1, false, 750)
                        {
                            Name = "Brown Cow",
                            Type = barnConstructed ? null : BarnIsRequired,
                            displayName = "Brown Cow"
                        };
                        animalsToPurchase.Add(new ClickableTextureComponent(
                            brownCow.salePrice().ToString(),
                            bounds: calculatedLater,
                            label: null,
                            hoverText: brownCow.Name,
                            texture: Game1.mouseCursors,
                            sourceRect: new Rectangle(64, 480, 32, 16),
                            scale: 4.0f,
                            brownCow.Type == null)
                        {
                            item = brownCow
                        });
                        textureOffset.Add(new Point(0, 0));
                        break;

                    case "Duck":

                        // duck (without egg)
                        StardewObject duck = new(100, 1, false, price: 600)
                        {
                            Name = "Duck",
                            Type = bigCoopConstructed ? null : BigCoopIsRequired,
                            displayName = "Duck"
                        };
                        animalsToPurchase.Add(new ClickableTextureComponent(
                            duck.salePrice().ToString(),
                            bounds: calculatedLater,
                            label: null,
                            hoverText: duck.Name,
                            texture: DuckTexture,
                            sourceRect: new Rectangle(0, 0, 16, 16),
                            scale: 4.0f,
                            duck.Type == null)
                        {
                            item = duck
                        });
                        textureOffset.Add(new Point(32, 0));
                        break;

                    default:
                        animalsToPurchase.Add(cc);
                        textureOffset.Add(new Point(0, 0));
                        break;
                }
            }

            // void chicken
            StardewObject void_chicken = new(100, 1, false, price: 2500)
            {
                Name = "Void Chicken",
                Type = bigCoopConstructed && /* player has access to Krobus's shop */ Game1.player.mailReceived.Contains("OpenedSewer")
                        ? null : BigCoopIsRequired + Environment.NewLine + "& " + ItIsSecret,
                displayName = "Void Chicken"
            };
            animalsToPurchase.Add(new ClickableTextureComponent(
                void_chicken.salePrice().ToString(),
                bounds: calculatedLater,
                label: null,
                hoverText: void_chicken.Name,
                texture: VoidChickenTexture,
                sourceRect: new Rectangle(0, 0, 16, 16),
                scale: 4.0f,
                void_chicken.Type == null)
            {
                item = void_chicken
            });
            textureOffset.Add(new Point(32, 0));

            // golden chicken
            StardewObject golden_chicken = new(100, 1, false, price: 50000)
            {
                Name = "Golden Chicken",
                Type = bigCoopConstructed && /* player has achieved perfection */ Game1.player.mailReceived.Contains("Farm_Eternal")
                        ? null : BigCoopIsRequired + Environment.NewLine + "& " + ItIsSecret,
                displayName = "Golden Chicken"
            };
            animalsToPurchase.Add(new ClickableTextureComponent(
                golden_chicken.salePrice().ToString(),
                bounds: calculatedLater,
                label: null,
                hoverText: golden_chicken.Name,
                texture: GoldenChickenTexture,
                sourceRect: new Rectangle(0, 0, 16, 16),
                scale: 4.0f,
                golden_chicken.Type == null)
            {
                item = golden_chicken
            });
            textureOffset.Add(new Point(32, 0));

            for (int i = 0; i < animalsToPurchase.Count; i++)
            {
                animalsToPurchase[i].bounds = new Rectangle(
                    __instance.xPositionOnScreen + IClickableMenu.borderWidth + ((i % MaxCols) * 128),
                    __instance.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + (IClickableMenu.borderWidth / 2) + (i / MaxCols) * 85,
                    128,
                    64);
                animalsToPurchase[i].bounds.Offset(textureOffset[i]);

                animalsToPurchase[i].myID = i;
                animalsToPurchase[i].leftNeighborID = CalcLeftNeighborId(i);
                animalsToPurchase[i].rightNeighborID = CalcRightNeighborId(i);
                animalsToPurchase[i].upNeighborID = CalcTopNeighborId(i);
                animalsToPurchase[i].downNeighborID = CalcBottomNeighborId(i);
            }

            __instance.animalsToPurchase.Clear();
            animalsToPurchase.ForEach(item => __instance.animalsToPurchase.Add(item));
        }
    }
}
