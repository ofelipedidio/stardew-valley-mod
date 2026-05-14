namespace StardewValleyMod;

internal sealed class DailyPrediction
{
    private DailyPrediction(
        int seed,
        int nextDayOfMonth,
        uint currentDaysPlayed,
        uint nextDaysPlayed,
        ulong seedSaveComponent,
        uint stepsTaken,
        string dishItemId,
        string dishName,
        int dishQuantity,
        double luck,
        IReadOnlyList<string> debugLines,
        bool isAvailable,
        string unavailableReason)
    {
        this.Seed = seed;
        this.NextDayOfMonth = nextDayOfMonth;
        this.CurrentDaysPlayed = currentDaysPlayed;
        this.NextDaysPlayed = nextDaysPlayed;
        this.SeedSaveComponent = seedSaveComponent;
        this.StepsTaken = stepsTaken;
        this.DishItemId = dishItemId;
        this.DishName = dishName;
        this.DishQuantity = dishQuantity;
        this.Luck = luck;
        this.LuckLabel = FormatLuck(luck);
        this.DebugLines = debugLines;
        this.IsAvailable = isAvailable;
        this.UnavailableReason = unavailableReason;
    }

    public int Seed { get; }

    public int NextDayOfMonth { get; }

    public uint CurrentDaysPlayed { get; }

    public uint NextDaysPlayed { get; }

    public ulong SeedSaveComponent { get; }

    public uint StepsTaken { get; }

    public string DishItemId { get; }

    public string DishName { get; }

    public int DishQuantity { get; }

    public double Luck { get; }

    public string LuckLabel { get; }

    public IReadOnlyList<string> DebugLines { get; }

    public bool IsAvailable { get; }

    public string UnavailableReason { get; }

    public static DailyPrediction Available(
        int seed,
        int nextDayOfMonth,
        uint currentDaysPlayed,
        uint nextDaysPlayed,
        ulong seedSaveComponent,
        uint stepsTaken,
        string dishItemId,
        string dishName,
        int dishQuantity,
        double luck,
        IReadOnlyList<string> debugLines)
    {
        return new DailyPrediction(
            seed,
            nextDayOfMonth,
            currentDaysPlayed,
            nextDaysPlayed,
            seedSaveComponent,
            stepsTaken,
            dishItemId,
            dishName,
            dishQuantity,
            luck,
            debugLines,
            isAvailable: true,
            unavailableReason: "");
    }

    public static DailyPrediction Unavailable(string reason)
    {
        return new DailyPrediction(
            seed: 0,
            nextDayOfMonth: 0,
            currentDaysPlayed: 0,
            nextDaysPlayed: 0,
            seedSaveComponent: 0,
            stepsTaken: 0,
            dishItemId: "",
            dishName: "",
            dishQuantity: 0,
            luck: 0,
            debugLines: Array.Empty<string>(),
            isAvailable: false,
            unavailableReason: reason);
    }

    public static string FormatLuck(double luck)
    {
        return luck.ToString("+0.000;-0.000;+0.000", System.Globalization.CultureInfo.InvariantCulture);
    }
}
