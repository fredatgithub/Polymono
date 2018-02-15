using OpenTK;
using OpenTK.Graphics;

namespace Polymono.Classes.Vertices {
    struct Vertex {
        public const int Size = (3 + 4 + 2) * 4;

        public Vector3 Position;
        public Vector4 Colour;
        public Vector2 Texture;

        public Vertex(Vector3 position, Color4 colour, Vector2 texture)
        {
            Position = position;
            Colour = new Vector4(colour.R, colour.G, colour.B, colour.A);
            Texture = texture;
        }
    }

    struct ColouredVertex {
        public const int Size = (3 + 4) * 4;

        public Vector3 Position;
        public Vector4 Colour;

        public ColouredVertex(Vector3 position, Color4 colour)
        {
            Position = position;
            Colour = new Vector4(colour.R, colour.G, colour.B, colour.A);
        }
    }

    struct NormalVertex {
        public const int Size = (3 + 3 + 4) * 4;

        public Vector3 Position;
        public Vector3 Normal;
        public Vector4 Colour;

        public NormalVertex(Vector3 position, Vector3 normal, Color4 colour)
        {
            Position = position;
            Normal = normal;
            Colour = new Vector4(colour.R, colour.G, colour.B, colour.A);
        }
    }

    struct ObjectVertex {
        public const int Size = (3 + 3 + 4 + 2) * 4;

        public Vector3 Position;
        public Vector3 Normal;
        public Vector4 Colour;
        public Vector2 Texture;

        public ObjectVertex(Vector3 position, Vector3 normal, Color4 colour, Vector2 texture)
        {
            Position = position;
            Normal = normal;
            Colour = new Vector4(colour.R, colour.G, colour.B, colour.A);
            Texture = texture;
        }
    }
}
