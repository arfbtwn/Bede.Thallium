using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;

#pragma warning disable 1591

namespace Bede.Thallium
{
    using _Static   = Dictionary<string, string[]>;
    using _Headers  = Dictionary<ParameterInfo, string>;
    using _Body     = Dictionary<ParameterInfo, ContentDescription>;

    using Expansion = _Rfc6570;
    using Collector = _Collector;
    using Request   = List<_Request>;
    using Headers   = Dictionary<ParameterInfo, _Header>;
    using Contents  = Dictionary<ParameterInfo, _Content>;

    // TODO: Interface these

    public delegate string      _Rfc6570  (string template, IDictionary<string, object> args);
    public delegate void        _Request  (HttpRequestMessage message);
    public delegate void        _Header   (HttpHeaders headers, object value);
    public delegate HttpContent _Content  (object value);
    public delegate HttpContent _Collector(params HttpContent[] contents);

    public class ContentDescription
    {
        public string Type;
        public string Disposition;

        public bool   SetName;
        public bool   SetFileName;
    }

    [DebuggerDisplay("{Verb.Method} {Template}")]
    public class Description
    {
        public HttpMethod Verb;
        public string     Template;
        public _Headers   Headers;
        public _Static    Static;

        public string     Subtype;
        public string     Boundary;
        public _Body      Body;
    }

    public class DescriptionV2
    {
        static DescriptionV2()
        {
            var my = new DescriptionV2
            {
                Expansion = new Templating.Compiler().Expand,
                Request   =
                {
                    new HeaderAttribute("foobar", "foo").Request
                },
                Headers   =
                {
                    { null, new HeaderAttribute("myheader").Header }
                },
                Contents  =
                {
                    { null, new FormUrlAttribute().Create },
                    { null, new OctetAttribute  ().Create },
                    { null, o =>
                            {
                                var list = (List<string>) o;

                                var content = new MultipartContent();

                                foreach (var str in list)
                                {
                                    content.Add(new StringContent(str));
                                }

                                return content;
                            }
                    }
                },
                Collector = new CollectorAttribute().Collect
            };
        }

        public Expansion Expansion;
        public Collector Collector;

        public readonly Request  Request  = new Request();
        public readonly Headers  Headers  = new Headers();
        public readonly Contents Contents = new Contents();
    }

    public class DescriptionV3
    {
        static DescriptionV3()
        {
            var my = new DescriptionV3
            {
                Route =
                {
                    new RouteAttribute("/api/foobar"),
                    new PutAttribute()
                },
                Request =
                {
                    new HeaderAttribute("User-Agent", "Bede.Thallium 2.x"),
                    new HeaderAttribute("X-Correlation-Token", /* TODO: Generated */ Guid.NewGuid().ToString())
                },
                Headers =
                {
                    { null, new HeaderAttribute("X-Site-Code")         },
                    { null, new HeaderAttribute("X-Correlation-Token") }
                },
                Content =
                {
                    { null, new BodyAttribute()    },
                    { null, new FormUrlAttribute() },
                    { null, new OctetAttribute()   }
                },
                Collector = new CollectorAttribute
                {
                    SubType   = "application/foo",
                    Boundary  = Guid.NewGuid().ToString(),
                    Multipart = true
                }
            };
        }


        public IList<RouteAttribute>   Route   { get; set; }

        public IList<RequestAttribute> Request { get; set; }

        public IDictionary<ParameterInfo, HeaderAttribute> Headers { get; set; }
        public IDictionary<ParameterInfo, BodyAttribute>   Content { get; set; }

        public CollectorAttribute Collector { get; set; } = new CollectorAttribute();

        // TODO: Functions to reduce to the real Descriptor implementation
    }

    public class CollectorAttribute : Attribute
    {
        public string SubType   { get; set; }
        public string Boundary  { get; set; }
        public bool   Multipart { get; set; }

        protected virtual MultipartContent Multi()
        {
            if (string.IsNullOrWhiteSpace(SubType))  return new MultipartContent();
            if (string.IsNullOrWhiteSpace(Boundary)) return new MultipartContent(SubType);

            return new MultipartContent(SubType, Boundary);
        }

        public virtual HttpContent Collect(HttpContent[] contents)
        {
            if (!Multipart)
            switch(contents.Length)
            {
                case 0: return null;
                case 1: return contents[0];
            }

            var multi = Multi();

            foreach (var _ in contents)
            {
                multi.Add(_);
            }

            return multi;
        }
    }
}