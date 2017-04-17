using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

#pragma warning disable 1591

namespace Bede.Thallium.Data
{
    public class Explode<T> : Pointer, IEnumerable<KeyValuePair<string, object>>
    {
        readonly T    _value;
        readonly Type _type;

        public Explode(T value) : base(value)
        {
            _value = value;
            _type  = value?.GetType();
        }

        protected virtual string Map(PropertyInfo pi) => pi.Name;

        protected virtual object Get(PropertyInfo pi) => pi.GetValue(_value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            if (null == _type)
            {
                yield break;
            }

            foreach (var pi in _type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                yield return new KeyValuePair<string, object>(Map(pi), Get(pi));
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public static implicit operator Explode<T>(T value) => new Explode<T>(value);
    }

    public class DataContractExplode<T> : Explode<T>
    {
        public DataContractExplode(T value) : base(value) { }

        protected override string Map(PropertyInfo pi)
        {
            var attr = pi.GetCustomAttribute<DataMemberAttribute>();

            return attr?.Name ?? pi.Name;
        }

        public static implicit operator DataContractExplode<T>(T value) => new DataContractExplode<T>(value);
    }
}
