using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;

#pragma warning disable 1591

namespace Bede.Thallium
{
    using Static    = Dictionary<string, string[]>;
    using Headers   = Dictionary<ParameterInfo, string>;
    using Body      = Dictionary<ParameterInfo, ContentDescription>;

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
    }
}