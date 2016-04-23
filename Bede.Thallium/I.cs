using System;
using System.Reflection;
using System.Reflection.Emit;

#pragma warning disable 1591

namespace Bede.Thallium
{
    using Belt;
    using Introspection;

    static class I
    {
        [Obsolete]
        internal static Descriptor Introspect(MethodInfo method)
        {
            return new AttributeInspection(method).V1();
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
                var br = @this.DefineLabel();
                @this.Emit(OpCodes.Dup);
                @this.Emit(OpCodes.Brfalse, br);

                @this.Emit(OpCodes.Newobj, nt.GetConstructor(new [] { source }));

                @this.MarkLabel(br);
                @this.Emit(OpCodes.Castclass, nt);
            }
        }
    }
}