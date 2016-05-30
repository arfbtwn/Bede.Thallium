using System;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bede.Thallium
{
    using Clients;

    static class Assertion
    {
        internal static void IsNotNull(string parameterName, object obj)
        {
            if (null == obj) throw new ArgumentNullException(parameterName);
        }

        internal static void IsClass(string parameterName, Type type)
        {
            if (!type.IsClass) throw new ArgumentException(type.Name + " is not a class", parameterName);
        }

        internal static void IsNotAbstract(string parameterName, Type type)
        {
            if (type.IsAbstract) throw new ArgumentException(type.Name + " is abstract", parameterName);
        }

        internal static void ExtendsBaseClient(string parameterName, Type type)
        {
            if (!typeof(BaseClient).IsAssignableFrom(type))
            {
                throw new ArgumentException(type.Name + " doesn't have a " + nameof(BaseClient) + " ancestor", parameterName);
            }
        }

        internal static void IsNotSealed(string parameterName, Type type)
        {
            if (type.IsSealed) throw new ArgumentException(type.Name + " is sealed", parameterName);
        }

        internal static void IsAccessible(string parameterName, Type type)
        {
            if (!type.IsVisible) throw new ArgumentException(type.Name + " is not visible", parameterName);
        }

        internal static void IsInterface(string parameterName, Type type)
        {
            if (!type.IsInterface) throw new ArgumentException(type.Name + " is not an interface", parameterName);
        }

        internal static void IsAsync(MethodInfo method)
        {
            if (!typeof(Task).IsAssignableFrom(method.ReturnType))
            {
                throw new InvalidOperationException(method.Name + " does not return a Task");
            }
        }

        internal static void HasFormatter(MediaTypeFormatter formatter, Type type, MediaTypeHeaderValue header)
        {
            if (null != formatter) return;

            var str = new StringBuilder("No media type formatter for ").Append(type.Name);

            if (null != header) str.Append(" as ").Append(header.MediaType);

            throw new InvalidOperationException(str.ToString());
        }

        internal static void HasValidDescription(MethodInfo method, Description call)
        {
            if (null == call)
            {
                var name = method.Name;
                var type = method.DeclaringType.Name;

                throw new InvalidOperationException(name + " on " + type + " has no call information");
            }

            if (null == call.Verb) throw new InvalidOperationException("No HTTP verb: " + method.Name);
        }
    }
}
