#version 400 core

in vec3 vPosition;
in vec2 vTexture;
in vec3 vNormal;

out vec2 finalTexture;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
    gl_Position = projection * view * model * vec4(vPosition.xyz, 1.0);
	finalTexture = vTexture;
}