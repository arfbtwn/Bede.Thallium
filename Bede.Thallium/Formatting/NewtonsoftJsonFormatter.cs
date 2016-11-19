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
        public NewtonsoftJsonFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));
            SupportedEncodings.Add(Encoding.UTF8);
        }

        public override bool CanReadType (Type type) => true;
        public override bool CanWriteType(Type type) => true;

        protected virtual object AsObject(string source, Type type) => JsonConvert.DeserializeObject(source, type);
        protected virtual string AsString(object value,  Type type) => JsonConvert.SerializeObject(value);

        public sealed override async Task<object> ReadFromStreamAsync(Type             type,
                                                                      Stream           readStream,
                                                                      HttpContent      content,
                                                                      IFormatterLogger formatterLogger)
        {
            using (var mem = new MemoryStream())
            {
                await readStream.CopyToAsync(mem).Caf();

                var bytes = new byte[mem.Length];

                mem.Position = 0;

                await mem.ReadAsync(bytes, 0, bytes.Length).Caf();

                var encoded = Encoding.UTF8.GetString(bytes);

                return AsObject(encoded, type);
            }
        }

        public sealed override Task WriteToStreamAsync(Type             type,
                                                       object           value,
                                                       Stream           writeStream,
                                                       HttpContent      content,
                                                       TransportContext transportContext)
        {
            var bytes = Encoding.UTF8.GetBytes(AsString(value, type));

            return writeStream.WriteAsync(bytes, 0, bytes.Length);
        }
    }
}