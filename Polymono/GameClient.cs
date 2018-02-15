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
        Default, Textured, Coloured, Full, Dice, Player
    }

    class GameClient : GameWindow {
        public static bool FatalError = false;
        public static bool StopForErrors = true;
        // Programs
        public Dictionary<ProgramID, ShaderProgram> Programs;
        // Models
        public Dictionary<int, AModel> Models;
        // Matrices
        public Matrix4 ViewMatrix;
        public Matrix4 ProjectionMatrix;
        // Game objects
        public Camera Camera;
        public Board Board;
        public Dice Dice;
        public Player Player;
        // Light object
        Light activeLight = new Light(Vector3.Zero, new Vector3(0.9f, 0.80f, 0.8f));

        public GameClient() : base(1280, 720, new GraphicsMode(32, 24, 0, 4))
        {
            Polymono.Print(ConsoleLevel.Debug, $"OpenGL Renderer: {GL.GetString(StringName.Renderer)}");
            Polymono.Print(ConsoleLevel.Debug, $"OpenGL Extensions: {GL.GetString(StringName.Extensions)}");
            Polymono.Print(ConsoleLevel.Debug, $"OpenGL Shader Language: {GL.GetString(StringName.ShadingLanguageVersion)}");
            Polymono.Print(ConsoleLevel.Debug, $"OpenGL Vendor: {GL.GetString(StringName.Vendor)}");
            Polymono.Print($"OpenGL version: {GL.GetString(StringName.Version)}");
            Polymono.Print($"Windows OS: {Environment.OSVersion}");
            Polymono.Print($"CLR version: {Environment.Version}");

            Programs = new Dictionary<ProgramID, ShaderProgram>();
            Models = new Dictionary<int, AModel>();
            Camera = new Camera();
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

            // Set vertices.
            List<Vertex> vertices = new List<Vertex> {
                new Vertex(new Vector3(-0.5f, -0.5f, 0.0f), Color4.White, new Vector2(1.0f, 1.0f)),
                new Vertex(new Vector3(-0.5f, 0.5f, 0.0f), Color4.White, new Vector2(0.0f, 1.0f)),
                new Vertex(new Vector3(0.5f, 0.5f, 0.0f), Color4.White, new Vector2(0.0f, 0.0f)),
                new Vertex(new Vector3(0.5f, -0.5f, 0.0f), Color4.White, new Vector2(1.0f, 0.0f))
            };

            int[] indices = new int[] {
                0, 1, 3,
                2, 3, 1
            };

            Board = new Board() {
                Model = new Model(vertices, indices,
                    Vector3.Zero, new Vector3(ToRadians(-90.0f), 0.0f, 0.0f), new Vector3(5.0f),
                    @"Resources\Textures\polymono.png")
            };

            Dice = new Dice() {
                Model = new ModelObject(@"Resources\Objects\cube.obj",
                    new Vector3(2.0f, 1.0f, 0.0f), Vector3.Zero, new Vector3(0.05f, 0.05f, 0.05f),
                    @"Resources\Textures\cube_textured_uv.png",
                    @"Resources\Objects\cube.mtl",
                    @"Material")
            };

            Player = new Player() {
                Model = new ModelObject(@"Resources\Objects\player.obj", Color4.Aqua,
                    new Vector3(0.0f, 0.0f, 0.0f), Vector3.Zero, new Vector3(0.25f, 0.25f, 0.25f),
                    @"Resources\Objects\player.mtl",
                    @"b0b0b0")
            };

            Board.Model.CreateBuffer();
            Dice.Model.CreateBuffer();
            Player.Model.CreateBuffer();

            Models.Add(Board.Model.ID, Board.Model);
            Models.Add(Dice.Model.ID, Dice.Model);
            Models.Add(Player.Model.ID, Player.Model);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            Title = $"Polymono | FPS: {1f / RenderPeriod:0} | TPS: {1f / UpdatePeriod:0}";
            // Error handling.
            if (FatalError && StopForErrors)
            {
                Console.WriteLine("Error occurred. Press ANY key to continue trying to run program.");
                Console.ReadLine();
                FatalError = false;
                StopForErrors = false;
            }
            // Update inputs and 
            UpdateInput(e.Time);
            UpdateCamera();

            Random random = new Random();
            Models[Dice.Model.ID].Rotate(new Vector3(0.001f, 0.0f, 0.0f));
            Polymono.DebugF($"{Models[Dice.Model.ID].Rotation}");

            // Update matrices.
            foreach (var model in Models.Values)
            {
                model.UpdateModelMatrix();
            }
            ViewMatrix = Camera.GetViewMatrix();
            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(
                ToRadians(Camera.Zoom),
                (float)Width / Height,
                0.1f,
                100f);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.ClearColor(Color.LightCyan);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
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
            GL.UniformMatrix4(17, false, ref ViewMatrix);
            Player.Model.RenderObject(ProgramID.Player);

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
                100f);
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
                if (keyState.IsKeyDown(Key.Keypad8))
                {
                    // Move object forward (Z)
                    Models[Dice.Model.ID].Translate(new Vector3(0.0f, 0.0f, 0.05f));
                }
                if (keyState.IsKeyDown(Key.Keypad2))
                {
                    // Move object backward (Z)
                    Models[Dice.Model.ID].Translate(new Vector3(0.0f, 0.0f, -0.05f));
                }
                if (keyState.IsKeyDown(Key.Keypad4))
                {
                    // Move object left (X)
                    Models[Dice.Model.ID].Translate(new Vector3(0.05f, 0.0f, 0.0f));
                }
                if (keyState.IsKeyDown(Key.Keypad6))
                {
                    // Move object right (X)
                    Models[Dice.Model.ID].Translate(new Vector3(-0.05f, 0.0f, 0.0f));
                }
                if (keyState.IsKeyDown(Key.Keypad9))
                {
                    // Move object up (Y)
                    Models[Dice.Model.ID].Translate(new Vector3(0.0f, 0.05f, 0.0f));
                }
                if (keyState.IsKeyDown(Key.Keypad7))
                {
                    // Move object Down (Y)
                    Models[Dice.Model.ID].Translate(new Vector3(0.0f, -0.05f, 0.0f));
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
