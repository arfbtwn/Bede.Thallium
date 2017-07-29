namespace Bede.Thallium.Introspection
{
    /// <summary>
    /// A set of readable expressions for API parameter configuration
    /// </summary>
    public static class P
    {
        /// <summary>
        /// A URI parameter
        /// </summary>
        /// <remarks>
        /// There is no special behaviour for this parameter, it
        /// is essentially ignored
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Uri<T>()
        {
            return default(T);
        }

        /// <summary>
        /// The parameter is a header
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string Header(string name)
        {
            return default(string);
        }

        /// <summary>
        /// The parameter is a header
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T Header<T>(string name)
        {
            return default(T);
        }

        /// <summary>
        /// The parameter is body with a type and disposition
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="disposition"></param>
        /// <returns></returns>
        public static T Body<T>(string type, string disposition)
        {
            return default(T);
        }

        /// <summary>
        /// The parameter is form URL encoded
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T FormUrl<T>()
        {
            return default(T);
        }

        /// <summary>
        /// The parameter is form URL encoded
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="form"></param>
        /// <returns></returns>
        public static T FormUrl<T>(this T form)
        {
            return form;
        }

        /// <summary>
        /// The parameter is an octet-stream
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Octet<T>()
        {
            return default(T);
        }

        /// <summary>
        /// The parameter is an octet-stream
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Octet<T>(this T form)
        {
            return form;
        }

        /// <summary>
        /// The parameter is form data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        /// <returns></returns>
        public static T Form<T>(params string[] args)
        {
            return default(T);
        }
    }
}