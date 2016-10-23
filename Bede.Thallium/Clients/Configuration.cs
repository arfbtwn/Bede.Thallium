using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Formatting;
using Bede.Thallium.Handlers;

#pragma warning disable 1591

namespace Bede.Thallium.Clients
{
    using Handler    = HttpMessageHandler;
    using Formatters = MediaTypeFormatterCollection;

    public abstract class DefaultConfig : IClientConfig, IDisposable
    {
        protected DefaultConfig()
        {
            Handler = new ThrowOnFail();

            Formatters = new Formatters { new FormUrlEncoder() };
        }

        public abstract Uri Uri { get; }

        public Handler    Handler    { get; set; }
        public Formatters Formatters { get; set; }
        public TimeSpan?  Timeout    { get; set; }

        public void Dispose()
        {
            Handler?.Dispose();
            Handler = null;
        }
    }

    public sealed class DynamicConfig : DefaultConfig
    {
        readonly Func<Uri> _uri;

        public DynamicConfig(Func<Uri> uri)
        {
            _uri = uri;
        }

        public override Uri Uri => _uri();
    }

    public sealed class FixedConfig : DefaultConfig
    {
        public FixedConfig(Uri uri)
        {
            Uri = uri;
        }

        public FixedConfig(string uri) : this(new Uri(uri)) { }

        public override Uri Uri { get; }

        public static FixedConfig FromAppSetting(string key)
        {
            return new FixedConfig(ConfigurationManager.AppSettings[key]);
        }
    }
}