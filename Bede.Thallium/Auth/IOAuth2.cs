using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace Bede.Thallium.Auth
{
    /// <summary>
    /// OAuth2 communication interface
    /// </summary>
    public interface IOAuth2 : IToken, IJwk, IOpenId { }

    public interface IToken
    {
        /// <summary>
        /// Issue an access token
        /// </summary>
        /// <param name="oauth"></param>
        /// <param name="basic"></param>
        /// <returns></returns>
        [Post("connect/token")]
        Task<Token> Auth([FormUrl] OAuth oauth, [Header("Authorization")] Basic basic = null);
    }

    public interface IJwk
    {
        /// <summary>
        /// Get the list of keys from the server
        /// </summary>
        /// <returns></returns>
        [Get(".well-known/jwks")]
        Task<Jwks> Keys();
    }

    public interface IOpenId
    {
        /// <summary>
        /// Get OpenID information from the server
        /// </summary>
        /// <returns></returns>
        [Get(".well-known/openid-configuration")]
        Task<OpenId> OpenId();
    }

    /// <summary>
    /// OpenID Configuration Data
    /// </summary>
    public class OpenId
    {
        public Uri          issuer                                { get; set; }
        public Uri          jwks_uri                              { get; set; }
        public Uri          authorization_endpoint                { get; set; }
        public Uri          token_endpoint                        { get; set; }
        public Uri          userinfo_endpoint                     { get; set; }
        public Uri          end_session_endpoint                  { get; set; }
        public Uri          check_session_iframe                  { get; set; }
        public Uri          revocation_endpoint                   { get; set; }
        public Uri          introspection_endpoint                { get; set; }
        public bool?        frontchannel_logout_supported         { get; set; }
        public bool?        frontchannel_logout_session_supported { get; set; }
        public List<string> scopes_supported                      { get; set; }
        public List<string> claims_supported                      { get; set; }
        public List<string> response_types_supported              { get; set; }
        public List<string> response_modes_supported              { get; set; }
        public List<string> grant_types_supported                 { get; set; }
        public List<string> subject_types_supported               { get; set; }
        public List<string> id_token_signing_alg_values_supported { get; set; }
        public List<string> code_challenge_methods_supported      { get; set; }
        public List<string> token_endpoint_auth_methods_supported { get; set; }
    }

    /// <summary>
    /// A list of JSON Web Tokens
    /// </summary>
    public class Jwks
    {
        public List<Jwk> keys { get; set; }
    }

    /// <summary>
    /// A JSON Web Token
    /// </summary>
    public class Jwk
    {
        public string       kty     { get; set; }
        public string       use     { get; set; }
        public string       key_ops { get; set; }
        public string       alg     { get; set; }
        public string       kid     { get; set; }
        public string       e       { get; set; }
        public string       n       { get; set; }
        public string       x5u     { get; set; }
        public List<string> x5c     { get; set; }
        public string       x5t     { get; set; }
    }

    /// <summary>
    /// An access token
    /// </summary>
    public struct Token
    {
        public string access_token  { get; set; }
        public int?   expires_in    { get; set; }
        public string token_type    { get; set; }
        public string refresh_token { get; set; }
    }

    /// <summary>
    /// General purpose OAuth2 request
    /// </summary>
    public class OAuth : Dictionary<string, string>
    {
        static readonly string[] EmpT = { };

        public new string this[string key]
        {
            get
            {
                string v;
                TryGetValue(key, out v);
                return v;
            }
            set
            {
                base[key] = value;
            }
        }

        public void Add(string key, IList<string> value)
        {
            Array(key, value);
        }

        protected IList<string> Array(string key)
        {
            var _ = new ObservableCollection<string>(this[key]?.Split(' ') ?? EmpT);

            _.CollectionChanged += (o, x) => Array(key, _);

            return _;
        }

        protected void Array(string key, IList<string> values)
        {
            this[key] = null == values ? null : string.Join(" ", values);
        }

        public string Grant
        {
            get { return this["grant_type"]; }
            set { this["grant_type"] = value; }
        }

        public string Client
        {
            get { return this["client_id"]; }
            set { this["client_id"] = value; }
        }

        public string Secret
        {
            get { return this["client_secret"]; }
            set { this["client_secret"] = value; }
        }

        public IList<string> Scope
        {
            get { return Array("scope"); }
            set { Array("scope", value); }
        }

        public IList<string> Acr
        {
            get { return Array("acr_values"); }
            set { Array("acr_values", value); }
        }
    }

    /// <summary>
    /// Client credentials request
    /// </summary>
    public sealed class Client : OAuth
    {
        public Client() { Grant = "client_credentials"; }
    }

    /// <summary>
    /// Resource owner request
    /// </summary>
    public sealed class Owner : OAuth
    {
        public Owner() { Grant = "password"; }

        public string Username
        {
            get { return this["username"]; }
            set { this["username"] = value; }
        }

        public string Password
        {
            get { return this["password"]; }
            set { this["password"] = value; }
        }
    }

    /// <summary>
    /// Refresh token request
    /// </summary>
    public sealed class Refresh : OAuth
    {
        public Refresh() { Grant = "refresh_token"; }

        public Refresh(OAuth request, Token token) : this()
        {
            if (string.IsNullOrWhiteSpace(token.refresh_token))
            {
                throw new ArgumentException("No refresh token", nameof(token));
            }

            Client = request.Client;
            Secret = request.Secret;
            Token  = token.refresh_token;
        }

        public string Token
        {
            get { return this["refresh_token"]; }
            set { this["refresh_token"] = value; }
        }
    }
}
