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
        public int meshRenderQueue = 1800;
        
        // the render queue value for the mask, should be less than 2000
        [KSPField] 
        public int maskRenderQueue = 1900;

        // depth mask object transform
        public Transform depthMask;
        
        // depth mask shader object
        public Shader depthShader;

        public override void OnLoad(ConfigNode node)
        {
            
            base.OnLoad(node);
            
            //if (HighLogic.LoadedSceneIsEditor || HighLogic.LoadedSceneIsFlight) return;

            if (!(base.part.FindModelTransform(maskTransform) is Transform depthMask))
            {
                this.LogError($"Can't find transform {maskTransform}");
                return;
            }
            else
            {
                this.Log($"found mask transform {maskTransform}");
            }

            if (!(Shader.Find(shaderName) is Shader depthShader))
            {
                this.LogError($"Can't find shader {shaderName}");
                return;
            }

            var windowRenderer = depthMask.GetComponent<MeshRenderer>();


            windowRenderer.material.shader = depthShader;
            windowRenderer.material.renderQueue = maskRenderQueue;

            this.Log(depthShader.name);
            this.Log(windowRenderer.material.shader.name);
            
            var meshRenderers = part.partTransform.GetComponentsInChildren<MeshRenderer>(true);
            var skinnedMeshRenderers = part.partTransform.GetComponentsInChildren<SkinnedMeshRenderer>(true);

            foreach (var renderer in meshRenderers)
            {
                if (renderer == windowRenderer) continue;
                renderer.material.renderQueue = meshRenderQueue;

            }
        }
    }
}