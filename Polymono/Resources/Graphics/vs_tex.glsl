#version 400 core

in vec3 vPosition;

void main()
{
    gl_Position = vec4(vPosition.xyz, 1.0);
}