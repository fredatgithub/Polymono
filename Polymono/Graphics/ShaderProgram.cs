using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.IO;

namespace Polymono.Graphics
{
    class ShaderProgram
    {
        // Program identifiers.
        public int ProgramID;
        public string ProgramName;
        // Shader buffer pointers.
        public int VertexShader;
        public int FragmentShader;

        public ShaderProgram(string vLocation, string fLocation, string name = "")
        {
            bool vSuccess, fSuccess;
            ProgramName = name;
            vSuccess = SetShader(vLocation, ShaderType.VertexShader);
            fSuccess = SetShader(fLocation, ShaderType.FragmentShader);
            if (vSuccess && fSuccess)
            {
                CreateProgram();
            }
        }

        public void UseProgram()
        {
            GL.UseProgram(ProgramID);
        }

        public void Uniform1(string uniformName, float value)
        {
            int location = GetUniform(uniformName);
            if (location != -1)
            {
                GL.Uniform1(location, value);
            }
            else
            {
                Polymono.Debug($"Program [{ProgramID}]: Uniform1 could not be set. [{uniformName}] does not exist.");
            }
        }

        public void Uniform3(string uniformName, ref Vector3 vector)
        {
            int location = GetUniform(uniformName);
            if (location != -1)
            {
                GL.Uniform3(location, ref vector);
            }
            else
            {
                Polymono.Debug($"Program [{ProgramID}]: Uniform3 could not be set. [{uniformName}] does not exist.");
            }
        }

        public void UniformMatrix4(string uniformName, ref Matrix4 matrix)
        {
            int location = GetUniform(uniformName);
            if (location != -1)
            {
                GL.UniformMatrix4(location, false, ref matrix);
            }
            else
            {
                Polymono.Debug($"Program [{ProgramID}]: UniformMatrix4 could not be set. [{uniformName}] does not exist.");
            }
        }

        public int GetAttrib(string attributeName)
        {
            return GL.GetAttribLocation(ProgramID, attributeName);
        }

        public int GetUniform(string uniformName)
        {
            return GL.GetUniformLocation(ProgramID, uniformName);
        }

        private bool SetShader(string location, ShaderType type)
        {
            // Sets variables for shader compilation.
            string infoLog = "";
            if (type == ShaderType.VertexShader)
            {
                // Creates shader then assigns reference.
                VertexShader = GL.CreateShader(type);
                // Specifies the shader data to OpenGL.
                GL.ShaderSource(VertexShader, File.ReadAllText(@"Resources\Graphics\" + location));
                // Compiles the shader from the source data.
                GL.CompileShader(VertexShader);
                // Return the shader parameter on compilation status.
                GL.GetShader(VertexShader, ShaderParameter.CompileStatus, out int success);
                if (success != 1)
                {
                    // Get the shader information log on compilation failure.
                    infoLog = GL.GetShaderInfoLog(VertexShader);
                    Polymono.Debug($"Shader [{location}:{VertexShader}] failed to compile: {infoLog}");
                    return false;
                }
            }
            else if (type == ShaderType.FragmentShader)
            {
                // Creates shader then assigns reference.
                FragmentShader = GL.CreateShader(type);
                // Specifies the shader data to OpenGL.
                GL.ShaderSource(FragmentShader, File.ReadAllText(@"Resources\Graphics\" + location));
                // Compiles the shader from the source data.
                GL.CompileShader(FragmentShader);
                // Return the shader parameter on compilation status.
                GL.GetShader(FragmentShader, ShaderParameter.CompileStatus, out int success);
                if (success != 1)
                {
                    // Get the shader information log on compilation failure.
                    infoLog = GL.GetShaderInfoLog(FragmentShader);
                    Polymono.Debug($"Shader [{location}:{FragmentShader}] failed to compile: {infoLog}");
                    return false;
                }
            }
            return true;
        }

        private bool CreateProgram()
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
                Polymono.Debug($"Program [{ProgramName}:{ProgramID}] failed to link: {infoLog}");
                GL.DeleteShader(VertexShader);
                GL.DeleteShader(FragmentShader);
                return false;
            }
            GL.DeleteShader(VertexShader);
            GL.DeleteShader(FragmentShader);
            return true;
        }
    }
}
