#version 450 core

in vec4 finalColour;

out vec4 FragColor;

void main()
{
    FragColor = finalColour;
} 