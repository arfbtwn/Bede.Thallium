using System;
using System.Net.Http;
using System.Net.Http.Formatting;

#pragma warning disable 1591

namespace Bede.Thallium.Clients
{
    using Formatters = MediaTypeFormatterCollection;

    /// <summary>
    /// Configuration object for <see cref="DelegatingClient"/> instances
    /// </summary>
    public interface IDelegatingConfig
    {
        /// <summary>
        /// The base URI that requests are sent to
        /// </summary>
        Uri Uri { get; }

        /// <summary>
        /// The set of media type formatters
        /// </summary>
        Formatters Formatters { get; }

        /// <summary>
        /// Returns an <see cref="HttpClient"/>
        /// </summary>
        /// <returns></returns>
        HttpClient Client();
    }

    public class DelegatingConfig : IDelegatingConfig, IDisposable
    {
        static HttpClient _() => new HttpClient(Default.Handler, true)
                                 {
                                     Timeout = Default.Timeout
                                 };

        readonly Lazy<HttpClient> _client = new Lazy<HttpClient>(_);

        public Uri Uri { get; set; }

        public Formatters Formatters { get; set; }

        public HttpClient Client() => _client.Value;

        public void Dispose()
        {
            if (_client.IsValueCreated)
            {
                _client.Value.Dispose();
            }
        }
    }

    /// <summary>
    /// A client that delegates <see cref="HttpClient"/> construction
    /// to another object
    /// </summary>
    public class DelegatingClient : SkeletonClient
    {
        readonly IDelegatingConfig _config;

        public DelegatingClient(IDelegatingConfig config)
        {
            _config = config;
        }

        public sealed override Uri Uri => _config.Uri;

        protected sealed override Formatters Formatters => _config.Formatters;
        protected sealed override HttpClient Client()   => _config.Client();
    }
}
