using System;
using System.Reflection;

namespace Bede.Thallium
{
    using Introspection;
    using Clients;
    using Content;

    /// <summary>
    /// API introspection interface
    /// </summary>
    public interface IIntrospect
    {
        /// <summary>
        /// Describe a method
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        Description Call(Type parent, MethodInfo method);
    }

    /// <summary>
    /// A static access point to the runtime client factory
    /// </summary>
    public static partial class Api
    {
        static readonly Factory Factory = new Factory();

        /// <summary>
        /// Gets or sets the default introspector used when none is provided
        /// </summary>
        /// <remarks>
        /// Can be used to customise introspection of interfaces used in black boxes
        /// </remarks>
        public static IIntrospect Introspector = new Simple();

        /// <summary>
        /// Gets a fluently configurable introspector
        /// </summary>
        /// <returns></returns>
        public static IFluent Fluent()
        {
            return new Fluent();
        }

        /// <summary>
        /// Get a client emitter based on some type
        /// </summary>
        /// <typeparam name="TBase"></typeparam>
        /// <returns></returns>
        public static IApi<TBase> On<TBase>() where TBase : SkeletonClient
        {
            return new _Api<TBase>(Factory);
        }

        /// <summary>
        /// Get a client emitter based on <see cref="RestClient" />
        /// </summary>
        /// <returns></returns>
        public static IApi<RestClient> Rest()
        {
            return On<RestClient>();
        }

        /// <summary>
        /// Get a client emitter based on <see cref="DynamicClient" />
        /// </summary>
        /// <returns></returns>
        public static IApi<DynamicClient> Dynamic()
        {
            return On<DynamicClient>();
        }

        /// <summary>
        /// Gets or Sets the <see cref="IImp" />
        /// </summary>
        [Obsolete]
        public static IImp Imp
        {
            get { return Factory.Imp;  }
            set { Factory.Imp = value; }
        }
    }
}
