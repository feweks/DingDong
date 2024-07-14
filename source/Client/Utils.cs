
using System.Numerics;
using Dong.Client;
using Raylib_cs;

class Utils
{
    public static float Lerp(float a, float b, float r)
    {
        return a + r * (b - a);
    }

    public static Vector2 ScreenCenterTexture(Texture2D texture, out int x, out int y, float scaleX = 1f, float scaleY = 1f)
    {
        x = Config.Width / 2 - (int)(texture.Width * scaleX * 0.5f);
        y = Config.Height / 2 - (int)(texture.Height * scaleY * 0.5f);

        return new Vector2(x, y);
    }

    public static Vector2 ScreenCenterFont(Font font, string text, int size, out int x, out int y, int spacing = 1)
    {
        Vector2 measure = Raylib.MeasureTextEx(font, text, size, spacing);

        x = (int)(1280 / 2 - (measure.X / 2));
        y = (int)(720 / 2 - (measure.Y / 2));

        return new Vector2(x, y);
    }

    public static Vector2 ScreenCenterRectangle(Rectangle rec, out int x, out int y)
    {
        x = (int)(Config.Width / 2 - (rec.Width / 2));
        y = (int)(Config.Height / 2 - (rec.Height / 2));

        return new Vector2(x, y);
    }

    public static Vector2 ScreenCenterRectangle(int x, int y, int width, int height, out int posX, out int posY)
    {
        posX = Config.Width / 2 - (width / 2);
        posY = Config.Height / 2 - (height / 2);

        return new Vector2(posX, posY);
    }

    public static bool CheckCollisionRectText(float x, float y, Font font, string txt, int size, Rectangle rec, int spacing = 1)
    {
        Vector2 measure = Raylib.MeasureTextEx(font, txt, size, spacing);
        Rectangle fontRec = new Rectangle(x * Config.ResolutionScale.X, y * Config.ResolutionScale.Y, measure.X * Config.ResolutionScale.X, measure.Y * Config.ResolutionScale.Y);

        return Raylib.CheckCollisionRecs(fontRec, rec);
    }
}