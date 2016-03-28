using System.Net;
using System.Net.Http;

namespace Bede.Thallium
{
#pragma warning disable 1591
    public static class Extensions
#pragma warning restore 1591
    {
        const string _code    = ExceptionKeys.Code;
        const string _content = ExceptionKeys.Content;

        /// <summary>
        /// Retrieves the status code from a request exception
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static HttpStatusCode Code(this HttpRequestException ex)
        {
            return ex.Data.Contains(_code) ? (HttpStatusCode) ex.Data[_code] : 0;
        }

        /// <summary>
        /// Checks if the status code from a request exception
        /// was a bad request
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static bool IsBadRequest(this HttpRequestException ex)
        {
            return HttpStatusCode.BadRequest == Code(ex);
        }

        /// <summary>
        /// Gets the content from a request exception
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static string Content(this HttpRequestException ex)
        {
            return ex.Data.Contains(_content) ? (string) ex.Data[_content] : null;
        }


        /// <summary>
        /// Check if a status code represents a successful outcome
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static bool IsSuccess(this HttpStatusCode code)
        {
            return 200 <= (int) code && (int) code < 300;
        }
    }
}
