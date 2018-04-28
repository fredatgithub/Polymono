using OpenTK;
using OpenTK.Graphics;
using Polymono.Graphics;
using Polymono.Graphics.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Graphics.Components
{
    class Label : Control
    {
        public Label(ShaderProgram program, 
            Vector3 position, Vector3 rotation, Vector3 scaling,
            Dictionary<int, Control> controls, Dictionary<int, AModel> models, Menu menu,
            int width, int height, int buffer, int windowWidth, int windowHeight, 
            Matrix4 projection, Color4 colour, string text = "", string fontLocation = "arial")
            : base(program, position, rotation, scaling, controls, models, menu, width, height, windowWidth, windowHeight, projection, text, fontLocation)
        {
            Square mainSquare = new Square(
                0,                  // X position
                0,                  // Y position
                Width,              // Width
                -Height,            // Height
                colour);
            // Create vertex list.
            List<Vertex> vertices = new List<Vertex>(mainSquare.Vertices.Count);
            // Adding each object to one list.
            vertices.AddRange(mainSquare.Vertices);
            // Create indices list.
            List<int> indices = new List<int>(mainSquare.Indices.Count);
            // Adding each object to one list.
            indices.AddRange(mainSquare.Indices);
            // Assign master list to vertex model.
            Models["Default"].Vertices = vertices.ToArray();
            Models["Default"].Indices = indices.ToArray();
            ControlBase(ref vertices, ref indices);
        }
    }
}
