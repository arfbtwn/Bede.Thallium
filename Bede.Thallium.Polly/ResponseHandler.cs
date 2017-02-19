using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Polly;

#pragma warning disable 1591

namespace Bede.Thallium.Polly
{
    using Predicate = Func<HttpResponseMessage, bool>;

    public abstract class ResponseHandler : DelegatingHandler
    {
        static bool Always(HttpResponseMessage msg) => true;

        readonly ISet<Predicate> _predicates = new HashSet<Predicate>();

        protected ResponseHandler(HttpMessageHandler inner) : base(inner)
        {
            Builder = Policy<HttpResponseMessage>.HandleResult(_);
        }

        bool _(HttpResponseMessage msg)
        {
            return _predicates.DefaultIfEmpty(Always).Any(x => x(msg));
        }

        protected PolicyBuilder<HttpResponseMessage> Builder { get; set; }

        protected Policy<HttpResponseMessage> Policy { get; set; }

        public ResponseHandler On(Predicate predicate)
        {
            _predicates.Add(predicate);
            return this;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (null == Policy) throw new InvalidOperationException();

            return Policy.ExecuteAsync(() => base.SendAsync(request, cancellationToken));
        }
    }
}
