using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Objects;

namespace StardewValleyMod;

internal sealed class DailyRngPredictor
{
    public DailyPrediction PredictNextDay(bool includeDebugLines)
    {
        if (!Context.IsWorldReady)
            return DailyPrediction.Unavailable("World is not ready.");

        if (Game1.player is null)
            return DailyPrediction.Unavailable("Game1.player is null.");

        if (Game1.netWorldState?.Value is null)
            return DailyPrediction.Unavailable("Game1.netWorldState is unavailable.");

        ulong saveId = Game1.uniqueIDForThisGame;
        ulong seedSaveComponent = saveId / 100UL;
        uint currentDaysPlayed = Game1.stats.DaysPlayed;
        uint nextDaysPlayed = currentDaysPlayed + 1;
        uint currentSteps = Game1.stats.StepsTaken;
        int currentDayOfMonth = Game1.dayOfMonth;
        int nextDayOfMonth = currentDayOfMonth + 1;
        if (nextDayOfMonth > 28)
            nextDayOfMonth = 1;

        List<string> debugLines = includeDebugLines ? new List<string>() : new List<string>(capacity: 0);

        int seed = Utility.CreateRandomSeed(
            seedSaveComponent,
            nextDaysPlayed * 10.0 + 1.0,
            currentSteps);

        Random rng = Utility.CreateRandom(seed);

        this.AddDebug(
            debugLines,
            includeDebugLines,
            $"SaveId: {saveId}",
            $"SaveId / 100UL: {seedSaveComponent}",
            $"Current dayOfMonth: {currentDayOfMonth}",
            $"Next dayOfMonth used for skip count: {nextDayOfMonth}",
            $"Current DaysPlayed: {currentDaysPlayed}",
            $"Next DaysPlayed used for seed: {nextDaysPlayed}",
            $"StepsTaken: {currentSteps}",
            $"Seed: {seed}");

        for (int i = 0; i < nextDayOfMonth; i++)
            rng.Next();

        if (includeDebugLines)
            debugLines.Add($"Skip draws: {nextDayOfMonth}");

        string itemId;
        bool forbidden;
        int dishRollCount = 0;
        do
        {
            itemId = rng.Next(194, 240).ToString();
            forbidden = Utility.IsForbiddenDishOfTheDay(itemId);
            dishRollCount++;

            if (includeDebugLines)
            {
                debugLines.Add($"Dish roll {dishRollCount}: itemId={itemId}");
                debugLines.Add($"Forbidden: {forbidden}");
            }
        }
        while (forbidden);

        double largeRangeRoll = rng.NextDouble();
        bool largeRange = largeRangeRoll < 0.08;
        int maxExclusive = 4 + (largeRange ? 10 : 0);
        int count = rng.Next(1, maxExclusive);
        string dishName = ItemRegistry.GetDataOrErrorItem("(O)" + itemId).DisplayName;

        this.AddDebug(
            debugLines,
            includeDebugLines,
            $"Dish count large-range roll: {largeRangeRoll:0.#################} (< 0.08 => {largeRange})",
            $"Dish count maxExclusive: {maxExclusive}",
            $"Dish count roll: {count}");

        bool foundFriend = Utility.TryGetRandom(
            Game1.player.friendshipData,
            out string friend,
            out Friendship friendship,
            rng);

        bool isSpouse = foundFriend && Game1.player.spouse == friend;
        bool hasMailKey = foundFriend && DataLoader.Mail(Game1.content).ContainsKey(friend);
        bool consumedFriendshipMailRng = false;
        bool friendshipMailChanceResult = false;

        if (foundFriend)
        {
            consumedFriendshipMailRng = true;
            double friendshipMailChance = (double)(friendship.Points / 250) * 0.1;
            friendshipMailChanceResult = rng.NextBool(friendshipMailChance);

            this.AddDebug(
                debugLines,
                includeDebugLines,
                $"Friendship TryGetRandom: true",
                $"Friendship candidate: {friend}",
                $"Friendship points: {friendship.Points}",
                $"Friendship mail chance: {friendshipMailChance:0.#################}",
                $"Friendship candidate is spouse: {isSpouse}",
                $"Friendship mail key exists: {hasMailKey}",
                $"Friendship mail RNG consumed: {consumedFriendshipMailRng}",
                $"Friendship mail chance result: {friendshipMailChanceResult}");
        }
        else
        {
            this.AddDebug(
                debugLines,
                includeDebugLines,
                $"Friendship TryGetRandom: false",
                $"Friendship candidate: (none)",
                $"Friendship mail chance: (not rolled)",
                $"Friendship mail RNG consumed: {consumedFriendshipMailRng}");
        }

        this.ReplayFarmerDayUpdateRandomDraws(rng, debugLines, includeDebugLines);

        int luckRoll = rng.Next(-100, 101);
        double luck = Math.Min(0.10000000149011612, luckRoll / 1000.0);

        this.AddDebug(
            debugLines,
            includeDebugLines,
            $"Luck roll: {luckRoll}",
            $"Predicted dish: {dishName} x{count} ({itemId})",
            $"Predicted luck: {DailyPrediction.FormatLuck(luck)}");

        return DailyPrediction.Available(
            seed,
            nextDayOfMonth,
            currentDaysPlayed,
            nextDaysPlayed,
            seedSaveComponent,
            currentSteps,
            itemId,
            dishName,
            count,
            luck,
            debugLines);
    }

    private void ReplayFarmerDayUpdateRandomDraws(Random rng, List<string> debugLines, bool includeDebugLines)
    {
        Farmer player = Game1.player;

        double rarecrowSocietyRoll = rng.NextDouble();
        this.AddDebug(
            debugLines,
            includeDebugLines,
            $"Farmer.dayupdate RarecrowSociety roll consumed: {rarecrowSocietyRoll:0.#################}");

        if (player.shirtItem.Value is null || player.pantsItem.Value is null)
        {
            this.AddDebug(
                debugLines,
                includeDebugLines,
                "Farmer.dayupdate cursed mannequin rolls skipped: player is missing shirt or pants item.");
            return;
        }

        if (player.currentLocation is not (FarmHouse or IslandFarmHouse or Shed))
        {
            this.AddDebug(
                debugLines,
                includeDebugLines,
                $"Farmer.dayupdate cursed mannequin rolls skipped: current location is {player.currentLocation?.NameOrUniqueName ?? "(none)"}.");
            return;
        }

        int cursedMannequinRollCount = 0;
        foreach (StardewValley.Object obj in player.currentLocation.netObjects.Values)
        {
            if (obj is not Mannequin mannequin)
                continue;

            bool isCursed =
                DataLoader.Mannequins(Game1.content).TryGetValue(mannequin.ItemId, out StardewValley.GameData.MannequinData? data)
                && data?.Cursed == true;
            if (!isCursed)
                continue;

            cursedMannequinRollCount++;
            double roll = rng.NextDouble();
            this.AddDebug(
                debugLines,
                includeDebugLines,
                $"Farmer.dayupdate cursed mannequin roll {cursedMannequinRollCount}: {roll:0.#################}");
        }

        if (cursedMannequinRollCount == 0)
        {
            this.AddDebug(
                debugLines,
                includeDebugLines,
                "Farmer.dayupdate cursed mannequin rolls consumed: 0");
        }
    }

    private void AddDebug(List<string> lines, bool includeDebugLines, params string[] values)
    {
        if (!includeDebugLines)
            return;

        lines.AddRange(values);
    }
}
