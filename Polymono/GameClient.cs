using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using Polymono.Game;
using Polymono.Graphics;
using Polymono.Networking;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Polymono
{
    class GameClient : AGameClient
    {
        // Game objects
        public GameState State;
        public Camera Camera;
        public Board Board;
        public Dice Dice;
        public Player[] Players;
        // Buttons
        public Button ButtonTest;
        public Button ButtonCreateServer;
        public Button ButtonClientJoin;
        public Button ButtonSendMessage;
        public Button ButtonExit;
        public Label LabelTest;
        public TextBox TextBoxTest;
        //Misc object
        public Skybox Skybox;
        public Light ActiveLight = new Light(new Vector3(0.0f, 5.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f));

        private Server _server;
        private Client _client;
        public INetwork Network {
            get {
                if (_isServer)
                {
                    return _server;
                }
                else
                {
                    return _client;
                }
            }
            set {
                if (value is Server)
                {
                    _server = (Server)value;
                    _isServer = true;
                }
                if (value is Client)
                {
                    _client = (Client)value;
                    _isServer = false;
                }
            }
        }
        private bool _isServer = false;

        public GameClient(int playerCount) : base()
        {
            State = new GameState(playerCount);
            Camera = new Camera(new Vector3(0.0f, 1.0f, 0.0f));
            Players = new Player[playerCount];
        }

        protected override void LoadObjects()
        {
            // Skybox
            Skybox = new Skybox(Programs[ProgramID.Skybox], @"Resources\Objects\sphere.obj", false);
            Skybox.CreateBuffer();
            // Board
            Board = new Board(Programs[ProgramID.Full]);
            Board.Model.CreateBuffer();
            Models.Add(Board.Model.ID, Board.Model);
            // Dice
            Dice = new Dice(Programs[ProgramID.Dice]);
            Dice.Model.CreateBuffer();
            Models.Add(Dice.Model.ID, Dice.Model);
            // Players
            for (int i = 0; i < State.PlayerCount; i++)
            {
                Players[i] = new Player(Programs[ProgramID.Player], Board);
                Players[i].Model.CreateBuffer();
                Models.Add(Players[i].Model.ID, Players[i].Model);
            }
            // Button
            ButtonTest = new Button(Programs[ProgramID.Button], "Test", new Color4(0.4f, 0.4f, 0.4f, 0.8f),
                128, 128, 64, 32, Width, Height, UIProjectionMatrix,
                async () =>
                {
                    Polymono.Print("Test function.");
                    await Task.Delay(2000);
                }, fontLocation: "arial_bold");
            ButtonTest.CreateBuffer();
            Models.Add(ButtonTest.ID, ButtonTest);
            // Button Create Server.
            ButtonCreateServer = new Button(Programs[ProgramID.Button], "Create server", new Color4(0.4f, 0.0f, 0.4f, 0.8f),
                (Width / 2) - 64, 100, 128, 32, Width, Height, UIProjectionMatrix,
                () =>
                {
                    Server server = new Server();
                    server.Start(2222);
                    Network = server;
                    ButtonCreateServer.Hide();
                    ButtonClientJoin.Hide();
                    return Task.Delay(0);
                }, fontLocation: "arial_bold");
            ButtonCreateServer.CreateBuffer();
            Models.Add(ButtonCreateServer.ID, ButtonCreateServer);
            // Button Client Join.
            ButtonClientJoin = new Button(Programs[ProgramID.Button], "Client join", new Color4(0.0f, 0.4f, 0.4f, 0.8f),
                (Width / 2) - 64, 164, 128, 32, Width, Height, UIProjectionMatrix,
                () =>
                {
                    Client client = new Client();
                    client.Start(TextBoxTest.Text, 2222);
                    Network = client;
                    ButtonCreateServer.Hide();
                    ButtonClientJoin.Hide();
                    return Task.Delay(0);
                }, fontLocation: "arial_bold");
            ButtonClientJoin.CreateBuffer();
            Models.Add(ButtonClientJoin.ID, ButtonClientJoin);
            // Label
            LabelTest = new Label(Programs[ProgramID.Label], "X:0 Y:0", new Color4(0.4f, 0.4f, 0.4f, 0.8f),
                0, 0, 74, 16, Width, Height, UIProjectionMatrix, fontLocation: "arial_bold");
            LabelTest.CreateBuffer();
            Models.Add(LabelTest.ID, LabelTest);
            // Text box
            TextBoxTest = new TextBox(Programs[ProgramID.Label], "", new Color4(0.4f, 0.4f, 0.4f, 0.8f),
                (Width / 2) - 64, 212, 74, 16, Width, Height, UIProjectionMatrix);
            TextBoxTest.CreateBuffer();
            Models.Add(TextBoxTest.ID, TextBoxTest);
        }

        protected override void UpdateObjects()
        {
            // Update inputs and camera.
            UpdateInput(UTimeDelta);
            UpdateCamera();
            // Manage game state.
            LabelTest.Text = $"X:{Mouse.X} Y:{Mouse.Y}";
            LabelTest.SetTranslate(new Vector3(Mouse.X + 16, Height - Mouse.Y, 0.0f));
            LabelTest.Update();

            TextBoxTest.Update();
            // Update matrices.
            Skybox.UpdateModelMatrix();
            foreach (var model in Models.Values)
            {
                model.UpdateModelMatrix();
            }
            ViewMatrix = Camera.GetViewMatrix();
            StaticViewMatrix = Camera.GetStaticViewMatrix();
            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(ToRadians(Camera.Zoom),
                (float)Width / Height,
                0.1f, 1000f);
            UIProjectionMatrix = Matrix4.CreateOrthographicOffCenter(
                ClientRectangle.X, ClientRectangle.Width,
                ClientRectangle.Y, ClientRectangle.Height,
                -2.0f, 2.0f);
            ButtonTest.ProjectionMatrix = UIProjectionMatrix;
            ButtonCreateServer.ProjectionMatrix = UIProjectionMatrix;
            ButtonClientJoin.ProjectionMatrix = UIProjectionMatrix;
            LabelTest.ProjectionMatrix = UIProjectionMatrix;
            TextBoxTest.ProjectionMatrix = UIProjectionMatrix;
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
            foreach (var player in Players)
            {
                Programs[ProgramID.Player].UniformMatrix4("projection", ref ProjectionMatrix);
                Programs[ProgramID.Player].UniformMatrix4("view", ref ViewMatrix);
                player.Model.Render();
            }
        }

        protected override void RenderUI()
        {
            // Button renderer
            Programs[ProgramID.Button].UseProgram();
            Programs[ProgramID.Button].UniformMatrix4("projection", ref ButtonTest.ProjectionMatrix);
            ButtonTest.Render();
            // Button Create Server renderer
            Programs[ProgramID.Button].UseProgram();
            Programs[ProgramID.Button].UniformMatrix4("projection", ref ButtonCreateServer.ProjectionMatrix);
            ButtonCreateServer.Render();
            // Button Create Server renderer
            Programs[ProgramID.Button].UseProgram();
            Programs[ProgramID.Button].UniformMatrix4("projection", ref ButtonClientJoin.ProjectionMatrix);
            ButtonClientJoin.Render();
            // Button Create Server renderer
            Programs[ProgramID.Label].UseProgram();
            Programs[ProgramID.Label].UniformMatrix4("projection", ref TextBoxTest.ProjectionMatrix);
            TextBoxTest.Render();
            // Mouse position renderer
            if (Focused && !isTrackingCursor)
            {
                Programs[ProgramID.Label].UseProgram();
                Programs[ProgramID.Label].UniformMatrix4("projection", ref UIProjectionMatrix);
                LabelTest.Render();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(ToRadians(Camera.Zoom), (float)Width / Height, 0.1f, 1000f);
            UIProjectionMatrix = Matrix4.CreateOrthographicOffCenter(ClientRectangle.X, ClientRectangle.Width, ClientRectangle.Y, ClientRectangle.Height, -1.0f, 1.0f);
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
            ButtonTest.Click(new Vector2(e.X, e.Y));
            ButtonCreateServer.Click(new Vector2(e.X, e.Y));
            ButtonClientJoin.Click(new Vector2(e.X, e.Y));
            TextBoxTest.Click(new Vector2(e.X, e.Y));
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
                TextBoxTest.InputText(state, LastKeyboardState);
                #endregion
                #region Camera manipulation.
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
