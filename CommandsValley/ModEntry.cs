using StardewModdingAPI;
using StardewValley;
using System;

namespace ShivaGuy.Stardew.CommandsValley
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            Helper.ConsoleCommands.Add("cv_cc", "", IsCommunityCenterComplete);
            Helper.ConsoleCommands.Add("cv_joja", "", IsJojaMartComplete);
        }

        private void IsCommunityCenterComplete(string command, string[] args)
        {
            var mailReceived = Game1.MasterPlayer.mailReceived;
            bool ccCompleted = mailReceived.Contains("ccIsComplete")
                || (mailReceived.Contains("ccCraftsRoom")
                    && mailReceived.Contains("ccVault")
                    && mailReceived.Contains("ccFishTank")
                    && mailReceived.Contains("ccBoilerRoom")
                    && mailReceived.Contains("ccPantry")
                    && mailReceived.Contains("ccBulletin"));

            Monitor.Log(ccCompleted ? "Yes" : "No", LogLevel.Info);
        }

        private void IsJojaMartComplete(string commmand, string[] args)
        {
            var mailReceived = Game1.MasterPlayer.mailReceived;
            bool jojaCompleted = mailReceived.Contains("JojaMember")
                || (mailReceived.Contains("jojaCraftsRoom")
                    && mailReceived.Contains("jojaVault")
                    && mailReceived.Contains("jojaFishTank")
                    && mailReceived.Contains("jojaBoilerRoom")
                    && mailReceived.Contains("jojaPantry"));

            Monitor.Log(jojaCompleted ? "Yes" : "No", LogLevel.Info);
        }
    }
}
