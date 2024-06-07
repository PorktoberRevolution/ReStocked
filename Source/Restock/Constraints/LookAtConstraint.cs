using System;
using UnityEngine;

namespace Restock.Constraints
{
    [System.Serializable]
    /**
     * Points the rotator object at the target object
     */
    public class LookAtConstraint : IConstraint
    {
        private readonly string rotatorsName;
        private readonly string targetName;

        // Cached components
        private readonly Transform rotator;
        private readonly Transform target;

        public LookAtConstraint(ConfigNode node, Part p)
        {
            node.TryGetValue("rotatorsName", ref rotatorsName);
            node.TryGetValue("targetName", ref targetName);
            
            rotator = p.FindModelTransform(rotatorsName);
            if (rotator == null)
            {
                throw new Exception($"Missing rotator transform {rotator}");
            }
            target = p.FindModelTransform(targetName);
            if (target == null)
            {
                throw new Exception($"Missing target transform {target}");
            }
        }
        
        public void Update()
        {
            var lookPos = target.position - rotator.position;
            var rotation = Quaternion.LookRotation(lookPos, rotator.up);
            rotator.rotation = rotation;
        }
    }
}