#version 440 core

in vec4 finalColour;
in vec2 finalTexture;

out vec4 FragColor;

uniform sampler2D textureObject;

void main()
{
    FragColor = texture(textureObject, finalTexture) * finalColour;
} 