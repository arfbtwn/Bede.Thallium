#pragma warning disable 1591
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;

namespace Bede.Thallium
{
    using static Constants;

    static class Constants
    {
        public const AttributeTargets CIM  = AttributeTargets.Class     |
                                             AttributeTargets.Interface |
                                             AttributeTargets.Method;

        public const AttributeTargets IPMP = AttributeTargets.Interface |
                                             AttributeTargets.Property  |
                                             AttributeTargets.Method    |
                                             AttributeTargets.Parameter;
    }

    /// <summary>
    /// Specifies a route template for a method
    /// </summary>
    [AttributeUsage(CIM)]
    public class RouteAttribute : Attribute
    {
        public string Route { get; private set; }

        public RouteAttribute(string route)
        {
            Route = route;
        }

        protected RouteAttribute() { }
    }

    /// <summary>
    /// Specifies an endpoint verb and routing template
    /// </summary>
    /// <remarks>
    /// This attribute explicitly supports subclassing
    /// for shorthand, its default constructor extracts
    /// the runtime type name and upper cases it to get
    /// the HTTP verb
    /// </remarks>
    [AttributeUsage(CIM)]
    public class VerbAttribute : RouteAttribute
    {
        public HttpMethod Verb { get; private set; }

        public VerbAttribute(string verb, string route)
            : base(route)
        {
            Verb = new HttpMethod(verb);
        }

        protected VerbAttribute(string route)
            : base(route)
        {
            Verb = new HttpMethod(GetType().Name.Replace("Attribute", string.Empty).ToUpper());
        }

        protected VerbAttribute() : this(string.Empty) { }
    }

    [AttributeUsage(CIM)]
    public sealed class PostAttribute : VerbAttribute
    {
        public PostAttribute() { }
        public PostAttribute(string route) : base(route) { }
    }

    [AttributeUsage(CIM)]
    public sealed class GetAttribute : VerbAttribute
    {
        public GetAttribute() { }
        public GetAttribute(string route) : base(route) { }
    }

    [AttributeUsage(CIM)]
    public sealed class PutAttribute : VerbAttribute
    {
        public PutAttribute() { }
        public PutAttribute(string route) : base(route) { }
    }

    [AttributeUsage(CIM)]
    public sealed class DeleteAttribute : VerbAttribute
    {
        public DeleteAttribute() { }
        public DeleteAttribute(string route) : base(route) { }
    }

    /// <summary>
    /// Marks a parameter as body
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class BodyAttribute : Attribute
    {
        protected virtual HttpContent Content(object value)
        {
            return new ObjectContent(value.GetType(), value, new JsonMediaTypeFormatter());
        }

        public virtual HttpContent Create(object value)
        {
            return null == value ? null : Content(value);
        }
    }

    /// <summary>
    /// Marks a member as a header with the given name
    /// </summary>
    [AttributeUsage(IPMP, AllowMultiple = true)]
    public class HeaderAttribute : RequestAttribute
    {
        public string Name  { get; private set; }
        public string Value { get; private set; }

        public HeaderAttribute(string name)
        {
            Name = name;
        }

        public HeaderAttribute(string name, string value)
            : this(name)
        {
            Value = value;
        }

        public override void Request(HttpRequestMessage message)
        {
            if (null == Value) return;

            message.Headers.Add(Name, Value);
        }

        public void Header(HttpHeaders headers, object value)
        {
            if (null == value) return;

            var e = value as IEnumerable<string>;

            if (null != e) headers.Add(Name, e);
            else           headers.Add(Name, value.ToString());
        }
    }

    public abstract class ContentHeaderAttribute : BodyAttribute
    {
        public virtual string Name  { get; set; }
        public virtual string Value { get; set; }
    }

    [AttributeUsage(IPMP)]
    public class TypeAttribute : ContentHeaderAttribute
    {
        protected TypeAttribute() { }

        public TypeAttribute(string type)
        {
            Value = type;
        }

        public sealed override string Name
        {
            get { return "Content-Type"; }
        }
    }

    [AttributeUsage(IPMP)]
    public class DispositionAttribute : ContentHeaderAttribute
    {
        protected DispositionAttribute() { }

        public DispositionAttribute(string disposition)
        {
            Value = disposition;
        }

        public sealed override string Name
        {
            get { return "Content-Disposition"; }
        }
    }

    [AttributeUsage(IPMP)]
    public sealed class FormDataAttribute : DispositionAttribute
    {
        public FormDataAttribute() { }

        public FormDataAttribute(string name)
        {
            Name = name;
        }

        public FormDataAttribute(string name, string fileName)
        {
            Name     = name;
            FileName = fileName;
        }

        public new string Name { get; set; }
        public string FileName { get; set; }

        public override string Value
        {
            get
            {
                return
                    new ContentDispositionHeaderValue("form-data")
                    {
                        Name         = Name,
                        FileName     = FileName,
                        FileNameStar = FileName

                    }.ToString();
            }
        }
    }

    public class RequestAttribute : Attribute
    {
        public virtual void Request(HttpRequestMessage message) { }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class MultipartAttribute : RequestAttribute
    {
        public MultipartAttribute() { }

        public MultipartAttribute(string subType, string boundary)
        {
            Subtype  = subType;
            Boundary = boundary;
        }

        public string Subtype  { get; set; }
        public string Boundary { get; set; }
    }

    public sealed class FormUrlAttribute : BodyAttribute
    {
        protected override HttpContent Content(object value)
        {
            return new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>) value);
        }
    }

    public sealed class OctetAttribute : TypeAttribute
    {
        public int? Offset { get; set; }
        public int? Length { get; set; }

        protected override HttpContent Content(object value)
        {
            var b = (byte[]) value;
            return new ByteArrayContent(b, Offset ?? 0, Length ?? b.Length);
        }
    }
}