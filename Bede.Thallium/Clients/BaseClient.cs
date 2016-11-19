using System;
using System.Net.Http;

#pragma warning disable 1591

namespace Bede.Thallium.Clients
{
    /// <summary>
    /// Basic client functionality
    /// </summary>
    public abstract class BaseClient : SkeletonClient, IDisposable
    {
        readonly Lazy<HttpClient> _client;

        protected BaseClient()
        {
            _client = new Lazy<HttpClient>(_Client);
        }

        #region Disposable

        ~BaseClient()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        void IDisposable.Dispose()
        {
            Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            if (_client.IsValueCreated)
            {
                _client.Value.Dispose();
            }
        }

        #endregion

        protected virtual HttpMessageHandler Handler => null;

        public virtual TimeSpan? Timeout => null;

        protected override HttpClient Client() => _client.Value;

        HttpClient _Client() => new HttpClient(Handler ?? Default.Handler, true)
                                {
                                    Timeout = Timeout ?? Default.Timeout
                                };
    }
}
