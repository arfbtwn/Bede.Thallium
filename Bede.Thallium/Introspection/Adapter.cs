using System;
using System.Reflection;

#pragma warning disable 612

namespace Bede.Thallium.Introspection
{
    sealed class Adapter : IIntrospect
    {
        readonly Introspector _inner;

        public Adapter(Introspector inner)
        {
            _inner = inner;
        }

        public Description Call(Type parent, MethodInfo method)
        {
            return _inner(method);
        }
    }
}