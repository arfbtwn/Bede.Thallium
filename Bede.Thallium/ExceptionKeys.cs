namespace Bede.Thallium
{
#pragma warning disable 1591
    public static class ExceptionKeys
#pragma warning restore 1591
    {
        /// <summary>
        /// The HTTP verb as a <see cref="System.String"/>
        /// </summary>
        public const string Verb = "verb";

        /// <summary>
        /// The HTTP version as a <see cref="System.Version"/>
        /// </summary>
        public const string Version = "version";

        /// <summary>
        /// The failing status code as an <see cref="System.Net.HttpStatusCode"/>
        /// </summary>
        public const string Code = "code";

        /// <summary>
        /// The string content
        /// </summary>
        public const string Content = "content";

        /// <summary>
        /// The request URI as a <see cref="System.Uri"/>
        /// </summary>
        public const string RequestUri = "uri";
    }
}