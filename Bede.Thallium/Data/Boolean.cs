#pragma warning disable 1591

namespace Bede.Thallium.Data
{
    /// <summary>
    /// A <see cref="bool"/> wrapper type with lowercase formatting
    /// </summary>
    public sealed class Boolean
    {
        readonly bool? _b;

        Boolean(bool? b) { _b = b; }

        public override string ToString()
        {
            return _b.HasValue ? _b.Value ? "true" : "false" : string.Empty;
        }

        public static implicit operator Boolean(bool? value) => new Boolean(value);
    }
}
