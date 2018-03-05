using OpenTK;
using OpenTK.Graphics;
using Polymono.Vertices;
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
    class Label : Model
    {
        // Positional data
        public int Width, Height;
        public int WindowWidth, WindowHeight;
        public LayoutAlign LayoutAlign;
        // Text data
        private string _text = "";
        public string Text {
            get { return _text; }
            set {
                if (_text != value)
                {
                    _text = value;
                    UpdateText = true;
                }
            }
        }
        public bool UpdateText = false;
        // Font data
        public QFont LabelFont;
        public QFontDrawing LabelDrawing;
        // Matrices
        public Matrix4 ProjectionMatrix;

        public Label(ShaderProgram program, string text, Color4 colour,
            int x, int y, int width, int height, int windowWidth, int windowHeight,
            Matrix4 projection,
            LayoutAlign layoutAlign = LayoutAlign.TopRight, string fontLocation = "arial")
            : base(program)
        {
            LayoutAlign = layoutAlign;
            WindowWidth = windowWidth;
            WindowHeight = windowHeight;
            switch (LayoutAlign)
            {
                case LayoutAlign.TopRight:
                    Position = new Vector3(x, y, 0.0f);
                    Width = width;
                    Height = -height;
                    break;
                case LayoutAlign.TopLeft:
                    Position = new Vector3(WindowWidth - x, WindowHeight - y, 0.0f);
                    Width = width;
                    Height = -height;
                    break;
                case LayoutAlign.BottomRight:
                    Position = new Vector3(x, y, 0.0f);
                    Width = width;
                    Height = -height;
                    break;
                case LayoutAlign.BottomLeft:
                    Position = new Vector3(WindowWidth - x, y, 0.0f);
                    Width = width;
                    Height = -height;
                    break;
                default:
                    Position = new Vector3(x, y, 0.0f);
                    Width = width;
                    Height = height;
                    break;
            }
            // Create model, then populate button positional data to that model data.
            Vertices = new Vertex[] {
                new Vertex(// Top left
                    new Vector3(0.0f, 0.0f, 0.0f),
                    colour,
                    new Vector2(0.0f, 0.0f)),
                new Vertex(// Top right
                    new Vector3(Width, 0.0f, 0.0f),
                    colour,
                    new Vector2(1.0f, 0.0f)),
                new Vertex(// Bottom right
                    new Vector3(Width, Height, 0.0f),
                    colour,
                    new Vector2(1.0f, 1.0f)),
                new Vertex(// Bottom left
                    new Vector3(0.0f, Height, 0.0f),
                    colour,
                    new Vector2(0.0f, 1.0f))
            };
            Indices = new int[]
            {
                0, 1, 2,
                0, 3, 2
            };
            // Text update
            Text = text;
            // Matrix update
            ProjectionMatrix = projection;
            // Configure text.
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
            LabelFont = new QFont(@"Resources\Fonts\" + fontLocation + ".ttf", 8, builderConfig);
            // Buffer text.
            LabelDrawing.DrawingPrimitives.Clear();
            LabelDrawing.Print(LabelFont, Text, new Vector3(
                Position.X + (Width / 2), Position.Y, 1.0f), QFontAlignment.Centre);
            LabelDrawing.RefreshBuffers();
        }

        public override void Update()
        {
            if (UpdateText)
            {
                LabelDrawing.DrawingPrimitives.Clear();
                LabelDrawing.Print(LabelFont, Text, new Vector3(
                    Position.X + (Width / 2), Position.Y, 1.0f), QFontAlignment.Centre);
                LabelDrawing.RefreshBuffers();
                UpdateText = false;
            }
        }

        public override void Render()
        {
            if (!IsHidden)
            {
                base.Render();
                LabelDrawing.ProjectionMatrix = ProjectionMatrix;
                LabelDrawing.Draw();
            }
        }
    }
}
