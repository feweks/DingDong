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

class MainMenuState : State
{
    Font titleFont;
    static Font vtFont;
    Texture2D menu;
    Texture2D menulabel;
    Version version = new Version(1, 0, 0);
    static Dictionary<uint, InputLabelState> inputLabelStates = new Dictionary<uint, InputLabelState>();
    MainMenuSubstate state = MainMenuSubstate.Check;
    Version latestVersion = new Version(0, 0, 0);
    public override void Create()
    {
        base.Create();

        titleFont = Assets.GetFont("resources/shogun.ttf");
        vtFont = Assets.GetFont("resources/vt323.ttf");
        menu = Assets.GetTexture("resources/menu.png");
        menulabel = Assets.GetTexture("resources/menu_bg.png");

        var r = GetWebServerData().Result;

        if (r != null)
        {
            string[] data = r.Split(".");
            latestVersion = new Version(int.Parse(data[0]), int.Parse(data[1]), int.Parse(data[2]));
        }
    }

    public override void Update(float dt)
    {
        base.Update(dt);

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
            int hostLabel = DrawLabel("HOSTUJ", 25);
            int joinLabel = DrawLabel("DOŁĄCZ", 125);

            if (hostLabel == 1)
            {
                state = MainMenuSubstate.Host;
            }
            else if (joinLabel == 1)
            {
                state = MainMenuSubstate.Join;
            }
        }
        else if (state == MainMenuSubstate.Host)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Escape))
            {
                state = MainMenuSubstate.Check;
            }

            string ip = DrawLabelInput(0, "IP: ", "", 0);
            string nick = DrawLabelInput(1, "NICK: ", "", 75);
            int host = DrawLabel("HOSTUJ", 75 * 2);

            if (nick != string.Empty && ip != string.Empty && host == 1)
            {
                DongServer.Start(ushort.Parse(ip.Split(":")[1]));
                DongClient.Connect(nick, 0, ip.Split(":")[0], ushort.Parse(ip.Split(":")[1]));
                Program.SwitchState(new PlayState());
            }
        }
        else if (state == MainMenuSubstate.Join)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Escape))
            {
                state = MainMenuSubstate.Check;
            }

            string ip = DrawLabelInput(10, "IP: ", "", 0);
            string nick = DrawLabelInput(11, "NICK: ", "", 75);
            int join = DrawLabel("DOŁĄCZ", 75 * 2);

            if (ip != string.Empty && ip != string.Empty && join == 1)
            {
                DongClient.Connect(nick, 1, ip.Split(":")[0], ushort.Parse(ip.Split(":")[1]));
                Thread.Sleep(100);
                Program.SwitchState(new PlayState());
            }
        }

        if (version != latestVersion)
        {
            Raylib.DrawRectangle(0, 0, Config.Width, Config.Height, new Color(0, 0, 0, (int)(200 * 0.85f)));
            int ver = DrawLabel($"Dostępna jest nowa wersja: {latestVersion}", 0, 500);
            if (ver == 1)
            {
                Environment.Exit(0);
            }
        }

        Vector2 mainMenuMeasure = Raylib.MeasureTextEx(vtFont, $"feweks 2024 | Ding Dong {version}", 32, 1);
        Raylib.DrawTextEx(vtFont, $"feweks 2024 | Ding Dong {version}", new Vector2(5, Config.Height - mainMenuMeasure.Y - 5), 32, 1, Color.White);
    }

    public static int DrawLabel(string text, int offset = 0, int width = 200, int height = 50)
    {
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
            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                result = 1;
            }
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

        int res = DrawLabel(staticText + inputLabelStates[id].Text, offset, width, height);
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
}