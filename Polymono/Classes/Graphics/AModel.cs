using Polymono.Classes.Vertices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Classes.Graphics {
    abstract class AModel {
        public static int TOTAL_IDS = 0;
        public int ID;

        public AModel()
        {
            ID = TOTAL_IDS++;
        }

        public abstract void CreateBuffer();

        public abstract void Render();

        public abstract void Delete();
    }
}
