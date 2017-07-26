using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

#pragma warning disable 1591

namespace Bede.Thallium.Soap
{
    using static Constants;

    /// <summary>
    /// Some constants used in SOAP API consumption
    /// </summary>
    public static class Constants
    {
        public const string Namespace = "http://schemas.xmlsoap.org/soap/envelope/";
        public const string MediaType = "application/soap+xml";
    }

    /// <summary>
    /// Marks a body parameter with SOAP encoding
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class SoapAttribute : TypeAttribute
    {
        public SoapAttribute() : base(MediaType) { }
    }

    /// <summary>
    /// A SOAP envelope for a given type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [XmlRoot("Envelope", Namespace = Namespace)]
    public class Envelope<T> where T : new()
    {
        public T Body { get; set; } = new T();
    }

    /// <summary>
    /// The default SOAP media type formatter
    /// </summary>
    /// <remarks>
    /// Uses <see cref="XmlSerializer"/>
    /// </remarks>
    public class MediaFormatter : MediaTypeFormatter
    {
        public List<Type>              Types      { get; set; } = new List<Type>();
        public XmlSerializerNamespaces Namespaces { get; set; } = new XmlSerializerNamespaces();
        public XmlWriterSettings       Settings   { get; set; }

        public MediaFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue(MediaType));
            SupportedEncodings.Add(Encoding.UTF8);

            Settings = new XmlWriterSettings
            {
                Indent             = true,
                OmitXmlDeclaration = true,
                NamespaceHandling  = NamespaceHandling.OmitDuplicates
            };
        }

        public override bool CanReadType (Type type) => true;
        public override bool CanWriteType(Type type) => true;

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            var s = new XmlSerializer(type, Types.ToArray());

            using (var writer = XmlWriter.Create(writeStream, Settings))
            {
                s.Serialize(writer, value, Namespaces);
            }

            return Task.FromResult(true);
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            var s = new XmlSerializer(type, Types.ToArray());

            using (var reader = XmlReader.Create(readStream))
            {
                return Task.FromResult(s.Deserialize(reader));
            }
        }
    }
}