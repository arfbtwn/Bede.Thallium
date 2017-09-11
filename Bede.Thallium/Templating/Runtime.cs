using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Bede.Thallium.Templating
{
    using Params  = Dictionary <string, object>;
    using IParams = IDictionary<string, object>;

    class Runtime : Rfc6570, IRfc6570
    {
        protected readonly StringBuilder Builder  = new StringBuilder();
        protected readonly StringBuilder Variable = new StringBuilder();

        public string Expand(string template, IParams parameters)
        {
            parameters = new Params(parameters, WordComparer.Instance);

            Builder.Clear();
            var inV = false;

            for (var i = 0; i < template.Length; ++i)
            {
                var c = template[i];

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
                        else     Builder .Append(c);
                        break;
                }
            }

            return Builder.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Expand(IParams keys)
        {
            Var[] vars;
            var spec = Rfc6570.spec(Variable, out vars);
            var first = true;

            for (var i = 0; i < vars.Length; ++i)
            {
                var var = vars[i];

                object obj;
                if (!keys.TryGetValue(var.key, out obj))
                {
                    continue;
                }

                var meta = Meta(obj);
                if (meta.IsNull || (!meta.IsString && meta.IsEmpty))
                {
                    continue;
                }

                var key   = var.key;
                var fst   = spec.fst;
                var sep   = spec.sep;
                var named = spec.named;
                var ifemp = spec.ifemp;
                var allow = spec.allow;

                Builder.Append(tos(first ? fst : sep));

                if (!meta.IsEnumerable || meta.IsString)
                {
                    if (var.prefix)
                    {
                        Scalar(Builder, named, ifemp, allow, key, var.length, meta);
                    }
                    else
                    {
                        Scalar(Builder, named, ifemp, allow, key, meta);
                    }
                }
                else if (var.explode)
                {
                    Explode(Builder, sep, named, ifemp, allow, key, meta);
                }
                else
                {
                    NoExplode(Builder, named, ifemp, allow, key, meta);
                }

                first = false;
            }
        }
    }
}
