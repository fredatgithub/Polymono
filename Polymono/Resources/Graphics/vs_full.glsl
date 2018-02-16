#version 440 core

layout (location = 0) in vec3 vPosition;
layout (location = 1) in vec4 vColour;
layout (location = 2) in vec2 vTexture;

out vec4 finalColour;
out vec2 finalTexture;

layout (location = 16) uniform mat4 model;
layout (location = 17) uniform mat4 view;
layout (location = 18) uniform mat4 projection;

void main()
{
    gl_Position = projection * view * model * vec4(vPosition.xyz, 1.0);
	finalColour = vColour;
	finalTexture = vTexture;
}