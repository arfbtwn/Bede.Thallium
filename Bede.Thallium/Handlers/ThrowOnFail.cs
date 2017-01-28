using System;
using System.IO;
using System.Net.Http;
using System.Text;
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
        class _
        {
            readonly HttpRequestMessage  _r;

            Exception           _e;
            HttpResponseMessage _m;
            string              _s;

            public _(HttpRequestMessage req)
            {
                _r = req;
            }

            public _ With(HttpResponseMessage msg)
            {
                _m = msg;
                return this;
            }

            public _ With(string content)
            {
                _s = content;
                return this;
            }

            public _ With(Exception inner)
            {
                _e = inner;
                return this;
            }

            public HttpRequestException Build()
            {
                var uri  = _r.RequestUri;
                var code = _m?.StatusCode ?? 0;

                var err = new StringBuilder()
                    .AppendFormat("{0} {1} HTTP/{2}", _r.Method, uri.PathAndQuery, _r.Version)
                    .AppendLine()
                    .AppendFormat("host: {0}:{1}", uri.Host, uri.Port)
                    .AppendLine()
                    .AppendFormat("code: {0:D} => {0}", code)
                    .AppendLine()
                    .Append(_s ?? _e?.Message)
                    .ToString();

                return new HttpRequestException(err, _e)
                {
                    Data =
                    {
                        { ExceptionKeys.Verb,       _r.Method.Method },
                        { ExceptionKeys.Version,    _r.Version       },
                        { ExceptionKeys.RequestUri, _r.RequestUri    },
                        { ExceptionKeys.Code,       code             },
                        { ExceptionKeys.Content,    _s               }
                    }
                };
            }
        }

        bool _wrapAll;

        public ThrowOnFail() : this(new HttpClientHandler()) { }

        public ThrowOnFail(HttpMessageHandler inner) : base(inner) { }

        /// <summary>
        /// Whether to wrap all exceptions
        /// </summary>
        /// <returns></returns>
        public ThrowOnFail WrapAll()
        {
            _wrapAll = true;
            return this;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken cancellationToken)
        {
            HttpResponseMessage msg;
            try
            {
                msg = await base.SendAsync(req, cancellationToken).Caf();
            }
            catch (HttpRequestException e)
            {
                throw new _(req).With(e).Build();
            }
            catch (Exception e)
            {
                if (!_wrapAll) throw;

                throw new _(req).With(e).Build();
            }

            if (msg.IsSuccessStatusCode) return msg;

            if (null == msg.Content)
            {
                throw new _(req).With(msg).Build();
            }

            await msg.Content.LoadIntoBufferAsync().Caf();

            using (var mem = await msg.Content.ReadAsStreamAsync().Caf())
            using (var red = new StreamReader(mem, true))
            {
                var str = await red.ReadToEndAsync().Caf();

                throw new _(req).With(msg).With(str).Build();
            }
        }
    }
}
