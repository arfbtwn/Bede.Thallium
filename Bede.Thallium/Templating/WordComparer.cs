using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Bede.Thallium.Templating
{
    class WordComparer : IEqualityComparer<string>
    {
        public static WordComparer Instance = new WordComparer();

        readonly Regex _filter = new Regex(@"[\w_0-9]+", RegexOptions.Compiled);

        WordComparer() { }

        string _(string x) => _filter.Match(x).Value;

        public bool Equals(string x, string y)
        {
            return _(x) == _(y);
        }

        public int GetHashCode(string obj)
        {
            return _(obj).GetHashCode();
        }
    }
}
