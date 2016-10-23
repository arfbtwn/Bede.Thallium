using System;
using System.Net.Http;
using System.Net.Http.Formatting;

namespace Bede.Thallium.Clients
{
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
            : this(uri, handler, formatters, null)
        {
        }

        /// <summary>
        /// Custom construction
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="handler"></param>
        /// <param name="formatters"></param>
        /// <param name="timeout"></param>
        public RestClient(Uri uri, Handler handler, Formatters formatters, TimeSpan? timeout)
        {
            if (null == uri) throw new ArgumentNullException(nameof(uri));

            Uri        = uri;
            Handler    = handler;
            Formatters = formatters;
            Timeout    = timeout;
        }

#pragma warning disable 1591
        public    sealed override Uri        Uri        { get; }
        protected sealed override Handler    Handler    { get; }
        protected sealed override Formatters Formatters { get; }
        public    sealed override TimeSpan?  Timeout    { get; }
#pragma warning restore 1591
    }
}
