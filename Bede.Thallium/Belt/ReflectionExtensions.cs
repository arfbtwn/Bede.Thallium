using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Bede.Thallium.Belt
{
    using MethodMap = ConcurrentDictionary<Type, ISet<MethodBase>>;

    static class ReflectionExtensions
    {
        static readonly MethodBase[] Null = { null };

        static readonly MethodMap Properties = new MethodMap();
        static readonly MethodMap Events     = new MethodMap();

        public static bool IsProperty(this MethodBase method)
        {
            return Properties.GetOrAdd(method.DeclaringType, PropertyMethods)
                .Contains(method);
        }

        public static bool IsEvent(this MethodBase method)
        {
            return Events.GetOrAdd(method.DeclaringType, EventMethods)
                .Contains(method);
        }

        public static bool IsMethod(this MethodBase method)
        {
            return !IsEvent(method) && !IsProperty(method);
        }

        public static Type AsNullAssignable(this Type type)
        {
            return IsNullAssignable(type) ? type : typeof(Nullable<>).MakeGenericType(type);
        }

        public static bool IsNullAssignable(this Type type)
        {
            return !type.IsValueType || IsNullable(type);
        }

        public static bool IsNullable(this Type type)
        {
            return HasGenericDefinition(type, typeof(Nullable<>));
        }

        public static bool IsNullable(this Type type, out Type element)
        {
            if (!IsNullable(type))
            {
                element = default(Type);

                return false;
            }

            element = type.GetGenericArguments()[0];

            return true;
        }

        public static bool HasGenericDefinition(this Type type, Type generic)
        {
            return type.Hierarchy()
                .SelectMany(t => t.Cons(t.GetInterfaces()))
                .Any(x => x.IsGenericType && generic == x.GetGenericTypeDefinition());
        }

        public static IEnumerable<Type> Hierarchy(this Type type)
        {
            while(null != type)
            {
                yield return type;

                type = type.BaseType;
            }
        }

        public static IEnumerable<T> GetAttributeValue<TAttr, T>(this ICustomAttributeProvider @this, Expression<Func<TAttr, T>> access)
        {
            return GetAttributeValue(@this, false, access);
        }

        public static IEnumerable<T> GetAttributeValue<TAttr, T>(this ICustomAttributeProvider @this, bool inherit, Expression<Func<TAttr, T>> access)
        {
            var func = new Lazy<Func<TAttr, T>>(access.Compile);

            return @this.GetCustomAttributes(inherit)
                .OfType<TAttr>()
                .Select(x => func.Value(x));
        }

        static ISet<MethodBase> PropertyMethods(Type type)
        {
            var methods = type
                .GetProperties()
                .SelectMany   (x => new[] { x.GetMethod, x.SetMethod })
                .Except       (Null);
            return new HashSet<MethodBase>(methods);
        }

        static ISet<MethodBase> EventMethods(Type type)
        {
            var methods = type
                .GetEvents ()
                .SelectMany(x => new[] { x.AddMethod, x.RemoveMethod })
                .Except    (Null);
            return new HashSet<MethodBase>(methods);
        }
    }
}