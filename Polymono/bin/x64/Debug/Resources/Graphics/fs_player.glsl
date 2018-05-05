#version 400 core

in vec4 finalColour;

out vec4 FragColor;

uniform vec4 colour;

void main()
{
    FragColor = finalColour * colour;
} 