using System;
using UnityEngine;
using Restock.MaterialModifiers;

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

                MaterialModifierParser parser = new MaterialModifierParser();

                foreach (ConfigNode node3 in node2.nodes)
                {
                    IMaterialModifier modifier;
                    try
                    {
                        modifier = parser.Parse(node3);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(new Exception($"[{nameof(ModuleRestockModifyMaterials)}] cannot parse node as material modifier: \n{node3.ToString()}\n", ex));
                        continue;
                    }

                    foreach (Renderer renderer in renderers)
                    {
                        modifier.Modify(renderer.material);
                    }
                }
            }

            isEnabled = false;
            moduleIsEnabled = false;
        }
    }
}
