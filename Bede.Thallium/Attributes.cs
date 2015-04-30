#pragma warning disable 1591
using System;
using System.Net.Http;

namespace Bede.Thallium
{
    /// <summary>
    /// Specifies a route template for a method
    /// </summary>
    [AttributeUsage(AttributeTargets.Class     |
                    AttributeTargets.Interface |
                    AttributeTargets.Method)]
    public class RouteAttribute : Attribute
    {
        public string Route { get; set; }

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
    [AttributeUsage(AttributeTargets.Method)]
    public class VerbAttribute : RouteAttribute
    {
        public HttpMethod Verb { get; private set; }

        public VerbAttribute(HttpMethod verb, string route)
            : base(route)
        {
            Verb = verb;
        }

        protected VerbAttribute(string route)
            : base(route)
        {
            Verb = new HttpMethod(GetType().Name.Replace("Attribute", string.Empty).ToUpper());
        }

        protected VerbAttribute() : this(string.Empty) { }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class PostAttribute : VerbAttribute
    {
        public PostAttribute() { }
        public PostAttribute(string route) : base(route) { }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class GetAttribute : VerbAttribute
    {
        public GetAttribute() { }
        public GetAttribute(string route) : base(route) { }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class PutAttribute : VerbAttribute
    {
        public PutAttribute() { }
        public PutAttribute(string route) : base(route) { }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class DeleteAttribute : VerbAttribute
    {
        public DeleteAttribute() { }
        public DeleteAttribute(string route) : base(route) { }
    }

    /// <summary>
    /// Marks a parameter as the request body
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class BodyAttribute : Attribute { }

    /// <summary>
    /// Marks a member as a header with the given name
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface |
                    AttributeTargets.Property  |
                    AttributeTargets.Method    |
                    AttributeTargets.Parameter,
                    AllowMultiple = true)]
    public class HeaderAttribute : Attribute
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
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ContentTypeAttribute : HeaderAttribute
    {
        public ContentTypeAttribute(string spec) : base("Content-Type", spec) { }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class FormUrlAttribute : ContentTypeAttribute
    {
        public FormUrlAttribute() : base("application/x-www-form-urlencoded") { }
    }
}