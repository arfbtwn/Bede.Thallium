using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace Bede.Thallium.Handlers
{
    using Belt;

    using Request  = HttpRequestMessage;
    using Response = HttpResponseMessage;

    /// <summary>
    /// A handler that throws on a set of predicates
    /// </summary>
    /// <remarks>
    /// Throws an <see cref="HttpRequestException" /> on any matching predicate
    /// with a request summary and the response content for its message and a set
    /// of <see cref="ExceptionKeys"/> inserted into its <see cref="System.Exception.Data"/>
    /// </remarks>
    public class Throw : DelegatingHandler
    {
        static bool Always(Response msg) => true;

        readonly ISet<Func<Response, bool>> _throws = new HashSet<Func<Response, bool>>();

        bool _wrapAll;

        public Throw() : this(new HttpClientHandler()) { }

        public Throw(HttpMessageHandler inner) : base(inner) { }

        /// <summary>
        /// Adds a predicate to throw on
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public Throw On(Func<Response, bool> predicate)
        {
            _throws.Add(predicate);
            return this;
        }

        /// <summary>
        /// Whether to wrap all exceptions
        /// </summary>
        /// <returns></returns>
        public Throw WrapAll()
        {
            _wrapAll = true;
            return this;
        }

        protected override async Task<Response> SendAsync(Request req, CancellationToken cancellationToken)
        {
            Response msg;
            try
            {
                msg = await base.SendAsync(req, cancellationToken).Caf();
            }
            catch (HttpRequestException e)
            {
                throw new _Exception(req).With(e).Build();
            }
            catch (Exception e)
            {
                if (!_wrapAll) throw;

                throw new _Exception(req).With(e).Build();
            }

            if (!_throws.DefaultIfEmpty(Always).Any(x => x(msg))) return msg;

            if (null == msg.Content)
            {
                throw new _Exception(req).With(msg).Build();
            }

            await msg.Content.LoadIntoBufferAsync().Caf();

            using (var mem = await msg.Content.ReadAsStreamAsync().Caf())
            using (var red = new StreamReader(mem, true))
            {
                var str = await red.ReadToEndAsync().Caf();

                throw new _Exception(req).With(msg).With(str).Build();
            }
        }
    }
}
