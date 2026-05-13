using StardewValley;

namespace StardewValleyMod;

internal readonly record struct PredictionInputs(
    uint StepsTaken,
    uint DaysPlayed,
    int DayOfMonth,
    ulong SaveId)
{
    public static PredictionInputs ReadCurrent()
    {
        return new PredictionInputs(
            Game1.stats.StepsTaken,
            Game1.stats.DaysPlayed,
            Game1.dayOfMonth,
            Game1.uniqueIDForThisGame);
    }
}
