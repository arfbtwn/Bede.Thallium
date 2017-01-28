using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;

#pragma warning disable 612, 1591

namespace Bede.Thallium
{
    using Static    = Dictionary<string, string[]>;
    using Headers   = Dictionary<ParameterInfo, string>;
    using Body      = Dictionary<ParameterInfo, ContentDescription>;
    using Parameter = ParameterInfo;
    using Call      = Tuple<
                            HttpMethod,
                            string,
                            Dictionary<ParameterInfo, string>,
                            ParameterInfo,
                            Dictionary<string, string[]>
                           >;

    /// <summary>
    /// Class defining a simple HTTP API call
    /// </summary>
    [Obsolete]
    public class Descriptor
    {
        public HttpMethod Verb;
        public string     Template;
        public Headers    Headers;
        public Static     Static;
        public Parameter  Body;

        public static implicit operator Call(Descriptor call)
        {
            return Tuple.Create(call.Verb, call.Template, call.Headers, call.Body, call.Static);
        }

        public static implicit operator Descriptor(Call call)
        {
            return new Descriptor
            {
                Verb     = call.Item1,
                Template = call.Item2,
                Headers  = call.Item3,
                Static   = call.Item5,
                Body     = call.Item4
            };
        }
    }

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
        public Headers    Headers;
        public Static     Static;

        public string     Subtype;
        public string     Boundary;
        public Body       Body;

        public static implicit operator Description(Descriptor call)
        {
            return new Description
            {
                Verb     = call.Verb,
                Template = call.Template,
                Headers  = call.Headers,
                Static   = call.Static,
                Body     = { { call.Body, new ContentDescription() } }
            };
        }

        public static implicit operator Description(Call call)
        {
            return (Descriptor) call;
        }
    }
}