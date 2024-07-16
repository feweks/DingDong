using Riptide;

namespace Dong.Server;

enum DongSrvMessageType
{
    PlayerJoined = 1,
    StartGame = 2,
    UpdatePlayer = 3,
    ChangePoints = 5,
    BallPos = 6,
    PlayerLeave = 7,
    PlayMusic = 8,
    PlaySound = 9
}

class Player
{
    public string? Nick;
    public int Position;
    public int Id;
    public int Points;
}

class Ball
{
    public float Angle = 0;
    public int X = 0;
    public int Y = 0;
}

class DongServer
{
    public static Riptide.Server? server;
    static Player player1 = new Player();
    static Player player2 = new Player();
    static Connection con1;
    static Connection con2;
    static Ball ball = new Ball();
    public static void Start(ushort port)
    {
        server = new Riptide.Server();

        server.Start(port, 2);
        server.ClientConnected += OnClientConnected;
        server.MessageReceived += OnMessageRecieved;
        server.ClientDisconnected += (s, ev) =>
        {
            if (ev.Client == con1)
            {
                Message leave = Message.Create(MessageSendMode.Reliable, (int)DongSrvMessageType.PlayerLeave);
                leave.AddInt(0);
                con2.Send(leave);
            }
            else
            {
                Message leave = Message.Create(MessageSendMode.Reliable, (int)(DongSrvMessageType.PlayerLeave));
                leave.AddInt(1);
                con1.Send(leave);
            }
        };
        Thread srvThread = new Thread(new ThreadStart(Update))
        {
            IsBackground = true,
            Name = "Server"
        };
        srvThread.Start();
        Console.WriteLine("[SERVER] Server Started on port " + port);
    }

    private static void Update()
    {
        while (true)
        {
            try
            {
                server?.Update();
            }
            catch (Exception e) { }
            Thread.Sleep(10);
        }
    }

    private static void OnClientConnected(object? sender, ServerConnectedEventArgs ev)
    {
        Console.WriteLine("[SERVER] Client " + ev.Client + " connected to server");
    }

    private static void OnMessageRecieved(object? sender, MessageReceivedEventArgs ev)
    {
        if (ev.MessageId == 0) return;

        switch ((DongSrvMessageType)ev.MessageId)
        {
            case DongSrvMessageType.PlayerJoined:
                {
                    string nick = ev.Message.GetString();
                    int id = ev.Message.GetInt();
                    int pos = ev.Message.GetInt();
                    if (nick == null || nick == string.Empty || con1 == ev.FromConnection || con2 == ev.FromConnection)
                    {
                        break;
                    }

                    if (id == 0)
                    {
                        player1.Nick = nick;
                        player1.Id = id;
                        con1 = ev.FromConnection;
                    }
                    else
                    {
                        player2.Nick = nick;
                        player2.Id = id;
                        con2 = ev.FromConnection;

                        Message p2message = Message.Create(MessageSendMode.Reliable, (ushort)DongSrvMessageType.PlayerJoined);
                        p2message.AddString(player1.Nick);
                        p2message.AddInt(player1.Id);
                        p2message.AddInt(720 / 2 - 100);
                        ev.FromConnection.Send(p2message);

                        Message msg = Message.Create(MessageSendMode.Reliable, (ushort)DongSrvMessageType.PlayerJoined);
                        msg.AddString(player2.Nick);
                        msg.AddInt(player2.Id);
                        msg.AddInt(720 / 2 - 100);
                        con1.Send(msg);
                    }
                    Console.WriteLine($"[SERVER] Player {nick} joined with id {id}");

                    break;
                }
            case DongSrvMessageType.StartGame:
                {
                    Message msg = Message.Create(MessageSendMode.Reliable, (ushort)DongSrvMessageType.StartGame);

                    con2.Send(msg);
                    break;
                }
            case DongSrvMessageType.UpdatePlayer:
                {
                    int pos = ev.Message.GetInt();
                    int id = ev.Message.GetInt();

                    if (id == 0)
                    {
                        player1.Position = pos;
                        Message msg = Message.Create(MessageSendMode.Unreliable, (ushort)(DongSrvMessageType.UpdatePlayer));
                        msg.AddInt(pos);
                        msg.AddInt(0);
                        con2.Send(msg);
                    }
                    else
                    {
                        player2.Position = pos;
                        Message msg = Message.Create(MessageSendMode.Unreliable, (ushort)(DongSrvMessageType.UpdatePlayer));
                        msg.AddInt(pos);
                        msg.AddInt(1);
                        con1.Send(msg);
                    }

                    break;
                }
            case DongSrvMessageType.BallPos:
                {
                    int x = ev.Message.GetInt();
                    int y = ev.Message.GetInt();
                    float angle = ev.Message.GetFloat();

                    Message msg = Message.Create(MessageSendMode.Reliable, (ushort)DongSrvMessageType.BallPos);
                    msg.AddInt(x);
                    msg.AddInt(y);
                    msg.AddFloat(angle);

                    con2.Send(msg);
                    break;
                }
            case DongSrvMessageType.ChangePoints:
                {
                    int points = ev.Message.GetInt();
                    int id = ev.Message.GetInt();

                    Message msg = Message.Create(MessageSendMode.Reliable, (ushort)DongSrvMessageType.ChangePoints);
                    msg.AddInt(points);
                    msg.AddInt(id);
                    con2.Send(msg);

                    break;
                }
            case DongSrvMessageType.PlayMusic:
                {
                    Message msg = Message.Create(MessageSendMode.Reliable, (ushort)DongSrvMessageType.PlayMusic);
                    con2.Send(msg);
                    con1.Send(msg);
                    break;
                }
            case DongSrvMessageType.PlaySound:
                {
                    Message msg = Message.Create(MessageSendMode.Reliable, (ushort)DongSrvMessageType.PlaySound);
                    con2.Send(msg);
                    break;
                }
            default:
                {
                    Console.WriteLine("[SERVER] [ERROR] Unknown message type: " + ev.MessageId);
                    break;
                }
        }
    }
}