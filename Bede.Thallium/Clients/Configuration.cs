using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Formatting;

#pragma warning disable 1591

namespace Bede.Thallium.Clients
{
    using Handler    = HttpMessageHandler;
    using Formatters = MediaTypeFormatterCollection;

    public abstract class DefaultConfig : IClientConfig, IDisposable
    {
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

    public class DynamicConfig : DefaultConfig
    {
        readonly Func<Uri> _uri;

        public DynamicConfig(Func<Uri> uri)
        {
            _uri = uri;
        }

        public sealed override Uri Uri => _uri();
    }

    public class FixedConfig : DefaultConfig
    {
        public FixedConfig(Uri uri)
        {
            Uri = uri;
        }

        public FixedConfig(string uri) : this(new Uri(uri)) { }

        public sealed override Uri Uri { get; }

        public static FixedConfig FromAppSetting(string key)
        {
            return new FixedConfig(ConfigurationManager.AppSettings[key]);
        }
    }
}
