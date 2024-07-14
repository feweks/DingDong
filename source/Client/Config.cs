using System.Numerics;
using Newtonsoft.Json;
using Raylib_cs;

namespace Dong.Client;

struct DeringConfig
{
    public int[] Resolution;
    public string Title;
    public int Fps;
    public ConfigFlags[] Flags;
    public string WebServer;
}

class Config
{
    public static DeringConfig AppConfig;
    public static int Width;
    public static int Height;
    public static Vector2 ResolutionScale = Vector2.One;
    public static void Load()
    {
        AppConfig = JsonConvert.DeserializeObject<DeringConfig>(File.ReadAllText("config.json"));

        Width = AppConfig.Resolution[0];
        Height = AppConfig.Resolution[1];
    }
}