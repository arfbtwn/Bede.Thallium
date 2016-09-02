using System;
using System.Linq;
using System.Net;
using System.Net.Http;

#pragma warning disable 1591

namespace Bede.Thallium.Polly
{
    using Handler = HttpMessageHandler;
    using Backoff = Func<int, TimeSpan>;

    public static class PolicyBuilder
    {
        const int Three = 3;
        const int Five  = 5;

        static readonly TimeSpan OneMinute = TimeSpan.FromMinutes(1);

        static TimeSpan Exponential(int attempt) => TimeSpan.FromMilliseconds(125 * (Math.Pow(2, attempt) - 1));

        static bool IsUnknown(Exception e) => true;

        static bool IsClient(HttpRequestException e) => e.Code().IsClientError();

        static bool IsServer(HttpRequestException e) => e.Code().IsServerError();

        [Obsolete]
        public static RetryHandler<E> WaitAndRetry<E>(this Handler       @this,
                                                           Func<E, bool>  predicate,
                                                           int?           attempts  = null,
                                                           Backoff        backoff   = null)
            where E : Exception
        {
            return new RetryHandler<E>(@this, predicate, attempts ?? Three, backoff ?? Exponential);
        }

        [Obsolete]
        public static CircuitBreakHandler<E> CircuitBreak<E>(this Handler       @this,
                                                                  Func<E, bool>  predicate,
                                                                  int?           limit     = null,
                                                                  TimeSpan?      rest      = null)
            where E : Exception
        {
            return new CircuitBreakHandler<E>(@this, predicate, limit ?? Five, rest ?? OneMinute);
        }

        [Obsolete]
        public static RetryHandler<Exception> RetryOnUnknown(this Handler @this,
                                                                  int?     attempts = null,
                                                                  Backoff  backoff  = null)
        {
            return WaitAndRetry<Exception>(@this, IsUnknown, attempts, backoff);
        }

        [Obsolete]
        public static CircuitBreakHandler<Exception> BreakOnUnknown(this Handler   @this,
                                                                         int?       limit = null,
                                                                         TimeSpan?  rest  = null)
        {
            return CircuitBreak<Exception>(@this, IsUnknown, limit, rest);
        }

        [Obsolete]
        public static RetryHandler<HttpRequestException> RetryOnServerError(this Handler @this,
                                                                                 int?     attempts = null,
                                                                                 Backoff  backoff  = null)
        {
            return WaitAndRetry<HttpRequestException>(@this, IsServer, attempts, backoff);
        }

        [Obsolete]
        public static CircuitBreakHandler<HttpRequestException> BreakOnClientError(this Handler   @this,
                                                                                        int?       limit = null,
                                                                                        TimeSpan?  rest  = null)
        {
            return CircuitBreak<HttpRequestException>(@this, IsClient, limit, rest);
        }

        public static ResponseHandler On(this ResponseHandler responseHandler, params HttpStatusCode[] codes)
        {
            return responseHandler.On(x => codes.Contains(x.StatusCode));
        }

        public static RetryHandler Retry(this Handler first, int? count = null, Backoff backoff = null)
        {
            return new RetryHandler(first, count ?? Three, backoff ?? Exponential);
        }

        public static CircuitBreakHandler Break(this Handler first, int? limit = null, TimeSpan? rest = null)
        {
            return new CircuitBreakHandler(first, limit ?? Five, rest ?? OneMinute);
        }
    }
}