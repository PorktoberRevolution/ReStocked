using System;
using UnityEngine;

namespace Restock
{
    public class ModuleRestockModifyMaterials : PartModule
    {
        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);

            if (HighLogic.LoadedSceneIsEditor || HighLogic.LoadedSceneIsFlight) return;

            Transform modelTransform = part.partTransform.Find("model");

            Renderer[] renderers = modelTransform.GetComponentsInChildren<Renderer>();

            if (modelTransform == null)
            {
                Debug.LogError("Couldn't find model transform");
                return;
            }

            foreach (ConfigNode node2 in node.GetNodes("MATERIAL"))
            {
                if (node2.GetValue("shader") is string newShaderName)
                {
                    if (Shader.Find(newShaderName) is Shader newShader)
                    {
                        foreach (Renderer renderer in renderers)
                        {
                            renderer.material.shader = newShader;
                        }
                    }
                    else
                    {
                        Debug.LogError($"Can't find shader {newShaderName}");
                        continue;
                    }

                }

                foreach (ConfigNode node3 in node2.GetNodes("TEXTURE_PROPERTY"))
                {
                    string name = node3.GetValue("name");
                    string textureUrl = node3.GetValue("textureUrl");
                    bool normalMapToggle = false;

                    if (node3.GetValue("isNormalMap") is string normalMapToggleString)
                    {
                        normalMapToggle = bool.Parse(normalMapToggleString);
                    }
                    
                    GameDatabase.TextureInfo textureInfo = GameDatabase.Instance.GetTextureInfo(textureUrl);

                    if (textureInfo == null)
                    {
                        Debug.LogError($"Cannot find texture: {textureUrl}");
                        continue;
                    }

                    foreach (Renderer renderer in renderers)
                    {
                        renderer.material.SetTexture(name, normalMapToggle ? textureInfo.normalMap : textureInfo.texture);
                    }
                }
            }

            isEnabled = false;
            moduleIsEnabled = false;
        }
    }
}
