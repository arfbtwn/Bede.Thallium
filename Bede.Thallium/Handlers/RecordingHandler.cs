using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Bede.Thallium.Handlers
{
    using Belt;

    /// <summary>
    /// An event driven handler
    /// </summary>
    public class RecordingHandler : DelegatingHandler
    {
        /// <summary>
        /// Default construction
        /// </summary>
        /// <param name="inner"></param>
        public RecordingHandler(HttpMessageHandler inner) : base(inner) { }

        /// <summary>
        /// Construction with an internal <see cref="HttpClientHandler" />
        /// </summary>
        public RecordingHandler() : this(new HttpClientHandler()) { }

        /// <summary>
        /// Emitted when the request message is about to be sent
        /// </summary>
        public event EventHandler<HttpRequestMessage>  Request;

        /// <summary>
        /// Emitted when the response message has been received
        /// </summary>
        public event EventHandler<HttpResponseMessage> Response;

#pragma warning disable 1591
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request?.Invoke(this, request);

            var response = await base.SendAsync(request, cancellationToken).Caf();

            Response?.Invoke(this, response);

            return response;
        }
#pragma warning restore 1591
    }
}
