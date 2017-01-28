using System;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace Bede.Thallium.Auth
{
    using Belt;

    /// <summary>
    /// Tracks an access token, authenticating as necessary
    /// </summary>
    public interface ITrack
    {
        Task<Token> Auth();
    }

    /// <summary>
    /// Tracks an access token for a specific request type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITrack<out T> : ITrack where T : OAuth
    {
        T Request { get; }
    }

    public static class Factory
    {
        /// <summary>
        /// Constructs a tracker
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <returns></returns>
        public static ITrack<T> Tracker<T>(this IOAuth2 client) where T : OAuth, new()
        {
            return client.Tracker(new T());
        }

        /// <summary>
        /// Constructs a tracker
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static ITrack<T> Tracker<T>(this IOAuth2 client, T request) where T : OAuth
        {
            return new Track<T>(client, request);
        }
    }

    class Cache
    {
        Token    _token;
        DateTime _next;

        public Token Token
        {
            get { return _token; }
            set
            {
                _token = value;
                _next = DateTime.UtcNow + TimeSpan.FromSeconds(_token.expires_in ?? 0);
            }
        }

        public bool Expired => _next <= DateTime.UtcNow;
    }

    class Track<T> : Cache, ITrack<T> where T : OAuth
    {
        readonly IOAuth2 _oauth;

        public Track(IOAuth2 oauth, T request)
        {
            _oauth  = oauth;

            Request = request;
        }

        public T Request { get; }

        public virtual async Task<Token> Auth()
        {
            return Expired ? Token = await _oauth.Auth(Request).Caf()
                           : Token;
        }
    }
}
