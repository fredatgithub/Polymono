﻿using OpenTK;
using OpenTK.Graphics.OpenGL4;
using Polymono.Vertices;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Graphics {
    class Model : AModel {
        // Vertex data
        public Vertex[] Vertices;
        public int[] Indices;

        public Model() : this(new List<Vertex>(), new int[] { 0, 1, 2 }, 
            Vector3.Zero, Vector3.Zero, Vector3.One)
        {

        }

        public Model(List<Vertex> vertices, int[] indices,
            Vector3 position, Vector3 rotation, Vector3 scaling, 
            string textureLocation = @"Resources\Textures\opentksquare.png")
            : base(position, rotation, scaling)
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
    }
}