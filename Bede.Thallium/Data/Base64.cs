using System;
using System.Text;
using System.Threading;

#pragma warning disable 1591

namespace Bede.Thallium.Data
{
    using static Convert;
    using static Encoding;

    /// <summary>
    /// A string that is base64 encoded
    /// </summary>
    public sealed class Base64
    {
        readonly Lazy<string> _encoded;

        Base64(string value)
        {
            Original = value;

            _encoded = new Lazy<string>(_, LazyThreadSafetyMode.PublicationOnly);
        }

        public string Original { get; }

        string _() => ToBase64String(UTF8.GetBytes(Original ?? string.Empty));

        public override string ToString() => _encoded.Value;

        public static implicit operator Base64(string value) => new Base64(value);
    }
}
