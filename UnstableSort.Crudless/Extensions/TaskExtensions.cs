using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace UnstableSort.Crudless
{
    internal static class TaskExtensions
    {
        internal static ConfiguredTaskAwaitable Configure(this Task task)
        {
            return task.ConfigureAwait(false);
        }

        internal static ConfiguredTaskAwaitable<TResult> Configure<TResult>(this Task<TResult> task)
        {
            return task.ConfigureAwait(false);
        }
    }
}
