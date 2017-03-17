using System;
using System.Net.Http;

namespace Bede.Thallium.Polly
{
    using Handler = HttpMessageHandler;
    using Backoff = Func<int, TimeSpan>;

#pragma warning disable 1591
    public static partial class PolicyBuilder
#pragma warning restore 1591
    {
        /// <summary>
        /// Wrap this handler in <see cref="RetryHandler"/> behaviour
        /// </summary>
        /// <param name="first">The handler to wrap</param>
        /// <param name="count">The number of retries, <see cref="Three"/> if not supplied</param>
        /// <param name="backoff">A function providing delay <see cref="TimeSpan"/>s, <see cref="Exponential(int)"/> if not supplied</param>
        public static RetryHandler Retry(this Handler first, int? count, Backoff backoff)
        {
            return Retry(first, count, backoff, null);
        }

        /// <summary>
        /// Wrap this handler in <see cref="CircuitBreakHandler"/> behaviour
        /// </summary>
        /// <param name="first">The handler to wrap</param>
        /// <param name="limit">The number of matched requests before breaking, <see cref="Five"/> if not supplied</param>
        /// <param name="rest">The rest period for a broken circuit, <see cref="OneMinute"/> if not supplied</param>
        /// <returns></returns>
        public static CircuitBreakHandler Break(this Handler first, int? limit, TimeSpan? rest)
        {
            return Break(first, limit, rest, null);
        }
    }
}
