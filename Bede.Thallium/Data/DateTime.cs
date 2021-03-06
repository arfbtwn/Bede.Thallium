﻿using System;

#pragma warning disable 1591

namespace Bede.Thallium.Data
{
    /// <summary>
    /// Base class for pre-specified <see cref="DateTimeOffset" /> formats
    /// </summary>
    [Obsolete]
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
    /// An object that knows a date time format
    /// </summary>
    public interface IDateTimeFormat
    {
        string Format { get; }
    }

    /// <summary>
    /// A generic date time formatter
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DateTime<T> : Pointer where T : IDateTimeFormat, new()
    {
        static readonly string _f = new T().Format;

        protected DateTime() { }

        readonly string _value;

        protected DateTime(DateTimeOffset? time) : base(time) { _value = time?.ToString(_f); }
        protected DateTime(DateTime?       time) : base(time) { _value = time?.ToString(_f); }

        public sealed override string ToString() => _value ?? string.Empty;

        public static implicit operator DateTime<T>(DateTimeOffset? value) => new DateTime<T>(value);
        public static implicit operator DateTime<T>(DateTime?       value) => new DateTime<T>(value);
    }

    /// <summary>
    /// Round robin format, &quot;o&quot;
    /// </summary>
    public sealed class Iso : DateTime<Iso>, IDateTimeFormat
    {
        public Iso() { }

        Iso(DateTimeOffset? time) : base(time) { }
        Iso(DateTime?       time) : base(time) { }

        public string Format { get; } = "o";

        public static implicit operator Iso(DateTimeOffset? value) => new Iso(value);
        public static implicit operator Iso(DateTime?       value) => new Iso(value);
    }
}
