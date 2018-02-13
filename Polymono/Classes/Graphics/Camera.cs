﻿using OpenTK;
using System;

namespace Polymono.Classes.Graphics {
    public enum CameraMovement {
        FORWARD, BACKWARD, LEFT, RIGHT
    }

    class Camera {
        public Vector3 Position = Vector3.Zero;
        public Vector3 Front;
        public Vector3 Up;
        public Vector3 Right;
        public Vector3 WorldUp = Vector3.UnitY;
        // Position on screen.
        public Vector2 LastPosition = new Vector2();

        public float Yaw = -90.0f;
        public float Pitch = 0.0f;
        public float MovementSpeed = 2.5f;
        public float MouseSensitivity = 0.1f;
        public float Zoom = 45.0f;
        
        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + Front, Up);
        }

        public void ProcessKeyboard(CameraMovement direction, float deltaTime)
        {
            float velocity = MovementSpeed * deltaTime;
            if (direction == CameraMovement.FORWARD)
                Position += Front * velocity;
            if (direction == CameraMovement.BACKWARD)
                Position -= Front * velocity;
            if (direction == CameraMovement.LEFT)
                Position -= Right * velocity;
            if (direction == CameraMovement.RIGHT)
                Position += Right * velocity;
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
            Vector3 front = new Vector3 {
                X = (float)(Math.Cos(GameClient.ToRadians(Yaw)) * Math.Cos(GameClient.ToRadians(Pitch))),
                Y = (float)Math.Sin(GameClient.ToRadians(Pitch)),
                Z = (float)(Math.Sin(GameClient.ToRadians(Yaw)) * Math.Cos(GameClient.ToRadians(Pitch)))
            };
            Front = Vector3.Normalize(front);
            // Also re-calculate the Right and Up vector
            Right = Vector3.Normalize(Vector3.Cross(Front, WorldUp));  // Normalize the vectors, because their length gets closer to 0 the more you look up or down which results in slower movement.
            Up = Vector3.Normalize(Vector3.Cross(Right, Front));
        }
    }
}
