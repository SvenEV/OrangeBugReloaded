using System.Threading;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core
{
    /// <summary>
    /// An async version of <see cref="System.Threading.ManualResetEvent"/>.
    /// </summary>
    /// <remarks>
    /// Taken from http://blogs.msdn.com/b/pfxteam/archive/2012/02/11/10266920.aspx.
    /// </remarks>
    public class AsyncManualResetEvent
    {
        private volatile TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();

        public Task WaitAsync() => _tcs.Task;

        public void Set() => _tcs.TrySetResult(true);

        public void Reset()
        {
            while (true)
            {
                var tcs = _tcs;
                if (!tcs.Task.IsCompleted || Interlocked.CompareExchange(ref _tcs, new TaskCompletionSource<bool>(), tcs) == tcs)
                    return;
            }
        }
    }
}
