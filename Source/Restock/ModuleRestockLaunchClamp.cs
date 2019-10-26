using System;
using UnityEngine;

namespace Restock
{
    public class ModuleRestockLaunchClamp : LaunchClamp
    {
        private Material _girderMaterial;
        private Matrix4x4[] _girderMatrices;
        private Mesh _girderMesh;

        private int _girderSegments;
        [KSPField] public int maxSegments = 100;
        [KSPField] public Transform towerAnchor;
        [KSPField] public Transform towerGirder;

        [KSPField] public Transform towerPivot;
        [KSPField] public Transform towerStretch;
        [KSPField] public Transform towerYoke;
        [KSPField] public string trf_towerGirder_name = "";
        [KSPField] public string trf_towerYoke_name = "";

        public override void OnLoad(ConfigNode node)
        {
            towerPivot = part.FindModelTransform(trf_towerPivot_name);
            towerYoke = part.FindModelTransform(trf_towerYoke_name);
            towerAnchor = part.FindModelTransform(trf_anchor_name);
            towerGirder = part.FindModelTransform(trf_towerGirder_name);
            towerStretch = part.FindModelTransform(trf_towerStretch_name);

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
            }
        }

        public void LateUpdate()
        {
            if (_girderSegments < 0) return;

            var height = HighLogic.LoadedSceneIsEditor ? towerStretch.position.y : this.height;
            var initialHeight = this.initialHeight;

            towerAnchor.position = towerStretch.position - towerStretch.up * height;

            var vec1 = Vector3.down;
            var vec2 = towerAnchor.localPosition - towerYoke.localPosition;
            towerYoke.localRotation = Quaternion.FromToRotation(vec1, vec2);

            _girderSegments = Mathf.CeilToInt(height / this.initialHeight);
            _girderSegments = Math.Min(_girderSegments, maxSegments);
            _girderSegments = Math.Max(_girderSegments, 0);

            var matrix = towerGirder.localToWorldMatrix;
            var offset = Matrix4x4.Translate(towerGirder.TransformVector(Vector3.down * initialHeight));

            for (var i = 0; i < _girderSegments; i++)
            {
                _girderMatrices[i] = matrix;
                matrix = offset * matrix;
            }

            Graphics.DrawMeshInstanced(_girderMesh, 0, _girderMaterial, _girderMatrices, _girderSegments, part.mpb);
        }
    }
}