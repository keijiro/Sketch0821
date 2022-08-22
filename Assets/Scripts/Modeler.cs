using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

namespace Sketch {

sealed class Modeler
{
    #region Model properties

    public float3 Origin { get; set; } = 0;
    public float Radius { get; set; } = 1;
    public float Height { get; set; } = 0.1f;
    public uint Resolution { get; set; } = 100;
    public uint Seed { get; set; } = 123;

    #endregion

    #region Private utility properties

    public uint VertexCount => 2 + Resolution * 4;
    public uint IndexCount => Resolution * 3 * 4;

    #endregion

    #region Public methods

    public void BuildGeometry(NativeSlice<float3> vertices,
                              NativeSlice<float4> uvs,
                              NativeSlice<uint> indices,
                              uint indexOffset)
    {
        BuildVertexArray(vertices);
        BuildUVArray(uvs);
        BuildIndexArray(indices, indexOffset);
    }

    #endregion

    #region Builder methods

    void BuildVertexArray(NativeSlice<float3> buffer)
    {
        var offs = math.float3(0, Height * 0.5f, 0);

        buffer[0] = Origin - offs;
        buffer[1] = Origin + offs;

        for (var i = 0; i < Resolution; i++)
        {
            var phi = math.PI * 2 / Resolution * i;
            var dir = math.float3(math.cos(phi), 0, math.sin(phi));
            var v = Origin + dir * Radius;
            buffer[2 + i * 4] = buffer[3 + i * 4] = v - offs;
            buffer[4 + i * 4] = buffer[5 + i * 4] = v + offs;
        }
    }

    void BuildUVArray(NativeSlice<float4> buffer)
    {
        var r = new Random(Seed);
        r.NextUInt4();
        r.NextUInt4();
        r.NextUInt4();

        var c = (Vector4)Color.HSVToRGB(r.NextFloat(), 1, 1);
        if (r.NextFloat() < 0.2f)
            c = math.float4(0.4f, 0.8f, 1, 5);
        else
            c.w = 0;

        for (var i = 0; i < VertexCount; i++) buffer[i] = c;
    }

    void BuildIndexArray(NativeSlice<uint> buffer, uint offs)
    {
        var count = 0;

        for (var i = 0u; i < Resolution; i++)
        {
            var i1 = 2 + i * 4;
            var i2 = 2 + (i == Resolution - 1 ? 0 : i + 1) * 4;

            buffer[count++] = offs + 0;
            buffer[count++] = offs + i1;
            buffer[count++] = offs + i2;

            buffer[count++] = offs + 1;
            buffer[count++] = offs + i2 + 3;
            buffer[count++] = offs + i1 + 3;

            buffer[count++] = offs + i1 + 1;
            buffer[count++] = offs + i1 + 2;
            buffer[count++] = offs + i2 + 1;

            buffer[count++] = offs + i1 + 2;
            buffer[count++] = offs + i2 + 2;
            buffer[count++] = offs + i2 + 1;
        }
    }

    #endregion
}

} // namespace Sketch
