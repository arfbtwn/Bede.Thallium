using System;
using System.Configuration;
using System.Reflection;

#pragma warning disable 1591

namespace Bede.Thallium.Introspection
{
    /// <summary>
    /// Inspects by attribute
    /// </summary>
    public sealed class Simple : IIntrospect
    {
        readonly bool _trim;

        /// <summary>
        /// Reads key Bede.Thallium.Attribute.Route.TrimLeadingSlashes
        /// </summary>
        public Simple()
        {
            bool.TryParse(ConfigurationManager.AppSettings["Bede.Thallium.Attribute.Route.TrimLeadingSlashes"], out _trim);
        }

        public Simple(bool trim)
        {
            _trim = trim;
        }

        public Description Call(Type type, MethodInfo method)
        {
            return new AttributeInspection(type, method) { Trim = _trim }.V2();
        }
    }
}