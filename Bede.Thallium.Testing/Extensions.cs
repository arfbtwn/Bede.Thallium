using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq.Language.Flow;

namespace Bede.Thallium.Testing
{
#pragma warning disable 1591
    public static class Extensions
#pragma warning restore 1591
    {
        /// <summary>
        /// Define a Thallium method call response
        /// </summary>
        /// <typeparam name="TMock"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="setup"></param>
        /// <param name="code"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static ISetup<TMock, TResult> Thallium<TMock, TResult>(this ISetup<TMock, TResult> setup,
                                                                           HttpStatusCode code,
                                                                           TResult result = default(TResult))
            where TMock : class
        {
            if (code.IsSuccess())
            {
                setup.Returns(result);
            }
            else
            {
                setup.Throws(new HttpRequestException { Data = { { ExceptionKeys.Code, code } } });
            }

            return setup;
        }

        /// <summary>
        /// Define a Thallium method call response
        /// </summary>
        /// <typeparam name="TMock"></typeparam>
        /// <param name="setup"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public static ISetup<TMock, Task> Thallium<TMock>(this ISetup<TMock, Task> setup, HttpStatusCode code)

            where TMock : class
        {
            return Thallium(setup, code, Task.FromResult(true));
        }

        /// <summary>
        /// Define a Thallium method call response
        /// </summary>
        /// <typeparam name="TMock"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="setup"></param>
        /// <param name="code"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static ISetup<TMock, Task<TResult>> Thallium<TMock, TResult>(this ISetup<TMock, Task<TResult>> setup,
                                                                                 HttpStatusCode code,
                                                                                 TResult result = default(TResult))
            where TMock : class
        {
            return Thallium(setup, code, Task.FromResult(result));
        }
    }
}
