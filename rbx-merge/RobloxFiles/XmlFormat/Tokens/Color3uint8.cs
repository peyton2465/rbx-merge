﻿using System.Xml;
using RobloxFiles.DataTypes;

namespace RobloxFiles.XmlFormat.PropertyTokens
{
    public class Color3uint8Token : IXmlPropertyToken
    {
        public string Token => "Color3uint8";

        public bool ReadProperty(Property prop, XmlNode token)
        {
            uint value;

            if (XmlPropertyTokens.ReadPropertyGeneric(token, out value))
            {
                uint r = (value >> 16) & 0xFF;
                uint g = (value >> 8) & 0xFF;
                uint b = value & 0xFF;

                Color3uint8 result = Color3.FromRGB(r, g, b);
                prop.Value = result;

                return true;
            }
            
            return false;
        }

        public void WriteProperty(Property prop, XmlDocument doc, XmlNode node)
        {
            Color3uint8 color = prop.CastValue<Color3uint8>();

            uint r = color.R,
                 g = color.G,
                 b = color.B;

            uint rgb = (255u << 24) | (r << 16) | (g << 8) | b;
            node.InnerText = rgb.ToString();
        }
    }
}
