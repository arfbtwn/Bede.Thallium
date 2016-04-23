using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable 1591
namespace Bede.Thallium
{
    using Belt;

    /// <summary>
    /// A media type formatter that can form encode sequences
    /// of key value pairs
    /// </summary>
    public sealed class FormUrlEncoder : MediaTypeFormatter
    {
        public FormUrlEncoder()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/x-www-form-urlencoded"));
            SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
            SupportedEncodings.Add(new UnicodeEncoding(bigEndian: false, byteOrderMark: false, throwOnInvalidBytes: true));
        }

        public override bool CanReadType(Type type)
        {
            return false;
        }

        public override bool CanWriteType(Type type)
        {
            if (typeof(IDictionary).IsAssignableFrom(type))
            {
                return true;
            }

            var i = type.GetInterfaces().FirstOrDefault(x => x.HasGenericDefinition(typeof(IEnumerable<>)));

            return null != i && i.GetGenericArguments()[0].HasGenericDefinition(typeof(KeyValuePair<,>));
        }

        public override Task WriteToStreamAsync(Type             type,
                                                object           value,
                                                Stream           writeStream,
                                                HttpContent      content,
                                                TransportContext transportContext)
        {
            var o = (IEnumerable) value;

            var first = true;

            var sb = new StringBuilder();

            foreach (dynamic kv in o)
            {
                if (!first)
                {
                    sb.Append('&');
                }

                sb.Append(Escape(kv.Key.ToString()));
                sb.Append('=');
                sb.Append(Escape((kv.Value ?? string.Empty).ToString()));

                first = false;
            }

            var buf = Encoding.ASCII.GetBytes(sb.ToString());

            return writeStream.WriteAsync(buf, 0, buf.Length);
        }

        string Escape(string input)
        {
            return Uri.EscapeDataString(input);
        }
    }
}
