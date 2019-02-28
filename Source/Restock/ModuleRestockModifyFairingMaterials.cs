using System;
using System.Collections;
using UnityEngine;
using Restock.MaterialModifiers;

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

            MaterialModifierParser parser = new MaterialModifierParser();

            foreach (ConfigNode node2 in node.nodes)
            {
                IMaterialModifier modifier;
                try
                {
                    modifier = parser.Parse(node2);
                }
                catch (Exception ex)
                {
                    Debug.LogException(new Exception($"[{nameof(ModuleRestockModifyFairingMaterials)}] cannot parse node as material modifier: \n{node2.ToString()}\n", ex));
                    continue;
                }

                modifier.Modify(fairingModule.FairingMaterial);
                modifier.Modify(fairingModule.FairingConeMaterial);
                modifier.Modify(fairingModule.FairingFlightMaterial);
                modifier.Modify(fairingModule.FairingFlightConeMaterial);

                foreach (ProceduralFairings.FairingPanel fairingPanel in fairingModule.Panels)
                {
                    MeshRenderer renderer = fairingPanel.go.GetComponent<MeshRenderer>();
                    modifier.Modify(renderer.material);
                }
            }
        }
    }
}
