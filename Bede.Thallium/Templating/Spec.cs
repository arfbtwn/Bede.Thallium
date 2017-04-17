#pragma warning disable 1591

namespace Bede.Thallium.Templating
{
    struct Spec
    {
        public Spec(char fst, char sep, bool named, char ifemp, bool allow)
        {
            this.fst   = fst;
            this.sep   = sep;
            this.named = named;
            this.ifemp = ifemp;
            this.allow = allow;
        }

        public readonly char fst;
        public readonly char sep;
        public readonly bool named;
        public readonly char ifemp;
        public readonly bool allow;
    }
}