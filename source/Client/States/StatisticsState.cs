using System.Numerics;
using Raylib_cs;

namespace Dong.Client.States;

enum StatType
{
    GameOpened,
    GamePlayed,
    Points,
    Highscore
}

class StatisticsState : State
{
    Texture2D menu;
    Font fnt1;
    Font fnt2;
    public static Dictionary<StatType, string> statistics = new Dictionary<StatType, string>();

    public static void Init()
    {
        string[] stats = File.ReadAllText("resources/stats.txt").Split(";");

        statistics[StatType.GameOpened] = stats[0];
        statistics[StatType.GamePlayed] = stats[1];
        statistics[StatType.Points] = stats[2];
        statistics[StatType.Highscore] = stats[3];
    }
    public override void Create()
    {
        base.Create();

        menu = Assets.GetTexture("resources/menu.png");
        fnt1 = Assets.GetFont("resources/shogun.ttf");
        fnt2 = Assets.GetFont("resources/vt323.ttf");
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
        {
            Raylib.PlaySound(MainMenuState.backSfx);
            Program.SwitchState(new MainMenuState());
        }
    }

    public override void Render()
    {
        base.Render();

        Raylib.DrawTexture(menu, 0, 0, Color.White);

        Utils.ScreenCenterFont(fnt1, "STATYSTYKI", 104, out int titleX, out int titleY);

        Raylib.DrawTextEx(fnt1, "STATYSTYKI", new Vector2(titleX, 50), 104, 1, new Color(22, 126, 209, 255));

        Utils.ScreenCenterFont(fnt2, $"OTWARCIA GRY: {statistics[StatType.GameOpened]}", 72, out int gameX, out int gameY);
        Raylib.DrawTextEx(fnt2, $"OTWARCIA GRY: {statistics[StatType.GameOpened]}", new Vector2(gameX, 175), 72, 1, Color.White);

        Utils.ScreenCenterFont(fnt2, $"ZAGRANE GRY: {statistics[StatType.GamePlayed]}", 72, out int playX, out int playY, 1);
        Raylib.DrawTextEx(fnt2, $"ZAGRANE GRY: {statistics[StatType.GamePlayed]}", new Vector2(playX, 275), 72, 1, Color.White);

        Utils.ScreenCenterFont(fnt2, $"WSZYSTKIE PUNKTY: {statistics[StatType.Points]}", 72, out int pointsX, out int poinstY);
        Raylib.DrawTextEx(fnt2, $"WSZYSTKIE PUNKTY: {statistics[StatType.Points]}", new Vector2(pointsX, 375), 72, 1, Color.White);

        Utils.ScreenCenterFont(fnt2, $"HIGHSCORE: {statistics[StatType.Highscore]}", 72, out int highscoreX, out int highscoreY);
        Raylib.DrawTextEx(fnt2, $"HIGHSCORE: {statistics[StatType.Highscore]}", new Vector2(highscoreX, 475), 72, 1, Color.White);
    }

    public static void SaveStat(StatType type, string stat)
    {
        statistics[type] = stat;
    }

    public static void Save()
    {
        string stats = statistics[StatType.GameOpened] + ";" + statistics[StatType.GamePlayed] + ";" + statistics[StatType.Points] + ";" + statistics[StatType.Highscore];
        File.WriteAllText("resources/stats.txt", stats);
    }
}