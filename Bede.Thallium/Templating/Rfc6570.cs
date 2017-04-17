using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

#pragma warning disable 1591

namespace Bede.Thallium.Templating
{
    class Rfc6570 : Dictionary<Type, Func<object, object>>
    {
        static readonly Type String = typeof(string);
        static readonly Type IDict  = typeof(IDictionary);
        static readonly Type IEnum  = typeof(IEnumerable);
        static readonly Type KvP    = typeof(KeyValuePair<,>);

        static readonly Spec[] Map =
        {
            new Spec ( '\0', ',', false, '\0', false ),
            new Spec ( '\0', ',', false, '\0', true  ),
            new Spec (  '.', '.', false, '\0', false ),
            new Spec (  '/', '/', false, '\0', false ),
            new Spec (  ';', ';', true,  '\0', false ),
            new Spec (  '?', '&', true,   '=', false ),
            new Spec (  '&', '&', true,   '=', false ),
            new Spec (  '#', ',', false,  '=', true  ),
        };

        /// <summary>
        /// Returns the <see cref="Spec" /> for a variable string
        /// </summary>
        /// <param name="var"></param>
        /// <param name="vars"></param>
        /// <returns></returns>
        public static Spec spec(StringBuilder var, out Var[] vars)
        {
            var op   = var[0];
            var args = Map[0];

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
                    var.Remove(0, 1);
                    break;
            }

            vars = Var.Parse(var.ToString());

            return args;
        }

        /// <summary>
        /// Determines whether an object implements <see cref="IDictionary" />
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool is_dict(Type type)
        {
            return IDict.IsAssignableFrom(type);
        }

        /// <summary>
        /// Determines if an object is <see cref="IEnumerable" /> and has
        /// an element type of <see cref="KeyValuePair{TKey, TValue}"/>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool is_enumKV(Type type)
        {
            var ie = ienum(type);

            if (null == ie) return false;

            var arg = ie.GetGenericArguments()[0];

            if (!arg.IsGenericType) return false;

            var def = arg.GetGenericTypeDefinition();

            return KvP == def;
        }

        /// <summary>
        /// Determines an <see cref="IEnumerable{T}"/> compatible type
        /// for the specified one
        /// </summary>
        /// <remarks>
        /// Recursive implementation
        /// </remarks>
        /// <param name="type"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type ienum(Type type)
        {
            if (null   == type) return null;
            if (String == type) return null;

            if (!IEnum.IsAssignableFrom(type)) return null;

            var iis = type.GetInterfaces();

            for (var i = 0; i < iis.Length; ++i)
            {
                var ii = iis[i];

                if (!ii.IsGenericType)           continue;
                if (!IEnum.IsAssignableFrom(ii)) continue;

                return ii;
            }

            return null;
        }

        /// <summary>
        /// Strips the type parameters from an <see cref="IEnumerable"/>
        /// <see cref="KeyValuePair{TKey, TValue}"/> and returns a plain
        /// <see cref="IDictionary"/>
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IDictionary strip(IEnumerable val)
        {
            var _ = new Dictionary<object, object>();
            foreach (dynamic o in val)
            {
                _[o.Key] = o.Value;
            }
            return _;
        }

        /// <summary>
        /// Encodes content with a specific length
        /// </summary>
        /// <param name="input"></param>
        /// <param name="allow"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string enc(string input, bool allow, int len)
        {
            if (len < input.Length) input = input.Substring(0, len);

            return enc(input, allow);
        }

        /// <summary>
        /// Encodes content
        /// </summary>
        /// <param name="input"></param>
        /// <param name="allow"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string enc(string input, bool allow)
        {
            return allow ? Uri.EscapeUriString(input) : Uri.EscapeDataString(input);
        }

        /// <summary>
        /// Converts a <see cref="char"/> to a <see cref="string"/>, ignoring '\0'
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string tos(char ch)
        {
            return 0 == ch ? string.Empty : ch.ToString();
        }

        /// <summary>
        /// Returns <see cref="object.ToString"/> or <see cref="string.Empty"/> if
        /// the object is null
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string fmt(object o)
        {
            return o?.ToString() ?? string.Empty;
        }

        public Rfc6570()
        {
            Register<DateTime,       Data.Iso>    (o => o);
            Register<DateTimeOffset, Data.Iso>    (o => o);
            Register<byte[],         Data.Base64> (o => o);
            Register<bool,           Data.Boolean>(o => o);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Named(StringBuilder builder, string key, bool empt, char ifemp, bool allow)
        {
            builder.Append(enc(key, allow));
            builder.Append(tos(empt ? ifemp : '='));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Scalar(StringBuilder builder, bool named, char ifemp, bool allow, string key, int length, Meta val)
        {
            if (named)
            {
                Named(builder, key, val.IsNullOrEmpty, ifemp, allow);
            }

            builder.Append(enc(fmt(val.Object), allow, length));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Scalar(StringBuilder builder, bool named, char ifemp, bool allow, string key, Meta val)
        {
            if (named)
            {
                Named(builder, key, val.IsNullOrEmpty, ifemp, allow);
            }

            builder.Append(enc(fmt(val.Object), allow));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NoExplode(StringBuilder builder, bool named, char ifemp, bool allow, string key, Meta val)
        {
            if (named)
            {
                Named(builder, key, val.IsNullOrEmpty, ifemp, allow);
            }

            if (val.IsDictionary || val.IsEnumKV)
            {
                Dict(builder, val, ',', ',', ifemp, allow);
            }
            else
            {
                List(builder, val, ',', allow);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Explode(StringBuilder builder, char sep, bool named, char ifemp, bool allow, string key, Meta val)
        {
            if (val.IsDictionary || val.IsEnumKV)
            {
                Dict(builder, val, sep, '=', ifemp, allow);
            }
            else
            {
                List(builder, val, sep, named, key, allow);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void List(StringBuilder builder, Meta val, char sep, bool allow)
        {
            var first = string.Empty;

            foreach (var o in val.Enumerable)
            {
                if (null == o) continue;

                builder.Append(first);

                AppendObj(builder, o, allow);

                first = sep.ToString();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void List(StringBuilder builder, Meta val, char sep, bool named, string key, bool allow)
        {
            var first = string.Empty;
            var name  = named ? key + '=' : string.Empty;

            foreach (var o in val.Enumerable)
            {
                var meta = Meta(o);

                if (meta.IsNullOrEmpty) continue;

                builder.Append(first);
                builder.Append(name);

                AppendObj(builder, meta.Object, allow);

                first = sep.ToString();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dict(StringBuilder builder, Meta dict, char sep, char kvsep, char ifemp, bool allow)
        {
            var first = string.Empty;

            foreach (DictionaryEntry o in dict.Dictionary)
            {
                builder.Append(first);

                var key = Meta(o.Key);
                var val = Meta(o.Value);

                AppendObj(builder, key.Object, allow);
                AppendValue(builder, val.Object, kvsep, ifemp, allow);

                first = sep.ToString();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AppendObj(StringBuilder builder, object o, bool allow)
        {
            var v = enc(fmt(o), allow);

            builder.Append(v);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AppendValue(StringBuilder builder, object value, char sep, char ifemp, bool allow)
        {
            var v = enc(fmt(value), allow);

            if (0 == v.Length)
            {
                builder.Append(ifemp);
            }
            else
            {
                builder.Append(sep + v);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Meta Meta(object o)
        {
            return new Meta(cnv(o));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        object cnv(object o)
        {
            if (null == o) return null;

            if (o is string) return o;

            var baset = o.GetType();
            var nullt = Nullable.GetUnderlyingType(baset);

            Func<object, object> func;
            if (TryGetValue(nullt ?? baset, out func))
            {
                o = func(o);
            }

            return o;
        }

        /// <summary>
        /// Registers a type conversion function for an object of the specified type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        public Rfc6570 Register<T, V>(Func<T, V> func)
        {
            this[typeof(T)] = o => func((T) o);
            return this;
        }
    }
}