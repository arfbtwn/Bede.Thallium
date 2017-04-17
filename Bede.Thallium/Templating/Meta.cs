using System;
using System.Collections;

namespace Bede.Thallium.Templating
{
    struct Meta
    {
        static bool _empt(string      value) =>  0 == value?.Length;
        static bool _empt(IEnumerable value) => !value?.GetEnumerator().MoveNext() ?? false;
        static bool _empt(ICollection value) =>  0 == value?.Count;

        public Meta(object value)
        {
            Object = value;
        }

        public readonly object Object;

        public Type Type   => Object?.GetType();
        public bool IsNull => null == Object || Object.Equals(null);

        public bool IsEmpty
        {
            get
            {
                if (IsString)     return _empt(String);
                if (IsCollection) return _empt(Collection);
                if (IsEnumerable) return _empt(Enumerable);

                return false;
            }
        }

        public bool IsNullOrEmpty => IsNull || IsEmpty;

        public bool IsString      => Object is string;
        public bool IsCollection  => Object is ICollection;
        public bool IsEnumerable  => Object is IEnumerable;
        public bool IsDictionary  => Rfc6570.is_dict  (Type);
        public bool IsEnumKV      => Rfc6570.is_enumKV(Type);

        public string      String     => (string)      Object;
        public ICollection Collection => (ICollection) Object;
        public IEnumerable Enumerable => (IEnumerable) Object;
        public IDictionary Dictionary
        {
            get
            {
                if (IsDictionary) return (IDictionary) Object;
                if (IsEnumKV)     return Rfc6570.strip(Enumerable);

                return null;
            }
        }
    }
}