
#pragma warning disable 1591

namespace Bede.Thallium.Auth
{
    /// <summary>
    /// A type that encodes as a Bearer token
    /// </summary>
    public sealed class Bearer : Header
    {
        Bearer(string value) : base("Bearer", value) { }

        public static implicit operator Bearer(string value) => new Bearer(value);
        public static implicit operator Bearer(Token value)  => value.access_token;
    }
}
