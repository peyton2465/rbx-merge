﻿using System;
using System.Text;
using System.Collections.Generic;
using Konscious.Security.Cryptography;

namespace RobloxFiles.DataTypes
{
    // SharedString is a datatype that takes a sequence of bytes and stores it in a 
    // lookup table that is shared by the entire file. It originally used MD5 for the
    // hashing, but Roblox now uses Blake2B to avoid the obvious problems with using MD5.
    
    // In practice the value of a SharedString does not have to match the hash of the
    // data it represents, it just needs to be distinct and MUST be 16 bytes long.
    // The XML format still uses 'md5' as its attribute key to the lookup table.

    public class SharedString
    {
        private static Dictionary<string, byte[]> Lookup = new Dictionary<string, byte[]>();
        public string Key { get; internal set; }
        public string ComputedKey { get; internal set; }

        public byte[] SharedValue => Find(ComputedKey ?? Key);
        public override string ToString() => $"Key: {ComputedKey ?? Key}";

        internal SharedString(string key)
        {
            Key = key;
        }

        internal static void Register(string key, byte[] buffer)
        {
            Lookup.Add(key, buffer);
        }

        private SharedString(byte[] buffer)
        {
            using (HMACBlake2B blake2B = new HMACBlake2B(16 * 8))
            {
                byte[] hash = blake2B.ComputeHash(buffer);
                ComputedKey = Convert.ToBase64String(hash);
                Key = ComputedKey;
            }

            if (Lookup.ContainsKey(ComputedKey))
                return;

            Register(ComputedKey, buffer);
        }

        public static byte[] Find(string key)
        {
            byte[] result = null;

            if (Lookup.ContainsKey(key))
                result = Lookup[key];

            return result;
        }

        public static SharedString FromBuffer(byte[] buffer)
        {
            return new SharedString(buffer);
        }

        public static SharedString FromString(string value)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(value);
            return new SharedString(buffer);
        }

        public static SharedString FromBase64(string base64)
        {
            byte[] buffer = Convert.FromBase64String(base64);
            return new SharedString(buffer);
        }
    }
}
