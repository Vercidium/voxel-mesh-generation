public class ChunkHelper
{
    public bool[] visitXN = new bool[CHUNK_SIZE_CUBED];
    public bool[] visitXP = new bool[CHUNK_SIZE_CUBED];
    public bool[] visitZN = new bool[CHUNK_SIZE_CUBED];
    public bool[] visitZP = new bool[CHUNK_SIZE_CUBED];
    public bool[] visitYN = new bool[CHUNK_SIZE_CUBED];
    public bool[] visitYP = new bool[CHUNK_SIZE_CUBED];

    public void Reset()
    {
        // Clearing is faster than allocating a new array
        Array.Clear(visitXN, 0, CHUNK_SIZE_CUBED);
        Array.Clear(visitXP, 0, CHUNK_SIZE_CUBED);
        Array.Clear(visitYN, 0, CHUNK_SIZE_CUBED);
        Array.Clear(visitYP, 0, CHUNK_SIZE_CUBED);
        Array.Clear(visitZN, 0, CHUNK_SIZE_CUBED);
        Array.Clear(visitZP, 0, CHUNK_SIZE_CUBED);
    }
}