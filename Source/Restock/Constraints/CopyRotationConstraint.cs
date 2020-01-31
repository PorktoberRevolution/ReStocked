using UnityEngine;

namespace Restock.Constraints
{
    [System.Serializable]
    public class CopyRotationConstraint : IConstraint
    {
        private readonly string rotatorsName;
        private readonly string targetName;
        
        private readonly bool local = false;

        // Cached components
        private readonly Transform target;
        private readonly Transform rotator;
        
        public CopyRotationConstraint(ConfigNode node, Part p)
        {
            node.TryGetValue("rotatorsName", ref rotatorsName);
            node.TryGetValue("targetName", ref targetName);
            
            rotator = p.FindModelTransform(rotatorsName);
            target = p.FindModelTransform(targetName);
        }
        
        public void Update()
        {
            if (rotator == null || target == null) return;

            if (local)
            {
                rotator.localRotation = target.localRotation;
            }
            else
            {
                rotator.rotation = target.rotation;
            }
        }
    }
}