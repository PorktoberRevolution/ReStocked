using System.Collections.Generic;
using System.Linq;
using Restock.Constraints;

namespace Restock
{
    public class ModuleRestockConstraints : PartModule
    {
        public List<IConstraint> constraints;

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            
            constraints = new List<IConstraint>();
            var cnodes = node.GetNodes();
            this.Log($"Loading {cnodes.Length} constraints");

            foreach (var cnode in cnodes)
            {
                switch (cnode.name)
                {
                    //LookAtConstraint
                    case "CONSTRAINLOOKFX":
                    case "LOOKATCONSTRAINT":
                        constraints.Add(new LookAtConstraint(cnode, this.part));
                        break;

                    //CopyPositionConstraint
                    case "COPYPOSITIONCONSTRAINT":
                        constraints.Add(new CopyPositionConstraint(cnode, this.part));
                        break;

                    //CopyRotationConstraint
                    case "COPYROTATIONCONSTRAINT":
                        constraints.Add(new CopyRotationConstraint(cnode, this.part));
                        break;
                }
            }

            this.Log($"Loaded {constraints.Count} constraints");
        }

        public override void OnStart(StartState state)
        {
            if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor)
            {
                if (constraints == null || constraints.Count == 0)
                {
                    foreach (UrlDir.UrlConfig pNode in GameDatabase.Instance.GetConfigs("PART"))
                    {
                        if (pNode.name.Replace("_", ".") == part.partInfo.name)
                        {
                            var cfg = pNode.config;
                            var node = cfg.GetNodes("MODULE").Single(n => n.GetValue("name") == moduleName);
                            OnLoad(node);
                        }
                    }
                }
            }
        }

        private void LateUpdate()
        {
            if (constraints == null) return;
            
            foreach (var constraint in constraints)
            {
                constraint.Update();
            }
        }
    }
}