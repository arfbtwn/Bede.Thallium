using System;
using System.Text;

#pragma warning disable 1591

namespace Bede.Thallium.Data
{
    using static Convert;
    using static Encoding;

    /// <summary>
    /// A value that is base64 encoded
    /// </summary>
    public sealed class Base64 : Pointer
    {
        readonly string _original;
        readonly byte[] _bytes;
        readonly string _encoded;

        Base64(byte[] value) : base(value)
        {
            _bytes   = value ?? new byte[0];
            _encoded = ToBase64String(_bytes);
        }

        Base64(string value) : base(value)
        {
            _original = value ?? string.Empty;
            _bytes    = UTF8.GetBytes(_original);
            _encoded  = ToBase64String(_bytes);
        }

        public string Original => _original ?? UTF8.GetString(_bytes);

        public override string ToString() => _encoded;

        public static implicit operator Base64(string value) => new Base64(value);
        public static implicit operator Base64(byte[] value) => new Base64(value);
    }
}
