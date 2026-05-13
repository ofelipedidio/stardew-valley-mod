using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace StardewValleyMod;

/// <summary>The mod entry point.</summary>
internal sealed class ModEntry : Mod
{
    private DailyRngPredictor? predictor;
    private HudRenderer? hudRenderer;
    private DailyPrediction? cachedPrediction;
    private PredictionInputs? cachedInputs;
    private string? lastWarningReason;

    /// <summary>The mod entry point, called after the mod is first loaded.</summary>
    /// <param name="helper">Provides simplified APIs for writing mods.</param>
    public override void Entry(IModHelper helper)
    {
        this.predictor = new DailyRngPredictor();
        this.hudRenderer = new HudRenderer();

        helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
        helper.Events.GameLoop.DayStarted += this.OnDayStarted;
        helper.Events.GameLoop.ReturnedToTitle += this.OnReturnedToTitle;
        helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        helper.Events.Display.RenderedHud += this.OnRenderedHud;

        helper.ConsoleCommands.Add(
            "daily_rng_debug",
            "Print the current next-day saloon dish and daily luck RNG prediction details.",
            this.OnDailyRngDebugCommand);
    }

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        DailyPrediction prediction = this.RefreshPrediction(force: true);

        this.Monitor.Log(
            $"[Daily RNG] Save loaded. SaveId={Game1.uniqueIDForThisGame}, DaysPlayed={Game1.stats.DaysPlayed}, StepsTaken={Game1.stats.StepsTaken}, Date={Game1.currentSeason} {Game1.dayOfMonth}, year {Game1.year}.",
            LogLevel.Info);

        if (prediction.IsAvailable)
        {
            this.Monitor.Log(
                $"[Daily RNG] Initial prediction: Seed={prediction.Seed}, Dish={prediction.DishName} x{prediction.DishQuantity}, Luck={prediction.LuckLabel}.",
                LogLevel.Info);
        }
    }

    private void OnDayStarted(object? sender, DayStartedEventArgs e)
    {
        DailyPrediction? previousPrediction = this.cachedPrediction?.IsAvailable == true
            ? this.cachedPrediction
            : null;

        if (previousPrediction is null)
        {
            this.Monitor.Log("[Daily RNG] Day started. No cached pre-sleep prediction was available for comparison.", LogLevel.Info);
            this.RefreshPrediction(force: true);
            return;
        }

        string actualDishItemId = Game1.dishOfTheDay?.ItemId ?? "";
        string actualDishName = Game1.dishOfTheDay?.DisplayName ?? "(none)";
        int actualDishQuantity = Game1.dishOfTheDay?.Stack ?? 0;
        double actualLuck = Game1.player.team.sharedDailyLuck.Value;

        bool dishMatches =
            actualDishItemId == previousPrediction.DishItemId
            && actualDishQuantity == previousPrediction.DishQuantity;
        bool luckMatches = Math.Abs(actualLuck - previousPrediction.Luck) < 0.0000000001;

        this.Monitor.Log(
            $"[Daily RNG] Day started comparison: Predicted dish={previousPrediction.DishName} x{previousPrediction.DishQuantity} ({previousPrediction.DishItemId}), actual dish={actualDishName} x{actualDishQuantity} ({actualDishItemId}) ({(dishMatches ? "MATCH" : "MISMATCH")}); predicted luck={previousPrediction.LuckLabel}, actual luck={DailyPrediction.FormatLuck(actualLuck)} ({(luckMatches ? "MATCH" : "MISMATCH")}).",
            LogLevel.Info);

        if (!dishMatches || !luckMatches)
        {
            this.Monitor.Log("[Daily RNG] Mismatch diagnostics:", LogLevel.Info);
            foreach (string line in previousPrediction.DebugLines)
                this.Monitor.Log($"[Daily RNG] {line}", LogLevel.Info);
        }

        this.RefreshPrediction(force: true);
    }

    private void OnReturnedToTitle(object? sender, ReturnedToTitleEventArgs e)
    {
        this.cachedPrediction = null;
        this.cachedInputs = null;
        this.lastWarningReason = null;
    }

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (!Context.IsWorldReady)
            return;

        PredictionInputs inputs = PredictionInputs.ReadCurrent();
        if (this.cachedInputs == inputs)
            return;

        this.RefreshPrediction(force: true);
    }

    private void OnRenderedHud(object? sender, RenderedHudEventArgs e)
    {
        if (!Context.IsWorldReady || this.hudRenderer is null)
            return;

        DailyPrediction? prediction = this.cachedPrediction;
        if (prediction?.IsAvailable != true)
            return;

        this.hudRenderer.Draw(e.SpriteBatch, prediction);
    }

    private void OnDailyRngDebugCommand(string command, string[] args)
    {
        if (this.predictor is null)
            return;

        DailyPrediction prediction = this.predictor.PredictNextDay(includeDebugLines: true);
        this.LogPredictionWarningIfNeeded(prediction);

        this.Monitor.Log("[Daily RNG Debug]", LogLevel.Info);
        if (!prediction.IsAvailable)
        {
            this.Monitor.Log($"Unavailable: {prediction.UnavailableReason}", LogLevel.Info);
            return;
        }

        foreach (string line in prediction.DebugLines)
            this.Monitor.Log(line, LogLevel.Info);
    }

    private DailyPrediction RefreshPrediction(bool force)
    {
        if (this.predictor is null)
            return DailyPrediction.Unavailable("Predictor is not initialized.");

        if (!force && this.cachedPrediction is not null)
            return this.cachedPrediction;

        DailyPrediction prediction = this.predictor.PredictNextDay(includeDebugLines: true);
        this.cachedPrediction = prediction;
        this.cachedInputs = Context.IsWorldReady ? PredictionInputs.ReadCurrent() : null;
        this.LogPredictionWarningIfNeeded(prediction);
        return prediction;
    }

    private void LogPredictionWarningIfNeeded(DailyPrediction prediction)
    {
        if (prediction.IsAvailable)
        {
            this.lastWarningReason = null;
            return;
        }

        string reason = prediction.UnavailableReason;
        if (this.lastWarningReason == reason)
            return;

        this.lastWarningReason = reason;
        this.Monitor.Log($"[Daily RNG] Prediction unavailable: {reason}", LogLevel.Warn);
    }
}
