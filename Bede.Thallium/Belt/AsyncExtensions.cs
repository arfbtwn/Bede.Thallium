using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Bede.Thallium.Belt
{
    static class AsyncExtensions
    {
        public static T Aw<T>(this Task<T> task)
        {
            return task.Caf().GetAwaiter().GetResult();
        }

        public static void Aw(this Task task)
        {
            task.Caf().GetAwaiter().GetResult();
        }

        public static ConfiguredTaskAwaitable Caf(this Task task)
        {
            return task.ConfigureAwait(false);
        }

        public static ConfiguredTaskAwaitable<T> Caf<T>(this Task<T> task)
        {
            return task.ConfigureAwait(false);
        }
    }
}