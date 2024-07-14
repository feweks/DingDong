using System.Numerics;
using Dong.Client.Netcode;
using Dong.Server;
using Raylib_cs;
using Riptide;

namespace Dong.Client.States;

class PlayState : State
{
    Texture2D table;
    Texture2D ball;
    Rectangle mouseHitbox;
    Font fnt;
    Random randomEngine;
    public static int ballForceX;
    public static int ballForceY;
    public static int ballSpeed = 0;
    bool gameStarted = false;
    int ballX = 0;
    int ballY = 0;
    float ballAngle = 0;
    int playerLeft = -1;
    public override void Create()
    {
        base.Create();

        DongClient.Join();

        table = Assets.GetTexture("resources/table.png");
        ball = Assets.GetTexture("resources/ball.png");
        fnt = Assets.GetFont("resources/vt323.ttf");

        randomEngine = new Random();

        DongClient.OnGameStarted += () =>
        {
            gameStarted = true;
            Console.WriteLine("Game started");
        };

        DongClient.OnPlayerLeave += (id) =>
        {
            playerLeft = id;
        };
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        try
        {
            DongClient.client?.Update();
        }
        catch (Exception error) { }

        Vector2 mousePos = Raylib.GetMousePosition();
        mouseHitbox = new Rectangle(mousePos.X, mousePos.Y, new Vector2(15, 15) * Config.ResolutionScale);

        if (gameStarted)
        {
            int change = 0;
            if (Raylib.IsKeyDown(KeyboardKey.S))
            {
                change += (int)(500 * dt);
            }
            else if (Raylib.IsKeyDown(KeyboardKey.W))
            {
                change -= (int)(500 * dt);
            }

            if (DongClient.PlayerId == 0)
            {
                try
                {
                    Message msg = Message.Create(MessageSendMode.Unreliable, (int)DongSrvMessageType.UpdatePlayer);
                    DongClient.player1.Position = Math.Clamp(DongClient.player1.Position + change, 35, Config.Height - 150 - 35);
                    msg.AddInt(DongClient.player1.Position);
                    msg.AddInt(0);
                    DongClient.Send(msg);
                }
                catch (Exception error)
                {
                    Console.WriteLine("Failed to send updateplayer");
                }
            }
            else
            {
                try
                {
                    Message msg = Message.Create(MessageSendMode.Unreliable, (int)DongSrvMessageType.UpdatePlayer);
                    DongClient.player2.Position = Math.Clamp(DongClient.player2.Position + change, 35, Config.Height - 150 - 35);
                    msg.AddInt(DongClient.player2.Position);
                    msg.AddInt(1);
                    DongClient.Send(msg);
                }
                catch (Exception error)
                {
                    Console.WriteLine("Failed to send updateplayer");
                }
            }

            if (DongClient.PlayerId == 0)
            {
                if (ballForceX == 0 && ballForceY == 0)
                {
                    randomEngine = new Random();

                    ballForceX = randomEngine.Next(-1, 1);
                    if (ballForceX == 0) ballForceX = 1;
                    ballForceY = randomEngine.Next(-1, 1);
                    if (ballForceY == 0) ballForceY = 1;
                }

                DongClient.ball.X += (int)(ballForceX * dt * (350 + ballSpeed * 10));
                DongClient.ball.Y += (int)(ballForceY * dt * (350 + ballSpeed * 10));
                DongClient.ball.Angle += (int)(200 * dt);
            }

            var p1hbox = new Rectangle(35, DongClient.player1.Position, 15, 150);
            var p2hbox = new Rectangle(Config.Width - 15 - 35, DongClient.player2.Position, 15, 150);
            var ballhbox = new Rectangle(DongClient.ball.X - (ball.Width * 0.25f * 0.5f), DongClient.ball.Y - (ball.Height * 0.25f * 0.5f), ball.Width * 0.25f, ball.Height * 0.25f);

            if (DongClient.ball.Y < ball.Height * 0.25f * 0.5f) ballForceY = -ballForceY;
            if (DongClient.ball.Y > Config.Height - ball.Height * 0.25f * 0.5f) ballForceY = -ballForceY;

            if (DongClient.PlayerId == 0)
            {
                if (Raylib.CheckCollisionRecs(p1hbox, ballhbox))
                {
                    ballSpeed += 1;

                    ballForceX = -ballForceX;

                    DongClient.ball.X += 5;
                }

                if (Raylib.CheckCollisionRecs(p2hbox, ballhbox))
                {
                    ballSpeed += 1;

                    ballForceX = -ballForceX;

                    DongClient.ball.X -= 5;
                }

                if (DongClient.ball.X < ball.Width * 0.25f * 0.5f)
                {
                    DongClient.player2.Points++;
                    ballForceX = 0;
                    ballForceY = 0;


                    DongClient.ball.X = Config.Width / 2;
                    DongClient.ball.Y = Config.Height / 2;

                    ballSpeed = 0;

                    Message pMsg = Message.Create(MessageSendMode.Reliable, (int)DongSrvMessageType.ChangePoints);
                    pMsg.AddInt(DongClient.player2.Points);
                    pMsg.AddInt(1);
                    DongClient.Send(pMsg);
                }
                else if (DongClient.ball.X > Config.Width - ball.Width * 0.25f * 0.5f)
                {
                    DongClient.player1.Points++;

                    ballForceX = 0;
                    ballForceY = 0;

                    DongClient.ball.X = Config.Width / 2;
                    DongClient.ball.Y = Config.Height / 2;

                    ballSpeed = 0;

                    Message pMsg = Message.Create(MessageSendMode.Reliable, (int)DongSrvMessageType.ChangePoints);
                    pMsg.AddInt(DongClient.player1.Points);
                    pMsg.AddInt(0);
                    DongClient.Send(pMsg);
                }

                Message ballPos = Message.Create(MessageSendMode.Reliable, (ushort)DongSrvMessageType.BallPos);
                ballPos.AddInt(DongClient.ball.X);
                ballPos.AddInt(DongClient.ball.Y);
                ballPos.AddFloat(DongClient.ball.Angle);
                DongClient.Send(ballPos);
            }
        }
    }

    public override void Render()
    {
        base.Render();

        Raylib.DrawTexture(table, 0, 0, Color.White);

        Utils.ScreenCenterFont(fnt, "VS", 46, out int vsX, out int vsY);
        Vector2 vsMeasure = Raylib.MeasureTextEx(fnt, "VS", 46, 1);
        Raylib.DrawTextEx(fnt, "VS", new Vector2(vsX, 30), 46, 1, Color.White);

        Vector2 p1Measure = Raylib.MeasureTextEx(fnt, DongClient.player1.Nick + ": " + DongClient.player1.Points, 46, 1);
        Raylib.DrawTextEx(fnt, DongClient.player1.Nick + ": " + DongClient.player1.Points, new Vector2(vsX - (vsMeasure.X / 2) - p1Measure.X, 30), 46, 1, Color.White);

        if (DongClient.player2.Nick != null)
        {
            //Vector2 p2Measure = Raylib.MeasureTextEx(fnt, DongClient.player2.Nick, 46, 1);
            Raylib.DrawTextEx(fnt, DongClient.player2.Nick + ": " + DongClient.player2.Points, new Vector2(vsX + vsMeasure.X + 15, 30), 46, 1, Color.White);

            Raylib.DrawRectangleRounded(new Rectangle(Config.Width - 15 - 35, DongClient.player2.Position, 15, 150), 1f, 15, Color.White);
        }

        Raylib.DrawRectangleRounded(new Rectangle(35, DongClient.player1.Position, 15, 150), 1f, 15, Color.White);

        if (gameStarted)
        {
            if (DongClient.PlayerId == 0)
                Raylib.DrawTexturePro(ball, new Rectangle(0, 0, ball.Width, ball.Height), new Rectangle(DongClient.ball.X, DongClient.ball.Y, ball.Width * 0.25f, ball.Height * 0.25f), new Vector2(336 * 0.25f * 0.5f, 323 * 0.25f * 0.5f), DongClient.ball.Angle, Color.White);
            else
            {
                ballX = (int)Utils.Lerp(ballX, DongClient.ball.X, 35f * Program.DeltaTime);
                ballY = (int)Utils.Lerp(ballY, DongClient.ball.Y, 35f * Program.DeltaTime);
                ballAngle = Utils.Lerp(ballAngle, DongClient.ball.Angle, 20f * Program.DeltaTime);
                Raylib.DrawTexturePro(ball, new Rectangle(0, 0, ball.Width, ball.Height), new Rectangle(ballX, ballY, ball.Width * 0.25f, ball.Height * 0.25f), new Vector2(336 * 0.25f * 0.5f, 323 * 0.25f * 0.5f), ballAngle, Color.White);
            }
        }

        if (DongClient.PlayerId == 1 && DongClient.player1.Nick == string.Empty || DongClient.player1.Nick == null)
        {
            int res = MainMenuState.DrawLabel("NIE UDAŁO POŁĄCZYĆ SIĘ Z SERWEREM", 0, 500);
            if (res == 1)
            {
                Program.SwitchState(new MainMenuState());
                DongClient.player2.Nick = string.Empty;
            }
        }

        if (DongClient.PlayerId == 0 && !gameStarted)
        {
            Utils.ScreenCenterFont(fnt, "START", 56, out int startX, out int startY);

            Color tint = Color.White;
            if (Utils.CheckCollisionRectText(startX, startY, fnt, "START", 56, mouseHitbox, 1))
            {
                tint = new Color(165, 165, 165, 255);
                if (DongClient.player2.Nick == null)
                {
                    Utils.ScreenCenterFont(fnt, "MUSISZ MIEĆ 2 GRACZA BY ROZPOCZĄĆ GRĘ", 46, out int wX, out int wY);
                    Vector2 startMeasure = Raylib.MeasureTextEx(fnt, "START", 56, 1);
                    Raylib.DrawTextEx(fnt, "MUSISZ MIEĆ 2 GRACZA BY ROZPOCZĄĆ GRĘ", new Vector2(wX, startY + startMeasure.Y), 46, 1, Color.Red);
                }
                else
                {
                    if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                    {
                        DongClient.Send(Message.Create(MessageSendMode.Reliable, (ushort)DongSrvMessageType.StartGame));
                        gameStarted = true;
                    }
                }
            }

            Raylib.DrawTextEx(fnt, "START", new Vector2(startX, startY), 56, 1, tint);
        }

        if (playerLeft == 0)
        {
            int res = MainMenuState.DrawLabel($"{DongClient.player1.Nick} WYSZEDŁ", 0, 500);
            gameStarted = false;
            if (res == 1)
            {
                Program.SwitchState(new MainMenuState());
                DongClient.player1.Nick = string.Empty;
                DongClient.player2.Nick = string.Empty;
                DongClient.client.Disconnect();
            }
        }
        else if (playerLeft == 1)
        {
            int res = MainMenuState.DrawLabel($"{DongClient.player2.Nick} WYSZEDŁ", 0, 500);
            gameStarted = false;
            if (res == 1)
            {
                Program.SwitchState(new MainMenuState());
                DongClient.player1.Nick = string.Empty;
                DongClient.player2.Nick = string.Empty;
                DongClient.client.Disconnect();
                DongServer.server.Stop();
            }
        }
    }

    public override void Destroy()
    {
        base.Destroy();
    }
}