using System;
using System.Linq.Expressions;
using System.Net.Http;

#pragma warning disable 1591

namespace Bede.Thallium.Introspection
{
    public static class FluentExtensions
    {
        public static IFluent Fallback<T>(this IFluent @this) where T : IIntrospect, new()
        {
            return @this.Fallback(new T());
        }

        public static IDescriptionBuilder<T> Call<T, TV>(this IFluent<T> @this, Expression<Func<T, TV>> expr)
        {
            return new DescriptionBuilder<T>(@this).Method(expr);
        }

        public static IDescriptionBuilder<T> Get<T>(this IFluent<T> @this, string template)
        {
            return new DescriptionBuilder<T>(@this).Get(template);
        }

        public static IDescriptionBuilder<T> Post<T>(this IFluent<T> @this, string template)
        {
            return new DescriptionBuilder<T>(@this).Post(template);
        }

        public static IDescriptionBuilder<T> Put<T>(this IFluent<T> @this, string template)
        {
            return new DescriptionBuilder<T>(@this).Put(template);
        }

        public static IDescriptionBuilder<T> Delete<T>(this IFluent<T> @this, string template)
        {
            return new DescriptionBuilder<T>(@this).Delete(template);
        }

        public static IDescriptionBuilder<T> Get<T>(this IDescriptionBuilder<T> @this, string template)
        {
            return @this.Verb(HttpMethod.Get).Template(template);
        }

        public static IDescriptionBuilder<T> Post<T>(this IDescriptionBuilder<T> @this, string template)
        {
            return @this.Verb(HttpMethod.Post).Template(template);
        }

        public static IDescriptionBuilder<T> Put<T>(this IDescriptionBuilder<T> @this, string template)
        {
            return @this.Verb(HttpMethod.Put).Template(template);
        }

        public static IDescriptionBuilder<T> Delete<T>(this IDescriptionBuilder<T> @this, string template)
        {
            return @this.Verb(HttpMethod.Delete).Template(template);
        }
    }
}
