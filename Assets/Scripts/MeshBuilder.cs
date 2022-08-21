using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;

namespace Sketch {

sealed class MeshBuilder : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] int2 _dimensions = math.int2(256, 256);
    [SerializeField] float _frequency = 8;
    [SerializeField] int _octaves = 3;

    #endregion

    #region Private variables

    (NativeArray<int> i, NativeArray<float3> v, NativeArray<float4> c) _buffer;
    Mesh _mesh;

    #endregion

    #region Private utilities

    int IndexCount => (_dimensions.x - 1) * (_dimensions.y - 1) * 6;
    int VertexCount => _dimensions.x * _dimensions.y;

    static NativeArray<T> NewBuffer<T>(int length) where T : unmanaged
      => new NativeArray<T>((int)length, Allocator.Persistent,
                            NativeArrayOptions.UninitializedMemory);

    static float Fbm(float2 p, int octaves)
    {
        var w = 1.0f;
        var acc = 0.0f;
        for (var i = 0; i < octaves; i++)
        {
            acc += noise.cnoise(p) * w;
            p *= 2;
            w *= 0.5f;
        }
        return acc;
    }

    #endregion

    #region MonoBehaviour

    void Start()
    {
        _buffer.i = NewBuffer<int>(IndexCount);
        _buffer.v = NewBuffer<float3>(VertexCount);
        _buffer.c = NewBuffer<float4>(VertexCount);

        BuildIndexBuffer();
        BuildVertexBuffer();
        BuildMesh();

        GetComponent<MeshFilter>().sharedMesh = _mesh;
    }

    void OnDestroy()
    {
        if (_buffer.i.IsCreated) _buffer.i.Dispose();
        if (_buffer.v.IsCreated) _buffer.v.Dispose();
        if (_buffer.c.IsCreated) _buffer.c.Dispose();
        Util.DestroyObject(_mesh);
    }

    #endregion

    #region Mesh building methods

    void BuildIndexBuffer()
    {
        ref var buf = ref _buffer.i;
        var (i, v, row) = (0, 0, _dimensions.x);

        for (var y = 0; y < _dimensions.y - 1; y++)
        {
            for (var x = 0; x < _dimensions.x - 1; x++)
            {
                buf[i++] = v;
                buf[i++] = v + row;
                buf[i++] = v + 1;

                buf[i++] = v + 1;
                buf[i++] = v + row;
                buf[i++] = v + row + 1;

                v++;
            }

            v++;
        }
    }

    void BuildVertexBuffer()
    {
        var i = 0;
        for (var yi = 0; yi < _dimensions.y; yi++)
        {
            var y = 2.0f * yi / (_dimensions.y - 1) - 1;
            for (var xi = 0; xi < _dimensions.x; xi++)
            {
                var x = 2.0f * xi / (_dimensions.x - 1) - 1;

                var np = math.float2(x, y) * _frequency;
                var h = Fbm(np, _octaves);
                var c = h;//Fbm(np, 1);
                var c2 = math.pow(math.abs(h), 10) * 100;
                var c3 = (Vector4)Color.HSVToRGB(math.abs(c), 1, 1);

                _buffer.v[i] = math.float3(x, h, y);
                _buffer.c[i] = math.float4(c3.x, c3.y, c3.z, c2);
                i++;
            }
        }
    }

    void BuildMesh()
    {
        _mesh = new Mesh();
        _mesh.SetVertices(_buffer.v);
        _mesh.SetUVs(0, _buffer.c);
        _mesh.SetIndices(_buffer.i, MeshTopology.Triangles, 0);
        _mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 1000);
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();
    }

    #endregion
}

} // namespace Sketch
