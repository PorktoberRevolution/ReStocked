using System;
using UnityEngine;

namespace Restock.MaterialModifiers
{
    public interface IMaterialModifier
    {
        void Modify(Material material);
    }
}
