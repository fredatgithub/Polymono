using OpenTK;
using OpenTK.Graphics;
using Polymono.Graphics;
using Polymono.Vertices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Game {
    class Board : GameObject {
        public Property[] Property;

        public Board(ShaderProgram program)
        {
            // Set vertices.
            List<Vertex> vertices = new List<Vertex> {
                new Vertex(new Vector3(-0.5f, -0.5f, 0.0f), Color4.White, new Vector2(1.0f, 1.0f)),
                new Vertex(new Vector3(-0.5f, 0.5f, 0.0f), Color4.White, new Vector2(0.0f, 1.0f)),
                new Vertex(new Vector3(0.5f, 0.5f, 0.0f), Color4.White, new Vector2(0.0f, 0.0f)),
                new Vertex(new Vector3(0.5f, -0.5f, 0.0f), Color4.White, new Vector2(1.0f, 0.0f))
            };

            int[] indices = new int[] {
                0, 1, 3,
                2, 3, 1
            };

            float scaleFactor = 5.0f;

            Model = new Model(program, vertices, indices,
                Vector3.Zero, new Vector3(GameClient.ToRadians(-90.0f), 0.0f, 0.0f), new Vector3(scaleFactor),
                    @"Resources\Textures\polymono.png");
            Property = new Property[40];
            float xSize = 0.5f * scaleFactor;
            float zSize = 0.5f * scaleFactor;
            Property[0] = new Property(new Vector3(-xSize, 0.0f, zSize));
        }
    }
}
