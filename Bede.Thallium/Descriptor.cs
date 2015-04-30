using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;

namespace Bede.Thallium
{
    using Static    = Dictionary<string, string[]>;
    using Headers   = Dictionary<ParameterInfo, string>;
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
    public class Descriptor
    {
#pragma warning disable 1591
        public HttpMethod Verb;
        public string     Template;
        public Headers    Headers;
        public Parameter  Body;
        public Static     Static;

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
                Body     = call.Item4,
                Static   = call.Item5
            };
        }
#pragma warning restore 1591
    }
}