using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Polymono.Vertices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polymono.Graphics {
    class Skybox : ModelObject {
        public new PositionVertex[] Vertices;

        public Skybox(string filename, bool giveID) : base(filename, giveID)
        {

        }

        public override void CreateBuffer()
        {
            VAO = GL.GenVertexArray();
            VBO = GL.GenBuffer();
            // Bind VAO then VBO for data inputs.
            GL.BindVertexArray(VAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            if (GameClient.MajorVersion == '4' && GameClient.MinorVersion < '5')
            {
                Polymono.Debug("Below 4.5 version.");
                // Input data to buffer.
                GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Length * PositionVertex.Size,
                    Vertices, BufferUsageHint.StaticDraw);
                // Set vertex attribute pointers.
                // Position
                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false,
                    PositionVertex.Size, 0);
            } else
            {
                Polymono.Debug("Above 4.5 version.");
                // Input data to buffer.
                GL.NamedBufferStorage(VBO, PositionVertex.Size * Vertices.Length, Vertices,
                    BufferStorageFlags.MapWriteBit);
                // Set vertex attribute pointers.
                // Position
                GL.VertexArrayAttribBinding(VAO, 0, 0);
                GL.EnableVertexArrayAttrib(VAO, 0);
                GL.VertexArrayAttribFormat(VAO, 0, 3, VertexAttribType.Float, false, 0);
                // Link
                GL.VertexArrayVertexBuffer(VAO, 0, VBO, IntPtr.Zero, PositionVertex.Size);
            }
            // Reset bindings.
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        public override void LoadFromString(string obj, Color4 colour)
        {
            // Seperate lines from the file
            List<String> lines = new List<string>(obj.Split('\n'));
            // Lists to hold model data
            List<Vector3> verts = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> texs = new List<Vector2>();
            List<Tuple<TempVertex, TempVertex, TempVertex>> faces = new List<Tuple<TempVertex, TempVertex, TempVertex>>();
            // Base values
            verts.Add(new Vector3());
            normals.Add(new Vector3());
            texs.Add(new Vector2());
            // Read file line by line
            foreach (String line in lines)
            {
                if (line.StartsWith("v ")) // Vertex definition
                {
                    // Cut off beginning of line
                    String temp = line.Substring(2);
                    Vector3 vec = new Vector3();
                    if (temp.Trim().Count((char c) => c == ' ') == 2) // Check if there's enough elements for a vertex
                    {
                        String[] vertparts = temp.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        // Attempt to parse each part of the vertice
                        bool success = float.TryParse(vertparts[0], out vec.X);
                        success |= float.TryParse(vertparts[1], out vec.Y);
                        success |= float.TryParse(vertparts[2], out vec.Z);
                        // If any of the parses failed, report the error
                        if (!success)
                        {
                            Console.WriteLine("Error parsing vertex: {0}", line);
                        }
                    } else
                    {
                        Console.WriteLine("Error parsing vertex: {0}", line);
                    }
                    verts.Add(vec);
                } else if (line.StartsWith("vt ")) // Texture coordinate
                {
                    // Cut off beginning of line
                    String temp = line.Substring(2);
                    Vector2 vec = new Vector2();
                    if (temp.Trim().Count((char c) => c == ' ') > 0) // Check if there's enough elements for a vertex
                    {
                        String[] texcoordparts = temp.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        // Attempt to parse each part of the vertice
                        bool success = float.TryParse(texcoordparts[0], out vec.X);
                        success |= float.TryParse(texcoordparts[1], out vec.Y);
                        // If any of the parses failed, report the error
                        if (!success)
                        {
                            Console.WriteLine("Error parsing texture coordinate: {0}", line);
                        }
                    } else
                    {
                        Console.WriteLine("Error parsing texture coordinate: {0}", line);
                    }
                    texs.Add(vec);
                } else if (line.StartsWith("vn ")) // Normal vector
                {
                    // Cut off beginning of line
                    String temp = line.Substring(2);
                    Vector3 vec = new Vector3();
                    if (temp.Trim().Count((char c) => c == ' ') == 2) // Check if there's enough elements for a normal
                    {
                        String[] vertparts = temp.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        // Attempt to parse each part of the vertice
                        bool success = float.TryParse(vertparts[0], out vec.X);
                        success |= float.TryParse(vertparts[1], out vec.Y);
                        success |= float.TryParse(vertparts[2], out vec.Z);
                        // If any of the parses failed, report the error
                        if (!success)
                        {
                            Console.WriteLine("Error parsing normal: {0}", line);
                        }
                    } else
                    {
                        Console.WriteLine("Error parsing normal: {0}", line);
                    }
                    normals.Add(vec);
                } else if (line.StartsWith("f ")) // Face definition
                {
                    // Cut off beginning of line
                    String temp = line.Substring(2);
                    Tuple<TempVertex, TempVertex, TempVertex> face = new Tuple<TempVertex, TempVertex, TempVertex>(new TempVertex(), new TempVertex(), new TempVertex());
                    if (temp.Trim().Count((char c) => c == ' ') == 2) // Check if there's enough elements for a face
                    {
                        String[] faceparts = temp.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        int v1, v2, v3, t1, t2, t3, n1, n2, n3;
                        // Attempt to parse each part of the face
                        bool success = int.TryParse(faceparts[0].Split('/')[0], out v1);
                        success |= int.TryParse(faceparts[1].Split('/')[0], out v2);
                        success |= int.TryParse(faceparts[2].Split('/')[0], out v3);
                        if (faceparts[0].Count((char c) => c == '/') >= 2)
                        {
                            success |= int.TryParse(faceparts[0].Split('/')[1], out t1);
                            success |= int.TryParse(faceparts[1].Split('/')[1], out t2);
                            success |= int.TryParse(faceparts[2].Split('/')[1], out t3);
                            success |= int.TryParse(faceparts[0].Split('/')[2], out n1);
                            success |= int.TryParse(faceparts[1].Split('/')[2], out n2);
                            success |= int.TryParse(faceparts[2].Split('/')[2], out n3);
                        } else
                        {
                            if (texs.Count > v1 && texs.Count > v2 && texs.Count > v3)
                            {
                                t1 = v1; t2 = v2; t3 = v3;
                            } else
                            {
                                t1 = 0; t2 = 0; t3 = 0;
                            }
                            if (normals.Count > v1 && normals.Count > v2 && normals.Count > v3)
                            {
                                n1 = v1; n2 = v2; n3 = v3;
                            } else
                            {
                                n1 = 0; n2 = 0; n3 = 0;
                            }
                        }
                        // If any of the parses failed, report the error
                        if (!success)
                        {
                            Console.WriteLine("Error parsing face: {0}", line);
                        } else
                        {
                            TempVertex tv1 = new TempVertex(v1, n1, t1);
                            TempVertex tv2 = new TempVertex(v2, n2, t2);
                            TempVertex tv3 = new TempVertex(v3, n3, t3);
                            face = new Tuple<TempVertex, TempVertex, TempVertex>(tv1, tv2, tv3);
                            faces.Add(face);
                        }
                    } else
                    {
                        Console.WriteLine("Error parsing face: {0}", line);
                    }
                }
            }
            Vertices = new PositionVertex[faces.Capacity * 3];
            int tempIndex = 0;
            foreach (var face in faces)
            {
                Vertices[tempIndex++] = new PositionVertex(
                    verts[face.Item1.Vertex]);
                Vertices[tempIndex++] = new PositionVertex(
                    verts[face.Item2.Vertex]);
                Vertices[tempIndex++] = new PositionVertex(
                    verts[face.Item3.Vertex]);
            }
        }

        public override void RenderObject(ProgramID id)
        {
            GL.BindVertexArray(VAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.UniformMatrix4(16, false, ref ModelMatrix);
            // Draw arrays...
            GL.DrawArrays(PrimitiveType.Triangles, 0, Vertices.Length);
        }

        class TempVertex {
            public int Vertex;
            public int Normal;
            public int Texcoord;

            public TempVertex(int vert = 0, int norm = 0, int tex = 0)
            {
                Vertex = vert;
                Normal = norm;
                Texcoord = tex;
            }
        }
    }
}
