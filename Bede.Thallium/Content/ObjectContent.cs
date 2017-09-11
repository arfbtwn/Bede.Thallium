using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace Bede.Thallium.Content
{
    class ObjectContent : HttpContent
    {
        readonly Type   _t;
        readonly object _v;
        readonly MediaTypeFormatterCollection _c;

        MediaTypeFormatter _f;

        public ObjectContent(Type type, object v, MediaTypeFormatterCollection c)
        {
            _t = type;
            _v = v;
            _c = c;
        }

        public void Ready()
        {
            if (null != _f) return;

            var ct = Headers.ContentType;

            _f = null == ct ? _c.FirstOrDefault(x => x.CanWriteType(_t)) : _c.FindWriter(_t, ct);

            Assertion.HasFormatter(_f, _t, ct);

            _f.SetDefaultContentHeaders(_t, Headers, ct);
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            Ready();

            return _f.WriteToStreamAsync(_t, _v, stream, this, context);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }
    }

    class ObjectContent<T> : ObjectContent
    {
        public ObjectContent(T obj, MediaTypeFormatterCollection collection)
            : base(typeof(T), obj, collection)
        {

        }
    }
}
