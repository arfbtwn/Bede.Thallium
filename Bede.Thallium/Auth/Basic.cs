using System;
using Bede.Thallium.Data;

#pragma warning disable 1591

namespace Bede.Thallium.Auth
{
    /// <summary>
    /// A type that encodes its parameters as a basic authorisation string
    /// </summary>
    public sealed class Basic : Header
    {
        readonly Base64 _value;

        Basic(Base64 value) : base("Basic", string.Empty)
        {
            _value = value ?? string.Empty;

            if (!_value.Original.Contains(":"))
            {
                throw new ArgumentException(nameof(value));
            }
        }

        public override string ToString() => base.ToString() + _value;

        public static implicit operator Basic(Base64 value) => new Basic(value);
        public static implicit operator Basic(string value) => (Base64) value;
    }
}
