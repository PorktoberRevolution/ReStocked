using System;
using System.Collections.Generic;
using UnityEngine;

namespace Restock
{
    public class ModuleRestockLaunchClamp : LaunchClamp
    {
        [KSPField] public int maxSegments = 100;
        [KSPField] public Transform towerAnchor;
        [KSPField] public Transform towerGirder;

        [KSPField] public Transform towerPivot;
        [KSPField] public Transform towerStretch;
        [KSPField] public Transform towerYoke;
        [KSPField] public string trf_towerGirder_name = "";
        [KSPField] public string trf_towerYoke_name = "";

        [KSPField] public Mesh girderMesh;

        private Material _girderMaterial;
        private Matrix4x4[] _girderMatrices;

        //used by non-instanced fallback girder implementation
        [KSPField] public bool instancingEnabled = true;
        [KSPField] public Mesh girderSegmentMesh;

        private List<Vector3> _girderVerts;
        private List<Vector2> _girderUVs;
        private List<Vector3> _girderNormals;
        private List<Vector4> _girderTangents;
        private List<Color32> _girderColors;
        private List<int> _girderTris;

        private bool _girderFlightUpdated = false;

        private bool _girderHasTangents = false;
        private bool _girderHasColors = false;
        private int _girderVertCount;
        private int _girderTriCount;
        private int _girderSegments;

        public override void OnLoad(ConfigNode node)
        {
            towerPivot = part.FindModelTransform(trf_towerPivot_name);
            towerYoke = part.FindModelTransform(trf_towerYoke_name);
            towerAnchor = part.FindModelTransform(trf_anchor_name);
            towerGirder = part.FindModelTransform(trf_towerGirder_name);
            towerStretch = part.FindModelTransform(trf_towerStretch_name);

            if (!SystemInfo.supportsInstancing)
            {
                this.LogWarning("You are using a computer which does not support instancing, " +
                                "falling back to a slower launch clamp implementation");
                instancingEnabled = false;
            }
            
            girderMesh = towerGirder.GetComponent<MeshFilter>().mesh;
            girderSegmentMesh = Instantiate<Mesh>(girderMesh);

            base.OnLoad(node);
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            girderMesh = towerGirder.GetComponent<MeshFilter>().mesh;

            if (instancingEnabled && HighLogic.LoadedSceneIsEditor)
            {
                var girderRenderer = towerGirder.GetComponent<MeshRenderer>();
                girderRenderer.enabled = false; // we'll render manually from now on

                _girderMatrices = new Matrix4x4[maxSegments];

                _girderMaterial = girderRenderer.material;
                _girderMaterial.enableInstancing = true;
            }
            else
            {
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
        }

        public void LateUpdate()
        {
            var height = HighLogic.LoadedSceneIsEditor ? towerStretch.position.y : this.height;
            var initialHeight = this.initialHeight;

            towerAnchor.position = towerStretch.position - towerStretch.up * height;

            var vec1 = Vector3.down;
            var vec2 = towerAnchor.localPosition - towerYoke.localPosition;
            towerYoke.localRotation = Quaternion.FromToRotation(vec1, vec2);

            var girderSegments = Mathf.CeilToInt(height / initialHeight);
            girderSegments = Math.Min(girderSegments, maxSegments);
            girderSegments = Math.Max(girderSegments, 0);

            if (HighLogic.LoadedSceneIsEditor){
                if (instancingEnabled)
                {
                    UpdateGirder(girderSegments);
                }
                else
                {
                    UpdateGirderFallback(girderSegments);
                }
            }
            else
            {
                if (_girderFlightUpdated) return;
                
                UpdateGirderFallback(girderSegments);
                _girderFlightUpdated = true;
            }
        }

        private void UpdateGirder(int girderSegments)
        {
            var matrix = towerGirder.localToWorldMatrix;
            var offset = Matrix4x4.Translate(towerGirder.TransformVector(Vector3.down * initialHeight));

            for (var i = 0; i < girderSegments; i++)
            {
                _girderMatrices[i] = matrix;
                matrix = offset * matrix;
            }

            Graphics.DrawMeshInstanced(girderMesh, 0, _girderMaterial, _girderMatrices, girderSegments, part.mpb,
                UnityEngine.Rendering.ShadowCastingMode.On, true, towerGirder.gameObject.layer);
        }

        private void UpdateGirderFallback(int newGirderSegments)
        {
            if (newGirderSegments == _girderSegments) return;

            if (newGirderSegments > _girderSegments)
            {
                for (int i = _girderSegments; i < newGirderSegments; i++)
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
            else if (newGirderSegments < _girderSegments)
            {
                var startIndex = newGirderSegments * _girderVertCount;
                var count = (_girderSegments - newGirderSegments) * _girderVertCount;
                _girderVerts.RemoveRange(startIndex, count);
                _girderNormals.RemoveRange(startIndex, count);
                _girderUVs.RemoveRange(startIndex, count);
                if (_girderHasTangents) _girderTangents.RemoveRange(startIndex, count);
                if (_girderHasColors) _girderColors.RemoveRange(startIndex, count);

                _girderTris.RemoveRange(newGirderSegments * _girderTriCount,
                    (_girderSegments - newGirderSegments) * _girderTriCount);
            }

            girderMesh.Clear();

            girderMesh.SetVertices(_girderVerts);
            girderMesh.SetNormals(_girderNormals);
            girderMesh.SetUVs(0, _girderUVs);
            if (_girderHasTangents) girderMesh.SetTangents(_girderTangents);
            if (_girderHasColors) girderMesh.SetColors(_girderColors);
            girderMesh.SetTriangles(_girderTris, 0);

            girderMesh.RecalculateBounds();
            _girderSegments = newGirderSegments;
        }
    }
}