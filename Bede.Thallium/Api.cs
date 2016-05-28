using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Reflection;

namespace Bede.Thallium
{
    using Introspection;
    using Content;

    using Handler    = HttpMessageHandler;
    using Formatters = MediaTypeFormatterCollection;

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

    /// <summary>
    /// API introspection interface
    /// </summary>
    public interface IIntrospect
    {
        /// <summary>
        /// Describe a method
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        Description Call(Type parent, MethodInfo method);
    }

    /// <summary>
    /// A static access point to the runtime client factory
    /// </summary>
    public static class Api
    {
        static readonly Factory Factory = new Factory();

        static object _new(Type type, params object[] args)
        {
            return Activator.CreateInstance(type, args);
        }

#pragma warning disable 612
        static IIntrospect _up(Introspector i)
#pragma warning restore 612
        {
            return null == i ? (IIntrospect) new Simple() : new Adapter(i);
        }

        /// <summary>
        /// Gets a fluently configurable introspector
        /// </summary>
        /// <returns></returns>
        public static IFluent Fluent()
        {
            return new Fluent();
        }

        /// <summary>
        /// Gets or Sets the <see cref="IImp" />
        /// </summary>
        public static IImp Imp
        {
            get { return Factory.Imp;  }
            set { Factory.Imp = value; }
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
        public static Type Emit(Type type, Type parent, IIntrospect introspector = null)
        {
            return Factory.Build(parent, type, introspector ?? new Simple());
        }

        /// <summary>
        /// Create a type for the given interface
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="introspector"></param>
        /// <returns></returns>
        [Obsolete("Use Api<T> instead")]
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
        [Obsolete("Use Api<T> instead")]
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
        [Obsolete("Use Api<T> instead")]
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
        [Obsolete("Use Api<T> instead")]
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
        [Obsolete("Use Api<T> instead")]
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
    public static class Api<TBase, T> where TBase : BaseClient
    {
        [Obsolete]
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
    public static class Api<T>
    {
        [Obsolete]
        public static Type Emit(Introspector introspector)
        {
            return Api<RestClient, T>.Emit(introspector);
        }

        public static Type Emit(IIntrospect introspector = null)
        {
            return Api<RestClient, T>.Emit(introspector);
        }

        [Obsolete("Use a more specific overload")]
        public static T New(params object[] args)
        {
            return Api<RestClient, T>.New(args);
        }

        public static T New(Uri uri)
        {
            return Api<RestClient, T>.New(uri);
        }

        public static T New(Uri uri, Handler handler)
        {
            return Api<RestClient, T>.New(uri, handler);
        }

        public static T New(Uri uri, Formatters formatters)
        {
            return Api<RestClient, T>.New(uri, formatters);
        }

        public static T New(Uri uri, Handler handler, Formatters formatters)
        {
            return Api<RestClient, T>.New(uri, handler, formatters);
        }
    }
}
