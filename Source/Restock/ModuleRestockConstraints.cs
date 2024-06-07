using System;
using System.Collections.Generic;
using System.Linq;
using Restock.Constraints;

namespace Restock
{
    public class ModuleRestockConstraints : PartModule
    {
        private List<IConstraint> _constraints;

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            
            _constraints = new List<IConstraint>();
            var cnodes = node.GetNodes();
            this.Log($"Loading {cnodes.Length} constraints");

            foreach (var cnode in cnodes)
            {
                try
                {
                    switch (cnode.name)
                    {
                        //LookAtConstraint
                        case "CONSTRAINLOOKFX":
                        case "LOOKATCONSTRAINT":
                            _constraints.Add(new LookAtConstraint(cnode, this.part));
                            break;

                        //CopyPositionConstraint
                        case "COPYPOSITIONCONSTRAINT":
                            _constraints.Add(new CopyPositionConstraint(cnode, this.part));
                            break;

                        //CopyRotationConstraint
                        case "COPYROTATIONCONSTRAINT":
                            _constraints.Add(new CopyRotationConstraint(cnode, this.part));
                            break;
                    }
                }
                catch(Exception e)
                {
                    this.LogError($"Exception while loading {cnode.name} Node: {e}");
                }
            }

            this.Log($"Loaded {_constraints.Count} constraints");
        }

        public override void OnStart(StartState state)
        {
            if (!HighLogic.LoadedSceneIsFlight && !HighLogic.LoadedSceneIsEditor) return;
            if (_constraints != null && _constraints.Count != 0) return;

            // I have no idea why this is here but I'm scared to remove it
            foreach (var pNode in GameDatabase.Instance.GetConfigs("PART"))
            {
                if (pNode.name.Replace("_", ".") != part.partInfo.name) continue;
                var cfg = pNode.config;
                var node = cfg.GetNodes("MODULE").Single(n => n.GetValue("name") == moduleName);
                OnLoad(node);
            }
        }

        private void LateUpdate()
        {
            for (var i = 0; i < _constraints.Count; i++)
            {
                try
                {
                    _constraints[i].Update();
                }
                catch (Exception e)
                {
                    this.LogError($"Encountered exception in constraint. Removing the constraint to prevent further errors\n {e}");
                    _constraints.RemoveAt(i--);
                }

            }
        }
    }
}