using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Drawing;
using System.IO;

namespace Polymono.Graphics
{
    abstract class AModel
    {
        // Identification
        public static int TotalID = 0;
        public int ID;
        // Buffer references
        public ShaderProgram Program;
        public int VBO;
        public int VAO;
        public int IBO;
        public int TextureID;
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
        // State
        public bool IsHidden { get; set; }

        public AModel(ShaderProgram program)
            : this(program, Vector3.Zero, Vector3.Zero, Vector3.One)
        {

        }

        public AModel(ShaderProgram program, Vector3 position, Vector3 rotation, Vector3 scaling)
        {
            ID = TotalID++;
            Program = program;
            Position = position;
            Rotation = rotation;
            Scaling = scaling;
            OriginPosition = position;
            OriginRotation = rotation;
            OriginScaling = scaling;
        }

        public abstract void CreateBuffer();

        public abstract void Update();

        public abstract void Render();

        public abstract void Delete();

        public int CreateTexture(string path, int quality = 1, bool repeat = false, bool flip_y = false)
        {
            Bitmap bitmap;
            try
            {
                bitmap = new Bitmap(path);
            }
            catch (FileNotFoundException e)
            {
                Polymono.Debug($"Cannot find {path}: {e}");
                return -1;
            }
            if (flip_y)
                bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
            int texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);
            switch (quality)
            {
                case 0:
                default:
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
                    break;
                case 1:
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Nearest);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Nearest);
                    break;
            }
            if (repeat)
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.Repeat);
            }
            else
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.ClampToEdge);
            }
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmap.Width, bitmap.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            System.Drawing.Imaging.BitmapData bitmap_data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, bitmap.Width, bitmap.Height, PixelFormat.Bgra, PixelType.UnsignedByte, bitmap_data.Scan0);
            bitmap.UnlockBits(bitmap_data);
            bitmap.Dispose();
            GL.BindTexture(TextureTarget.Texture2D, 0);
            return texture;
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

        public void Show()
        {
            IsHidden = false;
        }

        public void Hide()
        {
            IsHidden = true;
        }
    }
}
