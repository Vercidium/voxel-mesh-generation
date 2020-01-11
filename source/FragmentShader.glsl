#version 330

out vec4 gColor;

flat in int texID;
flat in float brightness;
in vec2 UV;

uniform sampler2DArray colourTexture;

void main()
{
    gColor = vec4(texture(colourTexture, vec3(UV, texID)).rgb * brightness, 1.0);
}