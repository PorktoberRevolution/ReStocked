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
        [KSPField] public MeshFilter girderMeshFilter;
        [KSPField] public float girderSegmentHeight;
        public LaunchClampGirderFactory girderFactory;

        private int _girderSegments;

        private Material _girderMaterial;
        private Matrix4x4[] _girderMatrices;
        private bool _girderFlightUpdated = false;

        [KSPField] public bool instancingEnabled = true;

        public override void OnLoad(ConfigNode node)
        {

            if (!HighLogic.LoadedSceneIsGame)
            {
                towerPivot = part.FindModelTransform(trf_towerPivot_name);
                towerYoke = part.FindModelTransform(trf_towerYoke_name);
                towerAnchor = part.FindModelTransform(trf_anchor_name);
                towerGirder = part.FindModelTransform(trf_towerGirder_name);
                towerStretch = part.FindModelTransform(trf_towerStretch_name);

                girderMeshFilter = towerGirder.GetComponent<MeshFilter>();
                girderMesh = girderMeshFilter.mesh;
                
                if (!SystemInfo.supportsInstancing)
                {
                    this.LogWarning("You are using a computer which does not support instancing, " +
                                    "falling back to a slower launch clamp implementation in the editor");
                    instancingEnabled = false;
                }

                if (girderFactory == null)
                {
                    //Debug.Log("Making new girder factory...");
                    girderSegmentHeight = Vector3.Distance(towerAnchor.position, towerStretch.position);
                    if (float.IsInfinity(girderSegmentHeight))
                    {
                        girderSegmentHeight = -1f;
                    }

                    girderFactory = ScriptableObject.CreateInstance<LaunchClampGirderFactory>();
                    girderFactory.Initialize(girderMesh, girderSegmentHeight, maxSegments);
                }
            }
            
            _girderSegments = 1;

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

            if (HighLogic.LoadedSceneIsEditor)
            {
                if (instancingEnabled)
                {
                    UpdateGirderInstanced(girderSegments);
                }
                else
                {
                    UpdateGirderMesh(girderSegments);
                }
            }
            else
            {
                if (_girderFlightUpdated) return;

                UpdateGirderMesh(girderSegments);
                _girderFlightUpdated = true;
            }
        }

        private void UpdateGirderInstanced(int girderSegments)
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

        private void UpdateGirderMesh(int girderSegments)
        {
            if (girderSegments == _girderSegments) return;

            girderMeshFilter.mesh = girderFactory.makeGirder(girderSegments);
        }
    }
}