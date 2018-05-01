using OpenTK;
using Polymono.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Game {
    class GameObject {
        public static int TOTAL_IDS = 0;
        public int ID;
        // Modelling
        public AModel Model;
        // Matrices
        public Matrix4 ModelMatrix;
        // Spatial data
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scaling;
        // Origin data
        public Vector3 OriginPosition;
        public Vector3 OriginRotation;
        public Vector3 OriginScaling;

        public GameObject(Vector3 position, Vector3 rotation, Vector3 scaling)
        {
            ID = TOTAL_IDS++;
            Position = position;
            Rotation = rotation;
            Scaling = scaling;
            OriginPosition = position;
            OriginRotation = rotation;
            OriginScaling = scaling;
        }

        public void Translate(Vector3 translation)
        {
            Position += translation;
        }

        public void SetTranslate(Vector3 translation)
        {
            Position = translation;
        }

        public void Rotate(Vector3 rotation)
        {
            Rotation += rotation;
        }

        public void Scale(Vector3 scaling)
        {
            Scaling += scaling;
        }

        public void ResetModel()
        {
            Position = OriginPosition;
            Rotation = OriginRotation;
            Scaling = OriginScaling;
        }

        public void UpdateModelMatrix()
        {
            ModelMatrix =
                Matrix4.CreateScale(Scaling) *
                Matrix4.CreateRotationX(Rotation.X) *
                Matrix4.CreateRotationY(Rotation.Y) *
                Matrix4.CreateRotationZ(Rotation.Z) *
                Matrix4.CreateTranslation(Position);
        }
    }
}
