using System;
using UnityEngine;

namespace Restock.MaterialModifiers
{
    public class TexturePropertyMaterialModifier : IMaterialModifier
    {
        private readonly string propertyName;
        private readonly Texture2D texture;

        public TexturePropertyMaterialModifier(string propertyName, Texture2D texture)
        {
            this.propertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            this.texture = texture ?? throw new ArgumentNullException(nameof(texture));
        }

        public void Modify(Material material)
        {
            material.SetTexture(propertyName, texture);
        }
    }
}
