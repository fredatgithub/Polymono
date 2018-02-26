using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Polymono.Vertices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Graphics
{
    public enum ButtonState
    {
        Pressing,
        Pressed,
        Hovering, // Takes too much processing???
        NotPressed
    }

    class Button : Model
    {
        // Button data
        string Label;
        ButtonState State;
        Action Callback;

        public Button(ShaderProgram program, string label, int x, int y, int width, int height, Action callback)
            : base(program)
        {
            // Create model, then populate button positional data to that model data.
            Vertices = new Vertex[] {
                new Vertex(// Top left
                    new Vector3(x, y, 0.0f),
                    Color4.Black,
                    new Vector2(0.0f, 0.0f)),
                new Vertex(// Top right
                    new Vector3(x + width, y, 0.0f),
                    Color4.White,
                    new Vector2(1.0f, 0.0f)),
                new Vertex(// Bottom right
                    new Vector3(x + width, y + height, 0.0f),
                    Color4.Black,
                    new Vector2(1.0f, 1.0f)),
                new Vertex(// Bottom left
                    new Vector3(x, y + height, 0.0f),
                    Color4.White,
                    new Vector2(0.0f, 1.0f))
            };
            Indices = new int[]
            {
                0, 1, 2,
                0, 3, 2
            };
            Label = label;
            State = ButtonState.NotPressed;
            Callback = callback;
        }

        public void Click(Vector2 mousePosition)
        {
            bool isHovering = PointInRectangle(Vertices[0].Position.Xy, Vertices[1].Position.Xy,
                Vertices[2].Position.Xy, Vertices[3].Position.Xy, mousePosition);
            if (isHovering && State == ButtonState.NotPressed)
            {
                State = ButtonState.Pressed;
                Callback();
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
    }
}
