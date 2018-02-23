#version 400 core

in vec3 vPosition;

out vec3 position;

uniform mat4 projection;
uniform mat4 view;
uniform mat4 model;

void main()
{
    gl_Position = projection * view * model * vec4(vPosition.xyz, 1.0);
	position = vPosition;
}