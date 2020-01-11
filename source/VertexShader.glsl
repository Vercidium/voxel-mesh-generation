#version 330

flat out int texID;
flat out float brightness;
out vec2 UV;

layout (location = 0) in int aData;

// Rather than storing block positions in the range 0-512,
// we store them 0-31 and move the chunks into position with a mat4 translation
// Therefore mvp contains the chunk's translation combined with the view projection
uniform mat4 mvp;

// Some textures are 1x1 blocks, some are 4x4, etc.
// The sizes of the textures are stored in this array
uniform vec2 uvSize[32];

// The global map position of the chunk
uniform vec3 chunkOffset;

void main()
{
    // Get the chunk-relative position from the first 18 bits
    vec3 position = vec3(float(aData&(63)), float((aData >> 6)&(63)), float((aData >> 12)&(63)));

    gl_Position = mvp * vec4(position, 1.0);

    // 5 bits for textureID
    texID = int((aData >> 18)&(31));

    // 4 bits for brightness (health)
    brightness = (float((aData >> 23)&(15)) + 2) / 8.0;

    // 3 bits for normal
    int normal = int((aData >> 27)&(7));

    // As textures repeat, we can use the position of blocks to calculate UVs.
    // Add the global map position of the chunk so UVs are calculated correctly
    position += chunkOffset;

    // If Y- or Y+
    if (normal < 2)
    {
        UV = position.xz * uvSize[texID];
        brightness *= normal == 0 ? 1.3 : 0.85;
    }
    else
    {
        UV = (normal < 4 ? position.zy : position.xy) * uvSize[texID];

        // If X- or X+
        if (normal < 4)
            brightness *= 1.15;

    }
}