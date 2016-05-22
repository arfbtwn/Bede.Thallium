using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace Bede.Thallium.Auth
{
    /// <summary>
    /// OAuth2 communication interface
    /// </summary>
    public interface IOAuth2
    {
        /// <summary>
        /// Get the list of keys from the server
        /// </summary>
        /// <returns></returns>
        [Get(".well-known/jwks")]
        Task<Jwks>  Keys();

        /// <summary>
        /// Issue an access token
        /// </summary>
        /// <param name="oauth"></param>
        /// <returns></returns>
        [Post("connect/token")]
        Task<Token> Auth([FormUrl] OAuth oauth);
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
        public int    expires_in    { get; set; }
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
                return ContainsKey(key) ? base[key] : null;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    Remove(key);
                }
                else
                {
                    base[key] = value;
                }
            }
        }

        public string GrantType
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

        public string[] Scopes
        {
            get { return Array("scope"); }
            set { Array("scope", value); }
        }

        public string[] AcrValues
        {
            get { return Array("acr_values"); }
            set { Array("acr_values", value); }
        }

        protected string[] Array(string key)
        {
            return this[key]?.Split(' ');
        }

        protected void Array(string key, string[] values)
        {
            this[key] = string.Join(" ", values ?? EmpT);
        }
    }

    /// <summary>
    /// Client credentials request
    /// </summary>
    public sealed class Client : OAuth
    {
        public Client() { GrantType = "client_credentials"; }
    }

    /// <summary>
    /// Resource owner request
    /// </summary>
    public sealed class Owner : OAuth
    {
        public Owner() { GrantType = "password"; }

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
        public Refresh() { GrantType = "refresh_token"; }

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
