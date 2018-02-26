using OpenTK;
using Polymono.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Game {
    class Dice : GameObject {
        public Dice(ShaderProgram program)
        {
            Model = new ModelObject(program, @"Resources\Objects\cube.obj",
                    new Vector3(0.25f, 0.05f, 0.0f), Vector3.Zero, new Vector3(0.05f),
                    @"Resources\Textures\cube_textured_uv.png",
                    @"Resources\Objects\cube.mtl",
                    @"Material");
        }
    }
}
