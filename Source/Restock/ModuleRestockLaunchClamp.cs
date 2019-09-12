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
        
        [KSPField] public Mesh girderSegmentMesh;
        [KSPField] public Mesh girderMesh;
        
        private List<Vector3> _girderVerts;
        private List<Vector2> _girderUVs;
        private List<Vector3> _girderNormals;
        private List<Vector4> _girderTangents;
        private List<Color32> _girderColors;
        private List<int> _girderTris;
        private int _girderSegments;
        
        private bool _girderHasTangents = false;
        private bool _girderHasColors = false;
        private int _girderVertCount;
        private int _girderTriCount;

        private bool _flightUpdate = false;

        public override void OnLoad(ConfigNode node)
        {
            Debug.Log("OnLoad Called");
            
            towerPivot = base.part.FindModelTransform(trf_towerPivot_name);
            towerYoke = base.part.FindModelTransform(trf_towerYoke_name);
            towerAnchor = base.part.FindModelTransform(trf_anchor_name);
            towerGirder = base.part.FindModelTransform(trf_towerGirder_name);
            towerStretch = base.part.FindModelTransform(trf_towerStretch_name);
            
            girderMesh = towerGirder.GetComponent<MeshFilter>().mesh;
            girderSegmentMesh = Instantiate<Mesh>(girderMesh);
            
            base.OnLoad(node);
        }

        public override void OnStart(StartState state)
        {

            Debug.Log("OnStart Called");
            Debug.Log(girderSegmentMesh == null);
            girderMesh = towerGirder.GetComponent<MeshFilter>().mesh;
            
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
            var height = base.height;
            var initialHeight = base.initialHeight;
            
            towerAnchor.position = towerStretch.position - (towerStretch.up * height);
            
            var vec1 = Vector3.down;
            var vec2 = towerAnchor.localPosition - towerYoke.localPosition ;
            towerYoke.localRotation = Quaternion.FromToRotation(vec1, vec2);

            var segments = Mathf.CeilToInt(height / initialHeight);
            if (segments != _girderSegments)
            {
                UpdateGirder(segments);
            }
        }

        private void UpdateGirder(int length)
        {
            if (length > _girderSegments)
            {
                for (int i = _girderSegments; i < length; i++)
                {
                    var offset = Vector3.down * base.initialHeight * i;
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
            else
            {
                var startIndex = length * _girderVertCount;
                var count = (_girderSegments - length) * _girderVertCount;
                _girderVerts.RemoveRange(startIndex, count);
                _girderNormals.RemoveRange(startIndex, count);
                _girderUVs.RemoveRange(startIndex, count);
                if (_girderHasTangents) _girderTangents.RemoveRange(startIndex, count);
                if (_girderHasColors) _girderColors.RemoveRange(startIndex, count);
                
                _girderTris.RemoveRange(length * _girderTriCount, (_girderSegments - length) * _girderTriCount);
            }
            
            girderMesh.Clear();
            
            girderMesh.SetVertices(_girderVerts);
            girderMesh.SetNormals(_girderNormals);
            girderMesh.SetUVs(0, _girderUVs);
            if (_girderHasTangents) girderMesh.SetTangents(_girderTangents);
            if (_girderHasColors) girderMesh.SetColors(_girderColors);
            girderMesh.SetTriangles(_girderTris, 0);
            
            girderMesh.RecalculateBounds();
            _girderSegments = length;
        }
    }
}