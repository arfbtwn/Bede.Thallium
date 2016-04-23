using System;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bede.Thallium
{
    static class Assertion
    {
        internal static void IsInterface(string parameterName, Type type)
        {
            if (!type.IsInterface) throw new ArgumentException(type.Name + " is not an interface", parameterName);
        }

        internal static void AllAsyncMethods(string parameterName, MethodInfo[] methods)
        {
            if (!methods.All(x => typeof(Task).IsAssignableFrom(x.ReturnType)))
            {
                throw new ArgumentException("All methods must return Tasks", parameterName);
            }
        }

        internal static void HasVerbAttribute(VerbAttribute verb, MethodInfo i)
        {
            if (null == verb) throw new InvalidOperationException("No verb attribute: " + i.Name);
        }

        internal static void HasFormatter(MediaTypeFormatter formatter, Type type, MediaTypeHeaderValue header)
        {
            if (null != formatter) return;

            var str = new StringBuilder("No media type formatter for ").Append(type.Name);

            if (null != header) str.Append(" as ").Append(header.MediaType);

            throw new InvalidOperationException(str.ToString());
        }
    }
}
