using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;

namespace Bede.Thallium
{
    using Belt;

    using Param      = KeyValuePair<string, object>;
    using Params     = Dictionary  <string, object>;

    using Handler    = HttpMessageHandler;
    using Formatter  = MediaTypeFormatter;
    using Formatters = MediaTypeFormatterCollection;

    delegate HttpContent Content<in T>(T body, MediaTypeHeaderValue media);

    /// <summary>
    /// Base class for dynamic rest clients, subclass it and do the legwork yourself
    /// or generate a client from an interface definition
    /// </summary>
    public class RestClient
    {
        static readonly MediaTypeFormatterCollection Formatters;

        static RestClient()
        {
            Formatters = new MediaTypeFormatterCollection { new FormUrlEncoder() };
        }

        readonly Uri        _uri;
        readonly Handler    _handler;
        readonly Formatters _formatters;
        readonly Params     _headers;

        /// <summary>
        /// Default construction
        /// </summary>
        /// <param name="uri"></param>
        public RestClient(Uri uri) : this(uri, (Handler) null) { }

        /// <summary>
        /// Construction with a specific handler and default formatter collection
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="handler"></param>
        public RestClient(Uri uri, Handler handler)
            : this(uri, handler, null) { }

        /// <summary>
        /// Construction with a list of formatters
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="formatters"></param>
        public RestClient(Uri uri, Formatters formatters)
            : this(uri, null, formatters) { }

        /// <summary>
        /// Custom construction
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="handler"></param>
        /// <param name="formatters"></param>
        public RestClient(Uri uri, Handler handler, Formatters formatters)
        {
            if (null == uri) throw new ArgumentNullException("uri");

            _uri        = uri;
            _handler    = handler    ?? new HttpClientHandler();
            _formatters = formatters ?? Formatters;
            _headers    = new Params();
        }

        /// <summary>
        /// The collection of default headers sent with each API request
        /// </summary>
        public Params Head
        {
            get { return _headers; }
        }

        /// <summary>
        /// Construct a client
        /// </summary>
        /// <returns></returns>
        protected virtual HttpClient Client()
        {
            return new HttpClient(_handler, false);
        }

        /// <summary>
        /// Internal send method, included for flexibility
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage message)
        {
            using (var client = Client())
            {
                return await client.SendAsync(message).Caf();
            }
        }

        /// <summary>
        /// Send an HTTP message
        /// </summary>
        /// <param name="method"></param>
        /// <param name="template"></param>
        /// <param name="parameters"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public Task<HttpResponseMessage> SendAsync(HttpMethod method,
                                                   string     template,
                                                   Params     parameters = null,
                                                   Params     headers    = null)
        {
            return SendAsync<object>(method, template, null, parameters, headers);
        }

        /// <summary>
        /// Send an HTTP message
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        /// <param name="template"></param>
        /// <param name="parameters"></param>
        /// <param name="body"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public Task<HttpResponseMessage> SendAsync<T>(HttpMethod method,
                                                      string     template,
                                                      Params     parameters = null,
                                                      T          body       = null,
                                                      Params     headers    = null)
            where T : class
        {
            var msg = Msg(method, template, parameters, body, headers, Content);

            return SendAsync(msg);
        }

        /// <summary>
        /// Send an HTTP message
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        /// <param name="template"></param>
        /// <param name="parameters"></param>
        /// <param name="body"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public Task<HttpResponseMessage> SendAsync<T>(HttpMethod method,
                                                      string     template,
                                                      Params     parameters = null,
                                                      T?         body       = null,
                                                      Params     headers    = null)
            where T : struct
        {
            var msg = Msg(method, template, parameters, body, headers, Content);

            return SendAsync(msg);
        }

        HttpRequestMessage Msg<T>(HttpMethod method,
                                  string     template,
                                  Params     parameters,
                                  T          body,
                                  Params     headers,
                                  Content<T> content)
        {
            var msg = new HttpRequestMessage(method, new Uri(_uri, Template(template, parameters)))
            {
                Content = content(body, Rfc2616.ContentType(headers))
            };

            Headers(msg, _headers);
            Headers(msg, headers);

            return msg;
        }

        /// <summary>
        /// Perform path and query string substitution
        /// </summary>
        /// <param name="path"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected virtual string Template(string path, Params parameters)
        {
            return new Rfc6750().Expand(path, parameters);
        }

        /// <summary>
        /// Produce a header collection
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="headers"></param>
        protected virtual void Headers(HttpRequestMessage msg, Params headers = null)
        {
            if (null == headers) return;

            Rfc2616.Populate(msg, headers);
        }

        /// <summary>
        /// Produce content
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="body"></param>
        /// <param name="mediaType"></param>
        /// <returns></returns>
        protected virtual HttpContent Content<T>(T body, MediaTypeHeaderValue mediaType)
            where T : class
        {
            var f = null == body ? null : Formatter<T>(mediaType);
            var m = null == f    ? null : mediaType ?? f.SupportedMediaTypes.First();

            return null == body
                ? null
                : new ObjectContent(typeof(T), body, f, m);
        }

        /// <summary>
        /// Produce content
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="body"></param>
        /// <param name="mediaType"></param>
        /// <returns></returns>
        protected virtual HttpContent Content<T>(T? body, MediaTypeHeaderValue mediaType)
            where T : struct
        {
            var f = null == body ? null : Formatter<T>(mediaType);
            var m = null == f    ? null : mediaType ?? f.SupportedMediaTypes.First();

            return null == body
                ? null
                : new ObjectContent(typeof(T), body, f, m);
        }

        /// <summary>
        /// Supply a media type formatter for objects of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mediaType"></param>
        /// <returns></returns>
        protected virtual Formatter Formatter<T>(MediaTypeHeaderValue mediaType)
        {
            var f = null == mediaType
                ? _formatters.First()
                : _formatters.FindWriter(typeof(T), mediaType);

            if (null == f)
            {
                throw new InvalidOperationException("No media type formatter for " + typeof(T).Name + " as " + mediaType);
            }

            return f;
        }

        /// <summary>
        /// Unwrap incoming async message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected async Task<HttpResponseMessage> Return(Task<HttpResponseMessage> message)
        {
            var msg = await message.Caf();

            if (msg.IsSuccessStatusCode)
            {
                await Success(msg).Caf();
            }
            else
            {
                await Fail(msg).Caf();
            }

            return msg;
        }

        /// <summary>
        /// Unwrap incoming async message
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <returns></returns>
        protected async Task<T> Return<T>(Task<HttpResponseMessage> message)
        {
            var msg = await Return(message).Caf();

            var task = msg.IsSuccessStatusCode
                ? Success<T>(msg)
                : Fail<T>(msg);

            return await task.Caf();
        }

        /// <summary>
        /// Handle success, no op
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
#pragma warning disable 1998
        protected virtual async Task Success(HttpResponseMessage msg) { }
#pragma warning restore 1998

        /// <summary>
        /// Handle success
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="msg"></param>
        /// <returns></returns>
        protected virtual Task<T> Success<T>(HttpResponseMessage msg)
        {
            return msg.Content.ReadAsAsync<T>(_formatters);
        }

        /// <summary>
        /// Handle failure
        /// </summary>
        /// <remarks>
        /// The default implementation throws an <see cref="HttpRequestException" />
        /// with a request summary and response content for its message and a set of
        /// keys inserted into its data collection defined by <see cref="ThalliumExceptionKeys"/>
        /// </remarks>
        /// <param name="msg"></param>
        /// <returns></returns>
        protected virtual async Task Fail(HttpResponseMessage msg)
        {
            using (var mem = new MemoryStream())
            using (var red = new StreamReader(mem, true))
            {
                await msg.Content.CopyToAsync(mem).Caf();

                mem.Position = 0;

                var str = await red.ReadToEndAsync().Caf();
                var req = msg.RequestMessage;

                var err = string.Format("{0} {1} HTTP/{2}\n" +
                                        "host: {3}:{4}\n" +
                                        "code: {5:D} => {5}\n" +
                                        "{6}",
                                        req.Method,
                                        req.RequestUri.PathAndQuery,
                                        req.Version,
                                        req.RequestUri.Host,
                                        req.RequestUri.Port,
                                        msg.StatusCode,
                                        str);
                throw new HttpRequestException(err)
                {
                    Data = {
                        { ThalliumExceptionKeys.Verb,       req.Method.Method },
                        { ThalliumExceptionKeys.Version,    req.Version       },
                        { ThalliumExceptionKeys.RequestUri, req.RequestUri    },
                        { ThalliumExceptionKeys.Code,       msg.StatusCode    },
                        { ThalliumExceptionKeys.Content,    str               }
                    }
                };
            }
        }

        /// <summary>
        /// Handle failure for return type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="msg"></param>
        /// <returns></returns>
        protected virtual Task<T> Fail<T>(HttpResponseMessage msg)
        {
            return Task.FromResult(default(T));
        }
    }
}
