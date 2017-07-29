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
    public interface IApiBuilder<T>
    {
        ICaseBuilder<T, TResult> Case<TResult>(Expression<Func<T, TResult>> op);
        ITaskBuilder<T> Case(Expression<Func<T, Task>> op);
        ITaskBuilder<T, TResult> Case<TResult>(Expression<Func<T, Task<TResult>>> op);

        T Build();
    }

    /// <summary>
    /// Concrete API builder implementation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiBuilder<T> : IApiBuilder<T> where T : class
    {
        readonly Mock<T> _mock = new Mock<T>();

        public ICaseBuilder<T, TResult> Case<TResult>(Expression<Func<T, TResult>> op)
        {
            return new CaseBuilder<T, TResult>(_mock, op);
        }

        public ITaskBuilder<T, TResult> Case<TResult>(Expression<Func<T, Task<TResult>>> op)
        {
            return new TaskBuilder<T, TResult>(_mock, op);
        }

        public ITaskBuilder<T> Case(Expression<Func<T, Task>> op)
        {
            return new TaskBuilder<T>(_mock, op);
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
    public interface ICaseBuilder<out T, in TResult>
    {
        ICaseBuilder<T, TResult> Define(HttpStatusCode code, TResult result = default(TResult));
    }

    /// <summary>
    /// Builder for a method returning a Task
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITaskBuilder<out T> : ICaseBuilder<T, Task>
    {
        ITaskBuilder<T> Define(HttpStatusCode code);
    }

    /// <summary>
    /// Builder for a method returning a Task of some type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public interface ITaskBuilder<out T, TResult> : ICaseBuilder<T, Task<TResult>>
    {
        ITaskBuilder<T, TResult> Define(HttpStatusCode code, TResult result = default(TResult));
    }

    class CaseBuilder<T, TResult> : ICaseBuilder<T, TResult> where T : class
    {
        readonly Mock<T> _mock;
        readonly Expression<Func<T, TResult>> _op;

        public CaseBuilder(Mock<T> mock, Expression<Func<T, TResult>> op)
        {
            _mock = mock;
            _op   = op;
        }

        public ICaseBuilder<T, TResult> Define(HttpStatusCode code, TResult result = default(TResult))
        {
            _mock.Setup(_op).Thallium(code, result);

            return this;
        }
    }

    class TaskBuilder<T> : CaseBuilder<T, Task>, ITaskBuilder<T> where T : class
    {
        public TaskBuilder(Mock<T> mock, Expression<Func<T, Task>> op) : base(mock, op) { }

        public ITaskBuilder<T> Define(HttpStatusCode code)
        {
            Define(code, Task.FromResult(true));

            return this;
        }
    }

    class TaskBuilder<T, TResult> : CaseBuilder<T, Task<TResult>>, ITaskBuilder<T, TResult> where T : class
    {
        public TaskBuilder(Mock<T> mock, Expression<Func<T, Task<TResult>>> op) : base(mock, op) { }

        public ITaskBuilder<T, TResult> Define(HttpStatusCode code, TResult result = default(TResult))
        {
            Define(code, Task.FromResult(result));

            return this;
        }
    }
}