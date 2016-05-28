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
        public static P<T> Uri<T>()
        {
            return new P<T>();
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
        /// The parameter is body with a type and disposition
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="disposition"></param>
        /// <returns></returns>
        public static P<T> Body<T>(string type, string disposition)
        {
            return new P<T>();
        }

        /// <summary>
        /// The parameter is form URL encoded
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static P<T> FormUrl<T>()
        {
            return new P<T>();
        }

        /// <summary>
        /// The parameter is form URL encoded
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="form"></param>
        /// <returns></returns>
        public static P<T> FormUrl<T>(this P<T> form)
        {
            return form;
        }

        /// <summary>
        /// The parameter is an octet-stream
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static P<T> Octet<T>()
        {
            return new P<T>();
        }

        /// <summary>
        /// The parameter is an octet-stream
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static P<T> Octet<T>(this P<T> form)
        {
            return form;
        }

        /// <summary>
        /// The parameter is form data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        /// <returns></returns>
        public static P<T> Form<T>(params string[] args)
        {
            return new P<T>();
        }

        /// <summary>
        /// For explicit conversion of the generic parameter type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="p"></param>
        /// <returns></returns>
        public static T Done<T>(this P<T> p)
        {
            return p;
        }
    }

#pragma warning disable 1591

    public class P<T>
    {
        public static implicit operator T(P<T> p)
        {
            return default(T);
        }
    }
}