using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Reflection.Emit;

namespace Bede.Thallium
{
    using Belt;

    using Call = Tuple<
                       HttpMethod,
                       string,
                       Dictionary<ParameterInfo, string>,
                       ParameterInfo,
                       Dictionary<string, string[]>
                      >;

    static class I
    {
        internal static Call Introspect(MethodInfo method)
        {
            var verb    = method.GetCustomAttribute<VerbAttribute>();

            if (null == verb) throw new ArgumentException("No verb attribute", "method");

            var nest = method.Cons(Declaring(method))
                             .Reverse()
                             .ToArray();

            var route = nest.SelectMany(x => x.GetAttributeValue((RouteAttribute a) => a.Route))
                            .Except(new [] { string.Empty, null })
                            .ToArray();

            var body    = method.GetParameters().FirstOrDefault(IsBody);
            var headers = method.GetParameters().Where         (IsHeader).ToArray();
            var @static = nest.SelectMany(x => x.GetCustomAttributes<HeaderAttribute>())
                              .ToArray();

            return new Descriptor
            {
                Verb     = verb.Verb,
                Template = string.Join("/", route).TrimStart('/'),
                Body     = body,
                Headers  = headers.ToDictionary(x => x, HeaderName),
                Static   = @static.ToLookup(x => x.Name, x => x.Value)
                                  .ToDictionary(x => x.Key, group => group.ToArray())
            };
        }

        static bool IsBody(ParameterInfo x)
        {
            return x.Name == "body" || x.IsDefined(typeof(BodyAttribute));
        }

        static bool IsHeader(ParameterInfo x)
        {
            return x.IsDefined(typeof(HeaderAttribute));
        }

        static string HeaderName(ParameterInfo x)
        {
            return x.GetAttributeValue((HeaderAttribute a) => a.Name).First();
        }

        static IEnumerable<MemberInfo> Declaring(MemberInfo member)
        {
            if (null == member) yield break;

            while(member.DeclaringType != null)
            {
                yield return member.DeclaringType;

                member = member.DeclaringType;
            }
        }

        internal static void EmitCall(this ILGenerator @this, MethodInfo method)
        {
            @this.Emit(method.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, method);
        }
    }
}