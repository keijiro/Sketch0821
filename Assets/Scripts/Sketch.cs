using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using Random = Unity.Mathematics.Random;

namespace Sketch {

sealed class Sketch : MonoBehaviour
{
    [SerializeField] uint _iteration = 10;
    [SerializeField] float _height = 0.1f;
    [SerializeField] float _radius = 0.1f;
    [SerializeField] float _shrink = 0.05f;
    [SerializeField] uint _seed = 123;

    Mesh _mesh;

    void Start()
    {
        var rand = new Random(_seed);
        rand.NextUInt4();

        var stack = new Stack<Modeler>();

        for (var i = 0u; i < _iteration; i++)
        {
            var rx = rand.NextFloat() * _shrink;
            var rz = rand.NextFloat() * _shrink;

            var m = new Modeler()
              { Height = _height, Resolution = 256, Seed = rand.NextUInt() };
            m.Origin = math.float3(rx, i * _height, rz);
            m.Radius = _radius - _shrink * i;

            stack.Push(m);
        }

        GetComponent<MeshFilter>().sharedMesh = _mesh = MeshBuilder.Build(stack);
    }

    void OnDestroy()
      => Util.DestroyObject(_mesh);
}

} // namespace Sketch
