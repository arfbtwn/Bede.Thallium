using System;
using System.Net.Http;
using Polly;
using Polly.CircuitBreaker;

namespace Bede.Thallium.Polly
{
    public sealed class BrokenArgs : EventArgs
    {
        public HttpResponseMessage Message;
        public Exception           Last;
        public TimeSpan            Wait;
    }

    [Obsolete]
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

    public class CircuitBreakHandler : ResponseHandler
    {
        public CircuitBreakHandler(HttpMessageHandler inner, int limit, TimeSpan rest) : base(inner)
        {
            Policy = Breaker = Builder.CircuitBreakerAsync(limit, rest, OnBroken, OnReset, OnHalfOpen);
        }

        CircuitBreakerPolicy<HttpResponseMessage> Breaker { get; }

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

        void OnBroken(DelegateResult<HttpResponseMessage> result, TimeSpan rest)
        {
            Broken?.Invoke(this, new BrokenArgs { Message = result.Result, Last = result.Exception, Wait = rest });
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