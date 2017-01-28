
#pragma warning disable 1591

namespace Bede.Thallium.Auth
{
    /// <summary>
    /// Base class for authorization headers
    /// </summary>
    public class Header
    {
        readonly string _value;

        protected Header(string type, string value)
        {
            _value = type + " " + value;
        }

        public override string ToString() => _value;

        public static implicit operator Header(Token token)
        {
            return new Header(token.token_type, token.access_token);
        }
    }
}
