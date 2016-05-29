using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using Bede.Thallium.Introspection;

namespace Bede.Thallium
{
    using Handler    = HttpMessageHandler;
    using Formatters = MediaTypeFormatterCollection;

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
    public static class EmitterExtensions
    {
        public static Type Emit<T>(this IApi emitter)
        {
            return emitter.Emit(typeof(T));
        }

        static T _new<T>(Type type, params object[] args)
        {
            return (T) Activator.CreateInstance(type, args);
        }

#pragma warning disable 618
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

        public static T New<T>(this IApi<RestClient> emitter, Uri        uri,
                                                                        Handler    handler    = null,
                                                                        Formatters formatters = null)
        {
            return emitter.New<T>(new object[] { uri, handler, formatters });
        }

        public static RestClient New(this IApi<RestClient> emitter, Type      @interface,
                                                                              Uri        uri,
                                                                              Handler    handler    = null,
                                                                              Formatters formatters = null)
        {
            return emitter.New(@interface, new object[] { uri, handler, formatters });
        }
#pragma warning restore 618
    }
#pragma warning restore 1591
}