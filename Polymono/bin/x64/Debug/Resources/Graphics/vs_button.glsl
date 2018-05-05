#version 400 core

in vec3 vPosition;
in vec4 vColour;
in vec2 vTexture;

out vec4 fColour;
out vec2 fTexture;

uniform mat4 model;
uniform mat4 projection;

void main()
{
    gl_Position = projection * model * vec4(vPosition.xyz, 1.0);
	fColour = vColour;
	fTexture = vTexture;
}