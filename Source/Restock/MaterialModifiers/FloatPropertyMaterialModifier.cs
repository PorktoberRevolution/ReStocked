using System;
using UnityEngine;

namespace Restock.MaterialModifiers
{
    public class FloatPropertyMaterialModifier : IMaterialModifier
    {
        private readonly string propertyName;
        private readonly float value;

        public FloatPropertyMaterialModifier(string propertyName, float value)
        {
            this.propertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            this.value = value;
        }

        public void Modify(Material material)
        {
            material.SetFloat(propertyName, value);
        }
    }
}
