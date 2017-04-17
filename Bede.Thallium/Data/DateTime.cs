using System;

#pragma warning disable 1591

namespace Bede.Thallium.Data
{
    /// <summary>
    /// Base class for pre-specified <see cref="DateTimeOffset" /> formats
    /// </summary>
    public class DateTimeFormat : Pointer
    {
        readonly string _value;

        public DateTimeFormat(DateTimeOffset? offset, string format) : base(offset)
        {
            _value = offset?.ToString(format) ?? string.Empty;
        }

        public DateTimeFormat(DateTime? time, string format) : base(time)
        {
            _value = time?.ToString(format) ?? string.Empty;
        }

        public sealed override string ToString() => _value;
    }

    /// <summary>
    /// Round robin format, &quot;o&quot;
    /// </summary>
    public sealed class Iso : DateTimeFormat
    {
        Iso(DateTimeOffset? time) : base(time, "o") { }
        Iso(DateTime?       time) : base(time, "o") { }

        public static implicit operator Iso(DateTimeOffset? value) => new Iso(value);

        public static implicit operator Iso(DateTime? value)       => new Iso(value);
    }
}
