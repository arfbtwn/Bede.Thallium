using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Bede.Thallium.Templating
{
    using Params = IReadOnlyDictionary<string, object>;

    class Compiled : Rfc6570
    {
        abstract class Op
        {
            public abstract void Expand(StringBuilder sb, Params args);
        }

        sealed class Append : Op
        {
            readonly string _frag;

            public Append(string frag)
            {
                _frag = frag;
            }

            public override void Expand(StringBuilder sb, Params args)
            {
                sb.Append(_frag);
            }
        }

        sealed class Expansion : Op
        {
            class Act
            {
                internal delegate void _(StringBuilder sb, Meta v, bool first);

                public Act(Rfc6570 rfc, Spec spec, Var var)
                {
                    Rfc  = rfc;
                    Spec = spec;
                    Var  = var;
                }

                public readonly Rfc6570 Rfc;
                public readonly Spec    Spec;
                public readonly Var     Var;
                public          Lazy<_> Func;

                Meta _type;
                Type _last;

                public Meta Meta
                {
                    set
                    {
                        _type = value;

                        if (_last == _type.Type) return;

                        _last = _type.Type;

                        Func = new Lazy<_>(_Func);
                    }
                }

                _ _Func()
                {
                    var str   = _type.IsString;
                    var ienum = _type.IsEnumerable;
                    var dict  = _type.IsDictionary || _type.IsEnumKV;

                    if (!ienum || str)
                    {
                        return Var.prefix  ? Expand         : new _(Prefix);
                    }
                    else if (dict)
                    {
                        return Var.explode ? Explode_Dict   : new _(Dict);
                    }
                    else
                    {
                        return Var.explode ? Explode_List   : new _(List);
                    }
                }

                void Expand(StringBuilder sb, Meta v, bool first)
                {
                    sb.Append(tos(first ? Spec.fst : Spec.sep));
                    Rfc6570.Scalar(sb, Spec.named, Spec.ifemp, Spec.allow, Var.key, Var.length, v);
                }

                void Prefix(StringBuilder sb, Meta v, bool first)
                {
                    sb.Append(tos(first ? Spec.fst : Spec.sep));
                    Rfc6570.Scalar(sb, Spec.named, Spec.ifemp, Spec.allow, Var.key, v);
                }

                void Explode_Dict(StringBuilder sb, Meta v, bool first)
                {
                    sb.Append(tos(first ? Spec.fst : Spec.sep));

                    Rfc.Dict(sb, v, Spec.sep, '=', Spec.ifemp, Spec.allow);
                }

                void Explode_List(StringBuilder sb, Meta v, bool first)
                {
                    sb.Append(tos(first ? Spec.fst : Spec.sep));

                    Rfc.List(sb, v, Spec.sep, Spec.named, Var.key, Spec.allow);
                }

                void Dict(StringBuilder sb, Meta v, bool first)
                {
                    sb.Append(tos(first ? Spec.fst : Spec.sep));

                    Named(sb, v);

                    Rfc.Dict(sb, v, ',', ',', Spec.ifemp, Spec.allow);
                }

                void List(StringBuilder sb, Meta v, bool first)
                {
                    sb.Append(tos(first ? Spec.fst : Spec.sep));

                    Named(sb, v);

                    Rfc.List(sb, v, ',', Spec.allow);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                void Named(StringBuilder sb, Meta v)
                {
                    if (Spec.named)
                    {
                        Rfc6570.Named(sb, Var.key, v.IsNullOrEmpty, Spec.ifemp, Spec.allow);
                    }
                }
            }

            readonly Rfc6570 _rfc;
            readonly Act[]   _actions;

            public Expansion(Rfc6570 rfc, Spec spec, Var[] vars)
            {
                var actions = new List<Act>();

                for (var i = 0; i < vars.Length; ++i)
                {
                    actions.Add(new Act(rfc, spec, vars[i]));
                }

                _rfc     = rfc;
                _actions = actions.ToArray();
            }

            public override void Expand(StringBuilder sb, Params args)
            {
                var first = true;

                for (var i = 0; i < _actions.Length; ++i)
                {
                    var act = _actions[i];

                    object v;
                    if (!args.TryGetValue(act.Var.key, out v))
                    {
                        continue;
                    }

                    var meta = _rfc.Meta(v);
                    if (meta.IsNull || (!meta.IsString && meta.IsEmpty))
                    {
                        continue;
                    }

                    act.Meta = meta;
                    act.Func.Value(sb, meta, first);
                    first = false;
                }
            }
        }

        public string Template { get; }

        readonly Lazy<Op[]> _ops;

        int _length;

        public Compiled(string template)
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

            for (var i = 0; i < Template.Length; ++i)
            {
                var c = Template[i];

                switch (c)
                {
                    case '{':
                        ops.Add(new Append(sb.ToString()));
                        sb.Clear();
                        inV = true;
                        break;
                    case '}':
                        Var[] vars;
                        ops.Add(new Expansion(this, spec(vb, out vars), vars));
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

        public Compiled Compile()
        {
            _length = _ops.Value.Length;
            return this;
        }

        public string Expand(Params parameters)
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