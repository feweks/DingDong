using DiscordRPC;

class Discord
{
    static DiscordRpcClient discordClient;
    public static void Init()
    {
        discordClient = new DiscordRpcClient(File.ReadAllText("discord_appid.txt"));
        discordClient.Initialize();
    }

    public static void ChangePresenece(string details, string? state = null, string? image = null, bool timeStamp = false)
    {
        Timestamps? time = null;
        if (timeStamp)
        {
            time = Timestamps.Now;
        }

        discordClient.SetPresence(new RichPresence()
        {
            Details = details,
            State = state,
            Timestamps = time,
            Assets = new Assets()
            {
                LargeImageKey = image
            }
        });
    }
}