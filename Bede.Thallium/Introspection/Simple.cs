using System;
using System.Reflection;

#pragma warning disable 1591

namespace Bede.Thallium.Introspection
{
    public sealed class Simple : IIntrospect
    {
        public Description Call(Type type, MethodInfo method)
        {
            return new AttributeInspection(type, method).V2();
        }
    }
}