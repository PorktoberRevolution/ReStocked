using UnityEngine;

namespace Restock.Constraints
{
    [System.Serializable]
    /**
     * Copies the rotation of the target object to the rotator object.
     * If `IsLocal` is true, uses local rotation instead of global rotation. 
     */
    public class CopyRotationConstraint : IConstraint
    {
        private readonly string rotatorsName;
        private readonly string targetName;
        
        private readonly bool local = false;

        // Cached components
        private readonly Transform rotator;
        private readonly Transform target;
        
        public CopyRotationConstraint(ConfigNode node, Part p)
        {
            node.TryGetValue("rotatorsName", ref rotatorsName);
            node.TryGetValue("targetName", ref targetName);
            node.TryGetValue("isLocal", ref local);
            
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