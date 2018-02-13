using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using Polymono.Classes.Game;
using Polymono.Classes.Graphics;
using Polymono.Classes.Networking;
using Polymono.Classes.Vertices;
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

namespace Polymono.Classes {
    class GameClient : GameWindow {
        // Programs
        public Dictionary<ProgramIDs, ShaderProgram> Programs;
        // Models
        public Dictionary<int, AModel> Models;
        // Matrices
        public Matrix4 ViewMatrix;
        public Matrix4 ProjectionMatrix;
        // Game objects
        public Camera Camera;
        public Board Board;
        public Player Player;

        public enum ProgramIDs {
            Default, Textured, Coloured, Full, Player
        }

        public GameClient() : base(1280, 720, new GraphicsMode(32, 24, 0, 4))
        {
            Polymono.Print(ConsoleLevel.Debug, $"OpenGL Renderer: {GL.GetString(StringName.Renderer)}");
            Polymono.Print(ConsoleLevel.Debug, $"OpenGL Extensions: {GL.GetString(StringName.Extensions)}");
            Polymono.Print(ConsoleLevel.Debug, $"OpenGL Shader Language: {GL.GetString(StringName.ShadingLanguageVersion)}");
            Polymono.Print(ConsoleLevel.Debug, $"OpenGL Vendor: {GL.GetString(StringName.Vendor)}");
            Polymono.Print($"OpenGL version: {GL.GetString(StringName.Version)}");
            Polymono.Print($"Windows OS: {Environment.OSVersion}");
            Polymono.Print($"CLR version: {Environment.Version}");

            Programs = new Dictionary<ProgramIDs, ShaderProgram>();
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
                            Polymono.Debug($"OpenGL error.{Environment.NewLine}ID: {id + Environment.NewLine}Message: {Marshal.PtrToStringAnsi(message, length)}");
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
            Programs.Add(ProgramIDs.Default, new ShaderProgram("vs.glsl", "fs.glsl", "Default"));
            Programs.Add(ProgramIDs.Textured, new ShaderProgram("vs_tex.glsl", "fs_tex.glsl", "Textured"));
            Programs.Add(ProgramIDs.Coloured, new ShaderProgram("vs_col.glsl", "fs_col.glsl", "Coloured"));
            Programs.Add(ProgramIDs.Full, new ShaderProgram("vs_full.glsl", "fs_full.glsl", "Full"));
            Programs.Add(ProgramIDs.Player, new ShaderProgram("vs_player.glsl", "fs_player.glsl", "Player"));

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

            Matrix4 modelMatrix =
                Matrix4.CreateTranslation(0.0f, 0.0f, 0.0f) *
                Matrix4.CreateRotationZ(0.0f) *
                Matrix4.CreateRotationY(0.0f) *
                Matrix4.CreateRotationX(ToRadians(-90.0f)) *
                Matrix4.CreateScale(5.0f);

            Board = new Board() {
                Model = new Model(vertices, indices, modelMatrix, @"Resources\Textures\polymono.png")
            };

            Player = new Player() {
                Model = new ModelObject(@"Resources\Objects\player.obj", Color4.Aqua)
            };

            Board.Model.CreateBuffer();
            Player.Model.CreateBuffer();

            Models.Add(Board.Model.ID, Board.Model);
            Models.Add(Player.Model.ID, Player.Model);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            Title = $"Polymono | FPS: {1f / RenderPeriod:0} | TPS: {1f / UpdatePeriod:0}";

            UpdateInput(e.Time);
            UpdateCamera();

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

            Programs[ProgramIDs.Full].UseProgram();
            GL.UniformMatrix4(18, false, ref ProjectionMatrix);
            GL.UniformMatrix4(17, false, ref ViewMatrix);
            Board.Model.Render();

            Programs[ProgramIDs.Player].UseProgram();
            GL.UniformMatrix4(18, false, ref ProjectionMatrix);
            GL.UniformMatrix4(17, false, ref ViewMatrix);
            Player.Model.Render();

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

                if (keyState.IsKeyDown(Key.W))
                {
                    Camera.ProcessKeyboard(CameraMovement.FORWARD, deltaTimef);
                }
                if (keyState.IsKeyDown(Key.S))
                {
                    Camera.ProcessKeyboard(CameraMovement.BACKWARD, deltaTimef);
                }
                if (keyState.IsKeyDown(Key.A))
                {
                    Camera.ProcessKeyboard(CameraMovement.LEFT, deltaTimef);
                }
                if (keyState.IsKeyDown(Key.D))
                {
                    Camera.ProcessKeyboard(CameraMovement.RIGHT, deltaTimef);
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
