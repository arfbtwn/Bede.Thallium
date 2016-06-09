using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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
    public sealed class ThrowOnFail : DelegatingHandler
    {
        public ThrowOnFail() : this(new HttpClientHandler()) { }

        public ThrowOnFail(HttpMessageHandler inner) : base(inner) { }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var msg = await base.SendAsync(request, cancellationToken).Caf();

            if (msg.IsSuccessStatusCode) return msg;

            using (var mem = new MemoryStream())
            using (var red = new StreamReader(mem, true))
            {
                await msg.Content.CopyToAsync(mem).Caf();

                mem.Position = 0;

                var str = await red.ReadToEndAsync().Caf();
                var req = msg.RequestMessage;

                var err = string.Format("{0} {1} HTTP/{2}\n" +
                                        "host: {3}:{4}\n" +
                                        "code: {5:D} => {5}\n" +
                                        "{6}",
                                        req.Method,
                                        req.RequestUri.PathAndQuery,
                                        req.Version,
                                        req.RequestUri.Host,
                                        req.RequestUri.Port,
                                        msg.StatusCode,
                                        str);
                throw new HttpRequestException(err)
                {
                    Data = {
                        { ExceptionKeys.Verb,       req.Method.Method },
                        { ExceptionKeys.Version,    req.Version       },
                        { ExceptionKeys.RequestUri, req.RequestUri    },
                        { ExceptionKeys.Code,       msg.StatusCode    },
                        { ExceptionKeys.Content,    str               }
                    }
                };
            }
        }
    }
}
