using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;

namespace Bede.Thallium.Clients
{
    partial class RestClient
    {
        /// <summary>
        /// Produce content
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="body"></param>
        /// <param name="mediaType"></param>
        /// <returns></returns>
        [Obsolete]
        protected virtual HttpContent Content<T>(T body, MediaTypeHeaderValue mediaType)
            where T : class
        {
            return ContentBuilder().Object(body).ContentType(mediaType).Build();
        }

        /// <summary>
        /// Produce content
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="body"></param>
        /// <param name="mediaType"></param>
        /// <returns></returns>
        [Obsolete]
        protected virtual HttpContent Content<T>(T? body, MediaTypeHeaderValue mediaType)
            where T : struct
        {
            return ContentBuilder().Struct(body).ContentType(mediaType).Build();
        }

        /// <summary>
        /// Supply a media type formatter for objects of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mediaType"></param>
        /// <returns></returns>
        [Obsolete]
        protected virtual MediaTypeFormatter Formatter<T>(MediaTypeHeaderValue mediaType)
        {
            var f = null == mediaType
                ? Formatters.First()
                : Formatters.FindWriter(typeof(T), mediaType);

            Assertion.HasFormatter(f, typeof(T), mediaType);

            return f;
        }
    }
}
