using System;
using System.Net.Http;
using System.Text;

namespace Bede.Thallium.Handlers
{
    using Request  = HttpRequestMessage;
    using Response = HttpResponseMessage;

    class _Exception
    {
        readonly Request  _r;

        Exception _e;
        Response  _m;
        string    _s;

        public _Exception(Request req)
        {
            _r = req;
        }

        public _Exception With(Response msg)
        {
            _m = msg;
            return this;
        }

        public _Exception With(string content)
        {
            _s = content;
            return this;
        }

        public _Exception With(Exception inner)
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
}