using System;
using System.Net.Http;
using System.Net.Http.Formatting;

#pragma warning disable 618, 1591

namespace Bede.Thallium
{
    using Clients;

    using Handler    = HttpMessageHandler;
    using Formatters = MediaTypeFormatterCollection;

    public static partial class ApiExtensions
    {
        public static T New<T>(this IApi<RestClient> emitter, Uri        uri,
                                                              Handler    handler,
                                                              Formatters formatters)
        {
            return emitter.New<T>(new object[] { uri, handler, formatters });
        }

        public static RestClient New(this IApi<RestClient> emitter, Type       @interface,
                                                                    Uri         uri,
                                                                    Handler     handler,
                                                                    Formatters  formatters)
        {
            return emitter.New(@interface, new object[] { uri, handler, formatters });
        }
    }
}
