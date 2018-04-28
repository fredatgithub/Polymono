using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Polymono.Graphics;
using QuickFont;
using QuickFont.Configuration;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Graphics
{
    public enum LayoutAlign
    {
        TopRight,
        TopLeft,
        BottomRight,
        BottomLeft
    }

    public enum ButtonState
    {
        Pressing,
        Pressed,
        Hovering,
        NotPressed,
        Hidden
    }

    class ButtonBackup : Model
    {
        // Positional data
        public int X, Y, Width, Height;
        public int WindowWidth, WindowHeight;
        public LayoutAlign LayoutAlign;
        // Button data
        public ButtonState State;
        public Func<Task> ExecDelegate;
        // Text data
        public string Text;
        // Font data
        public QFont LabelFont;
        public QFontDrawing LabelDrawing;
        // Matrices
        public Matrix4 ProjectionMatrix;

        public ButtonBackup(ShaderProgram program, string text, Color4 colour,
            int x, int y, int width, int height, int windowWidth, int windowHeight,
            Matrix4 projection, Func<Task> execDelegate,
            LayoutAlign layoutAlign = LayoutAlign.TopRight, string fontLocation = "arial")
            : base(program)
        {
            LayoutAlign = layoutAlign;
            WindowWidth = windowWidth;
            WindowHeight = windowHeight;
            switch (LayoutAlign)
            {
                case LayoutAlign.TopRight:
                    X = x;
                    Y = WindowHeight - y;
                    Width = width;
                    Height = -height;
                    break;
                case LayoutAlign.TopLeft:
                    X = WindowWidth - x;
                    Y = WindowHeight - y;
                    Width = width;
                    Height = -height;
                    break;
                case LayoutAlign.BottomRight:
                    X = x;
                    Y = y;
                    Width = width;
                    Height = -height;
                    break;
                case LayoutAlign.BottomLeft:
                    X = WindowWidth - x;
                    Y = y;
                    Width = width;
                    Height = -height;
                    break;
                default:
                    X = x;
                    Y = y;
                    Width = width;
                    Height = height;
                    break;
            }
            // Create model, then populate button positional data to that model data.
            Vertices = new Vertex[] {
                new Vertex(// Top left
                    new Vector3(X, Y, 0.0f),
                    colour,
                    new Vector2(0.0f, 0.0f)),
                new Vertex(// Top right
                    new Vector3(X + Width, Y, 0.0f),
                    colour,
                    new Vector2(1.0f, 0.0f)),
                new Vertex(// Bottom right
                    new Vector3(X + Width, Y + Height, 0.0f),
                    colour,
                    new Vector2(1.0f, 1.0f)),
                new Vertex(// Bottom left
                    new Vector3(X, Y + Height, 0.0f),
                    colour,
                    new Vector2(0.0f, 1.0f))
            };
            Indices = new int[]
            {
                0, 1, 2,
                0, 3, 2
            };
            Text = text;
            State = ButtonState.NotPressed;
            ProjectionMatrix = projection;
            ExecDelegate = execDelegate;
            // Configure text
            LabelDrawing = new QFontDrawing();
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
            LabelFont = new QFont(@"Resources\Fonts\" + fontLocation + ".ttf", 12, builderConfig);
            // Buffer text.
            LabelDrawing.DrawingPrimitives.Clear();
            LabelDrawing.Print(LabelFont, Text,
                new Vector3(X + (Width / 2), Y, 0.0f),
                new SizeF(Width, Height), QFontAlignment.Centre, new QFontRenderOptions()
                {
                    WordWrap = false
                });
            LabelDrawing.RefreshBuffers();
        }

        public async void Click(Vector2 mousePosition)
        {
            switch (LayoutAlign)
            {
                case LayoutAlign.TopRight:
                    mousePosition.Y = WindowHeight - mousePosition.Y;
                    break;
                case LayoutAlign.TopLeft:
                    mousePosition.X = WindowWidth - mousePosition.X;
                    mousePosition.Y = WindowHeight - mousePosition.Y;
                    break;
                case LayoutAlign.BottomRight:
                    break;
                case LayoutAlign.BottomLeft:
                    mousePosition.X = WindowWidth - mousePosition.X;
                    break;
                default:
                    break;
            }
            bool isHovering = PointInRectangle(Vertices[0].Position.Xy, Vertices[1].Position.Xy,
                Vertices[2].Position.Xy, Vertices[3].Position.Xy, mousePosition);
            if (isHovering && State == ButtonState.NotPressed)
            {
                State = ButtonState.Pressed;
                await ExecDelegate();
                State = ButtonState.NotPressed;
            }
        }

        private bool PointInRectangle(Vector2 pos1, Vector2 pos2,
            Vector2 pos3, Vector2 pos4, Vector2 posMouse)
        {
            return IsRight(pos1, pos2, posMouse)
                && IsRight(pos2, pos3, posMouse)
                && IsRight(pos3, pos4, posMouse)
                && IsRight(pos4, pos1, posMouse);
        }

        private bool IsRight(Vector2 pos1, Vector2 pos2, Vector2 posMouse)
        {
            return ((pos2.X - pos1.X) * (posMouse.Y - pos1.Y)
                - (pos2.Y - pos1.Y) * (posMouse.X - pos1.X)) < 0;
        }

        public override void Render()
        {
            if (!IsHidden && State != ButtonState.Hidden)
            {
                base.Render();
                LabelDrawing.ProjectionMatrix = ProjectionMatrix;
                LabelDrawing.Draw();
            }
        }
    }
}
