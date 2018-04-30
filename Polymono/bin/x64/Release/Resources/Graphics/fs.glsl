#version 400 core

in vec2 finalTexture;

out vec4 FragColor;

uniform sampler2D textureObject;
uniform vec4 overrideColour = vec4(0.0, 0.0, 1.0, 1.0);

void main()
{
    FragColor = texture(textureObject, finalTexture) * overrideColour;
}