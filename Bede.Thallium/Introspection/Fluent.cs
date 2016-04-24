using System;
using System.Collections.Generic;
using System.Reflection;

namespace Bede.Thallium.Introspection
{
    using Belt;

    using Map  = Dictionary <MethodInfo, Description>;
    using IMap = IDictionary<MethodInfo, Description>;

    /// <summary>
    /// A fluent introspector interface
    /// </summary>
    public interface IFluent : IMap, IIntrospect
    {
        /// <summary>
        /// Use a fallback introspector
        /// </summary>
        /// <param name="introspector"></param>
        /// <returns></returns>
        IFluent Fallback(IIntrospect introspector);

        /// <summary>
        /// Include another map
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        IFluent Include(IMap map);

        /// <summary>
        /// Return an API specific introspector
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IFluent<T> Api<T>();
    }

    /// <summary>
    /// A fluent introspector interface for an API type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IFluent<T> : IMap
    {
        /// <summary>
        /// Add a description for the given expression
        /// </summary>
        /// <param name="method"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        IFluent<T> With(MethodInfo method, Description description);
    }

    class Fluent : Map, IFluent
    {
        readonly Dictionary<Type, IMap> _maps;

        IIntrospect _default;

        public Fluent()
        {
            _maps = new Dictionary<Type, IMap>();
        }

        public IFluent Fallback(IIntrospect introspector)
        {
            _default = introspector;
            return this;
        }

        public IFluent<T> Api<T>()
        {
            return (IFluent<T>) _maps.Lookup(typeof(T), new Fluent<T>(this));
        }

        public IFluent Include(IMap map)
        {
            foreach (var i in map)
            {
                this[i.Key] = i.Value;
            }
            return this;
        }

        void Reduce()
        {
            foreach (var i in _maps.Values)
            {
                Include(i);
            }

            _maps.Clear();
        }

        public Description Call(Type parent, MethodInfo method)
        {
            Reduce();

            Description o;
            if (!TryGetValue(method, out o))
            {
                return o;
            }

            return _default.Call(parent, method);
        }
    }

    class Fluent<T> : Map, IFluent<T>
    {
        readonly IFluent _back;

        public Fluent(IFluent back)
        {
            _back = back;
        }

        public IFluent<T> With(MethodInfo method, Description description)
        {
            this[method] = description;

            return this;
        }

        public IFluent Back()
        {
            return _back;
        }
    }
}