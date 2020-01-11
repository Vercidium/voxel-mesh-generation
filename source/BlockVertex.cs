public enum FaceTypeShifted : int
{
    yp = 0 << 27,
    yn = 1 << 27,
    xp = 2 << 27,
    xn = 3 << 27,
    zp = 4 << 27,
    zn = 5 << 27,
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public static class BlockVertex
{
    public static int[] indexToTextureShifted;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AppendQuadX(BlockVertexBuffer buffer, int x, int yL, int yR, int kL, int kR, int normal, int textureHealth)
    {
        var shared = x             |
                     textureHealth |
                     normal;

        buffer.data[buffer.used]     = yR | kL | shared;
        buffer.data[buffer.used + 1] = buffer.data[buffer.used + 4] = yL | kL | shared;
        buffer.data[buffer.used + 2] = buffer.data[buffer.used + 3] = yR | kR | shared;
        buffer.data[buffer.used + 5] = yL | kR | shared;

        buffer.used += 6;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AppendQuadY(BlockVertexBuffer buffer, int xL, int xR, int y, int zL, int zR, int normal, int textureHealth)
    {
        var shared = y             |
                     textureHealth |
                     normal;

        buffer.data[buffer.used]     = xL |zR | shared;
        buffer.data[buffer.used + 1] = buffer.data[buffer.used + 4] = xL | zL | shared;
        buffer.data[buffer.used + 2] = buffer.data[buffer.used + 3] = xR | zR | shared;
        buffer.data[buffer.used + 5] = xR | zL | shared;

        buffer.used += 6;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AppendQuadZ(BlockVertexBuffer buffer, int xL, int xR, int yL, int yR, int z, int normal, int textureHealth)
    {
        var shared = z             |
                     textureHealth |
                     normal;

        buffer.data[buffer.used]     = xR | yR | shared;
        buffer.data[buffer.used + 1] = buffer.data[buffer.used + 4] = xR | yL | shared;
        buffer.data[buffer.used + 2] = buffer.data[buffer.used + 3] = xL | yR | shared;
        buffer.data[buffer.used + 5] = xL | yL | shared;

        buffer.used += 6;
    }
}