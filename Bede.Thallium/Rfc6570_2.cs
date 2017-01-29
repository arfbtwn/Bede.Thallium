using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Bede.Thallium
{
    class Rfc6570_2
    {
        abstract class Op
        {
            public abstract void Expand(StringBuilder sb, Dictionary<string, object> args);
        }

        sealed class Append : Op
        {
            readonly string _frag;

            public Append(string frag)
            {
                _frag = frag;
            }

            public override void Expand(StringBuilder sb, Dictionary<string, object> args)
            {
                sb.Append(_frag);
            }
        }

        sealed class Expansion : Op
        {
            class Def
            {
                public char fst;
                public char sep;
                public bool named;
                public char ifemp;
                public bool allow;
            }

            class Act
            {
                internal delegate void _(StringBuilder sb, object v, bool first);

                public Act(Def def, string var)
                {
                    _def = def;

                    var vb = new StringBuilder();
                    var pb = new StringBuilder();
                    foreach (var c in var)
                    {
                        switch(c)
                        {
                            case ':':
                                _prefix = true;
                                break;
                            case '*':
                                _explode = true;
                                break;
                            default:
                                if (_prefix) pb.Append(c);
                                else         vb.Append(c);
                                break;
                        }
                    }

                    _length = _prefix ? int.Parse(pb.ToString()) : int.MaxValue;

                    Key  = vb.ToString();
                }

                public readonly string  Key;
                public          Lazy<_> Func;

                readonly bool _explode;
                readonly bool _prefix;
                readonly int  _length;
                readonly Def  _def;

                Type _type;

                public Type Type
                {
                    set
                    {
                        if (_type == value) return;

                        _type = value;

                        Func = new Lazy<_>(_Func);
                    }
                }

                _ _Func()
                {
                    var str   = typeof(string).IsAssignableFrom(_type);
                    var ienum = typeof(IEnumerable).IsAssignableFrom(_type);
                    var dict  = Rfc6570.is_dict(_type);
                    var enKV  = Rfc6570.is_enumKV(_type);

                    if (!ienum || str)
                    {
                        return Expand;
                    }
                    else if (dict)
                    {
                        return _explode ? Explode_Dict   : new _(Dict);
                    }
                    else if (enKV)
                    {
                        return _explode ? Explode_EnumKV : new _(EnumKV);
                    }
                    else
                    {
                        return _explode ? Explode_List   : new _(List);
                    }
                }

                void Expand(StringBuilder sb, object v, bool first)
                {
                    sb.Append(Rfc6570.tos(first ? _def.fst : _def.sep));

                    Rfc6570.Expand(sb, _def.named, _def.ifemp, _def.allow, Key, v.ToString(), _prefix, _length);
                }

                void Explode_Dict(StringBuilder sb, object v, bool first)
                {
                    sb.Append(Rfc6570.tos(first ? _def.fst : _def.sep));

                    Rfc6570.Dict(sb, (IDictionary) v, _def.sep, '=', _def.ifemp, _def.allow);
                }

                void Explode_EnumKV(StringBuilder sb, object v, bool first)
                {
                    sb.Append(Rfc6570.tos(first ? _def.fst : _def.sep));

                    var idict = Rfc6570.strip((IEnumerable) v);

                    Rfc6570.Dict(sb, idict, _def.sep, '=', _def.ifemp, _def.allow);
                }

                void Explode_List(StringBuilder sb, object v, bool first)
                {
                    sb.Append(Rfc6570.tos(first ? _def.fst : _def.sep));

                    Rfc6570.List(sb, (IEnumerable) v, _def.sep, _def.named, Key, _def.allow);
                }

                void Dict(StringBuilder sb, object v, bool first)
                {
                    sb.Append(Rfc6570.tos(first ? _def.fst : _def.sep));

                    Named(sb, v);

                    Rfc6570.Dict(sb, (IDictionary) v, ',', ',', _def.ifemp, _def.allow);
                }

                void EnumKV(StringBuilder sb, object v, bool first)
                {
                    sb.Append(Rfc6570.tos(first ? _def.fst : _def.sep));

                    Named(sb, v);

                    var idict = Rfc6570.strip((IEnumerable) v);

                    Rfc6570.Dict(sb, idict, ',', ',', _def.ifemp, _def.allow);
                }

                void List(StringBuilder sb, object v, bool first)
                {
                    sb.Append(Rfc6570.tos(first ? _def.fst : _def.sep));

                    Named(sb, v);

                    Rfc6570.List(sb, (IEnumerable) v, ',', _def.allow);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                void Named(StringBuilder sb, object v)
                {
                    if (!_def.named)
                    {
                        return;
                    }

                    sb.Append(Key);
                    sb.Append(Rfc6570.tos(Rfc6570.empt(v) ? _def.ifemp : '='));
                }
            }

            readonly Act[] _actions;

            public Expansion(string var)
            {
                var op   = var[0];
                var args = Rfc6570.Map[0];

                switch(op)
                {
                    case '#':  args = Rfc6570.Map[7]; goto case '\0';
                    case '&':  args = Rfc6570.Map[6]; goto case '\0';
                    case '?':  args = Rfc6570.Map[5]; goto case '\0';
                    case ';':  args = Rfc6570.Map[4]; goto case '\0';
                    case '/':  args = Rfc6570.Map[3]; goto case '\0';
                    case '.':  args = Rfc6570.Map[2]; goto case '\0';
                    case '+':  args = Rfc6570.Map[1]; goto case '\0';
                    case '\0':
                        var = var.Substring(1);
                        break;
                }

                var def = new Def
                {
                    fst   = (char) args[0],
                    sep   = (char) args[1],
                    named = (bool) args[2],
                    ifemp = (char) args[3],
                    allow = (bool) args[4]
                };

                var splits = var.Split(',');

                var actions = new List<Act>();

                foreach (var spec in splits)
                {
                    actions.Add(new Act(def, spec));
                }

                _actions = actions.ToArray();
            }

            public override void Expand(StringBuilder sb, Dictionary<string, object> args)
            {
                var first = true;

                for (var i = 0; i < _actions.Length; ++i)
                {
                    var act = _actions[i];

                    object v;
                    if (!args.TryGetValue(act.Key, out v) || Rfc6570.empt(v))
                    {
                        continue;
                    }

                    act.Type = v.GetType();

                    act.Func.Value(sb, v, first);
                    first = false;
                }
            }
        }

        public string Template { get; }

        readonly Lazy<Op[]> _ops;

        int _length;

        public Rfc6570_2(string template)
        {
            Template = template;

            _ops = new Lazy<Op[]>(_, LazyThreadSafetyMode.PublicationOnly);
        }

        Op[] _()
        {
            var sb = new StringBuilder();
            var vb = new StringBuilder();

            var ops = new List<Op>();

            var inV = false;

            foreach (var c in Template)
            {
                switch (c)
                {
                    case '{':
                        ops.Add(new Append(sb.ToString()));
                        sb.Clear();
                        inV = true;
                        break;
                    case '}':
                        ops.Add(new Expansion(vb.ToString()));
                        vb.Clear();
                        inV = false;
                        break;
                    default:
                        if (inV) vb.Append(c);
                        else     sb.Append(c);
                        break;
                }
            }

            if (0 < sb.Length) ops.Add(new Append(sb.ToString()));

            return ops.ToArray();
        }

        public Rfc6570_2 Compile()
        {
            _length = _ops.Value.Length;
            return this;
        }

        public string Expand(Dictionary<string, object> parameters)
        {
            Compile();

            var ops = _ops.Value;

            var sb = new StringBuilder();

            for (var i = 0; i < _length; ++i)
            {
                ops[i].Expand(sb, parameters);
            }

            return sb.ToString();
        }
    }
}