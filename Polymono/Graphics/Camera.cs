using OpenTK;
using System;

namespace Polymono.Graphics
{
    public enum CameraMovement
    {
        Forward, Backward, Left, Right, Up, Down
    }

    class Camera
    {
        // Camera properties.
        public Vector3 Position;
        public Vector3 Front;
        public Vector3 Up;
        public Vector3 Right;
        public Vector3 WorldUp;
        public float Yaw;
        public float Pitch;
        public float MovementSpeed;
        public float MouseSensitivity;
        public float Zoom;
        // Position on screen.
        public Vector2 LastPosition;

        public Camera(Vector3 position)
        {
            Position = position;
            Front = -Vector3.UnitZ;
            Up = Vector3.UnitY;
            Right = Vector3.UnitX;
            WorldUp = Vector3.UnitY;
            Yaw = -90.0f;
            Pitch = 0.0f;
            MovementSpeed = 2.5f;
            MouseSensitivity = 0.1f;
            Zoom = 75.0f;
            LastPosition = new Vector2();
        }

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + Front, Up);
        }

        public Matrix4 GetStaticViewMatrix()
        {
            return Matrix4.LookAt(Vector3.Zero, Vector3.Zero + Front, Up);
        }

        public void ProcessKeyboard(CameraMovement direction, float deltaTime)
        {
            float velocity = MovementSpeed * deltaTime;
            if (direction == CameraMovement.Forward)
                Position += Front * velocity;
            if (direction == CameraMovement.Backward)
                Position -= Front * velocity;
            if (direction == CameraMovement.Left)
                Position -= Right * velocity;
            if (direction == CameraMovement.Right)
                Position += Right * velocity;
            if (direction == CameraMovement.Up)
                Position += Up * velocity;
            if (direction == CameraMovement.Down)
                Position -= Up * velocity;
        }

        public void ProcessMouseMovement(float xoffset, float yoffset, bool constrainPitch = true)
        {
            xoffset *= MouseSensitivity;
            yoffset *= MouseSensitivity;
            Yaw -= xoffset;
            Pitch += yoffset;
            // Make sure that when pitch is out of bounds, screen doesn't get flipped
            if (constrainPitch)
            {
                if (Pitch > 89.0f)
                    Pitch = 89.0f;
                if (Pitch < -89.0f)
                    Pitch = -89.0f;
            }
            // Update Front, Right and Up Vectors using the updated Eular angles
            UpdateCamera();
        }

        public void ProcessMouseScroll(float yoffset)
        {
            if (Zoom >= 30.0f && Zoom <= 90.0f)
                Zoom -= yoffset;
            if (Zoom <= 30.0f)
                Zoom = 30.0f;
            if (Zoom >= 90.0f)
                Zoom = 90.0f;
        }

        public void UpdateCamera()
        {
            // Calculate the new Front vector
            Vector3 front = new Vector3
            {
                X = (float)(Math.Cos(AGameClient.ToRadians(Yaw)) * Math.Cos(AGameClient.ToRadians(Pitch))),
                Y = (float)Math.Sin(AGameClient.ToRadians(Pitch)),
                Z = (float)(Math.Sin(AGameClient.ToRadians(Yaw)) * Math.Cos(AGameClient.ToRadians(Pitch)))
            };
            Front = Vector3.Normalize(front);
            // Also re-calculate the Right and Up vector
            Right = Vector3.Normalize(Vector3.Cross(Front, WorldUp));  // Normalize the vectors, because their length gets closer to 0 the more you look up or down which results in slower movement.
            Up = Vector3.Normalize(Vector3.Cross(Right, Front));
        }
    }
}
