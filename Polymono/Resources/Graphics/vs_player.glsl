#version 440 core

layout (location = 0) in vec3 vPosition;
layout (location = 1) in vec3 vNormal;
layout (location = 2) in vec4 vColour;

out vec4 finalColour;

layout (location = 16) uniform mat4 model;
layout (location = 17) uniform mat4 view;
layout (location = 18) uniform mat4 projection;

void main()
{
    gl_Position = projection * view * model * vec4(vPosition.xyz, 1.0);
	finalColour = vColour;
}