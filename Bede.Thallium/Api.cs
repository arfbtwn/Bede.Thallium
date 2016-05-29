using System;
using System.Reflection;

namespace Bede.Thallium
{
    using Introspection;
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
    /// An interface for a type that emits client types
    /// using the specified introspector
    /// </summary>
    public interface IApi
    {
        /// <summary>
        /// Use the specified introspector for API call description
        /// </summary>
        /// <param name="introspector"></param>
        /// <returns></returns>
        IApi Using(IIntrospect introspector);

        /// <summary>
        /// Emit a type implementing the given interface
        /// </summary>
        /// <param name="interface"></param>
        /// <returns></returns>
        Type Emit(Type @interface);
    }

    /// <summary>
    /// A generic version of <see cref="IApi" /> used to
    /// encode the type of the base client
    /// </summary>
    /// <typeparam name="TBase"></typeparam>
    public interface IApi<TBase> : IApi
    {
        /// <summary>
        /// Hide the base class <see cref="IApi.Using(IIntrospect)" />
        /// method
        /// </summary>
        /// <param name="introspector"></param>
        /// <returns></returns>
        new IApi<TBase> Using(IIntrospect introspector);
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
        public static IApi<TBase> Client<TBase>() where TBase : BaseClient
        {
            return new _Api<TBase>(Factory);
        }

        /// <summary>
        /// Get a client emitter based on <see cref="Thallium.RestClient" />
        /// </summary>
        /// <returns></returns>
        public static IApi<RestClient> RestClient()
        {
            return Client<RestClient>();
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
