using UnityEngine;
using UnityEngine.Serialization;

namespace Restock
{
    public class ModuleRestockDepthMask: PartModule
    {
        // The name of the transform that has your mask mesh. The only strictly required property
        [KSPField] 
        public string maskTransform= "";

        // The name of the depth mask shader
        [KSPField] 
        public string shaderName = "DepthMask";

        // The render queue value for the mesh, should be less than maskRenderQueue
        [KSPField] 
        public int meshRenderQueue = 1000;
        
        // the render queue value for the mask, should be less than 2000
        [KSPField] 
        public int maskRenderQueue = 1999;

        // depth mask object transform
        public Transform depthMask;
        
        // depth mask shader object
        public Shader depthShader;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            
            GameEvents.onVariantApplied.Add(OnVariantApplied);
        }

        private void OnDestroy()
        {
            GameEvents.onVariantApplied.Remove(OnVariantApplied);
        }

        public override void OnLoad(ConfigNode node)
        {
            
            base.OnLoad(node);
            
            //if (HighLogic.LoadedSceneIsEditor || HighLogic.LoadedSceneIsFlight) return;

            this.depthMask = base.part.FindModelTransform(maskTransform);
            if (!(this.depthMask is Transform))
            {
                this.LogError($"Can't find transform {maskTransform}");
                return;
            }
            
            
            this.depthShader = Shader.Find(shaderName);
            if (!(this.depthShader is Shader))
            {
                this.LogError($"Can't find shader {shaderName}");
                return;
            }

            UpdatematerialQueue();
        }
        
        public void OnVariantApplied(Part appliedPart, PartVariant variant)
        {
            if (appliedPart == this.part) UpdatematerialQueue();
        }
        
        private void UpdatematerialQueue()
        {
            var windowRenderer = depthMask.GetComponent<MeshRenderer>();


            windowRenderer.material.shader = depthShader;
            windowRenderer.material.renderQueue = maskRenderQueue;

            this.Log(depthShader.name);
            this.Log(windowRenderer.material.shader.name);
            
            var meshRenderers = part.partTransform.GetComponentsInChildren<MeshRenderer>(true);
            var skinnedMeshRenderers = part.partTransform.GetComponentsInChildren<SkinnedMeshRenderer>(true);

            foreach (var renderer in meshRenderers)
            {
                this.Log(renderer.material.name + " " + renderer.material.renderQueue.ToString());
                if (renderer == windowRenderer) continue;
                var queue = renderer.material.renderQueue;
                queue = meshRenderQueue + ((queue - 2000) / 2);
                renderer.material.renderQueue = queue;

            }
        }
    }
}