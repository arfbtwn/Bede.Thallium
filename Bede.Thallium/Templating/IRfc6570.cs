using System.Collections.Generic;

#pragma warning disable 1591

namespace Bede.Thallium.Templating
{
    using IParams = IDictionary<string, object>;

    /// <summary>
    /// An implementation of RFC6570, URI template expansion
    /// </summary>
    public interface IRfc6570
    {
        string Expand(string template, IParams parameters);
    }
}
