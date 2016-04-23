using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;

#pragma warning disable 1591

namespace Bede.Thallium.Content
{
    public class ContentBuilder : List<HttpContent>, IContentBuilder
    {
        readonly MediaTypeFormatterCollection _formatters;

        string _mSub, _mBound;

        public ContentBuilder(MediaTypeFormatterCollection formatters)
        {
            _formatters = formatters;
        }

        public ContentBuilder() : this(new MediaTypeFormatterCollection()) { }

        public HttpContent Last()
        {
            return this[Count - 1];
        }

        public IContentBuilder Header(string name, string value)
        {
            Last().Headers.Add(name, value);

            return this;
        }

        public IContentBuilder Header(string name, IEnumerable<string> values)
        {
            Last().Headers.Add(name, values);

            return this;
        }

        public IContentBuilder ContentType(MediaTypeHeaderValue header)
        {
            Last().Headers.ContentType = header;

            return this;
        }

        public IContentBuilder ContentDisposition(ContentDispositionHeaderValue header)
        {
            Last().Headers.ContentDisposition = header;

            return this;
        }

        public IContentBuilder Multi(string sub, string bound)
        {
            _mSub   = sub;
            _mBound = bound;

            return this;
        }

        public new IContentBuilder Add(HttpContent content)
        {
            return With(content);
        }

        public IContentBuilder With(HttpContent content)
        {
            if (null == content) return this;

            base.Add(content);
            return this;
        }

        public IContentBuilder String(string value)
        {
            return With(new StringContent(value));
        }

        public IContentBuilder Bytes(byte[] bytes)
        {
            return With(new ByteArrayContent(bytes));
        }

        public IContentBuilder Stream(Stream stream)
        {
            return With(new StreamContent(stream));
        }

        public IContentBuilder FormUrl(IEnumerable<KeyValuePair<string, string>> form)
        {
            return With(new FormUrlEncodedContent(form));
        }

        public IContentBuilder Struct<T>(T? obj) where T : struct
        {
            return null == obj ? this : With(new ObjectContent<T>(obj.Value, _formatters));
        }

        public IContentBuilder Object<T>(T obj) where T : class
        {
            return null == obj ? this : With(new ObjectContent<T>(obj, _formatters));
        }

        MultipartContent _multi(string sub, string bound)
        {
            return sub == "form-data" ? new MultipartFormDataContent(bound)
                                      : new MultipartContent(sub, bound);
        }

        public IContentBuilder Reduce()
        {
            var reduced = Build();

            Clear();

            Add(reduced);

            return this;
        }

        public HttpContent Build()
        {
            foreach (var c in this.OfType<ObjectContent>())
            {
                c.Ready();
            }

            switch(Count)
            {
                case 0: return null;
                case 1: return this[0];
            }

            var sub   = _mSub   ?? "mixed";
            var bound = _mBound ?? Guid.NewGuid().ToString();

            var multi = _multi(sub, bound);

            foreach (var part in this)
            {
                multi.Add(part);
            }

            return multi;
        }
    }
}