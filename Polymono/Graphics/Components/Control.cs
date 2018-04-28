using OpenTK;
using Polymono.Graphics;
using QuickFont;
using QuickFont.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Graphics.Components
{
    public enum ControlState
    {
        Clicked,
        Hovering,
        Focused,
        Unfocused,
        Normal
    }

    abstract class Control
    {
        // Identification
        public static int TotalID = 0;
        public int ID;
        // Model data
        public Dictionary<string, Model> Models;
        public Matrix4 ProjectionMatrix;
        // Positional data
        public int Width, Height;
        public int WindowWidth, WindowHeight;
        // Selector
        public string Selector = "Default";
        // Text data
        private string _text = "";
        public bool UpdateText = false;
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
        public bool Resizing = false;
        // Font data
        public QFont LabelFont;
        public QFontDrawing LabelDrawing;
        // State
        public ControlState State;

        public Control(ShaderProgram program,
            Vector3 position, Vector3 rotation, Vector3 scaling,
            Dictionary<int, Control> controls, Dictionary<int, AModel> models, Menu menu,
            int width, int height, int windowWidth, int windowHeight,
            Matrix4 projection, string text = "", string fontLocation = "arial")
        {
            ID = TotalID++;
            Width = width;
            Height = height;
            WindowWidth = windowWidth;
            WindowHeight = windowHeight;
            // Create model.
            Models = new Dictionary<string, Model>
            {
                { "Default", new Model(program, position, rotation, scaling) },
                { "Focused", new Model(program, position, rotation, scaling) },
                { "Hovering", new Model(program, position, rotation, scaling) }
            };
            foreach (var model in Models.Values)
            {
                models.Add(model.ID, model);
            }
            // Text assignments
            Text = text;
            ProjectionMatrix = projection;
            LabelDrawing = new QFontDrawing();
            // Text Config.
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
            PrintText(position);
            State = ControlState.Normal;
            // Add this control to dictionaries.
            controls.Add(ID, this);
            menu.Add(this);
        }

        public void PrintText(Vector3 position)
        {
            LabelDrawing.DrawingPrimitives.Clear();
            LabelDrawing.Print(LabelFont, Text, new Vector3(
                position.X + (Width / 2), position.Y - 6, 1.0f), QFontAlignment.Centre);
            LabelDrawing.RefreshBuffers();
        }

        public void CreateBuffer()
        {
            foreach (var model in Models.Values)
            {
                model.CreateBuffer();
            }
        }

        public void Update()
        {
            Models[Selector].Update();
            if (UpdateText || Resizing)
            {
                LabelDrawing.DrawingPrimitives.Clear();
                LabelDrawing.Print(LabelFont, Text, new Vector3(
                    Models[Selector].Position.X + (Width / 2),
                    Models[Selector].Position.Y, 1.0f),
                    QFontAlignment.Centre);
                LabelDrawing.RefreshBuffers();
                UpdateText = false;
                Resizing = false;
            }
        }

        public void Render()
        {
            if (!Models[Selector].IsHidden)
            {
                Models[Selector].Render();
                LabelDrawing.ProjectionMatrix = ProjectionMatrix;
                LabelDrawing.Draw();
            }
        }

        public void RenderFull()
        {
            UseProgram();
            UpdateMatrix();
            Render();
        }

        public void UseProgram()
        {
            Models[Selector].Program.UseProgram();
        }

        public void UpdateMatrix()
        {
            Models[Selector].Program.UniformMatrix4("projection", ref ProjectionMatrix);
        }

        public void Show()
        {
            Models[Selector].Show();
        }

        public void Show(string selector)
        {
            Models[selector].Show();
        }

        public void ShowAll()
        {
            foreach (var model in Models.Values)
            {
                model.Show();
            }
        }

        public void Hide()
        {
            Models[Selector].Hide();
        }

        public void Hide(string selector)
        {
            Models[selector].Hide();
        }

        public void HideAll()
        {
            foreach (var model in Models.Values)
            {
                model.Hide();
            }
            if (State != ControlState.Normal)
            {
                State = ControlState.Normal;
                Selector = "Default";
            }
        }

        public bool IsHidden()
        {
            return Models[Selector].IsHidden;
        }

        public bool IsHidden(string selector)
        {
            return Models[selector].IsHidden;
        }

        public void UpdateModelMatrix()
        {
            foreach (var model in Models.Values)
            {
                Models[Selector].UpdateModelMatrix();
            }
        }

        public void UpdateModelMatrix(string selector)
        {
            Models[selector].UpdateModelMatrix();
        }

        public void SetTranslate(Vector3 vector, string key = "")
        {
            if (key == "")
            {
                foreach (var model in Models.Values)
                {
                    model.SetTranslate(vector);
                }
            }
            else
            {
                Models[key].SetTranslate(vector);
            }
        }

        protected void ControlBase(ref List<Vertex> vertices, ref List<int> indices)
        {
            if (vertices == null || indices == null) return;
            foreach (var model in Models.Values)
            {
                if (model.Vertices == null || model.Vertices.Length == 0)
                {
                    model.Vertices = vertices.ToArray();
                }
                if (model.Indices == null || model.Indices.Length == 0)
                {
                    model.Indices = indices.ToArray();
                }
            }
        }
    }
}
