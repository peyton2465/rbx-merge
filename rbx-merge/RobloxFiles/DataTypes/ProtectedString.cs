﻿using System.Text;

namespace RobloxFiles.DataTypes
{
    /// <summary>
    /// ProtectedString is a type used by the Source property of scripts.
    /// If constructed as an array of bytes, it's assumed to be compiled byte-code.
    /// </summary>
    public class ProtectedString
    {
        public readonly bool IsCompiled;
        public readonly byte[] RawBuffer;

        public override string ToString()
        {
            if (IsCompiled)
                return $"byte[{RawBuffer.Length}]";

            return Encoding.UTF8.GetString(RawBuffer);
        }

        public ProtectedString(string value)
        {
            IsCompiled = false;
            RawBuffer = Encoding.UTF8.GetBytes(value);
        }

        public ProtectedString(byte[] compiled)
        {
            IsCompiled = true;
            RawBuffer = compiled;
        }

        public static implicit operator string(ProtectedString protectedString)
        {
            return Encoding.UTF8.GetString(protectedString.RawBuffer);
        }

        public static implicit operator ProtectedString(string value)
        {
            return new ProtectedString(value);
        }

        public static implicit operator byte[](ProtectedString protectedString)
        {
            return protectedString.RawBuffer;
        }

        public static implicit operator ProtectedString(byte[] value)
        {
            return new ProtectedString(value);
        }
    }
}
