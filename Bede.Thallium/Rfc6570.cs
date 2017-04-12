using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Bede.Thallium
{
    using Belt;

    using Params = IReadOnlyDictionary<string, object>;

    class Rfc6570
    {
        static readonly Type IDict = typeof(IDictionary);
        static readonly Type IEnum = typeof(IEnumerable<>);
        static readonly Type KvP   = typeof(KeyValuePair<,>);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static IDictionary as_dict(object val)
        {
            return val as IDictionary ?? (is_enumKV(val?.GetType()) ? strip((IEnumerable) val) : null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool is_dict(Type type)
        {
            return IDict.IsAssignableFrom(type);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool is_enumKV(Type type)
        {
            var ie = ienum(type);

            if (null == ie) return false;

            var arg = ie.GetGenericArguments()[0];

            if (!arg.IsGenericType) return false;

            var def = arg.GetGenericTypeDefinition();

            return KvP == def;
        }

        static Type ienum(Type type)
        {
            if (type == null || type == typeof(string))
            {
                return null;
            }

            if (type.IsArray)
            {
                return IEnum.MakeGenericType(type.GetElementType());
            }

            if (type.IsGenericType)
            {
                foreach (var arg in type.GetGenericArguments())
                {
                    var target = IEnum.MakeGenericType(arg);

                    if (target.IsAssignableFrom(type))
                    {
                        return target;
                    }
                }
            }

            foreach (var i in type.GetInterfaces())
            {
                var ie = ienum(i);

                if (null == ie)
                {
                    continue;
                }

                return ie;
            }

            return ienum(type.BaseType);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static IDictionary strip(IEnumerable val)
        {
            var _ = new Dictionary<object, object>();
            foreach (dynamic o in val)
            {
                _[o.Key] = o.Value;
            }
            return _;
        }

        protected internal          StringBuilder Builder  = new StringBuilder();
        protected internal readonly StringBuilder Variable = new StringBuilder();

        public string Expand(string template, Params parameters)
        {
            Builder.Clear();
            var inV = false;

            foreach (var c in template)
            {
                switch (c)
                {
                    case '{':
                        inV = true;
                        Variable.Clear();
                        break;
                    case '}':
                        inV = false;
                        Expand(parameters);
                        break;
                    default:
                        if (inV) Variable.Append(c);
                        else     Builder.Append(c);
                        break;
                }
            }

            return Builder.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Expand(Params keys)
        {
            char     op   = Variable[0];
            string   vars = null;
            object[] args = Map[0];

            switch(op)
            {
                case '#':  args = Map[7]; goto case '\0';
                case '&':  args = Map[6]; goto case '\0';
                case '?':  args = Map[5]; goto case '\0';
                case ';':  args = Map[4]; goto case '\0';
                case '/':  args = Map[3]; goto case '\0';
                case '.':  args = Map[2]; goto case '\0';
                case '+':  args = Map[1]; goto case '\0';
                case '\0':
                    vars = Variable.ToString(1, Variable.Length - 1);
                    break;
                default:
                    vars = Variable.ToString();
                    break;
            }

            Expand((char) args[0], (char) args[1], (bool) args[2], (char) args[3], (bool) args[4], vars, keys);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Expand(char fst, char sep, bool named, char ifemp, bool allow, string vars, Params keys)
        {
            var var = new StringBuilder();
            var len = new StringBuilder();

            var explode = false;
            var prefix  = false;
            var first   = true;

            foreach (var c in vars)
            {
                switch(c)
                {
                    case ',':
                        Expand(fst, sep, named, ifemp, allow, keys, var.ToString(), explode, prefix, len.ToString(), ref first);
                        var.Clear();
                        len.Clear();
                        prefix  = false;
                        explode = false;
                        break;
                    case ':':
                        prefix = true;
                        break;
                    case '*':
                        explode = true;
                        break;
                    default:
                        if (prefix) len.Append(c);
                        else        var.Append(c);
                        break;
                }
            }

            Expand(fst, sep, named, ifemp, allow, keys, var.ToString(), explode, prefix, len.ToString(), ref first);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Expand(char fst, char sep, bool named, char ifemp, bool allow, Params keys, string key, bool explode, bool prefix, string length, ref bool first)
        {
            object obj;
            if (!keys.TryGetValue(key, out obj) || empt(obj))
            {
                return;
            }

            Expand(Builder, fst, sep, named, ifemp, allow, key, obj, explode, prefix, length, first);
            first = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Expand(StringBuilder builder, char fst, char sep, bool named, char ifemp, bool allow, string key, object obj, bool explode, bool prefix, string length, bool first)
        {
            var str  = obj as string;
            var objs = obj as IEnumerable;

            builder.Append(tos(first ? fst : sep));

            if (null == objs || null != str)
            {
                if (prefix)
                {
                    Expand(builder, named, ifemp, allow, key, obj.ToString(), true, int.Parse(length));
                }
                else
                {
                    Expand(builder, named, ifemp, allow, key, obj.ToString(), false);
                }
            }
            else if (explode)
            {
                Explode(builder, sep, named, ifemp, allow, key, objs);
            }
            else
            {
                NoExplode(builder, named, ifemp, allow, key, objs);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Expand(StringBuilder builder, bool named, char ifemp, bool allow, string key, string val, bool prefix, int length = int.MaxValue)
        {
            if (named)
            {
                var empt = string.IsNullOrEmpty(val);

                builder.Append(key);
                builder.Append(tos(empt ? ifemp : '='));
            }

            builder.Append(prefix ? cyc(val, allow, length) : cyc(val, allow));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void NoExplode(StringBuilder builder, bool named, char ifemp, bool allow, string key, IEnumerable val)
        {
            var dict = as_dict(val);

            if (named)
            {
                builder.Append(key);
                builder.Append(tos(empt(val) ? ifemp : '='));
            }

            if (null == dict)
            {
                List(builder, val, ',', allow);
            }
            else
            {
                Dict(builder, dict, ',', ',', ifemp, allow);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Explode(StringBuilder builder, char sep, bool named, char ifemp, bool allow, string key, IEnumerable val)
        {
            var dict = as_dict(val);

            if (null == dict)
            {
                List(builder, val, sep, named, key, allow);
            }
            else
            {
                Dict(builder, dict, sep, '=', ifemp, allow);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void List(StringBuilder builder, IEnumerable val, char sep, bool allow)
        {
            var first = string.Empty;

            foreach (var o in val)
            {
                if (null == o) continue;

                builder.Append(first);

                AppendObj(builder, o, allow);

                first = sep.ToString();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void List(StringBuilder builder, IEnumerable val, char sep, bool named, string key, bool allow)
        {
            var first = string.Empty;
            var name  = named ? key + '=' : string.Empty;

            foreach (var o in val)
            {
                if (null == o) continue;

                builder.Append(first);
                builder.Append(name);

                AppendObj(builder, o, allow);

                first = sep.ToString();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Dict(StringBuilder builder, IDictionary dict, char sep, char kvsep, char ifemp, bool allow)
        {
            var first = string.Empty;

            foreach (DictionaryEntry o in dict)
            {
                builder.Append(first);

                AppendKvp(builder, o, kvsep, ifemp, allow);

                first = sep.ToString();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AppendKvp(StringBuilder builder, DictionaryEntry kv, char sep, char ifemp, bool allow)
        {
            AppendObj(builder, kv.Key, allow);

            var v = cyc((kv.Value ?? string.Empty).ToString(), allow);

            if (0 == v.Length)
                builder.Append(ifemp);
            else
                builder.Append(sep + v);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AppendObj(StringBuilder builder, object o, bool allow)
        {
            var v = cyc(o.ToString(), allow);

            builder.Append(v);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool empt(object o)
        {
            return null == o || o is ICollection && 0 == ((ICollection) o).Count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string cyc(string input, bool allow, int len)
        {
            if (len < input.Length) input = input.Substring(0, len);

            return cyc(input, allow);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string cyc(string input, bool allow)
        {
            return allow ? Uri.EscapeUriString(input) : Uri.EscapeDataString(input);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string tos(char ch)
        {
            return 0 == ch ? string.Empty : ch.ToString();
        }

        internal static readonly object[][] Map =
        {
            new object[] { '\0', ',', false, '\0', false },
            new object[] { '\0', ',', false, '\0', true  },
            new object[] {  '.', '.', false, '\0', false },
            new object[] {  '/', '/', false, '\0', false },
            new object[] {  ';', ';', true,  '\0', false },
            new object[] {  '?', '&', true,   '=', false },
            new object[] {  '&', '&', true,   '=', false },
            new object[] {  '#', ',', false,  '=', true  },
        };
    }
}
