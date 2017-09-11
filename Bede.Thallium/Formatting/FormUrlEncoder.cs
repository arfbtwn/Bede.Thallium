using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace Bede.Thallium.Formatting
{
    using Belt;

    /// <summary>
    /// A media type formatter that can form encode sequences
    /// of key value pairs
    /// </summary>
    public class FormUrlEncoder : FormUrlEncodedMediaTypeFormatter
    {
        public static readonly Type Supported = typeof(IEnumerable<KeyValuePair<string, string>>);

        public int WriteBufferSize { get; set; } = 4096;

        public override bool CanWriteType(Type type) => Supported.IsAssignableFrom(type);

        protected virtual string Escape(string input) => Uri.EscapeUriString(input);

        public async override Task WriteToStreamAsync(Type             type,
                                                      object           value,
                                                      Stream           writeStream,
                                                      HttpContent      content,
                                                      TransportContext transportContext)
        {
            var o = (IEnumerable<KeyValuePair<string, string>>) value;

            var first = true;

            using (var writer = new StreamWriter(writeStream, Encoding.UTF8, WriteBufferSize, true))
            {
                foreach (var kv in o)
                {
                    if (!first)
                    {
                        await writer.WriteAsync('&').Caf();
                    }

                    await writer.WriteAsync(Escape(kv.Key)).Caf();
                    await writer.WriteAsync('=').Caf();
                    await writer.WriteAsync(Escape(kv.Value ?? string.Empty)).Caf();

                    first = false;
                }
            }
        }
    }
}
