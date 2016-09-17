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
    }

    public class DynamicClient : BaseClient
    {
        readonly IClientConfig _config;

        public DynamicClient(IClientConfig config)
        {
            _config = config;
        }

        public    override Uri        Uri        => _config.Uri;
        protected override Handler    Handler    => _config.Handler;
        protected override Formatters Formatters => _config.Formatters;
    }
}