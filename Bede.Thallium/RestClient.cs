using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;

namespace Bede.Thallium
{
    using Belt;
    using Content;

    using Param      = KeyValuePair<string, object>;
    using Params     = Dictionary  <string, object>;

    using Handler    = HttpMessageHandler;
    using Formatters = MediaTypeFormatterCollection;

    using Token      = CancellationToken;

    /// <summary>
    /// Base class for dynamic rest clients, subclass it and do the legwork yourself
    /// or generate a client from an interface definition
    /// </summary>
    public partial class RestClient
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
        /// Gets a content builder
        /// </summary>
        protected virtual IContentBuilder ContentBuilder()
        {
            return new ContentBuilder(_formatters);
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
        /// <param name="token"></param>
        /// <returns></returns>
        public virtual async Task<HttpResponseMessage> SendAsync(HttpRequestMessage message, Token? token = null)
        {
            using (var client = Client())
            {
                return await client.SendAsync(message, token ?? Token.None).Caf();
            }
        }

        /// <summary>
        /// Send an HTTP request message
        /// </summary>
        /// <remarks>
        /// The principle send method, performs template expansion
        /// and header packaging
        /// </remarks>
        /// <param name="method"></param>
        /// <param name="template"></param>
        /// <param name="parameters"></param>
        /// <param name="headers"></param>
        /// <param name="body"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        protected Task<HttpResponseMessage> SendAsync(HttpMethod  method,
                                                      string      template,
                                                      Params      parameters,
                                                      Params      headers,
                                                      HttpContent body,
                                                      Token?      token = null)
        {
            var msg = new HttpRequestMessage(method, new Uri(_uri, Template(template, parameters)))
            {
                Content = body
            };

            Headers(msg, _headers);
            Headers(msg, headers);

            return SendAsync(msg, token);
        }

        /// <summary>
        /// Send an HTTP message with no content
        /// </summary>
        /// <param name="method"></param>
        /// <param name="template"></param>
        /// <param name="parameters"></param>
        /// <param name="headers"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<HttpResponseMessage> SendAsync(HttpMethod method,
                                                   string     template,
                                                   Params     parameters = null,
                                                   Params     headers    = null,
                                                   Token?     token      = null)
        {
            return SendAsync(method, template, parameters, headers, (HttpContent) null, token);
        }

        /// <summary>
        /// Send an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        /// <param name="template"></param>
        /// <param name="parameters"></param>
        /// <param name="body"></param>
        /// <param name="headers"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public virtual Task<HttpResponseMessage> SendAsync<T>(HttpMethod method,
                                                              string     template,
                                                              Params     parameters = null,
                                                              T          body       = null,
                                                              Params     headers    = null,
                                                              Token?     token      = null)
            where T : class
        {
            var type = Rfc2616.ContentType(headers);

            var content = ContentBuilder().Object(body).ContentType(type).Build();

            return SendAsync(method, template, parameters, headers, content, token);
        }

        /// <summary>
        /// Send an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        /// <param name="template"></param>
        /// <param name="parameters"></param>
        /// <param name="body"></param>
        /// <param name="headers"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public virtual Task<HttpResponseMessage> SendAsync<T>(HttpMethod method,
                                                              string     template,
                                                              Params     parameters = null,
                                                              T?         body       = null,
                                                              Params     headers    = null,
                                                              Token?     token      = null)
            where T : struct
        {
            var type = Rfc2616.ContentType(headers);

            var content = ContentBuilder().Struct(body).ContentType(type).Build();

            return SendAsync(method, template, parameters, headers, content, token);
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
        protected virtual Task Success(HttpResponseMessage msg)
        {
            return Task.FromResult(true);
        }

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
        /// keys inserted into its data collection defined by <see cref="ExceptionKeys"/>
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
                        { ExceptionKeys.Verb,       req.Method.Method },
                        { ExceptionKeys.Version,    req.Version       },
                        { ExceptionKeys.RequestUri, req.RequestUri    },
                        { ExceptionKeys.Code,       msg.StatusCode    },
                        { ExceptionKeys.Content,    str               }
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
