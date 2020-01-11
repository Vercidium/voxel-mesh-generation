
public class Chunk
{        
    const int EMPTY = 0;
    const int CHUNK_SIZE = 32;
    const int CHUNK_SIZE_SQUARED = 1024;
    const int CHUNK_SIZE_CUBED = 32768;
    const int CHUNK_SIZE_MINUS_ONE = 31;
    const int CHUNK_SIZE_SHIFTED = 32 << 6;

    public Block[] data = new Block[Constants.ChunkSizeCubed];

    public BlockVertexBuffer vertexBuffer;

    // Parent reference to access blocks in other chunks
    public Map m;

    // The position of this chunk in the chunk grid.
    // Maps are usually 16 chunks wide, 16 chunks long and 6 chunks tall
    public int chunkPosX, chunkPosY, chunkPosZ;

    // Height maps
    public byte[] MinY = new byte[CHUNK_SIZE_SQUARED];
    public byte[] MaxY = new byte[CHUNK_SIZE_SQUARED];

    ChunkHelper chunkHelper;
    ChunkBase cXN, cXP, cYN, cYP, cZN, cZP;

    public Chunk(Map map, int x, int y, int z)
    {
        m = map;
        chunkPosX = x;
        chunkPosY = y;
        chunkPosZ = z;

        // Set min defaults to 32
        for (int i = Constants.ChunkSizeSquared - 1; i >= 0; i--)
            MinY[i] = (byte)CHUNK_SIZE;
    }

    public void GenerateMesh()
    {
        // Default 4096, else use the lase size + 1024
        int newSize = vertexBuffer.used == 0 ? 4096 : vertexBuffer.used + 1024;
        vertexBuffer.Reset(newSize);

        // Negative X side
        cXN = chunkPosX > 0 ? m.chunks[chunkPosX - 1, chunkPosY, chunkPosZ] : null;

        // Positive X side
        cXP = chunkPosX < Constants.ChunkXAmount - 1 ? m.chunks[chunkPosX + 1, chunkPosY, chunkPosZ] : null;

        // Negative Y side
        cYN = chunkPosY > 0 ? m.chunks[chunkPosX, chunkPosY - 1, chunkPosZ] : null;

        // Positive Y side
        cYP = chunkPosY < Constants.ChunkYAmount - 1 ? m.chunks[chunkPosX, chunkPosY + 1, chunkPosZ] : null;

        // Negative Z neighbour
        cZN = chunkPosZ > 0 ? m.chunks[chunkPosX, chunkPosY, chunkPosZ - 1] : null;

        // Positive Z side
        cZP = chunkPosZ < Constants.ChunkZAmount - 1 ? m.chunks[chunkPosX, chunkPosY, chunkPosZ + 1] : null;

        // Precalculate the map-relative Y position of the chunk in the map
        int chunkY = chunkPosY * CHUNK_SIZE;

        // Allocate variables on the stack
        int access, heightMapAccess, iCS, kCS2, i1, k1, j, topJ;
        bool minXEdge, maxXEdge, minZEdge, maxZEdge;

        k1 = 1;

        for (int k = 0; k < CHUNK_SIZE; k++, k1++)
        {
            // Calculate this once, rather than multiple times in the inner loop
            kCS2 = k * CHUNK_SIZE_SQUARED;

            i1 = 1;
            heightMapAccess = k * CHUNK_SIZE;
            
            // Is the current run on the Z- or Z+ edge of the chunk
            minZEdge = k == 0;
            maxZEdge = k == CHUNK_SIZE_MINUS_ONE;

            for (int i = 0; i < CHUNK_SIZE; i++; i1++)
            {
                // Determine where to start the innermost loop
                j = MinY[heightMapAccess];
                topJ = MaxY[heightMapAccess];
                heightMapAccess++;
                
                // Calculate this once, rather than multiple times in the inner loop
                iCS = i * CHUNK_SIZE;
                
                // Calculate access here and increment it each time in the innermost loop
                access = kCS2 + iCS + j;

                // Is the current run on the X- or X+ edge of the chunk
                minX = i == 0;
                maxX = i == CHUNK_SIZE_MINUS_ONE;

                // X and Z runs search upwards to create runs, so start at the bottom.
                for (; j < topJ; j++, access++)
                {
                    ref Block b = ref data[access];

                    if (b.kind != EMPTY)
                    {
                        CreateRun(ref b, i, j, k << 12, i1, k1 << 12, j + chunkY, access, minX, maxX, j == 0, j == CHUNK_SIZE_MINUS_ONE, minZ, maxZ, iCS, kCS2);
                    }
                }

                // Extend the array if it is nearly full
                if (vertexBuffer.used > vertexBuffer.data.Length - 2048)
                    vertexBuffer.Extend(2048);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void CreateRun(ref Block b, int i, int j, int k, int i1, int k1, int y, int access, bool minX, bool maxX, bool minY, bool maxY, bool minZ, bool maxZ, int iCS, int kCS2)
    {
        int textureHealth16 = BlockVertex.indexToTextureShifted[b.index] | ((b.health / 16) << 23);
        int runFinish;
        int accessIncremented = access + 1;
        int chunkAccess;
        int j1 = j + 1;
        int jS = j << 6;
        int jS1 = j1 << 6;

        // Left (X-)
        int textureHealth16 = BlockVertex.indexToTextureShifted[b.index] | ((b.health / 16) << 23);
        int length;
        int accessIncremented = access + 1;
        int chunkAccess;
        int j1 = j + 1;
        int jS = j << 6;
        int jS1 = j1 << 6;

        // Left (X-)
        if (!chunkHelper.visitXN[access] && DrawFaceXN(j, access, minX, kCS2))
        {
            chunkHelper.visitXN[access] = true;
            chunkAccess = accessIncremented;
            
            for (length = jS1; length < Constants.ChunkSizeShifted; length += (1 << 6))
            {
                if (DifferentBlock(chunkAccess, ref b))
                    break;

                chunkHelper.visitXN[chunkAccess++] = true;
            }

            // k1 and k are already shifted
            BlockVertex.AppendQuadX(vertexBuffer, i, jS, length, k1, k, (int)FaceTypeShifted.xn, textureHealth16);
        }
        
        // Right (X+)
        if (!chunkHelper.visitXP[access] && DrawFaceXP(j, access, maxX, kCS2))
        {
            chunkHelper.visitXP[access] = true;

            chunkAccess = accessIncremented;

            for (length = jS1; length < Constants.ChunkSizeShifted; length += (1 << 6))
            {
                if (DifferentBlock(chunkAccess, ref b))
                    break;

                chunkHelper.visitXP[chunkAccess++] = true;
            }

            BlockVertex.AppendQuadX(vertexBuffer, i1, jS, length, k, k1, (int)FaceTypeShifted.xp, textureHealth16);
        }
        
        // Back (Z-)
        if (!chunkHelper.visitZN[access] && DrawFaceZN(j, access, minZ, iCS))
        {
            chunkHelper.visitZN[access] = true;

            chunkAccess = accessIncremented;

            for (length = jS1; length < Constants.ChunkSizeShifted; length += (1 << 6))
            {
                if (DifferentBlock(chunkAccess, ref b))
                    break;

                chunkHelper.visitZN[chunkAccess++] = true;
            }

            BlockVertex.AppendQuadZ(vertexBuffer, i1, i, jS, length, k, (int)FaceTypeShifted.zn, textureHealth16);
        }

        // Front (Z+)
        if (!chunkHelper.visitZP[access] && DrawFaceZP(j, access, maxZ, iCS))
        {
            chunkHelper.visitZP[access] = true;

            chunkAccess = accessIncremented;

            for (length = jS1; length < Constants.ChunkSizeShifted; length += (1 << 6))
            {
                if (DifferentBlock(chunkAccess, ref b))
                    break;

                chunkHelper.visitZP[chunkAccess++] = true;
            }

            BlockVertex.AppendQuadZ(vertexBuffer, i, i1, jS, length, k1, (int)FaceTypeShifted.zp, textureHealth16);
        }

        // Bottom (Y-)
        if (y > 0 && !chunkHelper.visitYN[access] && DrawFaceYN(access, minY, iCS, kCS2))
        {
            chunkHelper.visitYN[access] = true;

            chunkAccess = access + Constants.ChunkSize;

            for (length = i1; length < Constants.ChunkSize; length++)
            {
                if (DifferentBlock(chunkAccess, ref b))
                    break;

                chunkHelper.visitYN[chunkAccess] = true;

                chunkAccess += Constants.ChunkSize;
            }

            BlockVertex.AppendQuadY(vertexBuffer, i, length, jS, k1, k, (int)FaceTypeShifted.yn, textureHealth16);
        }

        // Top (Y+)
        if (!chunkHelper.visitYP[access] && DrawFaceYP(access, maxY, iCS, kCS2))
        {
            chunkHelper.visitYP[access] = true;

            chunkAccess = access + Constants.ChunkSize;

            for (length = i1; length < Constants.ChunkSize; length++)
            {
                if (DifferentBlock(chunkAccess, ref b))
                    break;

                chunkHelper.visitYP[chunkAccess] = true;

                chunkAccess += Constants.ChunkSize;
            }

            BlockVertex.AppendQuadY(vertexBuffer, i, length, jS1, k, k1, (int)FaceTypeShifted.yp, textureHealth16);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool DrawFaceXN(int j, int access, bool min, int kCS2)
    {
        if (min)
        {
            if (chunkPosX == 0)
                return false;

            if (cXN == null)
                return true;

            // If it is outside this chunk, get the block from the neighbouring chunk
            return cXN.data[31 * Constants.ChunkSize + j + kCS2].index == 0;
        }

        return data[access - Constants.ChunkSize].index == 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool DrawFaceXP(int j, int access, bool max, int kCS2)
    {
        if (max)
        {
            if (chunkPosX == Constants.ChunkXAmount - 1)
                return false;

            if (cXP == null)
                return true;

            // If it is outside this chunk, get the block from the neighbouring chunk
            return cXP.data[j + kCS2].index == 0;
        }

        return data[access + Constants.ChunkSize].index == 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool DrawFaceYN(int access, bool min, int iCS, int kCS2)
    {
        if (min)
        {
            if (chunkPosY == 0)
                return false;

            if (cYN == null)
                return true;

            // If it is outside this chunk, get the block from the neighbouring chunk
            return cYN.data[iCS + 31 + kCS2].index == 0;
        }

        return data[access - 1].index == 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool VisibleFaceYP(int access, bool max, int iCS, int kCS2)
    {
        if (max)
        {
            // Don't check chunkYPos here as players can move above the map

            if (cYP == null)
                return true;

            return cYP.data[iCS + kCS2].index == 0;
        }

        return data[access + 1].index == 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool VisibleFaceZN(int j, int access, bool min, int iCS)
    {
        if (min)
        {
            if (chunkPosZ == 0)
                return false;

            if (cZN == null)
                return true;

            return cZN.data[iCS + j + 31 * Constants.ChunkSizeSquared].index == 0;
        }

        return data[access - Constants.ChunkSizeSquared].index == 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool VisibleFaceZP(int j, int access, bool max, int iCS)
    {
        if (max)
        {
            if (chunkPosZ == Constants.ChunkZAmount - 1)
                return false;

            if (cZP == null)
                return true;

            return cZP.data[iCS + j].index == 0;
        }

        return data[access + Constants.ChunkSizeSquared].index == 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool DifferentBlock(int chunkAccess, ref Block compare)
    {
        ref var b = ref data[chunkAccess];
        return b.index != compare.index || b.health != compare.health;
    }
}