using UnityEngine;
using CompoundParts;

namespace Restock
{
    /* Extends the stock CModuleLinkedMesh used on struts and pipes to make pipe textures repeat instead of stretch when
       a pipe is extended beyond its default length */
    
    public class ModuleRestockLinkedMesh : CModuleLinkedMesh
    {
        // the direction along the texture that the pipe points. set to "x" or "y"
        [KSPField] 
        public string stretchAxis = "x";
        
        // space-seperated list of textures to be effected by the length, preferably all of the ones on the material
        // Unity has no good way to get all the texture names attached to a material so it has to be set manually, unfortunately
        [KSPField] 
        public string stretchTextures = "_MainTex";
        
        
        // reference to the material we will be modifying
        private Material[] pipeMaterials;
        
        // array of property IDs corresponding to the textures
        private int[] pipeMaterialIDs;
        
        // index of the texture scale vector, 0 for x, 1 for y
        private int pipeStretchIndex;
        
        // initial scale of the pipe object, may not be 1
        private float baseStretch;

        // scale vector for the material
        private Vector2 texScale = Vector2.one;
        
        // offset vector for the material
        private Vector2 texOffset = Vector2.zero;
        

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            
            // get all materials on the line object, including disabled ones
            var renderers = line.GetComponentsInChildren<MeshRenderer>(true);
            pipeMaterials = new Material[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                pipeMaterials[i] = renderers[i].material;
            }
            
            // split texture list and convert to property IDs for easy access
            var texNames = stretchTextures.Split(' ');
            pipeMaterialIDs = new int[texNames.Length];
            for (int i = 0; i < texNames.Length; i++)
            {
                pipeMaterialIDs[i] = Shader.PropertyToID(texNames[i]);
            }
            
            // default to 'x' if an invalid value is used
            pipeStretchIndex = stretchAxis != "y" ? 0 : 1;
            baseStretch = part.scaleFactor;
            
            GameEvents.onEditorVariantApplied.Add(OnVariantApplied);
        }

        private void OnDestroy()
        {
            GameEvents.onEditorVariantApplied.Remove(OnVariantApplied);
        }

        public override void OnTargetSet(Part newTarget)
        {
            base.OnTargetSet(newTarget);
            UpdateStretch();
        }

        public override void OnTargetUpdate()
        {
            base.OnTargetUpdate();
            UpdateStretch();
        }

        public override void OnPreviewAttachment(Vector3 rDir, Vector3 rPos, Quaternion rRot)
        {
            base.OnPreviewAttachment(rDir, rPos, rRot);
            UpdateStretch();
        }

        public void OnVariantApplied(Part appliedPart, PartVariant variant)
        {
            if (appliedPart == part) UpdateStretch();
        }
        
        // updates the texture stretch to match the pipe object's local scale
        private void UpdateStretch()
        {
            var stretch = line.localScale.z / baseStretch;
            
            texScale[pipeStretchIndex] = stretch;
            texOffset[pipeStretchIndex] = (1 - stretch) / 2;

            foreach (var material in pipeMaterials)
            {
                foreach (var id in pipeMaterialIDs)
                {
                    material.SetTextureScale(id, texScale);
                    material.SetTextureOffset(id, texOffset);
                }
            }
        }
    }
}