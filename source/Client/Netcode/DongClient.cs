using Dong.Client.States;
using Dong.Server;
using Riptide;

namespace Dong.Client.Netcode;

class DongClient
{
    public static Player player1 = new Player();
    public static Player player2 = new Player();
    public static Riptide.Client? client;
    public static int PlayerId;
    public static Ball ball = new Ball() { X = Config.Width / 2, Y = Config.Height / 2, Angle = 0 };
    public static Action? OnGameStarted;
    public static Action<int>? OnPlayerLeave;
    public static void Connect(string nick, int id, string ip, int port)
    {
        client = new Riptide.Client();
        client.MessageReceived += OnMessageRecieved;
        client.Connected += (s, ev) =>
        {
            Console.WriteLine("[CLIENT] Connected to server");
        };
        client.ConnectionFailed += (s, ev) =>
        {
            Console.WriteLine($"[CLIENT] Failed to connect to server [{ev.Reason}]");
        };

        client.Connect(ip + ":" + port, 5);

        Thread.Sleep(100);

        switch (id)
        {
            case 0:
                {
                    player1.Nick = nick;
                    player1.Id = id;
                    player1.Position = Config.Height / 2 - 150 / 2;
                    break;
                }
            case 1:
                {
                    player2.Nick = nick;
                    player2.Id = id;
                    player2.Position = Config.Height / 2 - 150 / 2;
                    break;
                }
        }
        PlayerId = id;
    }

    public static void Join()
    {
        string nick;
        int id;
        if (PlayerId == 0)
        {
            nick = player1.Nick;
            id = player1.Id;
        }
        else
        {
            nick = player2.Nick;
            id = player2.Id;
        }

        Message connectMsg = Message.Create(MessageSendMode.Reliable, (ushort)DongSrvMessageType.PlayerJoined);
        connectMsg.AddString(nick);
        connectMsg.AddInt(id);
        connectMsg.AddInt(Config.Height / 2 - 100);
        client?.Send(connectMsg);
    }

    public static void Send(Message msg)
    {
        client?.Send(msg);
    }

    private static void OnMessageRecieved(object? sender, MessageReceivedEventArgs ev)
    {
        switch ((DongSrvMessageType)ev.MessageId)
        {
            case DongSrvMessageType.PlayerJoined:
                {
                    string nick = ev.Message.GetString();
                    int id = ev.Message.GetInt();
                    int pos = ev.Message.GetInt();

                    if (id == 0)
                    {
                        player1.Position = pos;
                        player1.Nick = nick;
                        player1.Id = id;
                        Console.WriteLine(player1.Position);
                    }
                    else
                    {
                        player2.Position = pos;
                        player2.Nick = nick;
                        player2.Id = id;
                    }
                    break;
                }
            case DongSrvMessageType.StartGame:
                {
                    OnGameStarted?.Invoke();
                    break;
                }
            case DongSrvMessageType.UpdatePlayer:
                {
                    int pos = ev.Message.GetInt();
                    int id = ev.Message.GetInt();

                    if (id == 0)
                    {
                        player1.Position = pos;
                    }
                    else
                    {
                        player2.Position = pos;
                    }

                    break;
                }
            case DongSrvMessageType.ChangePoints:
                {
                    int points = ev.Message.GetInt();
                    int id = ev.Message.GetInt();

                    if (id == 0)
                    {
                        player1.Points = points;
                    }
                    else
                    {
                        player2.Points = points;
                    }

                    break;
                }
            case DongSrvMessageType.BallPos:
                {
                    int x = ev.Message.GetInt();
                    int y = ev.Message.GetInt();
                    float angle = ev.Message.GetFloat();

                    ball.X = x;
                    ball.Y = y;
                    ball.Angle = angle;

                    break;
                }
            case DongSrvMessageType.PlayerLeave:
                {
                    OnPlayerLeave?.Invoke(ev.Message.GetInt());

                    break;
                }
        }
    }
}