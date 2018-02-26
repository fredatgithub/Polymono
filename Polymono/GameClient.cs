using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using Polymono.Game;
using Polymono.Graphics;
using Polymono.Networking;
using Polymono.Vertices;
using QuickFont;
using QuickFont.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Polymono {
    public enum ProgramID {
        Default, Textured, Coloured, Full, Dice, Player, Skybox, Button
    }

    class GameClient : GameWindow {
        public static bool FatalError = false;
        public static bool StopForErrors = true;
        public static int MajorVersion = 0;
        public static int MinorVersion = 0;
        // Programs
        public Dictionary<ProgramID, ShaderProgram> Programs;
        // Models
        public Dictionary<int, AModel> Models;
        // Matrices
        public Matrix4 ViewMatrix;
        public Matrix4 StaticViewMatrix;
        public Matrix4 ProjectionMatrix;
        public Matrix4 UIProjectionMatrix;
        // Game objects
        public GameState State;
        public Camera Camera;
        public Board Board;
        public Dice Dice;
        public Player[] Players;
        public Button ButtonTest;
        // Light object
        Light activeLight = new Light(new Vector3(0.0f, 5.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f));
        //Misc object
        public Skybox Skybox;
        // Text
        public QFont _mainText;
        public QFontDrawing _drawing;

        public double rTime = 0.0d;
        public double uTime = 0.0d;

        public bool isTrackingCursor = false;

        public GameClient(int playerCount) : base(1280, 720, new GraphicsMode(32, 24, 0, 4))
        {
            Polymono.Print(ConsoleLevel.Debug, $"OpenGL Renderer: {GL.GetString(StringName.Renderer)}");
            Polymono.Print(ConsoleLevel.Debug, $"OpenGL Extensions: {GL.GetString(StringName.Extensions)}");
            Polymono.Print(ConsoleLevel.Debug, $"OpenGL Shader Language: {GL.GetString(StringName.ShadingLanguageVersion)}");
            Polymono.Print(ConsoleLevel.Debug, $"OpenGL Vendor: {GL.GetString(StringName.Vendor)}");
            Polymono.Print($"OpenGL version: {GL.GetString(StringName.Version)}");
            Polymono.Print($"Windows OS: {Environment.OSVersion}");
            Polymono.Print($"CLR version: {Environment.Version}");

            string version = GL.GetString(StringName.Version);
            MajorVersion = version[0];
            MinorVersion = version[2];
            if (MajorVersion < 3)
            {
                Polymono.ErrorF("Fatal error: OpenGL version 3 required.");
                Console.ReadLine();
                Exit();
            }
            State = new GameState(playerCount);
            Programs = new Dictionary<ProgramID, ShaderProgram>();
            Models = new Dictionary<int, AModel>();
            Camera = new Camera(new Vector3(0.0f, 2.0f, 0.0f));
            Players = new Player[playerCount];
        }

        protected override void OnLoad(EventArgs e)
        {
            // Enable OpenGL settings.
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.DebugOutput);
            GL.Enable(EnableCap.DebugOutputSynchronous);
            GL.DebugMessageCallback((DebugSource source, DebugType type, int id,
                DebugSeverity severity, int length, IntPtr message, IntPtr userParam) => {
                    switch (type)
                    {
                        case DebugType.DebugTypeError:
                            Polymono.Error($"OpenGL error ID: {id + Environment.NewLine}Message: {Marshal.PtrToStringAnsi(message, length)}");
                            Polymono.ErrorF(Environment.StackTrace);
                            FatalError = true;
                            break;
                        case DebugType.DebugTypeDeprecatedBehavior:
                        case DebugType.DebugTypeUndefinedBehavior:
                        case DebugType.DebugTypePortability:
                        case DebugType.DebugTypePerformance:
                        case DebugType.DebugTypeOther:
                        case DebugType.DebugTypeMarker:
                        case DebugType.DebugTypePushGroup:
                        case DebugType.DebugTypePopGroup:
                        default:
                            Polymono.Debug($"OpenGL debug message.{Environment.NewLine}ID: {id + Environment.NewLine}Message: {Marshal.PtrToStringAnsi(message, length)}");
                            break;
                    }
                }, (IntPtr)0);

            // Add shader programs.
            Programs.Add(ProgramID.Default, 
                new ShaderProgram("vs.glsl", "fs.glsl", "Default"));
            Programs.Add(ProgramID.Textured, 
                new ShaderProgram("vs_tex.glsl", "fs_tex.glsl", "Textured"));
            Programs.Add(ProgramID.Coloured, 
                new ShaderProgram("vs_col.glsl", "fs_col.glsl", "Coloured"));
            Programs.Add(ProgramID.Full, 
                new ShaderProgram("vs_full.glsl", "fs_full.glsl", "Full"));
            Programs.Add(ProgramID.Dice, 
                new ShaderProgram("vs_dice.glsl", "fs_dice.glsl", "Dice"));
            Programs.Add(ProgramID.Player, 
                new ShaderProgram("vs_player.glsl", "fs_player.glsl", "Player"));
            Programs.Add(ProgramID.Skybox, 
                new ShaderProgram("vs_skybox.glsl", "fs_skybox.glsl", "Skybox"));
            Programs.Add(ProgramID.Button, 
                new ShaderProgram("vs_button.glsl", "fs_button.glsl", "Button"));
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
            // Text
            _drawing = new QFontDrawing();
            var builderConfig = new QFontBuilderConfiguration(true)
            {
                ShadowConfig =
                {
                    BlurRadius = 2,
                    BlurPasses = 1,
                    Type = ShadowType.Blurred
                },
                TextGenerationRenderHint = TextGenerationRenderHint.ClearTypeGridFit,
                Characters = CharacterSet.General | CharacterSet.Japanese | CharacterSet.Thai | CharacterSet.Cyrillic
            };
            _mainText = new QFont(@"Fonts\times.ttf", 24, builderConfig);
            // Button
            ButtonTest = new Button(Programs[ProgramID.Button], "Button1", 128, Height - 128, 64, -64, () => { Polymono.Debug("Callback from button executed."); });
            ButtonTest.CreateBuffer();
            Models.Add(ButtonTest.ID, ButtonTest);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            uTime += e.Time;
            Title = $"Polymono | FPS: {1f / RenderPeriod:0} | TPS: {1f / UpdatePeriod:0}";
            // Error handling.
            if (FatalError && StopForErrors)
            {
                Console.WriteLine("Error occurred. Press ANY key to continue trying to run program.");
                Console.ReadLine();
                FatalError = false;
                StopForErrors = false;
            }
            // Update inputs and camera.
            UpdateInput(e.Time);
            UpdateCamera();

            // Manage game state.


            //Random random = new Random();
            //Models[Dice.Model.ID].Rotate(new Vector3(0.001f, 0.0f, 0.0f));
            //Polymono.DebugF($"{Models[Dice.Model.ID].Rotation}");

            // Update matrices.
            Skybox.UpdateModelMatrix();
            foreach (var model in Models.Values)
            {
                model.UpdateModelMatrix();
            }
            ViewMatrix = Camera.GetViewMatrix();
            StaticViewMatrix = Camera.GetStaticViewMatrix();
            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(ToRadians(Camera.Zoom), (float)Width / Height, 0.1f, 1000f);
            UIProjectionMatrix = Matrix4.CreateOrthographicOffCenter(ClientRectangle.X, ClientRectangle.Width, ClientRectangle.Y, ClientRectangle.Height, -1.0f, 1.0f);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            rTime += e.Time;
            GL.ClearColor(Color.Aqua);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            // Skybox renderer.
            Programs[ProgramID.Skybox].UseProgram();
            Programs[ProgramID.Skybox].UniformMatrix4("projection", ref ProjectionMatrix);
            Programs[ProgramID.Skybox].UniformMatrix4("view", ref StaticViewMatrix);
            Programs[ProgramID.Skybox].Uniform1("time", (float)rTime);
            Skybox.Render();
            //// Basic renderer.
            Programs[ProgramID.Full].UseProgram();
            Programs[ProgramID.Full].UniformMatrix4("projection", ref ProjectionMatrix);
            Programs[ProgramID.Full].UniformMatrix4("view", ref ViewMatrix);
            Board.Model.Render();
            // Dice renderer.
            Programs[ProgramID.Dice].UseProgram();
            Programs[ProgramID.Dice].Uniform3("light_position", ref activeLight.Position);
            Programs[ProgramID.Dice].Uniform3("light_color", ref activeLight.Color);
            Programs[ProgramID.Dice].Uniform1("light_ambientIntensity", activeLight.DiffuseIntensity);
            Programs[ProgramID.Dice].Uniform1("light_diffuseIntensity", activeLight.AmbientIntensity);
            Programs[ProgramID.Dice].UniformMatrix4("projection", ref ProjectionMatrix);
            Programs[ProgramID.Dice].UniformMatrix4("view", ref ViewMatrix);
            Programs[ProgramID.Dice].Uniform1("time", (float)rTime);
            Dice.Model.Render();
            // Player renderer.
            Programs[ProgramID.Player].UseProgram();
            foreach (var player in Players)
            {
                Programs[ProgramID.Player].UniformMatrix4("projection", ref ProjectionMatrix);
                Programs[ProgramID.Player].UniformMatrix4("view", ref ViewMatrix);
                player.Model.Render();
            }
            // Button renderer
            Programs[ProgramID.Button].UseProgram();
            Programs[ProgramID.Button].UniformMatrix4("projection", ref UIProjectionMatrix);
            ButtonTest.Render();
            // Text renderer.
            _drawing.ProjectionMatrix = UIProjectionMatrix;
            _drawing.DrawingPrimitives.Clear();
            _drawing.Print(_mainText, "Hello", new Vector3(0, Height, 0), QFontAlignment.Left);
            _drawing.Print(_mainText, "World", new Vector3(0, Height - 30, 0), QFontAlignment.Left);
            _drawing.RefreshBuffers();
            _drawing.Draw();
            // Finalise state.
            SwapBuffers();
        }

        protected override void OnClosed(EventArgs e)
        {
            foreach (AModel model in Models.Values)
            {
                model.Delete();
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
            ResetMouse();
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            Camera.ProcessMouseScroll(e.DeltaPrecise);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {

        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.Mouse.LeftButton == OpenTK.Input.ButtonState.Pressed)
            {
                ButtonTest.Click(new Vector2(e.Mouse.X, e.Mouse.Y));
            }   
        }

        KeyboardState lastKeyboardState;
        int selectedObjectID = 0;

        protected void UpdateInput(double deltaTime)
        {
            KeyboardState keyState = Keyboard.GetState();
            if (Focused && keyState.IsAnyKeyDown)
            {
                float deltaTimef = (float)deltaTime;
                #region Camera manipulation.
                if (IsUniquePress(keyState, Key.ControlLeft))
                {
                    isTrackingCursor = !isTrackingCursor;
                }
                if (keyState.IsKeyDown(Key.W))
                {
                    Camera.ProcessKeyboard(CameraMovement.Forward, deltaTimef);
                }
                if (keyState.IsKeyDown(Key.S))
                {
                    Camera.ProcessKeyboard(CameraMovement.Backward, deltaTimef);
                }
                if (keyState.IsKeyDown(Key.A))
                {
                    Camera.ProcessKeyboard(CameraMovement.Left, deltaTimef);
                }
                if (keyState.IsKeyDown(Key.D))
                {
                    Camera.ProcessKeyboard(CameraMovement.Right, deltaTimef);
                }
                if (keyState.IsKeyDown(Key.E))
                {
                    Camera.ProcessKeyboard(CameraMovement.Up, deltaTimef);
                }
                if (keyState.IsKeyDown(Key.Q))
                {
                    Camera.ProcessKeyboard(CameraMovement.Down, deltaTimef);
                }
                #endregion
                #region Object manipulation.
                // Selected object
                if (keyState.IsKeyDown(Key.Number0))
                {
                    if (selectedObjectID != 0)
                    {
                        Polymono.Print("Selected object ID changed to 0");
                    }
                    selectedObjectID = 0;
                }
                // Selected object
                if (keyState.IsKeyDown(Key.Number1))
                {
                    if (selectedObjectID != 1)
                    {
                        Polymono.Print("Selected object ID changed to 1");
                    }
                    selectedObjectID = 1;
                }
                // Selected object
                if (keyState.IsKeyDown(Key.Number2))
                {
                    if (selectedObjectID != 2)
                    {
                        Polymono.Print("Selected object ID changed to 2");
                    }
                    selectedObjectID = 2;
                }
                // Selected object
                if (keyState.IsKeyDown(Key.Number3))
                {
                    if (selectedObjectID != 3)
                    {
                        Polymono.Print("Selected object ID changed to 3");
                    }
                    selectedObjectID = 3;
                }
                // Selected object
                if (keyState.IsKeyDown(Key.Number4))
                {
                    if (selectedObjectID != 4)
                    {
                        Polymono.Print("Selected object ID changed to 4");
                    }
                    selectedObjectID = 4;
                }
                // Selected object
                if (keyState.IsKeyDown(Key.Number5))
                {
                    if (selectedObjectID != 5)
                    {
                        Polymono.Print("Selected object ID changed to 5");
                    }
                    selectedObjectID = 5;
                }
                if (keyState.IsKeyDown(Key.Keypad8))
                {
                    // Move object forward (Z)
                    Models[selectedObjectID].Translate(new Vector3(0.0f, 0.0f, 0.05f));
                }
                if (keyState.IsKeyDown(Key.Keypad2))
                {
                    // Move object backward (Z)
                    Models[selectedObjectID].Translate(new Vector3(0.0f, 0.0f, -0.05f));
                }
                if (keyState.IsKeyDown(Key.Keypad4))
                {
                    // Move object left (X)
                    Models[selectedObjectID].Translate(new Vector3(0.05f, 0.0f, 0.0f));
                }
                if (keyState.IsKeyDown(Key.Keypad6))
                {
                    // Move object right (X)
                    Models[selectedObjectID].Translate(new Vector3(-0.05f, 0.0f, 0.0f));
                }
                if (keyState.IsKeyDown(Key.Keypad9))
                {
                    // Move object up (Y)
                    Models[selectedObjectID].Translate(new Vector3(0.0f, 0.05f, 0.0f));
                }
                if (keyState.IsKeyDown(Key.Keypad7))
                {
                    // Move object Down (Y)
                    Models[selectedObjectID].Translate(new Vector3(0.0f, -0.05f, 0.0f));
                }
                if (keyState.IsKeyDown(Key.Keypad5))
                {
                    // Reset object
                    Models[selectedObjectID].ResetModel();
                }
                #endregion
                if (keyState.IsKeyDown(Key.Escape))
                {
                    Exit();
                }
            }
            lastKeyboardState = keyState;
        }

        private bool IsUniquePress(KeyboardState state, Key key)
        {
            return (state.IsKeyDown(key) && !lastKeyboardState.IsKeyDown(key));
        }

        public void UpdateCamera()
        {
            if (Focused && isTrackingCursor)
            {
                Vector2 delta = Camera.LastPosition - new Vector2(OpenTK.Input.Mouse.GetState().X, OpenTK.Input.Mouse.GetState().Y);
                Camera.ProcessMouseMovement(delta.X, delta.Y);
                ResetMouse();
            }
        }

        public void ResetMouse()
        {
            // Sets position of the mouse to the middle of the screen.
            OpenTK.Input.Mouse.SetPosition(Bounds.Left + Bounds.Width / 2, Bounds.Top + Bounds.Height / 2);
            Camera.LastPosition = new Vector2(OpenTK.Input.Mouse.GetState().X, OpenTK.Input.Mouse.GetState().Y);
        }

        public static float ToRadians(float degrees)
        {
            return (float)Math.PI * degrees / 180.0f;
        }
    }
}
