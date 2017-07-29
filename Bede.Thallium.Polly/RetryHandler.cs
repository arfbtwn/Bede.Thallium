using System;
using System.Net.Http;
using System.Threading.Tasks;
using Polly;

#pragma warning disable 1591

namespace Bede.Thallium.Polly
{
    public sealed class RetryArgs : EventArgs
    {
        public HttpResponseMessage Message;
        public Exception           Last;
        public TimeSpan            Wait;
    }

    public class RetryHandler : ResponseHandler
    {
        public RetryHandler(HttpMessageHandler inner, int count, Func<int, TimeSpan> backoff) : base(inner)
        {
            Policy = Builder.WaitAndRetryAsync(count, backoff, OnRetry);
        }

        public event EventHandler<RetryArgs> Retry;

        Task OnRetry(DelegateResult<HttpResponseMessage> result, TimeSpan current)
        {
            Retry?.Invoke(this, new RetryArgs { Message = result.Result, Last = result.Exception, Wait = current });
            return Task.FromResult(false);
        }
    }
}