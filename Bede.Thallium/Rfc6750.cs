using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Bede.Thallium
{
    using Belt;

    using Param  = KeyValuePair<string, object>;
    using Params = Dictionary  <string, object>;

    class Rfc6750
    {
        protected readonly StringBuilder Builder  = new StringBuilder();
        protected readonly StringBuilder Variable = new StringBuilder();

        public string Expand(string template, Params parameters)
        {
            List<string> list = null;
            return Expand(template, parameters, ref list);
        }

        public string Expand(string template, Params parameters, ref List<string> unused)
        {
            var used = new HashSet<string>();
            var inV  = false;

            foreach (var c in template)
            {
                switch (c)
                {
                    case '{':
                        inV = true;
                        break;
                    case '}':
                        inV = false;
                        Expand(parameters, used);
                        break;
                    default:
                        if (inV) Variable.Append(c);
                        else     Builder.Append(c);
                        break;
                }
            }

            if (null != unused)
            {
                unused.AddRange(parameters.Keys.Except(used));
            }

            return Builder.ToString();
        }

        void Expand(Params keys, ICollection<string> used)
        {
            char   op   = Variable[0];
            string vars = null;

            if (Mappings.ContainsKey(op))
            {
                vars = Variable.Remove(0, 1).ToString();
            }
            else
            {
                op   = '\0';
                vars = Variable.ToString();
            }

            var args = Mappings[op];

            Expand((char) args[0], (char) args[1], (bool) args[2], (char) args[3], (bool) args[4], vars, keys, used);

            Variable.Clear();
        }

        void Expand(char fst, char sep, bool named, char ifemp, bool allow, string vars, Params keys, ICollection<string> used)
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
                        used.Add(var.ToString());
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
            used.Add(var.ToString());
        }

        void Expand(char fst, char sep, bool named, char ifemp, bool allow, Params keys, string key, bool explode, bool prefix, string length, ref bool first)
        {
            object obj;
            if (!keys.TryGetValue(key, out obj) || empt(obj))
            {
                return;
            }

            Expand(fst, sep, named, ifemp, allow, key, obj, explode, prefix, length, first);
            first = false;
        }

        void Expand(char fst, char sep, bool named, char ifemp, bool allow, string key, object obj, bool explode, bool prefix, string length, bool first)
        {
            var objs = obj as ICollection;

            Builder.Append(tos(first ? fst : sep));

            if (null == objs)
            {
                Expand(named, ifemp, allow, key, obj.ToString(), prefix, length);
            }
            else if (!explode)
            {
                NoExplode(named, ifemp, allow, key, objs);
            }
            else
            {
                Explode(sep, named, ifemp, allow, key, objs);
            }
        }

        void Expand(bool named, char ifemp, bool allow, string key, string val, bool prefix, string length)
        {
            var empt = string.IsNullOrEmpty(val);

            if (named)
            {
                Builder.Append(key);
                Builder.Append(tos(empt ? ifemp : '='));
            }

            Builder.Append(cyc(val, allow, prefix ? int.Parse(length) : int.MaxValue));
        }

        void NoExplode(bool named, char ifemp, bool allow, string key, ICollection val)
        {
            var first = true;

            if (named)
            {
                Builder.Append(key);
                Builder.Append(tos(empt(val) ? ifemp : '='));
            }

            foreach (var o in val)
            {
                if (empt(o)) continue;

                var type = o.GetType();
                var isKvp = o is DictionaryEntry || type.HasGenericDefinition(typeof(KeyValuePair<,>));

                if (!first) Builder.Append(',');

                if (isKvp)  AppendKvp(o, ',', ifemp, allow);
                else        AppendObj(o, allow);

                first = false;
            }
        }

        void Explode(char sep, bool named, char ifemp, bool allow, string key, ICollection val)
        {
            var first = true;

            foreach (var o in val)
            {
                if (empt(o)) continue;

                var type = o.GetType();
                var isKvp = o is DictionaryEntry || type.HasGenericDefinition(typeof(KeyValuePair<,>));

                if (!first) Builder.Append(sep);

                if (isKvp) AppendKvp(o, '=', ifemp, allow);
                else
                {
                    if (named) Builder.Append(key + '=');
                    AppendObj(o, allow);
                }

                first = false;
            }
        }

        void AppendKvp(object o, char sep, char ifemp, bool allow)
        {
            var kv = kvp(o);

            var k = cyc(kv.Key, allow);
            Builder.Append(k);

            var v = cyc((kv.Value ?? string.Empty).ToString(), allow);

            if (string.IsNullOrWhiteSpace(v))
                Builder.Append(ifemp);
            else
                Builder.Append(sep + v);
        }

        void AppendObj(object o, bool allow)
        {
            var v = cyc(o.ToString(), allow);

            Builder.Append(v);
        }

        static bool empt(object o)
        {
            return null == o || o is ICollection && 0 == ((ICollection) o).Count;
        }

        static string cyc(string input, bool allow, int len = int.MaxValue)
        {
            var ues = Uri.UnescapeDataString(input);

            if (len < ues.Length) ues = ues.Substring(0, len);

            return allow
                ? Uri.EscapeUriString (ues)
                : Uri.EscapeDataString(ues);
        }

        static Param kvp(object o)
        {
            var dyn = (dynamic) o;

            return new Param(dyn.Key.ToString(), dyn.Value);
        }

        static string tos(char ch)
        {
            return 0 == ch ? string.Empty : ch.ToString();
        }

        static readonly Dictionary<char, object[]> Mappings = new Dictionary<char, object[]>
        {
            { '\0', new object[] { '\0', ',', false, '\0', false } },
            {  '+', new object[] { '\0', ',', false, '\0', true  } },
            {  '.', new object[] {  '.', '.', false, '\0', false } },
            {  '/', new object[] {  '/', '/', false, '\0', false } },
            {  ';', new object[] {  ';', ';', true,  '\0', false } },
            {  '?', new object[] {  '?', '&', true,   '=', false } },
            {  '&', new object[] {  '&', '&', true,   '=', false } },
            {  '#', new object[] {  '#', ',', false,  '=', true  } },
        };
    }
}
