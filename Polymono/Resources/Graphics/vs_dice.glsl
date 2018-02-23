#version 400 core

in vec3 vPosition;
in vec3 vNormal;
in vec4 vColour;
in vec2 vTexture;

out vec3 v_norm;
out vec3 v_pos;
out vec2 f_texcoord;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
	gl_Position = projection * view * model * vec4(vPosition, 1.0);
	f_texcoord = vTexture;

	mat3 normMatrix = transpose(inverse(mat3(model)));
	v_norm = normMatrix * vNormal;
	v_pos = (model * vec4(vPosition, 1.0)).xyz;
}