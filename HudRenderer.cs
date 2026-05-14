using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace StardewValleyMod;

internal sealed class HudRenderer
{
    private const int Padding = 8;
    private Texture2D? backgroundPixel;

    public void Draw(SpriteBatch spriteBatch, DailyPrediction prediction)
    {
        SpriteFont font = Game1.smallFont;
        string[] lines =
        {
            "Next Day RNG",
            $"Steps: {prediction.StepsTaken}",
            $"Seed: {prediction.Seed}",
            $"Skip day: {prediction.NextDayOfMonth}",
            $"Dish: {prediction.DishName} x{prediction.DishQuantity}",
            $"Luck: {prediction.LuckLabel}"
        };

        Vector2 textSize = Vector2.Zero;
        foreach (string line in lines)
        {
            Vector2 lineSize = font.MeasureString(line);
            textSize.X = Math.Max(textSize.X, lineSize.X);
            textSize.Y += lineSize.Y;
        }

        Rectangle safeArea = Game1.graphics.GraphicsDevice.Viewport.TitleSafeArea;
        int x = Math.Max(safeArea.Left, 12);
        int y = Math.Max(safeArea.Top, 12);
        int width = (int)Math.Ceiling(textSize.X) + Padding * 2;
        int height = (int)Math.Ceiling(textSize.Y) + Padding * 2;
        Rectangle background = new(x, y, width, height);

        Texture2D pixel = this.GetBackgroundPixel(Game1.graphics.GraphicsDevice);
        spriteBatch.Draw(pixel, background, Color.Black * 0.65f);

        Vector2 position = new(x + Padding, y + Padding);
        foreach (string line in lines)
        {
            spriteBatch.DrawString(font, line, position, Color.White);
            position.Y += font.MeasureString(line).Y;
        }
    }

    private Texture2D GetBackgroundPixel(GraphicsDevice graphicsDevice)
    {
        if (this.backgroundPixel is not null && !this.backgroundPixel.IsDisposed)
            return this.backgroundPixel;

        this.backgroundPixel = new Texture2D(graphicsDevice, 1, 1);
        this.backgroundPixel.SetData(new[] { Color.White });
        return this.backgroundPixel;
    }
}
