using UnityEngine;

namespace Restock.Constraints
{
    [System.Serializable]
    /**
     * Copies the position of the target object to the mover object.
     * If `IsLocal` is true, uses local position instead of global position. 
     */
    public class CopyPositionConstraint : IConstraint
    {
        private readonly string moversName;
        private readonly string targetName;
        
        private readonly bool local = false;

        // Cached components
        private readonly Transform mover;
        private readonly Transform target;
        
        public CopyPositionConstraint(ConfigNode node, Part p)
        {
            node.TryGetValue("moversName", ref moversName);
            node.TryGetValue("targetName", ref targetName);
            node.TryGetValue("isLocal", ref local);
            
            mover = p.FindModelTransform(moversName);
            target = p.FindModelTransform(targetName);
        }
        
        public void Update()
        {
            if (mover == null || target == null) return;

            if (local)
            {
                mover.localPosition = target.localPosition;
            }
            else
            {
                mover.position = target.position;
            }
        }
    }
}