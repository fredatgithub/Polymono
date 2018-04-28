using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Graphics.Geometry
{
    struct Square
    {
        public List<Vertex> Vertices;
        public List<int> Indices;

        public Square(float x, float y, float width, float height,
            Color4 colour, 
            int indicesIndex = 0)
            : this(new Vector3(x, y, 0.0f), new Vector3(x + width, y, 0.0f),
                  new Vector3(x + width, y + height, 0.0f), new Vector3(x, y + height, 0.0f),
                  colour, colour, colour, colour,
                  new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f),
                  new Vector2(1.0f, 1.0f), new Vector2(0.0f, 1.0f),
                  indicesIndex)
        {

        }

        public Square(Vector2 pos1, Vector2 pos2, Vector2 pos3, Vector2 pos4,
            Color4 colour,
            Vector2 texPos1, Vector2 texPos2, Vector2 texPos3, Vector2 texPos4,
            int indicesIndex = 0)
            : this(new Vector3(pos1.X, pos1.Y, 0.0f), new Vector3(pos2.X, pos2.Y, 0.0f),
                  new Vector3(pos3.X, pos3.Y, 0.0f), new Vector3(pos4.X, pos4.Y, 0.0f),
                  colour, colour, colour, colour,
                  texPos1, texPos2, texPos3, texPos4,
                  indicesIndex)
        {

        }

        public Square(Vector3 pos1, Vector3 pos2, Vector3 pos3, Vector3 pos4,
            Color4 colour1, Color4 colour2, Color4 colour3, Color4 colour4,
            Vector2 texPos1, Vector2 texPos2, Vector2 texPos3, Vector2 texPos4,
            int indicesIndex = 0)
        {
            Vertices = new List<Vertex>
            {
                new Vertex(pos1, colour1, texPos1),
                new Vertex(pos2, colour2, texPos2),
                new Vertex(pos3, colour3, texPos3),
                new Vertex(pos4, colour4, texPos4)
            };
            Indices = new List<int>
            {
                indicesIndex + 0,
                indicesIndex + 1,
                indicesIndex + 2,
                indicesIndex + 0,
                indicesIndex + 3,
                indicesIndex + 2
            };
        }

        public int Append()
        {
            return Indices.Max() + 1;
        }
    }
}
