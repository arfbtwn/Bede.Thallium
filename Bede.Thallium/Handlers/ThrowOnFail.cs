using System.Net.Http;

#pragma warning disable 1591

namespace Bede.Thallium.Handlers
{
    using Belt;

    /// <summary>
    /// Thallium's default handler
    /// </summary>
    /// <remarks>
    /// Throws an <see cref="HttpRequestException" /> on request failure
    /// with a request summary and the response content for its message
    /// and a set of keys inserted into its data collection defined by
    /// <see cref="ExceptionKeys"/>
    /// </remarks>
    public sealed class ThrowOnFail : Throw
    {
        static bool Fail(HttpResponseMessage msg) => !msg.IsSuccessStatusCode;

        public ThrowOnFail()
        {
            On(Fail);
        }

        public ThrowOnFail(HttpMessageHandler inner) : base(inner)
        {
            On(Fail);
        }
    }
}
