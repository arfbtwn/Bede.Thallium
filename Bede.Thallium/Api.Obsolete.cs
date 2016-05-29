using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;

#pragma warning disable 612

namespace Bede.Thallium
{
    using Introspection;

    using Call = Tuple<
                       HttpMethod,
                       string,
                       Dictionary<ParameterInfo, string>,
                       ParameterInfo,
                       Dictionary<string, string[]>
                      >;

    /// <summary>
    /// API introspection delegate
    /// </summary>
    /// <remarks>
    /// This is the point of contention for the Thallium API, it
    /// would be nice to handle properties, but should we always
    /// assume that they are headers? What else could they be?
    /// Events would be a nice analog for web hooks so we might
    /// expand this concept into a full interface later
    /// </remarks>
    /// <param name="method"></param>
    /// <returns></returns>
    [Obsolete]
    public delegate Call Introspector(MethodInfo method);

    public static partial class Api
    {
        static object _new(Type type, params object[] args)
        {
            return Activator.CreateInstance(type, args);
        }

        static IIntrospect _up(Introspector i)
        {
            return null == i ? (IIntrospect) new Simple() : new Adapter(i);
        }

        /// <summary>
        /// Create a type for the given interface
        /// </summary>
        /// <param name="type"></param>
        /// <param name="parent"></param>
        /// <param name="introspector"></param>
        /// <returns></returns>
        [Obsolete]
        public static Type Emit(Type type, Type parent, Introspector introspector)
        {
            return Factory.Build(parent, type, _up(introspector));
        }

        /// <summary>
        /// Create a type for the given interface
        /// </summary>
        /// <param name="type"></param>
        /// <param name="parent"></param>
        /// <param name="introspector"></param>
        /// <returns></returns>
        [Obsolete]
        public static Type Emit(Type type, Type parent = null, IIntrospect introspector = null)
        {
            return Factory.Build(parent ?? typeof(RestClient), type, introspector ?? new Simple());
        }

        /// <summary>
        /// Create a type for the given interface
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="introspector"></param>
        /// <returns></returns>
        [Obsolete]
        public static Type Emit<T>(Introspector introspector)
        {
            return Emit(typeof(T), typeof(RestClient), introspector);
        }

        /// <summary>
        /// Create a type for the given interface
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="introspector"></param>
        /// <returns></returns>
        [Obsolete]
        public static Type Emit<T>(IIntrospect introspector = null)
        {
            return Emit(typeof(T), typeof(RestClient), introspector);
        }

        /// <summary>
        /// Construct the given interface
        /// </summary>
        /// <param name="type"></param>
        /// <param name="parent"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [Obsolete]
        public static object New(Type type, Type parent, params object[] args)
        {
            return _new(Emit(type, parent), args);
        }

        /// <summary>
        /// Construct the given interface
        /// </summary>
        /// <param name="type"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [Obsolete]
        public static object New(Type type, params object[] args)
        {
            return New(type, typeof(RestClient), args);
        }

        /// <summary>
        /// Construct the given interface
        /// </summary>
        /// <param name="type"></param>
        /// <param name="uri"></param>
        /// <returns></returns>
        [Obsolete]
        public static object New(Type type, Uri uri)
        {
            return New(type, new object[] { uri });
        }

        /// <summary>
        /// Construct the given interface
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        /// <returns></returns>
        [Obsolete]
        public static T New<T>(params object[] args)
        {
            return (T) New(typeof(T), args);
        }
    }

#pragma warning disable 1591
    /// <summary>
    /// A static factory using a custom base class
    /// </summary>
    /// <typeparam name="TBase"></typeparam>
    /// <typeparam name="T"></typeparam>
    [Obsolete("Use Api.Client<T>() instead")]
    public static class Api<TBase, T> where TBase : BaseClient
    {
        public static Type Emit(Introspector introspector)
        {
            return Api.Emit(typeof(T), typeof(TBase), introspector);
        }

        public static Type Emit(IIntrospect introspector = null)
        {
            return Api.Emit(typeof(T), typeof(TBase), introspector);
        }

        public static T New(params object[] args)
        {
            return (T) Api.New(typeof(T), typeof(TBase), args);
        }
    }

    /// <summary>
    /// A static factory using <see cref="RestClient" /> as
    /// its base class and exposing appropriate construction
    /// signatures
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Obsolete("Use Api.RestClient() instead")]
    public static class Api<T>
    {
        public static Type Emit(Introspector introspector)
        {
            return Api<RestClient, T>.Emit(introspector);
        }

        public static Type Emit(IIntrospect introspector = null)
        {
            return Api<RestClient, T>.Emit(introspector);
        }

        public static T New(params object[] args)
        {
            return Api<RestClient, T>.New(args);
        }
    }
}
