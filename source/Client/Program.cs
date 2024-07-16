using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Dong.Client.States;
using Raylib_cs;

namespace Dong.Client;

class Program
{
    public static float DeltaTime { get; internal set; } = 0f;

    static RenderTexture2D renderTexture;
    [DllImport("User32.dll", CharSet = CharSet.Unicode)]
    public static extern int ToUnicode(uint keyCode, uint scanCode, byte[] keyboard, StringBuilder buffer, int bufferSize, uint flags);
    public static State? CurState { get; internal set; }
    public static void Initialize()
    {
        Config.Load();

        Discord.Init();
        foreach (var flag in Config.AppConfig.Flags)
        {
            Raylib.SetConfigFlags(flag);
        }
        Raylib.InitWindow(Config.Width, Config.Height, Config.AppConfig.Title);
        Raylib.InitAudioDevice();
        Raylib.SetTargetFPS(Config.AppConfig.Fps);
        Raylib.SetExitKey(KeyboardKey.Null);

        renderTexture = Raylib.LoadRenderTexture(Config.Width, Config.Height);

        StatisticsState.Init();
        StatisticsState.SaveStat(StatType.GameOpened, (int.Parse(StatisticsState.statistics[StatType.GameOpened]) + 1).ToString());

        SwitchState(new MainMenuState());
    }

    public static void Run()
    {
        while (!Raylib.WindowShouldClose())
        {
            Update();
            Render();
        }

        StatisticsState.Save();
    }

    private static void Update()
    {
        DeltaTime = Raylib.GetFrameTime();

        CurState?.Update(DeltaTime);

        Config.ResolutionScale = new Vector2((float)Raylib.GetScreenWidth() / Config.Width, (float)Raylib.GetScreenHeight() / Config.Height);
    }

    private static void Render()
    {
        Raylib.BeginTextureMode(renderTexture);
        Raylib.ClearBackground(Color.Black);

        CurState?.Render();

        Raylib.EndTextureMode();

        Raylib.BeginDrawing();

        Raylib.DrawTexturePro(renderTexture.Texture, new Rectangle(0, 0, renderTexture.Texture.Width, -renderTexture.Texture.Height),
        new Rectangle(0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight()), Vector2.Zero, 0, Color.White);

        Raylib.DrawText($"{Raylib.GetFPS()} FPS", 5, 3, 24, Color.White);

        Raylib.EndDrawing();
    }

    public static void SwitchState(State nextState)
    {
        if (CurState != null)
        {
            CurState.Destroy();
            CurState = null;
        }
        CurState = nextState;
        CurState.Create();
    }
}