using OpenTK;
using OpenTK.Graphics;
using Polymono.Graphics.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Graphics.Components
{
    class Checkbox : Control, IClickable
    {
        public Checkbox(ShaderProgram program,
            Vector3 position, Vector3 rotation, Vector3 scaling,
            Dictionary<int, Control> controls, Dictionary<int, AModel> models, Menu menu,
            int width, int height, int buffer, int windowWidth, int windowHeight,
            Matrix4 projection, Color4 colour, Color4 highlightColour,
            Color4 focusedColour, Color4 focusedHighlightColour)
            : base(program, position, rotation, scaling, controls, models, menu, width, height, windowWidth, windowHeight, projection, "", "arial")
        {
            Models.Add("Clicked", new Model(program, position, rotation, scaling));
            models.Add(Models["Clicked"].ID, Models["Clicked"]);
            CreateDefaultModel("Default", width, height, buffer, colour, highlightColour);
            CreateDefaultModel("Clicked", width, height, buffer, focusedColour, focusedHighlightColour);

        }

        public void Click(Vector2 vector)
        {
            vector.Y = WindowHeight - vector.Y;
            Vector3 position = Models[Selector].Position;
            bool isHovering = PointInRectangle(
                new Vector2(position.X, position.Y),
                new Vector2(position.X + Width, position.Y),
                new Vector2(position.X + Width, position.Y - Height),
                new Vector2(position.X, position.Y - Height), vector);
            if (isHovering && !Models[Selector].IsHidden)
            {
                Polymono.Debug($"Checkbox clicked: [{ID}]");
                if (State == ControlState.Normal)
                {
                    State = ControlState.Clicked;
                    Selector = "Clicked";
                }
                else
                {
                    State = ControlState.Normal;
                    Selector = "Default";
                }
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

        private void CreateDefaultModel(string selector, int width, int height, int buffer,
           Color4 mainColour, Color4 highlightColour)
        {
            height = -height;
            Square mainSquare = new Square(
                buffer,                     // X position
                -buffer,                    // Y position
                width - (buffer * 2),       // Width
                height + (buffer * 2),      // Height
                mainColour);
            Square topSquare = new Square(
                0,                          // X position
                0,                          // Y position
                width - buffer,             // Width
                -buffer,                    // Height
                highlightColour, mainSquare.Append());
            Square rightSquare = new Square(
                width - buffer,             // X position
                0,                          // Y position
                buffer,                     // Width
                height + buffer,            // Height
                highlightColour, topSquare.Append());
            Square bottomSquare = new Square(
                buffer,                     // X position
                height + buffer,            // Y position
                width - buffer,             // Width
                -buffer,                    // Height
                highlightColour, rightSquare.Append());
            Square leftSquare = new Square(
                0,                          // X position
                -buffer,                    // Y position
                buffer,                     // Width
                height + buffer,            // Height
                highlightColour, bottomSquare.Append());
            // Create vertex list.
            List<Vertex> vertices = new List<Vertex>(
                mainSquare.Vertices.Count +
                topSquare.Vertices.Count +
                rightSquare.Vertices.Count +
                bottomSquare.Vertices.Count +
                leftSquare.Vertices.Count);
            // Adding each object to one list.
            vertices.AddRange(mainSquare.Vertices);
            vertices.AddRange(topSquare.Vertices);
            vertices.AddRange(rightSquare.Vertices);
            vertices.AddRange(bottomSquare.Vertices);
            vertices.AddRange(leftSquare.Vertices);
            // Create indices list.
            List<int> indices = new List<int>(
                mainSquare.Indices.Count +
                topSquare.Indices.Count +
                rightSquare.Indices.Count +
                bottomSquare.Indices.Count +
                leftSquare.Indices.Count);
            // Adding each object to one list.
            indices.AddRange(mainSquare.Indices);
            indices.AddRange(topSquare.Indices);
            indices.AddRange(rightSquare.Indices);
            indices.AddRange(bottomSquare.Indices);
            indices.AddRange(leftSquare.Indices);
            // Assign master list to vertex model.
            Models[selector].Vertices = vertices.ToArray();
            Models[selector].Indices = indices.ToArray();
            ControlBase(ref vertices, ref indices);
        }
    }
}
