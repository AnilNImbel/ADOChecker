using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace ADOAnalyser.Extensions
{
    public static class Extensions
    {
        /// <summary>
        /// Concurrently Executes async actions for each item of <see cref="IEnumerable<typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">Type of IEnumerable</typeparam>
        /// <param name="enumerable">instance of <see cref="IEnumerable<typeparamref name="T"/>"/></param>
        /// <param name="action">an async <see cref="Action" /> to execute</param>
        /// <param name="cancellationToken">A cancellation token that should be used to cancel the work.</param>
        /// <param name="maxDegreeOfParallelism">Optional, An integer that represents the maximum degree of parallelism,
        /// It'll be considered only if it's grater than 0</param>
        /// <returns>A Task representing an async operation</returns>
        public static async Task ForEachAsyncConcurrent<T>(
            this IEnumerable<T> enumerable,
            Func<T, Task> action,
            CancellationToken cancellationToken,
            int? maxDegreeOfParallelism)
        {
            if (maxDegreeOfParallelism.HasValue && maxDegreeOfParallelism.Value > 0)
            {
                await Task.WhenAll(
                    Partitioner.Create(enumerable).GetPartitions(maxDegreeOfParallelism.Value)
                    .Select(partition => Task.Run(
                    async () =>
                    {
                        using (partition)
                        {
                            while (partition.MoveNext())
                            {
                                await action(partition.Current).ContinueWith(t =>
                                {
                                    //observe exceptions
                                });
                            }
                        }
                    }, cancellationToken)));
            }
            else
            {
                await Task.WhenAll(enumerable.Select(
                    item => Task.Run(
                    () =>
                    {
                        action(item);
                    }, cancellationToken)));
            }
        }
    }
}