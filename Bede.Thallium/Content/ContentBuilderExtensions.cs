using System.IO;
using System.Net.Http.Headers;

#pragma warning disable 1591

namespace Bede.Thallium.Content
{
    public static class ContentBuilderExtensions
    {
        public static IContentBuilder Octet(this IContentBuilder @this)
        {
            return ContentType(@this, "application/octet-stream");
        }

        public static IContentBuilder ContentType(this IContentBuilder @this, string header)
        {
            return @this.ContentType(Rfc2616.ContentType(header));
        }

        public static IContentBuilder FormData(this IContentBuilder @this, string name, string fileName = null)
        {
            return ContentDisposition(@this, "form-data", name, fileName);
        }

        public static IContentBuilder Name(this IContentBuilder @this, string name)
        {
            var last = @this.Last();

            last.Headers.ContentDisposition.Name = name;

            return @this;
        }

        public static IContentBuilder File(this IContentBuilder @this, string fileName)
        {
            var last = @this.Last();

            fileName = Path.GetFileName(fileName);

            last.Headers.ContentDisposition.FileName     = fileName;
            last.Headers.ContentDisposition.FileNameStar = fileName;

            return @this;
        }

        public static IContentBuilder File(this IContentBuilder @this, FileStream stream)
        {
            return File(@this, stream.Name);
        }

        public static IContentBuilder ContentDisposition(this IContentBuilder @this, string type, string name, string fileName = null)
        {
            var h = new ContentDispositionHeaderValue(type)
            {
                Name         = name,
                FileName     = fileName,
                FileNameStar = fileName
            };
            return @this.ContentDisposition(h);
        }
    }
}