using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.CircuitBreaker;

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

    public sealed class RetryArgs : EventArgs
    {
        public Exception Last;
        public TimeSpan  Wait;
    }

    public class RetryHandler<E> : PolicyHandler where E : Exception
    {
        public RetryHandler(HttpMessageHandler inner, Func<E, bool> predicate, int attempts, Func<int, TimeSpan> backoff)
            : base(inner)
        {
            Policy = Policy.Handle(predicate).WaitAndRetryAsync(attempts, backoff, OnRetry);
        }

        public event EventHandler<RetryArgs> Retry;

        void OnRetry(Exception exception, TimeSpan next)
        {
            Retry?.Invoke(this, new RetryArgs { Last = exception, Wait = next });
        }
    }

    public sealed class BrokenArgs : EventArgs
    {
        public Exception Last;
        public TimeSpan  Wait;
    }

    public class CircuitBreakHandler<E> : PolicyHandler where E : Exception
    {
        public CircuitBreakHandler(HttpMessageHandler inner, Func<E, bool> predicate, int limit, TimeSpan rest)
            : base(inner)
        {
            Policy = Breaker = Policy.Handle(predicate).CircuitBreakerAsync(limit, rest, OnBroken, OnReset, OnHalfOpen);
        }

        CircuitBreakerPolicy Breaker { get; }

        public event EventHandler<BrokenArgs> Broken;
        public event EventHandler             Reset;
        public event EventHandler             HalfOpen;

        public bool IsOpen     => Breaker.CircuitState == CircuitState.Open;
        public bool IsClosed   => Breaker.CircuitState == CircuitState.Closed;
        public bool IsHalfOpen => Breaker.CircuitState == CircuitState.HalfOpen;
        public bool IsIsolated => Breaker.CircuitState == CircuitState.Isolated;

        public Exception Last  => Breaker.LastException;

        public void Open()
        {
            Breaker.Isolate();
        }

        public void Close()
        {
            Breaker.Reset();
        }

        void OnBroken(Exception last, TimeSpan next)
        {
            Broken?.Invoke(this, new BrokenArgs { Last = last, Wait = next });
        }

        void OnReset()
        {
            Reset?.Invoke(this, EventArgs.Empty);
        }

        void OnHalfOpen()
        {
            HalfOpen?.Invoke(this, EventArgs.Empty);
        }
    }
}
