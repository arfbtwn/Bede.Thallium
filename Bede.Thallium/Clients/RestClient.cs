using System;
using System.Net.Http;
using System.Net.Http.Formatting;

namespace Bede.Thallium.Clients
{
    using Handlers;

    using Handler    = HttpMessageHandler;
    using Formatters = MediaTypeFormatterCollection;

    /// <summary>
    /// Base class for dynamic rest clients, subclass it and do the legwork yourself
    /// or generate a client from an interface definition
    /// </summary>
    public partial class RestClient : BaseClient
    {
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
            if (null == uri) throw new ArgumentNullException(nameof(uri));

            Uri        = uri;
            Handler    = handler    ?? new ThrowOnFail();
            Formatters = formatters ?? Default;
        }

#pragma warning disable 1591
        public    override Uri        Uri        { get; }
        protected override Handler    Handler    { get; }
        protected override Formatters Formatters { get; }

        protected override HttpClient Client() => new HttpClient(Handler, false);
#pragma warning restore 1591
    }
}
