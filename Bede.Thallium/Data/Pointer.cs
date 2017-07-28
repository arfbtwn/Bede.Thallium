using System.Diagnostics;

#pragma warning disable 1591

namespace Bede.Thallium.Data
{
    [DebuggerDisplay("{_value}")]
    public abstract class Pointer
    {
        readonly object _value;

        protected Pointer() { }

        protected Pointer(object value)
        {
            _value = value;
        }

        public sealed override bool Equals(object obj)
        {
            return _value == obj;
        }

        public sealed override int GetHashCode()
        {
            return _value?.GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            return _value?.ToString() ?? string.Empty;
        }
    }
}
