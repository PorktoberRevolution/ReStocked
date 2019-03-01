using System;
using System.Collections.Generic;
using System.Linq;
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

            foreach (ConfigNode node2 in node.GetNodes("MATERIAL"))
            {
                IEnumerable<Renderer> renderers = GetRenderers(node2);

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

        private IEnumerable<Renderer> GetRenderers(ConfigNode node)
        {
            IEnumerable<Renderer> renderers = Enumerable.Empty<Renderer>();
            bool useAllRenderers = true;

            foreach (ConfigNode.Value value in node.values)
            {
                if (value.name == "transform")
                {
                    Transform[] modelTransforms = part.FindModelTransforms(value.name);

                    if (modelTransforms.Length == 0)
                    {
                        Debug.LogError($"Couldn't find transform named '{value.name}' on part");
                        continue;
                    }

                    List<Renderer> transformRenderers = new List<Renderer>(modelTransforms.Length);
                    foreach (Transform transform in modelTransforms)
                    {
                        Renderer renderer = transform.GetComponent<Renderer>();

                        if (renderer == null)
                            Debug.LogError($"No renderer found on transform '{transform.name}'");
                        else
                            transformRenderers.Add(renderer);
                    }

                    renderers.Concat(transformRenderers);
                    useAllRenderers = false;
                }
                else if (value.name == "baseTransform")
                {
                    Transform[] modelTransforms = part.FindModelTransforms(value.name);

                    if (modelTransforms.Length == 0)
                    {
                        Debug.LogError($"Couldn't find transform named '{value.name}' on part");
                        continue;
                    }

                    foreach (Transform transform in modelTransforms)
                    {
                        Renderer[] transformRenderers = transform.GetComponentsInChildren<Renderer>();

                        if (transformRenderers.Length == 0)
                            Debug.LogError($"No renderers found on transform '{transform.name}' or its children");
                        else
                            renderers.Concat(transform.GetComponentsInChildren<Renderer>());
                    }

                    useAllRenderers = false;
                }
            }

            if (useAllRenderers)
            {
                Transform modelTransform = part.partTransform.Find("model");

                if (modelTransform == null)
                    Debug.LogError("Couldn't find model transform");
                else
                    renderers = modelTransform.GetComponentsInChildren<Renderer>();
            }

            return renderers;
        }
    }
}
