using OpenTK;
using OpenTK.Graphics.OpenGL4;
using Polymono.Vertices;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Graphics
{
    class Model : AModel
    {
        // Vertex data
        public Vertex[] Vertices;
        public int[] Indices;

        public Model(ShaderProgram program)
            : this(program, new List<Vertex>(), new int[] { 0, 1, 2 },
                  Vector3.Zero, Vector3.Zero, Vector3.One)
        {

        }

        public Model(ShaderProgram program, Vector3 position, Vector3 rotation, Vector3 scaling)
            : this(program, new List<Vertex>(), new int[] { 0, 1, 2 },
                  position, rotation, scaling)
        {

        }

        public Model(ShaderProgram program, List<Vertex> vertices, int[] indices,
            Vector3 position, Vector3 rotation, Vector3 scaling,
            string textureLocation = @"Resources\Textures\default.png")
            : base(program, position, rotation, scaling)
        {
            Vertices = vertices.ToArray();
            Indices = indices;
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
            int vPosition = Program.GetAttrib("vPosition");
            int vColour = Program.GetAttrib("vColour");
            int vTexture = Program.GetAttrib("vTexture");
            if (AGameClient.MajorVersion == '4' && AGameClient.MinorVersion < '5')
            {
                #region Low version
                // Input data to buffer.
                GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Length * Vertex.Size,
                    Vertices, BufferUsageHint.StaticDraw);
                GL.BufferData(BufferTarget.ElementArrayBuffer, Indices.Length * sizeof(int),
                    Indices, BufferUsageHint.StaticDraw);
                // Set vertex attribute pointers.
                // Position
                if (vPosition != -1)
                {
                    GL.EnableVertexAttribArray(vPosition);
                    GL.VertexAttribPointer(vPosition, 3, VertexAttribPointerType.Float, false,
                        Vertex.Size, 0);
                }
                // Colour
                if (vColour != -1)
                {
                    GL.EnableVertexAttribArray(vColour);
                    GL.VertexAttribPointer(vColour, 4, VertexAttribPointerType.Float, false,
                        Vertex.Size, 3 * sizeof(float));
                }
                // Texture
                if (vTexture != -1)
                {
                    GL.EnableVertexAttribArray(vTexture);
                    GL.VertexAttribPointer(vTexture, 2, VertexAttribPointerType.Float, false,
                        Vertex.Size, (3 + 4) * sizeof(float));
                }
                #endregion
            }
            else
            {
                #region High version
                // Input data to buffer.
                GL.NamedBufferStorage(VBO, Vertex.Size * Vertices.Length,
                    Vertices, BufferStorageFlags.MapWriteBit);
                GL.BufferData(BufferTarget.ElementArrayBuffer, Indices.Length * sizeof(int),
                    Indices, BufferUsageHint.StaticDraw);
                // Set vertex attribute pointers.
                // Position
                if (vPosition != -1)
                {
                    GL.VertexArrayAttribBinding(VAO, vPosition, 0);
                    GL.EnableVertexArrayAttrib(VAO, vPosition);
                    GL.VertexArrayAttribFormat(VAO, vPosition, 3, VertexAttribType.Float, false,
                        0);
                }
                // Colour
                if (vColour != -1)
                {
                    GL.VertexArrayAttribBinding(VAO, vColour, 0);
                    GL.EnableVertexArrayAttrib(VAO, vColour);
                    GL.VertexArrayAttribFormat(VAO, vColour, 4, VertexAttribType.Float, false,
                        3 * sizeof(float));
                }
                // Texture
                if (vTexture != -1)
                {
                    GL.VertexArrayAttribBinding(VAO, vTexture, 0);
                    GL.EnableVertexArrayAttrib(VAO, vTexture);
                    GL.VertexArrayAttribFormat(VAO, vTexture, 2, VertexAttribType.Float, false,
                        (3 + 4) * sizeof(float));
                }
                // Link
                GL.VertexArrayVertexBuffer(VAO, 0, VBO, IntPtr.Zero, Vertex.Size);
                #endregion
            }
            // Reset bindings.
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        public override void Update()
        {

        }

        public override void Render()
        {
            if (!IsHidden)
            {
                GL.BindVertexArray(VAO);
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, IBO);
                GL.BindTexture(TextureTarget.Texture2D, TextureID);
                Program.UniformMatrix4("model", ref ModelMatrix);
                GL.DrawElements(BeginMode.Triangles, Indices.Length, DrawElementsType.UnsignedInt, 0);
            }
        }

        public override void Delete()
        {
            GL.DeleteVertexArray(VAO);
            GL.DeleteBuffer(VBO);
            GL.DeleteBuffer(IBO);
            GL.DeleteTexture(TextureID);
        }
    }
}
