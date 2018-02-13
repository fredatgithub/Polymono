using OpenTK.Graphics.OpenGL4;
using System;
using System.IO;

namespace Polymono.Classes.Graphics {
    class ShaderProgram {
        public int ProgramID;
        public int VertexShader;
        public int FragmentShader;

        public ShaderProgram(string vLocation, string fLocation, string name = "")
        {
            bool success;
            success = SetVertexShader(vLocation);
            success = SetFragmentShader(fLocation);
            if (success)
            {
                CreateProgram(name);
            }
        }

        public void UseProgram()
        {
            GL.UseProgram(ProgramID);
        }

        public bool CreateProgram(string name = "")
        {
            // Sets variables for shader compilation.
            int success = 0;
            string infoLog = "";
            // Creates program to use for shaders, returns reference.
            ProgramID = GL.CreateProgram();
            // Attaches the shaders to the program.
            GL.AttachShader(ProgramID, VertexShader);
            GL.AttachShader(ProgramID, FragmentShader);
            // Link the program to GPU; further rendering uses this program/shaders.
            GL.LinkProgram(ProgramID);
            // Return the program parameter on linking status.
            GL.GetProgram(ProgramID, GetProgramParameterName.LinkStatus, out success);
            if (success != 1)
            {
                // Get the program ifnormation log on linking failure.
                GL.GetProgramInfoLog(ProgramID, out infoLog);
                Console.WriteLine("Shader Program '" + name + "': LINK STATUS failed."
                    + Environment.NewLine + infoLog);
                GL.DeleteShader(VertexShader);
                GL.DeleteShader(FragmentShader);
                return false;
            }
            GL.DeleteShader(VertexShader);
            GL.DeleteShader(FragmentShader);
            return true;
        }

        public bool SetVertexShader(string location)
        {
            // Sets variables for shader compilation.
            int success = 0;
            string infoLog = "";
            // Creates vertex shader then assigns reference.
            VertexShader = GL.CreateShader(ShaderType.VertexShader);
            // Specifies the shader data to OpenGL.
            GL.ShaderSource(VertexShader, File.ReadAllText(@"Resources\Graphics\" + location));
            // Compiles the shader from the source data.
            GL.CompileShader(VertexShader);
            // Return the shader parameter on compilation status.
            GL.GetShader(VertexShader, ShaderParameter.CompileStatus, out success);
            if (success != 1)
            {
                // Get the shader information log on compilation failure.
                infoLog = GL.GetShaderInfoLog(VertexShader);
                Console.WriteLine("Vertex Shader '" + location + "': COMPILE STATUS failed."
                    + Environment.NewLine + infoLog);
                return false;
            }
            return true;
        }

        public bool SetFragmentShader(string location)
        {
            // Sets variables for shader compilation.
            int success = 0;
            string infoLog = "";
            // Creates vertex shader then assigns reference.
            FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            // Specifies the shader data to OpenGL.
            GL.ShaderSource(FragmentShader, File.ReadAllText(@"Resources\Graphics\" + location));
            // Compiles the shader from the source data.
            GL.CompileShader(FragmentShader);
            // Return the shader parameter on compilation status.
            GL.GetShader(FragmentShader, ShaderParameter.CompileStatus, out success);
            if (success != 1)
            {
                // Get the shader information log on compilation failure.
                infoLog = GL.GetShaderInfoLog(FragmentShader);
                Console.WriteLine("Fragment Shader '" + location + "': COMPILE STATUS failed."
                    + Environment.NewLine + infoLog);
                return false;
            }
            return true;
        }
    }
}
