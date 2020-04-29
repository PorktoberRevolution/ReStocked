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
            target = p.FindModelTransform(targetName);
        }
        
        public void Update()
        {
            if (rotator == null || target == null) return;
            
            var lookPos = target.position - rotator.position;
            var rotation = Quaternion.LookRotation(lookPos, target.up);
            rotator.rotation = rotation;
        }
    }
}