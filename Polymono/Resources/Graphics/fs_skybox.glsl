#version 400 core

in vec3 position;

out vec4 FragColor;

uniform float time;

void main()
{
	float x = position.x + (time * 4);
	float y = position.y + (time * 4);
	float z = position.z + (time * 4);
	x = x / 10;
	y = y / 10;
	z = z / 10;
	float newx = abs(sin(x));
	float newy = abs(sin(y));
	float newz = abs(sin(z));
    FragColor = vec4(newx, newy, newz, 1.0f);
}