using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Restock
{

  public class ModuleRestockLookAtConstraint : PartModule
	{

    [System.Serializable]
    public class LookConstraint
    {
      string rotatorsName;
      string targetName;

      // Cached components
      Transform target;
      Transform rotator;
      Part part;

      public LookConstraint(ConfigNode node, Part p)
      {
        node.TryGetValue("rotatorsName", ref rotatorsName);
        node.TryGetValue("targetName", ref targetName);
        part = p;
        rotator = p.FindModelTransform(rotatorsName);
        target = p.FindModelTransform(targetName);
      }

      public void UpdateRotators()
      {
        if (rotator != null && target != null)
        {
          Vector3 targetPostition = new Vector3(target.position.x,
                                             target.position.y,
                                             target.position.z);

          Vector3 lookPos = target.position - rotator.position;
          var rotation = Quaternion.LookRotation(lookPos, target.up);
          rotator.rotation = rotation;
        }
      }
    }

    public List<LookConstraint> constraints;

    public override void OnLoad(ConfigNode node)
    {
      base.OnLoad(node);
      constraints = new List<LookConstraint>();
      ConfigNode[] cnodes = node.GetNodes("CONSTRAINLOOKFX");
      Debug.Log(String.Format("[ModuleAdvancedLookAtConstraint]: Loading {0} constraints", cnodes.Length));

      for (int i = 0; i < cnodes.Length; i++)
      {
        constraints.Add(new LookConstraint(cnodes[i], this.part));
      }

      Debug.Log(String.Format("[ModuleAdvancedLookAtConstraint]: Loaded {0} constraints", constraints.Count));
    }
    public override void OnStart(StartState state)
    {
      if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor)
      {
        if (constraints == null || constraints.Count == 0)
        {
          ConfigNode cfg;
          foreach (UrlDir.UrlConfig pNode in GameDatabase.Instance.GetConfigs("PART"))
          {
            if (pNode.name.Replace("_", ".") == part.partInfo.name)
            {
              cfg = pNode.config;
              ConfigNode node = cfg.GetNodes("MODULE").Single(n => n.GetValue("name") == moduleName);
              OnLoad(node);
            }
          }
        }
      }
    }
    
    void LateUpdate()
    {
      if (constraints != null)
      {
        for (int i = 0; i < constraints.Count; i++)
        {
          constraints[i].UpdateRotators();
        }
      }
    }
  }
}
