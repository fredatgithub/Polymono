#version 400 core

in vec3 vPosition;
in vec3 vNormal;
in vec4 vColour;

out vec4 finalColour;
out vec2 fTexture;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
    gl_Position = projection * view * model * vec4(vPosition.xyz, 1.0);
	finalColour = vColour;
}