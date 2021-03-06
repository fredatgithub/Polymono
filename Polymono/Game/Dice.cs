﻿using OpenTK;
using Polymono.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Game {
    struct DiceRoll
    {
        public int Roll1;
        public int Roll2;

        public DiceRoll(int roll1, int roll2)
        {
            Roll1 = roll1;
            Roll2 = roll2;
        }
    }

    class Dice : GameObject {
        public Random Random;

        public Dice(ShaderProgram program, Vector3 position, Vector3 rotation, Vector3 scaling)
            : base(position, rotation, scaling)
        {
            Model = new ModelObject(program, @"Resources\Objects\cube.obj",
                    Vector3.Zero, Vector3.Zero, Vector3.One,
                    @"Resources\Textures\cube_textured_uv.png",
                    @"Resources\Objects\cube.mtl",
                    @"Material");
            Random = new Random();
        }

        public int GetNumber()
        {
            return Random.Next(6) + 1;
        }
    }
}
