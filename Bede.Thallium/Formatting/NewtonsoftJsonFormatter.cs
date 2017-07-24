using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

#pragma warning disable 1591

namespace Bede.Thallium.Formatting
{
    using Belt;

    /// <summary>
    /// A JSON formatter using Newtonsoft under the covers
    /// </summary>
    public class NewtonsoftJsonFormatter : MediaTypeFormatter
    {
        readonly JsonSerializer _serializer;

        public NewtonsoftJsonFormatter(JsonSerializer serializer)
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));
            SupportedEncodings.Add(Encoding.UTF8);

            _serializer = serializer;
        }

        public NewtonsoftJsonFormatter() : this(new JsonSerializer())
        {
        }

        public override bool CanReadType (Type type) => true;
        public override bool CanWriteType(Type type) => true;

        public int BufferSize { get; set; } = 4096;

        [Obsolete]
        protected virtual object AsObject(string source, Type type) => null;
        [Obsolete]
        protected virtual string AsString(object value,  Type type) => null;

        public sealed override Task<object> ReadFromStreamAsync(Type             type,
                                                                Stream           readStream,
                                                                HttpContent      content,
                                                                IFormatterLogger formatterLogger)
        {
            using (var sr = new StreamReader(readStream, Encoding.UTF8, false, BufferSize, true))
            {
                return Task.FromResult(_serializer.Deserialize(sr, type));
            }
        }

        public sealed override Task WriteToStreamAsync(Type             type,
                                                       object           value,
                                                       Stream           writeStream,
                                                       HttpContent      content,
                                                       TransportContext transportContext)
        {
            using (var sw = new StreamWriter(writeStream, Encoding.UTF8, BufferSize, true))
            {
                _serializer.Serialize(sw, value, type);
            }

            return Task.FromResult(true);
        }
    }
}