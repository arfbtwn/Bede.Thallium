using System;
using System.Text;

#pragma warning disable 1591

namespace Bede.Thallium.Templating
{
    struct Var
    {
        public static Var[] Parse(string var)
        {
            var splits = var.Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var vars   = new Var[splits.Length];

            for (var i = 0; i < splits.Length; ++i)
            {
                vars[i] = new Var(splits[i]);
            }

            return vars;
        }

        public Var(string spec)
        {
            prefix  = false;
            explode = false;

            var vb = new StringBuilder();
            var pb = new StringBuilder();

            for (var i = 0; i < spec.Length; ++i)
            {
                var c = spec[i];

                switch(c)
                {
                    case ':':
                        prefix = true;
                        break;
                    case '*':
                        explode = true;
                        break;
                    default:
                        if (prefix) pb.Append(c);
                        else        vb.Append(c);
                        break;
                }
            }

            length = prefix ? int.Parse(pb.ToString()) : -1;

            key = vb.ToString();
        }

        public readonly string key;
        public readonly bool   prefix;
        public readonly bool   explode;
        public readonly int    length;
    }
}