using StardewValley;

namespace StardewValleyMod;

internal readonly record struct PredictionInputs(
    uint StepsTaken,
    uint DaysPlayed,
    uint NextDaysPlayed,
    int DayOfMonth,
    int NextDayOfMonth,
    ulong SaveId,
    ulong SeedSaveComponent)
{
    public static PredictionInputs ReadCurrent()
    {
        int nextDayOfMonth = Game1.dayOfMonth + 1;
        if (nextDayOfMonth > 28)
            nextDayOfMonth = 1;

        return new PredictionInputs(
            Game1.stats.StepsTaken,
            Game1.stats.DaysPlayed,
            Game1.stats.DaysPlayed + 1,
            Game1.dayOfMonth,
            nextDayOfMonth,
            Game1.uniqueIDForThisGame,
            Game1.uniqueIDForThisGame / 100UL);
    }
}
