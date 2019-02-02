using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Reflection.Emit;

#pragma warning disable 1591

namespace Bede.Thallium
{
    using Belt;
    using Introspection;

    using Call = Tuple<
                       HttpMethod,
                       string,
                       Dictionary<ParameterInfo, string>,
                       ParameterInfo,
                       Dictionary<string, string[]>
                      >;

    static class I
    {
        [Obsolete]
        internal static Call Introspect(MethodInfo method)
        {
            return new AttributeInspection(method).V1();
        }

        internal static IEnumerable<Type> Declaring(this Type type)
        {
            while(null != type.DeclaringType)
            {
                yield return type.DeclaringType;

                type = type.DeclaringType;
            }
        }

        internal static string Concrete(this Type type)
        {
            return type.IsInterface && 'I' == type.Name[0] ? type.Name.Substring(1)
                                                           : type.Name;
        }

        internal static string Derived(this Type parent, Type target)
        {
            var list = new List<string> { parent.Namespace };

            list.AddRange(parent.Declaring().Reverse().Select(x => x.Name));

            list.Add(parent.Name + target.Concrete());

            return string.Join(".", list.Except(new string[] { null }));
        }

        internal static void EmitCall(this ILGenerator @this, MethodInfo method)
        {
            @this.Emit(method.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, method);
        }

        internal static void EmitString(this ILGenerator @this, string value)
        {
            if (null == value)
            {
                @this.Emit(OpCodes.Ldnull);
            }
            else
            {
                @this.Emit(OpCodes.Ldstr, value);
            }
        }

        internal static void EmitNullConversion(this ILGenerator @this, Type source)
        {
            var nt = source.AsNullAssignable();

            if (nt != source)
            {
                @this.Emit(OpCodes.Newobj, nt.GetConstructor(new [] { source }));
            }
        }
    }
}