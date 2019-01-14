using System;
using System.Collections;
using UnityEngine;

namespace Restock
{
    public class ModuleRestockModifyFairingMaterials : PartModule
    {
        [SerializeField]
        private string serializedNode;

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);

            if (serializedNode == null)
                serializedNode = node.ToString();
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            StartCoroutine(WaitAndUpdateMaterials());
        }

        private IEnumerator WaitAndUpdateMaterials()
        {
            yield return null;

            if (string.IsNullOrEmpty(serializedNode))
            {
                Debug.LogError("Serialized node is null or empty!");
                yield break;
            }

            ConfigNode node = ConfigNode.Parse(serializedNode).nodes[0];

            ModuleProceduralFairing fairingModule = part.FindModuleImplementing<ModuleProceduralFairing>();

            if (fairingModule == null)
            {
                Debug.LogError("No fairing module found on part!");
                yield break;
            }

            UpdateMaterial(fairingModule.FairingMaterial, node);
            UpdateMaterial(fairingModule.FairingConeMaterial, node);
            UpdateMaterial(fairingModule.FairingFlightMaterial, node);
            UpdateMaterial(fairingModule.FairingFlightConeMaterial, node);

            foreach (ProceduralFairings.FairingPanel fairingPanel in fairingModule.Panels)
            {
                MeshRenderer renderer = fairingPanel.go.GetComponent<MeshRenderer>();
                UpdateMaterial(renderer.material, node);
            }
        }

        private void UpdateMaterial(Material material, ConfigNode node)
        {
            foreach (ConfigNode node2 in node.nodes)
            {
                if (node2.name == "COLOR_PROPERTY")
                {
                    string name = node2.GetValue("name");
                    Color color = ConfigNode.ParseColor(node2.GetValue("color"));

                    material.SetColor(name, color);
                }
                else if (node2.name == "FLOAT_PROPERTY")
                {
                    string name = node2.GetValue("name");
                    float value = float.Parse(node2.GetValue("value"));

                    material.SetFloat(name, value);
                }
            }
        }
    }
}
