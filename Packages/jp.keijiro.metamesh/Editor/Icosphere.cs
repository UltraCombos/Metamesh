using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Metamesh {

    [System.Serializable]
    public sealed class Icosphere
    {
        public float Radius = 1;
        public uint Subdivision = 2;
        public bool hasUV = false;

        public void Generate(Mesh mesh)
        {
            

            if (hasUV)
            {
                var builder = new VertexOnlyIcosphereBuilder();
                for (var i = 1; i < Subdivision; i++)
                    builder = new VertexOnlyIcosphereBuilder(builder);

                var vtx = builder.Vertices.Select(v => (Vector3)(v * Radius));
                var nrm = builder.Vertices.Select(v => (Vector3)v);
                var idx = builder.Indices;
                var nrmList = nrm.ToList();
                List<Vector2> uvs = new List<Vector2>();

                for (int i = 0; i < nrmList.Count / 3; ++i)
                {
                    var n0 = nrmList[3 * i + 0];
                    var n1 = nrmList[3 * i + 1];
                    var n2 = nrmList[3 * i + 2];

                    var uv0 = NormalToUV(n0);
                    var uv1 = NormalToUV(n1);
                    var uv2 = NormalToUV(n2);

                    FixUV(ref uv0, ref uv1);
                    FixUV(ref uv0, ref uv2);

                    uvs.Add(uv0);
                    uvs.Add(uv1);
                    uvs.Add(uv2);

                    /*
                    var n = Vector3.Normalize(n0 + n1 + n2 / 3);
                    nrmList[3 * i + 0] = n;
                    nrmList[3 * i + 1] = n;
                    nrmList[3 * i + 2] = n;
                    */
                }


                if (builder.VertexCount > 65535) mesh.indexFormat = IndexFormat.UInt32;
                mesh.SetVertices(vtx.ToList());
                mesh.SetNormals(nrmList);
                mesh.SetIndices(idx.ToList(), MeshTopology.Triangles, 0);
                mesh.SetUVs(0, uvs);
            }
            else
            { 
                var builder = new IcosphereBuilder();
                for (var i = 1; i < Subdivision; i++)
                    builder = new IcosphereBuilder(builder);

                var vtx = builder.Vertices.Select(v => (Vector3)(v * Radius));
                var nrm = builder.Vertices.Select(v => (Vector3)v);
                var idx = builder.Indices;

                if (builder.VertexCount > 65535) mesh.indexFormat = IndexFormat.UInt32;
                mesh.SetVertices(vtx.ToList());
                mesh.SetNormals(nrm.ToList());
                mesh.SetIndices(idx.ToList(), MeshTopology.Triangles, 0);
            }
        }
                void FixUV(ref Vector2 uv0, ref Vector2 uv1)
        {
            while (uv1.x - uv0.x > 0.5f)
                uv1.x -= 1;
            while (uv1.x - uv0.x < -0.5f)
                uv1.x += 1;


            while (uv1.y - uv0.y > 0.5f)
                uv1.y -= 1;  
            while (uv1.y - uv0.y < -0.5f)
                uv1.y += 1;

        }
        Vector2 NormalToUV(Vector3 normal)
        {
            float theta = Mathf.Acos(-normal.y);
            // Calculate v coordinate, ranging from 0 to 1
            float v = theta / Mathf.PI;
            // Calculate longitude phi, ranging from -£k to £k
            float phi = Mathf.Atan2(-normal.x, normal.z);
            // Calculate u coordinate, ranging from 0 to 1
            float u = (phi + Mathf.PI) / (2.0f * Mathf.PI);
            return new Vector2(Mathf.Clamp01(u), Mathf.Clamp01(v));
        }
    }

} // namespace Metamesh
