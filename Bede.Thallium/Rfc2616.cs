using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

#pragma warning disable 1591

namespace Bede.Thallium
{
    using Param  = KeyValuePair<string, object>;
    using Params = Dictionary  <string, object>;

    /// <summary>
    /// An internal header sorter, aware of top level request
    /// and content headers.
    /// </summary>
    /// <remarks>
    /// Had to become public for factory changes as this is
    /// used directly by generated instructions.
    ///
    /// It may disappear from the public interface at any time.
    /// You have been warned.
    /// </remarks>
    public static class Rfc2616
    {
        static string[] RequestHeaders = {
                                             "Accept",
                                             "Accept-Charset",
                                             "Accept-Encoding",
                                             "Accept-Language",
                                             "Authorization",
                                             "Expect",
                                             "From",
                                             "Host",
                                             "If-Match",
                                             "If-Modified-Since",
                                             "If-None-Match",
                                             "If-Range",
                                             "If-Unmodified-Since",
                                             "Max-Forwards",
                                             "Proxy-Authorization",
                                             "Range",
                                             "Referer",
                                             "TE",
                                             "User-Agent"
                                         };

        static string[] EntityHeaders =  {
                                             "Allow",
                                             "Content-Encoding",
                                             "Content-Language",
                                             "Content-Length",
                                             "Content-Location",
                                             "Content-MD5",
                                             "Content-Range",
                                             "Content-Type",
                                             "Content-Disposition",
                                             "Expires",
                                             "Last-Modified"
                                         };

        public static MediaTypeHeaderValue ContentType(string header)
        {
            MediaTypeHeaderValue mt;
            return MediaTypeHeaderValue.TryParse(header, out mt) ? mt : null;
        }

        public static ContentDispositionHeaderValue Disposition(string header)
        {
            ContentDispositionHeaderValue cd;
            return ContentDispositionHeaderValue.TryParse(header, out cd) ? cd : null;
        }

        public static MediaTypeHeaderValue ContentType(Params headers)
        {
            var val  = headers.FirstOrDefault(x => x.Key == "Content-Type").Value;
            var str  = val as string;
            var vals = val as IEnumerable;

            str = str ?? (null == vals ? null : vals.Cast<object>().Last().ToString());

            return ContentType(str);
        }

        public static void Populate(HttpRequestMessage msg, Params headers)
        {
            var rh  = headers.Where(x => RequestHeaders.Contains(x.Key)).ToArray();
            var ch  = headers.Where(x => EntityHeaders.Contains(x.Key)).ToArray();
            var rem = headers.Except(rh).Except(ch).ToArray();

            Set(msg.Headers, rh);

            if (null != msg.Content)
            {
                Set(msg.Content.Headers, ch);
            }

            Set(msg.Headers, rem);
        }

        static void Set(HttpHeaders collection, Param[] headers)
        {
            var set    = headers.Where(x => null != x.Value).ToArray();
            var multi  = set.Where(x => x.Value is ICollection).ToArray();
            var single = set.Except(multi).ToArray();

            foreach (var kv in multi)
            {
                var objs = ((ICollection) kv.Value).Cast<object>()
                                                   .Where(x => null != x)
                                                   .Select(x => x.ToString());

                collection.Remove(kv.Key);
                collection.Add(kv.Key, objs);
            }

            foreach (var kv in single)
            {
                collection.Remove(kv.Key);
                collection.Add(kv.Key, kv.Value.ToString());
            }
        }
    }
}
