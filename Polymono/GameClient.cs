using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using Polymono.Game;
using Polymono.Graphics;
using Polymono.Graphics.Components;
using Polymono.Networking;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Polymono
{
    class GameClient : AGameClient
    {
        // Game states
        public GameState State;
        // Camera data
        public Camera Camera;
        //public Label LabelTest;
        // Game objects
        public Board Board;
        public Dice Dice;
        #region Menus
        // Main menu
        public Menu MnuMain;
        public Button BtnCreate;
        public Button BtnJoin;
        public Button BtnExit;

        // Network lobby menu
        public Menu MnuNetwork;
        public Label LblNetName;
        public Textbox TxtNetName;
        public Label LblNetAddress;
        public Textbox TxtNetAddress;
        public Label LblNetPort;
        public Textbox TxtNetPort;
        public Button BtnNetJoin;
        public Button BtnNetCreate;
        public Button BtnNetBack;

        // Message menu
        public Menu MnuMessage;
        public Label LblMsgBox;
        public Textbox TxtMsgInput;
        public Button BtnMsgSend;

        // Game menu
        public Menu MnuGame;

        // Test menu
        public Menu MnuTest;
        public Label LblTest;
        public Button BtnTest;
        public Textbox TxtTest;

        // Player options menu
        // <Roll dice> <Purchase property> <Trade>
        public Menu MnuPlayerOptions;
        public Button BtnRollDice;
        public Button BtnPurchaseProperty;
        public Button BtnTrade;

        // Player jail menu
        // <Pay 50> <Use card>
        public Menu MnuPlayerJailOptions;
        public Button BtnPayJail;
        public Button BtnUseCard;

        // Trade menu
        public Menu MnuTrade;
        #endregion
        //Misc object
        public Skybox Skybox;
        public Light ActiveLight = new Light(new Vector3(0.0f, 5.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f));

        private Server _server;
        //private Client _client;
        public INetwork Network { set; get; }

        private bool _isServer = false;

        public GameClient(int playerCount) : base()
        {
            State = new GameState(playerCount);
            Camera = new Camera(new Vector3(0.0f, 1.0f, 0.0f));
        }

        protected override void LoadObjects()
        {
            // Skybox
            Skybox = new Skybox(Programs[ProgramID.Skybox], @"Resources\Objects\sphere.obj", false);
            Skybox.CreateBuffer();
            // Board
            Board = new Board(Programs[ProgramID.Full], Programs[ProgramID.Player], State);
            Board.Model.CreateBuffer();
            Models.Add(Board.Model.ID, Board.Model);
            // Players
            for (int i = 0; i < State.PlayerCount; i++)
            {
                Board.Players[i].Model.CreateBuffer();
                Models.Add(Board.Players[i].Model.ID, Board.Players[i].Model);
            }
            // Dice
            Dice = new Dice(Programs[ProgramID.Dice]);
            Dice.Model.CreateBuffer();
            Models.Add(Dice.Model.ID, Dice.Model);

            // Loads the controls of the program.
            LoadControls();

            // Reset hide/show states for all menus.
            Menu.HideAll();
            MnuMain.Show();
            MnuTest.Show();
        }

        protected void LoadControls()
        {
            #region Main menu
            MnuMain = new Menu();
            BtnCreate = new Button(Programs[ProgramID.Button],
                new Vector3(Width / 2 - 64, Height - 128, 0.0f), Vector3.Zero, Vector3.One,
                Controls, Models, MnuMain,
                128, 32, 2, Width, Height, Matrix4.Identity,
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.7f, 0.7f, 0.0f, 0.8f),
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.0f, 0.6f, 0.7f, 0.8f),
                () =>
                {
                    MnuMain.Hide();
                    MnuNetwork.Show();
                    LblNetAddress.Hide();
                    TxtNetAddress.Hide();
                    BtnNetJoin.Hide();
                    return Task.Delay(0);
                }, "Create game");
            BtnJoin = new Button(Programs[ProgramID.Button],
                new Vector3(Width / 2 - 64, Height - 192, 0.0f), Vector3.Zero, Vector3.One,
                Controls, Models, MnuMain,
                128, 32, 2, Width, Height, Matrix4.Identity,
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.7f, 0.7f, 0.0f, 0.8f),
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.0f, 0.6f, 0.7f, 0.8f),
                () =>
                {
                    MnuMain.Hide();
                    MnuNetwork.Show();
                    BtnNetCreate.Hide();
                    return Task.Delay(0);
                }, "Join game");
            BtnExit = new Button(Programs[ProgramID.Button],
                new Vector3(Width / 2 - 64, Height - 256, 0.0f), Vector3.Zero, Vector3.One,
                Controls, Models, MnuMain,
                128, 32, 2, Width, Height, Matrix4.Identity,
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.7f, 0.7f, 0.0f, 0.8f),
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.0f, 0.6f, 0.7f, 0.8f),
                () =>
                {
                    Exit();
                    return Task.Delay(0);
                }, "Exit");
            MnuMain.CreateBuffers();
            #endregion
            #region Test menu
            MnuTest = new Menu();
            LblTest = new Label(Programs[ProgramID.Label],
                Vector3.Zero, Vector3.Zero, Vector3.One,
                Controls, Models, MnuTest,
                92, 18, 2, Width, Height, Matrix4.Identity, new Color4(0.1f, 0.1f, 0.1f, 0.6f), "X:0 Y:0");
            BtnTest = new Button(Programs[ProgramID.Button],
                new Vector3(8.0f, 40.0f, 0.0f), Vector3.Zero, Vector3.One,
                Controls, Models, MnuTest,
                128, 32, 2, Width, Height, Matrix4.Identity,
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.7f, 0.7f, 0.0f, 0.8f),
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.0f, 0.6f, 0.7f, 0.8f),
                () =>
                {
                    Polymono.Print("Hello");
                    return Task.Delay(0);
                }, "Test button");
            TxtTest = new Textbox(Programs[ProgramID.Label],
                new Vector3(8.0f, 80.0f, 0.0f), Vector3.Zero, Vector3.One,
                Controls, Models, MnuTest,
                128, 32, 2, Width, Height, Matrix4.Identity,
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.7f, 0.7f, 0.0f, 0.8f),
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.0f, 0.6f, 0.7f, 0.8f),
                "Test Textbox");
            MnuTest.CreateBuffers();
            #endregion
            #region Server creation menu
            MnuNetwork = new Menu();
            LblNetName = new Label(Programs[ProgramID.Label],
                new Vector3((Width / 2) - 64, Height - (64 * 2) + 24, 0.0f), Vector3.Zero, Vector3.One,
                Controls, Models, MnuNetwork,
                92, 18, 2, Width, Height, Matrix4.Identity, new Color4(0.1f, 0.1f, 0.1f, 0.6f),
                "Enter name:");
            TxtNetName = new Textbox(Programs[ProgramID.Label],
                new Vector3((Width / 2) - 64, Height - (64 * 2), 0.0f), Vector3.Zero, Vector3.One,
                Controls, Models, MnuNetwork,
                128, 32, 2, Width, Height, Matrix4.Identity,
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.7f, 0.7f, 0.0f, 0.8f),
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.0f, 0.6f, 0.7f, 0.8f),
                "");
            LblNetAddress = new Label(Programs[ProgramID.Label],
                new Vector3((Width / 2) - 160, Height - (64 * 3) + 24, 0.0f), Vector3.Zero, Vector3.One,
                Controls, Models, MnuNetwork,
                108, 18, 2, Width, Height, Matrix4.Identity, new Color4(0.1f, 0.1f, 0.1f, 0.6f),
                "Network address:");
            TxtNetAddress = new Textbox(Programs[ProgramID.Label],
                new Vector3((Width / 2) - 160, Height - (64 * 3), 0.0f), Vector3.Zero, Vector3.One,
                Controls, Models, MnuNetwork,
                128, 32, 2, Width, Height, Matrix4.Identity,
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.7f, 0.7f, 0.0f, 0.8f),
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.0f, 0.6f, 0.7f, 0.8f),
                "");
            LblNetPort = new Label(Programs[ProgramID.Label],
                new Vector3((Width / 2) + 32, Height - (64 * 3) + 24, 0.0f), Vector3.Zero, Vector3.One,
                Controls, Models, MnuNetwork,
                92, 18, 2, Width, Height, Matrix4.Identity, new Color4(0.1f, 0.1f, 0.1f, 0.6f),
                "Network port:");
            TxtNetPort = new Textbox(Programs[ProgramID.Label],
                new Vector3((Width / 2) + 32, Height - (64 * 3), 0.0f), Vector3.Zero, Vector3.One,
                Controls, Models, MnuNetwork,
                128, 32, 2, Width, Height, Matrix4.Identity,
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.7f, 0.7f, 0.0f, 0.8f),
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.0f, 0.6f, 0.7f, 0.8f),
                "");
            BtnNetCreate = new Button(Programs[ProgramID.Button],
                new Vector3((Width / 2) - 64, Height - (64 * 4), 0.0f), Vector3.Zero, Vector3.One,
                Controls, Models, MnuNetwork,
                128, 32, 2, Width, Height, Matrix4.Identity,
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.7f, 0.7f, 0.0f, 0.8f), // Default model
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.0f, 0.6f, 0.7f, 0.8f), // Clicked model
                async () =>
                {
                    try
                    {
                        ISocket socket = new PolySocket(true);
                        int port = Convert.ToInt32(TxtNetPort.Text);
                        socket.Bind(port);
                        socket.Listen(10);
                        Polymono.Print("Server created: IPv6 enabled.");
                        ISocket remote = await socket.AcceptAsync();
                        Polymono.Print($"Client joined: {remote.GetSocket().RemoteEndPoint}");
                    }
                    catch (SocketException se)
                    {
                        Polymono.Error(se.Message);
                        Polymono.ErrorF(se.StackTrace);
                    }
                    catch (FormatException fe)
                    {
                        Polymono.Error("Port is not a readable number.");
                        Polymono.Error(fe.Message);
                        Polymono.ErrorF(fe.StackTrace);
                    }
                }, "Create Server");
            BtnNetJoin = new Button(Programs[ProgramID.Button],
                new Vector3((Width / 2) - 64, Height - (64 * 4), 0.0f), Vector3.Zero, Vector3.One,
                Controls, Models, MnuNetwork,
                128, 32, 2, Width, Height, Matrix4.Identity,
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.7f, 0.7f, 0.0f, 0.8f), // Default model
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.0f, 0.6f, 0.7f, 0.8f), // Clicked model
                async () =>
                {
                    try
                    {
                        ISocket socket = new PolySocket(true);
                        int port = Convert.ToInt32(TxtNetPort.Text);
                        string address = TxtNetAddress.Text;
                        Polymono.Print("Client created: IPv6 enabled.");
                        await socket.ConnectAsync(address, port);
                        Polymono.Print($"Joined server: [{address}]:{port}");
                    }
                    catch (SocketException se)
                    {
                        Polymono.Error(se.Message);
                        Polymono.ErrorF(se.StackTrace);
                    }
                    catch (FormatException fe)
                    {
                        Polymono.Error("Port is not a readable number.");
                        Polymono.Error(fe.Message);
                        Polymono.ErrorF(fe.StackTrace);
                    }
                }, "Join server");
            BtnNetBack = new Button(Programs[ProgramID.Button],
                new Vector3((Width / 2) - 64, Height - (64 * 6), 0.0f), Vector3.Zero, Vector3.One,
                Controls, Models, MnuNetwork,
                128, 32, 2, Width, Height, Matrix4.Identity,
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.7f, 0.7f, 0.0f, 0.8f),
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.0f, 0.6f, 0.7f, 0.8f),
                () =>
                {
                    MnuNetwork.Hide();
                    MnuMain.Show();
                    return Task.Delay(0);
                }, "Back");
            MnuNetwork.CreateBuffers();
            #endregion
            #region Message menu
            MnuMessage = new Menu();
            LblMsgBox = new Label(Programs[ProgramID.Label],
                new Vector3(0f, 0f, 0f), Vector3.Zero, Vector3.One,
                Controls, Models, MnuMessage,
                128, 32, 2, Width, Height, Matrix4.Identity, new Color4(0.1f, 0.1f, 0.1f, 0.6f), "Chat room text");
            TxtMsgInput = new Textbox(Programs[ProgramID.Label],
                new Vector3(0f, 0f, 0f), Vector3.Zero, Vector3.One,
                Controls, Models, MnuMessage,
                64, 64, 2, Width, Height, Matrix4.Identity,
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.7f, 0.7f, 0.0f, 0.8f),
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.0f, 0.6f, 0.7f, 0.8f),
                "");
            BtnMsgSend = new Button(Programs[ProgramID.Button],
                new Vector3(0f, 0f, 0f), Vector3.Zero, Vector3.One,
                Controls, Models, MnuMessage,
                64, 64, 2, Width, Height, Matrix4.Identity,
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.7f, 0.7f, 0.0f, 0.8f),
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.0f, 0.6f, 0.7f, 0.8f),
                () =>
                {
                    return Task.Delay(0);
                }, "");
            MnuMessage.CreateBuffers();
            #endregion
            #region Game menu
            MnuGame = new Menu();
            #endregion
            #region Game menu
            MnuPlayerOptions = new Menu();
            BtnRollDice = new Button(Programs[ProgramID.Button],
                new Vector3(), Vector3.Zero, Vector3.One,
                Controls, Models, MnuPlayerOptions,
                128, 32, 2, Width, Height, Matrix4.Identity,
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.7f, 0.7f, 0.0f, 0.8f),
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.0f, 0.6f, 0.7f, 0.8f),
                () =>
                {
                    Board.GetPlayer().MoveSpaces(Dice.GetNumber() + Dice.GetNumber());
                    return Task.Delay(0);
                }, "Roll Dice");
            BtnPurchaseProperty = new Button(Programs[ProgramID.Button],
                new Vector3(), Vector3.Zero, Vector3.One,
                Controls, Models, MnuPlayerOptions,
                128, 32, 2, Width, Height, Matrix4.Identity,
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.7f, 0.7f, 0.0f, 0.8f),
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.0f, 0.6f, 0.7f, 0.8f),
                () =>
                {
                    // Purchase current property.
                    return Task.Delay(0);
                }, "Purchase");
            BtnTrade = new Button(Programs[ProgramID.Button],
                new Vector3(), Vector3.Zero, Vector3.One,
                Controls, Models, MnuPlayerOptions,
                128, 32, 2, Width, Height, Matrix4.Identity,
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.7f, 0.7f, 0.0f, 0.8f),
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.0f, 0.6f, 0.7f, 0.8f),
                () =>
                {
                    // Trade screen.
                    return Task.Delay(0);
                }, "Trade");
            MnuGame.CreateBuffers();
            #endregion
            #region Game menu
            MnuPlayerJailOptions = new Menu();
            BtnPayJail = new Button(Programs[ProgramID.Button],
                new Vector3(), Vector3.Zero, Vector3.One,
                Controls, Models, MnuPlayerJailOptions,
                128, 32, 2, Width, Height, Matrix4.Identity,
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.7f, 0.7f, 0.0f, 0.8f),
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.0f, 0.6f, 0.7f, 0.8f),
                () =>
                {
                    // Pay Jail
                    return Task.Delay(0);
                }, "Roll Dice");
            BtnUseCard = new Button(Programs[ProgramID.Button],
                new Vector3(), Vector3.Zero, Vector3.One,
                Controls, Models, MnuPlayerJailOptions,
                128, 32, 2, Width, Height, Matrix4.Identity,
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.7f, 0.7f, 0.0f, 0.8f),
                new Color4(0.1f, 0.1f, 0.1f, 0.6f), new Color4(0.0f, 0.6f, 0.7f, 0.8f),
                () =>
                {
                    // Use card
                    return Task.Delay(0);
                }, "Purchase");
            MnuPlayerJailOptions.CreateBuffers();
            #endregion
        }

        protected override void UpdateObjects()
        {
            // Update inputs and camera.
            UpdateInput(UTimeDelta);
            UpdateCamera();
            // Manage game state.
            LblTest.Text = $"X:{Mouse.X} Y:{Mouse.Y}";
            LblTest.SetTranslate(new Vector3(Mouse.X + 16, Height - Mouse.Y, 0.0f));
            LblTest.Update();
            #region Update Matrices.
            // Update view matrices.
            ViewMatrix = Camera.GetViewMatrix();
            StaticViewMatrix = Camera.GetStaticViewMatrix();
            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(ToRadians(Camera.Zoom),
                (float)Width / Height,
                0.1f, 1000f);
            UIProjectionMatrix = Matrix4.CreateOrthographicOffCenter(
                ClientRectangle.X, ClientRectangle.Width,
                ClientRectangle.Y, ClientRectangle.Height,
                -1.0f, 1.0f);
            // Update projection matrices.
            foreach (var control in Controls.Values)
            {
                control.ProjectionMatrix = UIProjectionMatrix;
                control.Update();
            }
            // Update matrices.
            Skybox.UpdateModelMatrix();
            foreach (var model in Models.Values)
            {
                model.UpdateModelMatrix();
            }
            #endregion
        }

        protected override void RenderObjects()
        {
            // Skybox renderer.
            Programs[ProgramID.Skybox].UseProgram();
            Programs[ProgramID.Skybox].UniformMatrix4("projection", ref ProjectionMatrix);
            Programs[ProgramID.Skybox].UniformMatrix4("view", ref StaticViewMatrix);
            Programs[ProgramID.Skybox].Uniform1("time", (float)RTime);
            Skybox.Render();
            //// Basic renderer.
            Programs[ProgramID.Full].UseProgram();
            Programs[ProgramID.Full].UniformMatrix4("projection", ref ProjectionMatrix);
            Programs[ProgramID.Full].UniformMatrix4("view", ref ViewMatrix);
            Board.Model.Render();
            // Dice renderer.
            Programs[ProgramID.Dice].UseProgram();
            Programs[ProgramID.Dice].Uniform3("light_position", ref ActiveLight.Position);
            Programs[ProgramID.Dice].Uniform3("light_color", ref ActiveLight.Color);
            Programs[ProgramID.Dice].Uniform1("light_ambientIntensity", ActiveLight.DiffuseIntensity);
            Programs[ProgramID.Dice].Uniform1("light_diffuseIntensity", ActiveLight.AmbientIntensity);
            Programs[ProgramID.Dice].UniformMatrix4("projection", ref ProjectionMatrix);
            Programs[ProgramID.Dice].UniformMatrix4("view", ref ViewMatrix);
            Programs[ProgramID.Dice].Uniform1("time", (float)RTime);
            Dice.Model.Render();
            // Player renderer.
            Programs[ProgramID.Player].UseProgram();
            foreach (var player in Board.Players)
            {
                Programs[ProgramID.Player].UniformMatrix4("projection", ref ProjectionMatrix);
                Programs[ProgramID.Player].UniformMatrix4("view", ref ViewMatrix);
                player.Model.Render();
            }
        }

        protected override void RenderUI()
        {
            // Button Main create renderer
            Programs[ProgramID.Button].UseProgram();
            Programs[ProgramID.Button].UniformMatrix4("projection", ref BtnCreate.ProjectionMatrix);
            BtnCreate.Render();
            // Button Main join renderer
            Programs[ProgramID.Button].UseProgram();
            Programs[ProgramID.Button].UniformMatrix4("projection", ref BtnJoin.ProjectionMatrix);
            BtnJoin.Render();
            // Button Main exit renderer
            Programs[ProgramID.Button].UseProgram();
            Programs[ProgramID.Button].UniformMatrix4("projection", ref BtnExit.ProjectionMatrix);
            BtnExit.Render();
            // Label Network Name renderer
            Programs[ProgramID.Label].UseProgram();
            Programs[ProgramID.Label].UniformMatrix4("projection", ref LblNetName.ProjectionMatrix);
            LblNetName.Render();
            // Button Network Name renderer
            Programs[ProgramID.Label].UseProgram();
            Programs[ProgramID.Label].UniformMatrix4("projection", ref TxtNetName.ProjectionMatrix);
            TxtNetName.Render();
            // Label Network Address renderer
            Programs[ProgramID.Label].UseProgram();
            Programs[ProgramID.Label].UniformMatrix4("projection", ref LblNetAddress.ProjectionMatrix);
            LblNetAddress.Render();
            // Textbox Network Address renderer
            Programs[ProgramID.Label].UseProgram();
            Programs[ProgramID.Label].UniformMatrix4("projection", ref TxtNetAddress.ProjectionMatrix);
            TxtNetAddress.Render();
            // Label Network Port renderer
            Programs[ProgramID.Label].UseProgram();
            Programs[ProgramID.Label].UniformMatrix4("projection", ref LblNetPort.ProjectionMatrix);
            LblNetPort.Render();
            // Textbox Network Port renderer
            Programs[ProgramID.Label].UseProgram();
            Programs[ProgramID.Label].UniformMatrix4("projection", ref TxtNetPort.ProjectionMatrix);
            TxtNetPort.Render();
            // Button Create Server renderer
            Programs[ProgramID.Button].UseProgram();
            Programs[ProgramID.Button].UniformMatrix4("projection", ref BtnNetCreate.ProjectionMatrix);
            BtnNetCreate.Render();
            // Button Create Server renderer
            Programs[ProgramID.Button].UseProgram();
            Programs[ProgramID.Button].UniformMatrix4("projection", ref BtnNetJoin.ProjectionMatrix);
            BtnNetJoin.Render();
            // Button Network back renderer
            Programs[ProgramID.Button].UseProgram();
            Programs[ProgramID.Button].UniformMatrix4("projection", ref BtnNetBack.ProjectionMatrix);
            BtnNetBack.Render();
            // Button Roll Dice renderer
            Programs[ProgramID.Button].UseProgram();
            Programs[ProgramID.Button].UniformMatrix4("projection", ref BtnRollDice.ProjectionMatrix);
            BtnRollDice.Render();
            // Button Test renderer
            Programs[ProgramID.Label].UseProgram();
            Programs[ProgramID.Label].UniformMatrix4("projection", ref BtnTest.ProjectionMatrix);
            BtnTest.Render();
            // Textbox Test renderer
            Programs[ProgramID.Label].UseProgram();
            Programs[ProgramID.Label].UniformMatrix4("projection", ref TxtTest.ProjectionMatrix);
            TxtTest.Render();
            // Mouse position renderer
            if (Focused && !isTrackingCursor)
            {
                Programs[ProgramID.Label].UseProgram();
                Programs[ProgramID.Label].UniformMatrix4("projection", ref UIProjectionMatrix);
                LblTest.Render();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(ToRadians(Camera.Zoom), (float)Width / Height, 0.1f, 1000f);
            UIProjectionMatrix = Matrix4.CreateOrthographicOffCenter(ClientRectangle.X, ClientRectangle.Width, ClientRectangle.Y, ClientRectangle.Height, -1.0f, 1.0f);
            foreach (var model in BtnCreate.Models.Values)
            {
                model.Position = new Vector3(Width / 2 - 64, Height - (64 * 2), 0.0f);
            }
            foreach (var model in BtnJoin.Models.Values)
            {
                model.Position = new Vector3(Width / 2 - 64, Height - (64 * 3), 0.0f);
            }
            foreach (var model in BtnExit.Models.Values)
            {
                model.Position = new Vector3(Width / 2 - 64, Height - (64 * 6), 0.0f);
            }
            foreach (var control in Controls.Values)
            {
                control.Resizing = true;
            }
        }

        protected override void OnFocusedChanged(EventArgs e)
        {
            if (Focused)
            {
                if (isTrackingCursor)
                {
                    ResetMouse();
                    CursorVisible = false;
                }
                else
                {
                    CursorVisible = true;
                }
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            Camera.ProcessMouseScroll(e.DeltaPrecise);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            foreach (Control control in Controls.Values)
            {
                if (control is IClickable clickable)
                {
                    clickable.Click(new Vector2(e.X, e.Y));
                }
            }
        }

        protected void UpdateCamera()
        {
            if (Focused && isTrackingCursor)
            {
                Vector2 delta = Camera.LastPosition - new Vector2(OpenTK.Input.Mouse.GetState().X, OpenTK.Input.Mouse.GetState().Y);
                Camera.ProcessMouseMovement(delta.X, delta.Y);
                ResetMouse();
            }
            else
            {
                Camera.LastPosition = new Vector2(OpenTK.Input.Mouse.GetState().X, OpenTK.Input.Mouse.GetState().Y);
            }
        }

        protected void ResetMouse()
        {
            // Sets position of the mouse to the middle of the screen.
            OpenTK.Input.Mouse.SetPosition(Bounds.Left + Bounds.Width / 2, Bounds.Top + Bounds.Height / 2);
            Camera.LastPosition = new Vector2(OpenTK.Input.Mouse.GetState().X, OpenTK.Input.Mouse.GetState().Y);
        }

        protected void UpdateInput(double deltaTime)
        {
            KeyboardState state = Keyboard.GetState();
            if (Focused && state.IsAnyKeyDown)
            {
                float deltaTimef = (float)deltaTime;
                #region Text input
                bool controlFocused = false;
                foreach (var control in Controls.Values)
                {
                    if (control is Textbox textbox)
                    {
                        bool focused = textbox.InputText(state, LastKeyboardState);
                        if (focused)
                        {
                            controlFocused = true;
                        }
                    }
                }
                #endregion
                #region Camera manipulation.
                if (!controlFocused)
                {
                    if (IsUniquePress(state, Key.ControlLeft))
                    {
                        isTrackingCursor = !isTrackingCursor;
                        if (isTrackingCursor)
                        {
                            CursorVisible = false;
                        }
                        else
                        {
                            CursorVisible = true;
                        }
                    }
                    if (state.IsKeyDown(Key.ShiftLeft))
                    {
                        deltaTimef *= 5;
                    }
                    if (state.IsKeyDown(Key.W))
                    {
                        Camera.ProcessKeyboard(CameraMovement.Forward, deltaTimef);
                    }
                    if (state.IsKeyDown(Key.S))
                    {
                        Camera.ProcessKeyboard(CameraMovement.Backward, deltaTimef);
                    }
                    if (state.IsKeyDown(Key.A))
                    {
                        Camera.ProcessKeyboard(CameraMovement.Left, deltaTimef);
                    }
                    if (state.IsKeyDown(Key.D))
                    {
                        Camera.ProcessKeyboard(CameraMovement.Right, deltaTimef);
                    }
                    if (state.IsKeyDown(Key.E))
                    {
                        Camera.ProcessKeyboard(CameraMovement.Up, deltaTimef);
                    }
                    if (state.IsKeyDown(Key.Q))
                    {
                        Camera.ProcessKeyboard(CameraMovement.Down, deltaTimef);
                    }
                }
                #endregion
                if (state.IsKeyDown(Key.Escape))
                {
                    Exit();
                }
            }
            LastKeyboardState = state;
        }

        protected bool IsUniqueClick(MouseState state, OpenTK.Input.ButtonState buttonState,
            OpenTK.Input.ButtonState lastButtonState)
        {
            return (buttonState == OpenTK.Input.ButtonState.Pressed
                && !(lastButtonState == OpenTK.Input.ButtonState.Pressed));
        }

        protected bool IsUniquePress(KeyboardState state, Key key)
        {
            return (state.IsKeyDown(key) && !LastKeyboardState.IsKeyDown(key));
        }
    }
}
