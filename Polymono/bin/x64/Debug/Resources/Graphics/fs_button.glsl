#version 400 core

in vec4 fColour;
in vec2 fTexture;

out vec4 FragColor;

uniform sampler2D textureObject;

void main()
{
    FragColor = texture(textureObject, fTexture) * fColour;
}