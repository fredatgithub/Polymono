using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using Polymono.Game;
using Polymono.Graphics;
using Polymono.Networking;
using Polymono.Vertices;
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
        Default, Textured, Coloured, Full, Dice, Player, Skybox
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
        // Game objects
        public GameState State;
        public Camera Camera;
        public Board Board;
        public Dice Dice;
        public Player[] Players;
        // Light object
        Light activeLight = new Light(Vector3.Zero, new Vector3(0.9f, 0.80f, 0.8f));
        //Misc object
        public ModelObject Skybox;

        public double rTime = 0.0d;
        public double uTime = 0.0d;

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
            State = new GameState(playerCount);
            Programs = new Dictionary<ProgramID, ShaderProgram>();
            Models = new Dictionary<int, AModel>();
            Camera = new Camera();
            Players = new Player[playerCount];
        }

        protected override void OnLoad(EventArgs e)
        {
            // Enable OpenGL settings.
            GL.Enable(EnableCap.Multisample);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.DebugOutput);
            GL.DebugMessageCallback((DebugSource source, DebugType type, int id,
                DebugSeverity severity, int length, IntPtr message, IntPtr userParam) => {
                    switch (type)
                    {
                        case DebugType.DebugTypeError:
                            Polymono.Error($"OpenGL error.{Environment.NewLine}ID: {id + Environment.NewLine}Message: {Marshal.PtrToStringAnsi(message, length)}");
                            Polymono.ErrorF(source.ToString());
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
                            Polymono.DebugF(source.ToString());
                            break;
                    }
                }, (IntPtr)0);

            // Add shader programs.
            Programs.Add(ProgramID.Default, new ShaderProgram("vs.glsl", "fs.glsl", "Default"));
            Programs.Add(ProgramID.Textured, new ShaderProgram("vs_tex.glsl", "fs_tex.glsl", "Textured"));
            Programs.Add(ProgramID.Coloured, new ShaderProgram("vs_col.glsl", "fs_col.glsl", "Coloured"));
            Programs.Add(ProgramID.Full, new ShaderProgram("vs_full.glsl", "fs_full.glsl", "Full"));
            Programs.Add(ProgramID.Dice, new ShaderProgram("vs_dice.glsl", "fs_dice.glsl", "Dice"));
            Programs.Add(ProgramID.Player, new ShaderProgram("vs_player.glsl", "fs_player.glsl", "Player"));
            Programs.Add(ProgramID.Skybox, new ShaderProgram("vs_skybox.glsl", "fs_skybox.glsl", "Skybox"));

            Skybox = new ModelObject(@"Resources\Objects\sphere.obj", false);
            Skybox.CreateBuffer();

            Board = new Board();

            Dice = new Dice() {
                Model = new ModelObject(@"Resources\Objects\cube.obj",
                    new Vector3(0.25f, 0.05f, 0.0f), Vector3.Zero, new Vector3(0.05f),
                    @"Resources\Textures\cube_textured_uv.png",
                    @"Resources\Objects\cube.mtl",
                    @"Material")
            };

            for (int i = 0; i < State.PlayerCount; i++)
            {
                Players[i] = new Player(Board);
            }

            Board.Model.CreateBuffer();
            Dice.Model.CreateBuffer();
            foreach (var player in Players)
            {
                player.Model.CreateBuffer();
            }

            Models.Add(Board.Model.ID, Board.Model);
            Models.Add(Dice.Model.ID, Dice.Model);
            foreach (var player in Players)
            {
                Models.Add(player.Model.ID, player.Model);
            }
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
            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(
                ToRadians(Camera.Zoom),
                (float)Width / Height,
                0.1f,
                1000f);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            rTime += e.Time;
            GL.ClearColor(Color.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            // Skybox render
            Programs[ProgramID.Skybox].UseProgram();
            GL.UniformMatrix4(18, false, ref ProjectionMatrix);
            GL.UniformMatrix4(17, false, ref StaticViewMatrix);
            GL.Uniform1(32, (float)rTime);
            Skybox.RenderObject(ProgramID.Skybox);
            // Basic renderer
            Programs[ProgramID.Full].UseProgram();
            GL.UniformMatrix4(18, false, ref ProjectionMatrix);
            GL.UniformMatrix4(17, false, ref ViewMatrix);
            Board.Model.Render();
            // Dice renderer
            Programs[ProgramID.Dice].UseProgram();
            GL.Uniform3(12, ref activeLight.Position);
            GL.Uniform3(13, ref activeLight.Color);
            GL.Uniform1(14, activeLight.DiffuseIntensity);
            GL.Uniform1(15, activeLight.AmbientIntensity);
            GL.UniformMatrix4(18, false, ref ProjectionMatrix);
            GL.UniformMatrix4(17, false, ref ViewMatrix);
            Dice.Model.RenderObject(ProgramID.Dice);
            // Player renderer
            Programs[ProgramID.Player].UseProgram();
            GL.UniformMatrix4(18, false, ref ProjectionMatrix);
            foreach (var player in Players)
            {
                GL.UniformMatrix4(17, false, ref ViewMatrix);
                player.Model.RenderObject(ProgramID.Player);
            }

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
            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(
                ToRadians(Camera.Zoom),
                (float)Width / Height,
                0.1f,
                1000f);
        }

        protected override void OnFocusedChanged(EventArgs e)
        {
            if (Focused)
            {
                ResetMouse();
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            Camera.ProcessMouseScroll(e.DeltaPrecise);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {

        }

        int selectedObjectID = 0;

        protected void UpdateInput(double deltaTime)
        {
            if (Focused)
            {
                KeyboardState keyState = Keyboard.GetState();
                float deltaTimef = (float)deltaTime;
                // Camera manipulation.
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
                // Object manipulation.
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
                if (keyState.IsKeyDown(Key.Escape))
                {
                    Exit();
                }
            }
        }

        public void UpdateCamera()
        {
            if (Focused)
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
