using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace Bede.Thallium.Handlers
{
    using Belt;

    /// <summary>
    /// A simple handler that buffers its input and output content
    /// </summary>
    public sealed class BufferingHandler : DelegatingHandler
    {
        public BufferingHandler(HttpMessageHandler inner) : base(inner) { }
        public BufferingHandler() : this(new HttpClientHandler()) { }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await request.Content.LoadIntoBufferAsync().Caf();

            var msg = await base.SendAsync(request, cancellationToken).Caf();

            await msg.Content.LoadIntoBufferAsync().Caf();

            return msg;
        }
    }
}
