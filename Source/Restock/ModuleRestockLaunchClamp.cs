using System;
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
        [KSPField] public int maxSegments = 100;
        
        private bool _flightUpdate = false;
        private Material _girderMaterial;
        private Mesh _girderMesh;

        private int _girderSegments;
        private Matrix4x4[] _girderMatrices;

        public override void OnLoad(ConfigNode node)
        {
            towerPivot = base.part.FindModelTransform(trf_towerPivot_name);
            towerYoke = base.part.FindModelTransform(trf_towerYoke_name);
            towerAnchor = base.part.FindModelTransform(trf_anchor_name);
            towerGirder = base.part.FindModelTransform(trf_towerGirder_name);
            towerStretch = base.part.FindModelTransform(trf_towerStretch_name);

            base.OnLoad(node);
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            var girderRenderer = towerGirder.GetComponent<MeshRenderer>();
            _girderMaterial = girderRenderer.material;
            _girderMesh = towerGirder.GetComponent<MeshFilter>().mesh;

            girderRenderer.enabled = false; // we'll render manually from now on
            
            _girderSegments = 0;
            _girderMatrices = new Matrix4x4[maxSegments];

            _girderMaterial.enableInstancing = true;
            if (!_girderMaterial.enableInstancing)
            {
                this.LogError("Could not enable instancing! Aborting");
                _girderSegments = -1;
                return;
            }
        }

        public void LateUpdate()
        {
            
            if (_girderSegments < 0) return;

            if (!HighLogic.LoadedSceneIsEditor)
            {
                if (_flightUpdate) return;
                else _flightUpdate = true;
            }

            var height1 = HighLogic.LoadedSceneIsEditor ? towerStretch.position.y : base.height;
            var initialHeight1 = base.initialHeight;

            towerAnchor.position = towerStretch.position - (towerStretch.up * height1);

            var vec1 = Vector3.down;
            var vec2 = towerAnchor.localPosition - towerYoke.localPosition;
            towerYoke.localRotation = Quaternion.FromToRotation(vec1, vec2);
            
            _girderSegments = Mathf.CeilToInt(height1 / base.initialHeight);
            _girderSegments = Math.Min(_girderSegments, maxSegments);
            _girderSegments = Math.Max(_girderSegments, 0);
            
            var matrix = towerGirder.localToWorldMatrix;
            var offset = Matrix4x4.Translate(matrix.MultiplyVector(Vector3.down * base.initialHeight));
            
            for (int i = 0; i < _girderSegments; i++)
            {
                _girderMatrices[i] = matrix;
                matrix *= offset;
            }

            Graphics.DrawMeshInstanced(_girderMesh, 0, _girderMaterial, _girderMatrices, _girderSegments, part.mpb);
        }
    }
}