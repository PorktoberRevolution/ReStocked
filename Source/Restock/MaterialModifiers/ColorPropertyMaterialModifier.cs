using System;
using UnityEngine;

namespace Restock.MaterialModifiers
{
    public class ColorPropertyMaterialModifier : IMaterialModifier
    {
        private readonly string propertyName;
        private readonly Color color;

        public ColorPropertyMaterialModifier(string propertyName, Color color)
        {
            this.propertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            this.color = color;
        }

        public void Modify(Material material)
        {
            material.SetColor(propertyName, color);
        }
    }
}
