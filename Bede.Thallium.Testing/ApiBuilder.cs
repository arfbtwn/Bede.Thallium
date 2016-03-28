using System;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Moq;

#pragma warning disable 1591

namespace Bede.Thallium.Testing
{
    /// <summary>
    /// Base interface for API building
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IApiBuilder<T> : IBuilder<T>
    {
        ICaseBuilder<T, TResult, IApiBuilder<T>> Case<TResult>(Expression<Func<T, TResult>> op);
        ITaskBuilder<T, IApiBuilder<T>> Case(Expression<Func<T, Task>> op);
        ITaskBuilder<T, TResult, IApiBuilder<T>> Case<TResult>(Expression<Func<T, Task<TResult>>> op);
    }

    /// <summary>
    /// Concrete API builder implementation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiBuilder<T> : IApiBuilder<T> where T : class
    {
        readonly Mock<T> _mock = new Mock<T>();

        public ICaseBuilder<T, TResult, IApiBuilder<T>> Case<TResult>(Expression<Func<T, TResult>> op)
        {
            return new CaseBuilder<T, TResult, ApiBuilder<T>>(this, _mock, op);
        }

        public ITaskBuilder<T, TResult, IApiBuilder<T>> Case<TResult>(Expression<Func<T, Task<TResult>>> op)
        {
            return new TaskBuilder<T, TResult, IApiBuilder<T>>(this, _mock, op);
        }

        public ITaskBuilder<T, IApiBuilder<T>> Case(Expression<Func<T, Task>> op)
        {
            return new TaskBuilder<T, IApiBuilder<T>>(this, _mock, op);
        }

        public T Build()
        {
            return _mock.Object;
        }
    }

    /// <summary>
    /// Builder for a particular method
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TBack"></typeparam>
    public interface ICaseBuilder<out T, in TResult, out TBack> : IBuilder<T>, IBack<TBack>
    {
        ICaseBuilder<T, TResult, TBack> Define(HttpStatusCode code, TResult result = default(TResult));
    }

    /// <summary>
    /// Builder for a method returning a Task
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TBack"></typeparam>
    public interface ITaskBuilder<out T, out TBack> : ICaseBuilder<T, Task, TBack>
    {
        ITaskBuilder<T, TBack> Define(HttpStatusCode code);
    }

    /// <summary>
    /// Builder for a method returning a Task of some type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TBack"></typeparam>
    public interface ITaskBuilder<out T, TResult, out TBack> : ICaseBuilder<T, Task<TResult>, TBack>
    {
        ITaskBuilder<T, TResult, TBack> Define(HttpStatusCode code, TResult result = default(TResult));
    }

    class CaseBuilder<T, TResult, TBack> : ICaseBuilder<T, TResult, TBack>
        where T     : class
        where TBack : IBuilder<T>
    {
        readonly TBack   _back;
        readonly Mock<T> _mock;
        readonly Expression<Func<T, TResult>> _op;

        public CaseBuilder(TBack back, Mock<T> mock, Expression<Func<T, TResult>> op)
        {
            _back = back;
            _mock = mock;
            _op   = op;
        }

        public ICaseBuilder<T, TResult, TBack> Define(HttpStatusCode code, TResult result = default(TResult))
        {
            _mock.Setup(_op).Thallium(code, result);

            return this;
        }

        public T Build()
        {
            return _back.Build();
        }

        public TBack Back()
        {
            return _back;
        }
    }

    class TaskBuilder<T, TBack> : CaseBuilder<T, Task, TBack>, ITaskBuilder<T, TBack>
        where T     : class
        where TBack : IBuilder<T>
    {
        public TaskBuilder(TBack back, Mock<T> mock, Expression<Func<T, Task>> op) : base(back, mock, op) { }

        public ITaskBuilder<T, TBack> Define(HttpStatusCode code)
        {
            Define(code, Task.FromResult(true));

            return this;
        }
    }

    class TaskBuilder<T, TResult, TBack> : CaseBuilder<T, Task<TResult>, TBack>, ITaskBuilder<T, TResult, TBack>
        where T     : class
        where TBack : IBuilder<T>
    {
        public TaskBuilder(TBack back, Mock<T> mock, Expression<Func<T, Task<TResult>>> op) : base(back, mock, op) { }

        public ITaskBuilder<T, TResult, TBack> Define(HttpStatusCode code, TResult result = default(TResult))
        {
            Define(code, Task.FromResult(result));

            return this;
        }
    }
}