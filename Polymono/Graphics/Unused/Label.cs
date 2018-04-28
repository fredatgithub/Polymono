using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Polymono.Graphics;
using Polymono.Graphics.Geometry;
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
    class LabelBackup : Model
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

        public LabelBackup(ShaderProgram program, string text, Color4 colour, Color4 highlightColour,
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
                    Position = new Vector3(x, WindowHeight - y, 0.0f);
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
            float highlightSize = Height / 10;
            Square centreSquare = new Square(
                highlightSize, // The X coord
                highlightSize, // The Y coord
                Width - (highlightSize / 2), // The Width
                Height - (highlightSize / 2), // The Height
                colour, // The colour of the box
                0); // The indexer start point.
            Square topSquare = new Square(
                0.0f,
                0.0f,
                Width - highlightSize,
                highlightSize,
                highlightColour, 
                centreSquare.Vertices.Count);
            Square rightSquare = new Square(
                Width - highlightSize,
                0.0f,
                highlightSize,
                Height - highlightSize,
                highlightColour, 
                centreSquare.Vertices.Count + topSquare.Vertices.Count);
            Square bottomSquare = new Square(0.0f,
                Height - highlightSize,
                Width - highlightSize,
                highlightSize,
                highlightColour,
                centreSquare.Vertices.Count + topSquare.Vertices.Count + rightSquare.Vertices.Count);
            Square leftSquare = new Square(0.0f,
                highlightSize,
                highlightSize,
                Height - highlightSize,
                highlightColour,
                centreSquare.Vertices.Count + topSquare.Vertices.Count + rightSquare.Vertices.Count + bottomSquare.Vertices.Count);
            
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
                GL.BindVertexArray(VAO);
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, IBO);
                GL.BindTexture(TextureTarget.Texture2D, TextureID);
                Program.UniformMatrix4("model", ref ModelMatrix);
                GL.DrawElements(BeginMode.Triangles, Indices.Length, DrawElementsType.UnsignedInt, 0);
                LabelDrawing.ProjectionMatrix = ProjectionMatrix;
                LabelDrawing.Draw();
            }
        }
    }
}
