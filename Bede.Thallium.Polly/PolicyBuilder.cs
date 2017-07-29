using System;
using System.Linq;
using System.Net;
using System.Net.Http;

#pragma warning disable 1591

namespace Bede.Thallium.Polly
{
    using Handler = HttpMessageHandler;
    using Backoff = Func<int, TimeSpan>;

    using static TimeSpan;

    public static partial class PolicyBuilder
    {
        /// <summary>
        /// The default number of retries
        /// </summary>
        public const int Three = 3;

        /// <summary>
        /// The default number of errors before circuit break
        /// </summary>
        public const int Five = 5;

        /// <summary>
        /// The default circuit break rest period
        /// </summary>
        public static readonly TimeSpan OneMinute = FromMinutes(1);

        /// <summary>
        /// The default slot time for exponential backoff calculation
        /// </summary>
        public static readonly TimeSpan SlotTime = FromMilliseconds(125);

        /// <summary>
        /// Exponential backoff function using <see cref="SlotTime"/>
        /// </summary>
        /// <param name="attempt"></param>
        /// <returns></returns>
        public static TimeSpan Exponential(int attempt)
        {
            return FromTicks((long) (SlotTime.Ticks * (Math.Pow(2, attempt) - 1)));
        }

        /// <summary>
        /// Configures a <see cref="ResponseHandler"/> to respond to any of the specified status codes
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="codes"></param>
        /// <returns></returns>
        public static ResponseHandler On(this ResponseHandler handler, params HttpStatusCode[] codes)
        {
            return handler.On(x => codes.Contains(x.StatusCode));
        }

        /// <summary>
        /// Wrap this handler in <see cref="RetryHandler"/> behaviour
        /// </summary>
        /// <param name="first">The handler to wrap</param>
        /// <param name="count">The number of retries, <see cref="Three"/> if not supplied</param>
        /// <param name="backoff">A function providing delay <see cref="TimeSpan"/>s, <see cref="Exponential(int)"/> if not supplied</param>
        /// <param name="setup">A setup function to execute on the new handler</param>
        /// <returns></returns>
        public static RetryHandler Retry(this Handler first,
                                              int?    count   = null,
                                              Backoff backoff = null,
                                              Action<RetryHandler> setup = null)
        {
            var _ = new RetryHandler(first, count ?? Three, backoff ?? Exponential);
            setup?.Invoke(_);
            return _;
        }

        /// <summary>
        /// Wrap this handler in <see cref="CircuitBreakHandler"/> behaviour
        /// </summary>
        /// <param name="first">The handler to wrap</param>
        /// <param name="limit">The number of matched requests before breaking, <see cref="Five"/> if not supplied</param>
        /// <param name="rest">The rest period for a broken circuit, <see cref="OneMinute"/> if not supplied</param>
        /// <param name="setup">A setup function to execute on the new handler</param>
        /// <returns></returns>
        public static CircuitBreakHandler Break(this Handler   first,
                                                     int?      limit = null,
                                                     TimeSpan? rest  = null,
                                                     Action<CircuitBreakHandler> setup = null)
        {
            var _ = new CircuitBreakHandler(first, limit ?? Five, rest ?? OneMinute);
            setup?.Invoke(_);
            return _;
        }
    }
}
