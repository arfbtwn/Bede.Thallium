using System;
using System.Net.Http;

#pragma warning disable 1591

namespace Bede.Thallium.Handlers
{
    using Handler = HttpMessageHandler;
    using Backoff = Func<int, TimeSpan>;

    public static class PolicyBuilder
    {
        const int Three = 3;
        const int Five  = 5;

        static readonly TimeSpan OneMinute = TimeSpan.FromMinutes(1);

        static TimeSpan Exponential(int attempt) => TimeSpan.FromMilliseconds(125 * Math.Pow(2, attempt));

        static bool IsUnknown(Exception e) => true;

        static bool IsClient(HttpRequestException e) => e.Code().IsClientError();

        static bool IsServer(HttpRequestException e) => e.Code().IsServerError();

        public static RetryHandler<E> WaitAndRetry<E>(this Handler       @this,
                                                           Func<E, bool>  predicate,
                                                           int?           attempts  = null,
                                                           Backoff        backoff   = null)
            where E : Exception
        {
            return new RetryHandler<E>(@this, predicate, attempts ?? Three, backoff ?? Exponential);
        }

        public static CircuitBreakHandler<E> CircuitBreak<E>(this Handler       @this,
                                                                  Func<E, bool>  predicate,
                                                                  int?           limit     = null,
                                                                  TimeSpan?      rest      = null)
            where E : Exception
        {
            return new CircuitBreakHandler<E>(@this, predicate, limit ?? Five, rest ?? OneMinute);
        }

        public static RetryHandler<Exception> RetryOnUnknown(this Handler @this,
                                                                  int?     attempts = null,
                                                                  Backoff  backoff  = null)
        {
            return WaitAndRetry<Exception>(@this, IsUnknown, attempts, backoff);
        }

        public static CircuitBreakHandler<Exception> BreakOnUnknown(this Handler   @this,
                                                                         int?       limit = null,
                                                                         TimeSpan?  rest  = null)
        {
            return CircuitBreak<Exception>(@this, IsUnknown, limit, rest);
        }

        public static RetryHandler<HttpRequestException> RetryOnServerError(this Handler @this,
                                                                                 int?     attempts = null,
                                                                                 Backoff  backoff  = null)
        {
            return WaitAndRetry<HttpRequestException>(@this, IsServer, attempts, backoff);
        }

        public static CircuitBreakHandler<HttpRequestException> BreakOnClientError(this Handler   @this,
                                                                                        int?       limit = null,
                                                                                        TimeSpan?  rest  = null)
        {
            return CircuitBreak<HttpRequestException>(@this, IsClient, limit, rest);
        }
    }
}