using System.Net;

namespace Bede.Thallium
{
    /// <summary>
    /// Response container
    /// </summary>
    public class Response<TSuccess, TFail>
    {
        internal Response(HttpStatusCode code, TSuccess success)
        {
            Code = code;
            Success = success;
            IsSuccess = true;
        }

        internal Response(HttpStatusCode code, TFail fail)
        {
            Code = code;
            Fail = fail;
            IsSuccess = false;
        }

        /// <summary>
        /// The response body if successful
        /// </summary>
        public TSuccess Success { get; }

        /// <summary>
        /// The response body if failure
        /// </summary>
        public TFail Fail { get; }

        /// <summary>
        /// True if success
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Pure HTTP status code
        /// </summary>
        public HttpStatusCode Code { get; }
    }
}
