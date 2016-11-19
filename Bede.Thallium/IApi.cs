using System;
using System.Net.Http;
using System.Net.Http.Formatting;

namespace Bede.Thallium
{
    using Clients;
    using Introspection;

    using Handler    = HttpMessageHandler;
    using Formatters = MediaTypeFormatterCollection;

    /// <summary>
    /// An interface for a type that emits client types
    /// using the specified introspector
    /// </summary>
    public interface IApi
    {
        /// <summary>
        /// Use the specified introspector for API call description
        /// </summary>
        /// <param name="introspector"></param>
        /// <returns></returns>
        IApi Using(IIntrospect introspector);

        /// <summary>
        /// Emit a type implementing the given interface
        /// </summary>
        /// <param name="interface"></param>
        /// <returns></returns>
        Type Emit(Type @interface);
    }

    /// <summary>
    /// A generic version of <see cref="IApi" /> used to
    /// encode the type of the base client
    /// </summary>
    /// <typeparam name="TBase"></typeparam>
    public interface IApi<TBase> : IApi
    {
        /// <summary>
        /// Hide the base class <see cref="IApi.Using(IIntrospect)" />
        /// method
        /// </summary>
        /// <param name="introspector"></param>
        /// <returns></returns>
        new IApi<TBase> Using(IIntrospect introspector);
    }

    class _Api<TBase> : IApi<TBase>
    {
        readonly Factory _factory;

        IIntrospect _introspector = new Simple();

        public _Api(Factory factory)
        {
            _factory = factory;
        }

        public IApi<TBase> Using(IIntrospect introspector)
        {
            _introspector = introspector;
            return this;
        }

        IApi IApi.Using(IIntrospect introspector)
        {
            return Using(introspector);
        }

        public Type Emit(Type @interface)
        {
            return _factory.Build(typeof(TBase), @interface, _introspector);
        }
    }

#pragma warning disable 1591
    public static class ApiExtensions
    {
        public static Type Emit<T>(this IApi emitter)
        {
            return emitter.Emit(typeof(T));
        }

        static T _new<T>(Type type, params object[] args)
        {
            return (T) Activator.CreateInstance(type, args);
        }

        [Obsolete("Construction overload is not type-safe")]
        public static T New<T>(this IApi emitter, params object[] args)
        {
            return _new<T>(emitter.Emit<T>(), args);
        }

        [Obsolete("Construction overload is not type-safe")]
        public static TBase New<TBase>(this IApi<TBase> emitter, Type @interface, params object[] args)
        {
            return _new<TBase>(emitter.Emit(@interface), args);
        }

#pragma warning disable 618
        public static T New<T>(this IApi<RestClient> emitter, Uri        uri,
                                                              Handler    handler    = null,
                                                              Formatters formatters = null,
                                                              TimeSpan?  timeout    = null)
        {
            return emitter.New<T>(new object[] { uri, handler, formatters, timeout });
        }

        public static RestClient New(this IApi<RestClient> emitter, Type       @interface,
                                                                    Uri         uri,
                                                                    Handler     handler    = null,
                                                                    Formatters  formatters = null,
                                                                    TimeSpan?   timeout    = null)
        {
            return emitter.New(@interface, new object[] { uri, handler, formatters, timeout });
        }

        public static T New<T>(this IApi<DynamicClient> emitter, IClientConfig config)
        {
            return emitter.New<T>(new object[] { config });
        }

        public static DynamicClient New(this IApi<DynamicClient> emitter, Type @interface, IClientConfig config)
        {
            return emitter.New(@interface, new object[] { config });
        }

        public static T New<T>(this IApi<DelegatingClient> emitter, IDelegatingConfig config)
        {
            return emitter.New<T>(new object[] { config });
        }

        public static DelegatingClient New(this IApi<DelegatingClient> emitter, Type @interface, IDelegatingConfig config)
        {
            return emitter.New(@interface, new object[] { config });
        }
#pragma warning restore 618
    }
#pragma warning restore 1591
}