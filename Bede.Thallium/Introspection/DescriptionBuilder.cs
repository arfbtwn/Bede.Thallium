using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;

namespace Bede.Thallium.Introspection
{
    using Belt;

    /// <summary>
    /// Builds the description for an API call
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDescriptionBuilder<T> : IBack<IFluent<T>>
    {
        /// <summary>
        /// Declare this description is for the given method
        /// </summary>
        /// <typeparam name="Tv"></typeparam>
        /// <param name="expr"></param>
        /// <returns></returns>
        IDescriptionBuilder<T> Method<Tv>(Expression<Func<T, Tv>> expr);

        /// <summary>
        /// Sets the HTTP verb
        /// </summary>
        /// <param name="verb"></param>
        /// <returns></returns>
        IDescriptionBuilder<T> Verb(HttpMethod verb);

        /// <summary>
        /// Sets the URI template
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        IDescriptionBuilder<T> Template(string template);

        /// <summary>
        /// Sets multi-part options
        /// </summary>
        /// <param name="subType"></param>
        /// <param name="boundary"></param>
        /// <returns></returns>
        IDescriptionBuilder<T> Multi(string subType, string boundary);

        /// <summary>
        /// Sets a static header
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        IDescriptionBuilder<T> Header(string name, string value);

        /// <summary>
        /// Sets a static header with many values
        /// </summary>
        /// <param name="name"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        IDescriptionBuilder<T> Header(string name, IEnumerable<string> values);
    }

    class DescriptionBuilder<T> : IDescriptionBuilder<T>
    {
        readonly IFluent<T>  _back;

        readonly Description _desc = new Description();

        internal DescriptionBuilder(IFluent<T> back)
        {
            _back = back;
        }

        class _
        {
            readonly MethodCallExpression _e;
            readonly Description          _d;

            ParameterInfo _p;

            internal _(MethodCallExpression e, Description d)
            {
                _e = e;
                _d = d;
            }

            internal void p()
            {
                var ps = _e.Method.GetParameters();

                for (var i = 0; i < ps.Length; ++i)
                {
                    _p = ps[i];

                    __(_e.Arguments[i]);
                }
            }

            void __(Expression e)
            {
                switch(e.NodeType)
                {
                    case ExpressionType.Call:
                        __((MethodCallExpression) e);
                        break;
                    case ExpressionType.Convert:
                        __(((UnaryExpression) e).Operand);
                        break;
                    default:
                        break;
                }
            }

            void __(MethodCallExpression e)
            {
                if (null == e) return;

                if (e.Method.DeclaringType != typeof(P)) return;

                ContentDescription cd;

                switch(e.Method.Name)
                {
                    case "Header":
                        _d.Headers[_p] = (string) __(e.Arguments[0] as ConstantExpression);
                        break;
                    case "Body":
                        cd = _d.Body.Lookup(_p);

                        cd.Type        = (string) __(e.Arguments[0] as ConstantExpression);
                        cd.Disposition = (string) __(e.Arguments[1] as ConstantExpression);
                        break;
                    case "Form":
                        cd = _d.Body.Lookup(_p);

                        var fd = new ContentDispositionHeaderValue("form-data");

                        var strs = (string[]) __(e.Arguments[0] as NewArrayExpression);

                        switch(strs.Length)
                        {
                            case 3:
                                fd.FileNameStar = strs[2];
                                goto case 2;
                            case 2:
                                fd.FileName     = strs[1];
                                goto case 1;
                            case 1:
                                fd.Name         = strs[0];
                                break;
                        }

                        cd.Disposition = fd.ToString();
                        cd.SetName     = string.IsNullOrWhiteSpace(fd.Name);
                        cd.SetFileName = string.IsNullOrWhiteSpace(fd.FileName);
                        break;
                    case "FormUrl":
                        __(e.Arguments.FirstOrDefault());
                        cd = _d.Body.Lookup(_p);

                        cd.Type = "application/x-www-form-urlencoded";
                        break;
                    case "Octet":
                        __(e.Arguments.FirstOrDefault());
                        cd = _d.Body.Lookup(_p);

                        cd.Type = "application/octet-stream";
                        break;
                    case "Done":
                        __(e.Arguments.FirstOrDefault());
                        break;
                }
            }

            object __(ConstantExpression e)
            {
                return null == e ? null : e.Value;
            }

            object __(ConstantExpression e, Type t)
            {
                var o = __(e);
                return t.IsValueType && null == o ? Activator.CreateInstance(t) : o;
            }

            Array __(NewArrayExpression e)
            {
                var el = e.Type.GetElementType();

                var a = Array.CreateInstance(el, e.Expressions.Count);

                for (var i = 0; i < a.Length; ++i)
                {
                    a.SetValue(__(e.Expressions[i] as ConstantExpression, el), i);
                }

                return a;
            }
        }

        public IDescriptionBuilder<T> Method<Tv>(Expression<Func<T, Tv>> expr)
        {
            _desc.Body    = new Dictionary<ParameterInfo, ContentDescription>();
            _desc.Headers = new Dictionary<ParameterInfo, string>();

            var mce = (MethodCallExpression) expr.Body;

            new _(mce, _desc).p();

            _back.With(mce.Method, _desc);

            return this;
        }

        public IDescriptionBuilder<T> Verb(HttpMethod verb)
        {
            _desc.Verb = verb;
            return this;
        }

        public IDescriptionBuilder<T> Template(string template)
        {
            _desc.Template = (template ?? string.Empty).TrimStart('/');
            return this;
        }

        public IDescriptionBuilder<T> Multi(string subType, string boundary)
        {
            _desc.Subtype  = subType;
            _desc.Boundary = boundary;
            return this;
        }

        public IDescriptionBuilder<T> Header(string name, string value)
        {
            return Header(name, new [] { value });
        }

        public IDescriptionBuilder<T> Header(string name, IEnumerable<string> values)
        {
            _desc.Static = _desc.Static ?? new Dictionary<string, string[]>();

            _desc.Static[name] = values.ToArray();
            return this;
        }

        public IFluent<T> Back()
        {
            return _back;
        }
    }
}