using System;
using UnityEngine;

namespace Restock.MaterialModifiers
{
    public class MaterialModifierParser
    {
        private readonly string logContext;

        public MaterialModifierParser(string logContext = nameof(MaterialModifierParser))
        {
            this.logContext = logContext ?? throw new ArgumentNullException(nameof(logContext));
        }

        public IMaterialModifier Parse(ConfigNode node)
        {
            string propertyName = node.GetValue("name");

            if (propertyName == null)
                throw new FormatException("name not found, cannot create material modifier");
            else if (propertyName == string.Empty)
                throw new FormatException("name is empty, cannot create material modifier");

            if (node.name == "FLOAT_PROPERTY")
            {
                float value = float.Parse(node.GetValue("value"));

                return new FloatPropertyMaterialModifier(propertyName, value);
            }
            else if (node.name == "COLOR_PROPERTY")
            {
                Color color = ConfigNode.ParseColor(node.GetValue("color"));

                return new ColorPropertyMaterialModifier(propertyName, color);
            }
            else if (node.name == "TEXTURE_PROPERTY")
            {
                string textureUrl = node.GetValue("textureUrl");
                bool normalMapToggle = false;

                if (node.GetValue("isNormalMap") is string normalMapToggleString)
                    normalMapToggle = bool.Parse(normalMapToggleString);

                GameDatabase.TextureInfo textureInfo = GameDatabase.Instance.GetTextureInfo(textureUrl);

                if (textureInfo == null)
                    throw new Exception($"Cannot find texture: '{textureUrl}'");

                Texture2D texture = normalMapToggle ? textureInfo.normalMap : textureInfo.texture;

                if (texture == null)
                    throw new Exception($"Cannot get texture from texture info '{textureUrl}' isNormalMap = {normalMapToggle}");

                return new TexturePropertyMaterialModifier(propertyName, texture);
            }
            else
            {
                throw new FormatException($"Can't create material modifier from node: '{node.name}'");
            }
        }
    }
}
