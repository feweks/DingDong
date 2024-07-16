using System.Numerics;
using System.Text;
using Dong.Client.Netcode;
using Dong.Server;
using Raylib_cs;

namespace Dong.Client.States;

enum MainMenuSubstate
{
    Check,
    Host,
    Join
}

class InputLabelState
{
    public string Text;
    public bool Selected;
}

class LabelState
{
    public bool SoundPlayed;
}

class MainMenuState : State
{
    Font titleFont;
    static Font vtFont;
    Texture2D menu;
    Texture2D menulabel;
    Version version = new Version(1, 1, 0);
    static Dictionary<uint, InputLabelState> inputLabelStates = new Dictionary<uint, InputLabelState>();
    static Dictionary<uint, LabelState> labelStates = new Dictionary<uint, LabelState>();
    MainMenuSubstate state = MainMenuSubstate.Check;
    Version latestVersion = new Version(0, 0, 0);
    static Sound menuConfirmSfx;
    static Sound menuSelectSfx;
    public static Sound backSfx;
    Music music;
    string ipCache = string.Empty;
    string nickCache = string.Empty;
    public override void Create()
    {
        base.Create();

        titleFont = Assets.GetFont("resources/shogun.ttf");
        vtFont = Assets.GetFont("resources/vt323.ttf");
        menu = Assets.GetTexture("resources/menu.png");
        menulabel = Assets.GetTexture("resources/menu_bg.png");
        menuSelectSfx = Assets.GetSound("resources/menu_select.ogg");
        menuConfirmSfx = Assets.GetSound("resources/menu_confirm.ogg");
        backSfx = Assets.GetSound("resources/menu_back.ogg");

        music = Raylib.LoadMusicStream("resources/menu_music.ogg");
        music.Looping = true;
        Raylib.PlayMusicStream(music);

        var r = GetWebServerData().Result;

        if (r != null)
        {
            string[] data = r.Split(".");
            latestVersion = new Version(int.Parse(data[0]), int.Parse(data[1]), int.Parse(data[2]));
        }

        string cache = File.ReadAllText("resources/cache.txt");
        ipCache = cache.Split("\n")[0];
        nickCache = cache.Split("\n")[1];

        Discord.ChangePresenece("W menu głównym", null, "icon", true);
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        Raylib.UpdateMusicStream(music);

        if (Raylib.IsKeyPressed(KeyboardKey.Space))
        {
            DongServer.Start(5036);
            DongClient.Connect("feweks", 0, "77.87.220.23", 5036);
            Program.SwitchState(new PlayState());
        }

        if (Raylib.IsKeyPressed(KeyboardKey.LeftControl))
        {
            DongClient.Connect("skuter", 1, "77.87.220.23", 5036);
            Thread.Sleep(100);
            Program.SwitchState(new PlayState());
        }
    }

    public override void Render()
    {
        base.Render();

        Raylib.DrawTexture(menu, 0, 0, Color.White);

        Utils.ScreenCenterFont(titleFont, "DING DONG", 102, out int titleX, out int titleY);
        Raylib.DrawTextEx(titleFont, "DING DONG", new Vector2(titleX, 75), 102, 1, new Color(22, 126, 209, 255));

        Utils.ScreenCenterTexture(menulabel, out int labelX, out int labelY);
        labelY += 75;
        Raylib.DrawTexture(menulabel, labelX, labelY, Color.White);

        if (state == MainMenuSubstate.Check)
        {
            int hostLabel = DrawLabel(1, "HOSTUJ", -30);
            int joinLabel = DrawLabel(2, "DOŁĄCZ", 40);
            int statisticsLabel = DrawLabel(10, "STATYSYKI", 40 + 70);
            int exitLabel = DrawLabel(11, "WYJDŹ", 40 + (70 * 2));

            if (hostLabel == 1)
            {
                state = MainMenuSubstate.Host;
            }
            else if (joinLabel == 1)
            {
                state = MainMenuSubstate.Join;
            }
            else if (statisticsLabel == 1)
            {
                Program.SwitchState(new StatisticsState());
            }
            else if (exitLabel == 1)
            {
                Environment.Exit(0);
            }
        }
        else if (state == MainMenuSubstate.Host)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Escape))
            {
                state = MainMenuSubstate.Check;
                Raylib.PlaySound(backSfx);
            }

            string ip = DrawLabelInput(0, "IP: ", ipCache, 0);
            string nick = DrawLabelInput(1, "NICK: ", nickCache, 75);
            int host = DrawLabel(3, "HOSTUJ", 75 * 2);

            if (nick != string.Empty && ip != string.Empty && host == 1)
            {
                DongServer.Start(ushort.Parse(ip.Split(":")[1]));
                DongClient.Connect(nick, 0, ip.Split(":")[0], ushort.Parse(ip.Split(":")[1]));
                string cache = ip + "\n" + nick;
                File.WriteAllText("resources/cache.txt", cache);
                Program.SwitchState(new PlayState());
            }
        }
        else if (state == MainMenuSubstate.Join)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Escape))
            {
                state = MainMenuSubstate.Check;
                Raylib.PlaySound(backSfx);
            }

            string ip = DrawLabelInput(10, "IP: ", ipCache, 0);
            string nick = DrawLabelInput(11, "NICK: ", nickCache, 75);
            int join = DrawLabel(4, "DOŁĄCZ", 75 * 2);

            if (ip != string.Empty && ip != string.Empty && join == 1)
            {
                DongClient.Connect(nick, 1, ip.Split(":")[0], ushort.Parse(ip.Split(":")[1]));
                Thread.Sleep(100);
                string cache = ip + "\n" + nick;
                File.WriteAllText("resources/cache.txt", cache);
                Program.SwitchState(new PlayState());
            }
        }

        if (version != latestVersion)
        {
            Raylib.DrawRectangle(0, 0, Config.Width, Config.Height, new Color(0, 0, 0, (int)(200 * 0.85f)));
            int ver = DrawLabel(5, $"Dostępna jest nowa wersja: {latestVersion}", 0, 500);
            if (ver == 1)
            {
                Raylib.OpenURL("https://github.com/feweks/DingDong/releases");
                Environment.Exit(0);
            }
        }

        Vector2 mainMenuMeasure = Raylib.MeasureTextEx(vtFont, $"feweks 2024 | Ding Dong {version}", 32, 1);
        Raylib.DrawTextEx(vtFont, $"feweks 2024 | Ding Dong {version}", new Vector2(5, Config.Height - mainMenuMeasure.Y - 5), 32, 1, Color.White);
    }

    public static int DrawLabel(uint id, string text, int offset = 0, int width = 200, int height = 50)
    {
        if (!labelStates.ContainsKey(id)) labelStates.Add(id, new LabelState() { SoundPlayed = false });

        int result = 0;

        Rectangle label = new Rectangle(0, 0, width, height);
        Utils.ScreenCenterRectangle(label, out int labelX, out int labelY);
        label.X = labelX;
        label.Y = labelY + offset;

        Color outlineTint = new Color(13, 94, 150, 255);
        Vector2 mousePos = Raylib.GetMousePosition();

        if (Raylib.CheckCollisionRecs(new Rectangle(label.X * Config.ResolutionScale.X, label.Y * Config.ResolutionScale.Y, label.Width * Config.ResolutionScale.X, label.Height * Config.ResolutionScale.Y), new Rectangle(mousePos, new Vector2(15, 15) * Config.ResolutionScale)))
        {
            outlineTint = Color.White;
            if (!labelStates[id].SoundPlayed)
            {
                Raylib.PlaySound(menuConfirmSfx);
                labelStates[id].SoundPlayed = true;
            }
            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                result = 1;
                Raylib.PlaySound(menuSelectSfx);
            }
        }
        else
        {
            labelStates[id].SoundPlayed = false;
        }

        Raylib.DrawRectangleRounded(label, 0.35f, 25, new Color(18, 101, 167, 255));
        Raylib.DrawRectangleRoundedLines(label, 0.35f, 25, 5, outlineTint);

        Vector2 textPos = Utils.ScreenCenterFont(vtFont, text, (int)(height * 0.65f), out int x, out int y) + new Vector2(0, offset);
        Raylib.DrawTextEx(vtFont, text, textPos, height * 0.65f, 1, Color.White);

        return result;
    }

    string DrawLabelInput(uint id, string staticText, string dynamicText, int offset, int width = 200, int height = 50)
    {
        if (!inputLabelStates.ContainsKey(id)) inputLabelStates.Add(id, new InputLabelState() { Selected = false, Text = dynamicText });

        int res = DrawLabel(id, staticText + inputLabelStates[id].Text, offset, width, height);
        if (Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            if (res == 1)
                inputLabelStates[id].Selected = true;
            else
                inputLabelStates[id].Selected = false;
        }

        if (inputLabelStates[id].Selected)
            UpdateInput(id);

        return inputLabelStates[id].Text;
    }

    void UpdateInput(uint id)
    {
        var key = Raylib.GetKeyPressed();

        switch (key)
        {
            case 340: break;
            case 259:
                try
                {

                    inputLabelStates[id].Text = inputLabelStates[id].Text.Substring(0, inputLabelStates[id].Text.Length - 1);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                break;
            case 59:
                {
                    inputLabelStates[id].Text += ":";
                    break;
                }
            case 46:
                {
                    inputLabelStates[id].Text += ".";
                    break;
                }
            default:
                {
                    StringBuilder bdr = new StringBuilder(256);
                    Program.ToUnicode((uint)key, 0, new byte[256], bdr, bdr.Capacity, 0);

                    string u = bdr.ToString();
                    if (Raylib.IsKeyDown(KeyboardKey.LeftShift))
                        u = u.ToUpper();

                    inputLabelStates[id].Text += u;
                    break;
                }
        }
    }

    async Task<string?> GetWebServerData()
    {
        var client = new HttpClient();

        string response;
        try
        {
            response = await client.GetStringAsync(Config.AppConfig.WebServer);
        }
        catch (Exception error)
        {
            Console.WriteLine($"Failed to retrieve data from webserver {Config.AppConfig.WebServer} (error: {error.Message})", ConsoleColor.Red);
            return null;
        }

        return response;
    }

    public override void Destroy()
    {
        base.Destroy();

        Raylib.StopMusicStream(music);
    }
}