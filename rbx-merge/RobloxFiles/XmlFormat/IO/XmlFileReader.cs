﻿using System;
using System.Xml;

using RobloxFiles.DataTypes;

namespace RobloxFiles.XmlFormat
{
    public static class XmlRobloxFileReader
    {
        private static Func<string, Exception> createErrorHandler(string label)
        {
            var errorHandler = new Func<string, Exception>((message) =>
            {
                string contents = $"XmlRobloxFileReader.{label}: {message}";
                return new Exception(contents);
            });

            return errorHandler;
        }

        public static void ReadSharedStrings(XmlNode sharedStrings, XmlRobloxFile file)
        {
            var error = createErrorHandler("ReadSharedStrings");

            if (sharedStrings.Name != "SharedStrings")
                throw error("Provided XmlNode's class should be 'SharedStrings'!");

            foreach (XmlNode sharedString in sharedStrings)
            {
                if (sharedString.Name == "SharedString")
                {
                    XmlNode hashNode = sharedString.Attributes.GetNamedItem("md5");

                    if (hashNode == null)
                        throw error("Got a SharedString without an 'md5' attribute!");

                    string key = hashNode.InnerText;
                    string value = sharedString.InnerText.Replace("\n", "");

                    byte[] hash = Convert.FromBase64String(key);
                    var record = SharedString.FromBase64(value);

                    if (hash.Length != 16)
                        throw error($"SharedString base64 key '{key}' must decode to byte[16]!");

                    if (key != record.Key)
                    {
                        SharedString.Register(key, record.SharedValue);
                        record.Key = key;
                    }

                    file.SharedStrings.Add(key);
                }
            }
        }

        public static void ReadMetadata(XmlNode meta, XmlRobloxFile file)
        {
            var error = createErrorHandler("ReadMetadata");

            if (meta.Name != "Meta")
                throw error("Provided XmlNode's class should be 'Meta'!");

            XmlNode propName = meta.Attributes.GetNamedItem("name");

            if (propName == null)
                throw error("Got a Meta node without a 'name' attribute!");

            string key = propName.InnerText;
            string value = meta.InnerText;

            file.Metadata[key] = value;
        }

        public static void ReadProperties(Instance instance, XmlNode propsNode)
        {
            var error = createErrorHandler("ReadProperties");

            if (propsNode.Name != "Properties")
                throw error("Provided XmlNode's class should be 'Properties'!");

            foreach (XmlNode propNode in propsNode.ChildNodes)
            {
                if (propNode.NodeType == XmlNodeType.Comment)
                    continue;

                string propType = propNode.Name;
                XmlNode propName = propNode.Attributes.GetNamedItem("name");

                if (propName == null)
                {
                    if (propNode.Name == "Item")
                        continue;

                    throw error("Got a property node without a 'name' attribute!");
                }

                IXmlPropertyToken tokenHandler = XmlPropertyTokens.GetHandler(propType);

                if (tokenHandler != null)
                {
                    Property prop = new Property()
                    {
                        Name = propName.InnerText,
                        Instance = instance,
                        XmlToken = propType
                    };

                    if (!tokenHandler.ReadProperty(prop, propNode))
                        Console.WriteLine("Could not read property: " + prop.GetFullName() + '!');

                    instance.AddProperty(ref prop);
                }
                else
                {
                    Console.WriteLine("No IXmlPropertyToken found for property type: " + propType + '!');
                }
            }
        }

        public static Instance ReadInstance(XmlNode instNode, XmlRobloxFile file)
        {
            var error = createErrorHandler("ReadInstance");

            // Process the instance itself
            if (instNode.Name != "Item")
                throw error("Provided XmlNode's name should be 'Item'!");

            XmlNode classToken = instNode.Attributes.GetNamedItem("class");
            if (classToken == null)
                throw error("Got an Item without a defined 'class' attribute!");


            string className = classToken.InnerText;

            Type instType = Type.GetType($"RobloxFiles.{className}") ?? typeof(Instance);
            Instance inst = Activator.CreateInstance(instType) as Instance;
            
            // The 'referent' attribute is optional, but should be defined if a Ref property needs to link to this Instance.
            XmlNode refToken = instNode.Attributes.GetNamedItem("referent");

            if (refToken != null && file != null)
            {
                string referent = refToken.InnerText;
                inst.Referent = referent;

                if (file.Instances.ContainsKey(referent))
                    throw error("Got an Item with a duplicate 'referent' attribute!");

                file.Instances.Add(referent, inst);
            }

            // Process the child nodes of this instance.
            foreach (XmlNode childNode in instNode.ChildNodes)
            {
                if (childNode.Name == "Properties")
                {
                    ReadProperties(inst, childNode);
                }
                else if (childNode.Name == "Item")
                {
                    Instance child = ReadInstance(childNode, file);
                    child.Parent = inst;
                }
            }

            return inst;
        }
    }
}
