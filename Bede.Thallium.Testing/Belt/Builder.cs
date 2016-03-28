namespace Bede.Thallium.Testing
{
    /// <summary>
    /// Marks a type that creates another type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBuilder<out T>
    {
        /// <summary>
        /// Create the type
        /// </summary>
        /// <returns></returns>
        T Build();
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