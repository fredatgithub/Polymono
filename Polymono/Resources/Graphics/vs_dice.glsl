#version 440 core

layout (location = 0) in vec3 vPosition;
layout (location = 1) in vec3 vNormal;
layout (location = 2) in vec4 vColour;
layout (location = 3) in vec2 texcoord;

out vec3 v_norm;
out vec3 v_pos;
out vec2 f_texcoord;
out vec4 finalColour;

layout (location = 16) uniform mat4 model;
layout (location = 17) uniform mat4 view;
layout (location = 18) uniform mat4 projection;

void main()
{
	gl_Position = projection * view * model * vec4(vPosition, 1.0);
	f_texcoord = texcoord;

	mat3 normMatrix = transpose(inverse(mat3(model)));
	v_norm = normMatrix * vNormal;
	v_pos = (model * vec4(vPosition, 1.0)).xyz;
}