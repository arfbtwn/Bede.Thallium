using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;

#pragma warning disable 1591

namespace Bede.Thallium.Introspection
{
    using Belt;

    public class AttributeInspection
    {
        readonly Type       _p;
        readonly MethodInfo _i;

        public AttributeInspection(MethodInfo method)
        {
            _p = typeof(object);
            _i = method;
        }

        public AttributeInspection(Type parent, MethodInfo method)
        {
            _p = parent;
            _i = method;
        }

        public bool Trim { get; set; }

        protected Type P
        {
            get { return _p; }
        }

        protected MethodInfo I
        {
            get { return _i; }
        }

        protected virtual VerbAttribute Verb
        {
            get { return _i.GetCustomAttribute<VerbAttribute>(); }
        }

        protected virtual MemberInfo[] Nest
        {
            get
            {
                var p = _p.Cons(Declaring(_p))
                          .Reverse();

                var i = _i.Cons(Declaring(_i))
                          .Reverse();

                return p.Union(i).ToArray();
            }
        }

        protected virtual string[] Route
        {
            get
            {
                var pre = Nest.SelectMany(x => x.GetAttributeValue((RouteAttribute a) => a.Route))
                              .Except(new [] { string.Empty, null });

                return Trim ? pre.Select(x => x.TrimStart('/')).ToArray()
                            : pre.ToArray();
            }
        }

        protected virtual ParameterInfo[] Body
        {
            get { return _i.GetParameters().Where(IsBody).ToArray(); }
        }

        protected virtual ParameterInfo[] Headers
        {
            get { return _i.GetParameters().Where(IsHeader).ToArray(); }
        }

        protected virtual HeaderAttribute[] Static
        {
            get
            {
                return Nest.SelectMany(x => x.GetCustomAttributes<HeaderAttribute>())
                           .ToArray();
            }
        }

        public Description V2()
        {
            return new Description
            {
                Verb     = Verb?.Verb,
                Template = string.Join("/", Route),
                Body     = Body.ToDictionary(x => x, ContentInfo),
                Subtype  = Subtype(),
                Boundary = Boundary(),
                Headers  = Headers.ToDictionary(x => x, HeaderName),
                Static   = Static.ToLookup(x => x.Name, x => x.Value)
                                 .ToDictionary(x => x.Key, group => group.ToArray())
            };
        }

        protected virtual string Subtype()
        {
            return _i.GetAttributeValue<MultipartAttribute, string>(x => x.Subtype).FirstOrDefault();
        }

        protected virtual string Boundary()
        {
            return _i.GetAttributeValue<MultipartAttribute, string>(x => x.Boundary).FirstOrDefault();
        }

        protected virtual ContentDescription ContentInfo(ParameterInfo x)
        {
            var ct = ContentType(x);
            var cd = Disposition(x);

            return new ContentDescription
            {
                Type        = null == ct ? null : ct.ToString(),
                Disposition = null == cd ? null : cd.ToString(),

                SetName     = null != cd && string.IsNullOrWhiteSpace(cd.Name),
                SetFileName = null != cd && string.IsNullOrWhiteSpace(cd.FileName)
            };
        }

        protected virtual MediaTypeHeaderValue ContentType(ParameterInfo x)
        {
            var p = x.GetAttributeValue((TypeAttribute y) => y.Value).FirstOrDefault();

            return Rfc2616.ContentType(p);
        }

        protected virtual ContentDispositionHeaderValue Disposition(ParameterInfo x)
        {
            var p = x.GetAttributeValue((DispositionAttribute y) => y.Value).FirstOrDefault();

            return Rfc2616.Disposition(p);
        }

        protected virtual bool IsBody(ParameterInfo x)
        {
            return x.Name.StartsWith("body") || x.IsDefined(typeof(BodyAttribute));
        }

        protected virtual bool IsHeader(ParameterInfo x)
        {
            return x.IsDefined(typeof(HeaderAttribute));
        }

        protected virtual string HeaderName(ParameterInfo x)
        {
            return x.GetAttributeValue((HeaderAttribute a) => a.Name).First();
        }

        protected virtual IEnumerable<MemberInfo> Declaring(MemberInfo member)
        {
            if (null == member) yield break;

            while(member.DeclaringType != null)
            {
                yield return member.DeclaringType;

                member = member.DeclaringType;
            }
        }
    }
}
