using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using Polymono.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Polymono
{
    public enum ProgramID {
        Full, Dice, Player, Skybox, Button, Label
    }

    abstract class AGameClient : GameWindow
    {
        // Static variables (Prone to Memory Leaks).
        public static bool FatalError = false;
        public static bool StopForErrors = true;
        public static int MajorVersion = 0;
        public static int MinorVersion = 0;
        // Programs.
        public Dictionary<ProgramID, ShaderProgram> Programs;
        // Models.
        public Dictionary<int, AModel> Models;
        // Matrices.
        public Matrix4 ViewMatrix;
        public Matrix4 StaticViewMatrix;
        public Matrix4 ProjectionMatrix;
        public Matrix4 UIProjectionMatrix;
        // Time variables.
        public double RTime = 0.0d;
        public double UTime = 0.0d;
        public double RTimeDelta = 0.0d;
        public double UTimeDelta = 0.0d;
        // Cursor state.
        public MouseState LastMouseState;
        public bool isTrackingCursor;
        // Keyboard state.
        public KeyboardState LastKeyboardState;
        public int SelectedObject = 0;

        public AGameClient()
            : base(1280, 720, new GraphicsMode(32, 24, 0, 4))
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
            Programs = new Dictionary<ProgramID, ShaderProgram>();
            Models = new Dictionary<int, AModel>();
            isTrackingCursor = false;
            CursorVisible = true;
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
            Programs.Add(ProgramID.Label,
                new ShaderProgram("vs_label.glsl", "fs_label.glsl", "Label"));
            LoadObjects();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            UTimeDelta = e.Time;
            UTime += e.Time;
            Title = $"Polymono | FPS: {1f / RenderPeriod:0} | TPS: {1f / UpdatePeriod:0}";
            // Error handling.
            if (FatalError && StopForErrors)
            {
                Console.WriteLine("Error occurred. Press ANY key to continue trying to run program.");
                Console.ReadLine();
                FatalError = false;
                StopForErrors = false;
            }
            UpdateObjects();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            // Setup render state.
            RTimeDelta = e.Time;
            RTime += e.Time;
            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(Color4.Aqua);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            // Render object geometry.
            RenderObjects();
            // Clear depth for UI.
            GL.Disable(EnableCap.DepthTest);
            // Render UI geometry.
            RenderUI();
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

        public static float ToRadians(float degrees)
        {
            return (float)Math.PI * degrees / 180.0f;
        }

        protected abstract void LoadObjects();

        protected abstract void UpdateObjects();

        protected abstract void RenderObjects();

        protected abstract void RenderUI();
    }
}
