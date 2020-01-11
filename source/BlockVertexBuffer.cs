public class BlockVertexBuffer
{
    public BlockVertexBuffer()
    {
        arrayHandle = Gl.GenVertexArray();
        bufferHandle = Gl.GenBuffer();
        
        Gl.BindVertexArray(arrayHandle);
        Gl.BindBuffer(BufferTarget.ArrayBuffer, bufferHandle);

        Gl.EnableVertexAttribArray(0);

        vertexSize = Marshal.SizeOf(typeof(uint));
        Gl.VertexAttribIPointer(0, 1, VertexAttribType.Int, vertexSize, IntPtr.Zero);

        Gl.BindBuffer(BufferTarget.ArrayBuffer, 0);
        Gl.BindVertexArray(0);
    }

    uint arrayHandle;
    uint bufferHandle;
    int vertexSize;

    public int used;
    int lastUsed;
    public bool dirty = true;
    public bool initialised = false;


    int[] data;
    
    public void Reset(int length)
    {
        used = 0;
        data = new int[length];
        dirty = true;
    }

    public void Extend(int amount)
    {
        int[] newData = new int[data.Length + amount];
        Array.Copy(data, newData, data.Length);
        data = newData;
    }

    public void BufferData()
    {
        Gl.BindVertexArray(arrayHandle);

        if (used > 0 && dirty)
        {
            unsafe
            {
                fixed (int* p = data)
                {
                    Gl.BindBuffer(BufferTarget.ArrayBuffer, bufferHandle);

                    // BufferSubData if we can
                    if (used <= lastUsed < used)
                    {
                        Gl.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)0, (uint)(used * vertexSize), (IntPtr)p);
                    }
                    else
                    {
                        Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(used * vertexSize), (IntPtr)p, BufferUsage.StaticDraw);
                        lastUsed = used;
                    }

                    Gl.BindBuffer(BufferTarget.ArrayBuffer, 0);
                }
            }

            dirty = false;

            // Clear the data from memory as it is now stored on the GPU
            data = null;
        }

        Gl.BindVertexArray(0);
    }

    public void Draw()
    {
        Gl.BindVertexArray(arrayHandle);
        Gl.DrawArrays(PrimitiveType.Triangles, 0, used);
    }
}