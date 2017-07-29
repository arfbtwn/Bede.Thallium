using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;

#pragma warning disable 1591

namespace Bede.Thallium.Content
{
    using Belt;

    /// <summary>
    /// An interface defining quick methods of constructing HTTP content
    /// and doing it right
    /// </summary>
    public interface IContentBuilder
    {
        /// <summary>
        /// Get the last content
        /// </summary>
        /// <returns></returns>
        HttpContent Last();

        /// <summary>
        /// Adds the specified header value
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        IContentBuilder Header(string name, string value);

        /// <summary>
        /// Adds the specified header values
        /// </summary>
        /// <param name="name"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        IContentBuilder Header(string name, IEnumerable<string> values);

        /// <summary>
        /// Sets the content type header for the last added content
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        IContentBuilder ContentType(MediaTypeHeaderValue header);

        /// <summary>
        /// Sets the content disposition header for the last added content
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        IContentBuilder ContentDisposition(ContentDispositionHeaderValue header);

        /// <summary>
        /// Sets the multi-part sub-type and boundary
        /// </summary>
        /// <param name="sub"></param>
        /// <param name="bound"></param>
        /// <returns></returns>
        IContentBuilder Multi(string sub, string bound);

        /// <summary>
        /// Add arbitrary HTTP content to the builder
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        IContentBuilder With(HttpContent content);

        IContentBuilder String(string value);
        IContentBuilder Bytes(byte[] bytes);
        IContentBuilder Stream(Stream stream);
        IContentBuilder FormUrl(IEnumerable<KeyValuePair<string, string>> form);
        IContentBuilder Struct<T>(T? obj) where T : struct;
        IContentBuilder Object<T>(T obj) where T : class;

        /// <summary>
        /// Reduces the content in the builder to a single element
        /// </summary>
        /// <returns></returns>
        IContentBuilder Reduce();

        /// <summary>
        /// Returns a completed <see cref="HttpContent"/>
        /// </summary>
        /// <returns></returns>
        HttpContent Build();
    }
}