using System;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;

#pragma warning disable 612, 1591

namespace Bede.Thallium.Testing
{
    /// <summary>
    /// Base interface for multi-case API building
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IApis<T> : IBuilder<IApis<T>>
    {
        /// <summary>
        /// Good, or working, version of the API
        /// </summary>
        T Good { get; }

        /// <summary>
        /// Bad version of the API
        /// </summary>
        T Bad { get; }

        /// <summary>
        /// Ugly version of the API
        /// </summary>
        T Ugly { get; }

        /// <summary>
        /// Create a set of cases for an operation
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        ITaskApis<T, IApis<T>> Cases(Expression<Func<T, Task>> op);

        /// <summary>
        /// Create a set of cases for an operation
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="op"></param>
        /// <returns></returns>
        ITaskApis<T, TResult> Cases<TResult>(Expression<Func<T, Task<TResult>>> op);
    }

    /// <summary>
    /// Define the set of cases
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITaskApis<T> : IBuilder<IApis<T>>
    {
        ITaskApis<T> Good(HttpStatusCode code);
        ITaskApis<T> Bad(HttpStatusCode code);
        ITaskApis<T> Ugly(HttpStatusCode code);
    }

    /// <summary>
    /// Define the set of cases
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public interface ITaskApis<T, in TResult> : IBuilder<IApis<T>>
    {
        ITaskApis<T, TResult> Good(HttpStatusCode code, TResult result = default(TResult));
        ITaskApis<T, TResult> Bad(HttpStatusCode code, TResult result = default(TResult));
        ITaskApis<T, TResult> Ugly(HttpStatusCode code, TResult result = default(TResult));
    }

    /// <summary>
    /// Concrete multi-case API builder implementation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Apis<T> : Builder<IApis<T>>, IApis<T> where T : class
    {
        readonly IApiBuilder<T> _good, _bad, _ugly;

        public Apis()
        {
            _good = new ApiBuilder<T>();
            _bad  = new ApiBuilder<T>();
            _ugly = new ApiBuilder<T>();
        }

        public T Good { get; internal set; }
        public T Bad  { get; internal set; }
        public T Ugly { get; internal set; }

        public ITaskApis<T, IApis<T>> Cases(Expression<Func<T, Task>> op)
        {
            var good = _good.Case(op);
            var bad  = _bad .Case(op);
            var ugly = _ugly.Case(op);

            return new TaskApis<T, IApis<T>>(this, good, bad, ugly);
        }

        public ITaskApis<T, TResult> Cases<TResult>(Expression<Func<T, Task<TResult>>> op)
        {
            var good = _good.Case(op);
            var bad  = _bad .Case(op);
            var ugly = _ugly.Case(op);

            return new TaskApis<T, TResult>(this, good, bad, ugly);
        }

        public override IApis<T> Build()
        {
            Good = _good.Build();
            Bad  = _bad. Build();
            Ugly = _ugly.Build();

            return this;
        }
    }

    class Apis<T, TResult> : Builder<IApis<T>>
        where T     : class
    {
        readonly Apis<T> _back;
        readonly ICaseBuilder<T, TResult> _good, _bad, _ugly;

        internal Apis(Apis<T> back, ICaseBuilder<T, TResult> good,
                                    ICaseBuilder<T, TResult> bad,
                                    ICaseBuilder<T, TResult> ugly)
        {
            _back = back;

            _good = good;
            _bad  = bad;
            _ugly = ugly;
        }

        internal Apis<T, TResult> Good(HttpStatusCode code, TResult result = default(TResult))
        {
            _good.Define(code, result);
            return this;
        }

        internal Apis<T, TResult> Bad(HttpStatusCode code, TResult result = default(TResult))
        {
            _bad.Define(code, result);
            return this;
        }

        internal Apis<T, TResult> Ugly(HttpStatusCode code, TResult result = default(TResult))
        {
            _ugly.Define(code, result);
            return this;
        }

        public override IApis<T> Build()
        {
            return _back.Build();
        }
    }

    class TaskApis<T> : Apis<T, Task>, ITaskApis<T>
        where T     : class
    {
        internal TaskApis(Apis<T> back, ICaseBuilder<T, Task> good,
                                        ICaseBuilder<T, Task> bad,
                                        ICaseBuilder<T, Task> ugly)
            : base(back, good, bad, ugly)
        {
        }

        public ITaskApis<T> Good(HttpStatusCode code)
        {
            Good(code, Task.FromResult(true));
            return this;
        }

        public ITaskApis<T> Bad(HttpStatusCode code)
        {
            Bad(code, Task.FromResult(true));
            return this;
        }

        public ITaskApis<T> Ugly(HttpStatusCode code)
        {
            Ugly(code, Task.FromResult(true));
            return this;
        }
    }

    class TaskApis<T, TResult> : Apis<T, Task<TResult>>, ITaskApis<T, TResult>
        where T     : class
    {
        internal TaskApis(Apis<T> back, ICaseBuilder<T, Task<TResult>> good,
                                        ICaseBuilder<T, Task<TResult>> bad,
                                        ICaseBuilder<T, Task<TResult>> ugly)
            : base(back, good, bad, ugly)
        {
        }

        public ITaskApis<T, TResult> Good(HttpStatusCode code, TResult result = default(TResult))
        {
            Good(code, Task.FromResult(result));
            return this;
        }

        public ITaskApis<T, TResult> Bad(HttpStatusCode code, TResult result = default(TResult))
        {
            Bad(code, Task.FromResult(result));
            return this;
        }

        public ITaskApis<T, TResult> Ugly(HttpStatusCode code, TResult result = default(TResult))
        {
            Ugly(code, Task.FromResult(result));
            return this;
        }
    }
}