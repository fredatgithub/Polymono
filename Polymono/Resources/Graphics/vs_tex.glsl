#version 450 core

layout (location = 0) in vec3 vPosition;

void main()
{
    gl_Position = vec4(vPosition.xyz, 1.0);
}