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

    [Obsolete]
    public class RetryHandler<E> : PolicyHandler where E : Exception
    {
        public RetryHandler(HttpMessageHandler inner, Func<E, bool> predicate, int attempts, Func<int, TimeSpan> backoff)
            : base(inner)
        {
            Policy = Policy.Handle(predicate).WaitAndRetryAsync(attempts, backoff, OnRetry);
        }

        public event EventHandler<RetryArgs> Retry;

        Task OnRetry(Exception exception, TimeSpan next)
        {
            Retry?.Invoke(this, new RetryArgs { Last = exception, Wait = next });

            return Task.FromResult(true);
        }
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