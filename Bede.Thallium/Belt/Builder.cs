using System;

namespace Bede.Thallium.Belt
{
    /// <summary>
    /// A base type for generic builder classes, implementing
    /// the runtime builder interface and returning default(T)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Builder<T> : IBuilder<T>
    {
        readonly bool _empty;
        readonly T    _value;

        /// <summary>
        /// Construct a builder returning the default
        /// value for type T
        /// </summary>
        public Builder() { _empty = true; }

        /// <summary>
        /// Construct a builder returning a specific
        /// value for type T
        /// </summary>
        /// <param name="value"></param>
        public Builder(T value) { _value = value; }

        Type   IBuilder.Type     => typeof(T);
        object IBuilder.Build()  => Build();

        /// <summary>
        /// Builds the object
        /// </summary>
        /// <returns></returns>
        public virtual T Build() => _value;

        /// <summary>
        /// Returns the value of the thing
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _empty ? typeof(T).ToString() : _value?.ToString() ?? "null";
        }
    }

    /// <summary>
    /// Marks a type that builds an object
    /// </summary>
    public interface IBuilder
    {
        /// <summary>
        /// The type the builder will build
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Build the object
        /// </summary>
        /// <returns></returns>
        object Build();
    }

    /// <summary>
    /// Marks a type that creates another type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBuilder<out T> : IBuilder
    {
        /// <summary>
        /// Create the type
        /// </summary>
        /// <returns></returns>
        new T Build();
    }

    /// <summary>
    /// Marks a type with a link backwards
    /// </summary>
    /// <typeparam name="TBack"></typeparam>
    public interface IBack<out TBack>
    {
        /// <summary>
        /// Get the 'back' object
        /// </summary>
        /// <returns></returns>
        TBack Back();
    }
}