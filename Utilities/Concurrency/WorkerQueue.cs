using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Utilities
{
    public class WorkQueue : IDisposable
    {
        private readonly Task thread;
        public readonly BlockingCollection<Action> queue;
        public WorkQueue()
        {
            queue = new BlockingCollection<Action>();
            thread = Task.Factory.StartNew(DoWork);
        }

        public Task<T> Execute<T>(Func<T> f)
        {
            if (queue.IsCompleted)
                return null;
            var source = new TaskCompletionSource<T>();
            Execute(() => source.SetResult(f()));
            return source.Task;
        }

        public void Execute(Action f)
        {
            if (queue.IsCompleted)
                return;
            queue.Add(f);
        }

        public void Dispose()
        {
            queue.CompleteAdding();
        }

        private void DoWork()
        {
            foreach (var action in queue.GetConsumingEnumerable())
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Logger.Instance.Exception(ex.Message, ex.StackTrace);
                    int RetryCount = 0;
                    try
                    {
                        Retry.Do(() =>
                        {
                            RetryCount++;
                            Logger.Instance.Info(String.Format("{0} retry {1}", action.Method.Name, RetryCount));
                            action();
                        }, TimeSpan.FromSeconds(1), 5);
                    }
                    catch (AggregateException xxx)
                    {
                        foreach (Exception innerEx in xxx.InnerExceptions)
                        {
                            Logger.Instance.Exception(String.Format("Message: {0}", innerEx.Message), ex.StackTrace);
                        }
                    }
                }
            }
        }
    }
}
