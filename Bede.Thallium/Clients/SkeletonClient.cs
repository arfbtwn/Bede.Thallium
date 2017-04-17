using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;

namespace Bede.Thallium.Clients
{
    using Belt;
    using Content;
    using Formatting;
    using Handlers;
    using Templating;

    using Params     = Dictionary<string, object>;
    using Handler    = HttpMessageHandler;
    using Formatter  = MediaTypeFormatter;
    using Formatters = MediaTypeFormatterCollection;
    using Token      = CancellationToken;

    static class Default
    {
        internal static Handler    Handler    => new ThrowOnFail();
        internal static Formatters Formatters => new Formatters(new Formatter[0])
                                                 {
                                                    new NewtonsoftJsonFormatter(),
                                                    new XmlMediaTypeFormatter(),
                                                    new FormUrlEncoder()
                                                 };
        internal static TimeSpan   Timeout    => TimeSpan.FromMinutes(2);

        internal static Compiled   Expander(string path) => new Compiled(path);
    }

    /// <summary>
    /// Basic client functionality
    /// </summary>
    public abstract class SkeletonClient
    {
        readonly Dictionary<string, Compiled> _expanders = new Dictionary<string, Compiled>();

        /// <summary>
        /// The URI used by the client
        /// </summary>
        public abstract Uri Uri { get; }

        /// <summary>
        /// The collection of default headers sent with each API request
        /// </summary>
        public Params Head { get; } = new Params();

        /// <summary>
        /// The collection of formatters for deserializing responses
        /// </summary>
        protected virtual Formatters Formatters => null;

        /// <summary>
        /// Gets a content builder
        /// </summary>
        protected virtual IContentBuilder ContentBuilder() => new ContentBuilder(Formatters ?? Default.Formatters);

        /// <summary>
        /// Gets a client
        /// </summary>
        /// <returns></returns>
        protected abstract HttpClient Client();

        /// <summary>
        /// Internal send method, included for flexibility
        /// </summary>
        /// <param name="message"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public virtual Task<HttpResponseMessage> SendAsync(HttpRequestMessage message, Token? token = null)
        {
            return Client().SendAsync(message, token ?? Token.None);
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
            var msg = new HttpRequestMessage(method, new Uri(Uri, Template(template, parameters)))
            {
                Content = body
            };

            Headers(msg, Head);
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
            return _expanders.Lookup(path, Default.Expander).Expand(parameters);
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
        [Obsolete]
        protected Task<HttpResponseMessage> Return(Task<HttpResponseMessage> message)
        {
            return message;
        }

        /// <summary>
        /// Unwrap incoming async message
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <returns></returns>
        protected async Task<T> Return<T>(Task<HttpResponseMessage> message)
        {
            var msg = await message.Caf();

            var task = msg.IsSuccessStatusCode ? Success<T>(msg) : Fail<T>(msg);

            return await task.Caf();
        }

        /// <summary>
        /// Handle success, no op
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [Obsolete]
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
            return msg.Content.ReadAsAsync<T>(Formatters ?? Default.Formatters);
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
        [Obsolete]
        protected virtual Task Fail(HttpResponseMessage msg)
        {
            return Task.FromResult(false);
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
