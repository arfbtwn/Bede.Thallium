using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

#pragma warning disable 1591

namespace Bede.Thallium.Introspection
{
    public class Fluent<T> : IIntrospect
    {
        readonly Dictionary<MethodInfo, Description> _map = new Dictionary<MethodInfo, Description>();

        IIntrospect _default;

        public Fluent<T> Default(IIntrospect introspector)
        {
            _default = introspector;

            return this;
        }

        public Fluent<T> With<TV>(Expression<Func<T, TV>> expr, Description description)
        {
            var me = (MethodCallExpression) expr.Body;
            var mi = me.Method;

            _map[mi] = description;

            return this;
        }

        public Description Call(Type parent, MethodInfo method)
        {
            if (null == method) throw new ArgumentNullException("method");

            Description mapped;
            if (_map.TryGetValue(method, out mapped))
            {
                return mapped;
            }

            if (null != _default)
            {
                return _map[method] = _default.Call(parent, method);
            }

            throw new ArgumentException("Unmapped method: " + method.Name, "method");
        }
    }
}