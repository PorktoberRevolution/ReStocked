using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Restock
{
    public class LaunchClampGirderFactory : ScriptableObject
    {
        public Mesh girderSegmentMesh;
        
        [SerializeField] private Mesh[] _girderMeshCache;
        [SerializeField] private int _maxLength;

        private float _girderSegmentHeight;
        private bool _girderHasTangents;
        private bool _girderHasColors;
        private int _girderVertCount;
        private int _girderTriCount;
        
        private int _girderSegments;
        private List<Vector3> _girderVerts;
        private List<Vector2> _girderUVs;
        private List<Vector3> _girderNormals;
        private List<Vector4> _girderTangents;
        private List<Color32> _girderColors;
        private List<int> _girderTris;

        public void Initialize(Mesh girderSegmentMesh, float girderSegmentHeight, int maxLength = 100)
        {
            this.girderSegmentMesh = girderSegmentMesh;

            _girderSegmentHeight = girderSegmentHeight;
            _maxLength = maxLength;
            _girderMeshCache = new Mesh[maxLength];
            
            _girderMeshCache[0] = new Mesh();
            _girderMeshCache[1] = girderSegmentMesh;
            
            _girderVertCount = girderSegmentMesh.vertexCount;
            _girderTriCount = girderSegmentMesh.triangles.Length;

            _girderVerts = new List<Vector3>(girderSegmentMesh.vertices);
            _girderUVs = new List<Vector2>(girderSegmentMesh.uv);
            _girderNormals = new List<Vector3>(girderSegmentMesh.normals);
            if (girderSegmentMesh.tangents.Length > 0)
            {
                _girderHasTangents = true;
                _girderTangents = new List<Vector4>(girderSegmentMesh.tangents);
            }

            if (girderSegmentMesh.colors32.Length > 0)
            {
                _girderHasColors = true;
                _girderColors = new List<Color32>(girderSegmentMesh.colors32);
            }

            _girderTris = new List<int>(girderSegmentMesh.triangles);
            _girderSegments = 1;
        }
        public Mesh makeGirder(int length)
        {
            if (length < 0) length = 0;
            if (length > _maxLength) length = _maxLength;
            
            if (_girderMeshCache[length] == null)
            {
                Debug.Log("Girder mesh not generated, making it now...");
                var girderMesh = makeGirderMesh(length);
                _girderMeshCache[length] = girderMesh;
            }

            return _girderMeshCache[length];
        }

        private Mesh makeGirderMesh(int length)
        {
            if (length < 1)
            {
                return new Mesh();
            }
            var girderMesh = new Mesh();    
            if (length > _girderSegments)
            {
                for (int i = _girderSegments; i < length ; i++)
                {
                    var offset = Vector3.down * _girderSegmentHeight * i;
                    var indexOffset = _girderVertCount * i;
                    for (int v = 0; v < _girderVertCount; v++)
                    {
                        _girderVerts.Add(girderSegmentMesh.vertices[v] + offset);
                    }

                    _girderNormals.AddRange(girderSegmentMesh.normals);
                    _girderUVs.AddRange(girderSegmentMesh.uv);

                    if (_girderHasTangents) _girderTangents.AddRange(girderSegmentMesh.tangents);
                    if (_girderHasColors) _girderColors.AddRange(girderSegmentMesh.colors32);

                    for (int t = 0; t < _girderTriCount; t++)
                    {
                        _girderTris.Add(girderSegmentMesh.triangles[t] + indexOffset);
                    }
                }
            }
            else if (length < _girderSegments)
            {
                var startIndex = length * _girderVertCount;
                var count = (_girderSegments - length) * _girderVertCount;
                _girderVerts.RemoveRange(startIndex, count);
                _girderNormals.RemoveRange(startIndex, count);
                _girderUVs.RemoveRange(startIndex, count);
                if (_girderHasTangents) _girderTangents.RemoveRange(startIndex, count);
                if (_girderHasColors) _girderColors.RemoveRange(startIndex, count);

                _girderTris.RemoveRange(length * _girderTriCount,
                    (_girderSegments - length) * _girderTriCount);
            }

            girderMesh.SetVertices(_girderVerts);
            girderMesh.SetNormals(_girderNormals);
            girderMesh.SetUVs(0, _girderUVs);
            if (_girderHasTangents) girderMesh.SetTangents(_girderTangents);
            if (_girderHasColors) girderMesh.SetColors(_girderColors);
            girderMesh.SetTriangles(_girderTris, 0);

            girderMesh.RecalculateBounds();
            _girderSegments = length;

            return girderMesh;
        }
    }
}