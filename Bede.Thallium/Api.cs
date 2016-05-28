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
        public static IApi<TBase> On<TBase>() where TBase : BaseClient
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
        public static IImp Imp
        {
            get { return Factory.Imp;  }
            set { Factory.Imp = value; }
        }
    }
}
