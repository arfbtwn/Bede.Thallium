using System;
using System.Net.Http;
using System.Net.Http.Formatting;

#pragma warning disable 1591

namespace Bede.Thallium.Clients
{
    using Handler    = HttpMessageHandler;
    using Formatters = MediaTypeFormatterCollection;

    public interface IClientConfig
    {
        Uri        Uri        { get; }
        Handler    Handler    { get; }
        Formatters Formatters { get; }
        TimeSpan?  Timeout    { get; }
    }

    public class DynamicClient : BaseClient
    {
        readonly IClientConfig _config;

        public DynamicClient(IClientConfig config)
        {
            _config = config;
        }

        public    sealed override Uri        Uri        => _config.Uri;
        protected sealed override Handler    Handler    => _config.Handler;
        protected sealed override Formatters Formatters => _config.Formatters;
        public    sealed override TimeSpan?  Timeout    => _config.Timeout;
    }
}