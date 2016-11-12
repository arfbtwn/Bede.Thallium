using System;

#pragma warning disable 1591

namespace Bede.Thallium.Data
{
    /// <summary>
    /// Base class for pre-specified <see cref="DateTimeOffset" /> formats
    /// </summary>
    public class DateTimeFormat
    {
        readonly DateTimeOffset? _time;
        readonly string          _fmt;

        public DateTimeFormat(DateTimeOffset? time, string format)
        {
            _time = time;
            _fmt  = format;
        }

        public sealed override string ToString()
        {
            return _time?.ToString(_fmt) ?? string.Empty;
        }
    }

    /// <summary>
    /// Round robin format, &quot;o&quot;
    /// </summary>
    public sealed class Iso : DateTimeFormat
    {
        Iso(DateTimeOffset? time) : base(time, "o") { }

        public static implicit operator Iso(DateTimeOffset? value) => new Iso(value);

        public static implicit operator Iso(DateTime? value)       => new Iso(value);
    }
}
