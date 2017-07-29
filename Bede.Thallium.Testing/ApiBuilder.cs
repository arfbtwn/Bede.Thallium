using System;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Moq;

#pragma warning disable 612, 1591

namespace Bede.Thallium.Testing
{
    using Belt;

    /// <summary>
    /// Base interface for API building
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IApiBuilder<T> : IBuilder<T>
    {
        ICaseBuilder<T, TResult> Case<TResult>(Expression<Func<T, TResult>> op);
        ITaskBuilder<T> Case(Expression<Func<T, Task>> op);
        ITaskBuilder<T, TResult> Case<TResult>(Expression<Func<T, Task<TResult>>> op);
    }

    /// <summary>
    /// Concrete API builder implementation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiBuilder<T> : Builder<T>, IApiBuilder<T> where T : class
    {
        readonly Mock<T> _mock = new Mock<T>();

        public ICaseBuilder<T, TResult> Case<TResult>(Expression<Func<T, TResult>> op)
        {
            return new CaseBuilder<T, TResult>(this, _mock, op);
        }

        public ITaskBuilder<T, TResult> Case<TResult>(Expression<Func<T, Task<TResult>>> op)
        {
            return new TaskBuilder<T, TResult>(this, _mock, op);
        }

        public ITaskBuilder<T> Case(Expression<Func<T, Task>> op)
        {
            return new TaskBuilder<T>(this, _mock, op);
        }

        public override T Build()
        {
            return _mock.Object;
        }
    }

    /// <summary>
    /// Builder for a particular method
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public interface ICaseBuilder<out T, in TResult> : IBuilder<T>
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

    class CaseBuilder<T, TResult> : Builder<T>, ICaseBuilder<T, TResult>
        where T     : class
    {
        readonly ApiBuilder<T> _back;
        readonly Mock<T> _mock;
        readonly Expression<Func<T, TResult>> _op;

        public CaseBuilder(ApiBuilder<T> back, Mock<T> mock, Expression<Func<T, TResult>> op)
        {
            _back = back;
            _mock = mock;
            _op   = op;
        }

        public ICaseBuilder<T, TResult> Define(HttpStatusCode code, TResult result = default(TResult))
        {
            _mock.Setup(_op).Thallium(code, result);

            return this;
        }

        public override T Build()
        {
            return _back.Build();
        }
    }

    class TaskBuilder<T> : CaseBuilder<T, Task>, ITaskBuilder<T>
        where T     : class
    {
        public TaskBuilder(ApiBuilder<T> back, Mock<T> mock, Expression<Func<T, Task>> op) : base(back, mock, op) { }

        public ITaskBuilder<T> Define(HttpStatusCode code)
        {
            Define(code, Task.FromResult(true));

            return this;
        }
    }

    class TaskBuilder<T, TResult> : CaseBuilder<T, Task<TResult>>, ITaskBuilder<T, TResult>
        where T     : class
    {
        public TaskBuilder(ApiBuilder<T> back, Mock<T> mock, Expression<Func<T, Task<TResult>>> op) : base(back, mock, op) { }

        public ITaskBuilder<T, TResult> Define(HttpStatusCode code, TResult result = default(TResult))
        {
            Define(code, Task.FromResult(result));

            return this;
        }
    }
}