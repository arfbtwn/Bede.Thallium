﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Reflection;

namespace Bede.Thallium
{
    using Handler    = HttpMessageHandler;
    using Formatter  = MediaTypeFormatter;
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
    public delegate Call Introspector(MethodInfo method);

    /// <summary>
    /// A static access point to the client factory
    /// </summary>
    public static class Api
    {
        static readonly Factory Factory = new Factory();

        static object _new(Type type, params object[] args)
        {
            return Activator.CreateInstance(type, args);
        }

        /// <summary>
        /// Create a type for the given interface
        /// </summary>
        /// <param name="type"></param>
        /// <param name="parent"></param>
        /// <param name="introspector"></param>
        /// <returns></returns>
        public static Type Emit(Type type, Type parent = null, Introspector introspector = null)
        {
            return Factory.Build(parent ?? typeof(RestClient), type, introspector ?? I.Introspect);
        }

        /// <summary>
        /// Create a type for the given interface
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="introspector"></param>
        /// <returns></returns>
        public static Type Emit<T>(Introspector introspector = null)
        {
            return Emit(typeof(T), introspector: introspector);
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
        public static object New(Type type, params object[] args)
        {
            return New(type, null, args);
        }

        /// <summary>
        /// Construct the given interface
        /// </summary>
        /// <param name="type"></param>
        /// <param name="uri"></param>
        /// <returns></returns>
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
        public static T New<T>(params object[] args)
        {
            return (T) New(typeof(T), args);
        }
    }

#pragma warning disable 1591
    public static class Api<TBase, T> where TBase : RestClient
    {
        public static Type Emit(Introspector introspector = null)
        {
            return Api.Emit(typeof(T), typeof(TBase), introspector);
        }

        public static T New(params object[] args)
        {
            return (T) Api.New(typeof(T), typeof(TBase), args);
        }
    }

    public static class Api<T>
    {
        public static Type Emit(Introspector introspector = null)
        {
            return Api<RestClient, T>.Emit(introspector);
        }

        public static T New(params object[] args)
        {
            return Api<RestClient, T>.New(args);
        }
    }
}
