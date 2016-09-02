using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Polly;

#pragma warning disable 1591

namespace Bede.Thallium.Polly
{
    public class PolicyHandler : DelegatingHandler
    {
        protected PolicyHandler(HttpMessageHandler inner) : base(inner)
        {
        }

        public PolicyHandler(HttpMessageHandler inner, Policy policy) : base(inner)
        {
            Policy = policy;
        }

        protected Policy Policy { get; set; }

        protected sealed override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Policy.ExecuteAsync(() => base.SendAsync(request, cancellationToken));
        }
    }
}
