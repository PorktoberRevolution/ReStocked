using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Restock
{
    public class ModuleRestockLaunchClamp : LaunchClamp
    {
        [KSPField] public string trf_towerGirder_name = "";
        [KSPField] public string trf_towerYoke_name = "";
        
        [KSPField] public Transform towerPivot;
        [KSPField] public Transform towerYoke;
        [KSPField] public Transform towerAnchor;
        [KSPField] public Transform towerGirder;
        [KSPField] public Transform towerStretch;
        
        [KSPField] private Mesh _girderSegmentMesh;
        [KSPField] private Mesh _girderMesh;
        
        private List<Vector3> _girderVerts;
        private List<Vector2> _girderUVs;
        private List<Vector3> _girderNormals;
        private List<Vector4> _girderTangents;
        private List<Color32> _girderColors;
        private List<int> _girderTris;
        private int _girderSegments;
        
        private float _girderHeight = -1f;

        private bool _girderHasTangents = false;
        private bool _girderHasColors = false;
        private int _girderVertCount;
        private int _girderTriCount;

        private bool _flightUpdate = false;

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            
            if (HighLogic.LoadedSceneIsEditor || HighLogic.LoadedSceneIsFlight) return;
            

        }

        public override void OnStart(StartState state)
        {

            Debug.Log("OnStart Called");
            towerPivot = base.part.FindModelTransform(trf_towerPivot_name);
            towerYoke = base.part.FindModelTransform(trf_towerYoke_name);
            towerAnchor = base.part.FindModelTransform(trf_anchor_name);
            towerGirder = base.part.FindModelTransform(trf_towerGirder_name);
            towerStretch = base.part.FindModelTransform(trf_towerStretch_name);

            _girderMesh = towerGirder.GetComponent<MeshFilter>().mesh;
            _girderSegmentMesh = Instantiate<Mesh>(_girderMesh);

            _girderHeight = Vector3.Distance(towerStretch.position, towerAnchor.position);
            
            _girderVertCount = _girderSegmentMesh.vertexCount;
            _girderTriCount = _girderSegmentMesh.triangles.Length;
            
            _girderVerts = new List<Vector3>(_girderSegmentMesh.vertices);
            _girderUVs = new List<Vector2>(_girderSegmentMesh.uv);
            _girderNormals = new List<Vector3>(_girderSegmentMesh.normals);
            if (_girderSegmentMesh.tangents.Length > 0)
            {
                _girderHasTangents = true;
                _girderTangents = new List<Vector4>(_girderSegmentMesh.tangents);
            }
            if (_girderSegmentMesh.colors32.Length > 0)
            {
                _girderHasColors = true;
                _girderColors = new List<Color32>(_girderSegmentMesh.colors32);
            }
            _girderTris = new List<int>(_girderSegmentMesh.triangles);
            _girderSegments = 1;
            
            base.OnStart(state);
            
            UpdateClamp();
        }
        
        public void LateUpdate()
        {
            if (!HighLogic.LoadedSceneIsEditor)
            {
                if (_flightUpdate) return;
                else _flightUpdate = true;
            }
            
            UpdateClamp();
        }
        
        public void UpdateClamp()
        {
            var vec1 = Vector3.down;
            var vec2 = towerAnchor.localPosition - towerYoke.localPosition ;
            towerYoke.localRotation = Quaternion.FromToRotation(vec1, vec2);

            var height = Vector3.Distance(towerStretch.position, towerAnchor.position);
            var segments = Mathf.CeilToInt(height / _girderHeight);
            if (segments != _girderSegments)
            {
                UpdateGirder(segments);
            }
            
            towerAnchor.position = towerStretch.position - (towerStretch.up * height);
        }

        private void UpdateGirder(int length)
        {
            if (length > _girderSegments)
            {
                for (int i = _girderSegments; i < length; i++)
                {
                    var offset = Vector3.down * _girderHeight * i;
                    var indexOffset = _girderVertCount * i;
                    for (int v = 0; v < _girderVertCount; v++)
                    {
                        _girderVerts.Add(_girderSegmentMesh.vertices[v] + offset);
                    }
                    _girderNormals.AddRange(_girderSegmentMesh.normals);
                    _girderUVs.AddRange(_girderSegmentMesh.uv);
                    if (_girderHasTangents) _girderTangents.AddRange(_girderSegmentMesh.tangents);
                    if (_girderHasColors) _girderColors.AddRange(_girderSegmentMesh.colors32);

                    for (int t = 0; t < _girderTriCount; t++)
                    {
                        _girderTris.Add(_girderSegmentMesh.triangles[t] + indexOffset);
                    }
                }
            }
            else
            {
                var startIndex = length * _girderVertCount;
                var count = (_girderSegments - length) * _girderVertCount;
                Debug.Log("removing verts");
                _girderVerts.RemoveRange(startIndex, count);
                _girderNormals.RemoveRange(startIndex, count);
                _girderUVs.RemoveRange(startIndex, count);
                if (_girderHasTangents) _girderTangents.RemoveRange(startIndex, count);
                if (_girderHasColors) _girderColors.RemoveRange(startIndex, count);
                
                Debug.Log("removing tris");
                _girderTris.RemoveRange(length * _girderTriCount, (_girderSegments - length) * _girderTriCount);
            }
            
            _girderMesh.Clear();
            
            _girderMesh.SetVertices(_girderVerts);
            _girderMesh.SetNormals(_girderNormals);
            _girderMesh.SetUVs(0, _girderUVs);
            if (_girderHasTangents) _girderMesh.SetTangents(_girderTangents);
            if (_girderHasColors) _girderMesh.SetColors(_girderColors);
            _girderMesh.SetTriangles(_girderTris, 0);
            
            _girderMesh.RecalculateBounds();
            _girderSegments = length;
        }
    }
}