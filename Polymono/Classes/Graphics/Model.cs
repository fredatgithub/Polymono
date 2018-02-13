using OpenTK;
using OpenTK.Graphics.OpenGL4;
using Polymono.Classes.Vertices;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Classes.Graphics
{
    class Model : AModel
    {
        // Buffer references
        public int VBO;
        public int VAO;
        public int IBO;
        public int TextureID;
        // Vertex data
        public Vertex[] Vertices;
        public int[] Indices;
        // Matrices
        public Matrix4 ModelMatrix;
        
        public Model() : this(new List<Vertex>(), new int[] { 0, 1, 2 }, Matrix4.Identity)
        {

        }

        public Model(List<Vertex> vertices, int[] indices, Matrix4 modelMatrix, string textureLocation = @"Resources\Textures\opentksquare.png")
            : base()
        {
            Vertices = vertices.ToArray();
            Indices = indices;
            ModelMatrix = modelMatrix;
            TextureID = CreateTexture(textureLocation);
        }

        public override void CreateBuffer()
        {
            VAO = GL.GenVertexArray();
            VBO = GL.GenBuffer();
            IBO = GL.GenBuffer();
            // Bind VAO then VBO for data inputs.
            GL.BindVertexArray(VAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IBO);
            // Input data to buffer.
            GL.NamedBufferStorage(VBO, Vertex.Size * Vertices.Length, Vertices, BufferStorageFlags.MapWriteBit);
            // Set vertex attribute pointers.
            // Position
            GL.VertexArrayAttribBinding(VAO, 0, 0);
            GL.EnableVertexArrayAttrib(VAO, 0);
            GL.VertexArrayAttribFormat(VAO, 0, 3, VertexAttribType.Float, false, 0);
            // Colour
            GL.VertexArrayAttribBinding(VAO, 1, 0);
            GL.EnableVertexArrayAttrib(VAO, 1);
            GL.VertexArrayAttribFormat(VAO, 1, 4, VertexAttribType.Float, false, 3 * sizeof(float));
            // Texture
            GL.VertexArrayAttribBinding(VAO, 2, 0);
            GL.EnableVertexArrayAttrib(VAO, 2);
            GL.VertexArrayAttribFormat(VAO, 2, 2, VertexAttribType.Float, false, (3 + 4) * sizeof(float));
            // Link
            GL.VertexArrayVertexBuffer(VAO, 0, VBO, IntPtr.Zero, Vertex.Size);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(Indices.Length * sizeof(int)), Indices, BufferUsageHint.DynamicDraw);
            // Reset bindings.
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        public override void Render()
        {
            GL.BindVertexArray(VAO);
            GL.BindTexture(TextureTarget.Texture2D, TextureID);
            GL.UniformMatrix4(16, false, ref ModelMatrix);
            GL.DrawElements(BeginMode.Triangles, Indices.Length, DrawElementsType.UnsignedInt, 0);

        }

        public override void Delete()
        {
            GL.DeleteVertexArray(VAO);
            GL.DeleteBuffer(VBO);
            GL.DeleteBuffer(IBO);
            GL.DeleteTexture(TextureID);
        }

        private int CreateTexture(string filename)
        {
            int width, height;
            float[] data;
            using (var bmp = (Bitmap)Image.FromFile(filename))
            {
                width = bmp.Width;
                height = bmp.Height;
                data = new float[width * height * 4];
                int index = 0;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        var pixel = bmp.GetPixel(x, y);
                        data[index++] = pixel.R / 255f;
                        data[index++] = pixel.G / 255f;
                        data[index++] = pixel.B / 255f;
                        data[index++] = pixel.A / 255f;
                    }
                }
            }
            GL.CreateTextures(TextureTarget.Texture2D, 1, out int texture);
            GL.TextureStorage2D(texture, 1, SizedInternalFormat.Rgba32f, width, height);
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TextureSubImage2D(texture, 0, 0, 0, width, height, OpenTK.Graphics.OpenGL4.PixelFormat.Rgba,
                PixelType.Float, data);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            return texture;
        }
    }
}
