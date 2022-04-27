namespace BetterTrashCan
{
    public class ModConfig
    {
        public Progression progression { get; set; } = Progression.Linear;
    }

    public enum Progression
    {
        Linear, Exponential
    }
}